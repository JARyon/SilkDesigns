using SilkDesign.Models;
namespace SilkDesign.Models
{
    public class DogsCalendarIndexViewModel 
    {
        public string EventName { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string Owner { get; set; }
    }
}
