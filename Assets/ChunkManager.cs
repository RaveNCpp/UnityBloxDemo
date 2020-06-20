using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Xml.XPath;
using UnityEngine;

/// <summary>
/// Main class which handles chunk loading and unloading
/// </summary>
public class ChunkManager : MonoBehaviour
{
    private Dictionary<Vector3Int, Chunk> chunks = new Dictionary<Vector3Int, Chunk>();

    public GameObject meshInstance;
    public GameObject anchor;

    public int seed = 1337;
    public ChunkGenerator generator;
    private Vector3Int lastAnchorPosition = Vector3Int.zero;

    public int radius = 6;

    public Chunk Get(in Vector3Int chunkId)
    {
        Chunk c = null;
        chunks.TryGetValue(chunkId, out c);
        return c;
    }
    
    private Chunk Generate(in Vector3Int chunkId)
    {
        if(generator == null)
        {
            return null;
        }

        Chunk chunk = generator.Generate(chunkId);

        // Put chunk in the dictionary and mesh into the chunk
        chunks[chunkId] = chunk;
        return chunk;
    }

    private Tuple<Chunk, bool> GetOrGenerate(in Vector3Int chunkId)
    {
        var c = Get(chunkId);
        if(c != null)
        {
            return Tuple.Create(c, false);
        }
        return Tuple.Create(Generate(chunkId), true);
    }

    public bool ChunkIsSurroundedByLoadedChunks(in Vector3Int chunkId)
    {
        return Get(chunkId - new Vector3Int(-1, 0, 0)) != null &&
            Get(chunkId - new Vector3Int(1, 0, 0)) != null &&
            Get(chunkId - new Vector3Int(0, -1, 0)) != null &&
            Get(chunkId - new Vector3Int(0, 1, 0)) != null &&
            Get(chunkId - new Vector3Int(0, 0, -1)) != null &&
            Get(chunkId - new Vector3Int(0, 0, 1)) != null;
    }

    public Vector3 PickSpawnSpot()
    {
        return generator.PickSpawnSpot();
    }

    private bool BuildMesh(Chunk chunk)
    {
        if(chunk == null)
        {
            return false;
        }

        // First check if the chunk is surrounded by generated chunks
        if(!ChunkIsSurroundedByLoadedChunks(chunk.coord))
        {
            return false;
        }

        Vector3Int chunkId = chunk.coord;
        Mesh mesh = ChunkMeshGenerator.GenerateFrom(this, chunk);

        var chunkMeshInstance = chunk.chunkMesh == null ? Instantiate(meshInstance) : chunk.chunkMesh;
        var meshFilter = chunkMeshInstance.GetComponent<MeshFilter>();
        var meshRenderer = chunkMeshInstance.GetComponent<MeshRenderer>();
        var collider = chunkMeshInstance.GetComponent<MeshCollider>();

        // Give this instance a nice name
        if(chunk.chunkMesh == null)
        {
            chunkMeshInstance.name += chunkId.x + "_" + chunkId.y + "_" + chunkId.z;
        }

        // Use mesh for rendering and collision
        chunkMeshInstance.transform.position = new Vector3(chunkId.x, chunkId.y, chunkId.z) * Chunk.size;
        meshFilter.mesh = mesh;
        collider.sharedMesh = mesh;

        chunk.chunkMesh = chunkMeshInstance;
        return true;
    }

    public uint GetBlock(in BlockCoord coord)
    {
        var chunk = Get(coord.ChunkCoord);
        if (chunk != null)
        {
            var r = coord.LocalCoord;
            return chunk.GetBlock(
                    Mathf.FloorToInt(Mathf.Repeat(r.x, Chunk.size)),
                    Mathf.FloorToInt(Mathf.Repeat(r.y, Chunk.size)),
                    Mathf.FloorToInt(Mathf.Repeat(r.z, Chunk.size))
                );
        }
        return 0;
    }
    public bool SetBlock(in BlockCoord coord, uint id)
    {
        var chunkCoord = coord.ChunkCoord;
        var chunk = Get(coord.ChunkCoord);
        if (chunk != null)
        {
            var r = coord.LocalCoord;
            chunk.SetBlock(r.x, r.y, r.z, (byte)id);

            // Update this mesh
            BuildMesh(chunk);

            // Update neighbours
            if (r.x == 0)
            {
                BuildMesh(Get(chunkCoord + new Vector3Int(-1, 0, 0)));
            }
            else if (r.x == Chunk.size - 1)
            {
                BuildMesh(Get((chunkCoord + new Vector3Int(1, 0, 0))));
            }
            if (r.y == 0)
            {
                BuildMesh(Get(chunkCoord + new Vector3Int(0, -1, 0)));
            }
            else if (r.y == Chunk.size - 1)
            {
                BuildMesh(Get(chunkCoord + new Vector3Int(0, 1, 0)));
            }
            if (r.z == 0)
            {
                BuildMesh(Get(chunkCoord + new Vector3Int(0, 0, -1) ));
            }
            else if (r.z == Chunk.size - 1)
            {
                BuildMesh(Get(chunkCoord + new Vector3Int(0, 0, 1)));
            }

            return true;
        }
        return false;
    }

