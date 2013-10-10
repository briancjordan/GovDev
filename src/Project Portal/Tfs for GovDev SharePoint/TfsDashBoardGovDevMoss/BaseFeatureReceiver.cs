//-----------------------------------------------------------------------
// <copyright file="BaseFeatureReceiver.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Dpe.Ps.Govdev
{
    using System;
    using System.Diagnostics;
    using Microsoft.SharePoint;
    using Microsoft.SharePoint.Navigation;
    using Microsoft.SharePoint.Utilities;

    /// <summary>
    /// Bsse implmenation of SharePoint Feature Receiver class
    /// </summary>
    [CLSCompliant(false)]
    public class BaseFeatureReceiver : SPFeatureReceiver
    {
        /// <summary>
        /// The fixed GUID for the TfsDashboardBaseContent Feature.
        /// </summary>
        protected const string TfsBaseContentGuid = "4E72D346-276F-47b3-8D10-56E474A4FE4A";

        /// <summary>
        /// Name of the property that stores the default site-relative URL
        /// </summary>
        protected const string DefaultDashboardProperty = "teamfoundation.dashboards.DefaultDashboard";

        /// <summary>
        /// Gets an instance of the parent Site Collection. 
        /// </summary>
        protected SPWeb Web 
        { 
            get; 
            private set; 
        }

        /// <summary>
        /// Gets an instance of the Feature
        /// </summary>
        protected SPFeature Feature 
        { 
            get; 
            private set; 
        }

        /// <summary>
        /// Gets an instance of the Properties collection passed in for feature activation
        /// </summary>
        protected SPFeatureReceiverProperties Properties
        {
            get;
            private set;
        }

        /// <summary>
        /// Occurs after a Feature is activated. 
        /// </summary>
        /// <param name="properties">
        /// A Microsoft.SharePoint.SPFeatureReceiverProperties object that represents properties of the event handler.
        /// </param>
        public override void FeatureActivated(SPFeatureReceiverProperties properties)
        {
            this.WriteDebug("Entering BaseFeatureReceiver.FeatureActivated method.");
            this.Properties = properties;
            this.Feature = properties.Feature;
            this.Web = this.Feature.Parent as SPWeb;
            this.WriteDebug("BaseFeatureReceiver.FeatureActivated method - Calling OnActivate virtual method.");
            this.OnActivate();
            this.WriteDebug("Leaving BaseFeatureReceiver.FeatureActivated method.");
        }

        /// <summary>
        /// Occurs when a Feature is deactivating.
        /// </summary>
        /// <param name="properties">
        /// A Microsoft.SharePoint.SPFeatureReceiverProperties object that represents properties of the event handler.
        /// </param>
        public override void FeatureDeactivating(SPFeatureReceiverProperties properties) 
        { 
        }

        /// <summary>
        /// Occurs after a Feature is installed.
        /// </summary>
        /// <param name="properties">
        /// A Microsoft.SharePoint.SPFeatureReceiverProperties object that represents properties of the event handler.
        /// </param>
        public override void FeatureInstalled(SPFeatureReceiverProperties properties) 
        { 
        }

        /// <summary>
        /// Occurs when a Feature is uninstalling.
        /// </summary>
        /// <param name="properties">
        /// A Microsoft.SharePoint.SPFeatureReceiverProperties object that represents properties of the event handler.
        /// </param>
        public override void FeatureUninstalling(SPFeatureReceiverProperties properties) 
        { 
        }

        /// <summary>
        /// Override point for subclasses to handle activation
        /// </summary>
        protected virtual void OnActivate() 
        { 
        }

        /// <summary>
        /// Moves a file from one URL to another
        /// </summary>
        /// <param name="source">The site relative URL for the file to be moved.</param>
        /// <param name="target">The site relative URL to move the file to</param>
        /// <remarks>Move an existing file to a new URL, overwriting any existing file
        /// at the target location</remarks>
        protected void ReplaceFile(string source, string target)
        {
            SPFile sourceFile = this.Web.GetFile(source);
            if (sourceFile != null && sourceFile.Exists)
            {
                sourceFile.MoveTo(target, true);
            }
        }

        /// <summary>
        /// Deletes a file from the site
        /// </summary>
        /// <param name="target">The site relative URL of the file to delete</param>
        protected void DeleteFile(string target)
        {
            SPFile targetFile = this.Web.GetFile(target);
            if (targetFile != null && targetFile.Exists)
            {
                targetFile.Delete();
            }
        }

        /// <summary>
        /// Mark the site so that the TFS Dashboard timer job will process Excel
        /// workbook connections and pivot table filters
        /// </summary>
        protected void MarkSiteForTimerJobProcessing()
        {
            // Clear the properties cached by the Team Foundation Server
            // Timer Job so it re-processes the site
            SPPropertyBag properties = this.Web.Properties;
            properties["teamfoundation.dashboards.CubeConnectcion"] = null; // spell as shown
            properties["teamfoundation.dashboards.ProjectMdxId"] = null;
            properties["teamfoundation.dashboards.Sso"] = null;
            properties.Update();
        }

        /// <summary>
        /// Add a reference to the dashboard specified by <paramref name="file"/> to 
        /// the Quick Launch under the Dashboards navigation item
        /// </summary>
        /// <param name="file">The file the references the dashboard</param>
        /// <exception cref="ArgumentNullException">If <paramref name="file"/> is
        /// <c>null</c></exception>
        /// <exception cref="ArgumentException">If the <paramref name="file"/> does not
        /// exist</exception>
        protected void AddDashboardNavigation(SPFile file)
        {
            if (file == null)
            {
                throw new ArgumentNullException("file");
            }

            if (!file.Exists)
            {
                throw new ArgumentException("File does not exist. " + file.Url);
            }

            SPNavigationNode node = this.Web.Navigation.GetNodeByUrl(file.Url);
            if (node == null)
            {
                // 1102 is the ID for the Dashboards navigation node
                SPNavigationNode parentNode = this.Web.Navigation.GetNodeById(1102);
                if (parentNode != null)
                {
                    node = new SPNavigationNode(file.Title, file.Url);
                    parentNode.Children.AddAsLast(node);
                }
            }
        }

        /// <summary>
        /// Set the default dashboard to be the <paramref name="file"/> passed in
        /// </summary>
        /// <param name="file">The file that will be the site's default dashboard</param>
        /// <exception cref="ArgumentNullException">If <paramref name="file"/> 
        /// is <c>null</c></exception>
        /// <exception cref="ArgumentException">If the <paramref name="file"/> does not
        /// exist</exception>
        protected void SetDefaultDashboard(SPFile file)
        {
            if (file == null)
            {
                throw new ArgumentNullException("file");
            }

            if (!file.Exists)
            {
                throw new ArgumentException("File does not exist. " + file.Url);
            }

            SPFeature feature = this.Web.Features[new Guid(TfsBaseContentGuid)];
            if (feature == null)
            {
                return;
            }

            SPFeatureProperty property = feature.Properties[DefaultDashboardProperty];
            if (property == null)
            {
                property = new SPFeatureProperty(DefaultDashboardProperty, file.Url);
                feature.Properties.Add(property);
            }
            else
            {
                property.Value = file.Url;
            }

            feature.Properties.Update();
        }

        /// <summary>
        /// Allows for writing Debug statements
        /// </summary>
        /// <param name="statement">The string to output.</param>
        [Conditional("DEBUG")]
        protected void WriteDebug(string statement)
        {
            Debug.WriteLine(statement);
        }
    }
}