using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.IdentityModel.Tokens;
using SilkDesign.Models;
using SilkDesign.Shared;
using System.Data;
using System.Data.SqlClient;
using System.Dynamic;

namespace SilkDesign.Controllers
{
    public class ArrangementController : Controller
    {

        public IConfiguration Configuration { get; }
        string msUserName = string.Empty;
        string msUserID = string.Empty;
        string msIsAdmin = string.Empty;
        public ArrangementController(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IActionResult Index(string id, string SearchString, string SortOrder)
        {
            ISession currentSession = HttpContext.Session;
            if (!ControllersShared.IsLoggedOn(currentSession, ref msUserID, ref msUserName, ref msIsAdmin))
            {
                return RedirectToAction("Login", "Login");
            }
            string? sSearchString = string.Empty;
            string? sSortDirection = string.Empty;
            string? abc = Request.Query["SearchString"];
            string? sSort = Request.Query["SortOrder"];
            string sSortCol = "CODE";

            if (!String.IsNullOrEmpty(sSort))
            {
                sSortDirection = sSort;
            }
            else
            {
                sSortDirection = "Asc";
            }

            if (!String.IsNullOrEmpty(abc))
            {
                sSearchString = abc;
            }
            else
            {
                abc = string.Empty;
                if (ViewBag.SearchString != null)
                {
                    sSearchString = ViewBag.SearchString;
                }
                if (ViewBag.SearchOrder != null)
                {
                    sSortDirection = ViewBag.SearchOrder;
                }
            }

            // Set Sort Direction for next time
            if (sSortDirection == "Asc")
            {
                sSort = "Desc";
            }
            else
            {
                sSort = "Asc";
            }
            ViewBag.SearchString = abc;
            ViewBag.SearchOrder = sSort;

            if (!String.IsNullOrEmpty(sSearchString) && !sSearchString.Contains('%'))
            {
                sSearchString = "%" + sSearchString + "%";
            }

            if (String.IsNullOrEmpty(id))
                id = "CODE";

            switch (id.ToUpper())
            {
                case "CODE":
                    sSortCol = "ArrangementCODE";
                    break;
                case "NAME":
                    sSortCol = "NAME";
                    break;
                case "DESCRIPTION":
                    sSortCol = "DESCRIPTION";
                    break;
                case "QUANTITY":
                    sSortCol = "QUANTITY";
                    break;
                case "SIZE":
                    sSortCol = "SIZECODE";
                    break;
                default:
                    sSortCol = "CODE";
                    break;
            }

            string connectionString = Configuration["ConnectionStrings:SilkDesigns"];
            string sErrorMsg = string.Empty;
            List<SelectListItem> SizeList = SilkDesignUtility.GetSizes(connectionString);
            List<Arrangement> ArrangementList = new List<Arrangement>();
            List<ArrangementIndexViewModel> ivmList = new List<ArrangementIndexViewModel>();
                        
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                string sql = "SELECT " +
                    " a.Code               ArrangementCODE " +
                    ",a.ArrangementID      ArrangementID " +
                    ",a.Name               NAME " +
                    ",a.Description        DESCRIPTION " +
                    ",a.Price              PRICE " +
                    ",a.Quantity           QUANTITY " +
                    ",a.lastViewed         LASTVIEWED " +
                    ",a.SizeID             SIZEID " +
                    ",s.Code               SIZECODE " +
                    "FROM Arrangement a " +
                    "join Size s on a.SizeID = s.SizeId " +
                    " where a.UserID = @UserID " +
                    "   and a.Deleted = 'N' ";
                if (!string.IsNullOrEmpty(sSearchString))
                {
                    sql += " AND (a.Code like @SearchString ";
                    sql += " or a.Description like @SearchString ";
                    sql += " or a.Name like @SearchString) ";
                }
                sql += "Order by " + sSortCol + " " +sSortDirection;
 
                SqlCommand readcommand = new SqlCommand(sql, connection);

                SqlParameter parameter = new SqlParameter
                {
                    ParameterName = "@UserID",
                    Value = msUserID,
                    SqlDbType = SqlDbType.VarChar
                };
                readcommand.Parameters.Add(parameter);

                if (!string.IsNullOrEmpty(sSearchString))
                {
                    parameter = new SqlParameter
                    {
                        ParameterName = "@SearchString",
                        Value = sSearchString,
                        SqlDbType = SqlDbType.VarChar
                    };
                    readcommand.Parameters.Add(parameter);
                }

                //using (SqlDataReader  dr = readcommand.ExecuteReader())
                //{
                SqlDataReader dr = readcommand.ExecuteReader();
                if (dr.HasRows)
                {
                    while (dr.Read())
                    {

                        ArrangementIndexViewModel ivm = new ArrangementIndexViewModel();
                        ivm.Code = Convert.ToString(dr["ArrangementCODE"]);
                        ivm.ArrangementID = Convert.ToString(dr["ArrangementID"]);
                        ivm.Description = Convert.ToString(dr["Description"]);
                        ivm.Name = Convert.ToString(dr["NAME"]);
                        ivm.Price = Convert.ToDecimal(dr["PRICE"]);
                        ivm.Quantity = Convert.ToInt32(dr["QUANTITY"]);
                        ivm.SizeCode = Convert.ToString(dr["SIZECODE"]);
                        ivm.Sizes = SizeList;
                        ivm.SizeID = Convert.ToString(dr["SIZEID"]);
                        ivm.ImagePath = "/images/sm-150x150/" + ivm.Code + ".jpg";


                        FileInfo oFile = new FileInfo("~/../wwwroot/images/sm-150x150/" + ivm.Code + ".jpg");
                        if (!oFile.Exists)
                        {
                            ivm.ImagePath = "/images/ComingSoon.jpg";
                        }
                        ivmList.Add(ivm);
                    }
                }
                else // did not find  a matching arrangment so try arrangmentInventory
                {
                    string sInventoryCode = sSearchString.Replace("%", "");
                    string ArrangementInventoryID = SilkDesignUtility.SearchForInventoryItem(connectionString, sInventoryCode, msUserID, ref sErrorMsg);
                    if (!String.IsNullOrEmpty(sErrorMsg))
                    {
                        ViewBag.Result = sErrorMsg;
                        return View();
                    }
                    if (!String.IsNullOrEmpty(ArrangementInventoryID))
                    {
                        return RedirectToAction("UpdateArrangementInventory", new { id = ArrangementInventoryID });
                    }
                }
                //}
                connection.Close();
            }

            return View(ivmList);
        }


