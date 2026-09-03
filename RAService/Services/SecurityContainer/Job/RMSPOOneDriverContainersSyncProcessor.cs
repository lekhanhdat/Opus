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
using AvePoint.RA.Common.Global.Utils;
using AvePoint.RA.Common.Report;
using AvePoint.RA.Contract.DocAve;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.Object.Extension;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Explorer.Dao;
using AvePoint.RA.SharePoint.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AvePoint.RA.Service.Services.SecurityContainer.Job
{
    /// <summary>
    /// base class for SPO and One Driver containers sync processor
    /// </summary>
    internal abstract class RMSPOOneDriverContainersSyncProcessor : RMContainersSyncBaseProcessor
    {
        protected readonly ISPSettingTreeService _settingTreeService;
        protected readonly IList<string> _containerInGroups;


        public RMSPOOneDriverContainersSyncProcessor(IRMReportManager reportManger, ISPSettingTreeService spSettingTreeService, 
            IRMSecurityContainerService rmSecurityContainerService, IRMScopeRoleAssignmentDao scopeRoleAssignmentDao, 
            IRMSecurityContainerDao securityContainerDao, IExplorerDao explorerDao, IList<string> containerInGroups, SourceFlag sourceFlag)
            : base(reportManger, rmSecurityContainerService, scopeRoleAssignmentDao, securityContainerDao, explorerDao, sourceFlag)
        {
            _settingTreeService = spSettingTreeService;
            _containerInGroups = containerInGroups;
        }

        /// <summary>
        /// update SPO containers. 
        /// return false if there is any errors.
        /// </summary>
        /// <returns></returns>
        public override async Task<bool> ProcessAsync()
        {
            try
            {
                logger.Info($"Start to sync {_sourceFlag} data.");
                var farmNode = _settingTreeService.LoadFarmSampleTree()[0];
                if (farmNode == null || farmNode.Id.Equals(System.Guid.Empty))
                {
                    var exceptionMsg = "sharepoint farm node is null.";
                    logger.Warn(exceptionMsg);
                    AddJobDetail(string.Empty, string.Empty, JobDetailsStatus.Failed, exceptionMsg);
                    return false;
                }

                await ProcessRootAsync(farmNode);

                logger.Info($"Sync {_sourceFlag} data end.");
            }
            catch (Exception e)
            {
                logger.Error($"Error occurred while sync {_sourceFlag} data. Error: {e.ToString()}");
                AddJobDetail(string.Empty, string.Empty, JobDetailsStatus.Failed, e.Message);
                _isSucceed = false;
            }

            return _isSucceed;
        }

        protected abstract RMBrowseTreeNodeSourceType GetBrowseTreeNodeSourceType();

        protected override IList<RMSecurityContainerDto> GetRealDeletedSubContainers()
        {
            var existSubContainers = _securityContainerDao.GetByLambda(s => s.SourceFlag == _sourceFlag
            && s.Level == RMSecurityContainerLevel.SiteCollection
            && s.Status == RMSecurityContainerStaus.MaybeDeleted);
            return existSubContainers;
        }
        /// <summary>
        /// process root node
        /// </summary>
        /// <param name="root"></param>
        private async System.Threading.Tasks.Task ProcessRootAsync(RMSPSampleTreeNode root)
        {
            //var containers = _settingTreeService.BrowseSampleTree(root, false).Where(o => _containerInGroups.Contains(o.Id));
            var containerTreeNodes = await _settingTreeService.BrowseSampleTreeAsync(root, false, GetBrowseTreeNodeSourceType(), needI18N: false);
            var sameContainerIds = _containerInGroups.Intersect(containerTreeNodes.Select(o => o.Id)).ToList();
            var deletedContainerIds = _containerInGroups.Except(sameContainerIds).ToList();
            var containers = containerTreeNodes.Where(o => sameContainerIds.Contains(o.Id));

            ProcessDeletedContainers(deletedContainerIds);

            _reportManger.IncreaseBase(containers.Count());

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
                    logger.Warn($"Error occurred while sync data of {_sourceFlag} container : {container.Name}, error : {e.ToString()}");
                    AddJobDetail(container.Name, string.Empty, JobDetailsStatus.Failed, e.Message);
                    _isSucceed = false;
                }
            }

            ProcessDeletedSubContainers();
        }

        /// <summary>
        /// process container
        /// </summary>
        /// <param name="container"></param>
        private async System.Threading.Tasks.Task ProcessContainerAsync(RMSPSampleTreeNode container)
        {
            var subContainers = await _settingTreeService.BrowseSampleTreeAsync(container, false, GetBrowseTreeNodeSourceType());
            //if (subContainers.Count == 0) return;
            _reportManger.IncreaseBase(subContainers.Count());

            foreach (var subContainer in subContainers)
            {
                try
                {
                    logger.Info($"Try to sync for {_sourceFlag} sitecollection : {subContainer.Name}");
                    ProcessSubContainer(subContainer, container);
                    logger.Info($"End of sync for {_sourceFlag} sitecollection : {subContainer.Name}");
                }
                catch (Exception e)
                {
                    logger.Warn($"Error occurred while sync data of {_sourceFlag} sitecollection : {subContainer.Name}, error : {e.ToString()}");
                    _isSucceed = false;
                    AddJobDetail(container.Name, subContainer.Name, JobDetailsStatus.Failed, e.Message);
                }
            }

            MarkDeletedSubContainers(container.Id, subContainers.Select(o => o.Id).ToList());

            logger.Info($"Sync {subContainers.Count} sitecollection for {_sourceFlag} container : {container.Name}");
        }

        private void ProcessSubContainer(RMSPSampleTreeNode subContainer, RMSPSampleTreeNode container)
        {
            if (container != null)
            {
                CommonClientContext context = new CommonClientContext();
                var siteId = context.GetSiteId(subContainer);
                subContainer.Id = siteId.ToString();
                logger.Info($"get real site id for {_sourceFlag} container {subContainer.FullPath}");
            }
            ArgumentCheck.NotNull(container, nameof(container));
            UpdateRecords(container.Name, subContainer.Name, container.Id, subContainer.Id);
            var saveCount = SaveContainer(subContainer, container);
            AddJobDetail4Updated(container.Name, subContainer.Name, saveCount > 0);
        }

        private int SaveContainer(RMSPSampleTreeNode node, RMSPSampleTreeNode parent = null)
        {
            var sourceFlag = GetBrowseTreeNodeSourceType() == RMBrowseTreeNodeSourceType.SharepointOnline ? SourceFlag.SharePoint : SourceFlag.OneDrive;
            var result = node.Convert2SecurityContainer(sourceFlag, parent);
           
            return _containerService.UpSert(result);
        }
    }
}
