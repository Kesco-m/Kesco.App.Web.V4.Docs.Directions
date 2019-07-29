<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="DirectionIT.aspx.cs" Inherits="Kesco.App.Web.Docs.Directions.DirectionIT" %>
<%@ Register TagPrefix="dbs" Namespace="Kesco.Lib.Web.DBSelect.V4" Assembly="DBSelect.V4" %>
<%@ Register TagPrefix="base" Namespace="Kesco.Lib.Web.Controls.V4" Assembly="Controls.V4" %>


<!DOCTYPE html>
<html>
<head runat="server">
    <title></title>
    <link href="Kesco.Directions.css" rel="stylesheet" type="text/css"/>
    <script language="JavaScript" type="text/javascript" src="Kesco.Directions.js"></script>
</head>
<body>
<%= RenderDocumentHeader() %>

<div class="v4FormContainer">
<div id="divGroup1" class="marginT2">
    <div id="divHeader1" class="directionGroupHeader marginT"></div>
    <div id="divGroup11">

        <div id="divPhoto" class="floatLeft"><% RenderData.Photo(this, Response.Output, Dir); %></div>

        <div class="disp_inline w300" style="height: 1px"></div><br/>

        <div class="marginL disp_inline">
            <div class="disp_inlineBlock w100">
                <%= Resx.GetString("cEmplName") %>:
            </div>
            <div class="marginL disp_inlineBlockS">
                <dbs:DBSEmployee ID="efSotrudnik" runat="server" Width="321" CLID="3" IsCaller="True" CallerType="Employee" AutoSetSingleValue="True" OnChanged="efSotrudnik_OnChanged" OnBeforeSearch="efSotrudnik_OnBeforeSearch"></dbs:DBSEmployee>
                <div>
                    <span id="spSotrudnikPost" style="display: inline-block"><% RenderData.SotrudnikPost(this, Response.Output, Dir, false); %></span>
                    <span id="spSotrudnikAOrg" style="display: inline-block"><% RenderData.SotrudnikAOrg(this, Response.Output, Dir); %></span>
                </div>
                <div id="divSotrudnikFinOrg"><% RenderData.SotrudnikFinOrg(this, Response.Output, Dir); %></div>
                <div id="divCommonEmployee"><% RenderData.CommonEmployee(this, Response.Output, Dir); %></div>
                <div id="divSotrudnikCadrWorkPlaces"><% RenderData.SotrudnikCadrWorkPlaces(this, Response.Output, Dir); %></div>
                <div id="divSupervisor"><% RenderData.Supervisor(this, Response.Output, Dir); %></div>

            </div>
        </div>
        <br/>


        <fieldset class="marginL paddingT paddingB paddingR marginT disp_inlineBlock">
            <legend><%= Resx.GetString("DIRECTIONS_Field_WpType") %>:</legend>
            <div class="marginL disp_inlineBlock" id="divWorkPlaceType">
                <div class="disp_inlineBlock">

                    <div class="disp_inlineBlockS">
                        <base:Radio runat="server" ID="rdWorkPlaceType1" Name="rdWorkPlaceType" OnChanged="rdWorkPlaceType1_OnChanged"></base:Radio>
                    </div>
                    <div class="marginL2 marginT3 disp_inlineBlockS" id="divWorkPlaceLink" style="display: none" data-wp="1">
                        <a style="color: blue;" id="linkWorkPlace" href="javascript:void(0);" onclick="cmdasync('cmd', 'RenderWorkPlaceList', 'x', event.pageX, 'y', event.pageY);"><%= Resx.GetString("DIRECTIONS_Field_WP_Link") %></a>
                    </div>
                    <div class="marginL" id="divWorkPlace" style="display: none" data-wp="1"></div>
                </div>
                <br/>
                <div class="disp_inlineBlock">
                    <div class="disp_inlineBlockS">
                        <base:Radio ID="rdWorkPlaceType2" runat="server" Name="rdWorkPlaceType" OnChanged="rdWorkPlaceType2_OnChanged"></base:Radio>
                    </div>
                    <div class="marginL" id="divWorkPlace2" style="display: none" data-wp="2"></div>
                </div>
                <br/>
                <div class="disp_inlineBlock">
                    <div class="disp_inlineBlockS">
                        <base:Radio ID="rdWorkPlaceType4" runat="server" Name="rdWorkPlaceType" OnChanged="rdWorkPlaceType4_OnChanged"></base:Radio>
                    </div>
                </div>
            </div>
        </fieldset>
    </div>

