//-----------------------------------------------------------------------
// <copyright file="GovDevReportsReceiver.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Dpe.Ps.Govdev
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;
    using System.Web.UI.WebControls.WebParts;
    using Microsoft.SharePoint;
    using Microsoft.SharePoint.WebPartPages;
    using Microsoft.TeamFoundation.WebAccess.WebParts;

    /// <summary>
    /// Feaure Reciver class for deploying Excel Reports as part of the
    /// GovDev for TFS Process Template on WSS
    /// </summary>
    [CLSCompliant(false)]
    [Guid("b361868f-68df-42f9-ac40-b1366d77b1bf")]
    public class GovDevReportsReceiver : BaseFeatureReceiver
    {
        /// <summary>
        /// List of Dashoard Pages in MOSS
        /// </summary>
        private List<string> dashboardList = new List<string>() { "Dashboards/MyDashboard.aspx", "Dashboards/ProjectDashboard_wss.aspx" };

        /// <summary>
        /// Override implementation to handle feature activation
        /// </summary>
        protected override void OnActivate()
        {
            this.WriteDebug("Entering GovDevReportsReceiver.OnActivate method.");
            this.DeleteFile("Reports/User Story Progress.xlsx");
            this.DeleteFile("Reports/User Story Test Status.xlsx");
            this.UpdateProjectDashboard();
            this.UpdateWssProjectWorkItems();
            this.MarkSiteForTimerJobProcessing();
            this.WriteDebug("Leaving GovDevReportsReceiver.OnActivate method.");
        }

        /// <summary>
        /// Updates the Project Dashboard in the WSS site
        /// </summary>
        private void UpdateProjectDashboard()
        {
            this.WriteDebug("Entering GovDevReportsReceiver.UpdateDashboard method.");
            SPFile dashboard = null;

            if ((dashboard = Web.GetFile("Dashboards/ProjectDashboard_wss.aspx")) != null)
            {
                if (dashboard.Exists)
                { 
                    this.WriteDebug("GovDevReportsReceiver.UpdateDashboard method - ProjectDashboard_wss exists to be updated.");
                    using (SPLimitedWebPartManager manager = dashboard.GetLimitedWebPartManager(PersonalizationScope.Shared))
                    {
                        foreach (System.Web.UI.WebControls.WebParts.WebPart part in manager.WebParts)
                        {
                            using (part)
                            {
                                if (part.Title == "Product Backlog")
                                {
                                    QueryResultsWebPart queryWebPart = part as QueryResultsWebPart;
                                    if (queryWebPart != null)
                                    {
                                        this.WriteDebug("GovDevReportsReceiver.UpdateWssDashboard method - Found 'Product Backlog' QueryResult WebPart.");
                                        queryWebPart.ServerQuery = true;
                                        queryWebPart.Query = "Team Queries/Use Case Planning";
                                        this.WriteDebug("GovDevReportsReceiver.UpdateProjectDashboard method - Updating webpart query to Server Query 'Team Queries/Use Case Planning'.");
                                        queryWebPart.Title = "Use Case Backlog";
                                        this.WriteDebug("GovDevReportsReceiver.UpdateProjectDashboard method - Updating webpart title to 'Use Case Backlog'.");
                                        manager.SaveChanges(part);
                                        this.WriteDebug("GovDevReportsReceiver.UpdateProjectDashboard method - Returning after successflly modifying Excel WebPart in BurnDown Dashboard");
                                        return;
                                    }
                                }
                            }
                        }
                    }
                }
            }

            this.WriteDebug("Leaving GovDevReportsReceiver.UpdateProjectDashboard method.");
        }

        /// <summary>
        /// Updates the Project WorkItems WebPart to appropriate query
        /// </summary>
        private void UpdateWssProjectWorkItems()
        {
            this.WriteDebug("Entering GovDevReportsReceiver.UpdateWssProjectWorkItems method.");

            foreach (string dashBoardFilename in this.dashboardList)
            {
                SPFile dashboard = null;

                if ((dashboard = Web.GetFile(dashBoardFilename)) != null && dashboard.Exists)
                {
                    this.WriteDebug(string.Format("GovDevReportsReceiver.UpdateWssProjectWorkItems method - {0} exists to be updated.", dashBoardFilename));
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
                                        this.WriteDebug(string.Format("GovDevReportsReceiver.UpdateWssProjectWorkItems method - Found 'Project Work Items' WebPart on {0}.", dashBoardFilename));
                                        workItemSummaryWebPart.Query = "SELECT [System.Id], [System.Title] FROM WorkItems WHERE ([System.TeamProject] = @project AND ([System.WorkItemType] = 'Bug' OR [System.WorkItemType] = 'Task' OR [System.WorkItemType] = 'Test Case' OR [System.WorkItemType] = 'Use Case' OR [System.WorkItemType] = 'Requirement'  OR [System.WorkItemType] = 'Change Request' OR [System.WorkItemType] = 'Support Ticket')) ORDER BY [System.Id]";
                                        manager.SaveChanges(part);
                                        this.WriteDebug(string.Format("GovDevReportsReceiver.UpdateWssProjectWorkItems method - Returning after updating 'Project Work Items' WebPart on {0}.", dashBoardFilename));
                                        break;
                                    }
                                }
                            }
                        }
                    }
                }
            }

            this.WriteDebug("Leaving GovDevReportsReceiver.UpdateWssProjectWorkItems method.");
        }
    }
}