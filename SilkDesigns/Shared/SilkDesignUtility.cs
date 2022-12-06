using Microsoft.AspNetCore.Mvc.Rendering;
using SilkDesign.Models;
using System.Data;
using System.Data.SqlClient;
using System.Net.NetworkInformation;
using System.Reflection.Metadata;

namespace SilkDesign.Shared
{
    public static class SilkDesignUtility
    {
        const string sCustomerType = "Customer";
        public static string GetCustomerLocationTypeID(string connectionString)
        {
            string sRetValue = string.Empty;

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                string sCustomerTypeSQL = $"Select LocationTypeID from LocationType where Code = '{sCustomerType}'";
                using (SqlCommand command = new SqlCommand(sCustomerTypeSQL, connection))
                {
                    command.Parameters.Clear();
                    command.CommandText = sCustomerTypeSQL;

                    using (SqlDataReader dr = command.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            sRetValue = Convert.ToString(dr["LocationTypeID"]);
                        }
                    }
                }
                return sRetValue;
            }
        }
        public static String GetCustomerNameById(string connectionString, string id)
        {
            string sRetValue = string.Empty;

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                string sCustomerNameSQL = $"Select Name from Customer where CustomerID = @CustomerID";
                using (SqlCommand command = new SqlCommand(sCustomerNameSQL, connection))
                {
                    command.Parameters.Clear();

                    // adding parameters
                    SqlParameter parameter = new SqlParameter
                    {
                        ParameterName = "@CustomerID",
                        Value = id,
                        SqlDbType = SqlDbType.VarChar
                    };
                    command.Parameters.Add(parameter);
                    //command.CommandText = sCustomerTypeSQL;

                    using (SqlDataReader dr = command.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            sRetValue = Convert.ToString(dr["Name"]);
                        }
                    }
                }
                connection.Close();
            }

            return sRetValue;
        }
        public static string GetLocationNameById(string connectionString, string id)
        {
            string sRetValue = string.Empty;

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                string sLocationNameSQL = $"Select Name from Location where LocationID = @LocationID";
                using (SqlCommand command = new SqlCommand(sLocationNameSQL, connection))
                {
                    command.Parameters.Clear();

                    // adding parameters
                    SqlParameter parameter = new SqlParameter
                    {
                        ParameterName = "@LocationID",
                        Value = id,
                        SqlDbType = SqlDbType.VarChar
                    };
                    command.Parameters.Add(parameter);

                    using (SqlDataReader dr = command.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            sRetValue = Convert.ToString(dr["Name"]);
                        }
                    }
                }
                connection.Close();
            }

            return sRetValue;
        }

        public static  List<SelectListItem> GetSizes(string connectionString)
        {
            List<SelectListItem> list = new List<SelectListItem>();
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    string sql = "Select * from Size order by SortOrder";
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
        public static string CreateLocation(string connectionString, string LocationName, string Description, string LocationTypeID)
        {
            string sLocationID = string.Empty;
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string sql = "Insert Into Location (Name, Description, LocationTypeID) Values (@Name, @Description, @LocationTypeID)";

                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    command.CommandType = CommandType.Text;
                    if (String.IsNullOrEmpty(Description))
                    {
                        Description = "";
                    }
                    // adding parameters
                    SqlParameter parameter = new SqlParameter
                    {
                        ParameterName = "@Name",
                        Value = LocationName,
                        SqlDbType = SqlDbType.VarChar,
                        Size = 50
                    };
                    command.Parameters.Add(parameter);

                    parameter = new SqlParameter
                    {
                        ParameterName = "@Description",
                        Value = Description,
                        SqlDbType = SqlDbType.VarChar
                    };
                    command.Parameters.Add(parameter);

                    parameter = new SqlParameter
                    {
                        ParameterName = "@LocationTypeID",
                        Value = LocationTypeID,
                        SqlDbType = SqlDbType.VarChar
                    };
                    command.Parameters.Add(parameter);

                    connection.Open();
                    command.ExecuteNonQuery();

                    //Get newly created location ID
                    string sLocationSQL = $"Select LocationID from location where NAME = @Name";
                    command.Parameters.Clear();
                    parameter = new SqlParameter
                    {
                        ParameterName = "@Name",
                        Value = LocationName,
                        SqlDbType = SqlDbType.VarChar,
                        Size = 50
                    };
                    command.Parameters.Add(parameter);
                    command.CommandText = sLocationSQL;

                    using (SqlDataReader dr = command.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            sLocationID = Convert.ToString(dr["LocationID"]);
                        }
                    }
                    connection.Close();
                }
                return sLocationID;
            }
        }
        public static string CreateCustomer(string connectionString, SilkDesign.Models.Customer customer)
        {
            string sCustomerID = string.Empty;
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

                        string sLocationID = string.Empty;

                        string sCustomerSQL = $"Select CustomerID from customer where NAME = @Name";
                        command.Parameters.Clear();
                        parameter = new SqlParameter
                        {
                            ParameterName = "@Name",
                            Value = customer.Name,
                            SqlDbType = SqlDbType.VarChar,
                            Size = 50
                        };
                        command.Parameters.Add(parameter);
                        command.CommandText = sCustomerSQL;

                        using (SqlDataReader dr = command.ExecuteReader())
                        {
                            while (dr.Read())
                            {
                                sCustomerID = Convert.ToString(dr["CustomerID"]);
                            }
                        }
                    }
                    catch (Exception ex)
                    { }
                    finally
                    { connection.Close(); }
                }
            }
            return sCustomerID;
        }
        public static string CreateCustomerLocation(string connectionString, string sCustomerID, string sLocationID)
        {
            string sCustomerLocationID = string.Empty;
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                // Create a customer location 
                string sCustomerlocationSql = $"Insert Into CustomerLocation (LocationID, CustomerID)  " +
                    $"values (cast('{sLocationID}' AS UNIQUEIDENTIFIER), " +
                            $"cast('{sCustomerID}' AS UNIQUEIDENTIFIER))";
                using (SqlCommand command = new SqlCommand(sCustomerlocationSql, connection))
                {
                    connection.Open();
                    command.Parameters.Clear();
                    command.ExecuteNonQuery();

                    string sCustomerLocationSQL = $"Select CustLocationID from CustomerLocation where LocationID = @LocationID and CustomerID = @CustomerID";
                    command.Parameters.Clear();
                    SqlParameter parameter = new SqlParameter
                    {
                        ParameterName = "@LocationID",
                        Value = sLocationID,
                        SqlDbType = SqlDbType.VarChar
                    };
                    command.Parameters.Add(parameter);
                    parameter = new SqlParameter
                    {
                        ParameterName = "@CustomerID",
                        Value = sCustomerID,
                        SqlDbType = SqlDbType.VarChar
                    };
                    command.Parameters.Add(parameter);

                    command.CommandText = sCustomerLocationSQL;

                    using (SqlDataReader dr = command.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            sCustomerLocationID = Convert.ToString(dr["CustLocationID"]);
                        }
                    }

                    connection.Close();
                }

            }
            return sCustomerLocationID;
        }

        public static string CreateLocationArrangement(string connectionString, string sSizeID, string sDescription, string sLocationID)
        {
            string sLocationArrangementID = string.Empty;
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string sql = "Insert Into LocationArrangement (LocationID, SizeID, Description) Values (@LocationID, @SizeID, @Description)";

                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    command.CommandType = CommandType.Text;

                    // adding parameters
                    SqlParameter parameter = new SqlParameter
                    {
                        ParameterName = "@LocationID",
                        Value = sLocationID,
                        SqlDbType = SqlDbType.VarChar,
                        Size = 50
                    };
                    command.Parameters.Add(parameter);

                    parameter = new SqlParameter
                    {
                        ParameterName = "@SizeID",
                        Value = sSizeID,
                        SqlDbType = SqlDbType.VarChar
                    };
                    command.Parameters.Add(parameter);

                    parameter = new SqlParameter
                    {
                        ParameterName = "@Description",
                        Value = sDescription,
                        SqlDbType = SqlDbType.VarChar
                    };
                    command.Parameters.Add(parameter);
                    connection.Open();
                    try
                    {
                        command.ExecuteNonQuery();

                        string sCustomerSQL = $"Select LocationArrangementID from LocationArrangement where NAME = @Description";
                        command.Parameters.Clear();
                        parameter = new SqlParameter
                        {
                            ParameterName = "@Description",
                            Value = sDescription,
                            SqlDbType = SqlDbType.VarChar
                        };
                        command.Parameters.Add(parameter);
                        command.CommandText = sCustomerSQL;

                        using (SqlDataReader dr = command.ExecuteReader())
                        {
                            while (dr.Read())
                            {
                                sLocationArrangementID = Convert.ToString(dr["LocationArrangementID"]);
                            }
                        }
                    }
                    catch (Exception ex)
                    { }
                    finally
                    { connection.Close(); }
                }
            }
            return sLocationArrangementID;
        }


    }
}
