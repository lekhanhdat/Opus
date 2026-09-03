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
using AvePoint.RA.Common;
using AvePoint.RA.Common.Util;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract;
using AvePoint.RA.Contract.DocAve;
using AvePoint.RA.Contract.Exceptions;
using AvePoint.RA.Contract.RMRuleManageMent;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RMWeb.StorageDevice;
using AvePoint.RA.Contract.Services;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Dao.Extension;
using AvePoint.RA.DB.Explorer.Model;
using AvePoint.RA.DB.Model;
using AvePoint.Records.Core.Utilities.Extensions;
using RAGoogle.Common;
using RAGoogle.Extension;
using RAGoogle.Helper;
using RAGoogle.ManualManagement;
using RAGoogle.Models;
using RAGoogle.Models.Enums;
using RAGoogle.RecordsDisposal.Action.DeleteOnly;
using RAGoogle.RecordsDisposal.Action.ExportOnly;
using RAGoogle.RecordsDisposal.Action.MoveTo;
using RAGoogle.Report;
using RAGoogle.Services;
using RAGoogle.Util;
using System.Collections.Concurrent;
using System.Text;
using ExportTypeValue = AvePoint.GCommon.Contract.StorageOptimization.Object.ExportTypeValue;

namespace RAGoogle.RecordsDisposal
{
    public class NonClassificationProcessor
    {
        private readonly IRALogger _logger = RALogger.GetInstance(typeof(NonClassificationProcessor));

        private readonly GoogleDriveTreeNodeDto _selectedNode;

        private readonly RMGoogleSetting _googleSetting;

        private readonly Dictionary<int, Rule> _rules;

        private readonly RuleManager _ruleManager;

        private readonly RecordManager _recordManager;

        private readonly ReportCenter _reportCenter;

        private readonly GoogleConfiguration _googleConfiguration;

        private readonly GoogleManualManagement _manualManagement;

        private byte[] _naraConfigFile;

        private ConcurrentDictionary<string, GoogleExportBeforeArcInfo> _allExportRulesCache;

        private readonly IRuleManagerService _ruleManagerService = PlatformWindsorManager.GetService<IRuleManagerService>();

        private readonly IRMGoogleSettingDao _googleSettingDao = PlatformWindsorManager.GetService<IRMGoogleSettingDao>();

        private readonly IRMKeyValueDao _keyValueDao = (IRMKeyValueDao)PlatformWindsorManager.GetService(typeof(IRMKeyValueDao));

        private readonly IExportDataEncryptionSettingService _exportDataEncryptionSettingService = (IExportDataEncryptionSettingService)PlatformWindsorManager.GetService(typeof(IExportDataEncryptionSettingService));

        private readonly IStorageDeviceService _storageDeviceService = PlatformWindsorManager.GetService<IStorageDeviceService>();

        private readonly ISettingProfilesDao _settingProfileDao = PlatformWindsorManager.GetService<ISettingProfilesDao>();


        public NonClassificationProcessor(GoogleManualManagement googleManualManagement, GoogleConfiguration configuration, ConcurrentDictionary<string, GoogleExportBeforeArcInfo> allExportRulesCach)
        {
            this._selectedNode = configuration.SelectedNode;
            this._googleSetting = configuration.GoogleSetting;
            this._rules = new();
            this._googleConfiguration = configuration;
            this._ruleManager = configuration.RuleManager;
            this._recordManager = configuration.RecordManager;
            this._reportCenter = configuration.ReportCenter;
            this._allExportRulesCache = allExportRulesCach;
            this._manualManagement = googleManualManagement;
        }

        public async Task InitializeAsync(byte[] naraConfigFile)
        {
            var ruleIds = await GetNullClassificationRuleIdsAsync();
            var allGoogleRules = _ruleManagerService.GetRulesByIds(ruleIds);
            int key = 1;
            ruleIds.ForEach(id =>
            {
                var rule = allGoogleRules.FirstOrDefault(r => r.Id == id.ToString());
                if (rule != null)
                {
                    try
                    {
                        RebuildRecordsMoveSetting(rule);
                        _rules.Add(key, rule);
                    }
                    catch (Exception ex)
                    {
                        _logger.Error("Rebuild disposal rules '{0}' error. Inner exception: {1}", rule.Name, ex.ToString());
                    }
                }
                key++;
            });
            _naraConfigFile = naraConfigFile;
        }

