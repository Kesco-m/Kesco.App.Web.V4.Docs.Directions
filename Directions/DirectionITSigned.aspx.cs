using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Resources;
using System.Security.Principal;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using Kesco.Lib.BaseExtention;
using Kesco.Lib.BaseExtention.Enums.Controls;
using Kesco.Lib.DALC;
using Kesco.Lib.Entities;
using Kesco.Lib.Entities.Corporate;
using Kesco.Lib.Entities.Documents;
using Kesco.Lib.Entities.Documents.EF.Directions;
using Kesco.Lib.Log;
using Kesco.Lib.Web.Controls.V4;
using Kesco.Lib.Web.Controls.V4.Common.DocumentPage;
using Kesco.Lib.Web.Settings;

namespace Kesco.App.Web.Docs.Directions
{
    public partial class DirectionITSigned : DocPage
    {
        private RenderHelper _render;
        protected DataTable dtEquip;

        protected ResourceManager LocalResx = new ResourceManager("Kesco.App.Web.Docs.Directions.DirectionIT",
            Assembly.GetExecutingAssembly());

        protected RenderHelper Render
        {
            get { return _render ?? (_render = new RenderHelper()); }
        }

        protected Direction Dir
        {
            get { return (Direction) Doc; }
        }

        private void SetCultureText()
        {
            hAccessEml = LocalResx.GetString("hAccessEml");
            hDadaEml = LocalResx.GetString("hDadaEml");
            hEquipEml = LocalResx.GetString("hEquipEml");
            lAEOfiice = LocalResx.GetString("lAEOfiice");
            lAEVpn = LocalResx.GetString("lAEVpn");
            lAIMobile = LocalResx.GetString("lAIMobile");
            lAIModem = LocalResx.GetString("lAIModem");
            lAIOffice = LocalResx.GetString("lAIOffice");
            lCompN = LocalResx.GetString("lCompN");
            lCompP = LocalResx.GetString("lCompP");
            lCompT = LocalResx.GetString("lCompT");
            lPhoneDect = LocalResx.GetString("lPhoneDect");
            lPhoneDesk = LocalResx.GetString("lPhoneDesk");
            lPhoneIP = LocalResx.GetString("lPhoneIP");
            lPhoneIPCam = LocalResx.GetString("lPhoneIPCam");
            lPhoneSim = LocalResx.GetString("lPhoneSim");

            lPLExitInSide = LocalResx.GetString("lPLExitInSide");
            lPLExitOutCountry = LocalResx.GetString("lPLExitOutCountry");
            lPLExitOutTown = LocalResx.GetString("lPLExitOutTown");
            lPLExitTown = LocalResx.GetString("lPLExitTown");

            lSotrudnikParent1 = LocalResx.GetString("lSotrudnikParent1");
            lSotrudnikParent2 = LocalResx.GetString("lSotrudnikParent2");

            lRequire = LocalResx.GetString("lRequire");
            lComplete = LocalResx.GetString("lComplete");
        }

        private void CreateDataTableEquipment()
        {
            dtEquip = Dir.GetDirectionEquipment();
        }

        private void Execute()
        {
            if (Dir.IsNew) return;
            var cm = new SqlCommand(SQLQueries.SP_ВыполнениеУказанийIT);
            cm.Connection = new SqlConnection(Config.DS_user);
            cm.CommandType = CommandType.StoredProcedure;
            cm.CommandTimeout = 0;
            cm.Parameters.AddWithValue("@КодДокумента", int.Parse(Dir.Id));
            cm.Parameters.Add("@RETURN_VALUE", SqlDbType.Int, 4);
            cm.Parameters["@RETURN_VALUE"].Direction = ParameterDirection.ReturnValue;
            try
            {
                cm.Connection.Open();
                cm.ExecuteNonQuery();
                if (cm.Parameters["@RETURN_VALUE"].Value != null
                    && !cm.Parameters["@RETURN_VALUE"].Value.Equals(DBNull.Value))
                {
                    if (cm.Parameters["@RETURN_VALUE"].Value.ToString().Equals("1"))
                        RefreshData();
                }
            }
            catch (Exception ex)
            {
                throw new DetailedException(ex.Message, ex, cm);
            }
            finally
            {
                if (cm != null && cm.Connection != null && cm.Connection.State.Equals(ConnectionState.Open))
                    cm.Connection.Close();
            }
            JS.Write("Wait.render(false);");
        }

        protected override void LoadData(string id)
        {
            base.LoadData(id);
            if (id.IsNullEmptyOrZero()) return;
            Dir.LoadDocumentPositions();
        }

        #region CONST

        protected string hAccessEml = "";
        protected string hDadaEml = "";
        protected string hEquipEml = "";
        protected string lAEOfiice = "";
        protected string lAEVpn = "";
        protected string lAIMobile = "";
        protected string lAIModem = "";
        protected string lAIOffice = "";
        protected string lCompN = "";
        protected string lCompP = "";
        protected string lCompT = "";
        protected string lPhoneDect = "";
        protected string lPhoneDesk = "";
        protected string lPhoneIP = "";
        protected string lPhoneIPCam = "";
        protected string lPhoneSim = "";
        protected string lSotrudnikParent1 = "";
        protected string lSotrudnikParent2 = "";


        protected string lPLExitInSide = "";
        protected string lPLExitOutCountry = "";
        protected string lPLExitOutTown = "";
        protected string lPLExitTown = "";

        protected string lRequire = "";
        protected string lComplete = "";

        #endregion

        #region Override

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!V4IsPostBack && DocEditable)
            {
                V4Redirect("DirectionIT.aspx");
            }

