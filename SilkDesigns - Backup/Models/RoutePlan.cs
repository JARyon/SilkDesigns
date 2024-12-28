using Microsoft.AspNetCore.Mvc.Rendering;

namespace SilkDesign.Models
{
    public class RoutePlan
    {
        public string RoutePlanID { get; set; }
        public string Description { get; set; }
        public DateTime RouteDate { get; set; }
        public string RouteID { get; set; }
        public string RouteName { get; set; }
        public string RoutePlanStatusID { get; set; }
        public string RoutePlanStatusCode { get; set; }
        public string UserID { get; set; }  

        public IEnumerable<SelectListItem> AvailableRoutes { get; set; }

    }
}
