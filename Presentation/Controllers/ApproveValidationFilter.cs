using DataAccess.Context;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;

namespace Presentation.ActionFilters
{
    public class ApproveValidationFilter : ActionFilterAttribute
    {
        private readonly RestaurantDbContext _context;

        public ApproveValidationFilter(RestaurantDbContext context)
        { _context = context; }
        

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var userEmail = context.HttpContext.User.Identity?.Name;

            if (string.IsNullOrWhiteSpace(userEmail))
            {
                context.Result = new ForbidResult();
                return;
            }

            if (!context.ActionArguments.ContainsKey("itemIds"))
            {
                context.Result = new BadRequestObjectResult("No items received.");
                return;
            }

            var itemIds = context.ActionArguments["itemIds"] as IEnumerable<string>;

            if (itemIds == null || !itemIds.Any())
            {
                return;
            }

            foreach (var id in itemIds)
            {
                if (int.TryParse(id, out int restaurantId))
                {
                    var restaurant = _context.Restaurants.FirstOrDefault(r => r.Id == restaurantId);

                    if (restaurant == null)
                    {
                        context.Result = new ForbidResult();
                        return;
                    }

                    var validators = restaurant.GetValidators();

                    if (!validators.Any(v => v.Equals(userEmail, StringComparison.OrdinalIgnoreCase)))
                    {
                        context.Result = new ForbidResult();
                        return;
                    }
                }
                else if (Guid.TryParse(id, out Guid menuItemId))
                {
                    var menuItem = _context.MenuItems
                        .Include(m => m.Restaurant)
                        .FirstOrDefault(m => m.Id == menuItemId);

                    if (menuItem == null)
                    {
                        context.Result = new ForbidResult();
                        return;
                    }

                    var validators = menuItem.GetValidators();

                    if (!validators.Any(v => v.Equals(userEmail, StringComparison.OrdinalIgnoreCase)))
                    {
                        context.Result = new ForbidResult();
                        return;
                    }
                }
                else
                {
                    context.Result = new ForbidResult();
                    return;
                }
            }
        }
    }
}