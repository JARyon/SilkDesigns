using Microsoft.AspNetCore.Mvc;
using SilkDesign.Models;
using System.Data.SqlClient;
using System.Dynamic;
using SilkDesign.Shared;
using Newtonsoft.Json.Linq;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;
using System.Data;
using System.Reflection.Metadata;
using Azure.Core.Serialization;
using System.Text.Json;

namespace SilkDesign.Controllers
{
    public class DogsCalendarController : Controller
    {

        string msUserName = string.Empty;
        string msUserID = string.Empty;
        string msIsAdmin = string.Empty;
        string msconnectionString = string.Empty;

        public IConfiguration Configuration { get; }

        public DogsCalendarController(IConfiguration configuration)
        {
            Configuration = configuration;
        }
        public IActionResult Index()
        {
            ISession currentSession = HttpContext.Session;
            if (!ControllersShared.IsLoggedOn(currentSession, ref msUserID, ref msUserName, ref msIsAdmin))
            {
                return RedirectToAction("Login", "Login");
            }

            List<DogsCalendarIndexViewModel> ivmDogEvent = new List<DogsCalendarIndexViewModel>();
            string connectionString = Configuration["ConnectionStrings:SilkDesigns"];
            DogsCalendarIndexViewModel DogEvent = new DogsCalendarIndexViewModel();
            DogEvent.StartDate = DateTime.Now;
            DogEvent.EndDate = DateTime.Now + TimeSpan.FromDays(1);
            DogEvent.Owner = "Jill Ryon";
            DogEvent.EventName = "Watch John";
            ivmDogEvent.Add(DogEvent);
            //            ViewBag.Message = "{title: 'All Day Event', start: '2025-11-11'}";
            List<Event> eventList = new List<Event>();
            Event oEvent = new Event();
            oEvent.title = "John Test Title";
            oEvent.start = DateTime.Now;
            oEvent.allDay = true;

            Event oEvent2 = new Event();
            oEvent2.title = "John Test Title2";
            oEvent2.start = new DateTime(2025, 11, 13, 10, 30, 0);
            oEvent2.end = oEvent2.start.AddHours(12);
            oEvent2.allDay = false;

            Event oEvent3 = new Event();
            oEvent3.title = "John Test Title2";
            oEvent3.start = new DateTime(2025, 11, 18, 12, 00, 0);
            oEvent3.end = oEvent3.start.AddDays(4);
            oEvent3.allDay = true;

            eventList.Add(oEvent);
            eventList.Add(oEvent2);
            eventList.Add(oEvent3);

            var json = JsonSerializer.Serialize(eventList);
            ViewBag.Message = json;
            return View(ivmDogEvent);
        }
    }
}
