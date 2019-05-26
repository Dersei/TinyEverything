using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using TinyEverything.Common;

namespace TinyEverything.TinyRaytracerProject
{
    public class TinyRaytracer
    {
        public List<Sphere> Spheres { get; set; } = new List<Sphere>();
        public List<Light> Lights { get; set; } = new List<Light>();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private Vector3 Reflect(Vector3 first, Vector3 second)
        {
            return first - second * 2.0f * Vector3.Dot(first, second);
        }

        private Vector3 Refract(Vector3 origin, Vector3 n, float refractiveIndex)
        { // Snell's law
            var cosOrigin = -MathF.Max(-1.0f, MathF.Min(1.0f, Vector3.Dot(origin, n)));
            var etaI = 1f;
            var etaT = refractiveIndex;

            if (cosOrigin < 0)
            { // if the ray is inside the object, swap the indices and invert the normal to get the correct result
                cosOrigin = -cosOrigin;
                (etaI, etaT) = (etaT, etaI);
                n = -n;
            }
            var eta = etaI / etaT;
            var k = 1 - eta * eta * (1 - cosOrigin * cosOrigin);
            return k < 0 ? new Vector3(1, 0, 0) : origin * eta + n * (eta * cosOrigin - MathF.Sqrt(k));
        }

        private bool IsSceneIntersecting(Vector3 origin, Vector3 direction, out Vector3 hit, out Vector3 n, out Material material)
        {
            material = default;
            hit = default;
            n = default;

            var spheresDist = float.MaxValue;
            for (var i = 0; i < Spheres.Count; i++)
            {
                float distI = 0;
                if (Spheres[i].IsRayIntersecting(origin, direction, ref distI) && distI < spheresDist)
                {
                    spheresDist = distI;
                    hit = origin + direction * distI;
                    n = (hit - Spheres[i].Center).Normalize();
                    material = Spheres[i].Material;
                }
            }

            var checkerboardDist = float.MaxValue;
            if (MathF.Abs(direction.Y) > 1e-3f)
            {
                var d = -(origin.Y + 4) / direction.Y; // the checkerboard plane has equation y = -4
                var pt = origin + direction * d;
                if (d > 0 && MathF.Abs(pt.X) < 10 && pt.Z < -10 && pt.Z > -30 && d < spheresDist)
                {
                    checkerboardDist = d;
                    hit = pt;
                    n = new Vector3(0, 1, 0);
                    var color = ((int)(0.5f * hit.X + 1000) + (int)(0.5f * hit.Z) & 1) == 1 ? new Vector3(.3f, .3f, .3f) : new Vector3(.3f, .2f, .1f);
                    material = new Material(color);
                }
            }
            return MathF.Min(spheresDist, checkerboardDist) < 1000;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private Vector3 Perturb(Vector3 target, Vector3 point, Vector3 n)
        {
            return Vector3.Dot(target, n) < 0 ? point - n * 1e-3f : point + n * 1e-3f;
        }

        private (float diffuseLightIntensity, float specularLightIntensity) CreateShadows(Vector3 point, Vector3 n, Vector3 direction, Material material)
        {
            var diffuseLightIntensity = 0f;
            var specularLightIntensity = 0f;

            for (var i = 0; i < Lights.Count; i++)
            {
                var lightDir = (Lights[i].Position - point).Normalize();
                var lightDistance = (Lights[i].Position - point).Length();
                var shadowOrig = Perturb(lightDir, point, n);

                if (IsSceneIntersecting(shadowOrig, lightDir, out var shadowPt, out _, out _) &&
                    (shadowPt - shadowOrig).Length() < lightDistance)
                {
                    continue;
                }
                diffuseLightIntensity += Lights[i].Intensity * MathF.Max(0.0f, Vector3.Dot(lightDir, n));
                specularLightIntensity += MathF.Pow(MathF.Max(0.0f, Vector3.Dot(-Reflect(-lightDir, n), direction)), material.SpecularExponent) * Lights[i].Intensity;
            }

            return (diffuseLightIntensity, specularLightIntensity);
        }

        private Vector3 CastRay(Vector3 origin, Vector3 direction, float depth = 0)
        {
            if (depth > 4 || !IsSceneIntersecting(origin, direction, out var point, out var n, out var material))
            {
                return new Vector3(0.2f, 0.7f, 0.8f); // background color
            }

            var reflectDir = Reflect(direction, n).Normalize();
            var reflectOrig = Perturb(reflectDir, point, n);
            var reflectColor = CastRay(reflectOrig, reflectDir, depth + 1);

            var refractDir = Refract(direction, n, material.RefractiveIndex).Normalize();
            var refractOrig = Perturb(refractDir, point, n);
            var refractColor = CastRay(refractOrig, refractDir, depth + 1);

            var (diffuseLightIntensity, specularLightIntensity) = CreateShadows(point, n, direction, material);

            return material.DiffuseColor * diffuseLightIntensity * material.Albedo.X
                   + Vector3.One * specularLightIntensity * material.Albedo.Y
                   + reflectColor * material.Albedo.Z
                   + refractColor * material.Albedo.W;
        }

        public TinyRaytracer DefaultSettings()
        {
            var ivory = new Material(new Vector3(0.4f, 0.4f, 0.3f), new Vector4(0.6f, 0.3f, 0.1f, 0f), 50f);
            var glass = new Material(new Vector3(0.6f, 0.7f, 0.8f), new Vector4(0f, 0.5f, 0.1f, 0.8f), 125f, 1.5f);
            var redRubber = new Material(new Vector3(0.3f, 0.1f, 0.1f), new Vector4(0.9f, 0.1f, 0.0f, 0f), 10f);
            var mirror = new Material(Vector3.One, new Vector4(0.0f, 10.0f, 0.8f, 0f), 1425f);

            Spheres = new List<Sphere>()
            {
                new Sphere(new Vector3(-3,0,-16), 2, ivory),
                new Sphere(new Vector3(-1,-1.5f,-12), 2, glass),
                new Sphere(new Vector3(1.5f,-0.5f,-18), 3, redRubber),
                new Sphere(new Vector3(7,5,-18), 4, mirror),
            };

            Lights = new List<Light>
            {
                new Light(new Vector3(-20,20,20), 1.5f),
                new Light(new Vector3(-30,50,-25), 1.8f),
                new Light(new Vector3(30,20,30), 1.7f)
            };

            return this;
        }

        public void Run()
        {
            const int width = 1024;
            const int height = 768;
            const int fov = (int)(MathF.PI / 2.0f);
            var framebuffer = new Framebuffer<Vector3>(width, height, default);

            Parallel.For(0, height, j =>
            {
                for (var i = 0; i < width; i++)
                {
                    var x = (2 * (i + 0.5f) / width - 1) * MathF.Tan(fov / 2.0f) * width / height;
                    var y = -(2 * (j + 0.5f) / height - 1) * MathF.Tan(fov / 2.0f);
                    var dir = new Vector3(x, y, -1).Normalize();
                    framebuffer[i + j * width] = CastRay(Vector3.Zero, dir);
                }
            });

            var fileName = $"output-raytracer{DateTime.Now:yyyy-dd-M--HH-mm-ss.fff}.ppm";
            var image = new Image(fileName)
            {
                Height = height,
                Width = width,
                Data = framebuffer
            };

            ImageSaver.Save(image);
        }
    }
}
