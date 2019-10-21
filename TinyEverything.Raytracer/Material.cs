using System.Numerics;

namespace TinyEverything.Raytracer
{
    public readonly struct Material
    {
        public readonly Vector3 DiffuseColor;
        public readonly Vector4 Albedo;
        public readonly float SpecularExponent;
        public readonly float RefractiveIndex;

        public Material(Vector3 diffuseColor)
        {
            DiffuseColor = diffuseColor;
            Albedo = new Vector4(1, 0, 0, 0);
            SpecularExponent = 0;
            RefractiveIndex = 1;
        }

        public Material(Vector3 diffuseColor, Vector4 albedo, float specularExponent = 0, float refractiveIndex = 1)
        {
            DiffuseColor = diffuseColor;
            Albedo = albedo;
            SpecularExponent = specularExponent;
            RefractiveIndex = refractiveIndex;
        }
    }
}