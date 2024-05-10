using System.Web;
using System.Web.Mvc;

namespace WEB_e_Sign_RestAPI
{
    public class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
        }
    }
}
