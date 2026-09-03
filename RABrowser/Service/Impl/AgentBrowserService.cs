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
using AvePoint.GCommon.Contract.SharePointBrowser;
using AvePoint.GCommon.Contract.Tree.Object;
using AvePoint.RA.Browser.Handler;
using AvePoint.RA.Browser.Handler.Impl;
using AvePoint.RA.Browser.Provider;
using AvePoint.RA.Browser.Provider.Impl;
using AvePoint.RA.Common.SharePointBrowser;
using AvePoint.RA.CommonUtil;
using AvePoint.SharePointBrowser.Office365;
using AvePoint.Wrapper.Common;
using System;
using System.Reflection;
using System.Threading.Tasks;

namespace AvePoint.RA.Browser.Service.Impl
{
    public class AgentBrowserService
    {

        protected static readonly RALogger Logger = RALogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly ISharePointApiObjectModelProvider ApiObjectModelProvider = Singleton<SharePointApiObjectModelProvider>.SingletonInstance;

        private readonly IBrowserMessageHandler Handler = new SharePointBrowserMessageHandler();

        public async Task<BrowserMessage> HandleMessageAsync(BrowserMessage message, BrowserType browserType)
        {

            if (string.IsNullOrEmpty(message.TenantGroupId))
            {
                message.TenantGroupId = "DocAveOnlineCommonLog";   //Used for DocAveOnline for common logs(Invisible for customers)
                message.TenantGroupOwner = "DocAveOnline Common TenantAccount";
            }

            WrapperConfiguration.WrapperConfigurationForBPOS.SetUserAgent(Office365UserAgentGenerator.Create(ModuleUserAgent.Browser, true));
            //WrapperConfiguration.WrapperConfigurationForBPOS.EnableCache
            //IdentityManager.IdentityMode = IdentityMode.LogicalCallContext;
            //IdentityManager.IdentityType = MicroKernelConstant.IdentityTypeGroupId;
            //IdentityManager.IdentityContent = message.TenantGroupId;

            BrowserMessage messageRes = null;
            try
            {
                string siteUrl = null;
                if (browserType == BrowserType.CheckEndUserPermission)
                {
                    siteUrl = message.MessageContract.SiteCollectionUrl;
                    var apiObjectModel = await ApiObjectModelProvider.GetApiObjectModelProviderAsync(ApiObjectModelType.ClientObjectModel, siteUrl);
                    Office365BrowserMessageHandler handler = new Office365BrowserMessageHandler();
                    messageRes = handler.HandleMessage(message.AgentInfo, message, apiObjectModel);
                }
                else
                {
                    WrapperConfiguration.WrapperConfigurationForBPOS.LoadRootFolderUniqueId = true;

                    SharePointBrowserContract contract = message.BrowserContract as SharePointBrowserContract;
                    if (contract.IsBPOS)
                    {
                        foreach (var node in contract.ParentNodes)
                        {
                            if (node.Level == NodeLevel.SiteCollection)
                            {
                                siteUrl = node.FullPath;
                                break;
                            }
                        }
                    }
                    var apiObjectModel = await ApiObjectModelProvider.GetApiObjectModelProviderAsync(ApiObjectModelType.ClientObjectModel, siteUrl);
                    messageRes = Handler.HandleMessage(message.AgentInfo, message, apiObjectModel, browserType);
                }
            }
            catch(Exception e)
            {
                Logger.Error($"An error occurred while handle message. Error: {e}");
            }

            return messageRes;
        }
    }
}
