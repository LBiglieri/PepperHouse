using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PepperHouse.Data;
using PepperHouse.Models.ViewModels;
using PepperHouse.Utility;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace PepperHouse.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class MenuItemController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly IWebHostEnvironment _webHostEnvironment;

        [BindProperty]
        public MenuItemViewModel MenuItemVM { get; set; }
        public MenuItemController(ApplicationDbContext db, IWebHostEnvironment webHostEnvironment)
        {
            _db = db;
            _webHostEnvironment = webHostEnvironment;
            MenuItemVM = new MenuItemViewModel()
            {
                Category = _db.Category,
                MenuItem = new Models.MenuItem()
            };
        }
        public async Task<IActionResult> Index()
        {
            var menuItems = await _db.MenuItem.Include(m=>m.Category).Include(m=>m.SubCategory).ToListAsync();
            return View(menuItems);
        }

        //GET - Create
        public IActionResult Create()
        {
            return View(MenuItemVM);
        }

        [HttpPost,ActionName("Create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreatePOST()
        {
            MenuItemVM.MenuItem.SubCategoryID = Convert.ToInt32(Request.Form["SubCategoryID"].ToString());
            if (!ModelState.IsValid)
            {
                return View(MenuItemVM);
            }

            _db.MenuItem.Add(MenuItemVM.MenuItem);
            await _db.SaveChangesAsync();

            //work on the image saving

            string webRootPath = _webHostEnvironment.WebRootPath;
            var files = HttpContext.Request.Form.Files;

            var menuItemFromDB = await _db.MenuItem.FindAsync(MenuItemVM.MenuItem.ID);

            if (files.Count > 0)
            {
                //files has been uploaded
                var uploads = Path.Combine(webRootPath, "images");
                var extension = Path.GetExtension(files[0].FileName);

                using (var fileStream = new FileStream(Path.Combine(uploads,MenuItemVM.MenuItem.ID + extension), FileMode.Create))
                {
                    files[0].CopyTo(fileStream);
                }
                menuItemFromDB.Image = @"\images\" + MenuItemVM.MenuItem.ID + extension;
            }
            else
            {
                //no file has been uploaded
                var uploads = Path.Combine(webRootPath, @"images\" + SD.DefaultFoodImage);
                System.IO.File.Copy(uploads, webRootPath + @"\images\" + MenuItemVM.MenuItem.ID + ".png");
                menuItemFromDB.Image = @"\images\" + MenuItemVM.MenuItem.ID + ".png";
            }

            await _db.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        //GET - Edit
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            MenuItemVM.MenuItem = await _db.MenuItem.Include(m => m.Category).Include(m => m.SubCategory).SingleOrDefaultAsync(m => m.ID == id);
            MenuItemVM.SubCategory = await _db.SubCategory.Where(s => s.CategoryID == MenuItemVM.MenuItem.CategoryID).ToListAsync();

            if (MenuItemVM.MenuItem == null)
            {
                return NotFound();
            }
            return View(MenuItemVM);
        }

        [HttpPost, ActionName("Edit")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditPOST(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            MenuItemVM.MenuItem.SubCategoryID = Convert.ToInt32(Request.Form["SubCategoryID"].ToString());
            
            if (!ModelState.IsValid)
            {
                MenuItemVM.SubCategory = await _db.SubCategory.Where(s => s.CategoryID == MenuItemVM.MenuItem.CategoryID).ToListAsync();
                return View(MenuItemVM);
            }
            string webRootPath = _webHostEnvironment.WebRootPath;
            var files = HttpContext.Request.Form.Files;

            var menuItemFromDB = await _db.MenuItem.FindAsync(MenuItemVM.MenuItem.ID);

            if (files.Count > 0)
            {
                //New image has been uploaded
                var uploads = Path.Combine(webRootPath, "images");
                var extension_new = Path.GetExtension(files[0].FileName);

                //Delete original file 
                var imagePath = Path.Combine(webRootPath, menuItemFromDB.Image.TrimStart('\\'));
                if (System.IO.File.Exists(imagePath))
                {
                    System.IO.File.Delete(imagePath);
                }

                //upload new image
                using (var fileStream = new FileStream(Path.Combine(uploads, MenuItemVM.MenuItem.ID + extension_new), FileMode.Create))
                {
                    files[0].CopyTo(fileStream);
                }
                menuItemFromDB.Image = @"\images\" + MenuItemVM.MenuItem.ID + extension_new;
            }

            menuItemFromDB.Name = MenuItemVM.MenuItem.Name;
            menuItemFromDB.Description = MenuItemVM.MenuItem.Description;
            menuItemFromDB.Price = MenuItemVM.MenuItem.Price;
            menuItemFromDB.Hotness = MenuItemVM.MenuItem.Hotness;
            menuItemFromDB.CategoryID = MenuItemVM.MenuItem.CategoryID;
            menuItemFromDB.SubCategoryID = MenuItemVM.MenuItem.SubCategoryID;

            await _db.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        //GET - Details
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            MenuItemVM.MenuItem = await _db.MenuItem.Include(m => m.Category).Include(m => m.SubCategory).SingleOrDefaultAsync(m => m.ID == id);
            MenuItemVM.SubCategory = await _db.SubCategory.Where(s => s.CategoryID == MenuItemVM.MenuItem.CategoryID).ToListAsync();

            if (MenuItemVM.MenuItem == null)
            {
                return NotFound();
            }
            return View(MenuItemVM);
        }

        //GET - Delete
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            MenuItemVM.MenuItem = await _db.MenuItem.Include(m => m.Category).Include(m => m.SubCategory).SingleOrDefaultAsync(m => m.ID == id);
            MenuItemVM.SubCategory = await _db.SubCategory.Where(s => s.CategoryID == MenuItemVM.MenuItem.CategoryID).ToListAsync();

            if (MenuItemVM.MenuItem == null)
            {
                return NotFound();
            }
            return View(MenuItemVM);
        }

        //POST - Delete
        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            if (!ModelState.IsValid)
            {
                return NotFound();
            }

            var menuItemFromDB = await _db.MenuItem.SingleOrDefaultAsync(m => m.ID == id);

            if (menuItemFromDB == null)
            {
                return NotFound();
            }

            string webRootPath = _webHostEnvironment.WebRootPath;
            var files = HttpContext.Request.Form.Files;


            if (files.Count > 0)
            {
                //Delete image file
                var imagePath = Path.Combine(webRootPath, menuItemFromDB.Image.TrimStart('\\'));
                if (System.IO.File.Exists(imagePath))
                {
                    System.IO.File.Delete(imagePath);
                }
            }

            _db.MenuItem.Remove(menuItemFromDB);

            await _db.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
    }
}
