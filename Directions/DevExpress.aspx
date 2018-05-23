<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="DevExpress.aspx.cs" Inherits="Kesco.App.Web.Docs.Directions.DevExpress" %>
<%@ Register TagPrefix="dbs" Namespace="Kesco.Lib.Web.DBSelect.V4" Assembly="DBSelect.V4" %>

<%@ Register assembly="DevExpress.Web.v17.2, Version=17.2.5.0, Culture=neutral, PublicKeyToken=b88d1754d700e49a" namespace="DevExpress.Web" tagprefix="dx" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml" style="overflow: hidden">
<head runat="server">

</head>
<body>
 
   <dbs:DBSEmployee ID="efSotrudnik" runat="server" Width="258" CLID="3" IsCaller="True" CallerType="Employee" AutoSetSingleValue="True" OnChanged="efSotrudnik_OnChanged"  NextControl="efSupervisor" MaxItemsInPopup="100"></dbs:DBSEmployee>

    <form id="form1" runat="server">
        
    <dx:ASPxGridView ID="DevGrid" runat="server" AutoGenerateColumns="False" 
        DataSourceID="SqlDataSource1" EnableTheming="True" KeyboardSupport="True" 
        KeyFieldName="КодРоли" Theme="SoftOrange">
        <SettingsContextMenu Enabled="True" EnableFooterMenu="True" 
            EnableGroupFooterMenu="True" EnableGroupPanelMenu="True" EnableRowMenu="True">
        </SettingsContextMenu>
        <SettingsPager PageSize="20" 
            ShowSeparators="True">
        </SettingsPager>
        <Settings HorizontalScrollBarMode="Auto" ShowFilterBar="Visible" 
            ShowFilterRow="True" ShowFilterRowMenu="True" ShowGroupPanel="True" 
            ShowHeaderFilterButton="True" ShowPreview="True" ShowTitlePanel="True" 
            VerticalScrollBarMode="Auto" />
        <SettingsBehavior AllowSelectSingleRowOnly="True" AllowFocusedRow="True" 
            ConfirmDelete="True" />
        <SettingsResizing ColumnResizeMode="Control" />
        <SettingsDataSecurity AllowDelete="False" AllowEdit="False" 
            AllowInsert="False" />
        <SettingsSearchPanel Visible="True" />
        <EditFormLayoutProperties>
            <SettingsAdaptivity AdaptivityMode="SingleColumnWindowLimit" />
        </EditFormLayoutProperties>
        <Columns>
            <dx:GridViewDataTextColumn FieldName="КодРоли" ReadOnly="True" VisibleIndex="1">
            </dx:GridViewDataTextColumn>
            <dx:GridViewDataTextColumn FieldName="Роль" VisibleIndex="2" Width="100%">
            </dx:GridViewDataTextColumn>
            <dx:GridViewDataTextColumn FieldName="Сотрудник" ReadOnly="True" 
                VisibleIndex="3" Width="100%">
            </dx:GridViewDataTextColumn>
            <dx:GridViewDataTextColumn FieldName="Кличка" VisibleIndex="4" Width="100%">
            </dx:GridViewDataTextColumn>
        </Columns>
        
    </dx:ASPxGridView>

    <asp:SqlDataSource ID="SqlDataSource1" runat="server" 
        ConnectionString="<%$ ConnectionStrings:ИнвентаризацияConnectionString %>" 
        SelectCommand="SELECT Роли.КодРоли, Роли.Роль, Сотрудники.Сотрудник, ЛицаЗаказчики.Кличка FROM Роли INNER JOIN РолиСотрудников ON Роли.КодРоли = РолиСотрудников.КодРоли INNER JOIN Сотрудники ON РолиСотрудников.КодСотрудника = Сотрудники.КодСотрудника LEFT OUTER JOIN ЛицаЗаказчики ON РолиСотрудников.КодЛица = ЛицаЗаказчики.КодЛица">
    </asp:SqlDataSource>
    
   

    </form>
 


</body>



</html>
