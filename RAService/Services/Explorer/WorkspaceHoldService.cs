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
using AvePoint.RA.Common.Configurations;
using AvePoint.RA.Common.SystemSetting;
using AvePoint.RA.Common.Util;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Audit;
using AvePoint.RA.Contract.CloudService;
using AvePoint.RA.Contract.Configurations;
using AvePoint.RA.Contract.DocAve;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.Contract.JobMonitor;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RMWeb.Audit;
using AvePoint.RA.Contract.RMWeb.Explorer;
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Explorer.Dao.CosmosImp;
using AvePoint.RA.DB.Model;
using AvePoint.RA.DB.SecurityTrimming;
using AvePoint.RA.I18N.Core;
using AvePoint.RA.Service.Services.Explorer.AuditHandler;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
//using AvePoint.Wrapper.Common;
//using AvePoint.RA.Common.Lock;

namespace AvePoint.RA.Service.Services.Explorer
{
    [Audit]
    public class WorkspaceHoldService : RMServiceBase, IWorkspaceHoldService
    {
        private RALogger logger = RALogger.GetInstance(typeof(IWorkspaceHoldService));

        protected readonly string LocalAzConnectStr = RMGlobalConfiguration.StorageConfig[RMStorageSettingKey.RECO_STORAGE_CONNECTION_STRING];
        #region Interface

        private IRMSecurityTrimmingHelper SecurityTrimmingHelper => PlatformWindsorManager.GetService<IRMSecurityTrimmingHelper>();
        public ILocationManagementService LocationManagementService => PlatformWindsorManager.GetService<ILocationManagementService>();
        public ITaxonomyService TaxonomyService => PlatformWindsorManager.GetService<ITaxonomyService>();
        public IRMChangeClassificationDao ChangeClassificationDao => PlatformWindsorManager.GetService<IRMChangeClassificationDao>();

        public IRMClassificationHistoryDao ClassificationHistoryDao => PlatformWindsorManager.GetService<IRMClassificationHistoryDao>();

        public ITermDao TermDao => PlatformWindsorManager.GetService<ITermDao>();
        public ILabelDao LabelDao => PlatformWindsorManager.GetService<ILabelDao>();
        //public IExplorerDao ExplorerDao { get; set; }

        public IHoldDao HoldDao => PlatformWindsorManager.GetService<IHoldDao>();
        public IHoldMembershipDao HoldMembershipDao => PlatformWindsorManager.GetService<IHoldMembershipDao>();

        public IAccountDao AccountDao => PlatformWindsorManager.GetService<IAccountDao>();
        private IGeneralSettingService mGeneralSettingService => PlatformWindsorManager.GetService<IGeneralSettingService>();
        //TODO add to config file
        private IRMScopeDao RMScopeDao => PlatformWindsorManager.GetService<IRMScopeDao>();
        public IRMScopeRoleAssignmentDao RMScopeRoleAssignmentDao => PlatformWindsorManager.GetService<IRMScopeRoleAssignmentDao>();
        public IGlobalSettingService GlobalSettingService => PlatformWindsorManager.GetService<IGlobalSettingService>();

        private RA.DB.Explorer.Dao.IExplorerDao _explorerDao;
        public RA.DB.Explorer.Dao.IExplorerDao ExplorerDao
        {
            get
            {
                if (_explorerDao == null)
                {
                    _explorerDao = new ExplorerDao();
                }
                return _explorerDao;
            }
        }
        private IGeneralSettingService GeneralSettingService => PlatformWindsorManager.GetService<IGeneralSettingService>();

        private IJobQueueService mJobQueueService => PlatformWindsorManager.GetService<IJobQueueService>();
        protected IJobMonitorService RMJobService => PlatformWindsorManager.GetService<IJobMonitorService>();

        protected IRMRuleDao RMRuleDao => PlatformWindsorManager.GetService<IRMRuleDao>();

        public IRMSubJobDao SubJobDao => PlatformWindsorManager.GetService<IRMSubJobDao>();

        public ITermRuleAssociationDao TermRuleAssociationDao => PlatformWindsorManager.GetService<ITermRuleAssociationDao>();

        public IRMPhysicalPushColumnDao RMPhysicalPushColumnDao => PlatformWindsorManager.GetService<IRMPhysicalPushColumnDao>();
        public IRMTemplateDao TemplateDao => PlatformWindsorManager.GetService<IRMTemplateDao>();

        public IPhysicalUniqueIdSettingDao PhysicalUniqueIdSettingDao => PlatformWindsorManager.GetService<IPhysicalUniqueIdSettingDao>();

