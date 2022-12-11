using Microsoft.AspNetCore.Mvc.Rendering;

namespace SilkDesign.Models
{
    public class LocationArrangement
    {
        public string LocationArrangementID { get; set; }   
        public string LocationID { get;  set; }
        public string LocationName { get; set; }

        public string SizeID { get; set; }  
        public string Description { get; set; }
        public string Code { get; set; }

        public IEnumerable<SelectListItem> Sizes { get; set; }
    }
}
