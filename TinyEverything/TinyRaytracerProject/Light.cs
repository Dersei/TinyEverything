using System.Numerics;

namespace TinyEverything.TinyRaytracerProject
{
    public readonly struct Light
    {
        public readonly Vector3 Position;
        public readonly float Intensity;

        public Light(Vector3 position, float intensity)
        {
            Position = position;
            Intensity = intensity;
        }
    }
}