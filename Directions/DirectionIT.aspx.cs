using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Resources;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Hosting;
using System.Web.Script.Serialization;
using Kesco.Lib.BaseExtention;
using Kesco.Lib.BaseExtention.Enums.Controls;
using Kesco.Lib.BaseExtention.Enums.Corporate;
using Kesco.Lib.BaseExtention.Enums.Docs;
using Kesco.Lib.Entities.Corporate;
using Kesco.Lib.Entities.Corporate.Phones;
using Kesco.Lib.Entities.Documents;
using Kesco.Lib.Entities.Documents.EF.Directions;
using Kesco.Lib.Entities.Persons;
using Kesco.Lib.Entities.Persons.PersonOld;
using Kesco.Lib.Web.Controls.V4;
using Kesco.Lib.Web.Controls.V4.Common;
using Kesco.Lib.Web.Controls.V4.Common.DocumentPage;
using Kesco.Lib.Web.Settings;
using Convert = Kesco.Lib.ConvertExtention.Convert;

namespace Kesco.App.Web.Docs.Directions
{
    /*
     Подключаемый класс формы документа: Указание IT на организацию работы
     */
    /// <summary>
    ///     Класс формы документа указания на организацию работы
    /// </summary>
    public abstract partial class DirectionIT : DocPage
    {
        protected ResourceManager LocalResx = new ResourceManager("Kesco.App.Web.Docs.Directions.DirectionIT",
            Assembly.GetExecutingAssembly());

        protected Direction Dir
        {
            get { return (Direction) Doc; }
        }

        private RenderHelper _render;

        #region InitControls

        private void SetInitControls()
        {
            efSotrudnik.Filter.Status.ValueStatus = СотоянияСотрудника.Работающие;
            efSotrudnikParent.Filter.Status.ValueStatus = СотоянияСотрудника.РаботающиеИУволенные;
            efSotrudnikParent.Filter.HasLogin.ValueHasLogin = НаличиеЛогина.ЕстьЛогин;

            //-----------------------------------------------------------------

            efPLExit.Items = new Dictionary<string, string>
            {
                {"1", "только внутри холдинга"},
                {"7", "+ внутри страны"},
                {"15", "+ международная"}
            };

            Dir.Languages().ForEach(delegate(Language lang) { efLang.Items.Add(lang.Id, lang.Name); });
            Dir.DomainNames().ForEach(delegate(DomainName dName) { efDomain.Items.Add(dName.Id, dName.Id); });

            if (Dir.IsNew)
                efPLExit.Value = "7";

            efPRoles_Role.NextControl = "efPRoles_Person";
            efPRoles_Person.NextControl = "btnPRoles_Save";

            efPTypes_Catalog.NextControl = "efPTypes_Type";
            efPTypes_Type.NextControl = "efPTypes_Type_0";


        }

        #endregion

        #region SetHandlers

        private void SetHandlers()
        {
            efSotrudnik.OnRenderNtf += efSotrudnik_OnRenderNtf;
            efSotrudnikParent.OnRenderNtf += efSotrudnikParent_OnRenderNtf;
            efPRoles_Role.OnRenderNtf += efPRoles_Role_OnRenderNtf;
            efPTypes_Type.BeforeSearch += efPTypes_Type_BeforeSearch;
            efMailName.OnRenderNtf += efMailName_OnRenderNtf;
            efMailName.Changed += efMailName_Changed;
            efLogin.OnRenderNtf += efLogin_OnRenderNtf;
            efLogin.Changed += efLogin_Changed;
        }
        
        #endregion

        #region SetBinder

        private void SetBinders()
        {
            efSotrudnik.BindDocField = Dir.SotrudnikField;
            efMobilphone.BindDocField = Dir.RedirectNumField;
            efAdvEq.BindDocField = Dir.AdvEquipField;
            efPLExit.BindDocField = Dir.PhoneLinkField;

            efLogin.BindDocField = Dir.LoginField;
            efLang.BindDocField = Dir.SotrudnikLanguageField;
            efDomain.BindDocField = Dir.DomainField;
            efMailName.BindDocField = Dir.MailNameField;
            efAdvInfo.BindDocField = Dir.AdvInfoField;
            efAccessEthernet.BindDocField = Dir.AccessEthernetField;
            efSotrudnikParent.BindDocField = Dir.SotrudnikParentField;
        }

        #endregion

        #region Override

        protected RenderHelper Render {
            get { return _render ?? (_render = new RenderHelper()); }
        }

        protected override void DocumentInitialization(Document copy = null)
        {
            if (copy == null)
                Doc = new Direction();
            else
                Doc = (Direction) copy;

            if (Doc.IsNew)
                Doc.Date = DateTime.Today;
            ShowDocDate = false;

            SetBinders();
            SetHandlers();
           
        }

        protected override void LoadData(string id)
        {
            base.LoadData(id);
            if (!id.IsNullEmptyOrZero())
            {
                Dir.LoadDocumentPositions();

                RefreshPositionCommonFolders();
                RefreshPositionRoles();
                RefreshPositionTypes();
                RefreshPositionAdvancedGrants();
            }
            else
            {
                Dir.PositionCommonFolders = new List<PositionCommonFolder>();
                Dir.PositionRoles = new List<PositionRole>();
                Dir.PositionTypes = new List<PositionType>();
                Dir.PositionAdvancedGrants = new List<PositionAdvancedGrant>();
            }
            GetSotrudnikParentType();
        }

        protected override void DocumentToControls()
        {
            GetWorkPlaceType();
            RefreshWorkPlace("");
            DisplayDataWorkPlace();

            GetPhoneEquip();
            GetCompType();

            DisplayDataPhoneDesk(true);
            DisplayDataSim();
            DisplayDataEthernet();
        }

        protected override void SetControlProperties()
        {
            efSotrudnik.IsRequired = true;
            if (Dir.AccessEthernetField.ValueString.Length != 0 && Dir.AccessEthernetField.ValueInt == 1)
            {
                efLogin.IsRequired = true;
                efMailName.IsRequired = true;
            }

            if (Dir.SotrudnikParentCheckField.ValueString.Length != 0)
            {
                efSotrudnikParent.IsRequired = true;
            }
        }

        protected override void ProcessCommand(string cmd, NameValueCollection param)
        {
            switch (cmd)
            {
                case "SotrudnikParentSetInfo":
                    SotrudnikParentSetInfo();
                    break;
                case "EditPositionType":
                    EditPositionType(param["value"]);
                    break;
                case "DeletePositionByCatalog":
                    DeletePositionByCatalog(param["catalog"], param["closeForm"]);
                    break;
                case "DeletePositionType":
                    DeletePositionType(param["catalog"], param["theme"], param["closeForm"]);
                    break;
                case "SavePositionType":
                    SavePositionType(param["check"]);
                    break;
                case "NewPositionRole":
                    NewPositionRole(param["value"]);
                    break;
                case "EditPositionRole":
                    EditPositionRole(param["value"]);
                    break;
                case "DeletePositionRoleByGuid":
                    DeletePositionRoleByGuid(param["guid"], param["closeForm"]);
                    break;
                case "DeletePositionByRole":
                    DeletePositionByRole(param["role"]);
                    break;
                case "SavePositionRole":
                    SavePositionRole(param["value"], param["check"]);
                    break;
                case "DeletePositionAG":
                    DeletePositionAG(param["value"]);
                    break;
                case "SavePositionAG":
                    SavePositionAG(param["value"]);
                    break;
                case "DeletePositionCommonFolders":
                    DeletePositionCommonFolders(param["value"]);
                    break;
                case "SavePositionCommonFolders":
                    SavePositionCommonFolders(param["value"]);
                    break;
                case "AddPositionAdvancedGrants":
                    AddPositionAdvancedGrants();
                    break;
                case "AddPositionCommonFolders":
                    AddPositionCommonFolders();
                    break;
                case "AddPositionTypes":
                    AddPositionTypes();
                    break;
                case "AddPositionRole":
                    AddPositionRole();
                    break;
                case "RenderWorkPlaceList":
                    RenderWorkPlaceList(param["x"], param["y"]);
                    break;
                case "RenderMailNamesList":
                    RenderMailNamesList(param["x"], param["y"]);
                    break;
                case "AdvSearchWorkPlace":
                    AdvSearchWorkPlace();
                    break;
                case "SetWorkPlace":
                    SetWorkPlace(param["value"], param["label"]);
                    break;
                case "SetMailName":
                    SetMailName(param["value"], param["label"]);
                    break;
                case "ClearWorkPlace":
                    ClearWorkPlace();
                    break;
                case "TestSendMessage":
                    var jsStr = DocViewInterop.SendMessageDocument("1", "284,724,1", "Нихера не работает!!!");
                    JS.Write(jsStr);
                    break;
                case "TestCheckSimilar":
                    var jsStr1 = DocViewInterop.CheckSimilarDocument(HttpContext.Current, "0", "2353", "15.01.2018", "13", "1603,15950,22778","2");
                    JS.Write(jsStr1);
                    break;
                case "SendMessageDocument":
                    JS.Write(DocViewInterop.SendMessageDocument(param["docid"], param["empids"], param["msg"]));
                    break;
                default:
                    base.ProcessCommand(cmd, param);
                    break;
            }
        }