        public IPermissionManagementService PermissionManagementService => PlatformWindsorManager.GetService<IPermissionManagementService>();
        public IFSConnectionDao FSConnectionDao => PlatformWindsorManager.GetService<IFSConnectionDao>();
        public IRMFileSystemBrowserService RMFileSystemBrowserService => PlatformWindsorManager.GetService<IRMFileSystemBrowserService>();
        public IExplorerQueryParamProcesser ExplorerQueryParamProcesser => PlatformWindsorManager.GetService<IExplorerQueryParamProcesser>();
        public IExplorerQueryService ExplorerQueryService => PlatformWindsorManager.GetService<IExplorerQueryService>();
        public IRecordsHistoryService RecordsHistoryService => PlatformWindsorManager.GetService<IRecordsHistoryService>();
        private IRMRemoteNodeService RMRemoteNodeService => PlatformWindsorManager.GetService<IRMRemoteNodeService>();
        private IRMMailboxService RMMailboxService => PlatformWindsorManager.GetService<IRMMailboxService>();
        private IWorkplaceHoldDao WorkplaceHoldDao => PlatformWindsorManager.GetService<IWorkplaceHoldDao>();
        protected IRMNodeFlagDao NodeFlagDao => PlatformWindsorManager.GetService<IRMNodeFlagDao>();

        #endregion
        public List<WorkplaceDto> GetWorkspadeByNodeLevel(GetWorkspaceRequestDto dto)
        {

            if (dto.SourceType == (int)RMBrowseTreeNodeSourceType.Exchange)
            {
                var mailBoxes = RMMailboxService.GetAllMailboxNodesWithId();
                if (mailBoxes != null)
                {
                    var result = new List<WorkplaceDto>(mailBoxes.Count);
                    foreach (var mb in mailBoxes)
                    {
                        result.Add(new WorkplaceDto
                        {
                            Id = mb.ObjectId,
                            Url = mb.Email,
                            SourceType = (int)dto.SourceType
                        });
                    }
                    return result;
                }
                return new List<WorkplaceDto>();
            }

            var webApplications = RMRemoteNodeService.GetRemoteSiteCollectionsByNodeLevel(dto.SourceType);
            if (webApplications != null)
            {
                var appResult = new List<WorkplaceDto>(webApplications.Count);
                foreach (var site in webApplications)
                {
                    Guid id = Guid.Empty;
                    if (!string.IsNullOrEmpty(site.id))
                    {
                        Guid.TryParse(site.id, out id);
                    }
                    appResult.Add(new WorkplaceDto
                    {
                        Id = site.id,
                        Url = site.url,
                        SourceType = dto.SourceType

                    });
                }
                return appResult;
            }

            return new List<WorkplaceDto>();
        }

