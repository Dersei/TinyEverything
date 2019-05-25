using TinyEverything.TinyKaboomProject;
using TinyEverything.TinyRaycasterProject;
using TinyEverything.TinyRaytracerProject;

namespace TinyEverything.ConsoleRunner
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            //new TinyRaytracer().DefaultSettings().Run();
            new TinyRaycaster().Run();
        }
    }
}
