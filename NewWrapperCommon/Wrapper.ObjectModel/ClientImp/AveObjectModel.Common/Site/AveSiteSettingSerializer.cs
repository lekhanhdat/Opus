/********************************************************************
 *
 *  PROPRIETARY and CONFIDENTIAL
 *
 *  This file is licensed from, and is a trade secret of:
 *
 *                   AvePoint, Inc.
 *                   525 Washington Blvd, Suite 1400
 *                   Jersey City, NJ 07310
 *                   United States of America
 *                   Telephone: +1-201-793-1111
 *                   WWW: www.avepoint.com
 *
 *  Refer to your License Agreement for restrictions on use,
 *  duplication, or disclosure.
 *
 *  RESTRICTED RIGHTS LEGEND
 *
 *  Use, duplication, or disclosure by the Government is
 *  subject to restrictions as set forth in subdivision
 *  (c)(1)(ii) of the Rights in Technical Data and Computer
 *  Software clause at DFARS 252.227-7013 (Oct. 1988) and
 *  FAR 52.227-19 (C) (June 1987).
 *
 *  Copyright © 2017-2026 AvePoint® Inc. All Rights Reserved. 
 *
 *  Unpublished - All rights reserved under the copyright laws of the United States.
 */



namespace AvePoint.ObjectModel.Common
{
    #region using directives
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using AvePoint.Wrapper.Common;
    #endregion

    internal class AveSiteSettingSerializer : IAveSiteSettingSerializer
    {
        private AveSite m_Site;

        public AveSiteSettingSerializer(AveSite site)
        {
            m_Site = site;
        }

        public AveSiteSettingInfo GetObjectData()
        {
            AveSiteSettingInfo siteSettingInfo = new AveSiteSettingInfo();

            //siteSettingInfo.OwnerID = m_Site.Owner.ID;
            //siteSettingInfo.SecondaryContactID = m_Site.SecondaryContact.ID;
            if (m_Site.DataCache.IsPropertyAvailable("Id"))
            {
                siteSettingInfo.Id = m_Site.ID;
            }
            if (m_Site.DataCache.IsPropertyAvailable("CurrentResourceUsage"))
            {
                siteSettingInfo.CurrentResourceUsage = m_Site.CurrentResourceUsage;
            }
            if (m_Site.DataCache.IsPropertyAvailable("AverageResourceUsage"))
            {
                siteSettingInfo.AverageResourceUsage = m_Site.AverageResourceUsage;
            }
            if (m_Site.RootWeb != null)
            {
                siteSettingInfo.RootWebId = m_Site.RootWeb.ID;
            }

            if (m_Site.Solutions != null)
            {
                if (m_Site.Solutions.Count > 0 && siteSettingInfo.SolutionIdCollection == null)
                {
                    siteSettingInfo.SolutionIdCollection = new AveRestorableProperty<List<Guid>>(new List<Guid>());
                }
                foreach (IAveUserSolution solution in m_Site.Solutions)
                {
                    siteSettingInfo.SolutionIdCollection.Value.Add(solution.SolutionId);
                }
            }
            //if (m_Site.DataCache.IsPropertyAvailable("SyndicationEnabled"))
            //{
                siteSettingInfo.SyndicationEnabled = m_Site.SyndicationEnabled;
            //}
            if (m_Site.Audit != null)
            {
                siteSettingInfo.AuditFlags = (int)m_Site.Audit.AuditFlags;
                siteSettingInfo.UseAuditFlagCache = m_Site.Audit.UseAuditFlagCache;
                siteSettingInfo.AuditLogTrimmingRetention = m_Site.AuditLogTrimmingRetention;
                siteSettingInfo.TrimAuditLog = m_Site.TrimAuditLog;

            }
            if (m_Site.DataCache.IsPropertyAvailable("AuditLogTrimmingCallout"))
            {
                siteSettingInfo.AuditLogTrimmingCallout = m_Site.AuditLogTrimmingCallout;
            }
            //if (m_Site.DataCache.IsPropertyAvailable("PortalUrl"))
            //{
                siteSettingInfo.PortalURL = m_Site.PortalUrl;
            //}
            //if (m_Site.DataCache.IsPropertyAvailable("PortalName"))
            //{
                siteSettingInfo.PortalName = m_Site.PortalName;
            //}
            //Sharepoint designer settings.
            if (m_Site.DataCache.IsPropertyAvailable("AllowDesigner"))
            {
                siteSettingInfo.AllowDesigner = m_Site.AllowDesigner;
            }
            if (m_Site.DataCache.IsPropertyAvailable("AllowMasterPageEditing"))
            {
                siteSettingInfo.AllowMasterPageEditing = m_Site.AllowMasterPageEditing;
            }
            if (m_Site.DataCache.IsPropertyAvailable("AllowRevertFromTemplate"))
            {
                siteSettingInfo.AllowRevertFromTemplate = m_Site.AllowRevertFromTemplate;
            }
            if (m_Site.DataCache.IsPropertyAvailable("ShowURLStructure"))
            {
                siteSettingInfo.ShowURLStructure = m_Site.ShowURLStructure;
            }
            if (m_Site.DataCache.IsPropertyAvailable("UIVersionConfigurationEnabled"))
            {
                siteSettingInfo.UiversionConfigurationEnable = m_Site.UIVersionConfigurationEnabled;
            }
            if (m_Site.DataCache.IsPropertyAvailable("ShareByEmailEnabled"))
            {
                siteSettingInfo.ShareByEmailEnabled = m_Site.ShareByEmailEnabled;
            }

            return siteSettingInfo;
        }

        public object SetObjectData(object obj)
        {
            throw new NotImplementedException();
        }
    }
}