        public async Task<RAReturnMessage> CreateWorkspaceHold(WorkspaceRequestDto dto)
        {
            RAReturnMessage msg = new RAReturnMessage
            {
                MessageType = RAMessageType.Successful
            };
            try
            {
                var userName = WebUtil.LogonUserDisplayName;
                if (dto != null)
                {
                    if (WorkplaceHoldDao.CheckWorkspaceHoldExist(dto))
                    {
                        msg.MessageType = RAMessageType.Failed;
                        msg.FaildType = RAFailedType.NameExisting;
                        msg.ErrorMessage = I18NEntity.GetString("FRM_JS_RDM_WorkplaceHold_Exist");
                        return msg;
                    }
                    var holdsetting = new HoldSetting()
                    {
                        Id = dto.HoldId,
                        Type = (HoldDateType)dto.WorkspaceHoldSettingDto.Type,
                        Number = dto.WorkspaceHoldSettingDto.Number,
                        Unit = (HoldDateUnit)dto.WorkspaceHoldSettingDto.Unit,
                        CalenderTime = dto.WorkspaceHoldSettingDto.CalenderTime,
                        IsDayLightSaving = dto.WorkspaceHoldSettingDto.IsDayLightSaving,
                        TimeZoneId = dto.WorkspaceHoldSettingDto.TimeZoneId
                    };
                    var releaseTime = CalculateHoldReleaseTime(holdsetting);
                    if (releaseTime.Ticks < DateTime.UtcNow.Ticks)
                    {
                        msg.MessageType = RAMessageType.Failed;
                        msg.ErrorMessage = I18NEntity.GetString("RM_PRM_PRE_Msg_BeforeCurrentTime");
                        return msg;
                    }
                    var workplaceHold = new RMWorkspaceHold()
                    {
                        Id = Guid.NewGuid().ToString(),
                        WorkplaceId = dto.WorkplaceId,
                        HoldId = dto.HoldId,
                        HoldBy = userName,
                        SourceType = dto.SourceType,
                        ReleaseTime = releaseTime.Ticks
                    };
                    bool isSaveHoldSuccess = WorkplaceHoldDao.SaveWorkspaceHold(workplaceHold);
                    if (!isSaveHoldSuccess)
                    {
                        msg.MessageType = RAMessageType.Failed;
                        msg.ErrorMessage = "Save hold failed";
                        return msg;
                    }
                }
                return msg;
            }
            catch (Exception ex)
            {
                logger.Error("save workplace hold and update record error:{0}", ex.ToString());
                msg.MessageType = RAMessageType.Failed;
                msg.ErrorMessage = ex.Message;
            }
            return msg;
        }
        public async Task<RAReturnMessage> UpdateWorkspaceHoldAsync(WorkspaceHoldUpdateDto dto)
        {
            RAReturnMessage msg = new RAReturnMessage
            {
                MessageType = RAMessageType.Successful
            };
            var userName = WebUtil.LogonUserDisplayName;
            try
            {
                if (dto == null || string.IsNullOrEmpty(dto.Id))
                {
                    msg.MessageType = RAMessageType.Failed;
                    msg.ErrorMessage = "Invalid parameters";
                    return msg;
                }
                var holdsetting = new HoldSetting()
                {
                    Id = dto.HoldId,
                    Type = (HoldDateType)dto.WorkspaceHoldSettingDto.Type,
                    Number = dto.WorkspaceHoldSettingDto.Number,
                    Unit = (HoldDateUnit)dto.WorkspaceHoldSettingDto.Unit,
                    CalenderTime = dto.WorkspaceHoldSettingDto.CalenderTime,
                    IsDayLightSaving = dto.WorkspaceHoldSettingDto.IsDayLightSaving,
                    TimeZoneId = dto.WorkspaceHoldSettingDto.TimeZoneId
                };
                var releaseTime = CalculateHoldReleaseTime(holdsetting);
                var workplaceHold = new RMWorkspaceHold
                {
                    Id = dto.Id,
                    HoldId = dto.HoldId,
                    HoldBy = userName,
                    ReleaseTime = releaseTime.Ticks

                };

                var updateSuccess = await WorkplaceHoldDao.UpdateWorkspaceHoldAsync(workplaceHold);
                if (!updateSuccess)
                {
                    msg.MessageType = RAMessageType.Failed;
                    msg.ErrorMessage = "Update workspace hold failed";
                }
                return msg;
            }
            catch (Exception ex)
            {
                logger.Error("update workspace hold error:{0}", ex.ToString());
                msg.MessageType = RAMessageType.Failed;
                msg.ErrorMessage = ex.Message;
            }

            return msg;
        }

        public async Task<RAReturnMessage> DeleteWorkspaceHoldsAsync(List<string> workspaceHoldIds)
        {
            RAReturnMessage msg = new RAReturnMessage
            {
                MessageType = RAMessageType.Successful
            };
            try
            {
                if (workspaceHoldIds == null || !workspaceHoldIds.Any())
                {
                    msg.MessageType = RAMessageType.Failed;
                    msg.ErrorMessage = "Invalid parameters";
                    return msg;
                }

                await WorkplaceHoldDao.DeleteWorkspaceHolds(workspaceHoldIds);

            }
            catch (Exception ex)
            {
                logger.Error("delete workspace hold error:{0}", ex.ToString());
                msg.MessageType = RAMessageType.Failed;
                msg.ErrorMessage = ex.Message;
            }

            return msg;
        }

        public async Task<List<WorkspaceHoldItemDto>> GetWorkspaceHoldsByPageSizeAsync()
        {
            var items = await WorkplaceHoldDao.GetWorkspaceHoldsByPageSizeAsync();
            if (items == null || !items.Any())
            {
                return new List<WorkspaceHoldItemDto>();
            }

            var generalSetting = await GeneralSettingService.GetGeneralSettingAsync();
            var currentUtcTicks = DateTime.UtcNow.Ticks;

            foreach (var item in items)
            {
                item.IsChecked = false;

                if (long.TryParse(item.ReleaseTime, out long releaseTicks))
                {
                    item.IsHold = releaseTicks > currentUtcTicks;
                    item.ReleaseTime = GeneralSettingService
                        .ConvertTiksToDateTime(generalSetting, releaseTicks, true)
                        .SimplifyFormatTime;
                }
                else
                {
                    item.IsHold = false;
                    item.ReleaseTime = string.Empty;
                }
            }

            return items;
        }
        public async Task<RAReturnMessage> RunImportWorkspaceHoldJobAsync(JobRunBy jobRunBy, string blobName)
        {
            var returnMessage = new RAReturnMessage();
            try
            {
                var groupId = TenantLocalValue.LogonGroupId;
                var loginName = TenantLocalValue.LogonUserEmail;
                JobQueueDto jqDto = new JobQueueDto()
                {
                    JobType = JobType.ImportWorkspaceHold,
                    JobRunType = jobRunBy,
                    TenantGroupId = groupId,
                    JobRunByUser = loginName,
                    Parameters = blobName,
                };
                returnMessage.MessageType = RAMessageType.Successful;
                returnMessage.Extension = mJobQueueService.AddToDBJobQueue(jqDto);
            }
            catch (Exception ex)
            {
                logger.Error("error occurred while RunImportWorkspaceHoldJobAsync,ERROR:{0}", ex.ToString());
                returnMessage.MessageType = RAMessageType.Failed;
                returnMessage.ErrorMessage = ex.Message;
            }
            return returnMessage;
        }

