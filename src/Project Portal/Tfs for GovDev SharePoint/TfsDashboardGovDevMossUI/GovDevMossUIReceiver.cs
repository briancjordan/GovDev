//-----------------------------------------------------------------------
// <copyright file="GovDevMossUIReceiver.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Dpe.Ps.Govdev
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;
    using System.Web.UI.WebControls.WebParts;
    using Microsoft.Office.Excel.WebUI;
    using Microsoft.SharePoint;
    using Microsoft.SharePoint.WebPartPages;
    using Microsoft.TeamFoundation.WebAccess.WebParts;

    /// <summary>
    /// Feaure Reciver class for changing BurnDown Dashboard as part of the
    /// GovDev for TFS Process Template on MOSS
    /// </summary>
    [CLSCompliant(false)]
    [Guid("f8edf4c0-efb5-4be5-b31f-03a52396e171")]
    public class GovDevMossUIReceiver : BaseFeatureReceiver
    {
        /// <summary>
        /// List of Dashoard Pages in MOSS
        /// </summary>
        private List<string> dashboardList = new List<string>() 
                                                              {
                                                               "Dashboards/Burndown.aspx", 
                                                               "Dashboards/Quality.aspx",
                                                               "Dashboards/Bugs.aspx",
                                                               "Dashboards/Test.aspx",
                                                               "Dashboards/Build.aspx"
                                                              };

        /// <summary>
        /// Override implementation to handle feature activation
        /// </summary>
        protected override void OnActivate()
        {
            this.WriteDebug("Entering GovDevMossUIReceiver.OnActivate method.");
            this.UpdateBurndownDashboard();
            this.UpdateTestDashboard();
            this.UpdateMossProjectWorkItems();
            this.WriteDebug("Leaving GovDevMossUIReceiver.OnActivate method.");
        }

        /// <summary>
        /// Updates the BurnDown Dashboard in the MOSS site
        /// </summary>
        private void UpdateBurndownDashboard()
        {
            this.WriteDebug("Entering GovDevMossUIReceiver.UpdateBurndownDashboard method.");
            SPFile dashboard = null;

            if ((dashboard = Web.GetFile("Dashboards/Burndown.aspx")) != null && dashboard.Exists)
            {
                this.WriteDebug("GovDevMossUIReceiver.UpdateBurndownDashboard method - Burndown Dashboard exists to be updated.");
                using (SPLimitedWebPartManager manager = dashboard.GetLimitedWebPartManager(PersonalizationScope.Shared))
                {
                    foreach (System.Web.UI.WebControls.WebParts.WebPart part in manager.WebParts)
                    {
                        using (part)
                        {
                            if (part.Title == "User Story Progress (count)")
                            {
                                ExcelWebRenderer excelWebPart = part as ExcelWebRenderer;
                                if (excelWebPart != null)
                                {
                                    this.WriteDebug("GovDevMossUIReceiver.UpdateBurndownDashboard method - Found 'User Story Progress' Excel WebPart.");
                                    excelWebPart.Title = "Use Case Progress (count)";
                                    string workBookUri = excelWebPart.WorkbookUri;
                                    this.WriteDebug(string.Format("GovDevMossUIReceiver.UpdateBurndownDashboard method - Original Workbook URI: {0}.", workBookUri));
                                    workBookUri = workBookUri.Replace("User Story Progress.xlsx", "Use Case Progress.xlsx");
                                    this.WriteDebug(string.Format("GovDevMossUIReceiver.UpdateBurndownDashboard method - Updated Workbook URI: {0}.", workBookUri));
                                    excelWebPart.WorkbookUri = workBookUri;
                                    this.WriteDebug(string.Format("GovDevMossUIReceiver.UpdateBurndownDashboard method - Set Workbook URI: {0}.", excelWebPart.WorkbookUri));
                                    manager.SaveChanges(part);
                                    this.WriteDebug("Returning from GovDevMossUIReceiver.UpdateBurndownDashboard method after successflly modifying Excel WebPart in BurnDown Dashboard");
                                    return;
                                }
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Updates the Test Dashboard in the MOSS site
        /// </summary>
        private void UpdateTestDashboard()
        {
            this.WriteDebug("Entering GovDevMossUIReceiver.UpdateTestDashboard method.");
            SPFile dashboard = null;

            if ((dashboard = Web.GetFile("Dashboards/Test.aspx")) != null && dashboard.Exists)
            {
                this.WriteDebug("GovDevMossUIReceiver.UpdateTestDashboard method - Burndown Dashboard exists to be updated.");
                using (SPLimitedWebPartManager manager = dashboard.GetLimitedWebPartManager(PersonalizationScope.Shared))
                {
                    foreach (System.Web.UI.WebControls.WebParts.WebPart part in manager.WebParts)
                    {
                        using (part)
                        {
                            if (part.Title == "User Story Test Status")
                            {
                                ExcelWebRenderer excelWebPart = part as ExcelWebRenderer;
                                if (excelWebPart != null)
                                {
                                    this.WriteDebug("GovDevMossUIReceiver.UpdateTestDashboard method - Found 'User Story Test Status' Excel WebPart.");
                                    excelWebPart.Title = "Use Case Test Status";
                                    string workBookUri = excelWebPart.WorkbookUri;
                                    this.WriteDebug(string.Format("GovDevMossUIReceiver.UpdateTestDashboard method - Original Workbook URI: {0}.", workBookUri));
                                    workBookUri = workBookUri.Replace("User Story Test Status.xlsx", "Use Case Test Status.xlsx");
                                    this.WriteDebug(string.Format("GovDevMossUIReceiver.UpdateTestDashboard method - Updated Workbook URI: {0}.", workBookUri));
                                    excelWebPart.WorkbookUri = workBookUri;
                                    this.WriteDebug(string.Format("GovDevMossUIReceiver.UpdateTestDashboard method - Set Workbook URI: {0}.", excelWebPart.WorkbookUri));
                                    manager.SaveChanges(part);
                                    this.WriteDebug("Returning from GovDevMossUIReceiver.UpdateTestDashboard method after successflly modifying Excel WebPart in Test Dashboard");
                                    return;
                                }
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Updates the Project WorkItems WebPart to appropriate query
        /// </summary>
        private void UpdateMossProjectWorkItems()
        {
            this.WriteDebug("Entering GovDevMossUIReceiver.UpdateMossProjectWorkItems method.");

            foreach (string dashBoardFilename in this.dashboardList)
            {
                SPFile dashboard = null;

                if ((dashboard = Web.GetFile(dashBoardFilename)) != null && dashboard.Exists)
                {
                    this.WriteDebug(string.Format("GovDevMossUIReceiver.UpdateMossProjectWorkItems method - {0} exists to be updated.", dashBoardFilename));
                    using (SPLimitedWebPartManager manager = dashboard.GetLimitedWebPartManager(PersonalizationScope.Shared))
                    {
                        foreach (System.Web.UI.WebControls.WebParts.WebPart part in manager.WebParts)
                        {
                            using (part)
                            {
                                if (part.Title == "Project Work Items")
                                {
                                    WorkItemSummaryWebPart workItemSummaryWebPart = part as WorkItemSummaryWebPart;
                                    if (workItemSummaryWebPart != null)
                                    {
                                        this.WriteDebug(string.Format("GovDevMossUIReceiver.UpdateMOssProjectWorkItems method - Found 'Project Work Items' WebPart on {0}.", dashBoardFilename));
                                        workItemSummaryWebPart.Query = "SELECT [System.Id], [System.Title] FROM WorkItems WHERE ([System.TeamProject] = @project AND ([System.WorkItemType] = 'Bug' OR [System.WorkItemType] = 'Task' OR [System.WorkItemType] = 'Test Case' OR [System.WorkItemType] = 'Use Case' OR [System.WorkItemType] = 'Requirement'  OR [System.WorkItemType] = 'Change Request' OR [System.WorkItemType] = 'Support Ticket')) ORDER BY [System.Id]";
                                        manager.SaveChanges(part);
                                        this.WriteDebug(string.Format("GovDevMossUIReceiver.UpdateMossProjectWorkItems method - Returning after updating 'Project Work Items' WebPart on {0}.", dashBoardFilename));
                                        break;
                                    }
                                }
                            }
                        }
                    }
                }
            }

            this.WriteDebug("Leaving GovDevMossUIReceiver.UpdateMossProjectWorkItems method.");
        }
    }
}