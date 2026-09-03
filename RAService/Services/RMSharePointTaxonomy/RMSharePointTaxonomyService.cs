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
using AvePoint.RA.CommonUtil;
using AvePoint.GCommon.Utility;
using AvePoint.RA.Common.Util;
using AvePoint.RA.Contract.DocAve;
using AvePoint.RA.Contract.JobMonitor;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Model;
using AvePoint.RA.Service.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using AvePoint.RA.Contract.Audit;
using AvePoint.RA.Contract.RMWeb.Audit;
using AvePoint.RA.Service.Services.RMSharePointTaxonomy.AuditHandler;
using AvePoint.RA.I18N.Core;
using AvePoint.RA.Contract.RMWeb.ReportCenter;
using System.IO;
using System.Diagnostics;
using AvePoint.RA.Contract.CloudService;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.RADataBroker;
using AvePoint.GCommon.Contract.Server.ControlPanel.Office365;
using AvePoint.RA.RACommonUtility.Browser;
using AvePoint.RA.Contract.Global.Object;
using AvePoint.RA.RACommonUtility.SharePointOnPrem;
using AvePoint.RA.Contract.RMWeb.SignalR;
using AvePoint.RA.Common;
using AvePoint.RA.Service.Services.SignalR;
using AvePoint.RA.Common.Configurations;
using AvePoint.RA.Contract.Global.JobMessage;
using AvePoint.RA.Contract.SharePoint;
using AvePoint.RA.Contract.RMWeb.SingalR;
using AvePoint.RA.RACommonUtility.Telemetry;
using AvePoint.RA.Common.Aos;
using Newtonsoft.Json;
using Amazon.Runtime.Internal.Transform;

namespace AvePoint.RA.Service.Services.RMSharePointTaxonomy
{
    [Audit]
    public class RMSharePointTaxonomyService : RMServiceBase, IRMSharePointTaxonomyService
    {
        private ITermDao TermDAO => PlatformWindsorManager.GetService<ITermDao>();
        private ITermSetDao TermSetDAO => PlatformWindsorManager.GetService<ITermSetDao>();
        private ITermGroupDao TermGroupDAO => PlatformWindsorManager.GetService<ITermGroupDao>();
        private ITermGroupMembershipDao TermGroupMembershipDao => PlatformWindsorManager.GetService<ITermGroupMembershipDao>();
        private IRMSubJobDao SubJobDao => PlatformWindsorManager.GetService<IRMSubJobDao>();
        private IJobMonitorService JobMonitorService => PlatformWindsorManager.GetService<IJobMonitorService>();
        private IRMLocalNodeService RMLocalNodeService => PlatformWindsorManager.GetService<IRMLocalNodeService>();

        private IRMRemoteNodeService RMRemoteNodeService => PlatformWindsorManager.GetService<IRMRemoteNodeService>();

        private IJobQueueService mJobQueueService => PlatformWindsorManager.GetService<IJobQueueService>();
        private IHybridSharePointOnPremWorkerService HybridSharePointWorkerService => PlatformWindsorManager.GetService<IHybridSharePointOnPremWorkerService>();

        private ITenantService TenantService => PlatformWindsorManager.GetService<ITenantService>();

        private RALogger logger = RALogger.GetInstance(typeof(RMSharePointTaxonomyService));

        private BaseJobDto baseJobDto;

        private async Task<bool> IsExistLocalSiteCollectionsAsync()
        {
            try
            {
                var localSites = await SharePointOnPremClient.GetAllLocalSiteCollectionsAsync();
                return localSites.Count > 0 ? true : false;
            }
            catch (Exception ex)
            {
                logger.Info("no local site.");
                return false;
            }
        }

