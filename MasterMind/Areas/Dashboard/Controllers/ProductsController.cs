using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MasterMind.Data;
using MasterMind.Models;
using MasterMind.StaticData;
using Microsoft.AspNetCore.Authorization;

namespace MasterMind.Areas.Dashboard.Controllers
{
    [Area("Dashboard")]
    [Authorize(Roles = SD.Role_Admin)]
    public class ProductsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ProductsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Dashboard/Products
        public async Task<IActionResult> Index()
        {
            var Categorylist = await _context.Categories.ToListAsync();
            ViewBag.CategoryList = Categorylist;

            return View(await _context.Products.ToListAsync());

        }

        // GET: Dashboard/Products/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _context.Products
                .FirstOrDefaultAsync(m => m.Id == id);
            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }


        // GET: Dashboard/Products/Create
        public IActionResult Create()
        {
            ViewBag.CategoryId = new SelectList(_context.Categories, "Id", "Name");
            return View();
        }


        // POST: Dashboard/Products/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Product product, IFormFile Image)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.CategoryId = new SelectList(_context.Categories, "Id", "Name");
                return View(product);
            }

            if (Image == null)
            {
                ModelState.AddModelError(nameof(Product.Image), "Image is required.");
                ViewBag.CategoryId = new SelectList(_context.Categories, "Id", "Name"); // Reassign ViewBag
                return View(product);
            }

            var imageName = Guid.NewGuid() + Path.GetExtension(Image.FileName);
            var imagePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/img/Products");

            if (!Directory.Exists(imagePath))
            {
                Directory.CreateDirectory(imagePath);
            }

            var savePath = Path.Combine(imagePath, imageName);

            await using (var stream = new FileStream(savePath, FileMode.Create))
            {
                await Image.CopyToAsync(stream);
            }

            product.Image = $"/img/Products/{imageName}";

            _context.Add(product);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Product Added successfully!";
        
            return RedirectToAction(nameof(Index));

       
        }




        // GET: Dashboard/Products/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _context.Products.FindAsync(id);
            if (product == null)
            {
                return NotFound();
            }

            // Populate dropdown with categories and preselect current category
            ViewBag.CategoryId = new SelectList(_context.Categories, "Id", "Name", product.CategoryId);
            return View(product);
        }


        // POST: Dashboard/Products/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Product product, IFormFile? Image)
        {
            if (id != product.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var oldProduct = await _context.Products.FindAsync(id); // ✅ Use FindAsync instead of FirstOrDefaultAsync

                    if (oldProduct == null)
                    {
                        return NotFound();
                    }

                    // Handle Image Upload
                    if (Image != null)
                    {
                        var imageName = Guid.NewGuid() + Path.GetExtension(Image.FileName);
                        var imagePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/img/Products");

                        if (!Directory.Exists(imagePath))
                        {
                            Directory.CreateDirectory(imagePath);
                        }

                        var savePath = Path.Combine(imagePath, imageName);

                        await using (var stream = new FileStream(savePath, FileMode.Create))
                        {
                            await Image.CopyToAsync(stream);
                        }

                        // ✅ Delete old image if exists
                        if (!string.IsNullOrEmpty(oldProduct.Image))
                        {
                            var oldImagePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", oldProduct.Image.TrimStart('/'));
                            if (System.IO.File.Exists(oldImagePath))
                            {
                                System.IO.File.Delete(oldImagePath);
                            }
                        }

                        // Update the image path in the product
                        oldProduct.Image = $"/img/Products/{imageName}";
                    }

                    // ✅ Update other product properties
                    oldProduct.Name = product.Name;
                    oldProduct.Description = product.Description;
                    oldProduct.Price = product.Price;
                    oldProduct.CategoryId = product.CategoryId;
                    oldProduct.Specification = product.Specification; // ✅ Ensure Specification is updated
                    oldProduct.Quantity = product.Quantity;

                    _context.Products.Update(oldProduct);
                    await _context.SaveChangesAsync();

                    TempData["SuccessMessage"] = "Product updated successfully!";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException ex)
                {
                    // ✅ Log the exception (optional)
                    Console.WriteLine($"Concurrency error: {ex.Message}");

                    if (!ProductExists(product.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        ModelState.AddModelError("", "The record you attempted to edit was modified by another user. Try again.");
                    }
                }
            }

            ViewBag.CategoryId = new SelectList(_context.Categories, "Id", "Name", product.CategoryId);
            return View(product);
        }




        // GET: Dashboard/Products/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _context.Products
                .FirstOrDefaultAsync(m => m.Id == id);

            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }


        // POST: Dashboard/Products/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var product = await _context.Products.FindAsync(id);

            if (product != null)
            {
                // Delete product image from the wwwroot folder
                if (!string.IsNullOrEmpty(product.Image))
                {
                    var imagePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", product.Image.TrimStart('/'));

                    if (System.IO.File.Exists(imagePath))
                    {
                        System.IO.File.Delete(imagePath);
                    }
                }

                // Remove the product from the database
                _context.Products.Remove(product);
                await _context.SaveChangesAsync();

                // ✅ Store a success message in TempData
                TempData["SuccessMessage"] = "Product deleted successfully!";
            }

            return RedirectToAction(nameof(Index));
        }

    
    
    private bool ProductExists(int id)
        {
            return _context.Products.Any(e => e.Id == id);
        }
    }
    }
