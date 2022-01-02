using System.Collections.Generic;
using Infrastructure.Interfaces.Moves;

namespace Infrastructure.Sorters.Comparers
{
    public interface IMoveComparer : IComparer<IMove>
    {
        void Initialize();
    }
}