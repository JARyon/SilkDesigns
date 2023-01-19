namespace SilkDesign.Models
{
    public class RoutePlanDetail
    {
        public string RoutePlanDetailID { get; set; }

        public string RoutePlanID { get; set; }
        public string LocationID { get; set;}
        public string LocationName { get; set;}

        public string LocationPlacementID {  get; set;}
        public string PlacmentDescription {  get; set;}
        public int RouteOrder { get; set;}  
        public string SizeID { get; set;}   
        public string SizeCode { get; set; }
        public string OutgoingArrangementInventoryID { get; set; }
        public string OutgoingArrangmentInventoryCode { get; set; }
        public string OutgoingArrangementName { get; set; }
        public string OutgoingArrangementID {  get; set; }  


        public string IncomingArrangementInventoryID { get; set; }
        public string IncomingArrangmentInventoryCode { get; set; }
        public string IncomingingArrangementName { get; set; }
        public string IncomingArrangementID { get; set; }
        public string Disposition { get; set; }

    }
}
