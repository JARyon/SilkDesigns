using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using SilkDesign.Models;
using SilkDesign.Shared;
using System.Data;
using System.Data.SqlClient;
using System.Dynamic;

namespace SilkDesign.Controllers
{
    public class CatalogController : Controller
    {
        public IConfiguration Configuration { get; }
        string msUserName = string.Empty;
        string msUserID = string.Empty;
        string msIsAdmin = string.Empty;

        public CatalogController(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IActionResult Index(string id, string SearchString, string SortOrder, string sShowInactive)
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
            bool bShowInactive =  false;

            string bABC = Request.Query["ShowInactive"].ToString();
            if (bABC == "on" || bABC == "true,false")
            {
                bShowInactive = true;
            }

            //bool bShowInactive = Request.Query["ShowInactive"].ToString() == "on" ? true : false;
            //bool bQueryShowInactive = Request.Query["ShowInactive"].ToString() == "true,false" ? true : false;
            //string showViewBag = ViewBag.ShowInactive;
            //bool bShowInactive = Request.Query["ShowInactive"].ToString() == "true,false" ? true: false;

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
            ViewBag.ShowInactive = bShowInactive == true ? "on": "";

            if (!String.IsNullOrEmpty(sSearchString) && !sSearchString.Contains('%'))
            {
                sSearchString = "%" + sSearchString + "%";
            }

            if (String.IsNullOrEmpty(id))
                id = "CODE";

            switch (id.ToUpper())
            {
                case "CODE":
                    sSortCol = "CatalogCODE";
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
                case "STATUS":
                    sSortCol = "STATUS";
                    break;
                default:
                    sSortCol = "CODE";
                    break;
            }

            string connectionString = Configuration["ConnectionStrings:SilkDesigns"];
            List<SelectListItem> SizeList = SilkDesignUtility.GetSizes(connectionString);
            List<Catalog> CatalogList = new List<Catalog>();
            List<CatalogIndexViewModel> ivmList = new List<CatalogIndexViewModel>();

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                string sql = "SELECT " +
                    " a.Code               CatalogCODE " +
                    ",a.CatalogID          CatalogID " +
                    ",a.Name               NAME " +
                    ",a.Description        DESCRIPTION " +
                    ",a.SizeID             SIZEID " +
                    ",a.Quantity           QUANTITY " +
                    ",s.Code               SIZECODE " +
                    ",c.Code               STATUS" +
                    " FROM Catalog a " +
                    " join Size s on a.SizeID = s.SizeId " +
                " join CatalogStatus c on c.CatalogStatusID = a.CatalogStatusID ";
                if (!string.IsNullOrEmpty(sSearchString))
                {
                    sql += " AND (a.Code like @SearchString ";
                    sql += " or a.Description like @SearchString ";
                    sql += " or a.Code like @SearchString) ";
                }
                sql += " Where a.deleted = 'N' ";
                if (bShowInactive)
                { // do nothing as all will be dipslayed
                }
                else // limit display to active
                {
                    sql += " AND a.CatalogStatusID in (Select x.CatalogStatusID from CatalogStatus x where x.Code <> 'Inactive') ";
                }
                sql += " Order by " + sSortCol + " " + sSortDirection;

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

                using (SqlDataReader dr = readcommand.ExecuteReader())
                {
                    while (dr.Read())
                    {

                        CatalogIndexViewModel ivm = new CatalogIndexViewModel();
                        ivm.Code = Convert.ToString(dr["CatalogCODE"]);
                        ivm.CatalogID = Convert.ToString(dr["CatalogID"]);
                        ivm.Description = Convert.ToString(dr["Description"]);
                        ivm.Name = Convert.ToString(dr["NAME"]);
                        ivm.SizeCode = Convert.ToString(dr["SIZECODE"]);
                        ivm.Sizes = SizeList;
                        ivm.SizeID = Convert.ToString(dr["SIZEID"]);
                        ivm.StatusCode = Convert.ToString(dr["STATUS"]);
                        //ivm.Quantity = Convert.ToInt32(dr["QUANTITY"]);
                        ivm.ImagePath = "/images/sm-150x150/" + ivm.Code + ".jpg";
                        ivm.ShowInactive = bShowInactive;

                        FileInfo oFile = new FileInfo("~/../wwwroot/images/sm-150x150/" + ivm.Code + ".jpg");
                        if (!oFile.Exists)
                        {
                            ivm.ImagePath = "/images/ComingSoon.jpg";
                        }

                        ivmList.Add(ivm);
                    }
                }
                connection.Close();
            }

            return View(ivmList);
        }

        public ActionResult Create()
        {
            ModelState.Clear();
            string sErrorMsg = string.Empty;
            ISession currentSession = HttpContext.Session;
            if (!ControllersShared.IsLoggedOn(currentSession, ref msUserID, ref msUserName, ref msIsAdmin))
            {
                return RedirectToAction("Login", "Login");
            }

            string connectionString = Configuration["ConnectionStrings:SilkDesigns"];
            //ViewBag.ListOfSizes2 = SilkDesignUtility.GetSizes(connectionString);
            Catalog newCatalog = new Catalog();
            newCatalog.AvailableCatalogStatus = SilkDesignUtility.GetCatalogStatus(connectionString);
            newCatalog.UserID = msUserID;

            newCatalog.AvailableSizes = SilkDesignUtility.GetSizes(connectionString);
            return View(newCatalog);

        }

