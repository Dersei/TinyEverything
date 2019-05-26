using System;
using System.IO;
using System.Text;
using TinyEverything.Common;

namespace TinyEverything.TinyRaycasterProject
{
    public class TinyRaycaster
    {
        private readonly Map _map = new Map();
        private readonly Player _player = new Player();
        private readonly Framebuffer<uint> _framebuffer = new Framebuffer<uint>(1024, 512, ColorUtils.PackColor(255, 255, 255));

        private readonly string _directoryName = $"dir-{DateTime.Now:yyyy-dd-M--HH-mm-ss.fff}";

        private void DrawMap(int rectWidth, int rectHeight, Texture texture)
        {
            for (var j = 0; j < _map.Height; j++)
            { // draw the map
                for (var i = 0; i < _map.Width; i++)
                {
                    if (_map.IsEmpty(i, j)) continue; // skip empty spaces
                    var rectX = i * rectWidth;
                    var rectY = j * rectHeight;
                    var textureId = _map[i, j];
                    _framebuffer.DrawRectangle(rectX, rectY, rectWidth, rectHeight, texture[textureId * texture.Size]);
                }
            }
        }

        private void Render()
        {
            var texture = new Texture("Resources/walltext.png");
            var rectWidth = _framebuffer.Width / (_map.Width * 2);
            var rectHeight = _framebuffer.Height / _map.Height;


            _framebuffer.Clear(ColorUtils.PackColor(255, 255, 255));

            DrawMap(rectWidth, rectHeight, texture);

            for (var i = 0; i < _framebuffer.Width / 2; i++)
            {
                // draw the visibility cone AND the "3D" view
                var angle = _player.A - _player.FOV / 2 + _player.FOV * i / ((float)_framebuffer.Width / 2);
                for (float t = 0; t < 20; t += 0.01f)
                {
                    var x = _player.X + t * MathF.Cos(angle);
                    var y = _player.Y + t * MathF.Sin(angle);

                    var pixX = (int)(x * rectWidth);
                    var pixY = (int)(y * rectHeight);
                    _framebuffer.SetPixel(pixX, pixY, ColorUtils.PackColor(160, 160, 160)); // this draws the visibility cone

                    if (_map.IsEmpty((int)x, (int)y)) continue;

                    var textureId = _map[(int)x, (int)y];
                    // our ray touches a wall, so draw the vertical column to create an illusion of 3D
                    var columnHeight = (int)(_framebuffer.Height / (t * MathF.Cos(angle - _player.A)));
                    var hitX = x - MathF.Floor(x + 0.5f); // hitx and hity contain (signed) fractional parts of cx and cy,
                    var hitY = y - MathF.Floor(y + 0.5f); // they vary between -0.5 and +0.5, and one of them is supposed to be very close to 0
                    var textureCoord = (int)(hitX * texture.Size);

                    if (MathF.Abs(hitY) > MathF.Abs(hitX))
                    {
                        textureCoord = (int)(hitY * texture.Size);
                    }

                    if (textureCoord < 0)
                    {
                        textureCoord += texture.Size; // do not forget x_texcoord can be negative, fix that
                    }

                    var column = texture.GetScaledColumn(textureId, textureCoord, columnHeight);
                    pixX = _framebuffer.Width / 2 + i;
                    for (var j = 0; j < columnHeight; j++)
                    {
                        pixY = j + _framebuffer.Height / 2 - columnHeight / 2;
                        if (pixY < 0 || pixY >= _framebuffer.Height) continue;
                        _framebuffer.SetPixel(pixX, pixY, column[j]);
                    }
                    break;
                }
            }
        }


        public void Run()
        {

            Directory.CreateDirectory(_directoryName);
            _player.X = 3.456f; // player x position
            _player.Y = 2.345f; // player y position
            _player.A = 1.523f;
            _player.FOV = MathF.PI / 3.0f;


            for (var frame = 0; frame < 360; frame++)
            {
                _player.A += 2 * MathF.PI / 360f;
                Render();
                var fileName = $"{frame}.ppm";

                Save(fileName, _framebuffer.Height, _framebuffer.Width, _framebuffer);
            }


        }

        public void Save(string fileName, int height, int width, Framebuffer<uint> data)
        {
            using var fileStream = File.Open($"{_directoryName}\\{fileName}", FileMode.CreateNew, FileAccess.Write);
            using var writer = new BinaryWriter(fileStream, Encoding.ASCII);
            writer.Write(Encoding.ASCII.GetBytes($"P6 {width} {height} 255 ")); // trailing space!!!

            for (var i = 0; i < height * width; ++i)
            {
                ColorUtils.UnpackColor(data[i], out var r, out var g, out var b, out _);
                writer.Write(r);
                writer.Write(g);
                writer.Write(b);
                //writer.Write(a);
            }
        }
    }
}
