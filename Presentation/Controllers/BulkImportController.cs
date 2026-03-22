using Common.Interfaces;
using Common.Models;
using DataAccess.Factory;
using DataAccess.Repository;
using Microsoft.AspNetCore.Mvc;
using System.IO;
using System.IO.Compression;
using Microsoft.EntityFrameworkCore;
using DataAccess.Context;
using Microsoft.AspNetCore.Authorization;
using Presentation.ActionFilters;

namespace Presentation.Controllers
{
    public class BulkImportController : Controller
    {
        private const string adminEmail = "julian_vella@hotmail.com";
        private readonly RestaurantDbContext _context;

        public BulkImportController(RestaurantDbContext context)
        {
            _context = context;
        }

        public IActionResult Index([FromKeyedServices("cache")] IItemsRepository memory)
        {
            var cacheItems = memory.Get();
            
            var pendingRestaurants = _context.Restaurants
                .Where(r => r.Status == false)
                .ToList();
            
            var pendingMenuItems = _context.MenuItems
                .Where(m => m.Status == false)
                .ToList();
            
            var approvedRestaurants = _context.Restaurants
                .Where(r => r.Status == true)
                .Include(r => r.MenuItems)
                .ToList();

            ViewBag.PendingRestaurants = pendingRestaurants;
            ViewBag.PendingMenuItems = pendingMenuItems;
            ViewBag.ApprovedRestaurants = approvedRestaurants;
            ViewBag.Mode = "bulkimport";
            ViewBag.PageTitle = "Bulk Import";

            return View("Catalog", cacheItems);
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
            TempData["Message"] = $"{items.Count} items imported to cache";

            return RedirectToAction("Index");
        }

        [HttpPost]
        [Authorize]
        [ServiceFilter(typeof(ApproveValidationFilter))]
        public IActionResult Approve(IEnumerable<string> itemIds, [FromKeyedServices("db")] IItemsRepository dbRepo)
        {
            if (itemIds != null && itemIds.Any())
            {
                dbRepo.Approve(itemIds);
                TempData["Message"] = $"{itemIds.Count()} items approved";
                return RedirectToAction("Verification");
            }

            TempData["Message"] = "No items selected for approval";
            return RedirectToAction("Verification");
        }


        [HttpPost]
        public IActionResult DownloadZip([FromKeyedServices("cache")] IItemsRepository memory, [FromServices] IWebHostEnvironment env)
        {
            var items = memory.Get();

            if (items.Count == 0)
            {
                TempData["Message"] = "No items in cache to download";
                return RedirectToAction(nameof(Index));
            }

            using var ms = new MemoryStream();
            using (var zip = new ZipArchive(ms, ZipArchiveMode.Create, true))
            {
     
                var defaultImagePath = Path.Combine(env.WebRootPath ?? string.Empty, "uploads", "default.jpg");

                foreach (var item in items)
                {
                    string id = "";

                    if (item is Restaurant r)
                    {
                        id = r.ExternalId;
                    }
                    else if (item is MenuItem m)
                    {
                        id = m.ExternalId;
                    }
                    else
                    {
                        id = GetIdString(item);
                    }

               
                    var added = false;
                    if (!string.IsNullOrWhiteSpace((item as dynamic).ImagePath as string))
                    {
                        try
                        {
                            var imagePath = ((item as dynamic).ImagePath as string)!.TrimStart('/').Replace('/', Path.DirectorySeparatorChar);
                            var fullImagePath = Path.Combine(env.WebRootPath ?? string.Empty, imagePath);
                            if (System.IO.File.Exists(fullImagePath))
                            {
                                zip.CreateEntryFromFile(fullImagePath, $"item-{id}/default.jpg");
                                added = true;
                            }
                        }
                        catch
                        {
                        }
                    }

                    if (!added)
                    {
                        if (System.IO.File.Exists(defaultImagePath))
                        {
                            zip.CreateEntryFromFile(defaultImagePath, $"item-{id}/default.jpg");
                        }
                        else
                        {
                            var entry = zip.CreateEntry($"item-{id}/default.jpg");
                            using var entryStream = entry.Open();
                            var placeholder = System.Text.Encoding.UTF8.GetBytes("default image");
                            entryStream.Write(placeholder, 0, placeholder.Length);
                        }
                    }
                }
            }

            ms.Position = 0;
            return File(ms.ToArray(), "application/zip", "images-template.zip");
        }

