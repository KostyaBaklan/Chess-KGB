using Infrastructure.Moves;

namespace Infrastructure.Interfaces.Position
{
    public interface ICoordinateProvider
    {
        Coordinate GetCoordinate(int x, int y);
        Coordinate GetCoordinate(string cell);
        Coordinate GetCoordinate(int k);
    }
}