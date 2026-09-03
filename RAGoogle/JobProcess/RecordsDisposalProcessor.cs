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
using AvePoint.Cryptography;
using AvePoint.GCommon.Contract.Storage.Entity;
using AvePoint.GCommon.Contract.StorageOptimization.Object;
using AvePoint.GCommon.Contract.Tree;
using AvePoint.GCommon.Contract.Tree.Object;
using AvePoint.GCommon.Media.StorageService;
using AvePoint.RA.Common;
using AvePoint.RA.Common.Util;
using AvePoint.RA.Contract;
using AvePoint.RA.Contract.Exceptions;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.Contract.JobMonitor;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RMWeb.CP;
using AvePoint.RA.Contract.RMWeb.StorageDevice;
using AvePoint.RA.Contract.RMWeb.Tree.Base;
using AvePoint.RA.DB.Core;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Dao.Extension;
using AvePoint.RA.DB.Explorer.Model;
using AvePoint.RA.DB.Model;
using AvePoint.Records.Core.Utilities.Extensions;
using RAArchiverCommon.DestructionCache;
using RAGoogle.Common;
using RAGoogle.Extension;
using RAGoogle.GoogleObjDiscover;
using RAGoogle.Helper;
using RAGoogle.ManualManagement;
using RAGoogle.Models;
using RAGoogle.Models.Contract;
using RAGoogle.Models.Enums;
using RAGoogle.RecordsDisposal;
using RAGoogle.RecordsDisposal.Action.DeleteOnly;
using RAGoogle.RecordsDisposal.Action.ExportOnly;
using RAGoogle.RecordsDisposal.Action.MoveTo;
using RAGoogle.Services;
using RAGoogle.Util;
using System.Collections.Concurrent;
using System.Text;
using Util;

namespace RAGoogle.JobProcess
{
    public class RecordsDisposalProcessor : BaseProcessor
    {
        #region propreties

        private readonly GoogleManualManagement _manualManagement;

        private readonly GoogleConfiguration _configuration;
        private const string GOOGLE = "GOOGLE";
        // private IGoogleExport _googleExport = null;
        //private GoogleExportBeforeArcInfo _googleExportBefArcInfo = null;
        private IStorageDeviceService StorageDeviceService => PlatformWindsorManager.GetService<IStorageDeviceService>();
        private ISettingProfilesDao SettingProfileDao => PlatformWindsorManager.GetService<ISettingProfilesDao>();
        public IExportSettingsDao ExportSettingsDao => (IExportSettingsDao)PlatformWindsorManager.GetService(typeof(IExportSettingsDao));
        public IRMKeyValueDao RMKeyValueDao => (IRMKeyValueDao)PlatformWindsorManager.GetService(typeof(IRMKeyValueDao));
        public IExportDataEncryptionSettingService ExportDataEncryptionSettingService => (IExportDataEncryptionSettingService)PlatformWindsorManager.GetService(typeof(IExportDataEncryptionSettingService));

        //private ActionType _actionType = ActionType.None;
        private byte[] _NARAConfigFile { get; set; }
        private ConcurrentBag<GoogleItemData> AllFolderCache = [];
        private ConcurrentDictionary<string, GoogleExportBeforeArcInfo> AllExportRulesCache { get; set; } = new();

        private NonClassificationProcessor _nonClassficationProcessor;

        protected override bool NeedScanVersion => true;
        #endregion

        public RecordsDisposalProcessor(string jobId) : base(jobId, JobType.GoogleRecordsDisposal)
        {
            ReportCenter.InitCurrentJobInfo(jobId, JobType.GoogleRecordsDisposal);
            _manualManagement = new();
            _configuration = new();
        }

