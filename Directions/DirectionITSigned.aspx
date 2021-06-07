<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="DirectionITSigned.aspx.cs" Inherits="Kesco.App.Web.Docs.Directions.DirectionItSigned" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD HTML 4.0 Transitional//EN">

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
    <link href="Kesco.Directions.css?v=5" rel="stylesheet" type="text/css"/>
    <script language="JavaScript" type="text/javascript" src="Kesco.Directions.js?v=5"></script>
</head>
<body>
<%= RenderDocumentHeader() %>

<div class="v4formFilter marginL">
    <% RenderCheckTypeDirection(Response.Output); %>
    <% RenderNoSignSupervisor(Response.Output); %>

    <div id="divPhoto" class="block1 paddingT"><% RenderData.Photo(this, Response.Output, Dir); %></div>

    <div class="marginL marginT2 block2">

        <div id="divSotrudnikInfo"><% RenderData.Sotrudnik(this, Response.Output, Dir); %></div>
        <div id="divSotrudnikPost"><% RenderData.SotrudnikPost(this, Response.Output, Dir, false); %></div>
        <div id="divSotrudnikFinOrg"><% RenderData.SotrudnikFinOrg(this, Response.Output, Dir); %></div>
        <div id="divSupervisor"><% RenderData.Supervisor(this, Response.Output, Dir); %></div>        
        <div id="divSotrudnikCadrWorkPlaces"><% RenderData.SotrudnikCadrWorkPlaces(this, Response.Output, Dir); %></div>
        <div id="divSotrudnikAnotherWorkPlaces" style="display: none;"></div>
    </div>
    <br/>
    <div id="divSotrudnikWPType" class="marginT v4Bold" style="text-align: center"><% RenderData.WorkPlaceType(this, Response.Output, Dir); %></div>
</div>

<div class="v4formContainer marginL">

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

<div class="clearBoth"></div>
</body>
<script language="javascript">
    <!--

    function copyToClipboard(text) {        
        text = text.replace(RegExp('[^0-9\+]{1,}', 'ig'), '');        
        window.clipboardData.setData("Text", text);
    }

//-->
</script>
</html>