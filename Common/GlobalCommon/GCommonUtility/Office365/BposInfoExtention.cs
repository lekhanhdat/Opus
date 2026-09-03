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
using AvePoint.Common.Portal;
using AvePoint.GCommon.Contract.CentralAdmin.Object;
using AvePoint.GCommon.Contract.ExchangeOnline.ExchangeOnlineMailbox.Object;
using AvePoint.GCommon.Contract.Server.ControlPanel.Office365;
using AvePoint.GCommon.Contract.Server.ControlPanel.Office365Account.Object;
using AvePoint.GCommon.Contract.Tree.Object;
using AvePoint.GCommon.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.GCommon.Contract.CentralAdmin.Object
{
    public static class BposInfoExtention
    {
        private static AveLogger logger = AveLogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);

        public static void AddCertInfo(this BposInfo bposInfo, SPTreeNodeDto treeNode, Dictionary<string, AppProfile> scUrlToAppProfileDict)
        {
            var remoteSiteCollection = new RemoteSiteCollection()
            {
                url = treeNode.Url,
                AuthType = treeNode.NodeExtension.BposInfo.ConnectionType
            };
            AddCertInfo(bposInfo, remoteSiteCollection, scUrlToAppProfileDict);
        }

        public static void AddCertInfo(this BposInfo bposInfo, RemoteSiteCollection remoteSiteCollection, Dictionary<string, AppProfile> scUrlToAppProfileDict)
        {
            if (bposInfo == null)
            {
                logger.Error("Bpos info is null.");
                return;
            }
            bposInfo.TenantGroupId = TenantThreadLocalValue.LogonGroupId;
            if (remoteSiteCollection == null)
            {
                logger.Error("SiteCollection is null.");
                return;
            }
            if (scUrlToAppProfileDict == null || scUrlToAppProfileDict.Keys.Count == 0)
            {
                return;
            }
            if (remoteSiteCollection.AuthType != BposConnectionType.AppToken)
            {
                return;
            }
            if (remoteSiteCollection.AuthType == BposConnectionType.AppToken || remoteSiteCollection.AuthType == BposConnectionType.Modern)
            {
                if (scUrlToAppProfileDict.ContainsKey(remoteSiteCollection.url))
                {
                    AppProfile appProfile = scUrlToAppProfileDict[remoteSiteCollection.url];
                    bposInfo.UserAccountInfo.AppId = appProfile.Id;
                    bposInfo.UserAccountInfo.AppClientId = appProfile.AppClientId;
                    bposInfo.UserAccountInfo.AppCertSecret = appProfile.AppCertSecret;
                    //.UserAccountInfo.AppCertContent = appProfile.AppCertContent;
                    bposInfo.UserAccountInfo.AADEnvironment = appProfile.AADEnvironment;
                    bposInfo.UserAccountInfo.AppCertSecretContent = appProfile.AppCertSecretContent;
                }
                else
                {
                    logger.Error("Failed to find the app profile dict, SiteCollection url is {0}", remoteSiteCollection.url);
                    bposInfo.UserAccountInfo.AppId = string.Empty;
                    bposInfo.UserAccountInfo.AppClientId = string.Empty;
                    bposInfo.UserAccountInfo.AppCertSecret = string.Empty;
                    //bposInfo.UserAccountInfo.AppCertContent = string.Empty;
                    bposInfo.UserAccountInfo.AppCertSecretContent = string.Empty;
                    bposInfo.UserAccountInfo.AADEnvironment = AADEnvironment.None;
                }
            }
        }

        public static void AddCertInfo(this BposInfo bposInfo, EmailAccountDto mailbox, Dictionary<string, AppProfile> mailboxNameToAppProfileDict)
        {
            if (bposInfo == null)
            {
                logger.Error("Bpos info is null.");
                return;
            }
            bposInfo.TenantGroupId = TenantThreadLocalValue.LogonGroupId;
            if (mailbox == null)
            {
                logger.Error("Mailbox is null.");
                return;
            }
            if (mailboxNameToAppProfileDict == null || mailboxNameToAppProfileDict.Keys.Count == 0)
            {
                return;
            }
            if (mailbox.ConnectionType != BposConnectionType.AppToken && mailbox.ConnectionType != BposConnectionType.Modern)
            {
                return;
            }
            if (mailbox.ConnectionType == BposConnectionType.AppToken || mailbox.ConnectionType == BposConnectionType.Modern)
            {
                if (mailboxNameToAppProfileDict.ContainsKey(mailbox.Email))
                {
                    AppProfile appProfile = mailboxNameToAppProfileDict[mailbox.Email];
                    bposInfo.UserAccountInfo.AppId = appProfile.Id;
                    bposInfo.UserAccountInfo.AppClientId = appProfile.AppClientId;
                    bposInfo.UserAccountInfo.AppCertSecret = appProfile.AppCertSecret;
                    //bposInfo.UserAccountInfo.AppCertContent = appProfile.AppCertContent;
                    bposInfo.UserAccountInfo.AADEnvironment = appProfile.AADEnvironment;
                    bposInfo.UserAccountInfo.AppCertSecretContent = appProfile.AppCertSecretContent;
                }
                else
                {
                    logger.Error("Failed to find the app profile dict, mailbox email is {0}", mailbox.Email);
                    bposInfo.TenantGroupId = string.Empty;
                    bposInfo.UserAccountInfo.AppId = string.Empty;
                    bposInfo.UserAccountInfo.AppClientId = string.Empty;
                    bposInfo.UserAccountInfo.AppCertSecret = string.Empty;
                    //bposInfo.UserAccountInfo.AppCertContent = string.Empty;
                    bposInfo.UserAccountInfo.AppCertSecretContent = string.Empty;
                    bposInfo.UserAccountInfo.AADEnvironment = AADEnvironment.None;
                }
            }
        }
    }
}
