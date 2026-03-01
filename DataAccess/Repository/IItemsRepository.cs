using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common.Interfaces;

namespace DataAccess.Repository
{
    public interface IItemsRepository
    {
        List<IItemValidating> Get();
        void Save(List<IItemValidating> items);
        void Approve(IEnumerable<string> itemIds);
        void Remove(IItemValidating item);
    }
}