            if (!V4IsPostBack)
            {
                SetCultureText();
                CreateDataTableEquipment();
            }
        }

        protected override void SetDocMenuButtons()
        {
            base.SetDocMenuButtons();

            var wi = WindowsIdentity.GetCurrent();
            var wp = new WindowsPrincipal(wi);


            if (wp.IsInRole("TEST\\Programists")
                ||
                wp.IsInRole("EURO\\Domain Admins")
                )
            {
                
                var btnExecute = new Button
                {
                    ID = "btnExecute",
                    V4Page = this,
                    Text = LocalResx.GetString("btnGO"),
                    Title = LocalResx.GetString("btnGO"),
                    IconJQueryUI = ButtonIconsEnum.Ok,
                    Width = 105,
                    OnClick = "Wait.render(true); cmdasync('cmd','Execute');"
                };
                AddMenuButton(btnExecute);
            }

            var btnOldVersion = new Button
            {
                ID = "btnOldVersion",
                V4Page = this,
                Text = LocalResx.GetString("btnOldVersion"),
                Title = LocalResx.GetString("btnOldVersion"),
                IconJQueryUI = ButtonIconsEnum.Alert,
                Width = 125,
                OnClick = string.Format("v4_windowOpen('{0}','_self');", HttpUtility.JavaScriptStringEncode(WebExtention.UriBuilder(ConfigurationManager.AppSettings["URI_Direction_OldVersion"], CurrentQS)))
            };
            AddMenuButton(btnOldVersion);
        }

        protected override void DocumentInitialization(Document copy = null)
        {
            if (copy == null)
                Doc = new Direction();
            else
                Doc = (Direction) copy;
            ShowCopyButton = false;
            ShowDocDate = false;
        }

        protected override void DocumentToControls()
        {
        }

        protected override void SetControlProperties()
        {
        }

        protected override void ProcessCommand(string cmd, NameValueCollection param)
        {
            switch (cmd)
            {
                case "Execute":
                    Execute();
                    break;
                default:
                    base.ProcessCommand(cmd, param);
                    break;
            }
        }

        #endregion

        #region Render

        #region Render advanced function

        private void GetNtfFormatMsg(TextWriter w, string _msg)
        {
            GetNtfFormatMsg(w, _msg, true);
        }

        private void GetNtfFormatMsg(TextWriter w, string _msg, bool br)
        {
            w.Write("{1}<font class='NtfMsg'>{0}</font>", _msg, (br ? "<br>" : ""));
        }


        private void GetCompleteInfo(TextWriter w, string _msg, bool fl)
        {
            GetCompleteInfo(w, _msg, fl, true);
        }

        private void GetCompleteInfo(TextWriter w, string _msg, bool fl, bool br)
        {
            if (!string.IsNullOrEmpty(_msg) && _msg == "-" && fl)
            {
                w.Write("<br>&nbsp;");
                return;
            }
            var str = string.Format("{2}<span {0}>{1}</span>", (!fl ? "class='NoCI'" : ""),
                string.IsNullOrEmpty(_msg) ? "" : _msg, (br ? "<br>" : ""));
            w.Write(str);
        }


        private string GetCultureStr(string _ru, string _eng)
        {
            return IsRusLocal ? _ru : _eng;
        }

        #endregion

        protected void RenderMobilPhone(TextWriter w)
        {
            var bitMask = (Dir.PhoneEquipField.ValueString.Length == 0) ? 0 : Dir.PhoneEquipField.ValueInt;
            var fl = false;
            var _phoneNum = "";

            if (bitMask == 0 && Dir.RedirectNumField.ValueString.Length == 0)
            {
                w.Write("");
                return;
            }

            if (Dir.RedirectNumField.ValueString.Length == 0 &&
                (bitMask & 1) != 1 && (bitMask & 2) != 2 && (bitMask & 4) != 4) fl = false;

            w.Write("<div class=\"marginL marginT\">{0}:</div>", Dir.RedirectNumField.Name);
            w.Write("<div class=\"marginL2 marginT\">");
            if (Dir.RedirectNumField.ValueString.Length > 0)
            {
                fl = true;
                _phoneNum = Dir.RedirectNumField.ValueString;
                Dir.FormatingMobilNumber(ref _phoneNum);

                w.Write(
                    "<a href='#' title='" + LocalResx.GetString("_Msg_CopyBuffer") +
                    "' onclick=\"copyToClipboard('{0}')\">", _phoneNum);
                w.Write(Dir.RedirectNumField.ValueString);
                w.Write("</a>");
            }

            var dv = new DataView();
            dv.Table = dtEquip;

            dv.RowFilter = "ЕстьХарактеристикиSIM=1";
            dv.Sort = "КодТипаОборудования, Оборудование";

            for (var i = 0; i < dv.Count; i++)
            {
                fl = true;
                if (i == 0 && Dir.RedirectNumField.ValueString.Length > 0 || i > 0) w.Write(", ");

                if (dv[i]["НомерТелефона"].Equals(DBNull.Value))
                {
                    w.Write("<span ><img src='/styles/sim.gif' border=0>");
                    w.Write(LocalResx.GetString("_Msg_ВыданаSim"));
                    w.Write("</span>");
                    continue;
                }
                _phoneNum = dv[i]["НомерТелефона"].ToString();
                if (_phoneNum.Length > 6) Dir.FormatingMobilNumber(ref _phoneNum);


                w.Write(
                    "<a href='#' title='" + LocalResx.GetString("_Msg_CopyBuffer") +
                    "' onclick=\"copyToClipboard('{0}')\">", _phoneNum);
                w.Write(_phoneNum);
                w.Write("</a>");
            }

            if (!fl)
            {
                if ((bitMask & 16) != 16)
                    GetNtfFormatMsg(w, LocalResx.GetString("_Msg_NoSpecified"), false);
                else
                    w.Write(LocalResx.GetString("_Msg_SimGive"));
            }
            w.Write("</div>");
        }

        private void RefreshData()
        {
            var w = new StringWriter();
            CreateDataTableEquipment();
            RenderData(w);
            JS.Write("document.getElementById('divData').innerHTML='{0}';",
                HttpUtility.JavaScriptStringEncode(w.ToString()));
        }

        protected void RenderData(TextWriter w)
        {
            w.Write("<table class='bgcolor marginT2' cellpadding=0 cellspacing=0 style='width:96%;' >");

            RenderSotrudnikInfo(w);

            RenderHeader(w);
            RenderPhoneEquip(w);
            RenderCompEquip(w);
            RenderAdvEquip(w);
            RenderAEAccess(w);
            RenderLanguage(w);
            RenderEmail(w);
            RenderSotrudnikParent(w);
            RenderSFolder(w);
            RenderRoles(w);
            RenderTypes(w);
            RenderAdvancedGrants(w);

            RenderNote(w);
            w.Write("</table>");
        }

        private void RenderSotrudnikInfo(TextWriter w)
        {
        }

        private void RenderHeader(TextWriter w)
        {
            w.Write("<tr>");
            w.Write("<td width='50%' align='center' colspan=2>");
            w.Write("<b>{0}</b>", lRequire);
            w.Write("</td>");
            w.Write("<td width='50%' align='center' colspan=2>");
            w.Write("<b>{0}</b>", lComplete);
            w.Write("</td>");
            w.Write("</tr>");
        }


        private void RenderPhoneEquip(TextWriter w)
        {
            w.Write("<tr>");
            w.Write("<td colspan=2 class='TDBB' valign='top'>");
            RenderPhoneEquipRequired(w);
            w.Write("</td>");
            w.Write("<td colspan=2 class='TDBB TDBL' valign='top' style=\"padding-left:2px;\">");
            RenderInsidePhoneComplete(w);
            w.Write("</td>");
            w.Write("</tr>");
        }

        private void RenderPhoneEquipRequired(TextWriter w)
        {
            var w1 = new StringWriter();
            var w10 = new StringWriter();
            var w200 = new StringWriter();
            var bitMask = 0;

            w.Write("<table cellpadding=0 cellspacing=0 width='100%'>");
            w.Write("<tr>");
            w.Write("<td width='100%' colspan=3>");
            w.Write(Dir.PhoneEquipField.Name);
            w.Write("</td>");
            w.Write("</tr>");
            w.Write("<tr>");
            w.Write("<td colspan=2 class='TDDataPL' nowrap>");
            if (Dir.PhoneEquipField.ValueString.Length == 0)
                w.Write(LocalResx.GetString("_Msg_NoRequired"));
            else
            {
                bitMask = Dir.PhoneEquipField.ValueInt;
                var val = "";
                var col = new StringCollection();

                if ((bitMask & 1) == 1)
                {
                    val = lPhoneDesk;
                    if ((bitMask & 8) == 8)
                        val += "&nbsp;+&nbsp;" + lPhoneIPCam;
                    col.Add(val);
                }

                if ((bitMask & 2) == 2)
                {
                    col.Add(lPhoneDect);
                }
               
            }
            w.Write("</td>");
            w.Write("<td valign='top' noWrap width='100%' align='left' style='PADDING-LEFT:30px'>");

            if (Dir.PhoneLinkField.ValueString.Length > 0 && ((bitMask & 1) == 1 || (bitMask & 2) == 2))
            {
                bitMask = Dir.PhoneLinkField.ValueInt;

                if ((bitMask & 8) == 8) w.Write(lPLExitOutCountry);
                else if ((bitMask & 4) == 4) w.Write(lPLExitOutTown);
                else if ((bitMask & 2) == 2) w.Write(lPLExitTown);
                //else if ((bitMask & 2) == 2) w.Write(lPLExitOutTown);
                else if ((bitMask & 1) == 1) w.Write(lPLExitInSide);
            }
            w.Write("</td>");
            w.Write("</tr>");
            w.Write("</table>");
        }

        private void RenderInsidePhoneComplete(TextWriter w)
        {
            var sb = new StringBuilder();
            var fl = false;


            var dv = new DataView();
            var bitmask = 0;
            if (Dir.PhoneEquipField.ValueString.Length > 0)
            {
                bitmask = Dir.PhoneEquipField.ValueInt;
                if ((bitmask & 16) == 16) bitmask ^= 16;
                if ((bitmask & 32) == 32) bitmask ^= 32;
            }

            var flC = (Dir.PhoneEquipField.ValueString.Length == 0 ||
                       (Dir.PhoneEquipField.ValueString.Length != 0 && bitmask == 0));

            sb.AppendFormat("<table cellpadding=0 cellspacing=0 {0}>", flC ? "class='NoCI'" : "");

            dv.Table = dtEquip;


            if (dtEquip.Columns.Contains("ЕстьТелефонныйНомер"))
                dv.RowFilter = "(НомерТелефона IS NOT NULL AND ЕстьХарактеристикиSIM=0) OR ЕстьТелефонныйНомер > 0";
            else
                dv.RowFilter = "(НомерТелефона IS NOT NULL AND ЕстьХарактеристикиSIM=0)";

            dv.Sort = "НомерТелефона, Оборудование";

            if (dv.Count > 0)
            {
                fl = true;
                StringWriter wr;
                for (var i = 0; i < dv.Count; i++)
                {
                    if (i == 0)
                    {
                        sb.AppendFormat("<tr>");
                        sb.AppendFormat("<td colspan=2>");
                        sb.AppendFormat("&nbsp;");
                        sb.AppendFormat("</td>");
                        sb.AppendFormat("</tr>");
                    }
                    sb.AppendFormat("<tr>");
                    sb.AppendFormat("<td noWrap>");
                    sb.AppendFormat("{0}", dv[i]["НомерТелефона"]);
                    sb.AppendFormat("</td>");
                    sb.AppendFormat("<td noWrap>");
                    var className = (flC ? "class='NoCI'" : "");
                    var empls = Render.EquipmentEmployee(this, null, Dir, dv[i]["КодОборудования"].ToString(), className);
                    wr = new StringWriter();
                    RenderLinkEquipment(wr, dv[i]["КодОборудования"].ToString(), className,
                        "title=\"" + LocalResx.GetString("_Msg_OpenEquip") + "\"");
                    wr.Write(dv[i]["Оборудование"]);
                    RenderLinkEnd(wr);
                    wr.Write(empls);

                    sb.AppendFormat("&nbsp;{0}", wr);

                    sb.AppendFormat("</td>");
                    sb.AppendFormat("</tr>");
                }
            }
            sb.AppendFormat("</table>");

            if (fl)
                w.Write(sb.ToString());
            else
            {
                GetCompleteInfo(w, "-", flC);
                w.Write("<br>");
            }
        }

       
        private void RenderCompEquip(TextWriter w)
        {
            w.Write("<tr>");
            w.Write("<td colspan=2 class='TDBB' valign='top'>");
            RenderCompEquipRequired(w);
            w.Write("</td>");
            w.Write("<td colspan=2 class='TDBB TDBL' valign='top' style=\"padding-left:2px;\">");
            RenderCompEquipComplete(w);
            w.Write("</td>");
            w.Write("</tr>");
        }

        private void RenderCompEquipRequired(TextWriter w)
        {
            w.Write("<table cellpadding=0 cellspacing=0 width='100%'>");
            w.Write("<tr>");
            w.Write("<td width='100%'>");
            w.Write(Dir.CompTypeField.Name);
            w.Write("</td>");
            w.Write("</tr>");
            w.Write("<tr>");
            w.Write("<td colspan=2 class='TDDataPL'>");
            if (Dir.CompTypeField.ValueString.Length == 0)
                w.Write(LocalResx.GetString("_Msg_NoRequired"));
            else
            {
                var bitMask = Dir.CompTypeField.ValueInt;

                var col = new StringCollection();

                if ((bitMask & 1) == 1)
                    col.Add(lCompT);

                if ((bitMask & 2) == 2)
                    col.Add(lCompP);

                if ((bitMask & 4) == 4)
                    col.Add(lCompN);


                for (var i = 0; i < col.Count; i++)
                {
                    if (i > 0) w.Write("<br>");
                    w.Write(col[i]);
                }
                if ((bitMask & 2) == 2 || (bitMask & 1) == 1)
                {
                    if (Dir.AccessEthernetField.ValueString.Length == 0)
                        GetNtfFormatMsg(w, LocalResx.GetString("_Msg_NoAccessEthernet"));
                }
            }
            w.Write("</td>");
            w.Write("</tr>");
            w.Write("</table>");
        }

        private void RenderCompEquipComplete(TextWriter w)
        {
            var dv = new DataView();
            dv.Table = dtEquip;

            var flC = Dir.CompTypeField.ValueString.Length > 0;

            dv.RowFilter = "ЕстьХарактеристикиКомпьютера=1 OR ЕстьХарактеристикиМонитора=1";
            dv.Sort = "КодТипаОборудования, Оборудование";
            if (dv.Count == 0)
            {
                GetCompleteInfo(w, "-", !flC);
                return;
            }


            for (var i = 0; i < dv.Count; i++)
            {
                if (i > 0) w.Write("<br/>");
                var className = (!flC ? "class='NoCI'" : "");
                var empls = Render.EquipmentEmployee(this, null, Dir, dv[i]["КодОборудования"].ToString(), className);
                RenderLinkEquipment(w, dv[i]["КодОборудования"].ToString(), className,
                    "title=\"" + LocalResx.GetString("_Msg_OpenEquip") + "\"");
                w.Write(dv[i]["Оборудование"]);
                RenderLinkEnd(w);
                w.Write(empls);
            }
        }


        private void RenderAdvEquip(TextWriter w)
        {
            if (Dir.AdvEquipField.ValueString.Length == 0) return;
            w.Write("<tr>");
            w.Write("<td colspan=2 class='TDBB' valign='top'>");
            RenderAdvEquipRequired(w);
            w.Write("</td>");
            w.Write("<td colspan=2 class='TDBB TDBL' valign='top' style=\"padding-left:2px;\">");
            RenderAdvEquipComplete(w);
            w.Write("</td>");
            w.Write("</tr>");
        }

        private void RenderAdvEquipRequired(TextWriter w)
        {
            w.Write("<table cellpadding=0 cellspacing=0 width='100%'>");
            w.Write("<tr>");
            w.Write("<td width='100%'>");
            w.Write(Dir.AdvEquipField.Name);
            w.Write("</td>");
            w.Write("</tr>");
            w.Write("<tr>");
            w.Write("<td colspan=2 class='TDDataPL'>");
            if (Dir.AdvEquipField.ValueString.Length == 0)
                w.Write(LocalResx.GetString("_Msg_NoRequired"));
            else
                w.Write(Dir.AdvEquipField.ValueString);
            w.Write("</td>");
            w.Write("</tr>");
            w.Write("</table>");
        }

        private void RenderAdvEquipComplete(TextWriter w)
        {
            var dv = new DataView();
            dv.Table = dtEquip;

            if (dtEquip.Columns.Contains("ЕстьТелефонныйНомер"))
                dv.RowFilter =
                    "ЕстьХарактеристикиКомпьютера=0 AND ЕстьХарактеристикиМонитора=0 AND ЕстьХарактеристикиSIM=0 AND ЕстьТелефонныйНомер=0 AND НомерТелефона IS NULL";
            else
                dv.RowFilter =
                    "ЕстьХарактеристикиКомпьютера=0 AND ЕстьХарактеристикиМонитора=0 AND ЕстьХарактеристикиSIM=0 AND НомерТелефона IS NULL";

            dv.Sort = "КодТипаОборудования, Оборудование";

            var flC = Dir.AdvEquipField.ValueString.Length > 0;

            if (dv.Count == 0)
            {
                GetCompleteInfo(w, "-", !flC);
                return;
            }


            for (var i = 0; i < dv.Count; i++)
            {
                if (i > 0) w.Write("<br/>");
                var className = (!flC ? "class='NoCI'" : "");
                var empls = Render.EquipmentEmployee(this, null, Dir, dv[i]["КодОборудования"].ToString(), className);
                RenderLinkEquipment(w, dv[i]["КодОборудования"].ToString(), className,
                    "title=\"" + LocalResx.GetString("_Msg_OpenEquip") + "\"");
                w.Write(dv[i]["Оборудование"]);
                RenderLinkEnd(w);
                w.Write(empls);
            }
        }


        private void RenderAEAccess(TextWriter w)
        {
            w.Write("<tr>");
            w.Write("<td colspan=2 class='TDBB' valign='top'>");
            RenderAEAccessRequired(w);
            w.Write("</td>");
            w.Write("<td colspan=2 class='TDBB TDBL' valign='top' style=\"padding-left:2px;\">");
            RenderAEAccessComplete(w, lAEVpn);
            w.Write("</td>");
            w.Write("</tr>");
        }

        private void RenderAEAccessRequired(TextWriter w)
        {
            w.Write("<table cellpadding=0 cellspacing=0 width='100%'>");
            w.Write("<tr>");
            w.Write("<td width='100%'>");
            w.Write(Dir.AccessEthernetField.Name);
            w.Write("</td>");
            w.Write("</tr>");
            w.Write("<tr>");
            w.Write("<td colspan=2 class='TDDataPL'>");

            var bitMask = Dir.AccessEthernetField.ValueString.Length == 0 ? 0 : Dir.AccessEthernetField.ValueInt;
           
            var col = new StringCollection();

            if ((bitMask & 1) == 1 || (bitMask & 2) == 2)
                col.Add((Dir.LoginField.ValueString.Length > 0) ? Dir.LoginField.ValueString : lAEOfiice);
            else
                col.Add(LocalResx.GetString("_Msg_NoRequired") + lAEOfiice);

            if ((bitMask & 2) == 2)
                col.Add(lAEVpn);
            else
                col.Add(LocalResx.GetString("_Msg_NoRequired") + " " + lAEVpn);

            for (var i = 0; i < col.Count; i++)
            {
                if (i > 0) w.Write("<br/>");
                w.Write(col[i]);
            }
            w.Write("</td>");
            w.Write("</tr>");
            w.Write("</table>");
        }

        private void RenderAEAccessComplete(TextWriter w, string lAEVpn)
        {
            //if (ds == null || ds.Tables.Count == 0) GetFoldersAndInternet(w);
            //if (ds == null)
            //{
            //    w.Write(LocalResx.GetString("_Msg_NoData"));
            //    return;
            //}

            var bitMask = Dir.AccessEthernetField.ValueString.Length == 0 ? 0 : Dir.AccessEthernetField.ValueInt;
            var fl = false;


            if (Dir.SotrudnikField.ValueString.Length > 0 && !Dir.Sotrudnik.Unavailable)
            {
                if (Dir.Sotrudnik.Login != null)
                {
                    fl = (Dir.LoginField.ValueString.ToLower().Equals(Dir.Sotrudnik.Login.ToLower()));
                }

                if (!string.IsNullOrEmpty(Dir.Sotrudnik.Login))
                {
                    var _f = Dir.Sotrudnik.PersonalFolder;
                    var _href = Regex.Replace(_f, "\\\\[^\\\\]+$", "").Replace(":", "|").Replace("\\", "/");
                    GetCompleteInfo(w,
                        "<nobr>" + Dir.Sotrudnik.Login + " " + "<a href='file:///" + _href + "' target='_blabk'>" + _f +
                        "</a>" + "</nobr>", fl);
                    RenderADSIInfoByLogin(w, Dir.LoginField.ValueString);
                }
                else
                    GetCompleteInfo(w, "-", fl);
            }

            //try
            //{
            //    dt = new DataTable("AE");
            //    dt = ds.Tables[1];
            //}
            //catch (Exception)
            //{
            //    //Kesco.Env.Log.Write(ex,"Процедура получения данных о доступе к корпоративной сети отработала некорректно. DS.TABLES.COUNT="+(ds!=null?ds.Tables.Count:0));
            //    w.Write(resx.GetString("_Msg_NoData"));
            //    return;
            //}
            //if (dt.Rows.Count == 0)
            //{
            //    w.Write(resx.GetString("_Msg_NoData"));
            //    return;
            //}

            //int r = ((bitMask & 2) == 2 ? 1 : 0) + (dt.Rows[0]["VPN"].ToString().Equals("1") ? 1 : 0);
            //fl = (r != 1);
            //GetCompleteInfo(w, (dt.Rows[0]["VPN"].ToString().Equals("1") ? "+" : "-"), fl);
        }


        private void RenderLanguage(TextWriter w)
        {
            w.Write("<tr>");
            w.Write("<td colspan=2 class='TDBB' valign='top'>");
            RenderLanguageRequired(w);
            w.Write("</td>");
            w.Write("<td colspan=2 class='TDBB TDBL' valign='top' style=\"padding-left:2px;\">");
            RenderLanguageComplete(w);
            w.Write("</td>");
            w.Write("</tr>");
        }

        private void RenderLanguageRequired(TextWriter w)
        {
            w.Write("<table cellpadding=0 cellspacing=0 width='100%'>");
            w.Write("<tr>");
            w.Write("<td width='100%'>");
            w.Write(Dir.SotrudnikLanguageField.Name);
            w.Write("</td>");
            w.Write("</tr>");
            w.Write("<tr>");
            w.Write("<td colspan=2 class='TDDataPL'>");
            if (Dir.SotrudnikLanguageField.ValueString.Length == 0) w.Write("ru");
            else w.Write(Dir.SotrudnikLanguageField.ValueString);
            w.Write("</td>");
            w.Write("</tr>");
            w.Write("</table>");
        }

        private void RenderLanguageComplete(TextWriter w)
        {
            if (Dir.SotrudnikField.ValueString.Length == 0 || Dir.Sotrudnik.Unavailable)
            {
                w.Write(LocalResx.GetString("_Msg_NoData"));
                return;
            }

            var _l = (Dir.SotrudnikLanguageField.ValueString.Length == 0
                ? "ru"
                : Dir.SotrudnikLanguageField.ValueString.ToLower());
            var fl = Dir.Sotrudnik.Language.ToLower().Equals(_l);

            GetCompleteInfo(w, Dir.Sotrudnik.Language, fl, true);
        }


        private void RenderEmail(TextWriter w)
        {
            w.Write("<tr>");
            w.Write("<td colspan=2 class='TDBB' valign='top'>");
            RenderEmailRequired(w);
            w.Write("</td>");
            w.Write("<td colspan=2 class='TDBB TDBL' valign='top' style=\"padding-left:2px;\">");
            RenderEmailComplete(w);
            w.Write("</td>");
            w.Write("</tr>");
        }

        private void RenderEmailRequired(TextWriter w)
        {
            w.Write("<table cellpadding=0 cellspacing=0 width='100%'>");
            w.Write("<tr>");
            w.Write("<td width='100%'>");
            w.Write("E-Mail");
            w.Write("</td>");
            w.Write("</tr>");
            w.Write("<tr>");
            w.Write("<td colspan=2 class='TDDataPL'>");
            if (Dir.MailNameField.ValueString.Length == 0 || Dir.DomainField.ValueString.Length == 0)
                w.Write(LocalResx.GetString("_Msg_NoRequired"));
            else
            {
                var email = Dir.MailNameField.ValueString + "@" + Dir.DomainField.ValueString;
                w.Write(email);

                var s = "";

                using (var sw = new StringWriter())
                {
                    ValidationMessages.CheckEmailName(this, w, Dir);
                    s = sw.ToString();
                    if (s.Length > 0) RenderNtf(w, new List<string> {s});
                }
                using (var sw = new StringWriter())
                {
                    ValidationMessages.CheckUniqueEmail(this, sw, Dir);
                    s = sw.ToString();
                    if (s.Length > 0) RenderNtf(w, new List<string> {s});
                }
            }
            w.Write("</td>");
            w.Write("</tr>");
            w.Write("</table>");
        }

        private void RenderEmailComplete(TextWriter w)
        {
            if (Dir.SotrudnikField.ValueString.Length == 0 || Dir.Sotrudnik.Unavailable)
            {
                w.Write(LocalResx.GetString("_Msg_NoData"));
                return;
            }


            var email = "";
            if (Dir.Sotrudnik.Email.Length > 0)
                email = Dir.Sotrudnik.Email;
            else
                email = "-";

            var emaildd = (Dir.MailNameField.ValueString.Length == 0 || Dir.DomainField.ValueString.Length == 0)
                ? "-"
                : (Dir.MailNameField.ValueString + "@" + Dir.DomainField.ValueString);

            var fl = email.ToLower().Equals(emaildd.ToLower());

            GetCompleteInfo(w, email, fl);
        }


        private void RenderSotrudnikParent(TextWriter w)
        {
            if (Dir.SotrudnikParentField.ValueString.Length == 0 ||
                Dir.SotrudnikParentCheckField.ValueString.Length == 0) return;
            w.Write("<tr>");
            w.Write("<td colspan=4 class='TDBB' valign='top'>");
            RenderSotrudnikParentRequired(w);
            w.Write("</td>");
            w.Write("</tr>");
        }

        private void RenderSotrudnikParentRequired(TextWriter w)
        {
            w.Write("<table cellpadding=0 cellspacing=0 width='100%'>");
            w.Write("<tr>");
            w.Write("<td width='100%'>");
            w.Write(hAccessEml);
            w.Write("</td>");
            w.Write("</tr>");
            w.Write("<tr>");
            w.Write("<td colspan=2 class='TDDataPL'>");

            var bitMask = Dir.SotrudnikParentCheckField.ValueInt;
            if ((bitMask & 1) == 1)
                w.Write(lSotrudnikParent1);

            if ((bitMask & 2) == 2)
                w.Write(lSotrudnikParent2);

            w.Write(": ");

            RenderLinkEmployee(w, "parent" + Dir.SotrudnikParentField.ValueString, Dir.SotrudnikParent, NtfStatus.Empty);

            var p = Dir.SotrudnikParent;
            if (p.Unavailable)
            {
                GetNtfFormatMsg(w, LocalResx.GetString("_Msg_СотрудникНеДоступен"));
                return;
            }
            if (Dir.SotrudnikField.ValueString.Length > 0 &&
                Dir.SotrudnikParentField.ValueString.Equals(Dir.SotrudnikField.ValueString))
                GetNtfFormatMsg(w, LocalResx.GetString("_NTF_СотрудникСовпадает").ToLower());

            if ((bitMask & 1) == 1 && (p.Login.Length == 0))
                GetNtfFormatMsg(w, LocalResx.GetString("_Msg_СотрудникНеИмеетЛогина"));

            if ((bitMask & 2) == 2 && (p.Login.Length == 0)
                && Dir.SotrudnikField.ValueString.Length > 0 && !Dir.Sotrudnik.Unavailable &&
                (Dir.Sotrudnik.Login.Length == 0))
                GetNtfFormatMsg(w, LocalResx.GetString("_Msg_СотрудникНеИмеетЛогина"));

            using (var sw = new StringWriter())
            {
                var ntfList = new List<string>();
                ValidationMessages.CheckSotrudnikParentStatus(this, sw, Dir, ntfList);
                RenderNtf(w, ntfList, NtfStatus.Error);
            }

            w.Write("</td>");
            w.Write("</tr>");
            w.Write("</table>");
        }


        private void RenderSFolder(TextWriter w)
        {
            w.Write("<tr>");
            w.Write("<td colspan=2 class='TDBB' valign='top'>");
            RenderSFolderRequired(w);
            w.Write("</td>");
            w.Write("<td colspan=2 class='TDBB TDBL' valign='top' style=\"padding-left:2px;\">");
            RenderSFolderComplete(w, Dir.SotrudnikField.ValueString);
            w.Write("</td>");
            w.Write("</tr>");
        }

        private void RenderSFolderRequired(TextWriter w)
        {
            w.Write("<table cellpadding=0 cellspacing=0 width='100%'>");
            w.Write("<tr>");
            w.Write("<td width='100%'>");
            w.Write(Dir.PositionCommonFoldersField.Name);
            w.Write("</td>");
            w.Write("</tr>");
            w.Write("<tr>");
            w.Write("<td colspan=2 class='TDDataPL'>");
            if (!Dir.PositionCommonFolders.Any())
            {
                w.Write(LocalResx.GetString("_Msg_NoRequired"));
            }
            else
            {
                var sortedList = Dir.PositionCommonFolders.OrderBy(o => o.CommonFolderName).ToList();
                sortedList.ForEach(delegate(PositionCommonFolder p) { w.Write("<div>{0}</div>", p.CommonFolderName); });
            }
            w.Write("</td>");
            w.Write("</tr>");
            w.Write("</table>");
        }

        private void RenderSFolderComplete(TextWriter w, string _sId)
        {
            RenderSFolderComplete(w, _sId, true);
        }

        private void RenderSFolderComplete(TextWriter w, string _sId, bool br)
        {
            var commonFolders = Dir.Sotrudnik.CommonFolders;
            if (commonFolders == null)
            {
                if (!Dir.PositionCommonFolders.Any())
                    w.Write(LocalResx.GetString("_Msg_NoData"));
                else
                    GetCompleteInfo(w, "-", false);
                return;
            }

            var sortedList = commonFolders.OrderBy(x => x.Name).ToList();
            
            if (sortedList.Count == 0 && !Dir.PositionCommonFolders.Any())
            {
                w.Write(LocalResx.GetString("_Msg_NoData"));
                return;
            }

            if (sortedList.Count == 0 && Dir.PositionCommonFolders.Any())
            {
                GetCompleteInfo(w, "-", false);
                return;
            }


            sortedList.ForEach(delegate(CommonFolder p)
            {
                var cf = Dir.PositionCommonFolders.FirstOrDefault(x => x.CommonFolderId.ToString() == p.Id);
                var fl = cf != null;
                GetCompleteInfo(w, p.Name, fl);
            });
        }


        private void RenderRoles(TextWriter w)
        {
            w.Write("<tr>");
            w.Write("<td colspan=2 class='TDBB' valign='top'>");
            RenderRolesRequired(w);
            w.Write("</td>");
            w.Write("<td colspan=2 class='TDBB TDBL TDDataPL0' valign='top' style=\"padding-left:2px;\">");
            RenderRoleComplete(this, w, Dir.SotrudnikField.ValueString);
            w.Write("</td>");
            w.Write("</tr>");
        }

        private void RenderRolesRequired(TextWriter w)
        {
            w.Write("<table cellpadding=0 cellspacing=0 width='100%'>");
            w.Write("<tr>");
            w.Write("<td width='100%'>");
            w.Write(Dir.PositionRolesField.Name);
            w.Write("</td>");
            w.Write("</tr>");
            w.Write("<tr>");
            w.Write("<td colspan=2 class='TDDataPL'>");

            if (!Dir.PositionRoles.Any())
            {
                w.Write(LocalResx.GetString("_Msg_NoRequired"));
            }
            else
            {
                var dt = new DataTable("Roles");
                dt.Columns.Add("id");
                dt.Columns.Add("RoleId");
                dt.Columns.Add("Role");
                dt.Columns.Add("RoleDescr");
                dt.Columns.Add("PersonId");
                dt.Columns.Add("Person");

                DataRow dr = null;

                Dir.PositionRoles.ForEach(delegate(PositionRole p)
                {
                    dr = dt.NewRow();
                    dr[0] = p.PositionId;
                    dr[1] = p.RoleId;
                    if (!p.RoleObject.Unavailable)
                    {
                        dr[2] = p.RoleObject.Name;
                        dr[3] = p.RoleObject.Description;
                    }
                    else
                    {
                        dr[2] = "#" + p.RoleId;
                        dr[3] = "";
                    }
                    

                    dr[4] = (p.PersonId == 0) ? "" : p.PersonId.ToString();
                    dr[5] = p.PersonName;

                    dt.Rows.Add(dr);
                });


                var dv = new DataView();
                dv.Table = dt;
                dv.Sort = "Role, RoleId, Person";

                var rowSpan = 0;
                w.Write("<table cellpadding=0 cellspacing=0 width='100%'>");
                for (var i = 0; i < dv.Count; i++)
                {
                    w.Write("<tr>");
                    if (i == 0 || !dv[i]["RoleId"].Equals(dv[i - 1]["RoleId"]))
                    {
                        rowSpan = int.Parse(dt.Compute("COUNT(RoleId)", "RoleId='" + dv[i]["RoleId"] + "'").ToString());

                        w.Write("<td title='{0}' valign='top' rowSpan={1}>", dv[i]["RoleDescr"], rowSpan);
                        if (!dv[i]["RoleId"].Equals(""))
                            w.Write(dv[i]["Role"]);
                        else
                            w.Write("&nbsp;");
                        w.Write("</td>");
                    }
                    w.Write("<td>");
                    w.Write("<table width='100%' height='100%' cellpadding=0 cellspacing=0 border=0>");
                    w.Write("<tr>");
                    w.Write("<td noWrap align='left'>");
                    if (dv[i]["PersonId"].ToString().Length > 0)
                    {
                        RenderLinkPerson(w, "person" + dv[i]["PersonId"], dv[i]["PersonId"].ToString(),
                            dv[i]["Person"].ToString(), NtfStatus.Empty);
                    }
                    else
                        w.Write(LocalResx.GetString("_Rnd_AllCompany"));
                    w.Write("</td>");
                    w.Write("</tr>");
                    w.Write("</table>");
                    w.Write("</td>");
                    w.Write("</tr>");
                }
                w.Write("</table>");
            }
            w.Write("</td>");
            w.Write("</tr>");
            w.Write("</table>");
        }

        private void RenderRoleComplete(DocPage page, TextWriter w, string _sId)
        {
            var listRoles = Dir.Sotrudnik.Roles;

            if (listRoles.Count == 0 && !Dir.PositionRoles.Any())
            {
                w.Write(LocalResx.GetString("_Msg_NoData"));
                return;
            }

            if (listRoles.Count == 0 && Dir.PositionRoles.Any())
            {
                GetCompleteInfo(w, "-", false);
                return;
            }

            var dtR = new DataTable("Roles");
            dtR.Columns.Add("RoleId");
            dtR.Columns.Add("Role");
            dtR.Columns.Add("RoleDescr");
            dtR.Columns.Add("PersonId");
            dtR.Columns.Add("Person");

            DataRow dr = null;

            listRoles.ForEach(delegate(EmployeeRole r)
            {
                dr = dtR.NewRow();
                dr[0] = r.RoleId;
                if (r.RoleId > 0)
                {
                    if (!string.IsNullOrEmpty(r.RoleName))
                    {
                        dr[1] = r.RoleName;
                        dr[2] = r.RoleDescription;
                    }
                    else
                    {
                        dr[1] = "#" + r.RoleId;
                        dr[2] = "";
                    }
                }
                else
                    dr[1] = dr[2] = "";

                dr[3] = r.PersonId == 0 ? "" : r.PersonId.ToString();

                if (dr[3].ToString().Length > 0)
                {
                    if (r.PersonObject == null || r.PersonObject.Unavailable)
                        dr[4] = "#" + r.PersonId;
                    else
                        dr[4] = r.PersonObject.Name;
                }
                else
                    dr[3] = dr[4] = "";
                dtR.Rows.Add(dr);
            });


            var dv = new DataView();
            dv.Table = dtR;
            dv.Sort = "Role, RoleId, Person";

            var rowSpan = 0;
            w.Write("<br><table cellpadding=0 cellspacing=0 width='100%'>");
            var fl = true;
            for (var i = 0; i < dv.Count; i++)
            {
                fl = CheckRoleComleteByRequired(dv[i]);
                w.Write("<tr>");
                if (i == 0 || !dv[i]["RoleId"].Equals(dv[i - 1]["RoleId"]))
                {
                    rowSpan = int.Parse(dtR.Compute("COUNT(RoleId)", "RoleId='" + dv[i]["RoleId"] + "'").ToString());

                    w.Write("<td title='{0}' valign='top' rowSpan={1} style=\"padding-left:2px;\">", dv[i]["RoleDescr"],
                        rowSpan);
                    if (!dv[i]["RoleId"].Equals(""))
                        GetCompleteInfo(w, dv[i]["Role"].ToString(), fl, false);
                    else
                        w.Write("&nbsp;");
                    w.Write("</td>");
                }
                w.Write("<td>");
                w.Write("<table width='100%' height='100%' cellpadding=0 cellspacing=0 border=0>");
                w.Write("<tr>");
                w.Write("<td noWrap align='left'>");
                if (dv[i]["PersonId"].ToString().Length > 0)
                {
                    page.RenderLinkPerson(w, "personC" + dv[i]["PersonId"], dv[i]["PersonId"].ToString(),
                        dv[i]["Person"].ToString(), !fl ? NtfStatus.Error : NtfStatus.Empty);
                }
                else
                    GetCompleteInfo(w, LocalResx.GetString("_Rnd_AllCompany"), fl, false);
                w.Write("</td>");
                w.Write("</tr>");
                w.Write("</table>");
                w.Write("</td>");
                w.Write("</tr>");
            }
            RenderRoleCompleteError(page, dv, w);
            w.Write("</table>");
        }

        private void RenderRoleCompleteError(DocPage page, DataView dv, TextWriter wr)
        {
            var w = new StringWriter();
            var fl = false;

            Dir.PositionRoles.ForEach(delegate(PositionRole p)
            {
                dv.RowFilter = "";
                dv.RowFilter = "RoleId = '" + p.RoleId + "' AND PersonId = '" +
                               (p.PersonId == 0 ? "" : p.PersonId.ToString()) + "'";
                if (dv.Count != 0) return;

                fl = true;
                w.Write("<tr>");
                w.Write("<td style='padding-left:10px'>");
                GetCompleteInfo(w, p.RoleObject.Name, false, false);
                w.Write("</td>");
                w.Write("<td>");

                if (p.PersonId == 0)
                    GetCompleteInfo(w, LocalResx.GetString("_Rnd_AllCompany"), false, false);
                else
                {
                    page.RenderLinkPerson(w, "perr" + p.PersonId, p.PersonId.ToString(), p.PersonName, NtfStatus.Error);
                }

                w.Write("</td>");
                w.Write("</tr>");
            });


            if (fl)
            {
                wr.Write("<tr>");
                wr.Write("<td colspan = 2 style=\"padding-left:2px;\">");
                GetCompleteInfo(wr, "не выполнено:", false, false);
                wr.Write("</td>");
                wr.Write("</tr>");

                wr.Write(w);
            }
        }

        private bool CheckRoleComleteByRequired(DataRowView dr)
        {
            var fl = false;

            Dir.PositionRoles.ForEach(delegate(PositionRole p)
            {
                if (p.RoleId.ToString().Equals(dr["RoleId"].ToString()) &&
                    ((p.PersonId == 0) ? "" : p.PersonId.ToString()).Equals(
                        dr["PersonId"].ToString().Equals("0") ? "" : dr["PersonId"].ToString()
                        )
                    )
                {
                    fl = true;
                }
            });


            return fl;
        }


        private void RenderTypes(TextWriter w)
        {
            w.Write("<tr>");
            w.Write("<td colspan=2 class='TDBB' valign='top'>");
            RenderTypeRequired(w);
            w.Write("</td>");
            w.Write("<td colspan=2 class='TDBB TDBL TDDataPL0' valign='top' style=\"padding-left:2px;\">");
            RenderTypeComplete(this, w, Dir.SotrudnikField.ValueString);
            w.Write("</td>");
            w.Write("</tr>");
        }

        private void RenderTypeRequired(TextWriter w)
        {
            w.Write("<table cellpadding=0 cellspacing=0 width='100%'>");
            w.Write("<tr>");
            w.Write("<td width='100%'>");
            w.Write(Dir.PositionTypesField.Name);
            w.Write("</td>");
            w.Write("</tr>");
            w.Write("<tr>");
            w.Write("<td colspan=2 class='TDDataPL'>");
            if (!Dir.PositionTypes.Any())
            {
                w.Write(LocalResx.GetString("_Msg_NoRequired"));
            }
            else
            {
                var dt = new DataTable("Type");
                dt.Columns.Add("id");
                dt.Columns.Add("CatalogId");
                dt.Columns.Add("Catalog");
                dt.Columns.Add("CatalogDescr");
                dt.Columns.Add("ThemeId");
                dt.Columns.Add("Theme");

                DataRow dr = null;

                Dir.PositionTypes.ForEach(delegate(PositionType p)
                {
                    dr = dt.NewRow();
                    dr[0] = p.Id;
                    dr[1] = p.CatalogId;
                    if (p.CatalogId.HasValue && p.CatalogObject != null && !p.CatalogObject.Unavailable)
                    {
                        dr[2] = p.CatalogObject.Name;
                        dr[3] = "";
                    }
                    else
                        dr[2] = dr[3] = "";


                    dr[4] = (!p.ThemeId.HasValue || p.ThemeId.Value == 0) ? "" : p.ThemeId.Value.ToString();
                    if (dr[4].ToString().Length > 0)
                    {
                        if (p.ThemeObject == null || p.ThemeObject.Unavailable)
                            dr[5] = "#" + p.ThemeId;
                        else
                            dr[5] = p.ThemeObject.NameTheme;
                    }
                    else
                        dr[4] = dr[5] = "";

                    dt.Rows.Add(dr);
                });


                var dv = new DataView();
                dv.Table = dt;
                dv.Sort = "Catalog, CatalogId, Theme";

                var rowSpan = 0;
                w.Write("<table cellpadding=0 cellspacing=0 width='100%'>");
                for (var i = 0; i < dv.Count; i++)
                {
                    w.Write("<tr>");
                    if (i == 0 || !dv[i]["CatalogId"].Equals(dv[i - 1]["CatalogId"]))
                    {
                        rowSpan =
                            int.Parse(
                                dt.Compute("COUNT(CatalogId)", "CatalogId='" + dv[i]["CatalogId"] + "'").ToString());

                        w.Write("<td noWrap title='{0}' valign='top' rowSpan={1}>", "", rowSpan);
                        if (!dv[i]["CatalogId"].Equals(""))
                        {
                            w.Write(dv[i]["Catalog"]);
                        }
                        else
                            w.Write("<все каталоги>");
                        w.Write("</td>");
                    }
                    w.Write("<td>");
                    w.Write("<table width='100%' height='100%' cellpadding=0 cellspacing=0 border=0>");
                    w.Write("<tr>");
                    w.Write("<td align='left'>");
                    if (dv[i]["ThemeId"].ToString().Length > 0)
                    {
                        w.Write(dv[i]["Theme"]);
                    }
                    else
                        w.Write(LocalResx.GetString("_Rnd_AllTypePerson"));
                    w.Write("</td>");
                    w.Write("</tr>");
                    w.Write("</table>");
                    w.Write("</td>");
                    w.Write("</tr>");
                }
                w.Write("</table>");
            }
            w.Write("</td>");
            w.Write("</tr>");
            w.Write("</table>");
        }

        private void RenderTypeComplete(DocPage page, TextWriter w, string _sId)
        {
            var listTypes = Dir.Sotrudnik.Types;

            if (!listTypes.Any() && !Dir.PositionTypes.Any())
            {
                w.Write(LocalResx.GetString("_Msg_NoData"));
                return;
            }

            if (!listTypes.Any() && Dir.PositionTypes.Any())
            {
                GetCompleteInfo(w, "-", false);
                return;
            }

            var dtR = new DataTable("Type");
            dtR.Columns.Add("CatalogId");
            dtR.Columns.Add("Catalog");
            dtR.Columns.Add("CatalogDescr");
            dtR.Columns.Add("ThemeId");
            dtR.Columns.Add("Theme");

            DataRow dr = null;

            listTypes.ForEach(delegate(EmployeePersonType p)
            {
                dr = dtR.NewRow();
                dr[0] = !p.CatalogId.HasValue ? "" : p.CatalogId.Value.ToString();

                if (dr[0].ToString().Length > 0)
                {
                    dr[1] = p.CatalogObject.Name;
                    dr[2] = "";
                }
                else
                    dr[1] = dr[2] = "";

                dr[3] = !p.ThemeId.HasValue || p.ThemeId.Value == 0 ? "" : p.ThemeId.Value.ToString();
                if (dr[3].ToString().Length > 0)
                {
                    if (p.ThemeObject == null || p.ThemeObject.Unavailable)
                    {
                        if (p.ThemeId != null) dr[4] = "#" + p.ThemeId.Value;
                    }
                    else
                        dr[4] = p.ThemeObject.NameTheme;
                }
                else
                    dr[3] = dr[4] = "";
                dtR.Rows.Add(dr);
            });


            var dv = new DataView();
            dv.Table = dtR;
            dv.Sort = "Catalog, CatalogId, Theme";

            var rowSpan = 0;
            w.Write("<br><table cellpadding=0 cellspacing=0 width='100%' >");
            var fl = true;
            var _catName = "";
            var _thmName = "";

            for (var i = 0; i < dv.Count; i++)
            {
                fl = CheckTypeComleteByRequired(dv[i]);
                w.Write("<tr>");
                if (i == 0 || !dv[i]["CatalogId"].Equals(dv[i - 1]["CatalogId"]))
                {
                    rowSpan =
                        int.Parse(dtR.Compute("COUNT(CatalogId)", "CatalogId='" + dv[i]["CatalogId"] + "'").ToString());
                    w.Write("<td noWrap title='{0}' valign='top' rowSpan={1} style=\"padding-left:2px;\">", "", rowSpan);
                    if (!dv[i]["CatalogId"].Equals("")
                        && !dv[i]["CatalogId"].Equals(DBNull.Value)
                        && !dv[i]["CatalogId"].Equals(0))
                    {
                        _catName = dv[i]["Catalog"].ToString();
                        GetCompleteInfo(w,
                            string.IsNullOrEmpty(_catName) ? "Не удалось получить название каталога!" : _catName, fl,
                            false);
                    }
                    else
                        GetCompleteInfo(w, "<все каталоги>", fl, false);
                    w.Write("</td>");
                }
                w.Write("<td>");
                w.Write("<table width='100%' height='100%' cellpadding=0 cellspacing=0 border=0>");
                w.Write("<tr>");
                w.Write("<td align='left'>");
                if (!dv[i]["ThemeId"].Equals("") && !dv[i]["ThemeId"].Equals(DBNull.Value))
                {
                    _thmName = dv[i]["Theme"].ToString();
                    GetCompleteInfo(w,
                        string.IsNullOrEmpty(_thmName) ? "Не удалось получить название типа лица!" : _thmName, fl, false);
                }
                else
                {
                    var msgAllTypes = LocalResx.GetString("_Rnd_AllTypePerson");
                    GetCompleteInfo(w,
                        string.IsNullOrEmpty(msgAllTypes)
                            ? "Не удалось получить название всех типов лица!"
                            : msgAllTypes, fl, false);
                }
                w.Write("</td>");
                w.Write("</tr>");
                w.Write("</table>");
                w.Write("</td>");
                w.Write("</tr>");
            }
            RenderTypeCompleteError(page, dv, w);
            w.Write("</table>");
        }

        private void RenderTypeCompleteError(DocPage page, DataView dv, TextWriter wr)
        {
            var listTypes = Dir.PositionTypes;
            var w = new StringWriter();

            var fl = false;
            var nameEntity = "";


            listTypes.ForEach(delegate(PositionType p)
            {
                nameEntity = "";
                dv.RowFilter = "";

                dv.RowFilter = "CatalogId = '" +
                               (!p.CatalogId.HasValue || p.CatalogId.Value == 0 ? "" : p.CatalogId.Value.ToString()) +
                               "' AND ThemeId = '" +
                               (!p.ThemeId.HasValue || p.ThemeId.Value == 0 ? "" : p.ThemeId.Value.ToString()) + "'";
                if (dv.Count != 0) return;


                fl = true;
                w.Write("<tr>");
                w.Write("<td style='padding-left:10px'>");

                if (p.CatalogObject == null || p.CatalogObject.Unavailable)
                {
                    if (!p.CatalogId.HasValue)
                        nameEntity = LocalResx.GetString("_Rnd_AllCatalog");
                    else
                        nameEntity = "#" + p.CatalogId.Value;
                }
                else
                    nameEntity = p.CatalogObject.Name;

                GetCompleteInfo(w, nameEntity, false, false);
                w.Write("</td>");
                w.Write("<td>");

                if (!p.ThemeId.HasValue || p.ThemeId.Value == 0)
                    GetCompleteInfo(w, LocalResx.GetString("_Rnd_AllTypePerson"), false, false);
                else
                {
                    if (p.ThemeObject == null || p.ThemeObject.Unavailable)
                    {
                        nameEntity = "#" + p.ThemeId.Value;
                    }
                    else
                        nameEntity = p.ThemeObject.NameTheme;

                    GetCompleteInfo(w, nameEntity, false, false);
                }

                w.Write("</td>");
                w.Write("</tr>");
            });


            if (!fl) return;

            wr.Write("<tr>");
            wr.Write("<td colspan = 2 style=\"padding-left:2px;\">");
            GetCompleteInfo(wr, "не выполнено:", false, false);
            wr.Write("</td>");
            wr.Write("</tr>");

            wr.Write(w);
        }

        private bool CheckTypeComleteByRequired(DataRowView dr)
        {
            var listTypes = Dir.PositionTypes;
            var fl = false;

            listTypes.ForEach(delegate(PositionType p)
            {
                if ((p.CatalogId.HasValue && p.CatalogId.Value.ToString().Equals(dr["CatalogId"].ToString()) ||
                     !p.CatalogId.HasValue && (dr["CatalogId"].Equals(DBNull.Value) || dr["CatalogId"].Equals(""))) &&
                    (p.ThemeId.HasValue && p.ThemeId.Value.ToString().Equals(dr["ThemeId"].ToString()) ||
                     !p.ThemeId.HasValue && dr["ThemeId"].Equals(DBNull.Value) || dr["ThemeId"].Equals("")))
                {
                    fl = true;
                }
            });

            return fl;
        }


        private void RenderAdvancedGrants(TextWriter w)
        {
            if (!Dir.PositionAdvancedGrants.Any()) return;
            w.Write("<tr>");
            w.Write("<td colspan=4 class='TDBB' valign='top'>");
            RenderAdvancedGrantsRequired(w);
            w.Write("</td>");
            w.Write("</tr>");
        }

        private void RenderAdvancedGrantsRequired(TextWriter w)
        {
            w.Write("<table cellpadding=0 cellspacing=0 width='100%'>");
            w.Write("<tr>");
            w.Write("<td width='100%'>");
            w.Write(Dir.PositionAdvancedGrantsField.Name);
            w.Write("</td>");
            w.Write("</tr>");
            w.Write("<tr>");
            w.Write("<td colspan=2 class='TDDataPL'>");

            Dir.PositionAdvancedGrants.OrderBy(x => x.GrantDescription).ToList().ForEach(
                delegate(PositionAdvancedGrant p) { w.Write("<div>{0}</div>", p.GrantDescription); });


            w.Write("</td>");
            w.Write("</tr>");
            w.Write("</table>");
        }


        private void RenderADSIInfoByLogin(TextWriter w, string login)
        {
            var sqlParams = new Dictionary<string, object>
            {
                {"@Login", login}
            };

            using (
                var dbReader = new DBReader(SQLQueries.SELECT_ADSI_ПоЛогину, CommandType.Text, Config.DS_user, sqlParams)
                )
            {
                if (!dbReader.HasRows)
                {
                    w.Write("");
                    return;
                }

                var colPath = dbReader.GetOrdinal("Path");

                while (dbReader.Read())
                {
                    var path = dbReader.GetString(colPath);

                    var regex = new Regex("(OU=.+?(,|$))", RegexOptions.IgnoreCase);
                    var matches = regex.Matches(path);
                    w.Write("<div>");
                    var adsi = matches.Cast<object>().Aggregate("", (current, m) => current + m);

                    if (adsi.Right(1) == ",")
                        adsi = adsi.Left(adsi.Length - 1);
                    w.Write(adsi);

                    w.Write("</div>");

                    return;
                }
            }
        }

        private void RenderNote(TextWriter w)
        {
            if (Dir.AdvInfoField.ValueString.Length != 0)
            {
                w.Write("<tr>");
                w.Write("<td colspan=4 class='TDBB'>");
                w.Write("<table cellpadding=0 cellspacing=0 width='100%'>");
                w.Write("<tr>");
                w.Write("<td width='100%'>");
                w.Write(Dir.AdvInfoField.Name);
                w.Write("</td>");
                w.Write("</tr>");
                w.Write("<tr>");
                w.Write("<td colspan=2 class='TDDataPL'>");
                w.Write(Dir.AdvInfoField.ValueString);
                w.Write("</td>");
                w.Write("</tr>");
                w.Write("</table>");
                w.Write("</td>");
                w.Write("</tr>");
            }
        }


        protected void RenderNoSignSupervisor(TextWriter w)
        {
            if (Dir.SupervisorField.ValueString.Length == 0 || Dir.Supervisor.Unavailable) return;
            if (Dir.Supervisor.Login.Length == 0) return;

            bool fl = Dir.DocSigns.Where(t => !t.Unavailable).Any(t => t.EmployeeId == Dir.SupervisorField.ValueInt || t.EmployeeInsteadOf == Dir.SupervisorField.ValueInt);

            if (fl) return;

            w.Write("<div id='spNoSignSupervisor' style='COLOR:red; float:right; margin-right:50px;'>");
            w.Write(LocalResx.GetString("_Msg_NoSignSupervisor"));
            w.Write("</div>");
        }
        #endregion
    }
}