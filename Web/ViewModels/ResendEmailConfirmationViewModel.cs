using System.ComponentModel.DataAnnotations;

namespace EShopMVC.Web.ViewModels
{
    public class ResendEmailConfirmationViewModel
    {
        [Required, EmailAddress]
        public string Email { get; set; }
    }
}