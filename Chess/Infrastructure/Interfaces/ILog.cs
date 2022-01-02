using Infrastructure.Interfaces.Position;

namespace Infrastructure.Interfaces
{
    public interface ILog
    {
        void Log(IPosition position);
    }
}
