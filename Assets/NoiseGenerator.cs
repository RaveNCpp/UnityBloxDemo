using UnityEngine;

/// <summary>
/// Value noise generator
/// </summary>
public class NoiseGenerator
{
    private const int noiseSampleCount = 16;
    private float[,,] noiseMap = new float[noiseSampleCount, noiseSampleCount, noiseSampleCount];

    public NoiseGenerator(int seed)
    {
        // TODO get some nice RNG object
        UnityEngine.Random.InitState(seed);
        for (int i = 0; i < Chunk.size; ++i)
        {
            for (int j = 0; j < Chunk.size; ++j)
            {
                for (int k = 0; k < Chunk.size; ++k)
                {
                    noiseMap[i, j, k] = UnityEngine.Random.Range(0.0f, 1.0f);
                }
            }
        }
    }
    public float Noise3dInterpolated(float x, float y, float z)
    {
        x = Mathf.Repeat(x, noiseSampleCount);
        y = Mathf.Repeat(y, noiseSampleCount);
        z = Mathf.Repeat(z, noiseSampleCount);

        // Sample each corner
        var a = noiseMap[Mathf.FloorToInt(x), Mathf.FloorToInt(y), Mathf.FloorToInt(z)];
        var b = noiseMap[Mathf.FloorToInt(x), Mathf.FloorToInt(y), Mathf.CeilToInt(z) % noiseSampleCount];
        var c = noiseMap[Mathf.FloorToInt(x), Mathf.CeilToInt(y) % noiseSampleCount, Mathf.FloorToInt(z)];
        var d = noiseMap[Mathf.FloorToInt(x), Mathf.CeilToInt(y) % noiseSampleCount, Mathf.CeilToInt(z) % noiseSampleCount];
        var e = noiseMap[Mathf.CeilToInt(x) % noiseSampleCount, Mathf.FloorToInt(y), Mathf.FloorToInt(z)];
        var f = noiseMap[Mathf.CeilToInt(x) % noiseSampleCount, Mathf.FloorToInt(y), Mathf.CeilToInt(z) % noiseSampleCount];
        var g = noiseMap[Mathf.CeilToInt(x) % noiseSampleCount, Mathf.CeilToInt(y) % noiseSampleCount, Mathf.FloorToInt(z)];
        var h = noiseMap[Mathf.CeilToInt(x) % noiseSampleCount, Mathf.CeilToInt(y) % noiseSampleCount, Mathf.CeilToInt(z) % noiseSampleCount];

        // Interpolate Z
        var rz = z % 1.0f;
        var iza = Mathf.Lerp(a, b, rz);
        var izb = Mathf.Lerp(c, d, rz);
        var izc = Mathf.Lerp(e, f, rz);
        var izd = Mathf.Lerp(g, h, rz);

        // Interpolate Y
        var ry = y % 1.0f;
        var iya = Mathf.Lerp(iza, izb, ry);
        var iyb = Mathf.Lerp(izc, izd, ry);

        return Mathf.Lerp(iya, iyb, x % 1.0f);
    }

}
