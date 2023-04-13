using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace SilkDesign.Models
{
    public class Customer
    {
        [Required]
        public string CustomerId { get; set; }
        [Required]
        [StringLength(50), MinLength(3)]
        public string Name { get; set; }

        [StringLength(100)]
        public string Address { get; set; } 

        public string Deleted { get; }
    }
}
