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
using AvePoint.GCommon.Contract.Tree.Object;
using AvePoint.Hybrid.ClientLibrary.Data;
using AvePoint.Hybrid.Contract.SignalR;
using AvePoint.Hybrid.Utility;
using AvePoint.RA.Common.Global.Utils;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Hybrid.Browser.Contract;
using AvePoint.RA.Hybrid.Browser.SharePointBrowser;
using AvePoint.RA.Hybrid.Browser.Util;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.Hybrid.Browser.Browser
{
    public class SharePointOnPremBrowser : IBrowser
    {

        private static readonly AvePoint.GCommon.AveLogger Logger = AvePoint.GCommon.AveLogger.GetInstance(typeof(SharePointOnPremBrowser));

        public HybridBrowserType BrowserType => HybridBrowserType.SharePointOnPrem;

        private static readonly JsonSerializerSettings SerializerSettings = new JsonSerializerSettings
        {
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore
        };

        public string Browse(string message)
        {
            var result = new SharePointOnPremBrowserResult();
            try
            {
                var args = SerializerHelper.DeserializeByJsonConvert<SharePointOnPremBrowserArgs>(message);
                var treeMessage = JsonConvert.DeserializeObject<SPTreeMessage>(args.Message, SerializerSettings);
                var treeMessageResult = TreeBrowser.Browse(treeMessage);
                result.Result = SharePointOnPremBrowserResultEnum.Successed;
                var tenantId = CommonConfiguration.getConfig(HybridAppSettingKey.CustomerTenantId);
                var cache = new HybridBrowserCache
                {
                    BatchId = new Guid(args.BatchId),
                    TenantId = tenantId,
                    CacheData = JsonConvert.SerializeObject(treeMessageResult, SerializerSettings),
                };
                var res = Task.Run(() => HybridAgentApiClientUtil.Client.SharePointOnPremBrowserService.AddBrowserCache(cache)).Result;
                if(!res)
                {
                    Logger.Warn("SharePoint on-prem add brwoser cache failed.");
                }
            }
            catch(Exception e)
            {
                Logger.Error($"An error occur while browse sharepoint tree node. Error: {e}");
                result.Result = SharePointOnPremBrowserResultEnum.Failed;
                result.Message = e.Message;
            }
            return SerializerHelper.SerializeByJsonSerializer(result);
        }
    }
}