        public async Task<RAReturnMessage> RunSyncRMTermTreeToSharePointAsync(JobRunBy jobRunBy, bool fromTimerJobPage, bool fromGoogleOne = false)
        {
            RAReturnMessage reMsg = new RAReturnMessage();
            var isAllTermGroupBothNoneOption = await TermGroupDAO.CheckIsAllTermGroupsBothNoneOption();
            if (isAllTermGroupBothNoneOption)
            {
                reMsg.MessageType = RAMessageType.Failed;
                reMsg.ErrorMessage = I18NEntity.GetString("RM_JS_BCM_TermSync_AllOptionsAreNone");
                reMsg.FaildType = RAFailedType.AllTermGroupsHavingBothNoneOptions;
                return reMsg;
            }
            var syncToOnlineMessage = await CreateSyncJobQueueAsync(JobType.TermSynchronization, jobRunBy, fromTimerJobPage, fromGoogleOne);
            var hasSPOnPremiseLicense = TenantService.CheckLicenseWithAdditionalDataSource(TenantLocalValue.LogonGroupId, AvePoint.RA.Contract.RoleAssignments.PaidForModule.SharePointOnPrem);
            if (syncToOnlineMessage.MessageType == RAMessageType.Failed && (!hasSPOnPremiseLicense || fromGoogleOne))
            {
                reMsg.MessageType = RAMessageType.Failed;
                reMsg.ErrorMessage = I18NEntity.GetString("RM_JS_BCM_TermSync_NoSC");
            }
            else if (syncToOnlineMessage.MessageType == RAMessageType.Successful)
            {
                reMsg.Extension = syncToOnlineMessage.Extension;
            }

            if(hasSPOnPremiseLicense && !fromGoogleOne)
            {
                var syncToOnpremMessage = await CreateSyncJobQueueAsync(JobType.SPOnPremTermSynchronization, jobRunBy, fromTimerJobPage);
                if (syncToOnlineMessage.MessageType == RAMessageType.Failed && syncToOnpremMessage.MessageType == RAMessageType.Successful)
                {
                    reMsg.Extension = syncToOnpremMessage.Extension;
                }
            }

            TelemetryContext.SendToQueue(TelemetryModule.TermManagement, TelemetryEventType.TermSynchronise);
            await TelemetryContext.FlushAsync();

            return reMsg;
        }

        private async Task<RAReturnMessage> CreateSyncJobQueueAsync(JobType jobType, JobRunBy jobRunBy, bool fromTimerJobPage, bool fromGoogleOne = false)
        {
            var reMsg = new RAReturnMessage();
            if (await NeedAddSyncTermJobQueueAsync(jobType))
            {
                var parameter = jobType == JobType.SPOnPremTermSynchronization ? fromTimerJobPage.ToString() : JsonConvert.SerializeObject(new Dictionary<string, bool> { { "fromTimerJobPage", fromTimerJobPage }, { "fromGoogleOne", fromGoogleOne } });
                JobQueueDto jqDto = new JobQueueDto()
                {
                    JobType = jobType,
                    JobRunType = jobRunBy,
                    TenantGroupId = TenantLocalValue.LogonGroupId,
                    JobRunByUser = TenantLocalValue.LogonUserEmail.IsNullOrEmpty() ? "RM_TS_RunSchedule" : TenantLocalValue.LogonUserEmail,
                    Parameters = parameter,
                };
                string id = mJobQueueService.AddToDBJobQueue(jqDto);
                reMsg.Extension = id;
            }
            else 
            {
                reMsg.MessageType = RAMessageType.Failed;
            }
            return reMsg;
        }


        private async Task<bool> NeedAddSyncTermJobQueueAsync(JobType jobType)
        {
            var allTenantGoogle = RMAosApiClient.GetGoogleTenantIds(TenantLocalValue.LogonGroupId);

            var hasSyncTermToGoogle = allTenantGoogle.Count > 0 && TermGroupDAO.IsExistNeedSyncTermGroupGoogle();

            if (jobType == JobType.TermSynchronization)
            {
                return RMRemoteNodeService.IsRemoteSiteExist() && TermGroupDAO.IsExistNeedSyncTermGroup(SiteType.Online) || hasSyncTermToGoogle;
            }
            if (jobType == JobType.SPOnPremTermSynchronization)
            {
                return (await IsExistLocalSiteCollectionsAsync()) && TermGroupDAO.IsExistNeedSyncTermGroup(SiteType.OnPrem);
            }
            return false;
        }

