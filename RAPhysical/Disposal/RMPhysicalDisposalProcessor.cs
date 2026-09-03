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
using AvePoint.Common;
using AvePoint.GCommon.Contract.StorageOptimization.Object;
using AvePoint.GCommon.Utility;
using AvePoint.RA.Common;
using AvePoint.RA.Common.Report;
using AvePoint.RA.Common.Util;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract;
using AvePoint.RA.Contract.Archiver;
using AvePoint.RA.Contract.Exceptions;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.Contract.FileSystem;
using AvePoint.RA.Contract.FunctionSetting;
using AvePoint.RA.Contract.JobMonitor;
using AvePoint.RA.Contract.Object.JobMessage;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using AvePoint.RA.Contract.RMWeb.TemplateManagement;
using AvePoint.RA.Contract.RMWeb.Tree.Base;
using AvePoint.RA.Contract.Schedule;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Dao.Impl;
using AvePoint.RA.DB.Explorer.Dao;
using AvePoint.RA.DB.Explorer.Dao.CosmosImp;
using AvePoint.RA.DB.Explorer.Model;
using AvePoint.RA.DB.Model;
using AvePoint.RA.I18N.Core;
using AvePoint.RA.RACommonUtility.Email.Sender;
using AvePoint.RA.RACommonUtility.Email.Sender.Middleware;
using AvePoint.RA.RACommonUtility.Email.Sender.Storage;
using AvePoint.RA.RAPhysical.API;
using AvePoint.RA.RAPhysical.Common;
using AvePoint.RA.RAPhysical.Discover;
using AvePoint.RA.RAPhysical.Discover.DiscoverImps;
using AvePoint.RA.RAPhysical.Disposal.PhysicalDisposalActionImps;
using AvePoint.RA.SharePoint.ArchiverCommon;
using AvePoint.Wrapper.Common;
using RAArchiverCommon.DisposalProgress;
using RAArchiverCommon.DisposalProgress.Impl;
using RAManualApprovalCommon.Archiver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ArgumentCheck = AvePoint.GCommon.Utility.ArgumentCheck;
using DAContract = AvePoint.GCommon.Contract.StorageOptimization.Object;
using SOApproveDBStatus = AvePoint.RA.Contract.SOApproveDBStatus;

namespace AvePoint.RA.RAPhysical.Disposal
{
    public class RMPhysicalDisposalProcessor
    {
        private static readonly AvePoint.RA.Contract.Services.IRALogger logger = RALogger.GetInstance(typeof(RMPhysicalDisposalProcessor));
        private int mLocationId { get; set; }
        private IPhysicalLocation CurrentLocation { get; set; }

        private IRMPhysicalPushColumnDao mRMPhysicalPushColumnDao;
        protected IRMPhysicalPushColumnDao RMPhysicalPushColumnDao
        {
            get
            {
                if (mRMPhysicalPushColumnDao == null)
                {
                    mRMPhysicalPushColumnDao = (IRMPhysicalPushColumnDao)PlatformWindsorManager.GetService(typeof(IRMPhysicalPushColumnDao));
                }
                return mRMPhysicalPushColumnDao;
            }
        }

        private IRecordLoanAllianceDao mRecordLoanAllianceDao;
        public IRecordLoanAllianceDao RecordLoanAllianceDao
        {
            get
            {
                if (mRecordLoanAllianceDao == null)
                {
                    mRecordLoanAllianceDao = (IRecordLoanAllianceDao)PlatformWindsorManager.GetService(typeof(IRecordLoanAllianceDao));
                }
                return mRecordLoanAllianceDao;
            }
        }

        private IRelativeDataArchiverService EnduserArchiverAction => PlatformWindsorManager.GetService<IRelativeDataArchiverService>();

        private IExplorerService mExplorerService;
        protected IExplorerService ExplorerService
        {
            get
            {
                if (mExplorerService == null)
                {
                    mExplorerService = (IExplorerService)PlatformWindsorManager.GetService(typeof(IExplorerService));
                }
                return mExplorerService;
            }
        }
        //protected RMPhysicalDisposalJobMessage PhysicalJobMessage { get; set; }
        protected bool mJobHasException = false;
        protected bool mJobHasSuccessNode = false;

        //Consider Need Sub Job or not.
        //private IRMSubJobDao mSubJobDao { set; get; }
        //public IRMSubJobDao SubJobDao
        //{
        //    get
        //    {
        //        if (mSubJobDao == null)
        //        {
        //            mSubJobDao = (IRMSubJobDao)PlatformWindsorManager.GetService(typeof(IRMSubJobDao));
        //        }
        //        return mSubJobDao;
        //    }
        //}
        private IRMReportManager mReportManger;
        public IRMReportManager ReportManager
        {
            get
            {
                if (mReportManger == null)
                {
                    mReportManger = ReportMangerFactory.Instance.ReportManager;
                }
                return mReportManger;
            }
        }

        private IExplorerDao ExplorerDao = new ExplorerDao();

        private IPhysicalRecordSettingDao mPhysicalRecordSettingDao;
        protected IPhysicalRecordSettingDao PhysicalRecordSettingDao
        {
            get
            {
                if (mPhysicalRecordSettingDao == null)
                {
                    mPhysicalRecordSettingDao = (IPhysicalRecordSettingDao)PlatformWindsorManager.GetService(typeof(IPhysicalRecordSettingDao));
                }
                return mPhysicalRecordSettingDao;
            }
        }

        private IRMScheduleDao mRMScheduleDao;
        protected IRMScheduleDao RMScheduleDao
        {
            get
            {
                if (mRMScheduleDao == null)
                {
                    mRMScheduleDao = (IRMScheduleDao)PlatformWindsorManager.GetService(typeof(IRMScheduleDao));
                }
                return mRMScheduleDao;
            }
        }

        private ITemplateManagementService mTemplateManagementService { get; set; }
        public ITemplateManagementService TemplateManagementService
        {
            get
            {
                if (mTemplateManagementService == null)
                {
                    mTemplateManagementService = (ITemplateManagementService)PlatformWindsorManager.GetService(typeof(ITemplateManagementService));
                }
                return mTemplateManagementService;
            }
        }

