using Common.Interfaces;
using DataAccess.Repository;
using Microsoft.AspNetCore.Mvc;

namespace Presentation.Controllers
{
    public class ItemsController : Controller
    {
        public IActionResult Catalog(int? restId, [FromKeyedServices("db")] IItemsRepository dbRepo)
        {
            List<IItemValidating> items;

            if (restId.HasValue)
            {
                items = dbRepo.GetApprovedMenuItemsByRestaurant(restId.Value);
                ViewBag.PageTitle = "Approved Menu Items";
                ViewBag.ViewType = "menuitems";
            }
            else
            {
                items = dbRepo.GetApprovedRestaurants();
                ViewBag.PageTitle = "Approved Restaurants";
                ViewBag.ViewType = "restaurants";
            }

            ViewBag.Mode = "public";
            return View("~/Views/BulkImport/Catalog.cshtml", items);
        }
    }
}