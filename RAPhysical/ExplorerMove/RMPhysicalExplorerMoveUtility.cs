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
using AvePoint.GCommon.Contract.StorageOptimization.Object;
using AvePoint.GCommon.Contract.Tree.Object;
using AvePoint.GCommon.Utility;
using AvePoint.RA.Common;
using AvePoint.RA.Common.Report;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.Contract.Object.JobMessage;
using AvePoint.RA.Contract.Object.RealTime;
using AvePoint.RA.Contract.RMRelatedRecord;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RMWeb.Audit;
using AvePoint.RA.Contract.RMWeb.CP;
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using AvePoint.RA.Contract.RMWeb.Physical;
using AvePoint.RA.Contract.RMWeb.Physical.ColumnValues;
using AvePoint.RA.Contract.RMWeb.TemplateManagement;
using AvePoint.RA.Contract.RMWeb.Tree.Base;
using AvePoint.RA.Contract.Schedule;
using AvePoint.RA.Contract.TemplateManagement;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.DB.Core;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Explorer.Model;
using AvePoint.RA.DB.Model;
using AvePoint.RA.I18N.Core;
using AvePoint.RA.RACommonUtility;
using AvePoint.RA.RAPhysical.API;
using AvePoint.RA.RAPhysical.Common;
using AvePoint.RA.RAPhysical.Discover;
using AvePoint.RA.RAPhysical.Discover.DiscoverImps;
using AvePoint.RA.RAPhysical.Disposal;
using AvePoint.RA.RAPhysical.Disposal.PhysicalDisposalActionImps;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.RAPhysical.ExplorerMove
{
    public class RMPhysicalExplorerMoveUtility
    {
        private static readonly AvePoint.RA.Contract.Services.IRALogger logger = RALogger.GetInstance(typeof(RMPhysicalExplorerMoveUtility));
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
        private IRecordAllianceDao mIRecordAllianceDao;
        protected IRecordAllianceDao RecordAllianceDao
        {
            get
            {
                if (mIRecordAllianceDao == null)
                {
                    mIRecordAllianceDao = (IRecordAllianceDao)PlatformWindsorManager.GetService(typeof(IRecordAllianceDao));
                }
                return mIRecordAllianceDao;
            }
        }
        private IRecordLoanAllianceDao mRecordLoanAllianceDao;
        protected IRecordLoanAllianceDao RecordLoanAllianceDao
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
        private IRMRecordsUpdateTempDao mRMRecordsUpdateTempDao;
        public IRMRecordsUpdateTempDao RMRecordsUpdateTempDao
        {
            get
            {
                if (mRMRecordsUpdateTempDao == null)
                {
                    mRMRecordsUpdateTempDao = (IRMRecordsUpdateTempDao)PlatformWindsorManager.GetService(typeof(IRMRecordsUpdateTempDao)); ;
                }
                return mRMRecordsUpdateTempDao;
            }
        }
        private IRMLocationSuiteAssociationDao mRMLocationSuiteAssociationDao;
        public IRMLocationSuiteAssociationDao RMLocationSuiteAssociationDao
        {
            get
            {
                if (mRMLocationSuiteAssociationDao == null)
                {
                    mRMLocationSuiteAssociationDao = (IRMLocationSuiteAssociationDao)PlatformWindsorManager.GetService(typeof(IRMLocationSuiteAssociationDao)); ;
                }
                return mRMLocationSuiteAssociationDao;
            }
        }

        private IRMTemplateRelationshipDao mIRMTemplateRelationshipDao;
        public IRMTemplateRelationshipDao RMTemplateRelationshipDao
        {
            get
            {
                if (mIRMTemplateRelationshipDao == null)
                {
                    mIRMTemplateRelationshipDao = (IRMTemplateRelationshipDao)PlatformWindsorManager.GetService(typeof(IRMTemplateRelationshipDao)); ;
                }
                return mIRMTemplateRelationshipDao;
            }
        }
        private ITemplateManagementService mTemplateManagementService;
        public ITemplateManagementService TemplateManagementService
        {
            get
            {
                if (mTemplateManagementService == null)
                {
                    mTemplateManagementService = (ITemplateManagementService)PlatformWindsorManager.GetService(typeof(ITemplateManagementService)); ;
                }
                return mTemplateManagementService;
            }
        }

        

        private IRMTemplateDao rRMTemplateDao;
        public IRMTemplateDao RMTemplateDao
        {
            get
            {
                if (rRMTemplateDao == null)
                {
                    rRMTemplateDao = (IRMTemplateDao)PlatformWindsorManager.GetService(typeof(IRMTemplateDao)); ;
                }
                return rRMTemplateDao;
            }
        }
        private IRMSuiteDao rMSuiteDao;
        public IRMSuiteDao RMSuiteDao
        {
            get
            {
                if (rMSuiteDao == null)
                {
                    rMSuiteDao = (IRMSuiteDao)PlatformWindsorManager.GetService(typeof(IRMSuiteDao)); ;
                }
                return rMSuiteDao;
            }
        }
        private IRMScopePermissionDao rScopePermissionDao;
        public IRMScopePermissionDao RMScopePermissionDao
        {
            get
            {
                if (rScopePermissionDao == null)
                {
                    rScopePermissionDao = (IRMScopePermissionDao)PlatformWindsorManager.GetService(typeof(IRMScopePermissionDao)); ;
                }
                return rScopePermissionDao;
            }
        }

        private IRecordsHistoryService RecordsHistoryService => PlatformWindsorManager.GetService<IRecordsHistoryService>();
        private IExplorerService _explorerService => PlatformWindsorManager.GetService<IExplorerService>();
        private IPhysicalReqeustService _physicalRequestService => PlatformWindsorManager.GetService<IPhysicalReqeustService>();

        //public IPermissionManagementService PermissionManagementService { get; set; }
        protected IPhysicalDiscover PhysicalDiscover = new PhysicalFullDiscover();
        private IPhysicalLocation DestinationLocation { get; set; }
        private Guid DestinationBoxId { get; set; }
        protected Guid DestinationLocationId { get; set; }
        protected Guid DestinationFolderId { get; set; }
        protected string DestinationLocationPath { get; set; }
        private RMScopePermission DestinationLocationScopePermission { get; set; }
        private RMScopePermission DestinationBoxScopePermission { get; set; }
        private Contract.Object.RealTime.NameConflictOption ConflictOption { get; set; }
        private Contract.Object.RealTime.PhysicalMoveHoldConflictOption HoldConflictOption { get; set; }

        private bool mIsDisposalJob = false;
        #region move request
        private bool _isMoveRequestApprovalJob = false;
        private bool _fromMoveRequestModule;
        private bool _isSendEmailDestiontionRM;
        private Guid _groupRequestId;
        private int _failedItemCountInMoveRequest = 0;
        private int _successfulItemCountInMoveRequest = 0;
        private HashSet<string> _originalLocations = new();
        private PhysicalRequestDto _physicalRequestsNeedProcess = new();
        #endregion

        private JobRunBy mJobRunBy { get; set; }
        public bool HasFailedNode { get; private set; }
        public bool HasSuccessNode { get; private set; }
        public List<string> processRecordNames = new List<string>();
        public List<string> failedRecordNames = new List<string>();
        public List<string> skipedRecordNames = new List<string>();
        public List<Guid> failedRecordIds = new List<Guid>();

        private Dictionary<Guid, Record> boxCache = new Dictionary<Guid, Record>();
        private Dictionary<Guid, Record> folderCache = new Dictionary<Guid, Record>();

        /// <summary>
        /// 待处理 hold physicalFile数据
        /// </summary>
        List<IPhysicalFields> precessHoldPhysicalFieldList = new List<IPhysicalFields>();
        /// <summary>
        /// hold Record数据
        /// </summary>
        List<Record> holdRecordList = new List<Record>();
        public RMPhysicalExplorerMoveUtility(bool isDisposalJob = false, JobRunBy jobRunBy = JobRunBy.Control,bool isMoveRequestApprovalJob = false) 
        {
            mIsDisposalJob = isDisposalJob;
            _isMoveRequestApprovalJob = isMoveRequestApprovalJob;
            if (isDisposalJob)
            {
                mJobRunBy = jobRunBy;
            }
        }
       
        public async Task MoveAsync(PhysicalMoveOption moveDto, string tempJobId, bool isRealTimeMove = false, Guid groupRequestId = default)
        {
            try
            {
                _groupRequestId = groupRequestId;
                logger.Info("Physical move action start {0}", tempJobId);
                RMRecordsUpdateTempDao.InsertUpdateTemp(tempJobId, "", RecordsConstants.Explorer_RealTime_Running);
                InitDestinationSetting(moveDto);
                var sourceRecords = new List<Record>();
                using (new PerformanceScope(string.Format("move.physical.GetRecords")))
                {
                    sourceRecords = ExplorerDao.QueryAll(r => moveDto.SourcePhyRecordIds.Contains(r.Id)).ToList();
                }
                foreach (var r in sourceRecords)
                {
                    if (r.NodeType == (int)RMNodeLevel.PhysicalBox)
                    {
                        if (IfBoxUnderContainer(r))
                        {
                            await AddToMovePickAsync(new PhysicalBox(r), PickMoveStatusType.Fail, RMNodeType.PhyBox, "RM_PRM_Disposal_SkipBoxUnderContainer");
                            logger.Info($"Box is under custom container, will not move. Id:{r.NodeId}");
                            continue;
                        }
                        await ProcessBoxAsync(new PhysicalBox(r));
                    }
                    else if (r.NodeType == (int)RMNodeLevel.PhysicalFile)
                    {
                        if (IsFolderUnderContainer(r))
                        {
                            await AddToMovePickAsync(new PhysicalFile(r), PickMoveStatusType.Fail, RMNodeType.PhyFile, "RM_PRM_Disposal_SkipFolderUnderContainer");
                            logger.Info($"Folder is under custom container, will not move. Id:{r.NodeId}");
                            continue;
                        }
                        var fileLocation = new PhysicalLocation(r.LocationId);
                        var fileBox = new PhysicalBox(r.BoxId);
                        var file = new PhysicalFile(fileLocation, fileBox, r);
                        await ProcessFileAsync(file, DestinationBoxId, isRealTimeMove: isRealTimeMove);
                    }
                    else if (r.NodeType == (int)RMNodeLevel.PhysicalRecord)
                    {
                        if (r.LocationId != DestinationLocationId)
                        {
                            skipedRecordNames.Add(r.LeafName);
                            failedRecordIds.Add(r.Id);
                            continue;
                        }
                        
                        var fileRecord = ExplorerDao.GetRecordByIds(new List<Guid> { DestinationFolderId });
                        var fileLocation = new PhysicalLocation(r.LocationId);
                        var fileBox = new PhysicalBox(r.BoxId);
                        var file = new PhysicalFile(fileLocation, fileBox, fileRecord.FirstOrDefault());
                        var record = new PhysicalRecord(fileLocation, fileBox,r);
                        await ProcessRecordAsync(file, record, DestinationBoxId, DestinationFolderId);
                    }
                }
                UpdateBatchUseLongestHold();
                if (_isMoveRequestApprovalJob) return;
                if (processRecordNames.Count > 0)
                {
                    RMRecordsUpdateTempDao.InsertUpdateTemp(tempJobId, "", RecordsConstants.Explorer_RealTime_Running, JsonConvert.SerializeObject(processRecordNames));
                }
                if (skipedRecordNames.Count > 0)
                {
                    var failedPaths = string.Join(";", skipedRecordNames.ToArray());
                    RMRecordsUpdateTempDao.InsertUpdateTemp(tempJobId, failedPaths, RecordsConstants.Explorer_RealTime_Failed_Partial);
                    throw new Exception(string.Format(I18NEntity.GetString("Failed record path:{0}"), skipedRecordNames));
                }
                if (failedRecordNames.Count > 0)
                {
                    var failedPaths = string.Join(";", failedRecordNames.ToArray());
                    RMRecordsUpdateTempDao.InsertUpdateTemp(tempJobId, failedPaths, RecordsConstants.Explorer_RealTime_Failed_Partial);
                    throw new Exception(string.Format(I18NEntity.GetString("Failed record path:{0}"), failedPaths));
                }
                else
                {
                    RMRecordsUpdateTempDao.InsertUpdateTemp(tempJobId, "", RecordsConstants.Explorer_RealTime_Finished);
                }

            }
            catch (Exception ex)
            {
                logger.Error("Physical move error:{0}", ex.ToString());
                throw ex;
            }
            finally
            {
                if (_fromMoveRequestModule)
                {
                    try
                    {
                        logger.Info("Send email for move request");
                        if (_physicalRequestsNeedProcess != null)
                        {
                            var param = new ParameterMoveDto
                            {
                                SuccessfullCount = _successfulItemCountInMoveRequest,
                                FailedCount = _failedItemCountInMoveRequest,
                                OriginalLocation = string.Join(",", _originalLocations),
                                DestinationLocation = DestinationLocationPath,
                            };
                            await _physicalRequestService.SendEmailNotificationAsync(EmailTemplateInternalType.MoveRequestApprovedToEndUser, _physicalRequestsNeedProcess, param);
                            if (_isSendEmailDestiontionRM)
                                await _physicalRequestService.SendEmailNotificationAsync(EmailTemplateInternalType.MoveRequestApprovedToDestinationRM, _physicalRequestsNeedProcess, moveParam: param);

                        }
                    }
                    catch (Exception ex)
                    {
                        logger.Error("Send email for move request error:{0}", ex.ToString());

                    }
                }
            }
        }

        private bool IfBoxUnderContainer(Record record)
        {
            if (record.Ancestors != null && record.ParentId != record.LocationId)
            {
                return true;
            }
            return false;
        }

        private bool IsFolderUnderContainer(Record record)
        {
            if (record.Ancestors != null)
            {
                if (record.ParentId == record.LocationId || record.Ancestors[1] == record.BoxId)
                {
                    //under location or location/box
                    return false;
                }
                else
                {
                    //under container
                    return true;
                }
            }
            return false;
        }

        public bool CheckMoveHasHoldConflict(PhysicalMoveOption moveDto)
        {
            logger.Info("Physical move CheckMoveHasHoldConflict.");
            InitDestinationSetting(moveDto);
            var sourceRecords = ExplorerDao.QueryAll(r => moveDto.SourcePhyRecordIds.Contains(r.Id)).ToList();
            foreach (var r in sourceRecords)
            {
                if (r.NodeType == (int)RMNodeLevel.PhysicalBox || r.NodeType == (int)RMNodeLevel.PhysicalRecord)
                {

                }
                else if (r.NodeType == (int)RMNodeLevel.PhysicalFile)
                {
                    var fileLocation = new PhysicalLocation(r.LocationId);
                    var fileBox = new PhysicalBox(r.BoxId);
                    var file = new PhysicalFile(fileLocation, fileBox, r);
                    if (!IsAllowMove(file))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public async Task ProcessBoxAsync(IPhysicalBox box)
        {
            var boxFullPath = box.DirPath;
            var boxName = box.Name;
            logger.Info($"Process box begin {boxFullPath}");
            try
            {
                processRecordNames.Add(boxName);
                await MoveBoxAsync(box);
            }
            catch (Exception e)
            {
                failedRecordNames.Add(boxName);
                await AddToMovePickAsync(box, PickMoveStatusType.Fail, RMNodeType.PhyBox, e.Message);
                logger.Warn($"Process box failed {boxFullPath} : {e.ToString()}");
            }
        }

        private void checkBox(IPhysicalBox box) {
            ValidatePhysicalRequest(box.Id, box.Ancestors);
            if (box.LocationId == DestinationLocationId) {
                throw new Exception("RM_JS_BCM_Explorer_Move_ToSameLocation");
            }
        }
        private void checkFie(IPhysicalFile file, Guid boxId)
        {
            ValidatePhysicalRequest(file.Id, file.Ancestors);
            if (file.LocationId == DestinationLocationId && file.BoxId == boxId)
            {
                throw new Exception("RM_JS_BCM_Explorer_Move_ToSameLocation");
            }
            if (IsLoanFile(file))
            {
                throw new Exception(I18NEntity.GetString("RM_BCM_PDM_LoanFolderMoveError"));
            }
            //if (!IsAllowMove(file)) {
            //    throw new Exception(I18NEntity.GetString("RM_BCM_PDM_HasDeffrentHoldTime"));
            //}

            //TODO RECO-5182
        }
        private void CheckRecord(IPhysicalRecord record, Guid folderId)
        {   
            ValidatePhysicalRequest(record.Id, record.Ancestors);
            if (record.LocationId != DestinationLocationId)
            {
                throw new Exception("RM_JS_BCM_Explorer_Move_ToDifferentLocation");
            }
            if (record.ParentId == folderId)
            {
                throw new Exception("RM_JS_BCM_Explorer_Move_ToSameFolder");
            }
        }
        private async Task AddToMovePickAsync(dynamic record, PickMoveStatusType status, RMNodeType nodeType, string comment = "")
        {
            if (!_fromMoveRequestModule) return;

            var homeLocation = _explorerService.GetPhysicalObjectFullPath(record.Id);
            var movePick = await RecordsHistoryService.BuildPhysicalMoveDataAsync(record, (int)status, comment, DestinationLocationPath, DestinationLocationId, homeLocation: homeLocation);

            RecordsHistoryService.AddMoveData([movePick]);

            if (status == PickMoveStatusType.Successfull)
                _successfulItemCountInMoveRequest++;
            else
                _failedItemCountInMoveRequest++;

            if (!string.IsNullOrEmpty(homeLocation))
                _originalLocations.Add(homeLocation);

            if (_isMoveRequestApprovalJob)
                SendJobDetail(record, status == PickMoveStatusType.Successfull ? JobDetailsStatus.Successful : JobDetailsStatus.Failed, homeLocation, nodeType, comment);
        }
        public async Task ProcessFileAsync(IPhysicalFile file, Guid boxId, bool isRealTimeMove = false)
        {
            var fileFullPath = file.DirPath;
            var fileName = file.Name;
            logger.Info($"Process file begin {fileFullPath}");
            try
            {
                processRecordNames.Add(fileName);
                checkFie(file, boxId);//for catch exception
                string destinationPath = string.Empty;
                if(boxId != Guid.Empty)
                {
                    if (!boxCache.TryGetValue(boxId, out var box))
                    {
                        box = ExplorerDao.GetPhysicalRecordById(boxId);
                        _ = boxCache.TryAdd(boxId, box); 
                    }
                    destinationPath = DestinationLocation.DirPath + '/' + box?.LeafName;
                }
                await MoveFileAsync(file, boxId, destinationPath, isRealTimeMove: isRealTimeMove);
            }
            catch (Exception e)
            {
                failedRecordNames.Add(fileName);
                await AddToMovePickAsync(file, PickMoveStatusType.Fail, RMNodeType.PhyFile, e.Message);
                logger.Warn($"Process file failed {fileFullPath} : {e.ToString()}");
            }
        }

        public async Task ProcessRecordAsync(IPhysicalFile file, IPhysicalRecord record, Guid boxId, Guid folderId)
        {
            var fileFullPath = record.DirPath;
            var fileName = record.Name;
            logger.Info($"Process file begin {fileFullPath}");
            try
            {
                processRecordNames.Add(fileName);
                CheckRecord(record, folderId);
                string destinationPath = DestinationLocation.DirPath;
                
                if (boxId != Guid.Empty)
                {
                    if (!boxCache.TryGetValue(boxId, out var box))
                    {
                        box = ExplorerDao.GetPhysicalRecordById(boxId);
                        _ = boxCache.TryAdd(boxId, box);
                    }
                    destinationPath += '/' + box?.LeafName;
                }
                if (!folderCache.TryGetValue(folderId, out var folder))
                {
                    folder = ExplorerDao.GetPhysicalRecordById(folderId);
                    _ = folderCache.TryAdd(folderId, folder);
                }
                destinationPath += '/' + folder?.LeafName;
                await MoveRecordAsync(file, record, boxId, destinationPath);
            }
            catch (Exception e)
            {
                failedRecordNames.Add(fileName);
                failedRecordIds.Add(record.Id);
                await AddToMovePickAsync(record, PickMoveStatusType.Fail, RMNodeType.PhyRecord, e.Message);
                logger.Warn($"Process file failed {fileFullPath} : {e.ToString()}");
            }
        }


        public async Task MoveBoxAsync(IPhysicalBox box, string ruleName = "")
        {
            checkBox(box);
            var destinationPath = DestinationLocation.DirPath;
            var hasLoanFileInBox = HasLoanFile(box);
            var boxPath = box.DirPath;
            var boxId = box?.Id;
            string originName = box.Name;
            var moveBoxSkipComment = "RM_PRM_Disposal_SkipBox";
            JobDetailsStatus detailStatus = JobDetailsStatus.Successful;
            if (!IsLoanBox(box))
            {
                await InnerMoveBoxAsync(box, destinationPath);
                logger.Info($"Success Move box[{boxId}] to a new destination location: {DestinationLocationId}");
                if (hasLoanFileInBox)
                {
                    await AddToMovePickAsync(box, PickMoveStatusType.Fail, RMNodeType.PhyBox, "RM_BCM_PDM_BoxUnderLoanFolderMoveError");
                    detailStatus = JobDetailsStatus.Skipped;
                    moveBoxSkipComment = "RM_BCM_PDM_BoxUnderLoanFolderMoveError";
                }
            }
            else
            {
                await AddToMovePickAsync(box, PickMoveStatusType.Fail, RMNodeType.PhyBox, "RM_BCM_PDM_LoanBoxMoveError");
                detailStatus = JobDetailsStatus.Skipped;
                moveBoxSkipComment = "RM_BCM_PDM_LoanBoxMoveError";
                logger.Warn("Box is on loan, will not move. Box id:{0}", box.Id);
            }

            if (mIsDisposalJob)
            {
                //send Detail
                SendJobDetail(originName, boxPath, PhysicalDisposalActionType.Move, detailStatus == JobDetailsStatus.Skipped ? string.Empty : destinationPath, "RM_Common_ObjectLevel_PhysicalBox", detailStatus, detailStatus == JobDetailsStatus.Skipped ? moveBoxSkipComment : string.Empty, ruleName);
            }
        }

        private void SendJobDetail(string name, string originPath, PhysicalDisposalActionType action, string destinationPath, string ItemType, JobDetailsStatus status, string comment = "", string ruleName = "")
        {
            HasFailedNode |= status == JobDetailsStatus.Failed;
            HasSuccessNode |= status == JobDetailsStatus.Successful;

            ReportMangerFactory.Instance.ReportManager.SendJobDetail(new JMPhysicalDisposalJobDetails()
            {
                ObjectName = name,
                FullPath = originPath,
                ActionType = GetI18NActionType(action),
                RuleName = ruleName,
                DestinationPath = destinationPath,
                ItemType = ItemType,
                Status = status,
                Comment = comment
            });
        }
        private void SendJobDetail(dynamic data, JobDetailsStatus status, string originalLocation, RMNodeType nodeType, string comment = "")
        {
            HasFailedNode |= status == JobDetailsStatus.Failed;
            HasSuccessNode |= status == JobDetailsStatus.Successful;

            ReportMangerFactory.Instance.ReportManager.SendJobDetail(new JMPhysicalMoveJobDetails()
            {
                ObjectName = data.Name,
                UniqueId = data.RecordId,
                ItemType = ConvertNodeTypeToReportType(nodeType),
                Status = status,
                Comment = comment
            });
        }
        public string ConvertNodeTypeToReportType(RMNodeType nodeType)
        {
            string reportType = string.Empty;
            switch (nodeType)
            {
                case RMNodeType.PhyBox:
                    reportType = "RM_Common_ObjectLevel_PhysicalBox";
                    break;
                case RMNodeType.PhyFile:
                    reportType = "RM_Common_ObjectLevel_PhysicalFile";
                    break;
                case RMNodeType.PhyRecord:
                    reportType = "RM_Common_ObjectLevel_PhysicalRecord";
                    break;
                default:
                    break;
            }
            return reportType;
        }
        private string GetI18NActionType(PhysicalDisposalActionType action)
        {
            string result = string.Empty;
            switch (action)
            {
                case PhysicalDisposalActionType.Pending:
                    result = "RM_JMD_PD_DisposalAction_Pending";
                    break;
                case PhysicalDisposalActionType.Disposal:
                    result = "RM_JMD_PD_DisposalAction_Dispose";
                    break;
                case PhysicalDisposalActionType.Move:
                    result = "RM_JMD_PD_DisposalAction_Move";
                    break;
                default:
                    result = action.ToString();
                    break;
            }
            return result;
        }

        private bool IsLoanBox(IPhysicalBox box)
        {          
            var recordLoanAlliances = RecordLoanAllianceDao.GetPhyRecordAllianceByIds(new List<Guid>() { box.Id });
            if (recordLoanAlliances != null && recordLoanAlliances.Count > 0)
            {
                return true;
            }
            return false;           
        }
        public async Task MoveFileAsync(IPhysicalFile file, Guid boxId, string destinationPath, string ruleName = "", bool isRealTimeMove = false)
        {
            // checkFie(file, boxId);
            var filePath = file.DirPath;
            string originName = file.Name;
            var fileId = file?.Id;
            JobDetailsStatus detailStatus = JobDetailsStatus.Successful;
            if (IsLoanFile(file))
            {
                await AddToMovePickAsync(file, PickMoveStatusType.Fail, RMNodeType.PhyFile, "RM_BCM_PDM_LoanFolderMoveError");
                //add detail
                SendJobDetail(originName, filePath, PhysicalDisposalActionType.Move, "", "RM_Common_ObjectLevel_PhysicalFile", JobDetailsStatus.Skipped, "RM_BCM_PDM_LoanFolderMoveError", ruleName);
                return;
            }
            destinationPath = boxId == Guid.Empty ? DestinationLocation.DirPath : destinationPath;
            var result = await InnerMoveFileAsync(file, boxId, destinationPath, true, isRealTimeMove: isRealTimeMove);
            RecordsHistoryService.AddPhysicalAudit([result]);
            if (mIsDisposalJob)
            {
                //add detail 
                SendJobDetail(originName, filePath, PhysicalDisposalActionType.Move, destinationPath, "RM_Common_ObjectLevel_PhysicalFile", detailStatus, string.Empty, ruleName);
            }
        }

        public async Task MoveRecordAsync(IPhysicalFile file, IPhysicalRecord record, Guid boxId, string destinationPath, string ruleName = "")
        {
            var filePath = record.DirPath;
            string originName = record.Name;
            var fileId = record?.Id;
            if (IsLoanFile(file))
            {
               await AddToMovePickAsync(record, PickMoveStatusType.Fail, RMNodeType.PhyRecord, "RM_BCM_PDM_LoanFolderMoveError");
                //add detail
                SendJobDetail(originName, filePath, PhysicalDisposalActionType.Move, "", "RM_Common_ObjectLevel_PhysicalFile", JobDetailsStatus.Skipped, "RM_BCM_PDM_LoanFolderMoveError", ruleName);
                return;
            }
            var result = await InnerMoveRecordAsync(file, record, boxId, destinationPath, true);
            RecordsHistoryService.AddPhysicalAudit([result]);
        }

        public void InitDestinationSetting(PhysicalMoveOption moveDto)
        {
            DestinationLocation = new PhysicalLocation(new Guid(moveDto.LocationId));
            DestinationBoxId = !string.IsNullOrEmpty(moveDto.BoxId) ? new Guid(moveDto.BoxId) : Guid.Empty;
            DestinationLocationId = !string.IsNullOrEmpty(moveDto.LocationId) ? new Guid(moveDto.LocationId) : Guid.Empty;
            DestinationFolderId = !string.IsNullOrEmpty(moveDto.FolderId) ? new Guid(moveDto.FolderId) : Guid.Empty;
            _fromMoveRequestModule = moveDto.FromModule == (int)AuditCategory.PhysicalExplorerMoveRequest;
            if (_fromMoveRequestModule)
            {
                DestinationLocationPath = new PhysicalMoveBuilder(ExplorerDao).BuildDestinationPath(DestinationLocationId, DestinationBoxId, DestinationFolderId);
                _isSendEmailDestiontionRM = moveDto.IsSendEmailToDestinationRM;
                _physicalRequestsNeedProcess = _physicalRequestService.GetRequestDtoByGroupIdAndStatusAsync(_groupRequestId, PhysicalRequestStatus.Approved).GetAwaiter().GetResult();
            }
            ConflictOption = moveDto.NameConflictOption;
            HoldConflictOption = moveDto.HoldConflictOption;
            DestinationLocationScopePermission = RMScopePermissionDao.HasBreakInheritPermission(DestinationLocation.IntId.ToString());
            if (DestinationBoxId != Guid.Empty)
            {
                DestinationBoxScopePermission = RMScopePermissionDao.HasBreakInheritPermission(DestinationBoxId.ToString());
            }
            logger.Info($"Destination locationId is {moveDto.LocationId}, Destination boxId is {DestinationBoxId}, ConflictOption is {(int)ConflictOption}");
        }
        private void ValidatePhysicalRequest(Guid id, List<Guid> ancestorsCurrent)
        {
            if (!_fromMoveRequestModule) return;

            var ancestorsRequest = _physicalRequestsNeedProcess.PhysicalFileInfos.FirstOrDefault(x => x.Id == id)?.Ancestors ?? new List<Guid>(); ;
            bool isSame = ancestorsCurrent.Count == ancestorsRequest.Count && new HashSet<Guid>(ancestorsCurrent).SetEquals(ancestorsRequest);
            if (!isSame)
            {
                throw new Exception("RM_JS_Phy_MoveRequest_MovedToOtherLocation");
            }

        }
        private async Task InnerMoveBoxAsync(IPhysicalBox box, string destinationPath, string newName = "") {
            var auditList = new List<PhysicalRecordActionAudit>();
            var locationId = DestinationLocationId;
            var destinationName = destinationPath.Substring(destinationPath.LastIndexOf('/') + 1);
            var boxName = string.IsNullOrEmpty(newName) ? box.Name : newName;
            var filesInBox = box.Files.Where(f => f.RecordStatus != (int)RMRecordStatus.RMDeleted && f.RecordStatus != (int)RMRecordStatus.MoveOverwrite).ToList();
            ProcessBoxTemplate(locationId, box.TemplateId);
            int boxScopePermissionId = await ProcessBoxPermissionAsync(box.Id);
            var hasLoanFileInBox = HasLoanFile(box);
            var boxId = hasLoanFileInBox ? Guid.Empty : box.Id;
            await filesInBox.ForEachAsync(async f => {
                if (!IsLoanFile(f))
                {
                    await InnerMoveFileAsync(f, boxId, destinationPath + $"/{box.Name}", false);
                }
                else
                {
                    await AddToMovePickAsync(f, PickMoveStatusType.Fail, RMNodeType.PhyBox, "RM_BCM_PDM_LoanFolderMoveError");
                    failedRecordNames.Add(f.Name);
                    logger.Error($"Cannot move the folder because it is on loan. folder id:{f?.Id}");
                }
            });
            RecordsHistoryService.AddPhysicalAudit(auditList);
            if (hasLoanFileInBox) {
                throw new Exception("RM_BCM_PDM_BoxUnderLoanFolderMoveError");
            }
            box.LocationId = locationId;
            box.RuleId = Guid.Empty;
            box.DisposalStatus = (int)SOApproveDBStatus.None;
            box.DisposalDueDate = DueDateUtil.ConvertStringDueDate2Long("RM_JS_JM_EndTimePending");
            var locationField = new TaxonomyColumnValue() { Id = box.LocationId.ToString(), Name = destinationName };
            box[MetaInfo.HomelocationId] = JsonConvert.SerializeObject(locationField);
            box[MetaInfo.NameOrTitleId] = boxName;
            box.Name = boxName;
            //源端Box没有打破权限，需要根据目的端Location权限进行赋值ScopePermissionId
            //源端Box打破权限，则仍使用原有ScopePermissionId
            if (boxScopePermissionId == 0)
            {
                //目的端Location有权限，赋值目的端Location权限
                if (DestinationLocationScopePermission != null)
                {
                    box.ScopePermissionId = DestinationLocationScopePermission.Id;
                }
                //目的端Location没有有权限，Box赋值为0，避免源端Box所在的Location有打破继承影响
                else
                {
                    box.ScopePermissionId = 0;
                }
            }
            box.ParentId = DestinationLocationId;
            box.Ancestors = new List<Guid>() { DestinationLocationId };
            var parentPath = box.DirPath.Remove(box.DirPath.Length - (box.Name.Length + 1));
            var result = RecordsHistoryService.BuildPhysicalActionAuditForJob(box.Id, PhysicalActionType.Move, false, mJobRunBy, parentPath, destinationPath);
            RecordsHistoryService.AddPhysicalAudit([result]);
            await AddToMovePickAsync(box, PickMoveStatusType.Successfull, RMNodeType.PhyBox);
            box.Update(true, true);
        }

        /// <summary>
        /// Start Box只能作用于一个Suite，这种类型数据Move到目的端，直接检查目的端是否有同名suite ID即可，如果没有直接添加suite到目的端,如果有什么都不处理。
        /// </summary>
        private void ProcessBoxTemplate(Guid desLocationId, int currentBoxTemplateId)
        {
            //判断源端suite在目的端location下是否关联同一个suite,如果有直接move,没有add到目的端location再move.
            RMTemplate rMTemplate = RMTemplateDao.GetTemplateById(currentBoxTemplateId);
            var suiteUniqueId = RMTemplateRelationshipDao.GetSuiteUniqueId(rMTemplate.UniqueId);
            List<RMLocationSuiteAssociation> rmLocationSuiteAssociations = RMLocationSuiteAssociationDao.GetAllRMLocationSuiteAssociations().Where(x => x.LocationUniqueId == desLocationId && x.SuiteUniqueId == suiteUniqueId).ToList();
            if (rmLocationSuiteAssociations.Count > 0)
            {
                logger.Info("Destination location:{0} contains same suite:{1} and don't need add source suite to des.", desLocationId, rmLocationSuiteAssociations.FirstOrDefault()?.SuiteUniqueId);
            }
            else
            {
                var newObject = new RMLocationSuiteAssociation() { LocationUniqueId = desLocationId, SuiteUniqueId = suiteUniqueId };
                RMLocationSuiteAssociationDao.Create(newObject);
                logger.Info("Destination location:{0} does not contains same suite:{1} and need add source suite to des.", desLocationId, suiteUniqueId);
            }
        }

        private async Task<int> ProcessBoxPermissionAsync(Guid sourceBoxId)
        {
            //判断源端Box是否有打破继承的权限，如果有则Move到目的端.
            var rMScopePermission = RMScopePermissionDao.HasBreakInheritPermission(sourceBoxId.ToString());
            int boxScopePermissionId = 0;
            if (rMScopePermission != null)
            {
                boxScopePermissionId = rMScopePermission.Id;
                string updateSourceBoxPermissionParentScope = DestinationLocation.IntId.ToString();
                string desLocationScopePermissionPath = GetLocationScopePermissionPath(DestinationLocation) + sourceBoxId.ToString() + "/";
                rMScopePermission.ParentScope = updateSourceBoxPermissionParentScope;
                rMScopePermission.ScopePath = desLocationScopePermissionPath;
                await RMScopePermissionDao.UpdateAsync(rMScopePermission);
            }
            else
            {
                logger.Info($"Current box doesn't have break inherit permssion.BoxId:[{sourceBoxId}].");
            }
            return boxScopePermissionId;
        }

        private string GetLocationScopePermissionPath(IPhysicalLocation location)
        {
            string locationScopePermissionPath = string.Empty;
            if (location.CurrentLocationType != (int)RMNodeLevel.PhysicalRootLocation)
            {
                string parentLocationScopePermissionPath = GetLocationScopePermissionPath(location.ParentLocation);
                locationScopePermissionPath = parentLocationScopePermissionPath + location.IntId + "/";
            }
            else
            {
                locationScopePermissionPath = location.IntId + "/";
            }
            return locationScopePermissionPath;
        }

        /// <summary>
        /// 1.Start Folder只能作用于一个Suite，这种类型数据Move到目的端有两种情况：
        ///   ①Move到目的端Location下，直接检查目的端是否有同名suite ID即可，如果没有直接添加suite到目的端,如果有什么都不处理。
        ///   ②Move到目的端Box下，需要检查目的端Box下是否有当前Folder，如果没有添加Folder Template到目的端Box下，如果有什么都不处理。
        /// 2.对于非Start Folder的folder，即Box下Folder，这种类型数据Move到目的端，有两种情况：
        ///   ①Move Folder到目的端Box下：
        ///     检查目的端Box下是否有当前Folder Template，如果没有Add源端Folder Template到目的端.如果有什么都不处理.
        ///   ②Move Folder到目的端Location下：
        ///     检查目的端Location是否有Start Folder的Source Folder Template:
        ///       一.如果没有:
        ///            ①检查当前账号下所有的Template，查看是否有同样Template。
        ///               一.如果有，则add Template到目的端Box下.
        ///               二.如果没有则创建Suite+Start Folder Template.
        ///       二.如果有什么都不处理.
        /// </summary>
        private async Task ProcessFileTemplateAsync(Guid sourceBoxId, Guid desBoxId, Guid desLocationId, int sourceFileTemplateId)
        {
            //获取Object TemplateID对应的Unique ID
            RMTemplate sourceTemplate = RMTemplateDao.GetTemplateById(sourceFileTemplateId);
            if (sourceBoxId == Guid.Empty)
            {
                logger.Info("ProcessFileTemplate SourceBoxId is empty.");
                if (desBoxId == Guid.Empty)
                {
                    //location/folder->location/folder, 
                    logger.Info("ProcessFileTemplate DesBoxId is empty.");
                    //判断源端suite在目的端location下是否关联同一个suite,如果有直接move,没有add到目的端location再move.
                    var suiteUniqueId = RMTemplateRelationshipDao.GetSuiteUniqueId(sourceTemplate.UniqueId);
                    List<RMLocationSuiteAssociation> rmLocationSuiteAssociations = RMLocationSuiteAssociationDao.GetAllRMLocationSuiteAssociations().Where(x => x.LocationUniqueId == desLocationId && x.SuiteUniqueId == suiteUniqueId).ToList();                    
                    if (rmLocationSuiteAssociations.Count > 0)
                    {
                        logger.Info("Destination location:{0} contains same suite:{1} and don't need add source suite to des.", desLocationId, rmLocationSuiteAssociations.FirstOrDefault()?.SuiteUniqueId);
                    }
                    else
                    {
                        RMLocationSuiteAssociationDao.Create(new RMLocationSuiteAssociation() { LocationUniqueId = desLocationId, SuiteUniqueId = suiteUniqueId });
                        logger.Info("Destination location:{0} does not contains same suite:{1} and need add source suite to des.", desLocationId, suiteUniqueId);
                    }
                }
                else
                {
                    logger.Info("ProcessFileTemplate DesBoxId is not empty.");
                    //获取目的端Box下所有Folder Template，检查是否有源端Folder Template.
                    Record desBoxRecord = ExplorerDao.GetRecordByIds(new List<Guid>() { desBoxId }).First();
                    RMTemplate desBoxTemplate = RMTemplateDao.GetTemplateById(desBoxRecord.TemplateId);
                    var idPath = await TemplateManagementService.GetTemplateIdPathAsync(ConvertUtil.ConvertRMBaseRecordToPhysical(desBoxRecord));
                    if (RMTemplateRelationshipDao.Exists(idPath, sourceFileTemplateId))
                    {
                        logger.Info("Destination Box:{0} contains same folder template:{1} and don't need add source folder teamplate to des box.", desBoxId, sourceTemplate.UniqueId);
                    }
                    else
                    {
                        logger.Info("Destination Box:{0} doesn't contains same folder template:{1} and need add source folder teamplate to des box.", desBoxId, sourceTemplate.UniqueId);
                        var ancestorTemplateIds = idPath.Trim('/').Split('/').ToList();
                        RMTemplateDao.AddTemplateRelatonship(ancestorTemplateIds, sourceFileTemplateId);

                    }
                    await UpdatePushColumnToFoldAsync(sourceTemplate, desBoxId);
                }
            }
            else
            {
                logger.Info("ProcessFileTemplate SourceBoxId is not empty.");
                if (desBoxId == Guid.Empty)
                {
                    logger.Info("ProcessFileTemplate DesBoxId is empty.");
                    //获取目的端Location下所有Suite.
                    List<RMLocationSuiteAssociation> rmLocationSuiteAssociations = RMLocationSuiteAssociationDao.GetAllRMLocationSuiteAssociations().Where(x => x.LocationUniqueId == desLocationId).ToList();
                    //检查目的端Location Suite下是否有Start folder template.
                    bool hasRelationFolderTemplate = false;
                    foreach (RMLocationSuiteAssociation suiteAssociation in rmLocationSuiteAssociations)
                    {
                        var startTemplateUniqueId = RMTemplateRelationshipDao.GetStartTemplateUniqueId(suiteAssociation.SuiteUniqueId);
                        //if (rMSuiteMemberships.Count > 0)
                        if(startTemplateUniqueId == sourceTemplate.UniqueId)
                        {
                            hasRelationFolderTemplate = true;
                            logger.Info("Destination location:{0} contains same start folder template:{1} and don't need add source folder template to des.", desLocationId, sourceTemplate.UniqueId);
                            break;
                        }
                    }
                    //目的端suite没有关联源端folder template有两种情况：
                    //第一种有Template没关联到目的端Location
                    //第二种当前账号没有Template，需要创建Start Folder的Suite
                    if (!hasRelationFolderTemplate)
                    {

                        //if (rMSuiteMemberships.Count > 0)
                        if (RMTemplateRelationshipDao.UsedAsStartTemplate(sourceTemplate.UniqueId))
                        {
                            var suiteUniqueId = RMTemplateRelationshipDao.GetSuiteUniqueId(sourceTemplate.UniqueId);
                            logger.Info("Destination location:{0} does not contains start folder template but current tenant exist the same suite and add suite to des.", desLocationId);
                            RMLocationSuiteAssociationDao.Create(new RMLocationSuiteAssociation() { LocationUniqueId = desLocationId, SuiteUniqueId = suiteUniqueId });
                        }
                        else
                        {
                            logger.Info("Destination location:{0} does not contains start folder template and need create new suite and add SuiteAssociation.", desLocationId, sourceTemplate.UniqueId);
                            //RMSuite rMSuite = new RMSuite()
                            //{
                            //    UniqueId = Guid.NewGuid(),
                            //    Name = sourceTemplate.Name + "_Suite",
                            //    Description = string.Empty,
                            //    StartFromType = SuiteStartFromType.Folder,
                            //    Creater = 1,
                            //    CreatedOn = DateTime.UtcNow,
                            //    Modifier = 1,
                            //    LastModifiedOn = DateTime.UtcNow,
                            //    RootTemplateCreateType = SuiteRootTemplateCreateType.ExistingFolder,
                            //};
                            //RMSuiteDao.Create(rMSuite);

                            SuiteDto suiteDto = new SuiteDto()
                            {
                                UniqueId = Guid.NewGuid(),
                                Name = sourceTemplate.Name + "_Suite",
                                StartFromType = SuiteStartFromType.Folder,
                                RootTemplateCreateType = SuiteRootTemplateCreateType.ExistingFolder,
                                RootTemplateUniqueId = sourceTemplate.UniqueId,
                                RootTemplateName = sourceTemplate.Name
                            };
                            RMSuiteDao.CreateSuite(suiteDto);
                            RMLocationSuiteAssociationDao.Create(new RMLocationSuiteAssociation() { LocationUniqueId = desLocationId, SuiteUniqueId = suiteDto.UniqueId });                           
                            List<string> ancestorTemplateIdList = new List<string>();
                            ancestorTemplateIdList.Add(suiteDto.UniqueId.ToString());
                            RMTemplateDao.AddTemplateRelatonship(ancestorTemplateIdList, sourceFileTemplateId);
                        }
                    }
                }
                else
                {
                    logger.Info("ProcessFileTemplate DesBoxId is not empty.");
                    //获取目的端Box下所有Folder Template，检查是否有源端Folder Template.
                    Record desBoxRecord = ExplorerDao.GetRecordByIds(new List<Guid>() { desBoxId }).First();
                    RMTemplate desBoxTemplate = RMTemplateDao.GetTemplateById(desBoxRecord.TemplateId);
                    var idPath = await TemplateManagementService.GetTemplateIdPathAsync(ConvertUtil.ConvertRMBaseRecordToPhysical(desBoxRecord));
                    if (RMTemplateRelationshipDao.Exists(idPath,sourceFileTemplateId))
                    {
                        logger.Info("Destination Box:{0} contains same folder template:{1} and don't need add source folder teamplate to des box.", desBoxId, sourceTemplate.UniqueId);
                    }
                    else
                    {
                        logger.Info("Destination Box:{0} doesn't contains same folder template:{1} and need add source folder teamplate to des box.", desBoxId, sourceTemplate.UniqueId);
                        var ancestorTemplateIds = idPath.Trim('/').Split('/').ToList();
                        RMTemplateDao.AddTemplateRelatonship(ancestorTemplateIds, sourceFileTemplateId);
                    }
                    await UpdatePushColumnToFoldAsync(sourceTemplate, desBoxId);
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sourceFile">源端File对象</param>
        /// <param name="isFileLevelMove">判断是Box Level Move带Folder还是Folder Level Move</param>
        private async Task<int> ProcessFilePermissionAsync(IPhysicalFile sourceFile, bool isFileLevelMove)
        {
            int fileScopePermissionId = 0;
            //判断源端Box是否有打破继承的权限，如果有则Move到目的端.
            var rMScopePermission = RMScopePermissionDao.HasBreakInheritPermission(sourceFile.Id.ToString());
            if (rMScopePermission != null)
            {
                fileScopePermissionId = rMScopePermission.Id;
                string updateSourceBoxPermissionParentScope = string.Empty;
                string desLocationScopePermissionPath = string.Empty;
                if (isFileLevelMove)
                {
                    updateSourceBoxPermissionParentScope = DestinationBoxId == Guid.Empty ? DestinationLocation.IntId.ToString() : DestinationBoxId.ToString();
                    desLocationScopePermissionPath = GetLocationScopePermissionPath(DestinationLocation) + (DestinationBoxId == Guid.Empty ? "" : DestinationBoxId.ToString() + "/") + sourceFile.Id.ToString() + "/";
                }
                else
                {
                    //源端带着Box一起Move的，需要拼装box路径,Folder Parent Scope一定是源端Box
                    updateSourceBoxPermissionParentScope = sourceFile.ParentBox.Id.ToString();
                    desLocationScopePermissionPath = GetLocationScopePermissionPath(DestinationLocation) + sourceFile.ParentBox.Id.ToString() + "/" + sourceFile.Id.ToString() + "/";
                }
                rMScopePermission.ParentScope = updateSourceBoxPermissionParentScope;
                rMScopePermission.ScopePath = desLocationScopePermissionPath;
                await RMScopePermissionDao.UpdateAsync(rMScopePermission);
            }
            else
            {
                logger.Info($"Current folder doesn't have break inherit permssion.BoxId:[{sourceFile.Id.ToString()}].");
            }
            return fileScopePermissionId;
        }

        private async Task UpdatePushColumnToFoldAsync(RMTemplate template, Guid boxId)
        {
            bool needUpdate = false;
            Record box = ExplorerDao.GetRecordByIds(new List<Guid>() { boxId }).FirstOrDefault();
            if (box == null)
            {
                logger.Error("Can't find fold's parent box,box id is {0}", boxId.ToString());
                return;
            }
            RMTemplate boxTemplate = RMTemplateDao.GetTemplateById(box.TemplateId); 
            if (boxTemplate == null)
            {
                logger.Error("Can't find box's template ,template id is {0}", box.TemplateId.ToString());
                return;
            }
            TemplateColumnsSchema schema = SerializerHelper.DeserializeByDataContractSerializer<TemplateColumnsSchema>(boxTemplate.ColumnSchema);
            List<ColumnXmlSchema> columns = schema.Columns;
            for (int i = 0; i < columns.Count; i++)
            {
                var item = columns[i];
                if ((item.TemplateInheritSetting & (int)TemplateInheritSettingEnum.PushToChild) == (int)TemplateInheritSettingEnum.PushToChild)
                {
                    List<TemplateIdAndCategoryId> pushFoldTemplateCategoriesId = item.pushFoldTemplateCategoriesId;
                    if (pushFoldTemplateCategoriesId != null && pushFoldTemplateCategoriesId.Count > 0)
                    {
                        TemplateIdAndCategoryId templateCategoryId = pushFoldTemplateCategoriesId.Find(t => t.tempalteId == template.UniqueId.ToString());
                        if (templateCategoryId == null)
                        {
                            needUpdate = true;
                            List<RMTemplateCategory> categories = RMTemplateDao.LoadCategories(template.UniqueId);
                            schema.Columns[i].pushFoldTemplateCategoriesId.Add(new TemplateIdAndCategoryId() { tempalteId = template.UniqueId.ToString(), categoryId = categories.FirstOrDefault()?.UniqueId.ToString() });
                        }
                    }
                }
            }
            if (needUpdate)
            {
                boxTemplate.ColumnSchema = SerializerHelper.SerializeByDataContractSerializer(schema);
                await RMTemplateDao.UpdateAsync(boxTemplate);
            }
        }
        private async Task PushColumnToRecordAsync(RMTemplate template, Guid folderId)
        {
            bool needUpdate = false;
            Record folder = ExplorerDao.GetRecordByIds(new List<Guid>() { folderId }).FirstOrDefault();
            if (folder == null)
            {
                logger.Error("Can't find parent folder for record with Folder ID {0}", folderId.ToString());
                return;
            }
            RMTemplate folderTemplate = RMTemplateDao.GetTemplateById(folder.TemplateId);
            if (folderTemplate == null)
            {
                logger.Error("Can't find folder's template ,template id is {0}", folder.TemplateId.ToString());
                return;
            }
            TemplateColumnsSchema schema = SerializerHelper.DeserializeByDataContractSerializer<TemplateColumnsSchema>(folderTemplate.ColumnSchema);
            List<ColumnXmlSchema> columns = schema.Columns;
            for (int i = 0; i < columns.Count; i++)
            {
                var item = columns[i];
                if ((item.TemplateInheritSetting & (int)TemplateInheritSettingEnum.PushToChild) == (int)TemplateInheritSettingEnum.PushToChild)
                {
                    List<TemplateIdAndCategoryId> pusRecordTemplateCategoriesId = item.pushRecordTemplateCategoriesId;
                    if (pusRecordTemplateCategoriesId != null && pusRecordTemplateCategoriesId.Count > 0)
                    {
                        TemplateIdAndCategoryId templateCategoryId = pusRecordTemplateCategoriesId.Find(t => t.tempalteId == template.UniqueId.ToString());
                        if (templateCategoryId == null)
                        {
                            needUpdate = true;
                            List<RMTemplateCategory> categories = RMTemplateDao.LoadCategories(template.UniqueId);
                            schema.Columns[i].pushRecordTemplateCategoriesId.Add(new TemplateIdAndCategoryId() { tempalteId = template.UniqueId.ToString(), categoryId = categories.FirstOrDefault()?.UniqueId.ToString() });
                        }
                    }
                }
            }
            if (needUpdate)
            {
                folderTemplate.ColumnSchema = SerializerHelper.SerializeByDataContractSerializer(schema);
                await RMTemplateDao.UpdateAsync(folderTemplate);
            }
        }

        //Record一定在Folder下，Record Move到目的端，直接检查目的端Folder是否关联当前源端Record Template即可。
        public async Task ProcessRecordTemplateAsync(IPhysicalFile sourceFile, IPhysicalRecord sourceRecord, Guid desBoxId, bool isPartOfFolderMove = true)
        {
            //获取源端Record Template
            RMTemplate sourceRecordTemplate = RMTemplateDao.GetTemplateById(sourceRecord.TemplateId);
            //获取目的端Folder Template.Note：源端目的端Folder Template使用的是一个.
            RMTemplate desFileTemplate = RMTemplateDao.GetTemplateById(sourceFile.TemplateId);
            //目的端是Start Folder
            if (desBoxId == Guid.Empty)
            {
                //Start Folder 唯一，直接找到对应的Suite关联关系即可.
                //查看目的端suite下是否关联了源端Record模板.
                var suiteUniqueId = RMTemplateRelationshipDao.GetSuiteUniqueId(desFileTemplate.UniqueId);
                var idPath = suiteUniqueId.ToString() + "/" + sourceFile.TemplateId.ToString() + "/";
                    //TemplateManagementService.GetTemplateIdPath(ConvertUtil.ConvertRMBaseRecordToPhysical((sourceFile as PhysicalFile).Record));
                if (RMTemplateRelationshipDao.Exists(idPath, sourceRecord.TemplateId))
                {
                    logger.Info("Destination Folder:{0} contains same record template:{1} and don't need add source record template to des folder.", sourceFile.Id, sourceRecordTemplate.UniqueId);
                }
                else
                {
                    logger.Info("Destination Folder:{0} doesn't contains same record template:{1} and need add source folder template to des folder.", sourceFile.Id, sourceRecordTemplate.UniqueId);
                    var ancestorTemplateIds = idPath.Trim('/').Split('/').ToList();
                    RMTemplateDao.AddTemplateRelatonship(ancestorTemplateIds, sourceRecord.TemplateId);
                    if (!isPartOfFolderMove)
                    {
                        await PushColumnToRecordAsync(sourceRecordTemplate, sourceFile.Id);
                    }
                }
            }
            //目的端是Box
            else
            {
                //获取目的端Box和Box Template
                Record desBoxRecord = ExplorerDao.GetRecordByIds(new List<Guid>() { desBoxId }).First();
                RMTemplate desBoxTemplate = RMTemplateDao.GetTemplateById(desBoxRecord.TemplateId);
                var idPath = await TemplateManagementService.GetTemplateIdPathAsync(ConvertUtil.ConvertRMBaseRecordToPhysical(desBoxRecord));
                idPath = idPath + sourceFile.TemplateId.ToString() + "/";
                if (RMTemplateRelationshipDao.Exists(idPath, sourceRecord.TemplateId))
                {
                    logger.Info("Destination Box:{0} contains same record template:{1} and don't need add source record template to des box.", desBoxId, sourceRecordTemplate.UniqueId);
                }
                else
                {
                    logger.Info("Destination Box:{0} doesn't contains same record template:{1} and need add source record template to des box.", desBoxId, sourceRecordTemplate.UniqueId);
                    var ancestorTemplateIds = idPath.Trim('/').Split('/').ToList();
                    RMTemplateDao.AddTemplateRelatonship(ancestorTemplateIds, sourceRecord.TemplateId);
                    if (!isPartOfFolderMove)
                    {
                        await PushColumnToRecordAsync(sourceRecordTemplate, sourceFile.Id);
                    }
                }
            }
        }

        private async Task<PhysicalRecordActionAudit> InnerMoveFileAsync(IPhysicalFile file, Guid boxId, string destinationPath, bool isFileLevelMove, string newName = "", bool isRealTimeMove = false)
        {
            var auditList = new List<PhysicalRecordActionAudit>();
            logger.Debug($"before metaInfo Count: {file.Fields?.Count}");
            UpdateParentIdForAlliance(file);
            logger.Debug($"after metaInfo Count: {file.Fields?.Count}");
            await ProcessFileTemplateAsync(file.BoxId, boxId, DestinationLocationId, file.TemplateId);
            int fileScopePermissionId = await ProcessFilePermissionAsync(file, isFileLevelMove);
            int cosmosDBFileScopePermissionId = ResetCosmosDBFileScopePermissionId(fileScopePermissionId, isFileLevelMove, file.BoxId.ToString());
            var originFileFullPath = file.DirPath;
            var locationId = DestinationLocationId;
            var fileName = string.IsNullOrEmpty(newName) ? file.Name : newName;
            await file.Records.ForEachAsync(async r => await MoveRecordAsync(file, r, boxId, false, cosmosDBFileScopePermissionId, destinationPath + $"/{file.Name}"));
            RecordsHistoryService.AddPhysicalAudit(auditList);
            file.DisposalDueDate = DueDateUtil.ConvertStringDueDate2Long("RM_JS_JM_EndTimePending");
            file.LocationId = locationId;
            file.BoxId = boxId;
            var locationField = new TaxonomyColumnValue() { Id = locationId.ToString(), Name = DestinationLocation.Name };
            file[MetaInfo.HomelocationId] = JsonConvert.SerializeObject(locationField);
            file.Name = fileName;
            file[MetaInfo.NameOrTitleId] = fileName;
            // If it's real time move, we only update the location and box info, and keep the rule info if the file is already manual approved.
            // Otherwise, we will reset the rule info to make sure the file won't be processed by any rule which has been already triggered based on the original location/box info.
            if (!(isRealTimeMove && file.ManualApprovedStatus == (int)SOApproveDBStatus.Approved))
            {
                file.RuleId = Guid.Empty;
            }
            file.DisposalStatus = (int)SOApproveDBStatus.None;
            file.ScopePermissionId = cosmosDBFileScopePermissionId;
            file.ParentId = boxId == Guid.Empty ? locationId : boxId;
            List<Guid> ancestors = new List<Guid>();
            ancestors.Add(locationId);
            if (boxId != Guid.Empty)
            {
                ancestors.Add(boxId);
            }
            file.Ancestors = ancestors;
            var parentPath = originFileFullPath.Remove(originFileFullPath.Length - (file.Name.Length + 1));
            var result = RecordsHistoryService.BuildPhysicalActionAuditForJob(file.Id, PhysicalActionType.Move, false, mJobRunBy, parentPath, destinationPath);
            await AddToMovePickAsync(file, PickMoveStatusType.Successfull, RMNodeType.PhyFile);
            file.Update(true, true);
            logger.Info($"Move file:[{originFileFullPath}] to box:[{DestinationBoxId}] or to location:[{DestinationLocationId}]");
            return result;
        }

        private async Task<PhysicalRecordActionAudit> InnerMoveRecordAsync(IPhysicalFile file, IPhysicalRecord record, Guid boxId, string destinationPath, bool isFileLevelMove, string newName = "")
        {
            var auditList = new List<PhysicalRecordActionAudit>();
            var originFileFullPath = record.DirPath;
            await ProcessRecordTemplateAsync(file, record, boxId, false);

            if (ConflictOption == Contract.Object.RealTime.NameConflictOption.Overwrite)
            {
                ProcessSameRecord(file, record);
            }
            record.FileId = file.Id;
            record.BoxId = boxId;
            record.RuleId = Guid.Empty;
            record.DisposalStatus = (int)SOApproveDBStatus.None;
            record.ParentId = file.Id;
            file.ScopePermissionId = ProcessPermissionRecord(file);
            List<Guid> ancestors = new List<Guid>();
            ancestors.Add(record.LocationId);
            if (boxId != Guid.Empty)
            {
                ancestors.Add(boxId);
            }
            ancestors.Add(file.Id);
            record.Ancestors = ancestors;
            var parentPath = originFileFullPath.Substring(0, originFileFullPath.LastIndexOf('/')); 
            var result = RecordsHistoryService.BuildPhysicalActionAuditForJob(record.Id, PhysicalActionType.Move, false, mJobRunBy, parentPath, destinationPath);
            await AddToMovePickAsync(record, PickMoveStatusType.Successfull, RMNodeType.PhyRecord);
            record.Update(true, true);
            logger.Info($"Move file:[{originFileFullPath}] to folder [{DestinationFolderId}]");

            return result;
        }

        private int ProcessPermissionRecord(IPhysicalFile file)
        {
            int fileScopePermissionId = 0;
            var fileScopePermission = RMScopePermissionDao.HasBreakInheritPermission(file.Id.ToString());
            if (fileScopePermission != null)
            {
                logger.Info($"Current file has break inherit permssion, file id:[{file.Id.ToString()}], file scope permission id:[{fileScopePermission.Id}].");
                fileScopePermissionId = fileScopePermission.Id;
            }
            return fileScopePermissionId;
        }
        private int ResetCosmosDBFileScopePermissionId(int fileScopePermissionId, bool isFileLevelMove, string sourceBoxId)
        {
            int resetFileScopePermissionId = 0;
            //源端File没有打破权限，需要根据目的端Location/Box权限进行赋值ScopePermissionId
            //源端File打破权限，则仍使用原有ScopePermissionId
            if (fileScopePermissionId == 0)
            {
                //往目的端Box Move需要检查目的端Box和目的端Location的Permission
                if (DestinationBoxId != Guid.Empty)
                {
                    if (DestinationBoxScopePermission != null)
                    {
                        resetFileScopePermissionId = DestinationBoxScopePermission.Id;
                    }
                    else if (DestinationLocationScopePermission != null)
                    {
                        resetFileScopePermissionId = DestinationLocationScopePermission.Id;
                    }
                    else
                    {
                        resetFileScopePermissionId = 0;
                    }
                }
                //往目的端Location Move需要检查检查源端Box的Permission和目的端Location的Permission
                else
                {
                    if (isFileLevelMove)
                    {
                        //File Level Move到目的端Location，只需要检查目的端Location是否有打破继承权限
                        if (DestinationLocationScopePermission != null)
                        {
                            resetFileScopePermissionId = DestinationLocationScopePermission.Id;
                        }
                        else
                        {
                            resetFileScopePermissionId = 0;
                        }
                    }
                    else
                    {
                        //Box Level带Folder Move到目的端Location的case，需要检查源端BoxPermission和目的端Location Permission
                        var sourceBoxScopePermission = RMScopePermissionDao.HasBreakInheritPermission(sourceBoxId);
                        if (sourceBoxScopePermission != null)
                        {
                            resetFileScopePermissionId = sourceBoxScopePermission.Id;
                        }
                        else if (DestinationLocationScopePermission != null)
                        {
                            resetFileScopePermissionId = DestinationLocationScopePermission.Id;
                        }
                        else
                        {
                            resetFileScopePermissionId = 0;
                        }
                    }
                }
            }
            else
            {
                resetFileScopePermissionId = fileScopePermissionId;
            }
            return resetFileScopePermissionId;
        }

        

        
        /// <summary>
        /// 批量修改 UseLongest Hold
        /// </summary>
        private void UpdateBatchUseLongestHold()
        {
            if (HoldConflictOption == Contract.Object.RealTime.PhysicalMoveHoldConflictOption.UseLongest)
            {
                if (precessHoldPhysicalFieldList.IsNullOrEmpty() || holdRecordList.IsNullOrEmpty())
                {
                    return;
                }
                // 取最大的时间的Record
                Record maxReleaseTimeRecord = holdRecordList.MaxBy(x => x.HoldReleaseTime);
                foreach (var item in precessHoldPhysicalFieldList)
                {
                    if (item is IPhysicalBox physicalBox)
                    {
                        UpdateHoldRecord(physicalBox, maxReleaseTimeRecord);
                        logger.Info($"Success to update useLongestHold, physicalBox:{physicalBox.Id}");
                    }
                    else if (item is IPhysicalFile physicalFile)
                    {
                        UpdateHoldRecord(physicalFile, maxReleaseTimeRecord);
                        logger.Info($"Success to update useLongestHold, physicalFile:{physicalFile.Id}");
                    }
                }
            }

        }

        private void UpdateParentIdForAlliance(IPhysicalFile file)
        {
            try
            {
                //RecordAllianceDao.PhysicalFileMoveWithHold(file.Id, file.BoxId, DestinationBoxId, DestinationLocationId);
                List<Record> srcHolds = ExplorerDao.GetHoldRecordsByIds(new List<Guid>() { file.Id, file.BoxId });
                var destHold = DestinationBoxId == Guid.Empty ? null : ExplorerDao.GetHoldRecordsByIds(new List<Guid>() { DestinationBoxId }).FirstOrDefault();
                if (srcHolds.Any(a => a.Id == file.Id))
                {
                    //File本身是Hold的
                    var srcHold = srcHolds.First(a => a.Id == file.Id);
                    if (destHold != null)
                    {
                        ////目的端的Container 有Hold,需要根据冲突解决方案处理
                        if (HoldConflictOption == Contract.Object.RealTime.PhysicalMoveHoldConflictOption.UseDest)
                        {
                            RemoveSourceFileHold(file, srcHold.Id);
                        }
                        else if (HoldConflictOption == Contract.Object.RealTime.PhysicalMoveHoldConflictOption.UseLongest)
                        {
                            if (srcHold.HoldReleaseTime > destHold.HoldReleaseTime)
                            {
                                RemoveSourceFileHold(file, srcHold.Id);
                                if (mIsDisposalJob)
                                {
                                    UpdateHoldRecord(new PhysicalBox(DestinationBoxId), srcHold);
                                }
                                else
                                {
                                    // 加入到处理列表中
                                    precessHoldPhysicalFieldList.Add(new PhysicalBox(DestinationBoxId));
                                    // 加入hold
                                    holdRecordList.Add(srcHold);
                                }
                            }
                            else
                            {
                                if (mIsDisposalJob)
                                {
                                    UpdateHoldRecord(file, destHold);
                                }
                                else
                                {
                                    // 加入到处理列表中
                                    precessHoldPhysicalFieldList.Add(file);
                                    // 加入hold
                                    holdRecordList.Add(destHold);
                                }
                            }
                        }
                        else
                        {
                            if (srcHold.HoldId != destHold.HoldId)
                            {
                                //异常失败, 不允许Move (这种情况会再之前的逻辑中弹出冲突解决的提示框)
                                throw new GCommon.Utility.AveException(I18NEntity.GetString("RM_BCM_PDM_HasDeffrentHoldTime"));
                            }
                        }
                    }
                }
                else if (file.BoxId != Guid.Empty && srcHolds.Any(a => a.Id == file.BoxId))
                {
                    //File本身没有Hold,  但源端Box有Hold
                    if (destHold == null)
                    {
                        Record srcContainerHOld = srcHolds.First(a => a.Id == file.BoxId);
                        UpdateHoldRecord(file, srcContainerHOld);
                    }
                    else
                    {
                        //目的端Container 有Hold, 啥也不用做
                    }
                }
                logger.Info($"Success to update alliance parentId, fileId:{file.Id}");
            }
            catch (AveException aex)
            {
                logger.Error($"An error occurred when update alliance parentId, fileId:{file.Id}, aex message:{aex.Message}");
                throw;
            }
            catch (Exception ex)
            {
                logger.Error($"An error occurred when update alliance parentId, fileId:{file.Id}, message:{ex.Message}");
            }
            finally
            {
                //var holdFile = ExplorerDao.GetPhysicalRecordById(file.Id);
                //file = new PhysicalFile(holdFile);
            }
        }

        private static void UpdateHoldRecord(IPhysicalFile file, Record destHold)
        {
            file.HoldStatus = true;
            file.HoldType = destHold.HoldType;
            file.HoldReleaseTime = destHold.HoldReleaseTime;
            file.HoldId = destHold.HoldId;
            file.HoldBy = destHold.HoldBy;
            file.HoldByUsers = destHold.HoldByUsers;
            file.HoldUntilTimes = destHold.HoldUntilTimes;
            file.AppendHolds_Array = destHold.AppendHolds_Array;
            file.DisposalDueDate = destHold.DisposalDueDate;
            file.Update(true, true);
        }

        private static void UpdateHoldRecord(IPhysicalBox box, Record destHold)
        {
            box.HoldStatus = true;
            box.HoldType = destHold.HoldType;
            box.HoldReleaseTime = destHold.HoldReleaseTime;
            box.HoldId = destHold.HoldId;
            box.HoldBy = destHold.HoldBy;
            box.HoldByUsers = destHold.HoldByUsers;
            box.HoldUntilTimes = destHold.HoldUntilTimes;
            box.AppendHolds_Array = destHold.AppendHolds_Array;
            box.DisposalDueDate = destHold.DisposalDueDate;
            box.Update(true, true);
        }

        private void RemoveSourceFileHold(IPhysicalFile file, Guid srcFileHoldId)
        {
            file.HoldStatus = false;
            file.HoldType = 0;
            file.HoldReleaseTime = DateTime.MinValue.Ticks;
            file.HoldId = null;
            file.HoldBy = null;
            file.HoldByUsers = null;
            file.HoldUntilTimes = null;
            file.AppendHolds_Array = new string[0];
            file.DisposalDueDate = file.PreviousDisposalDueDate;
            file.Update(true, true);

        }

        private void RemoveRelatedRecordProperties(Guid physicalObjectId, string relatedRecordDBValue)
        {
            if (string.IsNullOrEmpty(relatedRecordDBValue)) return;
            var utility = new RelatedRecordsUtility();
            utility.RemoveRelatedProperty(physicalObjectId);
        }

        private bool IsAllowMove(IPhysicalFile file) {
            bool isAllow = true;
            if (DestinationBoxId != Guid.Empty)
            {
                var srcParentId = file.BoxId == Guid.Empty ? file.LocationId : file.BoxId;
                var destParentId = DestinationBoxId == Guid.Empty ? DestinationLocationId : DestinationBoxId;
                if (!ExplorerDao.CanPhysicalFileMove(file.Id, srcParentId, destParentId))
                {
                    isAllow = false;
                }
            }
            return isAllow;
        }
        private bool IsLoanFile(IPhysicalFile file)
        {

            var recordLoanAlliance = RecordLoanAllianceDao.GetPhyRecordAllianceById(file.Id);
            if (recordLoanAlliance != null && recordLoanAlliance.Count > 0)
            {
                return true;
            }
            return false;
        }
        private bool HasLoanFile(IPhysicalBox box)
        {
            var fileIds = box.Files.Select(f => f.Id).ToList();
            var recordLoanAlliances = RecordLoanAllianceDao.GetPhyRecordAllianceByIds(fileIds);
            if (recordLoanAlliances != null && recordLoanAlliances.Count > 0)
            {
                return true;
            }
            return false;
        }
        public async Task MoveRecordAsync(IPhysicalFile file, IPhysicalRecord record, Guid boxId, bool needCheckConflictOption, int fileScopePermissionId, string destinationPath)
        {
            await ProcessRecordTemplateAsync(file, record, boxId);
            //只有Merge方式Move且目的端存在同名Folder才会出现相同文件的情况
            if (ConflictOption == Contract.Object.RealTime.NameConflictOption.Overwrite && needCheckConflictOption)
            {
                ProcessSameRecord(file, record);
            }
            var locationId = DestinationLocationId;
            record.FileId = file.Id;
            record.BoxId = boxId;
            record.LocationId = locationId;
            record.RuleId = Guid.Empty;
            //record.RecordStatus = 1;
            record.DisposalStatus = (int)SOApproveDBStatus.None;
            record.ScopePermissionId = fileScopePermissionId;
            record.ParentId = file.Id;
            List<Guid> ancestors = new List<Guid>();
            ancestors.Add(locationId);
            if (boxId != Guid.Empty)
            {
                ancestors.Add(boxId);
            }           
            ancestors.Add(file.Id);
            record.Ancestors = ancestors;
            
            var originalPath = record.DirPath[..record.DirPath.LastIndexOf("/")];
            record.Update(true, true);
            await AddToMovePickAsync(record, PickMoveStatusType.Successfull, RMNodeType.PhyRecord);
            logger.Info($"Move Record:[{record.DirPath}] to box:[{boxId}] or to location:[{locationId}]");
        }

        private void ProcessSameRecord(IPhysicalFile desFile, IPhysicalRecord sourceRecord)
        {
            List<IPhysicalRecord> result = new List<IPhysicalRecord>();
            result = desFile.Records.Where(r => r.Name == sourceRecord.Name && r.RecordStatus != (int)RMRecordStatus.RMDeleted && r.RecordStatus != (int)RMRecordStatus.MoveOverwrite).ToList();
            if (result != null && result.Count > 0)
            {
                var destinationRecord = result.First();
                destinationRecord.RecordStatus = (int)RMRecordStatus.MoveOverwrite;
                RemoveRelatedRecordProperties(destinationRecord.Id, destinationRecord.RelatedRecords);
                destinationRecord.Update(true, true);
                logger.Info($"Delete conflict record. id:[{destinationRecord?.Id}]");
            }
        }
    }
}