        private IRecordsHistoryService RecordsHistoryService => PlatformWindsorManager.GetService<IRecordsHistoryService>();
        //Currently only have fulldiscover .Use factory to create more kind of discover objs
        protected IPhysicalDiscover PhysicalDiscover = new PhysicalFullDiscover();
        protected IPhysicalDisposalAction disposalAction;
        private DateTime mRunJobTime;
        private bool skipRemoveContentAndDestroyAction = false;
        private string jobId;
        private JobRunBy jobRunBy;
        private readonly PhysicalArchiverManualAction mManualAction;
        private static IRMFunctionSettingDao FunctionSettingDao => PlatformWindsorManager.GetService<IRMFunctionSettingDao>();
        public RMPhysicalDisposalProcessor(string jobId, int locationId, bool skipRemoveContentAndDestroyAction, JobRunBy jobRunBy)
        {
            //ReportMangerFactory.Instance.Init(jobId, AvePoint.RA.Contract.JobMonitor.JobType.PhysicalDisposal);
            ReportManager.StartUpdateJobProgress();
            mLocationId = locationId;
            mRunJobTime = DateTime.UtcNow;
            this.skipRemoveContentAndDestroyAction = skipRemoveContentAndDestroyAction;
            this.jobId = jobId;
            this.jobRunBy = jobRunBy;
            var mainJobId = jobId.Split("_", StringSplitOptions.RemoveEmptyEntries).First();
            CompoundDisposalStatistics.Instance.Init(new DisposalStaticInitObject()
            {
                JobType = JobType.PhysicalRecordsDisposal,
                MainJobId = mainJobId,
                SubJobId = jobId
            });
            WrapperConfiguration.IsRecheckRule = true;
            if (WrapperConfiguration.IsProcessApprovalDatasOnly)
            {
                var isRecheckRule = FunctionSettingDao.GetSettingInfo(FunctionSettingType.IsRecheckRule).GetAwaiter().GetResult();
                if (!string.IsNullOrEmpty(isRecheckRule))
                {
                    bool result = Convert.ToBoolean(isRecheckRule);
                    WrapperConfiguration.IsRecheckRule = result;
                }
                else
                {
                    WrapperConfiguration.IsRecheckRule = true;//the old setting need to check rule
                }
                logger.Info($"current is recheck rule status is :{WrapperConfiguration.IsRecheckRule}");
            }
            try
            {
                mManualAction = new PhysicalArchiverManualAction(mainJobId);
            }
            catch (Exception e)
            {
                if ((e.InnerException ?? e).Message == "RM_MA_NotFound_CustomApp")
                {
                    logger.Warn($"SetRecordIsAutoApproval ERROR, RM_MA_NotFound_CustomApp");
                    throw new Exception(I18NEntity.GetString("RM_MA_NotFound_CustomApp"));
                }

                throw;
            }
        }
        public async Task RunNowAsync()
        {
            //Get Job Setting.
            logger.Info($"Start physical disposal job ,Location {mLocationId}");
            try
            {
                CompoundDisposalStatistics.Instance.StartStatistic();
                disposalAction = new PhyscialDisposalAction(mRunJobTime, this.jobId, this.jobRunBy);
                CurrentLocation = new PhysicalLocation(mLocationId);
                await ProcessLocationAsync(CurrentLocation);
                var _emailSender = new RMEmailSender(new RMEmailRedisStorage(jobId, new RMEMailStorageManualMiddleware()));
                await _emailSender.SendAsync();
            }
            catch (JobStopException)
            {
                throw new JobStopException("This Job is stopped.");
            }
            catch (Exception e)
            {
                logger.Warn($"Run disposal job failed {e.ToString()}");
            }
            finally
            {
                CompoundDisposalStatistics.Instance.PrepareEndStatistic();
                //CosmosDBManualDataUpdater.WaitComplete();
                EnduserArchiverAction.UpdloadDestructionCache();
                var jobStatus = JobStatus.Finished;
                mJobHasSuccessNode |= disposalAction.HasMoveSuccess();
                mJobHasException |= disposalAction.HasMoveFailed();
                if (mJobHasSuccessNode)
                {
                    jobStatus = mJobHasException ? JobStatus.FinishWithException : JobStatus.Finished;
                }
                else
                {
                    jobStatus = mJobHasException ? JobStatus.Failed : JobStatus.Finished;
                }
                CompoundDisposalStatistics.Instance.WaitEndStatistic();
                ReportManager.SetJobFinished(jobStatus, jobStatus != JobStatus.Finished ? "RM_JMD_ContentDueSummary" : string.Empty);
            }
        }
        public async Task ProcessLocationAsync(IPhysicalLocation location)
        {
            using (var performance = new PerformanceScope("RMPhysicalDisposalProcessor.ProcessLocation", addToStatistics: true))
            {
                using (new CheckJobStopScope()) { }
                bool isRunJobNode = location.IntId == mLocationId;
                if (!isRunJobNode)
                {
                    bool isBreakInherit = RMScheduleDao.GetPhysicalScheduleByLocationId(location.UniqueId) != null;
                    if (isBreakInherit)
                    {
                        logger.Info($"Location is break inherit. Location Path:[{location?.UniqueId}]");
                        return;
                    }
                }
                ReportManager.IncreaseBase(1);
                if (location.IsBottomLocation)
                {
                    logger.Info($"Process bottom location {location.DirPath}");
                    List<IPhysicalBox> boxsUnderLocation = null;
                    using (var performance0 = new PerformanceScope("RMPhysicalDisposalProcessor.GetBoxs", addToStatistics: true))
                    {
                        boxsUnderLocation = PhysicalDiscover.GetBoxs(location);
                    }
                    ReportManager.IncreaseBase(boxsUnderLocation.Count);
                await boxsUnderLocation.ForEachAsync(async b =>
                    {
                    await ProcessBoxAsync(b);
                        ReportManager.Increase(1);
                    });

                    List<IPhysicalFile> filesUnderLocation = null;
                    using (var performance0 = new PerformanceScope("RMPhysicalDisposalProcessor.GetPhysicalFiles", addToStatistics: true))
                    {
                        filesUnderLocation = PhysicalDiscover.GetPhysicalFiles(location);
                    }
                    ReportManager.IncreaseBase(filesUnderLocation.Count);
                await filesUnderLocation.ForEachAsync(async b =>
                    {
                    await ProcessFileAsync(b);
                        ReportManager.Increase(1);
                    });
                    return;
                }
                logger.Info($"Process location {location.DirPath}");
                ReportManager.IncreaseBase(location.AllSubLocations.Count);
            await location.AllSubLocations.ForEachAsync(async l =>
                {
                await ProcessLocationAsync(l);
                    ReportManager.Increase(1);
                });
                ReportManager.Increase(1);
            }
        }
        public async Task ProcessBoxAsync(IPhysicalBox box)
        {
            using (var performance0 = new PerformanceScope("RMPhysicalDisposalProcessor.ProcessBox", addToStatistics: true))
            {
                using (new CheckJobStopScope()) { }
                List<Rule> rules;
                ReportManager.IncreaseBase(1);
                PhysicalDisposalActionType physicalAction = PhysicalDisposalActionType.None;
                var boxOriginFullPath = box.DirPath;
                logger.Info($"Process box {box?.Id}");
                Rule physicalRule = null;
                try
                {
                    var boxTermId = box.TermId;
                    if (RMPhysicalDisposalCache.Instance.TermRuleMapping.TryGetValue(boxTermId, out rules))
                    {
                        //此处逻辑需要优化，避免每个template 都获取一次，应该做global 级别的缓存
                    var template = await TemplateManagementService.LoadTemplateDtoAsync(box.TemplateId);
                        var columnCollection = new Dictionary<Guid, TemplateColumnDto>();
                        template.categories.ForEach(cat =>
                        {
                            cat.columns.ForEach(col => columnCollection[col.uniqueId] = col);
                        });

                        PhysicalRuleEngine engine = new PhysicalRuleEngine(rules);
                        Dictionary<Guid, List<RMPhysicalPushColumn>> columnIdAndPushColumn = new Dictionary<Guid, List<RMPhysicalPushColumn>>();
                        foreach (var fieldKey in box.Fields.Keys)
                        {
                            Guid fieldId;
                            if (Guid.TryParse(fieldKey, out fieldId))
                            {
                                if (columnCollection != null && columnCollection.ContainsKey(fieldId))
                                {
                                    var column = columnCollection[fieldId];
                                    if (column.pushToChild)
                                    {
                                        List<Guid> physicObjectIds = new List<Guid>();
                                        physicObjectIds.Add(box.Id);
                                        List<RMPhysicalPushColumn> pushColumn = RMPhysicalPushColumnDao.GetPushColumns(column.uniqueId, physicObjectIds);
                                        columnIdAndPushColumn[column.uniqueId] = pushColumn;
                                    }
                                }
                            }
                        }
                        var boxFilterObj = PhysicalObjectConvertor.ConvertPhysicalBoxFilterObject(engine.FilterPolicyCollection, box, columnCollection, columnIdAndPushColumn);
                        if (WrapperConfiguration.IsProcessApprovalDatasOnly && !WrapperConfiguration.IsRecheckRule)
                        {

                            logger.Info($"this item will not check rule ,and the record not null,ruleid:{box.RuleId.ToString()}");
                            physicalRule = engine.GetRuleFromRuleCollectionByRuleId(box.RuleId.ToString());
                            if (physicalRule == null)
                            {
                                logger.Info($"this item will not check rule ,ruleid:{box.RuleId.ToString()},can not find the rule in the rule result by rule id");
                            }
                        }
                        else
                        {
                            physicalRule = engine.CheckRule(boxFilterObj);//Imps objct
                        }
                        if (physicalRule != null && NeedSkipCurrentRule(physicalRule))
                        {
                            logger.Info("Current object fit rule:{0} and SkipRemoveContentAndDestroyAction is true.", physicalRule.Name);
                            SendJobDetail(box.Name, box.DirPath, physicalRule.Name, PhysicalDisposalActionType.Disposal, String.Empty,
                                  "RM_Common_ObjectLevel_PhysicalBox", JobDetailsStatus.Skipped, "StorageOptimization_SkipRemoveContentAndDestroyAction");
                            physicalRule = null;
                        }
                        physicalAction = await GetDisposalActionAsync(physicalRule, box.RuleId.ToString(), box.ManualApprovedStatus, box.Id, box.ManualExtendTime,box.ModifiedTimeTicks);
                        //CosmosDBManualDataUpdater.Commit();

                        switch (physicalAction)
                        {
                            case PhysicalDisposalActionType.Disposal:
                                bool onHold = IsRecordsHold(box, mRunJobTime.Ticks);
                                if (onHold)
                                {
                                    SendJobDetail(box.Name,
                                        box.DirPath,
                                        physicalRule?.Name,
                                        physicalAction,
                                        string.Empty,
                                        "RM_Common_ObjectLevel_PhysicalBox",
                                        JobDetailsStatus.Skipped,
                                        "RM_PRM_Disposal_SkipHoldBox");
                                }
                                else
                                {
                                    var actionAudit = disposalAction.DisposalBox(box, physicalRule, SendJobDetail);
                                    if (actionAudit != null)
                                    {
                                        RecordsHistoryService.AddPhysicalAudit([actionAudit]);
                                    }
                                }
                                break;
                            case PhysicalDisposalActionType.EmptyRuleInfo:
                                disposalAction.EmptyBoxRuleInfo(box);
                                foreach (var file in PhysicalDiscover.GetPhysicalFiles(box))
                                {
                                    await ProcessFileAsync(file);
                                }
                                break;
                            case PhysicalDisposalActionType.Move:
                                if (BoxUnderContainer(box))
                                {
                                    logger.Info($"Current box is under custom container, will not move. Id:{box.Id}");
                                    SendJobDetail(box.Name,
                                      box.DirPath,
                                      physicalRule?.Name,
                                      physicalAction,
                                      string.Empty,
                                      "RM_Common_ObjectLevel_PhysicalBox",
                                      JobDetailsStatus.Skipped,
                                      "RM_PRM_Disposal_SkipBoxUnderContainer");
                                    return;
                                }
                                ArgumentCheck.NotNull(physicalRule, nameof(physicalRule));
                                DAContract.ConflictOption conflictOption = physicalRule.PhysicalRule.spMoveOption.MoveSetting.ItemLevelConflictOption;
                                PhysicalHoldConflictOption physicalHoldConflictOption = physicalRule.PhysicalRule.spMoveOption.MoveSetting.PhysicalHoldConflictOption;
                                var locationId = new Guid(physicalRule.PhysicalRule.spMoveOption.MoveDestination.PhysicalTree.LocationId);
                                await disposalAction.MoveBoxAsync(box, locationId, physicalRule?.Name, conflictOption, SendJobDetail, physicalHoldConflictOption);
                                break;
                            case PhysicalDisposalActionType.Pending:
                                disposalAction.PendingBox(box, physicalRule, SendJobDetail);
                                break;
                            case PhysicalDisposalActionType.Waitingapproval:
                                if (box.ManualExtendTime >= DateTime.UtcNow.Ticks && physicalRule != null && physicalRule.Id.Equals(box.RuleId.ToString()))
                                {
                                    logger.Info($"Item:{box.Id} match manual rule, but is extend time data.");
                                }
                                else
                                {
                                    SendJobDetail(box.Name,
                                            box.DirPath,
                                            physicalRule?.Name,
                                            physicalAction,
                                            string.Empty,
                                            "RM_Common_ObjectLevel_PhysicalBox",
                                            JobDetailsStatus.Skipped,
                                            "RM_JM_FSFileWaitingForApproval");
                                }
                                return;
                            case PhysicalDisposalActionType.None:
                                if (physicalRule == null)
                                {
                                    foreach (var file in PhysicalDiscover.GetPhysicalFiles(box))
                                    {
                                        await ProcessFileAsync(file);
                                    }
                                }
                                else
                                {
                                    //例如waiting approve数据没换Rule状态也是None，此时不需要处理
                                }
                                return;
                            case PhysicalDisposalActionType.IsProcessApprovalDatasOnly:
                                break;
                        }
                    }
                    else
                    {
                        var filesInBox = PhysicalDiscover.GetPhysicalFiles(box);
                        ReportManager.IncreaseBase(filesInBox.Count);
                    await filesInBox.ForEachAsync(async f =>
                        {
                        await ProcessFileAsync(f);
                            ReportManager.Increase(1);
                        });
                    }
                }
                catch (JobStopException)
                {                   
                    throw new JobStopException("This Job is stopped.");
                }
                catch (Exception e)
                {
                    mJobHasException = true;
                    logger.Warn($"Disposal box failed {boxOriginFullPath} : {e.ToString()}");
                    ReportManager.SendJobDetail(new JMPhysicalDisposalJobDetails()
                    {
                        ObjectName = box.Name,
                        FullPath = boxOriginFullPath,
                        ActionType = GetI18NActionType(physicalAction),
                        DestinationPath = "",
                        ItemType = "RM_Common_ObjectLevel_PhysicalBox",
                        Status = JobDetailsStatus.Failed,
                        Comment = e.Message,
                        RuleName = physicalRule?.Name
                    });
                }
                finally
                {
                    ReportManager.Increase(1);
                }
            }
        }

