using SilkDesign.Models;
namespace SilkDesign.Models
{
    public class InventoryIndexViewModel
    {
        public Inventory Inventory { get; set; }    
        public Size Size { get; set; }  
        public string InventoryID { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }
        public decimal Price { get; set; }
        public int Quantity { get; set; }
        public string Code { get; set; }
    }
}