        protected override bool ValidateDocument(out List<string> errors, params string[] exeptions)
        {
            base.ValidateDocument(out errors);
            
            var bitMask = Dir.WorkPlaceTypeField.ValueInt;

            if (bitMask == 0)
            {
                errors.Add("Не указано, что требуется организовать сотрдунику.");
                return false;
            }

            //рабочее место в офисе==============================================================
            if ((bitMask & 1) == 1 || (bitMask & 2) == 2)
            {
                if ((bitMask & 1) == 1 && Dir.WorkPlaceField.ValueString.Length == 0)
                {
                    errors.Add("Не указано рабочее место в офисе.");
                    return false;
                }

                if ((Dir.PhoneEquipField.ValueString.Length == 0 || Dir.PhoneEquipField.ValueInt == 0)
                    && (Dir.CompTypeField.ValueString.Length == 0 || Dir.CompTypeField.ValueInt == 0)
                    && Dir.AdvEquipField.ValueString.Length == 0
                    && Dir.AdvInfoField.ValueString.Length == 0
                    && (Dir.AccessEthernetField.ValueString.Length == 0 || Dir.AccessEthernetField.ValueInt == 0)
                    )
                {
                    errors.Add("Не указано, что потребуется сотруднику для работы в офисе.");
                    return false;
                }
            }
            //только доступ к корпоративной сети через Internet ==============================================================
            if (bitMask == 4 || (Dir.AccessEthernetField.ValueString.Length != 0 && Dir.AccessEthernetField.ValueInt == 1))
            {
                if (Dir.AccessEthernetField.ValueString.Length == 0 || Dir.AccessEthernetField.ValueInt == 0)
                {
                    errors.Add("Не указан доступ к корпоративной сети.");
                    return false;
                }
                if (Dir.LoginField.ValueString.Length == 0)
                    errors.Add(LocalResx.GetString("_NTF_NoLogin"));
                if (Dir.MailNameField.ValueString.Length == 0)
                    errors.Add(LocalResx.GetString("_NTF_NoEmail"));
                if (Dir.DomainField.ValueString.Length == 0)
                    errors.Add(LocalResx.GetString("_NTF_NoDomain"));

                if (errors.Count > 0) return false;
            }


            if (Dir.SotrudnikParentCheckField.ValueString.Length != 0 &&
                Dir.SotrudnikParentField.ValueString.Length == 0)
            {
                if (Dir.SotrudnikParentCheckField.ValueInt==1)
                    errors.Add("Не заполнено поле 'как у сотрудника'");
                else
                    errors.Add("Не заполнено поле 'вместо сотрудника'");
                
                return false;
            }


            using (var w = new StringWriter())
            {
                ValidationMessages.CheckSotrudnikWorkPlaceGroup(this, w, Dir);
                var ws = w.ToString();
                if (ws.Length>0) errors.Add(ws);
            }


            if (errors.Count > 0)
                return false;


            ValidateDataFillCorrect();
            return true;
        }

        private void ValidateDataFillCorrect()
        {
            var bitMaskWP = Dir.WorkPlaceTypeField.ValueInt;
            var bitMaskPhoneEq = Dir.PhoneEquipField.ValueInt;

            if ((bitMaskWP & 1) != 1)
            {
                Dir.WorkPlaceField.ValueString = "";
                if ((bitMaskPhoneEq & 2) == 2) SetPhoneEquip(2, "0");

            }

            if ((bitMaskWP & 1) != 1 && (bitMaskWP & 2) != 2)
            {
                Dir.PhoneEquipField.ValueString = "";
                Dir.CompTypeField.ValueString = "";
                Dir.AdvEquipField.ValueString = "";
            }

            if (Dir.AccessEthernetField.ValueString.Length == 0 || Dir.AccessEthernetField.ValueInt == 0)
            {
                Dir.LoginField.ValueString = "";
                Dir.MailNameField.ValueString = "";
                Dir.DomainField.ValueString = "";
                Dir.SotrudnikParentCheckField.ValueString = "";
                Dir.PositionCommonFolders.Clear();
                Dir.PositionRoles.Clear();
                Dir.PositionTypes.Clear();
                Dir.PositionAdvancedGrants.Clear();

            }

            if (Dir.SotrudnikParentCheckField.ValueString.Length == 0)
                Dir.SotrudnikParentField.ValueString = "";

            Dir.PersonZakazchikField.ValueString = Dir.Sotrudnik.OrganizationId.HasValue ? Dir.Sotrudnik.OrganizationId.Value.ToString() : "";
            Dir.PersonEmployerField.ValueString = Dir.Sotrudnik.PersonEmployeeId.HasValue ? Dir.Sotrudnik.PersonEmployeeId.Value.ToString() : "";
        }
        

        protected void Page_Load(object sender, EventArgs e)
        {

            //var x = "http://sip-hnov.kescom.com/api/authenticate?key=" + HttpUtility.UrlEncode("dk4E%xtm@7amb$") +
            //        "&username=apiadmin&password=" + HttpUtility.UrlEncode("5x5=25");


            if (!V4IsPostBack && !DocEditable)
            {
                V4Redirect("DirectionITSigned.aspx");
                return;
            }
            if (!V4IsPostBack)
            {
                if (DocEditable)
                    efSotrudnik.Focus();

                SetInitControls();

                JS.Write(@"directions_clientLocalization = {{
CONFIRM_StdMessage:""{0}"",
CONFIRM_StdTitle:""{1}"", 
CONFIRM_StdCaptionYes:""{2}"",
CONFIRM_StdCaptionNo:""{3}"",
cmdDelete:""{4}"",
cmdOK:""{5}"",
cmdCancel:""{6}"",
cmdUncheck:""{7}"",
cmdWorkplaceOther:""{8}"",
DIRECTIONS_FORM_CF_Title:""{9}"",
DIRECTIONS_FORM_AG_Title:""{10}"",
DIRECTIONS_FORM_Role_Title:""{11}"",
DIRECTIONS_FORM_Type_Title:""{12}"",
DIRECTIONS_FORM_WP_Title:""{13}"",
DIRECTIONS_FORM_Mail_Title:""{14}""
}};",
                    Resx.GetString("CONFIRM_StdMessage"),
                    Resx.GetString("CONFIRM_StdTitle"),
                    Resx.GetString("CONFIRM_StdCaptionYes"),
                    Resx.GetString("CONFIRM_StdCaptionNo"),
                    Resx.GetString("cmdDelete"),
                    Resx.GetString("cmdOK"),
                    Resx.GetString("cmdCancel"),
                    Resx.GetString("cmdUncheck"),
                    Resx.GetString("cmdWorkplaceOther"),
                    Resx.GetString("DIRECTIONS_FORM_CF_Title"),
                    Resx.GetString("DIRECTIONS_FORM_AG_Title"),
                    Resx.GetString("DIRECTIONS_FORM_Role_Title"),
                    Resx.GetString("DIRECTIONS_FORM_Type_Title"),
                    Resx.GetString("DIRECTIONS_FORM_WP_Title"),
                    Resx.GetString("DIRECTIONS_FORM_Mail_Title")
                    );
                