        public bool IsRecordsHold(IPhysicalFile file, long ticks)
        {
            bool IsRecordsHold = false;
            logger.Info("IsRecordsHold.");
            try
            {
                //List<RMRecordAlliance> rMRecordAlliances = GetAllRMRecordAlliance();
                //int disposalCount = rMRecordAlliances.Count(a => a.HoldReleaseTime > ticks && ids.Any(temp => temp == a.RecordsId));
                if (file.HoldStatus && file.HoldReleaseTime > ticks
                    || (file.ParentBox != null && file.ParentBox.HoldStatus && file.ParentBox.HoldReleaseTime > ticks))
                {
                    return true;
                }
                List<Guid> ids = new List<Guid>();
                ids.Add(file.Id);
                if (file.ParentBox != null)
                {
                    ids.Add(file.ParentBox.Id);
                }
                List<RMRecordLoanAlliance> loanAlliances = GetPhyRecordAllianceByIds(ids);
                int loanCount = loanAlliances.Count;
                return loanCount > 0;
            }
            catch (Exception ex)
            {
                logger.Info("Failed IsRecordsHold.Message:{0}. ", ex.ToString());
            }
            return IsRecordsHold;
        }
        public bool IsRecordsHold(IPhysicalBox box, long ticks)
        {
            bool IsRecordsHold = false;
            logger.Info("IsRecordsHold.");
            try
            {
                //List<RMRecordAlliance> rMRecordAlliances = GetAllRMRecordAlliance();
                //int disposalCount = rMRecordAlliances.Count(a => a.HoldReleaseTime > ticks && ids.Any(temp => temp == a.RecordsId));
                if (box.HoldStatus && box.HoldReleaseTime > ticks)
                {
                    return true;
                }
                List<Guid> ids = new List<Guid>() { box.Id };
                List<RMRecordLoanAlliance> loanAlliances = GetPhyRecordAllianceByIds(ids);
                int loanCount = loanAlliances.Count;
                return loanCount > 0;
            }
            catch (Exception ex)
            {
                logger.Info("Failed IsRecordsHold.Message:{0}. ", ex.ToString());
            }
            return IsRecordsHold;
        }

