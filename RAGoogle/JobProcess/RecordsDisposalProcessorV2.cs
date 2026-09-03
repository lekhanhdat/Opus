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
using AvePoint.Common;
using AvePoint.Cryptography;
using AvePoint.GCommon.Contract.Media.Object;
using AvePoint.GCommon.Contract.Server.ControlPanel.Cryptography;
using AvePoint.GCommon.Contract.Server.ControlPanel.Object;
using AvePoint.GCommon.Contract.Storage.Entity;
using AvePoint.GCommon.Contract.StorageOptimization.Object;
using AvePoint.GCommon.Contract.Tree;
using AvePoint.GCommon.Contract.Tree.Object;
using AvePoint.GCommon.Media.StorageService;
using AvePoint.GCommon.Utility;
using AvePoint.Media.Common;
using AvePoint.Media.Service.ArchiverBackup;
using AvePoint.Media.Service.DomainModel;
using AvePoint.RA.Common;
using AvePoint.RA.Common.Util;
using AvePoint.RA.Contract;
using AvePoint.RA.Contract.Exceptions;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.Contract.JobMonitor;
using AvePoint.RA.Contract.RMRuleManageMent;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RMWeb.CP;
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using AvePoint.RA.Contract.RMWeb.Setting;
using AvePoint.RA.Contract.RMWeb.StorageDevice;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.DB.Core;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Dao.Extension;
using AvePoint.RA.DB.Dao.Utility;
using AvePoint.RA.DB.Explorer.Model;
using AvePoint.RA.DB.Model;
using AvePoint.RA.I18N.Core;
using AvePoint.RA.RACommonUtility.Encryption;
using AvePoint.RA.SharePoint.ArchiverCommon;
using AvePoint.Wrapper.Common;
using Media.Service.ArchiverBackup.Statistics;
using Newtonsoft.Json;
using RAArchiverCommon;
using RAArchiverCommon.DestructionCache;
using RAExportCommon;
using RAGoogle.Archive;
using RAGoogle.Archive.Common;
using RAGoogle.Archive.Media;
using RAGoogle.Archive.Scan;
using RAGoogle.Archive.Scan.Base;
using RAGoogle.Archive.Scan.Interface;
using RAGoogle.Common;
using RAGoogle.Extension;
using RAGoogle.GoogleObjDiscover;
using RAGoogle.Models;
using RAGoogle.Models.Contract;
using RAGoogle.RecordsDisposal;
using RAGoogle.RecordsDisposal.Action.Archive;
using RAGoogle.RecordsDisposal.Action.Archive.Data;
using RAGoogle.RecordsDisposal.Action.DeleteOnly;
using RAGoogle.RecordsDisposal.Action.ExportOnly;
using RAGoogle.RecordsDisposal.Action.MoveTo;
using RAGoogle.Util;
using System.Collections.Concurrent;
using System.Text;
using AvePoint.RA.Contract.Google;
using AvePoint.RA.Contract.Google.GControlPlatform;
using RAManualApprovalCommon;
using ActionType = RAGoogle.Models.Enums.ActionType;
using BaseJobDto = AvePoint.GCommon.Contract.Server.ControlPanel.Object.BaseJobDto;
using ExportTypeValue = AvePoint.GCommon.Contract.StorageOptimization.Object.ExportTypeValue;
using Google;
namespace RAGoogle.JobProcess
{
    public class RecordsDisposalProcessorV2 : BaseProcessor
    {
        #region propreties
        private readonly GoogleConfiguration _configuration;
        private IStorageDeviceService StorageDeviceService => PlatformWindsorManager.GetService<IStorageDeviceService>();
        private ISettingProfilesDao SettingProfileDao => PlatformWindsorManager.GetService<ISettingProfilesDao>();
        private IExportSettingsDao ExportSettingsDao => (IExportSettingsDao)PlatformWindsorManager.GetService(typeof(IExportSettingsDao));
        private IRMKeyValueDao RMKeyValueDao => (IRMKeyValueDao)PlatformWindsorManager.GetService(typeof(IRMKeyValueDao));
        private IExportDataEncryptionSettingService ExportDataEncryptionSettingService => (IExportDataEncryptionSettingService)PlatformWindsorManager.GetService(typeof(IExportDataEncryptionSettingService));
        private ISettingProfileService SettingProfileService => PlatformWindsorManager.GetService<ISettingProfileService>();
        private IGlobalStorageSettingDao GlobalStorageSettingDao => PlatformWindsorManager.GetService<IGlobalStorageSettingDao>();
        private readonly IGControlPlatformTaskAssigneeService _gControlPlatformTaskAssigneeService = PlatformWindsorManager.GetService<IGControlPlatformTaskAssigneeService>();
        private readonly IGControlTaskAssigneeService _taskAssigneeService = PlatformWindsorManager.GetService<IGControlTaskAssigneeService>();
        private RMAesEncryptorWrapper AesEncryptorWrapper => new();
        private byte[] _NARAConfigFile { get; set; }
        private ConcurrentDictionary<string, GoogleExportBeforeArcInfo> _naraMetadatas { get; set; } = new();
        
        protected override bool NeedScanVersion => true;
        private GoogleExportBeforeArcInfo _exportBeforeArcInfo { get; set; }
        private MediaServerManagementUtil _mediaServer = new MediaServerManagementUtil();
        private StreamWriter streamWriter { get; set; }
        private string secondHeaderFolderPath { get; set; }
        private string secondHeaderFilePath { get; set; }
        private string tenantId { get; set; }
        #endregion

        public RecordsDisposalProcessorV2(string jobId) : base(jobId, JobType.GoogleRecordsDisposal)
        {
            ReportCenter.InitCurrentJobInfo(jobId, JobType.GoogleRecordsDisposal);
            _configuration = new();
            _configuration.Init();
            _configuration.JobId = jobId;
            WrapperConfiguration.TempDirectory = Path.Combine(_configuration.ArchiveTemp, "Wrapper");
            ArchiverCommonStaticMethod.CreateDirectory(WrapperConfiguration.TempDirectory);
            secondHeaderFolderPath = SecurityUtils.SafeCombinePath(_configuration.ArchiveTemp, jobId);
            secondHeaderFilePath = SecurityUtils.SafeCombinePath(secondHeaderFolderPath, jobId + ".tmpheader");
        }

