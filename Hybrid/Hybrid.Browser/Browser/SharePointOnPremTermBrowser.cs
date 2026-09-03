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
using AvePoint.Hybrid.ClientLibrary.Data;
using AvePoint.Hybrid.Contract.SignalR;
using AvePoint.Hybrid.Utility;
using AvePoint.RA.Common.Global.Utils;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.OnPremiseSharePoint;
using AvePoint.RA.Hybrid.Browser.Contract;
using AvePoint.RA.Hybrid.Browser.Util;
using AvePoint.Wrapper.Common;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.Hybrid.Browser.Browser
{
    public class SharePointOnPremTermBrowser : IBrowser
    {

        private static readonly AvePoint.GCommon.AveLogger Logger = AvePoint.GCommon.AveLogger.GetInstance(typeof(SharePointOnPremTermBrowser));

        public HybridBrowserType BrowserType => HybridBrowserType.SharePointOnPremTerm;

        public string Browse(string message)
        {
            var result = new SharePointOnPremTermBrowserResult();
            try
            {
                var args = SerializerHelper.DeserializeByJsonConvert<SharePointOnPremTermBrowserArgs>(message);
                var factory = AveObjectModelFactory.CreateObjectModelFactory(null, null, AveContextKind.ServerObjectModel);
                var site = factory.CreateSite(args.Message);
                var defaultTermStore = site.AveSPTaxonomySession.TermStores.FirstOrDefault();
                var termStoreId = defaultTermStore.ID;
                var termStoreName = defaultTermStore.Name;
                result.Result = SharePointOnPremTermBrowserResultEnum.Successed;
                var tenantId = CommonConfiguration.getConfig(HybridAppSettingKey.CustomerTenantId);
                var cacheData = new OnPremiseSPTermInfo
                {
                    TermStoreId = termStoreId,
                    TermStoreName = termStoreName
                };
                var cache = new HybridBrowserCache
                {
                    BatchId = new Guid(args.BatchId),
                    TenantId = tenantId,
                    CacheData = JsonConvert.SerializeObject(cacheData),
                };
                var res = System.Threading.Tasks.Task.Run(() => HybridAgentApiClientUtil.Client.SharePointOnPremBrowserService.AddBrowserCache(cache)).Result;
                if (!res)
                {
                    Logger.Warn("SharePoint on-prem add term brwoser cache failed.");
                }
            }
            catch (Exception e)
            {
                Logger.Error($"An error occur while browse sharepoint term store info. Error: {e}");
                result.Result = SharePointOnPremTermBrowserResultEnum.Failed;
                result.Message = e.Message;
            }
            return SerializerHelper.SerializeByJsonSerializer(result);
        }
    }
}