</div>
<div class="clearLeft" style="display: none" data-wp="7"></div>

<div id="divGroup2" style="display: none" data-wp="7">
<div id="divEquipment" data-wp="3">
    <div id="divPhoneLink" class="marginL marginT2" style="display: none" data-wp="3">
        <div class="floatLeft marginR w210">
            <%= Resx.GetString("DIRECTIONS_Field_Phone") %>:
        </div>

        <div class="disp_inlineBlock" style="display: none" data-wp="3">
            <div>
                <div class="disp_inlineBlock">
                    <div class="disp_inlineBlockS" name="divPhone">
                        <base:CheckBox ID="efPhoneDesk" runat="server" OnChanged="efPhoneDesk_OnChanged"></base:CheckBox>
                    </div>
                    <div class="disp_inlineBlockS w75" name="divPhone">
                        <label for="efPhoneDesk_0">
                            <%= Resx.GetString("DIRECTIONS_Field_Phone_Table") %>
                        </label>
                    </div>
                    <div class="disp_inlineBlockS marginL" name="divPhone">
                        <base:CheckBox ID="efPhoneIPCam" runat="server" IsDisabled="True" OnChanged="efPhoneIPCam_OnChanged"></base:CheckBox>
                    </div>
                    <div class="disp_inlineBlockS w75" name="divPhone">
                        <label for="efPhoneIPCam_0">
                            <%= Resx.GetString("DIRECTIONS_Field_Phone_WebCam") %>
                        </label>
                    </div>

                    <div class="marginT" data-wp="1" style="display: none">
                        <div class="disp_inlineBlockS">
                            <base:CheckBox ID="efPhoneDect" runat="server" OnChanged="efPhoneDect_OnChanged"></base:CheckBox>
                        </div>
                        <div class="disp_inlineBlockS w170">
                            <label for="efPhoneDect_0">
                                <%= Resx.GetString("DIRECTIONS_Field_Phone_Dect") %>
                            </label>
                        </div>
                    </div>
                </div>
                <div class="disp_inlineBlock" style="display: none;" id="divPLExit">
                    <base:ComboBox ID="efPLExit" runat="server" Width="185px" EmptyValueExist="False" OnChanged="efPLExit_OnChanged"></base:ComboBox>
                </div>
            </div>

        </div>
    </div>

    <div class="clearLeft" style="display: none" data-wp="3"></div>

    <div id="divMobilPhone" class="marginL" style="display: none" data-phone="1">
        <div class="floatLeft w210">
            <%= Resx.GetString("DIRECTIONS_Field_MobilPhone") %>:
        </div>
        <div class="marginL disp_inlineBlockS ">
            <base:TextBox ID="efRedirectNum" runat="server" Width="235" NextControl="efComputer_0" OnChanged="efRedirectNum_OnChanged"></base:TextBox>
            <div id="spMobilphoneArea" class="v4NtfInformation"><% RenderMobilphoneArea(Response.Output); %></div>
        </div>
    </div>

    <div class="clearLeft" style="display: none" data-wp="3"></div>
    <div id="divGroup7" style="display: none;" data-wp="3">
        <div id="divAdvancedGrantsContainer4" class="floatLeft marginT2 marginL">
            <% RenderData.AdvancedGrants(this, Response.Output, Dir, 4); %>
        </div>
        <div class="clearLeft"></div>
    </div>

    <div id="divComputer" class="marginL marginT" style="display: none" data-wp="3">
        <div class="floatLeft marginR w210" style="white-space: normal !important;">
            <%= Resx.GetString("DIRECTIONS_Field_Computer") %>:
        </div>

        <div class="disp_inlineBlock">
            <div class="disp_inlineBlock">
                <div class="disp_inlineBlockS">
                    <base:CheckBox ID="efComputer" runat="server" OnChanged="efComputer_OnChanged"></base:CheckBox>
                </div>
                <div class="disp_inlineBlockS w210">
                    <label for="efComputer_0">
                        <%= Resx.GetString("DIRECTIONS_Field_Computer_Desktop") %>
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
                        <%= Resx.GetString("DIRECTIONS_Field_Computer_Laptop") %>
                    </label>
                </div>
            </div>
        </div>
    </div>

    <div class="clearLeft" style="display: none" data-wp="3"></div>
    <div id="divGroup6" style="display: none;" data-wp="3">
        <div id="divAdvancedGrantsContainer2" class="floatLeft marginT2 marginL">
            <% RenderData.AdvancedGrants(this, Response.Output, Dir, 2); %>
        </div>
        <div class="clearLeft"></div>
    </div>

    <div id="divAdvEq" class="marginL marginT" style="display: none" data-wp="3">
        <div class="floatLeft w220">
            <%= Resx.GetString("DIRECTIONS_Field_AdvEq") %>:
        </div>
        <div class="clearLeft" style="display: none" data-wp="3"></div>
        <div class="floatLeft">
            <base:TextArea ID="efAdvEq" runat="server" Width="450px" Rows="1" NextControl="efAccessEthernet"></base:TextArea>
        </div>
    </div>

