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
using AvePoint.RA.Common;
using AvePoint.RA.Common.Util;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.DB.Core;
using AvePoint.RA.DB.Core.Upgrade;
using AvePoint.RA.DB.Dao.Impl;
using AvePoint.RA.DB.DBLocker;
using AvePoint.RA.DB.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.DB.TenantMigrations.Upgrade.Impl
{
    public class RMRuleContainerUpgradgeDao : BaseDao<RMRuleContainer>, IDbUpgradeDao
    {
        private RALogger logger = RALogger.GetInstance(typeof(RMRuleContainerUpgradgeDao));
        private static readonly string RECORD_DEFAULT_CONTAINER_NAME = "RM_RDM_DefaultRuleContainer";
        public async Task UpgradeAsync(RMDbContext context)
        {
            try
            {
                List<RMRule> allRules = new List<RMRule>();
                if (context.RMRuleContainers.Count(c => c.ContainerId == RecordsConstants.RECORD_DEFAULT_CONTAINER_ID) != 0) 
                {
                    logger.Info($"The tenant: [{TenantLocalValue.LogonGroupId}] has init rule container.");
                    return;
                }
                allRules = context.RMRule.ToList();

                bool lockStatus = false;
                var lockerKey = "Rule_Container_Locker_" + TenantLocalValue.LogonGroupId;//根据Tenant去Lock
                try
                {
                    lockStatus = await RMDBlLocker.GetRecordsLockerAsync(lockerKey);
                    logger.Info($"Begin to upgradge default rule container: {TenantLocalValue.LogonGroupId}, lock status:{lockStatus}.");
                    var defaultContainer = context.RMRuleContainers.FirstOrDefault(c => c.ContainerId == RecordsConstants.RECORD_DEFAULT_CONTAINER_ID);
                    if (defaultContainer == null)
                    {
                        logger.Info("Create default rule container.");
                        context.RMRuleContainers.Add(new RMRuleContainer()
                        {
                            ContainerId = RecordsConstants.RECORD_DEFAULT_CONTAINER_ID,
                            Name = RECORD_DEFAULT_CONTAINER_NAME,
                            IsDefault = true,
                            ModifyTime = 0,
                            IsRemoved = false
                        });
                        context.SaveChanges();
                        logger.Info("Saved default rule container.");
                        if (!context.RMRuleContainerMemberships.Any(m => m.ContainerId == RecordsConstants.RECORD_DEFAULT_CONTAINER_ID))
                        {
                            logger.Info($"Need add membership rule count is: {allRules.Count}");
                            foreach (var rule in allRules)
                            {
                                context.RMRuleContainerMemberships.Add(new RMRuleContainerMembership()
                                {
                                    ContainerId = RecordsConstants.RECORD_DEFAULT_CONTAINER_ID,
                                    RuleId = rule.RuleId
                                });
                            }
                        }
                        context.SaveChanges();
                    }

                }
                catch (Exception ex)
                {
                    logger.Error("Error occurred while upgradge default rule container, ERROR:{0}", ex.ToString());
                }
                finally
                {
                    if (lockStatus && !string.IsNullOrEmpty(lockerKey))
                    {
                        await RMDBlLocker.ReleaseRecordsLockerAsync(lockerKey);
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error("error occurred while upgrade rule container:{0}", ex.ToString());
            }
            await RemoveDuplicateDefaultRuleContainerAsync(context);
        }

        public async Task RemoveDuplicateDefaultRuleContainerAsync(RMDbContext context)
        {
            bool lockStatus = false;
            var lockerKey = "Remove_Duplicate_Rule_Container_Locker_" + TenantLocalValue.LogonGroupId; //根据Tenant去Lock
            try
            {
                List<RMRule> allRules = new List<RMRule>();
                var defaultContainerCount = context.RMRuleContainers.Count(c => c.ContainerId == RecordsConstants.RECORD_DEFAULT_CONTAINER_ID);
                if (defaultContainerCount != 2)
                {
                    return;
                }
                lockStatus = await RMDBlLocker.GetRecordsLockerAsync(lockerKey);
                logger.Info($"begin to remove default rule container: {TenantLocalValue.LogonGroupId}, lock status:{lockStatus}.");

                string selectDuplicateRuleContainerSql = $"DELETE FROM {SecurityUtils.SanitizeSQLSchemaName(context.SchemaName)}.RMRuleContainers WHERE IsRemoved = 0 AND Id IN(SELECT MAX(Id) FROM {SecurityUtils.SanitizeSQLSchemaName(context.SchemaName)}.RMRuleContainers WHERE ContainerId ='{RecordsConstants.RECORD_DEFAULT_CONTAINER_ID}' GROUP BY [ContainerId] HAVING COUNT([ContainerId]) > 1)";
                int duplicateRuleContainersCount = context.Database.ExecuteSqlCommand(selectDuplicateRuleContainerSql);
                logger.Info($"need remove duplicate rule container count is: {duplicateRuleContainersCount}, {TenantLocalValue.LogonGroupId}");

                string selectDuplicateMappingSql = $"DELETE FROM {SecurityUtils.SanitizeSQLSchemaName(context.SchemaName)}.RMRuleContainerMemberships WHERE Id IN (SELECT MAX(Id) FROM {SecurityUtils.SanitizeSQLSchemaName(context.SchemaName)}.RMRuleContainerMemberships GROUP BY RuleId HAVING COUNT(RuleId) > 1)";
                int duplicateRuleContainerMembershipsCount = context.Database.ExecuteSqlCommand(selectDuplicateMappingSql);
                logger.Info($"need remove duplicate mapping count is: {duplicateRuleContainerMembershipsCount}, {TenantLocalValue.LogonGroupId}");              
            }
            catch (Exception ex)
            {
                logger.Error("error occurred while remove default rule container, ERROR:{0}", ex.ToString());
            }
            finally
            {
                if (lockStatus && !string.IsNullOrEmpty(lockerKey))
                {
                    await RMDBlLocker.ReleaseRecordsLockerAsync(lockerKey);
                }
            }
        }
    }
}
