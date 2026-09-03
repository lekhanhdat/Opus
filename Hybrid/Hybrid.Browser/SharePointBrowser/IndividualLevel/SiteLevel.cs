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
using AvePoint.Common.FilterEngine;
using AvePoint.GCommon;
using AvePoint.GCommon.Contract.CommonFilter;
using AvePoint.GCommon.Contract.Server.ControlPanel.FilterPolicy.Object;
using AvePoint.GCommon.Contract.Tree.Object;
using AvePoint.GCommon.Utility;
using AvePoint.RA.Hybrid.Browser.SharePointBrowser.DeploymentManager.BrowserFilter;
using AvePoint.Wrapper.Common;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.Hybrid.Browser.SharePointBrowser.IndividualLevel
{
    public class SiteLevel: IndividualBase
    {
        public SiteLevel(AveObjectModelFactory objectModel, string siteUrl)
            : base(objectModel, string.Empty, siteUrl)
        {
        }

        /// <summary>
        /// userName用于Trim作用
        /// </summary>
        /// <param name="webAppUrl"></param>
        /// <param name="userName"></param>
        /// <returns></returns>
        public List<SPTreeNodeDto> GetSites(string webAppUrl, List<string> usernames, int startIndex, uint perPage, ref int childrenCount)
        {
            return GetSites(webAppUrl, usernames, startIndex, perPage, ref childrenCount, null);
        }
        public List<SPTreeNodeDto> GetSites(string webAppUrl, List<string> usernames, int startIndex, uint perPage, ref int childrenCount, FilterPolicyInfo filterPolicyInfo)
        {
            var hasError = false;
            return GetSites(webAppUrl, usernames, startIndex, perPage, ref childrenCount, filterPolicyInfo, ref hasError);
        }

        public List<SPTreeNodeDto> GetSites(string webAppUrl, List<string> usernames, int startIndex, uint perPage, ref int childrenCount, FilterPolicyInfo filterPolicyInfo, ref bool hasError, bool trimResult = true)
        {
#if DEBUG
            Stopwatch sw = new Stopwatch();
            sw.Start();
#endif
            IAveWebApplication webApp = ObjectModel.CreateWebApplication(webAppUrl);
            if (webApp == null)
            {
                throw new AveException("Cannot find web application:{0}", webAppUrl);
            }
            StringBuilder userNameString = new StringBuilder();
            if (usernames != null)
            {
                foreach (string username in usernames)
                {
                    userNameString.Append(username + ";");
                }
            }
            Logger.Debug("Browser site collection under web app:{0}.", webApp.AlternateUrls.GetResponseUrl(AveUrlZone.Default).Uri.ToString());
            IEnumerable<AveSiteBrowserInfo> sitesInfo = Query.GetBrowserSites(webApp, usernames, startIndex, perPage, ref childrenCount, ref hasError, filterPolicyInfo != null);
            var sites = new List<SPTreeNodeDto>();
            if (filterPolicyInfo != null)
            {
                var policies = filterPolicyInfo.FItems.Select(filter => (FilterPolicy)filter).ToList();
                var expressions = filterPolicyInfo.AndOrExpression;
                var filterEngine = new FilterEngine(policies, expressions);
                foreach (AveSiteBrowserInfo siteInfo in sitesInfo)
                {
                    var isQualified = true;
                    try
                    {
                        isQualified = filterEngine.IsQualified(GetSiteCollectionInfo(siteInfo, policies));
                    }
                    catch (Exception e)
                    {
                        Logger.Info("Filter out the site failed.Url:{0}.Error message:{1}.", siteInfo.Url, e);
                    }
                    if (isQualified)
                    {
                        sites.Add(ConvertToDto(siteInfo));
                    }
                    else
                    {
                        if (trimResult)
                        {
                            continue;
                        }
                        else
                        {
                            var site = ConvertToDto(siteInfo);
                            site.CheckNumber = -1;
                            sites.Add(site);
                        }
                    }
                }
            }
            else
            {
                sites = sitesInfo.Select(siteInfo => ConvertToDto(siteInfo)).ToList();
            }
#if DEBUG
            sw.Stop();
            Logger.Debug("Brower Sites Elapsed Time: {0}, SiteCount: {1}, WebAppUrl: {2}, startIndex: {3}, PerPage: {4} ,ChildrenCount: {5} ", sw.Elapsed.ToString(), sites.Count, webAppUrl, startIndex, perPage, childrenCount);
#endif
            return sites;
        }

        public static SiteCollectionInfo GetSiteCollectionInfo(AveSiteBrowserInfo siteInfo, List<FilterPolicy> policies)
        {

            SiteCollectionInfo result = new SiteCollectionInfo();
            policies = policies.Where(filter => filter.Level == PolicyLevel.SiteCollection).Distinct(FilterRuleTypeEqualityComparer.GetInstance()).ToList();

            foreach (FilterPolicy policy in policies)
            {
                string ruleName = policy.Rule.GetType().Name;
                ruleName = ruleName.Substring(ruleName.LastIndexOf('.') + 1);
                switch (ruleName)
                {
                    case "UrlRule":
                        result.Url = siteInfo.Url;
                        break;
                    case "TitleRule":
                        result.Title = siteInfo.Title;
                        break;
                    case "ModifiedRule":
                        result.Modified = siteInfo.Modified;//siteInfo 中的Modified取是从数据库中取的本就是Utc时间
                        break;
                    case "CreatedRule":
                        result.Created = siteInfo.Created;//siteInfo 中的Created取是从数据库中取的本就是Utc时间
                        break;
                    case "CreatedByRule":
                    case "OwnerRule":
                        string LogonNameNoPrefix = siteInfo.OwnerLoginName;
                        if (!string.IsNullOrEmpty(siteInfo.OwnerLoginName) && siteInfo.OwnerLoginName.LastIndexOf('|') > 0)
                        {
                            LogonNameNoPrefix = siteInfo.OwnerLoginName.Substring(siteInfo.OwnerLoginName.LastIndexOf('|') + 1);
                        }
                        result.OwnerLogonName = LogonNameNoPrefix;
                        result.OwnerLogonNameWithPrefix = siteInfo.OwnerLoginName;
                        result.OwnerTitle = siteInfo.OwnerTitle;
                        break;
                    case "TemplateRule":
                        /*
                         * 需要说明的是:
                         * 对于使用"Save site as template"方式生成的模板创建的site,
                         * 其站点模板Id等同于其基础模板Id.
                         * 因而, 使用名字过滤时要使用其基础模板的名字.
                         * 如, 基于Team site创建的模板, 再使用该模板创建site, 则该site的tmplate id 为"STS#0",
                         * 过滤时应填写"Team Site".
                         * 
                         * 此处逻辑与TemplateIdRule保持一致.(需和QA交代清楚.)
                         * Web级别filter与此相同.
                         */
                        result.TemplateName = siteInfo.TemplateTitle;
                        break;
                    case "TemplateIdRule":
                        result.Template = siteInfo.TemplateName;
                        break;
                    case "CustomPropertyTextRule":
                    case "CustomPropertyNumberRule":
                    case "CustomPropertyDateTimeRule":
                    case "CustomPropertyBooleanRule":
                        if (result.Properties == null)
                        {
                            result.Properties = siteInfo.Properties;
                        }
                        break;
                    default:
                        throw new AveException("Invalid Rule.{0}", ruleName);
                }
            }
            return result;
        }

        private SPTreeNodeDto ConvertToDto(AveSiteBrowserInfo site)
        {
            SPTreeNodeDto dto = new SPTreeNodeDto();
            dto.Name = site.Title;
            dto.DisplayName = site.Title;
            dto.Url = site.Url;
            dto.FullPath = site.Url;
            dto.SiteLockStatusValue = site.BitFlags;
            string templateName = string.Empty;
            string templateTitle = string.Empty;
            uint LCID = 1033;
            dto.NodeExtension.PermissionList = new List<SPTreePermissionMappingDto>();
            foreach (var mask in site.Masks)
            {
                var permissionMappingDto = new SPTreePermissionMappingDto { UserName = mask.Key, Url = site.Url };
                permissionMappingDto.Permission = mask.Value;
                dto.NodeExtension.PermissionList.Add(permissionMappingDto);
            }
            try
            {
                dto.Title = site.Title;
                templateName = site.TemplateName;
                templateTitle = site.TemplateTitle;
                LCID = site.Language;
            }
            catch (Exception e)
            {
                Logger.Warn(e.Message);
            }
            if (dto.NodeExtension == null)
            {
                dto.NodeExtension = new NodeExtensionDto();
            }
            dto.NodeExtension.TemplateName = templateName;
            dto.NodeExtension.TemplateTitle = templateTitle;
            dto.NodeExtension.LCID = LCID;
            dto.NodeExtension.ContentDB = new ContentDB();
            dto.NodeExtension.ContentDB.ID = site.ContentDBID;//site.ContentDatabase.ID.ToString();
            dto.NodeExtension.ContentDB.Name = site.ContentDBName;//site.ContentDatabase.DisplayName;
            dto.NodeExtension.AuditActions = site.AuditActions;
            dto.SPObjectId = site.ID.ToString();
            dto.Level = NodeLevel.SiteCollection;
            dto.FarmID = FarmId;
            dto.NodeExtension = FillNodeExtension(dto.NodeExtension, site);
            //TO DO: dto.EnableRBS = GetRBSEnabled();  
            dto.NodeExtension.CompatibilityLevel = GetCompatibilityLevelFromPlatformVersion(site.PlatformVersion);
            return dto;
        }

        private CompatibilityLevelType GetCompatibilityLevelFromPlatformVersion(string platformVersion)
        {
            //platformVersion format like "4.0.25.0","15.0.35.0"
            if (string.IsNullOrEmpty(platformVersion))
            {
                return CompatibilityLevelType.None;
            }
            else
            {
                try
                {
                    return GetCompatibilityLevelType(new Version(platformVersion).Major);
                }
                catch (Exception ex)
                {
                    Logger.Info("cannot parse platform version:{0}, exception:{1}", platformVersion, ex.ToString());
                    return CompatibilityLevelType.None;
                }
            }
        }

        private CompatibilityLevelType GetCompatibilityLevelType(int majorVersion)
        {
            switch (majorVersion)
            {
                case 2:
                case 3:
                case 4:
                case 11:
                case 12:
                case 14:
                    return CompatibilityLevelType.None;
                default:
                    return CompatibilityLevelType.SP2013Mode;
            }
        }

        private SPTreeNodeDto ConvertToDto(IAveSite site)
        {
            SPTreeNodeDto dto = new SPTreeNodeDto();
            dto.Name = site.Url;
            dto.DisplayName = GetSiteServerRelativeUrl(site);
            dto.Url = site.Url;
            dto.FullPath = site.Url;
            dto.SiteLockStatusValue = (uint)site.Flags;
            string templateName = string.Empty;
            string templateTitle = string.Empty;
            uint LCID = 1033;
            try
            {
                dto.Title = site.RootWeb.Title;
                templateName = site.RootWeb.WebTemplate + "#" + site.RootWeb.Configuration.ToString();
                try// bpos-s does not support this
                {
                    templateTitle = site.GetWebTemplates(site.RootWeb.Language)[templateName].Title;
                }
                catch (Exception e)
                {
                    Logger.Debug("BPOS-S does not support TemplateTitle, Error Message: {0}", e.ToString());
                }
                LCID = site.RootWeb.Language;
            }
            catch (Exception e)
            {
                Logger.Warn(e.Message);
            }
            if (dto.NodeExtension == null)
            {
                dto.NodeExtension = new NodeExtensionDto();
            }
            dto.NodeExtension.TemplateName = templateName;
            dto.NodeExtension.TemplateTitle = templateTitle;
            dto.NodeExtension.LCID = LCID;
            try
            {
                dto.NodeExtension.ContentDB = new ContentDB();
                dto.NodeExtension.ContentDB.ID = site.ContentDatabase.ID.ToString();
                dto.NodeExtension.ContentDB.Name = site.ContentDatabase.DisplayName;
                dto.NodeExtension.AuditActions = (int)site.RootWeb.Audit.AuditFlags;
            }
            catch (Exception ex)
            {
                Logger.Warn(ex.ToString());
            }
            dto.SPObjectId = site.ID.ToString();
            dto.Level = NodeLevel.SiteCollection;
            dto.FarmID = FarmId;
            dto.NodeExtension = FillNodeExtension(dto.NodeExtension, site);
            //TO DO: dto.EnableRBS = GetRBSEnabled();  
            dto.NodeExtension.CompatibilityLevel = GetCompatibilityLevelType(site.CompatibilityLevel);
            return dto;
        }

        private string GetSiteServerRelativeUrl(IAveSite site)
        {
            string siteUrl = site.Url;
            if (!siteUrl.StartsWith("http", StringComparison.OrdinalIgnoreCase) && !siteUrl.StartsWith("https", StringComparison.OrdinalIgnoreCase))
            {
                return string.Empty;
            }
            else
            {
                if (site.HostHeaderIsSiteName)
                {
                    return siteUrl.Substring(siteUrl.IndexOf(':') + 3);
                }
                else
                {
                    return site.ServerRelativeUrl;
                }
            }
        }

        private bool GetRBSEnabled()
        {

            bool enableRBS = false;
            //SPRemoteBlobStorageSettings rbss = contentDatabase.RemoteBlobStorageSettings;
            //if (rbss != null)
            //{
            //    enableRBS = rbss.Installed() && rbss.Enabled && string.Equals(rbss.ActiveProviderName, RBSProviderName, StringComparison.OrdinalIgnoreCase);
            //}
            return enableRBS;
        }

        public string GetQueryConnectionString(string siteUrl, ref Guid siteId)
        {
            return Query.GetBrowserQueryConnectionString(siteUrl, ref siteId);
        }

        public void GetSiteNodeSelf(SPTreeNodeDto siteNode)
        {
            //Query.get
        }

        public SPTreeNodeDto ConvertToSiteDto(IAveSite site)
        {
            return ConvertToDto(site);
        }
    }
}
