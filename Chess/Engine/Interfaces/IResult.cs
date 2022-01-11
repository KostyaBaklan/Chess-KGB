using Engine.DataStructures;

namespace Engine.Interfaces
{
    public interface IResult
    {
        int Value { get; }
        GameResult GameResult { get; }
        IMove Move { get; }
        IMove Cut { get; }
    }
}