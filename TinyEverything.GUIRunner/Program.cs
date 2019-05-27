using System;
using SDL2;
using TinyEverything.Common;
using TinyEverything.TinyRaycasterProject;

namespace TinyEverything.GUIRunner
{
    class Program
    {
        static void Main(string[] args)
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

            };


            if (SDL.SDL_CreateWindowAndRenderer(512, 512, SDL.SDL_WindowFlags.SDL_WINDOW_SHOWN | SDL.SDL_WindowFlags.SDL_WINDOW_INPUT_FOCUS, out var window, out var renderer) != 0)
            {
                Console.WriteLine("Failed to create window: " + SDL.SDL_GetError());
                return;
            }

            var texture = SDL.SDL_CreateTexture(renderer, SDL.SDL_PIXELFORMAT_ABGR8888,
                (int) SDL.SDL_TextureAccess.SDL_TEXTUREACCESS_STREAMING, framebuffer.Width, framebuffer.Height);

            if (texture == (IntPtr) 0)
            {
                Console.WriteLine("Failed to create texture: " + SDL.SDL_GetError());
                return;
            }



            while (true)
            {

            }
        }
    }
}
