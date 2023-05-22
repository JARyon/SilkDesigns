using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.CodeAnalysis.FindSymbols;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.View;
//using Microsoft.Data.SqlClient;
using SilkDesign.Models;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Reflection.Metadata;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace SilkDesign.Shared
{
    public static class Dispositions
    {
        public static readonly string TransferTo = "To Truck";
        public static readonly string TransferFrom = "From Truck";
        public static readonly string FromWarehouse = "Warehouse";
        public static readonly string ToWareHouse = "Warehouse";
    }
    public static class SilkDesignUtility
    {

        const string sCustomerType = "Customer";
        public static List<Arrangement> GetArrangements(string connectionString, string sArrangementID, string sUserID, ref string sErrorMsg)
        {
            List<Arrangement> list = new List<Arrangement>();
            Arrangement arrangement = new Arrangement();
            List<SelectListItem> SizeList = SilkDesignUtility.GetSizes(connectionString);
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string sql = $" Select * " +
                             $" From Arrangement " +
                             $" Where ArrangementID= @ArrangementID " +
                             $" AND UserID = @UserID" +
                             $" AND Deleted = 'N' ";

                SqlCommand command = new SqlCommand(sql, connection);
                SqlParameter parameter = new SqlParameter
                {
                    ParameterName ="@ArrangementID",
                    Value = sArrangementID,
                    SqlDbType = SqlDbType.VarChar
                };
                command.Parameters.Add(parameter);

                parameter = new SqlParameter
                {
                    ParameterName = "@UserID",
                    Value = sArrangementID,
                    SqlDbType = SqlDbType.VarChar
                };
                command.Parameters.Add(parameter);

                connection.Open();

                try
                {
                    using (SqlDataReader dataReader = command.ExecuteReader())
                    {
                        while (dataReader.Read())
                        {
                            arrangement.ArrangementID = Convert.ToString(dataReader["ArrangementID"]);
                            arrangement.Code = Convert.ToString(dataReader["Code"]);
                            arrangement.Name = Convert.ToString(dataReader["Name"]);
                            arrangement.Description = Convert.ToString(dataReader["Description"]);
                            arrangement.Price = Convert.ToDecimal(dataReader["Price"]);
                            arrangement.Quantity  = Convert.ToInt32(dataReader["Quantity"]);
                            arrangement.LastViewed = Convert.ToDateTime(dataReader["LastViewed"]);
                            arrangement.SizeID = Convert.ToString(dataReader["SizeID"]);
                            arrangement.Sizes = SizeList;

                            list.Add(arrangement);
                        }
                    }
                }
                catch (Exception ex)
                {
                    sErrorMsg = "Unable to read arrangment. " + ex.Message;
                }

                connection.Close();
            }

            return list;
        }
        public static ArrangementInventory GetArrangementInventory(string connectionString, string sArrangementInventoryID, string sUserID, ref string sErrorMsg)
        {
            ArrangementInventory arrangementInventory = new ArrangementInventory();
            List<SelectListItem> StatusList = SilkDesignUtility.GetInventoryStatus(connectionString);
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                string sLocationNameSQL = $"SELECT ai.Code, " +
                                          $" ai.LocationID, " +
                                          $" ai.LocationPlacementID," +
                                          $" ai.InventoryStatusID, " +
                                          $"  a.SizeID," +
                                          $"  a.ArrangementID " +
                                          $" FROM ARRANGEMENTINVENTORY ai " +
                                          $" join arrangement a on ai.ArrangementID = a.ArrangementID " +
                                          $" where ArrangementInventoryID = @ArrangementInventoryID " +
                                          $" and ai.UserID = @UserID " +
                                          $" and ai.Deleted = 'N' ";
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

                    parameter = new SqlParameter
                    {
                        ParameterName = "@UserID",
                        Value = sUserID,
                        SqlDbType = SqlDbType.VarChar
                    };
                    command.Parameters.Add(parameter);

                    try
                    {
                        using (SqlDataReader dr = command.ExecuteReader())
                        {
                            while (dr.Read())
                            {
                                arrangementInventory.ArrangementInventoryID = sArrangementInventoryID;
                                arrangementInventory.Code = Convert.ToString(dr["Code"]);
                                arrangementInventory.LocationID = Convert.ToString(dr["LocationID"]);
                                arrangementInventory.ArrangementID = Convert.ToString(dr["ArrangementID"]);
                                arrangementInventory.SizeID = Convert.ToString(dr["SizeID"]);
                                arrangementInventory.InventoryStatusID = Convert.ToString(dr["InventoryStatusID"]);
                                arrangementInventory.LocationPlacementID = Convert.ToString(dr["LocationPlacementID"]);
                                if (String.IsNullOrEmpty(arrangementInventory.LocationID))
                                {
                                    arrangementInventory.LocationID = "0";
                                }
                                arrangementInventory.Locations = GetLocationDDL(connectionString, sUserID, ref sErrorMsg);
                                arrangementInventory.StatusList = StatusList;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        sErrorMsg = "Error getting arrangment inventory. " + ex.Message;
                    }
                }
                connection.Close();
            }

            return arrangementInventory;
        }
        public static Catalog GetCatalog(string connectionString, string sCatalogID, ref string sErrorMsg)
        {
            Catalog catalog = new Catalog();
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                string sArrangementSQL = $" Select * " +
                                          $" from Catalog " +
                                          $" where CatalogID = @CatalogID" +
                                          $" and Deleted = 'N' ";

                using (SqlCommand command = new SqlCommand(sArrangementSQL, connection))
                {
                    command.Parameters.Clear();

                    // adding parameters
                    SqlParameter parameter = new SqlParameter
                    {
                        ParameterName = "@CatalogID",
                        Value = sCatalogID,
                        SqlDbType = SqlDbType.VarChar
                    };
                    command.Parameters.Add(parameter);

                    using (SqlDataReader dr = command.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            catalog.CatalogID = sCatalogID;
                            catalog.Name = Convert.ToString(dr["Name"]);
                            catalog.Code = Convert.ToString(dr["Code"]);
                            catalog.Description = Convert.ToString(dr["Description"]);
                            catalog.SizeID = Convert.ToString(dr["SizeID"]);
                            catalog.CatalogStatusID = Convert.ToString(dr["CatalogStatusID"]);
                        }
                    }
                }
                connection.Close();
            }
            return catalog;
        }

        public static Arrangement GetArrangement(string connectionString, string sArrangementID, string sUserID, ref string sErrorMsg)
        {
            Arrangement arrangement = new Arrangement();
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                string sArrangementSQL = $" Select a.Code, " +
                                         $" a.Name, " +
                                         $" a.Description, " +
                                         $" a.Price, " +
                                         $" a.Quantity, " +
                                         $" a.LastViewed, " +
                                         $" a.SizeID, " +
                                         $" s.Code  SizeCode" +
                                         $" from Arrangement a " +
                                         $" join Size s on s.SizeID = a.SizeID" +
                                         $" where ArrangementID = @ArrangementID " +
                                         $" and UserID = @UserID ";

                using (SqlCommand command = new SqlCommand(sArrangementSQL, connection))
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

                    parameter = new SqlParameter
                    {
                        ParameterName = "@UserID",
                        Value = sUserID,
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
                            arrangement.SizeCode = Convert.ToString(dr["SizeCode"]);
                            arrangement.UserID = sUserID;
                        }
                    }
                }
                connection.Close();
            }
            return arrangement;
        }
        private static IEnumerable<SelectListItem> GetAvailableArrangements(string connectionString, string sSizeID, string OutgoingArrangmentName, string OutgoingInventoryCode, string OutgoingArrangementInventoryID, string sUserID)
        {
            List<SelectListItem> list = new List<SelectListItem>();
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    string sql = $" select" +
                                 $"   a.Name + ' | ' + ai.Code    DisplayName, " +
                                 $"   ai.ArrangementInventoryID   ArrangmentInventoryID " +
                                 $" from ArrangementInventory ai " +
                                 $" join Arrangement a on a.ArrangementID = ai.ArrangementID " +
                                 $" where ai.InventoryStatusID =  (Select InventoryStatusID " +
                                 $"                                  from inventoryStatus " +
                                 $"                                 where Description = 'Available') " +
                                 $" and a.SizeID = @SizeID " +
                                 $" and ai.Deleted = 'N' " +
                                 $" and a.UserID = @UserID " +
                                 $" and ai.UserID = @UserID "+
                                 $" order by DisplayName ";
                    SqlCommand command = new SqlCommand(sql, connection);
                    command.Parameters.Clear();

                    // adding parameters
                    SqlParameter parameter = new SqlParameter
                    {
                        ParameterName = "@SizeID",
                        Value = sSizeID,
                        SqlDbType = SqlDbType.VarChar
                    };
                    command.Parameters.Add(parameter);

                    parameter = new SqlParameter
                    {
                        ParameterName = "@UserD",
                        Value = sUserID,
                        SqlDbType = SqlDbType.VarChar
                    };
                    command.Parameters.Add(parameter);
                    
                    SqlDataReader reader = command.ExecuteReader();

                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            list.Add(new SelectListItem { Text = reader["DisplayName"].ToString(), Value = reader["ArrangmentInventoryID"].ToString() });
                        }
                    }
                    else
                    {
                        list.Add(new SelectListItem { Text = "No Arrangments found", Value = "0" });
                    }
                    list.Insert(0, new SelectListItem { Text = OutgoingArrangmentName + " | " + OutgoingInventoryCode, Value = OutgoingArrangementInventoryID });
                    list.Insert(0, new SelectListItem { Text = "-- Select Arrangement --", Value = "0" });
                    connection.Close();
                }
            }
            catch (Exception ex)
            {
                list.Add(new SelectListItem { Text = ex.Message.ToString(), Value = "0" });
            }

            return list;
        }
        public static IEnumerable<SelectListItem> GetArrangements(string connectionString, string sUserID, ref string sErrorMsg)
        {
            List<SelectListItem> list = new List<SelectListItem>();
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    string sql = $" select" +
                                 $"   a.Code + ' | ' + a.Name    DisplayName, " +
                                 $"   a.ArrangementID             ID" +
                                 $" from Arrangement a " +
                                 $" where UserID = @UserID " +
                                 $" order by DisplayName ";
                    SqlCommand command = new SqlCommand(sql, connection);
                    command.Parameters.Clear();
                    SqlParameter parameter = new SqlParameter
                    {
                        ParameterName = "@UserID",
                        Value = sUserID,
                        SqlDbType = SqlDbType.VarChar
                    };
                    command.Parameters.Add(parameter);

                    SqlDataReader reader = command.ExecuteReader();

                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            list.Add(new SelectListItem { Text = reader["DisplayName"].ToString(), Value = reader["ID"].ToString() });
                        }
                    }
                    else
                    {
                        list.Add(new SelectListItem { Text = "No Arrangments found", Value = "" });
                    }
                    list.Insert(0, new SelectListItem { Text = "-- Select Arrangement --", Value = "" });
                    connection.Close();
                }
            }
            catch (Exception ex)
            {
                list.Add(new SelectListItem { Text = ex.Message.ToString(), Value = "0" });
                sErrorMsg = "Can not get list of arrangements. " + ex.Message;
            }

            return list;
        }


        public static List<ArrangementInventory> GetArrangementInventories(string connectionString, string sArrangementID, string sUserID, ref string sErrorMsg)
        {
            List<ArrangementInventory> arrangementInventories = new List<ArrangementInventory>();
            ArrangementInventory arrangementInvetory = new ArrangementInventory();

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                string sArrangmentInventorySQL = $" Select ai.Code              Code, " +
                                          $" ai.ArrangementInventoryID   ID, " +
                                          $" ai.LocationPlacementID      PlacementID, " +
                                          $" p.Description               Placement, " +
                                          $" isNull(Convert(Varchar(20), lastUsed, 101), '') LastUsed, " +
                                          $" l.Name                      Location, " +
                                          $" s.Code                      StatusCode " +
                                          $" from ArrangementInventory ai" +
                                          $" left outer join Location l on ai.LocationID = l.LocationID and l.UserID = @UserID" +
                                          $" left outer join LocationPlacement P on ai.LocationPlacementID = P.LocationPlacementID  and p.UserID = @UserID" +
                                          $" left outer join InventoryStatus s on ai.InventoryStatusID = s.InventoryStatusID " +
                                          $" where ai.ArrangementID = @ArrangementID " +
                                          $" and ai.deleted = 'N' " +
                                          $" and ai.UserID = @UserID " +
                                          $" Order by ai.Code ";

                using (SqlCommand command = new SqlCommand(sArrangmentInventorySQL, connection))
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

                    parameter = new SqlParameter
                    {
                        ParameterName = "@UserID",
                        Value = sUserID,
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
                            arrangementInventory.InventoryStatusCode = Convert.ToString(dr["StatusCode"]);
                            arrangementInventories.Add(arrangementInventory);

                        }
                    }
                }
                connection.Close();
            }
            return arrangementInventories;

        }
        public static List<Location> GetLocation(string? connectionString, string sLocationID, string sUserID, ref string sErrorMsg)
        {
            List<Location> list = new List<Location>();
            Models.Location location = new Models.Location();
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string sql = $" Select * From Location " +
                             $" Where LocationId = @LocationID " +
                             $" AND UserID = @UserID";

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
                    ParameterName = "@LocationID",
                    Value = sLocationID,
                    SqlDbType = SqlDbType.VarChar
                };
                command.Parameters.Add(parameter);
                connection.Open();

                try
                {
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
                }
                catch (Exception ex)
                {
                    sErrorMsg = "Unable to get locations." + ex.Message;
                }

                connection.Close();
            }
            return list;
        }
        public static IEnumerable<SelectListItem> GetLocationDDL(string connectionString, string sUserID, ref string  sErrorMsg)
        {
            List<SelectListItem> list = new List<SelectListItem>();
    
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                string sql = "Select * from Location where UserID = @UserID order by Name";
                SqlCommand cmd = new SqlCommand(sql, connection);
                SqlParameter parameter = new SqlParameter
                {
                    ParameterName = "@UserID",
                    Value = sUserID,
                    SqlDbType = SqlDbType.VarChar
                };
                cmd.Parameters.Add(parameter);

                SqlDataReader reader = cmd.ExecuteReader();

                try
                {
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
                }
                catch (Exception ex)
                {
                    sErrorMsg = "Error loading location drop down." + ex.Message;
                }
                list.Insert(0, new SelectListItem { Text = "-- Select Location --", Value = "0" });
                connection.Close();
            }
    
            return list;
        }


        public static List<RoutePlan> GetRoutePlans(string connectionString, string sRoutePlanID, string sUserID, ref string sErrorMsg)
        {
            List<RoutePlan> routePlanList = new List<RoutePlan>();
            //string connectionString = Configuration["ConnectionStrings:SilkDesigns"];
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                string sql = "SELECT " +
                    "   rp.RoutePlanID  RoutePlanID " +
                    " ,  r.RouteID      RouteID " +
                    " ,  r.Name         Route " +
                    " , rp.Description  Description " +
                    " , rp.RouteDate    Date " +
                    " , rps.Code        Status " +
                    " from RoutePlan rp " +
                    " join Route r on r.RouteID = rp.RouteID and r.UserID = @UserID " +
                    " join RoutePlanStatus rps on rps.RoutePlanStatusID = rp.RoutePlanStatusID " +
                    " Where rp.RoutePlanID =  @RoutePlanID " +
                    " and rp.UserID = @UserID " +
                    " AND r.Deleted = 'N' " +
                    " AND rp.Deleted = 'N' " +
                    " Order by rp.RouteDate ";

                SqlCommand readcommand = new SqlCommand(sql, connection);
                SqlParameter parameter = new SqlParameter
                {
                    ParameterName = "@UserID",
                    Value = sUserID,
                    SqlDbType = SqlDbType.VarChar
                };
                readcommand.Parameters.Add(parameter);
                parameter = new SqlParameter
                {
                    ParameterName = "@RoutePlanID",
                    Value = sRoutePlanID,
                    SqlDbType = SqlDbType.VarChar
                };
                readcommand.Parameters.Add(parameter);

                using (SqlDataReader dr = readcommand.ExecuteReader())
                {
                    while (dr.Read())
                    {
                        RoutePlan routePlan = new RoutePlan();
                        routePlan.RouteID = Convert.ToString(dr["RouteID"]);
                        routePlan.RoutePlanID = Convert.ToString(dr["RoutePlanID"]);
                        routePlan.RouteName = Convert.ToString(dr["Route"]);
                        routePlan.Description = Convert.ToString(dr["Description"]);
                        routePlan.RouteDate = Convert.ToDateTime(dr["Date"]);
                        routePlan.RoutePlanStatusCode = Convert.ToString(dr["Status"]);
                        routePlan.UserID = sUserID;
                        routePlanList.Add(routePlan);
                    }
                }
                connection.Close();
            }
            return routePlanList;
        }
        public static RoutePlanStop GetRoutePlanDetail(string? connectionString, string sRoutePlanDetailInventoryID, string sUserID, ref string sErrorMsg)
        {
            RoutePlanStop rtPlanStop = new RoutePlanStop();

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                string sql = $"Select " +
                    $" rpd.routePlanDetailID        PlanDetailID, " +
                    $" l.Name                       LocName, " +
                    $" lp.Description               Placement, " +
                    $" rpd.RouteOrder               RouteOrder, " +
                    $" S.Code                       SizeCode, " +
                    $" S.SizeID                     SizeID, " +
                    $" Inai.Code                    InInvCode," +
                    $" Ina.Name                     IncomingArrangement, " +
                    $" Inai.ArrangementInventoryID  IncomingArrangementInventoryID, " +
                    $" Outai.Code                   OutInvCode, " +
                    $" Outa.Name                    OutgoingArrangment, " +
                    $" Outai.ArrangementInventoryID OutGoingArrangementID " +
                    $" from routePlanDetail rpd  " +
                    $" join routePlanDetailInventory rpdi on rpdi.RoutePlanDetailID = rpd.RoutePlanDetailID" +
                    $" join Location l on rpd.LocationID = l.LocationID and l.UserID = @UserID " +
                    $" join locationPlacement lp on lp.locationID = l.locationID and lp.LocationPlacementID = rpd.LocationPlacementID and lp.UserID = @UserID " +
                    $" join Size s on s.SizeID = lp.SizeID " +
                    $" left outer join ArrangementInventory Outai on Outai.ArrangementInventoryID = rpdi.OutgoingArrangementInventoryID and outai.Deleted = 'N' and outai.UserID = @UserID" +
                    $" left outer join Arrangement Outa on Outa.ArrangementID = Outai.ArrangementID and Outa.UserID = @UserID " +
                    $" left outer join ArrangementInventory Inai on Inai.ArrangementInventoryID = rpdi.IncomingArrangementInventoryID and inai.Deleted = 'N' and inai.UserID = @UserID " +
                    $" left outer join Arrangement Ina on Ina.ArrangementID = Inai.ArrangementID and Ina.UserID = @UserID " +
                    $" where rpdi.routePlanDetailInventoryID = @RoutePlanDetailInventoryID " +
                    $" and rpd.UserID = @UserID " +
                    $" order by rpd.RouteOrder, Placement, InInvCode";

                SqlCommand command = new SqlCommand(sql, connection);
                command.Parameters.Clear();

                // adding parameters
                SqlParameter parameter = new SqlParameter
                {
                    ParameterName = "@RoutePlanDetailInventoryID",
                    Value = sRoutePlanDetailInventoryID,
                    SqlDbType = SqlDbType.VarChar
                };
                command.Parameters.Add(parameter);
                parameter = new SqlParameter
                {
                    ParameterName = "@UserID",
                    Value = sUserID,
                    SqlDbType = SqlDbType.VarChar
                };
                command.Parameters.Add(parameter);
                using (SqlDataReader dr = command.ExecuteReader())
                {
                    if (dr.HasRows)
                    {
                        while (dr.Read())
                        {
                            rtPlanStop.RoutePlanDetailID = Convert.ToString(dr["PlanDetailID"]);
                            rtPlanStop.LocationName = Convert.ToString(dr["LocName"]);
                            rtPlanStop.PlacmentName = Convert.ToString(dr["Placement"]);
                            rtPlanStop.RouteOrder = Convert.ToInt32(dr["RouteOrder"]);
                            rtPlanStop.SizeID = Convert.ToString(dr["SizeID"]);
                            rtPlanStop.SizeDesc = Convert.ToString(dr["SizeCode"]);
                            rtPlanStop.IncomingArrangmentName = Convert.ToString(dr["IncomingArrangement"]);
                            rtPlanStop.IncomingInventoryCode = Convert.ToString(dr["InInvCode"]);
                            rtPlanStop.IncomingArrangementInventoryID = Convert.ToString(dr["IncomingArrangementInventoryID"]);
                            rtPlanStop.OutgoingArrangmentName = Convert.ToString(dr["OutgoingArrangment"]);
                            rtPlanStop.OutgoingArrangementInventoryID = Convert.ToString(dr["OutGoingArrangementID"]);
                            rtPlanStop.OutgoingInventoryCode = Convert.ToString(dr["OutInvCode"]);
                            rtPlanStop.AvailableArrangements = GetAvailableArrangements(connectionString, rtPlanStop.SizeID, rtPlanStop.OutgoingArrangmentName, rtPlanStop.OutgoingInventoryCode, rtPlanStop.OutgoingArrangementInventoryID, sUserID);
                            rtPlanStop.HasData = true;
                        }
                    }
                }
                connection.Close();
            }
            return rtPlanStop;
        }
        public static string GetSuggestedInventoryID(string connectionString, string sRoutePlanDetailID, string sUserID)
        {
            string sRetValue = string.Empty;

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                string sCustomerTypeSQL = $" select IncomingArrangementInventoryID SuggestedID " +
                                          $" from RoutePlanDetail " +
                                          $" where RoutePlanDetailID = @RoutePlanDetailID " +
                                          $" and UserID = @UserID ";
                using (SqlCommand command = new SqlCommand(sCustomerTypeSQL, connection))
                {
                    command.Parameters.Clear();
                    command.CommandText = sCustomerTypeSQL;

                    SqlParameter parameter = new SqlParameter
                    {
                        ParameterName = "@RoutePlanDetailID",
                        Value = sRoutePlanDetailID,
                        SqlDbType = SqlDbType.VarChar
                    };
                    command.Parameters.Add(parameter);

                    parameter = new SqlParameter
                    {
                        ParameterName = "@UserID",
                        Value = sUserID,
                        SqlDbType = SqlDbType.VarChar
                    };
                    command.Parameters.Add(parameter);

                    using (SqlDataReader dr = command.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            sRetValue = Convert.ToString(dr["SuggestedID"]);
                        }
                    }
                }
            }
            return sRetValue;
        }

        public static List<RoutePlanDetail> GetRoutePlanStops(string connectionString, string sRoutePlanID, string sUserID, ref string sErrorMsg)
        {
            List<RoutePlanDetail> routePlanDetailList = new List<RoutePlanDetail>();
            string sRetValue = string.Empty;

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                string sCustomerNameSQL = $"Select " +
                    $" rpd.routePlanDetailID     PlanDetailID, " +
                    $" l.Name                    LocName, " +
                    $" l.LocationID              LocationID, " +
                    $" lp.Description            Placement, " +
                    $" rpd.RouteOrder            RouteOrder, " +
                    $" rpd.Quantity              Quantity, " +
                    $" rpd.LocationPlacementID   LocationPlacementID, " +
                    $" r.WarehouseID             WarehouseID, " +
                    $" S.Code                    SizeCode, " +
                    $" S.SizeID                  SizeID,  " +
                    $" rpd.OutgoingArrangementID OutgoingArrangementID " +
                    $" from routePlanDetail rpd " +
                    $" join routePlan rp on rp.RoutePlanID = rpd.RoutePlanID and rp.UserID = @UserID " +
                    $" join route r on r.routeID = rp.routeID and r.UserID = @UserID " +
                    $" join Location l on rpd.LocationID = l.LocationID and l.UserID = @UserID  " +
                    $" join locationPlacement lp on lp.locationID = l.locationID and lp.LocationPlacementID = rpd.LocationPlacementID and lp.UserID = @UserID  " +
                    $" join Size s on s.SizeID = lp.SizeID " +
                    $" where rpd.routePlanID = @RoutePlanID " +
                    $"and rpd.UserID = @UserID " +
                    $" order by rpd.RouteOrder, Placement, SizeCode desc";
                using (SqlCommand command = new SqlCommand(sCustomerNameSQL, connection))
                {
                    command.Parameters.Clear();

                    // adding parameters
                    SqlParameter parameter = new SqlParameter
                    {
                        ParameterName = "@RoutePlanID",
                        Value = sRoutePlanID,
                        SqlDbType = SqlDbType.VarChar
                    };
                    command.Parameters.Add(parameter);
                    parameter = new SqlParameter
                    {
                        ParameterName = "@UserID",
                        Value = sUserID,
                        SqlDbType = SqlDbType.VarChar
                    };
                    command.Parameters.Add(parameter);
                    //command.CommandText = sCustomerTypeSQL;


                    using (SqlDataReader dr = command.ExecuteReader())
                    {
                        try
                        {
                            while (dr.Read())
                            {
                                RoutePlanDetail stop = new RoutePlanDetail();
                                stop.RoutePlanDetailID = Convert.ToString(dr["PlanDetailID"]);
                                stop.RoutePlanID = sRoutePlanID;
                                stop.LocationName = Convert.ToString(dr["LocName"]);
                                stop.LocationID = Convert.ToString(dr["LocationID"]);
                                stop.Quantity = Convert.ToInt32(dr["Quantity"]);
                                stop.PlacmentDescription = Convert.ToString(dr["Placement"]);
                                stop.RouteOrder = Convert.ToInt32(dr["RouteOrder"]);
                                stop.SizeCode = Convert.ToString(dr["SizeCode"]);
                                stop.SizeID = Convert.ToString(dr["SizeID"]);
                                stop.OutgoingArrangementID = Convert.ToString(dr["OutgoingArrangementID"]);
                                stop.LocationPlacementID = Convert.ToString(dr["LocationPlacementID"]);
                                stop.WarehouseID = Convert.ToString(dr["WarehouseID"]);
                                routePlanDetailList.Add(stop);
                            }
                        }
                        catch (Exception ex)
                        {
                            sErrorMsg = "Failure reading route plan stops. " + ex.Message;
                        }
                    }
                }
                if (connection.State == ConnectionState.Open)
                    connection.Close(); 
            }
            return routePlanDetailList;
        }

        public static List<RoutePlanDetail> GetRoutePlanDetails(string connectionString, string sRoutePlanID, string sUserID, ref string sErrorMsg)
        {
            List<RoutePlanDetail> routePlanDetailList = new List<RoutePlanDetail>();
            string sRetValue = string.Empty;

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                string sCustomerNameSQL = $"Select " +
                    $" rpd.routePlanDetailID    PlanDetailID, " +
                    $" l.Name                   LocName, " +
                    $" l.LocationID             LocationID, " +
                    $" lp.Description           Placement, " +
                    $" rpd.RouteOrder           RouteOrder, " +
                    $" rpd.Quantity             Quantity, " +
                    $" rpdi.RoutePlanDetailInventoryID InventoryID, " +
                    $" rpdi.OutgoingDisposition  OutgoingDisposition, " +
                    $" rpdi.IncomingDisposition  IncomingDisposition, " +
                    $" rpd.LocationPlacementID  LocationPlacementID, " +
                    $" r.WarehouseID            WarehouseID, " +
                    $" S.Code                   SizeCode, " +
                    $" S.SizeID                 SizeID,  " +
                    $" iai.ArrangementInventoryID IncomingArrangementInventoryID, " +
                    $" Ina.ArrangementID        IncomingArrangementID, " +
                    $" Ina.Name                 IncomingArrangement, " +
                    $" iai.Code                 IncomingCode, " +
                    $" oai.Code                 OutgoingCode, " +
                    $" Outa.Name                OutgoingArrangement, " +
                    $" Outa.ArrangementID       OutgoingArrangementID, " +
                    $" oai.ArrangementInventoryID OutgoingArrangementInventoryID " +
                    $" from routePlanDetail rpd " +
                    $" join routePlan rp on rp.RoutePlanID = rpd.RoutePlanID and rp.UserID = @UserID " +
                    $" join routePlanDetailInventory rpdi on rpdi.RoutePlanDetailID = rpd.RoutePlanDetailID " +
                    $" join route r on r.routeID = rp.routeID" +
                    $" join Location l on rpd.LocationID = l.LocationID " +
                    $" join locationPlacement lp on lp.locationID = l.locationID and lp.LocationPlacementID = rpd.LocationPlacementID " +
                    $" join Size s on s.SizeID = lp.SizeID " +
                    $" left outer join ArrangementInventory oai on oai.ArrangementInventoryID = rpdi.OutgoingArrangementInventoryID " +
                    $" left outer join ArrangementInventory iai on iai.ArrangementInventoryID = rpdi.IncomingArrangementInventoryID " +
                    $" left outer join Arrangement Outa on Outa.ArrangementID = oai.ArrangementID and oai.Deleted = 'N' " +
                    $" left outer join Arrangement Ina on  Ina.ArrangementID = iai.ArrangementID  and iai.Deleted = 'N' " +
                    $" where rpd.routePlanID = @RoutePlanID " +
                    $" and rpd.UserID = @UserID " +
                    $" order by rpd.RouteOrder, Placement, SizeCode desc";
                using (SqlCommand command = new SqlCommand(sCustomerNameSQL, connection))
                {
                    command.Parameters.Clear();

                    // adding parameters
                    SqlParameter parameter = new SqlParameter
                    {
                        ParameterName = "@RoutePlanID",
                        Value = sRoutePlanID,
                        SqlDbType = SqlDbType.VarChar
                    };
                    command.Parameters.Add(parameter);
                    parameter = new SqlParameter
                    {
                        ParameterName = "@UserID",
                        Value = sUserID,
                        SqlDbType = SqlDbType.VarChar
                    };
                    command.Parameters.Add(parameter);
                    //command.CommandText = sCustomerTypeSQL;

                    using (SqlDataReader dr = command.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            RoutePlanDetail stop = new RoutePlanDetail();
                            stop.RoutePlanDetailID = Convert.ToString(dr["PlanDetailID"]);
                            stop.RoutePlanID = sRoutePlanID;
                            stop.LocationName = Convert.ToString(dr["LocName"]);
                            stop.LocationID = Convert.ToString(dr["LocationID"]);
                            stop.Quantity = Convert.ToInt32(dr["Quantity"]);
                            stop.PlacmentDescription = Convert.ToString(dr["Placement"]);
                            stop.RouteOrder = Convert.ToInt32(dr["RouteOrder"]);
                            stop.RoutePlanDetailInventoryID = Convert.ToString(dr["InventoryID"]);
                            stop.OutgoingDisposition = Convert.ToString(dr["OutgoingDisposition"]);
                            stop.IncomingDisposition = Convert.ToString(dr["IncomingDisposition"]);
                            stop.SizeCode = Convert.ToString(dr["SizeCode"]);
                            stop.SizeID = Convert.ToString(dr["SizeID"]);
                            stop.IncomingArrangmentInventoryID = Convert.ToString(dr["IncomingArrangementInventoryID"]);
                            stop.IncomingArrangementID = Convert.ToString(dr["IncomingArrangementID"]);
                            stop.IncomingArrangementName = Convert.ToString(dr["IncomingArrangement"]);
                            stop.IncomingArrangmentInventoryCode = Convert.ToString(dr["IncomingCode"]);
                            stop.OutgoingArrangmentInventoryCode = Convert.ToString(dr["OutgoingCode"]);
                            stop.OutgoingArrangementID = Convert.ToString(dr["OutgoingArrangementID"]);
                            stop.OutgoingArrangementName = Convert.ToString(dr["OutgoingArrangement"]);
                            stop.OutgoingArrangementInventoryID = Convert.ToString(dr["OutgoingArrangementInventoryID"]);
                            stop.LocationPlacementID = Convert.ToString(dr["LocationPlacementID"]);
                            stop.WarehouseID = Convert.ToString(dr["WarehouseID"]);
                            routePlanDetailList.Add(stop);
                        }
                    }
                }
                connection.Close();
            }
            return routePlanDetailList;
        }

        private static void GetCodes(string connectionString, string sRoutePlanDetailID, ref string Incoming, ref string OutGoing)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                string sSQL = $" Select IncomingArrangementInventoryID," +
                                          $"        ai.Code IncomingCode, " +
                                          $"        outGoingArrangementInventoryID, " +
                                          $"        ai2.Code OutgoingCode " +
                                          $" from routePlanDetailInventory rpdi" +
                                          $" left outer join ArrangementInventory ai on ai.ArrangementInventoryID = rpdi.IncomingArrangementInventoryID " +
                                          $" left outer join ArrangementInventory ai2 on ai2.ArrangementInventoryID = rpdi.OutgoingArrangementInventoryID" +
                                          $" where routePlanDetailID = @routePlanDetailID" +
                                          $" and ai.Deleted = 'N' " +
                                          $" and ai2.Deleted = 'N' ";
                using (SqlCommand command = new SqlCommand(sSQL, connection))
                {
                    command.Parameters.Clear();

                    // adding parameters
                    SqlParameter parameter = new SqlParameter
                    {
                        ParameterName = "@routePlanDetailID",
                        Value = sRoutePlanDetailID,
                        SqlDbType = SqlDbType.VarChar
                    };
                    command.Parameters.Add(parameter);

                    using (SqlDataReader dr = command.ExecuteReader())
                    {
                        bool bFirst = true;
                        string sDelimiter = "";
                        while (dr.Read())
                        {
                            if (!bFirst)
                            {
                                sDelimiter = "|";
                            }
                            Incoming += sDelimiter + Convert.ToString(dr["IncomingCode"]);
                            OutGoing += sDelimiter + Convert.ToString(dr["OutgoingCode"]);
                            bFirst = false;
                        }
                    }
                }
                connection.Close();
            }
            return;
        }

        public static string CancelPlan(string connectionString, string sRoutePlanID, string sUserID, ref string sErrorMsg)
        {
            string sReturnValue = "Success";

            List<RoutePlanDetail> lRoutePlanDetails = GetRoutePlanDetails(connectionString, sRoutePlanID, sUserID, ref sErrorMsg);
            foreach (RoutePlanDetail oRoutePlanDetail in lRoutePlanDetails)
            {
                if (oRoutePlanDetail.IncomingDisposition.Trim() == "Warehouse")
                {
                    string sArrangmentInventoryID = oRoutePlanDetail.IncomingArrangmentInventoryID;
                    sReturnValue = SetArrangmentInventoryStatus(connectionString, sArrangmentInventoryID, "Available", sUserID, ref sErrorMsg);
                    if (!String.IsNullOrEmpty(sReturnValue))
                    {
                        return sReturnValue;
                    }
                }
            }
            sReturnValue = SetPlanStatus(connectionString, sRoutePlanID, sUserID, "Cancelled");
            
            return sReturnValue;
        }
        public static string DeactivateInventory(string connectionString, string sArrangmentInventoryID, string sUserID, ref string sErrorMsg)
        {
            string sReturnValue = "Success";
            string sStatusCode = "InActive";
            sReturnValue = SetArrangmentInventoryStatus(connectionString, sArrangmentInventoryID, sStatusCode, sUserID, ref sErrorMsg);
            return sReturnValue;
        }
        public static string DeactivateRoute(string connectionString, string sRouteID, string sUserID, ref string sErrorMsg)
        {
            string sReturnVal = "Success";

            string sRetValue = string.Empty;
            string sStatusID = string.Empty;

            string sql = $" Update Route " +
                         $" Set Deleted =  'Y' " +
                         $" where RouteID = @RouteID  " +
                         $" and UserID = @UserID " ;

            SqlConnection connection = new SqlConnection(connectionString);
            using (SqlCommand command = new SqlCommand(sql, connection))
            {
                command.CommandType = CommandType.Text;
                // adding parameters
                SqlParameter parameter = new SqlParameter
                {
                    ParameterName = "@RouteID",
                    Value = sRouteID.Trim(),
                    SqlDbType = SqlDbType.VarChar
                };
                command.Parameters.Add(parameter);

                parameter = new SqlParameter
                {
                    ParameterName = "@UserID",
                    Value = sUserID,
                    SqlDbType = SqlDbType.VarChar
                };
                command.Parameters.Add(parameter);
                try
                {
                    connection.Open();
                    command.ExecuteNonQuery();
                    connection.Close();
                }
                catch (Exception ex)
                {
                    sRetValue = sErrorMsg = ex.Message;
                }

            }

            return sReturnVal;
        }
        public static void DeactivateCustomer(string connectionString, string sCustomerID, string sUserID, ref string sErrorMsg)
        {
            string sRetValue = string.Empty;
            string sStatusID = string.Empty;

            string sql = $" Update Customer " +
                         $" Set Deleted =  'Y' " +
                         $" where CustomerID = @CustomerID  " +
                         $" and UserID = @UserID";

            SqlConnection connection = new SqlConnection(connectionString);
            using (SqlCommand command = new SqlCommand(sql, connection))
            {
                command.CommandType = CommandType.Text;
                // adding parameters
                SqlParameter parameter = new SqlParameter
                {
                    ParameterName = "@CustomerID",
                    Value = sCustomerID.Trim(),
                    SqlDbType = SqlDbType.VarChar
                };
                command.Parameters.Add(parameter);
                parameter = new SqlParameter
                {
                    ParameterName = "@UserID",
                    Value = sUserID,
                    SqlDbType = SqlDbType.VarChar
                };
                command.Parameters.Add(parameter);
                try
                {
                    connection.Open();
                    command.ExecuteNonQuery();
                    connection.Close();
                }
                catch (Exception ex)
                {
                    sErrorMsg = ex.Message;
                }

            }

            string sLocationSQL = $" Select LocationID from CustomerLocation " +
                                  $" Where CustomerID = @CustomerID" +
                                  $" and UserID = @UserID ";
                
            using (SqlCommand LocationCommand = new SqlCommand(sLocationSQL, connection))
            {
                LocationCommand.CommandType = CommandType.Text;
                // adding parameters
                SqlParameter parameter = new SqlParameter
                {
                    ParameterName = "@CustomerID",
                    Value = sCustomerID.Trim(),
                    SqlDbType = SqlDbType.VarChar
                };
                LocationCommand.Parameters.Add(parameter);
                parameter = new SqlParameter
                {
                    ParameterName = "@UserID",
                    Value = sUserID,
                    SqlDbType = SqlDbType.VarChar
                };
                LocationCommand.Parameters.Add(parameter);
                try
                {
                    connection.Open();
                    using (SqlDataReader dr = LocationCommand.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            string sLocationID = Convert.ToString(dr["LocationID"]);
                            DeactivateLocation(connectionString, sLocationID, sUserID, ref sErrorMsg);
                            if (!String.IsNullOrEmpty(sErrorMsg))
                            {
                                return;
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    sErrorMsg = ex.Message;
                }
                finally
                {
                    if (connection.State == ConnectionState.Open)
                    {
                        connection.Close();
                    }
                }
            }
            return;
        }
        public static void DeactivateLocation(string connectionString, string sLocationID, string sUserID, ref string sErrorMsg)
        {
            // - Delete the location
            string sReturnVal = "Success";
            string sql = $" Update Location " +
                         $" Set Deleted =  'Y' " +
                         $" where LocationID = @LocationID " +
                         $" and UserID = @UserID ";

            SqlConnection connection = new SqlConnection(connectionString);
            using (SqlCommand command = new SqlCommand(sql, connection))
            {
                command.CommandType = CommandType.Text;
                // adding parameters
                SqlParameter parameter = new SqlParameter
                {
                    ParameterName = "@LocationID",
                    Value = sLocationID.Trim(),
                    SqlDbType = SqlDbType.VarChar
                };
                command.Parameters.Add(parameter);

                parameter = new SqlParameter
                {
                    ParameterName = "@UserID",
                    Value = sUserID,
                    SqlDbType = SqlDbType.VarChar
                };
                command.Parameters.Add(parameter);

                try
                {
                    connection.Open();
                    command.ExecuteNonQuery();
                    connection.Close();
                }
                catch (Exception ex)
                {
                    sReturnVal = "Unable to update Location. " + ex.Message;
                    return;
                }

            }

            // Delete the CustomerLocation
            string sCustomerID = GetCustomerIDfromLocation(connectionString, sLocationID, sUserID);
            string sCustLocSQL = $" Update CustomerLocation " +
                                 $" set Deleted = 'Y' " +
                                 $" where LocationID = @LocationID " +
                                 $" and CustomerID = @CustomerID ";
            using (SqlCommand CustLocCmd = new SqlCommand(sCustLocSQL, connection))
            {
                CustLocCmd.CommandType = CommandType.Text;

                // adding parameters
                SqlParameter parameter = new SqlParameter
                {
                    ParameterName = "@LocationID",
                    Value = sLocationID.Trim(),
                    SqlDbType = SqlDbType.VarChar
                };
                CustLocCmd.Parameters.Add(parameter);

                parameter = new SqlParameter
                {
                    ParameterName = "@CustomerID",
                    Value = sCustomerID.Trim(),
                    SqlDbType = SqlDbType.VarChar
                };
                CustLocCmd.Parameters.Add(parameter);

                try
                {
                    connection.Open();
                    CustLocCmd.ExecuteNonQuery();
                    connection.Close();
                }
                catch (Exception ex)
                {
                    sErrorMsg = "Unable to update CustomerLocation." + ex.Message;
                    return;
                }
            }

            // Delete any Placments
            string sPlacementSQL = $" Select LocationPlacementID " +
                                   $" from LocationPlacement " +
                                   $" Where LocationID = @LocationID " +
                                   $" and UserID = @UserID";
            using (SqlCommand PlacementCommand = new SqlCommand(sPlacementSQL, connection))
            {
                PlacementCommand.CommandType = CommandType.Text;

                // adding parameters
                SqlParameter parameter = new SqlParameter
                {
                    ParameterName = "@LocationID",
                    Value = sLocationID.Trim(),
                    SqlDbType = SqlDbType.VarChar
                };
                PlacementCommand.Parameters.Add(parameter);

                parameter = new SqlParameter
                {
                    ParameterName = "@UserID",
                    Value = sUserID.Trim(),
                    SqlDbType = SqlDbType.VarChar
                };
                PlacementCommand.Parameters.Add(parameter);

                try
                {
                    connection.Open();
                    using (SqlDataReader dr = PlacementCommand.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            string sPlacementID = Convert.ToString(dr["LocationPlacementID"]);
                            string RESULT = DeactivatePlacement(connectionString, sPlacementID, sUserID, ref sErrorMsg);
                            if (!String.IsNullOrEmpty(sErrorMsg))
                            {
                                break;
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    sErrorMsg = "Unable to delete placements. " + ex.Message;
                }
                finally
                {
                    if (connection.State == ConnectionState.Open)
                    {
                        connection.Close();
                    }
                }
            }
            return;

        }
        public static string DeactivatePlacement(string connectionString, string sPlacementID, string sUserID, ref string sErrorMsg)
        {
            string sReturnVal = "Success";
            string sql = $" Update LocationPlacement " +
                         $" Set Deleted =  'Y' " +
                         $" where LocationPlacementID = @PlacementID " +
                         $" and UserID = @UserID ";

            SqlConnection connection = new SqlConnection(connectionString);
            using (SqlCommand command = new SqlCommand(sql, connection))
            {
                command.CommandType = CommandType.Text;
                // adding parameters
                SqlParameter parameter = new SqlParameter
                {
                    ParameterName = "@PlacementID",
                    Value = sPlacementID.Trim(),
                    SqlDbType = SqlDbType.VarChar
                };
                command.Parameters.Add(parameter);

                parameter = new SqlParameter
                {
                    ParameterName = "@UserID",
                    Value = sUserID.Trim(),
                    SqlDbType = SqlDbType.VarChar
                };
                command.Parameters.Add(parameter);

                try
                {
                    connection.Open();
                    command.ExecuteNonQuery();
                    connection.Close();
                }
                catch (Exception ex)
                {
                    sErrorMsg = ex.Message;
                }

                return sErrorMsg;

            }
        }

        public static string SetInventoryQuantity(string connectionString, string sArrangmentInventoryID, string sUserID, ref string sErrorMsg)
        {
            //Count only those arrangment inventory items that have a status id
            //and that status is NOT Inactive - thus returning the number of inventory items
            //that are IN inventory.  Those which are InActive, or have NO status are not
            //considered IN inventory.
            string sRetValue = "Success";
            string sql = $" update arrangement " +
                         $" set quantity = (select count(*) from arrangementInventory " +
                         $"                 where  ArrangementID = (select ai2.ArrangementID " +
                         $"                                         from arrangementInventory ai2" +
                         $"                                         where ai2.ArrangementInventoryID = @ArrangementInventoryID " +
                         $"                                         and ai2.UserID = @UserID " +
                         $"                                        ) " +
                         $"                 and Deleted = 'N' " +
                         $"                 ) " +
                         $" where ArrangementID = (select ai.ArrangementID " +
                         $"                        from arrangementInventory ai " +
                         $"                        where ai.ArrangementInventoryID =  @ArrangementInventoryID" +
                         $"                        and ai.UserID = @UserID ) ";

            SqlConnection connection = new SqlConnection(connectionString);
            using (SqlCommand command = new SqlCommand(sql, connection))
            {
                command.CommandType = CommandType.Text;
                // adding parameters
                SqlParameter parameter = new SqlParameter
                {
                    ParameterName = "@ArrangementInventoryID",
                    Value = sArrangmentInventoryID.Trim(),
                    SqlDbType = SqlDbType.VarChar
                };
                command.Parameters.Add(parameter);

                parameter = new SqlParameter
                {
                    ParameterName = "@UserID",
                    Value = sUserID.Trim(),
                    SqlDbType = SqlDbType.VarChar
                };
                command.Parameters.Add(parameter);

                try
                {
                    connection.Open();
                    command.ExecuteNonQuery();
                    connection.Close();
                }
                catch (Exception ex)
                {
                    sErrorMsg = "Error setting quantity value."  + ex.Message;
                }
            }
            return sRetValue;
        }
        private static string SetPlanStatus(string connectionString, string sRoutePlanID, string sUserID, string sRoutePlanStatusCode)
        {
            string sRetValue = string.Empty;
            string sStatusID = string.Empty;

            string sql = $"Update RoutePlan " +
                          $" Set RoutePlanStatusID = (Select RoutePlanStatusID from RoutePlanStatus " +
                          $"                          where Code = @RoutePlanStatusCode )" +
                          $" where RoutePlanID = @RoutePlanID " +
                          $" and UserID = @UserID ";

            SqlConnection connection = new SqlConnection(connectionString);
            using (SqlCommand command = new SqlCommand(sql, connection))
            {
                command.CommandType = CommandType.Text;
                // adding parameters
                SqlParameter parameter = new SqlParameter
                {
                    ParameterName = "@RoutePlanStatusCode",
                    Value = sRoutePlanStatusCode.Trim(),
                    SqlDbType = SqlDbType.VarChar
                };
                command.Parameters.Add(parameter);

                parameter = new SqlParameter
                {
                    ParameterName = "@UserID",
                    Value = sUserID,
                    SqlDbType = SqlDbType.VarChar
                };
                command.Parameters.Add(parameter);

                parameter = new SqlParameter
                {
                    ParameterName = "@RoutePlanID",
                    Value = sRoutePlanID,
                    SqlDbType = SqlDbType.VarChar
                };
                command.Parameters.Add(parameter);

                try
                {
                    connection.Open();
                    command.ExecuteNonQuery();
                    connection.Close();
                }
                catch (Exception ex)
                {
                    sRetValue = ex.Message;
                }

            }
            return sRetValue;
        }
        private static string SetArrangmentInventoryStatus(string connectionString, string sArrangmentInventoryID, string sInventoryStatusCode, string sUserID, ref string sErrorMsg)
        {
            string sRetValue = string.Empty;
            string sStatusID = string.Empty;

            string sql = $"Update ArrangementInventory " +
                          $" Set Deleted = 'Y'" +
                          $" Where ArrangementInventoryID = @ArrangmentInventoryID" +
                          $" and UserID = @UserID ";

            SqlConnection connection = new SqlConnection(connectionString);
            using (SqlCommand command = new SqlCommand(sql, connection))
            {
                command.CommandType = CommandType.Text;
                // adding parameters
                SqlParameter parameter = new SqlParameter
                {                   
                    ParameterName = "@ArrangmentInventoryID",
                    Value = sArrangmentInventoryID,
                    SqlDbType = SqlDbType.VarChar
                };
                command.Parameters.Add(parameter);

                parameter = new SqlParameter
                {
                    ParameterName = "@UserID",
                    Value = sUserID,
                    SqlDbType = SqlDbType.VarChar
                };
                command.Parameters.Add(parameter);

                try
                {
                    connection.Open();
                    command.ExecuteNonQuery();
                    connection.Close();
                }
                catch (Exception ex)
                {
                    sErrorMsg = ex.Message;
                }

            }
            return sRetValue;
        }
        public static string FinalizePlan(string connectionString, string sRoutePlanID, string sUserID, ref string sErrorMsg)
        {
            string sReturnValue = "Success";

            List<RoutePlanDetail> lRoutePlanDetails = GetRoutePlanDetails(connectionString, sRoutePlanID, sUserID, ref sErrorMsg);
            foreach (RoutePlanDetail oRoutePlanDetail in lRoutePlanDetails)
            {
                // update the incoming arrangement to the new placment
                string sIncomingArrangmentInventoryID = oRoutePlanDetail.IncomingArrangmentInventoryID;
                sReturnValue = AssignIncomingInventory(connectionString, sIncomingArrangmentInventoryID, oRoutePlanDetail);

                // update the outgoing if being returned to the warehouse.
                string sOutgoingArrangmentInventoryID = oRoutePlanDetail.OutgoingArrangementInventoryID;
                sReturnValue = ReturnInventoryToWarehouse(connectionString, sOutgoingArrangmentInventoryID, oRoutePlanDetail);
            }
            //sReturnValue = SetPlanStatus(connectionString, sRoutePlanID, "Finalized");

            return sReturnValue;
        }

        private static string AssignIncomingInventory(string connectionString, string sArrangmentInventoryID, RoutePlanDetail oRoutePlanDetail)
        {
            string sStatusID = GetInventoryStatus(connectionString, "InUse");
            string sRetValue = "Failure";
            using (SqlConnection connection = new SqlConnection(connectionString))
            {

                // string sql = "Insert Into Location (Name, Description, LocationTypeID) Values (@Name, @Description, @LocationTypeID)";
                string sql = $" Update ArrangementInventory " +
                             $" SET LastUsed=@LastUsed, LocationId=@LocationID, LocationPlacementID=@LocationPlacementID, " +
                             $" InventoryStatusID =@InventoryStatusID " +
                             $" Where ArrangementInventoryID=@ArrangmentInventoryID";
                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    command.CommandType = CommandType.Text;
                    // adding parameters
                    SqlParameter parameter = new SqlParameter
                    {
                        ParameterName = "@LastUsed",
                        Value = DateTime.Now,
                        SqlDbType = SqlDbType.DateTime
                    };
                    command.Parameters.Add(parameter);

                    parameter = new SqlParameter
                    {
                        ParameterName = "@LocationID",
                        Value = oRoutePlanDetail.LocationID,
                        SqlDbType = SqlDbType.VarChar
                    };
                    command.Parameters.Add(parameter);

                    parameter = new SqlParameter
                    {
                        ParameterName = "@LocationPlacementID",
                        Value = oRoutePlanDetail.LocationPlacementID,
                        SqlDbType = SqlDbType.VarChar
                    };
                    command.Parameters.Add(parameter);

                    parameter = new SqlParameter
                    {
                        ParameterName = "@ArrangmentInventoryID",
                        Value = sArrangmentInventoryID,
                        SqlDbType = SqlDbType.VarChar
                    };
                    command.Parameters.Add(parameter);

                    parameter = new SqlParameter
                    {
                        ParameterName = "@InventoryStatusID",
                        Value = sStatusID,
                        SqlDbType = SqlDbType.VarChar
                    };
                    command.Parameters.Add(parameter);

                    connection.Open();
                    command.ExecuteNonQuery();
                    connection.Close();
                }
            }
            return sRetValue;
        }

        private static string ReturnInventoryToWarehouse(string connectionString, string sArrangmentInventoryID, RoutePlanDetail oRoutePlanDetail)
        {
            string sStatusID = GetInventoryStatus(connectionString, "Available");
            string sRetValue = "";
            using (SqlConnection connection = new SqlConnection(connectionString))
            {

                // string sql = "Insert Into Location (Name, Description, LocationTypeID) Values (@Name, @Description, @LocationTypeID)";
                string sql = $" Update ArrangementInventory " +
                             $" SET  LocationId=@WarehouseID, LocationPlacementID=null, " +
                             $" InventoryStatusID=@InventoryStatusID" +
                             $" Where ArrangementInventoryID=@ArrangmentInventoryID";
                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    command.CommandType = CommandType.Text;
                    // adding parameters
                    SqlParameter parameter = new SqlParameter
                    {
                        ParameterName = "@ArrangmentInventoryID",
                        Value = sArrangmentInventoryID,
                        SqlDbType = SqlDbType.VarChar
                    };
                    command.Parameters.Add(parameter);

                    parameter = new SqlParameter
                    {
                        ParameterName = "@WarehouseID",
                        Value = oRoutePlanDetail.WarehouseID,
                        SqlDbType = SqlDbType.VarChar
                    };
                    command.Parameters.Add(parameter);

                    parameter = new SqlParameter
                    {
                        ParameterName = "@InventoryStatusID",
                        Value = sStatusID,
                        SqlDbType = SqlDbType.VarChar
                    };
                    command.Parameters.Add(parameter);


                    connection.Open();
                    command.ExecuteNonQuery();
                    connection.Close();
                }
            }
            return sRetValue;
        }

        public static string GetCustomerIDfromLocation(string connectionString, string LocationID, string sUserID)
        {
            string sRetValue = string.Empty;

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                string sCustomerTypeSQL = $" Select CustomerID " +
                                          $" from CustomerLocation " +
                                          $" where LocationID = @LocationID" ;

                using (SqlCommand command = new SqlCommand(sCustomerTypeSQL, connection))
                {
                    command.Parameters.Clear();
                    command.CommandText = sCustomerTypeSQL;

                    SqlParameter parameter = new SqlParameter
                    {
                        ParameterName = "@LocationID",
                        Value = LocationID,
                        SqlDbType = SqlDbType.VarChar
                    };
                    command.Parameters.Add(parameter);
                    using (SqlDataReader dr = command.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            sRetValue = Convert.ToString(dr["CustomerID"]);
                        }
                    }
                }
                return sRetValue;
            }

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
        public static string GetLocationType(string connectionString, string sLocationID)
        {
            string sRetValue = string.Empty;

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                string sSQL = $" Select lt.Code Code" +
                                          $" from location l " +
                                          $" join LocationType lt on l.locationTypeID = lt.LocationTypeID " +
                                          $" where LocationID = @LocationID";
                using (SqlCommand command = new SqlCommand(sSQL, connection))
                {
                    command.Parameters.Clear();

                    // adding parameters
                    SqlParameter parameter = new SqlParameter
                    {
                        ParameterName = "@LocationID",
                        Value = sLocationID,
                        SqlDbType = SqlDbType.VarChar
                    };
                    command.Parameters.Add(parameter);
                    //command.CommandText = sCustomerTypeSQL;

                    using (SqlDataReader dr = command.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            sRetValue = Convert.ToString(dr["Code"]);
                        }
                    }
                }
                connection.Close();
            }

            return sRetValue;
        }
        public static string GetCustomerNameById(string connectionString, string id, string sUserID, ref string sErrorMsg)
        {
            string sRetValue = string.Empty;

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                string sCustomerNameSQL = $" Select Name from Customer " +
                                          $" where CustomerID = @CustomerID " +
                                          $" and UserID = @UserID " +
                                          $" and Deleted = 'N' ";
                try
                {
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

                        parameter = new SqlParameter
                        {
                            ParameterName = "@UserID",
                            Value = sUserID,
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
                }
                catch (Exception ex)
                {
                    sErrorMsg = "Unable to get Customer Name by id. " + ex.Message;
                }
                connection.Close();
            }

            return sRetValue;
        }
        public static string GetLocationNameById(string connectionString, string id, string sUserID, ref string sErrorMsg)
        {
            string sRetValue = string.Empty;

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                string sLocationNameSQL = $"Select Name from Location where LocationID = @LocationID and UserID = @UserID";
                try
                {
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

                        parameter = new SqlParameter
                        {
                            ParameterName = "@UserID",
                            Value = sUserID,
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
                }
                catch (Exception ex)
                {
                    sErrorMsg = "Unable to get loctaion Name by ID. " + ex.Message;
                }
                connection.Close();
            }

            return sRetValue;
        }
        public static LocationPlacement GetLocationPlacement(string connectionString, string sPlacementID, string sUserID, ref string sErrorMsg)
        {
           
            LocationPlacement ivm = new LocationPlacement();
            List<SelectListItem> SizeList = SilkDesignUtility.GetSizes(connectionString);
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    string sql = "SELECT " +
                        "  s.Code        CODE " +
                        " ,la.LocationID LOCATIONID " +
                        " ,la.Quantity   QUANTITY " +
                        " ,s.SizeID      SIZEID " +
                        " ,la.Description DESCRIPTION " +
                        " ,la.UserID     USERID " +
                        " FROM LocationPlacement la " +
                        " join Size s on s.SizeID = la.SizeID " +
                        $" where la.LocationPlacementID= @PlacementID " +
                        $" and la.UserID = @UserID";

                    SqlCommand readcommand = new SqlCommand(sql, connection);
                    SqlParameter parameter = new SqlParameter
                    {
                        ParameterName = "@UserID",
                        Value = sUserID,
                        SqlDbType = SqlDbType.VarChar
                    };
                    readcommand.Parameters.Add(parameter);

                    parameter = new SqlParameter
                    {
                        ParameterName = "@PlacementID",
                        Value = sPlacementID,
                        SqlDbType = SqlDbType.VarChar
                    };
                    readcommand.Parameters.Add(parameter);
                    try
                    {
                        using (SqlDataReader dr = readcommand.ExecuteReader())
                        {
                            if (dr.HasRows)
                            {
                                while (dr.Read())
                                {
                                    ivm.LocationPlacementID = sPlacementID;
                                    ivm.LocationID = Convert.ToString(dr["LOCATIONID"]);
                                    ivm.Quantity = Convert.ToInt32(dr["QUANTITY"]);
                                    ivm.Description = Convert.ToString(dr["DESCRIPTION"]);
                                    ivm.SizeID = Convert.ToString(dr["SIZEID"]);
                                    ivm.UserID = Convert.ToString(dr["USERID"]);
                                    ivm.Sizes = SizeList;
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        sErrorMsg = "Unable to get location placement. " + ex.Message;
                    }
                    connection.Close();
                }
            }
            catch (Exception ex)
            {
                sErrorMsg = "Error reading Location Placment." + ex.Message;
            }
            return ivm;
        }
        public static List<LocationPlacement> GetLoationPlacements(string? connectionString, string id, string sUserID, ref string sErrorMsg)
        {
            string sLocationID = id;
            List<LocationPlacement> ivmList = new List<LocationPlacement>();
            List<SelectListItem> SizeList = SilkDesignUtility.GetSizes(connectionString);
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                string sql = "SELECT distinct " +
                    "  p.LocationPlacementID  ID " +
                    " ,p.Quantity    QUANTITY " +
                    " ,s.Code        CODE " +
                    " ,s.SizeID      SIZEID" +
                    " ,p.Description DESCRIPTION " +
                    " ,a.Name        ARRANGEMENT" +
                    " FROM LocationPlacement p " +
                    " join Size s on s.SizeID = p.SizeID " +
                    " left outer join arrangementInventory ai on ai.LocationPlacementID = p.LocationPlacementID and ai.Deleted = 'N'  and ai.UserID = @UserID " +
                    " left outer join arrangement a on a.arrangementID = ai.ArrangementID  and a.UserID = @UserID " +
                    $" where p.LocationID= @LocationID " +
                    $" and p.Deleted = 'N' " +
                    $" and p.UserID = @UserID ";

                SqlCommand readcommand = new SqlCommand(sql, connection);
                SqlParameter parameter = new SqlParameter
                {
                    ParameterName = "@UserID",
                    Value = sUserID,
                    SqlDbType = SqlDbType.VarChar
                };
                readcommand.Parameters.Add(parameter);
                parameter = new SqlParameter
                {
                    ParameterName = "@LocationID",
                    Value = sLocationID,
                    SqlDbType = SqlDbType.VarChar
                };
                readcommand.Parameters.Add(parameter);

                try
                {
                    using (SqlDataReader dr = readcommand.ExecuteReader())
                    {
                        while (dr.Read())
                        {

                            LocationPlacement ivm = new LocationPlacement();
                            ivm.LocationPlacementID = Convert.ToString(dr["ID"]);
                            ivm.Quantity = Convert.ToInt32(dr["Quantity"]);
                            ivm.SizeID = Convert.ToString(dr["SIZEID"]);
                            ivm.Code = Convert.ToString(dr["CODE"]);
                            ivm.Description = Convert.ToString(dr["DESCRIPTION"]);
                            string sArrangement = Convert.ToString(dr["ARRANGEMENT"]);
                            if (!String.IsNullOrEmpty(sArrangement))
                            {
                                ivm.Description += " / " + sArrangement;
                            }
                            ivm.Sizes = SizeList;
                            ivmList.Add(ivm);
                        }
                    }
                }
                catch (Exception ex)
                {
                    sErrorMsg = "Cant get placements for location. " + ex.Message;
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
                        list.Add(new SelectListItem { Text = "No sizes found", Value = "" });
                    }
                    list.Insert(0, new SelectListItem { Text = "-- Select Size--", Value = "" });
                    connection.Close();
                }
            }
            catch (Exception ex)
            {
                list.Add(new SelectListItem { Text = ex.Message.ToString(), Value = "0" });
            }

            return list;
        }
        public static List<SelectListItem> GetInventoryStatus(string connectionString)
        {
            List<SelectListItem> list = new List<SelectListItem>();
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    string sql = "Select * from InventoryStatus order by SortOrder";
                    SqlCommand cmd = new SqlCommand(sql, connection);
                    SqlDataReader reader = cmd.ExecuteReader();

                    //if (reader.HasRows)
                    //{
                        while (reader.Read())
                        {
                            list.Add(new SelectListItem { Text = reader["Code"].ToString(), Value = reader["InventoryStatusID"].ToString() });
                        }
                    //}
                    //else
                    //{
                    //    list.Add(new SelectListItem { Text = "No status found", Value = "" });
                    //}
                    list.Insert(0, new SelectListItem { Text = "-- Select Status --", Value = "" });
                    connection.Close();
                }
            }
            catch (Exception ex)
            {
                list.Add(new SelectListItem { Text = ex.Message.ToString(), Value = "0" });
            }

            return list;
        }
        public static List<RouteLocation> GetRouteLocations(string connectionString, string sRouteID, string sUserID, ref string sErrorMsg)
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
                                          $" join Location l on l.LocationID = rl.LocationID and l.UserID = @UserID" +
                                          $" join CustomerLocation cl on cl.locationID = l.locationID and cl.deleted = 'N'" +
                                          $" join customer c on c.customerID = cl.CustomerID and c.UserID = @UserID " +
                                          $" where rl.RouteID  = @RouteID " +
                                          $" and rl.UserID = @UserID" +
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
                    parameter = new SqlParameter
                    {
                        ParameterName = "@UserID",
                        Value = sUserID,
                        SqlDbType = SqlDbType.VarChar
                    };
                    command.Parameters.Add(parameter);
                    try
                    {
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
                            routeLocation.UserID = sUserID;
                            list.Add(routeLocation);
                        }
                    }
                    }
                    catch (Exception ex)
                    {
                        sErrorMsg = "Error getting route location. " + ex.Message;
                    }
                }
                connection.Close();
            }

            return list;
        }
        public static List<SilkDesign.Models.Route> GetRoutes(string connectionString, string sRouteID, string sUserID, ref string sErrorMsg)
        {
            List<SilkDesign.Models.Route> list = new List<SilkDesign.Models.Route>();
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string sql = $" Select * " +
                             $" From Route  " +
                             $" Where RouteId= @RouteID " +
                             $" and UserID = @UserID ";

                SqlCommand command = new SqlCommand(sql, connection);
                SqlParameter parameter = new SqlParameter
                {
                    ParameterName = "@RouteID",
                    Value = sRouteID,
                    SqlDbType = SqlDbType.VarChar
                };
                command.Parameters.Add(parameter);
                parameter = new SqlParameter
                {
                    ParameterName = "@UserID",
                    Value = sUserID,
                    SqlDbType = SqlDbType.VarChar
                };
                command.Parameters.Add(parameter);
                connection.Open();
                try
                {
                    using (SqlDataReader dataReader = command.ExecuteReader())
                    {
                        while (dataReader.Read())
                        {
                            Models.Route route = new Models.Route();
                            route.RouteId = Convert.ToString(dataReader["RouteId"]);
                            route.Name = Convert.ToString(dataReader["Name"]);
                            route.Description = Convert.ToString(dataReader["Description"]);
                            route.WarehouseID = Convert.ToString(dataReader["WarehouseID"]);
                            route.Warehouses = SilkDesignUtility.GetWarehouses(connectionString, sUserID);
                            list.Add(route);
                        }
                    }
                }
                catch (Exception ex)
                {
                    sErrorMsg = "Unable to get route. " + ex.Message;
                }

                connection.Close();
            }

            return list;
        }
        public static IEnumerable<SelectListItem> GetAvailableLocations(string connectionString, string sRouteID, string sUserID, ref string sErrorMsg)
        {
            List<SelectListItem> list = new List<SelectListItem>();
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    string sql = $"Select " +
                                 $"  c.Name + ' | ' + l.Name Name, " +
                                 $"  l.locationID            LocationID " +
                                 $" from CustomerLocation cl " +
                                 $" join Customer c on cl.CustomerID = c.CustomerID and c.UserID = @UserID and c.Deleted = 'N' " +
                                 $" join location l on cl.LocationID = l.LocationID and l.UserID = @UserID and l.Deleted = 'N'" +
                                 $" where cl.locationID not in (select rl.locationID  " +
                                 $"                             from routeLocation rl " +
                                 $"                             where rl.RouteID = @RouteID" +
                                 $"                             and rl.UserID = @UserID )" +
                                 $" and cl.deleted = 'N'" +
                                 $" order by c.Name, l.Name";

                    using (SqlCommand command = new SqlCommand(sql, connection))
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
                        parameter = new SqlParameter
                        {
                            ParameterName = "@UserID",
                            Value = sUserID,
                            SqlDbType = SqlDbType.VarChar
                        };
                        command.Parameters.Add(parameter);

                        try
                        {
                            using (SqlDataReader reader = command.ExecuteReader())
                            {
                                if (reader.HasRows)
                                {
                                    while (reader.Read())
                                    {
                                        string sText = reader["Name"].ToString();
                                        string sValue = reader["LocationID"].ToString();

                                        list.Add(new SelectListItem { Text = sText, Value = sValue });
                                    }
                                }
                                else
                                {
                                    list.Add(new SelectListItem { Text = "No Locations found", Value = "0" });
                                }
                                list.Insert(0, new SelectListItem { Text = "-- Select Location --", Value = "0" });
                            }
                        }
                        catch (Exception ex)
                        {
                            sErrorMsg = "Error retrieving location. " + ex.Message;
                        }
                    }
                    connection.Close();
                }
            }
            catch (Exception ex)
            {
                list.Add(new SelectListItem { Text = ex.Message.ToString(), Value = "0" });
            }


            return list;
        }
        public static RouteLocation GetRouteLocation(string? connectionString, string sRouteLocationID, string sUserID, ref string sErrorMsg)
        {
            RouteLocation routelocal = new RouteLocation();


            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                string sRouteLocationSQL = 
                          $" Select rl.RouteID         ROUTEID, " +
                          $" rl.LocationID             LOCATIONID, " +
                          $" c.Name                    CUSTOMERNAME, " +
                          $" l.Name                    LOCATIONNAME, " +
                          $" rl.RouteOrder             ROUTEORDER " +
                          $" from routelocation rl " +
                          $" join Location l on l.LocationID = rl.LocationID and l.UserID = @UserID" +
                          $" join CustomerLocation cl on cl.locationID = l.locationID and cl.deleted = 'N' " +
                          $" join customer c on c.customerID = cl.CustomerID and c.UserID = @UserID " +
                          $" where rl.RouteLocationID  = @RouteLocationID " +
                          $" and rl.UserID = @UserID " +
                          $" ORDER BY rl.RouteOrder ASC";

                try
                {
                    using (SqlCommand command = new SqlCommand(sRouteLocationSQL, connection))
                    {
                        command.Parameters.Clear();

                        // adding parameters
                        SqlParameter parameter = new SqlParameter
                        {
                            ParameterName = "@RouteLocationID",
                            Value = sRouteLocationID,
                            SqlDbType = SqlDbType.VarChar
                        };
                        command.Parameters.Add(parameter);
                        parameter = new SqlParameter
                        {
                            ParameterName = "@UserID",
                            Value = sUserID,
                            SqlDbType = SqlDbType.VarChar
                        };
                        command.Parameters.Add(parameter);
                        using (SqlDataReader dr = command.ExecuteReader())
                        {
                            while (dr.Read())
                            {
                                routelocal.RouteID = Convert.ToString(dr["ROUTEID"]);
                                routelocal.LocationID = Convert.ToString(dr["LOCATIONID"]);
                                routelocal.RouteOrder = Convert.ToInt32(dr["ROUTEORDER"]);
                                routelocal.CustomerName = Convert.ToString(dr["CUSTOMERNAME"]);
                                routelocal.LocationName = Convert.ToString(dr["LOCATIONNAME"]);
                                routelocal.OldRouteOrder = Convert.ToInt32(dr["ROUTEORDER"]);
                                //routelocal.AvailableLocations = GetAvailableLocations(connectionString, sRouteLocationID);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    sErrorMsg = "Error reading rout location. " + ex.Message;
                }
                connection.Close();
            }
            return routelocal;
        }
        internal static IEnumerable<SelectListItem> GetRoutes(string connectionString, string sUserID, ref string sErrorMsg)
        {
            List<SelectListItem> list = new List<SelectListItem>();
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    string sql = "Select RouteID, Name " +
                                 " from Route " +
                                 " where UserId = @UserID " +
                                 " and deleted = 'N' ";
                    SqlCommand cmd = new SqlCommand(sql, connection);
                    SqlParameter parameter = new SqlParameter
                    {
                        ParameterName = "@UserID",
                        Value = sUserID,
                        SqlDbType = SqlDbType.VarChar
                    };
                    cmd.Parameters.Add(parameter);
                    SqlDataReader reader = cmd.ExecuteReader();

                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            string sText = reader["Name"].ToString();
                            string sValue = reader["RouteID"].ToString();

                            list.Add(new SelectListItem { Text = sText, Value = sValue });
                        }
                    }
                    else
                    {
                        list.Add(new SelectListItem { Text = "No Routes found", Value = "0" });
                    }
                    list.Insert(0, new SelectListItem { Text = "-- Select Route --", Value = "0" });
                    connection.Close();
                }
            }
            catch (Exception ex)
            {
                sErrorMsg = "Error reading routes. " + ex.Message;
                list.Add(new SelectListItem { Text = "No Routes found", Value = "0" });
            }
            return list;
        }
        internal static List<SelectListItem> GetWarehouses(string? connectionString, string sUserID, bool bAddSelectOption = true)
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
                                                        " where code = 'Warehouse') " +
                                 "and UserID = @UserID ";
                    SqlCommand cmd = new SqlCommand(sql, connection);
                    SqlParameter parameter = new SqlParameter
                    {
                        ParameterName = "@userID",
                        Value = sUserID,
                        SqlDbType = SqlDbType.VarChar
                    };
                    cmd.Parameters.Add(parameter);

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
                        if (bAddSelectOption) 
                        { 
                            list.Add(new SelectListItem { Text = "No Warehouse found", Value = "" });
                        }
                    }
                    if (bAddSelectOption)
                    {
                        list.Insert(0, new SelectListItem { Text = "-- Select Warehouse--", Value = "" });
                    }
                    connection.Close();
                }
            }
            catch (Exception ex)
            {
                list.Add(new SelectListItem { Text = ex.Message.ToString(), Value = "0" });
            }

            return list;
        }
        
        public static string CreateLocation(string connectionString, string LocationName, string Description, string LocationTypeID, string sUserID, ref string ErrorMsg)
        {
            string sLocationID = string.Empty;
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string sql = "Insert Into Location (Name, Description, LocationTypeID, UserID ) Values (@Name, @Description, @LocationTypeID, @UserID)";
                try
                {
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

                        parameter = new SqlParameter
                        {
                            ParameterName = "@UserID",
                            Value = sUserID,
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
                }
                catch (Exception ex)
                {
                    ErrorMsg = "Unable to create location - " + ex.Message;
                }
                return sLocationID;
            }
        }
        public static string CreateCustomer(string connectionString, SilkDesign.Models.Customer customer, ref string ErrorMsg)
        {
            string sCustomerID = string.Empty;
            if (!CustomerNameDuplicated(connectionString, customer))
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    string sql = "Insert Into Customer (Name, Address, UserID) Values (@Name, @Address, @UserID)";

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

                        parameter = new SqlParameter
                        {
                            ParameterName = "@UserID",
                            Value = customer.UserID,
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
                        {
                            ErrorMsg = ex.Message;
                        }
                        finally
                        { connection.Close(); }
                    }
                }
            }
            return sCustomerID;
        }

        private static bool CustomerNameDuplicated(string connectionString, Customer customer)
        {
            bool bRetValue = false;
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                string sLocationNameSQL = $"Select CustomerID " +
                                          $" from Customer " +
                                          $" where trim(Name) = @Name " +
                                          $" and UserID = @UserID " +
                                          $" and Deleted = 'N' ";
                using (SqlCommand command = new SqlCommand(sLocationNameSQL, connection))
                {
                    command.Parameters.Clear();

                    // adding parameters
                    SqlParameter parameter = new SqlParameter
                    {
                        ParameterName = "@Name",
                        Value = customer.Name.Trim(),
                        SqlDbType = SqlDbType.VarChar
                    };
                    command.Parameters.Add(parameter);

                    parameter = new SqlParameter
                    {
                        ParameterName = "@UserID",
                        Value = customer.UserID,
                        SqlDbType = SqlDbType.VarChar
                    };
                    command.Parameters.Add(parameter);

                    using (SqlDataReader dr = command.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            bRetValue = true;
                            break;
                        }
                    }
                }
                connection.Close();
            }
            return bRetValue;
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
        public static string CreateLocationPlacement(string connectionString, string sSizeID, string sDescription, string sLocationID, int iQuantity, string sUserID, ref string sErrorMsg)
        {
            string sLocationPlacementID = string.Empty;
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string sql = "Insert Into LocationPlacement (LocationID,  SizeID,  Description,  Quantity, UserID) " +
                                                   "Values (@LocationID, @SizeID, @Description, @Quantity, @UserID)";

                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    command.CommandType = CommandType.Text;

                    // adding parameters
                    SqlParameter parameter = new SqlParameter
                    {
                        ParameterName = "@LocationID",
                        Value = sLocationID,
                        SqlDbType = SqlDbType.VarChar
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
                        ParameterName = "@UserID",
                        Value = sUserID,
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

                    parameter = new SqlParameter
                    {
                        ParameterName = "@Quantity",
                        Value = iQuantity,
                        SqlDbType = SqlDbType.Int
                    };
                    command.Parameters.Add(parameter);

                    connection.Open();


                    try
                    {
                        command.ExecuteNonQuery();

                        string sCustomerSQL = $" Select LocationPlacementID " +
                                              $" from LocationPlacement " +
                                              $" where Description = @Description";
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
                    { 
                        sErrorMsg = ex.Message;
                    }
                    finally
                    { connection.Close(); }
                }
            }
            return sLocationPlacementID;
        }
        public static string CreateCatalog(string connectionString, Catalog arrangement, ref string sErrorMsg)
        {
            string sCatalogID = string.Empty;
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string sql = "Insert Into Catalog (Code, Name, Description, SizeID, CatalogStatusID ) Values (@Code, @Name, @Description, @SizeID, @CatalogStatusID)";

                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    command.CommandType = CommandType.Text;

                    // adding parameters
                    SqlParameter parameter = new SqlParameter
                    {
                        ParameterName = "@Name",
                        Value = arrangement.Name,
                        SqlDbType = SqlDbType.VarChar
                    };
                    command.Parameters.Add(parameter);

                    parameter = new SqlParameter
                    {
                        ParameterName = "@Description",
                        Value = arrangement.Description,
                        SqlDbType = SqlDbType.VarChar
                    };
                    command.Parameters.Add(parameter);

                    parameter = new SqlParameter
                    {
                        ParameterName = "@Code",
                        Value = arrangement.Code,
                        SqlDbType = SqlDbType.VarChar
                    };
                    command.Parameters.Add(parameter);

                    parameter = new SqlParameter
                    {
                        ParameterName = "@SizeID",
                        Value = arrangement.SelectedSizeId,
                        SqlDbType = SqlDbType.VarChar,
                        Size = 50
                    };
                    command.Parameters.Add(parameter);
                    parameter = new SqlParameter
                    {
                        ParameterName = "@CatalogStatusID",
                        Value = arrangement.CatalogStatusID,
                        SqlDbType = SqlDbType.VarChar
                    };
                    command.Parameters.Add(parameter);
                    connection.Open();
                    try
                    {
                        command.ExecuteNonQuery();

                        sCatalogID = string.Empty;

                        string sCustomerSQL = $" Select CatalogID from Catalog " +
                                              $" where NAME = @Name " +
                                              $" and Code = @Code ";

                        command.Parameters.Clear();
                        parameter = new SqlParameter
                        {
                            ParameterName = "@Name",
                            Value = arrangement.Name,
                            SqlDbType = SqlDbType.VarChar
                        };
                        command.Parameters.Add(parameter);
                        parameter = new SqlParameter
                        {
                            ParameterName = "@Code",
                            Value = arrangement.Code,
                            SqlDbType = SqlDbType.VarChar
                        };
                        command.Parameters.Add(parameter);
                        command.CommandText = sCustomerSQL;

                        using (SqlDataReader dr = command.ExecuteReader())
                        {
                            while (dr.Read())
                            {
                                sCatalogID = Convert.ToString(dr["CatalogID"]);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        sErrorMsg = "Unable to create Catalog. " + ex.Message;
                    }
                    finally
                    { 
                        connection.Close(); 
                    }
                }
            }

            return sCatalogID;
        }

        public static string CreateArrangement(string connectionString, Arrangement arrangement, ref string sErrorMsg)
        {
            string sArrangementID = string.Empty;
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string sql = "Insert Into Arrangement (Code, Name, Price, Quantity, Description, SizeID, UserID, CatalogID) Values (@Code, @Name, @Price, @Quantity, @Description, @SizeID, @UserID, @CatalogID)";

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
                        Value = 0,
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
                        SqlDbType = SqlDbType.VarChar
                    };
                    command.Parameters.Add(parameter);
                    parameter = new SqlParameter
                    {
                        ParameterName = "@UserID",
                        Value = arrangement.UserID,
                        SqlDbType = SqlDbType.VarChar
                    };
                    command.Parameters.Add(parameter);

                    parameter = new SqlParameter
                    {
                        ParameterName = "@SizeID",
                        Value = arrangement.SelectedSizeId,
                        SqlDbType = SqlDbType.VarChar
                    };
                    command.Parameters.Add(parameter);
                    parameter = new SqlParameter
                    {
                        ParameterName = "@CatalogID",
                        Value = arrangement.CatalogID,
                        SqlDbType = SqlDbType.VarChar
                    };
                    command.Parameters.Add(parameter);
                    connection.Open();
                    try
                    {
                        command.ExecuteNonQuery();

                        sArrangementID = string.Empty;

                        string sCustomerSQL = $"Select ArrangementID from Arrangement where NAME = @Name and Code = @Code and UserID = @UserID";
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
                        parameter = new SqlParameter
                        {
                            ParameterName = "@UserID",
                            Value = arrangement.UserID,
                            SqlDbType = SqlDbType.VarChar
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
                    {
                        sErrorMsg = "Unable to create Arrangment. " + ex.Message;
                    }
                    finally
                    { connection.Close(); }

                }
            }

            return sArrangementID;
        }
        public static string CreateArrangementInventory(string connectionString, Arrangement arrangement, ref string sErrorMsg)
        {
            string sArrangementInventoryID = string.Empty;

            string sNextCode = GetNextInventoryCode(connectionString, arrangement.Code, arrangement.UserID);
            using (SqlConnection connection = new SqlConnection(connectionString))
            {

                string sql = "Insert Into ArrangementInventory (ArrangementID, Code, UserID) Values (@ArrangementID, @Code, @UserID)";
                using (SqlCommand command = new SqlCommand(sql, connection))
                {

                    command.CommandType = CommandType.Text;

                    // adding parameters
                    SqlParameter parameter = new SqlParameter
                    {
                        ParameterName = "@ArrangementID",
                        Value = arrangement.ArrangementID,
                        SqlDbType = SqlDbType.VarChar
                    };
                    command.Parameters.Add(parameter);
                    parameter = new SqlParameter
                    {
                        ParameterName = "@UserID",
                        Value = arrangement.UserID,
                        SqlDbType = SqlDbType.VarChar
                    };
                    command.Parameters.Add(parameter);

                    parameter = new SqlParameter
                    {
                        ParameterName = "@Code",
                        Value =sNextCode,
                        SqlDbType = SqlDbType.VarChar
                    };
                    command.Parameters.Add(parameter);

                    connection.Open();
                    try
                    {
                        command.ExecuteNonQuery();

                        string sCustomerSQL = $" Select ArrangementInventoryID " +
                                              $" from ArrangementInventory " +
                                              $" where ArrangementID = @ArrangementID " +
                                              $" and Code = @Code " +
                                              $" and UserID = @UserID " +
                                              $" and deleted = 'N'";
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

                        parameter = new SqlParameter
                        {
                            ParameterName = "@UserID",
                            Value = arrangement.UserID,
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
                    catch (Exception ex)
                    {
                        sErrorMsg = "Failed to create Arrangment Inventory. " + ex.Message;                        
                    }
                    finally { connection.Close(); } 
                }
            }
            return sArrangementInventoryID;
        }
        public static string CreateArrangementInventory(string connectionString, ArrangementInventory inventory, ref string sErrorMsg)
        {
            string sArrangementInventoryID = string.Empty;
            string sInventoryStatusID = string.Empty;

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string sInventoryStatusCode = string.Empty;
                string sSelectedLocationType = GetLocationType(connectionString, inventory.LocationID);
                if (sSelectedLocationType == "Customer")
                {
                    sInventoryStatusCode = "InUse";
                }
                else if (sSelectedLocationType == "Warehouse")
                {
                    sInventoryStatusCode = "Available";
                }
                else
                {
                    sInventoryStatusCode = "Allocated";
                }
                sInventoryStatusID = GetInventoryStatus(connectionString, sInventoryStatusCode);

                string sql = "Insert Into ArrangementInventory (ArrangementID, Code, LocationID, InventoryStatusID, UserID ";
                if (inventory.LocationPlacementID.Length > 1 )
                {
                    sql += ", LocationPlacementID ";
                }
                sql += " ) Values (@ArrangementID, @Code, @LocationID, @InventoryStatusID, @UserID ";
                if (inventory.LocationPlacementID.Length > 1)
                {
                    sql += ", @LocationPlacementID ";
                }
                sql += " )";

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
                        ParameterName = "@UserID",
                        Value = inventory.UserID,
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

                    parameter = new SqlParameter
                    {
                        ParameterName = "@InventoryStatusID",
                        Value = sInventoryStatusID,
                        SqlDbType = SqlDbType.VarChar
                    };
                    command.Parameters.Add(parameter);

                    if (inventory.LocationPlacementID.Trim().Length > 1)
                    {
                        parameter = new SqlParameter
                        {
                            ParameterName = "@LocationPlacementID",
                            Value = inventory.LocationPlacementID,
                            SqlDbType = SqlDbType.VarChar
                        };
                        command.Parameters.Add(parameter);
                    }

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
                    catch (Exception ex) 
                    {
                        sErrorMsg = "Error Creating Arrangement Inventory." + ex.Message;
                    }
                    finally { connection.Close(); }
                }
            }
            return sArrangementInventoryID;
        }

        private static string GetInventoryStatus(string connectionString, string sStatusCode)
        {
            string sRetValue = string.Empty;

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                string sLocationNameSQL = $"Select InventoryStatusID from InventoryStatus where Code = @Code";
                using (SqlCommand command = new SqlCommand(sLocationNameSQL, connection))
                {
                    command.Parameters.Clear();

                    // adding parameters
                    SqlParameter parameter = new SqlParameter
                    {
                        ParameterName = "@Code",
                        Value = sStatusCode,
                        SqlDbType = SqlDbType.VarChar
                    };
                    command.Parameters.Add(parameter);

                    using (SqlDataReader dr = command.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            sRetValue = Convert.ToString(dr["InventoryStatusID"]);
                        }
                    }
                }
                connection.Close();
            }

            return sRetValue;
        }

        internal static void CreateRouteLocation(string? connectionString, RouteLocation routeLocation, string sRouteID, string sUserID, ref string sErrorMsg)
        {
            string sLocationID = string.Empty;
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string sql = "Insert Into RouteLocation (RouteID, LocationID, RouteOrder, UserID) Values (@RouteID, @LocationID, @RouteOrder, @UserID)";

                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    command.CommandType = CommandType.Text;

                    // adding parameters
                    SqlParameter parameter = new SqlParameter
                    {
                        ParameterName = "@RouteID",
                        Value = sRouteID,
                        SqlDbType = SqlDbType.VarChar,
                    };
                    command.Parameters.Add(parameter);

                    parameter = new SqlParameter
                    {
                        ParameterName = "@LocationID",
                        Value = routeLocation.LocationID,
                        SqlDbType = SqlDbType.VarChar
                    };
                    command.Parameters.Add(parameter);

                    parameter = new SqlParameter
                    {
                        ParameterName = "@RouteOrder",
                        Value = routeLocation.RouteOrder,
                        SqlDbType = SqlDbType.Int
                    };
                    command.Parameters.Add(parameter);
                    parameter = new SqlParameter
                    {
                        ParameterName = "@UserID",
                        Value = sUserID,
                        SqlDbType = SqlDbType.VarChar
                    };
                    command.Parameters.Add(parameter);

                    connection.Open();
                    try
                    {
                        command.ExecuteNonQuery();

                        ////Get newly created location ID
                        //string sLocationSQL = $"Select RouteLocationID from routeLocation where NAME = @Name";
                        //command.Parameters.Clear();
                        //parameter = new SqlParameter
                        //{
                        //    ParameterName = "@Name",
                        //    Value = LocationName,
                        //    SqlDbType = SqlDbType.VarChar,
                        //    Size = 50
                        //};
                        //command.Parameters.Add(parameter);
                        //command.CommandText = sLocationSQL;

                        //using (SqlDataReader dr = command.ExecuteReader())
                        //{
                        //    while (dr.Read())
                        //    {
                        //        sLocationID = Convert.ToString(dr["LocationID"]);
                        //    }
                        //}
                    }
                    catch (Exception ex)
                    {
                        sErrorMsg = ex.Message;
                    }
                    connection.Close();
                }
               //return sLocationID;
            }
        }

        //------------------------
        // Route Planning logic
        //------------------------
        internal static void CreateRoutePlan(string connectionString, RoutePlan routePlan, string sUserID, ref string sErrorMsg)
        {
            string sPLanningID = GetRouteStatusID(connectionString, "Planning");
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string sql = "Insert Into RoutePlan (RoutePlanID, Description, RouteID, RoutePlanStatusID, UserID) Values (@RoutePlanID, @Description, @RouteID, @RoutePlanStatusID, @UserID )";

                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    command.CommandType = CommandType.Text;

                    // adding parameters
                    SqlParameter parameter = new SqlParameter
                    {
                        ParameterName = "@RoutePlanID",
                        Value = routePlan.RoutePlanID,
                        SqlDbType = SqlDbType.VarChar,
                    };
                    command.Parameters.Add(parameter);

                    parameter = new SqlParameter
                    {
                        ParameterName = "@Description",
                        Value = routePlan.Description,
                        SqlDbType = SqlDbType.VarChar
                    };
                    command.Parameters.Add(parameter);

                    parameter = new SqlParameter
                    {
                        ParameterName = "@RouteID",
                        Value = routePlan.RouteID,
                        SqlDbType = SqlDbType.VarChar
                    };
                    command.Parameters.Add(parameter);

                    parameter = new SqlParameter
                    {
                        ParameterName = "@RoutePlanStatusID",
                        Value = sPLanningID,
                        SqlDbType = SqlDbType.VarChar
                    };
                    command.Parameters.Add(parameter);
                    parameter = new SqlParameter
                    {
                        ParameterName = "@UserID",
                        Value = sUserID,
                        SqlDbType = SqlDbType.VarChar
                    };
                    //command.Parameters.Add(parameter);

                    try
                    {
                        command.Parameters.Add(parameter);
                        connection.Open();
                        command.ExecuteNonQuery();

                    }
                    catch (Exception ex)
                    {
                        sErrorMsg = "Unable to create Route Plan. " + ex.Message;
                    }
                    if (connection.State == ConnectionState.Open)
                    {
                        connection.Close();
                    }
                }
            }
        }
        internal static void CreateRoutePlanDetails(string connectionString, string sRouteID, string sRoutePlanID, string sUserID, ref string sErrorMsg)
        {
            List<RoutePlanDetail> lRoutePlanDetails = new List<RoutePlanDetail>();
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                string sql = $"select distinct" +
                             $"   lp.Description, " +
                             $"   rl.LocationID, " +
                             $"   lp.LocationPlacementID, " +
                             $"   lp.SizeID, " +
                             $"   rl.RouteOrder, " +
                             $"   lp.Quantity, " +
                             $"   ai.ArrangementID  " +
                             $" from routeLocation rl  " +
                             $" join locationPlacement lp on rl.LocationID = lp.LocationID  and lp.UserID = @UserID" +
                             $" left outer join ArrangementInventory ai on ai.LocationPlacementID = lp.LocationPlacementID and ai.LocationID = lp.LocationID and ai.UserID = @UserID" +
                             $" where rl.routeID = @RouteID " +
                             $" and rl.UserID = @UserID ";
                SqlCommand command = new SqlCommand(sql, connection);
                // adding parameters
                SqlParameter parameter = new SqlParameter
                {
                    ParameterName = "@RouteID",
                    Value = sRouteID,
                    SqlDbType = SqlDbType.VarChar
                };
                command.Parameters.Add(parameter);
                parameter = new SqlParameter
                {
                    ParameterName = "@UserID",
                    Value = sUserID,
                    SqlDbType = SqlDbType.VarChar
                };
                command.Parameters.Add(parameter);
                SqlDataReader reader = command.ExecuteReader();

                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        RoutePlanDetail plandetail = new RoutePlanDetail();
                        plandetail.RoutePlanID = sRoutePlanID;
                        plandetail.LocationID = Convert.ToString(reader["LocationID"]);
                        plandetail.LocationPlacementID = Convert.ToString(reader["LocationPlacementID"]);
                        plandetail.RouteOrder = Convert.ToInt32(reader["RouteOrder"]);
                        plandetail.SizeID = Convert.ToString(reader["SizeID"]);
                        plandetail.Quantity = Convert.ToInt32(reader["Quantity"]);
                        plandetail.OutgoingArrangementID = Convert.ToString(reader["ArrangementID"]);
                        plandetail.UserID = sUserID;
                        //plandetail.OutgoingArrangementInventoryID = Convert.ToString(reader["ArrangementInventoryID"]);

                        CreatePlanDetailRow(connectionString, plandetail);
                        lRoutePlanDetails.Add(plandetail);
                    }
                }
                connection.Close();

            }


            //List<RoutePlanDetail> lRoutePlanDetails = new List<RoutePlanDetail>();
            //using (SqlConnection connection = new SqlConnection(connectionString))
            //{
            //    connection.Open();
            //    string sql = $"select " +
            //                 $"   ai.Code, " +
            //                 $"   lp.Description, " +
            //                 $"   rl.LocationID, " +
            //                 $"   lp.LocationPlacementID, " +
            //                 $"   lp.SizeID, " +
            //                 $"   rl.RouteOrder, " +
            //                 $"   ai.ArrangementInventoryID  " +
            //                 $" from routeLocation rl  " +
            //                 $" join locationPlacement lp on rl.LocationID = lp.LocationID  " +
            //                 $" join ArrangementInventory ai on ai.LocationPlacementID = lp.LocationPlacementID and ai.LocationID = lp.LocationID " +
            //                 $" where rl.routeID = @RouteID";
            //    SqlCommand command = new SqlCommand(sql, connection);
            //    // adding parameters
            //    SqlParameter parameter = new SqlParameter
            //    {
            //        ParameterName = "@RouteID",
            //        Value = sRouteID,
            //        SqlDbType = SqlDbType.VarChar
            //    };
            //    command.Parameters.Add(parameter);
            //    SqlDataReader reader = command.ExecuteReader();

            //    if (reader.HasRows)
            //    {
            //        while (reader.Read())
            //        {
            //            RoutePlanDetail plandetail = new RoutePlanDetail();
            //            plandetail.RoutePlanID = sRoutePlanID;
            //            plandetail.LocationID = Convert.ToString(reader["LocationID"]);
            //            plandetail.LocationPlacementID = Convert.ToString(reader["LocationPlacementID"]);
            //            plandetail.RouteOrder = Convert.ToInt32(reader["RouteOrder"]);
            //            plandetail.SizeID = Convert.ToString(reader["SizeID"]);
            //            plandetail.OutgoingArrangementInventoryID = Convert.ToString(reader["ArrangementInventoryID"]);

            //            CreatePlanDetailRow(connectionString, plandetail);
            //            lRoutePlanDetails.Add(plandetail);
            //        }
            //    }
            //    connection.Close();

            //}
        }
        private static void CreatePlanDetailRow(string connectionString, RoutePlanDetail plandetail)
        {
            string sLocationID = string.Empty;
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string sRoutePlanDetailID = GetNewID(connectionString);
                string sql = $"Insert Into RoutePlanDetail " +
                             $"        ( RoutePlanDetailID,  RoutePlanID,  LocationID,  LocationPlacementID,  RouteOrder,  SizeID,  Quantity, UserID ) " +
                             $" Values (@RoutePlanDetailID, @RoutePlanID, @LocationID, @LocationPlacementID, @RouteOrder, @SizeID, @Quantity, @UserID) ";

                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    command.CommandType = CommandType.Text;
                    // adding parameters
                    SqlParameter parameter = new SqlParameter
                    {
                        ParameterName = "@RoutePlanDetailID",
                        Value = sRoutePlanDetailID,
                        SqlDbType = SqlDbType.VarChar
                    };
                    command.Parameters.Add(parameter);

                    parameter = new SqlParameter
                    {

                        ParameterName = "@RoutePlanID",
                        Value = plandetail.RoutePlanID,
                        SqlDbType = SqlDbType.VarChar
                    };
                    command.Parameters.Add(parameter);

                    parameter = new SqlParameter
                    {
                        ParameterName = "@LocationID",
                        Value = plandetail.LocationID,
                        SqlDbType = SqlDbType.VarChar
                    };
                    command.Parameters.Add(parameter);

                    parameter = new SqlParameter
                    {
                        ParameterName = "@LocationPlacementID",
                        Value = plandetail.LocationPlacementID,
                        SqlDbType = SqlDbType.VarChar
                    };
                    command.Parameters.Add(parameter);

                    parameter = new SqlParameter
                    {
                        ParameterName = "@RouteOrder",
                        Value = plandetail.RouteOrder,
                        SqlDbType = SqlDbType.Int
                    };
                    command.Parameters.Add(parameter);

                    parameter = new SqlParameter
                    {
                        ParameterName = "@SizeID",
                        Value = plandetail.SizeID,
                        SqlDbType = SqlDbType.VarChar
                    };
                    command.Parameters.Add(parameter);

                    parameter = new SqlParameter
                    {
                        ParameterName = "@Quantity",
                        Value = plandetail.Quantity,
                        SqlDbType = SqlDbType.Int
                    };
                    command.Parameters.Add(parameter);
                    parameter = new SqlParameter
                    {
                        ParameterName = "@UserID",
                        Value = plandetail.UserID,
                        SqlDbType = SqlDbType.VarChar
                    };
                    command.Parameters.Add(parameter);
                    //parameter = new SqlParameter
                    //{
                    //    ParameterName = "@OutgoingArrangementID",
                    //    Value = plandetail.OutgoingArrangementID,
                    //    SqlDbType = SqlDbType.VarChar
                    //};
                    //command.Parameters.Add(parameter);
                    try
                    {
                        connection.Open();
                        command.ExecuteNonQuery();
                        connection.Close();
                    }
                    catch (Exception ex)
                    {
                        string sMsg = ex.Message;
                    }
                }

                List<string> lOutgoingArrangementInventoryID = GetCurrentInventoryIDs(connectionString, plandetail.LocationPlacementID, plandetail.LocationID, plandetail.OutgoingArrangementID);
                // create a detail inventory record for each in the quanty  create blank ones if no outgoing inventory
                if (lOutgoingArrangementInventoryID.Count == 0)
                {
                    string detailInventorySQL = $"Insert into RoutePlanDetailInventory " +
                                                $"       ( RoutePlanDetailID ) " +
                                                $"Values (@RoutePlanDetailID ) ";
                    for (int i = 0; i < plandetail.Quantity; i++)
                    {
                        using (SqlCommand command = new SqlCommand(detailInventorySQL, connection))
                        {
                            command.CommandType = CommandType.Text;
                            // adding parameters
                            SqlParameter parameter = new SqlParameter
                            {
                                ParameterName = "@RoutePlanDetailID",
                                Value = sRoutePlanDetailID,
                                SqlDbType = SqlDbType.VarChar
                            };
                            command.Parameters.Add(parameter);

                            connection.Open();
                            command.ExecuteNonQuery();
                            connection.Close();
                        }
                    }
                }
                else
                {
                    foreach (string OutgoingArrangementInventoryID in lOutgoingArrangementInventoryID)
                    {
                        string detailInventorySQL = $"Insert into RoutePlanDetailInventory " +
                                                    $"       ( RoutePlanDetailID,  OutgoingArrangementInventoryID ) " +
                                                    $"Values (@RoutePlanDetailID, @OutgoingArrangementInventoryID ) ";
                        using (SqlCommand command = new SqlCommand(detailInventorySQL, connection))
                        {
                            command.CommandType = CommandType.Text;
                            // adding parameters
                            SqlParameter parameter = new SqlParameter
                            {
                                ParameterName = "@RoutePlanDetailID",
                                Value = sRoutePlanDetailID,
                                SqlDbType = SqlDbType.VarChar
                            };
                            command.Parameters.Add(parameter);

                            parameter = new SqlParameter
                            {
                                ParameterName = "@OutgoingArrangementInventoryID",
                                Value = OutgoingArrangementInventoryID,
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


            //string sLocationID = string.Empty;
            //using (SqlConnection connection = new SqlConnection(connectionString))
            //{
            //    string sql = $"Insert Into RoutePlanDetail " +
            //                 $"        ( RoutePlanID,  LocationID,  LocationPlacementID,  RouteOrder,  SizeID,  OutgoingArrangementInventoryID) " +
            //                 $" Values (@RoutePlanID, @LocationID, @LocationPlacementID, @RouteOrder, @SizeID, @OutgoingArrangementInventoryID) ";

            //    using (SqlCommand command = new SqlCommand(sql, connection))
            //    {
            //        command.CommandType = CommandType.Text;
            //          // adding parameters
            //        SqlParameter parameter = new SqlParameter
            //        {
            //            ParameterName = "@RoutePlanID",
            //            Value = plandetail.RoutePlanID,
            //            SqlDbType = SqlDbType.VarChar
            //        };
            //        command.Parameters.Add(parameter);

            //        parameter = new SqlParameter
            //        {
            //            ParameterName = "@LocationID",
            //            Value = plandetail.LocationID,
            //            SqlDbType = SqlDbType.VarChar
            //        };
            //        command.Parameters.Add(parameter);

            //        parameter = new SqlParameter
            //        {
            //            ParameterName = "@LocationPlacementID",
            //            Value = plandetail.LocationPlacementID,
            //            SqlDbType = SqlDbType.VarChar
            //        };
            //        command.Parameters.Add(parameter);

            //        parameter = new SqlParameter
            //        {
            //            ParameterName = "@RouteOrder",
            //            Value = plandetail.RouteOrder,
            //            SqlDbType = SqlDbType.Int
            //        };
            //        command.Parameters.Add(parameter);

            //        parameter = new SqlParameter
            //        {
            //            ParameterName = "@SizeID",
            //            Value = plandetail.SizeID,
            //            SqlDbType = SqlDbType.VarChar
            //        };
            //        command.Parameters.Add(parameter);

            //        parameter = new SqlParameter
            //        {
            //            ParameterName = "@OutgoingArrangementInventoryID",
            //            Value = plandetail.OutgoingArrangementInventoryID,
            //            SqlDbType = SqlDbType.VarChar
            //        };
            //        command.Parameters.Add(parameter);

            //        connection.Open();
            //        command.ExecuteNonQuery();
            //        connection.Close();
            //    }
            //}
        }

        private static List<string> GetCurrentInventoryIDs(string connectionString, string LocationPlacementID, string LocationID, string OutgoingArrangementID)
        {
            List<string> lPlacementInventoryID = new List<string>();
            if (String.IsNullOrEmpty(OutgoingArrangementID))
                return lPlacementInventoryID;

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string sql = $" Select ArrangementInventoryID " +
                             $" From ArrangementInventory " +
                             $" Where LocationPlacementID = @LocationPlacementID " +
                             $" and   LocationID = @LocationID " +
                             $" and   ArrangementID = @OutgoingArrangementID " +
                             $" and   Deleted = 'N' ";
                SqlCommand command = new SqlCommand(sql, connection);
                SqlParameter parameter = new SqlParameter
                {
                    ParameterName = "@LocationPlacementID",
                    Value = LocationPlacementID,
                    SqlDbType = SqlDbType.VarChar
                };
                command.Parameters.Add(parameter);

                parameter = new SqlParameter
                {
                    ParameterName = "@LocationID",
                    Value = LocationID,
                    SqlDbType = SqlDbType.VarChar
                };
                command.Parameters.Add(parameter);

                parameter = new SqlParameter
                {
                    ParameterName = "@OutgoingArrangementID",
                    Value = OutgoingArrangementID,
                    SqlDbType = SqlDbType.VarChar
                };
                command.Parameters.Add(parameter);

                connection.Open();
                using (SqlDataReader dr = command.ExecuteReader())
                {
                    while (dr.Read())
                    {
                        string sID = Convert.ToString(dr["ArrangementInventoryID"]);
                        lPlacementInventoryID.Add(sID);
                    }

                }
                connection.Close();

            }
            return lPlacementInventoryID;
        }

        internal static void PopulateIncoming(string connectionString, string routePlanID, string sUserID, ref string sErrorMsg)
        {
            string sIncomingArrangement = string.Empty;
            // read all of the stops.
            // Loop through each stop;
            //   try to find an arrangement that is currently being removed first
            //   find an arrangement from available
            List<RoutePlanDetail> lStops = new List<RoutePlanDetail>();

            // read all of the stops.
            // lStops = GetRoutePlanDetails(connectionString, routePlanID);
            lStops = GetRoutePlanStops(connectionString, routePlanID, sUserID, ref sErrorMsg);
            if (!String.IsNullOrEmpty(sErrorMsg))
            {
                return;
            }
            // Loop through each stop;
            foreach (RoutePlanDetail stop in lStops)
            {
                //   try to find an arrangement that is currently being removed first
                bool bUsePreviousArrangement = FindFromRoute(connectionString, stop, routePlanID, sUserID, ref sErrorMsg);


                if (!bUsePreviousArrangement)
                {
                    //   find an arrangement from available
                    FindFromInventory(connectionString, stop, routePlanID, sUserID, ref sErrorMsg);
                }

            }
        }

        private static void FindFromInventory(string connectionString, RoutePlanDetail oCurrentStop, string routePlanID, string sUserID, ref string sErrorMsg)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {

                // get a list of matching deail records using the same size arrangments as the currrent Stop
                connection.Open();
                string sql = $" select " +
                             $"        a.ArrangementID           ArrangementID, " +
                             $"        count(*)                  AvailableInventory," +
                             $"        ai.LastUsed, " +
                             $"        h.StartDate " +
                             $" from Arrangement a " +
                             $" join ArrangementInventory ai on ai.ArrangementID = a.ArrangementID  and ai.UserID = @UserID" +
                             $" left outer join CustomerInventoryHistory h on h.arrangementID = a.arrangementID  and h.LocationID = @LocationID and h.UserID = @UserID" +
                             $" where a.SizeID = @SizeID " +
                             $" and ai.InventoryStatusID in (Select InventoryStatusID from InventoryStatus" +
                             $"                             where Code in ('Available')" +
                             $"                             and a.ArrangementID not in (select x.ArrangementID " +
                             $"                                                              from ArrangementInventory x " +
                             $"                                                              join routePlanDetailInventory rpdi on rpdi.OutGoingArrangementInventoryID = x.ArrangementInventoryID  " +
                             $"                                                              join routePlanDetail rpd on rpd.RoutePlanDetailID = rpdi.RoutePlanDetailID and rpd.UserID = @UserID" +
                             $"                                                              and rpd.routeOrder = @RouteOrder " +
                             $"                                                              where SizeID = @SizeID " +
                             $"                                                              and rpd.RoutePlanID = @RoutePlanID " +
                             $"                                                              and x.Deleted = 'N' " +
                             $"                                                              and x.UserID = @UserID  " +
                             $"                                                         union " +
                             $"                                                            select x.ArrangementID " +
                             $"                                                            from ArrangementInventory x " +
                             $"                                                            join routePlanDetailInventory rpdi on rpdi.IncomingArrangementInventoryID = x.ArrangementInventoryID  " +
                             $"                                                            join routePlanDetail rpd on rpd.RoutePlanDetailID = rpdi.RoutePlanDetailID and rpd.UserID = @UserID " +
                             $"                                                            join arrangement a on a.ArrangementID = x.ArrangementId and a.UserID = @UserID " +
                             $"                                                            where rpd.SizeID = @SizeID " +
                             $"                                                            and rpd.routeOrder = @RouteOrder " +
                             $"                                                            and rpd.RoutePlanID = @RoutePlanID " +
                             $"                                                            and x.Deleted = 'N' " +
                             $"                                                            and x.UserID = @UserID  " +
                             $"                                                         ) " +
                             $"                             ) " +
                             $" and a.UserID = @UserID " +
                             $" group by  a.ArrangementID, h.StartDate, LastUsed " +
                             $" having count(*) >=  @Quantity" +
                             $" order by h.StartDate asc, LastUsed asc; ";

                SqlCommand command = new SqlCommand(sql, connection);
                SqlParameter parameter = new SqlParameter
                {
                    ParameterName = "@SizeID",
                    Value = oCurrentStop.SizeID,
                    SqlDbType = SqlDbType.VarChar
                };
                command.Parameters.Add(parameter);

                parameter = new SqlParameter
                {
                    ParameterName = "@RoutePlanID",
                    Value = oCurrentStop.RoutePlanID,
                    SqlDbType = SqlDbType.VarChar
                };
                command.Parameters.Add(parameter);

                parameter = new SqlParameter
                {
                    ParameterName = "@RouteOrder",
                    Value = oCurrentStop.RouteOrder,
                    SqlDbType = SqlDbType.Int
                };
                command.Parameters.Add(parameter);

                parameter = new SqlParameter
                {
                    ParameterName = "@LocationID",
                    Value = oCurrentStop.LocationID,
                    SqlDbType = SqlDbType.VarChar
                };
                command.Parameters.Add(parameter);

                parameter = new SqlParameter
                {
                    ParameterName = "@Quantity",
                    Value = oCurrentStop.Quantity,
                    SqlDbType = SqlDbType.Int
                };
                command.Parameters.Add(parameter);
                parameter = new SqlParameter
                {
                    ParameterName = "@UserID",
                    Value = sUserID,
                    SqlDbType = SqlDbType.VarChar
                };
                command.Parameters.Add(parameter);

                SqlDataReader reader = command.ExecuteReader();

                // Get a list of Detail rows who match in size, that could be a potential match for the 
                // current stop.  
                string sArrangementID = string.Empty;
                bool bFoundInventoryIDs = false;
                //string sInventoryID = string.Empty;
                while (reader.Read())
                {
                        
                    sArrangementID = Convert.ToString(reader["ArrangementID"]);
                    //See if this arrangement is valid for placement at oCurrentStop.
                    if (IsValidForCurrentStop(connectionString, oCurrentStop, sArrangementID, sUserID, ref sErrorMsg))
                    {

                        //TODO - Lookup the Inventory ID to be used 
                        List<string> lInventoryIDs = GetAvailableInventoryIDs(connectionString, oCurrentStop.Quantity, sArrangementID, sUserID, ref sErrorMsg);
                        foreach (string sInventoryID in lInventoryIDs)
                        {
                            // set the Incoming ArrangementID to the ArrangementID
                            AddIncomingtoPlanDetail(connectionString, oCurrentStop, sInventoryID, sUserID,Dispositions.FromWarehouse, true);
                            bFoundInventoryIDs = true;
                        }
                        if (bFoundInventoryIDs)
                            break;
                    }
                }
            connection.Close();
            }
        }

        private static List<string> GetAvailableInventoryIDs(string connectionString, int iQuantity, string? sArrangementID, string sUserID, ref string sErrorMsg)
        {
            List<string> lArrangementInventoryIDs = new List<string>();

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                string sql = $" SELECT TOP " + iQuantity.ToString() + " " +
                             $" ai.ArrangementInventoryID " +
                             $" FROM ArrangementInventory ai " +
                             $" WHERE ArrangementID = @ArrangementID " +
                             $" AND ai.Deleted = 'N' " +
                             $" AND ai.InventoryStatusID = (Select InventoryStatusID " +
                             $"                             from inventoryStatus " +
                             $"                             where Description = 'Available') " +
                             $" AND ai.UserID = @UserID " +
                             $" Order by LastUsed Desc";

                SqlCommand command = new SqlCommand(sql, connection);
                SqlParameter parameter = new SqlParameter
                {
                    ParameterName = "@ArrangementID",
                    Value = sArrangementID,
                    SqlDbType = SqlDbType.VarChar
                };
                command.Parameters.Add(parameter);
                parameter = new SqlParameter
                {
                    ParameterName = "@UserID",
                    Value = sUserID,
                    SqlDbType = SqlDbType.VarChar
                };
                command.Parameters.Add(parameter);
                SqlDataReader reader = command.ExecuteReader();
                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        string sArrangementInventoryID = Convert.ToString(reader["ArrangementInventoryID"]);
                        lArrangementInventoryIDs.Add(sArrangementInventoryID);
                    }
                }
            }
            return lArrangementInventoryIDs;
        }

        private static bool FindFromRoute(string connectionString, RoutePlanDetail oCurrentStop, string sRoutePlanID, string sUserID, ref string sErrorMsg)
        {
            bool bRetValue = false;

            // Get the start date end end dates for the most recent entry in the history table for that arrangement
            // at that locations.
            // Then if the EndDate is null, that arrangment is currently at the site, then exit
            // else check to see if the end date is older than X months old.  If is older than X months
            // then it can be used so return that arrangement to be used.
            // 
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                
                // get a list of matching deail records using the same size arrangments as the currrent Stop
                connection.Open();
                string sql = $" select  ai.ArrangementID      ArrangementID, " +
                             $"        rpdi.OutgoingArrangementInventoryID, " +
                             $"        rpd.RoutePlanDetailID RoutePlanDetailID, " +
                             $"        rpd.Quantity " +
                             $" from routeplandetail rpd " +
                             $" join routeplandetailInventory rpdi on rpdi.routePlanDetailID = rpd.RoutePlanDetailID" +
                             $" join ArrangementInventory ai on ai.ArrangementInventoryID = rpdi.OutGoingArrangementInventoryID and ai.UserID = @UserID" +
                             $" join Arrangement a on a.ArrangementID = ai.ArrangementID and a.UserID = @UserID" +
                             $" where " +
                             $" rpd.SizeID = @SizeID " +
                             $" and rpd.RouteOrder < @RouteOrder " +
                             $" and rpd.routePlanID = @RoutePlanID " +
                             $" and rpd.Quantity >= @Quantity " +
                             $" and ai.ArrangementID not in (" +
                             $"                             select aix.ArrangementID " +
                             $"                             from routeplandetail rpd2  " +
                             $"                             join ArrangementInventory aix on aix.ArrangementInventoryID = rpd2.OutGoingArrangementInventoryID " +
                             $"                             where rpd2.RouteOrder = @RouteOrder " +
                             $"                             and rpd2.routePlanID =  @RoutePlanID " +
                             $"                             and rpd2.SizeID = @SizeID " +
                             $"                             and rpd2.UserID = @UserID " +
                             $"                           ) " +
                             $" and rpd.OutgoingDisposition is null " +
                             $" and rpd.UserID = @UserID " +
                             $" Order by rpd.RouteOrder, LastUsed ";

                SqlCommand command = new SqlCommand(sql, connection);
                SqlParameter parameter = new SqlParameter
                {
                    ParameterName = "@SizeID",
                    Value = oCurrentStop.SizeID,
                    SqlDbType = SqlDbType.VarChar
                };
                command.Parameters.Add(parameter);

                parameter = new SqlParameter
                {
                    ParameterName = "@RoutePlanID",
                    Value = oCurrentStop.RoutePlanID,
                    SqlDbType = SqlDbType.VarChar
                };
                command.Parameters.Add(parameter);

                parameter = new SqlParameter
                {
                    ParameterName = "@RouteOrder",
                    Value = oCurrentStop.RouteOrder,
                    SqlDbType = SqlDbType.Int
                };
                command.Parameters.Add(parameter);
                parameter = new SqlParameter
                {
                    ParameterName = "@Quantity",
                    Value = oCurrentStop.Quantity,
                    SqlDbType = SqlDbType.Int
                };
                command.Parameters.Add(parameter);
                parameter = new SqlParameter
                {
                    ParameterName = "@CurrentArrangementID",
                    Value = oCurrentStop.OutgoingArrangementID,
                    SqlDbType = SqlDbType.VarChar
                };
                command.Parameters.Add(parameter);
                parameter = new SqlParameter
                {
                    ParameterName = "@UserID",
                    Value = sUserID,
                    SqlDbType = SqlDbType.VarChar
                };
                command.Parameters.Add(parameter);
                SqlDataReader reader = command.ExecuteReader();

                // Get a list of Detail rows who match in size, that could be a potential match for the 
                // current stop.  
                if (reader.HasRows)
                {
                    string sPotentialTransferArrangementID = String.Empty;
                    string sPotentialTransferRoutePlanDetailID = String.Empty;
                    string sPotentialOutgoingInventoryID = string.Empty;
                    while (reader.Read())
                    {
                        sPotentialTransferArrangementID = Convert.ToString(reader["ArrangementID"]);
                        sPotentialTransferRoutePlanDetailID = Convert.ToString(reader["RoutePlanDetailID"]);
                        sPotentialOutgoingInventoryID = Convert.ToString(reader["OutgoingArrangementInventoryID"]);
                        //See if this arrangement is valid for placement at oCurrentStop.
                        if (IsValidForCurrentStop(connectionString, oCurrentStop, sPotentialTransferArrangementID, sUserID, ref sErrorMsg))
                        {
                            // set the Incoming ArrangementID to the ArrangementID
                            AddIncomingtoPlanDetail(connectionString, oCurrentStop, sPotentialOutgoingInventoryID, sUserID, Dispositions.TransferFrom, false);

                            // set the Disposition of found RoutePlanDetail to "Transferred"
                            UpdateTranferredDetail(connectionString, sPotentialTransferRoutePlanDetailID, sPotentialOutgoingInventoryID);
                            bRetValue = true;
                        }
                    }
                }

            }

            return bRetValue;
              
        }

        private static void UpdateTranferredDetail(string connectionString, string? sPotentialTransferRoutePlanDetailID,  string ? sOutgoingInventoryID)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {

                // string sql = "Insert Into Location (Name, Description, LocationTypeID) Values (@Name, @Description, @LocationTypeID)";
                string sql = $" Update RoutePlanDetailInventory " +
                             $" SET OutgoingDisposition = @OutgoingDisposition " +
                             $" Where RoutePlanDetailId = @RoutePlanDetailId " +
                             $"   and OutGoingArrangementInventoryID = @OutgoingInventoryID";
                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    command.CommandType = CommandType.Text;
                    // adding parameters
                    SqlParameter parameter = new SqlParameter
                    {
                        ParameterName = "@OutgoingDisposition",
                        Value = Dispositions.TransferTo,
                        SqlDbType = SqlDbType.VarChar
                    };
                    command.Parameters.Add(parameter);

                    parameter = new SqlParameter
                    {
                        ParameterName = "@OutgoingInventoryID",
                        Value = sOutgoingInventoryID,
                        SqlDbType = SqlDbType.VarChar
                    };

                    command.Parameters.Add(parameter);
                    parameter = new SqlParameter
                    {
                        ParameterName = "@RoutePlanDetailId",
                        Value = sPotentialTransferRoutePlanDetailID,
                        SqlDbType = SqlDbType.VarChar
                    };
                    command.Parameters.Add(parameter);

                    connection.Open();
                    command.ExecuteNonQuery();
                    connection.Close();
                }
            }
        }

        private static void AddIncomingtoPlanDetail(string connectionString, RoutePlanDetail oCurrentStop, string sArrangmentInventoryID, string sUserID, string IncomingDisposition, bool bUpdateInventoryStatus)
        {
            string sRoutPlanDetailInventoryID = string.Empty;
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                // Update Route Detail Inventory with the selected InventoryID
                string sqlDetail = $" Select top 1 RoutePlanDetailInventoryID " +
                             $" FROM RoutePlanDetailInventory " +
                             $" WHERE RoutePlanDetailID = @RoutePlanDetailID " +
                             $" AND UserID = @UserID " +
                             $" AND IncomingArrangementInventoryID is NULL ";
                using (SqlCommand rpdiCommand = new SqlCommand(sqlDetail, connection))
                {
                    rpdiCommand.CommandType = CommandType.Text;
                    SqlParameter parameter = new SqlParameter
                    {
                        ParameterName = "@RoutePlanDetailID",
                        Value = oCurrentStop.RoutePlanDetailID,
                        SqlDbType = SqlDbType.VarChar
                    };
                    rpdiCommand.Parameters.Add(parameter);

                    parameter = new SqlParameter
                    {
                        ParameterName = "@UserID",
                        Value = sUserID,
                        SqlDbType = SqlDbType.VarChar
                    };
                    rpdiCommand.Parameters.Add(parameter);
                    connection.Open();
                    SqlDataReader reader = rpdiCommand.ExecuteReader();
                    while (reader.Read())
                    {
                        sRoutPlanDetailInventoryID = Convert.ToString(reader["RoutePlanDetailInventoryID"]);
                    }
                    connection.Close();
                }

                string sqlUpdtDetailInv = $" Update RoutePlanDetailInventory " +
                                          $" SET IncomingArrangementInventoryID = @IncomingArrangementInventoryID," +
                                          $" IncomingDisposition = @IncomingDisposition " +
                                          $" Where RoutePlanDetailInventoryID = @RoutePlanDetailInventoryID ";
                using (SqlCommand command = new SqlCommand(sqlUpdtDetailInv, connection))
                {
                    command.CommandType = CommandType.Text;
                    // adding parameters
                    SqlParameter parameter = new SqlParameter
                    {
                        ParameterName = "@IncomingArrangementInventoryID",
                        Value = sArrangmentInventoryID,
                        SqlDbType = SqlDbType.VarChar
                    };
                    command.Parameters.Add(parameter);

                    parameter = new SqlParameter
                    {
                        ParameterName = "@RoutePlanDetailInventoryID",
                        Value = sRoutPlanDetailInventoryID,
                        SqlDbType = SqlDbType.VarChar
                    };
                    command.Parameters.Add(parameter);

                    parameter = new SqlParameter
                    {
                        ParameterName = "@IncomingDisposition",
                        Value = IncomingDisposition,
                        SqlDbType = SqlDbType.VarChar
                    };
                    command.Parameters.Add(parameter);
                    connection.Open();
                    command.ExecuteNonQuery();
                    connection.Close();

                }

                //string sql = $"Update RoutePlanDetail SET IncomingArrangementInventoryID =@IncomingArrangementInventoryID, IncomingDisposition=@IncomingDisposition Where RoutePlanDetailId=@RoutePlanDetailId";
                //using (SqlCommand command = new SqlCommand(sql, connection))
                //{
                //    command.CommandType = CommandType.Text;
                //     // adding parameters
                //    SqlParameter parameter = new SqlParameter
                //    {
                //        ParameterName = "@IncomingArrangementInventoryID",
                //        Value = sArrangmentInventoryID,
                //        SqlDbType = SqlDbType.VarChar
                //    };
                //    command.Parameters.Add(parameter);

                //    parameter = new SqlParameter
                //    {
                //        ParameterName = "@RoutePlanDetailId",
                //        Value = oCurrentStop.RoutePlanDetailID,
                //        SqlDbType = SqlDbType.VarChar
                //    };
                //    command.Parameters.Add(parameter);

                //    parameter = new SqlParameter
                //    {
                //        ParameterName = "@IncomingDisposition",
                //        Value = IncomingDisposition,
                //        SqlDbType = SqlDbType.VarChar
                //    };
                //    command.Parameters.Add(parameter);
                //    connection.Open();
                //    command.ExecuteNonQuery();
                //    connection.Close();

                //}

                if (bUpdateInventoryStatus)
                {
                    //Update the ArrangmentInventoryStatus to Allocated for the selected Inventory
                    string sStatusSql = $" Update ArrangementInventory " +
                                        $" set InventoryStatusID = (select InventoryStatusID " +
                                        $"                          from " +
                                        $"                          InventoryStatus where Code = 'Allocated') " +
                                        $" where ArrangementInventoryID = @IncomingArrangementInventoryID " +
                                        $" and UserID = @UserID ";

                    using (SqlCommand statuscommand = new SqlCommand(sStatusSql, connection))
                    {

                        statuscommand.Parameters.Clear();
                        SqlParameter parameter = new SqlParameter
                        {
                            ParameterName = "@IncomingArrangementInventoryID",
                            Value = sArrangmentInventoryID,
                            SqlDbType = SqlDbType.VarChar
                        };
                        statuscommand.Parameters.Add(parameter);

                        parameter = new SqlParameter
                        {
                            ParameterName = "@UserID",
                            Value = sUserID,
                            SqlDbType = SqlDbType.VarChar
                        };
                        statuscommand.Parameters.Add(parameter);

                        connection.Open();
                        statuscommand.ExecuteNonQuery();
                        connection.Close();
                    }
                }
            }
        }
        private static bool IsValidForCurrentStop(string connectionString, RoutePlanDetail oCurrentStop, string? sArrangementID, string sUserID, ref string sErrorMsg)
        {
            bool bRetValue = true;
            // get the latest history for the current stop and see if the arrangment has been seen there recently.
            // if so return false.  Otherwise, return true so it can be assigned to the current stop.
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                string sql = $" select " +
                             $"    StartDate                     StartDate, " +
                             $"    EndDate                       EndDate, " +
                             $"    DateAdd(MONTH, -12, GetDate()) ValidAfterDate" +
                             $" from CustomerInventoryHistory CIH " +
                             $" WHERE CIH.LocationID = @LocationID " +
                             $" and CIH.arrangementID = @ArrangementID " +
                             $" AND StartDate = (select  max(x.StartDate) " +
                             $"                  from CustomerInventoryHistory x " +
                             $"                  where x.LocationID = CIH.LocationID " +
                             $"                  and x.ArrangementID = CIH.ArrangementID " +
                             $"                  and x.UserID = @UserID " +
                             $"                  ) " +
                             $" and CIH.EndDate is not null" +
                             $" and CIH.UserID = @UserID ";
                             //$" and EndDate < DateAdd(MONTH, -12, GetDate())";

                SqlCommand command = new SqlCommand(sql, connection);
                SqlParameter parameter = new SqlParameter
                {
                    ParameterName = "@LocationID",
                    Value = oCurrentStop.LocationID,
                    SqlDbType = SqlDbType.VarChar
                };
                command.Parameters.Add(parameter);

                parameter = new SqlParameter
                {
                    ParameterName = "@ArrangementID",
                    Value = sArrangementID,
                    SqlDbType = SqlDbType.VarChar
                };
                command.Parameters.Add(parameter);
                parameter = new SqlParameter
                {
                    ParameterName = "@UserID",
                    Value = sUserID,
                    SqlDbType = SqlDbType.VarChar
                };
                command.Parameters.Add(parameter);
                SqlDataReader reader = command.ExecuteReader();

                while (reader.Read())
                {
                    // we have a history record see if the end date > that the calculated date. If so, 
                    // it is to early to put it back at the location
                    DateTime? dtEndDate = Convert.ToDateTime(reader["EndDate"]);
                    DateTime? dtValidAfterDate = Convert.ToDateTime(reader["ValidAfterDate"]);

                    if (dtEndDate.HasValue)
                    {
                        if (dtEndDate > dtValidAfterDate)
                        {
                            bRetValue = false;
                        }
                        else
                        {
                            bRetValue = true;
                        }
                    }

                }
                connection.Close();
            }
            return bRetValue;
        }

        public static string GetNextInventoryCode(string connectionString, string code, string sUserID)

        {
            string sLastCode = string.Empty;
            string sql = $"select max(Code) LastCode from ArrangementInventory where code like @Code and UserID = @UserID ";
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

                    parameter = new SqlParameter
                    {
                        ParameterName = "@UserID",
                        Value = sUserID,
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
        public static List<Location> GetLocations(string connectionString, string sUserID, ref string sErrorMsg)
        {
            List<Location> locations = new List<Location>();

            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    string sql = " Select LocationID, Name " +
                                 " from Location l " +
                                 " join locationType lt on l.LocationTypeID = lt.LocationTypeID " +
                                 " where l.deleted = 'N' " +
                                 " and l.UserID = @UserID " +
                                 " order by lt.code, Name";
                    SqlCommand cmd = new SqlCommand(sql, connection);
                    SqlParameter parameter = new SqlParameter
                    {
                        ParameterName = "@UserID",
                        Value = sUserID,
                        SqlDbType = SqlDbType.VarChar
                    };
                    cmd.Parameters.Add(parameter);
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

                    locations.Insert(0, new Location { Name = "-- Select Location--", LocationID = "" });
                    connection.Close();
                }
            }
            catch (Exception ex) { }
            finally { }

            return locations;

        }

        public static List<Location> GetLocationsWithSize(string connectionString, string sSizeID, bool bAddWarehouse, string sUserID, ref string sErrorMsg)
        {
            List<Location> locations = new List<Location>();
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                string sql = $"Select Distinct l.LocationID, Name, lt.Code " +
                                $" from Location l" +
                                $" join LocationType lt on lt.LocationTypeID = l.LocationTypeID " +
                                $" join LocationPlacement lp on l.LocationID = lp.LocationID and lp.Deleted = 'N'" +
                                $" where lp.SizeID = @SizeID " +
                                $" and l.deleted = 'N' " +
                                $" and l.UserID = @UserID " +
                                $" and lp.UserID = @UserID " +
                                $" order by lt.Code, Name";
                SqlCommand cmd = new SqlCommand(sql, connection);

                cmd.Parameters.Clear();
                SqlParameter parameter = new SqlParameter
                {
                    ParameterName = "@SizeID",
                    Value = sSizeID,
                    SqlDbType = SqlDbType.VarChar
                };
                cmd.Parameters.Add(parameter);

                parameter = new SqlParameter
                {
                    ParameterName = "@UserID",
                    Value = sUserID,
                    SqlDbType = SqlDbType.VarChar
                };
                cmd.Parameters.Add(parameter);

                try
                {
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
                    if (bAddWarehouse)
                    {

                        IEnumerable<SelectListItem> lWarehouses = GetWarehouses(connectionString, sUserID, false);
                        foreach (SelectListItem item in lWarehouses)
                        {
                            if (item.Value.Trim() != "0")
                            {
                                Location location = new Location();
                                location.LocationID = item.Value;
                                location.Name = item.Text;
                                locations.Add(location);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    sErrorMsg = "Error getting locations by size. " + ex.Message;
                }
                connection.Close();
            }
            return locations;
        }
        public static List<LocationPlacement> GetLocationPlacementList(string? connectionString, string sLocationPlacmentID, string sUserID, ref string sErrorMsg)
        {
            List<LocationPlacement> ivmList = new List<LocationPlacement>();
            if ( !String.IsNullOrEmpty(sLocationPlacmentID) )
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    string sql = "SELECT " +
                        "  la.LocationPlacementID  ID " +
                        " ,la.Description        CODE " +
                        "  FROM LocationPlacement la " +
                        $" where la.LocationID = @LocationPlacementID " +
                        $" and la.UserID = @UserID ";

                    SqlCommand readcommand = new SqlCommand(sql, connection);
                    SqlParameter parameter = new SqlParameter
                    {
                        ParameterName = "@LocationPlacementID",
                        Value = sLocationPlacmentID,
                        SqlDbType = SqlDbType.VarChar
                    };
                    readcommand.Parameters.Add(parameter);

                    parameter = new SqlParameter
                    {
                        ParameterName = "@UserID",
                        Value = sUserID,
                        SqlDbType = SqlDbType.VarChar
                    };
                    readcommand.Parameters.Add(parameter);
                    try
                    {
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
                    }
                    catch (Exception ex)
                    {
                        sErrorMsg = ex.Message;
                    }
                    connection.Close();
                    ivmList.Insert(0, new LocationPlacement { Code = "-- Select Placement --", LocationPlacementID = "0" });
                }
            }
            return ivmList;
        }
        public static List<SelectListItem> GetLocationPlacements(string connectionString, string sLocationID, string sUserID, ref string sErrorMsg)
        {
            List<SelectListItem> list = new List<SelectListItem>();
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    string sql = "SELECT " +
                                 " la.LocationPlacementID  ID " +
                                 " ,la.Description        CODE " +
                                 " FROM LocationPlacement la " +
                                $" where la.LocationID = @LocationID " +
                                $" and UserID = @UserID ";

                    SqlCommand cmd = new SqlCommand(sql, connection);
                    SqlParameter parameter = new SqlParameter
                    {
                        ParameterName = "@LocationID",
                        Value = sLocationID,
                        SqlDbType = SqlDbType.VarChar
                    };
                    cmd.Parameters.Add(parameter);

                    parameter = new SqlParameter
                    {
                        ParameterName = "@UserID",
                        Value = sUserID,
                        SqlDbType = SqlDbType.VarChar
                    };
                    cmd.Parameters.Add(parameter);

                    SqlDataReader reader = cmd.ExecuteReader();
                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            list.Add(new SelectListItem { Text = reader["Code"].ToString(), Value = reader["ID"].ToString() });
                        }
                    }
                    list.Insert(0, new SelectListItem { Text = "-- Select Placement --", Value = "" });
                    connection.Close();
                }
            }
            catch (Exception ex)
            {
                sErrorMsg = ex.Message;
                list.Add(new SelectListItem { Text = ex.Message.ToString(), Value = "0" });
            }

            return list;
        }
        public static List<LocationPlacement> GetLocationPlacementListWithSize(string? connectionString, string sLocationID, string sSizeID, string sUserID, ref string sErrorMsg)
        {
            List<LocationPlacement> ivmList = new List<LocationPlacement>();
            if (!String.IsNullOrEmpty(sLocationID))
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    string sql = $" SELECT " +
                        $" la.LocationPlacementID  ID " +
                        $" ,la.Description        CODE " +
                        $" FROM LocationPlacement la " +
                        $" where la.LocationID= @LocationID " +
                        $" and la.UserID = @UserID " +
                        $" and la.SizeID = @SizeID";

                    SqlCommand readcommand = new SqlCommand(sql, connection);
                    readcommand.Parameters.Clear();
                    SqlParameter parameter = new SqlParameter
                    {
                        ParameterName = "@SizeID",
                        Value = sSizeID,
                        SqlDbType = SqlDbType.VarChar
                    };
                    readcommand.Parameters.Add(parameter);
                    parameter = new SqlParameter
                    {
                        ParameterName = "@UserID",
                        Value = sUserID,
                        SqlDbType = SqlDbType.VarChar
                    };
                    readcommand.Parameters.Add(parameter);

                    parameter = new SqlParameter
                    {
                        ParameterName = "@LocationID",
                        Value = sLocationID,
                        SqlDbType = SqlDbType.VarChar
                    };
                    readcommand.Parameters.Add(parameter);

                    try
                    {
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
                            ivmList.Insert(0, new LocationPlacement { Code = "-- Select Placement --", LocationPlacementID = "0" });
                        }
                    }
                    catch (Exception ex)
                    {
                        sErrorMsg = "Erro getting location placments with size." + ex.Message;
                    }
                    connection.Close();
                }
            }
            return ivmList;
        }

        public static void UpdateLocation(string? connectionString, Location location, string sUserID, ref string sErrorMsg)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {

               // string sql = "Insert Into Location (Name, Description, LocationTypeID) Values (@Name, @Description, @LocationTypeID)";
                string sql = $"Update Location SET Name=@Name, Description=@Description Where LocationId=@LocationID and UserID=@UserID";
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

                    parameter = new SqlParameter
                    {
                        ParameterName = "@UserID",
                        Value = sUserID,
                        SqlDbType = SqlDbType.VarChar
                    };
                    command.Parameters.Add(parameter);

                    try
                    {
                        connection.Open();
                        command.ExecuteNonQuery();
                    }
                    catch (Exception ex)
                    {
                        sErrorMsg = "Unable to update location. " + ex.Message;
                    }
                    connection.Close();
                }
            }
        }

        internal static void MoveLocationUp(string? connectionString, int iOldValue, int iNewValue, string sRouteID, string sRouteLocationID, string sUserID, ref string sErrorMsg)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                // string sql = "Insert Into Location (Name, Description, LocationTypeID) Values (@Name, @Description, @LocationTypeID)";
                string sql = $" Update RouteLocation " +
                             $" SET RouteOrder = RouteOrder + 1 " +
                             $" Where RouteId=@RouteID " +
                             $" AND   RouteOrder >= @NewValue " +
                             $" AND   RouteOrder < @OldValue " +
                             $" AND   UserID = @UserID ";

                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    command.CommandType = CommandType.Text;
                    //adding parameters
                    SqlParameter parameter = new SqlParameter
                    {
                        ParameterName = "@RouteID",
                        Value = sRouteID,
                        SqlDbType = SqlDbType.VarChar,

                    };
                    command.Parameters.Add(parameter);
                    parameter = new SqlParameter
                    {
                        ParameterName = "@NewValue",
                        Value = iNewValue,
                        SqlDbType = SqlDbType.Int,

                    };
                    command.Parameters.Add(parameter);
                    parameter = new SqlParameter
                    {
                        ParameterName = "@OldValue",
                        Value = iOldValue,
                        SqlDbType = SqlDbType.Int,

                    };
                    command.Parameters.Add(parameter);

                    parameter = new SqlParameter
                    {
                        ParameterName = "@UserID",
                        Value = sUserID,
                        SqlDbType = SqlDbType.VarChar,

                    };
                    command.Parameters.Add(parameter);
                    command.ExecuteNonQuery();
                }

                sql = $" Update RouteLocation " +
                      $" SET RouteOrder = @NewValue " +
                      $" WHERE RouteLocationID = @RouteLoctionID " +
                      $" AND UserID = @UserID ";
                try
                {
                    using (SqlCommand command = new SqlCommand(sql, connection))
                    {
                        command.CommandType = CommandType.Text;
                        //adding parameters
                        SqlParameter parameter = new SqlParameter
                        {
                            ParameterName = "@RouteLoctionID",
                            Value = sRouteLocationID,
                            SqlDbType = SqlDbType.VarChar,

                        };
                        command.Parameters.Add(parameter);
                        parameter = new SqlParameter
                        {
                            ParameterName = "@NewValue",
                            Value = iNewValue,
                            SqlDbType = SqlDbType.Int,

                        };
                        command.Parameters.Add(parameter);
                        parameter = new SqlParameter
                        {
                            ParameterName = "@UserID",
                            Value = sUserID,
                            SqlDbType = SqlDbType.VarChar,

                        };
                        command.Parameters.Add(parameter);
                        command.ExecuteNonQuery();

                    }
                }
                catch (Exception ex)
                {
                    sErrorMsg = "Unable to move location up." + ex.Message;
                }
                connection.Close();
            }
        }

        internal static void MoveLocationDown(string? connectionString, int iOldValue, int iNewValue, string sRouteID, string sRouteLocationID, string sUserID, ref string sErrorMsg)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                // string sql = "Insert Into Location (Name, Description, LocationTypeID) Values (@Name, @Description, @LocationTypeID)";
                string sql = $" Update RouteLocation " +
                             $" SET RouteOrder = RouteOrder - 1 " +
                             $" Where RouteId = @RouteID " +
                             $" AND   RouteOrder >  @OldValue " +
                             $" AND   RouteOrder <= @NewValue " +
                             $" AND   UserID = @UserID ";

                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    command.CommandType = CommandType.Text;
                    //adding parameters
                    SqlParameter parameter = new SqlParameter
                    {
                        ParameterName = "@RouteID",
                        Value = sRouteID,
                        SqlDbType = SqlDbType.VarChar,

                    };
                    command.Parameters.Add(parameter);
                    parameter = new SqlParameter
                    {
                        ParameterName = "@NewValue",
                        Value = iNewValue,
                        SqlDbType = SqlDbType.Int,

                    };
                    command.Parameters.Add(parameter);
                    parameter = new SqlParameter
                    {
                        ParameterName = "@OldValue",
                        Value = iOldValue,
                        SqlDbType = SqlDbType.Int,

                    };
                    command.Parameters.Add(parameter);
                    parameter = new SqlParameter
                    {
                        ParameterName = "@UserID",
                        Value = sUserID,
                        SqlDbType = SqlDbType.VarChar,

                    };
                    command.Parameters.Add(parameter);
                    command.ExecuteNonQuery();
                }

                sql = $" Update RouteLocation " +
                      $" SET RouteOrder = @NewValue " +
                      $" WHERE RouteLocationID = @RouteLoctionID" +
                      $" AND UserID= @UserID ";

                try
                {
                    using (SqlCommand command = new SqlCommand(sql, connection))
                    {
                        command.CommandType = CommandType.Text;
                        //adding parameters
                        SqlParameter parameter = new SqlParameter
                        {
                            ParameterName = "@RouteLoctionID",
                            Value = sRouteLocationID,
                            SqlDbType = SqlDbType.VarChar,

                        };
                        command.Parameters.Add(parameter);
                        parameter = new SqlParameter
                        {
                            ParameterName = "@NewValue",
                            Value = iNewValue,
                            SqlDbType = SqlDbType.Int,

                        };
                        command.Parameters.Add(parameter);
                        parameter = new SqlParameter
                        {
                            ParameterName = "@UserID",
                            Value = sUserID,
                            SqlDbType = SqlDbType.VarChar,

                        };
                        command.Parameters.Add(parameter);

                        command.ExecuteNonQuery();

                    }
                }
                catch (Exception ex)
                {
                    sErrorMsg = "Can no move lcotion postion down." + ex.Message;
                }
                connection.Close();
            }
        }

        internal static string GetNewID(string connectionString)
        {
            string sRetValue = string.Empty;
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                string sSql = $"Select NEWID() NewID";
                using (SqlCommand command = new SqlCommand(sSql, connection))
                {

                    using (SqlDataReader dr = command.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            sRetValue = Convert.ToString(dr["NewID"]);
                        }
                    }
                }
                connection.Close();
            }
            return sRetValue;
        }
        internal static string GetRouteStatusID(string connectionString, string sCode)
        {
            string sRetValue = string.Empty;
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                string sSql = $"select routePlanStatusID from routePlanStatus where code = @Code;";
                using (SqlCommand command = new SqlCommand(sSql, connection))
                {
                    command.Parameters.Clear();

                    // adding parameters
                    SqlParameter parameter = new SqlParameter
                    {
                        ParameterName = "@Code",
                        Value = sCode,
                        SqlDbType = SqlDbType.VarChar
                    };
                    command.Parameters.Add(parameter);
                    //command.CommandText = sCustomerTypeSQL;

                    using (SqlDataReader dr = command.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            sRetValue = Convert.ToString(dr["routePlanStatusID"]);
                        }
                    }
                }
                connection.Close();
            }
            return sRetValue;
        }

        internal static void SetDestinationStatus(string connectionString, string routePlanID, string sDisposition)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {

                // string sql = "Insert Into Location (Name, Description, LocationTypeID) Values (@Name, @Description, @LocationTypeID)";
                string sql = $" Update RoutePlanDetailInventory " +
                             $" SET OutgoingDisposition = @Disposition " +
                             $" Where OutgoingDisposition is null " +
                             $" and routePlanDetailInventoryID in (select routeplanDetailInventoryID " +
                             $"                                    from routeplandetailInventory i" +
                             $"                                    join routeplandetail d on d.RoutePlanDetailID = i.RoutePlanDetailID " +
                             $"                                    where d.RoutePlanID = @RoutePlanID )";
                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    command.CommandType = CommandType.Text;
                    // adding parameters
                    SqlParameter parameter = new SqlParameter
                    {
                        ParameterName = "@Disposition",
                        Value = sDisposition,
                        SqlDbType = SqlDbType.VarChar
                    };
                    command.Parameters.Add(parameter);


                    parameter = new SqlParameter
                    {
                        ParameterName = "@RoutePlanID",
                        Value = routePlanID,
                        SqlDbType = SqlDbType.VarChar
                    };
                    command.Parameters.Add(parameter);

                    connection.Open();
                    command.ExecuteNonQuery();
                    connection.Close();
                }
            }

        }

        internal static string CreateCustLocHistory(string? connectionString, CustomerInventoryHistory oCustLocHistory, ref string sErrorMsg)
        {
            string sArrangementID = string.Empty;

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                
                string sql = "Insert Into CustomerInventoryHistory (CustomerID,  ArrangementID,  LocationID,  StartDate,  EndDate, UserID) " +
                                                           "Values (@CustomerID, @ArrangementID, @LocationID, @StartDate, @EndDate,@UserID) ";

                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    command.CommandType = CommandType.Text;

                    // adding parameters
                    SqlParameter parameter = new SqlParameter
                    {
                        ParameterName = "@CustomerID",
                        Value = oCustLocHistory.CustomerID,
                        SqlDbType = SqlDbType.VarChar
                    };
                    command.Parameters.Add(parameter);

                    parameter = new SqlParameter
                    {
                        ParameterName = "@ArrangementID",
                        Value = oCustLocHistory.ArrangementID,
                        SqlDbType = SqlDbType.VarChar
                    };
                    command.Parameters.Add(parameter);

                    parameter = new SqlParameter
                    {
                        ParameterName = "@LocationID",
                        Value = oCustLocHistory.LocationID,
                        SqlDbType = SqlDbType.VarChar
                    };
                    command.Parameters.Add(parameter);

                    parameter = new SqlParameter
                    {
                        ParameterName = "@StartDate",
                        Value = oCustLocHistory.StartDate,
                        SqlDbType = SqlDbType.Date
                    };
                    command.Parameters.Add(parameter);

                    parameter = new SqlParameter
                    {
                        ParameterName = "@EndDate",
                        Value = oCustLocHistory.EndDate,
                        SqlDbType = SqlDbType.Date
                    };
                    command.Parameters.Add(parameter);

                    parameter = new SqlParameter
                    {
                        ParameterName = "@UserID",
                        Value = oCustLocHistory.UserID,
                        SqlDbType = SqlDbType.VarChar
                    };
                    command.Parameters.Add(parameter);

                    connection.Open();
                    try
                    {
                        command.ExecuteNonQuery();

                    }
                    catch (Exception ex)
                    {
                        sErrorMsg = "Unable to insert history record." + ex.Message;
                    }
                    finally
                    { connection.Close(); }

                }
            }

            return "Success";
        }
        internal static string UpdateCustLocHistory(string? connectionString, string sUserID, CustomerInventoryHistory oCustLocHistory)
        {
            string sql = $" Update CustomerInventoryHistory " +
                         $" Set StartDate  = @StartDate, " +
                         $"     EndDate    = @EndDate, " +
                         $"  ArrangementID = @ArrangementID " +
                         $" WHERE CustInvHistoryID = @CustInvHistoryID " +
                         $" AND UserID = @UserID ";

            SqlConnection connection = new SqlConnection(connectionString);
            using (SqlCommand command = new SqlCommand(sql, connection))
            {
                command.CommandType = CommandType.Text;

                // adding parameters
                SqlParameter parameter = new SqlParameter
                {
                    ParameterName = "@ArrangementID",
                    Value = oCustLocHistory.ArrangementID,
                    SqlDbType = SqlDbType.VarChar
                };
                command.Parameters.Add(parameter);

                parameter = new SqlParameter
                {
                    ParameterName = "@StartDate",
                    Value = oCustLocHistory.StartDate,
                    SqlDbType = SqlDbType.Date
                };
                command.Parameters.Add(parameter);

                parameter = new SqlParameter
                {
                    ParameterName = "@UserID",
                    Value = sUserID,
                    SqlDbType = SqlDbType.VarChar
                };
                command.Parameters.Add(parameter);
                if (oCustLocHistory.EndDate.ToString("dd/MM/yyyy HH:mm:ss") == "01/01/0001 00:00:00")
                {
                    parameter = new SqlParameter
                    {
                        ParameterName = "@EndDate",
                        Value = DBNull.Value,
                        SqlDbType = SqlDbType.Date
                    };
                }
                else
                {
                    parameter = new SqlParameter
                    {
                        ParameterName = "@EndDate",
                        Value = oCustLocHistory.EndDate,
                        SqlDbType = SqlDbType.Date
                    };
                }
                command.Parameters.Add(parameter);

                parameter = new SqlParameter
                {
                    ParameterName = "@CustInvHistoryID",
                    Value = oCustLocHistory.CustomerHistoryID,
                    SqlDbType = SqlDbType.VarChar
                };
                command.Parameters.Add(parameter);
                
                connection.Open();
                try
                {
                    command.ExecuteNonQuery();

                }
                catch (Exception ex)
                {
                    string msg = ex.Message;
                }
                finally
                { connection.Close(); }


            }
            return "Succes";
        }
        internal static CustomerInventoryHistory GetLocationHistory(string? connectionString, string sUserID, string sCustLocHistoryID)
        {
            string sRetValue = string.Empty;
            CustomerInventoryHistory cih = new CustomerInventoryHistory();
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                string sSQL = $" Select h.CustomerID CustomerID," +
                              $" c.Name              CustName, " +
                              $" h.locationID        LocationID, " +
                              $" l.Name              LocationName, " +
                              $" h.ArrangementID     ArrangementID, " +
                              $" h.StartDate         StartDate, " +
                              $" h.EndDate           EndDate " +
                              $" from CustomerInventoryHistory h " +
                              $" join location l on l.locationID = h.LocationID " +
                              $" join customer c on c.CustomerID = h.CustomerID " +
                              $" where h.CustInvHistoryID = @CustLocHistoryID " +
                              $" and h.UserID = @UserID ";
                using (SqlCommand command = new SqlCommand(sSQL, connection))
                {
                    command.Parameters.Clear();

                    // adding parameters
                    SqlParameter parameter = new SqlParameter
                    {
                        ParameterName = "@CustLocHistoryID",
                        Value = sCustLocHistoryID,
                        SqlDbType = SqlDbType.VarChar
                    };
                    command.Parameters.Add(parameter);

                    parameter = new SqlParameter
                    {
                        ParameterName = "@UserID",
                        Value = sUserID,
                        SqlDbType = SqlDbType.VarChar
                    };
                    command.Parameters.Add(parameter);

                    using (SqlDataReader dr = command.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            cih.ArrangementID = Convert.ToString(dr["ArrangementID"]);
                            cih.CustomerID = Convert.ToString(dr["CustomerID"]);
                            cih.CustomerName = Convert.ToString(dr["CustName"]);
                            cih.LocationID = Convert.ToString(dr["LocationID"]);
                            cih.LocationName = Convert.ToString(dr["LocationName"]);
                            cih.StartDate = Convert.ToDateTime(dr["StartDate"]);
                            if (dr["EndDate"] != DBNull.Value)
                            {
                                cih.EndDate = Convert.ToDateTime(dr["EndDate"]);
                            }
                        }
                    }
                }
                connection.Close();
            }

            return cih;

        }

        internal static bool ValidateLogin(string? connectionString, ref Login credentials)
        {
            bool bRetValue = false;
            Login cih = new Login();
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                string sSQL = $" Select UserId           ID," +
                              $" h.IsAdmin           IsAdmin " +
                              $" from SilkUser h " +
                              $" where h.IsLockedOut = 0 " +
                              $" and UserName = @UserName " +
                              $" and PasswordHash = @Password ";
                using (SqlCommand command = new SqlCommand(sSQL, connection))
                {
                    command.Parameters.Clear();

                    // adding parameters
                    SqlParameter parameter = new SqlParameter
                    {
                        ParameterName = "@UserName",
                        Value = credentials.UserName,
                        SqlDbType = SqlDbType.VarChar
                    };
                    command.Parameters.Add(parameter);

                    parameter = new SqlParameter
                    {
                        ParameterName = "@Password",
                        Value = credentials.PasswordHash,
                        SqlDbType = SqlDbType.VarChar
                    };
                    command.Parameters.Add(parameter);

                    using (SqlDataReader dr = command.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            cih.Id = Convert.ToString(dr["ID"]);
                            credentials.Id = cih.Id;
                            credentials.IsAdmin = Convert.ToBoolean(dr["IsAdmin"]);
                            bRetValue = true;
                        }
                    }
                }
                connection.Close();
            }

            return bRetValue;
        }

        internal static string GetPlacementsLocation(string connectionString, string sPlacmentId, string msUserID, ref string sErrorMsg)
        {
            throw new NotImplementedException();
        }

        internal static DateTime GetStartDate(string? connectionString, string sLocationID, string sUserID)
        {
            DateTime bRetValue = DateTime.Now;
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                string sSQL = $" select IsNull(DateAdd(Month, -1,Min(StartDate)), (getdate() )) PrevMonthStartDate " +
                              $" from CustomerInventoryHistory " +
                              $" where LocationID = @LocationID " +
                              $" and UserID = @UserID ";
                using (SqlCommand command = new SqlCommand(sSQL, connection))
                {
                    command.Parameters.Clear();

                    // adding parameters
                    SqlParameter parameter = new SqlParameter
                    {
                        ParameterName = "@LocationID",
                        Value = sLocationID,
                        SqlDbType = SqlDbType.VarChar
                    };
                    command.Parameters.Add(parameter);

                    parameter = new SqlParameter
                    {
                        ParameterName = "@UserID",
                        Value = sUserID,
                        SqlDbType = SqlDbType.VarChar
                    };
                    command.Parameters.Add(parameter);

                    using (SqlDataReader dr = command.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            bRetValue = Convert.ToDateTime(dr["PrevMonthStartDate"]);
                        }
                    }
                }
                connection.Close();
            }
            return bRetValue;
        }

        internal static IList<SelectListItem> GetCatalogStatus(string? connectionString)
        {
            List<SelectListItem> list = new List<SelectListItem>();
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    string sql = "Select * from CatalogStatus order by SortOrder asc";
                    SqlCommand cmd = new SqlCommand(sql, connection);
                    SqlDataReader reader = cmd.ExecuteReader();

                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            list.Add(new SelectListItem { Text = reader["Code"].ToString(), Value = reader["CatalogStatusID"].ToString() });
                        }
                    }
                    else
                    {
                        list.Add(new SelectListItem { Text = "No status found", Value = "" });
                    }
                    list.Insert(0, new SelectListItem { Text = "-- Select Status --", Value = "" });
                    connection.Close();
                }
            }
            catch (Exception ex)
            {
                list.Add(new SelectListItem { Text = ex.Message.ToString(), Value = "0" });
            }

            return list;

        }

        internal static IList<SelectListItem> GetCatalogItems(string connectionString, string sUserID, ref string sErrorMsg)
        {
            List<SelectListItem> list = new List<SelectListItem>();
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    string sql = " Select s.Code + '|' + c.Code + '|' + c.Name  CatalogItem, " +
                                 " c.CatalogID " +
                                 " from Catalog c " +
                                 " left outer join arrangement a on a.CatalogID = c.catalogID and a.UserID = @UserID " +
                                 " join size s on s.SizeID = c.SizeID " +
                                 " where c.CatalogStatusID = (select x.catalogStatusID " +
                                 "                          from catalogStatus x" +
                                 "                          where x.Code = 'Active')  " +
                                 " and a.CatalogID is null " +
                                 " order by c.Code, c.Name asc";
                    SqlCommand cmd = new SqlCommand(sql, connection);
                    SqlParameter parameter = new SqlParameter
                    {
                        ParameterName = "@UserID",
                        Value = sUserID,
                        SqlDbType = SqlDbType.VarChar
                    };
                    cmd.Parameters.Add(parameter);

                    SqlDataReader reader = cmd.ExecuteReader();

                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            list.Add(new SelectListItem { Text = reader["CatalogItem"].ToString(), Value = reader["CatalogID"].ToString() });
                        }
                    }
                    else
                    {
                        list.Add(new SelectListItem { Text = "No Catalog Arrangments found", Value = "" });
                    }
                    list.Insert(0, new SelectListItem { Text = "-- Select Catalog Arrangement --", Value = "" });
                    connection.Close();
                }
            }
            catch (Exception ex)
            {
                sErrorMsg = "Unable to read Catalog items" + ex.Message;
                list.Add(new SelectListItem { Text = ex.Message.ToString(), Value = "" });
            }

            return list;
        }

        internal static void FillArrangementFromCatalog(string? connectionString, string sUserID,ref Arrangement arrangement, ref string sErrorMsg)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                string sSQL = $" Select c.Name, " +
                              $" c.Description, " +
                              $" c.Code,  " +
                              $" c.SizeID " +
                              $" from Catalog c " +
                              $" where c.CatalogID = @CatalogID ";
                using (SqlCommand command = new SqlCommand(sSQL, connection))
                {
                    command.Parameters.Clear();

                    // adding parameters
                    SqlParameter parameter = new SqlParameter
                    {
                        ParameterName = "@CatalogID",
                        Value = arrangement.CatalogID,
                        SqlDbType = SqlDbType.VarChar
                    };
                    command.Parameters.Add(parameter);
                    try
                    {
                        using (SqlDataReader dr = command.ExecuteReader())
                        {
                            while (dr.Read())
                            {
                                arrangement.Name = Convert.ToString(dr["Name"]);
                                arrangement.SizeID = Convert.ToString(dr["SizeID"]);
                                arrangement.SelectedSizeId = Convert.ToString(dr["SizeID"]);
                                arrangement.Description = Convert.ToString(dr["Description"]);
                                arrangement.Code = Convert.ToString(dr["Code"]);
                                arrangement.UserID = sUserID;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        sErrorMsg = "Can not load arrangment from catalog. " + ex.Message;
                    }
                }
                connection.Close();
            }
        }
    }
}
