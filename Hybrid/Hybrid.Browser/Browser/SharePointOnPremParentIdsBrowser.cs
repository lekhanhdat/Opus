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
    public class SharePointOnPremParentIdsBrowser : IBrowser
    {

        private static readonly AvePoint.GCommon.AveLogger Logger = AvePoint.GCommon.AveLogger.GetInstance(typeof(SharePointOnPremParentIdsBrowser));

        public HybridBrowserType BrowserType => HybridBrowserType.SharePointOnPremParentId;

        public string Browse(string message)
        {
            var result = new SharePointOnPremParentIdsBrowserResult();
            try
            {
                var args = SerializerHelper.DeserializeByJsonConvert<SharePointOnPremParentIdsBrowserArgs>(message);
                var factory = AveObjectModelFactory.CreateObjectModelFactory(null, null, AveContextKind.ServerObjectModel);
                var browseArgs = JsonConvert.DeserializeObject<OnPremiseSPBrowseParentIdsArgs>(args.Message);

                var parentIds = new List<string>();
                using (var site = factory.CreateSite(browseArgs.SiteUrl))
                {
                    var web = site.AllWebs[new Guid(browseArgs.WebId)];
                    if(browseArgs.Level == SharePointOnPremBrowseParentIdsLevel.Folder)
                    {
                        var list = web.GetList(new Guid(browseArgs.ListId));
                        var folder = list.GetFolder(browseArgs.FolderServerRelativeUrl);
                        var rootFolderId = list.RootFolder.UniqueId;
                        while(rootFolderId != folder?.UniqueId && folder?.UniqueId != Guid.Empty)
                        {
                            parentIds.Add(folder.UniqueId.ToString());
                            folder = folder.ParentFolder;
                        }
                        parentIds.Add(rootFolderId.ToString());
                    }
                    else if(browseArgs.Level == SharePointOnPremBrowseParentIdsLevel.Web)
                    {
                        var rootWebId = site.RootWeb.ID;
                        while(rootWebId != web?.ID && web?.ID != Guid.Empty)
                        {
                            parentIds.Add(web.ID.ToString());
                            web = web.ParentWeb;
                        }
                        parentIds.Add(rootWebId.ToString());
                    }
                }
                result.Result = SharePointOnPremParentIdsBrowserResultEnum.Successed;
                var tenantId = CommonConfiguration.getConfig(HybridAppSettingKey.CustomerTenantId);
                var cache = new HybridBrowserCache
                {
                    BatchId = new Guid(args.BatchId),
                    TenantId = tenantId,
                    CacheData = JsonConvert.SerializeObject(parentIds),
                };
                var res = System.Threading.Tasks.Task.Run(() => HybridAgentApiClientUtil.Client.SharePointOnPremBrowserService.AddBrowserCache(cache)).Result;
                if (!res)
                {
                    Logger.Warn("SharePoint on-prem add parent ids brwoser cache failed.");
                }
            }
            catch (Exception e)
            {
                Logger.Error($"An error occur while browse sharepoint term store info. Error: {e}");
                result.Result = SharePointOnPremParentIdsBrowserResultEnum.Failed;
                result.Message = e.Message;
            }
            return SerializerHelper.SerializeByJsonSerializer(result);
        }
    }
}
