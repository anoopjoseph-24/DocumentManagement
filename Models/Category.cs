using DMS.Models;
using System.ComponentModel.DataAnnotations;
using System.Reflection.Metadata;

namespace DMS.Models;

public class Category
{
    public int Id { get; set; }

    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;

    public DateTime CreatedUtc { get; set; } = DateTime.UtcNow;

    [Required]
    public string OwnerId { get; set; } = string.Empty;

    public ApplicationUser Owner { get; set; } = null!;

    public ICollection<Document> Documents { get; set; }
        = new List<Document>();
}