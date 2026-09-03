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
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.DB.Explorer.Model;
using AvePoint.RA.RACommonUtility.Email.Sender;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.RACommonUtility.Email.Sender.Storage;
using RAManualApproval.Executors;
using RAManualApproval.Upgrade;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using AvePoint.RA.RACommonUtility.Email.Sender.Middleware;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.DB.Dao.Impl;
using AvePoint.RA.Service.Services.ManualApproval;
using AvePoint.RA.DB.Dao;

namespace RAManualApproval
{
    public class ManualApprovalProcessor
    {

        private static readonly RALogger s_logger = RALogger.GetInstance(typeof(ManualApprovalProcessor));

        private static readonly List<ManualApprovalExecutor> s_executors = new List<ManualApprovalExecutor>();

        private static readonly RMEmailSender s_emailSender = new(new RMEmailMemoryStorage(new RMEMailStorageManualMiddleware()));

        private static readonly ITenantService TenantService = PlatformWindsorManager.GetService<ITenantService>();
        private static IRMKeyValueDao RMKeyValueDao => PlatformWindsorManager.GetService<IRMKeyValueDao>();

        static ManualApprovalProcessor()
        {
            try
            {
                var executorType = typeof(ManualApprovalExecutor);
                var assembly = Assembly.GetAssembly(executorType);
                var isNewOpusTenant = TenantService.IsNewOpusTenant();
                s_logger.Info($"IsNewOpusTenant is [{isNewOpusTenant}]");
                foreach (var type in assembly.GetTypes())
                {
                    if (type.IsAbstract || type.IsInterface) continue;
                    if (type.BaseType?.Name == executorType.Name)
                    {
                        try
                        {
                            if (isNewOpusTenant)
                            {
                                var foundAttr = false;
                                var attrs = type.GetCustomAttributes();
                                if (attrs == null) continue;
                                foreach (var attr in attrs)
                                {
                                    if (attr is NewOpusManualApprovalAttribute a)
                                    {
                                        s_logger.Info($"This class [{type}] has the attribute [{a.GetType().Name}]");
                                        foundAttr = true;
                                        break;
                                    }
                                }
                                if(foundAttr) continue;
                            }
                        }
                        catch (System.Exception e)
                        {
                            s_logger.Warn($"Filter by attr error {e}");
                        }
                        var instance = Activator.CreateInstance(type, s_emailSender) as ManualApprovalExecutor;
                        if (instance == null || instance.Flag == SourceFlag.Box) continue; // currently not support manual approval job for Box content source old logic account
                        s_executors.Add(instance);
                    }
                }
                s_executors.Sort((a, b) => { return a.Flag.CompareTo(b.Flag); });
                s_logger.Info($"Successful initialize manual approval exexutors. exexutors count: {s_executors.Count}");
            }
            catch (Exception e)
            {
                s_logger.Error($"An error occurred while initialize manual approval exexutors. Error: {e}");
            }
        }

        public static async Task RunAsync(string jobId)
        {
            try
            {
                ManualApprovalJobManager.Init(jobId);
                s_logger.Info($"Successful init manual approval job manager.");
                await s_executors.ForEachAsync(async executor =>
                {
                    try
                    {
                        using (new PerformanceScope($"Execute [{executor.Flag}] manual approval"))
                        {
                            await executor.ExecuteAsync();
                        }
                    }
                    catch(Exception ex)
                    {
                        s_logger.Warn(ex.ToString());
                    }
                });

                await s_emailSender.SendAsync();
                ManualApprovalDataSyncManager.WaitComplete();
                ManualApprovalJobManager.SetJobFinished();
                PerformanceMonitor.WritePerformanceResult();
            }
            catch(Exception e)
            {
                s_logger.Error($"An error occurred while process manual approval job. Error: {e}");
                ManualApprovalJobManager.SetJobFailed(e.Message);
            }
        }
        private static bool HasNewAgentTag()
        {
            var key = RMKeyValueDao.GetValueByKey("FSInsertManualToCosmosByDisposal");
            if (key == null)
            {
                s_logger.Info("not exist FSInsertManualToCosmosByDisposal");
                return false;
            }
            else
            {
                if (bool.TryParse(key?.Value, out bool result) && result)
                {
                    s_logger.Info("will not process fs manual because FSInsertManualToCosmosByDisposal is true.");
                    return true;
                }
                else
                {
                    s_logger.Info("still process fs manual by timer.");
                    return false;
                }
            }
        }
    }
}
