using Microsoft.AspNetCore.Mvc;
using SilkDesign.Models;
using System.Data;
using System.Data.SqlClient;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using System.Xml;
using System.Collections;

namespace SilkDesign.Controllers
{
    public class CustomerController : Controller
    {

        public IConfiguration Configuration { get; }

        public CustomerController(IConfiguration configuration)
        {
            Configuration = configuration;
        }


        public IActionResult Index()
        {

            List<CustomerIndexViewModel> ivmList = new List<CustomerIndexViewModel>();
            string connectionString = Configuration["ConnectionStrings:SilkDesigns"];
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                string sql = "SELECT " +
                    " c.CustomerId         ID " +
                    " ,c.Name       NAME " +
                    " ,c.Address    ADDRESS " +
                    " FROM Customer c ";

                SqlCommand readcommand = new SqlCommand(sql, connection);

                using (SqlDataReader dr = readcommand.ExecuteReader())
                {
                    while (dr.Read())
                    {

                        CustomerIndexViewModel ivm = new CustomerIndexViewModel();
                        ivm.CustomerId = Convert.ToString(dr["ID"]);
                        ivm.Name = Convert.ToString(dr["NAME"]);
                        ivm.Address = Convert.ToString(dr["ADDRESS"]);
                        ivmList.Add(ivm);
                    }
                }
                connection.Close();
            }

            return View(ivmList);
        }
        public ActionResult Create()
        {

            return View();

        }

        [HttpPost]
        public IActionResult Create(SilkDesign.Models.Customer customer)
        {

            string strDDLValue = Request.Form["ddlSize"].ToString();
            string connectionString = Configuration["ConnectionStrings:SilkDesigns"];
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string sql = "Insert Into Customer (Name, Address) Values (@Name, @Address)";

                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    command.CommandType = CommandType.Text;

                    // adding parameters
                    SqlParameter parameter = new SqlParameter
                    {
                        ParameterName = "@Name",
                        Value = customer.Name,
                        SqlDbType = SqlDbType.VarChar,
                        Size = 50
                    };
                    command.Parameters.Add(parameter);

                    parameter = new SqlParameter
                    {
                        ParameterName = "@Address",
                        Value = customer.Address,
                        SqlDbType = SqlDbType.VarChar
                    };
                    command.Parameters.Add(parameter);

                    connection.Open();
                    try
                    {
                        command.ExecuteNonQuery();

                        string sCustomerID = string.Empty;
                        string sLocationID = string.Empty;

                        //string sql = $"Select * From Customer Where Id='{id}'";
                        string sCustomerSQL = $"Select CustomerID from customer where NAME = '{customer.Name}'";
                        command.Parameters.Clear();
                        command.CommandText = sCustomerSQL; ;

                        //SqlCommand readcommand = new SqlCommand(sql, connection);

                        using (SqlDataReader dr = command.ExecuteReader())
                        {
                            while (dr.Read())
                            {
                                sCustomerID = Convert.ToString(dr["CustomerID"]);
                            }
                        }

                        // Create a customer location 
                        string locationSql = "Insert Into Location (Name, Description) Values (@Name, @Description)";
                        command.CommandText = locationSql;

                        command.Parameters.Clear();
                        parameter = new SqlParameter
                        {
                            ParameterName = "@Name",
                            Value = customer.Name,
                            SqlDbType = SqlDbType.VarChar,
                            Size = 50
                        };
                        command.Parameters.Add(parameter);

                        parameter = new SqlParameter
                        {
                            ParameterName = "@Description",
                            Value = "Default Customer Site",
                            SqlDbType = SqlDbType.VarChar,
                            Size = 250

                        };
                        command.Parameters.Add(parameter);
                        command.ExecuteNonQuery();

                        string sLocationSQL = $"Select LocationID from location where NAME = '{customer.Name}'";
                        command.Parameters.Clear();
                        command.CommandText = sLocationSQL; ;

                        using (SqlDataReader dr = command.ExecuteReader())
                        {
                            while (dr.Read())
                            {
                                sLocationID = Convert.ToString(dr["LocationID"]);
                            }
                        }

                        string CustomerlocationSql = $"Insert Into CustomerLocation (LocationID, CustomerID)  " +
                            $"values (cast('{sLocationID}' AS UNIQUEIDENTIFIER), " +
                                    $"cast('{sCustomerID}' AS UNIQUEIDENTIFIER))";
                        command.CommandText = CustomerlocationSql;

                        command.Parameters.Clear();
                        command.ExecuteNonQuery();


                    }
                    catch (Exception ex) { }
                    finally { connection.Close(); }


                }
            }
            ViewBag.Result = "Success";
            return View();
        }

        public IActionResult Update(string id)
        {
            string connectionString = Configuration["ConnectionStrings:SilkDesigns"];

            Models.Customer customer = new Models.Customer();
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string sql = $"Select * From Customer Where CustomerId='{id}'";
                SqlCommand command = new SqlCommand(sql, connection);

                connection.Open();

                using (SqlDataReader dataReader = command.ExecuteReader())
                {
                    while (dataReader.Read())
                    {
                        customer.CustomerId = Convert.ToString(dataReader["CustomerId"]);
                        customer.Name = Convert.ToString(dataReader["Name"]);
                        customer.Address = Convert.ToString(dataReader["Address"]);
                    }
                }

                connection.Close();
            }
            return View(customer);
        }

        [HttpPost]
        public IActionResult Update(Models.Customer customer, int id)
        {
            string connectionString = Configuration["ConnectionStrings:SilkDesigns"];
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string sql = $"Update Customer SET Name='{customer.Name}', Address='{customer.Address}' Where Id='{id}'";
                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    connection.Open();
                    command.ExecuteNonQuery();
                    connection.Close();
                }
            }

            return RedirectToAction("Index");
        }

        public IActionResult Cancel()
        {
            return Redirect(Request.Headers["Referer"].ToString());
        }
    }
}