using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Engine.Interfaces
{
    public interface IOpeningService
    {
        IDictionary<string, ICollection<string>> GetSequences();
        IEnumerable<ICollection<short>> GetMoveKeys();
    }
}
