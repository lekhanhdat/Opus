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
using AutoMapper;
using AvePoint.GCommon.Contract.CommonFilter;
using AvePoint.GCommon.Contract.StorageOptimization.Object;
using AvePoint.RA.Common;
using AvePoint.RA.Common.Global.Utils;
using AvePoint.RA.Common.RMRuleManagement;
using AvePoint.RA.Contract.RMWeb.Setting;
using AvePoint.RA.Contract.RuleManagement;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Model;
using AvePoint.RA.I18N.Core;
using Microsoft.Extensions.Logging.Abstractions;
using System.Text;

namespace AvePoint.RA.ArchiverMigration.JobStage
{
    internal class MigrateRulesStage : AbstractArchiverMigrationStage
    {

        public override string StageType => "Migrate ArchiverRules";

        /* Fortify Issue Type: Insecure Randomness 
        * Sink Details:  AvePoint.RA.ArchiverMigration ArchiverMigrationJobExecutor  ResetJobProgressUpdaterAsync
        * Ignore Reason: random用于job进程参数，不涉及安全问题
        */
        public override int JobProgressWeight => new Random().Next(9, 11);

        public override string JobDetailType => "RM_JS_ArchiverMigration_DataType_ArchiverRule";

        private RMCPGlobalStorageSetting rmSettings;

        private Dictionary<Guid, Guid> ruleContainerIDs;

        private readonly Dictionary<StubSettingInfo, GCommon.Contract.Server.StubSetting.StubSettingDto> StubSettings = new ();
        private HashSet<string> ruleNames = new();
        private IEnumerable<RMRule> recordsRules = null;

        private HashSet<string> existsStubSettingNames { get; set; }

        private int StubTempalteIndex = 0;

        private IRecordsRuleManagement RecordsRuleManagement => PlatformWindsorManager.GetService<IRecordsRuleManagement>();
        private IStubSettingService StubSettingService => PlatformWindsorManager.GetService<IStubSettingService>();
        private IGlobalStorageSettingDao GlobalStorageSettingDao => PlatformWindsorManager.GetService<IGlobalStorageSettingDao>();
        private IRMMiscProfileDao RMMiscProfileDao => PlatformWindsorManager.GetService<IRMMiscProfileDao>();
        private IRMRuleDao RMRuleDao => PlatformWindsorManager.GetService<IRMRuleDao>();

        protected override async Task PreExecuteAsync()
        {
            IEnumerable<RMRule> allRules = await RMRuleDao.GetRulesWithoutRemovedAsync();
            recordsRules = allRules.Where(rule => rule.ModelType == (int)RuleModel.Records || rule.ModelType == (int)RuleModel.None);
            List<String> profileIdList = RMMiscProfileDao.LoadAllRecordsRules().Select(profile => profile.Id).ToList();
            profileIdList =  profileIdList.Where(profileId => recordsRules.Any(rule => rule.RuleId.ToString().Equals(profileId, StringComparison.OrdinalIgnoreCase))).ToList();
            try
            {
                await RMMiscProfileDao.BatchDeleteAsync(profileIdList);
                logger.Info($"Remove Id in ${String.Join(",",profileIdList)} of MiscProfile Success");
            }catch(Exception ex)
            {
                logger.Error($"Remove Id in ${String.Join(",", profileIdList)} of MiscProfile Fail,error:{ex}");
            }
            
        }

        public override async Task<int> GetStageProgressBaseSizeAsync()
        {
            return (await GetAllRulesCountAsync()) + recordsRules.Count();
        }

        public void FinalUpdateRecordsRMRules()
        {
            RMRuleDao.BatchUpdate(recordsRules.ToList());
        }