                if (Dir.IsNew)
                    JS.Write("directions_initNewDoc();");
            }
        }

        //protected override void SaveDocument()
        //{
        //    base.SaveDocument();
        //    if (Dir.Id.IsNullEmptyOrZero()) return;
        //    Dir.SaveDocumentPositions(false);
        //}

        #endregion

        #region Handlers

        #region Prerender

        private void efSotrudnik_OnRenderNtf(object sender, Ntf ntf)
        {
            ntf.Clear();
            if (Dir.SotrudnikField.ValueString.Length == 0) return;

            var employee = Dir.Sotrudnik;
            if (employee.Unavailable)
            {
                efSotrudnik.ValueText = "#" + employee.Id;
                ntf.Add(LocalResx.GetString("_Msg_СотрудникНеДоступен"), NtfStatus.Error);
                return;
            }

            using (var w = new StringWriter())
            {
                List<string> ntfList = new List<string>();
                ValidationMessages.CheckSotrudnik(this, w, Dir, ntfList);
                foreach (string msg in ntfList)
                    ntf.Add(msg, NtfStatus.Error);

                ntfList.Clear();
                ValidationMessages.CheckSotrudnikStatus(this, w, Dir, ntfList);
                foreach (string msg in ntfList)
                    ntf.Add(msg, NtfStatus.Error);
            }

        }

        private void efSotrudnikParent_OnRenderNtf(object sender, Ntf ntf)
        {
            ntf.Clear();
            
            Employee p = Dir.SotrudnikParent;
            if (p == null) return;

            if (p.Unavailable)
            {
                efSotrudnikParent.ValueText = "#" + p.Id;
                ntf.Add(LocalResx.GetString("_Msg_СотрудникНеДоступен"), NtfStatus.Error);
                return;
            }

            if (Dir.SotrudnikParentCheckField.ValueString.Length == 0) return;
            
            var bitMask = int.Parse(Dir.SotrudnikParentCheckField.ValueString);

            using (var w = new StringWriter())
            {
                List<string> ntfList = new List<string>();
                ValidationMessages.CheckSotrudnikParent(this, w, Dir, ntfList);
                foreach (string msg in ntfList)
                    ntf.Add(msg, NtfStatus.Error);

                ntfList.Clear();
                ValidationMessages.CheckSotrudnikParentStatus(this, w, Dir, ntfList);
                foreach (string msg in ntfList)
                    ntf.Add(msg, NtfStatus.Error);
            }
            
        }

       
        private void efPRoles_Role_OnRenderNtf(object sender, Ntf ntf)
        {
            RefreshPRoles_Role_Description();
        }

        private void efMailName_OnRenderNtf(object sender, Ntf ntf)
        {
            ntf.Clear();
            var ws = "";

            using (var w = new StringWriter())
            {
                ValidationMessages.CheckEmailName(this, w, Dir);
                ws = w.ToString();
                if (ws.Length > 0) ntf.Add(ws, NtfStatus.Error);
            }
            using (var w = new StringWriter())
            {
                ValidationMessages.CheckUniqueEmail(this, w, Dir);
                ws = w.ToString();
                if (ws.Length > 0) ntf.Add(ws, NtfStatus.Error);
            }
        }

        private void efLogin_OnRenderNtf(object sender, Ntf ntf)
        {
            ntf.Clear();
            var ws = "";

            using (var w = new StringWriter())
            {
                ValidationMessages.CheckLogin(this, w, Dir);
                ws = w.ToString();
                if (ws.Length > 0) ntf.Add(ws, NtfStatus.Error);
            }

            using (var w = new StringWriter())
            {
                ValidationMessages.CheckUniqueLogin(this, w, Dir);
                ws = w.ToString();
                if (ws.Length > 0) ntf.Add(ws, NtfStatus.Error);
            }
        }

        #endregion

        #region BeforeSearch

        protected void efSotrudnik_OnBeforeSearch(object sender)
        {
        }

        protected void efDonosEmpl_OnBeforeSearch(object sender)
        {
        }

        protected void efPTypes_Type_BeforeSearch(object sender)
        {
            efPTypes_Type.Filter.Catalog.Clear();
            if (efPTypes_Catalog.Value.Length > 0 && !efPTypes_Catalog.Value.Equals(efPTypes_Catalog.CustomRecordId))
                efPTypes_Type.Filter.Catalog.Add(efPTypes_Catalog.Value);
        }

        #endregion

        #region Changed

        protected void efSotrudnik_OnChanged(object sender, ProperyChangedEventArgs e)
        {
            SotrudnikClearInfo();
            if (Dir.SotrudnikField.ValueString.Length > 0)
            {
                var employee = Dir.Sotrudnik;
                if (employee != null && !employee.Unavailable)
                {
                    var dt = Dir.SupervisorData;
                    if (dt.Rows.Count != 0)
                    {
                        Dir.SotrudnikPostField.Value = dt.Rows[0]["ДолжностьСотрудника"];
                        Dir.SupervisorField.Value = dt.Rows[0]["КодРуководителя"];
                    }

                    if (Dir.SotrudnikField.ValueString.Length > 0)
                        Dir.RedirectNumField.Value = employee.GetMobilePhone();
                }
            }
            RefreshSotrudnikInfo();
        }
        
        protected void efMobilphone_OnChanged(object sender, ProperyChangedEventArgs e)
        {
            if (Dir.RedirectNumField.ValueString.Length > 0)
            {
                if (!Regex.IsMatch(Dir.RedirectNumField.ValueString, RegexPattern.PhoneNumber))
                {
                    ShowMessage(LocalResx.GetString("_Msg_CheckPhoneNumber"), efMobilphone, LocalResx.GetString("_Msg_AlertTitle"));
                    Dir.RedirectNumField.ValueString = "";
                    efMobilphone.Value = "";
                }
            }
            RefreshMobilphoneArea();
        }
        
        protected void efWorkPlaceType1_OnChanged(object sender, ProperyChangedEventArgs e)
        {
            SetWorkPlaceType(DirectionsWorkPlaceTypeBitEnum.РабочееМестоВОфисе, e.NewValue);
            DisplayDataWorkPlace();
            RefreshWorkPlace("");
        }

        protected void efWorkPlaceType2_OnChanged(object sender, ProperyChangedEventArgs e)
        {
            SetWorkPlaceType(DirectionsWorkPlaceTypeBitEnum.РабочееМестоВнеОфиса, e.NewValue);
            DisplayDataWorkPlace();
        }

        protected void efWorkPlaceType4_OnChanged(object sender, ProperyChangedEventArgs e)
        {
            SetWorkPlaceType(DirectionsWorkPlaceTypeBitEnum.ДоступЧерезVPN, e.NewValue);
            DisplayDataWorkPlace();
            if (e.NewValue.Equals("1") && Dir.AccessEthernetField.ValueString != "1")
            {
                Dir.AccessEthernetField.Value = 1;
                DisplayDataEthernet();
                SetRequiredLogin();
                SetRequiredMailName();
                
            }
        }

        protected void efWorkPlace_OnChanged(object sender, ProperyChangedEventArgs e)
        {
        }

        protected void efPhoneDesk_OnChanged(object sender, ProperyChangedEventArgs e)
        {
            SetPhoneEquip(1, e.NewValue);
            DisplayDataPhoneDesk(true);
        }

        protected void efPhoneIPCam_OnChanged(object sender, ProperyChangedEventArgs e)
        {
            SetPhoneEquip(8, e.NewValue);
        }

        protected void efPhoneDect_OnChanged(object sender, ProperyChangedEventArgs e)
        {
            SetPhoneEquip(2, e.NewValue);
            DisplayDataPhoneDesk(false);
        }

        protected void efPLExit_OnChanged(object sender, ProperyChangedEventArgs e)
        {
        }

        protected void efPhoneSim_OnChanged(object sender, ProperyChangedEventArgs e)
        {
            SetPhoneEquip(16, e.NewValue);
            if ((Dir.PhoneEquipField.ValueInt & 16) == 16) SetPhoneEquip(32, "1");
            DisplayDataSim();
        }

        protected void efAccessInternetGPRS_OnChanged(object sender, ProperyChangedEventArgs e)
        {
            SetPhoneEquip(32, e.NewValue);
        }

        protected void efComputer_OnChanged(object sender, ProperyChangedEventArgs e)
        {
            SetCompType(2, e.NewValue);
        }

        protected void efNotebook_OnChanged(object sender, ProperyChangedEventArgs e)
        {
            SetCompType(4, e.NewValue);
        }

        protected void efAccessEthernet_OnChanged(object sender, ProperyChangedEventArgs e)
        {
            DisplayDataEthernet();
            SetRequiredLogin();
            SetRequiredMailName();
        }
        
        protected void efSotrudnikParentCheck1_OnChanged(object sender, ProperyChangedEventArgs e)
        {
            SetSotrudnikParentType(DirectionsSotrudnikParentBitEnum.КакУСотрудника, e.NewValue);
            
        }

        protected void efSotrudnikParentCheck2_OnChanged(object sender, ProperyChangedEventArgs e)
        {
            SetSotrudnikParentType(DirectionsSotrudnikParentBitEnum.ВместоСотрудника, e.NewValue);
            
        }
        
        protected void efSotrudnikParent_OnChanged(object sender, ProperyChangedEventArgs e)
        {
            if (Dir.SotrudnikField.Value != null && Dir.SotrudnikField.Value.Equals(Dir.SotrudnikParentField.Value))
            {
                Dir.SotrudnikParentField.ValueString = e.OldValue;
                ShowMessage(LocalResx.GetString("_NTF_СотрудникСовпадает"), "Сообщение", MessageStatus.Warning, efSotrudnikParent.HtmlID);
            }
            else if (Dir.SotrudnikParentField.ValueString.Length > 0)
                ShowConfirm(string.Format(LocalResx.GetString("_Msg_FillFormByEmpl") + " " + Dir.SotrudnikParent.FullName + "?"), "cmdasync('cmd', 'SotrudnikParentSetInfo');", null);

            efSotrudnikParent.IsRequired = Dir.SotrudnikParentField.ValueString.Length <= 0;
    
        }

        protected void efMailName_Changed(object sender, ProperyChangedEventArgs e)
        {
            SetRequiredMailName();

        }

        protected void efLogin_Changed(object sender, ProperyChangedEventArgs e)
        {
            SetRequiredLogin();
            if (Dir.LoginField.ValueString.Length == 0) return;

            Dir.MailNameField.Value = Dir.LoginField.Value;
            efMailName.RenderNtf();
        }

        #region #Positions

        protected void efPRoles_Role_OnChanged(object sender, ProperyChangedEventArgs e)
        {
        }

        protected void efPTypes_Catalog_OnChanged(object sender, ProperyChangedEventArgs e)
        {
            FillPositionThemeItems(e.NewValue);

            var p =
                Dir.PositionTypes.FirstOrDefault(
                    x =>
                        (x.CatalogId.HasValue ? x.CatalogId.Value.ToString() : efPTypes_Catalog.CustomRecordId) ==
                        e.NewValue);
            if (p != null)
            {
                JS.Write("directions_setAttribute('{0}', '{1}', '{2}');", "divPositionTypesAdd", "data-id", e.NewValue);
                JS.Write("$('#btnPTypes_Delete').show();");
            }
            else
                JS.Write("$('#btnPTypes_Delete').hide();");
        }
        
        protected void efPTypes_Type_OnChanged(object sender, ProperyChangedEventArgs e)
        {
            if (e.NewValue.Equals(efPTypes_Type.CustomRecordId) &&
                efPTypes_Catalog.Value.Equals(efPTypes_Catalog.CustomRecordId))
                efPTypes_Catalog.Value = "";

            if (e.NewValue.Length == 0) return;

            var catalogId = (efPTypes_Catalog.Value.Length == 0 ||
                             efPTypes_Catalog.Value.Equals(efPTypes_Catalog.CustomRecordId))
                ? 0
                : int.Parse(efPTypes_Catalog.Value);

            var list = PersonTheme.GetParentAndChildThemes(int.Parse(e.NewValue), catalogId);

            if (list.Count > 0 && efPTypes_Type.SelectedItems.Count > 0)
            {
                efPTypes_Type.SelectedItems.RemoveAll(x => list.Any(y => x.Id == y.Id));
                efPTypes_Type.RefreshDataBlock();
            }
        }

        #endregion

        #endregion

        #endregion

        #region Render
        
        protected void RenderMobilphoneArea(TextWriter w)
        {
            var phoneNum = Dir.RedirectNumField.ValueString;
            var direction = Dir.FormatingMobilNumber(ref phoneNum);
            w.Write(direction);
        }
        
      
        private void RenderWorkPlaceList(object x, object y)
        {
            var w = new StringWriter();

            if (Dir.SotrudnikField.ValueString.Length == 0)
            {
                ShowMessage("Укажите сотрудника!", efSotrudnik);
                return;
            }

            var wps = Dir.Sotrudnik.Workplaces;
            var inx = 0;
            foreach (var wp in wps)
            {
                if (!wp.WorkPlacePar.Equals((int)ТипыРабочихМест.КомпьютеризированноеРабочееМесто)) continue;
                w.Write("<div>");
                w.Write("<nobr>");
                w.Write(
                    "<a id='imgWP_{1}' tabindex=0 onkeydown='var key=v4_getKeyCode(event); if(key == 13 || key == 32) {{directions_SetWorkPlace({0});}}' onclick='directions_SetWorkPlace({0});'><img src='/styles/backtolist.gif' border='0'/></a>",
                    wp.Id, inx);
                w.Write("<span class='marginL'>");
                RenderLinkLocation(w, wp.Id);
                w.Write(wp.Path);
                RenderLinkEnd(w);
                w.Write("</span>");
                w.Write("</nobr>");
                w.Write("</div>");
                inx++;
            }

            if (wps.Count == 0)
                RenderNtf(w, new List<string>(new[] { LocalResx.GetString("_Msg_СотрудникНетРабМеста") }),
                    NtfStatus.Error);

            RefreshControlText(w, "divWPL_Body");
            JS.Write("directions_SetPositionWorkPlaceList();");
            if (wps.Count == 0)
                AdvSearchWorkPlace();
        }

        #endregion

        #region Refresh data context

        private void RefreshControlText(TextWriter w, string controlId)
        {
            JS.Write("var obj_{0} = document.getElementById('{0}'); if(obj_{0}){{obj_{0}.innerHTML='{1}';}}", controlId,
                HttpUtility.JavaScriptStringEncode(w.ToString()));
        }


        private void RefreshPhoto()
        {
            var w = new StringWriter();
            Render.Photo(this, w, Dir);
            RefreshControlText(w, "divPhoto");
        }

        private void RefreshSotrudnikPost()
        {
            var w = new StringWriter();
            Render.SotrudnikPost(this, w, Dir);
            RefreshControlText(w, "spSotrudnikPost");
        }

        private void RefreshSotrudnikAOrg()
        {
            var w = new StringWriter();
            Render.SotrudnikAOrg(this, w, Dir);
            RefreshControlText(w, "spSotrudnikAOrg");
        }

        private void RefreshSotrudnikFinOrg()
        {
            var w = new StringWriter();
            Render.SotrudnikFinOrg(this, w, Dir);
            RefreshControlText(w, "divSotrudnikFinOrg");
        }

        private void RefreshSupervisorInfo()
        {
            var w = new StringWriter();
            Render.Supervisor(this, w, Dir);
            RefreshControlText(w, "divSupervisor");
        }

        private void RefreshMobilphoneArea()
        {
            var w = new StringWriter();
            RenderMobilphoneArea(w);
            RefreshControlText(w, "spMobilphoneArea");
        }

        private void RefreshWorkPlace(string label)
        {
            var w = new StringWriter();
            var bit = Dir.WorkPlaceTypeField.ValueInt;

            if ((bit & 1) == 1 && Dir.WorkPlaceField.ValueString.Length > 0)
                Render.WorkPlace(this, w, label, Dir);
            else
                RenderNtf(w, new List<string>{"не указано рабочее место"}, NtfStatus.Error);
            RefreshControlText(w, "divWorkPlace");
        }

        private void RefreshPRoles_Role_Description()
        {
            var w = new StringWriter();
            if (efPRoles_Role.Value.Length == 0)
                w.Write("");
            else
            {
                var r = new Role(efPRoles_Role.Value);
                RenderNtf(w, new List<string> {r.Description}, NtfStatus.Information);
            }

            RefreshControlText(w, "efPRoles_Role_Description");
        }

        #endregion

        #region Positions

        #region Common Folders

        private void AddPositionCommonFolders()
        {
            if (Dir.SotrudnikField.ValueString.Length == 0)
            {
                ShowMessage("Укажите сотрудника!", efSotrudnik);
                return;
            }

            RefreshPositionDialogCommonFolders();
            JS.Write("directions_SetPositionCFAdd();");
        }

        private void RefreshPositionDialogCommonFolders()
        {
            var w = new StringWriter();

            Dir.CommonFolders().ForEach(delegate(CommonFolder cf)
            {
                var check = false;
                if (Dir.PositionCommonFolders != null)
                {
                    var p = Dir.PositionCommonFolders.FirstOrDefault(x => x.CommonFolderId.ToString() == cf.Id);
                    check = (p != null);
                   
                }
                
                w.Write("<div class='marginL div_cf'>");
                w.Write("<input id='cf_{0}' data-id='{0}' data-name='{1}' class='CF' type='checkbox' {2}>", cf.Id,
                    HttpUtility.HtmlEncode(cf.Name), check ? "checked" : "");
                w.Write("<div class='disp_inlineBlock marginL'>");
                w.Write(cf.Name);
                w.Write("</div>");
                w.Write("</div>");
            }
                );
            
            RefreshControlText(w, "divCF_Body");
        }

        private void DeletePositionCommonFolders(string value)
        {
            var p = Dir.PositionCommonFolders.FirstOrDefault(x => x.Id == value);
            if (p != null)
                p.Delete(false);
            Dir.LoadPositionCommonFolders();
            RefreshPositionCommonFolders();
            if (Dir.PositionCommonFolders.Count == 0)
                JS.Write("directions_setElementFocus(null,'btnLinkRolesAdd');");
            else
                JS.Write("directions_setElementFocus('DelCF');");
        }

        private void SavePositionCommonFolders(string jsonValues)
        {
            var isNew = Dir.IsNew;
            if (isNew)
            {
                var result = SaveDocument(false);
                if (!result) return;
            }

            var serializer = new JavaScriptSerializer();
            var values = serializer.Deserialize<Dictionary<string, string>>(jsonValues);
            Dir.SavePositionsCommonFoldersByDictionary(values, !isNew);

            if (isNew) return;
            ReloadPositionCommonFolders(true);
        }

        private void ReloadPositionCommonFolders(bool setFocus = false)
        {
           // Dir.LoadPositionCommonFolders();
            RefreshPositionCommonFolders();
            if (setFocus)
                JS.Write("directions_setElementFocus(null,'btnLinkCFAdd');");
        }

        private void RefreshPositionCommonFolders()
        {
            var w = new StringWriter();
            RenderPositionCommonFolders(w);
            RefreshControlText(w, "divCommonFoldersData");
        }

        private void RenderPositionCommonFolders(TextWriter w)
        {
            w.Write("");
            var sortedList = Dir.PositionCommonFolders.OrderBy(o => o.CommonFolderName).ToList();
            sortedList.ForEach(delegate(PositionCommonFolder p)
            {
                w.Write("<div class='marginL disp_inlineBlock'>");
                w.Write(
                    "<a href='javascript:void(0);' class='DelCF' id='btnDelCF_{0}' onclick=\"{1}\" title='{2}'>",
                    p.Id,
                    ShowConfirmDeleteGetJS(string.Format("directions_deleteCFCallBack('{0}')", p.Id)),
                    "удалить"
                    );
                w.Write("<img src='/styles/delete.gif' class= border=0>");
                w.Write("</a>");
                w.Write("{0};", p.CommonFolderName);
                w.Write("</div>");
            });
        }

        private void FillPositionCommonFoldersByEmployee(Employee empl)
        {
            var commonFolders = empl.CommonFolders;
            commonFolders.ForEach(delegate(CommonFolder cf)
            {
                var cf0 = Dir.PositionCommonFolders.FirstOrDefault(x => x.CommonFolderId.ToString() == cf.Id);
                if (cf0 == null)
                {
                    var p = new PositionCommonFolder
                    {
                        DocumentId = Dir.DocId,
                        CommonFolderId = int.Parse(cf.Id),
                        CommonFolderName = cf.Name
                    };
                    Dir.PositionCommonFolders.Add(p);
                }
            });
            RefreshPositionCommonFolders();
        }

        #endregion

        #region Roles

        private void AddPositionRole()
        {
            if (Dir.SotrudnikField.ValueString.Length == 0)
            {
                ShowMessage("Укажите сотрудника!", efSotrudnik);
                return;
            }

            RefreshSotrudnikLink("efRoleEmpl", "divPRoles_Employee");
            JS.Write("directions_setAttribute('{0}', '{1}', '{2}');", "divPositionRolesAdd", "data-id", "");
            JS.Write("directions_SetPositionRolesAdd();");
            JS.Write("$('#btnPRoles_Delete').hide();");

            ClearDialogPositionRole();
            efPRoles_Role.EvalURLClick(efPRoles_Role.URLAdvancedSearch);
        }

        private void ClearDialogPositionRole()
        {
            efPRoles_Role.Value = "";
            efPRoles_Person.Value = "";
            RefreshPRoles_Role_Description();
        }

        private void NewPositionRole(string value)
        {
            if (Dir.SotrudnikField.ValueString.Length == 0)
            {
                ShowMessage("Укажите сотрудника!", efSotrudnik);
                return;
            }

            RefreshSotrudnikLink("efRoleEmpl", "divPRoles_Employee");
            efPRoles_Role.Value = value;
            efPRoles_Person.Value = "";
            efChanged.ChangedByID = null;
            efChanged.SetChangeDateTime = DateTime.MinValue;

            JS.Write("directions_setAttribute('{0}', '{1}', '{2}');", "divPositionRolesAdd", "data-id", "");
            JS.Write("$('#btnPRoles_Delete').hide();");
            JS.Write("directions_SetPositionRolesAdd();");
        }

        private void EditPositionRole(string value)
        {
            if (Dir.SotrudnikField.ValueString.Length == 0)
            {
                ShowMessage("Укажите сотрудника!", efSotrudnik);
                return;
            }


            var p = Dir.PositionRoles.FirstOrDefault(x => x.GuidId.ToString() == value);
            if (p == null)
            {
                ShowMessage("Запись не найдена!", "btnLinkRolesAdd");
                ReloadPositionRoles(true);
                return;
            }
            RefreshSotrudnikLink("efRoleEmpl", "divPRoles_Employee");
            efPRoles_Role.Value = p.RoleId.ToString();
            efPRoles_Person.Value = p.PersonId == 0 ? "" : p.PersonId.ToString();
            efPRoles_Person.ValueText = p.PersonId == 0 ? "" : p.PersonName;

            RefreshPRoles_Role_Description();
            JS.Write("directions_setAttribute('{0}', '{1}', '{2}');", "divPositionRolesAdd", "data-id", p.GuidId);
            JS.Write("$('#btnPRoles_Delete').show();");
            JS.Write("directions_SetPositionRolesAdd();");

            efChanged.SetChangeDateTime = p.ChangedTime;
            efChanged.ChangedByID = p.ChangedId;
        }

        private void DeletePositionRoleByGuid(string guid, string closeForm)
        {
            if (Dir.IsNew)
            {
                Dir.PositionRoles.RemoveAll(x => x.GuidId.ToString() == guid);
                RefreshPositionRoles();
            }
            else
            {
                var list = Dir.PositionRoles.Where(x => x.GuidId.ToString() == guid).ToList();
                list.ForEach(delegate(PositionRole p) { p.Delete(false); });
                ReloadPositionRoles(true);
            }
            
            if (!string.IsNullOrEmpty(closeForm))
                JS.Write("directions_ClosePositionRolesAdd();");
        }

        private void DeletePositionByRole(string role)
        {
            if (Dir.IsNew)
            {
                Dir.PositionRoles.RemoveAll(x => x.RoleId.ToString() == role);
                RefreshPositionRoles();
            }
            else
            {
                var list = Dir.PositionRoles.Where(x => x.RoleId.ToString() == role).ToList();
                list.ForEach(delegate(PositionRole p) { p.Delete(false); });
                ReloadPositionRoles(true);
            }
            JS.Write("directions_ClosePositionRolesAdd();");
        }

        private void SavePositionRole(string value, string check)
        {
            PositionRole p;

            if (check.Equals("1"))
            {
                if (efPRoles_Role.Value.Length == 0)
                {
                    ShowMessage("Сохранение невозможно!" + Environment.NewLine + "Не указана роль сотрудника.",
                        "Сообщение", MessageStatus.Warning, efPRoles_Role.HtmlID);
                    return;
                }

                p = Dir.PositionRoles.FirstOrDefault(
                    x =>
                        x.GuidId.ToString() != value && x.RoleId.ToString() == efPRoles_Role.Value &&
                        (x.PersonId == 0 ||
                         x.PersonId.ToString() == (efPRoles_Person.Value.Length == 0 ? "0" : efPRoles_Person.Value)));

                if (p != null)
                {
                    if (p.PersonId == 0 && efPRoles_Person.Value.Length > 0)
                        p.PersonId = int.Parse(efPRoles_Person.Value);
                    else
                    {
                        ShowMessage("Сохранение невозможно!" + Environment.NewLine + "Такая запись уже существует.",
                            "Сообщение", MessageStatus.Warning, efPRoles_Person.HtmlID);
                        return;
                    }
                    
                }


                if (efPRoles_Person.Value.Length == 0)
                {
                    var message = "Вы уверены, что хотите, чтобы данный сотрудник" + Environment.NewLine +
                                  "выполнял указанную роль во всех существующих компаниях холдинга" +
                                  Environment.NewLine + "и компаниях, которые войдут в холдинг в будущем?";
                    var callbackYes = string.Format("cmdasync('cmd','SavePositionRole', 'value', '{0}', 'check', 0);",
                        value);
                    ShowConfirm(message, "Сообщение", "Да", "Нет", callbackYes, "", 500);
                    return;
                }
            }

            var isNew = Dir.IsNew;
            if (isNew)
            {
                var result = SaveDocument(false);
                if (!result) return;
            }

            if (value.Length > 0)
            {
                p = Dir.PositionRoles.FirstOrDefault(x => x.GuidId.ToString() == value);

                if (p != null)
                {
                    if (efPRoles_Role.ValueInt != null) p.RoleId = efPRoles_Role.ValueInt.Value;
                    p.PersonId = efPRoles_Person.ValueInt ?? 0;
                }
            }
            else
            {
                p = new PositionRole {DocumentId = int.Parse(Dir.Id)};
                if (efPRoles_Role.ValueInt != null) p.RoleId = efPRoles_Role.ValueInt.Value;
                p.PersonId = efPRoles_Person.ValueInt ?? 0;
                p.Save(false);
            }

            if (isNew) return;

            JS.Write("directions_ClosePositionRolesAdd('btnLinkTypesAdd');");
            ReloadPositionRoles();
        }

        private void ReloadPositionRoles(bool setFocus = false)
        {
            Dir.LoadPositionRoles();
            RefreshPositionRoles();
            if (setFocus)
                JS.Write("directions_setElementFocus(null,'btnLinkRolesAdd');");
        }

        private void RefreshPositionRoles()
        {
            var w = new StringWriter();
            RenderPositionRoles(w);
            RefreshControlText(w, "divPositionRolesData");
        }

        private void RenderPositionRoles(TextWriter w)
        {
            if (Dir.PositionRoles.Count == 0)
            {
                w.Write("");
                return;
            }

            var listRoles = Dir.PositionRoles
                .GroupBy(p => p.RoleId)
                .Select(g => g.First()).OrderBy(o => o.RoleObject.Name).ToList();

            listRoles.ForEach(delegate(PositionRole p0)
            {
                w.Write("<div class='marginL disp_inlineBlock'>");

                w.Write(
                    "<a href=\"javascript:void(0);\" id=\"btnLinkRDel_{0}\" onclick=\"{1}\" title=\"{2}\"><img border=\"0\" src=\"/styles/delete.gif\"/></a>",
                    p0.RoleId,
                    ShowConfirmDeleteGetJS(string.Format("directions_deleteRoleAllCallBack('{0}')", p0.RoleId)),
                    "удалить");

                w.Write("{0}:", p0.RoleObject.Name);

                var r = Dir.PositionRoles.FirstOrDefault(x => x.PersonId == 0 && x.RoleId == p0.RoleId);

                if (r == null)
                    w.Write(
                        "<a href=\"javascript:void(0);\"  class='marginL' id=\"btnLinkNew_{0}\" onclick=\"cmdasync('cmd','NewPositionRole','value','{0}');\" title=\"{1}\"><img border=\"0\" src=\"/styles/new.gif\"/></a>",
                        p0.RoleId, "добавить");

                w.Write("</div>");

                var listPerson =
                    Dir.PositionRoles.Where(x => x.RoleId == p0.RoleId).OrderBy(o => o.PersonName).ToList();

                listPerson.ForEach(delegate(PositionRole p1)
                {
                    w.Write("<div class='marginL disp_inlineBlock'>");

                    var p = Dir.PositionRoles.Where(x => x.RoleId == p1.RoleId && x.PersonId != p1.PersonId).ToList();
                    if (p.Count > 0)
                    {
                        w.Write(
                            "<a href=\"javascript:void(0);\" id=\"btnLinkRemove_{0}\" onclick=\"{1}\" title=\"{2}\"><img border=\"0\" src=\"/styles/delete.gif\"/></a>",
                            p1.GuidId,
                            ShowConfirmDeleteGetJS(string.Format("directions_deleteRoleCallBack('{0}')", p0.GuidId)),
                            "удалить");
                    }
                    else w.Write("&nbsp;");

                    if (p1.PersonId == 0)
                        w.Write("<во всех компаниях>");
                    else
                    {
                        w.Write(
                            "<a href=\"javascript:void(0);\"  id=\"btnLinkEdit_{0}\" onclick=\"cmdasync('cmd','EditPositionRole','value','{0}');\" title=\"{1}\"><img border=\"0\" src=\"/styles/edit.gif\"/></a>",
                            p1.GuidId, "редактировать");
                        w.Write("&nbsp;");
                        w.Write("<span>");
                        RenderLinkPerson(w, "p_" + p0.RoleId + "_" + p1.PersonId, p1.PersonId.ToString(), p1.PersonName);
                        w.Write(";");
                        w.Write("</span>");
                    }
                    w.Write("</div>");
                });

                RenderNtf(w, new List<string>{p0.RoleObject.Description}, NtfStatus.Information);

                w.Write("<hr/>");
            });
        }

        private void FillPositionRolesByEmployee(Employee empl)
        {
            var roles = empl.Roles;
            roles.ForEach(delegate(EmployeeRole r)
            {
                var r0 = Dir.PositionRoles.FirstOrDefault(x => x.RoleId.ToString() == r.Id);
                if (r0 == null)
                {
                    var p = new PositionRole
                    {
                        DocumentId = Dir.DocId,
                        RoleId = r.RoleId,
                        PersonId = r.PersonId,
                        PersonName = r.PersonName
                    };


                    Dir.PositionRoles.Add(p);
                }
            });
            RefreshPositionRoles();
        }

        #endregion

        #region Types

        private void EditPositionType(string value)
        {
            if (Dir.SotrudnikField.ValueString.Length == 0)
            {
                ShowMessage("Укажите сотрудника!", efSotrudnik);
                return;
            }

            var p =
                Dir.PositionTypes.FirstOrDefault(
                    x =>
                        (x.CatalogId.HasValue ? x.CatalogId.Value.ToString() : efPTypes_Catalog.CustomRecordId) == value);
            if (p == null)
            {
                ShowMessage("Запись не найдена!", "btnLinkTypesAdd");
                ReloadPositionTypes(true);
                return;
            }

            efPTypes_Catalog.Value = value;
            FillPositionThemeItems(value);
            RefreshSotrudnikLink("efTypeEmpl", "divPTypes_Employee");
            JS.Write("directions_setAttribute('{0}', '{1}', '{2}');", "divPositionTypesAdd", "data-id", value);
            JS.Write("$('#btnPTypes_Delete').show();");
            JS.Write("directions_SetPositionTypesAdd();");
        }


        private void DeletePositionByCatalog(string catalog, string closeForm)
        {
            if (Dir.IsNew)
            {
                Dir.PositionTypes.RemoveAll(x => (x.CatalogId.HasValue ? x.CatalogId.Value.ToString() : efPTypes_Catalog.CustomRecordId) == catalog);
                RefreshPositionTypes();
            }
            else
            {
                var list = Dir.PositionTypes.Where(x =>(x.CatalogId.HasValue ? x.CatalogId.Value.ToString() : efPTypes_Catalog.CustomRecordId) == catalog).ToList();

                list.ForEach(delegate(PositionType p) { p.Delete(false); });
                ReloadPositionTypes(true);
            }

            if (!string.IsNullOrEmpty(closeForm))
                JS.Write("directions_ClosePositionTypesAdd();");
        }

        private void DeletePositionType(string catalog, string theme, string closeForm)
        {
            if (Dir.IsNew)
            {
                Dir.PositionTypes.RemoveAll(x =>
                    (x.CatalogId.HasValue ? x.CatalogId.Value.ToString() : efPTypes_Catalog.CustomRecordId) == catalog &&
                    (x.ThemeId.HasValue ? x.ThemeId.Value.ToString() : efPTypes_Type.CustomRecordId) == theme);
                RefreshPositionTypes();
            }
            else
            {
                var list = Dir.PositionTypes.Where(
                    x =>(x.CatalogId.HasValue ? x.CatalogId.Value.ToString() : efPTypes_Catalog.CustomRecordId) == catalog &&
                        (x.ThemeId.HasValue ? x.ThemeId.Value.ToString() : efPTypes_Type.CustomRecordId) == theme).ToList();

                list.ForEach(delegate(PositionType p) { p.Delete(false); });
                ReloadPositionTypes(true);
            }

            if (!string.IsNullOrEmpty(closeForm))
                JS.Write("directions_ClosePositionTypesAdd();");
        }

        private void AddPositionTypes()
        {
            if (Dir.SotrudnikField.ValueString.Length == 0)
            {
                ShowMessage("Укажите сотрудника!", efSotrudnik);
                return;
            }

            efPTypes_Catalog.Value = "";
            efPTypes_Type.ClearSelectedItems();

            RefreshSotrudnikLink("efTypeEmpl", "divPTypes_Employee");
            JS.Write("directions_SetPositionTypesAdd();");
            JS.Write("$('#btnPTypes_Delete').hide();");
        }

        private void SavePositionType(string check)
        {
            if (check.Equals("1"))
            {
                if (efPTypes_Catalog.Value.Length == 0)
                {
                    ShowMessage("Сохранение невозможно!" + Environment.NewLine + "Необходимо указать каталог.",
                        "Сообщение", MessageStatus.Warning, efPTypes_Catalog.HtmlID);
                    return;
                }

                if (efPTypes_Type.SelectedItems.Where(x => x.Id != efPTypes_Type.CustomRecordId).ToList().Count == 0 &&
                    (efPTypes_Catalog.Value.Length == 0 ||
                     efPTypes_Catalog.Value.Equals(efPTypes_Catalog.CustomRecordId)))
                {
                    ShowMessage(
                        "Сохранение невозможно!" + Environment.NewLine + "Необходимо указать или каталог или тип лица.",
                        "Сообщение", MessageStatus.Warning, efPTypes_Catalog.HtmlID);
                    return;
                }

                if (efPTypes_Catalog.Value.Length == 0)
                {
                    var message = "Вы уверены, что хотите, чтобы данный сотрудник" + Environment.NewLine +
                                  "получил доступ во всех каталогах," + Environment.NewLine +
                                  "которые существуют сейчас или появятся в будущем, к указанному типу лица?";
                    const string callbackYes = "cmdasync('cmd','SavePositionType', 'check',0);";
                    ShowConfirm(message, "Сообщение", "Да", "Нет", callbackYes, "", 400);
                    return;
                }

                if (efPTypes_Type.SelectedItems.Where(x => x.Id != efPTypes_Type.CustomRecordId).ToList().Count == 0)
                {
                    var message = "Вы уверены, что хотите, чтобы данный сотрудник" + Environment.NewLine +
                                  "получил доступ ко всем типам лиц в указанном каталоге," + Environment.NewLine +
                                  "которые существуют сейчас или появятся в будущем?";
                    const string callbackYes = "cmdasync('cmd','SavePositionType', 'check',0);";
                    ShowConfirm(message, "Сообщение", "Да", "Нет", callbackYes, "", 400);

                    return;
                }
            }

            var isNew = Dir.IsNew;
            if (isNew)
            {
                var result = SaveDocument(false);
                if (!result)
                {
                    JS.Write("directions_ClosePositionTypesAdd('btnLinkTypesAdd');");
                    return;
                }
            }

            var ps =
                Dir.PositionTypes.Where(
                    x =>
                        (x.CatalogId.HasValue ? x.CatalogId.Value.ToString() : efPTypes_Catalog.CustomRecordId) ==
                        (efPTypes_Catalog.Value.Length == 0 ? efPTypes_Catalog.CustomRecordId : efPTypes_Catalog.Value))
                    .ToList();

            //если такого каталога еще не было
            if (ps.Count == 0)
            {
                //проверяем, указаны ли реальные темы, если нет
                if (efPTypes_Type.SelectedItems.Where(x => x.Id != efPTypes_Type.CustomRecordId).ToList().Count == 0)
                {
                    //если все типы для конкретного каталога
                    var p = new PositionType
                    {
                        DocumentId = Dir.DocId,
                        CatalogId = efPTypes_Catalog.ValueInt,
                        ThemeId = null
                    };
                    p.Save(false);
                }
                else
                {
                    //Сохраняем каждую тему в указанном каталоге
                    efPTypes_Type.SelectedItems.ForEach(delegate(Lib.Entities.Item item)
                    {
                        var p = new PositionType
                        {
                            DocumentId = Dir.DocId,
                            CatalogId = efPTypes_Catalog.ValueInt,
                            ThemeId = item.Id.ToNullableInt()
                        };
                        p.Save(false);
                    });
                }
            }
            else
            {
                //удаляем из позиций записи, которых нет в списке
                Dir.PositionTypes.ForEach(delegate(PositionType p0)
                {
                    var r =
                        efPTypes_Type.SelectedItems.FirstOrDefault(
                            x =>
                                x.Id ==
                                (p0.ThemeId.HasValue ? p0.ThemeId.Value.ToString() : efPTypes_Type.CustomRecordId));

                    //если не нашли тему в каталоге, то удаляем
                    if (r.Equals(default(Lib.Entities.Item)) &&
                        (efPTypes_Catalog.Value.Length == 0 ? efPTypes_Catalog.CustomRecordId : efPTypes_Catalog.Value) ==
                        (p0.CatalogId.HasValue ? p0.CatalogId.Value.ToString() : efPTypes_Catalog.CustomRecordId))
                    {
                        if (!string.IsNullOrEmpty(p0.Id))
                            p0.Delete(false);
                        else
                            Dir.PositionTypes.RemoveAll(x => x.GuidId == p0.GuidId);
                    }
                });

                //если список типов пуст, добавляем custom
                if (efPTypes_Type.SelectedItems.Count == 0)
                    efPTypes_Type.SelectedItems.Add(new Lib.Entities.Item
                    {
                        Id = efPTypes_Type.CustomRecordId,
                        Value = efPTypes_Type.CustomRecordText
                    });

                //добавляем в позиции новые записи
                efPTypes_Type.SelectedItems.ForEach(delegate(Lib.Entities.Item item)
                {
                    var r =
                        Dir.PositionTypes.FirstOrDefault(
                            x =>
                                (x.CatalogId.HasValue ? x.CatalogId.Value.ToString() : efPTypes_Catalog.CustomRecordId) ==
                                (efPTypes_Catalog.Value.Length == 0
                                    ? efPTypes_Catalog.CustomRecordId
                                    : efPTypes_Catalog.Value)
                                &&
                                (x.ThemeId.HasValue ? x.ThemeId.Value.ToString() : efPTypes_Type.CustomRecordId) ==
                                item.Id
                            );
                    //если запись не найдена
                    if (r == null)
                    {
                        var p = new PositionType
                        {
                            DocumentId = Dir.DocId,
                            CatalogId = efPTypes_Catalog.Value.Length == 0 ? null : efPTypes_Catalog.ValueInt,
                            ThemeId = item.Id.Equals(efPTypes_Type.CustomRecordId) ? null : item.Id.ToNullableInt()
                        };

                        p.Save(false);
                    }
                });
            }
           
            if (isNew) return;
            JS.Write("directions_ClosePositionTypesAdd('btnLinkTypesAdd');");
            ReloadPositionTypes();
        }

        private void RefreshPositionTypes()
        {
            var w = new StringWriter();
            RenderPositionTypes(w);
            RefreshControlText(w, "divPositionTypesData");
        }

        private void RenderPositionTypes(TextWriter w)
        {
            if (Dir.PositionTypes.Count == 0)
            {
                w.Write("");
                return;
            }

            var listTypes = Dir.PositionTypes
                .GroupBy(p => p.CatalogId)
                .Select(g => g.First()).OrderBy(o => o.CatalogName).ToList().OrderBy(x => x.CatalogName).ToList();

            listTypes.ForEach(delegate(PositionType p0)
            {
                var catalog0 = (p0.CatalogId.HasValue
                    ? p0.CatalogId.Value.ToString()
                    : efPTypes_Catalog.CustomRecordId);

                w.Write("<div class='marginL disp_inlineBlock'>");
                w.Write(
                    "<a href=\"javascript:void(0);\"  id=\"btnLinkTEdit_{0}\" onclick=\"cmdasync('cmd','EditPositionType','value','{0}');\" title=\"{1}\"><img border=\"0\" src=\"/styles/edit.gif\"/></a>",
                    catalog0,
                    "редактировать");

                w.Write(
                    "<a href=\"javascript:void(0);\"  class='marginL' id=\"btnLinkTDel_{0}\" onclick=\"{1}\" title=\"{2}\"><img border=\"0\" src=\"/styles/delete.gif\"/></a>",
                    catalog0,
                    ShowConfirmDeleteGetJS(string.Format("directions_deleteTypeAllCallBack('{0}')", catalog0)),
                    "удалить"
                    );

                w.Write("{0}:", string.IsNullOrEmpty(p0.CatalogName) ? "<все каталоги>" : p0.CatalogName);
                w.Write("</div>");

                var listTheme =
                    Dir.PositionTypes.Where(x => x.CatalogId.Equals(p0.CatalogId)).OrderBy(o => o.ThemeName).ToList();

                listTheme.ForEach(delegate(PositionType p1)
                {
                    var catalog1 = (p1.CatalogId.HasValue
                        ? p1.CatalogId.Value.ToString()
                        : efPTypes_Catalog.CustomRecordId);

                    var theme1 = (p1.ThemeId.HasValue ? p1.ThemeId.Value.ToString() : efPTypes_Type.CustomRecordId);

                    var p =
                        Dir.PositionTypes.Where(
                            x =>
                                (x.CatalogId.HasValue ? x.CatalogId.Value.ToString() : efPTypes_Catalog.CustomRecordId) ==
                                catalog1 &&
                                (x.ThemeId.HasValue ? x.ThemeId.Value.ToString() : efPTypes_Type.CustomRecordId) !=
                                theme1).ToList();

                    w.Write("<div class='marginL disp_inlineBlock'>");
                    if (p.Count > 0)
                    {
                        w.Write(
                            "<a href=\"javascript:void(0);\" id=\"btnLinkTDel_{0}\" onclick=\"{1}\" title=\"{2}\"><img border=\"0\" src=\"/styles/delete.gif\"/></a>",
                            catalog1 + "_" + theme1,
                            ShowConfirmDeleteGetJS(string.Format("directions_deleteTypeCallBack('{0}','{1}')", catalog1,
                                theme1)),
                            "удалить"
                            );
                    }
                    w.Write("{0};", string.IsNullOrEmpty(p1.ThemeName) ? "<все типы лиц>" : p1.ThemeName);
                    w.Write("</div>");
                });

                w.Write("<hr/>");
            });
        }

        private void ReloadPositionTypes(bool setFocus = false)
        {
            Dir.LoadPositionTypes();
            RefreshPositionTypes();
            if (setFocus)
                JS.Write("directions_setElementFocus(null,'btnLinkTypesAdd');");
        }

        private void FillPositionThemeItems(string catalog)
        {
            efPTypes_Type.SelectedItems.Clear();

            if (!string.IsNullOrEmpty(catalog))
            {
                Dir.PositionTypes.Where(
                    x =>
                        (!x.CatalogId.HasValue ? efPTypes_Type.CustomRecordId : x.CatalogId.Value.ToString()) == catalog)
                    .ToList()
                    .ForEach(
                        delegate(PositionType p)
                        {
                            var pThId = (p.ThemeId.HasValue ? p.ThemeId.ToString() : efPTypes_Type.CustomRecordId);
                            var pThName = (p.ThemeId.HasValue ? p.ThemeName : efPTypes_Type.CustomRecordText);
                            efPTypes_Type.SelectedItems.Add(new Lib.Entities.Item
                            {
                                Id = pThId,
                                Value = new Lib.Entities.Item {Id = pThId, Value = pThName}
                            });
                        });
            }
            efPTypes_Type.RefreshDataBlock();
        }

        private void FillPositionTypesByEmployee(Employee empl)
        {
            var types = empl.Types;
            types.ForEach(delegate(EmployeePersonType t)
            {
                var t0 =
                    Dir.PositionTypes.FirstOrDefault(
                        x =>
                            ((x.CatalogId.HasValue && t.CatalogId.HasValue && x.CatalogId.Value == t.CatalogId.Value) ||
                             (!x.CatalogId.HasValue && !t.CatalogId.HasValue)) &&
                            ((x.ThemeId.HasValue && t.ThemeId.HasValue && x.ThemeId.Value == t.ThemeId.Value) ||
                             (!x.ThemeId.HasValue && !t.ThemeId.HasValue)));
                if (t0 == null)
                {
                    var p = new PositionType
                    {
                        DocumentId = Dir.DocId,
                        CatalogId = t.CatalogId,
                        CatalogName = t.CatalogName,
                        ThemeId = t.ThemeId,
                        ThemeName = t.ThemeName
                    };


                    Dir.PositionTypes.Add(p);
                }
            });
            RefreshPositionTypes();
        }

        #endregion

        #region AdvancedGrants

        private void AddPositionAdvancedGrants()
        {
            if (Dir.SotrudnikField.ValueString.Length == 0)
            {
                ShowMessage("Укажите сотрудника!", efSotrudnik);
                return;
            }

            RefreshPositionDialogAdvancedGrants();
            JS.Write("directions_SetPositionAGAdd();");
        }

        private void RefreshPositionDialogAdvancedGrants()
        {
            var w = new StringWriter();

            Dir.AdvancedGrants().ForEach(delegate(AdvancedGrant ag)
            {
                var check = false;

                if (Dir.PositionAdvancedGrants != null)
                {
                    var p = Dir.PositionAdvancedGrants.FirstOrDefault(x => x.GrantId.ToString() == ag.Id);
                    check = p != null;
                }
                w.Write("<div class='marginL div_ag'>");
                w.Write("<input id='ag_{0}' data-id='{0}' data-name='{1}' data-name-en='{2}' class='AG' type='checkbox' {3}>", 
                    ag.Id,
                    HttpUtility.HtmlEncode(ag.Name),
                    HttpUtility.HtmlEncode(ag.NameEn),
                    check ? "checked" : "");
                w.Write("<div class='disp_inlineBlock marginL'>");
                if (IsRusLocal) w.Write(ag.Name);
                else w.Write(ag.NameEn);
                w.Write("</div>");
                w.Write("</div>");
            }
                );

            RefreshControlText(w, "divAG_Body");
        }

        private void DeletePositionAG(string value)
        {
            var p = Dir.PositionAdvancedGrants.FirstOrDefault(x => x.Id == value);
            if (p != null)
                p.Delete(false);
            Dir.LoadPositionAdvancedGrants();
            RefreshPositionAdvancedGrants();
            if (Dir.PositionAdvancedGrants.Count == 0)
                JS.Write("directions_setElementFocus(null,'efAdvInfo_0');");
            else
                JS.Write("directions_setElementFocus('DelAG');");
        }

        private void SavePositionAG(string jsonValues)
        {
            var isNew = Dir.IsNew;
            if (isNew)
            {
                var result = SaveDocument(false);
                if (!result) return;
            }

            var serializer = new JavaScriptSerializer();
            var values = serializer.Deserialize<Dictionary<string, string>>(jsonValues);
            Dir.SavePositionsAdvancedGrantsByDictionary(values, !isNew);
            
            if (isNew) return;
            RefreshPositionAdvancedGrants();
        }

        private void RefreshPositionAdvancedGrants()
        {
            var w = new StringWriter();
            RenderPositionAdvancedGrants(w);
            RefreshControlText(w, "divAdvancedGrantsData");
        }

        private void RenderPositionAdvancedGrants(TextWriter w)
        {
            w.Write("");
            var sortedList = Dir.PositionAdvancedGrants.OrderBy(o => o.GrantDescription).ToList();
            sortedList.ForEach(delegate(PositionAdvancedGrant p)
            {
                w.Write("<div class='marginL disp_inlineBlock'>");
                w.Write(
                    "<a href='javascript:void(0);' class='DelAG' id='btnDelAG_{0}' onclick=\"{1}\" title='{2}'>",
                    p.Id,
                    ShowConfirmDeleteGetJS(string.Format("directions_deleteAGCallBack('{0}')", p.Id)),
                    "удалить"
                    );
                w.Write("<img src='/styles/delete.gif' class= border=0>");
                w.Write("</a>");
                w.Write("{0};", IsRusLocal ? p.GrantDescription : p.GrantDescriptionEn);
                w.Write("</div>");
            });
        }
        
        #endregion
       

        #endregion

        #region Adv functions

        private void SetRequiredMailName()
        {
            var accessEthernet = (Dir.AccessEthernetField.ValueString.Length == 0) ? 0 : Dir.AccessEthernetField.ValueInt;

            if (accessEthernet==1)
                efMailName.IsRequired = Dir.MailNameField.ValueString.Length == 0;
            else
                efMailName.IsRequired = false;
        }
        private void SetRequiredLogin()
        {
            var accessEthernet = (Dir.AccessEthernetField.ValueString.Length == 0) ? 0 : Dir.AccessEthernetField.ValueInt;

            if (accessEthernet == 1)
                efLogin.IsRequired = Dir.LoginField.ValueString.Length == 0;
            else
                efLogin.IsRequired = false;
        }

        

        private void SetWorkPlaceType(DirectionsWorkPlaceTypeBitEnum bit, string whatdo)
        {
            var bitMask = Dir.WorkPlaceTypeField.ValueInt;

            if (whatdo.Equals("0"))
                bitMask ^= (int) bit;
            else
                bitMask |= (int) bit;

            if (bit == DirectionsWorkPlaceTypeBitEnum.РабочееМестоВОфисе &&
                (bitMask & (int) DirectionsWorkPlaceTypeBitEnum.РабочееМестоВнеОфиса) ==
                (int) DirectionsWorkPlaceTypeBitEnum.РабочееМестоВнеОфиса)
                bitMask ^= (int) DirectionsWorkPlaceTypeBitEnum.РабочееМестоВнеОфиса;
            if (bit == DirectionsWorkPlaceTypeBitEnum.РабочееМестоВнеОфиса &&
                (bitMask & (int) DirectionsWorkPlaceTypeBitEnum.РабочееМестоВОфисе) ==
                (int) DirectionsWorkPlaceTypeBitEnum.РабочееМестоВОфисе)
                bitMask ^= (int) DirectionsWorkPlaceTypeBitEnum.РабочееМестоВОфисе;

            Dir.WorkPlaceTypeField.ValueString = (bitMask == 0) ? "" : bitMask.ToString();
            GetWorkPlaceType();

            //Убираем DECT
            if (Dir.WorkPlaceTypeField.ValueString.Length == 0 ||
                Dir.WorkPlaceTypeField.ValueString.Length > 0 && (Dir.WorkPlaceTypeField.ValueInt & 1) != 1)
            {
                if (Dir.PhoneEquipField.ValueString.Length > 0 &&
                    (Dir.PhoneEquipField.ValueInt & 2) == 2)
                    SetPhoneEquip(2, "0");
            }
        }
        private void GetWorkPlaceType()
        {
            var bitMask = Dir.WorkPlaceTypeField.ValueInt;

            if ((bitMask & 1) == 1) efWorkPlaceType1.Value = "1";
            else efWorkPlaceType1.Value = "0";

            if ((bitMask & 2) == 2) efWorkPlaceType2.Value = "1";
            else efWorkPlaceType2.Value = "0";

            if ((bitMask & 4) == 4) efWorkPlaceType4.Value = "1";
            else efWorkPlaceType4.Value = "0";
        }
        

        private void DisplayDataWorkPlace()
        {
            JS.Write("directions_Data_WP({0}, {1});",
                     Dir.WorkPlaceTypeField.ValueInt, Dir.AccessEthernetField.ValueInt);
        }
        private void DisplayDataPhoneDesk(bool setIp)
        {
            if (setIp) efPhoneIPCam.IsDisabled = (Dir.PhoneEquipField.ValueInt & 1) != 1;
            DisplayPLExit();
            DisplayMobilPhone();
        }
        private void DisplayDataSim()
        {
            efAccessInternetGPRS.IsDisabled = (Dir.PhoneEquipField.ValueInt & 16) != 16;
        }
        private void DisplayDataEthernet()
        {
            JS.Write("directions_Data_Lan({0});", Dir.AccessEthernetField.ValueInt);
        }


        private void SetPhoneEquip(int bit, string whatdo)
        {
            var bitMask = Dir.PhoneEquipField.ValueInt;

            if (whatdo.Equals("0"))
                bitMask ^= bit;
            else
                bitMask |= bit;

            if ((bitMask & 8) == 8 && (bitMask & 1) != 1) bitMask ^= 8;
            if ((bitMask & 32) == 32 && (bitMask & 16) != 16) bitMask ^= 32;

            Dir.PhoneEquipField.ValueString = (bitMask == 0) ? "" : bitMask.ToString();
            GetPhoneEquip();
        }
        private void GetPhoneEquip()
        {
            var bitMask = Dir.PhoneEquipField.ValueInt;

            if ((bitMask & 1) == 1) efPhoneDesk.Value = "1";
            else efPhoneDesk.Value = "0";

            if ((bitMask & 2) == 2) efPhoneDect.Value = "1";
            else efPhoneDect.Value = "0";

            if ((bitMask & 8) == 8) efPhoneIPCam.Value = "1";
            else efPhoneIPCam.Value = "0";

            if ((bitMask & 16) == 16) efPhoneSim.Value = "1";
            else efPhoneSim.Value = "0";

            if ((bitMask & 32) == 32) efAccessInternetGPRS.Value = "1";
            else efAccessInternetGPRS.Value = "0";
        }
        private void DisplayPLExit()
        {
            var display = "none";
            var bit = Dir.PhoneEquipField.ValueInt;
            if ((bit & 1) == 1 || (bit & 2) == 2) display = "inline-table";

            JS.Write("var objDiv = document.getElementById('divPLExit'); if (objDiv) objDiv.style.display ='{0}';",
                display);
        }
        private void DisplayMobilPhone()
        {
            var display = 0;
            var bit = Dir.PhoneEquipField.ValueInt;
            if ((bit & 1) == 1 || (bit & 2) == 2) display = 1;

            JS.Write("directions_Data_MobilPhone({0});", display);
        }


        private void SetCompType(int bit, string whatdo)
        {
            var bitMask = Dir.CompTypeField.ValueInt;

            if (whatdo.Equals("0"))
                bitMask ^= bit;
            else
                bitMask |= bit;

            Dir.CompTypeField.ValueString = (bitMask == 0) ? "" : bitMask.ToString();
            GetCompType();
        }
        private void GetCompType()
        {
            var bitMask = Dir.CompTypeField.ValueInt;

            if ((bitMask & 2) == 2) efComputer.Value = "1";
            else efComputer.Value = "0";

            if ((bitMask & 4) == 4) efNotebook.Value = "1";
            else efNotebook.Value = "0";
        }


        private void AdvSearchWorkPlace()
        {
            var parameters = "WorkPlace=1&hidemenu=1";
            ReturnDialogResult.ShowAdvancedDialogSearch(this, "directions_AdvSearchWorkPlace", "",
                Config.location_search, parameters, false, 0, 800, 600);
        }
        private void SetWorkPlace(string value, string label)
        {
            if (value.Length == 0) return;
            Dir.WorkPlaceField.Value = value;
            RefreshWorkPlace(label);
        }
        private void ClearWorkPlace()
        {
            Dir.WorkPlaceField.Value = "";
            RefreshWorkPlace("");
        }


        private void SetDefaultDomainByEmployee()
        {
            if (Dir.SotrudnikField.ValueString.Length == 0)
            {
                Dir.DomainField.ValueString = Config.domain;
                return;
            }

            var colD = new StringCollection();
            var empl = Dir.Sotrudnik;
            if (empl.Unavailable) return;

            var _p = empl.OrganizationId.ToString();
            if (_p.Length == 0) return;

            StringCollection colPersons = null;

            Dir.DomainNames().ForEach(delegate(DomainName dName)
            {
                if (string.IsNullOrEmpty(dName.PersonIds)) return;
                colPersons = Convert.Str2Collection(dName.PersonIds);
                foreach (var t in from string t in colPersons where t.Equals(_p) select t)
                {
                    colD.Add(dName.Id);
                }
            });


            if (Dir.DomainField.ValueString.Length > 0 && colD.Count > 0 && !colD.Contains(Dir.DomainField.ValueString))
            {
                Dir.DomainField.Value = colD[0];
                return;
            }

            if (Dir.DomainField.ValueString.Length == 0 && colD.Count > 0)
            {
                Dir.DomainField.Value = colD[0];
                return;
            }


            if (Dir.DomainField.ValueString.Length == 0 && colD.Count == 0)
            {
                Dir.DomainField.Value = Config.domain;
            }
        }
        private void RenderMailNamesList(object x, object y)
        {
            var w = new StringWriter();

            if (Dir.SotrudnikField.ValueString.Length == 0)
            {
                ShowMessage("Укажите сотрудника!", efSotrudnik);
                return;
            }

            var fl = false;
            var empl = Dir.Sotrudnik;
            if (!empl.Unavailable)
            {
                fl = true;
                var name = empl.Login;
                if (empl.Login.Length > 0)
                {
                    w.Write("<div noWrap>");
                    w.Write("<nobr>");
                    w.Write(
                        "<a id='imgMN_0' tabindex=0 onkeydown='var key=v4_getKeyCode(event); if(key == 13 || key == 32) directions_SetMailName(\"{0}\");' onclick='directions_SetMailName(\"{0}\");'><img src='/styles/backtolist.gif' border='0'/></a>",
                        HttpUtility.JavaScriptStringEncode(name));
                    w.Write("<span class='marginL'>");
                    w.Write(HttpUtility.HtmlEncode(name));
                    w.Write("</span>");
                    w.Write("</nobr>");
                    w.Write("</div>");
                }


                if (empl.FirstNameEn.Length > 0 && empl.LastNameEn.Length > 0)
                {
                    name = empl.FirstNameEn + "." + empl.LastNameEn;
                    w.Write("<div noWrap>");
                    w.Write("<nobr>");
                    w.Write(
                        "<a id='imgMN_{1}' tabindex=0 onkeydown='var key=v4_getKeyCode(event); if(key == 13 || key == 32) directions_SetMailName(\"{0}\");' onclick='directions_SetMailName(\"{0}\");'><img src='/styles/backtolist.gif' border='0'/></a>",
                        HttpUtility.JavaScriptStringEncode(name), string.IsNullOrEmpty(empl.Login) ? 0 : 1);
                    w.Write("<span class='marginL'>");
                    w.Write(HttpUtility.HtmlEncode(name));
                    w.Write("</span>");
                    w.Write("</nobr>");
                    w.Write("</div>");
                }
            }


            if (!fl)
                RenderNtf(w, new List<string>{"Ошибка получения возможных имен почтового ящика"}, NtfStatus.Error);

            RefreshControlText(w, "divMN_Body");
            JS.Write("directions_SetMailNamesList();");
        }
        private void SetMailName(string value, string label)
        {
            if (value.Length == 0) return;
            Dir.MailNameField.Value = value;
            efMailName.RenderNtf();
        }
        

        private void SetSotrudnikParentType(DirectionsSotrudnikParentBitEnum bit, string whatdo)
        {
            var bitMask = Dir.SotrudnikParentCheckField.ValueInt;

            if (whatdo.Equals("0"))
                bitMask ^= (int)bit;
            else
                bitMask |= (int)bit;

            if (bit == DirectionsSotrudnikParentBitEnum.КакУСотрудника &&
                (bitMask & (int)DirectionsSotrudnikParentBitEnum.ВместоСотрудника) ==
                (int)DirectionsSotrudnikParentBitEnum.ВместоСотрудника)
                bitMask ^= (int)DirectionsSotrudnikParentBitEnum.ВместоСотрудника;

            if (bit == DirectionsSotrudnikParentBitEnum.ВместоСотрудника &&
                (bitMask & (int)DirectionsSotrudnikParentBitEnum.КакУСотрудника) ==
                (int)DirectionsSotrudnikParentBitEnum.КакУСотрудника)
                bitMask ^= (int)DirectionsSotrudnikParentBitEnum.КакУСотрудника;

            Dir.SotrudnikParentCheckField.ValueString = (bitMask == 0) ? "" : bitMask.ToString();
            GetSotrudnikParentType();
            efSotrudnikParent.RenderNtf();
           
        }
        private void GetSotrudnikParentType()
        {
            var bitMask = Dir.SotrudnikParentCheckField.ValueInt;

            if ((bitMask & 1) == 1) efSotrudnikParentCheck1.Value = "1";
            else efSotrudnikParentCheck1.Value = "0";

            if ((bitMask & 2) == 2) efSotrudnikParentCheck2.Value = "1";
            else efSotrudnikParentCheck2.Value = "0";

            if (bitMask > 0)
            {
                efSotrudnikParent.IsRequired = Dir.SotrudnikParentField.ValueString.Length <= 0;
                JS.Write("$('#divSotrudnikParent').show();");
            }
            else
                JS.Write("$('#divSotrudnikParent').hide();");


        }


        private void RefreshSotrudnikLink(string linkId, string containerId)
        {
            var w = new StringWriter();
            RenderLinkEmployee(w, linkId, Dir.Sotrudnik, NtfStatus.Empty);
            RefreshControlText(w, containerId);
        }

        #endregion

        private void SotrudnikClearInfo()
        {
            Dir.SotrudnikPostField.Value = null;
            Dir.SupervisorField.Value = null;
            Dir.RedirectNumField.Value = null;
            Dir.WorkPlaceField.Value = null;
            Dir.LoginField.Value = null;
            Dir.MailNameField.Value = null;
            Dir.SotrudnikLanguageField.Value = null;
            
        }

        private void SotrudnikSetInfo()
        {
            var empl = Dir.Sotrudnik;

            if (Dir.SotrudnikField.ValueString.Length == 0 || empl == null || empl.Unavailable)
            {
                Dir.Description = "";
                return;
            }

            Dir.LoginField.Value = empl.Login;
            Dir.SotrudnikLanguageField.Value = empl.Language;

            if (empl.Email.Length > 0)
            {
                var inx = empl.Email.IndexOf("@", StringComparison.Ordinal);
                Dir.MailNameField.Value = empl.Email.Substring(0, inx);
                Dir.DomainField.Value = empl.Email.Substring(inx + 1).ToLower();
            }

            if (Dir.DomainField.ValueString.Length == 0)
                SetDefaultDomainByEmployee();

            if (empl.DisplayName.Length > 0)
                Dir.DisplayNameField.Value = empl.DisplayName;

            Dir.Description = empl.FIO;

            FillPositionCommonFoldersByEmployee(empl);
            SetWorkPlaceType(DirectionsWorkPlaceTypeBitEnum.ДоступЧерезVPN, empl.IsVPNGroup ? "1" : "0");

            Dir.AccessEthernetField.ValueString = (empl.Login.Length > 0 || empl.IsVPNGroup || empl.IsInternetGroup)
                ? "1"
                : "0";

            DisplayDataWorkPlace();
            DisplayDataEthernet();
            
            FillPositionRolesByEmployee(empl);
            FillPositionTypesByEmployee(empl);

        }

        private void RefreshSotrudnikInfo()
        {
            RefreshPhoto();
            RefreshSotrudnikAOrg();
            RefreshSotrudnikFinOrg();

            RefreshSotrudnikPost();
            RefreshSupervisorInfo();

            RefreshMobilphoneArea();

            SotrudnikSetInfo();

            efLogin.RenderNtf();
            efMailName.RenderNtf();
            
        }

        private void SotrudnikParentSetInfo()
        {
            if (Dir.SotrudnikParentField.ValueString.Length == 0) return;
            ClearDocumentPositions();
            FillPositionCommonFoldersByEmployee(Dir.SotrudnikParent);
            FillPositionRolesByEmployee(Dir.SotrudnikParent);
            FillPositionTypesByEmployee(Dir.SotrudnikParent);
        }


        private void ClearDocumentPositions()
        {
            Dir.PositionTypes.Clear();
            Dir.PositionRoles.Clear();
            Dir.PositionCommonFolders.Clear();
        }
    }
}