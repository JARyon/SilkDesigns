using System.ComponentModel.DataAnnotations;
using System.Runtime.CompilerServices;

namespace SilkDesign.Models
{
    public class Location
    {
        public string LocationID { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public string LocationTypeID { get; set; }
                
        public string CustomerID { get; set; }

        public virtual ICollection<LocationPlacement> locationarrangments { get; set; }

    }
}