        [Audit(Module = AuditModule.BusinessClassificationManagement, Category = AuditCategory.TermSynchronization, Action = AuditAction.RunTermSyncJob, BeforeHandler = typeof(RMTermSyncBeforeAuditHandler), AfterHandler = typeof(RMTermSyncAfterAuditHandler))]
        public string RealRunSyncJob(JobRunBy jobRunBy, string jobRunByUser, bool fromTimerJobPage, bool fromGoogleOne)
        {
            string id = string.Empty;
            //起Job，判断是前台起Job还是Schedule起的Job
            if (jobRunBy == JobRunBy.Control)
            {
                id = JobMonitorService.CreateJob(JobType.TermSynchronization, jobRunByUser);
                logger.Info("Begin control Sync Job {0}", id);
            }
            else if (jobRunBy == JobRunBy.Schedule)
            {
                id = JobMonitorService.CreateJob(JobType.TermSynchronization, "RM_TS_RunSchedule");
                logger.Info("Begin schedule Sync Job {0}", id);
            }
            else
            {
                id = JobMonitorService.CreateJob(JobType.TermSynchronization, jobRunByUser);
                logger.Info("Begin default Sync Job {0}", id);
            }
            //静态变量，记录一下这次IIS启动到现在所有起过的Term Sync Job
            baseJobDto = new BaseJobDto() { Id = id, JobType = (int)JobType.TermSynchronization };
            //string jobid = JobMonitorService.GetJobIdByJobTypeExceptCurrent(JobType.TermSynchronizatoin, id);
            //查询当前还没有结束的Term Sync Job
            List<string> runningJobs = JobMonitorService.GetRunningJobs(JobType.TermSynchronization);

            //Term Sync Job一次只能同时运行一个，所以判断当前起的Job是否要Skip掉
            bool isSkip = runningJobs.Any(j => j != id);

            if (!isSkip)
            {
                //新起线程起Job
                //ParameterizedThreadStart threadStart = new ParameterizedThreadStart(MainSyncProgress);
                //Thread thread = new Thread(threadStart);
                //thread.IsBackground = true;
                //thread.Start(id);
                StartSyncJob(id, jobRunBy, fromGoogleOne);
            }
            else
            {
                logger.Info(I18NEntity.GetString("RM_SYNC_JobSkip"));
                JobMonitorService.UpdateJobStatus(id, JobStatus.Skipped, "RM_SYNC_JobSkip");
            }
           
            return id;
        }
        private void StartSyncJob(string jobId, JobRunBy runBy, bool fromGoogleOne)
        {

            mJobQueueService.HandleMessage(new JobQueueMessage()
            {
                JobId = jobId,
                RunBy = runBy,
                JobType = JobType.TermSynchronization,
                CommandLine = string.Format("{0} {1} {2}", JobType.TermSynchronization, jobId, fromGoogleOne),
            });
        }


        #region public method of Orphanedterm 

        public RMTermStatus getRmTermStatus(RMTerm term)
        {
            if (term.IsSPRemoved)
            {
                return RMTermStatus.Removed;
            }
            else if (term.IsSPDeprecated)
            {
                return RMTermStatus.Retired;
            }
            else
            {
                if (term.IsRemoved)
                {
                    return RMTermStatus.Removed;
                }
                else
                {
                    return RMTermStatus.Retired;
                }
            }
        }
        #endregion

        [Audit(Module = AuditModule.BusinessClassificationManagement, Category = AuditCategory.TermSynchronization, Action = AuditAction.RunSPOnpremSyncTermJob, BeforeHandler = typeof(RMTermSyncBeforeAuditHandler), AfterHandler = typeof(RMTermSyncAfterAuditHandler))]
        public async Task<string> RealRunSyncJobForSPOnpremAsync(JobRunBy jobRunBy, string jobRunByUser, bool fromTimerJobPage)
        {
            var runningJobIds = JobMonitorService.GetRunningJobs(JobType.SPOnPremTermSynchronization);
            string jobId;
            var runJobUser = jobRunBy == JobRunBy.Schedule ? "RM_TS_RunSchedule" : jobRunByUser;
            if (!runningJobIds.IsNullOrEmpty())
            {
                logger.Info("Current running term sync job:{0}", string.Join(", ", runningJobIds.ToArray()));
                jobId = JobMonitorService.CreateJob(JobType.SPOnPremTermSynchronization, runJobUser);
                JobMonitorService.UpdateJobStatus(jobId, JobStatus.Skipped, "RM_SYNC_JobSkip");
            }
            else
            {
                jobId = await RunSPOnPremTermSyncJobAsync(runJobUser);
            }
            return jobId;
        }

        private async Task<string> RunSPOnPremTermSyncJobAsync(string jobRunByUser)
        {
            var jobType = JobType.SPOnPremTermSynchronization;
            string jobId = string.Empty;
            jobId = JobMonitorService.CreateJob(jobType, jobRunByUser);
            var needSyncFarmIds = await GetNeedSyncFarmIds();
            if (needSyncFarmIds.IsNullOrEmpty())
            {
                logger.Warn("No farms node.");
                JobMonitorService.UpdateJobStatus(jobId, JobStatus.Failed, "RM_SS_FSNoAvailableAgent");
                return jobId;
            }
            
            int subJobCount = needSyncFarmIds.Count;
            logger.Debug($"Need sync farms count :{subJobCount}");
            SubJobDao.UpdateSubJobCount(jobId, subJobCount);
            int currentSubjobIndex = 0;
            foreach (var farmId in needSyncFarmIds)
            {
                string subJobId = CreateSubJob(jobType, jobId, currentSubjobIndex, subJobCount, farmId);
                HybridSharePointWorkerService.StartSPJob(new Hybrid.Contract.RecordsJobArgs()
                {
                    JobId = subJobId,
                    JobType = AvePoint.Hybrid.Contract.JobType.SPOnPremTermSynchronization,
                    TenantId = TenantLocalValue.LogonGroupId,
                    FarmId = farmId
                });
                currentSubjobIndex++;
            }
            return jobId;
        }

