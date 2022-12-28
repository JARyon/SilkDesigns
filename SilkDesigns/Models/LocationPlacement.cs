﻿using Microsoft.AspNetCore.Mvc.Rendering;

namespace SilkDesign.Models
{
    public class LocationPlacement
    {
        public string LocationPlacementID { get; set; }   
        public string LocationID { get;  set; }
        public string LocationName { get; set; }

        public string SizeID { get; set; }  
        public string Description { get; set; }
        public string Code { get; set; }
        public virtual Location location { get; set; }
        public IEnumerable<SelectListItem> Sizes { get; set; }
    }
}