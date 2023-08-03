using Microsoft.AspNetCore.Mvc.Rendering;

namespace SilkDesign.Models
{
    public class RoutePlanStop
    {
        private bool _HasData = false;
        public bool HasData
        {
            get { return _HasData; }
            set { _HasData = value; }
        }
        public string RoutePlanID { get; set; }
        public string RoutePlanDetailID { get; set; }
        public string LocationName { get; set; }
        public string LocationID { get; set; }

        public string PlacmentName {  get; set; }
        public string PlacementID { get; set; }
        public int RouteOrder { get; set; }

        public string SizeDesc { get; set; }
        public string SizeID { get; set; }
        public string IncomingInventoryCode { get; set; }
        public string IncomingArrangmentName { get; set; }
        public string IncomingArrangementInventoryID { get; set; }
        public string OutgoingInventoryCode { get; set; }
        public string OutgoingArrangmentName { get; set; }
        public string OutgoingArrangementInventoryID { get; set; }

        public IEnumerable<SelectListItem> AvailableArrangements { get; set; }
  
    public RoutePlanStop()
    {
        RoutePlanID = RoutePlanID;
        AvailableArrangements = new List<SelectListItem>();
    }
    }
}
