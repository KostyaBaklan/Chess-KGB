namespace Engine.Interfaces
{
    public interface IMoveCollection
    {
        int Count { get; }
        int Pv { get; }
        int Cut { get; }
        int All { get; }
        int Late { get; }
        int Bad { get; }
        IMove this[int index] { get; }
        bool IsLmr(int i);
    }
}
