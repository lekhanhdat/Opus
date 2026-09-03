<%--Covered by AvePoint copyright and license agreement--%>
<%@ Assembly Name="Microsoft.SharePoint, Version=15.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c"%> 
<%@ Page Language="C#" Inherits="Microsoft.SharePoint.WebPartPages.WikiEditPage" MasterPageFile="~masterurl/default.master" MainContentID="PlaceHolderMain" %> 
<%@ Import Namespace="Microsoft.SharePoint.WebPartPages" %> 
<%@ Register Tagprefix="SharePoint" Namespace="Microsoft.SharePoint.WebControls" Assembly="Microsoft.SharePoint, Version=15.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %> 
<%@ Register Tagprefix="Utilities" Namespace="Microsoft.SharePoint.Utilities" Assembly="Microsoft.SharePoint, Version=15.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %> 
<%@ Import Namespace="Microsoft.SharePoint" %> 
<%@ Assembly Name="Microsoft.Web.CommandUI, Version=15.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %>

<asp:Content ContentPlaceHolderId="PlaceHolderAdditionalPageHead" runat="server">
    <%--<meta charset="UTF-8">--%>
    <style type="text/css">
        .ms-cui-topBar2,#s4-titlerow,#titlerow,#sideNavBox,#pageContentTitle,#notificationArea,#pageStatusBar{
            display:none !important;
        }
        #contentBox {
            margin-right: auto;
            margin-left: auto;
            text-align: center;
            font-family: 'Segoe UI', sans-serif;
		    font-size: 28px;
		    line-height: 42px;
            height:100%;
        }
        #contentRow {
            padding-top:0px;
            height:100%;
        }
        #DeltaPlaceHolderMain {
            height:100%;
        }
        #s4-bodyContainer {
            height: calc(100% - 35px);
        }
        .custom-placeHolder {
            height: calc(100% / 3);
        }
    </style>
</asp:Content>
<asp:Content ContentPlaceHolderId="PlaceHolderMain" runat="server">
    <div class="custom-placeHolder"></div>
    <div>This document has been disposed of.</div>
    <div>Please contact your RM Team for more details.</div>
</asp:Content>