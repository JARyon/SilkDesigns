using Microsoft.AspNetCore.Mvc;
using SilkDesign.Models;
using System.Data.SqlClient;
using System.Data;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace SilkDesign.Controllers
{
    public class LocationController : Controller
    {
        public IConfiguration Configuration { get; }

        public LocationController(IConfiguration configuration)
        {
            Configuration = configuration;
        }


        public IActionResult Index()
        {

            List<Location> ivmList = new List<Location>();
            string connectionString = Configuration["ConnectionStrings:SilkDesigns"];
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                string sql = "SELECT " +
                    " r.LocationId      ID " +
                    " ,r.Name        NAME " +
                    " ,r.Description DESCRIPTION " +
                    " FROM Location r ";

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

            return View(ivmList);
        }
        public ActionResult Create()
        {

            return View();

        }

        [HttpPost]
        public IActionResult Create(Location Location)
        {

            if (String.IsNullOrEmpty(Location.Name))
            {
                ViewBag.Result = "Location Name is required.";
                return View();
            }
            //string strDDLValue = Request.Form["ddlSize"].ToString();
            string connectionString = Configuration["ConnectionStrings:SilkDesigns"];
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string sql = "Insert Into Location (Name, Description) Values (@Name, @Description)";

                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    command.CommandType = CommandType.Text;

                    // adding parameters
                    SqlParameter parameter = new SqlParameter
                    {
                        ParameterName = "@Name",
                        Value = Location.Name,
                        SqlDbType = SqlDbType.VarChar,
                        Size = 50
                    };
                    command.Parameters.Add(parameter);

                    parameter = new SqlParameter
                    {
                        ParameterName = "@Description",
                        Value = Location.Description,
                        SqlDbType = SqlDbType.VarChar
                    };
                    command.Parameters.Add(parameter);

                    connection.Open();
                    command.ExecuteNonQuery();
                    connection.Close();
                }
            }
            ViewBag.Result = "Success";
            return View();
        }

        public IActionResult Update(string id)
        {
            string connectionString = Configuration["ConnectionStrings:SilkDesigns"];

            Location Location = new Location();
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string sql = $"Select * From Location Where LocationId='{id}'";
                SqlCommand command = new SqlCommand(sql, connection);

                connection.Open();

                using (SqlDataReader dataReader = command.ExecuteReader())
                {
                    while (dataReader.Read())
                    {
                        Location.LocationID= Convert.ToString(dataReader["LocationId"]);
                        Location.Name = Convert.ToString(dataReader["Name"]);
                        Location.Description = Convert.ToString(dataReader["Description"]);
                    }
                }

                connection.Close();
            }
            return View(Location);
        }

        [HttpPost]
        public IActionResult Update(Location Location, string id)
        {
            string connectionString = Configuration["ConnectionStrings:SilkDesigns"];
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string sql = $"Update Location SET Name='{Location.Name}', Description='{Location.Description}' Where LocationId='{id}'";
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
