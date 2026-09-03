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

    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Reflection;
    using System.Text;
    using AvePoint.GCommon;
    using AvePoint.GCommon.Contract.CodeReview;
    using AvePoint.GCommon.Contract.Media.Object;
    using AvePoint.GCommon.Contract.Server.Common;
    using AvePoint.GCommon.Contract.Server.ControlPanel.Object;
    using AvePoint.GCommon.Contract.Server.Job.Object;
    using AvePoint.GCommon.Contract.Storage.Entity;
    using AvePoint.GCommon.Contract.StorageOptimization.Object;
    using AvePoint.GCommon.Contract.Tree.Object;
    using AvePoint.GCommon.Utility;
    using AvePoint.Media.Common;
    using AvePoint.Media.Core.Index;
    using Merged18NResources.MediaServiceArchiverBackup;
    using AvePoint.Media.Service.DomainModel;
    using Storage;
    using AvePoint.Common;

    #endregion using directives

    #region CodeReview

    [AveCodeReview(
    "2012/2/29",
    "dwxue@avepoint.com",
    "jbli@avepoint.com",
    new string[] { CodeReviewConstants.CHECK_LIST_ID_CO_8 },
    "ADO-26066",
    false)]

    #endregion CodeReview

    public class ArchiverBackupUpgradeService
        : UpgradeServiceBase<ArchiverUpgradeInfo>
    {
        #region private field

        AveLogger logger = new AveLogger(MethodBase.GetCurrentMethod().DeclaringType);
        IXSystem oldIndexLogicalDevice;
        IXSystem newIndexLogicalDevice;
        ArchiverUpgradeInfo currentUpgradeInfo;
        ArchiverUpgradeSubInfo currentSubUpgradeInfo;
        String currentSiteUrl;
        String rootIndexDirectoryName;
        String rootDataDirectoryName;
        String errorMessage;
        Int32 finishedItemCount;
        List<JobDetail> jobDetailList = new List<JobDetail>();
        JobStatusInfo jobStatusInfo = new JobStatusInfo();

        #endregion private field

        #region public property

        public IJobProgressUpdater JobProgressUpdater { get; set; }

        public IVolumeGeneratorFactory VolumeGeneratorFactory { get; set; }

        public IIndexProcessor<ArchiverIndexProcessorParameter> OldIndexProcessor { get; set; }

        public IIndexProcessor<ArchiverIndexProcessorParameter> NewIndexProcessor { get; set; }

        #endregion public property

        #region static constants

        static readonly int jobDetailLimit = 200;
        static readonly int indexLimit = MediaConfigInfo.ArchiverConfigInfo.MergeIndexCount;
        static readonly string SelectTableNameArchiverHead = "SELECT * FROM " + IndexConstants.TableNameArchiveHead + " LIMIT @OFFSET, @LENGTH";
        static readonly string SelectTableNameArchiverBody = "SELECT * FROM " + IndexConstants.TableNameArchiveBody + " LIMIT @OFFSET, @LENGTH";
        static readonly string SelectArchiverHeadForReport = "SELECT COL_NAME, COL_TYPE, COL_EXTENSION_5 FROM " + IndexConstants.TableNameArchiveHead;
        static readonly string SelectArchiverBodyForReport = "SELECT COL_NAME, COL_TYPE, COL_EXTENSION_5 FROM " + IndexConstants.TableNameArchiveBody;
        static readonly string SelectTableNameArchiverSiteMaster = "SELECT * FROM " + IndexConstants.TableNameArchiveSiteMaster;
        static readonly string SelectTableNameArchiverJobInfo = "SELECT  * FROM " + IndexConstants.TableNameArchiveJobInfo + " WHERE COL_JOB_ID IS NOT NULL";
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
            oldIndexLogicalDevice = this.StorageDeviceManager.Open(upgradeInfo.OldIndexLogicalDevice.GetXRIS(PhysicalDeviceUsage.Index));
            var volumeGenerator = VolumeGeneratorFactory.GetVolumeGenerator(ProductModule.ArchiverBackup);
            rootIndexDirectoryName = volumeGenerator.GenerateIndexVolume(new VolumeParameter() { FarmName = string.Empty });
            rootDataDirectoryName = volumeGenerator.GenerateDataVolume(new VolumeParameter() { FarmName = string.Empty });
        }

        public override void Upgrade(ArchiverUpgradeInfo upgradeInfo)
        {
            var siteUpgradeInfos = GetAllSiteUpgradeInfos(upgradeInfo);
            foreach (var subUpgradeInfo in siteUpgradeInfos)
            {
                currentSubUpgradeInfo = subUpgradeInfo;
                try
                {
                    BackupD5Index(subUpgradeInfo);
                    OpenIndexProcessor(oldIndexLogicalDevice, OldIndexProcessor, Path.Combine(rootIndexDirectoryName, subUpgradeInfo.DirectoryPath));
                    List<ArchiverSiteMasterIndex> siteMasterIndexes = GetSiteMasterIndexes();
                    currentSiteUrl = siteMasterIndexes[0].SiteUrl;
                    if (upgradeInfo.FailedSiteCollections != null && upgradeInfo.FailedSiteCollections.Contains(currentSiteUrl))
                    {
                        logger.Warn(MediaServiceArchiverBackupResource.ArchiverBackupUpgradeServiceUpgradeContinue, currentSiteUrl);
                        continue;
                    }
                    var newIndexLogicalDeviceDto = GetIndexLogcialDevice(upgradeInfo, siteMasterIndexes[0]);
                    if (newIndexLogicalDeviceDto == null)
                    {
                        throw new KeyNotFoundException(String.Format(MediaServiceArchiverBackupResource.ArchiverBackupUpgradeServiceUpgradeIndexLogicalDeviceNotFound, currentSiteUrl));
                    }
                    UpdataWebsParentPathMd5();
                    UpdataPlatformType();
                    newIndexLogicalDevice = this.StorageDeviceManager.Open(newIndexLogicalDeviceDto.GetXRIS(PhysicalDeviceUsage.Index));
                    if (!oldIndexLogicalDevice.SystemLocation.EqualsIgnoreCase(newIndexLogicalDevice.SystemLocation))
                    {
                        newIndexLogicalDevice.Open();
                        StorageInfo info = new StorageInfo();
                        info.HighName = Path.Combine(rootIndexDirectoryName, subUpgradeInfo.DirectoryPath);
                        info.LowName = ServiceConstants.IndexDBName;
                        StorageInfo infoD6 = new StorageInfo();
                        infoD6.HighName = Path.Combine(Path.Combine(ServiceConstants.ArchiverPath, ServiceConstants.DefaultIndexVolume), subUpgradeInfo.DirectoryPath);
                        infoD6.LowName = ServiceConstants.IndexDBName;
                        if (newIndexLogicalDevice.FileExists(infoD6))
                        {
                            OpenIndexProcessor(newIndexLogicalDevice, NewIndexProcessor, infoD6.HighName);
                            MergeIndex();
                        }
                        else
                        {
                            CloseIndexProcessor(this.OldIndexProcessor);
                            oldIndexLogicalDevice.CopyFile(info, newIndexLogicalDevice, infoD6, true);
                            OpenIndexProcessor(oldIndexLogicalDevice, OldIndexProcessor, info.HighName);
                            var headIndexList = OldIndexProcessor.ExecuteQuery<ArchiverBasicIndex>(SelectArchiverHeadForReport, null);
                            var bodyIndexList = OldIndexProcessor.ExecuteQuery<ArchiverBasicIndex>(SelectArchiverBodyForReport, null);
                            headIndexList.AddRange(bodyIndexList);
                            foreach (var item in headIndexList)
                            {
                                GenerateJobDetail(item, string.Empty, 0);
                            }
                        }
                    }
                    foreach (var siteMasterIndex in siteMasterIndexes)
                    {
                        InsertControlSiteMasterIndex(siteMasterIndex);
                    }
                    var index = new ArchiverBasicIndex()
                    {
                        Name = String.Empty,
                        Type = "E",
                        ContentLength = 0
                    };
                    GenerateJobDetail(index, String.Empty, 0);
                    logger.Info(MediaServiceArchiverBackupResource.ArchiverBackupUpgradeServiceInsertControlSiteMasterIndexSuccessed, subUpgradeInfo.DirectoryPath);
                }
                catch (Exception ex)
                {
                    jobStatusInfo.State = 7;
                    var index = new ArchiverBasicIndex()
                    {
                        Name = String.Empty,
                        Type = "E",
                        ContentLength = 0
                    };
                    errorMessage = ServiceConstants.ArchiverUpgradeDataFailedMessage;
                    GenerateJobDetail(index, errorMessage, 1);
                    logger.Error(MediaServiceArchiverBackupResource.ArchiverBackupUpgradeServiceUpgradeError, ex.ToString());
                }
                finally
                {
                    this.finishedItemCount++;
                    this.JobProgressUpdater.UpdateJobProgress(this.jobStatusInfo, siteUpgradeInfos.Count, this.finishedItemCount, false);
                    if (newIndexLogicalDevice != null)
                    {
                        this.newIndexLogicalDevice.Close();
                    }
                    if (this.OldIndexProcessor != null)
                    {
                        this.OldIndexProcessor.Close();
                    }
                    if (this.NewIndexProcessor != null)
                    {
                        this.NewIndexProcessor.Close();
                    }
                }
            }
        }

        public override void ProcessException(Exception e)
        {
            e = e.InnerException ?? e;
            jobStatusInfo.State = 3;
            errorMessage = ServiceConstants.ArchiverUpgradeDataFailedMessage;
            this.logger.Error(MediaServiceArchiverBackupResource.ArchiverBackupUpgradeServiceProcessExceptionError, e.ToString());
        }

        public override void Dispose()
        {
            if (oldIndexLogicalDevice != null)
            {
                this.oldIndexLogicalDevice.Close();
            }
            if (newIndexLogicalDevice != null)
            {
                this.newIndexLogicalDevice.Close();
            }
            if (this.OldIndexProcessor != null)
            {
                this.OldIndexProcessor.Close();
            }
            if (this.NewIndexProcessor != null)
            {
                this.NewIndexProcessor.Close();
            }
            this.JobProgressUpdater.UpdateJobProgress(this.jobStatusInfo, 100, 100, true);
        }

        private void IndexBatchProcessWrap(String tableName, String sql, Boolean isVerifyMapped)
        {
            Dictionary<string, object> param = new Dictionary<string, object>();
            int count = tableName.EqualsIgnoreCase(IndexConstants.TableNameArchiveHead) ?
                Convert.ToInt32(this.OldIndexProcessor.ExecuteScalar(SelectTableNameArchiverHeadCount, null))
                : Convert.ToInt32(this.OldIndexProcessor.ExecuteScalar(SelectTableNameArchiverBodyCount, null));
            long number = count / indexLimit;
            long size = count % indexLimit;
            int offset = 0;

            if (number >= 1)
            {
                for (int i = 0; i < number; i++)
                {
                    param["@OFFSET"] = offset;
                    param["@LENGTH"] = indexLimit;
                    ExecuteInsert(tableName, sql, param);
                    offset = offset + indexLimit;
                }
            }
            if (size > 0)
            {
                param["@OFFSET"] = offset;
                param["@LENGTH"] = size;
                ExecuteInsert(tableName, sql, param);
            }
        }

        #region MergeIndex

        private void MergeIndex()
        {
            this.InsertIntoJobInfoTable();
            this.InsertIntoSiteMasterIndex();
            this.InsertIntoHeadTable();
            this.InsertIntoBodyTable();
        }

        private void InsertIntoBodyTable()
        {
            this.logger.Info(MediaServiceArchiverBackupResource.MergeIndexServiceInsertIntoMainIndexBodyBegin, IndexConstants.TableNameArchiveBody);
            this.IndexBatchProcessWrap(IndexConstants.TableNameArchiveBody, SelectTableNameArchiverBody, false);
            this.logger.Info(MediaServiceArchiverBackupResource.MergeIndexServiceInsertIntoMainIndexBodyEnd, IndexConstants.TableNameArchiveBody);
        }

        private void InsertIntoHeadTable()
        {
            this.logger.Info(MediaServiceArchiverBackupResource.MergeIndexServiceInsertIntoMainIndexHeadBegin, IndexConstants.TableNameArchiveHead);
            this.IndexBatchProcessWrap(IndexConstants.TableNameArchiveHead, SelectTableNameArchiverHead, false);
            this.logger.Info(MediaServiceArchiverBackupResource.MergeIndexServiceInsertIntoMainIndexHeadEnd, IndexConstants.TableNameArchiveHead);
        }

        private void ExecuteInsert(string tableName, string sql, Dictionary<string, object> param)
        {
            if (tableName.EqualsIgnoreCase(IndexConstants.TableNameArchiveHead))
            {
                List<ArchiverHeadIndex> indexes = this.OldIndexProcessor.ExecuteQuery<ArchiverHeadIndex>(sql, param);
                try
                {
                    this.NewIndexProcessor.Insert(indexes);
                    foreach (ArchiverHeadIndex index in indexes)
                    {
                        GenerateJobDetail(index, ServiceConstants.ArchiverUpgradeDataSuccessedMessage, 0);
                    }
                }
                catch (Exception e)
                {
                    logger.Error(MediaServiceArchiverBackupResource.ArchiverBackupUpgradeServiceExecuteInsertError, currentSiteUrl, e.ToString());
                    ReExecuteHeadInsert(indexes);
                }
            }
            else
            {
                List<ArchiverBodyIndex> indexes = this.OldIndexProcessor.ExecuteQuery<ArchiverBodyIndex>(sql, param);
                try
                {
                    this.NewIndexProcessor.Insert(indexes);
                    foreach (ArchiverBodyIndex index in indexes)
                    {
                        GenerateJobDetail(index, ServiceConstants.ArchiverUpgradeDataSuccessedMessage, 0);
                    }
                }
                catch (Exception ex)
                {
                    logger.Error(MediaServiceArchiverBackupResource.ArchiverBackupUpgradeServiceExecuteInsertError, currentSiteUrl, ex.ToString());
                    ReExecuteBodyInsert(indexes);
                }
            }
        }

        private void ReExecuteBodyInsert(List<ArchiverBodyIndex> indexes)
        {
            String sql = "select COL_ID from " + IndexConstants.TableNameArchiveBody;
            List<string> itemNamesList = this.NewIndexProcessor.ExecuteQueryForOneColume<string>(sql, null);
            foreach (var index in indexes)
            {
                try
                {
                    if (!itemNamesList.Contains(index.Id))
                    {
                        this.NewIndexProcessor.Insert(index);
                    }
                    GenerateJobDetail(index, ServiceConstants.ArchiverUpgradeDataSuccessedMessage, 0);
                }
                catch (Exception e)
                {
                    jobStatusInfo.State = 7;
                    errorMessage = ServiceConstants.ArchiverUpgradeDataFailedMessage;
                    GenerateJobDetail(index, errorMessage, 1);
                    logger.Error(MediaServiceArchiverBackupResource.ArchiverBackupUpgradeServiceReInsertError, index.Name, e.ToString());
                }
            }
        }

        private void ReExecuteHeadInsert(List<ArchiverHeadIndex> indexes)
        {
            String sql = "select COL_ID from " + IndexConstants.TableNameArchiveHead;
            List<string> itemNamesList = this.NewIndexProcessor.ExecuteQueryForOneColume<string>(sql, null);
            foreach (var index in indexes)
            {
                try
                {
                    if (!itemNamesList.Contains(index.Id))
                    {
                        this.NewIndexProcessor.Insert(index);
                    }
                    GenerateJobDetail(index, ServiceConstants.ArchiverUpgradeDataSuccessedMessage, 0);
                }
                catch (Exception e)
                {
                    jobStatusInfo.State = 7;
                    errorMessage = ServiceConstants.ArchiverUpgradeDataFailedMessage;
                    GenerateJobDetail(index, errorMessage, 1);
                    logger.Error(MediaServiceArchiverBackupResource.ArchiverBackupUpgradeServiceReInsertError, index.Name, e.ToString());
                }
            }
        }

        private void InsertIntoSiteMasterIndex()
        {
            List<ArchiverSiteMasterIndex> indexes = this.OldIndexProcessor.ExecuteQuery<ArchiverSiteMasterIndex>(SelectTableNameArchiverSiteMaster, null);
            try
            {
                this.NewIndexProcessor.Insert(indexes);
            }
            catch (Exception e)
            {
                logger.Error(MediaServiceArchiverBackupResource.ArchiverBackupUpgradeServiceInsertIntoSiteMasterIndexError, e.ToString());
            }
        }

        private void InsertIntoJobInfoTable()
        {
            List<ArchiverJobInfoIndex> indexes = this.OldIndexProcessor.ExecuteQuery<ArchiverJobInfoIndex>(SelectTableNameArchiverJobInfo, null);
            try
            {
                this.NewIndexProcessor.Insert(indexes);
            }
            catch (Exception e)
            {
                logger.Error(MediaServiceArchiverBackupResource.ArchiverBackupUpgradeServiceInsertIntoJobInfoTableError, e.ToString());
            }
        }

        #endregion MergeIndex

        #region private method

        private string GetItemLevel(string type)
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
            oldIndexLogicalDevice.CopyFile(srcInfo, destInfo, false);
        }

        private LogicalDeviceDto GetIndexLogcialDevice(ArchiverUpgradeInfo upgradeInfo, ArchiverSiteMasterIndex siteMasterIndex)
        {
            LogicalDeviceDto result = null;
            if (upgradeInfo.NewIndexLogicalDevices.ContainsKey(siteMasterIndex.SiteUrl))
            {
                result = upgradeInfo.NewIndexLogicalDevices[siteMasterIndex.SiteUrl];
            }
            else if (upgradeInfo.NewIndexLogicalDevices.ContainsKey(GetWebAppName(siteMasterIndex)))
            {
                result = upgradeInfo.NewIndexLogicalDevices[GetWebAppName(siteMasterIndex)];
            }
            else if (upgradeInfo.NewIndexLogicalDevices.ContainsKey(siteMasterIndex.FarmName))
            {
                result = upgradeInfo.NewIndexLogicalDevices[siteMasterIndex.FarmName];
            }
            return result;
        }

        private string GetWebAppName(ArchiverSiteMasterIndex siteMasterIndex)
        {
            StringBuilder webAppName = new StringBuilder();
            if (siteMasterIndex.WebAppName.EqualsIgnoreCase("http:") || siteMasterIndex.WebAppName.EqualsIgnoreCase("https:"))
            {
                for (int i = 0; i < 3; i++)
                {
                    webAppName.Append(siteMasterIndex.SiteUrl.Split('/')[i] + "/");
                }
            }
            else if (siteMasterIndex.WebAppName[siteMasterIndex.WebAppName.Length - 1].Equals('/'))
            {
                webAppName.Append(siteMasterIndex.WebAppName);
            }
            else
            {
                webAppName.Append(siteMasterIndex.WebAppName);
                webAppName.Append("/");
            }
            return webAppName.ToString();
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
            var directories = oldIndexLogicalDevice.ListDirectories(info);
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
            var directories = oldIndexLogicalDevice.ListDirectories(info);
            foreach (var directoryItem in directories)
            {
                subUpgradeInfo.DirectoryPath = Path.Combine(currentDirectory, directoryItem.Name);
                siteUpgradeInfos.AddRange(GetAllSiteUpgradeInfosFromWebApp(subUpgradeInfo));
            }
            return siteUpgradeInfos;
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
            return this.OldIndexProcessor.ExecuteQuery<ArchiverSiteMasterIndex>(sql, parameterDictionary);
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

        private List<JobSummary> GetJobSummary()
        {
            List<JobSummary> summaryList = new List<JobSummary>();
            summaryList.Add(new JobSummary() { Key = GConstants.JobSummaryKey.Comments, Value = this.errorMessage });
            return summaryList;
        }

        private void InsertControlSiteMasterIndex(ArchiverSiteMasterIndex siteMasterIndex)
        {
            if (siteMasterIndex.BackupTime.ToString().Length < 14)//is java time ?
            {
                siteMasterIndex.BackupTime = siteMasterIndex.BackupTime.JavaToDotNetTimeInLong();
            }
            var storagePolicies = GetStoragePolicyList(siteMasterIndex);
            foreach (var dataStoragePolicy in storagePolicies)
            {
                var archiverIndexSubInfo = new ArchiverIndexSubInfoContract()
                {
                    Id = Guid.NewGuid().ToString(),
                    JobId = siteMasterIndex.JobId,
                    LogicalDeviceId = dataStoragePolicy.PrimaryLogicalId,
                    PhysicalDeviceId = dataStoragePolicy.PrimaryStorage.PhysicalDrives[0].Id,
                    MediaServiceId = MediaEnvironment.MediaServer.MediaServerId,
                    StoragePolicyId = dataStoragePolicy.Id,
                    RetentionTime = siteMasterIndex.BackupTime,
                    RetentionTimeSpanSeconds = 0L
                };
                var archiverIndexSubInfoList = new List<ArchiverIndexSubInfoContract>();
                archiverIndexSubInfoList.Add(archiverIndexSubInfo);
                var archiverSiteMasterIndexContract = new ArchiverSiteMasterIndexContract
                {
                    JobId = siteMasterIndex.JobId.LastIndexOf('_') != -1 ? siteMasterIndex.JobId.Substring(0, siteMasterIndex.JobId.LastIndexOf('_')) : siteMasterIndex.JobId,
                    Id = Guid.NewGuid().ToString(),
                    ArchiverTime = siteMasterIndex.BackupTime,
                    FarmName = siteMasterIndex.FarmName,
                    IndexDeviceId = GetIndexLogcialDevice(currentUpgradeInfo, siteMasterIndex).Id,
                    WebURL = GetWebAppName(siteMasterIndex),
                    SiteURL = siteMasterIndex.SiteUrl,
                    WebId = string.Empty,
                    SiteId = string.Empty,
                    JobState = 0,
                    VersionDetails = new VersionDetails()
                    {
                        PlatformType = currentUpgradeInfo.PlatformType.ToString().ToEnum<AvePoint.GCommon.Contract.Media.Object.PlatformType>(),
                        ProductVersion = currentUpgradeInfo.ProductVersion,
                        LastImportedTime = DateTime.UtcNow.Ticks
                    },
                    StoragePolicyId = dataStoragePolicy.Id,
                    MediaServiceId = MediaEnvironment.MediaServer.MediaServerId,
                    SPVersion = siteMasterIndex.SPVersion,
                    MergeIndexState = MergeIndexState.Succeed,
                    SubInfo = archiverIndexSubInfoList,
                };
                try
                {
                    //this.ControlStubs.ArchiverUpgradeServiceSiteMasterIndexService.InsertSiteMasterIndex(archiverSiteMasterIndexContract);
                }
                catch (Exception ex)
                {
                    logger.Error(MediaServiceArchiverBackupResource.ArchiverBackupUpgradeServiceInsertControlSiteMasterIndexFailed, siteMasterIndex.SiteUrl, ex.ToString());
                    throw;
                }
            }
        }

        private List<StoragePolicyDto> GetStoragePolicyList(ArchiverSiteMasterIndex siteMasterIndex)
        {
            List<StoragePolicyDto> storagePolicyList;
            if (currentUpgradeInfo.JobStoragePolicies.Count != 0 && currentUpgradeInfo.JobStoragePolicies.ContainsKey(siteMasterIndex.JobId))
            {
                storagePolicyList = currentUpgradeInfo.JobStoragePolicies[siteMasterIndex.JobId];
            }
            else
            {
                storagePolicyList = currentUpgradeInfo.DataStoragePolicies;
            }
            return storagePolicyList;
        }

        private void CloseIndexProcessor(IIndexProcessor<ArchiverIndexProcessorParameter> indexProcessor)
        {
            if (indexProcessor != null)
            {
                indexProcessor.Close();
            }
        }

        private void UpdataWebsParentPathMd5()
        {
            string siteCollectionMd5 = currentSiteUrl.ToMD5HashCode();
            var parameterDictionary = new Dictionary<String, object>();
            String sql = "select * from " + IndexConstants.TableNameArchiveHead + " where COL_TYPE = 'W'";
            List<ArchiverBasicIndex> webIndexes = this.OldIndexProcessor.ExecuteQuery<ArchiverBasicIndex>(sql, parameterDictionary);
            var indexsNeedUpdate = webIndexes.FindAll((index) => { return index.Name.Contains("/") && index.ParentPathMD5.EqualsIgnoreCase(siteCollectionMd5); });
            indexsNeedUpdate.ForEach((index) =>
            {
                sql = "update " + IndexConstants.TableNameArchiveHead + " set COL_PARENT_PATH_MD5 = @COL_PARENT_PATH_MD5 where COL_ID = @COL_ID";
                string parentPath = Path.Combine(currentSiteUrl, index.Name.Substring(0, index.Name.LastIndexOf("/", StringComparison.OrdinalIgnoreCase)));
                parameterDictionary["@COL_PARENT_PATH_MD5"] = parentPath.ToMD5HashCode();
                parameterDictionary["@COL_ID"] = index.Id;
                this.OldIndexProcessor.Execute(sql, parameterDictionary);
            });
        }

        private void UpdataPlatformType()
        {
            var parameterDictionary = new Dictionary<String, object>();
            String sqlHead = "update " + IndexConstants.TableNameArchiveHead + " set COL_PLATFORM_TYPE = @COL_PLATFORM_TYPE";
            parameterDictionary["@COL_PLATFORM_TYPE"] = (int)currentUpgradeInfo.PlatformType;
            this.OldIndexProcessor.Execute(sqlHead, parameterDictionary);
            String sqlBody = "update " + IndexConstants.TableNameArchiveBody + " set COL_PLATFORM_TYPE = @COL_PLATFORM_TYPE";
            this.OldIndexProcessor.Execute(sqlBody, parameterDictionary);
        }

        #endregion private method
    }
}