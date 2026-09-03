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
using AvePoint.GCommon;
using AvePoint.GCommon.Contract.Storage.Entity;
using AvePoint.GCommon.Contract.StorageOptimization.Object;
using AvePoint.Hybrid.ClientLibrary.Data;
using AvePoint.Hybrid.Contract;
using AvePoint.Hybrid.Contract.DTOs;
using AvePoint.Hybrid.Contract.Object;
using AvePoint.Hybrid.Utility;
using AvePoint.RA.Common.Global.Utils;
using AvePoint.RA.Common.TransientFault;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Common;
using AvePoint.RA.Contract.DataIngestion;
using AvePoint.RA.Contract.Discovery;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.Contract.FileSystem;
using AvePoint.RA.Contract.Global.JobMessage;
using AvePoint.RA.Contract.Global.Object;
using AvePoint.RA.Contract.OnPremiseSharePoint;
using AvePoint.RA.Contract.Services;
using AvePoint.RA.Contract.Telemetry;
using RAFileSystem.FileSystem.FileSystem.Backup.Param;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace AvePoint.RA.Common.Hybrid
{
    /// <summary>
    /// 给Hybrid Agent使用, CustomerId以及Ids信息来自于Agent安装信息.
    /// </summary>
    public class HybridApiClient : HybridSdk
    {
        public static readonly AveRetryPolicy ApiRetryPolicy = new AveRetryPolicy(new AveTransientErrorCatchAllStrategy(), new FixedIntervalRetryStrategy(3, TimeSpan.FromSeconds(3)));
        protected static readonly AveLogger mLogger = AveLogger.GetInstance(typeof(HybridApiClient));
        private static object locker = new object();
        private HybridApiClient() : base()
        {
        }

        private static HybridApiClient hybridApiClient = null;
        public static HybridApiClient Instance
        {
            get
            {
                if (hybridApiClient == null)
                {
                    lock (locker)
                    {
                        if (hybridApiClient == null)
                        {
                            hybridApiClient = new HybridApiClient();
                        }
                    }
                }

                return hybridApiClient;
            }
        }

        public static void ReInitHybridApiClient()
        {
            hybridApiClient = new HybridApiClient();
        }

        public bool UpdateJobProgress(HBJobStatusInfo hBJobStatusInfo)
        {
            return ApiRetryPolicy.ExecuteAction(() =>
            {
                return this.HybridClient.JobMonitorService.UpdateJobProcess(hBJobStatusInfo).GetAwaiter().GetResult();
            });
        }

        public ServiceStatus GetAgentStatus(AgentInfo agentInfo)
        {
            return ApiRetryPolicy.ExecuteAction(() =>
            {
                return this.HybridClient.AgentMgmtService.GetAgentStatus(agentInfo).GetAwaiter().GetResult();
            });
        }

        public bool UpdateJobState(HBJobStatusInfo hBJobStatusInfo)
        {
            return ApiRetryPolicy.ExecuteAction(() =>
            {
                return this.HybridClient.JobMonitorService.UpdateJobState(hBJobStatusInfo).GetAwaiter().GetResult();
            });
        }

        public bool UpdateJobState(string jobId, int state, string comment)
        {
            return ApiRetryPolicy.ExecuteAction(() =>
            {
                return this.HybridClient.JobMonitorService.UpdateJobState(new HBJobStatusInfo()
                { JobId = jobId, State = state, Comment = comment }).GetAwaiter().GetResult();
            });
        }

        public void SendReport(HBReportInfo report)
        {

            ApiRetryPolicy.ExecuteAction(() =>
            {
                return this.HybridClient.JobMonitorService.SendReport(report).GetAwaiter().GetResult();
            });
        }
        public void DeleteJobById(string jobId)
        {
            ApiRetryPolicy.ExecuteAction(() =>
            {
                this.HybridClient.JobMonitorService.DeleteJobForAgentById(jobId).GetAwaiter().GetResult();
            });
        }
        public int GetJobState(string jobId)
        {
            return ApiRetryPolicy.ExecuteAction(() =>
            {
                return this.HybridClient.JobMonitorService.GetJobState(jobId).GetAwaiter().GetResult();
            });
        }

        public string GetJobMessage(string jobId, JobType jobType)
        {
            return RetryUtility.RetryAlways(() =>
            {
                return this.HybridClient.RecordsJobService.GetJobMessage(new JobInfo() { JobId = jobId, JobType = jobType}).GetAwaiter().GetResult();
            }, 2, 3000);
        }
        public string GetRetentionUnit(string countryCode, int retentionType,string termId)
        {
            return RetryUtility.RetryAlways(() =>
            {
                return this.HybridClient.RecordsJobService.GetRetentionUnit(new ApplyClassCodeDto() { CountryCode = countryCode, RetentionType = retentionType, TermId = termId}).GetAwaiter().GetResult();
            }, 2, 3000);
        }
        public string GetDisposalJobMessage(string jobId)
        {
            return RetryUtility.RetryAlways(() =>
            {
                return this.HybridClient.RecordsJobService.GetDisposalJobMessage(new JobInfo() { JobId = jobId }).GetAwaiter().GetResult();
            }, 2, 2000);
        }
        public string GetDisposalByClassCodeJobMessage(string jobId)
        {
            return RetryUtility.RetryAlways(() =>
            {
                return this.HybridClient.RecordsJobService.GetDisposalByClassCodeJobMessage(new JobInfo() { JobId = jobId }).GetAwaiter().GetResult();
            }, 2, 2000);
        }
        public bool LoadFSNodeEnableRecordManagement(Guid nodeId)
        {
            return RetryUtility.RetryAlways(() =>
            {
                return this.HybridClient.RecordsJobService.LoadFSNodeEnableRecordManagement(nodeId).GetAwaiter().GetResult();
            }, 2, 2000);
        }
        public List<Guid> ValidateEnableRecordManagementNodes(List<Guid> nodeIds)
        {
            return RetryUtility.RetryAlways(() =>
            {
                return this.HybridClient.RecordsJobService.ValidateEnableRecordManagementNodes(nodeIds).GetAwaiter().GetResult();
            }, 2, 2000);
        }
        public string GetFSArchiverRestoreJobMessage(string jobId)
        {
            return RetryUtility.RetryAlways(() =>
            {
                return this.HybridClient.RecordsJobService.GetFSRestoreJobMessage(new JobInfo() { JobId = jobId }).GetAwaiter().GetResult();
            }, 2, 2000);
        }
        public string GetFSRetainJobMessage(string jobId)
        {
            return RetryUtility.RetryAlways(() =>
            {
                return this.HybridClient.RecordsJobService.GetFSRetainJobMessage(new JobInfo() { JobId = jobId }).GetAwaiter().GetResult();
            }, 2, 2000);
        }
        
        public string GetFSDiscoveryJobMessage(string jobId)
        {
            return RetryUtility.RetryAlways(() =>
            {
                return this.HybridClient.RecordsJobService.GetFSDiscoveryJobMessage(new JobInfo() { JobId = jobId }).GetAwaiter().GetResult();
            }, 2, 2000);
        }
        public bool UpdateJobTime(RMFileSystemJobTimeReferenceDto dto)
        {
            return ApiRetryPolicy.ExecuteAction(() =>
            {
                return this.HybridClient.RecordsJobService.UpdateJobTime(dto).GetAwaiter().GetResult();
            });
        }


        public bool ResetApplyExistingOption(Guid scopeId)
        {
            return ApiRetryPolicy.ExecuteAction(() =>
            {
                return this.HybridClient.RecordsJobService.ResetApplyExistingOption(scopeId.ToString()).GetAwaiter().GetResult();
            });
        }


        public AgentSyncDataResultDto SyncData(List<FileSystemRecordDto> dtos)
        {
            return ApiRetryPolicy.ExecuteAction(() =>
            {
                return this.HybridClient.RecordsJobService.SyncData(dtos).GetAwaiter().GetResult();
            });
        }


        public FileSystemUniqueIdDto GetUniqueIdSetting()
        {
            return ApiRetryPolicy.ExecuteAction(() =>
            {
                return this.HybridClient.RecordsJobService.GetUniqueIdSetting().GetAwaiter().GetResult();
            });
        }        
        
        public List<long> GetUniqueIdList(int needCreateCount)
        {
            return ApiRetryPolicy.ExecuteAction(() =>
            {
                return this.HybridClient.RecordsJobService.GetUniqueIdList(needCreateCount).GetAwaiter().GetResult();
            });
        }

        public void DeleteMovedItem(FileSystemRecordDto record)
        {
            ApiRetryPolicy.ExecuteAction(() =>
            {
                this.HybridClient.RecordsJobService.DeleteMovedItem(record).GetAwaiter().GetResult();
            });
        }
        
        public void DeleteMovedItems(List<FsRecordProcessDto> records)
        {
            ApiRetryPolicy.ExecuteAction(() =>
            {
                this.HybridClient.RecordsJobService.DeleteMovedItems(records).GetAwaiter().GetResult();
            });
        }

        public AgentSyncDataResultDto SyncMovedData(List<FileSystemRecordDto> dtos)
        {
            return ApiRetryPolicy.ExecuteAction(() =>
            {
                return this.HybridClient.RecordsJobService.SyncMovedData(dtos).GetAwaiter().GetResult();
            });
        }


        public string GetRecords(List<Guid> ids)
        {
            return ApiRetryPolicy.ExecuteAction(() =>
            {
                return this.HybridClient.RecordsJobService.GetRecords(ids).GetAwaiter().GetResult();
            });
        }

        public List<Guid> AddScanData(List<FSAzureTableEntityDto> dtos)
        {
            return ApiRetryPolicy.ExecuteAction(() =>
            {
                return this.HybridClient.RecordsJobService.AddScanData(dtos).GetAwaiter().GetResult();
            });
        }
        public async Task<bool> UpdateFolderSizes(List<FolderSizeUpdateDto> dtos)
        {
            return await ApiRetryPolicy.ExecuteAction(async () =>
            {
                return await this.HybridClient.RecordsJobService.UpdateFolderSizes(dtos);
            });
        }
        public bool CheckIsHoldRecord(string id)
        {
            return ApiRetryPolicy.ExecuteAction(() =>
            {
                return this.HybridClient.RecordsJobService.CheckIsHoldRecord(id).GetAwaiter().GetResult();
            });
        }
        public List<Guid> AddScanDataToCosmos(FSAzureTableEntityDtoWithJobId dtos)
        {
            return ApiRetryPolicy.ExecuteAction(() =>
            {
                return this.HybridClient.RecordsJobService.AddScanDataToCosmos(dtos).GetAwaiter().GetResult();
            });
        }
        public List<Guid> RemoveManualData(List<FSAzureTableEntityDto> dtos)
        {
            return ApiRetryPolicy.ExecuteAction(() =>
            {
                return this.HybridClient.RecordsJobService.RemoveManualData(dtos).GetAwaiter().GetResult();
            });
        }

        public FSDueRecordsDto GetFSDueRecords(SearchFilterParam searchFilterParam)
        {
            return ApiRetryPolicy.ExecuteAction(() =>
            {
                return this.HybridClient.RecordsJobService.GetFSDueRecords(searchFilterParam).GetAwaiter().GetResult();
            });
        }

        public List<Guid> AddRejectScanData(List<FSAzureTableEntityDto> entities)
        {
            return ApiRetryPolicy.ExecuteAction(() =>
            {
                return this.HybridClient.RecordsJobService.AddRejectScanData(entities).GetAwaiter().GetResult();
            });
        }

        public List<FSAzureTableEntityDto> GetScanDataByPage(string jobId, int pageIndex, int pageSize)
        {
            return ApiRetryPolicy.ExecuteAction(() =>
            {
                return this.HybridClient.RecordsJobService.GetScanDataByPage(jobId, pageIndex, pageSize).GetAwaiter().GetResult();
            });
        }

        public List<Guid> GetRuleIdsByJob(string jobId)
        {
            return ApiRetryPolicy.ExecuteAction(() =>
            {
                return this.HybridClient.RecordsJobService.GetRuleIdsByJob(jobId).GetAwaiter().GetResult();
            });
        }

        public List<FSFolderCacheDto> GetExplorerDataByFolder(string folderId, string scopeId, long sortTicks, int pageSize)
        {
            return ApiRetryPolicy.ExecuteAction(() =>
            {
                return this.HybridClient.RecordsJobService.GetExplorerDataByFolder(folderId, scopeId, sortTicks, pageSize).GetAwaiter().GetResult();
            });
        }

        public List<FileSystemRecordDto> GetDBRecordsByFolder(string folderId, string scopeId, long sortTicks, int pageSize)
        {
            return ApiRetryPolicy.ExecuteAction(() =>
            {
                return this.HybridClient.RecordsJobService.GetDBRecordsByFolder(folderId, scopeId, sortTicks, pageSize).GetAwaiter().GetResult();
            });
        }

        public List<FileSystemRecordDto> GetDBRecordsByFolderAndFilterByEndTime(string folderId, string scopeId, long sortTicks, int pageSize)
        {
            return ApiRetryPolicy.ExecuteAction(() =>
            {
                return this.HybridClient.RecordsJobService.GetDBRecordsByFolderAndFilterByEndTime(folderId, scopeId, sortTicks, pageSize).GetAwaiter().GetResult();
            });
        }

        public List<FileSystemRecordDto> GetDBRecordsByNodeIds(List<Guid> nodeIds, string scopeId, long sortTicks)
        {
            return ApiRetryPolicy.ExecuteAction(() =>
            {
                var param = new RMAzureRecordParamsDto()
                {
                    NodeIds = nodeIds,
                    ScopeId = scopeId,
                    SortTicks = sortTicks
                };
                return this.HybridClient.RecordsJobService.GetDBRecordsByNodeIdsAndFilterByEndTime(param).GetAwaiter().GetResult();
            });
        }

        public async Task<List<FileSystemRecordDto>> GetDBRecordsByClassCodeAndFilterByEndTimeAsync(List<Guid> nodeIds, List<Guid> classCodeIds, string scopeId, long sortTicks)
        {
            return await ApiRetryPolicy.ExecuteAction(async () =>
            {
                var param = new RMAzureRecordByClassCodeParamsDto()
                {
                    NodeIds = nodeIds,
                    ClassCodeIds = classCodeIds,
                    ScopeId = scopeId,
                    SortTicks = sortTicks
                };
                return await this.HybridClient.RecordsJobService.GetDBRecordsByClassCodeAndFilterByEndTime(param);
            });
        }

        public List<RMAgentSyncFailureItem> FindSyncFailedItems(int dataSource, string scopeId, long sortTicks, int pageSize)
        {
            return ApiRetryPolicy.ExecuteAction(() =>
            {
                return this.HybridClient.RecordsJobService.FindSyncFailedItems(new RMSyncFailedScopeDto() { DataSource = dataSource, SiteId = scopeId, QueryTicks = sortTicks, PageSize = pageSize }).GetAwaiter().GetResult();
            });
        }

        public bool AddSyncFailedItems(List<RMAgentSyncFailureItem> failedItems)
        {
            return ApiRetryPolicy.ExecuteAction(() =>
            {
                return this.HybridClient.RecordsJobService.AddSyncFailedItems(failedItems).GetAwaiter().GetResult();
            });
        }

        public bool RemoveSuccessItemsInAzure(List<RMAgentSyncFailureItem> successItems)
        {
            return ApiRetryPolicy.ExecuteAction(() =>
            {
                return this.HybridClient.RecordsJobService.RemoveSuccessItemsInAzure(successItems).GetAwaiter().GetResult();
            });
        }

        public List<FileSystemRecordDto> GetFSDBRecords(List<Guid> ids)
        {
            return ApiRetryPolicy.ExecuteAction(() =>
            {
                return this.HybridClient.RecordsJobService.GetFSDBRecords(ids).GetAwaiter().GetResult();
            });
        }

        public List<FileSystemRecordDto> QueryFileSystemRecords(string connectionId, List<Guid> ids)
        {
            return ApiRetryPolicy.ExecuteAction(() =>
            {
                var queryParam = new FSQueryRecordRequestDto()
                {
                    ConnectionId = connectionId,
                    RecordIds = ids.Select(id => id.ToString()).ToList()
                };
                return this.HybridClient.RecordsJobService.QueryFileSystemRecords(queryParam).GetAwaiter().GetResult();
            });
        }

        public List<FsRecordProcessDto> QueryFileSystemRecordsByRecordsId(string connectionId, List<string> ids)
        {
            return ApiRetryPolicy.ExecuteAction(() =>
            {
                var queryParam = new FSQueryRecordRequestDto()
                {
                    ConnectionId = connectionId,
                    RecordIds = ids
                };
                return this.HybridClient.RecordsJobService.QueryFileSystemRecordsByRecordsId(queryParam).GetAwaiter().GetResult();
            });
        }

        public List<FileSystemRecordDto> GetFSDBRecordsByRecordsId(List<string> ids)
        {
            return ApiRetryPolicy.ExecuteAction(() =>
            {
                return this.HybridClient.RecordsJobService.GetFSDBRecordsByRecordsId(ids).GetAwaiter().GetResult();
            });
        }

        public List<FileSystemRecordDto> GetFSManualRecords(List<Guid> ids)
        {
            return ApiRetryPolicy.ExecuteAction(() =>
            {
                return this.HybridClient.RecordsJobService.GetFSManualRecords(ids).GetAwaiter().GetResult();
            });
        }

        
        public List<FSFolderCacheDto> GetFSFolderWithoutInheritTerm(string folderId, string termId)
        {
            return ApiRetryPolicy.ExecuteAction(() =>
            {
                return this.HybridClient.RecordsJobService.GetFSFolderWithoutInheritTerm(folderId, termId).GetAwaiter().GetResult();
            });
        }

        public List<FSFolderCacheDto> GetCurrentConnectionAllSettings(string connectionPath)
        {
            return ApiRetryPolicy.ExecuteAction(() =>
            {
                return this.HybridClient.RecordsJobService.GetCurrentConnectionAllSettings(connectionPath).GetAwaiter().GetResult();
            });
        }

        public List<FSFolderCacheDto> GetAzureDataByFolder(string folderId, string scopeId, long sortTicks, int pageSize)
        {
            return ApiRetryPolicy.ExecuteAction(() =>
            {
                return this.HybridClient.RecordsJobService.GetAzureDataByFolder(folderId, scopeId, sortTicks, pageSize).GetAwaiter().GetResult();
            });
        }

        public List<Guid> DeleteRecordsInExplorer(List<FSExplorerDeleteDto> dtos)
        {
            return ApiRetryPolicy.ExecuteAction(() =>
            {
                return this.HybridClient.RecordsJobService.DeleteRecordsInExplorer(dtos).GetAwaiter().GetResult();
            });
        }
        public void RunSendEmailJob(string jobId)
        {
            ApiRetryPolicy.ExecuteAction(() =>
            {
                this.HybridClient.RecordsJobService.RunSendEmailJob(jobId).GetAwaiter().GetResult();
            });
        }
        public bool DeleteAndMoveItemsInScope(string connectionPath, string scopeId)
        {
            return ApiRetryPolicy.ExecuteAction(() =>
            {
                return this.HybridClient.RecordsJobService.DeleteAndMoveItemsInScope(new FSAzureTableRequestInfo() { ConnectionPath = connectionPath, ScopeId = scopeId }).GetAwaiter().GetResult();
            });
        }

        #region apply setting
        public List<RMSPTreeNode> BrowseSPTreeNode(RMSPTreeNode node)
        {
            return ApiRetryPolicy.ExecuteAction(() =>
            {
                return this.HybridClient.SharePointJobService.BrowseSPTreeNode(node).GetAwaiter().GetResult();
            });
        }

        public bool SetSettingJobTime(Guid scopeId, Guid siteId, bool isFailedColumn, bool isFailedProperty)
        {
            return ApiRetryPolicy.ExecuteAction(() =>
            {
                return this.HybridClient.SharePointJobService.SetSettingJobTime(new SPSettingJobInfo() { ScopeId = scopeId, SiteId = siteId, IsFailedColumn = isFailedColumn, IsFailedProperty = isFailedProperty }).GetAwaiter().GetResult();
            });
        }

        public string GetSPJobMessage(string jobId, JobType jobType)
        {
            return RetryUtility.RetryAlways(() =>
            {
                return this.HybridClient.SharePointJobService.GetSPJobMessage(new JobInfo() { JobId = jobId, JobType = jobType }).GetAwaiter().GetResult();
            }, 2, 3000);
        }

        public long GetAutoJobCollectionTime(int type, Guid folderId, Guid listId, Guid nodeId, Guid groupId)
        {
            return ApiRetryPolicy.ExecuteAction(() =>
            {
                return this.HybridClient.SharePointJobService.GetAutoJobCollectionTime(type, folderId, listId, nodeId, groupId).GetAwaiter().GetResult();
            });
        }

        public bool UpdateAutoJobCollectionTime(List<NodeFlag> nodeFlags)
        {
            return ApiRetryPolicy.ExecuteAction(() =>
            {
                return this.HybridClient.SharePointJobService.UpdateAutoJobCollectionTime(nodeFlags).GetAwaiter().GetResult();
            });
        }

        #endregion

        #region OnPremiseSP Disposal
        public List<OnPremiseSPListCacheDto> GetOnPremiseSPAzureDataByListId(string listId, string scopeId, long sortTicks, int pageSize)
        {
            return ApiRetryPolicy.ExecuteAction(() =>
            {
                return this.HybridClient.RecordsJobService.GetOnPremiseSPAzureDataByListId(listId, scopeId, sortTicks, pageSize).GetAwaiter().GetResult();
            });
        }

        public List<Guid> AddOnpremiseSPManualDataToAzureTable(List<OnPremiseSPAzureTableEntityDto> dtos)
        {
            return ApiRetryPolicy.ExecuteAction(() =>
            {
                return this.HybridClient.RecordsJobService.AddOnpremiseSPManualDataToAzureTable(dtos).GetAwaiter().GetResult();
            });
        }
        public int AddRejectItemsToStaticTableForOnPremiseSP(List<OnPremiseSPAzureTableEntityDto> dtos)
        {
            return ApiRetryPolicy.ExecuteAction(() =>
            {
                return this.HybridClient.RecordsJobService.AddRejectItemsToStaticTableForOnPremiseSP(dtos).GetAwaiter().GetResult();
            });
        }
        public List<Guid> UpdateAzureTableOnpremiseSPManualItem(List<OnPremiseSPAzureTableEntityDto> dtos)
        {
            return ApiRetryPolicy.ExecuteAction(() =>
            {
                return this.HybridClient.RecordsJobService.UpdateAzureTableOnpremiseSPManualItem(dtos).GetAwaiter().GetResult();
            });
        }
        public List<Guid> OnPremiseSPUpdateRecordsInExplorer(List<OnPremiseSPAzureTableEntityDto> dtos)
        {
            return ApiRetryPolicy.ExecuteAction(() =>
            {
                return this.HybridClient.RecordsJobService.OnPremiseSPUpdateRecordsInExplorer(dtos).GetAwaiter().GetResult();
            });
        }
        public List<OnPremiseSPListCacheDto> GetOnPremiseSPExplorerDataByListId(string listId, string scopeId, long sortTicks, int pageSize)
        {
            return ApiRetryPolicy.ExecuteAction(() =>
            {
                return this.HybridClient.RecordsJobService.GetOnPremiseSPExplorerDataByListId(listId, scopeId, sortTicks, pageSize).GetAwaiter().GetResult();
            });
        }
        public List<OnPremRelatedResult> DeleteRelatedPhysicalRecord(OnPremRelatedDto dto)
        {
            return ApiRetryPolicy.ExecuteAction(() =>
            {
                return this.HybridClient.RecordsJobService.DeleteRelatedPhysicalRecord(dto).GetAwaiter().GetResult();
            });
        }
        #endregion



        #region data sync
        [Obsolete("Not in use, assemble record owner in api web")]
        public Dictionary<Guid, string> GetItemOwnerMapping(Guid siteId, List<Guid> nodeId)
        {
            return ApiRetryPolicy.ExecuteAction(() =>
            {
                return this.HybridClient.SharePointJobService.GetItemOwnerMapping(new ItemOwnerMappingDto() { ScopeId = siteId, NodeIds = nodeId }).GetAwaiter().GetResult();
            });
        }
        [Obsolete("Not in use, assemble record owner in api web")]
        public Dictionary<Guid, string> GetIncrementalItemOwnerMapping(Guid siteId, Guid listId, List<int> itemIds)
        {
            return ApiRetryPolicy.ExecuteAction(() =>
            {
                return this.HybridClient.SharePointJobService.GetIncrementalItemOwnerMapping(new IncrementalItemOwnerMappingDto() { ScopeId = siteId, ListId = listId, ItemId = itemIds }).GetAwaiter().GetResult();
            });
        }

        public Dictionary<Guid, List<string>> GetManualNodeAndApproverMapping(Guid siteId, List<Guid> nodeId)
        {
            return ApiRetryPolicy.ExecuteAction(() =>
            {
                return this.HybridClient.SharePointJobService.GetManualNodeAndApproverMapping(siteId, nodeId).GetAwaiter().GetResult();
            });
        }

        public List<RMAccount> GetUserByUserIds(List<string> userIds)
        {
            return ApiRetryPolicy.ExecuteAction(() =>
            {
                return this.HybridClient.SharePointJobService.GetUserByUserIds(userIds).GetAwaiter().GetResult();
            });
        }

        public AgentSyncDataResultDto AddSPDataToExplorer(List<RecordDto> records)
        {
            return ApiRetryPolicy.ExecuteAction(() =>
            {
                return this.HybridClient.SharePointJobService.AddSPDataToExplorer(records).GetAwaiter().GetResult();
            });
        }     

        public List<RecordDto> GetRecordsByTerms(Guid scopeId, List<Guid> termIds, long ticks, long sortTicks, int pageSize)
        {
            return ApiRetryPolicy.ExecuteAction(() =>
            {
                return this.HybridClient.SharePointJobService.GetRecordsByTerms(new QueryChangedTermItemsDto()
                {
                    ScopeId = scopeId,
                    TermIds = termIds,
                    Ticks = ticks,
                    SortTicks = sortTicks,
                    PageSize = pageSize
                }).GetAwaiter().GetResult();
            });
        }

        public bool RemoveSPObjInExplorer(Guid siteId, Guid objectId,int itemRowId)
        {
            return ApiRetryPolicy.ExecuteAction(() =>
            {
                return this.HybridClient.SharePointJobService.RemoveSPObjInExplorer(new RemoveSPObjDto() { SiteId = siteId, ObjectId = objectId, ItemRowId= itemRowId }).GetAwaiter().GetResult();
            });
        }

        public bool AddSiteFlagInfos(List<NodeFlag> nodeFlags)
        {
            return ApiRetryPolicy.ExecuteAction(() =>
            {
                return this.HybridClient.SharePointJobService.AddSiteFlagInfos(nodeFlags).GetAwaiter().GetResult();
            });
        }

        public bool AddSiteScope(RMScope site)
        {
            return ApiRetryPolicy.ExecuteAction(() =>
            {
                return this.HybridClient.SharePointJobService.AddSiteScope(site).GetAwaiter().GetResult();
            });
        }

        public List<Guid> UpdateRecordsInExplorer(List<RecordDto> records)
        {
            return ApiRetryPolicy.ExecuteAction(() =>
            {
                return this.HybridClient.SharePointJobService.UpdateRecordsInExplorer(records).GetAwaiter().GetResult();
            });
        }

        public bool UpdateDeletedItemsInExplorer(List<DeleteItemDto> dtos)
        {
            return ApiRetryPolicy.ExecuteAction(() =>
            {
                return this.HybridClient.SharePointJobService.UpdateDeletedItemsInExplorer(dtos).GetAwaiter().GetResult();
            });
        }

        public int MoveOnPremiseSPItemsToStatic(string scopeId)
        {
            return ApiRetryPolicy.ExecuteAction(() =>
            {
                return this.HybridClient.RecordsJobService.MoveOnPremiseSPItemsToStatic(scopeId).GetAwaiter().GetResult();
            });
        }
        #endregion

        #region realtime action

        public Dictionary<string, SiteInfo> GetOnPremiseSiteInfos(List<string> siteIds)
        {
            return ApiRetryPolicy.ExecuteAction(() =>
            {
                return this.HybridClient.SharePointJobService.GetOnPremiseSiteInfos(siteIds).GetAwaiter().GetResult();
            });
        }


        public bool AddRecordHistory(AvePoint.RA.Contract.Global.Explorer.RecordHistoryDto recordHistoryDto)
        {
            return ApiRetryPolicy.ExecuteAction(() =>
            {
                return this.HybridClient.SharePointJobService.AddRecordHistory(recordHistoryDto).GetAwaiter().GetResult();
            });
        }


        public bool UpdateTermChangeItems(AvePoint.RA.Contract.Global.Explorer.TermChangeItemDto termChangeItemDto)
        {
            return ApiRetryPolicy.ExecuteAction(() =>
            {
                return this.HybridClient.SharePointJobService.UpdateTermChangeItems(termChangeItemDto).GetAwaiter().GetResult();
            });
        }


        public bool UpdateDeclaredItems(AvePoint.RA.Contract.Global.Explorer.DeclareItemDto declareItemDto)
        {
            return ApiRetryPolicy.ExecuteAction(() =>
            {
                return this.HybridClient.SharePointJobService.UpdateDeclaredItems(declareItemDto).GetAwaiter().GetResult();
            });
        }


        public bool AddClassificationHistory(List<AvePoint.RA.Contract.Global.Object.RMClassificationHistory> classificationHistories)
        {
            return ApiRetryPolicy.ExecuteAction(() =>
            {
                return this.HybridClient.SharePointJobService.AddClassificationHistory(classificationHistories).GetAwaiter().GetResult();
            });
        }


        public List<RecordDto> GetRecordsByIds(List<Guid> ids)
        {
            return ApiRetryPolicy.ExecuteAction(() =>
            {
                return this.HybridClient.SharePointJobService.GetRecordsByIds(ids).GetAwaiter().GetResult();
            });
        }


        public bool UpdateRealtimeJobState(RealtimeJobState realtimeJobState)
        {
            return ApiRetryPolicy.ExecuteAction(() =>
            {
                return this.HybridClient.SharePointJobService.UpdateRealtimeJobState(realtimeJobState).GetAwaiter().GetResult();
            });
        }

        public AvePoint.RA.Contract.Global.Explorer.GlobalSearchQueryResult QueryDataForGlobalSearch(AvePoint.RA.Contract.Global.Explorer.GlobalSearchQueryDto dto)
        {
            return ApiRetryPolicy.ExecuteAction(() =>
            {
                return this.HybridClient.SharePointJobService.QueryDataForGlobalSearch(dto).GetAwaiter().GetResult();
            });
        }
        #endregion

        #region SharePoint On-Premise Scan Local Node

        public List<OnPremiseSPLocalNode> GetRecordsLocalNodes(int pageIndex, int total, string parentId)
        {
            return ApiRetryPolicy.ExecuteAction(() =>
            {
                return this.HybridClient.SharePointOnPremLocalNodeService.GetRecordsLocalNodes(pageIndex, total, parentId).GetAwaiter().GetResult();
            });
        }

        public OnPremSPScanNodeResult BatchAddRecordsLocalNodes(List<OnPremiseSPLocalNode> localNodes)
        {
            return ApiRetryPolicy.ExecuteAction(() =>
            {
                return this.HybridClient.SharePointOnPremLocalNodeService.BatchAddRecordsLocalNodes(localNodes).GetAwaiter().GetResult();
            });
        }

        public OnPremSPScanNodeResult BatchUpdateRecordsLocalNodes(List<OnPremiseSPLocalNode> localNodes)
        {
            return ApiRetryPolicy.ExecuteAction(() =>
            {
                return this.HybridClient.SharePointOnPremLocalNodeService.BatchUpdateRecordsLocalNodes(localNodes).GetAwaiter().GetResult();
            });
        }

        public OnPremSPScanNodeResult BatchDeleteRecordsLocalNodes(List<OnPremiseSPLocalNode> localNodes)
        {
            return ApiRetryPolicy.ExecuteAction(() =>
            {
                return this.HybridClient.SharePointOnPremLocalNodeService.BatchDeleteRecordsLocalNodes(localNodes).GetAwaiter().GetResult();
            });
        }

        #endregion

        #region StorageDevice
        public StorageDeviceDto GetIndexDevice()
        {
            return ApiRetryPolicy.ExecuteAction(() =>
            {
                string json = this.HybridClient.StorageDeviceService.GetIndexDevice().GetAwaiter().GetResult();
                return SerializerHelper.DeserializeByJsonConvert<StorageDeviceDto>(json);
            });
        }
        public StorageDeviceDto GetStorageDeviceById(string storageId)
        {
            return ApiRetryPolicy.ExecuteAction(() =>
            {
                string json = this.HybridClient.StorageDeviceService.GetStorageDeviceById(storageId).GetAwaiter().GetResult();
                return SerializerHelper.DeserializeByJsonConvert<StorageDeviceDto>(json);
            });
        }
        public bool UpdateLastArchivedTime(string id)
        {
            return ApiRetryPolicy.ExecuteAction(() =>
            {
                return this.HybridClient.StorageDeviceService.UpdateLastArchivedTime(id).GetAwaiter().GetResult();
            });
        }
        #endregion

        #region MediaDatas
        public bool UpdateOrInsertMediaData(KeyValuePair<string,string> keyValue)
        {
            return ApiRetryPolicy.ExecuteAction(() =>
            {
                return this.HybridClient.MediaDatasService.UpdateOrInsertMediaData(keyValue).GetAwaiter().GetResult();
            });
        }

        public string GetMediaDatas(string key)
        {
            return ApiRetryPolicy.ExecuteAction(() =>
            {
                return this.HybridClient.MediaDatasService.GetMediaDatas(key).GetAwaiter().GetResult();
            });
        }
        #endregion

        #region FSMasterIndex
        public string InsertIntoFSMasterIndex(FSMasterIndexContract indexDto)
        {
            return ApiRetryPolicy.ExecuteAction(() =>
            {
                string json = SerializerHelper.SerializeByJsonConvert(indexDto);
                return this.HybridClient.FSMasterIndexService.InsertIntoFSMasterIndex(json).GetAwaiter().GetResult();
            });
        }
        public List<FSMasterIndexContract> GetConnectionMasterWithSubInfosList(string connectionId)
        {
            return ApiRetryPolicy.ExecuteAction(() =>
            {
                return SerializerHelper.DeserializeByJsonSerializer<List<FSMasterIndexContract>>(this.HybridClient.FSMasterIndexService.GetConnectionMasterWithSubInfosList(connectionId).GetAwaiter().GetResult());
            });
        }
        public string GetConnectionNameById(string connectionId)
        {
            return ApiRetryPolicy.ExecuteAction(() =>
            {
                return this.HybridClient.FSMasterIndexService.GetConnectionNameById(connectionId).GetAwaiter().GetResult();
            });
        }
        public FSMasterIndexContract GetMasterIndexBySubjobId(string subJobid)
        {
            return ApiRetryPolicy.ExecuteAction(() =>
            {
                return SerializerHelper.DeserializeByJsonSerializer<FSMasterIndexContract>(this.HybridClient.FSMasterIndexService.GetMasterIndexBySubjobId(subJobid).GetAwaiter().GetResult());
            });
        }
        public void DeleteFSMasterIndex(FSMasterIndexContract masterIndexInfo)
        {
            ApiRetryPolicy.ExecuteAction(() =>
            {
                this.HybridClient.FSMasterIndexService.DeleteFSMasterIndex(SerializerHelper.SerializeByJsonSerializer(masterIndexInfo)).GetAwaiter().GetResult();
            });
        }
        #endregion
        #region FSIndexSubInfo
        public void UpdateFSIndexSubInfo(ArchiverIndexSubInfoContract fsIndexSubInfo)
        {
            ApiRetryPolicy.ExecuteAction(() =>
            {
                this.HybridClient.FSIndexSubInfoService.UpdateFSIndexSubInfo(SerializerHelper.SerializeByJsonSerializer(fsIndexSubInfo)).GetAwaiter().GetResult();
            });
        }
        public ArchiverIndexSubInfoContract GetFSIndexSubinfoBySubsubJobId(string subsubJobId)
        {
            return ApiRetryPolicy.ExecuteAction(() =>
            {
                string res = this.HybridClient.FSIndexSubInfoService.GetFSIndexSubinfoBySubsubJobId(subsubJobId).GetAwaiter().GetResult();
                if (string.IsNullOrWhiteSpace(res) || res == "null")
                {
                    return null;
                }
                else
                {
                    return SerializerHelper.DeserializeByJsonSerializer<ArchiverIndexSubInfoContract>(res);
                }  
            });
        }
        public bool ExistFSIndexSubInfoBySubJobId(string subJobId)
        {
            return ApiRetryPolicy.ExecuteAction(() =>
            {
                return this.HybridClient.FSIndexSubInfoService.ExistFSIndexSubInfoBySubJobId(subJobId).GetAwaiter().GetResult();
            });
        }
        public void DeleteFSIndexSubInfo(ArchiverIndexSubInfoContract fsIndexSubInfo)
        {
            ApiRetryPolicy.ExecuteAction(() =>
            {
                this.HybridClient.FSIndexSubInfoService.DeleteFSIndexSubInfo(SerializerHelper.SerializeByJsonSerializer(fsIndexSubInfo)).GetAwaiter().GetResult();
            });
        }
        public void UpdateRetainedSizeInfo(RetainedInfo retainedInfo)
        {
            ApiRetryPolicy.ExecuteAction(() =>
            {
                this.HybridClient.FSIndexSubInfoService.UpdateRetainedSizeInfo(SerializerHelper.SerializeByJsonSerializer(retainedInfo)).GetAwaiter().GetResult();
            });
        }
        #endregion
        #region FSArchiverManagement
        public bool UpdateSiteMasterMediaDataSize(string subjobId, long mediaDataSize)
        {
            JobIdStateInfo info = new JobIdStateInfo() { JobId = subjobId, MediaDataSize = mediaDataSize };
            //StorageDeviceDto info = new StorageDeviceDto();
            return ApiRetryPolicy.ExecuteAction(() =>
            {
                return this.HybridClient.FSArchiverManagementService.UpdateSiteMasterMediaDataSize(SerializerHelper.SerializeByJsonSerializer(info)).GetAwaiter().GetResult();
            });
        }
        public bool CheckCurrentJobHasMerged(string jobId)
        {
            return ApiRetryPolicy.ExecuteAction(() =>
            {
                return this.HybridClient.FSArchiverManagementService.CheckCurrentJobHasMerged(jobId).GetAwaiter().GetResult();
            });
        }
        public void UpdateMergeIndexStateAsync(string jobId, int mergeIndexState)
        {
            JobIdStateInfo info = new JobIdStateInfo() { JobId = jobId, MergeIndexState = mergeIndexState };
            //StorageDeviceDto info = new StorageDeviceDto();
            ApiRetryPolicy.ExecuteAction(() =>
            {
                this.HybridClient.FSArchiverManagementService.UpdateMergeIndexState(SerializerHelper.SerializeByJsonSerializer(info)).GetAwaiter().GetResult();
            });
        }
        #endregion
        #region FSDiscovery
        public void UploadAnalyzedFileToStorage(DiscoveryAnalyzedDataInfo data)
        {
            ApiRetryPolicy.ExecuteAction(() =>
            {
                this.HybridClient.FSDiscoveryService.UploadAnalyzedFileToStorage(data).GetAwaiter().GetResult();
            });
        }

        public string GetDiscoveryFSTagRuleInfos()
        {
            return ApiRetryPolicy.ExecuteAction(() =>
            {
                return this.HybridClient.FSDiscoveryService.GetDiscoveryFSTagRuleInfos().GetAwaiter().GetResult();
            });
        }
        #endregion

        #region SettingProfile
        public string GetDBSEEMasterKey()
        {
            return ApiRetryPolicy.ExecuteAction(() =>
            {
                return this.HybridClient.SettingProfileService.GetDBSEEMasterKey().GetAwaiter().GetResult();
            });
        }
        #endregion

        #region telemetry
        public void AddTelemetryForRetentionJob(RARetentionJobTelemetry retentionRecord)
        {
            ApiRetryPolicy.ExecuteAction(() =>
            {
                this.HybridClient.TelemetryService.AddTelemetryForRetentionJob(retentionRecord).GetAwaiter().GetResult();
            });
        }
        #endregion

        public AgentLogSaSResponse GetAgentLogUploadSas(AgentLogSaSRequest request)
        {
            return ApiRetryPolicy.ExecuteAction(() =>
            {
                return this.HybridClient.AgentLogCollectorService
                    .GetAgentLogUploadSas(request)
                    .GetAwaiter()
                    .GetResult();
            });
        }

        public AgentInformation GetAgentInformation(AgentInfo agentInfo)
        {
            return ApiRetryPolicy.ExecuteAction(() =>
            {
                var response = this.HybridClient.AgentMgmtService.GetAgentInfor(agentInfo).GetAwaiter().GetResult();
                return response;
            });
        }

        #region batch data upload
        public bool StartQueueListenerAsync(string jobId, JobType jobType)
        {
            return ApiRetryPolicy.ExecuteAction(() =>
            {
                return this.HybridClient.RecordsJobService.StartQueueListener(new JobInfo() { JobId = jobId, JobType = jobType }).GetAwaiter().GetResult();
            });
        }

        public string GetBlobSasUriAsync(string jobId, string blobName)
        {
            return ApiRetryPolicy.ExecuteAction(() =>
            {
                return this.HybridClient.RecordsJobService.GetBlobSasUri(jobId, blobName).GetAwaiter().GetResult();
            });
        }

        public string NotifyUploadCompleteAsync(FSBatchUploadNotification request)
        {
            return ApiRetryPolicy.ExecuteAction(() =>
            {
                return this.HybridClient.RecordsJobService.NotifyUploadComplete(request).GetAwaiter().GetResult();
            });
        }

        public FSBatchReportTableEntityDto GetBatchReportResponseAsync(string jobId, string messageId)
        {
            return ApiRetryPolicy.ExecuteAction(() =>
            {
                return this.HybridClient.RecordsJobService.GetBatchReportResponse(jobId, messageId).GetAwaiter().GetResult();
            });
        }

        public bool DisposeQueueListener(string jobId)
        {
            return ApiRetryPolicy.ExecuteAction(() =>
            {
                return this.HybridClient.RecordsJobService.DisposeQueueListener(jobId).GetAwaiter().GetResult();
            });
        }
        #endregion

        #region Agent upgrader
        public void UpdateAgentStatus(AgentInfo agentInfo)
        {
            ApiRetryPolicy.ExecuteAction(() =>
            {
                this.HybridClient.AgentMgmtService.UpdateAgentStatus(agentInfo).GetAwaiter().GetResult();
            });
        }
        #endregion

        #region Data Ingestion
        public RMDataIngestionMessageSendReceipt DataIngestionSendMessage(RMDataIngestionMessageDto message)
        {
            return ApiRetryPolicy.ExecuteAction(() =>
            {
                return this.HybridClient.DataIngestionService.SendMessage(message).GetAwaiter().GetResult();
            });
        }

        public RMDataIngestionBlobReference DataIngestionGenerateBlobReference(RMDataIngestionBlobNamingContext blobNamingContext)
        {
            return ApiRetryPolicy.ExecuteAction(() =>
            {
                return this.HybridClient.DataIngestionService.GenerateBlobReference(blobNamingContext).GetAwaiter().GetResult();
            });
        }

        public string DataIngestionGenerateBlobSasUri(RMDataIngestionType ingestionType, string blobName)
        {
            return ApiRetryPolicy.ExecuteAction(() =>
            {
                return this.HybridClient.DataIngestionService.GenerateBlobSasUri(ingestionType, blobName).GetAwaiter().GetResult();
            });
        }

        public RMDataIngestionExecutionResult DataIngestionGetExecutionResult(string jobId, string messageId)
        {
            return ApiRetryPolicy.ExecuteAction(() =>
            {
                return this.HybridClient.DataIngestionService.GetIngestionExecutionResult(jobId, messageId).GetAwaiter().GetResult();
            });
        }

        public bool DeleteBlobByName(RMDataIngestionBlobDto blobDto)
        {
            return ApiRetryPolicy.ExecuteAction(() =>
            {
                return this.HybridClient.DataIngestionService.DeleteBlobByName(blobDto).GetAwaiter().GetResult();
            });
        }

        public bool CheckConnectionStatus(string connId)
        {
            return ApiRetryPolicy.ExecuteAction(() =>
            {
                return this.HybridClient.RMFSConnManagementService.CheckConnectionStatus(connId).GetAwaiter().GetResult();
            });
        }


        #endregion
    }
}
