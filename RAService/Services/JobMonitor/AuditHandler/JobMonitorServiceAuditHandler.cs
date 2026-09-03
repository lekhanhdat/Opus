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
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Common.Audit;
using AvePoint.RA.Contract.JobMonitor;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.RMWeb.Audit;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.I18N.Core;
using AvePoint.RA.Common;

namespace AvePoint.RA.Service.Services.JobMonitor.AuditHandler
{
    public class JobMonitorServiceAuditHandler : IAfterAuditHandler
    {
        private RALogger logger = RALogger.GetInstance(typeof(JobMonitorServiceAuditHandler));
        private IRMJobExportSettingDao JESDao => PlatformWindsorManager.GetService<IRMJobExportSettingDao>();
        public async Task<RMAuditInfo> CollectAsync(RMAuditInfo info, int model, int category, int action, object[] args, object target, object returnValue)
        {
            if (info == null)
            {
                info = new RMAuditInfo();
            }
            info.Module = (AuditModule)model;
            info.Category = (AuditCategory)category;
            info.Action = (AuditAction)action;
            var result = false;
            try
            {
                if (action == (int)AuditAction.DeleteJobs)
                {
                    info.Object = string.Join(";", args[0] as List<string>);
                    result = Int32.Parse(returnValue.ToString()) > 0;
                }
                else if (action == (int)AuditAction.StopJobs)
                {
                    info.Object = string.Join(";", args[0] as List<string>);
                    result = Int32.Parse(returnValue.ToString()) > 0;
                }
                else if (action == (int)AuditAction.DownloadJobDetails)
                {
                    List<BaseJobDto> jobs = args[0] as List<BaseJobDto>;
                    info.Object = string.Join(";", jobs.Select(c => c.Id));
                    var tempValue = (FileTransferStream)returnValue;
                    result = tempValue != null;
                }
                else if (action == (int)AuditAction.DeleteQueues)
                {
                    result = true;
                }
                else if (action == (int)AuditAction.UpdateJobQueuePriority)
                {
                    result = true;
                }
                else if (action == (int)AuditAction.UpdateJobMonitorPriority)
                {
                    result = true;
                }
                else if (action == (int)AuditAction.RunDownloadJobDetailsJob)
                {
                    var jobId = returnValue.ToString();
                    info.Object = jobId != "-1" ? jobId : string.Empty;
                    result = jobId != "-1";
                }
                else if (action == (int)AuditAction.ConfigDownloadSettings)
                {
                    var setting = JESDao.GetExportSetting();
                    string NewVaule = string.Empty;

                    var useBrowser = setting.ExportSetting == 0;

                    NewVaule = useBrowser ? I18NEntity.GetString("RM_EL_Radio_Browser") : I18NEntity.GetString("RM_EL_Radio_Location") + string.Format(" [{0}]", setting.LocationName);
                    if (info.ModifyContent.Count > 0)
                    {
                        info.ModifyContent[0].NewValue = NewVaule;
                    }
                    result = (returnValue as RAReturnMessage).MessageType == RAMessageType.Successful;
                }
                
            }
            catch (Exception e)
            {
                logger.Error("Job Monitor Audit Error.Message:{0}.", e.ToString());
                info.Object = "";
                result = false;
            }
            info.Status = result ? (int)AuditStatus.Successful : (int)AuditStatus.Failed;
            return info;
        }
    }
}
