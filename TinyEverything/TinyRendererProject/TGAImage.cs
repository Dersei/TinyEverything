using System;
using System.IO;

namespace TinyEverything.TinyRendererProject
{
    internal enum Format
    {
        Grayscale = 1,
        BGR = 3,
        BGRA = 4
    }

    internal struct TGAColor
    {
        // the value stored as little endian:
        // ARGB -> BGRA
        // xRGB -> BGRx
        public readonly int Value;
        public readonly Format Format;

        public byte this[int offset]
        {
            get
            {
                if (offset > 3) // int has only 4 bytes (0, 1, 2, 3)
                    throw new ArgumentOutOfRangeException();
                return (byte)(Value >> 8 * (3 - offset));
            }
        }

        public static TGAColor Red = new TGAColor(red: 255, green: 0, blue: 0, alpha: 255);
        public static TGAColor Green = new TGAColor(red: 0, green: 255, blue: 0, alpha: 255);
        public static TGAColor Blue = new TGAColor(red: 0, green: 0, blue: 255, alpha: 255);
        public static TGAColor Black = new TGAColor(red: 0, green: 0, blue: 0, alpha: 255);
        public static TGAColor White = new TGAColor(red: 255, green: 255, blue: 255, alpha: 255);
        public static TGAColor Yellow = new TGAColor(red: 225, green: 225, blue: 0, alpha: 255);

        public TGAColor(byte red, byte green, byte blue, byte alpha)
            : this((blue << 24) | (green << 16) | (red << 8) | alpha, Format.BGRA)
        {
        }

        public TGAColor(byte red, byte green, byte blue)
            : this((blue << 24) | (green << 16) | (red << 8) | 0xFF, Format.BGR)
        {
        }

        public TGAColor(byte value)
            : this(value, Format.Grayscale)
        {
        }

        public TGAColor(int value, Format format)
        {
            Value = value;
            Format = format;
        }

        public static TGAColor operator *(TGAColor tgaColor, float intensity)
        {
            intensity = Math.Max(0f, Math.Min(1f, intensity));
            var ch0 = (byte)(tgaColor[0] * intensity);
            var ch1 = (byte)(tgaColor[1] * intensity);
            var ch2 = (byte)(tgaColor[2] * intensity);
            var ch3 = tgaColor[3];
            return new TGAColor(ch0 << 24 | ch1 << 16 | ch2 << 8 | ch3, tgaColor.Format);
        }
    }

    internal class TGAImage
    {
        internal byte[] Buffer;

        public int Width { get; }
        public int Height { get; }
        public Format Format { get; }

        public int BytesPerRow => Width * (int)Format;

        public TGAImage(int width, int height, Format format)
        {
            Width = width;
            Height = height;
            Format = format;

            Buffer = new byte[height * BytesPerRow];
        }

        public void VerticalFlip()
        {
            var bpp = (int)Format;
            var bytesPerLine = Width * bpp;

            var half = Height >> 1;
            for (var l = 0; l < half; l++)
            {
                var l1 = l * bytesPerLine;
                var l2 = (Height - 1 - l) * bytesPerLine;

                for (var i = 0; i < bytesPerLine; i++)
                {
                    var pixel = Buffer[l1 + i];
                    Buffer[l1 + i] = Buffer[l2 + i];
                    Buffer[l2 + i] = pixel;
                }
            }
        }

        public void Clear()
        {
            for (var i = 0; i < Buffer.Length; i++)
                Buffer[i] = 0;
        }

        public TGAColor this[int x, int y]
        {
            get
            {
                if (x < 0 || x >= Width) throw new ArgumentException("x");
                if (y < 0 || y >= Height) throw new ArgumentException("y");

                var offset = GetOffset(x, y);
                var len = (int)Format;
                var value = 0;
                for (var ch = 0; ch < 4; ch++)
                    value = (value << 8) | (ch < len ? Buffer[offset++] : 0xFF);

                return new TGAColor(value, Format);
            }
            set
            {
                if (x < 0 || x >= Width) return; //throw new ArgumentException ($"{nameof(x)}={x} {nameof(Width)}={Width}");
                if (y < 0 || y >= Height) return; // throw new ArgumentException ($"{nameof(y)}={y} {nameof(Height)}={Height}");

                var offset = GetOffset(x, y);
                var v = value.Value;
                var len = (int)Format;
                for (var ch = 0; ch < len; ch++)                   // 0123
                    Buffer[offset++] = (byte)(v >> (3 - ch) * 8); // BGRA
            }
        }

