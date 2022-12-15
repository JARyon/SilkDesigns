using Microsoft.AspNetCore.Mvc.Rendering;
using SilkDesign.Models;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Dynamic;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace SilkDesign.Shared
{
    public static class SilkDesignUtility
    {
        const string sCustomerType = "Customer";
        public static List<Arrangement> GetArrangements(string connectionString, string sArrangementID)
        {
            List<Arrangement> list = new List<Arrangement>();
            Arrangement arrangement = new Arrangement();
            List<SelectListItem> SizeList = SilkDesignUtility.GetSizes(connectionString);
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string sql = $"Select * From Arrangement Where ArrangementID='{sArrangementID}'";
                SqlCommand command = new SqlCommand(sql, connection);

                connection.Open();

                using (SqlDataReader dataReader = command.ExecuteReader())
                {
                    while (dataReader.Read())
                    {
                        arrangement.ArrangementID = Convert.ToString(dataReader["ArrangementID"]);
                        arrangement.Code = Convert.ToString(dataReader["Code"]);
                        arrangement.Name = Convert.ToString(dataReader["Name"]);
                        arrangement.Description = Convert.ToString(dataReader["Description"]);
                        arrangement.Price = Convert.ToDecimal(dataReader["Price"]);
                        arrangement.Quantity = Convert.ToInt32(dataReader["Quantity"]);
                        arrangement.LastViewed = Convert.ToDateTime(dataReader["LastViewed"]);
                        arrangement.SizeID = Convert.ToString(dataReader["SizeID"]);
                        arrangement.Sizes = SizeList;

                        list.Add(arrangement);
                    }
                }

                connection.Close();
            }

            return list;
        }
        public static ArrangementInventory GetArrangementInventory(string connectionString, string sArrangementInventoryID)
        {
            ArrangementInventory arrangementInventory = new ArrangementInventory();
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                string sLocationNameSQL = $"SELECT Code, " +
                                          $" LocationID, " +
                                          $" LocationPlacementID "+
                                          $" FROM ARRANGEMENTINVENTORY " +
                                          $" where ArrangementInventoryID = @ArrangementInventoryID";
                using (SqlCommand command = new SqlCommand(sLocationNameSQL, connection))
                {
                    command.Parameters.Clear();

                    //adding parameters
                    SqlParameter parameter = new SqlParameter
                    {
                        ParameterName = "@ArrangementInventoryID",
                        Value = sArrangementInventoryID,
                        SqlDbType = SqlDbType.VarChar
                    };
                    command.Parameters.Add(parameter);

                    using (SqlDataReader dr = command.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            arrangementInventory.ArrangementInventoryID = sArrangementInventoryID;
                            arrangementInventory.Code = Convert.ToString(dr["Code"]);
                            arrangementInventory.LocationID = Convert.ToString(dr["LocationID"]);
                            arrangementInventory.LocationPlacementID = Convert.ToString(dr["LocationPlacementID"]);
                            if (String.IsNullOrEmpty(arrangementInventory.LocationID))
                            {
                                arrangementInventory.LocationID = "0";
                            }
                            arrangementInventory.Locations = GetLocationDDL(connectionString);
                        }
                    }
                }
                connection.Close();
            }

            return arrangementInventory;
        }
        public static Arrangement GetArrangement(string connectionString, string sArrangementID)
        {
            Arrangement arrangement = new Arrangement();
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                string sCustomerNameSQL = $" Select * " +
                                          $" from Arrangement " +
                                          $" where ArrangementID = @ArrangementID";

                using (SqlCommand command = new SqlCommand(sCustomerNameSQL, connection))
                {
                    command.Parameters.Clear();

                    // adding parameters
                    SqlParameter parameter = new SqlParameter
                    {
                        ParameterName = "@ArrangementID",
                        Value = sArrangementID,
                        SqlDbType = SqlDbType.VarChar
                    };
                    command.Parameters.Add(parameter);

                    using (SqlDataReader dr = command.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            arrangement.Name = Convert.ToString(dr["Name"]);
                            arrangement.Code = Convert.ToString(dr["Code"]);
                            arrangement.Description = Convert.ToString(dr["Description"]);
                            arrangement.Price = Convert.ToDecimal(dr["Price"]);
                            arrangement.Quantity = Convert.ToInt32(dr["Quantity"]);
                            arrangement.LastViewed = Convert.ToDateTime(dr["LastViewed"]);
                            arrangement.SizeID = Convert.ToString(dr["SizeID"]);

                        }
                    }
                }
                connection.Close();
            }
            return arrangement;
        }

        public static List<ArrangementInventory> GetArrangementInventories(string connectionString, string sArrangementID)
        {
            List<ArrangementInventory> arrangementInventories = new List<ArrangementInventory>();
            ArrangementInventory arrangementInvetory = new ArrangementInventory();

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                string sCustomerNameSQL = $"Select ai.Code     Code, " +
                                          $" ai.ArrangementInventoryID  ID, " +
                                          $" ai.LocationPlacementID  PlacementID, " +
                                          $" p.Description           Placement, " +
                                          $" isNull(Convert(Varchar(20), lastUsed, 101), '') LastUsed, " +
                                          $" l.Name     Location " +
                                          $" from ArrangementInventory ai" +
                                          $" left outer join Location l on ai.LocationID = l.LocationID " +
                                          $" left outer join LocationPlacement P on ai.LocationPlacementID = P.LocationPlacementID " +
                                          $" where ai.ArrangementID = @ArrangementID " +
                                          $" Order by ai.Code ";

                using (SqlCommand command = new SqlCommand(sCustomerNameSQL, connection))
                {
                    command.Parameters.Clear();

                    // adding parameters
                    SqlParameter parameter = new SqlParameter
                    {
                        ParameterName = "@ArrangementID",
                        Value = sArrangementID,
                        SqlDbType = SqlDbType.VarChar
                    };
                    command.Parameters.Add(parameter);

                    using (SqlDataReader dr = command.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            ArrangementInventory arrangementInventory = new ArrangementInventory();
                            arrangementInventory.Code = Convert.ToString(dr["Code"]);
                            arrangementInventory.LastUsedDisplay = Convert.ToString(dr["LastUsed"]);
                            arrangementInventory.LocationName = Convert.ToString(dr["Location"]);
                            string sPlacement = Convert.ToString(dr["Placement"]);
                            if (!String.IsNullOrEmpty(sPlacement))
                            {
                                arrangementInventory.LocationName += " / " + sPlacement;
                            }
                            arrangementInventory.ArrangementInventoryID = Convert.ToString(dr["ID"]);
                            arrangementInventory.LocationPlacementID = Convert.ToString(dr["PlacementID"]);
                            arrangementInventories.Add(arrangementInventory);

                        }
                    }
                }
                connection.Close();
            }
            return arrangementInventories; 

        }
        public static List<Location> GetLocation(string? connectionString, string sLocationID)
        {
            List<Location> list = new List<Location>();
            Models.Location location = new Models.Location();
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string sql = $"Select * From Location " +
                    $"Where LocationId='{sLocationID}'";
                SqlCommand command = new SqlCommand(sql, connection);

                connection.Open();

                using (SqlDataReader dataReader = command.ExecuteReader())
                {
                    while (dataReader.Read())
                    {
                        location.LocationID = sLocationID;
                        location.Name = Convert.ToString(dataReader["Name"]);
                        location.Description = Convert.ToString(dataReader["Description"]);
                        list.Add(location);
                    }
                }

                connection.Close();
            }
            return list;
        }
        public static IEnumerable<SelectListItem> GetLocationDDL(string connectionString)
        {
            List<SelectListItem> list = new List<SelectListItem>();
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    string sql = "Select * from Location order by Name";
                    SqlCommand cmd = new SqlCommand(sql, connection);
                    SqlDataReader reader = cmd.ExecuteReader();

                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            list.Add(new SelectListItem { Text = reader["Name"].ToString(), Value = reader["LocationID"].ToString() });
                        }
                    }
                    else
                    {
                        list.Add(new SelectListItem { Text = "No locations found", Value = "0" });
                    }
                    list.Insert(0, new SelectListItem { Text = "-- Select Location --", Value = "0" });
                    connection.Close();
                }
            }
            catch (Exception ex)
            {
                list.Add(new SelectListItem { Text = ex.Message.ToString(), Value = "0" });
            }

            return list;
        }

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
        public static string GetCustomerNameById(string connectionString, string id)
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
        public static LocationPlacement GetLocationPlacement(string connectionString, string ArrangementID)
        {
            LocationPlacement ivm = new LocationPlacement();
            List<SelectListItem> SizeList = SilkDesignUtility.GetSizes(connectionString);

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                string sql = "SELECT " +
                    "  s.Code        CODE " +
                    " ,la.LocationID LOCATIONID " +
                    " ,s.SizeID      SIZEID " +
                    " ,la.Description DESCRIPTION " +
                    " FROM LocationPlacement la " +
                    " join Size s on s.SizeID = la.SizeID " +
                    $" where la.LocationPlacementID='{ArrangementID}'";

                SqlCommand readcommand = new SqlCommand(sql, connection);

                using (SqlDataReader dr = readcommand.ExecuteReader())
                {
                    if (dr.HasRows)
                    {
                        while (dr.Read())
                        {
                            ivm.LocationPlacementID = ArrangementID;
                            ivm.LocationID = Convert.ToString(dr["LOCATIONID"]);
                            ivm.Description = Convert.ToString(dr["DESCRIPTION"]);
                            ivm.SizeID = Convert.ToString(dr["SIZEID"]);
                            ivm.Sizes = SizeList;
                        }
                    }
                }
                connection.Close();
            }

            return ivm;
        }

        public static List<LocationPlacement> GetLoationArrangements(string? connectionString, string id)
        {
            List<LocationPlacement> ivmList = new List<LocationPlacement>();
            List<SelectListItem> SizeList = SilkDesignUtility.GetSizes(connectionString);
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                string sql = "SELECT " +
                    " la.LocationPlacementID  ID " +
                    " ,s.Code        CODE " +
                    " ,s.SizeID      SIZEID" +
                    " ,la.Description DESCRIPTION " +
                    " FROM LocationPlacement la " +
                    " join Size s on s.SizeID = la.SizeID " +
                    $" where la.LocationID='{id}'";

                SqlCommand readcommand = new SqlCommand(sql, connection);

                using (SqlDataReader dr = readcommand.ExecuteReader())
                {
                    while (dr.Read())
                    {

                        LocationPlacement ivm = new LocationPlacement();
                        ivm.LocationPlacementID = Convert.ToString(dr["ID"]);
                        ivm.SizeID = Convert.ToString(dr["SIZEID"]);
                        ivm.Code = Convert.ToString(dr["CODE"]);
                        ivm.Description = Convert.ToString(dr["DESCRIPTION"]);
                        ivm.Sizes = SizeList;
                        ivmList.Add(ivm);
                    }
                }
                connection.Close();
            }

            return ivmList;
        }
        public static List<SelectListItem> GetSizes(string connectionString)
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
        public static List<RouteLocation> GetRouteLocations(string connectionString, string sRouteID)
        {
            List<RouteLocation> list = new List<RouteLocation>();
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                string sCustomerNameSQL = $" Select rl.RouteLocationID ID, " +
                                          $" rl.LocationID             LOCATIONID, " +
                                          $" c.Name                    CUSTOMERNAME, " +
                                          $" l.Name                    LOCATIONNAME, " +
                                          $" rl.RouteOrder             ROUTEORDER " +
                                          $" from routelocation rl " +
                                          $" join Location l on l.LocationID = rl.LocationID " +
                                          $" join CustomerLocation cl on cl.locationID = l.locationID " +
                                          $" join customer c on c.customerID = cl.CustomerID " +
                                          $" where RouteID  = @RouteID " +
                                          $" ORDER BY rl.RouteOrder ASC";
                using (SqlCommand command = new SqlCommand(sCustomerNameSQL, connection))
                {
                    command.Parameters.Clear();

                    // adding parameters
                    SqlParameter parameter = new SqlParameter
                    {
                        ParameterName = "@RouteID",
                        Value = sRouteID,
                        SqlDbType = SqlDbType.VarChar
                    };
                    command.Parameters.Add(parameter);
                    //command.CommandText = sCustomerTypeSQL;

                    using (SqlDataReader dr = command.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            RouteLocation routeLocation = new RouteLocation();
                            routeLocation.RouteID = sRouteID;
                            routeLocation.RouteLocationID = Convert.ToString(dr["ID"]);
                            routeLocation.LocationID = Convert.ToString(dr["LOCATIONID"]);
                            routeLocation.LocationName = Convert.ToString(dr["LOCATIONNAME"]);
                            routeLocation.CustomerName = Convert.ToString(dr["CUSTOMERNAME"]);
                            routeLocation.RouteOrder = Convert.ToInt32(dr["ROUTEORDER"]);

                            list.Add(routeLocation);
                        }
                    }
                }
                connection.Close();
            }

            return list;
        }
        public static List<SilkDesign.Models.Route> GetRoutes(string connectionString, string sRouteID)
        {
            List<SilkDesign.Models.Route> list = new List<SilkDesign.Models.Route>();
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string sql = $"Select * From Route Where RouteId='{sRouteID}'";
                SqlCommand command = new SqlCommand(sql, connection);

                connection.Open();

                using (SqlDataReader dataReader = command.ExecuteReader())
                {
                    while (dataReader.Read())
                    {
                        Models.Route route = new Models.Route();
                        route.RouteId = Convert.ToString(dataReader["RouteId"]);
                        route.Name = Convert.ToString(dataReader["Name"]);
                        route.Description = Convert.ToString(dataReader["Description"]);
                        route.WarehouseID = Convert.ToString(dataReader["WarehouseID"]);
                        route.Warehouses = SilkDesignUtility.GetWarehouses(connectionString);
                        list.Add(route);
                    }
                }

                connection.Close();
            }

            return list;
        }
        internal static IEnumerable<SelectListItem> GetWarehouses(string? connectionString)
        {
            List<SelectListItem> list = new List<SelectListItem>();
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    string sql = "Select LocationID, Name, Description from location " +
                                 "Where locationTypeid = (Select locationTypeID  " +
                                                        " from LocationType " +
                                                        " where code = 'Warehouse')";
                    SqlCommand cmd = new SqlCommand(sql, connection);
                    SqlDataReader reader = cmd.ExecuteReader();

                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            string sText = reader["Name"].ToString();
                            string sDesc = reader["Description"].ToString();
                            if (!string.IsNullOrEmpty(sDesc))
                            {
                                sText = sText + "|" + sDesc;
                            }
                            else
                            {
                            }
                            list.Add(new SelectListItem { Text = sText, Value = reader["LocationID"].ToString() });
                        }
                    }
                    else
                    {
                        list.Add(new SelectListItem { Text = "No Warehouse found", Value = "0" });
                    }
                    list.Insert(0, new SelectListItem { Text = "-- Select Warehouse--", Value = "0" });
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
        public static string CreateLocationPlacement(string connectionString, string sSizeID, string sDescription, string sLocationID)
        {
            string sLocationPlacementID = string.Empty;
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string sql = "Insert Into LocationPlacement (LocationID, SizeID, Description) Values (@LocationID, @SizeID, @Description)";

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

                        string sCustomerSQL = $"Select LocationPlacementID from LocationPlacement where Description = @Description";
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
                                sLocationPlacementID = Convert.ToString(dr["LocationPlacementID"]);
                            }
                        }
                    }
                    catch (Exception ex)
                    { }
                    finally
                    { connection.Close(); }
                }
            }
            return sLocationPlacementID;
        }
        public static string CreateArrangement(string connectionString, Arrangement arrangement)
        {
            string sArrangementID = string.Empty;
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string sql = "Insert Into Arrangement (Code, Name, Price, Quantity, Description, SizeID) Values (@Code, @Name, @Price, @Quantity, @Description, @SizeID)";

                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    command.CommandType = CommandType.Text;

                    // adding parameters
                    SqlParameter parameter = new SqlParameter
                    {
                        ParameterName = "@Name",
                        Value = arrangement.Name,
                        SqlDbType = SqlDbType.VarChar,
                        Size = 50
                    };
                    command.Parameters.Add(parameter);

                    parameter = new SqlParameter
                    {
                        ParameterName = "@Price",
                        Value = arrangement.Price,
                        SqlDbType = SqlDbType.Money
                    };
                    command.Parameters.Add(parameter);

                    parameter = new SqlParameter
                    {
                        ParameterName = "@Quantity",
                        Value = arrangement.Quantity,
                        SqlDbType = SqlDbType.Int
                    };
                    command.Parameters.Add(parameter);

                    parameter = new SqlParameter
                    {
                        ParameterName = "@Description",
                        Value = arrangement.Description,
                        SqlDbType = SqlDbType.VarChar,
                        Size = 50
                    };
                    command.Parameters.Add(parameter);

                    parameter = new SqlParameter
                    {
                        ParameterName = "@Code",
                        Value = arrangement.Code,
                        SqlDbType = SqlDbType.VarChar,
                        Size = 50
                    };
                    command.Parameters.Add(parameter);

                    parameter = new SqlParameter
                    {
                        ParameterName = "@SizeID",
                        Value = arrangement.SizeID,
                        SqlDbType = SqlDbType.VarChar,
                        Size = 50
                    };
                    command.Parameters.Add(parameter);
                    connection.Open();
                    try
                    {
                        command.ExecuteNonQuery();

                        sArrangementID = string.Empty;

                        string sCustomerSQL = $"Select ArrangementID from Arrangement where NAME = @Name and Code = @Code";
                        command.Parameters.Clear();
                        parameter = new SqlParameter
                        {
                            ParameterName = "@Name",
                            Value = arrangement.Name,
                            SqlDbType = SqlDbType.VarChar,
                            Size = 50
                        };
                        command.Parameters.Add(parameter);
                        parameter = new SqlParameter
                        {
                            ParameterName = "@Code",
                            Value = arrangement.Code,
                            SqlDbType = SqlDbType.VarChar,
                            Size = 50
                        };
                        command.Parameters.Add(parameter);
                        command.CommandText = sCustomerSQL;

                        using (SqlDataReader dr = command.ExecuteReader())
                        {
                            while (dr.Read())
                            {
                                sArrangementID = Convert.ToString(dr["ArrangementID"]);
                            }
                        }
                    }
                    catch (Exception ex)
                    { }
                    finally
                    { connection.Close(); }

                }
            }

            return sArrangementID;
        }
        public static string CreateArrangementInventory(string connectionString, Arrangement arrangement)
        {
            string sArrangementInventoryID = string.Empty;

            string sNextCode = GetNextInventoryCode(connectionString, arrangement.Code);
            using (SqlConnection connection = new SqlConnection(connectionString))
            {

                string sql = "Insert Into ArrangementInventory (ArrangementID, Code) Values (@ArrangementID, @Code)";
                using (SqlCommand command = new SqlCommand(sql, connection))
                {

                    command.CommandType = CommandType.Text;

                    // adding parameters
                    SqlParameter parameter = new SqlParameter
                    {
                        ParameterName = "@ArrangementID",
                        Value = arrangement.ArrangementID,
                        SqlDbType = SqlDbType.VarChar,
                        Size = 50
                    };
                    command.Parameters.Add(parameter);

                    parameter = new SqlParameter
                    {
                        ParameterName = "@Code",
                        Value =arrangement.Code,
                        SqlDbType = SqlDbType.VarChar
                    };
                    command.Parameters.Add(parameter);

                    connection.Open();
                    try
                    {
                        command.ExecuteNonQuery();

                        string sCustomerSQL = $"Select ArrangementInventoryID from ArrangementInventory where ArrangementID = @ArrangementID and Code = @Code";
                        command.Parameters.Clear();
                        parameter = new SqlParameter
                        {
                            ParameterName = "@ArrangementID",
                            Value = arrangement.ArrangementID,
                            SqlDbType = SqlDbType.VarChar
                        };
                        command.Parameters.Add(parameter);
                        parameter = new SqlParameter
                        {
                            ParameterName = "@Code",
                            Value = sNextCode,
                            SqlDbType = SqlDbType.VarChar
                        };
                        command.Parameters.Add(parameter);
                        command.CommandText = sCustomerSQL;

                        using (SqlDataReader dr = command.ExecuteReader())
                        {
                            while (dr.Read())
                            {
                                sArrangementInventoryID = Convert.ToString(dr["ArrangementInventoryID"]);
                            }
                        }
                    }
                    catch (Exception ex) { }
                    finally { connection.Close(); } 
                }
            }
            return sArrangementInventoryID;
        }
        public static string CreateArrangementInventory(string connectionString, ArrangementInventory inventory)
        {
            string sArrangementInventoryID = string.Empty;
            using (SqlConnection connection = new SqlConnection(connectionString))
            {

                string sql = "Insert Into ArrangementInventory (ArrangementID, Code, LocationID) Values (@ArrangementID, @Code, @LocationID)";
                using (SqlCommand command = new SqlCommand(sql, connection))
                {

                    command.CommandType = CommandType.Text;

                    // adding parameters
                    SqlParameter parameter = new SqlParameter
                    {
                        ParameterName = "@ArrangementID",
                        Value = inventory.ArrangementID,
                        SqlDbType = SqlDbType.VarChar,
                        Size = 50
                    };
                    command.Parameters.Add(parameter);

                    parameter = new SqlParameter
                    {
                        ParameterName = "@LocationID",
                        Value = inventory.LocationID,
                        SqlDbType = SqlDbType.VarChar
                    };
                    command.Parameters.Add(parameter);
                    
                    parameter = new SqlParameter
                    {
                        ParameterName = "@Code",
                        Value = inventory.Code,
                        SqlDbType = SqlDbType.VarChar
                    };
                    command.Parameters.Add(parameter);
                    connection.Open();
                    try
                    {
                        command.ExecuteNonQuery();

                        string sCustomerSQL = $"Select ArrangementInventoryID from ArrangementInventory where ArrangementID = @ArrangementID and Code = @Code";
                        command.Parameters.Clear();
                        parameter = new SqlParameter
                        {
                            ParameterName = "@ArrangementID",
                            Value = inventory.ArrangementID,
                            SqlDbType = SqlDbType.VarChar
                        };
                        command.Parameters.Add(parameter);
                        parameter = new SqlParameter
                        {
                            ParameterName = "@Code",
                            Value = inventory.Code,
                            SqlDbType = SqlDbType.VarChar
                        };
                        command.Parameters.Add(parameter);
                        command.CommandText = sCustomerSQL;

                        using (SqlDataReader dr = command.ExecuteReader())
                        {
                            while (dr.Read())
                            {
                                sArrangementInventoryID = Convert.ToString(dr["ArrangementInventoryID"]);
                            }
                        }
                    }
                    catch (Exception ex) { }
                    finally { connection.Close(); }
                }
            }
            return sArrangementInventoryID;
        }
        //public static string CreateArrangementInventory(string connectionString, Arrangement arrangement)
        //{
        //    string sArrangementInventoryID = string.Empty;
        //    .
        //    using (SqlConnection connection = new SqlConnection(connectionString))
        //    {
        //        string maxCode = GetNextAvailableCode(connection, arrangement.Code);
        //        string sCounter = maxCode.Replace(arrangement.Code + "-", "");
        //        int iCounter = 1;
        //        int.TryParse(sCounter, out iCounter);
        //        iCounter++;
        //        string sArrangementInventoryCode = arrangement.Code + Convert.ToString(iCounter).PadLeft(2);

        //        string sql = "Insert Into ArrangementInventory (ArrangementID, Code) Values (@ArrangementID, @Code)";

        //        using (SqlCommand command = new SqlCommand(sql, connection))
        //        {
        //            command.CommandType = CommandType.Text;

        //            // adding parameters
        //            SqlParameter parameter = new SqlParameter
        //            {
        //                ParameterName = "@ArrangementID",
        //                Value = arrangement.ArrangementID,
        //                SqlDbType = SqlDbType.VarChar,
        //                Size = 50
        //            };
        //            command.Parameters.Add(parameter);

        //            parameter = new SqlParameter
        //            {
        //                ParameterName = "@Code",
        //                Value = sArrangementInventoryCode,
        //                SqlDbType = SqlDbType.VarChar
        //            };
        //            command.Parameters.Add(parameter);
        //            connection.Open();
        //            try
        //            {
        //                command.ExecuteNonQuery();

        //                string sCustomerSQL = $"Select ArrangementInventoryID from ArrangementInventory where ArrangementID = @ArrangementID and Code = @Code";
        //                command.Parameters.Clear();
        //                parameter = new SqlParameter
        //                {
        //                    ParameterName = "@ArrangementID",
        //                    Value = "",
        //                    SqlDbType = SqlDbType.VarChar
        //                };
        //                command.Parameters.Add(parameter);
        //                parameter = new SqlParameter
        //                {
        //                    ParameterName = "@Code",
        //                    Value = sArrangementInventoryCode,
        //                    SqlDbType = SqlDbType.VarChar
        //                };
        //                command.Parameters.Add(parameter);
        //                command.CommandText = sCustomerSQL;

        //                using (SqlDataReader dr = command.ExecuteReader())
        //                {
        //                    while (dr.Read())
        //                    {
        //                        sArrangementInventoryID = Convert.ToString(dr["ArrangementInventoryID"]);
        //                    }
        //                }
        //            }
        //            catch (Exception ex)
        //            { }
        //            finally
        //            { connection.Close(); }
        //        }
        //    }
        //    return sArrangementInventoryID;
        //}

        public static string GetNextInventoryCode(string connectionString, string code)

        {
            string sLastCode = string.Empty;
            string sql = $"select max(Code) LastCode from ArrangementInventory where code like @Code";
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    command.Parameters.Clear();
                    SqlParameter parameter = new SqlParameter
                    {
                        ParameterName = "@Code",
                        Value = code + "%",
                        SqlDbType = SqlDbType.VarChar
                    };
                    command.Parameters.Add(parameter);

                    using (SqlDataReader dr = command.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            sLastCode = Convert.ToString(dr["LastCode"]);
                            if (String.IsNullOrEmpty(sLastCode))
                            {
                                sLastCode = code + "-0";
                            }
                        }
                    }
                }
                connection.Close();
            }
            string sCounter = sLastCode.Replace(code + "-", "");
            int iCounter = 1;
            int.TryParse(sCounter, out iCounter);
            iCounter++;
            string sNextCode = code + "-" + Convert.ToString(iCounter).PadLeft(2, '0');
            return sNextCode;
        }

        public static List<Location> GetLocations(string connectionString)
        {
            List<Location> locations = new List<Location>();

            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    string sql = "Select LocationID, Name from Location order by Name";
                    SqlCommand cmd = new SqlCommand(sql, connection);
                    SqlDataReader reader = cmd.ExecuteReader();

                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            Location location = new Location();
                            location.LocationID = Convert.ToString(reader["LocationID"]);
                            location.Name = Convert.ToString(reader["Name"]);

                            locations.Add(location);
                        }
                    }

                    locations.Insert(0, new Location { Name = "-- Select Location--", LocationID = "0" });
                    connection.Close();
                }
            }
            catch (Exception ex) { }
            finally { }

            return locations;

        }
        public static List<LocationPlacement> GetLocationPlacementList(string? connectionString, string id)
        {
            List<LocationPlacement> ivmList = new List<LocationPlacement>();
            if ( !String.IsNullOrEmpty(id) )
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    string sql = "SELECT " +
                        " la.LocationPlacementID  ID " +
                        " ,la.Description        CODE " +
                        " FROM LocationPlacement la " +
                        $" where la.LocationID='{id}'";

                    SqlCommand readcommand = new SqlCommand(sql, connection);

                    using (SqlDataReader dr = readcommand.ExecuteReader())
                    {
                        if (dr.HasRows)
                        {
                            while (dr.Read())
                            {
                                LocationPlacement ivm = new LocationPlacement();
                                ivm.LocationPlacementID = Convert.ToString(dr["ID"]);
                                ivm.Code = Convert.ToString(dr["CODE"]);
                                ivmList.Add(ivm);
                            }
                        }
                    }
                    connection.Close();
                    ivmList.Insert(0, new LocationPlacement { Code = "-- Select Placement --", LocationPlacementID = "0" });
                }

            }
            return ivmList;
        }

        public static void UpdateLocation(string? connectionString, Location location, string LocationID)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {

               // string sql = "Insert Into Location (Name, Description, LocationTypeID) Values (@Name, @Description, @LocationTypeID)";
                string sql = $"Update Location SET Name=@Name, Description=@Description Where LocationId=@LocationID";
                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    command.CommandType = CommandType.Text;
                    if (String.IsNullOrEmpty(location.Description))
                    {
                        location.Description = "";
                    }
                    // adding parameters
                    SqlParameter parameter = new SqlParameter
                    {
                        ParameterName = "@Name",
                        Value = location.Name,
                        SqlDbType = SqlDbType.VarChar,
                        Size = 50
                    };
                    command.Parameters.Add(parameter);

                    parameter = new SqlParameter
                    {
                        ParameterName = "@Description",
                        Value = location.Description,
                        SqlDbType = SqlDbType.VarChar
                    };
                    command.Parameters.Add(parameter);

                    parameter = new SqlParameter
                    {
                        ParameterName = "@LocationID",
                        Value = location.LocationID,
                        SqlDbType = SqlDbType.VarChar
                    };
                    command.Parameters.Add(parameter);

                    connection.Open();
                    command.ExecuteNonQuery();
                    connection.Close();
                }
            }
        }
    }
}
