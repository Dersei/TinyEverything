using System;
using System.IO;
using System.Text;

namespace TinyEverything.Common
{
    internal static class ImageSaver
    {
        public static void Save(Image image)
        {
            if (!image.CheckIfCorrect())
            {
                Console.WriteLine("Wrong image data or size");
                return;
            }

            using var fileStream = File.Open(image.Name, FileMode.CreateNew, FileAccess.Write);
            using var writer = new BinaryWriter(fileStream, Encoding.ASCII);
            writer.Write(Encoding.ASCII.GetBytes($"P6 {image.Width} {image.Height} 255 ")); // trailing space!!!

            for (var i = 0; i < image.Height * image.Width; ++i)
            {
                var c = image.Data![i];
                var max = MathF.Max(c.X, MathF.Max(c.Y, c.Z));
                if (max > 1)
                {
                    image.Data![i] = c * (1.0f / max);
                }
                writer.Write((byte)(MathF.Max(0, MathF.Min(255, (int)(255 * image.Data![i].X)))));
                writer.Write((byte)(MathF.Max(0, MathF.Min(255, (int)(255 * image.Data[i].Y)))));
                writer.Write((byte)(MathF.Max(0, MathF.Min(255, (int)(255 * image.Data[i].Z)))));
            }
        }
    }
}
