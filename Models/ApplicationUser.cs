using Microsoft.AspNetCore.Identity;

namespace DMS.Models;

public class ApplicationUser : IdentityUser
{
    public string? FullName { get; set; }

    public ICollection<Category> Categories { get; set; }
        = new List<Category>();

    public ICollection<Document> Documents { get; set; }
        = new List<Document>();
}