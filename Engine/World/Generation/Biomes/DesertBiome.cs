using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public static class DesertBiome
{
    public static void Generate(ref Chunk chunk)
    {
        int offsetX = (int)chunk.Position.x * Chunk.CHUNK_SIZE;
        int offsetZ = (int)chunk.Position.y * Chunk.CHUNK_SIZE;
        for (int x = 0; x < Chunk.CHUNK_SIZE; x++)
            for (int z = 0; z < Chunk.CHUNK_SIZE; z++)
            {
                int gx = x + offsetX;
                int gz = z + offsetZ;

                float height = Mathf.Pow(GetHeight(gx, gz), 1.1f);
                float final = Mathf.Clamp(height, 0, 255);

                for (int i = (int)final - 5; i < final; i++)
                {
                    chunk.AddBlock(new Vector3(x, i, z), BLOCK_TYPE.Stone);
                }

            }
    }

    public static void GenerateVegetation(ref Chunk chunk)
    {
        RandomNumberGenerator Rng = NoiseMaker.Rng;
        float height;

        int offsetX = (int)chunk.Position.x * Chunk.CHUNK_SIZE;
        int offsetZ = (int)chunk.Position.y * Chunk.CHUNK_SIZE;
        for (int x = 0; x < Chunk.CHUNK_SIZE; x++)
            for (int z = 0; z < Chunk.CHUNK_SIZE; z++)
            {
                // Place the decoration above the ground.
                height = chunk.HighestBlockAt(x, z) + 1;
                float temp = NoiseMaker.GetTemperature((int)(chunk.Position.x * 16) + x,
                                                        (int)(chunk.Position.x * 16f) + z);
                if (Rng.RandiRange(0, 1000) < 2)
                {
                    var size = Rng.RandiRange(8, 16);
                    chunk.AddBlocks(Boulders.GetBoulder(size), new Vector3(x, height - size / 2, z));

                }

                if (temp > 0.80f)
                {
                    // 5% to place a cactus
                    if (Rng.Randf() < 0.05f)
                    {
                        // Decide random height.
                        var cactusHeight = Rng.RandiRange(0, 4);
                        for (int i = 0; i < cactusHeight; i++)
                        {
                            var position = new Vector3(x, height + i, z);

                            // Top of the cactus
                            if (i == cactusHeight - 1)
                            {
                                // No flower default.
                                Models topModel = Models.CactusTopCutoff;

                                // If the cactus is taller than 2 blocks. It has a flower.
                                if (cactusHeight == 2)
                                    topModel = Rng.Randf() > 0.5f ? Models.CactusTop : Models.CactusTopCutoff;
                                else if (cactusHeight > 2)
                                    topModel = Models.CactusTop;

                                // Add the top.
                                chunk.AddSprite(position, topModel);
                            }
                            else
                            {
                                // Normal body of cactus.
                                chunk.AddSprite(position, Models.CactusMid);
                            }
                        }
                    }
                }
                else if (temp > 0.6)
                {
                    if (Rng.Randf() < 0.0025f)
                        chunk.AddSprite(new Vector3(x, height, z), Models.DeadBush);
                }
            }

                
        
    
    }

    private static float GetHeight(int x, int z)
    {
        float biome = NoiseMaker.GetHumidity(x, z);
        float normalizedNoise = (NoiseMaker.fastNoise2.GetSimplexFractal(x, z) * NoiseMaker.Amplitude + 1f) / 2;
        return (normalizedNoise) * ((Chunk.CHUNK_SIZE * 16) / 2);
    }
}

