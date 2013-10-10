//-----------------------------------------------------------------------
// <copyright file="Activation.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Dpe.Ps.Govdev
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using Microsoft.SharePoint;
    using Microsoft.SharePoint.Administration;
    using Microsoft.TeamFoundation.SharePoint;
    using Microsoft.TeamFoundation.SharePoint.Dashboards.Common;

    /// <summary>
    /// The class used to manage feature activation in the GovDev for TFS process template.
    /// </summary>
    internal class Activation
    {
        /// <summary>
        /// The desired activation state for the site.
        /// </summary>
        private static Dictionary<Guid, ActivationState> targetState;

        /// <summary>
        /// The set of featrues available for activation.
        /// </summary>
        private ICollection<Feature> features;

        /// <summary>
        /// Initializes static members of the Activation class.
        /// </summary>
        static Activation()
        {
            Activation.targetState = new Dictionary<Guid, ActivationState>();
        }

        /// <summary>
        /// Initializes a new instance of the Activation class.
        /// </summary>
        public Activation()
        {
            Activation.WriteDebug("Entering Activation constructor.");
            List<Feature> features = new List<Feature>();
            features.Add(new Feature(new Guid("b361868f-68df-42f9-ac40-b1366d77b1bf"), "TfsDashboardGovDevReports", DashboardLevel.WssWarehouse, false, false));
            features.Add(new Feature(new Guid("9993ad09-5b76-40eb-b010-1c84f9f03eda"), "TfsDashboardGovDevPages", DashboardLevel.WssWarehouse, false, false));
            features.Add(new Feature(new Guid("f8edf4c0-efb5-4be5-b31f-03a52396e171"), "TfsDashboardGovDevMossUI", DashboardLevel.Moss, false, false));
            this.features = features;
            Activation.WriteDebug("Leaving Activation constructor.");
        }

        /// <summary>
        /// Determines the Reporting Services capabilities for the site. 
        /// </summary>
        /// <param name="web">The SharePoint site the features are to be activated on.</param>
        /// <returns>The level of suppport the site has for reporting.</returns>
        public static DashboardLevel GetCapabilityLevel(SPWeb web)
        {
            Activation.WriteDebug("Entering Activation.GetCapabilityLevel static method.");
            DashboardLevel dashboardLevel;

            try
            {
                ProjectInfo projectInfo = TeamFoundationWeb.GetProject(web);
                if (projectInfo.ReportingConfiguration.Cube != null)
                {
                    if (!MeetsDashboardPrerequisites(web))
                    {
                        dashboardLevel = DashboardLevel.WssWarehouse;
                        Activation.WriteDebug("Activation.GetCapabilityLevel method - Dashboard level is WssWarehouse.");
                    }
                    else
                    {
                        dashboardLevel = DashboardLevel.Moss;
                        Activation.WriteDebug("Activation.GetCapabilityLevel method - Dashboard level is Moss.");
                    }
                }
                else
                {
                    dashboardLevel = DashboardLevel.NoWarehouse;
                    Activation.WriteDebug("Activation.GetCapabilityLevel method - Dashboard level is NoWarehouse.");
                }
            }
            catch (Exception exp)
            {
                // TODO: Logging
                Activation.WriteDebug(string.Format("GetCapabilities - EXCEPTION CAUGHT: {0}", exp.Message));
                return DashboardLevel.NoWarehouse;
            }

            Activation.WriteDebug("Leaving Activation.GetCapabilityLevel static method by returning Dashboard Level.");
            return dashboardLevel;
        }

        /// <summary>
        /// Determines if the site has the required feature that supports Excel Services Dashboards.
        /// </summary>
        /// <param name="web">The SharePoint site the features are to be activated on.</param>
        /// <returns>Whether or not the site has the required feature that supports Excel Services Dashboards.</returns>
        public static bool MeetsDashboardPrerequisites(SPWeb web)
        {
            Activation.WriteDebug("Entering Acivation.MeetsDashboardPrerequisites method.");
            SPFeature feature = web.Site.WebApplication.Features[new Guid("0ea1c3b6-6ac0-44aa-9f3f-05e8dbe6d70b")];
            if (feature == null)
            {
                // Did not find Excel Features
                Activation.WriteDebug("Returning FALSE from Acivation.MeetsDashboardPrerequisites method.");
                return false;
            }
            else
            {
                // Found Excel Features
                Activation.WriteDebug("Returning TRUE from Acivation.MeetsDashboardPrerequisites method.");
                return true;
            }
        }

        /// <summary>
        /// Entry method for activating features on the site.
        /// </summary>
        /// <param name="web">The SharePoint site the features are to be activated on.</param>
        /// <param name="targetState">The desired activation state of the site.</param>
        /// <param name="initiatingFeature">The GUID of the top level featre that activates the other features.</param>
        public void Activate(SPWeb web, ActivationState targetState, Guid initiatingFeature)
        {
            Activation.WriteDebug("Entering Acivation.Activate method.");
            Activation.WriteDebug(string.Format("Acivation.Activate method - Web ID:{0} Web Name:{1} Web URL: {2}.", web.ID, web.Name, web.Url));
            Activation.WriteDebug(string.Format("Acivation.Activate method - Target State: {0}", targetState.ToString()));
            Activation.WriteDebug(string.Format("Acivation.Activate method - Initiating Feature Guid: {0}", initiatingFeature));

            Activation.WriteDebug("Activation.Activate method - targetsate.level > activationState.level.  Proceeding to get feature activation sequence list.");
            IList<Feature> featureActivationSequence = this.GetFeatureActivationSequence(DashboardLevel.NoDashboards, targetState.level, false);
            Activation.WriteDebug("Activation.Activate method - Have feature activation sequence list.");

            try
            {
                this.SaveTargetState(web, targetState);
                Activation.WriteDebug("Activation.Activate method - Saved Target State.");
                
                SPFarm local = SPFarm.Local;
                SPFeatureDefinitionCollection featureDefinitions = local.FeatureDefinitions;
                Activation.WriteDebug("Activation.Activate method - Obtained SPFarm feature definition collection.");

                Activation.WriteDebug("Activation.Activate method - Begin Iterating through SPFarm feature definition collection.");
                foreach (Feature feature in featureActivationSequence)
                {
                    Guid guid = feature.Guid;
                    if (guid.Equals(initiatingFeature))
                    {
                        Activation.WriteDebug("Activation.Activate method - Feature is initiating feature - moving on.");
                        continue;
                    }

                    Activation.WriteDebug("Activation.Activate method - Feature is not initiating feature - will atempt to activate.");
                    SPFeatureDefinition featureDefinition = featureDefinitions[feature.Guid];
                    Activation.WriteDebug("Activation.Activate method - Obtained Feature Definition to use in activation.");
                    if (featureDefinition != null)
                    {
                        this.ActivateFeature(web, featureDefinition);
                        Activation.WriteDebug(string.Format("Activation.Activate method - Activated Feature: {0} - {1}.", feature.Guid, feature.Name));
                    }
                    else
                    {
                        Activation.WriteDebug(string.Format("Activation.Activate method - Throwing unrecoverable exception because SPFeatureDefinition is null: {0} - {1}.", feature.Guid, feature.Name));
                        throw new SPException(string.Format("Featured Not Activated: {0} - {1}", feature.Name, feature.Guid));
                    }
                }

                Activation.WriteDebug("Activation.Activate method - Complete Iterating through SPFarm feature definition collection.");
            }
            finally
            {
                Activation.WriteDebug("Activation.Activate method - Finished activating features.  Cleaning up TargetState.");
                this.RemoveTargetState(web);
            }
        }

        /// <summary>
        /// Gets the order list of features to be activated by comparing the current Dashboard level to the required level.
        /// </summary>
        /// <param name="current">The Dashboard level to start for filtering features.</param>
        /// <param name="required">The Dashboard level of the required deployment site.</param>
        /// <param name="newSite">Whether or not the site is new or not.</param>
        /// <returns>A list of features to be activated in the order presented.</returns>
        internal IList<Feature> GetFeatureActivationSequence(DashboardLevel current, DashboardLevel required, bool newSite)
        {
            Activation.WriteDebug("Entering Activation.GetFeatureActivationSequence method.");

            IList<Feature> features = new List<Feature>();
            if (current < required)
            {
                do
                {
                    current++;
                    this.AddFeatures(features, current);
                    if (current == required)
                    {
                        this.AddFeatures(features, current);
                    }

                    if (!newSite)
                    {
                        continue;
                    }

                    this.AddFeatures(features, current);
                }
                while (current < required);

                Activation.WriteDebug(string.Format("Activation.GetFeatureActivationSequence method - Return with feature list containing {0} features for activation.", features.Count));
                return features;
            }
            else
            {
                Activation.WriteDebug(string.Format("Activation.GetFeatureActivationSequence method - Return with feature list containing {0} features for activation.", features.Count));
                return features;
            }
        }

        /// <summary>
        /// Loads the current activation state of the site.
        /// </summary>
        /// <param name="web">The SharePoint site the features are to be activated on.</param>
        /// <returns>The current activation state for the site.</returns>
        internal ActivationState LoadActivationState(SPWeb web)
        {
            Activation.WriteDebug("Entering Activation.LoadActivationState method.");
            ActivationState noDashboards;

            string activationState = web.Properties["teamfoundation.dashboards.ActivationState"];
            if (!Activation.targetState.ContainsKey(web.ID))
            {
                Activation.WriteDebug("Activation.LoadActivationState:  The target state does not contain the web ID key.");
                if (string.IsNullOrEmpty(activationState))
                {
                    noDashboards = ActivationState.NoDashboards;
                    Activation.WriteDebug(string.Format("Activation.LoadActivationState:  Set the current activation state to NoDashboards because current state is null or empty: {0}.", noDashboards.ToString()));
                }
                else
                {
                    Activation.WriteDebug(string.Format("Activation.LoadActivationState:  The current activation state will be set to the current state of the Web: {0}.", ActivationState.FromString(activationState)));
                    noDashboards = ActivationState.FromString(activationState);
                    Activation.WriteDebug(string.Format("Activation.LoadActivationState:  Set the current activation state to teamfoundation.dashboards.ActivationState: {0}.", noDashboards.ToString()));
                }
            }
            else
            {
                Activation.WriteDebug(string.Format("Activation.LoadActivationState:  The current activation state will be set from the targetstate member: {0}.", Activation.targetState[web.ID]));
                noDashboards = Activation.targetState[web.ID];
            }

            noDashboards.hasUI = web.Features[new Guid("A4EB8C3B-AA25-4093-8330-39CDBB51B07C")] != null;
            Activation.WriteDebug(string.Format("Activation.LoadActivationState:  Set the hasUI property of the activation state: {0}.", noDashboards.hasUI));
            Activation.WriteDebug(string.Format("Returning from Activation.LoadActivationState method - Activation state: {0}.", noDashboards.ToString()));

            return noDashboards;
        }

        /// <summary>
        /// Clears the desired activation state.
        /// </summary>
        /// <param name="web">The SharePoint site the features are to be activated on.</param>
        internal void RemoveTargetState(SPWeb web)
        {
            Activation.WriteDebug("Entering Activation.RemoveTargetSate method.");
            if (Activation.targetState.ContainsKey(web.ID))
            {
                lock (Activation.targetState)
                {
                    if (Activation.targetState.ContainsKey(web.ID))
                    {
                        Activation.targetState.Remove(web.ID);
                        Activation.WriteDebug(string.Format("Activation.RemoveTargetState - Removed actiavtion state for Web: {0} {1}.", web.ID, web.Name));
                    }
                }
            }

            Activation.WriteDebug("Leaving Activation.RemoveTargetState method.");
        }

        /// <summary>
        /// Saves the desired activation state. 
        /// </summary>
        /// <param name="web">The SharePoint site the features are to be activated on.</param>
        /// <param name="state">The acitvation state to be saved.</param>
        internal void SaveTargetState(SPWeb web, ActivationState state)
        {
            Activation.WriteDebug("Entering Activation.SaveTargetState method.");
            if (web != null)
            {
                lock (Activation.targetState)
                {
                    Activation.targetState[web.ID] = state;
                }

                Activation.WriteDebug("Leaving Activation.SaveTargetState method.");
                return;
            }
            else
            {
                Activation.WriteDebug("Activation.SaveTargetState method - throwing unrecoverable exception in web.");
                throw new ArgumentNullException("web");
            }
        }

        /// <summary>
        /// Allows for writing Debug statements
        /// </summary>
        /// <param name="statement">The string to output.</param>
        [Conditional("DEBUG")]
        protected static void WriteDebug(string statement)
        {
            Debug.WriteLine(statement);
        }

        /// <summary>
        /// Activates the specific feature.
        /// </summary>
        /// <param name="web">The SharePoint site the feature is to be activated on.</param>
        /// <param name="feature">The SharePoint Feature Defintion that describes the feature to be activated.</param>
        private void ActivateFeature(SPWeb web, SPFeatureDefinition feature)
        {
            Activation.WriteDebug("Entering Activation.ActivateFeature method.");

            SPSecurity.CodeToRunElevated codeToRunElevated = null;
            SPSecurity.CodeToRunElevated codeToRunElevated1 = null;
            SPSecurity.CodeToRunElevated codeToRunElevated2 = null;

            if (web != null)
            {
                if (feature != null)
                {
                    Activation.WriteDebug("Activation.ActivateFeature method - Entering switch.");

                    SPFeatureScope scope = feature.Scope;
                    switch (scope)
                    {
                        case SPFeatureScope.ScopeInvalid:
                        {
                            Activation.WriteDebug("Returning from Activation.ActivateFeature method - Switch is ScopeInvalid scope.");
                            return;
                        }

                        case SPFeatureScope.Farm:
                        {
                            if (web.Site.WebApplication.WebService.Features[feature.Id] != null)
                            {
                                Activation.WriteDebug("Returning from Activation.ActivateFeature method - Feature is already activated at Farm scope.");
                                return;
                            }

                            // TODO: Logging
                            if (codeToRunElevated == null)
                            {
                                codeToRunElevated = () =>
                                {
                                    using (SPSite sPSite = new SPSite(web.Site.ID))
                                    {
                                        SPWebService webService = sPSite.WebApplication.WebService;
                                        webService.Features.Add(feature.Id, true);
                                        Activation.WriteDebug(string.Format("Activation.ActivateFeature method for Farm scope - Activated feature: {0}.", feature.Name));
                                    }
                                };
                            }

                            SPSecurity.RunWithElevatedPrivileges(codeToRunElevated);
                            Activation.WriteDebug("Returning from Activation.ActivateFeature method - Switch is Farm scope.");
                            return;
                        }

                        case SPFeatureScope.WebApplication:
                        {
                            if (web.Site.WebApplication.Features[feature.Id] != null)
                            {
                                Activation.WriteDebug("Returning from Activation.ActivateFeature method - Feature is already activated at WebApplication scope.");
                                return;
                            }

                            // TODO: Logging
                            if (codeToRunElevated1 == null)
                            {
                                codeToRunElevated1 = () =>
                                {
                                    using (SPSite sPSite = new SPSite(web.Site.ID))
                                    {
                                        SPWebApplication webApplication = sPSite.WebApplication;
                                        webApplication.Features.Add(feature.Id, true);
                                        Activation.WriteDebug(string.Format("Activation.ActivateFeature method for WebApplication scope - Activated feature: {0}.", feature.Name));
                                    }
                                };
                            }

                            SPSecurity.RunWithElevatedPrivileges(codeToRunElevated1);
                            Activation.WriteDebug("Returning from Activation.ActivateFeature method - Switch is WebApplication scope.");
                            return;
                        }

                        case SPFeatureScope.Site:
                        {
                            if (web.Site.Features[feature.Id] != null)
                            {
                                Activation.WriteDebug("Returning from Activation.ActivateFeature method - Feature is already activated at Site scope.");
                                return;
                            }

                            // TODO: Logging
                            if (codeToRunElevated2 == null)
                            {
                                codeToRunElevated2 = () =>
                                {
                                    using (SPSite sPSite = new SPSite(web.Site.ID))
                                    {
                                        sPSite.Features.Add(feature.Id, true);
                                        Activation.WriteDebug(string.Format("Activation.ActivateFeature method for Site scope - Activated feature: {0}.", feature.Name));
                                    }
                                };
                            }

                            SPSecurity.RunWithElevatedPrivileges(codeToRunElevated2);
                            Activation.WriteDebug("Returning from Activation.ActivateFeature method - Switch is Site scope.");
                            return;
                        }

                        case SPFeatureScope.Web:
                        {
                            if (web.Features[feature.Id] != null)
                            {
                                Activation.WriteDebug("Returning from Activation.ActivateFeature method - Feature is already activated at Web scope.");
                                return;
                            }

                            // TODO: Logging
                            web.Features.Add(feature.Id);
                            Activation.WriteDebug(string.Format("Returning from Activation.ActivateFeature method for Site scope - Activated feature: {0}.", feature.Name));
                            return;
                        }

                        default:
                        {
                            Activation.WriteDebug("Returning from Activation.ActivateFeature method - Switch default!!!!");
                            return;
                        }
                    }
                }
                else
                {
                    Activation.WriteDebug("Activation.ActivateFeature method - throwing unrecoverable exception in feature.");
                    throw new ArgumentNullException("feature");
                }
            }
            else
            {
                Activation.WriteDebug("Activation.ActivateFeature method - throwing unrecoverable exception in web.");
                throw new ArgumentNullException("web");
            }
        }

        /// <summary>
        /// Determines the list of features to be added for activation
        /// </summary>
        /// <param name="guids">The list of feature GUIDs to be considered for activation.</param>
        /// <param name="required">The Dashboard level of the required deployment site.</param>
        private void AddFeatures(IList<Feature> guids, DashboardLevel required)
        {
            Activation.WriteDebug("Entering Activation.AddFeatures method.");

            foreach (Feature feature in this.features)
            {
                if (feature.Level != required)
                {
                    Activation.WriteDebug(string.Format("Activation.AddFeatures method - Not adding feature: {0}.", feature.Name));
                    continue;
                }

                guids.Add(feature);
                Activation.WriteDebug(string.Format("Activation.AddFeatures method - Added to features list: {0}.", feature.Name));
            }

            Activation.WriteDebug(string.Format("Returning from Activation.AddFeatures method - Features added count: {0}.", guids.Count));
        }

        /// <summary>
        /// Data holder for the properties of the Features that will be deployed.
        /// </summary>
        internal class Feature
        {
            /// <summary>
            /// Initializes a new instance of the Feature class.  
            /// This acts as a data holder for feature and deploymentinformation.
            /// </summary>
            /// <param name="guid">The SharePoint GUID for the Feature</param>
            /// <param name="name">The name of the Feature</param>
            /// <param name="level">The Dashboard deployment level of the feature</param>
            /// <param name="newSite">True or false that this Feature goes to a new site</param>
            /// <param name="optional">True or false that this feature is optional</param>
            public Feature(Guid guid, string name, DashboardLevel level, bool newSite, bool optional)
            {
                Activation.WriteDebug("Entering Acivation.Feature constructor.");
                this.Guid = guid;
                this.Name = name;
                this.Level = level;
                this.NewSite = newSite;
                this.Optional = optional;
                Activation.WriteDebug("Leaving Acivation.Feature constructor.");
            }

            /// <summary>
            /// Gets or sets SharePoint GUID for the Feature
            /// </summary>
            public Guid Guid
            {
                get;
                set;
            }

            /// <summary>
            /// Gets or sets the name of the Feature
            /// </summary>
            public string Name
            {
                get;
                set;
            }

            /// <summary>
            /// Gets or sets the Dashboard deployment level of the feature
            /// </summary>
            public DashboardLevel Level
            {
                get;
                set;
            }

            /// <summary>
            /// Gets or sets a value indicating whether this Feature goes to a new site
            /// </summary>
            public bool NewSite
            {
                get;
                set;
            }

            /// <summary>
            /// Gets or sets a value indicating whether this feature is optional
            /// </summary>
            public bool Optional
            {
                get;
                set;
            }
        }
    }
}