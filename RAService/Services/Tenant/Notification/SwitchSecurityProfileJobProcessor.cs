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
using AvePoint.RA.Common;
using AvePoint.RA.Common.Aos;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Aos;
using AvePoint.RA.Contract.Aos.Notification;
using AvePoint.RA.Contract.CloudService;
using AvePoint.RA.Contract.Security;
using AvePoint.RA.Contract.Tenant;
using System;
using System.Linq;

namespace AvePoint.RA.Service.Services.Tenant.Notification
{
    public interface ISwitchSecurityProfileJobProcessor
    {
        void Run(JobQueueMessage msg);
    }

    public class SwitchSecurityProfileJobProcessor : ISwitchSecurityProfileJobProcessor
    {
        private RALogger logger = RALogger.GetInstance(typeof(SwitchSecurityProfileJobProcessor));
        private bool hasFailedItem = false;

        private IDataEncryptionService EncryptionService => PlatformWindsorManager.GetService<IDataEncryptionService>();
        private IRMAOSNotificationService NotificationService => PlatformWindsorManager.GetService<IRMAOSNotificationService>();



        public void Run(JobQueueMessage msg)
        {
            var tenantId = TenantLocalValue.LogonGroupId;
            RADataEncryptionProfile profile = null;
            RMAosQueueMessage switchProfileMsg = null;
            try
            {
                profile = EncryptionService.GetProfile(tenantId);
                if(profile == null)
                {
                    logger.Error($"No security profile of the tenant: {tenantId}");
                    return;
                }

                EncryptionService.UpdateProfileJobStatus(profile.Id, RMSwitchSecurityProfileJobStatus.Running);

                try
                {
                    switchProfileMsg = NotificationService.GetSyncAOSSecurityProfileMessage(tenantId);
                    if (switchProfileMsg == null)
                    {
                        logger.Error($"No SyncAOSSecurityProfileMessage of the tenant: {tenantId}");
                        return;
                    }

                    var fromProfileId = switchProfileMsg.SyncAOSSecurityProfileMessage.Content.AppliedFrom;
                    var toProfileId = switchProfileMsg.SyncAOSSecurityProfileMessage.Content.AppliedTo;
                    profile.AosSecurityProfileId = toProfileId;
                    logger.Info($"Switch security profile from {fromProfileId} to {toProfileId}");
                    ReEncryptionData(tenantId, toProfileId);
                }
                finally
                {
                    if (profile != null)
                    {
                        profile.JobStatus = RMSwitchSecurityProfileJobStatus.Done;
                        EncryptionService.UpdateProfileInfo(profile);
                    }
                }
            }
            catch (Exception ex)
            {   
                hasFailedItem = true;
                logger.Error($"Error occurred while syncing data: {ex}");
            }

            UpdateApplyJobStatus(tenantId, switchProfileMsg);
        }

        private void ReEncryptionData(string tenantId, string profileId)
        {
            var allEncryptData = EncryptionService.GetAll();
            var allCount = allEncryptData.Count();
            var updateEncryptData = allEncryptData.Where(d => d.ProfileId != profileId);
            var switchCount = updateEncryptData.Count();
            int currentIndex = 1;
            logger.Info($"Need update encrypt data: {switchCount}, All data: {allCount}");
            foreach (var item in updateEncryptData)
            {
                try
                {
                    logger.Info($"Re-encrypt data: {item.Id}, progress: {currentIndex++} / {switchCount}");
                    var plainText = EncryptionService.Decrypt(item.Content, tenantId, item.ProfileId);
                    item.Content = EncryptionService.Encrypt(plainText, tenantId, profileId);
                    item.ProfileId = profileId;
                    EncryptionService.Update(item);
                }
                catch (Exception ex)
                {
                    hasFailedItem = true;
                    logger.Error($"Error occurred while re-encrypt data: {item.Id}, {ex}");
                }
            }
        }

        private void UpdateApplyJobStatus(string tenantId, RMAosQueueMessage switchProfileMsg)
        {
            if (switchProfileMsg != null)
            {
                try
                {
                    var jobId = switchProfileMsg.SyncAOSSecurityProfileMessage.JobId;
                    logger.Info($"Update aos apply job status. JobId: {jobId}, hasFailedItem: {hasFailedItem}");
                    RMAosApiClient.UpdateApplyJobStatus(jobId, tenantId, hasFailedItem);
                    NotificationService.Delete(switchProfileMsg.QueueMessageId);
                }
                catch (Exception ex)
                {
                    logger.Error($"Error occurred while update aos apply job status. {ex}");
                }
            }
        }

    }
}
