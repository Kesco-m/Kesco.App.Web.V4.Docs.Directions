using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using Kesco.Lib.BaseExtention;
using Kesco.Lib.BaseExtention.Enums.Controls;
using Kesco.Lib.DALC;
using Kesco.Lib.Entities;
using Kesco.Lib.Entities.Corporate;
using Kesco.Lib.Entities.Documents.EF.Directions;
using Kesco.Lib.Entities.Persons;
using Kesco.Lib.Entities.Persons.PersonOld;
using Kesco.Lib.Log;
using Kesco.Lib.Web.Controls.V4;
using Kesco.Lib.Web.Controls.V4.Common.DocumentPage;
using Kesco.Lib.Web.Settings;

namespace Kesco.App.Web.Docs.Directions
{
    /// <summary>
    ///     Подключаемый класс формы документа: Указание IT на организацию работы - нередактируемый вид
    /// </summary>
    public partial class DirectionItSigned : DocPage
    {
        //========================================================================================================== Execute
        /// <summary>
        ///     Выполнение указания
        /// </summary>
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
                    if (cm.Parameters["@RETURN_VALUE"].Value.ToString().Equals("1"))
                        RefreshFormData();
            }
            catch (Exception ex)
            {
                throw new DetailedException(ex.Message, ex, cm);
            }
            finally
            {
                if (cm.Connection != null && cm.Connection.State.Equals(ConnectionState.Open))
                    cm.Connection.Close();
            }
        }

        #region CONST

        protected string HAccessEml = "";
        protected string HDadaEml = "";
        protected string HEquipEml = "";
        protected string LAeOffice = "";
        protected string LAeVpn = "";
        protected string LAiMobile = "";
        protected string LAiModem = "";
        protected string LAiOffice = "";
        protected string LCompN = "";
        protected string LCompP = "";
        protected string LCompT = "";
        protected string LPhoneDect = "";
        protected string LPhoneDesk = "";
        protected string LPhoneIp = "";
        protected string LPhoneIpCam = "";
        protected string LPhoneSim = "";
        protected string LSotrudnikParent1 = "";
        protected string LSotrudnikParent2 = "";


        protected string LPlExitInSide = "";
        protected string LPlExitOutCountry = "";
        protected string LPlExitOutTown = "";
        protected string LPlExitTown = "";

        protected string LRequire = "";
        protected string LComplete = "";

        #endregion

        //========================================================================================================== Globals

        #region Глобальные объекты

        /// <summary>
        ///     Группа посменной работы без логина
        /// </summary>
        private bool _isCommonEmployeeNoLogin;

        /// <summary>
        ///     Объект отрисовки дополнительной информации
        /// </summary>
        private RenderHelper _render;

        /// <summary>
        ///     Таблица с выполненными по указанию данными по оборудорванию
        /// </summary>
        protected DataTable DtEquip;

        /// <summary>
        ///     Объект текущего документа Указание IT на организацию работы
        /// </summary>
        protected Direction Dir => (Direction) Doc;

        /// <summary>
        ///     Инициализация объекта отрисовки информации
        /// </summary>
        protected RenderHelper RenderData => _render ?? (_render = new RenderHelper());

        #endregion

        //========================================================================================================== Override

        #region Override

        /// <summary>
        ///     Если форма в нередактируемом виде - переадресуем
        /// </summary>
        /// <returns></returns>
        protected override bool RedirectPageByCondition()
        {
            if (!IsOpenInEditableMode(Request)) return base.RedirectPageByCondition();

            V4Redirect("DirectionIT.aspx");
            return true;
        }

        /// <summary>
        ///     Обработчик события загрузки страницы
        /// </summary>
        /// <param name="sender">Страница</param>
        /// <param name="e">Аргументы</param>
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!V4IsPostBack)
            {
                SetCultureText();
                CreateDataTableEquipment();
            }

            JS.Write(@"directions_clientLocalization = {{
DIRECTIONS_FORM_ADVINFO_Title:""{0}"",
}};",
                Resx.GetString("DIRECTIONS_FORM_ADVINFO_Title")
            );
        }

        /// <summary>
        ///     Настройка панели инструментов электронной формы
        /// </summary>
        protected override void SetDocMenuButtons()
        {
            base.SetDocMenuButtons();

            var wi = WindowsIdentity.GetCurrent();
            var wp = new WindowsPrincipal(wi);


            if (wp.IsInRole("TEST\\Programists") || wp.IsInRole("EURO\\Domain Admins")
            )
            {
                var btnExecute = new Button
                {
                    ID = "btnExecute",
                    V4Page = this,
                    Text = Resx.GetString("DIRECTIONS_btnGO"),
                    Title = Resx.GetString("DIRECTIONS_btnGO_Title"),
                    IconJQueryUI = ButtonIconsEnum.Ok,
                    Width = 105,
                    OnClick = "cmdasync('cmd','Execute');"
                };
                AddMenuButton(btnExecute);
            }

            var btnOldVersion = new Button
            {
                ID = "btnOldVersion",
                V4Page = this,
                Text = Resx.GetString("btnOldVersion"),
                Title = Resx.GetString("btnOldVersion_Title"),
                IconJQueryUI = ButtonIconsEnum.Alert,
                Width = 150,
                OnClick =
                    $"v4_windowOpen('{HttpUtility.JavaScriptStringEncode(WebExtention.UriBuilder(ConfigurationManager.AppSettings["URI_Direction_OldVersion"], CurrentQS))}','_self');"
            };
            AddMenuButton(btnOldVersion);
        }

        /// <summary>
        ///     Инициализация сущности документа
        /// </summary>
        /// <param name="copy">Инициализация по копии</param>
        protected override void EntityInitialization(Entity copy = null)
        {
            if (copy == null)
                Doc = new Direction();
            else
                Doc = (Direction) copy;
            ShowCopyButton = false;
            ShowDocDate = false;
        }

        /// <summary>
        ///     Загрузка данных текущего документа
        /// </summary>
        /// <param name="idEntity">Идентификатор документа</param>
        protected override void EntityLoadData(string idEntity)
        {
            base.EntityLoadData(idEntity);
            if (idEntity.IsNullEmptyOrZero()) return;
            Dir.LoadDocumentPositions();
        }

        /// <summary>
        ///     Первичное заполнение элементов управления на основании свойств документа
        /// </summary>
        protected override void DocumentToControls()
        {
        }

        /// <summary>
        ///     Установка свойств элементов управления
        /// </summary>
        protected override void SetControlProperties()
        {
        }

        /// <summary>
        ///     Обработчик клиентских команд
        /// </summary>
        /// <param name="cmd">Команда</param>
        /// <param name="param">Коллекция параметров</param>
        protected override void ProcessCommand(string cmd, NameValueCollection param)
        {
            switch (cmd)
            {
                case "Execute":
                    Execute();
                    break;
                case "OpenAnotherEquipmentDetails":
                    RenderData.EquipmentAnotherPlace(this, "divAdvInfoValidation_Body", Dir);
                    break;
                case "OpenEquipmentDetails":
                    RenderData.EquipmentInPlace(this, "divAdvInfoValidation_Body", Dir, param["IdLocation"]);
                    break;
                case "OpenGroupMembers":
                    RenderData.GroupMembers(this, "divAdvInfoValidation_Body", Dir, param["IdGroup"]);
                    break;
                default:
                    base.ProcessCommand(cmd, param);
                    break;
            }
        }

        /// <summary>
        ///     Перерисовка данных формы без перезагрузки
        /// </summary>
        protected override void RefreshManualNotifications()
        {
            base.RefreshManualNotifications();
            RefreshFormData();
        }

        #endregion


        #region Render

        //========================================================================================================== Render advanced function

        #region Render advanced function

        /// <summary>
        ///     Получение формата вывода сообщения
        /// </summary>
        /// <param name="w">Поток вывода</param>
        /// <param name="msg">Сообщение</param>
        private void GetNtfFormatMsg(TextWriter w, string msg)
        {
            GetNtfFormatMsg(w, msg, true);
        }

        /// <summary>
        ///     Получение формата вывода данных
        /// </summary>
        /// <param name="w">Поток вывода</param>
        /// <param name="msg">Данные</param>
        /// <param name="br">Нужен перенос после вывода</param>
        private void GetNtfFormatMsg(TextWriter w, string msg, bool br)
        {
            w.Write("{1}<font class='v4NtfError'>{0}</font>", msg, br ? "<br>" : "");
        }

        /// <summary>
        ///     Вывод данных в колонке выполнено
        /// </summary>
        /// <param name="w">Поток вывода</param>
        /// <param name="msg">Данные</param>
        /// <param name="fl">Отличается от требуемого</param>
        /// <param name="marginL">Нужен ли отступ слева</param>
        private void GetCompleteInfo(TextWriter w, string msg, bool fl, bool marginL = false)
        {
            GetCompleteInfo(w, msg, fl, true, marginL);
        }

        /// <summary>
        ///     Вывод данных в колонке выполнено
        /// </summary>
        /// <param name="w">Поток вывода</param>
        /// <param name="msg">Данные</param>
        /// <param name="fl">Отличается от требуемого</param>
        /// <param name="br">Нужен перевод на новуб строку после вывода</param>
        /// <param name="marginL">Нужен ли отступ слева</param>
        private void GetCompleteInfo(TextWriter w, string msg, bool fl, bool br, bool marginL = false)
        {
            if (!string.IsNullOrEmpty(msg) && msg == Resx.GetString("DIRECTIONS_lNoComplete") && fl)
            {
                w.Write("<br>&nbsp;");
                return;
            }

            var str =
                $"{(br ? "<br>" : "")}<span class=\"{(!fl ? "NoCI" : "")}{(marginL ? " marginL" : "")}\">{(string.IsNullOrEmpty(msg) ? "" : msg)}</span>";

            w.Write(str);
        }

        #endregion


        //========================================================================================================== Refresh

        #region Refresh

        /// <summary>
        ///     Обновление фото сотрудника
        /// </summary>
        private void RefreshPhoto()
        {
            using (var w = new StringWriter())
            {
                RenderData.Photo(this, w, Dir);
                JS.Write("var objPH=document.getElementById('divPhoto'); if (objPH) objPH.innerHTML = '{0}';",
                    HttpUtility.JavaScriptStringEncode(w.ToString()));
            }
        }

        /// <summary>
        ///     Обновление информации о сотруднике
        /// </summary>
        private void RefreshSotrudnikInfo()
        {
            using (var w = new StringWriter())
            {
                RenderData.Sotrudnik(this, w, Dir);
                JS.Write("var objSI=document.getElementById('divSotrudnikInfo'); if (objSI) objSI.innerHTML = '{0}';",
                    HttpUtility.JavaScriptStringEncode(w.ToString()));
            }
        }

        /// <summary>
        ///     Обновление должности сотрудника
        /// </summary>
        private void RefreshSotrudnikPost()
        {
            using (var w = new StringWriter())
            {
                RenderData.SotrudnikPost(this, w, Dir);
                JS.Write("var objSP=document.getElementById('spSotrudnikPost'); if (objSP) objSP.innerHTML = '{0}';",
                    HttpUtility.JavaScriptStringEncode(w.ToString()));
            }
        }

        /// <summary>
        ///     Обновление работодателя сотрудника
        /// </summary>
        private void RefreshSotrudnikAOrg()
        {
            using (var w = new StringWriter())
            {
                RenderData.SotrudnikAOrg(this, w, Dir);
                JS.Write(
                    "var objSAOrg=document.getElementById('spSotrudnikAOrg'); if (objSAOrg) objSAOrg.innerHTML = '{0}';",
                    HttpUtility.JavaScriptStringEncode(w.ToString()));
            }
        }

        /// <summary>
        ///     Обновление лица заказчика сотрудника
        /// </summary>
        private void RefreshSotrudnikFinOrg()
        {
            using (var w = new StringWriter())
            {
                RenderData.SotrudnikFinOrg(this, w, Dir);
                JS.Write(
                    "var objSFOrg=document.getElementById('divSotrudnikFinOrg'); if (objSFOrg) objSFOrg.innerHTML = '{0}';",
                    HttpUtility.JavaScriptStringEncode(w.ToString()));
            }
        }

        /// <summary>
        ///     Обновление информации об общем сотруднике
        /// </summary>
        private void RefreshCommonEmployee()
        {
            using (var w = new StringWriter())
            {
                RenderData.CommonEmployee(this, w, Dir);
                JS.Write("var objCE=document.getElementById('divCommonEmployee'); if (objCE) objCE.innerHTML = '{0}';",
                    HttpUtility.JavaScriptStringEncode(w.ToString()));
            }
        }

        /// <summary>
        ///     Обновление информации о рабочих местах сотрудника
        /// </summary>
        private void RefreshCadrWorkPlaces()
        {
            using (var w = new StringWriter())
            {
                RenderData.SotrudnikCadrWorkPlaces(this, w, Dir);
                JS.Write(
                    "var objSCP=document.getElementById('divSotrudnikCadrWorkPlaces'); if (objSCP) objSCP.innerHTML = '{0}';",
                    HttpUtility.JavaScriptStringEncode(w.ToString()));
            }
        }

        /// <summary>
        ///     Обновление информации о руководителе сотрудника
        /// </summary>
        private void RefreshSupervisor()
        {
            using (var w = new StringWriter())
            {
                RenderData.Supervisor(this, w, Dir);
                JS.Write("var objSS=document.getElementById('divSupervisor'); if (objSS) objSS.innerHTML = '{0}';",
                    HttpUtility.JavaScriptStringEncode(w.ToString()));
            }
        }

        /// <summary>
        ///     Обновление информации о том, что требуется организовать
        /// </summary>
        private void RefreshWorkPlaceType()
        {
            using (var w = new StringWriter())
            {
                RenderData.WorkPlaceType(this, w, Dir, false);
                JS.Write(
                    "var objWPT=document.getElementById('divSotrudnikWPType'); if (objWPT) objWPT.innerHTML = '{0}';",
                    HttpUtility.JavaScriptStringEncode(w.ToString()));
            }
        }


        /// <summary>
        ///     обновление информации о мобильном телефоне
        /// </summary>
        private void RefreshMobilPhone()
        {
            using (var w = new StringWriter())
            {
                RenderMobilPhone(w);
                JS.Write("var objMP=document.getElementById('divMobilPhone'); if (objMP) objMP.innerHTML = '{0}';",
                    HttpUtility.JavaScriptStringEncode(w.ToString()));
            }
        }

        #endregion

        //========================================================================================================== ADSI

        #region Base Render

        /// <summary>
        ///     Обновление колонки выполнено
        /// </summary>
        private void RefreshFormData()
        {
            Dir.RequiredRefreshInfo = true;
            Dir.SotrudnikRequiredRefreshInfo = true;
            Dir.SotrudnikParentRequiredRefreshInfo = true;

            RefreshPhoto();
            RefreshSotrudnikInfo();
            RefreshSotrudnikPost();
            RefreshSotrudnikAOrg();
            RefreshSotrudnikFinOrg();
            RefreshCommonEmployee();
            RefreshCadrWorkPlaces();
            RefreshSupervisor();
            RefreshWorkPlaceType();
            RefreshMobilPhone();

            var w = new StringWriter();
            CreateDataTableEquipment();
            RenderFormData(w);
            JS.Write("var objData=document.getElementById('divData'); if(objData) objData.innerHTML='{0}';",
                HttpUtility.JavaScriptStringEncode(w.ToString()));
        }

        /// <summary>
        ///     Вывод данных в колонке выполнено
        /// </summary>
        /// <param name="w">Поток вывода</param>
        protected void RenderFormData(TextWriter w)
        {
            w.Write("<table class='bgcolor marginT2' cellpadding=0 cellspacing=0 style='width:99%;' >");

            _isCommonEmployeeNoLogin = IsCommonEmployeeNoLogin();

            RenderHeader(w);
            if (!_isCommonEmployeeNoLogin) // Для сотрудника в группе с УЗ выводим только Логин, Email
            {
                RenderPhoneEquip(w);
                RenderAdvancedGrants(w, 4);
                RenderCompEquip(w);
                RenderAdvancedGrants(w, 2);
                RenderAdvEquip(w);
            }


            RenderAeAccess(w);
            RenderLanguage(w);
            RenderEmail(w);

            RenderAdvancedGrants(w, 1);
            RenderSotrudnikParent(w);
            RenderSFolder(w);
            RenderRoles(w);
            RenderTypes(w);

            RenderNote(w);
            w.Write("</table>");
        }

        /// <summary>
        ///     Вывода предупреждения, что указание не подписано непосредственным руководителем
        /// </summary>
        /// <param name="w">Поток вывода</param>
        protected void RenderNoSignSupervisor(TextWriter w)
        {
            var super = Dir.Sotrudnik.Supervisor;

            if (super == null || super.Unavailable) return;
            if (super.Login.Length == 0) return;

            var fl = Dir.DocSigns.Where(t => !t.Unavailable).Any(t => t.EmployeeId.Equals(int.Parse(super.Id)));
            if (fl || Dir.ChangePersonID == int.Parse(super.Id)) return;

            w.Write(
                "<div id='spNoSignSupervisor' class='v4Error v4ContextHelp' style='float:right; margin-right:1%;' title='{0}'>",
                Resx.GetString("DIRECTIONS_Msg_NoSignSupervisor_Title"));
            w.Write(Resx.GetString("DIRECTIONS_Msg_NoSignSupervisor"));
            w.Write("</div>");
        }

        /// <summary>
        ///     Вывод заголока таблицы Требуется/Выполнено
        /// </summary>
        /// <param name="w">Поток вывода</param>
        private void RenderHeader(TextWriter w)
        {
            w.Write("<tr>");
            w.Write("<td width='50%' align='center' colspan=2>");
            w.Write("<b>{0}</b>", LRequire);
            w.Write("</td>");
            w.Write("<td width='50%' align='center' colspan=2>");
            w.Write("<b>{0}</b>", LComplete);
            w.Write("</td>");
            w.Write("</tr>");
        }

        #endregion

        //========================================================================================================== Оборудование

        #region Оборудование

        //========================================================================================================== Телефонное оборудование

        #region Телефонное оборудование

        /// <summary>
        ///     Вывод информации о телефонном оборудовании
        /// </summary>
        /// <param name="w">Поток вывода</param>
        private void RenderPhoneEquip(TextWriter w)
        {
            var dv = GetInsidePhoneComplete();

            w.Write("<tr>");
            w.Write("<td colspan=2 class='TDBB' valign='top'>");
            RenderPhoneEquipRequired(w);
            w.Write("</td>");
            w.Write("<td colspan=2 class='TDBB TDBL' valign='top' style=\"padding-left:2px;\">");
            RenderInsidePhoneComplete(w, dv);
            w.Write("</td>");
            w.Write("</tr>");
        }

        /// <summary>
        ///     Вывод информации о телефонном оборудовании - что требуется
        /// </summary>
        /// <param name="w">Поток вывода</param>
        private void RenderPhoneEquipRequired(TextWriter w)
        {
            var bitMask = 0;

            w.Write("<table cellpadding=0 cellspacing=0 width='100%'>");
            w.Write("<tr>");
            w.Write("<td width='100%' colspan=3>");
            w.Write(Resx.GetString("DIRECTIONS_Field_Phone") + ":");
            w.Write("</td>");
            w.Write("</tr>");
            w.Write("<tr>");
            w.Write("<td colspan=2 class='TDDataPL' nowrap>");
            if (Dir.PhoneEquipField.ValueString.Length == 0)
            {
                w.Write(Resx.GetString("DIRECTIONS_Msg_NoRequired"));
            }
            else
            {
                bitMask = Dir.PhoneEquipField.ValueInt;
                var col = new StringCollection();

                if ((bitMask & 1) == 1)
                {
                    var val = LPhoneDesk;
                    if ((bitMask & 8) == 8)
                        val += "&nbsp;+&nbsp;" + LPhoneIpCam;
                    col.Add(val);
                }

                if ((bitMask & 2) == 2) col.Add(LPhoneDect);

                for (var i = 0; i < col.Count; i++)
                {
                    if (i > 0) w.Write("<br>");
                    w.Write(col[i]);
                }
            }

            w.Write("</td>");
            w.Write("<td valign='top' noWrap width='100%' align='left' style='PADDING-LEFT:30px'>");

            if (Dir.PhoneLinkField.ValueString.Length > 0 && ((bitMask & 1) == 1 || (bitMask & 2) == 2))
            {
                bitMask = Dir.PhoneLinkField.ValueInt;

                if ((bitMask & 8) == 8) w.Write(LPlExitOutCountry);
                else if ((bitMask & 4) == 4) w.Write(LPlExitOutTown);
                else if ((bitMask & 2) == 2) w.Write(LPlExitTown);
                //else if ((bitMask & 2) == 2) w.Write(lPLExitOutTown);
                else if ((bitMask & 1) == 1) w.Write(LPlExitInSide);
            }

            w.Write("</td>");
            w.Write("</tr>");
            w.Write("</table>");
        }

        /// <summary>
        ///     Вывод информации о телефонном оборудовании - что выполнено
        /// </summary>
        /// <param name="w">Поток вывода</param>
        /// <param name="dv">Отфильтрованные данные телефонному оборудованию</param>
        private void RenderInsidePhoneComplete(TextWriter w, DataView dv)
        {
            var sb = new StringBuilder();
            var fl = false;

            var bitmask = 0;
            if (Dir.PhoneEquipField.ValueString.Length > 0)
            {
                bitmask = Dir.PhoneEquipField.ValueInt;
                if ((bitmask & 16) == 16) bitmask ^= 16;
                if ((bitmask & 32) == 32) bitmask ^= 32;
            }

            var flC = Dir.PhoneEquipField.ValueString.Length == 0 ||
                      Dir.PhoneEquipField.ValueString.Length != 0 && bitmask == 0;

            if (dv.Count > 0)
            {
                fl = true;

                for (var i = 0; i < dv.Count; i++)
                {
                    if (i == 0)
                        sb.AppendFormat("<div>&nbsp;</div>");

                    sb.AppendFormat("<div class=\"marginL {0}\">", flC ? "NoCI" : "");

                    var phoneNumber = dv[i]["НомерТелефона"].Equals(DBNull.Value)
                        ? ""
                        : dv[i]["НомерТелефона"].ToString();

                    if (phoneNumber.Length > 0)
                        sb.AppendFormat("{0} ", dv[i]["НомерТелефона"]);

                    var empls = RenderData.EquipmentEmployee(this, null, Dir, dv[i]["КодОборудования"].ToString(),
                        false);
                    using (var wr = new StringWriter())
                    {
                        RenderLinkEquipment(wr, dv[i]["КодОборудования"].ToString(), flC ? "class=\"NoCI\"" : "",
                            "title=\"" + Resx.GetString("DIRECTIONS_Msg_OpenEquip") + "\"");
                        wr.Write(dv[i]["Оборудование"]);
                        RenderLinkEnd(wr);
                        wr.Write(empls);

                        sb.AppendFormat("{0}", wr);
                    }

                    sb.Append("</div>");
                }
            }

            if (fl)
            {
                w.Write(sb.ToString());
            }
            else
            {
                GetCompleteInfo(w, Resx.GetString("DIRECTIONS_lNoComplete"), flC, true);
                w.Write("<br>");
            }
        }

        /// <summary>
        ///     Получение информации о наличии телефонных номеров
        /// </summary>
        /// <returns>Список оборудования с телефонныит номерами</returns>
        private DataView GetInsidePhoneComplete()
        {
            return new DataView
            {
                Table = DtEquip,
                RowFilter = DtEquip.Columns.Contains("ЕстьТелефонныйНомер")
                    ? "(НомерТелефона IS NOT NULL AND ЕстьХарактеристикиSIM=0) OR ЕстьТелефонныйНомер > 0"
                    : "(НомерТелефона IS NOT NULL AND ЕстьХарактеристикиSIM=0)",
                Sort = "НомерТелефона, Оборудование"
            };
        }


        /// <summary>
        ///     Вывод информации о мобильном номере сотрудника
        /// </summary>
        /// <param name="w">Поток вывода</param>
        protected void RenderMobilPhone(TextWriter w)
        {
            var bitMask = Dir.PhoneEquipField.ValueString.Length == 0 ? 0 : Dir.PhoneEquipField.ValueInt;
            var fl = false;
            string phoneNum;
            var empl = Dir.Sotrudnik;
            var dv = new DataView();
            dv.Table = DtEquip;

            dv.RowFilter = "ЕстьХарактеристикиSIM=1";
            dv.Sort = "КодТипаОборудования, Оборудование";

            if (Dir.RedirectNumField.ValueString.Length == 0 && dv.Count == 0 && !empl.SimRequired)
            {
                w.Write("");
                return;
            }

            w.Write("<div class=\"marginT\">{0}:</div>", Resx.GetString("DIRECTIONS_Field_MobilPhone"));
            w.Write("<div class=\"marginL2 marginT\">");
            if (Dir.RedirectNumField.ValueString.Length > 0)
            {
                fl = true;
                phoneNum = Dir.RedirectNumField.ValueString;
                Dir.FormatingMobilNumber(ref phoneNum);

                w.Write(
                    "<a href='#' title='" + Resx.GetString("DIRECTIONS_Msg_CopyBuffer") +
                    "' onclick=\"copyToClipboard('{0}')\">", phoneNum);
                w.Write(Dir.RedirectNumField.ValueString);
                w.Write("</a>");
            }


            for (var i = 0; i < dv.Count; i++)
            {
                fl = true;
                if (i == 0 && Dir.RedirectNumField.ValueString.Length > 0 || i > 0) w.Write(", ");

                if (dv[i]["НомерТелефона"].Equals(DBNull.Value))
                {
                    w.Write("<span ><img src='/styles/sim.gif' border=0>");
                    w.Write(Resx.GetString("DIRECTIONS_Msg_ВыданаSim"));
                    w.Write("</span>");
                    continue;
                }

                phoneNum = dv[i]["НомерТелефона"].ToString();
                if (phoneNum.Length > 6) Dir.FormatingMobilNumber(ref phoneNum);


                w.Write(
                    "<a href='#' title='" + Resx.GetString("DIRECTIONS_Msg_CopyBuffer") +
                    "' onclick=\"copyToClipboard('{0}')\">", phoneNum);
                w.Write(phoneNum);
                w.Write("</a>");
            }


            if (!fl)
                if ((bitMask & 16) != 16 && !empl.SimRequired)
                    RenderNtf(w,
                        new List<Notification>
                        {
                            new Notification
                            {
                                Message = Resx.GetString("DIRECTIONS_Msg_NoSpecified"),
                                Status = NtfStatus.Error,
                                SizeIsNtf = false,
                                DashSpace = false,
                                Description = Resx.GetString("DIRECTIONS_Msg_NoSpecified_Title")
                            }
                        });

            if (dv.Count == 0)
                if (empl.SimRequired)
                {
                    var simNumber = empl.SimNumber;
                    w.Write(
                        $"{(Dir.RedirectNumField.ValueString.Length > 0 ? ";" : "")} {Resx.GetString("DIRECTIONS_Msg_SimGive")}{(empl.SimNumber.Length > 0 ? " [" + simNumber + "]" : "")}");

                    if (Dir.SotrudnikPostField.ValueString != empl.SimPost)
                        RenderNtfInline(w,
                            new List<Notification>
                            {
                                new Notification
                                {
                                    Message = $" [{empl.SimPost}]",
                                    Status = NtfStatus.Error,
                                    SizeIsNtf = false,
                                    DashSpace = false,
                                    Description = Resx.GetString("DIRECTIONS_NTF_PostNoEqualsCadr")
                                }
                            }, "", false);
                }

            w.Write("</div>");
        }

        #endregion

        //========================================================================================================== Компьютерное оборудование

        #region Компьютерное оборудование

        /// <summary>
        ///     Вывод информации о компьютерном оборудовании
        /// </summary>
        /// <param name="w">Поток вывода</param>
        private void RenderCompEquip(TextWriter w)
        {
            var dv = GetCompEquipComplete();
            w.Write("<tr>");
            w.Write("<td colspan=2 class='TDBB' valign='top'>");
            RenderCompEquipRequired(w);
            w.Write("</td>");
            w.Write("<td colspan=2 class='TDBB TDBL' valign='top' style=\"padding-left:2px;\">");
            RenderCompEquipComplete(w, dv);
            w.Write("</td>");
            w.Write("</tr>");
        }

        /// <summary>
        ///     Вывод информации о компьютерном оборудовании - что требуется
        /// </summary>
        /// <param name="w">Поток вывода</param>
        private void RenderCompEquipRequired(TextWriter w)
        {
            w.Write("<table cellpadding=0 cellspacing=0 width='100%'>");
            w.Write("<tr>");
            w.Write("<td width='100%'>");
            w.Write(Resx.GetString("DIRECTIONS_Field_Computer") + ":");
            w.Write("</td>");
            w.Write("</tr>");
            w.Write("<tr>");
            w.Write("<td colspan=2 class='TDDataPL'>");
            if (Dir.CompTypeField.ValueString.Length == 0)
            {
                w.Write(Resx.GetString("DIRECTIONS_Msg_NoRequired"));
            }
            else
            {
                var bitMask = Dir.CompTypeField.ValueInt;

                var col = new StringCollection();

                if ((bitMask & 1) == 1)
                    col.Add(LCompT);

                if ((bitMask & 2) == 2)
                    col.Add(LCompP);

                if ((bitMask & 4) == 4)
                    col.Add(LCompN);


                for (var i = 0; i < col.Count; i++)
                {
                    if (i > 0) w.Write("<br>");
                    w.Write(col[i]);
                }

                if ((bitMask & 2) == 2 || (bitMask & 1) == 1)
                    if (Dir.AccessEthernetField.ValueString.Length == 0)
                        GetNtfFormatMsg(w, Resx.GetString("DIRECTIONS_Msg_NoAccessEthernet"));
            }

            w.Write("</td>");
            w.Write("</tr>");
            w.Write("</table>");
        }

        /// <summary>
        ///     Вывод информации о компьютерном оборудовании - что выполнено
        /// </summary>
        /// <param name="w">Поток вывода</param>
        /// <param name="dv">Список компьютерного оборудования</param>
        private void RenderCompEquipComplete(TextWriter w, DataView dv)
        {
            var flC = Dir.CompTypeField.ValueString.Length > 0;

            if (dv.Count == 0)
            {
                GetCompleteInfo(w, Resx.GetString("DIRECTIONS_lNoComplete"), !flC, true);
                return;
            }

            for (var i = 0; i < dv.Count; i++)
            {
                if (i == 0)
                    w.Write("<div>&nbsp;</div>");
                var className = !flC ? "class='NoCI'" : "";

                w.Write("<div class=\"marginL {0}\">", className);
                var empls = RenderData.EquipmentEmployee(this, null, Dir, dv[i]["КодОборудования"].ToString(), false);
                RenderLinkEquipment(w, dv[i]["КодОборудования"].ToString(), className,
                    "title=\"" + Resx.GetString("DIRECTIONS_Msg_OpenEquip") + "\"");
                w.Write(dv[i]["Оборудование"]);
                RenderLinkEnd(w);
                w.Write(empls);
                w.Write("</div>");
            }
        }

        /// <summary>
        ///     Получение информации о компьютерного оборудования
        /// </summary>
        /// <returns>Список компьютерного оборудования</returns>
        private DataView GetCompEquipComplete()
        {
            return new DataView
            {
                Table = DtEquip,
                RowFilter = "ЕстьХарактеристикиКомпьютера=1 OR ЕстьХарактеристикиМонитора=1",
                Sort = "КодТипаОборудования, Оборудование"
            };
        }

        #endregion

        //========================================================================================================== Дополнительное оборудование

        #region Дополнительное оборудование

        /// <summary>
        ///     Вывод информации о доп. оборудовании на рабочем месте
        /// </summary>
        /// <param name="w">Поток вывода</param>
        private void RenderAdvEquip(TextWriter w)
        {
            var dv = GetAdvEquipComplete();

            w.Write("<tr>");
            w.Write("<td colspan=2 class='TDBB' valign='top'>");
            RenderAdvEquipRequired(w);
            w.Write("</td>");
            w.Write("<td colspan=2 class='TDBB TDBL' valign='top' style=\"padding-left:2px;\">");
            RenderAdvEquipComplete(w, dv);
            w.Write("</td>");
            w.Write("</tr>");
        }

        /// <summary>
        ///     Вывод информации о доп. оборудовании на рабочем месте - что требуется
        /// </summary>
        /// <param name="w">Поток вывода</param>
        private void RenderAdvEquipRequired(TextWriter w)
        {
            w.Write("<table cellpadding=0 cellspacing=0 width='100%'>");
            w.Write("<tr>");
            w.Write("<td width='100%'>");
            w.Write(Resx.GetString("DIRECTIONS_Field_AdvEq") + ":");
            w.Write("</td>");
            w.Write("</tr>");
            w.Write("<tr>");
            w.Write("<td colspan=2 class='TDDataPL'>");
            if (Dir.AdvEquipField.ValueString.Length == 0)
                w.Write(Resx.GetString("DIRECTIONS_Msg_NoRequired"));
            else
                w.Write(Dir.AdvEquipField.ValueString);
            w.Write("</td>");
            w.Write("</tr>");
            w.Write("</table>");
        }

        /// <summary>
        ///     Вывод информации о доп. оборудовании на рабочем месте - что выполнено
        /// </summary>
        /// <param name="w">Поток вывода</param>
        /// <param name="dv">Список дополнительного оборудования</param>
        private void RenderAdvEquipComplete(TextWriter w, DataView dv)
        {
            var flC = Dir.AdvEquipField.ValueString.Length > 0;

            if (dv.Count == 0)
            {
                GetCompleteInfo(w, Resx.GetString("DIRECTIONS_lNoComplete"), !flC, true);
                return;
            }

            var className = !flC ? "class='NoCI'" : "";

            for (var i = 0; i < dv.Count; i++)
            {
                if (i == 0)
                    w.Write("<div>&nbsp;</div>");

                w.Write("<div class=\"marginL {0}\">", !flC ? "NoCI" : "");
                var empls = RenderData.EquipmentEmployee(this, null, Dir, dv[i]["КодОборудования"].ToString(), false);
                RenderLinkEquipment(w, dv[i]["КодОборудования"].ToString(), className,
                    "title=\"" + HttpUtility.HtmlEncode(Resx.GetString("DIRECTIONS_Msg_OpenEquip")) + "\"");
                w.Write(HttpUtility.HtmlEncode(dv[i]["Оборудование"]));
                RenderLinkEnd(w);
                w.Write(empls);
                w.Write("</div>");
            }
        }

        /// <summary>
        ///     Получение информации о дополонительном оборудовании
        /// </summary>
        /// <returns>Список дополнительного оборудования</returns>
        private DataView GetAdvEquipComplete()
        {
            return new DataView
            {
                Table = DtEquip,
                RowFilter = DtEquip.Columns.Contains("ЕстьТелефонныйНомер")
                    ? "ЕстьХарактеристикиКомпьютера=0 AND ЕстьХарактеристикиМонитора=0 AND ЕстьХарактеристикиSIM=0 AND ЕстьТелефонныйНомер=0 AND НомерТелефона IS NULL"
                    : "ЕстьХарактеристикиКомпьютера=0 AND ЕстьХарактеристикиМонитора=0 AND ЕстьХарактеристикиSIM=0 AND НомерТелефона IS NULL",
                Sort = "КодТипаОборудования, Оборудование"
            };
        }

        #endregion

        #endregion

        //========================================================================================================== Учетная запись

        #region Учетная запись

        //========================================================================================================== Доступ к корпоративной сети

        #region Доступ к корпоративной сети

        /// <summary>
        ///     Вывод информации об учетной записи
        /// </summary>
        /// <param name="w">Поток вывода</param>
        private void RenderAeAccess(TextWriter w)
        {
            w.Write("<tr>");
            w.Write("<td colspan=2 class='TDBB' valign='top'>");
            RenderAeAccessRequired(w);
            w.Write("</td>");
            w.Write("<td colspan=2 class='TDBB TDBL' valign='top' style=\"padding-left:2px;\">");
            RenderAeAccessComplete(w);
            w.Write("</td>");
            w.Write("</tr>");
        }

        /// <summary>
        ///     Вывод информации об учетной записи - что требуется
        /// </summary>
        /// <param name="w">Поток вывода</param>
        private void RenderAeAccessRequired(TextWriter w)
        {
            w.Write("<table cellpadding=0 cellspacing=0 width='100%'>");
            w.Write("<tr>");
            w.Write("<td width='100%'>");
            w.Write(Resx.GetString("DIRECTIONS_Field_AEAccess") + ":");
            w.Write("</td>");
            w.Write("</tr>");
            w.Write("<tr>");
            w.Write("<td colspan=2 class='TDDataPL'>");

            var bitMask = Dir.AccessEthernetField.ValueString.Length == 0 ? 0 : Dir.AccessEthernetField.ValueInt;

            var col = new StringCollection();

            if ((bitMask & 1) == 1 || (bitMask & 2) == 2)
                col.Add(Dir.LoginField.ValueString.Length > 0 ? Dir.LoginField.ValueString : LAeOffice);

            if ((bitMask & 2) == 2)
                col.Add(LAeVpn);


            for (var i = 0; i < col.Count; i++)
            {
                if (i > 0) w.Write("<br/>");
                w.Write(col[i]);
            }

            if (Dir.LoginField.ValueString.Length > 0)
                RenderData.LoginCheck(this, w, Dir, new List<Notification>(), true);

            w.Write("</td>");
            w.Write("</tr>");
            w.Write("</table>");
        }

        /// <summary>
        ///     Вывод информации об учетной записи - что выполнено
        /// </summary>
        /// <param name="w">Поток вывода</param>
        private void RenderAeAccessComplete(TextWriter w)
        {
            var fl = false;

            if (Dir.SotrudnikField.ValueString.Length > 0 && !Dir.Sotrudnik.Unavailable)
            {
                if (Dir.Sotrudnik.Login != null)
                    fl = Dir.LoginField.ValueString.ToLower().Equals(Dir.Sotrudnik.Login.ToLower());

                if (!string.IsNullOrEmpty(Dir.Sotrudnik.Login))
                {
                    var f = Dir.Sotrudnik.PersonalFolder;
                    var href = Regex.Replace(f, "\\\\[^\\\\]+$", "").Replace(":", "|").Replace("\\", "/");
                    var ntfs = new List<Notification>();

                    w.Write("<div class=\"marginL\">");
                    GetCompleteInfo(w,
                        "<nobr>" + Dir.Sotrudnik.Login + " " + "<a href='file:///" + href + "' target='_blabk'>" + f +
                        "</a>" + "</nobr>", fl);

                    var adsiPath = RenderData.ADSI_RenderInfoByLogin(this, w, ntfs, Dir.LoginField.ValueString, Dir.Sotrudnik, 1);
                    RenderNtfInline(w, ntfs, ";", true);

                    w.Write("</div>");

                    if (adsiPath.Length > 0) w.Write("<div class=\"marginL\">{0}</div>", adsiPath);
                }
                else
                {
                    GetCompleteInfo(w, Resx.GetString("DIRECTIONS_lNoComplete"), fl, true);
                }
            }
        }

        #endregion


        //========================================================================================================== Предпочитаемый язык

        #region Предпочитаемый язык

        /// <summary>
        ///     Вывод информации о предпочитаемом языке
        /// </summary>
        /// <param name="w">Поток вывода</param>
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

        /// <summary>
        ///     Вывод информации о предпочитаемом языке - что требуется
        /// </summary>
        /// <param name="w">Поток вывода</param>
        private void RenderLanguageRequired(TextWriter w)
        {
            w.Write("<table cellpadding=0 cellspacing=0 width='100%'>");
            w.Write("<tr>");
            w.Write("<td width='100%'>");
            w.Write(Resx.GetString("DIRECTIONS_Field_Lang") + ":");
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

        /// <summary>
        ///     Вывод информации о предпочитаемом языке - что выполнено
        /// </summary>
        /// <param name="w">Поток вывода</param>
        private void RenderLanguageComplete(TextWriter w)
        {
            if (Dir.SotrudnikField.ValueString.Length == 0 || Dir.Sotrudnik.Unavailable)
            {
                w.Write(Resx.GetString("DIRECTIONS_Msg_NoData"));
                return;
            }

            var l = Dir.SotrudnikLanguageField.ValueString.Length == 0
                ? "ru"
                : Dir.SotrudnikLanguageField.ValueString.ToLower();
            var fl = Dir.Sotrudnik.Language.ToLower().Equals(l);

            w.Write("<div class=\"marginL\">");
            GetCompleteInfo(w, Dir.Sotrudnik.Language, fl);
            w.Write("</div>");
        }

        #endregion

        //========================================================================================================== Предпочитаемый язык

        #region Email

        /// <summary>
        ///     Вывод информации о почтовом ящике
        /// </summary>
        /// <param name="w">Поток вывода</param>
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

        /// <summary>
        ///     Вывод информации о почтовом ящике - что требуется
        /// </summary>
        /// <param name="w">Поток вывода</param>
        private void RenderEmailRequired(TextWriter w)
        {
            w.Write("<table cellpadding=0 cellspacing=0 width='100%'>");
            w.Write("<tr>");
            w.Write("<td width='100%'>");
            w.Write("E-Mail:");
            w.Write("</td>");
            w.Write("</tr>");
            w.Write("<tr>");
            w.Write("<td colspan=2 class='TDDataPL'>");
            if (Dir.MailNameField.ValueString.Length == 0 || Dir.DomainField.ValueString.Length == 0)
            {
                w.Write(Resx.GetString("DIRECTIONS_Msg_NoRequired"));
            }
            else
            {
                var email = Dir.MailNameField.ValueString + "@" + Dir.DomainField.ValueString;
                w.Write(email);

                RenderData.EmailCheck(this, w, Dir);
            }


            w.Write("</td>");
            w.Write("</tr>");
            w.Write("</table>");
        }

        /// <summary>
        ///     Вывод информации о почтовом ящике - что выполнено
        /// </summary>
        /// <param name="w">Поток вывода</param>
        private void RenderEmailComplete(TextWriter w)
        {
            if (Dir.SotrudnikField.ValueString.Length == 0 || Dir.Sotrudnik.Unavailable)
            {
                w.Write(Resx.GetString("DIRECTIONS_Msg_NoData"));
                return;
            }


            string email;
            email = Dir.Sotrudnik.Email.Length > 0 ? Dir.Sotrudnik.Email : Resx.GetString("DIRECTIONS_lNoComplete");

            var emaildd = Dir.MailNameField.ValueString.Length == 0 || Dir.DomainField.ValueString.Length == 0
                ? Resx.GetString("DIRECTIONS_lNoComplete")
                : Dir.MailNameField.ValueString + "@" + Dir.DomainField.ValueString;

            var fl = email.ToLower().Equals(emaildd.ToLower());
            w.Write("<div class=\"marginL\">");
            GetCompleteInfo(w, email, fl);
            w.Write("</div>");
        }

        #endregion

        #endregion

        //========================================================================================================== Доступ к данным

        #region Доступ к данным

        #region Как/Вместо

        /// <summary>
        ///     Вывод информации о сотруднике "как/вместо"
        /// </summary>
        /// <param name="w">Поток вывода</param>
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

        /// <summary>
        ///     Вывод информации о сотруднике "как/вместо" - что требуется
        /// </summary>
        /// <param name="w">Поток вывода</param>
        private void RenderSotrudnikParentRequired(TextWriter w)
        {
            w.Write("<table cellpadding=0 cellspacing=0 width='100%'>");
            w.Write("<tr>");
            w.Write("<td width='100%'>");
            w.Write(HAccessEml);
            w.Write("</td>");
            w.Write("</tr>");
            w.Write("<tr>");
            w.Write("<td colspan=2 class='TDDataPL'>");

            var bitMask = Dir.SotrudnikParentCheckField.ValueInt;
            if ((bitMask & 1) == 1)
                w.Write(LSotrudnikParent1);

            if ((bitMask & 2) == 2)
                w.Write(LSotrudnikParent2);

            w.Write(": ");

            RenderLinkEmployee(w, "parent" + Dir.SotrudnikParentField.ValueString, Dir.SotrudnikParent,
                NtfStatus.Empty);

            var p = Dir.SotrudnikParent;
            if (p == null || p.Unavailable)
            {
                GetNtfFormatMsg(w, Resx.GetString("DIRECTIONS_Msg_СотрудникНеДоступен"));
                return;
            }

            if (Dir.SotrudnikField.ValueString.Length > 0 &&
                Dir.SotrudnikParentField.ValueString.Equals(Dir.SotrudnikField.ValueString))
                GetNtfFormatMsg(w, Resx.GetString("DIRECTIONS_NTF_СотрудникСовпадает")?.ToLower());

            if ((bitMask & 1) == 1 && p.Login.Length == 0)
                GetNtfFormatMsg(w, Resx.GetString("DIRECTIONS_Msg_СотрудникНеИмеетЛогина"));

            if ((bitMask & 2) == 2 && p.Login.Length == 0
                                   && Dir.SotrudnikField.ValueString.Length > 0 && !Dir.Sotrudnik.Unavailable &&
                                   Dir.Sotrudnik.Login.Length == 0)
                GetNtfFormatMsg(w, Resx.GetString("DIRECTIONS_Msg_СотрудникНеИмеетЛогина"));

            using (var sw = new StringWriter())
            {
                var ntfs = new List<Notification>();
                ValidationMessages.CheckSotrudnikParentStatus(this, sw, Dir, ntfs);

                var adsiPath = RenderData.ADSI_RenderInfoByEmployee(this, w, ntfs, p, Dir.SotrudnikParentCheckField.ValueInt);
                RenderNtfInline(w, ntfs, ";", true);

                if (adsiPath.Length > 0) w.Write("<div>{0}</div>", adsiPath);
            }

            w.Write("</td>");
            w.Write("</tr>");
            w.Write("</table>");
        }

        #endregion

        //========================================================================================================== Доступ к общим папкам

        #region Доступ к общим папкам

        /// <summary>
        ///     Вывод информации об общих папках
        /// </summary>
        /// <param name="w">Поток вывода</param>
        private void RenderSFolder(TextWriter w)
        {
            w.Write("<tr>");
            w.Write("<td colspan=2 class='TDBB' valign='top'>");
            RenderSFolderRequired(w);
            w.Write("</td>");
            w.Write("<td colspan=2 class='TDBB TDBL' valign='top' style=\"padding-left:2px;\">");
            RenderSFolderComplete(w);
            w.Write("</td>");
            w.Write("</tr>");
        }

        /// <summary>
        ///     Вывод информации об общих папках - что требуется
        /// </summary>
        /// <param name="w">Поток вывода</param>
        private void RenderSFolderRequired(TextWriter w)
        {
            w.Write("<table cellpadding=0 cellspacing=0 width='100%'>");
            w.Write("<tr>");
            w.Write("<td width='100%'>");
            w.Write(Resx.GetString("DIRECTIONS_Field_Positions_Folders") + ":");
            w.Write("</td>");
            w.Write("</tr>");
            w.Write("<tr>");
            w.Write("<td colspan=2 class='TDDataPL'>");
            if (!Dir.PositionCommonFolders.Any())
            {
                w.Write(Resx.GetString("DIRECTIONS_Msg_NoRequired"));
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


        /// <summary>
        ///     Вывод информации об общих папках - что выполнено
        /// </summary>
        /// <param name="w">Поток вывода</param>
        private void RenderSFolderComplete(TextWriter w)
        {
            var commonFolders = Dir.Sotrudnik.CommonFolders;
            if (commonFolders == null)
            {
                if (!Dir.PositionCommonFolders.Any())
                    w.Write(Resx.GetString("DIRECTIONS_Msg_NoData"));
                else
                    GetCompleteInfo(w, Resx.GetString("DIRECTIONS_lNoComplete"), false, true);
                return;
            }

            var sortedList = commonFolders.OrderBy(x => x.Name).ToList();

            if (sortedList.Count == 0 && !Dir.PositionCommonFolders.Any())
            {
                w.Write(Resx.GetString("DIRECTIONS_Msg_NoData"));
                return;
            }

            if (sortedList.Count == 0 && Dir.PositionCommonFolders.Any())
            {
                GetCompleteInfo(w, Resx.GetString("DIRECTIONS_lNoComplete"), false, true);
                return;
            }

            w.Write("<br/>");
            sortedList.ForEach(delegate(CommonFolder p)
            {
                var cf = Dir.PositionCommonFolders.FirstOrDefault(x => x.CommonFolderId.ToString() == p.Id);
                var fl = cf != null;
                w.Write("<div class=\"marginL\">");
                GetCompleteInfo(w, HttpUtility.HtmlEncode(p.Name), fl, false, false);
                w.Write("</div>");
            });

            RenderSFolderCompleteError(sortedList, w);
        }

        /// <summary>
        ///     Вывод информации о различиях требуемого и выполненого
        /// </summary>
        /// <param name="folders">Список папок</param>
        /// <param name="w">Поток вывода</param>
        private void RenderSFolderCompleteError(IEnumerable<CommonFolder> folders, TextWriter w)
        {
            var list = new List<string>();
            Dir.PositionCommonFolders.ForEach(delegate(PositionCommonFolder f)
            {
                var cf = folders.FirstOrDefault(x => x.Id == f.CommonFolderId.ToString());
                if (cf == null)
                    list.Add(f.CommonFolderName);
            });

            if (list.Count == 0) return;

            w.Write("<div class=\"marginL\">");
            GetCompleteInfo(w, HttpUtility.HtmlEncode(Resx.GetString("DIRECTIONS_lNoComplete") + ":"), false, false,
                false);
            w.Write("</div>");
            list.OrderBy(x => x).ToList().ForEach(delegate(string x)
                {
                    w.Write("<div class=\"marginL2\">");
                    GetCompleteInfo(w, HttpUtility.HtmlEncode(x), false, false, false);
                    w.Write("</div>");
                }
            );
        }

        #endregion

        //========================================================================================================== Выполняемые роли

        #region Выполняемые роли

        /// <summary>
        ///     Вывод информации об ролях сотрудника
        /// </summary>
        /// <param name="w">Поток вывода</param>
        private void RenderRoles(TextWriter w)
        {
            w.Write("<tr>");
            w.Write("<td colspan=2 class='TDBB' valign='top'>");
            RenderRolesRequired(w);
            w.Write("</td>");
            w.Write("<td colspan=2 class='TDBB TDBL TDDataPL0' valign='top' style=\"padding-left:2px;\">");
            RenderRoleComplete(this, w);
            w.Write("</td>");
            w.Write("</tr>");
        }

        /// <summary>
        ///     Вывод информации об ролях сотрудника - что требуется
        /// </summary>
        /// <param name="w">Поток вывода</param>
        private void RenderRolesRequired(TextWriter w)
        {
            w.Write("<table cellpadding=0 cellspacing=0 width='100%'>");
            w.Write("<tr>");
            w.Write("<td width='100%'>");
            w.Write(Resx.GetString("DIRECTIONS_Field_Positions_Roles") + ":");
            w.Write("</td>");
            w.Write("</tr>");
            w.Write("<tr>");
            w.Write("<td colspan=2 class='TDDataPL'>");

            if (!Dir.PositionRoles.Any())
            {
                w.Write(Resx.GetString("DIRECTIONS_Msg_NoRequired"));
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

                DataRow dr;

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


                    dr[4] = p.PersonId == 0 ? "" : p.PersonId.ToString();
                    dr[5] = p.PersonName;

                    dt.Rows.Add(dr);
                });


                var dv = new DataView();
                dv.Table = dt;
                dv.Sort = "Role, RoleId, Person";

                int rowSpan;
                w.Write("<table cellpadding=0 cellspacing=0 width='100%'>");
                for (var i = 0; i < dv.Count; i++)
                {
                    w.Write("<tr>");
                    if (i == 0 || !dv[i]["RoleId"].Equals(dv[i - 1]["RoleId"]))
                    {
                        rowSpan = int.Parse(dt.Compute("COUNT(RoleId)", "RoleId='" + dv[i]["RoleId"] + "'").ToString());

                        w.Write("<td title='{0}' valign='top' rowSpan={1}>", dv[i]["RoleDescr"], rowSpan);
                        if (!dv[i]["RoleId"].Equals(""))
                            w.Write(HttpUtility.HtmlEncode(dv[i]["Role"]));
                        else
                            w.Write("&nbsp;");
                        w.Write("</td>");
                    }

                    w.Write("<td>");
                    w.Write("<table width='100%' height='100%' cellpadding=0 cellspacing=0 border=0>");
                    w.Write("<tr>");
                    w.Write("<td noWrap align='left'>");
                    if (dv[i]["PersonId"].ToString().Length > 0)
                        RenderLinkPerson(w, "person" + dv[i]["PersonId"], dv[i]["PersonId"].ToString(),
                            dv[i]["Person"].ToString());
                    else
                        w.Write(HttpUtility.HtmlEncode(Resx.GetString("DIRECTIONS_Rnd_AllCompany")));
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

        /// <summary>
        ///     Вывод информации об ролях сотрудника - что выполнено
        /// </summary>
        /// <param name="page">Страница вывода</param>
        /// <param name="w">Поток вывода</param>
        private void RenderRoleComplete(DocPage page, TextWriter w)
        {
            var listRoles = Dir.Sotrudnik.Roles;

            if (listRoles.Count == 0 && !Dir.PositionRoles.Any())
            {
                w.Write(Resx.GetString("DIRECTIONS_Msg_NoData"));
                return;
            }

            if (listRoles.Count == 0 && Dir.PositionRoles.Any())
            {
                GetCompleteInfo(w, Resx.GetString("DIRECTIONS_lNoComplete"), false, true);
                return;
            }

            var dtR = new DataTable("Roles");
            dtR.Columns.Add("RoleId");
            dtR.Columns.Add("Role");
            dtR.Columns.Add("RoleDescr");
            dtR.Columns.Add("PersonId");
            dtR.Columns.Add("Person");

            DataRow dr;

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
                {
                    dr[1] = dr[2] = "";
                }

                dr[3] = r.PersonId == 0 ? "" : r.PersonId.ToString();

                if (dr[3].ToString().Length > 0)
                {
                    var personObject = GetObjectById(typeof(PersonOld), dr[3].ToString()) as PersonOld;
                    if (personObject == null || personObject.Unavailable)
                        dr[4] = "#" + r.PersonId;
                    else
                        dr[4] = personObject.Name;
                }
                else
                {
                    dr[3] = dr[4] = "";
                }

                dtR.Rows.Add(dr);
            });


            var dv = new DataView();
            dv.Table = dtR;
            dv.Sort = "Role, RoleId, Person";

            w.Write("<br><table cellpadding=0 cellspacing=0 width='100%' class=\"marginL\">");
            for (var i = 0; i < dv.Count; i++)
            {
                var fl = CheckRoleComleteByRequired(dv[i]);
                w.Write("<tr>");
                if (i == 0 || !dv[i]["RoleId"].Equals(dv[i - 1]["RoleId"]))
                {
                    var rowSpan =
                        int.Parse(dtR.Compute("COUNT(RoleId)", "RoleId='" + dv[i]["RoleId"] + "'").ToString());

                    w.Write("<td title='{0}' valign='top' rowSpan={1} style=\"padding-left:2px;\">", dv[i]["RoleDescr"],
                        rowSpan);
                    if (!dv[i]["RoleId"].Equals(""))
                        GetCompleteInfo(w, HttpUtility.HtmlEncode(dv[i]["Role"].ToString()), fl, false, false);
                    else
                        w.Write("&nbsp;");
                    w.Write("</td>");
                }

                w.Write("<td>");
                w.Write("<table width='100%' height='100%' cellpadding=0 cellspacing=0 border=0>");
                w.Write("<tr>");
                w.Write("<td noWrap align='left'>");
                if (dv[i]["PersonId"].ToString().Length > 0)
                    page.RenderLinkPerson(w, "personC" + dv[i]["PersonId"], dv[i]["PersonId"].ToString(),
                        dv[i]["Person"].ToString(), !fl ? NtfStatus.Error : NtfStatus.Empty, false);
                else
                    GetCompleteInfo(w, HttpUtility.HtmlEncode(Resx.GetString("DIRECTIONS_Rnd_AllCompany")), fl, false,
                        false);
                w.Write("</td>");
                w.Write("</tr>");
                w.Write("</table>");
                w.Write("</td>");
                w.Write("</tr>");
            }

            RenderRoleCompleteError(page, dv, w);
            w.Write("</table>");
        }

        /// <summary>
        ///     Вывод информации о различиях ролей требуемых и выполненных
        /// </summary>
        /// <param name="page">Страница вывода</param>
        /// <param name="dv">Список ролей</param>
        /// <param name="wr">Поток вывода</param>
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
                GetCompleteInfo(w, HttpUtility.HtmlEncode(p.RoleObject.Name), false, false, false);
                w.Write("</td>");
                w.Write("<td>");

                if (p.PersonId == 0)
                    GetCompleteInfo(w, HttpUtility.HtmlEncode(Resx.GetString("DIRECTIONS_Rnd_AllCompany")), false,
                        false,
                        false);
                else
                    page.RenderLinkPerson(w, "perr" + p.PersonId, p.PersonId.ToString(), p.PersonName, NtfStatus.Error);

                w.Write("</td>");
                w.Write("</tr>");
            });


            if (fl)
            {
                wr.Write("<tr>");
                wr.Write("<td colspan = 2 style=\"padding-left:2px;\">");
                GetCompleteInfo(wr, HttpUtility.HtmlEncode(Resx.GetString("DIRECTIONS_lNoComplete") + ":"), false,
                    false,
                    false);
                wr.Write("</td>");
                wr.Write("</tr>");

                wr.Write(w);
            }
        }

        /// <summary>
        ///     Сопоставление требуемого и выполненного по ролям
        /// </summary>
        /// <param name="dr">Ичточник сопоставления</param>
        /// <returns></returns>
        private bool CheckRoleComleteByRequired(DataRowView dr)
        {
            var fl = false;

            Dir.PositionRoles.ForEach(delegate(PositionRole p)
            {
                if (p.RoleId.ToString().Equals(dr["RoleId"].ToString()) &&
                    (p.PersonId == 0 ? "" : p.PersonId.ToString()).Equals(
                        dr["PersonId"].ToString().Equals("0") ? "" : dr["PersonId"].ToString()
                    )
                )
                    fl = true;
            });


            return fl;
        }

        #endregion

        //========================================================================================================== Типы лиц

        #region Типы лиц

        /// <summary>
        ///     Вывод информации о типах лиц
        /// </summary>
        /// <param name="w">Поток вывода</param>
        private void RenderTypes(TextWriter w)
        {
            w.Write("<tr>");
            w.Write("<td colspan=2 class='TDBB' valign='top'>");
            RenderTypeRequired(w);
            w.Write("</td>");
            w.Write("<td colspan=2 class='TDBB TDBL TDDataPL0' valign='top' style=\"padding-left:2px;\">");
            RenderTypeComplete(this, w);
            w.Write("</td>");
            w.Write("</tr>");
        }

        /// <summary>
        ///     Вывод информации о типах лиц - что требуется
        /// </summary>
        /// <param name="w">Поток вывода</param>
        private void RenderTypeRequired(TextWriter w)
        {
            w.Write("<table cellpadding=0 cellspacing=0 width='100%'>");
            w.Write("<tr>");
            w.Write("<td width='100%'>");
            w.Write(Resx.GetString("DIRECTIONS_Field_Positions_Types") + ":");
            w.Write("</td>");
            w.Write("</tr>");
            w.Write("<tr>");
            w.Write("<td colspan=2 class='TDDataPL'>");
            if (!Dir.PositionTypes.Any())
            {
                w.Write($"<span {(Dir.AccessEthernetField.ValueString.Length > 0 ? "class=\"NoCI\"" : "")}>");
                w.Write(Resx.GetString("DIRECTIONS_Msg_NoRequired"));
                w.Write("</span>");
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

                DataRow dr;

                Dir.PositionTypes.ForEach(delegate(PositionType p)
                {
                    dr = dt.NewRow();
                    dr[0] = p.Id;
                    dr[1] = !p.CatalogId.HasValue ? "" : p.CatalogId.Value.ToString();
                    if (dr[1].ToString().Length > 0)
                    {
                        var catalogObject = GetObjectById(typeof(PersonCatalog), dr[1].ToString()) as PersonCatalog;
                        if (catalogObject != null && !catalogObject.Unavailable) dr[2] = catalogObject.Name;
                        else dr[2] = "";
                        dr[3] = "";
                    }
                    else
                    {
                        dr[2] = dr[3] = "";
                    }


                    dr[4] = !p.ThemeId.HasValue || p.ThemeId.Value == 0 ? "" : p.ThemeId.Value.ToString();
                    if (dr[4].ToString().Length > 0)
                    {
                        var themeObject = GetObjectById(typeof(PersonTheme), dr[4].ToString()) as PersonTheme;
                        if (themeObject == null || themeObject.Unavailable)
                            dr[5] = "#" + p.ThemeId;
                        else
                            dr[5] = themeObject.NameTheme;
                    }
                    else
                    {
                        dr[4] = dr[5] = "";
                    }

                    dt.Rows.Add(dr);
                });


                var dv = new DataView();
                dv.Table = dt;
                dv.Sort = "Catalog, CatalogId, Theme";

                int rowSpan;
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
                            w.Write(dv[i]["Catalog"]);
                        else
                            w.Write(Resx.GetString("DIRECTIONS_Rnd_AllCatalog"));
                        w.Write("</td>");
                    }

                    w.Write("<td>");
                    w.Write("<table width='100%' height='100%' cellpadding=0 cellspacing=0 border=0>");
                    w.Write("<tr>");
                    w.Write("<td align='left'>");
                    if (dv[i]["ThemeId"].ToString().Length > 0)
                        w.Write(HttpUtility.HtmlEncode(dv[i]["Theme"]));
                    else
                        w.Write(HttpUtility.HtmlEncode(Resx.GetString("DIRECTIONS_Rnd_AllTypePerson")));
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

        /// <summary>
        ///     Вывод информации о типах лиц - что выполнено
        /// </summary>
        /// <param name="page">Страница вывода</param>
        /// <param name="w">Поток вывода</param>
        private void RenderTypeComplete(DocPage page, TextWriter w)
        {
            var listTypes = Dir.Sotrudnik.Types;

            if (!listTypes.Any() && !Dir.PositionTypes.Any())
            {
                w.Write(Resx.GetString("DIRECTIONS_Msg_NoData"));
                return;
            }

            if (!listTypes.Any() && Dir.PositionTypes.Any())
            {
                GetCompleteInfo(w, Resx.GetString("DIRECTIONS_lNoComplete"), false, true);
                return;
            }

            var dtR = new DataTable("Type");
            dtR.Columns.Add("CatalogId");
            dtR.Columns.Add("Catalog");
            dtR.Columns.Add("CatalogDescr");
            dtR.Columns.Add("ThemeId");
            dtR.Columns.Add("Theme");

            DataRow dr;

            listTypes.ForEach(delegate(EmployeePersonType p)
            {
                dr = dtR.NewRow();
                dr[0] = !p.CatalogId.HasValue ? "" : p.CatalogId.Value.ToString();

                if (dr[0].ToString().Length > 0)
                {
                    var catalogObject = GetObjectById(typeof(PersonCatalog), dr[0].ToString()) as PersonCatalog;
                    if (catalogObject != null && !catalogObject.Unavailable) dr[1] = catalogObject.Name;
                    else dr[1] = "";
                    dr[2] = "";
                }
                else
                {
                    dr[1] = dr[2] = "";
                }

                dr[3] = !p.ThemeId.HasValue || p.ThemeId.Value == 0 ? "" : p.ThemeId.Value.ToString();
                if (dr[3].ToString().Length > 0)
                {
                    var themeObject = GetObjectById(typeof(PersonTheme), dr[3].ToString()) as PersonTheme;
                    if (themeObject == null || themeObject.Unavailable)
                    {
                        if (p.ThemeId != null) dr[4] = "#" + p.ThemeId.Value;
                    }
                    else
                    {
                        dr[4] = themeObject.NameTheme;
                    }
                }
                else
                {
                    dr[3] = dr[4] = "";
                }

                dtR.Rows.Add(dr);
            });


            var dv = new DataView();
            dv.Table = dtR;
            dv.Sort = "Catalog, CatalogId, Theme";

            w.Write("<br><table cellpadding=0 cellspacing=0 width='100%' class=\"marginL\">");

            for (var i = 0; i < dv.Count; i++)
            {
                var fl = CheckTypeComleteByRequired(dv[i]);
                w.Write("<tr>");
                if (i == 0 || !dv[i]["CatalogId"].Equals(dv[i - 1]["CatalogId"]))
                {
                    var rowSpan = int.Parse(dtR.Compute("COUNT(CatalogId)", "CatalogId='" + dv[i]["CatalogId"] + "'")
                        .ToString());
                    w.Write("<td noWrap title='{0}' valign='top' rowSpan={1} style=\"padding-left:2px;\">", "",
                        rowSpan);
                    if (!dv[i]["CatalogId"].Equals("")
                        && !dv[i]["CatalogId"].Equals(DBNull.Value)
                        && !dv[i]["CatalogId"].Equals(0))
                    {
                        var catName = dv[i]["Catalog"].ToString();
                        GetCompleteInfo(w,
                            string.IsNullOrEmpty(catName)
                                ? HttpUtility.HtmlEncode(Resx.GetString("DIRECTIONS_Pos_CatalogNoName"))
                                : catName, fl, false,
                            false);
                    }
                    else
                    {
                        GetCompleteInfo(w, HttpUtility.HtmlEncode(Resx.GetString("DIRECTIONS_Rnd_AllCatalog")), fl,
                            false,
                            false);
                    }

                    w.Write("</td>");
                }

                w.Write("<td>");
                w.Write("<table width='100%' height='100%' cellpadding=0 cellspacing=0 border=0>");
                w.Write("<tr>");
                w.Write("<td align='left'>");
                if (!dv[i]["ThemeId"].Equals("") && !dv[i]["ThemeId"].Equals(DBNull.Value))
                {
                    var thmName = dv[i]["Theme"].ToString();
                    GetCompleteInfo(w,
                        string.IsNullOrEmpty(thmName)
                            ? HttpUtility.HtmlEncode(Resx.GetString("DIRECTIONS_Pos_TypeNoName"))
                            : thmName, fl, false, false);
                }
                else
                {
                    var msgAllTypes = HttpUtility.HtmlEncode(Resx.GetString("DIRECTIONS_Rnd_AllTypePerson"));
                    GetCompleteInfo(w,
                        string.IsNullOrEmpty(msgAllTypes)
                            ? HttpUtility.HtmlEncode(Resx.GetString("DIRECTIONS_Pos_TypeAllNoName"))
                            : msgAllTypes, fl, false, false);
                }

                w.Write("</td>");
                w.Write("</tr>");
                w.Write("</table>");
                w.Write("</td>");
                w.Write("</tr>");
            }

            RenderTypeCompleteError(dv, w);
            w.Write("</table>");
        }

        /// <summary>
        ///     Вывод информации о различиях типов требуемых и выполненных
        /// </summary>
        /// <param name="dv">Список ролей</param>
        /// <param name="wr">Поток вывода</param>
        private void RenderTypeCompleteError(DataView dv, TextWriter wr)
        {
            var listTypes = Dir.PositionTypes;
            var w = new StringWriter();

            var fl = false;
            string nameEntity;


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


                if (p.CatalogId != null)
                {
                    var catalogObject =
                        GetObjectById(typeof(PersonCatalog), p.CatalogId.Value.ToString()) as PersonCatalog;
                    if (catalogObject == null || catalogObject.Unavailable)
                    {
                        if (!p.CatalogId.HasValue)
                            nameEntity = Resx.GetString("DIRECTIONS_Rnd_AllCatalog");
                        else
                            nameEntity = "#" + p.CatalogId.Value;
                    }
                    else
                    {
                        nameEntity = catalogObject.Name;
                    }
                }

                GetCompleteInfo(w, HttpUtility.HtmlEncode(nameEntity), false, false, false);
                w.Write("</td>");
                w.Write("<td>");

                if (!p.ThemeId.HasValue || p.ThemeId.Value == 0)
                {
                    GetCompleteInfo(w, HttpUtility.HtmlEncode(Resx.GetString("DIRECTIONS_Rnd_AllTypePerson")), false,
                        false,
                        false);
                }
                else
                {
                    var themeObject = GetObjectById(typeof(PersonTheme), p.ThemeId.Value.ToString()) as PersonTheme;

                    if (themeObject == null || themeObject.Unavailable)
                        nameEntity = "#" + p.ThemeId.Value;
                    else
                        nameEntity = themeObject.NameTheme;

                    GetCompleteInfo(w, HttpUtility.HtmlEncode(nameEntity), false, false, false);
                }

                w.Write("</td>");
                w.Write("</tr>");
            });


            if (!fl) return;

            wr.Write("<tr>");
            wr.Write("<td colspan = 2 style=\"padding-left:2px;\">");
            GetCompleteInfo(wr, HttpUtility.HtmlEncode(Resx.GetString("DIRECTIONS_lNoComplete") + ":"), false, false,
                false);
            wr.Write("</td>");
            wr.Write("</tr>");

            wr.Write(w);
        }

        /// <summary>
        ///     Сопоставление требуемого и выполненного по типам
        /// </summary>
        /// <param name="dr">Ичточник сопоставления</param>
        /// <returns></returns>
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
                    fl = true;
            });

            return fl;
        }

        #endregion

        //========================================================================================================== Дополнительные права

        #region Дополнительные права

        /// <summary>
        ///     Вывод информации о дополнительных правах
        /// </summary>
        /// <param name="w">Поток вывода</param>
        /// <param name="bitMask">относится к</param>
        private void RenderAdvancedGrants(TextWriter w, int bitMask)
        {
            Dir.AdvancedGrants.FindAll(x => x.RefersTo == bitMask).ForEach(delegate(AdvancedGrant grant)
            {
                var p = Dir.PositionAdvancedGrants.FirstOrDefault(x => x.GrantId.ToString() == grant.Id);
                if (p != null)
                {
                    p.OrderOutput = grant.OrderOutput;
                    p.RefersTo = grant.RefersTo;
                }
            });

            Dir.PositionAdvancedGrants.FindAll(x => x.RefersTo == bitMask).OrderBy(x => x.OrderOutput).ToList().ForEach(
                delegate(PositionAdvancedGrant p)
                {
                    w.Write("<tr>");
                    w.Write("<td colspan=2 class='TDBB' valign='top'>");
                    w.Write(IsRusLocal ? p.GrantDescription : p.GrantDescriptionEn);
                    w.Write("</td>");
                    w.Write("<td colspan=2 class='TDBB TDBL' valign='top' style=\"padding-left:2px;\">");

                    var existGrant = Dir.Sotrudnik.AdvancedGrants.FirstOrDefault(y => y == p.GrantId);
                    if (existGrant > 0)
                        GetCompleteInfo(w, Resx.GetString("DIRECTIONS_lComplete").ToLower(), true, false, true);
                    else
                        GetCompleteInfo(w, Resx.GetString("DIRECTIONS_lNoComplete"), false, false, true);
                    w.Write("</td>");
                    w.Write("</tr>");
                });

            Dir.AdvancedGrants
                .FindAll(x =>
                    x.RefersTo == bitMask &&
                    !x.NotAlive &&
                    Dir.Sotrudnik.AdvancedGrants.Any(y => y == int.Parse(x.Id)) &&
                    Dir.PositionAdvancedGrants.FindAll(y => y.RefersTo == bitMask)
                        .All(y => y.GrantId.ToString() != x.Id)).ToList()
                .OrderBy(z => z.OrderOutput).ToList().ForEach(
                    delegate(AdvancedGrant grant)
                    {
                        w.Write("<tr>");
                        w.Write("<td colspan=2 class='TDBB' valign='top'>");
                        w.Write(IsRusLocal ? grant.Name : grant.NameEn);
                        w.Write("<div class=\"TDDataPL\">");
                        w.Write(Resx.GetString("DIRECTIONS_Msg_NoRequired"));
                        w.Write("</div>");
                        w.Write("</td>");
                        w.Write("<td colspan=2 class='TDBB TDBL' valign='top' style=\"padding-left:2px;\">");

                        GetCompleteInfo(w, Resx.GetString("DIRECTIONS_lComplete").ToLower(), false, true, true);

                        w.Write("</td>");
                        w.Write("</tr>");
                    });
        }

        /// <summary>
        ///     Вывод информации о дополнительных правах - что требуется
        /// </summary>
        /// <param name="w">Поток вывода</param>
        private void RenderAdvancedGrantsRequired(TextWriter w)
        {
            w.Write("<table cellpadding=0 cellspacing=0 width='100%'>");
            w.Write("<tr>");
            w.Write("<td width='100%'>");
            w.Write(Resx.GetString("DIRECTIONS_Field_Positions_Grants") + ":");
            w.Write("</td>");
            w.Write("</tr>");
            w.Write("<tr>");
            w.Write("<td colspan=2 class='TDDataPL'>");

            if (!Dir.PositionAdvancedGrants.Any())
                w.Write(Resx.GetString("DIRECTIONS_Msg_NoRequired"));
            else
                Dir.PositionAdvancedGrants.OrderBy(x => x.GrantDescription).ToList().ForEach(
                    delegate(PositionAdvancedGrant p)
                    {
                        w.Write("<div>{0}</div>", IsRusLocal ? p.GrantDescription : p.GrantDescriptionEn);
                    });

            w.Write("</td>");
            w.Write("</tr>");
            w.Write("</table>");
        }

        #endregion

        //========================================================================================================== Дополнительная информация 

        #region Дополнительная информация 

        /// <summary>
        ///     Вывод дополнительных требований
        /// </summary>
        /// <param name="w">Поток вывода</param>
        private void RenderNote(TextWriter w)
        {
            if (Dir.AdvInfoField.ValueString.Length != 0)
            {
                w.Write("<tr>");
                w.Write("<td colspan=4 class='TDBB'>");
                w.Write("<table cellpadding=0 cellspacing=0 width='100%'>");
                w.Write("<tr>");
                w.Write("<td width='100%'>");
                w.Write(Dir.AdvInfoField.Name + ":");
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

        #endregion



        #endregion




        //========================================================================================================== Adv Functions

        #region Дополнительные функции

        /// <summary>
        ///     Метод получения ресурсов локализации - удалить в будущем
        /// </summary>
        private void SetCultureText()
        {
            HAccessEml = Resx.GetString("DIRECTIONS_hAccessEml");
            HDadaEml = Resx.GetString("DIRECTIONS_hDadaEml");
            HEquipEml = Resx.GetString("DIRECTIONS_hEquipEml");
            LAeOffice = Resx.GetString("DIRECTIONS_lAEOfiice");
            LAeVpn = Resx.GetString("DIRECTIONS_lAEVpn");
            LAiMobile = Resx.GetString("DIRECTIONS_lAIMobile");
            LAiModem = Resx.GetString("DIRECTIONS_lAIModem");
            LAiOffice = Resx.GetString("DIRECTIONS_lAIOffice");
            LCompN = Resx.GetString("DIRECTIONS_lCompN");
            LCompP = Resx.GetString("DIRECTIONS_lCompP");
            LCompT = Resx.GetString("DIRECTIONS_lCompT");
            LPhoneDect = Resx.GetString("DIRECTIONS_Field_Phone_Dect");
            LPhoneDesk = Resx.GetString("DIRECTIONS_Field_Phone_Table");
            LPhoneIp = Resx.GetString("DIRECTIONS_lPhoneIP");
            LPhoneIpCam = Resx.GetString("DIRECTIONS_Field_Phone_WebCam");
            LPhoneSim = Resx.GetString("DIRECTIONS_lPhoneSim");

            LPlExitInSide = Resx.GetString("DIRECTIONS_lPLExitInSide");
            LPlExitOutCountry = Resx.GetString("DIRECTIONS_lPLExitOutCountry");
            LPlExitOutTown = Resx.GetString("DIRECTIONS_lPLExitOutTown");
            LPlExitTown = Resx.GetString("DIRECTIONS_lPLExitTown");

            LSotrudnikParent1 = Resx.GetString("DIRECTIONS_lSotrudnikParent1");
            LSotrudnikParent2 = Resx.GetString("DIRECTIONS_lSotrudnikParent2");

            LRequire = Resx.GetString("DIRECTIONS_lRequire");
            LComplete = Resx.GetString("DIRECTIONS_lComplete");
        }

        /// <summary>
        ///     Заполнение таблицы с выполненными по указанию данными по оборудорванию
        /// </summary>
        private void CreateDataTableEquipment()
        {
            DtEquip = Dir.GetDirectionEquipment();
        }

        /// <summary>
        ///     Сотрудник входит в группу, которая не имеет логина
        /// </summary>
        /// <returns>Да/Нет</returns>
        private bool IsCommonEmployeeNoLogin()
        {
            var employee = Dir.Sotrudnik;
            if (employee != null && !employee.Unavailable)
                if (!string.IsNullOrEmpty(employee.CommonEmployeeID))
                {
                    var commonEmployeeId = new Employee(employee.CommonEmployeeID);
                    if (commonEmployeeId.Login.IsNullEmptyOrZero()) return true;
                }

            return false;
        }

        #endregion
    }
}