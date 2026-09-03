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
using AvePoint.RA.Common.Aos;
using AvePoint.RA.Common.Global.Utils;
using AvePoint.RA.Common.Report;
using AvePoint.RA.Contract.DocAve;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.Object.Extension;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Explorer.Dao;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AvePoint.RA.Service.Services.SecurityContainer.Job
{
    internal class RMEXOContainersSyncProcessor : RMContainersSyncBaseProcessor
    {
        //private RALogger logger = RALogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);

        //private IRMReportManager _reportManger;
        private ISPSettingTreeService _settingTreeService;
        //private IRMSecurityContainerService _containerService;
        //private IExplorerDao _explorerDao;
        private IList<string> _containerInGroups;

        //private RMSecurityContainerSyncRecordProcessor _recordProcessor;
        //private IRMSecurityContainerDao _securityContainerDao;
        //private IRMScopeRoleAssignmentDao _scopeRoleAssignmentDao;

        //private bool _isSucceed = true;
        private List<string> _o365TenantIds = new List<string>();

        public RMEXOContainersSyncProcessor(IRMReportManager reportManger, ISPSettingTreeService spSettingTreeService, 
            IRMSecurityContainerService rmSecurityContainerService, IRMScopeRoleAssignmentDao scopeRoleAssignmentDao,
            IRMSecurityContainerDao securityContainerDao, IExplorerDao explorerDao, IList<string> containerInGroups) 
            : base(reportManger, rmSecurityContainerService, scopeRoleAssignmentDao, securityContainerDao, explorerDao, SourceFlag.Exchange)
        {
            //_reportManger = reportManger;
            _settingTreeService = spSettingTreeService;
            //_containerService = rmSecurityContainerService;
            //_scopeRoleAssignmentDao = scopeRoleAssignmentDao;
            //_explorerDao = explorerDao;
            //_securityContainerDao = securityContainerDao;
            _containerInGroups = containerInGroups;
            //_recordProcessor = new RMSecurityContainerSyncRecordProcessor(_explorerDao, _reportManger);
        }

        /// <summary>
        /// update EXO data.
        /// return false if there is any errors.
        /// </summary>
        /// <returns></returns>
        public override async Task<bool> ProcessAsync()
        {
            try
            {
                logger.Info("Start to sync EXO data.");
                var exchangeRoot = _settingTreeService.LoadExchangeRoot()[0];
                if (exchangeRoot == null || exchangeRoot.Id.Equals(System.Guid.Empty))
                {
                    var exceptionMsg = "exchage farm node is null.";
                    logger.Warn(exceptionMsg);
                    AddJobDetail(string.Empty, string.Empty, JobDetailsStatus.Failed, exceptionMsg);

                    return false;
                }

                await ProcessRootAsync(exchangeRoot);

                logger.Info("Sync EXO data end.");
            }
            catch(Exception e)
            {
                logger.Error($"Error occurred while sync EXO data. Error: {e.ToString()}");
                AddJobDetail(string.Empty, string.Empty, JobDetailsStatus.Failed, e.Message);
                _isSucceed = false;
            }

            return _isSucceed;
        }

        /// <summary>
        /// 把container中可能已经被删除的sub container标记为待删除，等待后续处理
        /// </summary>
        /// <param name="container"></param>
        /// <param name="browseredSubContainers"></param>
        //private void MarkDeletedSubContainers(string container, List<string> browseredSubContainers)
        //{
        //    var existSubContainers = _containerService.GetSubContainers(container).Select(o => o.Id).ToList();
        //    var except = existSubContainers.Except(browseredSubContainers);
        //    if (except.Count() > 0)
        //    {
        //        _securityContainerDao.UpdateStatus(except, RMSecurityContainerStaus.MaybeDeleted);
        //    }
        //}

        protected override IList<RMSecurityContainerDto> GetRealDeletedSubContainers()
        {
            var existSubContainers = _securityContainerDao.GetByLambda(s => s.SourceFlag == _sourceFlag 
            && s.Level == RMSecurityContainerLevel.MailBox 
            && s.Status == RMSecurityContainerStaus.MaybeDeleted);
            return existSubContainers;
        }

        /// <summary>
        /// process root node
        /// </summary>
        /// <param name="root"></param>
        private async System.Threading.Tasks.Task ProcessRootAsync(RMSampleEXOTreeNode root)
        {
            //var containers = _settingTreeService.BrowseSampleExchangeTree(root, false).Where(o => _containerInGroups.Contains(o.Id));
            var containerTreeNodes = await _settingTreeService.BrowseSampleExchangeTreeAsync(root, false);
            var sameContainerIds = _containerInGroups.Intersect(containerTreeNodes.Select(o => o.Id)).ToList();
            var deletedContainerIds = _containerInGroups.Except(sameContainerIds).ToList();
            var containers = containerTreeNodes.Where(o => sameContainerIds.Contains(o.Id));

            ProcessDeletedContainers(deletedContainerIds);
            _reportManger.IncreaseBase(containers.Count());

            InitO365TenantIds();

            foreach (var container in containers)
            {
                try
                {
                    var saveCount = SaveContainer(container);
                    AddJobDetail4Updated(container.Name, container.Name, saveCount > 0);
                    await ProcessContainerAsync(container);
                }
                catch (Exception e)
                {
                    logger.Warn($"Error occurred while sync data of EXO container : {container.Name}, error : {e.ToString()}");
                    AddJobDetail(container.Name, string.Empty, JobDetailsStatus.Failed, e.Message);
                    _isSucceed = false;
                }
            }

            ProcessDeletedSubContainers();
        }

        private void InitO365TenantIds()
        {
            _o365TenantIds = RMAosApiClient.GetO365TenantIds(TenantLocalValue.LogonGroupId);
        }

        /// <summary>
        /// process container
        /// </summary>
        /// <param name="container"></param>
        private async System.Threading.Tasks.Task ProcessContainerAsync(RMSampleEXOTreeNode container)
        {
            var subContainers = await _settingTreeService.BrowseSampleExchangeTreeAsync(container, false);
            //if (subContainers.Count == 0) return;
            _reportManger.IncreaseBase(subContainers.Count());

            foreach (var subContainer in subContainers)
            {
                try
                {
                    logger.Info($"Try to sync for mailbox : {subContainer.Name}");
                    var realObjectId = GetRealObjectId(subContainer);
                    ProcessSubContainer(subContainer, container, realObjectId);
                    logger.Info($"End of sync for mailbox : {subContainer.Name}");
                }
                catch (Exception e)
                {
                    logger.Warn($"Error occurred while sync data of mailbox : {subContainer.Name}, error : {e.ToString()}");
                    AddJobDetail(container.Name, subContainer.Name, JobDetailsStatus.Failed, e.Message);
                    _isSucceed = false;
                }

            }

            MarkDeletedSubContainers(container.Id, subContainers.Select(o => o.Id).ToList());
            logger.Info($"Sync {subContainers.Count} mailboxes for EXO container : {container.Name}");
        }


        private void ProcessSubContainer(RMSampleEXOTreeNode subContainer, RMSampleEXOTreeNode container, string realObjectId)
        {
            if (container != null) AssembleOldTreeNodeId(subContainer, realObjectId);
            ArgumentCheck.NotNull(container, nameof(container));
            UpdateRecords(container.Name, null,  container.Id, subContainer.Id);
            if (realObjectId != null)
            {
                UpdateRecords(container.Name, null, container.Id, RemoveArchiverKeyWords(realObjectId));
            }

            var saveCount = SaveContainer(subContainer, container, realObjectId);
            AddJobDetail4Updated(container.Name, subContainer.Name, saveCount > 0);
        }

        /// <summary>
        /// 获取真实的O365中的mailbox的id.
        /// 
        /// </summary>
        /// <param name="subContainer"></param>
        /// <returns></returns>
        private string GetRealObjectId(RMSampleEXOTreeNode subContainer)
        {
            foreach(var o365TenantId in _o365TenantIds)
            {
               var objectId = RMAosApiClient.GetAOSMailboxGuid(TenantLocalValue.LogonGroupId, o365TenantId, subContainer.Name);
                if (!string.IsNullOrEmpty(objectId)) return objectId;
            }
            return null;
        }

        /// <summary>
        /// 重设Id.
        /// 由于历史原因，Opus中保存的数据（包括Cosmos DB）一直使用的都是从DAO返回的tree node id，而这个tree node id是会变的，需要获取之前的tree node的id.
        /// </summary>
        /// <param name="container"></param>
        private void AssembleOldTreeNodeId(RMSampleEXOTreeNode container, string realObjectId)
        {
            var existContainer = _securityContainerDao.GetByLambda(o => (o.Name == container.Name || (realObjectId != null && realObjectId == o.ObjectId))  && o.Id != container.Id && o.SourceFlag == SourceFlag.Exchange).FirstOrDefault();
            if (existContainer != null)
            {
                container.Id = existContainer.Id;
            }
        }
        //private void UpdateRecords(string containerName, string containerId, string subContainerId, bool isClearContainer = false)
        //{
        //    var scopeId = Guid.Parse(subContainerId);
        //    if (!_recordProcessor.Process(containerName, containerId, scopeId, isClearContainer)) _isSucceed = false;
        //}

        private int SaveContainer(RMSampleEXOTreeNode node, RMSampleEXOTreeNode parent = null, string realOjbectId = null)
        {
            var result = node.Convert2SecurityContainer(parent);
            if (!string.IsNullOrEmpty(realOjbectId)) result.ObjectId = realOjbectId;
            return _containerService.UpSert(result);
        }
    }
}
