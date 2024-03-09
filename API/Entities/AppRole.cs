using Microsoft.AspNetCore.Identity;

namespace API.Entities
{
    public class AppRole : IdentityRole<int>
    {
        public   ICollection<AppUserRole> userRoles { get; set; }
    }
}