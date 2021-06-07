using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Script.Serialization;
using Kesco.Lib.BaseExtention;
using Kesco.Lib.BaseExtention.Enums;
using Kesco.Lib.BaseExtention.Enums.Controls;
using Kesco.Lib.BaseExtention.Enums.Corporate;
using Kesco.Lib.BaseExtention.Enums.Docs;
using Kesco.Lib.DALC;
using Kesco.Lib.Entities;
using Kesco.Lib.Entities.Corporate;
using Kesco.Lib.Entities.Corporate.Phones;
using Kesco.Lib.Entities.Documents;
using Kesco.Lib.Entities.Documents.EF.Directions;
using Kesco.Lib.Entities.Persons;
using Kesco.Lib.Entities.Persons.Contacts;
using Kesco.Lib.Web.Controls.V4;
using Kesco.Lib.Web.Controls.V4.Common;
using Kesco.Lib.Web.Controls.V4.Common.DocumentPage;
using Kesco.Lib.Web.Settings;
using Convert = Kesco.Lib.ConvertExtention.Convert;
using Item = Kesco.Lib.Web.Controls.V4.Item;
using Role = Kesco.Lib.Entities.Corporate.Role;

namespace Kesco.App.Web.Docs.Directions
{
    /// <inheritdoc />
    /// <summary>
    ///     Подключаемый класс формы документа: Указание IT на организацию работы
    /// </summary>
    public abstract partial class DirectionIT : DocPage
    {
        private bool isRedirect = false;

        /// <summary>
        ///     Группа посменной работы без логина
        /// </summary>
        private bool _isCommonEmployeeNoLogin;

        /// <summary>
        ///     Объект отрисовки дополнительной информации
        /// </summary>
        private RenderHelper _render;
        protected string DIRECTIONS_Field_Phone;
        protected string DIRECTIONS_Field_Phone_Table;
        protected string DIRECTIONS_Field_Phone_WebCam;
        protected string DIRECTIONS_Field_Phone_Dect;
        protected string DIRECTIONS_Field_MobilPhone;
        protected string DIRECTIONS_Field_Computer;
        protected string DIRECTIONS_Field_Computer_Desktop;
        protected string DIRECTIONS_Field_Computer_Laptop;
        protected string DIRECTIONS_Field_AdvEq;
        protected string DIRECTIONS_Field_AEAccess;
        protected string DIRECTIONS_Field_AEAccess_Login;
        protected string DIRECTIONS_Field_Lang;
        protected string DIRECTIONS_Field_AccessData;
        protected string DIRECTIONS_Field_AccessData_How;
        protected string DIRECTIONS_Field_AccessData_Instead;
        protected string DIRECTIONS_Field_Positions_Folders;
        protected string DIRECTIONS_Field_Positions_Folders_Title;
        protected string DIRECTIONS_Field_Positions_Roles;
        protected string DIRECTIONS_Field_Positions_Roles_Title;
        protected string DIRECTIONS_Field_Positions_Types;
        protected string DIRECTIONS_Field_Positions_Types_Title;
        protected string DIRECTIONS_Field_AdvInfo;
        protected string DIRECTIONS_Field_WpType;
        protected string cmdWorkplaceOther;
        protected string DIRECTIONS_Field_Positions_Roles_Select;
        protected string DIRECTIONS_Pos_InCompany;
        protected string DIRECTIONS_Rnd_Catalog;
        protected string DIRECTIONS_Rnd_TypePerson;

        /// <summary>
        ///     Конструктор
        /// </summary>
        protected DirectionIT()
        {
            IsRenderDocTitle = false;
        }

        protected Direction Dir => (Direction) Doc;

        /// <summary>
        ///     Инициализация объекта отрисовки информации
        /// </summary>
        protected RenderHelper RenderData => _render ?? (_render = new RenderHelper());


        //========================================================================================================== InitConrols

        #region InitControls

        /// <summary>
        ///     Инициализация элементов управления
        ///     Установака фильтров и переходов по Tab
        /// </summary>
        private void SetInitControls()
        {
            ItemName = "DirectionIT";
            efSotrudnik.Filter.Status.ValueStatus = СотоянияСотрудника.Работающие;
            efSotrudnikParent.Filter.Status.ValueStatus = СотоянияСотрудника.РаботающиеИУволенные;
            efSotrudnikParent.Filter.HasLogin.ValueHasLogin = НаличиеЛогина.ЕстьЛогин;

            rdWorkPlaceType1.NextControl = "rdWorkPlaceType3";
            rdWorkPlaceType3.NextControl = "rdWorkPlaceType5";
            rdWorkPlaceType5.NextControl = "rdWorkPlaceType4";


            efPhoneDesk.NextControl = "efPhoneIPCam";
            efPhoneIPCam.NextControl = "efPhoneDect";
            efPhoneDect.NextControl = "efComputer";
            efComputer.NextControl = "efNotebook";
            efNotebook.NextControl = "efAdvEq";
            efAdvEq.NextControl = "efAccessEthernet";

            efPRoles_Role.NextControl = "efPRoles_Person";
            efPRoles_Person.NextControl = "btnPRoles_Save";

            efPTypes_Catalog.NextControl = "efPTypes_Type";
            efPTypes_Catalog.CustomRecordText = HttpUtility.HtmlEncode(Dir.Resx.GetString("DIRECTIONS_Rnd_AllCatalog"));

            efPTypes_Type.NextControl = "efPTypes_Type_0";
            efPTypes_Type.CustomRecordText = HttpUtility.HtmlEncode(Dir.Resx.GetString("DIRECTIONS_Rnd_AllTypePerson"));

            efPRoles_Role.IsNoModifying = true; efPRoles_Role.IsShowEditingStatus = false;
            efPRoles_Person.IsNoModifying = true; efPRoles_Person.IsShowEditingStatus = false;
            efPTypes_Catalog.IsNoModifying = true; efPTypes_Catalog.IsShowEditingStatus = false;
            efPTypes_Type.IsNoModifying = true; efPTypes_Type.IsShowEditingStatus = false;


            #region Localization

            DIRECTIONS_Field_Phone  = Dir.Resx.GetString("DIRECTIONS_Field_Phone");
            DIRECTIONS_Field_Phone_Table= Dir.Resx.GetString("DIRECTIONS_Field_Phone_Table");
            DIRECTIONS_Field_Phone_WebCam = Dir.Resx.GetString("DIRECTIONS_Field_Phone_WebCam");
            DIRECTIONS_Field_Phone_Dect = Dir.Resx.GetString("DIRECTIONS_Field_Phone_Dect");            
            DIRECTIONS_Field_MobilPhone = Dir.Resx.GetString("DIRECTIONS_Field_MobilPhone");
            DIRECTIONS_Field_Computer = Dir.Resx.GetString("DIRECTIONS_Field_Computer");
            DIRECTIONS_Field_Computer_Desktop = Dir.Resx.GetString("DIRECTIONS_Field_Computer_Desktop");
            DIRECTIONS_Field_Computer_Laptop = Dir.Resx.GetString("DIRECTIONS_Field_Computer_Laptop");
            DIRECTIONS_Field_AdvEq = Dir.Resx.GetString("DIRECTIONS_Field_AdvEq");
            DIRECTIONS_Field_AEAccess = Dir.Resx.GetString("DIRECTIONS_Field_AEAccess");
            DIRECTIONS_Field_AEAccess_Login = Dir.Resx.GetString("DIRECTIONS_Field_AEAccess_Login");
            DIRECTIONS_Field_Lang = Dir.Resx.GetString("DIRECTIONS_Field_Lang");
            DIRECTIONS_Field_AccessData = Dir.Resx.GetString("DIRECTIONS_Field_AccessData");
            DIRECTIONS_Field_AccessData_How = Dir.Resx.GetString("DIRECTIONS_Field_AccessData_How");
            DIRECTIONS_Field_AccessData_Instead = Dir.Resx.GetString("DIRECTIONS_Field_AccessData_Instead");
            DIRECTIONS_Field_Positions_Folders = Dir.Resx.GetString("DIRECTIONS_Field_Positions_Folders");
            DIRECTIONS_Field_Positions_Folders_Title = Dir.Resx.GetString("DIRECTIONS_Field_Positions_Folders_Title");
            DIRECTIONS_Field_Positions_Roles = Dir.Resx.GetString("DIRECTIONS_Field_Positions_Roles");
            DIRECTIONS_Field_Positions_Roles_Title = Dir.Resx.GetString("DIRECTIONS_Field_Positions_Roles_Title");
            DIRECTIONS_Field_Positions_Types = Dir.Resx.GetString("DIRECTIONS_Field_Positions_Types");
            DIRECTIONS_Field_Positions_Types_Title = Dir.Resx.GetString("DIRECTIONS_Field_Positions_Types_Title");
            DIRECTIONS_Field_AdvInfo = Dir.Resx.GetString("DIRECTIONS_Field_AdvInfo");
            DIRECTIONS_Field_WpType= Dir.Resx.GetString("DIRECTIONS_Field_WpType");
            cmdWorkplaceOther = Dir.Resx.GetString("cmdWorkplaceOther");
            DIRECTIONS_Field_Positions_Roles_Select =  Dir.Resx.GetString("DIRECTIONS_Field_Positions_Roles_Select");
            DIRECTIONS_Pos_InCompany = Dir.Resx.GetString("DIRECTIONS_Pos_InCompany");
            DIRECTIONS_Rnd_Catalog = Dir.Resx.GetString("DIRECTIONS_Rnd_Catalog");
            DIRECTIONS_Rnd_TypePerson = Dir.Resx.GetString("DIRECTIONS_Rnd_TypePerson");


            #endregion
        }

        #endregion


        //========================================================================================================== SetHandlers

        #region SetHandlers

        /// <summary>
        ///     Установка событийных делегатов
        /// </summary>
        private void SetHandlers()
        {
            efSotrudnik.OnRenderNtf += efSotrudnik_OnRenderNtf;
            efSotrudnikParent.OnRenderNtf += efSotrudnikParent_OnRenderNtf;

            efMailName.OnRenderNtf += efMailName_OnRenderNtf;
            efMailName.Changed += efMailName_Changed;

            efDomain.Changed += efDomain_Changed;

            efLogin.OnRenderNtf += efLogin_OnRenderNtf;
            efLogin.Changed += efLogin_Changed;

            efPRoles_Role.OnRenderNtf += efPRoles_Role_OnRenderNtf;
            efPTypes_Type.BeforeSearch += efPTypes_Type_BeforeSearch;

            efMailName.BeforeSearch += efMailName_BeforeSearch;
        }

        #endregion


        //========================================================================================================== SetBinder

        #region SetBinder

        /// <summary>
        ///     Связывание элементов управления и свойств объектов
        /// </summary>
        private void SetBinders()
        {
            FieldsToControlsMapping = new Dictionary<V4Control, DocField>
            {
                {efSotrudnik, Dir.SotrudnikField},
                {efRedirectNum, Dir.RedirectNumField},
                {efAdvEq, Dir.AdvEquipField},
                {efPLExit, Dir.PhoneLinkField},
                {efLogin, Dir.LoginField},
                {efLang, Dir.SotrudnikLanguageField},
                {efDomain, Dir.DomainField},
                {efMailName, Dir.MailNameField},
                {efAdvInfo, Dir.AdvInfoField},
                {efAccessEthernet, Dir.AccessEthernetField},
                {efSotrudnikParent, Dir.SotrudnikParentField}
            };
        }

        #endregion


        //========================================================================================================== Override

        #region Override

        /// <summary>
        ///     Если форма в нередактируемом виде - переадресуем
        /// </summary>
        /// <returns></returns>
        protected override bool RedirectPageByCondition()
        {
            if (IsOpenInEditableMode(Request)) return base.RedirectPageByCondition();

            isRedirect = true;
            V4Redirect("DirectionITSigned.aspx");
            return true;
        }

        /// <summary>
        ///     Инициализация сущности документа
        /// </summary>
        /// <param name="copy">Инициализация по копии</param>
        protected override void EntityInitialization(Entity copy = null)
        {
            if (Entity == null) Doc = new Direction();
        }

        protected override void EntityFieldInit()
        {
            base.EntityFieldInit();
            ShowDocDate = false;
            ShowCopyButton = false;
            SetBinders();
            SetHandlers();

            if (!V4IsPostBack && Doc != null)
            {
                               
                if (DocEditable)
                    efSotrudnik.Focus();

                SetInitControls();
            }
        }

        /// <summary>
        ///     Загрузка данных текущего документа
        /// </summary>
        /// <param name="idEntity">Идентификатор документа</param>
        protected override void EntityLoadData(string idEntity)
        {
            base.EntityLoadData(idEntity);
            
            if (!idEntity.IsNullEmptyOrZero())
            {
                Dir.LoadDocumentPositions();

                RefreshPositionCommonFolders();
                RefreshPositionRoles();
                RefreshPositionTypes();
            }
            else
            {
                Dir.PositionCommonFolders = new List<PositionCommonFolder>();
                Dir.PositionRoles = new List<PositionRole>();
                Dir.PositionTypes = new List<PositionType>();
                Dir.PositionAdvancedGrants = new List<PositionAdvancedGrant>();
            }

            GetSotrudnikParentType();
            SetAccessEthernetState();

            DisplayPhoto();
            DisplayAdvInfo();
                     
        }

        /// <summary>
        ///     Первичное заполнение элементов управления на основании свойств документа
        /// </summary>
        protected override void DocumentToControls()
        {
            if (!V4IsPostBack)
            {
                Dir.Languages.ForEach(delegate(Language lang) { efLang.DataItems.Add(lang.Id, lang.Name); });

                Dir.DomainNames.ForEach(delegate(DomainName dName) { efDomain.DataItems.Add(dName.Id, dName.Id); });

                rdWorkPlaceType1.Items.Add(new Item(((int) DirectionsTypeBitEnum.РабочееМестоВОфисе).ToString(),
                    " " + Dir.Resx.GetString(DirectionsTypeBitEnum.РабочееМестоВОфисе
                        .GetAttribute<Specifications.DirectionsType>().RadioLabel)));
                rdWorkPlaceType3.Items.Add(new Item(((int) DirectionsTypeBitEnum.РабочееМестоВнеОфиса).ToString(),
                    " " + Dir.Resx.GetString(DirectionsTypeBitEnum.РабочееМестоВнеОфиса
                        .GetAttribute<Specifications.DirectionsType>().RadioLabel)));
                rdWorkPlaceType4.Items.Add(new Item(((int) DirectionsTypeBitEnum.ИзменениеУчетнойЗаписи).ToString(),
                    " " + Dir.Resx.GetString(DirectionsTypeBitEnum.ИзменениеУчетнойЗаписи
                        .GetAttribute<Specifications.DirectionsType>().RadioLabel)));
                rdWorkPlaceType5.Items.Add(new Item(
                    ((int) DirectionsTypeBitEnum.УчетнаяЗаписьСотрудникаГруппы).ToString(),
                    " " + Dir.Resx.GetString(DirectionsTypeBitEnum.УчетнаяЗаписьСотрудникаГруппы
                        .GetAttribute<Specifications.DirectionsType>().RadioLabel)));
                rdWorkPlaceType6.Items.Add(new Item(
                   ((int)DirectionsTypeBitEnum.УчетнаяЗаписьНаГостевомМесте).ToString(),
                   " " + Dir.Resx.GetString(DirectionsTypeBitEnum.УчетнаяЗаписьНаГостевомМесте
                       .GetAttribute<Specifications.DirectionsType>().RadioLabel)));


                efPLExit.DataItems = new Dictionary<string, object>
                {
                    {"1", Dir.Resx.GetString("DIRECTIONS_lPLExitInSide")},
                    {"7", Dir.Resx.GetString("DIRECTIONS_lPLExitOutTown")},
                    {"15", Dir.Resx.GetString("DIRECTIONS_lPLExitOutCountry")}
                };
            }

            base.DocumentToControls();

            if (Dir.IsNew)
            {
                Dir.Date = DateTime.Today;
                Dir.PhoneLinkField.Value = "7";
            }

            if (V4IsPostBack || !Dir.IsNew)
            {
                GetWorkPlaceType();

                DisplayDataWorkPlace();

                GetPhoneEquip();
                GetCompType();
                GetSotrudnikParentType();

                DisplayDataPhoneDesk(true);
                DisplayDataEthernet();
            }
        }

        /// <summary>
        ///     Установка свойств элементов управления
        /// </summary>
        protected override void SetControlProperties()
        {
            efSotrudnik.IsRequired = true;

            efRedirectNum.IsRequired = true;

            var display = 0;
            var bit = Dir.PhoneEquipField.ValueInt;

            if (bit == (int) DirectionsTypeBitEnum.РабочееМестоВОфисе
                || bit == (int) DirectionsTypeBitEnum.РабочееМестоВнеОфиса) display = 1;

            RefreshMobilPhoneRequired(display);

            if (Dir.AccessEthernetChange)
            {
                efLogin.IsRequired = true;
                efMailName.IsRequired = true;
                if (Dir.WorkPlaceTypeField.ValueString.Length > 0
                    && (Dir.WorkPlaceTypeField.ValueInt == (int) DirectionsTypeBitEnum.УчетнаяЗаписьСотрудникаГруппы 
                    || Dir.WorkPlaceTypeField.ValueInt == (int)DirectionsTypeBitEnum.УчетнаяЗаписьНаГостевомМесте))
                    efAccessEthernet.IsDisabled = true;
            }

            if (Dir.SotrudnikParentCheckField.ValueString.Length != 0) efSotrudnikParent.IsRequired = true;
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
                case "CreateWorkPlace":
                    CreateWorkPlace();
                    break;
                case "GoTo":
                    V4Navigate("DirectionIT.aspx?id=" + param["idDoc"]);
                    break;
                case "ClearSotrudnik":
                    Dir.SotrudnikField.ValueString = "";                    
                    efSotrudnik.RefreshRequired = true;
                    SotrudnikClearInfo(2);
                    RefreshSotrudnikInfo();
                    break;
                case "ClearWorkPlace":
                    ClearWorkPlace();
                    break;
                case "CheckWorkplaceSelected":
                    CheckWorkplaceSelected();
                    break;
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
                case "EditPositionAG":
                    EditPositionAdvancedGrants(int.Parse(param["PositionId"]), param["GuidId"],
                        int.Parse(param["GrantId"]), int.Parse(param["WhatDo"]));
                    break;
                case "DeletePositionCommonFolders":
                    DeletePositionCommonFolders(param["value"]);
                    break;
                case "SavePositionCommonFolders":
                    SavePositionCommonFolders(param["value"]);
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
                    var canAnother = param["CanAnother"] == null || bool.Parse(param["CanAnother"]);
                    RenderWorkPlaceList(int.Parse(param["WPType"]), canAnother);
                    break;
                case "RenderMailNamesList":
                    RenderMailNamesList();
                    break;
                case "AdvSearchWorkPlace":
                    AdvSearchWorkPlace();
                    break;
                case "AdvSearchWorkPlaceTo":
                    AdvSearchWorkPlaceTo();
                    break;
                case "SetWorkPlace":
                    SetWorkPlace(param["value"], param["label"], param["coWorkers"] == "1");
                    break;
                case "SetWorkPlace1":
                    Dir.WorkPlaceTypeField.Value = (int) DirectionsTypeBitEnum.РабочееМестоВОфисе;
                    Dir.WorkPlaceField.Value = null;
                    Dir.WorkPlaceToField.Value = null;
                    RefreshWorkPlaceTypeInfo();
                    break;
                case "SetTransfer":
                    SetTransfer(param["value"], param["label"]);
                    break;
                case "SetWorkPlaceTo":
                    SetWorkPlaceTo(param["value"], "", true);
                    break;
                case "SetMailName":
                    SetMailName(param["value"]);
                    break;
                case "OpenAnotherEquipmentDetails":
                    RenderData.EquipmentAnotherPlace(this, "divSotrudnikAnotherWorkPlaces", Dir);
                    break;
                case "OpenEquipmentDetails":
                    RenderData.EquipmentInPlace(this, param["IdContainer"], Dir, param["IdLocation"]);
                    break;
                case "OpenGroupMembers":
                    RenderData.GroupMembers(this, param["IdContainer"], Dir, param["IdGroup"]);
                    break;
                default:
                    base.ProcessCommand(cmd, param);
                    break;
            }
        }

