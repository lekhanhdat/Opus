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
using AvePoint.GCommon.Contract.Server.ControlPanel.Office365;
using AvePoint.GCommon.Contract.Tree.Object;
using AvePoint.GCommon.Utility;
using AvePoint.RA.Common;
using AvePoint.RA.Common.Report;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.BoxBrowser;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.Contract.JobMonitor;
using AvePoint.RA.Contract.Object.RealTime;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RMWeb.Explorer;
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using AvePoint.RA.Contract.RMWeb.Tree.Base;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.DB.Core;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Explorer.Dao;
using AvePoint.RA.DB.Explorer.Dao.CosmosImp;
using AvePoint.RA.DB.Model;
using AvePoint.RA.RACommonUtility.Browser;
using AvePoint.RA.RADataBroker;
using AvePoint.Wrapper.Common;
using RAGlobalSearch.Common;
using RAGlobalSearch.Discover;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using AvePoint.RA.DB.Dao.Extension;

namespace RAGlobalSearch
{
    public class GlobalSearch
    {
        protected AvePoint.RA.Contract.Services.IRALogger logger = RALogger.GetInstance(typeof(GlobalSearch));
        #region interface
        private IRMSubJobDao mSubJobDao;
        public IRMSubJobDao SubJobDao
        {
            get
            {
                if (mSubJobDao == null)
                {
                    mSubJobDao = (IRMSubJobDao)PlatformWindsorManager.GetService(typeof(IRMSubJobDao));
                }
                return mSubJobDao;
            }
        }

        private IExplorerQueryService mExplorerQueryService;
        public IExplorerQueryService ExplorerQueryService
        {
            get
            {
                if (mExplorerQueryService == null)
                {
                    mExplorerQueryService = (IExplorerQueryService)PlatformWindsorManager.GetService(typeof(IExplorerQueryService));
                }
                return mExplorerQueryService;
            }
        }

        protected IRMReportManager mReportManager;
        protected IRMReportManager ReportManager
        {
            get
            {
                if (mReportManager == null)
                {
                    mReportManager = ReportMangerFactory.Instance.ReportManager;
                }
                return mReportManager;
            }
        }
        private IExplorerDao _explorerDao;
        public IExplorerDao ExplorerDao
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
        private IJobInfoUpdater _jobInfoUpdater;
        protected IJobInfoUpdater JobInfoUpdater
        {
            get
            {
                if (_jobInfoUpdater == null)
                {
                    _jobInfoUpdater = (IJobInfoUpdater)PlatformWindsorManager.GetService(typeof(IJobInfoUpdater));
                }
                return _jobInfoUpdater;
            }
        }
        private static IRMKeyValueDao s_RMKeyValueDao => PlatformWindsorManager.GetService<IRMKeyValueDao>();
        #endregion
        private string mJobId;
        private bool mHasError = false;
        private Dictionary<string, RemoteSiteCollection> sitesDic = new Dictionary<string, RemoteSiteCollection>();
        public GlobalSearch(string jobId)
        {
            mJobId = jobId;
            ReportMangerFactory.Instance.Init(mJobId, AvePoint.RA.Contract.JobMonitor.JobType.GlobalSearchAction, true);
            JobInfoUpdater.UpdateJobState(mJobId, (int)JobStatus.InProgress);
            ReportManager.StartUpdateJobProgress();
            WrapperConfiguration.IsEnableTeams = s_RMKeyValueDao.HasUpgradeTeams();
        }

