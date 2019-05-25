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
        private void LoadTexture(string filename, out List<uint> texture, out int textSize, out int textCount)
        {
            var loader = new ImageReader();
            using Stream stream = File.Open(filename, FileMode.Open);
            var image = loader.Read(stream, StbImage.STBI_rgb_alpha);
            var data = image.Data;

            textCount = image.Width / image.Height;
            textSize = image.Width / textCount;
            texture = Enumerable.Repeat(0u, image.Width * image.Height).ToList();
            for (var j = 0; j < image.Height; j++)
            {
                for (var i = 0; i < image.Width; i++)
                {
                    var r = data[(i + j * image.Width) * 4 + 0];
                    var g = data[(i + j * image.Width) * 4 + 1];
                    var b = data[(i + j * image.Width) * 4 + 2];
                    var a = data[(i + j * image.Width) * 4 + 3];
                    texture[i + j * image.Width] = PackColor(r, g, b, a);
                }
            }
        }

        private List<uint> TextureColumn(List<uint> img, int textureSize, int nTextures, int textureId, int textureCoord, int columnHeight)
        {
            var imageWidth = textureSize * nTextures;
            var imageHeight = textureSize;

            var column = Enumerable.Repeat(0u, columnHeight).ToList();
            for (var y = 0; y < columnHeight; y++)
            {
                var pixX = textureId * textureSize + textureCoord;
                var pixY = (y * textureSize) / columnHeight;
                column[y] = img[pixX + pixY * imageWidth];
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
            LoadTexture("Resources/walltext.png", out var wallTexture, out var wallTextureSize, out var wallTextureCount);

            Directory.CreateDirectory(_directoryName);
            const int width = 1024; // image width
            const int height = 512; // image height

            const float playerX = 3.456f; // player x position
            const float playerY = 2.345f; // player y position
            var playerA = 1.523f;
            const float fov = MathF.PI / 3.0f;

            const int nColors = 10;
            var colors = new uint[nColors];
            for (var i = 0; i < nColors; i++)
            {
                colors[i] = PackColor((byte)_random.Next(0, 255), (byte)_random.Next(0, 255), (byte)_random.Next(0, 255));
            }

            const int rectW = width / (MapWidth * 2);
            const int rectH = height / MapHeight;
            for (var frame = 0; frame < 360; frame++)
            {
                playerA += 2 * MathF.PI / 360f;
                var framebuffer = Enumerable.Repeat(PackColor(255, 255, 255), height * width).ToList();

                for (var j = 0; j < MapHeight; j++)
                { // draw the map
                    for (var i = 0; i < MapWidth; i++)
                    {
                        if (_map[i + j * MapWidth] == ' ') continue; // skip empty spaces
                        var rectX = i * rectW;
                        var rectY = j * rectH;
                        var textureId = _map[i + j * MapWidth] - '0';
                        Debug.Assert(textureId < wallTextureCount);
                        DrawRectangle(framebuffer, width, height, rectX, rectY, rectW, rectH, wallTexture[textureId * wallTextureSize]);
                    }
                }

                for (var i = 0; i < width / 2; i++)
                {
                    // draw the visibility cone AND the "3D" view
                    var angle = playerA - fov / 2 + fov * i / ((float)width / 2);
                    for (float t = 0; t < 20; t += 0.01f)
                    {
                        var cx = playerX + t * MathF.Cos(angle);
                        var cy = playerY + t * MathF.Sin(angle);

                        var pixX = (int)(cx * rectW);
                        var pixY = (int)(cy * rectH);
                        framebuffer[pixX + pixY * width] = PackColor(160, 160, 160); // this draws the visibility cone

                        if (_map[(int)(cx) + (int)(cy) * MapWidth] != ' ')
                        {
                            var textureId = _map[(int)cx + (int)cy * MapWidth] - '0';
                            Debug.Assert(textureId < wallTextureCount);
                            // our ray touches a wall, so draw the vertical column to create an illusion of 3D
                            var columnHeight = (int)(height / (t * MathF.Cos(angle - playerA)));
                            var hitX = cx - MathF.Floor(cx + 0.5f); // hitx and hity contain (signed) fractional parts of cx and cy,
                            var hitY = cy - MathF.Floor(cy + 0.5f); // they vary between -0.5 and +0.5, and one of them is supposed to be very close to 0
                            var xTextureCoord = (int)(hitX * wallTextureSize);
                            if (MathF.Abs(hitY) > MathF.Abs(hitX))
                            {
                                xTextureCoord = (int)(hitY * wallTextureSize);
                            }

                            if (xTextureCoord < 0)
                            {
                                xTextureCoord += wallTextureSize; // do not forget x_texcoord can be negative, fix that
                            }
                            Debug.Assert(xTextureCoord >= 0 && xTextureCoord < wallTextureSize);

                            var column = TextureColumn(wallTexture, wallTextureSize, wallTextureCount, textureId, xTextureCoord, columnHeight);
                            pixX = width / 2 + i;
                            for (var j = 0; j < columnHeight; j++)
                            {
                                pixY = j + height / 2 - columnHeight / 2;
                                if (pixY < 0 || pixY >= height) continue;
                                framebuffer[pixX + pixY * width] = column[j];
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
