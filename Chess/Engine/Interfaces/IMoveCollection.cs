namespace Engine.Interfaces
{
    public interface IMoveCollection
    {
        int Count { get; }
        IMove this[int index] { get; }
    }
}