        protected override async Task InnerExecuteAsync()
        {
            logger.Info($"Start migrating records rules. Count: {recordsRules.Count()}");
            HashSet<Guid> recordsRuleIDs = new HashSet<Guid>(); 
            foreach (var recordsRule in recordsRules)
            {
                logger.Info($"Migrate IL rule : {recordsRule.Id} | {recordsRule.RuleName}");

                ruleNames.Add(recordsRule.RuleName);
                recordsRuleIDs.Add(recordsRule.RuleId);
                JobProgressUpdater.Increase(1);
                if (string.IsNullOrEmpty(recordsRule.Extension))
                {
                    logger.Error($"Records Rule extension is empty: {recordsRule.Id} - {recordsRule.RuleId}");
                    continue;
                }

                var rule = SerializerHelper.DeserializeByDataContractJsonSerializer<Rule>(recordsRule.Extension);
                UpdateStoragePolicy(rule);
                recordsRule.Extension = SerializerHelper.SerializeByDataContractJsonSerializer(rule);
                try
                {
                    RecordsRuleManagement.CreateRecordsRule(rule, false, true);
                }
                catch (Exception ex)
                {
                    logger.Error($"Records Rule create profile by extension failed: {recordsRule.Id} - {recordsRule.RuleId}. {ex}");
                }
            }

            logger.Info($"Start migrating archiver rules.");
            rmSettings = GlobalStorageSettingDao.GetGlobalSettingInfoFromRA();
            ruleContainerIDs = RMRuleDao.GetAllRulesContainerIDs();
            existsStubSettingNames = StubSettingService.GetAllStubSettingNames();

            List<Rule> archiverRules = null;
            int count = 0;
            int fetchSize = 20;
            int offset = 0;
            do
            {
                archiverRules = await GetAllRulesAsync(offset, fetchSize);
                count = archiverRules?.Count ?? 0;
                logger.Info($"Fetch {count} rules from {offset}");

                if (archiverRules != null)
                {
                    foreach (var archiverRule in archiverRules)
                    {
                        logger.Info($"Start migrate SO rule: {archiverRule.Id} | {archiverRule.Name}");

                        RenameRuleNameForRepeatRuleName(archiverRule);

                        JobExecutor.RuleIdAndRuleInfoMappings[new Guid(archiverRule.Id)] = ((int)archiverRule.PolicyLevel, archiverRule.Name);

                        ResetSOArchiverRuleInfoCompatible(archiverRule);

                        UpgradeRuleSetting(archiverRule);

                        RecordsRuleManagement.CreateRecordsRule(archiverRule, false, true);

                        CreateRMRule(archiverRule, RuleModel.SOArchiver);

                        JobProgressUpdater.Increase(1);
                        AddJobDetail(Contract.RMWeb.JobMonitor.JobDetailsStatus.Successful, archiverRule.Name);
                    }
                }

                offset += count;
            } while (count >= fetchSize);

            logger.Info($"Finish migrate rules.");
        }

        //private bool IsSOArchiverRule(Rule archiverRule)
        //{
        //    return archiverRule.ProfileType == GCommon.Contract.Server.Common.Profile.Object.ProfileType.ArchiverRule;
        //}

