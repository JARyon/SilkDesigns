namespace SilkDesign.Models
{
    public class RouteLocation
    {
        public string RouteLocationID { get; set; } 
        public string RouteID { get; set; } 
        public string LocationID { get; set; }  
        public int RouteOrder { get; set;}

        public string CustomerName { get; set; }    
        public string LocationName { get; set; }    
    }
}
