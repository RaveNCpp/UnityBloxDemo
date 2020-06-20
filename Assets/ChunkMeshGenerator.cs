using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChunkMeshGenerator
{
    private const int meshMaterialCount = 4;

    public static Mesh GenerateFrom(ChunkManager manager, Chunk chunk)
    {
        return new ChunkMeshGenerator(manager, chunk).Generate();
    }

    public ChunkMeshGenerator(ChunkManager manager, Chunk chunk)
    {
        this.chunk = chunk;
        this.manager = manager;

        for(int i = 0; i < meshMaterialCount; ++i)
        {
            indices.Add(new List<int>());
        }
    }

    private List<Vector3> vertices = new List<Vector3>();
    private List<List<int>> indices = new List<List<int>>();
    private List<Vector2> uv = new List<Vector2>();
    private Chunk chunk;
    private ChunkManager manager;

    private enum FaceMask : int {
        X = 0x1,
        Y = 0x2,
        Z = 0x4,
        NX = 0x8,
        NY = 0x10,
        NZ = 0x20,
        ALL = 0x3F,
    }

    private void pushCube(List<int> indices, int x, int y, int z, int faceMask)
    {
        // Vertices are duplicated (because of normal vectors)
        var offset = new Vector3(x, y, z);

        // -X face
        if((faceMask & (int)FaceMask.NX) != 0)
        {
            var ioffset = vertices.Count;
            vertices.Add(new Vector3(0, 0, 0) + offset);
            vertices.Add(new Vector3(0, 0, 1) + offset);
            vertices.Add(new Vector3(0, 1, 0) + offset);
            vertices.Add(new Vector3(0, 1, 1) + offset);
            uv.Add(new Vector2(1, 0));
            uv.Add(new Vector2(0, 0));
            uv.Add(new Vector2(1, 1));
            uv.Add(new Vector2(0, 1));
            indices.Add(ioffset);
            indices.Add(ioffset + 1);
            indices.Add(ioffset + 2);
            indices.Add(ioffset + 2);
            indices.Add(ioffset + 1);
            indices.Add(ioffset + 3);
        }
        // +X face
        if ((faceMask & (int)FaceMask.X) != 0)
        {
            var ioffset = vertices.Count;
            vertices.Add(new Vector3(1, 0, 0) + offset);
            vertices.Add(new Vector3(1, 0, 1) + offset);
            vertices.Add(new Vector3(1, 1, 0) + offset);
            vertices.Add(new Vector3(1, 1, 1) + offset);
            uv.Add(new Vector2(0, 0));
            uv.Add(new Vector2(1, 0));
            uv.Add(new Vector2(0, 1));
            uv.Add(new Vector2(1, 1));
            indices.Add(ioffset + 2);
            indices.Add(ioffset + 1);
            indices.Add(ioffset);
            indices.Add(ioffset + 3);
            indices.Add(ioffset + 1);
            indices.Add(ioffset + 2);
        }
        // -Y face
        if ((faceMask & (int)FaceMask.NY) != 0)
        {
            var ioffset = vertices.Count;
            vertices.Add(new Vector3(0, 0, 0) + offset);
            vertices.Add(new Vector3(0, 0, 1) + offset);
            vertices.Add(new Vector3(1, 0, 0) + offset);
            vertices.Add(new Vector3(1, 0, 1) + offset);
            uv.Add(new Vector2(0, 1));
            uv.Add(new Vector2(0, 0));
            uv.Add(new Vector2(1, 1));
            uv.Add(new Vector2(1, 0));
            indices.Add(ioffset + 2);
            indices.Add(ioffset + 1);
            indices.Add(ioffset);
            indices.Add(ioffset + 3);
            indices.Add(ioffset + 1);
            indices.Add(ioffset + 2);
        }
        // +Y face
        if ((faceMask & (int)FaceMask.Y) != 0)
        {
            var ioffset = vertices.Count;
            vertices.Add(new Vector3(0, 1, 0) + offset);
            vertices.Add(new Vector3(0, 1, 1) + offset);
            vertices.Add(new Vector3(1, 1, 0) + offset);
            vertices.Add(new Vector3(1, 1, 1) + offset);
            uv.Add(new Vector2(0, 0));
            uv.Add(new Vector2(0, 1));
            uv.Add(new Vector2(1, 0));
            uv.Add(new Vector2(1, 1));
            indices.Add(ioffset);
            indices.Add(ioffset + 1);
            indices.Add(ioffset + 2);
            indices.Add(ioffset + 2);
            indices.Add(ioffset + 1);
            indices.Add(ioffset + 3);
        }
        // -Z face
        if ((faceMask & (int)FaceMask.NZ) != 0)
        {
            var ioffset = vertices.Count;
            vertices.Add(new Vector3(0, 0, 0) + offset);
            vertices.Add(new Vector3(0, 1, 0) + offset);
            vertices.Add(new Vector3(1, 0, 0) + offset);
            vertices.Add(new Vector3(1, 1, 0) + offset);
            uv.Add(new Vector2(0, 0));
            uv.Add(new Vector2(0, 1));
            uv.Add(new Vector2(1, 0));
            uv.Add(new Vector2(1, 1));
            indices.Add(ioffset);
            indices.Add(ioffset + 1);
            indices.Add(ioffset + 2);
            indices.Add(ioffset + 2);
            indices.Add(ioffset + 1);
            indices.Add(ioffset + 3);
        }
        // +Z face
        if ((faceMask & (int)FaceMask.Z) != 0)
        {
            var ioffset = vertices.Count;
            vertices.Add(new Vector3(0, 0, 1) + offset);
            vertices.Add(new Vector3(0, 1, 1) + offset);
            vertices.Add(new Vector3(1, 0, 1) + offset);
            vertices.Add(new Vector3(1, 1, 1) + offset);
            uv.Add(new Vector2(1, 0));
            uv.Add(new Vector2(1, 1));
            uv.Add(new Vector2(0, 0));
            uv.Add(new Vector2(0, 1));
            indices.Add(ioffset + 2);
            indices.Add(ioffset + 1);
            indices.Add(ioffset);
            indices.Add(ioffset + 3);
            indices.Add(ioffset + 1);
            indices.Add(ioffset + 2);
        }
    }

    private bool BlockIsOpaque(int x, int y, int z)
    {
        if(x < 0 || y < 0 || z < 0 || x >= Chunk.size || y >= Chunk.size || z >= Chunk.size)
        {
            var bc = new BlockCoord(x, y, z);
            var c = manager.Get(chunk.coord + bc.ChunkCoord);
            if(c == null)
            {
                Debug.LogError("ChunkMeshGenerator is missing surrounding chunks " + x + " " + y + " " + z + " -> " + bc.ChunkCoord);
            }
            return Block.Get(c.GetBlock(bc.LocalCoord)).isOpaque;
        }
        return Block.Get(chunk.GetBlock(x, y, z)).isOpaque;
    }

    private int GetVisibleFacesForBlock(int x, int y, int z)
    {
        int f = 0;
        if (!BlockIsOpaque(x - 1, y, z))
        {
            f |= (int)FaceMask.NX;
        }
        if (!BlockIsOpaque(x + 1, y, z))
        {
            f |= (int)FaceMask.X;
        }
        if (!BlockIsOpaque(x, y - 1, z))
        {
            f |= (int)FaceMask.NY;
        }
        if (!BlockIsOpaque(x, y + 1, z))
        {
            f |= (int)FaceMask.Y;
        }
        if (!BlockIsOpaque(x, y, z - 1))
        {
            f |= (int)FaceMask.NZ;
        }
        if (!BlockIsOpaque(x, y, z + 1))
        {
            f |= (int)FaceMask.Z;
        }
        return f;
    }

    private void ProcessBlocks()
    {
        for (int i = 0; i < Chunk.size; ++i)
        {
            for (int j = 0; j < Chunk.size; ++j)
            {
                for (int k = 0; k < Chunk.size; ++k)
                {
                    if (chunk.GetBlock(i, j, k) != (uint)Block.Type.Air)
                    {
                        int faces = GetVisibleFacesForBlock(i, j, k);

                        if (faces > 0)
                        {
                            pushCube(indices[(int)chunk.GetBlock(i, j, k) - 1], i, j, k, faces);
                        }

                        if (indices.Count + 8 >= 65536)
                        {
                            return;
                        }
                    }
                }
            }
        }
    }

    public Mesh Generate()
    {
        // Create mesh
        Mesh mesh = new Mesh();
        mesh.subMeshCount = meshMaterialCount;

        // Generate faces
        ProcessBlocks();

        mesh.vertices = vertices.ToArray();
        mesh.uv = uv.ToArray();
        for(int i = 0; i < meshMaterialCount; ++i)
        {
            mesh.SetTriangles(indices[i], i);
        }

        // Finishing touches
        mesh.Optimize();
        mesh.RecalculateNormals();
        mesh.RecalculateTangents();
        mesh.RecalculateBounds();

        return mesh;
    }
}