        public async Task ProcessAsync(GoogleItemData item)
        {
            try
            {
                using CheckJobStopScope jScope = new();
                using var performance = new PerformanceScope("NonClassificationProcessor:Process", "", true);
                using GoogleDriveService googleService = new(_googleConfiguration.AppProfile, item.MemberEmail);
                var itemInfo = item.ConvertToInfo();
                var matchedRule = _ruleManager.MatchedPotentialRule(itemInfo, _rules.Values.ToList())?.Item1;

                var actionType = GetActionType(matchedRule);

                var record = actionType is ActionType.DeleteOnly or ActionType.ExportBeforeDel ? ProcessRecordItemManager(item, _selectedNode, matchedRule) : null;

                if (matchedRule == null)
                {
                    _logger.Warn($"The item {item.Id} does not match any rule");
                    return;
                }

                if (actionType is ActionType.ExportOnly or ActionType.ExportBeforeDel)
                {
                    if (!_allExportRulesCache.TryGetValue(matchedRule.GoogleDriveRule.ExportInfo.exportLocationId, out var googleExportInfo))
                    {
                        matchedRule.GoogleDriveRule.NARAConfigFile = _naraConfigFile;
                        GetExportStorageConfiguration(matchedRule);
                        GetExportEncryption(matchedRule);
                        var executor = InitExportType(item, matchedRule);
                        if (executor == null)
                        {
                            return;
                        }
                        _allExportRulesCache.TryAdd(matchedRule.GoogleDriveRule.ExportInfo.exportLocationId, executor);
                    }
                }


                if (record != null && IsSkipProcess(record, item, matchedRule))
                {
                    return;
                }

                var (exportBeforeController, backupController) = InitBackupController(matchedRule, record, actionType);
                var settingInfo = ConvertHelper.ConvertRMSetting2Dto(_googleSetting);
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
            catch (JobStopException)
            {
                _logger.Warn("The job has stopped.");
                throw new JobStopException("The job has stopped.");
            }
            catch (Exception ex)
            {
                _logger.Warn($"An error occurred while calculate matched rule [{item.Name}]. Error: {ex}");
                throw;
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
                _logger.Warn($"InitExportType fail. Error {ex}");
                _reportCenter.RecordFailedCommon(item.GenerateDisposalActionJobDetail(I18NResource.ExportAction, rule.Name, ex.Message), (int)item.Level);
                return null;
            }
        }

        private (GoogleExportPathGeneratorBase generator, IGoogleExport export) InitVaultState(Rule rule, GoogleItemData item)
        {
            ExportTypeValue vaultExportType = rule.GoogleDriveRule.ExportInfo.exportType;
            PhysicalDeviceDto physicalDto = rule.PhysicalDeviceDto;
            _logger.Info("Google Export Type is: {0}.", vaultExportType.ToString());
            if (physicalDto != null)
            {
                if (vaultExportType == ExportTypeValue.NARA)
                {
                    string driveName = _googleConfiguration.SelectedNode.Level switch
                    {
                        NodeLevel.GoogleMyDrive => item.MemberEmail,
                        NodeLevel.GoogleSharedDrive => item.DriveName
                    };
                    var googleExport = new GoogleNARAExport(physicalDto, driveName, _googleConfiguration.JobId, rule.DisposalClass, _naraConfigFile);
                    var generator = new GoogleNARAExportPathGenerator(physicalDto.Location, driveName);
                    return (generator, googleExport);
                }
            }
            else
            {
                _logger.Info("The Vault Before Archiver is false.");
            }
            return (null, null);
        }

        private async Task<List<Guid>> GetNullClassificationRuleIdsAsync()
        {
            List<RMSimpleRule> simpleRules = await _googleSettingDao.GetGoogleDriveMappingRules(_googleSetting.ScopeId);
            return simpleRules.OrderBy(x => x.RuleOrder).Select(s => s.RuleId).ToList();
        }

        private Record? ProcessRecordItemManager(GoogleItemData item, GoogleDriveTreeNodeDto selectedNode, Rule? matchedRule)
        {
            try
            {
                using CheckJobStopScope jScope = new();
                int createdDate = (int)DateTime.UtcNow.Ticks;
                bool isProcess = false;
                if (matchedRule == null || !matchedRule.GoogleDriveRule.IsManualApproval)
                {
                    _logger.Info("Item does not match rule criteria or does not enabel manual approval. Skip to generate new record. itemId: {0}", item.Id);
                    return null;
                }
                if (_recordManager.TryGetRecordValue(item.UniqueId, createdDate, out Record existRecord))
                {
                    var oldRuleId = existRecord.RuleId.ToString();

                    if (matchedRule != null)
                    {
                        if (!oldRuleId.Eq(matchedRule.Id))
                        {
                            _logger.Info("Rule changed and matched. itemId: {0}, new rule id: {1}", item.Id, matchedRule.Id);
                            existRecord.RuleId = new Guid(matchedRule.Id);
                            existRecord.DisposalDueDate = AvePoint.RA.Contract.Common.DueDateUtil.None;
                            existRecord.PreviosDisposalDueDate = AvePoint.RA.Contract.Common.DueDateUtil.None;
                            if (existRecord.ManualApprovedStatus != (int)SOApproveDBStatus.None)
                            {
                                _logger.Info("The item change with new matched rule and item is in manual process, remove manual properties. Item id: {0}", item.Id);
                                _reportCenter.RecordSkipCommon(item.GenerateDisposalActionJobDetail(
                                    I18NResource.RemoveAndDestroyAction, matchedRule.Name,
                                    I18NResource.NewMatchedRule), (int)item.Level);
                                existRecord.RemoveManualProperties();
                            }
                        }
                        isProcess = true;
                    }
                    _recordManager.UpdateRecordInfo(existRecord, item);
                    _recordManager.UpdateManualProperties(existRecord, true);
                }
                else
                {
                    existRecord = item.ConvertToRecord(selectedNode, existRecord);
                    existRecord.RuleId = new Guid(matchedRule.Id);
                    _recordManager.AddNewRecord(existRecord);
                    isProcess = true;
                }

                if (!isProcess)
                {
                    return null;
                }
                return existRecord;
            }
            catch (JobStopException)
            {
                _logger.Warn("the job has stopped.");
                throw new JobStopException("The job has stopped.");
            }
            catch (Exception ex)
            {
                _logger.Error($"An error occurred while process item [{item.Id}]. Error: {ex}");
                throw;
            }

        }

        private (BaseBackupController? exportBeforeDel, BaseBackupController baseController) InitBackupController(Rule currentRule, Record? record, ActionType actionType)
        {
            var config = new GoogleConfiguration()
            {
                AppProfile = _googleConfiguration.AppProfile,
                JobId = _googleConfiguration.JobId,
                ReportCenter = _googleConfiguration.ReportCenter,
                RecordManager = _googleConfiguration.RecordManager,
                SelectedNode = _googleConfiguration.SelectedNode,
                CurrentRule = currentRule,
                RuleManager = _googleConfiguration.RuleManager,
                GoogleSetting = _googleConfiguration.GoogleSetting
            };
            switch (actionType)
            {
                case ActionType.Move:
                    return (null, new MoveToController(config));

                case ActionType.DeleteOnly:
                    return (null, new DeleteOnlyController(config, record));

                case ActionType.ExportOnly:
                    return (null, new ExportOnlyController(config, _allExportRulesCache[currentRule.GoogleDriveRule.ExportInfo.exportLocationId]));

                case ActionType.ExportBeforeDel:
                    var exportOnlyController = new ExportOnlyController(config, _allExportRulesCache[currentRule.GoogleDriveRule.ExportInfo.exportLocationId]);
                    var delOnlyController = new DeleteOnlyController(config, record);
                    return (exportOnlyController, delOnlyController);

                default:
                    throw new NotSupportedException("Error when init backup controller");
            }
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
                var dto = _settingProfileDao.Load(mDto);
                if (dto != null)
                {
                    physicalDeviceId = dto.Settings;
                }
            }
            var storageDevice = _storageDeviceService.GetStorageDeviceById(physicalDeviceId, needDecryptSecert: true);
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

        private void GetExportEncryption(Rule rule)
        {
            var exportEncryptionEnabled = _keyValueDao.IsExportDataEncryptionEnabled();
            if (exportEncryptionEnabled)
            {
                var keyIV = _exportDataEncryptionSettingService.GetCurrentAesKey().Extension;
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

        private bool IsSkipProcess(Record record, GoogleItemData item, Rule matchedRule)
        {
            if (record.HoldStatus && record.HoldReleaseTime > DateTime.UtcNow.Ticks)
            {
                _logger.Warn($"Item [{record.Id}] is RecordsHold.");
                _reportCenter.RecordSkipCommon(item.GenerateDisposalActionJobDetail(
                    I18NResource.RemoveAndDestroyAction, matchedRule.Name,
                    I18NResource.FileOnHold), (int)item.Level);
                return true;
            }

            if (record.DisposalDueDate > DateTime.UtcNow.Ticks)
            {
                _logger.Warn($"The item [{item.Id}] has not reached action due date yet.");
                _reportCenter.RecordSkipCommon(item.GenerateDisposalActionJobDetail(string.Empty,
                    matchedRule.Name,
                    I18NResource.NotYetDueDate), (int)item.Level);
                return true;
            }

            return false;
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

    }
}
