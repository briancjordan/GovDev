//-----------------------------------------------------------------------
// <copyright file="GovDevPagesReceiver.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Dpe.Ps.Govdev
{
    using System;
    using System.Runtime.InteropServices;

    /// <summary>
    /// Feaure Reciver class for deploying Dashpoard Pages as part of the
    /// GovDev for TFS Process Template
    /// </summary>
    [CLSCompliant(false)]
    [Guid("9993ad09-5b76-40eb-b010-1c84f9f03eda")]
    public class GovDevPagesReceiver : BaseFeatureReceiver
    {
        /// <summary>
        /// Override implementation to handle feature activation
        /// </summary>
        protected override void OnActivate()
        {
            this.WriteDebug("Entering GovDevPagesReceiver.OnActivate method.");
            this.AddDashboardNavigation(Web.GetFile("Dashboards/Contribution.aspx"));
            this.WriteDebug("GovDevPagesReceiver.OnActivate method - Added dashboard naviagtion for Contribution Dashboard.");
            this.AddDashboardNavigation(Web.GetFile("Dashboards/Traceability.aspx"));
            this.WriteDebug("GovDevPagesReceiver.OnActivate method - Added dashboard naviagtion for Traceability Dashboard.");
            this.WriteDebug("Leaving GovDevPagesReceiver.OnActivate method.");
        }
    }
}