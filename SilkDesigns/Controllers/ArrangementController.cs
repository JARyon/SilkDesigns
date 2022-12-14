using Microsoft.AspNetCore.Mvc;
using SilkDesign.Models;
using System.Data;
using System.Data.SqlClient;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;
using System.Reflection.Metadata;
using SilkDesign.Shared;
using System.Dynamic;

namespace SilkDesign.Controllers
{
    public class ArrangementController : Controller
    {
        
        public IConfiguration Configuration { get; }

        public ArrangementController(IConfiguration configuration)
        {
            Configuration = configuration;
        }


        public IActionResult Index()
        {
            string connectionString = Configuration["ConnectionStrings:SilkDesigns"];
            List<SelectListItem> SizeList = SilkDesignUtility.GetSizes(connectionString);
            List<Arrangement> ArrangementList = new List<Arrangement>();
            List<ArrangementIndexViewModel> ivmList = new List<ArrangementIndexViewModel>();
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                string sql = "SELECT " +
                    " a.Code               ArrangementCODE " +
                    ",a.ArrangementID      ArrangementID " +
                    ",a.Name       NAME " +
                    ",a.Description DESCRIPTION " +
                    ",a.Price      PRICE " +
                    ",a.Quantity   QUANTITY " +
                    ",a.lastViewed LASTVIEWED " +
                    ",a.SizeID     SIZEID " +
                    ",s.Code       SIZECODE " +
                    "FROM Arrangement a " +
                    "join Size s on a.SizeID = s.SizeId " +
                    "Order by NAME";
                SqlCommand readcommand = new SqlCommand(sql, connection);

                using (SqlDataReader dr = readcommand.ExecuteReader())
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
                        ivmList.Add(ivm);
                    }
                }
                connection.Close();
            }


            ViewBag.ListofSizes = SizeList;
            return View(ivmList);
        }
        public ActionResult Create()
        {
            string connectionString = Configuration["ConnectionStrings:SilkDesigns"];
            ViewBag.ListOfSizes2 = SilkDesignUtility.GetSizes(connectionString);
            return View();

        }

        [HttpPost]
        public IActionResult Create(Arrangement arrangement)
        {
            string connectionString = Configuration["ConnectionStrings:SilkDesigns"];
            string sArrangementInventoryID = string.Empty;
            arrangement.SizeID = Request.Form["ddlSize"].ToString();

            arrangement.ArrangementID = SilkDesignUtility.CreateArrangement(connectionString, arrangement);
            for (int i = 1; i <= arrangement.Quantity; i++)
            {
                sArrangementInventoryID = SilkDesignUtility.CreateArrangementInventory(connectionString, arrangement);
            }
            ViewBag.Result = "Success";
            ViewBag.ListOfSizes2 = SilkDesignUtility.GetSizes(connectionString);
            return View();
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
            string sArrangementID = id;
            string connectionString = Configuration["ConnectionStrings:SilkDesigns"];
            #region OldCode
            //List<SelectListItem> SizeList = SilkDesignUtility.GetSizes(connectionString);
            //Arrangement Arrangement = new Arrangement();
            //using (SqlConnection connection = new SqlConnection(connectionString))
            //{
            //    string sql = $"Select * From Arrangement Where ArrangementID='{id}'";
            //    SqlCommand command = new SqlCommand(sql, connection);

            //    connection.Open();

            //    using (SqlDataReader dataReader = command.ExecuteReader())
            //    {
            //        while (dataReader.Read())
            //        {
            //            Arrangement.ArrangementID = Convert.ToString(dataReader["ArrangementID"]);
            //            Arrangement.Code = Convert.ToString(dataReader["Code"]);
            //            Arrangement.Name = Convert.ToString(dataReader["Name"]);
            //            Arrangement.Description = Convert.ToString(dataReader["Description"]);
            //            Arrangement.Price = Convert.ToDecimal(dataReader["Price"]);
            //            Arrangement.Quantity = Convert.ToInt32(dataReader["Quantity"]);
            //            Arrangement.LastViewed = Convert.ToDateTime(dataReader["LastViewed"]);
            //            Arrangement.SizeID = Convert.ToString(dataReader["SizeID"]);
            //            Arrangement.Sizes = SizeList;
            //        }
            //    }

            //    connection.Close();
            //}
            #endregion OldCode
            dynamic ArrangementInventories = new ExpandoObject();
            ArrangementInventories.Arrangements = SilkDesignUtility.GetArrangements(connectionString, sArrangementID);
            ArrangementInventories.ArrangementInventories = SilkDesignUtility.GetArrangementInventories(connectionString, sArrangementID);

            return View(ArrangementInventories);
        }

        [HttpPost]
        public IActionResult Update(Arrangement Arrangement, string id)
        {
            string connectionString = Configuration["ConnectionStrings:SilkDesigns"];
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string sql = $"Update Arrangement SET " +
                                    $" Name= @Name, " +
                                    $" Description= @Description, " +
                                    $" Price= @Price,  " +
                                    $" SizeID = @SizeID " +
                                    $" Where ArrangementID='{id}'";

                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    command.Parameters.Clear();
                    SqlParameter NameParameter = new SqlParameter
                    {
                        ParameterName = "@Name",
                        Value = Arrangement.Name,
                        SqlDbType = SqlDbType.VarChar,
                        Size = 50
                    };

                    SqlParameter DescParameter = new SqlParameter
                    {
                        ParameterName = "@Description",
                        Value = Arrangement.Description,
                        SqlDbType = SqlDbType.VarChar,
                        Size = 250

                    };

                    SqlParameter QtyParameter = new SqlParameter
                    {
                        ParameterName = "@Quantity",
                        Value = Arrangement.Quantity,
                        SqlDbType = SqlDbType.Int

                    };
                    SqlParameter SizeParameter = new SqlParameter
                    {
                        ParameterName = "@SizeID",
                        Value = Arrangement.SizeID,
                        SqlDbType = SqlDbType.VarChar
                    };

                    SqlParameter PriceParameter = new SqlParameter
                    {
                        ParameterName = "@Price",
                        Value = Arrangement.Price,
                        SqlDbType = SqlDbType.Decimal

                    };
                    //command.Parameters.Add(parameter);

                    SqlParameter[] paramaters = new SqlParameter[] { NameParameter, DescParameter, PriceParameter, QtyParameter, SizeParameter };
                    command.Parameters.AddRange(paramaters);
                    connection.Open();
                    command.ExecuteNonQuery();

                }
                connection.Close();
            }

            return RedirectToAction("Index");
        }

        public ActionResult UpdateArrangementInventory(string id)
        {
            string connectionString = Configuration["ConnectionStrings:SilkDesigns"];
            ArrangementInventory arrangementInventory = SilkDesignUtility.GetArrangementInventory(connectionString, id);

            return View(arrangementInventory);
        }

        [HttpPost]
        public IActionResult UpdateArrangementInventory(ArrangementInventory arrangementInventory, string id)
        {
            string connectionString = Configuration["ConnectionStrings:SilkDesigns"];
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string sql = $"Update ArrangementInventory SET Code='{arrangementInventory.Code}', LocationID='{arrangementInventory.LocationID}' Where ArrangementInventoryID='{id}'";
                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    connection.Open();
                    command.ExecuteNonQuery();
                    connection.Close();
                }
            }
            return RedirectToAction("Index");

        }

        public IActionResult CreateArrangementInventory(string id)
        {
            string connectionString = Configuration["ConnectionStrings:SilkDesigns"];
            ArrangementInventory inventory = new ArrangementInventory();
            Arrangement arrangement = SilkDesignUtility.GetArrangement(connectionString, id);

            //Get next Code from arrangement
            inventory.Code = SilkDesignUtility.GetNextInventoryCode(connectionString, arrangement.Code);
            ViewBag.ArrangementID = inventory.ArrangementID = id;
            ViewBag.Locations = inventory.Locations = SilkDesignUtility.GetLocationDDL(connectionString);
            ViewBag.NewLocation = SilkDesignUtility.GetLocations(connectionString);

            //ViewBag.Placements = SilkDesign
            return View(inventory);
        }

        public JsonResult GetLocationPlacementsByLocation(string id)
        {
            List<LocationPlacement> list = new List<LocationPlacement>();
            string connectionString = Configuration["ConnectionStrings:SilkDesigns"];


            // get list of placements by loctiont code goes here
            list = SilkDesignUtility.GetLocationPlacementList(connectionString, id);

            //list.Insert(0, new LocationPlacement { LocationPlacementID = 0, LocationName = "--- Please Selct Placment ---" });
            SelectList returned = new SelectList(list, "LocationPlacementID", "Code");
            return Json(returned);


        }

        [HttpPost]
        public IActionResult CreateArrangementInventory(ArrangementInventory newInventory)
        {
            string connectionString = Configuration["ConnectionStrings:SilkDesigns"];
            string sArrangementID = newInventory.ArrangementID;
           

            string strDDLValue = Request.Form["ddlLocation"].ToString();
            if (strDDLValue == "0")
            {
                ViewBag.Result = "Must Select Size";
                return View();
            }
            string sArrangementInventoryID = SilkDesignUtility.CreateArrangementInventory(connectionString, newInventory);

            ViewBag.Locations = SilkDesignUtility.GetLocationDDL(connectionString);
            if (sArrangementInventoryID.Length > 0)
            {
                ViewBag.Result = "Success";
                return RedirectToAction("Index");
            }
            else
            {
                ViewBag.Result = "Inventory Not Created";
                return View();
            }
        }
    }
}
