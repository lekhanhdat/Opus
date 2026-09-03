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
using AvePoint.GCommon.Utility.AzureBlobStorage;
using AvePoint.RA.Common;
using AvePoint.RA.Common.Configurations;
using AvePoint.RA.Common.Report;
using AvePoint.RA.Common.Retrying;
using AvePoint.RA.Common.RMRuleManagement;
using AvePoint.RA.Common.Util;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Configurations;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.Contract.JobMonitor;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RMWeb.CP;
using AvePoint.RA.Contract.RMWeb.Explorer;
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using AvePoint.RA.Contract.RMWeb.TemplateManagement;
using AvePoint.RA.Contract.TemplateManagement;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Model;
using AvePoint.RA.I18N.Core;
using AvePoint.RA.RACommonUtility.Common;
using AvePoint.RA.SharePoint.Common;
using RAArchiverCommon.Utility;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using static AvePoint.RA.RACommonUtility.Common.CommonUtilityForSpecialTenant;
using SharePointSettingUtility = AvePoint.RA.SharePoint.Common.SharePointSettingUtility;

namespace RAGlobalSearch.Export
{
    public class ExportSearchResult
    {
        protected AvePoint.RA.Contract.Services.IRALogger logger = RALogger.GetInstance(typeof(ExportSearchResult));
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

        private IGeneralSettingService mGeneralSettingService { get; set; }
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

        private IExplorerQueryService mExplorerQueryService { get; set; }
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

        private IRMScopeDao mRMScopeDao { get; set; }
        public IRMScopeDao RMScopeDao
        {
            get
            {
                if (mRMScopeDao == null)
                {
                    mRMScopeDao = (IRMScopeDao)PlatformWindsorManager.GetService(typeof(IRMScopeDao));
                }
                return mRMScopeDao;
            }
        }

        private ITaxonomyService mTaxonomyService { get; set; }
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

        private ILabelDao mLabelDao { get; set; }
        public ILabelDao RMLabelDao
        {
            get
            {
                if (mLabelDao == null)
                {
                    mLabelDao = (ILabelDao)PlatformWindsorManager.GetService(typeof(ILabelDao));
                }
                return mLabelDao;
            }
        }

        private ITermDao _termDao { get; set; }
        public ITermDao TermDao
        {
            get
            {
                if (_termDao == null)
                {
                    _termDao = (ITermDao)PlatformWindsorManager.GetService(typeof(ITermDao));
                }
                return _termDao;
            }
        }

        private static readonly IDownloadDataInfoDao DownloadDataInfoDao = PlatformWindsorManager.GetService<IDownloadDataInfoDao>();

        private static readonly RMRetryer Retryer = RMRetryerBuilder.CreateBuilder().Build();

        #endregion
        private string mJobId = string.Empty;
        private string JobId = string.Empty;
        private GlobalSearchExportDto mGlobalSearchExportDto;
        Dictionary<Guid, RMScope> mScopeMap = new Dictionary<Guid, RMScope>();
        Dictionary<Guid, string> mTermPathMap = new Dictionary<Guid, string>();
        private readonly int mMaxRecordCountForSingleFile = 65535;
        private bool mIsJob = false;
        private ExportSearchCache searchCache = new ExportSearchCache();
        private string FolderPath { get; set; }
        private string FullPath { get; set; }

        public ExportSearchResult(string mjobId, string jobId)
        {
            mJobId = mjobId;
            JobId = jobId;
            ReportMangerFactory.Instance.Init(mJobId, AvePoint.RA.Contract.JobMonitor.JobType.ExportSearchResult, true);
            JobInfoUpdater.UpdateJobState(mJobId, (int)JobStatus.InProgress);
            ReportManager.StartUpdateJobProgress();
        }
        public ExportSearchResult(GlobalSearchExportDto dto)
        {
            mGlobalSearchExportDto = dto;
        }

        public async Task RunAsync()
        {
            var downloadDataInfo = DownloadDataInfoDao.GetDownloadDataInfosByStatus(new List<int>() { (int)DownloadContentJobStatus.Wait }).Where(item => item.JobId == JobId).First();

            try
            {

                logger.Info("Start to run export search result job.");

                RMSubJob subJobWithContext = SubJobDao.GetSubJob(mJobId, true);
                var globalSearchExportDto = SerializerHelper.DeserializeByDataContractSerializer<GlobalSearchExportDto>(subJobWithContext.JobContext.Content);
                logger.Info("Get job message:{0}", subJobWithContext.JobContext.Content);

                FullPath = await GenerateSearchResultReportAsync(globalSearchExportDto, true);

                var fileInfo = await UploadBlobAsync();

                if (fileInfo != null)
                {
                    downloadDataInfo.FileSize = fileInfo.Length;
                }

                downloadDataInfo.BlobSasUri = await DownloadCenterUtility.GenerateSasUri();

                ExportSearchResultJobManager.HasSucceedDetail = true;

                UpdateDownloadDataInfo(downloadDataInfo, DownloadContentJobStatus.Finished);

                logger.Info("Export search result finished.");

            }
            catch (Exception e)
            {
                ExportSearchResultJobManager.HasFailedDetail = true;
                ExportSearchResultJobManager.JobComment = e.Message;
                logger.Error($"Export  for term failed ,{e}");
                UpdateDownloadDataInfo(downloadDataInfo, DownloadContentJobStatus.Failed);
            }
            finally
            {
                try
                {
                    if (!string.IsNullOrWhiteSpace(FolderPath))
                    {
                        DeleteFolder(FolderPath);
                        DeleteFile(FolderPath + JobMonitorConstants.ZIP);
                    }
                }
                catch (Exception e)
                {
                    logger.Error($"Delete folder/file failed. error: {e}");
                }
                ExportSearchResultJobManager.SetJobFinished();
                PerformanceMonitor.WritePerformanceResult();
            }
        }

