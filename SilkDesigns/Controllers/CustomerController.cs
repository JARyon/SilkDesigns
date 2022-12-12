using Microsoft.AspNetCore.Mvc;
using SilkDesign.Models;
using System.Data.SqlClient;
using System.Dynamic;
using SilkDesign.Shared;

namespace SilkDesign.Controllers
{
    public class CustomerController : Controller
    {
        const string sCustomerType = "Customer";
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
                    " FROM Customer c " +
                    " Order by c.Name ";

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

            string connectionString = Configuration["ConnectionStrings:SilkDesigns"];

            string sCustomerLocationTypeID = SilkDesignUtility.GetCustomerLocationTypeID(connectionString);
            string sCustomerID = SilkDesignUtility.CreateCustomer(connectionString, customer);
            string sLocationID = SilkDesignUtility.CreateLocation(connectionString, customer.Name, "Default Customer Site", sCustomerLocationTypeID);
            string sCustLocationID = SilkDesignUtility.CreateCustomerLocation(connectionString, sCustomerID, sLocationID);

            ViewBag.Result = "Success";
            return View();
        }

        public IActionResult Update(string id)
        {
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
            CustomerLocations.Customers = GetCustomers(connectionString, id);
            CustomerLocations.Loations = GetLocations(connectionString, id);
            return View(CustomerLocations);
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
                    $" where cl.CustomerId='{id}'";

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

        private List<Customer> GetCustomers(string? connectionString, string id)
        {
            List<Customer> list = new List<Customer>();
            Models.Customer customer = new Models.Customer();
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string sql = $"Select * From Customer " +
                    $"Where CustomerId='{id}'";
                SqlCommand command = new SqlCommand(sql, connection);

                connection.Open();

                using (SqlDataReader dataReader = command.ExecuteReader())
                {
                    while (dataReader.Read())
                    {
                        customer.CustomerId = Convert.ToString(dataReader["CustomerId"]);
                        customer.Name = Convert.ToString(dataReader["Name"]);
                        customer.Address = Convert.ToString(dataReader["Address"]);
                        list.Add(customer);
                    }
                }

                connection.Close();
            }
            return list;
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

        public IActionResult Cancel()
        {
            return Redirect(Request.Headers["Referer"].ToString());
        }
    }
}