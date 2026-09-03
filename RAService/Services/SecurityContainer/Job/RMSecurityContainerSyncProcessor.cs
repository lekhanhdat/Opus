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
using AvePoint.RA.Common.Report;
using AvePoint.RA.Common.Util;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Aos.Notification;
using AvePoint.RA.Contract.DocAve;
using AvePoint.RA.Contract.Exceptions;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.Contract.Object.JobMessage;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Explorer.Dao;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace AvePoint.RA.Service.Services.SecurityContainer.Job
{
    public interface IRMSecurityContainerSyncProcessor
    {
        System.Threading.Tasks.Task RunAsync(RMJobMessage msg);
    }

    public class RMSecurityContainerSyncProcessor : IRMSecurityContainerSyncProcessor
    {
        private RALogger logger = RALogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);
        private bool _jobHasException = false;
        private string _jobComment = "";
        private bool _jobHasStopped = false;
        private bool _jobFailed = false;

        private IRMReportManager _reportManger;
        private IRMReportManager ReportManager
        {
            get
            {
                if (_reportManger == null)
                {
                    _reportManger = ReportMangerFactory.Instance.ReportManager;
                }
                return _reportManger;
            }
        }

        private IExplorerDao _explorerDao;
        private IExplorerDao ExplorerDao
        {
            get
            {
                if (_explorerDao == null)
                {
                    _explorerDao = new RA.DB.Explorer.Dao.CosmosImp.ExplorerDao();
                }
                return _explorerDao;
            }
        }

        public ISPSettingTreeService SPSettingTreeService => PlatformWindsorManager.GetService<ISPSettingTreeService>();
        public IRMSecurityContainerService SecurityContainerService { get; set; }
        public IRMScopeRoleAssignmentDao ScopeRoleAssignmentDao { get; set; }

        public IRMSecurityContainerDao SecurityContainerDao { get; set; }

        private ITenantService TenantService => PlatformWindsorManager.GetService<ITenantService>();


        public async System.Threading.Tasks.Task RunAsync(RMJobMessage msg)
        {
            var tmpAccountType = TenantLocalValue.AccountType;
            TenantLocalValue.AccountType = RMAccountType.ApplicationAdmin;
            try
            {
                Init(msg);

                using (new CheckJobStopScope())
                {
                    if (RemoteTreeNodeSynced())
                    {
                        var scopes = ScopeRoleAssignmentDao.QueryAllScopes();
                        var groups = scopes.GroupBy(o => o.DataSourceType);
                        ReportManager.IncreaseBase(scopes.Count);

                        foreach (var group in groups)
                        {
                            var containerIds = group.Select(o => o.ScopeId.ToString()).ToList();
                            await ProcessContainersAsync(group.Key, containerIds);
                        }
                    }
                    else
                    {
                        AddFailedJobDetail();
                    }
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
                _jobComment = e.Message;
                //throw;
            }
            finally
            {
                TenantLocalValue.AccountType = tmpAccountType;
                var finalStatus = _jobFailed? JobStatus.Failed: _jobHasStopped ? JobStatus.Stopped : _jobHasException ? JobStatus.FinishWithException : JobStatus.Finished;
                ReportManager.SetJobFinished(finalStatus, _jobComment);
            }
        }

        private void Init(RMJobMessage msg)
        {
            ReportMangerFactory.Instance.Init(msg.JobID, msg.JobType);
            ReportManager.StartUpdateJobProgress();
        }

        private async System.Threading.Tasks.Task ProcessContainersAsync(int sourceFlag, IList<string> containerIds)
        {
            var validSourceFlags = SourceFlagHelper.GetDefaultContainerIdSource().Select(o => (int)o);
            if (!validSourceFlags.Contains(sourceFlag))
            {
                logger.Warn($"Not supported source type : {sourceFlag}");
                return;
            }
            RMContainersSyncBaseProcessor processor;
            if (sourceFlag == (int)SourceFlag.Exchange)
            {
                processor = new RMEXOContainersSyncProcessor(ReportManager, SPSettingTreeService, SecurityContainerService, ScopeRoleAssignmentDao, SecurityContainerDao, ExplorerDao, containerIds);
            }
            else if (sourceFlag == (int)SourceFlag.SharePoint)
            {
                processor = new RMSPOContainersSyncProcessor(ReportManager, SPSettingTreeService, SecurityContainerService, ScopeRoleAssignmentDao, SecurityContainerDao, ExplorerDao, containerIds);
            }
            else
            {
                processor = new RMOneDriverContainersSyncProcessor(ReportManager, SPSettingTreeService, SecurityContainerService, ScopeRoleAssignmentDao, SecurityContainerDao, ExplorerDao, containerIds);
            }

            if (!(await processor.ProcessAsync())) _jobHasException = true;
        }

        /// <summary>
        /// 检查sync tree node的job是否已经初始化完毕
        /// </summary>
        /// <returns></returns>
        private bool RemoteTreeNodeSynced()
        {
            var isSynced = TenantService.GetTenantInitNodeState(TenantLocalValue.LogonGroupId) == RMInitNodeState.Synced;
            logger.Info($"Is remote node synced: {isSynced}");
            return isSynced;
        }

        private void AddFailedJobDetail()
        {
            _jobFailed = true;
            _jobComment = I18N.Core.I18NEntity.GetString("RM_JS_DAM_RemoteNodesNotInit");
            //ReportManager.SendJobDetail(new JMSyncSecurityContainerJobDetails()
            //{
            //    ObjectName = string.Empty,
            //    Container = string.Empty,
            //    FullPath = string.Empty,
            //    Status = JobDetailsStatus.Failed,
            //    Comment = I18N.Core.I18NEntity.GetString("RM_JS_DAM_RemoteNodesNotInit")
            //});
        }
    }
}
