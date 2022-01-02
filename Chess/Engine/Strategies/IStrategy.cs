namespace Engine.Strategies
{
    public interface IStrategy
    {
        int Size { get; }
        IResult GetResult();
    }
}
