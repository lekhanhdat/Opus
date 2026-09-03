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
using AvePoint.GCommon.Contract.Server.ControlPanel.Object;
using AvePoint.GCommon.Utility;
using AvePoint.RA.Common;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Audit;
using AvePoint.RA.Contract.CloudService;
using AvePoint.RA.Contract.DocAve;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.Contract.JobMonitor;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.PersonalSetting;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RMWeb.Audit;
using AvePoint.RA.Contract.RMWeb.Dashboard;
using AvePoint.RA.Contract.RMWeb.Explorer;
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using AvePoint.RA.Contract.RMWeb.Physical;
using AvePoint.RA.Contract.RMWeb.Physical.ColumnValues;
using AvePoint.RA.Contract.RMWeb.Tree.Base;
using AvePoint.RA.Contract.TemplateManagement;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Dao.Impl;
using AvePoint.RA.DB.Explorer.Dao;
using AvePoint.RA.DB.Explorer.Dao.CosmosImp;
using AvePoint.RA.DB.Explorer.Model;
using AvePoint.RA.DB.Model;
using AvePoint.RA.I18N.Core;
using AvePoint.RA.RAPhysical.Disposal.PhysicalDisposalActionImps;
using AvePoint.RA.Service.JobMonitor;
using AvePoint.RA.Service.LocationManagement;
using AvePoint.RA.Service.Services.PickList.AuditHandler;
using AvePoint.RA.Service.SharePointSetting;
using DocumentFormat.OpenXml.Spreadsheet;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.Service.Services.PickList
{
    [Audit]
    public class PickListService: RMServiceBase, IPickListService
    {
        private RALogger logger = RALogger.GetInstance(typeof(PickListService));
        private RA.DB.Explorer.Dao.IExplorerDao _explorerDao;
        public RA.DB.Explorer.Dao.IExplorerDao ExplorerDao
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

        private IExplorerQueryService ExplorerQueryService => PlatformWindsorManager.GetService<IExplorerQueryService>();
        private IJobQueueService JobQueueService => PlatformWindsorManager.GetService<IJobQueueService>();
        private IGeneralSettingService GeneralSettingService => PlatformWindsorManager.GetService<IGeneralSettingService>();
        private IJobMonitorService RMJobService => PlatformWindsorManager.GetService<IJobMonitorService>();
        private IAccountDao AccountDao => PlatformWindsorManager.GetService<IAccountDao>();
        private IRMSubJobDao SubJobDao => PlatformWindsorManager.GetService<IRMSubJobDao>();
        private IDownloadDataInfoDao DownloadDataInfoDao => PlatformWindsorManager.GetService<IDownloadDataInfoDao>();
        private IRMLocationDao LocationDao => PlatformWindsorManager.GetService<IRMLocationDao>();
        private ILocationManagementService LocationManagementService => PlatformWindsorManager.GetService<ILocationManagementService>();

        public async Task<PickListLoanResultDto> QueryPickLoanListAsync(PickListLoanParam pickListLoanParam)
        {
            PickListLoanResultDto resultDto = new()
            {
                List = new List<PickListLoanDto>()
            };
            var pageIndex = pickListLoanParam.PageIndex;
            var pageSize = pickListLoanParam.PageSize;
            var searchText = pickListLoanParam.SearchText;
            var filterOptions = pickListLoanParam.FilterOptions;
            ExplorerQueryV3Dto dto = GetLoanQueryDto(pageIndex, pageSize, searchText, filterOptions);

            var resultInfo = await ExplorerQueryService.QueryDataListWithTotalAsync(dto);
            resultDto.TotalCount = resultInfo.PagingInfo.Total;
            resultDto.PageIndex = resultInfo.PagingInfo.PageIndex;
            
            //var gls = GeneralSettingService.GetGeneralSettingAsync();
            
            foreach (var record in resultInfo.Datas)
            {
                try
                {
                    resultDto.List.Add(new PickListLoanDto()
                    {
                        Id = record.Id,
                        NodeType = record.NodeType,
                        HomeLocation = GetPhysicalObjectFullPath(record),
                        RecordName = record.LeafName,
                        Requestor = record.PersonHoldBy,
                        Status = record.LoanPickStatus,
                        UniqueId = record.RecordsId
                    });
                }
                catch (Exception e)
                {
                    logger.Warn($"Get pick list data error, {e}");
                }
            }
            return resultDto;
        }

        public ExplorerQueryV3Dto GetLoanQueryDto(string pageIndex, int pageSize, string searchText, PickFilterOption filterOptions)
        {
            var dto = new ExplorerQueryV3Dto()
            {
                QueryOption = new ExplorerQueryOptionV3()
                {
                    Values = new List<ExplorerSearchOptionV3>
                    {
                        new ExplorerSearchOptionV3
                        {
                            Column = new ExplorerQueryColumn
                            {
                                Id = QueryCloumnIds.SourceFlag,
                            },
                            Value = JsonConvert.SerializeObject(new List<SourceFlag> { SourceFlag.Physical })
                        },
                        new ExplorerSearchOptionV3
                        {
                            Column = new ExplorerQueryColumn
                            {
                                Id = QueryCloumnIds.FileExtension,
                            },
                            Value = JsonConvert.SerializeObject(new List<string> { ((int)RMNodeLevel.PhysicalCustom).ToString(), ((int)RMNodeLevel.PhysicalBox).ToString(), ((int)RMNodeLevel.PhysicalFile).ToString() })
                        },
                        new ExplorerSearchOptionV3
                        {
                            Column = new ExplorerQueryColumn { Id = QueryCloumnIds.Loan },
                            Value = JsonConvert.SerializeObject(new DateInfo
                            {
                                Condition = DateCondition.All,
                            })
                        }
                    }
                },
                PagingInfo = new ExplorerPagingInfo
                {
                    PageIndex = pageIndex,
                    PageSize = pageSize,
                }
            };
            if (!string.IsNullOrEmpty(searchText))
            {
                dto.QueryOption.Values.Add(new ExplorerSearchOptionV3
                {
                    Column = new ExplorerQueryColumn { Id = RecordBuildInColumnIds.NameOrTitle },
                    Value = JsonConvert.SerializeObject(searchText)
                });
            }
            if (filterOptions?.Status?.Count > 0)
            {
                dto.QueryOption.Values.Add(new ExplorerSearchOptionV3
                {
                    Column = new ExplorerQueryColumn { Id = QueryCloumnIds.LoanPickStatus },
                    Value = JsonConvert.SerializeObject(filterOptions.Status)
                });
            }

            return dto;
        }

        public async Task<PickListDestructionResultDto> QueryPickDestructionListAsync(PickListDestructionParam pickListDestructionParam)
        {
            PickListDestructionResultDto resultDto = new()
            {
                List = new List<PickListDestructionDto>()
            };

            var pageIndex = pickListDestructionParam.PageIndex;
            var pageSize = pickListDestructionParam.PageSize;
            var searchText = pickListDestructionParam.SearchText;
            var filterOptions = pickListDestructionParam.FilterOptions;
            ExplorerQueryV3Dto dto = GetDestructionQueryDto(pageIndex, pageSize, searchText, filterOptions);

            var resultInfo = await ExplorerQueryService.QueryDataListWithTotalAsync(dto);
            resultDto.TotalCount = resultInfo.PagingInfo.Total;
            resultDto.PageIndex = resultInfo.PagingInfo.PageIndex;


            List<int> userIdList = resultInfo.Datas.Select(r => r.ManualApprovedBy).ToList();

            List<RMAccount> accounts = new List<RMAccount>();
            if (userIdList.Count > 0)
            {
                accounts = await this.GetAccount(userIdList);
            }
            var gls = await GeneralSettingService.GetGeneralSettingAsync();

            foreach (var record in resultInfo.Datas)
            {
                var manualApprovedBy = accounts.FirstOrDefault(a => a.Id == record.ManualApprovedBy);
                try
                {
                    resultDto.List.Add(new PickListDestructionDto()
                    {
                        Id = record.Id,
                        NodeType = record.NodeType,
                        HomeLocation = GetPhysicalObjectFullPath(record),
                        RecordName = record.LeafName,
                        Status = record.DestructionPickStatus,
                        UniqueId = record.RecordsId,
                        Classification = record.TermName,
                        TermId = record.TermId.ToString(),
                        ApprovedBy = manualApprovedBy?.DisplayName,
                        DestroyedDate = record.DestryoedTime == 0 ? "" : GeneralSettingService.ConvertTiksToDateTime(gls, record.DestryoedTime, true).SimplifyFormatTime
                    });
                }
                catch (Exception e)
                {
                    logger.Warn($"Get pick list data error, {e}");
                }
            }
            return resultDto;
        }

        public ExplorerQueryV3Dto GetDestructionQueryDto(string pageIndex, int pageSize, string searchText, PickFilterOption filterOptions)
        {
            var dto = new ExplorerQueryV3Dto()
            {
                QueryOption = new ExplorerQueryOptionV3()
                {
                    Values = new List<ExplorerSearchOptionV3>
                    {
                        new ExplorerSearchOptionV3
                        {
                            Value = JsonConvert.SerializeObject(new List<SourceFlag> { SourceFlag.Physical }),
                            Column = new ExplorerQueryColumn
                            {
                                Id = QueryCloumnIds.SourceFlag,
                            }
                        },
                        new ExplorerSearchOptionV3
                        {
                            Column = new ExplorerQueryColumn
                            {
                                Id = QueryCloumnIds.FileExtension,
                            },
                            Value = JsonConvert.SerializeObject(new List<string> { ((int)RMNodeLevel.PhysicalCustom).ToString(), ((int)RMNodeLevel.PhysicalBox).ToString(), ((int)RMNodeLevel.PhysicalFile).ToString() })
                        },
                        new ExplorerSearchOptionV3
                        {
                            Value = JsonConvert.SerializeObject(new List<ChoiceColumnValue>() { new ChoiceColumnValue() { Value = ((int)RMRecordStatus.Destroyed).ToString() } }),
                            Column = new ExplorerQueryColumn
                            {
                                Id = DefaultColumnIDs.Status,
                                Type = Contract.TemplateManagement.ColumnType.SingleChoice
                            },
                        }
                    }
                },
                PagingInfo = new ExplorerPagingInfo
                {
                    PageIndex = pageIndex,
                    PageSize = pageSize,
                }
            };
            if (!string.IsNullOrEmpty(searchText))
            {
                dto.QueryOption.Values.Add(new ExplorerSearchOptionV3
                {
                    Column = new ExplorerQueryColumn { Id = RecordBuildInColumnIds.NameOrTitle },
                    Value = JsonConvert.SerializeObject(searchText)
                });
            }
            if (filterOptions?.Status?.Count > 0)
            {
                dto.QueryOption.Values.Add(new ExplorerSearchOptionV3
                {
                    Column = new ExplorerQueryColumn { Id = QueryCloumnIds.DestructionPickStatus },
                    Value = JsonConvert.SerializeObject(filterOptions.Status)
                });
            }

            return dto;
        }

        [Audit(Action = AuditAction.PhysicalLoanPickComplete, Category = AuditCategory.PhysicalRecordsExplorer, 
            Module = AuditModule.PhysicalRecordManagement, AfterHandler = typeof(PickListAfterAuditHandler), BeforeHandler = typeof(PickListBeforeAuditHandler))]
        public RAReturnMessage UpdatePickStatusCompelte(CompleteActionParam param, PickObjectType objectType)
        {
            RAReturnMessage returnMessage = new();
            List<Guid> errorItems = new();
            param.SelectedItemIds ??= new List<Guid>();
            var records = ExplorerDao.QueryAll(s => param.SelectedItemIds.Contains(s.Id));
            foreach (var rec in records)
            {
                try
                {
                    if (rec == null)
                    {
                        continue;
                    }
                    if (objectType == PickObjectType.Loan)
                    {
                        if (rec.LoanPickStatus == (int)PickStatusType.Pendding)
                        {
                            ExplorerDao.UpdateAll(s => s.Id == rec.Id, r => { r.LoanPickStatus = (int)PickStatusType.Complete; });
                        }
                    }
                    else if (objectType == PickObjectType.Destruction)
                    {
                        if (rec.DestructionPickStatus == (int)PickStatusType.Pendding)
                        {
                            ExplorerDao.UpdateAll(s => s.Id == rec.Id, r => { r.DestructionPickStatus = (int)PickStatusType.Complete; });
                        }
                    }
                }
                catch (Exception e)
                {
                    errorItems.Add(rec.Id);
                    logger.Warn($"Update loan pick status error:{e}");
                }
            }
            if (errorItems.Count > 0)
            {
                returnMessage.MessageType = RAMessageType.Failed;
                returnMessage.Extension = JsonConvert.SerializeObject(errorItems);
            }
            return returnMessage;
        }

        public RAReturnMessage StartJob(CompleteActionParam param, PickObjectType objectType, PickActionType pickActionType)
        {
            RAReturnMessage returnMessage = new();
            try
            {
                var jobType = JobType.None;
                var obb = new PickListStartJobDto { ObjectType = objectType, PickActionType = pickActionType };
                jobType = obb switch
                {
                    { ObjectType: PickObjectType.Loan, PickActionType: PickActionType.Complete } => JobType.PhysicalLoanPick,
                    { ObjectType: PickObjectType.Destruction, PickActionType: PickActionType.Complete } => JobType.PhysicalDestructionPick,
                    { ObjectType: PickObjectType.Loan, PickActionType: PickActionType.Export } => JobType.PhysicalLoanPickExportJob,
                    { ObjectType: PickObjectType.Destruction, PickActionType: PickActionType.Export } => JobType.PhysicalDestructionPickExportJob,
                    { ObjectType: PickObjectType.ReturnHistory, PickActionType: PickActionType.Export } => JobType.PhysicalReturnHistoryExport,
                    { ObjectType: PickObjectType.Move, PickActionType: PickActionType.Export } => JobType.PhysicalMovePickExportJob,
                    _ => throw new ArgumentOutOfRangeException(nameof(obb), $"Not expected start job dto"),
                };


                var groupId = TenantLocalValue.LogonGroupId;
                var loginName = TenantLocalValue.LogonUserEmail;
                JobQueueDto jqDto = new JobQueueDto()
                {
                    JobType = jobType,
                    Parameters = SerializerHelper.SerializeByDataContractSerializer(new PickListJobMessage() { ActionParam = param, LogonUserId = TenantLocalValue.LogonUserId }),
                    JobRunType = JobRunBy.Control,
                    TenantGroupId = groupId,
                    JobRunByUser = loginName
                };
                returnMessage.MessageType = RAMessageType.Successful;
                returnMessage.Extension = JobQueueService.AddToDBJobQueue(jqDto);
            }
            catch (Exception ex)
            {
                returnMessage.MessageType = RAMessageType.Failed;
                returnMessage.ErrorMessage = ex.Message;
            }
            return returnMessage;
        }

        private Task<List<RMAccount>> GetAccount(List<string> userIds)
        {
            return AccountDao.GetUserByUserIdsAsync(userIds);
        }
        private Task<List<RMAccount>> GetAccount(List<int> userIds)
        {
            return AccountDao.GetUserByIdsAsync(userIds);
        }

        [Audit(Action = AuditAction.PhysicalLoanPickCompleteJob, Category = AuditCategory.PhysicalRecordsExplorer,
            Module = AuditModule.PhysicalRecordManagement, AfterHandler = typeof(PickListAfterAuditHandler), BeforeHandler = typeof(PickListBeforeAuditHandler))]
        public async Task<string> RealRunStartPickCompleteJobAsync(JobType jobType, string param, string logonUserId)
        {
            var moveListJob = new List<JobType> { JobType.PhysicalMovePickExportJob };
            string jobId = string.Empty;
            string jobRunByUser = TenantLocalValue.LogonUserEmail;
            try
            {
                var pickListExportJob = new List<JobType>() { JobType.PhysicalDestructionPickExportJob, JobType.PhysicalLoanPickExportJob };
                if (pickListExportJob.Contains(jobType))
                {
                    var account = await AccountDao.GetActiveUserByNameAsync(TenantLocalValue.LogonUserEmail);
                    jobId = RMJobService.CreateJob(jobType, jobRunByUser, account.UserId);
                }
                else
                {
                    jobId = RMJobService.CreateJob(jobType, jobRunByUser);
                }
                SubJobDao.UpdateSubJobCount(jobId, 1);
                string userId;
                if (moveListJob.Contains(jobType))
                {
                    userId = SerializerHelper.DeserializeByDataContractSerializer<PickMoveListJobMessage>(param).LogonUserId;
                }
                else
                {
                    userId = SerializerHelper.DeserializeByDataContractSerializer<PickListJobMessage>(param).LogonUserId;
                }
                var subJobId = CreateSubJob(jobId, 0, jobType, JobStatus.InProgress, 1, param);

                if (new JobType[] { JobType.PhysicalLoanPickExportJob, JobType.PhysicalDestructionPickExportJob, JobType.PhysicalReturnHistoryExport, JobType.PhysicalMovePickExportJob }.Contains(jobType))
                {
                    //Export Job
                    JobQueueService.HandleMessage(new JobQueueMessage()
                    {
                        JobId = subJobId,
                        RunBy = JobRunBy.Control,
                        JobType = jobType,
                        CommandLine = string.Format("{0} {1}", jobType.ToString(), subJobId),
                    });

                    logger.Info(string.Format("Finished add job to job queue, job id is : {0}", subJobId));
                    DownloadDataInfoDao.Create(new RMDownloadDataInfo()
                    {
                        FileDownloadTime = DateTime.UtcNow.Ticks,
                        JobId = jobId,
                        RecordsId = Guid.NewGuid(),
                        JobStatus = (int)DownloadContentJobStatus.Wait,
                        UserId = userId,
                        Name = jobId + ".zip",
                        DownloadType = jobType switch
                        {
                            JobType.PhysicalLoanPickExportJob => DownloadContentType.LoanPickListContent,
                            JobType.PhysicalDestructionPickExportJob => DownloadContentType.DestructionPickListContent,
                            JobType.PhysicalReturnHistoryExport => DownloadContentType.ReturnLoanHistory,
                            JobType.PhysicalMovePickExportJob => DownloadContentType.MovePickListContent,
                            _ => throw new ArgumentOutOfRangeException(nameof(jobType), $"Not expected job type {jobType}"),
                        }
                    });
                }
                else
                {
                    List<string> runningJobs = RMJobService.GetRunningJobs(jobType);
                    bool isSkip = runningJobs.Any(j => j != jobId);
                    if (!isSkip)
                    {
                        JobQueueService.HandleMessage(new JobQueueMessage()
                        {
                            JobId = subJobId,
                            RunBy = JobRunBy.Control,
                            JobType = jobType,
                            CommandLine = string.Format("{0} {1}", jobType.ToString(), subJobId),
                        });

                        logger.Info(string.Format("Finished add job to job queue, job id is : {0}", subJobId));
                    }
                    else
                    {
                        logger.Info("Skipped this job. A pick list job is already running.");
                        RMJobService.UpdateJobStatus(jobId, JobStatus.Skipped, "RM_SYNC_JobSkip");
                    }
                }
                }
            catch (Exception ex)
            {
                logger.Error($"Error in RealRunStartPickCompleteJob, reason : {ex.ToString()}.");
            }
            return jobId;
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
                LastUpdateTime = DateTime.UtcNow.Ticks
            };
            if (jobState == JobStatus.Wait)
            {
                subJob.Runable = RecordsConstants.SubJob_Runnable_CanRun;
            }
            subJob.JobContext = new RMJobContext() { JobId = subJobId, Content = jobMessage };
            SubJobDao.CreateJob(subJob);
            logger.Info("Create sub job {0} sucessfull, type {1}, weight {2}, state {3}, string1 {4} ", subJob.Id, subJob.JobType, subJob.Weight, subJob.Status, string1);
            return subJobId;
        }


        private string GetPhysicalObjectFullPath(BaseRecordDto oPhy, bool isReplaceI18NKey = true)
        {
            var path = new StringBuilder();
            try
            {
                if (oPhy != null)
                {
                    path.Append(LocationManagementService.GetLocationPathById(oPhy.LocationId, isReplaceI18NKey));
                }

                if (oPhy.Ancestors != null) return GetPhysicalLocationFullPathByAncestors(oPhy, path.ToString(), ExplorerDao); //new format data

                //old format data
                if (oPhy.NodeType != (int)RMNodeType.PhyBox)
                {
                    if (oPhy.BoxId != Guid.Empty)
                    {
                        var parentBox = ExplorerDao.QueryAll(r => r.Id == oPhy.BoxId).FirstOrDefault();
                        path.Append($"/{parentBox?.LeafName}");
                    }
                    if (oPhy.NodeType == (int)RMNodeType.PhyRecord)
                    {
                        var parentFile = ExplorerDao.QueryAll(r => r.Id == oPhy.FileId).FirstOrDefault();
                        path.Append($"/{parentFile?.LeafName}");
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error($"Get PhysicalObject FullPath by id: [{oPhy.Id}], error: [{ex.ToString()}]");
            }
            return path.ToString();
        }

        private static string GetPhysicalLocationFullPathByAncestors(BaseRecordDto record, string locationPath, IExplorerDao ExplorerDao)
        {
            if (record.Ancestors == null || record.Ancestors.Count == 1) return locationPath;
            Guid[] ancestors = new Guid[record.Ancestors.Count - 1];
            record.Ancestors.CopyTo(1, ancestors, 0, record.Ancestors.Count - 1);//first one is location id,  do not need it
            var path = new StringBuilder(locationPath);
            var dic = ExplorerDao.QueryAll(o => Enumerable.Contains(ancestors, o.Id)).Select(o => new { o.Id, o.LeafName }).ToDictionary(o => o.Id);
            foreach (var r in ancestors)
            {
                path.Append($"/{dic[r].LeafName}");
            }

            return path.ToString();
        }

        public RAReturnMessage StartMoveJob(PickMoveListParam param, PickActionType pickActionType)
        {
            RAReturnMessage returnMessage = new();
            try
            {
                var jobType = JobType.None;
                var obb = new PickListStartJobDto { ObjectType = PickObjectType.Move, PickActionType = pickActionType };
                jobType = obb switch
                {
                    { ObjectType: PickObjectType.Move, PickActionType: PickActionType.Export } => JobType.PhysicalMovePickExportJob,
                    _ => throw new ArgumentOutOfRangeException(nameof(obb), $"Not expected start job dto"),
                };


                var groupId = TenantLocalValue.LogonGroupId;
                var loginName = TenantLocalValue.LogonUserEmail;
                JobQueueDto jqDto = new JobQueueDto()
                {
                    JobType = jobType,
                    Parameters = SerializerHelper.SerializeByDataContractSerializer(new PickMoveListJobMessage() { ActionParam = param, LogonUserId = TenantLocalValue.LogonUserId }),
                    JobRunType = JobRunBy.Control,
                    TenantGroupId = groupId,
                    JobRunByUser = loginName
                };
                returnMessage.MessageType = RAMessageType.Successful;
                returnMessage.Extension = JobQueueService.AddToDBJobQueue(jqDto);
            }
            catch (Exception ex)
            {
                returnMessage.MessageType = RAMessageType.Failed;
                returnMessage.ErrorMessage = ex.Message;
            }
            return returnMessage;
        }
    }
}