        private void ResetSOArchiverRuleInfoCompatible(Rule archiverRule)
        {
            if(archiverRule.MoveToRecordCenterAndDelareSetting != null)
            {
                archiverRule.spMoveOption = new()
                {
                    DestFlag = RecordFlag.SP,
                    SourceFlag = RecordFlag.SP,
                    MoveSetting = new()
                    {
                    },
                    MoveDestination = new()
                    {
                        SPUrl = archiverRule.MoveToRecordCenterAndDelareSetting.DestinationLocation.Url,
                        DestMode = DestMode.UrlMode,
                        NotDeclareMovedData = false,
                        IsMoveVersions = archiverRule.MoveToRecordCenterAndDelareSetting.IsMoveVersions,
                        KeepFolderStructure = archiverRule.MoveToRecordCenterAndDelareSetting.KeepFolderStructure,
                    },
                };
                switch (archiverRule.MoveToRecordCenterAndDelareSetting.ContentConflictResolution)
                {
                    case ContentConflictResolution.Skip:
                        archiverRule.spMoveOption.MoveSetting.ItemLevelConflictOption = ConflictOption.Skip;
                        break;
                    case ContentConflictResolution.Overwrite:
                        archiverRule.spMoveOption.MoveSetting.ItemLevelConflictOption = ConflictOption.Overwrite;
                        break;
                    case ContentConflictResolution.Append:
                        archiverRule.spMoveOption.MoveSetting.ItemLevelConflictOption = ConflictOption.AppendByName;
                        break;
                    default:
                        logger.Info("Not support ContentConflictResolution.");
                        archiverRule.spMoveOption.MoveSetting.ItemLevelConflictOption = ConflictOption.Skip;
                        break;
                }

                archiverRule.MoveToRecordCenterAndDelareSetting = null;
            }

            archiverRule.IsLeaveStubRemoveMetadata = false;
            archiverRule.IsDeleteParentFolder = false;

            ResetSizeRuleCriteria(archiverRule.SOFilters);
            ResetSizeRuleCriteria(archiverRule.Filters);

            if(archiverRule.IsArchivedTier)
            {
                archiverRule.MoveToArchiverTierWhenArchiving = true;
            }

            if (archiverRule.ExportInfo != null && archiverRule.ExportInfo.exportType > ExportTypeValue.Concordance)
            {
                archiverRule.ExportType = archiverRule.ExportInfo.exportType;
                archiverRule.ExportInfo.exportSPDataOption = ExportSPDataOption.ExportBeforeArchive;
                archiverRule.ExportDataBeforeArchiving = true;
            }
            else
            {
                archiverRule.ExportInfo = null;
            }

            // onedrive 不支持Site 以上Level的rule
            if (archiverRule.PolicyLevel > PolicyLevel.Site)
            {
                var oneDriveRuleString = SerializerHelper.SerializeByDataContractSerializer(archiverRule);
                archiverRule.OneDriveRule = SerializerHelper.DeserializeByDataContractSerializer<Rule>(oneDriveRuleString);
            }
        }

        private void ResetSizeRuleCriteria(IEnumerable<FilterPolicy> filters)
        {
            if (filters != null)
            {
                foreach (var filter in filters)
                {
                    if (filter.Level == PolicyLevel.DocumentVersion
                        || filter.Level == PolicyLevel.Attachment)
                    {
                        if (filter.Rule is SizeRule && filter.Rule.Value1 == "Size")
                        {
                            filter.Rule.Value1 = "Document Size";
                        }
                    }

                    if(filter.Condition == PolicyCondition.Exactly)
                    {
                        filter.Condition = PolicyCondition.Equals;
                    }
                }
            }
        }

        private int ConvertSOArchiverRuleKeepDataOption(Cloud.Sdk.Data.Dao.SORule sourceRule)
        {
            int keepDataOption = sourceRule.KeepDataOption;
            if (sourceRule.MoveToRecordCenterAndDelareSetting != null)
            {
                return keepDataOption;
            }

            switch (keepDataOption)
            {
                case 0:
                    keepDataOption = (int)KeepDataOption.ArchiveBackupAndRemove;
                    if (sourceRule.IsArchivedLatestVersion)
                    {
                        keepDataOption |= (int)KeepDataOption.ArchiveLatestVersion;
                    }
                    break;
                case 128:
                    keepDataOption = (int)KeepDataOption.ArchiveBackupAndRemoveLeaveStub;
                    if (sourceRule.IsArchivedLatestVersion)
                    {
                        keepDataOption |= (int)KeepDataOption.ArchiveLatestVersion;
                    }
                    break;
                case 4096:
                    keepDataOption = (int)KeepDataOption.DeleteOnly;
                    if (sourceRule.IsKeepLatestMajorAndMinorVersion)
                    {
                        keepDataOption |= (int)KeepDataOption.KeepLatestVersion;
                    }
                    break;
                default:
                    logger.Info($"KeepDataOption: {sourceRule.KeepDataOption}");
                    break;
            }
            return keepDataOption;
        }

