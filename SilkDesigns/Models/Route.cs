using Microsoft.AspNetCore.Mvc.Rendering;

namespace SilkDesign.Models
{
    public class Route
    {
        public string RouteId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string WarehouseID { get; set; }
        public IEnumerable<SelectListItem> Warehouses { get; set; }
    }
}