        public IActionResult InventoryList(string id)
        {
            string sErrorMsg = string.Empty;
            ISession currentSession = HttpContext.Session;
            if (!ControllersShared.IsLoggedOn(currentSession, ref msUserID, ref msUserName, ref msIsAdmin))
            {
                return RedirectToAction("Login", "Login");
            }
            string sSortCol = "NAME";

            if (String.IsNullOrEmpty(id))
                id = "ARRANGEMENT";

            switch (id.ToUpper())
            {
                case "ARRANGEMENT":
                    sSortCol = "Arrangement, InventoryCode";
                    break;
                case "CODE":
                    sSortCol = "Code";
                    break;
                case "INVENTORYCODE":
                    sSortCol = "InventoryCode";
                    break;
                case "LOCATIONNAME":
                    sSortCol = "LocationName";
                    break;
                case "PLACEMENT":
                    sSortCol = "Placement";
                    break;
                case "STATUS":
                    sSortCol = "Status";
                    break;
                default:
                    sSortCol = "Arrangement, InventoryCode";
                    break;
            }

            string connectionString = Configuration["ConnectionStrings:SilkDesigns"];
            List<ArrangementInventoryList> ArrangementMasterList = new List<ArrangementInventoryList>();

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                string sql = "SELECT " +
                    " a.Arrangement        Arrangement " +
                    ",a.Code               Code " +
                    ",a.InventoryCode      InventoryCode " +
                    ",a.SizeCode           SizeCode " +
                    ",a.LocationName       LocationName " +
                    ",a.Placement          Placement " +
                    ",a.StatusCode         Status " +
                    " FROM SilkDesign_InventoryList_VW a " +
                    " where UserID = @UserID " +
                    " order by " + sSortCol ;
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

                        ArrangementInventoryList inventoryItem = new ArrangementInventoryList();
                        inventoryItem.Code = Convert.ToString(dr["Code"]);
                        inventoryItem.Arrangement = Convert.ToString(dr["Arrangement"]);
                        inventoryItem.InventoryCode = Convert.ToString(dr["InventoryCode"]);
                        inventoryItem.LocationName = Convert.ToString(dr["LocationName"]);
                        inventoryItem.Placement = Convert.ToString(dr["Placement"]);
                        inventoryItem.Status = Convert.ToString(dr["Status"]);
                        inventoryItem.SizeCode = Convert.ToString(dr["SizeCode"]);
                        ArrangementMasterList.Add(inventoryItem);
                    }
                }
                connection.Close();
            }
            return View(ArrangementMasterList);

        }
        public IActionResult PlacementList()
        {
            string sErrorMsg = string.Empty;
            ISession currentSession = HttpContext.Session;
            if (!ControllersShared.IsLoggedOn(currentSession, ref msUserID, ref msUserName, ref msIsAdmin))
            {
                return RedirectToAction("Login", "Login");
            }

            string connectionString = Configuration["ConnectionStrings:SilkDesigns"];
            List<ArrangementInventoryList> ArrangementMasterList = new List<ArrangementInventoryList>();

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                string sql = "SELECT " +
                    " a.Arrangement        Arrangement " +
                    ",a.Code               Code " +
                    ",a.Size               Size " +
                    ",a.InventoryCode      InventoryCode " +
                    ",a.LocationName       LocationName " +
                    ",a.Placement          Placement " +
                    " FROM SilkDesign_locationArrangements_VW a " +
                    " WHERE a.UserID = @UserID " +
                    " order by Arrangement, InventoryCode, LocationName ";
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

                        ArrangementInventoryList inventoryItem = new ArrangementInventoryList();
                        inventoryItem.Code = Convert.ToString(dr["Code"]);
                        inventoryItem.SizeCode = Convert.ToString(dr["Size"]);
                        inventoryItem.Arrangement = Convert.ToString(dr["Arrangement"]);
                        inventoryItem.InventoryCode = Convert.ToString(dr["InventoryCode"]);
                        inventoryItem.LocationName = Convert.ToString(dr["LocationName"]);
                        inventoryItem.Placement = Convert.ToString(dr["Placement"]);
                        ArrangementMasterList.Add(inventoryItem);
                    }
                }
                connection.Close();
            }
            return View(ArrangementMasterList);

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
            //ViewBag.ListOfSizes2 = SilkDesignUtility.GetSizes(connectionString);
            Arrangement newArrangement = new Arrangement();

            newArrangement.UserID = msUserID;

            newArrangement.AvailableSizes = SilkDesignUtility.GetSizes(connectionString);
            newArrangement.AvailableCatalogItems = SilkDesignUtility.GetCatalogItems(connectionString, msUserID, ref sErrorMsg);
            newArrangement.WHLocations = SilkDesignUtility.GetWarehouses(connectionString, msUserID);
            newArrangement.Quantity = 1;
            {
                if (!String.IsNullOrEmpty(sErrorMsg))
                {
                    ViewBag.Result = sErrorMsg;
                    return View();
                }
            }
            return View(newArrangement);

        }

        [HttpPost]
        public ActionResult Create(Arrangement arrangement)
        {
            string sErrorMsg = string.Empty;
            ISession currentSession = HttpContext.Session;
            if (!ControllersShared.IsLoggedOn(currentSession, ref msUserID, ref msUserName, ref msIsAdmin))
            {
                return RedirectToAction("Login", "Login");
            }

            string connectionString = Configuration["ConnectionStrings:SilkDesigns"];
            SilkDesignUtility.FillArrangementFromCatalog(connectionString, msUserID, ref arrangement, ref sErrorMsg);
            if (!string.IsNullOrEmpty(sErrorMsg))
            {
                ViewBag.Result = sErrorMsg;
                return View();
            }
            var errors = ModelState
                .Where(x => x.Value.Errors.Count > 0)
                .Select(x => new { x.Key, x.Value.Errors })
                .ToArray();

            string sArrangementInventoryID = string.Empty;

            Arrangement newArrangement = new Arrangement();
            if (ModelState.IsValid)
            {

                //arrangement.SizeID = Request.Form["ddlSize"].ToString();
                arrangement.ArrangementID = SilkDesignUtility.CreateArrangement(connectionString, arrangement, ref sErrorMsg);
                if (!string.IsNullOrEmpty(sErrorMsg))
                {
                    newArrangement.WHLocations = SilkDesignUtility.GetWarehouses(connectionString, msUserID);
                    ViewBag.Result = sErrorMsg;
                    return View();
                }
                for (int i = 1; i <= arrangement.Quantity; i++)
                {
                    sArrangementInventoryID = SilkDesignUtility.CreateArrangementInventory(connectionString, arrangement, ref sErrorMsg);
                    if (String.IsNullOrWhiteSpace(sArrangementInventoryID))
                    {
                        ViewBag.Result = "Failure";
                        ViewBag.ListOfSizes2 = SilkDesignUtility.GetSizes(connectionString);

                        newArrangement.AvailableSizes = SilkDesignUtility.GetSizes(connectionString);
                        newArrangement.WHLocations = SilkDesignUtility.GetWarehouses(connectionString, msUserID);
                        return View(arrangement);
                    }
                }
                return RedirectToAction("Index");
            }
            else
            {
                newArrangement.AvailableSizes = SilkDesignUtility.GetSizes(connectionString);
                newArrangement.AvailableCatalogItems = SilkDesignUtility.GetCatalogItems(connectionString, msUserID, ref sErrorMsg);
                newArrangement.WHLocations = SilkDesignUtility.GetWarehouses(connectionString, msUserID);
                return View(newArrangement);
            }

        }

        public Size[] GetArraySize()
        {
            List<Size> list = new List<Size>();
            try
            {
                string connectionString = Configuration["ConnectionStrings:SilkDesigns"];
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    string sql = "Select * from Size";
                    SqlCommand cmd = new SqlCommand(sql, connection);
                    SqlDataReader reader = cmd.ExecuteReader();

                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            Size sizeOption = new Size(reader["Code"].ToString(), Convert.ToString(reader["SizeID"].ToString()));
                            list.Add(sizeOption);
                        }
                    }
                    connection.Close();
                }
            }
            catch (Exception ex)
            {
                throw;
            }
            return list.ToArray();
        }

        public IActionResult Update(string id)
        {
            string sErrorMsg = string.Empty;
            ISession currentSession = HttpContext.Session;
            if (!ControllersShared.IsLoggedOn(currentSession, ref msUserID, ref msUserName, ref msIsAdmin))
            {
                return RedirectToAction("Login", "Login");
            }

            string sArrangementID = id;
            string connectionString = Configuration["ConnectionStrings:SilkDesigns"];

            ArrangementIndexViewModel ArrangementInventories = new ArrangementIndexViewModel();
            Arrangement arr = SilkDesignUtility.GetArrangement(connectionString, sArrangementID, msUserID, ref sErrorMsg);
            if (!String.IsNullOrEmpty(sErrorMsg))
            {
                ViewBag.Result = sErrorMsg;
                return View();
            }
            ArrangementInventories.ArrangementID = sArrangementID;
            ArrangementInventories.Quantity = arr.Quantity;
            ArrangementInventories.Sizes = arr.Sizes;   
            ArrangementInventories.Price = arr.Price;   
            ArrangementInventories.AvailableSizes = arr.AvailableSizes;
            ArrangementInventories.SizeID = arr.SizeID;
            ArrangementInventories.SelectedSizeId = arr.SizeID;
            ArrangementInventories.AvailableSizes = SilkDesignUtility.GetSizes(connectionString);
            ArrangementInventories.Code = arr.Code;
            ArrangementInventories.Name = arr.Name;
            ArrangementInventories.ImagePath = "/images/sm-150x150/" + arr.Code + ".jpg";

            FileInfo oFile = new FileInfo("~/../wwwroot/images/sm-150x150/" + arr.Code + ".jpg");
            if (!oFile.Exists)
            {
                ArrangementInventories.ImagePath = "/images/ComingSoon.jpg";
            }
            
            ArrangementInventories.Description = arr.Description;
            ArrangementInventories.SizeCode = arr.SizeCode;

            ArrangementInventories.Inventory = SilkDesignUtility.GetArrangementInventories(connectionString, sArrangementID, msUserID, ref sErrorMsg);

            return View(ArrangementInventories);
        }

        [HttpPost]
        public IActionResult Update(ArrangementIndexViewModel ArrangementInventory, string id)
        {
            string sErrorMsg = string.Empty;
            ISession currentSession = HttpContext.Session;
            if (!ControllersShared.IsLoggedOn(currentSession, ref msUserID, ref msUserName, ref msIsAdmin))
            {
                return RedirectToAction("Login", "Login");
            }
            string sArrangementID = id;
            string connectionString = Configuration["ConnectionStrings:SilkDesigns"];
            var errors = ModelState
            .Where(x => x.Value.Errors.Count > 0)
            .Select(x => new { x.Key, x.Value.Errors })
            .ToArray();
            if (ModelState.IsValid)
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    string sql = $"Update Arrangement SET " +
                                 $" Name= @Name, " +
                                 $" Description= @Description " +
                                 $" Where ArrangementID= @ArrangmentID ";

                    using (SqlCommand command = new SqlCommand(sql, connection))
                    {
                        command.Parameters.Clear();
                        SqlParameter NameParameter = new SqlParameter
                        {
                            ParameterName = "@Name",
                            Value = ArrangementInventory.Name,
                            SqlDbType = SqlDbType.VarChar,
                            Size = 50
                        };

                        SqlParameter DescParameter = new SqlParameter
                        {
                            ParameterName = "@Description",
                            Value = ArrangementInventory.Description,
                            SqlDbType = SqlDbType.VarChar,
                            Size = 250

                        };

                        SqlParameter SizeParameter = new SqlParameter
                        {
                            ParameterName = "@ArrangmentID",
                            Value = id,
                            SqlDbType = SqlDbType.VarChar
                        };

                        SqlParameter[] paramaters = new SqlParameter[] { NameParameter, DescParameter, SizeParameter };
                        command.Parameters.AddRange(paramaters);
                        connection.Open();
                        command.ExecuteNonQuery();

                    }
                    connection.Close();
                }
                // Create ThumbNail
                //string sPath = "~/../wwwroot/images/sm-150x150/" + arr.Code + ".jpg";
                //FileInfo oFullSizeImage = new FileInfo(sPath);

                //FileInfo oThumbImage = new FileInfo("~/../wwwroot/images/thumbnails/" + arr.Code + ".jpg");
                //if (oFullSizeImage.Exists && !oThumbImage.Exists)
                //{
                //    SilkDesignUtility.GenerateThumbnail(oFullSizeImage.FullName, 75, 75, oThumbImage.FullName);
                //}
                return RedirectToAction("Index");
            }
            else
            {
                ArrangementIndexViewModel ArrangementInventories = new ArrangementIndexViewModel();
                ArrangementInventories.AvailableSizes = SilkDesignUtility.GetSizes(connectionString);
                ArrangementInventories.Inventory = SilkDesignUtility.GetArrangementInventories(connectionString, sArrangementID, msUserID, ref sErrorMsg);
                return View(ArrangementInventories);
            }
        }

        public ActionResult UpdateArrangementInventory(string id)
        {
            string sErrorMsg = string.Empty;
            ISession currentSession = HttpContext.Session;
            if (!ControllersShared.IsLoggedOn(currentSession, ref msUserID, ref msUserName, ref msIsAdmin))
            {
                return RedirectToAction("Login", "Login");
            }
            string sArrangementInventoryID = id;
            string connectionString = Configuration["ConnectionStrings:SilkDesigns"];
            ArrangementInventory arrangementInventory = SilkDesignUtility.GetArrangementInventory(connectionString, sArrangementInventoryID, msUserID, ref sErrorMsg);
            if (!String.IsNullOrEmpty(sErrorMsg))
            {
                ViewBag.Result = sErrorMsg;
                return View();
            }
            string sWarehouseTypeID = SilkDesignUtility.GetLocationTypeID(connectionString, "Warehouse", ref sErrorMsg);
            ViewBag.Locations = SilkDesignUtility.GetLocations(connectionString, msUserID, ref sErrorMsg, sWarehouseTypeID);
            if (!String.IsNullOrEmpty(sErrorMsg))
            {
                ViewBag.Result = sErrorMsg;
                return View();
            }

            ViewBag.Placements = SilkDesignUtility.GetLocationPlacementListWithSize(connectionString, arrangementInventory.LocationID, arrangementInventory.SizeID, msUserID, ref sErrorMsg);
            if (!String.IsNullOrEmpty(sErrorMsg))
            {
                ViewBag.Result = sErrorMsg;
                return View();
            }
            return View(arrangementInventory);
        }

        [HttpPost]
        public IActionResult UpdateArrangementInventory(ArrangementInventory arrangementInventory, string id)
        {
            string sErrorMsg = string.Empty;
            ISession currentSession = HttpContext.Session;
            if (!ControllersShared.IsLoggedOn(currentSession, ref msUserID, ref msUserName, ref msIsAdmin))
            {
                return RedirectToAction("Login", "Login");
            }

            string connectionString = Configuration["ConnectionStrings:SilkDesigns"];
            string sSelectedLocationID = Request.Form["ddlLocations"].ToString();
            string sSelectedPlacementID = Request.Form["ddlPlacements"].ToString();

            string sSelectedLocationType = SilkDesignUtility.GetLocationType(connectionString, sSelectedLocationID);
            string sRetValue = string.Empty;
            string sInventoryStatusClause = string.Empty;

            if (sSelectedLocationType == "Customer")
            {
                sInventoryStatusClause = " InventoryStatusID = (Select InventoryStatusID from InventoryStatus where Code = 'InUse')";
            }
            else if (sSelectedLocationType == "Warehouse")
            {
                sInventoryStatusClause = " InventoryStatusID = (Select InventoryStatusID from InventoryStatus where Code = 'Available')";
            }
            else
            {
                sInventoryStatusClause = " InventoryStatusID = (Select InventoryStatusID from InventoryStatus where Code = 'Allocated') ";
            }

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string sql = $" Update ArrangementInventory SET " + sInventoryStatusClause;
                
                if (sSelectedLocationID.Length > 1)
                {
                    sql += $", LocationID='{sSelectedLocationID}' ";
                }
                else
                {
                    sql += $", LocationID= null ";
                }


                if (sSelectedPlacementID.Length > 1)
                {
                    sql += $", LocationPlacementID='{sSelectedPlacementID}' ";
                }
                else
                {
                    sql += $", LocationPlacementID= null ";
                }

                sql += $" Where ArrangementInventoryID='{id}' and UserID='{msUserID}'";

                try
                {
                    using (SqlCommand command = new SqlCommand(sql, connection))
                    {
                        connection.Open();
                        command.ExecuteNonQuery();
                        connection.Close();
                    }
                }
                catch (Exception ex)
                {
                    sErrorMsg = "Error in updated Arrangement Inventory. " + ex.Message;
                    return View();
                }
            }
            string sReturn = SilkDesignUtility.SetInventoryQuantity(connectionString, id, msUserID, ref sErrorMsg);
            if (!String.IsNullOrEmpty(sErrorMsg))
            {
                ViewBag.Result = sErrorMsg;
                return View();
            }
            return RedirectToAction("Update", new { id = arrangementInventory.ArrangementID });

        }

        public IActionResult CreateArrangementInventory(string id)
        {
            string sErrorMsg = string.Empty;
            ISession currentSession = HttpContext.Session;
            if (!ControllersShared.IsLoggedOn(currentSession, ref msUserID, ref msUserName, ref msIsAdmin))
            {
                return RedirectToAction("Login", "Login");
            }

            string connectionString = Configuration["ConnectionStrings:SilkDesigns"];
            ArrangementInventory inventory = new ArrangementInventory();
            Arrangement arrangement = SilkDesignUtility.GetArrangement(connectionString, id, msUserID, ref sErrorMsg);
            if (!String.IsNullOrEmpty(sErrorMsg))
            {
                ViewBag.Result = sErrorMsg;
                return View();
            }
            //Get next Code from arrangement
            inventory.Code = SilkDesignUtility.GetNextInventoryCode(connectionString, arrangement.Code, arrangement.UserID);
            ViewBag.ArrangementID = inventory.ArrangementID = id;
            //ViewBag.Locations = SilkDesignUtility.GetLocations(connectionString);

            ViewBag.Locations = SilkDesignUtility.GetLocationsWithSize(connectionString, arrangement.SizeID, true, msUserID, ref sErrorMsg);
            if (!String.IsNullOrEmpty(sErrorMsg))
            {
                ViewBag.Result = "Error in Creating Arrangement Inventory." + sErrorMsg;
                return View();
            }

            ViewBag.Placements = SilkDesignUtility.GetLocationPlacementList(connectionString, string.Empty, msUserID, ref sErrorMsg);
            if (!String.IsNullOrEmpty(sErrorMsg))
            {
                ViewBag.Result = "Error in Creating Arrangement Inventory." + sErrorMsg;
                return View();
            }
            return View(inventory);
        }

        [HttpPost]
        public IActionResult CreateArrangementInventory(ArrangementInventory newInventory)
        {
            string sErrorMsg = String.Empty;
            ISession currentSession = HttpContext.Session;
            if (!ControllersShared.IsLoggedOn(currentSession, ref msUserID, ref msUserName, ref msIsAdmin))
            {
                return RedirectToAction("Login", "Login");
            }

            string connectionString = Configuration["ConnectionStrings:SilkDesigns"];
            string sArrangementID = newInventory.ArrangementID;
           

            string sLocationID = Request.Form["ddlLocations"].ToString();
            string sSelectedPlacmentID = Request.Form["ddlPlacements"].ToString();
            newInventory.LocationID = sLocationID;
            newInventory.LocationPlacementID = sSelectedPlacmentID;
            newInventory.UserID = msUserID;

            if (sLocationID == "0")
            {
                ViewBag.Result = "Must Select Size";
                return View();
            }
            string sArrangementInventoryID = SilkDesignUtility.CreateArrangementInventory(connectionString, newInventory, ref sErrorMsg);
            if (!String.IsNullOrEmpty(sErrorMsg))
            {
                ViewBag.Result = sErrorMsg;
                return View();
            }
            string sResult = SilkDesignUtility.SetInventoryQuantity(connectionString, sArrangementInventoryID, msUserID, ref sErrorMsg);
            if (!String.IsNullOrEmpty(sErrorMsg))
            {
                ViewBag.Result = sErrorMsg;
                return View();
            }

            ViewBag.Locations = SilkDesignUtility.GetLocationDDL(connectionString, msUserID, ref sErrorMsg);
            if (!String.IsNullOrEmpty(sErrorMsg))
            {
                ViewBag.Result = sErrorMsg;
                return View();
            }

            ViewBag.Placements = SilkDesignUtility.GetLocationPlacement(connectionString, sArrangementID, msUserID, ref sErrorMsg);
            if (sArrangementInventoryID.Length > 0)
            {
                ViewBag.Result = "";
                return RedirectToAction("Index");
            }
            else
            {
                ViewBag.Result = "Inventory Not Created";
                return View();
            }
        }
        public JsonResult GetLocationPlacementsByLocation(string id , string SizeID)
        {
            string sErrorMsg = String.Empty;
            ISession currentSession = HttpContext.Session;
            if (!ControllersShared.IsLoggedOn(currentSession, ref msUserID, ref msUserName, ref msIsAdmin))
            {
                return Json("");
            }
            List<LocationPlacement> list = new List<LocationPlacement>();
            string connectionString = Configuration["ConnectionStrings:SilkDesigns"];


            // get list of placements by loctiont code goes here
            list = SilkDesignUtility.GetLocationPlacementList(connectionString, id, msUserID, ref sErrorMsg);
            if (!String.IsNullOrEmpty(sErrorMsg))
            {
                return Json("");
            }
            //list.Insert(0, new LocationPlacement { LocationPlacementID = 0, LocationName = "--- Please Selct Placment ---" });
            SelectList returned = new SelectList(list, "LocationPlacementID", "Code");
            return Json(returned);
        }

        public JsonResult GetLocationPlacementsByLocationBySize(string id, string SizeID)
        {
            string sErrorMsg = String.Empty;
            ISession currentSession = HttpContext.Session;
            if (!ControllersShared.IsLoggedOn(currentSession, ref msUserID, ref msUserName, ref msIsAdmin))
            {
                return Json("");
            }

            List<LocationPlacement> list = new List<LocationPlacement>();
            string connectionString = Configuration["ConnectionStrings:SilkDesigns"];
            string sArrangeSizeID = string.Empty;
            sArrangeSizeID = SizeID;
            if (String.IsNullOrEmpty(SizeID))
            {
                ArrangementInventory arrangement = SilkDesignUtility.GetArrangementInventory(connectionString, id, msUserID, ref sErrorMsg);
                if (!String.IsNullOrEmpty(sErrorMsg))
                {
                    return Json("");
                }
                sArrangeSizeID = arrangement.SizeID;
            }
            // get list of placements by loctiont code goes here
            list = SilkDesignUtility.GetLocationPlacementListWithSize(connectionString, id, sArrangeSizeID, msUserID, ref sErrorMsg);
            if (!String.IsNullOrEmpty(sErrorMsg))
            {
                return Json("");
            }
            //list.Insert(0, new LocationPlacement { LocationPlacementID = 0, LocationName = "--- Please Selct Placment ---" });
            SelectList returned = new SelectList(list, "LocationPlacementID", "Code");
            return Json(returned);
        }

        public ActionResult InactivateInventory(string id)
        {
            string sErrorMsg = String.Empty;
            ISession currentSession = HttpContext.Session;
            if (!ControllersShared.IsLoggedOn(currentSession, ref msUserID, ref msUserName, ref msIsAdmin))
            {
                return RedirectToAction("Login", "Login");
            }

            string sArrangementInventoryID = id;
            string connectionString = Configuration["ConnectionStrings:SilkDesigns"];
            string sResult = SilkDesignUtility.DeactivateInventory(connectionString, sArrangementInventoryID, msUserID, ref sErrorMsg);
            if (!String.IsNullOrEmpty(sErrorMsg))
            {
                ViewBag.Result = sErrorMsg;
                return View();
            }
            sResult = SilkDesignUtility.SetInventoryQuantity(connectionString, sArrangementInventoryID, msUserID, ref sErrorMsg);
            if (!String.IsNullOrEmpty(sErrorMsg))
            {
                ViewBag.Result = sErrorMsg;
                return View();
            }
            return RedirectToAction("Index");
        }
    }
}
