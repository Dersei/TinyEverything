using System;
using System.Runtime.InteropServices;
using TinyEverything.Common;

namespace TinyEverything.GUIRunner.SDL2
{
    public class SDLWindow
    {
        public int Width { get; }
        public int Height { get; }
        public IntPtr Renderer => _renderer;
        public IntPtr Texture => _texture;

        private IntPtr _renderer;
        private IntPtr _texture;

        public Framebuffer<uint> Framebuffer;

        private const string NativeLibName = "SDL2";
        private IntPtr _window;

        public SDLWindow(int width, int height, Framebuffer<uint> framebuffer)
        {
            Width = width;
            Height = height;
            Framebuffer = framebuffer;
        }

        private const uint InitVideo = 32;
        private const uint ShownAndInputFocus = 0x00000004 | 0x00000200; //516
        public bool Init()
        {
            if (!SetHint()) return false;
            if (SDL_Init(InitVideo) != 0) return false;
            if (SDL_CreateWindowAndRenderer(Width, Height, ShownAndInputFocus, out _window, out _renderer) != 0) return false;
            if (CreateTexture()) return false;
            return true;
        }

        [DllImport(NativeLibName, CallingConvention = CallingConvention.Cdecl)]
        private static extern int SDL_Init(uint flags);

        /* window refers to an SDL_Window*, renderer to an SDL_Renderer* */
        [DllImport(NativeLibName, CallingConvention = CallingConvention.Cdecl)]
        private static extern int SDL_CreateWindowAndRenderer(int width, int height, uint windowFlags, out IntPtr window, out IntPtr renderer);

        [DllImport(NativeLibName, CallingConvention = CallingConvention.Cdecl)]
        private static extern int SDL_SetHint(byte[] name, byte[] value);
        private bool SetHint()
        {
            return SDL_SetHint(UTF8ToNative("SDL_WINDOWS_DISABLE_THREAD_NAMING")!, UTF8ToNative("1")!) == 1;
        }

        /* window refers to an SDL_Window* */
        [DllImport(NativeLibName, CallingConvention = CallingConvention.Cdecl)]
        private static extern void SDL_DestroyWindow(IntPtr window);

        public void Destroy()
        {
            SDL_DestroyTexture(Texture);
            SDL_DestroyRenderer(_renderer);
            SDL_DestroyWindow(_window);
            SDL_Quit();
        }

        [DllImport(NativeLibName, CallingConvention = CallingConvention.Cdecl)]
        private static extern void SDL_Quit();

        /* IntPtr refers to an SDL_Texture*, renderer to an SDL_Renderer* */
        [DllImport(NativeLibName, CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr SDL_CreateTexture(IntPtr renderer, uint format, int access, int w, int h);

        private bool CreateTexture()
        {
            const uint pixelFormatAbgr8888 = 376840196;
            _texture = SDL_CreateTexture(_renderer, pixelFormatAbgr8888, 1, Width, Height);
            return _texture == IntPtr.Zero;
        }

        public (SDLEvents.EventType type, SDLEvents.KeyCode key) GetEvent()
        {
            SDLEvents.PollEvent(out var sdlEvent);
            if (sdlEvent.Type == SDLEvents.EventType.Quit ||
                sdlEvent.Type == SDLEvents.EventType.KeyDown &&
                sdlEvent.Key.KeySymbol.Symbol == SDLEvents.KeyCode.Escape)
            {
                return (SDLEvents.EventType.Quit, sdlEvent.Key.KeySymbol.Symbol);
            }
            return (sdlEvent.Type, sdlEvent.Key.KeySymbol.Symbol);
        }

        public unsafe void Update()
        {
            var buffer = new uint[Framebuffer.Data.Length];
            Array.Copy(Framebuffer.Data, buffer, buffer.Length);
            fixed (uint* p = buffer)
            {
                var ptr = (IntPtr)p;
                SDL_UpdateTexture(Texture, IntPtr.Zero, ptr, Framebuffer.Width * 4);
                SDL_RenderClear(_renderer);
                SDL_RenderCopy(_renderer, Texture, IntPtr.Zero, IntPtr.Zero);
                SDL_RenderPresent(_renderer);
            }
        }

        /* renderer refers to an SDL_Renderer* */
        [DllImport(NativeLibName, CallingConvention = CallingConvention.Cdecl)]
        private static extern void SDL_DestroyRenderer(IntPtr renderer);

        /* texture refers to an SDL_Texture* */
        [DllImport(NativeLibName, CallingConvention = CallingConvention.Cdecl)]
        private static extern void SDL_DestroyTexture(IntPtr texture);

        /* texture refers to an SDL_Texture* */
        [DllImport(NativeLibName, CallingConvention = CallingConvention.Cdecl)]
        private static extern int SDL_UpdateTexture(IntPtr texture, IntPtr rect, IntPtr pixels, int pitch);

        [DllImport(NativeLibName, CallingConvention = CallingConvention.Cdecl)]
        private static extern int SDL_RenderClear(IntPtr renderer);

        [DllImport(NativeLibName, CallingConvention = CallingConvention.Cdecl)]
        private static extern int SDL_RenderCopy(IntPtr renderer, IntPtr texture, IntPtr srcrect, IntPtr dstrect);

        /* renderer refers to an SDL_Renderer* */
        [DllImport(NativeLibName, CallingConvention = CallingConvention.Cdecl)]
        private static extern void SDL_RenderPresent(IntPtr renderer);

        private static byte[]? UTF8ToNative(string s)
        {
            return s == null ? null : System.Text.Encoding.UTF8.GetBytes(s + '\0');
        }

        private static unsafe string? UTF8ToManaged(IntPtr s)
        {
            if (s == IntPtr.Zero)
            {
                return null;
            }

            var ptr = (byte*)s;
            while (*ptr != 0)
            {
                ptr++;
            }

            var result = System.Text.Encoding.UTF8.GetString((byte*)s, (int)(ptr - (byte*)s));

            return result;
        }


        [DllImport(NativeLibName, CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr SDL_GetError();
        public string? GetError()
        {
            return UTF8ToManaged(SDL_GetError());
        }
    }
}
