using Microsoft.AspNetCore.Mvc;
using SilkDesign.Models;
using System.Data.SqlClient;
using System.Dynamic;
using SilkDesign.Shared;
using System.Data;

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
        public IActionResult Update(string id)
        {
            string sRoutePlanID = id;
            string connectionString = Configuration["ConnectionStrings:SilkDesigns"];

            dynamic RouteDetails = new ExpandoObject();
            RouteDetails.RoutePlans = SilkDesignUtility.GetRoutePlans(connectionString, sRoutePlanID);
            RouteDetails.RoutePlanDetails = SilkDesignUtility.GetRoutePlanDetails(connectionString, sRoutePlanID);

            return View(RouteDetails);
        }

        [HttpPost]
        public IActionResult Update(RoutePlan routePlan, string id)
        {
            string sRoutePlanID = id;
            string connectionString = Configuration["ConnectionStrings:SilkDesigns"];
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string sql = $"Update RoutePlan SET " +
                                    $" RouteDate= @Date, " +
                                    $" Description= @Description " +
                                    $" Where RoutePlanID='{id}'";

                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    command.Parameters.Clear();
                    SqlParameter DateParameter = new SqlParameter
                    {
                        ParameterName = "@Date",
                        Value = routePlan.RouteDate,
                        SqlDbType = SqlDbType.DateTime
                    };

                    SqlParameter DescParameter = new SqlParameter
                    {
                        ParameterName = "@Description",
                        Value = routePlan.Description,
                        SqlDbType = SqlDbType.VarChar,
                        Size = 250

                    };

                    SqlParameter[] paramaters = new SqlParameter[] { DateParameter, DescParameter };
                    command.Parameters.AddRange(paramaters);
                    connection.Open();
                    command.ExecuteNonQuery();

                }
                connection.Close();
            }

            return RedirectToAction("Index");
        }

        public IActionResult UpdateRoutePlanStop(string id)
        {
            string connectionString = Configuration["ConnectionStrings:SilkDesigns"];
            //RoutePlanStop routePlanStop = new RoutePlanStop(); //  SilkDesignUtility.GetRoutPlanStop
            RoutePlanStop routePlanStopDetail = SilkDesignUtility.GetRoutePlanDetail(connectionString, id);
            return View(routePlanStopDetail);
        }

        [HttpPost]
        public IActionResult UpdateRoutePlanStop(RoutePlanStop routePlanStopDetail, string id)
        {
            string sRoutePlanDetailID = id;
            string connectionString = Configuration["ConnectionStrings:SilkDesigns"];
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string sExitingArrangementInventoryID = SilkDesignUtility.GetSuggestedInventoryID(connectionString, sRoutePlanDetailID);

                string sql = $" Update RoutePlanDetail SET " +
                             $" IncomingArrangementInventoryID = @IncomingArrangementID " +
                             $" Where RoutePlanDetailID=@RoutePlanDetailID";

                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    command.Parameters.Clear();
                    SqlParameter IncomingArrangementID = new SqlParameter
                    {
                        ParameterName = "@IncomingArrangementID",
                        Value = routePlanStopDetail.IncomingArrangementInventoryID,
                        SqlDbType = SqlDbType.VarChar
                    };

                    SqlParameter RoutePlanDetailID = new SqlParameter
                    {
                        ParameterName = "@RoutePlanDetailID",
                        Value = sRoutePlanDetailID,
                        SqlDbType = SqlDbType.VarChar,
                    };

                    SqlParameter[] paramaters = new SqlParameter[] { IncomingArrangementID, RoutePlanDetailID };
                    command.Parameters.AddRange(paramaters);
                    connection.Open();
                    command.ExecuteNonQuery();
                }

                // Update Inventory Status for target arrangment
                sql = $" Update ArrangementInventory " +
                      $" Set InventoryStatusID = (Select InventoryStatusID from InventoryStatus where Code = 'Allocated') " +
                      $" Where ArrangementInventoryID = @IncomingArrangementID" +
                      $" and  InventoryStatusID <> (Select InventoryStatusID from InventoryStatus where Code = 'InUse')";

                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    command.Parameters.Clear();
                    SqlParameter IncomingArrangementID = new SqlParameter
                    {
                        ParameterName = "@IncomingArrangementID",
                        Value = routePlanStopDetail.IncomingArrangementInventoryID,
                        SqlDbType = SqlDbType.VarChar
                    };
                    SqlParameter[] paramaters = new SqlParameter[] { IncomingArrangementID };
                    command.Parameters.AddRange(paramaters);
                    command.ExecuteNonQuery();
                }

                // Update Inventory Status for replaced arrangment
                sql = $" Update ArrangementInventory " +
                      $" Set InventoryStatusID = (Select InventoryStatusID from InventoryStatus where Code = 'Available') " +
                      $" Where ArrangementInventoryID = @ExitingArrangementInventoryID" +
                      $" and  InventoryStatusID <> (Select InventoryStatusID from InventoryStatus where Code = 'InUse')";

                if ( !String.IsNullOrEmpty(sExitingArrangementInventoryID))
                {
                    using (SqlCommand command = new SqlCommand(sql, connection))
                    {
                        command.Parameters.Clear();
                        SqlParameter ExitingArrangementInventoryID = new SqlParameter
                        {
                            ParameterName = "@ExitingArrangementInventoryID",
                            Value = sExitingArrangementInventoryID,
                            SqlDbType = SqlDbType.VarChar
                        };
                        SqlParameter[] paramaters = new SqlParameter[] { ExitingArrangementInventoryID };
                        command.Parameters.AddRange(paramaters);
                        command.ExecuteNonQuery();
                    }

                }

                connection.Close();
            }

            return RedirectToAction("Index");
        }

        public ActionResult Create()
        {
            string connectionString = Configuration["ConnectionStrings:SilkDesigns"];
            RoutePlan routePlan = new RoutePlan();
            routePlan.AvailableRoutes = SilkDesignUtility.GetRoutes(connectionString);
            return View(routePlan);
        }

        [HttpPost]
        public IActionResult Create(RoutePlan routePlan)
        {
            string connectionString = Configuration["ConnectionStrings:SilkDesigns"];
            string sArrangementInventoryID = string.Empty;

            string sRoutePlanID = SilkDesignUtility.GetNewID(connectionString);
            routePlan.RoutePlanID = sRoutePlanID;
            SilkDesignUtility.CreateRoutePlan(connectionString, routePlan);


            // TODO Add error checking


            return RedirectToAction("Index");
        }
    }
}
