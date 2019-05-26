namespace TinyEverything.TinyRaycasterProject
{
    internal static class ColorUtils
    {
        public static uint PackColor(byte r, byte g, byte b, byte a = 255)
        {
            return (((uint)a << 24) + ((uint)b << 16) + ((uint)g << 8) + r);
        }

        public static void UnpackColor(uint color, out byte r, out byte g, out byte b, out byte a)
        {
            r = (byte)((color >> 0) & 255);
            g = (byte)((color >> 8) & 255);
            b = (byte)((color >> 16) & 255);
            a = (byte)((color >> 24) & 255);
        }
    }
}
