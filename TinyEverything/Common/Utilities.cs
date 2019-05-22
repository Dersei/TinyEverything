using System;
using System.Numerics;

namespace TinyEverything.Common
{
    public static class Utilities
    {
        public static Vector3 Normalize(this Vector3 @this)
        {
            return Vector3.Normalize(@this);
        }

        internal static float Lerp(float first, float second, float value)
        {
            return first + (second - first) * MathF.Max(0.0f, MathF.Min(1.0f, value));
        }

        public static Vector3 Rotate(Vector3 vector)
        {
            return new Vector3(Vector3.Dot(new Vector3(0.00f, 0.80f, 0.60f), vector), Vector3.Dot(new Vector3(-0.80f, 0.36f, -0.48f), vector), Vector3.Dot(new Vector3(-0.60f, -0.48f, 0.64f), vector));
        }
    }
}
