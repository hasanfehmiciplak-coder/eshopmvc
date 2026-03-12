using System.ComponentModel.DataAnnotations;

namespace EShopMVC.Web.ViewModels
{
    public class ProfileViewModel
    {
        [Required]
        public string FullName { get; set; }

        public string? AvatarPath { get; set; }

        public IFormFile? AvatarFile { get; set; }
    }
}