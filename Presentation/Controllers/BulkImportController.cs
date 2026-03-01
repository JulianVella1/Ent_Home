using Common.Interfaces;
using Common.Models;
using DataAccess.Factory;
using DataAccess.Repository;
using Microsoft.AspNetCore.Mvc;

namespace Presentation.Controllers
{
    public class BulkImportController : Controller
    {
        public IActionResult Index([FromKeyedServices("cache")] IItemsRepository memory)
        {
            var items = memory.Get();
            return View("Catalog", items);
        }

        [HttpPost]
        public IActionResult BulkImport(IFormFile file, [FromKeyedServices("cache")] IItemsRepository memory, ImportItemFactory importItem)
        {
            if (file == null)
            {
                TempData["Message"] = "No file uploaded";
                return RedirectToAction("Index");
            }

            string json;

            using (var r = new StreamReader(file.OpenReadStream()))
            {
                json = r.ReadToEnd();
            }
            var items = importItem.Create(json);

            if (items == null)
            {
                TempData["Message"] = "Invalid JSON format";
                return RedirectToAction("Index");
            }

            memory.Save(items);
            TempData["Message"] = $"{items.Count} items imported";

            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult Approve(IEnumerable<string> itemIds, [FromKeyedServices("cache")] IItemsRepository memory)
        {
            if (itemIds != null && itemIds.Any())
            {
                memory.Approve(itemIds);
                TempData["Message"] = $"Cached {itemIds.Count()} items approved";
                return RedirectToAction("Index");
            }

            TempData["Message"] = "No items selected for approval";
            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult Commit([FromKeyedServices("cache")] IItemsRepository memory, [FromKeyedServices("db")] IItemsRepository db)
        {
            var items = memory.Get().Where(i => (i is Restaurant rest && rest.Status) || (i is MenuItem menuItem && menuItem.Status)).ToList();

            if (items.Count == 0)
            {
                TempData["Message"] = "No items to commit";
                return RedirectToAction("Index");
            }

            db.Save(items);

            foreach (var removeItem in items)
            {
                memory.Remove(removeItem);
            }

            TempData["Message"] = $"Committed {items.Count} items to db";
            return RedirectToAction("Index");
        }
    }
}
