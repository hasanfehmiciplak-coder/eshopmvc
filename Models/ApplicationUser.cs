using EShopMVC.Modules.Orders.Models;
using Microsoft.AspNetCore.Identity;

namespace EShopMVC.Models
{
    public class ApplicationUser : IdentityUser
    {
        // Ek alanlar
        public string? FullName { get; set; }

        // Navigation
        public ICollection<Order> Orders { get; set; }

        public ICollection<CustomerAddress> Addresses { get; set; }
        public string? AvatarPath { get; set; }
        public bool IsActive { get; set; } = true;
    }
}