        private bool ForceCheckBeforeSave() {

            if (ValidateDocument(out List<string> li))
                return true;

            RenderErrors(li);            
            return false;

        }

        /// <summary>
        ///     Проверка документа перед сохранением
        /// </summary>
        /// <param name="errors">Список ошибок, не допускающих сохранения</param>
        /// <param name="exeptions">массив исключений, !здесь не используется</param>
        /// <returns>Можно сохранять/нельзя сохранять</returns>
        protected override bool ValidateDocument(out List<string> errors, params string[] exeptions)
        {
            ValidateDataFillCorrect();

            base.ValidateDocument(out errors);

            if (Dir.WorkPlaceTypeField.ValueString.Length == 0 || Dir.WorkPlaceTypeField.ValueString == "0")
            {
                errors.Add($"{Dir.Resx.GetString("DIRECTIONS_Msg_Err_ЧтоТребуется")}.");
                return false;
            }

            var bitMask = Dir.WorkPlaceTypeField.ValueInt;

            if (bitMask == (int) DirectionsTypeBitEnum.ПереездНаДругоеРабочееМесто)
            {
                if (Dir.WorkPlaceField.ValueString.Length == 0)
                {
                    errors.Add("Не указано рабочее место на котором необходимо организовать работу.");
                    return false;
                }

                if (Dir.WorkPlaceToField.ValueString.Length == 0)
                {
                    errors.Add("Не указано рабочее место с которого необходимо пересадить сотрудника.");
                    return false;
                }
            }

            if (bitMask == (int)DirectionsTypeBitEnum.РабочееМестоВОфисе || bitMask == (int)DirectionsTypeBitEnum.ПереездНаДругоеРабочееМесто)
            {
                if ((Dir.PhoneEquipField.ValueString.Length == 0 || Dir.PhoneEquipField.ValueInt == 0)
                    && (Dir.CompTypeField.ValueString.Length == 0 || Dir.CompTypeField.ValueInt == 0)
                    && Dir.AdvEquipField.ValueString.Length == 0
                    && Dir.AdvInfoField.ValueString.Length == 0
                )
                {
                    errors.Add($"{Dir.Resx.GetString("DIRECTIONS_Msg_Err_ТребуетсяДляРаботы")}.");
                    return false;
                }
            }

            
            if (bitMask == (int) DirectionsTypeBitEnum.РабочееМестоВОфисе)
            {
                if (Dir.WorkPlaceField.ValueString.Length == 0)
                {
                    errors.Add($"{Dir.Resx.GetString("DIRECTIONS_Msg_Err_РабМестоВОфисе")}.");
                    return false;
                }

                if (Dir.RedirectNumField.ValueString.Length > 0)
                {
                    var wr = new StringWriter();
                    ValidationMessages.CheckPhoneNumberIsMobile(this, wr, Dir, Dir.RedirectNumField.ValueString);
                    if (wr.ToString().Length > 0)
                    {
                        var msg = Dir.Resx.GetString("DIRECTIONS_Msg_Err_МобТелефонТип").FirstWordCharToUpper();
                        errors.Add($"{msg}.");
                        return false;
                    }

                }
            }

            //только доступ к корпоративной сети через Internet ==============================================================
            if (Dir.AccessEthernetChange)
            {
                if (Dir.AccessEthernetField.ValueString.Length == 0 || Dir.AccessEthernetField.ValueInt == 0)
                {
                    errors.Add($"{Dir.Resx.GetString("DIRECTIONS_Msg_Err_Сеть")}.");
                    return false;
                }

                if (Dir.LoginField.ValueString.Length == 0)
                    errors.Add(Dir.Resx.GetString("DIRECTIONS_NTF_NoLogin"));
                
                if (Dir.SotrudnikLanguageField.ValueString.Length == 0)
                    errors.Add(Dir.Resx.GetString("DIRECTIONS_NTF_Lang"));

                if (Dir.MailNameField.ValueString.Length == 0)
                    errors.Add(Dir.Resx.GetString("DIRECTIONS_NTF_NoEmail"));
                if (Dir.DomainField.ValueString.Length == 0)
                    errors.Add(Dir.Resx.GetString("DIRECTIONS_NTF_NoDomain"));

                if (errors.Count > 0) return false;
            }

            bitMask = Dir.PhoneEquipField.ValueInt;
            if (((bitMask & 1) == 1 || (bitMask & 2) == 2)
                && Dir.RedirectNumField.ValueString.Length == 0
                && !Dir.Sotrudnik.SimRequired
                && (Dir.WorkPlaceTypeField.ValueInt == (int) DirectionsTypeBitEnum.РабочееМестоВОфисе
                    || Dir.WorkPlaceTypeField.ValueInt == (int) DirectionsTypeBitEnum.РабочееМестоВнеОфиса))
            {
                errors.Add($"{Dir.Resx.GetString("DIRECTIONS_Msg_Err_МобТелефон")}.");
                return false;
            }


            if (Dir.SotrudnikParentCheckField.ValueString.Length != 0 &&
                Dir.SotrudnikParentField.ValueString.Length == 0)
            {
                errors.Add(Dir.SotrudnikParentCheckField.ValueInt == 1
                    ? Dir.Resx.GetString("DIRECTIONS_Msg_Err_Как")
                    : Dir.Resx.GetString("DIRECTIONS_Msg_Err_Вместо"));

                if (Dir.SotrudnikParentCheckField.ValueInt == 2
                    && Dir.SotrudnikField.ValueString.Length > 0
                    && !Dir.Sotrudnik.Unavailable
                    && Dir.Sotrudnik.HasAccount_
                    && Dir.SotrudnikParentField.ValueString.Length > 0
                    && !Dir.SotrudnikParent.Unavailable
                    && Dir.SotrudnikParent.HasAccount_
                    && Dir.Sotrudnik.Login != Dir.SotrudnikParent.Login
                )
                    errors.Add(
                        $"{Dir.Resx.GetString("DIRECTIONS_Msg_Err_ВместоУчетки")}!");

                return false;
            }

            var employee = Dir.Sotrudnik;
            if (employee != null && !employee.Unavailable)
                if (!string.IsNullOrEmpty(employee.CommonEmployeeID))
                {
                    var commonEmployeeId = new Employee(employee.CommonEmployeeID);
                    if (commonEmployeeId.HasAccount_)
                        using (var w = new StringWriter())
                        {
                            ValidationMessages.CheckSotrudnikWorkPlaceGroup(this, w, Dir);
                            var ws = w.ToString();
                            if (ws.Length > 0) errors.Add(ws);
                        }
                }

            if (errors.Count > 0)
                return false;

            return true;
        }


        /// <summary>
        ///     Корректировка документа перед сохранением(очистка не требующих сохранения полей)
        /// </summary>
        private void ValidateDataFillCorrect()
        {
            Dir.LoadedExternalProperties.Remove(Direction.CacheKey_Sotrudnik);
            Dir.LoadedExternalProperties.Remove(Direction.CacheKey_SupervisorData);

            var bitMask = Dir.WorkPlaceTypeField.ValueInt;
            var bitMaskPhoneEq = Dir.PhoneEquipField.ValueInt;

            if (bitMask != (int) DirectionsTypeBitEnum.РабочееМестоВОфисе
                && bitMask != (int) DirectionsTypeBitEnum.ПереездНаДругоеРабочееМесто)
                Dir.WorkPlaceField.Value = null;

            if (bitMask != (int) DirectionsTypeBitEnum.ПереездНаДругоеРабочееМесто)
                Dir.WorkPlaceToField.Value = null;

            if (bitMask != (int) DirectionsTypeBitEnum.РабочееМестоВОфисе
                && bitMask != (int) DirectionsTypeBitEnum.ПереездНаДругоеРабочееМесто)
                if ((bitMaskPhoneEq & 2) == 2)
                    SetPhoneEquip(2, "0");

            if (bitMask != (int) DirectionsTypeBitEnum.РабочееМестоВОфисе
                && bitMask != (int) DirectionsTypeBitEnum.ПереездНаДругоеРабочееМесто
                && bitMask != (int) DirectionsTypeBitEnum.РабочееМестоВнеОфиса)
            {
                Dir.PhoneEquipField.ValueString = "";
                Dir.PhoneLinkField.ValueString = "";
                Dir.CompTypeField.ValueString = "";
                Dir.AdvEquipField.ValueString = "";

                bitMaskPhoneEq = Dir.PhoneEquipField.ValueInt;
            }

            if ((bitMaskPhoneEq & 1) != 1 && (bitMaskPhoneEq & 2) != 2 ||
                !string.IsNullOrEmpty(Dir.SotrudnikField.ValueString) && Dir.Sotrudnik.SimRequired ||
                Dir.WorkPlaceTypeField.ValueInt == (int) DirectionsTypeBitEnum.ПереездНаДругоеРабочееМесто ||
                Dir.WorkPlaceTypeField.ValueInt == (int) DirectionsTypeBitEnum.ИзменениеУчетнойЗаписи ||
                Dir.WorkPlaceTypeField.ValueInt == (int) DirectionsTypeBitEnum.УчетнаяЗаписьСотрудникаГруппы ||
                Dir.WorkPlaceTypeField.ValueInt == (int)DirectionsTypeBitEnum.УчетнаяЗаписьНаГостевомМесте)
                Dir.RedirectNumField.ValueString = "";

            if (!Dir.AccessEthernetChange)
            {
                Dir.AccessEthernetField.ValueString = "0";

                Dir.LoginField.Value = null;
                Dir.MailNameField.Value = null;
                Dir.DomainField.Value = null;
                Dir.SotrudnikLanguageField.Value= null;
                Dir.SotrudnikParentCheckField.Value = null;
                             
                SetRequiredLogin();
                SetRequiredMailName();
                DisplayDataEthernet();

                ClearDocumentPositions();
            }

            if (efSotrudnikParentCheck1.Value == "0" && efSotrudnikParentCheck2.Value == "0")
                Dir.SotrudnikParentCheckField.ValueString = "";

            if (Dir.SotrudnikParentCheckField.ValueString.Length == 0)
                Dir.SotrudnikParentField.ValueString = "";

            //Сохраняем лица по документу

            //КодЛицаСотрудника
            if (Dir.SotrudnikField.ValueString.Length > 0)
            {
                Dir.SotrudnikPersonField.ValueString =
                    Dir.Sotrudnik.PersonEmployeeId.HasValue ? Dir.Sotrudnik.PersonEmployeeId.Value.ToString() : "";

                //КодЛицаЗаказчика
                Dir.PersonZakazchikField.ValueString = Dir.Sotrudnik.OrganizationId.HasValue
                    ? Dir.Sotrudnik.OrganizationId.Value.ToString()
                    : "";

                var dt = Dir.SupervisorData;
                if (dt.Rows.Count != 0)
                {
                    //КодЛицаРаботодателя
                    Dir.PersonEmployerField.ValueString = dt.Rows[0]["КодЛицаКомпанииСотрудника"].ToString();

                    //КодЛицаРаботодателяРуководителя
                    Dir.PersonEmployerHeadField.ValueString = dt.Rows[0]["КодЛицаКомпанииРуководителя"].ToString();
                }
            }
        }

        /// <summary>
        ///     Обработчик события загрузки страницы
        /// </summary>
        /// <param name="sender">Страница</param>
        /// <param name="e">Аргументы</param>
        protected void Page_Load(object sender, EventArgs e)
        {
            if (isRedirect) return;
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
DIRECTIONS_FORM_Mail_Title:""{14}"",
DIRECTIONS_FORM_ADVINFO_Title:""{15}""
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
                Dir.Resx.GetString("DIRECTIONS_FORM_CF_Title"),
                Dir.Resx.GetString("DIRECTIONS_FORM_AG_Title"),
                Dir.Resx.GetString("DIRECTIONS_FORM_Role_Title"),
                Dir.Resx.GetString("DIRECTIONS_FORM_Type_Title"),
                Dir.Resx.GetString("DIRECTIONS_FORM_WP_Title"),
                Dir.Resx.GetString("DIRECTIONS_FORM_Mail_Title"),
                Dir.Resx.GetString("DIRECTIONS_FORM_ADVINFO_Title")
            );

