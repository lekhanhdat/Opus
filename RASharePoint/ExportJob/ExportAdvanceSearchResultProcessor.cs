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
using Aspose.Pdf.Operators;
using AvePoint.Common;
using AvePoint.GCommon;
using AvePoint.GCommon.Contract.CommonFilter;
using AvePoint.GCommon.Contract.Media.Object;
using AvePoint.GCommon.Contract.Server.Common;
using AvePoint.GCommon.Contract.Server.Common.BackupDataSearch;
using AvePoint.GCommon.Contract.Server.Common.RemoteNode;
using AvePoint.GCommon.Contract.Server.ControlPanel;
using AvePoint.GCommon.Contract.Server.ControlPanel.Object;
using AvePoint.GCommon.Contract.Server.ControlPanel.Office365;
using AvePoint.GCommon.Contract.Server.Job.Object;
using AvePoint.GCommon.Contract.Storage.Entity;
using AvePoint.GCommon.Contract.StorageOptimization.Object;
using AvePoint.GCommon.Utility;
using AvePoint.GCommon.Utility.AzureBlobStorage;
using AvePoint.GCommon.Utility.Exceptions;
using AvePoint.Item.Restore;
using AvePoint.Media.Common;
using AvePoint.Media.Core.Index;
using AvePoint.Media.Service;
using AvePoint.Media.Service.ArchiverBackup;
using AvePoint.Media.Service.DomainModel;
using AvePoint.RA.Common;
using AvePoint.RA.Common.Configurations;
using AvePoint.RA.Common.Retrying;
using AvePoint.RA.Common.Util;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract;
using AvePoint.RA.Contract.ArchivedFullTextIndex;
using AvePoint.RA.Contract.Archiver;
using AvePoint.RA.Contract.Configurations;
using AvePoint.RA.Contract.JobMonitor;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RMWeb.CP;
using AvePoint.RA.Contract.RMWeb.StorageDevice;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Dao.Impl;
using AvePoint.RA.DB.Model;
using AvePoint.RA.RACommonUtility.Common;
using AvePoint.RA.RACommonUtility.Encryption;
using AvePoint.RA.Service.Services.JobMonitor.Util;
using AvePoint.RA.SharePoint.ArchiverCommon;
using AvePoint.RA.SharePoint.Common;
using AvePoint.Wrapper.Common;
using Cloud.Sdk.EDiscovery.Services;
using Google.Apis.Storage.v1;
using Media.Common.ClassicStorageApi;
using Media.Service.ArchiverBackup.Restore;
using Merged18NResources.MediaServiceArchiverBackup;
using Microsoft.SharePoint.Client.RecordsRepository;
using Microsoft.SqlServer.Management.SqlParser.Metadata;
using RAArchiverCommon;
using RAExportCommon;
using RecordsHotfixMaintenanceService;
using Storage;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using Util;
using static AvePoint.RA.RACommonUtility.Common.CommonUtilityForSpecialTenant;
using CommonFilter = AvePoint.GCommon.Contract.CommonFilter;

namespace AvePoint.RA.SharePoint.ExportJob
{
    public class ExportAdvanceSearchResultProcessor
    {
        private const int MaxFileSizeInBytes = 10 * 1024 * 1024; // 10 MB, adjust as needed
        IXSystem destinationPhysicalDevice;

        public IIndexProcessor<ArchiverIndexProcessorParameter> IndexMainProcessor { get; set; }

        private JobContext jobContext = null;
        public JobReportImps mJobreport;
        private string JobId = string.Empty;
        private string zipPassword = string.Empty;
        private string tempRestoreFolder;
        private FileInfo? fileInfo;
        private static readonly AveLogger mLog = AveLogger.GetInstance(typeof(ExportAdvanceSearchResultProcessor));
        private RMAesEncryptorWrapper AesEncryptorWrapper => new();
        public IDownloadDataInfoDao DownloadDataInfoDao => PlatformWindsorManager.GetService<IDownloadDataInfoDao>();
        public IArchiverSiteMasterIndexDao ArchiverSiteMasterIndexService => PlatformWindsorManager.GetService<IArchiverSiteMasterIndexDao>();
        private IStorageDeviceService StorageDeviceService => PlatformWindsorManager.GetService<IStorageDeviceService>();
        private IArchiverSiteMasterIndexService ArchiverIndexService => PlatformWindsorManager.GetService<IArchiverSiteMasterIndexService>();
        private static readonly ISettingProfilesDao SettingProfileDao = PlatformWindsorManager.GetService<ISettingProfilesDao>();
        private static readonly RMRetryer Retryer = RMRetryerBuilder.CreateBuilder().Build();

        private static string SharedContentContainer = RMGlobalConfiguration.StorageConfig[RMStorageSettingKey.SHARED_STORAGE_CONTAINER_NAME];
        public IIndexDatabaseSynchronizer IndexSynchronizer { get; set; }
        private static readonly IGeneralSettingService GeneralSettingService = PlatformWindsorManager.GetService<IGeneralSettingService>();
        private IRMArchivedFullTextIndexService ArchivedFullTextIndexService => PlatformWindsorManager.GetService<IRMArchivedFullTextIndexService>();
        private string zipFolderPath = string.Empty;
        private const string TEMPFOLDERNAME = "ExportSearchFolder";
        private FileTransferStream FileStream;
        private IMCacheSettingService _CacheSettingService;
        private string BackUpJobId { get; set; }
        public IMCacheSettingService CacheSettingService
        {
            get
            {
                if (_CacheSettingService == null)
                {
                    _CacheSettingService = new CacheSettingService();
                    return _CacheSettingService;
                }
                else
                {
                    return _CacheSettingService;
                }
            }
        }

