using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration.UserSecrets;

namespace SilkDesign.Controllers
{
    public  class ControllersShared 
    {
        public static bool IsLoggedOn(ISession session, ref string UserID, ref string UserName, ref string IsAdmin)
        {
            bool bRetValue = false;
            if (session == null)
            {
                bRetValue = false;
            }
            else
            {
                UserID = session.GetString("UserID");
                UserName = session.GetString("UserName");
                IsAdmin = session.GetString("IsAdmin");

                if (string.IsNullOrEmpty(UserID)) 
                { 
                    bRetValue = false;
                }
                else 
                { 
                    bRetValue = true;
                }
            }
            return bRetValue;
        }
    }
}
