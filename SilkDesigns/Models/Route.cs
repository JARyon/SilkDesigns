using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace SilkDesign.Models
{
    public class Route
    {
        public string RouteId { get; set; }

        [Display(Name = "Route Name:")]
        [Required(ErrorMessage = "Name is required.")]
        public string Name { get; set; }
        public string Description { get; set; }

        [Display(Name = "Warehouse:")]
        [Required(ErrorMessage = "Warehouse is required.")]
        public string WarehouseID { get; set; }
        public IList<SelectListItem> Warehouses { get; set; }

        public Route()
        {
            Warehouses = new List<SelectListItem>();
        }  
    }
}
