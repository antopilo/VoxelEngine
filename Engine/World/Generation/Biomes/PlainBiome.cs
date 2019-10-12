using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class PlainBiome
{
    public static string Name { get; set; }

    public static void Generate(ref Chunk chunk, int x, int z)
    {
        int globalX = (int)chunk.Position.x * Chunk.CHUNK_SIZE + x;
        int globalZ = (int)chunk.Position.y * Chunk.CHUNK_SIZE + z;

        float height = GetHeight(globalX, globalZ);

        float final = Mathf.Clamp(height, 0, 255);

        for (int i = (int)final - 5; i < final; i++)
        {
            chunk.AddBlock(new Vector3(x, i, z), BLOCK_TYPE.Sand);
        }
            
    }

    public static void GenerateVegetation(ref Chunk chunk, int x, int z)
    {
        RandomNumberGenerator Rng = NoiseMaker.Rng;
        float height = chunk.HighestBlockAt(x, z) + 1;

        // Place the decoration above the ground.
        float temp = NoiseMaker.GetTemperature((int)(chunk.Position.x * 16) + x, 
                                               (int)(chunk.Position.y * 16) + z);

        // Placing plateaus
        //if (Rng.RandiRange(0, 10000) < 1)
        //{
        //    int treeHeight = Rng.RandiRange(10, 50);
        //    int depth = Rng.RandiRange(25, 70);
        //    int width = Rng.RandiRange(25, 70);
        //    chunk.AddBlocks(Plateau.CreatePlateau(width, treeHeight, depth), new Vector3(0, height - 2, 0));
        //}

        // Vegetation
        if (temp > 0.75f) // Flower
        {
            if (Rng.Randf() < 0.2f)
                chunk.AddSprite(new Vector3(x, height, z), Models.Flower);
        }
        else if (temp < 0.25f) // Fern
        {
            if (Rng.Randf() < 0.1f)
                chunk.AddSprite(new Vector3(x, height, z), Models.Fern);
        }
        //else // Tree
        //{
        //    if (Rng.Randf() < 0.1f)
        //        chunk.AddSprite(new Vector3(x, height, z), Models.Grass);

        //    else if (Rng.Randf() < 0.005f)
        //        chunk.AddBlocks(OakTree.GetTreeData(), new Vector3(x - 8, height, z - 8));
        //}
            
    }

    private static float GetHeight(int x, int z)
    {
        float biome = NoiseMaker.GetHumidity(x, z);
        float normalizedNoise = (NoiseMaker.fastNoise2.GetSimplexFractal(x, z) * NoiseMaker.Amplitude + 1f) / 2;
        return (normalizedNoise) * ((Chunk.CHUNK_SIZE * 16) / 2);
    }
}

