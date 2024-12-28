﻿using Microsoft.AspNetCore.Mvc;
using SilkDesign.Models;
using System.Data.SqlClient;
using System.Data;

namespace SilkDesign.Controllers
{
    public class SizeController : Controller
    {
        public IConfiguration Configuration { get; }

        public SizeController(IConfiguration configuration)
        {
            Configuration = configuration;
        }


        public IActionResult Index()
        {

            List<Size> ivmList = new List<Size>();
            string connectionString = Configuration["ConnectionStrings:SilkDesigns"];
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                string sql = "SELECT " +
                    " r.SizeId      ID " +
                    " ,r.Code        CODE " +
                    " ,isNull(r.SortOrder,99)   SORTORDER " +
                    " ,r.Description DESCRIPTION " +
                    " FROM Size r " +
                    " ORDER BY SORTORDER ";

                SqlCommand readcommand = new SqlCommand(sql, connection);

                using (SqlDataReader dr = readcommand.ExecuteReader())
                {
                    while (dr.Read())
                    {

                        Size ivm = new Size();
                        ivm.SizeID = Convert.ToString(dr["ID"]);
                        ivm.Code = Convert.ToString(dr["CODE"]);
                        ivm.Description = Convert.ToString(dr["DESCRIPTION"]);
                        ivm.SortOrder = Convert.ToInt32(dr["SORTORDER"]);
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
        public IActionResult Create(Size Size)
        {
            //string strDDLValue = Request.Form["ddlSize"].ToString();
            string connectionString = Configuration["ConnectionStrings:SilkDesigns"];
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                if (!CodeExits(Size.Code, connection))
                {
                    string sql = "Insert Into Size (Code, Description, SortOrder) Values (@Code, @Description, @SortOrder)";

                    using (SqlCommand command = new SqlCommand(sql, connection))
                    {
                        command.CommandType = CommandType.Text;

                        // adding parameters
                        SqlParameter parameter = new SqlParameter
                        {
                            ParameterName = "@Code",
                            Value = Size.Code,
                            SqlDbType = SqlDbType.VarChar,
                            Size = 10
                        };
                        command.Parameters.Add(parameter);

                        parameter = new SqlParameter
                        {
                            ParameterName = "@Description",
                            Value = Size.Description,
                            SqlDbType = SqlDbType.VarChar
                        };
                        command.Parameters.Add(parameter);

                        parameter = new SqlParameter
                        {
                            ParameterName = "@SortOrder",
                            Value = Size.SortOrder,
                            SqlDbType = SqlDbType.Int
                        };
                        command.Parameters.Add(parameter);
                        try
                        {
                            connection.Open();
                            command.ExecuteNonQuery();
                            connection.Close();
                        }
                        catch (SqlException sqlEx)
                        {
                            int iError = sqlEx.ErrorCode;
                        
                        }
                        catch (Exception ex)
                        {
                            string abc = ex.Message;
                        
                        }

                    }

                    ViewBag.Result = "Success";
                }
                else
                {
                    ViewBag.Result = "Failue: Code '" + Size.Code + "' already exits. Can not add duplicate";
                    return View();
                }
            }

            return RedirectToAction("Index");
        }

        private bool CodeExits(string code, SqlConnection connection)
        {
            bool bRetValue = false;
            connection.Open();
            string sql = $"SELECT r.Code CODE  FROM Size r WHERE r.Code = '{code}'";

            SqlCommand readcommand = new SqlCommand(sql, connection);

            using (SqlDataReader dr = readcommand.ExecuteReader())
            {
                while (dr.Read())
                {
                    string Code = Convert.ToString(dr["CODE"]);
                    bRetValue = true;
                }
            }
            connection.Close();
            return bRetValue;
        }

        public IActionResult Update(string id)
        {
            string connectionString = Configuration["ConnectionStrings:SilkDesigns"];

            Size size = new Size();
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string sql = $"Select * From Size Where SizeId='{id}'";
                SqlCommand command = new SqlCommand(sql, connection);

                connection.Open();

                using (SqlDataReader dataReader = command.ExecuteReader())
                {
                    while (dataReader.Read())
                    {
                        size.SizeID = Convert.ToString(dataReader["SizeId"]);
                        size.Code = Convert.ToString(dataReader["Code"]);
                        size.Description = Convert.ToString(dataReader["Description"]);
                    }
                }

                connection.Close();
            }
            return View(size);
        }

        [HttpPost]
        public IActionResult Update(Size Size, string id)
        {
            string connectionString = Configuration["ConnectionStrings:SilkDesigns"];
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string sql = $"Update Size SET Code='{Size.Code}', Description='{Size.Description}', SortOrder={Size.SortOrder} Where SizeId='{id}'";
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