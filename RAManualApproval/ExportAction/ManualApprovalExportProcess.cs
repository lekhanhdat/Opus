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
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Management.Automation;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using AvePoint.GCommon.Utility;
using AvePoint.GCommon.Utility.AzureBlobStorage;
using AvePoint.RA.Common;
using AvePoint.RA.Common.Configurations;
using AvePoint.RA.Common.Global.Utils;
using AvePoint.RA.Common.Retrying;
using AvePoint.RA.Common.Util;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract;
using AvePoint.RA.Contract.Configurations;
using AvePoint.RA.Contract.CustomizeConnector.Enums;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.Contract.ManualApproval.Model;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RMWeb.CP;
using AvePoint.RA.Contract.RoleAssignments;
using AvePoint.RA.Contract.Schedule;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Explorer.Dao.CosmosImp;
using AvePoint.RA.DB.Explorer.Model;
using AvePoint.RA.DB.Model;
using AvePoint.RA.DB.SecurityTrimming;
using AvePoint.RA.I18N.Core;
using AvePoint.RA.RACommonUtility.Common;
using AvePoint.RA.Service.Services.ManualApproval.Queriers;
using Newtonsoft.Json;
using OpenNLP.Tools.Util;
using RAExportCommon;
using RAManualApproval.ExportAction.History;
using RAManualApproval.ExportAction.UnderReview;
using RAManualApproval.Model;
using RATeams;
using static AvePoint.RA.RACommonUtility.Common.CommonUtilityForSpecialTenant;
using Directory = System.IO.Directory;

namespace RAManualApproval.ExportAction
{
    public abstract class ManualApprovalExportProcess
    {
        private static readonly RALogger Logger = RALogger.GetInstance(typeof(ManualUnderReviewExportProcessor));

        private static readonly IRMSubJobDao SubJobDao = PlatformWindsorManager.GetService<IRMSubJobDao>();


        private static readonly IDownloadDataInfoDao DownloadDataInfoDao = PlatformWindsorManager.GetService<IDownloadDataInfoDao>();

        private static readonly IGeneralSettingService GeneralSettingService = PlatformWindsorManager.GetService<IGeneralSettingService>();


        private static readonly IRMCustomizeConnectorContentSourceDao CustomizeConnectorContentSourceDao = PlatformWindsorManager.GetService<IRMCustomizeConnectorContentSourceDao>();

        private static readonly ITenantService TenantService = PlatformWindsorManager.GetService<ITenantService>();

        private static readonly RMRetryer Retryer = RMRetryerBuilder.CreateBuilder().Build();

        private static readonly GeneralSettingModel GeneralSetting = GeneralSettingService.GetGeneralSettingAsync().Result;


        private readonly Dictionary<int, string> UserDisplayNameCache = new();

        private static readonly Dictionary<ManualApprovalFilterOptions, IFilter> FilterCollection = new();

        private static readonly Dictionary<ManualApprovalOrderOptions, ISorter> SorterCollection = new();

        private string JobId;
        private string FullPath { get; set; }

        private string FolderPath { get; set; }
        private string Continuation { get; set; }
        private bool IsFirstBuild { get; set; }

        private Dictionary<int, string> ContentSourceInfoes { get; set; }

        private ManualApprovalQueryDefinition QueryDefinition;

        private readonly int CountOfOneSheet = 200000;

        private static readonly ManualApprovalRecordRepository Repository = new();
        private static readonly IUserService UserService = PlatformWindsorManager.GetService<IUserService>();

        private static readonly IRMSecurityTrimmingHelper SecurityTrimmingHelper = PlatformWindsorManager.GetService<IRMSecurityTrimmingHelper>();

        private readonly int PageSize = 1000;
        private List<Expression<Func<ManualApprovalRecord, bool>>> FilterExpresions { get; set; }
        private List<ManualApprovalExplorerOrderDefinition> SorterDefinitions { get; set; }
        private HashSet<int> NotHasPermissionSources { get; set; }
        protected abstract List<string> AssembleMaReviewInfoHeaderTittleForCsv();
        protected abstract Task<List<string>> GenerateRecordItemStringForCsvAsync(List<ManualApprovalRecord> manualItems, Dictionary<int, string> ContentSourceInfoes, Dictionary<int, string> UserDisplayNameCache);
        protected abstract void BuildStatusFilter(ManualApprovalQueryDefinition queryDefinition);

