<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="DirectionIT.aspx.cs" Inherits="Kesco.App.Web.Docs.Directions.DirectionIT" %>

<%@ Register TagPrefix="dbs" Namespace="Kesco.Lib.Web.DBSelect.V4" Assembly="DBSelect.V4" %>
<%@ Register TagPrefix="base" Namespace="Kesco.Lib.Web.Controls.V4" Assembly="Controls.V4" %>


<!DOCTYPE html>
<html>
<head runat="server">
    <title></title>
    <link href="Kesco.Directions.css?v=5" rel="stylesheet" type="text/css"/>
    <script language="JavaScript" type="text/javascript" src="Kesco.Directions.js?v=5"></script>
</head>
<body>
<%= RenderDocumentHeader() %>

<div class="v4formContainer">
<div id="divGroup1" class="marginT2">
    <div id="divHeader1" class="directionGroupHeader marginT"></div>
    <div id="divGroup11">
        <div class="v4DivTable">
            <div class="v4DivTableRow">
                <div class="v4DivTableCellT">
                    <div id="divPhoto" class="dispNone marginL"><% RenderData.Photo(this, Response.Output, Dir); %></div>
                </div>
                <div class="v4DivTableCellT" style="width: 100%">
                    <div class="v4DivTable">
                        <div class="v4DivTableRow">
                            <div class="v4DivTableCellT paddingL">
                                <%= Resx.GetString("cEmplName") %>:
                            </div>
                            <div class="v4DivTableCellT paddingL" style="width: 100%">
                                <dbs:DBSEmployee ID="efSotrudnik" runat="server" Width="358" CLID="3" IsCaller="True" CallerType="Employee" AutoSetSingleValue="True" OnChanged="efSotrudnik_OnChanged" OnBeforeSearch="efSotrudnik_OnBeforeSearch"></dbs:DBSEmployee>
                                <div id="divSotrudnikPost"><% RenderData.SotrudnikPost(this, Response.Output, Dir, false); %></div>
                                <div id="divSotrudnikFinOrg"><% RenderData.SotrudnikFinOrg(this, Response.Output, Dir); %></div>
                                <div id="divSupervisor"><% RenderData.Supervisor(this, Response.Output, Dir); %></div>
                               
                                <div id="divSotrudnikCadrWorkPlaces"><% RenderData.SotrudnikCadrWorkPlaces(this, Response.Output, Dir); %></div>
                                <div id="divSotrudnikAnotherWorkPlaces" style="display: none;" class="wpntf"></div>

                            </div>
                        </div>
                    </div>

                </div>
            </div>
        </div>
    </div>
    <div id="divWorkPlaceType_Info">
        <% RenderData.WorkPlaceType(this, Response.Output, Dir); %>
    </div>
</div>
<div class="clearLeft" style="display: none" data-wp="1;2;3;4;5;6"></div>

