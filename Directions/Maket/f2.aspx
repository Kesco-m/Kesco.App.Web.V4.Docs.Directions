<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="f2.aspx.cs" Inherits="Kesco.App.Web.Docs.Directions.Maket.f2" %>
<%@ Register TagPrefix="base" Namespace="Kesco.Lib.Web.Controls.V4" Assembly="Controls.V4" %>
<%@ Register TagPrefix="dbs" Namespace="Kesco.Lib.Web.DBSelect.V4" Assembly="DBSelect.V4" %>
<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
</head>
<body style="margin-left:50px">
    <h3>Указание в отдел ИТ на организацию работы (Новый документ)</h3>

    <table border="1" style="border-collapse: collapse;">
        <tr style="height: 200px" valign="top">
            <td style="width: 600px">
                <table>
                    <tr>
                        <td colspan="3" align="left" style="font-weight: bold;">
                            Где будет работать Жданов Семен Владимирович?
                        </td>
                    </tr>
                    <tr>
                        <td colspan="3">
                            <input type="radio" runat="server" id="location_1_1" name="location_group_1"  onchange="DisplaySettings('div_location_1', 1);" /><label for="location_1_1">на своем рабочем месте в офисе</label><br />
                               <div style="margin-left:20px; display:none;" id="div_location_1" level=1>
                                    <dbs:DBSEmployee ID="efLocation0" runat="server"  Width="350" ></dbs:DBSEmployee><br/>
                                    <div style="display:none">
                                    <input type="radio" runat="server" id="location_2_1" name="location_group_2"  onchange="DisplaySettings('div_location_3', 2);" /><label for="location_2_1">организовать работу на существующем рабочем месте</label><br />
                                    <div style="margin-left:20px; display:none;" id="div_location_3" level=2>
                                        <input type="radio" runat="server" id="location_3_1" name="location_group_3"  /><label for="location_3_1">Москва/Кутузовский_11/Офис_Кутузовский/Южная_зона_S/S-21</label><br />
                                        <input type="radio" runat="server" id="location_3_2" name="location_group_3"  /><label for="location_3_2">Москва/Кутузовский_11/Офис_Кутузовский/Южная_зона_S/S-22</label><br />
                                        <input type="radio" runat="server" id="location_3_3" name="location_group_3"  /><label for="location_3_3">Москва/Кутузовский_11/Офис_Кутузовский/Южная_зона_S/S-23</label><br />
                                    </div>    
                                        
                                                                  
                                     <input type="radio" runat="server" id="location_2_2" name="location_group_2"  onchange="DisplaySettings('div_location_4', 2);" /><label for="location_2_2">организовать работу на другом рабочем месте</label><br />
                                     <div style="margin-left:20px; display:none;" id="div_location_4" level=2>
                                        <dbs:DBSEmployee ID="efLocation1" runat="server"  Width="350" ></dbs:DBSEmployee><br/>
                                        <input type="radio" runat="server" id="location_4_1" name="location_group_4" onchange="DisplaySettings('div_location_5', 3);" /><label for="location_4_1">переезд с IT-оборудованием с </label><br />
                                            <div style="margin-left:20px; display:none;" id="div_location_5" level=3>
                                                <input type="radio" runat="server" id="location4_1" name="location_group_5"  /><label for="location4_1">Москва/Кутузовский_11/Офис_Кутузовский/Южная_зона_S/S-21</label><br />
                                                <input type="radio" runat="server" id="location4_2" name="location_group_5"  /><label for="location4_2">Москва/Кутузовский_11/Офис_Кутузовский/Южная_зона_S/S-22</label><br />
                                                <input type="radio" runat="server" id="location4_3" name="location_group_5"  /><label for="location4_3">Москва/Кутузовский_11/Офис_Кутузовский/Южная_зона_S/S-23</label><br />
                                            </div>
                                        <input type="radio" runat="server" id="location_4_2" name="location_group_4" onchange="DisplaySettings('', 3);"/><label for="location_4_2">дополнительное рабочее место</label><br />
                                    </div>
                                       
                                    </div>

                                </div> 


							<input type="radio" runat="server" id="location_1_2" name="location_group_1" onchange="DisplaySettings('', 1);" /><label for="location_1_2">на гостевом рабочем месте в офисе с доступом в сеть</label><br />
                            <input type="radio" runat="server" id="location_1_3" name="location_group_1" onchange="DisplaySettings('div_location_6', 1);" /><label for="location_1_3">удаленно</label><br />
                             <div style="margin-left:20px; display:none;" id="div_location_6" level=1>
                                  <input type="radio" runat="server" id="location_6_1" name="location_group_6" /><label for="location_6_1">из дома</label><br />
                                  <input type="radio" runat="server" id="location_6_2" name="location_group_6" /><label for="location_6_2">мобильно</label><br />
                                 
                             </div>  
                                
                        </td>
                    </tr>
                </table>
            </td>
        </tr>
        <tr>
            <td align="right">
                <input type="button" value="<< Назад" width="100px" onclick="PrevStep();" />
                <input type="button" value="Далее >>" width="100px" onclick="NextStep();" />
                <input type="button" value="Отмена" width="100px" />
            </td>
        </tr>
    </table>
</body>
<script>

    function DisplaySettings(name, level) {

        $('div[level]').each(function (index) {
            var l = parseInt($(this).attr("level"));

            if (l >= level) {
                $(this).css("display", "none");
                $(this).children("input[type='radio']").prop("checked", false);
            }
        });

        if (name!=null && name.length > 0)       
            $("#" + name).css("display", "block");
    
    
    }

    function PrevStep() {

        if (parent == null) return;

        parent.ResetFrameData("f1.aspx");

    }

    function NextStep() {

        if (parent == null) return;

        parent.ResetFrameData("f3.aspx");

    }

</script>
</html>
