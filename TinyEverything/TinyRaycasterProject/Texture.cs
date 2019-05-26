using System.Collections.Generic;
using System.IO;
using System.Linq;
using StbSharp;
using TinyEverything.Common;

namespace TinyEverything.TinyRaycasterProject
{
    internal class Texture
    {
        public readonly int Width;
        public readonly int Height;
        public readonly int Count;
        public readonly int Size;

        public Framebuffer<uint> Data { get; }

        public Texture(string filename)
        {
            var loader = new ImageReader();
            using Stream stream = File.Open(filename, FileMode.Open);
            var image = loader.Read(stream, StbImage.STBI_rgb_alpha);
            var data = image.Data;

            Width = image.Width;
            Height = image.Height;
            Count = image.Width / image.Height;
            Size = image.Width / Count;
            Data = new Framebuffer<uint>(image.Width, image.Height, 0u);
            for (var j = 0; j < image.Height; j++)
            {
                for (var i = 0; i < image.Width; i++)
                {
                    var r = data[(i + j * image.Width) * 4 + 0];
                    var g = data[(i + j * image.Width) * 4 + 1];
                    var b = data[(i + j * image.Width) * 4 + 2];
                    var a = data[(i + j * image.Width) * 4 + 3];
                    Data[i + j * image.Width] = PackColor(r, g, b, a);
                }
            }
        }

        public uint this[int index] => Data[index];

        private uint PackColor(byte r, byte g, byte b, byte a = 255)
        {
            return (((uint)a << 24) + ((uint)b << 16) + ((uint)g << 8) + r);
        }

        public uint Get(int i, int j, int idx)
        {
            return Data[i + idx * Size + j * Width];
        }

        public List<uint> GetScaledColumn(int textureID, int textureCoord, int columnHeight)
        {
            var column = Enumerable.Repeat(0u, columnHeight).ToList();
            for (int y = 0; y < columnHeight; y++)
            {
                column[y] = Get(textureCoord, (y * Size) / columnHeight, textureID);
            }

            return column;
        }
    }
}
