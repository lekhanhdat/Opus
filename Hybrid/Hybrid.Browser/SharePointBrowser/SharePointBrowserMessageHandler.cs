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
using AvePoint.GCommon.Contract.CentralAdmin.Object;
using AvePoint.GCommon.Contract.SharePointBrowser;
using AvePoint.GCommon.Contract.Tree.Object;
using AvePoint.GCommon.Utility.Cryptography;
using AvePoint.RA.Hybrid.Browser.SharePointBrowser.Worker;
using AvePoint.Wrapper.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.Hybrid.Browser.SharePointBrowser
{
    public class SharePointBrowserMessageHandler
    {
        public static BrowserMessage HandleMessage(BrowserMessage message)
        {
            var apiObjectModel = GetApiObjectModelProvider(message);
            var worker = new AdvanceSearchWorker(message.BrowserContract as SharePointBrowserContract, apiObjectModel);
            worker.BrowseChildren();
            return message;
        }

        private static AveObjectModelFactory GetApiObjectModelProvider(BrowserMessage message)
        {
            var modelType = ApiObjectModelType.Auto;
            var contract = message.BrowserContract as SharePointBrowserContract;
            BposInfo bpos = null;
            AveObjectModelFactory apiObjectModel = null;
            if (contract.IsBPOS)
            {
                foreach (var node in contract.ParentNodes)
                {
                    if (node.Level == NodeLevel.SiteCollection)
                    {
                        bpos = node.NodeExtension.BposInfo;
                        break;
                    }

                    if (node.Level == NodeLevel.Farm)
                    {
                        foreach (var webapp in node.Children)
                        {
                            if (webapp.Children.Count > 0)
                            {
                                bpos = webapp.Children[0]?.NodeExtension?.BposInfo;
                                break;
                            }
                        }
                        break;
                    }

                    if (bpos != null)
                    {
                        modelType = ApiObjectModelType.ClientObjectModel;
                    }
                }
            }
            return GetApiObjectModelProvider(modelType, bpos);
        }

        private static AveObjectModelFactory GetApiObjectModelProvider(ApiObjectModelType type, BposInfo info)
        {
            var siteUrl = info?.SiteUrl ?? default(string);
            var accountInfo = default(AveBPOSAccountInfo);
            if(info != null)
            {
                switch(info.AuthorizeType)
                {
                    case GCommon.Contract.SharePointBrowser.AuthorizeType.AccountInfo:
                    case GCommon.Contract.SharePointBrowser.AuthorizeType.UserNameAndPwd:
                        accountInfo = new AveBPOSAccountInfo
                        {
                            Domain = info.UserAccountInfo.Domain,
                            Password = CryptoUtil.ConvertBytesToString(CspCommunicationWrapper.UnWrapKey(info.UserAccountInfo.Password)),//EncryptionFactory.GetDefaultEncryption().DecryptString(info.UserAccountInfo.Password),
                            UserName = info.UserAccountInfo.Username,
                            ConnectionType = BposConnectionType.ServiceAccount
                        };
                        break;
                    case GCommon.Contract.SharePointBrowser.AuthorizeType.AppTokenInfo:
                        accountInfo = new AveBPOSAccountInfo()
                        {
                            AppCert = new X509Certificate2(Convert.FromBase64String(info.AppTokenInfo.AppTokenCertBase64String), CryptoUtil.ConvertBytesToString(CspCommunicationWrapper.UnWrapKey(Convert.FromBase64String(info.AppTokenInfo.AppTokenCertPassword))), X509KeyStorageFlags.MachineKeySet | X509KeyStorageFlags.PersistKeySet | X509KeyStorageFlags.Exportable),
                            ClientId = info.AppTokenInfo.ApplicationId,
                            TenantId = info.AppTokenInfo.TenantId,
                            AzureRegion = (Wrapper.Common.AzureRegions)info.AppTokenInfo.AzureRegion,
                            ConnectionType = BposConnectionType.AppToken,
                        };
                        break;
                    case GCommon.Contract.SharePointBrowser.AuthorizeType.MixAuthorizeInfo:
                        accountInfo = new AveBPOSAccountInfo()
                        {
                            UserName = info.UserAccountInfo.Username,
                            Password = CryptoUtil.ConvertBytesToString(CspCommunicationWrapper.UnWrapKey(info.UserAccountInfo.Password)),//EncryptionFactory.GetDefaultEncryption().DecryptString(info.UserAccountInfo.Password),
                            Domain = info.UserAccountInfo.Domain,
                            AppCert = new X509Certificate2(Convert.FromBase64String(info.AppTokenInfo.AppTokenCertBase64String), CryptoUtil.ConvertBytesToString(CspCommunicationWrapper.UnWrapKey(Convert.FromBase64String(info.AppTokenInfo.AppTokenCertPassword))), X509KeyStorageFlags.MachineKeySet | X509KeyStorageFlags.PersistKeySet | X509KeyStorageFlags.Exportable),
                            ClientId = info.AppTokenInfo.ApplicationId,
                            TenantId = info.AppTokenInfo.TenantId,
                            AzureRegion = (Wrapper.Common.AzureRegions)info.AppTokenInfo.AzureRegion,
                            ConnectionType = BposConnectionType.MixAuthorize
                        };
                        break;
                }
            }
            var contextKind = (AveContextKind)Enum.Parse(typeof(AveContextKind), type.ToString(), true);
            return AveObjectModelFactory.CreateObjectModelFactory(siteUrl, accountInfo, contextKind);
        }
    }
}
