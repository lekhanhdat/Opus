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
using AvePoint.GCommon.Utility;
using AvePoint.RA.Common.Util;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Aos;
using AvePoint.RA.Contract.Security;
using AvePoint.RA.DB.Core;
using AvePoint.RA.DB.Model;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.DB.Dao.Impl
{
    public class SecurityProfileDao : BaseDao<RMSecurityProfile>, ISecurityProfileDao
    {
        private const string TABLE_NAME = "RMSecurityProfiles";
        private RALogger logger = RALogger.GetInstance(typeof(SecurityProfileDao));

        public void DeleteProfile(string tenantId)
        {
            try
            {
                SystemDBExecuteWithRetry(context =>
                {
                    string sql = $"DELETE FROM {TABLE_NAME} WHERE TenantId=@TenantId;";
                    context.Database.ExecuteSqlCommand(sql, new SqlParameter("@TenantId", tenantId));
                });
                logger.Info($"Delete scurity profile for tenant: {tenantId}");
            }
            catch(Exception e)
            {
                logger.Error($"Delete scurity profile failed for tenant: {tenantId}, error : {e}");
                throw;
            }
        }

        public RADataEncryptionProfile GetProfile(string TenantId)
        {
            RADataEncryptionProfile rapa = null;
            using (var ctx = RMDBContextManager.GetSystemDBContext())
            {
                var data = ctx.SecurityProfile.FirstOrDefault(e => e.TenantId == TenantId);
                if (data != null)
                {
                    rapa = new RADataEncryptionProfile()
                    {
                        Id = data.Id,
                        AosSecurityProfileId = data.ProfileId,
                        Name = data.Name,
                        TenantId = data.TenantId
                    };
                }
            }
            return rapa;
        }

        public void AddProfile(RADataEncryptionProfile info)
        {
            RMSecurityProfile de = new RMSecurityProfile()
            {
                TenantId = info.TenantId,
                Name = info.Name,
                ProfileId = info.AosSecurityProfileId,
            };
            using (var ctx = RMDBContextManager.GetSystemDBContext())
            {
                var data = ctx.SecurityProfile.Where(d => d.TenantId == info.TenantId).FirstOrDefault();
                if (data != null)
                {
                    data.Name = info.Name;
                    data.ProfileId = info.AosSecurityProfileId;
                    ctx.SaveChanges();
                }
                else
                {
                    ctx.SecurityProfile.Add(de);
                    ctx.SaveChanges();
                }

            }
        }

        public int Update(RADataEncryptionProfile profile)
        {
            return SystemDBExecuteWithRetry(context =>
            {
                var sql =
$@"UPDATE {TABLE_NAME} SET 
  ProfileId=@ProfileId, Name=@Name, JobStatus=@JobStatus, JobUpdateTime=@UpdateTime
WHERE Id=@Id";
                var parameters = new SqlParameter[] {
                    new SqlParameter("@Id", System.Data.SqlDbType.Int) { Value = profile.Id },
                    new SqlParameter("@ProfileId", profile.AosSecurityProfileId),
                    new SqlParameter("@Name", profile.Name),
                    new SqlParameter("@JobStatus", System.Data.SqlDbType.Int) { Value = (int)profile.JobStatus },
                    new SqlParameter("@UpdateTime", System.Data.SqlDbType.BigInt) { Value =  DateTime.UtcNow.Ticks }
                };

                return context.Database.ExecuteSqlCommand(sql, parameters);
            });
        }

        public int UpdateProfileStatus4TimeoutJobs()
        {
            long waitingPeriod = DateTime.UtcNow.AddHours(-2).Ticks;
            long runningPeriod = DateTime.UtcNow.AddMinutes(-10).Ticks;
            string sqlFormat =
$@"UPDATE {SecurityUtils.SanitizeSQLSchemaName(TABLE_NAME)} SET JobStatus={(int)RMSwitchSecurityProfileJobStatus.Done} 
  WHERE JobStatus={{0}} AND JobUpdateTime<@Period;";

            int timeoutJobs = 0;
            SystemDBExecuteWithRetry(context =>
            {
                var updateJobs = context.Database.ExecuteSqlCommand(
                    string.Format(sqlFormat, (int)RMSwitchSecurityProfileJobStatus.Waiting), 
                    new SqlParameter("@Period", System.Data.SqlDbType.BigInt) { Value = waitingPeriod });
                if(updateJobs > 0)
                {
                    logger.Info($"Reset timeout switch profile waiting jobs: {timeoutJobs}");
                }
                timeoutJobs = updateJobs;

                updateJobs = context.Database.ExecuteSqlCommand(
                    string.Format(sqlFormat, (int)RMSwitchSecurityProfileJobStatus.Running),
                    new SqlParameter("@Period", System.Data.SqlDbType.BigInt) { Value = runningPeriod });
                if (updateJobs > 0)
                {
                    logger.Info($"Reset timeout switch profile running jobs: {updateJobs}");
                }
                timeoutJobs += updateJobs;
            });

            return timeoutJobs;
        }

        public int UpdateJobStatus(int id, RMSwitchSecurityProfileJobStatus status)
        {
            return SystemDBExecuteWithRetry(context =>
            {
                string sql = $"UPDATE {SecurityUtils.SanitizeSQLSchemaName(TABLE_NAME)} SET JobStatus={(int)status}, JobUpdateTime={DateTime.UtcNow.Ticks} WHERE Id=@Id;";
                return context.Database.ExecuteSqlCommand(sql, new SqlParameter("@Id", System.Data.SqlDbType.Int) { Value = id });
            });
        }

        public Dictionary<string, int> GetSwitchProfileTenants()
        {
            var notificationTable = RMAOSNotificationDao.TABLE_NAME;

            return SystemDBExecuteWithRetry(context =>
            {
                string sql = 
$@"SELECT DISTINCT ISNULL(p.Id, 0) AS Id, n.TenantId FROM {SecurityUtils.SanitizeSQLSchemaName(notificationTable)} n 
  LEFT JOIN {SecurityUtils.SanitizeSQLSchemaName(TABLE_NAME)} p ON n.TenantId=p.TenantId
WHERE n.[Type]=4 AND (p.Id IS NULL OR p.JobStatus={(int)RMSwitchSecurityProfileJobStatus.Done})";
                var results = context.Database.SqlQuery<RADataEncryptionProfile>(sql);
                var tenants = new Dictionary<string, int>();
                foreach (var item in results)
                {
                    tenants[item.TenantId] = item.Id;
                }
                return tenants;
            });
        }
    }
}