            if (!V4IsPostBack)
            {
                var form = Request.QueryString["form"];
                switch (form)
                {
                    case "cf":
                        JS.Write(@"directions_setElementFocus('', 'btnLinkCFAdd');");
                        break;
                    case "rl":
                        JS.Write(@"directions_setElementFocus('', 'btnLinkRolesAdd');");
                        break;
                    case "tp":
                        JS.Write(@"directions_setElementFocus('', 'btnLinkTypesAdd');");
                        break;
                    case "ag":
                        JS.Write(@"directions_setElementFocus('', 'btnLinkAGAdd');");
                        break;
                }
            }
        }


        /// <summary>
        ///     Проверка, что выбранный сотрудник является группой посменной работы без логина
        /// </summary>
        /// <returns></returns>
        protected bool IsCommonEmployeeNoLogin()
        {
            var employee = Dir.Sotrudnik;
            if (employee != null && !employee.Unavailable)
                if (!string.IsNullOrEmpty(employee.CommonEmployeeID))
                {
                    var commonEmployeeId = new Employee(employee.CommonEmployeeID);
                    if (!commonEmployeeId.HasAccount_) return true;
                }

            return false;
        }

        /// <summary>
        ///     Обновление проверок данных документа
        /// </summary>
        protected override void RefreshManualNotifications()
        {
            if (Dir.SotrudnikField.ValueString.Length == 0) return;
                      
            RefreshPhoto();
            RefreshSotrudnikPost();
            RefreshSotrudnikFinOrg();
            RefreshCommonEmployee();
            RefreshSotrudnikCadrWorkPlaces();
            RefreshSupervisorInfo();
            RefreshMobilPhoneRequired(1);
            RefreshMobilphoneArea();

            RefreshWorkPlaceTypeInfo();

            SetAccessEthernetState();

            DisplayDataWorkPlace();
            DisplayAdvInfo();
        }

        #endregion


        //========================================================================================================== Handlers

        #region Handlers

        #region OnRenderNtf

        /// <summary>
        ///     Событие отрисовки проверок элемента управления "Сотрудник"
        /// </summary>
        /// <param name="sender">Элемент управления</param>
        /// <param name="ntf">Коллекция предупреждений</param>
        private void efSotrudnik_OnRenderNtf(object sender, Ntf ntf)
        {
            ntf.Clear();
            if (Dir.SotrudnikField.ValueString.Length == 0) return;

            var employee = Dir.Sotrudnik;

            if (employee.Unavailable)
            {
                efSotrudnik.ValueText = "#" + employee.Id;
                ntf.Add(new Notification
                {
                    Message = Dir.Resx.GetString("DIRECTIONS_Msg_СотрудникНеДоступен"),
                    Status = NtfStatus.Error,
                    SizeIsNtf = false,
                    DashSpace = false
                });
                return;
            }

            using (var w = new StringWriter())
            {
                var ntfList = new List<Notification>();
                ValidationMessages.CheckSotrudnik(this, w, Dir, ntfList, false);

                foreach (var n in ntfList)
                    ntf.Add(n);
                                
                ntfList.Clear();
                ValidationMessages.CheckSotrudnikStatus(this, w, Dir, ntfList);
                foreach (var n in ntfList)
                    ntf.Add(n);
            }
        }

        /// <summary>
        ///     Событие отрисовки проверок элемента управления "как/вместо"
        /// </summary>
        /// <param name="sender">Элемент управления</param>
        /// <param name="ntf">Коллекция предупреждений</param>
        private void efSotrudnikParent_OnRenderNtf(object sender, Ntf ntf)
        {
            ntf.Clear();

            var p = Dir.SotrudnikParent;
            if (p == null) return;

            if (p.Unavailable)
            {
                efSotrudnikParent.ValueText = "#" + p.Id;
                ntf.Add(new Notification
                {
                    Message = Dir.Resx.GetString("DIRECTIONS_Msg_СотрудникНеДоступен"),
                    Status = NtfStatus.Error,
                    SizeIsNtf = false,
                    DashSpace = false
                });
                return;
            }

            if (Dir.SotrudnikParentCheckField.ValueString.Length == 0) return;

            using (var w = new StringWriter())
            {
                var ntfList = new List<Notification>();
                ValidationMessages.CheckSotrudnikParent(this, w, Dir, ntfList);
                foreach (var n in ntfList)
                    ntf.Add(n);

                ntfList.Clear();
                ValidationMessages.CheckSotrudnikParentStatus(this, w, Dir, ntfList);
                foreach (var n in ntfList)
                    ntf.Add(n);

                var ntfs = new List<Notification>();
                RenderData.ADSI_RenderInfoByEmployee(this, w, Dir, ntfs, p, Dir.SotrudnikParentCheckField.ValueInt);
                ntfs.ForEach(ntf.Add);
            }
        }

        /// <summary>
        ///     Событие отрисовки проверок элемента управления "Роли сотрудника"
        /// </summary>
        /// <param name="sender">Элемент управления</param>
        /// <param name="ntf">Коллекция предупреждений</param>
        private void efPRoles_Role_OnRenderNtf(object sender, Ntf ntf)
        {
            RefreshPRoles_Role_Description();
        }

        /// <summary>
        ///     Событие отрисовки проверок элемента управления "Имя почтового ящика"
        /// </summary>
        /// <param name="sender">Элемент управления</param>
        /// <param name="ntf">Коллекция предупреждений</param>
        private void efMailName_OnRenderNtf(object sender, Ntf ntf)
        {
            ntf.Clear();
            if (V4IsPostBack) RefreshEmailCheck();
        }


        /// <summary>
        ///     Событие отрисовки проверок элемента управления "Логин"
        /// </summary>
        /// <param name="sender">Элемент управления</param>
        /// <param name="ntf">Коллекция предупреждений</param>
        private void efLogin_OnRenderNtf(object sender, Ntf ntf)
        {
            ntf.Clear();
            var ntfs = new List<Notification>();
            using (var w = new StringWriter())
            {
                RenderData.LoginCheck(this, w, Dir, ntfs, false);
                ntfs.ForEach(ntf.Add);
            }
        }

        #endregion

        #region BeforeSearch

        /// <summary>
        ///     Событие установки динамичких фильтров на элемент управления "Сотрудник"
        /// </summary>
        /// <param name="sender">Элемент управления</param>
        protected void efSotrudnik_OnBeforeSearch(object sender)
        {
        }

        /// <summary>
        ///     Событие установки динамичких фильтров на элемент управления "Типы лиц"
        /// </summary>
        /// <param name="sender">Элемент управления</param>
        protected void efPTypes_Type_BeforeSearch(object sender)
        {
            efPTypes_Type.Filter.Catalog.Clear();
            if (efPTypes_Catalog.Value.Length > 0 && !efPTypes_Catalog.Value.Equals(efPTypes_Catalog.CustomRecordId))
                efPTypes_Type.Filter.Catalog.Add(efPTypes_Catalog.Value);
        }

        protected void efMailName_BeforeSearch(object sender)
        {
            efMailName.DataItems.Clear();
            if (Dir.SotrudnikField.ValueString.Length == 0)
            {
                ShowMessage($"{Dir.Resx.GetString("DIRECTIONS_Msg_Err_НетСотрудника")}!", efSotrudnik,
                    Resx.GetString("CONFIRM_StdTitle"));
                return;
            }

            var empl = Dir.Sotrudnik;
            if (empl.Unavailable) return;

            var name =
                $"{(empl.FirstNameEn.Length > 0 ? empl.FirstNameEn + "." : "")}{empl.LastNameEn}".Replace(" ", "");
            efMailName.DataItems.Add(name, name);
        }

        #endregion

        #region Changed

        /// <summary>
        ///     Событие после изменения на элемент управления "Сотрудник"
        /// </summary>
        /// <param name="sender">Элемент управления</param>
        /// <param name="e">Аргументы</param>
        protected void efSotrudnik_OnChanged(object sender, ProperyChangedEventArgs e)
        {
            Dir.LoadedExternalProperties.Remove(Direction.CacheKey_Sotrudnik);
            Dir.LoadedExternalProperties.Remove(Direction.CacheKey_SupervisorData);

            SotrudnikClearInfo(1);

            if (Dir.SotrudnikField.ValueString.Length == 0)
            {
                RefreshSotrudnikInfo();
                return;
            }

            DetermineWorkPlaceType();
            RefreshSotrudnikInfo();
        }

        /// <summary>
        ///     Событие после изменения на элемент управления "Мобильный телефон сотрудник"
        /// </summary>
        /// <param name="sender">Элемент управления</param>
        /// <param name="e">Аргументы</param>
        protected void efRedirectNum_OnChanged(object sender, ProperyChangedEventArgs e)
        {
            var direction = "";

            if (Dir.RedirectNumField.ValueString.Length > 0)
            {
                if (!Regex.IsMatch(Dir.RedirectNumField.ValueString, RegexPattern.PhoneNumber))
                {
                    ShowMessage(Dir.Resx.GetString("DIRECTIONS_Msg_CheckPhoneNumber"), efRedirectNum,
                        Resx.GetString("alertMessage"));
                    Dir.RedirectNumField.ValueString = "";
                    efRedirectNum.Value = "";
                }


                FormatingRedirectNumber(ref direction);
            }

            RefreshMobilphoneArea(direction);

        }

        /// <summary>
        ///     Форматирование введенного номера переадресации
        /// </summary>
        /// <param name="direction">Направление</param>
        private void FormatingRedirectNumber(ref string direction)
        {
            var phone = Dir.RedirectNumField.ValueString;

            var area = new AreaPhoneInfo();
            Phone.AdjustPhoneNumber(ref area, ref phone);
            Dir.RedirectNumField.Value =
                Contact.FormatingContact(22, area.CountryPhoneCode, area.PhoneCodeInCountry, phone, "");
        }

        /// <summary>
        ///     Событие после изменения на элемент управления "Рабочее место в офисе"
        /// </summary>
        /// <param name="sender">Элемент управления</param>
        /// <param name="e">Аргументы</param>
        protected void rdWorkPlaceType1_OnChanged(object sender, ProperyChangedEventArgs e)
        {
            if (Dir.SotrudnikField.ValueString.Length == 0)
            {
                SotrudnikClearInfo(2);
                ShowMessage($"{Dir.Resx.GetString("DIRECTIONS_Msg_Err_НетСотрудника")}!", efSotrudnik,
                    Resx.GetString("CONFIRM_StdTitle"));
                return;
            }

            var bitMask = Dir.WorkPlaceTypeField.ValueString.Length > 0 ? Dir.WorkPlaceTypeField.ValueInt : 0;
            var bit = int.Parse(e.NewValue);

            if (bitMask == bit) return;

            if (!SetWorkPlaceType(DirectionsTypeBitEnum.РабочееМестоВОфисе)) return;

            if (Dir.WorkPlaceField.ValueString.Length == 0)
            {
                RenderWorkPlaceList((int) DirectionsTypeBitEnum.РабочееМестоВОфисе);
            }
            else
            {
                efSotrudnik.RenderNtf();
                SetAccessEthernetState();
                DisplayDataWorkPlace();
                RefreshWorkPlaceTypeInfo();
            }
        }

        /// <summary>
        ///     Событие после изменения на элемент управления "Рабочее место вне офиса"
        /// </summary>
        /// <param name="sender">Элемент управления</param>
        /// <param name="e">Аргументы</param>
        protected void rdWorkPlaceType3_OnChanged(object sender, ProperyChangedEventArgs e)
        {
            if (Dir.SotrudnikField.ValueString.Length == 0)
            {
                ShowMessage($"{Dir.Resx.GetString("DIRECTIONS_Msg_Err_НетСотрудника")}!", efSotrudnik,
                    Resx.GetString("CONFIRM_StdTitle"));
                SotrudnikClearInfo(2);
                return;
            }

            if(!SetWorkPlaceType(DirectionsTypeBitEnum.РабочееМестоВнеОфиса)) return;

            efSotrudnik.RenderNtf();

            SetAccessEthernetState();
            DisplayDataWorkPlace();

            SetRequiredLogin();
            SetRequiredMailName();
            DisplayDataEthernet();

            RefreshWorkPlaceTypeInfo();
            CloseInfoDialogWorkPlace();
        }

      
        /// <summary>
        ///     Событие после изменения на элемент управления "ИзменениеУчетнойЗаписи"
        /// </summary>
        /// <param name="sender">Элемент управления</param>
        /// <param name="e">Аргументы</param>
        protected void rdWorkPlaceType4_OnChanged(object sender, ProperyChangedEventArgs e)
        {
            if (Dir.SotrudnikField.ValueString.Length == 0)
            {
                ShowMessage($"{Dir.Resx.GetString("DIRECTIONS_Msg_Err_НетСотрудника")}!", efSotrudnik,
                    Resx.GetString("CONFIRM_StdTitle"));
                SotrudnikClearInfo(2);
                return;
            }

            if (!SetWorkPlaceType(DirectionsTypeBitEnum.ИзменениеУчетнойЗаписи)) return;

            efSotrudnik.RenderNtf();

            SetAccessEthernetState();
            DisplayDataWorkPlace();

            SetRequiredLogin();
            SetRequiredMailName();
            DisplayDataEthernet();

            RefreshWorkPlaceTypeInfo();
            CloseInfoDialogWorkPlace();
        }

        /// <summary>
        ///     Событие после изменения на элемент управления "УчетнаяЗаписьСотрудникаГруппы"
        /// </summary>
        /// <param name="sender">Элемент управления</param>
        /// <param name="e">Аргументы</param>
        protected void rdWorkPlaceType5_OnChanged(object sender, ProperyChangedEventArgs e)
        {
            if (Dir.SotrudnikField.ValueString.Length == 0)
            {
                ShowMessage($"{Dir.Resx.GetString("DIRECTIONS_Msg_Err_НетСотрудника")}!", efSotrudnik,
                    Resx.GetString("CONFIRM_StdTitle"));
                SotrudnikClearInfo(2);
                return;
            }

            var bitMask = Dir.WorkPlaceTypeField.ValueString.Length > 0 ? Dir.WorkPlaceTypeField.ValueInt : 0;
            var bit = int.Parse(e.NewValue);

            if (bitMask == bit) return;

            if (!SetWorkPlaceType(DirectionsTypeBitEnum.УчетнаяЗаписьСотрудникаГруппы)) return;

            efSotrudnik.RenderNtf();

            SetAccessEthernetState();
            DisplayDataWorkPlace();

            SetRequiredLogin();
            SetRequiredMailName();
            DisplayDataEthernet();

            RefreshWorkPlaceTypeInfo();
            CloseInfoDialogWorkPlace();
        }

        /// <summary>
        ///     Событие после изменения на элемент управления "УчетнаяЗаписьНаГостевомМесте"
        /// </summary>
        /// <param name="sender">Элемент управления</param>
        /// <param name="e">Аргументы</param>
        protected void rdWorkPlaceType6_OnChanged(object sender, ProperyChangedEventArgs e)
        {
            if (Dir.SotrudnikField.ValueString.Length == 0)
            {
                ShowMessage($"{Dir.Resx.GetString("DIRECTIONS_Msg_Err_НетСотрудника")}!", efSotrudnik,
                    Resx.GetString("CONFIRM_StdTitle"));
                SotrudnikClearInfo(2);
                return;
            }

            var bitMask = Dir.WorkPlaceTypeField.ValueString.Length > 0 ? Dir.WorkPlaceTypeField.ValueInt : 0;
            var bit = int.Parse(e.NewValue);

            if (bitMask == bit) return;

            if (!SetWorkPlaceType(DirectionsTypeBitEnum.УчетнаяЗаписьНаГостевомМесте)) return;

            SetAccessEthernetState();
            DisplayDataWorkPlace();

            SetRequiredLogin();
            SetRequiredMailName();
            DisplayDataEthernet();

            RefreshWorkPlaceTypeInfo();
            CloseInfoDialogWorkPlace();
        }

            /// <summary>
            ///     Событие после изменения на элемент управления "Стационарный"
            /// </summary>
            /// <param name="sender">Элемент управления</param>
            /// <param name="e">Аргументы</param>
            protected void efPhoneDesk_OnChanged(object sender, ProperyChangedEventArgs e)
        {
            SetPhoneEquip(1, e.NewValue);
            DisplayDataPhoneDesk(true);
            if (e.NewValue == "1") efRedirectNum.Focus();
        }

        /// <summary>
        ///     Событие после изменения на элемент управления "Web-камера"
        /// </summary>
        /// <param name="sender">Элемент управления</param>
        /// <param name="e">Аргументы</param>
        protected void efPhoneIPCam_OnChanged(object sender, ProperyChangedEventArgs e)
        {
            SetPhoneEquip(8, e.NewValue);
        }

        /// <summary>
        ///     Событие после изменения на элемент управления "Переносная трубка"
        /// </summary>
        /// <param name="sender">Элемент управления</param>
        /// <param name="e">Аргументы</param>
        protected void efPhoneDect_OnChanged(object sender, ProperyChangedEventArgs e)
        {
            SetPhoneEquip(2, e.NewValue);
            DisplayDataPhoneDesk(false);
            if (e.NewValue == "1") efRedirectNum.Focus();
        }

        /// <summary>
        ///     Событие после изменения на элемент управления "Тип связи"
        /// </summary>
        /// <param name="sender">Элемент управления</param>
        /// <param name="e">Аргументы</param>
        protected void efPLExit_OnChanged(object sender, ProperyChangedEventArgs e)
        {
        }

        /// <summary>
        ///     Событие после изменения на элемент управления "Персональный"
        /// </summary>
        /// <param name="sender">Элемент управления</param>
        /// <param name="e">Аргументы</param>
        protected void efComputer_OnChanged(object sender, ProperyChangedEventArgs e)
        {
            SetCompType(2, e.NewValue);
            if (e.NewValue == "1") efAdvEq.Focus();
        }

        /// <summary>
        ///     Событие после изменения на элемент управления "Ноутбук"
        /// </summary>
        /// <param name="sender">Элемент управления</param>
        /// <param name="e">Аргументы</param>
        protected void efNotebook_OnChanged(object sender, ProperyChangedEventArgs e)
        {
            SetCompType(4, e.NewValue);
        }


        /// <summary>
        ///     Событие после изменения на элемент управления "Предпочитаемый язык"
        /// </summary>
        /// <param name="sender">Элемент управления</param>
        /// <param name="e">Аргументы</param>
        protected void efLang_OnChanged(object sender, ProperyChangedEventArgs e)
        {
        }

        /// <summary>
        ///     Событие после изменения на элемент управления "Как"
        /// </summary>
        /// <param name="sender">Элемент управления</param>
        /// <param name="e">Аргументы</param>
        protected void efSotrudnikParentCheck1_OnChanged(object sender, ProperyChangedEventArgs e)
        {
            SetSotrudnikParentType(DirectionsSotrudnikParentBitEnum.КакУСотрудника, e.NewValue);
            if (Dir.SotrudnikParentCheckField.ValueString.Length > 0) efSotrudnikParent.Focus();
        }

        /// <summary>
        ///     Событие после изменения на элемент управления "Вместо"
        /// </summary>
        /// <param name="sender">Элемент управления</param>
        /// <param name="e">Аргументы</param>
        protected void efSotrudnikParentCheck2_OnChanged(object sender, ProperyChangedEventArgs e)
        {
            SetSotrudnikParentType(DirectionsSotrudnikParentBitEnum.ВместоСотрудника, e.NewValue);
            if (Dir.SotrudnikParentCheckField.ValueString.Length > 0) efSotrudnikParent.Focus();
        }

        /// <summary>
        ///     Событие после изменения на элемент управления "Сотрудник как/вместо"
        /// </summary>
        /// <param name="sender">Элемент управления</param>
        /// <param name="e">Аргументы</param>
        protected void efSotrudnikParent_OnChanged(object sender, ProperyChangedEventArgs e)
        {
            if (Dir.SotrudnikParentField.ValueString.Length > 0 && Dir.SotrudnikParent != null &&
                !Dir.SotrudnikParent.Unavailable && !string.IsNullOrEmpty(Dir.SotrudnikParent.CommonEmployeeID))
            {
                Dir.SotrudnikParentField.ValueString = "";
                ShowMessage(Dir.Resx.GetString("DIRECTIONS_NTF_СотрудникВГруппе2"), Resx.GetString("CONFIRM_StdTitle"),
                    MessageStatus.Warning, efSotrudnikParent.HtmlID);
                return;
            }

            if (Dir.SotrudnikField.Value != null && Dir.SotrudnikField.Value.Equals(Dir.SotrudnikParentField.Value))
            {
                Dir.SotrudnikParentField.ValueString = e.OldValue;
                ShowMessage(Dir.Resx.GetString("DIRECTIONS_NTF_СотрудникСовпадает"), Resx.GetString("CONFIRM_StdTitle"),
                    MessageStatus.Warning,
                    efSotrudnikParent.HtmlID);
                return;
            }

            if (Dir.SotrudnikParentField.ValueString.Length > 0)
            {
                JS.Write("directions_setElementFocus(null,'btnLinkCFAdd');");
                ShowConfirm(
                    $"{Dir.Resx.GetString("DIRECTIONS_Msg_FillFormByEmpl")} {Dir.SotrudnikParent.FullName}?",
                    "cmdasync('cmd', 'SotrudnikParentSetInfo');", null);
            }

            efSotrudnikParent.IsRequired = Dir.SotrudnikParentField.ValueString.Length <= 0;
        }

        /// <summary>
        ///     Событие после изменения на элемент управления "Имя почтового ящика"
        /// </summary>
        /// <param name="sender">Элемент управления</param>
        /// <param name="e">Аргументы</param>
        protected void efMailName_Changed(object sender, ProperyChangedEventArgs e)
        {
            SetRequiredMailName();
        }

        /// <summary>
        ///     Событие после изменения на элемент управления "Домен почты"
        /// </summary>
        /// <param name="sender">Элемент управления</param>
        /// <param name="e">Аргументы</param>
        protected void efDomain_Changed(object sender, ProperyChangedEventArgs e)
        {
            efMailName.RenderNtf();
        }

        /// <summary>
        ///     Событие после изменения на элемент управления "Логин"
        /// </summary>
        /// <param name="sender">Элемент управления</param>
        /// <param name="e">Аргументы</param>
        protected void efLogin_Changed(object sender, ProperyChangedEventArgs e)
        {
            SetRequiredLogin();
        }


        #region #Positions

        /// <summary>
        ///     Событие после изменения на элемент управления "Роль сотрудника"
        /// </summary>
        /// <param name="sender">Элемент управления</param>
        /// <param name="e">Аргументы</param>
        protected void efPRoles_Role_OnChanged(object sender, ProperyChangedEventArgs e)
        {
        }

        /// <summary>
        ///     Событие после изменения на элемент управления "Каталог лица"
        /// </summary>
        /// <param name="sender">Элемент управления</param>
        /// <param name="e">Аргументы</param>
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
            {
                JS.Write("$('#btnPTypes_Delete').hide();");
            }
        }

        /// <summary>
        ///     Событие после изменения на элемент управления "Тип лица"
        /// </summary>
        /// <param name="sender">Элемент управления</param>
        /// <param name="e">Аргументы</param>
        protected void efPTypes_Type_OnChanged(object sender, ProperyChangedEventArgs e)
        {
            if (e.NewValue.Equals(efPTypes_Type.CustomRecordId) &&
                efPTypes_Catalog.Value.Equals(efPTypes_Catalog.CustomRecordId))
                efPTypes_Catalog.Value = "";

            if (e.NewValue.Length == 0) return;

            var catalogId = efPTypes_Catalog.Value.Length == 0 ||
                            efPTypes_Catalog.Value.Equals(efPTypes_Catalog.CustomRecordId)
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


        //========================================================================================================== Render

        #region Render

        /// <summary>
        ///     Отрисовка напрвления по мобильному телефону сотрудника
        /// </summary>
        /// <param name="w">Поток вывода</param>
        /// <param name="direction">направление</param>
        protected void RenderMobilphoneArea(TextWriter w, string direction = "")
        {
            if (Dir == null) return;
            var phoneNum = Dir.RedirectNumField.ValueString;
            if (direction.Length == 0) direction = Dir.FormatingMobilNumber(ref phoneNum);
            w.Write(direction);
        }

        /// <summary>
        ///     Отрисовка формы выбора рабочего места
        /// </summary>
        /// <param name="refreshOnly">Только обновить список</param>
        private void RenderWorkPlaceList(int bitMask, bool canAnother = true, bool refreshOnly = false)
        {
            if (Dir == null) return;
            var w = new StringWriter();

            if (Dir.SotrudnikField.ValueString.Length == 0)
            {
                ShowMessage($"{Dir.Resx.GetString("DIRECTIONS_Msg_Err_НетСотрудника")}!", efSotrudnik,
                    Resx.GetString("CONFIRM_StdTitle"));
                return;
            }

            var titleForm = "";


            if (bitMask == (int) DirectionsTypeBitEnum.ПереездНаДругоеРабочееМесто)
                titleForm = "Выберите рабочее место сотрудник, c которого необходимо организовать переезд:";
            else if (bitMask == (int) DirectionsTypeBitEnum.РабочееМестоВОфисе)
                titleForm = "Выберите рабочее место сотрудника, на котором необходимо организовать работу:";

            if (!string.IsNullOrEmpty(titleForm))
                w.Write($"<div class=\"paddingB\">{titleForm}</div>");

            var wps = Dir.Sotrudnik.Workplaces;
            var inx = 0;
            foreach (var wp in wps)
            {
                if (!wp.WorkPlace.Equals((int) ТипыРабочихМест.КомпьютеризированноеРабочееМесто)) continue;

                if (bitMask == (int) DirectionsTypeBitEnum.ПереездНаДругоеРабочееМесто)
                {
                    if (!wp.IsOrganized) continue;
                }
                else if (bitMask == (int) DirectionsTypeBitEnum.РабочееМестоВОфисе)
                {
                    if (wp.IsOrganized && Dir.Sotrudnik.HasAccountValid) continue;
                }


                w.Write("<div>");
                w.Write("<nobr>");

                if (bitMask == (int) DirectionsTypeBitEnum.ПереездНаДругоеРабочееМесто)
                    w.Write(
                        "<a id='imgWP_{1}' class='WP' tabindex=0 onkeydown='var key=v4_getKeyCode(event); if(key == 13 || key == 32) {{directions_SetWorkPlaceTo({0});}}' onclick='directions_SetWorkPlaceTo({0});'><img src='/styles/backtolist.gif' border='0'/></a>",
                        wp.Id, inx);
                else
                    w.Write(
                        "<a id='imgWP_{1}' class='WP' tabindex=0 onkeydown='var key=v4_getKeyCode(event); if(key == 13 || key == 32) {{directions_SetWorkPlace({0}, null, 1);}}' onclick='directions_SetWorkPlace({0}, null, 1);'><img src='/styles/backtolist.gif' border='0'/></a>",
                        wp.Id, inx);

                w.Write("<span class='marginL'>");
                RenderLinkLocation(w, $"wp{wp.Id}", wp.Id, wp.Name,NtfStatus.Empty, "открыть расположение","","listWP");                
                w.Write("</span>");
                w.Write("</nobr>");
                w.Write("</div>");
                inx++;
            }

            var isKadrovik = IsKadrovik();

            if (wps.Count == 0 && Dir.WorkPlaceTypeField.ValueInt == (int)DirectionsTypeBitEnum.РабочееМестоВОфисе ||
                inx == 0)
            {
                RenderNtf(w,
                    new List<Notification>
                    {
                        new Notification
                        {
                            Message = Dir.Resx.GetString("DIRECTIONS_Msg_СотрудникНетРабМеста"),
                            Status = NtfStatus.Empty,
                            SizeIsNtf = false,
                            DashSpace = false
                        }
                    });
              
                w.Write(" ");
                if (isKadrovik)
                {
                    RenderLink(w, "linkNewLocation",
                        "",
                        Dir.Resx.GetString("DIRECTIONS_Msg_СотрудникНетРабМеста_1"), 
                        "directions_SearchWorkPlace();",
                        "", 
                        "",
                        Dir.Resx.GetString("DIRECTIONS_Msg_СотрудникНетРабМеста_2_1"),
                        "0"                       
                        );
                    
                }
                else
                    w.Write(Dir.Resx.GetString("DIRECTIONS_Msg_СотрудникНетРабМеста_0"));
            }


            RefreshControlText(w, "divWorkPlaceList_Data");

            if (refreshOnly) return;

            JS.Write("$('#divWorkPlaceList_Link').show();");

            if (bitMask == (int) DirectionsTypeBitEnum.ПереездНаДругоеРабочееМесто) return;

            if (inx == 0 && bitMask == (int)DirectionsTypeBitEnum.РабочееМестоВОфисе && isKadrovik)
            {                 
                AdvSearchWorkPlace();
            }
        }

        #endregion


        //========================================================================================================== Refresh data context

        #region Refresh data context

        /// <summary>
        ///     Общий метод вывыда потока данных по идентификатору элемента управления
        /// </summary>
        /// <param name="w">Поток вывода</param>
        /// <param name="controlId">Идентификатор элемента управления</param>
        private void RefreshControlText(TextWriter w, string controlId)
        {
            JS.Write(
                "var obj_{0} = document.getElementById('{0}'); if(obj_{0}){{obj_{0}.innerHTML='{1}'; $('#{0}').show();}}",
                controlId,
                HttpUtility.JavaScriptStringEncode(w.ToString()));
        }

        /// <summary>
        ///     Вывод фотографии
        /// </summary>
        private void RefreshPhoto()
        {
            var w = new StringWriter();
            RenderData.Photo(this, w, Dir);
            RefreshControlText(w, "divPhoto");

            DisplayPhoto();
        }

        private void DisplayPhoto()
        {
            if (Dir.SotrudnikField.ValueString.Length > 0)
                JS.Write("$(\"#divPhoto\").show();");
            else
                JS.Write("$(\"#divPhoto\").hide();");
        }

        private void DisplayAdvInfo()
        {
            if (Dir.SotrudnikField.ValueString.Length > 0)
                JS.Write("$(\"#divAdvInfo\").show();");
            else
                JS.Write("$(\"#divAdvInfo\").hide();");
        }

        /// <summary>
        ///     Вывод должности сотрудника
        /// </summary>
        private void RefreshSotrudnikPost()
        {
            var w = new StringWriter();
            RenderData.SotrudnikPost(this, w, Dir, false);
            RefreshControlText(w, "divSotrudnikPost");
        }


        private void RefreshCommonEmployee()
        {
            var w = new StringWriter();
            RenderData.CommonEmployee(this, w, Dir, true);
            RefreshControlText(w, "divCommonEmployee");
        }

        /// <summary>
        ///     Вывод компании, ответственной за фин. регулирование
        /// </summary>
        private void RefreshSotrudnikFinOrg()
        {
            var w = new StringWriter();
            RenderData.SotrudnikFinOrg(this, w, Dir);
            RefreshControlText(w, "divSotrudnikFinOrg");
        }

        /// <summary>
        ///     Вывод рабочих мест сотрудника
        /// </summary>
        private void RefreshSotrudnikCadrWorkPlaces()
        {
            var w = new StringWriter();

            JS.Write("$('.wpntf').hide();");
            if (Dir.SotrudnikField.ValueString.Length > 0)
                Dir.Sotrudnik.LoadedExternalProperties.Remove(Employee.CacheKey_EquipmentsAnotherWorkPlaces);

            RenderData.SotrudnikCadrWorkPlaces(this, w, Dir);
            RefreshControlText(w, "divSotrudnikCadrWorkPlaces");
        }

        /// <summary>
        ///     Вывод информации о непосредственном руководителе
        /// </summary>
        private void RefreshSupervisorInfo()
        {
            var w = new StringWriter();
            RenderData.Supervisor(this, w, Dir);
            RefreshControlText(w, "divSupervisor");
        }

        /// <summary>
        ///     Вывод информации о регионе телефонного номера сотрудника
        /// </summary>
        /// <param name="direction"></param>
        private void RefreshMobilphoneArea(string direction = "")
        {
            var w = new StringWriter();
            RenderMobilphoneArea(w, direction);
            
            
            if (Dir.RedirectNumField.ValueString.Length > 0)
                ValidationMessages.CheckPhoneNumberIsMobile(this, w, Dir, Dir.RedirectNumField.ValueString);                
            
            RefreshControlText(w, "spMobilphoneArea");
        }

        private void RefreshWorkPlaceTypeInfo()
        {
            var w = new StringWriter();
            RenderData.WorkPlaceType(this, w, Dir);
            RefreshControlText(w, "divWorkPlaceType_Info");
        }

        private void RefreshWorkPlace8_LinkInfo(string label, bool setValue)
        {
            JS.Write("directions_CloseWorkPlaceList();");

            efSotrudnik.RenderNtf();

            SetAccessEthernetState();
            DisplayDataWorkPlace();

            RefreshWorkPlaceTypeInfo();
        }

        /// <summary>
        ///     Выовод описаний ролей сотрудника
        /// </summary>
        private void RefreshPRoles_Role_Description()
        {
            var w = new StringWriter();
            if (efPRoles_Role.Value.Length == 0)
                  w.Write("");            
            else
            {
                var r = new Role(efPRoles_Role.Value);
                if (r.Unavailable)
                {
                    w.Write("");
                    return;
                }
                RenderNtf(w,
                    new List<Notification>
                    {
                        new Notification
                        {
                            Message = r.Description,
                            Status = NtfStatus.Information,
                            SizeIsNtf = false,
                            DashSpace = false
                        }
                    });
            }

            RefreshControlText(w, "efPRoles_Role_Description");
        }

        private void RefreshEmailCheck()
        {
            using (var w = new StringWriter())
            {
                RenderData.EmailCheck(this, w, Dir);
                JS.Write(
                    "$('#divEmailCheck').html('{0}');",
                    HttpUtility.JavaScriptStringEncode(w.ToString()));
            }
        }

        #endregion


        //========================================================================================================== Positions

        #region Positions

        #region Common Folders

        /// <summary>
        ///     Вывод формы выбора общих папок
        /// </summary>
        private void AddPositionCommonFolders()
        {
            if (Dir.SotrudnikField.ValueString.Length == 0)
            {
                ShowMessage($"{Dir.Resx.GetString("DIRECTIONS_Msg_Err_НетСотрудника")}!", efSotrudnik,
                    Resx.GetString("CONFIRM_StdTitle"));
                return;
            }
            if (!ForceCheckBeforeSave()) return;
            RefreshPositionDialogCommonFolders();
            JS.Write("directions_SetPositionCFAdd();");
        }

        /// <summary>
        ///     Вывод содержимого формы выбора общих папок
        /// </summary>
        private void RefreshPositionDialogCommonFolders()
        {
            var w = new StringWriter();

            Dir.CommonFolders.ForEach(delegate(CommonFolder cf)
                {
                    var check = false;
                    if (Dir.PositionCommonFolders != null)
                    {
                        var p = Dir.PositionCommonFolders.FirstOrDefault(x => x.CommonFolderId.ToString() == cf.Id);
                        check = p != null;
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

        /// <summary>
        ///     Удаление доступа к общей папке
        /// </summary>
        /// <param name="value">Идентификатор папки</param>
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

        /// <summary>
        ///     Сохранение выбранных общих папок
        /// </summary>
        /// <param name="jsonValues">Список идентификаторов папок в формате json</param>
        private void SavePositionCommonFolders(string jsonValues)
        {
            var isNew = Dir.IsNew;
        
            if (isNew)
            {
                ValidateDataFillCorrect();
                if (!ForceCheckBeforeSave()) return;
                var result = SaveDocument(false, "form=cf");

                if (!result) return;
            }

            var serializer = new JavaScriptSerializer();
            var values = serializer.Deserialize<Dictionary<string, string>>(jsonValues);

            Dir.SavePositionsCommonFoldersByDictionary(values, !isNew);

            if (isNew) return;
            ReloadPositionCommonFolders(true);
        }

        /// <summary>
        ///     Загрузка данных о доступе к общим папкам
        /// </summary>
        /// <param name="setFocus">Устанавливать ли фокус на кнопку добавления</param>
        private void ReloadPositionCommonFolders(bool setFocus = false)
        {
            // Dir.LoadPositionCommonFolders();
            RefreshPositionCommonFolders();
            if (setFocus)
                JS.Write("directions_setElementFocus(null,'btnLinkCFAdd');");
        }

        /// <summary>
        ///     Обновление списка общих папок
        /// </summary>
        private void RefreshPositionCommonFolders()
        {
            var w = new StringWriter();

            Dir.LoadedExternalProperties.Remove("direction._groupInconsistencies");

            RenderPositionCommonFolders(w);
            RefreshControlText(w, "divCommonFoldersData");
        }

        /// <summary>
        ///     Отрисовка списка общих папок
        /// </summary>
        /// <param name="w"></param>
        private void RenderPositionCommonFolders(TextWriter w)
        {
            w.Write("");
            var sortedList = Dir.PositionCommonFolders.OrderBy(o => o.CommonFolderName).ToList();
            var cnt = sortedList.Count();
            var inx = 0;
            sortedList.ForEach(delegate(PositionCommonFolder p)
            {
                inx++;
                w.Write("<div class='marginL disp_inlineBlock'>");

                w.Write(
                    "<a href='javascript:void(0);' class='DelCF' id='btnDelCF_{0}' onclick=\"{1}\" title='{2}'>",
                    p.Id,
                    ShowConfirmDeleteGetJS($"directions_deleteCFCallBack('{p.Id}')"),
                    Resx.GetString("cmdDelete")
                );
                w.Write("<img src='/styles/delete.gif' class= border=0>");
                w.Write("</a>");


                w.Write("{0}{1}", p.CommonFolderName, cnt == 1 || inx == cnt ? "" : ",");
                w.Write("</div>");
            });
            ValidationMessages.CheckGroupInconsistencies(w, Dir, "Общие папки");
        }

        /// <summary>
        ///     Заполнение позиций общих папок, на основании списка общих папок,которым сотрудник уже имеет доступ
        /// </summary>
        /// <param name="empl">Сотрудник</param>
        private void FillPositionCommonFoldersByEmployee(Employee empl)
        {
            var commonFolders = _isCommonEmployeeNoLogin ? empl.CommonFoldersFullInGroup : empl.CommonFolders;

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

       

         /// <summary>
            ///     Заполнение позиций дополнительных прав, на основании существующих прав сотрудника
            /// </summary>
            /// <param name="empl">Сотрудник</param>
        private void FillPositionAdvancedGrantsByEmployee(Employee empl)
        {
            if (Dir == null || Dir.WorkPlaceTypeField.ValueInt == 0) return;

            var grants = _isCommonEmployeeNoLogin ? empl.AdvancedGrantsFullInGroup : empl.AdvancedGrants;

            var bit = (int) Math.Pow(2, Dir.WorkPlaceTypeField.ValueInt - 1);

            foreach (var entry in grants)
            {
                if (entry.Value == null || entry.Value == 0) continue;

                var p = Dir.PositionAdvancedGrants.FirstOrDefault(x => x.GrantId == entry.Key);
                if (p == null)
                {
                    var g = GetObjectById(typeof(AdvancedGrant), entry.Key.ToString()) as AdvancedGrant;
                    if (g != null && !g.Unavailable && g.TaskChoose > 0 && (g.TaskChoose & bit) == bit)
                    {
                        p = new PositionAdvancedGrant
                        {
                            DocumentId = Dir.DocId,
                            GuidId = Guid.NewGuid(),
                            GrantId = int.Parse(g.Id),
                            GrantDescription = g.Name,
                            GrantDescriptionEn = g.NameEn
                        };
                        Dir.PositionAdvancedGrants.Add(p);
                    }
                }
            }

            RefreshPositionAdvancedGrantsAvailable();
        }

        #endregion

        #region Roles

        /// <summary>
        ///     Вывод формы редактирования ролей сотрудника
        /// </summary>
        private void AddPositionRole()
        {
            if (Dir.SotrudnikField.ValueString.Length == 0)
            {
                ShowMessage($"{Dir.Resx.GetString("DIRECTIONS_Msg_Err_НетСотрудника")}!", efSotrudnik,
                    Resx.GetString("CONFIRM_StdTitle"));
                return;
            }
            
            if (!ForceCheckBeforeSave()) return;

            RefreshSotrudnikLink("efRoleEmpl", "divPRoles_Employee");
            JS.Write("directions_setAttribute('{0}', '{1}', '{2}');", "divPositionRolesAdd", "data-id", "");
            JS.Write("directions_SetPositionRolesAdd();");
            JS.Write("$('#btnPRoles_Delete').hide();");

            ClearDialogPositionRole();
            efPRoles_Role.EvalURLClick(efPRoles_Role.URLAdvancedSearch);
        }

        /// <summary>
        ///     Очистка формы редатирования ролей сотрудника
        /// </summary>
        private void ClearDialogPositionRole()
        {
            efPRoles_Role.Value = "";
            efPRoles_Person.Value = "";
            RefreshPRoles_Role_Description();
        }

        /// <summary>
        ///     Добавление роли сотруднику
        /// </summary>
        /// <param name="value"></param>
        private void NewPositionRole(string value)
        {
            if (Dir.SotrudnikField.ValueString.Length == 0)
            {
                ShowMessage($"{Dir.Resx.GetString("DIRECTIONS_Msg_Err_НетСотрудника")}!", efSotrudnik,
                    Resx.GetString("CONFIRM_StdTitle"));
                return;
            }
            
            if (!ForceCheckBeforeSave()) return;

            RefreshSotrudnikLink("efRoleEmpl", "divPRoles_Employee");
            efPRoles_Role.Value = value;
            efPRoles_Person.Value = "";
            efChanged.ChangedByID = null;
            efChanged.SetChangeDateTime = DateTime.MinValue;

            JS.Write("directions_setAttribute('{0}', '{1}', '{2}');", "divPositionRolesAdd", "data-id", "");
            JS.Write("$('#btnPRoles_Delete').hide();");
            JS.Write("directions_SetPositionRolesAdd();");
        }

        /// <summary>
        ///     Редактирование роли сотрудника
        /// </summary>
        /// <param name="value"></param>
        private void EditPositionRole(string value)
        {
            if (Dir.SotrudnikField.ValueString.Length == 0)
            {
                ShowMessage($"{Dir.Resx.GetString("DIRECTIONS_Msg_Err_НетСотрудника")}!", efSotrudnik,
                    Resx.GetString("CONFIRM_StdTitle"));
                return;
            }

            if (!ForceCheckBeforeSave())  return;

            var p = Dir.PositionRoles.FirstOrDefault(x => x.GuidId.ToString() == value);
            if (p == null)
            {
                ShowMessage($"{Dir.Resx.GetString("DIRECTIONS_Msg_Err_НетЗаписи")}!",
                    Resx.GetString("CONFIRM_StdTitle"),
                    MessageStatus.Warning, "btnLinkRolesAdd");
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

        /// <summary>
        ///     Удаление роли сотрудника по GUID
        /// </summary>
        /// <param name="guid">GUID-роли</param>
        /// <param name="closeForm">флаг вызова функции закрытия формы редактирования роли</param>
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

        /// <summary>
        ///     Удаление позции по идентификатору роли
        /// </summary>
        /// <param name="role">Идентификатор роли</param>
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

        /// <summary>
        ///     Сохранение роли сотрудника
        /// </summary>
        /// <param name="value">Идентификатор роли</param>
        /// <param name="check">Необходимость проверкт перед сохранением</param>
        private void SavePositionRole(string value, string check)
        {
            PositionRole p;

            if (check.Equals("1"))
            {
                if (efPRoles_Role.Value.Length == 0)
                {
                    ShowMessage(
                        Dir.Resx.GetString("DIRECTIONS_Msg_UnbleSave") + Environment.NewLine +
                        $"{Dir.Resx.GetString("DIRECTIONS_Msg_НеУказанаРоль")}.",
                        Resx.GetString("CONFIRM_StdTitle"), MessageStatus.Warning, efPRoles_Role.HtmlID);
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
                    {
                        p.PersonId = int.Parse(efPRoles_Person.Value);
                    }
                    else
                    {
                        ShowMessage(
                            Dir.Resx.GetString("DIRECTIONS_Msg_UnbleSave") + Environment.NewLine +
                            $"{Dir.Resx.GetString("DIRECTIONS_MSG_ЗаписьСуществует")}.",
                            Resx.GetString("CONFIRM_StdTitle"), MessageStatus.Warning, efPRoles_Person.HtmlID);
                        return;
                    }
                }

                if (efPRoles_Person.Value.Length == 0)
                {
                    var message = Dir.Resx.GetString("DIRECTIONS_Msg_ЧтоДанныйСотрудник") + Environment.NewLine +
                                  Dir.Resx.GetString("DIRECTIONS_Msg_РольВоВсехКомпаниях1") + Environment.NewLine +
                                  $"{Dir.Resx.GetString("DIRECTIONS_Msg_РольВоВсехКомпаниях2")}?";
                    var callbackYes = $"cmdasync('cmd','SavePositionRole', 'value', '{value}', 'check', 0);";
                    ShowConfirm(message, Resx.GetString("CONFIRM_StdTitle"), Resx.GetString("CONFIRM_StdCaptionYes"),
                        Resx.GetString("CONFIRM_StdCaptionNo"), callbackYes, "", 500);
                    return;
                }
            }

            var isNew = Dir.IsNew;
            if (isNew)
            {
                ValidateDataFillCorrect();
                var result = SaveDocument(false, "form=rl");
                if (!result) return;
            }

            if (value.Length > 0)
            {
                p = Dir.PositionRoles.FirstOrDefault(x => x.GuidId.ToString() == value);

                if (p != null)
                {                   
                    if (efPRoles_Role.ValueInt != null) p.RoleId = efPRoles_Role.ValueInt.Value;
                    p.PersonId = efPRoles_Person.ValueInt ?? 0;
                   
                    p.Save(false);
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

        /// <summary>
        ///     Перезагрузка позиций ролей документа
        /// </summary>
        /// <param name="setFocus"></param>
        private void ReloadPositionRoles(bool setFocus = false)
        {
            Dir.LoadPositionRoles();
            RefreshPositionRoles();
            if (setFocus)
                JS.Write("directions_setElementFocus(null,'btnLinkRolesAdd');");
        }

        /// <summary>
        ///     Обновление списка позиций ролей документа
        /// </summary>
        private void RefreshPositionRoles()
        {
            var w = new StringWriter();

            Dir.LoadedExternalProperties.Remove("direction._groupInconsistencies");

            RenderPositionRoles(w);
            RefreshControlText(w, "divPositionRolesData");
        }

        /// <summary>
        ///     Вывод списка позиций ролей документа
        /// </summary>
        /// <param name="w">Поток вывода</param>
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

            var cnt0 = listRoles.Count;
            var inx0 = 0;

            listRoles.ForEach(delegate(PositionRole p0)
            {
                inx0++;

                w.Write("<div class='marginL disp_inlineBlock'>");

                w.Write(
                    "<a href=\"javascript:void(0);\" id=\"btnLinkRDel_{0}\" onclick=\"{1}\" title=\"{2}\"><img border=\"0\" src=\"/styles/delete.gif\"/></a>",
                    p0.RoleId,
                    ShowConfirmDeleteGetJS($"directions_deleteRoleAllCallBack('{p0.RoleId}')"),
                    Resx.GetString("cmdDelete"));
                              

                w.Write(
                    "{0}: ",
                    p0.RoleObject.Name);

                var r = Dir.PositionRoles.FirstOrDefault(x => x.PersonId == 0 && x.RoleId == p0.RoleId);


                if (r == null)
                    w.Write(
                        "<a href=\"javascript:void(0);\"  class='marginL' id=\"btnLinkNew_{0}\" onclick=\"cmdasync('cmd','NewPositionRole','value','{0}');\" title=\"{1}\"><img border=\"0\" src=\"/styles/new.gif\"/></a>",
                        p0.RoleId, Resx.GetString("cmdAdd"));

                w.Write("</div>");

                var listPerson =
                    Dir.PositionRoles.Where(x => x.RoleId == p0.RoleId).OrderBy(o => o.PersonName).ToList();
                var cnt1 = listPerson.Count;
                var inx1 = 0;
                listPerson.ForEach(delegate(PositionRole p1)
                {
                    inx1++;
                    w.Write("<div class='marginL disp_inlineBlock'>");
                    var p = Dir.PositionRoles.Where(x => x.RoleId == p1.RoleId && x.PersonId != p1.PersonId)
                        .ToList();
                    if (p.Count > 0)
                        w.Write(
                            "<a href=\"javascript:void(0);\" id=\"btnLinkRemove_{0}\" onclick=\"{1}\" title=\"{2}\"><img border=\"0\" src=\"/styles/delete.gif\"/></a>",
                            p1.GuidId,
                            ShowConfirmDeleteGetJS($"directions_deleteRoleCallBack('{p0.GuidId}')"),
                            Resx.GetString("cmdDelete"));
                    else w.Write("&nbsp;");

                    if (p1.PersonId == 0)
                    {
                        w.Write(HttpUtility.HtmlEncode(Dir.Resx.GetString("DIRECTIONS_Rnd_AllCompany1")));
                    }
                    else
                    {
                        w.Write(
                            "<a href=\"javascript:void(0);\"  id=\"btnLinkEdit_{0}\" onclick=\"cmdasync('cmd','EditPositionRole','value','{0}');\" title=\"{1}\"><img border=\"0\" src=\"/styles/edit.gif\"/></a>",
                            p1.GuidId, Resx.GetString("cmdEdit"));

                        w.Write("&nbsp;");
                        w.Write("<span>");
                        RenderLinkPerson(w, "p_" + p0.RoleId + "_" + p1.PersonId, p1.PersonId.ToString(),
                            p1.PersonName);                        
                        w.Write(cnt1 == 1 || inx1 == cnt1 ? "" : ",");
                        w.Write("</span>");
                    }

                    w.Write("</div>");
                });

                if (!p0.RoleObject.Unavailable)
                    RenderNtf(w,
                        new List<Notification>
                        {
                            new Notification
                            {
                                Message = p0.RoleObject.Description,
                                Status = NtfStatus.Information,
                                SizeIsNtf = false,
                                DashSpace = false,
                                CSSClass = "marginL"
                            }
                        });

                if  (inx0 != cnt0)
                    w.Write("<hr/>");
            });

            ValidationMessages.CheckGroupInconsistencies(w, Dir, "Роли");
        }

        /// <summary>
        ///     Заполенение позиций ролей на основании списка ролей, к которым сотрудник уже имеет доступ
        /// </summary>
        /// <param name="empl">Сотрудник</param>
        private void FillPositionRolesByEmployee(Employee empl)
        {
            var roles = _isCommonEmployeeNoLogin ? empl.RolesFullInGroup : empl.Roles;

            roles.ForEach(delegate(EmployeeRole r)
            {
                var r0 = Dir.PositionRoles.FirstOrDefault(x => x.RoleId == r.RoleId && x.PersonId == r.PersonId);
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

        /// <summary>
        ///     Открытие формы добаления позиции типа лица
        /// </summary>
        private void AddPositionTypes()
        {
            if (Dir.SotrudnikField.ValueString.Length == 0)
            {
                ShowMessage($"{Dir.Resx.GetString("DIRECTIONS_Msg_Err_НетСотрудника")}!", efSotrudnik,
                    Resx.GetString("CONFIRM_StdTitle"));
                return;
            }
            if (!ForceCheckBeforeSave()) return;
            efPTypes_Catalog.Value = "";
            efPTypes_Type.ClearSelectedItems();

            RefreshSotrudnikLink("efTypeEmpl", "divPTypes_Employee");
            JS.Write("directions_SetPositionTypesAdd();");
            JS.Write("$('#btnPTypes_Delete').hide();");
        }

        /// <summary>
        ///     Открытие формы редактирования позиуции типа лица
        /// </summary>
        /// <param name="value"></param>
        private void EditPositionType(string value)
        {
            if (Dir.SotrudnikField.ValueString.Length == 0)
            {
                ShowMessage($"{Dir.Resx.GetString("DIRECTIONS_Msg_Err_НетСотрудника")}!", efSotrudnik,
                    Resx.GetString("CONFIRM_StdTitle"));
                return;
            }
            if (!ForceCheckBeforeSave()) return;
            var p =
                Dir.PositionTypes.FirstOrDefault(
                    x =>
                        (x.CatalogId.HasValue ? x.CatalogId.Value.ToString() : efPTypes_Catalog.CustomRecordId) ==
                        value);
            if (p == null)
            {
                ShowMessage($"{Dir.Resx.GetString("DIRECTIONS_Msg_Err_НетЗаписи")}!",
                    Resx.GetString("CONFIRM_StdTitle"),
                    MessageStatus.Warning, "btnLinkTypesAdd");
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

        /// <summary>
        ///     Удаление позиций типов лиц по каталогу
        /// </summary>
        /// <param name="catalog">Идентификатор каталога</param>
        /// <param name="closeForm">Закрывать ли форму после удаления</param>
        private void DeletePositionByCatalog(string catalog, string closeForm)
        {
            if (Dir.IsNew)
            {
                Dir.PositionTypes.RemoveAll(
                    x =>
                        (x.CatalogId.HasValue ? x.CatalogId.Value.ToString() : efPTypes_Catalog.CustomRecordId) ==
                        catalog);
                RefreshPositionTypes();
            }
            else
            {
                var list =
                    Dir.PositionTypes.Where(
                        x =>
                            (x.CatalogId.HasValue ? x.CatalogId.Value.ToString() : efPTypes_Catalog.CustomRecordId) ==
                            catalog).ToList();

                list.ForEach(delegate(PositionType p) { p.Delete(false); });
                ReloadPositionTypes(true);
            }

            if (!string.IsNullOrEmpty(closeForm))
                JS.Write("directions_ClosePositionTypesAdd();");
        }

        /// <summary>
        ///     Удание позиции по типу лица
        /// </summary>
        /// <param name="catalog">Идентификатор каталога</param>
        /// <param name="theme">Идентификатор темы лица</param>
        /// <param name="closeForm">Закрывать ли форму после удаления</param>
        private void DeletePositionType(string catalog, string theme, string closeForm)
        {
            if (Dir.IsNew)
            {
                Dir.PositionTypes.RemoveAll(x =>
                    (x.CatalogId.HasValue ? x.CatalogId.Value.ToString() : efPTypes_Catalog.CustomRecordId) ==
                    catalog &&
                    (x.ThemeId.HasValue ? x.ThemeId.Value.ToString() : efPTypes_Type.CustomRecordId) == theme);
                RefreshPositionTypes();
            }
            else
            {
                var list = Dir.PositionTypes.Where(
                        x =>
                            (x.CatalogId.HasValue ? x.CatalogId.Value.ToString() : efPTypes_Catalog.CustomRecordId) ==
                            catalog &&
                            (x.ThemeId.HasValue ? x.ThemeId.Value.ToString() : efPTypes_Type.CustomRecordId) == theme)
                    .ToList();

                list.ForEach(delegate(PositionType p) { p.Delete(false); });
                ReloadPositionTypes(true);
            }

            if (!string.IsNullOrEmpty(closeForm))
                JS.Write("directions_ClosePositionTypesAdd();");
        }


        /// <summary>
        ///     Сохранения позиции типа лица
        /// </summary>
        /// <param name="check"></param>
        private void SavePositionType(string check)
        {
            if (check.Equals("1"))
            {
                if (efPTypes_Catalog.Value.Length == 0)
                {
                    ShowMessage(
                        Dir.Resx.GetString("DIRECTIONS_Msg_UnbleSave") + Environment.NewLine +
                        $"{Dir.Resx.GetString("DIRECTIONS_Msg_НетУказанКаталог")}.",
                        Resx.GetString("CONFIRM_StdTitle"), MessageStatus.Warning, efPTypes_Catalog.HtmlID);
                    return;
                }

                if (efPTypes_Type.SelectedItems.Where(x => x.Id != efPTypes_Type.CustomRecordId).ToList().Count == 0 &&
                    (efPTypes_Catalog.Value.Length == 0 ||
                     efPTypes_Catalog.Value.Equals(efPTypes_Catalog.CustomRecordId)))
                {
                    ShowMessage(
                        Dir.Resx.GetString("DIRECTIONS_Msg_UnbleSave") + Environment.NewLine +
                        $"{Dir.Resx.GetString("DIRECTIONS_Msg_НетУказанКаталог1")}.",
                        Resx.GetString("CONFIRM_StdTitle"), MessageStatus.Warning, efPTypes_Catalog.HtmlID);
                    return;
                }

                if (efPTypes_Catalog.Value.Length == 0)
                {
                    var message = Dir.Resx.GetString("DIRECTIONS_Msg_ЧтоДанныйСотрудник") + Environment.NewLine +
                                  $"{Dir.Resx.GetString("DIRECTIONS_Msg_ВсеКаталоги")}," + Environment.NewLine +
                                  $"{Dir.Resx.GetString("DIRECTIONS_Msg_ВсеКаталоги1")}?";
                    const string callbackYes = "cmdasync('cmd','SavePositionType', 'check',0);";
                    ShowConfirm(message, Resx.GetString("CONFIRM_StdTitle"), Resx.GetString("CONFIRM_StdCaptionYes"),
                        Resx.GetString("CONFIRM_StdCaptionNo"), callbackYes, "", 400);
                    return;
                }

                if (efPTypes_Type.SelectedItems.Where(x => x.Id != efPTypes_Type.CustomRecordId).ToList().Count == 0)
                {
                    var message = Dir.Resx.GetString("DIRECTIONS_Msg_ЧтоДанныйСотрудник") + Environment.NewLine +
                                  $"{Dir.Resx.GetString("DIRECTIONS_Msg_ВсеТипы")}," + Environment.NewLine +
                                  $"{Dir.Resx.GetString("DIRECTIONS_Msg_ВсеТипы1")}?";
                    const string callbackYes = "cmdasync('cmd','SavePositionType', 'check',0);";
                    ShowConfirm(message, Resx.GetString("CONFIRM_StdTitle"), Resx.GetString("CONFIRM_StdCaptionYes"),
                        Resx.GetString("CONFIRM_StdCaptionNo"), callbackYes, "", 400);

                    return;
                }
            }

            var isNew = Dir.IsNew;
            if (isNew)
            {
                ValidateDataFillCorrect();
                var result = SaveDocument(false, "form=tp");
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
                            (efPTypes_Catalog.Value.Length == 0
                                ? efPTypes_Catalog.CustomRecordId
                                : efPTypes_Catalog.Value))
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
                        CatalogId = efPTypes_Catalog.ValueInt == 0 ? null : efPTypes_Catalog.ValueInt,
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
                            CatalogId = efPTypes_Catalog.ValueInt == 0 ? null : efPTypes_Catalog.ValueInt,
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
                        (efPTypes_Catalog.Value.Length == 0
                            ? efPTypes_Catalog.CustomRecordId
                            : efPTypes_Catalog.Value) ==
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
                                (x.CatalogId.HasValue
                                    ? x.CatalogId.Value.ToString()
                                    : efPTypes_Catalog.CustomRecordId) ==
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
                            CatalogId = efPTypes_Catalog.Value.Length == 0 || efPTypes_Catalog.ValueInt==0 ? null : efPTypes_Catalog.ValueInt,
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

        /// <summary>
        ///     Обновление позиций типов лиц
        /// </summary>
        private void RefreshPositionTypes()
        {
            var w = new StringWriter();

            Dir.LoadedExternalProperties.Remove("direction._groupInconsistencies");


            RenderPositionTypes(w);
            RefreshControlText(w, "divPositionTypesData");
        }

        /// <summary>
        ///     Вывод позиций типов лиц
        /// </summary>
        /// <param name="w">Поток вывода</param>
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

            var cnt0 = listTypes.Count;
            var inx0 = 0;

            listTypes.ForEach(delegate(PositionType p0)
            {
                inx0++;
                var catalog0 = p0.CatalogId.HasValue
                    ? p0.CatalogId.Value.ToString()
                    : efPTypes_Catalog.CustomRecordId;

                w.Write("<div class='marginL disp_inlineBlock'>");

                w.Write(
                    "<a href=\"javascript:void(0);\"  id=\"btnLinkTEdit_{0}\" onclick=\"cmdasync('cmd','EditPositionType','value','{0}');\" title=\"{1}\"><img border=\"0\" src=\"/styles/edit.gif\"/></a>",
                    catalog0,
                    Resx.GetString("cmdEdit"));

                w.Write(
                    "<a href=\"javascript:void(0);\"  class='marginL' id=\"btnLinkTDel_{0}\" onclick=\"{1}\" title=\"{2}\"><img border=\"0\" src=\"/styles/delete.gif\"/></a>",
                    catalog0,
                    ShowConfirmDeleteGetJS($"directions_deleteTypeAllCallBack('{catalog0}')"),
                    Resx.GetString("cmdDelete")
                );


                w.Write(
                    "{0}:",
                    string.IsNullOrEmpty(p0.CatalogName)
                        ? HttpUtility.HtmlEncode(Dir.Resx.GetString("DIRECTIONS_Rnd_AllCatalog"))
                        : p0.CatalogName);
                w.Write("</div>");

                var listTheme =
                    Dir.PositionTypes.Where(x => x.CatalogId.Equals(p0.CatalogId)).OrderBy(o => o.ThemeName).ToList();
                var cnt1 = listTheme.Count;
                var inx1 = 0;
                listTheme.ForEach(delegate(PositionType p1)
                {
                    inx1++;
                    var catalog1 = p1.CatalogId.HasValue
                        ? p1.CatalogId.Value.ToString()
                        : efPTypes_Catalog.CustomRecordId;

                    var theme1 = p1.ThemeId.HasValue ? p1.ThemeId.Value.ToString() : efPTypes_Type.CustomRecordId;

                    var p =
                        Dir.PositionTypes.Where(
                            x =>
                                (x.CatalogId.HasValue
                                    ? x.CatalogId.Value.ToString()
                                    : efPTypes_Catalog.CustomRecordId) ==
                                catalog1 &&
                                (x.ThemeId.HasValue ? x.ThemeId.Value.ToString() : efPTypes_Type.CustomRecordId) !=
                                theme1).ToList();

                    w.Write("<div class='marginL disp_inlineBlock'>");

                    if (p.Count > 0)
                        w.Write(
                            "<a href=\"javascript:void(0);\" id=\"btnLinkTDel_{0}\" onclick=\"{1}\" title=\"{2}\"><img border=\"0\" src=\"/styles/delete.gif\"/></a>",
                            catalog1 + "_" + theme1,
                            ShowConfirmDeleteGetJS($"directions_deleteTypeCallBack('{catalog1}','{theme1}')"),
                            Resx.GetString("cmdDelete")
                        );

                    w.Write("{0}",
                        HttpUtility.HtmlEncode(string.IsNullOrEmpty(p1.ThemeName)
                            ? Dir.Resx.GetString("DIRECTIONS_Rnd_AllTypePerson")
                            : p1.ThemeName));

                    w.Write(cnt1 == 1 || inx1 == cnt1 ? "" : ",");

                    w.Write("</div>");
                });

                if (inx0 != cnt0)
                    w.Write("<hr/>");
            });

            ValidationMessages.CheckGroupInconsistencies(w, Dir, "Типы лиц");
        }

        /// <summary>
        ///     Перезагрузка позиций типов лиц
        /// </summary>
        /// <param name="setFocus"></param>
        private void ReloadPositionTypes(bool setFocus = false)
        {
            Dir.LoadPositionTypes();
            RefreshPositionTypes();
            if (setFocus)
                JS.Write("directions_setElementFocus(null,'btnLinkTypesAdd');");
        }

        /// <summary>
        ///     Заполение позиций типов лиц по каталогу
        /// </summary>
        /// <param name="catalog">Каталог</param>
        private void FillPositionThemeItems(string catalog)
        {
            efPTypes_Type.SelectedItems.Clear();

            if (!string.IsNullOrEmpty(catalog))
                Dir.PositionTypes.Where(
                        x =>
                            (!x.CatalogId.HasValue ? efPTypes_Type.CustomRecordId : x.CatalogId.Value.ToString()) ==
                            catalog)
                    .ToList()
                    .ForEach(
                        delegate(PositionType p)
                        {
                            var pThId = p.ThemeId.HasValue ? p.ThemeId.ToString() : efPTypes_Type.CustomRecordId;
                            var pThName = p.ThemeId.HasValue ? p.ThemeName : efPTypes_Type.CustomRecordText;
                            efPTypes_Type.SelectedItems.Add(new Lib.Entities.Item
                            {
                                Id = pThId,
                                Value = new Lib.Entities.Item {Id = pThId, Value = pThName}
                            });
                        });
            efPTypes_Type.RefreshDataBlock();
        }

        /// <summary>
        ///     Заполнение позиций типов лиц на основании списка типов лиц, к которым сотрудник уже имеет доступ
        /// </summary>
        /// <param name="empl"></param>
        private void FillPositionTypesByEmployee(Employee empl)
        {
            var types = _isCommonEmployeeNoLogin ? empl.TypesFullInGroup : empl.Types;
            types.ForEach(delegate(EmployeePersonType t)
            {
                var t0 =
                    Dir.PositionTypes.FirstOrDefault(
                        x =>
                            (x.CatalogId.HasValue && t.CatalogId.HasValue && x.CatalogId.Value == t.CatalogId.Value ||
                             !x.CatalogId.HasValue && !t.CatalogId.HasValue) &&
                            (x.ThemeId.HasValue && t.ThemeId.HasValue && x.ThemeId.Value == t.ThemeId.Value ||
                             !x.ThemeId.HasValue && !t.ThemeId.HasValue));
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

        /// <summary>
        ///     Обновление списка позиций дополнительных прав
        /// </summary>
        private void RefreshPositionAdvancedGrantsAvailable()
        {
            Dir.LoadedExternalProperties.Remove("direction._groupInconsistencies");

            using (var w = new StringWriter())
            {
                RenderData.AdvancedGrants(this, w, Dir, 1);
                RefreshControlText(w, "divAdvancedGrantsContainer1");
            }

            using (var w = new StringWriter())
            {
                RenderData.AdvancedGrants(this, w, Dir, 2);
                RefreshControlText(w, "divAdvancedGrantsContainer2");
            }

            using (var w = new StringWriter())
            {
                RenderData.AdvancedGrants(this, w, Dir, 4);
                RefreshControlText(w, "divAdvancedGrantsContainer4");
            }
        }

        /// <summary>
        ///     Редактирование позиции Дополнительные права
        /// </summary>
        /// <param name="positionId">Идентификатор позиции</param>
        /// <param name="guidId">Динамический идентификатор позиции</param>
        /// <param name="grantId">Идентификатор права</param>
        /// <param name="whatDo">Добавляем/Удаляем</param>
        private void EditPositionAdvancedGrants(int positionId, string guidId, int grantId, int whatDo)
        {
            if (whatDo == 1)
            {
                var grant = GetObjectById(typeof(AdvancedGrant), grantId.ToString()) as AdvancedGrant;

                if (grant == null || grant.Unavailable)
                {
                    ShowMessage($"Ошибка получения данных о правах с идентификатором #{grantId}",
                        Resx.GetString("CONFIRM_StdTitle"));
                    return;
                }

                var p = Dir.PositionAdvancedGrants.FirstOrDefault(x => x.GuidId.ToString() == guidId);
                if (p == null)
                {
                    Dir.PositionAdvancedGrants.Add(new PositionAdvancedGrant
                    {
                        GuidId = Guid.Parse(guidId),
                        DocumentId = Dir.DocId,
                        GrantId = grantId,
                        GrantDescription = grant.Name,
                        GrantDescriptionEn = grant.NameEn
                    });
                    p = Dir.PositionAdvancedGrants.FirstOrDefault(x => x.GuidId.ToString() == guidId);
                }

                if (!Dir.IsNew)
                {
                    p?.Save(false);
                    Dir.LoadPositionAdvancedGrants();
                    RefreshPositionAdvancedGrantsAvailable();
                }
            }
            else
            {
                var p = Dir.PositionAdvancedGrants.FirstOrDefault(x =>
                    Dir.IsNew && x.GuidId.ToString() == guidId || !Dir.IsNew && x.Id == positionId.ToString());
                if (p == null)
                {
                    ShowMessage($"Ошибка получения данных о правах в позиции документа #{positionId}",
                        Resx.GetString("CONFIRM_StdTitle"));
                    return;
                }

                if (!Dir.IsNew)
                {
                    p.Delete(false);

                    Dir.LoadPositionAdvancedGrants();
                    RefreshPositionAdvancedGrantsAvailable();
                }
                else
                {
                    Dir.PositionAdvancedGrants.Remove(p);
                }
            }
        }

        #endregion

        #endregion


        //========================================================================================================== Adv functions

        #region Adv functions

        /// <summary>
        ///     Установка доступности полей "Доступ к корпоративной сети"
        /// </summary>
        private void SetAccessEthernetState()
        {
            if (Dir.SotrudnikField.ValueString.Length == 0 || Dir.WorkPlaceTypeField.ValueString.Length == 0) return;

            var empl = Dir.Sotrudnik;
            var type = Dir.WorkPlaceTypeField.ValueInt;
            var accessEthernetWasChange = false;

            var ha = empl.HasAccountValid;
            Dir.AccessEthernetChange =

                ha && !empl.ExistDirectionsAfterChangePost(int.Parse(Dir.Id)) && type == (int)DirectionsTypeBitEnum.ИзменениеУчетнойЗаписи

                || !ha && !empl.Workplaces.Any(x=>x.IsComputeredWorkPlace) && type == (int)DirectionsTypeBitEnum.УчетнаяЗаписьНаГостевомМесте

                || !ha && type == (int) DirectionsTypeBitEnum.УчетнаяЗаписьСотрудникаГруппы

                || !ha && type == (int) DirectionsTypeBitEnum.РабочееМестоВОфисе

                || !ha && type == (int) DirectionsTypeBitEnum.РабочееМестоВнеОфиса;

            efAccessEthernet.IsDisabled = true;

            if (Dir.IsNew)
            {
                var aef = Dir.AccessEthernetChange ? "1" : "0";

                if (Dir.AccessEthernetField.ValueString != aef)
                {
                    Dir.AccessEthernetField.ValueString = aef;
                    accessEthernetWasChange = true;
                }
                
            }
            else
            {
                if (Dir.AccessEthernetChange && Dir.AccessEthernetField.ValueString != "1")
                {
                    Dir.AccessEthernetField.ValueString = "1";
                    accessEthernetWasChange = true;
                }
                else if (Dir.AccessEthernetField.ValueString == "1")
                    Dir.AccessEthernetChange = true;

            }

            if (accessEthernetWasChange)
            {
                Dir.IsModified = true;              
                JS.Write("v4_SetSaveButtonDisabled(false);");
            }
            

            if (accessEthernetWasChange)
                SetEthernetData();
        }

        /// <summary>
        ///     Поиск свободного логина
        /// </summary>
        /// <returns>Логин</returns>
        private string GetFreeLogin()
        {
            var sqlParams = new Dictionary<string, object> {{"@КодСотрудника", Dir.Sotrudnik.Id}};
            var dt = DBManager.GetData(SQLQueries.SELECT_СвободныйЛогин, Config.DS_user, CommandType.Text, sqlParams);
            return dt.Rows.Count != 1 ? "" : dt.Rows[0][0].ToString();
        }

        /// <summary>
        ///     Заполнение скрытых полей сотрудника
        /// </summary>
        private void FillRedirectNumber()
        {
            if (Dir.SotrudnikField.ValueString.Length == 0)
            {
                Dir.RedirectNumField.ValueString = "";
                return;
            }

            if (Dir.SotrudnikField.ValueString.Length > 0 && Dir.RedirectNumField.ValueString.Length == 0)
                Dir.RedirectNumField.Value = Dir.Sotrudnik.GetMobilePhone();
        }

        /// <summary>
        ///     Установка обязательности имени почтового ящика
        /// </summary>
        private void SetRequiredMailName()
        {
            if (Dir.AccessEthernetChange)
                efMailName.IsRequired = Dir.MailNameField.ValueString.Length == 0;
            else
                efMailName.IsRequired = false;
        }

        /// <summary>
        ///     Установка обязательности логина
        /// </summary>
        private void SetRequiredLogin()
        {
            if (Dir.AccessEthernetChange)
                efLogin.IsRequired = Dir.LoginField.ValueString.Length == 0;
            else
                efLogin.IsRequired = false;

            efLogin.RefreshRequired = true;
        }

        /// <summary>
        ///     Установка значения поля объекта WorkPlaceTypeField из нескольких контролов
        /// </summary>
        /// <param name="bit">Что изменили</param>
        private bool SetWorkPlaceType(DirectionsTypeBitEnum bit)
        {
            var bitMask = (int) bit;
            
            Dir.WorkPlaceTypeField.ValueString = bitMask == 0 ? "" : bitMask.ToString();

            if (Dir.IsNew)
            {
                var idDoc = ValidationMessages.CheckExistsDirection(this, new StringWriter(), Dir);

                if (idDoc != -1)
                {
                    ShowConfirm(Dir.Resx.GetString("DIRECTIONS_Msg_ExistDirection")  + Environment.NewLine + Dir.Resx.GetString("DIRECTIONS_Msg_ExistDirection1"), Dir.Resx.GetString("DIRECTIONS_Msg_Title"), Resx.GetString("CONFIRM_StdCaptionYes"), Resx.GetString("CONFIRM_StdCaptionNo"), $"directions_goTo({idDoc});", $"directions_goTo(-1);", "efSotrudnik_0", 403); 
                    return false;
                }
            }

            GetWorkPlaceType();

            //Убираем DECT
            if (Dir.WorkPlaceTypeField.ValueString.Length == 0 || Dir.WorkPlaceTypeField.ValueString == "0" ||
                Dir.WorkPlaceTypeField.ValueString.Length > 0 && Dir.WorkPlaceTypeField.ValueInt !=
                (int) DirectionsTypeBitEnum.РабочееМестоВОфисе)
                if (Dir.PhoneEquipField.ValueString.Length > 0 &&
                    (Dir.PhoneEquipField.ValueInt & 2) == 2)
                    SetPhoneEquip(2, "0");

            SetDocumentName();
            DisplayFilterWorkPlace();
            RefreshPositionAdvancedGrantsAvailable();
            return true;
        }

        /// <summary>
        ///     Проставление значения элементам управления на основании WorkPlaceTypeField
        /// </summary>
        private void GetWorkPlaceType()
        {
            if (!V4IsPostBack)
                if (Dir.WorkPlaceField.ValueString.Length > 0 && Dir.WorkPlaceTypeField.ValueString.Length == 0)
                    Dir.WorkPlaceTypeField.Value = (int) DirectionsTypeBitEnum.РабочееМестоВОфисе;

            var bitMask = Dir.WorkPlaceTypeField.ValueInt;

            if (bitMask == (int) DirectionsTypeBitEnum.РабочееМестоВОфисе
                || bitMask == (int) DirectionsTypeBitEnum.ПереездНаДругоеРабочееМесто)
                rdWorkPlaceType1.Value = ((int) DirectionsTypeBitEnum.РабочееМестоВОфисе).ToString();
            else
                rdWorkPlaceType1.Value = "";


            rdWorkPlaceType3.Value =
                bitMask == (int) DirectionsTypeBitEnum.РабочееМестоВнеОфиса
                    ? ((int) DirectionsTypeBitEnum.РабочееМестоВнеОфиса).ToString()
                    : "";

            rdWorkPlaceType5.Value =
                bitMask == (int) DirectionsTypeBitEnum.УчетнаяЗаписьСотрудникаГруппы
                    ? ((int) DirectionsTypeBitEnum.УчетнаяЗаписьСотрудникаГруппы).ToString()
                    : "";

            rdWorkPlaceType6.Value =
               bitMask == (int)DirectionsTypeBitEnum.УчетнаяЗаписьНаГостевомМесте
                   ? ((int)DirectionsTypeBitEnum.УчетнаяЗаписьНаГостевомМесте).ToString()
                   : "";
        }

        private void SetDocumentName()
        {
            var oldDocumentName = Dir.DocumentName ?? "";
            switch (Dir.WorkPlaceTypeField.ValueInt)
            {
                case (int) DirectionsTypeBitEnum.ПереездНаДругоеРабочееМесто:
                    Dir.DocumentName = DirectionsTypeBitEnum.ПереездНаДругоеРабочееМесто
                        .GetAttribute<Specifications.DirectionsType>().DocumentName;
                    break;
                case (int) DirectionsTypeBitEnum.РабочееМестоВОфисе:
                    Dir.DocumentName = DirectionsTypeBitEnum.РабочееМестоВОфисе
                        .GetAttribute<Specifications.DirectionsType>().DocumentName;
                    break;
                case (int) DirectionsTypeBitEnum.РабочееМестоВнеОфиса:
                    Dir.DocumentName = DirectionsTypeBitEnum.РабочееМестоВнеОфиса
                        .GetAttribute<Specifications.DirectionsType>().DocumentName;
                    break;
                case (int) DirectionsTypeBitEnum.УчетнаяЗаписьСотрудникаГруппы:
                    Dir.DocumentName = DirectionsTypeBitEnum.УчетнаяЗаписьСотрудникаГруппы
                        .GetAttribute<Specifications.DirectionsType>().DocumentName;
                    break;
                case (int) DirectionsTypeBitEnum.ИзменениеУчетнойЗаписи:
                    Dir.DocumentName = DirectionsTypeBitEnum.ИзменениеУчетнойЗаписи
                        .GetAttribute<Specifications.DirectionsType>().DocumentName;
                    break;
                case (int)DirectionsTypeBitEnum.УчетнаяЗаписьНаГостевомМесте:
                    Dir.DocumentName = DirectionsTypeBitEnum.УчетнаяЗаписьНаГостевомМесте
                        .GetAttribute<Specifications.DirectionsType>().DocumentName;
                    break;
                default:
                    Dir.DocumentName = "";
                    break;
            }

            if (oldDocumentName != Dir.DocumentName)
                RefreshDocTitle();
        }


        /// <summary>
        ///     Отображение элемнентов управления, связанных  с рабочим местом
        /// </summary>
        private void DisplayDataWorkPlace()
        {
            var bitMask = Dir.WorkPlaceTypeField.ValueInt;
            var hasAccount = 0;
            if (Dir.SotrudnikField.ValueString.Length > 0 && !Dir.Sotrudnik.Unavailable && Dir.Sotrudnik.HasAccountValid)
                hasAccount = 1;

            JS.Write("directions_Data_WP({0}, {1}, {2}, {3});",
                bitMask,
                Dir.AccessEthernetChange ? 1 : 0,
                _isCommonEmployeeNoLogin ? 0 : 1,
                hasAccount);
        }

        private void DisplayFilterWorkPlace()
        {
            var hide = Dir.WorkPlaceTypeField.ValueInt != (int) DirectionsTypeBitEnum.РабочееМестоВОфисе;
            if (hide) JS.Write("$('.filterWP').hide();");
        }

        /// <summary>
        ///     Функция определения, какой вид указания можно создавать
        /// </summary>
        private void DetermineWorkPlaceType()
        {
            var empl = Dir.Sotrudnik;
            var radio4Vis = true;
            var radio6Vis = true;

            if (empl == null || empl.Unavailable) return;

            // сотрудник является группой посменной работы
            if (empl.IsGroupWork)
            {
                Dir.SotrudnikField.ValueString = "";
                SotrudnikClearInfo(2);

                ShowMessage(
                    string.Format(
                        "Сотрудник {0} является группой посменной работы. Создать указание на группу посменной работы невозможно!",
                        empl.FIO),
                    Resx.GetString("CONFIRM_StdTitle"), MessageStatus.Warning, $"{efSotrudnik.HtmlID}_0");
                return;
            }

            //входит в группу посменной работы
            if (!string.IsNullOrEmpty(empl.CommonEmployeeID))
            {
                var group = new Employee(empl.CommonEmployeeID);

                //если группа с логином
                if (group.HasAccount_)
                {
                    Dir.SotrudnikField.ValueString = "";
                    SotrudnikClearInfo(2);

                    ShowMessage(
                        string.Format(
                            "Сотрудник {0} входит в группу посменной работы {1}, которая имеет собственную учетную запись. Создать указание невозможно!",
                            empl.FullName, group.FullName),
                        Resx.GetString("CONFIRM_StdTitle"), MessageStatus.Warning, $"{efSotrudnik.HtmlID}_0");
                    return;
                }

                //группа не имеет логина
                _isCommonEmployeeNoLogin = true;

                //сотрудник имеет учетную запись
                if (empl.HasAccountValid)
                {
                    Dir.SotrudnikField.ValueString = "";
                    SotrudnikClearInfo(2);

                    ShowMessage(
                        string.Format(
                            "Сотрудник {0} входит в группу посменной работы {1}, и уже имеет учетную запись. Создать указание невозможно!",
                            empl.FullName, group.FullName),
                        Resx.GetString("CONFIRM_StdTitle"), MessageStatus.Warning, $"{efSotrudnik.HtmlID}_0");
                    return;
                }


                if (!SetWorkPlaceType(DirectionsTypeBitEnum.УчетнаяЗаписьСотрудникаГруппы)) return;

                efSotrudnik.RenderNtf();

                SetAccessEthernetState();
                DisplayDataWorkPlace();

                SetRequiredLogin();
                SetRequiredMailName();
                DisplayDataEthernet();

                RefreshWorkPlaceTypeInfo();
                return;
            }

            var ha = empl.HasAccountValid;

            if (ha || empl.Workplaces.Any(x => x.IsComputeredWorkPlace))
                radio6Vis = false;


            //TODO: Выяснить когда заполняется
            //SetSotrudikHiddenField();

            if (!ha || (ha && empl.ExistDirectionsAfterChangePost(int.Parse(Dir.Id))))
                radio4Vis = false;

            JS.Write("$(\".radio1\").show();");
            JS.Write("$(\".radio3\").show();");

            if (radio4Vis)
                JS.Write("$(\".radio4\").show();");
            else
                JS.Write("$(\".radio4\").hide();");

            if (radio6Vis)
                JS.Write("$(\".radio6\").show();");
            else
                JS.Write("$(\".radio6\").hide();");

            JS.Write("$(\"#divWorkPlaceList_Data\").html('');");
            JS.Write("$(\"#divWorkPlaceList_Data\").hide();$(\"#divWorkPlaceList_Link\").hide();");

            JS.Write("directions_SetInfoWorkPlaceType();");
        }

        /// <summary>
        ///     Отображение информации, связанной со стационарным телефоном
        /// </summary>
        /// <param name="setIp"></param>
        private void DisplayDataPhoneDesk(bool setIp)
        {
            if (setIp) efPhoneIPCam.IsDisabled = (Dir.PhoneEquipField.ValueInt & 1) != 1;
            DisplayPhoneLinkExit();
            DisplayMobilPhone();
        }

        /// <summary>
        ///     Отображение информации, связанной доступом к корпоративной сети
        /// </summary>
        private void DisplayDataEthernet()
        {            
            JS.Write("directions_Data_Lan({0},{1});",
                Dir.AccessEthernetChange ? 1 : 0, 1);
        }

        /// <summary>
        ///     Установка значения поля объекта PhoneEquipField из нескольких контролов
        /// </summary>
        /// <param name="bit">Что изменили</param>
        /// <param name="whatdo">Как изменили</param>
        private void SetPhoneEquip(int bit, string whatdo)
        {
            var bitMask = Dir.PhoneEquipField.ValueInt;

            if (whatdo.Equals("0"))
                bitMask ^= bit;
            else
                bitMask |= bit;

            if ((bitMask & 8) == 8 && (bitMask & 1) != 1) bitMask ^= 8;
            if ((bitMask & 32) == 32 && (bitMask & 16) != 16) bitMask ^= 32;

            Dir.PhoneEquipField.ValueString = bitMask == 0 ? "" : bitMask.ToString();

            GetPhoneEquip();
        }

        /// <summary>
        ///     Установка значений элементам управления на основании PhoneEquipField
        /// </summary>
        private void GetPhoneEquip()
        {
            var bitMask = Dir.PhoneEquipField.ValueInt;

            if ((bitMask & 1) == 1) efPhoneDesk.Value = "1";
            else efPhoneDesk.Value = "0";

            if ((bitMask & 2) == 2) efPhoneDect.Value = "1";
            else efPhoneDect.Value = "0";

            if ((bitMask & 8) == 8) efPhoneIPCam.Value = "1";
            else efPhoneIPCam.Value = "0";

            if (!V4IsPostBack)
            {
                efPhoneDesk.OriginalValue = efPhoneDesk.Value;
                efPhoneDect.OriginalValue = efPhoneDect.Value;
                efPhoneIPCam.OriginalValue = efPhoneIPCam.Value;
            }
        }

        /// <summary>
        ///     Отображение элемента управления Типа телефонной связи
        /// </summary>
        private void DisplayPhoneLinkExit()
        {
            var display = "none";
            if (Dir.PhoneEquipField.ValueInt > 0)
                display = "inline-table";

            JS.Write("var objDiv = document.getElementById('divPLExit'); if (objDiv) objDiv.style.display ='{0}';",
                display);
        }

        /// <summary>
        ///     Отображение элемента управления Мобильный телефон сотрудника
        /// </summary>
        private void DisplayMobilPhone()
        {
            var display = 0;
            if (Dir.PhoneEquipField.ValueInt > 0)
                display = 1;

            if (!string.IsNullOrEmpty(Dir.SotrudnikField.ValueString) && Dir.Sotrudnik.SimRequired
                || Dir.WorkPlaceTypeField.ValueInt == (int) DirectionsTypeBitEnum.ПереездНаДругоеРабочееМесто
                || Dir.WorkPlaceTypeField.ValueInt == (int) DirectionsTypeBitEnum.ИзменениеУчетнойЗаписи
                || Dir.WorkPlaceTypeField.ValueInt == (int) DirectionsTypeBitEnum.УчетнаяЗаписьСотрудникаГруппы
                || Dir.WorkPlaceTypeField.ValueInt == (int)DirectionsTypeBitEnum.УчетнаяЗаписьНаГостевомМесте
            ) display = 0;

            if (display == 1)
                FillRedirectNumber();

            RefreshMobilPhoneRequired(display);

            JS.Write("directions_Data_MobilPhone({0});", display);
        }

        private void RefreshMobilPhoneRequired(int display)
        {
            if (Dir.RedirectNumField.ValueString.Length == 0 && display == 1 &&
                Dir.SotrudnikField.ValueString.Length > 0) efRedirectNum.IsRequired = !Dir.Sotrudnik.SimRequired;
        }

        /// <summary>
        ///     Установка значения поля объекта CompTypeField из нескольких контролов
        /// </summary>
        /// <param name="bit">Что изменили</param>
        /// <param name="whatdo">Как изменили</param>
        private void SetCompType(int bit, string whatdo)
        {
            var bitMask = Dir.CompTypeField.ValueInt;

            if (whatdo.Equals("0"))
                bitMask ^= bit;
            else
                bitMask |= bit;

            Dir.CompTypeField.ValueString = bitMask == 0 ? "" : bitMask.ToString();
            GetCompType();
        }

        /// <summary>
        ///     Установка значений элементам управления на основании CompTypeField
        /// </summary>
        private void GetCompType()
        {
            var bitMask = Dir.CompTypeField.ValueInt;

            if ((bitMask & 2) == 2) efComputer.Value = "1";
            else efComputer.Value = "0";

            if ((bitMask & 4) == 4) efNotebook.Value = "1";
            else efNotebook.Value = "0";


            if (!V4IsPostBack)
            {
                efComputer.OriginalValue = efComputer.Value;
                efNotebook.OriginalValue = efNotebook.Value;
            }
        }

        /// <summary>
        ///     Открытие формы выбора расположений
        /// </summary>
        private void AdvSearchWorkPlace()
        {
            var parameters = "WorkPlace=1&hidemenu=1";
            ReturnDialogResult.ShowAdvancedDialogSearch(this, "directions_AdvSearchWorkPlace", "",
                Config.location_search, parameters, false, 0, 800, 600);
        }

        private void AdvSearchWorkPlaceTo()
        {
            var parameters = "WorkPlace=1&hidemenu=1";
            ReturnDialogResult.ShowAdvancedDialogSearch(this, "directions_AdvSearchWorkPlaceTo", "",
                Config.location_search, parameters, false, 0, 800, 600);
        }

        private void SetWorkPlaceTo(string value, string label, bool setValue)
        {
            if (value.Length == 0) return;

            if (setValue)
            {
                var bitMask = (int) DirectionsTypeBitEnum.ПереездНаДругоеРабочееМесто;

                Dir.WorkPlaceToField.Value = value;
                Dir.WorkPlaceTypeField.Value = bitMask;
                Dir.IsModified = true;
            }

            RefreshWorkPlace8_LinkInfo(label, setValue);
            SetWorkPlaceInfo(2);
            RefreshSotrudnikCadrWorkPlaces();
            CloseInfoDialogWorkPlace();
        }

        private void SetWorkPlaceInfo(int inx)
        {
            if (Dir.SotrudnikField.ValueString.Length == 0) return;
            var wp = inx == 1 ? Dir.WorkPlaceField.ValueString : Dir.WorkPlaceToField.ValueString;
            if (string.IsNullOrEmpty(wp)) return;

            var sqlParams = new Dictionary<string, object>
                {{"@КодСотрудника", Dir.SotrudnikField.Value}, {"@КодРасположения", wp}};
            var dt = DBManager.GetData(SQLQueries.SELECT_ID_ТипыОборудованияСотрудникаНаРасположении, Config.DS_user,
                CommandType.Text, sqlParams);

            if (dt.Rows.Count == 0) return;

            var rows = dt.AsEnumerable();
            var existComp = false;
            var existPhone = false;
            var existVideo = false;


            if (rows.Any(x => int.Parse(x["ЕстьТелефонныйНомер"].ToString()) > 0))
            {

                existPhone = true;
                SetPhoneEquip(1, "1");
                var line = int.Parse(rows.Max(x => x["УслугиСвязи"]).ToString());

                var lineValue = 0;
                switch (line)
                {
                    case 0:
                        lineValue = 1;
                        break;
                    case 4:
                        lineValue = 15;
                        break;
                    default:
                        lineValue = 7;
                        break;
                }

                Dir.PhoneLinkField.Value = lineValue;

                if (rows.Any(x => x["ТипОборудования"].ToString().Contains("Видеокамера")))
                {
                    existVideo = true;
                    SetPhoneEquip(8, "1");
                }

                var phone = rows.FirstOrDefault(x => x["Переадресация"].ToString().Length > 0);
                if (phone != null)
                {
                    Dir.RedirectNumField.ValueString = phone["Переадресация"].ToString();

                    var direction = "";
                    FormatingRedirectNumber(ref direction);
                    RefreshMobilphoneArea(direction);
                }
                else
                {
                    RefreshMobilPhoneRequired(1);
                }

                DisplayDataPhoneDesk(true);
            }

            if (rows.Any(x => int.Parse(x["ЕстьХарактеристикиКомпьютера"].ToString()) > 0)) {
                existComp = true;
                SetCompType(2, "1");
            }

            if (rows.Any(x => x["ТипОборудования"].ToString().Contains("Notebook"))) {
                existComp = true;
                SetCompType(4, "1");
            }

            var advs = new List<string>();
                        
            foreach (var dr in rows)
            {
                var cnt = int.Parse(dr["N"].ToString());
                var typeName = dr["ТипОборудования"];
                var item = "{0}-{1}шт.";
                switch (typeName)
                {
                    case "Компьютер":                        
                    case "Монитор":
                        if (existComp) cnt--;            
                        break;
                    case "Телефон":
                        if (existPhone) cnt--;
                        break;
                    case "Видеокамера":
                        if (existVideo) cnt--;
                        break;                    
                }

                if (cnt <= 0) continue;

                advs.Add(string.Format(item, typeName, cnt));
            }

            if (advs.Count > 0 )
                Dir.AdvEquipField.ValueString = String.Join("; ", advs.ToArray());
        }

        private void CreateNewEmployeeWorkplace()
        {
            if (IsKadrovik() && Dir.Sotrudnik.Workplaces != null && !Dir.Sotrudnik.Workplaces.Any(x => x.Id.Equals(Dir.WorkPlaceField.ValueString)))
            {
                ShowConfirm(Dir.Resx.GetString("DIRECTIONS_Msg_СотрудникНетРабМеста_2_0") + Environment.NewLine + Dir.Resx.GetString("DIRECTIONS_Msg_СотрудникНетРабМеста_2"), "Сообщение", "Да", "Нет", "cmdasync(\"cmd\", \"CreateWorkPlace\");", "cmdasync(\"cmd\", \"ClearSotrudnik\");", "efSotrudnik_0", 400);

            }

        }

        private void SetTransfer(string value, string label)
        {
            var employee = Dir.Sotrudnik;
            var wps = employee.Workplaces.Count(x => x.IsOrganized);
            if (wps == 1)
            {
                var wp = employee.Workplaces.FirstOrDefault(x => x.IsOrganized);
                if (wp != null)
                {
                    SetWorkPlaceTo(wp.Id, wp.NamePath1, true);
                    SetDocumentName();
                    CreateNewEmployeeWorkplace();
                    return;
                }
            }

            RefreshWorkPlace8_LinkInfo(label, false);
            var bitMask = (int) DirectionsTypeBitEnum.ПереездНаДругоеРабочееМесто;
            RenderWorkPlaceList(bitMask, false);
            SetDocumentName();
            CreateNewEmployeeWorkplace();
        }

        /// <summary>
        ///     Установка выбранного рабочего места в поле объекта и обновление надписи на форме
        /// </summary>
        /// <param name="value">Идентификатор рабочего места</param>
        /// <param name="label">Название рабочего места</param>
        /// <param name="checkCoWorkers">Проверять сотрудников на рабочем месте</param>
        private void SetWorkPlace(string value, string label, bool checkCoWorkers)
        {
            if (value.Length == 0) return;
            if (Dir.WorkPlaceTypeField.ValueString.Length == 0) return;

            Dir.WorkPlaceField.Value = value;
            Dir.WorkPlaceToField.Value = null;

            Dir.IsModified = true;

            var employee = Dir.Sotrudnik;
            var l = Dir.LocationWorkPlace;
            if (l == null || l.Unavailable)
            {
                ShowMessage($"Рабочее место #{value} не найдено или не доступно!",
                    Resx.GetString("CONFIRM_StdTitle"), MessageStatus.Error);
                return;
            }

            var wps = Dir.Sotrudnik.Workplaces;
            var isOwnWp = wps.Any(x => x.Id == l.Id);

            if (l.IsOrganized && Dir.Sotrudnik.HasAccountValid && !isOwnWp)
            {
                ShowMessage(
                    string.Format(Dir.Resx.GetString("DIRECTIONS_Msg_РаботаУжеОрганизована"), l.NamePath1),
                    Resx.GetString("CONFIRM_StdTitle"), MessageStatus.Information, "btnWPL_Add", 553, null,
                    "cmdasync('cmd', 'AdvSearchWorkPlace');");
                Dir.WorkPlaceField.Value = "";
                Dir.LoadedExternalProperties.Remove(Direction.CacheKey_LocationWorkPlace);
                return;
            }

            if (l.IsGroupWorkplace)
            {
                ShowMessage(
                    string.Format(Dir.Resx.GetString("DIRECTIONS_Msg_РабочееМестоГруппы"), l.NamePath1),
                    Resx.GetString("CONFIRM_StdTitle"), MessageStatus.Information, "btnWPL_Add", 553, null,
                    "cmdasync('cmd', 'AdvSearchWorkPlace');");
                Dir.WorkPlaceField.Value = "";
                Dir.LoadedExternalProperties.Remove(Direction.CacheKey_LocationWorkPlace);
                return;
            }

            if (checkCoWorkers && l.CoWorkers.Count <= 1 &&
                employee.Workplaces.Any(x => x.IsOrganized && x.Id != value))
            {
                var linkEmpl = "";
                var linkLoc = "";

                using (var w = new StringWriter())
                {
                    RenderLinkEmployee(w, $"le{employee.Id}", employee, NtfStatus.Empty, false);
                    linkEmpl = w.ToString();
                }

                using (var w = new StringWriter())
                {
                    RenderLinkLocation(w, $"ll{l.Id}", l.Id, l.NamePath1_WhiteSpace, NtfStatus.Empty, "открыть расположение", "", "setworkplace");
                    linkLoc = w.ToString();
                }


                ShowConfirm(
                    $"Вы хотите организовать переезд сотрудника {linkEmpl} на рабочее место {linkLoc} или организовать ему там дополнительное рабочее место?",
                    Resx.GetString("CONFIRM_StdTitle"),
                    "Организовать переезд", "Организовать доп. рабочее место",
                    $"directions_SetTransfer({l.Id}, '{HttpUtility.JavaScriptStringEncode(l.NamePath1)}');",
                    $"directions_SetWorkPlace({l.Id},'{HttpUtility.JavaScriptStringEncode(l.NamePath1)}', 0);",
                    "", 160, 240, 503, 153, true, false, "Отмена", "directions_SetWorkPlace1();");

                return;
            }

            var enumValue = DirectionsTypeBitEnum.РабочееМестоВОфисе;
            if (Dir.WorkPlaceTypeField.ValueInt == (int)DirectionsTypeBitEnum.ПереездНаДругоеРабочееМесто)
                enumValue = DirectionsTypeBitEnum.ПереездНаДругоеРабочееМесто;
            else if (Dir.WorkPlaceTypeField.ValueInt == (int)DirectionsTypeBitEnum.РабочееМестоВнеОфиса)
                enumValue = DirectionsTypeBitEnum.РабочееМестоВнеОфиса;
            else if (Dir.WorkPlaceTypeField.ValueInt == (int)DirectionsTypeBitEnum.УчетнаяЗаписьСотрудникаГруппы)
                enumValue = DirectionsTypeBitEnum.УчетнаяЗаписьСотрудникаГруппы;
            else if (Dir.WorkPlaceTypeField.ValueInt == (int)DirectionsTypeBitEnum.ИзменениеУчетнойЗаписи)
                enumValue = DirectionsTypeBitEnum.ИзменениеУчетнойЗаписи;
            else if (Dir.WorkPlaceTypeField.ValueInt == (int)DirectionsTypeBitEnum.УчетнаяЗаписьНаГостевомМесте)
                enumValue = DirectionsTypeBitEnum.УчетнаяЗаписьНаГостевомМесте;

            if (!SetWorkPlaceType(enumValue)) return;



            efSotrudnik.RenderNtf();

            SetAccessEthernetState();
            DisplayDataWorkPlace();


            SetRequiredLogin();
            SetRequiredMailName();
            DisplayDataEthernet();

            RefreshWorkPlaceTypeInfo();

            JS.Write("directions_CloseWorkPlaceList();");

            RefreshSotrudnikCadrWorkPlaces();
            CloseInfoDialogWorkPlace();

            CreateNewEmployeeWorkplace();

        }

        /// <summary>
        ///     Очистка рабочего места и поля объекта и надписи на форме
        /// </summary>
        private void ClearWorkPlace()
        {
            Dir.WorkPlaceField.Value = null;
            Dir.WorkPlaceToField.Value = null;
            CheckWorkplaceSelected();

            RefreshWorkPlaceTypeInfo();

            RefreshSotrudnikCadrWorkPlaces();
        }

        private void CheckWorkplaceSelected()
        {
            if (Dir.WorkPlaceField.ValueString.Length == 0)
            {
                Dir.WorkPlaceTypeField.ValueString = "";
                Dir.WorkPlaceToField.Value = null;
                JS.Write("directions_clearRadio('divWorkPlaceType');");
                DisplayDataWorkPlace();
                JS.Write("directions_setElementFocus(\"RDT\",\"\",\"radio\");");
                SetDocumentName();
                return;
            }

            var bitMak = Dir.WorkPlaceTypeField.ValueInt;

            if (Dir.WorkPlaceToField.ValueString.Length == 0
                && bitMak == (int) DirectionsTypeBitEnum.ПереездНаДругоеРабочееМесто)
                Dir.WorkPlaceTypeField.Value = (int) DirectionsTypeBitEnum.РабочееМестоВОфисе;

            RefreshWorkPlaceTypeInfo();
            SetDocumentName();
        }


        /// <summary>
        ///     Проставление почтового домена по-умолчанию для выбранного сотрудника
        /// </summary>
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

            var emplPersonId = empl.OrganizationId.ToString();
            if (emplPersonId.Length == 0) return;

            StringCollection colPersons;

            Dir.DomainNames.ForEach(delegate(DomainName dName)
            {
                if (string.IsNullOrEmpty(dName.PersonIds)) return;
                colPersons = Convert.Str2Collection(dName.PersonIds);
                foreach (var t in from string t in colPersons where t.Equals(emplPersonId) select t) colD.Add(dName.Id);
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


            if (Dir.DomainField.ValueString.Length == 0 && colD.Count == 0) Dir.DomainField.Value = Config.domain;
        }

        /// <summary>
        ///     Отрисовка рекомендованных имен почтового ящика
        /// </summary>
        private void RenderMailNamesList()
        {
            var w = new StringWriter();

            if (Dir.SotrudnikField.ValueString.Length == 0)
            {
                ShowMessage($"{Dir.Resx.GetString("DIRECTIONS_Msg_Err_НетСотрудника")}!", efSotrudnik,
                    Resx.GetString("CONFIRM_StdTitle"));
                return;
            }

            var fl = false;
            var empl = Dir.Sotrudnik;

            if (!empl.Unavailable)
            {
                if (empl.HasAccountValid) return;

                fl = true;

                if (empl.FirstNameEn.Length > 0 && empl.LastNameEn.Length > 0)
                {
                    var name = $"{(empl.FirstNameEn.Length > 0 ? empl.FirstNameEn + "." : "")}{empl.LastNameEn}"
                        .Replace(" ", "");
                    w.Write("<div noWrap>");
                    w.Write("<nobr>");
                    w.Write(
                        "<a id='imgMN_{1}' tabindex=0 onkeydown='var key=v4_getKeyCode(event); if(key == 13 || key == 32) directions_SetMailName(\"{0}\");' onclick='directions_SetMailName(\"{0}\");'><img src='/styles/backtolist.gif' border='0'/></a>",
                        HttpUtility.JavaScriptStringEncode(name), 1);
                    w.Write("<span class='marginL'>");
                    w.Write(HttpUtility.HtmlEncode(name));
                    w.Write("</span>");
                    w.Write("</nobr>");
                    w.Write("</div>");
                }
            }


            if (!fl)
                RenderNtf(w,
                    new List<Notification>
                    {
                        new Notification
                        {
                            Message = Dir.Resx.GetString("DIRECTIONS_Msg_Err_MailBox"),
                            Status = NtfStatus.Error,
                            SizeIsNtf = false,
                            DashSpace = false,
                            Description = Dir.Resx.GetString("DIRECTIONS_Msg_Err_MailBox_Title")
                        }
                    });

            RefreshControlText(w, "divMN_Body");
            JS.Write("directions_SetMailNamesList();");
        }

        /// <summary>
        ///     Проставление выбранного значения имени почтового ящика
        /// </summary>
        /// <param name="value">Значение</param>
        private void SetMailName(string value)
        {
            if (value.Length == 0) return;
            Dir.MailNameField.Value = value;
            efMailName.RenderNtf();
        }

        /// <summary>
        ///     Установка значения поля объекта SotrudnikParentCheckField из нескольких контролов
        /// </summary>
        /// <param name="bit">Что изменили</param>
        /// <param name="whatdo">Как изменили</param>
        private void SetSotrudnikParentType(DirectionsSotrudnikParentBitEnum bit, string whatdo)
        {
            var bitMask = Dir.SotrudnikParentCheckField.ValueInt;

            if (whatdo.Equals("0"))
                bitMask ^= (int) bit;
            else
                bitMask |= (int) bit;

            if (bit == DirectionsSotrudnikParentBitEnum.КакУСотрудника &&
                (bitMask & (int) DirectionsSotrudnikParentBitEnum.ВместоСотрудника) ==
                (int) DirectionsSotrudnikParentBitEnum.ВместоСотрудника)
                bitMask ^= (int) DirectionsSotrudnikParentBitEnum.ВместоСотрудника;

            if (bit == DirectionsSotrudnikParentBitEnum.ВместоСотрудника &&
                (bitMask & (int) DirectionsSotrudnikParentBitEnum.КакУСотрудника) ==
                (int) DirectionsSotrudnikParentBitEnum.КакУСотрудника)
                bitMask ^= (int) DirectionsSotrudnikParentBitEnum.КакУСотрудника;

            Dir.SotrudnikParentCheckField.ValueString = bitMask == 0 ? "" : bitMask.ToString();
            GetSotrudnikParentType();
            efSotrudnikParent.RenderNtf();
        }

        /// <summary>
        ///     Установка значений элементам управления на основании SotrudnikParentCheckField
        /// </summary>
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
            {
                JS.Write("$('#divSotrudnikParent').hide();");
            }
        }

        /// <summary>
        ///     Обновление ссылок на сотрудника
        /// </summary>
        /// <param name="linkId">Идентификатор ссылки</param>
        /// <param name="containerId">Идентификатор контейнера</param>
        private void RefreshSotrudnikLink(string linkId, string containerId)
        {
            var w = new StringWriter();
            RenderLinkEmployee(w, linkId, Dir.Sotrudnik, NtfStatus.Empty);
            RefreshControlText(w, containerId);
        }

        /// <summary>
        ///     Очистка информации о сотруднике
        /// </summary>
        /// <param name="clearRadio">0 - не очищать; 1-серверная очистка; 2-серверная и клиентская очистка</param>
        private void SotrudnikClearInfo(int clearRadio)
        {
            Dir.AccessEthernetChange = false;
            Dir.WorkPlaceTypeField.Value = null;
            Dir.WorkPlaceField.Value = null;
            Dir.WorkPlaceToField.Value = null;

            Dir.PhoneEquipField.Value = null;
            Dir.RedirectNumField.Value = null;
            GetPhoneEquip();

            Dir.CompTypeField.Value = null;
            GetCompType();

            Dir.AdvEquipField.Value = null;
            Dir.AdvInfoField.Value = null;

            Dir.AccessEthernetField.Value = null;
            Dir.LoginField.Value = null;
            Dir.MailNameField.Value = null;
            Dir.SotrudnikLanguageField.Value = null;

            Dir.SotrudnikParentField.Value = null;
            Dir.SotrudnikParentCheckField.Value = null;
            GetSotrudnikParentType();

            Dir.PositionCommonFolders.Clear();
            Dir.PositionRoles.Clear();
            Dir.PositionTypes.Clear();
            Dir.PositionAdvancedGrants.Clear();

            _isCommonEmployeeNoLogin = false;
            efAccessEthernet.IsDisabled = rdWorkPlaceType1.IsDisabled =
                rdWorkPlaceType3.IsDisabled = rdWorkPlaceType5.IsDisabled = false;

            JS.Write("$(\"#divWorkPlace\").html('');");
            JS.Write("$(\"#divWorkPlace2\").html('');");
            JS.Write("$(\"#divWorkPlace4\").html('');");

            SetDocumentName();

            if (clearRadio > 0)
            {
                DisplayDataWorkPlace();
                DisplayMobilPhone();
                RefreshWorkPlaceTypeInfo();
                RefreshPositionCommonFolders();
                RefreshPositionRoles();
                RefreshPositionTypes();
                RefreshPositionAdvancedGrantsAvailable();

                rdWorkPlaceType1.Value = "";
                rdWorkPlaceType3.Value = "";
                rdWorkPlaceType5.Value = "";

                if (clearRadio == 2) JS.Write("directions_clearRadio('divWorkPlaceType');");
            }
        }

        /// <summary>
        ///     Получение информации о сотруднике и заполнение соотвествующих элементов управления
        /// </summary>
        private void SotrudnikSetInfo()
        {           

            if (Dir.SotrudnikField.ValueString.Length == 0 || Dir.Sotrudnik == null || Dir.Sotrudnik.Unavailable)
            {
                Dir.Description = "";
                return;
            }
            var empl = Dir.Sotrudnik;
                      

            SetAccessEthernetState();

            DisplayDataWorkPlace();

            SetRequiredLogin();
            SetRequiredMailName();
            DisplayDataEthernet();

            Dir.SotrudnikLanguageField.Value = empl.Language;
            Dir.Description = string.IsNullOrEmpty(Dir.Description)? empl.FIO: Dir.Description;
        }

        /// <summary>
        ///     Поулчение информации об учетной записи сотрудника и заполнение соотвествующих элементов управления
        /// </summary>
        private void SetEthernetData()
        {
            var empl = Dir.Sotrudnik;
            if (empl == null || empl.Unavailable) return;

            if (empl.HasAccount_)
                Dir.LoginField.Value = empl.Login;
            else
                Dir.LoginField.Value = GetFreeLogin();


            if (empl.Email.Length > 0 )
            {               
                var inx = empl.Email.IndexOf("@", StringComparison.Ordinal);
                var mailName = empl.Email.Substring(0, inx);
                var domainName = empl.Email.Substring(inx + 1).ToLower();
                var domainNameIsCorrect = empl.CheckEmailDomainName(domainName, Dir.DomainNames);

                if (empl.HasAccountValid && domainNameIsCorrect)
                {
                    Dir.MailNameField.Value = mailName;
                    Dir.DomainField.Value = domainName;
                }
            }

            if (string.IsNullOrEmpty(Dir.MailNameField.ValueString) && !string.IsNullOrEmpty(Dir.LoginField.ValueString))
                Dir.MailNameField.Value =
                    $"{(empl.FirstNameEn.Length > 0 ? empl.FirstNameEn + "." : "")}{empl.LastNameEn}".Replace(" ", "");

            if (string.IsNullOrEmpty(Dir.DomainField.ValueString))
                SetDefaultDomainByEmployee();
            
            if (string.IsNullOrEmpty(Dir.SotrudnikLanguageField.ValueString))
                Dir.SotrudnikLanguageField.ValueString = empl.Language;


            FillPositionAdvancedGrantsByEmployee(empl);
            FillPositionCommonFoldersByEmployee(empl);
            FillPositionRolesByEmployee(empl);
            FillPositionTypesByEmployee(empl);

            efLang.RefreshRequired = true;
            efLogin.RefreshRequired = true;
            efMailName.RefreshRequired = true;
            efDomain.RefreshRequired = true;
        }

        /// <summary>
        ///     Обновление связанной с выбранным сотрудником информации
        /// </summary>
        private void RefreshSotrudnikInfo()
        {
            RefreshPhoto();

            RefreshSotrudnikPost();
            RefreshSotrudnikFinOrg();
            RefreshCommonEmployee();

            RefreshSotrudnikCadrWorkPlaces();

            RefreshSotrudnikPost();
            RefreshSupervisorInfo();

            RefreshMobilphoneArea();

            SotrudnikSetInfo();

            DisplayAdvInfo();
        }


        /// <summary>
        ///     Получение информации о сотруднике "как/вместо" и заполение соотвествующих элементов управления
        /// </summary>
        private void SotrudnikParentSetInfo()
        {
            if (Dir.SotrudnikParentField.ValueString.Length == 0) return;

            ClearDocumentPositions();
            FillPositionAdvancedGrantsByEmployee(Dir.SotrudnikParent);
            FillPositionCommonFoldersByEmployee(Dir.SotrudnikParent);
            FillPositionRolesByEmployee(Dir.SotrudnikParent);
            FillPositionTypesByEmployee(Dir.SotrudnikParent);
            if (!Dir.IsNew)
            {                
                Dir.Save(true);
                Dir.SaveDocumentPositions(true);
                DocumentToControls();
            }
        }

        /// <summary>
        ///     Очистка объектов позиций текущего документа
        /// </summary>
        private void ClearDocumentPositions()
        {
            Dir.PositionAdvancedGrants.Clear();
            Dir.PositionTypes.Clear();
            Dir.PositionRoles.Clear();
            Dir.PositionCommonFolders.Clear();
        }

        private void CloseInfoDialogWorkPlace()
        {
            JS.Write("directions_clearData = false; directions_CloseInfoWorkPlaceType();");
        }


        /// <summary>
        /// Является ли текущий пользователь Кадровик
        /// </summary>
        /// <returns></returns>
        private bool IsKadrovik()
        {
            var isKadrovik = CurrentUser.HasRole((int)Kesco.Lib.BaseExtention.Enums.Corporate.Role.Кадровик, 0);

            if (!isKadrovik)
            {
                var persons = Dir.Sotrudnik.PersonsIds(7);
                persons.ForEach(x =>
                {
                    if (!isKadrovik)
                        isKadrovik = CurrentUser.HasRole((int)Kesco.Lib.BaseExtention.Enums.Corporate.Role.Кадровик, x);
                    else return;

                });
            }

            return isKadrovik;
        }

        private void CreateWorkPlace() {

            if (string.IsNullOrEmpty(Dir.SotrudnikField.ValueString))
            {
                ShowMessage("Невозиожно создать рабочее место, т.к. не указан сотрудник!", "Ошибка", MessageStatus.Error);
                return;
            }

            if (string.IsNullOrEmpty(Dir.WorkPlaceField.ValueString))
            {
                ShowMessage("Невозиожно создать рабочее место, т.к. не выбрано рабочее место!", "Ошибка", MessageStatus.Error);
                return;
            }

            var sqlParams = new Dictionary<string, object>
                {
                    { "@КодСотрудника", Dir.SotrudnikField.ValueInt},
                    { "@КодРасположения",  Dir.WorkPlaceField.ValueInt}
                };

            try
            {
                DBManager.ExecuteNonQuery(SQLQueries.INSERT_РабочееМестоСотруднику, CommandType.Text, Config.DS_user, sqlParams);
            }
            catch (Exception ex)
            {
                ShowMessage(ex.Message, "Error", MessageStatus.Error);
                return;
            }
            finally
            {
                RefreshNotification();
            }

        }

        #endregion
    }
}