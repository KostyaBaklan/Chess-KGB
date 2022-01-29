using Engine.Interfaces;

namespace Engine.Strategies.Base
{
    public interface IStrategy
    {
        int Size { get; }
        IResult GetResult();
    }
}
