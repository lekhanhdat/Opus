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
using Aspose.Cells;
using AvePoint.GCommon;
using AvePoint.GCommon.Contract.CommonFilter;
using AvePoint.GCommon.Contract.StorageOptimization.Object;
using AvePoint.RA.Common;
using AvePoint.RA.Common.Configurations;
using AvePoint.RA.Common.Report;
using AvePoint.RA.Common.Util;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract;
using AvePoint.RA.Contract.Configurations;
using AvePoint.RA.Contract.DocAve;
using AvePoint.RA.Contract.JPMC;
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.I18N.Core;
using AvePoint.RA.SharePoint.Common;
using AvePoint.RA.SharePoint.Extension;
using AvePoint.Wrapper.Common;
using Microsoft.SharePoint.Client;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace AvePoint.RA.SharePoint.RMSharePointTaxnomy
{

    public class RMSyncCustomization4JPMC
    {
        #region Const
        private const string configListName = "OpusAppConfig";
        private const string appFileName = "opus-customization.sppkg";
        private const string appConfigJsonFileName = "opus_customization_app_config.json";
        private const int JPMC_BASIC_CONDITION_COUNT = 4; //record status + country code + retention type + retention period
        private const string RETENTIONTYPE_EVENT = "Event";
        private const string RETENTIONTYPE_FLAT = "Flat";
        private const string EeveryoneExceptExternalUser = "c:0-.f|rolemanager|spo-grid-all-users";
        private readonly string JobDetailsAcction_CustomizationAppSync;
        private readonly string JobDetailsAcction_UploadCustomizationApp;
        public const string RECORDSTATUS_FINAL = "Final";
        private readonly string RECORDSTATUS_WIP = "Work in Progress";
        private readonly string MODIFIED_TIME = "Modified";
        private bool hasError = false;
        #endregion

        #region Property
        private RALogger logger = RALogger.GetInstance(typeof(RMSyncCustomization4JPMC));
        private JPMCTenantConfig mConfig;
        private string tempAppFilePath;
        private Dictionary<string, RetentionSchedule> RuleAndRetentionMappings;
        #endregion

        #region Interface

        private ITermDao TermDao => PlatformWindsorManager.GetService<ITermDao>();
        private IRMKeyValueDao KeyValueDao => PlatformWindsorManager.GetService<IRMKeyValueDao>();
        private ISettingProfilesDao SettingProfileDao => PlatformWindsorManager.GetService<ISettingProfilesDao>();
        private ITermRuleAssociationDao TermRuleAssociationDao => PlatformWindsorManager.GetService<ITermRuleAssociationDao>();
        private IRuleManagerService RuleManagerService => PlatformWindsorManager.GetService<IRuleManagerService>();
        private IRMReportManager mReportManger;
        public IRMReportManager ReportManager
        {
            get
            {
                if (mReportManger == null)
                {
                    mReportManger = ReportMangerFactory.Instance.ReportManager;
                }
                return mReportManger;
            }
        }
        #endregion

        public RMSyncCustomization4JPMC(JPMCTenantConfig config)
        {
            mConfig = config;
            if (!string.IsNullOrEmpty(mConfig?.ConfigSite?.AdminUrl))
            {
                mConfig.ConfigSite.AdminUrl = mConfig.ConfigSite.AdminUrl.TrimEnd('/');
            }
            else
            {
                logger.Warn($"admin URL error, admin URL is {mConfig?.ConfigSite?.AdminUrl}");
            }
            JobDetailsAcction_CustomizationAppSync = "RM_TM_Action_CustomizationAppSync";
            JobDetailsAcction_UploadCustomizationApp = "RM_TM_Action_UploadCustomizationApp";
            RuleAndRetentionMappings = new();
        }

        public bool JPMCCustomizationSync()
        {
            logger.Info($"[Customization4JPMC]Start: {mConfig.ConfigSiteUrl}");
            JPMCAppConfig appConfig = AssemblyAppConfiguration();
            try
            {
                CommonClientContext commonClientContext = new();
                var context = commonClientContext.InitClientContext(mConfig.ConfigSite);
                List configList = EnsureConfigList(context);
                var appVersion = UploadAppFile(context, configList);
                appConfig.AppVersion = appVersion;
                string appJonsConfigTemp = WriteConfigToTempJsonFile(appConfig);
                UploadAppCofig(context, configList, appJonsConfigTemp);
                context.ExecuteQuery();
                return hasError;
            }
            catch (Exception e)
            {
                logger.Error($"JPMCCustomizationSync error {e}");
                hasError = true;
                return hasError;
            }
        }

        private JPMCAppConfig AssemblyAppConfiguration()
        {
            var advanceTerms = TermDao.GetAllTermHasAdvanceSettingsTerms();
            advanceTerms.ForEach(c =>
            {
                try
                {
                    c.AdvanceSettingsObject = JsonConvert.DeserializeObject<TermAdvanceSettings>(c.AdvanceSettings);
                }
                catch (Exception e)
                {
                    logger.Warn($"Deserialize advance settigs error, term name: {c.Name}, advance settings: {c.AdvanceSettings}, error: {e}");
                }
            });
            var isUseRecordLabel = KeyValueDao.GetValueByKey("JPMC_UseLockedRecordLabel")?.Value;
            var recordLabel = (bool.TryParse(isUseRecordLabel, out var useLabel) && useLabel) ? SettingProfileDao.LoadByTypeAsync((int)SettingProfilesType.RecordsLabelSetting).GetAwaiter().GetResult()?.Settings : "";
            JPMCAppConfig appConfig = new()
            {
                RecordRetentionLabel = recordLabel,
                SiteTypePropertyName = mConfig.SiteTypePropertyName,
                CustomColumns = new CustomColumns()
                {
                    ClassCode = mConfig.CustomColumns.ClassCode,
                    CountryCode = mConfig.CustomColumns.CountryCode,
                    EndDate = mConfig.CustomColumns.EndDate,
                    RecordStatus = mConfig.CustomColumns.RecordStatus,
                    RetentionType = mConfig.CustomColumns.RetentionType,
                    StartDate = mConfig.CustomColumns.StartDate
                },
                ClassCodeConfigs = []
            };

            var termIds = advanceTerms.Select(c => c.Id).ToList();
            var termRuleMappig = TermRuleAssociationDao.GetTermRuleInfoByTermIds(termIds);
            var allRules = RuleManagerService.GetRulesFromRecords().ToDictionary(d => d.Id);
            foreach (var term in advanceTerms)
            {
                try
                {
                    logger.Info($"[Customization4JPMC]Assembly term: {term.Name}");
                    var classCodeConfig = new ClassCodeConfig()
                    {
                        SiteType = term.AdvanceSettingsObject?.SiteType,
                        ClassCode = new ClassCode() { TermId = term.UniqueId, TermLabel = term.Name, Description = term.Description },
                        RetentionSchedules = [],
                    };
                    string canNotConvertRule = null;
                    var rules4CurrentTerm = termRuleMappig.Where(mp => mp.TermId == term.Id).ToList();
                    List<ClassCodeConfig4CheckDuplicate> classCodeConfig4CheckDuplicates = [];
                    foreach (var termRule in rules4CurrentTerm)
                    {
                        if (allRules.TryGetValue(termRule.RuleId.ToString(), out Rule rule))
                        {
                            RetentionSchedule retentionSchedule = AssemblyRetentioBySingleRule(rule);
                            if (retentionSchedule != null)
                            {
                                classCodeConfig.RetentionSchedules.Add(retentionSchedule);

                                foreach (var countryCode in retentionSchedule.CountryCodes)
                                {
                                    classCodeConfig4CheckDuplicates.Add(new ClassCodeConfig4CheckDuplicate()
                                    {
                                        CountryCode = countryCode,
                                        RecordStatus = retentionSchedule.RecordStatus,
                                        RetentionScheduleType = retentionSchedule.RetentionType,
                                        RuleName = rule.Name,
                                    });
                                }
                            }
                            else
                            {
                                canNotConvertRule = rule.Name;
                                break;
                            }
                        }
                    }

                    if (canNotConvertRule != null)
                    {
                        logger.Warn($"This term mapped not jpmc configuration rule, so skip this term:{term.Name}");
                        ReportManager.SendJobDetail(new JMTermSyncJobDetails()
                        {
                            Term = term.Name,
                            Action = JobDetailsAcction_CustomizationAppSync,
                            MMSApplication = mConfig?.ConfigSite?.AdminUrl,
                            Status = JobDetailsStatus.Failed,
                            Comment = $"RM_TM_Action_CustomizationAppSync_TermConvertError{I18NEntity.Separator}{canNotConvertRule}"
                        });
                        hasError = true;
                        continue;
                    }
                    var checkDuplicateResult = classCodeConfig4CheckDuplicates.GroupBy(c => c).Where(d => d.Count() > 1).ToDictionary(g => g.Key, g => g);
                    if (checkDuplicateResult.Count > 0)
                    {
                        logger.Info($"The two rules contain the same data, rule names: {string.Join(", ", checkDuplicateResult.Values.SelectMany(v => v.Select(c => c.RuleName)))}");
                        ReportManager.SendJobDetail(new JMTermSyncJobDetails()
                        {
                            Term = term.Name,
                            Action = JobDetailsAcction_CustomizationAppSync,
                            MMSApplication = mConfig?.ConfigSite?.AdminUrl,
                            Status = JobDetailsStatus.Failed,
                            Comment = $"RM_TM_Action_CustomizationAppSync_DuplicateRetentionSchedule4Term{I18NEntity.Separator}{string.Join(", ", checkDuplicateResult.Values.SelectMany(v => v.Select(c => c.RuleName)))}"
                        });
                        hasError = true;
                        continue;
                    }
                    if (classCodeConfig.RetentionSchedules.Count > 0)
                    {
                        ReportManager.SendJobDetail(new JMTermSyncJobDetails()
                        {
                            Term = term.Name,
                            Action = JobDetailsAcction_CustomizationAppSync,
                            MMSApplication = mConfig?.ConfigSite?.AdminUrl,
                            Status = JobDetailsStatus.Successful,
                        });
                        appConfig.ClassCodeConfigs.Add(classCodeConfig);
                    }
                }
                catch (Exception e)
                {
                    logger.Warn($"assembly term error, term name:{term.Name}, error:{e}");
                }
            }
            if (appConfig?.ClassCodeConfigs?.Count == 0)
            {
                ReportManager.SendJobDetail(new JMTermSyncJobDetails()
                {
                    Term = "RM_JS_Common_Pending",
                    Action = JobDetailsAcction_CustomizationAppSync,
                    MMSApplication = mConfig?.ConfigSite?.AdminUrl,
                    Status = JobDetailsStatus.Failed,
                    Comment = "RM_TM_Action_CustomizationAppSync_NoConfigExtracted"
                });
                hasError = true;
            }
            return appConfig;
        }

        private RetentionSchedule AssemblyRetentioBySingleRule(Rule rule)
        {
            logger.Info($"[Customization4JPMC]Assemble rule: {rule.Name}");
            RetentionSchedule result = null;
            if (rule.SOFilters.Count != JPMC_BASIC_CONDITION_COUNT)
            {
                logger.Warn($"Skip JPMC condition rule, condition count is: {rule.SOFilters.Count}, rule name:{rule.Name}");
                //break;
                return result;
            }

            if (rule.SOFilters.Any(f => !f.IsAnd))
            {
                logger.Warn($"Skip JPMC condition rule, exit OR combine, rule name:{rule.Name}");
                return result;
                //break;
            }
            RetentionSchedule retentionSchedule = new();

            List<SOFilterPolicy> GetApplyLevelCondition()
            {
                List<PolicyLevel> levels = [PolicyLevel.Folder, PolicyLevel.Document, PolicyLevel.Item];
                var applyLevelConditions = rule.SOFilters.Where(f => levels.Contains(f.Level)).ToList();
                logger.Info($"Folder/document/item level conditions count is {applyLevelConditions.Count}, rule name:{rule.Name}");
                return applyLevelConditions;
            }

            bool ProcessRecordStatus(out string recordStatus)
            {
                recordStatus = string.Empty;
                //rule level
                var applyLevelConditions = GetApplyLevelCondition();
                if (applyLevelConditions.Count == 0)
                {
                    logger.Info($"Skip JPMC condition rule, folder/document/item level conditions count is {applyLevelConditions.Count}, rule name:{rule.Name}");
                    return false;
                }

                //rule condition
                var equalsConditions = applyLevelConditions.Where(f => f.Condition == PolicyCondition.Equals).ToList();
                if (equalsConditions.Count == 0)
                {
                    logger.Warn($"Skip JPMC condition rule, do not exist any equals contidtion, rule name:{rule.Name}");
                    return false;
                }

                //rule property name
                var recordStatusCondition = equalsConditions.FirstOrDefault(r => string.Equals(r?.Rule?.Value1, $"[{mConfig?.CustomColumns?.RecordStatus}]", StringComparison.OrdinalIgnoreCase));

                if (recordStatusCondition == null)
                {
                    logger.Warn($"Skip JPMC condition rule, record status condition not exist, rule name:{rule.Name}");
                    return false;
                }

                //rule type
                if (recordStatusCondition?.Rule is not ColumnTextRule)
                {
                    logger.Warn($"Skip JPMC condition rule, record status condition rule type error, rule name:{rule.Name}");
                    return false;
                }

                recordStatus = recordStatusCondition?.Value?.Value1;
                if (!string.IsNullOrEmpty(recordStatus))
                {
                    if (RECORDSTATUS_FINAL.EqualIgnoreCase(recordStatus))
                    {
                        recordStatus = RECORDSTATUS_FINAL;
                        return true;
                    }
                    else if (RECORDSTATUS_WIP.EqualIgnoreCase(recordStatus))
                    {
                        //RECO-35517 During generation, we only check the data and still use the imported state value.
                        //recordStatus = RECORDSTATUS_WIP;
                        return true;
                    }
                    else
                    {
                        logger.Warn($"Skip JPMC condition rule, record status condition value error, rule name:{rule.Name}, condition value:{recordStatusCondition.Value.Value1}");
                        ReportManager.SendJobDetail(new JMTermSyncJobDetails()
                        {
                            Term = "RM_JS_Common_Pending",
                            Action = JobDetailsAcction_CustomizationAppSync,
                            MMSApplication = mConfig?.ConfigSite?.AdminUrl,
                            Status = JobDetailsStatus.Failed,
                            Comment = $"RM_TM_Action_CustomizationAppSync_TermConvertAdvanceSettingError4RecordStatus{I18NEntity.Separator}{recordStatus}"
                        });
                    }
                }
                return false;
            }

            bool ProcessCountryCodes(out List<string> codes)
            {
                codes = [];
                //rule level
                var applyLevelConditions = GetApplyLevelCondition();
                if (applyLevelConditions.Count == 0)
                {
                    logger.Info($"Skip JPMC condition rule, folder/document/item level conditions count is {applyLevelConditions.Count}, rule name:{rule.Name}");
                    return false;
                }

                //rule condition
                var listInConditions = applyLevelConditions.Where(f => f.Condition == PolicyCondition.ListIn).ToList();
                logger.Info($"List in conditions count is {listInConditions.Count}, rule name:{rule.Name}");
                if (listInConditions.Count != 1)//TODO Cyrus: The country codes condition currently only supports a single
                {
                    logger.Info($"Skip JPMC condition rule, List in conditions count is {listInConditions.Count}, rule name:{rule.Name}");
                    return false;
                }

                //rule type
                if (listInConditions.Any(f => f.Rule is not ColumnTextRule))
                {
                    logger.Warn($"Skip JPMC condition rule, country codes condition rule type error, rule name:{rule.Name}");
                    return false;
                }

                //rule property name
                var countryCodeConditions = listInConditions.Where(f => string.Equals(f?.Rule?.Value1, $"[{mConfig?.CustomColumns?.CountryCode}]", StringComparison.OrdinalIgnoreCase)).ToList();
                if (countryCodeConditions.Count == 0)
                {
                    logger.Warn($"Skip JPMC condition rule, country codes condition not exist, rule name:{rule.Name}");
                    return false;
                }
                codes = countryCodeConditions.SelectMany(f => f.Value.Value1.Split(";", StringSplitOptions.RemoveEmptyEntries)).ToList();
                return true;
            }

            bool ProcessRetentionType(out string retentionType)
            {
                retentionType = string.Empty;
                //rule level
                var applyLevelConditions = GetApplyLevelCondition();
                if (applyLevelConditions.Count == 0)
                {
                    logger.Info($"Skip JPMC condition rule, folder/document/item level conditions count is {applyLevelConditions.Count}, rule name:{rule.Name}");
                    return false;
                }

                //rule condition
                var equalsConditions = applyLevelConditions.Where(f => f.Condition == PolicyCondition.Equals).ToList();
                if (equalsConditions.Count == 0)
                {
                    logger.Warn($"Skip JPMC condition rule, do not exist any equals contidtion, rule name:{rule.Name}");
                    return false;
                }

                //rule property name
                var retentionTypeCondition = equalsConditions.FirstOrDefault(r => string.Equals(r?.Rule?.Value1, $"[{mConfig?.CustomColumns?.RetentionType}]", StringComparison.OrdinalIgnoreCase));

                if (retentionTypeCondition == null)
                {
                    logger.Warn($"Skip JPMC condition rule, retention type condition not exist, rule name:{rule.Name}");
                    return false;
                }

                //rule type
                if (retentionTypeCondition?.Rule is not ColumnTextRule)
                {
                    logger.Warn($"Skip JPMC condition rule, retention type condition rule type error, rule name:{rule.Name}");
                    return false;
                }

                if (!string.IsNullOrEmpty(retentionTypeCondition?.Value?.Value1))
                {
                    List<string> retentionTypes = [RETENTIONTYPE_EVENT, RETENTIONTYPE_FLAT];
                    var tempRetentionType = retentionTypes.FirstOrDefault(type => type.Equals(retentionTypeCondition.Value.Value1, StringComparison.OrdinalIgnoreCase));
                    if (!string.IsNullOrEmpty(tempRetentionType))
                    {
                        retentionType = tempRetentionType;
                        return true;
                    }
                    else
                    {
                        logger.Warn($"Skip JPMC condition rule, retention type condition value error, rule name:{rule.Name}, condition value:{retentionTypeCondition.Value.Value1}");
                    }
                }
                return false;
            }

            bool ProcessRetentionPeriod(out (int, string) retentionPeriod)
            {
                retentionPeriod = new(0, "");
                var applyLevelConditions = GetApplyLevelCondition();
                if (applyLevelConditions.Count == 0)
                {
                    logger.Info($"Skip JPMC condition rule, folder/document/item level conditions count is {applyLevelConditions.Count}, rule name:{rule.Name}");
                    return false;
                }

                var olderthanConditions = applyLevelConditions.Where(f => f.Condition == PolicyCondition.OlderThan).ToList();
                if (olderthanConditions.Count > 1)
                {
                    logger.Warn($"Skip JPMC condition rule, more than one retention period condition, rule name:{rule.Name}");
                    return false;
                }
                var retentionPeriodCondition = olderthanConditions.FirstOrDefault();
                if (retentionPeriodCondition == null)
                {
                    logger.Warn($"Skip JPMC condition rule, retention period condition not exist, rule name:{rule.Name}");
                    return false;
                }

                //Event + Start Date(date and time column)
                if (RETENTIONTYPE_EVENT.Equals(retentionSchedule.RetentionType, StringComparison.OrdinalIgnoreCase))
                {
                    if (retentionPeriodCondition.Rule is not ColumnDateTimeRule)
                    {
                        logger.Warn($"Skip JPMC condition rule, retention period condition rule type error, rule name:{rule.Name}");
                        return false;
                    }

                    //rule property name
                    if (!string.Equals(retentionPeriodCondition?.Rule?.Value1, $"[{mConfig?.CustomColumns?.StartDate}]", StringComparison.OrdinalIgnoreCase))
                    {
                        logger.Warn($"Skip JPMC condition rule, retention period condition filter name error, rule name:{rule.Name}");
                        return false;
                    }
                }
                //Flat + Create time
                if (RETENTIONTYPE_FLAT.Equals(retentionSchedule.RetentionType, StringComparison.OrdinalIgnoreCase))
                {
                    if (retentionPeriodCondition.Rule is not ModifiedRule)
                    {
                        logger.Warn($"Skip JPMC condition rule, retention period condition type error, rule name:{rule.Name}");
                        return false;
                    }
                }

                retentionPeriod = new(int.Parse(retentionPeriodCondition.Value.Value1),
                    retentionPeriodCondition.Value.Value1Unit switch
                    {
                        PolicyValueUnit.Years => "Y",
                        PolicyValueUnit.Months => "M",
                        PolicyValueUnit.Weeks => "W",
                        PolicyValueUnit.Days => "D",
                        _ => throw new ArgumentOutOfRangeException(nameof(retentionPeriodCondition.Value.Value1Unit), $"Unexpected unit {retentionPeriodCondition.Value.Value1Unit}")
                    });

                return true;
            }

            if (ProcessRecordStatus(out string recordStatus))
            {
                retentionSchedule.RecordStatus = recordStatus;
            }
            else
            {
                return result;
            }

            if (ProcessCountryCodes(out List<string> countryCodes))
            {
                retentionSchedule.CountryCodes = countryCodes;
            }
            else
            {
                return result;
            }

            if (ProcessRetentionType(out string retentionType))
            {
                retentionSchedule.RetentionType = retentionType;
            }
            else
            {
                return result;
            }

            if (ProcessRetentionPeriod(out (int, string) retentionPeriod))
            {
                retentionSchedule.RetentionPeriod = new();
                (int periodValue, string periodUnit) = retentionPeriod;
                retentionSchedule.RetentionPeriod.Value = periodValue;
                retentionSchedule.RetentionPeriod.Unit = periodUnit;
            }
            else
            {
                return result;
            }
            result = retentionSchedule;
            return result;
        }

        private static string WriteConfigToTempJsonFile(JPMCAppConfig appConfig)
        {
            var appConfigJson = JsonConvert.SerializeObject(appConfig);
            var appJonsConfigTemp = Path.Combine(WebUtil.GetInstallPath(), "Temp", appConfigJsonFileName);
            CreateDirectoryIfNotExist(Path.GetDirectoryName(appJonsConfigTemp));
            using (var stream = System.IO.File.Create(appJonsConfigTemp))
            {
                using (var writer = new StreamWriter(stream))
                {
                    string formattedJsonText = JToken.Parse(appConfigJson).ToString(Formatting.Indented);
                    writer.Write(formattedJsonText);
                }
            }

            return appJonsConfigTemp;
        }

        private List EnsureConfigList(ClientContext context)
        {
            List configList;
            if (context.Web.ListExists(configListName))
            {
                logger.Info("Config list exist.");
                configList = context.Web.GetListByUrl(configListName);
            }
            else
            {
                configList = context.Web.CreateList(ListTemplateType.DocumentLibrary, configListName, enableVersioning: true);
                logger.Info("Successfully created config list.");
            }

            return configList;
        }

        private string UploadAppFile(ClientContext clientContext, List configList)
        {
            var (packagedTime, appVersion) = RebuildAppConfig();
            var needUploadApp = true;
            var appFile = configList.RootFolder.GetFile(appFileName);
            if (appFile != null)
            {
                logger.Info("Successfully find app file.");
                clientContext.Load(appFile.ListItemAllFields, item => item.Properties);
                clientContext.ExecuteQuery();
                if (appFile.ListItemAllFields.Properties.FieldValues.TryGetValue("PackagedTime", out object propertyPackagedTime) && propertyPackagedTime?.ToString() == packagedTime)
                {
                    logger.Info($"The app packaged time on the site is the latest and does not require re uploading. PackagedTime:{propertyPackagedTime}");
                    needUploadApp = false;
                }
                else
                {
                    logger.Info($"PackagedTime is null");
                }
            }
            else
            {
                logger.Info($"App file not exist.");
            }

            try
            {
                if (needUploadApp)
                {
                    var uploadAppFile = configList.RootFolder.UploadFile(appFileName, tempAppFilePath, overwriteIfExists: true);
                    logger.Info("Successfully uploaded file.");
                    uploadAppFile.ListItemAllFields.Properties["PackagedTime"] = packagedTime;
                    uploadAppFile.ListItemAllFields.Update();
                    clientContext.ExecuteQuery();
                    ReportManager.SendJobDetail(new JMTermSyncJobDetails()
                    {
                        Term = "RM_JS_Common_Pending",
                        Action = JobDetailsAcction_UploadCustomizationApp,
                        MMSApplication = mConfig?.ConfigSite?.AdminUrl,
                        Status = JobDetailsStatus.Successful,
                        Comment = "RM_TM_Action_UploadCustomizationApp_Success"
                    });
                    logger.Info($"Successfully updated file properties. packaged time:{packagedTime}");
                }
                else
                {
                    ReportManager.SendJobDetail(new JMTermSyncJobDetails()
                    {
                        Term = "RM_JS_Common_Pending",
                        Action = JobDetailsAcction_UploadCustomizationApp,
                        MMSApplication = mConfig?.ConfigSite?.AdminUrl,
                        Status = JobDetailsStatus.Skipped,
                        Comment = "RM_TM_Action_UploadCustomizationApp_Skip"
                    });
                }
            }
            catch (Exception e)
            {
                logger.Warn($"Upload app error {e}");
            }
            finally
            {
                try
                {
                    System.IO.File.Delete(tempAppFilePath);
                }
                catch (Exception e)
                {
                    logger.Warn($"Delete temp app file error {e}");
                }
            }

            return appVersion;
        }

        private void UploadAppCofig(ClientContext context, List configList, string appJonsConfigTemp)
        {
            var configFile = configList.RootFolder.UploadFile(appConfigJsonFileName, appJonsConfigTemp, overwriteIfExists: true);
            var configItem = configFile.ListItemAllFields;

            var users = context.Web.SiteUsers;
            context.Load(users, r => r.Include(a => a.LoginName));
            context.ExecuteQuery();

            RoleDefinitionBindingCollection roleDefinition = new(context)
                {
                    context.Web.RoleDefinitions.GetByType(RoleType.Reader)
                };
            configItem.BreakRoleInheritance(false, true);
            var everyone = users.FirstOrDefault(u => u.LoginName.StartsWith(EeveryoneExceptExternalUser, StringComparison.OrdinalIgnoreCase));
            configItem.RoleAssignments.Add(everyone, roleDefinition);
            configItem.Update();
        }

        private (string, string) RebuildAppConfig()
        {
            var appFolderPath = Path.Combine("Config", "AppPackages", "JPMC");
            var appFilePath = Path.Combine(WebUtil.GetInstallPath(), appFolderPath, appFileName);
            //build base temp folder
            var tempBaseFolder = Path.Combine(WebUtil.GetInstallPath(), "Temp", appFolderPath);
            CreateDirectory(tempBaseFolder);

            //unzip app
            var unZipFolder = Path.Combine(tempBaseFolder, "opus-customization");
            CreateDirectoryIfNotExist(unZipFolder);
            ZipUtil.UnZipFile(appFilePath, unZipFolder);
            logger.Info($"Succcessfully unzip folder {appFilePath} to {unZipFolder}");

            string packagedTime = null;
            var clientSideAssetsFiles = Directory.GetFiles(Path.Combine(unZipFolder, "ClientSideAssets"), "*", SearchOption.AllDirectories);
            clientSideAssetsFiles = clientSideAssetsFiles.Where(filePath => filePath.Contains("deletion-date-calculation-field-customizer") || filePath.Contains("opus-actions-command-set")).ToArray();
            foreach (var clientSideAssetsFilePath in clientSideAssetsFiles)
            {
                if (Path.GetExtension(clientSideAssetsFilePath) != ".js")
                {
                    continue;
                }
                var fileContentt = System.IO.File.ReadAllText(clientSideAssetsFilePath);

                void ReplaceAppConfig(string repalceKey, string replaceValue)
                {
                    Regex extractConfigSiteUrlRegex = new($"{repalceKey}\\s?:\\s?\\\".*?\\\"", RegexOptions.None, TimeSpan.FromMinutes(3));
                    var configSiteUrlSettings = extractConfigSiteUrlRegex.Match(fileContentt);
                    logger.Info($"Find {repalceKey} value: {configSiteUrlSettings.Value}");
                    fileContentt = fileContentt.Replace(configSiteUrlSettings.Value, $"{repalceKey}:\"{replaceValue}\"");
                }
                ReplaceAppConfig("configSiteUrl", mConfig.ConfigSiteUrl);
                ReplaceAppConfig("csdApiUrl", RMGlobalConfiguration.AppConfig[RMAppSettingKey.OPUS_CSD_API_URL]);
                ReplaceAppConfig("aosLoginAppId", RMGlobalConfiguration.AppConfig[RMAppSettingKey.AOS_LOGIN_APP_ID]);

                Regex extractPackagedTimeRegex = new("packagedTime:\\\"(?<packagedTime>\\S*)\\\"", RegexOptions.None, TimeSpan.FromMinutes(3));
                var match = extractPackagedTimeRegex.Match(fileContentt);
                System.IO.File.WriteAllText(clientSideAssetsFilePath, fileContentt);
                packagedTime ??= match.Groups.GetValueOrDefault("packagedTime")?.Value;
            }

            var manifestFilePath = Path.Combine(unZipFolder, "AppManifest.xml");
            var manifestFileContent = System.IO.File.ReadAllText(manifestFilePath);
            manifestFileContent = manifestFileContent.Replace("ResourceId=\"c4763714-72c1-4746-a68e-a17bcf7ad292\"", $"ResourceId=\"{RMGlobalConfiguration.AppConfig[RMAppSettingKey.AOS_LOGIN_APP_ID]}\"");
            System.IO.File.WriteAllText(manifestFilePath, manifestFileContent);

            string appVersion = string.Empty;
            System.Xml.XmlDocument doc = new();
            doc.LoadXml(manifestFileContent);

            System.Xml.XmlNode appNode = doc.DocumentElement;
            if (appNode != null)
            {
                appVersion = appNode.Attributes["Version"].Value;
            }
            tempAppFilePath = Path.Combine(tempBaseFolder, appFileName);
            if (System.IO.File.Exists(tempAppFilePath))
            {
                System.IO.File.Delete(tempAppFilePath);
            }
            ZipUtil.ZipFolder(unZipFolder, tempAppFilePath);
            logger.Info($"Succcessfully zip folder {tempAppFilePath} to {unZipFolder}");

            Directory.Delete(unZipFolder, recursive: true);
            logger.Info($"Succcessfully clear folder {unZipFolder}");
            return (packagedTime, appVersion);
        }

        private static void CreateDirectoryIfNotExist(string filePath)
        {
            if (!Directory.Exists(filePath))
            {
                Directory.CreateDirectory(filePath);
            }
        }

        private static void CreateDirectory(string filePath)
        {
            if (Directory.Exists(filePath))
            {
                Directory.Delete(filePath, true);
            }
            CreateDirectoryIfNotExist(filePath);
        }
    }

}
