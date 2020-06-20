using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;

public class Chunk
{
    public const int size = 16; // 16 * 16 * 16 = 4096

    // TODO consider different data type?
    public byte[,,] content = new byte[size, size, size];
    public GameObject chunkMesh;

    public Vector3Int coord;

    // TODO
    // public bool isModified;

    public uint GetBlock(int x, int y, int z)
    {
        return content[x, y, z];
    }
    public uint GetBlock(in Vector3Int v)
    {
        return content[v.x, v.y, v.z];
    }

    public void SetBlock(int x, int y, int z, byte id)
    {
        content[x, y, z] = id;
    }
    public void SetBlock(in Vector3Int v, byte id)
    {
        content[v.x, v.y, v.z] = id;
    }

    public void Fill(byte id)
    {
        // TODO foreach ???
        for(int i = 0; i < size; ++i)
        {
            for(int j = 0; j < size; ++j)
            {
                for(int k = 0; k < size; ++k)
                {
                    content[i, j, k] = id;
                }
            }
        }
    }

    public void Clear()
    {
        Fill(0);
    }

    public Chunk(in Vector3Int coord) {
        this.coord = coord;
    }
    public Chunk(in Vector3Int coord, byte id) : this(coord)
    {
        Fill(id);
    }
}
