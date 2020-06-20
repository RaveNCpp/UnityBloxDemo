using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Block
{
    public enum Type : byte
    {
        Air = 0,
        Stone,
        Dirt,
        Grass,
        Snow,

        Count,
    }

    public enum Face : int
    {
        X,
        Y,
        Z,
        NX,
        NY,
        NZ,
    }

    public string name;
    public float hardness;
    public bool isOpaque;
    public Material material;

    public Block(string name, float hardness, bool isOpaque, Material material)
    {
        this.name = name;
        this.hardness = hardness;
        this.isOpaque = isOpaque;
        this.material = material;
    }

    public static Block[] blocks;

    public static void InitBlocks()
    {
        if(blocks == null)
        {
            blocks = new Block[]
            {
                new Block("Air", 0.0f, false, Resources.Load("Materials/Indicator") as Material),
                new Block("Stone", 5.0f, true, Resources.Load("Materials/Stone") as Material),
                new Block("Dirt", 2.0f, true, Resources.Load("Materials/Dirt") as Material),
                new Block("Grass", 3.0f, true, Resources.Load("Materials/Grass") as Material),
                new Block("Snow", 1.0f, true, Resources.Load("Materials/Snow") as Material)
            };
        }
    }

    public static Block Get(uint id)
    {
        if(id >= blocks.Length)
        {
            return null;
        }
        return blocks[id];
    }
}
