using Microsoft.AspNetCore.Mvc.Rendering;

namespace SilkDesign.Models
{
    public class ArrangementInventory
    {
        public string ArrangementInventoryID { get; set; }
        public string ArrangementID { get; set; }
        public string SizeID { get; set; }
        public string InventoryStatusID { get; set; }
        public string InventoryStatusCode {  get; set; }
        public string Code { get; set; }
        public DateTime LastUsed { get; set; }
        public String LastUsedDisplay { get; set; } 
        public string LocationID { get; set; }
        public string LocationName { get; set; }
        public string LocationPlacementID { get; set; }
        public IEnumerable<SelectListItem> Locations { get; set; }
        public IEnumerable<SelectListItem> StatusList { get; set; }

        public virtual ArrangementInventory Placement { get; set; }

    }
}
