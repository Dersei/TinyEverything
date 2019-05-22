using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using TinyEverything.Common;

namespace TinyEverything.TinyRaytracerProject
{
    public class TinyRaytracer
    {
        private Vector3 Reflect(Vector3 first, Vector3 second)
        {
            return first - second * 2.0f * Vector3.Dot(first, second);
        }

        Vector3 Refract(Vector3 I, Vector3 n, float refractiveIndex)
        { // Snell's law
            var cosi = -MathF.Max(-1.0f, MathF.Min(1.0f, Vector3.Dot(I, n)));
            var etai = 1f;
            var etat = refractiveIndex;

            if (cosi < 0)
            { // if the ray is inside the object, swap the indices and invert the normal to get the correct result
                cosi = -cosi;
                (etai, etat) = (etat, etai);
                n = -n;
            }
            var eta = etai / etat;
            var k = 1 - eta * eta * (1 - cosi * cosi);
            return k < 0 ? new Vector3(1, 0, 0) : I * eta + n * (eta * cosi - MathF.Sqrt(k));
        }
        private bool IsSceneIntersecting(Vector3 orig, Vector3 dir, List<Sphere> spheres, ref Vector3 hit, ref Vector3 n, ref Material material)
        {
            var spheresDist = float.MaxValue;
            for (var i = 0; i < spheres.Count; i++)
            {
                float distI = 0;
                if (spheres[i].IsRayIntersecting(orig, dir, ref distI) && distI < spheresDist)
                {
                    spheresDist = distI;
                    hit = orig + dir * distI;
                    n = (hit - spheres[i].Center).Normalize();
                    material = spheres[i].Material;
                }
            }

            var checkerboardDist = float.MaxValue;
            if (MathF.Abs(dir.Y) > 1e-3f)
            {
                var d = -(orig.Y + 4) / dir.Y; // the checkerboard plane has equation y = -4
                var pt = orig + dir * d;
                if (d > 0 && MathF.Abs(pt.X) < 10 && pt.Z < -10 && pt.Z > -30 && d < spheresDist)
                {
                    checkerboardDist = d;
                    hit = pt;
                    n = new Vector3(0, 1, 0);
                    material.DiffuseColor = (((int)(0.5f * hit.X + 1000) + (int)(0.5f * hit.Z)) & 1) == 1 ? new Vector3(.3f, .3f, .3f) : new Vector3(.3f, .2f, .1f);
                }
            }
            return MathF.Min(spheresDist, checkerboardDist) < 1000;
        }


        private Vector3 CastRay(Vector3 orig, Vector3 dir, List<Sphere> spheres, List<Light> lights, float depth = 0)
        {
            Vector3 point = default;
            Vector3 n = default;
            var material = new Material(Vector3.Zero, new Vector4(1, 0, 0, 0));

            if (depth > 4 || !IsSceneIntersecting(orig, dir, spheres, ref point, ref n, ref material))
            {
                return new Vector3(0.2f, 0.7f, 0.8f); // background color
            }

            var reflectDir = Reflect(dir, n).Normalize();
            var refractDir = Refract(dir, n, material.RefractiveIndex).Normalize();
            var reflectOrig = Vector3.Dot(reflectDir, n) < 0 ? point - n * 1e-3f : point + n * 1e-3f;
            var refractOrig = Vector3.Dot(refractDir, n) < 0 ? point - n * 1e-3f : point + n * 1e-3f;
            var reflectColor = CastRay(reflectOrig, reflectDir, spheres, lights, depth + 1);
            var refractColor = CastRay(refractOrig, refractDir, spheres, lights, depth + 1);

            var diffuseLightIntensity = 0f;
            var specularLightIntensity = 0f;

            for (var i = 0; i < lights.Count; i++)
            {
                var lightDir = (lights[i].Position - point).Normalize();
                var lightDistance = (lights[i].Position - point).Length();
                var shadowOrig = Vector3.Dot(lightDir, n) < 0 ? point - n * 1e-3f : point + n * 1e-3f;
                Vector3 shadowPt = default;
                Vector3 shadowN = default;
                Material tempMaterial = default;
                if (IsSceneIntersecting(shadowOrig, lightDir, spheres, ref shadowPt, ref shadowN, ref tempMaterial) &&
                    (shadowPt - shadowOrig).Length() < lightDistance)
                {
                    continue;
                }
                diffuseLightIntensity += lights[i].Intensity * MathF.Max(0.0f, Vector3.Dot(lightDir, n));
                specularLightIntensity += MathF.Pow(MathF.Max(0.0f, Vector3.Dot(-Reflect(-lightDir, n), dir)), material.SpecularExponent) * lights[i].Intensity;
            }
            return material.DiffuseColor * diffuseLightIntensity * material.Albedo.X + Vector3.One * specularLightIntensity * material.Albedo.Y + reflectColor * material.Albedo.Z + refractColor * material.Albedo.W;
        }

        public void Run()
        {
            const int width = 1024;
            const int height = 768;
            const int fov = (int)(MathF.PI / 2.0f);
            var framebuffer = Enumerable.Repeat(default(Vector3), height * width).ToList();
            var ivory = new Material(new Vector3(0.4f, 0.4f, 0.3f), new Vector4(0.6f, 0.3f, 0.1f, 0f), 50f);
            var glass = new Material(new Vector3(0.6f, 0.7f, 0.8f), new Vector4(0f, 0.5f, 0.1f, 0.8f), 125f, 1.5f);
            var redRubber = new Material(new Vector3(0.3f, 0.1f, 0.1f), new Vector4(0.9f, 0.1f, 0.0f, 0f), 10f);
            var mirror = new Material(Vector3.One, new Vector4(0.0f, 10.0f, 0.8f, 0f), 1425f);
            var spheres = new List<Sphere>()
            {
                new Sphere(new Vector3(-3,0,-16), 2, ivory),
                new Sphere(new Vector3(-1,-1.5f,-12), 2, glass),
                new Sphere(new Vector3(1.5f,-0.5f,-18), 3, redRubber),
                new Sphere(new Vector3(7,5,-18), 4, mirror),
            };
            var lights = new List<Light>
            {
                new Light(new Vector3(-20,20,20), 1.5f),
                new Light(new Vector3(-30,50,-25), 1.8f),
                new Light(new Vector3(30,20,30), 1.7f)
            };

            Parallel.For(0, height, j =>
            {
                for (var i = 0; i < width; i++)
                {
                    var x = (2 * (i + 0.5f) / width - 1) * MathF.Tan(fov / 2.0f) * width / height;
                    var y = -(2 * (j + 0.5f) / height - 1) * MathF.Tan(fov / 2.0f);
                    var dir = new Vector3(x, y, -1).Normalize();
                    framebuffer[i + j * width] = CastRay(Vector3.Zero, dir, spheres, lights);
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
