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
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.DB.Dao;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AvePoint.RA.Contract.Aos;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.JobMonitor;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.CloudService;
using AvePoint.RA.Common.Util;
using AvePoint.RA.Common.Aos;
using AvePoint.RA.Contract.Security;
using AvePoint.RA.Common;

namespace AvePoint.RA.Service.Services.Tenant
{
    public class DataEncryptionService : RMServiceBase, IDataEncryptionService
    {
        private RALogger logger = RALogger.GetInstance(typeof(DataEncryptionService));

        private ISecurityProfileDao SecurityProfileDao => PlatformWindsorManager.GetService<ISecurityProfileDao>();
        private IEncryptionDataDao EncryptionDataDao => PlatformWindsorManager.GetService<IEncryptionDataDao>();
        private IJobQueueService JobQueueService => PlatformWindsorManager.GetService<IJobQueueService>();
        private IJobMonitorService JobMonitorService => PlatformWindsorManager.GetService<IJobMonitorService>();



        public void AddProfile(RADataEncryptionProfile info)
        {
            SecurityProfileDao.AddProfile(info);
        }

        public void DeleteProfile(string tenantId)
        {
            SecurityProfileDao.DeleteProfile(tenantId);
        }

        public RADataEncryptionProfile GetProfile(string tenantId)
        {
            return SecurityProfileDao.GetProfile(tenantId);
        }

        public int UpdateProfileJobStatus(int id, RMSwitchSecurityProfileJobStatus status)
        {
            return SecurityProfileDao.UpdateJobStatus(id, status);
        }

        public int UpdateProfileInfo(RADataEncryptionProfile profile)
        {
            return SecurityProfileDao.Update(profile);
        }

        public int UpdateProfileStatus4TimeoutJobs()
        {
            return SecurityProfileDao.UpdateProfileStatus4TimeoutJobs();
        }

        public void CreateSwitchProfileJobs()
        {
            var tenants = SecurityProfileDao.GetSwitchProfileTenants();
            foreach (var item in tenants)
            {
                var tenantId = item.Key;
                var profileId = item.Value;
                try
                {
                    if(profileId > 0)
                    {
                        SecurityProfileDao.UpdateJobStatus(profileId, RMSwitchSecurityProfileJobStatus.Waiting);
                        logger.Info($"Update to waiting status for security profile of tenant: {tenantId}");
                    }
                    else
                    {
                        string aosProfileId = null;
                        if(!CreateProfile(tenantId, out aosProfileId))
                        {
                            continue;
                        }
                    }
                    var jobId = JobMonitorService.GenerateJobId(JobType.SwitchSecurityProfile);
                    TenantUtil.RunUnderTenant(tenantId, string.Empty, () => {
                        JobQueueService.HandleMessage(new JobQueueMessage()
                        {
                            JobId = jobId,
                            JobType = JobType.SwitchSecurityProfile,
                            CommandLine = string.Format("{0} {1}", JobType.SwitchSecurityProfile, jobId),
                        });
                    });
                    logger.Info($"Create switch profile job success. TenantId: {tenantId}, JobId: {jobId}.");
                }
                catch (Exception ex)
                {
                    logger.Error($"Create switch profile job failed. TenantId: {tenantId}. {ex}");
                }
            }

        }
        
        private bool CreateProfile(string tenantId, out string aosProfileId)
        {
            logger.Info($"Create security profile for tenant: {tenantId}");
            var aosProfile = RMAosApiClient.GetCurrentAppliedSecurityProfile(tenantId);
            aosProfileId = aosProfile?.Id;
            if (aosProfile == null)
            {
                logger.Error($"Can't get security profile from AOS for tenant: {tenantId}");
                return false;
            }
            SecurityProfileDao.AddProfile(new RADataEncryptionProfile()
            {
                AosSecurityProfileId = aosProfile.Id,
                Name = aosProfile.Name,
                TenantId = tenantId,
                JobStatus = RMSwitchSecurityProfileJobStatus.Waiting
            });
            logger.Info($"Add security profile of tenant: {tenantId}, AosProfileId: {aosProfile.Id}");
            return true;
        }


        public RMEncryptionDataInfo AddEncryptionDataItem(RMEncryptionDataInfo item)
        {
            return EncryptionDataDao.Add(item);
        }

        public IEnumerable<RMEncryptionDataInfo> GetAll()
        {
            return EncryptionDataDao.GetAll();
        }

        public int Update(RMEncryptionDataInfo data)
        {
            return EncryptionDataDao.Update(data);
        }

        public string Encrypt(string plainText, string tenantId, string profileId = null)
        {
            if(string.IsNullOrEmpty(profileId))
            {
                var profile = GetProfile(tenantId);
                if(profile == null)
                {
                    if(!CreateProfile(tenantId, out profileId))
                    {
                        throw new Exception($"Encrypt failed. Can't get profile.");
                    }
                }
            }

            return RMAosApiClient.Encrypt(plainText, profileId, tenantId);
        }

        public string Decrypt(string plainText, string tenantId, string profileId = null)
        {
            if (string.IsNullOrEmpty(profileId))
            {
                var profile = GetProfile(tenantId);
                if (profile == null)
                {
                    throw new Exception($"Encrypt failed. Can't get profile.");
                }
            }

            return RMAosApiClient.Decrypt(plainText, profileId, tenantId);
        }

    }
}
