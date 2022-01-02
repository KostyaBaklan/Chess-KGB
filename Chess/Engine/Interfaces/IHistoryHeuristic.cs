namespace Engine.Interfaces
{
    public interface IHistoryHeuristic
    {
        void Update(IMove move);

        int Get(IMove move);
    }
}