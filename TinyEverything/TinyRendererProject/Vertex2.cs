using System;

namespace TinyEverything.TinyRendererProject
{
    public struct Vertex2 : IEquatable<Vertex2>, IFormattable
    {
        public int X;
        public int Y;

        public Vertex2(int x, int y)
        {
            X = x;
            Y = y;
        }

        public static Vertex2 operator +(Vertex2 first, Vertex2 second)
        {
            first.X += second.X;
            first.Y += second.X;
            return first;
        }

        public static Vertex2 operator -(Vertex2 first, Vertex2 second)
        {
            first.X -= second.X;
            first.Y -= second.X;
            return first;
        }

        public static Vertex2 operator *(Vertex2 vertex, float value)
        {
            vertex.X = (int) (vertex.X * value);
            vertex.Y = (int) (vertex.Y * value);
            return vertex;
        }

        public static Vertex2 operator /(Vertex2 vertex, float value)
        {
            vertex.X = (int)(vertex.X / value);
            vertex.Y = (int)(vertex.Y / value);
            return vertex;
        }

        public bool Equals(Vertex2 other)
        {
            return other.X == X && other.Y == Y;
        }

        public string ToString(string? format, IFormatProvider? formatProvider)
        {
            return $"[{X}, {Y}]";
        }
    }
}