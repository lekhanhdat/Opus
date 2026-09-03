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
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Explorer.Dao;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace AvePoint.RA.Service.Services.SecurityContainer.Job
{
    internal abstract class RMContainersSyncBaseProcessor
    {
        protected readonly RALogger logger = RALogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);
        protected readonly IRMSecurityContainerDao _securityContainerDao;
        protected readonly IRMScopeRoleAssignmentDao _scopeRoleAssignmentDao;
        protected readonly IRMSecurityContainerService _containerService;
        protected readonly RMSecurityContainerSyncRecordProcessor _recordProcessor;
        protected readonly IExplorerDao _explorerDao;
        protected readonly IRMReportManager _reportManger;
        protected bool _isSucceed = true;

        protected readonly SourceFlag _sourceFlag;

        public RMContainersSyncBaseProcessor(IRMReportManager reportManger, IRMSecurityContainerService rmSecurityContainerService, IRMScopeRoleAssignmentDao scopeRoleAssignmentDao,
            IRMSecurityContainerDao securityContainerDao, IExplorerDao explorerDao, SourceFlag sourceFlag)
        {
            _reportManger = reportManger;
            _containerService = rmSecurityContainerService;
            _scopeRoleAssignmentDao = scopeRoleAssignmentDao;
            _explorerDao = explorerDao;
            _securityContainerDao = securityContainerDao;
            _recordProcessor = new RMSecurityContainerSyncRecordProcessor(_explorerDao, _reportManger);
            _sourceFlag = sourceFlag;
        }

        /// <summary>
        /// 可能有些cotainer已经在AOS中被删除，但是在Opus的group中还存在，需要将这些container从group中移除
        /// </summary>
        /// <param name="treeNodes"></param>
        /// <param name="removedContainerIds"></param>
        protected void ProcessDeletedContainers(List<string> removedContainerIds)
        {
            if (removedContainerIds.Count == 0) return;
            var names = _securityContainerDao.GetByLambda(o => removedContainerIds.Contains(o.Id)).Select(o => o.Name);
            var removedContainerNames = string.Join(" , ", names);

            var scopeIds = removedContainerIds.Select(o => new Guid(o)).ToList();
            logger.Info($"Try to remove the following deleted {_sourceFlag} containers from groups: {removedContainerNames}");
            try
            {
                _scopeRoleAssignmentDao.RemoveContainers(scopeIds);
                AddJobDetails4RemovedContainers(names);
            }
            catch (Exception e)
            {
                logger.Warn($"Error occurred while remove the deleted {_sourceFlag} containers from groups. error : {e.ToString()}");
            }
        }

        /// <summary>
        /// 把container中可能已经被删除的sub container标记为待删除，等待后续处理
        /// </summary>
        /// <param name="container"></param>
        /// <param name="browseredSubContainers"></param>
        protected void MarkDeletedSubContainers(string container, List<string> browseredSubContainers)
        {
            var existSubContainers = _containerService.GetSubContainers(container).Select(o => o.Id).ToList();
            var except = existSubContainers.Except(browseredSubContainers);
            MarkAsMaybeDeleted(except);
        }

        private void MarkAsMaybeDeleted(IEnumerable<string> exceptIds)
        {
            if (exceptIds.Count() > 0)
            {
                _securityContainerDao.UpdateStatus(exceptIds, RMSecurityContainerStaus.MaybeDeleted);
            }
        }

        /// <summary>
        /// update record in cosmos db
        /// </summary>
        /// <param name="containerName"></param>
        /// <param name="containerId"></param>
        /// <param name="subContainerId"></param>
        /// <param name="isClearContainer"></param>
        protected void UpdateRecords(string containerName, string subContainerName, string containerId, string subContainerId, bool isClearContainer = false)
        {
            var scopeId = Guid.Parse(subContainerId);
            if (!_recordProcessor.Process(containerName, subContainerName, containerId, scopeId, isClearContainer)) _isSucceed = false;
        }

        protected abstract IList<RMSecurityContainerDto> GetRealDeletedSubContainers();
        public abstract System.Threading.Tasks.Task<bool> ProcessAsync();

        protected void ProcessDeletedSubContainers()
        {
            var subContainers = GetRealDeletedSubContainers();
            if (subContainers.Count == 0) return;

            var parentIds = subContainers.Select(o => o.Parent).Distinct().ToList();
            var containers = _securityContainerDao.GetByLambda(s => parentIds.Contains(s.Id));
            var groups = subContainers.GroupBy(o => o.Parent);
            foreach (var parent in groups)
            {
                var container = containers.First(o => o.Id == parent.Key);
                foreach (var subContainer in parent)
                {
                    try
                    {
                        logger.Info($"Try to process the deleted {_sourceFlag} sub  container of container {container?.Name}, sub {_sourceFlag} container : {subContainer.Name}");
                        UpdateRecords(container?.Name, subContainer?.Name, container?.Id, subContainer?.Id, true);
                        if (!string.IsNullOrEmpty(subContainer.ObjectId))
                        {
                            UpdateRecords(container?.Name, subContainer.Name, container?.Id, RemoveArchiverKeyWords(subContainer.ObjectId), true);
                        }
                        _securityContainerDao.UpdateStatus(new List<string>() { subContainer.Id }, RMSecurityContainerStaus.InActive);
                        AddJobDetail4Removed(container?.Name, subContainer.Name);
                    }
                    catch (Exception e)
                    {
                        logger.Warn($"Error occurred while process the deleted {_sourceFlag} sub containers, error : {e.ToString()}");
                        _isSucceed = false;
                    }
                }
            }
        }

        private void AddJobDetails4RemovedContainers(IEnumerable<string> containers)
        {
            foreach(var container in containers)
            {
                AddJobDetail4Removed(container, container);
            }
        }

        private void AddJobDetail4Removed(string container, string subContainer)
        {
            AddJobDetail(container, subContainer, JobDetailsStatus.Successful, "Deleted");
        }

        protected void AddJobDetail4Updated(string container, string subContainer, bool isUpdated)
        {
            var detailStatus = isUpdated ? JobDetailsStatus.Successful : JobDetailsStatus.Skipped;
            AddJobDetail(container, subContainer, detailStatus);
        }

        protected void AddJobDetail(string container, string subContainer, JobDetailsStatus status = JobDetailsStatus.Successful, string exceptionMsg = null)
        {
            _reportManger.SendJobDetail(new JMSyncSecurityContainerJobDetails()
            {
                ObjectName = subContainer,
                Container = container,
                FullPath = string.Empty,
                Status = status,
                Comment = exceptionMsg,
            });
        }

        protected static string RemoveArchiverKeyWords(string source)
        {
            if (!string.IsNullOrEmpty(source) && source.IndexOf("(Archive)") != -1)
            {
                return source.Substring(0, source.IndexOf("(Archive)"));
            }
            return source;
        }
    }
}
