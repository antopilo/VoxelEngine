using System;
using Godot;

public class Plateau 
{
    public static int[,,] CreatePlateau(int width, int height, int depth)
    {
        if(width < depth)
        {
            var temp = width;
            width = depth;
            depth = temp;
        }

        var data = new int[width,height,depth];

        // Filling array with air.
        for(int x = 0; x < width; x++)
            for(int y = 0; y < height; y++)
                for(int z = 0; z < depth; z++)
                    data[x, y, z] = (int)BLOCK_TYPE.Empty;

        var center = new Vector2(width / 2, depth / 2);
        var ratio = width / depth;
        for(int x = 0; x < width; x++)
            for(int z = 0; z < depth; z++)
                if((center - new Vector2(x, z)).Length() < width / 2)
                {
                    for(int y = 0; y < height; y++)
                    {
                        if(y == height - 1)
                            data[x, y, z] = (int)BLOCK_TYPE.Grass;
                        else
                            data[x, y, z] = (int)BLOCK_TYPE.Stone;
                    }
                }
        
        return data;
    }
}