        public async System.Threading.Tasks.Task RunAsync()
        {
            IGlobalSearchAction action = null;
            try
            {
                logger.Info("Start to run global search action job.");
                //WrapperConfiguration.EnableDownloadLATData = false;
                RMSubJob subJobWithContext = SubJobDao.GetSubJob(mJobId, true);
                var jobContext = SerializerHelper.DeserializeByDataContractSerializer<GlobalSearchActionDto>(subJobWithContext.JobContext.Content);
                action = GlobalSearchActionFactory.GetGlobalSearchAction(jobContext.Action);
                //logger.Info($"Job context:{subJobWithContext.JobContext.Content}");
                if (jobContext.ForceDiscoverAll && !((SourceFlag)jobContext.SourceFlag == SourceFlag.Physical && jobContext.Action == GlobalSearchAction.AccessControl))
                {
                    logger.Info("Run with discover.");
                    if (jobContext.FilterInfo.PagingInfo == null)
                    {
                        jobContext.FilterInfo.PagingInfo = new ExplorerPagingInfo()
                        {
                            PageIndex = "",
                            PageSize = 500
                        };
                    }
                    GlobalSearchDiscover discover = new GlobalSearchDiscover(jobContext);
                    discover.Run();
                    //if (NeedProcessAll(jobContext.Action, jobContext.SourceFlag, jobContext.NeedDiscover))
                    //{
                    //    ProcessAllData(discover, action, jobContext);
                    //}
                    //else
                    {
                        await ProcessDataInBatchAsync(discover, action, jobContext); 
                    }
                }
                else
                {
                    logger.Info($"Run without discover. Item count:{jobContext.RecordIds?.Count}");
                    //for sp move or fs folder reclassify, when RecordIds is not null, do not need discover
                    await ProcessSpecificDataAsync(action, jobContext);
                }
                logger.Info("Global search action finished.");
            }
            catch (Exception e)
            {
                mHasError = true;
                logger.Error($"An error occurred while running global search action job. Error:{e.ToString()}");
            }
            finally
            {
                UpdateJobState(action);
            }
        }

        private async System.Threading.Tasks.Task ProcessDataInBatchAsync(GlobalSearchDiscover discover, IGlobalSearchAction action, GlobalSearchActionDto dto)
        {
            while (true)
            {
                if (GlobalSearchCache.Instance.DiscoverCache.Count >= 100)
                {
                    var data = GlobalSearchCache.Instance.DiscoverCache.Take(100).ToList();
                    var realExtension = AppendSourceRecordsForSPMove(dto.Action, (SourceFlag)dto.SourceFlag, dto.ActionExtension, data);
                    logger.Info($"Start to process {data.Count} items.");
                    await action.DoActionAsync(data, (SourceFlag)dto.SourceFlag, dto.ActionExtension, mJobId, true);
                }
                else
                {
                    if (discover.DiscoverFinish)
                    {
                        var data = GlobalSearchCache.Instance.DiscoverCache.TakeAll().ToList();
                        if (data.Count > 0)
                        {
                            var realExtension = AppendSourceRecordsForSPMove(dto.Action, (SourceFlag)dto.SourceFlag, dto.ActionExtension, data);
                            logger.Info($"Start to process {data.Count} items.");
                            await action.DoActionAsync(data, (SourceFlag)dto.SourceFlag, realExtension, mJobId, true);
                        }
                        break;
                    }
                    else
                    {
                        Thread.Sleep(5000);
                    }
                }
            }
        }

        //maybe need to separate job later
        //private bool NeedProcessAll(GlobalSearchAction action, SourceFlag flag, bool needDiscover)
        //{
        //    ////sp move
        //    //if (action == GlobalSearchAction.MoveTo && flag == SourceFlag.SharePoint)
        //    //{
        //    //    return true;
        //    //}

        //    if (flag == SourceFlag.FileSystem && action == GlobalSearchAction.Reclassify && needDiscover)
        //    {
        //        //fs folder reclassify
        //        return true;
        //    }
        //    return false;
        //}

