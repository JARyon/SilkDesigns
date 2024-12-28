using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace SilkDesign.Models
{
    public class ArrangementIndexViewModel
    {
        public string ArrangementID { get; set; }

        [Required(ErrorMessage = "Name is required.")]
        public string? Name { get; set; }
        public string? Description { get; set; }

        public string Code { get; set; }

        public decimal Price { get; set; }

        public int Quantity { get; set; }
        public DateTime LastViewed { get; set; }

        public string ImagePath { get; set; }
        public string SizeID { get; set; }

        public IEnumerable<SelectListItem> Sizes { get; set; }
        public string SizeCode { get; set; }

        public string SelectedSizeId { get; set; }

        public IList<SelectListItem> AvailableSizes { get; set; }
        public List<ArrangementInventory> Inventory { get; set; }
        public ArrangementIndexViewModel()
        {
            AvailableSizes = new List<SelectListItem>();
            Inventory = new List<ArrangementInventory>();
        }
    }
}
