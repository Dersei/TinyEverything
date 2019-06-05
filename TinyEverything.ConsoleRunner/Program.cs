using TinyEverything.TinyKaboomProject;
using TinyEverything.TinyRaycasterProject;
using TinyEverything.TinyRaytracerProject;
using TinyEverything.TinyRendererProject;

namespace TinyEverything.ConsoleRunner
{
    internal class Program
    {
        private static void Main()
        {
            //new TinyRaytracer().DefaultSettings().Run();
            new TinyRenderer().Run();
        }
    }
}
