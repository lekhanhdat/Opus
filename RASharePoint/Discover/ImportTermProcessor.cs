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
using AvePoint.GCommon.Utility.Cryptography;
using AvePoint.RA.Common;
using AvePoint.RA.Contract.DocAve;
using AvePoint.RA.Contract.JobMonitor;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Model;
//using AvePoint.RA.DocAveService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Threading;
using AvePoint.RA.Common.Cache;
using AvePoint.RA.Common.Util;
using AvePoint.RA.I18N.Core;
using AvePoint.RA.Contract.Exceptions;
using AvePoint.RA.Contract.RMRuleManageMent;
using AvePoint.GCommon.Contract.CommonFilter;
using AvePoint.GCommon.Contract.StorageOptimization.Object;
using AvePoint.RA.Common.SystemSetting;
using AvePoint.RA.Contract.TaxonomyModel;
using System.Collections.ObjectModel;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.Contract.Aos;
using AvePoint.GCommon.Contract.Server.Login;
using DocumentFormat.OpenXml.Packaging;
using AvePoint.RA.Contract.RMWeb.CP;
using AvePoint.RA.Contract.Services;
using System.Xml;
using AvePoint.GCommon.Contract.Tree;
using AvePoint.RA.Contract.RMWeb.Setting;
using AvePoint.RA.Contract.RMWeb.StorageDevice;
using AvePoint.RA.DB.Dao.Impl;
using AvePoint.RA.Contract.RoleAssignments;
using AvePoint.RA.DB.SecurityTrimming;
using AvePoint.RA.Contract.Explorer;
using Path = System.IO.Path;
using AvePoint.GCommon.Media.StorageService;
using AvePoint.GCommon.Contract.Storage.Entity;
using AvePoint.RA.DB.Dao.Extension;
using AvePoint.RA.Contract.Common;
using System.Text.RegularExpressions;
using AvePoint.RA.SharePoint.Common;
using RAArchiverCommon.Utility;
using AvePoint.RA.Contract.Multi_Geo;
using AvePoint.RA.RACommonUtility.MultiGeo;

namespace AvePoint.RA.SharePoint.Discover
{
    public partial class ImportTermProcessor
    {
        #region Properties
        private ITermDao mTermDao;
        private ITermSetDao mTermSetDao;
        private ITermGroupDao mTermGroupDAO;
        private ITermSetMembershipDao mTermSetMembershipDAO;
        private IFSConnectionDao mFSConnectionDao { get; set; }
        private BaseJobDto mBaseJobDto;
        private IJobDetailService mJobDetailService;
        private IJobMonitorService mJobService;
        private IAOSUserWrapperService mUserWrapperService;
        private IUserService mUserService;
        private string mCurrentJobId;
        private string mPath;
        private List<JMImportTermDetail> mDetails;
        private int mSucceedCount;
        private int mFailedCount;
        private int mSkipCount;
        private int mtotalCount;
        private bool breakJob = false;
        private string mExtension;
        private int mJobProcess = 10;
        private Dictionary<Guid, TermInfo> mTermDic;
        private bool isValidFormat = true;
        private bool isRightTemplateVersion = true;
        //private List<string> mTimeZoneIds;
        private string TermSheetName = "Terms";
        private string RuleSheetName = "Rules";
        private Dictionary<PolicyLevel, List<ArchiverFilterRuleType>> mLevelAndCriteriaTypeDic;
        private Dictionary<PolicyLevel, List<ArchiverFilterRuleType>> mLevelAndCriteriaTypeDicForPhy;
        private Dictionary<ArchiverFilterRuleType, List<ArchiverFilterCondition>> mCriteriaAndConditionDic;
        private Dictionary<ArchiverFilterRuleType, List<SourceFlag>> mSourceAndCriteriaTypeDic;
        private Dictionary<string, int> mTermSetPathAndTermSettingIdMapping;
        private const int DetailUploadBatchSize = 100;
        private const int DefaultProgressBatchSize = 5;
        private int mProcessedSinceLastProgress;
        private int mExpectedDetailCount;
        private string Action_KeepDataActionString = "Declare or tag content";
        private const string Action_RemoveDataActionString = "Remove content";
        private const string Action_MoveDataActionString = "Move content";
        private const string Action_ExportOnlyActionString = "Export content";
        private const string Action_ArchiveContentActionString = "Archive content";
        private const string Action_CalculateDisposalDate = "Calculate action due date";
        private const string YesString = "Yes";
        private const string NoString = "No";

        private const string SourceType_Both = "Both";
        private const string SourceType_SP = "SharePoint Online";
        private const string SourceType_EXO = "Exchange Online";
        private const string SourceType_PHY = "Physical Records";
        private const string SourceType_FS = "File System";
        private const string SourceType_SPLocal = "SharePoint On-Premises";
        private const string SourceType_OneDrive = "OneDrive";
        private const string SourceType_AzureFile = "Azure File Share";
        private const string SourceType_Connector = "Connector";
        private const string Google_Drive = "Google Drive";
        private const string SourceType_Box = "Box";
        private string SourceType_Teams = string.Empty;
        private const string SourceType_Any = "Any";




        private const string SheetName_Term = "Terms";
        private const string SheetName_Rule = "Rules";
        private const Char PathSeparator = '|';
        private const string DEFAULTSTORAGENAME = "AvePoint Storage";
        private const string Action_M365ArchiveActionString = "Store in Microsoft 365 Archive";

        private bool isJPMCOpen;
        private bool HasUpgradeTeams;
        private bool isControlPlus;
        private bool isSupportRecordLabel;
        private int IndexChangeFromIncludeDeclaredRecord = 0;
        private int IndexChangeFromCustomColumnTimeZone = 0;
        private int IndexChangeFromLabel = 0;

