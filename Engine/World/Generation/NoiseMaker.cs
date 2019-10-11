using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class NoiseMaker
{
    private static int seed = 0;
    private static FastNoise fastNoise = new FastNoise();
    public static FastNoise fastNoise2 = new FastNoise();
    public static RandomNumberGenerator Rng = new RandomNumberGenerator();
    private static OpenSimplexNoise Noise = new OpenSimplexNoise();
    private static OpenSimplexNoise Humidity = new OpenSimplexNoise();
    private static OpenSimplexNoise Temperature = new OpenSimplexNoise();

    private static float WaterLevel = 80f;
    public static float Amplitude = 1f;
    private static BLOCK_TYPE newBlock = BLOCK_TYPE.Stone;


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
        seed = Rng.RandiRange(0, 99999999);
        fastNoise.SetNoiseType(FastNoise.NoiseType.Cellular);
        fastNoise.SetSeed(seed);
        fastNoise.SetFrequency(0.0025f);
        fastNoise.SetCellularJitter(0.5f);
        fastNoise.SetCellularDistanceFunction(FastNoise.CellularDistanceFunction.Natural);
        fastNoise.SetCellularReturnType(FastNoise.CellularReturnType.CellValue);

        fastNoise2.SetSeed(seed);
        fastNoise2.SetNoiseType(FastNoise.NoiseType.SimplexFractal);
        fastNoise2.SetFrequency(0.0015f);
        fastNoise2.SetFractalLacunarity(2);
        fastNoise2.SetFractalGain(0.75f);

        Noise.Seed = seed;
        Noise.Octaves = 12;
        Noise.Period = 64;
        Noise.Persistence = 0.8f;
        Noise.Lacunarity = 0.7f;

        Humidity.Seed = Rng.RandiRange(0, 99999);
        Humidity.Octaves = 5;
        Humidity.Period = 64;
        Humidity.Persistence = 0.9f;
        Humidity.Lacunarity = 0.5f;

        Temperature.Seed = Rng.RandiRange(0, 99999);
        Temperature.Octaves = 1;
        Temperature.Period = 16;
        Temperature.Persistence = 0.6f;
        Temperature.Lacunarity = 0.75f;
    }

    public static void GenerateChunk(Chunk chunk)
    {
        int gx = (int)chunk.Position.x * 16;
        int gz = (int)chunk.Position.y * 16;

        for (int x = 0; x < Chunk.CHUNK_SIZE; x ++)
            for (int z = 0; z < Chunk.CHUNK_SIZE; z ++)
            {
                BIOME_TYPE biome = GetBiome(gx + x, gz + z);
                switch (biome)
                {
                    case BIOME_TYPE.Plains:
                        PlainBiome.Generate(ref chunk, x, z);
                        break;
                    case BIOME_TYPE.Desert:
                        DesertBiome.Generate(ref chunk, x, z);
                        break;
                    case BIOME_TYPE.Sea:
                        SeaBiome.Generate(ref chunk, x, z);
                        break;
                    case BIOME_TYPE.Forest:
                        ForestBiome.Generate(ref chunk, x, z);
                        break;
                }
            }
    }

    public static void GenerateChunkDecoration(Chunk chunk)
    {
        int gx = (int)chunk.Position.x * 16;
        int gz = (int)chunk.Position.y * 16;

        for (int x = 0; x < Chunk.CHUNK_SIZE; x++)
            for (int z = 0; z < Chunk.CHUNK_SIZE; z++)
            {
                BIOME_TYPE biome = GetBiome(gx + x, gz + z);
                switch (biome)
                {
                    case BIOME_TYPE.Plains:
                        PlainBiome.GenerateVegetation(ref chunk, x, z);
                        break;
                    case BIOME_TYPE.Desert:
                        DesertBiome.GenerateVegetation(ref chunk, x, z);
                        break;
                    case BIOME_TYPE.Sea:
                        SeaBiome.GenerateVegetation(ref chunk, x, z);
                        break;
                    case BIOME_TYPE.Forest:
                        ForestBiome.GenerateVegetation(ref chunk, x, z);
                        break;
                }
        }
    }


    public static float GetTemperature(int x, int z)
    {
        return (Temperature.GetNoise2d(x, z) + 1f) / 2f;
    }

    public static float GetHumidity(int x, int z)
    {
        return (Humidity.GetNoise2d(x, z) + 1f) / 2f;
    }

    private static BIOME_TYPE GetBiome(int gx, int gz)
    {
        // Voronoi range from 0 to 1.
        float voronoi = fastNoise.GetCellular(gx, gz);
        if (0f <= voronoi && voronoi < 0.25f)
        {
            Amplitude = 1f;
            return BIOME_TYPE.Plains;
        }
        else if (0.25f <= voronoi && voronoi < 0.5f)
        {
            Amplitude = 0.3f;
            return BIOME_TYPE.Desert;
        }
        else if (0.5f <= voronoi && voronoi < 0.75f)
        {
            Amplitude = 1f;
            return BIOME_TYPE.Sea;
        }
        else
        {
            Amplitude = 0.2f;
            return BIOME_TYPE.Forest;
        }
    }


    /// <summary>
    /// Calculate a bilinear interpolation between 4 corners 
    /// at a given point;
    /// </summary>
    /// <param name="q11">Bottom left corner value</param>
    /// <param name="q21">Bottom right corner value</param>
    /// <param name="q12">Top left corner value</param>
    /// <param name="q22">Top right corner value</param>
    /// <param name="position">point</param>
    /// <param name="distanceBetweenCorners">Sample rate.</param>
    /// <returns></returns>
    private static float BilinearInterpolation(float q11, float q21, float q12, float q22, Vector2 position, int distanceBetweenCorners)
    {
        float r2 = Mathf.Lerp(q12, q22, position.x + 1 / distanceBetweenCorners);
        float r1 = Mathf.Lerp(q11, q21, position.x + 1 / distanceBetweenCorners);
        float p = Mathf.Lerp(r1, r2, position.y / q22);
        return p;
    }
}


