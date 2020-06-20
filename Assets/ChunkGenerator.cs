using System;
using UnityEngine;
using UnityEngine.UIElements;

public class ChunkGenerator
{
    private const float baseElevation = 64f;

    private const float noiseScale = 0.035f;
    private const float noiseElevationScale = 96.0f;

    private const float noiseScale2 = 0.01f;
    private const float noiseElevationScale2 = 384.0f;

    private const float noiseScale3 = 0.001f;
    // The holes in the ground
    private const float holeNoiseScale = 0.065f;
    private const float holeBias = 0.3f;

    private const float snowBias = 128.0f;
    private const float snowBiasRandomization = 16.0f;

    private NoiseGenerator noiseGenerator;

    public ChunkGenerator(int seed)
    {
        noiseGenerator = new NoiseGenerator(seed);
    }

    private float Noise3d(float x, float y, float z)
    {
        return noiseGenerator.Noise3dInterpolated(x, y, z);
    }

    private float ElevationAt(Vector3Int coord, float x, float z)
    {
        float f = Mathf.PerlinNoise(noiseScale * (x + coord.x * Chunk.size), noiseScale * (z + coord.z * Chunk.size));
        float f2 = Mathf.PerlinNoise(noiseScale2 * (x + coord.x * Chunk.size), noiseScale2 * (z + coord.z * Chunk.size));
        float f3 = Mathf.PerlinNoise(noiseScale3 * (x + coord.x * Chunk.size), noiseScale3 * (z + coord.z * Chunk.size));
        return Mathf.FloorToInt((f * noiseElevationScale + f2 * noiseElevationScale2) * Mathf.Pow(f3, 2.0f) + baseElevation);
    }

    public Chunk Generate(Vector3Int coord)
    {
        //Let's start with height map based generation
        Chunk chunk = new Chunk(coord);

        // Go over X and Z
        for(int i = 0; i < Chunk.size; ++i)
        {
            for (int j = 0; j < Chunk.size; ++j)
            {
                var elevation = ElevationAt(coord, i, j);
                var localHeight = Math.Min(Chunk.size, elevation - coord.y * Chunk.size);
                var grassOnTop = elevation - coord.y * Chunk.size <= Chunk.size;

                for (int k = 0; k < localHeight; ++k)
                {
                    if (Noise3d(holeNoiseScale * (i + coord.x * Chunk.size), holeNoiseScale * (k + coord.y * Chunk.size), holeNoiseScale * (j + coord.z * Chunk.size)) > holeBias)
                    {
                        if (grassOnTop && k + 1 == localHeight)
                        {
                            var bcoord = new BlockCoord();
                            bcoord.ChunkCoord = coord;
                            bcoord.LocalCoord = new Vector3Int(i, k, j);
                            var randomization = noiseGenerator.Noise3dInterpolated(i, j, k) * snowBiasRandomization;
                            if (elevation >= snowBias + randomization)
                            {
                                chunk.SetBlock(i, k, j, (byte)Block.Type.Snow);
                            }
                            else
                            {
                                chunk.SetBlock(i, k, j, (byte)Block.Type.Grass);
                            }
                        }
                        else if (grassOnTop && k + 3 >= localHeight)
                        {
                            chunk.SetBlock(i, k, j, (byte)Block.Type.Dirt);
                        }
                        else
                        {
                            chunk.SetBlock(i, k, j, (byte)Block.Type.Stone);
                        }
                    }
                }
            }
        }

        return chunk;
    }

    public Vector3 PickSpawnSpot()
    {
        return new BlockCoord(0, Mathf.CeilToInt(ElevationAt(new Vector3Int(0, 0, 0), 0, 0)) + 2, 0).block;
    }
}
