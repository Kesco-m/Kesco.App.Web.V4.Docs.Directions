//================================================= Управление
var directions_clientLocalization = {};
var directions_clearData = null;

function directions_goTo(idDoc) {

    if (idDoc != -1)
        cmd("cmd", "GoTo", "idDoc", idDoc);
    else
        directions_CloseInfoWorkPlaceType();
}

function directions_clearRadio(divId) {
	setTimeout(function() {
			$("#" + divId + ' input[type="radio"]').prop("checked", false);
		},
		10);

}

function directions_setElementFocus(className, elId, inpType) {
	var obj;
    if (elId != null && elId.length > 0)
        obj = $("#" + elId);
    else {
        if (inpType)
            obj = $('.' + className + " input[type=\""+inpType+"\"]:visible").first();
        else
            obj = $("." + className).first();
    }
	//alert(elId);
	if (obj) setTimeout(function() { obj.focus(); }, 10);
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

function directions_Data_Lan(lan, user) {

	if (lan == 0) {
		$("[data-lan]").not('[data-lan=""]').hide();
	} else {
		$("[data-lan]").not('[data-lan=""]').each(function(index, item) {
			var datalan = parseInt($(item).attr("data-lan"));
			var datauser = $(item).attr("data-user") ? parseInt($(item).attr("data-user")) : 0;

			if ((datalan & lan) == lan && lan > 0) {
				if (datauser > 0) {
					if ((datauser & user) == user && user > 0) {
						$(item).show();
					} else {
						$(item).hide();
					}
                } else {                   
                     $(item).show();                   
				}
			}
		});
	}
}

//================================================= Отображение блока Мобильный телефон

function directions_Data_MobilPhone(x) {

	if (x == 0) $("[data-phone]").not('[data-phone=""]').hide();
	else $("[data-phone]").not('[data-phone=""]').show();
}

//================================================= Выбор рабочего места

directions_SetPositionWorkPlaceList.form = null;

function directions_SetPositionWorkPlaceList(canAnother) {

	var title = directions_clientLocalization.DIRECTIONS_FORM_WP_Title;
	var width = 453;
	var height = 150;
	var onOpen = function() { directions_setElementFocus("WP"); };
	var onClose = function() { directions_checkWorkplaceSelected(); };
	var buttons = [
		{
			id: "btnWPL_Add",
			text: directions_clientLocalization.cmdWorkplaceOther,
			icons: {
				primary: v4_buttonIcons.Document
			},
			click: function() { cmdasync("cmd", "AdvSearchWorkPlace"); }
		},
		{
			id: "btnWPL_Cancel",
			text: directions_clientLocalization.cmdCancel,
			icons: {
				primary: v4_buttonIcons.Cancel
			},
			width: 100,
			click: directions_CloseWorkPlaceList
		}
	];

	if (!canAnother)
		buttons = jQuery.grep(buttons,
			function(btn) {
				return (btn.id !== "btnWPL_Add");
			});


	directions_SetPositionWorkPlaceList.form = v4_dialog("divWorkPlaceList",
		$("#divWorkPlaceList"),
		title,
		width,
		height,
		onOpen,
		onClose,
		buttons);

	directions_SetPositionWorkPlaceList.form.dialog("open");
}


function directions_Data_WP(wp, lan, user, hasAccount) {

	$("[data-wp]").not('[data-wp=""]').each(function(index, item) {
		var datawp = $(item).attr("data-wp").split(";");

		var datalan = $(item).attr("data-lan") ? parseInt($(item).attr("data-lan")) : 0;
        var datauser = $(item).attr("data-user") ? parseInt($(item).attr("data-user")) : 0;
        var datahasaccount = $(item).attr("data-hasaccount") ? parseInt($(item).attr("data-hasaccount")) : -1;

		if (wp > 0 && $.inArray(wp.toString(), datawp) >= 0) {
			if (datalan > 0) {
				if ((datalan & lan) == lan && lan > 0) {
                    if (datauser > 0) {
                        if ((datauser & user) == user && user > 0) {
                            $(item).show();
                        } else {
                            $(item).hide();
                        }
                    }                  
                    else
                        $(item).show();					  
                } else {
					$(item).hide();
				}
            } else {
                 if (datahasaccount != -1) {
                    if (datahasaccount == hasAccount)
                        $(item).show();
                    else
                        $(item).hide();
                } 
                else
				    $(item).show();
			}
		} else {
			$(item).hide();
		}
	});
}

function directions_SetWorkPlace(value, label, coWorkers) {

	if (value == null && value.length == 0) return;
	if (label == null) label = "";
	cmdasync("cmd", "SetWorkPlace", "value", value, "label", label, "coWorkers", coWorkers);
}

function directions_SetWorkPlace1() {
	cmdasync("cmd", "SetWorkPlace1");
}

function directions_SetTransfer(value, label) {
	cmdasync("cmd", "SetTransfer", "value", value, "label", label);
}



function directions_SetWorkPlaceTo(value, label) {

	if (value == null && value.length == 0) return;
	if (label == null) label = "";
	cmdasync("cmd", "SetWorkPlaceTo", "value", value, "label", label);
}

function directions_ClearWorkPlace() {
	cmdasync("cmd", "ClearWorkPlace");
}

function directions_CloseWorkPlaceList() {
	if (null != directions_SetPositionWorkPlaceList.form)
		directions_SetPositionWorkPlaceList.form.dialog("close");
	directions_setElementFocus(null, "efPhoneDesk_0");
	directions_checkWorkplaceSelected();
}

function directions_checkWorkplaceSelected() {
	cmdasync("cmd", "CheckWorkplaceSelected");
}

function directions_SearchWorkPlace() {
    
    cmdasync("cmd", "AdvSearchWorkPlace");

}

function directions_AdvSearchWorkPlace(control, callbackKey, result, isMultiReturn) {
	if (result.length == 0) return;
	directions_SetWorkPlace(result[0].value, result[0].label, 1);
}

function directions_AdvSearchWorkPlaceTo(control, callbackKey, result, isMultiReturn) {
	if (result.length == 0) return;
	directions_SetWorkPlaceTo(result[0].value, result[0].label);
}


//================================================= Что организовать

directions_SetInfoWorkPlaceType.form = null;

function directions_SetInfoWorkPlaceType() {
	if (null == directions_SetInfoWorkPlaceType.form) {
		var title = "Выберите, что организовать сотруднику";
		var width = 400;
		var height = 150;

		var onOpen = function() { directions_setElementFocus("RDT", "", "radio"); };
		var onClose = function() { directions_CloseInfoWorkPlaceType(); };
		var buttons = [
			{
				id: "btnInvoWPType_Cancel",
				text: directions_clientLocalization.cmdCancel,
				icons: {
					primary: v4_buttonIcons.Cancel
				},
				width: 100,
				click: directions_CloseInfoWorkPlaceType
			}
		];

		directions_SetInfoWorkPlaceType.form = v4_dialog("divInfoWorkPlaceType",
			$("#divInfoWorkPlaceType"),
			title,
			width,
			height,
			onOpen,
			onClose,
			buttons);
	}

	directions_clearData = true;
	directions_clearRadio("divWorkPlaceType");
	directions_SetInfoWorkPlaceType.form.dialog("open");
}

function directions_CloseInfoWorkPlaceType() {
	if (null != directions_SetInfoWorkPlaceType.form)
		directions_SetInfoWorkPlaceType.form.dialog("close");

	if (directions_clearData) {
		$("#divAdvInfo").hide();
		cmdasync("cmd", "ClearSotrudnik");
	} else {
		$("#divAdvInfo").show();
	}
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
				width: 100,
				click: directions_CloseMailNameList
			}
		];
		var dialogPostion = { my: "top", at: "bottom", of: $("#efMailName_0") };
		directions_SetMailNamesList.form = v4_dialog("divMailNames",
			$("#divMailNames"),
			title,
			width,
			height,
			onOpen,
			null,
			buttons,
			dialogPostion);


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
		var height = 321;
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
				click: function() {
					v4_showConfirm(directions_clientLocalization.CONFIRM_StdMessage,
						directions_clientLocalization.CONFIRM_StdTitle,
						directions_clientLocalization.CONFIRM_StdCaptionYes,
						directions_clientLocalization.CONFIRM_StdCaptionNo,
						directions_deleteRoleCallBack(directions_getAttribute("divPositionRolesAdd", "data-id"), 1),
						400);
				}
			},
			{
				id: "btnPRoles_Cancel",
				text: directions_clientLocalization.cmdCancel,
				icons: {
					primary: v4_buttonIcons.Cancel
				},
				width: 100,
				click: directions_ClosePositionRolesAdd
			}
		];

		directions_SetPositionRolesAdd.form = v4_dialog("divPositionRolesAdd",
			$("#divPositionRolesAdd"),
			title,
			width,
			height,
			onOpen,
			null,
			buttons);

	}

	directions_SetPositionRolesAdd.form.dialog("open");
}

