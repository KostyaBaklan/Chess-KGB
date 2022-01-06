using Engine.Models.Enums;

namespace Engine.Interfaces
{
    public interface IAttack : IMove
    {
        Piece Captured { get; set; }
    }
}