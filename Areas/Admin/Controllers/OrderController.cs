using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PepperHouse.Data;
using PepperHouse.Models;
using PepperHouse.Models.ViewModels;
using PepperHouse.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PepperHouse.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class OrderController : Controller
    {
        private readonly ApplicationDbContext _db;
        public readonly IEmailSender _emailSender;
        private int PageSize = 10;
        public OrderController(ApplicationDbContext db, IEmailSender emailSender)
        {
            _db = db;
            _emailSender = emailSender;
        }
        
        [Authorize(Roles = SD.ManagerUser + "," + SD.KitchenUser)]
        public async Task<IActionResult> ManageOrder()
        {

            List<OrderDetailsViewModel> orderDetailsVM = new List<OrderDetailsViewModel>();


            List<OrderHeader> OrderHeaderList = await _db.OrderHeader
                .Where(o=>o.Status == SD.StatusSubmitted || o.Status == SD.StatusInProcess)
                .OrderByDescending(o=>o.PickupTime).ToListAsync();

            foreach (OrderHeader item in OrderHeaderList)
            {
                OrderDetailsViewModel individual = new OrderDetailsViewModel
                {
                    OrderHeader = item,
                    OrderDetails = await _db.OrderDetails.Where(o => o.OrderID == item.ID).ToListAsync()
                };
                orderDetailsVM.Add(individual);
            }

            return View(orderDetailsVM.OrderByDescending(o => o.OrderHeader.PickupTime).ToList());
        }

        [Authorize(Roles = SD.KitchenUser + "," + SD.ManagerUser)]
        public async Task<IActionResult> OrderPrepare(int OrderId)
        {
            OrderHeader orderHeader = await _db.OrderHeader.FindAsync(OrderId);
            orderHeader.Status = SD.StatusInProcess;
            await _db.SaveChangesAsync();
            return RedirectToAction("ManageOrder", "Order");
        }

        [Authorize(Roles = SD.KitchenUser + "," + SD.ManagerUser)]
        public async Task<IActionResult> OrderReady(int OrderId)
        {
            OrderHeader orderHeader = await _db.OrderHeader.FindAsync(OrderId);
            orderHeader.Status = SD.StatusReady;
            await _db.SaveChangesAsync();

            await _emailSender.SendEmailAsync(_db.Users.Where(u => u.Id == orderHeader.UserID).FirstOrDefault().Email, "PepperHouse - Order Ready for Pickup - ID:" + orderHeader.ID.ToString(), "Your Order is ready for Pickup.");

            return RedirectToAction("ManageOrder", "Order");
        }

        [Authorize(Roles = SD.KitchenUser + "," + SD.ManagerUser)]
        public async Task<IActionResult> OrderCancel(int OrderId)
        {
            OrderHeader orderHeader = await _db.OrderHeader.FindAsync(OrderId);
            orderHeader.Status = SD.StatusCancelled;
            await _emailSender.SendEmailAsync(_db.Users.Where(u => u.Id == orderHeader.UserID).FirstOrDefault().Email, "PepperHouse - Order Cancelled - ID:" + orderHeader.ID.ToString(), "Your Order has been cancelled.");
            await _db.SaveChangesAsync();
            return RedirectToAction("ManageOrder", "Order");
        }

        [Authorize(Roles = SD.FrontDeskUser + "," + SD.ManagerUser)]
        public async Task<IActionResult> OrderPickup(int productPage = 1, string searchEmail = null, string searchPhone = null, string searchName = null)
        {
            //var claimsIdentity = (ClaimsIdentity)User.Identity;
            //var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);

            OrderListViewModel orderListVM = new OrderListViewModel()
            {
                Orders = new List<OrderDetailsViewModel>()
            };

            StringBuilder param = new StringBuilder();
            param.Append("/Customer/Order/OrderPickup?productPage=:");
            param.Append("&searchName=");
            if (searchName != null)
            {
                param.Append(searchName);
            }
            param.Append("&searchEmail=");
            if (searchEmail != null)
            {
                param.Append(searchEmail);
            }
            param.Append("&searchPhone=");
            if (searchPhone != null)
            {
                param.Append(searchPhone);
            }

            List<OrderHeader> OrderHeaderList = new List<OrderHeader>();
            if (searchName != null || searchEmail != null || searchPhone != null)
            {
                var user = new ApplicationUser();

                if (searchName != null)
                {
                    OrderHeaderList = await _db.OrderHeader.Include(o => o.ApplicationUser)
                                                .Where(u => u.PickupName.ToLower().Contains(searchName.ToLower()))
                                                .OrderByDescending(o => o.OrderDate).ToListAsync();
                }
                else
                {
                    if (searchEmail != null)
                    {
                        user = await _db.ApplicationUser.Where(u => u.Email.ToLower().Contains(searchEmail.ToLower())).FirstOrDefaultAsync();
                        OrderHeaderList = await _db.OrderHeader.Include(o => o.ApplicationUser)
                                                    .Where(o => o.UserID == user.Id)
                                                    .OrderByDescending(o => o.OrderDate).ToListAsync();
                    }
                    else
                    {
                        if (searchPhone != null)
                        {
                            OrderHeaderList = await _db.OrderHeader.Include(o => o.ApplicationUser)
                                                        .Where(u => u.PhoneNumber.Contains(searchPhone))
                                                        .OrderByDescending(o => o.OrderDate).ToListAsync();
                        }
                    }
                }
            }
            else
            {
                OrderHeaderList = await _db.OrderHeader.Include(o => o.ApplicationUser).Where(u => u.Status == SD.StatusReady).ToListAsync();
            }

            foreach (OrderHeader item in OrderHeaderList)
            {
                OrderDetailsViewModel individual = new OrderDetailsViewModel
                {
                    OrderHeader = item,
                    OrderDetails = await _db.OrderDetails.Where(o => o.OrderID == item.ID).ToListAsync()
                };
                orderListVM.Orders.Add(individual);
            }



            var count = orderListVM.Orders.Count;
            orderListVM.Orders = orderListVM.Orders.OrderByDescending(p => p.OrderHeader.ID)
                                 .Skip((productPage - 1) * PageSize)
                                 .Take(PageSize).ToList();

            orderListVM.PagingInfo = new PagingInfo
            {
                CurrentPage = productPage,
                ItemsPerPage = PageSize,
                TotalItem = count,
                urlParam = param.ToString()
            };

            return View(orderListVM);
        }

        [Authorize(Roles = SD.FrontDeskUser + "," + SD.ManagerUser)]
        [HttpPost]
        [ActionName("OrderPickup")]
        public async Task<IActionResult> OrderPickupPost(int orderId)
        {
            OrderHeader orderHeader = await _db.OrderHeader.FindAsync(orderId);
            orderHeader.Status = SD.StatusCompleted;
            await _db.SaveChangesAsync();

            await _emailSender.SendEmailAsync(_db.Users.Where(u => u.Id == orderHeader.UserID).FirstOrDefault().Email, "PepperHouse - Order Completed - ID:" + orderHeader.ID.ToString(), "Your Order has been completed succesfully!");

            return RedirectToAction("OrderPickup", "Order");
        }
    }
}