        protected abstract string ExportName { get; }
        public ManualApprovalExportProcess()
        {
        }

        private static void InitFilterCollection()
        {
            try
            {
                var filterType = typeof(IFilter);
                var assembly = Assembly.GetAssembly(filterType);

                foreach (var type in assembly.GetTypes())
                {
                    if (type.IsInterface) continue;
                    if (type.GetInterfaces().Contains(filterType))
                    {
                        var instance = Activator.CreateInstance(type) as IFilter;
                        FilterCollection.Add(instance.FilterOption, instance);
                    }
                }
                Logger.Info($"Succeed init filter collection.");
            }
            catch (Exception e)
            {
                Logger.Error($"An error occurred while init filter collection. Error: {e}");
                throw;
            }
        }

        private static void InitSorterCollection()
        {
            try
            {
                var sorterType = typeof(ISorter);
                var assembly = Assembly.GetAssembly(sorterType);

                foreach (var type in assembly.GetTypes())
                {
                    if (type.IsInterface) continue;
                    if (type.GetInterfaces().Contains(sorterType))
                    {
                        var instance = Activator.CreateInstance(type) as ISorter;
                        SorterCollection.Add(instance.OrderOption, instance);
                    }
                }
                Logger.Info($"Succeed init sorter collection.");
            }
            catch (Exception e)
            {
                Logger.Error($"An error occurred while init sorter collection. Error: {e}");
                throw;
            }
        }


        private static void UpdateDownloadDataInfo(RMDownloadDataInfo DownCenterInfo, DownloadContentJobStatus downloadStatus)
        {
            using (new PerformanceScope("Update download data ", $"Download data status is {downloadStatus}")) ;
            {
                DownCenterInfo.JobStatus = (int)downloadStatus;
                var success = DownloadDataInfoDao.UpdateDownloadInfo(DownCenterInfo);
                if (success)
                {
                    Logger.Info($"Update download file status to {downloadStatus} finished.");
                }
                else
                {
                    Logger.Info($"Update download file status to {downloadStatus} failed, retry update.");
                    success = DownloadDataInfoDao.UpdateDownloadInfo(DownCenterInfo);
                    var status = success ? "finished" : "failed";
                    Logger.Info($"Update retry download file {status}.");
                }
            }
        }

        private void InitNotHasPermissionSources()
        {
            NotHasPermissionSources = [];

            try
            {
                if (!TenantService.CheckLicenseWithAdditionalDataSource(TenantLocalValue.LogonGroupId, PaidForModule.Box))
                {
                    NotHasPermissionSources.Add((int)SourceFlag.Box);
                }
                if (!TeamsPermissionHelper.HasUpgradeTeamsFeature()) // todo: currently only check upgrade, will add check license when has
                {
                    NotHasPermissionSources.Add((int)SourceFlag.Teams);
                }

                Logger.Info($"Not has permission sources: [{string.Join(", ", NotHasPermissionSources.Select(cs => (SourceFlag)cs))}].");
            }
            catch (Exception ex)
            {
                Logger.Error($"An error occurred while init NotHasPermissionSources. Error: {ex}");
            }
        }