</div>

<div id="divEthernet" data-wp="7">
    <div class="clearLeft" style="display: none" data-wp="3"></div>

    <div class="marginL disp_hr" style="display: none" data-wp="3">
        <hr/>
    </div>

    <div id="divAccessEthernet" class="marginL marginT2" data-wp="7">
        <div class="floatLeft w210">
            <%= Resx.GetString("DIRECTIONS_Field_AEAccess") %>:
        </div>

        <div class="disp_inlineBlock marginL" data-wp="7">
            <div class="disp_inlineBlockS">
                <base:CheckBox ID="efAccessEthernet" runat="server" OnChanged="efAccessEthernet_OnChanged"></base:CheckBox>
            </div>
            <div class="disp_inlineBlock" style="display: none;" data-lan="1">
                <div class="disp_inlineBlockS w70">
                    <%= Resx.GetString("DIRECTIONS_Field_AEAccess_Login") %>:
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
            <%= Resx.GetString("DIRECTIONS_Field_Lang") %>:
        </div>
        <div class="disp_inlineBlock marginL">
            <base:ComboBox ID="efLang" runat="server" Width="76px" EmptyValueExist="False"></base:ComboBox>
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
                        <base:TextBox ID="efMailName" runat="server" Width="170px" NextControl="efDomain"></base:TextBox>
                    </div>
                    <div class="disp_inlineBlockS">
                        <input type="button" class="v4s_btn" value="..." onclick="cmdasync('cmd', 'RenderMailNamesList', 'x', event.pageX, 'y', event.pageY);"/>
                    </div>
                </div>

                <div class="disp_inlineBlock">
                    <div class="disp_inlineBlockS marginL w25">
                        <font style="font-size: 14pt;">@</font>
                    </div>
                    <div class="disp_inlineBlockS">
                        <base:ComboBox ID="efDomain" runat="server" Width="145px" EmptyValueExist="False" NextControl="efSotrudnikParentCheck1"></base:ComboBox>
                    </div>
                </div>
            </div>
            <div id="divEmailCheck"><% RenderData.EmailCheck(this, Response.Output, Dir); %></div>
        </div>
    </div>
    <div class="clearLeft"></div>
</div>

<div id="divGroup5" style="display: none;" data-wp="7" data-lan="1">
    <div id="divAdvancedGrantsContainer1" class="floatLeft marginT2 marginL">
        <% RenderData.AdvancedGrants(this, Response.Output, Dir, 1); %>
    </div>
    <div class="clearLeft"></div>
</div>