        private int GetOffset(int x, int y)
        {
            return y * BytesPerRow + x * (int)Format;
        }

        public bool WriteToFile(string path, bool rle = true)
        {
            var bpp = (int)Format;
            using (var writer = new BinaryWriter(File.Create(path)))
            {
                var header = new TGAHeader
                {
                    IdLength = 0, // The IDLength set to 0 indicates that there is no image identification field in the TGA file
                    ColorMapType = 0, // a value of 0 indicates that no palette is included
                    BitsPerPixel = (byte)(bpp * 8),
                    Width = (short)Width,
                    Height = (short)Height,
                    DataTypeCode = DataTypeFor(bpp, rle),
                    ImageDescriptor = (byte)(0x20 | (Format == Format.BGRA ? 8 : 0)) // top-left origin
                };
                WriteTo(writer, header);
                if (!rle)
                    writer.Write(Buffer);
                else
                    UnloadRleData(writer);
            }
            return true;
        }

        public static TGAImage Load(string path)
        {
            using var reader = new BinaryReader(File.OpenRead(path));
            var header = ReadHeader(reader);

            var height = header.Height;
            var width = header.Width;
            var bytespp = header.BitsPerPixel >> 3;
            var format = (Format)bytespp;

            if (width <= 0 || height <= 0)
                throw new InvalidProgramException($"bad image size: width={width} height={height}");
            if (format != Format.BGR && format != Format.BGRA && format != Format.Grayscale)
                throw new InvalidProgramException($"unknown format {format}");

            var img = new TGAImage(width, height, format);

            switch (header.DataTypeCode)
            {
                case DataType.UncompressedTrueColorImage:
                case DataType.UncompressedBlackAndWhiteImage:
                    reader.Read(img.Buffer, 0, img.Buffer.Length);
                    break;
                case DataType.RleTrueColorImage:
                case DataType.RleBlackAndWhiteImage:
                    img.LoadRleData(reader);
                    break;
                default:
                    throw new InvalidProgramException($"Unsupported image format {header.DataTypeCode}");
            }

            if ((header.ImageDescriptor & 0x20) == 0)
                img.VerticalFlip();

            return img;
        }

        private static void WriteTo(BinaryWriter writer, TGAHeader header)
        {
            writer.Write(header.IdLength);
            writer.Write(header.ColorMapType);
            writer.Write((byte)header.DataTypeCode);
            writer.Write(header.ColorMapOrigin);
            writer.Write(header.ColorMapLength);
            writer.Write(header.ColorMapDepth);
            writer.Write(header.OriginX);
            writer.Write(header.OriginY);
            writer.Write(header.Width);
            writer.Write(header.Height);
            writer.Write(header.BitsPerPixel);
            writer.Write(header.ImageDescriptor);
        }

        private static TGAHeader ReadHeader(BinaryReader reader)
        {
            var header = new TGAHeader
            {
                IdLength = reader.ReadByte(),
                ColorMapType = reader.ReadByte(),
                DataTypeCode = (DataType)reader.ReadByte(),
                ColorMapOrigin = reader.ReadInt16(),
                ColorMapLength = reader.ReadInt16(),
                ColorMapDepth = reader.ReadByte(),
                OriginX = reader.ReadInt16(),
                OriginY = reader.ReadInt16(),
                Width = reader.ReadInt16(),
                Height = reader.ReadInt16(),
                BitsPerPixel = reader.ReadByte(),
                ImageDescriptor = reader.ReadByte()
            };
            return header;
        }

