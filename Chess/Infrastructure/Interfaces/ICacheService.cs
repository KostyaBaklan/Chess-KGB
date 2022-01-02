namespace Infrastructure.Interfaces
{
    public interface ICacheService
    {
        int Size { get; }
        void Clear();
    }
}