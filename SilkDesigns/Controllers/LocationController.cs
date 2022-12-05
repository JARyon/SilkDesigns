using Microsoft.AspNetCore.Mvc;
using SilkDesign.Models;
using System.Data.SqlClient;
using System.Data;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;
using System.Reflection.Metadata;
using Microsoft.IdentityModel.Tokens;
using SilkDesign.Shared;

namespace SilkDesign.Controllers
{
    public class LocationController : Controller
    {
        const string sCustomerType = "Customer";
        
        public IConfiguration Configuration { get; }

        public LocationController(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IActionResult Index()
        {

            List<LocationIndexViewModel> ivmList = new List<LocationIndexViewModel>();
            string connectionString = Configuration["ConnectionStrings:SilkDesigns"];
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                string sql = "SELECT " +
                    " r.LocationId      ID " +
                    " ,r.Name        NAME " +
                    " ,r.Description DESCRIPTION " +
                    " ,t.Code        CODE " +
                    " FROM Location r " +
                    " join LocationType t on r.LocationTypeID = t.LocationTypeID " +
                    " Order by CODE, NAME";

                SqlCommand readcommand = new SqlCommand(sql, connection);

                using (SqlDataReader dr = readcommand.ExecuteReader())
                {
                    while (dr.Read())
                    {

                        LocationIndexViewModel ivm = new LocationIndexViewModel();
                        ivm.LocationID = Convert.ToString(dr["ID"]);
                        ivm.Name = Convert.ToString(dr["NAME"]);
                        ivm.Description = Convert.ToString(dr["DESCRIPTION"]);
                        ivm.LocationType = Convert.ToString(dr["CODE"]);
                        ivmList.Add(ivm);
                    }
                }
                connection.Close();
            }

            return View(ivmList);
        }
        /// <summary>
        /// Methods for creating Locations.
        /// </summary>
        /// <returns></returns>
        // CREATE LOCATION 
        public ActionResult Create()
        {
            ViewBag.ListOfTypes = GetTypes();
            return View();

        }