        [HttpPost]
        public ActionResult Create(Catalog catalog)
        {
            string sErrorMsg = string.Empty;
            ISession currentSession = HttpContext.Session;
            if (!ControllersShared.IsLoggedOn(currentSession, ref msUserID, ref msUserName, ref msIsAdmin))
            {
                return RedirectToAction("Login", "Login");
            }

            string connectionString = Configuration["ConnectionStrings:SilkDesigns"];
            catalog.UserID = msUserID;

            var errors = ModelState
                .Where(x => x.Value.Errors.Count > 0)
                .Select(x => new { x.Key, x.Value.Errors })
                .ToArray();

            //string sArrangementInventoryID = string.Empty;

            if (ModelState.IsValid)
            {

                catalog.CatalogID = SilkDesignUtility.CreateCatalog(connectionString, catalog, ref sErrorMsg);
                if (!String.IsNullOrEmpty(sErrorMsg))
                {
                    ViewBag.Result = sErrorMsg;
                    catalog.AvailableSizes = SilkDesignUtility.GetSizes(connectionString);
                    catalog.AvailableCatalogStatus = SilkDesignUtility.GetCatalogStatus(connectionString);
                    return View(catalog);
                }
                return RedirectToAction("Index");
            }
            else
            {
                catalog.AvailableSizes = SilkDesignUtility.GetSizes(connectionString);
                catalog.AvailableCatalogStatus = SilkDesignUtility.GetCatalogStatus(connectionString);
                return View(catalog);
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

            string sCatalogID = id;
            string connectionString = Configuration["ConnectionStrings:SilkDesigns"];

            CatalogIndexViewModel CatalogInventories = new CatalogIndexViewModel();
            Catalog cat = SilkDesignUtility.GetCatalog(connectionString, sCatalogID, ref sErrorMsg);
            cat.AvailableCatalogStatus = SilkDesignUtility.GetCatalogStatus(connectionString);
            cat.AvailableCatalogStatus = SilkDesignUtility.GetCatalogStatus(connectionString);
            if (!String.IsNullOrEmpty(sErrorMsg))
            {
                ViewBag.Result = sErrorMsg;
                return View();
            }
            CatalogInventories.CatalogID = sCatalogID;
            CatalogInventories.Sizes = cat.Sizes;
            CatalogInventories.AvailableSizes = cat.AvailableSizes;
            CatalogInventories.SizeID = cat.SizeID;
            CatalogInventories.Quantity = cat.Quantity;
            CatalogInventories.SelectedSizeId = cat.SizeID;
            CatalogInventories.AvailableSizes = SilkDesignUtility.GetSizes(connectionString);
            CatalogInventories.Code = cat.Code;
            CatalogInventories.Name = cat.Name;
            CatalogInventories.ImagePath = "/images/sm-150x150/" + cat.Code + ".jpg";
            CatalogInventories.Description = cat.Description;
            CatalogInventories.SelectedCatalogStatusID = cat.CatalogStatusID;
            CatalogInventories.AvailableCatalogStatus = SilkDesignUtility.GetCatalogStatus(connectionString);

            return View(CatalogInventories);
        }

        [HttpPost]
        public IActionResult Update(CatalogIndexViewModel catalog, string id)
        {
            string sCatalogID = id;
            string connectionString = Configuration["ConnectionStrings:SilkDesigns"];

            var errors = ModelState
            .Where(x => x.Value.Errors.Count > 0)
            .Select(x => new { x.Key, x.Value.Errors })
            .ToArray();

            if (ModelState.IsValid)
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    string sql = $"Update Catalog SET " +
                                 $" Name= @Name, " +
                                 $" Description= @Description, " +
                                 $" SizeID = @SizeID, " +
                                 $" CatalogStatusID = @CatalogStatusID " +
                                 $" Where CatalogID='{sCatalogID}'";

                    using (SqlCommand command = new SqlCommand(sql, connection))
                    {
                        command.Parameters.Clear();
                        SqlParameter NameParameter = new SqlParameter
                        {
                            ParameterName = "@Name",
                            Value = catalog.Name,
                            SqlDbType = SqlDbType.VarChar
                        };

                        SqlParameter DescParameter = new SqlParameter
                        {
                            ParameterName = "@Description",
                            Value = catalog.Description,
                            SqlDbType = SqlDbType.VarChar
                        };

                        SqlParameter SizeParameter = new SqlParameter
                        {
                            ParameterName = "@SizeID",
                            Value = catalog.SelectedSizeId,
                            SqlDbType = SqlDbType.VarChar
                        };
                        
                        SqlParameter CatalogParameter = new SqlParameter
                        {
                            ParameterName = "@CatalogStatusID",
                            Value = catalog.SelectedCatalogStatusID,
                            SqlDbType = SqlDbType.VarChar
                        };

                        SqlParameter[] paramaters = new SqlParameter[] { NameParameter, DescParameter, SizeParameter, CatalogParameter };
                        command.Parameters.AddRange(paramaters);
                        connection.Open();
                        command.ExecuteNonQuery();

                    }
                    connection.Close();
                }

                return RedirectToAction("Index");
            }
            else
            {
                catalog.AvailableSizes = SilkDesignUtility.GetSizes(connectionString);
                return View(catalog);
            }
        }

    }

}
