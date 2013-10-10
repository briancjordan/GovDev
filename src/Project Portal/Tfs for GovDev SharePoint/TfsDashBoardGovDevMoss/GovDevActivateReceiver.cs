//-----------------------------------------------------------------------
// <copyright file="GovDevActivateReceiver.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Dpe.Ps.Govdev
{
    using System;
    using System.Runtime.InteropServices;
    using System.Web;
    using Microsoft.SharePoint;
    using Microsoft.TeamFoundation.SharePoint;
    using Microsoft.TeamFoundation.SharePoint.Dashboards.Common;

    /// <summary>
    /// Top level Feaure Reciver class for deploying all SharePoint customizations
    /// as part of the GovDev for TFS Process Template
    /// </summary>
    [CLSCompliant(false)]
    [Guid("2f1da286-e15f-4b99-9e83-afd81680e20c")]
    public class GovDevActivateReceiver : BaseFeatureReceiver
    {
        /// <summary>
        /// Override implementation to handle feature activation
        /// </summary>
        protected override void OnActivate()
        {
            this.WriteDebug("Entering GovDevActivateReceiver.OnActivate method");

            try
            {
                SPWeb parent = this.Web;

                if (parent != null)
                {
                    this.WriteDebug("GovDevActivateReceiver.OnActivate method - Parent is not null.");

                    if (HttpContext.Current != null)
                    {
                        this.WriteDebug("GovDevActivateReceiver.OnActivate method - Current context is not null.");

                        int num = 0;
                        SPFeatureProperty timeoutProperty = this.Feature.Properties["ActivationTimeout"];
                        if (timeoutProperty != null)
                        {
                            this.WriteDebug("GovDevActivateReceiver.OnActivate method - timeoutProperty is not null.");

                            int.TryParse(timeoutProperty.Value.ToString(), out num);

                            HttpServerUtility server = HttpContext.Current.Server;
                            if (server.ScriptTimeout < num)
                            {
                                this.WriteDebug("GovDevActivateReceiver.OnActivate method - Server script timeout is less than desired.");
                                server.ScriptTimeout = num;
                                this.WriteDebug(string.Format("GovDevActivateReceiver method - set server script timeout to {0}.", num));
                            }
                        }
                    }

                    if (this.IsSiteConnectedToProject())
                    {
                        this.WriteDebug("GovDevActivateReceiver.OnActivate method - Site is connect to project.");

                        Activation activation = new Activation();
                        ActivationState activationState = activation.LoadActivationState(parent);
                        DashboardLevel dashboardLevel = this.GetDashboardLevel(this.Properties.Definition.Properties);
                        DashboardLevel capabilityLevel = Activation.GetCapabilityLevel(parent);
                        bool failActivation = false;

                        this.WriteDebug("GovDevActivateReceiver.OnActivate method - Successfully created Activation object and obtained activation state, dashboard level and capability level.");

                        if (dashboardLevel > capabilityLevel)
                        {
                            this.WriteDebug("GovDevActivateReceiver.OnActivate method - Dashboard level required is greater than current Capability level.");
                            dashboardLevel = capabilityLevel;
                            failActivation = true;
                            this.WriteDebug("GovDevActivateReceiver.OnActivate method - Dashboard level reduced to current Capability level.");
                        }

                        if (dashboardLevel >= activationState.level)
                        {
                            activationState.level = dashboardLevel;
                            this.WriteDebug("GovDevActivateReceiver.OnActivate method - Set activationState.level to Dashboard level.");
                            this.WriteDebug("GovDevActivateReceiver.OnActivate method - Calling Activation.Activate method.");
                            activation.Activate(parent, activationState, this.Feature.DefinitionId);
                        }

                        if (failActivation)
                        {
                            this.WriteDebug("GovDevActivateReceiver.OnActivate method - Failing activation due to the current site capability level.");
                            this.FailFeatureActivation();
                        }
                    }
                    else
                    {
                        this.WriteDebug("GovDevActivateReceiver.OnActivate method - Throwing unrecoverable exeception due to no team project association for site collection.");
                        throw new SPException("This Feature Not Activated - No Team Project");
                    }
                }
            }
            catch (Exception exp)
            {
                // Log exception
                this.WriteDebug(string.Format("GovDevActivateReceiver.OnActivate method - Throwing unrecoverable exeception due to Feature Activation or other exception: {0}.", exp.StackTrace));
                this.FailFeatureActivation();
                throw;
            }
        }
        
        /// <summary>
        /// Cleans up the SPWeb (site collection) on feature activation failure
        /// </summary>
        private void FailFeatureActivation()
        {
            this.WriteDebug("Entering GovDevActivateReceiver.FailFeatureActivation method");
            using (SPWeb parent = this.Web)
            {
                Guid id = this.Properties.Definition.Id;

                if (parent != null && parent.Features[id] != null)
                {
                    parent.Features.Remove(id);
                    this.WriteDebug(string.Format("GovDevActivateReceiver.FailFeatureActivation method - Removed Feature: {0}", id));
                }
            }

            this.WriteDebug("Leaving GovDevActivateReceiver.FailFeatureActivation method");
         }

        /// <summary>
        /// Use to determine the level of features we want to install
        /// </summary>
        /// <param name="properties">The SharePoint Feature properties bag</param>
        /// <returns>The level of Dashboard support.</returns>
        private DashboardLevel GetDashboardLevel(SPFeaturePropertyCollection properties)
        {
            this.WriteDebug("Entering GovDevActivateReceiver.GetDashboardLevel method");

            DashboardLevel dashboardLevel = DashboardLevel.NoDashboards;
            SPFeatureProperty featureProperty = properties["DashboardLevel"];
     
            if (featureProperty != null && !string.IsNullOrEmpty(featureProperty.Value))
            {
                this.WriteDebug("GovDevActivateReceiver.GetDashboardLevel method - setting dashboard level in conditional");
                dashboardLevel = (DashboardLevel)Enum.Parse(typeof(DashboardLevel), featureProperty.Value);
                this.WriteDebug("GovDevActivateReceiver.GetDashboardLevel method - set dashboard level");
            }

            this.WriteDebug(string.Format("Returning from GovDevActivateReceiver.GetDashboardLevel method with dashboard level: {0}", dashboardLevel));
            return dashboardLevel;
        }

        /// <summary>
        /// Determines if the SharePoint site is connected to the TFS Team Project
        /// </summary>
        /// <returns>Boolean true or false</returns>
        private bool IsSiteConnectedToProject()
        {
            this.WriteDebug("Entering GovDevActivateReceiver.IsSiteConnectedToProject method");

            bool isConnected = false;

            ProjectInfo projectInfo = TeamFoundationWeb.GetProject(this.Web);
            if (projectInfo != null && projectInfo.ProjectId != Guid.Empty)
            {
                isConnected = true;
            }

            this.WriteDebug(string.Format("GovDevActivateReceiver.IsSiteConnectedToProject method - Returning isConnected: {0}", isConnected));
            return isConnected;
        }
    }
}