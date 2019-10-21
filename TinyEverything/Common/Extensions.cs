using System;

namespace TinyEverything.Common
{
    public static class Extensions
    {
        public static bool IsAbout(this float first, float second, float precisionMultiplier = 8)
        {
            const float epsilon = 1.175494E-38f;
            return MathF.Abs(second - first) < MathF.Max(1E-06f * MathF.Max(MathF.Abs(first), MathF.Abs(second)), epsilon * precisionMultiplier);
        }

        public static bool IsNotZero(this float value)
        {
            const float epsilon = 1.175494E-38f;
            return MathF.Abs(value) > epsilon;
        }

        public static bool Contains(this string source, string toCheck, System.StringComparison comp)
        {
            return source?.IndexOf(toCheck, comp) >= 0;
        }

    }
}
