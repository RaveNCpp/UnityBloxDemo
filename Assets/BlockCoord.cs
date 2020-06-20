using UnityEngine;

public struct BlockCoord
{
    public BlockCoord(in Vector3Int block)
    {
        this.block = block;
    }

    public BlockCoord(int x, int y, int z) : this(new Vector3Int(x, y, z))
    {
    }

    public Vector3Int block;

    public Vector3Int LocalCoord
    {
        get => new Vector3Int(Mathf.FloorToInt(Mathf.Repeat(block.x, Chunk.size)), Mathf.FloorToInt(Mathf.Repeat(block.y, Chunk.size)), Mathf.FloorToInt(Mathf.Repeat(block.z, Chunk.size)));
        set => block = ChunkCoord + value;
    }

    private static int CorrectChunkCoord(int x)
    {
        return x >= 0 ? x / Chunk.size : (x + 1) / Chunk.size - 1;
    }

    public Vector3Int ChunkCoord {
        get => new Vector3Int(CorrectChunkCoord(block.x), CorrectChunkCoord(block.y), CorrectChunkCoord(block.z));
        set => block = value * Chunk.size;
    }

    public static bool operator ==(in BlockCoord a, in BlockCoord b)
    {
        return a.block == b.block;
    }

    public static bool operator !=(in BlockCoord a, in BlockCoord b)
    {
        return a.block != b.block;
    }

    // override object.Equals
    public override bool Equals(object obj)
    {
        return block.Equals(obj);
    }

    // override object.GetHashCode
    public override int GetHashCode()
    {
        return block.GetHashCode();
    }
}
