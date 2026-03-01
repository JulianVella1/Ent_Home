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

        public void Remove(IItemValidating item)//used to clear only the approved items
        {
            var items = Get();
            items.Remove(item);
            Save(items);
        }
    }
}
