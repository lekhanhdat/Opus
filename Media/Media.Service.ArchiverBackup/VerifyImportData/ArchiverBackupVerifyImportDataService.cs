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




namespace AvePoint.Media.Service.ArchiverBackup
{
    #region using directives

    using AvePoint.GCommon;
    using AvePoint.GCommon.Contract.Server.Common;
    using AvePoint.GCommon.Contract.Server.ControlPanel.Object;
    using AvePoint.GCommon.Contract.Server.Job.Object;
    using AvePoint.GCommon.Contract.Storage.Entity;
    using AvePoint.GCommon.Contract.Tree.Object;
    using AvePoint.GCommon.Utility;
    using AvePoint.Media.Common;
    using AvePoint.Media.Core.Index;
    using Merged18NResources.MediaServiceArchiverBackup;
    using AvePoint.Media.Service.DomainModel;

    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Reflection;
    using Storage;
    using AvePoint.Common;

    #endregion using directives

    public class ArchiverBackupVerifyImportDataService :
        VerifyImportDataServiceBase<ArchiverUpgradeInfo>
    {
        #region private field

        AveLogger logger = new AveLogger(MethodBase.GetCurrentMethod().DeclaringType);
        IXSystem indexLogicalDevice;
        ArchiverUpgradeInfo currentUpgradeInfo;
        ArchiverUpgradeSubInfo currentSubUpgradeInfo;
        String currentSiteUrl;
        String rootIndexDirectoryName;
        String rootDataDirectoryName;
        String errorMessage;
        Int32 finishedItemCount;
        List<String> failedSiteCollectionUrl = new List<String>();
        List<JobDetail> jobDetailList = new List<JobDetail>();
        List<String> existBlockNames = new List<String>();
        List<String> lostBlockNames = new List<String>();
        JobStatusInfo jobStatusInfo = new JobStatusInfo();

        #endregion private field

        #region public property

        public IJobProgressUpdater JobProgressUpdater { get; set; }

        public IVolumeGeneratorFactory VolumeGeneratorFactory { get; set; }

        public IFileNameGeneratorFactory FileNameGeneratorFactory { get; set; }

        public IIndexProcessor<ArchiverIndexProcessorParameter> IndexProcessor { get; set; }

        #endregion public property

        #region static constants

        static readonly int jobDetailLimit = 200;
        static readonly int indexLimit = MediaConfigInfo.ArchiverConfigInfo.MergeIndexCount;
        static readonly string SelectTableNameArchiverHead = "SELECT * FROM " + IndexConstants.TableNameArchiveHead + " LIMIT @OFFSET, @LENGTH";
        static readonly string SelectTableNameArchiverBody = "SELECT * FROM " + IndexConstants.TableNameArchiveBody + " LIMIT @OFFSET, @LENGTH";
        static readonly string SelectTableNameArchiverHeadCount = "SELECT COUNT(*) FROM " + IndexConstants.TableNameArchiveHead;
        static readonly string SelectTableNameArchiverBodyCount = "SELECT COUNT(*) FROM " + IndexConstants.TableNameArchiveBody;

        #endregion static constants

        public override void Open(ArchiverUpgradeInfo upgradeInfo)
        {
            this.currentUpgradeInfo = upgradeInfo;
            this.jobStatusInfo.State = 2;
            this.jobStatusInfo.Type = upgradeInfo.JobType;
            this.jobStatusInfo.Id = upgradeInfo.JobId;
            this.JobProgressUpdater.UpdateJobProgress(jobStatusInfo, 100, 1, false);
            indexLogicalDevice = this.StorageDeviceManager.Open(upgradeInfo.OldIndexLogicalDevice.GetXRIS(PhysicalDeviceUsage.Index));
            var volumeGenerator = VolumeGeneratorFactory.GetVolumeGenerator(ProductModule.ArchiverBackup);
            rootIndexDirectoryName = volumeGenerator.GenerateIndexVolume(new VolumeParameter() { FarmName = string.Empty });
            rootDataDirectoryName = volumeGenerator.GenerateDataVolume(new VolumeParameter() { FarmName = string.Empty });
        }

        public override void Verify(ArchiverUpgradeInfo upgradeInfo)
        {
            var siteUpgradeInfos = GetAllSiteUpgradeInfos(upgradeInfo);
            foreach (var subUpgradeInfo in siteUpgradeInfos)
            {
                try
                {
                    BackupD5Index(subUpgradeInfo);
                    OpenIndexProcessor(indexLogicalDevice, IndexProcessor, Path.Combine(rootIndexDirectoryName, subUpgradeInfo.DirectoryPath));
                    var siteMasterIndexes = GetSiteMasterIndexes();
                    currentSiteUrl = siteMasterIndexes[0].SiteUrl;
                    currentSubUpgradeInfo = subUpgradeInfo;
                    logger.Info(MediaServiceArchiverBackupResource.ArchiverBackupUpgradeServiceVerifyDataMappedBegin, currentSiteUrl);
                    IndexBatchProcessWrap(IndexConstants.TableNameArchiveHead, SelectTableNameArchiverHead, true);
                    IndexBatchProcessWrap(IndexConstants.TableNameArchiveBody, SelectTableNameArchiverBody, true);
                }
                catch (Exception ex)
                {
                    logger.Error(MediaServiceArchiverBackupResource.ArchiverBackupUpgradeServiceVerifyDataMappedError, ex.ToString());
                    errorMessage = ServiceConstants.UpgradeDataNotMappedAllMessage;
                    if (!failedSiteCollectionUrl.Contains(currentSiteUrl))
                    {
                        failedSiteCollectionUrl.Add(currentSiteUrl);
                        //this.ControlStubs.ArchiverUpgradeServiceSiteMasterIndexService.SetUpgradeNodeState(new EIVerifyMessage
                        //{
                        //    JobId = currentUpgradeInfo.JobId,
                        //    SiteUrl = currentSiteUrl
                        //});
                    }
                    var index = new ArchiverBasicIndex()
                    {
                        Name = String.Empty,
                        Type = "E",
                        ContentLength = 0
                    };
                    GenerateJobDetail(index, errorMessage, 1);
                    jobStatusInfo.State = 3;
                }
                finally
                {
                    finishedItemCount++;
                    logger.Info(MediaServiceArchiverBackupResource.ArchiverBackupUpgradeServiceVerifyDataMappedFinished, currentSiteUrl);
                    this.JobProgressUpdater.UpdateJobProgress(this.jobStatusInfo, siteUpgradeInfos.Count, this.finishedItemCount, false);
                    IndexProcessor.Close();
                }
            }
            if (failedSiteCollectionUrl.Count > 0 && failedSiteCollectionUrl.Count < siteUpgradeInfos.Count)
            {
                jobStatusInfo.State = 7;
            }
        }

        public override void ProcessException(Exception e)
        {
            e = e.InnerException ?? e;
            jobStatusInfo.State = 3;
            errorMessage = ServiceConstants.UpgradeDataNotMappedAllMessage;
            this.logger.Error(MediaServiceArchiverBackupResource.ArchiverBackupUpgradeServiceProcessExceptionError, e.ToString());
        }

        public override void GenerateJobReport()
        {
            this.logger.Info(MediaServiceArchiverBackupResource.ArchiverBackupUpgradeServiceGenerateJobReportBegin);
            try
            {
                BaseJobDto jobInfo = new BaseJobDto() { Id = this.jobStatusInfo.Id, Category = (Int32)PlanCategory.DataManager, PlanId = currentUpgradeInfo.PlanId, Type = currentUpgradeInfo.JobType };
                this.ControlStubs.JobDetailService.UpdateJobDetails(jobDetailList, jobInfo);
                this.ControlStubs.JobDetailService.UpdateJobSummary(GetJobSummary(), jobInfo);
            }
            catch (Exception ex)
            {
                this.logger.Info(MediaServiceArchiverBackupResource.ArchiverBackupUpgradeServiceGenerateJobReportError, ex.ToString());
            }
            this.logger.Info(MediaServiceArchiverBackupResource.ArchiverBackupUpgradeServiceGenerateJobReportFinished);
        }

        public override void Dispose()
        {
            this.existBlockNames.Clear();
            this.lostBlockNames.Clear();
            if (indexLogicalDevice != null)
            {
                this.indexLogicalDevice.Close();
            }
            if (this.IndexProcessor != null)
            {
                this.IndexProcessor.Close();
            }
            this.JobProgressUpdater.UpdateJobProgress(this.jobStatusInfo, 100, 100, true);
        }

        private void IndexBatchProcessWrap(String tableName, String sql, Boolean isVerifyMapped)
        {
            Dictionary<string, object> param = new Dictionary<string, object>();
            int count = tableName.EqualsIgnoreCase(IndexConstants.TableNameArchiveHead) ?
                Convert.ToInt32(this.IndexProcessor.ExecuteScalar(SelectTableNameArchiverHeadCount, null))
                : Convert.ToInt32(this.IndexProcessor.ExecuteScalar(SelectTableNameArchiverBodyCount, null));
            long number = count / indexLimit;
            long size = count % indexLimit;
            int offset = 0;

            if (number >= 1)
            {
                for (int i = 0; i < number; i++)
                {
                    param["@OFFSET"] = offset;
                    param["@LENGTH"] = indexLimit;
                    ExecuteVerify(sql, param);
                    offset = offset + indexLimit;
                }
            }
            if (size > 0)
            {
                param["@OFFSET"] = offset;
                param["@LENGTH"] = size;
                ExecuteVerify(sql, param);
            }
        }

        private void ExecuteVerify(string sql, Dictionary<string, object> param)
        {
            List<ArchiverBasicIndex> indexes = this.IndexProcessor.ExecuteQuery<ArchiverBasicIndex>(sql, param);
            foreach (var indexItem in indexes)
            {
                try
                {
                    VerifyDataFileExist(indexItem, FileType.MetaData);
                    if (indexItem.ContentLength != 0)
                    {
                        VerifyDataFileExist(indexItem, FileType.Content);
                    }
                }
                catch (Exception e)
                {
                    logger.Error(MediaServiceArchiverBackupResource.ArchiverBackupUpgradeServiceExecuteVerifyError, indexItem.Name, e.ToString());
                    errorMessage = ServiceConstants.UpgradeDataNotMappedAllMessage;
                    jobStatusInfo.State = 3;
                    GenerateJobDetail(indexItem, errorMessage, 1);
                    if (!failedSiteCollectionUrl.Contains(currentSiteUrl))
                    {
                        failedSiteCollectionUrl.Add(currentSiteUrl);
                        //this.ControlStubs.ArchiverUpgradeServiceSiteMasterIndexService.SetUpgradeNodeState(new EIVerifyMessage
                        //{
                        //    JobId = currentUpgradeInfo.JobId,
                        //    SiteUrl = currentSiteUrl
                        //});
                    }
                }
            }
        }

        private void VerifyDataFileExist(ArchiverBasicIndex index, FileType fileType)
        {
            Boolean result = false;
            StorageInfo info = new StorageInfo();
            String mappedMessage = fileType == FileType.MetaData ? ServiceConstants.MappedMetaDataMessage : ServiceConstants.MappedContentMessage;
            String notMappedMessage = fileType == FileType.MetaData ? ServiceConstants.NotMappedMetaDataMessage : ServiceConstants.NotMappedContentMessage;
            info.HighName = GetFileHighName(fileType, index);
            var fileNameGeneratorFactory = FileNameGeneratorFactory.GetFileNameGenerator(ProductModule.ArchiverBackup, (DataVersion)index.Version);
            info.LowName = fileNameGeneratorFactory.Generate(new FileNameParameter(index, fileType));

            if (existBlockNames.Contains(Path.Combine(info.HighName, info.LowName)))
            {
                GenerateJobDetail(index, mappedMessage, 0);
            }
            else if (lostBlockNames.Contains(Path.Combine(info.HighName, info.LowName)))
            {
                GenerateJobDetail(index, notMappedMessage, 1);
            }
            else
            {
                var logicalDevice = GetLogicalDevice(index.BackupJobId);
                IXSystem dataLogicalDevice = this.StorageDeviceManager.Open(logicalDevice.GetXRIS(PhysicalDeviceUsage.Data));
                result = dataLogicalDevice.FileExists(info);
                dataLogicalDevice.Close();
                if (result)
                {
                    existBlockNames.Add(Path.Combine(info.HighName, info.LowName));
                    GenerateJobDetail(index, mappedMessage, 0);
                }
                else
                {
                    if (!failedSiteCollectionUrl.Contains(currentSiteUrl))
                    {
                        failedSiteCollectionUrl.Add(currentSiteUrl);
                        //this.ControlStubs.ArchiverUpgradeServiceSiteMasterIndexService.SetUpgradeNodeState(new EIVerifyMessage
                        //{
                        //    JobId = currentUpgradeInfo.JobId,
                        //    SiteUrl = currentSiteUrl
                        //});
                    }
                    lostBlockNames.Add(Path.Combine(info.HighName, info.LowName));
                    errorMessage = ServiceConstants.UpgradeDataNotMappedAllMessage;
                    GenerateJobDetail(index, notMappedMessage, 1);
                    logger.Error(MediaServiceArchiverBackupResource.ArchiverBackupUpgradeServiceVerifyDataFileExistError, currentSiteUrl + index.Name, info.HighName + "\\" + info.LowName);
                    jobStatusInfo.State = 3;
                }
            }
        }

        private LogicalDeviceDto GetLogicalDevice(String jobId)
        {
            var logicalDevice = new LogicalDeviceDto();
            if (currentUpgradeInfo.JobStoragePolicies.Count == 0)
            {
                this.currentUpgradeInfo.DataStoragePolicies.ForEach(storagePolicy =>
                {
                    storagePolicy.PrimaryStorage.PhysicalDrives.ForEach(physicalDevice =>
                    {
                        logger.Info(physicalDevice.Name);
                        logicalDevice.PhysicalDrives.Add(physicalDevice);
                    });
                });
            }
            else if (this.currentUpgradeInfo.JobStoragePolicies.ContainsKey(jobId))
            {
                this.currentUpgradeInfo.JobStoragePolicies[jobId].ForEach(storagePolicy =>
                {
                    storagePolicy.PrimaryStorage.PhysicalDrives.ForEach(physicalDevice =>
                    {
                        logger.Info(physicalDevice.Name);
                        logicalDevice.PhysicalDrives.Add(physicalDevice);
                    });
                });
            }
            return logicalDevice;
        }

        private String GetFileHighName(FileType fileType, ArchiverBasicIndex index)
        {
            String result = Path.Combine(rootDataDirectoryName, currentSubUpgradeInfo.DirectoryPath);
            return result;
        }

        private void GenerateJobDetail(ArchiverBasicIndex index, String message, int status)
        {
            JobDetail jobDetail = new JobDetail();
            jobDetail.MediaHost = MediaEnvironment.MediaServer.MediaServerName;
            jobDetail.Status = status;
            jobDetail.Message = message;
            jobDetail.Remark7 = currentSiteUrl;
            jobDetail.Remark8 = index.Name;
            jobDetail.Remark9 = GetItemLevel(index.Type);
            jobDetail.Size = index.ContentLength;
            jobDetailList.Add(jobDetail);
            if (jobDetailList.Count == jobDetailLimit)
            {
                BaseJobDto jobInfo = new BaseJobDto() { Id = this.jobStatusInfo.Id, Category = (Int32)PlanCategory.DataManager, PlanId = currentUpgradeInfo.PlanId, Type = currentUpgradeInfo.JobType };
                this.ControlStubs.JobDetailService.UpdateJobDetails(jobDetailList, jobInfo);
                jobDetailList.Clear();
            }
        }

        private String GetItemLevel(string type)
        {
            String itemLevel = string.Empty;
            switch (type)
            {
                case "E":
                    itemLevel = "Site Collection";
                    break;
                case "W":
                    itemLevel = "Site";
                    break;
                case "L":
                    itemLevel = "List";
                    break;
                case "D":
                case "V":
                    itemLevel = "Document";
                    break;
                case "A":
                    itemLevel = "Attachment";
                    break;
                case "F":
                    itemLevel = "Folder";
                    break;
                case "I":
                case "U":
                    itemLevel = "Item";
                    break;
            }
            return itemLevel;
        }

        private void BackupD5Index(ArchiverUpgradeSubInfo subUpgradeInfo)
        {
            StorageInfo srcInfo = new StorageInfo()
            {
                HighName = Path.Combine(rootIndexDirectoryName, subUpgradeInfo.DirectoryPath),
                LowName = ServiceConstants.IndexDBName
            };
            StorageInfo destInfo = new StorageInfo()
            {
                HighName = Path.Combine(rootIndexDirectoryName, subUpgradeInfo.DirectoryPath),
                LowName = ServiceConstants.ArchiverIndexDBName
            };
            indexLogicalDevice.CopyFile(srcInfo, destInfo, false);
        }

        private void OpenIndexProcessor(IXSystem indexLogicalDevice, IIndexProcessor<ArchiverIndexProcessorParameter> indexProcessor, String indexVolume)
        {
            var openParam = new ArchiverIndexServiceOpenParameter()
            {
                IndexDatabaseName = ServiceConstants.IndexDBName,
                IndexVolume = indexVolume,
                IndexLogicalDeviceSystem = indexLogicalDevice,
            };
            IndexDatabaseDownLoadResult indexDownLoadInfo;
            StorageInfo logicalStorageInfo = XConvert.FromNames(openParam.IndexVolume, openParam.IndexDatabaseName, openParam.StorageInfo);
            if (openParam.IndexLogicalDeviceSystem.FileExists(logicalStorageInfo))
            {
                indexDownLoadInfo = new IndexDatabaseDownLoadResult(IndexDatabaseStatus.Cached, SecurityUtils.SafeCombinePath(openParam.IndexLogicalDeviceSystem.SystemLocation, SecurityUtils.SafeCombinePath(openParam.IndexVolume, openParam.IndexDatabaseName)));
            }
            else
            {
                indexDownLoadInfo = new IndexDatabaseDownLoadResult(IndexDatabaseStatus.Nonexistent, SecurityUtils.SafeCombinePath(openParam.IndexLogicalDeviceSystem.SystemLocation, SecurityUtils.SafeCombinePath(openParam.IndexVolume, openParam.IndexDatabaseName)));
            }
            openParam.IndexLogicalDeviceSystem.OpenDirectory(XConvert.FromNames(openParam.IndexVolume, string.Empty), FileMode.Create);
            IdentityManager.IdentityMode = IdentityMode.Process;
            ArchiverIndexProcessorParameter param = new ArchiverIndexProcessorParameter(IdentityManager.IdentityContent);
            param.DownLoadResult = indexDownLoadInfo;
            param.IndexWorkingSystem = openParam.IndexLogicalDeviceSystem;
            this.logger.Info(MediaServiceArchiverBackupResource.ArchiverBackupUpgradeServiceOpenIndexProcessorBegin);
            indexProcessor.Open(param);
            this.logger.Info(MediaServiceArchiverBackupResource.ArchiverBackupUpgradeServiceOpenIndexProcessorSuccessed);
        }

        private List<ArchiverSiteMasterIndex> GetSiteMasterIndexes()
        {
            var parameterDictionary = new Dictionary<String, object>();
            String sql = "select * from " + IndexConstants.TableNameArchiveSiteMaster;
            return this.IndexProcessor.ExecuteQuery<ArchiverSiteMasterIndex>(sql, parameterDictionary);
        }

        private List<ArchiverUpgradeSubInfo> GetAllSiteUpgradeInfos(ArchiverUpgradeInfo importInfo)
        {
            List<ArchiverUpgradeSubInfo> siteUpgradeInfos = new List<ArchiverUpgradeSubInfo>();
            foreach (var subUpgradeInfo in importInfo.ImportInfos)
            {
                switch (subUpgradeInfo.TreeNodeLevel)
                {
                    case NodeLevel.Farm:
                        siteUpgradeInfos.AddRange(GetAllSiteUpgradeInfosFromFarm(subUpgradeInfo));
                        break;
                    case NodeLevel.WebApplication:
                        siteUpgradeInfos.AddRange(GetAllSiteUpgradeInfosFromWebApp(subUpgradeInfo));
                        break;
                    case NodeLevel.SiteCollection:
                        siteUpgradeInfos.Add(subUpgradeInfo);
                        break;
                    default:
                        break;
                }
            }
            if (siteUpgradeInfos.Count == 0)
            {
                throw new Exception(String.Format(MediaServiceArchiverBackupResource.ArchiverBackupVerifyImportDataServiceGetUpgradeInfoError, importInfo.OldIndexLogicalDevice.Name));
            }
            return siteUpgradeInfos;
        }

        private List<ArchiverUpgradeSubInfo> GetAllSiteUpgradeInfosFromWebApp(ArchiverUpgradeSubInfo subUpgradeInfo)
        {
            StorageInfo info = new StorageInfo();
            List<ArchiverUpgradeSubInfo> siteUpgradeInfos = new List<ArchiverUpgradeSubInfo>();
            info.HighName = Path.Combine(rootIndexDirectoryName, subUpgradeInfo.DirectoryPath);
            var directories = indexLogicalDevice.ListDirectories(info);
            foreach (var directoryItem in directories)
            {
                ArchiverUpgradeSubInfo siteUpgradeInfo = new ArchiverUpgradeSubInfo();
                siteUpgradeInfo.TreeNodeLevel = NodeLevel.SiteCollection;
                siteUpgradeInfo.DirectoryPath = Path.Combine(subUpgradeInfo.DirectoryPath, directoryItem.Name);
                siteUpgradeInfos.Add(siteUpgradeInfo);
            }
            return siteUpgradeInfos;
        }

        private List<ArchiverUpgradeSubInfo> GetAllSiteUpgradeInfosFromFarm(ArchiverUpgradeSubInfo subUpgradeInfo)
        {
            StorageInfo info = new StorageInfo();
            String currentDirectory = subUpgradeInfo.DirectoryPath;
            List<ArchiverUpgradeSubInfo> siteUpgradeInfos = new List<ArchiverUpgradeSubInfo>();
            info.HighName = Path.Combine(rootIndexDirectoryName, subUpgradeInfo.DirectoryPath);
            var directories = indexLogicalDevice.ListDirectories(info);
            foreach (var directoryItem in directories)
            {
                subUpgradeInfo.DirectoryPath = Path.Combine(currentDirectory, directoryItem.Name);
                siteUpgradeInfos.AddRange(GetAllSiteUpgradeInfosFromWebApp(subUpgradeInfo));
            }
            return siteUpgradeInfos;
        }

        private List<JobSummary> GetJobSummary()
        {
            List<JobSummary> summaryList = new List<JobSummary>();
            summaryList.Add(new JobSummary() { Key = GConstants.JobSummaryKey.Comments, Value = this.errorMessage });
            return summaryList;
        }
    }
}