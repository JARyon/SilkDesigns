﻿using Microsoft.AspNetCore.Mvc;
using SilkDesign.Models;
using System.Data.SqlClient;
using System.Data;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;
using System.Reflection.Metadata;
using Microsoft.IdentityModel.Tokens;
using SilkDesign.Shared;
using System.Dynamic;

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
                    " ,CASE " +
                    "   WHEN c.Name is null THEN t.Code " +
                    "   ELSE t.Code + '|' + c.Name" +
                    "  END  CODE " +
                    " FROM Location r " +
                    " join LocationType t on r.LocationTypeID = t.LocationTypeID " +
                    " left outer join CustomerLocation cl on r.LocationID = cl.LocationID " +
                    " left outer join Customer c on cl.CustomerID = c.CustomerID " +
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
            string connectionString = Configuration["ConnectionStrings:SilkDesigns"];
            string sLocationTypeID = SilkDesignUtility.GetCustomerLocationTypeID(connectionString);
            string sCustomerName = SilkDesignUtility.GetCustomerNameById(connectionString, id);

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

        //CREATE LOCATION ARRANGEMENT
        public ActionResult CreateLocationPlacement(string id)
        {
            string connectionString = Configuration["ConnectionStrings:SilkDesigns"];
            string sLocationName = SilkDesignUtility.GetLocationNameById(connectionString, id);

            ViewBag.ListOfSizes2 = SilkDesignUtility.GetSizes(connectionString);
            ViewBag.LocationName = sLocationName;
            ViewBag.LocationID = id;
            return View();
        }

        [HttpPost]
        public IActionResult CreateLocationPlacement(LocationPlacement newArrangement)
        {
            string connectionString = Configuration["ConnectionStrings:SilkDesigns"];
            ViewBag.ListOfSizes2 = SilkDesignUtility.GetSizes(connectionString);

            string sCustomerID = ViewBag.CustomerID;
            string strDDLValue = Request.Form["ddlSize"].ToString();
            if (strDDLValue == "0")
            {
                ViewBag.Result = "Must Select Size";
                return View();
            }


            if (String.IsNullOrEmpty(newArrangement.Description))
            {
                ViewBag.Result = "Descripton is required.";
                return View();
            }


            string sLocationAgreementID = SilkDesignUtility.CreateLocationPlacement(connectionString, strDDLValue, newArrangement.Description, newArrangement.LocationID);
            //SilkDesignUtility.CreateCustomerLocationAssoc(connectionString, )
            ViewBag.ListOfSizes2 = SilkDesignUtility.GetSizes(connectionString);
            ViewBag.Result = "Success";

            return View();
        }


        //UPDATE LOCATION ARRANGEMENT
        public ActionResult UpdateLocationPlacement(string id)
        {
            string connectionString = Configuration["ConnectionStrings:SilkDesigns"];
            LocationPlacement arrangement = SilkDesignUtility.GetLocationPlacement(connectionString, id);

            return View(arrangement);
        }

        [HttpPost]
        public IActionResult UpdateLocationPlacement(LocationPlacement updateArrangement, string id)
        {
            string connectionString = Configuration["ConnectionStrings:SilkDesigns"];
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string sql = $"Update LocationPlacement SET Description= @Description, " +
                    $" SizeID = @SizeID " +
                    $" Where LocationPlacementID ='{id}'";

                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    command.Parameters.Clear();
                    SqlParameter DescParameter = new SqlParameter
                    {
                        ParameterName = "@SizeID",
                        Value = updateArrangement.SizeID,
                        SqlDbType = SqlDbType.VarChar
                    };
                    SqlParameter SizeParameter = new SqlParameter
                    {
                        ParameterName = "@Description",
                        Value = updateArrangement.Description,
                        SqlDbType = SqlDbType.VarChar
                    };
                    //command.Parameters.Add(parameter);

                    SqlParameter[] paramaters = new SqlParameter[] {  DescParameter, SizeParameter };
                    command.Parameters.AddRange(paramaters);
                    connection.Open();
                    command.ExecuteNonQuery();

                }
                connection.Close();
            }

            return RedirectToAction("Update", new RouteValueDictionary(new { controller = "Location", action = "Update", Id = updateArrangement.LocationID }));
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
            #region oldCode

            //Location Location = new Location();
            //using (SqlConnection connection = new SqlConnection(connectionString))
            //{
            //    string sql = $"Select * From Location Where LocationId='{id}'";
            //    SqlCommand command = new SqlCommand(sql, connection);

            //    connection.Open();

            //    using (SqlDataReader dataReader = command.ExecuteReader())
            //    {
            //        while (dataReader.Read())
            //        {
            //            Location.LocationID= Convert.ToString(dataReader["LocationId"]);
            //            Location.Name = Convert.ToString(dataReader["Name"]);
            //            Location.Description = Convert.ToString(dataReader["Description"]);
            //        }
            //    }

            //    connection.Close();
            //}
            //return View(Location);
            #endregion
            dynamic LocationPlacements = new ExpandoObject();
            LocationPlacements.Locations = SilkDesignUtility.GetLocation(connectionString, id);
            LocationPlacements.Arrangements = SilkDesignUtility.GetLoationArrangements(connectionString, id);
            return View(LocationPlacements);
        }

        [HttpPost]
        public IActionResult Update(Location Location, string id)
        {
            string connectionString = Configuration["ConnectionStrings:SilkDesigns"];

            string sName = $"{Location.Name}";
            string sDesc = $"{Location.Description}";
            Location oLocation = new Location();
            oLocation.Description = sDesc;
            oLocation.Name = sName;
            oLocation.LocationID = id;
            SilkDesignUtility.UpdateLocation(connectionString, oLocation, id);

            return RedirectToAction("Index");
        }

        // PRIVATE Methods
        #region Private Methods

        #endregion Private Methods

    }
}
