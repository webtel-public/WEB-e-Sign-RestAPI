using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WEB_e_Sign_RestAPI
{
    public static class cls_static
    {
        public static bool VaidateUser(string username, string password)
        {

            if (username == "Tui#@a#rByb" && password == "Kc$@63hTqZe")
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}