        private object AppendSourceRecordsForSPMove(GlobalSearchAction action, SourceFlag flag, object actionExtension, List<BaseRecordDto> data)
        {
            if (action == GlobalSearchAction.MoveTo && (flag == SourceFlag.SharePoint || flag == SourceFlag.OneDrive || flag == SourceFlag.Teams || flag == SourceFlag.Groups))
            {
                // DAOAPIClientV1 docAveClient = new DAOAPIClientV1();
                RMExplorerMoveJobMessage msg = SerializerHelper.DeserializeByDataContractSerializer<RMExplorerMoveJobMessage>(actionExtension.ToString());
                msg.SourceRecords.Clear();
                foreach (var rec in data)
                {
                    RemoteSiteCollection site = null;
                    if (sitesDic.ContainsKey(rec.AveSiteId))
                    {
                        site = sitesDic[rec.AveSiteId];
                    }
                    else
                    {
                        site = RABrowserClient.GetRemoteSiteCollectionById(rec.AveSiteId);
                        //docAveClient.GetRemoteSiteCollectionById(rec.AveSiteId);
                        if (site != null)
                        {
                            sitesDic.Add(site.id, site);
                        }
                    }
                    var siteUrl = string.Empty;
                    if (site != null)
                    {
                        siteUrl = site.url;
                    }
                    else
                    {
                        logger.Warn("get source record site error, site id: {0}, id: {1}", rec.AveSiteId, rec?.Id );
                    }
                    msg.SourceRecords.Add(new SourceRecord()
                    {
                        SourceFlag = (RecordFlag)rec.SourceFlag,
                        AveSiteId = rec.AveSiteId,
                        DeclareAsRecord = rec.DeclareAsRecord,
                        DirPath = rec.DirPath,
                        DisposalAction = rec.DisposalAction,
                        DisposalDueDate = rec.DisposalDueDate,
                        FolderId = rec.FolderId,
                        FullPath = rec.FullPath,
                        HoldStatus = rec.HoldStatus,
                        Id = rec.Id,
                        ItemId = rec.ItemId,
                        ItemRowId = rec.ItemRowId,
                        LeafName = rec.LeafName,
                        ListId = rec.ListId,
                        MetaInfo = rec.MetaInfo,
                        NodeId = rec.NodeId,
                        NodeType = rec.NodeType,
                        RecordsId = rec.RecordsId,
                        ReleaseTime = rec.ReleaseTime,
                        RuleId = rec.RuleId,
                        RuleName = rec.RuleName,
                        ScopeId = rec.ScopeId,
                        TermId = rec.TermId,
                        TermName = rec.TermName,
                        TimeCreated = rec.TimeCreated,
                        TimeLastModified = rec.TimeLastModified,
                        WebId = rec.WebId,
                        SiteUrl = siteUrl,
                    });
                }

                return SerializerHelper.SerializeByDataContractSerializer(msg);
            }
            return actionExtension;
        }

        private void UpdateJobState(IGlobalSearchAction action)
        {
            JobStatus status = JobStatus.Failed;
            if (action != null && !mHasError)
            {
                int successCount = action.GetSuccessCount();
                int failedCount = action.GetFailedCount();
                if (failedCount > 0 && successCount == 0)
                {
                    status = JobStatus.Failed;
                }
                else if (failedCount > 0 && successCount > 0)
                {
                    status = JobStatus.FinishWithException;
                }
                else
                {
                    status = JobStatus.Finished;
                }
            }
            ReportManager.SetJobFinished(status);
        }

