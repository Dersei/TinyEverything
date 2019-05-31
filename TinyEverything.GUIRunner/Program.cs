using System;
using System.Threading;
using SDL2;
using TinyEverything.Common;
using TinyEverything.TinyRaycasterProject;

namespace TinyEverything.GUIRunner
{
    class Program
    {
        static unsafe void Main(string[] args)
        {
            SDL.SDL_SetHint(SDL.SDL_HINT_WINDOWS_DISABLE_THREAD_NAMING, "1");
            if (SDL.SDL_Init(SDL.SDL_INIT_VIDEO) != 0)
            {
                Console.WriteLine("Failed to initialize SDL: " + SDL.SDL_GetError());
                return;
            }

            Framebuffer<uint> framebuffer = new Framebuffer<uint>(1024, 512, ColorUtils.PackColor(255, 255, 255));
            var raycaster = new TinyRaycaster()
            {
                Framebuffer = framebuffer,
                Player = new Player
                {
                    X = 3.456f, // player x position
                    Y = 2.345f, // player y position
                    A = 1.523f
                }
            };


            if (SDL.SDL_CreateWindowAndRenderer(framebuffer.Width, framebuffer.Height, SDL.SDL_WindowFlags.SDL_WINDOW_SHOWN | SDL.SDL_WindowFlags.SDL_WINDOW_INPUT_FOCUS, out var window, out var renderer) != 0)
            {
                Console.WriteLine("Failed to create window: " + SDL.SDL_GetError());
                return;
            }

            var texture = SDL.SDL_CreateTexture(renderer, SDL.SDL_PIXELFORMAT_ABGR8888,
                (int)SDL.SDL_TextureAccess.SDL_TEXTUREACCESS_STREAMING, framebuffer.Width, framebuffer.Height);

            if (texture == (IntPtr)0)
            {
                Console.WriteLine("Failed to create texture: " + SDL.SDL_GetError());
                return;
            }

            var time = DateTime.Now;

            while (true)
            {
                var time2 = DateTime.Now;
                if ((time2 - time).Milliseconds < 20)
                {
                    Thread.Sleep(3);
                    continue;
                }

                time = time2;

                if (SDL.SDL_PollEvent(out var sdlEvent) != 0)
                {
                    if (SDL.SDL_EventType.SDL_QUIT == sdlEvent.type ||
                        SDL.SDL_EventType.SDL_KEYDOWN == sdlEvent.type &&
                        SDL.SDL_Keycode.SDLK_ESCAPE == sdlEvent.key.keysym.sym)
                    {
                        break;
                    }

                    if (SDL.SDL_EventType.SDL_KEYUP == sdlEvent.type)
                    {
                        if (SDL.SDL_Keycode.SDLK_a == sdlEvent.key.keysym.sym ||
                            SDL.SDL_Keycode.SDLK_d == sdlEvent.key.keysym.sym)
                        {
                            raycaster.Player.Turn = 0;
                        }
                        if (SDL.SDL_Keycode.SDLK_w == sdlEvent.key.keysym.sym ||
                            SDL.SDL_Keycode.SDLK_s == sdlEvent.key.keysym.sym)
                        {
                            raycaster.Player.Walk = 0;
                        }
                    }

                    if (SDL.SDL_EventType.SDL_KEYDOWN == sdlEvent.type)
                    {
                        if (SDL.SDL_Keycode.SDLK_a == sdlEvent.key.keysym.sym)
                        {
                            raycaster.Player.Turn = -1;
                        }
                        if (SDL.SDL_Keycode.SDLK_d == sdlEvent.key.keysym.sym) raycaster.Player.Turn = 1;
                        if (SDL.SDL_Keycode.SDLK_w == sdlEvent.key.keysym.sym) raycaster.Player.Walk = 1;
                        if (SDL.SDL_Keycode.SDLK_s == sdlEvent.key.keysym.sym) raycaster.Player.Walk = -1;
                    }
                }

                {
                    raycaster.Player.A += raycaster.Player.Turn * 0.05f; // TODO measure elapsed time and modify the speed accordingly
                    var nx = raycaster.Player.X + raycaster.Player.Walk * MathF.Cos(raycaster.Player.A) * 0.05f;
                    var ny = raycaster.Player.Y + raycaster.Player.Walk * MathF.Sin(raycaster.Player.A) * 0.05f;

                    if ((int)nx >= 0 && (int)nx < raycaster.Map.Width && (int)ny >= 0 &&
                        (int)ny < raycaster.Map.Height)
                    {
                        if (raycaster.Map.IsEmpty((int)nx, (int)raycaster.Player.Y)) raycaster.Player.X = nx;
                        if (raycaster.Map.IsEmpty((int)raycaster.Player.X, (int)ny)) raycaster.Player.Y = ny;
                    }

                    for (var i = 0; i < raycaster.Sprites.Count; i++)
                    {
                        // update the distances from the player to each sprite
                        raycaster.Sprites[i].PlayerDist =
                            MathF.Sqrt(MathF.Pow(raycaster.Player.X - raycaster.Sprites[i].X, 2) +
                                       MathF.Pow(raycaster.Player.Y - raycaster.Sprites[i].Y, 2));
                    }

                    raycaster.Sprites.Sort((s1, s2) => (int)(s2.PlayerDist - s1.PlayerDist));
                }

                raycaster.Run();
                uint[] buffer = new uint[framebuffer.GetData().Length];
                Array.Copy(framebuffer.GetData(), buffer, buffer.Length);
                fixed (uint* p = buffer)
                {
                    IntPtr ptr = (IntPtr)p;
                    SDL.SDL_UpdateTexture(texture, IntPtr.Zero, ptr, framebuffer.Width * 4);
                    SDL.SDL_RenderClear(renderer);
                    SDL.SDL_RenderCopy(renderer, texture, IntPtr.Zero, IntPtr.Zero);
                    SDL.SDL_RenderPresent(renderer);
                }

            }

            SDL.SDL_DestroyTexture(texture);
            SDL.SDL_DestroyRenderer(renderer);
            SDL.SDL_DestroyWindow(window);
            SDL.SDL_Quit();
        }
    }
}
