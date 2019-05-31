using System.Collections.Generic;
using System.Linq;

namespace TinyEverything.Common
{
    public class Framebuffer<T> where T : struct
    {
        private T[] _data;
        public readonly int Width;
        public readonly int Height;
        public int Count => _data.Length;

        public Framebuffer(int width, int height, T value)
        {
            Width = width;
            Height = height;
            _data = Enumerable.Repeat(value, width * height).ToArray();
        }

        public void Clear(T value) => _data = Enumerable.Repeat(value, Width * Height).ToArray();

        public T this[int index]
        {
            get => _data[index];
            set => _data[index] = value;
        }

        public void SetPixel(int x, int y, T color)
        {
            _data[x + y * Width] = color;
        }

        public void DrawRectangle(int x, int y, int width, int height, T color)
        {
            for (var i = 0; i < width; i++)
            {
                for (var j = 0; j < height; j++)
                {
                    var cx = x + i;
                    var cy = y + j;
                    if (cx < Width && cy < Height) // no need to check for negative values (unsigned variables)
                        SetPixel(cx, cy, color);
                }
            }
        }

        public T[] GetData()
        {
            return _data;
        }
    }
}
