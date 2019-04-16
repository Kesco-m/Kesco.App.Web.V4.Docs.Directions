<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="DirectionIT.aspx.cs" Inherits="Kesco.App.Web.Docs.Directions.DirectionIT" %>
<%@ Import Namespace="Kesco.App.Web.Docs.Directions" %>
<%@ Register TagPrefix="dbs" Namespace="Kesco.Lib.Web.DBSelect.V4" Assembly="DBSelect.V4" %>
<%@ Register TagPrefix="base" Namespace="Kesco.Lib.Web.Controls.V4" Assembly="Controls.V4" %>


<!DOCTYPE html>
<html>
<head runat="server">
    <title></title>
    <link href='Kesco.Directions.css' rel='stylesheet' type='text/css'/>
    <script language="JavaScript" type="text/javascript" src="Kesco.Directions.js"></script>
</head>
<body>
    <%=RenderDocumentHeader()%> 
    
    <div class="v4FormContainer">
        <div id="divGroup1" class="marginT2">
            <div id="divHeader1" class="directionGroupHeader marginT"></div>
            <div id="divGroup11">
                 
                <div id="divPhoto" class="floatLeft" ><%Render.Photo(this, Response.Output, Dir);%></div>

                <div class="disp_inline w300" style="height:1px"></div><br/>
                
                <div class="marginL disp_inline">
                    <div class="disp_inlineBlock w100">
                        Сотрудник:
                    </div>
                    <div class="marginL disp_inlineBlockS">
                        <dbs:DBSEmployee ID="efSotrudnik" runat="server" Width="260" CLID="3" IsCaller="True" CallerType="Employee" AutoSetSingleValue="True" OnChanged="efSotrudnik_OnChanged" OnBeforeSearch="efSotrudnik_OnBeforeSearch" NextControl="efSupervisor"></dbs:DBSEmployee>
                        <div>
                            <span id="spSotrudnikPost" style="display: inline-block"><%Render.SotrudnikPost(this, Response.Output, Dir);%></span>
                            <span id="spSotrudnikAOrg" style="display: inline-block"><%Render.SotrudnikAOrg(this, Response.Output, Dir);%></span>
                        </div>
                        <div id = "divSotrudnikFinOrg"><%Render.SotrudnikFinOrg(this, Response.Output, Dir);%></div>
                        <div id = "divSotrudnikCadrWorkPlaces"><%Render.SotrudnikCadrWorkPlaces(this, Response.Output, Dir);%></div>
                        <div id = "divSupervisor"><%Render.Supervisor(this, Response.Output, Dir);%></div>
                    </div>
                </div>
                <br/>
                               
                
                <fieldset class="marginL paddingT paddingB paddingR marginT disp_inlineBlock">
                    <legend>Организовать сотруднику:</legend>
                    <div class="marginL disp_inlineBlock">
                        <div class="disp_inlineBlock">
                            <div class="disp_inlineBlockS">
                                <base:CheckBox ID="efWorkPlaceType1" runat="server" OnChanged="efWorkPlaceType1_OnChanged"></base:CheckBox>
                            </div>
                            <div class="marginL w300 disp_inlineBlockS">
                                рабочее место в офисе
                            </div>
                            <div class="marginL disp_inlineBlockS" id="divWorkPlaceLink" style="display: none" data-wp="1">
                                <a style="color: blue;" id="linkWorkPlace" href="javascript:void(0);" onclick="cmdasync('cmd','RenderWorkPlaceList', 'x', event.pageX, 'y', event.pageY);" >Выбрать рабочее место</a>
                            </div>
                            <div class="marginL" id="divWorkPlace" style="display: none" data-wp="1"></div>
                        </div>
                        <br/>
                        <div class="disp_inlineBlock">
                            <div class="disp_inlineBlockS">
                                <base:CheckBox ID="efWorkPlaceType2" runat="server" OnChanged="efWorkPlaceType2_OnChanged"></base:CheckBox>
                            </div>
                            <div class="marginL disp_inlineBlockS">
                               рабочее место вне офиса
                            </div>
                        </div>
                        <br/>
                        <div class="disp_inlineBlock">
                            <div class="disp_inlineBlockS">
                                <base:CheckBox ID="efWorkPlaceType4" runat="server" OnChanged="efWorkPlaceType4_OnChanged"></base:CheckBox>
                            </div>
                             <div class="marginL disp_inlineBlockS">
                               учетную запись без создания рабочего места
                            </div>
                        </div>
                    </div>
                </fieldset>
            </div>
            
        </div>
        <div class="clearLeft" style="display: none" data-wp="7"></div>
      
        <div id="divGroup2" style="display: none" data-wp="7">
            <div id="divHeader2"  class="directionGroupHeader marginT" data-wp="7">Оборудование и коммуникации, необходимые сотруднику для работы</div>
            
            
            <div id="divPhoneLink" class="marginL marginT2" style="display: none" data-wp="3">
                <div class="floatLeft marginR w75">
                    Телефон:
                </div>
                
                <div class="disp_inlineBlock" style="display: none" data-wp="3">
                    <div>
                        <div class="disp_inlineBlock">
                            <div class="disp_inlineBlockS" name="divPhone">
                                 <base:CheckBox ID="efPhoneDesk" runat="server" OnChanged="efPhoneDesk_OnChanged"></base:CheckBox>
                            </div>
                            <div class="disp_inlineBlockS w75" name="divPhone">
                                 стационарный
                            </div>
                            <div class="disp_inlineBlockS marginL" name="divPhone">
                                 <base:CheckBox ID="efPhoneIPCam" runat="server" IsDisabled="True" OnChanged="efPhoneIPCam_OnChanged"></base:CheckBox>
                            </div>
                            <div class="disp_inlineBlockS w75" name="divPhone">
                                 WEB-камера
                            </div>
                            
                            <div class="marginT" data-wp="1" style="display: none">
                                <div class="disp_inlineBlockS" >
                                     <base:CheckBox ID="efPhoneDect" runat="server" OnChanged="efPhoneDect_OnChanged"></base:CheckBox>
                                </div>
                                <div class="disp_inlineBlockS w170"  >
                                     переносная по офису трубка 
                                </div>
                            </div>
                        </div>
                        <div class="disp_inlineBlock" style="display: none;" id="divPLExit">
                              <base:ComboBox ID="efPLExit" runat="server" Width="185px" EmptyValueExist="False" OnChanged="efPLExit_OnChanged"></base:ComboBox>
                        </div>
                    </div>
                   
                </div>
            </div>
            <br style="display: none" data-wp="3"/>

            <div class="clearLeft" style="display: none" data-wp="3"></div>

            <div id="divMobilPhone" class="marginL marginT" style="display: none" data-phone="1">
                <div class="floatLeft w210">
                    Мобильный телефон сотрудника:
                </div>
                <div class="disp_inlineBlockS ">
                    <base:TextBox ID="efMobilphone" runat="server" Width="185" IsCaller="True"  CallerType="Contact" NextControl="efWorkPlaceType0_0" OnChanged="efMobilphone_OnChanged"></base:TextBox>
                    <div id="spMobilphoneArea" class="v4NtfInformation"><%RenderMobilphoneArea(Response.Output);%></div>
                </div>
            </div>
            
            <div class="clearLeft" style="display: none" data-wp="3"></div>
          
            <div id="divComputer" class="marginL marginT" style="display: none" data-wp="3">
                <div class="floatLeft marginR w75">
                    Компьютер:
                </div>
                
                <div class="disp_inlineBlock">
                    <div class="disp_inlineBlock">
                        <div class="disp_inlineBlockS">
                             <base:CheckBox ID="efComputer" runat="server" OnChanged="efComputer_OnChanged"></base:CheckBox>
                        </div>
                        <div class="disp_inlineBlockS w75">
                             настольный
                        </div>
                    </div>
                    <br/>
                    <div class="disp_inlineBlock marginT">
                         <div class="disp_inlineBlockS">
                            <base:CheckBox ID="efNotebook" runat="server" OnChanged="efNotebook_OnChanged"></base:CheckBox>
                        </div>
                        <div class="disp_inlineBlockS w75">
                            ноутбук 
                        </div>  
                    </div>
                </div>
            </div>

            <div class="clearLeft" style="display: none" data-wp="3"></div>
            
            <div id="divAdvEq" class="marginL marginT" style="display: none" data-wp="3">
                <div class="floatLeft w210">
                   Дополнительное оборудование: 
                </div>
                <div class="disp_inlineBlockS">
                     <base:TextArea ID="efAdvEq" runat="server" Width="285px" Rows="2"></base:TextArea>
                </div>
            </div>

            <div class="clearLeft" style="display: none" data-wp="3"></div>
            
            <div class="marginL disp_hr" style="display: none" data-wp="3">
                <hr/>
            </div>
            <br style="display: none" data-wp="3"/>

            <div id="divAccessEthernet" class="marginL marginT"  data-wp="7">
                 <div class="floatLeft w210">
                   Доступ к корпоративной сети: 
                </div>
                
                <div class="disp_inlineBlock" data-wp="7">
                    <div class="disp_inlineBlockS">
                        <base:CheckBox ID="efAccessEthernet" runat="server"  OnChanged="efAccessEthernet_OnChanged"></base:CheckBox>
                    </div>
                    <div class="disp_inlineBlock" style="display: none;" data-lan="1">
                        <div class="disp_inlineBlockS">
                           Логин:
                        </div>
                        <div class="disp_inlineBlockS">
                            <base:TextBox ID="efLogin" runat="server" Width="227px"></base:TextBox>
                        </div>
                    </div>
                </div>
            </div>
            <div class="clearLeft"></div>
            
            <div id="divLang" class="marginT marginL" style="display: none;" data-lan="1">
                <div class="floatLeft w210">
                   Предпочитаемый язык:
                </div>
                <div class="disp_inlineBlock">
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
                            <div class="floatLeft">
                                  <base:TextBox ID="efMailName" runat="server" Width="190px"></base:TextBox>
                            </div>
                            <div class="disp_inlineBlockS">
                                  <input type="button" class="v4s_btn" value="..." onclick="cmdasync('cmd','RenderMailNamesList', 'x', event.pageX, 'y', event.pageY);"/>
                            </div>
                        </div>
                        
                    <div class="disp_inlineBlock">
                        <div class="disp_inlineBlockS marginL w25">
                              <font style="font-size: 14pt;">@</font>
                        </div>
                        <div class="disp_inlineBlockS">
                          <base:ComboBox ID="efDomain" runat="server" Width="170px" EmptyValueExist="False"></base:ComboBox>
                        </div>
                    </div>
                </div>
            </div>
            <div class="clearLeft"></div>
       </div> 

        <div id="divGroup3"  style="display: none;" data-wp="7" data-lan="1">
            <div id="divHeader3"  class="directionGroupHeader marginT">Доступ к данным, необходимый сотруднику для работы</div>
            
            <div id="divSotrudnikParentContainer" class="floatLeft marginT marginL">
                <div class="disp_inlineBlock">
                    <div class="disp_inlineBlockS" >
                             <base:CheckBox ID="efSotrudnikParentCheck1" runat="server" OnChanged="efSotrudnikParentCheck1_OnChanged"></base:CheckBox>
                    </div>
                    <div class="disp_inlineBlockS w185">
                            как у сотрудника
                    </div>
                    <br/>
                    <div class="disp_inlineBlockS ">
                             <base:CheckBox ID="efSotrudnikParentCheck2" runat="server" OnChanged="efSotrudnikParentCheck2_OnChanged"></base:CheckBox>
                    </div>
                    <div class="disp_inlineBlockS w185">
                            вместо сотрудника
                    </div>
                    
                </div>
                <div id="divSotrudnikParent" class="disp_inlineBlockS marginT2 marginL">
                     <dbs:DBSEmployee ID="efSotrudnikParent" runat="server" Width="265" CLID="3" IsCaller="True"  CallerType="Employee" AutoSetSingleValue="True" OnChanged="efSotrudnikParent_OnChanged"></dbs:DBSEmployee>
                </div>
            </div>

            <div class="clearLeft"></div>
            <br/>
            
            <div class="marginL disp_hr" style="display: none" data-wp="3">
                <hr/>
            </div>
            
            <div class="clearLeft"></div>
            
            <div id="divPositionFolders" class="marginT marginL">
                
                <div class="floatLeft">
                    <div class="disp_inlineBlock w145">
                       Доступ к общим папкам:
                    </div>
                    <div class="disp_inlineBlock">
                        <a href="javascript:void(0);"  id="btnLinkCFAdd" onclick="cmdasync('cmd','AddPositionCommonFolders');" title="выбрать общие папки, к которым будет иметь доступ сотрудник"><img border="0" src="/styles/new.gif"/></a>
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
                        Выполняемые роли:
                    </div>
                    <div class="disp_inlineBlock">
                        <a href="javascript:void(0);"  id="btnLinkRolesAdd" onclick="cmdasync('cmd','AddPositionRole');" title="выбрать роль, которые сотрудник выполняет в компании"><img border="0" src="/styles/new.gif"/></a>
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
                        Доступ к типам лиц:
                    </div>
                    <div class="disp_inlineBlock">
                         <a href="javascript:void(0);"  id="btnLinkTypesAdd" onclick="cmdasync('cmd','AddPositionTypes');" title="выбрать типы лиц, к которым сотрудник имеет доступ"><img border="0" src="/styles/new.gif"/></a>
                    </div>
                </div>
                <div class="disp_inlineBlock">
                        <span id="divPositionTypesData"></span>
                </div>
            </div>

            <div class="clearLeft"></div>

            <div id="divPositionAdv" class="marginT2 marginL">
                <div class="floatLeft">
                    <div class="disp_inlineBlock w145">
                        Дополнительные права:
                    </div>
                    <div class="disp_inlineBlock">
                        <a href="javascript:void(0);"  id="btnLinkAGAdd" onclick="cmdasync('cmd','AddPositionAdvancedGrants');" title="выбрать дополнительные права"><img border="0" src="/styles/new.gif"/></a>
                    </div>
                </div>
                <div class="disp_inlineBlock">
                        <span id="divAdvancedGrantsData"></span>
                </div>
            </div>  
        </div>

        <div class="clearLeft"></div>

        <div class="marginL disp_hr" style="display: none;" data-block="dblEthernet"><hr/></div>
        
        <br/>
        
        <div id="divGroup4">
            <div id="divAdvInfo" class="marginT">
                 <div class="floatLeft w145">
                    Доп. информация по организации работы:
                </div>
                <div class="disp_inlineBlock">
                    <base:TextArea ID="efAdvInfo" runat="server" Width="298px" Rows="3"></base:TextArea>
                </div>
            </div>
            <div class="marginT2">
             <% StartRenderVariablePart(Response.Output,145,250,0,1, true); %>   
             <% EndRenderVariablePart(Response.Output); %>
            </div>
        </div>
    </div>
     
     <!--================ отображение дополнительной информации        ================-->
    <div id = "divAdvInfoValidation" style="display: none;">
        <div id="divAdvInfoValidation_Body" class="marginL marginR marginT"></div>        
    </div>

    <!--================ Список доступных компьютеризированных рабочих мест        ================-->
    
    <div id = "divWorkPlaceList" style="display: none;">
        <div id="divWPL_Body" class="marginL marginR marginT"></div>        
    </div>
     
    <!--================ Список доступных имен почтовых ящиков      ================-->
    
    <div id = "divMailNames" style="display: none;">
        <div id="divMN_Body" class="marginL marginR marginT"></div>
    </div>
    
    <!--================ Выбор общих папок                  ================-->
  
     <div id = "divCommonFoldersAdd" style="display: none;">
        <div id="divCF_Body" class="marginL marginR marginT" style="height: 250px; overflow: auto"></div>        
    </div>


    <!--================ Редактирование позиции Ролей       ================-->

    <div id = "divPositionRolesAdd" data-id="" style="display: none;">
        <div id="divPRoles_Body" class="marginL marginR marginT">
            <div class="marginT marginL">
                <div class="floatLeft w100">Сотрудник:</div>
                <div id="divPRoles_Employee" class="disp_inlineBlock"></div>
            </div>
            <div><hr style="color: gainsboro; background-color: gainsboro; border: none;height: 2px;"/></div>
            <div class="marginT marginL">
                <div class="floatLeft w100">Выполняет роль:</div>
                <div class="disp_inlineBlock">
                    <dbs:DBSRole ID="efPRoles_Role" runat="server" Width="300" OnChanged="efPRoles_Role_OnChanged" AutoSetSingleValue="True" IsRequired="True"></dbs:DBSRole>
                </div>
            </div>
            <div class="marginL" id="efPRoles_Role_Description"></div>
            <div class="marginT2 marginL">
                <div class="floatLeft w100">В компании:</div>
                <div class="disp_inlineBlock">
                    <dbs:DBSPerson ID="efPRoles_Person" runat="server" IsCaller="True" CallerType="Person" Width="300" AutoSetSingleValue="True"></dbs:DBSPerson>
                </div>
            </div>
            <div style = "width:98%; position:absolute; bottom:5px; left:0;">
                <base:Changed ID="efChanged" runat="server"></base:Changed>
            </div>
        </div>
    </div>
    
    <!--================ Редактирование позиции Типов       ================-->

    <div id = "divPositionTypesAdd" data-id="" style="display: none;">
        <div id="divPTypes_Body" class="marginL marginR marginT">
            <div class="marginT marginL">
                <div class="floatLeft w100">Сотрудник:</div>
                <div id="divPTypes_Employee" class="disp_inlineBlock"></div>
            </div>
            <div><hr style="color: gainsboro; background-color: gainsboro; border: none;height: 2px;"/></div>
            <div class="marginT marginL">
                <div class="floatLeft w100">Каталог:</div>
                <div class="disp_inlineBlock">
                    <dbs:DBSPersonCatalog ID="efPTypes_Catalog" IsNotUseSelectTop="True" IsRequired="True" IsCustomRecordInPopup="True" CustomRecordId="0" CustomRecordText="<все каталоги>" runat="server" Width="300" OnChanged="efPTypes_Catalog_OnChanged" AutoSetSingleValue="True"></dbs:DBSPersonCatalog>
                </div>
            </div>
            <div class="marginT2 marginL">
                <div class="floatLeft w100">Тип лица:</div>
                <div class="disp_inlineBlock">
                    <dbs:DBSPersonTheme ID="efPTypes_Type" runat="server" IsCustomRecordInPopup="True" CustomRecordId="0" CustomRecordText="<все типы лиц>" IsSelectOnlyCustomRecord="True" IsMultiSelect="true" IsMultiReturn="True" IsRow="False" IsRemove="true" Width="300" OnChanged="efPTypes_Type_OnChanged"></dbs:DBSPersonTheme>
                </div>
            </div>
        </div>
    </div>
    
     <!--================ Выбор дополнительных прав                  ================-->
    
     <div id = "divAdvancedGrantAdd" style="display: none;">
        <div id="divAG_Body" class="marginL marginR marginT"></div>
    </div>
   
   
    <script type="text/javascript">


        
        function directions_initNewDoc() {

        }
        
        $(document).ready(function () {
        
        });

        
    </script>
</body>
</html>