    public void Refresh()
    {
        var pos = lastAnchorPosition = GetAnchorPosition();
        List<Vector3Int> invalidChunks = new List<Vector3Int>();
        foreach(var c in chunks)
        {
            var diff = pos - c.Key;

            if(Math.Max(Math.Max(Math.Abs(diff.x), Math.Abs(diff.y)), Math.Abs(diff.z)) > radius)
            {
                invalidChunks.Add(c.Key);
            }
        }

        foreach(var c in invalidChunks)
        {
            Unload(c);
        }
    }

    public void Unload(in Vector3Int chunkId)
    {
        var c = Get(chunkId);
        if (c != null)
        {
            Destroy(c.chunkMesh);
            chunks.Remove(chunkId);
        }
    }

    void Start()
    {
        // Init block definitions
        Block.InitBlocks();

        // Create map generator
        generator = new ChunkGenerator(seed);

        // Set up materials used by the mesh (ignore block 0 air)
        var meshRenderer = meshInstance.GetComponent<MeshRenderer>();
        var materials = new List<Material>();

        for (int i = 1; i < (int)Block.Type.Count; ++i)
        {
            var block = Block.Get((uint)i);
            var mat = block.material;

            if(mat == null)
            {
                Debug.LogError("Null material for block " + block.name);
            }
            materials.Add(mat);
        }

        meshRenderer.materials = materials.ToArray();

        var r = Resources.Load("Materials/Snow");
        if(r == null)
        {
            Debug.LogError("Failed to load");
        }
    }

    private bool TryLoadChunk(in Vector3Int chunkId)
    {
        var v = GetOrGenerate(chunkId);
        if (v.Item2)
        {
            return true;
        }
        else if (v.Item1.chunkMesh == null)
        {
            return BuildMesh(v.Item1);
        }
        return false;
    }

    void LoadChunkAround(in Vector3Int origin, int radius, uint limit)
    {
        if (TryLoadChunk(origin))
        {
            if (--limit == 0)
            {
                return;
            }
        }

        // The radius
        for (int i = 1; i <= radius; ++i)
        {
            // Each point in a cube
            for (int j = -i; j <= i; ++j)
            {
                for (int k = -i; k <= i; ++k)
                {
                    for(int l = -i; l <= i; ++l)
                    {
                        if (TryLoadChunk(origin + new Vector3Int(j, k, l)))
                        {
                            if (--limit == 0)
                            {
                                return;
                            }
                        }
                    }
                }
            }
        }
    }

    Vector3Int GetAnchorPosition()
    {
        return new BlockCoord(Vector3Int.FloorToInt(anchor.transform.position)).ChunkCoord;
    }

    private static int IndexOfMin(float a, float b)
    {
        if(a < b)
        {
            return 0;
        }
        return 1;
    }

    private static int IndexOfMin(float a, float b, float c)
    {
        if(IndexOfMin(a, c) > 0)
        {
            return IndexOfMin(b, c) + 1;
        }
        return IndexOfMin(a, b);
    }

    /// <summary>
    /// Min of a and b larger than 0 (assuming either a or b are non zero) 
    /// </summary>
    private static float SmallestNonZero(float a, float b)
    {
        if(a == 0)
        {
            return b;
        }
        else if(b == 0)
        {
            return a;
        }
        return Math.Min(a, b);
    }
    private static float SmallestNonZero(float a, float b, float c)
    {
        return SmallestNonZero(a, SmallestNonZero(b, c));
    }

