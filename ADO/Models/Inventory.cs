using Microsoft.AspNetCore.Mvc.Rendering;

namespace ADO.Models
{
    public class Inventory
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public decimal Price { get; set; }
        public int Quantity { get; set; }
        public DateTime LastViewed { get; set; }
        public int SizeID { get; set; }
        
        public IEnumerable<SelectListItem> SelectSize { get; set; }
        public Size[] arSize { get; set; }
    }

}