        public async Task<string> ExportDirectlyAsync()
        {
            var folderPath = string.Empty;
            try
            {
                folderPath = await GenerateSearchResultReportAsync(mGlobalSearchExportDto);
            }
            catch (Exception e)
            {
                logger.Error("Failed to generate report, error:{0}", e.ToString());
                folderPath = string.Empty;
            }
            return folderPath;
        }

        #region Opus API Report

        public async Task<Tuple<ExportRowBuilderContext,string[]>> BuildReportRowContextAndHeadersAsync(List<SelectedColumn> selectedColumns, List<SelectedColumn> sharedColumns = null)
        {
            var processedColumns = PreProcessColumns(selectedColumns);
            var buildInColumnMapping = GetBuildInColumnDic(processedColumns);
            var allCustomColumns = await TemplateManagementService.GetAllColumnsAsync();
            var customColumnMapping = GetCustomColumnDic(processedColumns, allCustomColumns);
            var headers = AssembleSearchResultReportHeader(buildInColumnMapping, customColumnMapping, false);
            var customMetadataColumns = await TemplateManagementService.GetCustomMetadataColumnsAsync();
            var customColumnDic = GetAllCustomColumnIdNameMapping(allCustomColumns);
            var columnExtractors = BuildColumnExtractors(buildInColumnMapping, GeneralSettingService);
            var gls = await GeneralSettingService.GetGeneralSettingAsync();
            var context = new ExportRowBuilderContext
            {
                BuildInColumnMapping = buildInColumnMapping,
                CustomColumnMapping = customColumnMapping,
                CustomColumnDic = customColumnDic,
                CustomMetadataColumns = customMetadataColumns,
                ColumnExtractors = columnExtractors,
                GeneralSettings = gls
            };
            return Tuple.Create(context, headers);
        }
     
        public string[][] BuildRowsFromContext(List<BaseRecordDto> records, ExportRowBuilderContext ctx)
        {
            if (records == null || records.Count == 0) return Array.Empty<string[]>();

            return ConvertRecordToOpusAPIReportArray(
                records,
                ctx.BuildInColumnMapping,
                ctx.CustomColumnMapping,
                ctx.CustomColumnDic,
                ctx.CustomMetadataColumns,
                ctx.ColumnExtractors,
                ctx.GeneralSettings);
        }

        private Func<BaseRecordDto, GeneralSettingModel, string>[] BuildColumnExtractors(Dictionary<Guid, string> buildInColumnMapping, IGeneralSettingService generalSettingService)
        {
            var extractors = new Func<BaseRecordDto, GeneralSettingModel, string>[buildInColumnMapping.Count];
            int i = 0;
            foreach (var column in buildInColumnMapping)
            {
                var id = column.Key.ToString();
                extractors[i++] = id switch
                {
                    RecordBuildInColumnIds.SPOLocation => (r, _) => r.ExtensionValue ?? string.Empty,
                    RecordBuildInColumnIds.NameOrTitle => (r, _) => GetRecordFullPath(r) ?? string.Empty,
                    RecordBuildInColumnIds.UniqueId => (r, _) => r.RecordsId ?? string.Empty,
                    RecordBuildInColumnIds.Type => (r, _) => r.ExtensionForFile ?? string.Empty,
                    RecordBuildInColumnIds.Classification => (r, _) => GetRecordTermPath(r) ?? string.Empty,
                    RecordBuildInColumnIds.RuleName => (r, _) => r.RuleName ?? string.Empty,
                    RecordBuildInColumnIds.RuleAction => (r, _) => GetRuleAction(r) ?? string.Empty,
                    RecordBuildInColumnIds.HoldStatus => (r, _) => r.HoldStatus ? "Yes" : "No",
                    RecordBuildInColumnIds.HoldBy => (r, _) => r.HoldBy ?? string.Empty,
                    RecordBuildInColumnIds.ActionDueDate => (r, _) => I18NEntity.GetString(r.DisposalDueDate),
                    RecordBuildInColumnIds.HoldUntil => (r, gls) => r.HoldStatus ? generalSettingService.ConvertTiksToDateTime(gls, r.HoldReleaseTime, true).SimplifyFormatTime : string.Empty,
                    RecordBuildInColumnIds.HoldTitle => (r, _) => r.HoldTitle ?? string.Empty,
                    RecordBuildInColumnIds.Owners => (r, _) => r.RecordOwner ?? string.Empty,
                    RecordBuildInColumnIds.DeclaredRecord => (r, _) => r.DeclareAsRecord ? "Yes" : "No",
                    RecordBuildInColumnIds.CreatedBy => (r, _) => r.CreatedBy ?? string.Empty,
                    RecordBuildInColumnIds.ModifiedBy => (r, _) => r.ModifiedBy ?? string.Empty,
                    RecordBuildInColumnIds.LockedByRecordLabel => (r, _) => r.LockedByRecordLabel ? "Yes" : "No",
                    RecordBuildInColumnIds.CreatedDateInfo => (r, gls) => generalSettingService.ConvertTiksToDateTime(gls, r.TimeCreated, true).SimplifyFormatTime,
                    RecordBuildInColumnIds.ModifiedTime => (r, gls) => generalSettingService.ConvertTiksToDateTime(gls, r.TimeLastModified, true).SimplifyFormatTime,
                    RecordBuildInColumnIds.ArchivedTime => (r, gls) => r.TimeArchived == 0 ? string.Empty : generalSettingService.ConvertTiksToDateTime(gls, r.TimeArchived, true).SimplifyFormatTime,
                    _ => (_, __) => string.Empty
                };
            }
            return extractors;
        }

