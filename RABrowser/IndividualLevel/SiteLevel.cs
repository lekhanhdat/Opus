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
using AvePoint.Wrapper.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.Browser.IndividualLevel
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
        public List<SPTreeNodeDto> GetSites(string webAppUrl, string username, int startIndex, uint perPage, ref int childrenCount)
        {
            return GetSites(webAppUrl, username, startIndex, perPage, ref childrenCount, null);
        }

        public List<SPTreeNodeDto> GetSites(string webAppUrl, string username, int startIndex, uint perPage, ref int childrenCount, FilterPolicyInfo filterPolicyInfo)
        {
            Logger.Debug("Browser site collection under web app:{0}.", webAppUrl);
            IEnumerable<AveSiteBrowserInfo> sitesInfo = Query.GetBrowserSites(webAppUrl, username, startIndex, perPage, ref childrenCount, filterPolicyInfo != null);
            if (filterPolicyInfo != null)
            {
                var policies = filterPolicyInfo.FItems.Select(filter => (FilterPolicy)filter).ToList();
                var expressions = filterPolicyInfo.AndOrExpression;
                var filterEngine = new FilterEngine(policies, expressions);
                sitesInfo = sitesInfo.Where(siteInfo =>
                {
                    try
                    {
                        return filterEngine.IsQualified(GetSiteCollectionInfo(siteInfo, policies));
                    }
                    catch (Exception e)
                    {
                        Logger.Warn("Filter out the site failed.Url:{0}.Error message:{1}.", siteInfo.Url, e);
                        return true;
                    }
                });
            }
            var sites = sitesInfo.Select(siteInfo => ConvertToDto(siteInfo)).ToList();
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
                        result.OwnerLogonName = siteInfo.OwnerLoginName;
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
                        if (result.ColumnInfos == null)
                        {
                            result.ColumnInfos = siteInfo.ColumnInfos;
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
            dto.Name = site.Url;
            dto.DisplayName = site.DisplayName;
            dto.Url = site.Url;
            dto.FullPath = site.Url;
            dto.SiteLockStatus = site.BitFlags;
            string templateName = string.Empty;
            string templateTitle = string.Empty;
            uint LCID = 1033;
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
            dto.NodeExtension.ContentDB.ID = site.ContentDBID;
            dto.NodeExtension.ContentDB.Name = site.ContentDBName;
            dto.NodeExtension.AuditActions = site.AuditActions;
            dto.SPObjectId = site.ID.ToString();
            dto.Level = NodeLevel.SiteCollection;
            dto.FarmID = FarmId;
            dto.NodeExtension = FillNodeExtension(dto.NodeExtension, site);
            return dto;
        }

        public string GetQueryConnectionString(string webAppUrl, ref Guid siteId)
        {
            return Query.GetBrowserQueryConnectionString(webAppUrl, ref siteId);
        }
    }
}
