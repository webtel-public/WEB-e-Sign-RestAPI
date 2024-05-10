using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WEB_e_Sign_RestAPI
{
    public class Req_List
    {
        public string pdfByte1 { get; set; }
        public String AuthorizedSignatory { get; set; }
        public String SignerName { get; set; }
        public int TopLeft { get; set; }
        public int BottomLeft { get; set; }
        public int TopRight { get; set; }
        public int BottomRight { get; set; }
        public String ExcludePageNo { get; set; }
        public string InvoiceNumber { get; set; }
        public int pageNo { get; set; }
        public string PrintDateTime { get; set; }
        public string FindAuth { get; set; }
        public int FindAuthLocation { get; set; }
        public int fontsize { get; set; }
        public int adjustCoordinates { get; set; }
        public int signOnlySearchTextPage { get; set; }
    }
}