        private async System.Threading.Tasks.Task ProcessSpecificDataAsync(IGlobalSearchAction action, GlobalSearchActionDto dto)
        {
            TenantLocalValue.LogonUserId = dto.UserId;
            if ((SourceFlag)dto.SourceFlag == SourceFlag.FileSystem && dto.Action == GlobalSearchAction.Reclassify)
            {
                ChangeTermOption changeTermDto = SerializerHelper.DeserializeByDataContractSerializer<ChangeTermOption>(dto.ActionExtension.ToString());
                List<BaseRecordDto> records = new List<BaseRecordDto>();
                foreach (var id in changeTermDto.SourceFSRecordIds)
                {
                    records.Add(new BaseRecordDto()
                    {
                        Id = id,
                        NodeId = id,
                        NodeType = (int)NodeLevel.FSFolder
                    });
                }
                await action.DoActionAsync(records, (SourceFlag)dto.SourceFlag, dto.ActionExtension, mJobId, true);
            }
            else if (((SourceFlag)dto.SourceFlag == SourceFlag.SharePoint || (SourceFlag)dto.SourceFlag == SourceFlag.OneDrive || (SourceFlag)dto.SourceFlag == SourceFlag.Teams || (SourceFlag)dto.SourceFlag == SourceFlag.Groups) && dto.Action == GlobalSearchAction.MoveTo)
            {
                if (dto.SourceFlag == (int)SourceFlag.Teams || dto.SourceFlag == (int)SourceFlag.Groups)
                {
                    dto.SourceFlag = (int)SourceFlag.SharePoint;
                }
                //var records = ExplorerDao.GetRecordByIds(dto.RecordIds);
                //List<BaseRecordDto> baseRecords = new List<BaseRecordDto>();
                //foreach (var record in records)
                //{
                //    baseRecords.Add(ConvertUtil.ConvertToBaseRecordDto(record));
                //}
                //var realExtension = GetActionExtension(dto.Action, dto.SourceFlag, dto.ActionExtension, baseRecords);
                await action.DoActionAsync(null, (SourceFlag)dto.SourceFlag, dto.ActionExtension, mJobId, true);
            }
            else if ((SourceFlag)dto.SourceFlag == SourceFlag.Physical && dto.Action == GlobalSearchAction.AccessControl)
            {
                List<BaseRecordDto> records = new List<BaseRecordDto>();
                if (dto.RecordIds != null && dto.RecordIds.Count > 0)
                {
                    foreach (var id in dto.RecordIds)
                    {
                        records.Add(new BaseRecordDto()
                        {
                            Id = id,
                            NodeId = id
                        });
                    }
                }
                ScopePermissionJobContextDto contextDto = null;
                if (!dto.ForceDiscoverAll)
                {
                    contextDto = SerializerHelper.DeserializeByDataContractSerializer<ScopePermissionJobContextDto>(dto.ActionExtension.ToString());
                    contextDto.GSJobContextDto.QueryV3Dto = null;
                    contextDto.GSJobContextDto.QueryDto = null;
                }
                await action.DoActionAsync(records, (SourceFlag)dto.SourceFlag, dto.ForceDiscoverAll ? dto.ActionExtension.ToString() : SerializerHelper.SerializeByDataContractSerializer(contextDto), mJobId, true);
            }
            else if((SourceFlag)dto.SourceFlag == SourceFlag.SharePoint && dto.Action == GlobalSearchAction.Reclassify)
            {
                ChangeTermOption changeTermDto = SerializerHelper.DeserializeByDataContractSerializer<ChangeTermOption>(dto.ActionExtension.ToString());
                List<BaseRecordDto> records = new List<BaseRecordDto>();
                foreach (var id in changeTermDto.SourceRecordIds)
                {
                    records.Add(new BaseRecordDto()
                    {
                        Id = id,
                        NodeId = id,
                        NodeType = (int)NodeLevel.Folder
                    });
                }
                await action.DoActionAsync(records, (SourceFlag)dto.SourceFlag, dto.ActionExtension, mJobId, true);
            }
            else if ((SourceFlag)dto.SourceFlag == SourceFlag.OneDrive && dto.Action == GlobalSearchAction.Reclassify)
            {
                ChangeTermOption changeTermDto = SerializerHelper.DeserializeByDataContractSerializer<ChangeTermOption>(dto.ActionExtension.ToString());
                List<BaseRecordDto> records = new List<BaseRecordDto>();
                foreach (var id in changeTermDto.SourceOneDriveRecordIds)
                {
                    records.Add(new BaseRecordDto()
                    {
                        Id = id,
                        NodeId = id,
                        NodeType = (int)NodeLevel.Folder
                    });
                }
                await action.DoActionAsync(records, (SourceFlag)dto.SourceFlag, dto.ActionExtension, mJobId, true);
            }
            else if ((SourceFlag)dto.SourceFlag == SourceFlag.Teams && dto.Action == GlobalSearchAction.Reclassify)
            {
                ChangeTermOption changeTermDto = SerializerHelper.DeserializeByDataContractSerializer<ChangeTermOption>(dto.ActionExtension.ToString());
                List<BaseRecordDto> records = new List<BaseRecordDto>();
                foreach (var id in changeTermDto.SourceTeamsRecordIds)
                {
                    records.Add(new BaseRecordDto()
                    {
                        Id = id,
                        NodeId = id,
                        NodeType = (int)NodeLevel.Folder
                    });
                }
                await action.DoActionAsync(records, (SourceFlag)dto.SourceFlag, dto.ActionExtension, mJobId, true);
            }
            else if ((SourceFlag)dto.SourceFlag == SourceFlag.Google && dto.Action == GlobalSearchAction.Reclassify)
            {
                ChangeTermOption changeTermDto = SerializerHelper.DeserializeByDataContractSerializer<ChangeTermOption>(dto.ActionExtension.ToString());
                List<BaseRecordDto> records = new List<BaseRecordDto>();
                foreach (var id in changeTermDto.GoogleDriveRecordIds)
                {
                    records.Add(new BaseRecordDto()
                    {
                        Id = id,
                        NodeId = id,
                        NodeType = (int)AvePoint.RA.Contract.RMWeb.Tree.Base.RMNodeLevel.GoogleFolder,
                    });
                }
                await action.DoActionAsync(records, (SourceFlag)dto.SourceFlag, dto.ActionExtension, mJobId, true);
            }
        }
    }
}
