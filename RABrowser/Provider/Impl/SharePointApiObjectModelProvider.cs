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
using AvePoint.Common;
using AvePoint.GCommon.Contract.CentralAdmin.Object;
using AvePoint.GCommon.Contract.Common;
using AvePoint.Wrapper.Common;
using System;
using AvePoint.ObjectModel.Common;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AvePoint.RA.RADataBroker;
using AvePoint.RA.SharePoint.Common;
using AvePoint.GCommon.Contract.Server.ControlPanel.Office365;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.Common;

namespace AvePoint.RA.Browser.Provider.Impl
{
    public class SharePointApiObjectModelProvider : ISharePointApiObjectModelProvider, ISingleton
    {

        protected static readonly IRMRemoteNodeService RemoteNodeService = PlatformWindsorManager.GetService<IRMRemoteNodeService>();

        private SharePointApiObjectModelProvider() { }

        public async Task<AveObjectModelFactory> GetApiObjectModelProviderAsync(ApiObjectModelType type, string siteUrl)
        {
            var accountInfo = await GetBPOSBySiteUrlAsync(siteUrl);
            var factory = new AveClientObjectModelFactory(siteUrl, accountInfo);
            WrapperRuntime.CurrentContext.ModelFactory = factory;
            return factory;
        }
        
        private async Task<AveBPOSAccountInfo> GetBPOSBySiteUrlAsync(string siteUrl)
        {
            var remoteSiteCollection = RemoteNodeService.GetRemoteSiteCollectionByUrl(siteUrl);
            var bposInfo = await PoolUserUtil.GetBPOSInfoAsync(remoteSiteCollection);
            return bposInfo;
        }

        public Task<AveObjectModelFactory> GetApiObjectModelProviderAsync(AveMessage message)
        {
            return GetApiObjectModelProviderAsync(message.ObjectModelType, message.BposInfo.SiteUrl);
        }
    }
}