        private string GetIdString(IItemValidating item)
        {
            if (item is Restaurant r) return r.Id.ToString();
            if (item is MenuItem m) return m.Id.ToString();
            return "unknown";
        }

        [HttpPost]
        public IActionResult Commit(IFormFile zipFile, [FromKeyedServices("cache")] IItemsRepository memory, [FromKeyedServices("db")] IItemsRepository db, [FromServices] IWebHostEnvironment env)
        {
            if (zipFile == null || zipFile.Length == 0)
            {
                TempData["Message"] = "No ZIP file uploaded";
                return RedirectToAction("Index");
            }

            var items = memory.Get();
            if (items.Count == 0)
            {
                TempData["Message"] = "No items in cache to commit";
                return RedirectToAction("Index");
            }

            var uploadsPath = Path.Combine(env.WebRootPath, "uploads");
            Directory.CreateDirectory(uploadsPath);

            using (var zip = new ZipArchive(zipFile.OpenReadStream(), ZipArchiveMode.Read))
            {
                foreach (var entry in zip.Entries)
                {
                    var fullName = entry.FullName.Replace('\\', '/');

                    if (!fullName.EndsWith("/default.jpg", StringComparison.OrdinalIgnoreCase))
                        continue;

                    var folderName = fullName.Replace("/default.jpg", "");
                    var lastFolder = folderName.Split('/').Last();
                    var id = lastFolder.Replace("item-", "");

                    var fileName = Guid.NewGuid().ToString() + ".jpg";
                    var savePath = Path.Combine(uploadsPath, fileName);

                    using (var entryStream = entry.Open())
                    using (var fileStream = System.IO.File.Create(savePath))
                    {
                        entryStream.CopyTo(fileStream);
                    }

                    var relativePath = "/uploads/" + fileName;

                    foreach (var item in items)
                    {
                        if (item is Restaurant r && r.ExternalId == id)
                        {
                            r.ImagePath = relativePath;
                        }

                        if (item is MenuItem m && m.ExternalId == id)
                        {
                            m.ImagePath = relativePath;
                        }
                    }
                }
            }

            db.Save(items);
            memory.Save(new List<IItemValidating>());

            TempData["Message"] = $"Committed {items.Count} item(s) to database pending approval";
            return RedirectToAction(nameof(Index));
        }


        [Authorize]
        public IActionResult Verification(int? restId, [FromKeyedServices("db")] IItemsRepository dbRepo)
        {
            var userEmail = User.Identity?.Name;

            if (string.IsNullOrEmpty(userEmail))
                return Forbid();

            List<IItemValidating> items;

            if (userEmail.Equals(adminEmail, StringComparison.OrdinalIgnoreCase))
            {
                items = dbRepo.GetPendingRestaurants();
                ViewBag.PageTitle = "Verification - Pending Restaurants";
                ViewBag.VerificationType = "admin-restaurants";
            }
            else
            {
                if (restId.HasValue)
                {
                    items = dbRepo.GetPendingMenuItemsForOwnerByRestaurant(userEmail, restId.Value);
                    ViewBag.PageTitle = "Verification - Pending Menu Items";
                    ViewBag.VerificationType = "owner-menuitems";
                    ViewBag.SelectedRestaurantId = restId.Value;
                }
                else
                {
                    items = dbRepo.GetRestaurantsByOwner(userEmail);
                    ViewBag.PageTitle = "Verification - My Restaurants";
                    ViewBag.VerificationType = "owner-restaurants";
                }
            }

            ViewBag.Mode = "verification";
            return View("Catalog", items);
        }
    }
}