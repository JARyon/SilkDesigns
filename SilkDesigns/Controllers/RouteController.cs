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

        public RouteController(IConfiguration configuration)
        {
            Configuration = configuration;
        }


        public IActionResult Index()
        {

            List<Models.Route> ivmList = new List<Models.Route>();
            string connectionString = Configuration["ConnectionStrings:SilkDesigns"];
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                string sql = "SELECT " +
                    " r.RouteId      ID " +
                    " ,r.Name        NAME " +
                    " ,r.Description DESCRIPTION " +
                    " FROM Route r ";

                SqlCommand readcommand = new SqlCommand(sql, connection);

                using (SqlDataReader dr = readcommand.ExecuteReader())
                {
                    while (dr.Read())
                    {

                        Models.Route ivm = new Models.Route();
                        ivm.RouteId = Convert.ToString(dr["ID"]);
                        ivm.Name = Convert.ToString(dr["NAME"]);
                        ivm.Description = Convert.ToString(dr["DESCRIPTION"]);
                        ivmList.Add(ivm);
                    }
                }
                connection.Close();
            }

            return View(ivmList);
        }
        public ActionResult Create()
        {
            string connectionString = Configuration["ConnectionStrings:SilkDesigns"];
            SilkDesign.Models.Route route = new SilkDesign.Models.Route();
            ViewBag.ListOfWarehouses = SilkDesignUtility.GetWarehouses(connectionString);
            route.WarehouseID = "0";
            return View(route);

        }

        [HttpPost]
        public IActionResult Create(SilkDesign.Models.Route route)
        {

            string strDDLValue = Request.Form["ddlWarehouse"].ToString();
            string connectionString = Configuration["ConnectionStrings:SilkDesigns"];
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string sql = "Insert Into Route (Name, Description, WarehouseID) Values (@Name, @Description, @WarehouseID)";

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
                        Value = strDDLValue,
                        SqlDbType = SqlDbType.VarChar
                    };
                    command.Parameters.Add(parameter);

                    connection.Open();
                    command.ExecuteNonQuery();
                    connection.Close();
                }
            }
            ViewBag.Result = "Success";
            ViewBag.ListOfWarehouses = SilkDesignUtility.GetWarehouses(connectionString);
            //return View();
            return RedirectToAction("Index");
        }

        public IActionResult Update(string id)
        {
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
            RouteCustomers.Routes = SilkDesignUtility.GetRoutes(connectionString, sRouteID);
            RouteCustomers.RouteLocations = SilkDesignUtility.GetRouteLocations(connectionString, sRouteID);

            return View(RouteCustomers);
        }

        [HttpPost]
        public IActionResult Update(Models.Route route, string id)
        {
            string connectionString = Configuration["ConnectionStrings:SilkDesigns"];
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string sql = $"Update Route SET Name='{route.Name}', Description='{route.Description}' Where RouteId='{id}'";
                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    connection.Open();
                    command.ExecuteNonQuery();
                    connection.Close();
                }
            }

            return RedirectToAction("Index");
        }

        public IActionResult UpdateRouteOrder(string id)
        {
            string sRouteLocationID = id;
            string connectionString = Configuration["ConnectionStrings:SilkDesigns"];
            RouteLocation selectedLocation = SilkDesignUtility.GetRouteLocation(connectionString, sRouteLocationID);
            return View(selectedLocation);
        }

        [HttpPost]
        public IActionResult UpdateRouteOrder(RouteLocation routeLocation, string id)
        {
            string connectionString = Configuration["ConnectionStrings:SilkDesigns"];
            string sRouteLocationID = id;
            int iOldValue = routeLocation.OldRouteOrder;
            int iNewValue = routeLocation.RouteOrder;
            string sRouteID = routeLocation.RouteID;
            if (iOldValue > iNewValue)
            {
                // moving the location up in the order 
                SilkDesignUtility.MoveLocationUp(connectionString, iOldValue, iNewValue, sRouteID, sRouteLocationID);

            }
            if (iNewValue > iOldValue)
            {
                SilkDesignUtility.MoveLocationDown(connectionString, iOldValue, iNewValue, sRouteID, sRouteLocationID);
            }
            return RedirectToAction("Index");
        }
        public IActionResult RouteAddStop(string id)
        {
            string sRouteID = id;
            string connectionString = Configuration["ConnectionStrings:SilkDesigns"];
            RouteLocation routeLocation = new RouteLocation();
            routeLocation.AvailableLocations = SilkDesignUtility.GetAvailableLocations(connectionString, sRouteID);
            return View(routeLocation);
        }
        [HttpPost]
        public IActionResult RouteAddStop(RouteLocation routeLocation, string id)
        {
            string sRouteID = id;
            string connectionString = Configuration["ConnectionStrings:SilkDesigns"];
            SilkDesignUtility.CreateRouteLocation(connectionString, routeLocation, sRouteID);
            return RedirectToAction("Index");
        }
    }
}
