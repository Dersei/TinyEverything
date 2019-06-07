using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Numerics;
using TinyEverything.Common;

namespace TinyEverything.TinyRendererProject
{
    public class TinyRenderer
    {
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

        private Vector3 Barycentric(Vertex2[] pts, Vertex2 p)
        {
            Vector3 u = Vector3.Cross(new Vector3(pts[2].X - pts[0].X, pts[1].X - pts[0].X, pts[0].X - p.X),
                new Vector3(pts[2].Y - pts[0].Y, pts[1].Y - pts[0].Y, pts[0].Y - p.Y));
            /* `pts` and `P` has integer value as coordinates
               so `abs(u[2])` < 1 means `u[2]` is 0, that means
               triangle is degenerate, in this case return something with negative coordinates */
            if (MathF.Abs(u.Z) < 1) return new Vector3(-1, 1, 1);
            return new Vector3(1.0f - (u.X + u.Y) / u.Z, u.Y / u.Z, u.X / u.Z);
        }

        void Triangle(Vertex2[] pts, TGAImage image, TGAColor color)
        {
            var bboxmin = new Vertex2(image.Width - 1, image.Height - 1);
            var bboxmax = new Vertex2(0, 0);
            var clamp = new Vertex2(image.Width - 1, image.Height - 1);
            for (var i = 0; i < 3; i++)
            {
                bboxmin.X = Math.Max(0, Math.Min(bboxmin.X, pts[i].X));
                bboxmax.X = Math.Min(clamp.X, Math.Max(bboxmax.X, pts[i].X));
                bboxmin.Y = Math.Max(0, Math.Min(bboxmin.Y, pts[i].Y));
                bboxmax.Y = Math.Min(clamp.Y, Math.Max(bboxmax.Y, pts[i].Y));

            }
            Vertex2 P = default;
            for (P.X = bboxmin.X; P.X <= bboxmax.X; P.X++)
            {
                for (P.Y = bboxmin.Y; P.Y <= bboxmax.Y; P.Y++)
                {
                    var bcScreen = Barycentric(pts, P);
                    if (bcScreen.X < 0 || bcScreen.Y < 0 || bcScreen.Z < 0) continue;
                    image[P.X, P.Y] = color;
                }
            }
        }

        private static Random _random = new Random();
        public void Run()
        {
            const int width = 500;
            const int height = 500;
            var image = new TGAImage(width, height, Format.BGR);
            var model = new Model("Resources/african_head.obj");
            Vector3 light_dir = new Vector3(0, 0, -1);
            Vertex2[] pts = { new Vertex2(10, 10), new Vertex2(100, 30), new Vertex2(190, 160) };
            var sw = Stopwatch.StartNew();
            //Triangle(pts, image, new TGAColor(255, 0, 0));
            for (int i = 0; i < model.Faces.Count; i++)
            {
                int[] face = model.Faces[i];
                Vertex2[] screen_coords = new Vertex2[3];
                Vector3[] world_coords = new Vector3[3];
                for (int j = 0; j < 3; j++)
                {
                    Vector3 v = model.Vertices[face[j]];
                    screen_coords[j] = new Vertex2((int)((v.X + 1.0f) * width / 2.0f), (int)((v.Y + 1.0f) * height / 2.0f));
                    world_coords[j] = v;
                }

                Vector3 n = Vector3.Cross((world_coords[2] - world_coords[0]), (world_coords[1] - world_coords[0])).Normalize();
                float intensity = Vector3.Dot(n, light_dir);
                if (intensity > 0)
                {
                    Triangle(screen_coords, image, new TGAColor((byte) (intensity*255), (byte) (intensity*255), (byte) (intensity*255), 255));
                }
            }
            sw.Stop();
            Console.WriteLine(sw.Elapsed);
            image.VerticalFlip(); // i want to have the origin at the left bottom corner of the image
            image.WriteToFile("output.tga");
        }
    }
}
