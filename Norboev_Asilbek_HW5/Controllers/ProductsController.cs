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
    public class ProductsController : Controller
    {
        private readonly AppDbContext _context;

        public ProductsController(AppDbContext context)
        {
            _context = context;
        }

        // GET: Products
        public async Task<IActionResult> Index()
        {
              return _context.Products != null ? 
                          View(await _context.Products.ToListAsync()) :
                          Problem("Entity set 'AppDbContext.Products'  is null.");
        }

        // GET: Products/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Products == null)
            {
                return NotFound();
            }

            var product = await _context.Products.Include(c => c.Suppliers)
                .FirstOrDefaultAsync(m => m.ProductID == id);
            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }

        // GET: Products/Create
        public IActionResult Create()
        {
            ViewBag.AllSuppliers = GetAllSuppliersList();

            if (User.IsInRole("Admin")==false)
            {
                return View("Error", new string[] { "You are not authorized to view this page!" });
            }
            return View();
        }

        // POST: Products/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ProductID,Name,Description,Price,ProductType")] Product product, int[] Suppliers )
        {
            //ViewBag.AllMonths = GetAllMonthsMultiSelectList();

            if (ModelState.IsValid == false)
            {
                ViewBag.AllSuppliers = GetAllSuppliersList();
                return View(product);
            }

            _context.Add(product);
            await _context.SaveChangesAsync();


            foreach (int SupplierID in Suppliers)
            {
                Supplier dbSupplier = _context.Suppliers.Find(SupplierID);

                product.Suppliers.Add(dbSupplier);
                _context.SaveChanges();
            }
            return RedirectToAction(nameof(Index));
        }

        // GET: Products/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (User.IsInRole("Admin") == false)
            {
                return View("Error", new string[] { "You are not authorized to view this page!" });
            }

            if (id == null || _context.Products == null)
            {
                return NotFound();
            }

            Product product = await _context.Products.Include(c => c.Suppliers)
                                        .FirstOrDefaultAsync(c => c.ProductID == id);
            if (product == null)
            {
                return View("Error", new string[] { "This product was not found!" });
            }
            ViewBag.AllSuppliers = GetAllSuppliersList(product);
            return View(product);
        }

        // POST: Products/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ProductID,Name,Description,Price,ProductType")] Product product, int[] selectedSuppliers)
        {
            if (id != product.ProductID)
            {
                return View("Error", new string[] { "Please try again!" });
            }

            if (ModelState.IsValid == false)
            {
                ViewBag.AllSuppliers = GetAllSuppliersList(product);
                return View(product);

            }
            try
            {
                Product dbproduct = _context.Products
                    .Include(c => c.Suppliers)
                    .FirstOrDefault(c => c.ProductID == product.ProductID);

                List<Supplier> supplierToRemove = new List<Supplier>();

                foreach (Supplier supplier in dbproduct.Suppliers)
                {
                    if (selectedSuppliers.Contains(supplier.SupplierID) == false)
                    {
                        supplierToRemove.Add(supplier);
                    }
                }

                foreach (Supplier supplier in supplierToRemove)
                {
                    dbproduct.Suppliers.Remove(supplier);
                    _context.SaveChanges();
                }
                
                foreach (int SupplierID in selectedSuppliers)
                {
                    if (dbproduct.Suppliers.Any(d => d.SupplierID == SupplierID) == false)
                    {
                        Supplier dbSupplier = _context.Suppliers.Find(SupplierID);

                        dbproduct.Suppliers.Add(dbSupplier);
                        _context.SaveChanges();
                    }
                }
                dbproduct.Name = product.Name;
                dbproduct.Description = product.Description;
                dbproduct.ProductType = product.ProductType;
                dbproduct.Price = product.Price;
                _context.Update(dbproduct);
                _context.SaveChanges();
            }
            catch (Exception ex)
            {
                return View("Error", new string[] { "There was an error editing this course.", ex.Message });

            }
            return RedirectToAction(nameof(Index));
        }

        private MultiSelectList GetAllSuppliersList()
        {
            //Get the list of months from the database (Select *)  
            //This would be Book in HW3
            List<Supplier> SupplierList = _context.Suppliers.ToList();

            //What happens if I choose MonthID that already exists?
            //Supplier SelectNone = new Category() { CategoryID = 0, CategoryName = "All Categories" };

            //incrementally added this to monthList 
            //CategoryList.Add(SelectNone);

            MultiSelectList SupplierSelectList = new MultiSelectList(SupplierList.OrderBy(m => m.SupplierID), "SupplierID", "SupplierName");

            //return the SelectList
            return SupplierSelectList;
        }
        private MultiSelectList GetAllSuppliersList(Product product)
        {
            //Get the list of months from the database (Select *)  
            //This would be Book in HW3
            List<Supplier> SupplierList = _context.Suppliers.ToList();

            List<Int32> selectedSupplierIDs = new List<Int32>();
            //What happens if I choose MonthID that already exists?
            //Supplier SelectNone = new Category() { CategoryID = 0, CategoryName = "All Categories" };
            foreach (Supplier supplier in product.Suppliers)
            {
                selectedSupplierIDs.Add(supplier.SupplierID);
            }
            //incrementally added this to monthList 
            //CategoryList.Add(SelectNone);
            MultiSelectList mslSuppliers = new MultiSelectList(SupplierList.OrderBy(m => m.SupplierName), "SupplierID", "SupplierName", selectedSupplierIDs);

            //return the SelectList
            return mslSuppliers;
        }

        private bool ProductExists(int id)
        {
          return (_context.Products?.Any(e => e.ProductID == id)).GetValueOrDefault();
        }
    }
}
