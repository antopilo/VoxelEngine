using System;
using Godot;

public class Boulders
{
    public static int[,,] GetBoulder(float size)
    {
        var data = new int[(int)size, (int)size, (int)size];
        var center = new Vector3(size / 2, size / 2, size / 2);

        // Filling array with air.
        for(int x = 0; x < size; x++)
            for(int y = 0; y < size; y++)
                for(int z = 0; z < size; z++)
                    data[x, y, z] = (int)BLOCK_TYPE.Empty;

        for(int x = 0; x < 16; x++)
            for(int y = 0; y < 16; y++)
                for(int z = 0; z < 16; z++)
                    if((new Vector3(x + NoiseMaker.Rng.RandiRange(0,1), y + NoiseMaker.Rng.RandiRange(0,1), z + NoiseMaker.Rng.RandiRange(0,1)) - center).Length() < size / 2)
                         data[x, y, z] = (int)BLOCK_TYPE.Stone;

        return data;
    }
}