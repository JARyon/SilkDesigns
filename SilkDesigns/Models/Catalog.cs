using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;
using Xunit.Sdk;

namespace SilkDesign.Models
{
    public class Catalog
    {
        public string CataglogID { get; set; }

        [Required(ErrorMessage = "Name is required.")]
        public string? Name { get; set; }
        public string? Description { get; set; }

        [Required(ErrorMessage = "Code is required.")]
        public string Code { get; set; }
        public string SizeID { get; set; }

        public String UserID { get; set; }
        public IEnumerable<SelectListItem> Sizes { get; set; }

        [Display(Name = "Size")]
        [Required(ErrorMessage = "The Size is required.")]
        public string SelectedSizeId { get; set; }

        public IList<SelectListItem> AvailableSizes { get; set; }

        public Catalog() 
        {
            AvailableSizes = new List<SelectListItem>();
        }    

    }
}