        private static string[] BuildRowFromExtractors(BaseRecordDto record, Func<BaseRecordDto, GeneralSettingModel, string>[] extractors, GeneralSettingModel gls)
        {
            var row = new string[extractors.Length];
            for (var i = 0; i < extractors.Length; i++)
            {
                row[i] = extractors[i](record, gls) ?? string.Empty;
            }
            return row;
        }
        #endregion

        private void DeleteFolder(string dir)
        {

            foreach (string f in Directory.GetFileSystemEntries(dir))
            {
                if (File.Exists(f))
                {
                    FileInfo fi = new FileInfo(f);
                    File.Delete(f);
                }
                else
                {
                    DeleteFolder(f);
                }
            }
            Directory.Delete(dir);

        }

        private void DeleteFile(string filePath)
        {
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
        }

        private async Task<string> GenerateSearchResultReportAsync(GlobalSearchExportDto globalSearchExportDto, bool isJob = false)
        {
            mIsJob = isJob;
            string folderPath = string.Empty;
            string fileFullPath = string.Empty;
            using (var performance0 = new PerformanceScope("ExportSearchResult.GenerateSearchResultReport", addToStatistics: mIsJob))
            {
                try
                {
                    string fileName = await GetFileName(); ;
                    folderPath = JobReportUtility.GetDownloadRecordExportReportTempleFolder(Guid.NewGuid().ToString());
                    FolderPath = folderPath;
                    if (!Directory.Exists(folderPath))
                    {
                        Directory.CreateDirectory(folderPath);
                    }
                    logger.Info("Start to generate search result report in {0}", folderPath);
                    var selectedColumns = PreProcessColumns(globalSearchExportDto.SelectedColumns);
                    var buildInColumnMapping = GetBuildInColumnDic(selectedColumns);
                    logger.Info("Build in column count:{0} Columns:{1}", buildInColumnMapping.Count, string.Join(";", buildInColumnMapping.Values.ToList()));
                    var allCustomColumns = await TemplateManagementService.GetAllColumnsAsync();
                    var customMetadataColumns = await TemplateManagementService.GetCustomMetadataColumnsAsync();
                    var customColumnMapping = GetCustomColumnDic(selectedColumns, allCustomColumns);
                    logger.Info("Custom column count:{0} Columns:{1}", customColumnMapping.Count, string.Join(";", customColumnMapping.Keys.ToList()));
                    var customColumnDic = GetAllCustomColumnIdNameMapping(allCustomColumns);

                    var gls = await GeneralSettingService.GetGeneralSettingAsync();
                    globalSearchExportDto.FilterInfo.PagingInfo = new ExplorerPagingInfo()
                    {
                        PageSize = 200,
                        PageIndex = ""
                    };

                    ExplorerPagingInfo pageInfo;
                    int xlsxFileIndex = 0;
                    int totalCount = 0;

                    List<BaseRecordDto> records = new List<BaseRecordDto>();
                    do
                    {
                        ExplorerResultInfo result = null;
                        using (var performance = new PerformanceScope("ExportSearchResult.QueryDataListWithoutTotal", addToStatistics: mIsJob))
                        {
                            result = await ExplorerQueryService.QueryDataListWithoutTotalAsync(globalSearchExportDto.FilterInfo);
                            logger.Debug("Query data finished, count:{0}", result?.Datas != null ? result.Datas.Count : 0);
                        }
                        if (result != null && result.Datas != null && result.Datas.Count > 0)
                        {
                            records.AddRange(result.Datas);
                            if (mIsJob)
                            {
                                ReportManager.Increase(records.Count / 2);
                            }
                            if (records.Count >= mMaxRecordCountForSingleFile)
                            {
                                var hasPhysicalRecord = records.Count(r => r.SourceFlag == (int)SourceFlag.Physical) > 0 || customMetadataColumns.Count > 0;
                                var datas = ConvertRecordToReportArray(records, buildInColumnMapping, customColumnMapping, customColumnDic, customMetadataColumns, gls);
                                fileFullPath = xlsxFileIndex == 0 ?
                                                 AvePoint.GCommon.Utility.SecurityUtils.SafeCombinePath(folderPath, fileName)
                                                 : AvePoint.GCommon.Utility.SecurityUtils.SafeCombinePath(folderPath, fileName + "_" + xlsxFileIndex.ToString());
                                string[][] headerArray = new string[1][];
                                var headers = AssembleSearchResultReportHeader(buildInColumnMapping, customColumnMapping, hasPhysicalRecord);
                                headerArray[0] = headers;
                                var dataWithHeader = headerArray.Concat(datas).ToArray();
                                ReportUtil.CreateExcel(fileFullPath, "Sheet", dataWithHeader);
                                logger.Info("Create file successfully. File:{0}", fileFullPath);
                                xlsxFileIndex++;
                                if (mIsJob)
                                {
                                    ReportManager.Increase(records.Count / 2);
                                }
                                totalCount += records.Count;
                                records.Clear();
                            }
                        }
                        pageInfo = result?.PagingInfo;
                    }
                    while (pageInfo != null && pageInfo.HasNextPage);

                    if (records.Count > 0)
                    {
                        var hasPhysicalRecord = records.Count(r => r.SourceFlag == (int)SourceFlag.Physical || r.SourceFlag >= 1000) > 0 || customMetadataColumns.Count > 0;
                        var datas = ConvertRecordToReportArray(records, buildInColumnMapping, customColumnMapping, customColumnDic, customMetadataColumns, gls);
                        fileFullPath = xlsxFileIndex == 0 ?
                                         AvePoint.GCommon.Utility.SecurityUtils.SafeCombinePath(folderPath, fileName)
                                         : AvePoint.GCommon.Utility.SecurityUtils.SafeCombinePath(folderPath, fileName + "_" + xlsxFileIndex.ToString());
                        string[][] headerArray = new string[1][];
                        var headers = AssembleSearchResultReportHeader(buildInColumnMapping, customColumnMapping, hasPhysicalRecord);
                        headerArray[0] = headers;
                        var dataWithHeader = headerArray.Concat(datas).Select(row => row.Select(ReplaceInvalidXmlCharacters).ToArray()).ToArray();
                        ReportUtil.CreateExcel(fileFullPath, "Sheet", dataWithHeader);
                        logger.Info("Create file successfully. File:{0}", fileFullPath);
                        if (mIsJob)
                        {
                            ReportManager.Increase(records.Count / 2);
                        }
                        totalCount += records.Count;
                        records.Clear();
                    }

                }
                catch (Exception e)
                {
                    logger.Error("An error occurred while generating search result report. Error:{0}", e.ToString());
                    try
                    {
                        if (!string.IsNullOrWhiteSpace(fileFullPath))
                        {
                            //DeleteFolder(folderPath);
                            DeleteFile(fileFullPath);
                            logger.Info("Delete temp file successfully in : {0}", folderPath);
                        }
                    }
                    catch (Exception ex)
                    {
                        logger.Error("An error occurred while deleting temp files, path:{0}, error:{1}", folderPath, ex.ToString());
                    }
                    throw e;
                }
            }
            return fileFullPath;
        }

