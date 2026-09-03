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
//using AvePoint.GCommon.Contract.Server.ControlPanel.ManagedAccount.Object;
using AvePoint.GCommon.Contract.Server.Common.BackupDataSearch;
using AvePoint.GCommon.Contract.Server.ControlPanel.Office365Account.Object;
using AvePoint.GCommon.Contract.Tree.Object;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.Contract.FileSystem;
using AvePoint.RA.Contract.JobMonitor;
using AvePoint.RA.Contract.Multi_Geo.Model;
using AvePoint.RA.Contract.Myhub.Items.Views;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.Object.RealTime;
using AvePoint.RA.Contract.OnPremiseSharePoint;
using AvePoint.RA.Contract.RMRelatedRecord;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RMWeb.CP;
using AvePoint.RA.Contract.RMWeb.Explorer;
using AvePoint.RA.Contract.RMWeb.Physical;
using AvePoint.RA.Contract.RMWeb.TemplateManagement;
using Cloud.Sdk.Telemetry.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.Contract.RMWeb
{
    public interface IExplorerService
    {


        //void AddScopeData(ScopeDto dto);
        //long GetCollectionTime(Guid scopeId);

        /// <summary>
        /// get data by query dto for UI pager
        /// </summary>
        /// <param name="dto">query dto</param>
        /// <returns></returns>
        //ExplorerResultInfo QueryDataListWithoutTotal(ExplorerQueryDto dto, bool isGlobalSearch = false);

        //int QueryDataListGetTotal(ExplorerQueryDto dto);

        Task<RAReturnMessage> CreateHoldAsync(UpdateHoldDto dto);

        Task<RAReturnMessage> EditHoldAsync(UpdateHoldDto dto);

        Task<RAReturnMessage> ChangeHoldCreateAsync(UpdateHoldDto dto);

        RAReturnMessage ChangeHoldReuse(UpdateHoldDto dto);

        Task<RAReturnMessage> CreateHoldTypeWithRecordAsync(UpdateHoldDto dto,bool isFS = false);

        Task<RAReturnMessage> ReuseHoldTypeWithRecord(UpdateHoldDto dto, bool isFS = false);
        RAReturnMessage CheckItemOnLoaned(List<Guid> ids);
        Task<List<HoldSetting>> GetHoldAsync(int profileType = 0);
        Task<List<HoldSetting>> GetSampleHoldAsync(int profileType = 0);
        Task<List<HoldSetting>> GetAssignedHoldsAsync();
        Task<HoldSetting> GetHoldByRecoedIdAsync(Guid recordId, int allianceType = 1);

        List<string> GetHoldsByRecoedId(Guid recordId);
        Task<List<RemoveHoldSetting>> GetHoldListByRecoedIdAsync(Guid recordId);

        RAReturnMessage CancelHoldByRecords(List<Guid> recordsId, bool isPhysical = false, List<string> removeHoldIds = null);

        RAReturnMessage SusPendRecords(UpdateHoldDto dto,bool isFS = false);

        RAReturnMessage SusPendHolds(UpdateHoldDto dto,bool isFS = false);

        RAReturnMessage CancelHoldSetting(List<string> holdIds ,bool isFS = false);

        Task<RAReturnMessage> DeleteHoldAndSettingAsync(List<string> holdIds,bool isFS = false);

        Task<ExplorerResultInfo> GetRecordbyHoldIdAsync(ExplorerSetHoldDto dto);
        Task<RAReturnMessage> RunExportHoldRecordsJobAsync(JobRunBy jobRunBy, List<string> holdIds);
        Task<RAReturnMessage> RunImportHoldRecordsJobAsync(JobRunBy jobRunBy, string blobName);

        /// <summary>
        /// change term(batch update)
        /// </summary>
        /// <param name="keys">records id in db</param>
        /// <param name="termId">changed term id</param>
        /// <returns></returns>
        Task<RAReturnMessage> ChangeTermAsync(ChangeTermDto changeTermInfo);
        Task<RAReturnMessage> ChangeGoogleTermAsync(ChangeTermDto changeTermDto);
        bool CheckItemsInTheSameSecurityGroup(List<Guid> recordIds);
        RARealTimeJobMessage GetRealTimeJobStatusInfo(string jobId);

        RAReturnMessage DoGlobalSearchRealTimeAction(GlobalSearchActionDto globalSearchActionDto);

        RAReturnMessage StartGlobalSearchActionJob(GlobalSearchActionDto globalSearchActionDto);

        Task<RAReturnMessage> ValidateParameterAsync(GlobalSearchActionDto actionDto, ChangeTermPage page);

        /// <summary>
        /// declare as record(batch)
        /// </summary>
        /// <param name="keys">records id in db</param>
        /// <returns></returns>
        Task<RAReturnMessage> DeclareAsRecordAsync(List<Guid> ids);

        /// <summary>
        /// undeclare as record(batch)
        /// </summary>
        /// <param name="ids">records id in db</param>
        /// <returns></returns>
        Task<RAReturnMessage> UndeclareAsRecordAsync(List<Guid> ids);
        RAReturnMessage PhysicalMove(PhysicalMoveDto moveDto);
        /// <summary>
        /// get data by id
        /// </summary>
        /// <param name="isArchived">Archived by DA</param>
        /// <param name="key">records id in db</param>
        /// <returns></returns>
        //BaseRecordDto GetObjectData(Guid scopeId, Guid spObjectId);//to do next...
        List<BaseRecordDto> GetObjectDatas(Guid scopeId, List<Guid> termIds, long ticks);

        /// <summary>
        /// get detail by id for UI
        /// </summary>
        /// <param name="isArchived">Archived by DA</param>
        /// <param name="key">records id in db</param>
        /// <returns></returns>
        Task<RecordDetailDto> LoadDetailByKeyAsync(int status, Guid id, ExplorerDetailTab tab, bool isControlPlus = false);

        RAReturnMessage StartRestoreArchivedContent(List<Guid> ids);
        Task<ArchivedContentResultInfo> LoadDownloadArchivedContentAsync(ArchivedContentSearchInfo searchInfo);
        Task<RMRCCReportResult> LoadRCCInfoByIdAsync(RMRCCReportInfo requestInfo, string timeZoneId, bool isDaylight);
        Task<RMDisposalHistoryReportResult> LoadDisposalHistoryReportAsync(RMDisposalHistoryReportInfo request, string timeZoneId, bool isDaylight);
        RAReturnMessage DeleteArchivedContent(List<Guid> jobIds);

        void StartFCJob();
        //   void StartICJob();

        //Dictionary<Guid, int> TestUnion();

        Task<ExplorerResultInfo> GetRelatedRecoredsInfoAsync(Guid id);
        Task<ExplorerResultInfo> SearchRecordsAsync(string pageIndex, int pageSize, string value, Guid currentId, List<Guid> relatedsCache);
        //int SearchRecordsGetTotal(int pageIndex, int pageSize, string value, int currentId, List<int> relatedsCache);
        RAReturnMessage UpdateRelatedRecords(Guid id, List<Guid> relatedIds, List<Guid> removeRelatedIds, Dictionary<Guid, string> idNameDict, out List<Guid> addrelatedIdsForHistory);
        //void UpdateCollectionTime(Guid scopeId, long timeTicks);
        List<Guid> GetChangeTermIds(long ticks);

        Dictionary<Guid, long> GetChangedTerms(long ticks);

        System.Threading.Tasks.Task RemoveAllChangeTermIdsAsync();
        
        CheckLocationObject CheckUNCLocation(string locationPath, RMAccountProfileDto account);
        Task<CheckLocationObject> CheckSPUrlAsync(string locationPath, RMAccountProfileDto account);

        Task<CheckLocationObject> CheckUNCLocation4RuleAsync(string locationPath, Office365AccountInfo account);
        Task<CheckLocationObject> CheckSPUrl4RuleAsync(string locationPath, RMAccountProfileDto account);
        DestinationSPOLocationInfo CheckSPUrl4Job(string locationPath, RMAccountProfileDto account, bool isSupportSiteLevel = false);
        //string AddMoveJobTODBJobQueue(MoveToDto dto);
        string RunMoveToJob(string jobRunBy, string param);
        List<Office365AccountInfo> GetAllO365Accounts();
        string RealRunCollectionJob(JobRunBy jobRunBy, JobType jobType);
        /// <summary>
        /// Explore db has data or not
        /// </summary>
        /// <returns></returns>
        bool IsExplorerDBDataExist();

        Task<RecordsReturnMessage> ChangeTermRealTimeAllSourceAsync(ChangeTermOption changeTermOption, string jobId);
        Task<RecordsReturnMessage> ChangeTermRealTimeSPAsync(ChangeTermOption changeTermOption, string jobId, bool waiting4EXO);
        RecordsReturnMessage ChangeTermRealTimeEXO(ChangeTermOption changeTermOption, string jobId, bool waiting4EXO);

        Task<RecordsReturnMessage> DeclareAsRecordRealTimeAsync(List<Guid> ids, string jobId, string declareBy);
        Task<RecordsReturnMessage> UndeclareAsRecordRealTimeAsync(List<Guid> ids, string jobId, string declareBy);
        Task<RecordsReturnMessage> PhysicalExplorerMoveRealTimeAsync(PhysicalMoveOption moveOption, string jobId, Guid groupRequestId = default);
        Task<RecordsReturnMessage> PhysicalMoveForMobileAsync(PhysicalMoveOption moveOption, string jobId);
        List<RMRelatedItemInfo> GetRelatedRecoredsBaseInfo(Guid id);
        List<RMRelatedItemInfo> GetRelatedRecoredsBaseInfoForStandardUser(Guid id,List<int> ScopePermissions);
        bool PhysicalObjectUnderContainer(Guid id);

        #region Physical

        void AppendPushedColumns(List<PhysicalObjectDto> results);
        Task<RAReturnMessage> AddOrUpdatePhysicalObjectAsync(PhysicalObjectDto dto);

        void AddPushColumnToFold(TemplateDto resultDto, Guid boxId);
        Task<RAReturnMessage> EditPhysicalObjectAsync(PhysicalObjectDto dto);
        Task<RAReturnMessage> BulkEditPhysicalObjectAsync(List<Guid> recordIds, Dictionary<string, string> metaInfoDic, int templateId);
        RAReturnMessage UpdatePhysicalRecordState2Hold(List<string> uniqueId, AOSUserDto holdByUser, long releaseTime);
        Task<List<Tuple<ItemActionResult, PhysicalObjectDto>>> UpdatePhyFilesHoldStateByBoxIdAsync(Tuple<Guid, AOSUserDto, long> request);
        Task<(List<Tuple<ItemActionResult, PhysicalObjectDto>>,string,bool)> UpdatePhyFilesHoldStateByBoxIdAsync(Tuple<Guid, AOSUserDto, long> request, int pateSize, string pageIndex);
        Task<List<Tuple<ItemActionResult, PhysicalObjectDto>>> ReutrnPhyFilesByBoxIdAsync(AOSUserDto boxLoanUser, List<Guid> loanedIds, int pageSize, int pageIndex);
        int GetPhyBoxAndFileCountByBoxIds(List<Guid> uniqueIds);
        List<PhysicalObjectDto> GetAllLoanedFolders(List<Guid> guids);
        RAReturnMessage UpdatePhysicalRecordStatusForMobile(MobileChangeStatusDto requestDto);
        Task<PhysicalObjectDto> GetPhysicalObjectByIdAsync(Guid id, bool getBarcode = false);
        //Record GetPhysicalRecordById(Guid id);
        PhysicalObjectDto GetPhysicalObjectByUniqueId(string uniqueId);
        PhysicalObjectDto GetPhysicalObjectById(Guid id);
        Dictionary<Guid, string> GetPushedColumnValues(Guid phyObjUniqueId, IEnumerable<PushColumnDto> columnUniqueIDs);
        Task<PhysicalResultInfo> QueryPhysicalNodesAsync(PhysicalExplorerQueryDto dto);
        Task<PhysicalObjectDto> FindPhysicalObjectByRecordsIdAsync(string recordsId);
        Task<PhysicalObjectDto> FindPhysicalObjectByBarcodeAsync(string barcode);
        DeleteResultInfo DeletePhysicalObject(List<PhysicalObjectDto> physicalObjectDtos);
        List<PhysicalObjectDto> PreDeletePhysicalObjects(List<PhysicalObjectDto> physicalObjectDtos);
        Task<RAReturnMessage> RemovePersonalHoldAsync(List<Guid> nodeIDs);
        Task<RAReturnMessage> RemovePersonalHoldForMobileAsync(List<Guid> nodeIDs);
        Task<string> GetPhysicalBoxPathByIdAsync(Guid id);
        string RunPhysicalTimerJob(JobRunBy jobRunBy);
        string RealRunPhysicalTimerJob(string param, JobRunBy JobRunType);
        string RunConnectorTimerJob(JobRunBy jobRunBy);
        string RealRunConnectorTimerJob(string param, JobRunBy JobRunType);
        List<string> GetHoldChildrenByBox(List<Guid> boxId);
        bool IsBoxHasHoldChildren(List<Guid> boxId);
        bool IsPhysicaRecordExistForCreateTime(Guid id, DateTime startUtcTime, DateTime endUtcTime);
        bool IsPhysicaRecordExistForDestroyedTime(Guid id, DateTime startUtcTime, DateTime endUtcTime);
        string GetPhysicalObjectFullPath(Guid id, bool isReplaceI18NKey = true);
        string GetPhysicalObjectFullPath(PhysicalObjectDto dto);
        int GetSelectNodeAllChildCount(ExportBarcodeDto exportBarcodeDto);
        Task<ExportResultDto> ExportBarcodeAsync(ExportBarcodeDto exportBarcodeDto);

        Task<RAReturnMessage> ExportBarcodeToLocationAsync(ExportBarcodeDto exportBarcodeDto);
        string RealExportBarcode(JobRunBy JobRunType, string exportLocationId, string nodeId, string nodeType, string exportLocationName, string suiteId);

        Task<string> ExportSearchResultAsync(GlobalSearchExportDto globalSearchExportDto);
        Task<RAReturnMessage> StartExportSearchResultJobAsync(GlobalSearchExportDto globalSearchExportDto);
        Task<string> RealRunExportSearchResultJobAsync(string parameter);
        Task<string> RealRunExportHoldRecordsJobAsync(string parameter);
        Task<string> RealRunImportHoldRecordsJobAsync(string blobName);
        void AssignRecordsToHoldAsync(UpdateHoldDto dto, string currentHoldBy);

        System.Threading.Tasks.Task GetPhysicalBarcodeInfoAsync(PhysicalObjectDto dto);
        List<int> GetPhysicalObjectPermissionIds(List<Guid> nodeIds);
        Task<List<int>> GetPermissionConditionAsync();
        Task<bool> IsPhysicalEndUserAsync();
        bool IsPhysicalRecord(Guid id);

        //bool IsPhysicalEndUser();
        System.Threading.Tasks.Task ConvertDateTimeColumnValueTimeZoneAsync(PhysicalObjectDto dto);

        string GetPhysicalScopeIdFullPath(Guid nodeId);
        #endregion

        #region FS
        RAReturnMessage AddOrUpdateFileSystemObject(FileSystemRecordDto dto);
        List<FileSystemRecordDto> GetFileSystemObjectByGuids(List<Guid> nodeIds);
        string GetFSTreeData(int treeNodeType, string treeNodeId);
        List<FSFolderCacheDto> GetExplorerDataByFolder(string folderId, string scopeId, long sortTicks, int pageSize);
        List<FileSystemRecordDto> GetDBRecordsByFolder(string folderId, string scopeId, long sortTicks, int pageSize);
        bool HasFileMatchTerm(string dirPath, string scopeId, List<Guid> classCodeIds);
        List<FileSystemRecordDto> GetDBRecordsByFolderAndEndTime(string folderId, string scopeId, long sortTicks, int pageSize);
        List<FileSystemRecordDto> GetDBRecordsByNodeIds(List<Guid> nodeIds, string scopeId, long sortTicks);
        List<FileSystemRecordDto> GetDBRecordsByNodeIdsAndEndTime(List<Guid> nodeIds, string scopeId, long sortTicks);
        List<FileSystemRecordDto> GetDBRecordsByClassCodeAndFilterByEndTime(IEnumerable<Guid> nodeIds, IEnumerable<Guid> classCodeIds, string scopeId, long sortTicks);
        List<FSFolderCacheDto> GetDifferentTermDBRecordsByFolder(string folderId, string termId);
        List<FileSystemRecordDto> GetFSDBRecords(List<Guid> ids);
        List<FileSystemRecordDto> GetFSConnectionUnderGroup(Guid connectionGroupId, int level);
        List<FileSystemRecordDto> GetFSManualRecords(List<Guid> ids);
        List<FileSystemRecordDto> GetFSDBRecordsByRecordsId(List<string> recordsId);
        List<FsRecordProcessDto> GetFSRecordsForAdsProcessing(List<string> recordsId);
        string RealStartFSDashBoard(JobRunBy JobRunType);
        string RealStartFSMyHubDashBoard(JobRunBy JobRunType, string param);

        string RealStartSPOnPremDashBoard(JobRunBy jobRunBy);

        List<Guid> UpdateFSDeleteRecord(List<FSExplorerDeleteDto> dtos);
        bool UpdateFSFolderSize(List<FolderSizeUpdateDto> dtos);
        string GetFSRecordId(int nodeType, Guid nodeId, Guid scopeId);
        string GetFSConnectionIdByItemId(Guid nodeId);
        string RealRunFSFolderReclassifyJob(JobRunBy JobRunType, string param);
        string RealRunFSFolderHoldJob(JobRunBy JobRunType, string param);
        #endregion

        #region global search action      
        Task<int> ChangeTermForGlobalSearchAsync(List<Guid> recordsId, SourceFlag flag, string jobId, ChangeTermOption changeTermOption, bool isJob);
        Task<string> RealRunGlobalSearchActionJobAsync(string param);
        Task<int> DeclareAsRecordForGlobalSearchAsync(List<Guid> ids, string jobId, string declareBy, bool isGlobalSearchActionJob = false);
        Task<int> UndeclareAsRecordForGlobalSearchAsync(List<Guid> ids, string jobId, string declareBy, bool isGlobalSearchActionJob = false);
        Task<int> DeclareTeamsRecordForGlobalSearchAsync(List<Guid> ids, string jobId, string declareBy, bool isGlobalSearchJob);
        Task<int> UndeclareTeamsRecordForGlobalSearchAsync(List<Guid> ids, string jobId, string declareBy, bool isGlobalSearchJob);
        #endregion
        bool IsFolderHasParentHold(List<Guid> recordIds, out List<string> holdingBoxes);

        Task<List<string>> GetRecordReleaseTimeAsync(List<Guid> recordIds);
        Task<SecurityTermPermissionDto> GetSecurityTermDtoAsync();

        #region sp on premise
        RAReturnMessage AddOrUpdateSPOnPremObject(RecordDto dto);
        bool IsSPOnPremObjectExist(Guid scopeId, Guid id);
        List<Guid> OnPremiseSPUpdateRecordsInExplorer(List<OnPremiseSPAzureTableEntityDto> dtos);
        bool CheckIsHoldRecord(Guid Id);
        List<OnPremiseSPListCacheDto> GetOnPremiseSPExplorerDataByListId(string listId, string scopeId, long sortTicks, int pageSize);
        #endregion

        FSDueRecordsDto GetFSDueRecords(SearchFilterParam searchFilterParam);
        Task<RecordsReturnMessage> ChangeTermRealTimeForAIAsync(ChangeTermType changeTermType, ChangeTermOption changeTermOption, string jobId);
        Task<int> ChangeTermForAIJobAsync(List<Guid> recordsId, SourceFlag flag, string jobId, ChangeTermType changeTermType, ChangeTermOption changeTermOption, bool isJob);

        Task<bool> SaveBarcodeStandardAsync(int barcodeType);
        Task<int> GetBarcodeStandardAsync();
        RecordDetailDto GetRelatedItemDetailsInfo(RelatedItemSubmitInfo submitInfo);
        RAReturnMessage SubmitRelatedItems(RelatedItemSubmit saveInfo);
        System.Threading.Tasks.Task AddArchiverItemsForFSAsync(string tenantGroupId, List<FSAzureTableEntityDto> entities,string jobId, bool isFSHighPerformanceMode = false);
        void RunSendEmailJobAsync(string jobId);
        void HandleCalculateZeroShotAccuracy(List<Guid> predictTermIds, ChangeTermType type);

        #region FS JPMC
        List<FileSystemRecordDto> QueryFileSystemRecords(string connectionId, List<Guid> ids);
        List<FsRecordProcessDto> QueryFileSystemRecords(string connectionId, List<string> ids);
        public bool HasJPMCConnectionRecord(string connectionId);
        #endregion

        #region Maestro AI
        bool ResetMARecordsForRemovedMLTerms(List<Guid> predictTermIds);
        #endregion

        System.Threading.Tasks.Task BuildHoldNotificationScheduleJob(UpdateHoldDto dto);
        Task<ExplorerResultInfo> SearchPhysicalRecordsAsync(string pageIndex, int pageSize, string value);
        Task<List<LocationPermissionDto>> GetEffectiveLocationPermissionsAsync();
        Task<List<RecordPermissionDto>> GetRecordsPermission(List<ExplorerRecordPermission> recordPermission);
        RAReturnMessage PhysicalMoves(List<PhysicalMoveRequest> moveRequests);
    }
}