        public async override Task RunNowAsync(RMGoogleSetting? setting, GoogleDriveTreeNodeDto? node)
        {
            using (var performance = new PerformanceScope("EnforceRuleProcessor.RunNowAsync"))
            {
                try
                {
                    AllFolderCache.Clear();
                    AllExportRulesCache.Clear();
                    using (CheckJobStopScope jScope = new())
                    {
                        if (setting is null || node is null)
                        {
                            logger.Error("Setting node and Node info are invalid.");
                            throw new ArgumentNullException("Setting node and Node info are invalid.");
                        }
                        InitConfiguration(node, setting);
                        InitNARAConfigFile();
                        var itemQueue = new DataQueue<GoogleItemData>();
                        var task = Task.Run(() => ProcessItemDataAsync(itemQueue, node, setting));
                        if (RecordManager.IsLoadedCache(node))
                        {
                            RecordManager.LoadRuleActionCache();
                        }
                        _manualManagement.Build(RecordManager, ReportCenter, jobId);
                        if (setting.IsNullClassificationSetting)
                        {
                            _nonClassficationProcessor = new(_manualManagement, _configuration, AllExportRulesCache);
                            await _nonClassficationProcessor.InitializeAsync(_NARAConfigFile);
                        }
                        await ProcessDiscoveryAsync(node, setting, itemQueue);
                        itemQueue.Complete();
                        task.Wait();
                        FinishExport();
                        await EndWork();
                        UploadDestructionCache();
                    }
                }
                catch (JobStopException)
                {
                    ReportCenter.JobHasStopped = true;
                }
                catch (Exception ex)
                {
                    logger.Error("Failed to kick off google records disposal job, Message: {0}", ex);
                    throw;
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
        }

        private async Task ProcessDiscoveryAsync(GoogleDriveTreeNodeDto node, RMGoogleSetting setting, DataQueue<GoogleItemData> itemQueue)
        {
            using (var performance = new PerformanceScope("RecordsDisposalProcessor.ProcessrecordsDisposalAsync"))
            using (CheckJobStopScope jScope = new CheckJobStopScope())
            {
                try
                {
                    setting.RunAutoFullJob = true;
                    await ProcessDiscoveryItemsData(node, setting, itemQueue);
                }
                catch (JobStopException)
                {
                    logger.Warn("The records disposal job has been stopped.");
                    throw new JobStopException("The job has stopped."); ;
                }
                catch (Exception ex)
                {
                    logger.Error($"Failed to process records disposal job, Message: {ex}");
                    throw;
                }
            }
        }

        private async Task ProcessItemDataAsync(DataQueue<GoogleItemData> itemQueue, GoogleDriveTreeNodeDto selectedNode, RMGoogleSetting setting)
        {
            using (CheckJobStopScope jScope = new())
            {
                await itemQueue.ToIEnumerable().ParallelExecute(async item =>
                {
                    try
                    {
                        using (CheckJobStopScope subJScope = new CheckJobStopScope())
                        {
                            using (new PerformanceScope("GoogleRecordsDisposal:ProcessDataItemAsync"))
                            {
                                if (item.Level == RMNodeLevel.GoogleFolder)
                                {
                                    AllFolderCache.Add(item);
                                }
                                if (item.Level == RMNodeLevel.GoogleFile)
                                {
                                    await PreProcessAsync(item, selectedNode, setting);
                                }
                            }
                        }
                    }
                    catch (JobStopException)
                    {
                        logger.Warn("The records disposal job has been stopped.");
                        throw new JobStopException("The job has stopped.");
                    }
                    catch (Exception ex)
                    {
                        logger.Error($"An error occurred while process item [{item.Id}]. Error: {ex}");
                        ReportCenter.RecordFailedCommon(item.GenerateDisposalActionJobDetail(I18NResource.RemoveAndDestroyAction, string.Empty,
                ex.Message), (int)item.Level);
                    }
                }, MaxDegreeOfParallelism, Cts.Token);
            }
        }

        private Record? ProcessRecordItemManager(GoogleItemData item, GoogleDriveTreeNodeDto selectedNode, Rule? matchedRule, RMTerm? rmTerm)
        {
            try
            {
                using (CheckJobStopScope jScope = new())
                {
                    int createdDate = (int)DateTime.UtcNow.Ticks;
                    bool isProcess = false;
                    if (RecordManager.TryGetRecordValue(item.UniqueId, createdDate, out Record existRecord))
                    {
                        if (rmTerm == null)
                        {
                            logger.Info("Not found any label associated with matched rule applied on item. itemId: {0}", item.Id);
                            existRecord.TermId = Guid.Empty;
                            existRecord.TermName = string.Empty;
                            existRecord.RuleId = Guid.Empty;
                            if (existRecord.ManualApprovedStatus != (int)SOApproveDBStatus.None)
                            {
                                logger.Info("The item change with no matched rule and item is in manual process, remove manual properties. Item id: {0}", item.Id);
                                ReportCenter.RecordSkipCommon(item.GenerateDisposalActionJobDetail(
                                        I18NResource.RemoveAndDestroyAction, string.Empty,
                                        I18NResource.NoMatchedRule), (int)item.Level);
                                existRecord.RemoveManualProperties();
                            }
                        }
                        else
                        {
                            existRecord.TermId = rmTerm.UniqueId;
                            existRecord.TermName = rmTerm.Name;
                            var oldRuleId = existRecord.RuleId.ToString();
                            if (matchedRule == null && oldRuleId != Guid.Empty.ToString())
                            {
                                logger.Info("Rule changed and not matched. itemId: {0}, old rule id: {1}", item.Id, oldRuleId);
                                if (existRecord.ManualApprovedStatus != (int)SOApproveDBStatus.None)
                                {
                                    logger.Info("The item change with no matched rule and item is in manual process, remove manual properties. Item id: {0}", item.Id);
                                    ReportCenter.RecordSkipCommon(item.GenerateDisposalActionJobDetail(
                                            I18NResource.RemoveAndDestroyAction, string.Empty,
                                            I18NResource.NoMatchedRule), (int)item.Level);
                                    existRecord.RemoveManualProperties();
                                }
                                existRecord.RuleId = Guid.Empty;
                            }

                            if (matchedRule != null)
                            {
                                if (!oldRuleId.Eq(matchedRule.Id))
                                {
                                    logger.Info("Rule changed and matched. itemId: {0}, new rule id: {1}", item.Id, matchedRule.Id);
                                    existRecord.RuleId = new Guid(matchedRule.Id);
                                    if (existRecord.ManualApprovedStatus != (int)SOApproveDBStatus.None)
                                    {
                                        logger.Info("The item change with new matched rule and item is in manual process, remove manual properties. Item id: {0}", item.Id);
                                        ReportCenter.RecordSkipCommon(item.GenerateDisposalActionJobDetail(
                                            I18NResource.RemoveAndDestroyAction, matchedRule.Name,
                                            I18NResource.NewMatchedRule), (int)item.Level);
                                        existRecord.RemoveManualProperties();
                                    }
                                }
                                isProcess = true;
                            }
                        }
                        RecordManager.UpdateRecordInfo(existRecord, item);
                        RecordManager.UpdateManualProperties(existRecord, true);
                    }
                    else
                    {
                        if (rmTerm == null || matchedRule == null || !matchedRule.GoogleDriveRule.IsManualApproval)
                        {
                            logger.Info("Item does not match rule criteria or does not enabel manual approval. Skip to generate new record. itemId: {0}", item.Id);
                            return null;
                        }
                        existRecord = item.ConvertToRecord(selectedNode, existRecord);
                        existRecord.RuleId = new Guid(matchedRule.Id);
                        existRecord.TermId = rmTerm.UniqueId;
                        existRecord.TermName = rmTerm.Name;
                        RecordManager.AddNewRecord(existRecord);
                        isProcess = true;
                    }

                    if (!isProcess)
                    {
                        return null;
                    }
                    return existRecord;
                }
            }
            catch (JobStopException)
            {
                logger.Warn("the job has stopped.");
                throw new JobStopException("The job has stopped.");
            }
            catch (Exception ex)
            {
                logger.Error($"An error occurred while process item [{item.Id}]. Error: {ex}");
                throw;
            }

        }

        #region April 2025

        private async Task PreProcessAsync(GoogleItemData item, GoogleDriveTreeNodeDto selectedNode,
            RMGoogleSetting setting)
        {
            try
            {
                using (CheckJobStopScope jScope = new())
                {
                    using (GoogleDriveService service = new(appProfile, item.MemberEmail))
                    {
                        bool isNullClassification = setting.IsNullClassificationSetting;
                        if (isNullClassification)
                        {
                            await _nonClassficationProcessor.ProcessAsync(item);
                            return;
                        }
                        (Rule? matchedRule, RMTerm? rmTerm) = CalculateMatchedPotentialRule(item, selectedNode, setting);

                        var actionType = GetActionType(matchedRule);

                        var record = actionType is ActionType.DeleteOnly or ActionType.ExportBeforeDel ? ProcessRecordItemManager(item, selectedNode, matchedRule, rmTerm) : null;

                        if (matchedRule == null || rmTerm == null)
                        {
                            logger.Warn($"The item {item.Id} does not match any rule");
                            return;
                        }
                        if (actionType is ActionType.ExportOnly or ActionType.ExportBeforeDel)
                        {
                            if (!AllExportRulesCache.TryGetValue(matchedRule.GoogleDriveRule.ExportInfo.exportLocationId, out var googleExportInfo))
                            {
                                matchedRule.GoogleDriveRule.NARAConfigFile = _NARAConfigFile;
                                GetExportStorageConfiguration(matchedRule);
                                GetExportEncryption(matchedRule);
                                var executor = InitExportType(item, matchedRule);
                                if (executor == null)
                                {
                                    return;
                                }
                                AllExportRulesCache.TryAdd(matchedRule.GoogleDriveRule.ExportInfo.exportLocationId, executor);
                            }
                        }


                        if (record != null && IsSkipProcess(record, item, matchedRule))
                        {
                            return;
                        }

                        var (exportBeforeController, backupController) = InitBackupController(matchedRule, rmTerm, record, actionType);
                        var settingInfo = ConvertHelper.ConvertRMSetting2Dto(setting);
                        if (record == null || !await _manualManagement.IsNeedProcessManualDisposalAsync(matchedRule,
        settingInfo, record))
                        {
                            if (exportBeforeController != null)
                            {
                                await exportBeforeController.Process(item);
                            }
                            await backupController.Process(item);
                        }
                    }
                }
            }
            catch (JobStopException)
            {
                logger.Warn("The job has stopped.");
                throw new JobStopException("The job has stopped.");
            }
            catch (Exception ex)
            {
                logger.Warn($"An error occurred while pre process action [{item.Name}]. Error: {ex}");
                ReportCenter.RecordFailedCommon(item.GenerateDisposalActionJobDetail(I18NResource.RemoveAndDestroyAction, string.Empty,
                I18NResource.DeleteItemFailed), (int)item.Level);
                throw;
            }
        }

        private (Rule? rule, RMTerm? term) CalculateMatchedPotentialRule(GoogleItemData item, GoogleDriveTreeNodeDto selectedNode,
            RMGoogleSetting setting)
        {
            try
            {
                using (CheckJobStopScope jScope = new())
                {
                    using (GoogleDriveService service = new(appProfile, item.MemberEmail))
                    {
                        var itemInfo = item.ConvertToInfo();
                        Tuple<Rule, TimeSpan>? matchedRule = null;
                        int matchedTermId = -1;
                        List<int> aveLabelIds = [];
                        Dictionary<int, List<Rule>>? associatedRules = null;

                        foreach (var label in item.MetaInfo.Labels)
                        {
                            associatedRules = RuleManager.GetAssociatedRuleAsync(label.Id, selectedNode.TenantId, true);
                            if (associatedRules.IsNullOrEmpty())
                            {
                                logger.Warn($"Not found any associated rules label, labelId: {label.Id}");
                                continue;
                            }
                            matchedTermId = associatedRules.FirstOrDefault().Key;
                            foreach (var associatedRule in associatedRules)
                            {
                                matchedRule = RuleManager.MatchedPotentialRule(itemInfo, associatedRule.Value);
                                if (matchedRule.Item1 != null)
                                {
                                    matchedTermId = associatedRule.Key;
                                    break;
                                }
                            }
                            if (matchedRule?.Item1 != null && matchedTermId > 0)
                            {
                                break;
                            }
                        }

                        RMTerm? rmTerm = null;
                        if (matchedTermId > 0)
                        {
                            rmTerm = TermDao.GetRMTermByTermId(matchedTermId);
                        }

                        return (matchedRule?.Item1, rmTerm);
                    }
                }
            }
            catch (JobStopException)
            {
                logger.Warn("The job has stopped.");
                throw new JobStopException("The job has stopped.");
            }
            catch (Exception ex)
            {
                logger.Warn($"An error occurred while calculate matched rule [{item.Name}]. Error: {ex}");
                ReportCenter.RecordFailedCommon(item.GenerateDisposalActionJobDetail(I18NResource.RemoveAndDestroyAction, string.Empty,
                I18NResource.DeleteItemFailed), (int)item.Level);
                throw;
            }
        }

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
                GoogleSetting = _configuration.GoogleSetting
            };
            switch (actionType)
            {
                case ActionType.Move:
                    return (null, new MoveToController(config));

                case ActionType.DeleteOnly:
                    return (null, new DeleteOnlyController(config, record));

                case ActionType.ExportOnly:
                    return (null, new ExportOnlyController(config, AllExportRulesCache[currentRule.GoogleDriveRule.ExportInfo.exportLocationId]));

                case ActionType.ExportBeforeDel:
                    var exportOnlyController = new ExportOnlyController(config, AllExportRulesCache[currentRule.GoogleDriveRule.ExportInfo.exportLocationId]);
                    var delOnlyController = new DeleteOnlyController(config, record);
                    return (exportOnlyController, delOnlyController);

                default:
                    throw new NotSupportedException("Error when init backup controller");
            }
        }

