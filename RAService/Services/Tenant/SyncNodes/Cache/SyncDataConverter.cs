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
using AvePoint.GCommon.Contract.ExchangeOnline.ExchangeOnlineMailbox.Object;
using AvePoint.GCommon.Contract.Server.ControlPanel.Office365;
using AvePoint.RA.Common.Encryption;
using System.Collections.Generic;

namespace AvePoint.RA.Service.Services.Tenant.SyncNodes.Cache
{
    public static class SyncDataConverter
    {
        #region Group
        public static Dictionary<string, SyncRemoteNodePara> ConvertCacheListToDict(List<SyncRemoteNodePara> list)
        {
            var dict = new Dictionary<string, SyncRemoteNodePara>();
            foreach (SyncRemoteNodePara node in list)
            {
                dict.Add(node.NodeName, node);
            }
            return dict;
        }
        #endregion

        #region Mailbox
        public static SyncRemoteNodePara ConvertDBNodeModelToCacheModel(EmailAccountDto daoModel)
        {
            return new SyncRemoteNodePara()
            {
                NodeName = daoModel.Email,
                ParentId = daoModel.ParentId,
                AppType = daoModel.AppType,
                AuthType = daoModel.ConnectionType,
                ServiceAccountId = daoModel.ServiceAccountId,
                TenantId = daoModel.TenantId,
                UserName = EncryUsernameForMailbox(daoModel.Username),
            };
        }

        private static string EncryUsernameForMailbox(string userName)
        {
            return string.IsNullOrEmpty(userName) ? string.Empty : RMDatabaseDefaultEncryptor.EncryptToString(userName);
        }
        #endregion

        #region RemoteNode
        public static SyncRemoteNodePara ConvertDBNodeModelToCacheModel(RemoteSiteCollection siteCollection)
        {
            return new SyncRemoteNodePara()
            {
                NodeName = siteCollection.url,
                ParentId = siteCollection.parentId,
                AppType = siteCollection.AppType,
                AuthType = siteCollection.AuthType,
                ServiceAccountId = siteCollection.ServiceAccountId,
                TenantId = siteCollection.TenantId,
                UserName = string.Empty,
                ScanSource = siteCollection.ScanSource,
            };
        }
        #endregion
    }
}