        private void RenameRuleNameForRepeatRuleName(Rule archiverRule)
        {
            int repeatCount = 0;
            var originRuleNameKey = archiverRule.Name.ToLower();
            var ruleNameKey = originRuleNameKey;
            while (!ruleNames.Add(ruleNameKey))
            {
                repeatCount++;
                ruleNameKey = $"{originRuleNameKey}_{repeatCount}";
            }
            if (repeatCount > 0)
            {
                archiverRule.Name = $"{archiverRule.Name}_{repeatCount}";
            }
        }

        private List<Rule> ConvertRules(List<Cloud.Sdk.Data.Dao.SORule> rules)
        {
            var configuration = new MapperConfiguration(cfg =>
            {
                cfg.LicenseKey = ReadEmbeddedLicense();
                cfg.CreateMap<Cloud.Sdk.Data.Dao.SORule, Rule>(MemberList.Destination);
                cfg.CreateMap<Cloud.Sdk.Data.Dao.SOExportInfo, SOExportInfo>();
                cfg.CreateMap<Cloud.Sdk.Data.Dao.MoveToRecordCenterAndDelareSetting, MoveToRecordCenterAndDelareSetting>();
                cfg.CreateMap<Cloud.Sdk.Data.Dao.TagContentInfo, TagContentInfo>();
                cfg.CreateMap<Cloud.Sdk.Data.Dao.SOFilterPolicy, SOFilterPolicy>();
                cfg.CreateMap<Cloud.Sdk.Data.Dao.ArchiverSetting, ArchiverSetting>();
                cfg.CreateMap<Cloud.Sdk.Data.Dao.ArchiverVEOSetting, ArchiverVEOSetting>();
                cfg.CreateMap<Cloud.Sdk.Data.Dao.UserInfo, UserInfo>();
                cfg.CreateMap<Cloud.Sdk.Data.Dao.RetentionInfo, RetentionInfo>();
                cfg.CreateMap<Cloud.Sdk.Data.Dao.DestinationLocationInfo, DestinationLocationInfo>();
                cfg.CreateMap<Cloud.Sdk.Data.Dao.DisplayDateTime, DisplayDateTime>();
                cfg.CreateMap<Cloud.Sdk.Data.Dao.FilterPolicy, GCommon.Contract.CommonFilter.FilterPolicy>();
                cfg.CreateMap<Cloud.Sdk.Data.Dao.Extention, GCommon.Contract.CommonFilter.Extention>();
                cfg.CreateMap<Cloud.Sdk.Data.Dao.PolicyRuleBase, GCommon.Contract.CommonFilter.PolicyRuleBase>();
                cfg.CreateMap<Cloud.Sdk.Data.Dao.PolicyValue, GCommon.Contract.CommonFilter.PolicyValue>();
                cfg.CreateMap<Cloud.Sdk.Data.Dao.BposInfo, GCommon.Contract.CentralAdmin.Object.BposInfo>();
                cfg.CreateMap<Cloud.Sdk.Data.Dao.BposUserAccountInfo, GCommon.Contract.CentralAdmin.Object.BposUserAccountInfo>();
            }, NullLoggerFactory.Instance);
            var mapper = configuration.CreateMapper();

            var soRules = new List<Rule>();
            foreach (var sourceRule in rules)
            {
                var targetRule = mapper.Map<Rule>(sourceRule);

                ConvertRuleFilters(mapper, sourceRule, targetRule);

                if(sourceRule.IsColdTier)
                {
                    targetRule.MoveToAnotherTierType = (int)Storage.AccessTierType.Cold;
                }
                else if (sourceRule.IsArchivedTier)
                {
                    targetRule.MoveToArchiverTierWhenArchiving = true;
                    targetRule.MoveToAnotherTierType = (int)Storage.AccessTierType.Archive;
                }

                targetRule.KeepDataOption = ConvertSOArchiverRuleKeepDataOption(sourceRule);

                soRules.Add(targetRule);
            }

            return soRules;
        }