    public bool Raycast(in Vector3 origin, in Vector3 direction, float distance, out BlockRaycastResult result)
    {
        // TODO fix
        /*var dir = direction.normalized;
        if (dir.x == 0) dir.x = float.Epsilon;
        if (dir.y == 0) dir.y = float.Epsilon;
        if (dir.z == 0) dir.z = float.Epsilon;

        int prevAxis = -1;

        var offsetAxisX = dir.x >= 0 ? 1 : -1;
        var offsetAxisY = dir.y >= 0 ? 1 : -1;
        var offsetAxisZ = dir.z >= 0 ? 1 : -1;

        // While the max distance wasn't reached
        Vector3 current = origin;
        Vector3Int currentBlock = Vector3Int.FloorToInt(origin);
        Vector3Int prevBlock = currentBlock;

        int iterations = 0;
        result = new BlockRaycastResult();
        while (++iterations < 2048 && (origin - current).magnitude < distance)
        {
            // Check current block
            var b = GetBlock(new BlockCoord(currentBlock));
            if (b > 0)
            {
                result.hit = current;
                result.hitBlock = new BlockCoord(currentBlock);
                if (prevBlock.x < currentBlock.x)
                {
                    result.face = Block.Face.NX;
                }
                else if (prevBlock.x > currentBlock.x)
                {
                    result.face = Block.Face.X;
                }
                else if (prevBlock.y < currentBlock.y)
                {
                    result.face = Block.Face.NY;
                }
                else if (prevBlock.y > currentBlock.y)
                {
                    result.face = Block.Face.Y;
                }
                else if (prevBlock.z < currentBlock.z)
                {
                    result.face = Block.Face.NZ;
                }
                else if (prevBlock.z > currentBlock.z)
                {
                    result.face = Block.Face.Z;
                }
                result.block = Block.Get(GetBlock(result.hitBlock));
                return true;
            }
            else if (b < 0)
            {
                break;
            }

            // Calculate the nearest edge
            var ey = (dir.y >= 0 ? 0 : 1.0f) + currentBlock.y;
            var ez = (dir.z >= 0 ? 0 : 1.0f) + currentBlock.z;
            var ex = (dir.x >= 0 ? 0 : 1.0f) + currentBlock.x;

            // Calculate the factor required for the direction
            var fx = Mathf.Abs((ex - current.x) / dir.x);
            var fy = Mathf.Abs((ey - current.y) / dir.y);
            var fz = Mathf.Abs((ez - current.z) / dir.z);

            // Pick the shortest intersecting axis and factor
            var shortestAxis = IndexOfMin(fx, fy, fz);
            var travelFactor = Mathf.Min(fx, fy, fz);

            // Avoid errors by making sure it's either different axis or we're actually moving a bit
            if (shortestAxis != prevAxis || (travelFactor > 0.0000000001))
            {
                //Uh yeah, okay
                prevBlock = currentBlock;
                currentBlock += new Vector3Int(shortestAxis == 0 ? offsetAxisX : 0, shortestAxis == 1 ? offsetAxisY : 0, shortestAxis == 2 ? offsetAxisZ : 0);
                prevAxis = shortestAxis;
            }
            else
            {
                travelFactor = SmallestNonZero(fx, fy, fz);
            }

            Debug.DrawLine(prevBlock, currentBlock, new Color(1.0f, 0.0f, 0.0f), 1.0f);
            Debug.DrawLine(current, current + dir * travelFactor, new Color(0.0f, ((float)iterations) / 32.0f, 1.0f), 1.0f);
            current += dir * travelFactor;
        }
        return false;*/

        // Bruteforce dummy raycast
        const float stepSize = 0.1f;
        Vector3 pos = origin;
        Vector3 dir = direction.normalized * stepSize;

        Vector3 prev;

        result = new BlockRaycastResult();
        result.start = origin;
        for (float f = 0; f < distance; f += stepSize)
        {
            prev = pos;
            pos += dir;

            if(GetBlock(new BlockCoord(Vector3Int.FloorToInt(pos))) != (uint)Block.Type.Air)
            {
                var prevBlock = Vector3Int.FloorToInt(prev);
                var posBlock = Vector3Int.FloorToInt(pos);
                if (prevBlock.x < posBlock.x)
                {
                    result.face = Block.Face.NX;
                }
                else if (prevBlock.x > posBlock.x)
                {
                    result.face = Block.Face.X;
                }
                else if (prevBlock.y < posBlock.y)
                {
                    result.face = Block.Face.NY;
                }
                else if (prevBlock.y > posBlock.y)
                {
                    result.face = Block.Face.Y;
                }
                else if (prevBlock.z < posBlock.z)
                {
                    result.face = Block.Face.NZ;
                }
                else if (prevBlock.z > posBlock.z)
                {
                    result.face = Block.Face.Z;
                }
                result.hit = pos;
                result.hitBlock = new BlockCoord(Vector3Int.FloorToInt(pos));
                result.block = Block.Get(GetBlock(result.hitBlock));
                return true;
            }
        }
        return false;
    }

    void UnloadDistantChunks(in Vector3Int oldOrigin, in Vector3Int newOrigin, int radius)
    {
        for (int i = -radius; i <= radius; ++i)
        {
            for (int j = -radius; j <= radius; ++j)
            {
                for (int k = -radius; k <= radius; ++k)
                {
                    if(
                        Math.Abs(newOrigin.x - (i + oldOrigin.x)) > radius ||
                        Math.Abs(newOrigin.y - (j + oldOrigin.y)) > radius ||
                        Math.Abs(newOrigin.z - (k + oldOrigin.z)) > radius
                        )
                    {
                        Unload(new Vector3Int(i, j, k) + oldOrigin);
                    }
                }
            }
        }
    }

    void Update()
    {
        // Anchor
        var anchorPosition = GetAnchorPosition();

        LoadChunkAround(anchorPosition, radius, 1);
        
        if(anchorPosition != lastAnchorPosition)
        {
            // Keep chunks loaded even if they are further than the radius to reduce amount of chunk generation when moving around
            UnloadDistantChunks(lastAnchorPosition, anchorPosition, radius + 1);
        }

        lastAnchorPosition = anchorPosition;
    }
}
