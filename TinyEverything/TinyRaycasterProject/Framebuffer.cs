using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TinyEverything.TinyRaycasterProject
{
    internal class Framebuffer<T> where T : struct
    {
        private List<T> _data;

        public Framebuffer(int size, T value) => _data = Enumerable.Repeat(value, size).ToList();

        public void Clear(int size, T value) => _data = Enumerable.Repeat(value, size).ToList();

        public T this[int index] => _data[index];
    }
}
