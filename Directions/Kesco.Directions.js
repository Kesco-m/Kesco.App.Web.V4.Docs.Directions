//================================================= Управление
var directions_clientLocalization = {};

function directions_setElementFocus(className, elId) {
    var obj;
    if (elId != null && elId.length > 0)
        obj = $("#" + elId);
    else
        obj = $("." + className).first();

    if (obj) obj.focus();
}

function directions_setAttribute(elId, attrName, attrValue) {
    var obj = $("#" + elId);
    if (!obj) return;
    $(obj).attr(attrName, attrValue);

}

function directions_getAttribute(elId, attrName) {
    var obj = $("#" + elId);
    if (!obj) return "";

    return $(obj).attr(attrName);
}


//================================================= Отображение блоков телефонных номеров
function directions_SetDivPhoneStyle(addClass, removeClass) {

    if (addClass != null && addClass != "")
        $("#divPLExit").addClass(addClass);
    if (removeClass != null && removeClass != "")
        $("#divPLExit").removeClass(removeClass);
}

//================================================= Отображение блока Internet

function directions_Data_Lan(x) {

    if (x == 0) $('[data-lan]').not('[data-lan=""]').hide();
    else $('[data-lan]').not('[data-lan=""]').show();
}

//================================================= Отображение блока Мобильный телефон

function directions_Data_MobilPhone(x) {

    if (x == 0) $('[data-phone]').not('[data-phone=""]').hide();
    else $('[data-phone]').not('[data-phone=""]').show();
}

//================================================= Выбор рабочего места

directions_SetPositionWorkPlaceList.form = null;
function directions_SetPositionWorkPlaceList() {

    if (null == directions_SetPositionWorkPlaceList.form) {

        var title = directions_clientLocalization.DIRECTIONS_FORM_WP_Title;
        var width = 450;
        var height = 150;
        var onOpen = function() { directions_setElementFocus("WP"); };
        var buttons = [
            {
                id: "btnWPL_Add",
                text: directions_clientLocalization.cmdWorkplaceOther,
                icons: {
                    primary: v4_buttonIcons.Document
                },
                click: function() { cmdasync('cmd', 'AdvSearchWorkPlace'); }
            },
            {
                id: "btnWPL_Cancel",
                text: directions_clientLocalization.cmdCancel,
                icons: {
                    primary: v4_buttonIcons.Cancel
                },
                width: 75,
                click: directions_CloseWorkPlaceList
            }
        ];

            directions_SetPositionWorkPlaceList.form = v4_dialog("divWorkPlaceList", $("#divWorkPlaceList"), title, width, height, onOpen, null, buttons);
            
        

    }

    directions_SetPositionWorkPlaceList.form.dialog("open");
}


function directions_Data_WP(mask, lan) {
    $('[data-wp]').not('[data-wp=""]').each(function (index, item) {
        var value = parseInt($(item).attr("data-wp"));
        
        if ((value & mask) == 0) {
            $(item).hide();
        } else {
            if ($(item).attr("data-lan") && lan == 0) return;
            $(item).show();
        }
    });

}

function directions_SetWorkPlace(value, label) {
    if (value == null && value.length == 0) return;
    if (label == null) label = "";
    cmdasync("cmd", "SetWorkPlace", "value", value, "label", label);
    directions_CloseWorkPlaceList();
}

function directions_ClearWorkPlace() {
    cmdasync("cmd", "ClearWorkPlace");
}

function directions_CloseWorkPlaceList() {
    if (null != directions_SetPositionWorkPlaceList.form)
        directions_SetPositionWorkPlaceList.form.dialog("close");
    directions_setElementFocus(null, "efWorkPlaceType4_0");
}

function directions_AdvSearchWorkPlace(control, callbackKey, result, isMultiReturn) {
    if (result.length == 0) return;
    directions_SetWorkPlace(result[0].value, result[0].label);
}


//================================================= Выбор имени почтового ящика

directions_SetMailNamesList.form = null;
function directions_SetMailNamesList() {

    if (null == directions_SetMailNamesList.form) {
        var title = directions_clientLocalization.DIRECTIONS_FORM_Mail_Title;
        var width = 251;
        var height = 131;
        var onOpen = function() { directions_setElementFocus(null, "imgMN_0"); };
        var buttons = [
            {
                id: "btnMail_Cancel",
                text: directions_clientLocalization.cmdCancel,
                icons: {
                    primary: v4_buttonIcons.Cancel
                },
                width: 75,
                click: directions_CloseMailNameList
            }
        ];
        var dialogPostion = { my: "top", at: "bottom", of: $('#efMailName_0') };
        directions_SetMailNamesList.form = v4_dialog("divMailNames", $("#divMailNames"), title, width, height, onOpen, null, buttons, dialogPostion);

        
    }

    directions_SetMailNamesList.form.dialog("open");
}

function directions_SetMailName(value, label) {
    if (value == null && value.length == 0) return;
    if (label == null) label = "";
    cmdasync("cmd", "SetMailName", "value", value, "label", label);
    directions_CloseMailNameList();
}

