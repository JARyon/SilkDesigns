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
        public int Quantity { get; set; }
        public string ImagePath { get; set; }
        public string SizeID { get; set; }

        public string Deleted { get; set; }
        public IEnumerable<SelectListItem> Sizes { get; set; }
        public string SizeCode { get; set; }
        public string StatusCode { get; set; }
        [Display(Name = "Size")]
        [Required(ErrorMessage = "The Size is required.")]
        public string SelectedSizeId { get; set; }
        public IList<SelectListItem> AvailableSizes { get; set; }

        [Display(Name = "Catalog Status")]
        [Required(ErrorMessage = "The Status is required.")]
        public string SelectedCatalogStatusID { get; set; }
        public IList<SelectListItem> AvailableCatalogStatus { get; set; }

        public bool ShowInactive {  get; set; }
        public CatalogIndexViewModel()
        {
            AvailableSizes = new List<SelectListItem>();
            AvailableCatalogStatus = new List<SelectListItem>();
        }
    }
}
