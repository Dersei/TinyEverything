using System.Numerics;

namespace TinyEverything.Common
{
    public readonly struct BaseSettings
    {
        public readonly int Width;
        public readonly int Height;
        public readonly Vector3 BackgroundColor;

        public BaseSettings(int width, int height, Vector3 backgroundColor)
        {
            Width = width;
            Height = height;
            BackgroundColor = backgroundColor;
        }

        public BaseSettings(int width, int height)
        {
            Width = width;
            Height = height;
            BackgroundColor = new Vector3(0.2f, 0.7f, 0.8f);
        }
    }
}