function directions_CloseMailNameList() {
    if (null != directions_SetMailNamesList.form)
        directions_SetMailNamesList.form.dialog("close");
    directions_setElementFocus(null, "efMailName_0");
}

//================================================= Позиции Роли

directions_SetPositionRolesAdd.form = null;
function directions_SetPositionRolesAdd() {

    if (null == directions_SetPositionRolesAdd.form) {
        var title = directions_clientLocalization.DIRECTIONS_FORM_Role_Title;
        var width = 465;
        var height = 300;
        var onOpen = function() { directions_setElementFocus(null, "efPRoles_Role_0"); };
        var buttons = [
            {
                id: "btnPRoles_Save",
                text: directions_clientLocalization.cmdOK,
                icons: {
                    primary: v4_buttonIcons.Ok
                },
                width: 75,
                click: function() {
                    var value = directions_getAttribute("divPositionRolesAdd", "data-id");
                    cmdasync("cmd", "SavePositionRole", "value", value, "check", 1);
                }
            },
            {
                id: "btnPRoles_Delete",
                text: directions_clientLocalization.cmdDelete,
                icons: {
                    primary: v4_buttonIcons.Delete
                },
                width: 75,
                click: function () {
                    v4_showConfirm( directions_clientLocalization.CONFIRM_StdMessage,
                                    directions_clientLocalization.CONFIRM_StdTitle,
                                    directions_clientLocalization.CONFIRM_StdCaptionYes, 
                                    directions_clientLocalization.CONFIRM_StdCaptionNo, 
                                    directions_deleteRoleCallBack(directions_getAttribute("divPositionRolesAdd", "data-id"), 1), 400);
                }
            },
            {
                id: "btnPRoles_Cancel",
                text: directions_clientLocalization.cmdCancel,
                icons: {
                    primary: v4_buttonIcons.Cancel
                },
                width: 75,
                click: directions_ClosePositionRolesAdd
            }
        ];

            directions_SetPositionRolesAdd.form = v4_dialog("divPositionRolesAdd", $("#divPositionRolesAdd"), title, width, height, onOpen, null, buttons);
            
    }

    directions_SetPositionRolesAdd.form.dialog("open");
}

function directions_ClosePositionRolesAdd(setFocus) {
    if (null != directions_SetPositionRolesAdd.form)
        directions_SetPositionRolesAdd.form.dialog("close");
    directions_setAttribute("divPositionRolesAdd", "data-id", "");
    directions_setElementFocus(null, setFocus != null ? setFocus : 'btnLinkRolesAdd');
}

//================================================= Позиции Типы

directions_SetPositionTypesAdd.form = null;
function directions_SetPositionTypesAdd() {

    if (null == directions_SetPositionTypesAdd.form) {

        var title = directions_clientLocalization.DIRECTIONS_FORM_Type_Title;
        var width = 450;
        var height = 250;
        var onOpen = function() { directions_setElementFocus(null, "efPTypes_Catalog_0"); };
        var buttons = [
            {
                id: "btnPTypes_Save",
                text: directions_clientLocalization.cmdOK,
                icons: {
                    primary: v4_buttonIcons.Ok
                },
                width: 75,
                click: function() { cmdasync('cmd', 'SavePositionType', 'check', 1); }
            },
            {
                id: "btnPTypes_Delete",
                text: directions_clientLocalization.cmdDelete,
                icons: {
                    primary: v4_buttonIcons.Delete
                },
                width: 75,
                click: function() {
                    v4_showConfirm( directions_clientLocalization.CONFIRM_StdMessage,
                                    directions_clientLocalization.CONFIRM_StdTitle,
                                    directions_clientLocalization.CONFIRM_StdCaptionYes,
                                    directions_clientLocalization.CONFIRM_StdCaptionNo, 
                                    directions_deleteTypeAllCallBack(directions_getAttribute("divPositionTypesAdd", "data-id"), 1), 400);

                }
            },
            {
                id: "btnPTypes_Cancel",
                text: directions_clientLocalization.cmdCancel,
                icons: {
                    primary: v4_buttonIcons.Cancel
                },
                width: 75,
                click: directions_ClosePositionTypesAdd
            }
        ];
            directions_SetPositionTypesAdd.form = v4_dialog("divPositionTypesAdd", $("#divPositionTypesAdd"), title, width, height, onOpen, null, buttons);

           

    }

    directions_SetPositionTypesAdd.form.dialog("open");
}


function directions_ClosePositionTypesAdd(setFocus) {
    if (null != directions_SetPositionTypesAdd.form)
        directions_SetPositionTypesAdd.form.dialog("close");
    directions_setElementFocus(null, setFocus != null ? setFocus : 'btnLinkTypesAdd');
}

//================================================= Позиции папки

