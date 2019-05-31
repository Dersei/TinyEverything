using System;
using System.Runtime.CompilerServices;

namespace TinyEverything.Common
{
    public class Framebuffer<T> where T : struct
    {
        public T[] Data { get; }
        public readonly int Width;
        public readonly int Height;
        public int Count => Data.Length;

        public Framebuffer(int width, int height, T value)
        {
            Width = width;
            Height = height;
            Data = new T[width * height];
            Array.Fill(Data, value);
        }

        public void Clear(T value) => Array.Fill(Data, value);

        public T this[int index]
        {
            get => Data[index];
            set => Data[index] = value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetPixel(int x, int y, T color)
        {
            Data[x + y * Width] = color;
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
    }
}