<div id="divGroup2" style="display: none" data-wp="1;2;3;4;5;6">
<div id="divEquipment" data-wp="1;2;3">
    <div id="divPhoneLink" class="marginL marginT2" style="display: none" data-wp="1;2;3">
        <div class="floatLeft marginR w210">
            <%= DIRECTIONS_Field_Phone %>:
        </div>

        <div class="disp_inlineBlock" style="display: none" data-wp="1;2;3">
            <div>
                <div class="disp_inlineBlock">
                    <div class="disp_inlineBlockS" name="divPhone">
                        <base:CheckBox ID="efPhoneDesk" runat="server" OnChanged="efPhoneDesk_OnChanged"></base:CheckBox>
                    </div>
                    <div class="disp_inlineBlockS w75" name="divPhone">
                        <label for="efPhoneDesk_0">
                            <%= DIRECTIONS_Field_Phone_Table %>
                        </label>
                    </div>
                    <div class="disp_inlineBlockS marginL" name="divPhone">
                        <base:CheckBox ID="efPhoneIPCam" runat="server" IsDisabled="True" OnChanged="efPhoneIPCam_OnChanged"></base:CheckBox>
                    </div>
                    <div class="disp_inlineBlockS w85" name="divPhone">
                        <label for="efPhoneIPCam_0">
                            <%= DIRECTIONS_Field_Phone_WebCam %>
                        </label>
                    </div>

                    <div class="marginT" data-wp="1;2" style="display: none">
                        <div class="disp_inlineBlockS">
                            <base:CheckBox ID="efPhoneDect" runat="server" OnChanged="efPhoneDect_OnChanged"></base:CheckBox>
                        </div>
                        <div class="disp_inlineBlockS w170">
                            <label for="efPhoneDect_0">
                                <%= DIRECTIONS_Field_Phone_Dect %>
                            </label>
                        </div>
                    </div>
                </div>
                <div class="disp_inlineBlock" style="display: none;" id="divPLExit">
                    <base:DropDownList ID="efPLExit" Width="150px" IsReadOnlyAlways="True" runat="server" OnChanged="efPLExit_OnChanged"></base:DropDownList>
                </div>
            </div>

        </div>
    </div>

    <div class="clearLeft" style="display: none" data-wp="1;2;3"></div>

    <div id="divMobilPhone" class="marginL" style="display: none" data-phone="1">
        <div class="floatLeft w210">
            <%= DIRECTIONS_Field_MobilPhone %>:
        </div>
        <div class="marginL disp_inlineBlockS ">
            <base:TextBox ID="efRedirectNum" runat="server" Width="235" NextControl="efComputer_0" OnChanged="efRedirectNum_OnChanged"></base:TextBox>
            <div id="spMobilphoneArea" class="v4Information"><% RenderMobilphoneArea(Response.Output); %></div>
        </div>
    </div>

    <div class="clearLeft" style="display: none" data-wp="1;2;3"></div>
    <div id="divGroup7" style="display: none;" data-wp="1;2;3">
        <div id="divAdvancedGrantsContainer4" class="floatLeft marginT2 marginL">
            <% RenderData.AdvancedGrants(this, Response.Output, Dir, 4); %>
        </div>
        <div class="clearLeft"></div>
    </div>

    <div id="divComputer" class="marginL marginT" style="display: none" data-wp="1;2;3">
        <div class="floatLeft marginR w210" style="white-space: normal !important;">
            <%= DIRECTIONS_Field_Computer %>:
        </div>

        <div class="disp_inlineBlock">
            <div class="disp_inlineBlock">
                <div class="disp_inlineBlockS">
                    <base:CheckBox ID="efComputer" runat="server" OnChanged="efComputer_OnChanged"></base:CheckBox>
                </div>
                <div class="disp_inlineBlockS w210">
                    <label for="efComputer_0">
                        <%= DIRECTIONS_Field_Computer_Desktop %>
                    </label>
                </div>
            </div>
            <br/>
            <div class="disp_inlineBlock marginT">
                <div class="disp_inlineBlockS">
                    <base:CheckBox ID="efNotebook" runat="server" OnChanged="efNotebook_OnChanged"></base:CheckBox>
                </div>
                <div class="disp_inlineBlockS w75">
                    <label for="efNotebook_0">
                        <%= DIRECTIONS_Field_Computer_Laptop %>
                    </label>
                </div>
            </div>
        </div>
    </div>

    <div class="clearLeft" style="display: none" data-wp="1;2;3"></div>
    <div id="divGroup6" style="display: none;" data-wp="1;2;3">
        <div id="divAdvancedGrantsContainer2" class="floatLeft marginT2 marginL">
            <% RenderData.AdvancedGrants(this, Response.Output, Dir, 2); %>
        </div>
        <div class="clearLeft"></div>
    </div>

    <div id="divAdvEq" class="marginL marginT" style="display: none" data-wp="1;2;3">
        <div class="floatLeft w220">
            <%= DIRECTIONS_Field_AdvEq %>:
        </div>
        <div class="clearLeft" style="display: none" data-wp="1;2;3"></div>
        <div class="floatLeft">
            <base:TextArea ID="efAdvEq" runat="server" Width="450px" Rows="2"></base:TextArea>
        </div>
    </div>