<div id="divGroup3" class="marginT2" style="display: none;" data-wp="7" data-lan="1" data-user="1">
    <div style="height: 5px;">&nbsp;</div>
    <div id="divHeader3" class="marginL directionGroupHeader"><%= Resx.GetString("DIRECTIONS_Field_AccessData") %>:</div>

    <div id="divSotrudnikParentContainer" class="floatLeft marginT2 marginL">
        <div class="disp_inlineBlock">
            <div class="disp_inlineBlockS">
                <base:CheckBox ID="efSotrudnikParentCheck1" runat="server" OnChanged="efSotrudnikParentCheck1_OnChanged"></base:CheckBox>
            </div>
            <div class="disp_inlineBlockS w120">
                <label for="efSotrudnikParentCheck1_0">
                    <%= Resx.GetString("DIRECTIONS_Field_AccessData_How") %>
                </label>
            </div>
            <br/>
            <div class="disp_inlineBlockS ">
                <base:CheckBox ID="efSotrudnikParentCheck2" runat="server" OnChanged="efSotrudnikParentCheck2_OnChanged"></base:CheckBox>
            </div>
            <div class="disp_inlineBlockS w120" style="white-space: normal !important;">
                <label for="efSotrudnikParentCheck2_0">
                    <%= Resx.GetString("DIRECTIONS_Field_AccessData_Instead") %>
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
                <%= Resx.GetString("DIRECTIONS_Field_Positions_Folders") %>:
            </div>
           <div class="disp_inlineBlock" id="divNewPositionFolders">
                <a href="javascript:void(0);" id="btnLinkCFAdd" onclick="cmdasync('cmd', 'AddPositionCommonFolders');" title="<%= Resx.GetString("DIRECTIONS_Field_Positions_Folders_Title") %>">
                    <img border="0" src="/styles/new.gif"/>
                </a>
            </div>

        </div>
        <div class="disp_inlineBlock">
            <span id="divCommonFoldersData"></span>
        </div>
    </div>
    <div class="clearLeft"></div>

    <div id="divPositionRoles" class="marginT2 marginL">
        <div class="floatLeft">
            <div class="disp_inlineBlock w145">
                <%= Resx.GetString("DIRECTIONS_Field_Positions_Roles") %>:
            </div>
           
            <div class="disp_inlineBlock" id="divNewPositionRoles">
                <a href="javascript:void(0);" id="btnLinkRolesAdd" onclick="cmdasync('cmd', 'AddPositionRole');" title="<%= Resx.GetString("DIRECTIONS_Field_Positions_Roles_Title") %>">
                    <img border="0" src="/styles/new.gif"/>
                </a>
            </div>
                
        </div>
        <div class="disp_inlineBlock">
            <span id="divPositionRolesData"></span>
        </div>
    </div>

    <div class="clearLeft"></div>

    <div id="divPositionTypes" class="marginT2 marginL">
        <div class="floatLeft">
            <div class="disp_inlineBlock w145">
                <%= Resx.GetString("DIRECTIONS_Field_Positions_Types") %>:
            </div>
           
            <div class="disp_inlineBlock" id="divNewPositionTypes">
                <a href="javascript:void(0);" id="btnLinkTypesAdd" onclick="cmdasync('cmd', 'AddPositionTypes');" title="<%= Resx.GetString("DIRECTIONS_Field_Positions_Types_Title") %>">
                    <img border="0" src="/styles/new.gif"/>
                </a>
            </div>
              
        </div>
        <div class="disp_inlineBlock">
            <span id="divPositionTypesData"></span>
        </div>
    </div>
</div>

<div class="clearLeft"></div>

<div class="marginL disp_hr" style="display: none;" data-block="dblEthernet">
    <hr/>
</div>
</div>


<div id="divGroup4">
    <div id="divAdvInfo" class="marginL marginT2">
        <div class="floatLeft  w145">
            <%= Resx.GetString("DIRECTIONS_Field_AdvInfo") %>:
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
            <div class="floatLeft w100"><%= Resx.GetString("DIRECTIONS_Field_Positions_Roles_Select") %>:</div>
            <div class="disp_inlineBlock">
                <dbs:DBSRole ID="efPRoles_Role" runat="server" Width="300" OnChanged="efPRoles_Role_OnChanged" AutoSetSingleValue="True" IsRequired="True"></dbs:DBSRole>
            </div>
        </div>
        <div class="marginL" id="efPRoles_Role_Description"></div>
        <div class="marginT2 marginL">
            <div class="floatLeft w100"><%= Resx.GetString("DIRECTIONS_Pos_InCompany") %>:</div>
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
            <div class="floatLeft w100 marginT"><%= Resx.GetString("DIRECTIONS_Rnd_Catalog") %>:</div>
            <div class="disp_inlineBlock">
                <dbs:DBSPersonCatalog ID="efPTypes_Catalog" IsNotUseSelectTop="True" IsRequired="True" IsCustomRecordInPopup="True" CustomRecordId="0" runat="server" Width="300" OnChanged="efPTypes_Catalog_OnChanged" AutoSetSingleValue="True"></dbs:DBSPersonCatalog>
            </div>
        </div>
        <div class="marginT2 marginL">
            <div class="floatLeft w100 marginT2"><%= Resx.GetString("DIRECTIONS_Rnd_TypePerson") %>:</div>
            <div class="disp_inlineBlock">
                <dbs:DBSPersonTheme ID="efPTypes_Type" runat="server" IsCustomRecordInPopup="True" CustomRecordId="0" AutoSetSingleValue="True" IsSelectOnlyCustomRecord="True" IsMultiSelect="true" IsMultiReturn="True" IsRow="False" IsRemove="true" Width="300" OnChanged="efPTypes_Type_OnChanged"></dbs:DBSPersonTheme>
            </div>
        </div>
    </div>
</div>

</div>


<script type="text/javascript">


    function directions_initNewDoc() {

    }

    $(document).ready(function() {

    });


</script>
</body>
</html>