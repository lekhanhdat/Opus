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

using AvePoint.GCommon;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Services;
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
        private static AveLogger logger = AveLogger.GetInstance(typeof(AveSiteExtension));
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

        public static void EnsureWebDeclarationSetting(this IAveSite site)
        {
            if (site.RootWeb.AllProperties.ContainsKey(ROOTWEB_DECLARE_SETTING_PROPERTY)
                && site.RootWeb.AllProperties[ROOTWEB_DECLARE_SETTING_PROPERTY].ToString() == BLOCK_DELETE_AND_EDIT)
            {
                return;
            }

            site.RootWeb.AllProperties[ROOTWEB_DECLARE_SETTING_PROPERTY] = BLOCK_DELETE_AND_EDIT;
            site.RootWeb.Update();
            site.RootWeb.ReloadWeb();
            if (!site.RootWeb.AllProperties.ContainsKey(ROOTWEB_DECLARE_SETTING_PROPERTY))
            {
                throw new Exception("Add web prop ecm_siterecordrestrictions is error, please check site DenyAddAndCustomizePages is disabled.");
            }
            else
            {
                var webOption = site.RootWeb.AllProperties[ROOTWEB_DECLARE_SETTING_PROPERTY].ToString();
                if (null == webOption || !string.Equals(webOption, BLOCK_DELETE_AND_EDIT, StringComparison.OrdinalIgnoreCase))
                {
                    throw new Exception("Update web prop RevIM is error, please check site DenyAddAndCustomizePages is disabled.");
                }
            }
        }

        public static string GetWebServerRelativeUrl(this IAveSite site, string listUrl)
        {
            string matchWebUrl = "";
            try
            {
                //List<string> mWebUrls = new List<string>();
                //using (var webs = site.AllWebs)
                //{
                //    var webUrls = webs.AsQueryable().Select(w => w.ServerRelativeUrl).OrderByDescending(s => s.Length).ToList();
                //    foreach (string item in webUrls)
                //    {
                //        if (listUrl.Contains(item))
                //        {
                //            matchWebUrl = item;
                //            break;
                //        }
                //    }                   
                //}
            }
            catch (Exception ex)
            {
                logger.Warn("Error Get Web Server Relative Url By List Url :{1}, message:{0}", ex.Message, listUrl.LogBase64());
            }
            return matchWebUrl;
        }

        public static int GetMaxItemsPerThrottledOperation(this IAveSite aveSite)
        {
            int maxItemsPer = 5000;
            try
            {
                var dataCacheType = aveSite.GetType().GetProperty("DataCache");
                var dataCacheObj = dataCacheType.GetValue(aveSite);
                BindingFlags InstanceBindFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
                var propertiesCacheProp = dataCacheObj.GetType().GetProperty("PropertiesCache", InstanceBindFlags);
                var propertiesCacheObj = propertiesCacheProp.GetValue(dataCacheObj);
                var propertiesDic = (propertiesCacheObj as Dictionary<string, object>);
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
    }
}
