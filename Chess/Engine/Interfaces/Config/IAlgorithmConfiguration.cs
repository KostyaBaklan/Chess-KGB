namespace Engine.Interfaces.Config
{
    public interface IAlgorithmConfiguration
    {
        int InitialDepth { get; }
        int NullWindow { get; }
        int AspirationWindow { get; }
        int AspirationDepth { get; }
    }
}