using System;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using TinyEverything.Common;

namespace TinyEverything.Renderer
{
    public class TinyRenderer
    {
        const int width = 800;
        const int height = 800;
        const int depth = 255;
        private void Line(int x0, int y0, int x1, int y1, TGAImage image, TGAColor color)
        {
            var steep = false;
            if (MathF.Abs(x0 - x1) < MathF.Abs(y0 - y1))
            {
                (x0, y0) = (y0, x0);
                (x1, y1) = (y1, x1);
                steep = true;
            }

            if (x0 > x1)
            {
                (x0, x1) = (x1, x0);
                (y0, y1) = (y1, y0);
            }

            var dx = x1 - x0;
            var dy = y1 - y0;
            var derror2 = Math.Abs(dy) * 2;
            var error2 = 0;
            var y = y0;
            for (var x = x0; x <= x1; x++)
            {
                if (steep)
                {
                    image[y, x] = color;
                }
                else
                {
                    image[x, y] = color;
                }

                error2 += derror2;
                if (error2 > dx)
                {
                    y += y1 > y0 ? 1 : -1;
                    error2 -= dx * 2;
                }
            }
        }

        Vector3 WorldToScreen(Vector3 v)
        {
            return new Vector3((int)((v.X + 1.0f) * width / 2.0f + .5), (int)((v.Y + 1.0f) * height / 2.0f + .5), v.Z);
        }

        private Vector3 Barycentric(Vector3 a, Vector3 b, Vector3 c, Vector3 p)
        {
            var s = new Vector3[2];
            s[1].X = c.Y - a.Y;
            s[1].Y = b.Y - a.Y;
            s[1].Z = a.Y - p.Y;
            s[0].X = c.X - a.X;
            s[0].Y = b.X - a.X;
            s[0].Z = a.X - p.X;
            var u = Vector3.Cross(s[0], s[1]);
            if (MathF.Abs(u.Z) > 1e-2f)
            {
                return new Vector3(1.0f - (u.X + u.Y) / u.Z, u.Y / u.Z, u.X / u.Z);
            }
            return new Vector3(-1, 1, 1);
        }

        void Triangle(Vector3[] pts, float[] zbuffer, TGAImage image, TGAColor color)
        {
            var bboxmin = new Vector2(float.MaxValue, float.MaxValue);
            var bboxmax = new Vector2(float.MinValue, float.MinValue);
            var clamp = new Vertex2(image.Width - 1, image.Height - 1);
            for (var i = 0; i < 3; i++)
            {
                bboxmin.X = Math.Max(0, Math.Min(bboxmin.X, pts[i].X));
                bboxmax.X = Math.Min(clamp.X, Math.Max(bboxmax.X, pts[i].X));
                bboxmin.Y = Math.Max(0, Math.Min(bboxmin.Y, pts[i].Y));
                bboxmax.Y = Math.Min(clamp.Y, Math.Max(bboxmax.Y, pts[i].Y));

            }
            Vector3 P = default;
            for (P.X = bboxmin.X; P.X <= bboxmax.X; P.X++)
            {
                for (P.Y = bboxmin.Y; P.Y <= bboxmax.Y; P.Y++)
                {
                    Vector3 bc_screen = Barycentric(pts[0], pts[1], pts[2], P);
                    if (bc_screen.X < 0 || bc_screen.Y < 0 || bc_screen.Z < 0) continue;
                    P.Z = 0;
                    P.Z += pts[0].Y * bc_screen.X;
                    P.Z += pts[1].Y * bc_screen.Y;
                    P.Z += pts[2].Y * bc_screen.Z;
                    if (zbuffer[(int)(P.X + P.Y * width)] < P.Z)
                    {
                        zbuffer[(int)(P.X + P.Y * width)] = P.Z;
                        image[(int)P.X, (int)P.Y] = color;
                    }
                }
            }
        }