        public async override Task RunNowAsync(RMGoogleSetting? setting, GoogleDriveTreeNodeDto? node)
        {
            using (var performance = new PerformanceScope("RecordsDisposalProcessorV2.RunNowAsync"))
            {
                this.ReportCenter.SummaryComments = string.Empty;
                _naraMetadatas.Clear();
                tenantId = node.TenantId;
                try
                {
                    logger.Debug("Job start.");
                    using (CheckJobStopScope jScope = new())
                    {
                        if (setting is null || node is null)
                        {
                            logger.Error("Setting node and Node info are invalid.");
                            throw new ArgumentNullException("Setting node and Node info are invalid.");
                        }
                        //init
                        InitConfiguration(node, setting);
                        InitNARAConfigFile();
                        byte[] communicationKey = SettingProfileService.GetCommunicationEncryptionKey();
                        CspCommunicationWrapper.CommunicationEncryptionKey = communicationKey;
                        int i = 1;
                        ScanDataCache.Instance.Initialize(SourceFlag.Google);
                        Dictionary<int, Rule> rules = new();
                        var isNullClassification = setting.IsNullClassificationSetting;
                        logger.Info($"Is non-classification job: {isNullClassification}");
                        if(isNullClassification)
                        {
                            rules = await GetNullClassificationRuleIdsAsync();
                        }
                        else
                        {
                            rules = ScanDataCache.Instance.RulesBindingInTerms.Values.ToDictionary(v => i++);
                        }
                        _configuration.RuleCollection = rules;
                        RMSubJob subJobInfo = ReportCenter.GetSubJobInfo(jobId, true);
                        var driveNode = GetDriveNode(node);
                        var scanJobSetting = new ScanJobSettings()
                        {
                            Configuration = _configuration,
                            Id = subJobInfo.ParentId,
                            SubJobId = jobId,
                            DriveNode = driveNode,
                        };

                        SOGDriveArchiverJobInfoStatistics.Instance.InitGDriveInstance(_configuration.JobId, driveNode.DisplayName, JobType.GoogleRecordsDisposal, driveNode.ObjectId);

                        //scanner
                        using (IGoogleScanner scanner = new RecordGoogleScanner(scanJobSetting, Cts))
                        {
                            await scanner.RunAsync();
                            //bakcup data when scanner finish
                            var dataReader = scanner.GetScanDataReader();
                            var dataCount = dataReader.GetDataCount();
                            ScanDataCache.Instance.SetScanDataReader(dataReader);
                            var ruleIds = dataReader.GetAllRuleIds();
                            logger.Debug($"rule id count:{ruleIds.Count}, data count:{dataCount}");
                            foreach (var ruleid in ruleIds)
                            {
                                logger.Debug($"Start to handle datas by rule id:{ruleid}");
                                var temp = rules.Where(r => r.Value.Id == ruleid).FirstOrDefault();
                                var rule = temp.Value;

                                rule.GoogleDriveRule.Id = rule.Id;
                                rule.GoogleDriveRule.Name = rule.Name;
                                rule.Order = temp.Key;
                                _configuration.CurrentRule = rule;
                                _configuration.CurrentRule.Order = temp.Key;
                                _configuration.RuleManager.RebuildRecordsMoveSetting(rule);
                                var dataEnumer = dataReader.GetArchiveApproveReports(ruleid);
                                var action = GetActionType(rule);
                                _configuration.Action = action;
                                logger.Debug($"rule name is:{rule.Name}, action is:{action}");
                                if (action is ActionType.ExportOnly or ActionType.ExportBeforeDel or ActionType.ExportBeforeArchive)
                                {
                                    if (!_naraMetadatas.TryGetValue(rule.GoogleDriveRule.ExportInfo.exportLocationId, out var googleExportInfo))
                                    {
                                        rule.GoogleDriveRule.NARAConfigFile = _NARAConfigFile;
                                        GetExportStorageConfiguration(rule);
                                        GetExportEncryption(rule);
                                        InitExportType(rule, scanJobSetting.DriveNode.DisplayName);
                                        _naraMetadatas.TryAdd(rule.GoogleDriveRule.ExportInfo.exportLocationId, _exportBeforeArcInfo);
                                    }
                                }
                                if (action is ActionType.ArchiveToStorage or ActionType.ExportBeforeArchive)
                                {
                                    var gRule = rule.GoogleDriveRule;
                                    WrapperConfiguration.MoveToArchiverTierWhenArchiving = gRule.MoveToArchiverTierWhenArchiving ? true : (gRule.MoveToAnotherTierType == (int)Storage.AccessTierType.Other || gRule.MoveToAnotherTierType == null) ? false : true;
                                    WrapperConfiguration.MoveToAnotherTierType = gRule.MoveToArchiverTierWhenArchiving ? (int)Storage.AccessTierType.Archive : (gRule.MoveToAnotherTierType == null ? 0 : gRule.MoveToAnotherTierType);
                                    RebuildStoragePolicyDto(gRule);
                                }
                                switch (action)
                                {
                                    case ActionType.DeleteOnly:
                                    case ActionType.ExportBeforeDel:
                                        await DeleteActionAsync(dataEnumer);
                                        break;
                                    case ActionType.ExportOnly:
                                        await ExportActionAsync(dataEnumer);
                                        break;
                                    case ActionType.Move:
                                        await MoveActionAsync(dataEnumer);
                                        break;
                                    case ActionType.ExportBeforeArchive:
                                    case ActionType.ArchiveToStorage:
                                        await ArchiveActionAsync(dataEnumer);
                                        break;
                                }
                                if (SOGDriveArchiverJobInfoStatistics.Instance.IsNeedStatisticsAction)
                                {
                                    SOGDriveArchiverJobInfoStatistics.Instance.SaveInfoToGDriveDB();
                                }
                            }
                            ReportJobDetails();
                        }
                        FinishExport();
                        await EndWork();
                        UploadDestructionCache();
                    }
                }
                catch (JobStopException)
                {
                    this.ReportCenter.JobHasStopped = true;
                    ReportCenter.JobHasStopped = true;
                }
                catch (Exception ex)
                {
                    this.ReportCenter.SummaryComments = ex.Message;
                    logger.Error("Failed to kick off google records disposal job, Message: {0}", ex);
                    if (ex is GoogleApiException gex && (gex.HttpStatusCode == System.Net.HttpStatusCode.NotFound && gex.Message.Contains(node.ObjectId)))
                    {
                        throw new NotFoundDriveException(I18NEntity.GetString("RM_JM_JD_NotFound_Drive"));
                    }
                }
            }
        }
        private async Task<Dictionary<int, Rule>> GetNullClassificationRuleIdsAsync()
        {
            Dictionary<int, Rule> rules = new();
            int i = 0;
            List<RMSimpleRule> simpleRules = await GoogleSettingDao.GetGoogleDriveMappingRules(_configuration.GoogleSetting.ScopeId);
            var ruleIds = simpleRules.OrderBy(x => x.RuleOrder).Select(s => s.RuleId.ToString()).ToList();
            ruleIds.ForEach(id =>
            {
                var rule = ScanDataCache.Instance.Rules.Values.FirstOrDefault(r => r.Id == id.ToString());
                if (rule != null)
                {
                    RebuildRecordsMoveSetting(rule);
                    rules.TryAdd(i++, rule);
                }
            });
            logger.Info($"Get null classification rules count:{rules.Count}, scopeId:{_configuration.GoogleSetting.ScopeId}");
            return rules;
        }
        
