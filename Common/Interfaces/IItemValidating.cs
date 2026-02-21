using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Interfaces
{
    internal interface IItemValidating
    {
        List<string> GetValidators();
        string GetCardPartial();
    }
}
