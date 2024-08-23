using System.Collections.Generic;
using Auburn.FastNoiseLite;

namespace Data
{
    public class World
    {
        private readonly List<FastNoiseLite> _noises = new();
        private int _seed;

        public void SetSeed(int seed)
        {
            _seed = seed;
            InitNoise();
        }

        private void InitNoise()
        {
            var random = new System.Random(_seed);
            _noises.Add(new FastNoiseLite(random.Next()));
            _noises.Add(new FastNoiseLite(random.Next()));
            _noises.Add(new FastNoiseLite(random.Next()));
            _noises.Add(new FastNoiseLite(random.Next()));
            foreach (var fastNoise in _noises)
            {
                fastNoise.SetNoiseType(FastNoiseLite.NoiseType.Perlin);
            }
        }

        private float GetNoiseValue(float x, float y)
        {
            var value = 0.0f;
            foreach (var noise in _noises)
            {
                value += noise.GetNoise(x, y);
            }

            value += _noises.Count;
            value /= _noises.Count * 2;

            return value;
        }
    }
}