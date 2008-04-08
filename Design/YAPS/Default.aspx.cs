﻿using System;
using System.Collections;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Xml.Linq;

namespace YAPS
{
    public partial class _Default : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            YAPS_Service.YAPSService ServiceClient = new YAPS.YAPS_Service.YAPSService();

            WCFVersionNumber.Text = ServiceClient.YAPSVersion();
        }
    }
}
