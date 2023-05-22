using Microsoft.AspNetCore.Mvc;
using SilkDesign.Models;
using System.Data.SqlClient;
using System.Data;
using System.Dynamic;
using SilkDesign.Shared;
using Microsoft.AspNetCore.Routing;

namespace SilkDesign.Controllers
{
    public class RouteController : Controller
    {
        public IConfiguration Configuration { get; }
        string msUserName = string.Empty;
        string msUserID = string.Empty;
        string msIsAdmin = string.Empty;

        public RouteController(IConfiguration configuration)
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


            List<Models.Route> ivmList = new List<Models.Route>();
            string connectionString = Configuration["ConnectionStrings:SilkDesigns"];
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                string sql = "SELECT " +
                    " r.RouteId      ID " +
                    " ,r.Name        NAME " +
                    " ,r.Description DESCRIPTION " +
                    " FROM Route r " +
                    " WHERE Deleted = 'N' " +
                    " AND UserID = @UserID ";

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

                        Models.Route ivm = new Models.Route();
                        ivm.RouteId = Convert.ToString(dr["ID"]);
                        ivm.Name = Convert.ToString(dr["NAME"]);
                        ivm.Description = Convert.ToString(dr["DESCRIPTION"]);
                        ivm.UserID = msUserID;
                        ivmList.Add(ivm);
                    }
                }
                connection.Close();
            }

            return View(ivmList);
        }
        public ActionResult Create()
        {
            ISession currentSession = HttpContext.Session;
            if (!ControllersShared.IsLoggedOn(currentSession, ref msUserID, ref msUserName, ref msIsAdmin))
            {
                return RedirectToAction("Login", "Login");
            }

            string connectionString = Configuration["ConnectionStrings:SilkDesigns"];
            SilkDesign.Models.Route route = new SilkDesign.Models.Route();
            route.UserID = msUserID;

            //ViewBag.ListOfWarehouses = SilkDesignUtility.GetWarehouses(connectionString);
            route.Warehouses = SilkDesignUtility.GetWarehouses(connectionString, msUserID);
            route.WarehouseID = "0";
            return View(route);

        }

        [HttpPost]
        public IActionResult Create(SilkDesign.Models.Route route)
        {
            string sErrorMsg = string.Empty;
            ISession currentSession = HttpContext.Session;
            if (!ControllersShared.IsLoggedOn(currentSession, ref msUserID, ref msUserName, ref msIsAdmin))
            {
                return RedirectToAction("Login", "Login");
            }

            //string strDDLValue = Request.Form["ddlWarehouse"].ToString();
            string connectionString = Configuration["ConnectionStrings:SilkDesigns"];

            var errors = ModelState
                .Where(x => x.Value.Errors.Count > 0)
                .Select(x => new { x.Key, x.Value.Errors })
                .ToArray();

            if (ModelState.IsValid)
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    string sql = "Insert Into Route (Name, Description, WarehouseID, UserID) Values (@Name, @Description, @WarehouseID, @UserID)";

                    using (SqlCommand command = new SqlCommand(sql, connection))
                    {
                        command.CommandType = CommandType.Text;

                        // adding parameters
                        SqlParameter parameter = new SqlParameter
                        {
                            ParameterName = "@Name",
                            Value = route.Name,
                            SqlDbType = SqlDbType.VarChar,
                            Size = 50
                        };
                        command.Parameters.Add(parameter);

                        parameter = new SqlParameter
                        {
                            ParameterName = "@Description",
                            Value = route.Description,
                            SqlDbType = SqlDbType.VarChar
                        };
                        command.Parameters.Add(parameter);

                        parameter = new SqlParameter
                        {
                            ParameterName = "@WarehouseID",
                            Value = route.WarehouseID,
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

                        connection.Open();
                        command.ExecuteNonQuery();
                        connection.Close();
                    }
                }
                //ViewBag.Result = "Success";
                ViewBag.ListOfWarehouses = SilkDesignUtility.GetWarehouses(connectionString, msUserID);
                //return View();
                return RedirectToAction("Index");
            }
            else
            {
                route.Warehouses = SilkDesignUtility.GetWarehouses(connectionString, msUserID  );
                route.WarehouseID = "0";
                return View(route);
            }
        }

        public IActionResult Update(string id)
        {
            string sErrorMsg = string.Empty;

            ISession currentSession = HttpContext.Session;
            if (!ControllersShared.IsLoggedOn(currentSession, ref msUserID, ref msUserName, ref msIsAdmin))
            {
                return RedirectToAction("Login", "Login");
            }

            string sRouteID = id;
            string connectionString = Configuration["ConnectionStrings:SilkDesigns"];

            #region OldCode
            //Models.Route route = new Models.Route();
            //using (SqlConnection connection = new SqlConnection(connectionString))
            //{
            //    string sql = $"Select * From Route Where RouteId='{id}'";
            //    SqlCommand command = new SqlCommand(sql, connection);

            //    connection.Open();

            //    using (SqlDataReader dataReader = command.ExecuteReader())
            //    {
            //        while (dataReader.Read())
            //        {
            //            route.RouteId = Convert.ToInt32(dataReader["RouteId"]);
            //            route.Name = Convert.ToString(dataReader["Name"]);
            //            route.Description = Convert.ToString(dataReader["Description"]);
            //        }
            //    }

            //    connection.Close();
            //}
            #endregion OldCode;
            dynamic RouteCustomers = new ExpandoObject();
            RouteCustomers.Routes = SilkDesignUtility.GetRoutes(connectionString, sRouteID, msUserID, ref sErrorMsg);
            if (!String.IsNullOrEmpty(sErrorMsg))
            {
                ViewBag.Result = sErrorMsg;
                return View();
            }
            RouteCustomers.RouteLocations = SilkDesignUtility.GetRouteLocations(connectionString, sRouteID, msUserID, ref sErrorMsg);
            if (!String.IsNullOrEmpty(sErrorMsg))
            {
                ViewBag.Result = sErrorMsg;
                return View();
            }
            return View(RouteCustomers);
        }

        [HttpPost]
        public IActionResult Update(Models.Route route, string id)
        {
            string sErrorMsg = string.Empty;

            ISession currentSession = HttpContext.Session;
            if (!ControllersShared.IsLoggedOn(currentSession, ref msUserID, ref msUserName, ref msIsAdmin))
            {
                return RedirectToAction("Login", "Login");
            }

            string connectionString = Configuration["ConnectionStrings:SilkDesigns"];
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string sql = $" Update Route " +
                             $" SET Name= @RouteName, " +
                             $" Description= @Description " +
                             $" Where RouteId= @RouteID ";
                SqlCommand command = new SqlCommand(sql, connection);
                SqlParameter parameter = new SqlParameter
                {
                    ParameterName = "@RouteName",
                    Value = route.Name,
                    SqlDbType = SqlDbType.VarChar
                };
                command.Parameters.Add(parameter);
                parameter = new SqlParameter
                {
                    ParameterName = "@Description",
                    Value = route.Description,
                    SqlDbType = SqlDbType.VarChar
                };
                command.Parameters.Add(parameter);
                parameter = new SqlParameter
                {
                    ParameterName = "@RouteID",
                    Value = id,
                    SqlDbType = SqlDbType.VarChar
                };
                command.Parameters.Add(parameter);
                {
                    connection.Open();
                    command.ExecuteNonQuery();
                    connection.Close();
                }

                try
                {
                    connection.Open();
                    command.ExecuteNonQuery();
                    connection.Close();
                }
                catch (Exception ex)
                {
                    sErrorMsg = "Unable to update route." + ex.Message;
                }
            }

            return RedirectToAction("Index");
        }

        public IActionResult UpdateRouteOrder(string id)
        {
            string sErrorMsg = string.Empty;

            ISession currentSession = HttpContext.Session;
            if (!ControllersShared.IsLoggedOn(currentSession, ref msUserID, ref msUserName, ref msIsAdmin))
            {
                return RedirectToAction("Login", "Login");
            }

            string sRouteLocationID = id;
            string connectionString = Configuration["ConnectionStrings:SilkDesigns"];
            RouteLocation selectedLocation = SilkDesignUtility.GetRouteLocation(connectionString, sRouteLocationID, msUserID, ref sErrorMsg);
            if (!String.IsNullOrEmpty(sErrorMsg))
            {
                ViewBag.Result = sErrorMsg;
                return View();
            }
            return View(selectedLocation);
        }

        [HttpPost]
        public IActionResult UpdateRouteOrder(RouteLocation routeLocation, string id)
        {
            string sErrorMsg = string.Empty;

            ISession currentSession = HttpContext.Session;
            if (!ControllersShared.IsLoggedOn(currentSession, ref msUserID, ref msUserName, ref msIsAdmin))
            {
                return RedirectToAction("Login", "Login");
            }

            string connectionString = Configuration["ConnectionStrings:SilkDesigns"];
            string sRouteLocationID = id;
            int iOldValue = routeLocation.OldRouteOrder;
            int iNewValue = routeLocation.RouteOrder;
            string sRouteID = routeLocation.RouteID;
            if (iOldValue > iNewValue)
            {
                // moving the location up in the order 
                SilkDesignUtility.MoveLocationUp(connectionString, iOldValue, iNewValue, sRouteID, sRouteLocationID, msUserID, ref sErrorMsg );
                if (!String.IsNullOrEmpty(sErrorMsg))
                {
                    ViewBag.Result = sErrorMsg;
                    return View();
                }
            }
            if (iNewValue > iOldValue)
            {
                SilkDesignUtility.MoveLocationDown(connectionString, iOldValue, iNewValue, sRouteID, sRouteLocationID, msUserID, ref sErrorMsg);
                {
                    if (!String.IsNullOrEmpty(sErrorMsg))
                    {
                        ViewBag.Result = sErrorMsg;
                        return View();
                    }
                }
            }
            return RedirectToAction("Index");
        }
        public IActionResult RouteAddStop(string id)
        {
            string sErrorMsg = string.Empty;

            ISession currentSession = HttpContext.Session;
            if (!ControllersShared.IsLoggedOn(currentSession, ref msUserID, ref msUserName, ref msIsAdmin))
            {
                return RedirectToAction("Login", "Login");
            }

            string sRouteID = id;
            string connectionString = Configuration["ConnectionStrings:SilkDesigns"];
            RouteLocation routeLocation = new RouteLocation();
            routeLocation.AvailableLocations = SilkDesignUtility.GetAvailableLocations(connectionString, sRouteID, msUserID, ref sErrorMsg);
            if (!String.IsNullOrEmpty(sErrorMsg))
            {
                ViewBag.Result = sErrorMsg;
                return View();
            }
            return View(routeLocation);
        }
        [HttpPost]
        public IActionResult RouteAddStop(RouteLocation routeLocation, string id)
        {
            string sErrorMsg = string.Empty;

            ISession currentSession = HttpContext.Session;
            if (!ControllersShared.IsLoggedOn(currentSession, ref msUserID, ref msUserName, ref msIsAdmin))
            {
                return RedirectToAction("Login", "Login");
            }

            string sRouteID = id;
            string connectionString = Configuration["ConnectionStrings:SilkDesigns"];
            SilkDesignUtility.CreateRouteLocation(connectionString, routeLocation, sRouteID, msUserID, ref sErrorMsg);
            if (!String.IsNullOrEmpty(sErrorMsg))
            {
                ViewBag.Result = sErrorMsg;
                return View();
            }
            return RedirectToAction("Index");
        }
        public ActionResult InactivateRoute(string id)
        {
            string sErrorMsg = string.Empty;

            ISession currentSession = HttpContext.Session;
            if (!ControllersShared.IsLoggedOn(currentSession, ref msUserID, ref msUserName, ref msIsAdmin))
            {
                return RedirectToAction("Login", "Login");
            }

            string sRouteID = id;
            string connectionString = Configuration["ConnectionStrings:SilkDesigns"];
            string sResult = SilkDesignUtility.DeactivateRoute(connectionString, sRouteID, msUserID, ref sErrorMsg);
            if (!String.IsNullOrEmpty(sErrorMsg))
            {
                ViewBag.Result = sErrorMsg;
                return View();
            }
            return RedirectToAction("Index");
        }
    }
}
