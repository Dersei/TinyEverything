using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using TinyEverything.Common;

namespace TinyEverything.TinyRaycasterProject
{
    public class TinyRaycaster
    {
        private readonly Map _map = new Map();
        private readonly Player _player = new Player();
        private readonly Framebuffer<uint> _framebuffer = new Framebuffer<uint>(1024, 512, ColorUtils.PackColor(255, 255, 255));
        private List<Sprite> _sprites = new List<Sprite>();

        private readonly string _directoryName = $"dir-{DateTime.Now:yyyy-dd-M--HH-mm-ss.fff}";

        private void MapShowSprite(Sprite sprite)
        {
            var rectW = _framebuffer.Width / (_map.Width * 2); // size of one map cell on the screen
            var rectH = _framebuffer.Height / _map.Height;
            _framebuffer.DrawRectangle((int)(sprite.X * rectW - 3), (int)(sprite.Y * rectH - 3), 6, 6, ColorUtils.PackColor(255, 0, 0));
        }

        private int WallTextureCoord(float x, float y, Texture wallTexture)
        {
            var hitX = x - MathF.Floor(x + 0.5f);
            var hitY = y - MathF.Floor(y + 0.5f);
            var texture = (int)(hitX * wallTexture.Size);
            if (MathF.Abs(hitY) > MathF.Abs(hitX))
            {
                texture = (int)(hitY * wallTexture.Size);
            }

            if (texture < 0)
            {
                texture += wallTexture.Size;
            }

            return texture;
        }

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

        private void DrawSprite(Sprite sprite, List<float> depthBuffer, Texture texSprites)
        {
            // absolute direction from the player to the sprite (in radians)
            var spriteDir = MathF.Atan2(sprite.Y - _player.Y, sprite.X - _player.X);
            // remove unnecessary periods from the relative direction
            while (spriteDir - _player.A > MathF.PI) spriteDir -= 2 * MathF.PI;
            while (spriteDir - _player.A < -MathF.PI) spriteDir += 2 * MathF.PI;

            // distance from the player to the sprite
            var spriteScreenSize = (int)MathF.Min(1000, (int)(_framebuffer.Height / sprite.PlayerDist));
            // do not forget the 3D view takes only a half of the framebuffer, thus fb.Width/2 for the screen width
            var hOffset = (int)((spriteDir - _player.A) / (_player.FOV) * (_framebuffer.Width / 2) + (_framebuffer.Width / 2) / 2 - texSprites.Size / 2);
            var vOffset = _framebuffer.Height / 2 - spriteScreenSize / 2;

            for (var i = 0; i < spriteScreenSize; i++)
            {
                if (hOffset + i < 0 || hOffset + i >= _framebuffer.Width / 2) continue;
                if (depthBuffer[hOffset + i] < sprite.PlayerDist) continue;
                for (var j = 0; j < spriteScreenSize; j++)
                {
                    if (vOffset + j < 0 || vOffset + j >= _framebuffer.Height) continue;
                    var color = texSprites.Get(i * texSprites.Size / spriteScreenSize, j * texSprites.Size / spriteScreenSize, sprite.TextureID);
                    ColorUtils.UnpackColor(color, out _, out _, out _, out var a);
                    if (a > 128)
                    {
                        _framebuffer.SetPixel(_framebuffer.Width / 2 + hOffset + i, vOffset + j, color);
                    }
                }
            }
        }

        private void Render()
        {
            var texture = new Texture("Resources/walltext.png");
            var monsters = new Texture("Resources/monsters.png");
            var rectWidth = _framebuffer.Width / (_map.Width * 2);
            var rectHeight = _framebuffer.Height / _map.Height;

            var depthBuffer = Enumerable.Repeat(1e3f, _framebuffer.Width / 2).ToList();
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
                    _framebuffer.SetPixel(pixX, pixY,
                        ColorUtils.PackColor(160, 160, 160)); // this draws the visibility cone

                    if (_map.IsEmpty((int)x, (int)y)) continue;

                    var textureId = _map[(int)x, (int)y];
                    // our ray touches a wall, so draw the vertical column to create an illusion of 3D
                    var dist = (t * MathF.Cos(angle - _player.A));
                    var columnHeight = (int)(_framebuffer.Height / dist);
                    depthBuffer[i] = dist;
                    var textureCoord = WallTextureCoord(x, y, texture);

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

            for (var s = 0; s < _sprites.Count; s++)
            { // update the distances from the player to each sprite
                _sprites[s].PlayerDist = MathF.Sqrt(MathF.Pow(_player.X - _sprites[s].X, 2) + MathF.Pow(_player.Y - _sprites[s].Y, 2));
            }

            _sprites.Sort((s1, s2) => (int)(s2.PlayerDist - s1.PlayerDist));

            for (var s = 0; s < _sprites.Count; s++)
            {
                MapShowSprite(_sprites[s]);
                DrawSprite(_sprites[s], depthBuffer, monsters);
            }
        }


        public void Run()
        {

            Directory.CreateDirectory(_directoryName);
            _player.X = 3.456f; // player x position
            _player.Y = 2.345f; // player y position
            _player.A = 1.523f;
            _player.FOV = MathF.PI / 3.0f;
            _sprites = new List<Sprite>()
            {
                new Sprite(3.523f, 3.812f, 0,2),
                new Sprite(1.834f, 8.765f, 0,0),
                new Sprite(5.323f, 5.365f, 0,1),
                new Sprite(4.123f, 10.265f, 0,1),
            };

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
