using System;
using System.Threading;
using TinyEverything.Common;
using TinyEverything.GUIRunner.SDL2;
using TinyEverything.TinyRaycasterProject;
using static TinyEverything.GUIRunner.SDL2.SDLEvents;

namespace TinyEverything.GUIRunner
{
    internal class Program
    {
        private static void Main()
        {
            var framebuffer = new Framebuffer<uint>(1024, 512, ColorUtils.PackColor(255, 255, 255));
            var raycaster = new TinyRaycaster
            {
                Framebuffer = framebuffer,
                Player = new Player
                {
                    X = 3.456f, // player x position
                    Y = 2.345f, // player y position
                    A = 1.523f,
                    FOV = MathF.PI / 3.0f
                }
            };

            var sdlWindow = new SDLWindow(1024, 512, framebuffer);


            if (!sdlWindow.Init())
            {
                Console.WriteLine(sdlWindow.GetError());
                return;
            }


            var time = DateTime.Now;

            while (true)
            {
                var time2 = DateTime.Now;
                if ((time2 - time).TotalMilliseconds < 20)
                {
                    Thread.Sleep(3);
                    continue;
                }

                time = time2;

                var (type, key) = sdlWindow.GetEvent();

                if (type == EventType.Quit)
                {
                    break;
                }

                if (type == EventType.KeyUp)
                {
                    switch (key)
                    {
                        case KeyCode.A:
                        case KeyCode.D:
                            raycaster.Player.Turn = 0;
                            break;
                        case KeyCode.W:
                        case KeyCode.S:
                            raycaster.Player.Walk = 0;
                            break;
                    }
                }

                if (type == EventType.KeyDown)
                {
                    switch (key)
                    {
                        case KeyCode.A:
                            raycaster.Player.Turn = -1;
                            break;
                        case KeyCode.D:
                            raycaster.Player.Turn = 1;
                            break;
                        case KeyCode.W:
                            raycaster.Player.Walk = 1;
                            break;
                        case KeyCode.S:
                            raycaster.Player.Walk = -1;
                            break;
                    }
                }

                raycaster.Run();
                sdlWindow.Update();

            }

            sdlWindow.Destroy();
        }
    }
}
