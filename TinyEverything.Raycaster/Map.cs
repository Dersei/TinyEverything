namespace TinyEverything.Raycaster
{
    public class Map
    {
        public readonly int Width = 16;
        public readonly int Height = 16;

        private char[] _map = ("0000222222220000" +
                                        "1              0" +
                                        "1      11111   0" +
                                        "1     0        0" +
                                        "0     0  1110000" +
                                        "0     3        0" +
                                        "0   10000      0" +
                                        "0   3   11100  0" +
                                        "5   4   0      0" +
                                        "5   4   1  00000" +
                                        "0       1      0" +
                                        "2       1      0" +
                                        "0       0      0" +
                                        "0 0000000      0" +
                                        "0              0" +
                                        "0002222222200000").ToCharArray();

        public int this[int i, int j] => _map[i + j * Width] - '0';

        public bool IsEmpty(int i, int j)
        {
            return _map[i + j * Width] == ' ';
        }

        public void SetMap(char[] array)
        {
            _map = array;
        }


        public void SetMap(string array)
        {
            _map = array.ToCharArray();
        }
    }
}