        public List<RMRecordLoanAlliance> GetPhyRecordAllianceByIds(List<Guid> ids)
        {
            logger.Info("GetPhyRecordAllianceByIds.");
            List<RMRecordLoanAlliance> loanAlliances = new List<RMRecordLoanAlliance>();
            loanAlliances = RecordLoanAllianceDao.GetPhyRecordAllianceByIds(ids);
            return loanAlliances.Where(a => ids.Any(temp => temp == a.RecordsId)).ToList();
        }

        private Record GetRecord(Guid id)
        {
            var rec = ExplorerDao.GetFirstOrDefault(r => r.Id == id);
            if (rec != null)
            {
                rec.ExtensionForFile = GetFileExtension(rec);
            }
            return rec;
        }

        private string GetFileExtension(Record record)
        {
            if (!string.IsNullOrEmpty(record.ExtensionForFile))
            {
                return record.ExtensionForFile;
            }

            switch ((RMNodeLevel)record.NodeType)
            {
                case RMNodeLevel.ExchangeOnlineItem:
                    return "msg";
                //case RMNodeLevel.Item:
                //    if (record.ArchiveLevel == (int)CacheNodeType.Item)
                //    {
                //        return "RM_RDM_RecordDetails_DataType_SPItem";
                //    }
                //    var ext = Path.GetExtension(record.LeafName);
                //    return ext.Contains('.', StringComparison.CurrentCulture) ? ext[1..] : "RM_RDM_RecordDetails_DataType_FileNull";
                case RMNodeLevel.SiteCollection:
                    return "RM_JS_Rule_ObjectLevel_SiteCollection";
                case RMNodeLevel.Site:
                    return "RM_JS_Rule_ObjectLevel_Site";
                case RMNodeLevel.List:
                    return "RM_Common_ObjectLevel_List";
                case RMNodeLevel.Folder:
                    return "RM_Common_ObjectLevel_Folder";
                case RMNodeLevel.FSFolder:
                    return "RM_RDM_RecordDetails_DataType_FSFolder";
                //case RMNodeLevel.FSFile:
                //    var fsExt = Path.GetExtension(record.LeafName);
                //    if (fsExt.Contains('.', StringComparison.CurrentCulture))
                //    {
                //        return fsExt[1..];
                //    }
                //    return "";
                case RMNodeLevel.PhysicalBox:
                    return "RM_PRM_PRE_Filter_PhysicalBox";
                case RMNodeLevel.PhysicalFile:
                    return "RM_PRM_PRE_Filter_PhysicalFile";
                case RMNodeLevel.PhysicalRecord:
                    return "RM_PRM_PRE_Filter_PhysicalRecord";
                case RMNodeLevel.PhysicalCustom:
                    return "RM_PRM_PRE_TableItemType_Container";
                case RMNodeLevel.CustomizeConnectorItem:
                    return "RM_Connector_ItemLevel_Item";
            }


            return "";
        }

