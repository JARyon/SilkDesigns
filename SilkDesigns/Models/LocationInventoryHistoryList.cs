namespace SilkDesign.Models
{
    public class LocationInventoryHistoryList
    {
        public string CustomerName { get; set; }    
        public string LocationName { get; set; }
        public string Placement { get; set; }
        public string Arrangement { get; set; }
        public string Size { get; set; }
        public DateTime StartDate { get; set; } 
        public DateTime? EndDate { get; set; }


    }
}
