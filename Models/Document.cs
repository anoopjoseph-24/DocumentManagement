using DMS.Models;
using System.ComponentModel.DataAnnotations;

namespace DMS.Models;

public class Document
{
    public int Id { get; set; }

    [Required]
    [StringLength(255)]
    public string OriginalFileName { get; set; } = string.Empty;

    [Required]
    [StringLength(255)]
    public string StorageKey { get; set; } = string.Empty;

    [Required]
    [StringLength(150)]
    public string ContentType { get; set; } = "application/octet-stream";

    public long SizeBytes { get; set; }

    public DateTime UploadedUtc { get; set; } = DateTime.UtcNow;

    [Required]
    public string OwnerId { get; set; } = string.Empty;

    public ApplicationUser Owner { get; set; } = null!;

    public int? CategoryId { get; set; }

    public Category? Category { get; set; }
}