directions_SetPositionCFAdd.form = null;
function directions_SetPositionCFAdd() {

    if (null == directions_SetPositionCFAdd.form) {
        var title = directions_clientLocalization.DIRECTIONS_FORM_CF_Title;
        var width = 341;
        var height = 351;
        var onOpen = function() { directions_setElementFocus("CF"); };
        var buttons = [
            {
                id: "btnCF_Clear",
                text: directions_clientLocalization.cmdUncheck,
                click: directions_PositionCF_Clear
            },
            {
                id: "btnCF_Save",
                text: directions_clientLocalization.cmdOK,
                icons: {
                    primary: v4_buttonIcons.Ok
                },
                width: 75,
                click: directions_PositionCF_Save
            },
            {
                id: "btnCF_Cancel",
                text: directions_clientLocalization.cmdCancel,
                icons: {
                    primary: v4_buttonIcons.Cancel
                },
                width: 75,
                click: directions_ClosePositionCFAdd
            }
        ];

        directions_SetPositionCFAdd.form = v4_dialog("divCommonFoldersAdd", $("#divCommonFoldersAdd"), title, width, height, onOpen, null, buttons);
        
    }

    directions_SetPositionCFAdd.form.dialog("open");
}


function directions_ClosePositionCFAdd() {
    if (null != directions_SetPositionCFAdd.form)
        directions_SetPositionCFAdd.form.dialog("close");
}


function directions_PositionCF_Save() {

    var selected = {};
    $(".div_cf input:checked")  .each(function() {
        var id = $(this).attr("data-id");
        var name = $(this).attr("data-name");
        selected[id] = name;
    });
    var value = JSON.stringify(selected);
    cmdasync("cmd", "SavePositionCommonFolders", "value", value);
    directions_PositionCF_Clear();
    directions_ClosePositionCFAdd();

    directions_setElementFocus(null, "btnLinkRolesAdd");
}

function directions_PositionCF_Clear() {
    $(".div_cf input:checked").each(function() {
        this.checked = false;
    });
}


//================================================= Позиции права

directions_SetPositionAGAdd.form = null;
function directions_SetPositionAGAdd() {

    if (null == directions_SetPositionAGAdd.form) {
        var title = directions_clientLocalization.DIRECTIONS_FORM_AG_Title;
        var width = 350;
        var height = 240;
        var onOpen = function() { directions_setElementFocus("AG"); };
        var buttons = [
            {
                id: "btnAG_Clear",
                text: directions_clientLocalization.cmdUncheck,
                click: directions_PositionAG_Clear
            },
            {
                id: "btnAG_Save",
                text: directions_clientLocalization.cmdOK,
                icons: {
                    primary: v4_buttonIcons.Ok
                },
                width: 75,
                click: directions_PositionAG_Save
            },
            {
                id: "btnAG_Cancel",
                text: directions_clientLocalization.cmdCancel,
                icons: {
                    primary: v4_buttonIcons.Cancel
                },
                width: 75,
                click: directions_ClosePositionAGAdd
            }
        ];

            directions_SetPositionAGAdd.form = v4_dialog("divAdvancedGrantAdd", $("#divAdvancedGrantAdd"), title, width, height, onOpen, null, buttons);
            
    }

    directions_SetPositionAGAdd.form.dialog("open");
}

function directions_ClosePositionAGAdd() {
    if (null != directions_SetPositionAGAdd.form)
        directions_SetPositionAGAdd.form.dialog("close");

}

function directions_PositionAG_Save() {

    var selected = {};
    $(".div_ag input:checked").each(function () {
        var id = $(this).attr("data-id");
        var name = $(this).attr("data-name");
        var nameEn = $(this).attr("data-name-en");
        selected[id] = name + '#' + nameEn;
    });
    var value = JSON.stringify(selected);
    cmdasync("cmd", "SavePositionAG", "value", value);
    directions_PositionAG_Clear();
    directions_ClosePositionAGAdd();

    directions_setElementFocus(null, "efAdvInfo_0");
}

function directions_PositionAG_Clear() {
    $(".div_ag input:checked").each(function() {
        this.checked = false;
    });
}

//================================================= Callback confirm
function directions_deleteTypeAllCallBack(id, close) {
    return 'cmdasync("cmd", "DeletePositionByCatalog", "catalog",' + id + ', "closeForm", "' + (close == 1 ? 1 : 0) + '");'; 
}

function directions_deleteTypeCallBack(catalog, theme) {
    return 'cmdasync("cmd", "DeletePositionType", "catalog", "' + catalog + '", "theme", "' + theme + '", "closeForm", "0");';
}

function directions_deleteAGCallBack(id) {
    return 'cmdasync("cmd", "DeletePositionAG", "value", "' + id + '");';
}

function directions_deleteCFCallBack(id) {
    return 'cmdasync("cmd", "DeletePositionCommonFolders", "value", "' + id + '");';
}

function directions_deleteRoleAllCallBack(id) {
    return 'cmdasync("cmd", "DeletePositionByRole", "role", "' + id + '");';
}
function directions_deleteRoleCallBack(id, close) {
    return 'cmdasync("cmd", "DeletePositionRoleByGuid", "guid", "' + id + '", "closeForm", "' + (close == 1 ? 1 : 0) + '");';
}

