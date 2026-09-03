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
using AvePoint.RA.Common.Retrying;
using AvePoint.RA.Common.Threads;
using AvePoint.RA.Common.Util;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Exceptions;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.Contract.JobMonitor;
using AvePoint.RA.Contract.Object.JobMessage;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RMWeb.Explorer;
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using AvePoint.RA.Contract.RMWeb.TemplateManagement;
using AvePoint.RA.Contract.TemplateManagement;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Explorer.Dao.CosmosImp;
using AvePoint.RA.DB.Explorer.Model;
using AvePoint.RA.DB.Model;
using AvePoint.RA.I18N.Core;
using AvePoint.RA.RACommonUtility.Common;
using AvePoint.RA.RAPhysical.User;
using OpenNLP.Tools.Util;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AvePoint.RA.RAPhysical.Import
{
    public class ExportPhysicalZipRecordsWork : IDisposable
    {
        private RALogger logger = RALogger.GetInstance(typeof(ExportPhysicalZipRecordsWork));
        #region Job Param
        private JobType jobType;
        private string jobRunBy;
        private string mCurrentJobId;
        private string mGlobalTimeZoneId;

        private List<int> templateIds;
        private JobResult Result;
        private string commomErrorMessage = "RM_TS_SS_Summary";
        private int FailedItemCount = 0;
        private static int itemsPerTask = 5;

        private int CheckFileSizeItemsCount = 1000;
        private long PerFileSize = 49 * 1024 * 1024;

        private long TotalRecordCount = 0;
        private bool IsAdmin = false;
        private List<Guid> PhysicalLocationPermission;
        //private int CheckFileSizeItemsCount = 10;
        //private long PerFileSize = 5 * 1024;

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

        private static readonly RMRetryer retryer = RMRetryerBuilder.CreateBuilder().Build();

        private ITaxonomyService _TaxonomyService;
        private ITaxonomyService TaxonomyService => PlatformWindsorManager.GetService(ref _TaxonomyService);

        public ITemplateManagementService TemplateManagementService => PlatformWindsorManager.GetService<ITemplateManagementService>();
        private static readonly IDownloadDataInfoDao DownloadDataInfoDao = PlatformWindsorManager.GetService<IDownloadDataInfoDao>();
        #endregion

        #region Init

        public ExportPhysicalZipRecordsWork(RMExportPhysicalZipRecordMessage msg)
        {
            this.jobType = msg.JobType;
            this.jobRunBy = msg.JobRunBy;
            mCurrentJobId = msg.JobID;
            mGlobalTimeZoneId = msg.GlobalTimeZoneId;

            List<int> temIdList = new List<int>();
            msg.TemplateIds.Split(",").ForEach(tem =>
            {
                temIdList.Add(int.Parse(tem));
            });
            this.templateIds = temIdList;
            ReportMangerFactory.Instance.Init(mCurrentJobId, this.jobType);

            Result = new JobResult();

            //默认初始化 进度为2
            ReportManager.Increase(2);
            ReportManager.StartUpdateJobProgress();
        }

        #endregion
        protected UserService userService = new UserService();

        private async Task InitUserPermission()
        {
            (PhysicalLocationPermission, IsAdmin) = await userService.GetPhysicalLocationPermissionAsync();
        }

        public async Task ImportPhysicalRecordsAsync()
        {
            logger.Info("Begin to import physical records!");
            JobStatus status = JobStatus.None;
            try
            {
                await InitUserPermission();
                #region 生成临时文件夹
                string tenantGroupId = TenantLocalValue.LogonGroupId;
                string tempPath = JobReportUtility.GetDownloadPhysicalBulkImportZipTempleFolder(tenantGroupId);
                tempPath += (jobType.ToString()+ DateTime.Now.ToString("yyyyMMddhhssmm"));
                if (!Directory.Exists(tempPath))
                {
                    Directory.CreateDirectory(tempPath);
                }
                #endregion
                // 创建
                DownloadDataInfoDao.CreateDownloadDataInfo(new RMDownloadDataInfo()
                {
                    FileDownloadTime = DateTime.UtcNow.Ticks,
                    JobId = mCurrentJobId,
                    RecordsId = Guid.NewGuid(),
                    DownloadType = DownloadContentType.PhysicalBuklExport,
                    JobStatus = (int)DownloadContentJobStatus.InProgress,
                    UserId = TenantLocalValue.LogonUserId,
                    Name = mCurrentJobId + ".zip"
                });
                #region 生成数据
                foreach (int id in templateIds)
                {
                    try
                    {
                        logger.Info($"Export template:{id}");
                        Dictionary<string, List<string[]>> dic = new Dictionary<string, List<string[]>>();
                        TemplateDto template = await TemplateManagementService.LoadTemplateDtoAsync(id);
                        if (template != null && template.categories != null && template.categories.Count > 0)
                        {
                            List<string> headers = new List<string>();
                            //新增Sheet， 按Teplate生成Excel Header
                            foreach (TemplateCategoryDto categoryDto in template.categories)
                            {
                                foreach (TemplateColumnDto column in categoryDto.columns)
                                {
                                    string uniqueId = column.uniqueId.ToString().ToLower();
                                    // 删除HomeLocation列
                                    if (uniqueId == Contract.TemplateManagement.DefaultColumnIDs.LoanedBy || uniqueId == DefaultColumnIDs.HomeLocation)
                                    {
                                        logger.Info("Not supported loan by yet");
                                        continue;
                                    }
                                    headers.Add(I18NEntity.GetString(column.columnName));
                                }
                            }
                            headers.Insert(0, "Unique ID");
                            string[] header1 = new string[headers.Count];
                            header1[0] = I18NEntity.GetString(template.name);
                            // excel 数据
                            List<string[]> data = new List<string[]>() { header1, headers.ToArray() };
                            SynchronizedCollection<string[]> dataItem = new SynchronizedCollection<string[]>();

                            // 查询Records数据
                            ExplorerQueryOptionV2 explorerQueryOptionV2 = PhysicalExplorerQueryDtoExtension.GetDefaultQueryOptionV2();
                            explorerQueryOptionV2.FilterOption.PhysicalTemplateds = new List<int>()
                        {
                            template.id
                        };
                            
                            ExplorerQueryV2Dto explorerQueryV2Dto = new()
                            {
                                QueryOption = explorerQueryOptionV2,
                                // 如何查询下一页的数据
                                PagingInfo = new ExplorerPagingInfo()
                                {
                                    PageSize = 500,
                                    Total = 0
                                }
                            };

                            if (!IsAdmin)
                            {
                                explorerQueryV2Dto.QueryOption.FilterOption.PhysicalLocationIds = PhysicalLocationPermission;
                            }

                            var builder = DB.Explorer.Dao.CosmosImp.Builder.SqlQuerySpecBuilderFactory.Create();
                            do
                            {
                                Tuple<IEnumerable<Record>, string> queryData = default;
                                using (var scope = new PerformanceScope("Find one page of data", addToStatistics: true))
                                {
                                    queryData = ExplorerDao.SearchRecordsV2(explorerQueryV2Dto, builder);
                                }
                                if (queryData.Item1.IsNullOrEmpty())
                                {
                                    break;
                                }
                                TotalRecordCount += queryData.Item1.Count();
                                explorerQueryV2Dto.PagingInfo.PageIndex = queryData.Item2;
                                #region 拼接item数据
                                int existingItemsPerTask = queryData.Item1.Count() / 4;
                                void ProcessOneItem(Record item, TemplateDto template, SynchronizedCollection<string[]> dataItem)
                                {
                                    using (new CheckJobStopScope()) { }
                                    using var scope = new PerformanceScope("Export One Item", addToStatistics: true);
                                    JMImportPhysicalRecordsJobDetail detail = new JMImportPhysicalRecordsJobDetail();
                                    detail.SrcRecordType = "N/A";
                                    detail.TemplateName = template.name;
                                    detail.DestRecordType = template.type.ToString();
                                    try
                                    {

                                        List<string> cellData = new List<string>();
                                        cellData.Add(item.RecordsId);
                                        detail.UniqueId = item.RecordsId;
                                        detail.Title = item.LeafName;
                                        foreach (TemplateCategoryDto categoryDto in template.categories)
                                        {
                                            foreach (TemplateColumnDto column in categoryDto.columns)
                                            {
                                                string uniqueId = column.uniqueId.ToString().ToLower();
                                                // 删除HomeLocation列
                                                if (uniqueId == Contract.TemplateManagement.DefaultColumnIDs.LoanedBy || uniqueId == DefaultColumnIDs.HomeLocation)
                                                {
                                                    logger.Info("Not supported loan by yet");
                                                    continue;
                                                }
                                                if (item.CustomColumnDic.ContainsKey(column.uniqueId.ToString()))
                                                {
                                                    foreach (var customColumn in item.CustomColumnDic)
                                                    {
                                                        if (Guid.Parse(customColumn.Key) == column.uniqueId)
                                                        {
                                                            if (customColumn.Key == DefaultColumnIDs.Classification)
                                                            {
                                                                using var scope1 = new PerformanceScope("GetTermPathByTermId", addToStatistics: true);
                                                                var classification = TaxonomyService.GetTermPathByTermId(item.TermId, forExport: true);
                                                                cellData.Add(classification);
                                                                if (string.IsNullOrEmpty(classification))
                                                                {
                                                                    detail.Comment = "Failed to analyse term path, inherit from parent.";
                                                                }
                                                                break;
                                                            }
                                                            else if (customColumn.Key == DefaultColumnIDs.Status)
                                                            {
                                                                cellData.Add(CustomColumnExtension.GetSingleChoiceColumnValue(customColumn.Value).Name);
                                                                break;
                                                            }
                                                            else
                                                            {
                                                                ///  SingleText = 1,
                                                                ////MultipleText = 2,
                                                                ////DateTime = 3,
                                                                ////SingleChoice = 4,
                                                                ////PeopleOrGroup = 5,
                                                                ////Number = 6,
                                                                ////MultipleChoice = 7,
                                                                ////Taxonomy = 10,
                                                                // 需要处理不同的Type
                                                                string val;
                                                                switch (column.typeId)
                                                                {
                                                                    case 1:
                                                                        val = CustomColumnExtension.GetSingleTextColumnValue(customColumn.Value);
                                                                        break;
                                                                    case 2:
                                                                        val = CustomColumnExtension.GetMultipleTextColumnValue(customColumn.Value);
                                                                        break;
                                                                    case 3:
                                                                        val = DateTimeUtil.ConvertTimeFromUtc(customColumn.Value.Date.Ticks, customColumn.Value.TimeZoneId, customColumn.Value.IsSetDayLight).ToString();
                                                                        break;
                                                                    case 4:
                                                                        val = CustomColumnExtension.GetSingleChoiceColumnValue(customColumn.Value).Name;
                                                                        break;
                                                                    case 5:
                                                                        val = string.Join(";", CustomColumnExtension.GetPeopleOrGroupColumnValue(customColumn.Value).Select(u => u.UserPrincipalName));
                                                                        break;
                                                                    case 6:
                                                                        val = CustomColumnExtension.GetNumberColumnValue(customColumn.Value);
                                                                        break;
                                                                    case 7:
                                                                        val = string.Join(";", CustomColumnExtension.GetMultipleChoiceColumnValue(customColumn.Value).Select(explorerQueryOptionV2 => explorerQueryOptionV2.Name));
                                                                        break;
                                                                    case 10:
                                                                        val = CustomColumnExtension.GetTaxonomyColumnValue(customColumn.Value).Name;
                                                                        break;
                                                                    default:
                                                                        val = CustomColumnExtension.GetSingleTextColumnValue(customColumn.Value);
                                                                        break;
                                                                }
                                                                cellData.Add(val);
                                                                if(customColumn.Key == DefaultColumnIDs.Barcode)
                                                                {
                                                                    detail.Barcode = val;
                                                                }
                                                                break;
                                                            }
                                                        }
                                                    }
                                                }
                                                else
                                                {
                                                    if(uniqueId == DefaultColumnIDs.Barcode)
                                                    {
                                                        cellData.Add(item.RecordsId);
                                                        continue;
                                                    }
                                                    cellData.Add(string.Empty);
                                                }
                                            }
                                        }
                                        dataItem.Add(cellData.ToArray());
                                    }
                                    catch (JobStopException)
                                    {
                                        throw new JobStopException("This Job is stopped.");
                                    }
                                    catch (InputParameterException ex)
                                    {
                                        detail.Status = JobDetailsStatus.Failed;
                                        detail.Comment = ex.Message;
                                        Result.HasFailed = true;
                                        Interlocked.Increment(ref FailedItemCount);
                                        logger.Warn(ex.ToString());
                                    }
                                    catch (SkipItemException ex)
                                    {
                                        detail.Status = JobDetailsStatus.Skipped;
                                        detail.Comment = ex.Message;
                                    }
                                    catch (Exception e)
                                    {
                                        detail.Status = JobDetailsStatus.Failed;
                                        detail.Comment = string.Format(I18NEntity.GetString("RM_CommonErrorMessage"), e.Message);
                                        Result.HasFailed = true;
                                        Interlocked.Increment(ref FailedItemCount);
                                        logger.Error(@"Update physical record failed.RecordsId:[{0}],Error:{1}", item.RecordsId, e);
                                    }
                                    finally
                                    {
                                        ReportManager.Increase();
                                        if (!CheckJobStatusUtility.isStopping)
                                        {
                                            ReportManager.Increase();
                                            ReportManager.SendJobDetail(detail);
                                        }
                                    }
                                }
                                if (queryData.Item1.Count() > itemsPerTask)
                                {
                                    AveTenantTasks.RunParallel(queryData.Item1, existingItemsPerTask, new CancellationTokenSource(), item =>
                                    {
                                        ProcessOneItem(item, template, dataItem);
                                    });
                                }
                                else
                                {
                                    foreach (var item in queryData.Item1)
                                    {
                                        ProcessOneItem(item, template, dataItem);
                                    }
                                }
                                #endregion
                            }
                            while (!string.IsNullOrEmpty(explorerQueryV2Dto.PagingInfo.PageIndex));

                            if (dataItem.IsNotNullOrEmpty())
                            {
                                // 把item里面的数据全部添加到data中
                                data.AddRange(dataItem);
                            }

                            // 添加excel sheet名称
                            dic.Add(I18N.Core.I18NEntity.GetString(template.name), data);
                        }
                        CreateCsv(dic, SecurityUtils.SafeCombinePath(tempPath, $"{I18N.Core.I18NEntity.GetString(template.name)}"));
                        Result.HasSuccessful = true;
                    }
                    catch (Exception e)
                    {
                        logger.Error($"Export template {id} error, {e}");
                    }
                }
                #endregion
                // 压缩zip
                logger.Debug("export excel finished:{0}", tempPath);
                //ZipUtil.ZipFolder(tempPath, tempPath + JobMonitorConstants.ZIP, Encoding.UTF8);
                logger.Debug("zip file finished:{0}", tempPath);


                #region 存储到storage center中 
                var downloadDataInfo = DownloadDataInfoDao.GetDownloadDataInfosByStatus([(int)DownloadContentJobStatus.InProgress]).Where(item => item.JobId == mCurrentJobId).First();

                var zipPath = await UploadBlobAsync(tempPath);
                FileInfo fileInfo = new(zipPath);
                downloadDataInfo.FileSize = fileInfo.Length;
                downloadDataInfo.BlobSasUri = await DownloadCenterUtility.GenerateSasUri();

                UpdateDownloadDataInfo(downloadDataInfo, DownloadContentJobStatus.Finished);

                #endregion


                status = Result.HasFailed
                    ? Result.HasSuccessful
                        ? JobStatus.FinishWithException
                        : JobStatus.Failed
                    : JobStatus.Finished;
                // 删除解压的zip文件夹和zip文件
                DirectoryInfo directory = new DirectoryInfo(tempPath);
                directory.Delete(true);
                System.IO.File.Delete(tempPath + JobMonitorConstants.ZIP);
            }
            catch (Exception e)
            {
                status = JobStatus.Failed;
                logger.Error($"Import physical records failed , error {e}");
                var downloadDataInfo = DownloadDataInfoDao.GetDownloadDataInfosByStatus([(int)DownloadContentJobStatus.InProgress]).Where(item => item.JobId == mCurrentJobId).First();
                UpdateDownloadDataInfo(downloadDataInfo, DownloadContentJobStatus.Failed);
            }
            finally
            {

                string jobComment = (status == JobStatus.FinishWithException || status == JobStatus.Failed)
                    ? commomErrorMessage
                    : string.Empty;
                if(TotalRecordCount == 0)
                {
                    jobComment = "RM_RDM_MA_ExportNoData";
                }
                ReportManager.SetJobFinished(status, jobComment);

            }
        }

        /// <summary>
        /// 创建csv文件
        /// </summary>
        /// <param name="dic"></param>
        /// <param name="tempPath"></param>
        /// <returns></returns>
        public string CreateCsv(Dictionary<string, List<string[]>> dic, string tempPath)
        {
            FileStream stream = null;
            StreamWriter writer = null;

            var fileIndex = 0;
            string fileName = string.Empty;
            var dataIndex = 0;
            var needWriteNextFile = true;
            try
            {
                foreach (var item in dic)
                {
                    var headers = item.Value.Take(2).ToArray();
                    foreach (var itemStr in item.Value.Skip(2))
                    {
                        if (needWriteNextFile)
                        {
                            fileIndex++;
                            fileName = fileIndex == 1 ? $"{tempPath}.csv" : $"{tempPath}_{fileIndex}.csv";
                            logger.Info($"File name is {fileName}");
                            needWriteNextFile = false;
                            writer?.Dispose();
                            stream?.Dispose();

                            stream = new FileStream(fileName, FileMode.CreateNew, FileAccess.ReadWrite);
                            writer = new StreamWriter(stream, Encoding.UTF8);
                            foreach (var header in headers)
                            {
                                var headerLine = StringUtils.ToCSVString(header);
                                writer?.WriteLine(headerLine);
                            }
                        }
                        dataIndex++;
                        var dataLine = StringUtils.ToCSVString(itemStr);
                        writer?.WriteLine(dataLine);
                        if (CheckFileSizeItemsCount == dataIndex)
                        {
                            dataIndex = 0;
                            writer?.Flush();
                            stream?.Flush();
                            if (new FileInfo(fileName).Length > PerFileSize)
                            {
                                needWriteNextFile = true;
                            }
                        }
                    }
                }
                if (fileIndex > 1)
                {
                    File.Move($"{tempPath}.csv", $"{tempPath}_1.csv");
                }
            }
            finally
            {
                writer?.Dispose();
                stream?.Dispose();
            }
            return tempPath;
        }

        private async Task<string> UploadBlobAsync(string localZipFolderPath)
        {
            var zipPath = localZipFolderPath + ".zip";
            using (new PerformanceScope("Upload blob to azure storage", "", true))
            {
                AvePoint.GCommon.ZipUtil.ZipFolder(localZipFolderPath, zipPath, Encoding.UTF8);
                var customId = TenantLocalValue.LogonGroupId;
                var blobName = SecurityUtils.SafeCombinePath(customId, mCurrentJobId + ".zip");//System.IO.Path.Combine(customId, mCurrentJobId + ".zip");
                try
                {
                    await retryer.RetryAsync(() =>
                    {
                        blobName = DownloadCenterUtility.UploadStorageForDownloadCenter(blobName, zipPath);
                        logger.Info($"Upload report profile details success");
                        return Task.CompletedTask;
                    });
                }
                catch (Exception e)
                {
                    logger.Error($"Upload report profile details failed,error is :{e}");
                    throw;
                }

                logger.Info($"finish to upload blob name:{blobName}");
            }
            return zipPath;
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

        #region Dispose method
        public void Dispose()
        {

        }
        #endregion
    }

}
