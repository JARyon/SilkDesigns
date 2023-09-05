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
            string connectionString = Configuration["ConnectionStrings:SilkDesigns"];

            string sSelectedInventoryID = routePlanStopDetail.IncomingArrangementInventoryID;
            RoutePlanStop oCurrentStop = SilkDesignUtility.GetRoutePlanDetail(connectionString, id, msUserID, ref sErrorMsg);

            string sRoutePDInventoryID = id;
            string sSelectedRoutePlanDetailInventoryID = string.Empty;
            string sOutgoingDisposition = string.Empty;
            string sIncomingDisposition = string.Empty;
            string sInventoryID = string.Empty;
            bool OverrideIsSelectedFromPlan = IsSelectedFromPlan(connectionString, sSelectedInventoryID, oCurrentStop.RoutePlanID, ref sSelectedRoutePlanDetailInventoryID, ref  sOutgoingDisposition);

            // if override is to keep the same arrangemet at the location....
            if (sSelectedInventoryID == oCurrentStop.OutgoingArrangementInventoryID)
            {
                if (oCurrentStop.IncomingDisposition.Trim() == "From Truck")  //pulled from previous stop on route
                {
                    // Locatate associated stop by outgoing InventoryID (selected id)
                    string AssocatedPDInventoryID = SilkDesignUtility.GetAssocPDInventoryID(connectionString, oCurrentStop, sSelectedInventoryID);
                    sIncomingDisposition = "";
                    sOutgoingDisposition = "Warehouse";
                    sInventoryID = "";
                    UpdateRoutePlanDetailInventory(connectionString, AssocatedPDInventoryID, sInventoryID, sIncomingDisposition, sOutgoingDisposition);
                }
                
                sIncomingDisposition = "Location";
                sOutgoingDisposition = "Location";
                sInventoryID = sSelectedInventoryID;
                UpdateRoutePlanDetailInventory(connectionString, sRoutePDInventoryID, sInventoryID, sIncomingDisposition, sOutgoingDisposition);

            }
            // Assigning a previouly outgoing arrangement as the override
            else if (OverrideIsSelectedFromPlan) 
            {
                if (oCurrentStop.IncomingDisposition.Trim() == "From Truck")  //pulled from previous stop on route
                {
                    //set slected arrangmenent stop to dest = to truck
                    string sSelectedArrangmentPDInventoryID = SilkDesignUtility.GetPlanDetailInventoryID(connectionString, oCurrentStop, sSelectedInventoryID);
                    sIncomingDisposition = "";
                    sOutgoingDisposition = "To Truck";
                    sInventoryID = "";
                    UpdateRoutePlanDetailInventory(connectionString, sSelectedArrangmentPDInventoryID, sInventoryID, sIncomingDisposition, sOutgoingDisposition);

                    // Locatate associated stop by outgoing InventoryID (selected id)
                    string AssocatedPDInventoryID = SilkDesignUtility.GetAssocPDInventoryID(connectionString, oCurrentStop, sSelectedInventoryID);
                    sIncomingDisposition = "";
                    sOutgoingDisposition = "Warehouse";
                    sInventoryID = "";
                    UpdateRoutePlanDetailInventory(connectionString, AssocatedPDInventoryID, sInventoryID, sIncomingDisposition, sOutgoingDisposition);

                    //Update current stop with selected Arrangment and new source
                    sIncomingDisposition = "From Truck";
                    sOutgoingDisposition = "";
                    sInventoryID = sSelectedInventoryID;
                    UpdateRoutePlanDetailInventory(connectionString, sRoutePDInventoryID, sInventoryID, sIncomingDisposition, sOutgoingDisposition);

                }
                else //pulled from warehouse
                {
                    //set slected arrangmenent stop to dest = to truck
                    string sSelectedArrangmentPDInventoryID = SilkDesignUtility.GetPlanDetailInventoryID(connectionString, oCurrentStop, sSelectedInventoryID);
                    sIncomingDisposition = "";
                    sOutgoingDisposition = "To Truck";
                    sInventoryID = "";
                    UpdateRoutePlanDetailInventory(connectionString, sSelectedArrangmentPDInventoryID, sInventoryID, sIncomingDisposition, sOutgoingDisposition);

                    //Update current stop with selected Arrangment and new source
                    sIncomingDisposition = "From Truck";
                    sOutgoingDisposition = "Warehouse";
                    sInventoryID = sSelectedInventoryID;
                    UpdateRoutePlanDetailInventory(connectionString, sRoutePDInventoryID, sInventoryID, sIncomingDisposition, sOutgoingDisposition);
                }
            }
            // the selected arrangment is from the warehouse
            else
            {
                if (oCurrentStop.IncomingDisposition.Trim() == "From Truck")  //was expecting from previous stop
                {
                    // Locatate associated stop by outgoing InventoryID (selected id)
                    string AssocatedPDInventoryID = SilkDesignUtility.GetAssocPDInventoryID(connectionString, oCurrentStop, sSelectedInventoryID);
                    sIncomingDisposition = "";
                    sOutgoingDisposition = "Warehouse";
                    sInventoryID = "";
                    UpdateRoutePlanDetailInventory(connectionString, AssocatedPDInventoryID, sInventoryID, sIncomingDisposition, sOutgoingDisposition);

                    //Update current stop with selected Arrangment and new source
                    sIncomingDisposition = "Warehouse";
                    sOutgoingDisposition = "";
                    sInventoryID = sSelectedInventoryID;
                    UpdateRoutePlanDetailInventory(connectionString, sRoutePDInventoryID, sInventoryID, sIncomingDisposition, sOutgoingDisposition);
                }
                else
                {
                    //Update current stop with selected Arrangment and new source
                    sIncomingDisposition = "Warehouse";
                    sOutgoingDisposition = "Warehouse";
                    sInventoryID = sSelectedInventoryID;
                    UpdateRoutePlanDetailInventory(connectionString, sRoutePDInventoryID, sInventoryID, sIncomingDisposition, sOutgoingDisposition);

                }
            }
            using (SqlConnection connection = new SqlConnection(connectionString))
                {
                string sExitingArrangementInventoryID = oCurrentStop.IncomingArrangementInventoryID;

                // Update Inventory Status for target arrangment
                string sql = $" Update ArrangementInventory " +
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
                    connection.Open();
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

        private void UpdateRoutePlanDetailInventory(string? connectionString, string sRoutePlanDetailInventoryID, string sSelectedInventoryID, string sIncomingDisposition, string sOutgoingDisposition)
        {
            string sDelimiter = "";
            string sSql = "Update RoutePlanDetailInventory SET ";
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                if (!String.IsNullOrEmpty(sSelectedInventoryID))
                {
                    sSql += $"    IncomingArrangementInventoryID = '{sSelectedInventoryID}' ";
                    sDelimiter = ",";
                }
                if (!String.IsNullOrEmpty(sIncomingDisposition))
                {
                    sSql += sDelimiter + $"   IncomingDisposition = '{sIncomingDisposition}' ";
                    sDelimiter = ",";
                }
                if (!String.IsNullOrEmpty(sOutgoingDisposition))
                {
                    sSql += sDelimiter + $"   OutgoingDisposition = '{sOutgoingDisposition}' ";
                }
                sSql +=   $" Where RoutePlanDetailInventoryID='{sRoutePlanDetailInventoryID}'";
                using (SqlCommand command = new SqlCommand(sSql, connection))
                {
                    try
                    {
                        connection.Open();
                        command.ExecuteNonQuery();
                        connection.Close();
                    }
                    catch (Exception ex)
                    {
                        if (connection.State == ConnectionState.Open)
                            connection.Close();
                    }
                }
            }
        }

        private bool IsSelectedFromPlan(string? connectionString, string sSelectedInventoryID, string sRoutePlanID, ref string sRoutePlanDetailInventoryID, ref string sOutgoingDisposition)
        {
            bool bRetValue = false;

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                string sSql = $" select " +
                              $"   rpdi.RoutePlanDetailInventoryID RoutePlanDetailInventoryID, " +
                              $"   rpdi.OutgoingDisposition        OutgoingDisposition " +
                              $" from " +
                              $"   routePlanDetail rpd " +
                              $"   join routePlanDetailInventory rpdi on rpdi.routePlanDetailID = rpd.RoutePlanDetailID " +
                              $" where " +
                              $"   routePlanID =@RoutePlanID  " +
                              $"   and rpdi.OutgoingArrangementInventoryID = @SelectedInventoryID ";

                using (SqlCommand command = new SqlCommand(sSql, connection))
                {
                    command.Parameters.Clear();
                    SqlParameter parameter = new SqlParameter
                    {
                        ParameterName = "@RoutePlanID",
                        Value = sRoutePlanID,
                        SqlDbType = SqlDbType.VarChar
                    };
                    command.Parameters.Add(parameter);

                    parameter = new SqlParameter
                    {
                        ParameterName = "@SelectedInventoryID",
                        Value = sSelectedInventoryID,
                        SqlDbType = SqlDbType.VarChar
                    };
                    command.Parameters.Add(parameter);
                    using (SqlDataReader dr = command.ExecuteReader())
                    {
                        if (dr.HasRows)
                        {
                            while (dr.Read())
                            {
                                sRoutePlanDetailInventoryID = Convert.ToString(dr["RoutePlanDetailInventoryID"]);
                                sOutgoingDisposition = Convert.ToString(dr["OutgoingDisposition"]);
                                bRetValue = true;

                                break;
                            }
                        }
                    }
                    connection.Close();
                }
            }
            return bRetValue;
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