        private bool UnloadRleData(BinaryWriter writer)
        {
            const int maxChunkLength = 128;
            var nPixels = Width * Height;
            var currentPixel = 0;
            var bpp = (int)Format;

            while (currentPixel < nPixels)
            {
                var chunkStart = currentPixel * bpp;
                var currentByte = currentPixel * bpp;
                var runLength = 1;
                var literal = true;
                while (currentPixel + runLength < nPixels && runLength < maxChunkLength && currentPixel + runLength < currentPixel + Width)
                {
                    var succEq = true;
                    for (var t = 0; succEq && t < bpp; t++)
                    {
                        succEq = (Buffer[currentByte + t] == Buffer[currentByte + t + bpp]);
                    }
                    currentByte += bpp;
                    if (1 == runLength)
                    {
                        literal = !succEq;
                    }
                    if (literal && succEq)
                    {
                        runLength--;
                        break;
                    }
                    if (!literal && !succEq)
                    {
                        break;
                    }
                    runLength++;
                }
                currentPixel += runLength;

                writer.Write((byte)(literal ? runLength - 1 : 128 + (runLength - 1)));
                writer.Write(Buffer, chunkStart, literal ? runLength * bpp : bpp);
            }
            return true;
        }

        private void LoadRleData(BinaryReader reader)
        {
            var pixelCount = Width * Height;
            var currentPixel = 0;
            var currentByte = 0;

            var bytespp = (int)Format;
            var color = new byte[4];

            do
            {
                var chunkReader = reader.ReadByte();
                if (chunkReader < 128)
                {
                    chunkReader++;
                    for (var i = 0; i < chunkReader; i++)
                    {
                        for (var t = 0; t < bytespp; t++)
                        {
                            Buffer[currentByte++] = reader.ReadByte();
                        }
                        currentPixel++;
                        if (currentPixel > pixelCount)
                        {
                            throw new InvalidProgramException("Too many pixels read");
                        }
                    }
                }
                else
                {
                    chunkReader -= 127;
                    reader.Read(color, 0, bytespp);
                    for (var i = 0; i < chunkReader; i++)
                    {
                        for (var t = 0; t < bytespp; t++)
                        {
                            Buffer[currentByte++] = color[t];
                        }
                        currentPixel++;
                        if (currentPixel > pixelCount)
                        {
                            throw new InvalidProgramException("Too many pixels read");
                        }
                    }
                }
            } while (currentPixel < pixelCount);
        }

        private static DataType DataTypeFor(int bpp, bool rle)
        {
            var format = (Format)bpp;
            if (format == Format.Grayscale)
            {
                return rle ? DataType.RleBlackAndWhiteImage : DataType.UncompressedBlackAndWhiteImage;
            }
            return rle ? DataType.RleTrueColorImage : DataType.UncompressedTrueColorImage;
        }
    }

    internal struct TGAHeader
    {
        public byte IdLength;
        public byte ColorMapType;
        public DataType DataTypeCode;

        // field #4. Color map specification
        public short ColorMapOrigin; // index of first color map entry that is included in the file
        public short ColorMapLength; // number of entries of the color map that are included in the file
        public byte ColorMapDepth;   // number of bits per pixel

        // field #5. Image specification
        public short OriginX; // absolute coordinate of lower-left corner for displays where origin is at the lower left
        public short OriginY; // as for X-origin
        public short Width;   // width in pixels
        public short Height;  // height in pixels
        public byte BitsPerPixel;     // pixel depth
        public byte ImageDescriptor;  // bits 3-0 give the alpha channel depth, bits 5-4 give direction
    }

    public enum DataType : byte
    {
        NoImageData = 0, // no image data is present
        UncompressedColorMappedImage = 1,
        UncompressedTrueColorImage = 2,
        UncompressedBlackAndWhiteImage = 3,
        RleColorMappedImage = 9, // run-length encoded color-mapped image
        RleTrueColorImage = 10, // run-length encoded true-color image
        RleBlackAndWhiteImage = 11 // run-length encoded black-and-white (grayscale) image
    }
}