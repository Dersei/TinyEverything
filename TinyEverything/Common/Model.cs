using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Numerics;

namespace TinyEverything.Common
{
    internal class Model
    {
        public List<Vector3> Vertices { get; } = new List<Vector3>();
        public List<Vector<int>> Faces { get; } = new List<Vector<int>>();

        public Model(string filename)
        {
            using var stream = new StreamReader(filename);

            while (!stream.EndOfStream)
            {
                var line = stream.ReadLine();
                if (line is null)
                {
                }
                else if (line.StartsWith("v "))
                {
                    var array = line.Split(" ").Select(v => float.Parse(v, CultureInfo.InvariantCulture)).ToArray();
                    Vertices.Add(new Vector3(array[0], array[1], array[2]));
                }
                else if (line.StartsWith("f "))
                {
                    var array = line.Split(" ").Select(int.Parse).Select(f => --f).ToArray();
                    Faces.Add(new Vector<int>(array));
                }
            }
            //GetBbox(new Vector3(), new Vector3());

        }

        private bool IsRayTriangleIntersecting(int fi, Vector3 orig, Vector3 dir, ref float tnear)
        {
            var edge1 = Vertices[Faces[fi][1]] - Vertices[Faces[fi][0]];
            var edge2 = Vertices[Faces[fi][2]] - Vertices[Faces[fi][0]];
            var pVector = Vector3.Cross(dir, edge2);
            var det = Vector3.Dot(edge1, pVector);
            if (det < 1e-5) return false;

            var tVector = orig - Vertices[Faces[fi][0]];
            var u = Vector3.Dot(tVector, pVector);
            if (u < 0 || u > det) return false;

            var qVector = Vector3.Cross(tVector, edge1);
            var v = Vector3.Dot(dir, qVector);
            if (v < 0 || u + v > det) return false;

            tnear = Vector3.Dot(edge2, qVector) * 1.0f / det;
            return tnear > 1e-5;
        }

        private void GetBbox(ref Vector3 min, ref Vector3 max)
        {
            min = max = Vertices[0];
            for (var i = 1; i < Vertices.Count; ++i)
            {
                min.X = MathF.Min(min.X, Vertices[i].X);
                min.Y = MathF.Min(min.Y, Vertices[i].Y);
                min.Z = MathF.Min(min.Z, Vertices[i].Z);
                max.X = MathF.Max(max.X, Vertices[i].X);
                max.Y = MathF.Max(max.Y, Vertices[i].Y);
                max.Z = MathF.Max(max.Z, Vertices[i].Z);
            }
        }
    }
}