        public ExportAdvanceSearchResultProcessor(string jobId, JobType mJobType)
        {
            if (jobId.Contains("_"))
            {
                var index = jobId.IndexOf("_");
                JobId = jobId.Substring(0, index);
            }
            tempRestoreFolder = JobId;
            jobContext = JobContext.GetInstance(jobId, mJobType);
            jobContext.ReportManager.StartUpdateJobProgress();
            IndexSynchronizer = new IndexDatabaseSynchronizer();
            zipFolderPath = SecurityUtils.SafeCombinePath(RecordsEnv.AppDomainRootFolder, TEMPFOLDERNAME, JobId);

        }
        public async System.Threading.Tasks.Task RunRestoreCenterExportNowAsync()
        {
            try
            {
                mJobreport = new JobReportImps(jobContext.ReportManager);
                var gls = await GeneralSettingService.GetGeneralSettingAsync();
                ArchiverRestoreResult exportSetting = SerializerHelper.DeserializeByJsonSerializer<ArchiverRestoreResult>(jobContext.JobContextSetting);
                mLog.Info("Start export data by search result");
                exportSetting.PageSize = 1000;
                int pageIndex = 1;
                int fileCounter = 1;
                if (!Directory.Exists(zipFolderPath))
                {
                    Directory.CreateDirectory(zipFolderPath);
                }
                string currentFilePath = GetNewFilePath(zipFolderPath, fileCounter);
                StreamWriter writer = new StreamWriter(currentFilePath);
                

                #region google export
                if (exportSetting.SerchContract?.FilterPolicy?.Level == PolicyLevel.GoogleDriveDocument)
                {
                    WriteCsvHeader(writer);
                    while (true)
                    {
                        List<DataRecord> records = new List<DataRecord>();
                        ArchiverRestoreResult searchResult = await GetDriveSearchTreeResultAsync(exportSetting);
                        mLog.Info($"this search result count is:{searchResult.RestoreSerchNodes?.Count},pageIndex:{pageIndex}");
                        if (searchResult != null && searchResult.RestoreSerchNodes != null && searchResult.RestoreSerchNodes.Count > 0)
                        {
                            foreach (var result in searchResult.RestoreSerchNodes)
                            {
                                DataRecord record = new DataRecord()
                                {
                                    Type = "Google Item",
                                    SourceUrl = result.FullPath,
                                    Size = ConvertUnitUtil.ConvertToKB(JobDetailHelper.GetDataSizeToView(result.ContentLenth)),
                                    CreatedTime = result.CreatedDate,
                                    LastModifiedTime = result.LastModifiedTime,
                                    ArchivedTime = result.ArchivedTime
                                };
                                records.Add(ConvertToCorrectRecord(record));
                            }
                            foreach (var record in records)
                            {
                                FileInfo fileInfo = new FileInfo(currentFilePath);
                                long fileSizeInBytes = fileInfo.Length;
                                if (fileSizeInBytes >= MaxFileSizeInBytes)
                                {
                                    writer.Close();
                                    fileCounter++;
                                    currentFilePath = GetNewFilePath(zipFolderPath, fileCounter);
                                    writer = new StreamWriter(currentFilePath);
                                    WriteCsvHeader(writer);
                                }

                                WriteCsvRow(writer, record);
                            }
                        }
                        else
                        {
                            mLog.Info("not final page and not exist item");
                            writer.Close();
                            await ZipSearchResultFile();
                            break;
                        }                       
                        exportSetting.PageIndex++;
                    }
                }
                #endregion
                else
                {
                    if (exportSetting.SearchMode != (int)SearchMode.NormalSearch)
                    {
                        WriteCsvHeader(writer);
                    }
                    else
                    {
                        WriteCsvHeaderForSharepointNormalSearch(writer);
                    }
                    while (true)
                    {
                        List<DataRecord> records = new List<DataRecord>();
                        if (exportSetting.SearchMode == (int)SearchMode.NormalSearch)
                        {
                            List<ArchiverBasicIndex> searchResult = GetNormalSearchResult(exportSetting, pageIndex);
                            if (searchResult != null && searchResult.Count > 0)
                            {
                                mLog.Info($"this search result count is:{searchResult.Count},pageIndex:{pageIndex}");
                                foreach (var result in searchResult)
                                {
                                    DataRecord record = new DataRecord()
                                    {
                                        Type = "Item",
                                        SourceUrl = GetFullPath(result.ExtraInfo, result.Url),
                                        Size = ConvertUnitUtil.ConvertToKB(JobDetailHelper.GetDataSizeToView(result.ContentLength)),
                                        CreatedTime = GeneralSettingService.ConvertTiksToDateTime(gls, result.CreateTime, true).SimplifyFormatTime,
                                        LastModifiedTime = GeneralSettingService.ConvertTiksToDateTime(gls, result.ModifyTime, true).SimplifyFormatTime,
                                        ArchivedTime = GeneralSettingService.ConvertTiksToDateTime(gls, result.ArchiveTime, true).SimplifyFormatTime,
                                        CreateBy = result.Author,
                                        ModifiedBy = result.Editor,
                                        JobId = result.JobId?.Split('_').FirstOrDefault(),
                                        SoftDelete = result.RetentionStatus == (int)FilterDeletedType.Soft
                                    };
                                    records.Add(ConvertToCorrectRecord(record));
                                }
                                foreach (var record in records)
                                {
                                    FileInfo fileInfo = new FileInfo(currentFilePath);
                                    long fileSizeInBytes = fileInfo.Length;
                                    if (fileSizeInBytes >= MaxFileSizeInBytes)
                                    {
                                        writer.Close();
                                        fileCounter++;
                                        currentFilePath = GetNewFilePath(zipFolderPath, fileCounter);
                                        writer = new StreamWriter(currentFilePath);
                                        WriteCsvHeaderForSharepointNormalSearch(writer);
                                    }

                                    WriteCsvRowForSharepointNormalSearch(writer, record);
                                }
                            }
                            else
                            {
                                mLog.Warn("No search results found after search.");
                                writer.Close();
                                await ZipSearchResultFile();
                                break;
                            }
                        }
                        if (exportSetting.SearchMode == (int)SearchMode.FullTextAdvanceSearch)
                        {
                            ArchiverRestoreResult searchResult = await GetFullTextAdvanceSearchResult(exportSetting, pageIndex);
                            exportSetting.ContinuationToken = searchResult.ContinuationToken;
                            exportSetting.CategoryId = searchResult.CategoryId;
                            if (searchResult != null && searchResult.RestoreSerchNodes != null && searchResult.RestoreSerchNodes.Count > 0)
                            {
                                mLog.Info($"this search result count is:{searchResult.RestoreSerchNodes.Count},pageIndex:{pageIndex}");
                                foreach (var result in searchResult.RestoreSerchNodes)
                                {
                                    DataRecord record = new DataRecord()
                                    {
                                        Type = "Item",
                                        SourceUrl = result.FullPath,
                                        Size = ConvertUnitUtil.ConvertToKB(JobDetailHelper.GetDataSizeToView(result.ContentLenth)),
                                        CreatedTime = result.CreatedDate,
                                        LastModifiedTime = result.LastModifiedTime,
                                        ArchivedTime = result.ArchivedTime
                                    };
                                    records.Add(ConvertToCorrectRecord(record));
                                }
                                foreach (var record in records)
                                {
                                    FileInfo fileInfo = new FileInfo(currentFilePath);
                                    long fileSizeInBytes = fileInfo.Length;
                                    if (fileSizeInBytes >= MaxFileSizeInBytes)
                                    {
                                        writer.Close();
                                        fileCounter++;
                                        currentFilePath = GetNewFilePath(zipFolderPath, fileCounter);
                                        writer = new StreamWriter(currentFilePath);
                                        WriteCsvHeader(writer);
                                    }

                                    WriteCsvRow(writer, record);
                                }
                            }
                            if (exportSetting.ContinuationToken == null || exportSetting.ContinuationToken == "null")
                            {
                                mLog.Warn("No search results found after search.");
                                writer.Close();
                                await ZipSearchResultFile();
                                break;
                            }
                            else
                            {
                                mLog.Info("not final page and not exist item");
                                writer.Close();
                                await ZipSearchResultFile();
                                break;
                            }
                        }
                        if (exportSetting.SearchMode == (int)SearchMode.FullTextSimpleSearch)
                        {
                            var searchResult = await GetFullTextSimpleSearchResult(exportSetting, pageIndex);
                            exportSetting.ContinuationToken = searchResult.ContinuationToken;
                            exportSetting.CategoryId = searchResult.CategoryId;
                            if (searchResult != null && searchResult.RestoreSerchNodes != null && searchResult.RestoreSerchNodes.Count > 0)
                            {
                                mLog.Info($"this search result count is:{searchResult.RestoreSerchNodes.Count},pageIndex:{pageIndex}");
                                foreach (var result in searchResult.RestoreSerchNodes)
                                {
                                    DataRecord record = new DataRecord()
                                    {
                                        Type = "Item",
                                        SourceUrl = result.FullPath,
                                        CreatedTime = result.CreatedDate,
                                        LastModifiedTime = result.LastModifiedTime,
                                        Size = ConvertUnitUtil.ConvertToKB(JobDetailHelper.GetDataSizeToView(result.ContentLenth)),
                                        ArchivedTime = result.ArchivedTime
                                    };
                                    records.Add(ConvertToCorrectRecord(record));
                                }
                                foreach (var record in records)
                                {
                                    FileInfo fileInfo = new FileInfo(currentFilePath);
                                    long fileSizeInBytes = fileInfo.Length;
                                    if (fileSizeInBytes >= MaxFileSizeInBytes)
                                    {
                                        writer.Close();
                                        fileCounter++;
                                        currentFilePath = GetNewFilePath(zipFolderPath, fileCounter);
                                        writer = new StreamWriter(currentFilePath);
                                        WriteCsvHeader(writer);
                                    }

                                    WriteCsvRow(writer, record);
                                }
                            }
                            if (exportSetting.ContinuationToken == null || exportSetting.ContinuationToken == "null")
                            {
                                mLog.Warn("No search results found after search.");
                                writer.Close();
                                await ZipSearchResultFile();
                                break;
                            }
                            else
                            {
                                mLog.Info("not final page and not exist item");
                                writer.Close();
                                await ZipSearchResultFile();
                                break;
                            }
                        }
                        pageIndex++;
                    }
                }
                
            }
            catch(Exception e)
            {
                mJobreport.HasErrorNode = true;
                mLog.Error($"someting went wrong when export search result.{e}");
            }
            finally
            {
                mJobreport.FinishRestoreReport();
            }
        }