</div>

<div id="divEthernet" data-wp="1;3;4;5;6" data-lan="1">
    <div class="clearLeft" style="display: none" data-wp="1;3"></div>

    <div class="marginL disp_hr" style="display: none" data-wp="1;3">
        <hr/>
    </div>

    <div id="divAccessEthernet" class="marginL marginT2" data-wp="1;3;4;5;6">
        <div class="floatLeft w210">
            <%= DIRECTIONS_Field_AEAccess %>:
        </div>

        <div class="disp_inlineBlock marginL" data-wp="1;3;4;5;6">
            <div class="disp_inlineBlockS">
                <base:CheckBox ID="efAccessEthernet" runat="server"></base:CheckBox>
            </div>
            <div class="disp_inlineBlock" style="display: none;" data-lan="1">
                <div class="disp_inlineBlockS w70">
                    <%= DIRECTIONS_Field_AEAccess_Login %>:
                </div>
                <div class="disp_inlineBlockS">
                    <base:TextBox ID="efLogin" runat="server" Width="145px" NextControl="efMailName"></base:TextBox>
                </div>
            </div>
        </div>
    </div>
    <div class="clearLeft"></div>

    <div id="divLang" class="marginT marginL" style="display: none;" data-lan="1">
        <div class="floatLeft w210">
            <%= DIRECTIONS_Field_Lang %>:
        </div>
        <div class="disp_inlineBlock marginL">
            <base:DropDownList ID="efLang" Width="65px" IsReadOnlyAlways="True" runat="server" NextControl="efMailName" OnChanged="efLang_OnChanged"></base:DropDownList>
        </div>
    </div>
    <div class="clearLeft"></div>

    <div id="divEmail" class="marginT marginL" style="display: none;" data-lan="1">
        <div class="floatLeft w75">
            E-Mail :
        </div>
        <div class="disp_inlineBlock">
            <div class="disp_inlineBlock">
                <div class="disp_inlineBlock">
                    <div class="floatLeft">
                        <base:DropDownList ID="efMailName" Width="170px" runat="server" NextControl="efDomain" IsNoLimitList="True"></base:DropDownList>
                    </div>
                </div>

                <div class="disp_inlineBlock">
                    <div class="disp_inlineBlockS marginL w25">
                        <font style="font-size: 14pt;">@</font>
                    </div>
                    <div class="disp_inlineBlockS">
                        <base:DropDownList ID="efDomain" Width="125px" IsReadOnlyAlways="True" runat="server" NextControl="efSotrudnikParentCheck1"></base:DropDownList>
                    </div>
                </div>
            </div>
            <div id="divEmailCheck"><% RenderData.EmailCheck(this, Response.Output, Dir); %></div>
        </div>
    </div>
    <div class="clearLeft"></div>
</div>

<div id="divGroup5" style="display: none;" data-wp="1;3;5;6" data-lan="1">
    <div id="divAdvancedGrantsContainer1" class="floatLeft marginT2 marginL">
        <% RenderData.AdvancedGrants(this, Response.Output, Dir, 1); %>
    </div>
    <div class="clearLeft"></div>
</div>

