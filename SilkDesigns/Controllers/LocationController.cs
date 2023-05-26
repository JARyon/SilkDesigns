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
using System.Dynamic;
using System.Data.Common;

namespace SilkDesign.Controllers
{
    public class LocationController : Controller
    {
        const string sCustomerType = "Customer";
        string msUserName = string.Empty;
        string msUserID = string.Empty;
        string msIsAdmin = string.Empty;

        public IConfiguration Configuration { get; }

        public LocationController(IConfiguration configuration)
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

            List<LocationIndexViewModel> ivmList = new List<LocationIndexViewModel>();
            string connectionString = Configuration["ConnectionStrings:SilkDesigns"];
            try
            {
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
                        " join CustomerLocation cl on r.LocationID = cl.LocationID and cl.Deleted = 'N' " +
                        " left outer join Customer c on cl.CustomerID = c.CustomerID and c.Deleted = 'N' " +
                        " WHERE r.Deleted = 'N' " +
                        " and r.UserID = @UserID" +
                        " Order by CODE, NAME";

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
            }
            catch (Exception ex)
            {
                ViewBag.Result = "Error reading Locations. " + ex.Message;
                return View();
            }
            return View(ivmList);
        }
        public IActionResult InventoryList()
        {
            ISession currentSession = HttpContext.Session;
            if (!ControllersShared.IsLoggedOn(currentSession, ref msUserID, ref msUserName, ref msIsAdmin))
            {
                return RedirectToAction("Login", "Login");
            }

            string connectionString = Configuration["ConnectionStrings:SilkDesigns"];
            List<LocationArrangementList> LocationInventoryList = new List<LocationArrangementList>();

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                string sql = $"SELECT " +
                    $" a.Arrangement        Arrangement " +
                    $",a.Code               Code " +
                    $",a.Size               Size " +
                    $",a.InventoryCode      InventoryCode " +
                    $",a.LocationName       LocationName " +
                    $",a.Placement          Placement " +
                    $"FROM SilkDesign_locationArrangements_VW a " +
                    $"WHERE a.UserID = @UserID " +
                    $"order by LocationName, InventoryCode ";
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

                        LocationArrangementList inventoryItem = new LocationArrangementList();
                        inventoryItem.Code = Convert.ToString(dr["Code"]);
                        inventoryItem.Size = Convert.ToString(dr["Size"]);
                        inventoryItem.Arrangement = Convert.ToString(dr["Arrangement"]);
                        inventoryItem.InventoryCode = Convert.ToString(dr["InventoryCode"]);
                        inventoryItem.LocationName = Convert.ToString(dr["LocationName"]);
                        inventoryItem.Placement = Convert.ToString(dr["Placement"]);
                        LocationInventoryList.Add(inventoryItem);
                    }
                }
                connection.Close();
            }
            return View(LocationInventoryList);

        }

        public IActionResult LocationInventoryHistoryList(string id)
        {
            ISession currentSession = HttpContext.Session;
            if (!ControllersShared.IsLoggedOn(currentSession, ref msUserID, ref msUserName, ref msIsAdmin))
            {
                return RedirectToAction("Login", "Login");
            }

            string sLocationID = id;
            string connectionString = Configuration["ConnectionStrings:SilkDesigns"];
            List<LocationInventoryHistoryList> LocationInventoryList = new List<LocationInventoryHistoryList>();

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                string sql = $" select c.Name   Customer, " +
                             $"   l.Name        Location," +
                             $"   l.LocationID  LocationID, " +
                             $"   p.Description Placement, " +
                             $"   a.Name        Arrangement, " +
                             $"   a.Code        Code, " +
                             $"   s.Code        SizeCode, " +
                             $"   h.CustInvHistoryID ID," +
                             $"   h.StartDate, " +
                             $"   IsNull(h.EndDate, '') EndDate" +
                             $" from customerInventoryHistory h " +
                             $" join customer c on c.CustomerID = h.CustomerID and c.UserID = @UserID  " +
                             $" left outer join locationPlacement p on h.LocationPlacementID = p.LocationPlacementID and p.UserID = @UserID" +
                             $" join location l on l.LocationID = h.LocationID " +
                             $" join arrangement a on a.ArrangementID = h.ArrangementID  and a.UserID = @UserID " +
                             $" join Size s on s.SizeID = a.SizeID " +
                             $" where h.locationID = @LocationID " +
                             // $" and h.UserID = @UserID " +
                             $" Order by h.StartDate desc, p.Description";
                SqlCommand readcommand = new SqlCommand(sql, connection);
                SqlParameter parameter = new SqlParameter
                {
                    ParameterName = "@LocationID",
                    Value = sLocationID,
                    SqlDbType = SqlDbType.VarChar
                };
                readcommand.Parameters.Add(parameter);
                parameter = new SqlParameter
                {
                    ParameterName = "@UserID",
                    Value = msUserID,
                    SqlDbType = SqlDbType.VarChar
                };
                readcommand.Parameters.Add(parameter);

                using (SqlDataReader dr = readcommand.ExecuteReader())
                {
                    if (dr.HasRows)
                    {
                        while (dr.Read())
                        {

                            LocationInventoryHistoryList inventoryItem = new LocationInventoryHistoryList();
                            inventoryItem.CustInvHistoryID = Convert.ToString(dr["ID"]);
                            inventoryItem.LocationID = Convert.ToString(dr["LocationID"]);
                            inventoryItem.CustomerName = Convert.ToString(dr["Customer"]);
                            inventoryItem.LocationName = Convert.ToString(dr["Location"]);
                            inventoryItem.Placement = Convert.ToString(dr["Placement"]);
                            inventoryItem.Arrangement = Convert.ToString(dr["Code"]) + "|" +Convert.ToString(dr["Arrangement"]);
                            inventoryItem.Size = Convert.ToString(dr["SizeCode"]);
                            inventoryItem.StartDate = Convert.ToDateTime(dr["StartDate"]);
                            inventoryItem.EndDate = Convert.ToDateTime(dr["EndDate"]);
                            LocationInventoryList.Add(inventoryItem);
                        }
                    }
                    else
                    {
                        LocationInventoryHistoryList inventoryItem = new LocationInventoryHistoryList();
                        inventoryItem.LocationID = sLocationID;
                        LocationInventoryList.Add(inventoryItem);
                    }
                }
                connection.Close();
            }
            return View(LocationInventoryList);
        }
        /// <summary>
        /// Methods for creating Locations.
        /// </summary>
        /// <returns></returns>
        // CREATE LOCATION 
        public ActionResult Create()
        {
            ISession currentSession = HttpContext.Session;
            if (!ControllersShared.IsLoggedOn(currentSession, ref msUserID, ref msUserName, ref msIsAdmin))
            {
                return RedirectToAction("Login", "Login");
            }

            ViewBag.ListOfTypes = GetTypes();
            return View();

        }

        [HttpPost]
        public IActionResult Create(Location newLocation)
        {
            ISession currentSession = HttpContext.Session;
            if (!ControllersShared.IsLoggedOn(currentSession, ref msUserID, ref msUserName, ref msIsAdmin))
            {
                return RedirectToAction("Login", "Login");
            }

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
                string sql = "Insert Into Location (Name, Description, LocationTypeID, UserID) Values (@Name, @Description, @LocationTypeID, @UserID)";

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
                        ParameterName = "@UserID",
                        Value = msUserID,
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
            string sErrorMsg = string.Empty;
            ISession currentSession = HttpContext.Session;
            if (!ControllersShared.IsLoggedOn(currentSession, ref msUserID, ref msUserName, ref msIsAdmin))
            {
                return RedirectToAction("Login", "Login");
            }

            string connectionString = Configuration["ConnectionStrings:SilkDesigns"];
            string sLocationTypeID = SilkDesignUtility.GetCustomerLocationTypeID(connectionString);
            string sCustomerName = SilkDesignUtility.GetCustomerNameById(connectionString, id, msUserID, ref sErrorMsg);
            if (!String.IsNullOrEmpty(sErrorMsg))
            {
                ViewBag.Result = "Unable to create Location.";
                return View();
            }
            ViewBag.LocationTypeID = sLocationTypeID;
            ViewBag.CustomerID  = id;
            ViewBag.CustomerName = sCustomerName;
            return View();
        }

        [HttpPost]
        public IActionResult CreateCustomerLocation(Location newCustLocation)
        {
            ISession currentSession = HttpContext.Session;
            if (!ControllersShared.IsLoggedOn(currentSession, ref msUserID, ref msUserName, ref msIsAdmin))
            {
                return RedirectToAction("Login", "Login");
            }

            newCustLocation.UserID = msUserID;
            string sCustomerID = newCustLocation.CustomerID;
            string sErrorMsg = string.Empty;
            if (String.IsNullOrEmpty(newCustLocation.Name))
            {
                ViewBag.Result = "Location Name is required.";
                return View();
            }

            string connectionString = Configuration["ConnectionStrings:SilkDesigns"];
            string sLocationTypeID = SilkDesignUtility.GetCustomerLocationTypeID(connectionString);
            string sLocationID = SilkDesignUtility.CreateLocation(connectionString, newCustLocation.Name, newCustLocation.Description, sLocationTypeID, msUserID, ref sErrorMsg);
            if (!String.IsNullOrEmpty(sErrorMsg))
            {
                ViewBag.Result = sErrorMsg;
                return View();
            }
            string sCustomerLocationID = SilkDesignUtility.CreateCustomerLocation(connectionString, newCustLocation.CustomerID, sLocationID);
            if (!String.IsNullOrEmpty(sErrorMsg))
            {
                ViewBag.Result = sErrorMsg;
                return View();
            }
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

            ViewBag.Result = string.Empty;
            return RedirectToAction("Update","Customer", new {id = sCustomerID });
        }

        //CREATE LOCATION ARRANGEMENT
        public ActionResult CreateLocationPlacement(string id)
        {
            string sErrorMsg = string.Empty;
            ISession currentSession = HttpContext.Session;
            if (!ControllersShared.IsLoggedOn(currentSession, ref msUserID, ref msUserName, ref msIsAdmin))
            {
                return RedirectToAction("Login", "Login");
            }

            string connectionString = Configuration["ConnectionStrings:SilkDesigns"];
            string sLocationName = SilkDesignUtility.GetLocationNameById(connectionString, id, msUserID, ref sErrorMsg);
            if (!String.IsNullOrEmpty(sErrorMsg))
            {
                ViewBag.Result = sErrorMsg;
                return View();
            }
            ViewBag.ListOfSizes2 = SilkDesignUtility.GetSizes(connectionString);
            ViewBag.LocationName = sLocationName;
            ViewBag.LocationID = id;
            return View();
        }

        [HttpPost]
        public IActionResult CreateLocationPlacement(LocationPlacement newArrangement)
        {
            string sErrorMsg = string.Empty;
            ISession currentSession = HttpContext.Session;
            if (!ControllersShared.IsLoggedOn(currentSession, ref msUserID, ref msUserName, ref msIsAdmin))
            {
                return RedirectToAction("Login", "Login");
            }

            string connectionString = Configuration["ConnectionStrings:SilkDesigns"];
            ViewBag.ListOfSizes2 = SilkDesignUtility.GetSizes(connectionString);

            string sCustomerID = ViewBag.CustomerID;
            string strDDLValue = Request.Form["ddlSize"].ToString();
            int iQty = Convert.ToInt32(Request.Form["Quantity"].ToString());
            if (strDDLValue == "0")
            {
                ViewBag.Result = "Must Select Size";
                return View();
            }

            if (String.IsNullOrEmpty(newArrangement.Description))
            {
                ViewBag.Result = "Description is required.";
                return View();
            }

            string sLocationAgreementID = SilkDesignUtility.CreateLocationPlacement(connectionString, strDDLValue, newArrangement.Description, newArrangement.LocationID, iQty, msUserID, ref sErrorMsg);
            if (!String.IsNullOrEmpty(sErrorMsg))
            {
                ViewBag.Result = sErrorMsg;
                return View();
            }     
            //SilkDesignUtility.CreateCustomerLocationAssoc(connectionString, )
            ViewBag.ListOfSizes2 = SilkDesignUtility.GetSizes(connectionString);
            ViewBag.Result = "Success";

            return RedirectToAction("Update", new {id = newArrangement.LocationID});
        }


        //UPDATE LOCATION ARRANGEMENT
        public ActionResult UpdateLocationPlacement(string id)
        {
            string sErrorMsg = string.Empty;
            ISession currentSession = HttpContext.Session;
            if (!ControllersShared.IsLoggedOn(currentSession, ref msUserID, ref msUserName, ref msIsAdmin))
            {
                return RedirectToAction("Login", "Login");
            }

            string connectionString = Configuration["ConnectionStrings:SilkDesigns"];
            LocationPlacement placement = SilkDesignUtility.GetLocationPlacement(connectionString, id, msUserID, ref sErrorMsg );
            if (!String.IsNullOrEmpty(sErrorMsg))
            {
                ViewBag.Result = sErrorMsg;
                return View();
            }
            return View(placement);
        }

        [HttpPost]
        public IActionResult UpdateLocationPlacement(LocationPlacement updateArrangement, string id)
        {
            string sErrorMsg = String.Empty;
            string sPlacementID = id;

            ISession currentSession = HttpContext.Session;
            if (!ControllersShared.IsLoggedOn(currentSession, ref msUserID, ref msUserName, ref msIsAdmin))
            {
                return RedirectToAction("Login", "Login");
            }

            string connectionString = Configuration["ConnectionStrings:SilkDesigns"];
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    string sql = $"Update LocationPlacement SET Description= @Description, " +
                        $" SizeID = @SizeID, Quantity = @Quantity " +
                        $" Where LocationPlacementID = @PlacementID " +
                        $" and UserID = @UserID ";

                    using (SqlCommand command = new SqlCommand(sql, connection))
                    {
                        command.Parameters.Clear();
                        SqlParameter SizeParameter = new SqlParameter
                        {
                            ParameterName = "@SizeID",
                            Value = updateArrangement.SizeID,
                            SqlDbType = SqlDbType.VarChar
                        };
                        SqlParameter DescParameter = new SqlParameter
                        {
                            ParameterName = "@Description",
                            Value = updateArrangement.Description,
                            SqlDbType = SqlDbType.VarChar
                        };
                        SqlParameter QtyParameter = new SqlParameter
                        {
                            ParameterName = "@Quantity",
                            Value = updateArrangement.Quantity,
                            SqlDbType = SqlDbType.Int
                        };
                        SqlParameter PlacementParameter = new SqlParameter
                        {
                            ParameterName = "@PlacementID",
                            Value = id,
                            SqlDbType = SqlDbType.VarChar
                        };
                        SqlParameter UserIDParameter = new SqlParameter
                        {
                            ParameterName = "@UserID",
                            Value = msUserID,
                            SqlDbType = SqlDbType.VarChar
                        };

                        //command.Parameters.Add(parameter);

                        SqlParameter[] paramaters = new SqlParameter[] { DescParameter, SizeParameter, QtyParameter, PlacementParameter, UserIDParameter };
                        command.Parameters.AddRange(paramaters);
                        connection.Open();
                        command.ExecuteNonQuery();

                    }
                    connection.Close();
                }
            }
            catch (Exception ex)
            {
                sErrorMsg = "Unable to update location placement." + ex.Message;
                return View();
            }

            return RedirectToAction("Update", new RouteValueDictionary(new { controller = "Location", action = "Update", Id = updateArrangement.LocationID }));
        }

        public ActionResult AddPlacementArrangement(string id)
        {
            string sErrorMsg = string.Empty;
            ISession currentSession = HttpContext.Session;
            if (!ControllersShared.IsLoggedOn(currentSession, ref msUserID, ref msUserName, ref msIsAdmin))
            {
                return RedirectToAction("Login", "Login");
            }
            string connectionString = Configuration["ConnectionStrings:SilkDesigns"];

            string sLocationPlacementID = id;
            PlacementArrangement placementArrange = SilkDesignUtility.GetArrangementsForPlacement(connectionString, sLocationPlacementID, msUserID, ref sErrorMsg);
            if (!String.IsNullOrEmpty(sErrorMsg))
            {
                ViewBag.Result = sErrorMsg;
                return View();
            }
            bool bAddSelectLine = false;
            placementArrange.Arrangements = SilkDesignUtility.GetArrangementInventoryBySize(connectionString, placementArrange.SizeID, msUserID, bAddSelectLine, ref sErrorMsg);
            if (!String.IsNullOrEmpty(sErrorMsg))
            {
                ViewBag.Result = sErrorMsg;
                return View();
            }
            return View(placementArrange);

        }

        [HttpPost]
        public IActionResult AddPlacementArrangement(PlacementArrangement pa, string id)
        {
            string sErrorMsg = string.Empty;
            ISession currentSession = HttpContext.Session;
            if (!ControllersShared.IsLoggedOn(currentSession, ref msUserID, ref msUserName, ref msIsAdmin))
            {
                return RedirectToAction("Login", "Login");
            }
            string connectionString = Configuration["ConnectionStrings:SilkDesigns"];
            string sLocationPlacmentID = id;

            string sSelectedIDs = string.Empty;
            SilkDesignUtility.UpdateArrangementInventory(connectionString, pa, msUserID, ref sErrorMsg);
            if (!String.IsNullOrEmpty(sErrorMsg))
            {
                ViewBag.Result = sErrorMsg;
                return View();
            }
            return RedirectToAction("Update", new { id = pa.LocationID });
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
            string sErrorMsg = String.Empty;
            ISession currentSession = HttpContext.Session;
            if (!ControllersShared.IsLoggedOn(currentSession, ref msUserID, ref msUserName, ref msIsAdmin))
            {
                return RedirectToAction("Login", "Login");
            }

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
            LocationPlacements.Locations = SilkDesignUtility.GetLocation(connectionString, id, msUserID, ref sErrorMsg);
            if (!String.IsNullOrEmpty(sErrorMsg))
            {
                ViewBag.Result = "Can not get Location. " + sErrorMsg;
                return View();
            }
            LocationPlacements.Placements = SilkDesignUtility.GetLoationPlacements(connectionString, id, msUserID, ref sErrorMsg);
            if (!String.IsNullOrEmpty(sErrorMsg))
            {
                ViewBag.Result = "Can not get Placments. " + sErrorMsg;
                return View();
            }
            return View(LocationPlacements);
        }

        [HttpPost]
        public IActionResult Update(Location Location, string id)
        {
            string sErrorMsg = String.Empty;
            ISession currentSession = HttpContext.Session;
            if (!ControllersShared.IsLoggedOn(currentSession, ref msUserID, ref msUserName, ref msIsAdmin))
            {
                return RedirectToAction("Login", "Login");
            }

            string connectionString = Configuration["ConnectionStrings:SilkDesigns"];

            string sName = $"{Location.Name}";
            string sDesc = $"{Location.Description}";
            Location oLocation = new Location();
            oLocation.Description = sDesc;
            oLocation.Name = sName;
            oLocation.LocationID = id;
            SilkDesignUtility.UpdateLocation(connectionString, oLocation, msUserID, ref sErrorMsg);
            if (!String.IsNullOrEmpty(sErrorMsg))
            {
                ViewBag.Result = "Can not udpate Location. " + sErrorMsg;
                return View();
            }
            return RedirectToAction("Index");
        }

        public ActionResult CreateHistory(string id, string LocationName, string CustomerName)
        {
            string sErrorMsg = String.Empty;

            ISession currentSession = HttpContext.Session;
            if (!ControllersShared.IsLoggedOn(currentSession, ref msUserID, ref msUserName, ref msIsAdmin))
            {
                return RedirectToAction("Login", "Login");
            }

            string connectionString = Configuration["ConnectionStrings:SilkDesigns"];
            string sLocationID = id;

            CustomerInventoryHistory cih = new CustomerInventoryHistory();
            cih.LocationID = sLocationID;
            cih.LocationName = LocationName;
            cih.CustomerName = CustomerName;
            cih.StartDate = SilkDesignUtility.GetStartDate(connectionString, sLocationID, msUserID);
            if (cih.StartDate.Year == 1)
            {
                cih.StartDate = DateTime.Now;
            }

            cih.CustomerID = SilkDesignUtility.GetCustomerIDfromLocation(connectionString, sLocationID, msUserID);
            //cih.Placements = SilkDesignUtility.GetLocationPlacements(connectionString, sLocationID);
            cih.Arrangements = SilkDesignUtility.GetArrangements(connectionString, msUserID, ref sErrorMsg);
            if (!String.IsNullOrEmpty(sErrorMsg))
            {
                ViewBag.Result = sErrorMsg;
                return View();

            }
            return View(cih);

        }

        [HttpPost]
        public IActionResult CreateHistory(CustomerInventoryHistory oCustLocHistory)
        {
            string sErrorMsg = string.Empty;

            ISession currentSession = HttpContext.Session;
            if (!ControllersShared.IsLoggedOn(currentSession, ref msUserID, ref msUserName, ref msIsAdmin))
            {
                return RedirectToAction("Login", "Login");
            }

            string connectionString = Configuration["ConnectionStrings:SilkDesigns"];
            var errors = ModelState
                         .Where(x => x.Value.Errors.Count > 0)
                         .Select(x => new { x.Key, x.Value.Errors })
                         .ToArray();
            CustomerInventoryHistory cih = new CustomerInventoryHistory();
            if (ModelState.IsValid)
            {
                oCustLocHistory.EndDate = 
                    new DateTime(oCustLocHistory.StartDate.Year,
                                 oCustLocHistory.StartDate.Month,
                                 DateTime.DaysInMonth(oCustLocHistory.StartDate.Year,
                                                     oCustLocHistory.StartDate.Month));
                oCustLocHistory.UserID = msUserID;

                string sResult = SilkDesignUtility.CreateCustLocHistory(connectionString, oCustLocHistory, ref sErrorMsg);
                if (!String.IsNullOrEmpty(sErrorMsg))
                {
                    ViewBag.Result = sErrorMsg;
                    return View();
                }
                return RedirectToAction("LocationInventoryHistoryList", "Location", new { id = oCustLocHistory.LocationID });
            }
            else
            {
                cih.LocationID = oCustLocHistory.LocationID;
                cih.LocationName = oCustLocHistory.LocationName;
                cih.CustomerName = oCustLocHistory.CustomerName;
                cih.StartDate = oCustLocHistory.StartDate;

                cih.CustomerID = oCustLocHistory.CustomerID;
                cih.Arrangements = SilkDesignUtility.GetArrangements(connectionString, msUserID, ref sErrorMsg);
                if (!String.IsNullOrEmpty(sErrorMsg))
                {
                    ViewBag.Result = sErrorMsg;
                    return View();
                }
                return View(cih);
            }
            
        }

        public IActionResult UpdateLocationHistory(string id)
        {
            string sErrorMsg = string.Empty;
            ISession currentSession = HttpContext.Session;
            if (!ControllersShared.IsLoggedOn(currentSession, ref msUserID, ref msUserName, ref msIsAdmin))
            {
                return RedirectToAction("Login", "Login");
            }

            string connectionString = Configuration["ConnectionStrings:SilkDesigns"];
            string sCustLocHistoryID = id;

            CustomerInventoryHistory cih = new CustomerInventoryHistory();
            cih = SilkDesignUtility.GetLocationHistory(connectionString, msUserID, sCustLocHistoryID);
            cih.Arrangements = SilkDesignUtility.GetArrangements(connectionString, msUserID, ref sErrorMsg);
            if (!String.IsNullOrEmpty(sErrorMsg))
            {
                ViewBag.Result = sErrorMsg;
                return View();  
            }
            cih.CustomerHistoryID = sCustLocHistoryID;
            return View(cih);

        }

        [HttpPost]
        public IActionResult UpdateLocationHistory(CustomerInventoryHistory oCustLocHistory, string id)
        {
            string sErrorMsg = string.Empty;
            ISession currentSession = HttpContext.Session;
            if (!ControllersShared.IsLoggedOn(currentSession, ref msUserID, ref msUserName, ref msIsAdmin))
            {
                return RedirectToAction("Login", "Login");
            }

            string connectionString = Configuration["ConnectionStrings:SilkDesigns"];
            var errors = ModelState
                         .Where(x => x.Value.Errors.Count > 0)
                         .Select(x => new { x.Key, x.Value.Errors })
                         .ToArray();
            CustomerInventoryHistory cih = new CustomerInventoryHistory();
            if (ModelState.IsValid)
            {

                //oCustLocHistory.EndDate =
                //    new DateTime(oCustLocHistory.StartDate.Year,
                //                 oCustLocHistory.StartDate.Month,
                //                 DateTime.DaysInMonth(oCustLocHistory.StartDate.Year,
                //                                     oCustLocHistory.StartDate.Month));
                oCustLocHistory.CustomerHistoryID = id;

                string sResult = SilkDesignUtility.UpdateCustLocHistory(connectionString, msUserID, oCustLocHistory);

                return RedirectToAction("LocationInventoryHistoryList", "Location", new { id = oCustLocHistory.LocationID });
            }
            else
            {
                cih.LocationID = oCustLocHistory.LocationID;
                cih.LocationName = oCustLocHistory.LocationName;
                cih.CustomerName = oCustLocHistory.CustomerName;
                cih.StartDate = oCustLocHistory.StartDate;

                cih.CustomerID = oCustLocHistory.CustomerID;
                cih.Arrangements = SilkDesignUtility.GetArrangements(connectionString, msUserID, ref sErrorMsg);
                if (!String.IsNullOrEmpty(sErrorMsg)) 
                {
                    ViewBag.Result = sErrorMsg;
                    return View();
                }
                return View(cih);
            }

        }

        public ActionResult InactivateLocation(string id)
        {
            string sErrorMsg = string.Empty;
            ISession currentSession = HttpContext.Session;
            if (!ControllersShared.IsLoggedOn(currentSession, ref msUserID, ref msUserName, ref msIsAdmin))
            {
                return RedirectToAction("Login", "Login");
            }

            string sLocationID = id;
            string connectionString = Configuration["ConnectionStrings:SilkDesigns"];
            SilkDesignUtility.DeactivateLocation(connectionString, sLocationID, msUserID, ref sErrorMsg);
            if (!String.IsNullOrEmpty(sErrorMsg))
            {
                ViewBag.Result = "Unable to Inactivate Location. " + sErrorMsg;
                return View();
            }
            string sCustomerID = SilkDesignUtility.GetCustomerIDfromLocation(connectionString, sLocationID, msUserID);
            return RedirectToAction("Update", "Customer", new {id= sCustomerID });
        }
        public ActionResult InactivatePlacement(string id)
        {
            string sErrorMsg = string.Empty;
            ISession currentSession = HttpContext.Session;
            if (!ControllersShared.IsLoggedOn(currentSession, ref msUserID, ref msUserName, ref msIsAdmin))
            {
                return RedirectToAction("Login", "Login");
            }

            string sPlacmentId = id;
            string connectionString = Configuration["ConnectionStrings:SilkDesigns"];
            LocationPlacement locPlacement = SilkDesignUtility.GetLocationPlacement(connectionString, sPlacmentId, msUserID, ref sErrorMsg);

            string sResult = SilkDesignUtility.DeactivatePlacement(connectionString, sPlacmentId, msUserID, ref sErrorMsg);
            if (!String.IsNullOrEmpty(sErrorMsg)) 
            { 
                ViewBag.Result = sErrorMsg;
                return View();
            }
            return RedirectToAction("Update", "Location", new { id = locPlacement.LocationID });
        }

        // PRIVATE Methods
        #region Private Methods

        #endregion Private Methods

    }
}