        public async Task<ArchiverRestoreResult> GetDriveSearchTreeResultAsync(ArchiverRestoreResult searchContract)
        {
            this.BackUpJobId = searchContract.SerchContract.BackupJobId;
            ArchiverRestoreResult re = new ArchiverRestoreResult();
            SiteCollectionNodesInfo node = searchContract.SerchContract.SearchNode;
            try
            {           
                mLog.Info($"Do google drive archiver restore search, search node:{node.SiteUrl}.SiteGroupId:{node.SiteGroupId}.SPObjectId:{node.SPObjectId}");
                re = await HandleGDriveSearchCommonNodeAsync(searchContract, node,
                    new ArchiverRestoreOrderBy
                    {
                        ColName = searchContract.OrderBy,
                        Order = searchContract.IsDesc ? DocAveOnline.WebApi.Contracts.Order.Desc : DocAveOnline.WebApi.Contracts.Order.Asc
                    });
                if (re.SerchContract?.FilterPolicy?.DataSource != null)
                {
                    re.SerchContract.FilterPolicy.DataSource = (int)RestoreDataSource.GoogleDrive;
                }            
            }
            catch (AveException ex)
            {
                mLog.Error("Get restore failed:", ex.ToString());
                throw;
            }
            catch (OpenIndexDbTimeoutException ex)
            {
                re.Failed = true;
                re.Message = "WaitDownloadIndexDb";
                mLog.Error(ex.Message);
            }
            catch (Exception ex)
            {
                mLog.Error("Error occured while Archiver Restore searching:", ex.ToString());
            }
            return re;
        }

        private async Task<ArchiverRestoreResult> HandleGDriveSearchCommonNodeAsync(ArchiverRestoreResult filterPolicy, SiteCollectionNodesInfo searchNode, ArchiverRestoreOrderBy orderBy)
        {
            ArchiverRestoreResult res = new ArchiverRestoreResult();
            List<ArchiverRestoreSerchResult> result = new List<ArchiverRestoreSerchResult>();
            ArchiverSiteMasterIndexContract siteIndex = GetGoogleDriveIndex(searchNode);
            if (null == siteIndex)
            {
                mLog.Warn("the siteIndex is null");
                return null;
            }
            List<ArchiverSiteMasterIndexContract> indexes = new List<ArchiverSiteMasterIndexContract> { siteIndex };
            filterPolicy.SerchContract.FilterPolicy.PageIndex = filterPolicy.PageIndex;
            filterPolicy.SerchContract.FilterPolicy.PageSize = filterPolicy.PageSize;
            mLog.Info($"HandleSearchCommonNode.FilterPolicy PageIndex:{filterPolicy.PageIndex}.FilterPolicy PageSize:{filterPolicy.PageSize}.OpenIndexDbTimeoutInMs:{filterPolicy.OpenIndexDbTimeoutInMs}.");
            List<TreeNode> trees = GetGDriveSearchNodesFromMedia(indexes, new List<SiteCollectionNodesInfo> { searchNode }, filterPolicy.SerchContract.FilterPolicy, filterPolicy.OpenIndexDbTimeoutInMs, orderBy);
            if (trees == null || trees.Count == 0)
            {
                mLog.Warn("HandleSearchCommonNode.List <TreeNode> trees is null");
            }
            var tempTree = trees.FirstOrDefault();
            res.TotalNumber = tempTree == null ? 0 : tempTree.Count;
            var gls = await GeneralSettingService.GetGeneralSettingAsync();
            if (null != trees && trees.Count > 0)
            {
                foreach (var re in trees)
                {
                    var temp = ConvertToSerchResult(re, filterPolicy.SerchContract.FilterPolicy, gls);
                    if (!result.Contains(temp))
                    {
                        result.Add(temp);
                    }
                }
            }
            else
            {
                if (filterPolicy.SerchContract.FilterPolicy.Level > PolicyLevel.SiteCollection)
                {
                    mLog.Warn("serch tree is null");
                    return res;
                }
            }
            res.RestoreSerchNodes = result;
            return res;
        }

