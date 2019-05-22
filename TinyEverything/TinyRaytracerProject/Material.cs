using System.Numerics;

namespace TinyEverything.TinyRaytracerProject
{
    public struct Material
    {
        public Vector3 DiffuseColor { get; set; }
        public Vector4 Albedo { get; }
        public float SpecularExponent { get; }
        public float RefractiveIndex { get; }

        public Material(Vector3 diffuseColor, Vector4 albedo, float specularExponent = 0, float refractiveIndex = 1)
        {
            DiffuseColor = diffuseColor;
            Albedo = albedo;
            SpecularExponent = specularExponent;
            RefractiveIndex = refractiveIndex;
        }
    }
}