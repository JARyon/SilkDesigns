using Microsoft.AspNetCore.Mvc.Rendering;

namespace SilkDesign.Models
{
    public class Inventory
    {
        public string InventoryID { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }

        public decimal Price { get; set; }
        public int Quantity { get; set; }
        public DateTime LastViewed { get; set; }
        public string SizeID { get; set; }
        
        public IEnumerable<SelectListItem> Sizes { get; set; }
       // public Size[] arSize { get; set; }
    }

}