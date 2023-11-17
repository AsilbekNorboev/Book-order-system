using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Norboev_Asilbek_HW5.DAL;
using Norboev_Asilbek_HW5.Models;

namespace Norboev_Asilbek_HW5.Controllers
{
    public class OrderDetailsController : Controller
    {
        private readonly AppDbContext _context;

        public OrderDetailsController(AppDbContext context)
        {
            _context = context;
        }

        // GET: OrderDetails
        public async Task<IActionResult> Index(int? orderID)
        {
            if (orderID == null)
            {
                return View("Error", new String[] { "Please specify a order to view!" });
            }

            //limit the list to only the registration details that belong to this registration
            List<OrderDetail> rds = _context.OrderDetails
                                          .Include(rd => rd.Product)
                                          .Where(rd => rd.Order.OrderID == orderID)
                                          .ToList();
            //pass to views/orders/index
            //return View("Orders",rds);
            return View(rds);
        }

        //// GET: OrderDetails/Details/5
        //public async Task<IActionResult> Details(int? id)
        //{
        //    if (id == null || _context.OrderDetails == null)
        //    {
        //        return NotFound();
        //    }

        //    var orderDetail = await _context.OrderDetails
        //        .FirstOrDefaultAsync(m => m.OrderDetailID == id);
        //    if (orderDetail == null)
        //    {
        //        return NotFound();
        //    }

        //    return View(orderDetail);
        //}

        // GET: OrderDetails/Create
        public IActionResult Create(int orderID)
        {
            OrderDetail od = new OrderDetail();

            Order dbOrder = _context.Orders.Find(orderID);
            od.Order = dbOrder;

            ViewBag.AllProducts = GetAllProducts();
            return View(od);
        }

        // POST: OrderDetails/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(OrderDetail orderDetail, int SelectedProduct)
        {
            if(orderDetail.Quantity == null || SelectedProduct == null)
            {
                ViewBag.AllProducts = GetAllProducts();
                return View(orderDetail);
            }
            Product dbproduct = _context.Products.Find(SelectedProduct);

            orderDetail.Product = dbproduct;

            Order dbOrder = _context.Orders.Find(orderDetail.Order.OrderID);

            orderDetail.Order = dbOrder;
            orderDetail.ProductPrice = dbproduct.Price;
            orderDetail.ExtendedPrice = orderDetail.ProductPrice * orderDetail.Quantity;

            _context.Add(orderDetail);
            await _context.SaveChangesAsync();

            return RedirectToAction("Details", "Orders", new { id = orderDetail.Order.OrderID });
        }

        // GET: OrderDetails/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.OrderDetails == null)
            {
                return NotFound();
            }

            var orderDetail = await _context.OrderDetails.FindAsync(id);
            if (orderDetail == null)
            {
                return NotFound();
            }
            return View(orderDetail);
        }

        // POST: OrderDetails/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, OrderDetail orderDetail)
        {
            //[Bind("OrderDetailID,Quantity,ProductPrice,ExtendedPrice")]
            if (id != orderDetail.OrderDetailID)
            {
                return View("Error", new String[] { "There was a problem editing this record. Try again!" });
            }

            //create a new registration detail
            OrderDetail dbRD;
            //if code gets this far, update the record
            try
            {
                //find the existing registration detail in the database
                //include both registration and course
                dbRD = _context.OrderDetails
                      .Include(rd => rd.Product)
                      .Include(rd => rd.Order)
                      .FirstOrDefault(rd => rd.OrderDetailID == orderDetail.OrderDetailID);

                //information is not valid, try again
                if (orderDetail.Quantity == null)
                {
                    return View(orderDetail);
                }

                //update the scalar properties
                dbRD.Quantity = orderDetail.Quantity;
                dbRD.ProductPrice = dbRD.ProductPrice;
                dbRD.ExtendedPrice = dbRD.Quantity * dbRD.ProductPrice;

                //save changes
                _context.Update(dbRD);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                return View("Error", new String[] { "There was a problem editing this record", ex.Message });
            }

            //if code gets this far, go back to the registration details index page
            return RedirectToAction("Details", "Orders", new { id = dbRD.Order.OrderID });
        }

        // GET: OrderDetails/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.OrderDetails == null)
            {
                return NotFound();
            }

            var orderDetail = await _context.OrderDetails
                .FirstOrDefaultAsync(m => m.OrderDetailID == id);
            if (orderDetail == null)
            {
                return NotFound();
            }

            return View(orderDetail);
        }

        // POST: OrderDetails/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            OrderDetail orderDetail = await _context.OrderDetails
                                                   .Include(r => r.Order)
                                                   .FirstOrDefaultAsync(r => r.OrderDetailID == id);

            //delete the registration detail
            _context.OrderDetails.Remove(orderDetail);
            await _context.SaveChangesAsync();

            //return the user to the registration/details page
            return RedirectToAction("Details", "Orders", new { id = orderDetail.Order.OrderID });
        }

        private bool OrderDetailExists(int id)
        {
          return (_context.OrderDetails?.Any(e => e.OrderDetailID == id)).GetValueOrDefault();
        }

        private SelectList GetAllProducts()
        {
            //create a list for all the courses
            List<Product> allCourses = _context.Products.ToList();

            //the user MUST select a course, so you don't need a dummy option for no course

            //use the constructor on select list to create a new select list with the options
            SelectList slAllCourses = new SelectList(allCourses, nameof(Product.ProductID), nameof(Product.Name));

            return slAllCourses;
        }
    }
}
