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
                                          $" LocationPlacementID " +
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
        private static IEnumerable<SelectListItem> GetAvailableArrangements(string connectionString, string sSizeID, string OutgoingArrangmentName, string OutgoingInventoryCode, string OutgoingArrangementInventoryID)
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


        public static List<RoutePlan> GetRoutePlans(string connectionString, string sRoutePlanID)
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
                    " join Route r on r.RouteID = rp.RouteID " +
                    " join RoutePlanStatus rps on rps.RoutePlanStatusID = rp.RoutePlanStatusID " +
                    " Where rp.RoutePlanID = '" + sRoutePlanID + "' " +
                    " Order by rp.RouteDate ";

                SqlCommand readcommand = new SqlCommand(sql, connection);

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
                        routePlanList.Add(routePlan);
                    }
                }
                connection.Close();
            }
            return routePlanList;
        }
        public static RoutePlanStop GetRoutePlanDetail(string? connectionString, string sRoutePlanDetailID)
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
                    $" from routePlanDetail rpd " +
                    $" join Location l on rpd.LocationID = l.LocationID " +
                    $" join locationPlacement lp on lp.locationID = l.locationID and lp.LocationPlacementID = rpd.LocationPlacementID " +
                    $" join Size s on s.SizeID = lp.SizeID " +
                    $" join ArrangementInventory Outai on Outai.ArrangementInventoryID = rpd.OutgoingArrangementInventoryID " +
                    $" join Arrangement Outa on Outa.ArrangementID = Outai.ArrangementID " +
                    $" left outer join ArrangementInventory Inai on Inai.ArrangementInventoryID = rpd.IncomingArrangementInventoryID" +
                    $" left outer join Arrangement Ina on Ina.ArrangementID = Inai.ArrangementID " +
                    $" where rpd.routePlanDetailID = @RoutePlanDetailID " +
                    $" order by rpd.RouteOrder";

                SqlCommand command = new SqlCommand(sql, connection);
                command.Parameters.Clear();

                // adding parameters
                SqlParameter parameter = new SqlParameter
                {
                    ParameterName = "@RoutePlanDetailID",
                    Value = sRoutePlanDetailID,
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
                            rtPlanStop.AvailableArrangements = GetAvailableArrangements(connectionString, rtPlanStop.SizeID, rtPlanStop.OutgoingArrangmentName, rtPlanStop.OutgoingInventoryCode, rtPlanStop.OutgoingArrangementInventoryID);
                        }
                    }
                }
                connection.Close();
            }
            return rtPlanStop;
        }
        public static string GetSuggestedInventoryID(string connectionString, string sRoutePlanDetailID)
        {
            string sRetValue = string.Empty;

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                string sCustomerTypeSQL = $"select IncomingArrangementInventoryID SuggestedID from RoutePlanDetail where RoutePlanDetailID = '{sRoutePlanDetailID}'";
                using (SqlCommand command = new SqlCommand(sCustomerTypeSQL, connection))
                {
                    command.Parameters.Clear();
                    command.CommandText = sCustomerTypeSQL;

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

        public static List<RoutePlanDetail> GetRoutePlanDetails(string connectionString, string sRoutePlanID)
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
                    $" S.Code                   SizeCode, " +
                    $" S.SizeID                 SizeID,  " +
                    $" Inai.Code                InInvCode," +
                    $" Ina.Name                 IncomingArrangement, " +
                    $" Outai.Code               InvCode, " +
                    $" Outa.Name                OutgoingArrangment " +
                    $" from routePlanDetail rpd " +
                    $" join Location l on rpd.LocationID = l.LocationID " +
                    $" join locationPlacement lp on lp.locationID = l.locationID and lp.LocationPlacementID = rpd.LocationPlacementID " +
                    $" join Size s on s.SizeID = lp.SizeID " +
                    $" join ArrangementInventory Outai on Outai.ArrangementInventoryID = rpd.OutgoingArrangementInventoryID " +
                    $" join Arrangement Outa on Outa.ArrangementID = Outai.ArrangementID " +
                    $" left outer join ArrangementInventory Inai on Inai.ArrangementInventoryID = rpd.IncomingArrangementInventoryID" +
                    $" left outer join Arrangement Ina on Ina.ArrangementID = Inai.ArrangementID " +
                    $" where rpd.routePlanID = @RoutePlanID " +
                    $" order by rpd.RouteOrder";
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
                            stop.PlacmentDescription = Convert.ToString(dr["Placement"]);
                            stop.RouteOrder = Convert.ToInt32(dr["RouteOrder"]);
                            stop.SizeCode = Convert.ToString(dr["SizeCode"]);
                            stop.SizeID = Convert.ToString(dr["SizeID"]);
                            stop.IncomingingArrangementName = Convert.ToString(dr["IncomingArrangement"]);
                            if (!String.IsNullOrEmpty(stop.IncomingingArrangementName))
                            {
                                stop.IncomingingArrangementName += " | " + Convert.ToString(dr["InInvCode"]);
                            }
                            stop.OutgoingArrangementName = Convert.ToString(dr["OutgoingArrangment"]);
                            stop.OutgoingArrangementName += " | " + Convert.ToString(dr["InvCode"]);
                            routePlanDetailList.Add(stop);
                        }
                    }
                }
                connection.Close();
            }
            return routePlanDetailList;
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
                string sCustomerNameSQL = $" Select lt.Code Code" +
                                          $" from location l " +
                                          $" join LocationType lt on l.locationTypeID = lt.LocationTypeID " +
                                          $" where LocationID = @LocationID";
                using (SqlCommand command = new SqlCommand(sCustomerNameSQL, connection))
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
        public static List<LocationPlacement> GetLoationPlacements(string? connectionString, string id)
        {
            List<LocationPlacement> ivmList = new List<LocationPlacement>();
            List<SelectListItem> SizeList = SilkDesignUtility.GetSizes(connectionString);
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                string sql = "SELECT " +
                    "  p.LocationPlacementID  ID " +
                    " ,s.Code        CODE " +
                    " ,s.SizeID      SIZEID" +
                    " ,p.Description DESCRIPTION " +
                    " ,a.Name        ARRANGEMENT" +
                    " FROM LocationPlacement p " +
                    " join Size s on s.SizeID = p.SizeID " +
                    " left outer join arrangementInventory ai on ai.LocationPlacementID = p.LocationPlacementID " +
                    " left outer join arrangement a on a.arrangementID = ai.ArrangementID " +
                    $" where p.LocationID='{id}'";

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
                        string sArrangement = Convert.ToString(dr["ARRANGEMENT"]);
                        if (!String.IsNullOrEmpty(sArrangement))
                        {
                            ivm.Description += " / " + sArrangement;
                        }
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
        public static IEnumerable<SelectListItem> GetAvailableLocations(string connectionString, string sRouteID)
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
                                 $" join Customer c on cl.CustomerID = c.CustomerID " +
                                 $" join location l on cl.LocationID = l.LocationID " +
                                 $" where cl.locationID not in (select locationID  " +
                                 $"                             from routeLocation " +
                                 $"                             where RouteID = @RouteID)";

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
                    connection.Close();
                }
            }
            catch (Exception ex)
            {
                list.Add(new SelectListItem { Text = ex.Message.ToString(), Value = "0" });
            }


            return list;
        }
        public static RouteLocation GetRouteLocation(string? connectionString, string sRouteLocationID)
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
                          $" join Location l on l.LocationID = rl.LocationID " +
                          $" join CustomerLocation cl on cl.locationID = l.locationID " +
                          $" join customer c on c.customerID = cl.CustomerID " +
                          $" where RouteLocationID  = @RouteLocationID " +
                          $" ORDER BY rl.RouteOrder ASC";

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
                connection.Close();
            }
            return routelocal;
        }
        internal static IEnumerable<SelectListItem> GetRoutes(string connectionString)
        {
            List<SelectListItem> list = new List<SelectListItem>();
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    string sql = "Select RouteID, Name from Route";
                    SqlCommand cmd = new SqlCommand(sql, connection);
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
                list.Add(new SelectListItem { Text = ex.Message.ToString(), Value = "0" });
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
                        Value =sNextCode,
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
                    catch (Exception ex)
                    {
                        
                    }
                    finally { connection.Close(); } 
                }
            }
            return sArrangementInventoryID;
        }
        public static string CreateArrangementInventory(string connectionString, ArrangementInventory inventory)
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
                else
                {
                    sInventoryStatusCode = "Available";
                }
                sInventoryStatusID = GetInventoryStatus(connectionString, sInventoryStatusCode);

                string sql = "Insert Into ArrangementInventory (ArrangementID, Code, LocationID, InventoryStatusID";
                if (inventory.LocationPlacementID.Length > 1 )
                {
                    sql += ", LocationPlacementID ";
                }
                sql += " ) Values (@ArrangementID, @Code, @LocationID, @InventoryStatusID ";
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
                    catch (Exception ex) { }
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

        internal static void CreateRouteLocation(string? connectionString, RouteLocation routeLocation, string sRouteID)
        {
            string sLocationID = string.Empty;
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string sql = "Insert Into RouteLocation (RouteID, LocationID, RouteOrder) Values (@RouteID, @LocationID, @RouteOrder)";

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

                    connection.Open();
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
                    connection.Close();
                }
               //return sLocationID;
            }
        }

        //------------------------
        // Route Planning logic
        //------------------------
        internal static void CreateRoutePlan(string connectionString, RoutePlan routePlan)
        {
            string sPLanningID = GetRouteStatusID(connectionString, "Planning");
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string sql = "Insert Into RoutePlan (RoutePlanID, Description, RouteID, RoutePlanStatusID) Values (@RoutePlanID, @Description, @RouteID, @RoutePlanStatusID )";

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
                    connection.Open();
                    command.ExecuteNonQuery();

                    connection.Close();
                }
            }
        }
        internal static void CreateRoutePlanDetails(string connectionString, string sRouteID, string sRoutePlanID)
        {
            List<RoutePlanDetail> lRoutePlanDetails = new List<RoutePlanDetail>();
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                string sql = $"select " +
                             $"   ai.Code, " +
                             $"   lp.Description, " +
                             $"   rl.LocationID, " +
                             $"   lp.LocationPlacementID, " +
                             $"   lp.SizeID, " +
                             $"   rl.RouteOrder, " +
                             $"   ai.ArrangementInventoryID  " +
                             $" from routeLocation rl  " +
                             $" join locationPlacement lp on rl.LocationID = lp.LocationID  " +
                             $" join ArrangementInventory ai on ai.LocationPlacementID = lp.LocationPlacementID and ai.LocationID = lp.LocationID " +
                             $" where rl.routeID = @RouteID";
                SqlCommand command = new SqlCommand(sql, connection);
                // adding parameters
                SqlParameter parameter = new SqlParameter
                {
                    ParameterName = "@RouteID",
                    Value = sRouteID,
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
                        plandetail.OutgoingArrangementInventoryID = Convert.ToString(reader["ArrangementInventoryID"]);

                        CreatePlanDetailRow(connectionString, plandetail);
                        lRoutePlanDetails.Add(plandetail);
                    }
                }
                connection.Close();

            }
        }
        private static void CreatePlanDetailRow(string connectionString, RoutePlanDetail plandetail)
        {
            string sLocationID = string.Empty;
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string sql = $"Insert Into RoutePlanDetail " +
                             $"        ( RoutePlanID,  LocationID,  LocationPlacementID,  RouteOrder,  SizeID,  OutgoingArrangementInventoryID) " +
                             $" Values (@RoutePlanID, @LocationID, @LocationPlacementID, @RouteOrder, @SizeID, @OutgoingArrangementInventoryID) ";

                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    command.CommandType = CommandType.Text;
                      // adding parameters
                    SqlParameter parameter = new SqlParameter
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
                        ParameterName = "@OutgoingArrangementInventoryID",
                        Value = plandetail.OutgoingArrangementInventoryID,
                        SqlDbType = SqlDbType.VarChar
                    };
                    command.Parameters.Add(parameter);

                    connection.Open();
                    command.ExecuteNonQuery();
                    connection.Close();
                }
            }
        }
        internal static void PopulateIncoming(string connectionString, string routePlanID)
        {
            string sIncomingArrangement = string.Empty;
            // read all of the stops.
            // Loop through each stop;
            //   try to find an arrangement that is currently being removed first
            //   find an arrangement from available
            List<RoutePlanDetail> lStops = new List<RoutePlanDetail>();

            // read all of the stops.
            lStops = GetRoutePlanDetails(connectionString, routePlanID);


            // Loop through each stop;
            foreach (RoutePlanDetail stop in lStops)
            {
                //   try to find an arrangement that is currently being removed first
                sIncomingArrangement = FindFromRoute(connectionString, stop, routePlanID);

                if (String.IsNullOrEmpty(sIncomingArrangement))
                {
                    //   find an arrangement from available
                    //sIncomingArrangement = FindFromInventory(connectionString, stop);
                }

                if (!String.IsNullOrEmpty(sIncomingArrangement))
                {
                    //update PlanDetail Record w/ Incoming Arrangement
                    //update Inventory Status for Incoming
                }
            }
        }

        private static string FindFromRoute(string connectionString, RoutePlanDetail oCurrentStop, string sRoutePlanID)
        {
            string sIncomingArrangementID = string.Empty;

            //Get the start date end end dates for the most recent entry in the history table for that arrangement
            //at that locations.
            // Then if the EndDate is null, that arrangment is currently at the site, then exit
            // else check to see if the end date is older than X months old.  If is older than X months
            // then it can be used so return that arrangement to be used.
            // 
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                
                // get a list of matching deail records using the same size arrangments as the currrent Stop
                connection.Open();
                string sql = $" select ai.ArrangementID      ArrangementID, " +
                             $"        rpd.RoutePlanDetailID RoutePlanDetailID " +
                             $" from routeplandetail rpd " +
                             $" join ArrangementInventory ai on ai.ArrangementInventoryID = rpd.OutGoingArrangementInventoryID " +
                             $" join Arrangement a on a.ArrangementID = ai.ArrangementID " +
                             $" where " +
                             $" rpd.SizeID = @SizeID " +
                             $" and rpd.RouteOrder < @RouteOrder " +
                             $" and rpd.routePlanID = @RoutePlanID " +
                             $" and rpd.Disposition is null " +
                             $" Order by  rpd.RouteOrder, LastUsed ";

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
                SqlDataReader reader = command.ExecuteReader();

                // Get a list of Detail rows who match in size, that could be a potential match for the 
                // current stop.  
                if (reader.HasRows)
                {
                    string sPotentialTransferArrangementID = String.Empty;
                    string sPotentialTransferRoutePlanDetailID = String.Empty;
                    while (reader.Read())
                    {
                        sPotentialTransferArrangementID = Convert.ToString(reader["ArrangementID"]);
                        sPotentialTransferRoutePlanDetailID = Convert.ToString(reader["RoutePlanDetailID"]);

                        //See if this arrangement is valid for placement at oCurrentStop.
                        if (IsValidForCurrentStop(connectionString, oCurrentStop, sPotentialTransferArrangementID))
                        {
                            // TODO START HERE
                            // set the Incoming ArrangementID to the ArrangementID
                            // set the Disposition of found RoutePlanDetail to "Transferred"
                        }
                    }
                }

            }


            //Get the start date end end dates for the most recent entry in the history table for that arrangement
            //at that locations.
            //using (SqlConnection connection = new SqlConnection(connectionString))
            //{
            //    connection.Open();
            //    string sql = $" select StartDate, EndDate, CIH.arrangementID " +
            //                 $" from CustomerInventoryHistory CIH " +
            //                 $" WHERE CIH.LocationID = @LocationID " +
            //                 $" and CIH.arrangementID in ( select ai.ArrangementID " +
            //                 $"                            from routeplandetail rpd " +
            //                 $"                            join ArrangementInventory ai on ai.ArrangementInventoryID = rpd.OutGoingArrangementInventoryID " +
            //                 $"                            join Arrangement a on a.ArrangementID = ai.ArrangementID " +
            //                 $"                            where " +
            //                 $"                            rpd.SizeID = @SizeID " +
            //                 $"                            and rpd.RouteOrder < @RouteOrder " +
            //                 $"                            and rpd.Disposition is null " +
            //                 $"                            and rpd.routePlanID = @RoutePlanID " +
            //                 $"                          ) " +
            //                 $" AND StartDate = (select  max(x.StartDate) " +
            //                 $"                  from CustomerInventoryHistory x " +
            //                 $"                  where x.LocationID = CIH.LocationID " +
            //                 $"                  and x.ArrangementID = CIH.ArrangementID" +
            //                 $"                  ) " +
            //                 $" and CIH.EndDate is not null " +
            //                 $" and EndDate < DateAdd(MONTH, -12, GetDate())";

            //    SqlCommand command = new SqlCommand(sql, connection);
            //    SqlParameter parameter = new SqlParameter
            //    {
            //        ParameterName = "@LocationID",
            //        Value = stop.LocationID,
            //        SqlDbType = SqlDbType.VarChar
            //    };
            //    command.Parameters.Add(parameter);

            //    parameter = new SqlParameter
            //    {
            //        ParameterName = "@SizeID",
            //        Value = stop.SizeID,
            //        SqlDbType = SqlDbType.VarChar
            //    };
            //    command.Parameters.Add(parameter);

            //    parameter = new SqlParameter
            //    {
            //        ParameterName = "@RoutePlanID",
            //        Value = stop.RoutePlanID,
            //        SqlDbType = SqlDbType.VarChar
            //    };
            //    command.Parameters.Add(parameter);

            //    parameter = new SqlParameter
            //    {
            //        ParameterName = "@RouteOrder",
            //        Value = stop.RouteOrder,
            //        SqlDbType = SqlDbType.Int
            //    };
            //    command.Parameters.Add(parameter);
            //    SqlDataReader reader = command.ExecuteReader();

            //    if (reader.HasRows)
            //    {
            //        while (reader.Read())
            //        {
            //            sIncomingArrangementID = Convert.ToString(reader["ArrangementID"]);
            //        }
            //    }

            //    connection.Close();
            //}

            return sIncomingArrangementID;
              
        }

        private static bool IsValidForCurrentStop(string connectionString, RoutePlanDetail oCurrentStop, string? sArrangementID)
        {
            bool bRetValue = false;
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
                             $"                  and x.ArrangementID = CIH.ArrangementID" +
                             $"                  ) " +
                             $" and CIH.EndDate is not null " +
                             $" and EndDate < DateAdd(MONTH, -12, GetDate())";

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

                SqlDataReader reader = command.ExecuteReader();

                if (reader.HasRows)
                {
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
                }
                else
                {
                    bRetValue = true;
                }

                connection.Close();
            }
            return bRetValue;
        }

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

        internal static void MoveLocationUp(string? connectionString, int iOldValue, int iNewValue, string sRouteID, string sRouteLocationID)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                // string sql = "Insert Into Location (Name, Description, LocationTypeID) Values (@Name, @Description, @LocationTypeID)";
                string sql = $" Update RouteLocation " +
                             $" SET RouteOrder = RouteOrder + 1 " +
                             $" Where RouteId=@RouteID " +
                             $" AND   RouteOrder >= @NewValue " +
                             $" AND   RouteOrder < @OldValue ";

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
                    command.ExecuteNonQuery();
                }

                sql = $" Update RouteLocation " +
                      $" SET RouteOrder = @NewValue " +
                      $" WHERE RouteLocationID = @RouteLoctionID ";

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
                    command.ExecuteNonQuery();

                }
                connection.Close();
            }
        }

        internal static void MoveLocationDown(string? connectionString, int iOldValue, int iNewValue, string sRouteID, string sRouteLocationID)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                // string sql = "Insert Into Location (Name, Description, LocationTypeID) Values (@Name, @Description, @LocationTypeID)";
                string sql = $" Update RouteLocation " +
                             $" SET RouteOrder = RouteOrder - 1 " +
                             $" Where RouteId = @RouteID " +
                             $" AND   RouteOrder >  @OldValue " +
                             $" AND   RouteOrder <= @NewValue";

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
                    command.ExecuteNonQuery();
                }

                sql = $" Update RouteLocation " +
                      $" SET RouteOrder = @NewValue " +
                      $" WHERE RouteLocationID = @RouteLoctionID ";

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
                    command.ExecuteNonQuery();

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

    }
}