function directions_ClosePositionRolesAdd(setFocus) {
	if (null != directions_SetPositionRolesAdd.form)
		directions_SetPositionRolesAdd.form.dialog("close");
	directions_setAttribute("divPositionRolesAdd", "data-id", "");
	directions_setElementFocus(null, setFocus != null ? setFocus : "btnLinkRolesAdd");
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
				click: function() { cmdasync("cmd", "SavePositionType", "check", 1); }
			},
			{
				id: "btnPTypes_Delete",
				text: directions_clientLocalization.cmdDelete,
				icons: {
					primary: v4_buttonIcons.Delete
				},
				width: 75,
				click: function() {
					v4_showConfirm(directions_clientLocalization.CONFIRM_StdMessage,
						directions_clientLocalization.CONFIRM_StdTitle,
						directions_clientLocalization.CONFIRM_StdCaptionYes,
						directions_clientLocalization.CONFIRM_StdCaptionNo,
						directions_deleteTypeAllCallBack(directions_getAttribute("divPositionTypesAdd", "data-id"), 1),
						400);

				}
			},
			{
				id: "btnPTypes_Cancel",
				text: directions_clientLocalization.cmdCancel,
				icons: {
					primary: v4_buttonIcons.Cancel
				},
				width: 100,
				click: directions_ClosePositionTypesAdd
			}
		];
		directions_SetPositionTypesAdd.form = v4_dialog("divPositionTypesAdd",
			$("#divPositionTypesAdd"),
			title,
			width,
			height,
			onOpen,
			null,
			buttons);


	}

	directions_SetPositionTypesAdd.form.dialog("open");
}


