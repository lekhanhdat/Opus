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



namespace AvePoint.ObjectModel.Server19
{
    #region using directives
    using System;
    using AvePoint.GCommon;
    using AvePoint.Wrapper.Common;
    using AvePoint.Wrapper.Resource.ServerAPI2010;
    using AvePoint.Wrapper.Resource.ServerAPI2010;
    #endregion

    internal class AveSiteSettingSerializer : IAveSiteSettingSerializer
    {
        private static AveLogger logger = AveLogger.GetInstance(typeof(AveSiteSettingSerializer));
        private AveSite m_Site;
        private IAveBackupRestoreQueryService m_QueryService;

        public AveSiteSettingSerializer(IAveBackupRestoreQueryService queryService, AveSite site)
        {
            m_QueryService = queryService;
            m_Site = site;
        }

        public AveSiteSettingInfo GetObjectData()
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveSiteSettingSerializer.GetObjectData"))
            {

                AveSiteSettingInfo siteSettingInfo = m_QueryService.GetSiteSettingFromSites(m_Site);

                try
                {
                    foreach (IAveUserSolution solution in m_Site.Solutions)
                    {
                        siteSettingInfo.SolutionIdCollection.Value.Add(solution.SolutionId);
                    }
                }
                catch (Exception e)
                {
                    logger.Log(AveLogLevel.DEBUG, ServerAPIResource.SolutionIdAddFailed, e);
                }
                siteSettingInfo.SyndicationEnabled = m_Site.SyndicationEnabled;
                siteSettingInfo.AuditFlags = (int)m_Site.Audit.AuditFlags;
                siteSettingInfo.UseAuditFlagCache = m_Site.Audit.UseAuditFlagCache;
                siteSettingInfo.AuditLogTrimmingRetention = m_Site.AuditLogTrimmingRetention;
                siteSettingInfo.TrimAuditLog = m_Site.TrimAuditLog;
                siteSettingInfo.AuditLogTrimmingCallout = m_Site.AuditLogTrimmingCallout;
                siteSettingInfo.PortalURL = m_Site.PortalUrl;
                siteSettingInfo.PortalName = m_Site.PortalName;
                //Sharepoint designer settings.
                siteSettingInfo.AllowDesigner = m_Site.AllowDesigner;
                siteSettingInfo.AllowMasterPageEditing = m_Site.AllowMasterPageEditing;
                siteSettingInfo.AllowRevertFromTemplate = m_Site.AllowRevertFromTemplate;
                siteSettingInfo.ShowURLStructure = m_Site.ShowURLStructure;
                siteSettingInfo.UiversionConfigurationEnable = m_Site.UIVersionConfigurationEnabled;
                //SharePoint 2013 specific settings
                //siteSettingInfo.AllowExternalEmbedding = m_Site.AllowExternalEmbedding;
                //agent account will be added when we call 'm_Site.AllowExternalEmbedding'
                object mObject = m_Site.RootWeb.AllProperties["__AllowExternalEmbedding"];
                if (mObject != null)
                {
                    siteSettingInfo.AllowExternalEmbedding = (AveScriptSafeExternalEmbedding)mObject;
                }
                else
                {
                    siteSettingInfo.AllowExternalEmbedding = AveScriptSafeExternalEmbedding.AllowedDomains;
                }

                //if the root web current user is not system account,PRItem cannot get ScriptSafeDomains,it will throw exception
                try
                {
                    //To fix ADO-60795
                    siteSettingInfo.ScriptSafeDomains = m_Site.ScriptSafeDomains;
                }
                catch (Exception e)
                {
                    
                    logger.Log(AveLogLevel.DEBUG,ServerAPIResource.GetScriptSafeDomainsFailed, e);
                }
                return siteSettingInfo;

            }

        }

        public object SetObjectData(object obj)
        {
            return null;
        }
    }
}
