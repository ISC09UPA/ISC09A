using Microsoft.AspNetCore.Identity;

namespace EasyMemories.Models;

public class AppUser : IdentityUser
{
    public string DisplayName { get; set; } = string.Empty;

    public ICollection<Space> Spaces { get; set; } = new List<Space>();
}
