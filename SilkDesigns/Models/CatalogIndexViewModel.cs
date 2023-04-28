using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace SilkDesign.Models
{
    public class CatalogIndexViewModel
    {
        public string CatalogID { get; set; }

        [Required(ErrorMessage = "Name is required.")]
        public string? Name { get; set; }
        public string? Description { get; set; }

        public string Code { get; set; }

        public string ImagePath { get; set; }
        public string SizeID { get; set; }

        public IEnumerable<SelectListItem> Sizes { get; set; }
        public string SizeCode { get; set; }

        [Display(Name = "Size")]
        [Required(ErrorMessage = "The Size is required.")]
        public string SelectedSizeId { get; set; }

        public IList<SelectListItem> AvailableSizes { get; set; }
        public CatalogIndexViewModel()
        {
            AvailableSizes = new List<SelectListItem>();
        //    Inventory = new List<ArrangementInventory>();
        }
    }
}
