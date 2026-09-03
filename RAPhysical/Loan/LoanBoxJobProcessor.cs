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
using AvePoint.GCommon.Utility;
using AvePoint.RA.Common;
using AvePoint.RA.Common.Report;
using AvePoint.RA.Common.SystemSetting;
using AvePoint.RA.Common.Util;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.Contract.JobMonitor;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RMWeb.CP;
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using AvePoint.RA.Contract.RMWeb.Physical;
using AvePoint.RA.Contract.RMWeb.Tree.Base;
using AvePoint.RA.DB.Core;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Dao.Impl;
using AvePoint.RA.DB.Explorer.Dao.CosmosImp;
using AvePoint.RA.DB.Explorer.Model;
using AvePoint.RA.DB.Model;
using AvePoint.RA.DB.Model.Extension;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.RAPhysical.Loan
{
    public class LoanBoxJobProcessor
    {
        protected AvePoint.RA.Contract.Services.IRALogger logger = RALogger.GetInstance(typeof(LoanBoxJobProcessor));
        #region interface
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

        private IPhysicalRequestDao mPhysicalRequestDao;
        public IPhysicalRequestDao PhysicalRequestDao
        {
            get
            {
                if (mPhysicalRequestDao == null)
                {
                    mPhysicalRequestDao = (IPhysicalRequestDao)PlatformWindsorManager.GetService(typeof(IPhysicalRequestDao));
                }
                return mPhysicalRequestDao;
            }
        }
        private IAccountDao mAccountDao;
        public IAccountDao AccountDao
        {
            get
            {
                if (mAccountDao == null)
                {
                    mAccountDao = (IAccountDao)PlatformWindsorManager.GetService(typeof(IAccountDao));
                }
                return mAccountDao;
            }
        }

        private IExplorerService mExplorerService;
        public IExplorerService ExplorerService
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

        private IGeneralSettingService mGeneralSettingService;
        public IGeneralSettingService GeneralSettingService
        {
            get
            {
                if (mGeneralSettingService == null)
                {
                    mGeneralSettingService = (IGeneralSettingService)PlatformWindsorManager.GetService(typeof(IGeneralSettingService));
                }
                return mGeneralSettingService;
            }
        }

        private ITaxonomyService mTaxonomyService;
        public ITaxonomyService TaxonomyService
        {
            get
            {
                if (mTaxonomyService == null)
                {
                    mTaxonomyService = (ITaxonomyService)PlatformWindsorManager.GetService(typeof(ITaxonomyService));
                }
                return mTaxonomyService;
            }
        }

        private IPhysicalReqeustService mPhysicalRequestService;
        public IPhysicalReqeustService PhysicalRequestService
        {
            get
            {
                if (mPhysicalRequestService == null)
                {
                    mPhysicalRequestService = (IPhysicalReqeustService)PlatformWindsorManager.GetService(typeof(IPhysicalReqeustService));
                }
                return mPhysicalRequestService;
            }
        }

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

        public IRecordsHistoryService RecordsHistoryService => PlatformWindsorManager.GetService<IRecordsHistoryService>();

        #endregion
        private string mJobId = string.Empty;
        public LoanBoxJobProcessor(JobType jobType, string jobId)
        {
            mJobId = jobId;
            ReportMangerFactory.Instance.Init(mJobId, jobType, true);
            JobInfoUpdater.UpdateJobState(mJobId, (int)JobStatus.InProgress);
            ReportManager.StartUpdateJobProgress();
        }

        public async Task RunAsync()
        {
            logger.Info("Start to run loan box result job.");
            RMSubJob subJobWithContext = SubJobDao.GetSubJob(mJobId, true);
            logger.Info("Get job message:{0}", subJobWithContext.JobContext.Content);
            var jobParam = SerializerHelper.DeserializeByDataContractSerializer<BoxLoanJobMessage>(subJobWithContext.JobContext.Content);
            PhysicalRequestParam requestsParam = jobParam.RequestsParam;
            var  returnsList = jobParam.Returns;
            var actionType = jobParam.LoanAction;
            if (actionType == Contract.Explorer.LoanAction.Loan)
            {
                await LoanActionAsync(requestsParam, actionType);
            }
            else if (actionType == Contract.Explorer.LoanAction.Reutrn)
            {
                await ReturnActionAsync(returnsList, actionType);
            }
        }

        private async Task ReturnActionAsync(List<PhysicalReturnObject> returnsList, LoanAction actionType)
        {
            logger.Info("Start return action.");
            List<Tuple<ItemActionResult, PhysicalObjectDto>> resultList = new List<Tuple<ItemActionResult, PhysicalObjectDto>>();
            foreach (var returnObj in returnsList)
            {
                try
                {
                    var phyBox = ExplorerDao.GetPhysicalRawDataById(returnObj.UniqueId);
                    AOSUserDto boxLoanUser = phyBox.GetPersonalHoldData()?.GetPeopleOrGroupColumnValue()?.FirstOrDefault();
                    var allLoanedFolderIds = RecordLoanAllianceDao.GetPhyFoldersIdByBoxIds(new List<Guid>() { returnObj.UniqueId });
                    var pageSize = 100;
                    for (int pageIndex = 0; pageIndex <= allLoanedFolderIds.Count / pageSize; pageIndex++)
                    {
                        var loanedIds = allLoanedFolderIds.Skip(pageSize * pageIndex).Take(pageSize).ToList();
                        resultList.AddRange(await ExplorerService.ReutrnPhyFilesByBoxIdAsync(boxLoanUser, loanedIds, pageSize, pageIndex));
                    }
                    await RecordLoanAllianceDao.BatchDeleteRecordAllianceByIdsAsync(new List<Guid>() { returnObj.UniqueId });
                    var physicalAudit = RecordsHistoryService.BuildPhysicalReturnLoanAudit(phyBox.Id, phyBox.CustomColumnDic);
                    RecordsHistoryService.AddPhysicalAudit([physicalAudit]);
                    phyBox.RemovePersonalHoldData();
                    ExplorerDao.Upsert(phyBox);
                    ReportManager.SendJobDetail(new JMPhyBoxLoanJobDetails()
                    {
                        Name = phyBox.LeafName,
                        Level = GetNodeTypeString(RMNodeType.PhyBox),
                        Status = JobDetailsStatus.Successful
                    });
                }
                catch (Exception e)
                {
                    logger.Warn($"return {returnObj.UniqueId} error: {e}");
                }

                foreach (var item in resultList)
                {
                    ReportManager.SendJobDetail(new JMPhyBoxLoanJobDetails()
                    {
                        Name = item.Item2.Name,
                        Level = GetNodeTypeString(item.Item2.NodeType),
                        Status = (JobDetailsStatus)item.Item1.Status
                    });
                }
            }
            ReportManager.SetJobFinished(JobStatus.Finished);
        }

        private async Task LoanActionAsync(PhysicalRequestParam requestsParam, LoanAction actionType)
        {
            logger.Info("Start loan action.");
            List<int> failedIds = new List<int>();
            List<Tuple<ItemActionResult, PhysicalObjectDto>> resultList = new List<Tuple<ItemActionResult, PhysicalObjectDto>>();
            Dictionary<Guid, List<RMPhysicalRequest>> mailTempList = new();

            DateTime endDateTime = DateTime.MinValue;
            string holdUserId = string.Empty;
            List<RMPhysicalRequest> rmRequest = ConvertUtil.ConvertDto2Domain(requestsParam.Requests).OrderBy(r => r.Id).ToList();
            var requestIds = rmRequest.Select(_ => _.Id).ToList();
            var dbReuqests = PhysicalRequestDao.GetRequestByIds(requestIds);

            foreach (RMPhysicalRequest request in rmRequest)
            {
                RMPhysicalRequest dbRequest = dbReuqests.FirstOrDefault(a => a.Id == request.Id);
                if (dbRequest == null)
                {
                    logger.Warn("Request {0}, id {1} has been approved or rejected already", request.Title, request.Id);
                    failedIds.Add(request.Id);
                    continue;
                }
                List<RMPhysicalRequest> groupRequests = dbRequest.GroupRequestId == Guid.Empty ? new List<RMPhysicalRequest> { dbRequest }
                                        : dbReuqests.Where(_ => _.GroupRequestId == dbRequest.GroupRequestId).ToList();
                if (groupRequests != null && groupRequests.Count > 0 && groupRequests[0].Status == (int)PhysicalRequestStatus.WaitingForApproval)
                {
                    foreach (var phyRequest in groupRequests) 
                    {
                        var metaInfo = phyRequest.MetaData;
                        if (phyRequest.Type == (int)PhysicalRequestType.Loan)
                        {
                            if (phyRequest.EndTime > 0)
                            {
                                endDateTime = new DateTime(phyRequest.EndTime);
                            }
                            holdUserId = phyRequest.HoldUserId;
                            AOSUserDto aosHoldUser = null;
                            if (!string.IsNullOrEmpty(holdUserId))
                            {
                                aosHoldUser = (await AccountDao.GetUserByUserIdAsync(holdUserId))?.Convert2AOSUser();
                            }
                            else
                            {
                                aosHoldUser = new AOSUserDto() { DisplayName = phyRequest.HoldByDisplayName };
                            }

                            //更新RecordAlliance, Insert or Update PersonalHold的记录
                            //更新PhysicalFile的状态到Hold
                            ExplorerService.UpdatePhysicalRecordState2Hold(new List<string> { phyRequest.PhysicalFileId }, aosHoldUser, endDateTime.Ticks);

                            var loanRequest = requestsParam.Requests.First(r => r.Id == dbRequest.Id);
                            try
                            {
                                loanRequest.PhysicalFileInfo = GCommon.Utility.SerializerHelper.DeserializeByDataContractSerializer<PhysicalObjectDto>(phyRequest.MetaData);
                                var phyBoxObj = ExplorerDao.GetPhysicalRawDataById(loanRequest.PhysicalFileInfo.Id);
                                ReportManager.SendJobDetail(new JMPhyBoxLoanJobDetails()
                                {
                                    Name = phyBoxObj.LeafName,
                                    Level = GetNodeTypeString(RMNodeType.PhyBox),
                                    Status = JobDetailsStatus.Successful
                                });
                            }
                            catch (Exception e)
                            {
                                logger.Warn($"deserialize physical file info error, {e}");
                            }

                            if (loanRequest.PhysicalFileInfo != null && loanRequest.PhysicalFileInfo.NodeType == RMNodeType.PhyBox)
                            {
                                bool hasNext = true;
                                string pageIndex = string.Empty;
                                while (hasNext)
                                {
                                    var tupleData = new Tuple<Guid, AOSUserDto, long>(loanRequest.PhysicalFileInfo.Id, aosHoldUser, endDateTime.Ticks);
                                    (resultList, pageIndex, hasNext) = await ExplorerService.UpdatePhyFilesHoldStateByBoxIdAsync(tupleData, 100, pageIndex);
                                    foreach (var item in resultList)
                                    {
                                        ReportManager.SendJobDetail(new JMPhyBoxLoanJobDetails()
                                        {
                                            Name = item.Item2.Name,
                                            Level = GetNodeTypeString(item.Item2.NodeType),
                                            Status = (JobDetailsStatus)item.Item1.Status
                                        });
                                    }
                                }
                            }
                        }
                        phyRequest.Status = (int)PhysicalRequestStatus.Approved;
                        phyRequest.HoldCategory = request.HoldCategory;
                        phyRequest.HoldNumber = request.HoldNumber;
                        phyRequest.HoldUnit = request.HoldUnit;
                        phyRequest.MetaData = metaInfo;
                        phyRequest.ReviewComment = request.ReviewComment;
                        phyRequest.ModifiedTime = DateTime.UtcNow.Ticks;
                    }
                    PhysicalRequestDao.BatchUpdate(groupRequests);
                    if (mailTempList.ContainsKey(dbRequest.GroupRequestId))
                        mailTempList[dbRequest.GroupRequestId].AddRange(groupRequests);
                    else
                        mailTempList[dbRequest.GroupRequestId] = groupRequests;
                }
                else
                {
                    logger.Warn("Request {0}, id {1} has been approved or rejected already", request.Title, request.Id);
                    failedIds.Add(request.Id);
                }
            }

            if (failedIds.Count != rmRequest.Count)
            {
                foreach (var re in mailTempList)
                {
                    if (!failedIds.Any(id => re.Value.Any(_ => _.Id == id)))
                    {
                        if (re.Key == Guid.Empty)
                        {
                            foreach (var request in re.Value)
                            {
                                await PhysicalRequestService.SendEmailNotificationAsync(request.Type == (int)PhysicalRequestType.Creation ? EmailTemplateInternalType.CreationRequestApproved : EmailTemplateInternalType.LoanRequsetApproved,
                            await this.ConvertDomain2DtoAsync(new List<RMPhysicalRequest> { request }));
                            }
                        }
                        else
                        {
                            await PhysicalRequestService.SendEmailNotificationAsync(re.Value[0].Type == (int)PhysicalRequestType.Creation ? EmailTemplateInternalType.CreationRequestApproved : EmailTemplateInternalType.LoanRequsetApproved,
                            await this.ConvertDomain2DtoAsync(re.Value));
                        }
                    }
                }
            }

            logger.Info("Loan box job finished.");

            if (failedIds.Count > 0)
            {
                if (failedIds.Count == rmRequest.Count)
                {
                    ReportManager.SetJobFinished(JobStatus.Failed);
                }
                else
                {
                    ReportManager.SetJobFinished(JobStatus.FinishWithException);
                }
            }
            else
            {
                ReportManager.SetJobFinished(JobStatus.Finished);
            }
        }

        private string GetNodeTypeString(RMNodeType nodeType)
        {
            if (nodeType == RMNodeType.PhyBox)
            {
                return "RM_Common_ObjectLevel_PhysicalBox";
            }
            else if (nodeType == RMNodeType.PhyFile)
            {
                return "RM_Common_ObjectLevel_PhysicalFile";
            }
            return "";
        }

        private async Task<PhysicalRequestDto> ConvertDomain2DtoAsync(List<RMPhysicalRequest> requests)
        {
            if (requests == null || requests.Count == 0)
            {
                return null;
            }
            var domain = requests[0];
            PhysicalRequestDto dto = new PhysicalRequestDto();
            dto.Id = domain.Id;
            dto.RequestId = RecordsConstants.RequestIdPrefix + domain.Id;  //display as REC-123
            dto.Type = (PhysicalRequestType)domain.Type;
            dto.Status = (PhysicalRequestStatus)domain.Status;
            dto.CreatedTime = domain.CreatedTime;
            dto.ModifiedTime = domain.ModifiedTime;
            dto.CreatedUserId = domain.CreatedUserId;
            dto.ManagerUserId = domain.ManagerUserId;
            dto.HoldUserId = domain.HoldUserId;
            dto.HoldUserDisplay = domain.HoldByDisplayName;
            dto.Comment = domain.Comment;
            dto.DisposalClass = new PhysicalRequestDisposal();
            dto.DisposalClass.HoldCategory = (HoldCategory)domain.HoldCategory;
            dto.DisposalClass.HoldNumber = domain.HoldNumber;
            dto.DisposalClass.HoldUnit = (HoldUnit)domain.HoldUnit;
            dto.GroupRequestId = domain.GroupRequestId;
            GeneralSettingModel gls = await GeneralSettingService.GetGeneralSettingAsync();
            var endDateTime = DateTimeUtil.ConvertTimeFromUtc(domain.EndTime, gls);

            dto.DisposalClass.TimeZoneId = gls.TimeZoneId;
            dto.DisposalClass.IsDaylightSavingTime = gls.DayLight;
            dto.DisposalClass.EndTimeStr = domain.EndTime > 0 ? endDateTime.ToString(JSDateTimeFormat.DEFAULT_TIME_FORMAT) : "";

            dto.DisposalClass.EndTime = domain.EndTime;
            dto.DisposalClass.ReviewComment = domain.ReviewComment;
            if (domain.GroupRequestId == Guid.Empty)
            {
                dto.Title = domain.Title;
                dto.RecordId = domain.PhysicalFileId;
                if (domain.MetaData != null)
                {
                    dto.PhysicalFileInfo = GCommon.Utility.SerializerHelper.DeserializeByDataContractSerializer<PhysicalObjectDto>(domain.MetaData);
                }
                if (dto.PhysicalFileInfo != null)
                {
                    if (!string.IsNullOrEmpty(domain.ScopePermissionInfo))
                        dto.PhysicalFileInfo.ScopePerDto = SerializerHelper.DeserializeByDataContractSerializer<PhysicalObjectPermissionDto>(domain.ScopePermissionInfo);
                    dto.PhysicalFileInfo.HomeLocationFullPath = ExplorerService.GetPhysicalObjectFullPath(dto.PhysicalFileInfo);
                    dto.PhysicalFileInfo.TermFullPath = TaxonomyService.GetTermPathByTermId(dto.PhysicalFileInfo.TermId);
                }
            }
            else
            {
                if (dto.PhysicalFileInfos == null) dto.PhysicalFileInfos = new List<PhysicalObjectDto>();
                dto.Titles = requests.Select(_ => _.Title).ToList();
                dto.RecordIds = requests.Select(_ => _.PhysicalFileId).ToList();
                foreach (var request in requests)
                {
                    PhysicalObjectDto physicalFileInfo = null;
                    if (request.MetaData != null)
                    {
                        physicalFileInfo = GCommon.Utility.SerializerHelper.DeserializeByDataContractSerializer<PhysicalObjectDto>(request.MetaData);
                    }
                    if (physicalFileInfo != null)
                    {
                        if (!string.IsNullOrEmpty(request.ScopePermissionInfo))
                            physicalFileInfo.ScopePerDto = SerializerHelper.DeserializeByDataContractSerializer<PhysicalObjectPermissionDto>(request.ScopePermissionInfo);
                        physicalFileInfo.HomeLocationFullPath = ExplorerService.GetPhysicalObjectFullPath(physicalFileInfo);
                        physicalFileInfo.TermFullPath = TaxonomyService.GetTermPathByTermId(physicalFileInfo.TermId);
                        dto.PhysicalFileInfos.Add(physicalFileInfo);
                    }
                }
            }
            return dto;
        }
    }
}