        private ActionType GetActionType(Rule? rule)
        {
            if (rule == null)
            {
                return ActionType.DeleteOnly;
            }
            if (rule.GoogleDriveRule.ExportInfo is { exportSPDataOption: ExportSPDataOption.ExportWithoutArchive })
            {
                return ActionType.ExportOnly;
            }
            if (rule.GoogleDriveRule.ExportInfo is { exportSPDataOption: ExportSPDataOption.ExportBeforeArchive })
            {
                return ActionType.ExportBeforeDel;
            }
            return rule.GoogleDriveRule is { MoveToRecordCenterAndDelareSetting: not null } ? ActionType.Move : ActionType.DeleteOnly;
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
        private GoogleExportBeforeArcInfo? InitExportType(GoogleItemData item, Rule rule)
        {
            try
            {
                var (generator, export) = InitVaultState(rule, item);

                return new GoogleExportBeforeArcInfo()
                {
                    GoogleExport = export,
                    GoogleExportPathGenerator = generator
                };
            }
            catch (Exception ex)
            {
                logger.Warn($"InitExportType fail. Error {ex}");
                ReportCenter.RecordFailedCommon(item.GenerateDisposalActionJobDetail(I18NResource.ExportAction, rule.Name, ex.Message), (int)item.Level);
                return null;
            }
        }
        private (GoogleExportPathGeneratorBase generator, IGoogleExport export) InitVaultState(Rule rule, GoogleItemData item)
        {
            ExportTypeValue vaultExportType = rule.GoogleDriveRule.ExportInfo.exportType;
            PhysicalDeviceDto physicalDto = rule.PhysicalDeviceDto;
            logger.Info("Google Export Type is: {0}.", vaultExportType.ToString());
            if (physicalDto != null)
            {
                if (vaultExportType == ExportTypeValue.NARA)
                {
                    string driveName = _configuration.SelectedNode.Level switch
                    {
                        NodeLevel.GoogleMyDrive => item.MemberEmail,
                        NodeLevel.GoogleSharedDrive => item.DriveName
                    };
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

                rule.PhysicalDeviceDto = physicalDto;
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
            if (AllExportRulesCache.Any())
            {
                try
                {
                    logger.Info("begin build nara metadata file");
                    AllExportRulesCache.Values.ForEach(x =>
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
    }
}
