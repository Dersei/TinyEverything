using System;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using TinyEverything.Common;
using static TinyEverything.Common.Utilities;

namespace TinyEverything.TinyKaboomProject
{
    internal class TinyKaboom
    {
        public float SphereRadius { set; get; }
        public float NoiseAmplitude { set; get; }
        public NoiseGenerator NoiseGenerator { get; set; } = new NoiseGenerator();

        public TinyKaboom(float sphereRadius = 1.5f, float noiseAmplitude = 1.0f)
        {
            SphereRadius = sphereRadius;
            NoiseAmplitude = noiseAmplitude;
        }

        private float SignedDistance(Vector3 p)
        {
            var displacement = -NoiseGenerator.FractalBrownianMotion(p * 3.4f) * NoiseAmplitude;
            return p.Length() - (SphereRadius + displacement);
        }

        private Vector3 DistanceFieldNormal(Vector3 pos)
        {
            const float eps = 0.1f;
            var d = SignedDistance(pos);
            var nx = SignedDistance(pos + new Vector3(eps, 0, 0)) - d;
            var ny = SignedDistance(pos + new Vector3(0, eps, 0)) - d;
            var nz = SignedDistance(pos + new Vector3(0, 0, eps)) - d;
            return new Vector3(nx, ny, nz).Normalize();
        }

        private static Vector3 FirePalette(float d)
        {
            var yellow = new Vector3(1.7f, 1.3f, 1.0f); // note that the color is "hot"f, i.e. has components >1
            var orange = new Vector3(1.0f, 0.6f, 0.0f);
            var red = new Vector3(1.0f, 0.0f, 0.0f);
            var darkGray = new Vector3(0.2f, 0.2f, 0.2f);
            var gray = new Vector3(0.4f, 0.4f, 0.4f);

            var x = MathF.Max(0.0f, MathF.Min(1.0f, d));
            if (x < .25f)
                return Lerp(gray, darkGray, x * 4.0f);
            if (x < .5f)
                return Lerp(darkGray, red, x * 4.0f - 1.0f);
            if (x < .75f)
                return Lerp(red, orange, x * 4.0f - 2.0f);

            return Lerp(orange, yellow, x * 4.0f - 3.0f);
        }

        private bool SphereTrace(Vector3 orig, Vector3 dir, out Vector3 pos)
        {
            if (Floatify(orig, orig) - MathF.Pow(Floatify(orig, dir), 2) > MathF.Pow(SphereRadius, 2))
            {
                pos = default;
                return false;
            }
            pos = orig;
            for (var i = 0; i < 128; i++)
            {
                var d = SignedDistance(pos);
                if (d < 0) return true;
                pos += dir * MathF.Max(d * 0.1f, .01f);
            }
            return false;
        }

        public void Run()
        {
            const int width = 640;
            const int height = 480;
            const float fov = MathF.PI / 3.0f;
            var framebuffer = Enumerable.Repeat(default(Vector3), height * width).ToList();

            Parallel.For(0, height, j =>
             {
                 for (var i = 0; i < width; i++)
                 {
                     var dirX = (i + 0.5f) - width / 2.0f;
                     var dirY = -(j + 0.5f) + height / 2.0f;    // this flips the image at the same time
                     var dirZ = -height / (2.0f * MathF.Tan(fov / 2.0f));

                     if (SphereTrace(new Vector3(0, 0, 3), new Vector3(dirX, dirY, dirZ).Normalize(), out var hit))
                     { // the camera is placed to (0,0,3) and it looks along the -z axis
                         var noiseLevel = (SphereRadius - hit.Length()) / NoiseAmplitude;
                         var lightDir = (new Vector3(10, 10, 10) - hit).Normalize();
                         var lightIntensity = MathF.Max(0.4f, Floatify(lightDir, DistanceFieldNormal(hit)));
                         framebuffer[i + j * width] = FirePalette((-0.2f + noiseLevel) * 2) * lightIntensity;
                     }
                     else
                     {
                         framebuffer[i + j * width] = new Vector3(0.2f, 0.7f, 0.8f); // background color
                     }
                 }
             });

            var fileName = $"output-C#{DateTime.Now:yyyy-dd-M--HH-mm-ss.fff}.ppm";
            using var fileStream = File.Open(fileName, FileMode.CreateNew, FileAccess.Write);
            using var writer = new BinaryWriter(fileStream, Encoding.ASCII);
            writer.Write(Encoding.ASCII.GetBytes($"P6 {width} {height} 255 ")); // trailing space!!!

            for (var i = 0; i < height * width; ++i)
            {
                writer.Write((byte)(MathF.Max(0, MathF.Min(255, (int)(255 * framebuffer[i].X)))));
                writer.Write((byte)(MathF.Max(0, MathF.Min(255, (int)(255 * framebuffer[i].Y)))));
                writer.Write((byte)(MathF.Max(0, MathF.Min(255, (int)(255 * framebuffer[i].Z)))));
            }
        }
    }
}
