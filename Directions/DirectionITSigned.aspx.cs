using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Resources;
using System.Security.Principal;
using System.Text;
using System.Web;
using Kesco.Lib.BaseExtention;
using Kesco.Lib.BaseExtention.Enums.Controls;
using Kesco.Lib.Entities;
using Kesco.Lib.Entities.Documents;
using Kesco.Lib.Entities.Documents.EF.Directions;
using Kesco.Lib.Log;
using Kesco.Lib.Web.Controls.V4;
using Kesco.Lib.Web.Controls.V4.Common.DocumentPage;
using Kesco.Lib.Web.Settings;
using System.Text.RegularExpressions;
using Kesco.Lib.DALC;

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


            if (!wp.IsInRole("TEST\\Programists")
                &&
                !wp.IsInRole("EURO\\Domain Admins")
                ) return;


            var btnExecute = new Button
            {
                ID = "btnExecute",
                V4Page = this,
                Text = LocalResx.GetString("btnGO"),
                Title = "Выполнить указание",
                IconJQueryUI = ButtonIconsEnum.Ok,
                Width = 105,
                OnClick = "Wait.render(true); cmdasync('cmd','Execute');"
            };
            AddMenuButton(btnExecute);
        }

        protected override void DocumentInitialization(Document copy = null)
        {
            if (copy == null)
                Doc = new Direction();
            else
                Doc = (Direction) copy;

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

            if (Dir.RedirectNumField.ValueString.Length == 0 &&
                (bitMask & 1) != 1 && (bitMask & 2) != 2 && (bitMask & 4) != 4) fl = false;

            w.Write(Dir.RedirectNumField.Name + ":");
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
            w.Write("<td colspan=2 class='TDBB TDBL' valign='top'>");
            RenderInsidePhoneComplete(w);
            RenderPhoneSimComplete(w, false);
            w.Write("</td>");
            w.Write("</tr>");
        }
        private void RenderPhoneEquipRequired(TextWriter w)
        {
            var w1 = new StringWriter();
            var w10 = new StringWriter();
            var w200 = new StringWriter();

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
                var bitMask = Dir.PhoneEquipField.ValueInt;
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


                if ((bitMask & 16) == 16)
                {
                    val = lPhoneSim;
                    if ((bitMask & 32) == 32)
                        val += "&nbsp;+&nbsp;" + "Мобильный интернет";
                    col.Add(lPhoneSim);
                }
                for (var i = 0; i < col.Count; i++)
                {
                    if (i > 0) w.Write("<br>");
                    w.Write(col[i]);
                }
            }
            w.Write("</td>");
            w.Write("<td valign='top' noWrap width='100%' align='left' style='PADDING-LEFT:30px'>");
            if (Dir.PhoneLinkField.ValueString.Length > 0)
            {
                var bitMask = Dir.PhoneLinkField.ValueInt;

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

            var flC = (Dir.PhoneEquipField.ValueString.Length == 0 ||
                       (Dir.PhoneEquipField.ValueString.Length != 0 && Dir.PhoneEquipField.ValueInt == 16));

            sb.AppendFormat("<table cellpadding=0 cellspacing=0 {0}>", flC ? "class='NoCI'" : "");

            dv.Table = dtEquip;

            //dv.RowFilter = "(НомерТелефона IS NOT NULL AND ЕстьХарактеристикиSIM=0) OR ЕстьТелефонныйНомер = 1";
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

                    sb.AppendFormat("&nbsp;-&nbsp;{0}", wr);

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
        private void RenderPhoneSimComplete(TextWriter w, bool fl)
        {
            DataView dv = new DataView();
            dv.Table = dtEquip;

            dv.RowFilter = "ЕстьХарактеристикиSIM=1";
            dv.Sort = "КодТипаОборудования, Оборудование";

            int bitmask = (Dir.PhoneEquipField.ValueString.Length == 0) ? 0 : Dir.PhoneEquipField.ValueInt;

            bool flC = ((bitmask & 16) != 16);

            if (dv.Count == 0)
            {
                if (!flC) GetCompleteInfo(w, "-", false, false);
                else w.Write("");
                return;
            }
            string _phoneNum = "";
            for (int i = 0; i < dv.Count; i++)
            {
                if (i > 0) w.Write("<br>");

                w.Write("<span ><img src='/styles/sim.gif' border=0>");

                var className = (flC ? "class='NoCI'" : "");
                var empls = Render.EquipmentEmployee(this, null, Dir, dv[i]["КодОборудования"].ToString(), className);

                RenderLinkEquipment(w, dv[i]["КодОборудования"].ToString(), className, "title=\"" + LocalResx.GetString("_Msg_OpenEquip") + "\"");

                w.Write(LocalResx.GetString("_Msg_ВыданаSim"));
                if (!dv[i]["НомерТелефона"].Equals(DBNull.Value))
                {
                    _phoneNum = dv[i]["НомерТелефона"].ToString();
                    if (_phoneNum.Length > 6) Dir.FormatingMobilNumber(ref _phoneNum);
                    w.Write(" [{0}]", _phoneNum);

                }
                RenderLinkEnd(w);

                w.Write(empls);
            }
        }

        private void RenderCompEquip(TextWriter w)
        {
            w.Write("<tr>");
            w.Write("<td colspan=2 class='TDBB' valign='top'>");
                RenderCompEquipRequired(w);
            w.Write("</td>");
            w.Write("<td colspan=2 class='TDBB TDBL' valign='top'>");
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
                int bitMask = Dir.CompTypeField.ValueInt;

                StringCollection col = new StringCollection();

                if ((bitMask & 1) == 1)
                    col.Add(lCompT);

                if ((bitMask & 2) == 2)
                    col.Add(lCompP);

                if ((bitMask & 4) == 4)
                    col.Add(lCompN);


                for (int i = 0; i < col.Count; i++)
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
            DataView dv = new DataView();
            dv.Table = dtEquip;

            bool flC = Dir.CompTypeField.ValueString.Length > 0;

            dv.RowFilter = "ЕстьХарактеристикиКомпьютера=1 OR ЕстьХарактеристикиМонитора=1";
            dv.Sort = "КодТипаОборудования, Оборудование";
            if (dv.Count == 0)
            {
                GetCompleteInfo(w, "-", !flC);
                return;
            }


            for (int i = 0; i < dv.Count; i++)
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
            if (Dir.WorkPlaceField.ValueString.Length == 0) return;
            w.Write("<tr>");
            w.Write("<td colspan=2 class='TDBB' valign='top'>");
                RenderAdvEquipRequired(w);
            w.Write("</td>");
            w.Write("<td colspan=2 class='TDBB TDBL' valign='top'>");
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
            DataView dv = new DataView();
            dv.Table = dtEquip;

            //dv.RowFilter = "ЕстьХарактеристикиКомпьютера=0 AND ЕстьХарактеристикиМонитора=0 AND ЕстьХарактеристикиSIM=0 AND ЕстьТелефонныйНомер=0 AND НомерТелефона IS NULL";
            dv.RowFilter = "ЕстьХарактеристикиКомпьютера=0 AND ЕстьХарактеристикиМонитора=0 AND ЕстьХарактеристикиSIM=0 AND НомерТелефона IS NULL";
            dv.Sort = "КодТипаОборудования, Оборудование";

            if (dv.Count == 0)
            {
                w.Write(LocalResx.GetString("_Msg_NoData"));
                return;
            }


            bool flC = Dir.AdvInfoField.ValueString.Length > 0;

            for (int i = 0; i < dv.Count; i++)
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
            w.Write("<td colspan=2 class='TDBB TDBL' valign='top'>");
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

            int bitMask = Dir.AccessEthernetField.ValueString.Length == 0 ? 0 : Dir.AccessEthernetField.ValueInt;
            int bitMaskWp = Dir.WorkPlaceTypeField.ValueString.Length == 0 ? 0 : Dir.WorkPlaceTypeField.ValueInt;

            StringCollection col = new StringCollection();

            if (bitMask == 1)
                col.Add((Dir.LoginField.ValueString.Length > 0) ? Dir.LoginField.ValueString : lAEOfiice);
            else
                col.Add(LocalResx.GetString("_Msg_NoRequired") + lAEOfiice);

            if ((bitMaskWp & 4) == 4)
                col.Add(lAEVpn);
            else
                col.Add(LocalResx.GetString("_Msg_NoRequired") + " " + lAEVpn);

            for (int i = 0; i < col.Count; i++)
            {
                if (i > 0) w.Write("<br>");
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

            int bitMask = Dir.AccessEthernetField.ValueString.Length == 0 ? 0 : Dir.AccessEthernetField.ValueInt;
            bool fl = false;

           
            if (Dir.SotrudnikField.ValueString.Length > 0 && !Dir.Sotrudnik.Unavailable)
            {
                if (Dir.Sotrudnik.Login != null)
                {
                    fl = (Dir.LoginField.ValueString.ToLower().Equals(Dir.Sotrudnik.Login.ToLower()));
                }

                if (!string.IsNullOrEmpty(Dir.Sotrudnik.Login))
                {
                    string _f = Dir.Sotrudnik.PersonalFolder;
                    string _href = Regex.Replace(_f, "\\\\[^\\\\]+$", "").Replace(":", "|").Replace("\\", "/");
                    GetCompleteInfo(w, "<nobr>" + Dir.Sotrudnik.Login + " " + "<a href='file:///" + _href + "' target='_blabk'>" + _f + "</a>" + "</nobr>", fl);
                    RendeADSIInfoByLogin(w, Dir.LoginField.ValueString);
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

        private void RendeADSIInfoByLogin(TextWriter w, string login)
        {
            var sqlParams = new Dictionary<string, object>
            {
                {"@Login", login},
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

                    Regex regex = new Regex("(OU=.+?(,|$))", RegexOptions.IgnoreCase);
                    MatchCollection matches = regex.Matches(path);
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




        #endregion

        
    }
}