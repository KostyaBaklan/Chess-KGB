using Infrastructure.Interfaces.Moves;

namespace Algorithms.Interfaces
{
    public interface IResult
    {
        int Value { get; }
        IMove Move { get; }
        IMove Cut { get;  }
    }
}