<div id="divGroup3" class="marginT2" style="display: none;" data-wp="1;3;4;5;6" data-lan="1" data-user="1">
    <div style="height: 5px;">&nbsp;</div>
    <div id="divHeader3" class="marginL directionGroupHeader"><%= DIRECTIONS_Field_AccessData %>:</div>

    <div id="divSotrudnikParentContainer" class="floatLeft marginT2 marginL" style="display: none;" data-wp="1;3;4;5;6">
        <div class="disp_inlineBlock"  style="display: none;" data-wp="1;3;4;5;6">
            
            <div class="disp_inlineBlockS" style="display: none;" data-wp="1;3;4;6" >
                <base:CheckBox ID="efSotrudnikParentCheck1" runat="server" OnChanged="efSotrudnikParentCheck1_OnChanged"></base:CheckBox>
            </div>
            
            <div class="disp_inlineBlockS w120" style="display: none;" data-wp="1;3;4;6" >
                <label for="efSotrudnikParentCheck1_0">
                    <%= DIRECTIONS_Field_AccessData_How %>
                </label>
            </div>

            <br data-wp="1;3;5;6"  data-hasaccount="0" style="display: none;"/>
            
            <div class="disp_inlineBlockS " data-wp="1;3;5;6" data-hasaccount="0" style="display: none;">
                <base:CheckBox ID="efSotrudnikParentCheck2" runat="server" OnChanged="efSotrudnikParentCheck2_OnChanged"></base:CheckBox>
            </div>
            <div class="disp_inlineBlockS w120"  data-wp="1;3;5;6" data-hasaccount="0" style="display: none; white-space: normal !important;" >
                <label for="efSotrudnikParentCheck2_0">
                    <%= DIRECTIONS_Field_AccessData_Instead %>
                </label>
            </div>

        </div>
        <div id="divSotrudnikParent" class="disp_inlineBlockS marginT2 marginL">
            <dbs:DBSEmployee ID="efSotrudnikParent" runat="server" Width="283" CLID="3" IsCaller="True" CallerType="Employee" NextControl="btnLinkCFAdd" AutoSetSingleValue="True" OnChanged="efSotrudnikParent_OnChanged"></dbs:DBSEmployee>
        </div>
    </div>

    <div class="clearLeft"></div>
    <br/>

    <div class="marginL disp_hr" style="display: none">
        <hr/>
    </div>

    <div class="clearLeft"></div>

    <div id="divPositionFolders" class="marginT marginL">

        <div class="floatLeft">
            <div class="disp_inlineBlock w145">
                <%= DIRECTIONS_Field_Positions_Folders %>:
            </div>
            <div class="disp_inlineBlock" id="divNewPositionFolders">
                <a href="javascript:void(0);" id="btnLinkCFAdd" onclick="cmdasync('cmd', 'AddPositionCommonFolders');" title="<%= DIRECTIONS_Field_Positions_Folders_Title %>">
                    <img border="0" src="/styles/new.gif"/>
                </a>
            </div>

        </div>
        <div class="disp_inlineBlock">
            <span id="divCommonFoldersData"></span>
        </div>
        <hr/>
    </div>
    <div class="clearLeft"></div>

    <div id="divPositionRoles" class="marginT2 marginL">
        <div class="floatLeft">
            <div class="disp_inlineBlock w145">
                <%= DIRECTIONS_Field_Positions_Roles %>:
            </div>

            <div class="disp_inlineBlock" id="divNewPositionRoles">
                <a href="javascript:void(0);" id="btnLinkRolesAdd" onclick="cmdasync('cmd', 'AddPositionRole');" title="<%= DIRECTIONS_Field_Positions_Roles_Title %>">
                    <img border="0" src="/styles/new.gif"/>
                </a>
            </div>

        </div>
        <div class="disp_inlineBlock">
            <span id="divPositionRolesData"></span>
        </div>
        <hr/>
    </div>

    <div class="clearLeft"></div>

    <div id="divPositionTypes" class="marginT2 marginL">
        <div class="floatLeft">
            <div class="disp_inlineBlock w145">
                <%= DIRECTIONS_Field_Positions_Types %>:
            </div>

            <div class="disp_inlineBlock" id="divNewPositionTypes">
                <a href="javascript:void(0);" id="btnLinkTypesAdd" onclick="cmdasync('cmd', 'AddPositionTypes');" title="<%= DIRECTIONS_Field_Positions_Types_Title %>">
                    <img border="0" src="/styles/new.gif"/>
                </a>
            </div>

        </div>
        <div class="disp_inlineBlock">
            <span id="divPositionTypesData"></span>
        </div>
       <hr/>
    </div>
  
</div>

<div class="clearLeft"></div>

<div class="marginL disp_hr" style="display: none;" data-block="dblEthernet">
    <hr/>
</div>
</div>


