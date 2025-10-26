// User.cs
using Microsoft.AspNetCore.Identity;

namespace EMRSystem.Core.Entities
{
    public class ApplicationUser : IdentityUser<int>
    {
        [Required]
        [StringLength(100)]
        public string FullName { get; set; }
        
        public DateTime CreatedAt { get; set; }
        public DateTime? LastLogin { get; set; }
        public bool IsActive { get; set; }
        
        // Navigation properties
        public ICollection<UserRole> UserRoles { get; set; }
    }
    
    public class ApplicationRole : IdentityRole<int>
    {
        public string Description { get; set; }
        
        // Navigation properties
        public ICollection<UserRole> UserRoles { get; set; }
    }
    
    public class UserRole : IdentityUserRole<int>
    {
        public ApplicationUser User { get; set; }
        public ApplicationRole Role { get; set; }
    }
}