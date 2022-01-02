namespace Infrastructure.Interfaces.Moves
{
    public interface IMoveSorterFactory
    {
        IMoveSorter GetMoveSorter();
    }
}