        private string ReadEmbeddedLicense()
        {
            var assembly = typeof(MigrateRulesStage).Assembly;
            using var stream = assembly.GetManifestResourceStream("AvePoint.RA.ArchiverMigration.ArchiverMigration.JobStage.automapper.lic");
            if (stream == null)
                throw new InvalidOperationException("Embedded resource 'AvePoint.RA.ArchiverMigration.ArchiverMigration.JobStage.automapper.lic' not found.");
            using var reader = new StreamReader(stream);
            return reader.ReadToEnd();
        }

        private void ConvertRuleFilters(IMapper mapper, Cloud.Sdk.Data.Dao.SORule sourceRule, Rule targetRule)
        {
            if (sourceRule == null || targetRule == null)
            {
                return;
            }

            targetRule.SOFilters = sourceRule.SOFilters?.Select(filter =>
            {
                var targetFilter = mapper.Map<SOFilterPolicy>(filter);
                targetFilter.Rule = ConvertToPolicyRuleBase(filter.Rule);
                return targetFilter;
            })?.ToList();

            targetRule.Filters = sourceRule.Filters?.Select(filter =>
            {
                var targetFilter = mapper.Map<GCommon.Contract.CommonFilter.FilterPolicy>(filter);
                targetFilter.Rule = ConvertToPolicyRuleBase(filter.Rule);
                return targetFilter;
            })?.ToList();
        }

        private GCommon.Contract.CommonFilter.PolicyRuleBase ConvertToPolicyRuleBase(Cloud.Sdk.Data.Dao.PolicyRuleBase rulebase)
        {
            var fullName = $"AvePoint.GCommon.Contract.CommonFilter.{rulebase.GetType().Name}";
            var assembly = Array.Find(AppDomain.CurrentDomain.GetAssemblies(), (System.Reflection.Assembly a) => a.FullName.StartsWith("CommonContract"));
            var result = assembly?.CreateInstance(fullName) as GCommon.Contract.CommonFilter.PolicyRuleBase;
            if (result == null)
            {
                result = new GCommon.Contract.CommonFilter.PolicyRuleBase();
                logger.Error($"Not found policy rule type: {fullName}");
            }
            result.Value1 = rulebase.Value1;
            return result;
        }

        private async Task<int> GetAllRulesCountAsync()
        {
            return await GetArchiverMigrationDataAsync<int>((service) =>
            {
                return service.GetAllRulesCount();
            });
        }

        private async Task<List<Rule>> GetAllRulesAsync(int offset, int fetchRows)
        {
            var rules = await GetArchiverMigrationDataAsync<List<Cloud.Sdk.Data.Dao.SORule>>((service) =>
            {
                return service.GetAllRules(new Cloud.Sdk.Data.Dao.FetchDataInfo()
                {
                    Offset = offset,
                    FetchSize = fetchRows
                });
            }, true);

            return ConvertRules(rules);
        }

        private void CreateRMRule(Rule soRule, RuleModel modelType)
        {
            var ruleId = new Guid(soRule.Id);
            Guid containerId;
            RMRuleDao.AddOrUpdateRMRule(
                new RMRule()
                {
                    RuleId = ruleId,
                    RuleName = soRule.Name,
                    RuleLevel = (int)soRule.PolicyLevel,
                    DisposalAction = (int)RuleHelper.GetOperationTypeForSP(soRule),
                    ExchangeDisposalAction = (int)RuleHelper.GetOperationTypeForEXO(soRule.EXORule),
                    PhysicalDisposalAction = (int)RuleHelper.GetOperationTypeForPhysical(soRule.PhysicalRule),
                    FSDisposalAction = (int)RuleHelper.GetOperationTypeForFS(soRule.FSRule),
                    SPLocalDisposalAction = (int)RuleHelper.GetOperationTypeForSPLocal(soRule.SPLocalRule),
                    OneDriveDisposalAction = (int)RuleHelper.GetOperationTypeForOneDrive(soRule.OneDriveRule),
                    AzureFileDisposalAction = (int)RuleHelper.GetOperationTypeForAzureFile(soRule.AzureFileRule),
                    ConnectorDisposalAction = (int)RuleHelper.GetOperationTypeForConnector(soRule.ConnectorRule),
                    DeleteRecords = soRule.DeleteRecords,
                    IsRemoved = false,
                    Description = soRule.Description,
                    ModifyTime = soRule.ModifyTime,
                    DisposalClass = soRule.DisposalClass,
                    Extension = SerializerHelper.SerializeByDataContractJsonSerializer(soRule),
                    ModelType = (int)modelType,
                    DAOMigrated = true
                },
                ruleContainerIDs.TryGetValue(ruleId, out containerId) ? containerId : null);
        }