        private IRuleManagerService mRuleManagerService;
        private IManualProcessManagementService mManualProcessManagementService;
        private ITaxonomyService mTaxonomyService;
        public ITermDao TermDAO
        {
            get
            {
                if (mTermDao == null)
                {
                    mTermDao = (ITermDao)PlatformWindsorManager.GetService(typeof(ITermDao));
                }
                return mTermDao;
            }
        }
        public ITermSetDao TermSetDAO
        {
            get
            {
                if (mTermSetDao == null)
                {
                    mTermSetDao = (ITermSetDao)PlatformWindsorManager.GetService(typeof(ITermSetDao));
                }
                return mTermSetDao;
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

        public ITermGroupDao TermGroupDAO
        {
            get
            {
                if (mTermGroupDAO == null)
                {
                    mTermGroupDAO = (ITermGroupDao)PlatformWindsorManager.GetService(typeof(ITermGroupDao));
                }
                return mTermGroupDAO;
            }
        }
        public ITermSetMembershipDao TermSetMembershipDAO
        {
            get
            {
                if (mTermSetMembershipDAO == null)
                {
                    mTermSetMembershipDAO = (ITermSetMembershipDao)PlatformWindsorManager.GetService(typeof(ITermSetMembershipDao));
                }
                return mTermSetMembershipDAO;
            }
        }

        public IFSConnectionDao FSConnectionDao
        {
            get
            {
                if (mFSConnectionDao == null)
                {
                    mFSConnectionDao = (IFSConnectionDao)PlatformWindsorManager.GetService(typeof(IFSConnectionDao));
                }
                return mFSConnectionDao;
            }
        }

        public IJobDetailService JobDetailService
        {
            get
            {
                if (mJobDetailService == null)
                {
                    mJobDetailService = (IJobDetailService)PlatformWindsorManager.GetService(typeof(IJobDetailService));
                }
                return mJobDetailService;
            }
        }

        public IRuleManagerService RuleManagerService
        {
            get
            {
                if (mRuleManagerService == null)
                {
                    mRuleManagerService = (IRuleManagerService)PlatformWindsorManager.GetService(typeof(IRuleManagerService));
                }
                return mRuleManagerService;
            }
        }
        public IJobMonitorService JobMonitorService
        {
            get
            {
                if (mJobService == null)
                {
                    mJobService = (IJobMonitorService)PlatformWindsorManager.GetService(typeof(IJobMonitorService));
                }
                return mJobService;
            }
        }
        public IAOSUserWrapperService UserWrapperService
        {
            get
            {
                if (mUserWrapperService == null)
                {
                    mUserWrapperService = (IAOSUserWrapperService)PlatformWindsorManager.GetService(typeof(IAOSUserWrapperService));
                }
                return mUserWrapperService;
            }
        }
        public IUserService UserSerive
        {
            get
            {
                if (mUserService == null)
                {
                    mUserService = (IUserService)PlatformWindsorManager.GetService(typeof(IUserService));
                }
                return mUserService;
            }
        }

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

        public IManualProcessManagementService ManualProcessManagementService
        {
            get
            {
                if (mManualProcessManagementService == null)
                {
                    mManualProcessManagementService = (IManualProcessManagementService)PlatformWindsorManager.GetService(typeof(IManualProcessManagementService));
                }
                return mManualProcessManagementService;
            }
        }

        private Task<GeneralSettingModel> mGeneralSetting = null;
        private Task<GeneralSettingModel> GeneralSetting
        {
            get
            {
                if (mGeneralSetting == null)
                {
                    mGeneralSetting = ((IGeneralSettingService)PlatformWindsorManager.GetService(typeof(IGeneralSettingService))).GetGeneralSettingAsync();
                }
                return mGeneralSetting;
            }
        } 

        private IRMRuleDao mRuleDao = null;
        private IRMRuleDao RuleDao
        {
            get
            {
                if (mRuleDao == null)
                {
                    mRuleDao = ((IRMRuleDao)PlatformWindsorManager.GetService(typeof(IRMRuleDao)));
                }
                return mRuleDao;
            }
        }
        private IStubSettingService mStubSettingService = null;
        private IStubSettingService StubSettingService
        {
            get
            {
                if (mStubSettingService == null)
                {
                    mStubSettingService = ((IStubSettingService)PlatformWindsorManager.GetService(typeof(IStubSettingService)));
                }
                return mStubSettingService;
            }
        }
        private IStorageDeviceService mStorageDeviceService = null;
        private IStorageDeviceService StorageDeviceService
        {
            get
            {
                if (mStorageDeviceService == null)
                {
                    mStorageDeviceService = ((IStorageDeviceService)PlatformWindsorManager.GetService(typeof(IStorageDeviceService)));
                }
                return mStorageDeviceService;
            }
        }

        private IRMKeyValueDao RMKeyValueDao => PlatformWindsorManager.GetService<IRMKeyValueDao>();
        private IRMSecurityTrimmingHelper SecurityTrimmingHelper => PlatformWindsorManager.GetService<IRMSecurityTrimmingHelper>();
        public ITenantService TenantService => PlatformWindsorManager.GetService<ITenantService>();

        private IMultiGeoDataCenterService mMultiGeoDataCenterService = null;
        private IMultiGeoDataCenterService MultiGeoDataCenterService
        {
            get
            {
                return mMultiGeoDataCenterService ??= (IMultiGeoDataCenterService)PlatformWindsorManager.GetService(typeof(IMultiGeoDataCenterService));
            }
        }
        #endregion

        protected static readonly IRALogger mLog = RALogger.GetInstance(typeof(ImportTermProcessor));
        private Random _rand => new();

        private readonly bool _hasGControlLicense;
        public ImportTermProcessor(string jobId, JobType jobType, string extension, string path, bool isControlPlus = false)
        {
            mCurrentJobId = jobId;
            mPath = path;
            mBaseJobDto = new BaseJobDto() { Id = mCurrentJobId, JobType = (int)jobType };
            mDetails = new List<JMImportTermDetail>();
            mTermDic = new Dictionary<Guid, TermInfo>();
            mSucceedCount = 0;
            mFailedCount = 0;
            mExtension = extension;
            isJPMCOpen = isControlPlus ? false : RMKeyValueDao.GetValueByKey("JPMC_Customization") != null;
            HasUpgradeTeams = RMKeyValueDao.HasUpgradeTeams();
            JobMonitorService.UpdateJobProgress(mCurrentJobId, 1);
            InitCriteriaRelationship();
            mTermSetPathAndTermSettingIdMapping = new Dictionary<string, int>();
            SourceType_Teams = I18NEntity.GetString("RM_JS_SPS_TabLabel_Teams");
            this.isControlPlus = isControlPlus;
            _hasGControlLicense = TenantService.HasInitGControlPlatForm().Result;
            isSupportRecordLabel = AccountUtility.IsSupportRecordLabel();
            if (isSupportRecordLabel)
            {
                Action_KeepDataActionString = "Tag or lock content";
                IndexChangeFromIncludeDeclaredRecord = 1;
                IndexChangeFromCustomColumnTimeZone = 2;
                IndexChangeFromLabel = 3;
            }
        }

        public async Task RunJobAsync()
        {

            try
            {
                using (CheckJobStopScope jScope = new CheckJobStopScope())
                {
                    mLog.Info("Start to import terms.");
                    mLog.Info($"Path InvalidFileNameChars: {string.Join(",", Path.GetInvalidFileNameChars())}");
                    mLog.Info($"Path InvalidPathChars: {string.Join(",", Path.GetInvalidPathChars())}");
                    if (!mExtension.Equals("CSV", StringComparison.OrdinalIgnoreCase) && !mExtension.Equals("XLSX", StringComparison.OrdinalIgnoreCase) && !mExtension.Equals("xml", StringComparison.OrdinalIgnoreCase))
                    {
                        throw new TermCsvFormateExcetion("The file is not a 'CSV' or 'XLSX' file.");
                    }
                    //DeleteOldTerms();
                    var tempFilePath = string.Empty;
                    try
                    {
                        tempFilePath = JobReportUtility.GetImportJobCSVFile(mPath);
                    }
                    catch (Exception e)
                    {
                        mLog.Error("can not download file:{0}, error:{1}", mPath, e.ToString());
                        throw;
                    }

                    if (tempFilePath.EndsWith(".xlsx"))
                    {
                        await ProcessExcelAsync(tempFilePath);
                    }
                    else if (tempFilePath.EndsWith(".csv"))
                    {
                        await ProcessCsvAsync(tempFilePath);
                    }else if (tempFilePath.EndsWith(".xml"))
                    {
                        await ProcessTermXmlAsync(tempFilePath);
                    }
                    File.Delete(tempFilePath);

                }
            }
            catch (JobStopException ex)
            {
                mLog.Info("Import Term Structure Job is stopped.");
            }
            catch (TermCsvFormateExcetion te)
            {
                isValidFormat = false;
                mLog.Error("The csv or xlsx file is error :{0}", te.ToString());
            }
            catch (Exception e)
            {
                mLog.Error("Some error occurred.Error:{0}", e.ToString());
            }
            finally
            {
                mLog.Info("SucceedCount:[{0}] FailedCount:[{1}]", mSucceedCount, mFailedCount);
                UpdateJobDetail(true);
                JobDetailService.UploadJobDetailsAndReport(mBaseJobDto);
                UpdateJobStatus(out var isNeedRunSyncCommonTask);
                if (isNeedRunSyncCommonTask)
                {
                    MultiGeoReplicaFailureLogWriter.WriteForJob(TenantLocalValue.LogonGroupId, MultiGeoOperationType.ImportTermAndRule.ToString());
                    await MultiGeoDataCenterService.RunMainDCSyncCommonDataJob(JobRunBy.Schedule);
                }
            }

        }

        private void AddUniqueJobDetail(string name, string action, JobDetailsStatus status, string comment = "")
        {
            if(!mDetails.Any(d => d.Term.Equals(name, StringComparison.Ordinal)
                        && d.Comment.Equals(comment, StringComparison.Ordinal)))
            {
                AddJobDetail(name, action, status, comment);
                mFailedCount++;
            }
        }

        private void AddtoTermDic(TermInfo curTerm)
        {
            if (curTerm.TermIntId != -1)
            {
                if (!mTermDic.ContainsKey(curTerm.TermUniqueId))
                {
                    mTermDic.Add(curTerm.TermUniqueId, curTerm);
                }
            }
        }

        private int GetTermIntId(Guid uniqueId)
        {
            if (mTermDic.ContainsKey(uniqueId))
            {
                return mTermDic[uniqueId].TermIntId;
            }
            else
            {
                throw new Exception(string.Format("Cant not find term. TermGuid:[{0}]", uniqueId));
            }
        }

        private TermConflictType CheckTermConflict(TermInfo term, ref object obj, ref int termsetId)
        {
            TermConflictType type = TermConflictType.None;
            if (string.IsNullOrEmpty(term.TermSetName))
            {
                var termGruop = TermGroupDAO.GetTermGroupByGuid(term.TermGroupUniqueId);
                obj = termGruop;
                if (termGruop != null)
                {
                    if (!term.TermGroupName.Equals(termGruop.Name) || !term.Description.Equals(termGruop.Description) || termGruop.IsRemoved)
                    {
                        if (TermGroupDAO.HasSameNameTermGroup(term.TermGroupName))
                        {
                            return TermConflictType.SkipRECSameName;
                        }
                        type = TermConflictType.Conflict;
                    }
                    else
                    {
                        type = TermConflictType.Skip;
                    }
                }
                else
                {
                    if (TermGroupDAO.HasSameNameTermGroup(term.TermGroupName))
                    {
                        return TermConflictType.SkipRECSameName;
                    }
                }
            }
            else if (string.IsNullOrEmpty(term.TermName))
            {
                var termset = TermSetDAO.GetRMTermSetByGuid(term.TermSetUniqueId);
                //var hasOtherTermSet = TermSetDAO.HasOtherTermSet(term.TermGroupUniqueId, term.TermSetUniqueId);
                obj = termset;
                if (termset != null)
                {
                    if (!term.TermSetName.Equals(termset.Name) || !term.Description.Equals(termset.Description) || termset.IsRemoved)
                    {
                        type = TermConflictType.Conflict;
                    }
                    else
                    {
                        type = TermConflictType.Skip;
                    }
                    termsetId = termset.Id;
                }
                //if (hasOtherTermSet)
                //{
                //    type = TermConflictType.Skip;
                //}
            }
            else
            {
                var termFromDB = TermDAO.GetRMTermByUniqueId(term.TermUniqueId, false);
                if (termFromDB != null)
                {
                    obj = termFromDB;
                    term.TermIntId = termFromDB.Id;
                    AddtoTermDic(term);
                    if (CheckTermconflict(term, termFromDB))
                    {
                        type = TermConflictType.Conflict;
                    }
                    else
                    {
                        type = TermConflictType.Skip;
                    }

                }
            }
            return type;
        }

        private bool CheckTermconflict(TermInfo term, RMTerm termFromDB)
        {
            return !term.TermName.Equals(termFromDB.Name) || !term.Description.Equals(termFromDB.Description) || term.IsDeprecated != termFromDB.IsDeprecated || termFromDB.IsRemoved;
        }
        private async Task<(JMImportTermDetail,int)> HandleConflictAsync(TermInfo term, object termObj, int termsetId)
        {
            int termId = -1;
            JMImportTermDetail detail = new JMImportTermDetail();
            try
            {
                detail.Action = "RM_TS_Action_Update";
                if (string.IsNullOrEmpty(term.TermSetName))
                {
                    var termFromDB = termObj as RMTermGroup;
                    //update termgroup
                    var group = await TermGroupDAO.UpdateTermGroupAsync(termFromDB.Id, term.TermGroupName, term.Description);
                    detail.Term = group.Name;
                }
                else if (string.IsNullOrEmpty(term.TermName))
                {
                    var termFromDB = termObj as RMTermSet;
                    //update termset
                    var termset = await TermSetDAO.UpdateTermSetAsync(termFromDB.Id, term.TermSetName, term.Description);
                    detail.Term = termset.Name;
                }
                else
                {
                    var termFromDB = termObj as RMTerm;
                    //update term
                    int parentId = term.ParentUniqueId == term.TermSetUniqueId ? 0 : GetTermIntId(term.ParentUniqueId);
                    var updateTerm = await TermDAO.UpdateTermAsync(term.TermName, parentId, termsetId, term.IsDeprecated, term.TermUniqueId, term.Description);
                    termId = updateTerm.Id;
                    detail.Term = updateTerm.Name;

                }
                detail.Status = JobDetailsStatus.Successful;
                detail.Comment = string.Empty;
            }
            catch (Exception e)
            {
                detail.Status = JobDetailsStatus.Failed;
                detail.Comment = "Failed to HandleConflict. For detailed information, refer to the log files.";
                mFailedCount++;
                mLog.Error("Failed to create.Error:[{0}]", e.ToString());
            }
            return (detail, termId);
        }

        private JMImportTermDetail CreateTerm(TermInfo tempTerm, ref int termSetId, out int curTermId)
        {
            curTermId = -1;
            bool needCounted = true;
            JMImportTermDetail detail = new JMImportTermDetail();
            detail.Action = "RM_TS_Action_New";//这里需要用string不能用枚举，历史遗留问题
            try
            {
                if (string.IsNullOrEmpty(tempTerm.TermSetName))
                {
                    detail.Term = tempTerm.TermGroupName;
                    var termGroup = TermGroupDAO.GetTermGroupByGuid(tempTerm.TermGroupUniqueId);
                    if (termGroup == null)
                    {
                        TermGroupDAO.CreateTermGroupById(tempTerm.TermGroupUniqueId, tempTerm.TermGroupName, tempTerm.Description, tempTerm.usingMMSSpecified);
                    }
                    else
                    {

                        needCounted = false;
                    }
                }
                else if (string.IsNullOrEmpty(tempTerm.TermName))
                {
                    detail.Term = tempTerm.TermSetName;
                    TermSetDAO.CreateTermSetByUniqueId(tempTerm.TermSetUniqueId, tempTerm.TermSetName, tempTerm.Description, tempTerm.TermGroupUniqueId);
                    var termSet = TermSetDAO.GetRMTermSetByGuid(tempTerm.TermSetUniqueId);
                    termSetId = termSet.Id;
                }
                else
                {
                    detail.Term = tempTerm.TermName;
                    if (termSetId == -1)
                    {
                        throw new Exception(string.Format("Can not find termset. TermSetId:[{0}]", termSetId));
                    }
                    int parentId = tempTerm.ParentUniqueId == tempTerm.TermSetUniqueId ? 0 : GetTermIntId(tempTerm.ParentUniqueId);
                    var term = TermDAO.CreateTermForImport(tempTerm.TermName, parentId, termSetId, tempTerm.IsDeprecated, tempTerm.TermUniqueId, tempTerm.Description);
                    curTermId = term.Id;
                }
                detail.Status = JobDetailsStatus.Successful;
                detail.Comment = string.Empty;
                if (needCounted)
                {
                    mSucceedCount++;
                }
            }
            catch (Exception e)
            {
                detail.Status = JobDetailsStatus.Failed;
                detail.Comment = "Failed to create object. For detailed information, refer to the log files.";
                mFailedCount++;
                mLog.Error("Failed to create.Error:[{0}]", e.ToString());
            }
            return detail;
        }

        private JMImportTermDetail SkipTerm(TermInfo tempTerm)
        {
            JMImportTermDetail detail = new JMImportTermDetail();
            detail.Action = "RM_TS_Action_Skip";
            if (string.IsNullOrEmpty(tempTerm.TermSetName))
            {
                detail.Term = tempTerm.TermGroupName;
                detail.Comment = "RM_TS_ITS_ExistTermGroup";
            }
            else if (string.IsNullOrEmpty(tempTerm.TermName))
            {
                detail.Term = tempTerm.TermSetName;
                detail.Comment = "RM_TS_ITS_ExistTermTermSet";
            }
            else
            {
                detail.Term = tempTerm.TermName;
                detail.Comment = "RM_TS_ITS_ExistTerm";
            }
            detail.Status = JobDetailsStatus.Skipped;
            return detail;
        }

        private JMImportTermDetail SkipTermGroupWithSameNameInReco(TermInfo tempTerm)
        {
            JMImportTermDetail detail = new JMImportTermDetail();
            detail.Action = "RM_TS_Action_Skip";
            detail.Term = tempTerm.TermGroupName;
            detail.Comment = "RM_TS_ITS_ExistSameNameTermGroup";
            detail.Status = JobDetailsStatus.Skipped;
            return detail;
        }

        private void InnerUpdateJobDetail(object details)
        {
            JobDetailService.UpdateJobDetails((List<JMImportTermDetail>)details, mBaseJobDto);
        }

        private void InitializeExpectedDetailCount(int expectedCount)
        {
            mLog.Info("Initialize expected detail count: {0}", expectedCount);
            if (expectedCount > 0)
            {
                mExpectedDetailCount = expectedCount;
            }
        }

        private void UpdateJobDetail(bool forceUpdate = false, int processedIncrement = 0)
        {
            if (processedIncrement > 0)
            {
                mProcessedSinceLastProgress += processedIncrement;
            }

            if (forceUpdate)
            {
                UpdateProcess(true);
                mProcessedSinceLastProgress = 0;
            }
            else
            {
                int progressBatchSize = CalculateProgressBatchSize();
                while (mProcessedSinceLastProgress >= progressBatchSize)
                {
                    UpdateProcess(false);
                    mProcessedSinceLastProgress -= progressBatchSize;
                }
            }

            if ((mDetails.Count >= DetailUploadBatchSize) || (forceUpdate && mDetails.Count > 0))
            {
                InnerUpdateJobDetail(mDetails);
                mDetails.Clear();
            }
        }

        private int CalculateProgressBatchSize()
        {
            if (mExpectedDetailCount > 0)
            {
                return Math.Max(1, (int)Math.Ceiling(mExpectedDetailCount / 10.0));
            }

            return DefaultProgressBatchSize;
        }

        private void UpdateProcess(bool forceUpdate)
        {
            if (forceUpdate && !breakJob && mtotalCount == mSkipCount)
            {
                JobMonitorService.UpdateJobProgress(mCurrentJobId, 100);
                return;
            }
            JobMonitorService.UpdateJobProgress(mCurrentJobId, mJobProcess);
            mJobProcess += 10;
            if (mJobProcess > 99)
            {
                mJobProcess = 99;
            }
        }
        private void UpdateJobStatus(out bool isNeedRunSyncCommonTask)
        {
            isNeedRunSyncCommonTask = false;
            if (!isRightTemplateVersion)
            {
                JobMonitorService.UpdateJobStatus(mCurrentJobId, JobStatus.Failed, "RM_JS_JM_ImportTermTemplateVersionIsLow");
                return;
            }

            if (!isValidFormat)
            {
                JobMonitorService.UpdateJobStatus(mCurrentJobId, JobStatus.Failed, "RM_JS_JM_ImportFileFormatError");
                return;
            }

            if (CheckJobStatusUtility.isStopping)
            {
                JobMonitorService.UpdateJobStatus(mCurrentJobId, JobStatus.Stopped, "");
                return;
            }
            if (mFailedCount == 0)
            {
                isNeedRunSyncCommonTask = true;
                JobMonitorService.UpdateJobStatus(mCurrentJobId, JobStatus.Finished);
                mLog.Info("Import term job finished.");
            }
            else if (mSucceedCount == 0)
            {
                JobMonitorService.UpdateJobStatus(mCurrentJobId, JobStatus.Failed, "RM_JS_JM_ImportFileFormatError");
                mLog.Info("Import term job failed.");
            }
            else if (mSucceedCount > 0 && mFailedCount > 0)
            {
                isNeedRunSyncCommonTask = true;
                JobMonitorService.UpdateJobStatus(mCurrentJobId, JobStatus.FinishWithException, "RM_SS_CommonErrorMessage");
                mLog.Info("Import term job finished with exception.");
            }
        }

        public void CheckNameIsValid(string name, TermLevel level = TermLevel.Term)
        {
            if (name.Length > 255)
            {
                throw new Exception("RM_TM_NameLenTooLongMsg");
            }
            //Name cannot contain any of the following characters:";<>|and Tab.
            char[] illegalChar = new char[] { '"', ';', '<', '>', '|', '\t' };
            char[] nameChars = name.ToArray();
            foreach (var charInName in nameChars)
            {
                if (illegalChar.Contains(charInName))
                {
                    throw new Exception(string.Format(I18NEntity.GetString("RM_TM_IllegalCharacterMsg"), level.ToString()));
                }
            }
        }
        public RMImportTermGroupObject ConvertToTermGroupObject(string[] termInfo)
        {
            RMImportTermGroupObject termGroupObject = new RMImportTermGroupObject();
            termGroupObject.Name = termInfo[TermPropertyIndex.TermGroupName];
            termGroupObject.Path = termInfo[TermPropertyIndex.TermGroupName];
            return termGroupObject;
        }

        public RMImportTermSetObject ConvertToTermSetObject(string[] termInfo)
        {
            RMImportTermSetObject termSetObject = new RMImportTermSetObject();
            termSetObject.Name = termInfo[TermPropertyIndex.TermSetName];
            if (!string.IsNullOrEmpty(termInfo[TermPropertyIndex.Description]) && termInfo[TermPropertyIndex.Description].Length > 5000)
            {
                throw new Exception("RM_TM_CustomProperties_DescriptionLengthLimit");
            }

            if (string.IsNullOrEmpty(termInfo[TermPropertyIndex.Level1]))
            {
                termSetObject.Desciption = termInfo[TermPropertyIndex.Description];
            }

            for (int i = 0; i <= 1; i++)
            {
                termSetObject.Path += termInfo[i];
                if (i < 1)
                {
                    termSetObject.Path += PathSeparator;
                }
            }
            return termSetObject;
        }
        public string GetTermObjectName(string[] termInfo)
        {
            RMImportTermObject termObject = new RMImportTermObject();
            if (!string.IsNullOrEmpty(termInfo[TermPropertyIndex.TermGroupName]))
            {
                termObject.Name = termInfo[TermPropertyIndex.TermGroupName];
            }
            if (!string.IsNullOrEmpty(termInfo[TermPropertyIndex.TermSetName]))
            {
                termObject.Name = termInfo[TermPropertyIndex.TermSetName];
            }
            if (!string.IsNullOrEmpty(termInfo[TermPropertyIndex.Level1]))
            {
                termObject.Name = termInfo[TermPropertyIndex.Level1];
            }
            if (!string.IsNullOrEmpty(termInfo[TermPropertyIndex.Level2]))
            {
                termObject.Name = termInfo[TermPropertyIndex.Level2];
            }
            if (!string.IsNullOrEmpty(termInfo[TermPropertyIndex.Level3]))
            {
                termObject.Name = termInfo[TermPropertyIndex.Level3];
            }
            if (!string.IsNullOrEmpty(termInfo[TermPropertyIndex.Level4]))
            {
                termObject.Name = termInfo[TermPropertyIndex.Level4];
            }
            if (!string.IsNullOrEmpty(termInfo[TermPropertyIndex.Level5]))
            {
                termObject.Name = termInfo[TermPropertyIndex.Level5];
            }
            return termObject.Name;
        }

        public RMImportTermObject ConvertToTermObject(string[] termInfo)
        {
            RMImportTermObject termObject = new RMImportTermObject();
            if (!string.IsNullOrEmpty(termInfo[TermPropertyIndex.Level5]))
            {
                termObject.CurrentLevel = TermPropertyIndex.Level5;
            }
            else if (!string.IsNullOrEmpty(termInfo[TermPropertyIndex.Level4]))
            {
                termObject.CurrentLevel = TermPropertyIndex.Level4;
            }
            else if (!string.IsNullOrEmpty(termInfo[TermPropertyIndex.Level3]))
            {
                termObject.CurrentLevel = TermPropertyIndex.Level3;
            }
            else if (!string.IsNullOrEmpty(termInfo[TermPropertyIndex.Level2]))
            {
                termObject.CurrentLevel = TermPropertyIndex.Level2;
            }
            else if (!string.IsNullOrEmpty(termInfo[TermPropertyIndex.Level1]))
            {
                termObject.CurrentLevel = TermPropertyIndex.Level1;
            }
            termObject.Name = termInfo[termObject.CurrentLevel];

            for (int i = 0; i <= termObject.CurrentLevel; i++)
            {
                termObject.Path += termInfo[i];
                if (i < termObject.CurrentLevel)
                {
                    termObject.Path += PathSeparator;
                }
            }

            termObject.RuleName = termInfo[TermPropertyIndex.RuleName];

            termObject.Desciption = termInfo[TermPropertyIndex.Description];       
            bool result = false;
            if (bool.TryParse(termInfo[TermPropertyIndex.Retention], out result))
            {
                if (result)
                {
                    string retentionType = termInfo[TermPropertyIndex.RetentionSourceType];
                    if (string.IsNullOrEmpty(retentionType))
                    {
                        throw new Exception("RM_JS_TM_TermImport_RetentionSourceError");
                    }
                    var spLabelName = termInfo[TermPropertyIndex.SharePointOnlineLabelName];
                    var exoLabelName = termInfo[TermPropertyIndex.ExchangeOnlineLabelName];
                    var oneDriveLabelName = termInfo[TermPropertyIndex.OneDriveLabelName];
                    var teamsLabelName = HasUpgradeTeams ? (isJPMCOpen ? termInfo[TermPropertyIndex.TeamsLabelName] : termInfo[TermPropertyIndex.TeamsLabelName - 1]) : string.Empty;
                    if (retentionType.Equals(SourceType_Any))
                    {
                        termObject.spLabel = spLabelName;
                        termObject.exoLabel = exoLabelName;
                        termObject.oneDriveLabel = oneDriveLabelName;
                        termObject.teamsLabel = teamsLabelName;
                        var enforceRetention = 0;
                        if (!string.IsNullOrEmpty(spLabelName))
                        {
                            enforceRetention |= (int)(EnforceRetentionType.SharePoint);
                        }
                        if (!string.IsNullOrEmpty(exoLabelName))
                        {
                            enforceRetention |= (int)(EnforceRetentionType.Exchange);
                        }
                        if (!string.IsNullOrEmpty(oneDriveLabelName))
                        {
                            enforceRetention |= (int)(EnforceRetentionType.OneDrive);
                        }
                        termObject.enforceRetention = enforceRetention;
                    }

                    else if (retentionType.Equals(SourceType_SP))
                    {
                        if (string.IsNullOrEmpty(spLabelName))
                        {
                            throw new Exception("RM_JS_TM_TermImport_RetentionLabelError");
                        }
                        termObject.enforceRetention = (int)EnforceRetentionType.SharePoint;
                        termObject.spLabel = termInfo[TermPropertyIndex.SharePointOnlineLabelName];
                    }
                    else if (retentionType.Equals(SourceType_EXO))
                    {
                        if (string.IsNullOrEmpty(exoLabelName))
                        {
                            throw new Exception("RM_JS_TM_TermImport_RetentionLabelError");
                        }
                        termObject.enforceRetention = (int)EnforceRetentionType.Exchange;
                        termObject.exoLabel = termInfo[TermPropertyIndex.ExchangeOnlineLabelName];
                    }
                    else if (retentionType.Equals(SourceType_OneDrive))
                    {
                        if (string.IsNullOrEmpty(oneDriveLabelName))
                        {
                            throw new Exception("RM_JS_TM_TermImport_RetentionLabelError");
                        }
                        termObject.enforceRetention = (int)EnforceRetentionType.OneDrive;
                        termObject.oneDriveLabel = termInfo[TermPropertyIndex.OneDriveLabelName];
                    }
                    else if(retentionType.Equals(SourceType_Teams) && HasUpgradeTeams)
                    {
                        if (string.IsNullOrEmpty(teamsLabelName))
                        {
                            throw new Exception("RM_JS_TM_TermImport_RetentionLabelError");
                        }
                        termObject.enforceRetention = (int)EnforceRetentionType.Teams;
                        termObject.teamsLabel = teamsLabelName;
                    }
                }
                else
                {
                    termObject.enforceRetention = 0;
                }
            }
            if (termObject.CurrentLevel == TermPropertyIndex.Level1)
            {
                if (String.IsNullOrEmpty(termObject.RuleName) && termObject.enforceRetention == 0)
                {
                    termObject.InheritParent = true;
                }
                else
                {
                    termObject.InheritParent = false;
                }
            }
            else
            {
                bool isInherit;
                if (bool.TryParse(termInfo[TermPropertyIndex.Inherit], out isInherit))
                {
                    termObject.InheritParent = isInherit;
                }
                else
                {
                    termObject.InheritParent = false;
                }
            }
            termObject.selDateType = GetTermActiveSetting(termInfo[TermPropertyIndex.TermActivationSettings]);
            if (termObject.selDateType == DateType.startTime)
            {
                var startTime = termInfo[TermPropertyIndex.StartTime];
                if (!string.IsNullOrEmpty(startTime))
                {
                    try
                    {
                        startTime = GetDateTimeStr(startTime);
                    }
                    catch
                    {
                        throw new Exception("RM_JS_TM_TermImport_TermSettingStartTimeError");
                    }
                    termObject.beginTime = startTime;
                    string timeZoneId = GetTimeZoneId(termInfo[TermPropertyIndex.TimeZone]);
                    //string timeZoneId = GeneralSetting.TimeZoneId;
                    if (string.IsNullOrEmpty(timeZoneId))
                    {
                        //default or exception
                        throw new Exception("RM_JS_TM_TermImport_TimeZoneErr");
                    }
                    termObject.TimeZoneId = timeZoneId;
                    DateTime dtStart = DateTime.Parse(termObject.beginTime);
                    dtStart = DateTimeUtil.ConvertTimeToUtcDate(dtStart, GeneralSettingConfig.FindSystemTimeZoneById(timeZoneId), true);
                    if (DateTime.Compare(dtStart, DateTime.UtcNow) < 0)
                    {
                        throw new Exception("RM_JS_TM_TermImport_StartTimeEarlier");
                    }
                }
                else
                {
                    throw new Exception("RM_JS_TM_TermImport_TermSettingStartTimeError");
                }
            }
            else if (termObject.selDateType == DateType.endTime)
            {
                var endTime = termInfo[TermPropertyIndex.StartTime];
                if (!string.IsNullOrEmpty(endTime))
                {
                    try
                    {
                        endTime = GetDateTimeStr(endTime);
                    }
                    catch
                    {
                        throw new Exception("RM_JS_TM_TermImport_TermSettingStartTimeError");
                    }
                    termObject.endTime = endTime;
                    string timeZoneId = GetTimeZoneId(termInfo[TermPropertyIndex.TimeZone]);
                    //string timeZoneId = GeneralSetting.TimeZoneId;
                    if (string.IsNullOrEmpty(timeZoneId))
                    {
                        //default or exception
                        throw new Exception("RM_JS_TM_TermImport_TimeZoneErr");
                    }
                    termObject.TimeZoneId = timeZoneId;
                    DateTime dtEnd = DateTime.Parse(termObject.endTime);
                    dtEnd = DateTimeUtil.ConvertTimeToUtcDate(dtEnd, GeneralSettingConfig.FindSystemTimeZoneById(timeZoneId), true);
                    if (DateTime.Compare(dtEnd, DateTime.UtcNow) < 0)
                    {
                        throw new Exception("RM_JS_TM_TermImport_EndTimeEarlier");
                    }
                }
                else
                {
                    throw new Exception("RM_JS_TM_TermImport_TermSettingStartTimeError");
                }
            }
            else if (termObject.selDateType == DateType.fromTimeAndToTime)
            {
                var startTime = termInfo[TermPropertyIndex.StartTime];
                if (!string.IsNullOrEmpty(startTime))
                {
                    try
                    {
                        startTime = GetDateTimeStr(startTime);
                    }
                    catch
                    {
                        throw new Exception("RM_JS_TM_TermImport_TermSettingStartTimeError");
                    }
                    termObject.beginTime = startTime;
                }
                else
                {
                    throw new Exception("RM_JS_TM_TermImport_TermSettingStartTimeError");
                }
                var endTime = termInfo[TermPropertyIndex.EndTime];
                if (!string.IsNullOrEmpty(endTime))
                {
                    try
                    {
                        endTime = GetDateTimeStr(endTime);
                    }
                    catch
                    {
                        throw new Exception("RM_JS_TM_TermImport_TermSettingEndTimeError");
                    }
                    termObject.endTime = endTime;
                }
                else
                {
                    throw new Exception("RM_JS_TM_TermImport_TermSettingEndTimeError");
                }
                string timeZoneId = GetTimeZoneId(termInfo[TermPropertyIndex.TimeZone]);
                //string timeZoneId = GeneralSetting.TimeZoneId;
                if (string.IsNullOrEmpty(timeZoneId))
                {
                    throw new Exception("RM_JS_TM_TermImport_TimeZoneErr");
                }
                termObject.TimeZoneId = timeZoneId;

                DateTime dtStart = DateTime.Parse(termObject.beginTime);
                dtStart = DateTimeUtil.ConvertTimeToUtcDate(dtStart, GeneralSettingConfig.FindSystemTimeZoneById(timeZoneId), true);

                DateTime dtEnd = DateTime.Parse(termObject.endTime);
                dtEnd = DateTimeUtil.ConvertTimeToUtcDate(dtEnd, GeneralSettingConfig.FindSystemTimeZoneById(timeZoneId), true);
                if (DateTime.Compare(dtStart, dtEnd) > 0)
                {
                    throw new Exception("RM_JS_TM_TermImport_EndTimeEarlierStartTime");
                }
                if (DateTime.Compare(dtStart, DateTime.UtcNow) < 0)
                {
                    throw new Exception("RM_JS_TM_TermImport_StartTimeEarlier");
                }
                if (DateTime.Compare(dtEnd, DateTime.UtcNow) < 0)
                {
                    throw new Exception("RM_JS_TM_TermImport_EndTimeEarlier");
                }

            }
            //JPMC-AdvanceSetting
            if (isJPMCOpen)
            {
                var advanceSetting = termInfo[TermPropertyIndex.AdvanceSetting];
                termObject.AdvanceSetting = advanceSetting;
            }

            return termObject;
        }

        public async Task<TermSettingsInfo> ConvertToTermSettingAsync(RMImportTermObject termObj, Dictionary<Guid, RMRuleInfos> rules, bool isRetire, RMTerm DBTerm)
        {
            TermSettingsInfo ts = new TermSettingsInfo();
            ts.tId = termObj.Id;
            ts.breakInhert = !termObj.InheritParent;
            ts.des = termObj.Desciption;
            if (termObj.CurrentLevel == TermPropertyIndex.Level1 || ts.breakInhert)
            {
                ts.EnforceRetention = termObj.enforceRetention;
                ts.SPRetentionLabel = termObj.spLabel;
                ts.EXORetentionLabel = termObj.exoLabel;
                ts.OneDriveRetentionLabel = termObj.oneDriveLabel;
                ts.TeamsRetentionLabel = termObj.teamsLabel;
                List<RuleDisplayInfo> ruleDisplayInfos = new List<RuleDisplayInfo>();
                if (!string.IsNullOrEmpty(termObj.RuleName))
                {
                    string[] ruleNames = termObj.RuleName.Split(';').Select(t => t.TrimStart(' ')).Distinct(StringComparer.CurrentCultureIgnoreCase).ToArray();
                    List<RMRuleInfos> findedRules = new List<RMRuleInfos>();
                    for (int i = 0; i < ruleNames.Length; i++)
                    {
                        string curRuleName = ruleNames[i];
                        var curRule = rules.Values.Where(r => r.RuleName.Equals(curRuleName, StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
                        if (curRule == null)
                        {
                            List<RMRuleInfos> existedRules = await RuleManagerService.GetSimpleRecordsRulesFromDBAsync();
                            if (!existedRules.IsNullOrEmpty())
                            {
                                curRule = existedRules.Where(e => e.RuleName.Equals(curRuleName, StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
                            }
                            if (curRule == null)
                            {
                                mLog.Warn("Can not find Rule. Name:[{0}]", curRuleName);
                                continue;
                            }
                        }
                        findedRules.Add(curRule);
                    }

                    List<RMRuleInfos> sortRules = findedRules.OrderBy(r => (int)r.RuleLevel).ToList();
                    for (int i = 0; i < sortRules.Count; i++)
                    {
                        var tempRule = sortRules[i];
                        RuleDisplayInfo ruleDisplayInfo = new RuleDisplayInfo();
                        ruleDisplayInfo.RuleName = tempRule.RuleName;
                        ruleDisplayInfo.RuleId = tempRule.RuleId;
                        ruleDisplayInfo.RuleLevel = tempRule.RuleLevel.ToString();
                        ruleDisplayInfo.RuleOrder = i + 1;
                        ruleDisplayInfos.Add(ruleDisplayInfo);
                    }
                }
                ts.infos = ruleDisplayInfos;
            }
            if (!isRetire)
            {
                ts.selDateType = termObj.selDateType;
                if (ts.selDateType == DateType.startTime)
                {
                    ts.beginTime = termObj.beginTime;
                    //ts.TimeZoneId = termObj.TimeZoneId;
                    ts.TimeZoneId = termObj.TimeZoneId;
                }
                else if (ts.selDateType == DateType.endTime)
                {
                    ts.endTime = termObj.endTime;
                    //ts.TimeZoneId = termObj.TimeZoneId;
                    ts.TimeZoneId = termObj.TimeZoneId;
                }
                else if (ts.selDateType == DateType.fromTimeAndToTime)
                {
                    ts.beginTime = termObj.beginTime;
                    ts.endTime = termObj.endTime;
                    //ts.TimeZoneId = termObj.TimeZoneId;
                    ts.TimeZoneId = termObj.TimeZoneId;
                }
            }
            else
            {
                if (DBTerm.TermExpirationFrom != 0 && DBTerm.TermExpirationTo != 0)
                {
                    ts.selDateType = DateType.fromTimeAndToTime;
                }
                else if (DBTerm.TermExpirationFrom != 0)
                {
                    ts.selDateType = DateType.startTime;
                }
                else if (DBTerm.TermExpirationTo != 0)
                {
                    ts.selDateType = DateType.endTime;
                }
                else
                {
                    ts.selDateType = DateType.noExpireDate;
                }
                ts.TimeZoneId = DBTerm.TimeZoneId;
                ts.beginTime = GetStrDateTime(DBTerm.TermExpirationFrom, DBTerm.TimeZoneId, false);
                ts.endTime = GetStrDateTime(DBTerm.TermExpirationTo, DBTerm.TimeZoneId, false);
                //ts.beginTime = GetStrDateTime(DBTerm.TermExpirationFrom, GeneralSetting.TimeZoneId, !GeneralSetting.isShowDayLight);
                //ts.endTime = GetStrDateTime(DBTerm.TermExpirationTo, GeneralSetting.TimeZoneId, !GeneralSetting.isShowDayLight);
            }
            if(isJPMCOpen)
            {
                ts.advanceSettings = DBTerm.AdvanceSettings;
            }
            return ts;
        }

        public string GetStrDateTime(long ticks, string timeZoneId, bool isDayLight)
        {
            if (0 == ticks || string.IsNullOrEmpty(timeZoneId))
            {
                return "";
            }
            var dt = DateTimeUtil.ConvertTimeFromUtc(ticks, GeneralSettingConfig.FindSystemTimeZoneById(timeZoneId), !isDayLight);
            return dt.ToString(JSDateTimeFormat.DEFAULT_TIME_FORMAT);
        }

        private DateType GetTermActiveSetting(string typeStr)
        {
            switch (typeStr)
            {
                case "Always active":
                    return DateType.noExpireDate;
                case "Take effect from":
                    return DateType.startTime;
                case "Retire after":
                    return DateType.endTime;
                case "Active from...to...":
                    return DateType.fromTimeAndToTime;
                default:
                    return DateType.noExpireDate;
            }
        }
        public string GetTimeZoneId(string timeZoneDisplayName)
        {
            return GeneralSettingConfig.TimeZones.Where(t => t.DisplayName.Equals(timeZoneDisplayName)).Select(t => t.Id).FirstOrDefault();
        }
        public async Task<List<UserInfo>> GetImportUsersAsync(string userStrs)
        {
            Dictionary<string, UserInfo> users = new Dictionary<string, UserInfo>();
            if (!string.IsNullOrEmpty(userStrs))
            {
                List<string> userList = userStrs.Split(';').Select(t => t.TrimStart(' ')).Distinct(StringComparer.CurrentCultureIgnoreCase).ToList();
                foreach (var userStr in userList)
                {
                    if (string.IsNullOrEmpty(userStr))
                    {
                        continue;
                    }
                    //check user
                    List<AOSUserDto> findedUser = await UserSerive.SearchUsersWithoutDisplayNameAsync(TenantLocalValue.LogonGroupId, userStr);
                    //var findedUser = UserWrapperService.SearchSingleAccountIgnoreCase(TenantLocalValue.LogonGroupId, userStr);
                    if (findedUser.IsNullOrEmpty())
                    {
                        mLog.Warn("Can not find user. Name:[{0}]", userStr);
                        continue;
                    }
                    UserInfo user = new UserInfo();
                    user.UserId = findedUser[0].UserId;
                    user.DisplayName = findedUser[0].DisplayName;
                    //user.Email = findedUser.Email;
                    user.InviteType = (InviteType)((int)findedUser[0].InviteType);
                    user.UserPrincipalName = findedUser[0].UserPrincipalName;
                    if (!users.ContainsKey(user.UserId))
                    {
                        users.Add(user.UserId, user);
                    }
                }
                if (users.Values.IsNullOrEmpty())
                {
                    throw new Exception("RM_JS_TM_TermImport_NoUser");
                }
            }
            else
            {
                throw new Exception("RM_JS_TM_TermImport_RecordOwnerNull");
            }
            return users.Values.ToList();
        }

        public string GetDateTimeStr(string input)
        {
            DateTime result;
            if (!DateTime.TryParse(input, out result))
            {
                result = DateTime.FromOADate(double.Parse(input));
            }
            return result.ToString();
        }
       
        private bool CheckRuleSource(string sourceType, string checkSourceType)
        {
            if (!string.IsNullOrEmpty(sourceType) && (sourceType.Equals(checkSourceType) || sourceType.Equals(SourceType_Both)))
            {
                return true;
            }
            return false;
        }

        //处理exo row写在sp row前的特殊Excel
        private async Task<RMRuleInfos> CreateRMRuleForDesAsync(string[] ruleInfo, RMRuleInfos rmRule)
        {
            #region Normal Rule
            rmRule.IsSpSource = SourceType_SP.Equals(ruleInfo[RulePropertyIndex.SourceType]) || SourceType_Both.Equals(ruleInfo[RulePropertyIndex.SourceType]);
            if (string.IsNullOrEmpty(ruleInfo[RulePropertyIndex.RuleLevel]) || string.IsNullOrEmpty(ruleInfo[RulePropertyIndex.RuleLevel].Trim()))
            {
                throw new Exception("RM_JS_TM_TermImport_NoObjectLevel");
            }
            if (ruleInfo[RulePropertyIndex.RuleLevel].Equals("List / Library / Physical box"))
            {
                ruleInfo[RulePropertyIndex.RuleLevel] = "List";
            }
            if (ruleInfo[RulePropertyIndex.RuleLevel].Equals("Folder / Physical folder"))
            {
                ruleInfo[RulePropertyIndex.RuleLevel] = "Folder";
            }
            if (ruleInfo[RulePropertyIndex.RuleLevel].Equals("Site collection"))
            {
                ruleInfo[RulePropertyIndex.RuleLevel] = "SiteCollection";
            }
            if (ruleInfo[RulePropertyIndex.RuleLevel].Equals("Document/Email"))
            {
                ruleInfo[RulePropertyIndex.RuleLevel] = "Document";
            }
            rmRule.RuleLevel = (PolicyLevel)Enum.Parse(typeof(PolicyLevel), ruleInfo[RulePropertyIndex.RuleLevel]);
            rmRule.ArchiverActions = ruleInfo[RulePropertyIndex.RuleAction];

            bool readRemoveOption_LeaveStub = false;
            bool readRemoveOption_IncludeRelatedRecords = false;
            bool readMoveOption = false;
            bool readKeepOption_DeclareRecord = false;
            bool readKeepOption_Tag = false;
            bool readRemoveOption_DeleteSiteCollectionToRecycleBin = rmRule.IsSpSource && rmRule.RuleLevel == PolicyLevel.SiteCollection;
            switch (rmRule.RuleLevel)
            {
                case PolicyLevel.SiteCollection:
                case PolicyLevel.Site:
                case PolicyLevel.List:
                    if (rmRule.ArchiverActions.Equals(Action_KeepDataActionString, StringComparison.OrdinalIgnoreCase)
                        || rmRule.ArchiverActions.Equals(Action_MoveDataActionString, StringComparison.OrdinalIgnoreCase))
                    {
                        throw new Exception("RM_JS_TM_TermImport_ActionCannotBeKeepDataOrMove");
                    }

                    readRemoveOption_LeaveStub = false;
                    readRemoveOption_IncludeRelatedRecords = false;
                    break;
                case PolicyLevel.Folder:
                    if (rmRule.ArchiverActions.Equals(Action_MoveDataActionString, StringComparison.OrdinalIgnoreCase))
                    {
                        throw new Exception("RM_JS_TM_TermImport_ActionCannotBeMove");
                    }

                    readRemoveOption_LeaveStub = false;
                    readRemoveOption_IncludeRelatedRecords = false;
                    readKeepOption_Tag = true;
                    break;
                case PolicyLevel.Item:
                    if (rmRule.ArchiverActions.Equals(Action_MoveDataActionString, StringComparison.OrdinalIgnoreCase))
                    {
                        throw new Exception("RM_JS_TM_TermImport_ActionCannotBeMove");
                    }

                    readRemoveOption_LeaveStub = false;
                    readRemoveOption_IncludeRelatedRecords = true;
                    readKeepOption_DeclareRecord = true;
                    readKeepOption_Tag = true;
                    break;
                case PolicyLevel.Document:
                    readRemoveOption_LeaveStub = true;
                    readRemoveOption_IncludeRelatedRecords = true;
                    readMoveOption = true;
                    readKeepOption_DeclareRecord = true;
                    readKeepOption_Tag = true;
                    break;
            }

            bool result = false;
            #region Remove Data - Remove content from SharePoint and destroy
            //remove data 才读IncludeRelatedRecord和IncludeDeclaredRecord
            if (rmRule.ArchiverActions.Equals(Action_RemoveDataActionString, StringComparison.OrdinalIgnoreCase))
            {
                if (readRemoveOption_IncludeRelatedRecords && bool.TryParse(ruleInfo[RulePropertyIndex.IncludeRelatedRecord], out result))
                {
                    if (result)
                    {
                        rmRule.RelatedRecordOption = AvePoint.GCommon.Contract.StorageOptimization.Object.RelatedRecordOption.Both;
                        SetArchiverStorage(ruleInfo, rmRule);
                    }
                }

                if (bool.TryParse(ruleInfo[RulePropertyIndex.IncludeDeclaredRecord], out result))
                {
                    rmRule.DeleteRecords = result;
                }
                if (bool.TryParse(ruleInfo[RulePropertyIndex.DeleteToRecycleBin + IndexChangeFromLabel], out result))
                {
                    rmRule.DeleteToRecycleBin = result;
                }
                if (isSupportRecordLabel)
                {
                    if (bool.TryParse(ruleInfo[RulePropertyIndex.IncludeLockedFileByRecordLabel], out result))
                    {
                        rmRule.IncludeDeleteRecordLabel = result;
                    }
                    else
                    {
                        rmRule.IncludeDeleteRecordLabel = false;
                    }
                }
                if (readRemoveOption_DeleteSiteCollectionToRecycleBin 
                    && bool.TryParse(ruleInfo[RulePropertyIndex.DeleteSiteCollectionToRecycleBin + IndexChangeFromLabel], out result))
                {
                    rmRule.DeleteSiteCollectionToRecycleBin = result;
                }
                if (rmRule.IsSpSource
                    && bool.TryParse(ruleInfo[RulePropertyIndex.LockRecordBeforeDestroy + IndexChangeFromLabel], out result))
                {
                    rmRule.LockRecordBeforeDestroy = result;
                }
            }

            int keepDataOption = (int)KeepDataStatus.Delete;
            //remove data 才读LeaveStub
            if (rmRule.ArchiverActions.Equals(Action_RemoveDataActionString, StringComparison.OrdinalIgnoreCase)
                && readRemoveOption_LeaveStub
                && bool.TryParse(ruleInfo[RulePropertyIndex.LeaveStub + IndexChangeFromIncludeDeclaredRecord], out result))
            {
                if (result)
                {
                    await SetStubTemplateAsync(ruleInfo, rmRule);
                    keepDataOption |= (int)KeepDataStatus.LinkToDocument;
                }
            }

            if (rmRule.IsSpSource && rmRule.ArchiverActions.Equals(Action_RemoveDataActionString, StringComparison.OrdinalIgnoreCase))
            {
                if (bool.TryParse(ruleInfo[RulePropertyIndex.ArchiverBeforeDestory + IndexChangeFromIncludeDeclaredRecord], out result))
                {
                    if (!result)
                    {
                        keepDataOption |= (int)KeepDataStatus.NotBackup;
                    }
                    else
                    {
                        SetArchiverStorage(ruleInfo, rmRule);
                    }
                }
                else
                {
                    keepDataOption |= (int)KeepDataStatus.NotBackup;
                }
            }
            #endregion

            #region Keep Data - Record Declaration and Tagging
            if (rmRule.ArchiverActions.Equals(Action_KeepDataActionString, StringComparison.OrdinalIgnoreCase))
            {
                keepDataOption |= (int)KeepDataStatus.Keep;
            }
            //keep data 才读 DeclareRecord
            if (rmRule.ArchiverActions.Equals(Action_KeepDataActionString, StringComparison.OrdinalIgnoreCase) && readKeepOption_DeclareRecord)
            {
                if (bool.TryParse(ruleInfo[RulePropertyIndex.DeclareRecord + IndexChangeFromIncludeDeclaredRecord], out result))
                {
                    if (result)
                    {
                        if(CheckRuleSource(ruleInfo[RulePropertyIndex.SourceType], SourceType_SPLocal))
                        {
                            keepDataOption |= (int)KeepDataStatus.DeclareRecord;
                        }
                        else if ((CheckRuleSource(ruleInfo[RulePropertyIndex.SourceType], SourceType_OneDrive) || CheckRuleSource(ruleInfo[RulePropertyIndex.SourceType], SourceType_SP))
                                && !(isSupportRecordLabel && (rmRule.RuleLevel == PolicyLevel.Document || rmRule.RuleLevel == PolicyLevel.Item)))
                        {
                            keepDataOption |= (int)KeepDataStatus.DeclareRecord;
                        }
                        //keepDataOption |= (int)KeepDataStatus.DeclareRecord;
                    }
                    //else
                    //{
                    //    throw new Exception("RM_JS_TM_TermImport_DeclareRecordCanNotBeFalse");
                    //}
                }
                else
                {
                    //throw new Exception("RM_JS_TM_TermImport_NoDeclareRecord");
                }
            }

            //keep data 才读 Tag
            bool doTag = false;
            if (rmRule.ArchiverActions.Equals(Action_KeepDataActionString, StringComparison.OrdinalIgnoreCase) && readKeepOption_Tag
                && bool.TryParse(ruleInfo[RulePropertyIndex.DoTag + IndexChangeFromIncludeDeclaredRecord], out doTag) && doTag)
            {
                #region tag
                List<RMTagContentInfo> tagContentInfos = new List<RMTagContentInfo>();
                bool tagWithArchived;
                if (bool.TryParse(ruleInfo[RulePropertyIndex.TagWithArchived + IndexChangeFromIncludeDeclaredRecord], out tagWithArchived))
                {
                    if (tagWithArchived)
                    {
                        RMTagContentInfo tagArchived = new RMTagContentInfo();
                        tagArchived.Type = TagContentInfoType.Archived;
                        //TODO leon need check
                        tagArchived.DateTime = DateTime.UtcNow;

                        tagContentInfos.Add(tagArchived);
                    }
                }
                bool tagWithArchivedBy;
                if (bool.TryParse(ruleInfo[RulePropertyIndex.TagWithArchivedBy + IndexChangeFromIncludeDeclaredRecord], out tagWithArchivedBy))
                {
                    if (tagWithArchivedBy)
                    {
                        RMTagContentInfo tagArchivedBy = new RMTagContentInfo();
                        tagArchivedBy.Type = TagContentInfoType.ArchivedBy;
                        //TODO leon need check
                        tagArchivedBy.DateTime = DateTime.UtcNow;

                        tagContentInfos.Add(tagArchivedBy);
                    }
                }
                bool tagWithArchivedDate;
                if (bool.TryParse(ruleInfo[RulePropertyIndex.TagWithArchivedTime + IndexChangeFromIncludeDeclaredRecord], out tagWithArchivedDate))
                {
                    if (tagWithArchivedDate)
                    {
                        RMTagContentInfo tagArchivedDate = new RMTagContentInfo();
                        tagArchivedDate.Type = TagContentInfoType.ArchivedDate;
                        //TODO leon need check
                        tagArchivedDate.DateTime = DateTime.UtcNow;

                        tagContentInfos.Add(tagArchivedDate);
                    }
                }
                bool tagWithCustomColumn;
                if (bool.TryParse(ruleInfo[RulePropertyIndex.TagWithCustomColumn + IndexChangeFromIncludeDeclaredRecord], out tagWithCustomColumn))
                {
                    if (tagWithCustomColumn)
                    {
                        RMTagContentInfo tagCustomColumn = new RMTagContentInfo();
                        tagCustomColumn.Type = GetTagType(ruleInfo[RulePropertyIndex.CustomColumnType + IndexChangeFromIncludeDeclaredRecord]);
                        if (string.IsNullOrEmpty(ruleInfo[RulePropertyIndex.CustomColumnName + IndexChangeFromIncludeDeclaredRecord]))
                        {
                            throw new Exception("RM_JS_TM_TermImport_NoCustomColumnName");
                        }
                        tagCustomColumn.ColumnName = ruleInfo[RulePropertyIndex.CustomColumnName + IndexChangeFromIncludeDeclaredRecord];
                        if (string.IsNullOrEmpty(ruleInfo[RulePropertyIndex.CustomColumnValue + IndexChangeFromIncludeDeclaredRecord]))
                        {
                            throw new Exception("RM_JS_TM_TermImport_NoCustomColumnValue");
                        }
                        tagCustomColumn.Value = ruleInfo[RulePropertyIndex.CustomColumnValue + IndexChangeFromIncludeDeclaredRecord];
                        //TODO leon need check
                        if (tagCustomColumn.Type == TagContentInfoType.DateTime)
                        {
                            var dateTimeStr = ruleInfo[RulePropertyIndex.CustomColumnValue + IndexChangeFromIncludeDeclaredRecord];
                            try
                            {
                                dateTimeStr = GetDateTimeStr(dateTimeStr);
                            }
                            catch
                            {
                                throw new Exception("RM_JS_TM_TermImport_CustomColumnValueDateTimeError");
                            }
                            var timeZoneId = GetTimeZoneId(ruleInfo[RulePropertyIndex.CustomColumnTimeZone + IndexChangeFromIncludeDeclaredRecord]);

                            //在创建rule时,tagCustomColumn.DateTime会自动赋值，这里不用赋值
                            //DateTime dt = Convert.ToDateTime(dateTimeStr);
                            //dt = DateTimeUtil.ConvertTimeToUtcDate(dt, timeZoneId, true);
                            //tagCustomColumn.DateTime = dt;
                            tagCustomColumn.Value = dateTimeStr;
                            tagCustomColumn.TimeZoneId = timeZoneId ?? (await GeneralSetting).TimeZoneId;

                        }
                        else
                        {
                            tagCustomColumn.DateTime = DateTime.MinValue;
                        }

                        tagContentInfos.Add(tagCustomColumn);
                    }
                }

                if (isSupportRecordLabel)
                {
                    if (bool.TryParse(ruleInfo[RulePropertyIndex.RetentionLabel], out var isCheckRetentionLabel) && isCheckRetentionLabel)
                    {
                        RMTagContentInfo tagLabel = new RMTagContentInfo();
                        tagLabel.Type = TagContentInfoType.RetentionLabel;
                        if (!string.IsNullOrEmpty(ruleInfo[RulePropertyIndex.Label + IndexChangeFromCustomColumnTimeZone]))
                        {
                            tagLabel.Value = ruleInfo[RulePropertyIndex.Label + IndexChangeFromCustomColumnTimeZone];
                            tagLabel.Option = (int)RetentionLabelOptions.Default;
                            tagContentInfos.Add(tagLabel);
                        }
                        else if (bool.TryParse(ruleInfo[RulePropertyIndex.RecordLabel], out result) && result)
                        {
                            tagLabel.Value = ruleInfo[RulePropertyIndex.RecordLabel];
                            tagLabel.Option = (int)RetentionLabelOptions.GetFromGeneralSetting;
                            tagContentInfos.Add(tagLabel);
                        }
                    }
                }
                else
                {
                    if (!string.IsNullOrEmpty(ruleInfo[RulePropertyIndex.Label + IndexChangeFromCustomColumnTimeZone]))
                    {
                        RMTagContentInfo tagLabel = new RMTagContentInfo();
                        tagLabel.DateTime = DateTime.UtcNow;
                        tagLabel.Type = TagContentInfoType.RetentionLabel;
                        //TODO leon need check
                        tagLabel.Value = ruleInfo[RulePropertyIndex.Label + IndexChangeFromCustomColumnTimeZone];
                        tagContentInfos.Add(tagLabel);
                    }
                }
                #endregion

                keepDataOption |= (int)KeepDataStatus.TagContent;
                rmRule.TagContentInfo = tagContentInfos;
                if (rmRule.TagContentInfo.Count == 0)
                {
                    throw new Exception("RM_JS_TM_TermImport_AtLeastHaveOneTagOption");
                }
            }
            else
            {
                if(rmRule.ArchiverActions.Equals(Action_KeepDataActionString, StringComparison.OrdinalIgnoreCase) && readKeepOption_Tag)
                {
                    if (isSupportRecordLabel && (CheckRuleSource(ruleInfo[RulePropertyIndex.SourceType], SourceType_SP) || CheckRuleSource(ruleInfo[RulePropertyIndex.SourceType], SourceType_OneDrive)))
                    {
                        throw new Exception("RM_JS_RDM_CreateRule_Validation_ConditioNoTag");
                    }
                }
                rmRule.TagContentInfo = new List<RMTagContentInfo>();
            }

            //如果是KeepData, 则至少包含Declare和Tag里的一项
            if ((keepDataOption & (int)KeepDataStatus.Keep) == (int)KeepDataStatus.Keep)
            {
                if ((keepDataOption & (int)KeepDataStatus.DeclareRecord) != (int)KeepDataStatus.DeclareRecord
                    && (keepDataOption & (int)KeepDataStatus.TagContent) != (int)KeepDataStatus.TagContent)
                {
                    if (rmRule.RuleLevel == PolicyLevel.Folder)
                    {
                        throw new Exception("RM_JS_TM_TermImport_NoTag");
                    }
                    else
                    {
                        throw new Exception("RM_JS_TM_TermImport_NoDeclareNoTag");
                    }
                }
            }
            rmRule.RuleKeepDataOption = keepDataOption;
            //rmRule.DeclareLinkFile = (!string.IsNullOrWhiteSpace(ruleInfo[RulePropertyIndex.DeclareLinkFile])) && ruleInfo[RulePropertyIndex.DeclareLinkFile].Equals("true", StringComparison.OrdinalIgnoreCase) ? true : false;
            #endregion

            #region Move - Move documents to a new destination library
            //Action是Move才读 Move设置
            if (rmRule.ArchiverActions.Equals(Action_MoveDataActionString, StringComparison.OrdinalIgnoreCase) && readMoveOption)
            {
                var moveDto = new MoveToDto();
                moveDto.IsSpecifyLocation = true;
                moveDto.LocationPath = ruleInfo[RulePropertyIndex.MoveUrl + IndexChangeFromLabel];
                moveDto.FileNameConflictOption = GetConflictOption(ruleInfo[RulePropertyIndex.ConflictResolution + IndexChangeFromLabel]);
                bool IsDeclareMoveDate = false;
                bool.TryParse(ruleInfo[RulePropertyIndex.DeclareAfterMove + IndexChangeFromLabel], out IsDeclareMoveDate);
                moveDto.NotDeclareMovedData = IsDeclareMoveDate;
                bool IsKeepClassification = false;
                bool.TryParse(ruleInfo[RulePropertyIndex.KeepReclassifyAfterMove + IndexChangeFromLabel], out IsKeepClassification);
                moveDto.isKeepClassification = IsKeepClassification;
                rmRule.MoveDto = moveDto;
            }
            #endregion
            #region exportonly
            if (rmRule.ArchiverActions.Equals(Action_ExportOnlyActionString, StringComparison.OrdinalIgnoreCase))
            {
                rmRule.EnableExport = true;
                var exportInfo = new SOExportInfo();
                exportInfo.exportSPDataOption = ExportSPDataOption.ExportWithoutArchive;
                var exportFormat = ruleInfo[RulePropertyIndex.ExportOnlyFormat];
                if (string.IsNullOrEmpty(exportFormat) || string.IsNullOrEmpty(exportFormat.Trim()))
                {
                    throw new Exception("RM_JS_TM_TermImport_NoExportFormat");
                }
                exportInfo.exportType = (AvePoint.GCommon.Contract.StorageOptimization.Object.ExportTypeValue)Enum.Parse(typeof(AvePoint.GCommon.Contract.StorageOptimization.Object.ExportTypeValue), exportFormat);
                if ((exportInfo.exportType == AvePoint.GCommon.Contract.StorageOptimization.Object.ExportTypeValue.NAA || exportInfo.exportType == AvePoint.GCommon.Contract.StorageOptimization.Object.ExportTypeValue.NARA) && rmRule.RuleLevel != PolicyLevel.Document && rmRule.RuleLevel != PolicyLevel.Folder)
                {
                    throw new Exception("RM_JS_TM_TermImport_ExportOnlyNotSupport");
                }
                SetExportToDestinationLibrary(ruleInfo, rmRule, exportInfo);
                rmRule.ExportInfo = exportInfo;
            }
            #endregion
            if (rmRule.EnableExport == true && rmRule.ExportInfo != null && rmRule.ExportInfo.exportSPDataOption == ExportSPDataOption.ExportWithoutArchive)
            {
                //export only的rule不收集approve 和 export archive 
                mLog.Info("Rule is export without archiver,not collect approve and export archive info.");
            }
            else
            {
                #region Manual Approval
                bool isEnableMannual = false;
                if (rmRule.ArchiverActions.Equals(Action_RemoveDataActionString, StringComparison.OrdinalIgnoreCase)
                    && bool.TryParse(ruleInfo[RulePropertyIndex.EnableMannualApprove + IndexChangeFromLabel], out isEnableMannual))
                {
                    rmRule.EnableManualApproval = isEnableMannual;
                    if (isEnableMannual)
                    {
                        if (string.IsNullOrEmpty(ruleInfo[RulePropertyIndex.ReviewType + IndexChangeFromLabel]) || string.IsNullOrEmpty(ruleInfo[RulePropertyIndex.ReviewType + IndexChangeFromLabel].Trim()))
                        {
                            throw new Exception("RM_JS_TM_TermImport_NoReviewType");
                        }
                        rmRule.ManualReviewType = GetReviewType(ruleInfo[RulePropertyIndex.ReviewType + IndexChangeFromLabel]);
                        if (rmRule.ManualReviewType == ReviewType.Workflow)
                        {
                            if (string.IsNullOrEmpty(ruleInfo[RulePropertyIndex.WorkflowName + IndexChangeFromLabel].Trim()))
                            {
                                throw new Exception("RM_JS_TM_TermImport_NoWorkflowInput");
                            }
                            var workflow = ManualProcessManagementService.GetSimpleProcessByName(ruleInfo[RulePropertyIndex.WorkflowName + IndexChangeFromLabel]);
                            if (workflow != null)
                            {
                                rmRule.WorkflowId = workflow.ReferenceId.ToString();
                            }
                            if (rmRule.WorkflowId == null || rmRule.WorkflowId == string.Empty || rmRule.WorkflowId == Guid.Empty.ToString())
                            {
                                throw new Exception("RM_JS_TM_TermImport_NoWorkflow");
                            }
                            bool sendEmail = false;
                            if (bool.TryParse(ruleInfo[RulePropertyIndex.SendEmail + IndexChangeFromLabel], out sendEmail))
                            {
                                rmRule.IsSendEmailToOwner = sendEmail;
                            }
                        }
                        else
                        {
                            //set user
                            string userStr = ruleInfo[RulePropertyIndex.RecordOwner + IndexChangeFromLabel];
                            if (!string.IsNullOrEmpty(userStr))
                            {
                                rmRule.Users = RuleManagerService.Convert2AOSUserDtos(await GetImportUsersAsync(userStr));
                            }
                            else
                            {
                                throw new Exception("RM_JS_TM_TermImport_UserColumnIsNull");
                            }
                            //send email
                            bool sendEmail = false;
                            if (bool.TryParse(ruleInfo[RulePropertyIndex.SendEmail + IndexChangeFromLabel], out sendEmail))
                            {
                                rmRule.IsSendEmailToOwner = sendEmail;
                                if (sendEmail)
                                {
                                    //column值为空
                                    throw new Exception("RM_JS_TM_TermImport_RecordOwnerNull");
                                }
                                if (sendEmail && rmRule.Users.IsNullOrEmpty())
                                {
                                    //column值不为空 但是没找到user
                                    throw new Exception("RM_JS_TM_TermImport_NoUser");
                                }
                            }
                        }
                    }
                }
                #endregion

                #region Export
                if (!rmRule.ArchiverActions.Equals(Action_MoveDataActionString, StringComparison.OrdinalIgnoreCase))
                {
                    bool enableExport = false;
                    if (bool.TryParse(ruleInfo[RulePropertyIndex.EnableExport + IndexChangeFromLabel], out enableExport))
                    {
                        rmRule.EnableExport = enableExport;
                        if (enableExport)
                        {
                            var exportInfo = new SOExportInfo();
                            exportInfo.exportSPDataOption = ExportSPDataOption.ExportBeforeArchive;
                            var exportFormat = ruleInfo[RulePropertyIndex.ExportFormat + IndexChangeFromLabel];
                            if (string.IsNullOrEmpty(exportFormat) || string.IsNullOrEmpty(exportFormat.Trim()))
                            {
                                throw new Exception("RM_JS_TM_TermImport_NoExportFormat");
                            }
                            exportInfo.exportType = (AvePoint.GCommon.Contract.StorageOptimization.Object.ExportTypeValue)Enum.Parse(typeof(AvePoint.GCommon.Contract.StorageOptimization.Object.ExportTypeValue), exportFormat);
                            if ((exportInfo.exportType == AvePoint.GCommon.Contract.StorageOptimization.Object.ExportTypeValue.NAA || exportInfo.exportType == AvePoint.GCommon.Contract.StorageOptimization.Object.ExportTypeValue.NARA) && rmRule.RuleLevel != PolicyLevel.Document && rmRule.RuleLevel != PolicyLevel.Folder)
                            {
                                throw new Exception("RM_JS_TM_TermImport_ExportNotSupport");
                            }
                            SetExportToDestinationLibrary(ruleInfo, rmRule, exportInfo);
                            rmRule.ExportInfo = exportInfo;
                        }
                    }
                }
                #endregion
            }
            #endregion
            return rmRule;
        }
     

        private async Task SetStubTemplateAsync(string[] ruleInfo, RMRuleInfos rmRule)
        {
            try
            {
                if (!SourceType_FS.Equals(ruleInfo[RulePropertyIndex.SourceType]) 
                    && !SourceType_SPLocal.Equals(ruleInfo[RulePropertyIndex.SourceType])
                    && !SourceType_AzureFile.Equals(ruleInfo[RulePropertyIndex.SourceType])
                    && !SourceType_Box.Equals(ruleInfo[RulePropertyIndex.SourceType]))
                {
                    var templateName = ruleInfo[RulePropertyIndex.StubTemplate + IndexChangeFromIncludeDeclaredRecord];
                    if (string.IsNullOrWhiteSpace(templateName))
                    {
                        throw new Exception("stub template name is null.");
                    }
                    var template = await StubSettingService.GetStubTemplateByNameAsync(templateName);
                    if (template == null)
                    {
                        throw new Exception("stub template not found.");
                    }
                    rmRule.StubTemplateId = template.Id;
                    rmRule.StubTemplateName = template.Name;
                }
            }
            catch (Exception e)
            {
                mLog.Error($"Error occurred while setting stub template. Rule:{rmRule.RuleName} Error:{e.ToString()}");
                throw new Exception("RM_JS_TM_TermImport_StubTemplateInvalid");
            }
        }

        private void SetArchiverStorage(string[] ruleInfo, RMRuleInfos rmRule)
        {
            try
            {
                var storageName = ruleInfo[RulePropertyIndex.ArchiveDataStorage + IndexChangeFromLabel];
                if (string.IsNullOrWhiteSpace(storageName))
                {
                    throw new Exception("archive storage name is null.");
                }
                var storage = StorageDeviceService.GetStorageDeviceByName(storageName);
                if (string.IsNullOrEmpty(storage.Id) && string.IsNullOrEmpty(storage.Name))
                {
                    throw new Exception("archive storage not found.");
                }
                if ((storage.Type == (int)StorageDeviceType.Google || storage.Type == (int)StorageDeviceType.Dropbox) && rmRule.RuleLevel == PolicyLevel.FileSysFile)
                {
                    throw new Exception("RM_JS_TM_TermImport_FSCannotUseDefaultStorage");
                }
                rmRule.StoragePolicyId = storage.Id;
                rmRule.StoragePolicyName = storage.Name;
            }
            catch (Exception e)
            {
                mLog.Error($"Error occurred while setting archive storage. Rule:{rmRule.RuleName} Error:{e.ToString()}");
                if (e.Message == "RM_JS_TM_TermImport_FSCannotUseDefaultStorage")
                {
                    throw;
                }
                else
                {
                    throw new Exception("RM_JS_TM_TermImport_ArchiveStorageLocationInvalid");
                }
            }
        }
        private void SetArchiverStorageToExportInfo(string[] ruleInfo, RMRuleInfos rmRule)
        {
            try
            {
                var storageName = ruleInfo[RulePropertyIndex.ExportLocation + IndexChangeFromLabel];
                if (string.IsNullOrWhiteSpace(storageName))
                {
                    throw new Exception("archive storage name is null.");
                }
                if (!string.IsNullOrWhiteSpace(storageName) && DEFAULTSTORAGENAME.Equals(storageName, StringComparison.OrdinalIgnoreCase))
                {
                    throw new Exception("RM_JS_TM_TermImport_ExportLocationCannotUseDefaultStorage");
                }
                var storage = StorageDeviceService.GetStorageDeviceByName(storageName);
                if (string.IsNullOrEmpty(storage.Id) && string.IsNullOrEmpty(storage.Name))
                {
                    throw new Exception("archive storage not found.");
                }
                rmRule.ExportInfo = new()
                {
                    exportLocationId = storage.Id,
                    exportLocationName = storageName
                };
                rmRule.ExportInfo.exportLocationId = storage.Id;
                rmRule.ExportInfo.exportLocationName = storage.Name;
            }
            catch (Exception e)
            {
                mLog.Error($"Error occurred while setting archive storage. Rule:{rmRule.RuleName} Error:{e.ToString()}");
                if (e.Message == "RM_JS_TM_TermImport_ExportLocationCannotUseDefaultStorage")
                {
                    throw;
                }
                else
                {
                    throw new Exception("RM_JS_TM_TermImport_ArchiveStorageInvalid");
                }
            }
        }

        private async Task<RMRuleInfos> BuildSPRuleAsync(string[] ruleInfo, string ruleSourceType, bool isSPSource)
        {
            if (string.IsNullOrEmpty(ruleInfo[RulePropertyIndex.RuleAction]) || string.IsNullOrEmpty(ruleInfo[RulePropertyIndex.RuleAction].Trim()))
            {
                throw new Exception("RM_JS_TM_TermImport_NoRuleAction");
            }

            RMRuleInfos rmRule = new RMRuleInfos();
            rmRule.RuleFilters = new List<RuleFilter>();
            #region Normal Rule
            rmRule.RuleId = Guid.NewGuid().ToString();
            rmRule.RuleName = ruleInfo[RulePropertyIndex.Name];
            rmRule.ContainerName = ruleInfo[RulePropertyIndex.ContainerName];
            int keepDataOption = (int)KeepDataStatus.Delete;

            if (!string.IsNullOrEmpty(ruleInfo[RulePropertyIndex.Description]) && ruleInfo[RulePropertyIndex.Description].Length > 5000)
            {
                throw new Exception("RM_TM_CustomProperties_DescriptionLengthLimit");
            }
            rmRule.Description = ruleInfo[RulePropertyIndex.Description];
            if (string.IsNullOrEmpty(ruleInfo[RulePropertyIndex.RuleLevel]) || string.IsNullOrEmpty(ruleInfo[RulePropertyIndex.RuleLevel].Trim()))
            {
                throw new Exception("RM_JS_TM_TermImport_NoObjectLevel");
            }
            if (ruleInfo[RulePropertyIndex.RuleLevel].Equals("List / Library / Physical box"))
            {
                ruleInfo[RulePropertyIndex.RuleLevel] = "List";
            }
            if (ruleInfo[RulePropertyIndex.RuleLevel].Equals("Folder / Physical folder"))
            {
                ruleInfo[RulePropertyIndex.RuleLevel] = "Folder";
            }
            if (ruleInfo[RulePropertyIndex.RuleLevel].Equals("Site collection"))
            {
                ruleInfo[RulePropertyIndex.RuleLevel] = "SiteCollection";
            }
            if (ruleInfo[RulePropertyIndex.RuleLevel].Equals("Document/Email"))
            {
                ruleInfo[RulePropertyIndex.RuleLevel] = "Document";
            }
            rmRule.RuleLevel = (PolicyLevel)Enum.Parse(typeof(PolicyLevel), ruleInfo[RulePropertyIndex.RuleLevel]);
            if (!string.IsNullOrEmpty(ruleInfo[RulePropertyIndex.DisposalClass]) && ruleInfo[RulePropertyIndex.DisposalClass].Length > 5000)
            {
                throw new Exception("RM_TM_CustomProperties_DisposalLengthLimit");
            }
            rmRule.DisposalClass = ruleInfo[RulePropertyIndex.DisposalClass];
            //rmRule.FilterCombineMode = ConvertCombineMode(ruleInfo[RulePropertyIndex.CombineMode]);
            rmRule.ArchiverActions = ruleInfo[RulePropertyIndex.RuleAction];
            if (rmRule.ArchiverActions.Equals(Action_M365ArchiveActionString, StringComparison.OrdinalIgnoreCase))
            {
                keepDataOption = (int)KeepDataStatus.TriggerMicrosoft365Archiving;
            }
            bool readRemoveOption_LeaveStub = false;
            bool readRemoveOption_IncludeRelatedRecords = false;
            bool readRemoveOption_IsEnableRemoveRetentionLabel = false;
            bool readMoveOption = false;
            bool readKeepOption_DeclareRecord = false;
            bool readKeepOption_Tag = false;
            //bool readKeepOption_ArchiveEachDocument = false;
            bool readRemoveOption_DeleteSiteCollectionToRecycleBin = (rmRule.IsSpSource || isSPSource) && rmRule.RuleLevel == PolicyLevel.SiteCollection;

            #region check RuleLevel
            switch (rmRule.RuleLevel)
            {
                case PolicyLevel.SiteCollection:
                case PolicyLevel.Site:
                case PolicyLevel.List:
                    if (rmRule.ArchiverActions.Equals(Action_KeepDataActionString, StringComparison.OrdinalIgnoreCase)
                        || rmRule.ArchiverActions.Equals(Action_MoveDataActionString, StringComparison.OrdinalIgnoreCase))
                    {
                        throw new Exception("RM_JS_TM_TermImport_ActionCannotBeKeepDataOrMove");
                    }

                    readRemoveOption_LeaveStub = false;
                    readRemoveOption_IncludeRelatedRecords = false;
                    //readKeepOption_ArchiveEachDocument = false;
                    break;
                case PolicyLevel.Folder:
                    if (rmRule.ArchiverActions.Equals(Action_MoveDataActionString, StringComparison.OrdinalIgnoreCase))
                    {
                        throw new Exception("RM_JS_TM_TermImport_ActionCannotBeMove");
                    }

                    readRemoveOption_LeaveStub = false;
                    readRemoveOption_IncludeRelatedRecords = false;
                    readKeepOption_Tag = true;
                    //readKeepOption_ArchiveEachDocument = true;
                    break;
                case PolicyLevel.Item:
                    if (rmRule.ArchiverActions.Equals(Action_MoveDataActionString, StringComparison.OrdinalIgnoreCase))
                    {
                        throw new Exception("RM_JS_TM_TermImport_ActionCannotBeMove");
                    }

                    readRemoveOption_LeaveStub = false;
                    readRemoveOption_IncludeRelatedRecords = true;
                    readKeepOption_DeclareRecord = true;
                    readKeepOption_Tag = true;
                    readRemoveOption_IsEnableRemoveRetentionLabel = true;
                    //readKeepOption_ArchiveEachDocument = true;
                    break;
                case PolicyLevel.Document:
                    readRemoveOption_LeaveStub = true;
                    readRemoveOption_IncludeRelatedRecords = true;
                    readMoveOption = true;
                    readKeepOption_DeclareRecord = true;
                    readKeepOption_Tag = true;
                    readRemoveOption_IsEnableRemoveRetentionLabel = true;
                    //readKeepOption_ArchiveEachDocument = true;
                    break;
            }
            #endregion

            bool result = false;
            #region Remove Data - Remove content from SharePoint and destroy
            //remove data 才读IncludeRelatedRecord和IncludeDeclaredRecord
            if (rmRule.ArchiverActions.Equals(Action_RemoveDataActionString, StringComparison.OrdinalIgnoreCase))
            {
                if (readRemoveOption_IncludeRelatedRecords && bool.TryParse(ruleInfo[RulePropertyIndex.IncludeRelatedRecord], out result))
                {
                    if (result)
                    {
                        rmRule.RelatedRecordOption = AvePoint.GCommon.Contract.StorageOptimization.Object.RelatedRecordOption.Both;
                        if (!CheckRuleSource(ruleSourceType, SourceType_SPLocal))
                        {
                            SetArchiverStorage(ruleInfo, rmRule);
                        }
                    }
                }

                if (bool.TryParse(ruleInfo[RulePropertyIndex.IncludeDeclaredRecord], out result))
                {
                    rmRule.DeleteRecords = result;
                }
                if (bool.TryParse(ruleInfo[RulePropertyIndex.DeleteToRecycleBin + IndexChangeFromLabel], out result))
                {
                    rmRule.DeleteToRecycleBin = result;
                }
                if (isSupportRecordLabel)
                {
                    if (bool.TryParse(ruleInfo[RulePropertyIndex.IncludeLockedFileByRecordLabel], out result))
                    {
                        rmRule.IncludeDeleteRecordLabel = result;
                    }
                    else
                    {
                        rmRule.IncludeDeleteRecordLabel = false;
                    }
                }

                if (readRemoveOption_DeleteSiteCollectionToRecycleBin
                    && bool.TryParse(ruleInfo[RulePropertyIndex.DeleteSiteCollectionToRecycleBin + IndexChangeFromLabel], out result))
                {
                    rmRule.DeleteSiteCollectionToRecycleBin = result;
                }
                if (isSPSource
                    && bool.TryParse(ruleInfo[RulePropertyIndex.LockRecordBeforeDestroy + IndexChangeFromLabel], out result))
                {
                    rmRule.LockRecordBeforeDestroy = result;
                }
            }


            //int keepDataOption = (int)KeepDataStatus.Delete;
            //remove data 才读LeaveStub
            if (rmRule.ArchiverActions.Equals(Action_RemoveDataActionString, StringComparison.OrdinalIgnoreCase)
                && readRemoveOption_LeaveStub
                && bool.TryParse(ruleInfo[RulePropertyIndex.LeaveStub + IndexChangeFromIncludeDeclaredRecord], out result))
            {
                if (result)
                {
                    await SetStubTemplateAsync(ruleInfo, rmRule);
                    keepDataOption |= (int)KeepDataStatus.LinkToDocument;
                }
            }

            if (rmRule.ArchiverActions.Equals(Action_RemoveDataActionString, StringComparison.OrdinalIgnoreCase))
            {
                if (bool.TryParse(ruleInfo[RulePropertyIndex.ArchiverBeforeDestory + IndexChangeFromIncludeDeclaredRecord], out result))
                {
                    if (!result)
                    {
                        keepDataOption |= (int)KeepDataStatus.NotBackup;
                    }
                    else
                    {
                        SetArchiverStorage(ruleInfo, rmRule);
                    }
                }
                else
                {
                    keepDataOption |= (int)KeepDataStatus.NotBackup;
                }
            }

            if (rmRule.ArchiverActions.Equals(Action_ArchiveContentActionString, StringComparison.OrdinalIgnoreCase))
            {
                keepDataOption = (int)KeepDataStatus.Archive;
                if (readRemoveOption_LeaveStub && bool.TryParse(ruleInfo[RulePropertyIndex.LeaveStub + IndexChangeFromIncludeDeclaredRecord], out result))
                {
                    if (result)
                    {
                        await SetStubTemplateAsync(ruleInfo, rmRule);
                        keepDataOption = (int)KeepDataStatus.ArchiveAndLeaveStub;
                    }
                }
                if (bool.TryParse(ruleInfo[RulePropertyIndex.IncludeDeclaredRecord], out result))
                {
                    rmRule.DeleteRecords = result;
                }

                if (isSupportRecordLabel)
                {
                    if (bool.TryParse(ruleInfo[RulePropertyIndex.IncludeLockedFileByRecordLabel], out result))
                    {
                        rmRule.IncludeDeleteRecordLabel = result;
                    }
                    else
                    {
                        rmRule.IncludeDeleteRecordLabel = false;
                    }
                }

                SetArchiverStorage(ruleInfo, rmRule);
            }

            if ((rmRule.ArchiverActions.Equals(Action_ArchiveContentActionString, StringComparison.OrdinalIgnoreCase) || rmRule.ArchiverActions.Equals(Action_RemoveDataActionString, StringComparison.OrdinalIgnoreCase))
                && readRemoveOption_IsEnableRemoveRetentionLabel
                && bool.TryParse(ruleInfo[RulePropertyIndex.IsEnableRemoveRetentionLabel + IndexChangeFromIncludeDeclaredRecord], out result) && result)
            {
                keepDataOption |= (int)KeepDataStatus.IsEnableRemoveRetentionLabel;
            }
            #endregion

            #region Keep Data - Record Declaration and Tagging
            if (rmRule.ArchiverActions.Equals(Action_KeepDataActionString, StringComparison.OrdinalIgnoreCase))
            {
                keepDataOption |= (int)KeepDataStatus.Keep;
            }
            //keep data 才读 DeclareRecord
            if (rmRule.ArchiverActions.Equals(Action_KeepDataActionString, StringComparison.OrdinalIgnoreCase) && readKeepOption_DeclareRecord)
            {
                if (bool.TryParse(ruleInfo[RulePropertyIndex.DeclareRecord + IndexChangeFromIncludeDeclaredRecord], out result))
                {
                    bool shouldDeclare = result 
                        && (CheckRuleSource(ruleSourceType, SourceType_SPLocal) || !(isSupportRecordLabel && (rmRule.RuleLevel == PolicyLevel.Document || rmRule.RuleLevel == PolicyLevel.Item)));

                    if (shouldDeclare)
                    {
                        keepDataOption |= (int)KeepDataStatus.DeclareRecord;
                    }
                }
                else
                {
                    //throw new Exception("RM_JS_TM_TermImport_NoDeclareRecord");
                }
            }

            //keep data 才读 Tag
            bool doTag = false;
            if (rmRule.ArchiverActions.Equals(Action_KeepDataActionString, StringComparison.OrdinalIgnoreCase) && readKeepOption_Tag
                && bool.TryParse(ruleInfo[RulePropertyIndex.DoTag + IndexChangeFromIncludeDeclaredRecord], out doTag) && doTag)
            {
                #region tag
                List<RMTagContentInfo> tagContentInfos = new List<RMTagContentInfo>();
                bool tagWithArchived;
                if (bool.TryParse(ruleInfo[RulePropertyIndex.TagWithArchived + IndexChangeFromIncludeDeclaredRecord], out tagWithArchived))
                {
                    if (tagWithArchived)
                    {
                        RMTagContentInfo tagArchived = new RMTagContentInfo();
                        tagArchived.Type = TagContentInfoType.Archived;
                        //TODO leon need check
                        tagArchived.DateTime = DateTime.UtcNow;

                        tagContentInfos.Add(tagArchived);
                    }
                }
                bool tagWithArchivedBy;
                if (bool.TryParse(ruleInfo[RulePropertyIndex.TagWithArchivedBy + IndexChangeFromIncludeDeclaredRecord], out tagWithArchivedBy))
                {
                    if (tagWithArchivedBy)
                    {
                        RMTagContentInfo tagArchivedBy = new RMTagContentInfo();
                        tagArchivedBy.Type = TagContentInfoType.ArchivedBy;
                        //TODO leon need check
                        tagArchivedBy.DateTime = DateTime.UtcNow;

                        tagContentInfos.Add(tagArchivedBy);
                    }
                }
                bool tagWithArchivedDate;
                if (bool.TryParse(ruleInfo[RulePropertyIndex.TagWithArchivedTime + IndexChangeFromIncludeDeclaredRecord], out tagWithArchivedDate))
                {
                    if (tagWithArchivedDate)
                    {
                        RMTagContentInfo tagArchivedDate = new RMTagContentInfo();
                        tagArchivedDate.Type = TagContentInfoType.ArchivedDate;
                        //TODO leon need check
                        tagArchivedDate.DateTime = DateTime.UtcNow;

                        tagContentInfos.Add(tagArchivedDate);
                    }
                }
                bool tagWithCustomColumn;
                if (bool.TryParse(ruleInfo[RulePropertyIndex.TagWithCustomColumn + IndexChangeFromIncludeDeclaredRecord], out tagWithCustomColumn))
                {
                    if (tagWithCustomColumn)
                    {
                        RMTagContentInfo tagCustomColumn = new RMTagContentInfo();
                        var cusColumnType = ruleInfo[RulePropertyIndex.CustomColumnType + IndexChangeFromIncludeDeclaredRecord];
                        var cusColumnName = ruleInfo[RulePropertyIndex.CustomColumnName + IndexChangeFromIncludeDeclaredRecord];
                        var cusColumnValue = ruleInfo[RulePropertyIndex.CustomColumnValue + IndexChangeFromIncludeDeclaredRecord];
                        var cusColumnTimeZone = ruleInfo[RulePropertyIndex.CustomColumnTimeZone + IndexChangeFromIncludeDeclaredRecord];
                        //自定义验证
                        await ValidateCustomColumnAsync(cusColumnType, cusColumnValue, cusColumnName, cusColumnTimeZone, tagCustomColumn);

                        tagContentInfos.Add(tagCustomColumn);
                    }
                }
                if (isSupportRecordLabel)
                {
                    if (bool.TryParse(ruleInfo[RulePropertyIndex.RetentionLabel], out var isCheckRetentionLabel) && isCheckRetentionLabel)
                    {
                        RMTagContentInfo tagLabel = new RMTagContentInfo();
                        tagLabel.Type = TagContentInfoType.RetentionLabel;
                        if (!string.IsNullOrEmpty(ruleInfo[RulePropertyIndex.Label + IndexChangeFromCustomColumnTimeZone]))
                        {
                            tagLabel.Value = ruleInfo[RulePropertyIndex.Label + IndexChangeFromCustomColumnTimeZone];
                            tagLabel.Option = (int)RetentionLabelOptions.Default;
                            tagContentInfos.Add(tagLabel);
                        }
                        else if (bool.TryParse(ruleInfo[RulePropertyIndex.RecordLabel], out result) && result)
                        {
                            tagLabel.Value = ruleInfo[RulePropertyIndex.RecordLabel];
                            tagLabel.Option = (int)RetentionLabelOptions.GetFromGeneralSetting;
                            tagContentInfos.Add(tagLabel);
                        }
                    }
                }
                else
                {
                    if (!string.IsNullOrEmpty(ruleInfo[RulePropertyIndex.Label + IndexChangeFromCustomColumnTimeZone]))
                    {
                        RMTagContentInfo tagLabel = new RMTagContentInfo();
                        tagLabel.DateTime = DateTime.UtcNow;
                        tagLabel.Type = TagContentInfoType.RetentionLabel;
                        //TODO leon need check
                        tagLabel.Value = ruleInfo[RulePropertyIndex.Label + IndexChangeFromCustomColumnTimeZone].ToString();
                        tagContentInfos.Add(tagLabel);
                    }
                }
                #endregion

                keepDataOption |= (int)KeepDataStatus.TagContent;
                rmRule.TagContentInfo = tagContentInfos;
                if (rmRule.TagContentInfo.Count == 0)
                {
                    throw new Exception("RM_JS_TM_TermImport_AtLeastHaveOneTagOption");
                }
            }
            else
            {
                if (rmRule.ArchiverActions.Equals(Action_KeepDataActionString, StringComparison.OrdinalIgnoreCase) && readKeepOption_Tag)
                {
                    if (isSupportRecordLabel && !CheckRuleSource(ruleSourceType, SourceType_SPLocal))
                    {
                        throw new Exception("RM_JS_RDM_CreateRule_Validation_ConditioNoTag");
                    }
                }

                rmRule.TagContentInfo = new List<RMTagContentInfo>();
            }

            //如果是KeepData, 则至少包含Declare和Tag里的一项
            if ((keepDataOption & (int)KeepDataStatus.Keep) == (int)KeepDataStatus.Keep && rmRule.IsExoSource != true)
            {
                if ((keepDataOption & (int)KeepDataStatus.DeclareRecord) != (int)KeepDataStatus.DeclareRecord
                    && (keepDataOption & (int)KeepDataStatus.TagContent) != (int)KeepDataStatus.TagContent)
                {
                    if (rmRule.RuleLevel == PolicyLevel.Folder)
                    {
                        throw new Exception("RM_JS_TM_TermImport_NoTag");
                    }
                    else
                    {
                        throw new Exception("RM_JS_TM_TermImport_NoDeclareNoTag");
                    }
                }
            }
            rmRule.RuleKeepDataOption = keepDataOption;
            //rmRule.DeclareLinkFile = (!string.IsNullOrWhiteSpace(ruleInfo[RulePropertyIndex.DeclareLinkFile])) && ruleInfo[RulePropertyIndex.DeclareLinkFile].Equals("true", StringComparison.OrdinalIgnoreCase) ? true : false;
            #endregion

            #region Move - Move documents to a new destination library
            string moveUrl = ruleInfo[RulePropertyIndex.MoveUrl + IndexChangeFromLabel];

            //Action是Move才读 Move设置
            if (rmRule.ArchiverActions.Equals(Action_MoveDataActionString, StringComparison.OrdinalIgnoreCase) && readMoveOption && !CheckRuleSource(ruleInfo[RulePropertyIndex.SourceType], Google_Drive))
            {
                var moveDto = new MoveToDto()
                {
                    IsSpecifyLocation = true,
                    LocationPath = moveUrl
                };

                ValidateMoveUrl(moveDto);
                moveDto.FileNameConflictOption = GetConflictOption(ruleInfo[RulePropertyIndex.ConflictResolution + IndexChangeFromLabel]);
                bool IsDeclareMoveDate = false;
                bool.TryParse(ruleInfo[RulePropertyIndex.DeclareAfterMove + IndexChangeFromLabel], out IsDeclareMoveDate);
                moveDto.NotDeclareMovedData = !IsDeclareMoveDate;
                bool IsKeepClassification = false;
                bool.TryParse(ruleInfo[RulePropertyIndex.KeepReclassifyAfterMove + IndexChangeFromLabel], out IsKeepClassification);
                moveDto.isKeepClassification = IsKeepClassification;
                rmRule.MoveDto = moveDto;
            }
            if (rmRule.ArchiverActions.Equals(Action_ExportOnlyActionString, StringComparison.OrdinalIgnoreCase))
            {
                rmRule.EnableExport = true;
                var exportInfo = new SOExportInfo();
                exportInfo.exportSPDataOption = ExportSPDataOption.ExportWithoutArchive;
                var exportFormat = ruleInfo[RulePropertyIndex.ExportOnlyFormat];
                if (string.IsNullOrEmpty(exportFormat) || string.IsNullOrEmpty(exportFormat.Trim()))
                {
                    throw new Exception("RM_JS_TM_TermImport_NoExportFormat");
                }
                exportInfo.exportType = (GCommon.Contract.StorageOptimization.Object.ExportTypeValue)Enum.Parse(typeof(GCommon.Contract.StorageOptimization.Object.ExportTypeValue), exportFormat);
                if ((exportInfo.exportType == GCommon.Contract.StorageOptimization.Object.ExportTypeValue.NAA || exportInfo.exportType == GCommon.Contract.StorageOptimization.Object.ExportTypeValue.NARA) && rmRule.RuleLevel != PolicyLevel.Document && rmRule.RuleLevel != PolicyLevel.Folder)
                {
                    throw new Exception("RM_JS_TM_TermImport_ExportOnlyNotSupport");
                }
                SetExportToDestinationLibrary(ruleInfo, rmRule, exportInfo);
                rmRule.ExportInfo = exportInfo;
            }
            #endregion
            #endregion
            SetExportSettings(ruleInfo, rmRule);
            await SetManunalSettingsAsync(ruleInfo, rmRule);
            mLog.Info($"BuildSPRuleAsync result: RuleName={rmRule.RuleName}, ArchiverActions={rmRule.ArchiverActions}, RuleKeepDataOption={rmRule.RuleKeepDataOption}");
            return rmRule;
        }

        private async Task<RMRuleInfos> BuildEXORuleAsync(string[] ruleInfo)
        {
            var rmRule = new RMRuleInfos
            {
                RuleFilters = new List<RuleFilter>()
            };
            rmRule.RuleLevel = PolicyLevel.ExchangeOnlineItem;
            rmRule.ArchiverActions = ruleInfo[RulePropertyIndex.RuleAction];
            if (rmRule.ArchiverActions.Equals(Action_RemoveDataActionString, StringComparison.OrdinalIgnoreCase))
            {
                rmRule.RuleKeepDataOption = (int)KeepDataStatus.Delete;
            }
            if (rmRule.ArchiverActions.Equals(Action_KeepDataActionString, StringComparison.OrdinalIgnoreCase))
            {
                rmRule.RuleKeepDataOption = (int)KeepDataStatus.Keep | (int)KeepDataStatus.TagContent;
                if (string.IsNullOrEmpty(ruleInfo[RulePropertyIndex.Label + IndexChangeFromCustomColumnTimeZone]) || string.IsNullOrEmpty(ruleInfo[RulePropertyIndex.Label + IndexChangeFromCustomColumnTimeZone].Trim()))
                {
                    throw new Exception("RM_JS_TM_TermImport_NoLabel");
                }
                List<RMTagContentInfo> tagContentInfos = new List<RMTagContentInfo>();
                RMTagContentInfo tagLabel = new RMTagContentInfo();
                tagLabel.DateTime = DateTime.UtcNow;
                tagLabel.Type = TagContentInfoType.RetentionLabel;
                //TODO leon need check
                tagLabel.Value = ruleInfo[RulePropertyIndex.Label + IndexChangeFromCustomColumnTimeZone];
                tagContentInfos.Add(tagLabel);
                rmRule.TagContentInfo = tagContentInfos;
            }
            if (rmRule.ArchiverActions.Equals(Action_MoveDataActionString, StringComparison.OrdinalIgnoreCase))
            {
                string moveUrl = ruleInfo[RulePropertyIndex.MoveUrl + IndexChangeFromLabel];
                var moveDto = new MoveToDto()
                {
                    IsSpecifyLocation = true,
                    LocationPath = moveUrl
                };
                ValidateMoveUrl(moveDto);
                moveDto.FileNameConflictOption = GetConflictOption(ruleInfo[RulePropertyIndex.ConflictResolution + IndexChangeFromLabel]);
                bool IsRemoveSource = false;
                bool.TryParse(ruleInfo[RulePropertyIndex.RemoveSourceAfterMove + IndexChangeFromLabel], out IsRemoveSource);
                moveDto.IsDeleteSourceItem = IsRemoveSource;
                bool IsKeepClassification = false;
                bool.TryParse(ruleInfo[RulePropertyIndex.KeepReclassifyAfterMove + IndexChangeFromLabel], out IsKeepClassification);
                moveDto.isKeepClassification = IsKeepClassification;
                rmRule.MoveDto = moveDto;
            }
            if (rmRule.ArchiverActions.Equals(Action_ExportOnlyActionString, StringComparison.OrdinalIgnoreCase))
            {
                rmRule.EnableExport = true;
                var exportInfo = new SOExportInfo();
                exportInfo.exportSPDataOption = ExportSPDataOption.ExportWithoutArchive;
                var exportFormat = ruleInfo[RulePropertyIndex.ExportOnlyFormat];
                if (string.IsNullOrEmpty(exportFormat) || string.IsNullOrEmpty(exportFormat.Trim()))
                {
                    throw new Exception("RM_JS_TM_TermImport_NoExportFormat");
                }
                exportInfo.exportType = (AvePoint.GCommon.Contract.StorageOptimization.Object.ExportTypeValue)Enum.Parse(typeof(AvePoint.GCommon.Contract.StorageOptimization.Object.ExportTypeValue), exportFormat);
                SetExportToDestinationLibrary(ruleInfo, rmRule, exportInfo);
                rmRule.ExportInfo = exportInfo;
            }
            if (rmRule.ArchiverActions.Equals(Action_ArchiveContentActionString, StringComparison.OrdinalIgnoreCase))
            {
                throw new Exception("RM_JS_TM_TermImport_RuleLevelCannotArchiveContent");
            }
            if (string.IsNullOrEmpty(ruleInfo[RulePropertyIndex.RuleLevel]) || string.IsNullOrEmpty(ruleInfo[RulePropertyIndex.RuleLevel].Trim()))
            {
                throw new Exception("RM_JS_TM_TermImport_NoObjectLevel");
            }
            if (ruleInfo[RulePropertyIndex.RuleLevel].Equals("Document/Email"))
            {
                ruleInfo[RulePropertyIndex.RuleLevel] = "Document";
            }
            var excelRuleLevel = (PolicyLevel)Enum.Parse(typeof(PolicyLevel), ruleInfo[RulePropertyIndex.RuleLevel]);
            if (excelRuleLevel != PolicyLevel.Document)
            {   //如果是包含Exo的rule 则objectlevel只能有Document
                throw new Exception("RM_JS_TM_TermImport_ObjectIsOnlyDoc");
            }
            SetExportSettings(ruleInfo, rmRule);
            await SetManunalSettingsAsync(ruleInfo, rmRule);
            return rmRule;
        }

        private async Task<RMRuleInfos> BuildPhysicalRuleAsync(string[] ruleInfo)
        {
            var rmRule = new RMRuleInfos
            {
                RuleFilters = new List<RuleFilter>()
            };
            if (string.IsNullOrEmpty(ruleInfo[RulePropertyIndex.RuleLevel]) || string.IsNullOrEmpty(ruleInfo[RulePropertyIndex.RuleLevel].Trim()))
            {
                throw new Exception("RM_JS_TM_TermImport_NoObjectLevel");
            }
            if (ruleInfo[RulePropertyIndex.RuleLevel].Equals("List / Library / Physical box") || ruleInfo[RulePropertyIndex.RuleLevel].Equals("List"))
            {
                rmRule.RuleLevel = PolicyLevel.PhysicalBox;
            }
            if (ruleInfo[RulePropertyIndex.RuleLevel].Equals("Folder / Physical folder") || ruleInfo[RulePropertyIndex.RuleLevel].Equals("Folder"))
            {
                rmRule.RuleLevel = PolicyLevel.PhysicalFile;
            }
            if (!ruleInfo[RulePropertyIndex.RuleLevel].Equals("List / Library / Physical box") && !ruleInfo[RulePropertyIndex.RuleLevel].Equals("List") && !ruleInfo[RulePropertyIndex.RuleLevel].Equals("Folder / Physical folder") && !ruleInfo[RulePropertyIndex.RuleLevel].Equals("Folder"))
            {
                throw new Exception("RM_JS_TM_TermImport_PhyOnlyLevel");
            }
            rmRule.ArchiverActions = ruleInfo[RulePropertyIndex.RuleAction];
            if (rmRule.ArchiverActions.Equals(Action_CalculateDisposalDate, StringComparison.OrdinalIgnoreCase))
            {
                rmRule.IsCalculationDisposalDate = true;
            }
            if (rmRule.ArchiverActions.Equals(Action_RemoveDataActionString, StringComparison.OrdinalIgnoreCase))
            {

                rmRule.RuleKeepDataOption = (int)KeepDataStatus.Delete;
                bool result = false;
                if (rmRule.RuleLevel != PolicyLevel.PhysicalBox && bool.TryParse(ruleInfo[RulePropertyIndex.IncludeRelatedRecord], out result))
                {
                    if (result)
                    {
                        rmRule.RelatedRecordOption = AvePoint.GCommon.Contract.StorageOptimization.Object.RelatedRecordOption.Both;
                        if (!string.IsNullOrWhiteSpace(ruleInfo[RulePropertyIndex.ArchiveDataStorage + IndexChangeFromLabel]))
                        {
                            SetArchiverStorage(ruleInfo, rmRule);
                        }
                    }
                }
                if (rmRule.RuleLevel != PolicyLevel.PhysicalBox && bool.TryParse(ruleInfo[RulePropertyIndex.RemoveBox + IndexChangeFromIncludeDeclaredRecord], out result))
                {
                    if (result)
                    {
                        rmRule.DestroyEmptyBoxOnFolderRule = true;
                    }
                }
            }
            await SetManunalSettingsAsync(ruleInfo, rmRule);
            if (rmRule.ArchiverActions.Equals(Action_KeepDataActionString, StringComparison.OrdinalIgnoreCase))
            {
                throw new Exception("RM_JS_TM_TermImport_PhyCannotKeepContent");
            }
            if (rmRule.ArchiverActions.Equals(Action_ExportOnlyActionString, StringComparison.OrdinalIgnoreCase))
            {
                throw new Exception("RM_JS_TM_TermImport_PhyCannotExportOnly");
            }
            if (rmRule.ArchiverActions.Equals(Action_MoveDataActionString, StringComparison.OrdinalIgnoreCase))
            {
                throw new Exception("RM_JS_TM_TermImport_PhyCannotMove");
            }
            if (rmRule.ArchiverActions.Equals(Action_ArchiveContentActionString, StringComparison.OrdinalIgnoreCase))
            {
                throw new Exception("RM_JS_TM_TermImport_RuleLevelCannotArchiveContent");
            }
            return rmRule;
        }

        private async Task<RMRuleInfos> BuildFSRuleAsync(string[] ruleInfo)
        {
            var rmRule = new RMRuleInfos
            {
                RuleFilters = new List<RuleFilter>()
            };
            if (string.IsNullOrEmpty(ruleInfo[RulePropertyIndex.RuleLevel]) || string.IsNullOrEmpty(ruleInfo[RulePropertyIndex.RuleLevel].Trim()))
            {
                throw new Exception("RM_JS_TM_TermImport_NoObjectLevel");
            }
            if (!ruleInfo[RulePropertyIndex.RuleLevel].Equals("Document") && !ruleInfo[RulePropertyIndex.RuleLevel].Equals("Document/Email"))
            {
                throw new Exception("RM_JS_TM_TermImport_FSOnlyLevel");
            }
            rmRule.RuleLevel = PolicyLevel.FileSysFile;
            rmRule.ArchiverActions = ruleInfo[RulePropertyIndex.RuleAction];
            if (rmRule.ArchiverActions.Equals(Action_KeepDataActionString, StringComparison.OrdinalIgnoreCase))
            {
                throw new Exception("RM_JS_TM_TermImport_FSCannotKeepContent");
            }
            if (rmRule.ArchiverActions.Equals(Action_ExportOnlyActionString, StringComparison.OrdinalIgnoreCase))
            {
                throw new Exception("RM_JS_TM_TermImport_FSCannotExportOnly");
            }
            if (rmRule.ArchiverActions.Equals(Action_ArchiveContentActionString, StringComparison.OrdinalIgnoreCase))
            {
                rmRule.RuleKeepDataOption = (int)KeepDataStatus.Archive;
                var storageName = ruleInfo[RulePropertyIndex.ArchiveDataStorage + IndexChangeFromLabel];
                if (!string.IsNullOrWhiteSpace(storageName) && DEFAULTSTORAGENAME.Equals(storageName, StringComparison.OrdinalIgnoreCase))
                {
                    throw new Exception("RM_JS_TM_TermImport_FSCannotUseDefaultStorage");
                }
                SetArchiverStorage(ruleInfo, rmRule);
            }
            if (rmRule.ArchiverActions.Equals(Action_RemoveDataActionString, StringComparison.OrdinalIgnoreCase))
            {
                rmRule.RuleKeepDataOption = (int)KeepDataStatus.Delete;
                bool result = false;
                if (rmRule.ArchiverActions.Equals(Action_RemoveDataActionString, StringComparison.OrdinalIgnoreCase)
                    && bool.TryParse(ruleInfo[RulePropertyIndex.LeaveStub + IndexChangeFromIncludeDeclaredRecord], out result))
                {
                    if (result)
                    {
                        await SetStubTemplateAsync(ruleInfo, rmRule);
                        rmRule.RuleKeepDataOption |= (int)KeepDataStatus.LinkToDocument;
                    }
                }
            }
            if (rmRule.ArchiverActions.Equals(Action_MoveDataActionString, StringComparison.OrdinalIgnoreCase))
            {
                string moveUrl = ruleInfo[RulePropertyIndex.MoveUrl + IndexChangeFromLabel];
                var moveDto = new MoveToDto()
                {
                    IsSpecifyLocation = true,
                    LocationPath = moveUrl
                };

                ValidateMoveUrl(moveDto);
                moveDto.FileNameConflictOption = GetConflictOption(ruleInfo[RulePropertyIndex.ConflictResolution + IndexChangeFromLabel]);
                rmRule.MoveDto = moveDto;
                var connection = FSConnectionDao.GetParentConnectionInfoForImport(moveDto.LocationPath);
                if (connection == null)
                {
                    throw new Exception("RM_JS_TM_TermImport_FSDesNotConfig");
                }
                //如果是move 且EnableMannual有值  应该给出错误提示 还是跳过。
            }
            await SetManunalSettingsAsync(ruleInfo, rmRule);
            return rmRule;
        }

        private async Task<RMRuleInfos> BuildBoxRuleAsync(string[] ruleInfo)
        {
            var rmRule = new RMRuleInfos
            {
                RuleFilters = new List<RuleFilter>()
            };
            if (string.IsNullOrEmpty(ruleInfo[RulePropertyIndex.RuleLevel]) || string.IsNullOrEmpty(ruleInfo[RulePropertyIndex.RuleLevel].Trim()))
            {
                throw new Exception("RM_JS_TM_TermImport_NoObjectLevel");
            }
            if (!ruleInfo[RulePropertyIndex.RuleLevel].Equals("Document") && !ruleInfo[RulePropertyIndex.RuleLevel].Equals("Document/Email"))
            {
                throw new Exception("RM_JS_TM_TermImport_BoxOnlyLevel");
            }
            rmRule.RuleLevel = PolicyLevel.BoxDocument;
            rmRule.ArchiverActions = ruleInfo[RulePropertyIndex.RuleAction];
            if (rmRule.ArchiverActions.Equals(Action_KeepDataActionString, StringComparison.OrdinalIgnoreCase))
            {
                throw new Exception("RM_JS_TM_TermImport_BoxCannotKeepContent");
            }
            if (rmRule.ArchiverActions.Equals(Action_ExportOnlyActionString, StringComparison.OrdinalIgnoreCase))
            {
                throw new Exception("RM_JS_TM_TermImport_BoxCannotExportOnly");
            }
            if (rmRule.ArchiverActions.Equals(Action_ArchiveContentActionString, StringComparison.OrdinalIgnoreCase))
            {
                throw new Exception("RM_JS_TM_TermImport_RuleLevelCannotArchiveContent");
            }
            if (rmRule.ArchiverActions.Equals(Action_MoveDataActionString, StringComparison.OrdinalIgnoreCase))
            {
                throw new Exception("RM_JS_TM_TermImport_ActionCannotBeMove");
            }
            if (rmRule.ArchiverActions.Equals(Action_RemoveDataActionString, StringComparison.OrdinalIgnoreCase))
            {
                rmRule.RuleKeepDataOption = (int)KeepDataStatus.Delete;
                bool result = false;
                if (rmRule.ArchiverActions.Equals(Action_RemoveDataActionString, StringComparison.OrdinalIgnoreCase)
                    && bool.TryParse(ruleInfo[RulePropertyIndex.LeaveStub + IndexChangeFromIncludeDeclaredRecord], out result))
                {
                    if (result)
                    {
                        await SetStubTemplateAsync(ruleInfo, rmRule);
                        rmRule.RuleKeepDataOption |= (int)KeepDataStatus.LinkToDocument;
                    }
                }
            }
            if (rmRule.ArchiverActions.Equals(Action_MoveDataActionString, StringComparison.OrdinalIgnoreCase))
            {
                string moveUrl = ruleInfo[RulePropertyIndex.MoveUrl + IndexChangeFromLabel];
                var moveDto = new MoveToDto()
                {
                    IsSpecifyLocation = true,
                    LocationPath = moveUrl
                };

                ValidateMoveUrl(moveDto);
                moveDto.FileNameConflictOption = GetConflictOption(ruleInfo[RulePropertyIndex.ConflictResolution + IndexChangeFromLabel]);
                rmRule.MoveDto = moveDto;
                var connection = FSConnectionDao.GetParentConnectionInfoForImport(moveDto.LocationPath);
                if (connection == null)
                {
                    throw new Exception("RM_JS_TM_TermImport_FSDesNotConfig");
                }
            }
            await SetManunalSettingsAsync(ruleInfo, rmRule);
            return rmRule;
        }

        private async Task<RMRuleInfos> BuildAzureFilerRuleAsync(string[] ruleInfo)
        {
            var rmRule = new RMRuleInfos
            {
                RuleFilters = new List<RuleFilter>()
            };
            if (string.IsNullOrEmpty(ruleInfo[RulePropertyIndex.RuleLevel]) || string.IsNullOrEmpty(ruleInfo[RulePropertyIndex.RuleLevel].Trim()))
            {
                throw new Exception("RM_JS_TM_TermImport_NoObjectLevel");
            }
            if (!ruleInfo[RulePropertyIndex.RuleLevel].Equals("Document") && !ruleInfo[RulePropertyIndex.RuleLevel].Equals("Document/Email"))
            {
                throw new Exception("RM_JS_TM_TermImport_AzureFileOnlyLevel");
            }
            rmRule.RuleLevel = PolicyLevel.AzureFileDocument;
            rmRule.ArchiverActions = ruleInfo[RulePropertyIndex.RuleAction];
            if (rmRule.ArchiverActions.Equals(Action_KeepDataActionString, StringComparison.OrdinalIgnoreCase))
            {
                throw new Exception("RM_JS_TM_TermImport_AzureFileCannotKeepContent");
            }
            if (rmRule.ArchiverActions.Equals(Action_ExportOnlyActionString, StringComparison.OrdinalIgnoreCase))
            {
                throw new Exception("RM_JS_TM_TermImport_AzureFileCannotExportOnly");
            }
            if (rmRule.ArchiverActions.Equals(Action_ArchiveContentActionString, StringComparison.OrdinalIgnoreCase))
            {
                throw new Exception("RM_JS_TM_TermImport_RuleLevelCannotArchiveContent");
            }
            if (rmRule.ArchiverActions.Equals(Action_MoveDataActionString, StringComparison.OrdinalIgnoreCase))
            {
                throw new Exception("RM_JS_TM_TermImport_ActionCannotBeMove");
            }
            if (rmRule.ArchiverActions.Equals(Action_RemoveDataActionString, StringComparison.OrdinalIgnoreCase))
            {
                rmRule.RuleKeepDataOption = (int)KeepDataStatus.Delete;
                bool result = false;
                if (rmRule.ArchiverActions.Equals(Action_RemoveDataActionString, StringComparison.OrdinalIgnoreCase)
                    && bool.TryParse(ruleInfo[RulePropertyIndex.LeaveStub + IndexChangeFromIncludeDeclaredRecord], out result))
                {
                    if (result)
                    {
                        await SetStubTemplateAsync(ruleInfo, rmRule);
                        rmRule.RuleKeepDataOption |= (int)KeepDataStatus.LinkToDocument;
                    }
                }
            }
            if (rmRule.ArchiverActions.Equals(Action_MoveDataActionString, StringComparison.OrdinalIgnoreCase))
            {
                string moveUrl = ruleInfo[RulePropertyIndex.MoveUrl + IndexChangeFromLabel];
                var moveDto = new MoveToDto()
                {
                    IsSpecifyLocation = true,
                    LocationPath = moveUrl
                };

                ValidateMoveUrl(moveDto);
                moveDto.FileNameConflictOption = GetConflictOption(ruleInfo[RulePropertyIndex.ConflictResolution + IndexChangeFromLabel]);
                rmRule.MoveDto = moveDto;
                var connection = FSConnectionDao.GetParentConnectionInfoForImport(moveDto.LocationPath);
                if (connection == null)
                {
                    throw new Exception("RM_JS_TM_TermImport_FSDesNotConfig");
                }
                //如果是move 且EnableMannual有值  应该给出错误提示 还是跳过。
            }
            await SetManunalSettingsAsync(ruleInfo, rmRule);
            return rmRule;
        }

        private async Task<RMRuleInfos> BuildConnectorRuleAsync(string[] ruleInfo)
        {
            var rmRule = new RMRuleInfos
            {
                RuleFilters = new List<RuleFilter>()
            };
            if (string.IsNullOrEmpty(ruleInfo[RulePropertyIndex.RuleLevel]) || string.IsNullOrEmpty(ruleInfo[RulePropertyIndex.RuleLevel].Trim()))
            {
                throw new Exception("RM_JS_TM_TermImport_NoObjectLevel");
            }
            if (!ruleInfo[RulePropertyIndex.RuleLevel].Equals("Document") && !ruleInfo[RulePropertyIndex.RuleLevel].Equals("Document/Email"))
            {
                throw new Exception("RM_JS_TM_TermImport_AzureFileOnlyLevel");
            }
            rmRule.RuleLevel = PolicyLevel.Document;
            rmRule.ArchiverActions = ruleInfo[RulePropertyIndex.RuleAction];
            if (rmRule.ArchiverActions.Equals(Action_KeepDataActionString, StringComparison.OrdinalIgnoreCase))
            {
                throw new Exception("RM_JS_TM_TermImport_AzureFileCannotKeepContent");
            }
            if (rmRule.ArchiverActions.Equals(Action_ExportOnlyActionString, StringComparison.OrdinalIgnoreCase))
            {
                throw new Exception("RM_JS_TM_TermImport_AzureFileCannotExportOnly");
            }
            if (rmRule.ArchiverActions.Equals(Action_ArchiveContentActionString, StringComparison.OrdinalIgnoreCase))
            {
                throw new Exception("RM_JS_TM_TermImport_RuleLevelCannotArchiveContent");
            }
            if (rmRule.ArchiverActions.Equals(Action_MoveDataActionString, StringComparison.OrdinalIgnoreCase))
            {
                throw new Exception("RM_JS_TM_TermImport_ActionCannotBeMove");
            }
            if (rmRule.ArchiverActions.Equals(Action_RemoveDataActionString, StringComparison.OrdinalIgnoreCase))
            {
                rmRule.RuleKeepDataOption = (int)KeepDataStatus.Delete;
                bool result = false;
                if (rmRule.ArchiverActions.Equals(Action_RemoveDataActionString, StringComparison.OrdinalIgnoreCase)
                    && bool.TryParse(ruleInfo[RulePropertyIndex.LeaveStub + IndexChangeFromIncludeDeclaredRecord], out result))
                {
                    if (result)
                    {
                        await SetStubTemplateAsync(ruleInfo, rmRule);
                        rmRule.RuleKeepDataOption |= (int)KeepDataStatus.LinkToDocument;
                    }
                }
            }
            if (rmRule.ArchiverActions.Equals(Action_MoveDataActionString, StringComparison.OrdinalIgnoreCase))
            {
                string moveUrl = ruleInfo[RulePropertyIndex.MoveUrl + IndexChangeFromLabel];
                var moveDto = new MoveToDto()
                {
                    IsSpecifyLocation = true,
                    LocationPath = moveUrl
                };

                ValidateMoveUrl(moveDto);
                moveDto.FileNameConflictOption = GetConflictOption(ruleInfo[RulePropertyIndex.ConflictResolution + IndexChangeFromLabel]);
                rmRule.MoveDto = moveDto;
                var connection = FSConnectionDao.GetParentConnectionInfoForImport(moveDto.LocationPath);
                if (connection == null)
                {
                    throw new Exception("RM_JS_TM_TermImport_FSDesNotConfig");
                }
                //如果是move 且EnableMannual有值  应该给出错误提示 还是跳过。
            }
            await SetManunalSettingsAsync(ruleInfo, rmRule);
            return rmRule;
        }

        private async Task<RMRuleInfos> BuildSPLocalRuleAsync(string[] ruleInfo)
        {
            var rmRule = new RMRuleInfos
            {
                RuleFilters = new List<RuleFilter>()
            };
            if (string.IsNullOrEmpty(ruleInfo[RulePropertyIndex.RuleLevel]) || string.IsNullOrEmpty(ruleInfo[RulePropertyIndex.RuleLevel].Trim()))
            {
                throw new Exception("RM_JS_TM_TermImport_NoObjectLevel");
            }
            if (!ruleInfo[RulePropertyIndex.RuleLevel].Equals("Document/Email") && !ruleInfo[RulePropertyIndex.RuleLevel].Equals("Document") && !ruleInfo[RulePropertyIndex.RuleLevel].Equals("Item"))
            {
                throw new Exception("RM_JS_TM_TermImport_SPLocalOnlyLevel");
            }
            if (ruleInfo[RulePropertyIndex.RuleLevel].Equals("Document/Email") || ruleInfo[RulePropertyIndex.RuleLevel].Equals("Document"))
            {
                rmRule.RuleLevel = PolicyLevel.Document;
            }
            if (ruleInfo[RulePropertyIndex.RuleLevel].Equals("Item"))
            {
                rmRule.RuleLevel = PolicyLevel.Item;
            }

            rmRule.ArchiverActions = ruleInfo[RulePropertyIndex.RuleAction];
            if (rmRule.ArchiverActions.Equals(Action_ExportOnlyActionString, StringComparison.OrdinalIgnoreCase))
            {
                throw new Exception("RM_JS_TM_TermImport_SPLocalCannotExportOnly");
            }
            if (rmRule.ArchiverActions.Equals(Action_MoveDataActionString, StringComparison.OrdinalIgnoreCase))
            {
                throw new Exception("RM_JS_TM_TermImport_ActionCannotBeMove");
            }
            if (rmRule.ArchiverActions.Equals(Action_ArchiveContentActionString, StringComparison.OrdinalIgnoreCase))
            {
                throw new Exception("RM_JS_TM_TermImport_RuleLevelCannotArchiveContent");
            }
            bool readRemoveOption_LeaveStub = false;
            bool readKeepOption_DeclareRecord = false;
            bool readKeepOption_Tag = false;
            bool readRemoveOption_IncludeRelatedRecords = false;
            if (rmRule.RuleLevel == PolicyLevel.Document)
            {
                readRemoveOption_IncludeRelatedRecords = true;
                readRemoveOption_LeaveStub = true;
                readKeepOption_DeclareRecord = true;
                readKeepOption_Tag = true;
            }
            if (rmRule.RuleLevel == PolicyLevel.Item)
            {
                
                readRemoveOption_LeaveStub = false;
                readKeepOption_DeclareRecord = true;
                readKeepOption_Tag = true;
            }
            bool result = false;
            #region Remove Data - Remove content from SharePoint and destroy
            //remove data 才读IncludeDeclaredRecord
            if (rmRule.ArchiverActions.Equals(Action_RemoveDataActionString, StringComparison.OrdinalIgnoreCase))
            {
                if (readRemoveOption_IncludeRelatedRecords && bool.TryParse(ruleInfo[RulePropertyIndex.IncludeRelatedRecord], out result))
                {
                    if (result)
                    {
                        rmRule.RelatedRecordOption = AvePoint.GCommon.Contract.StorageOptimization.Object.RelatedRecordOption.Both;
                    }
                }

                if (bool.TryParse(ruleInfo[RulePropertyIndex.IncludeDeclaredRecord], out result))
                {
                    rmRule.DeleteRecords = result;
                }

                if (isSupportRecordLabel)
                {
                    if (bool.TryParse(ruleInfo[RulePropertyIndex.IncludeLockedFileByRecordLabel], out result))
                    {
                        rmRule.IncludeDeleteRecordLabel = result;
                    }
                    else
                    {
                        rmRule.IncludeDeleteRecordLabel = false;
                    }
                }
            }

            int keepDataOption = (int)KeepDataStatus.Delete;
            //remove data 才读LeaveStub
            if (rmRule.ArchiverActions.Equals(Action_RemoveDataActionString, StringComparison.OrdinalIgnoreCase)
                && readRemoveOption_LeaveStub
                && bool.TryParse(ruleInfo[RulePropertyIndex.LeaveStub + IndexChangeFromIncludeDeclaredRecord], out result))
            {
                if (result)
                {
                    await SetStubTemplateAsync(ruleInfo, rmRule);
                    keepDataOption |= (int)KeepDataStatus.LinkToDocument;
                }
            }

            if (rmRule.ArchiverActions.Equals(Action_RemoveDataActionString, StringComparison.OrdinalIgnoreCase))
            {
                keepDataOption |= (int)KeepDataStatus.NotBackup;
            }
            #endregion

            #region Keep Data - Record Declaration and Tagging
            if (rmRule.ArchiverActions.Equals(Action_KeepDataActionString, StringComparison.OrdinalIgnoreCase))
            {
                keepDataOption |= (int)KeepDataStatus.Keep;
            }
            //keep data 才读 DeclareRecord
            if (rmRule.ArchiverActions.Equals(Action_KeepDataActionString, StringComparison.OrdinalIgnoreCase) && readKeepOption_DeclareRecord)
            {
                if (bool.TryParse(ruleInfo[RulePropertyIndex.DeclareRecord + IndexChangeFromIncludeDeclaredRecord], out result))
                {
                    if (result)
                    {
                        keepDataOption |= (int)KeepDataStatus.DeclareRecord;
                    }
                }
            }

            //keep data 才读 Tag
            bool doTag = false;
            if (rmRule.ArchiverActions.Equals(Action_KeepDataActionString, StringComparison.OrdinalIgnoreCase) && readKeepOption_Tag
                && bool.TryParse(ruleInfo[RulePropertyIndex.DoTag + IndexChangeFromIncludeDeclaredRecord], out doTag) && doTag)
            {
                #region tag
                List<RMTagContentInfo> tagContentInfos = new List<RMTagContentInfo>();
                bool tagWithArchived;
                if (bool.TryParse(ruleInfo[RulePropertyIndex.TagWithArchived + IndexChangeFromIncludeDeclaredRecord], out tagWithArchived))
                {
                    if (tagWithArchived)
                    {
                        RMTagContentInfo tagArchived = new RMTagContentInfo();
                        tagArchived.Type = TagContentInfoType.Archived;
                        //TODO leon need check
                        tagArchived.DateTime = DateTime.UtcNow;

                        tagContentInfos.Add(tagArchived);
                    }
                }
                bool tagWithArchivedBy;
                if (bool.TryParse(ruleInfo[RulePropertyIndex.TagWithArchivedBy + IndexChangeFromIncludeDeclaredRecord], out tagWithArchivedBy))
                {
                    if (tagWithArchivedBy)
                    {
                        RMTagContentInfo tagArchivedBy = new RMTagContentInfo();
                        tagArchivedBy.Type = TagContentInfoType.ArchivedBy;
                        //TODO leon need check
                        tagArchivedBy.DateTime = DateTime.UtcNow;

                        tagContentInfos.Add(tagArchivedBy);
                    }
                }
                bool tagWithArchivedDate;
                if (bool.TryParse(ruleInfo[RulePropertyIndex.TagWithArchivedTime + IndexChangeFromIncludeDeclaredRecord], out tagWithArchivedDate))
                {
                    if (tagWithArchivedDate)
                    {
                        RMTagContentInfo tagArchivedDate = new RMTagContentInfo();
                        tagArchivedDate.Type = TagContentInfoType.ArchivedDate;
                        //TODO leon need check
                        tagArchivedDate.DateTime = DateTime.UtcNow;

                        tagContentInfos.Add(tagArchivedDate);
                    }
                }
                bool tagWithCustomColumn;
                if (bool.TryParse(ruleInfo[RulePropertyIndex.TagWithCustomColumn + IndexChangeFromIncludeDeclaredRecord], out tagWithCustomColumn))
                {
                    if (tagWithCustomColumn)
                    {
                        RMTagContentInfo tagCustomColumn = new RMTagContentInfo();
                        var cusColumnType = ruleInfo[RulePropertyIndex.CustomColumnType + IndexChangeFromIncludeDeclaredRecord];
                        var cusColumnName = ruleInfo[RulePropertyIndex.CustomColumnName + IndexChangeFromIncludeDeclaredRecord];
                        var cusColumnValue = ruleInfo[RulePropertyIndex.CustomColumnValue + IndexChangeFromIncludeDeclaredRecord];
                        var cusColumnTimeZone = ruleInfo[RulePropertyIndex.CustomColumnTimeZone + IndexChangeFromIncludeDeclaredRecord];
                        //自定义验证
                        await ValidateCustomColumnAsync(cusColumnType, cusColumnValue, cusColumnName, cusColumnTimeZone, tagCustomColumn);

                        tagContentInfos.Add(tagCustomColumn);
                    }
                }
                #endregion

                keepDataOption |= (int)KeepDataStatus.TagContent;
                rmRule.TagContentInfo = tagContentInfos;
                if (rmRule.TagContentInfo.Count == 0)
                {
                    throw new Exception("RM_JS_TM_TermImport_AtLeastHaveOneTagOption");
                }
            }
            else
            {
                rmRule.TagContentInfo = new List<RMTagContentInfo>();
            }
            //如果是KeepData, 则至少包含Declare和Tag里的一项
            if ((keepDataOption & (int)KeepDataStatus.Keep) == (int)KeepDataStatus.Keep)
            {
                if ((keepDataOption & (int)KeepDataStatus.DeclareRecord) != (int)KeepDataStatus.DeclareRecord
                    && (keepDataOption & (int)KeepDataStatus.TagContent) != (int)KeepDataStatus.TagContent)
                {
                    if (rmRule.RuleLevel == PolicyLevel.Folder)
                    {
                        throw new Exception("RM_JS_TM_TermImport_NoTag");
                    }
                    else
                    {
                        throw new Exception("RM_JS_TM_TermImport_NoDeclareNoTag");
                    }
                }
            }
            rmRule.RuleKeepDataOption = keepDataOption;
            #endregion
            await SetManunalSettingsAsync(ruleInfo, rmRule);
            return rmRule;
        }

        private async Task<RMRuleInfos> BuildOneDriveRuleAsync(string[] ruleInfo)
        {
            var rmRule = new RMRuleInfos
            {
                RuleFilters = new List<RuleFilter>()
            };

            if (string.IsNullOrEmpty(ruleInfo[RulePropertyIndex.RuleLevel]) || string.IsNullOrEmpty(ruleInfo[RulePropertyIndex.RuleLevel].Trim()))
            {
                throw new Exception("RM_JS_TM_TermImport_NoObjectLevel");
            }
            if (!ruleInfo[RulePropertyIndex.RuleLevel].Equals("Document/Email") && !ruleInfo[RulePropertyIndex.RuleLevel].Equals("Document"))
            {
                throw new Exception("RM_JS_TM_TermImport_OneDriveOnlyLevel");
            }
            rmRule.RuleLevel = PolicyLevel.Document;
            rmRule.ArchiverActions = ruleInfo[RulePropertyIndex.RuleAction];

            bool readRemoveOption_LeaveStub = false;
            bool readRemoveOption_IncludeRelatedRecords = false;
            bool readRemoveOption_IsEnableRemoveRetentionLabel = false;
            bool readMoveOption = false;
            bool readKeepOption_DeclareRecord = false;
            bool readKeepOption_Tag = false;
            //bool readKeepOption_ArchiveEachDocument = false;
            if (rmRule.RuleLevel == PolicyLevel.Document)
            {
                readRemoveOption_LeaveStub = true;
                readRemoveOption_IncludeRelatedRecords = true;
                readMoveOption = true;
                readKeepOption_DeclareRecord = true;
                readKeepOption_Tag = true;
                readRemoveOption_IsEnableRemoveRetentionLabel = true;
            }

            bool result = false;
            #region Remove Data - Remove content from SharePoint and destroy
            //remove data 才读IncludeRelatedRecord和IncludeDeclaredRecord
            if (rmRule.ArchiverActions.Equals(Action_RemoveDataActionString, StringComparison.OrdinalIgnoreCase))
            {
                //if (readRemoveOption_IncludeRelatedRecords && bool.TryParse(ruleInfo[RulePropertyIndex.IncludeRelatedRecord], out result))
                //{
                //    if (result)
                //    {
                //        rmRule.RelatedRecordOption = AvePoint.GCommon.Contract.StorageOptimization.Object.RelatedRecordOption.Both;
                //    }
                //}

                if (bool.TryParse(ruleInfo[RulePropertyIndex.IncludeDeclaredRecord], out result))
                {
                    rmRule.DeleteRecords = result;
                }
                if (bool.TryParse(ruleInfo[RulePropertyIndex.DeleteToRecycleBin + IndexChangeFromLabel], out result))
                {
                    rmRule.DeleteToRecycleBin = result;
                }

                if (isSupportRecordLabel)
                {
                    if (bool.TryParse(ruleInfo[RulePropertyIndex.IncludeLockedFileByRecordLabel], out result))
                    {
                        rmRule.IncludeDeleteRecordLabel = result;
                    }
                    else
                    {
                        rmRule.IncludeDeleteRecordLabel = false;
                    }
                }
                if (bool.TryParse(ruleInfo[RulePropertyIndex.LockRecordBeforeDestroy + IndexChangeFromLabel], out result))
                {
                    rmRule.LockRecordBeforeDestroy = result;
                }
            }

            int keepDataOption = (int)KeepDataStatus.Delete;
            //remove data 才读LeaveStub
            if (rmRule.ArchiverActions.Equals(Action_RemoveDataActionString, StringComparison.OrdinalIgnoreCase)
                && readRemoveOption_LeaveStub
                && bool.TryParse(ruleInfo[RulePropertyIndex.LeaveStub + IndexChangeFromIncludeDeclaredRecord], out result))
            {
                if (result)
                {
                    await SetStubTemplateAsync(ruleInfo, rmRule);
                    keepDataOption |= (int)KeepDataStatus.LinkToDocument;
                }
            }

            if (rmRule.ArchiverActions.Equals(Action_RemoveDataActionString, StringComparison.OrdinalIgnoreCase))
            {
                if (bool.TryParse(ruleInfo[RulePropertyIndex.ArchiverBeforeDestory + IndexChangeFromIncludeDeclaredRecord], out result))
                {
                    if (!result)
                    {
                        keepDataOption |= (int)KeepDataStatus.NotBackup;
                    }
                    else
                    {
                        SetArchiverStorage(ruleInfo, rmRule);
                    }
                }
                else
                {
                    keepDataOption |= (int)KeepDataStatus.NotBackup;
                }
            }

            if (rmRule.ArchiverActions.Equals(Action_ArchiveContentActionString, StringComparison.OrdinalIgnoreCase))
            {
                keepDataOption = (int)KeepDataStatus.Archive;
                if (readRemoveOption_LeaveStub && bool.TryParse(ruleInfo[RulePropertyIndex.LeaveStub + IndexChangeFromIncludeDeclaredRecord], out result))
                {
                    if (result)
                    {
                        await SetStubTemplateAsync(ruleInfo, rmRule);
                        keepDataOption = (int)KeepDataStatus.ArchiveAndLeaveStub;
                    }
                }
                if (bool.TryParse(ruleInfo[RulePropertyIndex.IncludeDeclaredRecord], out result))
                {
                    rmRule.DeleteRecords = result;
                }
                if (bool.TryParse(ruleInfo[RulePropertyIndex.DeleteToRecycleBin + IndexChangeFromLabel], out result))
                {
                    rmRule.DeleteToRecycleBin = result;
                }
                if (isSupportRecordLabel)
                {
                    if (bool.TryParse(ruleInfo[RulePropertyIndex.IncludeLockedFileByRecordLabel], out result))
                    {
                        rmRule.IncludeDeleteRecordLabel = result;
                    }
                    else
                    {
                        rmRule.IncludeDeleteRecordLabel = false;
                    }
                }

                SetArchiverStorage(ruleInfo, rmRule);
            }

            if ((rmRule.ArchiverActions.Equals(Action_ArchiveContentActionString, StringComparison.OrdinalIgnoreCase) || rmRule.ArchiverActions.Equals(Action_RemoveDataActionString, StringComparison.OrdinalIgnoreCase))
                && readRemoveOption_IsEnableRemoveRetentionLabel
                && bool.TryParse(ruleInfo[RulePropertyIndex.IsEnableRemoveRetentionLabel + IndexChangeFromIncludeDeclaredRecord], out result) && result)
            {
                keepDataOption |= (int)KeepDataStatus.IsEnableRemoveRetentionLabel;
            }
            #endregion

            #region Keep Data - Record Declaration and Tagging
            if (rmRule.ArchiverActions.Equals(Action_KeepDataActionString, StringComparison.OrdinalIgnoreCase))
            {
                keepDataOption |= (int)KeepDataStatus.Keep;
            }
            //keep data 才读 DeclareRecord
            if (rmRule.ArchiverActions.Equals(Action_KeepDataActionString, StringComparison.OrdinalIgnoreCase) && readKeepOption_DeclareRecord)
            {
                if (bool.TryParse(ruleInfo[RulePropertyIndex.DeclareRecord + IndexChangeFromIncludeDeclaredRecord], out result))
                {
                    bool shouldDeclare = result && !(isSupportRecordLabel && (rmRule.RuleLevel == PolicyLevel.Document || rmRule.RuleLevel == PolicyLevel.Item));

                    if (shouldDeclare)
                    {
                        keepDataOption |= (int)KeepDataStatus.DeclareRecord;
                    }
                }
            }

            //keep data 才读 Tag
            bool doTag = false;
            if (rmRule.ArchiverActions.Equals(Action_KeepDataActionString, StringComparison.OrdinalIgnoreCase) && readKeepOption_Tag
                && bool.TryParse(ruleInfo[RulePropertyIndex.DoTag + IndexChangeFromIncludeDeclaredRecord], out doTag) && doTag)
            {
                #region tag
                List<RMTagContentInfo> tagContentInfos = new List<RMTagContentInfo>();
                bool tagWithArchived;
                if (bool.TryParse(ruleInfo[RulePropertyIndex.TagWithArchived + IndexChangeFromIncludeDeclaredRecord], out tagWithArchived))
                {
                    if (tagWithArchived)
                    {
                        RMTagContentInfo tagArchived = new RMTagContentInfo();
                        tagArchived.Type = TagContentInfoType.Archived;
                        //TODO leon need check
                        tagArchived.DateTime = DateTime.UtcNow;

                        tagContentInfos.Add(tagArchived);
                    }
                }
                bool tagWithArchivedBy;
                if (bool.TryParse(ruleInfo[RulePropertyIndex.TagWithArchivedBy + IndexChangeFromIncludeDeclaredRecord], out tagWithArchivedBy))
                {
                    if (tagWithArchivedBy)
                    {
                        RMTagContentInfo tagArchivedBy = new RMTagContentInfo();
                        tagArchivedBy.Type = TagContentInfoType.ArchivedBy;
                        //TODO leon need check
                        tagArchivedBy.DateTime = DateTime.UtcNow;

                        tagContentInfos.Add(tagArchivedBy);
                    }
                }
                bool tagWithArchivedDate;
                if (bool.TryParse(ruleInfo[RulePropertyIndex.TagWithArchivedTime + IndexChangeFromIncludeDeclaredRecord], out tagWithArchivedDate))
                {
                    if (tagWithArchivedDate)
                    {
                        RMTagContentInfo tagArchivedDate = new RMTagContentInfo();
                        tagArchivedDate.Type = TagContentInfoType.ArchivedDate;
                        //TODO leon need check
                        tagArchivedDate.DateTime = DateTime.UtcNow;

                        tagContentInfos.Add(tagArchivedDate);
                    }
                }
                bool tagWithCustomColumn;
                if (bool.TryParse(ruleInfo[RulePropertyIndex.TagWithCustomColumn + IndexChangeFromIncludeDeclaredRecord], out tagWithCustomColumn))
                {
                    if (tagWithCustomColumn)
                    {
                        RMTagContentInfo tagCustomColumn = new RMTagContentInfo();
                        var cusColumnType = ruleInfo[RulePropertyIndex.CustomColumnType + IndexChangeFromIncludeDeclaredRecord];
                        var cusColumnName = ruleInfo[RulePropertyIndex.CustomColumnName + IndexChangeFromIncludeDeclaredRecord];
                        var cusColumnValue = ruleInfo[RulePropertyIndex.CustomColumnValue + IndexChangeFromIncludeDeclaredRecord];
                        var cusColumnTimeZone = ruleInfo[RulePropertyIndex.CustomColumnTimeZone + IndexChangeFromIncludeDeclaredRecord];
                        //自定义验证
                        await ValidateCustomColumnAsync(cusColumnType, cusColumnValue, cusColumnName, cusColumnTimeZone, tagCustomColumn);

                        tagContentInfos.Add(tagCustomColumn);
                    }
                }
                if (isSupportRecordLabel)
                {
                    if (bool.TryParse(ruleInfo[RulePropertyIndex.RetentionLabel], out var isCheckRetentionLabel) && isCheckRetentionLabel)
                    {
                        RMTagContentInfo tagLabel = new RMTagContentInfo();
                        tagLabel.Type = TagContentInfoType.RetentionLabel;
                        if (!string.IsNullOrEmpty(ruleInfo[RulePropertyIndex.Label + IndexChangeFromCustomColumnTimeZone]))
                        {
                            tagLabel.Value = ruleInfo[RulePropertyIndex.Label + IndexChangeFromCustomColumnTimeZone];
                            tagLabel.Option = (int)RetentionLabelOptions.Default;
                            tagContentInfos.Add(tagLabel);
                        }
                        else if (bool.TryParse(ruleInfo[RulePropertyIndex.RecordLabel], out result) && result)
                        {
                            tagLabel.Value = ruleInfo[RulePropertyIndex.RecordLabel];
                            tagLabel.Option = (int)RetentionLabelOptions.GetFromGeneralSetting;
                            tagContentInfos.Add(tagLabel);
                        }
                    }
                }
                else
                {
                    if (!string.IsNullOrEmpty(ruleInfo[RulePropertyIndex.Label + IndexChangeFromCustomColumnTimeZone]))
                    {
                        RMTagContentInfo tagLabel = new RMTagContentInfo();
                        tagLabel.DateTime = DateTime.UtcNow;
                        tagLabel.Type = TagContentInfoType.RetentionLabel;
                        //TODO leon need check
                        tagLabel.Value = ruleInfo[RulePropertyIndex.Label + IndexChangeFromCustomColumnTimeZone].ToString();
                        tagContentInfos.Add(tagLabel);
                    }
                }
                #endregion

                    keepDataOption |= (int)KeepDataStatus.TagContent;
                rmRule.TagContentInfo = tagContentInfos;
                if (rmRule.TagContentInfo.Count == 0)
                {
                    throw new Exception("RM_JS_TM_TermImport_AtLeastHaveOneTagOption");
                }
            }
            else
            {
                if (rmRule.ArchiverActions.Equals(Action_KeepDataActionString, StringComparison.OrdinalIgnoreCase) 
                    && readKeepOption_Tag
                    && isSupportRecordLabel)
                {
                    throw new Exception("RM_JS_RDM_CreateRule_Validation_ConditioNoTag");
                }

                rmRule.TagContentInfo = new List<RMTagContentInfo>();
            }

            //如果是KeepData, 则至少包含Declare和Tag里的一项
            if ((keepDataOption & (int)KeepDataStatus.Keep) == (int)KeepDataStatus.Keep && rmRule.IsExoSource != true)
            {
                if ((keepDataOption & (int)KeepDataStatus.DeclareRecord) != (int)KeepDataStatus.DeclareRecord
                    && (keepDataOption & (int)KeepDataStatus.TagContent) != (int)KeepDataStatus.TagContent)
                {
                    if (rmRule.RuleLevel == PolicyLevel.Folder)
                    {
                        throw new Exception("RM_JS_TM_TermImport_NoTag");
                    }
                    else
                    {
                        throw new Exception("RM_JS_TM_TermImport_NoDeclareNoTag");
                    }
                }
            }
            rmRule.RuleKeepDataOption = keepDataOption;
            //rmRule.DeclareLinkFile = (!string.IsNullOrWhiteSpace(ruleInfo[RulePropertyIndex.DeclareLinkFile])) && ruleInfo[RulePropertyIndex.DeclareLinkFile].Equals("true", StringComparison.OrdinalIgnoreCase) ? true : false;
            #endregion

            #region Move - Move documents to a new destination library
            string moveUrl = ruleInfo[RulePropertyIndex.MoveUrl + IndexChangeFromLabel];

            //Action是Move才读 Move设置
            if (rmRule.ArchiverActions.Equals(Action_MoveDataActionString, StringComparison.OrdinalIgnoreCase) && readMoveOption)
            {
                var moveDto = new MoveToDto()
                {
                    IsSpecifyLocation = true,
                    LocationPath = moveUrl
                };

                ValidateMoveUrl(moveDto);
                moveDto.FileNameConflictOption = GetConflictOption(ruleInfo[RulePropertyIndex.ConflictResolution + IndexChangeFromLabel]);
                bool IsDeclareMoveDate = false;
                bool.TryParse(ruleInfo[RulePropertyIndex.DeclareAfterMove + IndexChangeFromLabel], out IsDeclareMoveDate);
                moveDto.NotDeclareMovedData = !IsDeclareMoveDate;
                bool IsKeepClassification = false;
                bool.TryParse(ruleInfo[RulePropertyIndex.KeepReclassifyAfterMove + IndexChangeFromLabel], out IsKeepClassification);
                moveDto.isKeepClassification = IsKeepClassification;
                rmRule.MoveDto = moveDto;
            }
            if (rmRule.ArchiverActions.Equals(Action_ExportOnlyActionString, StringComparison.OrdinalIgnoreCase))
            {
                rmRule.EnableExport = true;
                var exportInfo = new SOExportInfo();
                exportInfo.exportSPDataOption = ExportSPDataOption.ExportWithoutArchive;
                var exportFormat = ruleInfo[RulePropertyIndex.ExportOnlyFormat];
                if (string.IsNullOrEmpty(exportFormat) || string.IsNullOrEmpty(exportFormat.Trim()))
                {
                    throw new Exception("RM_JS_TM_TermImport_NoExportFormat");
                }
                exportInfo.exportType = (GCommon.Contract.StorageOptimization.Object.ExportTypeValue)Enum.Parse(typeof(GCommon.Contract.StorageOptimization.Object.ExportTypeValue), exportFormat);
                if ((exportInfo.exportType == GCommon.Contract.StorageOptimization.Object.ExportTypeValue.NAA || exportInfo.exportType == GCommon.Contract.StorageOptimization.Object.ExportTypeValue.NARA) && rmRule.RuleLevel != PolicyLevel.Document && rmRule.RuleLevel != PolicyLevel.Folder)
                {
                    throw new Exception("RM_JS_TM_TermImport_ExportOnlyNotSupport");
                }
                SetExportToDestinationLibrary(ruleInfo, rmRule, exportInfo);
                rmRule.ExportInfo = exportInfo;
            }
            #endregion
            SetExportSettings(ruleInfo, rmRule);
            await SetManunalSettingsAsync(ruleInfo, rmRule);
            return rmRule;
        }

        private async Task<RMRuleInfos> BuildGoogleRuleAsync(string[] ruleInfo)
        {
            var rmRule = new RMRuleInfos
            {
                RuleFilters = new List<RuleFilter>()
            };

            if (string.IsNullOrEmpty(ruleInfo[RulePropertyIndex.RuleLevel]) || string.IsNullOrEmpty(ruleInfo[RulePropertyIndex.RuleLevel].Trim()))
            {
                throw new Exception("RM_JS_TM_TermImport_NoObjectLevel");
            }

            if (!ruleInfo[RulePropertyIndex.RuleLevel].Equals("Document") && !ruleInfo[RulePropertyIndex.RuleLevel].Equals("Document/Email"))
            {
                throw new Exception("Invalid rule level. Only 'Document' or 'Document/Email' are allowed for Google Drive");
            }

            rmRule.RuleLevel = PolicyLevel.GoogleDriveDocument;
            rmRule.ArchiverActions = ruleInfo[RulePropertyIndex.RuleAction];

            if (rmRule.ArchiverActions.Equals(Action_RemoveDataActionString, StringComparison.OrdinalIgnoreCase))
            {
                rmRule.RuleKeepDataOption = (int)KeepDataStatus.Delete;

                if (!string.IsNullOrEmpty(ruleInfo[RulePropertyIndex.ExportLocation + IndexChangeFromLabel]))
                {
                    SetArchiverStorageToExportInfo(ruleInfo, rmRule);
                    rmRule.ExportInfo.exportSPDataOption = ExportSPDataOption.ExportBeforeArchive;
                    var exportFormat = ruleInfo[RulePropertyIndex.ExportFormat + IndexChangeFromLabel];
                    if (string.IsNullOrEmpty(exportFormat) || string.IsNullOrEmpty(exportFormat.Trim()))
                    {
                        throw new Exception("RM_JS_TM_TermImport_NoExportFormat");
                    }
                    rmRule.ExportInfo.exportType = (AvePoint.GCommon.Contract.StorageOptimization.Object.ExportTypeValue)Enum.Parse(typeof(GCommon.Contract.StorageOptimization.Object.ExportTypeValue), exportFormat);
                    rmRule.EnableExport = true;
                }
            }
            else if (rmRule.ArchiverActions.Equals(Action_KeepDataActionString, StringComparison.OrdinalIgnoreCase))
            {
                rmRule.RuleKeepDataOption = (int)KeepDataStatus.Keep;

                bool doTag = false;
                if (bool.TryParse(ruleInfo[RulePropertyIndex.DoTag + IndexChangeFromIncludeDeclaredRecord], out doTag) && doTag)
                {
                    List<RMTagContentInfo> tagContentInfos = new List<RMTagContentInfo>();


                    if (isSupportRecordLabel)
                    {
                        if (bool.TryParse(ruleInfo[RulePropertyIndex.RetentionLabel], out var isCheckRetentionLabel) && isCheckRetentionLabel)
                        {
                            RMTagContentInfo tagLabel = new RMTagContentInfo();
                            tagLabel.Type = TagContentInfoType.RetentionLabel;
                            if (!string.IsNullOrEmpty(ruleInfo[RulePropertyIndex.Label + IndexChangeFromCustomColumnTimeZone]))
                            {
                                tagLabel.Value = ruleInfo[RulePropertyIndex.Label + IndexChangeFromCustomColumnTimeZone];
                                tagLabel.Option = (int)RetentionLabelOptions.Default;
                                tagContentInfos.Add(tagLabel);
                            }
                            else if (bool.TryParse(ruleInfo[RulePropertyIndex.RecordLabel], out var result) && result)
                            {
                                tagLabel.Value = ruleInfo[RulePropertyIndex.RecordLabel];
                                tagLabel.Option = (int)RetentionLabelOptions.GetFromGeneralSetting;
                                tagContentInfos.Add(tagLabel);
                            }
                        }
                    }
                    else
                    {
                        if (!string.IsNullOrEmpty(ruleInfo[RulePropertyIndex.Label + IndexChangeFromCustomColumnTimeZone]))
                        {
                            RMTagContentInfo tagLabel = new RMTagContentInfo
                            {
                                Type = TagContentInfoType.RetentionLabel,
                                DateTime = DateTime.UtcNow,
                                Value = ruleInfo[RulePropertyIndex.Label + IndexChangeFromCustomColumnTimeZone]
                            };
                            tagContentInfos.Add(tagLabel);
                        }
                    }

                    rmRule.TagContentInfo = tagContentInfos;
                    if (rmRule.TagContentInfo.Count == 0)
                    {
                        throw new Exception("RM_JS_TM_TermImport_AtLeastHaveOneTagOption");
                    }
                }
            }
            else if (rmRule.ArchiverActions.Equals(Action_ExportOnlyActionString, StringComparison.OrdinalIgnoreCase))
            {
                if (!string.IsNullOrEmpty(ruleInfo[RulePropertyIndex.ExportLocation + IndexChangeFromLabel]))
                {
                    SetArchiverStorageToExportInfo(ruleInfo, rmRule);
                    rmRule.ExportInfo.exportSPDataOption = ExportSPDataOption.ExportWithoutArchive;
                    var exportFormat = ruleInfo[RulePropertyIndex.ExportOnlyFormat];
                    if (string.IsNullOrEmpty(exportFormat) || string.IsNullOrEmpty(exportFormat.Trim()))
                    {
                        throw new Exception("RM_JS_TM_TermImport_NoExportFormat");
                    }
                    rmRule.ExportInfo.exportType = (AvePoint.GCommon.Contract.StorageOptimization.Object.ExportTypeValue)Enum.Parse(typeof(GCommon.Contract.StorageOptimization.Object.ExportTypeValue), exportFormat);
                    rmRule.EnableExport = true;
                }
            }
            else if (rmRule.ArchiverActions.Equals(Action_ArchiveContentActionString, StringComparison.OrdinalIgnoreCase))
            {
                rmRule.RuleKeepDataOption = (int)KeepDataStatus.Archive;
                SetArchiverStorage(ruleInfo, rmRule);
            }
            else
            {
                throw new Exception("Invalid action specified for Google Drive rules");
            }

            await SetManunalSettingsAsync(ruleInfo, rmRule);

            return rmRule;
        }
        
        private void SetExportToDestinationLibrary(string[] ruleInfo, RMRuleInfos rmRule, SOExportInfo exportInfo)
        {
            var exportToLibraryPath = ruleInfo[RulePropertyIndex.ExportToDestinationLibrary + IndexChangeFromLabel];
            var exportLocation = ruleInfo[RulePropertyIndex.ExportLocation + IndexChangeFromLabel];
            if( string.IsNullOrEmpty(exportToLibraryPath) && string.IsNullOrEmpty(exportLocation))
            {
                throw new Exception("Export location or export to destination library path must be have value.");
            }
            if(!string.IsNullOrEmpty(exportToLibraryPath))
            {
                var moveDto = new MoveToDto
                {
                    IsSpecifyLocation = true,
                    LocationPath = exportToLibraryPath
                };

                rmRule.MoveDto = moveDto;
                return;
            }

            try
            {
                var exportLocationName = exportLocation;
                if (string.IsNullOrWhiteSpace(exportLocationName))
                {
                    throw new Exception("export location name is null.");
                }
                if (!string.IsNullOrWhiteSpace(exportLocationName) && DEFAULTSTORAGENAME.Equals(exportLocationName, StringComparison.OrdinalIgnoreCase))
                {
                    throw new Exception("RM_JS_TM_TermImport_ExportLocationCannotUseDefaultStorage");
                }
                var storage = StorageDeviceService.GetStorageDeviceByName(exportLocationName);
                if (string.IsNullOrEmpty(storage.Id) && string.IsNullOrEmpty(storage.Name))
                {
                    throw new Exception("export location not found.");
                }
                if ((storage.Type == (int)StorageDeviceType.Google || storage.Type == (int)StorageDeviceType.Dropbox) && rmRule.RuleLevel == PolicyLevel.FileSysFile)
                {
                    throw new Exception("RM_JS_TM_TermImport_FSCannotUseDefaultStorage");
                }
                exportInfo.exportLocationId = storage.Id;
                exportInfo.exportLocationName = storage.Name;
            }
            catch (Exception e)
            {
                mLog.Error($"Error occurred while setting archive storage. Rule:{rmRule.RuleName} Error:{e.ToString()}");
                switch (e.Message)
                {
                    case "RM_JS_TM_TermImport_FSCannotUseDefaultStorage":
                    case "RM_JS_TM_TermImport_ExportLocationCannotUseDefaultStorage":
                        throw;
                    default:
                        throw new Exception("RM_JS_TM_TermImport_ArchiveStorageInvalid");
                }
             }
        }

        /// <summary>
        /// phy & fs & sp-onprem 暂不支持export功能
        /// </summary>
        /// <param name="ruleInfo"></param>
        /// <param name="rmRule"></param>
        private void SetExportSettings(string[] ruleInfo, RMRuleInfos rmRule)
        {
            
            if (rmRule.EnableExport == true && rmRule.ExportInfo != null && rmRule.ExportInfo.exportSPDataOption == ExportSPDataOption.ExportWithoutArchive)
            {
                //export only的rule不收集approve 和 export archive 
                mLog.Info("Rule is export without archiver,not collect approve and export archive info.");
            }
            else
            {
                if (rmRule.ArchiverActions != null && !rmRule.ArchiverActions.Equals(Action_MoveDataActionString, StringComparison.OrdinalIgnoreCase))
                {
                    bool enableExport = false;
                    if (bool.TryParse(ruleInfo[RulePropertyIndex.EnableExport + IndexChangeFromLabel], out enableExport))
                    {
                        rmRule.EnableExport = enableExport;
                        if (enableExport)
                        {
                            var exportInfo = new SOExportInfo();
                            exportInfo.exportSPDataOption = ExportSPDataOption.ExportBeforeArchive;
                            var exportFormat = ruleInfo[RulePropertyIndex.ExportFormat + IndexChangeFromLabel];
                            if (string.IsNullOrEmpty(exportFormat) || string.IsNullOrEmpty(exportFormat.Trim()))
                            {
                                throw new Exception("RM_JS_TM_TermImport_NoExportFormat");
                            }
                            exportInfo.exportType = (AvePoint.GCommon.Contract.StorageOptimization.Object.ExportTypeValue)Enum.Parse(typeof(AvePoint.GCommon.Contract.StorageOptimization.Object.ExportTypeValue), exportFormat);
                            if ((exportInfo.exportType == AvePoint.GCommon.Contract.StorageOptimization.Object.ExportTypeValue.NAA || exportInfo.exportType == AvePoint.GCommon.Contract.StorageOptimization.Object.ExportTypeValue.NARA) 
                                && rmRule.RuleLevel != PolicyLevel.Document && rmRule.RuleLevel != PolicyLevel.ExchangeOnlineItem && rmRule.RuleLevel != PolicyLevel.Folder)
                            {
                                throw new Exception("RM_JS_TM_TermImport_ExportNotSupport");
                            }
                            SetExportToDestinationLibrary(ruleInfo, rmRule, exportInfo);
                            rmRule.ExportInfo = exportInfo;
                        }
                    }
                }
            }
        }

        private async Task SetManunalSettingsAsync(string[] ruleInfo, RMRuleInfos rmRule)
        {
            #region Manual Approval
            bool isEnableMannual = false;
            if (rmRule.ArchiverActions != null 
                && (rmRule.ArchiverActions.Equals(Action_RemoveDataActionString, StringComparison.OrdinalIgnoreCase) 
                || rmRule.ArchiverActions.Equals(Action_ArchiveContentActionString, StringComparison.OrdinalIgnoreCase))
                && bool.TryParse(ruleInfo[RulePropertyIndex.EnableMannualApprove + IndexChangeFromLabel], out isEnableMannual))
            {
                rmRule.EnableManualApproval = isEnableMannual;
                if (isEnableMannual)
                {
                    if (string.IsNullOrEmpty(ruleInfo[RulePropertyIndex.ReviewType + IndexChangeFromLabel]) || string.IsNullOrEmpty(ruleInfo[RulePropertyIndex.ReviewType + IndexChangeFromLabel].Trim()))
                    {
                        throw new Exception("RM_JS_TM_TermImport_NoReviewType");
                    }
                    rmRule.ManualReviewType = GetReviewType(ruleInfo[RulePropertyIndex.ReviewType + IndexChangeFromLabel]);
                    if (rmRule.ManualReviewType == ReviewType.Workflow)
                    {
                        if (string.IsNullOrEmpty(ruleInfo[RulePropertyIndex.WorkflowName + IndexChangeFromLabel]) || string.IsNullOrEmpty(ruleInfo[RulePropertyIndex.WorkflowName + IndexChangeFromLabel].Trim()))
                        {
                            throw new Exception("RM_JS_TM_TermImport_NoWorkflowInput");
                        }
                        var workflow = ManualProcessManagementService.GetSimpleProcessByName(ruleInfo[RulePropertyIndex.WorkflowName + IndexChangeFromLabel]);
                        if (workflow != null)
                        {
                            rmRule.WorkflowId = workflow.ReferenceId.ToString();
                        }
                        if (rmRule.WorkflowId == null || rmRule.WorkflowId == string.Empty || rmRule.WorkflowId == Guid.Empty.ToString())
                        {
                            throw new Exception("RM_JS_TM_TermImport_NoWorkflow");
                        }
                        bool sendEmail = false;
                        if (bool.TryParse(ruleInfo[RulePropertyIndex.SendEmail + IndexChangeFromLabel], out sendEmail))
                        {
                            rmRule.IsSendEmailToOwner = sendEmail;
                        }
                    }
                    else
                    {
                        //set user
                        string userStr = ruleInfo[RulePropertyIndex.RecordOwner + IndexChangeFromLabel];
                        if (!string.IsNullOrEmpty(userStr))
                        {
                            rmRule.Users = RuleManagerService.Convert2AOSUserDtos(await GetImportUsersAsync(userStr));
                        }
                        else
                        {
                            throw new Exception("RM_JS_TM_TermImport_UserColumnIsNull");
                        }
                        //send email
                        bool sendEmail = false;
                        if (bool.TryParse(ruleInfo[RulePropertyIndex.SendEmail + IndexChangeFromLabel], out sendEmail))
                        {
                            rmRule.IsSendEmailToOwner = sendEmail;
                            if (sendEmail && string.IsNullOrEmpty(userStr))
                            {
                                //column值为空
                                throw new Exception("RM_JS_TM_TermImport_RecordOwnerNull");
                            }
                            if (sendEmail && rmRule.Users.IsNullOrEmpty())
                            {
                                //column值不为空 但是没找到user
                                throw new Exception("RM_JS_TM_TermImport_NoUser");
                            }
                        }
                    }
                }
            }
            #endregion
        }

        private TagContentInfoType GetTagType(string typeStr)
        {
            switch (typeStr)
            {
                case "Text":
                    return TagContentInfoType.Text;
                case "Number":
                    return TagContentInfoType.Number;
                case "Date and Time":
                    return TagContentInfoType.DateTime;
                case "Yes/No":
                    return TagContentInfoType.Boolean;
                default:
                    throw new Exception("RM_JS_TM_TermImport_CustomColumnTypeError");
            }
        }

        private FileNameConflictOption GetConflictOption(string conflictOptionStr)
        {
            switch (conflictOptionStr)
            {
                case "Skip":
                    return FileNameConflictOption.Skip;
                case "Overwrite":
                    return FileNameConflictOption.Overwrite;
                case "Add a suffix":
                    return FileNameConflictOption.Rename;
                default:
                    throw new Exception("RM_JS_TM_TermImport_ConflictResolutionError");
            }
        }

        private ReviewType GetReviewType(string type)
        {
            switch (type)
            {
                case "Manual approval process":
                    return ReviewType.Workflow;
                case "Record owner":
                    return ReviewType.RecordOwner;
                default:
                    throw new Exception("RM_JS_TM_TermImport_ReviewTypeError");
            }
        }
        private async Task<RMRuleInfos> GetRuleInfoObjectAsync(string[] ruleInfo, Dictionary<string, RMRuleInfos> importRuleDic)
        {
            var ruleName = ruleInfo[RulePropertyIndex.Name];
            var ruleSourceType = ruleInfo[RulePropertyIndex.SourceType];
            var convertedCriteriaType  = ReportUtil.KeyValues.ContainsKey(ruleInfo[RulePropertyIndex.CriteriaType]) ? ReportUtil.KeyValues[ruleInfo[RulePropertyIndex.CriteriaType]].ToString() : ruleInfo[RulePropertyIndex.CriteriaType];
            var criteriaType = (ArchiverFilterRuleType)Enum.Parse(typeof(ArchiverFilterRuleType), convertedCriteriaType);
            if (string.IsNullOrEmpty(ruleName) || string.IsNullOrEmpty(ruleName.Trim()))
            {
                throw new Exception("RM_JS_TM_TermImport_NoRuleName");
            }
            if (string.IsNullOrEmpty(ruleSourceType) || string.IsNullOrEmpty(ruleSourceType.Trim()))
            {
                throw new Exception("RM_JS_TM_TermImport_NoRuleSource");
            }
            if (!TenantService.IsNewOpusTenant() && (criteriaType == ArchiverFilterRuleType.SensitivityLabel || criteriaType == ArchiverFilterRuleType.RetentionLabel))
            {
                throw new Exception("RM_JS_TM_TermImport_NoSupportRetentionSensitiveOldAccount");
            }
            bool isSPSource = CheckRuleSource(ruleSourceType, SourceType_SP);
            RMRuleInfos rmRule;
            if (importRuleDic.ContainsKey(ruleName))
            {
                rmRule = importRuleDic[ruleName];
                //exo row在sprow前  只处理第一行的sprow action
                if ((rmRule.IsExoSource || rmRule.IsPhySource || rmRule.IsFSSource || rmRule.IsSPLocalSource || rmRule.IsOneDriveSource || rmRule.IsGoogleDriveSource || rmRule.IsTeamsSource) && !rmRule.IsSpSource && isSPSource)
                {
                    rmRule = await CreateRMRuleForDesAsync(ruleInfo, rmRule);
                }
            }
            else
            {
                rmRule = await BuildSPRuleAsync(ruleInfo, ruleSourceType, isSPSource);
            }
            if (isSPSource)
            {
                rmRule.IsSpSource = true;
            }
            if (CheckRuleSource(ruleSourceType, SourceType_EXO) && rmRule.EXORule == null)
            {
                rmRule.EXORule = await BuildEXORuleAsync(ruleInfo);
                rmRule.IsExoSource = true;
            }
            if (CheckRuleSource(ruleSourceType, SourceType_PHY) && rmRule.PhysicalRule == null)
            {
                rmRule.PhysicalRule = await BuildPhysicalRuleAsync(ruleInfo);
                rmRule.IsPhySource = true;
            }
            if (CheckRuleSource(ruleSourceType, SourceType_FS) && rmRule.FSRule == null)
            {
                rmRule.FSRule = await BuildFSRuleAsync(ruleInfo);
                rmRule.IsFSSource = true;
            }
            if (CheckRuleSource(ruleSourceType, SourceType_SPLocal) && rmRule.SPLocalRule == null)
            {
                rmRule.SPLocalRule = await BuildSPLocalRuleAsync(ruleInfo);
                rmRule.IsSPLocalSource = true;
            }
            if (CheckRuleSource(ruleSourceType, SourceType_OneDrive) && rmRule.OneDriveRule == null)
            {
                rmRule.OneDriveRule = await BuildOneDriveRuleAsync(ruleInfo);
                rmRule.IsOneDriveSource = true;
            }
            if (CheckRuleSource(ruleSourceType, SourceType_AzureFile) && rmRule.AzureFileRule == null)
            {
                rmRule.AzureFileRule = await BuildAzureFilerRuleAsync(ruleInfo);
                rmRule.IsAzureFileSource = true;
            }
            if (CheckRuleSource(ruleSourceType, SourceType_Connector) && rmRule.ConnectorRule == null)
            {
                rmRule.ConnectorRule = await BuildConnectorRuleAsync(ruleInfo);
                rmRule.IsConnectorSource = true;
            }
            if (CheckRuleSource(ruleSourceType, SourceType_Box) && rmRule.BoxRule == null)
            {
                rmRule.BoxRule = await BuildBoxRuleAsync(ruleInfo);
                rmRule.IsBoxSource = true;
            }
            if (CheckRuleSource(ruleSourceType, Google_Drive) && rmRule.GoogleDriveRule == null)
            {
                if (rmRule.ArchiverActions.Equals(Action_MoveDataActionString, StringComparison.OrdinalIgnoreCase))
                {
                    throw new Exception("RM_JS_TM_TermImport_NotSupportMoveActionImport");
                }
                rmRule.GoogleDriveRule = await BuildGoogleRuleAsync(ruleInfo);
                rmRule.IsGoogleDriveSource = true;
                rmRule.EnableExport = false;
            }
            return rmRule;
        }

        private async Task<RuleFilter> BuildRuleFilterAsync(string[] ruleInfo, RMRuleInfos rmRuleInfo)
        {
            RuleFilter filter = new RuleFilter();
            var ruleLevel = rmRuleInfo.RuleLevel;
            if (ruleLevel == PolicyLevel.ExchangeOnlineItem)
            {
                filter.Level = PolicyLevel.ExchangeOnlineItem_Message;
            }
            else
            {
                filter.Level = ruleLevel;
            }
            var combineModeString = ruleInfo[RulePropertyIndex.CombineMode];
            
            if (I18NEntity.GetString("RM_JS_RDM_CreateRule_AllOrAny_All").Equals(combineModeString, StringComparison.OrdinalIgnoreCase) 
                || I18NEntity.GetString("RM_JS_RDM_CreateRule_AllOrAny_Any").Equals(combineModeString, StringComparison.OrdinalIgnoreCase))
            {
                if (rmRuleInfo.RuleFilters.IsNullOrEmpty())
                {
                    filter.CombineMode = (ArchiverFilterCombineMode)Enum.Parse(typeof(ArchiverFilterCombineMode), ConvertCombineMode(ruleInfo[RulePropertyIndex.CombineMode]));
                }
                else
                {
                    filter.CombineMode = rmRuleInfo.RuleFilters[0].CombineMode;
                }
            }
            else
            {
                filter.CombineMode = (ArchiverFilterCombineMode)Enum.Parse(typeof(ArchiverFilterCombineMode), ConvertCombineMode(ruleInfo[RulePropertyIndex.CombineMode]));
            }
            if (string.IsNullOrEmpty(ruleInfo[RulePropertyIndex.CriteriaType]))
            {
                throw new Exception("RM_JS_TM_TermImport_NoCriteriaType");
            }
            if (ReportUtil.KeyValues.Keys.Contains(ruleInfo[RulePropertyIndex.CriteriaType]))
            {
                ruleInfo[RulePropertyIndex.CriteriaType] = ReportUtil.KeyValues[ruleInfo[RulePropertyIndex.CriteriaType]].ToString();
            }
            filter.RuleType = (ArchiverFilterRuleType)Enum.Parse(typeof(ArchiverFilterRuleType), ruleInfo[RulePropertyIndex.CriteriaType]);
            var filterName = ruleInfo[RulePropertyIndex.CriteriaName];
            if(filterName != null && filterName.StartsWith('[') && filterName.EndsWith(']') && filterName.IndexOf(' ') > 0)
            {
                filterName = filterName.Replace(" ", ContractConstants.SHAREPOINT_SITECOLUMN_SPACE_ESCAPE_CHARACTER);
            }
            filter.filterName = filterName;
            if (string.IsNullOrEmpty(ruleInfo[RulePropertyIndex.CriteriaCondition]))
            {
                throw new Exception("RM_JS_TM_TermImport_NoCriteriaCondition");
            }
            if(ruleInfo[RulePropertyIndex.CriteriaCondition].Equals("IsBlank", StringComparison.InvariantCultureIgnoreCase))
            {
                ruleInfo[RulePropertyIndex.CriteriaCondition] = "IsEmpty";
            }
            filter.Condition = (ArchiverFilterCondition)Enum.Parse(typeof(ArchiverFilterCondition), ruleInfo[RulePropertyIndex.CriteriaCondition]);
            if (filter.Condition == ArchiverFilterCondition.Equals)
            {
                filter.Condition = (ArchiverFilterCondition)262936;
            }
            if (filter.RuleType == ArchiverFilterRuleType.DocumentSize || filter.RuleType == ArchiverFilterRuleType.Size)
            {
                filter.filterName = string.Empty;
            }
            filter.Value1 = ruleInfo[RulePropertyIndex.ConditionValue];
            if(filter.Condition == ArchiverFilterCondition.IsEmpty)
            {
                filter.Value1 = string.Empty;
            }
            if (filter.RuleType == ArchiverFilterRuleType.DocumentSize || filter.RuleType == ArchiverFilterRuleType.SiteCollectionSizeTrigger || filter.RuleType == ArchiverFilterRuleType.Size)
            {
                string Value1Unit = ruleInfo[RulePropertyIndex.ConditionValueUnit];
                if (!string.IsNullOrEmpty(Value1Unit))
                {
                    filter.Value1Unit = (PolicyValueUnit)Enum.Parse(typeof(PolicyValueUnit), Value1Unit);
                }
            }
            if (filter.RuleType == ArchiverFilterRuleType.ModifiedTime || filter.RuleType == ArchiverFilterRuleType.CreatedTime ||
               filter.RuleType == ArchiverFilterRuleType.DateTimeColumn || filter.RuleType == ArchiverFilterRuleType.LastAccessedTime || filter.RuleType == ArchiverFilterRuleType.LastActiveTime
               || filter.RuleType == ArchiverFilterRuleType.DateTimeCustomProperty || filter.RuleType == ArchiverFilterRuleType.SendDateUTC
               || filter.RuleType == ArchiverFilterRuleType.ParentLibraryDateTime || filter.RuleType == ArchiverFilterRuleType.ParentSiteCollectionDateTime
               || filter.RuleType == ArchiverFilterRuleType.PropertyBagDateTime
               || filter.RuleType == ArchiverFilterRuleType.LastestSubfolderDisposalDate)
            {
                if (filter.Condition == ArchiverFilterCondition.OlderThan)
                {
                    filter.Value1 = ruleInfo[RulePropertyIndex.ConditionValue];
                    string Value1Unit = ruleInfo[RulePropertyIndex.ConditionValueUnit];
                    if (!string.IsNullOrEmpty(Value1Unit))
                    {
                        var nuit = (PolicyValueUnit)Enum.Parse(typeof(PolicyValueUnit), Value1Unit);
                        if (nuit == PolicyValueUnit.Weeks || nuit == PolicyValueUnit.Months || nuit == PolicyValueUnit.Years)
                        {
                            if (filter.Value1 == "0")
                            {
                                throw new Exception("RM_JS_TM_TermImport_OldThanValueError");
                            }
                        }
                        filter.Value1Unit = nuit;
                    }

                }
                else
                {
                    var startTime = ruleInfo[RulePropertyIndex.ConditionBeginTime];
                    if (!string.IsNullOrEmpty(startTime))
                    {
                        try
                        {
                            startTime = GetDateTimeStr(startTime);
                        }
                        catch
                        {
                            throw new Exception("RM_JS_TM_TermImport_ConditionStartTimeError");
                        }
                        filter.Value1 = startTime;
                        string timeZoneId = (await GeneralSetting).TimeZoneId;
                        if (string.IsNullOrEmpty(timeZoneId))
                        {
                            //default or exception
                            throw new Exception("RM_JS_TM_TermImport_TimeZoneErr");
                        }
                        filter.StartTimeInfo = new DisplayDateTime()
                        {
                            IsDayLightSaving = false,
                            StartTime = startTime,
                            TimeZoneId = timeZoneId
                        };
                    }
                    else
                    {
                        throw new Exception("RM_JS_TM_TermImport_ConditionStartTimeError");
                    }
                    if (filter.Condition == ArchiverFilterCondition.FromTo)
                    {
                        var endTime = ruleInfo[RulePropertyIndex.ConditionEndTime];
                        if (!string.IsNullOrEmpty(endTime))
                        {
                            try
                            {
                                endTime = GetDateTimeStr(endTime);
                            }
                            catch
                            {
                                throw new Exception("RM_JS_TM_TermImport_ConditionEndTimeError");
                            }
                            filter.Value2 = endTime;
                            string timeZoneId = (await GeneralSetting).TimeZoneId;
                            if (string.IsNullOrEmpty(timeZoneId))
                            {
                                //default or exception
                                throw new Exception("RM_JS_TM_TermImport_TimeZoneErr");
                            }
                            filter.EndTimeInfo = new DisplayDateTime()
                            {
                                IsDayLightSaving = false,
                                StartTime = endTime,
                                TimeZoneId = timeZoneId
                            };
                        }
                        else
                        {
                            throw new Exception("RM_JS_TM_TermImport_ConditionEndTimeError");
                        }
                    }
                }
            }
            if (filter.RuleType == ArchiverFilterRuleType.TextLabelProperty || filter.RuleType == ArchiverFilterRuleType.NumberLabelProperty || filter.RuleType == ArchiverFilterRuleType.DateTimeLabelProperty)
            {
                if (ruleInfo[RulePropertyIndex.CriteriaName].Contains(':'))
                {
                    var parts = ruleInfo[RulePropertyIndex.CriteriaName].Split(':');
                    filter.filterName = parts[0];
                    filter.Value1 = parts[1];
                }
                
                filter.Value2 = !string.IsNullOrEmpty(ruleInfo[RulePropertyIndex.ConditionValue]) ? ruleInfo[RulePropertyIndex.ConditionValue] : string.Empty;

                if (filter.RuleType == ArchiverFilterRuleType.NumberLabelProperty)
                {
                    if (!Regex.IsMatch(filter.Value2, @"^\d+$"))
                    {
                        throw new Exception("RM_JS_TM_TermImport_ValueErr");
                    }
                }

                if (filter.RuleType == ArchiverFilterRuleType.DateTimeLabelProperty)
                {
                    if (filter.Condition == ArchiverFilterCondition.OlderThan)
                    {
                        filter.Value2 = ruleInfo[RulePropertyIndex.ConditionValue];
                        string Value1Unit = ruleInfo[RulePropertyIndex.ConditionValueUnit];
                        if (!string.IsNullOrEmpty(Value1Unit))
                        {
                            var nuit = (PolicyValueUnit)Enum.Parse(typeof(PolicyValueUnit), Value1Unit);
                            if (nuit == PolicyValueUnit.Weeks || nuit == PolicyValueUnit.Months || nuit == PolicyValueUnit.Years)
                            {
                                if (filter.Value2 == "0")
                                {
                                    throw new Exception("RM_JS_TM_TermImport_OldThanValueError");
                                }
                            }
                            filter.Value2Unit = nuit;
                        }
                    }
                    if (filter.Condition == ArchiverFilterCondition.FromTo || filter.Condition == ArchiverFilterCondition.Before)
                    {
                        var startTime = ruleInfo[RulePropertyIndex.ConditionBeginTime];
                        if (!string.IsNullOrEmpty(startTime))
                        {
                            try
                            {
                                startTime = GetDateTimeStr(startTime);
                            }
                            catch
                            {
                                throw new Exception("RM_JS_TM_TermImport_ConditionStartTimeError");
                            }
                            filter.Value2 = startTime;
                            string timeZoneId = (await GeneralSetting).TimeZoneId;
                            if (string.IsNullOrEmpty(timeZoneId))
                            {
                                throw new Exception("RM_JS_TM_TermImport_TimeZoneErr");
                            }
                            filter.StartTimeInfo = new DisplayDateTime()
                            {
                                IsDayLightSaving = false,
                                StartTime = startTime,
                                TimeZoneId = timeZoneId
                            };
                        }
                        else
                        {
                            throw new Exception("RM_JS_TM_TermImport_ConditionStartTimeError");
                        }
                        
                        var endTime = ruleInfo[RulePropertyIndex.ConditionEndTime];
                        if (!string.IsNullOrEmpty(endTime))
                        {
                            try
                            {
                                endTime = GetDateTimeStr(endTime);
                            }
                            catch
                            {
                                throw new Exception("RM_JS_TM_TermImport_ConditionEndTimeError");
                            }
                            filter.Value3 = endTime;
                            string timeZoneId = (await GeneralSetting).TimeZoneId;
                            if (string.IsNullOrEmpty(timeZoneId))
                            {
                                throw new Exception("RM_JS_TM_TermImport_TimeZoneErr");
                            }
                            filter.EndTimeInfo = new DisplayDateTime()
                            {
                                IsDayLightSaving = false,
                                StartTime = endTime,
                                TimeZoneId = timeZoneId
                            };
                        }
                        else if (filter.Condition != ArchiverFilterCondition.Before)
                        {
                            throw new Exception("RM_JS_TM_TermImport_ConditionEndTimeError");
                        }
                    }
                }
            }
            if (string.IsNullOrEmpty(filter.Value1) || string.IsNullOrEmpty(filter.Value1.Trim()))
            {
                if (filter.Condition != ArchiverFilterCondition.IsEmpty)
                {
                    throw new Exception("RM_JS_TM_TermImport_NoConditionValue");
                }
            }
          
            switch (filter.RuleType)
            {
                case ArchiverFilterRuleType.BooleanColumn:
                case ArchiverFilterRuleType.BooleanCustomProperty:
                case ArchiverFilterRuleType.ParentLibraryBoolean:
                case ArchiverFilterRuleType.ParentSiteCollectionBoolean:
                case ArchiverFilterRuleType.PropertyBagBoolean:
                    if (filter.Value1.Equals(NoString, StringComparison.OrdinalIgnoreCase))
                    {
                        filter.Value1 = NoString;
                    }
                    else if (filter.Value1.Equals(YesString, StringComparison.OrdinalIgnoreCase))
                    {
                        filter.Value1 = YesString;
                    }
                    break;
            }
            return filter;
        }
        public async Task<RMRuleInfos> ConvertToRuleObjectAsync(string[] ruleInfo, Dictionary<string, RMRuleInfos> importRuleDic)
        {
            var ruleInfoObj = await GetRuleInfoObjectAsync(ruleInfo, importRuleDic);
            var ruleSourceType = ruleInfo[RulePropertyIndex.SourceType];
            if (CheckRuleSource(ruleSourceType, SourceType_SP))
            {
                ruleInfoObj.RuleFilters.Add(await BuildRuleFilterAsync(ruleInfo, ruleInfoObj));
            }
            if (CheckRuleSource(ruleSourceType, SourceType_EXO))
            {
                ruleInfoObj.EXORule.RuleFilters.Add(await BuildRuleFilterAsync(ruleInfo, ruleInfoObj.EXORule));
            }
            if (CheckRuleSource(ruleSourceType, SourceType_PHY))
            {
                ruleInfoObj.PhysicalRule.RuleFilters.Add(await BuildRuleFilterAsync(ruleInfo, ruleInfoObj.PhysicalRule));
            }
            if (CheckRuleSource(ruleSourceType, SourceType_FS))
            {
                ruleInfoObj.FSRule.RuleFilters.Add(await BuildRuleFilterAsync(ruleInfo, ruleInfoObj.FSRule));
            }
            if (CheckRuleSource(ruleSourceType, SourceType_SPLocal))
            {
                ruleInfoObj.SPLocalRule.RuleFilters.Add(await BuildRuleFilterAsync(ruleInfo, ruleInfoObj.SPLocalRule));
            }
            if (CheckRuleSource(ruleSourceType, SourceType_OneDrive))
            {
                ruleInfoObj.OneDriveRule.RuleFilters.Add(await BuildRuleFilterAsync(ruleInfo, ruleInfoObj.OneDriveRule));
            }
            if (CheckRuleSource(ruleSourceType, SourceType_AzureFile))
            {
                ruleInfoObj.AzureFileRule.RuleFilters.Add(await BuildRuleFilterAsync(ruleInfo, ruleInfoObj.AzureFileRule));
            }
            if (CheckRuleSource(ruleSourceType, SourceType_Connector))
            {
                ruleInfoObj.ConnectorRule.RuleFilters.Add(await BuildRuleFilterAsync(ruleInfo, ruleInfoObj.ConnectorRule));
            }
            if (CheckRuleSource(ruleSourceType, SourceType_Box))
            {
                ruleInfoObj.BoxRule.RuleFilters.Add(await BuildRuleFilterAsync(ruleInfo, ruleInfoObj.BoxRule));
            }
            if (CheckRuleSource(ruleSourceType, Google_Drive))
            {
                ruleInfoObj.GoogleDriveRule.RuleFilters.Add(await BuildRuleFilterAsync(ruleInfo, ruleInfoObj.GoogleDriveRule));
            }
            return ruleInfoObj;
        }

        public void InitCriteriaRelationship()
        {
            #region level and criteria types
            List<ArchiverFilterRuleType> SCCriteriaType = new List<ArchiverFilterRuleType>()
            {
                ArchiverFilterRuleType.URL, ArchiverFilterRuleType.Title, ArchiverFilterRuleType.ModifiedTime,
                ArchiverFilterRuleType.CreatedTime, ArchiverFilterRuleType.PrimaryAdministrator,
                ArchiverFilterRuleType.SiteCollectionSizeTrigger, ArchiverFilterRuleType.TextCustomProperty,
                ArchiverFilterRuleType.NumberCustomProperty, ArchiverFilterRuleType.BooleanCustomProperty,
                ArchiverFilterRuleType.DateTimeCustomProperty
            };
            List<ArchiverFilterRuleType> siteCriteriaType = new List<ArchiverFilterRuleType>()
            {
                ArchiverFilterRuleType.URL, ArchiverFilterRuleType.Title, ArchiverFilterRuleType.ModifiedTime,
                ArchiverFilterRuleType.CreatedTime, ArchiverFilterRuleType.TextCustomProperty,
                ArchiverFilterRuleType.NumberCustomProperty, ArchiverFilterRuleType.BooleanCustomProperty,
                ArchiverFilterRuleType.DateTimeCustomProperty
            };
            List<ArchiverFilterRuleType> listCriteriaType = new List<ArchiverFilterRuleType>()
            {
                ArchiverFilterRuleType.Name, ArchiverFilterRuleType.ModifiedTime,
                ArchiverFilterRuleType.CreatedTime, ArchiverFilterRuleType.TextCustomProperty,
                ArchiverFilterRuleType.NumberCustomProperty, ArchiverFilterRuleType.BooleanCustomProperty,
                ArchiverFilterRuleType.DateTimeCustomProperty
            };
            List<ArchiverFilterRuleType> folderCriteriaType = new List<ArchiverFilterRuleType>()
            {
                ArchiverFilterRuleType.Name, ArchiverFilterRuleType.ModifiedTime,
                ArchiverFilterRuleType.CreatedTime, ArchiverFilterRuleType.CreatedBy,
                ArchiverFilterRuleType.ContentType, ArchiverFilterRuleType.TextColumn,
                ArchiverFilterRuleType.NumberColumn, ArchiverFilterRuleType.BooleanColumn,
                ArchiverFilterRuleType.DateTimeColumn, ArchiverFilterRuleType.OrphanedFolderRule
            };
            List<ArchiverFilterRuleType> itemCriteriaType = new List<ArchiverFilterRuleType>()
            {
                ArchiverFilterRuleType.Title, ArchiverFilterRuleType.ModifiedTime,
                ArchiverFilterRuleType.CreatedTime, ArchiverFilterRuleType.CreatedBy,
                ArchiverFilterRuleType.ModifiedBy, ArchiverFilterRuleType.ContentType,
                ArchiverFilterRuleType.TextColumn, ArchiverFilterRuleType.NumberColumn,
                ArchiverFilterRuleType.BooleanColumn, ArchiverFilterRuleType.DateTimeColumn,
                ArchiverFilterRuleType.ParentListTypeID
            };
            List<ArchiverFilterRuleType> documentCriteriaType = new List<ArchiverFilterRuleType>()
            {
                ArchiverFilterRuleType.Name, ArchiverFilterRuleType.DocumentSize, ArchiverFilterRuleType.ModifiedTime,
                ArchiverFilterRuleType.CreatedTime, ArchiverFilterRuleType.CreatedBy,
                ArchiverFilterRuleType.ModifiedBy, ArchiverFilterRuleType.ContentType,
                ArchiverFilterRuleType.TextColumn, ArchiverFilterRuleType.NumberColumn,
                ArchiverFilterRuleType.BooleanColumn, ArchiverFilterRuleType.DateTimeColumn,
                ArchiverFilterRuleType.ParentListTypeID, ArchiverFilterRuleType.LastAccessedTime,ArchiverFilterRuleType.LastActiveTime,
                ArchiverFilterRuleType.ParentFolderName,ArchiverFilterRuleType.ParentFolderNameHeirarchically,ArchiverFilterRuleType.ParentLibraryName,ArchiverFilterRuleType.SensitivityLabel,
                ArchiverFilterRuleType.RetentionLabel, ArchiverFilterRuleType.MetadataTextColumn, ArchiverFilterRuleType.MetadataNumberColumn,
                ArchiverFilterRuleType.SensitivityLabelFullName,
                ArchiverFilterRuleType.ParentLibraryText, ArchiverFilterRuleType.ParentLibraryNumber,
                ArchiverFilterRuleType.ParentLibraryBoolean, ArchiverFilterRuleType.ParentLibraryDateTime,
                ArchiverFilterRuleType.ParentSiteCollectionText, ArchiverFilterRuleType.ParentSiteCollectionNumber,
                ArchiverFilterRuleType.ParentSiteCollectionBoolean, ArchiverFilterRuleType.ParentSiteCollectionDateTime,
                ArchiverFilterRuleType.PropertyBagText, ArchiverFilterRuleType.PropertyBagNumber,
                ArchiverFilterRuleType.PropertyBagBoolean, ArchiverFilterRuleType.PropertyBagDateTime
            };
            List<ArchiverFilterRuleType> exoItemCriteriaType = new List<ArchiverFilterRuleType>()
            {
                ArchiverFilterRuleType.Subject, ArchiverFilterRuleType.AttachmentCount,
                ArchiverFilterRuleType.Size, ArchiverFilterRuleType.SendDateUTC,
                ArchiverFilterRuleType.SendFrom, ArchiverFilterRuleType.SendTo, ArchiverFilterRuleType.RetentionLabel, ArchiverFilterRuleType.SensitivityLabel
            };
            List<ArchiverFilterRuleType> fsFileCriteriaType = new List<ArchiverFilterRuleType>()
            {
                ArchiverFilterRuleType.Name, ArchiverFilterRuleType.DocumentSize, ArchiverFilterRuleType.ModifiedTime,
                ArchiverFilterRuleType.CreatedTime, ArchiverFilterRuleType.LastAccessedTime,
                ArchiverFilterRuleType.Type, ArchiverFilterRuleType.Owner,ArchiverFilterRuleType.FilePath,
                ArchiverFilterRuleType.TextColumn, ArchiverFilterRuleType.NumberColumn,ArchiverFilterRuleType.DateTimeColumn
            };
            List<ArchiverFilterRuleType> azureFileCriteriaType = new List<ArchiverFilterRuleType>()
            {
                ArchiverFilterRuleType.Name, ArchiverFilterRuleType.DocumentSize, ArchiverFilterRuleType.ModifiedTime,
                ArchiverFilterRuleType.CreatedTime, ArchiverFilterRuleType.LastAccessedTime,
                ArchiverFilterRuleType.Type,ArchiverFilterRuleType.FilePath
            };
            List<ArchiverFilterRuleType> boxCriteriaType = new List<ArchiverFilterRuleType>()
            {
                ArchiverFilterRuleType.Name, ArchiverFilterRuleType.DocumentSize, 
                ArchiverFilterRuleType.ModifiedTime, ArchiverFilterRuleType.CreatedTime,
                ArchiverFilterRuleType.Type,ArchiverFilterRuleType.FilePath
            };
            List<ArchiverFilterRuleType> googleCriteriaType = new List<ArchiverFilterRuleType>()
            {
                ArchiverFilterRuleType.Name, ArchiverFilterRuleType.DocumentSize, ArchiverFilterRuleType.ModifiedTime,
                ArchiverFilterRuleType.CreatedTime, ArchiverFilterRuleType.CreatedBy,
                ArchiverFilterRuleType.ModifiedBy, ArchiverFilterRuleType.ContentType,
                ArchiverFilterRuleType.TextLabelProperty, ArchiverFilterRuleType.NumberLabelProperty,
                ArchiverFilterRuleType.BooleanColumn, ArchiverFilterRuleType.DateTimeLabelProperty,
                ArchiverFilterRuleType.LabelName
            };
            mLevelAndCriteriaTypeDic = new Dictionary<PolicyLevel, List<ArchiverFilterRuleType>>();
            mLevelAndCriteriaTypeDic.Add(PolicyLevel.SiteCollection, SCCriteriaType);
            mLevelAndCriteriaTypeDic.Add(PolicyLevel.Site, siteCriteriaType);
            mLevelAndCriteriaTypeDic.Add(PolicyLevel.List, listCriteriaType);
            mLevelAndCriteriaTypeDic.Add(PolicyLevel.Folder, folderCriteriaType);
            mLevelAndCriteriaTypeDic.Add(PolicyLevel.Item, itemCriteriaType);
            mLevelAndCriteriaTypeDic.Add(PolicyLevel.Document, documentCriteriaType);
            mLevelAndCriteriaTypeDic.Add(PolicyLevel.ExchangeOnlineItem_Message, exoItemCriteriaType);
            mLevelAndCriteriaTypeDic.Add(PolicyLevel.FileSysFile, fsFileCriteriaType);
            mLevelAndCriteriaTypeDic.Add(PolicyLevel.AzureFileDocument, azureFileCriteriaType);
            mLevelAndCriteriaTypeDic.Add(PolicyLevel.BoxDocument, boxCriteriaType);
            mLevelAndCriteriaTypeDic.Add(PolicyLevel.GoogleDriveDocument, googleCriteriaType);
            #endregion

            List<ArchiverFilterRuleType> physicalFoldCriteriaType = new List<ArchiverFilterRuleType>()
            {
                ArchiverFilterRuleType.Name, ArchiverFilterRuleType.ModifiedTime,
                ArchiverFilterRuleType.CreatedTime, ArchiverFilterRuleType.CreatedBy,
                ArchiverFilterRuleType.ModifiedBy, ArchiverFilterRuleType.TextColumn,
                ArchiverFilterRuleType.DateTimeColumn
            };
            List<ArchiverFilterRuleType> physicalBoxCriteriaType = new List<ArchiverFilterRuleType>()
            {
                ArchiverFilterRuleType.Name, ArchiverFilterRuleType.ModifiedTime,
                ArchiverFilterRuleType.CreatedTime, ArchiverFilterRuleType.CreatedBy,
                ArchiverFilterRuleType.ModifiedBy, ArchiverFilterRuleType.TextColumn,
                ArchiverFilterRuleType.DateTimeColumn, ArchiverFilterRuleType.LastestSubfolderDisposalDate,
            };
            mLevelAndCriteriaTypeDicForPhy = new Dictionary<PolicyLevel, List<ArchiverFilterRuleType>>();
            mLevelAndCriteriaTypeDicForPhy.Add(PolicyLevel.PhysicalFile, physicalFoldCriteriaType);
            mLevelAndCriteriaTypeDicForPhy.Add(PolicyLevel.PhysicalBox, physicalBoxCriteriaType);

            #region criteria type and condition
            List<ArchiverFilterCondition> nameConditions = new List<ArchiverFilterCondition>()
            {
                ArchiverFilterCondition.Matches, ArchiverFilterCondition.DoesNotMatch,
                ArchiverFilterCondition.Contains, ArchiverFilterCondition.DoesNotContain,
                (ArchiverFilterCondition)262936, ArchiverFilterCondition.DoesNotEqual,
                ArchiverFilterCondition.IsEmpty,
                //ArchiverFilterCondition.Equals,
            };
            List<ArchiverFilterCondition> textColumConditions;
            if (isJPMCOpen)
            {
                textColumConditions = new List<ArchiverFilterCondition>()
                {
                ArchiverFilterCondition.Matches, ArchiverFilterCondition.DoesNotMatch,
                ArchiverFilterCondition.Contains, ArchiverFilterCondition.DoesNotContain,
                (ArchiverFilterCondition)262936, ArchiverFilterCondition.DoesNotEqual,
                ArchiverFilterCondition.ListIn,
                    //ArchiverFilterCondition.Equals,
                };
            }
            else
            {
                textColumConditions = new List<ArchiverFilterCondition>()
                {
                ArchiverFilterCondition.Matches, ArchiverFilterCondition.DoesNotMatch,
                ArchiverFilterCondition.Contains, ArchiverFilterCondition.DoesNotContain,
                (ArchiverFilterCondition)262936, ArchiverFilterCondition.DoesNotEqual,
                    //ArchiverFilterCondition.Equals,
                };
            }
            List<ArchiverFilterCondition> docSizeConditions = new List<ArchiverFilterCondition>()
            {
                ArchiverFilterCondition.GreaterThanOrEqualTo, ArchiverFilterCondition.LessThanOrEqualTo
            };
            List<ArchiverFilterCondition> timeConditions = new List<ArchiverFilterCondition>()
            {
                ArchiverFilterCondition.FromTo, ArchiverFilterCondition.Before, ArchiverFilterCondition.OlderThan
            };
            List<ArchiverFilterCondition> userConditions = new List<ArchiverFilterCondition>()
            {
                (ArchiverFilterCondition)262936, ArchiverFilterCondition.Contains
            };
            List<ArchiverFilterCondition> numberConditions = new List<ArchiverFilterCondition>()
            {
                (ArchiverFilterCondition)262936, ArchiverFilterCondition.GreaterThanOrEqualTo, ArchiverFilterCondition.LessThanOrEqualTo
            };
            List<ArchiverFilterCondition> boolenConditions = new List<ArchiverFilterCondition>()
            {
                (ArchiverFilterCondition)262936
            };
            List<ArchiverFilterCondition> listIdConditions = new List<ArchiverFilterCondition>()
            {
                (ArchiverFilterCondition)262936, ArchiverFilterCondition.DoesNotEqual
            };
            List<ArchiverFilterCondition> SCSizeTiggerConditions = new List<ArchiverFilterCondition>()
            {
                ArchiverFilterCondition.GreaterThanOrEqualTo
            };
            List<ArchiverFilterCondition> labelNameConditions = new List<ArchiverFilterCondition>()
            {
                (ArchiverFilterCondition)262936,
                ArchiverFilterCondition.IsEmpty,
                ArchiverFilterCondition.DoesNotEqual,
            };
            List<ArchiverFilterCondition> sensitivityLabelConditions = new List<ArchiverFilterCondition>()
            {
                (ArchiverFilterCondition)262936,
                ArchiverFilterCondition.IsEmpty,
                ArchiverFilterCondition.DoesNotEqual,
                ArchiverFilterCondition.Matches,
                ArchiverFilterCondition.DoesNotMatch,
            };
            mCriteriaAndConditionDic = new Dictionary<ArchiverFilterRuleType, List<ArchiverFilterCondition>>();
            mCriteriaAndConditionDic.Add(ArchiverFilterRuleType.Name, nameConditions);
            mCriteriaAndConditionDic.Add(ArchiverFilterRuleType.DocumentSize, docSizeConditions);
            mCriteriaAndConditionDic.Add(ArchiverFilterRuleType.CreatedTime, timeConditions);
            mCriteriaAndConditionDic.Add(ArchiverFilterRuleType.ModifiedTime, timeConditions);
            mCriteriaAndConditionDic.Add(ArchiverFilterRuleType.CreatedBy, userConditions);
            mCriteriaAndConditionDic.Add(ArchiverFilterRuleType.ModifiedBy, userConditions);
            mCriteriaAndConditionDic.Add(ArchiverFilterRuleType.ContentType, nameConditions);
            mCriteriaAndConditionDic.Add(ArchiverFilterRuleType.TextColumn, textColumConditions);
            mCriteriaAndConditionDic.Add(ArchiverFilterRuleType.NumberColumn, numberConditions);
            mCriteriaAndConditionDic.Add(ArchiverFilterRuleType.BooleanColumn, boolenConditions);
            mCriteriaAndConditionDic.Add(ArchiverFilterRuleType.DateTimeColumn, timeConditions);
            mCriteriaAndConditionDic.Add(ArchiverFilterRuleType.ParentListTypeID, listIdConditions);
            mCriteriaAndConditionDic.Add(ArchiverFilterRuleType.LastAccessedTime, timeConditions);
            mCriteriaAndConditionDic.Add(ArchiverFilterRuleType.LastActiveTime, timeConditions);
            mCriteriaAndConditionDic.Add(ArchiverFilterRuleType.Title, nameConditions);
            mCriteriaAndConditionDic.Add(ArchiverFilterRuleType.URL, nameConditions);
            mCriteriaAndConditionDic.Add(ArchiverFilterRuleType.TextCustomProperty, nameConditions);
            mCriteriaAndConditionDic.Add(ArchiverFilterRuleType.NumberCustomProperty, numberConditions);
            mCriteriaAndConditionDic.Add(ArchiverFilterRuleType.BooleanCustomProperty, boolenConditions);
            mCriteriaAndConditionDic.Add(ArchiverFilterRuleType.DateTimeCustomProperty, timeConditions);
            mCriteriaAndConditionDic.Add(ArchiverFilterRuleType.PrimaryAdministrator, userConditions);
            mCriteriaAndConditionDic.Add(ArchiverFilterRuleType.SiteCollectionSizeTrigger, SCSizeTiggerConditions);
            mCriteriaAndConditionDic.Add(ArchiverFilterRuleType.ParentFolderName, nameConditions);
            mCriteriaAndConditionDic.Add(ArchiverFilterRuleType.ParentFolderNameHeirarchically, nameConditions);
            mCriteriaAndConditionDic.Add(ArchiverFilterRuleType.Subject, nameConditions);
            mCriteriaAndConditionDic.Add(ArchiverFilterRuleType.AttachmentCount, docSizeConditions);
            mCriteriaAndConditionDic.Add(ArchiverFilterRuleType.Size, docSizeConditions);
            mCriteriaAndConditionDic.Add(ArchiverFilterRuleType.SendDateUTC, timeConditions);
            mCriteriaAndConditionDic.Add(ArchiverFilterRuleType.SendFrom, userConditions);
            mCriteriaAndConditionDic.Add(ArchiverFilterRuleType.SendTo, userConditions);
            mCriteriaAndConditionDic.Add(ArchiverFilterRuleType.Type, nameConditions);
            mCriteriaAndConditionDic.Add(ArchiverFilterRuleType.Owner, nameConditions);
            mCriteriaAndConditionDic.Add(ArchiverFilterRuleType.FilePath, nameConditions);
            mCriteriaAndConditionDic.Add(ArchiverFilterRuleType.RetentionLabel, sensitivityLabelConditions);
            mCriteriaAndConditionDic.Add(ArchiverFilterRuleType.SensitivityLabelFullName, sensitivityLabelConditions);
            mCriteriaAndConditionDic.Add(ArchiverFilterRuleType.ParentLibraryName, nameConditions);
            mCriteriaAndConditionDic.Add(ArchiverFilterRuleType.SensitivityLabel, sensitivityLabelConditions);
            mCriteriaAndConditionDic.Add(ArchiverFilterRuleType.MetadataTextColumn, nameConditions);
            mCriteriaAndConditionDic.Add(ArchiverFilterRuleType.MetadataNumberColumn, numberConditions);
            mCriteriaAndConditionDic.Add(ArchiverFilterRuleType.OrphanedFolderRule, boolenConditions);

            var sensitiveLabelSource = new List<SourceFlag> { SourceFlag.OneDrive, SourceFlag.SharePoint, SourceFlag.Exchange, SourceFlag.Teams };
            var retentionLabelSource = new List<SourceFlag> { SourceFlag.OneDrive, SourceFlag.SharePoint, SourceFlag.Exchange };
            mSourceAndCriteriaTypeDic = new Dictionary<ArchiverFilterRuleType, List<SourceFlag>>
            {
                { ArchiverFilterRuleType.SensitivityLabel, sensitiveLabelSource },
                { ArchiverFilterRuleType.RetentionLabel, retentionLabelSource },
                { ArchiverFilterRuleType.OrphanedFolderRule, new List<SourceFlag> {SourceFlag.SharePoint} }
            };
            mCriteriaAndConditionDic.Add(ArchiverFilterRuleType.DateTimeLabelProperty, timeConditions);
            mCriteriaAndConditionDic.Add(ArchiverFilterRuleType.NumberLabelProperty, numberConditions);
            mCriteriaAndConditionDic.Add(ArchiverFilterRuleType.TextLabelProperty, nameConditions);
            mCriteriaAndConditionDic.Add(ArchiverFilterRuleType.LabelName, nameConditions);
            
            mCriteriaAndConditionDic.Add(ArchiverFilterRuleType.ParentLibraryText, nameConditions);
            mCriteriaAndConditionDic.Add(ArchiverFilterRuleType.ParentLibraryNumber, numberConditions);
            mCriteriaAndConditionDic.Add(ArchiverFilterRuleType.ParentLibraryBoolean, boolenConditions);
            mCriteriaAndConditionDic.Add(ArchiverFilterRuleType.ParentLibraryDateTime, timeConditions);

            mCriteriaAndConditionDic.Add(ArchiverFilterRuleType.ParentSiteCollectionText, nameConditions);
            mCriteriaAndConditionDic.Add(ArchiverFilterRuleType.ParentSiteCollectionNumber, numberConditions);
            mCriteriaAndConditionDic.Add(ArchiverFilterRuleType.ParentSiteCollectionBoolean, boolenConditions);
            mCriteriaAndConditionDic.Add(ArchiverFilterRuleType.ParentSiteCollectionDateTime, timeConditions);

            mCriteriaAndConditionDic.Add(ArchiverFilterRuleType.PropertyBagText, nameConditions);
            mCriteriaAndConditionDic.Add(ArchiverFilterRuleType.PropertyBagNumber, numberConditions);
            mCriteriaAndConditionDic.Add(ArchiverFilterRuleType.PropertyBagBoolean, boolenConditions);
            mCriteriaAndConditionDic.Add(ArchiverFilterRuleType.PropertyBagDateTime, timeConditions);
            mCriteriaAndConditionDic.Add(ArchiverFilterRuleType.LastestSubfolderDisposalDate, timeConditions);

            #endregion
            //#region 
        }

        public void IsRuleRight(RMRuleInfos rule)
        {
            if (rule.IsSpSource)
            {
                CheckRuleFilter(rule, source: SourceFlag.SharePoint);
            }
            if (rule.IsExoSource)
            {
                CheckRuleFilter(rule.EXORule, true, source: SourceFlag.Exchange);
            }
            if (rule.IsPhySource)
            {
                CheckRuleFilter(rule.PhysicalRule, false, true, source: SourceFlag.Physical);
            }
            if (rule.IsFSSource)
            {
                CheckRuleFilter(rule.FSRule, source: SourceFlag.FileSystem);
            }
            if (rule.IsSPLocalSource)
            {
                CheckRuleFilter(rule.SPLocalRule, source: SourceFlag.SharePointOnPrem);
            }
            if (rule.IsOneDriveSource)
            {
                CheckRuleFilter(rule.OneDriveRule, source: SourceFlag.OneDrive);
            }
            if (rule.IsAzureFileSource)
            {
                CheckRuleFilter(rule.AzureFileRule, source: SourceFlag.AzureFileShare);
            }
            if (rule.IsConnectorSource)
            {
                CheckRuleFilter(rule.ConnectorRule, source: SourceFlag.Connector);
            }
            if (rule.IsBoxSource)
            {
                CheckRuleFilter(rule.BoxRule, source: SourceFlag.Box);
            }
            if (rule.IsGoogleDriveSource)
            {
                CheckRuleFilter(rule.GoogleDriveRule, source: SourceFlag.Google);
            }
            if(rule.IsTeamsSource)
            {
                CheckRuleFilter(rule.TeamsRule, source: SourceFlag.Teams);
            }
        }

        private void CheckRuleFilter(RMRuleInfos ruleInfo, bool isEXOFilter = false, bool isPhyFilter = false, SourceFlag source = SourceFlag.None)
        {
            if (ruleInfo != null)
            {
                foreach (var filter in ruleInfo.RuleFilters)
                {
                    IsFilterRight(filter, isEXOFilter, isPhyFilter, source);
                }
            }
        }

        public void IsFilterRight(RuleFilter filter, bool isEXOFilter,bool isPhyFilter = false, SourceFlag source = SourceFlag.None)
        {
            if (filter.RuleType == ArchiverFilterRuleType.Size && !string.IsNullOrEmpty(filter.filterName))
            {
                throw new Exception("RM_JS_TM_TermImport_CriteriaNameIsNotNull");
            }
            if (!isPhyFilter)
            {
                if (!mLevelAndCriteriaTypeDic.ContainsKey(filter.Level))
                {
                    throw new Exception("RM_JS_TM_TermImport_RuleLevelErr");
                }
                else
                {
                    var currentCriteria = mLevelAndCriteriaTypeDic[filter.Level].Where(f => f.Equals(filter.RuleType));
                    if (currentCriteria == null || currentCriteria.Count() == 0)
                    {
                        throw new Exception("RM_JS_TM_TermImport_CriteriaTypeErr");
                    }
                }
            }
            else
            {
                if (!mLevelAndCriteriaTypeDicForPhy.ContainsKey(filter.Level))
                {
                    throw new Exception("RM_JS_TM_TermImport_RuleLevelErr");
                }
                else
                {
                    var currentCriteria = mLevelAndCriteriaTypeDicForPhy[filter.Level].Where(f => f.Equals(filter.RuleType));
                    if (currentCriteria == null || currentCriteria.Count() == 0)
                    {
                        throw new Exception("RM_JS_TM_TermImport_CriteriaTypeErr");
                    }
                }
            }
            if (!mCriteriaAndConditionDic.ContainsKey(filter.RuleType))
            {
                throw new Exception("RM_JS_TM_TermImport_CriteriaTypeErr");
            }
            else
            {
                var currentCondition = mCriteriaAndConditionDic[filter.RuleType].Where(f => f.Equals(filter.Condition));
                if (currentCondition == null || currentCondition.Count() == 0)
                {
                    throw new Exception("RM_JS_TM_TermImport_ConditionErr");
                }
            }

            if(mSourceAndCriteriaTypeDic.ContainsKey(filter.RuleType))
            {
                if (!mSourceAndCriteriaTypeDic[filter.RuleType].Contains(source))
                {
                    throw new Exception("RM_JS_TM_TermImport_CriteriaTypeErr");
                }
            }

            if (filter.RuleType == ArchiverFilterRuleType.MetadataTextColumn || filter.RuleType == ArchiverFilterRuleType.MetadataNumberColumn)
            {
                if (source != SourceFlag.SharePoint)
                {
                    throw new Exception("RM_JS_TM_TermImport_CriteriaTypeErr");
                }
            }

            #region check value and uint
            int conditionIntValue;
            switch (filter.RuleType)
            {
                case ArchiverFilterRuleType.DocumentSize:
                case ArchiverFilterRuleType.SiteCollectionSizeTrigger:
                    if (filter.Value1Unit != PolicyValueUnit.KB && filter.Value1Unit != PolicyValueUnit.MB && filter.Value1Unit != PolicyValueUnit.GB)
                    {
                        throw new Exception("RM_JS_TM_TermImport_ValueUnitErr");
                    }
                    if (!int.TryParse(filter.Value1, out conditionIntValue))
                    {
                        throw new Exception("RM_JS_TM_TermImport_ValueErr");
                    }
                    break;
                case ArchiverFilterRuleType.Size:
                    if (isEXOFilter && filter.Value1Unit != PolicyValueUnit.KB && filter.Value1Unit != PolicyValueUnit.MB)
                    {
                        throw new Exception("RM_JS_TM_TermImport_ValueUnitErr");
                    }
                    if (!isEXOFilter && filter.Value1Unit != PolicyValueUnit.KB && filter.Value1Unit != PolicyValueUnit.MB && filter.Value1Unit != PolicyValueUnit.GB)
                    {
                        throw new Exception("RM_JS_TM_TermImport_ValueUnitErr");
                    }
                    if (!int.TryParse(filter.Value1, out conditionIntValue))
                    {
                        throw new Exception("RM_JS_TM_TermImport_ValueErr");
                    }
                    break;
                case ArchiverFilterRuleType.BooleanColumn:
                case ArchiverFilterRuleType.BooleanCustomProperty:
                    if (!filter.Value1.Equals("yes", StringComparison.OrdinalIgnoreCase) && !filter.Value1.Equals("no", StringComparison.OrdinalIgnoreCase))
                    {
                        throw new Exception("RM_JS_TM_TermImport_ValueErr");
                    }
                    break;
                case ArchiverFilterRuleType.RetentionLabel:
                case ArchiverFilterRuleType.SensitivityLabel:
                    if (isEXOFilter && filter.Condition == ArchiverFilterCondition.DoesNotEqual)
                    {
                        var IsNestleCustomize = RMKeyValueDao.GetValueByKey("IsNestleCustomize");
                        if (IsNestleCustomize != null)
                        {
                            _ = bool.TryParse(IsNestleCustomize.Value, out var result);
                            if (!result)
                            {
                                throw new Exception("RM_JS_TM_TermImport_ConditionErr");
                            }
                        }
                        else
                        {
                            throw new Exception("RM_JS_TM_TermImport_ConditionErr");
                        }
                    }
                    break;
                case ArchiverFilterRuleType.NumberColumn:
                case ArchiverFilterRuleType.MetadataNumberColumn:
                case ArchiverFilterRuleType.NumberCustomProperty:
                    if (!double.TryParse(filter.Value1, out _))
                    {
                        throw new Exception("RM_JS_TM_TermImport_ValueErr");
                    }
                    break;
                default:
                    break;
            }

            DateTime startTime;
            DateTime endTime;
            switch (filter.Condition)
            {
                //case ArchiverFilterCondition.Equals:
                //    break;
                case (ArchiverFilterCondition)262936:
                    break;
                case ArchiverFilterCondition.DoesNotEqual:
                    break;
                case ArchiverFilterCondition.GreaterThanOrEqualTo:
                    break;
                case ArchiverFilterCondition.LessThanOrEqualTo:
                    break;
                case ArchiverFilterCondition.FromTo:
                    if (filter.RuleType != ArchiverFilterRuleType.DateTimeLabelProperty)
                    {
                        if (DateTime.TryParse(filter.Value1, out startTime) && DateTime.TryParse(filter.Value2, out endTime))
                        {
                            if (startTime >= endTime)
                            {
                                throw new Exception("RM_JS_RDM_CreateRule_Validation_ConditionDateTime");
                            }
                        }
                        else
                        {
                            throw new Exception("RM_JS_TM_TermImport_ValueErr");
                        }

                        break;
                    }
                    
                    if (DateTime.TryParse(filter.Value2, out startTime) && DateTime.TryParse(filter.Value3, out endTime))
                    {
                        if (startTime >= endTime)
                        {
                            throw new Exception("RM_JS_RDM_CreateRule_Validation_ConditionDateTime");
                        }
                    }
                    else
                    {
                        throw new Exception("RM_JS_TM_TermImport_ValueErr");
                    }
                    
                    break;
                case ArchiverFilterCondition.Before:
                    if (filter.RuleType != ArchiverFilterRuleType.DateTimeLabelProperty)
                    {
                        if (!DateTime.TryParse(filter.Value1, out startTime))
                        {
                            throw new Exception("RM_JS_TM_TermImport_ValueErr");
                        }
                        break;
                    }
                    if (!DateTime.TryParse(filter.Value2, out startTime))
                    {
                        throw new Exception("RM_JS_TM_TermImport_ValueErr");
                    }

                    break;
                //check value uint
                case ArchiverFilterCondition.OlderThan:
                    if (filter.RuleType != ArchiverFilterRuleType.DateTimeLabelProperty)
                    {
                        if (filter.Value1Unit != PolicyValueUnit.Days && filter.Value1Unit != PolicyValueUnit.Weeks
                      && filter.Value1Unit != PolicyValueUnit.Months && filter.Value1Unit != PolicyValueUnit.Years)
                        {
                            throw new Exception("RM_JS_TM_TermImport_ValueUnitErr");
                        }
                        if (!int.TryParse(filter.Value1, out conditionIntValue))
                        {
                            throw new Exception("RM_JS_TM_TermImport_ValueErr");
                        }

                        break;
                    }
                    
                    if (filter.Value2Unit != PolicyValueUnit.Days && filter.Value2Unit != PolicyValueUnit.Weeks && filter.Value2Unit != PolicyValueUnit.Months && filter.Value2Unit != PolicyValueUnit.Years)
                    {
                        throw new Exception("RM_JS_TM_TermImport_ValueUnitErr");
                    }
                    if (!int.TryParse(filter.Value2, out conditionIntValue))
                    {
                        throw new Exception("RM_JS_TM_TermImport_ValueErr");
                    }
                    
                    break;
                case ArchiverFilterCondition.IsEmpty:
                    if (!CheckFilterRuleIsBlankCondition(filter.RuleType))
                    {
                        throw new Exception("RM_JS_TM_TermImport_NoConditionValue");
                    }
                    break;
                default:
                    break;
            }
            #endregion
        }

        public string ConvertCombineMode(string input)
        {
            string result = string.Empty;
            switch (input)
            {
                case "Any":
                    result = "Or";
                    break;
                case "All":
                    result = "And";
                    break;
                case "Or":
                    result = "Or";
                    break;
                case "And":
                    result = "And";
                    break;
                default:
                    result = "And";
                    break;
            }
            return result;
        }

        public void AddJobDetail(string name, string action, JobDetailsStatus status, string comment = "")
        {
            JMImportTermDetail detail = new JMImportTermDetail();
            detail.Action = action;
            detail.Term = name;
            detail.Status = status;
            detail.Comment = comment;
            mDetails.Add(detail);
        }

        private Dictionary<string, int> GetImportSheetColumnCounts()
        {
            return new Dictionary<string, int>
            {
                { SheetName_Term, GetImportTermSheetColumnCount() },
                { SheetName_Rule, GetImportRuleSheetColumnCount() }
            };
        }

        private int GetImportTermSheetColumnCount()
        {
            int columnCount = ReportUtil.GetRowColumn(SheetName_Term);
            if (ShouldReadTemplateNotesColumn())
            {
                columnCount++;
            }
            if (isJPMCOpen)
            {
                columnCount++;
            }
            if (HasUpgradeTeams)
            {
                columnCount++;
            }
            return columnCount;
        }

        private int GetImportRuleSheetColumnCount()
        {
            int columnCount = ReportUtil.GetRowColumn(SheetName_Rule);
            if (ShouldReadTemplateNotesColumn())
            {
                columnCount++;
            }
            if (isSupportRecordLabel)
            {
                columnCount += IndexChangeFromLabel;
            }
            return columnCount;
        }

        private bool ShouldReadTemplateNotesColumn()
        {
            return isJPMCOpen || HasUpgradeTeams || isSupportRecordLabel;
        }

        public async Task ProcessExcelAsync(string filePath)
        {
            if (!isControlPlus)
            {
                string templateVersion = ExcelUtil.GetCustomProperty(filePath, TermAndRuleTemplateVersion.PROPERTIES_KEY);
                if (!TermAndRuleTemplateVersion.PROPERTIES_VALUE.Equals(templateVersion, StringComparison.OrdinalIgnoreCase))
                {
                    isRightTemplateVersion = false;
                    throw new TermCsvFormateExcetion($"now excel template is old version, ver:{templateVersion}");
                }
            }

            Dictionary<string, List<string[]>> datas = new Dictionary<string, List<string[]>>();
            Dictionary<string, List<string[]>> headers = new Dictionary<string, List<string[]>>();
            try
            {
                using (FileStream fs = new FileStream(filePath, FileMode.OpenOrCreate, FileAccess.ReadWrite))
                {
                    Dictionary<string, int> sheetNameCountDic = GetImportSheetColumnCounts();
                    datas = ExcelUtil.ReadExcel(fs, sheetNameCountDic);
                    headers = ExcelUtil.ReadExcelHeader(fs, sheetNameCountDic);
                }
            }
            catch (OpenXmlPackageException e)
            {
                if (e.ToString().Contains("Invalid Hyperlink") || e.ToString().Contains("Invalid URI"))
                {
                    using (FileStream fs = new FileStream(filePath, FileMode.OpenOrCreate, FileAccess.ReadWrite))
                    {
                        UriFixer.FixInvalidUri(fs, brokenUri => UriFixer.FixUri(brokenUri));
                    }
                    using (FileStream fs = new FileStream(filePath, FileMode.OpenOrCreate, FileAccess.ReadWrite))
                    {
                        Dictionary<string, int> sheetNameCountDic = GetImportSheetColumnCounts();
                        datas = ExcelUtil.ReadExcel(fs, sheetNameCountDic);
                    }
                }
            }

            if (!datas.ContainsKey(RuleSheetName) && !datas.ContainsKey(TermSheetName))
            {
                throw new TermCsvFormateExcetion("Excel format is not right.");
            }

            Dictionary<Guid, RMRuleInfos> allRuleDic = new Dictionary<Guid, RMRuleInfos>();

            InitializeExpectedDetailCount(
                (datas.ContainsKey(RuleSheetName) ? datas[RuleSheetName].Count : 0) +
                (datas.ContainsKey(TermSheetName) ? datas[TermSheetName].Count : 0));
            if (datas.ContainsKey(RuleSheetName))
            {
                datas[RuleSheetName] = FilterRepteadedRules(datas[RuleSheetName]);
                await ProcessRuleAsync(datas[RuleSheetName], allRuleDic);
            }
            if (datas.ContainsKey(TermSheetName))
            {
                datas[TermSheetName] = FilterEmptyTerms(datas[TermSheetName]);
                await ProcessTermsAsync(datas[TermSheetName], allRuleDic);
            }
        }


        //过滤相同的criteria 
        public List<string[]> FilterRepteadedRules(List<string[]> rules)
        {
            mLog.Info("Begin filter rule's criteria");
            List<string[]> result = new List<string[]>();
            foreach (string[] ruleInfo in rules)
            {
                mLog.Info("Begin filter rule's criteria,rule name is {0}", ruleInfo[0]);
                bool isEmptyRow = ruleInfo.Where(t => !string.IsNullOrEmpty(t)).Count() <= 0;
                if (isEmptyRow || string.IsNullOrEmpty(ruleInfo[RulePropertyIndex.Name]))
                {
                    continue;
                }
                bool isContain = result.Where(r => r[RulePropertyIndex.Name] == ruleInfo[RulePropertyIndex.Name] && r[RulePropertyIndex.SourceType] == ruleInfo[RulePropertyIndex.SourceType]).Count() > 0;
                if (!isContain)
                {
                    result.Add(ruleInfo);
                }
                else
                {
                    bool isSameCriteria = true;
                    List<string[]> sameNameRule = result.Where(r => r[RulePropertyIndex.Name] == ruleInfo[RulePropertyIndex.Name] && r[RulePropertyIndex.SourceType] == ruleInfo[RulePropertyIndex.SourceType]).ToList();
                    foreach (string[] temp in sameNameRule)
                    {
                        for (int i = RulePropertyIndex.CriteriaType; i < RulePropertyIndex.RuleAction; i++)
                        {
                            if (temp[i] != ruleInfo[i])
                            {
                                isSameCriteria = false;
                                break;
                            }
                        }
                        if (!isSameCriteria)
                        {
                            break;
                        }
                    }
                    if (!isSameCriteria)
                    {
                        result.Add(ruleInfo);
                    }
                }
            }
            mLog.Info("Finish filter rule's criteria");
            return result;
        }

        public async Task ProcessRuleAsync(List<string[]> rules, Dictionary<Guid, RMRuleInfos> allRuleDic)
        {
            #region convert to rule object
            Dictionary<string, RMRuleInfos> importRuleDic = new Dictionary<string, RMRuleInfos>();
            HashSet<string> failedRuleNames = new HashSet<string>();
            var groupedRules = rules.GroupBy(item => item[0]).ToDictionary(item => item.Key,
                   item => item.ToList().Select(
                       innerItem => new List<string>(innerItem).ToArray()).ToList());
            foreach (var ruleInfo in rules)
            {
                var groupedRuleList = groupedRules[ruleInfo[0]];
                var objectLevelSet = groupedRuleList.Select(item => item[3]).ToHashSet();
                if (objectLevelSet.Count != 1)
                {
                    AddUniqueJobDetail(ruleInfo[RulePropertyIndex.Name], string.Empty, JobDetailsStatus.Failed, "RM_TS_Action_TheSameRuleCanthavemanyObjectLevel");
                    failedRuleNames.Add(ruleInfo[RulePropertyIndex.Name]);
                    continue;
                }
                using (CheckJobStopScope fScope = new CheckJobStopScope())
                {
                    try
                    {
                        bool isEmptyRow = ruleInfo.All(t => string.IsNullOrEmpty(t));
                        if (!isEmptyRow && !string.IsNullOrEmpty(ruleInfo[RulePropertyIndex.Name]))
                        {
                            RMRuleInfos ruleObj = await ConvertToRuleObjectAsync(ruleInfo, importRuleDic);
                         
                            if (string.IsNullOrEmpty(ruleObj.ContainerName))
                            {
                                AddUniqueJobDetail(ruleInfo[RulePropertyIndex.Name], string.Empty, JobDetailsStatus.Failed, "RM_TS_Action_RuleContainerNameIsEmptyError");
                                failedRuleNames.Add(ruleInfo[RulePropertyIndex.Name]);
                            }
                            else
                            {
                                mLog.Info("Convert string to rule object succeed. RuleName:[{0}]", ruleObj.RuleName);
                                if (!importRuleDic.ContainsKey(ruleObj.RuleName))
                                {
                                    importRuleDic.Add(ruleObj.RuleName, ruleObj);
                                }
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        if(e.Message == "RM_JS_TM_TermImport_NoSupportRetentionSensitiveOldAccount")
                        {
                            AddJobDetail(ruleInfo[RulePropertyIndex.Name], string.Empty, JobDetailsStatus.Skipped, e.Message);
                        }
                        else
                        {
                            AddUniqueJobDetail(ruleInfo[RulePropertyIndex.Name], string.Empty, JobDetailsStatus.Failed, e.Message);
                            failedRuleNames.Add(ruleInfo[RulePropertyIndex.Name]);
                        }                       
                        mLog.Error("Convert string to rule object error:{0}", e.ToString());
                    }
                }
            }
            failedRuleNames.ToList().ForEach(name => importRuleDic.Remove(name));
            #endregion

            #region import rule
            var checkGoogleLicense = await CheckGoogleLicenseAsync();
            allRuleDic = (await RuleManagerService.GetRuleInfosFromDAAsync()).ToDictionary(r => new Guid(r.RuleId));
            var allContainers = RuleDao.GetAllRuleContainers();
            foreach (var ruleObj in importRuleDic.Values)
            {
                using (CheckJobStopScope fScope = new CheckJobStopScope())
                {
                    mtotalCount++;
                    string detailAction = string.Empty;
                    RAReturnMessage returnMsg;
                    try
                    {
                        mLog.Info("Import rule:[{0}]", ruleObj.RuleName);
                        string ruleId = allRuleDic.Values.Where(r => r.RuleName.Equals(ruleObj.RuleName))
                                                .Select(r => r.RuleId).FirstOrDefault();
                        if(!ruleObj.IsSpSource && !ruleObj.IsGoogleDriveSource && !ruleObj.IsTeamsSource && !ruleObj.IsFSSource && !ruleObj.IsBoxSource
                            && !ruleObj.IsExoSource && !ruleObj.IsAzureFileSource && !ruleObj.IsPhySource && !ruleObj.IsConnectorSource
                            && !ruleObj.IsOneDriveSource && !ruleObj.IsSPLocalSource)
                        {
                            detailAction = string.IsNullOrEmpty(ruleId) ? "RM_TS_Action_New" : "RM_TS_Action_Update";
                            mLog.Warn($"Current rule does not have the content source valid {ruleObj.RuleName}");
                            AddJobDetail(ruleObj.RuleName, detailAction, JobDetailsStatus.Failed, "RM_JS_TM_RuleImportNoContentSource");
                            continue;
                        }
                        if (ruleObj.ContainerName.Equals(I18NEntity.GetString("RM_RDM_DefaultRuleContainer"), StringComparison.CurrentCultureIgnoreCase))
                        {
                            ruleObj.ContainerName = "RM_RDM_DefaultRuleContainer";
                        }

                        var ruleContaienr = allContainers.FirstOrDefault(c => c.Name.Equals(ruleObj.ContainerName,StringComparison.CurrentCultureIgnoreCase));
                        if (ruleContaienr == null)
                        {
                            var newContainerId = Guid.NewGuid();
                            RuleDao.UpsertRuleContainer(new DB.Model.RMRuleContainer()
                            {
                                ContainerId = newContainerId,
                                Name = ruleObj.ContainerName,
                                IsDefault = false,
                                ModifyTime = DateTime.UtcNow.Ticks
                            });
                            ruleObj.ContainerId = newContainerId;
                            allContainers = RuleDao.GetAllRuleContainers();
                            AddJobDetail(ruleObj.ContainerName, "RM_TS_Action_New", JobDetailsStatus.Successful, "RM_JS_TM_TermImport_ImportRuleContainerSuccessfully");
                        }
                        else
                        {
                            ruleObj.ContainerId = ruleContaienr.ContainerId;
                        }
                        IsRuleRight(ruleObj);
                        ruleObj.ModelType = RuleModel.Records;
                        if (ruleObj.IsGoogleDriveSource && (!checkGoogleLicense && !(_hasGControlLicense && isControlPlus)))
                        {
                            detailAction = string.IsNullOrEmpty(ruleId) ? "RM_TS_Action_New" : "RM_TS_Action_Update";
                            AddJobDetail(ruleObj.RuleName, detailAction, JobDetailsStatus.Failed, "RM_JS_TM_RuleImportNoGoogleLicense");

                            mFailedCount++;
                            continue;
                        }
                        if (string.IsNullOrEmpty(ruleId))
                        {
                            detailAction = "RM_TS_Action_New";
                            returnMsg = await RuleManagerService.CreateImportRuleInDAAsync(ruleObj);
                        }
                        else
                        {
                            detailAction = "RM_TS_Action_Update";
                            ruleObj.RuleId = ruleId;
                            returnMsg = await RuleManagerService.ModifyRuleInDAAsync(ruleObj);
                        }
                        if (returnMsg.MessageType != RAMessageType.Successful)
                        {
                            mFailedCount++;
                            mLog.Error("Import rule error name:[{0}] Error:{1}", ruleObj.RuleName, returnMsg.ErrorMessage);
                            if (returnMsg.ErrorMessage.Equals("RM_RDM_Rule_ConfigureStoragePolicy"))
                            {
                                returnMsg.ErrorMessage = $"RM_RDM_Rule_ConfigureStoragePolicy|I18NSplit|{"RM_JS_CP_StorageSetting"}";
                            }
                            if (returnMsg.ErrorMessage.Contains("An error occurred while validating the destination."))
                            {
                                returnMsg.ErrorMessage = "RM_JS_Rule_SPDestUrlError";
                            }
                            if (returnMsg.ErrorMessage.Contains("This rule is currently in use by a running job."))
                            {
                                returnMsg.ErrorMessage = "RM_JS_RDM_EditRule_UsedByJob";
                            }
                            AddJobDetail(ruleObj.RuleName, detailAction, JobDetailsStatus.Failed, returnMsg.ErrorMessage);
                            continue;
                        }
                        else
                        {
                            mSucceedCount++;
                            var isShowDestroyWithoutBackupMessage = await IsShowDestroyWithoutBackupMessageAsync(ruleObj);
                            AddJobDetail(ruleObj.RuleName, detailAction, JobDetailsStatus.Successful, isShowDestroyWithoutBackupMessage ? "RM_JS_TM_TermImport_ItemIsRule_DestroyDataWithoutBackup" : "RM_JS_TM_TermImport_ItemIsRule");
                            if (!allRuleDic.ContainsKey(new Guid(ruleObj.RuleId)))
                            {
                                allRuleDic.Add(new Guid(ruleObj.RuleId), ruleObj);
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        mFailedCount++;
                        if (e.Message.Contains("An error occurred while validating the destination."))
                        {
                            AddJobDetail(ruleObj.RuleName, detailAction, JobDetailsStatus.Failed, "RM_JS_Rule_SPDestUrlError");
                        }
                        else if(e.Message.Contains("This rule is currently in use by a running job."))
                        {
                            AddJobDetail(ruleObj.RuleName, detailAction, JobDetailsStatus.Failed, "RM_JS_RDM_EditRule_UsedByJob");
                        }
                        else
                        {
                            AddJobDetail(ruleObj.RuleName, detailAction, JobDetailsStatus.Failed, e.Message);
                        }
                        mLog.Error("Import rule error name:[{0}] Error:{1}", ruleObj.RuleName, e.ToString());
                    }
                    UpdateJobDetail(processedIncrement: 1);
                }
            }
            #endregion
        }

        public List<string[]> FilterEmptyTerms(List<string[]> terms)
        {
            mLog.Info("Begin filter term's empty");
            List<string[]> result = new List<string[]>();
            foreach (string[] term in terms)
            {
                mLog.Info("Begin filter term's criteria");
                bool isEmptyRow = term.Where(t => !string.IsNullOrEmpty(t)).Count() <= 0;
                if (isEmptyRow || string.IsNullOrEmpty(term[TermPropertyIndex.TermGroupName]) || string.IsNullOrEmpty(term[TermPropertyIndex.TermSetName]))
                {
                    continue;
                }
                term[0] = term[0] == null ? term[0] : term[0].Replace("&", "＆");
                term[1] = term[1] == null ? term[1] : term[1].Replace("&", "＆");
                term[2] = term[2] == null ? term[2] : term[2].Replace("&", "＆");
                term[3] = term[3] == null ? term[3] : term[3].Replace("&", "＆");
                term[4] = term[4] == null ? term[4] : term[4].Replace("&", "＆");
                term[5] = term[5] == null ? term[5] : term[5].Replace("&", "＆");
                term[6] = term[6] == null ? term[6] : term[6].Replace("&", "＆");
                result.Add(term);
            }
            mLog.Info("Finish filter term's empty");
            return result;
        }

        public async Task ProcessTermsAsync(List<string[]> termInfos, Dictionary<Guid, RMRuleInfos> ruleDic)
        {
            List<string> groups = new List<string>();
            foreach (var termInfo in termInfos)
            {
                using (CheckJobStopScope fScope = new CheckJobStopScope())
                {
                    string detailAction = string.Empty;
                    RMImportTermObject termObject = null;
                    RMImportTermGroupObject termGroupObject = null;
                    RMImportTermSetObject termSetObject = null;
                    try
                    {
                        #region 过滤空行
                        //与Arno确认:只有当前行所有列都空的(包括空格),才跳过;否则如果其他列有值时,如果GroupName为空,则仍要在jobDetail中提示
                        bool bl = termInfo.Any(p => !string.IsNullOrWhiteSpace(p));
                        if (!bl)
                        {
                            mLog.Info($"Import term: blank line");
                            continue;
                        }
                        #endregion

                        if (string.IsNullOrEmpty(termInfo[TermPropertyIndex.TermGroupName]))
                        {
                            //AddJobDetail("Term Group", detailAction, JobDetailsStatus.Failed, "RM_TM_TermGroupIsNull");
                            throw new Exception("RM_TM_TermGroupIsNull");
                        }
                        termGroupObject = ConvertToTermGroupObject(termInfo);
                        if (!groups.Contains(termGroupObject.Name))
                        {
                            CheckNameIsValid(termGroupObject.Name, TermLevel.Group);
                            CheckGroupExitAndCreate(termGroupObject);
                            groups.Add(termGroupObject.Name);
                        }
                        if (string.IsNullOrEmpty(termInfo[TermPropertyIndex.TermSetName]) && !string.IsNullOrEmpty(termInfo[TermPropertyIndex.Level1]))
                        {
                            //AddJobDetail("Term Set", detailAction, JobDetailsStatus.Failed, "RM_TM_TermSetIsNull");
                            throw new Exception("RM_TM_TermSetIsNull");
                        }
                        else if (string.IsNullOrEmpty(termInfo[TermPropertyIndex.TermSetName]) && string.IsNullOrEmpty(termInfo[TermPropertyIndex.Level1]))
                        {
                            continue;
                        }
                        termSetObject = ConvertToTermSetObject(termInfo);
                        CheckNameIsValid(termSetObject.Name, TermLevel.TermSet);
                        int termSetId = CheckTermSetExitAndCreate(termSetObject);

                        if (string.IsNullOrEmpty(termInfo[TermPropertyIndex.Level1]))
                        {
                            continue;
                        }
                        termObject = ConvertToTermObject(termInfo);

                        if (string.IsNullOrEmpty(termGroupObject.Path) || string.IsNullOrEmpty(termSetObject.Path) || string.IsNullOrEmpty(termObject.Name) || string.IsNullOrEmpty(termObject.Path))
                        {
                            mLog.Warn("Term Group or termset or term name or path is Empty");
                            continue;
                        }
                        CheckNameIsValid(termObject.Name, TermLevel.Term);

                        termObject.TermSetId = termSetId;
                        mLog.Info("Import term:[{0}]", termObject.Name);
                        int parentTermId = 0;
                        //get parent id
                        if (termObject.CurrentLevel != TermPropertyIndex.Level1)
                        {
                            parentTermId = TermDAO.GetParentTermIdByPath(termObject.Path, termSetId);
                            if (parentTermId == 0)
                            {
                                throw new Exception("RM_TM_ParentTermCannotFind");
                            }
                        }
                        int curTermId = 0;
                        bool isTermExist = TermDAO.CheckTermExist(parentTermId, termObject.Name, termSetId, out curTermId);

                        if (!isTermExist)
                        {
                            detailAction = "RM_TS_Action_New";
                            var termDto = new AvePoint.RA.Contract.TaxonomyModel.TermInfo { TermName = termObject.Name, ParentTermId = parentTermId, TermSetId = termObject.TermSetId ,AdvanceSetting = termObject.AdvanceSetting};
                            RMTerm rmTerm = TermDAO.CreateTerm(termDto);
                            curTermId = rmTerm.Id;
                        }
                        else
                        {

                            //detailAction = "RM_TS_Action_Update";
                            RMTerm oldTerm = TermDAO.GetRMTermByTermId(curTermId,false);

                            // term id, termset id,description without changed
                            if (IsSkip(termObject, oldTerm))
                            {
                                detailAction = "RM_TS_Action_Skip";
                            }
                            else
                            {
                                detailAction = "RM_TS_Action_Update";
                                if (isJPMCOpen)
                                {
                                    TermDAO.UpdateTermForJPMC(termObject.Name, curTermId, parentTermId, !termObject.InheritParent, termObject.TermSetId, termObject.AdvanceSetting);
                                }
                                else
                                {
                                    TermDAO.UpdateTerm(termObject.Name, curTermId, parentTermId, !termObject.InheritParent, termObject.TermSetId, termObject.Desciption);
                                }
                            }


                            /*如果Term存在,获取其状态是 Retire|Activation
                                                               if(Retire ||当前时间>from时间)
                                                               {
                                                           更新除Activation之外的信息            
                                                   }else {更新全部信息}
                           */
                            //if(oldTerm!=null&&oldTerm.)




                            //TermDAO.UpdateTerm(termObject.Name, curTermId, parentTermId, !termObject.InheritParent, termObject.TermSetId);
                            //以前是打破继承，现在是继承, 走的逻辑
                            if (parentTermId != 0 && oldTerm.BreakInheritFromParent && termObject.InheritParent)
                            {
                                TermSettingsInfo tsInfo = new TermSettingsInfo();
                                tsInfo.tId = curTermId;
                                tsInfo.breakInhert = false;
                                //tsInfo.selDateType = DateType.noExpireDate;
                                if (oldTerm.TermExpirationFrom != 0 && oldTerm.TermExpirationTo != 0)
                                {
                                    tsInfo.selDateType = DateType.fromTimeAndToTime;
                                }
                                else if (oldTerm.TermExpirationFrom != 0)
                                {
                                    tsInfo.selDateType = DateType.startTime;
                                }
                                else if (oldTerm.TermExpirationTo != 0)
                                {
                                    tsInfo.selDateType = DateType.endTime;
                                }
                                else
                                {
                                    tsInfo.selDateType = DateType.noExpireDate;
                                }
                                tsInfo.TimeZoneId = oldTerm.TimeZoneId;
                                tsInfo.beginTime = GetStrDateTime(oldTerm.TermExpirationFrom, oldTerm.TimeZoneId, false);
                                tsInfo.endTime = GetStrDateTime(oldTerm.TermExpirationTo, oldTerm.TimeZoneId, false);
                                //tsInfo.beginTime = GetStrDateTime(oldTerm.TermExpirationFrom, GeneralSetting.TimeZoneId, !GeneralSetting.isShowDayLight);
                                //tsInfo.endTime = GetStrDateTime(oldTerm.TermExpirationTo, GeneralSetting.TimeZoneId, !GeneralSetting.isShowDayLight);
                                await TaxonomyService.SaveTermSettingInheritToParentAsync(curTermId, tsInfo);
                            }
                        }


                        termObject.Id = curTermId;
                        bool isRetire = false;
                        RMTerm DBTerm = TermDAO.GetRMTermByTermId(curTermId);
                        if (isTermExist)
                        {
                            bool isTimeExpired = TermDAO.IsExpiredTerm(DBTerm.Id);
                            if (DBTerm.IsDeprecated || isTimeExpired)
                            {
                                isRetire = true;
                            }
                        }
                        else
                        {
                            isRetire = false;
                        }
                        //bind rule
                        mLog.Info($"Import Term Retention Info: {termObject.enforceRetention}, splabel:{termObject.spLabel}, exolabel:{termObject.exoLabel}, onedrivelabel:{termObject.oneDriveLabel}, teamsLabel:{termObject.teamsLabel}, breakInher:{!termObject.InheritParent}");
                        if (termObject.CurrentLevel == TermPropertyIndex.Level1 || !termObject.InheritParent)
                        {
                            var termSetting = await ConvertToTermSettingAsync(termObject, ruleDic, isRetire, DBTerm);
                            await TaxonomyService.SaveTermSettingAsync(termSetting);
                        }
                        else if (termObject.CurrentLevel != TermPropertyIndex.Level1 || termObject.InheritParent)
                        {
                            var termSetting = await ConvertToTermSettingAsync(termObject, ruleDic, isRetire, DBTerm);
                            await TaxonomyService.SaveTermSettingInheritToParentAsync(termSetting.tId, termSetting);
                        }
                        mSucceedCount++;
                        if (detailAction == "RM_TS_Action_Skip")
                        {
                            AddJobDetail(termObject.Name, detailAction, JobDetailsStatus.Skipped, "RM_TS_ITS_ExistTerm");
                        }
                        else {
                            var isRuleDeleteToRecycleBin = await CheckRuleHaveDeleteToRecycleBinAsync(termObject.RuleName, ruleDic);
                            AddJobDetail(termObject.Name, detailAction, JobDetailsStatus.Successful, isRuleDeleteToRecycleBin ? "RM_JS_TM_TermImport_ItemIsTerm_DeleteToRecycleBin" : "RM_JS_TM_TermImport_ItemIsTerm");
                        }
                    }
                    catch (Exception e)
                    {
                        //Parent Term is retired
                        //if (e.Message.Contains("Term is Deprecated"))
                        //{
                        //    //failed
                        //}
                        mFailedCount++;
                        if (termObject != null)
                        {
                            AddJobDetail(termObject.Name, detailAction, JobDetailsStatus.Failed, e.Message);
                            mLog.Error("Import term error.TermName:[{0}] Error:{1}", termObject.Name, e.ToString());
                        }
                        else
                        {
                            string objectName = GetTermObjectName(termInfo);
                            AddJobDetail(objectName, detailAction, JobDetailsStatus.Failed, e.Message);
                            mLog.Error("import term error.Error:{0}", e.ToString());
                        }
                    }
                    UpdateJobDetail(processedIncrement: 1);
                }
            }
        }

        public bool IsSkip(RMImportTermObject termObject, RMTerm oldTerm)
        {
            if(isJPMCOpen)
            {
                if(termObject.AdvanceSetting != oldTerm.AdvanceSettings)
                {
                    return false;
                }
            }

            if (oldTerm.Name == termObject.Name && oldTerm.TermSetId == termObject.TermSetId && oldTerm.Description == termObject.Desciption &&
                !oldTerm.BreakInheritFromParent == termObject.InheritParent && oldTerm.EnforceRetention == termObject.enforceRetention &&
                oldTerm.SPRetentionLabel == termObject.spLabel && oldTerm.EXORetentionLabel == termObject.exoLabel && oldTerm.OneDriveRetentionLabel == termObject.oneDriveLabel && oldTerm.TeamsRetentionLabel == termObject.teamsLabel
                )
            {
                return true;
            }
            else {
				return false;
			}
        }
        public Guid CheckGroupExitAndCreate(RMImportTermGroupObject termGroupObject)
        {
            RMTermGroup termGroup = TermGroupDAO.GetTermGroupByName(termGroupObject.Name);
            string detailAction = string.Empty;
            detailAction = "RM_TS_Action_New";
            if (termGroup == null)
            {
                try
                {
                    termGroup = TermGroupDAO.CreateTermGroupByName(termGroupObject.Name);
                    if(termGroup == null)
                    {
                        mLog.Error("CreateTermGroup Failed");
                        throw new Exception("Create Term Group Failed");
                    }
                    mSucceedCount++;
                    AddJobDetail(termGroup.Name, detailAction, JobDetailsStatus.Successful, "RM_JS_TM_TermImport_ItemIsTermGroup");
                }
                catch (Exception e)
                {
                    mFailedCount++;
                    AddJobDetail(termGroup?.Name, detailAction, JobDetailsStatus.Failed, e.Message);
                    mLog.Error("Import term group error.TermGroup Name:[{0}] Error:{1}", termGroup?.Name, e.ToString());
                }
            }
            else
            {
                detailAction = "RM_TS_Action_Skip";
                mSkipCount++;
                AddJobDetail(termGroup.Name, detailAction, JobDetailsStatus.Skipped, "RM_TS_ITS_ExistTermGroup");
            }
            return termGroup?.UniqueId ?? Guid.Empty;
        }
        //mSucceedCount和totalCount会计算不准确  因为一行term info可能会经过创建group  termset 以及term
        public int CheckTermSetExitAndCreate(RMImportTermSetObject termSetObject)
        {
            if (!mTermSetPathAndTermSettingIdMapping.ContainsKey(termSetObject.Path))
            {
                var groupName = termSetObject.Path.Split(PathSeparator).ToList()[0];
                var group = TermGroupDAO.GetTermGroupByName(groupName);
                if (group != null)
                {
                    var detailAction = "RM_TS_Action_New";
                    var termSets = TermSetDAO.GetRMTermSetsByGroupUniqueIdAndTermSetName(group.UniqueId, termSetObject.Name);
                    if (termSets.IsNullOrEmpty())
                    {
                        var termSet = !isControlPlus ?
                            TermSetDAO.CreateTermSet(termSetObject.Name, group.UniqueId, termSetObject.Desciption)
                            : TermSetDAO.CreateGoogleTermSet(termSetObject.Name, group.UniqueId).Result;
                        mSucceedCount++;
                        AddJobDetail(termSet.Name, detailAction, JobDetailsStatus.Successful, "RM_JS_TM_TermImport_ItemIsTermSet");
                        mTermSetPathAndTermSettingIdMapping[termSetObject.Path] = termSet.Id;
                    }
                    else
                    {
                        detailAction = "RM_TS_Action_Skip";
                        AddJobDetail(termSets[0].Name, detailAction, JobDetailsStatus.Skipped, "RM_TS_ITS_ExistTermTermSet");
                        mTermSetPathAndTermSettingIdMapping[termSetObject.Path] = termSets[0].Id;
                    }
                }
                else
                {
                    throw new Exception(string.Format(I18NEntity.GetString("Import term set error.There is no termGroup.Term Group Name:[{0}]"), groupName));
                }
            }

            return mTermSetPathAndTermSettingIdMapping[termSetObject.Path];

        }
        public async Task ProcessCsvAsync(string filePath)
        {
            InitializeExpectedDetailCount(CountCsvDataRows(filePath));
            using (FileStream fs = new FileStream(filePath, FileMode.Open,FileAccess.Read))
            {
                using (StreamReader sr = new StreamReader(fs, Encoding.UTF8))
                {
                    var lineNumber = 1;
                    string title = sr.ReadLine();
                    string[] titleValues = title?.Split(',').ToArray();

                    if (titleValues.Count() != 9)
                    {
                        throw new TermCsvFormateExcetion("The 'CSV' file content is not right.");
                    }

                    int termSetId = -1;
                    while (!sr.EndOfStream)
                    {
                        using (CheckJobStopScope fScope = new CheckJobStopScope())
                        {
                            lineNumber++;
                            string curLine = sr.ReadLine();
                            JMImportTermDetail detail = null;
                            mLog.Info("Current term info.[{0}]", curLine);
                            object termObj = new object();
                            int termId = -1;
                            mtotalCount++;
                            TermInfo curTerm = ConvertToTermInfo(curLine, lineNumber);
                            if (string.IsNullOrEmpty(curTerm.TermGroupName) || !IsTermCorrect(curTerm))
                            {
                                continue;
                            }
                            TermConflictType type = CheckTermConflict(curTerm, ref termObj, ref termSetId);
                            switch (type)
                            {
                                case TermConflictType.Skip:
                                    breakJob = termObj as RMTermGroup == null && termObj as RMTermSet == null && termObj as RMTerm == null && termSetId == -1;
                                    detail = SkipTerm(curTerm);
                                    mSkipCount++;
                                    break;
                                case TermConflictType.Conflict:
                                    (detail,termId) = await HandleConflictAsync(curTerm, termObj, termSetId);
                                    mSucceedCount++;
                                    curTerm.TermIntId = termId;
                                    break;
                                case TermConflictType.None:
                                    detail = CreateTerm(curTerm, ref termSetId, out termId);
                                    curTerm.TermIntId = termId;
                                    break;
                                case TermConflictType.SkipRECSameName:
                                    breakJob = termObj as RMTermGroup == null;
                                    detail = SkipTermGroupWithSameNameInReco(curTerm);
                                    mSkipCount++;
                                    break;
                                default:
                                    break;
                            }
                            mDetails.Add(detail);
                            if (breakJob)
                            {
                                break;
                            }
                            AddtoTermDic(curTerm);
                            UpdateJobDetail(processedIncrement: 1);
                        }
                    }
                }
            }
        }

        private int CountCsvDataRows(string filePath)
        {
            int count = 0;
            using (FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            using (StreamReader sr = new StreamReader(fs, Encoding.UTF8))
            {
                sr.ReadLine();
                while (!sr.EndOfStream)
                {
                    sr.ReadLine();
                    count++;
                }
            }
            return count;
        }

        private bool IsTermCorrect(TermInfo term)
        {
            bool result = false;
            try
            {
                if (string.IsNullOrEmpty(term.TermSetName))
                {
                    CheckNameIsValid(term.TermGroupName);
                }
                else if (string.IsNullOrEmpty(term.TermName))
                {
                    CheckNameIsValid(term.TermSetName);
                }
                else
                {
                    CheckNameIsValid(term.TermName);
                }
                result = true;
            }
            catch (Exception e)
            {
                result = false;
                var name = string.Empty;
                if (string.IsNullOrEmpty(term.TermSetName))
                {
                    name = term.TermGroupName;
                }
                else if (string.IsNullOrEmpty(term.TermName))
                {
                    name = term.TermSetName;
                }
                else
                {
                    name = term.TermName;
                }
                mLog.Error("Name:[{0}] Error:{1}", name, e.ToString());
                JMImportTermDetail detail = new JMImportTermDetail();
                detail.Term = name;
                detail.Status = JobDetailsStatus.Failed;
                detail.Comment = e.Message;

                mDetails.Add(detail);
            }
            return result;
        }

        private struct TermInfo
        {
            public string TermGroupName;
            public Guid TermGroupUniqueId;
            public string TermSetName;
            public Guid TermSetUniqueId;
            public string TermName;
            public Guid TermUniqueId;
            public Guid ParentUniqueId;
            public int TermIntId;
            public bool IsDeprecated;
            public string Description;
            public bool usingMMSSpecified;
        }

        private TermInfo ConvertToTermInfo(string text, int lineNumber)
        {
            TermInfo result = new TermInfo();
            try
            {
                const string commaStr = "(RevIM_Comma)";
                const string backslashStr = "(RevIM_Backslash)";
                const string enterStr = "(RevIM_Enter)";
                string[] columnValues = text?.Split(',').Select(t => t.Replace(commaStr, ",").Replace(backslashStr, @"\").Replace(enterStr, "\n")).ToArray();
                if (columnValues?.Length != 9)
                {
                    JMImportTermDetail detail = new JMImportTermDetail();
                    detail.Status = JobDetailsStatus.Failed;
                    detail.Comment = string.Format(I18NEntity.GetString("RM_TM_Import_Analyse_Error"), lineNumber);
                    detail.Action = "RM_TM_Term_Import_Analyse";
                    detail.Term = "RM_TM_CSV_Term_IsNone";
                    mDetails.Add(detail);
                    throw new Exception("The format of csv file is not correct and the linenumber is " + lineNumber);
                }
                result.TermGroupName = columnValues[0].Replace("&", "＆").Replace("\"", "＂");
                result.TermGroupUniqueId = !string.IsNullOrEmpty(columnValues[1]) ? new Guid(columnValues[1]) : Guid.Empty;
                result.TermSetName = columnValues[2].Replace("&", "＆").Replace("\"", "＂");
                result.TermSetUniqueId = !string.IsNullOrEmpty(columnValues[3]) ? new Guid(columnValues[3]) : Guid.Empty;
                result.TermName = columnValues[4].Replace("&", "＆").Replace("\"", "＂");
                result.TermUniqueId = !string.IsNullOrEmpty(columnValues[5]) ? new Guid(columnValues[5]) : Guid.Empty;
                result.ParentUniqueId = !string.IsNullOrEmpty(columnValues[6]) ? new Guid(columnValues[6]) : Guid.Empty;
                result.TermIntId = -1;
                result.IsDeprecated = !string.IsNullOrEmpty(columnValues[7]) ? bool.Parse(columnValues[7]) : false;
                if (!string.IsNullOrEmpty(columnValues[8]) && columnValues[8].Length > 5000)
                {
                    throw new Exception("RM_TM_CustomProperties_DescriptionLengthLimit");
                }
                result.Description = columnValues[8];
            }
            catch (Exception e)
            {
                mFailedCount++;
                mLog.Error("Convert text to TermObject error:{0}", e.ToString());
            }
            return result;
        }
        private enum TermConflictType
        {
            Skip,
            Conflict,
            None,
            SkipRECSameName
        }
        private enum TermType
        {
            TermGroup = 1,
            TermSet,
            Term,
        }


        #region XML file export through pnp script
        private const string pnpTermGroups = "pnp:TermGroups";
        private const string xmlID = "ID";
        private const string xmlName = "Name";
        private const string xmlDescription = "Description";
        private const string xmlIsDeprecated = "IsDeprecated";
        private async Task ProcessTermXmlAsync(string filePath)
        {
            XmlDocument doc = new XmlDocument();
            doc.Load(filePath);
            foreach (XmlElement child in doc.ChildNodes)
            {
                if (child.Name == pnpTermGroups)
                {
                    foreach (XmlElement group in child.ChildNodes)
                    {
                        await ReadGroupAsync(group);
                    }
                }
            }
        }
        
        public async Task ReadGroupAsync(XmlElement group)
        {
            RMTermGroup gGroup = new RMTermGroup();
            try
            {
                gGroup.UniqueId = Guid.Parse(group.GetAttribute(xmlID));
                gGroup.Name = FixNameXml(group.GetAttribute(xmlName));
                var description = group.GetAttribute(xmlDescription);
                if (!string.IsNullOrEmpty(description) && description.Length > 5000)
                {
                    this.AddJobDetail(gGroup.Name, null, JobDetailsStatus.Failed, "RM_TM_CustomProperties_DescriptionLengthLimit");
                }
                gGroup.Description = description;
                mLog.Info("process group : {0}", gGroup.Name);
                RMTermGroup groupInDB = TermGroupDAO.GetTermGroupByGuid(gGroup.UniqueId);
                if(groupInDB != null)
                {
                    if (groupInDB.Description == gGroup.Description && groupInDB.Name == gGroup.Name && !groupInDB.IsRemoved)
                    {
                        mLog.Info("Group {0} already exists, or removed.", gGroup.Name);
                        //Skip detail
                        this.AddJobDetail(gGroup.Name, "RM_TS_Action_Skip", JobDetailsStatus.Skipped, "RM_TS_ITS_ExistTermGroup");
                    }
                    else
                    {
                        mLog.Info("update term group {0}", gGroup.Name);
                        if (TermGroupDAO.ReNameHasSameNameTermGroup(groupInDB.Id, gGroup.Name))
                        {
                            this.AddJobDetail(gGroup.Name, "RM_TS_Action_Update", JobDetailsStatus.Skipped, "RM_TS_ITS_ExistSameNameTermGroup"); 
                        }
                        else
                        {
                            await TermGroupDAO.UpdateTermGroupAsync(groupInDB.Id, gGroup.Name, groupInDB.Description);
                            //update or sucess detail
                            this.AddJobDetail(gGroup.Name, "RM_TS_Action_Update", JobDetailsStatus.Successful); 
                        }
                    }
                }
                else
                {
                    mLog.Info("create term group {0}", gGroup.Name);
                    if (TermGroupDAO.HasSameNameTermGroup(gGroup.Name))
                    {
                        this.AddJobDetail(gGroup.Name, "RM_TS_Action_New", JobDetailsStatus.Failed, "RM_TS_ITS_ExistSameNameTermGroup");
                        mFailedCount++;
                        return;
                    }
                    else
                    {
                        TermGroupDAO.CreateTermGroupById(gGroup.UniqueId, gGroup.Name, gGroup.Description, false);
                        //sucess detail
                        this.AddJobDetail(gGroup.Name, "RM_TS_Action_New", JobDetailsStatus.Successful);
                    }
                }
                mSucceedCount++;
            }
            catch (Exception e)
            {
                mLog.Error(e.Message, e);
                mFailedCount++;
                //failed detail
                this.AddJobDetail(gGroup.Name, string.Empty, JobDetailsStatus.Failed, e.Message);
                return;
            }
            
            foreach (XmlElement sets in group.ChildNodes)
            {
                if(sets.Name == "pnp:TermSets")
                {
                    foreach (XmlElement set in sets.ChildNodes)
                    {
                        await ReadSetAsync(set, gGroup); 
                        UpdateJobDetail(processedIncrement: 1);
                    }
                }
            }
        }

        public async Task ReadSetAsync(XmlElement set, RMTermGroup gGroup)
        {
            RMTermSet termSet = new RMTermSet();
            try
            {
                termSet.UniqueId = Guid.Parse(set.GetAttribute(xmlID));
                termSet.Name = FixNameXml(set.GetAttribute(xmlName));
                termSet.Description = set.GetAttribute(xmlDescription);
                termSet.TermGroupId = gGroup.UniqueId;
                mLog.Info("process term set : {0}", termSet.Name);
                RMTermSet setInDB = TermSetDAO.GetRMTermSetByGuid(termSet.UniqueId);
                if(setInDB != null)
                {
                    termSet.Id = setInDB.Id;
                    if(setInDB.Description == termSet.Description && setInDB.Name == termSet.Name && !setInDB.IsRemoved)
                    {
                        //Skip detail
                        this.AddJobDetail(termSet.Name, "RM_TS_Action_Skip", JobDetailsStatus.Skipped, "RM_TS_ITS_ExistTermTermSet");
                    }
                    else
                    {
                        await TermSetDAO.UpdateTermSetAsync(setInDB.Id, termSet.Name, setInDB.Description);
                        //update or sucess detail
                        this.AddJobDetail(termSet.Name, "RM_TS_Action_Update", JobDetailsStatus.Successful);
                    }
                }
                else
                {
                    termSet = TermSetDAO.Create(termSet);
                    //sucess detail
                    this.AddJobDetail(termSet.Name, "RM_TS_Action_New", JobDetailsStatus.Successful);
                }
                mSucceedCount++;
            }
            catch (Exception e)
            { 
                mLog.Error(e.Message, e);
                mFailedCount++;
                //failed detail
                this.AddJobDetail(termSet.Name, string.Empty, JobDetailsStatus.Failed, e.Message);
                return;
            }
            if (set.HasChildNodes)
            {
                foreach (XmlElement terms in set.ChildNodes)
                {
                    if (terms.Name == "pnp:Terms" && terms.HasChildNodes)
                    {
                        foreach (XmlElement term in terms.ChildNodes)
                        {
                            await ReadTermAsync(term, termSet, null);
                            UpdateJobDetail(processedIncrement: 1);
                        }
                    }
                }
            }
        }
        public async Task ReadTermAsync(XmlElement term, RMTermSet termSet, RMTerm parentTerm)
        {
            RMTerm tm = new RMTerm();
            try
            {
                tm.UniqueId = Guid.Parse(term.GetAttribute(xmlID));
                tm.Name = FixNameXml(term.GetAttribute(xmlName));
                tm.Description = term.GetAttribute(xmlDescription);
                mLog.Info("process term : {0}", tm.Name);
                if (term.HasAttribute(xmlIsDeprecated))
                {
                    tm.IsDeprecated = string.Equals(term.GetAttribute(xmlIsDeprecated), true.ToString(), StringComparison.OrdinalIgnoreCase);
                }
                RMTerm termInDB = TermDAO.GetRMTermByGuId(tm.UniqueId);
                if(termInDB != null)
                {
                    tm.Id = termInDB.Id;
                    if(!termInDB.IsRemoved && termInDB.Name == tm.Name && termInDB.Description == tm.Description && termInDB.IsDeprecated == tm.IsDeprecated)
                    {
                        //skip detail
                        this.AddJobDetail(tm.Name, "RM_TS_Action_Skip", JobDetailsStatus.Skipped, "RM_TS_ITS_ExistTerm");
                    }
                    else
                    {
                        await TermDAO.UpdateTermAsync(tm.Name, parentTerm == null ? 0 : parentTerm.Id, termSet.Id, tm.IsDeprecated, tm.UniqueId, tm.Description);
                        //success detail
                        this.AddJobDetail(tm.Name, "RM_TS_Action_Update", JobDetailsStatus.Successful);
                    }
                }
                else
                {
                    tm = TermDAO.CreateTermForImport(tm.Name, parentTerm == null ? 0 : parentTerm.Id, termSet.Id, tm.IsDeprecated, tm.UniqueId, tm.Description);
                    mLog.Info($"Add term successfully, int ID {tm.Id}, uniuqe ID {tm.UniqueId}, name {tm.Name}");
                    //scucess detail
                    this.AddJobDetail(tm.Name, "RM_TS_Action_New", JobDetailsStatus.Successful);
                }
                mSucceedCount++;
            }
            catch (Exception e)
            {
                mLog.Error(e.Message, e);
                mFailedCount++;
                //failed detail
                this.AddJobDetail(tm.Name, string.Empty, JobDetailsStatus.Failed, e.Message);
                return;
            }
            if (term.HasChildNodes)
            {
                foreach (XmlElement terms in term.ChildNodes)
                {
                    if (terms.Name == "pnp:Terms" && terms.HasChildNodes)
                    {
                        foreach (XmlElement subTerm in terms.ChildNodes)
                        {
                            await ReadTermAsync(subTerm, termSet, tm);
                        }
                    }
                }
            }
        }

        private string FixNameXml(string name)
        {
            if (name != null)
            {
                return name.Replace("&", "＆").Replace("\"", "＂").Trim();
            }
            return name;
        }
        private async Task<bool> CheckGoogleLicenseAsync()
        {
            var isUser = await SecurityTrimmingHelper.DoesUserHasThisPermissionAsync(RMPermissionExtensionMasks.GoogleEndUser);
            var isAdmin = await SecurityTrimmingHelper.DoesUserHasThisPermissionAsync(RMPermissionExtensionMasks.GoogleAdmin);
            if (isUser || isAdmin)
            {
                return true;
            }

            return false;
        }

        public bool CheckFilterRuleIsBlankCondition(ArchiverFilterRuleType ruleType)
        {
            List<ArchiverFilterRuleType> ruleTypeForIsBlankCondition = new List<ArchiverFilterRuleType>
            {
                ArchiverFilterRuleType.TextColumn,
                ArchiverFilterRuleType.SensitivityLabel,
                ArchiverFilterRuleType.RetentionLabel,
                ArchiverFilterRuleType.TextLabelProperty,
                ArchiverFilterRuleType.SensitivityLabelFullName,
            };
            if (ruleTypeForIsBlankCondition.Contains(ruleType))
            {
                return true;
            }
            return false;
        }

        private async Task<bool> CheckRuleHaveDeleteToRecycleBinAsync(string ruleName, Dictionary<Guid, RMRuleInfos> ruleDic)
        {
            if (string.IsNullOrEmpty(ruleName))
            {
                return false;
            }

            string[] ruleNames = ruleName.Split(';').Select(t => t.TrimStart(' ')).Distinct(StringComparer.CurrentCultureIgnoreCase).ToArray();
            List<RMRuleInfos> dbRecordsRules = null;
            foreach (string curRuleName in ruleNames)
            {
                RMRuleInfos curRule = ruleDic?.Values.FirstOrDefault(r => r.RuleName.Equals(curRuleName, StringComparison.OrdinalIgnoreCase));
                if (curRule == null)
                {
                    dbRecordsRules = dbRecordsRules ?? await RuleManagerService.GetSimpleRecordsRulesFromDBAsync();
                    string ruleId = dbRecordsRules.Where(r => r.RuleName.Equals(curRuleName, StringComparison.OrdinalIgnoreCase)).Select(r => r.RuleId).FirstOrDefault();
                    if (string.IsNullOrEmpty(ruleId))
                    {
                        mLog.Warn("Can not find Rule. Name:[{0}]", curRuleName);
                        continue;
                    }
                    curRule = await RuleManagerService.LoadRuleAsync(ruleId);
                }

                if (IsDeleteToRecycleBinEnabledForSpOrOneDrive(curRule))
                {
                    return true;
                }
            }

            return false;
        }

        private bool IsDeleteToRecycleBinEnabledForSpOrOneDrive(RMRuleInfos rule)
        {
            if (rule == null)
            {
                return false;
            }

            bool isSharePointDeleteToRecycleBin = rule.DeleteToRecycleBin;
            bool isOneDriveDeleteToRecycleBin = rule.OneDriveRule != null && rule.OneDriveRule.DeleteToRecycleBin;
            return isSharePointDeleteToRecycleBin || isOneDriveDeleteToRecycleBin;
        }

        private async Task<bool> IsShowDestroyWithoutBackupMessageAsync(RMRuleInfos ruleObj)
        {
            return (ruleObj.IsSpSource && HasNoStoragePolicy(ruleObj)) || (ruleObj.IsOneDriveSource && HasNoStoragePolicy(ruleObj.OneDriveRule)) || (ruleObj.IsFSSource && HasNoStoragePolicy(ruleObj.FSRule));
        }

        private bool HasNoStoragePolicy(RMRuleInfos sourceRule)
        {
            return sourceRule != null
                && string.IsNullOrWhiteSpace(sourceRule.StoragePolicyId)
                && string.IsNullOrWhiteSpace(sourceRule.StoragePolicyName);
        }

        //private void AssembleXmlDetail(JMImportTermAction action, JobDetailsStatus status, string name, string comment = null)
        //{
        //    JMImportTermDetail detail = new JMImportTermDetail();
        //    detail.Action = action.ToString();
        //    detail.Term = name; 
        //    detail.Status = status;
        //    detail.Comment = comment;

        //    mDetails.Add(detail);
        //}
        #endregion
    }

}
