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
        string msUserName = string.Empty;
        string msUserID = string.Empty;
        string msIsAdmin = string.Empty;
        public RoutePlanController(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IActionResult Index()
        {
            string sErrorMsg = string.Empty;
            ISession currentSession = HttpContext.Session;
            if (!ControllersShared.IsLoggedOn(currentSession, ref msUserID, ref msUserName, ref msIsAdmin))
            {
                return RedirectToAction("Login", "Login");
            }

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
                    " join Route r on r.RouteID = rp.RouteID and r.UserID = @UserID " +
                    " join RoutePlanStatus rps on rps.RoutePlanStatusID = rp.RoutePlanStatusID " +
                    " where rp.UserID = @UserID " +
                    " and rps.Code = 'Planning' " +
                    " Order by rp.RouteDate Desc";

                SqlCommand readcommand = new SqlCommand(sql, connection);
                SqlParameter parameter = new SqlParameter
                {
                    ParameterName = "@UserID",
                    Value = msUserID,
                    SqlDbType = SqlDbType.VarChar
                };
                readcommand.Parameters.Add(parameter);
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
        public IActionResult Closed ()
        {
            string sErrorMsg = string.Empty;
            ISession currentSession = HttpContext.Session;
            if (!ControllersShared.IsLoggedOn(currentSession, ref msUserID, ref msUserName, ref msIsAdmin))
            {
                return RedirectToAction("Login", "Login");
            }

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
                    " join Route r on r.RouteID = rp.RouteID and r.UserID = @UserID " +
                    " join RoutePlanStatus rps on rps.RoutePlanStatusID = rp.RoutePlanStatusID " +
                    " where rp.UserID = @UserID " +
                    " and rps.Code in ('Cancelled', 'Finalized') " +
                    " Order by rp.RouteDate Desc";

                SqlCommand readcommand = new SqlCommand(sql, connection);
                SqlParameter parameter = new SqlParameter
                {
                    ParameterName = "@UserID",
                    Value = msUserID,
                    SqlDbType = SqlDbType.VarChar
                };
                readcommand.Parameters.Add(parameter);
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
            string sErrorMsg = string.Empty;
            ISession currentSession = HttpContext.Session;
            if (!ControllersShared.IsLoggedOn(currentSession, ref msUserID, ref msUserName, ref msIsAdmin))
            {
                return RedirectToAction("Login", "Login");
            }

            string sRoutePlanID = id;
            string connectionString = Configuration["ConnectionStrings:SilkDesigns"];

            dynamic RouteDetails = new ExpandoObject();
            RouteDetails.RoutePlans = SilkDesignUtility.GetRoutePlans(connectionString, sRoutePlanID, msUserID, ref sErrorMsg);
            if (!String.IsNullOrEmpty(sErrorMsg))
            {
                ViewBag.Detail = sErrorMsg;
                return View();
            }
            RouteDetails.RoutePlanDetails = SilkDesignUtility.GetRoutePlanDetails(connectionString, sRoutePlanID, msUserID, ref sErrorMsg);
            if (!String.IsNullOrEmpty(sErrorMsg))
            {
                ViewBag.Detail = sErrorMsg;
                return View();
            }
            return View(RouteDetails);
        }

        [HttpPost]
        public IActionResult Update(RoutePlan routePlan, string id)
        {
            string sErrorMsg = string.Empty;
            ISession currentSession = HttpContext.Session;
            if (!ControllersShared.IsLoggedOn(currentSession, ref msUserID, ref msUserName, ref msIsAdmin))
            {
                return RedirectToAction("Login", "Login");
            }

            string sRoutePlanID = id;
            string connectionString = Configuration["ConnectionStrings:SilkDesigns"];
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string sql = $"Update RoutePlan SET " +
                                    $" RouteDate         = @Date, " +
                                    $" Description       = @Description " +
                                    $" Where RoutePlanID = @RoutePlanID " +
                                    $" and UserID = @UserID ";

                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    command.Parameters.Clear();
                    SqlParameter parameter = new SqlParameter
                    {
                        ParameterName = "@Date",
                        Value = routePlan.RouteDate,
                        SqlDbType = SqlDbType.DateTime
                    };
                    command.Parameters.Add(parameter);
                    parameter = new SqlParameter
                    {
                        ParameterName = "@Description",
                        Value = routePlan.Description,
                        SqlDbType = SqlDbType.VarChar,
                        Size = 250
                    };
                    command.Parameters.Add(parameter);
                    parameter = new SqlParameter
                    {
                        ParameterName = "@RoutePlanID",
                        Value = id,
                        SqlDbType = SqlDbType.VarChar
                    };
                    command.Parameters.Add(parameter);
                    parameter = new SqlParameter
                    {
                        ParameterName = "@UserID",
                        Value = msUserID,
                        SqlDbType = SqlDbType.VarChar
                    };
                    command.Parameters.Add(parameter);

                    try
                    {
                        connection.Open();
                        command.ExecuteNonQuery();
                    }
                    catch (Exception ex)
                    {
                        sErrorMsg = "Unable to update route plan. " + ex.Message;
                    }

                }
                connection.Close();
            }

            return RedirectToAction("Index");
        }

        public IActionResult UpdateRoutePlanStop(string id)
        {
            string sErrorMsg = string.Empty;
            ISession currentSession = HttpContext.Session;
            if (!ControllersShared.IsLoggedOn(currentSession, ref msUserID, ref msUserName, ref msIsAdmin))
            {
                return RedirectToAction("Login", "Login");
            }

            string connectionString = Configuration["ConnectionStrings:SilkDesigns"];
            //RoutePlanStop routePlanStop = new RoutePlanStop(); //  SilkDesignUtility.GetRoutPlanStop
            RoutePlanStop routePlanStopDetail = SilkDesignUtility.GetRoutePlanDetail(connectionString, id, msUserID, ref sErrorMsg);
            return View(routePlanStopDetail);
        }

        [HttpPost]
        public IActionResult UpdateRoutePlanStop(RoutePlanStop routePlanStopDetail, string id)
        {
            string sErrorMsg = string.Empty;
            ISession currentSession = HttpContext.Session;
            if (!ControllersShared.IsLoggedOn(currentSession, ref msUserID, ref msUserName, ref msIsAdmin))
            {
                return RedirectToAction("Login", "Login");
            }

            string sRoutePlanDetailInventoryID = id;
            string connectionString = Configuration["ConnectionStrings:SilkDesigns"];
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string sExitingArrangementInventoryID = SilkDesignUtility.GetSuggestedInventoryID(connectionString, sRoutePlanDetailInventoryID, msUserID);

                string sql = $" Update RoutePlanDetailInventory SET " +
                             $" IncomingArrangementInventoryID = @IncomingArrangementID " +
                             $" Where RoutePlanDetailInventoryID=@RoutePlanDetailInventoryID";

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
                        ParameterName = "@RoutePlanDetailInventoryID",
                        Value = sRoutePlanDetailInventoryID,
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
            string sErrorMsg = string.Empty;
            ISession currentSession = HttpContext.Session;
            if (!ControllersShared.IsLoggedOn(currentSession, ref msUserID, ref msUserName, ref msIsAdmin))
            {
                return RedirectToAction("Login", "Login");
            }

            string connectionString = Configuration["ConnectionStrings:SilkDesigns"];
            RoutePlan routePlan = new RoutePlan();
            routePlan.AvailableRoutes = SilkDesignUtility.GetRoutes(connectionString, msUserID, ref sErrorMsg);
            if (!String.IsNullOrEmpty(sErrorMsg))
            {
                ViewBag.Result = sErrorMsg;
                return View();
            }
            return View(routePlan);
        }

        [HttpPost]
        public IActionResult Create(RoutePlan routePlan)
        {
            string sErrorMsg = string.Empty;
            ISession currentSession = HttpContext.Session;
            if (!ControllersShared.IsLoggedOn(currentSession, ref msUserID, ref msUserName, ref msIsAdmin))
            {
                return RedirectToAction("Login", "Login");
            }
            string connectionString = Configuration["ConnectionStrings:SilkDesigns"];
            string sArrangementInventoryID = string.Empty;

            string sRoutePlanID = SilkDesignUtility.GetNewID(connectionString);
            routePlan.RoutePlanID = sRoutePlanID;
            SilkDesignUtility.CreateRoutePlan(connectionString, routePlan, msUserID, ref sErrorMsg);
            if (!String.IsNullOrEmpty(sErrorMsg))
            {
                ViewBag.Result = sErrorMsg;
                return View();
            }
            SilkDesignUtility.CreateRoutePlanDetails(connectionString, routePlan.RouteID, routePlan.RoutePlanID, msUserID, ref sErrorMsg);
            if (!String.IsNullOrEmpty(sErrorMsg))
            {
                ViewBag.Result = sErrorMsg;
                return View();
            }
            SilkDesignUtility.PopulateIncoming(connectionString, routePlan.RoutePlanID, msUserID, ref sErrorMsg);
            SilkDesignUtility.SetDestinationStatus(connectionString, routePlan.RoutePlanID, Dispositions.ToWareHouse);
            // TODO Add error checking


            return RedirectToAction("Index");
        }

        public ActionResult CancelPlan(string id)
        {
            string sErrorMsg = string.Empty;
            ISession currentSession = HttpContext.Session;
            if (!ControllersShared.IsLoggedOn(currentSession, ref msUserID, ref msUserName, ref msIsAdmin))
            {
                return RedirectToAction("Login", "Login");
            }

            string sRoutePlanID = id;
            string connectionString = Configuration["ConnectionStrings:SilkDesigns"];
            //List<RoutePlanDetail> lRoutePlanDetails = SilkDesignUtility.GetRoutePlanDetails(connectionString, sRoutePlanID);
            //dynamic RouteDetails = new ExpandoObject();
            //RouteDetails.Stops = SilkDesignUtility.GetRoutePlanDetails(connectionString, sRoutePlanID);
            string sResult = SilkDesignUtility.CancelPlan(connectionString, sRoutePlanID, msUserID, ref sErrorMsg);
            if (!String.IsNullOrEmpty(sErrorMsg))
            {
                ViewBag.Result = sErrorMsg;
                return View();
            }

            return RedirectToAction("Index");
        }
        public ActionResult FinalizePlan(string id)
        {
            string sErrorMsg = string.Empty;
            ISession currentSession = HttpContext.Session;
            if (!ControllersShared.IsLoggedOn(currentSession, ref msUserID, ref msUserName, ref msIsAdmin))
            {
                return RedirectToAction("Login", "Login");
            }

            string sRoutePlanID = id;
            string connectionString = Configuration["ConnectionStrings:SilkDesigns"];
            //List<RoutePlanDetail> lRoutePlanDetails = SilkDesignUtility.GetRoutePlanDetails(connectionString, sRoutePlanID);
            //dynamic RouteDetails = new ExpandoObject();
            //RouteDetails.Stops = SilkDesignUtility.GetRoutePlanDetails(connectionString, sRoutePlanID);
            string sResult = SilkDesignUtility.FinalizePlan(connectionString, sRoutePlanID, msUserID, ref sErrorMsg);

            return RedirectToAction("Index");
        }
    }
}
