using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common.Interfaces;
using Common.Models;
using DataAccess.Context;
using DataAccess.Repository;

namespace DataAccess.Repository
{
    public class ItemsDbRepository: IItemsRepository
    {
        private readonly RestaurantDbContext context;

        public ItemsDbRepository(RestaurantDbContext context)
        {
            this.context = context;
        }

        public List<IItemValidating> Get()
        {
            List<IItemValidating> items = new List<IItemValidating>();
            items.AddRange(context.Restaurants.ToList());
            items.AddRange(context.MenuItems.ToList());

            return items;

        }

        public void Save(List<IItemValidating> items)
        {
            foreach (var item in items)
            {
                if (item is Restaurant r)
                    context.Restaurants.Add(r);

                if (item is MenuItem m)
                    context.MenuItems.Add(m);
            }

            context.SaveChanges();

        
        }

        public void Approve(IEnumerable<string> itemIds)
        {
            foreach(var id in itemIds)
            {
                if (int.TryParse(id, out int restId))
                {
                    Restaurant? restaurant = context.Restaurants.Find(restId);
                    if (restaurant != null)
                    {
                        restaurant.Status = true;
                    }

                }
                else if (Guid.TryParse(id, out Guid itemId))
                { 
                    MenuItem menuItem = context.MenuItems.Find(itemId);
                    if (menuItem != null)
                    {
                        menuItem.Status = true;
                    }
                }
             }

            context.SaveChanges();
        }

        public void Remove(IItemValidating item)
        {
            throw new NotImplementedException();
        }
    }
}
