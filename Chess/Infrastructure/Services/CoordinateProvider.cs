using Infrastructure.Interfaces.Position;
using Infrastructure.Moves;

namespace Infrastructure.Services
{
    public class CoordinateProvider: ICoordinateProvider
    {
        private readonly string _x = "abcdefgh";
        private readonly string _y = "12345678";
        private readonly Coordinate[] _coordinates;

        public CoordinateProvider()
        {
            _coordinates = new Coordinate[64];
            for (byte i = 0; i < 8; i++)
            {
                for (byte j = 0; j < 8; j++)
                {
                    _coordinates[i*8+j] = new Coordinate(i,j);
                }
            }
        }

        #region Implementation of ICoordinateProvider

        public Coordinate GetCoordinate(int x, int y)
        {
            return _coordinates[x*8+ y];
        }

        public Coordinate GetCoordinate(string cell)
        {
            return _coordinates[_x.IndexOf(cell[0]) * 8 + _y.IndexOf(cell[1])];
        }

        public Coordinate GetCoordinate(int k)
        {
            return _coordinates[k];
        }

        #endregion
    }
}