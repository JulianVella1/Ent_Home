using Common.Interfaces;
using Common.Models;
using DataAccess.Context;
using DataAccess.Repository;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
            items.AddRange(context.Restaurants.Where(r => !r.Status).ToList());
            items.AddRange(context.MenuItems.Where(m => !m.Status).ToList());

            return items;
        }

        public void Save(List<IItemValidating> items)
        {
            var restaurants = items.OfType<Restaurant>().ToList();
            var menuItems = items.OfType<MenuItem>().ToList();

            foreach (var r in restaurants)
            {
                var existingRestaurant = context.Restaurants
                    .FirstOrDefault(existing => existing.ExternalId == r.ExternalId);

                if (existingRestaurant != null)
                {
                    var changed = false;

                  
                    if (!string.IsNullOrWhiteSpace(r.Name) && existingRestaurant.Name != r.Name)
                    {
                        existingRestaurant.Name = r.Name;
                        changed = true;
                    }

                    if (!string.IsNullOrWhiteSpace(r.OwnerEmail) && existingRestaurant.OwnerEmail != r.OwnerEmail)
                    {
                        existingRestaurant.OwnerEmail = r.OwnerEmail;
                        changed = true;
                    }

                    
                    if (!string.IsNullOrWhiteSpace(r.ImagePath))
                    {
                        existingRestaurant.ImagePath = r.ImagePath;
                    }

                 
                    if (changed)
                    {
                        existingRestaurant.Status = false;
                    }
                }
                else
                {
                   
                    r.Status = false;
                    context.Restaurants.Add(r);
                }
            }

            context.SaveChanges();

            foreach (var m in menuItems)
            {
                var existingMenuItem = context.MenuItems
                    .FirstOrDefault(existing => existing.ExternalId == m.ExternalId);

                var restaurant = context.Restaurants
                    .FirstOrDefault(r => r.ExternalId == m.RestaurantExternalId);

                if (restaurant == null)
                    continue;

                if (existingMenuItem != null)
                {
                    var changed = false;
                    var newRestId = restaurant.Id;

                  
                    if (!string.IsNullOrWhiteSpace(m.Title) && existingMenuItem.Title != m.Title)
                    {
                        existingMenuItem.Title = m.Title;
                        changed = true;
                    }

                    if (existingMenuItem.Price != m.Price)
                    {
                        existingMenuItem.Price = m.Price;
                        changed = true;
                    }

                    if (existingMenuItem.RestId != newRestId)
                    {
                        existingMenuItem.RestId = newRestId;
                        changed = true;
                    }

                    
                    if (!string.IsNullOrWhiteSpace(m.ImagePath))
                    {
                        existingMenuItem.ImagePath = m.ImagePath;
                    }

                 
                    if (changed)
                    {
                        existingMenuItem.Status = false;
                    }
                }
                else
                {
                    
                    m.Status = false;
                    m.RestId = restaurant.Id;
                    context.MenuItems.Add(m);
                }
            }

            context.SaveChanges();
        }

        public void Approve(IEnumerable<string> itemIds)
        {
            foreach (var id in itemIds)
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
                    MenuItem? menuItem = context.MenuItems.Find(itemId);
                    if (menuItem != null)
                    {
                        menuItem.Status = true;
                    }
                }
            }

            context.SaveChanges();
        }

        public List<IItemValidating> GetPendingRestaurants()
        {
            return context.Restaurants
                .Where(r => !r.Status)
                .Cast<IItemValidating>()
                .ToList();
        }


        public List<IItemValidating> GetApprovedRestaurants()
        {
            return context.Restaurants
                .Where(r => r.Status)
                .Cast<IItemValidating>()
                .ToList();
        }

        public List<IItemValidating> GetApprovedMenuItemsByRestaurant(int restId)
        {
            return context.MenuItems
                .Include(m => m.Restaurant)
                .Where(m => m.Status && m.RestId == restId)
                .Cast<IItemValidating>()
                .ToList();
        }

        public List<IItemValidating> GetRestaurantsByOwner(string email)
        {
            return context.Restaurants
                .Where(r => r.OwnerEmail == email)
                .Cast<IItemValidating>()
                .ToList();
        }

        public List<IItemValidating> GetPendingMenuItemsForOwnerByRestaurant(string email, int restId)
        {
            return context.MenuItems
                .Include(m => m.Restaurant)
                .Where(m => !m.Status
                            && m.RestId == restId
                            && m.Restaurant != null
                            && m.Restaurant.OwnerEmail == email)
                .Cast<IItemValidating>()
                .ToList();
        }
    }
}
