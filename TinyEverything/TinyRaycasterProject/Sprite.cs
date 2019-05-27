namespace TinyEverything.TinyRaycasterProject
{
    public class Sprite
    {
        public float X;
        public float Y;
        public float PlayerDist;
        public int TextureID;

        public Sprite(float x, float y, float playerDist, int textureId)
        {
            X = x;
            Y = y;
            TextureID = textureId;
            PlayerDist = playerDist;
        }
    }
}
