﻿using System;
using System.Numerics;

namespace TinyEverything.TinyRaytracerProject
{
    internal struct Sphere
    {
        public Vector3 Center { get; }
        public float Radius { get; }
        public Material Material { get; }

        public Sphere(Vector3 center, float radius, Material material)
        {
            Center = center;
            Radius = radius;
            Material = material;
        }

        public bool IsRayIntersecting(Vector3 origin, Vector3 direction, ref float distance)
        {
            var l = Center - origin;
            var tca = Vector3.Dot(l, direction);
            var d2 = Vector3.Dot(l, l) - tca * tca;
            if (d2 > Radius * Radius) return false;
            var thc = MathF.Sqrt(Radius * Radius - d2);
            distance = tca - thc;
            var t1 = tca + thc;
            if (distance < 0) distance = t1;
            return !(distance < 0);
        }
    };
}