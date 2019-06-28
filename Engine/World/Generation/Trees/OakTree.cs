using System;
using Godot;

public static class OakTree
{
    
    public static int[,,] GetTreeData()
    {
        var data = new int[16,16,16];

        for(int x = 0; x < 16; x++)
            for(int y = 0; y < 16; y++)
                for(int z = 0; z < 16; z++)
                    data[x, y, z] = (int)BLOCK_TYPE.Empty;

        var origin = new Vector3(8, 0, 8);
        var trunkHeight = NoiseMaker.Rng.RandiRange(4, 7);   

        for(int i = 0; i < trunkHeight; i++)
        {
            data[(int)origin.x, i, (int)origin.z] = (int)BLOCK_TYPE.Trunk;
        }

        // Top cross
        data[(int)origin.x, trunkHeight, (int)origin.z]  = (int)BLOCK_TYPE.Leaves;
        data[(int)origin.x - 1, trunkHeight, (int)origin.z]  = (int)BLOCK_TYPE.Leaves;
        data[(int)origin.x + 1, trunkHeight, (int)origin.z]  = (int)BLOCK_TYPE.Leaves;
        data[(int)origin.x, trunkHeight, (int)origin.z + 1]  = (int)BLOCK_TYPE.Leaves;
        data[(int)origin.x, trunkHeight, (int)origin.z - 1]  = (int)BLOCK_TYPE.Leaves;

        // Second layer of top.
        data[(int)origin.x - 1, trunkHeight - 1, (int)origin.z]  = (int)BLOCK_TYPE.Leaves;
        data[(int)origin.x + 1, trunkHeight - 1, (int)origin.z]  = (int)BLOCK_TYPE.Leaves;
        data[(int)origin.x, trunkHeight - 1, (int)origin.z + 1]  = (int)BLOCK_TYPE.Leaves;
        data[(int)origin.x, trunkHeight - 1, (int)origin.z - 1]  = (int)BLOCK_TYPE.Leaves;

        //var startPos = new Vector3(origin.x - 3, trunkHeight - 2, origin.z - 3);
        //var endPos = new Vector3(origin.x + 3, trunkHeight - 4, origin.z + 3);

        
        var startPos = new Vector3(origin.x - 2, trunkHeight - 2, origin.z - 2);
        var endPos = new Vector3(origin.x + 2, trunkHeight - 1, origin.z + 2);
        FillRectangle(data, startPos, endPos);

        return data;
    }

    public static void FillRectangle( int[,,] data, Vector3 startPos, Vector3 endPos)
    {
        // AABB of rectangle.
        int startX = (int)startPos.x, startY = (int)startPos.y, startZ = (int)startPos.z;
        int endX   = (int)endPos.x,     endY = (int)endPos.y,     endZ = (int)endPos.z;

        // We placing leaves right here man.
        var blockType = BLOCK_TYPE.Leaves;

        for(int x = startX; x <= endX; x++)
            for(int y = startY; y <= endY; y++)
                for(int z = startZ; z <= endZ; z++)
                {
                    // Donc do corners!
                    if((x != startX || x != endX) && (z != startZ || z != endZ))
                    {
                        // If there is already a block skip.
                        if(data[x, y ,z] != -1)
                            continue;

                        // Place leaves block.
                        data[x, y, z] = (int)blockType;
                        }
                }
    }
}