        private bool BoxUnderContainer(IPhysicalBox box)
        {
            if (box.Ancestors != null && box.ParentId != box.LocationId)
            {
                return true;
            }
            return false;
        }

        private bool NeedSkipCurrentRule(Rule rule)
        {
            bool needSkipCurrentRule = false;
            if (
                rule != null
                && skipRemoveContentAndDestroyAction
                && !(rule.PhysicalRule != null && rule.PhysicalRule.spMoveOption != null && rule.PhysicalRule.spMoveOption.MoveDestination != null))//Move Rule  

            {
                needSkipCurrentRule = true;
            }
            return needSkipCurrentRule;
        }
        public void SendJobDetail(string name, string originPath, string ruleName, PhysicalDisposalActionType action, string destinationPath, string ItemType, JobDetailsStatus status, string comment = "")
        {
            mJobHasSuccessNode = mJobHasSuccessNode ? mJobHasSuccessNode : status == JobDetailsStatus.Successful;
            mJobHasException = mJobHasException ? mJobHasException : status == JobDetailsStatus.Failed;

            ReportManager.SendJobDetail(new JMPhysicalDisposalJobDetails()
            {
                ObjectName = name,
                FullPath = originPath,
                RuleName = ruleName,
                ActionType = GetI18NActionType(action),
                DestinationPath = destinationPath,
                ItemType = ItemType,
                Status = status,
                Comment = comment
            });
        }