        [HttpPost]
        public IActionResult Create(Location newLocation)
        {

            if (String.IsNullOrEmpty(newLocation.Name))
            {
                ViewBag.Result = "Location Name is required.";
                return View();
            }
            string strDDLValue = Request.Form["ddlType"].ToString();
            if (strDDLValue == "0")
            {
                ViewBag.Result = "LocationType is required.";
                return View();
            }
            string connectionString = Configuration["ConnectionStrings:SilkDesigns"];
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string sql = "Insert Into Location (Name, Description, LocationTypeID) Values (@Name, @Description, @LocationTypeID)";

                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    command.CommandType = CommandType.Text;

                    // adding parameters
                    SqlParameter parameter = new SqlParameter
                    {
                        ParameterName = "@Name",
                        Value = newLocation.Name,
                        SqlDbType = SqlDbType.VarChar,
                        Size = 50
                    };
                    command.Parameters.Add(parameter);

                    parameter = new SqlParameter
                    {
                        ParameterName = "@Description",
                        Value = newLocation.Description,
                        SqlDbType = SqlDbType.VarChar
                    };
                    command.Parameters.Add(parameter);

                    parameter = new SqlParameter
                    {
                        ParameterName = "@LocationTypeID",
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
            ViewBag.ListOfTypes = GetTypes();
            return View();
        }

        // CREATE CUSTOMER LOCATION
        public ActionResult CreateCustomerLocation(string id)
        {

            string sLocationTypeID = string.Empty;
            string sCustomerName = string.Empty;

            string connectionString = Configuration["ConnectionStrings:SilkDesigns"];
            sLocationTypeID = SilkDesignUtility.GetCustomerLocationTypeID(connectionString);
            sCustomerName = SilkDesignUtility.GetCustomerNameById(connectionString, id);

            CustomerLocationCreateViewModel customerLocation = new CustomerLocationCreateViewModel();
            //customerLocation.CustomerID = id;
            //customerLocation.LocationTypeID locationTypeID= sLocationTypeID;

            ViewBag.LocationTypeID = sLocationTypeID;
            ViewBag.CustomerID  = id;
            ViewBag.CustomerName = sCustomerName;
            return View();
        }

        [HttpPost]
        public IActionResult CreateCustomerLocation(Location newCustLocation)
        {
            string sCustomerID = ViewBag.CustomerID;

            if (String.IsNullOrEmpty(newCustLocation.Name))
            {
                ViewBag.Result = "Location Name is required.";
                return View();
            }

            string connectionString = Configuration["ConnectionStrings:SilkDesigns"];
            string sLocationTypeID = SilkDesignUtility.GetCustomerLocationTypeID(connectionString);
            string sLocationID = SilkDesignUtility.CreateLocation(connectionString, newCustLocation.Name, newCustLocation.Description, sLocationTypeID);
            string sCustomerLocationID = SilkDesignUtility.CreateCustomerLocation(connectionString, newCustLocation.CustomerID, sLocationID);
            //sCusetomerLocationTypeID = SilkDesignUtility.GetCustomerLocationTypeID(connectionString);

            
            //SilkDesignUtility.CreateCustomerLocationAssoc(connectionString, )
            #region oldCode
            //using (SqlConnection connection = new SqlConnection(connectionString))
            //{
            //    string sql = "Insert Into Location (Name, Description, LocationTypeID) Values (@Name, @Description, @LocationTypeID)";

            //    using (SqlCommand command = new SqlCommand(sql, connection))
            //    {
            //        command.CommandType = CommandType.Text;

            //        // adding parameters
            //        SqlParameter parameter = new SqlParameter
            //        {
            //            ParameterName = "@Name",
            //            Value = newCustLocation.Name,
            //            SqlDbType = SqlDbType.VarChar,
            //            Size = 50
            //        };
            //        command.Parameters.Add(parameter);

            //        parameter = new SqlParameter
            //        {
            //            ParameterName = "@Description",
            //            Value = newCustLocation.Description,
            //            SqlDbType = SqlDbType.VarChar
            //        };
            //        command.Parameters.Add(parameter);

            //        parameter = new SqlParameter
            //        {
            //            ParameterName = "@LocationTypeID",
            //            Value = newCustLocation.LocationTypeID,
            //            SqlDbType = SqlDbType.VarChar
            //        };
            //        command.Parameters.Add(parameter);

            //        connection.Open();
            //        command.ExecuteNonQuery();

            //        //Create customer location assocatiation

            //        connection.Close();
            //    }
            //}
            #endregion oldCode

            ViewBag.Result = "Success";
            
            return View();
        }
        private dynamic GetTypes()
        {
            List<SelectListItem> list = new List<SelectListItem>();
            try
            {
                string connectionString = Configuration["ConnectionStrings:SilkDesigns"];
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    string sql = $"Select * from LocationType where Code != '{sCustomerType}'";
                    SqlCommand cmd = new SqlCommand(sql, connection);
                    SqlDataReader reader = cmd.ExecuteReader();

                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            list.Add(new SelectListItem { Text = reader["Code"].ToString(), Value = reader["LocationTypeID"].ToString() });
                        }
                    }
                    else
                    {
                        list.Add(new SelectListItem { Text = "No sizes found", Value = "0" });
                    }
                    list.Insert(0, new SelectListItem { Text = "-- Select Type --", Value = "0" });
                    connection.Close();
                }
            }
            catch (Exception ex)
            {
                list.Add(new SelectListItem { Text = ex.Message.ToString(), Value = "0" });
            }

            return list;
        }

        public IActionResult Update(string id)
        {
            string connectionString = Configuration["ConnectionStrings:SilkDesigns"];

            Location Location = new Location();
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string sql = $"Select * From Location Where LocationId='{id}'";
                SqlCommand command = new SqlCommand(sql, connection);

                connection.Open();

                using (SqlDataReader dataReader = command.ExecuteReader())
                {
                    while (dataReader.Read())
                    {
                        Location.LocationID= Convert.ToString(dataReader["LocationId"]);
                        Location.Name = Convert.ToString(dataReader["Name"]);
                        Location.Description = Convert.ToString(dataReader["Description"]);
                    }
                }

                connection.Close();
            }
            return View(Location);
        }

        [HttpPost]
        public IActionResult Update(Location Location, string id)
        {
            string connectionString = Configuration["ConnectionStrings:SilkDesigns"];
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string sql = $"Update Location SET Name='{Location.Name}', Description='{Location.Description}' Where LocationId='{id}'";
                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    connection.Open();
                    command.ExecuteNonQuery();
                    connection.Close();
                }
            }

            return RedirectToAction("Index");
        }
    }
}
