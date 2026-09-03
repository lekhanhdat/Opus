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
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Exceptions;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Model;
using AvePoint.RA.RAPhysical.ConfiguePermission.Interface;
using System;
using System.Collections.Generic;

namespace AvePoint.RA.RAPhysical.ConfiguePermission
{
    public class PhysicalPermissionService : IPhysicalPermissionService
    {
        private static readonly RALogger logger = RALogger.GetInstance(typeof(PhysicalPermissionService));
        public IPhysicalPermissionProccessor PhysicalPermissionProccessor { get; set; }
        private IRMSubJobDao SubJobDao => PlatformWindsorManager.GetService<IRMSubJobDao>();
        public IRMScopePermissionDao ScopePermissionDao { get; set; }
        private bool _jobHasException = false;
        private bool _jobHasStopped = false;
        private ScopePermissionJobContextDto _jobContextDto;

        private PermissionOption GetOptions(string jobId)
        {
            RMSubJob subJobWithContext = SubJobDao.GetSubJob(jobId, true);
            var jobContext = SerializerHelper.DeserializeByDataContractSerializer<ScopePermissionJobContextDto>(subJobWithContext.JobContext.Settings);
            _jobContextDto = jobContext;
            return new PermissionOption()
            {
                Scopes = jobContext.Scopes,
                GSJobContext = jobContext.GSJobContextDto,
                JobId = jobId
            };
        }

        public void Run(string jobId)
        {
            try
            {
                var options = GetOptions(jobId);
                if (options.GSJobContext != null)
                {
                    PhysicalPermissionProccessor.ProcessByGlobalSearch(options);
                }
                else {
                    PhysicalPermissionProccessor.Process(options);
                }
            }
            catch (JobStopException)
            {
                logger.Warn("This Job is stopped.");
                _jobHasStopped = true;
            }
            catch (Exception e)
            {
                logger.Error("An error occurred while runnning. ", e.ToString());
                _jobHasException = true;
            }
            finally
            {
                var finalStatus = JobStatus.None;
                if (_jobHasStopped)
                {
                    finalStatus = JobStatus.Stopped;
                }
                else {
                    var hasSuccessNode = PhysicalPermissionProccessor.HasSuccessNode;
                    var hasFailedNode = PhysicalPermissionProccessor.HasErrorNode || _jobHasException;
                    if (hasSuccessNode && hasFailedNode)
                    {
                        finalStatus = JobStatus.FinishWithException;
                    }
                    else if (!hasFailedNode)
                    {
                        finalStatus = JobStatus.Finished;
                    }
                    else if (!hasSuccessNode)
                    {
                        finalStatus = JobStatus.Failed;
                    }
                    else
                    {
                        finalStatus = JobStatus.Skipped;
                    }
                }
                PhysicalPermissionProccessor.ReportManager.SetJobFinished(finalStatus);
                logger.Info($"Job finished.");
            }
        }
    }
}