        public async Task ProcessFileAsync(IPhysicalFile file)
        {
            using (var performance0 = new PerformanceScope("RMPhysicalDisposalProcessor.ProcessFile", addToStatistics: true))
            {
                using (new CheckJobStopScope()) { }
                List<Rule> rules;
                ReportManager.IncreaseBase(1);
                PhysicalDisposalActionType physicalAction = PhysicalDisposalActionType.None;
                var fileOriginFullPath = file.DirPath;
                logger.Info($"Process File {fileOriginFullPath}");
                Rule physicalRule = null;
                try
                {
                    var fileTermId = file.TermId;
                    if (RMPhysicalDisposalCache.Instance.TermRuleMapping.TryGetValue(fileTermId, out rules))
                    {
                        PhysicalRuleEngine engine = new PhysicalRuleEngine(rules);
                        //此处逻辑需要优化，避免每个template 都获取一次，应该做global 级别的缓存
                        var columnIdAndNameMapping = new Dictionary<Guid, TemplateColumnDto>();
                        Dictionary<Guid, List<RMPhysicalPushColumn>> columnIdAndPushColumn = new Dictionary<Guid, List<RMPhysicalPushColumn>>();
                        using (var performance = new PerformanceScope("RMPhysicalDisposalProcessor.LoadTemplateDto", addToStatistics: true))
                        {
                            var template = await TemplateManagementService.LoadTemplateDtoAsync(file.TemplateId);
                            if (file.BoxId != Guid.Empty)
                            {
                                ExplorerService.AddPushColumnToFold(template, file.BoxId);
                            }
                            template.categories.ForEach(cat =>
                            {
                                cat.columns.ForEach(col => columnIdAndNameMapping[col.uniqueId] = col);
                            });
                            foreach (var fieldKey in file.Fields.Keys)
                            {
                                Guid fieldId;
                                if (Guid.TryParse(fieldKey, out fieldId))
                                {
                                    if (columnIdAndNameMapping != null && columnIdAndNameMapping.ContainsKey(fieldId))
                                    {
                                        var column = columnIdAndNameMapping[fieldId];
                                        if (column.pushToChild)
                                        {
                                            List<Guid> physicObjectIds = new List<Guid>();
                                            if (column.inheritFromParent)
                                            {
                                                physicObjectIds.Add(file.BoxId);
                                            }
                                            else
                                            {
                                                physicObjectIds.Add(file.Id);
                                            }
                                            List<RMPhysicalPushColumn> pushColumn = RMPhysicalPushColumnDao.GetPushColumns(column.uniqueId, physicObjectIds);
                                            columnIdAndPushColumn[column.uniqueId] = pushColumn;
                                        }
                                    }
                                }
                            }
                        }
                        
                        var fileFilterObj = PhysicalObjectConvertor.ConvertPhysicalFileFilterObject(engine.FilterPolicyCollection, file, columnIdAndNameMapping, columnIdAndPushColumn);
                        if (WrapperConfiguration.IsProcessApprovalDatasOnly && !WrapperConfiguration.IsRecheckRule)
                        {

                            logger.Info($"this item will not check rule ,and the record not null,ruleid:{file.RuleId.ToString()}");
                            physicalRule = engine.GetRuleFromRuleCollectionByRuleId(file.RuleId.ToString());
                            if (physicalRule == null)
                            {
                                logger.Info($"this item will not check rule ,ruleid:{file.RuleId.ToString()},can not find the rule in the rule result by rule id");
                            }
                        }
                        else
                        {
                            using (var performance = new PerformanceScope("RMPhysicalDisposalProcessor.CheckRule", addToStatistics: true))
                            {
                                physicalRule = engine.CheckRule(fileFilterObj);
                            }
                        }
                        if (physicalRule != null && NeedSkipCurrentRule(physicalRule) && !physicalRule.IsCalculationDisposalDate)
                        {
                            logger.Info("Current object fit rule:{0} and SkipRemoveContentAndDestroyAction is true.", physicalRule.Name);
                            SendJobDetail(file.Name, file.DirPath, physicalRule.Name, PhysicalDisposalActionType.Disposal, String.Empty, "RM_Common_ObjectLevel_PhysicalFile", JobDetailsStatus.Skipped, "StorageOptimization_SkipRemoveContentAndDestroyAction");
                            physicalRule = null;
                        }
                        physicalAction = await GetDisposalActionAsync(physicalRule, file.RuleId.ToString(), file.ManualApprovedStatus, file.Id, file.ManualExtendTime,file.ModifiedTimeTicks);
                        //CosmosDBManualDataUpdater.Commit();
                        switch (physicalAction)
                        {
                            case PhysicalDisposalActionType.Disposal:
                                bool onHold = IsRecordsHold(file, mRunJobTime.Ticks);
                                if (onHold)
                                {
                                    SendJobDetail(file.Name,
                                        file.DirPath,
                                        physicalRule?.Name,
                                        PhysicalDisposalActionType.Disposal,
                                        string.Empty,
                                        "RM_Common_ObjectLevel_PhysicalFile",
                                        JobDetailsStatus.Skipped,
                                        "RM_PRM_Disposal_SkipHoldFolder");
                                }
                                else
                                {
                                    var actionAudit = disposalAction.DisposalFile(file, physicalRule, SendJobDetail, includeDeleteBlock : false);
                                    if (actionAudit != null)
                                    {
                                        RecordsHistoryService.AddPhysicalAudit([actionAudit]);
                                    }
                                }
                                break;
                            case PhysicalDisposalActionType.EmptyRuleInfo:
                                disposalAction.EmptyFileRuleInfo(file);
                                break;
                            case PhysicalDisposalActionType.Move:
                                if (FolderUnderContainer(file))
                                {
                                    logger.Info($"Current folder is under custom container, will not move. Id:{file.Id}");
                                    SendJobDetail(file.Name,
                                      file.DirPath,
                                      physicalRule?.Name,
                                      physicalAction,
                                      string.Empty,
                                      "RM_Common_ObjectLevel_PhysicalFile",
                                      JobDetailsStatus.Skipped,
                                      "RM_PRM_Disposal_SkipFolderUnderContainer");
                                    return;
                                }
                                Guid boxId = Guid.Empty;
                                ArgumentCheck.NotNull(physicalRule, nameof(physicalRule));
                                if (!string.IsNullOrEmpty(physicalRule.PhysicalRule.spMoveOption.MoveDestination.PhysicalTree.BoxId))
                                {
                                    boxId = new Guid(physicalRule.PhysicalRule.spMoveOption.MoveDestination.PhysicalTree.BoxId);
                                }
                                Guid locationId = new Guid(physicalRule.PhysicalRule.spMoveOption.MoveDestination.PhysicalTree.LocationId);
                                DAContract.ConflictOption conflictOption = physicalRule.PhysicalRule.spMoveOption.MoveSetting.ItemLevelConflictOption;
                                PhysicalHoldConflictOption physicalHoldConflictOption = physicalRule.PhysicalRule.spMoveOption.MoveSetting.PhysicalHoldConflictOption;
                                await disposalAction.MoveFileAsync(file, boxId, locationId, physicalRule.PhysicalRule.spMoveOption.MoveDestination.PhysicalTree.FullPath, physicalRule?.Name, conflictOption, SendJobDetail, physicalHoldConflictOption);
                                break;
                            case PhysicalDisposalActionType.Pending:
                                disposalAction.PendingFile(file, physicalRule, SendJobDetail);
                                break;
                            case PhysicalDisposalActionType.Waitingapproval:
                                if (file.ManualExtendTime > DateTime.UtcNow.Ticks)
                                {
                                    logger.Info($"Item:{file.Id} match manual rule, but is extend time data.");
                                }
                                else
                                {
                                    SendJobDetail(file.Name,
                                                file.DirPath,
                                                physicalRule?.Name,
                                                physicalAction,
                                                string.Empty,
                                                "RM_Common_ObjectLevel_PhysicalFile",
                                                JobDetailsStatus.Skipped,
                                                "RM_JM_FSFileWaitingForApproval");
                                }
                                break;
                                case PhysicalDisposalActionType.IsProcessApprovalDatasOnly:
                                    break;
                            case PhysicalDisposalActionType.CalculateDisposalDate:
                                bool onHoldPhysicalRecord = IsRecordsHold(file, mRunJobTime.Ticks);
                                logger.Info($"The folder is hold: {onHoldPhysicalRecord}, continue calcualate disposal date for folder");
                                disposalAction.CalculateDisposalDateForFolder(file, engine, fileFilterObj, SendJobDetail);
                                break;
                        }
                    }
                    else
                    {
                        //No need process records. no records rule now.
                        logger.Info($"Process file skip {fileOriginFullPath}");
                    }
                }
                catch (Exception e)
                {
                    mJobHasException = true;
                    logger.Warn($"Disposal file failed {fileOriginFullPath} : {e.ToString()}");
                    ReportManager.SendJobDetail(new JMPhysicalDisposalJobDetails()
                    {
                        ObjectName = file.Name,
                        FullPath = fileOriginFullPath,
                        ActionType = GetI18NActionType(physicalAction),
                        DestinationPath = "",
                        ItemType = "RM_Common_ObjectLevel_PhysicalFile",
                        Status = JobDetailsStatus.Failed,
                        Comment = e.Message,
                        RuleName = physicalRule?.Name
                    });
                }
                finally
                {
                    ReportManager.Increase(1);
                }
            }
        }