        private string ReplaceInvalidXmlCharacters(string input)
        {
            if (string.IsNullOrEmpty(input))
                return input;

            var sb = new StringBuilder();
            foreach (var c in input)
            {
                if (XmlConvert.IsXmlChar(c))
                {
                    sb.Append(c);
                }
                else
                {
                    logger.Warn($"Invalid character detected: '\\u{(int)c:X4}' in string: {input}");
                    sb.Append('-');
                }
            }

            return sb.ToString();
        }

        private List<SelectedColumn> PreProcessColumns(List<SelectedColumn> columns)
        {
            var isSupportRecordLabel = AccountUtility.IsSupportRecordLabel();
            List<SelectedColumn> selectedColumns = new List<SelectedColumn>();
            foreach (var column in columns)
            {
                if (!isSupportRecordLabel)
                {
                    if(RecordBuildInColumnIds.LockedByRecordLabel.Equals(column.UniqueId, StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }
                }
                if (!selectedColumns.Any(c => c.UniqueId == column.UniqueId))
                {
                    selectedColumns.Add(column);
                }
            }
            return selectedColumns;
        }



        private string[] AssembleSearchResultReportHeader(Dictionary<Guid, string> buildInColumnMapping, Dictionary<string, List<Guid>> customColumnNameIdsMapping, bool containsPhysical)
        {
            string[] headers = null;
            if (containsPhysical)
            {
                headers = buildInColumnMapping.Values.ToList().ToArray();
                if (customColumnNameIdsMapping != null && customColumnNameIdsMapping.Count > 0)
                {
                    var customHeaders = customColumnNameIdsMapping.Keys.ToArray();
                    headers = headers.Concat(customHeaders).ToArray();
                }
            }
            else
            {
                var buildColumnNames = buildInColumnMapping.Where(k => !k.Key.ToString().Equals(RecordBuildInColumnIds.OnLoan, StringComparison.OrdinalIgnoreCase)
                && !k.Key.ToString().Equals(RecordBuildInColumnIds.LoanBy, StringComparison.OrdinalIgnoreCase)).Select(n => n.Value).ToList();
                headers = buildColumnNames.ToArray();
            }
            return headers;
        }

        private string[][] ConvertRecordToReportArray(List<BaseRecordDto> records, Dictionary<Guid, string> buildInColumnMapping, Dictionary<string, List<Guid>> customColumnMapping, Dictionary<Guid, TemplateColumn4Display> customColumnDic, List<TemplateColumn4Display> customMetadataColumns, GeneralSettingModel generalSettingModel)
        {
            string[][] datas = new string[records.Count][];
            int index = 0; ;
            foreach (var record in records)
            {
                var buildInColumnValues = GetRecordBuildInColumnValues(record, buildInColumnMapping, generalSettingModel);
                string[] customColumnValues = null;
                if (record.SourceFlag == (int)SourceFlag.Physical || record.SourceFlag >= 1000 || customMetadataColumns.Count > 0 && customColumnMapping.Count > 0)
                {
                    customColumnValues = GetRecordCustomColumnValues(record, customColumnMapping, customColumnDic, generalSettingModel);
                }
                datas[index] = customColumnValues != null && customColumnValues.Count() > 0 ? buildInColumnValues.Concat(customColumnValues).ToArray() : buildInColumnValues;
                index++;
            }
            return datas;
        }

        private string[][] ConvertRecordToOpusAPIReportArray(List<BaseRecordDto> records, Dictionary<Guid, string> buildInColumnMapping, Dictionary<string, List<Guid>> customColumnMapping, Dictionary<Guid, TemplateColumn4Display> customColumnDic, List<TemplateColumn4Display> customMetadataColumns, Func<BaseRecordDto, GeneralSettingModel, string>[] columnExtractors, GeneralSettingModel generalSettingModel)
        {
            string[][] datas = new string[records.Count][];
            int index = 0;
            foreach (var record in records)
            {
                var buildInColumnValues = BuildRowFromExtractors(record, columnExtractors, generalSettingModel);
                string[] customColumnValues = null;
                if (customMetadataColumns.Count > 0 && customColumnMapping.Count > 0)
                {
                    customColumnValues = GetRecordCustomColumnValues (record, customColumnMapping, customColumnDic, generalSettingModel);
                }
                datas[index] = customColumnValues != null && customColumnValues.Count() > 0 ? buildInColumnValues.Concat(customColumnValues).ToArray() : buildInColumnValues;
                index++;
            }
            return datas;
        }

        private string[] GetRecordBuildInColumnValues(BaseRecordDto record, Dictionary<Guid, string> buildInColumnMapping, GeneralSettingModel generalSettingModel)
        {
            var data = new string[buildInColumnMapping.Count];
            int index = 0;
            foreach (var column in buildInColumnMapping)
            {
                string value = string.Empty;
                switch (column.Key.ToString())
                {
                    case RecordBuildInColumnIds.NameOrTitle:
                        value = GetRecordFullPath(record);
                        break;
                    case RecordBuildInColumnIds.UniqueId:
                        value = record.RecordsId;
                        break;
                    case RecordBuildInColumnIds.Type:
                        value = record.ExtensionForFile;
                        break;
                    case RecordBuildInColumnIds.Classification:
                        value = GetRecordTermPath(record);
                        break;
                    case RecordBuildInColumnIds.RuleName:
                        value = record.RuleName;
                        break;
                    case RecordBuildInColumnIds.RuleAction:
                        value = GetRuleAction(record);
                        break;
                    case RecordBuildInColumnIds.HoldStatus:
                        value = ConvertBool2Str(record.HoldStatus);
                        break;
                    case RecordBuildInColumnIds.PlaceOnHoldBy:
                        value = record.HoldBy;
                        break;
                    case RecordBuildInColumnIds.ActionDueDate:
                        value = record.DisposalDueDate;
                        break;
                    case RecordBuildInColumnIds.HoldTitle:
                        value = record.HoldTitle;
                        break;
                    case RecordBuildInColumnIds.HoldUntil:
                        value = record.HoldStatus ? GeneralSettingService.ConvertTiksToDateTime(generalSettingModel, record.HoldReleaseTime, true).SimplifyFormatTime : string.Empty;
                        break;
                    case RecordBuildInColumnIds.Owners:
                        value = record.RecordOwner;
                        break;
                    case RecordBuildInColumnIds.CreatedDateInfo:
                        value = GeneralSettingService.ConvertTiksToDateTime(generalSettingModel, record.TimeCreated, true).SimplifyFormatTime;
                        break;
                    case RecordBuildInColumnIds.DeclaredRecord:
                        value = ConvertBool2Str(record.DeclareAsRecord);
                        break;
                    case RecordBuildInColumnIds.CreatedBy:
                        value = record.CreatedBy;
                        break;
                    case RecordBuildInColumnIds.ModifiedBy:
                        value = record.ModifiedBy;
                        break;
                    case RecordBuildInColumnIds.ModifiedTime:
                        value = GeneralSettingService.ConvertTiksToDateTime(generalSettingModel, record.TimeLastModified, true).SimplifyFormatTime;
                        break;
                    case RecordBuildInColumnIds.ArchivedTime:
                        value = record.TimeArchived == 0 ? string.Empty : GeneralSettingService.ConvertTiksToDateTime(generalSettingModel, record.TimeArchived, true).SimplifyFormatTime;
                        break;
                    case RecordBuildInColumnIds.OnLoan:
                        if (record.SourceFlag == (int)SourceFlag.Physical)
                        {
                            value = ConvertBool2Str(record.PersonHold);
                        }
                        break;
                    case RecordBuildInColumnIds.LoanBy:
                        if (record.SourceFlag == (int)SourceFlag.Physical)
                        {
                            value = record.PersonHoldBy;
                        }
                        break;
                    case RecordBuildInColumnIds.LockedByRecordLabel:
                        value = ConvertBool2Str(record.LockedByRecordLabel);
                        break;
                    default:
                        logger.Warn("Build in column id not exist, column id:{0} record id:{1}", column.Key, record.NodeId);
                        break;
                }
                data[index] = value;
                index++;
            }
            return data;
        }

        private string ConvertBool2Str(bool yes)
        {
            if (yes)
            {
                return I18NEntity.GetString("RM_PRM_PRE_Cell_HoldStatusYes");
            }
            else
            {
                return I18NEntity.GetString("RM_PRM_PRE_Cell_HoldStatusNo");
            }
        }

        private string GetRecordFullPath(BaseRecordDto record)
        {
            string fullPath = string.Empty;
            PerformanceScope performance = null;
            try
            {
                if (mIsJob)
                {
                    performance = new PerformanceScope("ExportSearchResult.GetRecordFullPath", addToStatistics: mIsJob);
                }
                switch (record.SourceFlag)
                {
                    case (int)SourceFlag.SharePoint:
                    case (int)SourceFlag.SharePointOnPrem:
                    case (int)SourceFlag.OneDrive:
                    case (int)SourceFlag.Teams:
                        if (!string.IsNullOrWhiteSpace(record.FullPath))
                        {
                            fullPath = record.FullPath;
                        }
                        else
                        {
                            if (mScopeMap.ContainsKey(record.ScopeId))
                            {
                                var sPath = mScopeMap[record.ScopeId];
                                fullPath = WebUtil.MakeFullUrl(sPath?.FullPath, record.DirPath);
                            }
                            else
                            {
                                var scope = RMScopeDao.GetScopeInfoByIds(new List<Guid>() { record.ScopeId }).Values?.FirstOrDefault();
                                if (scope != null)
                                {
                                    fullPath = WebUtil.MakeFullUrl(scope.FullPath, record.DirPath);
                                    mScopeMap.Add(record.ScopeId, scope);
                                }
                                else
                                {
                                    SharePointSettingUtility SPUtility = new SharePointSettingUtility();
                                    var site = SPUtility.GetRemoteSiteCollection(record.AveSiteId.ToString());
                                    fullPath = site == null ? string.Empty : WebUtil.MakeFullUrl(site.url, record.DirPath);
                                    logger.Info("get site info from dao:siteId:{0}, siteUrl:{1},id:{2}", record.AveSiteId.ToString(), site?.url, record?.Id);
                                    if (site != null)
                                    {
                                        var newScope = new RMScope()
                                        {
                                            FullPath = site.url,
                                            ScopeId = record.ScopeId,
                                            ScopeName = site.Name,
                                            IsRemoved = false,
                                        };
                                        RMScopeDao.AddOrUpateSiteScope(newScope);
                                        mScopeMap.Add(record.ScopeId, newScope);
                                    }
                                }

                            }
                        }
                        if (record.ExtensionForFile == "RM_RDM_RecordDetails_DataType_SPItem")
                        {
                            fullPath = WebUtil.GetListItemRealPath(fullPath);
                        }
                        break;
                    case (int)SourceFlag.Exchange:
                        fullPath = string.Format(AvePoint.RA.Common.RecordsConstants.EXOLocationFormat, record.EmailAddress, record.DirPath, new DateTime(record.TimeCreated).ToString("R"));
                        break;
                    case (int)SourceFlag.Google:
                    case (int)SourceFlag.Physical:
                        fullPath = record.LeafName;
                        //ExplorerService.GetPhysicalObjectFullPath(record.Id) + "/" + record.LeafName;
                        break;
                    case (int)SourceFlag.FileSystem:
                        fullPath = record.DirPath + "/" + record.LeafName;
                        break;
                    case (int)SourceFlag.AzureFileShare:
                        fullPath = record.DirPath + "/" + record.LeafName;
                        break;
                    case (int)SourceFlag.Box:
                        fullPath = record.DirPath;
                        break;
                    case >= 1000:
                        fullPath = record.LeafName;
                        break;
                    default:
                        logger.Warn("Invalid source flag, node id:{0} flag:{1}", record.NodeId, record.SourceFlag);
                        break;
                }
            }
            catch (Exception e)
            {
                logger.Error("An error occurred while getting full path. NodeId:{0} Error:{1}", record.NodeId, e.ToString());
            }
            finally
            {
                if (mIsJob && performance != null)
                {
                    performance.Dispose();
                }
            }
            return fullPath;
        }

        private string GetRecordTermPath(BaseRecordDto record)
        {
            if (record.SourceFlag == (int)SourceFlag.Google && !string.IsNullOrEmpty(record.TermName))
            {
                RMTerm term;
                if (!string.IsNullOrEmpty(record.TermName) && record.TermId == Guid.Empty)
                {
                    term = TermDao.GetTermByNameAndScopeId(record.TermName, record.ScopeId);
                }
                else
                {
                    term = TermDao.GetRMTermByUniqueId(record.TermId);
                }

                if (term == null)
                {
                    return record.TermName;
                }
                var termFullPath = TermDao.GetTermNamePath(term.Id);
                if (string.IsNullOrEmpty(termFullPath))
                {
                    return record.TermName;
                }
                return termFullPath;
            }

            string termPath = string.Empty;
            if (record.TermId != Guid.Empty)
            {
                if (mTermPathMap.ContainsKey(record.TermId))
                {
                    termPath = mTermPathMap[record.TermId];
                }
                else
                {
                    if (record.SourceFlag == (int)SourceFlag.Google)
                    {
                        var label = RMLabelDao.GetLabelByUniqueId(record.TermId);
                        if (label == null)
                        {
                            logger.Warn($"Could not found the label with uniqued id [{record.TermId}]");
                            return string.Empty;
                        }
                        termPath = label.Name;
                        mTermPathMap.Add(record.TermId, termPath);
                        return termPath;
                    }
                    termPath = TaxonomyService.GetTermPathByTermId(record.TermId);
                    mTermPathMap.Add(record.TermId, termPath);
                }
            }
            return termPath;
        }

        private string[] GetRecordCustomColumnValues(BaseRecordDto record, Dictionary<string, List<Guid>> customColumnMapping, Dictionary<Guid, TemplateColumn4Display> customColumnDic, GeneralSettingModel generalSettingModel)
        {
            var data = new string[customColumnMapping.Count];
            int index = 0;
            foreach (var column in customColumnMapping)
            {
                string value = string.Empty;
                if (record.CustomColumnDic != null && record.CustomColumnDic.Keys.Any(k => column.Value.Contains(new Guid(k))))
                {
                    var tempColumnId = record.CustomColumnDic.Keys.Where(k => column.Value.Contains(new Guid(k))).First();
                    if (customColumnDic.ContainsKey(new Guid(tempColumnId)))
                    {
                        if (tempColumnId.Equals(DefaultColumnIDs.HomeLocation))
                        {
                            value = searchCache?.GetPhyNodeHomeLocation(record);
                        }
                        else
                        {
                            value = GetCustomColumnValue(record.CustomColumnDic[tempColumnId], customColumnDic[new Guid(tempColumnId)], generalSettingModel);
                        }
                    }
                    else
                    {
                        logger.Warn("Custom column not found, column id:{0}", tempColumnId);
                    }
                }
                data[index] = value;
                index++;
            }
            return data;
        }

        private string GetCustomColumnValue(CustomColumn column, TemplateColumn4Display templateColumn, GeneralSettingModel generalSettingModel)
        {
            string value = string.Empty;
            if (templateColumn == null)
            {
                return value;
            }
            switch (templateColumn.ColumnType)
            {
                case AvePoint.RA.Contract.TemplateManagement.ColumnType.SingleText:
                case AvePoint.RA.Contract.TemplateManagement.ColumnType.MultipleText:
                case AvePoint.RA.Contract.TemplateManagement.ColumnType.Number:
                    value = column.Value;
                    break;
                case AvePoint.RA.Contract.TemplateManagement.ColumnType.DateTime:
                    value = GeneralSettingService.ConvertTiksToDateTime(generalSettingModel, column.Date.Ticks, true).SimplifyFormatTime;
                    break;
                case AvePoint.RA.Contract.TemplateManagement.ColumnType.PeopleOrGroup:
                    if (column.Users != null && column.Users.Count > 0)
                    {
                        value = string.Join(",", column.Users.Select(u => u.DisplayName).ToList());
                    }
                    break;
                case AvePoint.RA.Contract.TemplateManagement.ColumnType.SingleChoice:
                case AvePoint.RA.Contract.TemplateManagement.ColumnType.Taxonomy:
                    value = column.Name;
                    break;
                case AvePoint.RA.Contract.TemplateManagement.ColumnType.MultipleChoice:
                    if (column.MultiChoice != null && column.MultiChoice.Count > 0)
                    {
                        value = string.Join(",", column.MultiChoice.Select(u => u.Name).ToList());
                    }
                    break;
                case AvePoint.RA.Contract.TemplateManagement.ColumnType.YesOrNo:
                    value = column.YesOrNo;
                    break;
                default:
                    logger.Warn("Custom column type not exist, column type:{0} column id:{1}", templateColumn.ColumnType.ToString(), templateColumn.UniqueId.ToString());
                    break;
            }
            return value;
        }

        private string GetRuleAction(BaseRecordDto record)
        {
            List<string> avtionKeys = null;
            switch (record.SourceFlag)
            {
                case (int)SourceFlag.Physical:
                    avtionKeys = RuleHelper.ParseDisposalActionListForPhysical(record.DisposalAction);
                    if (avtionKeys.IsNullOrEmpty())
                    {
                        logger.Warn("Failed to get physical disposal action. Node id:{0} DisposalAction:{1}", record.NodeId, record.DisposalAction);
                    }
                    break;
                case (int)SourceFlag.FileSystem:
                case (int)SourceFlag.AzureFileShare:
                    avtionKeys = RuleHelper.ParseDisposalActionListForFS(record.DisposalAction);
                    if (avtionKeys.IsNullOrEmpty())
                    {
                        logger.Warn("Failed to get fs disposal action. Node id:{0} DisposalAction:{1}", record.NodeId, record.DisposalAction);
                    }
                    break;
                case (int)SourceFlag.Box:
                    avtionKeys = RuleHelper.ParseDisposalActionListForBox(record.DisposalAction);
                    if (avtionKeys.IsNullOrEmpty())
                    {
                        logger.Warn("Failed to get box disposal action. Node id:{0} DisposalAction:{1}", record.NodeId, record.DisposalAction);
                    }
                    break;
                case (int)SourceFlag.Exchange:
                    avtionKeys = RuleHelper.ParseDisposalActionListForSP(record.ExchangeDisposalAction, (SourceFlag)record.SourceFlag);
                    if (avtionKeys.IsNullOrEmpty())
                    {
                        logger.Warn("Failed to get exo disposal action. Node id:{0} DisposalAction:{1}", record.NodeId, record.ExchangeDisposalAction);
                    }
                    break;
                case (int)SourceFlag.SharePoint:
                case (int)SourceFlag.SharePointOnPrem:
                case (int)SourceFlag.OneDrive:
                case (int)SourceFlag.Teams:
                    avtionKeys = RuleHelper.ParseDisposalActionListForSP(record.DisposalAction, (SourceFlag)record.SourceFlag);
                    if (avtionKeys.IsNullOrEmpty())
                    {
                        logger.Warn("Failed to get disposal action. Node id:{0} DisposalAction:{1}", record.NodeId, record.DisposalAction);
                    }
                    break;
                case (int)SourceFlag.Google:
                    avtionKeys = RuleHelper.ParseDisposalActionListForGoogle(record.DisposalAction);
                    if (avtionKeys.IsNullOrEmpty())
                    {
                        logger.Warn("Failed to get google disposal action. Node id:{0} DisposalAction:{1}", record.NodeId, record.DisposalAction);
                    }
                    break;
                case >= 1000:
                    avtionKeys = RuleHelper.ParseDisposalActionListForFS(record.DisposalAction);
                    if (avtionKeys.IsNullOrEmpty())
                    {
                        logger.Warn("Failed to get fs disposal action. Node id:{0} DisposalAction:{1}", record.NodeId, record.DisposalAction);
                    }
                    break;
                default:
                    break;
            }
            return GetDisplayActionStr(avtionKeys);
        }

        private string GetDisplayActionStr(List<string> actionKeys)
        {
            if (actionKeys.IsNullOrEmpty())
            {
                return string.Empty;
            }
            else
            {
                return string.Join("; ", actionKeys.ConvertAll(key => I18NEntity.GetString(key)));
            }
        }

        private Dictionary<Guid, string> GetBuildInColumnDic(List<SelectedColumn> selectedColumns)
        {
            return selectedColumns.Where(c => RecordBuildInColumnIds.BuildInColumns.Contains(c.UniqueId)).ToDictionary(c => new Guid(c.UniqueId), c => c.DisplayName);
        }

        //对于custom column，可能出现Name相同Unique ID不同。生成report时只显示一个column name。此方法返回的key为column name，value为column id的集合
        private Dictionary<string, List<Guid>> GetCustomColumnDic(List<SelectedColumn> selectedColumns, List<TemplateColumn4Display> allColumns)
        {
            Dictionary<string, List<Guid>> customColumnDic = new Dictionary<string, List<Guid>>();
            var customColumns = selectedColumns.Where(c => !RecordBuildInColumnIds.BuildInColumns.Contains(c.UniqueId));
            foreach (var column in customColumns)
            {
                if (!customColumnDic.ContainsKey(column.DisplayName))
                {
                    var tempColumn = allColumns.Where(c => GetTemplateColumnUniqueId(c) == column.UniqueId || c.UniqueId.ToString().Equals(column.UniqueId, StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
                    if (tempColumn != null)
                    {
                        customColumnDic.Add(column.DisplayName, tempColumn.IdsWithDuplicateName);
                    }
                    else
                    {
                        logger.Warn("Custom column not found, namehash:{0}", column.UniqueId);
                    }
                }
            }
            return customColumnDic;
        }

        private string GetTemplateColumnUniqueId(TemplateColumn4Display column)
        {
            string uniqueId = column.UniqueId.ToString();
            if (DefaultColumnIDs.AllIDs.Contains(uniqueId))
            {
                return uniqueId;
            }
            else
            {
                string temp = column.ColumnName + AvePoint.RA.Contract.Explorer.ColumnType.GetName(column.ColumnType.GetType(), column.ColumnType);
                return HashCodeHelper.StringHash(temp).ToString();
            }
        }

        public Guid NameHash(TemplateColumn4Display templateColumn4Display)
        {

            if (DefaultColumnIDs.AllIDs.Contains(templateColumn4Display.UniqueId.ToString()))
            {
                return templateColumn4Display.UniqueId;
            }
            else
            {
                string temp = templateColumn4Display.ColumnName + AvePoint.RA.Contract.Explorer.ColumnType.GetName(templateColumn4Display.ColumnType.GetType(), templateColumn4Display.ColumnType);
                return HashCodeHelper.StringHash(temp);
            }

        }

        private Dictionary<Guid, TemplateColumn4Display> GetAllCustomColumnIdNameMapping(List<TemplateColumn4Display> allColumns)
        {
            Dictionary<Guid, TemplateColumn4Display> mapping = new Dictionary<Guid, TemplateColumn4Display>();
            foreach (var column in allColumns)
            {
                if (column.IdsWithDuplicateName != null && column.IdsWithDuplicateName.Count > 1)
                {
                    foreach (var id in column.IdsWithDuplicateName)
                    {
                        if (!mapping.ContainsKey(id))
                        {
                            mapping.Add(id, column);
                        }
                    }
                }
                else
                {
                    if (!mapping.ContainsKey(column.UniqueId))
                    {
                        mapping.Add(column.UniqueId, column);
                    }
                }
            }
            return mapping;
        }
        private async Task<string> GetFileName()
        {
            DateTime nowTime = DateTime.UtcNow;
            string nowTimeStr = (await GeneralSettingService.ConvertTiksToDateTimeAsync(nowTime.Ticks, false)).DataTime.ToString(AveDateTimeUtility.DATETYPE022);
            return I18NEntity.GetString("RM_JM_ExportSearchResultReport") + "_" + nowTimeStr + ".xlsx";
        }

        private void UpdateDownloadDataInfo(RMDownloadDataInfo DownCenterInfo, DownloadContentJobStatus downloadStatus)
        {
            using (new PerformanceScope("Update download data ", $"Download data status is {downloadStatus}")) ;
            {
                DownCenterInfo.JobStatus = (int)downloadStatus;
                var success = DownloadDataInfoDao.UpdateDownloadInfo(DownCenterInfo);
                if (success)
                {
                    logger.Info($"Update download file status to {downloadStatus} finished.");
                }
                else
                {
                    logger.Info($"Update download file status to {downloadStatus} failed, retry update.");
                    success = DownloadDataInfoDao.UpdateDownloadInfo(DownCenterInfo);
                    var status = success ? "finished" : "failed";
                    logger.Info($"Update retry download file {status}.");
                }
            }
        }

        private async Task<FileInfo> UploadBlobAsync()
        {
            using (new PerformanceScope("Upload blob to azure storage", "", true))
            {
                AvePoint.GCommon.ZipUtil.ZipFolder(FolderPath, FolderPath + ".zip", Encoding.UTF8);
                var customId = TenantLocalValue.LogonGroupId;
                var blobName = SecurityUtils.SafeCombinePath(customId, JobId + ".zip");
                try
                {
                    await Retryer.RetryAsync(() =>
                    {
                        blobName = DownloadCenterUtility.UploadStorageForDownloadCenter(blobName, FolderPath + ".zip");
                        logger.Info($"Upload export search result success");
                        return Task.CompletedTask;
                    });
                }
                catch (Exception e)
                {
                    logger.Error($"Upload export search result failed,error is :{e}");
                    throw;
                }

                logger.Info($"finish to upload blob name:{blobName}");
                return new FileInfo(FolderPath + ".zip");
            }
        }
    }

    public class ExportRowBuilderContext
    {
        public Dictionary<Guid, string> BuildInColumnMapping { get; init; }
        public Dictionary<string, List<Guid>> CustomColumnMapping { get; init; }
        public Dictionary<Guid, TemplateColumn4Display> CustomColumnDic { get; init; }
        public List<TemplateColumn4Display> CustomMetadataColumns { get; init; }
        public Func<BaseRecordDto, GeneralSettingModel, string>[] ColumnExtractors { get; init; }
        public GeneralSettingModel GeneralSettings { get; init; }
    }
}
