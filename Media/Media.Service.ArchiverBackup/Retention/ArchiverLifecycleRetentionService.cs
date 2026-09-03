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
    using System.Linq;
    using System.Collections.Generic;
    using System.IO;
    using System.Reflection;
    using System.Text;
    using System.Threading;
    using AvePoint.GCommon;
    using AvePoint.GCommon.Contract.CodeReview;
    using AvePoint.GCommon.Contract.Media.Object;
    using AvePoint.GCommon.Contract.Server.Job.Object;
    using AvePoint.GCommon.Contract.Storage.Entity;
    using AvePoint.Media.Common;
    using Merged18NResources.MediaServiceArchiverBackup;
    using AvePoint.Media.Service.DomainModel;
    using AvePoint.GCommon.Contract.Server.Job;
    using AvePoint.GCommon.Contract.Server.ControlPanel.Object;
    using AvePoint.Media.Core.Index;
    using AvePoint.GCommon.Utility;
    using AvePoint.GCommon.Contract.StorageOptimization.Object;
    using AvePoint.GCommon.Contract.CommonFilter;
    using Newtonsoft.Json;
    using AvePoint.RA.DB.Core;
    using Azure.Data.Tables;
    using Azure;
    using AvePoint.RA.DB.Model;
    using AvePoint.Common;
    using Storage;
    using AvePoint.RA.DB.Dao;
    using AvePoint.RA.DB.Explorer.Dao;
    using AvePoint.RA.Common;
    using AvePoint.RA.DB.Dao.Extension;
    using AvePoint.RA.DB.Explorer.Bulk;
    using AvePoint.RA.DB.Explorer.Model;
    using AvePoint.RA.Contract.Explorer;
    using AvePoint.RA.Contract.RMWeb.ReportCenter;
    using AvePoint.RA.Contract.Schedule;
    using AvePoint.RA.DB.AzureTable.Model;
    using System.Xml;
    using RAManualApproval.Converters;
    using AvePoint.Records.Core.Utilities.Extensions;
    using AvePoint.RA.DB.Explorer.Dao.CosmosImp;
    using AvePoint.RA.Contract.RMWeb.Tree.Base;
    using AvePoint.RA.Contract;
    using static ICSharpCode.SharpZipLib.Zip.ExtendedUnixData;
    using RAManualApprovalCommon.Model;
    using RAManualApprovalCommon;
    using NVelocity.Runtime.Resource;
    using AvePoint.RA.RACommonUtility.Workflow;
    using AvePoint.RA.Common.Email;
    using AvePoint.RA.RACommonUtility.Email.Model;
    using AvePoint.RA.RACommonUtility.Email.Sender;
    using AvePoint.RA.I18N.Core;
    using AvePoint.RA.Contract.RMWeb.StorageDevice;
    using AvePoint.GCommon.Contract.Server.ControlPanel.Office365;
    using AvePoint.Wrapper.Common;
    using AvePoint.RA.RACommonUtility.Browser;
    using AvePoint.Wrapper.Common.Office;
    using RAManualApprovalCommon.Archiver;
    using AvePoint.RA.Contract.TaxonomyModel;
    using AvePoint.RA.Contract.Tenant;
    using AngleSharp.Common;
    using AvePoint.Media.Core.IO;
    using AvePoint.RA.Contract.Common;
    using AvePoint.RA.DB.Dao.Impl;
    using AvePoint.RA.Contract.RMWeb.JobMonitor;
    using DocumentFormat.OpenXml.Wordprocessing;
    using AvePoint.RA.Contract.Telemetry;
    using AvePoint.RA.Contract.RMWeb.Telemetry;
    using System.Threading.Tasks;

    #endregion using directives
    public class ArchiverLifecycleRetentionService : RetentionServiceBase<ArchiverLifecycleRetentionInfo, ArchiverLifecycleRetentionResult>
        , IRetentionService
    {
        AveLogger logger = new AveLogger(MethodBase.GetCurrentMethod().DeclaringType);
        Dictionary<int, Rule> Rules;
        ArchiverMessage ArchiverMessage;
        ArchiverRetentionInfo archiverRetentionInfo = new ArchiverRetentionInfo();
        IXSystem indexLogicalDevice;
        IXSystem dataLogicalDevice;
        String dataVolume;
        String indexVolume;
        Boolean isObjectType;
        String ErrorMessage = ServiceConstants.ArchvierRetentionFailedMessage;
        bool IsOneDriveSite = false;
        bool IsTeams = false;
        private String rehydrationTemp;
        private IExplorerDao explorerDao = new ExplorerDao();
        private IRMKeyValueDao keyValueDao;
        private bool isCosmosBulkOperationEnabled = false; //是否开启了批量插入数据到cosmos db
        private int bulkSize = 0;
        private int mNodeLevel;
        private string mSiteId;
        private Guid mSiteGroupId;
        private string mScanJobID = null;
        private readonly RMEmailSender _emailSender;
        public static string ScanSubJobId
        {
            get;
            private set;
        }
        public IIndexService<ArchiverIndexServiceOpenParameter> IndexService { get; set; }

        public IArchiverRetentionIndexService RetentionIndexService { get; set; }

        public IIndexProcessor<ArchiverIndexProcessorParameter> IndexMainProcessor { get; set; }

        private IRMArchiveSiteInfoDao ArchiveSiteInfoDao = PlatformWindsorManager.GetService<IRMArchiveSiteInfoDao>();
        private IRetentionIndexSubInfoDao RetentionIndexSubInfoDao => PlatformWindsorManager.GetService<IRetentionIndexSubInfoDao>();
        private IRATelemetryService RATelemetryService => PlatformWindsorManager.GetService<IRATelemetryService>();

        private IArchiverSiteMasterIndexDao ArchiverSiteMasterIndexDao = PlatformWindsorManager.GetService<IArchiverSiteMasterIndexDao>();
        private IRMRemoteNodeDao RemoteNodeDao => PlatformWindsorManager.GetService<IRMRemoteNodeDao>();
        private ArchiverManualAction mManualAction;
        private readonly string SelectTableNameArchiverBodyFileCount = "SELECT COUNT(*) FROM " + IndexConstants.TableNameArchiveBody + " WHERE COL_TYPE = 'D' AND COL_NAME NOT LIKE '%:%'";
        private readonly string SelectTableNameArchiverBodyVersionCount = "SELECT COUNT(*) FROM " + IndexConstants.TableNameArchiveBody + " WHERE COL_TYPE = 'D' AND COL_NAME LIKE '%:%'";
        private IAveORecords Record
        {
            get
            {
                IAveORecords records = AveObjectModelFactory.CreateObjectModelFactory("", null, AveContextKind.Auto).CreateRecords();
                return records;
            }
        }
        public IStorageDeviceManager DeviceManager { get; set; }
        private void InitCosmosBulkOperation()
        {
            keyValueDao = (IRMKeyValueDao)PlatformWindsorManager.GetService(typeof(IRMKeyValueDao));
            isCosmosBulkOperationEnabled = keyValueDao.IsCosmosBulkOperationEnabled();
            bulkSize = keyValueDao.GetCosmosBulkInsertOperationBufferSize();
            if (bulkSize == default(int)) bulkSize = CosmosBulkOperator.DefualtBufferSize;
            logger.Info($"Cosmos bulk operation enabled, bulk size: {bulkSize}");
            if (isCosmosBulkOperationEnabled)
            {
                CosmosBulkOperator.Instance.Start(bulkSize, UpdateProcessSucceedRecord, UpdateProcessFailedRecord);
            }
        }

        public override void Open(ArchiverLifecycleRetentionInfo retentionInfo)
        {
            this.mSiteId = retentionInfo.SiteId;
            this.mNodeLevel = retentionInfo.NodeLevel;
            this.mSiteGroupId = retentionInfo.SiteGroupId;
            this.IsOneDriveSite = retentionInfo.IsOneDriveSite;
            this.IsTeams = retentionInfo.IsTeams;
            this.archiverRetentionInfo = retentionInfo;
            this.Rules = retentionInfo.Rules;
            this.ArchiverMessage = retentionInfo.ArchiverMessage;
            this.logger.Info(MediaServiceArchiverBackupResource.ArchiverBackupRetentionServiceOpenStart, this.archiverRetentionInfo.JobId); 
            //this.JobDetailService = JobReportServiceFactory.CreateJobDetailService();
            //var mediaDataService = JobReportServiceFactory.CreateMediaDataService(); 
            //mediaDataService.InitiateMediaDataService(IdentityManager.IdentityContent);
            //this.ArchiverJobManagementService = JobReportServiceFactory.CreateArchiverJobManagementService();
             
            this.dataVolume = retentionInfo.DataVolume;
            this.indexVolume = retentionInfo.IndexVolume;
            this.logger.Info(MediaServiceArchiverBackupResource.RetentionServiceOpenIndexAndDataDevice);
            this.indexLogicalDevice = this.DeviceManager.Open(this.archiverRetentionInfo.IndexLogicalDevice.GetXRIS(PhysicalDeviceUsage.Index));
            this.dataLogicalDevice = this.DeviceManager.Open(this.archiverRetentionInfo.DataLogicalDevice.GetXRIS(PhysicalDeviceUsage.Data));
            this.CacheManager.Open(retentionInfo.CacheSetting, false, true);
            this.isObjectType = this.dataLogicalDevice.StorageInterfaceType.Equals(StorageInterfaceType.Object) || this.indexLogicalDevice.StorageInterfaceType.Equals(StorageInterfaceType.Object);
            this.logger.Info(MediaServiceArchiverBackupResource.RetentionServiceOpenIndexAndDataDeviceFinished);
             
            this.OpenMainIndex(this.archiverRetentionInfo, this.indexVolume); 
            this.rehydrationTemp = Path.Combine("data_archive", "Temp" + Guid.NewGuid());
            if (retentionInfo.JobId.Contains("_"))
            {
                this.mScanJobID = retentionInfo.JobId.Substring(0, retentionInfo.JobId.IndexOf('_'));
                ScanSubJobId = retentionInfo.JobId;//msg.Job.Id;
            }
            else
            {
                this.mScanJobID = retentionInfo.JobId;
                ScanSubJobId = this.mScanJobID;
            }
            InitCosmosBulkOperation();

            if (mManualAction == null)
            {
                mManualAction = new LifecycleRetentionManualApprovalExecutor(mScanJobID, Guid.Empty);
            }
        }

        public override ArchiverLifecycleRetentionResult Retain(ArchiverLifecycleRetentionInfo retentionInfo)
        {
            var retentionResult = new ArchiverLifecycleRetentionResult() { sucessItem = new List<ArchiverBasicIndex>(), manualSkippedItem = new List<ArchiverBasicIndex>(), DoesNotSupportSharePointItem = new List<ArchiverBasicIndex>() };
            List<string> needClearManualRules = new List<string>();
            //this.AzureDBWorker = new SOArchiverAzureDBWorker(ArchiverMessage, this.IsOneDriveSite);
            List<string> usedRuleIds = this.RetentionIndexService.GetUniqueRetentions();
            try
            {
                foreach (string ruleId in usedRuleIds)
                {
                    bool removeStub = false;
                    Rule rule = GetRule(ruleId);
                    if (rule != null && rule.IsEnableRetention)
                    {
                        if (!string.IsNullOrEmpty(rule.StoragePolicyId))
                        {
                            var storageDevice = StorageDeviceService.GetStorageDeviceById(rule.StoragePolicyId);
                            if (storageDevice != null && storageDevice.SetupDataRetention && storageDevice.ArchiveRetentionRules.Count > 0)
                            {
                                foreach (var retentionRule in storageDevice.ArchiveRetentionRules)
                                {
                                    if (retentionRule.IsMove)
                                    {
                                        continue;
                                    }
                                    if (retentionRule.RemoveOrphanedStub)
                                    {
                                        retentionInfo.TenantGroupId = TenantLocalValue.LogonGroupId;
                                        removeStub = true;
                                        break;
                                    }
                                }
                            }
                        }

                        List<ArchiverBasicIndex> manualItem = new List<ArchiverBasicIndex>();
                        logger.Info("Start to process retetion in rule {0}", rule.Name);
                        long expireDAte = this.CalculateExpireDate(rule.RetentionInfo);
                        logger.Info("Queryable expire date long is {0}", expireDAte);
                        List<ArchiverBasicIndex> indexes = this.RetentionIndexService.GetRetentionData(rule.Id, expireDAte);
                        logger.Info("doc count in the {0}, is {1}", rule.Name, indexes.Count);
                        List<ArchiverBasicIndex> needDeleteDataAndIndex = new List<ArchiverBasicIndex>();
                        foreach (var index in indexes)
                        {
                            index.ListBaseType = rule.Name; //for job detail
                            if (rule.RetentionInfo != null && rule.RetentionInfo.IsManualApproval)
                            {
                                logger.Info("need check manual info, id {0}", index.PathMD5);
                                manualItem.Add(index); 
                            }
                            else
                            {
                                logger.Info("need to remove or check manual info, id {0}", index.PathMD5);
                                //remove data, index;  
                                needDeleteDataAndIndex.Add(index);
                            }
                        }
                        List<ArchiverBasicIndex> needManualDelete = new List<ArchiverBasicIndex>();
                        List<Record> needUpdateEntity = null;
                        List<ArchiverBasicIndex> doesNotSupportSharePointGroup = null;
                        if (manualItem.Count > 0)
                        {
                            var result = ProcessManualApproveData(manualItem, rule, out needUpdateEntity, out doesNotSupportSharePointGroup);
                            needManualDelete = result.Item1;
                            needDeleteDataAndIndex.AddRange(needManualDelete);
                            retentionResult.manualSkippedItem.AddRange(result.Item2);
                            if(doesNotSupportSharePointGroup != null)
                            {
                                retentionResult.DoesNotSupportSharePointItem.AddRange(doesNotSupportSharePointGroup);
                            }
                        }
                        if (rule.RetentionInfo != null && !rule.RetentionInfo.IsManualApproval)
                        {
                            //remove all data from manual approve by rule Id
                            logger.Info("remove all data from manual approve by rule Id {0}", ruleId);
                            //AzureDBWorker.DeleteItemByRule(ArchiverMessage.ScheduledConfigs[0].SiteId, ruleId);
                        }
                        if (needDeleteDataAndIndex.Count > 0)
                        {
                            ArchiverLifecycleRetentionResult tempResult = RetainJobData(needDeleteDataAndIndex, removeStub);
                            logger.Info("Add success count {0} of Rule {1} to total", tempResult.sucessItem, ruleId);
                            retentionResult.sucessItem.AddRange(tempResult.sucessItem);
                            GenaratAndInsertRetentionSubInfos(tempResult.sucessItem, retentionInfo.JobId, retentionInfo.SiteGroupId, retentionInfo.SiteUrl, retentionInfo.SiteId);
                        }

                        if(needUpdateEntity != null && needUpdateEntity.Count > 0)
                        {
                            foreach (var entity in needUpdateEntity)
                            {
                                entity.ManualArchiveStatus = (int)AvePoint.RA.Contract.Schedule.ActionStatus.Archiverd;
                                CosmosBulkOperator.Instance.Add(entity);
                            }
                            HistoryAddAction _historyAction = new();
                            foreach (var recordInDB in needUpdateEntity)
                            {
                                var historyData = _historyAction.Convert(
                                    recordInDB,
                                    (SOApproveDBStatus)recordInDB.ManualApprovedStatus,
                                    recordInDB.ManualApprovedBy,
                                    recordInDB.ManualActionTime
                                );

                                _historyAction.Add(historyData).GetAwaiter().GetResult();
                            }
                        }
                    }
                    else
                    {
                        logger.Info("remove all data from manual approve by rule Id {0}", ruleId);
                        
                    }
                }

                retentionResult.HasIndexRelatedToBackupJob = IsExistsIndexRelatedToJob(this.archiverRetentionInfo.JobId);
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                if (isCosmosBulkOperationEnabled)
                {
                    CosmosBulkOperator.Instance.Complete();
                    CosmosBulkOperator.Instance.Reset();
                }
            }

            //to do
            return retentionResult;
        }
        private bool IsExistsIndexRelatedToJob(string jobId)
        {
            var parameters = new Dictionary<String, Object>();
            parameters["@JobId"] = $"%{jobId}%";
            var deleteBodyTable = $"SELECT COL_ID FROM {IndexConstants.TableNameArchiveBody} WHERE COL_POOL_GUID LIKE @JobId LIMIT 1;";
            var result = this.IndexMainProcessor.ExecuteScalar(deleteBodyTable, parameters);
            return result != null;
        }
        private void GenaratAndInsertRetentionSubInfos(List<ArchiverBasicIndex> IndexItem, string jobId,Guid siteGroupId,string siteUrl,string siteId)
        {
            List<string> jobIds = new List<string>();
            List<RetentionIndexSubInfo> result = new List<RetentionIndexSubInfo>();
            foreach (var item in IndexItem)
            {
                RetentionIndexSubInfo tempResult = new RetentionIndexSubInfo();
                tempResult.Id = Guid.NewGuid().ToString();
                tempResult.RetentionTime = item.ArchiveTime;
                tempResult.JobId = jobId;
                tempResult.ArchiverJobId = item.BackupJobId;
                tempResult.SiteGroupId = siteGroupId == Guid.Empty ? string.Empty : siteGroupId.ToString();
                tempResult.SiteURL = siteUrl;
                tempResult.SiteId = siteId;
                if (!jobIds.Contains(item.BackupJobId))
                {
                    jobIds.Add(item.BackupJobId);
                    result.Add(tempResult);
                }
            }
            logger.Info($"need insert retention result count is:{result.Count}");
            RetentionIndexSubInfoDao.InsertIntoRetentionIndexSubInfo(result);
        }
        private Tuple<List<ArchiverBasicIndex>, List<ArchiverBasicIndex>> ProcessManualApproveData(List<ArchiverBasicIndex> indexes, Rule rule, out List<Record> needUpdateEntity, out List<ArchiverBasicIndex> doesNotSupportSharePointGroup)
        {
            List<ArchiverBasicIndex> result = new List<ArchiverBasicIndex>();
            List<ArchiverBasicIndex> manualSkippedResult = new List<ArchiverBasicIndex>();
            doesNotSupportSharePointGroup = new List<ArchiverBasicIndex>();
            needUpdateEntity = new List<Record>();
            var sortIndexs = SortIndexById(indexes);

            foreach (var indexs in sortIndexs)
            {
                Guid recordId = Guid.Empty;
                Guid nodeId = Guid.Empty;
                ManualExportReportInfo manualApprovalReportInfo = null;
                try
                {
                    var currentIndex = FindCurrentVersion(indexs.Value);
                    if (currentIndex == null)
                    {
                        currentIndex = indexs.Value.FirstOrDefault();
                    }
                    var archiverentity = GenerateEntity(currentIndex, rule);
                    manualApprovalReportInfo = RMArchiverItemConverter.ConvertToReportInfo(archiverentity, true);
                    PerProcessManualApprovalReport(manualApprovalReportInfo);
                    if (manualApprovalReportInfo.SourceFlag == (int)SourceFlag.All)
                    {
                        manualApprovalReportInfo.SourceFlag = IsTeams ? (int)SourceFlag.Teams : IsOneDriveSite ? (int)SourceFlag.OneDrive : (int)SourceFlag.SharePoint;
                    }
                    nodeId = manualApprovalReportInfo.NodeID;
                    recordId = (mSiteId.ToString().ToLowerInvariant() + manualApprovalReportInfo.NodeID.ToString().ToLowerInvariant()).ToMd5();
                    var recordInDB = explorerDao.QueryAll(r => r.Id == recordId).FirstOrDefault();
                    //记录不存在，插入新的记录
                    if (recordInDB == null)
                    {
                        if (WrapperConfiguration.IsProcessApprovalDatasOnly)
                        {
                            logger.Info($"match manual rule, and record in DB is null,is IsProcessApprovalDatasOnly will not insert to cosmos.");
                        }
                        else
                        {
                            try
                            {
                                //ProcessWaitingForApprovalRecord(manualApprovalReportInfo);
                                logger.Info($"Item:{recordId} match manual rule, and record in DB is null.");
                                var record = BasicConvertReportToManualAprovalRecord(manualApprovalReportInfo);
                                record.RuleId = new Guid(rule.Id);
                                mManualAction.ProcessWaitingForApprovalRecordAsync(record).GetAwaiter().GetResult();
                                CosmosBulkOperator.Instance.Add(record);
                                manualSkippedResult.Add(currentIndex);
                            }
                            catch(Exception e)
                            {
                                if (e.Message == "RM_MA_DoesNotSupportSPGroupForRetention" && currentIndex != null)
                                {
                                    doesNotSupportSharePointGroup.Add(currentIndex);
                                }
                                throw;
                            }
                        }
                    }
                    else
                    {
                        //ManualRetentionStatus=1 说明是Retention数据
                        if (recordInDB.ManualRetentionStatus == 1)
                        {
                            if (recordInDB.ManualApprovedStatus == (int)SOArchiverAzureDBWorker.SOApproveDBStatus.Approved)
                            {
                                logger.Info($"Item:{recordId} match manual rule, and approve status is Approved.");
                                if (int.TryParse(keyValueDao.GetValueByKey(RMKeyValuesConstants.RecordsBackupOutputStreamLevel)?.Value, out var outputStreamLevel))
                                {
                                    if (outputStreamLevel == (int)OutputStreamLevel.DataBlockLevel)
                                    {
                                        logger.Warn($"this item Item:{recordId} match rule and approve status is Approved,but outputStreamLevel is DataBlockLevel,so skip delete");
                                    }
                                    else
                                    {
                                        needUpdateEntity.Add(recordInDB);
                                        result.AddRange(indexs.Value);
                                    }
                                }
                                else
                                {
                                    needUpdateEntity.Add(recordInDB);
                                    result.AddRange(indexs.Value);
                                }
                            }
                            else if (WrapperConfiguration.IsProcessApprovalDatasOnly)
                            {
                                logger.Info($"Item match manual rule, not approvaed ,and it is process ApprovalDatasOnly");
                            }
                            else if (recordInDB.ManualApprovedStatus == (int)SOApproveDBStatus.None)
                            {
                                logger.Info($"Item:{recordId} match manual rule, and approve status is none.");
                                recordInDB.RuleId = new Guid(rule.Id);
                                PerProcessRecordForManual(manualApprovalReportInfo, recordInDB);
                                mManualAction.ProcessWaitingForApprovalRecordAsync(recordInDB).GetAwaiter().GetResult();
                                CosmosBulkOperator.Instance.Add(recordInDB);
                                manualSkippedResult.Add(currentIndex);
                            }
                            else if (recordInDB.ManualApprovedStatus == (int)SOArchiverAzureDBWorker.SOApproveDBStatus.Rejected)
                            {
                                if (recordInDB.ManualExtendTime > DateTime.UtcNow.Ticks )
                                {
                                    logger.Info($"Item:{recordId} is disposal extensions itenName:{recordInDB.LeafName}");
                                    continue;
                                }
                                logger.Info($"Item:{recordId} match manual rule, and approve status is Rejected.");
                                mManualAction.ProcessApprovedOrRejectedRecord(recordInDB);
                                mManualAction.ProcessWaitingForApprovalRecordAsync(recordInDB);

                                CosmosBulkOperator.Instance.Add(recordInDB);
                                manualSkippedResult.Add(currentIndex);
                            }
                        }
                        //说明原来不是Retention 的manual记录，目前已经触发了Retention,覆盖原来的记录
                        else
                        {
                            if (WrapperConfiguration.IsProcessApprovalDatasOnly)
                            {
                                logger.Info("not retention manual records,and it is process approval datas only");
                            }
                            else
                            {
                                recordInDB.RuleId = new Guid(rule.Id);
                                recordInDB.ManualRetentionStatus = 1;
                                PerProcessRecordForManual(manualApprovalReportInfo, recordInDB);
                                mManualAction.ProcessWaitingForApprovalRecordAsync(recordInDB).GetAwaiter().GetResult();
                                CosmosBulkOperator.Instance.Add(recordInDB);
                                manualSkippedResult.Add(currentIndex);
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    logger.Error($"ProcessManualApproveData NodeId {nodeId} RecordId{recordId} Error: {e}");
                }
            }

            return new Tuple<List<ArchiverBasicIndex>, List<ArchiverBasicIndex>>(result, manualSkippedResult);
        }

        private ArchiverBasicIndex FindCurrentVersion(List<ArchiverBasicIndex> indexs)
        {
            ArchiverBasicIndex currentVersion = null;
            foreach (var index in indexs)
            {
                if (!index.Name.Contains(":"))
                {
                    currentVersion = index;
                    break;
                }
            }

            return currentVersion;
        }

        private Dictionary<string, List<ArchiverBasicIndex>> SortIndexById(List<ArchiverBasicIndex> indexes)
        {
            Dictionary<string, List<ArchiverBasicIndex>> keyValues = new Dictionary<string, List<ArchiverBasicIndex>>();
            foreach (var index in indexes)
            {
                if (!keyValues.ContainsKey(index.NodeGuid))
                {
                    keyValues[index.NodeGuid] = new List<ArchiverBasicIndex>();
                }
                keyValues[index.NodeGuid].Add(index);
            }
            return keyValues;
        }

        private void PerProcessRecordForManual(ManualExportReportInfo manualApprovalReportInfo, Record record)
        {
            if (manualApprovalReportInfo != null) 
            {
                record.ManualActionTime = DateTime.UtcNow.Ticks;
                record.ManualApprovedBy = 0;
                record.ManualApprovedStatus = (int)SOApproveDBStatus.WaitingApprove;
                record.ManualArchiveStatus = (int)AvePoint.RA.Contract.Schedule.ActionStatus.None;
                record.ManualFullPath = manualApprovalReportInfo.Path;
                record.ManualSiteUrl = manualApprovalReportInfo.SiteUrl;
                record.ManualFolderPath = GetManualFolderPath(manualApprovalReportInfo);
                record.ManualEscalateFrom = 0;
                record.ManualEscalatedComment = "";
                record.ManualExtendTime = 0;
                record.ManualExtendComment = "";
                record.ManualCollectionTime = DateTime.UtcNow.Ticks;
                record.ManualArchivedTime = 0;
                record.ManualPartitionKey = manualApprovalReportInfo.PartKey;
                record.ManualRowKey = manualApprovalReportInfo.RowKey;
                record.ManualVersion = GetVersion(manualApprovalReportInfo.UIVersion);
                record.ManualIsRelatedRecords = manualApprovalReportInfo.HasRelatedDocument > 0;
                record.ManualRelatedRecordsAction = manualApprovalReportInfo.DeleteRelatedRecords;
                record.ManualEmailNotificationCount = 0;
                record.ManualEmailNotificationLastTime = DateTime.UtcNow.Ticks;
                record.ManualExtendCount = 0;
                record.ManualIsAutoReassigned = false;
                record.ManualRetentionStatus = manualApprovalReportInfo.RetentionStatus;
                record.ManualLastApproveRejectComment = string.Empty;
                record.ManualLastReviewedBy = string.Empty;
                record.ManualLastlReviewTime = 0;
                record.ManualModifiedTime = manualApprovalReportInfo.ModifiedTime;
            }
        }

        private Rule GetRule(string ruleId)
        {
            Rule rule = Rules.Values.FirstOrDefault(a => a.Id == ruleId);
            if (this.IsOneDriveSite && rule != null)
            {
                if (rule.OneDriveRule != null)
                {
                    rule.OneDriveRule.Id = ruleId; 
                }
                return rule.OneDriveRule;
            }
            return rule;
        }

        protected ManualApprovalSettingModel GetManualApprovalSettingInfo(ManualExportReportInfo manualApprovalReportInfo, ManualApprovalRuleModel ruleInfo)
        {
            var model = new ManualApprovalSettingModel();
            if (ruleInfo.RetentionInfo != null)
            {
                model.IsSendEmialToOwner = ruleInfo.RetentionInfo.IsSendEamilToOwner;
                model.ManualApprovalType = ruleInfo.RetentionInfo.ReviewType == AvePoint.GCommon.Contract.StorageOptimization.Object.ReviewType.RecordOwner ? AvePoint.RA.DB.Model.ApprovalType.RecordOwners : AvePoint.RA.DB.Model.ApprovalType.ApprovalProcess;
                model.Owners = ruleInfo.RetentionInfo.UserInfos;
                model.WorkflowId = ruleInfo.RetentionInfo.WorkflowId;
            }
            return model;
        }
        protected void ProcessManualApprovalReportByOwner(ManualExportReportInfo manualApprovalReport, ManualApprovalRuleModel ruleInfo)
        {
            if (ruleInfo.Owners.Count == 0)
            {
                logger.Error($"The current manual approval report onwers is not set. PartKey: [{manualApprovalReport.PartKey}], RowKey: [{manualApprovalReport.RowKey}].");
                return;
            }

            var manualApprovalRecord = BasicConvertReportToManualAprovalRecord(manualApprovalReport, ruleInfo);
            var ownerIds = ManualApprovalOwnerManager.GetOwnerIds(ruleInfo.Owners);

            manualApprovalRecord.ManualWorkflowInstanceId = Guid.Empty;
            manualApprovalRecord.ManualInternalApprovedStatus = (int)SOApproveDBStatus.WaitingApprove;
            manualApprovalRecord.ManualReviewer = ownerIds.ToArray();

            if (!isCosmosBulkOperationEnabled)
            {
                explorerDao.Upsert(manualApprovalRecord);
            }
            else
            {
                CosmosBulkOperator.Instance.Add(manualApprovalRecord);
            }

            if (ruleInfo.IsSendEmailToOwner)
            {
                foreach (var owner in ruleInfo.Owners)
                {
                    _emailSender.Add(RMEmailTemplateId.MANUAL_APPROVAL, new RMManualEmailTemplateParameters
                    {
                        UserId = owner.UserId,
                        ToUser = owner.UserPrincipalName,
                        TemplateType = RMEmailTemplateType.Manual,
                        RequestComment = "" 
                    });
                }
            }
        }

        protected async System.Threading.Tasks.Task ProcessManualApprovalReportByWorkflowNewAsync(ManualExportReportInfo manualApprovalReport, Record manualApprovalRecord, AvePoint.RA.RACommonUtility.Workflow.RMWorkflowStep step, ManualApprovalRuleModel ruleInfo, bool usedSiteOwnerMode)
        {
            var reviewers = await step.GetReviewersAsync(manualApprovalRecord.ScopeId);
            var tempalteId = RMEmailTemplateId.MANUAL_APPROVAL;
            if (step.UsedEmailTemplateMode == RA.Contract.RMWeb.CP.RMWorkflowStepUsedEmailTemplateMode.Custom && step.CustomIntervalSettings[0] != null)
            {
                var customTemplateId = new Guid((step.CustomIntervalSettings[0]).UsedEmailTemplateId);
                if (customTemplateId != Guid.Empty)
                {
                    tempalteId = customTemplateId;
                }
            }
            manualApprovalRecord.ManualReviewer = reviewers.Select(item => item.RMUserId).ToArray();
            if (ruleInfo.IsSendEmailToOwner)
            {
                foreach (var reviewer in reviewers)
                {
                    _emailSender.Add(tempalteId, new RMManualEmailTemplateParameters
                    {
                        UserId = reviewer.UserId,
                        ToUser = reviewer.UserPrincipalName,
                        TemplateType = RMEmailTemplateType.Manual,
                        RequestComment = ""
                    });
                }
            }

            manualApprovalRecord.ManualInternalApprovedStatus = (int)SOApproveDBStatus.WorkflowInProgress;
            if (!isCosmosBulkOperationEnabled)
            {
                explorerDao.Upsert(manualApprovalRecord);
            }
            else
            {
                CosmosBulkOperator.Instance.Add(manualApprovalRecord);
            }
        }

        protected static void PerProcessManualApprovalReport(ManualExportReportInfo manualApprovalReport)
        {
            if (manualApprovalReport.ObjectLevel == AvePoint.RA.Contract.RMWeb.ReportCenter.RMReportObjectLevel.SiteCollection)
            {
                manualApprovalReport.ContentType = "RM_JS_Rule_ObjectLevel_SiteCollection";
            }
            else if (manualApprovalReport.ObjectLevel == AvePoint.RA.Contract.RMWeb.ReportCenter.RMReportObjectLevel.ExchangeOnlineItem)
            {
                manualApprovalReport.ContentType = "RM_JS_Rule_ObjectLevel_ExchangeOnlineItem";
            }
        }

        private long CalculateExpireDate(RetentionInfo info)
        {
            DateTime now = DateTime.UtcNow;
            if (info.Condition == GCommon.Contract.Server.Common.Profile.Object.TimeFilterCondition.OlderThan)
            {
                switch (info.KeepDateUnite)
                {
                    case GCommon.Contract.Server.Common.Profile.Object.TimeUnit.Day:
                        now = now.AddDays(0 - info.KeepDateNumber);
                        break;
                    case GCommon.Contract.Server.Common.Profile.Object.TimeUnit.Week:
                        now = now.AddDays(0 - (info.KeepDateNumber * 7));
                        break;
                    case GCommon.Contract.Server.Common.Profile.Object.TimeUnit.Month:
                        now = now.AddMonths(0 - info.KeepDateNumber);
                        break;
                    case GCommon.Contract.Server.Common.Profile.Object.TimeUnit.Year:
                        now = now.AddYears(0 - info.KeepDateNumber);
                        break;
                } 
            } 
            else if(info.Condition == GCommon.Contract.Server.Common.Profile.Object.TimeFilterCondition.Is)
            {
                return info.Date;
            }
            return now.Ticks;
        }

        public override void GenerateJobReport(Int32 jobState)
        {
            //this.logger.Info(MediaServiceArchiverBackupResource.RetentionServiceGenerateJobReportBegin, this.archiverRetentionInfo.JobId);
            //var jobDetailList = new List<JobDetail>();
            //var jobSummaryList = new List<JobSummary>();
            //try
            //{
            //    // generate archiver deletion detail and summary
            //}
            //catch (Exception ex)
            //{
            //    this.logger.Error(MediaServiceArchiverBackupResource.RetentionServiceGenerateJobReportError, this.archiverRetentionInfo.JobId, ex.ToString());
            //}
        }

        private RMManualArchiverSharePointOnlineTableEntity GenerateEntity(ArchiverBasicIndex index, Rule rule)
        {
            RMManualArchiverSharePointOnlineTableEntity entity = new RMManualArchiverSharePointOnlineTableEntity();
            try
            {
                entity.PartitionKey = this.mSiteId;
                entity.SourceFlag = (int)(IsTeams ? SourceFlag.Teams : IsOneDriveSite ? SourceFlag.OneDrive : SourceFlag.SharePoint);
                entity.RowKey = index.PathMD5 + "_" + index.ArchiveTime;
                logger.Info("The RowKey is: {0}, node id:{1}", entity.RowKey, index.PathMD5);
                entity.ScanJobID = mScanJobID;
                entity.NodeID = new Guid(index.NodeGuid);
                entity.ParentID = new Guid(index.PathMD5);
                //entity.UIVersion = reportNode.UIVersion;
                entity.CacheNodeType = 0;
                entity.ArchiveLevel = (int)PolicyLevel.Document;
                entity.Status = 1; //waiting for approve
                entity.ExportToRECO = false;
                entity.RuleID = new Guid(index.Retention);
                entity.ScopeID = new Guid(mSiteId);
                //entity.HasRelatedDocument = reportNode.HasRelatedDocument;
                //entity.DeleteRelatedRecords = reportNode.DeleteRelatedRecords;
                //entity.RelatedRecordInfo = reportNode.RelatedRecordInfo;
                #region Json Meta
                ArchiverSharePointDto spDataSource = new ArchiverSharePointDto();
                spDataSource.ScopeID = entity.ScopeID;
                spDataSource.ScanJobID = entity.ScanJobID;
                spDataSource.NodeID = entity.NodeID;
                //spDataSource.ParentID = entity.ParentID;
                //spDataSource.UIVersion = entity.UIVersion;
                //spDataSource.CacheNodeType = entity.CacheNodeType;
                spDataSource.ArchiveLevel = entity.ArchiveLevel;
                spDataSource.KeepDataStatus = 0;
                spDataSource.RuleID = entity.RuleID;

                spDataSource.LastModifiedTime = index.ModifyTime;
                spDataSource.LeafName = index.Name;
                spDataSource.Level = mNodeLevel;
                spDataSource.ExpireTime = CalculateExpire(index, rule); //calculate;
                spDataSource.Path = index.Url;
                spDataSource.Property = GetRootXml();
                //spDataSource.SPNodeLevel = reportNode.SPNodeLevel;
                spDataSource.ScanItemID = 0;
                spDataSource.ScanTime = DateTime.UtcNow;
                spDataSource.SiteUrl = index.SitePath;
                spDataSource.SiteId = new Guid(this.mSiteId);
                spDataSource.RegistedSiteId = new Guid(this.mSiteId);
                //spDataSource.WebId = index.WebID;
                //spDataSource.Metadata = reportNode.Metadata ?? string.Empty;
                spDataSource.ArchivedTime = new DateTime(index.ArchiveTime);
                spDataSource.SiteGroupId = mSiteGroupId;
                spDataSource.SiteTitle = index.SitePath;
                spDataSource.SourceFlag = (int)(IsTeams ? SourceFlag.Teams : IsOneDriveSite ? SourceFlag.OneDrive : SourceFlag.SharePoint);
                #endregion
                string jsonMeta = JsonConvert.SerializeObject(spDataSource);
                entity.JsonMeta = jsonMeta;

            }
            catch (Exception ex)
            {
                logger.Error(string.Format("Error in generate entity {0}, reason : {1}.", "", ex.ToString()));
                throw;
            }
            return entity;
        }

        private DateTime CalculateExpire(ArchiverBasicIndex index, Rule rule)
        {
            if (rule.RetentionInfo.Condition == GCommon.Contract.Server.Common.Profile.Object.TimeFilterCondition.OlderThan)
            {
                DateTime archiverTime = new DateTime(index.ArchiveTime);
                switch (rule.RetentionInfo.KeepDateUnite)
                {
                    case GCommon.Contract.Server.Common.Profile.Object.TimeUnit.Day:
                        archiverTime = archiverTime.AddDays(rule.RetentionInfo.KeepDateNumber);
                        break;
                    case GCommon.Contract.Server.Common.Profile.Object.TimeUnit.Week:
                        archiverTime = archiverTime.AddDays(rule.RetentionInfo.KeepDateNumber * 7);
                        break;
                    case GCommon.Contract.Server.Common.Profile.Object.TimeUnit.Month:
                        archiverTime = archiverTime.AddMonths(rule.RetentionInfo.KeepDateNumber);
                        break;
                    case GCommon.Contract.Server.Common.Profile.Object.TimeUnit.Year:
                        archiverTime = archiverTime.AddYears(rule.RetentionInfo.KeepDateNumber);
                        break;
                }
                return archiverTime;
            }
            else if (rule.RetentionInfo.Condition == GCommon.Contract.Server.Common.Profile.Object.TimeFilterCondition.Is)
            {
                return new DateTime(rule.RetentionInfo.Date, DateTimeKind.Utc);
            }
            return DateTime.MaxValue;
        }

        private string GetRootXml()
        {
            string rootXml = "<SOArchive/>";
            return rootXml;
        }

        protected Record BasicConvertReportToManualAprovalRecord(ManualExportReportInfo manualApparovalReport)
        {
            using (new PerformanceScope("ManualApproval:GetItem", "", true))
            {
                var basicRecord = new Record();
                basicRecord = ConvertReportToManualApprovalRecord(manualApparovalReport, basicRecord);

                var record = new Record
                {
                    Id = basicRecord.Id,
                    RecordStatus = (int)RMRecordStatus.ManualPreSync
                };

                record.ScopeId = basicRecord.ScopeId;

                record.ManualRelatedRecords = basicRecord.ManualRelatedRecords;
                record.NodeId = manualApparovalReport.NodeID;
                record.IsManualSynced = true;
                record.LeafName = manualApparovalReport.LeafName;
                record.NodeType = ConvertObjectLevelToNodeLevel(manualApparovalReport.ObjectLevel);
                record.ExtensionForFile = GetFileExtension(manualApparovalReport, record);
                record.SourceFlag = manualApparovalReport.SourceFlag; 
                record.ManualActionTime = DateTime.UtcNow.Ticks;
                record.ManualApprovedBy = 0;
                record.ManualApprovedStatus = (int)SOApproveDBStatus.WaitingApprove;
                record.ManualArchiveStatus = (int)AvePoint.RA.Contract.Schedule.ActionStatus.None;
                record.ManualFullPath = manualApparovalReport.Path;
                record.ManualSiteUrl = manualApparovalReport.SiteUrl;
                record.ManualFolderPath = GetManualFolderPath(manualApparovalReport);
                record.ManualEscalateFrom = 0;
                record.ManualEscalatedComment = "";
                record.ManualExtendTime = 0;
                record.ManualExtendComment = "";
                record.ManualCollectionTime = DateTime.UtcNow.Ticks;
                record.ManualArchivedTime = 0;
                record.ManualPartitionKey = manualApparovalReport.PartKey;
                record.ManualRowKey = manualApparovalReport.RowKey;
                record.ManualVersion = GetVersion(manualApparovalReport.UIVersion);
                record.ManualIsRelatedRecords = manualApparovalReport.HasRelatedDocument > 0;
                record.ManualRelatedRecordsAction = manualApparovalReport.DeleteRelatedRecords;
                record.ManualEmailNotificationCount = 0;
                record.ManualEmailNotificationLastTime = DateTime.UtcNow.Ticks;
                record.ManualExtendCount = 0;
                record.ManualIsAutoReassigned = false;
                record.ManualRetentionStatus = manualApparovalReport.RetentionStatus;
                record.ManualLastApproveRejectComment = string.Empty;
                record.ManualLastReviewedBy = string.Empty;
                record.ManualLastlReviewTime = 0;
                record.ManualModifiedTime = manualApparovalReport.ModifiedTime;
                if (string.IsNullOrEmpty(record.CreatedBy))
                {
                    record.CreatedBy = manualApparovalReport.CreatedBy;
                }

                if (string.IsNullOrEmpty(record.ModifiedBy))
                {
                    record.ModifiedBy = manualApparovalReport.ModifiedBy;
                }

                return record;
            }
        }

        protected Record BasicConvertReportToManualAprovalRecord(ManualExportReportInfo manualApparovalReport, ManualApprovalRuleModel ruleInfo)
        {  // No use
            using (new PerformanceScope("ManualApproval:GetItem", "", true))
            {
                var basicRecord = new Record();
                basicRecord = ConvertReportToManualApprovalRecord(manualApparovalReport, basicRecord);

                var record = new Record
                {
                    Id = basicRecord.Id,
                    RecordStatus = (int)RMRecordStatus.ManualPreSync
                };


                record.ScopeId = basicRecord.ScopeId;

                record.ManualRelatedRecords = basicRecord.ManualRelatedRecords;
                record.NodeId = manualApparovalReport.NodeID;
                record.IsManualSynced = true;
                record.LeafName = manualApparovalReport.LeafName;
                record.NodeType = ConvertObjectLevelToNodeLevel(manualApparovalReport.ObjectLevel);
                record.RuleId = new Guid(ruleInfo.RuleId);
                record.ManualRuleName = ruleInfo.RuleName;
                record.ManualRuleCriteria = ruleInfo.RuleCriterias;
                record.ManualRuleDisposalClass = ruleInfo.RuleDisposalClass;
                record.ExtensionForFile = GetFileExtension(manualApparovalReport, record);
                record.SourceFlag = (int)GetInnerRuleFlag(manualApparovalReport); //(int)Flag; //todo
                record.ManualActionTime = DateTime.UtcNow.Ticks;
                record.ManualApprovedBy = 0;
                record.ManualApprovedStatus = (int)SOApproveDBStatus.WaitingApprove;
                record.ManualArchiveStatus = (int)AvePoint.RA.Contract.Schedule.ActionStatus.None;
                record.ManualFullPath = manualApparovalReport.Path;
                record.ManualSiteUrl = manualApparovalReport.SiteUrl;
                record.ManualFolderPath =  GetManualFolderPath(manualApparovalReport);
                record.ManualEscalateFrom = 0;
                record.ManualEscalatedComment = "";
                record.ManualExtendTime = 0;
                record.ManualExtendComment = "";
                record.ManualCollectionTime = DateTime.UtcNow.Ticks;
                record.ManualArchivedTime = 0;
                record.ManualPartitionKey = manualApparovalReport.PartKey;
                record.ManualRowKey = manualApparovalReport.RowKey;
                record.ManualVersion = GetVersion(manualApparovalReport.UIVersion);
                record.ManualIsRelatedRecords = manualApparovalReport.HasRelatedDocument > 0;
                record.ManualRelatedRecordsAction = manualApparovalReport.DeleteRelatedRecords;
                record.ManualNeedEmailNotification = ruleInfo.IsSendEmailToOwner;
                record.ManualEmailNotificationCount = 0;
                record.ManualEmailNotificationLastTime = DateTime.UtcNow.Ticks;
                record.ManualExtendCount = 0;
                record.ManualIsAutoReassigned = false;
                record.ManualRetentionStatus = manualApparovalReport.RetentionStatus;
                record.ManualWorkflowDefinitionId = ruleInfo.WorkflowId == null ? Guid.Empty : Guid.Parse(ruleInfo.WorkflowId);
                if (string.IsNullOrEmpty(record.CreatedBy))
                {
                    record.CreatedBy = manualApparovalReport.CreatedBy;
                }

                if (string.IsNullOrEmpty(record.ModifiedBy))
                {
                    record.ModifiedBy = manualApparovalReport.ModifiedBy;
                }

                return record;
            }
        }

        protected SourceFlag GetInnerRuleFlag(ManualExportReportInfo reportInfo)
        {
            if (reportInfo.JsonMeta != null)
            {
                try
                {
                    ArchiverSharePointDto dto = JsonConvert.DeserializeObject<ArchiverSharePointDto>(reportInfo.JsonMeta);
                    return (SourceFlag)dto.SourceFlag;
                }
                catch(Exception e)
                {
                    logger.Warn(@$"fail get Inner Rulr Flag, ex:{e}");
                }
            }
            return SourceFlag.LifecycleRetention;
        }

        private static string GetVersion(int uiversion)
        {
            var version = string.Empty;
            if (uiversion > 0)
            {
                int majorVers = uiversion / 512;
                int minorVers = uiversion % 512;
                version = string.Format("{0}.{1}", majorVers, minorVers);
            }
            return version;
        }

        private static string GetFileExtension(ManualExportReportInfo data, Record record)
        {
            if (!string.IsNullOrEmpty(record.ExtensionForFile))
            {
                return record.ExtensionForFile;
            }

            switch ((RMNodeLevel)record.NodeType)
            {
                case RMNodeLevel.ExchangeOnlineItem:
                    return "msg";
                case RMNodeLevel.Item:
                    if (data.ArchiveLevel == (int)CacheNodeType.Item)
                    {
                        return "RM_RDM_RecordDetails_DataType_SPItem";
                    }
                    var ext = Path.GetExtension(data.LeafName);
                    return ext.Contains('.', StringComparison.CurrentCulture) ? ext[1..] : "RM_RDM_RecordDetails_DataType_FileNull";
                case RMNodeLevel.SiteCollection:
                    return "RM_JS_Rule_ObjectLevel_SiteCollection";
                case RMNodeLevel.Site:
                    return "RM_JS_Rule_ObjectLevel_Site";
                case RMNodeLevel.List:
                    return "RM_Common_ObjectLevel_List";
                case RMNodeLevel.Folder:
                    return "RM_Common_ObjectLevel_Folder";
                case RMNodeLevel.FSFolder:
                    return "RM_RDM_RecordDetails_DataType_FSFolder";
                case RMNodeLevel.FSFile:
                    var fsExt = Path.GetExtension(data.LeafName);
                    if (fsExt.Contains('.', StringComparison.CurrentCulture))
                    {
                        return fsExt[1..];
                    }
                    return "";
                case RMNodeLevel.PhysicalBox:
                    return "RM_PRM_PRE_Filter_PhysicalBox";
                case RMNodeLevel.PhysicalFile:
                    return "RM_PRM_PRE_Filter_PhysicalFile";
                case RMNodeLevel.PhysicalRecord:
                    return "RM_PRM_PRE_Filter_PhysicalRecord";
                case RMNodeLevel.PhysicalCustom:
                    return "RM_PRM_PRE_TableItemType_Container";
                case RMNodeLevel.CustomizeConnectorItem:
                    return "RM_Connector_ItemLevel_Item";
            }

            return "";
        }
        protected Record ConvertReportToManualApprovalRecord(ManualExportReportInfo manualApprovalReportInfo, Record record)
        {
            var siteId = manualApprovalReportInfo.ScopeID;
            record.Id = (siteId.ToString().ToLowerInvariant() + manualApprovalReportInfo.NodeID.ToString().ToLowerInvariant()).ToMd5(); ;
            record.ScopeId = new Guid(siteId);
            record.NodeId = manualApprovalReportInfo.NodeID;
            return record;
        }
        private int ConvertObjectLevelToNodeLevel(RMReportObjectLevel objectLevel)
        {
            var nodeLevel = RMNodeLevel.Undefined;
            switch (objectLevel)
            {
                case RMReportObjectLevel.Item:
                    nodeLevel = RMNodeLevel.Item;
                    break;
                case RMReportObjectLevel.SiteCollection:
                    nodeLevel = RMNodeLevel.SiteCollection;
                    break;
                case RMReportObjectLevel.Site:
                    nodeLevel = RMNodeLevel.Site;
                    break;
                case RMReportObjectLevel.List:
                    nodeLevel = RMNodeLevel.List;
                    break;
                case RMReportObjectLevel.Folder:
                    nodeLevel = RMNodeLevel.Folder;
                    break;
                case RMReportObjectLevel.PhyBox:
                case RMReportObjectLevel.PhysicalBox:
                    nodeLevel = RMNodeLevel.PhysicalBox;
                    break;
                case RMReportObjectLevel.PhyCustom:
                    nodeLevel = RMNodeLevel.PhysicalCustom;
                    break;
                case RMReportObjectLevel.PhyFolder:
                    break;
                case RMReportObjectLevel.PhyRecord:
                case RMReportObjectLevel.PhysicalRecord:
                    nodeLevel = RMNodeLevel.PhysicalRecord;
                    break;
                case RMReportObjectLevel.PhysicalFile:
                    nodeLevel = RMNodeLevel.PhysicalFile;
                    break;
                case RMReportObjectLevel.ExchangeOnlineItem:
                    nodeLevel = RMNodeLevel.ExchangeOnlineItem;
                    break;
                case RMReportObjectLevel.FSFolder:
                    nodeLevel = RMNodeLevel.FSFolder;
                    break;
                case RMReportObjectLevel.FSFile:
                    nodeLevel = RMNodeLevel.FSFile;
                    break;
                case RMReportObjectLevel.CustomizeConnectorItem:
                    nodeLevel = RMNodeLevel.CustomizeConnectorItem;
                    break;
            }

            return (int)nodeLevel;
        }

        public override void ProcessException(Exception e, ArchiverLifecycleRetentionResult result)
        {
            e = e.InnerException ?? e;
            this.logger.Error(e.Message, e); 
        }

        public override void Dispose()
        {
            this.UploadIndexToRealSystem();
            if (this.IndexService != null && this.archiverRetentionInfo.RetentionRule.Equals(DomainModel.RetentionRule.RetainArchiverJobData))
            {
                this.IndexService.Close();
            }
           
            if (this.DeviceManager != null)
            {
                this.DeviceManager.Close(this.indexLogicalDevice);
                this.DeviceManager.Close(this.dataLogicalDevice); 
            } 
        }

        private ArchiverLifecycleRetentionResult RetainJobData(List<ArchiverBasicIndex> indexes, bool removeStub)
        {
            this.logger.Info("Start to retain data for Records lifecycle, job id {0}", this.archiverRetentionInfo.JobId);
            ArchiverLifecycleRetentionResult result = this.ConvertInfoToResult(this.archiverRetentionInfo);
            List<ArchiverBasicIndex> failedOnes = this.DeleteDataFromDevice(indexes, this.dataVolume, this.indexVolume, removeStub);
            result.failedItem = failedOnes;
            result.sucessItem = indexes.Where(a => !failedOnes.Any(f => f.Id == a.Id)).ToList();
            this.logger.Info("Finish RetainJobData, success count {0}, failed count {1}", result.sucessItem.Count, result.failedItem.Count);
            return result;
        }

        private Int64 DeleteFileByStorageInfo(List<String> storageInfoList)
        {
            //如果有一个storage info删除失败，则认为整个retention失败
            var deleteResult = new StorageDeleteResult();
            var result = new StorageDeleteResult();
            Boolean isDeleteSucceed = false; 
            Int32 totalRetentionTimes = storageInfoList.Count;
            foreach (String storageInfo in storageInfoList)
            {
                var info = new StorageInfo
                {
                    ExtraStorageInfo = storageInfo,
                };
                try
                {
                    result = this.dataLogicalDevice.DeleteFile(info);
                    isDeleteSucceed = true;
                    this.logger.Debug(MediaServiceArchiverBackupResource.RetentionServiceDeleteFileByStorageInfoInfo, storageInfo); 
                }
                catch (Exception ex)
                {
                    if (!isDeleteSucceed)
                    {
                        this.ErrorMessage = ex.Message; 
                        this.logger.Error(MediaServiceArchiverBackupResource.RetentionServiceDeleteDataFromDeviceError, info.LowName, ex.ToString());
                        throw;
                    }
                    else
                    { 
                        this.logger.Warn(MediaServiceArchiverBackupResource.RetentionServiceDeleteFileByStorageInfoWarn, storageInfo, ex.ToString());
                    }
                }
                deleteResult.DeletedFileSize += result.DeletedFileSize;
            }
            return deleteResult.DeletedFileSize;
        }
         
         
           
       /* private long GetContentFileNumber(String name)
        {
            return Convert.ToInt64(name.Substring(name.LastIndexOf("_", StringComparison.OrdinalIgnoreCase) + 1, name.LastIndexOf(".", StringComparison.OrdinalIgnoreCase) - name.LastIndexOf("_", StringComparison.OrdinalIgnoreCase) - 1));
        }*/
          
        private ArchiverLifecycleRetentionResult ConvertInfoToResult(ArchiverRetentionInfo info)
        {
            var result = new ArchiverLifecycleRetentionResult();
            result.FarmName = info.FarmName;
            result.JobId = info.JobId;
            result.SiteUrl = info.SiteUrl;
            result.ArchiverBackupTime = info.ArchiverBackupTime;
            result.StoragePolicyId = info.StoragePolicyId;
            result.MediaService = info.MediaService;
            result.RetentionAction = info.RetentionAction;
            result.RetentionJob = info.RetentionJob;
            result.DestinationPhysicalDeviceId = info.DestinationPhysicalDeviceId;
            result.DataLogicalDevice = info.DataLogicalDevice;
            result.IndexLogicalDevice = info.IndexLogicalDevice;
            result.IsDeleteJob = info.IsDeleteJob;
            return result;
        }

   /*     private ArchiverPruningJob ConvertToArchiverPruningJob(ArchiverRetentionInfo archiverRetentionInfo)
        {
            ArchiverPruningJob pruningJob = new ArchiverPruningJob();
            pruningJob.FarmName = archiverRetentionInfo.FarmName;
            pruningJob.JobId = archiverRetentionInfo.JobId;
            pruningJob.SiteUrl = archiverRetentionInfo.SiteUrl;
            pruningJob.WebApp = archiverRetentionInfo.WebApp;
            pruningJob.ArchiverBackupTime = archiverRetentionInfo.ArchiverBackupTime;
            pruningJob.StoragePolicyId = archiverRetentionInfo.StoragePolicyId;
            pruningJob.MediaService = archiverRetentionInfo.MediaService;
            pruningJob.RetentionAction = archiverRetentionInfo.RetentionAction;
            pruningJob.RetentionJob = archiverRetentionInfo.RetentionJob;
            pruningJob.DestinationPhysicalDeviceId = archiverRetentionInfo.DestinationPhysicalDeviceId;
            pruningJob.DataLogicalDevice = archiverRetentionInfo.DataLogicalDevice;
            pruningJob.IndexLogicalDevice = archiverRetentionInfo.IndexLogicalDevice;
            pruningJob.IsDeleteJob = archiverRetentionInfo.IsDeleteJob;
            pruningJob.DestinationDevice = archiverRetentionInfo.DestinationDevice; 
            pruningJob.RetentionTimeSpanSeconds = archiverRetentionInfo.RetentionTimeSpanSeconds; 
            return pruningJob;
        }*/

        private Dictionary<string, HashSet<string>> deletingDuplicatedFiles = new Dictionary<string, HashSet<string>>();
        private HashSet<string> RecordDeletingDuplicatedFiles(ArchiverBasicIndex item)
        {
            if (item.DuplicateStatus > 0)
            {
                if(!deletingDuplicatedFiles.TryGetValue(item.StorageCrc64, out var duplicatedFileIDs))
                {
                    duplicatedFileIDs = new HashSet<string>();
                    deletingDuplicatedFiles[item.StorageCrc64] = duplicatedFileIDs;
                }
                duplicatedFileIDs.Add(item.Id);
                return duplicatedFileIDs;
            }

            return new HashSet<string>();
        }

        private bool CheckIsLastDuplicatedFileWithSameCRC(ArchiverBasicIndex item, HashSet<string> deletingFileIDs)
        {
            var sql = $"SELECT * FROM {IndexConstants.TableNameArchiveBody} WHERE COL_EXTENSION_8 = @CRC;";
            var dupFiles = this.IndexMainProcessor.ExecuteQuery<ArchiverBasicIndex>(sql, new Dictionary<string, object>() { { "@CRC", item.StorageCrc64 } });
            var refsCount = dupFiles.Count(f => !deletingFileIDs.Contains(f.Id) && f.DuplicateStatus > 0);
            return refsCount == 0;
        }

        private void RealDeleteDeuplicateFileDataFromDevice(
            ArchiverBasicIndex item, Dictionary<string, long> deleteSizePairs, List<string> needUpdateSiteForDashBoard,
            List<ArchiverBasicIndex> sucessOnes, List<ArchiverBasicIndex> failedOnes)
        {
            logger.Info($"This is dedup file data: {item.Id}. Source File: {item.DedupSourceFileId}");
            var deletingFileIDs = RecordDeletingDuplicatedFiles(item);
            if (!CheckIsLastDuplicatedFileWithSameCRC(item, deletingFileIDs))
            {
                logger.Info($"Don't need remove source file data. Exists other duplicate file refs.");
                if (!needUpdateSiteForDashBoard.Contains(item.SitePath))
                {
                    needUpdateSiteForDashBoard.Add(item.SitePath);
                }
                sucessOnes.Add(item);
            }
            else
            {
                logger.Info($"Need remove source file data. Not exists duplicate file refs.");
                var subJobIdOfSouceFile = item.DedupSourceFileJobId;
                string highName = this.dataVolume;
                string lowName = subJobIdOfSouceFile + "_content_" + item.ContentDataFileNumber + ".dat";
                try
                {
                    var dedupDevice = GetDataLogicalDeviceByJobId(subJobIdOfSouceFile);
                    if (dedupDevice == null)
                    {
                        logger.Error($"Data logical device not found. {subJobIdOfSouceFile}.");
                        failedOnes.Add(item);
                        return;
                    }

                    var sourceFileInfo = XConvert.FromNames(highName, lowName);
                    StorageDeleteResult deleteDataResult = dedupDevice.DeleteFile(sourceFileInfo);
                    if (deleteDataResult.IsDeleted)
                    {
                        AddOrUpdate(deleteSizePairs, item.JobId, deleteDataResult.DeletedFileSize);
                        if (!needUpdateSiteForDashBoard.Contains(item.SitePath))
                        {
                            needUpdateSiteForDashBoard.Add(item.SitePath);
                        }
                        sucessOnes.Add(item);
                    }
                    else
                    {
                        logger.Error("Failed to delete content {0}, {1}", lowName, deleteDataResult.Message);
                        failedOnes.Add(item);
                    }
                }
                catch (Exception ex)
                {
                    logger.Error("Failed to delete content {0}, {1}", lowName, ex);
                    failedOnes.Add(item);
                }
            }
        }

        private List<ArchiverBasicIndex> DeleteDataFromDevice(List<ArchiverBasicIndex> indexes, String dataVolume, String indexVolume, bool removeStub)
        {
            logger.Info("Start to delete index and data from Records");
            var tempDeleteDataSize = default(Int64);
            List<ArchiverBasicIndex> sucessOnes = new List<ArchiverBasicIndex>();
            List<ArchiverBasicIndex> failedOnes = new List<ArchiverBasicIndex>(); 
            var groupedIndex = indexes.GroupBy(a => a.JobId);

            if (this.dataLogicalDevice.StorageInterfaceType.Equals(StorageInterfaceType.Object))
            {
                var storageInfoList = indexes.Where(a => a.StorageInfo != null && a.StorageInfo != String.Empty).Select(a => a.StorageInfo).ToList();
                logger.Info($"this retention storage interface type is Object and storageInfoList is :{storageInfoList?.Count}");
                tempDeleteDataSize = this.DeleteFileByStorageInfo(storageInfoList);
                foreach(var item in groupedIndex)
                {
                    string siteUrl = this.RetentionIndexService.GetSiteUrlFromMainIndex(null, item.Key);
                    if (removeStub)
                    {
                        string stubType = string.Empty;
                        logger.Info($"RemoveOrphanedStub is true,strat remove stub file.jobid is {item.Key}");
                        var stubUrlList = this.RetentionIndexService.FilterDocumentUrlForLifecycle(item.ToList(), item.Key, ref stubType);
                        RemoveStubFromSharePoint(stubUrlList, siteUrl, item.Key, stubType);
                        logger.Info("RemoveOrphanedStub is true,finish remove stub file.");
                    }
                    var retentionInfoList = this.RetentionIndexService.GetDeletedDataFromMainIndexByPathMD5(item.Key, item.Select(a=>a.PathMD5).ToList(), siteUrl);
                    logger.Info("Begin Deleted Data From Main Index By PathMD5");
                    this.RetentionIndexService.DeletedDataFromMainIndexByPathMD5(item.Key, item.Select(a=>a.PathMD5).ToList());
                    UpdateRetentionInfo(retentionInfoList);
                    logger.Info("Finish Deleted Data From Main Index By PathMD5");
                }
            }
            else
            {
                Dictionary<string,long> deleteSizePairs = new Dictionary<string, long>();
                List<string> needUpdateSiteForDashBoard = new List<string>();
                foreach (var item in indexes)
                {
                    if (item.DuplicateStatus > 0)
                    {
                        RealDeleteDeuplicateFileDataFromDevice(item, deleteSizePairs, needUpdateSiteForDashBoard, sucessOnes, failedOnes);
                        continue;
                    }
                    string name = item.JobId + "_content_" + item.ContentDataFileNumber + ".dat";
                    string highName = this.dataVolume;
                    logger.Info($"{highName}, {name}");
                    var info = XConvert.FromNames(highName, name);
                    try
                    {
                        //info.Length = this.dataLogicalDevice.OpenFile(info).FileSize;//for cloud
                        var itemStorageInfo = this.dataLogicalDevice.OpenFile(info);
                        logger.Info("Start to delete data {0}", item.Name);
                        if (itemStorageInfo!= null && itemStorageInfo.Exists)
                        {
                            StorageDeleteResult deleteDataResult = this.dataLogicalDevice.DeleteFile(info);
                            if (deleteDataResult.IsDeleted)
                            {
                                AddOrUpdate(deleteSizePairs, item.JobId, itemStorageInfo.Length);//change for Google GCP , for performance consideration, reduce the API trigger counts. Also work for other device.
                                if (!needUpdateSiteForDashBoard.Contains(item.SitePath))
                                {
                                    needUpdateSiteForDashBoard.Add(item.SitePath);
                                }
                                sucessOnes.Add(item);
                            }
                            else
                            {
                                logger.Error("Failed to remove content {0}, {1}", name, deleteDataResult.Message);
                                failedOnes.Add(item);
                            }
                        }
                        else
                        {
                            logger.Error("File not Exists {0}", item.Name);
                            var xSystem = base.GetDataLogicalDeviceByJobId(item.JobId);
                            if(xSystem != null)
                            {
                                var itemJobStorageInfo = xSystem.OpenFile(info);
                                if (itemJobStorageInfo != null && itemJobStorageInfo.Exists)
                                {
                                    StorageDeleteResult deleteDataResult = xSystem.DeleteFile(info);
                                    if (deleteDataResult.IsDeleted)
                                    {
                                        AddOrUpdate(deleteSizePairs, item.JobId, itemJobStorageInfo.Length);
                                        if (!needUpdateSiteForDashBoard.Contains(item.SitePath))
                                        {
                                            needUpdateSiteForDashBoard.Add(item.SitePath);
                                        }
                                        sucessOnes.Add(item);
                                    }
                                    else
                                    {
                                        logger.Error("Failed to remove content {0}, {1}", name, deleteDataResult.Message);
                                        failedOnes.Add(item);
                                    }
                                }
                            }
                        }

                    }
                    catch (Exception ex)
                    {
                        logger.Error("Failed to remove content {0}, {1}", name, ex);
                        failedOnes.Add(item);
                    }
                }
                UpdateJobSubInfo(deleteSizePairs).GetAwaiter().GetResult();
                UpdateArchivedSiteInfo(needUpdateSiteForDashBoard);
                //var tempFileList = this.dataLogicalDevice.ListFiles(XConvert.FromNames(dataVolume, null));
                ////todo avoid list files
                //var fileList = tempFileList.FindAll(file => {
                //    string fileName = file.LowName;
                //    return indexes.Exists(a =>
                //    {
                //        string name = a.JobId + "_content_" + a.ContentDataFileNumber +".dat";
                //        if(fileName.Equals(name, StringComparison.OrdinalIgnoreCase))
                //        {
                //            return true;
                //        }
                //        if(fileName.StartsWith(a.JobId + "_content_" + a.ContentDataFileNumber + "_", StringComparison.OrdinalIgnoreCase) && fileName.EndsWith(".dat"))
                //        {
                //            return true;
                //        }
                //        return false;
                //    });
                //});
                //fileList.ForEach(item =>
                //{
                //    var info = XConvert.FromNames(item.HighName, item.Name);
                //    info.Length = this.dataLogicalDevice.OpenFile(info).FileSize;//for cloud
                //    try
                //    {
                //        logger.Info("Start to delete data {0}", item.Name);
                //        deleteDataResult = this.dataLogicalDevice.DeleteFile(info);

                //    }
                //    catch (Exception ex)
                //    {
                //        throw ex;
                //    } 
                //}); 
                if (sucessOnes.Count > 0)
                {
                    var sucessGroup = sucessOnes.GroupBy(a => a.JobId);
                    foreach (var item in sucessGroup)
                    {
                        int index = 0;
                        int page = 200;
                        List<ArchiverBasicIndex> temp = item.Skip(index * page).Take(page).ToList();
                        while (temp != null && temp.Count > 0)
                        {
                            index++;
                            try
                            {
                                string siteUrl = this.RetentionIndexService.GetSiteUrlFromMainIndex(null, item.Key);
                                if (removeStub)
                                {
                                    string stubType = string.Empty;
                                    logger.Info($"RemoveOrphanedStub is true,strat remove stub file.jobid is {item.Key}");
                                    var stubUrlList = this.RetentionIndexService.FilterDocumentUrlForLifecycle(item.ToList(), item.Key, ref stubType);
                                    RemoveStubFromSharePoint(stubUrlList, siteUrl, item.Key, stubType);
                                    logger.Info("RemoveOrphanedStub is true,finish remove stub file.");
                                }
                                var retentionInfoList = this.RetentionIndexService.GetDeletedDataFromMainIndexByPathMD5(item.Key, temp.Select(a => a.PathMD5).ToList(), siteUrl);
                                logger.Info("Begin Deleted Data From Main Index By PathMD5");
                                this.RetentionIndexService.DeletedDataFromMainIndexByPathMD5(item.Key, temp.Select(a => a.PathMD5).ToList());
                                UpdateRetentionInfo(retentionInfoList);
                                logger.Info("Finish Deleted Data From Main Index By PathMD5");
                            }
                            catch (Exception ex)
                            {
                                logger.Error("failed to remove data from index, count {0}, message:{1}", temp.Count, ex);
                                failedOnes.AddRange(temp);
                            }
                            temp = item.Skip(index * page).Take(page).ToList();
                        } 
                    }
                }
            }
            return failedOnes;
        }

        private void UpdateRetentionInfo(List<KeyValuePair<string, long>> retentionInfoList)
        {
            foreach (var info in retentionInfoList)
            {
                logger.Info($"Retention file info:{info.Value}, site URL: {this.archiverRetentionInfo.SiteUrl}, list URL:{info.Key}, archiver job: {this.archiverRetentionInfo.JobId}");
                var retentionSiteInfo = new RMRetentionSiteInfo()
                {
                    Id = Guid.NewGuid().ToString(),
                    ListUrl = info.Key,
                    SiteUrl = this.archiverRetentionInfo.SiteUrl,
                    RetentionJobID = this.archiverRetentionInfo.RetentionJob.Id,
                    FileNumber = info.Value
                };
                ArchiveSiteInfoDao.SaveRetentionSiteInfo(retentionSiteInfo);
            }
        }
        private void UpdateArchivedInfo(string siteUrl)
        {
            var siteUrlAndJobIdMapping = ArchiverSiteMasterIndexDao.GetAllBackupSiteCollectionDistinctJobIdMappings(new List<string>() { siteUrl });
            var siteUrlAndSizeMapping = ArchiverIndexSubInfoDao.GetAllArchiverIndexSubInfoBySiteUrls(siteUrlAndJobIdMapping);
            var o365TenantId = RemoteNodeDao.GetRemoteSiteCollectionByUrl(siteUrl)?.TenantId;
            ArchiveSiteInfoDao.UpdateArchiverInfo(siteUrl, GetFileCount(), GetFileVersionCount(), o365TenantId, siteUrlAndSizeMapping[siteUrl]);
        }
        private long GetFileCount()
        {
            var result = Convert.ToInt64(this.IndexMainProcessor.ExecuteScalar(SelectTableNameArchiverBodyFileCount, null));
            logger.Info($"file count is:{result}");
            return result;
        }
        private long GetFileVersionCount()
        {
            var result = Convert.ToInt64(this.IndexMainProcessor.ExecuteScalar(SelectTableNameArchiverBodyVersionCount, null));
            logger.Info($"file version count is:{result}");
            return result;
        }
        private void UpdateArchivedSiteInfo(List<string> siteUrl)
        {
            foreach (var temp in siteUrl)
            {
                UpdateArchivedInfo(temp);
            }
        }
        //private void AppendDataSize(List<ArchiverBasicIndex> index, string fileName, long dataSize)
        //{
        //    foreach(var temp in index)
        //    { 
        //        string name = temp.JobId + "_content_" + temp.ContentDataFileNumber ;
        //        if(name == fileName)
        //        {
        //            temp.ContentLength
        //        }
        //    }
        //}
        private void AddOrUpdate(Dictionary<string, long> dictionary, string key, long valueToAdd)
        {
            if (dictionary.TryGetValue(key, out long currentValue))
            {
                dictionary[key] = currentValue + valueToAdd;
            }
            else
            {
                dictionary[key] = valueToAdd;
            }
        }
        private async Task UpdateJobSubInfo(Dictionary<string, long> dictionary)
        {
            foreach (var temp in dictionary)
            {
                RARetentionJobTelemetry telemetry = await BuildRARetentionJobTelemetry(temp.Key);
                await ArchiverIndexSubInfoDao.UpdateArchiverIndexSubInfoMediaSizeAsync(temp.Key, temp.Value);
                await EndAndSendRetentionJobTelelmetry(telemetry);
            }
        }

        private async Task<RARetentionJobTelemetry> BuildRARetentionJobTelemetry(string objectSubSubJobId)
        {
            RARetentionJobTelemetry telemetry = new RARetentionJobTelemetry();
            try
            {
                telemetry.JobId = archiverRetentionInfo.JobId;
                telemetry.MainJobId = archiverRetentionInfo?.JobId?.Split("_")?.FirstOrDefault();
                telemetry.RetentionObject = archiverRetentionInfo.SiteUrl;
                telemetry.ArchivedSubJobId = objectSubSubJobId;
                telemetry.JobType = (archiverRetentionInfo as ArchiverLifecycleRetentionInfo)?.JobType.ToString() ?? string.Empty;
                telemetry.StorageName = archiverRetentionInfo?.DataLogicalDevice?.Name;
                telemetry.RetentionAction = (int)archiverRetentionInfo.RetentionAction;
                ArchiverIndexSubInfo indexSubInfo = await ArchiverIndexSubInfoDao.GetSubInfoBySubsubJobIdAsync(objectSubSubJobId);
                telemetry.MediaDataSize = indexSubInfo?.MediaDataSize ?? 0;
            }
            catch (Exception e)
            {
                logger.Error("Fail build telemetry object :{0}", e.ToString());
            }
            return telemetry;
        }

        private async Task EndAndSendRetentionJobTelelmetry(RARetentionJobTelemetry telemetry)
        {
            try
            {
                ArchiverIndexSubInfo indexSubInfo = await ArchiverIndexSubInfoDao.GetSubInfoBySubsubJobIdAsync(telemetry.ArchivedSubJobId);
                telemetry.RemainingMediaDataSize = indexSubInfo?.MediaDataSize ?? 0;
                telemetry.RetentionDataSize = telemetry.MediaDataSize - telemetry.RemainingMediaDataSize;
                await RATelemetryService.AddTelemetryForRetentionJob(telemetry);
            }
            catch (Exception e)
            {
                logger.Error(@$"Fail end and send telemetry, ex:{e}");
            }
        }



        private string GetWebServerServerRelativeUrl(string webUrl, IAveSite site)
        {
            if (webUrl.TrimEnd('/').Length == site.Url.TrimEnd('/').Length)
            {
                return string.Empty;
            }
            else
            {
                int hostLength = site.Url.Length - site.ServerRelativeUrl.Length;
                var result = webUrl.Substring(hostLength, webUrl.Length - hostLength);
                return result.Substring(result.IndexOf('/'));
            }
        }
        private void RemoveStubFromSharePoint(Dictionary<string, List<string>> docFullUrls, string siteUrl, string jobId, string stubType)
        {
            try
            {
                if (docFullUrls != null && docFullUrls.Count > 0)
                {
                    if (stubType == "null")
                    {
                        logger.Info("this job is not create stub job");
                        return;
                    }

                    var defaultSuffix = EnsureStubType(stubType);
                    RemoteSiteCollection remoteSiteCollection = RABrowserClient.GetRemoteSiteCollectionByUrl(siteUrl);
                    AveBPOSAccountInfo bposInfo = RA.RACommonUtility.CommonPoolUserUtil.GetBPOSInfo(remoteSiteCollection);
                    var aveObjectModelFactory = RA.Common.Util.MultiAppUtil.CreateAveObjectModelFactory(siteUrl, bposInfo, AveContextKind.ClientObjectModel);
                    using (IAveSite mSite = aveObjectModelFactory.CreateSite(siteUrl))
                    {
                        foreach (var webUrl in docFullUrls.Keys)
                        {
                            if (docFullUrls[webUrl].Count <= 0)
                            {
                                continue;
                            }
                            var webServerRelatedUrl = GetWebServerServerRelativeUrl(webUrl, mSite);
                            using (IAveWeb web = mSite.OpenWeb(webServerRelatedUrl))
                            {
                                foreach (var docUrl in docFullUrls[webUrl])
                                {
                                    try
                                    {
                                        var possiblyStubSuffixes = GetPossiblyStubSuffixes(defaultSuffix);
                                        foreach (var stub in possiblyStubSuffixes)
                                        {
                                            var stubRelativeUrl = GetWebServerServerRelativeUrl(string.Format("{0}{1}", docUrl, stub), mSite);
                                            var stubFile = web.GetFile(stubRelativeUrl);
                                            bool isStubMatch = false;
                                            if (stubFile.Exists)
                                            {
                                                PossiblyStubSuffix = stub;
                                                try
                                                {
                                                    if (stubFile.Item != null)
                                                    {
                                                        var archiverLinkFileType = stubFile.Item.FieldValues["ArchiverLinkFileType"];
                                                        isStubMatch = archiverLinkFileType.ToString().StartsWith(jobId.Substring(0, jobId.LastIndexOf('_')));
                                                    }
                                                    else
                                                    {
                                                        continue;
                                                    }
                                                }
                                                catch (Exception e)
                                                {
                                                    logger.Warn("file not a stub,because it's fieldValues does not contain ArchiverLinkFileType,error:{0}", e.ToString());
                                                    continue;
                                                }
                                                if (isStubMatch)
                                                {
                                                    try
                                                    {
                                                        stubFile.Delete();
                                                    }
                                                    catch (Exception e)
                                                    {
                                                        logger.Info($"delete file exception: {e.Message}. retry action.");
                                                        Record.UndeclareItemAsRecord(stubFile.Item);
                                                        stubFile.Delete();
                                                    }
                                                    break;
                                                }
                                                else
                                                {
                                                    logger.Info(string.Format("stub type: {0} does not exist in library.", System.IO.Path.GetExtension(stub)));
                                                }
                                            }
                                            else
                                            {
                                                logger.Info("current stub type:{0} not exsit.", stub);
                                            }
                                        }
                                    }
                                    catch (Exception e)
                                    {
                                        logger.Error($"delete stubfile failed reson:{e.ToString()}");
                                        throw;
                                    }
                                }
                            }
                        }
                    }
                }
                else
                {
                    logger.Info($"the job:{jobId} has no stub need to delete.");
                }
            }
            catch (Exception e)
            {
                logger.Error(string.Format("Error in remove archive stub.reason : {0}.", e.ToString()));
                throw;
            }
        }
        private void OpenMainIndex(ArchiverRetentionInfo archiverRetentionInfo, String indexVolume)
        {
            this.logger.Info("Begin opening mainindex.");
            var indexServiceOpenParameter = new ArchiverIndexServiceOpenParameter()
            {
                IndexDatabaseName = ServiceConstants.IndexDBName,
                BackupJobId = archiverRetentionInfo.JobId,
                IndexVolume = indexVolume,
                TreeMode = TreeMode.SiteCollectionMode,
                IndexLogicalDeviceSystem = this.indexLogicalDevice,
                IndexCacheDeviceSystem = this.CacheManager.CacheSystem,
                CacheSetting = archiverRetentionInfo.CacheSetting,
                StorageInfo = archiverRetentionInfo.MainIndexStorageInfo
            };
            IndexSynchronizer.Initialize(indexServiceOpenParameter);
            this.InitIndexProcessor(indexServiceOpenParameter);
        }

        private void InitIndexProcessor(ArchiverIndexServiceOpenParameter openParam)
        {
            var realIndexDevice = this.CacheManager.CacheSystem;
            IndexDatabaseDownLoadResult indexDownLoadInfo;
            var logicalStorageInfo = XConvert.FromNames(openParam.IndexVolume, openParam.IndexDatabaseName, openParam.StorageInfo);
            if (openParam.IndexLogicalDeviceSystem.FileExists(logicalStorageInfo))
            {
                if (MediaConfigInfo.CommonConfigInfo.ForceUseCache)
                {
                    var dbInfo = new IndexDatabaseInfo(openParam);
                    indexDownLoadInfo = this.IndexSynchronizer.Download(dbInfo);
                }
                else
                    indexDownLoadInfo = new IndexDatabaseDownLoadResult(IndexDatabaseStatus.Cached, SecurityUtils.SafeCombinePath(realIndexDevice.SystemLocation, SecurityUtils.SafeCombinePath(openParam.IndexVolume, openParam.IndexDatabaseName)));
            }
            else
            {
                indexDownLoadInfo = new IndexDatabaseDownLoadResult(IndexDatabaseStatus.Nonexistent, SecurityUtils.SafeCombinePath(realIndexDevice.SystemLocation, SecurityUtils.SafeCombinePath(openParam.IndexVolume, openParam.IndexDatabaseName)));
            }
            realIndexDevice.OpenDirectory(XConvert.FromNames(openParam.IndexVolume, string.Empty), FileMode.Create);
            AvePoint.Common.IdentityManager.IdentityMode = AvePoint.Common.IdentityMode.Process;
            ArchiverIndexProcessorParameter param = new ArchiverIndexProcessorParameter(IdentityManager.IdentityContent)
            {
                DownLoadResult = indexDownLoadInfo,
                IndexWorkingSystem = realIndexDevice,
            };
            if (openParam.IndexDatabaseName.Equals(ServiceConstants.IndexDBName))
            {
                param.IsNeedCheckIntegrity = true;
                this.IndexMainProcessor.Open(param);
            }
            this.logger.Info("Open MainIndex Finished.");
        }

        private void UploadIndexToRealSystem()
        {
            if (this.IndexMainProcessor != null)
            {
                this.IndexMainProcessor.Close();
            }
            var storageInfo = XConvert.FromNames(archiverRetentionInfo.IndexVolume, ServiceConstants.IndexDBName, archiverRetentionInfo.MainIndexStorageInfo);
            var dbInfo = new IndexDatabaseInfo(ServiceConstants.IndexDBName, null);
            this.IndexSynchronizer.Upload(dbInfo);
        }

        private async System.Threading.Tasks.Task UpdateProcessSucceedRecord(Record record)
        {
            logger.Info($"Update record to db success, the item id:{record?.Id}");
            //only report item

        }

        private void UpdateProcessFailedRecord(Record record, Exception ex)
        {
            logger.Warn($"Update record to db failed, the item id:{record?.Id}, error: {ex}");
        }

        private string GetManualFolderPath(ManualExportReportInfo item)
        {
            string folderPath = string.Empty;
            try
            {
                 if (item.ObjectLevel == RMReportObjectLevel.Item || item.ObjectLevel == RMReportObjectLevel.Folder || item.ObjectLevel == RMReportObjectLevel.Document)
                 {
                    folderPath = item.Path.Replace("\\", "/").Replace(item.SiteUrl, "").Replace(item.LeafName, "");
                    folderPath = folderPath.Substring(0, folderPath.Length - 1);
                  }
            }
            catch (Exception ex)
            {
                logger.Error($"ArchiverLifecycleRetentionService-GetManualFolderPath Error, Exception: {ex}");
                throw ;
            }
            return folderPath;

        }

    }

    #region Code copy from SO for manual approve
    /// <summary>
    /// Azure table storage
    /// </summary>
    public class SOArchiverAzureDBWorker : IDisposable
    {
        private static AveLogger logger = AveLogger.GetInstance(typeof(SOArchiverAzureDBWorker));

        private const string _SOArchiverDBPrefix = "SOArchiverDB";
        private const string _SOStaticArchiverDBPrefix = "SOStaticArchiverDB";

        private string mSiteUrl = null;
        private string mScanJobID = null; 
        private string mScanScopePath = null;
        private int mNodeLevel = 0;
        private string mApproveTableName = null;
        private string mStaticApproveTableName = null; 
        private string EMPTYRULEID = null;
        private string mCurrentRuleId = null;
        private string mCurrentJobId = string.Empty;
        private string mSiteId = null;
        private string mRegisterSiteId = string.Empty;
        private Guid mSiteGroupId; 
        //private Queue mNodeCacheQueue = new Queue();
        private List<ArchiverTableEntity> scanInsertDBEntities = new List<ArchiverTableEntity>();
        private bool IsOneDriveSite;
        //TODO:unused
        public const int DEFAULT_TIMEOUT = 180;

        public static string ScanSubJobId
        {
            get;
            private set;
        }

        public string ConnectionString = null;

        public string ScanScopePath
        {
            get
            {
                return mScanScopePath;
            }
        }
         

        public SOArchiverAzureDBWorker()
        { }

        public SOArchiverAzureDBWorker(string connectionString)
            : this(connectionString, DEFAULT_TIMEOUT)
        { }

        public SOArchiverAzureDBWorker(string connectionString, int timeout)
        {
            ConnectionString = connectionString;
        }

        public SOArchiverAzureDBWorker(ArchiverMessage msg, bool IsOneDriveSite)
        {
            this.IsOneDriveSite = IsOneDriveSite;
            initArchiverAzureDBConnection(msg.ArchiverDBInfo);
            initArchiverJob(msg);
            CreateArchiverTable();
            //DeleteAzureTableOldData(msg);
        }

        #region Private Func
        private void initArchiverJob(ArchiverMessage msg)
        {
            if (msg.Job.Id.Contains("S"))
            {
                this.mScanJobID = msg.Job.Id.Substring(0, msg.Job.Id.IndexOf('S'));
                ScanSubJobId = msg.SubJobId;//msg.Job.Id;
            }
            else
            {
                this.mScanJobID = msg.Job.Id.Substring(0, msg.Job.Id.LastIndexOf('A'));
                ScanSubJobId = this.mScanJobID;
            }

            this.mApproveTableName = string.Concat(_SOArchiverDBPrefix, msg.TenantGroupId.Replace("-", string.Empty));
            this.mStaticApproveTableName = string.Concat(_SOStaticArchiverDBPrefix, msg.TenantGroupId.Replace("-", string.Empty)); 

            mRegisterSiteId = msg.RegisterSiteId;
            mSiteId = msg.ScheduledConfigs[0].SiteId;
            this.mSiteUrl = msg.ScheduledConfigs[0].SiteUrl;
            this.mNodeLevel = (int)msg.ScheduledConfigs[0].NodeLevel;
            mSiteGroupId = new Guid(msg.ScheduledConfigs[0].WebAppId);
            this.mScanScopePath = this.mSiteId;
        }
        private void initArchiverAzureDBConnection(AzureTableConnectContract archiverDBInfo)
        {
            ConnectionString = GetConnectString(archiverDBInfo);
        }

        private string GetConnectString(AzureTableConnectContract info)
        {
            //Maybe useless code
            if (string.IsNullOrEmpty(info.AccountKey) || string.IsNullOrEmpty(info.AccountName))
            {
                logger.Info("Use managed identity authentication table connection string");
                return info.Endpoint;
            }

            string accountKey = GetArchiverDBAccountKey(info.AccountKey);
            if (!string.IsNullOrEmpty(info.Endpoint))
            {
                return string.Format("DefaultEndpointsProtocol=https;AccountName={0};AccountKey={1};TableEndpoint={2}", info.AccountName, accountKey, info.Endpoint);
            }
            else
            {
                return string.Format("DefaultEndpointsProtocol=https;AccountName={0};AccountKey={1};", info.AccountName, accountKey);
            }
        }

        private string GetArchiverDBAccountKey(string accountKey)
        {
            string key = accountKey;
            try
            {
                //TODO
                //key = CryptoUtil.ConvertBytesToString(CspCommunicationWrapper.UnWrapKey(accountKey));
            }
            catch (Exception ex)
            {
                logger.Info("Can't get ArchiverDBAccountKey:{0}.Message:{1}.", accountKey, ex.ToString());
                key = accountKey;
            }
            return key;
        }

        private void CreateArchiverTable()
        {
            AzureTableStorageUtility.CreateAzureTable(ConnectionString, this.mApproveTableName, this.mStaticApproveTableName);
        }
          

        /// <summary>
        /// RowKey is consist of ScanSubJobId, '_' and timestamp
        /// </summary> 
        /// <returns></returns>
        private ArchiverTableEntity GenerateEntity(ArchiverBasicIndex index, Rule rule)
        { 
            ArchiverTableEntity entity = new ArchiverTableEntity();
            try
            {
                entity.PartitionKey = mScanScopePath;
                entity.SourceFlag = 99;
                entity.RowKey = index.PathMD5 + "_" + index.ArchiveTime;
                logger.Info("The RowKey is: {0}, node id:{1}", entity.RowKey, index.PathMD5); 
               
                entity.SortTicks = Snowflake.Instance().GetTicks().ToString();
                entity.ScanJobID = mScanJobID;
                entity.NodeID = new Guid(index.NodeGuid);
                entity.ParentID = new Guid(index.PathMD5);
                //entity.UIVersion = reportNode.UIVersion;
                entity.CacheNodeType = 0;
                entity.ArchiveLevel = (int)PolicyLevel.Document;
                entity.Status = 1; //waiting for approve
                entity.ExportToRECO = false;
                entity.RuleID = new Guid(index.Retention);
                entity.ScopeID = new Guid(mSiteId); 
                //entity.HasRelatedDocument = reportNode.HasRelatedDocument;
                //entity.DeleteRelatedRecords = reportNode.DeleteRelatedRecords;
                //entity.RelatedRecordInfo = reportNode.RelatedRecordInfo;
                #region Json Meta
                ArchiverSharePointDto spDataSource = new ArchiverSharePointDto();
                spDataSource.ScopeID = entity.ScopeID;
                spDataSource.ScanJobID = entity.ScanJobID;
                spDataSource.NodeID = entity.NodeID;
                //spDataSource.ParentID = entity.ParentID;
                //spDataSource.UIVersion = entity.UIVersion;
                //spDataSource.CacheNodeType = entity.CacheNodeType;
                spDataSource.ArchiveLevel = entity.ArchiveLevel;
                spDataSource.KeepDataStatus = 0;
                spDataSource.RuleID = entity.RuleID;

                spDataSource.LastModifiedTime = index.ModifyTime;
                spDataSource.LeafName = index.Name;
                spDataSource.Level = mNodeLevel;
                spDataSource.ExpireTime = CalculateExpire(index, rule); //calculate;
                spDataSource.Path = index.Url;
                spDataSource.Property = GetRootXml();
                //spDataSource.SPNodeLevel = reportNode.SPNodeLevel;
                spDataSource.ScanItemID = 0;
                spDataSource.ScanTime = DateTime.UtcNow;
                spDataSource.SiteUrl = index.SitePath;
                spDataSource.SiteId = new Guid(mSiteId);
                spDataSource.RegistedSiteId = new Guid(mRegisterSiteId);
                //spDataSource.WebId = index.WebID;
                //spDataSource.Metadata = reportNode.Metadata ?? string.Empty;
                spDataSource.ArchivedTime = new DateTime(index.ArchiveTime);
                spDataSource.SiteGroupId = mSiteGroupId;
                spDataSource.SiteTitle = index.SitePath;
                spDataSource.SourceFlag = IsOneDriveSite ? 6 : 1;
                #endregion
                string jsonMeta = JsonConvert.SerializeObject(spDataSource);
                entity.JsonMeta = jsonMeta;

            }
            catch (Exception ex)
            {
                logger.Error(string.Format("Error in generate entity {0}, reason : {1}.",  "", ex.ToString()));
                throw;
            }
            return entity; 
        }
          
        private DateTime CalculateExpire(ArchiverBasicIndex index, Rule rule)
        {
            if(rule.RetentionInfo.Condition == GCommon.Contract.Server.Common.Profile.Object.TimeFilterCondition.OlderThan)
            {
                DateTime archiverTime = new DateTime(index.ArchiveTime);
                switch (rule.RetentionInfo.KeepDateUnite)
                {
                    case GCommon.Contract.Server.Common.Profile.Object.TimeUnit.Day:
                        archiverTime = archiverTime.AddDays(rule.RetentionInfo.KeepDateNumber);
                        break;
                    case GCommon.Contract.Server.Common.Profile.Object.TimeUnit.Week:
                        archiverTime = archiverTime.AddDays(rule.RetentionInfo.KeepDateNumber * 7);
                        break;
                    case GCommon.Contract.Server.Common.Profile.Object.TimeUnit.Month:
                        archiverTime = archiverTime.AddMonths(rule.RetentionInfo.KeepDateNumber);
                        break;
                    case GCommon.Contract.Server.Common.Profile.Object.TimeUnit.Year:
                        archiverTime = archiverTime.AddYears(rule.RetentionInfo.KeepDateNumber);
                        break;
                }
                return archiverTime;
            }
            else if (rule.RetentionInfo.Condition == GCommon.Contract.Server.Common.Profile.Object.TimeFilterCondition.Is)
            {
                return new DateTime(rule.RetentionInfo.Date, DateTimeKind.Utc);
            }
            return DateTime.MaxValue;
        }

        private string GetRootXml()
        {
            string rootXml = "<SOArchive/>";
            return rootXml;
        }

        #endregion

        #region Public Func

        public bool CheckAzureTableConnect()
        {
            return AzureTableStorageUtility.CreateAzureTable(ConnectionString, this.mApproveTableName, this.mStaticApproveTableName);
        }

         
        internal ArchiverTableEntity GetItem(string rowKey)
        {
            //string condition = ArchiverQueryFactory.CreatePartitionKeyAndRowKeyQuery(mScanScopePath, rowKey);
            return AzureTableStorageUtility.RetrieveTableEntity<ArchiverTableEntity>(ConnectionString, mApproveTableName, mScanScopePath, rowKey);
        }
        internal ArchiverTableEntity GetItemByUniqueId(Guid uniqueId)
        {
            string condition = ArchiverQueryFactory.CreateItemIdQuery(mScanScopePath, uniqueId);

            return AzureTableStorageUtility.RetrieveTableEntitiesInCondition<ArchiverTableEntity>(ConnectionString, mApproveTableName, condition).FirstOrDefault();
        }


        public void InsertScanReportToApproveDB(ArchiverBasicIndex index, Rule rule)
        { 
            ArchiverTableEntity entity = GenerateEntity(index,rule);
            scanInsertDBEntities.Add(entity);
            if (scanInsertDBEntities.Count >= 100)
            {
                logger.Info("Current ScanInsertDBEntities count:{0}.Begin to add Azure Table.", scanInsertDBEntities.Count);
                AzureTableStorageUtility.AddAzureTableEntities(ConnectionString, mApproveTableName, scanInsertDBEntities);
                scanInsertDBEntities.Clear();
            } 
        }
 
        public void Reset(string ruleId, string jobId)
        {
            if (string.IsNullOrEmpty(ruleId))
            {
                mCurrentRuleId = EMPTYRULEID;
            }
            else
            {
                mCurrentRuleId = ruleId;
            }
            mCurrentJobId = jobId; 
        }

        internal void AddItems2Static(List<ArchiverTableEntity> entities)
        {
            AzureTableStorageUtility.AddAzureTableEntities<ArchiverTableEntity>(ConnectionString, mStaticApproveTableName, entities);
        }

        //for Manual....
        internal void DeleteItem(ArchiverTableEntity entity)
        { 
            AzureTableStorageUtility.DeleteTableEntity<ArchiverTableEntity>(ConnectionString, mApproveTableName, entity);
        }
        internal void DeleteItems(List<ArchiverTableEntity> entities)
        {
            AzureTableStorageUtility.DeleteTableEntities<ArchiverTableEntity>(ConnectionString, mApproveTableName, entities);
        }
        internal async System.Threading.Tasks.Task updateItemsAsync(List<ArchiverTableEntity> entity)
        {
            await AzureTableStorageUtility.UpdateTableEnitiesAsync<ArchiverTableEntity>(ConnectionString, mApproveTableName, entity);
        }
        internal void updateItem(ArchiverTableEntity entity)
        {
            AzureTableStorageUtility.UpdateTableEnity<ArchiverTableEntity>(ConnectionString, mApproveTableName, entity);
        }
        //for Manual....
        public void DeleteItemByRule(string partitionKey, string ruleId)
        { 
            string condition = ArchiverQueryFactory.CreateGetItemByRuleQuery(partitionKey, new Guid(ruleId));
            AzureTableStorageUtility.DeleteTableEntitiesWithCondition<ArchiverTableEntity>(ConnectionString, mApproveTableName, condition); 
        }
         

        #endregion

        public void Dispose()
        {
            if (scanInsertDBEntities != null && scanInsertDBEntities.Count > 0)
            {
                logger.Info("Dispose ScanInsertDBEntities count:{0}.Begin to add Azure Table.", scanInsertDBEntities.Count);
                AzureTableStorageUtility.AddAzureTableEntities(ConnectionString, mApproveTableName, scanInsertDBEntities);
                scanInsertDBEntities.Clear();
            }
            mScanJobID = null;
            mScanScopePath = null;
            mApproveTableName = null;
            ScanSubJobId = null;
            ConnectionString = null;
        }



        internal class ArchiverQueryFactory
        {
            internal static string CreatePartitionKeyQuery(string partitionKey)
            {
                AzureTableQueryConditionBuilder builder = new AzureTableQueryConditionBuilder(partitionKey);
                return builder.ToString();
            }

            internal static string CreateManualPartitionKeyQuery(string partitionKey)
            {
                AzureTableQueryConditionBuilder builder = new AzureTableQueryConditionBuilder();
                builder.AppendAndQuery(ArchiverTableEntityProperty.PartitionKey, AzureQueryComparisons.Equal, string.Concat(partitionKey, "Manual"), AzureDataType.String);
                return builder.ToString();
            }

            internal static string CreatePartitionKeyAndRowKeyQuery(string partitionKey, string rowKey)
            {
                AzureTableQueryConditionBuilder builder = new AzureTableQueryConditionBuilder(partitionKey, rowKey);
                return builder.ToString();
            }


            //internal static string CreatePartitionKeyAndNodeIdsQuery(string partitionKey, List<Guid> nodeIds)
            //{
            //    AzureTableQueryConditionBuilder builder = new AzureTableQueryConditionBuilder(partitionKey);
            //    builder.AppendAndQuery(ArchiverTableEntityProperty.NodeID, AzureQueryComparisons.con, nodeId, AzureDataType.Guid);
            //    return builder.ToString();
            //}

            internal static string CreateItemIdQuery(string partitionKey, Guid nodeId)
            {
                AzureTableQueryConditionBuilder builder = new AzureTableQueryConditionBuilder(partitionKey);
                builder.AppendAndQuery(ArchiverTableEntityProperty.NodeID, AzureQueryComparisons.Equal, nodeId, AzureDataType.Guid);
                return builder.ToString();
            }

            internal static string CreateGetItemWithVersionQuery(string partitionKey, Guid nodeId, int versionId)
            {
                AzureTableQueryConditionBuilder builder = new AzureTableQueryConditionBuilder();
                string ManualCondition = AzureTableQueryConditionBuilder.CreateTemperaryQuery(ArchiverTableEntityProperty.PartitionKey, AzureQueryComparisons.Equal, string.Concat(partitionKey, "Manual"), AzureDataType.String);
                string NotManualCondition = AzureTableQueryConditionBuilder.CreateTemperaryQuery(ArchiverTableEntityProperty.PartitionKey, AzureQueryComparisons.Equal, partitionKey, AzureDataType.String);
                string CombineCondition = AzureTableQueryConditionBuilder.CombineOrQueries(ManualCondition, NotManualCondition);//Manual_PartitionKey OR PartitionKey

                builder.AppendAndQuery(ArchiverTableEntityProperty.NodeID, AzureQueryComparisons.Equal, nodeId, AzureDataType.Guid);
                builder.AppendAndQuery(ArchiverTableEntityProperty.UIVersion, AzureQueryComparisons.Equal, versionId, AzureDataType.Int);
                return AzureTableQueryConditionBuilder.CombineAndQueries(builder.ToString(), CombineCondition);
            }

            //add for SAAS-22498
            internal static string CreateGetItemKeepDataOptionInfoQuery(string partitionKey, string rowKey, Guid nodeId)
            {
                AzureTableQueryConditionBuilder builder = new AzureTableQueryConditionBuilder(partitionKey);
                //builder.AppendAndQuery(ArchiverTableEntityProperty.RowKey, AzureQueryComparisons.GreaterThanOrEqual, rowKey);
                builder.AppendAndQuery(ArchiverTableEntityProperty.NodeID, AzureQueryComparisons.Equal, nodeId, AzureDataType.Guid);
                return builder.ToString();
            }

            internal static string CreateGetApprovedOrWaitingApproveItemQuery(string partitionKey, Guid nodeId, Guid ruleId, int approvedStatus, int waitingApprovedStatus)
            {
                partitionKey = string.Concat(partitionKey, "Manual");
                AzureTableQueryConditionBuilder builder = new AzureTableQueryConditionBuilder(partitionKey);
                //builder.AppendAndQuery(ArchiverTableEntityProperty.RowKey, AzureQueryComparisons.GreaterThanOrEqual, rowKey);
                builder.AppendAndQuery(ArchiverTableEntityProperty.NodeID, AzureQueryComparisons.Equal, nodeId, AzureDataType.Guid);
                builder.AppendAndQuery(ArchiverTableEntityProperty.RuleID, AzureQueryComparisons.Equal, ruleId, AzureDataType.Guid);
                string approvedCondition = AzureTableQueryConditionBuilder.CreateTemperaryQuery(ArchiverTableEntityProperty.Status, AzureQueryComparisons.Equal, approvedStatus, AzureDataType.Int);
                string waitingApprovedCondition = AzureTableQueryConditionBuilder.CreateTemperaryQuery(ArchiverTableEntityProperty.Status, AzureQueryComparisons.Equal, waitingApprovedStatus, AzureDataType.Int);
                string waitingOrApprovedCondition = AzureTableQueryConditionBuilder.CombineOrQueries(approvedCondition, waitingApprovedCondition);
                return AzureTableQueryConditionBuilder.CombineAndQueries(builder.ToString(), waitingOrApprovedCondition);
            }
                  
            internal static string CreateGetNotWaitingAndRejectedItemsByScanJobIdQuery(string partitionKey, string scanJobId, Guid ruleId)
            {
                AzureTableQueryConditionBuilder builder = new AzureTableQueryConditionBuilder();

                string ManualCondition = AzureTableQueryConditionBuilder.CreateTemperaryQuery(ArchiverTableEntityProperty.PartitionKey, AzureQueryComparisons.Equal, string.Concat(partitionKey, "Manual"), AzureDataType.String);
                string NotManualCondition = AzureTableQueryConditionBuilder.CreateTemperaryQuery(ArchiverTableEntityProperty.PartitionKey, AzureQueryComparisons.Equal, partitionKey, AzureDataType.String);
                string CombineCondition = AzureTableQueryConditionBuilder.CombineOrQueries(ManualCondition, NotManualCondition);//Manual_PartitionKey OR PartitionKey

                builder.AppendAndQuery(ArchiverTableEntityProperty.ScanJobID, AzureQueryComparisons.Equal, scanJobId);
                builder.AppendAndQuery(ArchiverTableEntityProperty.Status, AzureQueryComparisons.Equal, 3, AzureDataType.Int); //approved
                builder.AppendAndQuery(ArchiverTableEntityProperty.RuleID, AzureQueryComparisons.Equal, ruleId, AzureDataType.Guid);
                return AzureTableQueryConditionBuilder.CombineAndQueries(builder.ToString(), CombineCondition);
            }

            internal static string CreateGetApprovalItemsBySubJobIdAndRuleIdQuery(string partitionKey, string subJobId, string ruleId)
            {
                AzureTableQueryConditionBuilder builder = new AzureTableQueryConditionBuilder(partitionKey);

                builder.AppendAndQuery(ArchiverTableEntityProperty.ScanJobID, AzureQueryComparisons.Equal, subJobId);
                builder.AppendAndQuery(ArchiverTableEntityProperty.RuleID, AzureQueryComparisons.Equal, ruleId);
                builder.AppendAndQuery(ArchiverTableEntityProperty.Status, AzureQueryComparisons.Equal, 3, AzureDataType.Int);
                return builder.ToString();
            }
               

            internal static string CreateGetItemQueryByNodeId(string partitionKey, Guid nodeId)
            {
                string manualCondition = AzureTableQueryConditionBuilder.CreateTemperaryQuery(ArchiverTableEntityProperty.PartitionKey, AzureQueryComparisons.Equal, string.Concat(partitionKey, "Manual"), AzureDataType.String);
                string notManualCondition = AzureTableQueryConditionBuilder.CreateTemperaryQuery(ArchiverTableEntityProperty.PartitionKey, AzureQueryComparisons.Equal, partitionKey, AzureDataType.String);
                string combineCondition = AzureTableQueryConditionBuilder.CombineOrQueries(manualCondition, notManualCondition);//Manual_PartitionKey OR PartitionKey

                string waitingStatusCondition = AzureTableQueryConditionBuilder.CreateTemperaryQuery(ArchiverTableEntityProperty.Status, AzureQueryComparisons.Equal, 1, AzureDataType.Int); //waiting
                string approvedStatusCondition = AzureTableQueryConditionBuilder.CreateTemperaryQuery(ArchiverTableEntityProperty.Status, AzureQueryComparisons.Equal, 3, AzureDataType.Int); //approved
                string statusCondition = AzureTableQueryConditionBuilder.CombineOrQueries(waitingStatusCondition, approvedStatusCondition);

                string combineKeyStatus = AzureTableQueryConditionBuilder.CombineAndQueries(combineCondition, statusCondition);
                string nodeIdCondition = AzureTableQueryConditionBuilder.CreateTemperaryQuery(ArchiverTableEntityProperty.NodeID, AzureQueryComparisons.Equal, nodeId, AzureDataType.Guid);

                return AzureTableQueryConditionBuilder.CombineAndQueries(combineKeyStatus, nodeIdCondition);
            }

            internal static string CreateGetSpecificScanJobItemsQuery(string partitionKey, string rowKey, string scanJobId)
            {
                AzureTableQueryConditionBuilder builder = new AzureTableQueryConditionBuilder(partitionKey);
                builder.AppendAndQuery(ArchiverTableEntityProperty.RowKey, AzureQueryComparisons.GreaterThanOrEqual, rowKey);
                builder.AppendAndQuery(ArchiverTableEntityProperty.ScanJobID, AzureQueryComparisons.Equal, scanJobId);
                return builder.ToString();
            }
               

            internal static string CreateGetItemByRuleQuery(string partitionKey, Guid ruleId)
            {
                //Manual Job PartitionKey带prefix Manual
                AzureTableQueryConditionBuilder builder = new AzureTableQueryConditionBuilder();
                builder.AppendAndQuery(ArchiverTableEntityProperty.PartitionKey, AzureQueryComparisons.Equal, partitionKey); 
                string condition = AzureTableQueryConditionBuilder.CreateTemperaryQuery(ArchiverTableEntityProperty.RuleID, AzureQueryComparisons.Equal, ruleId, AzureDataType.Guid); 
                return AzureTableQueryConditionBuilder.CombineAndQueries(builder.ToString(), condition);
            }

        }

        internal class ArchiverTableEntityProperty
        {
            internal static string ScanItemID = "ScanItemID";
            internal static string ScopeID = "ScopeID";
            internal static string ScanJobID = "ScanJobID";
            internal static string NodeID = "NodeID";
            internal static string ParentID = "ParentID";
            internal static string LeafName = "LeafName";
            internal static string Path = "Path";
            internal static string ScanTime = "ScanTime";
            internal static string UIVersion = "UIVersion";
            internal static string LibRowID = "LibRowID";
            internal static string NodeType = "NodeType";
            internal static string SPNodeLevel = "SPNodeLevel";
            internal static string CacheNodeType = "CacheNodeType";
            internal static string Level = "Level";
            internal static string ArchiveLevel = "ArchiveLevel";
            internal static string Status = "Status";
            internal static string RuleID = "RuleID";
            internal static string ExpireTime = "ExpireTime";
            internal static string LastModifiedTime = "LastModifiedTime";
            internal static string KeepDataStatus = "KeepDataStatus";
            internal static string Property = "Property";
            internal static string SourceFlag = "SourceFlag";

            internal static string RowKey = "RowKey";
            internal static string PartitionKey = "PartitionKey";
        }

        internal class Snowflake
        {









            private static long sequenceBits = 12L; //计数器字节数，12个字节用来保存计数码        



            public static long sequenceMask = -1L ^ -1L << (int)sequenceBits; //一微秒内可以产生计数，如果达到该值则等到下一微妙在进行生成
            private static long lastTimestamp = -1L;//最后时间戳


            static Snowflake snowflake;

            public static Snowflake Instance()
            {
                if (snowflake == null)
                    snowflake = new Snowflake();
                return snowflake;
            }

            public Snowflake()
            {
                //Snowflakes(0L, -1);
            }
             
            /// <summary>
            /// 生成当前时间戳
            /// </summary>
            /// <returns>毫秒</returns>


             

            public long GetTicks()
            {
                long nowTicks = 0L;
                bool flag = false;
                do
                {
                    nowTicks = DateTime.UtcNow.Ticks;
                    if (nowTicks > lastTimestamp)
                    {
                        lastTimestamp = nowTicks;
                        flag = true;
                    }
                } while (!flag);

                return nowTicks;
            }
        }
        internal enum SOApproveDBStatus
        {
            None = 0,
            WaitingApprove = 1,
            HasBeenReported = 2,
            Approved = 3,
            Rejected = 4,
            Archived = 5,
            Failed = 6,
            Rescan = 7,
            KeepData = 8,
            CheckOption = 9,
        }
    } 
    #endregion
}
