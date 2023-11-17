using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Norboev_Asilbek_HW5.DAL;
using Norboev_Asilbek_HW5.Models;

namespace Norboev_Asilbek_HW5.Controllers
{
    [Authorize]
    public class OrdersController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<AppUser> _userManager;

        public OrdersController(AppDbContext context, UserManager<AppUser> userManger)
        {
            _context = context;
            _userManager = userManger;

        }

        // GET: Orders
        public IActionResult Index()
        {
            List<Order> orders;
            if (User.IsInRole("Admin"))
            {
                orders = _context.Orders
                                .Include(r => r.OrderDetails)
                                .ToList();
            }
            else //user is a customer, so only display their records
            //registration is assocated with a particular user (look on the registration model class)
            //every logged in user is allowed to access index page, but their results will be different
            {
                orders = _context.Orders
                                .Include(r => r.OrderDetails)
                                .Where(r => r.User.UserName == User.Identity.Name)
                                .ToList();
            }

            //
            return View(orders);

        }

        // GET: Orders/Details/5
        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return View("Error", new String[] { "Please specify a registration to view!" });
            }

            //find the registration in the database
            Order order = await _context.Orders
                                              .Include(r => r.OrderDetails)
                                              .ThenInclude(r => r.Product)
                                              .Include(r => r.User)
                                              .FirstOrDefaultAsync(m => m.OrderID == id);

            //registration was not found in the database
            if (order == null)
            {
                return View("Error", new String[] { "This registration was not found!" });
            }

            //make sure a customer isn't trying to look at someone else's order
            if (User.IsInRole("Admin") == false &&  order.User.UserName != User.Identity.Name)
            {
                return View("Error", new string[] { "You are not authorized to edit this order!" });
            }

            return View(order);
        }

        // GET: Orders/Create
        [Authorize(Roles = "Customer")]
        public IActionResult Create()
        {
            return View();
        }

        // POST: Orders/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [Authorize(Roles = "Customer")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("User, OrderNotes")] Order order)
        {
            //[Bind("OrderID,OrderNumber,OrderDate,OrderNotes")
            order.OrderNumber = Utilities.GenerateNextOrderNumber.GetNextOrderNumber(_context);
            order.OrderDate = DateTime.Now;
            //order.User = await _userManager.FindByEmailAsync("UserName");
            order.User = await _userManager.FindByNameAsync(User.Identity.Name);

            _context.Add(order);
            await _context.SaveChangesAsync();

            return RedirectToAction("Create", "OrderDetails", new { orderID = order.OrderID });
        }

        // GET: Orders/Edit/5
        [Authorize]
        public async Task<IActionResult> Edit(int? id)
        {
            if (User.IsInRole("Admin"))
            {
                return View("Error", new String[] { "You are not authorized to edit this order!" });
            }

            if (id == null || _context.Orders == null)
            {
                return NotFound();
            }

            var order = _context.Orders
                .Include(r => r.OrderDetails)
                .ThenInclude(r => r.Product)
                .Include(r => r.User)
                .FirstOrDefault(r => r.OrderID == id);
            if (order == null)
            {
                return NotFound();
            }
            return View(order);
        }

        // POST: Orders/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("OrderID,OrderNumber,OrderDate,OrderNotes")] Order order)
        {
            //this is a security measure to make sure the user is editing the correct registration
            if (id != order.OrderID)
            {
                return View("Error", new String[] { "There was a problem editing this order. Try again!" });
            }

            //if there is something wrong with this order, try again
            if (ModelState.IsValid == false)
            {
                return View(order);
            }

            //if code gets this far, update the record
            try
            {
                //find the record in the database
                Order dbOrder = _context.Orders.Find(order.OrderID);

                //update the notes
                dbOrder.OrderNotes = order.OrderNotes;

                _context.Update(dbOrder);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                return View("Error", new String[] { "There was an error updating this order!", ex.Message });
            }

            //send the user to the Registrations Index page.
            return RedirectToAction(nameof(Index));
        }


        private bool OrderExists(int id)
        {
          return (_context.Orders?.Any(e => e.OrderID == id)).GetValueOrDefault();
        }
    }
}
