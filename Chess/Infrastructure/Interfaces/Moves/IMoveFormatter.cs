using Infrastructure.Models.Enums;

namespace Infrastructure.Interfaces.Moves
{
    public interface IMoveFormatter
    {
        string Format(IMove move);
        IMove Parse(string move, Turn turn);
    }
}
