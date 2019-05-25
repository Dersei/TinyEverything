using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace TinyEverything.TinyRaycasterProject
{
    public class TinyRaycaster
    {
        private uint PackColor(byte r, byte g, byte b, byte a = 255)
        {
            return (((uint)a << 24) + ((uint)b << 16) + ((uint)g << 8) + r);
        }

        private void UnpackColor(uint color, out byte r, out byte g, out byte b, out byte a)
        {
            r = (byte)((color >> 0) & 255);
            g = (byte)((color >> 8) & 255);
            b = (byte)((color >> 16) & 255);
            a = (byte)((color >> 24) & 255);
        }

        private void DrawRectangle(List<uint> img, int imgW, int imgH, int x, int y, int w, int h, uint color)
        {
            Debug.Assert(img.Count == imgW * imgH);
            for (var i = 0; i < w; i++)
            {
                for (var j = 0; j < h; j++)
                {
                    var cx = x + i;
                    var cy = y + j;
                    Debug.Assert(cx < imgW && cy < imgH);
                    img[cx + cy * imgW] = color;
                }
            }
        }

        private const int MapWidth = 16; // map width
        private const int MapHeight = 16; // map height

        private readonly char[] _map = ("0000222222220000" +
        "1              0" +
        "1      11111   0" +
        "1     0        0" +
        "0     0  1110000" +
        "0     3        0" +
        "0   10000      0" +
        "0   0   11100  0" +
        "0   0   0      0" +
        "0   0   1  00000" +
        "0       1      0" +
        "2       1      0" +
        "0       0      0" +
        "0 0000000      0" +
        "0              0" +
        "0002222222200000").ToCharArray();

        public void Run()
        {
            const int width = 512; // image width
            const int height = 512; // image height
            var framebuffer = Enumerable.Repeat(255u, height * width).ToList();

            var player_x = 3.456f; // player x position
            var player_y = 2.345f; // player y position
            var player_a = 1.523f;
            const float fov = MathF.PI / 3.0f;

            for (var j = 0; j < height; j++)
            { // fill the screen with color gradients
                for (var i = 0; i < width; i++)
                {
                    var r = (byte)(255 * (j / (float)height)); // varies between 0 and 255 as j sweeps the vertical
                    var g = (byte)(255 * (i / (float)width)); // varies between 0 and 255 as i sweeps the horizontal
                    byte b = 0;
                    framebuffer[i + j * width] = PackColor(r, g, b);
                }
            }

            const int rectW = width / MapWidth;
            const int rectH = height / MapHeight;
            for (var j = 0; j < MapHeight; j++)
            { // draw the map
                for (var i = 0; i < MapWidth; i++)
                {
                    if (_map[i + j * MapWidth] == ' ') continue; // skip empty spaces
                    var rectX = i * rectW;
                    var rectY = j * rectH;
                    DrawRectangle(framebuffer, width, height, rectX, rectY, rectW, rectH, PackColor(0, 255, 255));
                }
            }

            DrawRectangle(framebuffer, width, height, (int)(player_x * rectW), (int)(player_y * rectH), 5, 5, PackColor(255, 255, 255));

            for (int i = 0; i < width; i++)
            { // draw the visibility cone
                float angle = player_a - fov / 2 + fov * i / (float)width;

                for (float t = 0; t < 20; t += 0.05f)
                {
                    float cx = player_x + t * MathF.Cos(angle);
                    float cy = player_y + t * MathF.Sin(angle);
                    if (_map[(int)cx + (int)cy * MapWidth] != ' ') break;

                    int pix_x = (int)(cx * rectW);
                    int pix_y = (int)(cy * rectH);
                    framebuffer[pix_x + pix_y * width] = PackColor(255, 255, 255);
                }
            }

            Save(height, width, framebuffer);
        }

        public void Save(int height, int width, List<uint> data)
        {
            var fileName = $"output-raycaster{DateTime.Now:yyyy-dd-M--HH-mm-ss.fff}.ppm";

            using var fileStream = File.Open(fileName, FileMode.CreateNew, FileAccess.Write);
            using var writer = new BinaryWriter(fileStream, Encoding.ASCII);
            writer.Write(Encoding.ASCII.GetBytes($"P6 {width} {height} 255 ")); // trailing space!!!

            for (var i = 0; i < height * width; ++i)
            {
                UnpackColor(data[i], out var r, out var g, out var b, out var a);
                writer.Write(r);
                writer.Write(g);
                writer.Write(b);
                //writer.Write(a);
            }
        }
    }
}
