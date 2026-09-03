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
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.Contract.TenantUpgrade;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Model;
using Microsoft.SharePoint.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.Service.Services.Tenant.Upgrade
{
    public class RMTenantUpgradeHelper
    {
        private static readonly RALogger s_logger = RALogger.GetInstance(typeof(RMTenantUpgradeHelper));

        private static readonly IRMTenantUpgradeInfoDao s_tenantUpgradeInfoDao = PlatformWindsorManager.GetService<IRMTenantUpgradeInfoDao>();

        private static readonly Dictionary<string, RMTenantUpgradeInfo> s_tenantUpgradeInfoes;

        private static readonly List<RMUpgraderDefinition> s_immediatelyUpgraderDefinitions;

        private static readonly List<RMUpgraderDefinition> s_delayUpgraderDefinitions;

        static RMTenantUpgradeHelper()
        {
            s_tenantUpgradeInfoes = InitTenantUpgradeInfoes();
            s_immediatelyUpgraderDefinitions = InitUpgraderDefinitions<IRMTenantImmediatelyUpgrader>();
            s_delayUpgraderDefinitions = InitUpgraderDefinitions<IRMTenantDelayUpgrader>();
        }

        public static bool NeedRunDelayUpgradeJob(string tenantId)
        {
            return GetDelayUpgraderDefinitions(tenantId).Any();
        }

        public static IEnumerable<RMUpgraderDefinition> GetImmediatelyUpgraderDefinitions(string tenantId)
        {
            return GetUpgraderDefinitions(tenantId, s_immediatelyUpgraderDefinitions);
        }

        public static IEnumerable<RMUpgraderDefinition> GetDelayUpgraderDefinitions(string tenantId)
        {
            return GetUpgraderDefinitions(tenantId, s_delayUpgraderDefinitions);
        }

        public static void SetToUpgrading(string tenantId)
        {
            var tenantUpgradeInfo = s_tenantUpgradeInfoDao.UpdateTenantUpgradeInfoToRunning(tenantId);
            if(!s_tenantUpgradeInfoes.ContainsKey(tenantId))
            {
                s_tenantUpgradeInfoes.TryAdd(tenantId, tenantUpgradeInfo);
            }
        }

        public static bool IsNeedUpgrade(string tenantId, RMUpgradeFeature feature)
        {
            var tenantUpgradeInfo = s_tenantUpgradeInfoDao.Get(tenantId, true);
            if (!s_tenantUpgradeInfoes.ContainsKey(tenantId))
            {
                s_tenantUpgradeInfoes[tenantId] = tenantUpgradeInfo;
            }
            if (tenantUpgradeInfo == null)
            {
                return false;
            }
            if (!tenantUpgradeInfo.FinishedFeature.HasFlag(feature))
            {
                return true;
            }

            if (!tenantUpgradeInfo.SucceedFeature.HasFlag(feature))
            {
                return true;
            }

            return false;
        }

        public static void SetToFinish(string tenantId, RMUpgradeFeature feature, RMUpgradeStatus status)
        {
            if (s_tenantUpgradeInfoes.TryGetValue(tenantId, out RMTenantUpgradeInfo value))
            {
                var tenantUpgradeInfo = value;

                tenantUpgradeInfo.UpgradeFinishTime = DateTime.UtcNow.Ticks;
                tenantUpgradeInfo.IsUpgrading = false;
                tenantUpgradeInfo.FinishedFeature |= feature;
                if (status == RMUpgradeStatus.Success)
                {
                    tenantUpgradeInfo.SucceedFeature |= feature;
                    tenantUpgradeInfo.HasExceptionFeature = ExecuteXOROnConditionTarget(tenantUpgradeInfo.HasExceptionFeature, feature);
                    tenantUpgradeInfo.FailedFeature = ExecuteXOROnConditionTarget(tenantUpgradeInfo.FailedFeature, feature);
                }
                else if (status == RMUpgradeStatus.Exception)
                {
                    tenantUpgradeInfo.SucceedFeature = ExecuteXOROnConditionTarget(tenantUpgradeInfo.SucceedFeature, feature);
                    tenantUpgradeInfo.HasExceptionFeature |= feature;
                    tenantUpgradeInfo.FailedFeature = ExecuteXOROnConditionTarget(tenantUpgradeInfo.FailedFeature, feature);
                }
                else
                {
                    tenantUpgradeInfo.SucceedFeature = ExecuteXOROnConditionTarget(tenantUpgradeInfo.SucceedFeature, feature);
                    tenantUpgradeInfo.HasExceptionFeature = ExecuteXOROnConditionTarget(tenantUpgradeInfo.HasExceptionFeature, feature);
                    tenantUpgradeInfo.FailedFeature |= feature;
                }

                s_tenantUpgradeInfoDao.Update(tenantUpgradeInfo);
            }
        }


        public static RMUpgradeFeature ExecuteXOROnConditionTarget(RMUpgradeFeature source, RMUpgradeFeature target)
        {
            if (source.HasFlag(target))
            {
                return source ^= target;
            }
            return source;
        }

        private static Dictionary<string, RMTenantUpgradeInfo> InitTenantUpgradeInfoes()
        {
            try
            {
                var infoes = s_tenantUpgradeInfoDao.GetAllTenantUpgradeInfo();
                return infoes.DistinctBy(info => info.TenantId).Where(item => !string.IsNullOrEmpty(item.TenantId)).ToDictionary(item => item.TenantId, item => item);
            }
            catch (Exception e)
            {
                s_logger.Error($"An error occurred while init tenant upgrade infoes. Error: {e}");
                throw;
            }
        }

        private static List<RMUpgraderDefinition> InitUpgraderDefinitions<T>()
        {
            try
            {
                var res = new List<RMUpgraderDefinition>();

                var compilerType = typeof(T);
                var assembly = Assembly.GetAssembly(compilerType);

                foreach (var type in assembly.GetTypes())
                {
                    if (type.IsInterface) continue;
                    if (type.GetInterfaces().Contains(compilerType))
                    {
                        var attribute = type.GetCustomAttribute<RMUpgradeConfigAttribute>();
                        var instance = Activator.CreateInstance(type) as IRMTenantUpgrader;
                        res.Add(new RMUpgraderDefinition
                        {
                            Feature = attribute.Feature,
                            Version = attribute.Version,
                            ExecutionMode = attribute.ExecutionMode,
                            UnsuccessfulNeedRetry = attribute.UnsuccessfulNeedRetry,
                            RetryTimes = attribute.RetryTimes,
                            Upgrader = instance
                        });
                    }
                }

                return res;
            }
            catch (Exception e)
            {
                s_logger.Error($"An error occurred while init [{typeof(T)}] upgrader definitions. Error: {e}");
                throw;
            }
        }

        private static IEnumerable<RMUpgraderDefinition> GetUpgraderDefinitions(string tenantId, List<RMUpgraderDefinition> upgraderDefinitions)
        {
            if (s_tenantUpgradeInfoes.ContainsKey(tenantId))
            {
                var tenantUpgradeInfo = s_tenantUpgradeInfoes[tenantId];
                foreach (var upgraderDefinition in upgraderDefinitions)
                {
                    if (upgraderDefinition.ExecutionMode == RMUpgradeExecutionMode.Always)
                    {
                        yield return upgraderDefinition;
                        continue;
                    }

                    if (!tenantUpgradeInfo.FinishedFeature.HasFlag(upgraderDefinition.Feature))
                    {
                        yield return upgraderDefinition;
                        continue;
                    }

                    if (upgraderDefinition.UnsuccessfulNeedRetry
                       && (tenantUpgradeInfo.HasExceptionFeature.HasFlag(upgraderDefinition.Feature)
                       || tenantUpgradeInfo.FailedFeature.HasFlag(upgraderDefinition.Feature)))
                    {
                        yield return upgraderDefinition;
                    }
                }
            }
        }
    }
}
