using System.Numerics;

namespace TinyEverything.TinyRaytracerProject
{
    public struct Light
    {
        public Vector3 Position { get; }
        public float Intensity { get; }

        public Light(Vector3 position, float intensity)
        {
            Position = position;
            Intensity = intensity;
        }
    }
}