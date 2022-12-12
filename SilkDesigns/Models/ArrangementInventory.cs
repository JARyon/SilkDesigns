using Microsoft.AspNetCore.Mvc.Rendering;

namespace SilkDesign.Models
{
    public class ArrangementInventory
    {
        public string ArrangementInventoryID { get; set; }
        public string Code { get; set; }
        public DateTime LastUsed { get; set; }
        public String LastUsedDisplay { get; set; } 
        public string LocationID { get; set; }
        public string LocationName { get; set; }


    }
}
