using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;
namespace SilkDesign.Models
{
    [Serializable]
    public class Login
    {
        public string Id { get; set; }

        [Required(ErrorMessage = "UserName is required.")]
        public string UserName { get; set; }

        [Required(ErrorMessage = "Password is required.")]
        public string PasswordHash { get; set; }
        public bool IsAdmin { get; set; }   
        public string Email { get; set; }
        public bool IsLockedOut { get; set; }
        public int AccessFailedCount { get; set; }
    }
}
