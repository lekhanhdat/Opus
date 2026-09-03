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
using AvePoint.GCommon.Contract.Server.ControlPanel.Office365;
using AvePoint.GCommon.Utility;
using AvePoint.RA.Common;
using AvePoint.RA.Common.Util;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Aos.Notification;
using AvePoint.RA.Contract.Tenant;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.Service.Services.Tenant.Notification.Excutor
{
    class SyncServiceAccountExecutor : IAosQueueMessageExecutor
    {
        private static RALogger logger = RALogger.GetInstance(typeof(SyncServiceAccountExecutor));

        public IRMRemoteO365AccountService O365AccountService => PlatformWindsorManager.GetService<IRMRemoteO365AccountService>();

        public bool Execute(RMAosQueueMessage queueMessage)
        {
            try
            {
                var tenantGroupId = queueMessage.TenantGroupId;
                logger.Debug("Sync service account tenantGroupId is {0}.", tenantGroupId);
                TenantUtil.RunUnderTenant(tenantGroupId, null, () =>
                {
                    try
                    {
                        logger.Info("Run under group {0}.", tenantGroupId);
                        SyncServiceAccountMessage syncMessage = queueMessage.SyncServiceAccountMessage;
                        var content = syncMessage.Content;
                        if (content == null)
                        {
                            logger.Warn("The sync service account is null.");
                            return;
                        }
                        logger.Info("Sync o365 service account info, user name: {0}, tenant id: {1}, tenant name: {2}, admin url: {3}.", content.UserName, content.TenantId, content.TenantName, content.AdminUrl);
                        var account = ConvertServiceAccount(content);
                        if (!O365AccountService.CheckServiceAccountExisted(account.UserName))
                        {
                            logger.Info("Create service account id: {0}.", account?.Id);
                            O365AccountService.CreateServiceAccount(account);
                        }
                        else
                        {
                            logger.Info("Update service account id: {0}.", account.Id);
                            O365AccountService.UpdateServiceAccount(account);
                        }
                    }
                    catch (Exception ex)
                    {
                        logger.Error("Run sync service account under group failed: {0}.", ex.ToString());
                        return;
                    }
                });
                logger.Debug("Execute sync service account complete");
                return true;
            }
            catch (Exception ex)
            {
                logger.Error("Execute sync service account task error.", ex.ToString());
                return false;
            }
            finally
            {
                logger.Debug("Complete sync service account message.");
            }
        }

        public static O365ServiceAccountDto ConvertServiceAccount(ServiceAccountMessage message)
        {
            return new O365ServiceAccountDto()
            {
                Id = HashCodeHelper.ToMD5HashCode(message.UserName),
                UserName = message.UserName,
                Password = string.Empty,
                TenantId = message.TenantId,
                TenantName = message.TenantName,
                AdminUrl = message.AdminUrl
            };
        }
    }
}
