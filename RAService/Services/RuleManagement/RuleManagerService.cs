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
using AvePoint.GCommon.Contract.CommonFilter;
using AvePoint.GCommon.Contract.GranularBackup.Object;
using AvePoint.GCommon.Contract.Media.Object;
using AvePoint.GCommon.Contract.Server.ControlPanel.Office365;
using AvePoint.GCommon.Contract.Server.ControlPanel.Office365Account.Object;
using AvePoint.GCommon.Contract.Storage.Entity;
using AvePoint.GCommon.Contract.StorageOptimization.Object;
using AvePoint.GCommon.Contract.Tree.Object;
using AvePoint.GCommon.Utility;
using AvePoint.GCommon.Utility.Cryptography;
using AvePoint.Media.Core.IO;
using AvePoint.RA.Common;
using AvePoint.RA.Common.Report;
using AvePoint.RA.Common.RMRuleManagement;
using AvePoint.RA.Common.SystemSetting;
using AvePoint.RA.Common.Util;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract;
using AvePoint.RA.Contract.Audit;
using AvePoint.RA.Contract.CodeView;
using AvePoint.RA.Contract.Common;
using AvePoint.RA.Contract.DocAve;
using AvePoint.RA.Contract.Exceptions;
using AvePoint.RA.Contract.Google.GControlPlatform;
using AvePoint.RA.Contract.JobMonitor;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.RMRuleManageMent;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RMWeb.Audit;
using AvePoint.RA.Contract.RMWeb.CP;
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using AvePoint.RA.Contract.RMWeb.Rule;
using AvePoint.RA.Contract.RMWeb.Setting;
using AvePoint.RA.Contract.RMWeb.StorageDevice;
using AvePoint.RA.Contract.RMWeb.Tree;
using AvePoint.RA.Contract.RMWeb.Tree.Base;
using AvePoint.RA.Contract.RuleManagement;
using AvePoint.RA.Contract.Schedule;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Dao.Extension;
using AvePoint.RA.DB.Dao.Impl;
using AvePoint.RA.DB.Dao.Utility;
using AvePoint.RA.DB.Model;
using AvePoint.RA.DB.SecurityTrimming;
using AvePoint.RA.I18N.Core;
using AvePoint.RA.RADataBroker;
using AvePoint.RA.Service.Services;
using AvePoint.RA.Service.Services.ControlPanel;
using AvePoint.RA.Service.Services.RuleManagement.AuditHandler;
using AvePoint.RA.SharePoint.RMExplorer;
using Microsoft365.Common.Extension;
using Newtonsoft.Json.Linq;
using NVelocity.Runtime.Directive;
using RAArchiverCommon.Utility;
using RATeams;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Xml;
using SOSourceFlag = AvePoint.RA.SharePoint.ArchiverCommon.SOSourceFlag;
using TimeFilterCondition = AvePoint.GCommon.Contract.Server.Common.Profile.Object.TimeFilterCondition;
using TimeUnit = AvePoint.GCommon.Contract.Server.Common.Profile.Object.TimeUnit;

namespace AvePoint.RA.Service.RuleManagement
{
    [Audit]
    public class RuleManagerService : RMServiceBase, IRuleManagerService
    {
        private RALogger logger = RALogger.GetInstance(typeof(RuleManagerService));

        private IGlobalStorageSettingDao GlobalStorageSettingDao => PlatformWindsorManager.GetService<IGlobalStorageSettingDao>();
        private ITermRuleAssociationDao TermRuleAssocition => PlatformWindsorManager.GetService<ITermRuleAssociationDao>();
        private IGeneralSettingService mGeneralSettingService => PlatformWindsorManager.GetService<IGeneralSettingService>();
        private IExplorerService ExplorerService => PlatformWindsorManager.GetService<IExplorerService>();
        private ILocationManagementService LocationManagementService => PlatformWindsorManager.GetService<ILocationManagementService>();
        private IManualProcessManagementService ManualProcessManagementService => PlatformWindsorManager.GetService<IManualProcessManagementService>();
        private IRMSecurityTrimmingHelper SecurityTrimmingHelper => PlatformWindsorManager.GetService<IRMSecurityTrimmingHelper>();
        private IRMChangeClassificationDao ChangeClassificationDao => PlatformWindsorManager.GetService<IRMChangeClassificationDao>();
        private IUserService UserService => PlatformWindsorManager.GetService<IUserService>();
        protected IRMRuleDao RMRuleDao => PlatformWindsorManager.GetService<IRMRuleDao>();
        private IStorageDeviceService StorageDeviceService => PlatformWindsorManager.GetService<IStorageDeviceService>();
        private IStubSettingService StubSettingService => PlatformWindsorManager.GetService<IStubSettingService>();
        private ISettingProfilesDao SettingProfileDao => PlatformWindsorManager.GetService<ISettingProfilesDao>();
        private ITenantService TenantService => PlatformWindsorManager.GetService<ITenantService>();
        private ILicenseHelperService LicenseHelperService => PlatformWindsorManager.GetService<ILicenseHelperService>();
        private IRMKeyValueDao RMKeyValueDao => PlatformWindsorManager.GetService<IRMKeyValueDao>();
        private IRMKeyValueDao KeyValueDao => PlatformWindsorManager.GetService<IRMKeyValueDao>();
        private IRMRemoteNodeDao RemoteNodeDao = PlatformWindsorManager.GetService<IRMRemoteNodeDao>();
        
        private IGControlPlatformApprovalProcessService _gControlPlatformApprovalProcessService = PlatformWindsorManager.GetService<IGControlPlatformApprovalProcessService>();
        private IScheduleService ScheduleService => PlatformWindsorManager.GetService<IScheduleService>();

        private bool _existShowExport = false;
        private bool _existEnableExport = false;
        private bool _existEnableManualAproval = false;
        private bool _existManualWF = false;
        private bool _existManualRO = false;
        private bool _existShowStorageInfo = false;
        private bool _existShowRetentionInfo = false;
        private bool _existShowArchivedTier = false;
        private static string DEFAULTSTORAGEID = "6a040c17-af8a-4f1f-96c1-7ceb2e23b1f3";
        private List<RMRuleSourceType> _showExportBeforeActionSources = [ RMRuleSourceType.SP, RMRuleSourceType.OneDrive, RMRuleSourceType.EXO, RMRuleSourceType.GoogleDrive ];
        private List<RMRuleSourceType> _showStorageLocationSources = [ RMRuleSourceType.SP, RMRuleSourceType.OneDrive, RMRuleSourceType.Physical, RMRuleSourceType.FS, RMRuleSourceType.GoogleDrive ];

        [RACodeReview("Allen yin")]
        public async Task<List<Rule>> GetRulesFromDAAsync()
        {
            try
            {
                //IMStorageOptimizationService soService = DocAveServiceHelper.CreateServiceClient<IMStorageOptimizationService>();
                //List<Rule> rules = soService.GetRACreatedRules();
                var client = new DAOAPIClientV1();
                using (PerformanceScope scope = new PerformanceScope("API get all rules"))
                {
                    List<Rule> rules = await client.GetRACreatedRulesAsync();
                    return rules;
                }
            }
            catch (AveException ae)
            {
                logger.Error(ae.Message, ae);
                throw;
            }
            catch (Exception e)
            {
                logger.Error(e.Message, e);
                throw new AveException("Error occured while communicating with DocAve Service, Please check the configure file or DocAve Service status.");
            }
        }

        public async System.Threading.Tasks.Task SyncDARuleToRecordsAsync()
        {
            try
            {
                logger.Info("Start sync DAO rules to Records.");
                using (PerformanceScope scope = new PerformanceScope("Sync rule time."))
                {
                    var availableRules = RMRuleDao.GetAvailableRules();
                    var client = new DAOAPIClientV1();
                    if (availableRules.Count == 0)
                    {
                        List<Rule> daRules = await client.GetRACreatedRulesAsync();
                        List<RMRule> rmRules = daRules.ConvertAll(r =>
                        {
                            return new RMRule()
                            {
                                RuleId = new Guid(r.Id),
                                RuleName = r.Name,
                                RuleLevel = (int)r.PolicyLevel,
                                DisposalAction = (int)RuleHelper.GetOperationType(r),
                                DeleteRecords = r.DeleteRecords,
                                IsRemoved = false,
                                Description = r.Description,
                                ModifyTime = r.ModifyTime,
                                DisposalClass = !string.IsNullOrEmpty(r.DisposalClass) ? r.DisposalClass : null,
                                Extension = SerializerHelper.SerializeByDataContractJsonSerializer(r)
                            };
                        });
                        if (rmRules.Count > 0)
                        {
                            RMRuleDao.BatchCreate(rmRules);
                            logger.Info("Complete sync rules.");
                        }
                        else
                        {
                            logger.Info("skip sync rules.");
                        }
                    }
                    else
                    {
                        foreach (var rmRule in availableRules)
                        {
                            if (!string.IsNullOrEmpty(rmRule.DisposalClass) && !string.IsNullOrEmpty(rmRule.Extension))
                            {
                                continue;
                            }
                            else
                            {
                                var dRule = client.LoadRule(rmRule.RuleId.ToString());
                                if (dRule != null)
                                {
                                    if (string.IsNullOrEmpty(rmRule.DisposalClass))
                                    {
                                        rmRule.DisposalClass = dRule.DisposalClass;
                                    }

                                    if (string.IsNullOrEmpty(rmRule.Extension))
                                    {
                                        rmRule.Extension = SerializerHelper.SerializeByDataContractJsonSerializer(dRule);
                                    }
                                }
                            }
                        }
                        RMRuleDao.BatchUpdate(availableRules);
                        logger.Info($"Complete sync {availableRules.Count} rules.");
                    }
                }
            }
            catch (Exception e)
            {
                logger.Error("Error occurred when sync rules. Error:{0}", e.ToString());
            }
        }

        public async Task<RMRuleInfos> ConvertToRuleInfoAsync(Rule rule, bool isControlPlus = false)
        {
            var isNewLogicAccount = TenantService.IsNewOpusTenant();
            await ConvertRuleFiltersTimeZoneAsync(rule);
            List<SOFilterPolicy> filters = rule.SOFilters;
            List<RuleFilter> displayFilters = new List<RuleFilter>();
            List<string> ruleCriteria = new List<string>();
            if (filters != null)
            {
                foreach (var filter in filters)
                {
                    ArchiverRuleFilter arFilter = new ArchiverRuleFilter(filter);
                    try
                    {
                        var ruleType = arFilter.RuleType;
                    }
                    catch (Exception ex)
                    {
                        // TODO: need review
                        logger.Error($"Get rule type failed. {ex}");
                        continue;
                    }
                    RuleFilter newFilter = new RuleFilter()
                    {
                        CombineMode = arFilter.CombineMode,
                        Condition = arFilter.Condition,
                        RuleType = arFilter.RuleType,
                        filterName = arFilter.RuleName,
                        Level = arFilter.Level,
                        SequenceNo = arFilter.SequenceNo,
                        Value1 = arFilter.Value1,
                        Value1Unit = arFilter.Value1Unit,
                        Value2 = arFilter.Value2,
                        Value2Unit = arFilter.Value2Unit,
                        Value3 = arFilter.Value3,
                        Value3Unit = arFilter.Value3Unit,
                        FilterCretia = arFilter.FilterCretia(),
                    };
                    if (filter.RuleType == PolicyRuleType.Attachment)
                    {
                        newFilter.Value1Unit = PolicyValueUnit.None;
                        newFilter.Value2Unit = PolicyValueUnit.None;
                    }
                    if (newFilter.Condition == ArchiverFilterCondition.FromTo)
                    {
                        newFilter.StartTimeInfo = arFilter.GetFilterDateTimeInfo(true);
                        newFilter.EndTimeInfo = arFilter.GetFilterDateTimeInfo(false);      
                    }
                    else if (newFilter.Condition == ArchiverFilterCondition.Before)
                    {
                        newFilter.StartTimeInfo = arFilter.GetFilterDateTimeInfo(true);
                    }
                    displayFilters.Add(newFilter);
                    ruleCriteria.Add(arFilter.FilterCretia(isControlPlus));
                }
            }

            RMRuleInfos newRule = new RMRuleInfos()
            {
                RuleKeepDataOption = rule.KeepDataOption,
                RelatedRecordOption = rule.RelatedRecordOption,
                //这里不要直接merge Local的方法，会导致页面rule view detail的bug
                ArchiverActions = GetArchiverRuleSettingType(rule, SOSourceFlag.SharePoint),//Display rule action
                Description = rule.Description,
                EnableManualApproval = rule.IsManualApproval,
                DeleteRecords = rule.DeleteRecords,
                IncludeDeleteRecordLabel = rule.IncludeDeleteRecordLabel,
                LockRecordBeforeDestroy = rule.LockRecordBeforeDestroy,
                DeleteSiteCollectionToRecycleBin = rule.IsDeleteSiteCollectionToRecycleBin((int)SOSourceFlag.SharePoint),
                DeleteToRecycleBin = rule.DeleteToRecycleBin,
                DeclareLinkFile = rule.DeclareLinkFile,
                isChecked = false,
                RuleId = rule.Id,
                RuleName = rule.Name,
                RuleFilters = displayFilters,
                ExportFormat = rule.ExportType.ToString(),
                FilterCombineMode = rule.AndOrExpression == null ? "" : GetAndOrExpression(rule.AndOrExpression[rule.PolicyLevel].ToString()).Replace("And", I18NEntity.GetString("RM_JS_Rule_ConditionAnd")).Replace("Or", I18NEntity.GetString("RM_JS_Rule_ConditionOr")),
                RuleCretias = ruleCriteria,
                MoveToRecordCenterSettings = rule.MoveToRecordCenterAndDelareSetting,
                MoveToLocationPasswordEncrypted = true,
                RuleLevel = rule.PolicyLevel,
                ExportInfo = rule.ExportInfo,
                TagContentInfo = await ConvertAllTagContentInfoAsync(rule.TagContentInfo, isNewLogicAccount, rule.KeepDataOption, rule.PolicyLevel, AccountUtility.IsSupportRecordLabel()),
                Modified = (await mGeneralSettingService.ConvertTiksToDateTimeAsync(rule.ModifyTime, true, isControlPlus)).FormaTime,
                DisposalClass = rule.DisposalClass,
                LeaveStubMessage = rule.LeaveStubMessage,
                IsRestoreLink = rule.IsRestoreLink,
                //IsEnableRetention = rule.IsEnableRetention,
                //RetentionInfo = await this.ConvertRetentionInfoAsync(rule.RetentionInfo),
                StoragePolicyId = rule.StoragePolicyId,
                StoragePolicyName = rule.StoragePolicyName,
                StubTemplateId = rule.StubTemplateId,
                StubTemplateName = rule.StubTemplateName,
                ExportDataBeforeArchiving = rule.ExportDataBeforeArchiving,
                MoveToArchiverTierWhenArchiving = rule.MoveToArchiverTierWhenArchiving,
                ArchivedLatestVersion = rule.ArchivedLatestVersion,
                KeepLatestMajorAndMinorVersion = rule.KeepLatestMajorAndMinorVersion,
                ArchiverOnlyLastestVersion = rule.ArchiverOnlyLastestVersion,
                KeepLatestMajorAndMinorVersionAndArchiveOthers = rule.KeepLatestMajorAndMinorVersionAndArchiveOthers,
                MoveToAnotherTierType = rule.MoveToArchiverTierWhenArchiving? (int)Storage.AccessTierType.Archive :rule.MoveToAnotherTierType?? (int)Storage.AccessTierType.Other,

                //Terms = GetRuleTermsName(terms),
            };

            if(AccountUtility.IsSupportRecordLabel() && (newRule.RuleKeepDataOption & (int)KeepDataStatus.DeclareRecord) == (int)KeepDataStatus.DeclareRecord)
            {
                newRule.RuleKeepDataOption -= (int)KeepDataStatus.DeclareRecord;
                if((newRule.RuleKeepDataOption & (int)KeepDataStatus.TagContent) != (int)KeepDataStatus.TagContent)
                {
                    newRule.RuleKeepDataOption += (int)KeepDataOption.TagContent;
                }
            }

            if ((rule.KeepDataOption & (int)KeepDataStatus.Archive) == (int)KeepDataStatus.Archive || (rule.KeepDataOption & (int)KeepDataStatus.ArchiveAndLeaveStub) == (int)KeepDataStatus.ArchiveAndLeaveStub)
            {
                newRule.IsEnableRetention = rule.IsEnableRetention;
                newRule.RetentionInfo = await this.ConvertRetentionInfoAsync(rule.RetentionInfo);
            }
            else
            {
                newRule.IsEnableRetention = rule.IsEnableStoreContentRetention;
                newRule.RetentionInfoList = this.ConvertStoreContentRetentionInfo(rule.StoreContentRetentionInfos);
                newRule.RetentionInfo = this.ConvertStoreContentRetentionInfo(rule.StoreContentRetentionInfos)?.FirstOrDefault();
            }


            if (rule.ProfileType == GCommon.Contract.Server.Common.Profile.Object.ProfileType.ArchiverRule)
            {
                newRule.ModelType = AvePoint.GCommon.Contract.StorageOptimization.Object.RuleModel.SOArchiver;
            }
            else
            {
                newRule.ModelType = AvePoint.GCommon.Contract.StorageOptimization.Object.RuleModel.Records;
            }
            if (rule.MoveToRecordCenterAndDelareSetting != null
                && rule.MoveToRecordCenterAndDelareSetting.DestinationLocation != null
                && !string.IsNullOrEmpty(rule.MoveToRecordCenterAndDelareSetting.DestinationLocation.Password))
            {
                newRule.MoveDto = new MoveToDto();
                newRule.MoveDto.IsSpecifyLocation = true;
                newRule.MoveDto.LocationPath = rule.MoveToRecordCenterAndDelareSetting.DestinationLocation.Url;
                newRule.MoveDto.NotDeclareMovedData = rule.MoveToRecordCenterAndDelareSetting.DelaredRecord;
                newRule.MoveDto.FileNameConflictOption = FileNameConflictOption.Skip;
            }
            if (rule.ExportInfo != null && !rule.ExportType.Equals(AvePoint.GCommon.Contract.StorageOptimization.Object.ExportTypeValue.None))
            {
                newRule.EnableExport = true;
            }
            if ((rule.KeepDataOption & 128) == (int)KeepDataStatus.LinkToDocument || (rule.KeepDataOption & 2048) == (int)KeepDataStatus.ArchiveAndLeaveStub)
            {
                newRule.LeaveStubMessage = !string.IsNullOrEmpty(rule.LeaveStubMessage) ? HttpUtility.HtmlDecode(rule.LeaveStubMessage) : I18NEntity.GetString("RM_RDM_CreateRule_LeaveStubOptionMessage_Default");
            }
            InitRMRuleManualApprovalInfo(rule, newRule);

            if (rule.OneDriveRule != null)
            {
                rule.OneDriveRule.ProfileType = rule.ProfileType;
            }

            if(rule.TeamsRule != null)
            {
                rule.TeamsRule.ProfileType = rule.ProfileType;
            }

            newRule.EXORule = rule.EXORule != null ? await ConvertToEXORuleInfoAsync(rule.EXORule, isNewLogicAccount) : null;
            newRule.PhysicalRule = rule.PhysicalRule != null ? await ConvertToPhysicalRuleInfoAsync(rule.PhysicalRule, isNewLogicAccount) : null;
            newRule.FSRule = rule.FSRule != null ? await ConvertToFSRuleInfoAsync(rule.FSRule) : null;
            newRule.SPLocalRule = rule.SPLocalRule != null ? await ConvertToSPLocalRuleInfoAsync(rule.SPLocalRule, isNewLogicAccount) : null;
            newRule.OneDriveRule = rule.OneDriveRule != null ? await ConvertToOneDriveRuleInfoAsync(rule.OneDriveRule, isNewLogicAccount) : null;
            newRule.AzureFileRule = rule.AzureFileRule != null ? await ConvertToAzureFileRuleInfoAsync(rule.AzureFileRule) : null;
            newRule.BoxRule = rule.BoxRule != null ? await ConvertToBoxRuleInfoAsync(rule.BoxRule) : null;
            newRule.ConnectorRule = rule.ConnectorRule != null ? await ConvertToConnectorRuleInfoAsync(rule.ConnectorRule) : null;
            newRule.GoogleDriveRule = rule.GoogleDriveRule != null ? await ConvertToGoogleDriveRuleInfoAsync(rule.GoogleDriveRule, isControlPlus) : null;
            newRule.TeamsRule = rule.TeamsRule != null ? await ConvertToTeamsRuleInfoAsync(rule.TeamsRule, isNewLogicAccount) : null;
            ConvertToRMMoveSettings(rule, newRule);
            if (TenantService.IsNewOpusTenant())
            {
                ConvertToStorageInfoFromExportInfo(rule, newRule);
            }
            return newRule;
        }

        private async Task<List<RMTagContentInfo>> ConvertAllTagContentInfoAsync(List<TagContentInfo> tagContentInfo, bool isNewLogicAccount, int keepDataOption, PolicyLevel policyLevel, bool isSupportConvertRetentionLabel)
        {
            if (tagContentInfo == null)
            {
                if(isSupportConvertRetentionLabel && isNewLogicAccount && (policyLevel == PolicyLevel.Document || policyLevel == PolicyLevel.Item) && (keepDataOption & (int)KeepDataOption.DeclareRecord) == (int)KeepDataOption.DeclareRecord)
                {
                    tagContentInfo = new List<TagContentInfo>()
                    {
                        new TagContentInfo()
                        {
                            Type = TagContentInfoType.RetentionLabel,
                            Option = (int)RetentionLabelOptions.GetFromGeneralSetting
                        }
                    };
                }
                else
                {
                    return null;
                }
            }
            if (isSupportConvertRetentionLabel && isNewLogicAccount && (policyLevel == PolicyLevel.Document || policyLevel == PolicyLevel.Item))
            {
                var hasRetentionLabelTagContent = tagContentInfo.Any(_ => _.Type == TagContentInfoType.RetentionLabel);
                if(!hasRetentionLabelTagContent && (keepDataOption & (int)KeepDataOption.DeclareRecord) == (int)KeepDataOption.DeclareRecord)
                {
                    tagContentInfo.Add(new TagContentInfo()
                    {
                        Type = TagContentInfoType.RetentionLabel,
                        Option = (int)RetentionLabelOptions.GetFromGeneralSetting
                    });
                }
            }
            return (await Task.WhenAll(tagContentInfo?.Select(async t => await ConvertTagContentInfoAsync(t)))).ToList();
        }

        private static string GetAndOrExpression(string express)
        {
            //(1 Or 2 And 3)
            var tempStrs = express.Split(new char[] { ' ', '(', ')' }, StringSplitOptions.RemoveEmptyEntries);
            var lastAndOrOperator = "";
            string andOrExpression = "(";
            foreach (var str in tempStrs)
            {
                int sequenceNo = 0;
                if (int.TryParse(str, out sequenceNo))
                {
                    andOrExpression = string.Format("{0}{1}", andOrExpression, sequenceNo.ToString());
                    sequenceNo++;
                }
                else
                {
                    var currentAndOrOperator = str;
                    if (!string.IsNullOrEmpty(lastAndOrOperator) && lastAndOrOperator != currentAndOrOperator)
                    {
                        andOrExpression = string.Format("({0}) {1} ", andOrExpression, currentAndOrOperator);
                    }
                    else
                    {
                        andOrExpression = string.Format("{0} {1} ", andOrExpression, currentAndOrOperator);
                    }
                    lastAndOrOperator = currentAndOrOperator;
                }
            }
            andOrExpression += ")";
            return andOrExpression;
        }

        public async Task<RMRuleInfos> ConvertToEXORuleInfoAsync(Rule rule, bool isNewLogicAccount)
        {
            await ConvertRuleFiltersTimeZoneAsync(rule);
            List<SOFilterPolicy> filters = rule.SOFilters;
            List<RuleFilter> displayFilters = new List<RuleFilter>();
            List<string> ruleCriteria = new List<string>();
            List<PolicyLevel> filterLevels = new List<PolicyLevel>();
            foreach (var filter in filters)
            {
                ArchiverRuleFilter arFilter = new ArchiverRuleFilter(filter);
                RuleFilter newFilter = new RuleFilter()
                {
                    CombineMode = arFilter.CombineMode,
                    Condition = arFilter.Condition,
                    RuleType = arFilter.RuleType,
                    filterName = arFilter.RuleName,
                    Level = arFilter.Level,
                    SequenceNo = arFilter.SequenceNo,
                    Value1 = arFilter.Value1,
                    Value1Unit = arFilter.Value1Unit,
                    Value2 = arFilter.Value2,
                    Value2Unit = arFilter.Value2Unit,
                    FilterCretia = arFilter.FilterCretia(),
                };
                if (filter.RuleType == PolicyRuleType.Attachment)
                {
                    newFilter.Value1Unit = PolicyValueUnit.None;
                    newFilter.Value2Unit = PolicyValueUnit.None;
                }
                if (newFilter.Condition == ArchiverFilterCondition.FromTo)
                {
                    newFilter.StartTimeInfo = arFilter.GetFilterDateTimeInfo(true);
                    newFilter.EndTimeInfo = arFilter.GetFilterDateTimeInfo(false);
                }
                else if (newFilter.Condition == ArchiverFilterCondition.Before)
                {
                    newFilter.StartTimeInfo = arFilter.GetFilterDateTimeInfo(true);
                }
                displayFilters.Add(newFilter);
                ruleCriteria.Add(arFilter.FilterCretia());
                if (!filterLevels.Contains(arFilter.Level))
                {
                    filterLevels.Add(arFilter.Level);
                }
            }
            RMRuleInfos newRule = new RMRuleInfos()
            {
                RuleKeepDataOption = rule.KeepDataOption,
                //RelatedRecordOption = rule.RelatedRecordOption,
                //这里不要直接merge Local的方法，会导致页面rule view detail的bug
                ArchiverActions = GetArchiverRuleSettingType(rule),//Display rule action
                //Description = rule.Description,
                EnableManualApproval = rule.IsManualApproval,
                //DeleteRecords = rule.DeleteRecords,
                isChecked = false,
                //RuleId = rule.Id,
                //RuleName = rule.Name,
                RuleFilters = displayFilters,
                ExportFormat = rule.ExportType.ToString(),
                //FilterCombineMode = rule.AndOrExpression[rule.PolicyLevel].ToString(),
                RuleCretias = ruleCriteria,
                //MoveToRecordCenterSettings = rule.MoveToRecordCenterAndDelareSetting,
                RuleLevel = rule.PolicyLevel,
                ExportInfo = rule.ExportInfo,
                TagContentInfo = await ConvertAllTagContentInfoAsync(rule.TagContentInfo, isNewLogicAccount, rule.KeepDataOption, rule.PolicyLevel, false),
                //Modified = mGeneralSettingService.ConvertTiksToDateTime(rule.ModifyTime, true).FormaTime,
                //DisposalClass = rule.DisposalClass
                //Terms = GetRuleTermsName(terms),
                ExportDataBeforeArchiving = rule.ExportDataBeforeArchiving,
                MoveToArchiverTierWhenArchiving = rule.MoveToArchiverTierWhenArchiving,
                MoveToAnotherTierType = rule.MoveToAnotherTierType,
            };
            if (rule.ProfileType == GCommon.Contract.Server.Common.Profile.Object.ProfileType.ArchiverRule)
            {
                newRule.ModelType = AvePoint.GCommon.Contract.StorageOptimization.Object.RuleModel.SOArchiver;
            }
            else
            {
                newRule.ModelType = AvePoint.GCommon.Contract.StorageOptimization.Object.RuleModel.Records;
            }
            StringBuilder filterCombineModeString = new StringBuilder();
            foreach (var filterLevel in filterLevels)
            {
                filterCombineModeString.Append(GetAndOrExpression(rule.AndOrExpression[filterLevel]));
            }
            newRule.FilterCombineMode = filterCombineModeString.ToString().Replace("And", I18NEntity.GetString("RM_JS_Rule_ConditionAnd")).Replace("Or", I18NEntity.GetString("RM_JS_Rule_ConditionOr"));

            if (rule.ExportInfo != null && !rule.ExportType.Equals(AvePoint.GCommon.Contract.StorageOptimization.Object.ExportTypeValue.None))
            {
                newRule.EnableExport = true;
            }
            InitRMRuleManualApprovalInfo(rule, newRule);
            return newRule;
        }
        public async Task<RMRuleInfos> ConvertToPhysicalRuleInfoAsync(Rule rule, bool isNewLogicAccount)
        {
            await ConvertRuleFiltersTimeZoneAsync(rule);
            List<SOFilterPolicy> filters = rule.SOFilters;
            List<RuleFilter> displayFilters = new List<RuleFilter>();
            List<string> ruleCriteria = new List<string>();
            List<PolicyLevel> filterLevels = new List<PolicyLevel>();
            foreach (var filter in filters)
            {
                ArchiverRuleFilter arFilter = new ArchiverRuleFilter(filter);
                RuleFilter newFilter = new RuleFilter()
                {
                    CombineMode = arFilter.CombineMode,
                    Condition = arFilter.Condition,
                    RuleType = arFilter.RuleType,
                    filterName = arFilter.RuleName,
                    Level = arFilter.Level,
                    SequenceNo = arFilter.SequenceNo,
                    Value1 = arFilter.Value1,
                    Value1Unit = arFilter.Value1Unit,
                    Value2 = arFilter.Value2,
                    Value2Unit = arFilter.Value2Unit,
                    FilterCretia = arFilter.FilterCretia(),
                };
                if (filter.RuleType == PolicyRuleType.Attachment)
                {
                    newFilter.Value1Unit = PolicyValueUnit.None;
                    newFilter.Value2Unit = PolicyValueUnit.None;
                }
                if (newFilter.Condition == ArchiverFilterCondition.FromTo)
                {
                    newFilter.StartTimeInfo = arFilter.GetFilterDateTimeInfo(true);
                    newFilter.EndTimeInfo = arFilter.GetFilterDateTimeInfo(false);
                }
                else if (newFilter.Condition == ArchiverFilterCondition.Before)
                {
                    newFilter.StartTimeInfo = arFilter.GetFilterDateTimeInfo(true);
                }
                displayFilters.Add(newFilter);
                ruleCriteria.Add(arFilter.FilterCretia());
                if (!filterLevels.Contains(arFilter.Level))
                {
                    filterLevels.Add(arFilter.Level);
                }
            }
            RMRuleInfos newRule = new RMRuleInfos()
            {
                RuleKeepDataOption = rule.KeepDataOption,
                RelatedRecordOption = rule.RelatedRecordOption,
                DestroyEmptyBoxOnFolderRule = rule.IsDeleteParentBox,
                //这里不要直接merge Local的方法，会导致页面rule view detail的bug
                ArchiverActions = GetArchiverRuleSettingType(rule),//Display rule action
                //Description = rule.Description,
                EnableManualApproval = rule.IsManualApproval,
                //DeleteRecords = rule.DeleteRecords,
                isChecked = false,
                //RuleId = rule.Id,
                //RuleName = rule.Name,
                RuleFilters = displayFilters,
                ExportFormat = rule.ExportType.ToString(),
                //FilterCombineMode = rule.AndOrExpression[rule.PolicyLevel].ToString(),
                RuleCretias = ruleCriteria,
                //MoveToRecordCenterSettings = rule.MoveToRecordCenterAndDelareSetting,
                RuleLevel = rule.PolicyLevel,
                ExportInfo = rule.ExportInfo,
                TagContentInfo = await ConvertAllTagContentInfoAsync(rule.TagContentInfo, isNewLogicAccount, rule.KeepDataOption, rule.PolicyLevel, false),
                //Modified = mGeneralSettingService.ConvertTiksToDateTime(rule.ModifyTime, true).FormaTime,
                //DisposalClass = rule.DisposalClass
                //Terms = GetRuleTermsName(terms),
                StoragePolicyId = rule.StoragePolicyId,
                StoragePolicyName = rule.StoragePolicyName,
                ExportDataBeforeArchiving = rule.ExportDataBeforeArchiving,
                MoveToArchiverTierWhenArchiving = rule.MoveToArchiverTierWhenArchiving,
                MoveToAnotherTierType = rule.MoveToAnotherTierType,
                IsCalculationDisposalDate = rule.IsCalculationDisposalDate,
            };
            newRule.IsEnableRetention = rule.IsEnableStoreContentRetention;
            newRule.RetentionInfoList = this.ConvertStoreContentRetentionInfo(rule.StoreContentRetentionInfos);
            newRule.RetentionInfo = this.ConvertStoreContentRetentionInfo(rule.StoreContentRetentionInfos)?.FirstOrDefault();
            if (rule.ProfileType == GCommon.Contract.Server.Common.Profile.Object.ProfileType.ArchiverRule)
            {
                newRule.ModelType = AvePoint.GCommon.Contract.StorageOptimization.Object.RuleModel.SOArchiver;
            }
            else
            {
                newRule.ModelType = AvePoint.GCommon.Contract.StorageOptimization.Object.RuleModel.Records;
            }
            StringBuilder filterCombineModeString = new StringBuilder();
            foreach (var filterLevel in filterLevels)
            {
                filterCombineModeString.Append(GetAndOrExpression(rule.AndOrExpression[filterLevel]));
            }
            newRule.FilterCombineMode = filterCombineModeString.ToString().Replace("And", I18NEntity.GetString("RM_JS_Rule_ConditionAnd")).Replace("Or", I18NEntity.GetString("RM_JS_Rule_ConditionOr"));

            if (rule.ExportInfo != null && !rule.ExportType.Equals(AvePoint.GCommon.Contract.StorageOptimization.Object.ExportTypeValue.None))
            {
                newRule.EnableExport = true;
            }
            InitRMRuleManualApprovalInfo(rule, newRule);
            return newRule;
        }
        public async Task<RMRuleInfos> ConvertToFSRuleInfoAsync(Rule rule)
        {
            await ConvertRuleFiltersTimeZoneAsync(rule);
            List<SOFilterPolicy> filters = rule.SOFilters;
            List<RuleFilter> displayFilters = new List<RuleFilter>();
            List<string> ruleCriteria = new List<string>();
            List<PolicyLevel> filterLevels = new List<PolicyLevel>();
            foreach (var filter in filters)
            {
                ArchiverRuleFilter arFilter = new ArchiverRuleFilter(filter);
                RuleFilter newFilter = new RuleFilter()
                {
                    CombineMode = arFilter.CombineMode,
                    Condition = arFilter.Condition,
                    RuleType = arFilter.RuleType,
                    filterName = arFilter.RuleName,
                    Level = arFilter.Level,
                    SequenceNo = arFilter.SequenceNo,
                    Value1 = arFilter.Value1,
                    Value1Unit = arFilter.Value1Unit,
                    Value2 = arFilter.Value2,
                    Value2Unit = arFilter.Value2Unit,
                    FilterCretia = arFilter.FilterCretia(),
                };

                if (newFilter.Condition == ArchiverFilterCondition.FromTo)
                {
                    newFilter.StartTimeInfo = arFilter.GetFilterDateTimeInfo(true);
                    newFilter.EndTimeInfo = arFilter.GetFilterDateTimeInfo(false);
                }
                else if (newFilter.Condition == ArchiverFilterCondition.Before)
                {
                    newFilter.StartTimeInfo = arFilter.GetFilterDateTimeInfo(true);
                }
                displayFilters.Add(newFilter);
                ruleCriteria.Add(arFilter.FilterCretia());
                if (!filterLevels.Contains(arFilter.Level))
                {
                    filterLevels.Add(arFilter.Level);
                }
            }
            RMRuleInfos newRule = new RMRuleInfos()
            {
                RuleKeepDataOption = rule.KeepDataOption,
                RelatedRecordOption = rule.RelatedRecordOption,
                //这里不要直接merge Local的方法，会导致页面rule view detail的bug
                ArchiverActions = GetArchiverRuleSettingType(rule),//Display rule action
                //Description = rule.Description,
                EnableManualApproval = rule.IsManualApproval,
                //DeleteRecords = rule.DeleteRecords,
                isChecked = false,
                //RuleId = rule.Id,
                //RuleName = rule.Name,
                RuleFilters = displayFilters,
                ExportFormat = rule.ExportType.ToString(),
                //FilterCombineMode = rule.AndOrExpression[rule.PolicyLevel].ToString(),
                RuleCretias = ruleCriteria,
                //MoveToRecordCenterSettings = rule.MoveToRecordCenterAndDelareSetting,
                RuleLevel = rule.PolicyLevel,
                ExportInfo = rule.ExportInfo,
                //TagContentInfo = rule.TagContentInfo?.Select(t => ConvertTagContentInfo(t)).ToList(),
                //Modified = mGeneralSettingService.ConvertTiksToDateTime(rule.ModifyTime, true).FormaTime,
                //DisposalClass = rule.DisposalClass
                //Terms = GetRuleTermsName(terms),
                ExportDataBeforeArchiving = rule.ExportDataBeforeArchiving,
                MoveToArchiverTierWhenArchiving = rule.MoveToArchiverTierWhenArchiving,
                MoveToAnotherTierType = rule.MoveToAnotherTierType,
                StoragePolicyId = rule.StoragePolicyId,
                StoragePolicyName = rule.StoragePolicyName,
                
            };
            if (rule.ProfileType == GCommon.Contract.Server.Common.Profile.Object.ProfileType.ArchiverRule)
            {
                newRule.ModelType = AvePoint.GCommon.Contract.StorageOptimization.Object.RuleModel.SOArchiver;
            }
            else
            {
                newRule.ModelType = AvePoint.GCommon.Contract.StorageOptimization.Object.RuleModel.Records;
            }
            StringBuilder filterCombineModeString = new StringBuilder();
            foreach (var filterLevel in filterLevels)
            {
                filterCombineModeString.Append(GetAndOrExpression(rule.AndOrExpression[filterLevel]));
            }
            newRule.FilterCombineMode = filterCombineModeString.ToString().Replace("And", I18NEntity.GetString("RM_JS_Rule_ConditionAnd")).Replace("Or", I18NEntity.GetString("RM_JS_Rule_ConditionOr"));

            if (rule.ExportInfo != null && !rule.ExportType.Equals(AvePoint.GCommon.Contract.StorageOptimization.Object.ExportTypeValue.None))
            {
                newRule.EnableExport = true;
            }
            InitRMRuleManualApprovalInfo(rule, newRule);
            return newRule;
        }

        public async Task<RMRuleInfos> ConvertToAzureFileRuleInfoAsync(Rule rule)
        {
            await ConvertRuleFiltersTimeZoneAsync(rule);
            List<SOFilterPolicy> filters = rule.SOFilters;
            List<RuleFilter> displayFilters = new List<RuleFilter>();
            List<string> ruleCriteria = new List<string>();
            List<PolicyLevel> filterLevels = new List<PolicyLevel>();
            foreach (var filter in filters)
            {
                ArchiverRuleFilter arFilter = new ArchiverRuleFilter(filter);
                RuleFilter newFilter = new RuleFilter()
                {
                    CombineMode = arFilter.CombineMode,
                    Condition = arFilter.Condition,
                    RuleType = arFilter.RuleType,
                    filterName = arFilter.RuleName,
                    Level = arFilter.Level,
                    SequenceNo = arFilter.SequenceNo,
                    Value1 = arFilter.Value1,
                    Value1Unit = arFilter.Value1Unit,
                    Value2 = arFilter.Value2,
                    Value2Unit = arFilter.Value2Unit,
                    FilterCretia = arFilter.FilterCretia(),
                };

                if (newFilter.Condition == ArchiverFilterCondition.FromTo)
                {
                    newFilter.StartTimeInfo = arFilter.GetFilterDateTimeInfo(true);
                    newFilter.EndTimeInfo = arFilter.GetFilterDateTimeInfo(false);
                }
                else if (newFilter.Condition == ArchiverFilterCondition.Before)
                {
                    newFilter.StartTimeInfo = arFilter.GetFilterDateTimeInfo(true);
                }
                displayFilters.Add(newFilter);
                ruleCriteria.Add(arFilter.FilterCretia());
                if (!filterLevels.Contains(arFilter.Level))
                {
                    filterLevels.Add(arFilter.Level);
                }
            }
            RMRuleInfos newRule = new RMRuleInfos()
            {
                RuleKeepDataOption = rule.KeepDataOption,
                RelatedRecordOption = rule.RelatedRecordOption,
                //这里不要直接merge Local的方法，会导致页面rule view detail的bug
                ArchiverActions = GetArchiverRuleSettingType(rule),//Display rule action
                //Description = rule.Description,
                EnableManualApproval = rule.IsManualApproval,
                //DeleteRecords = rule.DeleteRecords,
                isChecked = false,
                //RuleId = rule.Id,
                //RuleName = rule.Name,
                RuleFilters = displayFilters,
                ExportFormat = rule.ExportType.ToString(),
                //FilterCombineMode = rule.AndOrExpression[rule.PolicyLevel].ToString(),
                RuleCretias = ruleCriteria,
                //MoveToRecordCenterSettings = rule.MoveToRecordCenterAndDelareSetting,
                RuleLevel = rule.PolicyLevel,
                ExportInfo = rule.ExportInfo,
                //TagContentInfo = rule.TagContentInfo?.Select(t => ConvertTagContentInfo(t)).ToList(),
                //Modified = mGeneralSettingService.ConvertTiksToDateTime(rule.ModifyTime, true).FormaTime,
                //DisposalClass = rule.DisposalClass
                //Terms = GetRuleTermsName(terms),
                ExportDataBeforeArchiving = rule.ExportDataBeforeArchiving,
                MoveToArchiverTierWhenArchiving = rule.MoveToArchiverTierWhenArchiving,
                MoveToAnotherTierType = rule.MoveToAnotherTierType,
            };
            if (rule.ProfileType == GCommon.Contract.Server.Common.Profile.Object.ProfileType.ArchiverRule)
            {
                newRule.ModelType = AvePoint.GCommon.Contract.StorageOptimization.Object.RuleModel.SOArchiver;
            }
            else
            {
                newRule.ModelType = AvePoint.GCommon.Contract.StorageOptimization.Object.RuleModel.Records;
            }
            StringBuilder filterCombineModeString = new StringBuilder();
            foreach (var filterLevel in filterLevels)
            {
                filterCombineModeString.Append(GetAndOrExpression(rule.AndOrExpression[filterLevel]));
            }
            newRule.FilterCombineMode = filterCombineModeString.ToString().Replace("And", I18NEntity.GetString("RM_JS_Rule_ConditionAnd")).Replace("Or", I18NEntity.GetString("RM_JS_Rule_ConditionOr"));

            if (rule.ExportInfo != null && !rule.ExportType.Equals(AvePoint.GCommon.Contract.StorageOptimization.Object.ExportTypeValue.None))
            {
                newRule.EnableExport = true;
            }
            InitRMRuleManualApprovalInfo(rule, newRule);
            return newRule;
        }

        //Box

        private async Task<RMRuleInfos> ConvertToBoxRuleInfoAsync(Rule rule)
        {
            await ConvertRuleFiltersTimeZoneAsync(rule);
            List<SOFilterPolicy> filters = rule.SOFilters;
            List<RuleFilter> displayFilters = new List<RuleFilter>();
            List<string> ruleCriteria = new List<string>();
            List<PolicyLevel> filterLevels = new List<PolicyLevel>();
            foreach (var filter in filters)
            {
                ArchiverRuleFilter arFilter = new ArchiverRuleFilter(filter);
                RuleFilter newFilter = new RuleFilter()
                {
                    CombineMode = arFilter.CombineMode,
                    Condition = arFilter.Condition,
                    RuleType = arFilter.RuleType,
                    filterName = arFilter.RuleName,
                    Level = arFilter.Level,
                    SequenceNo = arFilter.SequenceNo,
                    Value1 = arFilter.Value1,
                    Value1Unit = arFilter.Value1Unit,
                    Value2 = arFilter.Value2,
                    Value2Unit = arFilter.Value2Unit,
                    FilterCretia = arFilter.FilterCretia(),
                };

                if (newFilter.Condition == ArchiverFilterCondition.FromTo)
                {
                    newFilter.StartTimeInfo = arFilter.GetFilterDateTimeInfo(true);
                    newFilter.EndTimeInfo = arFilter.GetFilterDateTimeInfo(false);
                }
                else if (newFilter.Condition == ArchiverFilterCondition.Before)
                {
                    newFilter.StartTimeInfo = arFilter.GetFilterDateTimeInfo(true);
                }
                displayFilters.Add(newFilter);
                ruleCriteria.Add(arFilter.FilterCretia());
                if (!filterLevels.Contains(arFilter.Level))
                {
                    filterLevels.Add(arFilter.Level);
                }
            }
            RMRuleInfos newRule = new RMRuleInfos()
            {
                RuleKeepDataOption = rule.KeepDataOption,
                RelatedRecordOption = rule.RelatedRecordOption,
                ArchiverActions = GetArchiverRuleSettingType(rule),
                EnableManualApproval = rule.IsManualApproval,
                isChecked = false,
                RuleFilters = displayFilters,
                ExportFormat = rule.ExportType.ToString(),
                RuleCretias = ruleCriteria,
                RuleLevel = rule.PolicyLevel,
                ExportInfo = rule.ExportInfo,
                ExportDataBeforeArchiving = rule.ExportDataBeforeArchiving,
                MoveToArchiverTierWhenArchiving = rule.MoveToArchiverTierWhenArchiving,
                MoveToAnotherTierType = rule.MoveToAnotherTierType,
            };
            if (rule.ProfileType == GCommon.Contract.Server.Common.Profile.Object.ProfileType.ArchiverRule)
            {
                newRule.ModelType = AvePoint.GCommon.Contract.StorageOptimization.Object.RuleModel.SOArchiver;
            }
            else
            {
                newRule.ModelType = AvePoint.GCommon.Contract.StorageOptimization.Object.RuleModel.Records;
            }
            StringBuilder filterCombineModeString = new StringBuilder();
            foreach (var filterLevel in filterLevels)
            {
                filterCombineModeString.Append(GetAndOrExpression(rule.AndOrExpression[filterLevel]));
            }
            newRule.FilterCombineMode = filterCombineModeString.ToString().Replace("And", I18NEntity.GetString("RM_JS_Rule_ConditionAnd")).Replace("Or", I18NEntity.GetString("RM_JS_Rule_ConditionOr"));

            if (rule.ExportInfo != null && !rule.ExportType.Equals(AvePoint.GCommon.Contract.StorageOptimization.Object.ExportTypeValue.None))
            {
                newRule.EnableExport = true;
            }
            InitRMRuleManualApprovalInfo(rule, newRule);
            return newRule;
        }


        public async Task<RMRuleInfos> ConvertToConnectorRuleInfoAsync(Rule rule)
        {
            await ConvertRuleFiltersTimeZoneAsync(rule);
            List<SOFilterPolicy> filters = rule.SOFilters;
            List<RuleFilter> displayFilters = new List<RuleFilter>();
            List<string> ruleCriteria = new List<string>();
            List<PolicyLevel> filterLevels = new List<PolicyLevel>();
            foreach (var filter in filters)
            {
                ArchiverRuleFilter arFilter = new ArchiverRuleFilter(filter);
                RuleFilter newFilter = new RuleFilter()
                {
                    CombineMode = arFilter.CombineMode,
                    Condition = arFilter.Condition,
                    RuleType = arFilter.RuleType,
                    filterName = arFilter.RuleName,
                    Level = arFilter.Level,
                    SequenceNo = arFilter.SequenceNo,
                    Value1 = arFilter.Value1,
                    Value1Unit = arFilter.Value1Unit,
                    Value2 = arFilter.Value2,
                    Value2Unit = arFilter.Value2Unit,
                    FilterCretia = arFilter.FilterCretia(),
                };

                if (newFilter.Condition == ArchiverFilterCondition.FromTo)
                {
                    newFilter.StartTimeInfo = arFilter.GetFilterDateTimeInfo(true);
                    newFilter.EndTimeInfo = arFilter.GetFilterDateTimeInfo(false);
                }
                else if (newFilter.Condition == ArchiverFilterCondition.Before)
                {
                    newFilter.StartTimeInfo = arFilter.GetFilterDateTimeInfo(true);
                }
                displayFilters.Add(newFilter);
                ruleCriteria.Add(arFilter.FilterCretia());
                if (!filterLevels.Contains(arFilter.Level))
                {
                    filterLevels.Add(arFilter.Level);
                }
            }
            RMRuleInfos newRule = new RMRuleInfos()
            {
                RuleKeepDataOption = rule.KeepDataOption,
                RelatedRecordOption = rule.RelatedRecordOption,
                //这里不要直接merge Local的方法，会导致页面rule view detail的bug
                ArchiverActions = GetArchiverRuleSettingType(rule),//Display rule action
                //Description = rule.Description,
                EnableManualApproval = rule.IsManualApproval,
                //DeleteRecords = rule.DeleteRecords,
                isChecked = false,
                //RuleId = rule.Id,
                //RuleName = rule.Name,
                RuleFilters = displayFilters,
                ExportFormat = rule.ExportType.ToString(),
                //FilterCombineMode = rule.AndOrExpression[rule.PolicyLevel].ToString(),
                RuleCretias = ruleCriteria,
                //MoveToRecordCenterSettings = rule.MoveToRecordCenterAndDelareSetting,
                RuleLevel = rule.PolicyLevel,
                ExportInfo = rule.ExportInfo,
                //TagContentInfo = rule.TagContentInfo?.Select(t => ConvertTagContentInfo(t)).ToList(),
                //Modified = mGeneralSettingService.ConvertTiksToDateTime(rule.ModifyTime, true).FormaTime,
                //DisposalClass = rule.DisposalClass
                //Terms = GetRuleTermsName(terms),
                ExportDataBeforeArchiving = rule.ExportDataBeforeArchiving,
                MoveToArchiverTierWhenArchiving = rule.MoveToArchiverTierWhenArchiving,
                MoveToAnotherTierType = rule.MoveToAnotherTierType,
            };
            if (rule.ProfileType == GCommon.Contract.Server.Common.Profile.Object.ProfileType.ArchiverRule)
            {
                newRule.ModelType = AvePoint.GCommon.Contract.StorageOptimization.Object.RuleModel.SOArchiver;
            }
            else
            {
                newRule.ModelType = AvePoint.GCommon.Contract.StorageOptimization.Object.RuleModel.Records;
            }
            StringBuilder filterCombineModeString = new StringBuilder();
            foreach (var filterLevel in filterLevels)
            {
                filterCombineModeString.Append(GetAndOrExpression(rule.AndOrExpression[filterLevel]));
            }
            newRule.FilterCombineMode = filterCombineModeString.ToString().Replace("And", I18NEntity.GetString("RM_JS_Rule_ConditionAnd")).Replace("Or", I18NEntity.GetString("RM_JS_Rule_ConditionOr"));

            if (rule.ExportInfo != null && !rule.ExportType.Equals(AvePoint.GCommon.Contract.StorageOptimization.Object.ExportTypeValue.None))
            {
                newRule.EnableExport = true;
            }
            InitRMRuleManualApprovalInfo(rule, newRule);
            return newRule;
        }

        public async Task<RMRuleInfos> ConvertToSPLocalRuleInfoAsync(Rule rule, bool isNewLogicAccount)
        {
            await ConvertRuleFiltersTimeZoneAsync(rule);
            List<SOFilterPolicy> filters = rule.SOFilters;
            List<RuleFilter> displayFilters = new List<RuleFilter>();
            List<string> ruleCriteria = new List<string>();
            if (filters != null)
            {
                foreach (var filter in filters)
                {
                    ArchiverRuleFilter arFilter = new ArchiverRuleFilter(filter);
                    RuleFilter newFilter = new RuleFilter()
                    {
                        CombineMode = arFilter.CombineMode,
                        Condition = arFilter.Condition,
                        RuleType = arFilter.RuleType,
                        filterName = arFilter.RuleName,
                        Level = arFilter.Level,
                        SequenceNo = arFilter.SequenceNo,
                        Value1 = arFilter.Value1,
                        Value1Unit = arFilter.Value1Unit,
                        Value2 = arFilter.Value2,
                        Value2Unit = arFilter.Value2Unit,
                        FilterCretia = arFilter.FilterCretia(),
                    };
                    if (filter.RuleType == PolicyRuleType.Attachment)
                    {
                        newFilter.Value1Unit = PolicyValueUnit.None;
                        newFilter.Value2Unit = PolicyValueUnit.None;
                    }
                    if (newFilter.Condition == ArchiverFilterCondition.FromTo)
                    {
                        newFilter.StartTimeInfo = arFilter.GetFilterDateTimeInfo(true);
                        newFilter.EndTimeInfo = arFilter.GetFilterDateTimeInfo(false);
                    }
                    else if (newFilter.Condition == ArchiverFilterCondition.Before)
                    {
                        newFilter.StartTimeInfo = arFilter.GetFilterDateTimeInfo(true);
                    }
                    displayFilters.Add(newFilter);
                    ruleCriteria.Add(arFilter.FilterCretia());
                }
            }

            RMRuleInfos newRule = new RMRuleInfos()
            {
                RuleKeepDataOption = rule.KeepDataOption,
                RelatedRecordOption = rule.RelatedRecordOption,
                //这里不要直接merge Local的方法，会导致页面rule view detail的bug
                ArchiverActions = GetArchiverRuleSettingType(rule),//Display rule action
                Description = rule.Description,
                EnableManualApproval = rule.IsManualApproval,
                DeleteRecords = rule.DeleteRecords,
                IncludeDeleteRecordLabel = rule.IncludeDeleteRecordLabel,
                LockRecordBeforeDestroy = rule.LockRecordBeforeDestroy,
                DeleteSiteCollectionToRecycleBin = rule.IsDeleteSiteCollectionToRecycleBin(),
                DeleteToRecycleBin = rule.DeleteToRecycleBin,
                DeclareLinkFile = rule.DeclareLinkFile,
                isChecked = false,
                RuleId = rule.Id,
                RuleName = rule.Name,
                RuleFilters = displayFilters,
                ExportFormat = rule.ExportType.ToString(),
                FilterCombineMode = rule.AndOrExpression == null ? "" : GetAndOrExpression(rule.AndOrExpression[rule.PolicyLevel].ToString()).Replace("And", I18NEntity.GetString("RM_JS_Rule_ConditionAnd")).Replace("Or", I18NEntity.GetString("RM_JS_Rule_ConditionOr")),
                RuleCretias = ruleCriteria,
                MoveToRecordCenterSettings = rule.MoveToRecordCenterAndDelareSetting,
                MoveToLocationPasswordEncrypted = true,
                RuleLevel = rule.PolicyLevel,
                ExportInfo = rule.ExportInfo,
                TagContentInfo = await ConvertAllTagContentInfoAsync(rule.TagContentInfo, isNewLogicAccount, rule.KeepDataOption, rule.PolicyLevel, false),
                Modified = (await mGeneralSettingService.ConvertTiksToDateTimeAsync(rule.ModifyTime, true)).FormaTime,
                DisposalClass = rule.DisposalClass,
                //Terms = GetRuleTermsName(terms),
                ExportDataBeforeArchiving = rule.ExportDataBeforeArchiving,
                MoveToArchiverTierWhenArchiving = rule.MoveToArchiverTierWhenArchiving,
                MoveToAnotherTierType = rule.MoveToAnotherTierType,
            };
            if (rule.ProfileType == GCommon.Contract.Server.Common.Profile.Object.ProfileType.ArchiverRule)
            {
                newRule.ModelType = AvePoint.GCommon.Contract.StorageOptimization.Object.RuleModel.SOArchiver;
            }
            else
            {
                newRule.ModelType = AvePoint.GCommon.Contract.StorageOptimization.Object.RuleModel.Records;
            }
            if (rule.MoveToRecordCenterAndDelareSetting != null
                && rule.MoveToRecordCenterAndDelareSetting.DestinationLocation != null
                && !string.IsNullOrEmpty(rule.MoveToRecordCenterAndDelareSetting.DestinationLocation.Password))
            {
                newRule.MoveDto = new MoveToDto();
                newRule.MoveDto.IsSpecifyLocation = true;
                newRule.MoveDto.LocationPath = rule.MoveToRecordCenterAndDelareSetting.DestinationLocation.Url;
                newRule.MoveDto.NotDeclareMovedData = rule.MoveToRecordCenterAndDelareSetting.DelaredRecord;
                newRule.MoveDto.FileNameConflictOption = FileNameConflictOption.Skip;
            }
            if (rule.ExportInfo != null && !rule.ExportType.Equals(AvePoint.GCommon.Contract.StorageOptimization.Object.ExportTypeValue.None))
            {
                newRule.EnableExport = true;
            }
            InitRMRuleManualApprovalInfo(rule, newRule);
            return newRule;
        }

        public async Task<RMRuleInfos> ConvertToOneDriveRuleInfoAsync(Rule rule, bool isNewLogicAccount)
        {
            await ConvertRuleFiltersTimeZoneAsync(rule);
            List<SOFilterPolicy> filters = rule.SOFilters;
            List<RuleFilter> displayFilters = new List<RuleFilter>();
            List<string> ruleCriteria = new List<string>();
            if (filters != null)
            {
                foreach (var filter in filters)
                {
                    ArchiverRuleFilter arFilter = new ArchiverRuleFilter(filter);
                    RuleFilter newFilter = new RuleFilter()
                    {
                        CombineMode = arFilter.CombineMode,
                        Condition = arFilter.Condition,
                        RuleType = arFilter.RuleType,
                        filterName = arFilter.RuleName,
                        Level = arFilter.Level,
                        SequenceNo = arFilter.SequenceNo,
                        Value1 = arFilter.Value1,
                        Value1Unit = arFilter.Value1Unit,
                        Value2 = arFilter.Value2,
                        Value2Unit = arFilter.Value2Unit,
                        FilterCretia = arFilter.FilterCretia(),
                    };
                    if (filter.RuleType == PolicyRuleType.Attachment)
                    {
                        newFilter.Value1Unit = PolicyValueUnit.None;
                        newFilter.Value2Unit = PolicyValueUnit.None;
                    }
                    if (newFilter.Condition == ArchiverFilterCondition.FromTo)
                    {
                        newFilter.StartTimeInfo = arFilter.GetFilterDateTimeInfo(true);
                        newFilter.EndTimeInfo = arFilter.GetFilterDateTimeInfo(false);
                    }
                    else if (newFilter.Condition == ArchiverFilterCondition.Before)
                    {
                        newFilter.StartTimeInfo = arFilter.GetFilterDateTimeInfo(true);
                    }
                    displayFilters.Add(newFilter);
                    ruleCriteria.Add(arFilter.FilterCretia());
                }
            }

            RMRuleInfos newRule = new RMRuleInfos()
            {
                RuleKeepDataOption = rule.KeepDataOption,
                RelatedRecordOption = rule.RelatedRecordOption,
                //这里不要直接merge Local的方法，会导致页面rule view detail的bug
                ArchiverActions = GetArchiverRuleSettingType(rule, SOSourceFlag.OneDrive),//Display rule action
                Description = rule.Description,
                EnableManualApproval = rule.IsManualApproval,
                DeleteRecords = rule.DeleteRecords,
                IncludeDeleteRecordLabel = rule.IncludeDeleteRecordLabel,
                LockRecordBeforeDestroy = rule.LockRecordBeforeDestroy,
                DeleteSiteCollectionToRecycleBin = rule.IsDeleteSiteCollectionToRecycleBin((int)SOSourceFlag.OneDrive),
                DeleteToRecycleBin = rule.DeleteToRecycleBin,
                DeclareLinkFile = rule.DeclareLinkFile,
                isChecked = false,
                RuleId = rule.Id,
                RuleName = rule.Name,
                RuleFilters = displayFilters,
                ExportFormat = rule.ExportType.ToString(),
                FilterCombineMode = rule.AndOrExpression == null ? "" : GetAndOrExpression(rule.AndOrExpression[rule.PolicyLevel].ToString()).Replace("And", I18NEntity.GetString("RM_JS_Rule_ConditionAnd")).Replace("Or", I18NEntity.GetString("RM_JS_Rule_ConditionOr")),
                RuleCretias = ruleCriteria,
                MoveToRecordCenterSettings = rule.MoveToRecordCenterAndDelareSetting,
                MoveToLocationPasswordEncrypted = true,
                RuleLevel = rule.PolicyLevel,
                ExportInfo = rule.ExportInfo,
                TagContentInfo = await ConvertAllTagContentInfoAsync(rule.TagContentInfo, isNewLogicAccount, rule.KeepDataOption, rule.PolicyLevel, AccountUtility.IsSupportRecordLabel()),
                Modified = (await mGeneralSettingService.ConvertTiksToDateTimeAsync(rule.ModifyTime, true)).FormaTime,
                DisposalClass = rule.DisposalClass,
                LeaveStubMessage = rule.LeaveStubMessage,
                IsRestoreLink = rule.IsRestoreLink,
                //IsEnableRetention = rule.IsEnableRetention,
                //RetentionInfo = await this.ConvertRetentionInfoAsync(rule.RetentionInfo),
                StoragePolicyId = rule.StoragePolicyId,
                StoragePolicyName = rule.StoragePolicyName,
                StubTemplateId = rule.StubTemplateId,
                StubTemplateName = rule.StubTemplateName,
                ExportDataBeforeArchiving = rule.ExportDataBeforeArchiving,
                MoveToArchiverTierWhenArchiving = rule.MoveToArchiverTierWhenArchiving,
                MoveToAnotherTierType = rule.MoveToArchiverTierWhenArchiving ? (int)Storage.AccessTierType.Archive : rule.MoveToAnotherTierType ?? (int)Storage.AccessTierType.Other,
                KeepLatestMajorAndMinorVersionAndArchiveOthers = rule.KeepLatestMajorAndMinorVersionAndArchiveOthers,
                KeepLatestMajorAndMinorVersion = rule.KeepLatestMajorAndMinorVersion,
                ArchiverOnlyLastestVersion = rule.ArchiverOnlyLastestVersion,
                ArchivedLatestVersion = rule.ArchivedLatestVersion
                //Terms = GetRuleTermsName(terms),
            };

            if (AccountUtility.IsSupportRecordLabel() && (newRule.RuleKeepDataOption & (int)KeepDataStatus.DeclareRecord) == (int)KeepDataStatus.DeclareRecord)
            {
                newRule.RuleKeepDataOption -= (int)KeepDataStatus.DeclareRecord;
                if ((newRule.RuleKeepDataOption & (int)KeepDataStatus.TagContent) != (int)KeepDataStatus.TagContent)
                {
                    newRule.RuleKeepDataOption += (int)KeepDataOption.TagContent;
                }
            }

            if ((rule.KeepDataOption & (int)KeepDataStatus.Archive) == (int)KeepDataStatus.Archive || (rule.KeepDataOption & (int)KeepDataStatus.ArchiveAndLeaveStub) == (int)KeepDataStatus.ArchiveAndLeaveStub)
            {
                newRule.IsEnableRetention = rule.IsEnableRetention;
                newRule.RetentionInfo = await this.ConvertRetentionInfoAsync(rule.RetentionInfo);
            }
            else
            {
                newRule.IsEnableRetention = rule.IsEnableStoreContentRetention;
                newRule.RetentionInfoList = this.ConvertStoreContentRetentionInfo(rule.StoreContentRetentionInfos);
                newRule.RetentionInfo = this.ConvertStoreContentRetentionInfo(rule.StoreContentRetentionInfos)?.FirstOrDefault();
            }
            if (rule.ProfileType == GCommon.Contract.Server.Common.Profile.Object.ProfileType.ArchiverRule)
            {
                newRule.ModelType = AvePoint.GCommon.Contract.StorageOptimization.Object.RuleModel.SOArchiver;
            }
            else
            {
                newRule.ModelType = AvePoint.GCommon.Contract.StorageOptimization.Object.RuleModel.Records;
            }
            if (rule.MoveToRecordCenterAndDelareSetting != null
                && rule.MoveToRecordCenterAndDelareSetting.DestinationLocation != null
                && !string.IsNullOrEmpty(rule.MoveToRecordCenterAndDelareSetting.DestinationLocation.Password))
            {
                newRule.MoveDto = new MoveToDto();
                newRule.MoveDto.IsSpecifyLocation = true;
                newRule.MoveDto.LocationPath = rule.MoveToRecordCenterAndDelareSetting.DestinationLocation.Url;
                newRule.MoveDto.NotDeclareMovedData = rule.MoveToRecordCenterAndDelareSetting.DelaredRecord;
                newRule.MoveDto.FileNameConflictOption = FileNameConflictOption.Skip;
            }
            if (rule.ExportInfo != null && !rule.ExportType.Equals(AvePoint.GCommon.Contract.StorageOptimization.Object.ExportTypeValue.None))
            {
                newRule.EnableExport = true;
            }
            if ((rule.KeepDataOption & 128) == (int)KeepDataStatus.LinkToDocument || (rule.KeepDataOption & 2048) == (int)KeepDataStatus.ArchiveAndLeaveStub)
            {
                newRule.LeaveStubMessage = !string.IsNullOrEmpty(rule.LeaveStubMessage) ? HttpUtility.HtmlDecode(rule.LeaveStubMessage) : I18NEntity.GetString("RM_RDM_CreateRule_LeaveStubOptionMessage_Default");
            }
            InitRMRuleManualApprovalInfo(rule, newRule);
            return newRule;
        }

        public async Task<RMRuleInfos> ConvertToTeamsRuleInfoAsync(Rule rule, bool isNewLogicAccount)
        {
            await ConvertRuleFiltersTimeZoneAsync(rule);
            List<SOFilterPolicy> filters = rule.SOFilters;
            List<RuleFilter> displayFilters = new List<RuleFilter>();
            List<string> ruleCriteria = new List<string>();
            if (filters != null)
            {
                foreach (var filter in filters)
                {
                    ArchiverRuleFilter arFilter = new ArchiverRuleFilter(filter);
                    try
                    {
                        var ruleType = arFilter.RuleType;
                    }
                    catch (Exception ex)
                    {
                        // TODO: need review
                        logger.Error($"Get rule type failed. {ex}");
                        continue;
                    }
                    RuleFilter newFilter = new RuleFilter()
                    {
                        CombineMode = arFilter.CombineMode,
                        Condition = arFilter.Condition,
                        RuleType = arFilter.RuleType,
                        filterName = arFilter.RuleName,
                        Level = arFilter.Level,
                        SequenceNo = arFilter.SequenceNo,
                        Value1 = arFilter.Value1,
                        Value1Unit = arFilter.Value1Unit,
                        Value2 = arFilter.Value2,
                        Value2Unit = arFilter.Value2Unit,
                        Value3 = arFilter.Value3,
                        Value3Unit = arFilter.Value3Unit,
                        FilterCretia = arFilter.FilterCretia(),
                    };
                    if (filter.RuleType == PolicyRuleType.Attachment)
                    {
                        newFilter.Value1Unit = PolicyValueUnit.None;
                        newFilter.Value2Unit = PolicyValueUnit.None;
                    }
                    if (newFilter.Condition == ArchiverFilterCondition.FromTo)
                    {
                        newFilter.StartTimeInfo = arFilter.GetFilterDateTimeInfo(true);
                        newFilter.EndTimeInfo = arFilter.GetFilterDateTimeInfo(false);
                    }
                    else if (newFilter.Condition == ArchiverFilterCondition.Before)
                    {
                        newFilter.StartTimeInfo = arFilter.GetFilterDateTimeInfo(true);
                    }
                    displayFilters.Add(newFilter);
                    ruleCriteria.Add(arFilter.FilterCretia());
                }
            }

            RMRuleInfos newRule = new RMRuleInfos()
            {
                RuleKeepDataOption = rule.KeepDataOption,
                RelatedRecordOption = rule.RelatedRecordOption,
                //这里不要直接merge Local的方法，会导致页面rule view detail的bug
                ArchiverActions = GetArchiverRuleSettingType(rule),//Display rule action
                Description = rule.Description,
                EnableManualApproval = rule.IsManualApproval,
                DeleteRecords = rule.DeleteRecords,
                IncludeDeleteRecordLabel = rule.IncludeDeleteRecordLabel,
                LockRecordBeforeDestroy = rule.LockRecordBeforeDestroy,
                DeleteSiteCollectionToRecycleBin = rule.IsDeleteSiteCollectionToRecycleBin(),
                DeleteToRecycleBin = rule.DeleteToRecycleBin,
                DeclareLinkFile = rule.DeclareLinkFile,
                isChecked = false,
                RuleId = rule.Id,
                RuleName = rule.Name,
                RuleFilters = displayFilters,
                ExportFormat = rule.ExportType.ToString(),
                FilterCombineMode = rule.AndOrExpression == null ? "" : GetAndOrExpression(rule.AndOrExpression[rule.PolicyLevel].ToString()).Replace("And", I18NEntity.GetString("RM_JS_Rule_ConditionAnd")).Replace("Or", I18NEntity.GetString("RM_JS_Rule_ConditionOr")),
                RuleCretias = ruleCriteria,
                MoveToRecordCenterSettings = rule.MoveToRecordCenterAndDelareSetting,
                MoveToLocationPasswordEncrypted = true,
                RuleLevel = rule.PolicyLevel,
                ExportInfo = rule.ExportInfo,
                TagContentInfo = await ConvertAllTagContentInfoAsync(rule.TagContentInfo, isNewLogicAccount, rule.KeepDataOption, rule.PolicyLevel, AccountUtility.IsSupportRecordLabel()),
                Modified = (await mGeneralSettingService.ConvertTiksToDateTimeAsync(rule.ModifyTime, true)).FormaTime,
                DisposalClass = rule.DisposalClass,
                LeaveStubMessage = rule.LeaveStubMessage,
                IsRestoreLink = rule.IsRestoreLink,
                //IsEnableRetention = rule.IsEnableRetention,
                //RetentionInfo = await this.ConvertRetentionInfoAsync(rule.RetentionInfo),
                StoragePolicyId = rule.StoragePolicyId,
                StoragePolicyName = rule.StoragePolicyName,
                StubTemplateId = rule.StubTemplateId,
                StubTemplateName = rule.StubTemplateName,
                ExportDataBeforeArchiving = rule.ExportDataBeforeArchiving,
                MoveToArchiverTierWhenArchiving = rule.MoveToArchiverTierWhenArchiving,
                ArchivedLatestVersion = rule.ArchivedLatestVersion,
                KeepLatestMajorAndMinorVersion = rule.KeepLatestMajorAndMinorVersion,
                ArchiverOnlyLastestVersion = rule.ArchiverOnlyLastestVersion,
                KeepLatestMajorAndMinorVersionAndArchiveOthers = rule.KeepLatestMajorAndMinorVersionAndArchiveOthers,
                MoveToAnotherTierType = rule.MoveToArchiverTierWhenArchiving ? (int)Storage.AccessTierType.Archive : rule.MoveToAnotherTierType,

                //Terms = GetRuleTermsName(terms),
            };
            if ((rule.KeepDataOption & (int)KeepDataStatus.Archive) == (int)KeepDataStatus.Archive || (rule.KeepDataOption & (int)KeepDataStatus.ArchiveAndLeaveStub) == (int)KeepDataStatus.ArchiveAndLeaveStub)
            {
                newRule.IsEnableRetention = rule.IsEnableRetention;
                newRule.RetentionInfo = await this.ConvertRetentionInfoAsync(rule.RetentionInfo);
            }
            else
            {
                newRule.IsEnableRetention = rule.IsEnableStoreContentRetention;
                newRule.RetentionInfoList = this.ConvertStoreContentRetentionInfo(rule.StoreContentRetentionInfos);
                newRule.RetentionInfo = this.ConvertStoreContentRetentionInfo(rule.StoreContentRetentionInfos)?.FirstOrDefault();
            }


            if (rule.ProfileType == GCommon.Contract.Server.Common.Profile.Object.ProfileType.ArchiverRule)
            {
                newRule.ModelType = AvePoint.GCommon.Contract.StorageOptimization.Object.RuleModel.SOArchiver;
            }
            else
            {
                newRule.ModelType = AvePoint.GCommon.Contract.StorageOptimization.Object.RuleModel.Records;
            }
            if (rule.MoveToRecordCenterAndDelareSetting != null
                && rule.MoveToRecordCenterAndDelareSetting.DestinationLocation != null
                && !string.IsNullOrEmpty(rule.MoveToRecordCenterAndDelareSetting.DestinationLocation.Password))
            {
                newRule.MoveDto = new MoveToDto();
                newRule.MoveDto.IsSpecifyLocation = true;
                newRule.MoveDto.LocationPath = rule.MoveToRecordCenterAndDelareSetting.DestinationLocation.Url;
                newRule.MoveDto.NotDeclareMovedData = rule.MoveToRecordCenterAndDelareSetting.DelaredRecord;
                newRule.MoveDto.FileNameConflictOption = FileNameConflictOption.Skip;
            }
            if (rule.ExportInfo != null && !rule.ExportType.Equals(AvePoint.GCommon.Contract.StorageOptimization.Object.ExportTypeValue.None))
            {
                newRule.EnableExport = true;
            }
            if ((rule.KeepDataOption & 128) == (int)KeepDataStatus.LinkToDocument || (rule.KeepDataOption & 2048) == (int)KeepDataStatus.ArchiveAndLeaveStub)
            {
                newRule.LeaveStubMessage = !string.IsNullOrEmpty(rule.LeaveStubMessage) ? HttpUtility.HtmlDecode(rule.LeaveStubMessage) : I18NEntity.GetString("RM_RDM_CreateRule_LeaveStubOptionMessage_Default");
            }
            InitRMRuleManualApprovalInfo(rule, newRule);
            return newRule;
        }

        private async Task<RMRuleInfos> ConvertToGoogleDriveRuleInfoAsync(Rule rule, bool isControlPlus = false)
        {
            await ConvertRuleFiltersTimeZoneAsync(rule);
            List<SOFilterPolicy> filters = rule.SOFilters;
            List<RuleFilter> displayFilters = new List<RuleFilter>();
            List<string> ruleCriteria = new List<string>();
            List<PolicyLevel> filterLevels = new List<PolicyLevel>();
            foreach (var filter in filters)
            {
                ArchiverRuleFilter arFilter = new ArchiverRuleFilter(filter);
                RuleFilter newFilter = new RuleFilter()
                {
                    CombineMode = arFilter.CombineMode,
                    Condition = arFilter.Condition,
                    RuleType = arFilter.RuleType,
                    filterName = arFilter.RuleName,
                    Level = arFilter.Level,
                    SequenceNo = arFilter.SequenceNo,
                    Value1 = arFilter.Value1,
                    Value1Unit = arFilter.Value1Unit,
                    Value2 = arFilter.Value2,
                    Value2Unit = arFilter.Value2Unit,
                    Value3 = arFilter.Value3,
                    Value3Unit = arFilter.Value3Unit,
                    FilterCretia = arFilter.FilterCretia(isControlPlus),
                };

                if (newFilter.Condition == ArchiverFilterCondition.FromTo)
                {
                    if (isControlPlus)
                    {
                        var g1TimeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById(TenantLocalValue.TimezoneId);
                        var gls = await mGeneralSettingService.GetGeneralSettingAsync();
                        if (newFilter.RuleType == ArchiverFilterRuleType.DateTimeLabelProperty)
                        {
                            newFilter.Value2 = DateTimeUtil.GetFormattedTimeFromUtc(newFilter.Value2, g1TimeZoneInfo.Id);
                            newFilter.Value3 = DateTimeUtil.GetFormattedTimeFromUtc(newFilter.Value3, g1TimeZoneInfo.Id);
                        }
                        else
                        {
                            newFilter.Value1 = DateTimeUtil.GetFormattedTimeFromUtc(newFilter.Value1, g1TimeZoneInfo.Id);
                            newFilter.Value2 = DateTimeUtil.GetFormattedTimeFromUtc(newFilter.Value2, g1TimeZoneInfo.Id);
                        }
                    }
                    newFilter.StartTimeInfo = arFilter.GetFilterDateTimeInfo(true);
                    newFilter.EndTimeInfo = arFilter.GetFilterDateTimeInfo(false);
                }
                else if (newFilter.Condition == ArchiverFilterCondition.Before)
                {
                    if (isControlPlus)
                    {
                        var g1TimeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById(TenantLocalValue.TimezoneId);
                        var gls = await mGeneralSettingService.GetGeneralSettingAsync();
                        if (newFilter.RuleType == ArchiverFilterRuleType.DateTimeLabelProperty)
                        {
                            newFilter.Value2 = DateTimeUtil.GetFormattedTimeFromUtc(newFilter.Value2, g1TimeZoneInfo.Id);
                        }
                        else
                        {
                            newFilter.Value1 = DateTimeUtil.GetFormattedTimeFromUtc(newFilter.Value1, g1TimeZoneInfo.Id);
                        }
                    }
                    newFilter.StartTimeInfo = arFilter.GetFilterDateTimeInfo(true);
                }
                displayFilters.Add(newFilter);
                ruleCriteria.Add(arFilter.FilterCretia());
                if (!filterLevels.Contains(arFilter.Level))
                {
                    filterLevels.Add(arFilter.Level);
                }
            }
            RMRuleInfos newRule = new RMRuleInfos()
            {
                RuleKeepDataOption = rule.KeepDataOption,
                RelatedRecordOption = rule.RelatedRecordOption,
                ArchiverActions = GetArchiverRuleSettingType(rule),
                EnableManualApproval = rule.IsManualApproval,
                isChecked = false,
                RuleFilters = displayFilters,
                ExportFormat = rule.ExportType.ToString(),
                RuleCretias = ruleCriteria,
                RuleLevel = rule.PolicyLevel,
                ExportInfo = rule.ExportInfo,
                StoragePolicyId = rule.StoragePolicyId,
                StoragePolicyName = rule.StoragePolicyName,
                ExportDataBeforeArchiving = rule.ExportDataBeforeArchiving,
                MoveToArchiverTierWhenArchiving = rule.MoveToArchiverTierWhenArchiving,
                KeepLatestMajorAndMinorVersionAndArchiveOthers = rule.KeepLatestMajorAndMinorVersionAndArchiveOthers,
                KeepLatestMajorAndMinorVersion = rule.KeepLatestMajorAndMinorVersion,
                ArchiverOnlyLastestVersion = rule.ArchiverOnlyLastestVersion,
                ArchivedLatestVersion = rule.ArchivedLatestVersion,
                MoveToAnotherTierType = rule.MoveToArchiverTierWhenArchiving ? (int)Storage.AccessTierType.Archive : rule.MoveToAnotherTierType,
            };
            /*if ((rule.KeepDataOption & (int)KeepDataStatus.Archive) == (int)KeepDataStatus.Archive || (rule.KeepDataOption & (int)KeepDataStatus.ArchiveAndLeaveStub) == (int)KeepDataStatus.ArchiveAndLeaveStub)
            {
                newRule.IsEnableRetention = rule.IsEnableRetention;
                newRule.RetentionInfo = await this.ConvertRetentionInfoAsync(rule.RetentionInfo);
            }
            else*/
            {
                newRule.IsEnableRetention = rule.IsEnableStoreContentRetention;
                newRule.RetentionInfoList = this.ConvertStoreContentRetentionInfo(rule.StoreContentRetentionInfos);
                newRule.RetentionInfo = this.ConvertStoreContentRetentionInfo(rule.StoreContentRetentionInfos)?.FirstOrDefault();
            }
            if (rule.ProfileType == GCommon.Contract.Server.Common.Profile.Object.ProfileType.ArchiverRule)
            {
                newRule.ModelType = AvePoint.GCommon.Contract.StorageOptimization.Object.RuleModel.SOArchiver;
            }
            else
            {
                newRule.ModelType = AvePoint.GCommon.Contract.StorageOptimization.Object.RuleModel.Records;
            }
            StringBuilder filterCombineModeString = new StringBuilder();
            foreach (var filterLevel in filterLevels)
            {
                filterCombineModeString.Append(GetAndOrExpression(rule.AndOrExpression[filterLevel]));
            }
            newRule.FilterCombineMode = filterCombineModeString.ToString().Replace("And", I18NEntity.GetString("RM_JS_Rule_ConditionAnd")).Replace("Or", I18NEntity.GetString("RM_JS_Rule_ConditionOr"));

            if (rule.ExportInfo != null && !rule.ExportType.Equals(AvePoint.GCommon.Contract.StorageOptimization.Object.ExportTypeValue.None))
            {
                newRule.EnableExport = true;
            }
            InitRMRuleManualApprovalInfo(rule, newRule);
            return newRule;
        }
        private async System.Threading.Tasks.Task ConvertRuleFiltersTimeZoneAsync(Rule rule)
        {
            if (rule == null)
            {
                return;
            }
            var gls = await mGeneralSettingService.GetGeneralSettingAsync();
            if (rule.SOFilters != null)
            {
                foreach (var f in rule.SOFilters)
                {
                    try
                    {
                        logger.Info($"Convert rule filters TimeZone. Rule Name:{rule.Name}");
                        if (f.Level == PolicyLevel.GoogleDriveDocument && f.Value.Value3 != null)
                        {
                            RuleUtil.ModifyDisplayDateTimeByPolicyValue(f.BeginTime, f.Value.Value2, gls);
                            RuleUtil.ModifyDisplayDateTimeByPolicyValue(f.EndTime, f.Value.Value3, gls);
                        }
                        else
                        {
                            
                            if (DateTime.TryParseExact(f.Value.Value1, AveDateTimeUtility.DATETYPEForAPI003, 
                                CultureInfo.InvariantCulture, DateTimeStyles.None, out _))
                            {
                                RuleUtil.ModifyDisplayDateTimeByPolicyValue(f.BeginTime, f.Value.Value1, gls);
                            }
                            else
                            {
                                RuleUtil.ModifyDisplayDateTimeByPolicyValue(f.BeginTime, f.Value.Value2, gls);
                            }
                            RuleUtil.ModifyDisplayDateTimeByPolicyValue(f.EndTime, f.Value.Value2, gls);
                        }
                    }
                    catch (Exception e)
                    {
                        logger.Warn($"Convert rule filters TimeZone error. Rule Name:{rule.Name}, Error Message:{e}");
                    }
                }
            }
        }

        private async Task<RetentionSettings> ConvertRetentionInfoAsync(RetentionInfo info)
        {
            if (info == null)
            {
                return null;
            }
            RetentionSettings retentionSetting = new RetentionSettings();
            retentionSetting.IsManualApproval = info.IsManualApproval;
            retentionSetting.ColumnName = info.ColumnName;
            retentionSetting.Condition = info.Condition;
            retentionSetting.KeepDateNumber = info.KeepDateNumber;
            retentionSetting.KeepDateUnite = info.KeepDateUnite;
            retentionSetting.Date = await ConvertTime4GUIAsync(info.Date);
            retentionSetting.ReviewType = info.ReviewType;
            retentionSetting.WorkflowId = info.WorkflowId;
            retentionSetting.IsSendEamilToOwner = info.IsSendEamilToOwner;
            retentionSetting.UserInfos = info.UserInfos;
            return retentionSetting;
        }

        private List<RetentionSettings> ConvertStoreContentRetentionInfo(List<RetentionRule> infos)
        {
            List<RetentionSettings> resultRetentionSetting = new List<RetentionSettings>();
            if (infos == null || infos.Count == 0)
            {
                return null;
            }
            foreach (var tempInfo in infos)
            {

                RetentionSettings retentionSetting = new RetentionSettings();
                retentionSetting.IsManualApproval = false;
                retentionSetting.ColumnName = "Archived Time";
                retentionSetting.Condition = TimeFilterCondition.OlderThan;
                retentionSetting.KeepDateNumber = tempInfo.KeepValue;
                retentionSetting.IsEnableRetention = tempInfo.SetupDataRetention;
                retentionSetting.KeepDateUnite = tempInfo.ArchiveDateUnit switch
                {
                    DateUnit.Day => TimeUnit.Day,
                    DateUnit.Week => TimeUnit.Week,
                    DateUnit.Month => TimeUnit.Month,
                    DateUnit.Year => TimeUnit.Year,
                    _ => throw new NotImplementedException(),
                };
                //load rule
                retentionSetting.RemoveOrphanedStub = !tempInfo.KeepOrphanedStub4CompatibilityExistingRule;
                retentionSetting.OperateDataType = tempInfo.DeleteTheData ? (int)OperateDateTypeEnum.Delete : tempInfo.IsMarkDataTier ? (int)OperateDateTypeEnum.MarkTier : (int)OperateDateTypeEnum.None;
                retentionSetting.TierType = tempInfo.TierType;
                retentionSetting.RetentionDataTimeType = tempInfo.RetentionDataTimeType == KeepDateType.None? KeepDateType.ArchiveTime: tempInfo.RetentionDataTimeType;
                retentionSetting.SoftKeepDateNumber = tempInfo.SoftDeleteKeepValue;
                retentionSetting.IsSoftDelete = tempInfo.IsSoftDelete;
                retentionSetting.SoftKeepDateUnite = tempInfo.SoftDeleteDateUnit switch
                {
                    DateUnit.Day => TimeUnit.Day,
                    DateUnit.Week => TimeUnit.Week,
                    DateUnit.Month => TimeUnit.Month,
                    DateUnit.Year => TimeUnit.Year,
                    _ => throw new NotImplementedException(),
                };
                resultRetentionSetting.Add(retentionSetting);
            }
            return resultRetentionSetting;
        }

        private async Task<string> ConvertTime4GUIAsync(long backendDate)
        {
            if (backendDate > 0)
            {
                var gls = await mGeneralSettingService.GetGeneralSettingAsync();
                return DateTimeUtil.ConvertTimeFromUtc(backendDate, gls.TimeZoneId, gls.DayLight).ToString("yyyy/M/d H:m");
            }
            return "";
        }
        private async Task<long> ConvertTime4BackendAsync(string guiDateStr)
        {
            DateTime guiDate;
            if (DateTime.TryParseExact(guiDateStr, "yyyy/M/d H:m", new System.Globalization.CultureInfo(1033), System.Globalization.DateTimeStyles.None, out guiDate))
            {
                if (guiDate != DateTime.MinValue)
                {
                    var gls = await mGeneralSettingService.GetGeneralSettingAsync();
                    return DateTimeUtil.ConvertTimeToUtc(guiDate, gls.TimeZoneId, gls.DayLight);
                }
            }
            logger.Warn("Error parse time string {0}", guiDateStr);
            return 0L;
        }
        private async Task<RMTagContentInfo> ConvertTagContentInfoAsync(TagContentInfo t)
        {
            if (t.Type == TagContentInfoType.DateTime)
            {
                var timeZoneId = string.Empty;
                bool isDayLightSaving = false;
                var splitsVals = t.Value.Split('/');
                if (splitsVals.Length == 2)
                {
                    timeZoneId = splitsVals[0];
                    isDayLightSaving = bool.Parse(splitsVals[1]);
                }
                else
                {
                    var gls = await mGeneralSettingService.GetGeneralSettingAsync();
                    timeZoneId = gls.TimeZoneId;
                    isDayLightSaving = gls.DayLight;
                }
                return new RMTagContentInfo()
                {
                    ColumnName = t.ColumnName,
                    DateTime = t.DateTime,
                    Type = t.Type,
                    TimeZoneId = timeZoneId,
                    IsDayLightSaving = isDayLightSaving,
                    Value = DateTimeUtil.ConvertTimeFromUtc(t.DateTime.Ticks, GeneralSettingConfig.FindSystemTimeZoneById(timeZoneId), !isDayLightSaving).ToString(JSDateTimeFormat.DEFAULT_TIME_FORMAT, CultureInfo.InvariantCulture),
                    Option = t.Option,
                };
            }
            else
            {
                return new RMTagContentInfo()
                {
                    ColumnName = t.ColumnName,
                    DateTime = t.DateTime,
                    Type = t.Type,
                    TimeZoneId = "",
                    IsDayLightSaving = false,
                    Value = t.Value,
                    Option = t.Option
                };
            }
        }

        //to do next 
        private string GetArchiverRuleSettingType(Rule rule, SOSourceFlag sourceFlag = SOSourceFlag.None)
        {
            var retentionLables = I18NEntity.GetString("RM_RDM_CreateRule_Options_IncludeRetentionLabels");
            string strArchiverActions = "";
            int keepDataOption = rule.KeepDataOption;
            var isSupportRecordLabel = AccountUtility.IsSupportRecordLabel();
            if (rule.PolicyLevel == PolicyLevel.ExchangeOnlineItem)
            {
                if (rule.ExportInfo != null && rule.ExportInfo.exportSPDataOption == ExportSPDataOption.ExportWithoutArchive)
                {
                    strArchiverActions = I18NEntity.GetString("RM_JS_RDM_CreateRule_Options_ExportOnly");
                }
                else if ((keepDataOption & (int)KeepDataStatus.Keep) == (int)KeepDataStatus.Keep)
                {
                    //strArchiverActions = I18NEntity.GetString("RM_JS_RDM_CreateRule_Options_ExchangeArchiveAndKeep");
                    strArchiverActions = I18NEntity.GetString("RM_JS_RDM_CreateRule_Options_ArchiveAndKeep");
                }
                //else if((keepDataOption & (int)KeepDataStatus.Remove) == (int)KeepDataStatus.Remove)
                else if (RuleAuditUtil.ExcludeOptionUnderMoveAction(keepDataOption) == (int)KeepDataStatus.Delete && rule.spMoveOption != null && rule.spMoveOption.MoveDestination != null)
                {
                    strArchiverActions = I18NEntity.GetString("RM_JS_RDM_CreateRule_Options_MoveRecord");
                    if (rule.spMoveOption.MoveDestination.DeleteSourceItem)
                    {
                        strArchiverActions = string.Format("{0};{1}", strArchiverActions, I18NEntity.GetString("RM_JS_BCM_Rule_Move_IsRemoveEmail"));
                    }
                    if (rule.spMoveOption.MoveDestination.KeepSourceClassification)
                    {
                        strArchiverActions = string.Format("{0};{1}", strArchiverActions, I18NEntity.GetString("RM_JS_BCM_Rule_Move_IsReclassify"));
                    }
                    if (rule.spMoveOption.IsMoveToSP)
                    {
                        strArchiverActions = string.Format("{0};{1}", strArchiverActions, I18NEntity.GetString("RM_JS_BCM_Explorer_ExoMoveToSP_CheckboxTitle"));
                    }
                }
                else
                {
                    strArchiverActions = I18NEntity.GetString("RM_JS_RDM_CreateRule_Options_ExchangeArchiveAndRemove");
                }
            }
            else if (rule.PolicyLevel == PolicyLevel.PhysicalBox || rule.PolicyLevel == PolicyLevel.PhysicalFile)
            {
                if (rule.IsCalculationDisposalDate)
                {
                    strArchiverActions = I18NEntity.GetString("RM_JS_RDM_CreateRule_Options_CalculateDisposalDate");
                }
                else if (keepDataOption == (int)KeepDataStatus.Delete && rule.spMoveOption != null && rule.spMoveOption.MoveDestination != null)
                {
                    strArchiverActions = I18NEntity.GetString("RM_JS_RDM_CreateRule_Options_MoveLocation");
                }
                else
                {
                    strArchiverActions = I18NEntity.GetString("RM_JS_RDM_CreateRule_Options_ArchiveAndRemove");
                    if (rule.RelatedRecordOption == AvePoint.GCommon.Contract.StorageOptimization.Object.RelatedRecordOption.Both)
                    {
                        strArchiverActions = string.Format("{0}; {1}", strArchiverActions, I18NEntity.GetString("RM_RDM_CreateRule_DeleteRelatedRecord"));
                    }
                    if (rule.IsDeleteParentBox)
                    {
                        strArchiverActions = string.Format("{0}; {1}", strArchiverActions, I18NEntity.GetString("RM_RDM_CreateRule_DestroyEmptyBox"));
                    }
                }
            }
            else if (rule.PolicyLevel == PolicyLevel.FileSysFile)
            {
                if (keepDataOption == (int)KeepDataStatus.Delete && rule.spMoveOption != null && rule.spMoveOption.MoveDestination != null)
                {
                    strArchiverActions = I18NEntity.GetString("RM_JS_RDM_CreateRule_Options_MoveRecord_FS");
                }
                else if ((keepDataOption & (int)KeepDataStatus.Archive) == (int)KeepDataStatus.Archive)
                {
                    strArchiverActions = I18NEntity.GetString("RM_RDM_CreateRule_ArchiveToAzureBlobStorage");
                }
                else
                {
                    strArchiverActions = I18NEntity.GetString("RM_JS_RDM_CreateRule_Options_ArchiveAndRemove");
                    if ((keepDataOption & 128) == (int)KeepDataStatus.LinkToDocument)
                    {
                        strArchiverActions = string.Format("{0}; {1}", strArchiverActions, I18NEntity.GetString("RM_JS_RDM_CreateRule_Options_LeaveStub_FS"));
                    }
                }
            }
            else if (rule.PolicyLevel == PolicyLevel.AzureFileDocument)
            {
                if (keepDataOption == (int)KeepDataStatus.Delete && rule.spMoveOption != null && rule.spMoveOption.MoveDestination != null)
                {
                    strArchiverActions = I18NEntity.GetString("RM_JS_RDM_CreateRule_Options_MoveRecord_FS");
                }
                else
                {
                    strArchiverActions = I18NEntity.GetString("RM_JS_RDM_CreateRule_Options_ArchiveAndRemove");
                    if ((keepDataOption & 128) == (int)KeepDataStatus.LinkToDocument)
                    {
                        strArchiverActions = string.Format("{0}; {1}", strArchiverActions, I18NEntity.GetString("RM_JS_RDM_CreateRule_Options_LeaveStub_FS"));
                    }
                }
            } 
            else if (rule.PolicyLevel == PolicyLevel.BoxDocument)
            {
                if (keepDataOption == (int)KeepDataStatus.Delete && rule.spMoveOption != null && rule.spMoveOption.MoveDestination != null)
                {
                    strArchiverActions = I18NEntity.GetString("RM_JS_RDM_CreateRule_Options_MoveRecord_FS");
                }
                else
                {
                    strArchiverActions = I18NEntity.GetString("RM_JS_RDM_CreateRule_Options_ArchiveAndRemove");
                    if ((keepDataOption & 128) == (int)KeepDataStatus.LinkToDocument)
                    {
                        strArchiverActions = string.Format("{0}; {1}", strArchiverActions, I18NEntity.GetString("RM_JS_RDM_CreateRule_Options_LeaveStub_FS"));
                    }
                }
            }
            else if (rule.PolicyLevel == PolicyLevel.GoogleDriveDocument)
            {         
                if (keepDataOption == (int)KeepDataStatus.Delete && rule.spMoveOption != null && rule.spMoveOption.MoveDestination != null)
                {
                    strArchiverActions = I18NEntity.GetString("RM_JS_RDM_CreateRule_Options_MoveRecord_FS");
                }
                else if ((keepDataOption & (int)KeepDataStatus.Archive) == (int)KeepDataStatus.Archive)
                {
                    strArchiverActions = I18NEntity.GetString("RM_RDM_CreateRule_ArchiveToAzureBlobStorage");
                }
                else if (rule.ExportInfo != null && rule.ExportInfo.exportSPDataOption == ExportSPDataOption.ExportWithoutArchive)
                {
                    strArchiverActions = I18NEntity.GetString("RM_JS_RDM_CreateRule_Options_ExportOnly");
                }
                else
                {
                    strArchiverActions = I18NEntity.GetString("RM_JS_RDM_CreateRule_Options_ArchiveAndRemove");
                }
            }
            else
            {
                if ((keepDataOption & (int)KeepDataStatus.ArchiverOnly) == (int)KeepDataStatus.ArchiverOnly)
                {
                    strArchiverActions = I18NEntity.GetString("RM_JS_RDM_CreateRule_Options_Backup");
                    if ((keepDataOption & (int)KeepDataStatus.ArchiveOnlyLastestVersion) == (int)KeepDataStatus.ArchiveOnlyLastestVersion)
                    {
                        strArchiverActions = string.Format("{0}; {1}", strArchiverActions, I18NEntity.GetString("RM_JS_Rule_ArchiveVersionAndDestroyFile"));
                    }
                }
                else if (rule.ExportInfo != null && rule.ExportInfo.exportSPDataOption == ExportSPDataOption.ExportWithoutArchive)
                {
                    strArchiverActions = I18NEntity.GetString("RM_JS_RDM_CreateRule_Options_ExportOnly");
                }
                else if ((keepDataOption & (int)KeepDataStatus.Delete) != (int)KeepDataStatus.Delete
                    && (keepDataOption & (int)KeepDataStatus.Remove) != (int)KeepDataStatus.Remove
                    && (keepDataOption & 128) != (int)KeepDataStatus.LinkToDocument
                    && (keepDataOption & 256) != (int)KeepDataStatus.NotBackup
                    && (keepDataOption & (int)KeepDataStatus.Vault) != (int)KeepDataStatus.Vault
                    && (keepDataOption & (int)KeepDataStatus.Archive) != (int)KeepDataStatus.Archive
                    && (keepDataOption & (int)KeepDataStatus.ArchiveAndLeaveStub) != (int)KeepDataStatus.ArchiveAndLeaveStub
                    && (keepDataOption & (int)KeepDataStatus.ArchiveBackupAndRemove) != (int)KeepDataStatus.ArchiveBackupAndRemove
                    && (keepDataOption & (int)KeepDataStatus.ArchiveBackupAndRemoveLeaveStub) != (int)KeepDataStatus.ArchiveBackupAndRemoveLeaveStub
                    && (keepDataOption & (int)KeepDataStatus.TriggerMicrosoft365Archiving) != (int)KeepDataStatus.TriggerMicrosoft365Archiving)
                {
                    if (sourceFlag == SOSourceFlag.OneDrive)
                    {
                        strArchiverActions = isSupportRecordLabel ? I18NEntity.GetString("RM_JS_RDM_CreateRule_Options_TagOrLock") : I18NEntity.GetString("RM_JS_RDM_CreateRule_Options_ArchiveAndKeep_TagContent");
                    }
                    else
                    {
                        strArchiverActions = sourceFlag == SOSourceFlag.SharePoint && isSupportRecordLabel ? I18NEntity.GetString("RM_JS_RDM_CreateRule_Options_TagOrLock") : I18NEntity.GetString("RM_JS_RDM_CreateRule_Options_ArchiveAndKeep");
                    }
                }
                else if (RuleAuditUtil.ExcludeOptionUnderMoveAction(keepDataOption) == (int)KeepDataStatus.Delete && rule.spMoveOption != null && rule.spMoveOption.MoveDestination != null)
                {
                    strArchiverActions = I18NEntity.GetString("RM_JS_RDM_CreateRule_Options_MoveRecord");
                    if (!rule.spMoveOption.MoveDestination.NotDeclareMovedData)
                    {
                        if((sourceFlag == SOSourceFlag.SharePoint || sourceFlag == SOSourceFlag.OneDrive) && isSupportRecordLabel)
                        {
                            strArchiverActions = string.Format("{0}; {1}", strArchiverActions, I18NEntity.GetString("RM_JS_RDM_CreateRule_Options_Move_LockByRecordsLabel"));
                        }
                        else
                        {
                            strArchiverActions = string.Format("{0}; {1}", strArchiverActions, I18NEntity.GetString("RM_JS_RDM_CreateRule_Options_Move_DeclareRecord"));
                        }
                    }

                    if (rule.spMoveOption.MoveDestination.KeepSourceClassification)
                    {
                        strArchiverActions = string.Format("{0};{1}", strArchiverActions, I18NEntity.GetString("RM_JS_BCM_Rule_Move_IsReclassify"));
                    }
                    if (rule.spMoveOption.MoveDestination.KeepFolderStructure)
                    {
                        strArchiverActions = string.Format("{0};{1}", strArchiverActions, I18NEntity.GetString("RM_JS_RDM_CreateRule_Options_Move_FolderStructure"));
                    }
                    if (rule.spMoveOption.MoveDestination.IsMoveVersions)
                    {
                        strArchiverActions = string.Format("{0};{1}", strArchiverActions, I18NEntity.GetString("RM_JS_RDM_CreateRule_Options_Move_AllVersions"));
                    }
                    if ((keepDataOption & (int)KeepDataStatus.IsEnableRemoveRetentionLabel) == (int)KeepDataStatus.IsEnableRemoveRetentionLabel)
                    {
                        strArchiverActions = string.Format("{0}; {1}", strArchiverActions, retentionLables);
                    }
                }
                else if ((keepDataOption & (int)KeepDataStatus.Archive) == (int)KeepDataStatus.Archive || (keepDataOption & (int)KeepDataStatus.ArchiveAndLeaveStub) == (int)KeepDataStatus.ArchiveAndLeaveStub)
                {
                    strArchiverActions = I18NEntity.GetString("RM_RDM_CreateRule_ArchiveToAzureBlobStorage");
                    if ((keepDataOption & 2048) == (int)KeepDataStatus.ArchiveAndLeaveStub)
                    {
                        strArchiverActions = string.Format("{0}; {1}", strArchiverActions, I18NEntity.GetString("RM_RDM_CreateRule_LeaveStubOption"));
                        if (rule.DeclareLinkFile)
                        {
                            strArchiverActions = string.Format("{0}; {1}", strArchiverActions, I18NEntity.GetString("RM_JS_RDM_CreateRule_Options_DeclareStub"));
                        }
                    }
                    if ((keepDataOption & (int)KeepDataStatus.IsEnableRemoveRetentionLabel) == (int)KeepDataStatus.IsEnableRemoveRetentionLabel)
                    {
                        strArchiverActions = string.Format("{0}; {1}", strArchiverActions, retentionLables);
                    }
                    if (rule.DeleteRecords)
                    {
                        strArchiverActions = string.Format("{0}; {1}", strArchiverActions, I18NEntity.GetString("RM_RDM_CreateRule_Options_IncludeDeclaredFile"));
                    }
                    if (isSupportRecordLabel && rule.IncludeDeleteRecordLabel)
                    {
                        strArchiverActions = string.Format("{0}; {1}", strArchiverActions, I18NEntity.GetString("RM_RDM_CreateRule_RecordsLabelOption"));
                        if (rule.LockRecordBeforeDestroy)
                        {
                            strArchiverActions = string.Format("{0}; {1}", strArchiverActions, I18NEntity.GetString("RM_RDM_CreateRule_LockRecordBeforeDestroy"));
                        }
                    }
                }
                else if ((keepDataOption & (int)KeepDataStatus.ArchiveBackupAndRemove) == (int)KeepDataStatus.ArchiveBackupAndRemove || (keepDataOption & (int)KeepDataStatus.ArchiveBackupAndRemoveLeaveStub) == (int)KeepDataStatus.ArchiveBackupAndRemoveLeaveStub)
                {
                    strArchiverActions = I18NEntity.GetString("RM_JS_RDM_CreateRule_Options_BackupAndRemove");
                    if ((keepDataOption & (int)KeepDataStatus.ArchiveBackupAndRemoveLeaveStub) == (int)KeepDataStatus.ArchiveBackupAndRemoveLeaveStub)
                    {
                        strArchiverActions = string.Format("{0}; {1}", strArchiverActions, I18NEntity.GetString("RM_RDM_CreateRule_LeaveStubOption"));
                    }
                    if ((keepDataOption & (int)KeepDataStatus.ArchiveLatestVersion) == (int)KeepDataStatus.ArchiveLatestVersion)
                    {
                        strArchiverActions = string.Format("{0}; {1} {2}", strArchiverActions, I18NEntity.GetString("RM_JS_Audit_ArchiveVersionAndDestroyFile"), rule.ArchivedLatestVersion);
                    }
                    if ((keepDataOption & (int)KeepDataStatus.KeepLatestVersionAndArhiveOthers) == (int)KeepDataStatus.KeepLatestVersionAndArhiveOthers)
                    {
                        strArchiverActions = string.Format("{0}; {1} {2}", strArchiverActions, I18NEntity.GetString("RM_JS_Audit_KeepVersionAndArchiveOther"), rule.KeepLatestMajorAndMinorVersionAndArchiveOthers);
                    }

                    if (rule.DeleteRecords)
                    {
                        strArchiverActions = string.Format("{0}; {1}", strArchiverActions, I18NEntity.GetString("RM_RDM_CreateRule_Options_IncludeDeclaredFile"));
                    }
                    if ((keepDataOption & (int)KeepDataStatus.IsEnableRemoveRetentionLabel) == (int)KeepDataStatus.IsEnableRemoveRetentionLabel)
                    {
                        strArchiverActions = string.Format("{0}; {1}", strArchiverActions, retentionLables);
                    }
                    if (isSupportRecordLabel && rule.IncludeDeleteRecordLabel)
                    {
                        strArchiverActions = string.Format("{0}; {1}", strArchiverActions, I18NEntity.GetString("RM_RDM_CreateRule_RecordsLabelOption"));
                        if (rule.LockRecordBeforeDestroy)
                        {
                            strArchiverActions = string.Format("{0}; {1}", strArchiverActions, I18NEntity.GetString("RM_RDM_CreateRule_LockRecordBeforeDestroy"));
                        }
                    }
                    if (rule.IsDeleteSiteCollectionToRecycleBin((int)sourceFlag))
                    {
                        strArchiverActions = string.Format("{0}; {1}", strArchiverActions, I18NEntity.GetString("RM_RDM_CreateRule_Options_DeleteSiteCollectionToRecycleBin"));
                    }
                }
                else if ((keepDataOption & (int)KeepDataStatus.TriggerMicrosoft365Archiving) == (int)KeepDataStatus.TriggerMicrosoft365Archiving)
                {
                    strArchiverActions = I18NEntity.GetString("RM_JS_RDM_CreateRule_Options_StoreInM365Archive");
                }
                else if (rule.TagContentInfo != null && rule.TagContentInfo.Any())
                {

                    if (sourceFlag == SOSourceFlag.OneDrive)
                    {
                        strArchiverActions = isSupportRecordLabel ? I18NEntity.GetString("RM_JS_RDM_CreateRule_Options_TagOrLock") : I18NEntity.GetString("RM_JS_RDM_CreateRule_Options_ArchiveAndKeep_TagContent");
                    }
                    else
                    {
                        strArchiverActions = sourceFlag == SOSourceFlag.SharePoint && isSupportRecordLabel ? I18NEntity.GetString("RM_JS_RDM_CreateRule_Options_TagOrLock") : I18NEntity.GetString("RM_JS_RDM_CreateRule_Options_ArchiveAndKeep");
                    }
                    if (keepDataOption == 21)
                    {
                        strArchiverActions = string.Format("{0}; {1}; {2}", strArchiverActions, I18NEntity.GetString("RM_RDM_CreateRule_Options_DeclareDocumnet"), I18NEntity.GetString("RM_TM_Excel_DoTag"));
                    }
                    else
                    {
                        string strTagContent = "";
                        if (rule.PolicyLevel == PolicyLevel.Folder)
                        {
                            strTagContent = "RM_TM_Excel_Folder_DoTag";
                        }
                        else
                        {
                            strTagContent = "RM_TM_Excel_DoTag";
                        }
                        strArchiverActions = string.Format("{0}; {1}", strArchiverActions, I18NEntity.GetString(strTagContent));
                    }
                }
                else if (keepDataOption == 20)
                {
                    if (sourceFlag == SOSourceFlag.OneDrive)
                    {
                        strArchiverActions = string.Format("{0}; {1}", isSupportRecordLabel ? I18NEntity.GetString("RM_JS_RDM_CreateRule_Options_TagOrLock") : I18NEntity.GetString("RM_JS_RDM_CreateRule_Options_ArchiveAndKeep_TagContent"), I18NEntity.GetString("RM_RDM_CreateRule_Options_DeclareDocumnet"));                    
                    }
                    else
                    {
                        strArchiverActions = string.Format("{0}; {1}", sourceFlag == SOSourceFlag.SharePoint && isSupportRecordLabel ? I18NEntity.GetString("RM_JS_RDM_CreateRule_Options_TagOrLock") : I18NEntity.GetString("RM_JS_RDM_CreateRule_Options_ArchiveAndKeep"), I18NEntity.GetString("RM_RDM_CreateRule_Options_DeclareDocumnet"));                    
                    }
                }
                else
                {
                    strArchiverActions = I18NEntity.GetString("RM_JS_RDM_CreateRule_Options_ArchiveAndRemove");
                    if ((keepDataOption & 256) != (int)KeepDataStatus.NotBackup)
                    {
                        if ((keepDataOption & (int)KeepDataStatus.KeepLatestVersion) == (int)KeepDataStatus.KeepLatestVersion)
                        {
                            strArchiverActions = string.Format("{0}; {1} {2}", strArchiverActions, I18NEntity.GetString("RM_JS_Audit_KeepLatestVersionAndDestroyOther"), rule.KeepLatestMajorAndMinorVersion);
                        }
                        if ((keepDataOption & (int)KeepDataStatus.DeleteOnly) != (int)KeepDataStatus.DeleteOnly)
                        {
                            strArchiverActions = string.Format("{0}; {1}", strArchiverActions, I18NEntity.GetString("RM_RDM_CreateRule_BackupBeforeDestroying"));
                        }
                    }
                    
                    if (rule.RelatedRecordOption == AvePoint.GCommon.Contract.StorageOptimization.Object.RelatedRecordOption.Both)
                    {
                        strArchiverActions = string.Format("{0}; {1}", strArchiverActions, I18NEntity.GetString("RM_RDM_CreateRule_DeleteRelatedRecord"));
                    }
                    if (rule.DeleteRecords)
                    {
                        strArchiverActions = string.Format("{0}; {1}", strArchiverActions, I18NEntity.GetString("RM_RDM_CreateRule_Options_IncludeDeclaredFile"));
                    }
                    if (rule.DeleteToRecycleBin)
                    {
                        strArchiverActions = string.Format("{0}; {1}", strArchiverActions, I18NEntity.GetString("RM_RDM_CreateRule_Options_DeleteToRecycleBin"));
                    }
                    if (isSupportRecordLabel && rule.IncludeDeleteRecordLabel)
                    {
                        strArchiverActions = string.Format("{0}; {1}", strArchiverActions, I18NEntity.GetString("RM_RDM_CreateRule_RecordsLabelOption"));
                        if (rule.LockRecordBeforeDestroy)
                        {
                            strArchiverActions = string.Format("{0}; {1}", strArchiverActions, I18NEntity.GetString("RM_RDM_CreateRule_LockRecordBeforeDestroy"));
                        }
                    }
                    if (rule.IsDeleteSiteCollectionToRecycleBin((int)sourceFlag))
                    {
                        strArchiverActions = string.Format("{0}; {1}", strArchiverActions, I18NEntity.GetString("RM_RDM_CreateRule_Options_DeleteSiteCollectionToRecycleBin"));
                    }
                    if ((keepDataOption & 128) == (int)KeepDataStatus.LinkToDocument)
                    {
                        strArchiverActions = string.Format("{0}; {1}", strArchiverActions, I18NEntity.GetString("RM_RDM_CreateRule_LeaveStubOption"));
                        if (rule.DeclareLinkFile)
                        {
                            strArchiverActions = string.Format("{0}; {1}", strArchiverActions, I18NEntity.GetString("RM_JS_RDM_CreateRule_Options_DeclareStub"));
                        }
                    }
                    if (RuleAuditUtil.ExcludeOptionUnderMoveAction(keepDataOption) == (int)KeepDataStatus.Delete && rule.MoveToRecordCenterAndDelareSetting != null && rule.MoveToRecordCenterAndDelareSetting.DestinationLocation != null)
                    {
                        strArchiverActions = I18NEntity.GetString("RM_JS_RDM_CreateRule_Options_MoveRecord");
                    }
                    if ((keepDataOption & (int)KeepDataStatus.IsEnableRemoveRetentionLabel) == (int)KeepDataStatus.IsEnableRemoveRetentionLabel)
                    {
                        strArchiverActions = string.Format("{0}; {1}", strArchiverActions, retentionLables);
                    }
                    //return I18NEntity.GetString("RM_JS_RDM_CreateRule_Options_ArchiveAndRemove");
                }
            }
            return strArchiverActions;
        }

        public string GetArchiverRuleActionStringForDiscoveryOptimization(Rule rule, bool isSimulation)
        {
            var keepDataOption = rule.KeepDataOption;
            if(rule.PolicyLevel == PolicyLevel.Document)
            {
                if ((keepDataOption & (int)KeepDataOption.ArchiveBackupAndRemove) == (int)KeepDataOption.ArchiveBackupAndRemove
                    || (keepDataOption & (int)KeepDataOption.ArchiveBackupAndRemoveLeaveStub) == (int)KeepDataOption.ArchiveBackupAndRemoveLeaveStub
                    || ((rule.KeepDataOption & (int)(KeepDataOption.ArchiveBackupAndRemove | KeepDataOption.ArchiveLatestVersion)) == (int)(KeepDataOption.ArchiveBackupAndRemove | KeepDataOption.ArchiveLatestVersion)))
                {
                    return "RM_FA_DataOptimize_File_ArchiveAndRemove";
                }
                if ((keepDataOption & (int)KeepDataOption.DeleteOnly) == (int)KeepDataOption.DeleteOnly)
                {
                    return "RM_FA_DataOptimize_File_RemoveFile";
                }
                if (isSimulation)
                {
                    if((keepDataOption & (int)KeepDataOption.ArchiverOnly) == (int)KeepDataOption.ArchiverOnly
                      || (rule.KeepDataOption & (int)(KeepDataOption.ArchiverOnly | KeepDataOption.ArchiveOnlyLastestVersion)) == (int)(KeepDataOption.ArchiverOnly | KeepDataOption.ArchiveOnlyLastestVersion))
                        return "RM_FA_DataOptimize_File_ArchiveFile";
                }
            }
            else if(rule.PolicyLevel == PolicyLevel.DocumentVersion)
            {
                if ((keepDataOption & (int)KeepDataOption.ArchiveBackupAndRemove) == (int)KeepDataOption.ArchiveBackupAndRemove)
                {
                    return "RM_FA_DataOptimize_Version_ArchiveAndRemove";
                }
                if ((keepDataOption & (int)KeepDataOption.DeleteOnly) == (int)KeepDataOption.DeleteOnly)
                {
                    return "RM_FA_DataOptimize_Version_RemoveVersion";
                }
            }
            return string.Empty;
        }

        public string GetArchiverRuleActionString(Rule rule, JobType jobType)
        {
            var keepDataOption = rule.KeepDataOption;
            if ((keepDataOption & (int)KeepDataOption.TriggerMicrosoft365Archiving) == (int)KeepDataOption.TriggerMicrosoft365Archiving)
            {
                return "RM_JS_RDM_CreateRule_Options_StoreInM365Archive";
            }
            if ((keepDataOption & (int)KeepDataOption.ArchiverOnly) == (int)KeepDataOption.ArchiverOnly)
            {
                return "RM_JS_RDM_CreateRule_Options_Backup";
            }
            if ((keepDataOption & (int)KeepDataOption.ArchiveBackupAndRemove) == (int)KeepDataOption.ArchiveBackupAndRemove
                || (keepDataOption & (int)KeepDataOption.DeleteOnly) == (int)KeepDataOption.DeleteOnly)
            {
                return "SO_Action_Delete";
            }
            if ((keepDataOption & (int)KeepDataOption.Keep) == (int)KeepDataOption.Keep || keepDataOption == 17 || keepDataOption == 20 || keepDataOption == 21)
            {
                return AccountUtility.IsSupportRecordLabel() && (jobType == JobType.SOPreScan || jobType == JobType.ArchiverBackup || jobType == JobType.RecordsDisposal || jobType == JobType.OneDriveRecordsDisposal) 
                    ? "RM_JS_RDM_CreateRule_Options_TagOrLock" 
                    : "SO_Action_Keep";
            }
            if ((keepDataOption & (int)KeepDataOption.ArchiveBackupAndRemoveLeaveStub) == (int)KeepDataOption.ArchiveBackupAndRemoveLeaveStub
                || (keepDataOption & (int)KeepDataOption.LeaveOnlyStub) == (int)KeepDataOption.LeaveOnlyStub
                || (keepDataOption & (int)KeepDataOption.ArchiveAndLeaveStub) == (int)KeepDataOption.ArchiveAndLeaveStub)
            {
                return "SO_Action_LevelStub";
            }
            if ((keepDataOption & (int)KeepDataOption.Delete) == (int)KeepDataOption.Delete)
            {
                if ((rule.MoveToRecordCenterAndDelareSetting != null && rule.MoveToRecordCenterAndDelareSetting.DestinationLocation != null) || rule.spMoveOption != null)
                {
                    return "SO_Action_Move";
                }
                return "SO_Action_ExportOnly";
            }
            return string.Empty;
        }

        [RACodeReview("Allen yin")]
        [Audit(Module = AuditModule.RetentionAndDisposalManagement, Category = AuditCategory.RuleManagement, Action = AuditAction.DeleteRule, BeforeHandler = typeof(RuleManagerBeforeAuditHandler), AfterHandler = typeof(RuleManagerAfterAuditHandler))]
        public async Task<RAReturnMessage> DeleteRulesAsync(List<string> ids)
        {
            RAReturnMessage result = new RAReturnMessage() { MessageType = RAMessageType.Successful};
            try
            {
                logger.Info("Batch delete rules from DocAve Control Service.");
                //IMStorageOptimizationService soService = DocAveServiceHelper.CreateServiceClient<IMStorageOptimizationService>();
                //clean up rule tables 
                using (new RA.Common.PerformanceScope(string.Format("manage.rule.delete.total")))
                {
                    //var client = new DAOAPIClientV1();
                    //client.BatchDeleteRules(ids);
                    if (await RealDeleteRuleAsync(ids))
                    {
                        Dictionary<Guid, int> changes = [];

                        using (new RA.Common.PerformanceScope(string.Format("manage.rule.delete.TermRuleAssocition")))
                        {
                            foreach (var ruleId in ids)
                            {
                                var termIds = TermRuleAssocition.GetTermUniqueIdsByRuleId(ruleId);
                                foreach (var termId in termIds)
                                {
                                    changes[termId] = (int)TermChangeType.TermRule;
                                }

                                TermRuleAssocition.DeleteTermRuleInfos(new Guid(ruleId));
                            }
                        }

                        using (new RA.Common.PerformanceScope(string.Format("manage.rule.deleteFromDB")))
                        {
                            var ruleIds = ids.ConvertAll(i => { return new Guid(i); });
                            RMRuleDao.DeleteRule(ruleIds);
                        }

                        using (new RA.Common.PerformanceScope(string.Format("manage.rule.delete.changeclassification")))
                        {
                            if (changes.Count > 0)
                            {
                                ChangeClassificationDao.AddChangeLabelsAndTerms(changes);
                            }
                        }
                    }
                    else
                    {
                        throw new Exception("^Failed to delete rule");
                    }
                }
                return result;
            }
            catch (AveException ae)
            {
                result.MessageType = RAMessageType.Failed;
                logger.Error(ae.Message, ae);
                return result;
            }
            catch (Exception e)
            {
                logger.Error(e.Message, e);
                if (TenantService.IsNewOpusTenant())
                {
                    result.MessageType = RAMessageType.Failed;
                    result.ErrorMessage = string.Format(I18NEntity.GetString("RM_JS_Common_ErrorInOperateRule"), e.Message);
                    return result;
                }
                else
                {
                    result.MessageType = RAMessageType.Failed;
                    result.ErrorMessage = "Error occured while communicating with DocAve Service, Please check the configure file or DocAve Service status.";
                    return result;
                }
            }
        }
        /// <summary>
        /// load rule from recors..
        /// </summary>
        /// <param name="ruleId"></param>
        /// <returns></returns>
        [RACodeReview("Allen Yin")]
        public async Task<RMRuleInfos> LoadRuleAsync(string ruleId, bool isControlPlus = false)
        {
            try
            {
                logger.Info("Load rule from DocAve Control Service.");
                RMRuleInfos ruleInfo = null;
                // DAOAPIClientV1 client = null;
                //IMStorageOptimizationService soService = DocAveServiceHelper.CreateServiceClient<IMStorageOptimizationService>();
                //using (var performance = new PerformanceScope($"init client api,rule id:{ruleId}"))
                //{
                //    client = new DAOAPIClientV1();
                //}
                using (var performance = new PerformanceScope($"get rule by id:{ruleId}"))
                {
                    Rule rule = RealLoadRule(ruleId);
                    ruleInfo = await ConvertToRuleInfoAsync(rule, isControlPlus);

                    var allStorage = (await StorageDeviceService.GetAllAsync()).ToDictionary(s => s.Id);
                    if (ruleInfo.StoragePolicyId != null && allStorage.ContainsKey(ruleInfo.StoragePolicyId))
                    {
                        ruleInfo.StoragePolicyType = allStorage[ruleInfo.StoragePolicyId]?.Type ?? (int)StorageDeviceType.None;
                        ruleInfo.IsSystemStorage = allStorage[ruleInfo.StoragePolicyId]?.IsSystemStorage ?? false;
                    }

                    if (ruleInfo.OneDriveRule != null && ruleInfo.OneDriveRule.StoragePolicyId != null && allStorage.ContainsKey(ruleInfo.OneDriveRule.StoragePolicyId))
                    {
                        ruleInfo.OneDriveRule.StoragePolicyType = allStorage[ruleInfo.OneDriveRule.StoragePolicyId]?.Type ?? (int)StorageDeviceType.None;
                        ruleInfo.OneDriveRule.IsSystemStorage = allStorage[ruleInfo.OneDriveRule.StoragePolicyId]?.IsSystemStorage ?? false;
                    }

                    if (ruleInfo.TeamsRule != null && ruleInfo.TeamsRule.StoragePolicyId != null && allStorage.ContainsKey(ruleInfo.TeamsRule.StoragePolicyId))
                    {
                        ruleInfo.TeamsRule.StoragePolicyType = allStorage[ruleInfo.TeamsRule.StoragePolicyId]?.Type ?? (int)StorageDeviceType.None;
                        ruleInfo.TeamsRule.IsSystemStorage = allStorage[ruleInfo.TeamsRule.StoragePolicyId]?.IsSystemStorage ?? false;
                    }
                    if (ruleInfo.GoogleDriveRule != null && ruleInfo.GoogleDriveRule.StoragePolicyId != null && allStorage.ContainsKey(ruleInfo.GoogleDriveRule.StoragePolicyId))
                    {
                        ruleInfo.GoogleDriveRule.StoragePolicyType = allStorage[ruleInfo.GoogleDriveRule.StoragePolicyId]?.Type ?? (int)StorageDeviceType.None;
                        ruleInfo.GoogleDriveRule.IsSystemStorage = allStorage[ruleInfo.GoogleDriveRule.StoragePolicyId]?.IsSystemStorage ?? false;
                    }

                    var ruleContainer = RMRuleDao.GetRuleContainersByRuleId(new Guid(ruleId));
                    ruleInfo.ContainerId = ruleContainer.ContainerId;
                    ruleInfo.ContainerName = I18NEntity.GetString(ruleContainer.Name);
                }

                return ruleInfo;
            }
            catch (AveException ae)
            {
                logger.Error(ae.Message, ae);
                throw;
            }
            catch (Exception e)
            {
                logger.Error(e.Message, e);
                if (TenantService.IsNewOpusTenant())
                {
                    throw new AveException(string.Format(I18NEntity.GetString("RM_JS_Common_ErrorInOperateRule"), e.Message));
                }
                else
                {
                    throw new AveException("Error occured while communicating with DocAve Service, Please check the configure file or DocAve Service status.");
                }
            }
        }

        private bool IsEnableDeleteOnlyOptionSetting()
        {
            var key = RMKeyValueDao.GetValueByKey("EnableDeleteOnlyOption");
            bool.TryParse(key?.Value, out bool result);
            return result;
        }

        private bool IsEnableArchiveOnlyOptionSetting()
        {
            var key = RMKeyValueDao.GetValueByKey("EnableArchiveOnlyOption");
            bool.TryParse(key?.Value, out bool result);
            return result;
        }

        private bool IsDeleteOnlyAction(int ruleKeepDataOption)
        {
            if ((ruleKeepDataOption & (int)KeepDataOption.DeleteOnly) == (int)KeepDataOption.DeleteOnly
                || (ruleKeepDataOption & (int)KeepDataOption.KeepLatestVersion) == (int)KeepDataOption.KeepLatestVersion
                || (ruleKeepDataOption & (int)KeepDataOption.ArchiveLatestVersion) == (int)KeepDataOption.ArchiveLatestVersion
                || (ruleKeepDataOption & (int)KeepDataOption.KeepLatestVersionAndArhiveOthers) == (int)KeepDataOption.KeepLatestVersionAndArhiveOthers
                )
            {
                return true;
            }
            return false;
        }

        private bool IsArchiveOnlyAction(int ruleKeepDataOption)
        {
            if ((ruleKeepDataOption & (int)KeepDataOption.ArchiverOnly) == (int)KeepDataOption.ArchiverOnly
                || (ruleKeepDataOption & (int)KeepDataOption.ArchiveOnlyLastestVersion) == (int)KeepDataOption.ArchiveOnlyLastestVersion
                )
            {
                return true;
            }
            return false;
        }

        private bool ValidRuleActionPemerssion(int ruleKeepDataOption, out RAReturnMessage errorMessage)
        {
            bool needCheckUserLicense = false;
            if (IsArchiveOnlyAction(ruleKeepDataOption))
            {
                if (!IsEnableArchiveOnlyOptionSetting())
                {
                    errorMessage = new RAReturnMessage() { MessageType = RAMessageType.Failed, ErrorMessage = "Can not save or edit this rule because the rule contains unsupported action, no db permission." };
                    return false;
                }
                needCheckUserLicense = true;
            }
            if (IsDeleteOnlyAction(ruleKeepDataOption) && !IsEnableDeleteOnlyOptionSetting())
            {
                needCheckUserLicense = true;
            }
            if (needCheckUserLicense && !IsPrePaidConsumptionLicense())
            {
                errorMessage = new RAReturnMessage() { MessageType = RAMessageType.Failed, ErrorMessage = "Can not save or edit this rule because the rule contains unsupported action." };
                return false;
            }
            errorMessage = new RAReturnMessage() { MessageType = RAMessageType.Successful };
            return true;
        }

        public DestinationLocationInfo ConverDADestinationInfo(MoveDestinationInfo info)
        {
            DestinationLocationInfo destinationInfo = new DestinationLocationInfo();

            destinationInfo.Url = Convert.ToBase64String(CspCommunicationWrapper.WrapKey(Encoding.UTF8.GetBytes(info.Url)));
            destinationInfo.UserName = Convert.ToBase64String(CspCommunicationWrapper.WrapKey(Encoding.UTF8.GetBytes(info.UserName)));
            destinationInfo.Password = Convert.ToBase64String(CspCommunicationWrapper.WrapKey(Encoding.UTF8.GetBytes(info.PassWord)));
            return destinationInfo;
        }

        [Audit(Module = AuditModule.RetentionAndDisposalManagement, Category = AuditCategory.RuleManagement, Action = AuditAction.CreateRule, AfterHandler = typeof(RuleManagerAfterAuditHandler))]
        [RACodeReview("Allen Yin")]
        public async Task<RAReturnMessage> CreateRuleInDAAsync(RMRuleInfos rule)
        {
            if (rule == null)
            {
                return null;
            }
            if(!ValidRuleActionPemerssion(rule.RuleKeepDataOption | rule?.OneDriveRule?.RuleKeepDataOption ?? 0, out RAReturnMessage errorMessage))
            {
                return errorMessage;
            }
            await CheckRuleIsRightAsync(rule, allowMissingExistingRule: true);
            try
            {
                logger.Info("Ceate rule to DocAve Control from REC.");
                var syncUserResult = await SyncADUsersAsync(rule);
                if (syncUserResult.MessageType == RAMessageType.Failed)
                {
                    return syncUserResult;
                }

                //var client = new DAOAPIClientV1();
                //IMArchiverService archiverService = DocAveServiceHelper.CreateServiceClient<IMArchiverService>();
                try
                {
                    using (var performance = new PerformanceScope($"create rule:{rule?.RuleName}"))
                    {
                        rule.RuleName = rule.RuleName.Trim();
                        //if (string.IsNullOrEmpty(rule.RuleName) || rule.ModelType == RuleModel.None)
                        if (string.IsNullOrEmpty(rule.RuleName))
                        {
                            NotifyInvalidRule();
                        }

                        var bRule = await BuildRMRuleAsync(rule);
                        if (string.IsNullOrEmpty(bRule.Id))
                        {
                            bRule.Id = Guid.NewGuid().ToString();
                        }
                        rule.RuleId = bRule.Id;
                        ValidRetentionKeepDateOption(rule);
                        CheckSameNameRule(rule);
                        if (RealCreateRule(bRule))
                        {
                            RMRuleDao.AddOrUpdateRMRule(new RMRule()
                            {
                                RuleId = new Guid(bRule.Id),
                                RuleName = bRule.Name,
                                RuleLevel = (int)bRule.PolicyLevel,
                                DisposalAction = (int)RuleHelper.GetOperationTypeForSP(bRule),
                                ExchangeDisposalAction = (int)RuleHelper.GetOperationTypeForEXO(bRule.EXORule),
                                PhysicalDisposalAction = (int)RuleHelper.GetOperationTypeForPhysical(bRule.PhysicalRule),
                                FSDisposalAction = (int)RuleHelper.GetOperationTypeForFS(bRule.FSRule),
                                SPLocalDisposalAction = (int)RuleHelper.GetOperationTypeForSPLocal(bRule.SPLocalRule),
                                OneDriveDisposalAction = (int)RuleHelper.GetOperationTypeForOneDrive(bRule.OneDriveRule),
                                AzureFileDisposalAction = (int)RuleHelper.GetOperationTypeForAzureFile(bRule.AzureFileRule),
                                BoxDisposalAction = (int)RuleHelper.GetOperationTypeForBox(bRule.BoxRule),
                                ConnectorDisposalAction = (int)RuleHelper.GetOperationTypeForConnector(bRule.ConnectorRule),
                                GoogleDriveDisposalAction = (int)RuleHelper.GetOperationTypeForGoogleDrive(bRule.GoogleDriveRule),
                                TeamsDisposalAction = (int)RuleHelper.GetOperationTypeForTeams(bRule.TeamsRule),
                                DeleteRecords = bRule.DeleteRecords,
                                IsRemoved = false,
                                Description = bRule.Description,
                                ModifyTime = bRule.ModifyTime,
                                DisposalClass = bRule.DisposalClass,
                                Extension = SerializerHelper.SerializeByDataContractJsonSerializer(bRule),
                                ModelType = (int)rule.ModelType,
                            }, rule.ContainerId);
                            return new RAReturnMessage() { MessageType = RAMessageType.Successful };
                        }
                        else
                        {
                            return new RAReturnMessage() { MessageType = RAMessageType.Failed };
                        }
                    }

                }
                catch (EnableDataCollectionStatusException e)
                {
                    logger.Error($"An error occured while call insights api,rule name:{rule.RuleName},ERROR:{e}");
                    return new RAReturnMessage() { MessageType = RAMessageType.Failed, ErrorMessage = I18NEntity.GetString("RM_EnableDataCollectionSwitch_Error_Message") };
                }
                catch (Exception e)
                {
                    logger.Error("an error occured while create rule,rule name:{0},ERROR:{1}", rule.RuleName, e.ToString());
                    return new RAReturnMessage() { MessageType = RAMessageType.Failed, ErrorMessage = ParseErrorMessageFromDAOL(e.Message) };
                }
            }
            catch (Exception e)
            {
                logger.Error("an error occured while connect to DAO,rule name:{0},ERROR:{1}", rule.RuleName, e.ToString());
                throw new AveException(I18NEntity.GetString("RM_JS_JM_Summary_ConnectDAOFailed"));
            }
        }

        private void ValidRetentionKeepDateOption(RMRuleInfos rule)
        {
            bool isEnableSoftDelete = IsEnableSoftDeleteSetting();
            if (rule != null && rule.RetentionInfoList != null && rule.RetentionInfoList.Count == 1)
            {
                var retentionInfo = rule.RetentionInfoList[0];
                if (retentionInfo.IsSoftDelete && !isEnableSoftDelete)
                {
                    logger.Error("this retention rule is soft delete but it not enable soft delete");
                    throw new Exception("Error param");
                }
                if (retentionInfo.RetentionDataTimeType == KeepDateType.ModifiedTime)
                {
                    if (int.TryParse(RMKeyValueDao.GetValueByKey(RMKeyValuesConstants.ArchiverBackupOutputStreamLevel)?.Value, out var outputStreamLevel))
                    {
                        if (outputStreamLevel != (int)OutputStreamLevel.FileLevel)
                        {
                            throw new Exception("Error param");
                        }
                    }
                    else
                    {
                        throw new Exception("Error param");
                    }
                }
            }
            else if (rule != null && rule.RetentionInfoList != null)
            {
                foreach (var temp in rule.RetentionInfoList)
                {
                    if (temp.IsSoftDelete && !isEnableSoftDelete)
                    {
                        logger.Error("this retention rule is soft delete but it not enable soft delete");
                        throw new Exception("Error param");
                    }
                }
            }
            if (rule != null && rule.OneDriveRule != null && rule.OneDriveRule.RetentionInfoList != null && rule.OneDriveRule.RetentionInfoList.Count == 1)
            {
                var retentionInfo = rule.OneDriveRule.RetentionInfoList[0];
                if (retentionInfo.IsSoftDelete && !isEnableSoftDelete)
                {
                    logger.Error("this retention rule is soft delete but it not enable soft delete");
                    throw new Exception("Error param");
                }
                if (retentionInfo.RetentionDataTimeType == KeepDateType.ModifiedTime)
                {
                    if (int.TryParse(RMKeyValueDao.GetValueByKey(RMKeyValuesConstants.ArchiverBackupOutputStreamLevel)?.Value, out var outputStreamLevel))
                    {
                        if (outputStreamLevel != (int)OutputStreamLevel.FileLevel)
                        {
                            throw new Exception("Error param");
                        }
                    }
                }
            }
            else if (rule != null && rule.OneDriveRule != null && rule.OneDriveRule.RetentionInfoList != null)
            {
                foreach (var temp in rule.OneDriveRule.RetentionInfoList)
                {
                    if (temp.IsSoftDelete && !isEnableSoftDelete)
                    {
                        logger.Error("this retention rule is soft delete but it not enable soft delete");
                        throw new Exception("Error param");
                    }
                }
            }
        }
        private bool IsEnableSoftDeleteSetting()
        {
            var key = RMKeyValueDao.GetValueByKey("EnableSoftDelete");
            bool.TryParse(key?.Value, out bool result);
            return result;
        }
        public async Task<RAReturnMessage> CreateImportRuleInDAAsync(RMRuleInfos rule)
        {
            if (rule == null)
            {
                return null;
            }
            //CheckRuleIsRight(rule);
            try
            {
                logger.Info("Ceate rule to DocAve Control from REC.");
                //var client = new DAOAPIClientV1();
                //IMArchiverService archiverService = DocAveServiceHelper.CreateServiceClient<IMArchiverService>();
                try
                {

                    var bRule = await BuildRMRuleAsync(rule);
                    if (string.IsNullOrEmpty(bRule.Id))
                    {
                        bRule.Id = Guid.NewGuid().ToString();
                    }
                    if (RealCreateRule(bRule))
                    {
                        RMRuleDao.AddOrUpdateRMRule(new RMRule()
                        {
                            RuleId = new Guid(bRule.Id),
                            RuleName = bRule.Name,
                            RuleLevel = (int)bRule.PolicyLevel,
                            DisposalAction = (int)RuleHelper.GetOperationTypeForSP(bRule),
                            ExchangeDisposalAction = (int)RuleHelper.GetOperationTypeForEXO(bRule.EXORule),
                            //TO DO FS
                            PhysicalDisposalAction = (int)RuleHelper.GetOperationTypeForPhysical(bRule.PhysicalRule),
                            //PhysicalDisposalAction = (int)RuleHelper.GetOperationTypeForReport(bRule.PhysicalRule),
                            FSDisposalAction = (int)RuleHelper.GetOperationTypeForFS(bRule.FSRule),
                            SPLocalDisposalAction = (int)RuleHelper.GetOperationTypeForSPLocal(bRule.SPLocalRule),
                            OneDriveDisposalAction = (int)RuleHelper.GetOperationTypeForOneDrive(bRule.OneDriveRule),
                            AzureFileDisposalAction = (int)RuleHelper.GetOperationTypeForAzureFile(bRule.AzureFileRule),
                            ConnectorDisposalAction = (int)RuleHelper.GetOperationTypeForConnector(bRule.ConnectorRule),
                            TeamsDisposalAction = (int)RuleHelper.GetOperationTypeForTeams(bRule.TeamsRule),
                            DeleteRecords = bRule.DeleteRecords,
                            IsRemoved = false,
                            Description = bRule.Description,
                            ModifyTime = bRule.ModifyTime,
                            DisposalClass = bRule.DisposalClass,
                            Extension = SerializerHelper.SerializeByDataContractJsonSerializer(bRule),
                            ModelType = (int)rule.ModelType,
                            GoogleDriveDisposalAction = (int)RuleHelper.GetOperationTypeForConnector(bRule.GoogleDriveRule)
                        }, rule.ContainerId) ;
                        return new RAReturnMessage() { MessageType = RAMessageType.Successful };
                    }
                    else
                    {
                        return new RAReturnMessage() { MessageType = RAMessageType.Failed };
                    }
                }
                catch (Exception e)
                {
                    logger.Error("an error occured while create rule,rule name:{0},ERROR:{1}", rule.RuleName, e.ToString());
                    return new RAReturnMessage() { MessageType = RAMessageType.Failed, ErrorMessage = ParseErrorMessageFromDAOL(e.Message) };
                }
            }
            catch (Exception e)
            {
                logger.Error("an error occured while connect to DAO,rule name:{0},ERROR:{1}", rule.RuleName, e.ToString());
                throw new AveException(I18NEntity.GetString("RM_JS_JM_Summary_ConnectDAOFailed"));
            }
        }

        private bool RealCreateRule(Rule rule)
        {
            var client = new DAOAPIClientV1();
            return client.CreateRuleInProfile(rule);
        }

        private Task<bool> RealEditRuleAsync(Rule rule)
        {
            var client = new DAOAPIClientV1();
            return client.EditRuleAsync(rule);
        }

        private async Task<bool> RealDeleteRuleAsync(List<string> ids)
        {
            var client = new DAOAPIClientV1();
            return await client.BatchDeleteRulesAsync(ids);
        }

        private Rule RealLoadRule(string ruleId)
        {
            var client = new DAOAPIClientV1();
            return client.LoadRule(ruleId);
        }

        private async System.Threading.Tasks.Task CheckRuleIsRightAsync(RMRuleInfos rule, bool allowMissingExistingRule = false)
        {
            await CheckRuleNameAsync(rule, allowMissingExistingRule);
            CheckTriggerMicrosoft365ArchivingAction(rule);
            if (rule.IsSpSource)
            {
                int keepDataOption = rule.RuleKeepDataOption;
                bool delReecords = rule.DeleteRecords;
                bool manualApp = rule.EnableManualApproval;
                bool export = rule.EnableExport;

                //check RuleLevel
                if (rule.RuleLevel == PolicyLevel.None)
                {
                    NotifyInvalidRule();
                }
                if(rule.ModelType != RuleModel.SOArchiver)
                {
                    if((keepDataOption & (int)KeepDataOption.KeepLatestVersionAndArhiveOthers) == (int)KeepDataOption.KeepLatestVersionAndArhiveOthers
                        || rule.KeepLatestMajorAndMinorVersionAndArchiveOthers != 0)
                    {
                        NotifyInvalidRule();
                    }
                }

                if(rule.RuleLevel <= PolicyLevel.Folder &&
                    (keepDataOption & (int)KeepDataOption.IsEnableRemoveRetentionLabel) == (int)KeepDataOption.IsEnableRemoveRetentionLabel)
                {
                    rule.RuleKeepDataOption -= (int)KeepDataOption.IsEnableRemoveRetentionLabel;
                }

                //keep data
                if ((keepDataOption & 16) == 16)
                {
                    if (delReecords || manualApp)
                    {
                        NotifyInvalidRule();
                    }
                }
                //move
                if (keepDataOption == 0 && (rule.MoveToRecordCenterSettings != null &&
                    !string.IsNullOrEmpty(rule.MoveToRecordCenterSettings.DestinationLocation.Url)
                    && !string.IsNullOrEmpty(rule.MoveToRecordCenterSettings.DestinationLocation.Password)
                    && !string.IsNullOrEmpty(rule.MoveToRecordCenterSettings.DestinationLocation.UserName)))
                {
                    if (delReecords || manualApp || export)
                    {
                        NotifyInvalidRule();
                    }
                }
                //RECO-1380
                if (rule.EnableExport && rule.ExportInfo != null)
                {
                    bool NAAOrNARA = rule.ExportInfo.exportType == AvePoint.GCommon.Contract.StorageOptimization.Object.ExportTypeValue.NAA || rule.ExportInfo.exportType == AvePoint.GCommon.Contract.StorageOptimization.Object.ExportTypeValue.NARA;
                    if (NAAOrNARA && rule.RuleLevel != PolicyLevel.Document && rule.RuleLevel != PolicyLevel.Folder)
                    {
                        NotifyInvalidRule();
                    }

                    ValidateExportInfoData(rule);
                }

                if((rule.RuleLevel == PolicyLevel.DocumentVersion || rule.RuleLevel == PolicyLevel.ItemVersion || rule.RuleLevel == PolicyLevel.Attachment) && rule.DeleteRecords)
                {
                    NotifyInvalidRule();
                }
                await CheckStoragePolicyAsync(rule);
                CheckStubTemplate(rule);
                await CheckRetentionAsync(rule);
                CheckManualRule(rule);
                //check value: value1不能为空
                CheckRuleFilters(rule.RuleFilters, RMRuleSourceType.SP, rule.ModelType);
                if (AccountUtility.IsSupportRecordLabel())
                {
                    if((keepDataOption & (int)KeepDataOption.DeclareRecord) == (int)KeepDataOption.DeclareRecord)
                    {
                        NotifyInvalidRule();
                    }
                }
            }

            CheckExoRule(rule);
            await CheckPhysicalRuleAsync(rule);
            CheckFSRule(rule);
            CheckSPLocalRule(rule);
            await CheckOneDriveRuleAsync(rule);
            CheckAzureFileRule(rule);
            CheckBoxRule(rule);
            CheckConnectorRule(rule);
            CheckGoogleDriveRule(rule);
            await CheckTeamsRule(rule);
        }

        private void CheckTriggerMicrosoft365ArchivingAction(RMRuleInfos rule)
        {
            ValidateTriggerMicrosoft365ArchivingAction(rule, rule?.IsSpSource == true);
            ValidateTriggerMicrosoft365ArchivingAction(rule?.EXORule, false);
            ValidateTriggerMicrosoft365ArchivingAction(rule?.PhysicalRule, false);
            ValidateTriggerMicrosoft365ArchivingAction(rule?.FSRule, false);
            ValidateTriggerMicrosoft365ArchivingAction(rule?.SPLocalRule, false);
            ValidateTriggerMicrosoft365ArchivingAction(rule?.OneDriveRule, false);
            ValidateTriggerMicrosoft365ArchivingAction(rule?.AzureFileRule, false);
            ValidateTriggerMicrosoft365ArchivingAction(rule?.BoxRule, false);
            ValidateTriggerMicrosoft365ArchivingAction(rule?.ConnectorRule, false);
            ValidateTriggerMicrosoft365ArchivingAction(rule?.GoogleDriveRule, false);
            ValidateTriggerMicrosoft365ArchivingAction(rule?.TeamsRule, false);
        }

        private void ValidateTriggerMicrosoft365ArchivingAction(RMRuleInfos rule, bool isSharePointSource)
        {
            if (rule == null)
            {
                return;
            }

            if ((rule.RuleKeepDataOption & (int)KeepDataOption.TriggerMicrosoft365Archiving) == (int)KeepDataOption.TriggerMicrosoft365Archiving)
            {
                bool isValidLevel = rule.RuleLevel == PolicyLevel.Document
                                 || rule.RuleLevel == PolicyLevel.SiteCollection;

                if (!isSharePointSource || !isValidLevel)
                {
                    NotifyInvalidRule();
                }
            }
        }

        private void ValidateExportInfoData(RMRuleInfos ruleInfo)
        {
            if (!TenantService.IsNewOpusTenant())
            {
                return;
            }
            var invalidExportInfoStorage = ruleInfo.ExportInfo.exportLocationId.IsNullOrEmpty() ||
                                           ruleInfo.ExportInfo.exportLocationName.IsNullOrEmpty();
            if (ruleInfo.MoveDto == null && invalidExportInfoStorage)
            {
                NotifyInvalidRule();
            }
        }

        private bool CheckFilterOfListInIsRight(List<RuleFilter> filters, RMRuleSourceType source)
        {
            var isJpmcOpen = KeyValueDao.GetValueByKey("JPMC_Customization") != null;
            if (isJpmcOpen)
            {
                if (source != RMRuleSourceType.SP && source != RMRuleSourceType.OneDrive && source != RMRuleSourceType.Teams && source != RMRuleSourceType.FS)
                {
                    if (filters.Any(s => s.Condition == ArchiverFilterCondition.ListIn))
                    {
                        return false;
                    }
                }
                else
                {
                    if (filters.Any(s => s.Condition == ArchiverFilterCondition.ListIn && s.RuleType != ArchiverFilterRuleType.TextColumn))
                    {
                        return false;
                    }
                }
            }
            else
            {
                if (filters.Any(s => s.Condition == ArchiverFilterCondition.ListIn))
                {
                    return false;
                }
            }
            return true;
        }

        private async Task CheckStoragePolicyAsync(RMRuleInfos rule)
        {
            if (NeedCheckStorage(rule))
            {
                if (string.IsNullOrWhiteSpace(rule.StoragePolicyId))
                {
                    NotifyInvalidRule();
                }

                var storage = StorageDeviceService.GetStorageDeviceById(rule.StoragePolicyId);
                if (storage == null || !storage.Name.Equals(rule.StoragePolicyName))
                {
                    NotifyInvalidRule();
                }
                if (storage!= null && storage.IsSystemStorage && rule.IsEnableRetention && rule.RetentionInfo != null
                    && !StorageDeviceService.IsDisableRetentionPeriodLimitation())
                {
                    if (rule.RetentionInfo.Condition == TimeFilterCondition.OlderThan)
                    {
                        if (rule.RetentionInfo.KeepDateUnite == TimeUnit.Day)
                        {
                            if (rule.RetentionInfo.KeepDateNumber < 91)
                            {
                                NotifyInvalidRule();
                            }
                        }
                        if (rule.RetentionInfo.KeepDateUnite == TimeUnit.Week)
                        {
                            if (rule.RetentionInfo.KeepDateNumber < 13)
                            {
                                NotifyInvalidRule();
                            }
                        }
                        if (rule.RetentionInfo.KeepDateUnite == TimeUnit.Month)
                        {
                            if (rule.RetentionInfo.KeepDateNumber < 4)
                            {
                                NotifyInvalidRule();
                            }
                        }
                    }
                    else if (rule.RetentionInfo.Condition == TimeFilterCondition.Is)
                    {
                        if (DateTime.UtcNow.Ticks - await ConvertTime4BackendAsync(rule.RetentionInfo.Date) < TimeSpan.FromDays(91).Ticks)
                        {
                            NotifyInvalidRule();
                        }
                    }
                }
            }
        }

        private void CheckStubTemplate(RMRuleInfos rule)
        {
            if (NeedCheckStubTemplate(rule))
            {
                if (string.IsNullOrWhiteSpace(rule.StubTemplateId))
                {
                    NotifyInvalidRule();
                }

                var stubSetting = StubSettingService.GetStubSettingById(rule.StubTemplateId);
                if (stubSetting == null || !stubSetting.Name.Equals(rule.StubTemplateName))
                {
                    NotifyInvalidRule();
                }
            }
        }

        private bool NeedCheckStubTemplate(RMRuleInfos rule)
        {
            if (rule.RuleLevel == PolicyLevel.Document && ((rule.RuleKeepDataOption & 128) == (int)KeepDataStatus.LinkToDocument || rule.RuleKeepDataOption == (int)KeepDataStatus.ArchiveAndLeaveStub))
            {
                return true;
            }
            return false;
        }

        private bool NeedCheckStorage(RMRuleInfos rule)
        {
            //export
            if (rule.EnableExport && rule.ExportInfo != null && rule.ExportInfo.exportSPDataOption == ExportSPDataOption.ExportWithoutArchive)
            {
                return false;
            }
            //move
            if (rule.MoveDto != null)
            {
                return false;
            }
            //keep data
            if ((rule.RuleKeepDataOption & 16) == 16)
            {
                return false;
            }

            List<PolicyLevel> ruleLevels = new List<PolicyLevel>()
            {
                PolicyLevel.SiteCollection,
                PolicyLevel.Site,
                PolicyLevel.List,
                PolicyLevel.Folder,
                PolicyLevel.Item,
                PolicyLevel.ItemVersion,
                PolicyLevel.Attachment,
                PolicyLevel.Document,
                PolicyLevel.DocumentVersion
            };
            if (ruleLevels.Contains(rule.RuleLevel) && rule.RuleKeepDataOption == (int)KeepDataStatus.Delete)
            {
                return true;
            }

            if (rule.RuleLevel == PolicyLevel.Document && (rule.RuleKeepDataOption == (int)KeepDataStatus.Archive
                || rule.RuleKeepDataOption == (int)KeepDataStatus.ArchiveAndLeaveStub)
                || rule.RuleKeepDataOption == 4224)
            {
                return true;
            }

            if (rule.IsSpSource && rule.RelatedRecordOption == GCommon.Contract.StorageOptimization.Object.RelatedRecordOption.Both)
            {
                return true;
            }

            if (rule.RuleLevel == PolicyLevel.PhysicalFile 
                && rule.RuleKeepDataOption == (int)KeepDataStatus.Delete 
                && rule.RelatedRecordOption == GCommon.Contract.StorageOptimization.Object.RelatedRecordOption.Both)
            {
                return true;
            }

            return false;
        }

        private void CheckFSRule(RMRuleInfos rule)
        {
            var fsRule = rule.FSRule;
            if (null != fsRule)
            {
                bool isNewOpus = TenantService.IsNewOpusTenant();
                CheckArchiveRuleHasStorageInfo(fsRule.RuleKeepDataOption, fsRule.StoragePolicyId, fsRule.StoragePolicyName);
                //check ruleLevel
                if (rule.RuleLevel != PolicyLevel.Document || fsRule.RuleLevel != PolicyLevel.FileSysFile)
                {
                    NotifyInvalidRule();
                }

                //*****FS暂时不支持Export****
                //if (fsRule.EnableExport && fsRule.ExportInfo != null)
                //{
                //    bool NAAOrNARA = fsRule.ExportInfo.exportType == ExportTypeValue.NAA || fsRule.ExportInfo.exportType == ExportTypeValue.NARA;
                //    if (NAAOrNARA && (fsRule.RuleLevel != PolicyLevel.FileSysFile || fsRule.RuleLevel != PolicyLevel.FileSysFolder))
                //    {
                //        NotifyInvalidRule();
                //    }
                //}
                if ((fsRule.RuleKeepDataOption & (int)KeepDataOption.Archive) == (int)KeepDataOption.Archive && !isNewOpus)
                {
                    NotifyInvalidRule();
                }
                if (fsRule.StoragePolicyId != null && fsRule.StoragePolicyId != Guid.Empty.ToString() && !isNewOpus)
                {
                    NotifyInvalidRule();
                }

                CheckManualRule(fsRule);
                //check value: value1不能为空
                CheckRuleFilters(fsRule.RuleFilters, RMRuleSourceType.FS);
                if (fsRule.RuleFilters.Any(o => o.Level != PolicyLevel.FileSysFile))
                {
                    NotifyInvalidRule();
                }
            }
        }
        private void CheckAzureFileRule(RMRuleInfos rule)
        {
            var azureFileRule = rule.AzureFileRule;
            if (null != azureFileRule)
            {
                //check ruleLevel
                if (rule.RuleLevel != PolicyLevel.Document || azureFileRule.RuleLevel != PolicyLevel.AzureFileDocument)
                {
                    NotifyInvalidRule();
                }

                //*****FS暂时不支持Export****
                //if (fsRule.EnableExport && fsRule.ExportInfo != null)
                //{
                //    bool NAAOrNARA = fsRule.ExportInfo.exportType == ExportTypeValue.NAA || fsRule.ExportInfo.exportType == ExportTypeValue.NARA;
                //    if (NAAOrNARA && (fsRule.RuleLevel != PolicyLevel.FileSysFile || fsRule.RuleLevel != PolicyLevel.FileSysFolder))
                //    {
                //        NotifyInvalidRule();
                //    }
                //}
                CheckManualRule(azureFileRule);
                //check value: value1不能为空
                CheckRuleFilters(azureFileRule.RuleFilters, RMRuleSourceType.AzureFile);
                if (azureFileRule.RuleFilters.Any(o => o.Level != PolicyLevel.AzureFileDocument))
                {
                    NotifyInvalidRule();
                }
            }
        }

        private void CheckBoxRule(RMRuleInfos rule)
        {
            var boxRule = rule.BoxRule;
            if (null != boxRule)
            {
                if (rule.RuleLevel != PolicyLevel.Document || boxRule.RuleLevel != PolicyLevel.BoxDocument)
                {
                    NotifyInvalidRule();
                }
                CheckManualRule(boxRule);
                CheckRuleFilters(boxRule.RuleFilters, RMRuleSourceType.Box);
                if (boxRule.RuleFilters.Any(o => o.Level != PolicyLevel.BoxDocument))
                {
                    NotifyInvalidRule();
                }
            }
        }

        private void CheckConnectorRule(RMRuleInfos rule)
        {
            var connectorRule = rule.ConnectorRule;
            if (null != connectorRule)
            {
                //check ruleLevel
                if (rule.RuleLevel != PolicyLevel.Document)
                {
                    NotifyInvalidRule();
                }

                //*****FS暂时不支持Export****
                //if (fsRule.EnableExport && fsRule.ExportInfo != null)
                //{
                //    bool NAAOrNARA = fsRule.ExportInfo.exportType == ExportTypeValue.NAA || fsRule.ExportInfo.exportType == ExportTypeValue.NARA;
                //    if (NAAOrNARA && (fsRule.RuleLevel != PolicyLevel.FileSysFile || fsRule.RuleLevel != PolicyLevel.FileSysFolder))
                //    {
                //        NotifyInvalidRule();
                //    }
                //}
                CheckManualRule(connectorRule);
                //check value: value1不能为空
                CheckRuleFilters(connectorRule.RuleFilters, RMRuleSourceType.Connector);
                if (connectorRule.RuleFilters.Any(o => o.Level != PolicyLevel.Document))
                {
                    NotifyInvalidRule();
                }
            }
        }
        private void CheckExoRule(RMRuleInfos rule)
        {
            var exoRule = rule.EXORule;
            if (null != exoRule)
            {
                //check ruleLevel
                if (rule.RuleLevel != PolicyLevel.Document || exoRule.RuleLevel != PolicyLevel.ExchangeOnlineItem)
                {
                    NotifyInvalidRule();
                }
                if (exoRule.EnableExport && exoRule.ExportInfo != null)
                {
                    bool NAAOrNARA = exoRule.ExportInfo.exportType == AvePoint.GCommon.Contract.StorageOptimization.Object.ExportTypeValue.NAA || exoRule.ExportInfo.exportType == AvePoint.GCommon.Contract.StorageOptimization.Object.ExportTypeValue.NARA;
                    if (NAAOrNARA && exoRule.RuleLevel != PolicyLevel.ExchangeOnlineItem)
                    {
                        NotifyInvalidRule();
                    }

                    ValidateExportInfoData(exoRule);
                }
                CheckManualRule(exoRule);
                //check value: value1不能为空
                CheckRuleFilters(exoRule.RuleFilters, RMRuleSourceType.EXO);
                CheckMoveSPMetadata(exoRule.MoveDto);
            }
        }
        private async Task CheckPhysicalRuleAsync(RMRuleInfos rule)
        {
            var phyRule = rule.PhysicalRule;
            if (null != phyRule)
            {
                //check ruleLevel
                if (rule.RuleLevel != PolicyLevel.Folder && rule.RuleLevel != PolicyLevel.List)
                {
                    NotifyInvalidRule();
                }
                if (rule.RuleLevel == PolicyLevel.Folder && phyRule.RuleLevel == PolicyLevel.PhysicalBox)
                {
                    NotifyInvalidRule();
                }
                if (rule.RuleLevel == PolicyLevel.List && phyRule.RuleLevel == PolicyLevel.PhysicalFile)
                {
                    NotifyInvalidRule();
                }
                //physical box rule暂时不支持related records option
                if (rule.RuleLevel == PolicyLevel.List)
                {
                    if (phyRule.RelatedRecordOption != AvePoint.GCommon.Contract.StorageOptimization.Object.RelatedRecordOption.None)
                    {
                        NotifyInvalidRule();
                    }
                }
                //physical rule不支持export
                if (phyRule.EnableExport)
                {
                    NotifyInvalidRule();
                }
                //physical RuleKeepDataOption前台传值是0
                if ((phyRule.RuleKeepDataOption != (int)KeepDataStatus.Delete))
                {
                    NotifyInvalidRule();
                }
                await CheckStoragePolicyAsync(phyRule);
                await CheckRetentionAsync(phyRule);
                CheckManualRule(phyRule);
                //check value: value1不能为空
                CheckRuleFilters(phyRule.RuleFilters, RMRuleSourceType.Physical);
            }
        }

        private void CheckSPLocalRule(RMRuleInfos rule)
        {
            var spLocalRule = rule.SPLocalRule;
            if (spLocalRule != null)
            {
                int keepDataOption = spLocalRule.RuleKeepDataOption;
                bool delReecords = spLocalRule.DeleteRecords;
                bool manualApp = spLocalRule.EnableManualApproval;
                bool export = spLocalRule.EnableExport;

                //check RuleLevel
                if (rule.RuleLevel != PolicyLevel.Document && rule.RuleLevel != PolicyLevel.Item)
                {
                    NotifyInvalidRule();
                }

                //keep data
                if ((keepDataOption & 16) == 16)
                {
                    if (delReecords || manualApp)
                    {
                        NotifyInvalidRule();
                    }
                }
                CheckManualRule(spLocalRule);
                CheckRuleFilters(spLocalRule.RuleFilters, RMRuleSourceType.SPLocal);
            }
        }
        private void CheckGoogleDriveRule(RMRuleInfos rule)
        {
            var googleRule = rule.GoogleDriveRule;
            if (googleRule != null)
            {
                //check RuleLevel
                if (rule.RuleLevel != PolicyLevel.Document || googleRule.RuleLevel != PolicyLevel.GoogleDriveDocument)
                {
                    NotifyInvalidRule();
                }
                
                if (googleRule.EnableExport && googleRule.ExportInfo != null)
                {
                    var exportTypeWhiteList = new List<GCommon.Contract.StorageOptimization.Object.ExportTypeValue> {
                        GCommon.Contract.StorageOptimization.Object.ExportTypeValue.NAA,
                        GCommon.Contract.StorageOptimization.Object.ExportTypeValue.NARA,
                        GCommon.Contract.StorageOptimization.Object.ExportTypeValue.VEO
                    };
                    if (!exportTypeWhiteList.Contains(googleRule.ExportInfo.exportType))
                    {
                        NotifyInvalidRule();
                    }

                    bool NAAOrNARA = googleRule.ExportInfo.exportType == AvePoint.GCommon.Contract.StorageOptimization.Object.ExportTypeValue.NAA || googleRule.ExportInfo.exportType == AvePoint.GCommon.Contract.StorageOptimization.Object.ExportTypeValue.NARA;
                    if (NAAOrNARA && googleRule.RuleLevel != PolicyLevel.GoogleDriveDocument && googleRule.RuleLevel != PolicyLevel.Folder)
                    {
                        NotifyInvalidRule();
                    }

                    ValidateExportInfoData(googleRule);
                }
                CheckManualRule(googleRule);
                CheckRuleFilters(googleRule.RuleFilters, RMRuleSourceType.GoogleDrive);
            }
        }

        private async Task CheckTeamsRule(RMRuleInfos rule)
        {
            var teamsRule = rule.TeamsRule;
            if (teamsRule != null)
            {
                int keepDataOption = teamsRule.RuleKeepDataOption;
                bool delReecords = teamsRule.DeleteRecords;
                bool manualApp = teamsRule.EnableManualApproval;
                bool export = teamsRule.EnableExport;

                //check RuleLevel
                if (rule.RuleLevel == PolicyLevel.None)
                {
                    NotifyInvalidRule();
                }
                if (rule.ModelType != RuleModel.SOArchiver)
                {
                    if ((keepDataOption & (int)KeepDataOption.KeepLatestVersionAndArhiveOthers) == (int)KeepDataOption.KeepLatestVersionAndArhiveOthers
                        || rule.KeepLatestMajorAndMinorVersionAndArchiveOthers != 0)
                    {
                        NotifyInvalidRule();
                    }
                }

                //keep data
                if ((keepDataOption & 16) == 16)
                {
                    if (delReecords || manualApp)
                    {
                        NotifyInvalidRule();
                    }
                }
                //move
                if (keepDataOption == 0 && (teamsRule.MoveToRecordCenterSettings != null &&
                    !string.IsNullOrEmpty(teamsRule.MoveToRecordCenterSettings.DestinationLocation.Url)
                    && !string.IsNullOrEmpty(teamsRule.MoveToRecordCenterSettings.DestinationLocation.Password)
                    && !string.IsNullOrEmpty(teamsRule.MoveToRecordCenterSettings.DestinationLocation.UserName)))
                {
                    if (delReecords || manualApp || export)
                    {
                        NotifyInvalidRule();
                    }
                }
                if (teamsRule.EnableExport && teamsRule.ExportInfo != null)
                {
                    bool NAAOrNARA = teamsRule.ExportInfo.exportType == AvePoint.GCommon.Contract.StorageOptimization.Object.ExportTypeValue.NAA || teamsRule.ExportInfo.exportType == AvePoint.GCommon.Contract.StorageOptimization.Object.ExportTypeValue.NARA;
                    if (NAAOrNARA && teamsRule.RuleLevel != PolicyLevel.Document && teamsRule.RuleLevel != PolicyLevel.Folder)
                    {
                        NotifyInvalidRule();
                    }

                    ValidateExportInfoData(teamsRule);
                }

                if ((teamsRule.RuleLevel == PolicyLevel.DocumentVersion || teamsRule.RuleLevel == PolicyLevel.ItemVersion || teamsRule.RuleLevel == PolicyLevel.Attachment) && teamsRule.DeleteRecords)
                {
                    NotifyInvalidRule();
                }

                await CheckStoragePolicyAsync(teamsRule);
                CheckStubTemplate(teamsRule);
                await CheckRetentionAsync(teamsRule);
                CheckManualRule(teamsRule);
                //check value: value1不能为空
                CheckRuleFilters(teamsRule.RuleFilters, RMRuleSourceType.Teams, rule.ModelType);
            }
        }

        private async System.Threading.Tasks.Task CheckOneDriveRuleAsync(RMRuleInfos rule)
        {
            var odRule = rule.OneDriveRule;
            if (odRule != null)
            {
                int keepDataOption = odRule.RuleKeepDataOption;
                bool delReecords = odRule.DeleteRecords;
                bool manualApp = odRule.EnableManualApproval;
                bool export = odRule.EnableExport;


                //check RuleLevel, 只支持document level
                if (rule.RuleLevel == PolicyLevel.None || 
                    (rule.RuleLevel == PolicyLevel.SiteCollection && rule.ModelType != RuleModel.SOArchiver && odRule.ModelType != RuleModel.SOArchiver))
                {
                    NotifyInvalidRule();
                }

                if (odRule.RuleLevel <= PolicyLevel.Folder &&
                    (keepDataOption & (int)KeepDataOption.IsEnableRemoveRetentionLabel) == (int)KeepDataOption.IsEnableRemoveRetentionLabel)
                {
                    odRule.RuleKeepDataOption -= (int)KeepDataOption.IsEnableRemoveRetentionLabel;
                    keepDataOption = odRule.RuleKeepDataOption;
                }

                if ((keepDataOption & (int)KeepDataOption.DeclareRecord) == (int)KeepDataOption.DeclareRecord)
                {
                    odRule.RuleKeepDataOption -= (int)KeepDataOption.DeclareRecord;
                    keepDataOption = odRule.RuleKeepDataOption;
                }

                if (rule.ModelType != RuleModel.SOArchiver)
                {
                    if ((keepDataOption & (int)KeepDataOption.KeepLatestVersionAndArhiveOthers) == (int)KeepDataOption.KeepLatestVersionAndArhiveOthers
                        || rule.KeepLatestMajorAndMinorVersionAndArchiveOthers != 0)
                    {
                        NotifyInvalidRule();
                    }
                }

                if ((odRule.RuleLevel == PolicyLevel.DocumentVersion || odRule.RuleLevel == PolicyLevel.ItemVersion || odRule.RuleLevel == PolicyLevel.Attachment) && odRule.DeleteRecords)
                {
                    NotifyInvalidRule();
                }

                //onedrive rule不支持RelatedRecord
                if (odRule.RelatedRecordOption != GCommon.Contract.StorageOptimization.Object.RelatedRecordOption.None)
                {
                    NotifyInvalidRule();
                }
                //keep data
                if ((keepDataOption & 16) == 16)
                {
                    if (delReecords || manualApp)
                    {
                        NotifyInvalidRule();
                    }
                }
                //move
                if (keepDataOption == 0 && (odRule.MoveToRecordCenterSettings != null &&
                    !string.IsNullOrEmpty(odRule.MoveToRecordCenterSettings.DestinationLocation.Url)
                    && !string.IsNullOrEmpty(odRule.MoveToRecordCenterSettings.DestinationLocation.Password)
                    && !string.IsNullOrEmpty(odRule.MoveToRecordCenterSettings.DestinationLocation.UserName)))
                {
                    if (delReecords || manualApp || export)
                    {
                        NotifyInvalidRule();
                    }
                }

                if (odRule.EnableExport && odRule.ExportInfo != null)
                {
                    var exportTypeWhiteList = new List<GCommon.Contract.StorageOptimization.Object.ExportTypeValue> {
                        GCommon.Contract.StorageOptimization.Object.ExportTypeValue.NAA,
                        GCommon.Contract.StorageOptimization.Object.ExportTypeValue.NARA,
                        GCommon.Contract.StorageOptimization.Object.ExportTypeValue.VEO
                    };
                    if (!exportTypeWhiteList.Contains(odRule.ExportInfo.exportType))
                    {
                        NotifyInvalidRule();
                    }

                    bool NAAOrNARA = odRule.ExportInfo.exportType == AvePoint.GCommon.Contract.StorageOptimization.Object.ExportTypeValue.NAA || odRule.ExportInfo.exportType == AvePoint.GCommon.Contract.StorageOptimization.Object.ExportTypeValue.NARA;
                    if (NAAOrNARA && odRule.RuleLevel != PolicyLevel.Document && odRule.RuleLevel != PolicyLevel.Folder)
                    {
                        NotifyInvalidRule();
                    }

                    ValidateExportInfoData(odRule);
                }
                await CheckStoragePolicyAsync(odRule);
                CheckStubTemplate(odRule);
                await CheckRetentionAsync(odRule);
                CheckManualRule(odRule);
                CheckRuleFilters(odRule.RuleFilters, RMRuleSourceType.OneDrive, rule.ModelType);
            }
        }

        private async System.Threading.Tasks.Task CheckRetentionAsync(RMRuleInfos rule)
        {
            if (rule.IsEnableRetention)
            {
                var retention = rule.RetentionInfo;
                if (retention.ColumnName != "Archived Time" ||
                    (retention.Condition != TimeFilterCondition.OlderThan && retention.Condition != TimeFilterCondition.Is))
                {
                    NotifyInvalidRule();
                }
                int[] availableUnit = new int[] { 1, 2, 3, 4 };
                if ((retention.Condition == TimeFilterCondition.OlderThan && (retention.KeepDateNumber < 0 || !availableUnit.Contains((int)retention.KeepDateUnite))) ||
                    (retention.Condition == TimeFilterCondition.Is && string.IsNullOrEmpty(retention.Date)))
                {
                    NotifyInvalidRule();
                }
                if (retention.Condition == TimeFilterCondition.Is)
                {
                    var date0 = await ConvertTime4BackendAsync(retention.Date);
                    if (date0 > DateTime.UtcNow.Ticks)
                    {
                        throw new Exception(I18NEntity.GetString("RM_JS_RDM_Rule_InvalidArchiveDate"));
                    }
                }

                if (retention.IsManualApproval)
                {
                    switch (retention.ReviewType)
                    {
                        case ReviewType.RecordOwner:
                            if (retention.IsSendEamilToOwner)
                            {
                                var recordOwners = retention.UserInfos;
                                if (recordOwners == null || recordOwners.Count == 0)
                                {
                                    //勾选发送邮件，RecordOwner不能为空
                                    NotifyInvalidRule();
                                }
                                else
                                {
                                    //Check recordOwner中信息是否正确
                                    if (recordOwners.Any(o =>
                                    (string.IsNullOrEmpty(o.DisplayName) && string.IsNullOrEmpty(o.UserPrincipalName))
                                    //当user没在aos注册时，id有值，userId为空
                                    //注册过user，userId有值，id为空
                                    || (string.IsNullOrEmpty(o.Id) && string.IsNullOrEmpty(o.UserId))))
                                    {
                                        logger.Error($"Rule: [{rule.RuleName}] record owners information in retention settings is incorrect.");
                                        NotifyInvalidRule();
                                    }
                                }
                            }
                            break;
                        case ReviewType.Workflow:
                            if (!Guid.TryParse(retention.WorkflowId, out Guid workflowId))
                            {
                                NotifyInvalidRule();
                            }
                            break;
                        default:
                            NotifyInvalidRule();
                            break;
                    }
                }
            }
        }

        /// <summary>
        /// Check manual Rule 
        /// </summary>
        /// <param name="rule"></param>
        private void CheckManualRule(RMRuleInfos rule)
        {
            if (rule.EnableManualApproval)
            {
                switch (rule.ManualReviewType)
                {
                    case ReviewType.RecordOwner:
                        if (rule.IsSendEmailToOwner)
                        {
                            var recordOwners = rule.Users;
                            if (recordOwners == null || recordOwners.Count == 0)
                            {
                                //勾选发送邮件，RecordOwner不能为空
                                NotifyInvalidRule();
                            }
                            else
                            {
                                //Check recordOwner中信息是否正确
                                if (recordOwners.Any(o =>
                                (string.IsNullOrEmpty(o.DisplayName) && string.IsNullOrEmpty(o.UserPrincipalName))
                                //当user没在aos注册时，id有值，userId为空
                                //注册过user，userId有值，id为空
                                || (string.IsNullOrEmpty(o.Id) && string.IsNullOrEmpty(o.UserId))))
                                {
                                    logger.Error($"Rule: [{rule.RuleName}] record owners information is incorrect.");
                                    NotifyInvalidRule();
                                }
                            }
                        }
                        break;
                    case ReviewType.Workflow:
                        if (!Guid.TryParse(rule.WorkflowId, out Guid workflowId))
                        {
                            NotifyInvalidRule();
                        }
                        break;
                    default:
                        NotifyInvalidRule();
                        break;
                }
            }
        }
        private void CheckMoveSPMetadata(MoveToDto dto)
        {
            List<string> exoColumnList = ExoColumnNameList();
            if (dto!=null)
            {
                if (dto.IsMoveToSP)
                {
                    if (dto.MoveToSPDataList == null
                        || dto.MoveToSPDataList.Count==0 
                        || dto.MoveToSPDataList.Count>50 
                        || dto.MoveToSPDataList.Where(m=>string.IsNullOrWhiteSpace(m.SPColumn) || m.SPColumn.Length > 255).ToList().Count>0
                        || dto.MoveToSPDataList.Where(m=> string.IsNullOrWhiteSpace(m.ExoColumn)).ToList().Count > 0
                        || dto.MoveToSPDataList.Where(m=> !exoColumnList.Contains(m.ExoColumn)).ToList().Count>0
                        )
                    {
                        NotifyInvalidRule();
                    }
                }
            }
        }
        private List<string> ExoColumnNameList()
        {
            return new List<string>() {
                "Subject",
                "Received",
                "From",
                "To",
                "Size",
                "Conversation",
                "Created",
                "Due Date",
                "Flag Completed Date",
                "Flag Status",
                "Importance",
                "Received Representing Name",
                "Recipient Name",
                "Sensitivity",
                "Sent",
                "Start Date",
                "Cc",
                "Email Account"
            };
        }
        private void CheckRuleFilters(List<RuleFilter> filters, RMRuleSourceType source, RuleModel mode = RuleModel.Records)
        {
            List<ArchiverFilterRuleType> listFilterRuleTypeHasValue1Empty = new List<ArchiverFilterRuleType>() { ArchiverFilterRuleType.Privacy, ArchiverFilterRuleType.TeamsStatus, ArchiverFilterRuleType.TeamType};
            var invalid = false;
            if (filters == null || filters.Count == 0 || filters.Any(f => string.IsNullOrWhiteSpace(f.Value1) && f.Condition != ArchiverFilterCondition.IsEmpty && !listFilterRuleTypeHasValue1Empty.Contains(f.RuleType)))
            {
                invalid = true;
            }
            else
            {
                if (source == RMRuleSourceType.GoogleDrive && filters.Any(o => (o.Value2Unit == PolicyValueUnit.Weeks || o.Value2Unit == PolicyValueUnit.Months || o.Value2Unit == PolicyValueUnit.Years)
                 && int.Parse(o.Value2) <= 0))
                {
                    NotifyInvalidRule();
                }
                
                if (source != RMRuleSourceType.GoogleDrive && filters.Any(o => (o.Value1Unit == PolicyValueUnit.Weeks || o.Value1Unit == PolicyValueUnit.Months || o.Value1Unit == PolicyValueUnit.Years)
                 && int.Parse(o.Value1) <= 0))
                {
                    NotifyInvalidRule();
                }

                if(filters.Any(o => (o.RuleType == ArchiverFilterRuleType.Size || o.RuleType == ArchiverFilterRuleType.DocumentSize)
                 && int.Parse(o.Value1) <= 0))
                {
                    NotifyInvalidRule();
                }

                var ruleLevelWhiteList = new List<PolicyLevel>();
                if (source == RMRuleSourceType.SP)
                {
                    if (mode == RuleModel.SOArchiver)
                    {
                        ruleLevelWhiteList = new List<PolicyLevel> { PolicyLevel.SiteCollection, PolicyLevel.Site, PolicyLevel.List, PolicyLevel.Folder, PolicyLevel.Item, PolicyLevel.Document, PolicyLevel.DocumentVersion, PolicyLevel.ItemVersion, PolicyLevel.Attachment };
                    }
                    else
                    {
                        ruleLevelWhiteList = new List<PolicyLevel> { PolicyLevel.SiteCollection, PolicyLevel.Site, PolicyLevel.List, PolicyLevel.Folder, PolicyLevel.Item, PolicyLevel.Document };
                    }
                }
                if (source == RMRuleSourceType.FS)
                {
                    ruleLevelWhiteList = new List<PolicyLevel> { PolicyLevel.FileSysFile };
                }
                if (source == RMRuleSourceType.EXO)
                {
                    ruleLevelWhiteList = new List<PolicyLevel> { PolicyLevel.ExchangeOnlineItem_Message };
                    var retentionLabelCriteria = filters.Where(f => f.RuleType == ArchiverFilterRuleType.RetentionLabel || f.RuleType == ArchiverFilterRuleType.SensitivityLabel).ToList();
                    if (retentionLabelCriteria != null && retentionLabelCriteria.Any(c => c.Condition != (ArchiverFilterCondition)262936 && c.Condition != ArchiverFilterCondition.IsEmpty && c.Condition != ArchiverFilterCondition.DoesNotEqual))
                    {
                        NotifyInvalidRule();
                    }
                }
                if (source == RMRuleSourceType.Physical)
                {
                    ruleLevelWhiteList = new List<PolicyLevel> { PolicyLevel.Document, PolicyLevel.Folder, PolicyLevel.List };
                }
                if (source == RMRuleSourceType.SPLocal)
                {
                    ruleLevelWhiteList = new List<PolicyLevel> { PolicyLevel.Document, PolicyLevel.Item };
                }
                if (source == RMRuleSourceType.OneDrive)
                {
                    if (mode == RuleModel.SOArchiver)
                    {
                        ruleLevelWhiteList = new List<PolicyLevel> { PolicyLevel.SiteCollection, PolicyLevel.Site, PolicyLevel.List, PolicyLevel.Folder, PolicyLevel.Item, PolicyLevel.Document, PolicyLevel.DocumentVersion, PolicyLevel.ItemVersion, PolicyLevel.Attachment };
                    }
                    else
                    {
                        ruleLevelWhiteList = new List<PolicyLevel> { PolicyLevel.Document };
                    }
                }
                if (source == RMRuleSourceType.Teams)
                {
                    if (mode == RuleModel.SOArchiver)
                    {
                        ruleLevelWhiteList = new List<PolicyLevel> { PolicyLevel.Document, PolicyLevel.DocumentVersion, PolicyLevel.Item, PolicyLevel.ItemVersion, PolicyLevel.Attachment, PolicyLevel.Folder, PolicyLevel.List, PolicyLevel.Site, PolicyLevel.Teams};
                    }
                    else
                    {
                        ruleLevelWhiteList = new List<PolicyLevel> { PolicyLevel.Document, PolicyLevel.Item, PolicyLevel.Folder, PolicyLevel.List, PolicyLevel.Site };
                    }
                }
                if (source == RMRuleSourceType.AzureFile)
                {
                    ruleLevelWhiteList = new List<PolicyLevel> { PolicyLevel.AzureFileDocument };
                }
                if (source == RMRuleSourceType.Box)
                {
                    ruleLevelWhiteList = new List<PolicyLevel> { PolicyLevel.BoxDocument };
                }
                if (source == RMRuleSourceType.Connector)
                {
                    ruleLevelWhiteList = new List<PolicyLevel> { PolicyLevel.Document };
                }
                if (source == RMRuleSourceType.GoogleDrive)
                {
                    ruleLevelWhiteList = new List<PolicyLevel> { PolicyLevel.GoogleDriveDocument };
                }
                invalid = filters.Any(o => !ruleLevelWhiteList.Contains(o.Level));
            }

            if (invalid)
            {
                NotifyInvalidRule();
            }
            if (!CheckFilterOfListInIsRight(filters, source))
            {
                NotifyInvalidRule();
            }
        }
        private async System.Threading.Tasks.Task CheckRuleNameAsync(RMRuleInfos rule, bool allowMissingExistingRule = false)
        {
            if (!string.IsNullOrEmpty(rule.RuleId))
            {
                if (allowMissingExistingRule && !RuleExistsInCurrentDataCenter(rule.RuleId))
                {
                    return;
                }

                var oldRule = await LoadRuleAsync(rule.RuleId);
                if (!oldRule.RuleName.Equals(rule.RuleName))
                {
                    throw new Exception(I18NEntity.GetString("RM_JS_RDM_EditRule_RuleNameChanged"));
                }
            }
        }

        private void CheckSameNameRule(RMRuleInfos rule)
        {
            if (RMRuleDao.IsExistRule(rule.RuleName, Guid.Parse(rule.RuleId)))
            {
                throw new Exception(I18NEntity.GetString("RM_JS_RDM_CreateRule_Validation_EqualCopyName"));
            }
        }

        private void NotifyInvalidRule()
        {
            throw new Exception("RM_JS_RDM_CreateRule_InvalidRuleInfo");
        }

        private bool RuleExistsInCurrentDataCenter(string ruleId)
        {
            if (!Guid.TryParse(ruleId, out var parsedRuleId))
            {
                return false;
            }

            return RMRuleDao.GetRuleById(parsedRuleId) != null;
        }

        [RACodeReview("Allen Yin")]
        [Audit(Module = AuditModule.RetentionAndDisposalManagement, Category = AuditCategory.RuleManagement, Action = AuditAction.EditRule, BeforeHandler = typeof(RuleManagerBeforeAuditHandler), AfterHandler = typeof(RuleManagerAfterAuditHandler))]
        public async Task<RAReturnMessage> ModifyRuleInDAAsync(RMRuleInfos rule)
        {
            if(!ValidRuleActionPemerssion(rule.RuleKeepDataOption | rule?.OneDriveRule?.RuleKeepDataOption ?? 0, out RAReturnMessage errorMessage))
            {
                return errorMessage;
            }
            await CheckRuleIsRightAsync(rule);
            var oldContainer = RMRuleDao.GetRuleContainersByRuleId(new Guid(rule.RuleId));
            if (oldContainer != null && oldContainer.ContainerId != rule.ContainerId)
            {
                var message = RMRuleDao.CheckContainerCrossSecurityGroup(oldContainer.ContainerId, rule.ContainerId, rule.RuleId);
                if (message.MessageType == RAMessageType.Failed)
                {
                    logger.Debug($"Need remove rule associate terms. rule id: {rule.RuleId}");
                    await RemoveRuleAssociationTermAsync(rule);
                }
            }
            try
            {
                logger.Info("Modify rule to DocAve Control from REC.");
                try
                {
                    using (var performance = new PerformanceScope($"modified rule:{rule?.RuleId}"))
                    {
                        var syncUserResult = await SyncADUsersAsync(rule);
                        if (syncUserResult.MessageType == RAMessageType.Failed)
                        {
                            return syncUserResult;
                        }
                        ValidRetentionKeepDateOption(rule);
                        //var client = new DAOAPIClientV1();
                        var buildRule = await BuildRMRuleAsync(rule);

                        if (await RealEditRuleAsync(buildRule))
                        {
                            var changes = TermRuleAssocition.GetTermUniqueIdsByRuleId(buildRule.Id).ToDictionary(id => id, _ => (int)TermChangeType.TermRule);
                            ChangeClassificationDao.AddChangeLabelsAndTerms(changes);
                            RMRuleDao.AddOrUpdateRMRule(new RMRule()
                            {
                                RuleId = new Guid(buildRule.Id),
                                RuleName = buildRule.Name,
                                DisposalAction = (int)RuleHelper.GetOperationTypeForSP(buildRule),
                                ExchangeDisposalAction = (int)RuleHelper.GetOperationTypeForEXO(buildRule.EXORule),
                                PhysicalDisposalAction = (int)RuleHelper.GetOperationTypeForPhysical(buildRule.PhysicalRule),
                                FSDisposalAction = (int)RuleHelper.GetOperationTypeForFS(buildRule.FSRule),
                                SPLocalDisposalAction = (int)RuleHelper.GetOperationTypeForSPLocal(buildRule.SPLocalRule),
                                OneDriveDisposalAction = (int)RuleHelper.GetOperationTypeForOneDrive(buildRule.OneDriveRule),
                                AzureFileDisposalAction = (int)RuleHelper.GetOperationTypeForAzureFile(buildRule.AzureFileRule),
                                ConnectorDisposalAction = (int)RuleHelper.GetOperationTypeForConnector(buildRule.ConnectorRule),
                                GoogleDriveDisposalAction = (int)RuleHelper.GetOperationTypeForGoogleDrive(buildRule.GoogleDriveRule),
                                TeamsDisposalAction = (int)RuleHelper.GetOperationTypeForTeams(buildRule.TeamsRule),
                                RuleLevel = (int)buildRule.PolicyLevel,
                                DeleteRecords = buildRule.DeleteRecords,
                                IsRemoved = false,
                                Description = buildRule.Description,
                                ModifyTime = buildRule.ModifyTime,
                                DisposalClass = buildRule.DisposalClass,
                                Extension = SerializerHelper.SerializeByDataContractJsonSerializer(buildRule),
                                ModelType = (int)rule.ModelType,
                            }, rule.ContainerId);
                            return new RAReturnMessage() { MessageType = RAMessageType.Successful };
                        }
                        else
                        {
                            return new RAReturnMessage() { MessageType = RAMessageType.Failed };
                        }
                    }
                }
                catch (EnableDataCollectionStatusException e)
                {
                    logger.Error($"An error occured while call insights api,rule name:{rule.RuleName},ERROR:{e}");
                    return new RAReturnMessage() { MessageType = RAMessageType.Failed, ErrorMessage = I18NEntity.GetString("RM_EnableDataCollectionSwitch_Error_Message") };
                }
                catch (Exception e)
                {
                    return new RAReturnMessage() { MessageType = RAMessageType.Failed, ErrorMessage = ParseErrorMessageFromDAOL(e.Message) };
                }
            }
            catch (Exception e)
            {
                logger.Error("an error occured while modify rule,rule name:{0},ERROR:{1}", rule.RuleName, e.ToString());
                throw new AveException(I18NEntity.GetString("RM_JS_JM_Summary_ConnectDAOFailed"));
            }

        }

        private bool IsPrePaidConsumptionLicense()
        {
            var client = AosApiUtility.GetAosModernClient(TenantLocalValue.LogonGroupId);
            var info = client.LicenseService.GetLicenseAsync(RecordsConstants.RECORDS_APPLICATION_NAME).GetAwaiter().GetResult();
            if (info.Type == Cloud.Sdk.Data.AosModern.LicenseType.Trial)
            {
                return false;
            }
            else
            {
                Cloud.Sdk.Data.AosModern.CloudRecordsExtension extension = info.Extension as Cloud.Sdk.Data.AosModern.CloudRecordsExtension;
                if (extension.SaleType == Cloud.Sdk.Data.AosModern.SaleType.PrePaidConsumption)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        private void CheckArchiveRuleHasStorageInfo(int ruleKeepDataOption, string storagePolicyId, string storagePolicyName)
        {
            if (ruleKeepDataOption == (int)KeepDataOption.Archive && (string.IsNullOrEmpty(storagePolicyId) || string.IsNullOrEmpty(storagePolicyName)))
            {
                NotifyInvalidRule();
            }
        }

        private async System.Threading.Tasks.Task RemoveRuleAssociationTermAsync(RMRuleInfos rule)
        {
            try
            {
                await TermRuleAssocition.BatchDeleteAsync(r => r.RuleId.ToString().Equals(rule.RuleId));
            }
            catch (Exception e)
            {
                logger.Warn($"Remove rule association term error: {e}");
            }
        }

        public Rule GetSpecifyTeamsArchiverBackupRule()
        {
            var storageDevice = StorageDeviceService.GetIndexDevice();
            Rule teamRule = new Rule
            {
                Name = "N/A",
                Id = RecordsConstants.FAKE_SPECIFY_TEAMS_RULE_ID,
                IncludeNew = "1",
                KeepDataOption = (int)KeepDataOption.ArchiveBackupAndRemove,
                ProfileType = AvePoint.GCommon.Contract.Server.Common.Profile.Object.ProfileType.ArchiverRuleForRevIM,
                PolicyLevel = PolicyLevel.Teams,
                AndOrExpression = new Dictionary<PolicyLevel, string>() { { PolicyLevel.Teams, "(1)" } },
                IncludeDeleteRecordLabel = true,
                Filters =
                        [
                            new FilterPolicy
                {
                    Condition = PolicyCondition.Match,
                    Level = PolicyLevel.Teams,
                    Rule = new UrlRule() { Value1 = "Display Name" },
                    Value = new PolicyValue(){Value1 = "*"},
                    SequenceNo = 1
                },
            ],
                SOFilters =
                    [
                            new ()
                {
                    IsAnd = true,
                    Condition = PolicyCondition.Match,
                    Level = PolicyLevel.Teams,
                    Rule = new UrlRule() { Value1 = "Display Name" },
                    Value = new PolicyValue(){Value1 = "*"},
                    SequenceNo = 1
                },
            ],
                TeamsRule = new Rule
                {
                    Name = "N/A",
                    IncludeDeleteRecordLabel = true,
                    Filters =
                        [
                            new FilterPolicy
                {
                    Condition = PolicyCondition.Match,
                    Level = PolicyLevel.Teams,
                    Rule = new UrlRule() { Value1 = "Display Name" },
                    Value = new PolicyValue(){Value1 = "*"},
                    SequenceNo = 1
                },
            ],
                    SOFilters =
                    [
                            new ()
                {
                    IsAnd = true,
                    Condition = PolicyCondition.Match,
                    Level = PolicyLevel.Teams,
                    Rule = new UrlRule() { Value1 = "Display Name" },
                    Value = new PolicyValue(){Value1 = "*"},
                    SequenceNo = 1
                },
            ],
                    PolicyLevel = PolicyLevel.Teams,
                    AndOrExpression = new Dictionary<PolicyLevel, string>() { { PolicyLevel.Teams, "(1)" } },
                    Order = 1,
                    ProfileType = AvePoint.GCommon.Contract.Server.Common.Profile.Object.ProfileType.ArchiverRuleForRevIM,
                    KeepDataOption = (int)KeepDataOption.ArchiveBackupAndRemove,
                    IncludeNew = "1",
                    StoragePolicyId = storageDevice.Id,
                }
            };
            return teamRule;
        }

        public List<Rule> GetRulesByIds(List<Guid> ids)
        {
            var rules = RMRuleDao.GetRulesByIds(ids);
            List<Rule> resultRules = new List<Rule>();
            foreach (var rule in rules)
            {
                if (!string.IsNullOrEmpty(rule.Extension))
                {
                    Rule soRule = SerializerHelper.DeserializeByDataContractJsonSerializer<Rule>(rule.Extension);
                    if (soRule.SOFilters != null)
                    {
                        soRule.Filters = ConvertSOFilterPolicysToFilterPolicys(soRule.SOFilters);
                    }
                    if (soRule.EXORule != null && soRule.EXORule.SOFilters != null)
                    {
                        soRule.EXORule.Filters = ConvertSOFilterPolicysToFilterPolicys(soRule.EXORule.SOFilters);
                    }
                    if (soRule.FSRule != null && soRule.FSRule.SOFilters != null)
                    {
                        soRule.FSRule.Filters = ConvertSOFilterPolicysToFilterPolicys(soRule.FSRule.SOFilters);
                    }
                    if (soRule.OneDriveRule != null && soRule.OneDriveRule.SOFilters != null)
                    {
                        soRule.OneDriveRule.Filters = ConvertSOFilterPolicysToFilterPolicys(soRule.OneDriveRule.SOFilters);
                    }
                    if (soRule.PhysicalRule != null && soRule.PhysicalRule.SOFilters != null)
                    {
                        soRule.PhysicalRule.Filters = ConvertSOFilterPolicysToFilterPolicys(soRule.PhysicalRule.SOFilters);
                    }
                    if (soRule.SPLocalRule != null && soRule.SPLocalRule.SOFilters != null)
                    {
                        soRule.SPLocalRule.Filters = ConvertSOFilterPolicysToFilterPolicys(soRule.SPLocalRule.SOFilters);
                    }
                    if (soRule.AzureFileRule != null && soRule.AzureFileRule.SOFilters != null)
                    {
                        soRule.AzureFileRule.Filters = ConvertSOFilterPolicysToFilterPolicys(soRule.AzureFileRule.SOFilters);
                    }
                    if (soRule.BoxRule != null && soRule.BoxRule.SOFilters != null)
                    {
                        soRule.BoxRule.Filters = ConvertSOFilterPolicysToFilterPolicys(soRule.BoxRule.SOFilters);
                    }
                    if (soRule.ConnectorRule != null && soRule.ConnectorRule.SOFilters != null)
                    {
                        soRule.ConnectorRule.Filters = ConvertSOFilterPolicysToFilterPolicys(soRule.ConnectorRule.SOFilters);
                    }
                    if (soRule.GoogleDriveRule != null && soRule.GoogleDriveRule.SOFilters != null)
                    {
                        soRule.GoogleDriveRule.Filters = ConvertSOFilterPolicysToFilterPolicys(soRule.GoogleDriveRule.SOFilters);
                    }

                    resultRules.Add(soRule);
                }
            }
            return resultRules;
        }

        public List<Rule> GetRulesFromRecords()
        {
            var rules = RMRuleDao.GetAvailableRules();
            List<Rule> resultRules = new List<Rule>();
            foreach (var rule in rules)
            {
                if (!string.IsNullOrEmpty(rule.Extension))
                {
                    Rule soRule = SerializerHelper.DeserializeByDataContractJsonSerializer<Rule>(rule.Extension);
                    if (soRule.SOFilters != null)
                    {
                        soRule.Filters = ConvertSOFilterPolicysToFilterPolicys(soRule.SOFilters);
                        if (AccountUtility.IsSupportRecordLabel())
                        {
                            ChangeTagContentAfterSupportRecordLabel(soRule.TagContentInfo, soRule.PolicyLevel, soRule.KeepDataOption);
                            if ((soRule.KeepDataOption & (int)KeepDataStatus.DeclareRecord) == (int)KeepDataStatus.DeclareRecord)
                            {
                                soRule.KeepDataOption -= (int)KeepDataStatus.DeclareRecord;
                                if ((soRule.KeepDataOption & (int)KeepDataStatus.TagContent) != (int)KeepDataStatus.TagContent)
                                {
                                    soRule.KeepDataOption += (int)KeepDataOption.TagContent;
                                }
                            }
                        }
                        
                    }
                    if (soRule.EXORule != null && soRule.EXORule.SOFilters != null)
                    {
                        soRule.EXORule.Filters = ConvertSOFilterPolicysToFilterPolicys(soRule.EXORule.SOFilters);
                    }
                    if (soRule.FSRule != null && soRule.FSRule.SOFilters != null)
                    {
                        soRule.FSRule.Filters = ConvertSOFilterPolicysToFilterPolicys(soRule.FSRule.SOFilters);
                    }
                    if (soRule.OneDriveRule != null && soRule.OneDriveRule.SOFilters != null)
                    {
                        soRule.OneDriveRule.Filters = ConvertSOFilterPolicysToFilterPolicys(soRule.OneDriveRule.SOFilters);
                        if (AccountUtility.IsSupportRecordLabel())
                        {
                            ChangeTagContentAfterSupportRecordLabel(soRule.OneDriveRule.TagContentInfo, soRule.PolicyLevel, soRule.OneDriveRule.KeepDataOption);
                            if ((soRule.OneDriveRule.KeepDataOption & (int)KeepDataStatus.DeclareRecord) == (int)KeepDataStatus.DeclareRecord)
                            {
                                soRule.OneDriveRule.KeepDataOption -= (int)KeepDataStatus.DeclareRecord;
                                if ((soRule.OneDriveRule.KeepDataOption & (int)KeepDataStatus.TagContent) != (int)KeepDataStatus.TagContent)
                                {
                                    soRule.OneDriveRule.KeepDataOption += (int)KeepDataOption.TagContent;
                                }
                            }
                        }
                    }
                    if (soRule.PhysicalRule != null && soRule.PhysicalRule.SOFilters != null)
                    {
                        soRule.PhysicalRule.Filters = ConvertSOFilterPolicysToFilterPolicys(soRule.PhysicalRule.SOFilters);
                    }
                    if (soRule.SPLocalRule != null && soRule.SPLocalRule.SOFilters != null)
                    {
                        soRule.SPLocalRule.Filters = ConvertSOFilterPolicysToFilterPolicys(soRule.SPLocalRule.SOFilters);
                    }
                    if (soRule.AzureFileRule != null && soRule.AzureFileRule.SOFilters != null)
                    {
                        soRule.AzureFileRule.Filters = ConvertSOFilterPolicysToFilterPolicys(soRule.AzureFileRule.SOFilters);
                    }
                    if (soRule.BoxRule != null && soRule.BoxRule.SOFilters != null)
                    {
                        soRule.BoxRule.Filters = ConvertSOFilterPolicysToFilterPolicys(soRule.BoxRule.SOFilters);
                    }
                    if (soRule.GoogleDriveRule != null && soRule.GoogleDriveRule.SOFilters != null)
                    {
                        soRule.GoogleDriveRule.Filters = ConvertSOFilterPolicysToFilterPolicys(soRule.GoogleDriveRule.SOFilters);
                    }
                    if (soRule.ConnectorRule != null && soRule.ConnectorRule.SOFilters != null)
                    {
                        soRule.ConnectorRule.Filters = ConvertSOFilterPolicysToFilterPolicys(soRule.ConnectorRule.SOFilters);
                    }
                    if(soRule.TeamsRule != null && soRule.TeamsRule.SOFilters != null)
                    {
                        soRule.TeamsRule.Filters = ConvertSOFilterPolicysToFilterPolicys(soRule.TeamsRule.SOFilters);
                    }
                    soRule.ModifyTime = rule.ModifyTime;
                    resultRules.Add(soRule);
                }
            }
            return resultRules;
        }

        private void ChangeTagContentAfterSupportRecordLabel(List<TagContentInfo> tagContentInfo, PolicyLevel policyLevel, int keepDataOption)
        {
            if (tagContentInfo == null || tagContentInfo.Count == 0)
            {
                if ((policyLevel == PolicyLevel.Document || policyLevel == PolicyLevel.Item) && (keepDataOption & (int)KeepDataOption.DeclareRecord) == (int)KeepDataOption.DeclareRecord)
                {
                    tagContentInfo = new List<TagContentInfo>()
                    {
                        new TagContentInfo()
                        {
                            Type = TagContentInfoType.RetentionLabel,
                            Option = (int)RetentionLabelOptions.GetFromGeneralSetting
                        }
                    };
                }
                return;
            }
            if ((policyLevel == PolicyLevel.Document || policyLevel == PolicyLevel.Item))
            {
                var hasRetentionLabelTagContent = tagContentInfo.Any(_ => _.Type == TagContentInfoType.RetentionLabel);
                if (!hasRetentionLabelTagContent && (keepDataOption & (int)KeepDataOption.DeclareRecord) == (int)KeepDataOption.DeclareRecord)
                {
                    tagContentInfo.Add(new TagContentInfo()
                    {
                        Type = TagContentInfoType.RetentionLabel,
                        Option = (int)RetentionLabelOptions.GetFromGeneralSetting
                    });
                }
            }
        }

        public List<Rule> GetFSRulesFromRecords()
        {
            var rules = RMRuleDao.GetAvailableFSRules();
            List<Rule> resultRules = new List<Rule>();
            foreach (var rule in rules)
            {
                if (!string.IsNullOrEmpty(rule.Extension))
                {
                    Rule soRule = SerializerHelper.DeserializeByDataContractJsonSerializer<Rule>(rule.Extension);
                    if (soRule.SOFilters != null)
                    {
                        soRule.Filters = ConvertSOFilterPolicysToFilterPolicys(soRule.SOFilters);
                    }
                    if (soRule.EXORule != null && soRule.EXORule.SOFilters != null)
                    {
                        soRule.EXORule.Filters = ConvertSOFilterPolicysToFilterPolicys(soRule.EXORule.SOFilters);
                    }
                    if (soRule.FSRule != null && soRule.FSRule.SOFilters != null)
                    {
                        soRule.FSRule.Filters = ConvertSOFilterPolicysToFilterPolicys(soRule.FSRule.SOFilters);
                    }
                    if (soRule.OneDriveRule != null && soRule.OneDriveRule.SOFilters != null)
                    {
                        soRule.OneDriveRule.Filters = ConvertSOFilterPolicysToFilterPolicys(soRule.OneDriveRule.SOFilters);
                    }
                    if (soRule.PhysicalRule != null && soRule.PhysicalRule.SOFilters != null)
                    {
                        soRule.PhysicalRule.Filters = ConvertSOFilterPolicysToFilterPolicys(soRule.PhysicalRule.SOFilters);
                    }
                    if (soRule.SPLocalRule != null && soRule.SPLocalRule.SOFilters != null)
                    {
                        soRule.SPLocalRule.Filters = ConvertSOFilterPolicysToFilterPolicys(soRule.SPLocalRule.SOFilters);
                    }
                    if (soRule.AzureFileRule != null && soRule.AzureFileRule.SOFilters != null)
                    {
                        soRule.AzureFileRule.Filters = ConvertSOFilterPolicysToFilterPolicys(soRule.AzureFileRule.SOFilters);
                    }
                    if (soRule.BoxRule != null && soRule.BoxRule.SOFilters != null)
                    {
                        soRule.BoxRule.Filters = ConvertSOFilterPolicysToFilterPolicys(soRule.BoxRule.SOFilters);
                    }
                    if (soRule.GoogleDriveRule != null && soRule.GoogleDriveRule.SOFilters != null)
                    {
                        soRule.GoogleDriveRule.Filters = ConvertSOFilterPolicysToFilterPolicys(soRule.GoogleDriveRule.SOFilters);
                    }
                    if (soRule.ConnectorRule != null && soRule.ConnectorRule.SOFilters != null)
                    {
                        soRule.ConnectorRule.Filters = ConvertSOFilterPolicysToFilterPolicys(soRule.ConnectorRule.SOFilters);
                    }
                    resultRules.Add(soRule);
                }
            }
            return resultRules;
        }


        public List<AvePoint.GCommon.Contract.CommonFilter.FilterPolicy> ConvertSOFilterPolicysToFilterPolicys(List<SOFilterPolicy> soFilterPolicys)
        {
            List<AvePoint.GCommon.Contract.CommonFilter.FilterPolicy> result = new List<AvePoint.GCommon.Contract.CommonFilter.FilterPolicy>();
            if (soFilterPolicys != null && soFilterPolicys.Count > 0)
            {
                foreach (SOFilterPolicy soFilterPolicy in soFilterPolicys)
                {
                    result.Add(ConvertSOFilterPolicyToFilterPolicy(soFilterPolicy));
                }
            }
            return result;
        }
        public AvePoint.GCommon.Contract.CommonFilter.FilterPolicy ConvertSOFilterPolicyToFilterPolicy(SOFilterPolicy soFilterPolicy)
        {
            if (soFilterPolicy == null)
            {
                return null;
            }
            AvePoint.GCommon.Contract.CommonFilter.FilterPolicy result = new AvePoint.GCommon.Contract.CommonFilter.FilterPolicy();
            result.Rule = soFilterPolicy.Rule;
            result.Condition = soFilterPolicy.Condition;
            result.Value = soFilterPolicy.Value;
            result.Level = soFilterPolicy.Level;
            result.SequenceNo = soFilterPolicy.SequenceNo;
            return result;
        }
        [RACodeReview("Allen yin")]
        public async Task<List<RMRuleInfos>> GetRuleInfosFromDAAsync()
        {
            List<Rule> rules = await GetRulesFromDAAsync();
            rules = rules.OrderByDescending(rule => rule.ModifyTime).ToList();
            List<RMRuleInfos> ruleInfos = new List<RMRuleInfos>();
            foreach (var rule in rules)
            {
                try
                {
                    ruleInfos.Add(await ConvertToRuleInfoAsync(rule));
                }
                catch (Exception e)
                {
                    logger.Warn("ConvertToRuleInfo error :{0}", e.ToString());
                }
            }
            return ruleInfos;
        }

        public async Task<List<RMRuleInfos>> GetRuleInfosFromRecordsAsync()
        {
            List<Rule> rules = GetRulesFromRecords();
            var ruleContainerNameDic = RMRuleDao.GetRuleContainerNameMemberships(rules.Select(r => new Guid(r.Id)).ToList());
            rules = rules.OrderByDescending(rule => rule.ModifyTime).ToList();
            List<RMRuleInfos> ruleInfos = new List<RMRuleInfos>();
            foreach (var rule in rules)
            {
                try
                {
                    var ruleInfo = await ConvertToRuleInfoAsync(rule);
                    ruleInfo.ContainerName = ruleContainerNameDic.ContainsKey(new Guid(ruleInfo.RuleId)) ?
                        ruleContainerNameDic[new Guid(ruleInfo.RuleId)] : "";
                    ruleInfos.Add(ruleInfo);
                }
                catch (Exception e)
                {
                    logger.Warn("ConvertToRuleInfo error :{0}", e.ToString());
                }
            }
            ruleInfos = ruleInfos.OrderBy(rule => rule.ContainerName).ToList();
            return ruleInfos;
        }


        public async Task<Rule> BuildRMRuleAsync(RMRuleInfos info)
        {
            List<SOFilterPolicy> soFilters = new List<SOFilterPolicy>();
            if (info.IsSpSource)
            {
                foreach (var filter in info.RuleFilters)
                {
                    ArchiverRuleFilter arFilter = new ArchiverRuleFilter();
                    arFilter.CombineMode = filter.CombineMode;
                    arFilter.SequenceNo = filter.SequenceNo;
                    arFilter.Level = filter.Level;
                    arFilter.Condition = filter.Condition;
                    arFilter.RuleType = filter.RuleType;
                    if (!string.IsNullOrEmpty(filter.filterName))
                    {
                        arFilter.RuleName = filter.filterName;
                    }
                    if (!TenantService.IsNewOpusTenant() && (filter.RuleType == ArchiverFilterRuleType.RetentionLabel || filter.RuleType == ArchiverFilterRuleType.SensitivityLabel || filter.RuleType == ArchiverFilterRuleType.SensitivityLabelFullName))
                    {
                        throw new Exception("Doesn't support Retention/Sensitivity Label for old logic account.");
                    }
                    //arFilter.Dto.Rule = arFilter.RuleBase;
                    if (arFilter.RuleType == ArchiverFilterRuleType.ModifiedTime ||
                        arFilter.RuleType == ArchiverFilterRuleType.CreatedTime ||
                        arFilter.RuleType == ArchiverFilterRuleType.LastAccessedTime ||
                        arFilter.RuleType == ArchiverFilterRuleType.LastActiveTime ||
                        arFilter.RuleType == ArchiverFilterRuleType.DateTimeColumn ||
                        arFilter.RuleType == ArchiverFilterRuleType.DateTimeCustomProperty ||
                        arFilter.RuleType == ArchiverFilterRuleType.DocumentModified ||
                        arFilter.RuleType == ArchiverFilterRuleType.ParentLibraryDateTime ||
                        arFilter.RuleType == ArchiverFilterRuleType.ParentSiteCollectionDateTime ||
                        arFilter.RuleType == ArchiverFilterRuleType.PropertyBagDateTime ||
                        arFilter.RuleType == ArchiverFilterRuleType.LastestSubfolderDisposalDate
                        )
                    {
                        string startDayLightSaving = filter.StartTimeInfo == null ? "true" : filter.StartTimeInfo.IsDayLightSaving.ToString();
                        string endDayLightSaving = filter.EndTimeInfo == null ? "true" : filter.EndTimeInfo.IsDayLightSaving.ToString();
                        if (arFilter.Condition == ArchiverFilterCondition.FromTo)
                        {

                            DateTime startUtcTime = arFilter.SetDateTime(filter.Value1, filter.StartTimeInfo.TimeZoneId, startDayLightSaving, false);
                            DateTime endUtcTime = arFilter.SetDateTime(filter.Value2, filter.EndTimeInfo.TimeZoneId, endDayLightSaving, true);
                            if (DateTime.Parse(filter.Value1) >= DateTime.Parse(filter.Value2))
                            {
                                //throw new InvalidArgumentException(Messages.Get("start_date_after_end_date"));
                                throw new Exception("");
                            }
                            arFilter.Value1 = startUtcTime.ToString(APIDateTimeFormat.DATETYPEForAPI003);
                            arFilter.Value2 = endUtcTime.ToString(APIDateTimeFormat.DATETYPEForAPI003);
                        }
                        else if (arFilter.Condition == ArchiverFilterCondition.Before)
                        {
                            // ValidateValueCount(value, 3);
                            DateTime utcTime = arFilter.SetDateTime(filter.Value1, filter.StartTimeInfo.TimeZoneId, startDayLightSaving, false);
                            arFilter.Value1 = utcTime.ToString(APIDateTimeFormat.DATETYPEForAPI003);
                        }
                        else if (arFilter.Condition == ArchiverFilterCondition.OlderThan)
                        {
                            //ValidateValueCount(value, 1);
                            //SetValueForOlderThan(value[0]);
                            arFilter.Value1 = filter.Value1;
                            arFilter.Value1Unit = filter.Value1Unit;
                        }
                        soFilters.Add(arFilter.Dto);
                    }
                    else if (arFilter.RuleType == ArchiverFilterRuleType.DateTimeLabelProperty)
                    {
                        string startDayLightSaving = filter.StartTimeInfo == null ? "true" : filter.StartTimeInfo.IsDayLightSaving.ToString();
                        string endDayLightSaving = filter.EndTimeInfo == null ? "true" : filter.EndTimeInfo.IsDayLightSaving.ToString();
                        if (arFilter.Condition == ArchiverFilterCondition.FromTo)
                        {

                            DateTime startUtcTime = arFilter.SetDateTime(filter.Value2, filter.StartTimeInfo.TimeZoneId, startDayLightSaving, false);
                            DateTime endUtcTime = arFilter.SetDateTime(filter.Value3, filter.EndTimeInfo.TimeZoneId, endDayLightSaving, true);
                            if (DateTime.Parse(filter.Value2) >= DateTime.Parse(filter.Value3))
                            {
                                //throw new InvalidArgumentException(Messages.Get("start_date_after_end_date"));
                                throw new Exception("");
                            }
                            arFilter.Value2 = startUtcTime.ToString(APIDateTimeFormat.DATETYPEForAPI003);
                            arFilter.Value3 = endUtcTime.ToString(APIDateTimeFormat.DATETYPEForAPI003);
                        }
                        else if (arFilter.Condition == ArchiverFilterCondition.Before)
                        {
                            // ValidateValueCount(value, 3);
                            DateTime utcTime = arFilter.SetDateTime(filter.Value2, filter.StartTimeInfo.TimeZoneId, startDayLightSaving, false);
                            arFilter.Value2 = utcTime.ToString(APIDateTimeFormat.DATETYPEForAPI003);
                        }
                        else if (arFilter.Condition == ArchiverFilterCondition.OlderThan)
                        {
                            //ValidateValueCount(value, 1);
                            //SetValueForOlderThan(value[0]);
                            arFilter.Value2 = filter.Value2;
                            arFilter.Value2Unit = filter.Value2Unit;
                        }
                        soFilters.Add(arFilter.Dto);
                    }
                    else
                    {
                        arFilter.Value1 = filter.Value1;
                        if (filter.RuleType == ArchiverFilterRuleType.DocumentSize || filter.RuleType == ArchiverFilterRuleType.SiteCollectionSizeTrigger)
                        {
                            arFilter.Value1Unit = filter.Value1Unit;
                            arFilter.Value2Unit = filter.Value2Unit;
                        }
                        arFilter.Value2 = filter.Value2;
                        soFilters.Add(arFilter.Dto);
                    }
                    if (arFilter.RuleType == ArchiverFilterRuleType.TextColumn && arFilter.Condition == ArchiverFilterCondition.ListIn)
                    {
                        var inArray = arFilter.Value1.Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                        List<string> list = [];
                        foreach (var arrayItem in inArray)
                        {
                            if (!list.Any(i => i.Equals(arrayItem, StringComparison.OrdinalIgnoreCase)))
                            {
                                list.Add(arrayItem);
                            }
                        }
                        arFilter.Value1 = string.Join(";", list);
                    }
                }
                EnableInsightsDataCollection(info.RuleFilters);
            }

            Rule rule = new Rule();
            if (!string.IsNullOrEmpty(info.RuleId))
            {
                rule.Id = info.RuleId;
            }
            try
            {
                //rule.ModifyTime = mGeneralSettingService.ConvertDateTimeToUtc(DateTime.Parse(info.Modified)).Ticks;
                rule.ModifyTime = DateTime.UtcNow.Ticks;
            }
            catch (Exception)
            {
                throw new Exception("Please configure general settings first.");//TODO i18n
            }
            rule.Name = info.RuleName;
            rule.Description = info.Description;
            rule.DisposalClass = !string.IsNullOrEmpty(info.DisposalClass) ? info.DisposalClass : null;
            rule.SOFilters = soFilters;
            if (info.ModelType == RuleModel.SOArchiver)
            {
                rule.ProfileType = GCommon.Contract.Server.Common.Profile.Object.ProfileType.ArchiverRule;
            }
            else
            {
                rule.ProfileType = GCommon.Contract.Server.Common.Profile.Object.ProfileType.ArchiverRuleForRevIM;
            }
            //rule.ProfileType = GCommon.Contract.Server.Common.Profile.Object.ProfileType.ArchiverRuleForRevIM;
            rule.IncludeNew = "1";
            rule.DeleteRecords = info.DeleteRecords;
            rule.IncludeDeleteRecordLabel = info.IncludeDeleteRecordLabel;
            rule.LockRecordBeforeDestroy = info.LockRecordBeforeDestroy;
            rule.DeleteSiteCollectionToRecycleBin = info.IsDeleteSiteCollectionToRecycleBin();
            rule.DeleteToRecycleBin = info.DeleteToRecycleBin;
            rule.DeclareLinkFile = info.DeclareLinkFile;
            rule.PolicyLevel = info.RuleLevel;
            rule.StoragePolicyId = info.StoragePolicyId;
            rule.StoragePolicyName = info.StoragePolicyName;
            rule.StubTemplateId = info.StubTemplateId;
            rule.StubTemplateName = info.StubTemplateName;
            rule.MoveToArchiverTierWhenArchiving = false;
            rule.MoveToAnotherTierType = info.MoveToAnotherTierType;
            rule.ArchivedLatestVersion = info.ArchivedLatestVersion;
            rule.ArchiverOnlyLastestVersion = info.ArchiverOnlyLastestVersion;
            rule.KeepLatestMajorAndMinorVersion = info.KeepLatestMajorAndMinorVersion;
            rule.KeepLatestMajorAndMinorVersionAndArchiveOthers = info.KeepLatestMajorAndMinorVersionAndArchiveOthers;
            rule.IsCalculationDisposalDate = info.IsCalculationDisposalDate;
            if (info.MoveToRecordCenterSettings != null)
            {
                rule.MoveToRecordCenterAndDelareSetting = null;
            }

            #region init rm settings

            if (info.EnableExport && info.ExportInfo != null && info.ExportInfo.exportSPDataOption != ExportSPDataOption.None && !(info.ExportInfo.exportType == AvePoint.GCommon.Contract.StorageOptimization.Object.ExportTypeValue.None))
            {
                SetRuleExportLocationInfo(info);
                rule.ExportInfo = info.ExportInfo;
                rule.ExportType = info.ExportInfo.exportType;
                rule.ExportInfo.exportSPDataOption = info.ExportInfo.exportSPDataOption;
                rule.ExportDataBeforeArchiving = info.ExportDataBeforeArchiving;
                if (TenantService.IsNewOpusTenant())
                {
                    rule.ExportInfo.exportLocationId = info.ExportInfo.exportLocationId;
                    rule.ExportInfo.exportLocationName = info.ExportInfo.exportLocationName;
                    rule.ExportInfo.newOptionsOfExportInfo = true;
                    await ExportMoveSettingsAsync(rule, info);
                }
            }
            //}
            if (info.IsSpSource)
            {
                //rule.MoveToRecordCenterAndDelareSetting = info.MoveToRecordCenterSettings;

                rule.KeepDataOption = info.RuleKeepDataOption;
                rule.RelatedRecordOption = info.RelatedRecordOption;
                //需要对tag中datetime类型数据做处理.每个rule最多4个tag
                rule.TagContentInfo = new List<TagContentInfo>();
                if (info.TagContentInfo != null)
                {
                    foreach (RMTagContentInfo tag in info.TagContentInfo)
                    {
                        if (tag.Type == TagContentInfoType.DateTime)
                        {
                            DateTime dt = DateTime.Parse(tag.Value);
                            tag.DateTime = DateTimeUtil.ConvertTimeToUtcDate(dt, GeneralSettingConfig.FindSystemTimeZoneById(tag.TimeZoneId), !tag.IsDayLightSaving);
                            //tag.Value = dt.ToString(JSDateTimeFormat.DEFAULT_TIME_FORMAT, CultureInfo.InvariantCulture);
                        }
                        rule.TagContentInfo.Add(new TagContentInfo()
                        {
                            ColumnName = tag.ColumnName,
                            DateTime = tag.DateTime,
                            Type = tag.Type,
                            Value = tag.Type == TagContentInfoType.DateTime ? string.Format("{0}/{1}", tag.TimeZoneId, tag.IsDayLightSaving) : tag.Value,
                            Option = tag.Option
                        });
                    }
                }
                if (info.RetentionInfo != null)
                {
                    await CheckUserInfoIdIsNullAndSetItAsync(info.RetentionInfo.UserInfos);
                }
                if ((rule.KeepDataOption & (int)KeepDataStatus.Archive) == (int)KeepDataStatus.Archive || (rule.KeepDataOption & (int)KeepDataStatus.ArchiveAndLeaveStub) == (int)KeepDataStatus.ArchiveAndLeaveStub)
                {
                    rule.IsEnableRetention = info.IsEnableRetention;
                    rule.RetentionInfo = await this.ConvertRetentionSettingAsync(info.RetentionInfo);
                    rule.IsEnableStoreContentRetention = false;
                    rule.StoreContentRetentionInfos = new();
                }
                else
                {
                    rule.IsEnableRetention = false;
                    rule.RetentionInfo = new();
                    rule.IsEnableStoreContentRetention = info.RetentionInfoList!=null?true:info.RetentionInfo!=null;
                    if (info.RetentionInfo != null)
                    {
                        info.RetentionInfo.IsEnableRetention = true;
                    }
                    rule.StoreContentRetentionInfos = ConvertStoreContentRetentionSetting(info.RetentionInfoList ?? new List<RetentionSettings>() { info.RetentionInfo });
                }
                //rule.TagContentInfo = info.TagContentInfo;
                SetLeaveStubMessage(rule, info);
                InitSORuleManualApprovalInfo(rule, info);
            }
            await ResetMoveSettingsAsync(rule, info);
            ResetSOFilter(rule);
            rule.EXORule = info.EXORule != null ? BuildRMEXORule(info.EXORule) : null;
            rule.PhysicalRule = info.PhysicalRule != null ? BuildRMPhysicalRule(info.PhysicalRule) : null;
            rule.FSRule = info.FSRule != null ? BuildRMFSRule(info.FSRule) : null;
            rule.SPLocalRule = info.SPLocalRule != null ? BuildRMSPLocalRule(info.SPLocalRule) : null;
            rule.OneDriveRule = info.OneDriveRule != null ? await BuildRMOneDriveRuleAsync(info.OneDriveRule) : null;
            rule.AzureFileRule = info.AzureFileRule != null ? BuildRMAzureFileRule(info.AzureFileRule) : null;
            rule.BoxRule = info.BoxRule != null ? BuildRMBoxRule(info.BoxRule) : null;
            rule.ConnectorRule = info.ConnectorRule != null ? BuildRMConnectorRule(info.ConnectorRule) : null;
            rule.GoogleDriveRule = info.GoogleDriveRule != null ? await BuildRMGoogleDriveRule(info.GoogleDriveRule) : null;
            rule.TeamsRule = info.TeamsRule != null ? await BuildRMTeamsRuleAsync(info.TeamsRule) : null;
            //TO DO FS ??????????? FS Move settings why not put move settings logic to Build FS Rule or Phy move 
            await ResetPhyMoveSettingsAsync(rule, info);
            ResetFSMoveSettings(rule, info);
            await ResetEXOMoveSettingsAsync(rule, info);
            //ResetSPLocalMoveSettings(rule, info); //暂时不支持Move
            await ResetOneDriveMoveSettingsAsync(rule, info);
            //ResetAzureFileMoveSettings(rule, info); //暂时不支持Move
            ResetGoogleMoveSettingsAsync(rule, info);
            #endregion
            return rule;
        }
        private async Task CheckUserInfoIdIsNullAndSetItAsync(List<UserInfo> users)
        {
            try
            {
                if (users != null && users.Count > 0)
                {
                    foreach (UserInfo user in users)
                    {
                        if (string.IsNullOrEmpty(user.UserId))
                        {
                            logger.Warn($"this user id is null,will set it by get user,user name:{user.UserPrincipalName}");
                            var userInfo = await UserService.GetUserByNameAsync(user.UserPrincipalName);
                            user.UserId = userInfo?.UserId;
                            logger.Warn($"set user id from user,id:{userInfo?.UserId}");
                        }
                    }
                }
            }
            catch (Exception e)
            {
                logger.Error($"set user id from user failed,error:{e}");
            }
        }

        public void InitSORuleManualApprovalInfo(Rule rule, RMRuleInfos info)
        {
            rule.IsManualApproval = info.EnableManualApproval;
            rule.IsSendEamilToOwner = info.IsSendEmailToOwner;
            switch (info.ManualReviewType)
            {
                case ReviewType.RecordOwner:
                    rule.ReviewType = info.ManualReviewType;
                    rule.UserInfos = Convert2RecordOwnerInfos(info.Users);
                    rule.WorkflowId = "";
                    break;
                case ReviewType.Workflow:
                    rule.ReviewType = info.ManualReviewType;
                    rule.WorkflowId = info.WorkflowId;
                    rule.IsGControlManualApproval = info.IsGControlManualApproval;
                    break;
                default:
                    break;
            }
            rule.IsRestoreLink = info.IsRestoreLink;
            //rule.IsEnableRetention = info.IsEnableRetention;
            //rule.RetentionInfo = this.ConvertRetentionSetting(info.RetentionInfo);
        }

        private async Task<RetentionInfo> ConvertRetentionSettingAsync(RetentionSettings setting)
        {
            if (setting == null)
            {
                return null;
            }
            RetentionInfo info = new RetentionInfo();
            info.IsManualApproval = setting.IsManualApproval;
            info.ColumnName = setting.ColumnName;
            info.Condition = setting.Condition;
            info.KeepDateNumber = setting.KeepDateNumber;
            info.KeepDateUnite = setting.KeepDateUnite;
            info.Date = setting.Date == null ? DateTime.MinValue.Ticks : await ConvertTime4BackendAsync(setting.Date);
            info.ReviewType = setting.ReviewType;
            info.WorkflowId = setting.WorkflowId;
            info.IsSendEamilToOwner = setting.IsSendEamilToOwner;
            info.UserInfos = setting.UserInfos;
            return info;
        }

        private List<RetentionRule> ConvertStoreContentRetentionSetting(List<RetentionSettings> setting)
        {
            List<RetentionRule> infos = new();
            if (setting == null || setting.Count==0)
            {
                return null;
            }
            foreach (RetentionSettings tempSetting in setting)
            {
                if(tempSetting == null)
                {
                    continue;
                }
                RetentionRule info = new();
                infos.Add(info);
                info.SetupDataRetention = tempSetting.IsEnableRetention;
                info.RetentionDataTimeType = tempSetting.RetentionDataTimeType == KeepDateType.None? KeepDateType.ArchiveTime: tempSetting.RetentionDataTimeType;
                info.KeepValue = tempSetting.KeepDateNumber;
                info.ArchiveDateUnit = tempSetting.KeepDateUnite switch
                {
                    TimeUnit.Day => DateUnit.Day,
                    TimeUnit.Week => DateUnit.Week,
                    TimeUnit.Month => DateUnit.Month,
                    TimeUnit.Year => DateUnit.Year,
                    _ => throw new NotImplementedException(),
                };
                //save rule
                if (tempSetting.OperateDataType == (int)OperateDateTypeEnum.Delete)
                {
                    info.DeleteTheData = true;
                }
                else if(tempSetting.OperateDataType == (int)OperateDateTypeEnum.MarkTier)
                {
                    info.IsMarkDataTier = true;
                    info.TierType = tempSetting.TierType??0;
                }
                info.RemoveOrphanedStub = tempSetting.RemoveOrphanedStub;
                info.KeepOrphanedStub4CompatibilityExistingRule = !tempSetting.RemoveOrphanedStub;
                info.SoftDeleteDateUnit = tempSetting.SoftKeepDateUnite switch
                {
                    TimeUnit.Day => DateUnit.Day,
                    TimeUnit.Week => DateUnit.Week,
                    TimeUnit.Month => DateUnit.Month,
                    TimeUnit.Year => DateUnit.Year,
                    _ => throw new NotImplementedException(),
                };
                info.SoftDeleteKeepValue = tempSetting.SoftKeepDateNumber;
                info.IsSoftDelete = tempSetting.IsSoftDelete;
            }
            return infos;
        }

        private void SetLeaveStubMessage(Rule rule, RMRuleInfos info)
        {
            if ((info.RuleKeepDataOption & 128) == (int)KeepDataStatus.LinkToDocument
                || (info.RuleKeepDataOption & 2048) == (int)KeepDataStatus.ArchiveAndLeaveStub
                || info.RuleKeepDataOption == 4224)
            {
                rule.LeaveStubMessage = !string.IsNullOrEmpty(info.LeaveStubMessage) ? HttpUtility.HtmlEncode(info.LeaveStubMessage) : I18NEntity.GetString("RM_RDM_CreateRule_LeaveStubOptionMessage_Default");
            }
            else
            {
                rule.LeaveStubMessage = "";
            }
        }

        public void InitRMRuleManualApprovalInfo(Rule rule, RMRuleInfos newRule)
        {
            if (rule.IsManualApproval)
            {
                switch (rule.ReviewType)
                {
                    case ReviewType.RecordOwner:
                        newRule.Users = Convert2AOSUserDtos(rule.UserInfos);
                        newRule.ManualReviewType = ReviewType.RecordOwner;
                        break;
                    case ReviewType.Workflow:
                        newRule.ManualReviewType = ReviewType.Workflow;
                        if (rule.IsGControlManualApproval)
                        {
                            var gControlWorkflow =
                                 _gControlPlatformApprovalProcessService.GetPlatformApprovalProcess(
                                    new Guid(rule.WorkflowId)).Result;
                            newRule.WorkflowId = rule.WorkflowId;
                            newRule.WorkflowName = gControlWorkflow?.Name;
                            newRule.IsGControlManualApproval = rule.IsGControlManualApproval;

                            break;
                        }
                        var workflow = ManualProcessManagementService.GetWorkflow(Guid.Parse(rule.WorkflowId));
                        if (workflow != null)
                        {
                            newRule.WorkflowId = rule.WorkflowId;
                            newRule.WorkflowName = workflow?.Name;
                        }
                        break;
                    default:
                        break;
                }
                newRule.IsSendEmailToOwner = rule.IsSendEamilToOwner;
            }
        }

        /// <summary>
        /// Support for export content to custom location
        /// Current reuse MoveDto, improve next time to be cleaner
        /// </summary>
        public async Task ExportMoveSettingsAsync(Rule rule, RMRuleInfos info)
        {
            if ( info.MoveDto != null && (info.MoveDto.LocationPath != null || info.MoveDto.SPTree != null))
            {
                rule.ExportInfo.spMoveOption = new MoveOption
                {
                    MoveDestination = new MoveDestination
                    {
                        NotDeclareMovedData = info.MoveDto.NotDeclareMovedData,
                    }
                };
                if (info.MoveDto.IsSpecifyLocation)
                {
                    CheckLocationObject resultCheckLocation = await ExplorerService.CheckSPUrl4RuleAsync(info.MoveDto.LocationPath, null);
                    if (resultCheckLocation == null)
                    {
                        throw new Exception(I18NEntity.GetString("RM_JS_Rule_SPDestUrlError"));
                    }
                    rule.ExportInfo.spMoveOption.MoveDestination.SPAccount = new()
                    {
                        UserName = resultCheckLocation.UserInfoName,
                        Password = resultCheckLocation.UserInfoKey
                    };
                    rule.ExportInfo.spMoveOption.MoveDestination.ContainerId = resultCheckLocation.ContainerId;
                    rule.ExportInfo.spMoveOption.MoveDestination.DestMode = AvePoint.GCommon.Contract.StorageOptimization.Object.DestMode.UrlMode;
                    rule.ExportInfo.spMoveOption.MoveDestination.SPUrl = info.MoveDto.LocationPath;
                }
                else
                {
                    CheckLocationObject resultCheckLocation = await ExplorerService.CheckSPUrl4RuleAsync(info.MoveDto.SPTree.FullPath, null);
                    if (resultCheckLocation == null)
                    {
                        throw new Exception(I18NEntity.GetString("RM_JS_Rule_SPDestUrlError"));
                    }
                    rule.ExportInfo.spMoveOption.MoveDestination.DestMode = AvePoint.GCommon.Contract.StorageOptimization.Object.DestMode.TreeMode; 
                    rule.ExportInfo.spMoveOption.MoveDestination.SPUrl = info.MoveDto.SPTree.FullPath;
                    rule.ExportInfo.spMoveOption.MoveDestination.SPTreeStr = !string.IsNullOrEmpty(info.MoveDto.SPTreeStr) ? RuleSPTreeUtil.BuildSPTreeXMLStr(info.MoveDto.SPTreeStr) : "";
                    rule.ExportInfo.spMoveOption.MoveDestination.ContainerId = RuleSPTreeUtil.GetContainerNode(info.MoveDto.SPTree) == null ? string.Empty : RuleSPTreeUtil.GetContainerNode(info.MoveDto.SPTree).Id;
                }
            }
            
            info.MoveDto = null;
        }
        public async System.Threading.Tasks.Task ResetMoveSettingsAsync(Rule rule, RMRuleInfos info)
        {
            #region SP
            if (info.IsSpSource && info.MoveDto != null && (info.MoveDto.LocationPath != null || info.MoveDto.SPTree != null))
            {
                rule.spMoveOption = new MoveOption();
                rule.spMoveOption.MoveDestination = new MoveDestination();
                rule.spMoveOption.MoveDestination.NotDeclareMovedData = info.MoveDto.NotDeclareMovedData;
                rule.spMoveOption.MoveDestination.KeepSourceClassification = info.MoveDto.isKeepClassification;
                rule.spMoveOption.MoveDestination.KeepFolderStructure = info.MoveDto.IsKeepFolderStructure;
                rule.spMoveOption.MoveDestination.IsMoveVersions = info.MoveDto.IsMoveAllVersions;
                if (info.MoveDto.IsSpecifyLocation)
                {
                    CheckLocationObject resultCheckLocation = await ExplorerService.CheckSPUrl4RuleAsync(info.MoveDto.LocationPath, null);
                    if (resultCheckLocation == null)
                    {
                        throw new Exception(I18NEntity.GetString("RM_JS_Rule_SPDestUrlError"));
                    }
                    rule.spMoveOption.MoveDestination.SPAccount = new Office365AccountInfo();
                    rule.spMoveOption.MoveDestination.SPAccount.UserName = resultCheckLocation.UserInfoName;
                    rule.spMoveOption.MoveDestination.SPAccount.Password = resultCheckLocation.UserInfoKey;
                    rule.spMoveOption.MoveDestination.ContainerId = resultCheckLocation.ContainerId;
                    rule.spMoveOption.MoveDestination.DestMode = AvePoint.GCommon.Contract.StorageOptimization.Object.DestMode.UrlMode;
                    rule.spMoveOption.MoveDestination.SPUrl = info.MoveDto.LocationPath;
                }
                else
                {
                    CheckLocationObject resultCheckLocation = await ExplorerService.CheckSPUrl4RuleAsync(info.MoveDto.SPTree.FullPath, null);
                    if (resultCheckLocation == null)
                    {
                        throw new Exception(I18NEntity.GetString("RM_JS_Rule_SPDestUrlError"));
                    }
                    rule.spMoveOption.MoveDestination.DestMode = AvePoint.GCommon.Contract.StorageOptimization.Object.DestMode.TreeMode;
                    //rule.spMoveOption.MoveDestination.SPTreeNode = RMDtoConverter.ConvertRMTree2SPTree(info.MoveDto.SPTree);
                    rule.spMoveOption.MoveDestination.SPUrl = info.MoveDto.SPTree.FullPath;
                    rule.spMoveOption.MoveDestination.SPTreeStr = !string.IsNullOrEmpty(info.MoveDto.SPTreeStr) ? RuleSPTreeUtil.BuildSPTreeXMLStr(info.MoveDto.SPTreeStr) : "";
                    rule.spMoveOption.MoveDestination.ContainerId = RuleSPTreeUtil.GetContainerNode(info.MoveDto.SPTree) == null ? string.Empty : RuleSPTreeUtil.GetContainerNode(info.MoveDto.SPTree).Id;
                }

                #region Move Settings
                rule.spMoveOption.MoveSetting = new MoveRecordSetting();
                rule.spMoveOption.MoveSetting.ConflictType = info.MoveDto.DestMode == Contract.RMWeb.DestMode.SharePoint ? ConflictType.SharePointConflict : ConflictType.FileSystemConflict;
                if (info.MoveDto.FolderNameConflictOption == FolderNameConflictOption.Merge)
                {
                    rule.spMoveOption.MoveSetting.ContainerLevelConflictOption = ConflictOption.Merge;
                    switch (info.MoveDto.FolderFilesNameConflictOption)
                    {
                        case FileNameConflictOption.Skip:
                            rule.spMoveOption.MoveSetting.ItemLevelConflictOption = ConflictOption.Skip;
                            break;
                        case FileNameConflictOption.Overwrite:
                            rule.spMoveOption.MoveSetting.ItemLevelConflictOption = ConflictOption.Overwrite;
                            break;
                        case FileNameConflictOption.Rename:
                            rule.spMoveOption.MoveSetting.ItemLevelConflictOption = ConflictOption.AppendByName;
                            break;
                        default:
                            break;
                    }
                }
                else if (info.MoveDto.FolderNameConflictOption == FolderNameConflictOption.Skip)
                {
                    rule.spMoveOption.MoveSetting.ContainerLevelConflictOption = ConflictOption.Skip;
                }

                if (info.MoveDto.FileNameConflictOption == FileNameConflictOption.Skip)
                {
                    rule.spMoveOption.MoveSetting.ItemLevelConflictOption = ConflictOption.Skip;
                }
                else if (info.MoveDto.FileNameConflictOption == FileNameConflictOption.Overwrite)
                {
                    rule.spMoveOption.MoveSetting.ItemLevelConflictOption = ConflictOption.Overwrite;
                }
                else if (info.MoveDto.FileNameConflictOption == FileNameConflictOption.Rename)
                {
                    rule.spMoveOption.MoveSetting.ItemLevelConflictOption = ConflictOption.AppendByName;
                }

                #endregion
            }

            #endregion
        }

        public async System.Threading.Tasks.Task ResetTeamsMoveSettingsAsync(Rule rule, RMRuleInfos info)
        {
            #region Teams
            if (info.MoveDto != null && (info.MoveDto.LocationPath != null || info.MoveDto.SPTree != null))
            {
                rule.spMoveOption = new MoveOption();
                rule.spMoveOption.MoveDestination = new MoveDestination();
                rule.spMoveOption.MoveDestination.NotDeclareMovedData = info.MoveDto.NotDeclareMovedData;
                rule.spMoveOption.MoveDestination.KeepSourceClassification = info.MoveDto.isKeepClassification;
                rule.spMoveOption.MoveDestination.KeepFolderStructure = info.MoveDto.IsKeepFolderStructure;
                rule.spMoveOption.MoveDestination.IsMoveVersions = info.MoveDto.IsMoveAllVersions;
                if (info.MoveDto.IsSpecifyLocation)
                {
                    CheckLocationObject resultCheckLocation = await ExplorerService.CheckSPUrl4RuleAsync(info.MoveDto.LocationPath, null);
                    if (resultCheckLocation == null)
                    {
                        throw new Exception(I18NEntity.GetString("RM_JS_Rule_SPDestUrlError"));
                    }
                    rule.spMoveOption.MoveDestination.SPAccount = new Office365AccountInfo();
                    rule.spMoveOption.MoveDestination.SPAccount.UserName = resultCheckLocation.UserInfoName;
                    rule.spMoveOption.MoveDestination.SPAccount.Password = resultCheckLocation.UserInfoKey;
                    rule.spMoveOption.MoveDestination.ContainerId = resultCheckLocation.ContainerId;
                    rule.spMoveOption.MoveDestination.DestMode = AvePoint.GCommon.Contract.StorageOptimization.Object.DestMode.UrlMode;
                    rule.spMoveOption.MoveDestination.SPUrl = info.MoveDto.LocationPath;
                }
                else
                {
                    CheckLocationObject resultCheckLocation = await ExplorerService.CheckSPUrl4RuleAsync(info.MoveDto.SPTree.FullPath, null);
                    if (resultCheckLocation == null)
                    {
                        throw new Exception(I18NEntity.GetString("RM_JS_Rule_SPDestUrlError"));
                    }
                    rule.spMoveOption.MoveDestination.DestMode = AvePoint.GCommon.Contract.StorageOptimization.Object.DestMode.TreeMode;
                    //rule.spMoveOption.MoveDestination.SPTreeNode = RMDtoConverter.ConvertRMTree2SPTree(info.MoveDto.SPTree);
                    rule.spMoveOption.MoveDestination.SPUrl = info.MoveDto.SPTree.FullPath;
                    rule.spMoveOption.MoveDestination.SPTreeStr = !string.IsNullOrEmpty(info.MoveDto.SPTreeStr) ? RuleSPTreeUtil.BuildSPTreeXMLStr(info.MoveDto.SPTreeStr) : "";
                    rule.spMoveOption.MoveDestination.ContainerId = RuleSPTreeUtil.GetContainerNode(info.MoveDto.SPTree) == null ? string.Empty : RuleSPTreeUtil.GetContainerNode(info.MoveDto.SPTree).Id;
                }

                #region Move Settings
                rule.spMoveOption.MoveSetting = new MoveRecordSetting();
                rule.spMoveOption.MoveSetting.ConflictType = info.MoveDto.DestMode == Contract.RMWeb.DestMode.SharePoint ? ConflictType.SharePointConflict : ConflictType.FileSystemConflict;
                if (info.MoveDto.FolderNameConflictOption == FolderNameConflictOption.Merge)
                {
                    rule.spMoveOption.MoveSetting.ContainerLevelConflictOption = ConflictOption.Merge;
                    switch (info.MoveDto.FolderFilesNameConflictOption)
                    {
                        case FileNameConflictOption.Skip:
                            rule.spMoveOption.MoveSetting.ItemLevelConflictOption = ConflictOption.Skip;
                            break;
                        case FileNameConflictOption.Overwrite:
                            rule.spMoveOption.MoveSetting.ItemLevelConflictOption = ConflictOption.Overwrite;
                            break;
                        case FileNameConflictOption.Rename:
                            rule.spMoveOption.MoveSetting.ItemLevelConflictOption = ConflictOption.AppendByName;
                            break;
                        default:
                            break;
                    }
                }
                else if (info.MoveDto.FolderNameConflictOption == FolderNameConflictOption.Skip)
                {
                    rule.spMoveOption.MoveSetting.ContainerLevelConflictOption = ConflictOption.Skip;
                }

                if (info.MoveDto.FileNameConflictOption == FileNameConflictOption.Skip)
                {
                    rule.spMoveOption.MoveSetting.ItemLevelConflictOption = ConflictOption.Skip;
                }
                else if (info.MoveDto.FileNameConflictOption == FileNameConflictOption.Overwrite)
                {
                    rule.spMoveOption.MoveSetting.ItemLevelConflictOption = ConflictOption.Overwrite;
                }
                else if (info.MoveDto.FileNameConflictOption == FileNameConflictOption.Rename)
                {
                    rule.spMoveOption.MoveSetting.ItemLevelConflictOption = ConflictOption.AppendByName;
                }

                #endregion
            }

            #endregion
        }

        public async System.Threading.Tasks.Task ResetEXOMoveSettingsAsync(Rule rule, RMRuleInfos info)
        {
            #region EXO
            if (info.IsExoSource && info.EXORule.MoveDto != null && (info.EXORule.MoveDto.LocationPath != null || info.EXORule.MoveDto.SPTree != null))
            {
                rule.EXORule.spMoveOption = new MoveOption();
                rule.EXORule.spMoveOption.MoveDestination = new MoveDestination();
                rule.EXORule.spMoveOption.MoveDestination.NotDeclareMovedData = info.EXORule.MoveDto.NotDeclareMovedData;
                rule.EXORule.spMoveOption.MoveDestination.DeleteSourceItem = info.EXORule.MoveDto.IsDeleteSourceItem;
                rule.EXORule.spMoveOption.MoveDestination.KeepSourceClassification = info.EXORule.MoveDto.isKeepClassification;
                rule.EXORule.spMoveOption.SourceFlag = RecordFlag.EXO;
                rule.EXORule.spMoveOption.DestFlag = RecordFlag.SP;
                rule.EXORule.spMoveOption.IsMoveToSP = info.EXORule.MoveDto.IsMoveToSP;
                rule.EXORule.spMoveOption.MoveToSPDataList = info.EXORule.MoveDto.MoveToSPDataList;
                if (info.EXORule.MoveDto.IsSpecifyLocation)
                {
                    CheckLocationObject resultCheckLocation = await ExplorerService.CheckSPUrl4RuleAsync(info.EXORule.MoveDto.LocationPath, null);
                    if (resultCheckLocation == null)
                    {
                        throw new Exception(I18NEntity.GetString("RM_JS_Rule_SPDestUrlError"));
                    }
                    rule.EXORule.spMoveOption.MoveDestination.SPAccount = new Office365AccountInfo();
                    rule.EXORule.spMoveOption.MoveDestination.SPAccount.UserName = resultCheckLocation.UserInfoName;
                    rule.EXORule.spMoveOption.MoveDestination.SPAccount.Password = resultCheckLocation.UserInfoKey;
                    rule.EXORule.spMoveOption.MoveDestination.ContainerId = resultCheckLocation.ContainerId;
                    rule.EXORule.spMoveOption.MoveDestination.DestMode = AvePoint.GCommon.Contract.StorageOptimization.Object.DestMode.UrlMode;
                    rule.EXORule.spMoveOption.MoveDestination.SPUrl = info.EXORule.MoveDto.LocationPath;
                }
                else
                {
                    rule.EXORule.spMoveOption.MoveDestination.DestMode = AvePoint.GCommon.Contract.StorageOptimization.Object.DestMode.TreeMode;
                    //rule.spMoveOption.MoveDestination.SPTreeNode = RMDtoConverter.ConvertRMTree2SPTree(info.MoveDto.SPTree);
                    rule.EXORule.spMoveOption.MoveDestination.SPUrl = info.EXORule.MoveDto.SPTree.FullPath;
                    rule.EXORule.spMoveOption.MoveDestination.SPTreeStr = !string.IsNullOrEmpty(info.EXORule.MoveDto.SPTreeStr) ? RuleSPTreeUtil.BuildSPTreeXMLStr(info.EXORule.MoveDto.SPTreeStr) : "";
                    rule.EXORule.spMoveOption.MoveDestination.ContainerId = RuleSPTreeUtil.GetContainerNode(info.EXORule.MoveDto.SPTree) == null ? string.Empty : RuleSPTreeUtil.GetContainerNode(info.EXORule.MoveDto.SPTree).Id;
                }


                #region Move Settings
                rule.EXORule.spMoveOption.MoveSetting = new MoveRecordSetting();
                rule.EXORule.spMoveOption.MoveSetting.ConflictType = ConflictType.SharePointConflict;
                if (info.EXORule.MoveDto.FolderNameConflictOption == FolderNameConflictOption.Merge)
                {
                    rule.EXORule.spMoveOption.MoveSetting.ContainerLevelConflictOption = ConflictOption.Merge;
                    switch (info.EXORule.MoveDto.FolderFilesNameConflictOption)
                    {
                        case FileNameConflictOption.Skip:
                            rule.EXORule.spMoveOption.MoveSetting.ItemLevelConflictOption = ConflictOption.Skip;
                            break;
                        case FileNameConflictOption.Overwrite:
                            rule.EXORule.spMoveOption.MoveSetting.ItemLevelConflictOption = ConflictOption.Overwrite;
                            break;
                        case FileNameConflictOption.Rename:
                            rule.EXORule.spMoveOption.MoveSetting.ItemLevelConflictOption = ConflictOption.AppendByName;
                            break;
                        default:
                            break;
                    }
                }
                else if (info.EXORule.MoveDto.FolderNameConflictOption == FolderNameConflictOption.Skip)
                {
                    rule.EXORule.spMoveOption.MoveSetting.ContainerLevelConflictOption = ConflictOption.Skip;
                }

                if (info.EXORule.MoveDto.FileNameConflictOption == FileNameConflictOption.Skip)
                {
                    rule.EXORule.spMoveOption.MoveSetting.ItemLevelConflictOption = ConflictOption.Skip;
                }
                else if (info.EXORule.MoveDto.FileNameConflictOption == FileNameConflictOption.Overwrite)
                {
                    rule.EXORule.spMoveOption.MoveSetting.ItemLevelConflictOption = ConflictOption.Overwrite;
                }
                else if (info.EXORule.MoveDto.FileNameConflictOption == FileNameConflictOption.Rename)
                {
                    rule.EXORule.spMoveOption.MoveSetting.ItemLevelConflictOption = ConflictOption.AppendByName;
                }

                #endregion
            }

            #endregion
        }
        public void ResetGoogleMoveSettingsAsync(Rule rule, RMRuleInfos info)
        {
            #region Google
            if (info.IsGoogleDriveSource && info.GoogleDriveRule.MoveDto is { GoogleTree: not null })
            {
                string url = info.GoogleDriveRule.MoveDto.GoogleTree.Level switch
                {
                    (int)NodeLevel.GoogleSharedDrive or (int)NodeLevel.GoogleFolder => info.GoogleDriveRule.MoveDto
                        .GoogleTree.ObjectId,
                    (int)NodeLevel.GoogleMyDrive => "root"
                };
                rule.GoogleDriveRule.spMoveOption = new MoveOption
                {
                    MoveDestination = new MoveDestination()
                    {
                        DestinationId = url,
                        GoogleTreeNode = RMDtoConverter.ConvertRMGoogleTreeNode2Dto(info.GoogleDriveRule.MoveDto.GoogleTree),
                        GoogleTreeStr = !string.IsNullOrEmpty(info.GoogleDriveRule.MoveDto.GoogleTreeStr) ? RuleSPTreeUtil.BuildGoogleTreeXmlStr(info.GoogleDriveRule.MoveDto.GoogleTreeStr) : ""
                    }
                };
            }

            #endregion
        }
        public void ResetFSMoveSettings(Rule rule, RMRuleInfos info)
        {
            #region SP
            if (info.IsFSSource && info.FSRule.MoveDto != null && (info.FSRule.MoveDto.LocationPath != null || info.FSRule.MoveDto.FSTree != null))
            {
                rule.FSRule.spMoveOption = new MoveOption();
                rule.FSRule.spMoveOption.MoveDestination = new MoveDestination();
                rule.FSRule.spMoveOption.MoveDestination.NotDeclareMovedData = info.FSRule.MoveDto.NotDeclareMovedData;
                rule.FSRule.spMoveOption.SourceFlag = RecordFlag.FS;
                rule.FSRule.spMoveOption.DestFlag = RecordFlag.FS;
                if (info.FSRule.MoveDto.IsSpecifyLocation)
                {
                    //TO DO Validate
                    //CheckLocationObject resultCheckLocation = ExplorerService.CheckSPUrl4Rule(info.MoveDto.LocationPath, null);
                    //if (resultCheckLocation == null)
                    //{
                    //    throw new Exception(I18NEntity.GetString("RM_JS_Rule_SPDestUrlError"));
                    //}
                    rule.FSRule.spMoveOption.MoveDestination.DestMode = AvePoint.GCommon.Contract.StorageOptimization.Object.DestMode.UrlMode;
                    rule.FSRule.spMoveOption.MoveDestination.FSPath = info.FSRule.MoveDto.LocationPath;
                }
                else
                {
                    rule.FSRule.spMoveOption.MoveDestination.DestMode = AvePoint.GCommon.Contract.StorageOptimization.Object.DestMode.TreeMode;
                    rule.FSRule.spMoveOption.MoveDestination.FSTreeNode = RMDtoConverter.ConvertRMTree2FSTree(info.FSRule.MoveDto.FSTree);
                    if (info.FSRule.MoveDto.FSTree != null && !string.IsNullOrEmpty(info.FSRule.MoveDto.FSTree.FullPath))
                    {
                        rule.FSRule.spMoveOption.MoveDestination.FSPath = EncodeUtil.DecryptByCommunicationKey(info.FSRule.MoveDto.FSTree.FullPath);//TO DO FS
                    }
                    rule.FSRule.spMoveOption.MoveDestination.FSTreeStr = !string.IsNullOrEmpty(info.FSRule.MoveDto.FSTreeStr) ? RuleSPTreeUtil.BuildFSTreeXMLStr(info.FSRule.MoveDto.FSTreeStr) : "";
                }

                #region Move Settings
                rule.FSRule.spMoveOption.MoveSetting = new MoveRecordSetting();
                rule.FSRule.spMoveOption.MoveSetting.ConflictType = info.FSRule.MoveDto.DestMode == Contract.RMWeb.DestMode.FileSystem ? ConflictType.FileSystemConflict : ConflictType.SharePointConflict;
                if (info.FSRule.MoveDto.FolderNameConflictOption == FolderNameConflictOption.Merge)
                {
                    rule.FSRule.spMoveOption.MoveSetting.ContainerLevelConflictOption = ConflictOption.Merge;
                    switch (info.FSRule.MoveDto.FolderFilesNameConflictOption)
                    {
                        case FileNameConflictOption.Skip:
                            rule.FSRule.spMoveOption.MoveSetting.ItemLevelConflictOption = ConflictOption.Skip;
                            break;
                        case FileNameConflictOption.Overwrite:
                            rule.FSRule.spMoveOption.MoveSetting.ItemLevelConflictOption = ConflictOption.Overwrite;
                            break;
                        case FileNameConflictOption.Rename:
                            rule.FSRule.spMoveOption.MoveSetting.ItemLevelConflictOption = ConflictOption.AppendByName;
                            break;
                        default:
                            break;
                    }
                }
                else if (info.FSRule.MoveDto.FolderNameConflictOption == FolderNameConflictOption.Skip)
                {
                    rule.FSRule.spMoveOption.MoveSetting.ContainerLevelConflictOption = ConflictOption.Skip;
                }

                if (info.FSRule.MoveDto.FileNameConflictOption == FileNameConflictOption.Skip)
                {
                    rule.FSRule.spMoveOption.MoveSetting.ItemLevelConflictOption = ConflictOption.Skip;
                }
                else if (info.FSRule.MoveDto.FileNameConflictOption == FileNameConflictOption.Overwrite)
                {
                    rule.FSRule.spMoveOption.MoveSetting.ItemLevelConflictOption = ConflictOption.Overwrite;
                }
                else if (info.FSRule.MoveDto.FileNameConflictOption == FileNameConflictOption.Rename)
                {
                    rule.FSRule.spMoveOption.MoveSetting.ItemLevelConflictOption = ConflictOption.AppendByName;
                }

                #endregion
            }

            #endregion
        }

        public async System.Threading.Tasks.Task ResetSPLocalMoveSettingsAsync(Rule rule, RMRuleInfos info)
        {
            #region SP Local
            if (info.IsSPLocalSource && info.SPLocalRule.MoveDto != null && (info.SPLocalRule.MoveDto.LocationPath != null || info.SPLocalRule.MoveDto.SPTree != null))
            {
                rule.SPLocalRule.spMoveOption = new MoveOption();
                rule.SPLocalRule.spMoveOption.MoveDestination = new MoveDestination();
                rule.SPLocalRule.spMoveOption.MoveDestination.NotDeclareMovedData = info.SPLocalRule.MoveDto.NotDeclareMovedData;
                rule.SPLocalRule.spMoveOption.SourceFlag = RecordFlag.SPLocal;
                rule.SPLocalRule.spMoveOption.DestFlag = RecordFlag.SPLocal;
                if (info.SPLocalRule.MoveDto.IsSpecifyLocation)
                {
                    CheckLocationObject resultCheckLocation = await ExplorerService.CheckSPUrl4RuleAsync(info.SPLocalRule.MoveDto.LocationPath, null);
                    if (resultCheckLocation == null)
                    {
                        throw new Exception(I18NEntity.GetString("RM_JS_Rule_SPDestUrlError"));
                    }
                    rule.SPLocalRule.spMoveOption.MoveDestination.SPAccount = new Office365AccountInfo();
                    rule.SPLocalRule.spMoveOption.MoveDestination.SPAccount.UserName = resultCheckLocation.UserInfoName;
                    rule.SPLocalRule.spMoveOption.MoveDestination.SPAccount.Password = resultCheckLocation.UserInfoKey;
                    rule.SPLocalRule.spMoveOption.MoveDestination.ContainerId = resultCheckLocation.ContainerId;
                    rule.SPLocalRule.spMoveOption.MoveDestination.DestMode = AvePoint.GCommon.Contract.StorageOptimization.Object.DestMode.UrlMode;
                    rule.SPLocalRule.spMoveOption.MoveDestination.SPUrl = info.SPLocalRule.MoveDto.LocationPath;
                }
                else
                {
                    rule.SPLocalRule.spMoveOption.MoveDestination.DestMode = AvePoint.GCommon.Contract.StorageOptimization.Object.DestMode.TreeMode;
                    //rule.spMoveOption.MoveDestination.SPTreeNode = RMDtoConverter.ConvertRMTree2SPTree(info.MoveDto.SPTree);
                    rule.SPLocalRule.spMoveOption.MoveDestination.SPUrl = info.SPLocalRule.MoveDto.SPTree.FullPath;
                    rule.SPLocalRule.spMoveOption.MoveDestination.SPTreeStr = !string.IsNullOrEmpty(info.SPLocalRule.MoveDto.SPTreeStr) ? RuleSPTreeUtil.BuildSPTreeXMLStr(info.SPLocalRule.MoveDto.SPTreeStr) : "";
                    rule.SPLocalRule.spMoveOption.MoveDestination.ContainerId = RuleSPTreeUtil.GetContainerNode(info.SPLocalRule.MoveDto.SPTree) == null ? string.Empty : RuleSPTreeUtil.GetContainerNode(info.SPLocalRule.MoveDto.SPTree).Id;
                }

                #region Move Settings
                rule.SPLocalRule.spMoveOption.MoveSetting = new MoveRecordSetting();
                rule.SPLocalRule.spMoveOption.MoveSetting.ConflictType = info.SPLocalRule.MoveDto.DestMode == Contract.RMWeb.DestMode.SharePoint ? ConflictType.SharePointConflict : ConflictType.FileSystemConflict;
                if (info.SPLocalRule.MoveDto.FolderNameConflictOption == FolderNameConflictOption.Merge)
                {
                    rule.SPLocalRule.spMoveOption.MoveSetting.ContainerLevelConflictOption = ConflictOption.Merge;
                    switch (info.SPLocalRule.MoveDto.FolderFilesNameConflictOption)
                    {
                        case FileNameConflictOption.Skip:
                            rule.SPLocalRule.spMoveOption.MoveSetting.ItemLevelConflictOption = ConflictOption.Skip;
                            break;
                        case FileNameConflictOption.Overwrite:
                            rule.SPLocalRule.spMoveOption.MoveSetting.ItemLevelConflictOption = ConflictOption.Overwrite;
                            break;
                        case FileNameConflictOption.Rename:
                            rule.SPLocalRule.spMoveOption.MoveSetting.ItemLevelConflictOption = ConflictOption.AppendByName;
                            break;
                        default:
                            break;
                    }
                }
                else if (info.SPLocalRule.MoveDto.FolderNameConflictOption == FolderNameConflictOption.Skip)
                {
                    rule.SPLocalRule.spMoveOption.MoveSetting.ContainerLevelConflictOption = ConflictOption.Skip;
                }

                if (info.SPLocalRule.MoveDto.FileNameConflictOption == FileNameConflictOption.Skip)
                {
                    rule.SPLocalRule.spMoveOption.MoveSetting.ItemLevelConflictOption = ConflictOption.Skip;
                }
                else if (info.SPLocalRule.MoveDto.FileNameConflictOption == FileNameConflictOption.Overwrite)
                {
                    rule.SPLocalRule.spMoveOption.MoveSetting.ItemLevelConflictOption = ConflictOption.Overwrite;
                }
                else if (info.SPLocalRule.MoveDto.FileNameConflictOption == FileNameConflictOption.Rename)
                {
                    rule.SPLocalRule.spMoveOption.MoveSetting.ItemLevelConflictOption = ConflictOption.AppendByName;
                }

                #endregion
            }

            #endregion
        }
        public async System.Threading.Tasks.Task ResetPhyMoveSettingsAsync(Rule rule, RMRuleInfos info)
        {
            #region Physical

            if (info.IsPhySource && info.PhysicalRule.MoveDto != null && info.PhysicalRule.MoveDto.PhysicalTreeNode != null)
            {
                var moveDto = info.PhysicalRule.MoveDto;
                var moveOption = new MoveOption();
                rule.PhysicalRule.spMoveOption = moveOption;
                moveOption.MoveDestination = new MoveDestination();
                moveOption.MoveDestination.DestMode = AvePoint.GCommon.Contract.StorageOptimization.Object.DestMode.TreeMode;
                moveOption.MoveDestination.PhysicalTree = await ConvertRMPhyTreeNodeToSOTreeNodeAsync(moveDto.PhysicalTreeNode);
                moveOption.MoveDestination.PhysicalTreeStr = moveDto.PhysicalTreeStr;
                moveOption.SourceFlag = RecordFlag.Physical;
                moveOption.DestFlag = RecordFlag.Physical;
                var moveSetting = new MoveRecordSetting();
                moveOption.MoveSetting = moveSetting;
                moveSetting.ConflictType = ConflictType.FileSystemConflict;
                if (moveDto.FolderNameConflictOption == FolderNameConflictOption.Merge)
                {
                    moveSetting.ContainerLevelConflictOption = ConflictOption.Merge;
                    switch (moveDto.FolderFilesNameConflictOption)
                    {
                        case FileNameConflictOption.Skip:
                            moveSetting.ItemLevelConflictOption = ConflictOption.Skip;
                            break;
                        case FileNameConflictOption.Overwrite:
                            moveSetting.ItemLevelConflictOption = ConflictOption.Overwrite;
                            break;
                        case FileNameConflictOption.Rename:
                            moveSetting.ItemLevelConflictOption = ConflictOption.AppendByName;
                            break;
                        default:
                            break;
                    }
                }
                else if (moveDto.FolderNameConflictOption == FolderNameConflictOption.Skip)
                {
                    moveSetting.ContainerLevelConflictOption = ConflictOption.Skip;
                }

                if (moveDto.FileNameConflictOption == FileNameConflictOption.Skip)
                {
                    moveSetting.ItemLevelConflictOption = ConflictOption.Skip;
                }
                else if (moveDto.FileNameConflictOption == FileNameConflictOption.Overwrite)
                {
                    moveSetting.ItemLevelConflictOption = ConflictOption.Overwrite;
                }
                else if (moveDto.FileNameConflictOption == FileNameConflictOption.Rename)
                {
                    moveSetting.ItemLevelConflictOption = ConflictOption.AppendByName;
                }

                if (info.RuleLevel == PolicyLevel.Folder)
                {
                    if (moveDto.MoveHoldConflictOption == MoveHoldConflictOption.Current)
                    {
                        moveSetting.PhysicalHoldConflictOption = PhysicalHoldConflictOption.UseDesDefinedHoldSetting;
                    }
                    else if (moveDto.MoveHoldConflictOption == MoveHoldConflictOption.Compare)
                    {
                        moveSetting.PhysicalHoldConflictOption = PhysicalHoldConflictOption.CompareHoldSetting;
                    }
                }
            }
            #endregion
        }
        public async System.Threading.Tasks.Task ResetOneDriveMoveSettingsAsync(Rule rule, RMRuleInfos info)
        {
            #region OneDrive
            if (info.IsOneDriveSource && info.OneDriveRule.MoveDto != null && (info.OneDriveRule.MoveDto.LocationPath != null || info.OneDriveRule.MoveDto.SPTree != null))
            {
                rule.OneDriveRule.spMoveOption = new MoveOption();
                rule.OneDriveRule.spMoveOption.MoveDestination = new MoveDestination();
                rule.OneDriveRule.spMoveOption.MoveDestination.NotDeclareMovedData = info.OneDriveRule.MoveDto.NotDeclareMovedData;
                rule.OneDriveRule.spMoveOption.MoveDestination.KeepSourceClassification = info.OneDriveRule.MoveDto.isKeepClassification;
                rule.OneDriveRule.spMoveOption.MoveDestination.KeepFolderStructure = info.OneDriveRule.MoveDto.IsKeepFolderStructure;
                rule.OneDriveRule.spMoveOption.MoveDestination.IsMoveVersions = info.OneDriveRule.MoveDto.IsMoveAllVersions;
                if (info.OneDriveRule.MoveDto.IsSpecifyLocation)
                {
                    CheckLocationObject resultCheckLocation = await ExplorerService.CheckSPUrl4RuleAsync(info.OneDriveRule.MoveDto.LocationPath, null);
                    if (resultCheckLocation == null)
                    {
                        throw new Exception(I18NEntity.GetString("RM_JS_Rule_SPDestUrlError"));
                    }
                    rule.OneDriveRule.spMoveOption.MoveDestination.SPAccount = new Office365AccountInfo();
                    rule.OneDriveRule.spMoveOption.MoveDestination.SPAccount.UserName = resultCheckLocation.UserInfoName;
                    rule.OneDriveRule.spMoveOption.MoveDestination.SPAccount.Password = resultCheckLocation.UserInfoKey;
                    rule.OneDriveRule.spMoveOption.MoveDestination.ContainerId = resultCheckLocation.ContainerId;
                    rule.OneDriveRule.spMoveOption.MoveDestination.DestMode = AvePoint.GCommon.Contract.StorageOptimization.Object.DestMode.UrlMode;
                    rule.OneDriveRule.spMoveOption.MoveDestination.SPUrl = info.OneDriveRule.MoveDto.LocationPath;
                }
                else
                {
                    CheckLocationObject resultCheckLocation = await ExplorerService.CheckSPUrl4RuleAsync(info.OneDriveRule.MoveDto.SPTree.FullPath, null);
                    if (resultCheckLocation == null)
                    {
                        throw new Exception(I18NEntity.GetString("RM_JS_Rule_SPDestUrlError"));
                    }
                    rule.OneDriveRule.spMoveOption.MoveDestination.DestMode = AvePoint.GCommon.Contract.StorageOptimization.Object.DestMode.TreeMode;
                    //rule.spMoveOption.MoveDestination.SPTreeNode = RMDtoConverter.ConvertRMTree2SPTree(info.MoveDto.SPTree);
                    rule.OneDriveRule.spMoveOption.MoveDestination.SPUrl = info.OneDriveRule.MoveDto.SPTree.FullPath;
                    rule.OneDriveRule.spMoveOption.MoveDestination.SPTreeStr = !string.IsNullOrEmpty(info.OneDriveRule.MoveDto.SPTreeStr) ? RuleSPTreeUtil.BuildSPTreeXMLStr(info.OneDriveRule.MoveDto.SPTreeStr) : "";
                    rule.OneDriveRule.spMoveOption.MoveDestination.ContainerId = RuleSPTreeUtil.GetContainerNode(info.OneDriveRule.MoveDto.SPTree) == null ? string.Empty : RuleSPTreeUtil.GetContainerNode(info.OneDriveRule.MoveDto.SPTree).Id;
                }

                #region Move Settings
                rule.OneDriveRule.spMoveOption.MoveSetting = new MoveRecordSetting();
                rule.OneDriveRule.spMoveOption.MoveSetting.ConflictType = info.OneDriveRule.MoveDto.DestMode == Contract.RMWeb.DestMode.SharePoint ? ConflictType.SharePointConflict : ConflictType.FileSystemConflict;
                if (info.OneDriveRule.MoveDto.FolderNameConflictOption == FolderNameConflictOption.Merge)
                {
                    rule.OneDriveRule.spMoveOption.MoveSetting.ContainerLevelConflictOption = ConflictOption.Merge;
                    switch (info.OneDriveRule.MoveDto.FolderFilesNameConflictOption)
                    {
                        case FileNameConflictOption.Skip:
                            rule.OneDriveRule.spMoveOption.MoveSetting.ItemLevelConflictOption = ConflictOption.Skip;
                            break;
                        case FileNameConflictOption.Overwrite:
                            rule.OneDriveRule.spMoveOption.MoveSetting.ItemLevelConflictOption = ConflictOption.Overwrite;
                            break;
                        case FileNameConflictOption.Rename:
                            rule.OneDriveRule.spMoveOption.MoveSetting.ItemLevelConflictOption = ConflictOption.AppendByName;
                            break;
                        default:
                            break;
                    }
                }
                else if (info.OneDriveRule.MoveDto.FolderNameConflictOption == FolderNameConflictOption.Skip)
                {
                    rule.OneDriveRule.spMoveOption.MoveSetting.ContainerLevelConflictOption = ConflictOption.Skip;
                }

                if (info.OneDriveRule.MoveDto.FileNameConflictOption == FileNameConflictOption.Skip)
                {
                    rule.OneDriveRule.spMoveOption.MoveSetting.ItemLevelConflictOption = ConflictOption.Skip;
                }
                else if (info.OneDriveRule.MoveDto.FileNameConflictOption == FileNameConflictOption.Overwrite)
                {
                    rule.OneDriveRule.spMoveOption.MoveSetting.ItemLevelConflictOption = ConflictOption.Overwrite;
                }
                else if (info.OneDriveRule.MoveDto.FileNameConflictOption == FileNameConflictOption.Rename)
                {
                    rule.OneDriveRule.spMoveOption.MoveSetting.ItemLevelConflictOption = ConflictOption.AppendByName;
                }

                #endregion
            }

            #endregion
        }
        public async Task<PhysicalDestTree> ConvertRMPhyTreeNodeToSOTreeNodeAsync(RMPhysicalExplorerNode phyTreeNode)
        {
            PhysicalDestTree soTreeNode = new PhysicalDestTree();
            soTreeNode.LocationId = phyTreeNode.LocationId;
            soTreeNode.FileId = phyTreeNode.FileId;
            if (phyTreeNode.NodeType == (int)RMNodeLevel.PhysicalBox)
            {
                soTreeNode.BoxId = phyTreeNode.Id;
                soTreeNode.FullPath = await ExplorerService.GetPhysicalBoxPathByIdAsync(new Guid(phyTreeNode.Id));
            }
            else
            {
                if (!string.IsNullOrEmpty(phyTreeNode.LocationId))
                {
                    soTreeNode.FullPath = LocationManagementService.GetLocationPathById(new Guid(phyTreeNode.LocationId));
                    soTreeNode.BoxId = phyTreeNode.BoxId;
                }
            }
            return soTreeNode;
        }

        public RMPhysicalExplorerNode ConvertSoPhyTreeToRMTreeNode(PhysicalDestTree soTreeNode)
        {
            RMPhysicalExplorerNode rmTreeNode = new RMPhysicalExplorerNode();
            rmTreeNode.LocationId = soTreeNode.LocationId;
            rmTreeNode.LocationName = soTreeNode.FullPath;
            rmTreeNode.BoxId = soTreeNode.BoxId;
            rmTreeNode.FileId = soTreeNode.FileId;
            return rmTreeNode;
        }
        public void ConvertToRMMoveSettings(Rule rule, RMRuleInfos info)
        {
            if (info.RuleCretias != null && info.RuleCretias.Count > 0)
            {
                info.IsSpSource = true;
            }
            if (info.EXORule != null)
            {
                info.IsExoSource = true;
            }
            if (info.PhysicalRule != null)
            {
                info.IsPhySource = true;
            }
            if (info.SPLocalRule != null)
            {
                info.IsSPLocalSource = true;
            }
            if (info.OneDriveRule != null)
            {
                info.IsOneDriveSource = true;
            }
            if (info.AzureFileRule != null)
            {
                info.IsAzureFileSource = true;
            }
            if (info.BoxRule != null)
            {
                info.IsBoxSource = true;
            }
            if (info.ConnectorRule != null)
            {
                info.IsConnectorSource = true;
            }
            if (info.GoogleDriveRule != null)
            {
                info.IsGoogleDriveSource = true;
            }
            if(info.TeamsRule != null)
            {
                info.IsTeamsSource = true;
            }
            #region SPOnline
            if (info.IsSpSource && rule.spMoveOption != null && rule.spMoveOption.MoveDestination != null)
            {
                info.MoveDto = new MoveToDto();
                rule.spMoveOption.SourceFlag = RecordFlag.SP;
                rule.spMoveOption.DestFlag = RecordFlag.SP;
                info.MoveDto.NotDeclareMovedData = rule.spMoveOption.MoveDestination.NotDeclareMovedData;
                info.MoveDto.isKeepClassification = rule.spMoveOption.MoveDestination.KeepSourceClassification;
                info.MoveDto.IsKeepFolderStructure = rule.spMoveOption.MoveDestination.KeepFolderStructure;
                info.MoveDto.IsMoveAllVersions = rule.spMoveOption.MoveDestination.IsMoveVersions;
                if (rule.spMoveOption.MoveDestination.DestMode == AvePoint.GCommon.Contract.StorageOptimization.Object.DestMode.UrlMode)
                {
                    info.MoveDto.IsSpecifyLocation = true;
                    info.MoveDto.LocationPath = rule.spMoveOption.MoveDestination.SPUrl;
                }
                else
                {
                    info.MoveDto.IsSpecifyLocation = false;
                    //info.MoveDto.SPTree = RMDtoConverter.ConvertSPTree2RMTree(rule.spMoveOption.MoveDestination.SPTreeNode);
                    if (!string.IsNullOrEmpty(rule.spMoveOption.MoveDestination.SPTreeStr))
                    {
                        RMSPTreeNode farmNode = SerializerHelper.DeserializeByDataContractSerializer<RMSPTreeNode>(rule.spMoveOption.MoveDestination.SPTreeStr);
                        var remoteNode = RemoteNodeDao.GetRemoteNodeById(new Guid(rule.spMoveOption.MoveDestination.ContainerId));
                        bool hasTeamsFeature = RMKeyValueDao.HasUpgradeTeams();
                        if (farmNode != null
                            && hasTeamsFeature
                            && farmNode.Type != ContentSourceType.Teams
                            && remoteNode != null
                            && (remoteNode.NodeLevel == (int)NodeLevel.O365GroupSitesGroup || remoteNode.NodeLevel == (int)NodeLevel.PrivateChannelGroup)
                            )
                        {
                            info.MoveDto.IsSpecifyLocation = true;
                        }
                        else
                        {
                            if (farmNode != null && farmNode.Type == ContentSourceType.Teams && !hasTeamsFeature)
                            {
                                info.MoveDto.IsSpecifyLocation = true;
                            }
                            else
                            {
                                info.MoveDto.SPTreeStr = RuleSPTreeUtil.ConvertXmlStrToSPTreeJsonStr(rule.spMoveOption.MoveDestination.SPTreeStr);
                            }
                        }
                    }
                    info.MoveDto.LocationPath = rule.spMoveOption.MoveDestination.SPUrl;
                }

                #region Move Settings
                info.MoveDto.DestMode = rule.spMoveOption.MoveSetting.ConflictType == ConflictType.SharePointConflict ? Contract.RMWeb.DestMode.SharePoint : Contract.RMWeb.DestMode.FileSystem;
                if (rule.spMoveOption.MoveSetting.ContainerLevelConflictOption == ConflictOption.Merge)
                {
                    info.MoveDto.FolderNameConflictOption = FolderNameConflictOption.Merge;
                    switch (rule.spMoveOption.MoveSetting.ItemLevelConflictOption)
                    {
                        case ConflictOption.Skip:
                            info.MoveDto.FolderFilesNameConflictOption = FileNameConflictOption.Skip;
                            break;
                        case ConflictOption.Overwrite:
                            info.MoveDto.FolderFilesNameConflictOption = FileNameConflictOption.Overwrite;
                            break;
                        case ConflictOption.AppendByName:
                            info.MoveDto.FolderFilesNameConflictOption = FileNameConflictOption.Rename;
                            break;
                        default:
                            break;
                    }
                }
                else if (rule.spMoveOption.MoveSetting.ContainerLevelConflictOption == ConflictOption.Skip)
                {
                    info.MoveDto.FolderNameConflictOption = FolderNameConflictOption.Skip;
                }



                if (info.MoveDto.FileNameConflictOption == FileNameConflictOption.Skip)
                {
                    rule.spMoveOption.MoveSetting.ItemLevelConflictOption = ConflictOption.Skip;
                }
                else if (info.MoveDto.FileNameConflictOption == FileNameConflictOption.Overwrite)
                {
                    rule.spMoveOption.MoveSetting.ItemLevelConflictOption = ConflictOption.Overwrite;
                }
                else if (info.MoveDto.FileNameConflictOption == FileNameConflictOption.Rename)
                {
                    rule.spMoveOption.MoveSetting.ItemLevelConflictOption = ConflictOption.AppendByName;
                }


                if (rule.spMoveOption.MoveSetting.ItemLevelConflictOption == ConflictOption.Skip)
                {
                    info.MoveDto.FileNameConflictOption = FileNameConflictOption.Skip;
                }
                else if (rule.spMoveOption.MoveSetting.ItemLevelConflictOption == ConflictOption.Overwrite)
                {
                    info.MoveDto.FileNameConflictOption = FileNameConflictOption.Overwrite;
                }
                else if (rule.spMoveOption.MoveSetting.ItemLevelConflictOption == ConflictOption.AppendByName)
                {
                    info.MoveDto.FileNameConflictOption = FileNameConflictOption.Rename;
                }
                #endregion
            }

            if (info.IsSpSource)
            {
                ConvertToExportLibrary(rule, info);
            }
            
            #endregion
            #region File System
            if (info.FSRule != null)
            {
                if (info.FSRule.RuleCretias != null && info.FSRule.RuleCretias.Count > 0)
                {
                    info.IsFSSource = true;//TO DO FS
                }
                if (info.IsFSSource && rule.FSRule.spMoveOption != null && rule.FSRule.spMoveOption.MoveDestination != null)
                {
                    info.FSRule.MoveDto = new MoveToDto();

                    info.FSRule.MoveDto.NotDeclareMovedData = rule.FSRule.spMoveOption.MoveDestination.NotDeclareMovedData;
                    rule.FSRule.spMoveOption.SourceFlag = RecordFlag.FS;
                    rule.FSRule.spMoveOption.DestFlag = RecordFlag.FS;
                    if (rule.FSRule.spMoveOption.MoveDestination.DestMode == AvePoint.GCommon.Contract.StorageOptimization.Object.DestMode.UrlMode)
                    {
                        info.FSRule.MoveDto.IsSpecifyLocation = true;
                        info.FSRule.MoveDto.LocationPath = rule.FSRule.spMoveOption.MoveDestination.FSPath;
                    }
                    else
                    {
                        info.FSRule.MoveDto.IsSpecifyLocation = false;
                        //info.FSRule.MoveDto.FSTree = RMDtoConverter.ConvertFSTree2RMTree(rule.FSRule.spMoveOption.MoveDestination.FSTreeNode);
                        if (!string.IsNullOrEmpty(rule.FSRule.spMoveOption.MoveDestination.FSTreeStr))
                        {
                            info.FSRule.MoveDto.FSTreeStr = RuleSPTreeUtil.ConvertXmlStrToFSTreeJsonStr(rule.FSRule.spMoveOption.MoveDestination.FSTreeStr);
                        }
                        if (!string.IsNullOrEmpty(rule.FSRule.spMoveOption.MoveDestination.FSPath))
                        {
                            //老数据FSPath没有值
                            info.FSRule.MoveDto.LocationPath = rule.FSRule.spMoveOption.MoveDestination.FSPath;
                        }
                        else if (info.FSRule.MoveDto.FSTree != null && !string.IsNullOrEmpty(info.FSRule.MoveDto.FSTree.FullPath))
                        {
                            info.FSRule.MoveDto.LocationPath = EncodeUtil.DecryptByCommunicationKey(info.FSRule.MoveDto.FSTree.FullPath);
                        }
                    }
                    #region Move Settings
                    info.FSRule.MoveDto.DestMode = rule.FSRule.spMoveOption.MoveSetting.ConflictType == ConflictType.SharePointConflict ? Contract.RMWeb.DestMode.SharePoint : Contract.RMWeb.DestMode.FileSystem;
                    if (rule.FSRule.spMoveOption.MoveSetting.ContainerLevelConflictOption == ConflictOption.Merge)
                    {
                        info.FSRule.MoveDto.FolderNameConflictOption = FolderNameConflictOption.Merge;
                        switch (rule.FSRule.spMoveOption.MoveSetting.ItemLevelConflictOption)
                        {
                            case ConflictOption.Skip:
                                info.FSRule.MoveDto.FolderFilesNameConflictOption = FileNameConflictOption.Skip;
                                break;
                            case ConflictOption.Overwrite:
                                info.FSRule.MoveDto.FolderFilesNameConflictOption = FileNameConflictOption.Overwrite;
                                break;
                            case ConflictOption.AppendByName:
                                info.FSRule.MoveDto.FolderFilesNameConflictOption = FileNameConflictOption.Rename;
                                break;
                            default:
                                break;
                        }
                    }
                    else if (rule.FSRule.spMoveOption.MoveSetting.ContainerLevelConflictOption == ConflictOption.Skip)
                    {
                        info.FSRule.MoveDto.FolderNameConflictOption = FolderNameConflictOption.Skip;
                    }
                    //TO DO FS
                    //if (info.MoveDto.FileNameConflictOption == FileNameConflictOption.Skip)
                    //{
                    //    rule.FSRule.spMoveOption.MoveSetting.ItemLevelConflictOption = ConflictOption.Skip;
                    //}
                    //else if (info.MoveDto.FileNameConflictOption == FileNameConflictOption.Overwrite)
                    //{
                    //    rule.FSRule.spMoveOption.MoveSetting.ItemLevelConflictOption = ConflictOption.Overwrite;
                    //}
                    //else if (info.MoveDto.FileNameConflictOption == FileNameConflictOption.Rename)
                    //{
                    //    rule.FSRule.spMoveOption.MoveSetting.ItemLevelConflictOption = ConflictOption.AppendByName;
                    //}


                    if (rule.FSRule.spMoveOption.MoveSetting.ItemLevelConflictOption == ConflictOption.Skip)
                    {
                        info.FSRule.MoveDto.FileNameConflictOption = FileNameConflictOption.Skip;
                    }
                    else if (rule.FSRule.spMoveOption.MoveSetting.ItemLevelConflictOption == ConflictOption.Overwrite)
                    {
                        info.FSRule.MoveDto.FileNameConflictOption = FileNameConflictOption.Overwrite;
                    }
                    else if (rule.FSRule.spMoveOption.MoveSetting.ItemLevelConflictOption == ConflictOption.AppendByName)
                    {
                        info.FSRule.MoveDto.FileNameConflictOption = FileNameConflictOption.Rename;
                    }
                    #endregion
                }
            }
            #endregion
            #region AzureFile
            if (info.AzureFileRule != null)
            {
                if (info.AzureFileRule.RuleCretias != null && info.AzureFileRule.RuleCretias.Count > 0)
                {
                    info.IsAzureFileSource = true;//TO DO FS
                }
                //if (info.AzureFileRule != null && rule.AzureFileRule.spMoveOption != null && rule.AzureFileRule.spMoveOption.MoveDestination != null)
                //{
                //    info.AzureFileRule.MoveDto = new MoveToDto();

                //    info.AzureFileRule.MoveDto.NotDeclareMovedData = rule.AzureFileRule.spMoveOption.MoveDestination.NotDeclareMovedData;
                //    rule.AzureFileRule.spMoveOption.SourceFlag = RecordFlag.FS;
                //    rule.AzureFileRule.spMoveOption.DestFlag = RecordFlag.FS;
                //    if (rule.AzureFileRule.spMoveOption.MoveDestination.DestMode == AvePoint.GCommon.Contract.StorageOptimization.Object.DestMode.UrlMode)
                //    {
                //        info.AzureFileRule.MoveDto.IsSpecifyLocation = true;
                //        info.AzureFileRule.MoveDto.LocationPath = rule.AzureFileRule.spMoveOption.MoveDestination.FSPath;
                //    }
                //    else
                //    {
                //        info.AzureFileRule.MoveDto.IsSpecifyLocation = false;
                //        info.AzureFileRule.MoveDto.FSTree = RMDtoConverter.ConvertFSTree2RMTree(rule.AzureFileRule.spMoveOption.MoveDestination.FSTreeNode);
                //        if (!string.IsNullOrEmpty(rule.AzureFileRule.spMoveOption.MoveDestination.FSTreeStr))
                //        {
                //            info.AzureFileRule.MoveDto.FSTreeStr = RuleSPTreeUtil.ConvertXmlStrToFSTreeJsonStr(rule.AzureFileRule.spMoveOption.MoveDestination.FSTreeStr);
                //        }
                //        if (!rule.AzureFileRule.spMoveOption.MoveDestination.FSPath.IsNullOrEmpty())
                //        {
                //            //老数据FSPath没有值
                //            info.AzureFileRule.MoveDto.LocationPath = rule.AzureFileRule.spMoveOption.MoveDestination.FSPath;
                //        }
                //        else if (info.AzureFileRule.MoveDto.FSTree != null && !info.AzureFileRule.MoveDto.FSTree.FullPath.IsNullOrEmpty())
                //        {
                //            info.AzureFileRule.MoveDto.LocationPath = EncodeUtil.DecryptByCommunicationKey(info.AzureFileRule.MoveDto.FSTree.FullPath);
                //        }
                //    }
                //    #region Move Settings
                //    info.AzureFileRule.MoveDto.DestMode = rule.AzureFileRule.spMoveOption.MoveSetting.ConflictType == ConflictType.SharePointConflict ? Contract.RMWeb.DestMode.SharePoint : Contract.RMWeb.DestMode.FileSystem;
                //    if (rule.AzureFileRule.spMoveOption.MoveSetting.ContainerLevelConflictOption == ConflictOption.Merge)
                //    {
                //        info.AzureFileRule.MoveDto.FolderNameConflictOption = FolderNameConflictOption.Merge;
                //        switch (rule.AzureFileRule.spMoveOption.MoveSetting.ItemLevelConflictOption)
                //        {
                //            case ConflictOption.Skip:
                //                info.AzureFileRule.MoveDto.FolderFilesNameConflictOption = FileNameConflictOption.Skip;
                //                break;
                //            case ConflictOption.Overwrite:
                //                info.AzureFileRule.MoveDto.FolderFilesNameConflictOption = FileNameConflictOption.Overwrite;
                //                break;
                //            case ConflictOption.AppendByName:
                //                info.AzureFileRule.MoveDto.FolderFilesNameConflictOption = FileNameConflictOption.Rename;
                //                break;
                //            default:
                //                break;
                //        }
                //    }
                //    else if (rule.AzureFileRule.spMoveOption.MoveSetting.ContainerLevelConflictOption == ConflictOption.Skip)
                //    {
                //        info.AzureFileRule.MoveDto.FolderNameConflictOption = FolderNameConflictOption.Skip;
                //    }
                //    //TO DO FS
                //    //if (info.MoveDto.FileNameConflictOption == FileNameConflictOption.Skip)
                //    //{
                //    //    rule.FSRule.spMoveOption.MoveSetting.ItemLevelConflictOption = ConflictOption.Skip;
                //    //}
                //    //else if (info.MoveDto.FileNameConflictOption == FileNameConflictOption.Overwrite)
                //    //{
                //    //    rule.FSRule.spMoveOption.MoveSetting.ItemLevelConflictOption = ConflictOption.Overwrite;
                //    //}
                //    //else if (info.MoveDto.FileNameConflictOption == FileNameConflictOption.Rename)
                //    //{
                //    //    rule.FSRule.spMoveOption.MoveSetting.ItemLevelConflictOption = ConflictOption.AppendByName;
                //    //}


                //    if (rule.AzureFileRule.spMoveOption.MoveSetting.ItemLevelConflictOption == ConflictOption.Skip)
                //    {
                //        info.AzureFileRule.MoveDto.FileNameConflictOption = FileNameConflictOption.Skip;
                //    }
                //    else if (rule.AzureFileRule.spMoveOption.MoveSetting.ItemLevelConflictOption == ConflictOption.Overwrite)
                //    {
                //        info.AzureFileRule.MoveDto.FileNameConflictOption = FileNameConflictOption.Overwrite;
                //    }
                //    else if (rule.AzureFileRule.spMoveOption.MoveSetting.ItemLevelConflictOption == ConflictOption.AppendByName)
                //    {
                //        info.AzureFileRule.MoveDto.FileNameConflictOption = FileNameConflictOption.Rename;
                //    }
                //    #endregion
                //}
            }
            #endregion

            #region Box
            if (info.BoxRule != null)
            {
                if (info.BoxRule.RuleCretias != null && info.BoxRule.RuleCretias.Count > 0)
                {
                    info.IsBoxSource = true;
                }
            }
            #endregion

            #region Physical
            if (info.IsPhySource && rule.PhysicalRule.spMoveOption != null && rule.PhysicalRule.spMoveOption.MoveDestination != null)
            {
                info.PhysicalRule.MoveDto = new MoveToDto();
                info.PhysicalRule.MoveDto.IsSpecifyLocation = false;
                info.PhysicalRule.MoveDto.PhysicalTreeStr = rule.PhysicalRule.spMoveOption.MoveDestination.PhysicalTreeStr;
                info.PhysicalRule.MoveDto.PhysicalTreeNode = ConvertSoPhyTreeToRMTreeNode(rule.PhysicalRule.spMoveOption.MoveDestination.PhysicalTree);
                rule.PhysicalRule.spMoveOption.SourceFlag = RecordFlag.Physical;
                rule.PhysicalRule.spMoveOption.DestFlag = RecordFlag.Physical;
                if (rule.PhysicalRule.spMoveOption.MoveSetting.ItemLevelConflictOption == ConflictOption.Skip)
                {
                    info.PhysicalRule.MoveDto.FileNameConflictOption = FileNameConflictOption.Skip;
                }
                else if (rule.PhysicalRule.spMoveOption.MoveSetting.ItemLevelConflictOption == ConflictOption.Overwrite)
                {
                    info.PhysicalRule.MoveDto.FileNameConflictOption = FileNameConflictOption.Overwrite;
                }
                else if (rule.PhysicalRule.spMoveOption.MoveSetting.ItemLevelConflictOption == ConflictOption.AppendByName)
                {
                    info.PhysicalRule.MoveDto.FileNameConflictOption = FileNameConflictOption.Rename;
                }

                if (rule.PhysicalRule.spMoveOption.MoveSetting.PhysicalHoldConflictOption == PhysicalHoldConflictOption.CompareHoldSetting)
                {
                    info.PhysicalRule.MoveDto.MoveHoldConflictOption = MoveHoldConflictOption.Compare;
                }
                else
                {
                    info.PhysicalRule.MoveDto.MoveHoldConflictOption = MoveHoldConflictOption.Current;
                }
            }
            #endregion
            #region EXO
            if (info.IsExoSource && rule.EXORule.spMoveOption != null && rule.EXORule.spMoveOption.MoveDestination != null)
            {
                info.EXORule.MoveDto = new MoveToDto();
                rule.EXORule.spMoveOption.SourceFlag = RecordFlag.EXO;
                rule.EXORule.spMoveOption.DestFlag = RecordFlag.SP;
                info.EXORule.MoveDto.NotDeclareMovedData = rule.EXORule.spMoveOption.MoveDestination.NotDeclareMovedData;
                info.EXORule.MoveDto.IsDeleteSourceItem = rule.EXORule.spMoveOption.MoveDestination.DeleteSourceItem;
                info.EXORule.MoveDto.isKeepClassification = rule.EXORule.spMoveOption.MoveDestination.KeepSourceClassification;
                info.EXORule.MoveDto.IsMoveToSP = rule.EXORule.spMoveOption.IsMoveToSP;
                info.EXORule.MoveDto.MoveToSPDataList = rule.EXORule.spMoveOption.MoveToSPDataList ?? new List<MoveMetadataInfo>();
                if (rule.EXORule.spMoveOption.MoveDestination.DestMode == AvePoint.GCommon.Contract.StorageOptimization.Object.DestMode.UrlMode)
                {
                    info.EXORule.MoveDto.IsSpecifyLocation = true;
                    info.EXORule.MoveDto.LocationPath = rule.EXORule.spMoveOption.MoveDestination.SPUrl;
                }
                else
                {
                    info.EXORule.MoveDto.IsSpecifyLocation = false;
                    //info.MoveDto.SPTree = RMDtoConverter.ConvertSPTree2RMTree(rule.spMoveOption.MoveDestination.SPTreeNode);
                    if (!string.IsNullOrEmpty(rule.EXORule.spMoveOption.MoveDestination.SPTreeStr))
                    {
                        bool hasTeamsFeature = RMKeyValueDao.HasUpgradeTeams();
                        RMSPTreeNode farmNode = SerializerHelper.DeserializeByDataContractSerializer<RMSPTreeNode>(rule.EXORule.spMoveOption.MoveDestination.SPTreeStr);
                        var remoteNode = RemoteNodeDao.GetRemoteNodeById(new Guid(rule.EXORule.spMoveOption.MoveDestination.ContainerId));
                        if (farmNode != null
                            && farmNode.Type != ContentSourceType.Teams
                            && hasTeamsFeature
                            && remoteNode != null
                            && (remoteNode.NodeLevel == (int)NodeLevel.O365GroupSitesGroup || remoteNode.NodeLevel == (int)NodeLevel.PrivateChannelGroup)
                            )
                        {
                            info.EXORule.MoveDto.IsSpecifyLocation = true;
                        }
                        else
                        {
                            if (farmNode != null && farmNode.Type == ContentSourceType.Teams && !hasTeamsFeature)
                            {
                                info.EXORule.MoveDto.IsSpecifyLocation = true;
                            }
                            else
                            {
                                info.EXORule.MoveDto.SPTreeStr = RuleSPTreeUtil.ConvertXmlStrToSPTreeJsonStr(rule.EXORule.spMoveOption.MoveDestination.SPTreeStr);
                            }
                        }
                    }
                    info.EXORule.MoveDto.LocationPath = rule.EXORule.spMoveOption.MoveDestination.SPUrl;
                }

                #region Move Settings
                info.EXORule.MoveDto.DestMode = rule.EXORule.spMoveOption.MoveSetting.ConflictType == ConflictType.SharePointConflict ? Contract.RMWeb.DestMode.SharePoint : Contract.RMWeb.DestMode.FileSystem;
                if (rule.EXORule.spMoveOption.MoveSetting.ContainerLevelConflictOption == ConflictOption.Merge)
                {
                    info.EXORule.MoveDto.FolderNameConflictOption = FolderNameConflictOption.Merge;
                    switch (rule.EXORule.spMoveOption.MoveSetting.ItemLevelConflictOption)
                    {
                        case ConflictOption.Skip:
                            info.EXORule.MoveDto.FolderFilesNameConflictOption = FileNameConflictOption.Skip;
                            break;
                        case ConflictOption.Overwrite:
                            info.EXORule.MoveDto.FolderFilesNameConflictOption = FileNameConflictOption.Overwrite;
                            break;
                        case ConflictOption.AppendByName:
                            info.EXORule.MoveDto.FolderFilesNameConflictOption = FileNameConflictOption.Rename;
                            break;
                        default:
                            break;
                    }
                }
                else if (rule.EXORule.spMoveOption.MoveSetting.ContainerLevelConflictOption == ConflictOption.Skip)
                {
                    info.EXORule.MoveDto.FolderNameConflictOption = FolderNameConflictOption.Skip;
                }



                if (info.EXORule.MoveDto.FileNameConflictOption == FileNameConflictOption.Skip)
                {
                    rule.EXORule.spMoveOption.MoveSetting.ItemLevelConflictOption = ConflictOption.Skip;
                }
                else if (info.EXORule.MoveDto.FileNameConflictOption == FileNameConflictOption.Overwrite)
                {
                    rule.EXORule.spMoveOption.MoveSetting.ItemLevelConflictOption = ConflictOption.Overwrite;
                }
                else if (info.EXORule.MoveDto.FileNameConflictOption == FileNameConflictOption.Rename)
                {
                    rule.EXORule.spMoveOption.MoveSetting.ItemLevelConflictOption = ConflictOption.AppendByName;
                }


                if (rule.EXORule.spMoveOption.MoveSetting.ItemLevelConflictOption == ConflictOption.Skip)
                {
                    info.EXORule.MoveDto.FileNameConflictOption = FileNameConflictOption.Skip;
                }
                else if (rule.EXORule.spMoveOption.MoveSetting.ItemLevelConflictOption == ConflictOption.Overwrite)
                {
                    info.EXORule.MoveDto.FileNameConflictOption = FileNameConflictOption.Overwrite;
                }
                else if (rule.EXORule.spMoveOption.MoveSetting.ItemLevelConflictOption == ConflictOption.AppendByName)
                {
                    info.EXORule.MoveDto.FileNameConflictOption = FileNameConflictOption.Rename;
                }
                #endregion
            }

            if (info.IsExoSource)
            {
                ConvertToExportLibrary(rule.EXORule, info.EXORule);
            }
           
            #endregion
            #region SPOnPremise
            if (info.IsSPLocalSource && rule.SPLocalRule.spMoveOption != null && rule.SPLocalRule.spMoveOption.MoveDestination != null)
            {
                info.SPLocalRule.MoveDto = new MoveToDto();
                rule.SPLocalRule.spMoveOption.SourceFlag = RecordFlag.SPLocal;
                rule.SPLocalRule.spMoveOption.DestFlag = RecordFlag.SPLocal;
                info.SPLocalRule.MoveDto.NotDeclareMovedData = rule.SPLocalRule.spMoveOption.MoveDestination.NotDeclareMovedData;
                info.SPLocalRule.MoveDto.IsDeleteSourceItem = rule.SPLocalRule.spMoveOption.MoveDestination.DeleteSourceItem;
                info.SPLocalRule.MoveDto.isKeepClassification = rule.SPLocalRule.spMoveOption.MoveDestination.KeepSourceClassification;
                if (rule.SPLocalRule.spMoveOption.MoveDestination.DestMode == AvePoint.GCommon.Contract.StorageOptimization.Object.DestMode.UrlMode)
                {
                    info.SPLocalRule.MoveDto.IsSpecifyLocation = true;
                    info.SPLocalRule.MoveDto.LocationPath = rule.SPLocalRule.spMoveOption.MoveDestination.SPUrl;
                }
                else
                {
                    info.SPLocalRule.MoveDto.IsSpecifyLocation = false;
                    //info.MoveDto.SPTree = RMDtoConverter.ConvertSPTree2RMTree(rule.spMoveOption.MoveDestination.SPTreeNode);
                    if (!string.IsNullOrEmpty(rule.SPLocalRule.spMoveOption.MoveDestination.SPTreeStr))
                    {
                        info.SPLocalRule.MoveDto.SPTreeStr = RuleSPTreeUtil.ConvertXmlStrToSPTreeJsonStr(rule.SPLocalRule.spMoveOption.MoveDestination.SPTreeStr);
                    }
                    info.SPLocalRule.MoveDto.LocationPath = rule.SPLocalRule.spMoveOption.MoveDestination.SPUrl;
                }

                #region Move Settings
                info.SPLocalRule.MoveDto.DestMode = rule.SPLocalRule.spMoveOption.MoveSetting.ConflictType == ConflictType.SharePointConflict ? Contract.RMWeb.DestMode.SharePoint : Contract.RMWeb.DestMode.FileSystem;
                if (rule.SPLocalRule.spMoveOption.MoveSetting.ContainerLevelConflictOption == ConflictOption.Merge)
                {
                    info.SPLocalRule.MoveDto.FolderNameConflictOption = FolderNameConflictOption.Merge;
                    switch (rule.SPLocalRule.spMoveOption.MoveSetting.ItemLevelConflictOption)
                    {
                        case ConflictOption.Skip:
                            info.SPLocalRule.MoveDto.FolderFilesNameConflictOption = FileNameConflictOption.Skip;
                            break;
                        case ConflictOption.Overwrite:
                            info.SPLocalRule.MoveDto.FolderFilesNameConflictOption = FileNameConflictOption.Overwrite;
                            break;
                        case ConflictOption.AppendByName:
                            info.SPLocalRule.MoveDto.FolderFilesNameConflictOption = FileNameConflictOption.Rename;
                            break;
                        default:
                            break;
                    }
                }
                else if (rule.SPLocalRule.spMoveOption.MoveSetting.ContainerLevelConflictOption == ConflictOption.Skip)
                {
                    info.SPLocalRule.MoveDto.FolderNameConflictOption = FolderNameConflictOption.Skip;
                }



                if (info.SPLocalRule.MoveDto.FileNameConflictOption == FileNameConflictOption.Skip)
                {
                    rule.SPLocalRule.spMoveOption.MoveSetting.ItemLevelConflictOption = ConflictOption.Skip;
                }
                else if (info.SPLocalRule.MoveDto.FileNameConflictOption == FileNameConflictOption.Overwrite)
                {
                    rule.SPLocalRule.spMoveOption.MoveSetting.ItemLevelConflictOption = ConflictOption.Overwrite;
                }
                else if (info.SPLocalRule.MoveDto.FileNameConflictOption == FileNameConflictOption.Rename)
                {
                    rule.SPLocalRule.spMoveOption.MoveSetting.ItemLevelConflictOption = ConflictOption.AppendByName;
                }


                if (rule.SPLocalRule.spMoveOption.MoveSetting.ItemLevelConflictOption == ConflictOption.Skip)
                {
                    info.SPLocalRule.MoveDto.FileNameConflictOption = FileNameConflictOption.Skip;
                }
                else if (rule.SPLocalRule.spMoveOption.MoveSetting.ItemLevelConflictOption == ConflictOption.Overwrite)
                {
                    info.SPLocalRule.MoveDto.FileNameConflictOption = FileNameConflictOption.Overwrite;
                }
                else if (rule.SPLocalRule.spMoveOption.MoveSetting.ItemLevelConflictOption == ConflictOption.AppendByName)
                {
                    info.SPLocalRule.MoveDto.FileNameConflictOption = FileNameConflictOption.Rename;
                }
                #endregion
            }
            
            #endregion
            #region OneDrive
            if (info.IsOneDriveSource && rule.OneDriveRule.spMoveOption != null && rule.OneDriveRule.spMoveOption.MoveDestination != null)
            {
                info.OneDriveRule.MoveDto = new MoveToDto();
                rule.OneDriveRule.spMoveOption.SourceFlag = RecordFlag.SP;
                rule.OneDriveRule.spMoveOption.DestFlag = RecordFlag.SP;
                info.OneDriveRule.MoveDto.NotDeclareMovedData = rule.OneDriveRule.spMoveOption.MoveDestination.NotDeclareMovedData;
                info.OneDriveRule.MoveDto.isKeepClassification = rule.OneDriveRule.spMoveOption.MoveDestination.KeepSourceClassification;
                info.OneDriveRule.MoveDto.IsKeepFolderStructure = rule.OneDriveRule.spMoveOption.MoveDestination.KeepFolderStructure;
                info.OneDriveRule.MoveDto.IsMoveAllVersions = rule.OneDriveRule.spMoveOption.MoveDestination.IsMoveVersions;
                if (rule.OneDriveRule.spMoveOption.MoveDestination.DestMode == AvePoint.GCommon.Contract.StorageOptimization.Object.DestMode.UrlMode)
                {
                    info.OneDriveRule.MoveDto.IsSpecifyLocation = true;
                    info.OneDriveRule.MoveDto.LocationPath = rule.OneDriveRule.spMoveOption.MoveDestination.SPUrl;
                }
                else
                {
                    info.OneDriveRule.MoveDto.IsSpecifyLocation = false;
                    if (!string.IsNullOrEmpty(rule.OneDriveRule.spMoveOption.MoveDestination.SPTreeStr))
                    {
                        bool hasTeamsFeature = RMKeyValueDao.HasUpgradeTeams();
                        RMSPTreeNode farmNode = SerializerHelper.DeserializeByDataContractSerializer<RMSPTreeNode>(rule.OneDriveRule.spMoveOption.MoveDestination.SPTreeStr);
                        var remoteNode = RemoteNodeDao.GetRemoteNodeById(new Guid(rule.OneDriveRule.spMoveOption.MoveDestination.ContainerId));
                        if (farmNode != null
                            && farmNode.Type != ContentSourceType.Teams
                            && hasTeamsFeature
                            && remoteNode != null
                            && (remoteNode.NodeLevel == (int)NodeLevel.O365GroupSitesGroup || remoteNode.NodeLevel == (int)NodeLevel.PrivateChannelGroup)
                            )
                        {
                            info.OneDriveRule.MoveDto.IsSpecifyLocation = true;
                        }
                        else
                        {
                            if (farmNode != null && farmNode.Type == ContentSourceType.Teams && !hasTeamsFeature)
                            {
                                info.OneDriveRule.MoveDto.IsSpecifyLocation = true;
                            }
                            else
                            {
                                info.OneDriveRule.MoveDto.SPTreeStr = RuleSPTreeUtil.ConvertXmlStrToSPTreeJsonStr(rule.OneDriveRule.spMoveOption.MoveDestination.SPTreeStr);
                            }
                        }
                    }
                    info.OneDriveRule.MoveDto.LocationPath = rule.OneDriveRule.spMoveOption.MoveDestination.SPUrl;
                }

                #region Move Settings
                info.OneDriveRule.MoveDto.DestMode = rule.OneDriveRule.spMoveOption.MoveSetting.ConflictType == ConflictType.SharePointConflict ? Contract.RMWeb.DestMode.SharePoint : Contract.RMWeb.DestMode.FileSystem;
                if (rule.OneDriveRule.spMoveOption.MoveSetting.ContainerLevelConflictOption == ConflictOption.Merge)
                {
                    info.OneDriveRule.MoveDto.FolderNameConflictOption = FolderNameConflictOption.Merge;
                    switch (rule.OneDriveRule.spMoveOption.MoveSetting.ItemLevelConflictOption)
                    {
                        case ConflictOption.Skip:
                            info.OneDriveRule.MoveDto.FolderFilesNameConflictOption = FileNameConflictOption.Skip;
                            break;
                        case ConflictOption.Overwrite:
                            info.OneDriveRule.MoveDto.FolderFilesNameConflictOption = FileNameConflictOption.Overwrite;
                            break;
                        case ConflictOption.AppendByName:
                            info.OneDriveRule.MoveDto.FolderFilesNameConflictOption = FileNameConflictOption.Rename;
                            break;
                        default:
                            break;
                    }
                }
                else if (rule.OneDriveRule.spMoveOption.MoveSetting.ContainerLevelConflictOption == ConflictOption.Skip)
                {
                    info.OneDriveRule.MoveDto.FolderNameConflictOption = FolderNameConflictOption.Skip;
                }



                if (info.OneDriveRule.MoveDto.FileNameConflictOption == FileNameConflictOption.Skip)
                {
                    rule.OneDriveRule.spMoveOption.MoveSetting.ItemLevelConflictOption = ConflictOption.Skip;
                }
                else if (info.OneDriveRule.MoveDto.FileNameConflictOption == FileNameConflictOption.Overwrite)
                {
                    rule.OneDriveRule.spMoveOption.MoveSetting.ItemLevelConflictOption = ConflictOption.Overwrite;
                }
                else if (info.OneDriveRule.MoveDto.FileNameConflictOption == FileNameConflictOption.Rename)
                {
                    rule.OneDriveRule.spMoveOption.MoveSetting.ItemLevelConflictOption = ConflictOption.AppendByName;
                }


                if (rule.OneDriveRule.spMoveOption.MoveSetting.ItemLevelConflictOption == ConflictOption.Skip)
                {
                    info.OneDriveRule.MoveDto.FileNameConflictOption = FileNameConflictOption.Skip;
                }
                else if (rule.OneDriveRule.spMoveOption.MoveSetting.ItemLevelConflictOption == ConflictOption.Overwrite)
                {
                    info.OneDriveRule.MoveDto.FileNameConflictOption = FileNameConflictOption.Overwrite;
                }
                else if (rule.OneDriveRule.spMoveOption.MoveSetting.ItemLevelConflictOption == ConflictOption.AppendByName)
                {
                    info.OneDriveRule.MoveDto.FileNameConflictOption = FileNameConflictOption.Rename;
                }
                #endregion
            }

            if (info.IsOneDriveSource)
            {
                ConvertToExportLibrary(rule.OneDriveRule, info.OneDriveRule);
            }
            
            #endregion
            #region GoogleDrive

            if (info.IsGoogleDriveSource && rule.GoogleDriveRule.spMoveOption is { MoveDestination: not null })
            {
                info.GoogleDriveRule.MoveDto = new MoveToDto();
                info.GoogleDriveRule.MoveDto.IsSpecifyLocation = false;
                //info.MoveDto.SPTree = RMDtoConverter.ConvertSPTree2RMTree(rule.spMoveOption.MoveDestination.SPTreeNode);
                if (!string.IsNullOrEmpty(rule.GoogleDriveRule.spMoveOption.MoveDestination.GoogleTreeStr))
                {
                    info.GoogleDriveRule.MoveDto.GoogleTreeStr =
                        RuleSPTreeUtil.ConvertXmlStrToGoogleTreeJsonStr(rule.GoogleDriveRule.spMoveOption.MoveDestination.GoogleTreeStr);
                }

                info.GoogleDriveRule.MoveDto.LocationPath = rule.GoogleDriveRule.spMoveOption.MoveDestination.DestinationId;
                info.GoogleDriveRule.MoveDto.GoogleTree = RMDtoConverter.ConvertRMGoogleDto2TreeNode(rule.GoogleDriveRule.spMoveOption.MoveDestination.GoogleTreeNode);
            }

            #endregion

            #region Teams
            if (info.IsTeamsSource && rule.TeamsRule.spMoveOption != null && rule.TeamsRule.spMoveOption.MoveDestination != null)
            {
                ConvertTeamsToRMMoveSettings(rule.TeamsRule, info.TeamsRule);
            }
            if (info.IsTeamsSource)
            {
                ConvertToExportLibrary(rule.TeamsRule, info.TeamsRule);
            }
            #endregion
        }

        private void ConvertTeamsToRMMoveSettings(Rule rule, RMRuleInfos info)
        {
            info.MoveDto = new MoveToDto();
            rule.spMoveOption.SourceFlag = RecordFlag.SP;
            rule.spMoveOption.DestFlag = RecordFlag.SP;
            info.MoveDto.NotDeclareMovedData = rule.spMoveOption.MoveDestination.NotDeclareMovedData;
            info.MoveDto.isKeepClassification = rule.spMoveOption.MoveDestination.KeepSourceClassification;
            info.MoveDto.IsKeepFolderStructure = rule.spMoveOption.MoveDestination.KeepFolderStructure;
            info.MoveDto.IsMoveAllVersions = rule.spMoveOption.MoveDestination.IsMoveVersions;
            if (rule.spMoveOption.MoveDestination.DestMode == AvePoint.GCommon.Contract.StorageOptimization.Object.DestMode.UrlMode)
            {
                info.MoveDto.IsSpecifyLocation = true;
                info.MoveDto.LocationPath = rule.spMoveOption.MoveDestination.SPUrl;
            }
            else
            {
                info.MoveDto.IsSpecifyLocation = false;
                //info.MoveDto.SPTree = RMDtoConverter.ConvertSPTree2RMTree(rule.spMoveOption.MoveDestination.SPTreeNode);
                if (!string.IsNullOrEmpty(rule.spMoveOption.MoveDestination.SPTreeStr))
                {
                    info.MoveDto.SPTreeStr = RuleSPTreeUtil.ConvertXmlStrToSPTreeJsonStr(rule.spMoveOption.MoveDestination.SPTreeStr);
                }
                info.MoveDto.LocationPath = rule.spMoveOption.MoveDestination.SPUrl;
            }

            #region Move Settings
            info.MoveDto.DestMode = rule.spMoveOption.MoveSetting.ConflictType == ConflictType.SharePointConflict ? Contract.RMWeb.DestMode.SharePoint : Contract.RMWeb.DestMode.FileSystem;
            if (rule.spMoveOption.MoveSetting.ContainerLevelConflictOption == ConflictOption.Merge)
            {
                info.MoveDto.FolderNameConflictOption = FolderNameConflictOption.Merge;
                switch (rule.spMoveOption.MoveSetting.ItemLevelConflictOption)
                {
                    case ConflictOption.Skip:
                        info.MoveDto.FolderFilesNameConflictOption = FileNameConflictOption.Skip;
                        break;
                    case ConflictOption.Overwrite:
                        info.MoveDto.FolderFilesNameConflictOption = FileNameConflictOption.Overwrite;
                        break;
                    case ConflictOption.AppendByName:
                        info.MoveDto.FolderFilesNameConflictOption = FileNameConflictOption.Rename;
                        break;
                    default:
                        break;
                }
            }
            else if (rule.spMoveOption.MoveSetting.ContainerLevelConflictOption == ConflictOption.Skip)
            {
                info.MoveDto.FolderNameConflictOption = FolderNameConflictOption.Skip;
            }



            if (info.MoveDto.FileNameConflictOption == FileNameConflictOption.Skip)
            {
                rule.spMoveOption.MoveSetting.ItemLevelConflictOption = ConflictOption.Skip;
            }
            else if (info.MoveDto.FileNameConflictOption == FileNameConflictOption.Overwrite)
            {
                rule.spMoveOption.MoveSetting.ItemLevelConflictOption = ConflictOption.Overwrite;
            }
            else if (info.MoveDto.FileNameConflictOption == FileNameConflictOption.Rename)
            {
                rule.spMoveOption.MoveSetting.ItemLevelConflictOption = ConflictOption.AppendByName;
            }


            if (rule.spMoveOption.MoveSetting.ItemLevelConflictOption == ConflictOption.Skip)
            {
                info.MoveDto.FileNameConflictOption = FileNameConflictOption.Skip;
            }
            else if (rule.spMoveOption.MoveSetting.ItemLevelConflictOption == ConflictOption.Overwrite)
            {
                info.MoveDto.FileNameConflictOption = FileNameConflictOption.Overwrite;
            }
            else if (rule.spMoveOption.MoveSetting.ItemLevelConflictOption == ConflictOption.AppendByName)
            {
                info.MoveDto.FileNameConflictOption = FileNameConflictOption.Rename;
            }
            #endregion

        }

        private void ConvertToExportLibrary(Rule rule, RMRuleInfos info)
        {
            if (rule.ExportInfo is { spMoveOption.MoveDestination: not null })
            {
                info.MoveDto = new()
                {
                    NotDeclareMovedData = rule.ExportInfo.spMoveOption.MoveDestination.NotDeclareMovedData,
                };
            
                if (rule.ExportInfo.spMoveOption.MoveDestination.DestMode == AvePoint.GCommon.Contract.StorageOptimization.Object.DestMode.UrlMode)
                {
                    info.MoveDto.IsSpecifyLocation = true;
                    info.MoveDto.LocationPath = rule.ExportInfo.spMoveOption.MoveDestination.SPUrl;
                }
                else
                {
                    info.MoveDto.IsSpecifyLocation = false;
                    //info.MoveDto.SPTree = RMDtoConverter.ConvertSPTree2RMTree(rule.spMoveOption.MoveDestination.SPTreeNode);
                    if (!string.IsNullOrEmpty(rule.ExportInfo.spMoveOption.MoveDestination.SPTreeStr))
                    {
                        RMSPTreeNode farmNode = SerializerHelper.DeserializeByDataContractSerializer<RMSPTreeNode>(rule.ExportInfo.spMoveOption.MoveDestination.SPTreeStr);
                        var remoteNode = RemoteNodeDao.GetRemoteNodeById(new Guid(rule.ExportInfo.spMoveOption.MoveDestination.ContainerId));
                        bool hasTeamsFeature = RMKeyValueDao.HasUpgradeTeams();
                        if (farmNode != null
                            && hasTeamsFeature
                            && farmNode.Type != ContentSourceType.Teams
                            && remoteNode != null
                            && (remoteNode.NodeLevel == (int)NodeLevel.O365GroupSitesGroup || remoteNode.NodeLevel == (int)NodeLevel.PrivateChannelGroup)
                            )
                        {
                            info.MoveDto.IsSpecifyLocation = true;
                        }
                        else
                        {
                            if (farmNode.Type == ContentSourceType.Teams && !hasTeamsFeature)
                            {
                                info.MoveDto.IsSpecifyLocation = true;
                            }
                            else
                            {
                                info.MoveDto.SPTreeStr = RuleSPTreeUtil.ConvertXmlStrToSPTreeJsonStr(rule.ExportInfo.spMoveOption.MoveDestination.SPTreeStr);
                            }
                        }
                    }
                    info.MoveDto.LocationPath = rule.ExportInfo.spMoveOption.MoveDestination.SPUrl;
                }
            }
        }
        private void ConvertToStorageInfoFromExportInfo(Rule rule, RMRuleInfos info)
        {
            #region SPOnline
            if (info.IsSpSource)
            {
                ConvertToStorageInfo(rule, info);
            }
            #endregion
            #region EXO
            if (info.IsExoSource)
            {
                ConvertToStorageInfo(rule.EXORule, info.EXORule);
            }
            #endregion
            #region OneDrive
            if (info.IsOneDriveSource)
            {
                ConvertToStorageInfo(rule.OneDriveRule, info.OneDriveRule);
            }
            #endregion
            #region Google Drive
            if (info.IsGoogleDriveSource)
            {
                ConvertToStorageInfo(rule.GoogleDriveRule, info.GoogleDriveRule);
            }
            #endregion
            #region Teams
            if (info.IsTeamsSource)
            {
                ConvertToStorageInfo(rule.TeamsRule, info.TeamsRule);
            }
            #endregion
        }

        private void ConvertToStorageInfo(Rule rule, RMRuleInfos info)
        {
            if (rule.ExportInfo is { newOptionsOfExportInfo: false })
            {
                SettingProfileDto mDto = new()
                {
                    Type = (int)SettingProfilesType.ExportLocationDevice,
                    Name = "UsingExportLocationDevice"
                };
                var dto = SettingProfileDao.Load(mDto);
                if (dto != null)
                {
                    info.ExportInfo.exportLocationId = dto.Settings;
                    info.ExportInfo.exportLocationName = StorageDeviceService.GetStorageDeviceNameById(dto.Settings);
                }
            }
        }
        public void ResetSOFilter(Rule rule)
        {
            if (rule.SOFilters != null)
            {
                string AndOrExpression = "(";
                for (int i = 0; i < rule.SOFilters.Count; i++)
                {
                    SOFilterPolicy filterDto = rule.SOFilters[i];
                    filterDto.Level = rule.PolicyLevel;
                    rule.SOFilters[i].SequenceNo = i + 1;
                    if (i == rule.SOFilters.Count - 1)
                    {
                        AndOrExpression += string.Format("{0}", filterDto.SequenceNo);
                    }
                    else
                    {
                        AndOrExpression += string.Format("{0} {1} ", filterDto.SequenceNo, filterDto.IsAnd ? "And" : "Or");
                    }
                }
                AndOrExpression += ")";
                rule.AndOrExpression = new Dictionary<PolicyLevel, string>()
                {
                    { rule.PolicyLevel, AndOrExpression }
                };
            }
        }
        public Rule BuildRMFSRule(RMRuleInfos info)
        {
            List<SOFilterPolicy> soFilters = new List<SOFilterPolicy>();

            foreach (var filter in info.RuleFilters)
            {
                ArchiverRuleFilter arFilter = new ArchiverRuleFilter();
                arFilter.CombineMode = filter.CombineMode;
                arFilter.SequenceNo = filter.SequenceNo;
                arFilter.Level = filter.Level;
                arFilter.Condition = filter.Condition;
                arFilter.RuleType = filter.RuleType;
                if (!string.IsNullOrEmpty(filter.filterName))
                {
                    arFilter.RuleName = filter.filterName;
                }

                if (arFilter.RuleType == ArchiverFilterRuleType.ModifiedTime ||
                    arFilter.RuleType == ArchiverFilterRuleType.CreatedTime ||
                    arFilter.RuleType == ArchiverFilterRuleType.LastAccessedTime ||
                    arFilter.RuleType == ArchiverFilterRuleType.DateTimeColumn ||
                    arFilter.RuleType == ArchiverFilterRuleType.DateTimeCustomProperty ||
                    arFilter.RuleType == ArchiverFilterRuleType.SendDateUTC)
                {
                    string startDayLightSaving = filter.StartTimeInfo == null ? "true" : filter.StartTimeInfo.IsDayLightSaving.ToString();
                    string endDayLightSaving = filter.EndTimeInfo == null ? "true" : filter.EndTimeInfo.IsDayLightSaving.ToString();
                    if (arFilter.Condition == ArchiverFilterCondition.FromTo)
                    {

                        DateTime startUtcTime = arFilter.SetDateTime(filter.Value1, filter.StartTimeInfo.TimeZoneId, startDayLightSaving, false);
                        DateTime endUtcTime = arFilter.SetDateTime(filter.Value2, filter.EndTimeInfo.TimeZoneId, endDayLightSaving, true);
                        if (DateTime.Parse(filter.Value1) >= DateTime.Parse(filter.Value2))
                        {
                            //throw new InvalidArgumentException(Messages.Get("start_date_after_end_date"));
                            throw new Exception("");
                        }
                        arFilter.Value1 = startUtcTime.ToString(APIDateTimeFormat.DATETYPEForAPI003);
                        arFilter.Value2 = endUtcTime.ToString(APIDateTimeFormat.DATETYPEForAPI003);
                    }
                    else if (arFilter.Condition == ArchiverFilterCondition.Before)
                    {
                        // ValidateValueCount(value, 3);
                        DateTime utcTime = arFilter.SetDateTime(filter.Value1, filter.StartTimeInfo.TimeZoneId, startDayLightSaving, false);
                        arFilter.Value1 = utcTime.ToString(APIDateTimeFormat.DATETYPEForAPI003);
                    }
                    else if (arFilter.Condition == ArchiverFilterCondition.OlderThan)
                    {
                        //ValidateValueCount(value, 1);
                        //SetValueForOlderThan(value[0]);
                        arFilter.Value1 = filter.Value1;
                        arFilter.Value1Unit = filter.Value1Unit;
                    }
                    soFilters.Add(arFilter.Dto);
                }
                else
                {
                    arFilter.Value1 = filter.Value1;
                    if (filter.RuleType == ArchiverFilterRuleType.DocumentSize || filter.RuleType == ArchiverFilterRuleType.SiteCollectionSizeTrigger
                        || filter.RuleType == ArchiverFilterRuleType.Size)
                    {
                        arFilter.Value1Unit = filter.Value1Unit;
                        arFilter.Value2Unit = filter.Value2Unit;
                    }
                    else if (filter.RuleType == ArchiverFilterRuleType.AttachmentCount)
                    {
                        arFilter.Value1Unit = PolicyValueUnit.None;
                        arFilter.Value2Unit = PolicyValueUnit.None;
                    }
                    arFilter.Value2 = filter.Value2;
                    soFilters.Add(arFilter.Dto);
                }
            }
            Rule rule = new Rule();

            rule.SOFilters = soFilters;
            rule.ProfileType = GCommon.Contract.Server.Common.Profile.Object.ProfileType.ArchiverRuleForRevIM;
            rule.IncludeNew = "1";
            rule.PolicyLevel = info.RuleLevel;
            rule.MoveToArchiverTierWhenArchiving = info.MoveToArchiverTierWhenArchiving;
            rule.MoveToAnotherTierType = info.MoveToAnotherTierType;
            rule.StoragePolicyId = info.StoragePolicyId;
            rule.StoragePolicyName = info.StoragePolicyName;
            #region init rm settings

            if (info.EnableExport == true && info.ExportInfo != null && info.ExportInfo.exportSPDataOption != ExportSPDataOption.None && !(info.ExportInfo.exportType == AvePoint.GCommon.Contract.StorageOptimization.Object.ExportTypeValue.None))
            {
                SetRuleExportLocationInfo(info);
                rule.ExportInfo = info.ExportInfo;
                rule.ExportType = info.ExportInfo.exportType;
                rule.ExportInfo.exportSPDataOption = info.ExportInfo.exportSPDataOption;
                rule.ExportInfo.exportLocationId = info.StoragePolicyId;
                rule.ExportInfo.exportLocationName = info.StoragePolicyName;
            }

            rule.KeepDataOption = info.RuleKeepDataOption;
            rule.RelatedRecordOption = info.RelatedRecordOption;
            //需要对tag中datetime类型数据做处理.每个rule最多4个tag
            rule.TagContentInfo = new List<TagContentInfo>();
            if (info.TagContentInfo != null)
            {
                foreach (RMTagContentInfo tag in info.TagContentInfo)
                {
                    if (tag.Type == TagContentInfoType.DateTime)
                    {
                        DateTime dt = DateTime.Parse(tag.Value);
                        tag.DateTime = DateTimeUtil.ConvertTimeToUtcDate(dt, GeneralSettingConfig.FindSystemTimeZoneById(tag.TimeZoneId), !tag.IsDayLightSaving);
                        //tag.Value = dt.ToString(JSDateTimeFormat.DEFAULT_TIME_FORMAT, CultureInfo.InvariantCulture);
                    }
                    rule.TagContentInfo.Add(new TagContentInfo()
                    {
                        ColumnName = tag.ColumnName,
                        DateTime = tag.DateTime,
                        Type = tag.Type,
                        Value = tag.Type == TagContentInfoType.DateTime ? string.Format("{0}/{1}", tag.TimeZoneId, tag.IsDayLightSaving) : tag.Value,
                    });
                }
            }

            //rule.TagContentInfo = info.TagContentInfo;
            InitSORuleManualApprovalInfo(rule, info);
            ResetFSSOFilter(rule);
            //ResetFSMoveSettings(rule, info);
            #endregion
            return rule;
        }

        public Rule BuildRMAzureFileRule(RMRuleInfos info)
        {
            List<SOFilterPolicy> soFilters = new List<SOFilterPolicy>();

            foreach (var filter in info.RuleFilters)
            {
                ArchiverRuleFilter arFilter = new ArchiverRuleFilter();
                arFilter.CombineMode = filter.CombineMode;
                arFilter.SequenceNo = filter.SequenceNo;
                arFilter.Level = filter.Level;
                arFilter.Condition = filter.Condition;
                arFilter.RuleType = filter.RuleType;
                if (!string.IsNullOrEmpty(filter.filterName))
                {
                    arFilter.RuleName = filter.filterName;
                }

                if (arFilter.RuleType == ArchiverFilterRuleType.ModifiedTime ||
                    arFilter.RuleType == ArchiverFilterRuleType.CreatedTime ||
                    arFilter.RuleType == ArchiverFilterRuleType.LastAccessedTime ||
                    arFilter.RuleType == ArchiverFilterRuleType.DateTimeColumn ||
                    arFilter.RuleType == ArchiverFilterRuleType.DateTimeCustomProperty ||
                    arFilter.RuleType == ArchiverFilterRuleType.SendDateUTC)
                {
                    string startDayLightSaving = filter.StartTimeInfo == null ? "true" : filter.StartTimeInfo.IsDayLightSaving.ToString();
                    string endDayLightSaving = filter.EndTimeInfo == null ? "true" : filter.EndTimeInfo.IsDayLightSaving.ToString();
                    if (arFilter.Condition == ArchiverFilterCondition.FromTo)
                    {

                        DateTime startUtcTime = arFilter.SetDateTime(filter.Value1, filter.StartTimeInfo.TimeZoneId, startDayLightSaving, false);
                        DateTime endUtcTime = arFilter.SetDateTime(filter.Value2, filter.EndTimeInfo.TimeZoneId, endDayLightSaving, true);
                        if (DateTime.Parse(filter.Value1) >= DateTime.Parse(filter.Value2))
                        {
                            //throw new InvalidArgumentException(Messages.Get("start_date_after_end_date"));
                            throw new Exception("");
                        }
                        arFilter.Value1 = startUtcTime.ToString(APIDateTimeFormat.DATETYPEForAPI003);
                        arFilter.Value2 = endUtcTime.ToString(APIDateTimeFormat.DATETYPEForAPI003);
                    }
                    else if (arFilter.Condition == ArchiverFilterCondition.Before)
                    {
                        // ValidateValueCount(value, 3);
                        DateTime utcTime = arFilter.SetDateTime(filter.Value1, filter.StartTimeInfo.TimeZoneId, startDayLightSaving, false);
                        arFilter.Value1 = utcTime.ToString(APIDateTimeFormat.DATETYPEForAPI003);
                    }
                    else if (arFilter.Condition == ArchiverFilterCondition.OlderThan)
                    {
                        //ValidateValueCount(value, 1);
                        //SetValueForOlderThan(value[0]);
                        arFilter.Value1 = filter.Value1;
                        arFilter.Value1Unit = filter.Value1Unit;
                    }
                    soFilters.Add(arFilter.Dto);
                }
                else
                {
                    arFilter.Value1 = filter.Value1;
                    if (filter.RuleType == ArchiverFilterRuleType.DocumentSize || filter.RuleType == ArchiverFilterRuleType.SiteCollectionSizeTrigger
                        || filter.RuleType == ArchiverFilterRuleType.Size)
                    {
                        arFilter.Value1Unit = filter.Value1Unit;
                        arFilter.Value2Unit = filter.Value2Unit;
                    }
                    else if (filter.RuleType == ArchiverFilterRuleType.AttachmentCount)
                    {
                        arFilter.Value1Unit = PolicyValueUnit.None;
                        arFilter.Value2Unit = PolicyValueUnit.None;
                    }
                    arFilter.Value2 = filter.Value2;
                    soFilters.Add(arFilter.Dto);
                }
            }
            Rule rule = new Rule();

            rule.SOFilters = soFilters;
            rule.ProfileType = GCommon.Contract.Server.Common.Profile.Object.ProfileType.ArchiverRuleForRevIM;
            rule.IncludeNew = "1";
            rule.PolicyLevel = info.RuleLevel;
            rule.MoveToArchiverTierWhenArchiving = info.MoveToArchiverTierWhenArchiving;
            rule.MoveToAnotherTierType = info.MoveToAnotherTierType;
            #region init rm settings

            if (info.EnableExport == true && info.ExportInfo != null && info.ExportInfo.exportSPDataOption != ExportSPDataOption.None && !(info.ExportInfo.exportType == AvePoint.GCommon.Contract.StorageOptimization.Object.ExportTypeValue.None))
            {
                SetRuleExportLocationInfo(info);
                rule.ExportInfo = info.ExportInfo;
                rule.ExportType = info.ExportInfo.exportType;
                rule.ExportInfo.exportSPDataOption = info.ExportInfo.exportSPDataOption;
            }
            //}

            rule.KeepDataOption = info.RuleKeepDataOption;
            rule.RelatedRecordOption = info.RelatedRecordOption;
            //需要对tag中datetime类型数据做处理.每个rule最多4个tag
            rule.TagContentInfo = new List<TagContentInfo>();
            if (info.TagContentInfo != null)
            {
                foreach (RMTagContentInfo tag in info.TagContentInfo)
                {
                    if (tag.Type == TagContentInfoType.DateTime)
                    {
                        DateTime dt = DateTime.Parse(tag.Value);
                        tag.DateTime = DateTimeUtil.ConvertTimeToUtcDate(dt, GeneralSettingConfig.FindSystemTimeZoneById(tag.TimeZoneId), !tag.IsDayLightSaving);
                        //tag.Value = dt.ToString(JSDateTimeFormat.DEFAULT_TIME_FORMAT, CultureInfo.InvariantCulture);
                    }
                    rule.TagContentInfo.Add(new TagContentInfo()
                    {
                        ColumnName = tag.ColumnName,
                        DateTime = tag.DateTime,
                        Type = tag.Type,
                        Value = tag.Type == TagContentInfoType.DateTime ? string.Format("{0}/{1}", tag.TimeZoneId, tag.IsDayLightSaving) : tag.Value,
                    });
                }
            }

            //rule.TagContentInfo = info.TagContentInfo;
            InitSORuleManualApprovalInfo(rule, info);
            ResetAzureFileSOFilter(rule);
            //ResetFSMoveSettings(rule, info);
            #endregion
            return rule;
        }

        private Rule BuildRMBoxRule(RMRuleInfos info)
        {
            List<SOFilterPolicy> soFilters = new List<SOFilterPolicy>();

            foreach (var filter in info.RuleFilters)
            {
                ArchiverRuleFilter arFilter = new ArchiverRuleFilter();
                arFilter.CombineMode = filter.CombineMode;
                arFilter.SequenceNo = filter.SequenceNo;
                arFilter.Level = filter.Level;
                arFilter.Condition = filter.Condition;
                arFilter.RuleType = filter.RuleType;
                if (!string.IsNullOrEmpty(filter.filterName))
                {
                    arFilter.RuleName = filter.filterName;
                }

                if (arFilter.RuleType == ArchiverFilterRuleType.ModifiedTime ||
                    arFilter.RuleType == ArchiverFilterRuleType.CreatedTime ||
                    arFilter.RuleType == ArchiverFilterRuleType.LastAccessedTime ||
                    arFilter.RuleType == ArchiverFilterRuleType.DateTimeColumn ||
                    arFilter.RuleType == ArchiverFilterRuleType.DateTimeCustomProperty ||
                    arFilter.RuleType == ArchiverFilterRuleType.SendDateUTC)
                {
                    string startDayLightSaving = filter.StartTimeInfo == null ? "true" : filter.StartTimeInfo.IsDayLightSaving.ToString();
                    string endDayLightSaving = filter.EndTimeInfo == null ? "true" : filter.EndTimeInfo.IsDayLightSaving.ToString();
                    if (arFilter.Condition == ArchiverFilterCondition.FromTo)
                    {

                        DateTime startUtcTime = arFilter.SetDateTime(filter.Value1, filter.StartTimeInfo.TimeZoneId, startDayLightSaving, false);
                        DateTime endUtcTime = arFilter.SetDateTime(filter.Value2, filter.EndTimeInfo.TimeZoneId, endDayLightSaving, true);
                        if (DateTime.Parse(filter.Value1) >= DateTime.Parse(filter.Value2))
                        {
                            throw new Exception("");
                        }
                        arFilter.Value1 = startUtcTime.ToString(APIDateTimeFormat.DATETYPEForAPI003);
                        arFilter.Value2 = endUtcTime.ToString(APIDateTimeFormat.DATETYPEForAPI003);
                    }
                    else if (arFilter.Condition == ArchiverFilterCondition.Before)
                    {
                        DateTime utcTime = arFilter.SetDateTime(filter.Value1, filter.StartTimeInfo.TimeZoneId, startDayLightSaving, false);
                        arFilter.Value1 = utcTime.ToString(APIDateTimeFormat.DATETYPEForAPI003);
                    }
                    else if (arFilter.Condition == ArchiverFilterCondition.OlderThan)
                    {
                        arFilter.Value1 = filter.Value1;
                        arFilter.Value1Unit = filter.Value1Unit;
                    }
                    soFilters.Add(arFilter.Dto);
                }
                else
                {
                    arFilter.Value1 = filter.Value1;
                    if (filter.RuleType == ArchiverFilterRuleType.DocumentSize || filter.RuleType == ArchiverFilterRuleType.SiteCollectionSizeTrigger
                        || filter.RuleType == ArchiverFilterRuleType.Size)
                    {
                        arFilter.Value1Unit = filter.Value1Unit;
                        arFilter.Value2Unit = filter.Value2Unit;
                    }
                    else if (filter.RuleType == ArchiverFilterRuleType.AttachmentCount)
                    {
                        arFilter.Value1Unit = PolicyValueUnit.None;
                        arFilter.Value2Unit = PolicyValueUnit.None;
                    }
                    arFilter.Value2 = filter.Value2;
                    soFilters.Add(arFilter.Dto);
                }
            }
            Rule rule = new Rule();

            rule.SOFilters = soFilters;
            rule.ProfileType = GCommon.Contract.Server.Common.Profile.Object.ProfileType.ArchiverRuleForRevIM;
            rule.IncludeNew = "1";
            rule.PolicyLevel = info.RuleLevel;
            rule.MoveToArchiverTierWhenArchiving = info.MoveToArchiverTierWhenArchiving;
            rule.MoveToAnotherTierType = info.MoveToAnotherTierType;
            #region init rm settings

            if (info.EnableExport == true && info.ExportInfo != null && info.ExportInfo.exportSPDataOption != ExportSPDataOption.None && !(info.ExportInfo.exportType == AvePoint.GCommon.Contract.StorageOptimization.Object.ExportTypeValue.None))
            {
                SetRuleExportLocationInfo(info);
                rule.ExportInfo = info.ExportInfo;
                rule.ExportType = info.ExportInfo.exportType;
                rule.ExportInfo.exportSPDataOption = info.ExportInfo.exportSPDataOption;
            }
            //}

            rule.KeepDataOption = info.RuleKeepDataOption;
            rule.RelatedRecordOption = info.RelatedRecordOption;
            rule.TagContentInfo = new List<TagContentInfo>();
            if (info.TagContentInfo != null)
            {
                foreach (RMTagContentInfo tag in info.TagContentInfo)
                {
                    if (tag.Type == TagContentInfoType.DateTime)
                    {
                        DateTime dt = DateTime.Parse(tag.Value);
                        tag.DateTime = DateTimeUtil.ConvertTimeToUtcDate(dt, GeneralSettingConfig.FindSystemTimeZoneById(tag.TimeZoneId), !tag.IsDayLightSaving);
                    }
                    rule.TagContentInfo.Add(new TagContentInfo()
                    {
                        ColumnName = tag.ColumnName,
                        DateTime = tag.DateTime,
                        Type = tag.Type,
                        Value = tag.Type == TagContentInfoType.DateTime ? string.Format("{0}/{1}", tag.TimeZoneId, tag.IsDayLightSaving) : tag.Value,
                    });
                }
            }
            InitSORuleManualApprovalInfo(rule, info);
            ResetBoxSOFilter(rule);

            #endregion
            return rule;
        }

        public Rule BuildRMConnectorRule(RMRuleInfos info)
        {
            List<SOFilterPolicy> soFilters = new List<SOFilterPolicy>();

            foreach (var filter in info.RuleFilters)
            {
                ArchiverRuleFilter arFilter = new ArchiverRuleFilter();
                arFilter.CombineMode = filter.CombineMode;
                arFilter.SequenceNo = filter.SequenceNo;
                arFilter.Level = filter.Level;
                arFilter.Condition = filter.Condition;
                arFilter.RuleType = filter.RuleType;
                if (!string.IsNullOrEmpty(filter.filterName))
                {
                    arFilter.RuleName = filter.filterName;
                }

                if (arFilter.RuleType == ArchiverFilterRuleType.ModifiedTime ||
                    arFilter.RuleType == ArchiverFilterRuleType.CreatedTime ||
                    arFilter.RuleType == ArchiverFilterRuleType.LastAccessedTime ||
                    arFilter.RuleType == ArchiverFilterRuleType.DateTimeColumn ||
                    arFilter.RuleType == ArchiverFilterRuleType.DateTimeCustomProperty ||
                    arFilter.RuleType == ArchiverFilterRuleType.SendDateUTC)
                {
                    string startDayLightSaving = filter.StartTimeInfo == null ? "true" : filter.StartTimeInfo.IsDayLightSaving.ToString();
                    string endDayLightSaving = filter.EndTimeInfo == null ? "true" : filter.EndTimeInfo.IsDayLightSaving.ToString();
                    if (arFilter.Condition == ArchiverFilterCondition.FromTo)
                    {

                        DateTime startUtcTime = arFilter.SetDateTime(filter.Value1, filter.StartTimeInfo.TimeZoneId, startDayLightSaving, false);
                        DateTime endUtcTime = arFilter.SetDateTime(filter.Value2, filter.EndTimeInfo.TimeZoneId, endDayLightSaving, true);
                        if (DateTime.Parse(filter.Value1) >= DateTime.Parse(filter.Value2))
                        {
                            //throw new InvalidArgumentException(Messages.Get("start_date_after_end_date"));
                            throw new Exception("");
                        }
                        arFilter.Value1 = startUtcTime.ToString(APIDateTimeFormat.DATETYPEForAPI003);
                        arFilter.Value2 = endUtcTime.ToString(APIDateTimeFormat.DATETYPEForAPI003);
                    }
                    else if (arFilter.Condition == ArchiverFilterCondition.Before)
                    {
                        // ValidateValueCount(value, 3);
                        DateTime utcTime = arFilter.SetDateTime(filter.Value1, filter.StartTimeInfo.TimeZoneId, startDayLightSaving, false);
                        arFilter.Value1 = utcTime.ToString(APIDateTimeFormat.DATETYPEForAPI003);
                    }
                    else if (arFilter.Condition == ArchiverFilterCondition.OlderThan)
                    {
                        //ValidateValueCount(value, 1);
                        //SetValueForOlderThan(value[0]);
                        arFilter.Value1 = filter.Value1;
                        arFilter.Value1Unit = filter.Value1Unit;
                    }
                    soFilters.Add(arFilter.Dto);
                }
                else
                {
                    arFilter.Value1 = filter.Value1;
                    if (filter.RuleType == ArchiverFilterRuleType.DocumentSize || filter.RuleType == ArchiverFilterRuleType.SiteCollectionSizeTrigger
                        || filter.RuleType == ArchiverFilterRuleType.Size)
                    {
                        arFilter.Value1Unit = filter.Value1Unit;
                        arFilter.Value2Unit = filter.Value2Unit;
                    }
                    else if (filter.RuleType == ArchiverFilterRuleType.AttachmentCount)
                    {
                        arFilter.Value1Unit = PolicyValueUnit.None;
                        arFilter.Value2Unit = PolicyValueUnit.None;
                    }
                    arFilter.Value2 = filter.Value2;
                    soFilters.Add(arFilter.Dto);
                }
            }
            Rule rule = new Rule();

            rule.SOFilters = soFilters;
            rule.ProfileType = GCommon.Contract.Server.Common.Profile.Object.ProfileType.ArchiverRuleForRevIM;
            rule.IncludeNew = "1";
            rule.PolicyLevel = info.RuleLevel;
            rule.MoveToArchiverTierWhenArchiving = info.MoveToArchiverTierWhenArchiving;
            rule.MoveToAnotherTierType = info.MoveToAnotherTierType;
            #region init rm settings

            if (info.EnableExport == true && info.ExportInfo != null && info.ExportInfo.exportSPDataOption != ExportSPDataOption.None && !(info.ExportInfo.exportType == AvePoint.GCommon.Contract.StorageOptimization.Object.ExportTypeValue.None))
            {
                SetRuleExportLocationInfo(info);
                rule.ExportInfo = info.ExportInfo;
                rule.ExportType = info.ExportInfo.exportType;
                rule.ExportInfo.exportSPDataOption = info.ExportInfo.exportSPDataOption;
            }
            //}

            rule.KeepDataOption = info.RuleKeepDataOption;
            rule.RelatedRecordOption = info.RelatedRecordOption;
            //需要对tag中datetime类型数据做处理.每个rule最多4个tag
            rule.TagContentInfo = new List<TagContentInfo>();
            if (info.TagContentInfo != null)
            {
                foreach (RMTagContentInfo tag in info.TagContentInfo)
                {
                    if (tag.Type == TagContentInfoType.DateTime)
                    {
                        DateTime dt = DateTime.Parse(tag.Value);
                        tag.DateTime = DateTimeUtil.ConvertTimeToUtcDate(dt, GeneralSettingConfig.FindSystemTimeZoneById(tag.TimeZoneId), !tag.IsDayLightSaving);
                        //tag.Value = dt.ToString(JSDateTimeFormat.DEFAULT_TIME_FORMAT, CultureInfo.InvariantCulture);
                    }
                    rule.TagContentInfo.Add(new TagContentInfo()
                    {
                        ColumnName = tag.ColumnName,
                        DateTime = tag.DateTime,
                        Type = tag.Type,
                        Value = tag.Type == TagContentInfoType.DateTime ? string.Format("{0}/{1}", tag.TimeZoneId, tag.IsDayLightSaving) : tag.Value,
                    });
                }
            }

            //rule.TagContentInfo = info.TagContentInfo;
            InitSORuleManualApprovalInfo(rule, info);
            ResetConneectorSOFilter(rule);
            //ResetFSMoveSettings(rule, info);
            #endregion
            return rule;
        }

        public Rule BuildRMSPLocalRule(RMRuleInfos info)
        {
            List<SOFilterPolicy> soFilters = new List<SOFilterPolicy>();

            foreach (var filter in info.RuleFilters)
            {
                ArchiverRuleFilter arFilter = new ArchiverRuleFilter();
                arFilter.CombineMode = filter.CombineMode;
                arFilter.SequenceNo = filter.SequenceNo;
                arFilter.Level = filter.Level;
                arFilter.Condition = filter.Condition;
                arFilter.RuleType = filter.RuleType;
                if (!string.IsNullOrEmpty(filter.filterName))
                {
                    arFilter.RuleName = filter.filterName;
                }
                //arFilter.Dto.Rule = arFilter.RuleBase;
                if (arFilter.RuleType == ArchiverFilterRuleType.ModifiedTime ||
                    arFilter.RuleType == ArchiverFilterRuleType.CreatedTime ||
                    //arFilter.RuleType == ArchiverFilterRuleType.LastAccessedTime ||
                    arFilter.RuleType == ArchiverFilterRuleType.DateTimeColumn ||
                    arFilter.RuleType == ArchiverFilterRuleType.DateTimeCustomProperty)
                {
                    string startDayLightSaving = filter.StartTimeInfo == null ? "true" : filter.StartTimeInfo.IsDayLightSaving.ToString();
                    string endDayLightSaving = filter.EndTimeInfo == null ? "true" : filter.EndTimeInfo.IsDayLightSaving.ToString();
                    if (arFilter.Condition == ArchiverFilterCondition.FromTo)
                    {

                        DateTime startUtcTime = arFilter.SetDateTime(filter.Value1, filter.StartTimeInfo.TimeZoneId, startDayLightSaving, false);
                        DateTime endUtcTime = arFilter.SetDateTime(filter.Value2, filter.EndTimeInfo.TimeZoneId, endDayLightSaving, true);
                        if (DateTime.Parse(filter.Value1) >= DateTime.Parse(filter.Value2))
                        {
                            //throw new InvalidArgumentException(Messages.Get("start_date_after_end_date"));
                            throw new Exception("");
                        }
                        arFilter.Value1 = startUtcTime.ToString(APIDateTimeFormat.DATETYPEForAPI003);
                        arFilter.Value2 = endUtcTime.ToString(APIDateTimeFormat.DATETYPEForAPI003);
                    }
                    else if (arFilter.Condition == ArchiverFilterCondition.Before)
                    {
                        // ValidateValueCount(value, 3);
                        DateTime utcTime = arFilter.SetDateTime(filter.Value1, filter.StartTimeInfo.TimeZoneId, startDayLightSaving, false);
                        arFilter.Value1 = utcTime.ToString(APIDateTimeFormat.DATETYPEForAPI003);
                    }
                    else if (arFilter.Condition == ArchiverFilterCondition.OlderThan)
                    {
                        //ValidateValueCount(value, 1);
                        //SetValueForOlderThan(value[0]);
                        arFilter.Value1 = filter.Value1;
                        arFilter.Value1Unit = filter.Value1Unit;
                    }
                    soFilters.Add(arFilter.Dto);
                }
                else
                {
                    arFilter.Value1 = filter.Value1;
                    if (filter.RuleType == ArchiverFilterRuleType.DocumentSize || filter.RuleType == ArchiverFilterRuleType.SiteCollectionSizeTrigger)
                    {
                        arFilter.Value1Unit = filter.Value1Unit;
                        arFilter.Value2Unit = filter.Value2Unit;
                    }
                    arFilter.Value2 = filter.Value2;
                    soFilters.Add(arFilter.Dto);
                }
            }


            Rule rule = new Rule();
            if (!string.IsNullOrEmpty(info.RuleId))
            {
                rule.Id = info.RuleId;
            }
            try
            {
                //rule.ModifyTime = mGeneralSettingService.ConvertDateTimeToUtc(DateTime.Parse(info.Modified)).Ticks;
                rule.ModifyTime = DateTime.UtcNow.Ticks;
            }
            catch (Exception)
            {
                throw new Exception("Please configure general settings first.");//TODO i18n
            }
            rule.Name = info.RuleName;
            rule.Description = info.Description;
            rule.DisposalClass = !string.IsNullOrEmpty(info.DisposalClass) ? info.DisposalClass : null;
            rule.SOFilters = soFilters;
            rule.ProfileType = GCommon.Contract.Server.Common.Profile.Object.ProfileType.ArchiverRuleForRevIM;
            rule.IncludeNew = "1";
            rule.DeleteRecords = info.DeleteRecords;
            rule.IncludeDeleteRecordLabel = info.IncludeDeleteRecordLabel;
            rule.LockRecordBeforeDestroy = info.LockRecordBeforeDestroy;
            rule.DeclareLinkFile = info.DeclareLinkFile;
            rule.PolicyLevel = info.RuleLevel;
            rule.MoveToArchiverTierWhenArchiving = info.MoveToArchiverTierWhenArchiving;
            rule.MoveToAnotherTierType = info.MoveToAnotherTierType;
            if (info.MoveToRecordCenterSettings != null)
            {
                rule.MoveToRecordCenterAndDelareSetting = null;
            }

            #region init rm settings

            rule.KeepDataOption = info.RuleKeepDataOption;
            rule.RelatedRecordOption = info.RelatedRecordOption;
            //需要对tag中datetime类型数据做处理.每个rule最多4个tag
            rule.TagContentInfo = new List<TagContentInfo>();
            if (info.TagContentInfo != null)
            {
                foreach (RMTagContentInfo tag in info.TagContentInfo)
                {
                    if (tag.Type == TagContentInfoType.DateTime)
                    {
                        DateTime dt = DateTime.Parse(tag.Value);
                        tag.DateTime = DateTimeUtil.ConvertTimeToUtcDate(dt, GeneralSettingConfig.FindSystemTimeZoneById(tag.TimeZoneId), !tag.IsDayLightSaving);
                        //tag.Value = dt.ToString(JSDateTimeFormat.DEFAULT_TIME_FORMAT, CultureInfo.InvariantCulture);
                    }
                    rule.TagContentInfo.Add(new TagContentInfo()
                    {
                        ColumnName = tag.ColumnName,
                        DateTime = tag.DateTime,
                        Type = tag.Type,
                        Value = tag.Type == TagContentInfoType.DateTime ? string.Format("{0}/{1}", tag.TimeZoneId, tag.IsDayLightSaving) : tag.Value,
                    });
                }
            }

            //rule.TagContentInfo = info.TagContentInfo;
            InitSORuleManualApprovalInfo(rule, info);
            #endregion
            //ResetMoveSettings(rule, info);
            ResetSOFilter(rule);
            return rule;
        }

        public async Task<Rule> BuildRMOneDriveRuleAsync(RMRuleInfos info)
        {
            List<SOFilterPolicy> soFilters = new List<SOFilterPolicy>();


            foreach (var filter in info.RuleFilters)
            {
                ArchiverRuleFilter arFilter = new ArchiverRuleFilter();
                arFilter.CombineMode = filter.CombineMode;
                arFilter.SequenceNo = filter.SequenceNo;
                arFilter.Level = filter.Level;
                arFilter.Condition = filter.Condition;
                arFilter.RuleType = filter.RuleType;
                if (!string.IsNullOrEmpty(filter.filterName))
                {
                    arFilter.RuleName = filter.filterName;
                }
                if (!TenantService.IsNewOpusTenant() && (filter.RuleType == ArchiverFilterRuleType.RetentionLabel || filter.RuleType == ArchiverFilterRuleType.SensitivityLabel)) // RECO-29793
                {
                    throw new Exception("Doesn't support Retention/Sensitivity Label for old logic account.");
                }
                //arFilter.Dto.Rule = arFilter.RuleBase;
                if (arFilter.RuleType == ArchiverFilterRuleType.ModifiedTime ||
                    arFilter.RuleType == ArchiverFilterRuleType.CreatedTime ||
                    arFilter.RuleType == ArchiverFilterRuleType.LastAccessedTime ||
                    arFilter.RuleType == ArchiverFilterRuleType.LastActiveTime ||
                    arFilter.RuleType == ArchiverFilterRuleType.DateTimeColumn ||
                    arFilter.RuleType == ArchiverFilterRuleType.DateTimeCustomProperty ||
                    arFilter.RuleType == ArchiverFilterRuleType.ParentLibraryDateTime ||
                    arFilter.RuleType == ArchiverFilterRuleType.PropertyBagDateTime ||
                    arFilter.RuleType == ArchiverFilterRuleType.DocumentModified)
                {
                    string startDayLightSaving = filter.StartTimeInfo == null ? "true" : filter.StartTimeInfo.IsDayLightSaving.ToString();
                    string endDayLightSaving = filter.EndTimeInfo == null ? "true" : filter.EndTimeInfo.IsDayLightSaving.ToString();
                    if (arFilter.Condition == ArchiverFilterCondition.FromTo)
                    {

                        DateTime startUtcTime = arFilter.SetDateTime(filter.Value1, filter.StartTimeInfo.TimeZoneId, startDayLightSaving, false);
                        DateTime endUtcTime = arFilter.SetDateTime(filter.Value2, filter.EndTimeInfo.TimeZoneId, endDayLightSaving, true);
                        if (DateTime.Parse(filter.Value1) >= DateTime.Parse(filter.Value2))
                        {
                            //throw new InvalidArgumentException(Messages.Get("start_date_after_end_date"));
                            throw new Exception("");
                        }
                        arFilter.Value1 = startUtcTime.ToString(APIDateTimeFormat.DATETYPEForAPI003);
                        arFilter.Value2 = endUtcTime.ToString(APIDateTimeFormat.DATETYPEForAPI003);
                    }
                    else if (arFilter.Condition == ArchiverFilterCondition.Before)
                    {
                        // ValidateValueCount(value, 3);
                        DateTime utcTime = arFilter.SetDateTime(filter.Value1, filter.StartTimeInfo.TimeZoneId, startDayLightSaving, false);
                        arFilter.Value1 = utcTime.ToString(APIDateTimeFormat.DATETYPEForAPI003);
                    }
                    else if (arFilter.Condition == ArchiverFilterCondition.OlderThan)
                    {
                        //ValidateValueCount(value, 1);
                        //SetValueForOlderThan(value[0]);
                        arFilter.Value1 = filter.Value1;
                        arFilter.Value1Unit = filter.Value1Unit;
                    }
                    soFilters.Add(arFilter.Dto);
                }
                else
                {
                    arFilter.Value1 = filter.Value1;
                    if (filter.RuleType == ArchiverFilterRuleType.DocumentSize || filter.RuleType == ArchiverFilterRuleType.SiteCollectionSizeTrigger)
                    {
                        arFilter.Value1Unit = filter.Value1Unit;
                        arFilter.Value2Unit = filter.Value2Unit;
                    }
                    arFilter.Value2 = filter.Value2;
                    soFilters.Add(arFilter.Dto);
                }
            }

            EnableInsightsDataCollection(info.RuleFilters);

            Rule rule = new Rule();
            if (!string.IsNullOrEmpty(info.RuleId))
            {
                rule.Id = info.RuleId;
            }
            try
            {
                //rule.ModifyTime = mGeneralSettingService.ConvertDateTimeToUtc(DateTime.Parse(info.Modified)).Ticks;
                rule.ModifyTime = DateTime.UtcNow.Ticks;
            }
            catch (Exception)
            {
                throw new Exception("Please configure general settings first.");//TODO i18n
            }

            rule.Name = info.RuleName;
            rule.Description = info.Description;
            rule.DisposalClass = !string.IsNullOrEmpty(info.DisposalClass) ? info.DisposalClass : null;
            rule.SOFilters = soFilters;
            if (info.ModelType == RuleModel.SOArchiver)
            {
                rule.ProfileType = GCommon.Contract.Server.Common.Profile.Object.ProfileType.ArchiverRule;
            }
            else
            {
                rule.ProfileType = GCommon.Contract.Server.Common.Profile.Object.ProfileType.ArchiverRuleForRevIM;
            }
            rule.IncludeNew = "1";
            rule.DeleteRecords = info.DeleteRecords;
            rule.IncludeDeleteRecordLabel = info.IncludeDeleteRecordLabel;
            rule.LockRecordBeforeDestroy = info.LockRecordBeforeDestroy;
            rule.DeleteSiteCollectionToRecycleBin = info.IsDeleteSiteCollectionToRecycleBin();
            rule.DeleteToRecycleBin = info.DeleteToRecycleBin;
            rule.DeclareLinkFile = info.DeclareLinkFile;
            rule.PolicyLevel = info.RuleLevel;
            rule.StoragePolicyId = info.StoragePolicyId;
            rule.StoragePolicyName = info.StoragePolicyName;
            rule.StubTemplateId = info.StubTemplateId;
            rule.StubTemplateName = info.StubTemplateName;
            rule.MoveToArchiverTierWhenArchiving = info.MoveToArchiverTierWhenArchiving;
            rule.MoveToAnotherTierType = info.MoveToAnotherTierType;
            rule.KeepLatestMajorAndMinorVersion = info.KeepLatestMajorAndMinorVersion;
            rule.ArchiverOnlyLastestVersion = info.ArchiverOnlyLastestVersion;
            rule.KeepLatestMajorAndMinorVersionAndArchiveOthers = info.KeepLatestMajorAndMinorVersionAndArchiveOthers;
            rule.ArchivedLatestVersion = info.ArchivedLatestVersion;

            if (info.MoveToRecordCenterSettings != null)
            {
                rule.MoveToRecordCenterAndDelareSetting = null;
            }

            #region init rm settings

            if (info.EnableExport == true && info.ExportInfo != null && info.ExportInfo.exportSPDataOption != ExportSPDataOption.None && !(info.ExportInfo.exportType == AvePoint.GCommon.Contract.StorageOptimization.Object.ExportTypeValue.None))
            {
                SetRuleExportLocationInfo(info);
                rule.ExportInfo = info.ExportInfo;
                rule.ExportType = info.ExportInfo.exportType;
                rule.ExportInfo.exportSPDataOption = info.ExportInfo.exportSPDataOption;
                rule.ExportDataBeforeArchiving = info.ExportDataBeforeArchiving;
                if (TenantService.IsNewOpusTenant())
                {
                    rule.ExportInfo.exportLocationId = info.ExportInfo.exportLocationId;
                    rule.ExportInfo.exportLocationName = info.ExportInfo.exportLocationName;
                    rule.ExportInfo.newOptionsOfExportInfo = true;
                    await ExportMoveSettingsAsync(rule, info);
                }
            }
            //}

            rule.KeepDataOption = info.RuleKeepDataOption;
            //需要对tag中datetime类型数据做处理.每个rule最多4个tag
            rule.TagContentInfo = new List<TagContentInfo>();
            if (info.TagContentInfo != null)
            {
                foreach (RMTagContentInfo tag in info.TagContentInfo)
                {
                    if (tag.Type == TagContentInfoType.DateTime)
                    {
                        DateTime dt = DateTime.Parse(tag.Value);
                        tag.DateTime = DateTimeUtil.ConvertTimeToUtcDate(dt, GeneralSettingConfig.FindSystemTimeZoneById(tag.TimeZoneId), !tag.IsDayLightSaving);
                        //tag.Value = dt.ToString(JSDateTimeFormat.DEFAULT_TIME_FORMAT, CultureInfo.InvariantCulture);
                    }
                    rule.TagContentInfo.Add(new TagContentInfo()
                    {
                        ColumnName = tag.ColumnName,
                        DateTime = tag.DateTime,
                        Type = tag.Type,
                        Value = tag.Type == TagContentInfoType.DateTime ? string.Format("{0}/{1}", tag.TimeZoneId, tag.IsDayLightSaving) : tag.Value,
                        Option = tag.Option,
                    });
                }
            }
            if (info.RetentionInfo != null)
            {
                await CheckUserInfoIdIsNullAndSetItAsync(info.RetentionInfo.UserInfos);
            }
            if ((rule.KeepDataOption & (int)KeepDataStatus.Archive) == (int)KeepDataStatus.Archive || (rule.KeepDataOption & (int)KeepDataStatus.ArchiveAndLeaveStub) == (int)KeepDataStatus.ArchiveAndLeaveStub)
            {
                rule.IsEnableRetention = info.IsEnableRetention;
                rule.RetentionInfo = await this.ConvertRetentionSettingAsync(info.RetentionInfo);
                rule.IsEnableStoreContentRetention = false;
                rule.StoreContentRetentionInfos = new();
            }
            else
            {
                rule.IsEnableRetention = false;
                rule.RetentionInfo = new();
                rule.IsEnableStoreContentRetention = info.RetentionInfoList != null ? true : info.RetentionInfo != null;
                if (info.RetentionInfo != null)
                {
                    info.RetentionInfo.IsEnableRetention = true;
                }
                rule.StoreContentRetentionInfos = ConvertStoreContentRetentionSetting(info.RetentionInfoList ?? new List<RetentionSettings>() { info.RetentionInfo });
            }

            //rule.TagContentInfo = info.TagContentInfo;
            SetLeaveStubMessage(rule, info);
            InitSORuleManualApprovalInfo(rule, info);
            #endregion
            await ResetMoveSettingsAsync(rule, info);
            ResetSOFilter(rule);
            return rule;
        }
        private async Task<Rule> BuildRMGoogleDriveRule(RMRuleInfos info)
        {
            List<SOFilterPolicy> soFilters = new List<SOFilterPolicy>();

            foreach (var filter in info.RuleFilters)
            {
                ArchiverRuleFilter arFilter = new ArchiverRuleFilter();
                arFilter.CombineMode = filter.CombineMode;
                arFilter.SequenceNo = filter.SequenceNo;
                arFilter.Level = filter.Level;
                arFilter.Condition = filter.Condition;
                arFilter.RuleType = filter.RuleType;
                if (!string.IsNullOrEmpty(filter.filterName))
                {
                    arFilter.RuleName = filter.filterName;
                }

                if (arFilter.RuleType == ArchiverFilterRuleType.ModifiedTime ||
                    arFilter.RuleType == ArchiverFilterRuleType.CreatedTime ||
                    arFilter.RuleType == ArchiverFilterRuleType.LastAccessedTime ||
                    arFilter.RuleType == ArchiverFilterRuleType.DateTimeColumn ||
                    arFilter.RuleType == ArchiverFilterRuleType.DateTimeCustomProperty ||
                    arFilter.RuleType == ArchiverFilterRuleType.SendDateUTC)
                {
                    string startDayLightSaving = filter.StartTimeInfo == null ? "true" : filter.StartTimeInfo.IsDayLightSaving.ToString();
                    string endDayLightSaving = filter.EndTimeInfo == null ? "true" : filter.EndTimeInfo.IsDayLightSaving.ToString();
                    if (arFilter.Condition == ArchiverFilterCondition.FromTo)
                    {

                        DateTime startUtcTime = arFilter.SetDateTime(filter.Value1, filter.StartTimeInfo.TimeZoneId, startDayLightSaving, false);
                        DateTime endUtcTime = arFilter.SetDateTime(filter.Value2, filter.EndTimeInfo.TimeZoneId, endDayLightSaving, true);
                        if (DateTime.Parse(filter.Value1) >= DateTime.Parse(filter.Value2))
                        {
                            throw new Exception("");
                        }
                        arFilter.Value1 = startUtcTime.ToString(APIDateTimeFormat.DATETYPEForAPI003);
                        arFilter.Value2 = endUtcTime.ToString(APIDateTimeFormat.DATETYPEForAPI003);
                    }
                    else if (arFilter.Condition == ArchiverFilterCondition.Before)
                    {
                        DateTime utcTime = arFilter.SetDateTime(filter.Value1, filter.StartTimeInfo.TimeZoneId, startDayLightSaving, false);
                        arFilter.Value1 = utcTime.ToString(APIDateTimeFormat.DATETYPEForAPI003);
                    }
                    else if (arFilter.Condition == ArchiverFilterCondition.OlderThan)
                    {
                        arFilter.Value1 = filter.Value1;
                        arFilter.Value1Unit = filter.Value1Unit;
                    }
                    soFilters.Add(arFilter.Dto);
                }
                else if (arFilter.RuleType == ArchiverFilterRuleType.DateTimeLabelProperty)
                {
                    arFilter.Value1 = filter.Value1;
                    arFilter.Value1Unit = filter.Value1Unit;
                    string startDayLightSaving = filter.StartTimeInfo == null ? "true" : filter.StartTimeInfo.IsDayLightSaving.ToString();
                    string endDayLightSaving = filter.EndTimeInfo == null ? "true" : filter.EndTimeInfo.IsDayLightSaving.ToString();
                    if (arFilter.Condition == ArchiverFilterCondition.FromTo)
                    {

                        DateTime startUtcTime = arFilter.SetDateTime(filter.Value2, filter.StartTimeInfo.TimeZoneId, startDayLightSaving, false);
                        DateTime endUtcTime = arFilter.SetDateTime(filter.Value3, filter.EndTimeInfo.TimeZoneId, endDayLightSaving, true);
                        if (DateTime.Parse(filter.Value2) >= DateTime.Parse(filter.Value3))
                        {
                            throw new Exception("");
                        }
                        arFilter.Value2 = startUtcTime.ToString(APIDateTimeFormat.DATETYPEForAPI003);
                        arFilter.Value3 = endUtcTime.ToString(APIDateTimeFormat.DATETYPEForAPI003);
                    }
                    else if (arFilter.Condition == ArchiverFilterCondition.Before)
                    {
                        DateTime utcTime = arFilter.SetDateTime(filter.Value2, filter.StartTimeInfo.TimeZoneId, startDayLightSaving, false);
                        arFilter.Value2 = utcTime.ToString(APIDateTimeFormat.DATETYPEForAPI003);
                    }
                    else if (arFilter.Condition == ArchiverFilterCondition.OlderThan)
                    {
                        arFilter.Value2 = filter.Value2;
                        arFilter.Value2Unit = filter.Value2Unit;
                    }
                    soFilters.Add(arFilter.Dto);
                }
                else
                {
                    arFilter.Value1 = filter.Value1;
                    if (filter.RuleType == ArchiverFilterRuleType.DocumentSize || filter.RuleType == ArchiverFilterRuleType.SiteCollectionSizeTrigger
                        || filter.RuleType == ArchiverFilterRuleType.Size)
                    {
                        arFilter.Value1Unit = filter.Value1Unit;
                        arFilter.Value2Unit = filter.Value2Unit;
                    }
                    else if (filter.RuleType == ArchiverFilterRuleType.AttachmentCount)
                    {
                        arFilter.Value1Unit = PolicyValueUnit.None;
                        arFilter.Value2Unit = PolicyValueUnit.None;
                    }
                    arFilter.Value2 = filter.Value2;
                    soFilters.Add(arFilter.Dto);
                }
            }
            Rule rule = new Rule();

            rule.SOFilters = soFilters;
            rule.ProfileType = GCommon.Contract.Server.Common.Profile.Object.ProfileType.ArchiverRuleForRevIM;
            rule.IncludeNew = "1";
            rule.PolicyLevel = info.RuleLevel;
            rule.MoveToArchiverTierWhenArchiving = info.MoveToArchiverTierWhenArchiving;
            rule.MoveToAnotherTierType = info.MoveToAnotherTierType;
            rule.StoragePolicyId = info.StoragePolicyId;
            rule.StoragePolicyName = info.StoragePolicyName;
            rule.StubTemplateId = info.StubTemplateId;
            rule.StubTemplateName = info.StubTemplateName;
            rule.KeepLatestMajorAndMinorVersion = info.KeepLatestMajorAndMinorVersion;
            rule.ArchiverOnlyLastestVersion = info.ArchiverOnlyLastestVersion;
            rule.KeepLatestMajorAndMinorVersionAndArchiveOthers = info.KeepLatestMajorAndMinorVersionAndArchiveOthers;
            rule.ArchivedLatestVersion = info.ArchivedLatestVersion;
            #region init rm settings

            if (info.EnableExport == true && info.ExportInfo != null && info.ExportInfo.exportSPDataOption != ExportSPDataOption.None && !(info.ExportInfo.exportType == AvePoint.GCommon.Contract.StorageOptimization.Object.ExportTypeValue.None))
            {
                if (info.EnableExport && info.ExportInfo != null && info.ExportInfo.exportSPDataOption != ExportSPDataOption.None && !(info.ExportInfo.exportType == AvePoint.GCommon.Contract.StorageOptimization.Object.ExportTypeValue.None))
                {
                    SetRuleGoogleExportLocationInfo(info);
                    rule.ExportInfo = info.ExportInfo;
                    rule.ExportType = info.ExportInfo.exportType;
                    rule.ExportInfo.exportSPDataOption = info.ExportInfo.exportSPDataOption;
                    rule.ExportDataBeforeArchiving = info.ExportDataBeforeArchiving;
                    if (TenantService.IsNewOpusTenant())
                    {
                        rule.ExportInfo.exportLocationId = info.ExportInfo.exportLocationId;
                        rule.ExportInfo.exportLocationName = info.ExportInfo.exportLocationName;
                        rule.ExportInfo.newOptionsOfExportInfo = true;
                        await ExportMoveSettingsAsync(rule, info);
                    }
                }
            }
            //}

            rule.KeepDataOption = info.RuleKeepDataOption;
            rule.RelatedRecordOption = info.RelatedRecordOption;
            rule.TagContentInfo = new List<TagContentInfo>();
            if (info.TagContentInfo != null)
            {
                foreach (RMTagContentInfo tag in info.TagContentInfo)
                {
                    if (tag.Type == TagContentInfoType.DateTime)
                    {
                        DateTime dt = DateTime.Parse(tag.Value);
                        tag.DateTime = DateTimeUtil.ConvertTimeToUtcDate(dt, GeneralSettingConfig.FindSystemTimeZoneById(tag.TimeZoneId), !tag.IsDayLightSaving);
                    }
                    rule.TagContentInfo.Add(new TagContentInfo()
                    {
                        ColumnName = tag.ColumnName,
                        DateTime = tag.DateTime,
                        Type = tag.Type,
                        Value = tag.Type == TagContentInfoType.DateTime ? string.Format("{0}/{1}", tag.TimeZoneId, tag.IsDayLightSaving) : tag.Value,
                    });
                }
            }
            if (info.RetentionInfo != null)
            {
                await CheckUserInfoIdIsNullAndSetItAsync(info.RetentionInfo.UserInfos);
            }
            /*if ((rule.KeepDataOption & (int)KeepDataStatus.Archive) == (int)KeepDataStatus.Archive || (rule.KeepDataOption & (int)KeepDataStatus.ArchiveAndLeaveStub) == (int)KeepDataStatus.ArchiveAndLeaveStub)
            {
                rule.IsEnableRetention = info.IsEnableRetention;
                rule.RetentionInfo = await this.ConvertRetentionSettingAsync(info.RetentionInfo);
                rule.IsEnableStoreContentRetention = false;
                rule.StoreContentRetentionInfos = new();
            }
            else*/
            {
                rule.IsEnableRetention = false;
                rule.RetentionInfo = new();
                rule.IsEnableStoreContentRetention = info.RetentionInfoList != null ? true : info.RetentionInfo != null;
                if (info.RetentionInfo != null)
                {
                    info.RetentionInfo.IsEnableRetention = true;
                }
                rule.StoreContentRetentionInfos = ConvertStoreContentRetentionSetting(info.RetentionInfoList ?? new List<RetentionSettings>() { info.RetentionInfo });
            }
            InitSORuleManualApprovalInfo(rule, info);
            ResetGoogleSOFilter(rule);

            #endregion
            return rule;
        }

        private async Task<Rule> BuildRMTeamsRuleAsync(RMRuleInfos info)
        {
            List<SOFilterPolicy> soFilters = new List<SOFilterPolicy>();

            foreach (var filter in info.RuleFilters)
            {
                ArchiverRuleFilter arFilter = new ArchiverRuleFilter();
                arFilter.CombineMode = filter.CombineMode;
                arFilter.SequenceNo = filter.SequenceNo;
                arFilter.Level = filter.Level;
                arFilter.Condition = filter.Condition;
                arFilter.RuleType = filter.RuleType;
                if (!string.IsNullOrEmpty(filter.filterName))
                {
                    arFilter.RuleName = filter.filterName;
                }
                if (!TenantService.IsNewOpusTenant() && (filter.RuleType == ArchiverFilterRuleType.RetentionLabel || filter.RuleType == ArchiverFilterRuleType.SensitivityLabel))
                {
                    throw new Exception("Doesn't support Retention/Sensitivity Label for old logic account.");
                }
                //arFilter.Dto.Rule = arFilter.RuleBase;
                if (arFilter.RuleType == ArchiverFilterRuleType.ModifiedTime ||
                    arFilter.RuleType == ArchiverFilterRuleType.CreatedTime ||
                    arFilter.RuleType == ArchiverFilterRuleType.LastAccessedTime ||
                    arFilter.RuleType == ArchiverFilterRuleType.LastActiveTime ||
                    arFilter.RuleType == ArchiverFilterRuleType.DateTimeColumn ||
                    arFilter.RuleType == ArchiverFilterRuleType.DateTimeCustomProperty)
                {
                    string startDayLightSaving = filter.StartTimeInfo == null ? "true" : filter.StartTimeInfo.IsDayLightSaving.ToString();
                    string endDayLightSaving = filter.EndTimeInfo == null ? "true" : filter.EndTimeInfo.IsDayLightSaving.ToString();
                    if (arFilter.Condition == ArchiverFilterCondition.FromTo)
                    {

                        DateTime startUtcTime = arFilter.SetDateTime(filter.Value1, filter.StartTimeInfo.TimeZoneId, startDayLightSaving, false);
                        DateTime endUtcTime = arFilter.SetDateTime(filter.Value2, filter.EndTimeInfo.TimeZoneId, endDayLightSaving, true);
                        if (DateTime.Parse(filter.Value1) >= DateTime.Parse(filter.Value2))
                        {
                            //throw new InvalidArgumentException(Messages.Get("start_date_after_end_date"));
                            throw new Exception("");
                        }
                        arFilter.Value1 = startUtcTime.ToString(APIDateTimeFormat.DATETYPEForAPI003);
                        arFilter.Value2 = endUtcTime.ToString(APIDateTimeFormat.DATETYPEForAPI003);
                    }
                    else if (arFilter.Condition == ArchiverFilterCondition.Before)
                    {
                        // ValidateValueCount(value, 3);
                        DateTime utcTime = arFilter.SetDateTime(filter.Value1, filter.StartTimeInfo.TimeZoneId, startDayLightSaving, false);
                        arFilter.Value1 = utcTime.ToString(APIDateTimeFormat.DATETYPEForAPI003);
                    }
                    else if (arFilter.Condition == ArchiverFilterCondition.OlderThan)
                    {
                        //ValidateValueCount(value, 1);
                        //SetValueForOlderThan(value[0]);
                        arFilter.Value1 = filter.Value1;
                        arFilter.Value1Unit = filter.Value1Unit;
                    }
                    soFilters.Add(arFilter.Dto);
                }
                else if (arFilter.RuleType == ArchiverFilterRuleType.DateTimeLabelProperty)
                {
                    string startDayLightSaving = filter.StartTimeInfo == null ? "true" : filter.StartTimeInfo.IsDayLightSaving.ToString();
                    string endDayLightSaving = filter.EndTimeInfo == null ? "true" : filter.EndTimeInfo.IsDayLightSaving.ToString();
                    if (arFilter.Condition == ArchiverFilterCondition.FromTo)
                    {

                        DateTime startUtcTime = arFilter.SetDateTime(filter.Value2, filter.StartTimeInfo.TimeZoneId, startDayLightSaving, false);
                        DateTime endUtcTime = arFilter.SetDateTime(filter.Value3, filter.EndTimeInfo.TimeZoneId, endDayLightSaving, true);
                        if (DateTime.Parse(filter.Value2) >= DateTime.Parse(filter.Value3))
                        {
                            //throw new InvalidArgumentException(Messages.Get("start_date_after_end_date"));
                            throw new Exception("");
                        }
                        arFilter.Value2 = startUtcTime.ToString(APIDateTimeFormat.DATETYPEForAPI003);
                        arFilter.Value3 = endUtcTime.ToString(APIDateTimeFormat.DATETYPEForAPI003);
                    }
                    else if (arFilter.Condition == ArchiverFilterCondition.Before)
                    {
                        // ValidateValueCount(value, 3);
                        DateTime utcTime = arFilter.SetDateTime(filter.Value2, filter.StartTimeInfo.TimeZoneId, startDayLightSaving, false);
                        arFilter.Value2 = utcTime.ToString(APIDateTimeFormat.DATETYPEForAPI003);
                    }
                    else if (arFilter.Condition == ArchiverFilterCondition.OlderThan)
                    {
                        //ValidateValueCount(value, 1);
                        //SetValueForOlderThan(value[0]);
                        arFilter.Value2 = filter.Value2;
                        arFilter.Value2Unit = filter.Value2Unit;
                    }
                    soFilters.Add(arFilter.Dto);
                }
                else
                {
                    arFilter.Value1 = filter.Value1;
                    if (filter.RuleType == ArchiverFilterRuleType.DocumentSize || filter.RuleType == ArchiverFilterRuleType.SiteCollectionSizeTrigger || filter.RuleType == ArchiverFilterRuleType.Privacy || filter.RuleType == ArchiverFilterRuleType.TeamsStatus || filter.RuleType == ArchiverFilterRuleType.TeamType)
                    {
                        arFilter.Value1Unit = filter.Value1Unit;
                        arFilter.Value2Unit = filter.Value2Unit;
                    }
                    arFilter.Value2 = filter.Value2;
                    soFilters.Add(arFilter.Dto);
                }
                if (arFilter.RuleType == ArchiverFilterRuleType.TextColumn && arFilter.Condition == ArchiverFilterCondition.ListIn)
                {
                    var inArray = arFilter.Value1.Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                    List<string> list = [];
                    foreach (var arrayItem in inArray)
                    {
                        if (!list.Any(i => i.Equals(arrayItem, StringComparison.OrdinalIgnoreCase)))
                        {
                            list.Add(arrayItem);
                        }
                    }
                    arFilter.Value1 = string.Join(";", list);
                }
            }
            EnableInsightsDataCollection(info.RuleFilters);

            Rule rule = new Rule();
            if (!string.IsNullOrEmpty(info.RuleId))
            {
                rule.Id = info.RuleId;
            }
            try
            {
                rule.ModifyTime = DateTime.UtcNow.Ticks;
            }
            catch (Exception)
            {
                throw new Exception("Please configure general settings first.");//TODO i18n
            }

            rule.Name = info.RuleName;
            rule.Description = info.Description;
            rule.DisposalClass = !string.IsNullOrEmpty(info.DisposalClass) ? info.DisposalClass : null;
            rule.SOFilters = soFilters;
            if (info.ModelType == RuleModel.SOArchiver)
            {
                rule.ProfileType = GCommon.Contract.Server.Common.Profile.Object.ProfileType.ArchiverRule;
            }
            else
            {
                rule.ProfileType = GCommon.Contract.Server.Common.Profile.Object.ProfileType.ArchiverRuleForRevIM;
            }
            rule.IncludeNew = "1";
            rule.DeleteRecords = info.DeleteRecords;
            rule.IncludeDeleteRecordLabel = info.IncludeDeleteRecordLabel;
            rule.LockRecordBeforeDestroy = info.LockRecordBeforeDestroy;
            rule.DeclareLinkFile = info.DeclareLinkFile;
            rule.PolicyLevel = info.RuleLevel;
            rule.StoragePolicyId = info.StoragePolicyId;
            rule.StoragePolicyName = info.StoragePolicyName;
            rule.StubTemplateId = info.StubTemplateId;
            rule.StubTemplateName = info.StubTemplateName;
            rule.MoveToArchiverTierWhenArchiving = false;
            rule.MoveToAnotherTierType = info.MoveToAnotherTierType;
            rule.ArchivedLatestVersion = info.ArchivedLatestVersion;
            rule.KeepLatestMajorAndMinorVersion = info.KeepLatestMajorAndMinorVersion;
            rule.ArchiverOnlyLastestVersion = info.ArchiverOnlyLastestVersion;
            rule.KeepLatestMajorAndMinorVersionAndArchiveOthers = info.KeepLatestMajorAndMinorVersionAndArchiveOthers;
            if (info.MoveToRecordCenterSettings != null)
            {
                rule.MoveToRecordCenterAndDelareSetting = null;
            }

            #region init rm settings

            if (info.EnableExport && info.ExportInfo != null && info.ExportInfo.exportSPDataOption != ExportSPDataOption.None && !(info.ExportInfo.exportType == AvePoint.GCommon.Contract.StorageOptimization.Object.ExportTypeValue.None))
            {
                SetRuleExportLocationInfo(info);
                rule.ExportInfo = info.ExportInfo;
                rule.ExportType = info.ExportInfo.exportType;
                rule.ExportInfo.exportSPDataOption = info.ExportInfo.exportSPDataOption;
                rule.ExportDataBeforeArchiving = info.ExportDataBeforeArchiving;
                if (TenantService.IsNewOpusTenant())
                {
                    rule.ExportInfo.exportLocationId = info.ExportInfo.exportLocationId;
                    rule.ExportInfo.exportLocationName = info.ExportInfo.exportLocationName;
                    rule.ExportInfo.newOptionsOfExportInfo = true;
                    await ExportMoveSettingsAsync(rule, info);
                }
            }

            rule.KeepDataOption = info.RuleKeepDataOption;
            rule.RelatedRecordOption = info.RelatedRecordOption;
            rule.TagContentInfo = new List<TagContentInfo>();
            if (info.TagContentInfo != null)
            {
                foreach (RMTagContentInfo tag in info.TagContentInfo)
                {
                    if (tag.Type == TagContentInfoType.DateTime)
                    {
                        DateTime dt = DateTime.Parse(tag.Value);
                        tag.DateTime = DateTimeUtil.ConvertTimeToUtcDate(dt, GeneralSettingConfig.FindSystemTimeZoneById(tag.TimeZoneId), !tag.IsDayLightSaving);
                        tag.DateTime = DateTimeUtil.ConvertTimeToUtcDate(dt, GeneralSettingConfig.FindSystemTimeZoneById(tag.TimeZoneId), !tag.IsDayLightSaving);
                    }
                    rule.TagContentInfo.Add(new TagContentInfo()
                    {
                        ColumnName = tag.ColumnName,
                        DateTime = tag.DateTime,
                        Type = tag.Type,
                        Value = tag.Type == TagContentInfoType.DateTime ? string.Format("{0}/{1}", tag.TimeZoneId, tag.IsDayLightSaving) : tag.Value,
                    });
                }
            }
            if (info.RetentionInfo != null)
            {
                await CheckUserInfoIdIsNullAndSetItAsync(info.RetentionInfo.UserInfos);
            }
            if ((rule.KeepDataOption & (int)KeepDataStatus.Archive) == (int)KeepDataStatus.Archive || (rule.KeepDataOption & (int)KeepDataStatus.ArchiveAndLeaveStub) == (int)KeepDataStatus.ArchiveAndLeaveStub)
            {
                rule.IsEnableRetention = info.IsEnableRetention;
                rule.RetentionInfo = await this.ConvertRetentionSettingAsync(info.RetentionInfo);
                rule.IsEnableStoreContentRetention = false;
                rule.StoreContentRetentionInfos = new();
            }
            else
            {
                rule.IsEnableRetention = false;
                rule.RetentionInfo = new();
                rule.IsEnableStoreContentRetention = info.RetentionInfoList != null ? true : info.RetentionInfo != null;
                if (info.RetentionInfo != null)
                {
                    info.RetentionInfo.IsEnableRetention = true;
                }
                rule.StoreContentRetentionInfos = ConvertStoreContentRetentionSetting(info.RetentionInfoList ?? new List<RetentionSettings>() { info.RetentionInfo });
            }
            SetLeaveStubMessage(rule, info);
            InitSORuleManualApprovalInfo(rule, info);
            #endregion
            await ResetTeamsMoveSettingsAsync(rule, info);
            ResetSOFilter(rule);
            return rule;
        }

        private void ResetGoogleSOFilter(Rule rule)
        {
            Dictionary<PolicyLevel, List<SOFilterPolicy>> filterGroups = rule.SOFilters.AsQueryable().GroupBy(so => so.Level).ToDictionary(g => g.Key, p => p.ToList());
            rule.PolicyLevel = PolicyLevel.GoogleDriveDocument;
            foreach (KeyValuePair<PolicyLevel, List<SOFilterPolicy>> filterGroup in filterGroups)
            {
                PolicyLevel filterLevel = filterGroup.Key;
                string AndOrExpression = "(";
                for (int i = 0; i < filterGroup.Value.Count; i++)
                {
                    SOFilterPolicy filterDto = filterGroup.Value[i];
                    filterGroup.Value[i].SequenceNo = i + 1;
                    if (i == filterGroup.Value.Count - 1)
                    {
                        AndOrExpression += string.Format("{0}", filterDto.SequenceNo);
                    }
                    else
                    {
                        AndOrExpression += string.Format("{0} {1} ", filterDto.SequenceNo, filterDto.IsAnd ? "And" : "Or");
                    }
                }
                AndOrExpression += ")";
                if (rule.AndOrExpression == null)
                {
                    rule.AndOrExpression = new Dictionary<PolicyLevel, string>()
                    {
                        { filterLevel, AndOrExpression }
                    };
                }
                else
                {
                    if (!rule.AndOrExpression.ContainsKey(filterLevel))
                    {
                        rule.AndOrExpression.Add(filterLevel, AndOrExpression);
                    }
                }
            }
        }
        public void ResetFSSOFilter(Rule rule)
        {
            Dictionary<PolicyLevel, List<SOFilterPolicy>> filterGroups = rule.SOFilters.AsQueryable().GroupBy(so => so.Level).ToDictionary(g => g.Key, p => p.ToList());
            rule.PolicyLevel = PolicyLevel.FileSysFile;
            foreach (KeyValuePair<PolicyLevel, List<SOFilterPolicy>> filterGroup in filterGroups)
            {
                PolicyLevel filterLevel = filterGroup.Key;
                string AndOrExpression = "(";
                for (int i = 0; i < filterGroup.Value.Count; i++)
                {
                    SOFilterPolicy filterDto = filterGroup.Value[i];
                    filterGroup.Value[i].SequenceNo = i + 1;
                    if (i == filterGroup.Value.Count - 1)
                    {
                        AndOrExpression += string.Format("{0}", filterDto.SequenceNo);
                    }
                    else
                    {
                        AndOrExpression += string.Format("{0} {1} ", filterDto.SequenceNo, filterDto.IsAnd ? "And" : "Or");
                    }
                }
                AndOrExpression += ")";
                if (rule.AndOrExpression == null)
                {
                    rule.AndOrExpression = new Dictionary<PolicyLevel, string>()
                    {
                        { filterLevel, AndOrExpression }
                    };
                }
                else
                {
                    if (!rule.AndOrExpression.ContainsKey(filterLevel))
                    {
                        rule.AndOrExpression.Add(filterLevel, AndOrExpression);
                    }
                }
            }
        }

        public void ResetAzureFileSOFilter(Rule rule)
        {
            Dictionary<PolicyLevel, List<SOFilterPolicy>> filterGroups = rule.SOFilters.AsQueryable().GroupBy(so => so.Level).ToDictionary(g => g.Key, p => p.ToList());
            rule.PolicyLevel = PolicyLevel.AzureFileDocument;
            foreach (KeyValuePair<PolicyLevel, List<SOFilterPolicy>> filterGroup in filterGroups)
            {
                PolicyLevel filterLevel = filterGroup.Key;
                string AndOrExpression = "(";
                for (int i = 0; i < filterGroup.Value.Count; i++)
                {
                    SOFilterPolicy filterDto = filterGroup.Value[i];
                    filterGroup.Value[i].SequenceNo = i + 1;
                    if (i == filterGroup.Value.Count - 1)
                    {
                        AndOrExpression += string.Format("{0}", filterDto.SequenceNo);
                    }
                    else
                    {
                        AndOrExpression += string.Format("{0} {1} ", filterDto.SequenceNo, filterDto.IsAnd ? "And" : "Or");
                    }
                }
                AndOrExpression += ")";
                if (rule.AndOrExpression == null)
                {
                    rule.AndOrExpression = new Dictionary<PolicyLevel, string>()
                    {
                        { filterLevel, AndOrExpression }
                    };
                }
                else
                {
                    if (!rule.AndOrExpression.ContainsKey(filterLevel))
                    {
                        rule.AndOrExpression.Add(filterLevel, AndOrExpression);
                    }
                }
            }
        }

        private void ResetBoxSOFilter(Rule rule)
        {
            Dictionary<PolicyLevel, List<SOFilterPolicy>> filterGroups = rule.SOFilters.AsQueryable().GroupBy(so => so.Level).ToDictionary(g => g.Key, p => p.ToList());
            rule.PolicyLevel = PolicyLevel.BoxDocument;
            foreach (KeyValuePair<PolicyLevel, List<SOFilterPolicy>> filterGroup in filterGroups)
            {
                PolicyLevel filterLevel = filterGroup.Key;
                string AndOrExpression = "(";
                for (int i = 0; i < filterGroup.Value.Count; i++)
                {
                    SOFilterPolicy filterDto = filterGroup.Value[i];
                    filterGroup.Value[i].SequenceNo = i + 1;
                    if (i == filterGroup.Value.Count - 1)
                    {
                        AndOrExpression += string.Format("{0}", filterDto.SequenceNo);
                    }
                    else
                    {
                        AndOrExpression += string.Format("{0} {1} ", filterDto.SequenceNo, filterDto.IsAnd ? "And" : "Or");
                    }
                }
                AndOrExpression += ")";
                if (rule.AndOrExpression == null)
                {
                    rule.AndOrExpression = new Dictionary<PolicyLevel, string>()
                    {
                        { filterLevel, AndOrExpression }
                    };
                }
                else
                {
                    if (!rule.AndOrExpression.ContainsKey(filterLevel))
                    {
                        rule.AndOrExpression.Add(filterLevel, AndOrExpression);
                    }
                }
            }
        }

        public void ResetConneectorSOFilter(Rule rule)
        {
            Dictionary<PolicyLevel, List<SOFilterPolicy>> filterGroups = rule.SOFilters.AsQueryable().GroupBy(so => so.Level).ToDictionary(g => g.Key, p => p.ToList());
            rule.PolicyLevel = PolicyLevel.Document;
            foreach (KeyValuePair<PolicyLevel, List<SOFilterPolicy>> filterGroup in filterGroups)
            {
                PolicyLevel filterLevel = filterGroup.Key;
                string AndOrExpression = "(";
                for (int i = 0; i < filterGroup.Value.Count; i++)
                {
                    SOFilterPolicy filterDto = filterGroup.Value[i];
                    filterGroup.Value[i].SequenceNo = i + 1;
                    if (i == filterGroup.Value.Count - 1)
                    {
                        AndOrExpression += string.Format("{0}", filterDto.SequenceNo);
                    }
                    else
                    {
                        AndOrExpression += string.Format("{0} {1} ", filterDto.SequenceNo, filterDto.IsAnd ? "And" : "Or");
                    }
                }
                AndOrExpression += ")";
                if (rule.AndOrExpression == null)
                {
                    rule.AndOrExpression = new Dictionary<PolicyLevel, string>()
                    {
                        { filterLevel, AndOrExpression }
                    };
                }
                else
                {
                    if (!rule.AndOrExpression.ContainsKey(filterLevel))
                    {
                        rule.AndOrExpression.Add(filterLevel, AndOrExpression);
                    }
                }
            }
        }

        public Rule BuildRMEXORule(RMRuleInfos info)
        {
            List<SOFilterPolicy> soFilters = new List<SOFilterPolicy>();

            foreach (var filter in info.RuleFilters)
            {
                ArchiverRuleFilter arFilter = new ArchiverRuleFilter();
                arFilter.CombineMode = filter.CombineMode;
                arFilter.SequenceNo = filter.SequenceNo;
                arFilter.Level = filter.Level;
                arFilter.Condition = filter.Condition;
                arFilter.RuleType = filter.RuleType;
                if (!string.IsNullOrEmpty(filter.filterName))
                {
                    arFilter.RuleName = filter.filterName;
                }

                if (arFilter.RuleType == ArchiverFilterRuleType.ModifiedTime ||
                    arFilter.RuleType == ArchiverFilterRuleType.CreatedTime ||
                    arFilter.RuleType == ArchiverFilterRuleType.LastAccessedTime ||
                    arFilter.RuleType == ArchiverFilterRuleType.DateTimeColumn ||
                    arFilter.RuleType == ArchiverFilterRuleType.DateTimeCustomProperty ||
                    arFilter.RuleType == ArchiverFilterRuleType.SendDateUTC)
                {
                    string startDayLightSaving = filter.StartTimeInfo == null ? "true" : filter.StartTimeInfo.IsDayLightSaving.ToString();
                    string endDayLightSaving = filter.EndTimeInfo == null ? "true" : filter.EndTimeInfo.IsDayLightSaving.ToString();
                    if (arFilter.Condition == ArchiverFilterCondition.FromTo)
                    {

                        DateTime startUtcTime = arFilter.SetDateTime(filter.Value1, filter.StartTimeInfo.TimeZoneId, startDayLightSaving, false);
                        DateTime endUtcTime = arFilter.SetDateTime(filter.Value2, filter.EndTimeInfo.TimeZoneId, endDayLightSaving, true);
                        if (DateTime.Parse(filter.Value1) >= DateTime.Parse(filter.Value2))
                        {
                            //throw new InvalidArgumentException(Messages.Get("start_date_after_end_date"));
                            throw new Exception("");
                        }
                        arFilter.Value1 = startUtcTime.ToString(APIDateTimeFormat.DATETYPEForAPI003);
                        arFilter.Value2 = endUtcTime.ToString(APIDateTimeFormat.DATETYPEForAPI003);
                    }
                    else if (arFilter.Condition == ArchiverFilterCondition.Before)
                    {
                        // ValidateValueCount(value, 3);
                        DateTime utcTime = arFilter.SetDateTime(filter.Value1, filter.StartTimeInfo.TimeZoneId, startDayLightSaving, false);
                        arFilter.Value1 = utcTime.ToString(APIDateTimeFormat.DATETYPEForAPI003);
                    }
                    else if (arFilter.Condition == ArchiverFilterCondition.OlderThan)
                    {
                        //ValidateValueCount(value, 1);
                        //SetValueForOlderThan(value[0]);
                        arFilter.Value1 = filter.Value1;
                        arFilter.Value1Unit = filter.Value1Unit;
                    }
                    soFilters.Add(arFilter.Dto);
                }
                else
                {
                    arFilter.Value1 = filter.Value1;
                    if (filter.RuleType == ArchiverFilterRuleType.DocumentSize || filter.RuleType == ArchiverFilterRuleType.SiteCollectionSizeTrigger
                        || filter.RuleType == ArchiverFilterRuleType.Size)
                    {
                        arFilter.Value1Unit = filter.Value1Unit;
                        arFilter.Value2Unit = filter.Value2Unit;
                    }
                    else if (filter.RuleType == ArchiverFilterRuleType.AttachmentCount)
                    {
                        arFilter.Value1Unit = PolicyValueUnit.None;
                        arFilter.Value2Unit = PolicyValueUnit.None;
                    }
                    arFilter.Value2 = filter.Value2;
                    soFilters.Add(arFilter.Dto);
                }
            }
            Rule rule = new Rule();
            //if (!string.IsNullOrEmpty(info.RuleId))
            //{
            //    rule.Id = info.RuleId;
            //}
            //try
            //{
            //    rule.ModifyTime = DateTime.UtcNow.Ticks;
            //}
            //catch (Exception)
            //{
            //    throw new Exception("Please configure general settings first.");//TODO i18n
            //}
            //rule.Name = info.RuleName;
            //rule.Description = info.Description;
            //rule.DisposalClass = info.DisposalClass;
            rule.SOFilters = soFilters;
            rule.ProfileType = GCommon.Contract.Server.Common.Profile.Object.ProfileType.ArchiverRuleForRevIM;
            rule.IncludeNew = "1";
            //rule.DeleteRecords = info.DeleteRecords;
            rule.PolicyLevel = info.RuleLevel;

            #region init rm settings

            if (info.EnableExport == true && info.ExportInfo != null && info.ExportInfo.exportSPDataOption != ExportSPDataOption.None && !(info.ExportInfo.exportType == AvePoint.GCommon.Contract.StorageOptimization.Object.ExportTypeValue.None))
            {
                SetRuleExportLocationInfo(info);
                rule.ExportInfo = info.ExportInfo;
                rule.ExportType = info.ExportInfo.exportType;
                rule.ExportInfo.exportSPDataOption = info.ExportInfo.exportSPDataOption;
                rule.ExportDataBeforeArchiving = info.ExportDataBeforeArchiving;
                if (TenantService.IsNewOpusTenant())
                {
                    rule.ExportInfo.exportLocationId = info.ExportInfo.exportLocationId;
                    rule.ExportInfo.exportLocationName = info.ExportInfo.exportLocationName;
                    rule.ExportInfo.newOptionsOfExportInfo = true;
                    ExportMoveSettingsAsync(rule, info).GetAwaiter().GetResult();
                }
            }
            // }
            rule.MoveToArchiverTierWhenArchiving = info.MoveToArchiverTierWhenArchiving;
            rule.MoveToAnotherTierType = info.MoveToAnotherTierType;
            rule.KeepDataOption = info.RuleKeepDataOption;
            //rule.RelatedRecordOption = info.RelatedRecordOption;
            //需要对tag中datetime类型数据做处理.每个rule最多4个tag
            rule.TagContentInfo = new List<TagContentInfo>();
            if (info.TagContentInfo != null)
            {
                foreach (RMTagContentInfo tag in info.TagContentInfo)
                {
                    if (tag.Type == TagContentInfoType.DateTime)
                    {
                        DateTime dt = DateTime.Parse(tag.Value);
                        tag.DateTime = DateTimeUtil.ConvertTimeToUtcDate(dt, GeneralSettingConfig.FindSystemTimeZoneById(tag.TimeZoneId), !tag.IsDayLightSaving);
                        //tag.Value = dt.ToString(JSDateTimeFormat.DEFAULT_TIME_FORMAT, CultureInfo.InvariantCulture);
                    }
                    rule.TagContentInfo.Add(new TagContentInfo()
                    {
                        ColumnName = tag.ColumnName,
                        DateTime = tag.DateTime,
                        Type = tag.Type,
                        Value = tag.Type == TagContentInfoType.DateTime ? string.Format("{0}/{1}", tag.TimeZoneId, tag.IsDayLightSaving) : tag.Value,
                    });
                }
            }

            //rule.TagContentInfo = info.TagContentInfo;
            InitSORuleManualApprovalInfo(rule, info);
            ResetEXOSOFilter(rule);
            #endregion
            return rule;
        }
        public void ResetEXOSOFilter(Rule rule)
        {
            Dictionary<PolicyLevel, List<SOFilterPolicy>> filterGroups = rule.SOFilters.AsQueryable().GroupBy(so => so.Level).ToDictionary(g => g.Key, p => p.ToList());
            rule.PolicyLevel = PolicyLevel.ExchangeOnlineItem;
            foreach (KeyValuePair<PolicyLevel, List<SOFilterPolicy>> filterGroup in filterGroups)
            {
                PolicyLevel filterLevel = filterGroup.Key;
                string AndOrExpression = "(";
                for (int i = 0; i < filterGroup.Value.Count; i++)
                {
                    SOFilterPolicy filterDto = filterGroup.Value[i];
                    filterGroup.Value[i].SequenceNo = i + 1;
                    if (i == filterGroup.Value.Count - 1)
                    {
                        AndOrExpression += string.Format("{0}", filterDto.SequenceNo);
                    }
                    else
                    {
                        AndOrExpression += string.Format("{0} {1} ", filterDto.SequenceNo, filterDto.IsAnd ? "And" : "Or");
                    }
                }
                AndOrExpression += ")";
                if (rule.AndOrExpression == null)
                {
                    rule.AndOrExpression = new Dictionary<PolicyLevel, string>()
                    {
                        { filterLevel, AndOrExpression }
                    };
                }
                else
                {
                    if (!rule.AndOrExpression.ContainsKey(filterLevel))
                    {
                        rule.AndOrExpression.Add(filterLevel, AndOrExpression);
                    }
                }
            }
        }

        public Rule BuildRMPhysicalRule(RMRuleInfos info)
        {
            List<SOFilterPolicy> soFilters = new List<SOFilterPolicy>();

            foreach (var filter in info.RuleFilters)
            {
                ArchiverRuleFilter arFilter = new ArchiverRuleFilter();
                arFilter.CombineMode = filter.CombineMode;
                arFilter.SequenceNo = filter.SequenceNo;
                arFilter.Level = filter.Level;
                arFilter.Condition = filter.Condition;
                arFilter.RuleType = filter.RuleType;
                if (!string.IsNullOrEmpty(filter.filterName))
                {
                    arFilter.RuleName = filter.filterName;
                }

                if (arFilter.RuleType == ArchiverFilterRuleType.ModifiedTime ||
                    arFilter.RuleType == ArchiverFilterRuleType.CreatedTime ||
                    arFilter.RuleType == ArchiverFilterRuleType.LastAccessedTime ||
                    arFilter.RuleType == ArchiverFilterRuleType.DateTimeColumn ||
                    arFilter.RuleType == ArchiverFilterRuleType.DateTimeCustomProperty ||
                    arFilter.RuleType == ArchiverFilterRuleType.SendDateUTC || 
                    arFilter.RuleType == ArchiverFilterRuleType.LastestSubfolderDisposalDate)
                {
                    string startDayLightSaving = filter.StartTimeInfo == null ? "true" : filter.StartTimeInfo.IsDayLightSaving.ToString();
                    string endDayLightSaving = filter.EndTimeInfo == null ? "true" : filter.EndTimeInfo.IsDayLightSaving.ToString();
                    if (arFilter.Condition == ArchiverFilterCondition.FromTo)
                    {

                        DateTime startUtcTime = arFilter.SetDateTime(filter.Value1, filter.StartTimeInfo.TimeZoneId, startDayLightSaving, false);
                        DateTime endUtcTime = arFilter.SetDateTime(filter.Value2, filter.EndTimeInfo.TimeZoneId, endDayLightSaving, true);
                        if (DateTime.Parse(filter.Value1) >= DateTime.Parse(filter.Value2))
                        {
                            //throw new InvalidArgumentException(Messages.Get("start_date_after_end_date"));
                            throw new Exception("");
                        }
                        arFilter.Value1 = startUtcTime.ToString(APIDateTimeFormat.DATETYPEForAPI003);
                        arFilter.Value2 = endUtcTime.ToString(APIDateTimeFormat.DATETYPEForAPI003);
                    }
                    else if (arFilter.Condition == ArchiverFilterCondition.Before)
                    {
                        // ValidateValueCount(value, 3);
                        DateTime utcTime = arFilter.SetDateTime(filter.Value1, filter.StartTimeInfo.TimeZoneId, startDayLightSaving, false);
                        arFilter.Value1 = utcTime.ToString(APIDateTimeFormat.DATETYPEForAPI003);
                    }
                    else if (arFilter.Condition == ArchiverFilterCondition.OlderThan)
                    {
                        //ValidateValueCount(value, 1);
                        //SetValueForOlderThan(value[0]);
                        arFilter.Value1 = filter.Value1;
                        arFilter.Value1Unit = filter.Value1Unit;
                    }
                    soFilters.Add(arFilter.Dto);
                }
                else
                {
                    arFilter.Value1 = filter.Value1;
                    if (filter.RuleType == ArchiverFilterRuleType.DocumentSize || filter.RuleType == ArchiverFilterRuleType.SiteCollectionSizeTrigger
                        || filter.RuleType == ArchiverFilterRuleType.Size)
                    {
                        arFilter.Value1Unit = filter.Value1Unit;
                        arFilter.Value2Unit = filter.Value2Unit;
                    }
                    else if (filter.RuleType == ArchiverFilterRuleType.AttachmentCount)
                    {
                        arFilter.Value1Unit = PolicyValueUnit.None;
                        arFilter.Value2Unit = PolicyValueUnit.None;
                    }
                    arFilter.Value2 = filter.Value2;
                    soFilters.Add(arFilter.Dto);
                }
            }
            Rule rule = new Rule();
            rule.SOFilters = soFilters;
            rule.ProfileType = GCommon.Contract.Server.Common.Profile.Object.ProfileType.ArchiverRuleForRevIM;
            rule.IncludeNew = "1";
            //rule.DeleteRecords = info.DeleteRecords;
            rule.PolicyLevel = info.RuleLevel;
            rule.MoveToArchiverTierWhenArchiving = info.MoveToArchiverTierWhenArchiving;
            rule.MoveToAnotherTierType = info.MoveToAnotherTierType;
            rule.IsCalculationDisposalDate = info.IsCalculationDisposalDate;
            #region init rm settings

            if (info.EnableExport == true && info.ExportInfo != null && info.ExportInfo.exportSPDataOption == ExportSPDataOption.ExportBeforeArchive && !(info.ExportInfo.exportType == AvePoint.GCommon.Contract.StorageOptimization.Object.ExportTypeValue.None))
            {
                SetRuleExportLocationInfo(info);
                rule.ExportInfo = info.ExportInfo;
                rule.ExportType = info.ExportInfo.exportType;
                rule.ExportInfo.exportLocationId = info.StoragePolicyId;
                rule.ExportInfo.exportLocationName = info.StoragePolicyName;
            }

            rule.KeepDataOption = info.RuleKeepDataOption;
            rule.RelatedRecordOption = info.RelatedRecordOption;
            rule.IsDeleteParentBox = info.DestroyEmptyBoxOnFolderRule;
            rule.StoragePolicyId = info.StoragePolicyId;
            rule.StoragePolicyName = info.StoragePolicyName;

            rule.IsEnableRetention = false;
            rule.RetentionInfo = new();
            rule.IsEnableStoreContentRetention = info.RetentionInfoList != null ? true : info.RetentionInfo != null;
            if (info.RetentionInfo != null)
            {
                info.RetentionInfo.IsEnableRetention = true;
            }
            rule.StoreContentRetentionInfos = ConvertStoreContentRetentionSetting(info.RetentionInfoList ?? new List<RetentionSettings>() { info.RetentionInfo });

            rule.TagContentInfo = new List<TagContentInfo>();
            if (info.TagContentInfo != null)
            {
                foreach (RMTagContentInfo tag in info.TagContentInfo)
                {
                    if (tag.Type == TagContentInfoType.DateTime)
                    {
                        DateTime dt = DateTime.Parse(tag.Value);
                        tag.DateTime = DateTimeUtil.ConvertTimeToUtcDate(dt, GeneralSettingConfig.FindSystemTimeZoneById(tag.TimeZoneId), !tag.IsDayLightSaving);
                        //tag.Value = dt.ToString(JSDateTimeFormat.DEFAULT_TIME_FORMAT, CultureInfo.InvariantCulture);
                    }
                    rule.TagContentInfo.Add(new TagContentInfo()
                    {
                        ColumnName = tag.ColumnName,
                        DateTime = tag.DateTime,
                        Type = tag.Type,
                        Value = tag.Type == TagContentInfoType.DateTime ? string.Format("{0}/{1}", tag.TimeZoneId, tag.IsDayLightSaving) : tag.Value,
                    });
                }
            }
            InitSORuleManualApprovalInfo(rule, info);
            ResetPhysicalSOFilter(rule);
            #endregion
            return rule;
        }

        public void ResetPhysicalSOFilter(Rule rule)
        {
            if (rule.SOFilters != null)
            {
                string AndOrExpression = "(";
                for (int i = 0; i < rule.SOFilters.Count; i++)
                {
                    SOFilterPolicy filterDto = rule.SOFilters[i];
                    filterDto.Level = rule.PolicyLevel;
                    rule.SOFilters[i].SequenceNo = i + 1;
                    if (i == rule.SOFilters.Count - 1)
                    {
                        AndOrExpression += string.Format("{0}", filterDto.SequenceNo);
                    }
                    else
                    {
                        AndOrExpression += string.Format("{0} {1} ", filterDto.SequenceNo, filterDto.IsAnd ? "And" : "Or");
                    }
                }
                AndOrExpression += ")";
                rule.AndOrExpression = new Dictionary<PolicyLevel, string>()
                {
                    { rule.PolicyLevel, AndOrExpression }
                };
            }
        }
        public bool IsExchangeRuleFilter(PolicyLevel level)
        {
            switch (level)
            {
                case PolicyLevel.ExchangeOnlineItem_Message:
                case PolicyLevel.ExchangeOnlineItem_Task:
                case PolicyLevel.ExchangeOnlineItem_Post:
                case PolicyLevel.ExchangeOnlineItem_Event:
                case PolicyLevel.ExchangeOnlineItem_Journal:
                case PolicyLevel.ExchangeOnlineItem_Note:
                case PolicyLevel.ExchangeOnlineItem_Contact:
                case PolicyLevel.ExchangeOnlineItem_Document:
                    return true;
            }
            return false;
        }
        [RACodeReview("Allen Yin")]
        public RMRuleTermsDto GetRuleTermInfos(List<RMRuleTermInfos> infos)
        {
            RMRuleTermsDto rtd = new RMRuleTermsDto();
            rtd.HasTerms = false;
            for (int i = infos.Count - 1; i >= 0; i--)
            {
                var info = infos[i];
                List<string> termNames = TermRuleAssocition.GetTermNamesByRuleId(new Guid(info.RuleId));

                StringBuilder sb = new StringBuilder();
                for (int j = 0; j < termNames.Count; j++)
                {
                    sb.Append(termNames[j]);
                    if (j != termNames.Count - 1)
                    {
                        sb.Append(", ");
                    }
                    rtd.HasTerms = true;
                    rtd.TermsCount++;

                }
                info.TermNames = sb.ToString();
            }
            rtd.Terms = infos;
            return rtd;
        }
        [Audit(Module = AuditModule.ReportCenter, Category = AuditCategory.ReportCenter, Action = AuditAction.ExportRuleUsageReport, AfterHandler = typeof(RuleManagerAfterAuditHandler))]
        public async System.Threading.Tasks.Task GenerateReportForRuleReportAsync(string folderPath, string fileName, string sheetName, string ruleId)
        {
            string reportFilePath = AvePoint.GCommon.Utility.SecurityUtils.SafeCombinePath(folderPath, fileName + ".xlsx");
            string[][] datas = null;
            int countOfOneSheet = 65535;
            List<RMRuleInfos> ruleInfoList = new List<RMRuleInfos>() { await this.LoadRuleAsync(ruleId) };
            List<RMRuleInfos> templeRuleInfoList = new List<RMRuleInfos>();
            int jobReportTotalCount = ruleInfoList.Count;
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }
            try
            {
                if (jobReportTotalCount > 0)
                {
                    for (int i = 1; i < ruleInfoList.Count + 1; i++)
                    {
                        if (templeRuleInfoList.Count > 0 && templeRuleInfoList.Count % countOfOneSheet == 0)
                        {
                            templeRuleInfoList.Add(ruleInfoList[i - 1]);
                            templeRuleInfoList = InsertDataToExcel(reportFilePath, templeRuleInfoList, i, countOfOneSheet, sheetName);

                        }
                        else
                        {
                            templeRuleInfoList.Add(ruleInfoList[i - 1]);
                        }
                    }
                    if (templeRuleInfoList.Count > 0)
                    {
                        InsertDataToExcel(reportFilePath, templeRuleInfoList, jobReportTotalCount, countOfOneSheet, sheetName);
                    }
                }
                else
                {
                    datas = new string[1][];
                    datas[0] = new string[] { I18NEntity.GetString("RM_Common_NoReport") };
                    ReportUtil.CreateExcel(reportFilePath, sheetName + templeRuleInfoList.Count / countOfOneSheet, datas);
                }
            }
            catch (Exception e)
            {
                logger.Debug("generate Report Erro Info:{0},{1}", e.Message, e.StackTrace);
            }
        }

        public List<RMRuleInfos> InsertDataToExcel(string reportFilePath, List<RMRuleInfos> templeRuleInfoList, int currentInsertCount, int maxCountOfOneSheet, string sheetName)
        {

            string[][] datas = new string[17][];
            datas = AssembleRuleInfoHeaderTittle(templeRuleInfoList, datas);
            datas = ConvertRuleInfoToArray(templeRuleInfoList, datas);
            if (currentInsertCount <= maxCountOfOneSheet)
            {
                ReportUtil.CreateExcel(reportFilePath, sheetName, datas);
                templeRuleInfoList.Clear();
            }
            else
            {
                ReportUtil.InsertWorksheet(reportFilePath, sheetName + templeRuleInfoList.Count / maxCountOfOneSheet, datas);
                templeRuleInfoList.Clear();
            }
            return templeRuleInfoList;
        }


        //To DO fs   ???
        private dynamic GetStartColumn(RMRuleInfos reportInfo, string[][] datas)
        {
            dynamic parm = new ExpandoObject();
            string spHeader = datas[0][1];
            string oneDriveHeader = datas[0][2];
            string exoHeader = datas[0][3];
            string phyHeader = datas[0][4];
            string fsHeader = datas[0][5];
            string spLocalHeader = datas[0][6];
            string azureFileHeader = datas[0][7];
            string connectorHeader = datas[0][8];
            string boxHeader = datas[0][9];
            string googleHeader = datas[0][10];
            string teamsHeader = datas[0][11];
            datas[0][1] = datas[0][2] = datas[0][3] = datas[0][4] = datas[0][5] = datas[0][6] = datas[0][7] = datas[0][8] = datas[0][9] = datas[0][10] = datas[0][11] = "";

            int spStartColumn = 0;
            int oneDriveStartColumn = 0;
            int exoStartColumn = 0;
            int phyStartColumn = 0;
            int fsStartColumn = 0;
            int spLocalStartColumn = 0;
            int azureFileStartColumn = 0;
            int connectorStartColumn = 0;
            int boxStartColumn = 0;
            int googleStartColumn = 0;
            int teamsStartColumn = 0;
            int currentIndex = 0;
            if (reportInfo.IsSpSource)
            {
                spStartColumn = ++currentIndex;
                datas[0][spStartColumn] = spHeader;
            }
            if (reportInfo.IsOneDriveSource)
            {
                oneDriveStartColumn = ++currentIndex;
                datas[0][oneDriveStartColumn] = oneDriveHeader;
            }
            if (reportInfo.IsExoSource)
            {
                exoStartColumn = ++currentIndex;
                datas[0][exoStartColumn] = exoHeader;
            }
            if (reportInfo.IsPhySource)
            {
                phyStartColumn = ++currentIndex;
                datas[0][phyStartColumn] = phyHeader;
            }
            if (reportInfo.IsFSSource)
            {
                fsStartColumn = ++currentIndex;
                datas[0][fsStartColumn] = fsHeader;
            }
            if (reportInfo.IsSPLocalSource)
            {
                spLocalStartColumn = ++currentIndex;
                datas[0][spLocalStartColumn] = spLocalHeader;
            }
            if (reportInfo.IsAzureFileSource)
            {
                azureFileStartColumn = ++currentIndex;
                datas[0][azureFileStartColumn] = azureFileHeader;
            }
            if (reportInfo.IsConnectorSource)
            {
                connectorStartColumn = ++currentIndex;
                datas[0][connectorStartColumn] = connectorHeader;
            }
            if (reportInfo.IsBoxSource)
            {
                boxStartColumn = ++currentIndex;
                datas[0][boxStartColumn] = boxHeader;
            }
            if (reportInfo.IsGoogleDriveSource)
            {
                googleStartColumn = ++currentIndex;
                datas[0][googleStartColumn] = googleHeader;
            }
            if (reportInfo.IsTeamsSource)
            {
                teamsStartColumn = ++currentIndex;
                datas[0][teamsStartColumn] = teamsHeader;
            }
            parm.spStartColumn = spStartColumn;
            parm.oneDriveStartColumn = oneDriveStartColumn;
            parm.exoStartColumn = exoStartColumn;
            parm.phytartColumn = phyStartColumn;
            parm.fsStartColumn = fsStartColumn;
            parm.spLocalStartColumn = spLocalStartColumn;
            parm.azureFileStartColumn = azureFileStartColumn;
            parm.connectorStartColumn = connectorStartColumn;
            parm.boxStartColumn = boxStartColumn;
            parm.googleStartColumn = googleStartColumn;
            parm.teamsStartColumn = teamsStartColumn;
            return parm;
        }


        public string[][] ConvertRuleInfoToArray(IEnumerable<RMRuleInfos> reportDetails, string[][] datas)
        {
            RMRuleInfos reportInfo = null;
            foreach (RMRuleInfos report in reportDetails)
            {
                dynamic parm = GetStartColumn(report, datas);
                reportInfo = report as RMRuleInfos;
                if (reportInfo.IsSpSource)
                {
                    InsertRuleInfoToExcel(reportInfo, RMRuleSourceType.SP, datas, parm.spStartColumn);
                }
                if (reportInfo.IsOneDriveSource)
                {
                    InsertRuleInfoToExcel(reportInfo, RMRuleSourceType.OneDrive, datas, parm.oneDriveStartColumn);
                }
                if (reportInfo.IsExoSource)
                {
                    InsertRuleInfoToExcel(reportInfo, RMRuleSourceType.EXO, datas, parm.exoStartColumn);
                }
                if (reportInfo.IsPhySource)
                {
                    InsertRuleInfoToExcel(reportInfo, RMRuleSourceType.Physical, datas, parm.phytartColumn);
                }
                if (reportInfo.IsFSSource)
                {
                    InsertRuleInfoToExcel(reportInfo, RMRuleSourceType.FS, datas, parm.fsStartColumn);
                }
                if (reportInfo.IsSPLocalSource)
                {
                    InsertRuleInfoToExcel(reportInfo, RMRuleSourceType.SPLocal, datas, parm.spLocalStartColumn);
                }
                if (reportInfo.IsAzureFileSource)
                {
                    InsertRuleInfoToExcel(reportInfo, RMRuleSourceType.AzureFile, datas, parm.azureFileStartColumn);
                }
                if (reportInfo.IsConnectorSource)
                {
                    InsertRuleInfoToExcel(reportInfo, RMRuleSourceType.Connector, datas, parm.connectorStartColumn);
                }
                if (reportInfo.IsBoxSource)
                {
                    InsertRuleInfoToExcel(reportInfo, RMRuleSourceType.Box, datas, parm.boxStartColumn);
                }
                if(reportInfo.IsGoogleDriveSource)
                {
                   InsertRuleInfoToExcel(reportInfo, RMRuleSourceType.GoogleDrive, datas, parm.googleStartColumn);
                }
                if (reportInfo.IsTeamsSource)
                {
                    InsertRuleInfoToExcel(reportInfo, RMRuleSourceType.Teams, datas, parm.teamsStartColumn);
                }
            }
            return datas;
        }

        private void InsertRuleInfoToExcel(RMRuleInfos baseRuleInfo, RMRuleSourceType ruleSourceType, string[][] datas, int startColumn)
        {
            var ruleInfo = GetRuleInfoBySourceType(baseRuleInfo, ruleSourceType);
            StringBuilder tempCretias = new StringBuilder();
            if (ruleInfo != null && !ruleInfo.RuleCretias.IsNullOrEmpty())
            {
                for (int i = 0; i < ruleInfo.RuleCretias.Count; i++)
                {
                    tempCretias.Append(ruleInfo.RuleCretias[i] + "\n");
                }
            }
            ArgumentCheck.NotNull(ruleInfo, nameof(ruleInfo));
            var rowColumn = 1;
            datas[rowColumn++][startColumn] = baseRuleInfo.RuleName;
            datas[rowColumn++][startColumn] = baseRuleInfo.Description;
            datas[rowColumn++][startColumn] = baseRuleInfo.ContainerName;
            datas[rowColumn++][startColumn] = ruleSourceType != RMRuleSourceType.Physical ? ConvertPolicyLevelToI18NStr(baseRuleInfo.RuleLevel) : GetPhysicalRuleLevel(baseRuleInfo);
            datas[rowColumn++][startColumn] = baseRuleInfo.DisposalClass;
            datas[rowColumn++][startColumn] = tempCretias.Append(ruleInfo.FilterCombineMode).ToString();
            datas[rowColumn++][startColumn] = ruleSourceType != RMRuleSourceType.SP ? ruleInfo.ArchiverActions : GetSPRuleArchiverActions(ruleInfo);
            datas[rowColumn++][startColumn] = ruleInfo.EnableManualApproval ? I18NEntity.GetString("RM_JS_Common_Yes") : I18NEntity.GetString("RM_JS_Common_No");
            if (_existEnableManualAproval)
            {
                var workflowName = (_existManualWF && ruleInfo.EnableManualApproval) ? ruleInfo.WorkflowName ?? "" : "";
                var sendEmail = ruleInfo.EnableManualApproval ? (ruleInfo.IsSendEmailToOwner ? I18NEntity.GetString("RM_JS_Common_Yes") : I18NEntity.GetString("RM_JS_Common_No")) : "";
                var owners = (_existManualRO && ruleInfo.EnableManualApproval) ? (ruleInfo.Users != null ? string.Join("; ", ruleInfo.Users.Select(u => u.DisplayName)) : "") : "";

                if (_existManualWF) datas[rowColumn++][startColumn] = workflowName;
                datas[rowColumn++][startColumn] = sendEmail;
                if (_existManualRO) datas[rowColumn++][startColumn] = owners;
            }
            if (_existShowExport) datas[rowColumn++][startColumn] = _showExportBeforeActionSources.Contains(ruleSourceType) 
                    ? (ruleInfo.ExportInfo != null && ruleInfo.ExportInfo.exportSPDataOption == ExportSPDataOption.ExportBeforeArchive)
                                                ? I18NEntity.GetString("RM_JS_Common_Yes")
                                                : I18NEntity.GetString("RM_JS_Common_No")
                    : "" ;

            if (_existEnableExport) datas[rowColumn++][startColumn] = 
                    ruleInfo.ExportInfo != null 
                    && (ruleInfo.ExportInfo.exportSPDataOption == ExportSPDataOption.ExportBeforeArchive || ruleInfo.ExportInfo.exportSPDataOption == ExportSPDataOption.ExportWithoutArchive)
                    ? ExportTypeToString(ruleInfo) : "";

            var (isShowStorageInfo, isShowRetentionInfo, isShowArchivedTier) = GetStorageAndRetentionInfo(ruleInfo);

            if (_existShowStorageInfo)
            {
                datas[rowColumn++][startColumn] = ruleInfo.StoragePolicyName;
            }
            if (_existShowArchivedTier)
            {
                datas[rowColumn++][startColumn] = isShowArchivedTier 
                    ? ruleInfo.MoveToArchiverTierWhenArchiving 
                        ? "RM_JS_Rule_DetailValue_ArchiveTier" 
                        : ruleInfo.MoveToAnotherTierType switch
                        {
                            0 => I18NEntity.GetString("RM_JS_Rule_DetailValue_DefaultTier"),
                            3 => I18NEntity.GetString("RM_JS_Rule_DetailValue_ArchiveTier"),
                            4 => I18NEntity.GetString("RM_JS_Rule_DetailValue_ColdTier"),
                            _ => I18NEntity.GetString("RM_JS_Rule_DetailValue_DefaultTier")
                        } //0 default,3 archive,4 cold
                    : "";
            }
            if (_existShowRetentionInfo)
            {
                datas[rowColumn++][startColumn] = isShowRetentionInfo 
                    ? ruleInfo.RetentionInfoList == null 
                    ? ruleInfo.RetentionInfo == null 
                    ? I18NEntity.GetString("RM_JS_Common_No") 
                    : I18NEntity.GetString("RM_JS_Common_Yes") 
                    : I18NEntity.GetString("RM_JS_Common_Yes")
                    : "";
            }
        }

        private string GetSPRuleArchiverActions(RMRuleInfos ruleInfo)
        {
            var archiverActions = ruleInfo.ArchiverActions;
            if (ruleInfo.MoveToRecordCenterSettings != null
                            && ruleInfo.MoveToRecordCenterSettings.DestinationLocation != null
                            && !string.IsNullOrEmpty(ruleInfo.MoveToRecordCenterSettings.DestinationLocation.Url))
            {
                //由于逻辑的是反的，所以false代表勾选declare，true代表没勾选declare
                if (!ruleInfo.MoveToRecordCenterSettings.DelaredRecord)
                {
                    archiverActions = I18NEntity.GetString("RM_JS_RDM_CreateRule_Options_MoveRecord") + ";" + I18NEntity.GetString(AccountUtility.IsSupportRecordLabel() ? "RM_JS_RDM_CreateRule_Options_Move_LockByRecordsLabel" : "RM_JS_RDM_CreateRule_Options_Move_DeclareRecord"); ;
                }
            }
            return archiverActions;
        }

        private string GetPhysicalRuleLevel(RMRuleInfos baseRuleInfo)
        {
            return baseRuleInfo.RuleLevel == PolicyLevel.Folder ?
                           I18NEntity.GetString("RM_Common_ObjectLevel_PhysicalFile") :
                           baseRuleInfo.RuleLevel == PolicyLevel.List ? I18NEntity.GetString("RM_JS_Rule_ObjectLevel_List") : string.Empty;
        }

        private RMRuleInfos GetRuleInfoBySourceType(RMRuleInfos baseRuleInfo, RMRuleSourceType type)
        {
            if (type == RMRuleSourceType.SP)
            {
                return baseRuleInfo;
            }
            if (type == RMRuleSourceType.EXO)
            {
                return baseRuleInfo.EXORule;
            }
            if (type == RMRuleSourceType.FS)
            {
                return baseRuleInfo.FSRule;
            }
            if (type == RMRuleSourceType.SPLocal)
            {
                return baseRuleInfo.SPLocalRule;
            }
            if (type == RMRuleSourceType.Physical)
            {
                return baseRuleInfo.PhysicalRule;
            }
            if (type == RMRuleSourceType.OneDrive)
            {
                return baseRuleInfo.OneDriveRule;
            }
            if (type == RMRuleSourceType.AzureFile)
            {
                return baseRuleInfo.AzureFileRule;
            }
            if (type == RMRuleSourceType.Connector)
            {
                return baseRuleInfo.ConnectorRule;
            }
            if (type == RMRuleSourceType.Box)
            {
                return baseRuleInfo.BoxRule;
            }
            if(type == RMRuleSourceType.GoogleDrive)
            {
                return baseRuleInfo.GoogleDriveRule;
            }
            if(type == RMRuleSourceType.Teams)
            {
                return baseRuleInfo.TeamsRule;
            }
            return null;
        }

        public string[][] AssembleRuleInfoHeaderTittle(IEnumerable<RMRuleInfos> reportDetails, string[][] datas)
        {
            foreach (var reportInfo in reportDetails)
            {
                var sourceMappings = new Dictionary<RMRuleSourceType, (bool IsSource, RMRuleInfos ReportInfo)>
                {
                    { RMRuleSourceType.SP, (reportInfo.IsSpSource, reportInfo) },
                    { RMRuleSourceType.OneDrive, (reportInfo.IsOneDriveSource, reportInfo) },
                    { RMRuleSourceType.SPLocal, (reportInfo.IsSPLocalSource, reportInfo) },
                    { RMRuleSourceType.EXO, (reportInfo.IsExoSource, reportInfo) },
                    { RMRuleSourceType.Physical, (reportInfo.IsPhySource, reportInfo) },
                    { RMRuleSourceType.FS, (reportInfo.IsFSSource, reportInfo) },
                    { RMRuleSourceType.AzureFile, (reportInfo.IsAzureFileSource, reportInfo) },
                    { RMRuleSourceType.Connector, (reportInfo.IsConnectorSource, reportInfo) },
                    { RMRuleSourceType.Box, (reportInfo.IsBoxSource, reportInfo) },
                    { RMRuleSourceType.GoogleDrive, (reportInfo.IsGoogleDriveSource, reportInfo) },
                    { RMRuleSourceType.Teams, (reportInfo.IsTeamsSource, reportInfo) }
                };

                foreach (var (ruleType, (isSource, ruleItem)) in sourceMappings.Where(x => x.Value.IsSource))
                {
                    
                    var ruleInfo = GetRuleInfoBySourceType(reportInfo, ruleType);
                    _existShowExport |= _showExportBeforeActionSources.Contains(ruleType);
                    _existEnableExport |= ruleInfo.EnableExport;
                    _existEnableManualAproval |= ruleInfo.EnableManualApproval;
                    _existManualWF |= !string.IsNullOrEmpty(ruleInfo.WorkflowName);
                    _existManualRO |= ruleInfo.Users != null;

                    var (isShowStorageInfo, isShowRetentionInfo, isShowArchivedTier) = GetStorageAndRetentionInfo(ruleInfo);

                    var isShowArchiverStorageInfo = _showStorageLocationSources.Contains(ruleType);

                    _existShowStorageInfo |= isShowStorageInfo && isShowArchiverStorageInfo;
                    _existShowArchivedTier |= isShowArchivedTier && isShowArchiverStorageInfo;
                    _existShowRetentionInfo |= isShowRetentionInfo && isShowArchiverStorageInfo;
                }
            }
            var totalRowNumber = 17;
            var rowHeaders = new List<string>
            {
                string.Empty,
                "RM_JS_Common_ReportType_SharePoint",
                "RM_JS_SPS_TabLabel_OneDrive",
                "RM_JS_Common_ReportType_Exchange",
                "RM_JS_Common_ReportType_PhysicalItem",
                "RM_JS_Common_ReportType_FileSystem",
                "RM_JS_SPS_TabLabel_SPLocal",
                "RM_JS_Common_ReportType_AzureFile",
                "RM_Connector_Title",
                "RM_JS_SPS_TabLabel_Box",
                "RM_JS_SPS_TabLabel_GoogleDrive",
                "RM_JS_SPS_TabLabel_Teams",
            };
            for (int i = 0; i < totalRowNumber; i++)
            {
                datas[i] = new string[rowHeaders.Count];
            }

            var colNumber = 0;
            foreach (var headerValue in rowHeaders)
            {
                datas[0][colNumber++] = I18NEntity.GetString(headerValue);
            }

            var columnValues = new List<string>
            {
                I18NEntity.GetString("RM_JS_RC_Common_ReportType"),
                I18NEntity.GetString("RM_JS_Rule_Detail_Name"),
                I18NEntity.GetString("RM_JS_RDM_Rule_Description"),
                I18NEntity.GetString("RM_JS_Rule_Detail_RuleContainer"),
                I18NEntity.GetString("RM_JS_Rule_Detail_RuleLevel"),
                I18NEntity.GetString("RM_JS_Rule_DisposalClass_Title"),
                I18NEntity.GetString("RM_JS_Rule_Detail_Criteria"),
                I18NEntity.GetString("RM_JS_Rule_Detail_DWSP"),
                I18NEntity.GetString("RM_JS_Rule_Detail_Approval")
            };

            if (_existEnableManualAproval)
            {
                if (_existManualWF) columnValues.Add(I18NEntity.GetString("RM_JS_Rule_Detail_ProcessName"));
                columnValues.Add(I18NEntity.GetString("RM_JS_MA_Grid_SendEmailRecordOwner"));
                if (_existManualRO) columnValues.Add(I18NEntity.GetString("RM_JS_MA_Grid_RecordOwner"));
            }

            if (_existShowExport) columnValues.Add(I18NEntity.GetString("RM_JS_Rule_Detail_EXSP"));
            if (_existEnableExport) columnValues.Add(I18NEntity.GetString("RM_JS_Rule_Detail_EXFormat"));
            if (_existShowStorageInfo) columnValues.Add(I18NEntity.GetString("RM_TM_Excel_ArchiveStorage"));
            if (_existShowArchivedTier) columnValues.Add(I18NEntity.GetString("RM_JS_Rule_Detail_StoreData"));
            if (_existShowRetentionInfo) columnValues.Add(I18NEntity.GetString("RM_JS_Rule_Detail_Retention"));

            columnValues = columnValues.Where(a => a != null).ToList();

            var rowNumber = 0;
            foreach (var colValue in columnValues)
            {
                datas[rowNumber++][0] = colValue;
            }
            return datas;
        }

        public (bool isShowStorageInfo, bool isShowRetentionInfo, bool isShowArchivedTier) GetStorageAndRetentionInfo(RMRuleInfos ruleItem)
        {
            bool isPhy = ruleItem.RuleLevel == PolicyLevel.PhysicalBox || ruleItem.RuleLevel == PolicyLevel.PhysicalFile;
            bool isFS = ruleItem.RuleLevel == PolicyLevel.FileSysFile;
            bool isShowStorageInfo = false, isShowRetentionInfo = false, isShowArchivedTier = false;
            if (ruleItem.ModelType != RuleModel.SOArchiver)
            {
                var relatedRecordOption = ruleItem.RelatedRecordOption;
                var ruleKeepDataOption = ruleItem.RuleKeepDataOption;

                bool isExportOnly = ruleItem.ExportInfo != null && ruleItem.ExportInfo.exportSPDataOption == ExportSPDataOption.ExportWithoutArchive;
                bool isDeleteRelatedRecordOption = (int)relatedRecordOption == 1;
                bool isArchiveToAzureBlobStorage = ruleKeepDataOption == 1024 || ruleKeepDataOption == 2048;
                bool isNotBackup = true;

                if (!isPhy && !isFS)
                {
                    if ((ruleKeepDataOption == 0 || ruleKeepDataOption == 128) && ruleItem.MoveDto == null && !isExportOnly)
                    {
                        isNotBackup = (ruleKeepDataOption & 256) == 256;
                    }
                }

                isShowStorageInfo = isArchiveToAzureBlobStorage || isDeleteRelatedRecordOption || !isNotBackup;
                isShowRetentionInfo = isArchiveToAzureBlobStorage;
            }
            else
            {
                var keepDataOption = ruleItem.RuleKeepDataOption;
                bool isBackupAndRemove = (keepDataOption & 4096) == 4096 || (keepDataOption & 8192) == 8192;
                isShowStorageInfo = !string.IsNullOrEmpty(ruleItem.StoragePolicyName) && isBackupAndRemove;
            }

            var hiddenArchivedTier = isPhy || ruleItem.StoragePolicyType != (int)StorageType.AzureBlob
                || ruleItem.StoragePolicyId.Equals(DEFAULTSTORAGEID, StringComparison.OrdinalIgnoreCase)
                || ruleItem.IsSystemStorage;


            if (TenantService.IsNewOpusTenant())
            {
                isShowArchivedTier = isShowStorageInfo && !hiddenArchivedTier;
                isShowRetentionInfo = isShowStorageInfo && ruleItem.RuleLevel != PolicyLevel.FileSysFile;
            }
            else
            {
                isShowRetentionInfo = isShowRetentionInfo && ruleItem.RuleLevel != PolicyLevel.FileSysFile;
            }

            return (isShowStorageInfo, isShowRetentionInfo, isShowArchivedTier);
        }

        public string ExportTypeToString(RMRuleInfos reportInfo)
        {
            string tempEnableExportType = string.Empty;
            if (reportInfo.ExportInfo != null)
            {
                switch (reportInfo.ExportInfo.exportType)
                {
                    case AvePoint.GCommon.Contract.StorageOptimization.Object.ExportTypeValue.Autonomy:
                        return tempEnableExportType = I18NEntity.GetString("RM_JS_RDM_CreateRule_ExportType_Autonomy");
                    case AvePoint.GCommon.Contract.StorageOptimization.Object.ExportTypeValue.Concordance:
                        return tempEnableExportType = I18NEntity.GetString("RM_JS_RDM_CreateRule_ExportType_Concordance");
                    case AvePoint.GCommon.Contract.StorageOptimization.Object.ExportTypeValue.EDRM:
                        return tempEnableExportType = I18NEntity.GetString("RM_JS_RDM_CreateRule_ExportType_EDRM");
                    case AvePoint.GCommon.Contract.StorageOptimization.Object.ExportTypeValue.VEO:
                        return tempEnableExportType = I18NEntity.GetString("RM_JS_RDM_CreateRule_ExportType_VEO");
                    case AvePoint.GCommon.Contract.StorageOptimization.Object.ExportTypeValue.NAA:
                        return tempEnableExportType = I18NEntity.GetString("RM_JS_RDM_CreateRule_ExportType_NAA");
                    case AvePoint.GCommon.Contract.StorageOptimization.Object.ExportTypeValue.NARA:
                        return tempEnableExportType = I18NEntity.GetString("RM_JS_RDM_CreateRule_ExportType_NARA");
                    default:
                        return tempEnableExportType = I18NEntity.GetString("RM_JS_RDM_CreateRule_ExportType_None");
                }
            }
            else
            {
                return tempEnableExportType;
            }
        }

        private string ParseErrorMessageFromDAOL(string errorMsg)
        {
            try
            {
                JObject obj = JObject.Parse(errorMsg);
                foreach (var pari in obj)
                {
                    if ("ErrorMessage".Equals(pari.Key.ToString(), StringComparison.OrdinalIgnoreCase))
                    {
                        errorMsg = pari.Value.ToString();
                        if (errorMsg.Contains("An error occur:"))
                        {
                            errorMsg = errorMsg.Replace("An error occur:", "");
                        }
                    }
                }
            }
            catch (Exception)
            {
                logger.Warn("parse error message error");
            }
            return errorMsg;
        }
        public List<RuleDto> GetBaseRulesFromDB()
        {
            var rules = RMRuleDao.GetAllRules();
            return rules.ConvertAll(r => { return new RuleDto() { RuleId = r.RuleId.ToString(), RuleName = r.RuleName, RuleLevel = ConvertPolicyLevelToI18NStr((PolicyLevel)r.RuleLevel) }; });
        }

        public async Task<List<string>> GetBaseRulesNameFromDBAsync(List<string> ids)
        {
            return (await RMRuleDao.FindListAsync(r => ids.Contains(r.RuleId.ToString()))).Select(r => r.RuleName).ToList();
        }

        public async Task<List<RMRuleInfos>> GetSearchRuleFromDBAsync(RuleParameter parameter)
        {
            List<RMRule> rules = new List<RMRule>();
            if (string.IsNullOrEmpty(parameter.SearchValue))
            {
                rules = RMRuleDao.GetAvailableRules(GetRuleModels(), new List<Guid> { parameter.ContainerId }).OrderByDescending(r => r.ModifyTime).ToList();
            }
            else
            {
                rules = RMRuleDao.GetSearchRules(GetRuleModels(), parameter.SearchValue, parameter.ContainerId).OrderByDescending(r => r.ModifyTime).ToList();
            }
            List<RMRuleInfos> rmRules = new List<RMRuleInfos>();
            GeneralSettingModel gls = await mGeneralSettingService.GetGeneralSettingAsync();
            foreach (var r in rules)
            {
                try
                {
                    rmRules.Add(await ConvertToRuleInfoAsync(SerializerHelper.DeserializeByDataContractJsonSerializer<Rule>(r.Extension)));
                }
                catch (Exception e)
                {
                    logger.Warn($"Error occurred while convert rule. Name:{r.RuleName} Error:{e.ToString()}");
                }
            }
            return rmRules;
        }

        public async Task<List<RMRuleInfos>> GetSearchRuleAsync(RulePageRequestModel requestModel) 
        {
            var rules = await RMRuleDao.GetAvailableRulesBySearch(requestModel);
            List<RMRuleInfos> rmRules = [];
            foreach (var r in rules)
            {
                try
                {
                    Rule rule = SerializerHelper.DeserializeByDataContractJsonSerializer<Rule>(r.Extension);
                    rmRules.Add(await ConvertToRuleInfoAsync(rule, true));
                }
                catch (Exception e)
                {
                    logger.Warn($"Error occurred while convert rule. Name:{r.RuleName} Error:{e.ToString()}");
                    rmRules.Add(new()
                    {
                        RuleName = r.RuleName,
                        RuleId = r.RuleId.ToString()
                    });
                }
            }
            return rmRules;
        }

        private List<RuleModel> GetRuleModels()
        {
            List<RuleModel> result = new();
            if (LicenseHelperService.HasOpusILLicense || LicenseHelperService.HasOpusGoogleLicense)
            {
                result.Add(RuleModel.None);
                result.Add(RuleModel.Records);
            }
            if (LicenseHelperService.HasOpusSOLicense)
            {
                result.Add(RuleModel.SOArchiver);
            }
            return result;
        }

        public async Task<List<RMRuleInfos>> GetCanCopyRulesByTermIdAsync(int termId, int moduleType)
        {
            List<Guid> scopeRuleContainers = null;
            List<RMRule> rules = new List<RMRule>();
            if (termId == 0)
            {
                scopeRuleContainers = await SecurityTrimmingHelper.GetRuleScopeAsync();
            }
            else
            {
                scopeRuleContainers = SecurityTrimmingHelper.GetRuleScopeByTermId(TenantLocalValue.LogonGroupId, TenantLocalValue.LogonUserId, termId.ToString());
            }

            if (moduleType == -1)
            {
                rules = RMRuleDao.GetAvailableRules(GetRuleModels(), scopeRuleContainers).OrderByDescending(r => r.ModifyTime).ToList();
            }
            else
            {
                if (moduleType == (int)RuleModel.None) { moduleType = (int)RuleModel.Records; }
                rules = RMRuleDao.GetAvailableRules(new() { (RuleModel)moduleType }, scopeRuleContainers).OrderByDescending(r => r.ModifyTime).ToList();
            }
            List<RMRuleInfos> rmRules = new List<RMRuleInfos>();
            GeneralSettingModel gls = await mGeneralSettingService.GetGeneralSettingAsync();
            foreach (var r in rules)
            {
                rmRules.Add(await ConvertToRuleInfoAsync(SerializerHelper.DeserializeByDataContractJsonSerializer<Rule>(r.Extension)));
            }
            return rmRules;
        }

        public async Task<List<RMRuleInfos>> GetSimpleRulesFromDBAsync(List<Guid> containerIds = null)
        {
            var rules = RMRuleDao.GetAvailableRules(containerIds).OrderByDescending(r => r.ModifyTime).ToList();
            List<RMRuleInfos> rmRules = new List<RMRuleInfos>();
            GeneralSettingModel gls = await mGeneralSettingService.GetGeneralSettingAsync();
            foreach (var r in rules)
            {
                RMRuleInfos rmRule = new RMRuleInfos();
                rmRule.RuleId = r.RuleId.ToString();
                rmRule.RuleName = r.RuleName;
                rmRule.RuleLevel = (PolicyLevel)r.RuleLevel;
                rmRule.Description = r.Description;
                if (r.ModifyTime == 0)
                {
                    rmRule.Modified = string.Empty;
                }
                else
                {
                    rmRule.Modified = mGeneralSettingService.ConvertTiksToDateTime(gls, r.ModifyTime, true).SimplifyFormatTime;
                }
                rmRule.ModifiedTicks = r.ModifyTime;
                rmRule.DisposalClass = r.DisposalClass;
                rmRule.ModelType = (RuleModel)r.ModelType;
                rmRules.Add(rmRule);
            }
            return rmRules;
        }

        public async Task<List<RMRuleInfos>> GetSimpleRecordsRulesFromDBAsync(List<Guid> containerIds = null)
        {
            var rules = RMRuleDao.GetRecordsAvailableRules(containerIds).OrderByDescending(r => r.ModifyTime).ToList();
            List<RMRuleInfos> rmRules = new List<RMRuleInfos>();
            GeneralSettingModel gls = await mGeneralSettingService.GetGeneralSettingAsync();
            foreach (var r in rules)
            {
                if (r.RuleLevel == (int)PolicyLevel.Attachment || r.RuleLevel == (int)PolicyLevel.DocumentVersion || r.RuleLevel == (int)PolicyLevel.ItemVersion)
                {
                    continue;
                }
                RMRuleInfos rmRule = new RMRuleInfos();
                rmRule.RuleId = r.RuleId.ToString();
                rmRule.RuleName = r.RuleName;
                rmRule.RuleLevel = (PolicyLevel)r.RuleLevel;
                rmRule.Description = r.Description;
                if (r.ModifyTime == 0)
                {
                    rmRule.Modified = string.Empty;
                }
                else
                {
                    rmRule.Modified = mGeneralSettingService.ConvertTiksToDateTime(gls, r.ModifyTime, true).SimplifyFormatTime;
                }
                rmRule.ModifiedTicks = r.ModifyTime;
                rmRule.DisposalClass = r.DisposalClass;
                rmRules.Add(rmRule);
            }
            return rmRules;
        }

        public async Task<List<RMRuleInfos>> GetSimpleArchiverRulesFromDBAsync(List<Guid> containerIds = null)
        {
            var rules = RMRuleDao.GetArchiverAvailableRules(containerIds).OrderByDescending(r => r.ModifyTime).ToList();
            List<RMRuleInfos> rmRules = new List<RMRuleInfos>();
            GeneralSettingModel gls = await mGeneralSettingService.GetGeneralSettingAsync();
            foreach (var r in rules)
            {
                RMRuleInfos rmRule = new RMRuleInfos();
                rmRule.RuleId = r.RuleId.ToString();
                rmRule.RuleName = r.RuleName;
                rmRule.RuleLevel = (PolicyLevel)r.RuleLevel;
                rmRule.Description = r.Description;
                if (r.ModifyTime == 0)
                {
                    rmRule.Modified = string.Empty;
                }
                else
                {
                    rmRule.Modified = mGeneralSettingService.ConvertTiksToDateTime(gls, r.ModifyTime, true).SimplifyFormatTime;
                }
                rmRule.ModifiedTicks = r.ModifyTime;
                rmRule.DisposalClass = r.DisposalClass;
                rmRules.Add(rmRule);
            }
            return rmRules;
        }

        //TO DO FS check need this method in disposal job 
        public Rule ConvertToFSRule(Rule rule)
        {
            rule.AndOrExpression = rule.FSRule.AndOrExpression;
            rule.PolicyLevel = rule.FSRule.PolicyLevel;
            //rule.PolicyLevel = rule.FSPolicyLevel == PolicyLevel.Document?PolicyLevel.FileSysFile:PolicyLevel.FileSysFolder;//TO DO yyang
            rule.RelatedRecordOption = rule.FSRule.RelatedRecordOption;
            rule.KeepDataOption = rule.FSRule.KeepDataOption;
            rule.MoveToRecordCenterAndDelareSetting = rule.FSRule.MoveToRecordCenterAndDelareSetting;
            rule.TagContentInfo = rule.FSRule.TagContentInfo == null ? new List<TagContentInfo>() : rule.FSRule.TagContentInfo;
            rule.IsSendEamilToOwner = rule.FSRule.IsSendEamilToOwner;
            rule.ExportInfo = rule.FSRule.ExportInfo;
            rule.ExportType = rule.FSRule.ExportType;
            if (rule.FSRule.Filters == null || rule.FSRule.Filters.Count == 0)
            {
                rule.Filters = new List<FilterPolicy>();
                foreach (var sofilter in rule.FSRule.SOFilters)
                {
                    FilterPolicy p = new FilterPolicy();
                    rule.Filters.Add(p);//TO DO debug...
                }

            }
            rule.Filters = rule.FSRule.Filters;
            rule.IsManualApproval = rule.FSRule.IsManualApproval;

            rule.SOFilters = rule.FSRule.SOFilters;
            rule.UserInfos = rule.FSRule.UserInfos;
            //ResetSOFilter(rule);

            return rule;
        }
        public Rule ConvertToEXORule(Rule rule)
        {
            rule.AndOrExpression = rule.EXORule.AndOrExpression;
            rule.PolicyLevel = rule.EXORule.PolicyLevel;
            //rule.PolicyLevel = rule.FSPolicyLevel == PolicyLevel.Document?PolicyLevel.FileSysFile:PolicyLevel.FileSysFolder;//TO DO yyang
            rule.RelatedRecordOption = rule.EXORule.RelatedRecordOption;
            rule.KeepDataOption = rule.EXORule.KeepDataOption;
            rule.MoveToRecordCenterAndDelareSetting = rule.EXORule.MoveToRecordCenterAndDelareSetting;
            rule.TagContentInfo = rule.EXORule.TagContentInfo == null ? new List<TagContentInfo>() : rule.EXORule.TagContentInfo;
            rule.IsSendEamilToOwner = rule.EXORule.IsSendEamilToOwner;
            rule.ExportInfo = rule.EXORule.ExportInfo;
            rule.ExportType = rule.EXORule.ExportType;
            if (rule.EXORule.Filters == null || rule.EXORule.Filters.Count == 0)
            {
                rule.Filters = new List<FilterPolicy>();
                foreach (var sofilter in rule.EXORule.SOFilters)
                {
                    FilterPolicy p = new FilterPolicy();
                    rule.Filters.Add(p);//TO DO debug...
                }

            }
            rule.Filters = rule.EXORule.Filters;
            rule.IsManualApproval = rule.EXORule.IsManualApproval;

            rule.SOFilters = rule.EXORule.SOFilters;
            rule.UserInfos = rule.EXORule.UserInfos;
            //ResetSOFilter(rule);

            return rule;
        }
        public Rule ConvertToOneDriveRule(Rule rule)
        {
            rule.AndOrExpression = rule.OneDriveRule.AndOrExpression;
            rule.PolicyLevel = rule.OneDriveRule.PolicyLevel;
            //rule.PolicyLevel = rule.FSPolicyLevel == PolicyLevel.Document?PolicyLevel.FileSysFile:PolicyLevel.FileSysFolder;//TO DO yyang
            rule.RelatedRecordOption = rule.OneDriveRule.RelatedRecordOption;
            rule.KeepDataOption = rule.OneDriveRule.KeepDataOption;
            rule.MoveToRecordCenterAndDelareSetting = rule.OneDriveRule.MoveToRecordCenterAndDelareSetting;
            rule.TagContentInfo = rule.OneDriveRule.TagContentInfo == null ? new List<TagContentInfo>() : rule.OneDriveRule.TagContentInfo;
            rule.IsSendEamilToOwner = rule.OneDriveRule.IsSendEamilToOwner;
            rule.ExportInfo = rule.OneDriveRule.ExportInfo;
            rule.ExportType = rule.OneDriveRule.ExportType;
            rule.DeleteRecords = rule.OneDriveRule.DeleteRecords;
            rule.IncludeDeleteRecordLabel = rule.OneDriveRule.IncludeDeleteRecordLabel;
            rule.LockRecordBeforeDestroy = rule.OneDriveRule.LockRecordBeforeDestroy;
            rule.DeleteSiteCollectionToRecycleBin = rule.OneDriveRule.IsDeleteSiteCollectionToRecycleBin((int)SOSourceFlag.OneDrive);
            rule.DeleteToRecycleBin = rule.OneDriveRule.DeleteToRecycleBin;
            rule.DeclareLinkFile = rule.OneDriveRule.DeclareLinkFile;
            rule.LeaveStubMessage = rule.OneDriveRule.LeaveStubMessage;
            rule.spMoveOption = rule.OneDriveRule.spMoveOption;
            if (rule.OneDriveRule.Filters == null || rule.OneDriveRule.Filters.Count == 0)
            {
                rule.Filters = new List<FilterPolicy>();
                foreach (var sofilter in rule.OneDriveRule.SOFilters)
                {
                    FilterPolicy p = new FilterPolicy();
                    rule.Filters.Add(p);//TO DO debug...
                }
            }
            rule.Filters = rule.OneDriveRule.Filters;
            rule.IsManualApproval = rule.OneDriveRule.IsManualApproval;
            rule.IsEnableRetention = rule.OneDriveRule.IsEnableRetention;
            rule.RetentionInfo = rule.OneDriveRule.RetentionInfo;
            rule.SOFilters = rule.OneDriveRule.SOFilters;
            rule.UserInfos = rule.OneDriveRule.UserInfos;
            rule.StubTemplateId = rule.OneDriveRule.StubTemplateId;
            rule.StubTemplateName = rule.OneDriveRule.StubTemplateName;
            rule.StoragePolicyId = rule.OneDriveRule.StoragePolicyId;
            rule.StoragePolicyName = rule.OneDriveRule.StoragePolicyName;
            rule.ProfileType = rule.OneDriveRule.ProfileType;
            rule.MoveToArchiverTierWhenArchiving = rule.OneDriveRule.MoveToArchiverTierWhenArchiving;
            rule.MoveToAnotherTierType = rule.OneDriveRule.MoveToAnotherTierType;
            //ResetSOFilter(rule);
            return rule;
        }

        public Rule ConvertToTeamsRule(Rule rule)
        {
            if(rule.TeamsRule == null)
            {
                return rule;
            }
            rule.AndOrExpression = rule.TeamsRule.AndOrExpression;
            rule.PolicyLevel = rule.TeamsRule.PolicyLevel;
            //rule.PolicyLevel = rule.FSPolicyLevel == PolicyLevel.Document?PolicyLevel.FileSysFile:PolicyLevel.FileSysFolder;//TO DO yyang
            rule.RelatedRecordOption = rule.TeamsRule.RelatedRecordOption;
            rule.KeepDataOption = rule.TeamsRule.KeepDataOption;
            rule.MoveToRecordCenterAndDelareSetting = rule.TeamsRule.MoveToRecordCenterAndDelareSetting;
            rule.TagContentInfo = rule.TeamsRule.TagContentInfo == null ? new List<TagContentInfo>() : rule.TeamsRule.TagContentInfo;
            rule.IsSendEamilToOwner = rule.TeamsRule.IsSendEamilToOwner;
            rule.ExportInfo = rule.TeamsRule.ExportInfo;
            rule.ExportType = rule.TeamsRule.ExportType;
            rule.DeleteRecords = rule.TeamsRule.DeleteRecords;
            rule.DeclareLinkFile = rule.TeamsRule.DeclareLinkFile;
            rule.LeaveStubMessage = rule.TeamsRule.LeaveStubMessage;
            rule.spMoveOption = rule.TeamsRule.spMoveOption;
            if (rule.TeamsRule.Filters == null || rule.TeamsRule.Filters.Count == 0)
            {
                rule.Filters = new List<FilterPolicy>();
                foreach (var sofilter in rule.TeamsRule.SOFilters)
                {
                    FilterPolicy p = new FilterPolicy();
                    rule.Filters.Add(p);//TO DO debug...
                }
            }
            rule.Filters = rule.TeamsRule.Filters;
            rule.IsManualApproval = rule.TeamsRule.IsManualApproval;
            rule.IsEnableRetention = rule.TeamsRule.IsEnableRetention;
            rule.RetentionInfo = rule.TeamsRule.RetentionInfo;
            rule.SOFilters = rule.TeamsRule.SOFilters;
            rule.UserInfos = rule.TeamsRule.UserInfos;
            rule.StubTemplateId = rule.TeamsRule.StubTemplateId;
            rule.StubTemplateName = rule.TeamsRule.StubTemplateName;
            rule.StoragePolicyId = rule.TeamsRule.StoragePolicyId;
            rule.StoragePolicyName = rule.TeamsRule.StoragePolicyName;
            rule.ProfileType = rule.TeamsRule.ProfileType;
            rule.MoveToArchiverTierWhenArchiving = rule.TeamsRule.MoveToArchiverTierWhenArchiving;
            rule.MoveToAnotherTierType = rule.TeamsRule.MoveToAnotherTierType;
            //ResetSOFilter(rule);
            return rule;
        }

        public Rule ConvertToSPLocalRule(Rule rule)
        {
            rule.AndOrExpression = rule.SPLocalRule.AndOrExpression;
            rule.PolicyLevel = rule.SPLocalRule.PolicyLevel;
            //rule.PolicyLevel = rule.FSPolicyLevel == PolicyLevel.Document?PolicyLevel.FileSysFile:PolicyLevel.FileSysFolder;//TO DO yyang
            rule.RelatedRecordOption = rule.SPLocalRule.RelatedRecordOption;
            rule.KeepDataOption = rule.SPLocalRule.KeepDataOption;
            rule.MoveToRecordCenterAndDelareSetting = rule.SPLocalRule.MoveToRecordCenterAndDelareSetting;
            rule.TagContentInfo = rule.SPLocalRule.TagContentInfo == null ? new List<TagContentInfo>() : rule.SPLocalRule.TagContentInfo;
            rule.IsSendEamilToOwner = rule.SPLocalRule.IsSendEamilToOwner;
            rule.ExportInfo = rule.SPLocalRule.ExportInfo;
            rule.ExportType = rule.SPLocalRule.ExportType;
            if (rule.SPLocalRule.Filters == null || rule.SPLocalRule.Filters.Count == 0)
            {
                rule.Filters = new List<FilterPolicy>();
                foreach (var sofilter in rule.SPLocalRule.SOFilters)
                {
                    FilterPolicy p = new FilterPolicy();
                    rule.Filters.Add(p);//TO DO debug...
                }

            }
            rule.Filters = rule.SPLocalRule.Filters;
            rule.IsManualApproval = rule.SPLocalRule.IsManualApproval;

            rule.SOFilters = rule.SPLocalRule.SOFilters;
            rule.UserInfos = rule.SPLocalRule.UserInfos;
            //ResetSOFilter(rule);

            return rule;
        }
        public Rule ConvertToPhysicalRule(Rule rule)
        {
            rule.AndOrExpression = rule.PhysicalRule.AndOrExpression;
            rule.PolicyLevel = rule.PhysicalRule.PolicyLevel;
            rule.RelatedRecordOption = rule.PhysicalRule.RelatedRecordOption;
            rule.KeepDataOption = rule.PhysicalRule.KeepDataOption;
            rule.MoveToRecordCenterAndDelareSetting = rule.PhysicalRule.MoveToRecordCenterAndDelareSetting;
            rule.TagContentInfo = rule.PhysicalRule.TagContentInfo == null ? new List<TagContentInfo>() : rule.PhysicalRule.TagContentInfo;
            rule.IsSendEamilToOwner = rule.PhysicalRule.IsSendEamilToOwner;
            rule.ExportInfo = rule.PhysicalRule.ExportInfo;
            rule.ExportType = rule.PhysicalRule.ExportType;
            if (rule.PhysicalRule.Filters == null || rule.PhysicalRule.Filters.Count == 0)
            {
                rule.Filters = new List<FilterPolicy>();
                foreach (var sofilter in rule.PhysicalRule.SOFilters)
                {
                    FilterPolicy p = new FilterPolicy();
                    rule.Filters.Add(p);//TO DO debug...
                }

            }
            rule.Filters = rule.PhysicalRule.Filters;
            rule.IsManualApproval = rule.PhysicalRule.IsManualApproval;

            rule.SOFilters = rule.PhysicalRule.SOFilters;
            rule.UserInfos = rule.PhysicalRule.UserInfos;
            rule.StoragePolicyId = rule.PhysicalRule.StoragePolicyId;
            rule.StoragePolicyName = rule.PhysicalRule.StoragePolicyName;
            //ResetSOFilter(rule);

            return rule;
        }
        public async Task<List<string[]>> ConvertRuleInfosToListAsync()
        {
            var reportManager = ReportMangerFactory.Instance.ReportManager;
            List<JMJobDetails> jobDetails = [];
            List<RMRuleInfos> rmRules = await GetRuleInfosFromRecordsAsync();
            List<string[]> strRules = new List<string[]>();
            foreach (var r in rmRules)
            {
                if (r.ModelType == RuleModel.SOArchiver)
                {
                    continue;
                }
                if (r.IsSpSource)
                {
                    strRules.AddRange(new ExportSPRuleHelper(r, r, I18NEntity.GetString("RM_TM_Excel_SharePointOnline")).ConvertRuleInfoToArray());
                }
                if (r.IsExoSource)
                {
                    strRules.AddRange(new ExportEXORuleHelper(r, r.EXORule, I18NEntity.GetString("RM_JS_SPS_TabLabel_EXO")).ConvertRuleInfoToArray());
                }
                if (r.IsPhySource)
                {
                    strRules.AddRange(new ExportPhysicalRuleHelper(r, r.PhysicalRule, I18NEntity.GetString("RM_JS_SPS_TabLabel_Physical")).ConvertRuleInfoToArray());
                }

                if (r.IsFSSource)
                {
                    strRules.AddRange(new ExportFSRuleHelper(r, r.FSRule, I18NEntity.GetString("RM_JS_SPS_TabLabel_FS")).ConvertRuleInfoToArray());
                }
                if (r.IsSPLocalSource)
                {
                    strRules.AddRange(new ExportSPLocalRuleHelper(r, r.SPLocalRule, I18NEntity.GetString("RM_JS_SPS_TabLabel_SPLocal")).ConvertRuleInfoToArray());
                }
                if (r.IsOneDriveSource)
                {
                    strRules.AddRange(new ExportOneDriveRuleHelper(r, r.OneDriveRule, I18NEntity.GetString("RM_JS_SPS_TabLabel_OneDrive")).ConvertRuleInfoToArray());
                }

                if (r.IsAzureFileSource)
                {
                    strRules.AddRange(new ExportFSRuleHelper(r, r.AzureFileRule, I18NEntity.GetString("RM_JS_Common_ReportType_AzureFile")).ConvertRuleInfoToArray());
                }

                if (r.IsConnectorSource)
                {
                    strRules.AddRange(new ExportFSRuleHelper(r, r.ConnectorRule, I18NEntity.GetString("RM_JS_Common_ReportType_Connector")).ConvertRuleInfoToArray());
                }

                if (r.IsBoxSource)
                {
                    strRules.AddRange(new ExportBoxRuleHelper(r, r.BoxRule, I18NEntity.GetString("RM_JS_SPS_TabLabel_Box")).ConvertRuleInfoToArray());
                }

                if (r.IsGoogleDriveSource)
                {
                    strRules.AddRange(new ExportGoogleRuleHelper(r, r.GoogleDriveRule, I18NEntity.GetString("RM_JS_SPS_TabLabel_GoogleDrive")).ConvertRuleInfoToArray());
                }
            }
            reportManager.BatchSendJobDetail(jobDetails);
            return strRules;
        }

        public string ConvertPolicyLevelToI18NStr(PolicyLevel level)
        {
            var key = "";
            switch (level)
            {
                case PolicyLevel.WebApplication:
                    key = "RM_JS_Rule_ObjectLevel_WebApplication";
                    break;
                case PolicyLevel.SiteCollection:
                    key = "RM_JS_Rule_ObjectLevel_SiteCollection";
                    break;
                case PolicyLevel.Site:
                    key = "RM_JS_Rule_ObjectLevel_Site";
                    break;
                case PolicyLevel.List:
                case PolicyLevel.Library:
                case PolicyLevel.PhysicalBox:
                    key = "RM_JS_Rule_ObjectLevel_List";
                    break;
                case PolicyLevel.PhysicalFile:
                case PolicyLevel.Folder:
                    key = "RM_JS_Rule_ObjectLevel_Folder";
                    break;
                case PolicyLevel.Item:
                    key = "RM_JS_Rule_ObjectLevel_Item";
                    break;
                case PolicyLevel.Document:
                    key = "RM_JS_Rule_ObjectLevel_Document";
                    break;
                case PolicyLevel.Attachment:
                    key = "RM_JS_Rule_ObjectLevel_Attachment";
                    break;
                case PolicyLevel.DocumentVersion:
                    key = "RM_JS_Rule_ObjectLevel_DocumentVersion";
                    break;
                case PolicyLevel.ItemVersion:
                    key = "RM_JS_Rule_ObjectLevel_ItemVersion";
                    break;
                //case PolicyLevel.AzureFileDocument:
                //    key = "RM_JS_Rule_ObjectLevel_AzureFileDocument";
                //    break;
                default:
                    key = level.ToString();
                    break;
            }
            return I18NEntity.GetString(key);
        }

        public async Task<RAReturnMessage> SyncADUsersAsync(RMRuleInfos ruleInfo)
        {
            var returnMessage = new RAReturnMessage();
            try
            {
                if (ruleInfo.IsSpSource)
                {
                    await RegisterAOSAndUpdateAccountForRuleAsync(ruleInfo);
                }
                if (ruleInfo.IsExoSource)
                {
                    await RegisterAOSAndUpdateAccountForRuleAsync(ruleInfo.EXORule);
                }
                if (ruleInfo.IsPhySource)
                {
                    await RegisterAOSAndUpdateAccountForRuleAsync(ruleInfo.PhysicalRule);
                }
                if (ruleInfo.IsFSSource)
                {
                    await RegisterAOSAndUpdateAccountForRuleAsync(ruleInfo.FSRule);
                }
                if (ruleInfo.IsSPLocalSource)
                {
                    await RegisterAOSAndUpdateAccountForRuleAsync(ruleInfo.SPLocalRule);
                }
                if (ruleInfo.IsOneDriveSource)
                {
                    await RegisterAOSAndUpdateAccountForRuleAsync(ruleInfo.OneDriveRule);
                }
                if (ruleInfo.IsAzureFileSource)
                {
                    await RegisterAOSAndUpdateAccountForRuleAsync(ruleInfo.AzureFileRule);
                }
                if (ruleInfo.IsBoxSource)
                {
                    await RegisterAOSAndUpdateAccountForRuleAsync(ruleInfo.BoxRule);
                }
                if (ruleInfo.IsConnectorSource)
                {
                    await RegisterAOSAndUpdateAccountForRuleAsync(ruleInfo.ConnectorRule);
                }
                if (ruleInfo.IsGoogleDriveSource)
                {
                    await RegisterAOSAndUpdateAccountForRuleAsync(ruleInfo.GoogleDriveRule);
                }
            }
            catch (Exception ex)
            {
                returnMessage.ErrorMessage = I18NEntity.GetString("RM_RegisterUser_Error_Message");
                returnMessage.MessageType = RAMessageType.Failed;
            }
            return returnMessage;
        }

        private async System.Threading.Tasks.Task RegisterAOSAndUpdateAccountForRuleAsync(RMRuleInfos ruleInfo)
        {
            if (ruleInfo.EnableManualApproval && ruleInfo.ManualReviewType == ReviewType.RecordOwner)
            {
                var users = ruleInfo.Users;
                if (users != null && users.Count > 0)
                {
                    await UserService.SyncUsersAsync(TenantLocalValue.LogonGroupId, users);
                    logger.Info($"success to sync user to aos on save rule: {ruleInfo?.RuleName}");
                }
            }

            if (ruleInfo.RetentionInfo != null && ruleInfo.RetentionInfo.IsManualApproval && ruleInfo.RetentionInfo.ReviewType == ReviewType.RecordOwner)
            {
                var uesrs = ruleInfo.RetentionInfo.UserInfos;
                var aosUsers = Convert2AOSUserDtos(uesrs);
                if (aosUsers != null && aosUsers.Count > 0)
                {
                    await UserService.SyncUsersAsync(TenantLocalValue.LogonGroupId, aosUsers);
                    logger.Info($"success to sync user to aos on save rule: {ruleInfo?.RuleName}");
                }
            }
        }

        private UserInfo Convert2RecordOwnerInfo(AOSUserDto dto)
        {
            var recordOwner = new UserInfo
            {
                UserId = dto.UserId,
                DisplayName = dto.DisplayName,
                Email = dto.Email,
                UserPrincipalName = dto.UserPrincipalName,
            };
            switch (dto.InviteType)
            {
                case AccountType.User:
                    recordOwner.InviteType = GCommon.Contract.Server.Login.InviteType.User;
                    break;
                case AccountType.Group:
                    recordOwner.InviteType = GCommon.Contract.Server.Login.InviteType.Group;
                    break;
                default:
                    break;
            }
            return recordOwner;
        }

        private AOSUserDto Convert2AOSUserDto(UserInfo dto)
        {
            var aosUserDto = new AOSUserDto
            {
                Id = dto.Id,
                UserId = dto.UserId,
                DisplayName = dto.DisplayName,
                Email = dto.Email,
                UserPrincipalName = dto.UserPrincipalName,
                TenantId = dto.TenantId,
            };
            switch (dto.InviteType)
            {
                case GCommon.Contract.Server.Login.InviteType.User:
                    aosUserDto.InviteType = AccountType.User;
                    break;
                case GCommon.Contract.Server.Login.InviteType.Group:
                    aosUserDto.InviteType = AccountType.Group;
                    break;
                default:
                    break;
            }
            return aosUserDto;
        }

        public List<AOSUserDto> Convert2AOSUserDtos(List<UserInfo> users)
        {
            List<AOSUserDto> aosUsersDto = null;
            if (users != null && users.Count > 0)
            {
                aosUsersDto = new List<AOSUserDto>();
                users.ForEach(o =>
                {
                    aosUsersDto.Add(Convert2AOSUserDto(o));
                });
            }
            return aosUsersDto;
        }

        public List<UserInfo> Convert2RecordOwnerInfos(List<AOSUserDto> users)
        {
            List<UserInfo> recordOwners = null;
            if (users != null && users.Count > 0)
            {
                recordOwners = new List<UserInfo>();
                users.ForEach(o =>
                {
                    recordOwners.Add(Convert2RecordOwnerInfo(o));
                });
            }
            return recordOwners;
        }

        public async Task<List<RMRuleInfos>> GetExchangeRulesAsync(List<Guid> containerIds = null)
        {
            var rules = RMRuleDao.GetRecordsAvailableRules(containerIds).OrderByDescending(r => r.ModifyTime).ToList();
            List<RMRuleInfos> result = new List<RMRuleInfos>();
            GeneralSettingModel gls = await mGeneralSettingService.GetGeneralSettingAsync();
            foreach (var r in rules)
            {
                if (!string.IsNullOrEmpty(r.Extension))
                {
                    Rule soRule = SerializerHelper.DeserializeByDataContractJsonSerializer<Rule>(r.Extension);
                    if (soRule.EXORule != null)
                    {
                        RMRuleInfos rmRule = new RMRuleInfos
                        {
                            RuleId = r.RuleId.ToString(),
                            RuleName = r.RuleName,
                            RuleLevel = (PolicyLevel)r.RuleLevel,
                            Description = r.Description,
                            Modified = r.ModifyTime == 0 ? string.Empty : mGeneralSettingService.ConvertTiksToDateTime(gls, r.ModifyTime, true).SimplifyFormatTime,
                            ModifiedTicks = r.ModifyTime,
                            DisposalClass = r.DisposalClass
                        };
                        result.Add(rmRule);
                    }
                }
            }
            return result;
        }

        public async Task<List<RMRuleInfos>> GetGoogleRulesAsync(List<Guid> containerIds = null)
        {
            var rules = RMRuleDao.GetRecordsAvailableRules(containerIds).OrderByDescending(r => r.ModifyTime).ToList();
            List<RMRuleInfos> result = [];
            GeneralSettingModel gls = await mGeneralSettingService.GetGeneralSettingAsync();
            foreach (var r in rules)
            {
                if (!string.IsNullOrEmpty(r.Extension))
                {
                    Rule soRule = SerializerHelper.DeserializeByDataContractJsonSerializer<Rule>(r.Extension);
                    if (soRule.GoogleDriveRule != null)
                    {
                        RMRuleInfos rmRule = new RMRuleInfos
                        {
                            RuleId = r.RuleId.ToString(),
                            RuleName = r.RuleName,
                            RuleLevel = (PolicyLevel)r.RuleLevel,
                            Description = r.Description,
                            Modified = r.ModifyTime == 0 ? string.Empty : mGeneralSettingService.ConvertTiksToDateTime(gls, r.ModifyTime, true).SimplifyFormatTime,
                            ModifiedTicks = r.ModifyTime,
                            DisposalClass = r.DisposalClass
                        };
                        result.Add(rmRule);
                    }
                }
            }
            return result;
        }

        public async Task<(List<RMRuleInfos>, List<Guid>)> GetGoogleRulesAndMixedRuleIdsAsync()
        {
            var rules = RMRuleDao.GetRecordsAvailableRules(null).OrderByDescending(r => r.ModifyTime).ToList();
            List<RMRuleInfos> googleRules = [];
            List<Guid> mixedRuleIds = [];
            GeneralSettingModel gls = await mGeneralSettingService.GetGeneralSettingAsync();
            foreach (var r in rules)
            {
                if (!string.IsNullOrEmpty(r.Extension))
                {
                    Rule soRule = SerializerHelper.DeserializeByDataContractJsonSerializer<Rule>(r.Extension);
                    if (soRule.GoogleDriveRule != null)
                    {
                        RMRuleInfos rmRule = new RMRuleInfos
                        {
                            RuleId = r.RuleId.ToString(),
                            RuleName = r.RuleName,
                            RuleLevel = (PolicyLevel)r.RuleLevel,
                            Description = r.Description,
                            Modified = r.ModifyTime == 0 ? string.Empty : mGeneralSettingService.ConvertTiksToDateTime(gls, r.ModifyTime, true).SimplifyFormatTime,
                            ModifiedTicks = r.ModifyTime,
                            DisposalClass = r.DisposalClass
                        };
                        googleRules.Add(rmRule);

                        var isMixedRule = soRule.SOFilters.IsNotNullOrEmpty() ||
                          (soRule.EXORule?.SOFilters != null) ||
                          (soRule.FSRule?.SOFilters != null) ||
                          (soRule.OneDriveRule?.SOFilters != null) ||
                          (soRule.PhysicalRule?.SOFilters != null) ||
                          (soRule.SPLocalRule?.SOFilters != null) ||
                          (soRule.AzureFileRule?.SOFilters != null) ||
                          (soRule.BoxRule?.SOFilters != null) ||
                          (soRule.ConnectorRule?.SOFilters != null);

                        if (isMixedRule)
                        {
                            mixedRuleIds.Add(r.RuleId);
                        }

                    }

                }
            }
            return (googleRules, mixedRuleIds);
        }

        public async Task<List<RMRuleInfos>> GetRulesByDataSourceAsync(int dataSource, List<Guid> containerIds = null)
        {
            var rules = RMRuleDao.GetRecordsAvailableRules(containerIds).OrderByDescending(r => r.ModifyTime).ToList();
            List<RMRuleInfos> result = new List<RMRuleInfos>();
            GeneralSettingModel gls = await mGeneralSettingService.GetGeneralSettingAsync();
            foreach (var r in rules)
            {
                if (!string.IsNullOrEmpty(r.Extension))
                {
                    Rule soRule = SerializerHelper.DeserializeByDataContractJsonSerializer<Rule>(r.Extension);
                    if (dataSource == (int)AvePoint.RA.Contract.Explorer.SourceFlag.OneDrive)
                    {
                        if (soRule.OneDriveRule != null)
                        {
                            RMRuleInfos rmRule = new RMRuleInfos
                            {
                                RuleId = r.RuleId.ToString(),
                                RuleName = r.RuleName,
                                RuleLevel = (PolicyLevel)r.RuleLevel,
                                Description = r.Description,
                                Modified = r.ModifyTime == 0 ? string.Empty : mGeneralSettingService.ConvertTiksToDateTime(gls, r.ModifyTime, true).SimplifyFormatTime,
                                ModifiedTicks = r.ModifyTime,
                                DisposalClass = r.DisposalClass
                            };
                            result.Add(rmRule);
                        }
                    }
                    else if (dataSource == (int)AvePoint.RA.Contract.Explorer.SourceFlag.SharePoint)
                    {
                        if (soRule.SOFilters != null && soRule.SOFilters.Count > 0)
                        {
                            RMRuleInfos rmRule = new RMRuleInfos
                            {
                                RuleId = r.RuleId.ToString(),
                                RuleName = r.RuleName,
                                RuleLevel = (PolicyLevel)r.RuleLevel,
                                Description = r.Description,
                                Modified = r.ModifyTime == 0 ? string.Empty : mGeneralSettingService.ConvertTiksToDateTime(gls, r.ModifyTime, true).SimplifyFormatTime,
                                ModifiedTicks = r.ModifyTime,
                                DisposalClass = r.DisposalClass
                            };
                            result.Add(rmRule);
                        }
                    }
                }
            }
            return result;
        }

        public async Task<List<RMRuleInfos>> GetArchiverRulesByDataSourceAsync(int dataSource, List<Guid> containerIds = null)
        {
            var rules = RMRuleDao.GetArchiverAvailableRules(containerIds).OrderByDescending(r => r.ModifyTime).ToList();
            List<RMRuleInfos> result = new List<RMRuleInfos>();
            GeneralSettingModel gls = await mGeneralSettingService.GetGeneralSettingAsync();
            foreach (var r in rules)
            {
                if (!string.IsNullOrEmpty(r.Extension))
                {
                    Rule soRule = SerializerHelper.DeserializeByDataContractJsonSerializer<Rule>(r.Extension);
                    if (dataSource == (int)AvePoint.RA.Contract.Explorer.SourceFlag.OneDrive)
                    {
                        if (soRule.OneDriveRule != null)
                        {
                            RMRuleInfos rmRule = new RMRuleInfos
                            {
                                RuleId = r.RuleId.ToString(),
                                RuleName = r.RuleName,
                                RuleLevel = (PolicyLevel)r.RuleLevel,
                                Description = r.Description,
                                Modified = r.ModifyTime == 0 ? string.Empty : mGeneralSettingService.ConvertTiksToDateTime(gls, r.ModifyTime, true).SimplifyFormatTime,
                                ModifiedTicks = r.ModifyTime,
                                DisposalClass = r.DisposalClass
                            };
                            result.Add(rmRule);
                        }
                    }
                    else if (dataSource == (int)AvePoint.RA.Contract.Explorer.SourceFlag.SharePoint)
                    {
                        if (soRule.SOFilters != null && soRule.SOFilters.Count > 0)
                        {
                            RMRuleInfos rmRule = new RMRuleInfos
                            {
                                RuleId = r.RuleId.ToString(),
                                RuleName = r.RuleName,
                                RuleLevel = (PolicyLevel)r.RuleLevel,
                                Description = r.Description,
                                Modified = r.ModifyTime == 0 ? string.Empty : mGeneralSettingService.ConvertTiksToDateTime(gls, r.ModifyTime, true).SimplifyFormatTime,
                                ModifiedTicks = r.ModifyTime,
                                DisposalClass = r.DisposalClass
                            };
                            result.Add(rmRule);
                        }
                    }
                    else if (dataSource == (int)AvePoint.RA.Contract.Explorer.SourceFlag.Teams)
                    {
                        if (r.RuleLevel == (int)PolicyLevel.Teams)
                        {
                            if (soRule.TeamsRule != null && soRule.TeamsRule.SOFilters != null && soRule.TeamsRule.SOFilters.Count > 0)
                            {
                                RMRuleInfos rmRule = new RMRuleInfos
                                {
                                    RuleId = r.RuleId.ToString(),
                                    RuleName = r.RuleName,
                                    RuleLevel = (PolicyLevel)r.RuleLevel,
                                    Description = r.Description,
                                    Modified = r.ModifyTime == 0 ? string.Empty : mGeneralSettingService.ConvertTiksToDateTime(gls, r.ModifyTime, true).SimplifyFormatTime,
                                    ModifiedTicks = r.ModifyTime,
                                    DisposalClass = r.DisposalClass
                                };
                                result.Add(rmRule);
                            }
                        }
                        else
                        {
                            if (soRule.SOFilters != null && soRule.SOFilters.Count > 0)
                            {
                                RMRuleInfos rmRule = new RMRuleInfos
                                {
                                    RuleId = r.RuleId.ToString(),
                                    RuleName = r.RuleName,
                                    RuleLevel = (PolicyLevel)r.RuleLevel,
                                    Description = r.Description,
                                    Modified = r.ModifyTime == 0 ? string.Empty : mGeneralSettingService.ConvertTiksToDateTime(gls, r.ModifyTime, true).SimplifyFormatTime,
                                    ModifiedTicks = r.ModifyTime,
                                    DisposalClass = r.DisposalClass
                                };
                                result.Add(rmRule);
                            }
                        }
                    }
                }
            }
            return result;
        }

        private void SetRuleExportLocationInfo(RMRuleInfos info)
        {
            RMCPGlobalStorageSetting rmSettings = GlobalStorageSettingDao.GetGlobalSettingInfoFromRA();
            var configureExportLocation = I18NEntity.GetString("RM_RDM_Rule_ConfigureExportLocation");
            if (rmSettings == null)
            {
                if (info.EnableExport && info.ExportInfo is { exportSPDataOption: ExportSPDataOption.ExportBeforeArchive } && info.ExportInfo.exportType != AvePoint.GCommon.Contract.StorageOptimization.Object.ExportTypeValue.None)
                {
                    throw new Exception(configureExportLocation);
                }
            }
            
            if (TenantService.IsNewOpusTenant() && info.MoveDto == null)
            {
                var isValidStorage = StorageDeviceService.ValidateExportStorageInfo(info.ExportInfo.exportLocationId);
                if (!isValidStorage)
                {
                    throw new Exception(configureExportLocation);
                }
            }

            if (!TenantService.IsNewOpusTenant())
            {
                DAOAPIClientV1 Client = new DAOAPIClientV1();

                if (info.MoveDto != null)
                {
                    throw new Exception(configureExportLocation);
                }

                if (rmSettings?.ExportLocationId == Guid.Empty)
                {
                    throw new Exception(configureExportLocation);
                }

                if (Client.GetExportLocationbyId(rmSettings?.ExportLocationId.ToString()) == null)
                {
                    throw new Exception(string.Format(I18NEntity.GetString("RM_EL_NoExportLocation"), rmSettings?.ExportLocationName));
                }
                info.ExportInfo!.exportLocationId = rmSettings?.ExportLocationId.ToString();
                info.ExportInfo.exportLocationName = rmSettings?.ExportLocationName;
            }
        }
        private void SetRuleGoogleExportLocationInfo(RMRuleInfos info)
        {
            RMCPGlobalStorageSetting rmSettings = GlobalStorageSettingDao.GetGlobalSettingInfoFromRA();
            var configureExportLocation = I18NEntity.GetString("RM_RDM_Rule_ConfigureExportLocation");
            if (rmSettings == null)
            {
                if (info.EnableExport && info.ExportInfo is { exportSPDataOption: ExportSPDataOption.ExportBeforeArchive } && info.ExportInfo.exportType != AvePoint.GCommon.Contract.StorageOptimization.Object.ExportTypeValue.None)
                {
                    throw new Exception(configureExportLocation);
                }
            }

            if (TenantService.IsNewOpusTenant() && info.MoveDto == null)
            {
                var isValidStorage = StorageDeviceService.ValidateExportGoogleStorageInfo(info.ExportInfo.exportLocationId);
                if (!isValidStorage)
                {
                    throw new Exception(configureExportLocation);
                }
            }

            if (!TenantService.IsNewOpusTenant())
            {
                DAOAPIClientV1 Client = new DAOAPIClientV1();

                if (info.MoveDto != null)
                {
                    throw new Exception(configureExportLocation);
                }

                if (rmSettings?.ExportLocationId == Guid.Empty)
                {
                    throw new Exception(configureExportLocation);
                }

                if (Client.GetExportLocationbyId(rmSettings?.ExportLocationId.ToString()) == null)
                {
                    throw new Exception(string.Format(I18NEntity.GetString("RM_EL_NoExportLocation"), rmSettings?.ExportLocationName));
                }
                info.ExportInfo!.exportLocationId = rmSettings?.ExportLocationId.ToString();
                info.ExportInfo.exportLocationName = rmSettings?.ExportLocationName;
            }
        }


        private bool NeedShowStoragePolicy(RMRuleInfos rule)
        {
            //export
            if (rule.EnableExport && rule.ExportInfo != null && rule.ExportInfo.exportSPDataOption == ExportSPDataOption.ExportWithoutArchive)
            {
                return false;
            }
            //move
            if (rule.MoveDto != null)
            {
                return false;
            }
            //keep data
            if ((rule.RuleKeepDataOption & 16) == 16)
            {
                return false;
            }

            List<AvePoint.GCommon.Contract.CommonFilter.PolicyLevel> ruleLevels = new List<AvePoint.GCommon.Contract.CommonFilter.PolicyLevel>()
            {
                AvePoint.GCommon.Contract.CommonFilter.PolicyLevel.SiteCollection,
                AvePoint.GCommon.Contract.CommonFilter.PolicyLevel.Site,
                AvePoint.GCommon.Contract.CommonFilter.PolicyLevel.List,
                AvePoint.GCommon.Contract.CommonFilter.PolicyLevel.Folder,
                AvePoint.GCommon.Contract.CommonFilter.PolicyLevel.Item,
                AvePoint.GCommon.Contract.CommonFilter.PolicyLevel.ItemVersion,
                AvePoint.GCommon.Contract.CommonFilter.PolicyLevel.Attachment,
                AvePoint.GCommon.Contract.CommonFilter.PolicyLevel.Document,
                AvePoint.GCommon.Contract.CommonFilter.PolicyLevel.DocumentVersion
            };
            if (ruleLevels.Contains(rule.RuleLevel) && rule.RuleKeepDataOption == (int)KeepDataStatus.Delete)
            {
                return true;
            }

            if (rule.RuleLevel == AvePoint.GCommon.Contract.CommonFilter.PolicyLevel.Document && (rule.RuleKeepDataOption == (int)KeepDataStatus.Archive || rule.RuleKeepDataOption == (int)KeepDataStatus.ArchiveAndLeaveStub))
            {
                return true;
            }
            if (rule.IsSpSource && rule.RelatedRecordOption == GCommon.Contract.StorageOptimization.Object.RelatedRecordOption.Both)
            {
                return true;
            }

            if (rule.ModelType == RuleModel.SOArchiver && (rule.RuleKeepDataOption == (int)KeepDataStatus.ArchiveBackupAndRemove || rule.RuleKeepDataOption == (int)KeepDataStatus.ArchiveBackupAndRemoveLeaveStub))
            {
                return true;
            }

            return false;
        }


        public void EnableInsightsDataCollection(List<RuleFilter> filters)
        {
            List<ArchiverFilterRuleType> needEnableDataCollectionRuleTypes = new List<ArchiverFilterRuleType> {
                ArchiverFilterRuleType.LastAccessedTime, ArchiverFilterRuleType.LastActiveTime
            };
            if (filters != null && filters.Count > 0)
            {
                if (filters.Any(o => needEnableDataCollectionRuleTypes.Contains(o.RuleType)))
                {
                    SetDataCollectionStatus(true);
                    logger.Info("Enable Data Collection for insights is successfully.");
                }
            }
        }

        private void SetDataCollectionStatus(bool status)
        {
            try
            {
                AosApiUtility.GetCloudInsightsClient().SettingsService.SetAnalysisStatus(status).GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                logger.Warn($"An error while set data collection analysis status, {ex}");
                throw new EnableDataCollectionStatusException();
            }
        }

        public async Task<List<RMRuleInfos>> GetCanCopyRulesForDisableClassificationAsync(int moduleType)
        {
            List<Guid> scopeRuleContainers = null;
            List<RMRule> rules = new List<RMRule>();
          
            scopeRuleContainers = await SecurityTrimmingHelper.GetRuleScopeAsync();
          
            if (moduleType == -1)
            {
                rules = RMRuleDao.GetAvailableRules(GetRuleModels(), scopeRuleContainers).OrderByDescending(r => r.ModifyTime).ToList();
            }
            else
            {
                if (moduleType == (int)RuleModel.None) { moduleType = (int)RuleModel.Records; }
                rules = RMRuleDao.GetAvailableRules(new() { (RuleModel)moduleType }, scopeRuleContainers).OrderByDescending(r => r.ModifyTime).ToList();
            }
            List<RMRuleInfos> rmRules = new List<RMRuleInfos>();
            GeneralSettingModel gls = await mGeneralSettingService.GetGeneralSettingAsync();
            foreach (var r in rules)
            {
                try
                {
                    var rule = SerializerHelper.DeserializeByDataContractJsonSerializer<Rule>(r.Extension);
                    rmRules.Add(await ConvertToRuleInfoAsync(rule));
                }
                catch (Exception ex)
                {
                    logger.Error($"Failed to process rule with Rule Name: {r.RuleName}. Error: {ex.Message}");
                }
            }
            return rmRules;
        }

        public async Task BuildManualAprovalJobScheduleForCreateRule(RMRuleInfos ruleInfo)
        {
            //enable manual approval, create manula approval job schedule
            if ((ruleInfo.EnableManualApproval || (ruleInfo.RetentionInfo != null && ruleInfo.RetentionInfo.IsManualApproval))
                || (ruleInfo.EXORule != null && ruleInfo.EXORule.EnableManualApproval)
                || (ruleInfo.PhysicalRule != null && ruleInfo.PhysicalRule.EnableManualApproval)
                || (ruleInfo.FSRule != null && ruleInfo.FSRule.EnableManualApproval)
                || (ruleInfo.SPLocalRule != null && ruleInfo.SPLocalRule.EnableManualApproval)
                || (ruleInfo.OneDriveRule != null && (ruleInfo.OneDriveRule.EnableManualApproval || (ruleInfo.OneDriveRule.RetentionInfo != null && ruleInfo.OneDriveRule.RetentionInfo.IsManualApproval)))
                || (ruleInfo.AzureFileRule != null && ruleInfo.AzureFileRule.EnableManualApproval)
                || (ruleInfo.BoxRule != null && ruleInfo.BoxRule.EnableManualApproval)
                || (ruleInfo.ConnectorRule != null && ruleInfo.ConnectorRule.EnableManualApproval)
                || (ruleInfo.GoogleDriveRule != null && ruleInfo.GoogleDriveRule.EnableManualApproval)
                || (ruleInfo.TeamsRule != null && ruleInfo.TeamsRule.EnableManualApproval))
            {
                await ScheduleService.CreateCustomScheduleAsync(false, ScheduleType.ManualApprovalEmailSchedule);
            }

            if (TenantService.IsNewOpusTenant() && ruleInfo.ModelType == RuleModel.Records)
            {
                //enable manual approval, create manula approval job schedule
                if ((ruleInfo.FSRule != null && ruleInfo.FSRule.EnableManualApproval)
                    || (ruleInfo.SPLocalRule != null && ruleInfo.SPLocalRule.EnableManualApproval)
                    || (ruleInfo.AzureFileRule != null && ruleInfo.AzureFileRule.EnableManualApproval)
                    || (ruleInfo.ConnectorRule != null && ruleInfo.ConnectorRule.EnableManualApproval)
                    || (ruleInfo.GoogleDriveRule != null && ruleInfo.GoogleDriveRule.EnableManualApproval)
                    || (ruleInfo.TeamsRule != null && ruleInfo.TeamsRule.EnableManualApproval))
                {
                    await ScheduleService.CreateCustomScheduleAsync(false, ScheduleType.ManualApprovalScheduleTimer);
                }
            }
            else
            {
                //enable manual approval, create manula approval job schedule
                if ((ruleInfo.EnableManualApproval || (ruleInfo.RetentionInfo != null && ruleInfo.RetentionInfo.IsManualApproval))
                    || (ruleInfo.EXORule != null && ruleInfo.EXORule.EnableManualApproval)
                    || (ruleInfo.PhysicalRule != null && ruleInfo.PhysicalRule.EnableManualApproval)
                    || (ruleInfo.FSRule != null && ruleInfo.FSRule.EnableManualApproval)
                    || (ruleInfo.SPLocalRule != null && ruleInfo.SPLocalRule.EnableManualApproval)
                    || (ruleInfo.OneDriveRule != null && (ruleInfo.OneDriveRule.EnableManualApproval || (ruleInfo.OneDriveRule.RetentionInfo != null && ruleInfo.OneDriveRule.RetentionInfo.IsManualApproval)))
                    || (ruleInfo.AzureFileRule != null && ruleInfo.AzureFileRule.EnableManualApproval)
                    || (ruleInfo.ConnectorRule != null && ruleInfo.ConnectorRule.EnableManualApproval)
                    || (ruleInfo.GoogleDriveRule != null && ruleInfo.GoogleDriveRule.EnableManualApproval)
                    || (ruleInfo.TeamsRule != null && ruleInfo.TeamsRule.EnableManualApproval))
                {
                    await ScheduleService.CreateCustomScheduleAsync(false, ScheduleType.ManualApprovalScheduleTimer);
                }
            }
        }

        public async Task BuildManualAprovalJobScheduleForEditRule(RMRuleInfos ruleInfo)
        {
            if ((ruleInfo.EnableManualApproval || (ruleInfo.RetentionInfo != null && ruleInfo.RetentionInfo.IsManualApproval))
                                 || (ruleInfo.EXORule != null && ruleInfo.EXORule.EnableManualApproval)
                                 || (ruleInfo.PhysicalRule != null && ruleInfo.PhysicalRule.EnableManualApproval)
                                 || (ruleInfo.FSRule != null && ruleInfo.FSRule.EnableManualApproval)
                                 || (ruleInfo.SPLocalRule != null && ruleInfo.SPLocalRule.EnableManualApproval)
                                 || (ruleInfo.OneDriveRule != null && (ruleInfo.OneDriveRule.EnableManualApproval || (ruleInfo.OneDriveRule.RetentionInfo != null && ruleInfo.OneDriveRule.RetentionInfo.IsManualApproval)))
                                 || (ruleInfo.AzureFileRule != null && ruleInfo.AzureFileRule.EnableManualApproval)
                                 || (ruleInfo.BoxRule != null && ruleInfo.BoxRule.EnableManualApproval)
                                 || (ruleInfo.ConnectorRule != null && ruleInfo.ConnectorRule.EnableManualApproval)
                                 || (ruleInfo.GoogleDriveRule != null && ruleInfo.GoogleDriveRule.EnableManualApproval)
                                 || (ruleInfo.TeamsRule != null && ruleInfo.TeamsRule.EnableManualApproval))
            {
                await ScheduleService.CreateCustomScheduleAsync(false, ScheduleType.ManualApprovalEmailSchedule);
            }

            if (TenantService.IsNewOpusTenant() && ruleInfo.ModelType == RuleModel.Records)
            {
                //enable manual approval, create manula approval job schedule
                if ((ruleInfo.FSRule != null && ruleInfo.FSRule.EnableManualApproval)
                    || (ruleInfo.SPLocalRule != null && ruleInfo.SPLocalRule.EnableManualApproval)
                    || (ruleInfo.AzureFileRule != null && ruleInfo.AzureFileRule.EnableManualApproval)
                    || (ruleInfo.ConnectorRule != null && ruleInfo.ConnectorRule.EnableManualApproval)
                    || (ruleInfo.GoogleDriveRule != null && ruleInfo.GoogleDriveRule.EnableManualApproval)
                    || (ruleInfo.TeamsRule != null && ruleInfo.TeamsRule.EnableManualApproval))
                {
                    await ScheduleService.CreateCustomScheduleAsync(false, ScheduleType.ManualApprovalScheduleTimer);
                }
            }
            else
            {
                //enable manual approval, create manula approval job schedule
                if ((ruleInfo.EnableManualApproval || (ruleInfo.RetentionInfo != null && ruleInfo.RetentionInfo.IsManualApproval))
                    || (ruleInfo.EXORule != null && ruleInfo.EXORule.EnableManualApproval)
                    || (ruleInfo.PhysicalRule != null && ruleInfo.PhysicalRule.EnableManualApproval)
                    || (ruleInfo.FSRule != null && ruleInfo.FSRule.EnableManualApproval)
                    || (ruleInfo.SPLocalRule != null && ruleInfo.SPLocalRule.EnableManualApproval)
                    || (ruleInfo.OneDriveRule != null && (ruleInfo.OneDriveRule.EnableManualApproval || (ruleInfo.OneDriveRule.RetentionInfo != null && ruleInfo.OneDriveRule.RetentionInfo.IsManualApproval)))
                    || (ruleInfo.AzureFileRule != null && ruleInfo.AzureFileRule.EnableManualApproval)
                    || (ruleInfo.ConnectorRule != null && ruleInfo.ConnectorRule.EnableManualApproval)
                    || (ruleInfo.GoogleDriveRule != null && ruleInfo.GoogleDriveRule.EnableManualApproval)
                    || (ruleInfo.TeamsRule != null && ruleInfo.TeamsRule.EnableManualApproval))
                {
                    await ScheduleService.CreateCustomScheduleAsync(false, ScheduleType.ManualApprovalScheduleTimer);
                }
            }
        }
    }
}
