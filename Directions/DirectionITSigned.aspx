<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="DirectionITSigned.aspx.cs" Inherits="Kesco.App.Web.Docs.Directions.DirectionItSigned" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD HTML 4.0 Transitional//EN">

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
    <link href="Kesco.Directions.css" rel="stylesheet" type="text/css"/>
    <script language="JavaScript" type="text/javascript" src="Kesco.Directions.js"></script>
</head>
<body>
<%= RenderDocumentHeader() %>

<div class="v4FormContainer">
    <% RenderNoSignSupervisor(Response.Output); %>
    <div id="divGroup1" class="marginT2">
        <div id="divPhoto" class="floatLeft"><% RenderData.Photo(this, Response.Output, Dir); %></div>

        <div class="disp_inline w300" style="height: 1px"></div><br/>

        <div class="marginL disp_inline">
            <div class="marginL disp_inlineBlockS">
                <div id="divSotrudnikInfo"><% RenderData.Sotrudnik(this, Response.Output, Dir); %></div>
                <div>
                    <span id="spSotrudnikPost" style="display: inline-block"><% RenderData.SotrudnikPost(this, Response.Output, Dir); %></span>
                    <span id="spSotrudnikAOrg" style="display: inline-block"><% RenderData.SotrudnikAOrg(this, Response.Output, Dir); %></span>
                </div>
                <div id="divSotrudnikFinOrg"><% RenderData.SotrudnikFinOrg(this, Response.Output, Dir); %></div>
                <div id="divCommonEmployee"><% RenderData.CommonEmployee(this, Response.Output, Dir); %></div>
                <div id="divSotrudnikCadrWorkPlaces"><% RenderData.SotrudnikCadrWorkPlaces(this, Response.Output, Dir); %></div>
                <div id="divSupervisor"><% RenderData.Supervisor(this, Response.Output, Dir); %></div>
            </div><br>
            <div id="divSotrudnikWPType"><% RenderData.WorkPlaceType(this, Response.Output, Dir, false); %></div>
        </div>
    </div>
    <div id="divMobilPhone" class="marginT disp_inline"><% RenderMobilPhone(Response.Output); %></div>
    <div id="divData"><% RenderFormData(Response.Output); %></div>
    <div class="marginT2">
        <% StartRenderVariablePart(Response.Output, 145, 250, 0, 1, true); %>
        <% EndRenderVariablePart(Response.Output); %>
    </div>
</div>

<!--================ отображение дополнительной информации        ================-->
<div id="divAdvInfoValidation" style="display: none;">
    <div id="divAdvInfoValidation_Body" class="marginL marginR marginT"></div>
</div>
</body>
<script language="javascript">
    <!--

    function copyToClipboard(text) {
        //text=text.substring(text.indexOf("(",0),text.length);
        text = text.replace(RegExp('[^0-9\+]{1,}', 'ig'), '');
        //alert(text);
        window.clipboardData.setData("Text", text);
    }

//-->
</script>
</html>