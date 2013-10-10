<%@ Page Language="C#" MasterPageFile="~masterurl/default.master" Inherits="Microsoft.SharePoint.WebPartPages.WebPartPage,Microsoft.SharePoint,Version=12.0.0.0,Culture=neutral,PublicKeyToken=71e9bce111e9429c" %>
<%@ Register TagPrefix="dsc" Namespace="Microsoft.TeamFoundation.SharePoint.Dashboards.Controls" Assembly="Microsoft.TeamFoundation.SharePoint.Dashboards.Controls, Version=10.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a" %>
<%@ Register TagPrefix="SharePoint" Namespace="Microsoft.SharePoint.WebControls" Assembly="Microsoft.SharePoint, Version=12.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %>
<%@ Register TagPrefix="Utilities" Namespace="Microsoft.SharePoint.Utilities" Assembly="Microsoft.SharePoint, Version=12.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %>
<%@ Import Namespace="Microsoft.SharePoint" %>
<%@ Register TagPrefix="WebPartPages" Namespace="Microsoft.SharePoint.WebPartPages" Assembly="Microsoft.SharePoint, Version=12.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %>

<asp:Content ID="HeadTitle" ContentPlaceHolderID="PlaceHolderPageTitle" runat="server">
	<SharePoint:FieldValue FieldName="Title" runat="server" />
</asp:Content>

<asp:Content ID="ContentBodyAreaClass" ContentPlaceHolderID="PlaceHolderBodyAreaClass" runat="server">
<style type="text/css">
.toolbarcontainer {
background-image: url(/_layouts/Microsoft.TeamFoundation/images/toptoolbarBKGD.gif);
background-repeat: repeat-x;
padding: 4px 6px 4px 6px;
color: white;
margin-bottom: 6px;
}
.toolbar_item {
font-size: 11px;
padding-right: 3px;
color: #ffffff;
}
.toolbar_item_button {
border: 1px solid #8B9FC8;
padding: 2px 5px 2px 2px;
cursor: pointer;
}
.toolbar_item_button_hover {
border: 1px solid #3A548D;
padding: 2px 5px 2px 2px;
cursor: pointer;
background-image: url(/_layouts/Microsoft.TeamFoundation/images/tmbgrp.gif);
}
.toolbar_item_button a {
white-space: nowrap;
text-decoration: none;
color: #ffffff;
}
.toolbar_item_button_hover a, .toolbar_item_button_hover a:hover {
white-space: nowrap;
text-decoration: none;
color: #000000;
}

/* Dashboard layout */
.ms-bodyareaframe {
padding: 0px;
}
.ms-dashboards {
margin:10px;
}
.ms-dashboard-main {
width: 100%;
padding: 0;
}
.ms-dashboard-layout-table {
width: 1%;
}
.ms-dashboard-main .ms-WPHeader td, .ms-dashboard-side .ms-WPHeader td {
border-style: none;
border-width: 0;
}
.ms-dashboard-main .ms-WPBorder {
border-style: none;
border-width: 0;
}
.ms-dashboard-layout-table .ms-WPBody .ms-sectionline {
display: none;
}
.ms-dashboard-layout-table .ms-WPTitle {
padding-left: 6px;
}
.ms-dashboard-zone-cell {
vertical-align: top;
padding: 20px 0 0 0;
}
.ms-dashboard-rpad {
padding-right: 9px;
}
.ms-dashboard-side {
width: 1%;
vertical-align: top;
padding-left: 6px;
}
.ms-dashboard-main .ms-PartSpacingVertical, .ms-dashboard-side .ms-PartSpacingVertical {
margin-top: 20px;
}
.ms-dashboard-main .ms-PartSpacingHorizontal {
margin-right: 2px;
}

/* Unmapped Control */
.ms-dashboards-error {
display:block;
margin: 6px;
padding: 6px;
border: 1px solid #d94e5a;
color: #c93f4e;
}

/* Cube Control */
.ms-dashboards-warning 
{
color: Red;
}
</style>
</asp:Content>

