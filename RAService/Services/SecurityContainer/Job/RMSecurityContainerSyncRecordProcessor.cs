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
using AvePoint.RA.Common.Report;
using AvePoint.RA.Common.Util;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using AvePoint.RA.DB.Explorer.Dao;
using AvePoint.RA.DB.Explorer.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace AvePoint.RA.Service.Services.SecurityContainer.Job
{
    internal class RMSecurityContainerSyncRecordProcessor
    {
        private RALogger logger = RALogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);
        private IExplorerDao _explorerDao;
        private IRMReportManager _reportManger;
        private bool _isSucceed = true;

        public RMSecurityContainerSyncRecordProcessor(IExplorerDao explorerDao, IRMReportManager reportManger)
        {
            _explorerDao = explorerDao;
            _reportManger = reportManger;
        }

        /// <summary>
        /// update container id to record.
        /// return true if there is no errors, otherwise, return false
        /// </summary>
        /// <param name="containerName"></param>
        /// <param name="containerId"></param>
        /// <param name="scopeId"></param>
        /// <returns></returns>
        public bool Process(string containerName, string subContainerName, string containerId, Guid scopeId, bool isClearContainer = false)
        {
            if (isClearContainer) return ProcessDeletedScope(containerName,subContainerName, scopeId);

            logger.Info($"Start to update container id field of record for container : {containerName}");
            var continuation = "";
            do
            {
                using (new CheckJobStopScope())
                {
                    var result = _explorerDao.GetRecordsByContainer(scopeId,containerId, continuation,10000);
                    var count = result.Item1.Count();
                    if (count == 0) return _isSucceed;

                    ProcessItem(count, result, containerName, subContainerName, containerId);

                    continuation = result.Item2;
                }
            }
            while (!string.IsNullOrEmpty(continuation));

            return _isSucceed;
        }

        private void  ProcessItem(int count, Tuple<IEnumerable<Record>, string> result, string containerName, string subContainerName, string containerId)
        {
            _reportManger.IncreaseBase(count);

            foreach (var record in result.Item1)
            {
                ProcessRecord(record, containerName, subContainerName, containerId);
            }
        }

        /// <summary>
        /// clear the contianer id field of records
        /// </summary>
        /// <param name="containerName"></param>
        /// <param name="scopeId"></param>
        /// <returns></returns>
        private bool ProcessDeletedScope(string containerName, string subContainerName, Guid scopeId)
        {
            logger.Info($"Start to delete container id field of record for container : {containerName}");
            var continuation = "";
            do
            {
                using (new CheckJobStopScope())
                {
                    var result = _explorerDao.QueryByPage(s => s.ScopeId == scopeId && s.DirPath != null, 10000, continuation);
                    var count = result.Item1.Count();
                    if (count == 0) return _isSucceed;

                    ProcessItem(count, result, containerName, subContainerName, "");

                    continuation = result.Item2;
                }
            }
            while (!string.IsNullOrEmpty(continuation));

            return _isSucceed;
        }

        private void ProcessRecord(Record record, string containerName, string subContainerName, string containerId)
        {
            try
            {
                record.ContainerId = containerId;
                _explorerDao.Replace(record);
                logger.Info($"Update container id '{containerId}' to record succussfully. container name: {containerName}, record id: {record.Id}");
                _reportManger.Increase();
                AddJobDetail(record, containerName, subContainerName);
            }
            catch (Exception e)
            {
                logger.Error($"Update container id '{containerId}' to record failed. container name: {containerName}, record id: {record.Id}, error: {e.ToString()}");
                _isSucceed = false;
                AddJobDetail(record, containerName, subContainerName, JobDetailsStatus.Failed, e.Message);
            }
        }

        private void AddJobDetail(Record record, string containerName, string subContainerName, JobDetailsStatus status = JobDetailsStatus.Successful, string exceptionMsg = null)
        {
            var fullPath = string.IsNullOrEmpty(subContainerName) ? record.DirPath : WebUtil.MakeFullUrl(subContainerName, record.DirPath);
            _reportManger.SendJobDetail(new JMSyncSecurityContainerJobDetails()
            {
                ObjectName = record.LeafName,
                Container = containerName,
                FullPath = fullPath,
                Status = status,
                Comment = exceptionMsg,
            });
        }

    }
}