        private ArchiverRestoreSerchResult ConvertToSerchResult(TreeNode index, ArchiverRestoreFilter filterPolicy, Contract.RMWeb.CP.GeneralSettingModel gls)
        {
            ArchiverRestoreSerchResult result = new ArchiverRestoreSerchResult();
            result.TreeNode = SerializerHelper.SerializeByDataContractSerializer(index);
            while (true)
            {
                if (index.Children.Count > 0)
                {
                    if (filterPolicy.Level != PolicyLevel.Document && filterPolicy.Level != PolicyLevel.Attachment && filterPolicy.Level.ToString() == index.TreeNodeLevel.ToString())
                    {
                        if (filterPolicy.Level == PolicyLevel.Folder || filterPolicy.Level == PolicyLevel.Site)
                        {
                            index = index.Children[0];
                        }
                    }
                    else
                    {
                        index = index.Children[0];
                    }
                }
                else
                {
                    break;
                }
            }
            if (index.TreeNodeLevel != TreeNodeLevel.Item)
            {
                if (index.TreeNodeLevel == TreeNodeLevel.Site)
                {
                    result.ObjectName = index.Title;
                }
                else
                {
                    result.ObjectName = index.Name;
                }
            }
            else
            {
                if (index.TypeInIndex == "I")
                {
                    string temp = index.Description.Substring("Title:".Length, index.Description.IndexOf(Environment.NewLine) - "Title:".Length);
                    result.ObjectName = index.Name.IndexOf(":") < 0 ? index.Name + $"({temp})" : index.Name.Insert(index.Name.IndexOf(":"), $"({temp})");
                }
                else if (index.TypeInIndex == "A")
                {
                    result.ObjectName = index.Name;
                }
                else
                {
                    result.ObjectName = index.Name;
                }
            }
            result.Location = index.FullPath;
            result.FullPath = index.FullPath;
            result.ParentPathMd5 = index.ParentPathMD5;
            result.PathMd5 = index.PathMD5;
            result.ModifiedBy = index.ModifiedBy;
            result.SitePath = index.SitePath;
            result.IsArchiveTier = index.IsArchiveTier;
            result.ModifiedTime = index.ModifiedTime;
            result.ArchiveTime = index.ArchivedTime;
            result.ContentLenth = index.ContentLenth;
            result.IsSoftDeleted = index.IsSoftDeleted;
            result.IsVersion = !string.IsNullOrEmpty(index.Name) && index.Name.Contains(":");
            if (index.ModifiedTime > 0)
            {
                result.LastModifiedTime = GeneralSettingService.ConvertTiksToDateTime(gls, index.ModifiedTime, true).SimplifyFormatTime;
            }
            else
            {
                result.LastModifiedTime = string.Empty;
            }
            if (index.ArchivedTime > 0)
            {
                result.ArchivedTime = GeneralSettingService.ConvertTiksToDateTime(gls, index.ArchivedTime, true).SimplifyFormatTime;
            }
            else
            {
                result.ArchivedTime = string.Empty;
            }
            if (index.CreatedTime > 0)
            {
                result.CreatedDate = GeneralSettingService.ConvertTiksToDateTime(gls, index.CreatedTime, true).SimplifyFormatTime;
                result.CreatedDateTicks = index.CreatedTime.ToString();
            }
            else
            {
                result.CreatedDate = string.Empty;
            }
            return result;
        }

        public List<TreeNode> GetGDriveSearchNodesFromMedia(List<ArchiverSiteMasterIndexContract> indexes, List<SiteCollectionNodesInfo> searchNodes, ArchiverRestoreFilter filterPolicy, int openIndexTimeoutInMs, ArchiverRestoreOrderBy orderBy)
        {
            var sitesMap = AssembleGDriveSearchParamInfo(indexes, searchNodes);
            var advancedSearchInfo = ConvertToGDriveArchiverAdvancedInfo(sitesMap, filterPolicy);
            advancedSearchInfo.OpenIndexDbTimeoutInMs = openIndexTimeoutInMs;
            var advancedSearchService = new ArchiverAdvancedSearchService();
            var searchResult = advancedSearchService.SearchForGoogle(advancedSearchInfo, orderBy);
            return searchResult;
        }

        private GDriveArchiverAdvancedSearchInfo ConvertToGDriveArchiverAdvancedInfo(List<ArchiverRestoreSearchContractDto> searchContract, ArchiverRestoreFilter filterPolicy)
        {
            GDriveArchiverAdvancedSearchInfo searchInfo = new GDriveArchiverAdvancedSearchInfo()
            {
                NodeInfos = new List<GDriveArchiverSearchNodeInfo>(),
                FilterInfors = new ArchiverRestoreFilter(),
            };
            searchContract.ForEach(node =>
            {
                searchInfo.NodeInfos.Add(new GDriveArchiverSearchNodeInfo()
                {
                    BrowseInfo = new GDriveBrowseInfo(node.SearchParam as GDriveRestoreParamDto, ProductModule.GDriveArchiverBackup),
                    SiteId = node.SearchNode.SPObjectId,
                });
            });
            searchInfo.FilterInfors = filterPolicy;
            mLog.Info($"ConverToArchiverAdvancedInfo.searchInfo.NodeInfos count:{searchInfo.NodeInfos.Count}." +
                $"searchInfo.FilterInfors.PolicyLevel:{filterPolicy.Level}." +
                $"FilterName:{filterPolicy.FilterName}." +
                $"CreateStartTime:{filterPolicy.CreateStartTime}." +
                $"CreateEndTime:{filterPolicy.CreateEndTime}." +
                $"ModifiedStartTime:{filterPolicy.ModifiedStartTime}." +
                $"ModifiedEndTime:{filterPolicy.ModifiedEndTime}." +
                $"MainJobId:{filterPolicy.MainJobId}");
            return searchInfo;
        }

