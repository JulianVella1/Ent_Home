using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common.Interfaces;
using Common.Models;

namespace DataAccess.Repository
{
    public class ItemsInMemoryRepository : IItemsRepository
    {
        private readonly IMemoryCache storage;
        private const string key = "items";

        public ItemsInMemoryRepository(IMemoryCache storage)
        {
            this.storage = storage;
        }

        public List<IItemValidating> Get()
        {
            if (storage.TryGetValue(key, out List<IItemValidating> items))
            {
                return items;
            }
            return new List<IItemValidating>();
        }

        public void Save(List<IItemValidating> items)
        {
            storage.Set(key, items);
        }

        public void Approve(IEnumerable<string> itemIds)
        {
            var items = Get();
            var ids = itemIds?.ToList() ?? new List<string>();

            foreach (var item in items)
            {
                if (item is Restaurant restaurant)
                {
                    if (ids.Contains(restaurant.Id.ToString()))
                    {
                        restaurant.Status = true;
                    }
                }
                else if (item is MenuItem menuItem)
                {
                    if (ids.Contains(menuItem.Id.ToString()))
                    {
                        menuItem.Status = true;
                    }
                }
            }

            Save(items);
        }


        public List<IItemValidating> GetPendingRestaurants()
        {
            throw new NotImplementedException();
        }

        public List<IItemValidating> GetApprovedRestaurants()
        {
            throw new NotImplementedException();
        }

        public List<IItemValidating> GetApprovedMenuItemsByRestaurant(int restId)
        {
            throw new NotImplementedException();

        }
        public List<IItemValidating> GetRestaurantsByOwner(string email)
        {
            return Get()
                .OfType<Restaurant>()
                .Where(r => r.OwnerEmail == email)
                .Cast<IItemValidating>()
                .ToList();
        }

        public List<IItemValidating> GetPendingMenuItemsForOwnerByRestaurant(string email, int restId)
        {
            return Get()
                .OfType<MenuItem>()
                .Where(m => !m.Status
                            && m.RestId == restId
                            && m.Restaurant != null
                            && m.Restaurant.OwnerEmail == email)
                .Cast<IItemValidating>()
                .ToList();
        }
    }
}
