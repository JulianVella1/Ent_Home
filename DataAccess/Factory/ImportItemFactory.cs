using Common.Interfaces;
using Common.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace DataAccess.Factory
{
    public class ImportItemFactory
    {

        public List<IItemValidating> Create(string json)
        {
            var list = new List<IItemValidating>();
            var js = JsonSerializer.Deserialize<List<JsonElement>>(json);

            if (js == null)
                return list;

            foreach (var item in js)
            {
                var type = item.GetProperty("type").GetString()?.ToLower();

                if (type == "menuitem")
                    list.Add(CreateMenuitem(item));//status for both will be set to false
                else if (type == "restaurant")
                    list.Add(CreateRestaurant(item));
                else
                    continue;
            }
            return list;
        }

        private Restaurant CreateRestaurant(JsonElement item)
        {
            return new Restaurant
            {
                Name = item.GetProperty("name").GetString() ?? "",
                OwnerEmail = item.GetProperty("ownerEmailAddress").GetString() ?? "",
                
            };
        }

        private MenuItem CreateMenuitem(JsonElement item)
        {
            return new MenuItem
            {
                Id = Guid.NewGuid(),
                Title = item.GetProperty("title").GetString() ?? "",
                Price = item.GetProperty("price").GetDecimal(),
                RestId = item.GetProperty("restaurantId").GetInt32(),
            };
        }
    }
}
