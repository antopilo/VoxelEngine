using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VoxelEngine.engine.World;

public class NoiseMaker
{
    private static RandomNumberGenerator Rng = new RandomNumberGenerator();
    private static OpenSimplexNoise Noise = new OpenSimplexNoise();

    public static void Initialize()
    {
        Rng.Randomize();
        
        Noise.Seed = Rng.RandiRange(0, 99999);

        Noise.Octaves = 5;
        Noise.Period = 64;
        Noise.Persistence = 0.6f;
        Noise.Lacunarity = 0.75f;
    }

    public static void GenerateChunk(Chunk chunk)
    {
        int offsetX = (int)chunk.Position.x * Chunk.CHUNK_SIZE;
        int offsetZ = (int)chunk.Position.y * Chunk.CHUNK_SIZE;

        BLOCK_TYPE newBlock = BLOCK_TYPE.Stone;
        for (int x = 0; x < Chunk.CHUNK_SIZE; x++)
        {
            for (int z = 0; z < Chunk.CHUNK_SIZE; z++)
            {

                float height = ((Noise.GetNoise2d(x + offsetX, z + offsetZ) + 1f) / 2 ) * (Chunk.CHUNK_SIZE * 16) / 2;
                for (int i = 0; i < height; i++)
                {
                    if (i > height - 5)
                        newBlock = BLOCK_TYPE.Grass;
                    else
                        newBlock = BLOCK_TYPE.Stone;

                    chunk.AddBlock(new Vector3(x, i, z), newBlock);
                }
               
            }
        }
        
    }
}

