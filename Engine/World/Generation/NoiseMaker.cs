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
    private static OpenSimplexNoise Humidity = new OpenSimplexNoise();
    private static OpenSimplexNoise Temperature = new OpenSimplexNoise();

    private static int[,] m_Biomes = { 
        { 0, 0, 0, 0 ,0},
        { 1, 1, 1, 1, 1},
        { 2, 2, 2, 2, 2},
        { 3, 3, 3, 3, 3},
        { 4, 4, 4, 4, 4}
    };

    public static void Initialize()
    {
        Rng.Randomize();
        
        Noise.Seed = Rng.RandiRange(0, 99999);
        Noise.Octaves = 16;
        Noise.Period = 256;
        Noise.Persistence = 0.8f;
        Noise.Lacunarity = 0.7f;

        Humidity.Seed = Rng.RandiRange(0, 99999);
        Humidity.Octaves = 5;
        Humidity.Period = 64;
        Humidity.Persistence = 0.6f;
        Humidity.Lacunarity = 0.75f;

        Temperature.Seed = Rng.RandiRange(0, 99999);
        Temperature.Octaves = 2;
        Temperature.Period = 128;
        Temperature.Persistence = 0.6f;
        Temperature.Lacunarity = 0.75f;
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
                int gx = x + offsetX;
                int gz = z + offsetZ;
                
                float height = Mathf.Pow(GetHeight(gx, gz), 1.4f);
                float final = Mathf.Clamp(height, 0, 255);

                for (int i = 0; i < final; i++)
                {
                    if (i == final)
                        newBlock = BLOCK_TYPE.Grass;

                    chunk.AddBlock(new Vector3(x, i, z), newBlock);
                }
            }
        }
        
    }

    private static float GetHeight(int x, int z)
    {
        float biome = GetHumidity(x, z);
        float normalizedNoise = (Noise.GetNoise2d(x, z) + 1f) / 2;
        return (normalizedNoise) * ((Chunk.CHUNK_SIZE * 16) / 2);
    }


    private static float GetTemperature(int x, int z)
    {
        return (Temperature.GetNoise2d(x, z) + 1f) / 2f;
    }

    public static float GetHumidity(int x, int z)
    {
        return (Humidity.GetNoise2d(x, z) + 1f) / 2f;
    }

    private static float GetBiome(int x, int z)
    {
        int temp = (int)(GetTemperature(x, z) * 5);
        int hum = (int)(GetHumidity(x, z) * 5);

        return m_Biomes[temp, hum];
    }

    private static float BilinearInterpolation(float bottomLeft, float topLeft, float bottomRight, float topRight,
                                        float xMin, float xMax, float zMin, float zMax, float x, float z)
    {
        float width = xMax - xMin;
        float height = zMax - zMin;
        float xDistanceToMaxValue = xMax - x;
        float zDistanceToMaxValue = zMax - z;
        float xDistanceToMinValue = xMin - x;
        float zDistanceToMinValue = zMin - z;

        return 1.0f / (width - height) * (
            bottomLeft * xDistanceToMaxValue * zDistanceToMaxValue +
            bottomRight * xDistanceToMinValue * zDistanceToMaxValue + 
            topLeft * xDistanceToMaxValue * zDistanceToMinValue +
            topRight * xDistanceToMinValue * zDistanceToMinValue
        );
    }
}

