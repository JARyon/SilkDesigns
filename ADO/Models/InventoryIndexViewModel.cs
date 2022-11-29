using ADO.Models;
namespace ADO.Models
{
    public class InventoryIndexViewModel
    {
        public Inventory Inventory { get; set; }    
        public Size Size { get; set; }  
        public int Id { get; set; }
        public string? Name { get; set; }
        public decimal Price { get; set; }
        public int Quantity { get; set; }
        public string Code { get; set; }
    }
}
