using Engine.DataStructures.Moves;
using Engine.Interfaces;

namespace Engine.Sorting.Sorters
{
    public interface IMoveSorter
    {
        //IMove[] Order(IEnumerable<IAttack> attacks, IEnumerable<IMove> moves, IMove pvNode);
        //IMove[] Order(IEnumerable<IAttack> attacks);
        IMove[] Order(AttackList attacks, MoveList moves, IMove pvNode);
        IMove[] Order(AttackList attacks);
    }
}