<asp:Content ID="Main" ContentPlaceHolderID="PlaceHolderMain" runat="server">
	<div id="TfsDashboardsToolBar" class="toolbarcontainer">
		<table cellpadding="0" cellspacing="0" border="0">
			<tr>
				<td class="toolbar_item">
					<SharePoint:DelegateControl id="tfsPortalLink" runat="server" ControlId="PortalLink" />
				</td>

				<td class="toolbar_item">
					<SharePoint:DelegateControl id="tfsNewWorkItems" runat="server" ControlId="NewWorkItemLinks" />
				</td>

				<SharePoint:SPSecurityTrimmedControl runat="server" PermissionsString="OpenItems,ViewListItems">
				<td class="toolbar_item">
					<div id="_invisibleIfEmpty" name="_invisibleIfEmpty" class="toolbar_item_button" onmouseover="this.className='toolbar_item_button_hover';" onmouseout="this.className='toolbar_item_button';">
					<dsc:NewExcelReportButton ID="NewExcelReportButton" runat="server" CssClass=""
						ImageUrl="/_layouts/Microsoft.TeamFoundation/images/icon_newexcel.gif" />
					</div>
				</td>
				</SharePoint:SPSecurityTrimmedControl>

				<SharePoint:SPSecurityTrimmedControl runat="server" PermissionsString="ViewFormPages,OpenItems,AddListItems,EditListItems">
				<td class="toolbar_item" style="white-space:nowrap">
					<div id="_invisibleIfEmpty" name="_invisibleIfEmpty" class="toolbar_item_button" onmouseover="this.className='toolbar_item_button_hover';" onmouseout="this.className='toolbar_item_button';">
					<dsc:CopyDashboardButton ID="CopyDashboardButton" runat="server" CssClass=""
						ImageUrl="/_layouts/Microsoft.TeamFoundation/images/icon_newdashboard.gif"
						NavigateUrl="~site/_layouts/Microsoft.TeamFoundation/CopyDashboardPage.aspx?List={ListId}&amp;Item={ItemId}" />
					</div>
				</td>
				</SharePoint:SPSecurityTrimmedControl>
				
				<td class="toolbar_item" style="text-align:right;width:99%">
					<SharePoint:DelegateControl id="tfsGoToWorkItem" runat="server" ControlId="GoToWorkItem" />
				</td>
			</tr>
		</table>
	</div>

	<dsc:UnmappedControl id="ProjectMessageControl" runat="server" CssClass="ms-dashboards-error" />

	<table class="ms-dashboards" cellpadding="0" cellspacing="0" border="0">
		<tr>
			<td class="ms-dashboard-main ms-dashboard-rpad" valign="top">
				<WebPartPages:WebPartZone ID="Header" runat="server" Title="Header" AllowPersonalization="true" />

				<dsc:WebPartCssStyling runat="server" CssClassToApply="ms-WPBody" TargetTypeName="Microsoft.Office.Excel.WebUI.ExcelWebRenderer" />

				<table class="ms-dashboard-layout-table" cellpadding="0" cellspacing="0" border="0">

					<tr>
						<td class="ms-dashboard-zone-cell">
							<WebPartPages:WebPartZone ID="Row1" runat="server" Title="Row1" LayoutOrientation="Horizontal" PartChromeType="TitleAndBorder" AllowPersonalization="true" />
						</td>
					</tr>

					<tr>
						<td class="ms-dashboard-zone-cell">
							<WebPartPages:WebPartZone ID="Row2" runat="server" Title="Row2" LayoutOrientation="Horizontal" PartChromeType="TitleAndBorder" AllowPersonalization="true" />
						</td>
					</tr>

					<tr>
						<td class="ms-dashboard-zone-cell">
							<WebPartPages:WebPartZone ID="Row3" runat="server" Title="Row3" LayoutOrientation="Horizontal" PartChromeType="TitleAndBorder" AllowPersonalization="true" />
						</td>
					</tr>

					<tr>
						<td class="ms-dashboard-zone-cell">
							<WebPartPages:WebPartZone ID="Row4" runat="server" Title="Row4" LayoutOrientation="Horizontal" PartChromeType="TitleAndBorder" AllowPersonalization="true" />
						</td>
					</tr>

					<tr>
						<td class="ms-dashboard-zone-cell">
							<WebPartPages:WebPartZone ID="Row5" runat="server" Title="Row5" LayoutOrientation="Horizontal" PartChromeType="TitleAndBorder" AllowPersonalization="true" />
						</td>
					</tr>

					<tr>
						<td class="ms-dashboard-zone-cell">
							<WebPartPages:WebPartZone ID="Footer" runat="server" Title="Footer" PartChromeType="TitleAndBorder" AllowPersonalization="true" />
						</td>
					</tr>
				</table>
				<table width="100%">
					<tr>
						<td class="ms-vb">
							<p><dsc:CubeProcessedView ID="CubeProcessedView1" runat="server" /></p>
						</td>
					</tr>
				</table>
			</td>
			<td valign="top">
				<table class="ms-dashboard-side" cellpadding="0" cellspacing="0" border="0">
					<tr>
						<td>
							<WebPartPages:WebPartZone ID="Right" runat="server" Title="Right" PartChromeType="TitleOnly" AllowPersonalization="true" />
						</td>
					</tr>
				</table>
			</td>
		</tr>
	</table>
	<script language="javascript" type="text/javascript">if(typeof(MSOLayout_MakeInvisibleIfEmpty) == "function") {MSOLayout_MakeInvisibleIfEmpty();}</script>
</asp:Content>

<asp:Content ContentPlaceHolderID="PlaceHolderPageTitleInTitleArea" runat="server">
	<SharePoint:FieldValue FieldName="Title" runat="server" />
</asp:Content>

<asp:Content ContentPlaceHolderID="PlaceHolderPageImage" runat="server">
	<img width="145" height="54" src="/_layouts/Microsoft.TeamFoundation/images/dashboard.png" alt="Dashboard" />
</asp:Content>