        public void UpgradeRuleSetting(Rule soRule)
        {
            try
            {
                logger.Info($"Upgrade rule setting for rule: {soRule.Id}");
                UpdateStoragePolicy(soRule);

                var isLeaveStub = (soRule.KeepDataOption & (int)KeepDataOption.ArchiveBackupAndRemoveLeaveStub) == (int)KeepDataOption.ArchiveBackupAndRemoveLeaveStub;
                if (isLeaveStub)
                {
                    if (string.IsNullOrEmpty(soRule.StubTemplateId))
                    {
                        logger.Info("Update sp leave stub rule");
                        MigrateStubSetting(soRule);
                    }
                    if (soRule.OneDriveRule != null && string.IsNullOrEmpty(soRule.OneDriveRule.StubTemplateId))
                    {
                        logger.Info("Update one leave stub rule");
                        MigrateStubSetting(soRule.OneDriveRule);
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error($"Upgrade stub setting rule failed, rule name is {soRule.Name}, {ex}");
            }
        }

        private void UpdateStoragePolicy(Rule soRule)
        {
            UpdateStoragePolicyForSingleRule(soRule);
            UpdateStoragePolicyForSingleRule(soRule.OneDriveRule);
            UpdateStoragePolicyForSingleRule(soRule.PhysicalRule);
        }
        private void UpdateStoragePolicyForSingleRule(Rule soRule)
        {
            if(soRule == null)
            {
                return;
            }

            if (string.IsNullOrEmpty(soRule.StoragePolicyId))
            {
                //if (rmSettings != null && rmSettings.StoragePolicyId != Guid.Empty)
                //{
                //    soRule.StoragePolicyId = rmSettings.StoragePolicyId.ToString();
                //    soRule.StoragePolicyName = rmSettings.StoragePolicyName;
                //}
            }
            else
            {
                var storageDevice = JobExecutor.StorageMigrationService.GetStorageDeviceByDAOStoragePolicyId(soRule.StoragePolicyId);
                if(storageDevice != null)
                {
                    soRule.StoragePolicyId = storageDevice.Id;
                    soRule.StoragePolicyName = storageDevice.Name;
                }
                else
                {
                    soRule.StoragePolicyId = Guid.Empty.ToString();
                }
            }
        }

        private void MigrateStubSetting(Rule soRule)
        {
            var stubSettingInfo = BuildStubSettingInfo(soRule);

            var stubSettingDto = StubSettings.FirstOrDefault(item => Equels(item.Key, stubSettingInfo)).Value;
            if (stubSettingDto == null)
            {
                stubSettingDto = CreateNewStubSetting(stubSettingInfo);
            }

            soRule.StubTemplateId = stubSettingDto.Id;
            soRule.StubTemplateName = stubSettingDto.Name;
        }

        private static StubSettingInfo BuildStubSettingInfo(Rule soRule)
        {
            var isDeclare = soRule.DeclareStubOption == DeclareStubType.Declare;
            var leaveStubMessage = soRule.LeaveStubMessage;
            var isCustomizeMsg = soRule.LeaveStubType != LeaveStubType.Link && !string.IsNullOrEmpty(leaveStubMessage);
            var stubSettingInfo = new StubSettingInfo
            {
                StubType = soRule.LeaveStubType,
                IsDeclare = isDeclare,
                IsFileName = isCustomizeMsg && soRule.IsFileName,
                IsFilePath = isCustomizeMsg && soRule.IsFilePath,
                IsArchivedTime = isCustomizeMsg && soRule.IsArchivedDate,
                IsRuleName = isCustomizeMsg && soRule.IsRuleName,
                IsRestore = isCustomizeMsg && soRule.IsRestoreLink,
            };
            if(isCustomizeMsg)
            {
                stubSettingInfo.StubContent = ReplaceStubTags(leaveStubMessage, soRule);
            }
            else if(soRule.LeaveStubType != LeaveStubType.Link)
            {
                stubSettingInfo.StubContent = RMResourceManager.GetString("StorageOptimization.Gui_A1AA2887-13C3-44B6-B26B-01E7DC580F21");
            }
            return stubSettingInfo;
        }

        private GCommon.Contract.Server.StubSetting.StubSettingDto CreateNewStubSetting(StubSettingInfo stubSettingInfo)
        {
            var stubSettingDto = new GCommon.Contract.Server.StubSetting.StubSettingDto
            {
                Id = Guid.NewGuid().ToString(),
                StubType = (int)stubSettingInfo.StubType,
                StubContent = stubSettingInfo.StubContent,
                IsDeclareStubAsRecords = stubSettingInfo.IsDeclare
            };

            do
            {
                StubTempalteIndex++;
                stubSettingDto.Name = "Stub Template_" + StubTempalteIndex;
            }
            while (existsStubSettingNames.Contains(stubSettingDto.Name));

            StubSettingService.MagrateDAOStubSetting(stubSettingDto);
            StubSettings[stubSettingInfo] = stubSettingDto;

            return stubSettingDto;
        }

        private static string ReplaceStubTags(string stubContent, Rule soRule)
        {
            var finalStubContent = new StringBuilder(stubContent).AppendLine();
            if (soRule.IsFileName)
            {
                finalStubContent.AppendLine($"File Name: [StorageOptimization.Gui_9FE3A6A6-DB1B-478A-9C84-3793B070A958]");
            }
            if (soRule.IsFilePath)
            {
                finalStubContent.AppendLine($"File Path: [StorageOptimization.Gui_FB4CF4C0-AA67-43A7-9C37-97719E9B97A3]");
            }
            if (soRule.IsArchivedDate)
            {
                finalStubContent.AppendLine($"Archived Time: [StorageOptimization.Gui_E5E06835-59BF-4AB1-903D-B0BF3EA6E15B]");
            }
            if (soRule.IsRuleName)
            {
                finalStubContent.AppendLine($"Rule Name: [StorageOptimization.Gui_AE414513-8007-44BC-98B9-8E6B1212C257]");
            }
            if (soRule.IsRestoreLink)
            {
                finalStubContent.AppendLine($"Restore Link: [RM_AR_CP_Stub_Panel_RestoreLink]");
            }
            return finalStubContent.ToString();
        }

        private static bool Equels(StubSettingInfo stubSettingInfo1, StubSettingInfo stubSettingInfo2)
        {
            if (stubSettingInfo1.StubType == stubSettingInfo2.StubType
                && stubSettingInfo1.IsDeclare == stubSettingInfo2.IsDeclare
                && stubSettingInfo1.IsFileName == stubSettingInfo2.IsFileName
                && stubSettingInfo1.IsFilePath == stubSettingInfo2.IsFilePath
                && stubSettingInfo1.IsArchivedTime == stubSettingInfo2.IsArchivedTime
                && stubSettingInfo1.IsRuleName == stubSettingInfo2.IsRuleName
                && stubSettingInfo1.IsRestore == stubSettingInfo2.IsRestore
                && stubSettingInfo1.StubContent == stubSettingInfo2.StubContent)
            {
                return true;
            }
            return false;
        }

        private class StubSettingInfo
        {
            public LeaveStubType StubType;

            public string StubContent { get; set; }

            public bool IsDeclare { get; set; }
            public bool IsFileName { get; set; }
            public bool IsFilePath { get; set; }
            public bool IsArchivedTime { get; set; }
            public bool IsRuleName { get; set; }

            public bool IsRestore { get; set; }
        }
    }
}