        public async Task RunAsync(string subJobId, string jobId)
        {
            ManualApprovalExportJobManager.Init(jobId, AvePoint.RA.Contract.JobMonitor.JobType.ManualExportRecordsForReviewDatasJob);

            JobId = jobId;

            var subJob = SubJobDao.GetSubJob(subJobId, true);

            QueryDefinition = AvePoint.RA.Common.Global.Utils.SerializerHelper.DeserializeByJsonSerializer<ManualApprovalQueryDefinition>(subJob.JobContext.Content);

            var nowDateTimeStr = GeneralSettingService.ConvertTiksToDateTime(GeneralSetting, DateTime.UtcNow.Ticks, false).DataTime.ToString("yyyyMMddhhmmss");

            FolderPath = JobReportUtility.GetDownloadManualApprovalReviewReportTempleFolder("Temple") + Path.DirectorySeparatorChar + ExportName + "_" + nowDateTimeStr + Guid.NewGuid();

             InitNotHasPermissionSources();

            ContentSourceInfoes = CustomizeConnectorContentSourceDao.GetAllSimpleInfoes(CustomizeConnectorOrigin.BuildIn, CustomizeConnectorOrigin.ExternalCustomize)
           .GetAwaiter().GetResult()
           .Where(cs => !NotHasPermissionSources.Contains(cs.Flag))
           .ToDictionary(item => item.Flag, item => I18NEntity.GetString(item.Name));

            InitFilterCollection();

            InitSorterCollection();

            var downloadDataInfo = DownloadDataInfoDao.GetDownloadDataInfosByStatus(new List<int>() { (int)DownloadContentJobStatus.Wait }).Where(item => item.JobId == JobId).First();
            try
            {
                UpdateDownloadDataInfo(downloadDataInfo, DownloadContentJobStatus.InProgress);

                await CreateForCSVAsync();

                var fileInfo = await UploadBlobAsync();

                if (fileInfo != null)
                {
                    downloadDataInfo.FileSize = fileInfo.Length;
                }

                downloadDataInfo.BlobSasUri = await DownloadCenterUtility.GenerateSasUri();

                ManualApprovalExportJobManager.HasSucceedDetail = true;

                UpdateDownloadDataInfo(downloadDataInfo, DownloadContentJobStatus.Finished);
            }
            catch (Exception e)
            {
                ManualApprovalExportJobManager.HasFailedDetail = true;
                ManualApprovalExportJobManager.JobComment = e.Message;
                Logger.Error($"Export Records for review Under Review datas failed ,{e}");
                UpdateDownloadDataInfo(downloadDataInfo, DownloadContentJobStatus.Failed);
            }
            finally
            {
                ManualApprovalExportJobManager.SetJobFinished();
                PerformanceMonitor.WritePerformanceResult();
            }
        }


        private async Task CreateForCSVAsync()
        {
            using (new PerformanceScope("Create csv file async", "", true))
            {
                try
                {

                    var hasNextExcel = true;
                    var fileIndex = 0;
                    IsFirstBuild = true;
                    do
                    {
                        GenerateFullPath();
                        var currentRowCount = 0;
                        using var stream = new FileStream(FullPath, FileMode.CreateNew, FileAccess.ReadWrite);
                        using var writer = new StreamWriter(stream, Encoding.UTF8);
                        var headers = AssembleMaReviewInfoHeaderTittleForCsv();
                        var headerLine = StringUtils.ToCSVString(headers.ToArray());
                        writer.WriteLine(headerLine);
                        do
                        {
                            var explorerQueryResult = await BuildQueryDefinitionAsync(QueryDefinition);
                            Continuation = explorerQueryResult.Continuation;
                            var manualItems = explorerQueryResult.Items;

                            var itemsLines = await GenerateRecordItemStringForCsvAsync(manualItems, ContentSourceInfoes, UserDisplayNameCache);
                            itemsLines.ForEach(writer.WriteLine);
                            currentRowCount += itemsLines.Count;
                            hasNextExcel = (currentRowCount >= CountOfOneSheet);
                            Logger.Info($"Insert data to csv {fileIndex} success,current row count is {currentRowCount}");
                        }
                        while (!string.IsNullOrEmpty(Continuation) && !hasNextExcel);
                        fileIndex++;
                    } while (!string.IsNullOrEmpty(Continuation) && hasNextExcel);
                }
                catch (Exception e)
                {
                    Logger.Error($"Create excel error, error :{e}");
                    throw;
                }
            }
        }

        private async Task<ManualApprovalResultModel> BuildQueryDefinitionAsync(ManualApprovalQueryDefinition queryDefinition)
        {
            using (new PerformanceScope("Query item from cosmos db", $"page size is {queryDefinition.PageSize}", true))
            {
                var repository = Repository;
                if (IsFirstBuild)
                {
                    using (new PerformanceScope("Export Action", "PermissionValidate"))
                    {
                        await PrePermissionValidateAsync(queryDefinition);
                    }
                    BuildStatusFilter(queryDefinition);

                    FilterExpresions = await BuildCosmosDBFilterAsync(queryDefinition, FilterCollection);
                    SorterDefinitions = BuildCosmosDBSorter(queryDefinition);
                    IsFirstBuild = false;
                }
                var explorerQueryDefinition = new ManualApprovalExplorerQueryDefinition
                {
                    PageSize = PageSize,
                    Continuation = Continuation,
                    Predicates = FilterExpresions,
                    OrderDefinitions = SorterDefinitions,
                };
                var explorerQueryResult = repository.QueryItemsWithPaginationAsync(explorerQueryDefinition).GetAwaiter().GetResult();
                var result = new ManualApprovalResultModel
                {
                    Continuation = explorerQueryResult.Continuation,
                    Items = explorerQueryResult.Items
                };
                return result;
            }

        }

