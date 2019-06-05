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

        public void Run()
        {
            const int width = 800;
            const int height = 800;
            var image = new TGAImage(width, height, Format.BGR);
            var model = new Model("Resources/african_head.obj");

            var sw = Stopwatch.StartNew();
            for (var i = 0; i < model.Faces.Count; i++)
            {
                var face = model.Faces[i];
                for (var j = 0; j < 3; j++)
                {
                    var v0 = model.Vertices[face[j]];
                    var v1 = model.Vertices[face[(j + 1) % 3]];
                    var x0 = (int)((v0.X + 1.0f) * width / 2.0f);
                    var y0 = (int)((v0.Y + 1.0f) * height / 2.0f);
                    var x1 = (int)((v1.X + 1.0f) * width / 2.0f);
                    var y1 = (int)((v1.Y + 1.0f) * height / 2.0f);
                    Line(x0, y0, x1, y1, image, TGAColor.White);
                }
            }
            sw.Stop();
            Console.WriteLine(sw.Elapsed);
            image.VerticalFlip(); // i want to have the origin at the left bottom corner of the image
            image.WriteToFile("output.tga");
        }
    }
}
