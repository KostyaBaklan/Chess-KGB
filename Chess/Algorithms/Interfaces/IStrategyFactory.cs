namespace Algorithms.Interfaces
{
    public interface IStrategyFactory
    {
        IStrategy GetStrategy(string name);
    }
}