        private async Task<List<string>> GetNeedSyncFarmIds()
        {
            var farmIds = new List<string>();
            var allTermGroups = TermGroupDAO.LoadTermGroup(false);
            if (allTermGroups.Count > 0)
            {
                if (allTermGroups.Any(o => !o.UsingMMSSpecified))
                {
                    farmIds = await GetAllFarmIdsAsync();
                    logger.Info("Need to sync term to all farms.");
                }
                else
                {
                    farmIds = TermGroupDAO.GetFarmIdsBySpecificSites();
                    logger.Info($"Need to sync term to specific farms, ids:{string.Join(",", farmIds)}");
                }
            }
            return farmIds;
        }

        private async Task<List<string>> GetAllFarmIdsAsync()
        {
            var farmIds = new List<string>();
            var spTreeMessage = await SharePointOnPremClient.BrowseFarmsAsync();
            if (spTreeMessage != null && spTreeMessage.NodeList != null && spTreeMessage.NodeList.Count > 0)
            {
                var spFarms = spTreeMessage.NodeList;
                farmIds = spFarms.Select(o => o.FarmID).Distinct().ToList();
            }
            return farmIds;
        }

        private string CreateSubJob(JobType jobType, string jobId, int currentSubjobIndex, int subJobCount, string farmId)
        {
            string subJobId = string.Format(jobId + "_{0:D3}", currentSubjobIndex);
            var subJob = new RMSubJob() { Id = subJobId, ParentId = jobId, FarmId = farmId, StartTime = DateTime.UtcNow.Ticks, JobType = (int)jobType, Progress = 0, Status = (int)JobStatus.Wait, Weight = 100d / subJobCount };
            subJob.Runable = RecordsConstants.SubJob_Runnable_Runing;
            SubJobDao.CreateJob(subJob);
            logger.Info("Create sub job {0} sucessfull, type {1}, weight {2} ", subJob.Id, subJob.JobType, subJob.Weight);
            return subJobId;
        }

        #region agent api call method

