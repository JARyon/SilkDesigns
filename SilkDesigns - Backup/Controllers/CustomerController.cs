using Microsoft.AspNetCore.Mvc;
using SilkDesign.Models;
using System.Data.SqlClient;
using System.Dynamic;
using SilkDesign.Shared;
using Newtonsoft.Json.Linq;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;
using System.Data;
using System.Reflection.Metadata;

namespace SilkDesign.Controllers
{
    public class CustomerController : Controller 
    {
        const string sCustomerType = "Customer";
        string msUserName = string.Empty;
        string msUserID = string.Empty;
        string msIsAdmin = string.Empty;
        string msconnectionString = string.Empty;

        public IConfiguration Configuration { get; }

        public CustomerController(IConfiguration configuration)
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

            List<CustomerIndexViewModel> ivmList = new List<CustomerIndexViewModel>();
            string connectionString = Configuration["ConnectionStrings:SilkDesigns"];
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                string sql = "SELECT " +
                    " c.CustomerId         ID " +
                    " ,c.Name       NAME " +
                    " ,c.Address    ADDRESS " +
                    " FROM Customer c " +
                    " WHERE Deleted = 'N' " +
                    " AND UserID = @UserID " +
                    " Order by c.Name ";

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
                    if (dr.HasRows)
                    {
                        while (dr.Read())
                        {

                            CustomerIndexViewModel ivm = new CustomerIndexViewModel();
                            ivm.CustomerId = Convert.ToString(dr["ID"]);
                            ivm.Name = Convert.ToString(dr["NAME"]);
                            ivm.Address = Convert.ToString(dr["ADDRESS"]);
                            ivm.UserName = msUserName;
                            ivmList.Add(ivm);
                        }
                    }
                }
                connection.Close();
            }

            return View(ivmList);
        }
        public ActionResult Create()
        {
            ISession currentSession = HttpContext.Session;
            if (!ControllersShared.IsLoggedOn(currentSession, ref msUserID, ref msUserName, ref msIsAdmin))
            {
                return RedirectToAction("Login", "Login");
            }
            return View();

        }

        [HttpPost]
        public IActionResult Create(SilkDesign.Models.Customer customer)
        {
            string sErrorMsg = string.Empty;
            ISession currentSession = HttpContext.Session;
            if (!ControllersShared.IsLoggedOn(currentSession, ref msUserID, ref msUserName, ref msIsAdmin))
            {
                return RedirectToAction("Login", "Login");
            }
            customer.UserID = msUserID;

            string sCustomerLocationTypeID = string.Empty;
            string sCustomerID = string.Empty;
            string ssLocationID = string.Empty;
            string sCustLocationID = string.Empty;

            string connectionString = Configuration["ConnectionStrings:SilkDesigns"];

            sCustomerLocationTypeID = SilkDesignUtility.GetCustomerLocationTypeID(connectionString);
            if (sCustomerLocationTypeID.Length != 36)
            {
                ViewBag.Result = "Invalid Customer Type.";
                return View();
            }

            sCustomerID = SilkDesignUtility.CreateCustomer(connectionString, customer, ref sErrorMsg);
            if (sCustomerID.Length != 36)
            {
                ViewBag.Result = "Unable to create Customer. " + " " + sErrorMsg;
                return View();
            }
            string sLocationID = SilkDesignUtility.CreateLocation(connectionString, customer.Name, customer.Name, sCustomerLocationTypeID, msUserID, ref sErrorMsg);
            if (sLocationID.Length != 36)
            {
                ViewBag.Result = "Unable to create Location.";
                return View();
            }

            sCustLocationID = SilkDesignUtility.CreateCustomerLocation(connectionString, sCustomerID, sLocationID);
            if (sCustLocationID.Length != 36)
            {
                ViewBag.Result = "Unable to create Location.";
                return View();
            }

            return RedirectToAction("Update", "Customer", new { id = sCustomerID });
        }

        public IActionResult Update(string id)
        {
            ISession currentSession = HttpContext.Session;
            if (!ControllersShared.IsLoggedOn(currentSession, ref msUserID, ref msUserName, ref msIsAdmin))
            {
                return RedirectToAction("Login", "Login");
            }
            string connectionString = Configuration["ConnectionStrings:SilkDesigns"];

            #region oldCode
            //Models.Customer customer = new Models.Customer();
            //using (SqlConnection connection = new SqlConnection(connectionString))
            //{
            //    string sql = $"Select * From Customer Where CustomerId='{id}'";
            //    SqlCommand command = new SqlCommand(sql, connection);

            //    connection.Open();

            //    using (SqlDataReader dataReader = command.ExecuteReader())
            //    {
            //        while (dataReader.Read())
            //        {
            //            customer.CustomerId = Convert.ToString(dataReader["CustomerId"]);
            //            customer.Name = Convert.ToString(dataReader["Name"]);
            //            customer.Address = Convert.ToString(dataReader["Address"]);
            //        }
            //    }

            //    connection.Close();
            //}
            //return View(customer);
            #endregion

            dynamic CustomerLocations = new ExpandoObject();
            CustomerLocations.Customers = GetCustomers(connectionString, msUserID, id);
            if (CustomerLocations.Customers.Count > 0)
            { 
                CustomerLocations.Loations = GetLocations(connectionString, id);
                return View(CustomerLocations);
            }
            else
            {
                return RedirectToAction("Index"); 
            }
        }

        [HttpPost]
        public IActionResult Update(Models.Customer customer, string id)
        {
            string connectionString = Configuration["ConnectionStrings:SilkDesigns"];
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string sql = $"Update Customer SET Name='{customer.Name}', Address='{customer.Address}' Where CustomerId='{id}'";
                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    connection.Open();
                    command.ExecuteNonQuery();
                    connection.Close();
                }
            }

            return RedirectToAction("Index");
        }

        private List<Location> GetLocations(string? connectionString, string id)
        {
            List<Location> ivmList = new List<Location>();
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                string sql = "SELECT " +
                    "  l.LocationID  ID " +
                    " ,l.Name        NAME " +
                    " ,l.Description DESCRIPTION " +
                    " FROM CustomerLocation cl " +
                    " join Location l on l.LocationID = cl.LocationID " +
                    $" where cl.CustomerId='{id}' " +
                    $" and l.Deleted = 'N' " +
                    $" and cl.Deleted = 'N' ";

                SqlCommand readcommand = new SqlCommand(sql, connection);

                using (SqlDataReader dr = readcommand.ExecuteReader())
                {
                    while (dr.Read())
                    {

                        Location ivm = new Location();
                        ivm.LocationID = Convert.ToString(dr["ID"]);
                        ivm.Name = Convert.ToString(dr["NAME"]);
                        ivm.Description = Convert.ToString(dr["DESCRIPTION"]);
                        ivmList.Add(ivm);
                    }
                }
                connection.Close();
            }

            return ivmList;
        }

        private List<Customer> GetCustomers(string? connectionString, string sUserID, string sCustomerId)
        {
            List<Customer> list = new List<Customer>();
            Models.Customer customer = new Models.Customer();
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string sql = $" Select * From Customer " +
                             $" Where CustomerId= @CustomerID " +
                             $" AND UserID = @UserID " +
                             $" AND Deleted = 'N' ";

                
                SqlCommand command = new SqlCommand(sql, connection);
                SqlParameter parameter = new SqlParameter
                {
                    ParameterName = "@UserID",
                    Value = sUserID,
                    SqlDbType = SqlDbType.VarChar
                };
                command.Parameters.Add(parameter);

                parameter = new SqlParameter
                {
                    ParameterName = "@CustomerID",
                    Value = sCustomerId,
                    SqlDbType = SqlDbType.VarChar
                };
                command.Parameters.Add(parameter);

                connection.Open();

                using (SqlDataReader dataReader = command.ExecuteReader())
                {
                    if (dataReader.HasRows)
                    {
                        while (dataReader.Read())
                        {
                            customer.CustomerId = Convert.ToString(dataReader["CustomerId"]);
                            customer.Name = Convert.ToString(dataReader["Name"]);
                            customer.Address = Convert.ToString(dataReader["Address"]);
                            list.Add(customer);
                        }
                    }
                }

                connection.Close();
            }
            return list;
        }

        public ActionResult InactivateCustomer(string id)
        {
            string sErrorMsg = string.Empty;

            ISession currentSession = HttpContext.Session;
            if (!ControllersShared.IsLoggedOn(currentSession, ref msUserID, ref msUserName, ref msIsAdmin))
            {
                return RedirectToAction("Login", "Login");
            }

            string sCustomerID = id;
            string connectionString = Configuration["ConnectionStrings:SilkDesigns"];
            SilkDesignUtility.DeactivateCustomer(connectionString, sCustomerID, msUserID, ref sErrorMsg);
            if (!String.IsNullOrEmpty(sErrorMsg))
            {
                ViewBag.Result = "Unable to deactivate Customer. " + sErrorMsg;
            }
            return View();

            return RedirectToAction("Index");
        }
        public IActionResult Cancel()
        {
            return Redirect(Request.Headers["Referer"].ToString());
        }
    }
}