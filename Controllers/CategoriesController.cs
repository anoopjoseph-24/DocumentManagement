using System.Security.Claims;
using DMS.Data;
using DMS.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DMS.Controllers;

[Authorize]
public class CategoriesController : Controller
{
    private readonly DmsDbContext _context;

    public CategoriesController(DmsDbContext context)
    {
        _context = context;
    }

    // GET: /Categories
    public async Task<IActionResult> Index()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        var categories = await _context.Categories
            .Where(category => category.OwnerId == userId)
            .OrderBy(category => category.Name)
            .ToListAsync();

        return View(categories);
    }

    // GET: /Categories/Create
    public IActionResult Create()
    {
        return View();
    }

    // POST: /Categories/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Category category)
    {
        // The owner must come from the authenticated session, not the form.
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrEmpty(userId))
        {
            return Challenge();
        }

        ModelState.Remove(nameof(Category.OwnerId));
        ModelState.Remove(nameof(Category.Owner));

        if (!ModelState.IsValid)
        {
            return View(category);
        }

        var nameExists = await _context.Categories
            .AnyAsync(c => c.OwnerId == userId && c.Name == category.Name);

        if (nameExists)
        {
            ModelState.AddModelError(nameof(Category.Name),
                "You already have a category with this name.");
            return View(category);
        }

        category.OwnerId = userId;
        category.CreatedUtc = DateTime.UtcNow;

        _context.Categories.Add(category);
        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }
}