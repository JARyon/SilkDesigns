using Microsoft.AspNetCore.Mvc;
using SilkDesign.Models;
using System.Data.SqlClient;
using System.Data;

namespace SilkDesign.Controllers
{
    public class RouteController : Controller
    {
        public IConfiguration Configuration { get; }

        public RouteController(IConfiguration configuration)
        {
            Configuration = configuration;
        }


        public IActionResult Index()
        {

            List<Models.Route> ivmList = new List<Models.Route>();
            string connectionString = Configuration["ConnectionStrings:SilkDesigns"];
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                string sql = "SELECT " +
                    " r.RouteId      ID " +
                    " ,r.Name        NAME " +
                    " ,r.Description DESCRIPTION " +
                    " FROM Route r ";

                SqlCommand readcommand = new SqlCommand(sql, connection);

                using (SqlDataReader dr = readcommand.ExecuteReader())
                {
                    while (dr.Read())
                    {

                        Models.Route ivm = new Models.Route();
                        ivm.RouteId = Convert.ToInt32(dr["ID"]);
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
        public IActionResult Create(SilkDesign.Models.Route route)
        {

            //string strDDLValue = Request.Form["ddlSize"].ToString();
            string connectionString = Configuration["ConnectionStrings:SilkDesigns"];
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string sql = "Insert Into Route (Name, Description) Values (@Name, @Description)";

                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    command.CommandType = CommandType.Text;

                    // adding parameters
                    SqlParameter parameter = new SqlParameter
                    {
                        ParameterName = "@Name",
                        Value = route.Name,
                        SqlDbType = SqlDbType.VarChar,
                        Size = 50
                    };
                    command.Parameters.Add(parameter);

                    parameter = new SqlParameter
                    {
                        ParameterName = "@Description",
                        Value = route.Description,
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

        public IActionResult Update(int id)
        {
            string connectionString = Configuration["ConnectionStrings:SilkDesigns"];

            Models.Route route = new Models.Route();
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string sql = $"Select * From Route Where RouteId='{id}'";
                SqlCommand command = new SqlCommand(sql, connection);

                connection.Open();

                using (SqlDataReader dataReader = command.ExecuteReader())
                {
                    while (dataReader.Read())
                    {
                        route.RouteId = Convert.ToInt32(dataReader["RouteId"]);
                        route.Name = Convert.ToString(dataReader["Name"]);
                        route.Description = Convert.ToString(dataReader["Description"]);
                    }
                }

                connection.Close();
            }
            return View(route);
        }

        [HttpPost]
        public IActionResult Update(Models.Route route, int id)
        {
            string connectionString = Configuration["ConnectionStrings:SilkDesigns"];
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string sql = $"Update Route SET Name='{route.Name}', Description='{route.Description}' Where RouteId='{id}'";
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
