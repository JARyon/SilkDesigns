using Microsoft.AspNetCore.Mvc.Rendering;

namespace SilkDesign.Models
{
    public class RouteLocation
    {
        public string RouteLocationID { get; set; } 
        public string RouteID { get; set; } 
        public string RouteName { get; set; }

        public string LocationID { get; set; }  
        public string LocationName { get; set; }

        public int RouteOrder { get; set;}
        public int OldRouteOrder { get; set; }

        public string CustomerName { get; set; }    
        public string UserID { get; set; }
        public IEnumerable<SelectListItem> AvailableLocations { get; set; } 

    }
}
