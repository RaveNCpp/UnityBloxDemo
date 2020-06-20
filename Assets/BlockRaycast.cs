using UnityEngine;

public struct BlockRaycastResult
{
    public Vector3 start;
    public Vector3 hit;
    public BlockCoord hitBlock;
    public Block block;
    public Block.Face face;

    public BlockCoord GetPlacementBlockPosition()
    {
        switch(face)
        {
            case Block.Face.X:
                return new BlockCoord(hitBlock.block.x + 1, hitBlock.block.y, hitBlock.block.z);
            case Block.Face.Y:
                return new BlockCoord(hitBlock.block.x, hitBlock.block.y + 1, hitBlock.block.z);
            case Block.Face.Z:
                return new BlockCoord(hitBlock.block.x, hitBlock.block.y, hitBlock.block.z + 1);
            case Block.Face.NX:
                return new BlockCoord(hitBlock.block.x - 1, hitBlock.block.y, hitBlock.block.z);
            case Block.Face.NY:
                return new BlockCoord(hitBlock.block.x, hitBlock.block.y - 1, hitBlock.block.z);
            case Block.Face.NZ:
                return new BlockCoord(hitBlock.block.x, hitBlock.block.y, hitBlock.block.z - 1);
        }

        return hitBlock;
    }
}
