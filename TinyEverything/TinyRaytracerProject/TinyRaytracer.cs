using System;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using TinyEverything.Common;

namespace TinyEverything.TinyRaytracerProject
{
    public class TinyRaytracer
    {
        private Vector3 CastRay(Vector3 orig, Vector3 dir, Sphere sphere)
        {
            var sphereDist = float.MaxValue;
            if (!sphere.IsRayIntersecting(orig, dir, ref sphereDist))
            {
                return new Vector3(0.2f, 0.7f, 0.8f); // background color
            }

            return new Vector3(0.4f, 0.4f, 0.3f);
        }

        public void Run()
        {
            const int width = 1024;
            const int height = 768;
            const int fov = (int)(MathF.PI / 2.0f);
            var framebuffer = Enumerable.Repeat(default(Vector3), height * width).ToList();
            var sphere = new Sphere(new Vector3(-3, 0, -16), 2);

            Parallel.For(0, height, j =>
            {
                for (var i = 0; i < width; i++)
                {
                    var x = (2 * (i + 0.5f) / width - 1) * MathF.Tan(fov / 2.0f) * width / height;
                    var y = -(2 * (j + 0.5f) / height - 1) * MathF.Tan(fov / 2.0f);
                    var dir = new Vector3(x, y, -1).Normalize();
                    framebuffer[i + j * width] = CastRay(Vector3.Zero, dir, sphere);
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
