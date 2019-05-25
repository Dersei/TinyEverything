using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using StbSharp;

namespace TinyEverything.TinyRaycasterProject
{
    public class TinyRaycaster
    {
        private bool LoadTexture(string filename, out List<uint> texture, out int textSize, out int textCount)
        {
            ImageReader loader = new ImageReader();
            using Stream stream = File.Open(filename, FileMode.Open);
            Image image = loader.Read(stream, StbImage.STBI_rgb_alpha);
            byte[] data = image.Data;

            textCount = image.Width / image.Height;
            textSize = image.Width / textCount;
            texture = Enumerable.Repeat(0u, image.Width * image.Height).ToList();
            for (int j = 0; j < image.Height; j++)
            {
                for (int i = 0; i < image.Width; i++)
                {
                    byte r = data[(i + j * image.Width) * 4 + 0];
                    byte g = data[(i + j * image.Width) * 4 + 1];
                    byte b = data[(i + j * image.Width) * 4 + 2];
                    byte a = data[(i + j * image.Width) * 4 + 3];
                    texture[i + j * image.Width] = PackColor(r, g, b, a);
                }
            }
            return true;
        }

        List<uint> TextureColumn(List<uint> img, int texsize, int ntextures, int texid, int texcoord, int column_height)
        {
            int img_w = texsize * ntextures;
            int img_h = texsize;

            List<uint> column = Enumerable.Repeat(0u, column_height).ToList();
            for (int y = 0; y < column_height; y++)
            {
                int pix_x = texid * texsize + texcoord;
                int pix_y = (y * texsize) / column_height;
                column[y] = img[pix_x + pix_y * img_w];
            }
            return column;
        }

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
                    if (cx >= imgW || cy >= imgH) continue;
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
        "0   3   11100  0" +
        "5   4   0      0" +
        "5   4   1  00000" +
        "0       1      0" +
        "2       1      0" +
        "0       0      0" +
        "0 0000000      0" +
        "0              0" +
        "0002222222200000").ToCharArray();

        private readonly Random _random = new Random();
        private readonly string _directoryName = $"dir-{DateTime.Now:yyyy-dd-M--HH-mm-ss.fff}";
        public void Run()
        {
            LoadTexture("Resources/walltext.png", out var walltext, out var walltext_size, out var walltext_count);

            Directory.CreateDirectory(_directoryName);
            const int width = 1024; // image width
            const int height = 512; // image height

            var player_x = 3.456f; // player x position
            var player_y = 2.345f; // player y position
            var player_a = 1.523f;
            const float fov = MathF.PI / 3.0f;

            const int ncolors = 10;
            uint[] colors = new uint[ncolors];
            for (int i = 0; i < ncolors; i++)
            {
                colors[i] = PackColor((byte)_random.Next(0, 255), (byte)_random.Next(0, 255), (byte)_random.Next(0, 255));
            }

            const int rectW = width / (MapWidth * 2);
            const int rectH = height / MapHeight;
            for (int frame = 0; frame < 360; frame++)
            {
                player_a += 2 * MathF.PI / 360f;
                var framebuffer = Enumerable.Repeat(PackColor(255, 255, 255), height * width).ToList();

                for (var j = 0; j < MapHeight; j++)
                { // draw the map
                    for (var i = 0; i < MapWidth; i++)
                    {
                        if (_map[i + j * MapWidth] == ' ') continue; // skip empty spaces
                        var rectX = i * rectW;
                        var rectY = j * rectH;
                        var texid = _map[i + j * MapWidth] - '0';
                        Debug.Assert(texid < walltext_count);
                        DrawRectangle(framebuffer, width, height, rectX, rectY, rectW, rectH, walltext[texid * walltext_size]);
                    }
                }

                for (int i = 0; i < width / 2; i++)
                {
                    // draw the visibility cone AND the "3D" view
                    float angle = player_a - fov / 2 + fov * i / ((float)width / 2);
                    for (float t = 0; t < 20; t += 0.01f)
                    {
                        float cx = player_x + t * MathF.Cos(angle);
                        float cy = player_y + t * MathF.Sin(angle);

                        int pix_x = (int)(cx * rectW);
                        int pix_y = (int)(cy * rectH);
                        framebuffer[pix_x + pix_y * width] = PackColor(160, 160, 160); // this draws the visibility cone

                        if (_map[(int)(cx) + (int)(cy) * MapWidth] != ' ')
                        {
                            int texid = _map[(int)cx + (int)cy * MapWidth] - '0';
                            Debug.Assert(texid < walltext_count);
                            // our ray touches a wall, so draw the vertical column to create an illusion of 3D
                            int column_height = (int)(height / (t * MathF.Cos(angle - player_a)));
                            float hitx = cx - MathF.Floor(cx + 0.5f); // hitx and hity contain (signed) fractional parts of cx and cy,
                            float hity = cy - MathF.Floor(cy + 0.5f); // they vary between -0.5 and +0.5, and one of them is supposed to be very close to 0
                            int x_texcoord = (int)(hitx * walltext_size);
                            if (MathF.Abs(hity) > MathF.Abs(hitx))
                            {
                                x_texcoord = (int)(hity * walltext_size);
                            }

                            if (x_texcoord < 0) x_texcoord += walltext_size; // do not forget x_texcoord can be negative, fix that
                            Debug.Assert(x_texcoord >= 0 && x_texcoord < (int)walltext_size);

                            List<uint> column = TextureColumn(walltext, walltext_size, walltext_count, texid, x_texcoord, column_height);
                            pix_x = width / 2 + i;
                            for (var j = 0; j < column_height; j++)
                            {
                                pix_y = j + height / 2 - column_height / 2;
                                if (pix_y < 0 || pix_y >= (int)height) continue;
                                framebuffer[pix_x + pix_y * width] = column[j];
                            }
                            break;
                        }
                    }
                }

                var fileName = $"{frame}.ppm";

                Save(fileName, height, width, framebuffer);
            }


        }

        public void Save(string fileName, int height, int width, List<uint> data)
        {
            using var fileStream = File.Open($"{_directoryName}\\{fileName}", FileMode.CreateNew, FileAccess.Write);
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
