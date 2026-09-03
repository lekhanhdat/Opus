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
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.TenantUpgrade;
using AvePoint.RA.DB.Core;
using AvePoint.RA.DB.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.DB.Dao.Impl
{
    public class RMTenantUpgradeInfoDao : IRMTenantUpgradeInfoDao
    {

        private RALogger logger = RALogger.GetInstance(typeof(RMTenantUpgradeInfoDao));

        public RMTenantUpgradeInfo Get(string tenantId)
        {
            return Get(tenantId, false);
        }

        public RMTenantUpgradeInfo Get(string tenantId, bool ifNotExistCreateIt)
        {
            using(var context = RMDBContextManager.GetSystemDBContext())
            {
                var exist = context.TenantUpgradeInfo.Any(item => item.TenantId == tenantId);
                if(!exist && ifNotExistCreateIt)
                {
                    var upgradeInfo = new RMTenantUpgradeInfo
                    {
                        Id = Guid.NewGuid(),
                        TenantId = tenantId,
                        FinishedFeature = RMUpgradeFeature.General,
                        SucceedFeature = RMUpgradeFeature.General,
                        FailedFeature = RMUpgradeFeature.General,
                        HasExceptionFeature = RMUpgradeFeature.General,
                        UpgradeStartTime = 0,
                        UpgradeFinishTime = 0,
                    };
                    context.TenantUpgradeInfo.Add(upgradeInfo);
                    context.SaveChanges();
                }

                return context.TenantUpgradeInfo.FirstOrDefault(item => item.TenantId == tenantId);
            }
        }

        public bool Update(RMTenantUpgradeInfo upgradeInfo)
        {
            using(var context = RMDBContextManager.GetSystemDBContext())
            {
                var exist = context.TenantUpgradeInfo.Any(item => item.Id == upgradeInfo.Id);
                if(!exist)
                {
                    throw new ArgumentException($"Can't find tenant upgrade info by Id: [{upgradeInfo.Id}].");
                }

                var existUpgradeInfo = context.TenantUpgradeInfo.First(item => item.Id == upgradeInfo.Id);
                existUpgradeInfo.FinishedFeature = upgradeInfo.FinishedFeature;
                existUpgradeInfo.SucceedFeature = upgradeInfo.SucceedFeature;
                existUpgradeInfo.FailedFeature = upgradeInfo.FailedFeature;
                existUpgradeInfo.HasExceptionFeature = upgradeInfo.HasExceptionFeature;
                existUpgradeInfo.UpgradeStartTime = upgradeInfo.UpgradeStartTime;
                existUpgradeInfo.UpgradeFinishTime = upgradeInfo.UpgradeFinishTime;
                existUpgradeInfo.IsUpgrading = upgradeInfo.IsUpgrading;
                existUpgradeInfo.Content = upgradeInfo.Content;
                var effectCount = context.SaveChanges();
                return effectCount > 0;
            }
        }

        public List<RMTenantUpgradeInfo> GetAllTenantUpgradeInfo()
        {
            using(var context = RMDBContextManager.GetSystemDBContext())
            {
                var query = from upgrade in context.TenantUpgradeInfo
                join tenant in context.TenantInfo
                on upgrade.TenantId equals tenant.Id
                where tenant.Status == 0
                select upgrade;
                return query.ToList();
            }
        }

        public RMTenantUpgradeInfo Create(string tenantId, RMUpgradeFeature option)
        {
            using(var context = RMDBContextManager.GetSystemDBContext())
            {
                var upgradeInfo = new RMTenantUpgradeInfo
                {
                    Id = Guid.NewGuid(),
                    TenantId = tenantId,
                    FinishedFeature = option,
                    SucceedFeature = option,
                    FailedFeature = RMUpgradeFeature.General,
                    HasExceptionFeature = RMUpgradeFeature.General,
                    UpgradeStartTime = 0,
                    UpgradeFinishTime = 0,
                };
                context.TenantUpgradeInfo.Add(upgradeInfo);
                context.SaveChanges();
                return upgradeInfo;
            }
        }

        public RMTenantUpgradeInfo UpdateTenantUpgradeInfoToRunning(string tenantId)
        {
            using(var context = RMDBContextManager.GetSystemDBContext())
            {
                var upgradeInfo = Get(tenantId, true);
                upgradeInfo.IsUpgrading = true;
                upgradeInfo.UpgradeStartTime = DateTime.UtcNow.Ticks;
                Update(upgradeInfo);
                return upgradeInfo;
            }
        }

        public void Delete(string tenantId)
        {
            using var ctx = RMDBContextManager.GetSystemDBContext();
            try
            {
                var tinfo = ctx.TenantUpgradeInfo.Where(t => t.TenantId.Equals(tenantId)).FirstOrDefault();
                if (tinfo != null)
                {
                    ctx.TenantUpgradeInfo.Remove(tinfo);
                    ctx.SaveChanges();
                }
                logger.Info($"Success to delete tenant upgrade info : {tenantId}");
            }
            catch(Exception e)
            {
                logger.Info($"Delete tenant upgrade info failed, tenant id : {tenantId}, error :  {e}");
            }
        }
    }
}
