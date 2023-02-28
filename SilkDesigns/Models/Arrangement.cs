using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;
using Xunit.Sdk;

namespace SilkDesign.Models
{
    public class Arrangement
    {
        public string ArrangementID { get; set; }

        [Required(ErrorMessage = "Name is required.")]
        public string? Name { get; set; }
        public string? Description { get; set; }

        [Required(ErrorMessage = "Code is required.")]
        public string Code {  get; set; }   
        public decimal Price { get; set; }

        [Range(1, 10, ErrorMessage = "The quantity must be between 1 and 10.")] 
        public int Quantity { get; set; }
        public DateTime LastViewed { get; set; }


        public string SizeID { get; set; }
        
        public IEnumerable<SelectListItem> Sizes { get; set; }

        [Display(Name = "Size")]
        [Required(ErrorMessage = "The Size is required.")]
        public string SelectedSizeId { get; set; }

        public IList<SelectListItem> AvailableSizes { get; set; }

        public Arrangement()
        {
            AvailableSizes = new List<SelectListItem>();
        }
    }

}