namespace Algorithms.Interfaces
{
    public interface IStrategy
    {
        int Size { get; }
        IResult GetResult();
    }
}
