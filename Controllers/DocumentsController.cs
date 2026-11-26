using System.Security.Claims;
using DMS.Data;
using DMS.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace DMS.Controllers;

[Authorize]
public class DocumentsController : Controller
{
    private readonly DmsDbContext _context;
    private readonly IWebHostEnvironment _environment;

    private static readonly HashSet<string> AllowedExtensions =
        new(StringComparer.OrdinalIgnoreCase)
        {
            ".pdf", ".doc", ".docx", ".xls", ".xlsx", ".txt"
        };

    private const long MaxFileSize = 10 * 1024 * 1024; // 10 MB

    public DocumentsController(
        DmsDbContext context,
        IWebHostEnvironment environment)
    {
        _context = context;
        _environment = environment;
    }

    public async Task<IActionResult> Index(string? search)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        var query = _context.Documents
            .Where(document => document.OwnerId == userId);

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(document =>
                document.OriginalFileName.Contains(search));
        }

        ViewBag.Search = search;

        var documents = await query
            .OrderByDescending(document => document.UploadedUtc)
            .ToListAsync();

        return View(documents);
    }
    

    [HttpGet]
    public async Task<IActionResult> Upload()
    {
        await LoadCategoriesAsync();
        return View();
    }

    [HttpGet]
    public async Task<IActionResult> Download(int id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        var document = await _context.Documents
            .FirstOrDefaultAsync(d => d.Id == id && d.OwnerId == userId);

        if (document is null)
            return NotFound();

        var storageFolder = Path.Combine(
            _environment.ContentRootPath, "App_Data", "DocumentFiles");

        // Use only the stored file name, never a path supplied by the request.
        var safeStorageKey = Path.GetFileName(document.StorageKey);
        var filePath = Path.Combine(storageFolder, safeStorageKey);

        if (!System.IO.File.Exists(filePath))
            return NotFound();

        return PhysicalFile(
            filePath,
            "application/octet-stream",
            document.OriginalFileName);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [RequestSizeLimit(MaxFileSize)]
    public async Task<IActionResult> Upload(IFormFile? file, int? categoryId)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
            return Challenge();

        if (file is null || file.Length == 0)
            ModelState.AddModelError(nameof(file), "Choose a file to upload.");

        if (file is not null && file.Length > MaxFileSize)
            ModelState.AddModelError(nameof(file), "The maximum file size is 10 MB.");

        var originalName = file is null
            ? ""
            : Path.GetFileName(file.FileName);

        var extension = Path.GetExtension(originalName);
        if (file is not null && !AllowedExtensions.Contains(extension))
            ModelState.AddModelError(nameof(file), "This file type is not allowed.");

        if (categoryId.HasValue &&
            !await _context.Categories.AnyAsync(c =>
                c.Id == categoryId.Value && c.OwnerId == userId))
        {
            ModelState.AddModelError(nameof(categoryId), "Choose one of your categories.");
        }

        if (!ModelState.IsValid)
        {
            await LoadCategoriesAsync(userId);
            return View();
        }

        var storageFolder = Path.Combine(
            _environment.ContentRootPath, "App_Data", "DocumentFiles");

        Directory.CreateDirectory(storageFolder);

        var storageKey = $"{Guid.NewGuid():N}{extension}";
        var filePath = Path.Combine(storageFolder, storageKey);

        await using (var stream = new FileStream(filePath, FileMode.CreateNew))
        {
            await file!.CopyToAsync(stream);
        }

        var document = new Document
        {
            OriginalFileName = originalName,
            StorageKey = storageKey,
            ContentType = "application/octet-stream",
            SizeBytes = file!.Length,
            UploadedUtc = DateTime.UtcNow,
            OwnerId = userId,
            CategoryId = categoryId
        };

        _context.Documents.Add(document);
        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }

    private async Task LoadCategoriesAsync(string? userId = null)
    {
        userId ??= User.FindFirstValue(ClaimTypes.NameIdentifier);

        var categories = await _context.Categories
            .Where(c => c.OwnerId == userId)
            .OrderBy(c => c.Name)
            .ToListAsync();

        ViewBag.Categories = new SelectList(categories, "Id", "Name");
    }
}