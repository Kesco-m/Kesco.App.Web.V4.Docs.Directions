<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="f.aspx.cs" Inherits="Kesco.App.Web.Docs.Directions.Maket.f" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
</head>
<body>
    <iframe src="" id="frData" style="width:600px; height:300px;" frameborder="0"></iframe>
</body>
<script language=javascript>

    ResetFrameData("f1.aspx");
    function ResetFrameData(url) {

        $('#frData').attr('src', url)

    }

</script>
</html>