        private List<ArchiverRestoreSearchContractDto> AssembleGDriveSearchParamInfo(List<ArchiverSiteMasterIndexContract> indexes, List<SiteCollectionNodesInfo> searchNodes)
        {
            mLog.Info($"AssembleGoogleSearchParamInfo.indexes count:{indexes.Count}.searchNodes count:{searchNodes.Count}.");
            List<ArchiverRestoreSearchContractDto> sitesMap = new List<ArchiverRestoreSearchContractDto>();
            ArchiverSiteMasterIndexContract currentIndex = null;
            foreach (var node in searchNodes)
            {
                string siteURL = node.SiteUrl;
                currentIndex = indexes.Where<ArchiverSiteMasterIndexContract>(s => s.SiteURL.Equals(siteURL, StringComparison.OrdinalIgnoreCase) && s.SiteId.Equals(node.SPObjectId, StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
                if (currentIndex == null)
                {
                    mLog.Warn($"AssembleGoogleSearchParamInfo.currentIndex is null.SiteUrl:{siteURL}.");
                    continue;
                }
                else
                {
                    mLog.Warn($"AssembleGoogleSearchParamInfo.Successs add ArchiverRestoreSearchContractDto.SiteUrl:{siteURL}.");
                }
                ArchiverRestoreSearchContractDto paramDto = new ArchiverRestoreSearchContractDto();
                paramDto.SearchNode = node;
                paramDto.SearchParam = AssembleGDriveRestoreParamDto(currentIndex, node);
                paramDto.SearchParam.CacheLocation = CacheSettingService.GetBrowserCacheInfo();
                sitesMap.Add(paramDto);
            }
            mLog.Info($"Finished AssembleGoogleSearchParamInfo.sitesMap count:{sitesMap.Count}.");
            return sitesMap;
        }

        private ArchiverRestoreParamDto AssembleGDriveRestoreParamDto(ArchiverSiteMasterIndexContract index, SiteCollectionNodesInfo searchNode)
        {
            StorageDeviceDto Indexdevice = null;
            SettingProfileDto indexDto = new SettingProfileDto()
            {
                Type = (int)SettingProfilesType.IndexDevice,
                Name = "UsingIndexDevice"
            };
            var indexDBInfo = SettingProfileDao.Load(indexDto);
            if (indexDBInfo != null)
            {
                Indexdevice = StorageDeviceService.GetStorageDeviceById(indexDBInfo.Settings, needDecryptSecert: true);
            }
            if (this.BackUpJobId != null && !this.BackUpJobId.Contains('_'))
            {
                mLog.Warn($"this stub may rebuild stub,will use main index,job id is:{this.BackUpJobId},index job id:{index.JobId}");
                this.BackUpJobId = string.Empty;
            }
            var param = new GDriveRestoreParamDto
            {
                Path = searchNode.SiteUrl,
                BackupJobId = string.IsNullOrEmpty(this.BackUpJobId) ? index.JobId : this.BackUpJobId,
                FarmName = string.Empty,
                BackupPlanId = index.PlanId,
                EndTime = DateTime.MaxValue.Ticks,
                IndexLogicalDevice = Indexdevice,
                LoadTreeOption = string.IsNullOrEmpty(this.BackUpJobId) ? ArchiverLoadTreeOption.SiteCollectionMode : ArchiverLoadTreeOption.JobMode,
                StorageInfo = index.StorageInfo,
                SiteUrl = searchNode.SiteUrl,
                DriveId = searchNode.SPObjectId,
                TenantId = searchNode.SiteGroupId,
            };
            return param;
        }

        private ArchiverSiteMasterIndexContract GetGoogleDriveIndex(SiteCollectionNodesInfo node)
        {
            ArchiverSiteMasterIndexContract siteIndex = new ArchiverSiteMasterIndexContract { SiteId = node.SPObjectId, SiteURL = node.SiteUrl };
            return ArchiverIndexService.GetGoogleDriveInfo(siteIndex);
        }

        private DataRecord ConvertToCorrectRecord(DataRecord tempRecord)
        {
            DataRecord result = new DataRecord();
            result.Type = tempRecord.Type;
            result.SourceUrl = ConvertStringToCsvValue(tempRecord.SourceUrl);
            result.Size = tempRecord.Size;
            result.CreatedTime = ConvertStringToCsvValue(tempRecord.CreatedTime);
            result.LastModifiedTime = ConvertStringToCsvValue(tempRecord.LastModifiedTime);
            result.ArchivedTime = ConvertStringToCsvValue(tempRecord.ArchivedTime);
            result.Name = ConvertStringToCsvValue(tempRecord.Name);
            result.ModifiedBy = ConvertStringToCsvValue(tempRecord.ModifiedBy);
            result.CreateBy = ConvertStringToCsvValue(tempRecord.CreateBy);
            result.JobId = tempRecord.JobId;
            result.SoftDelete = tempRecord.SoftDelete;
            return result;
        }

        private string ConvertStringToCsvValue(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return value;
            }

            if (value.Contains(","))
            {
                if (value.Contains("\""))
                {
                    value = value.Replace("\"", "\"\"");
                }
                value = "\"" + value + "\"";
            }
            return value;
        }

        public async System.Threading.Tasks.Task RunNowAsync()
        {
            mJobreport = new JobReportImps(jobContext.ReportManager);
            try
            {
                var gls = await GeneralSettingService.GetGeneralSettingAsync();
                DocAveOnline.WebApi.Contracts.EndUserRestoreConfig ExportSetting = SerializerHelper.DeserializeByDataContractSerializer<DocAveOnline.WebApi.Contracts.EndUserRestoreConfig>(jobContext.JobContextSetting);
                GenerateZipPassword();
                if (ExportSetting.IsExportAllSearchResult)
                {
                    mLog.Info("Start export data by search result");
                    int pageIndex = 1;
                    int fileCounter = 1;
                    if (!Directory.Exists(zipFolderPath))
                    {
                        Directory.CreateDirectory(zipFolderPath);
                    }
                    string currentFilePath = GetNewFilePath(zipFolderPath, fileCounter);
                    StreamWriter writer = new StreamWriter(currentFilePath);
                    WriteCsvHeaderForEndUser(writer);
                    while (true)
                    {
                        List<DataRecord> records = new List<DataRecord>();
                        List<ArchiverBasicIndex> searchResult = GetSearchResult(ExportSetting.SearchJobInfo.SiteUrl, ExportSetting.searchCondition, pageIndex);
                        
                        if (searchResult != null && searchResult.Count > 0)
                        {
                            mLog.Info($"this search result count is:{searchResult.Count},pageIndex:{pageIndex}");
                            foreach (var result in searchResult)
                            {
                                DataRecord record = new DataRecord()
                                {
                                    Type = "Item",
                                    SourceUrl = GetFullPath(result.ExtraInfo, result.Url),
                                    Size = ConvertUnitUtil.ConvertToKB(JobDetailHelper.GetDataSizeToView(result.ContentLength)),
                                    CreatedTime = GeneralSettingService.ConvertTiksToDateTime(gls, result.CreateTime, true).SimplifyFormatTime,
                                    LastModifiedTime = GeneralSettingService.ConvertTiksToDateTime(gls, result.ModifyTime, true).SimplifyFormatTime,
                                    ArchivedTime = GeneralSettingService.ConvertTiksToDateTime(gls, result.ArchiveTime, true).SimplifyFormatTime,
                                    Name = result.Name,
                                    ModifiedBy = result.Editor
                                };
                                records.Add(ConvertToCorrectRecord(record));
                            }

                            foreach (var record in records)
                            {
                                FileInfo fileInfo = new FileInfo(currentFilePath);
                                long fileSizeInBytes = fileInfo.Length;
                                if (fileSizeInBytes >= MaxFileSizeInBytes)
                                {
                                    writer.Close();
                                    fileCounter++;
                                    currentFilePath = GetNewFilePath(zipFolderPath, fileCounter);
                                    writer = new StreamWriter(currentFilePath);
                                    WriteCsvHeaderForEndUser(writer);
                                }

                                WriteCsvRowForEndUser(writer, record);
                            }
                        }
                        else
                        {
                            mLog.Warn("No search results found after search.");
                            writer.Close();
                            ZipFileWithPasswordAndUpload(zipFolderPath);
                            break;
                        }
                        pageIndex++;
                    }
                }
                else
                {
                    if (ExportSetting.SearchJobInfo?.AdvanceSearchResults != null && ExportSetting.SearchJobInfo.AdvanceSearchResults.Count > 0)
                    {
                        mLog.Info("Start export data by select search result");
                        List<DataRecord> records = new List<DataRecord>();
                        foreach (var result in ExportSetting.SearchJobInfo.AdvanceSearchResults)
                        {
                            DataRecord record = new DataRecord()
                            {
                                Type = "Item",
                                SourceUrl = result.FullPath,
                                Size = ConvertUnitUtil.ConvertToKB(JobDetailHelper.GetDataSizeToView(result.ContentLenth)),
                                CreatedTime = GeneralSettingService.ConvertTiksToDateTime(gls, result.CreateTime, true).SimplifyFormatTime,
                                LastModifiedTime = GeneralSettingService.ConvertTiksToDateTime(gls, result.ModifiedTime, true).SimplifyFormatTime,
                                ArchivedTime = GeneralSettingService.ConvertTiksToDateTime(gls, result.ArchiveTime, true).SimplifyFormatTime,
                                Name = result.Name,
                                ModifiedBy = result.ModifiedBy,
                            };
                            records.Add(ConvertToCorrectRecord(record));
                        }
                        CreateCsvAndZip(records, zipFolderPath);
                    }
                    else
                    {
                        mLog.Warn("No search results found.");
                    }
                }
            }
            catch (Exception e)
            {
                mJobreport.HasErrorNode = true;
                mLog.Error(@"Looks up a localized string similar to An error occurred while doing the restore job.{0}", e);
            }
            finally
            {
                mJobreport.FinishRestoreReport();
            }
        }
        private string GetFullPath(string extraInfo, string url)
        {
            var document = new XmlDocument();
            document.LoadXml(extraInfo);
            var apUrlElements = document.GetElementsByTagName("HeaderExtraAttribute");
            if (apUrlElements != null && apUrlElements.Count > 0)
            {
                var apUrl = apUrlElements[0]?.Attributes["APUrl"]?.Value ?? url;
                return apUrl.Contains("\\") ? apUrl?.Replace("\\", "/") : apUrl;
            }
            return url;
        }
        private void GenerateZipPassword()
        {
            zipPassword = GeneratePassword(13, true, false, true, true);
            var encryptPassword = AesEncryptorWrapper.Encrypt(zipPassword);
            DownloadDataInfoDao.CreateZipPasswordInfo(new RA.DB.Model.RMDownloadDataInfo() { Name = encryptPassword, JobId = JobId, FileDownloadTime = DateTime.UtcNow.Ticks, DownloadType = DownloadContentType.ZipPasswordInfo });
        }
        private List<ArchiverBasicIndex> GetSearchResult(string siteUrl, DocAveOnline.WebApi.Contracts.AdvanceSearchCondition searchCondition,int pageIndex)
        {
            var siteIndex = ArchiverSiteMasterIndexService.GetAllSiteCollectionNodsInfoByUrl(siteUrl).FirstOrDefault();
            if (siteIndex == null)
            {
                mLog.Warn($"Advance search scope haven't archived history. scope: {searchCondition.Scope}");
                return null;
            }
            List<ArchiverRestoreSerchResult> result = new List<ArchiverRestoreSerchResult>();
            var searchResult = new ArchiverRestoreResult()
            {
                PageSize = 1000,
                PageIndex = pageIndex,
                SerchContract = new BackupDataSearchContract()
                {
                    SearchNode = new SiteCollectionNodesInfo() { SiteUrl = siteUrl, SiteGroupId = siteIndex.SiteGroupId, SPObjectId = siteIndex.SiteId },
                    FilterPolicy = new CommonFilter.ArchiverRestoreFilter()
                    {
                        FilterName = searchCondition.Keyword,
                        Level = AvePoint.GCommon.Contract.CommonFilter.PolicyLevel.DocumentVersion,
                        CreateStartTime = searchCondition.CreatedDateFrom == 0 ? string.Empty : searchCondition.CreatedDateFrom.ToString(),
                        CreateEndTime = searchCondition.CreatedDateTo == 0 ? string.Empty : searchCondition.CreatedDateTo.ToString(),
                        FolderName = searchCondition.FolderNameOrPath,
                        ModifiedBy = searchCondition.ModifiedBy,
                        CreatedBy = searchCondition.CreatedBy
                    }
                }
            };

            searchResult.SerchContract.FilterPolicy.PageIndex = searchResult.PageIndex;
            searchResult.SerchContract.FilterPolicy.PageSize = searchResult.PageSize;
            var sitesMap = AssembleSearchParamInfo(searchResult.SerchContract.SearchNode);
            var advancedSearchInfo = ConverToArchiverAdvancedInfo(sitesMap, searchResult.SerchContract.FilterPolicy);
            var advancedSearchService = new ArchiverAdvancedSearchService();
            List<ArchiverBasicIndex> exportSearchResult = advancedSearchService.SearchForExport(advancedSearchInfo);
            return exportSearchResult;
        }
        private async Task<ArchiverRestoreResult> GetFullTextSimpleSearchResult(ArchiverRestoreResult ExportSetting, int pageIndex)
        {
            var tempInfo = ExportSetting.archiverRestoreSimpleSearchQueryParameter;
            tempInfo.PageSize = 1000;
            //tempInfo.in = pageIndex;
            var querier = await ArchivedFullTextIndexService.GetEDiscoverySimpleSearchResult(tempInfo);
            return querier;
        }
        private async Task<ArchiverRestoreResult> GetFullTextAdvanceSearchResult(ArchiverRestoreResult ExportSetting, int pageIndex)
        {
            ExportSetting.PageIndex = pageIndex;
            var querier =await ArchivedFullTextIndexService.GetSearchResultByFilter(ExportSetting);
            return querier;
        }
        private List<ArchiverBasicIndex> GetNormalSearchResult(ArchiverRestoreResult ExportSetting, int pageIndex)
        {
            string siteUrl = ExportSetting?.SerchContract?.SearchNode?.SiteUrl;
            mLog.Info($"export search result url is:{siteUrl}");
            var siteIndex = ArchiverSiteMasterIndexService.GetAllSiteCollectionNodsInfoByUrl(siteUrl).FirstOrDefault();
            if (siteIndex == null)
            {
                mLog.Warn($"Advance search scope haven't archived history. scope: {siteUrl}");
                return null;
            }
            List<ArchiverRestoreSerchResult> result = new List<ArchiverRestoreSerchResult>();
            var searchResult = new ArchiverRestoreResult()
            {
                PageSize = 1000,
                PageIndex = pageIndex,
                SerchContract = ExportSetting.SerchContract
            };

            searchResult.SerchContract.FilterPolicy.PageIndex = searchResult.PageIndex;
            searchResult.SerchContract.FilterPolicy.PageSize = searchResult.PageSize;
            var sitesMap = AssembleSearchParamInfo(searchResult.SerchContract.SearchNode);
            var advancedSearchInfo = ConverToArchiverAdvancedInfo(sitesMap, searchResult.SerchContract.FilterPolicy);
            var advancedSearchService = new ArchiverAdvancedSearchService();
            List<ArchiverBasicIndex> exportSearchResult = advancedSearchService.SearchForExport(advancedSearchInfo);
            return exportSearchResult;
        }
        private ArchiverAdvancedSearchInfo ConverToArchiverAdvancedInfo(List<ArchiverRestoreSearchContractDto> searchContract, ArchiverRestoreFilter filterPolicy)
        {
            ArchiverAdvancedSearchInfo searchInfo = new ArchiverAdvancedSearchInfo()
            {
                NodeInfos = new List<ArchiverSearchNodeInfo>(),
                FilterInfors = new ArchiverRestoreFilter(),
            };
            searchContract.ForEach(node =>
            {
                searchInfo.NodeInfos.Add(new ArchiverSearchNodeInfo()
                {
                    BrowseInfo = new ArchiverBrowseInfo(node.SearchParam),
                    SiteId = node.SearchNode.SPObjectId,
                });
            });
            searchInfo.FilterInfors = filterPolicy;
            return searchInfo;
        }
        private List<ArchiverRestoreSearchContractDto> AssembleSearchParamInfo(SiteCollectionNodesInfo searchNodes)
        {
            List<ArchiverRestoreSearchContractDto> sitesMap = new List<ArchiverRestoreSearchContractDto>();

            string siteURL = searchNodes.SiteUrl;

            ArchiverRestoreSearchContractDto paramDto = new ArchiverRestoreSearchContractDto();
            paramDto.SearchNode = searchNodes;
            paramDto.SearchParam = AssembleExportParamDto(siteURL);
            paramDto.SearchParam.CacheLocation = CacheSettingService.GetBrowserCacheInfo();
            sitesMap.Add(paramDto);

            return sitesMap;
        }
        private ArchiverRestoreParamDto AssembleExportParamDto(string siteUrl)
        {
            StorageDeviceDto Indexdevice = null;
            SettingProfileDto indexDto = new SettingProfileDto()
            {
                Type = (int)SettingProfilesType.IndexDevice,
                Name = "UsingIndexDevice"
            };
            var indexDBInfo = SettingProfileDao.Load(indexDto);
            if (indexDBInfo != null)
            {
                Indexdevice = StorageDeviceService.GetStorageDeviceById(indexDBInfo.Settings, needDecryptSecert: true);
            }
            ArchiverRestoreParamDto param = new ArchiverRestoreParamDto
            {
                Path = siteUrl,
                //Level = searchNode.Level,
                //BackupJobId = index.JobId,
                FarmName = string.Empty,
                //BackupPlanId = index.PlanId,
                EndTime = DateTime.MaxValue.Ticks,
                //LogicalDevice = SOUtilityService.GetLogicalDeviceInfo(index.LogicalDeviceId),
                IndexLogicalDevice = Indexdevice,
                LoadTreeOption = ArchiverLoadTreeOption.SiteCollectionMode,
                //StorageInfo = index.StorageInfo,
                SiteUrl = siteUrl
            };
            param.CacheLocation = CacheSettingService.GetBrowserCacheInfo();
            return param;
        }

        private string GeneratePassword(int intLength, bool booNumber, bool booSign, bool booSmallword, bool booBigword)
        {
            //定义
            int intResultRound = 0;
            string strB = "";
            while (intResultRound < intLength)
            {
                //生成随机数A，表示生成类型
                //1=数字，2=符号，3=小写字母，4=大写字母
                int intA = SecurityUtils.GetRandomNumber(1, 5);
                //如果随机数A=1，则运行生成数字
                //生成随机数A，范围在0-10
                //把随机数A，转成字符
                //生成完，位数+1，字符串累加，结束本次循环
                if (intA == 1 && booNumber)
                {
                    intA = SecurityUtils.GetRandomNumber(0, 10);
                    strB = intA.ToString(CultureInfo.InvariantCulture) + strB;
                    intResultRound = intResultRound + 1;
                    continue;
                }
                //如果随机数A=2，则运行生成符号
                //生成随机数A，表示生成值域
                //1：33-47值域，2：58-64值域，3：91-96值域，4：123-126值域
                if (intA == 2 && booSign)
                {
                    intA = SecurityUtils.GetRandomNumber(1, 5);

                    //如果A=1
                    //生成随机数A，33-47的Ascii码
                    //把随机数A，转成字符
                    //生成完，位数+1，字符串累加，结束本次循环
                    if (intA == 1)
                    {
                        intA = SecurityUtils.GetRandomNumber(33, 48);
                        strB = ((char)intA).ToString(CultureInfo.InvariantCulture) + strB;
                        intResultRound = intResultRound + 1;
                        continue;
                    }

                    //如果A=2
                    //生成随机数A，58-64的Ascii码
                    //把随机数A，转成字符
                    //生成完，位数+1，字符串累加，结束本次循环
                    if (intA == 2)
                    {
                        intA = SecurityUtils.GetRandomNumber(58, 65);
                        strB = ((char)intA).ToString(CultureInfo.InvariantCulture) + strB;
                        intResultRound = intResultRound + 1;
                        continue;
                    }

                    //如果A=3
                    //生成随机数A，91-96的Ascii码
                    //把随机数A，转成字符
                    //生成完，位数+1，字符串累加，结束本次循环
                    if (intA == 3)
                    {
                        intA = SecurityUtils.GetRandomNumber(91, 97);
                        strB = ((char)intA).ToString(CultureInfo.InvariantCulture) + strB;
                        intResultRound = intResultRound + 1;
                        continue;
                    }

                    //如果A=4
                    //生成随机数A，123-126的Ascii码
                    //把随机数A，转成字符
                    //生成完，位数+1，字符串累加，结束本次循环
                    if (intA == 4)
                    {
                        intA = SecurityUtils.GetRandomNumber(123, 127);
                        strB = ((char)intA).ToString(CultureInfo.InvariantCulture) + strB;
                        intResultRound = intResultRound + 1;
                        continue;
                    }
                }
                //如果随机数A=3，则运行生成小写字母
                //生成随机数A，范围在97-122
                //把随机数A，转成字符
                //生成完，位数+1，字符串累加，结束本次循环
                if (intA == 3 && booSmallword)
                {
                    intA = SecurityUtils.GetRandomNumber(97, 123);
                    strB = ((char)intA).ToString(CultureInfo.InvariantCulture) + strB;
                    intResultRound = intResultRound + 1;
                    continue;
                }

                //如果随机数A=4，则运行生成大写字母
                //生成随机数A，范围在65-90
                //把随机数A，转成字符
                //生成完，位数+1，字符串累加，结束本次循环
                if (intA == 4 && booBigword)
                {
                    intA = SecurityUtils.GetRandomNumber(65, 89);
                    strB = ((char)intA).ToString(CultureInfo.InvariantCulture) + strB;
                    intResultRound = intResultRound + 1;
                }
            }
            return strB;
        }


        public void CreateCsvAndZip(List<DataRecord> records, string outputDirectory)
        {
            int fileCounter = 1;
            if (!Directory.Exists(outputDirectory))
            {
                Directory.CreateDirectory(outputDirectory);
            }
            string currentFilePath = GetNewFilePath(outputDirectory, fileCounter);
            StreamWriter writer = new StreamWriter(currentFilePath);
            WriteCsvHeaderForEndUser(writer);

            foreach (var record in records)
            {
                FileInfo fileInfo = new FileInfo(currentFilePath);
                long fileSizeInBytes = fileInfo.Length;
                if (fileSizeInBytes >= MaxFileSizeInBytes)
                {
                    writer.Close();
                    fileCounter++;
                    currentFilePath = GetNewFilePath(outputDirectory, fileCounter);
                    writer = new StreamWriter(currentFilePath);
                    WriteCsvHeaderForEndUser(writer);
                }

                WriteCsvRowForEndUser(writer, record);
            }

            writer.Close();
            ZipFileWithPasswordAndUpload(outputDirectory);
        }
        
        private string GetNewFilePath(string outputDirectory, int fileCounter)
        {
            return SecurityUtils.SafeCombinePath(outputDirectory, $"{JobId}_{fileCounter}.csv");
        }

        private void WriteCsvHeader(StreamWriter writer)
        {
            writer.WriteLine("Type,Source URL,Size (KB),Created time,Last modified time,Archived time");
        }

        private void WriteCsvHeaderForSharepointNormalSearch(StreamWriter writer)
        {
            writer.WriteLine("Type,Source URL,Size (KB),Created time,Last modified time,Archived time,Create by,Modify by,Job ID,Soft deleted");
        }
        private void WriteCsvRowForSharepointNormalSearch(StreamWriter writer, DataRecord record)
        {
            writer.WriteLine($"{record.Type},{record.SourceUrl},{record.Size},{record.CreatedTime},{record.LastModifiedTime},{record.ArchivedTime},{record.CreateBy},{record.ModifiedBy},{record.JobId},{(record.SoftDelete ? "Yes" : "No")}");
        }

        private void WriteCsvRow(StreamWriter writer, DataRecord record)
        {
            writer.WriteLine($"{record.Type},{record.SourceUrl},{record.Size},{record.CreatedTime},{record.LastModifiedTime},{record.ArchivedTime}");
        }


        private void WriteCsvHeaderForEndUser(StreamWriter writer)
        {
            writer.WriteLine("Type,File name,Source URL,Size (KB),Created time,Last modified time,Modified by,Archived time");
        }

        private void WriteCsvRowForEndUser(StreamWriter writer, DataRecord record)
        {
            writer.WriteLine($"{record.Type},{record.Name},{record.SourceUrl},{record.Size},{record.CreatedTime},{record.LastModifiedTime},{record.ModifiedBy},{record.ArchivedTime}");
        }


        private string AssembleExportStorageXriString()
        {
            string containerName = RMGlobalConfiguration.StorageConfig[RMStorageSettingKey.SHARED_STORAGE_CONTAINER_NAME];
            var tempConn = CommonUtilityForSpecialTenant.GetStorageConnectionStringFromConfigFile(StorageStringType.SharedStorage);
            return RA.Common.Util.AzureUtil.GetConnectionBuilderString(tempConn, containerName);
        }
        private void ZipFileWithPasswordAndUpload(string filePath)
        {
            string zipFileName = JobId + ".zip";
            string zipFolderPath = SecurityUtils.SafeCombinePath(RecordsEnv.AppDomainRootFolder, TEMPFOLDERNAME, JobId);
            string zipFilePath = SecurityUtils.SafeCombinePath(RecordsEnv.AppDomainRootFolder, TEMPFOLDERNAME, JobId, zipFileName);

            try
            {
                ZipUtil.ZipFolder(zipFolderPath, zipFilePath, zipPassword, Encoding.UTF8);
            }
            catch (Exception e)
            {
                mLog.Warn($"zip the directory {zipFolderPath} failed, maybe the path is too long, try to zip with alphaFS. {e.ToString()}");
                ZipUtil.ZipFolderForLongPath(zipFolderPath, zipFilePath, zipPassword, Encoding.UTF8);
            }
            try
            {

                var connectionString = AssembleExportStorageXriString();
                StorageDeviceDto storage = new StorageDeviceDto();
                storage.ConnectionString = connectionString;
                storage.Type = (int)StorageDeviceType.CloudAzure;
                var physical = new PhysicalDeviceDto()
                {
                    Id = storage.Id,
                    ConnectionString = storage.ConnectionString,
                    ModifyTime = storage.ModifyTime,
                    Type = storage.Type,
                };
                var storageInfo = new StorageInfo { HighName = "ArchivedExportContent" + "\\" + TenantLocalValue.LogonGroupId + "\\" + this.tempRestoreFolder, LowName = zipFileName };
                StorageInfo tempStorageInfo = new StorageInfo { HighName = string.Empty, LowName = zipFilePath };
                using (var cacheStream = File.Open(zipFilePath, FileMode.Open))
                {
                    this.destinationPhysicalDevice = XFactoryCommon.InstanceSystem(physical.BuildXRI());
                    destinationPhysicalDevice.CommitStream(cacheStream, storageInfo);
                }
            }
            catch (Exception e)
            {
                mLog.Error($"zipfile and upload failed,error:{e}");
                throw;
            }
            finally
            {
                if (Directory.Exists(zipFolderPath))
                {
                    Directory.Delete(zipFolderPath, true);
                }
            }

        }
        private async Task ZipSearchResultFile()
        {
            string zipFileName = JobId + ".zip";
            string zipFolderPath = SecurityUtils.SafeCombinePath(RecordsEnv.AppDomainRootFolder, TEMPFOLDERNAME, JobId);
            string zipFilePath = SecurityUtils.SafeCombinePath(RecordsEnv.AppDomainRootFolder, TEMPFOLDERNAME, JobId, zipFileName);

            try
            {
                ZipUtil.ZipFolder(zipFolderPath, zipFilePath,"",Encoding.UTF8);
                var reportProfile = DownloadDataInfoDao.GetDownloadDataInfosByStatus(new List<int>() { (int)DownloadContentJobStatus.Wait })
                       .FirstOrDefault(item => item.JobId == JobId);
                if (reportProfile == null)
                {
                    mLog.Error($"Can not find report download info!");
                    return;
                }

                reportProfile.JobStatus = (int)DownloadContentJobStatus.InProgress;

                await DownloadDataInfoDao.UpdateAsync(reportProfile);
                FileStream = new FileTransferStream(zipFilePath, zipFolderPath, FileMode.Open);
                await UploadBlobAsync();
                if (fileInfo != null)
                {
                    reportProfile.FileSize = fileInfo.Length;
                }

                reportProfile.BlobSasUri = await DownloadCenterUtility.GenerateSasUri();

                mLog.Info("Upload blob success!");


                reportProfile.JobStatus = (int)DownloadContentJobStatus.Finished;

                DownloadDataInfoDao.UpdateDownloadInfo(reportProfile);
            }
            catch (Exception e)
            {
                mLog.Error($"zip the directory {zipFolderPath} failed, {e}");
            }
            finally
            {
                FileStream?.Close();
                if (Directory.Exists(zipFolderPath))
                {
                    Directory.Delete(zipFolderPath, true);
                }

            }

        }

        private async Task UploadBlobAsync()
        {
            var customId = TenantLocalValue.LogonGroupId;
            var blobName = SecurityUtils.SafeCombinePath(customId, JobId + ".zip");
            try
            {
                await Retryer.RetryAsync(() =>
                {
                    blobName = DownloadCenterUtility.UploadStorageForDownloadCenter(blobName, FileStream);
                    mLog.Info($"Upload export restore search result success");
                    return Task.CompletedTask;
                });
            }
            catch (Exception e)
            {
                mLog.Error($"Upload export restore search result failed,error is :{e}");
                throw;
            }

            mLog.Info($"finish to upload blob name:{blobName}");
            fileInfo = new FileInfo(FileStream.Name);
        }
    }
    public class DataRecord
    {
        public string Type { get; set; }
        public string SourceUrl { get; set; }
        public string Size { get; set; }
        public string CreatedTime { get; set; }
        public string LastModifiedTime { get; set; }
        public string ArchivedTime { get; set; }
        public string Name { get; set; }
        public string ModifiedBy { get; set; }
        public string CreateBy { get; set; }
        public string JobId { get; set; }
        public bool SoftDelete { get; set; }
    }
}
