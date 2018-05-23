<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="f1.aspx.cs" Inherits="Kesco.App.Web.Docs.Directions.Maket.f1" %>
<%@ Register TagPrefix="dbs" Namespace="Kesco.Lib.Web.DBSelect.V4" Assembly="DBSelect.V4" %>
<%@ Register TagPrefix="base" Namespace="Kesco.Lib.Web.Controls.V4" Assembly="Controls.V4" %>
<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Указание в отдел ИТ на организацию работы (Новый документ)</title>
</head>
<body style="margin-left:50px">
   <h3>Указание в отдел ИТ на организацию работы (Новый документ)</h3>
   
   <table border="1" style="border-collapse: collapse;">
       <tr style="height: 200px" valign="top">
           <td style="width:600px">
               <table>
                <tr>
                       <td colspan="3" align="left" style="font-weight:bold;">
                           Информация о сотруднике:
                       </td>
                   </tr>
                   <tr>
                       <td colspan="3" align="right">
                           <table>
                               <tr>
                                   <td>Сотрудник:</td>
                                   <td><dbs:DBSEmployee ID="DBSEmployee1" runat="server" Value="1684"  Width="250"></dbs:DBSEmployee></td>
                               </tr>
                           </table>
                       </td>
                   </tr>
                   <tr>
                       <td><img src="http://ktz-nick.testcom.com/users/photo.ashx?id=724&w=120"/></td>
                       <td valign="top">
                           Руководитель службы поддержки пользователей<br>
                           Ответственная компания: ООО "КЕСКО-М" 
                           <table>
                               <tr style="height:15px"><td colspan="2">&nbsp;</td></tr>
                               <tr>
                                     <td>Непосредственный руководитель:</td>
                                     <td><dbs:DBSEmployee ID="efSupervisor" runat="server"  Width="250" Value="1"></dbs:DBSEmployee></td>             
                               </tr>
                               <tr>
                                   <td colspan="2" >
                                       начальник отдела информационных технологий
                                   </td>
                               </tr>
                               <tr style="height:15px"><td colspan="2">&nbsp;</td></tr>
                               <tr >
                                   <td>Мобильный телефон:</td>
                                   <td><base:TextBox ID="efMobilphone" runat="server" Width="150" Value="+7 (916) 1370145"></base:TextBox></td>
                               </tr>
                               <tr>
                                   <td colspan="2">
                                       Россия, Московская область
                                   </td>
                               </tr>
                           </table>
                       </td>
                   </tr>
                   </table>
                </td>
       </tr>
       <tr>
           <td align="right">
               <input type="button" value="Далее >>" width="100px" onclick="NextStep();"/>
               <input type="button" value="Отмена" width="100px"/>
           </td>
       </tr>
   </table>
</body>

<script>

    function NextStep() {

        if (parent == null) return;

        parent.ResetFrameData("f2.aspx");
    
    }
</script>
</html>
