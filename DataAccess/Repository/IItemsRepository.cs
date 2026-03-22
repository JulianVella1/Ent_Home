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
        List<IItemValidating> GetPendingRestaurants();
        List<IItemValidating> GetApprovedRestaurants();
        List<IItemValidating> GetApprovedMenuItemsByRestaurant(int restId);
        List<IItemValidating> GetRestaurantsByOwner(string email);
        List<IItemValidating> GetPendingMenuItemsForOwnerByRestaurant(string email, int restId);
    }
}
