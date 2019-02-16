<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="SurveyWebSiteExample._Default" UICulture="en-US" %>

<%@ Register TagName="Survey" TagPrefix="camelot" Src="~/SurveyControl.ascx" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
</head>
<body>
    <form id="form1" runat="server">
    <div>
        <camelot:Survey ID="Survey" runat="server" ListName="Cats" ConnectionString="sharepoint_connection"></camelot:Survey>
    </div>
    </form>
</body>
</html>
