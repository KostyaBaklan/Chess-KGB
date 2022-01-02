namespace Infrastructure.Moves
{
    public class Coordinate
    {
        public Coordinate(byte x, byte y)
        {
            X = x;
            Y = y;
            Key = (byte) (x * 8 + y);
        }

        public byte X { get; }
        public byte Y { get; }
        public byte Key { get; }

        #region Overrides of Object

        public override string ToString()
        {
            return $"[{X},{Y}]";
        }

        #endregion
    }
}