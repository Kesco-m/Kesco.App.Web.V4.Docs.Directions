using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Web;
using Kesco.Lib.BaseExtention;
using Kesco.Lib.Entities.Corporate;
using Kesco.Lib.Entities.Documents;
using Kesco.Lib.Entities.Documents.EF.Directions;
using Kesco.Lib.Log;
using Kesco.Lib.Web.Controls.V4;
using Kesco.Lib.Web.Controls.V4.Common;
using Kesco.Lib.Web.Controls.V4.Common.DocumentPage;
using Kesco.Lib.Web.DBSelect.V4.DSO;
using Kesco.Lib.Web.Settings;


namespace Kesco.App.Web.Docs.Directions.Maket
{
    public partial class f :  EntityPage
    {
        private string help = "";
        protected override string HelpUrl { get { return ""; } set { help = value; } }

        protected void Page_Load(object sender, EventArgs e)
        {

        }
    }
}