        private void RebuildRecordsMoveSetting(Rule rule)
        {
            if (rule.GoogleDriveRule.spMoveOption is { MoveDestination: not null } && !string.IsNullOrEmpty(rule.GoogleDriveRule.spMoveOption.MoveDestination.DestinationId))
            {
                rule.GoogleDriveRule.MoveToRecordCenterAndDelareSetting = new MoveToRecordCenterAndDelareSetting
                {
                    DestinationLocation = new DestinationLocationInfo
                    {
                        DestinationId = rule.GoogleDriveRule.spMoveOption.MoveDestination.DestinationId,
                        GoogleTreeNode = rule.GoogleDriveRule.spMoveOption.MoveDestination.GoogleTreeNode
                    }
                };
            }
        }
        private async Task ArchiveActionAsync(IEnumerable<ArchiveApproveReport> reader)
        {
            string indexJobId = string.Empty;
            var sourceFlag = SourceFlag.Google;
            var ruleId = _configuration.CurrentRule.Id;
            var dataFlag = SourceFlag.Google;
            var subJobId = _configuration.JobId;
            BackupInfoSender aveSender = null;
            var driveNames = new List<string>();
            var retentionService = new GDriveArchiverLifecycleRetentionService();
            var driveId = string.Empty;
            SOGDriveArchiverJobInfoStatistics.Instance.IsNeedStatisticsAction = true;
            SOGDriveArchiverJobInfoStatistics.Instance.IsDeleteArchiveAction = true;
            try
            {
                InitStreamWriter();
                aveSender = _mediaServer.ConfigMedia(ruleId, subJobId, _configuration, ref indexJobId, sourceFlag, dataFlag);
                _configuration.CurrentIndexJobID = indexJobId;
                var (exportController, backupController) = InitBackupController(_configuration.CurrentRule, null, null, _configuration.Action);
                logger.Debug($"Start to handle data one by one in archive action.");
                foreach (var item in reader)
                {
                    try
                    {
                        using (CheckJobStopScope jScope = new CheckJobStopScope())
                        {
                            using (new PerformanceScope("GoogleRecordsDisposalV2.ArchiveActionAsync"))
                            {
                                InitScanReportJobDetails(item);
                                if (item.CacheNodeType == (int)GoogleCacheNodeType.ItemVersion)
                                {
                                    logger.Info("Ignore item version cache. ItemName: {0}", item.NodeId);
                                    continue;
                                }
                                if (item.CacheNodeType == (int)GoogleCacheNodeType.Drive)
                                {
                                    driveNames.Add(item.LeafName);
                                    driveId = item.NodeId;
                                }

                                var backupNodeParameters = new BackupNodeParameters()
                                {
                                    Node = item,
                                    CacheNode = new CacheNode(),
                                    RuleName = _configuration.currentRule.Name,
                                    SubJobId = jobId,
                                    RuleLevel = (int)_configuration.currentRule.PolicyLevel,
                                    MediaName = string.Empty,
                                    Sender = aveSender,
                                    ExportBeforeArcInfo = _configuration.Action == ActionType.ExportBeforeArchive ? _exportBeforeArcInfo : null,
                                };
                                try
                                {
                                    aveSender.BackupStream.SetStreamTransfered(0);
                                    RegisterSecondHeaderEventHandler(backupNodeParameters.CacheNode);
                                    await backupController.ProcessArchiveReport(item, backupNodeParameters);
                                }
                                catch (Exception ex)
                                {
                                    logger.Error($"An error occurred while processing archive action for item [{item.NodeId}]. Error: {ex}");
                                }
                                finally
                                {
                                    backupNodeParameters.CacheNode.Dispose();
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        logger.Error($"An error occurred while processing archive action for item [{item.NodeId}]. Error: {ex}");
                        item.AddToReportsByArchiveApproveReport(_configuration.ActionApproveReports, ActionTab.Backup, JobDetailsStatus.Failed, item.DocumentSize, _configuration.CurrentRule?.Name, ex.Message);
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error($"An error occurred while processing archive action. Error: {ex}");
            }
            BackupCloseInfo closeInfo = new BackupCloseInfo()
            {
                ErrorMessage = "",
            };
            if (aveSender != null) aveSender.FileSender.Close(closeInfo);

            CacheSecondHeader("End");
            // merge sub sub job Index
            try
            {
                await MergeIndexAsync();
            }
            catch (Exception ex)
            {
                logger.Error($"An error occurred while merging index for archive action. Error: {ex}");
                throw;
            }
            try
            {
                var processor = new MessageProcessor(_configuration, Cts);
                await ProcessDeleteFileAsync(processor);
                processor.Dispose();
            }
            catch (Exception ex)
            {
                logger.Error($"An error occurred while sending second headers for archive action. Error: {ex}");
            }
            finally
            {
                //retentionService.UpdateArchivedSiteInfo(driveNames, tenantId, driveId);
            }
        }
        #region second file header
        private void InitStreamWriter()
        {
            if (!Directory.Exists(secondHeaderFolderPath))
            {
                logger.Info("Begin Create second header temp folder for Deletion");
                Directory.CreateDirectory(secondHeaderFolderPath);
            }
            streamWriter = new StreamWriter(secondHeaderFilePath);
        }
        private void RegisterSecondHeaderEventHandler(CacheNode cacheNode)
        {
            cacheNode.CustomizedDisposeAction = () =>
            {
                try
                {
                    CacheSecondHeader(cacheNode.GenerateSecondFileHeader());
                }
                catch (Exception ex)
                {
                    logger.Error(string.Format("Failed to save header, Message:{0}", ex.ToString()));
                    //TODO:Logging
                }
            };
        }

        private void CacheSecondHeader(string tempHeader)
        {
            if (string.IsNullOrEmpty(tempHeader))
            {
                logger.Info("Current second Header IsNullOrEmpty.");
                return;
            }

            try
            {
                streamWriter.WriteLine(tempHeader);
                if (tempHeader.Equals("End", StringComparison.OrdinalIgnoreCase))
                {
                    if (streamWriter != null)
                    {
                        streamWriter.Dispose();
                    }
                }
            }
            catch (Exception e)
            {
                logger.Error($"Current second Header write failed,header:{tempHeader},it may caused the file delete failed,error:{e}.");
            }
        }
        private async Task ProcessDeleteFileAsync(MessageProcessor responseHandle)
        {
            if (System.IO.File.Exists(secondHeaderFilePath))
            {
                using (PerformanceScope pc = new PerformanceScope("GoogleRecordsDisposalV2.ProcessDeleteFileAsync"))
                {
                    await responseHandle.StartProcessAsync();
                    logger.Info($"Second header file exist.path:{secondHeaderFilePath}");
                    using (StreamReader streamReader = new StreamReader(secondHeaderFilePath))
                    {
                        while (streamReader.Peek() > 0)
                        {
                            string tempHeader = streamReader.ReadLine();
                            try
                            {
                                await responseHandle.SaveXmlHeaderAsync(tempHeader);
                            }
                            catch (Exception ex)
                            {
                                logger.Error(string.Format("SendSecondHeaders error. Message:{0}", ex.ToString()));
                            }
                        }
                    }
                    System.IO.File.Delete(secondHeaderFilePath);
                }
            }
            else
            {
                logger.Info("Second header file not exist.");
            }
        }
        #endregion
        private async Task MergeIndexAsync()
        {
            using (PerformanceScope pc = new PerformanceScope("GoogleRecordsDisposalV2.MergeIndexAsync"))
            {
                IdentityManager.IdentityMode = IdentityMode.Process;
                IdentityManager.IdentityType = ServiceConstants.IdentityTypeGroupId;
                IdentityManager.IdentityContent = TenantLocalValue.LogonGroupId;
                var subsubJobId = _mediaServer.GDriveBackupRequest.JobId;
                var driveId = _mediaServer.GDriveBackupRequest.DriveId;
                try
                {
                    logger.Info($"Start to merge index for google drive.");
                    var jobInfo = new GDriveMergeIndexJobInfo()
                    {
                        IndexLogicalDevice = _mediaServer.GDriveBackupRequest.IndexLogicalDevice,
                        JobDto = new BaseJobDto(),
                        CacheLocation = new CacheSettingDto(),
                        MergeIndexJobsState = new List<MergeIndexJobState>() { new MergeIndexJobState(_configuration.JobId, true) },
                    };

                    var mergeHandler = new GDriveArchiveMergeIndexJobHandler();
                    mergeHandler.PerformMergeIndex(jobInfo, _mediaServer.GDriveBackupRequest);
                    logger.Info($"Success to merge index for google drive.");
                    //await ProcessRetentionAsync(subsubJobId, jobInfo);
                }
                catch (Exception ex)
                {
                    logger.Error($"Error occurred while merging teams index, {ex}");
                    throw;
                }
            }   
        }
        private async Task ProcessRetentionAsync(string subJobId, GDriveMergeIndexJobInfo job)
        {
            try
            {
                ArchiverPruningJob pruningJob = new ArchiverPruningJob();
                pruningJob.FarmName = "";
                pruningJob.JobId = subJobId;
                pruningJob.SiteUrl = "";
                pruningJob.WebApp = "";
                pruningJob.ArchiverBackupTime = 0;
                pruningJob.StoragePolicyId = _configuration.CurrentRule.StoragePolicyId;
                pruningJob.RetentionAction = MediaArchiverRetentionAction.DeleteData;
                pruningJob.RetentionJob = new SOJob() { Id = subJobId };


                pruningJob.DataLogicalDevice = _configuration.CurrentRule.IsEnableRetention ? _configuration.CurrentRule.StoragePolicyDto.PrimaryStorage : new();
                if (pruningJob.DataLogicalDevice.PhysicalDrives?.Count > 0)
                {
                    pruningJob.IndexLogicalDevice = job.IndexLogicalDevice;
                    pruningJob.IsDeleteJob = false;
                    CacheSettingDto cache = new CacheSettingDto();
                    cache.Extension = new CacheSettingExtension();
                    cache.Extension.Path = new List<PathMap>
                                    {
                                        new PathMap() { DiskInfo = new DiskInfoDto() { Path = BackgroundSettings.GetInstance().ArchiveTemp } }
                                    };
                    pruningJob.CacheSettings = cache;
                    var retentionInfo = new ArchiverLifecycleRetentionInfo(pruningJob)
                    {
                        RetentionRule = AvePoint.Media.Service.DomainModel.RetentionRule.RetainArchiverJobData,
                        Rules = new Dictionary<int, Rule> { { 0, _configuration.CurrentRule } },
                        JobType = (int)JobType.GoogleRecordsDisposal,
                        NodeLevel = (int)_configuration.SelectedNode.Level
                    };
                    var retentionService = new GDriveArchiverLifecycleRetentionService();
                    var result = retentionService.Retain(retentionInfo, new Action<JMJobDetails>(SendRetentionJobReport)) as GDriveArchiverLifecycleRetentionResult;
                    //analyze result
                    //todo
                }
                else
                {
                    logger.Info("There aren't rules enabled LF retention.");
                }
            }
            catch (Exception e)
            {
                logger.Error(e.Message, e);
            }

        }
        private void SendRetentionJobReport(JMJobDetails details)
        {
            //todo 
        }
        private async Task ExportActionAsync(IEnumerable<ArchiveApproveReport> reader)
        {
            SOGDriveArchiverJobInfoStatistics.Instance.IsNeedStatisticsAction = true;
            var (exportController, backupController) = InitBackupController(_configuration.CurrentRule, null, null, _configuration.Action);
            foreach (var item in reader)
            {
                using (CheckJobStopScope jScope = new CheckJobStopScope())
                {
                    if (IsNeedAddScanReport(item)) InitScanReportJobDetails(item);

                    if (IsHandleExportArchiveReport(item))
                    {
                        await backupController.ProcessArchiveReport(item, null);
                    }
                    else
                    {
                        logger.Info($"Folder or Drive nodeId {item.NodeId} name: {item.LeafName} don't have any file match rule Export");
                    }
                }
            }
        }

        private bool IsNeedAddScanReport(ArchiveApproveReport item)
        {
            if (item.SPNodeLevel == (int)NodeLevel.GoogleSharedDrive || item.SPNodeLevel == (int)NodeLevel.GoogleMyDrive) return false;

            return true;
        }

        private bool IsHandleExportArchiveReport(ArchiveApproveReport item)
        {
            if (item.SPNodeLevel == (int)NodeLevel.GoogleSharedDrive || item.SPNodeLevel == (int)NodeLevel.GoogleMyDrive) return false;
            if (item.CacheNodeType == (int)GoogleCacheNodeType.ItemVersion)
            {
                logger.Info("Ignore item version cache. ItemName: {0}", item.NodeId);
                return false;
            }
            //if (item.SPNodeLevel == (int)NodeLevel.GoogleFolder)
            //{
            //    var googleDataItem = JsonConvert.DeserializeObject<GoogleItemData>(item.JsonMeta) ?? new();
            //    _folderExportCache.Add(googleDataItem);
            //    return false;
            //}

            return true;
        }

        private void InitScanReportJobDetails(ArchiveApproveReport item)
        {
            var ruleName = string.Empty;
            if (item.CacheNodeType == (int)GoogleCacheNodeType.Item || item.CacheNodeType == (int)GoogleCacheNodeType.ItemVersion)
            {
                ruleName = _configuration.CurrentRule?.Name;
            }
            item.AddToReportsByArchiveApproveReport(_configuration.ActionApproveReports, ActionTab.Scan, JobDetailsStatus.Successful, item.DocumentSize, ruleName, string.Empty);
        }

        private void ReportJobDetails()
        {
            if (_configuration.ActionApproveReports.TryGetValue(ActionTab.Scan, out var scanReports))
            {
                foreach (var report in scanReports)
                {
                    _configuration.ReportCenter.AddScanReport(report, report.Level);
                }
            }
            if (_configuration.ActionApproveReports.TryGetValue(ActionTab.Backup, out var backupReports))
            {
                foreach (var report in backupReports)
                {
                    _configuration.ReportCenter.AddBackupReport(report, report.Level);
                }
            }
            if (_configuration.ActionApproveReports.TryGetValue(ActionTab.Action, out var actionReports))
            {
                foreach (var report in actionReports)
                {
                    _configuration.ReportCenter.AddDeletionReport(report, report.Level);
                }
            }
            if (_configuration.ActionApproveReports.TryGetValue(ActionTab.Export, out var exportReports))
            {
                foreach (var report in exportReports)
                {
                    _configuration.ReportCenter.AddExportReport(report, report.Level);
                }
            }
        }

        private async Task MoveActionAsync(IEnumerable<ArchiveApproveReport> reader)
        {
            SOArchiverJobInfoStatistics.Instance.IsNeedStatisticsAction = true;
            var (exportController, backupController) = InitBackupController(_configuration.CurrentRule, null, null, ActionType.Move);
            foreach (var item in reader)
            {
                using (CheckJobStopScope jScope = new CheckJobStopScope())
                {
                    using (new PerformanceScope("GoogleRecordsDisposal.MoveActionAsync"))
                    {
                        try
                        {
                            InitScanReportJobDetails(item);
                            if (item.CacheNodeType == (int)GoogleCacheNodeType.ItemVersion)
                            {
                                logger.Info("Ignore item version cache. ItemName: {0}", item.NodeId);
                                continue;
                            }
                            await backupController.ProcessArchiveReport(item, null);
                        }
                        catch (Exception ex)
                        {
                            logger.Error($"An error occurred while processing move action for item [{item.ToString()}]. Error: {ex}");
                        }
                    }
                }
            }
        }
        private async Task DeleteActionAsync(IEnumerable<ArchiveApproveReport> reader)
        {
            SOGDriveArchiverJobInfoStatistics.Instance.IsNeedStatisticsAction = true;
            //SOGDriveArchiverJobInfoStatistics.Instance.IsDeleteOnlyActionOrRestore = true;
            SOGDriveArchiverJobInfoStatistics.Instance.IsDeleteArchiveAction = true;

            var (exportController, backupController) = InitBackupController(_configuration.CurrentRule, null, null, _configuration.Action);
            foreach (var item in reader)
            {
                using (CheckJobStopScope jScope = new CheckJobStopScope())
                {
                    using (new PerformanceScope("GoogleRecordsDisposal.MoveActionAsync"))
                    {
                        try
                        {
                            if (item.CacheNodeType == (int)GoogleCacheNodeType.ItemVersion)
                            {
                                logger.Info("Ignore item version cache. ItemName: {0}", item.NodeId);
                                continue;
                            }
                            InitScanReportJobDetails(item);
                            if (_configuration.Action == ActionType.ExportBeforeDel && exportController != null)
                            {
                                if (IsHandleExportArchiveReport(item))
                                {
                                    await exportController.ProcessArchiveReport(item, null);
                                }
                                else
                                {
                                    logger.Info($"Folder or Drive nodeId {item.NodeId}  don't have any file match rule Export");
                                }
                            }

                            await backupController.ProcessArchiveReport(item, null);
                        }
                        catch (Exception ex)
                        {
                            logger.Error($"An error occurred while processing move action for item [{item.ToString()}]. Error: {ex}");
                        }
                    }
                }
            }
        }
        private async Task EndWork()
        {
            var permissionDestinationDrive = ReportCenter.GetPermissionInfoInDestinationDrive();
            if (permissionDestinationDrive.Count > 0)
            {
                RMGoogleDiscoverBase discoverBase = new(null);
                discoverBase.Init(appProfile);
                await discoverBase.DeletePermissionToRuleDrive(permissionDestinationDrive);
            }

            var newAssignees = ManualApprovalWorkflowManager.GetCachedGControlWorkflowForUserTaskMappings();
            var newAssigneeIds = ManualApprovalWorkflowManager.GetNewUserTaskMapping(newAssignees);
            var existedAssignees = (await _gControlPlatformTaskAssigneeService.GetCurrentPlatformTaskAssignees()) ?? [];
            var userIdsToAdd = newAssigneeIds.Except(existedAssignees.Select(a => a.AssigneeId)).ToList();
            var result = await _gControlPlatformTaskAssigneeService.AddPlatformTaskAssigneesAsync(userIdsToAdd);
            if (!result)
            {
                await _taskAssigneeService.BatchAddAsync(newAssignees);
            }
        }


        #region April 2025

        private void InitConfiguration(GoogleDriveTreeNodeDto selectedNode, RMGoogleSetting googleSetting)
        {
            _configuration.AppProfile = appProfile;
            _configuration.JobId = jobId;
            _configuration.ReportCenter = ReportCenter;
            _configuration.RecordManager = RecordManager;
            _configuration.SelectedNode = selectedNode;
            _configuration.GoogleSetting = googleSetting;
            _configuration.RuleManager = RuleManager;
        }

        private (BaseBackupController? exportBeforeDel, BaseBackupController baseController) InitBackupController(Rule currentRule, RMTerm currentTerm, Record? record, ActionType actionType)
        {
            var config = new GoogleConfiguration()
            {
                AppProfile = appProfile,
                JobId = jobId,
                ReportCenter = ReportCenter,
                RecordManager = RecordManager,
                SelectedNode = _configuration.SelectedNode,
                CurrentRule = currentRule,
                CurrentTerm = currentTerm,
                RuleManager = _configuration.RuleManager,
                GoogleSetting = _configuration.GoogleSetting,
                ActionApproveReports = _configuration.ActionApproveReports
            };
            switch (actionType)
            {
                case ActionType.Move:
                    return (null, new MoveToController(config));

                case ActionType.DeleteOnly:
                    return (null, new DeleteOnlyController(config, null));

                case ActionType.ExportOnly:
                    return (null, new ExportOnlyController(config, _exportBeforeArcInfo));

                case ActionType.ExportBeforeDel:
                    var exportOnlyController = new ExportOnlyController(config, _exportBeforeArcInfo);
                    var delOnlyController = new DeleteOnlyController(config, null);
                    return (exportOnlyController, delOnlyController);

                case ActionType.ArchiveToStorage:
                    var archiveController = new ArchiveController(config);
                    return (null, archiveController);

                case ActionType.ExportBeforeArchive:
                    var exportBeforeArchiveController = new ExportOnlyController(config, _exportBeforeArcInfo);
                    var archiveToStorageController = new ArchiveController(config);
                    return (exportBeforeArchiveController, archiveToStorageController);

                default:
                    throw new NotSupportedException("Error when init backup controller");
            }
        }

        private ActionType GetActionType(Rule? rule)
        {
            if (rule == null) return ActionType.None;

            var gooogleDriveRule = rule.GoogleDriveRule;
            if (gooogleDriveRule.ExportInfo is { exportSPDataOption: ExportSPDataOption.ExportWithoutArchive })
            {
                return ActionType.ExportOnly;
            }
            else if (gooogleDriveRule is { MoveToRecordCenterAndDelareSetting: not null })
            {
                return ActionType.Move;
            }
            else if (gooogleDriveRule.KeepDataOption == (int)KeepDataOption.Delete)
            {
                if (gooogleDriveRule.ExportInfo?.exportType == ExportTypeValue.NARA)
                {
                    return ActionType.ExportBeforeDel;
                }
                else
                {
                    return ActionType.DeleteOnly;
                }
            }
            else if ((gooogleDriveRule.KeepDataOption & (int)KeepDataOption.NotBackup) == (int)KeepDataOption.NotBackup)
            {
                if (gooogleDriveRule.ExportInfo.exportType == ExportTypeValue.NARA)
                {
                    return ActionType.ExportBeforeDel;
                }
                else
                {
                    return ActionType.DeleteOnly;
                }
            }
            else if ((gooogleDriveRule.KeepDataOption & (int)KeepDataOption.Archive) == (int)KeepDataOption.Archive)
            {
                if (gooogleDriveRule.ExportInfo?.exportType == ExportTypeValue.NARA)
                {
                    return ActionType.ExportBeforeArchive;
                }
                else
                {
                    return ActionType.ArchiveToStorage;
                }
            }
            return ActionType.None;
        }

        #endregion

        private void AddToDestructionCache(string scopeId)
        {
            GoogleLiteDBWrapper wrapper = GoogleLiteDBWrapper.CreateInstance(GooglePathUtil.GetDisposalRecordDBPath(jobId));
            int index = 0;
            int pageSize = 1000;
            bool hasMore = true;
            List<GoogleDestructionData> records = null;
            do
            {
                using (new PerformanceScope("GoogleRecordsDisposal.QueryAllByPage", addToStatistics: true))
                {
                    records = wrapper.QueryAllByPage(index, pageSize, scopeId);
                }
                if (records != null && records.Count > 0)
                {
                    index++;
                    hasMore = true;
                    try
                    {
                        foreach (var record in records)
                        {
                            if (record == null)
                            {
                                continue;
                            }
                            DestructionFactory.GetInstance(scopeId, jobId).InsertValueToDB(new List<DestructionReport>() { GenerateDestructionReport(record) });
                        }
                    }
                    catch (Exception e)
                    {
                        logger.Error($"Error occurred while insert destruction cache. Error: {e.ToString()}");
                    }
                }
                else
                {
                    hasMore = false;
                }
            } while (hasMore);
        }

        private void UploadDestructionCache()
        {
            try
            {
                AddToDestructionCache(scopeId);
                DestructionFactory.GetInstance(scopeId, jobId).UploadToStorage();
            }
            catch (Exception e)
            {
                logger.Error($"Error occurred while uploading destruction cache. Error: {e.ToString()}");
            }
        }

        private DestructionReport GenerateDestructionReport(GoogleDestructionData data)
        {
            return new DestructionReport()
            {
                NodeId = data.ScopeId,
                ArchivedTime = data.DestroyedTime,
                RuleID = new Guid(data.RuleId),
                SortTicks = Snowflake.Instance().GetTicks().ToString(),
                JsonMeta = data.MetaInfo,
                FullPath = data.FullPath,
                ActionType = (int)ActionType.DeleteOnly,
            };
        }

        private bool IsSkipProcess(Record record, GoogleItemData item, Rule matchedRule)
        {
            if (record.HoldStatus && record.HoldReleaseTime > DateTime.UtcNow.Ticks)
            {
                logger.Warn($"Item [{record.Id}] is RecordsHold.");
                ReportCenter.RecordSkipCommon(item.GenerateDisposalActionJobDetail(
                    I18NResource.RemoveAndDestroyAction, matchedRule.Name,
                    I18NResource.FileOnHold), (int)item.Level);
                return true;
            }

            if (record.DisposalDueDate > DateTime.UtcNow.Ticks)
            {
                logger.Warn($"The item [{item.Id}] has not reached action due date yet.");
                ReportCenter.RecordSkipCommon(item.GenerateDisposalActionJobDetail(string.Empty,
                    matchedRule.Name,
                    I18NResource.NotYetDueDate), (int)item.Level);
                return true;
            }

            return false;
        }
        private void InitExportType(Rule rule, string driveName)
        {
            try
            {
                var (generator, export) = InitVaultState(rule, driveName);

                _exportBeforeArcInfo = new GoogleExportBeforeArcInfo()
                {
                    GoogleExport = export,
                    GoogleExportPathGenerator = generator
                };
            }
            catch (Exception ex)
            {
                logger.Warn($"InitExportType fail. Error {ex}");
            }
        }
        private (GoogleExportPathGeneratorBase generator, IGoogleExport export) InitVaultState(Rule rule, string driveName)
        {
            ExportTypeValue vaultExportType = rule.GoogleDriveRule.ExportInfo.exportType;
            PhysicalDeviceDto physicalDto = rule.GoogleDriveRule.PhysicalDeviceDto;
            logger.Info("Google Export Type is: {0}.", vaultExportType.ToString());
            if (physicalDto != null)
            {
                if (vaultExportType == ExportTypeValue.NARA)
                {
                    var googleExport = new GoogleNARAExport(physicalDto, driveName, _configuration.JobId, rule.DisposalClass, _NARAConfigFile);
                    var generator = new GoogleNARAExportPathGenerator(physicalDto.Location, driveName);
                    return (generator, googleExport);
                }
            }
            else
            {
                logger.Info("The Vault Before Archiver is false.");
            }
            return (null, null);
        }
        private void GetExportStorageConfiguration(Rule rule)
        {
            if (rule.GoogleDriveRule?.ExportInfo == null)
            {
                return;
            }

            var physicalDeviceId = string.Empty;
            var exportInfor = rule.GoogleDriveRule.ExportInfo;
            if (exportInfor is { newOptionsOfExportInfo: true })
            {
                physicalDeviceId = exportInfor.exportLocationId;
            }
            else
            {
                SettingProfileDto mDto = new SettingProfileDto()
                {
                    Type = (int)SettingProfilesType.ExportLocationDevice,
                    Name = "UsingExportLocationDevice"
                };
                var dto = SettingProfileDao.Load(mDto);
                if (dto != null)
                {
                    physicalDeviceId = dto.Settings;
                }
            }
            var storageDevice = StorageDeviceService.GetStorageDeviceById(physicalDeviceId, needDecryptSecert: true);
            if (storageDevice != null)
            {
                PhysicalDeviceDto physicalDto = new PhysicalDeviceDto()
                {
                    ConnectionString = storageDevice.ConnectionString,
                    Type = storageDevice.Type,
                };

                rule.GoogleDriveRule.PhysicalDeviceDto = physicalDto;
            }
        }
        private void RebuildStoragePolicyDto(Rule rule)
        {
            var globalSetting = GlobalStorageSettingDao.GetGlobalSettingInfoFromRA();

            if (!string.IsNullOrEmpty(rule.StoragePolicyId))
            {
                var storageDevice = StorageDeviceService.GetStorageDeviceById(rule.StoragePolicyId, needDecryptSecert: true);
                ArchiverCommonStaticMethod.VerifyAvePoint(storageDevice);
                var logical = ArchiverTypeConvert.ConvertStorageDeviceDtoToLogicalDeviceDto(storageDevice);
                rule.StoragePolicyDto = new StoragePolicyDto()
                {
                    Id = storageDevice.Id,
                    Name = rule.Id,
                    PrimaryStorage = logical,
                    Type = storageDevice.Type,
                };
                if (storageDevice.SetupDataRetention)
                {
                    rule.StoragePolicyDto.RetentionOption = StorageDeviceConvert.ConvertToRetentionRuleOption(storageDevice.ArchiveRetentionRules);
                }

                if (globalSetting != null)
                {
                    if (globalSetting.UseCompression)
                    {
                        rule.ArchiverCompressionType = (AvePoint.GCommon.Contract.GranularBackup.Object.CompressionType)globalSetting.CompressionSpeed;
                        rule.ArchiverDataSecurity = rule.ArchiverDataSecurity | AvePoint.GCommon.Contract.GranularBackup.Object.DataSecurity.CompressionMedia;
                    }
                    if (globalSetting.UseEncryption)
                    {
                        storageDevice.EncryptionProfileId = globalSetting.SecurityProfileId.ToString();
                        var encryptionInfo = SettingProfileDao.LoadById(new Guid(storageDevice.EncryptionProfileId));
                        DataEncryptionProfile mProfile = SerializerHelper.DeserializeByDataContractSerializer<DataEncryptionProfile>(encryptionInfo.Settings);

                        if (mProfile.CurrentProtectionAlgorithm != null && mProfile.CurrentProtectionAlgorithm.Type == ProtectionAlgorithmType.TenantMasterKeyEncryptionService)
                        {
                            rule.EncryptionMethods = AvePoint.GCommon.Contract.GranularBackup.Object.EncryptionMethods.AES_ENCRYPTION;
                            rule.ArchiverDataSecurity = rule.ArchiverDataSecurity | AvePoint.GCommon.Contract.GranularBackup.Object.DataSecurity.EncryptionMedia;
                            rule.DataEncryptionProfileId = storageDevice.EncryptionProfileId;
                            rule.DataEncryptionInfoWrapper = new AvePoint.GCommon.Contract.Server.ControlPanel.Cryptography.Wrapper.DataEncryptionInfoWrapper();
                            var info = new AvePoint.GCommon.Contract.Server.ControlPanel.Cryptography.DataEncryptionInfo();
                            byte[] result;
                            result = KeyGenerateProviderFactory.CreateProvider().GenerateKeyBytes(mProfile.KeyLength / 8);
                            info.Checksum = HashAlgorithmFactory.CreateHashAlgorithm(HashAlgorithm.SHA1).ComputeHash(result);
                            info.EncryptionType = mProfile.AlgorithmType;
                            info.ProfileGuid = storageDevice.EncryptionProfileId;
                            info.ProtectionGuid = storageDevice.EncryptionProfileId;
                            info.ProfileName = "Default Encryption Profile";
                            info.EncryptedDynamicKey = AesEncryptorWrapper.Encrypt(result);
                            rule.DataEncryptionInfoWrapper.EncryptionInfo = info;
                            rule.DataEncryptionInfoWrapper.DynamicKey = AvePoint.GCommon.Utility.Cryptography.CspCommunicationWrapper.WrapKeyToBase64String(result);
                        }
                        else
                        {
                            logger.Warn("Not the desired encryption method.");
                            if (mProfile.CurrentProtectionAlgorithm != null)
                            {
                                logger.Warn("CurrentProtectionAlgorithm is null.");
                            }
                            else
                            {
                                logger.Warn($"CurrentProtectionAlgorithm Type is {mProfile.CurrentProtectionAlgorithm.Type}.");
                            }
                        }
                    }
                }
            }
        }
        private void InitNARAConfigFile()
        {
            try
            {
                var naraExportSetting = ExportSettingsDao.GetExportSetting((int)ExportSettingType.NARA, (int)SourceFlag.Google);
                if (naraExportSetting != null)
                {
                    _NARAConfigFile = naraExportSetting.ExportConfig;
                }
                else
                {
                    var filepath = Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, "Config", "NARA Configuration File.zip");
                    var unZipFolder = Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, "Temp", "Config", "NARA Configuration File");
                    AvePoint.GCommon.ZipUtil.UnZipFile(filepath, unZipFolder);

                    _NARAConfigFile = GetMemoryStream(unZipFolder, "Google NARA Configuration File.xml");

                }
            }
            catch (Exception e)
            {
                logger.Warn("set NARA export setting when run job error {0}", e.ToString());
            }
        }
        private byte[] GetMemoryStream(string unZipFolder, string fileName)
        {
            using (FileStream fs = new FileStream(Path.Combine(unZipFolder, fileName), FileMode.Open))
            {
                using (MemoryStream ms = new MemoryStream())
                {
                    fs.CopyTo(ms);
                    return ms.ToArray();
                }
            }
        }

        private void GetExportEncryption(Rule rule)
        {
            var exportEncryptionEnabled = RMKeyValueDao.IsExportDataEncryptionEnabled();
            if (exportEncryptionEnabled)
            {
                var keyIV = ExportDataEncryptionSettingService.GetCurrentAesKey().Extension;
                if (!string.IsNullOrWhiteSpace(keyIV) && keyIV.IndexOf("|") > 0)
                {
                    rule.GoogleDriveRule.ExportDataEncryptionKey = CspCrossPlatformExchangeWrapper.WrapKeyToBase64String(Encoding.UTF8.GetBytes(keyIV.Split('|')[0]));
                    rule.GoogleDriveRule.ExportDataEncryptionIV = CspCrossPlatformExchangeWrapper.WrapKeyToBase64String(Encoding.UTF8.GetBytes(keyIV.Split('|')[1]));
                }
                else
                {
                    throw new Exception("Export data encryption is enabled, but we cannot valid encryption key.");
                }
            }
        }
        public void FinishExport()
        {
            try
            {
                if (_naraMetadatas.Any())
                {
                    try
                    {
                        logger.Info("begin build nara metadata file");
                        _naraMetadatas.Values.ForEach(x =>
                        {
                            var googleExport = x.GoogleExport;
                            googleExport.HandleCSVMetadataFolder();
                            List<CsvMetaData> metaData = new List<CsvMetaData>();
                            metaData.AddRange(googleExport.SortCSVMetadata());
                            if (metaData.Count > 0)
                            {
                                googleExport.ExtensionMethod(metaData);
                            }

                            logger.Info("build nara metadata file success.metaData Count:{0}.", metaData.Count);
                            googleExport.Dispose();
                        });
                    }
                    catch (Exception ex)
                    {
                        logger.Error("Failed to finish export, Message: {0}", ex);
                    }
                }

            }
            catch (Exception ex)
            {

            }
        }
    }
}
