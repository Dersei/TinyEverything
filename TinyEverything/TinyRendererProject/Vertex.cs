using System;

namespace TinyEverything.TinyRendererProject
{
    public readonly struct Vertex<T> : IEquatable<Vertex<T>>, IFormattable where T : struct
    {
        public readonly T X;
        public readonly T Y;
        public Vertex(T x, T y)
        {
            X = x;
            Y = y;
        }

        public T this[int index]
        {
            get
            {
                if (index == 0)
                {
                    return X;
                }
                if (index == 1)
                {
                    return Y;
                }
                throw new ArgumentOutOfRangeException(nameof(index));
            }
        }

        public static Vertex<T> operator -(Vertex<T> first, Vertex<T> second)
        {
            if (typeof(T) == typeof(int))
            {
                return new Vertex<T>((T)(ValueType)((int)(ValueType)first.X - (int)(ValueType)second.X), (T)(ValueType)((int)(ValueType)first.Y - (int)(ValueType)second.Y));
            }

            if (typeof(T) == typeof(float))
            {
                return new Vertex<T>((T)(ValueType)((float)(ValueType)first.X - (float)(ValueType)second.X), (T)(ValueType)((float)(ValueType)first.Y - (float)(ValueType)second.Y));
            }
            
            throw new NotSupportedException();
        }

        public static Vertex<T> operator +(Vertex<T> first, Vertex<T> second)
        {
            if (typeof(T) == typeof(int))
            {
                return new Vertex<T>((T)(ValueType)((int)(ValueType)first.X + (int)(ValueType)second.X), (T)(ValueType)((int)(ValueType)first.Y + (int)(ValueType)second.Y));
            }

            if (typeof(T) == typeof(float))
            {
                return new Vertex<T>((T)(ValueType)((float)(ValueType)first.X + (float)(ValueType)second.X), (T)(ValueType)((float)(ValueType)first.Y + (float)(ValueType)second.Y));
            }

            throw new NotSupportedException();
        }

        public static Vertex<T> operator *(Vertex<T> vertex, float value)
        {
            if (typeof(T) == typeof(int))
            {
                return new Vertex<T>((T)(ValueType)((int)(ValueType)vertex.X * (int)value), (T)(ValueType)((int)(ValueType)vertex.Y * (int)value));
            }

            if (typeof(T) == typeof(float))
            {
                return new Vertex<T>((T)(ValueType)((float)(ValueType)vertex.X * value), (T)(ValueType)((float)(ValueType)vertex.Y * value));
            }

            throw new NotSupportedException();
        }

        public static Vertex<T> operator /(Vertex<T> vertex, float value)
        {
            return vertex * (1 / value);
        }

        public bool Equals(Vertex<T> other)
        {
            return other.X.Equals(X) && other.Y.Equals(Y);
        }

        public string ToString(string? format, IFormatProvider? formatProvider = null)
        {
            return $"[{X}, {Y}] {typeof(T)}";
        }
    }
}