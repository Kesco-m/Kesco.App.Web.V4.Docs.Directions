using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Kesco.Lib.DALC;
using Kesco.Lib.Entities.Documents.EF;
using Kesco.Lib.Entities.Documents.EF.Directions;
using Kesco.Lib.Web.Controls.V4;
using Kesco.Lib.Web.Controls.V4.Common;

namespace Kesco.App.Web.Docs.Directions
{
    public partial class DevExpress : EntityPage
    {
        protected override string HelpUrl
        {
            get; set;
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            
            //DataTable dt = Kesco.Lib.DALC.DBManager.GetData(Kesco.Lib.Entities.SQLQueries.SELECT_Роли, Kesco.Lib.Web.Settings.Config.DS_user);

            //DevGrid.DataSource = dt;
            //DevGrid.KeyFieldName = "КодРоли";
            //DevGrid.SettingsPager.PageSize = 15;
            ////DevGrid.AutoFilterByColumn("Роль");
            //DevGrid.DataBind();
        }

        protected void efSotrudnik_OnChanged(object sender, ProperyChangedEventArgs e)
        {

            

            JS.Write("DevGrid.Refresh();");
        }
    }
}