        private void Swap<T>(ref T first, ref T second)
        {
            (first, second) = (second, first);
        }
        void Triangle(Vector3 t0, Vector3 t1, Vector3 t2, TGAImage image, TGAColor color, int[] zbuffer)
        {
            if (t0.Y == t1.Y && t0.Y == t2.Y) return; // i dont care about degenerate triangles
            if (t0.Y > t1.Y)
            {
                Swap(ref t0,ref t1);
            }

            if (t0.Y > t2.Y)
            {
                Swap(ref t0,ref t2);
            }

            if (t1.Y > t2.Y)
            {
                Swap(ref t1,ref t2);
            }
            int total_height = (int) (t2.Y - t0.Y);
            for (int i = 0; i < total_height; i++)
            {
                bool second_half = i > t1.Y - t0.Y || t1.Y == t0.Y;
                int segment_height = (int) (second_half ? t2.Y - t1.Y : t1.Y - t0.Y);
                float alpha = (float)i / total_height;
                float beta = (float)(i - (second_half ? t1.Y - t0.Y : 0)) / segment_height; // be careful: with above conditions no division by zero here
                Vector3 A = t0 + (t2 - t0) * alpha;
                Vector3 B = second_half ? t1 + (t2 - t1) * beta : t0 + (t1 - t0) * beta;
                if (A.X > B.X)
                {
                    Swap(ref A,ref B);
                }
                for (int j = (int) A.X; j <= B.X; j++)
                {
                    float phi = B.X == A.X ? 1.0f : (float)(j - A.X) / (float)(B.X - A.X);
                    Vector3 P = A + (B - A) * phi;
                    P.X = j; P.Y = t0.Y + i; // a hack to fill holes (due to int cast precision problems)
                    int idx = (int) (j + (t0.Y + i) * width);
                    if (zbuffer[idx] < P.Z)
                    {
                        zbuffer[idx] = (int) P.Z;
                        image[(int) P.X, (int) P.Y] = color; // attention, due to int casts t0.Y+i != A.Y
                    }
                }
            }
        }

        private static readonly Random Random = new Random();
        public void Run()
        {

            var image = new TGAImage(width, height, Format.BGR);
            var model = new Model("Resources/african_head.obj");
            var zbuffer = Enumerable.Repeat(int.MinValue, width * height).ToArray();
            Vector3 light_dir = new Vector3(0, 0, -1);
            var sw = Stopwatch.StartNew();
            //Triangle(pts, image, new TGAColor(255, 0, 0));
            for (int i = 0; i < model.Faces.Count; i++)
            {
                var face = model.Faces[i];
                Vector3[] screen_coords = new Vector3[3];
                Vector3[] world_coords = new Vector3[3];
                for (int j = 0; j < 3; j++)
                {
                    Vector3 v = model.Vertices[face[j]];
                    screen_coords[j] = new Vector3((v.X + 1.0f) * width / 2.0f, (v.Y + 1.0f) * height / 2.0f, (v.Z + 1.0f) * depth / 2.0f);
                    world_coords[j] = v;
                }
                Vector3 n = Vector3.Cross((world_coords[2] - world_coords[0]), (world_coords[1] - world_coords[0])).Normalize();
                float intensity = Vector3.Dot(n, light_dir);
                if (intensity > 0)
                {  Triangle(screen_coords[0], screen_coords[1], screen_coords[2], image, new TGAColor((byte) (intensity * 255), (byte) (intensity * 255), (byte) (intensity * 255), 255), zbuffer);
                }
            }
            TGAImage zbimage = new TGAImage(width, height, Format.Grayscale);
            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < height; j++)
                {
                    zbimage[i, j] = new TGAColor(zbuffer[i + j * width], Format.Grayscale);
                }
            }
            sw.Stop();
            zbimage.VerticalFlip(); // i want to have the origin at the left bottom corner of the image
            zbimage.WriteToFile("zbuffer.tga");

            Console.WriteLine(sw.Elapsed);
            image.VerticalFlip(); // i want to have the origin at the left bottom corner of the image
            image.WriteToFile("output.tga");
        }
    }
}
