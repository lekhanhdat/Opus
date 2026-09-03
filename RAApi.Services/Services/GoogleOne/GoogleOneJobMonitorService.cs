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
using System;
using AvePoint.GCommon;
using AvePoint.RA.Common;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using Newtonsoft.Json;
using AvePoint.Api.Service.Interface;
using System.Threading.Tasks;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.Contract.JobMonitor;
using System.Collections.Generic;
using System.Linq;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.I18N.Core;
using Newtonsoft.Json.Linq;

namespace AvePoint.Api.Service.Implement
{
    public class GoogleOneJobMonitorService : IGoogleOneJobMonitorService
    {
        private static AveLogger logger = AveLogger.GetInstance(typeof(GoogleOneJobMonitorService));
        public IJobMonitorService JobMonitorService => PlatformWindsorManager.GetService<IJobMonitorService>();
        public IJobMonitorDao JMDao => PlatformWindsorManager.GetService<IJobMonitorDao>();

        private static readonly Dictionary<SourceFlag, string> _contentSourceI18Ns = new()
        {
            { SourceFlag.SharePoint, "RM_JS_Common_ReportType_SharePoint" },
            { SourceFlag.Exchange, "RM_JS_Common_ReportType_Exchange" },
            { SourceFlag.OneDrive, "RM_JS_Common_ReportType_OneDrive" },
            { SourceFlag.Google, "RM_JS_Common_ReportType_GoogleDrive" },
            { SourceFlag.Teams, "RM_JS_Common_ReportType_Teams" },
        };
        private static readonly Dictionary<SourceFlag, string> _contentSourceTabLableI18Ns = new()
        {
            { SourceFlag.SharePoint, "RM_JS_SPS_TabLabel_SP" },
            { SourceFlag.Exchange, "RM_JS_SPS_TabLabel_EXO" },
            { SourceFlag.OneDrive, "RM_JS_SPS_TabLabel_OneDrive" },
            { SourceFlag.Google, "RM_JS_SPS_TabLabel_Google" },
            { SourceFlag.Teams, "RM_JS_SPS_TabLabel_Teams" },
        };

        public async Task<JMJobSummary> GetJobSummaryAsync(string id)
        {
            try
            {
                var opusJobId = GetOpusJobId(id);
                return await JobMonitorService.GetJobSummaryAsync(opusJobId);
            }
            catch (Exception e)
            {
                logger.Error($"GetJobSummary error: {e}");
                throw;
            }
        }

        public async Task<String> GetJobDetailsAsync(JMDetailsQuery queryModel)
        {
            try
            {
                var opusJobId = GetOpusJobId(queryModel.JobID);
                queryModel.JobID = opusJobId;

                var data = await JobMonitorService.GetJobDetailsAsync(queryModel);

                return GetJobDetailByJobType((JobType)queryModel.JobType, data);
            }
            catch (Exception e)
            {
                logger.Error($"GetJobDetails error: {e}");
                throw;
            }
        }

        public async Task<JMJobDetails> GetJobSummaryStatisticsAsync(string id)
        {
            try
            {
                var opusJobId = GetOpusJobId(id);
                return await JobMonitorService.GetSOJobSummaryDetailsAsync(opusJobId);
            }
            catch (Exception e)
            {
                logger.Error($"GetJobSummaryStatistics error: {e}");
                throw;
            }
        }
        private String  GetOpusJobId(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return id;
            }
            var opusJobId = JMDao.GetJobIdByAdditional(id);
            logger.Info($"Opus job id is {opusJobId} by control job id {id}");
            return opusJobId;
        }

        private String GetJobDetailByJobType(JobType jobType, string data)
        {
            List<JobType> specialJobType = [JobType.SyncNodesFromAOS, JobType.Dashboard];
            if (specialJobType.Contains(jobType))
            {
                var parseObject = JsonConvert.DeserializeObject<dynamic>(data);
                if (parseObject.Details is JArray details)
                {
                    object filterList = jobType switch 
                    {
                        JobType.SyncNodesFromAOS => GetFilterContentSource(details.ToObject<List<JMSyncRemoteNodesJobDetails>>()),
                        JobType.Dashboard => GetFilterContentSource(details.ToObject<List<JMDashboardJobDetail>>())
                    };
                    parseObject.Details = JArray.FromObject(filterList);
                }

                return JsonConvert.SerializeObject(parseObject);
            }

            return data;
        }

        private object GetFilterContentSource(List<JMSyncRemoteNodesJobDetails> filterList)
        {
            var googleType = I18NEntity.GetString(_contentSourceI18Ns[SourceFlag.Google]);
            filterList = filterList.Where(n => n.ItemType == googleType).ToList();
            return filterList;
        }

        private object GetFilterContentSource(List<JMDashboardJobDetail> filterList)
        {
            var googleType = I18NEntity.GetString(_contentSourceTabLableI18Ns[SourceFlag.Google]);
            filterList = filterList.Where(n => n.SourceFlag == googleType).ToList();
            return filterList;
        }
    }
}
