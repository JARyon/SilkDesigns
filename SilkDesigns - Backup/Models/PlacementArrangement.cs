using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace SilkDesign.Models
{
    public class PlacementArrangement
    {   
        public string LocationPlacementID { get; set; }

        [Display(Name = "Placement: ")]
        public string PlacementName { get; set; }
        public string SizeID { get; set; }
        [Display(Name = "Size: ")] 
        public string SizeCode { get; set; }
        public string LocationID { get; set; }
        [Display(Name = "Quantity: ")]
        public int Quantity { get; set; }

        public IEnumerable<string> ArrangementInventoryID { get; set; }
        public IEnumerable<SelectListItem> Arrangements { get; set; }

        public PlacementArrangement()
        {
            Arrangements = new List<SelectListItem>();
            
        }
    }


}
