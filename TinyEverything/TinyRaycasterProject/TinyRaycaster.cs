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
        public Map Map = new Map();
        public Player Player = new Player();
        public Framebuffer<uint> Framebuffer = new Framebuffer<uint>(1024, 512, ColorUtils.PackColor(255, 255, 255));
        public List<Sprite> Sprites = new List<Sprite>
        {
            new Sprite(3.523f, 3.812f, 0,2),
            new Sprite(1.834f, 8.765f, 0,0),
            new Sprite(5.323f, 5.365f, 0,1),
            new Sprite(4.123f, 10.265f, 0,1),
        };

        private readonly string _directoryName = $"dir-{DateTime.Now:yyyy-dd-M--HH-mm-ss.fff}";

        private void MapShowSprite(Sprite sprite)
        {
            var rectW = Framebuffer.Width / (Map.Width * 2); // size of one map cell on the screen
            var rectH = Framebuffer.Height / Map.Height;
            Framebuffer.DrawRectangle((int)(sprite.X * rectW - 3), (int)(sprite.Y * rectH - 3), 6, 6, ColorUtils.PackColor(255, 0, 0));
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
            for (var j = 0; j < Map.Height; j++)
            { // draw the map
                for (var i = 0; i < Map.Width; i++)
                {
                    if (Map.IsEmpty(i, j)) continue; // skip empty spaces
                    var rectX = i * rectWidth;
                    var rectY = j * rectHeight;
                    var textureId = Map[i, j];
                    Framebuffer.DrawRectangle(rectX, rectY, rectWidth, rectHeight, texture[textureId * texture.Size]);
                }
            }
        }

        private void DrawSprite(Sprite sprite, List<float> depthBuffer, Texture texSprites)
        {
            // absolute direction from the player to the sprite (in radians)
            var spriteDir = MathF.Atan2(sprite.Y - Player.Y, sprite.X - Player.X);
            // remove unnecessary periods from the relative direction
            while (spriteDir - Player.A > MathF.PI) spriteDir -= 2 * MathF.PI;
            while (spriteDir - Player.A < -MathF.PI) spriteDir += 2 * MathF.PI;

            // distance from the player to the sprite
            var spriteScreenSize = (int)MathF.Min(1000, (int)(Framebuffer.Height / sprite.PlayerDist));
            // do not forget the 3D view takes only a half of the framebuffer, thus fb.Width/2 for the screen width
            var hOffset = (int)((spriteDir - Player.A) / (Player.FOV) * (Framebuffer.Width / 2) + (Framebuffer.Width / 2) / 2 - texSprites.Size / 2);
            var vOffset = Framebuffer.Height / 2 - spriteScreenSize / 2;

            for (var i = 0; i < spriteScreenSize; i++)
            {
                if (hOffset + i < 0 || hOffset + i >= Framebuffer.Width / 2) continue;
                if (depthBuffer[hOffset + i] < sprite.PlayerDist) continue;
                for (var j = 0; j < spriteScreenSize; j++)
                {
                    if (vOffset + j < 0 || vOffset + j >= Framebuffer.Height) continue;
                    var color = texSprites.Get(i * texSprites.Size / spriteScreenSize, j * texSprites.Size / spriteScreenSize, sprite.TextureID);
                    ColorUtils.UnpackColor(color, out _, out _, out _, out var a);
                    if (a > 128)
                    {
                        Framebuffer.SetPixel(Framebuffer.Width / 2 + hOffset + i, vOffset + j, color);
                    }
                }
            }
        }
        private void Render()
        {
            var texture = new Texture("Resources/walltext.png");
            var monsters = new Texture("Resources/monsters.png");
            var rectWidth = Framebuffer.Width / (Map.Width * 2);
            var rectHeight = Framebuffer.Height / Map.Height;

            var depthBuffer = Enumerable.Repeat(1e3f, Framebuffer.Width / 2).ToList();
            Framebuffer.Clear(ColorUtils.PackColor(255, 255, 255));

            DrawMap(rectWidth, rectHeight, texture);

            for (var i = 0; i < Framebuffer.Width / 2; i++)
            {
                // draw the visibility cone AND the "3D" view
                var angle = Player.A - Player.FOV / 2 + Player.FOV * i / ((float)Framebuffer.Width / 2);
                for (float t = 0; t < 20; t += 0.01f)
                {
                    var x = Player.X + t * MathF.Cos(angle);
                    var y = Player.Y + t * MathF.Sin(angle);

                    var pixX = (int)(x * rectWidth);
                    var pixY = (int)(y * rectHeight);
                    Framebuffer.SetPixel(pixX, pixY,
                        ColorUtils.PackColor(160, 160, 160)); // this draws the visibility cone

                    if (Map.IsEmpty((int)x, (int)y)) continue;

                    var textureId = Map[(int)x, (int)y];
                    // our ray touches a wall, so draw the vertical column to create an illusion of 3D
                    var dist = (t * MathF.Cos(angle - Player.A));
                    var columnHeight = (int)(Framebuffer.Height / dist);
                    depthBuffer[i] = dist;
                    var textureCoord = WallTextureCoord(x, y, texture);

                    var column = texture.GetScaledColumn(textureId, textureCoord, columnHeight);
                    pixX = Framebuffer.Width / 2 + i;
                    for (var j = 0; j < columnHeight; j++)
                    {
                        pixY = j + Framebuffer.Height / 2 - columnHeight / 2;
                        if (pixY < 0 || pixY >= Framebuffer.Height) continue;
                        Framebuffer.SetPixel(pixX, pixY, column[j]);
                    }

                    break;
                }
            }

            for (var s = 0; s < Sprites.Count; s++)
            { // update the distances from the player to each sprite
                Sprites[s].PlayerDist = MathF.Sqrt(MathF.Pow(Player.X - Sprites[s].X, 2) + MathF.Pow(Player.Y - Sprites[s].Y, 2));
            }

            Sprites.Sort((s1, s2) => (int)(s2.PlayerDist - s1.PlayerDist));

            for (var s = 0; s < Sprites.Count; s++)
            {
                MapShowSprite(Sprites[s]);
                DrawSprite(Sprites[s], depthBuffer, monsters);
            }
        }


        public void Run()
        {

            //Directory.CreateDirectory(_directoryName);

            Player.FOV = MathF.PI / 3.0f;
            //for (var frame = 0; frame < 360; frame++)
            //{
            //    Player.A += 2 * MathF.PI / 360f;
            //    Render();
            //    var fileName = $"{frame}.ppm";

            //    Save(fileName, Framebuffer.Height, Framebuffer.Width, Framebuffer);
            //}
            Render();


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
