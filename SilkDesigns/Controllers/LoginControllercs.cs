using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using SilkDesign.Models;
using SilkDesign.Shared;
using System.Data.SqlClient;
using System.Runtime.Serialization.Formatters.Binary;

namespace SilkDesign.Controllers
{
    public class LoginController: Controller
    {
       //private readonly IHttpContextAccessor contxt;
       //   public IConfiguration Configuration { get; }

       // public LoginControllercs(IHttpContextAccessor httpContextAccessor)
       // {
       //     contxt = httpContextAccessor;   
       // }

        public IConfiguration Configuration { get; }

        public LoginController(IConfiguration configuration)
    {
        Configuration = configuration;
    }

        public IActionResult Index()
        {
            string sUserName = string.Empty;
            if (HttpContext.Session.IsAvailable)
            {
                sUserName = HttpContext.Session.GetString("UserName");
                if (!String.IsNullOrEmpty(sUserName))
                {
                    RedirectToAction("Index", "Home");
                }
            }
            return RedirectToAction("Login", "Login");

            #region old code
            //List<Login> ivmList = new List<Login>();
            //string connectionString = Configuration["ConnectionStrings:SilkDesigns"];
            //using (SqlConnection connection = new SqlConnection(connectionString))
            //{
            //    connection.Open();
            //    string sql = "SELECT " +
            //        " c.Id         ID " +
            //        " ,c.UserName       NAME " +
            //        " FROM Users c " +
            //        " Order by c.UserName ";

            //    SqlCommand readcommand = new SqlCommand(sql, connection);

            //    using (SqlDataReader dr = readcommand.ExecuteReader())
            //    {
            //        while (dr.Read())
            //        {

            //            Login ivm = new Login();
            //            ivm.Id = Convert.ToString(dr["ID"]);
            //            ivm.UserName = Convert.ToString(dr["NAME"]);
            //            ivmList.Add(ivm);
            //        }
            //    }
            //    connection.Close();
            //}
            #endregion

        }

        public IActionResult Login(string id)
        {
            bool isDevelopment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development";
            if (isDevelopment)
            {
                Login credentials = new Login();
                credentials.UserName = "jaryon@yahoo.com";
                credentials.PasswordHash = "password";
                return View(credentials);
            }
            else
            {
                return View();
            }

        }

        [HttpPost]
        public IActionResult Login(Login credentials)
        {
            
            string connectionString = Configuration["ConnectionStrings:SilkDesigns"];

            var errors = ModelState
                .Where(x => x.Value.Errors.Count > 0)
                .Select(x => new { x.Key, x.Value.Errors })
                .ToArray();

            if (ModelState.IsValid)
            {
                bool isValidLogin = SilkDesignUtility.ValidateLogin(connectionString, ref credentials);
                //bool isValidLogin = true;
                if (!isValidLogin)
                {
                    ModelState.AddModelError("ValidLogin", "Unable to login. Either your id or password was incorrect.");
                    return View();
                }
                else
                {
                    HttpContext.Session.SetString("UserID", credentials.Id);
                    HttpContext.Session.SetString("UserName", credentials.UserName);
                    HttpContext.Session.SetString("IsAdmin", credentials.IsAdmin == true ? "Y" : "N");
                    ViewBag.UserName = credentials.UserName;
                    return RedirectToAction("Index", "Home");
                }
            }
            else
            {
                HttpContext.Session.SetString("UserID", "");
                HttpContext.Session.SetString("UserName", "");
                return View(credentials);
            }
        }

        public IActionResult Logout(string id)
        {
            HttpContext.Session.SetString("UserName", "");
            HttpContext.Session.SetString("UserID", "");
            return RedirectToAction("Login", "Login");
        }

        public static byte[] ObjectToByteArray(Object obj)
        {
            BinaryFormatter bf = new BinaryFormatter();
            using (var ms = new MemoryStream())
            {
                bf.Serialize(ms, obj);
                return ms.ToArray();
            }
        }
    }
}