        private bool FolderUnderContainer(IPhysicalFile folder)
        {
            if (folder.Ancestors != null)
            {
                if (folder.ParentId == folder.LocationId || folder.Ancestors[1] == folder.BoxId)
                {
                    //folder under location or location/box
                    return false;
                }
                else
                {
                    return true;
                }
            }
            return false;
        }
        public void ProcessRecord(IPhysicalRecord record)
        {
            //Do action with rules.
        }

        private string GetI18NActionType(PhysicalDisposalActionType action)
        {
            string result = string.Empty;
            switch (action)
            {
                case PhysicalDisposalActionType.Pending:
                    result = "RM_JS_JM_DataOperation_PendingDataFromPhysical";
                    break;
                case PhysicalDisposalActionType.Disposal:
                    result = "RM_JMD_PD_DisposalAction_Dispose";
                    break;
                case PhysicalDisposalActionType.Move:
                    result = "RM_JMD_PD_DisposalAction_Move";
                    break;
                case PhysicalDisposalActionType.Waitingapproval:
                    result = "RM_DAM_ManualApproval_WaitingApproveStatus";
                    break;
                case PhysicalDisposalActionType.CalculateDisposalDate:
                    result = "RM_JS_RDM_CreateRule_Options_CalculateDisposalDate";
                    break;
                default:
                    result = action.ToString();
                    break;
            }
            return result;
        }


