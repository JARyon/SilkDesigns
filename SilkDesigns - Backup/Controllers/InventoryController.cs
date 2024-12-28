using Microsoft.AspNetCore.Mvc;
using SilkDesign.Models;
using System.Data;
using System.Data.SqlClient;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;
using System.Reflection.Metadata;
using SilkDesign.Shared;

namespace SilkDesign.Controllers
{
    public class InventoryController : Controller
    {
        public IConfiguration Configuration { get; }

        public InventoryController(IConfiguration configuration)
        {
            Configuration = configuration;
        }


        public IActionResult Index()
        {
            string connectionString = Configuration["ConnectionStrings:SilkDesigns"];
            List<SelectListItem> SizeList = SilkDesignUtility.GetSizes(connectionString);
            List<Inventory> inventoryList = new List<Inventory>();
            List<InventoryIndexViewModel> ivmList = new List<InventoryIndexViewModel>();
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                string sql = "SELECT " +
                    " i.InventoryID      InventoryID " +
                    ",i.Name       NAME " +
                    ",i.Description DESCRIPTION " +
                    ",i.Price      PRICE " +
                    ",i.Quantity   QUANTITY " +
                    ",i.lastViewed LASTVIEWED " +
                    ",i.SizeID     SIZEID " +
                    ",s.Code       CODE " +
                    "FROM Inventory i " +
                    "join Size s on i.SizeID = s.SizeId " +
                    "Order by NAME";
                SqlCommand readcommand = new SqlCommand(sql, connection);

                using (SqlDataReader dr = readcommand.ExecuteReader())
                {
                    while (dr.Read())
                    {

                        InventoryIndexViewModel ivm = new InventoryIndexViewModel();
                        ivm.InventoryID = Convert.ToString(dr["InventoryID"]);
                        ivm.Description = Convert.ToString(dr["Description"]);
                        ivm.Name = Convert.ToString(dr["NAME"]);
                        ivm.Price = Convert.ToDecimal(dr["PRICE"]);
                        ivm.Quantity = Convert.ToInt32(dr["QUANTITY"]);
                        ivm.Code = Convert.ToString(dr["CODE"]);
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
        public IActionResult Create(Inventory inventory)
        {
            string strDDLValue = Request.Form["ddlSize"].ToString();
            string connectionString = Configuration["ConnectionStrings:SilkDesigns"];
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string sql = "Insert Into Inventory (Name, Price, Quantity, Description, SizeID) Values (@Name, @Price, @Quantity, @Description, @SizeID)";

                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    command.CommandType = CommandType.Text;

                    // adding parameters
                    SqlParameter parameter = new SqlParameter
                    {
                        ParameterName = "@Name",
                        Value = inventory.Name,
                        SqlDbType = SqlDbType.VarChar,
                        Size = 50
                    };
                    command.Parameters.Add(parameter);

                    parameter = new SqlParameter
                    {
                        ParameterName = "@Price",
                        Value = inventory.Price,
                        SqlDbType = SqlDbType.Money
                    };
                    command.Parameters.Add(parameter);

                    parameter = new SqlParameter
                    {
                        ParameterName = "@Quantity",
                        Value = inventory.Quantity,
                        SqlDbType = SqlDbType.Int
                    };
                    command.Parameters.Add(parameter);

                    parameter = new SqlParameter
                    {
                        ParameterName = "@Description",
                        Value = inventory.Description,
                        SqlDbType = SqlDbType.VarChar,
                        Size = 50
                    };
                    command.Parameters.Add(parameter);

                    parameter = new SqlParameter
                    {
                        ParameterName = "@SizeID",
                        Value = strDDLValue,
                        SqlDbType = SqlDbType.VarChar,
                        Size = 50
                    };
                    command.Parameters.Add(parameter);
                    connection.Open();
                    command.ExecuteNonQuery();
                    connection.Close();
                }
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
            string connectionString = Configuration["ConnectionStrings:SilkDesigns"];
            List<SelectListItem> SizeList = SilkDesignUtility.GetSizes(connectionString);
            Inventory inventory = new Inventory();
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string sql = $"Select * From Inventory Where InventoryID='{id}'";
                SqlCommand command = new SqlCommand(sql, connection);

                connection.Open();

                using (SqlDataReader dataReader = command.ExecuteReader())
                {
                    while (dataReader.Read())
                    {
                        inventory.InventoryID = Convert.ToString(dataReader["InventoryID"]);
                        inventory.Name = Convert.ToString(dataReader["Name"]);
                        inventory.Description = Convert.ToString(dataReader["Description"]);
                        inventory.Price = Convert.ToDecimal(dataReader["Price"]);
                        inventory.Quantity = Convert.ToInt32(dataReader["Quantity"]);
                        inventory.LastViewed = Convert.ToDateTime(dataReader["LastViewed"]);
                        inventory.SizeID = Convert.ToString(dataReader["SizeID"]);
                        inventory.Sizes = SizeList;
                    }
                }

                connection.Close();
            }
            return View(inventory);
        }

        [HttpPost]
        public IActionResult Update(Inventory inventory, string id)
        {
            string connectionString = Configuration["ConnectionStrings:SilkDesigns"];
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string sql = $"Update Inventory SET Name= @Name, Description= @Description, " +
                    $" Price= @Price,  " +
                    $" SizeID = @SizeID, "+
                    $" Quantity=@Quantity Where InventoryID='{id}'";

                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    command.Parameters.Clear();
                    SqlParameter NameParameter = new SqlParameter
                    {
                        ParameterName = "@Name",
                        Value = inventory.Name,
                        SqlDbType = SqlDbType.VarChar,
                        Size = 50
                    };

                    SqlParameter  DescParameter = new SqlParameter
                    {
                        ParameterName = "@Description",
                        Value = inventory.Description,
                        SqlDbType = SqlDbType.VarChar,
                        Size = 250

                    };
                    SqlParameter PriceParameter = new SqlParameter
                    {
                        ParameterName = "@Price",
                        Value = inventory.Price,
                        SqlDbType = SqlDbType.Decimal

                    };
                    SqlParameter QtyParameter = new SqlParameter
                    {
                        ParameterName = "@Quantity",
                        Value = inventory.Quantity,
                        SqlDbType = SqlDbType.Int

                    };
                    SqlParameter SizeParameter = new SqlParameter
                    {
                        ParameterName = "@SizeID",
                        Value = inventory.SizeID,
                        SqlDbType = SqlDbType.VarChar
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
    }
}