using System;
using System.Diagnostics;
using System.Numerics;
using TinyEverything.Common;
using TinyEverything.Kaboom;
using TinyEverything.Raytracer;
using TinyEverything.Renderer;

namespace TinyEverything.ConsoleRunner
{
    internal class Program
    {
        private static void Main()
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();

            var target = 1;
            switch (target)
            {
                case 0:
                    new TinyKaboom
                    {
                        BaseSettings = new BaseSettings(800, 800, new Vector3(0, 0, 0))
                    }.Run();
                    break;
                case 1:
                    new TinyRaytracer
                    {
                        BaseSettings = new BaseSettings(1024, 768, new Vector3(0, 0.5f, 0))
                    }.DefaultSettings().Run();
                    break;
                case 2:
                    new TinyRenderer().Run();
                    break;
            }

            stopwatch.Stop();
            Console.WriteLine(stopwatch.Elapsed);
        }
    }
}