        public async Task<string> GetTermSyncJobMessageAsync(string jobId)
        {
            string message = "";
            try
            {
                logger.Info($"start get sync term job message, jobId:{jobId}");
                var subJobInfo = SubJobDao.GetSubJob(jobId, true);
                var farmId = subJobInfo.FarmId;
                logger.Info($"farm id: {farmId}");
                var siteUrls = new List<string>();
                try
                {
                    var sites = await RMLocalNodeService.GetLocalSiteCollectionsByFarmIdAsync(farmId);
                    siteUrls = sites.Select(o => o.Url).Distinct().ToList();
                }
                catch (Exception ex)
                {
                    logger.Warn($"An error while get local site urls, farmId:{farmId}, error message:{ex}");
                    throw;
                }
                var jobMessage = new TermSyncJobMessage
                {
                    FarmTermGroupIdsRelation = TermGroupDAO.GetTermGroupIdsByFarmId(farmId),
                    TermGroupNodes = LoadTermNodes(),
                    TermGroupMembership = GetAllTermGroupMembership()
                };
                message = SerializerHelper.SerializeByDataContractSerializer(jobMessage);
                logger.Info($"success get sync term job message, jobId:{jobId}");
            }
            catch (Exception ex)
            {
                logger.Error($"An error while get term sync job message, error message:{ex}");
            }
            return message;
        }
        public List<GRMTermGroupMembership> GetAllTermGroupMembership()
        {
            var gRMTermGroupMembership = new List<GRMTermGroupMembership>();
            try
            {
                var allTermGroupMembers = TermGroupMembershipDao.GetAllTermGroupMembership();
                gRMTermGroupMembership = allTermGroupMembers.ConvertAll(o => ConvertToGRMTermGroupMembership(o));
            }
            catch (Exception ex)
            {
                logger.Error($"call api: An error while GetAllTermGroupMembership, error:{ex}");
            }
            return gRMTermGroupMembership;
        }
        /// <summary>
        /// 所有Term Group节点信息，包括子节点信息,For sync onpremise terms
        /// </summary>
        /// <returns></returns>
        public List<GRMTermGroup> LoadTermNodes()
        {
            var termGroups = LoadTermGroups();
            foreach (var termGroup in termGroups)
            {
                termGroup.subTerms = LoadTermSets(termGroup.UniqueId);
            }
            return termGroups;
        }
        /// <summary>
        /// for sync onpremise nodes.
        /// </summary>
        /// <returns></returns>
        private List<GRMTermGroup> LoadTermGroups()
        {
            var gTermGroups = new List<GRMTermGroup>();
            try
            {
                var termGroups = TermGroupDAO.LoadSPTermGroup();
                gTermGroups = termGroups.ConvertAll(o => ConvertToGRMTermGroup(o));
            }
            catch (Exception ex)
            {
                logger.Error($"call api: An error while get term groups, error:{ex}");
            }
            return gTermGroups;
        }
        private List<GRMTermSet> LoadTermSets(Guid termGroupId)
        {
            var gTermSets = new List<GRMTermSet>();
            try
            {
                logger.Info("begin LoadTermSets:{0}", termGroupId);
                var termSets = TermSetDAO.LoadTermSetNodes(termGroupId);
                gTermSets = termSets.ConvertAll(o => ConvertToGRmTermSet(o));

                if (gTermSets.Count == 0)
                {
                    logger.Warn("Term set not found,tgroupId:{0}", termGroupId);
                    return gTermSets;
                }

                foreach (var gTermSet in gTermSets)
                {
                    AppendTerms(gTermSet);
                }
                logger.Info("LoadTermSets Complete.");
            }
            catch (Exception e)
            {
                logger.Error("There are some error in LoadTermSets {0}", e.ToString());
            }
            return gTermSets;
        }
        private void AppendTerms(GRMTermSet gTermSet)
        {
            List<RMTerm> terms = TermDAO.GetTermFromTermSet(gTermSet.Id, true);
            var gTerms = terms.ConvertAll(o => ConvertToGRMTerm(o));
            if (gTerms.Count > 0)
            {
                gTermSet.RMTerms = gTerms;
                foreach (var term in gTerms)
                {
                    AppendTerm(term);
                }
            }
        }
        private void AppendTerm(GRMTerm pTerm)
        {
            var subTerms = TermDAO.GetTermFromParentId(pTerm.Id);
            if (subTerms.Count > 0)
            {
                var gSubTerms = subTerms.ConvertAll(o => ConvertToGRMTerm(o));
                pTerm.subTerms = gSubTerms;
                foreach (var subTerm in gSubTerms)
                {
                    AppendTerm(subTerm);
                }
            }
        }
        private GRMTermGroup ConvertToGRMTermGroup(RMTermGroup termGroup)
        {
            var gTermGroup = new GRMTermGroup
            {
                Name = termGroup.Name,
                Description = termGroup.Description,
                UniqueId = termGroup.UniqueId,
                UsingMMSSpecified = termGroup.UsingMMSSpecified,
                IsRemoved = termGroup.IsRemoved
            };
            return gTermGroup;
        }
        private GRMTermSet ConvertToGRmTermSet(RMTermSet termSet)
        {
            var gTermSet = new GRMTermSet
            {
                Id = termSet.Id,
                Name = termSet.Name,
                Description = termSet.Description,
                UniqueId = termSet.UniqueId,
                IsRemoved = termSet.IsRemoved
            };
            return gTermSet;
        }
        private GRMTerm ConvertToGRMTerm(RMTerm term)
        {
            var gTerm = new GRMTerm
            {
                Id = term.Id,
                TermSetId = term.TermSetId,
                Name = term.Name,
                Description = term.Description,
                UniqueId = term.UniqueId,
                IsDeprecated = term.IsDeprecated,
                IsExpired = term.IsExpired,
                IsRemoved = term.IsRemoved,
                TermExpirationFrom = term.TermExpirationFrom,
                TermExpirationTo = term.TermExpirationTo
            };
            return gTerm;
        }

        private GRMTermGroupMembership ConvertToGRMTermGroupMembership(RMTermGroupMembership t)
        {
            var gRMTermGroupMembership = new GRMTermGroupMembership
            {
                TermGroupId = t.TermGroupId,
                SiteUrl = t.SiteUrl,
                TermStoreId = t.TermStoreId,
                TermStoreName = t.TermStoreName,
                DisplayName = t.DisplayName,
                AgentGroupId = t.AgentGroupId
            };
            return gRMTermGroupMembership;
        }

        #endregion
    }
}