<div id="divGroup4">
    <div id="divAdvInfo" class="marginL marginT2" style="display: none;">
        <div class="floatLeft  w145">
            <%= DIRECTIONS_Field_AdvInfo %>:
        </div>
        <div class="disp_inlineBlock">
            <base:TextArea ID="efAdvInfo" runat="server" Width="305px" Rows="2"></base:TextArea>
        </div>
    </div>
    <div class="clearLeft"></div>
    <div class="marginL marginT2">
        <% StartRenderVariablePart(Response.Output, 145, 450, 0, 1, true); %>
        <% EndRenderVariablePart(Response.Output); %>
    </div>
</div>

<!--================ основной выбор, что организованить        ================-->
<div id="divInfoWorkPlaceType" style="display: none;">
    <div id="divInfoWorkPlaceType_Body" class="marginL marginR marginT">

        <fieldset class="marginL paddingT paddingB paddingR marginT">
            <legend><%= DIRECTIONS_Field_WpType %>:</legend>
            <div class="marginL disp_inlineBlock" id="divWorkPlaceType">
                                              
                <div class="disp_inlineBlock radio4" style="display: none;">
                    <div class="disp_inlineBlockS">
                        <base:Radio ID="rdWorkPlaceType4" runat="server" CSSClass="RDT" Name="rdWorkPlaceType" OnChanged="rdWorkPlaceType4_OnChanged" TabIndex="0"></base:Radio>
                    </div>
                </div>

                <br class="radio4" style="display: none;"/>
                <div class="disp_inlineBlock radio1" style="display: none;">
                    <div class="disp_inlineBlockS">
                        <base:Radio runat="server" ID="rdWorkPlaceType1" CSSClass="RDT" Name="rdWorkPlaceType" OnChanged="rdWorkPlaceType1_OnChanged" TabIndex="0"></base:Radio>
                    </div>

                    <div id="divWorkPlaceList_Data" class="marginL2 filterWP" style="display: none;"></div>
                    <div id="divWorkPlaceList_Link" class="marginL2 marginT filterWP" style="display: none; text-align: right;">
                        <a id="linkWorkPlace" href="javascript:void(0);" CSSClass="RDT" onclick="cmdasync('cmd', 'AdvSearchWorkPlace');"><%= cmdWorkplaceOther %></a>
                    </div>
                </div>

                <br class="radio3" style="display: none;"/>
                <div class="disp_inlineBlock radio3" style="display: none;">
                    <div class="disp_inlineBlockS">
                        <base:Radio ID="rdWorkPlaceType3" runat="server" CSSClass="RDT" Name="rdWorkPlaceType" OnChanged="rdWorkPlaceType3_OnChanged" TabIndex="0"></base:Radio>
                    </div>

                </div>
                               

                <br class="radio5" style="display: none;"/>
                <div class="disp_inlineBlock radio5" style="display: none;">
                    <div class="disp_inlineBlockS">
                        <base:Radio ID="rdWorkPlaceType5" runat="server" CSSClass="RDT" Name="rdWorkPlaceType" OnChanged="rdWorkPlaceType5_OnChanged" TabIndex="0"></base:Radio>
                    </div>
                </div>

                <br class="radio6" style="display: none;"/>
                <div class="disp_inlineBlock radio6" style="display: none;">
                    <div class="disp_inlineBlockS">
                        <base:Radio ID="rdWorkPlaceType6" runat="server" CSSClass="RDT" Name="rdWorkPlaceType" OnChanged="rdWorkPlaceType6_OnChanged" TabIndex="0"></base:Radio>
                    </div>
                </div>

            </div>
        </fieldset>

    </div>
</div>

<!--================ отображение дополнительной информации        ================-->
<div id="divAdvInfoValidation" style="display: none;">
    <div id="divAdvInfoValidation_Body" class="marginL marginR marginT"></div>
</div>

<!--================ Список доступных компьютеризированных рабочих мест        ================-->

<div id="divWorkPlaceList" style="display: none;">
    <div id="divWPL_Body" class="marginL marginR marginT"></div>
