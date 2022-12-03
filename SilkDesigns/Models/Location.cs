using System.ComponentModel.DataAnnotations;
using System.Runtime.CompilerServices;

namespace SilkDesign.Models
{
    public class Location
    {
        public string LocationID { get; set; }

        [Required]
        public string Name { get; set; }

        public string Description { get; set; }
    }
}
