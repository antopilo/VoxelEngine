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
    private static FastNoise fastNoise2 = new FastNoise();
    public static RandomNumberGenerator Rng = new RandomNumberGenerator();
    private static OpenSimplexNoise Noise = new OpenSimplexNoise();
    private static OpenSimplexNoise Humidity = new OpenSimplexNoise();
    private static OpenSimplexNoise Temperature = new OpenSimplexNoise();

    private static float WaterLevel = 80f;
    private static float Amplitude = 1f;
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
        int offsetX = (int)chunk.Position.x * Chunk.CHUNK_SIZE;
        int offsetZ = (int)chunk.Position.y * Chunk.CHUNK_SIZE;
        for (int x = 0; x < Chunk.CHUNK_SIZE; x++)
            for (int z = 0; z < Chunk.CHUNK_SIZE; z++)
            {
                int gx = x + offsetX;
                int gz = z + offsetZ;
                
                chunk.Biome = GetBiome(gx, gz);

                float height = Mathf.Pow(GetHeight(gx, gz), 1.1f);
                float final = Mathf.Clamp(height , 0, 255);
                
                for (int i = (int)final - 5; i < final; i++)
                {
                    chunk.AddBlock(new Vector3(x, i, z), newBlock);
                }
            }
    }

    public static void GenerateVegetation(Chunk chunk)
    {
        int offsetX = (int)chunk.Position.x * Chunk.CHUNK_SIZE;
        int offsetZ = (int)chunk.Position.y * Chunk.CHUNK_SIZE;

        int biome = chunk.Biome;

        for (int x = 0; x < Chunk.CHUNK_SIZE; x++)
            for (int z = 0; z < Chunk.CHUNK_SIZE; z++)
            {
                int gx = x + offsetX;
                int gz = z + offsetZ;
                float temp = GetTemperature(gx, gz);
                int final = chunk.HighestBlockAt(x, z);

                // GRASS BIOME.
                if(biome == 1)
                {
                    if(Rng.RandiRange(0, 10000) < 1)
                    {
                        GD.Print("PLATEAU SPAWNED !!!!");
                        int height = Rng.RandiRange(10,50);
                        int depth = Rng.RandiRange(25,70);
                        int width = Rng.RandiRange(25,70);
                        chunk.AddBlocks(Plateau.CreatePlateau(width, height, depth), new Vector3(0, final - 2, 0));
                        
                    }

                    if (temp > 0.75f)
                    {
                        if (Rng.Randf() < 0.2f)
                            chunk.AddSprite(new Vector3(x, final + 1, z), Models.Flower);
                    }

                    else if (temp < 0.25f)
                    {
                        if (Rng.Randf() < 0.1f)
                            chunk.AddSprite(new Vector3(x, final + 1, z), Models.Fern);
                    }
                    else
                    {
                        
                        if(Rng.Randf() < 0.1f)
                            chunk.AddSprite(new Vector3(x, final + 1, z), Models.Grass);

                        //else if(Rng.Randf() < 0.005f)
                        //    chunk.AddBlocks(OakTree.GetTreeData(), new Vector3(x - 8, final + 1, z - 8));
                            
                    }
                }
                // DESERT BIOME.
                else if(biome == 3)
                {

                    if(Rng.RandiRange(0,1000) < 2)
                    {
                        var size = Rng.RandiRange(8,16);
                        chunk.AddBlocks(Boulders.GetBoulder(size), new Vector3(x, final - size/2, z));
                            
                    }

                    if(temp> 0.80f)
                    {
                        // 5% to place a cactus
                        if(Rng.Randf() < 0.05f)
                        {
                            // Decide random height.
                            var cactusHeight = Rng.RandiRange(0, 4);
                            for(int i = 0; i < cactusHeight; i++)
                            {
                                var position = new Vector3(x, final + 1 + i, z);

                                // Top of the cactus
                                if(i == cactusHeight - 1)
                                {
                                    // No flower default.
                                    Models topModel = Models.CactusTopCutoff;

                                    // If the cactus is taller than 2 blocks. It has a flower.
                                    if(cactusHeight == 2)
                                        topModel = Rng.Randf() > 0.5f ? Models.CactusTop : Models.CactusTopCutoff;
                                    else if(cactusHeight > 2)
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

                    else if(temp > 0.6)
                    {
                        if(Rng.Randf() < 0.0025f)
                            chunk.AddSprite(new Vector3(x, final + 1, z), Models.DeadBush);
                    }
                }

            }
    }


    private static float GetHeight(int x, int z)
    {
        float biome = GetHumidity(x, z);
        float normalizedNoise = (fastNoise2.GetSimplexFractal(x, z) * Amplitude + 1f) / 2;
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

    private static int GetBiome(int gx, int gz)
    {
        float voronoi = fastNoise.GetCellular(gx, gz);
        if (0f <= voronoi && voronoi < 0.5f)
        {
            newBlock = BLOCK_TYPE.Dirt;
            Amplitude = 1f;
            return 0;
        }
        else if (0.5f <= voronoi && voronoi < 1f)
        {
            newBlock = BLOCK_TYPE.Grass;
            Amplitude = 0.3f;
            return 1;
        }
        else if (1f <= voronoi && voronoi < 1.5f)
        {
            newBlock = BLOCK_TYPE.Water;
            Amplitude = 0f;
            return 2;
        }
        else
        {
            newBlock = BLOCK_TYPE.Sand;
            Amplitude = 0.2f;
            return 3;
        }
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

