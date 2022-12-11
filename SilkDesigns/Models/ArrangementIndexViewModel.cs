using Microsoft.AspNetCore.Mvc.Rendering;
using SilkDesign.Models;
namespace SilkDesign.Models
{
    public class ArrangementIndexViewModel
    {
        public Arrangement Arrangement { get; set; }    
        public Size Size { get; set; }  
        public string ArrangementID { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }
        public decimal Price { get; set; }
        public int Quantity { get; set; }
        public string Code { get; set; }
        public IEnumerable<SelectListItem> Sizes { get; set; }
        public string SizeID { get; set; }
    }
}
