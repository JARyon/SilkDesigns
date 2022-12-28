using Microsoft.AspNetCore.Mvc;
using SilkDesign.Models;
using System.Data.SqlClient;
using System.Dynamic;
using SilkDesign.Shared;

namespace SilkDesign.Controllers
{
    public class RoutePlanController : Controller
    {
        public IConfiguration Configuration { get; }

        public RoutePlanController(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IActionResult Index()
        {
            List<RoutePlan> routePlanList = new List<RoutePlan>();
            string connectionString = Configuration["ConnectionStrings:SilkDesigns"];
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                string sql = "SELECT " +
                    "   rp.RoutePlanID  RoutePlanID " +
                    " ,  r.Name         Route " +
                    " , rp.Description  Description " +
                    " , rp.RouteDate    Date " +
                    " , rps.Code        Status " +
                    " from RoutePlan rp " +
                    " join Route r on r.RouteID = rp.RouteID " +
                    " join RoutePlanStatus rps on rps.RoutePlanStatusID = rp.RoutePlanStatusID" +
                    " Order by rp.RouteDate ";

                SqlCommand readcommand = new SqlCommand(sql, connection);

                using (SqlDataReader dr = readcommand.ExecuteReader())
                {
                    while (dr.Read())
                    {
                        RoutePlan routePlan = new RoutePlan();
                        routePlan.RoutePlanID = Convert.ToString(dr["RoutePlanID"]);
                        routePlan.RouteName = Convert.ToString(dr["Route"]);
                        routePlan.Description = Convert.ToString(dr["Description"]);
                        routePlan.RouteDate = Convert.ToDateTime(dr["Date"]);
                        routePlan.RoutePlanStatusCode = Convert.ToString(dr["Status"]);
                        routePlanList.Add(routePlan);
                    }
                }
                connection.Close();
            }

            return View(routePlanList);
        }
    }
}