        [Audit(Module = AuditModule.PhysicalRecordManagement, Category = AuditCategory.ManageHold, Action = AuditAction.ImportWorkspaceHold, BeforeHandler = typeof(ExplorerBeforeAuditHandler), AfterHandler = typeof(ExplorerAfterAuditHandler))]
        public async Task<string> RealRunImportWorkspaceHoldJobAsync(string blobName)
        {
            logger.Info("RealRunImportWorkspaceHoldJobAsync start.");
            string jobId = string.Empty;
            try
            {
                var account = await AccountDao.GetActiveUserByNameAsync(TenantLocalValue.LogonUserEmail);

                jobId = RMJobService.CreateJob(JobType.ImportWorkspaceHold, TenantLocalValue.LogonUserEmail, account.UserId);

                SubJobDao.UpdateSubJobCount(jobId, 1);

                var subJobId = CreateSubJob(jobId, 0, JobType.ImportWorkspaceHold, JobStatus.InProgress, 1, blobName);

                mJobQueueService.HandleMessage(new JobQueueMessage()
                {
                    JobId = subJobId,
                    RunBy = JobRunBy.Control,
                    JobType = JobType.ImportWorkspaceHold,
                    CommandLine = string.Format("{0} {1} {2} {3}", JobType.ImportWorkspaceHold, subJobId, jobId, blobName)
                });

                logger.Info($"RealRunImportWorkspaceHoldJobAsync end. JobId: {jobId}.");
            }
            catch (Exception ex)
            {
                logger.Error($"Error in RealRunImportWorkspaceHoldJobAsync, reason: {ex}.");
            }
            return jobId;
        }
        private DateTime CalculateHoldReleaseTime(HoldSetting hold)
        {
            if (hold.Type == HoldDateType.Custom)
            {
                DateTime tempNow = new DateTime();
                if (hold.Unit == HoldDateUnit.Day)
                {
                    tempNow = DateTime.UtcNow.AddDays(hold.Number);
                }
                else if (hold.Unit == HoldDateUnit.Week)
                {
                    tempNow = DateTime.UtcNow.AddDays(7 * hold.Number);
                }
                else if (hold.Unit == HoldDateUnit.Month)
                {
                    tempNow = DateTime.UtcNow.AddMonths(hold.Number);
                }
                else if (hold.Unit == HoldDateUnit.Years)
                {
                    tempNow = DateTime.UtcNow.AddYears(hold.Number);
                }
                return tempNow;
            }
            else
            {
                DateTime calenderTime = DateTime.Parse(hold.CalenderTime);
                calenderTime = DateTime.SpecifyKind(calenderTime, DateTimeKind.Unspecified);
                DateTime utcTime = DateTimeUtil.ConvertTimeToUtcDate(calenderTime, GeneralSettingConfig.FindSystemTimeZoneById(hold.TimeZoneId), !hold.IsDayLightSaving);
                return utcTime;
            }
        }
        private string CreateSubJob(string jobId, int currentSubjobIndex, JobType jobType, JobStatus jobState, int subJobCount, string jobMessage, string string1 = null)
        {
            string subJobId = string.Format(jobId + "_{0:D3}", currentSubjobIndex);
            var subJob = new RMSubJob()
            {
                Id = subJobId,
                ParentId = jobId,
                StartTime = DateTime.UtcNow.Ticks,
                JobType = (int)jobType,
                Progress = 0,
                Status = (int)jobState,
                Weight = 100d / subJobCount,
                String1 = string1,
                LastUpdateTime = DateTime.UtcNow.Ticks,
                Runable = jobState == JobStatus.InProgress ? RecordsConstants.SubJob_Runnable_Runing : RecordsConstants.SubJob_Runnable_Waiting
            };
            subJob.JobContext = new RMJobContext() { JobId = subJobId, Content = jobMessage };
            SubJobDao.CreateJob(subJob);
            logger.Info("Create sub job {0} sucessfull, type {1}, weight {2}, state {3}, string1 {4} ", subJob.Id, subJob.JobType, subJob.Weight, subJob.Status, string1);
            return subJobId;
        }
    }
}