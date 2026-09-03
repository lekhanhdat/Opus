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



namespace AvePoint.ObjectModel.Server16
{
    #region using directives
    using System;
    using System.Collections.Generic;
    using AvePoint.GCommon;
    using AvePoint.Wrapper.Common;
    using AvePoint.Wrapper.Resource.ServerAPI2010;
    using AvePoint.Wrapper.Resource.ServerAPI2010;
    using Microsoft.SharePoint;
    using SPDisposeCheck;
    #endregion

    internal class AveSiteSerializer : IAveSiteSerializer
    {
        private static AveLogger logger = AveLogger.GetInstance(typeof(AveSiteSerializer));
        private AveSite m_Site;
        private IAveBackupRestoreQueryService m_QueryService;

        public AveSiteSerializer(IAveBackupRestoreQueryService queryService, AveSite site)
        {
            m_QueryService = queryService;
            m_Site = site;
        }

        /// <summary>
        /// Get Site Basic Info without all templates
        /// </summary>
        /// <returns></returns>
        [SPDisposeCheckIgnore(SPDisposeCheckID._140, "The dispose cleanup is handled automatically by the SharePoint framework")]
        public AveSiteInfo GetSiteBasicInfo()
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveSiteSerializer.GetSiteBasicInfo"))
            {

                AveSiteInfo siteInfo = new AveSiteInfo();
                AveWeb rootWeb = m_Site.RootWeb as AveWeb;

                siteInfo.WebAppUrl = m_Site.WebApplication.GetResponseUri(AveUrlZone.Default).ToString();
                siteInfo.IsHostheader = m_Site.HostHeaderIsSiteName;
                siteInfo.ServerRelativeUrl = m_Site.ServerRelativeUrl;
                siteInfo.CompatibilityLevel = m_Site.CompatibilityLevel;
                siteInfo.SPVersion = m_Site.SPVersion;

                try
                {
                    siteInfo.Id = m_Site.ID;
                    siteInfo.Url = rootWeb.Url;
                    siteInfo.Title = rootWeb.Title;
                    siteInfo.Description = rootWeb.Description;
                    siteInfo.LCID = rootWeb.Language;
                    siteInfo.WebTemplate = AveWebDatabaseSite.IsWebDatabaseWeb(rootWeb.Web) ? AveWebDatabaseSite.TryGetACCSRVWebTemplate(rootWeb.Web) : rootWeb.WebTemplate + "#" + rootWeb.Configuration;
                }
                catch (Exception e)
                {
                    logger.Log(AveLogLevel.WARN, ServerAPIResource.SiteBasicInfo, siteInfo.Url, e);
                }

                //siteInfo.PortalUrl = mSPSite.PortalUrl;
                //siteInfo.PortalName = mSPSite.PortalName;

                try
                {
                    siteInfo.OwnerLogin = m_Site.Owner.LoginName;
                    siteInfo.OwnerName = m_Site.Owner.Name;
                    siteInfo.OwnerEmail = m_Site.Owner.Email;
                }
                catch (Exception e)
                {
                    logger.Log(AveLogLevel.WARN, ServerAPIResource.SiteOwnerInfoGetFailed, siteInfo.Url, e);
                    siteInfo.OwnerLogin = string.Empty;
                    siteInfo.OwnerName = string.Empty;
                    siteInfo.OwnerEmail = string.Empty;
                }

                try
                {
                    if (m_Site.SecondaryContact != null) // we should use try catch as mSPSite.SecondaryContact may throw a exception, why??
                    {
                        siteInfo.SecondaryContactLogin = m_Site.SecondaryContact.LoginName;
                        siteInfo.SecondaryContactName = m_Site.SecondaryContact.Name;
                        siteInfo.SecondaryContactEmail = m_Site.SecondaryContact.Email;
                    }
                }
                catch (Exception e)
                {
                    logger.Log(AveLogLevel.WARN, ServerAPIResource.SiteOwnerInfoGetFailed, siteInfo.Url, e);
                }
                
                try
                {
                    foreach (IAvePrefix prefix in m_Site.WebApplication.Prefixes)
                    {
                        siteInfo.Prefixes.Add(prefix.Name);
                    }
                }
                catch (Exception ex)
                {
                    logger.Log(AveLogLevel.WARN, ServerAPIResource.WebAppPrefixGetFailed + ex);
                }


                return siteInfo;

            }

        }

        [SPDisposeCheckIgnore(SPDisposeCheckID._140, "The dispose cleanup is handled automatically by the SharePoint framework")]
        public AveSiteInfo GetObjectData()
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveSiteSerializer.GetObjectData"))
            {

                AveSiteInfo siteInfo = GetSiteBasicInfo();

                GetAllWebsTemplate(siteInfo);

                return siteInfo;


            }

        }

        /// <summary>
        /// Get all webs template
        /// </summary>
        /// <param name="siteInfo"></param>
        internal void GetAllWebsTemplate(AveSiteInfo siteInfo)
        {
            try
            {
                siteInfo.AllWebTemplates = GetAllWebsTemplateFromDB(m_Site.RootWeb.Language);
            }
            catch (Exception ex)
            {
                logger.Log(AveLogLevel.WARN, ServerAPIResource.WebTemplateGetFailed, siteInfo.Url, ex);
            }
        }

        public Dictionary<Guid, string> GetAllWebsTemplateFromDB(uint lcid)
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveSiteSerializer.GetAllWebsTemplateFromDB"))
            {

                return m_QueryService.GetALLWebTemplates(m_Site, lcid);

            }

        }

        public string WebTemplateIdName(int id, string configuration, SPWebTemplateCollection webTemplateCollection)
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveSiteSerializer.WebTemplateIdName"))
            {

                string webTemplateStr = null;
                string sConfig = "#" + configuration;
                foreach (SPWebTemplate sWebTemplate in webTemplateCollection)
                {
                    if (sWebTemplate.ID == id && sWebTemplate.Name.EndsWith(sConfig, StringComparison.OrdinalIgnoreCase))
                    {
                        webTemplateStr = sWebTemplate.Name;
                        break;
                    }
                }
                return webTemplateStr;

            }

        }

        public object SetObjectData(object obj)
        {
            return null;
        }
    }
}
