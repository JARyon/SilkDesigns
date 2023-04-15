using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;
using Xunit.Sdk;

namespace SilkDesign.Models
{
    public class CustomerInventoryHistory
    {
        public string CustomerHistoryID { get; set; }
        public string CustomerID { get; set; }
        [Display(Name = "Customer: ")]
        public string CustomerName { get; set; }

        public string LocationID { get; set; }
        [Display(Name = "Location: ")]
        public string LocationName { get; set; }
        public string LocationPlacementID { get; set; }
        public IEnumerable<SelectListItem> Placements { get; set; }

        [Required(ErrorMessage = "Arrangement is required.")]
        public string ArrangementID { get; set; }   
        public IEnumerable<SelectListItem> Arrangements { get; set; }

        [Required(ErrorMessage = "Start Date is required.")]
        [Display(Name ="Start Date: ")]
        public DateTime StartDate { get; set; }

        [Display(Name = "End Date: ")]
        public DateTime EndDate { get; set; }
        public String InventoryStatusID { get; set; }
        public CustomerInventoryHistory() 
        { 
            Placements = new List<SelectListItem>();
            Arrangements = new List<SelectListItem>();
        }
    }
}
