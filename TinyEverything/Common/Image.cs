using System.Collections.Generic;
using System.Numerics;

namespace TinyEverything.Common
{
    internal class Image
    {
        public int Height { get; set; }
        public int Width { get; set; }
        public List<Vector3>? Data { get; set; }
        public string Name { get; set; }

        public Image(string name)
        {
            Name = name;
        }

        public bool CheckSizes()
        {
            return Height > 0 && Width > 0;
        }

        public bool CheckIfCorrect()
        {
            return CheckData() && CheckSizes();
        }

        public bool CheckData()
        {
            return Data != null && Data.Count == Height * Width;
        }
    }
}