        /// <summary>
        ///  
        ///  //TO do consider add base obj of IPhysical if necessary ylgu
        /// 
        /// </summary>
        /// <param name="physicalRule"></param>
        /// <param name="originalRuleId"></param>
        /// <param name="originalStatus"></param>
        /// <returns></returns>
        async Task<PhysicalDisposalActionType> GetDisposalActionAsync(Rule physicalRule, string originalRuleId, int originalStatus, Guid id,long manualExtendTime,long manualModifiedTimeTicks)
        {
            if (physicalRule!= null && physicalRule.PhysicalRule.IsCalculationDisposalDate)
            {
                logger.Info("PhysicalRule is CalculationDisposalDate");
                return PhysicalDisposalActionType.CalculateDisposalDate;
            }
            if (physicalRule != null && physicalRule.PhysicalRule.IsManualApproval)
            {
                var isProcessByOwners = string.IsNullOrEmpty(physicalRule.WorkflowId);
                if (physicalRule.Id != null && physicalRule.Id.Equals(originalRuleId.ToString()))
                {
                    if (originalStatus == (int)SOApproveDBStatus.Approved)
                    {
                        logger.Info("PhysicalRule IsManualApproval and originalStatus is Approved.PhysicalDisposalActionType is Disposal.");
                        //disposalAction.DisposalBox(box);
                        return PhysicalDisposalActionType.Disposal;
                    }
                    else if (Wrapper.Common.WrapperConfiguration.IsProcessApprovalDatasOnly)
                    {
                        logger.Info("PhysicalRule IsManualApproval and originalStatus is IsProcessApprovalDatasOnly.PhysicalDisposalActionType is None.");
                        return PhysicalDisposalActionType.IsProcessApprovalDatasOnly;
                    }
                    else if (originalStatus == (int)SOApproveDBStatus.WaitingApprove/* || originalStatus == (int)SOApproveDBStatus.Rejected*/)
                    {
                        //disposalAction.PendingBox(box);
                        logger.Info("PhysicalRule IsManualApproval and originalStatus is WaitingApprove.PhysicalDisposalActionType is None.");
                        return PhysicalDisposalActionType.Waitingapproval;
                    }
                    else if (originalStatus == (int)SOApproveDBStatus.Rejected)
                    {

                        logger.Info("PhysicalRule IsManualApproval and originalStatus is Rejected.PhysicalDisposalActionType is None.");
                        var rec = GetRecord(id);
                        if (manualExtendTime >= DateTime.UtcNow.Ticks)
                        {
                            logger.Info($"Item: match manual rule, but is extend time data.");
                            return PhysicalDisposalActionType.Waitingapproval;
                        }
                        rec.ManualApprovedStatus = (int)SOApproveDBStatus.Rejected;
                        rec.ManualModifiedTime = manualModifiedTimeTicks;
                        mManualAction.ProcessApprovedOrRejectedRecord(rec);

                        var newRec = await mManualAction.ProcessWaitingForApprovalRecordAsync(rec);
                        if (isProcessByOwners && newRec.ManualReviewer.Length == 0)
                        {
                            throw new Exception("RM_MA_NoRecordOwner");
                        }
                        //CosmosDBManualDataUpdater.Add(newRec);
                        ExplorerDao.Upsert(newRec);
                        return PhysicalDisposalActionType.Pending;
                    }
                    else
                    {
                        //TODO Which case can enter into this part of the logic
                        //In dao not have Reject case
                        logger.Info("PhysicalRule IsManualApproval and originalStatus is originalStatus.PhysicalDisposalActionType is Pending.");
                        var rec = GetRecord(id);
                        rec.RuleId = new Guid(physicalRule.Id);
                        rec.DisposalStatus = (int)SOApproveDBStatus.WaitingApprove;
                        rec.ExportToRECO = false;
                        rec.DestroyedTime = 0;
                        if (rec.NodeType == (int)RMNodeLevel.PhysicalFile && physicalRule.PhysicalRule.IsManualApproval && physicalRule.PhysicalRule.RelatedRecordOption == RelatedRecordOption.Both)
                        {
                            rec.DeleteRelatedRecords = (int)RelatedRecordOption.Both;
                        }
                        rec.ManualFullPath = GetManualFullPath(rec);
                        rec.ManualModifiedTime = manualModifiedTimeTicks;
                        var newRec = await mManualAction.ProcessWaitingForApprovalRecordAsync(rec);
                        if (isProcessByOwners && newRec.ManualReviewer.Length == 0)
                        {
                            throw new Exception("RM_MA_NoRecordOwner");
                        }
                        //CosmosDBManualDataUpdater.Add(newRec);
                        ExplorerDao.Upsert(newRec);
                        return PhysicalDisposalActionType.Pending;
                    }
                }
                else
                {
                    if (Wrapper.Common.WrapperConfiguration.IsProcessApprovalDatasOnly)
                    {
                        logger.Info("PhysicalRule IsManualApproval and originalStatus is IsProcessApprovalDatasOnly.PhysicalDisposalActionType is None.");
                        return PhysicalDisposalActionType.IsProcessApprovalDatasOnly;
                    }
                    else
                    {
                        logger.Info("PhysicalRule IsManualApproval and physicalRule is different.PhysicalDisposalActionType is Pending.");
                        var rec = GetRecord(id);
                        rec.RuleId = new Guid(physicalRule.Id);
                        rec.DisposalStatus = (int)SOApproveDBStatus.WaitingApprove;
                        rec.ExportToRECO = false;
                        rec.DestroyedTime = 0;
                        if (rec.NodeType == (int)RMNodeLevel.PhysicalFile && physicalRule.PhysicalRule.IsManualApproval && physicalRule.PhysicalRule.RelatedRecordOption == RelatedRecordOption.Both)
                        {
                            rec.DeleteRelatedRecords = (int)RelatedRecordOption.Both;
                        }
                        rec.ManualFullPath = GetManualFullPath(rec);
                        rec.ManualModifiedTime = manualModifiedTimeTicks;
                        var newRec = await mManualAction.ProcessWaitingForApprovalRecordAsync(rec);
                        if (isProcessByOwners && newRec.ManualReviewer.Length == 0)
                        {
                            throw new Exception("RM_MA_NoRecordOwner");
                        }
                        newRec.ManualExtendCount = 0;
                        //CosmosDBManualDataUpdater.Add(newRec);
                        ExplorerDao.Upsert(newRec);
                        return PhysicalDisposalActionType.Pending;//if ruleid is not the same switch the rule.
                    }
                }
            }
            else if (physicalRule != null && physicalRule.PhysicalRule.spMoveOption != null && physicalRule.PhysicalRule.spMoveOption.MoveDestination != null)//TO do how to judge move option.
            {
                if (Wrapper.Common.WrapperConfiguration.IsProcessApprovalDatasOnly)
                {
                    logger.Info("PhysicalRule is MoveDestination.PhysicalDisposalActionType is Move.and IsProcessApprovalDatasOnly");
                    return PhysicalDisposalActionType.IsProcessApprovalDatasOnly;
                }
                else
                {
                    logger.Info("PhysicalRule is MoveDestination.PhysicalDisposalActionType is Move.");
                    //Not Manual Case
                    var rec = GetRecord(id);
                    rec.RemoveManualFields();
                    //CosmosDBManualDataUpdater.Add(rec);
                    ExplorerDao.Upsert(rec);
                    return PhysicalDisposalActionType.Move;
                }
            }
            else if (physicalRule != null && !physicalRule.PhysicalRule.IsManualApproval)
            {
                if (Wrapper.Common.WrapperConfiguration.IsProcessApprovalDatasOnly)
                {
                    logger.Info("PhysicalRule is MoveDestination.PhysicalDisposalActionType is Not Manual.and IsProcessApprovalDatasOnly");
                    return PhysicalDisposalActionType.IsProcessApprovalDatasOnly;
                }
                else
                {
                    logger.Info("PhysicalRule is MoveDestination.PhysicalDisposalActionType is Not Manual.");
                    //Not Manual Case
                    var rec = GetRecord(id);
                    rec.RemoveManualFields();
                    //CosmosDBManualDataUpdater.Add(rec);
                    ExplorerDao.Upsert(rec);
                    return PhysicalDisposalActionType.Disposal;
                }
            }
            else if (physicalRule != null)
            {
                if (Wrapper.Common.WrapperConfiguration.IsProcessApprovalDatasOnly)
                {
                    logger.Info("PhysicalRule is Disposal.PhysicalDisposalActionType is Disposal.and IsProcessApprovalDatasOnly");
                    return PhysicalDisposalActionType.IsProcessApprovalDatasOnly;
                }
                else
                {
                    logger.Info("PhysicalRule is Disposal.PhysicalDisposalActionType is Disposal.");
                    return PhysicalDisposalActionType.Disposal;
                }
 
            }
            else
            {
                if (Wrapper.Common.WrapperConfiguration.IsProcessApprovalDatasOnly)
                {
                    if (originalStatus == (int)SOApproveDBStatus.Approved)
                    {
                        logger.Info("PhysicalRule IsManualApproval and originalStatus is IsProcessApprovalDatasOnly.PhysicalDisposalActionType is IsProcessApprovalDatasOnly.");
                        var rec = GetRecord(id);
                        rec.RemoveManualFields(false);
                        ExplorerDao.Upsert(rec);
                    }
                    return PhysicalDisposalActionType.None;
                }
                else
                {
                    var rec = GetRecord(id);
                    rec.RemoveManualFields(false);
                    //CosmosDBManualDataUpdater.Add(rec);
                    ExplorerDao.Upsert(rec);
                    return PhysicalDisposalActionType.None;
                }
            }
        }

        private string GetManualFullPath(Record rec)
        {
            string path = string.Empty;
            if (rec.NodeType == (int)RMNodeLevel.PhysicalFile)
            {
                var folder = new PhysicalFile(rec);
                path = folder.DirPath;
            }
            else if (rec.NodeType == (int)RMNodeLevel.PhysicalBox)
            {
                var box = new PhysicalBox(rec);
                path = box.DirPath;
            }
            return ReplaceUrlWith18NValue(path);
        }

        private string ReplaceUrlWith18NValue(string url)
        {
            if (!string.IsNullOrWhiteSpace(url) && url.StartsWith("RM_SPS_Location_RootNode"))
            {
                return url.Replace("RM_SPS_Location_RootNode", I18NEntity.GetString("RM_SPS_Location_RootNode"));
            }
            return url;
        }
    }
    public enum PhysicalDisposalActionType
    {
        EmptyRuleInfo = 0,
        Pending = 1,
        Disposal = 2,
        Move = 3,
        None = 4,
        Waitingapproval = 5,
        IsProcessApprovalDatasOnly = 6,
        CalculateDisposalDate = 7,
    }
}