</div>

<!--================ Список доступных имен почтовых ящиков      ================-->

<div id="divMailNames" style="display: none;">
    <div id="divMN_Body" class="marginL marginR marginT"></div>
</div>

<!--================ Выбор общих папок                  ================-->

<div id="divCommonFoldersAdd" style="display: none;">
    <div id="divCF_Body" class="marginL marginR marginT" style="height: 250px; overflow: auto"></div>
</div>


<!--================ Редактирование позиции Ролей       ================-->

<div id="divPositionRolesAdd" data-id="" style="display: none;">
    <div id="divPRoles_Body" class="marginL marginR marginT">
        <div class="marginT marginL">
            <div class="floatLeft w100"><%= Resx.GetString("cEmplName") %>:</div>
            <div id="divPRoles_Employee" class="disp_inlineBlock"></div>
        </div>
        <div>
            <hr style="background-color: gainsboro; border: none; color: gainsboro; height: 2px;"/>
        </div>
        <div class="marginT marginL">
            <div class="floatLeft w100"><%= DIRECTIONS_Field_Positions_Roles_Select %>:</div>
            <div class="disp_inlineBlock">
                <dbs:DBSRole ID="efPRoles_Role" runat="server" Width="300" OnChanged="efPRoles_Role_OnChanged" AutoSetSingleValue="True" IsRequired="True"></dbs:DBSRole>
            </div>
        </div>
        <div class="marginL" id="efPRoles_Role_Description"></div>
        <div class="marginT2 marginL">
            <div class="floatLeft w100"><%= DIRECTIONS_Pos_InCompany %>:</div>
            <div class="disp_inlineBlock">
                <dbs:DBSPerson ID="efPRoles_Person" runat="server" IsCaller="True" CallerType="Person" Width="300" AutoSetSingleValue="True"></dbs:DBSPerson>
            </div>
        </div>
        <div style="bottom: 5px; left: 0; position: absolute; width: 98%;">
            <base:Changed ID="efChanged" runat="server"></base:Changed>
        </div>
    </div>
</div>

<!--================ Редактирование позиции Типов       ================-->

<div id="divPositionTypesAdd" data-id="" style="display: none;">
    <div id="divPTypes_Body" class="marginL marginR marginT">
        <div class="marginT marginL">
            <div class="floatLeft w100"><%= Resx.GetString("cEmplName") %>:</div>
            <div id="divPTypes_Employee" class="disp_inlineBlock"></div>
        </div>
        <div>
            <hr style="background-color: gainsboro; border: none; color: gainsboro; height: 2px;"/>
        </div>
        <div class="marginT marginL">
            <div class="floatLeft w100 marginT"><%= DIRECTIONS_Rnd_Catalog %>:</div>
            <div class="disp_inlineBlock">
                <dbs:DBSPersonCatalog ID="efPTypes_Catalog" IsNotUseSelectTop="True" IsRequired="True" IsCustomRecordInPopup="True" CustomRecordId="0" runat="server" Width="300" OnChanged="efPTypes_Catalog_OnChanged" AutoSetSingleValue="True"></dbs:DBSPersonCatalog>
            </div>
        </div>
        <div class="marginT2 marginL">
            <div class="floatLeft w100 marginT2"><%= DIRECTIONS_Rnd_TypePerson %>:</div>
            <div class="disp_inlineBlock">
                <dbs:DBSPersonTheme ID="efPTypes_Type" runat="server" IsCustomRecordInPopup="True" CustomRecordId="0" AutoSetSingleValue="True" IsSelectOnlyCustomRecord="True" IsMultiSelect="true" IsMultiReturn="True" IsRow="False" IsRemove="true" Width="300" OnChanged="efPTypes_Type_OnChanged"></dbs:DBSPersonTheme>
            </div>
        </div>
    </div>
</div>

</div>
<div class="clearBoth"></div>
<script type="text/javascript">


    function directions_initNewDoc() {

    }

    $(document).ready(function() {

    });


</script>
</body>
</html>