        private void GenerateFullPath()
        {
            var nowDateTimeStr = GeneralSettingService.ConvertTiksToDateTime(GeneralSetting, DateTime.UtcNow.Ticks, false).DataTime.ToString("yyyyMMddhhmmss");
            var fileName = ExportName + "_" + nowDateTimeStr;
            FullPath = FolderPath + Path.DirectorySeparatorChar + fileName + ".csv";
            if (!System.IO.Directory.Exists(FolderPath))
            {
                Directory.CreateDirectory(FolderPath);
            }
        }

        private async Task PrePermissionValidateAsync(ManualApprovalQueryDefinition queryDefinition)
        {
            var isAdmin = await SecurityTrimmingHelper.DoesUserHasThisPermissionAsync(AvePoint.RA.Contract.RoleAssignments.RMPermissionMasks.ManualReviewAdmin);
            if (isAdmin)
            {
                return;
            }

            var reviewerFilter = new ManualApprovalFilterDefinition
            {
                FilterOption = ManualApprovalFilterOptions.Reviewer,
                Value = "[]"
            };
            queryDefinition.Filters.Add(reviewerFilter);

            var userHasPermissionIntIds = UserService.GetUserWithRemovedAndGroupIds(TenantLocalValue.LogonUserId);
            reviewerFilter.Value = JsonConvert.SerializeObject(userHasPermissionIntIds);
        }

        private async Task<List<Expression<Func<ManualApprovalRecord, bool>>>> BuildCosmosDBFilterAsync(ManualApprovalQueryDefinition queryDefinition, Dictionary<ManualApprovalFilterOptions, IFilter> FilterCollection)
        {
            var result = new List<Expression<Func<ManualApprovalRecord, bool>>>();
            foreach (var filterDefinition in queryDefinition.Filters)
            {
                var filterOption = filterDefinition.FilterOption;
                var filter = FilterCollection[filterOption];
                var expression = await filter.GetCosmosDBFilterExpressionAsync(filterDefinition.Value);
                result.Add(expression);
            }
            result.Add(item => item.IsManualSynced);
            result.Add(item => item.ManualArchiveStatus != (int)ActionStatus.Archiverd);
            result.Add(item => item.RecordStatus != (int)RMRecordStatus.Hidden);
            result.Add(item => item.RecordStatus != (int)RMRecordStatus.RMDeleted);
            result.Add(item => !NotHasPermissionSources.Contains(item.SourceFlag));
            return result;
        }

        private List<ManualApprovalExplorerOrderDefinition> BuildCosmosDBSorter(ManualApprovalQueryDefinition queryDefinition)
        {
            var result = new List<ManualApprovalExplorerOrderDefinition>();
            if (queryDefinition.OrderBy != ManualApprovalOrderOptions.None)
            {
                var sorter = SorterCollection[queryDefinition.OrderBy];
                var expression = sorter.GetCosmosDBOrderExpression();
                result.Add(new ManualApprovalExplorerOrderDefinition
                {
                    OrderKeySelector = expression,
                    IsDesc = queryDefinition.IsDesc
                });
            }

            if (queryDefinition.OrderBy == ManualApprovalOrderOptions.None)
            {
                var collectionTimeSorter = SorterCollection[ManualApprovalOrderOptions.CollectioinTime];
                var collectionTimeExpression = collectionTimeSorter.GetCosmosDBOrderExpression();
                result.Add(new ManualApprovalExplorerOrderDefinition
                {
                    OrderKeySelector = collectionTimeExpression,
                    IsDesc = true
                });
            }

            return result;
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
                        Logger.Info($"Upload manual approval export success");
                        return Task.CompletedTask;
                    });
                }
                catch (Exception e)
                {
                    Logger.Error($"Upload manual approval export failed,error is :{e}");
                    throw;
                }

                Logger.Info($"finish to upload blob name:{blobName}");
                return new FileInfo(FolderPath + ".zip");
            }
        }
    }
}
