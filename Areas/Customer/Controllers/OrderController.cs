using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PepperHouse.Data;
using PepperHouse.Models.ViewModels;
using PepperHouse.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace PepperHouse.Areas.Customer.Controllers
{
    [Area("Customer")]
    [Authorize(Roles = SD.CustomerEndUser)]
    public class OrderController : Controller
    {
        private readonly ApplicationDbContext _db;

        public OrderController(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<IActionResult> Confirm(int id)
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);

            OrderDetailsViewModel orderDetailsViewModel = new OrderDetailsViewModel()
            {
                OrderHeader = await _db.OrderHeader.Include(o => o.ApplicationUser).FirstOrDefaultAsync(o => o.ID == id && o.UserID == claim.Value),
                OrderDetails = await _db.OrderDetails.Where(o => o.OrderID == id).ToListAsync()
            };

            return View(orderDetailsViewModel);
        }
    }
}
