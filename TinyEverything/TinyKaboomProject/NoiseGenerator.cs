using System;
using System.Numerics;
using System.Runtime.CompilerServices;
using static TinyEverything.Common.Utilities;

namespace TinyEverything.TinyKaboomProject
{
    public class NoiseGenerator
    {
        public float HashValue { get; set; } = 43758.5453f;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private float Hash(float n)
        {
            var x = MathF.Sin(n) * HashValue;
            return x - MathF.Floor(x);
        }
        
        private float Noise(Vector3 x)
        {
            var p = new Vector3(MathF.Floor(x.X), MathF.Floor(x.Y), MathF.Floor(x.Z));
            var f = new Vector3(x.X - p.X, x.Y - p.Y, x.Z - p.Z);

            f *= (f * (new Vector3(3.0f, 3.0f, 3.0f) - f * 2.0f));
            var n = Vector3.Dot(p, new Vector3(1.0f, 57.0f, 113.0f));
            return Lerp(Lerp(
                    Lerp(Hash(n + 0.0f), Hash(n + 1.0f), f.X),
                    Lerp(Hash(n + 57.0f), Hash(n + 58.0f), f.X), f.Y),
                Lerp(
                    Lerp(Hash(n + 113.0f), Hash(n + 114.0f), f.X),
                    Lerp(Hash(n + 170.0f), Hash(n + 171.0f), f.X), f.Y), f.Z);
        }

        internal float FractalBrownianMotion(Vector3 x)
        {
            var p = Rotate(x);
            float f = 0;
            f += 0.5000f * Noise(p);
            p *= 2.32f;
            f += 0.2500f * Noise(p);
            p *= 3.03f;
            f += 0.1250f * Noise(p);
            p *= 2.61f;
            f += 0.0625f * Noise(p);
            return f / 0.9375f;
        }
    }
}