function directions_ClosePositionTypesAdd(setFocus) {
	if (null != directions_SetPositionTypesAdd.form)
		directions_SetPositionTypesAdd.form.dialog("close");
	directions_setElementFocus(null, setFocus != null ? setFocus : "btnLinkTypesAdd");
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
				width: 100,
				click: directions_ClosePositionCFAdd
			}
		];

		directions_SetPositionCFAdd.form = v4_dialog("divCommonFoldersAdd",
			$("#divCommonFoldersAdd"),
			title,
			width,
			height,
			onOpen,
			null,
			buttons);

	}

	directions_SetPositionCFAdd.form.dialog("open");
}


function directions_ClosePositionCFAdd() {
	if (null != directions_SetPositionCFAdd.form)
		directions_SetPositionCFAdd.form.dialog("close");
}


function directions_PositionCF_Save() {

	var selected = {};
	$(".div_cf input:checked").each(function() {
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


//================================================= Дополнительный функиции

function directions_openAnotherEquipment() {

	if ($("#divSotrudnikAnotherWorkPlaces").is(":visible")) {
		$("#divSotrudnikAnotherWorkPlaces").hide();
		return;
	}

	cmdasync("cmd", "OpenAnotherEquipmentDetails");
}

function directions_openEquipment(idLocation, idContainer) {

	if ($("#" + idContainer).is(":visible")) {
		$("#" + idContainer).hide();
		return;
	}

	cmdasync("cmd", "OpenEquipmentDetails", "IdLocation", idLocation, "IdContainer", idContainer);
}

function directions_groupMembers(idGroup) {
	var idContainer = "divInfoGroup_" + idGroup;

	if ($("#" + idContainer).is(":visible")) {
		$("#" + idContainer).hide();
		return;
	}
	cmdasync("cmd", "OpenGroupMembers", "IdGroup", idGroup, "IdContainer", idContainer);
}

//================================================= Выбор рабочего места

directions_anotherEquipmentList.form = null;

function directions_anotherEquipmentList() {

	if (null == directions_anotherEquipmentList.form) {

		var title = directions_clientLocalization.DIRECTIONS_FORM_ADVINFO_Title;
		var width = 483;
		var height = 253;
		var onOpen = function() { directions_setElementFocus(null, "btnAEQ_Add"); };
		var buttons = [
			{
				id: "btnAEQ_Add",
				text: "Ок",
				icons: {
					primary: v4_buttonIcons.Ok
				},
				click: directions_closeAnotherEquipmentList
			}
		];

		directions_anotherEquipmentList.form = v4_dialog("divAdvInfoValidation",
			$("#divAdvInfoValidation"),
			title,
			width,
			height,
			onOpen,
			null,
			buttons);

	}

	directions_anotherEquipmentList.form.dialog("open");
}

function directions_closeAnotherEquipmentList() {
	if (null != directions_anotherEquipmentList.form) {
		var obj = document.getElementById("divAdvInfoValidation_Body");
		if (obj) obj.innerHTML = "";
		directions_anotherEquipmentList.form.dialog("close");
	}
}

//================================================= Callback confirm
function directions_deleteTypeAllCallBack(id, close) {
	return 'cmdasync("cmd", "DeletePositionByCatalog", "catalog",' +
		id +
		', "closeForm", "' +
		(close == 1 ? 1 : 0) +
		'");';
}

function directions_deleteTypeCallBack(catalog, theme) {
	return 'cmdasync("cmd", "DeletePositionType", "catalog", "' +
		catalog +
		'", "theme", "' +
		theme +
		'", "closeForm", "0");';
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
	return 'cmdasync("cmd", "DeletePositionRoleByGuid", "guid", "' +
		id +
		'", "closeForm", "' +
		(close == 1 ? 1 : 0) +
		'");';
}

function directions_showInconsist(id) {

	if ($("#" + id).is(":visible")) {
		$("#" + id).hide();
		return;
	}
	$("#" + id).show();
}


$(document).ready(function () {

    $('input[type="radio"]').keydown(function (e) {
        var arrowKeys = [37, 38, 39, 40];
        if (arrowKeys.indexOf(e.which) !== -1) {
            $(this).blur();            
            return false;
        }
    });

});

//$(".changeCopy").bind('copy', function () {
//    var text = window.getSelection().toString().replace(///[ ]+/g, '//');
//    copyToClipboard(text);
//});

//function copyToClipboard(text) {
//    var textarea = document.createElement("textarea");
//    textarea.textContent = text;
//    textarea.style.position = "fixed";
//    document.body.appendChild(textarea);
//    textarea.select();
//    try {
//        return document.execCommand("cut");
//    } catch (ex) {
//        console.warn("Copy to clipboard failed.", ex);
//        return false;
//    } finally {
//        document.body.removeChild(textarea);
//    }
//}