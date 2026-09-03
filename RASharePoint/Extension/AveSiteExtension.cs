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
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Common;
using AvePoint.RA.Contract.Common;
using AvePoint.RA.DB.Dao;
using AvePoint.Wrapper.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.SharePoint.Extension
{
    public static class AveSiteExtension
    {
        private static AvePoint.RA.Contract.Services.IRALogger logger = RALogger.GetInstance(typeof(AveSiteExtension));
        private const string BLOCK_DELETE_AND_EDIT = "BlockDelete, BlockEdit";
        private const string ROOTWEB_DECLARE_SETTING_PROPERTY = "ecm_siterecordrestrictions";
        public static void EnsureRecordFeatureEnabled(this IAveSite spSite, Guid mRecordFeatureId)
        {
            try
            {
                if (spSite.Features[mRecordFeatureId] == null)
                {
                    spSite.Features.Add(mRecordFeatureId, true);
                }
            }
            catch (InvalidOperationException ex)
            {
                logger.Warn("In Place Records Management Feature is not installed in this farm, cannot declare document as a record:{0}", ex.ToString());
                //throw;
            }
            catch (Exception ex)
            {
                logger.Warn("Activate In Place Records Management feature error:{0}", ex.ToString());
            }
        }

        public static bool CheckDeclarationSettingIsBlockEditAndDelete(this IAveSite site)
        {
            //has property ecm_siterecordrestrictions AND ecm_siterecordrestrictions is "BlockDelete, BlockEdit"
            return site.RootWeb.AllProperties.ContainsKey(ROOTWEB_DECLARE_SETTING_PROPERTY)
                && site.RootWeb.AllProperties[ROOTWEB_DECLARE_SETTING_PROPERTY].ToString() == BLOCK_DELETE_AND_EDIT;
        }

        public static bool IsOD4BSite(this IAveSite site)
        {
            bool isOD4B = false;
            try
            {
                var siteInfo = site.SiteSerializer.GetObjectData() as AveSiteInfo;
                if (siteInfo != null && siteInfo.WebTemplate != null)
                {
                    isOD4B = siteInfo.WebTemplate.StartsWith("SPSPERS", StringComparison.OrdinalIgnoreCase);
                }
            }
            catch (Exception e)
            {
                logger.Warn("An error occurrred while checking if site is OD4BSite. Url:{0} Error:{1}", site.Url, e.ToString());
            }
            logger.Info("Site:{0} is OD4BSite:{1}", site.Url, isOD4B);
            return isOD4B;
        }

        public static void EnsureWebDeclarationSetting(this IAveSite site)
        {
            if (site.RootWeb.AllProperties.ContainsKey(ROOTWEB_DECLARE_SETTING_PROPERTY)
                && site.RootWeb.AllProperties[ROOTWEB_DECLARE_SETTING_PROPERTY].ToString() == BLOCK_DELETE_AND_EDIT)
            {
                return;
            }

            string orginWebOption = ArchiverCommon.RecordRestrictions.None.ToString();
            if (site.RootWeb.AllProperties.ContainsKey(ROOTWEB_DECLARE_SETTING_PROPERTY))
            {
                orginWebOption = (site.RootWeb.AllProperties[ROOTWEB_DECLARE_SETTING_PROPERTY]?.ToString());
            }
            try
            {
                site.RootWeb.AllProperties[ROOTWEB_DECLARE_SETTING_PROPERTY] = BLOCK_DELETE_AND_EDIT;
                site.RootWeb.Update();
            }
            catch (Exception e)
            {
                logger.Warn($"EnsureWebDeclarationSetting error {e}");
                logger.Warn($"EnsureWebDeclarationSetting update web property {ROOTWEB_DECLARE_SETTING_PROPERTY} error, reset property to {orginWebOption}");
                site.RootWeb.AllProperties[ROOTWEB_DECLARE_SETTING_PROPERTY] = orginWebOption;
            }
            site.RootWeb.ReloadWeb();
            if (!site.RootWeb.AllProperties.ContainsKey(ROOTWEB_DECLARE_SETTING_PROPERTY))
            {
                logger.Warn("Add web prop ecm_siterecordrestrictions is error, please check site DenyAddAndCustomizePages is disabled.");
                throw new Exception("RM_UI_Failed_EnableCustomScript");
            }
            else
            {
                var webOption = site.RootWeb.AllProperties[ROOTWEB_DECLARE_SETTING_PROPERTY].ToString();
                if (null == webOption || !string.Equals(webOption, BLOCK_DELETE_AND_EDIT, StringComparison.OrdinalIgnoreCase))
                {
                    logger.Warn("Update web prop ecm_siterecordrestrictions is error, please check site DenyAddAndCustomizePages is disabled.");
                    throw new Exception("RM_UI_Failed_EnableCustomScript");
                }
            }
        }

        public static string GetWebServerRelativeUrl(this IAveSite site, string listUrl)
        {
            string matchWebUrl = "";
            try
            {
                using (var webs = site.AllWebs)
                {
                    var webUrls = webs.AsQueryable().Select(w => w.ServerRelativeUrl).OrderByDescending(s => s.Length).ToList();
                    foreach (string item in webUrls)
                    {
                        if (listUrl.Contains(item))
                        {
                            matchWebUrl = item;
                            break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Warn("Error Get Web Server Relative Url By List Url :{1}, message:{0}", ex.Message, listUrl);
            }
            return matchWebUrl;
        }
        
        public static List<string> GetListWebServerRelativeUrl(this IAveSite site, string listUrl)
        {
            List<string> matchWebUrls = [];
            try
            {
                using var webs = site.AllWebs;
                var webUrls = webs.AsQueryable().Select(w => w.ServerRelativeUrl).OrderByDescending(s => s.Length).ToList();
                matchWebUrls.AddRange(webUrls.Where(listUrl.Contains));
            }
            catch (Exception ex)
            {
                logger.Warn("Error Get Web Server Relative Url By List Url :{1}, message:{0}", ex.Message, listUrl);
            }
            return matchWebUrls;
        }

        public static int GetMaxItemsPerThrottledOperation(this IAveSite aveSite)
        {
            int maxItemsPer = GetConfiguredSPQueryRowLimit();
            try
            {
                var dataCacheType = aveSite.GetType().GetProperty("DataCache");
                var dataCacheObj = dataCacheType.GetValue(aveSite);
                BindingFlags InstanceBindFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
                var propertiesCacheProp = dataCacheObj.GetType().GetProperty("PropertiesCache", InstanceBindFlags);
                var propertiesCacheObj = propertiesCacheProp.GetValue(dataCacheObj);
                var propertiesDic = (propertiesCacheObj as AveDictionary<string, object>);
                object maxItemsPerObj;
                if (propertiesDic.TryGetValue("MaxItemsPerThrottledOperation", out maxItemsPerObj))
                {
                    maxItemsPer = Convert.ToInt32(maxItemsPerObj);
                    logger.Info($"GetMaxItemsPerThrottledOperation succeed. Count:[{maxItemsPer}]");

                    if (maxItemsPer > 2000)
                    {
                        logger.Info("MaxItemsPerThrottledOperation is > 2000, limit it to 2000");
                        maxItemsPer = 2000;
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Warn("GetMaxItemsPerThrottledOperation by siteCollection NodeItem faild, Error message:", ex.ToString());
            }
            return maxItemsPer;
        }

        private static int GetConfiguredSPQueryRowLimit()
        {
            const int defaultRowLimit = 5000;

            try
            {
                var keyValueDao = PlatformWindsorManager.GetService<IRMKeyValueDao>();
                var setting = keyValueDao?.GetValueByKey(KeyNameCollection.SPQueryRowLimit);
                if (setting == null || string.IsNullOrWhiteSpace(setting.Value))
                {
                    return defaultRowLimit;
                }

                if (int.TryParse(setting.Value, out var configuredRowLimit) && configuredRowLimit > 0)
                {
                    logger.Info($"Use configured SP query row limit from rmkeyvalue. Key:{KeyNameCollection.SPQueryRowLimit}, Value:{configuredRowLimit}.");
                    return configuredRowLimit;
                }

                logger.Warn($"Ignore invalid rmkeyvalue setting. Key:{KeyNameCollection.SPQueryRowLimit}, Value:{setting.Value}. Use default row limit:{defaultRowLimit}.");
            }
            catch (Exception ex)
            {
                logger.Warn("Read configured SP query row limit failed. Error:{0}", ex.ToString());
            }

            return defaultRowLimit;
        }
    }
}
