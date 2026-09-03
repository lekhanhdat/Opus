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
using Aspose.Pdf.Operators;
using AvePoint.RA.Common;
using AvePoint.RA.Common.Util;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Task;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Model;
using AvePoint.RA.RACommonUtility.Telemetry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AvePoint.RA.Service.RMTasks
{
    public class UpdateSubJobIdToArchiverIndexSubInfoTableTaskExecutor : ITaskExecutor
    {
        private RALogger logger => RALogger.GetInstance(typeof(UpdateSubJobIdToArchiverIndexSubInfoTableTaskExecutor));

        private static ITenantInfoDao TenantInfoDao => PlatformWindsorManager.GetService<ITenantInfoDao>();

        private static IArchiverIndexSubInfoDao ArchiverIndexSubInfoDao => PlatformWindsorManager.GetService<IArchiverIndexSubInfoDao>();

        public async Task ExecutorAsync(TaskBase task)
        {
            try
            {
                logger.Info("Start to execute UpdateSubJobIdToArchiverIndexSubInfoTable task");
                var availableTenants = TenantInfoDao.GetAllAvailableTenantInfo();
                foreach (var tenantInfo in availableTenants)
                {
                    try
                    {
                        logger.Info($"Start UpdateSubJobIdToArchiverIndexSubInfoTable for [{tenantInfo.TenantId}]");
                        TenantUtil.RunUnderTenant(
                            tenantInfo.TenantId, 
                            tenantInfo.RegisterEmail, 
                            UpdateSubJobIdForTenant);
                        logger.Info($"Finish UpdateSubJobIdToArchiverIndexSubInfoTable for [{tenantInfo.TenantId}]");
                    }
                    catch(Exception e)
                    {
                        logger.Error($"Error occurred while UpdateSubJobIdToArchiverIndexSubInfoTable for [{tenantInfo.TenantId}]. {e}");
                        SendTelemetryRecord("DBUpgradeFailed", e.Message);
                    }
                }

                await TelemetryContext.FlushAsync();
            }
            catch(Exception ex)
            {
                logger.Error($"Error occurred while executing UpdateSubJobIdToArchiverIndexSubInfoTable Task, {ex}");
            }
        }

        private void UpdateSubJobIdForTenant()
        {
            var allItems = ArchiverIndexSubInfoDao.GetAllArchiverIndexSubInfoThatNoSubJobIdAsync().GetAwaiter().GetResult();
            if(allItems.Count == 0)
            {
                logger.Info($"Don't need UpdateSubJobIdToArchiverIndexSubInfoTable");
                return;
            }

            TelemetryContext.SendToQueue(TelemetryModule.DBUpgrade, TelemetryEventType.DBUpgradeInfo, new List<object>() { "GetAllArchiverIndexSubInfoThatNoSubJobId", allItems.Count });
            DatabaseUtility.BatchOperation(
                allItems,
                (batchItems) =>
                {
                    try
                    {
                        ArchiverIndexSubInfoDao.BatchUpdateSubJobId(
                            batchItems.ConvertAll(i => new ArchiverIndexSubInfo() { Id = i.Item1, SubJobId = GetSubJobId(i.Item2) }).ToList()
                        );
                    }
                    catch (Exception ex)
                    {
                        logger.Error($"Error occurred while UpdateSubJobIdToArchiverIndexSubInfoTable for items: {string.Join(',', batchItems.Select(i => $"{i.Item1}|{i.Item2}"))}. {ex}");
                        SendTelemetryRecord("BatchUpdateSubJobIdFailed", ex.Message);
                    }
                },
                200
            );
        }

        private void SendTelemetryRecord(string title, string content)
        {
            TelemetryContext.SendToQueue(
                TelemetryModule.DBUpgrade, 
                TelemetryEventType.DBUpgradeInfo, 
                new List<object>() { title, (content?.Length > 500 ? content.Substring(0, 500) : content) });
        }

        private string GetSubJobId(string subSubJobId)
        {
            var splitedJobId = subSubJobId.Split("_");
            if(splitedJobId.Length >= 3)
            {
                return splitedJobId[0] + "_" + splitedJobId[1];
            }
            else
            {
                // Opus end user backup job id start with 'EA', and the the job don't have sub job. so sub job id is also the main job id.
                return splitedJobId[0];
            }
        }
    }
}
