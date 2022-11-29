using Microsoft.AspNetCore.Mvc;
using ADO.Models;
using System.Data;
using System.Data.SqlClient;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;

namespace ADO.Controllers
{
    public class HomeController : Controller
    {
        public static Size cMain;
        public IConfiguration Configuration { get; }

        public HomeController(IConfiguration configuration)
        {
            Configuration = configuration;
        }
        //public IActionResult Index()
        //{
        //    return View();
        //}

        public IActionResult Index()
        {
            Size s = new Size();
            cMain = new Size();
            //s.Sizes = cMain.Sizes = GetSizes();
            List<SelectListItem> SizeList = GetSizes();
            Size[] arSize = GetArraySize();

            List<Inventory> inventoryList = new List<Inventory>();
            List<InventoryIndexViewModel> ivmList = new List<InventoryIndexViewModel>();
            string connectionString = Configuration["ConnectionStrings:SilkDesigns"];
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                string sql = "SELECT " +
                    " i.Id         ID " +
                    ",i.Name       NAME " +
                    ",i.Price      PRICE " +
                    ",i.Quantity   QUANTITY " +
                    ",i.lastViewed LASTVIEWED " +
                    ",i.SizeID     SIZEID " +
                    ",s.Code       CODE " +
                    "FROM Inventory i " +
                    "join Size s on i.SizeID = s.SizeId";
                SqlCommand readcommand = new SqlCommand(sql, connection);

                using (SqlDataReader dr = readcommand.ExecuteReader())
                {
                    while (dr.Read())
                    {
                        //Inventory inventory = new Inventory();
                        //inventory.Id = Convert.ToInt32(dr["ID"]);
                        //inventory.Name = Convert.ToString(dr["NAME"]);
                        //inventory.Price = Convert.ToDecimal(dr["PRICE"]);
                        //inventory.Quantity = Convert.ToInt32(dr["QUANTITY"]);
                        //inventory.LastViewed = Convert.ToDateTime(dr["LASTVIEWED"]);
                        //inventoryList.Add(inventory);
                        //inventory.arSize = arSize;

                        InventoryIndexViewModel ivm = new InventoryIndexViewModel();
                        ivm.Id = Convert.ToInt32(dr["ID"]);
                        ivm.Name = Convert.ToString(dr["NAME"]);
                        ivm.Price = Convert.ToDecimal(dr["PRICE"]);
                        ivm.Quantity = Convert.ToInt32(dr["QUANTITY"]);
                        ivm.Code = Convert.ToString(dr["CODE"]);
                        ivmList.Add(ivm);
                    }
                }
                connection.Close();
            }


            ViewBag.ListofSizes = SizeList;
            //return View(inventoryList);
            return View(ivmList);
        }
        public ActionResult Create()
        {

            ViewBag.ListOfSizes2 = GetSizes();
            return View();
            
        }

        [HttpPost]
        public IActionResult Create(Inventory inventory)
        {
            //Size s = new Size();
            //cMain = new Size();
            //s.Sizes = cMain.Sizes = GetSizes();
            string strDDLValue = Request.Form["ddlSize"].ToString();
            string connectionString = Configuration["ConnectionStrings:SilkDesigns"];
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string sql = "Insert Into Inventory (Name, Price, Quantity, SizeID) Values (@Name, @Price, @Quantity, @SizeID)";

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
                        ParameterName = "@SizeID",
                        Value = strDDLValue,
                        SqlDbType = SqlDbType.Int
                    };
                    command.Parameters.Add(parameter);
                    connection.Open();
                    command.ExecuteNonQuery();
                    connection.Close();
                }
            }
            ViewBag.Result = "Success";
            ViewBag.ListOfSizes2 = GetSizes();
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
                            Size sizeOption = new Size(reader["Code"].ToString(), Convert.ToInt32(reader["SizeID"].ToString()));
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
        public List<SelectListItem> GetSizes()
        {
            List<SelectListItem> list = new List<SelectListItem>();
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
                            list.Add(new SelectListItem { Text = reader["Code"].ToString(), Value = reader["SizeID"].ToString() });
                        }
                    }
                    else
                    {
                        list.Add(new SelectListItem { Text = "No sizes found", Value = "0" });
                    }
                    list.Insert(0, new SelectListItem { Text = "-- Select Size--", Value = "0" });
                    connection.Close();
                }
            }
            catch (Exception ex)
            {
                list.Add(new SelectListItem { Text = ex.Message.ToString(), Value = "0" });
            }
        
            return list;
        }
        public IActionResult Update(int id)
        {
            string connectionString = Configuration["ConnectionStrings:SilkDesigns"];

            Inventory inventory = new Inventory();
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string sql = $"Select * From Inventory Where Id='{id}'";
                SqlCommand command = new SqlCommand(sql, connection);

                connection.Open();

                using (SqlDataReader dataReader = command.ExecuteReader())
                {
                    while (dataReader.Read())
                    {
                        inventory.Id = Convert.ToInt32(dataReader["Id"]);
                        inventory.Name = Convert.ToString(dataReader["Name"]);
                        inventory.Price = Convert.ToDecimal(dataReader["Price"]);
                        inventory.Quantity = Convert.ToInt32(dataReader["Quantity"]);
                        inventory.LastViewed = Convert.ToDateTime(dataReader["LastViewed"]);
                    }
                }

                connection.Close();
            }
            return View(inventory);
        }

        [HttpPost]
        public IActionResult Update(Inventory inventory, int id)
        {
            string connectionString = Configuration["ConnectionStrings:SilkDesigns"];
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string sql = $"Update Inventory SET Name='{inventory.Name}', Price='{inventory.Price}', Quantity='{inventory.Quantity}' Where Id='{id}'";
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