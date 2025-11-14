using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace SilkDesign.Models
{
    public class DogsCalendar
    {
    }
    public class Event
    {
        public string title { get; set; }
        public DateTime start { get; set; }
        public DateTime end { get; set; }
        public Boolean allDay { get; set; }
        public string url { get; set; }
    }
}
