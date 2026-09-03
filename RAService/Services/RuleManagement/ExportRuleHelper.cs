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
using AvePoint.RA.Contract.RMRuleManageMent;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AvePoint.GCommon.Contract.CommonFilter;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.I18N.Core;
using AvePoint.GCommon.Contract.StorageOptimization.Object;
using AvePoint.GCommon.Utility;
using AvePoint.RA.Common.SystemSetting;
using AvePoint.RA.Common.Util;
using AvePoint.RA.Service.Services.StorageDevice;
using AvePoint.RA.Common;
using AvePoint.RA.Contract.RMWeb.StorageDevice;
using AvePoint.RA.Contract.RMWeb.Setting;
using AvePoint.RA.Services.Settings;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Service.Services.RMReport;
using AvePoint.RA.Contract.Tenant;
using RAArchiverCommon.Utility;

namespace AvePoint.RA.Service.RuleManagement
{
    public class ExportRuleHelper
    {
        private RALogger logger = RALogger.GetInstance(typeof(ExportRuleHelper));
        private static IStorageDeviceService StorageDeviceService => PlatformWindsorManager.GetService<IStorageDeviceService>();

        private static IStubSettingService StubSettingService => PlatformWindsorManager.GetService<IStubSettingService>();

        protected static ITenantService TenantService => PlatformWindsorManager.GetService<ITenantService>();

        public static readonly string Action_KeepDataActionString = "Declare or tag content";
        public static readonly string Action_NewKeepDataActionString = "Tag or lock content";
        public static readonly string Action_RemoveDataActionString = "Remove content";
        public static readonly string Action_MoveDataActionString = "Move content";
        public static readonly string Action_ArchiveDataActionString = "Archive content";
        public static readonly string Action_ExportOnly = "Export content";
        public static readonly string Action_CalculateDisposalDate = "Calculate action due date";
        public static readonly string TrueString = "TRUE";
        public static readonly string FalseString = "FALSE";
        protected bool IsSupportRecordLabel = false;
        public ExportRuleHelper() { }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="outerRule">包含Rule的基本信息和SPORule的信息</param>
        /// <param name="innerRule"></param>
        /// <param name="ruleSourceTitle"></param>
        public ExportRuleHelper(RMRuleInfos outerRule, RMRuleInfos innerRule, string ruleSourceTitle)
        {
            OuterRule = outerRule;
            CurrentRule = innerRule;
            RuleSourceLabel = ruleSourceTitle;
            IsSupportRecordLabel = AccountUtility.IsSupportRecordLabel();

        }
        public RMRuleInfos OuterRule { get; set; }
        public RMRuleInfos CurrentRule { get; set; }
        public string RuleSourceLabel { get; set; }
        public List<string[]> StrRules { get; set; } = new List<string[]>();
        public List<string> StrRule { get; set; }

        public Dictionary<string, string> StorageIdNameMapping = new();

        public Dictionary<string, string> StubIdNameMapping = new();
        public virtual List<string[]> ConvertRuleInfoToArray()
        {
            for (int i = 0; i < CurrentRule.RuleFilters.Count; i++)
            {
                RuleFilter filter = this.CurrentRule.RuleFilters[i];
                StrRule = new List<string>();
                ConvertRuleInfoToArray(filter, i == CurrentRule.RuleFilters.Count - 1);
                if (StrRule.Count > 0)
                {
                    StrRules.Add(StrRule.ToArray());
                }
            }
            return StrRules;
        }
        public virtual void ConvertRuleInfoToArray(RuleFilter filter, bool isLast)
        {
            AppendRuleBaseInfo();
            AppendRuleFilter(filter, isLast);
            AppendArchiverActions();
            AppendTagContent();
            AppendMoveInfo();
            AppendManualInfo();
            AppendExportInfo();
            AppendArchiveStorage();
            AppendExportToDestinationLibrary();
            AppendExportLocation();
            AppendDeleteToRecycleBin();
        }
        public virtual void AppendArchiverActions()
        {
            var r = this.CurrentRule;
            var archiverRuleAction = GetArchiverRuleAction(r);
            StrRule.Add(archiverRuleAction);
            if (archiverRuleAction.Equals(Action_ExportOnly))
            {
                StrRule.Add(r.ExportInfo.exportType.ToString());
            }
            else
            {
                StrRule.Add("");
            }
            StrRule.Add((int)r.RelatedRecordOption == (int)AvePoint.RA.Contract.RMRuleManageMent.RelatedRecordOption.Both ? TrueString : FalseString);
            StrRule.Add(r.DeleteRecords ? TrueString : FalseString);
            if (IsSupportRecordLabel)
            {
                StrRule.Add(r.IncludeDeleteRecordLabel ? TrueString : FalseString);
            }
            StrRule.Add((r.RuleKeepDataOption & 128) == (int)KeepDataStatus.LinkToDocument || (r.RuleKeepDataOption & 2048) == (int)KeepDataStatus.ArchiveAndLeaveStub ? TrueString : FalseString);
            if((r.RuleKeepDataOption & 128) == (int)KeepDataStatus.LinkToDocument || (r.RuleKeepDataOption & 2048) == (int)KeepDataStatus.ArchiveAndLeaveStub)
            {
                try
                {
                    if (!StubIdNameMapping.ContainsKey(r.StubTemplateId))
                    {
                        var stubTemplate = StubSettingService.GetStubTemplateByIdAsync(r.StubTemplateId).GetAwaiter().GetResult();
                        StubIdNameMapping.Add(r.StubTemplateId, stubTemplate.Name);
                    }

                    StrRule.Add(StubIdNameMapping[r.StubTemplateId]);
                }
                catch
                {
                    StrRule.Add("");
                }
            }
            else
            {
                StrRule.Add("");
            }
            var archiverContentBeforeDisposal = FalseString;
            if (archiverRuleAction == Action_RemoveDataActionString)
            {
                archiverContentBeforeDisposal = (r.RuleKeepDataOption & 256) != (int)KeepDataStatus.NotBackup ? TrueString : FalseString;
            }
            StrRule.Add(archiverContentBeforeDisposal);
            FillNullCellValue(StrRule, 1);

            StrRule.Add((r.RuleKeepDataOption & (int)KeepDataStatus.IsEnableRemoveRetentionLabel) == (int)KeepDataStatus.IsEnableRemoveRetentionLabel ? TrueString : FalseString);

            StrRule.Add((r.RuleKeepDataOption & (int)KeepDataStatus.DeclareRecord) == (int)KeepDataStatus.DeclareRecord ? TrueString : FalseString);
            StrRule.Add((r.RuleKeepDataOption & (int)KeepDataStatus.TagContent) == (int)KeepDataStatus.TagContent ? TrueString : FalseString); // Tag Document/Item
        }
        public virtual void AppendTagContent(bool withRetentionLable = true)
        {
            var r = this.CurrentRule;
            List<RMTagContentInfo> tagContentInfos = r.TagContentInfo;
            if (tagContentInfos != null && tagContentInfos.Count > 0)
            {
                var tagWithArchiverd = tagContentInfos.Where(t => t.Type == TagContentInfoType.Archived).FirstOrDefault();
                StrRule.Add(tagWithArchiverd != null ? TrueString : FalseString);

                var tagWithArchiverdBy = tagContentInfos.Where(t => t.Type == TagContentInfoType.ArchivedBy).FirstOrDefault();
                StrRule.Add(tagWithArchiverdBy != null ? TrueString : FalseString);

                var tagWithArchiverdTime = tagContentInfos.Where(t => t.Type == TagContentInfoType.ArchivedDate).FirstOrDefault();
                StrRule.Add(tagWithArchiverdTime != null ? TrueString : FalseString);

                var tagWithCustom = tagContentInfos.Where(t => t.Type != TagContentInfoType.Archived && t.Type != TagContentInfoType.ArchivedBy && t.Type != TagContentInfoType.ArchivedDate && t.Type != TagContentInfoType.RetentionLabel).FirstOrDefault();
                bool isTagWithCustom = tagWithCustom != null;
                StrRule.Add(isTagWithCustom ? TrueString : FalseString);

                StrRule.Add(isTagWithCustom ? GetTagType(tagWithCustom.Type) : "");
                StrRule.Add(isTagWithCustom ? tagWithCustom.ColumnName : "");
                StrRule.Add(isTagWithCustom && tagWithCustom.ColumnName != "" ? tagWithCustom.Value : "");
                StrRule.Add(isTagWithCustom && tagWithCustom.Type == TagContentInfoType.DateTime ? GeneralSettingConfig.GetTimeZoneInforById(tagWithCustom.TimeZoneId).DisplayName : "");

                if (withRetentionLable)
                {
                    var tagRetentionLabel = tagContentInfos.Where(t => t.Type == TagContentInfoType.RetentionLabel).FirstOrDefault();
                    if (IsSupportRecordLabel)
                    {
                        if(tagRetentionLabel != null)
                        {
                            StrRule.Add(TrueString);
                            StrRule.Add(tagRetentionLabel.Option == (int)RetentionLabelOptions.Default ? tagRetentionLabel.Value : "");
                            StrRule.Add(tagRetentionLabel.Option == (int)RetentionLabelOptions.GetFromGeneralSetting ? TrueString : "");
                        }
                        else
                        {
                            StrRule.Add(FalseString);
                            FillNullCellValue(StrRule, 2);
                        }
                    }
                    else
                    {
                        StrRule.Add(tagRetentionLabel != null ? tagRetentionLabel.Value : "");
                    }
                }
                else {
                    FillNullCellValue(StrRule, IsSupportRecordLabel ? 3 : 1);
                }
            }
            else
            {
                FillNullCellValue(StrRule, IsSupportRecordLabel ? 11 : 9);
            }
        }
        public virtual void AppendMoveInfo()
        {
            var r = this.CurrentRule;
            if (r.MoveDto != null && !r.EnableExport)
            {
                StrRule.Add(r.MoveDto.LocationPath);
                StrRule.Add(GetConflictOptionStr(r.MoveDto.FileNameConflictOption));
                StrRule.Add(!r.MoveDto.NotDeclareMovedData ? TrueString : FalseString);
                FillNullCellValue(StrRule, 2);
            }
            else
            {
                FillNullCellValue(StrRule, 5);
            }
        }
        public void AppendRuleBaseInfo()
        {
            StrRule.Add(OuterRule.RuleName);
            StrRule.Add(OuterRule.Description);
            StrRule.Add(OuterRule.ContainerName);
            StrRule.Add(GetRuleLevelStr(OuterRule.RuleLevel));
            StrRule.Add(OuterRule.DisposalClass);
            StrRule.Add(RuleSourceLabel);
        }
        public void AppendRuleFilter(RuleFilter filter, bool isLast)
        {
            if (isLast)
            {
                StrRule.Add("");
            }
            else
            {
                StrRule.Add(I18NEntity.GetString(filter.CombineMode.Equals(ArchiverFilterCombineMode.And) ? "RM_JS_Rule_ConditionAnd" : "RM_JS_Rule_ConditionOr"));
            }

            if (ReportUtil.KeyValues.Values.Contains((int)filter.RuleType))
            {
                foreach (var keyValue in ReportUtil.KeyValues)
                {
                    if (keyValue.Value.Equals((int)filter.RuleType))
                    {
                        StrRule.Add(keyValue.Key);
                        break;
                    }
                }
            }
            else
            {
                StrRule.Add(filter.RuleType.ToString());
            }

            if (!string.IsNullOrEmpty(filter.filterName) && IsCustomPropertyCriteria(filter.RuleType))
            {
                if (filter.RuleType == ArchiverFilterRuleType.TextLabelProperty || filter.RuleType == ArchiverFilterRuleType.NumberLabelProperty || filter.RuleType == ArchiverFilterRuleType.DateTimeLabelProperty)
                {
                    StrRule.Add($"{filter.filterName}:{filter.Value1}");
                }
                else
                {
                    StrRule.Add(filter.filterName);
                }
                //Criteria Name
            }
            else
            {             
                StrRule.Add("");
            }
            if(filter.Condition == ArchiverFilterCondition.IsEmpty)
            {
                StrRule.Add("IsBlank");
            }
            else
            {
                StrRule.Add(filter.Condition == (ArchiverFilterCondition)262936 ? "Equals" : filter.Condition.ToString());
            }
            
            if (filter.RuleType == ArchiverFilterRuleType.ModifiedTime || filter.RuleType == ArchiverFilterRuleType.CreatedTime ||
           filter.RuleType == ArchiverFilterRuleType.DateTimeColumn || filter.RuleType == ArchiverFilterRuleType.LastAccessedTime || filter.RuleType == ArchiverFilterRuleType.LastActiveTime
           || filter.RuleType == ArchiverFilterRuleType.DateTimeCustomProperty || filter.RuleType == ArchiverFilterRuleType.SendDateUTC || filter.RuleType == ArchiverFilterRuleType.DateTimeLabelProperty
           || filter.RuleType == ArchiverFilterRuleType.ParentLibraryDateTime || filter.RuleType == ArchiverFilterRuleType.ParentSiteCollectionDateTime || filter.RuleType == ArchiverFilterRuleType.PropertyBagDateTime)
            {
                if (filter.Condition == ArchiverFilterCondition.OlderThan)
                {
                    if (filter.RuleType == ArchiverFilterRuleType.DateTimeLabelProperty)
                    {
                        StrRule.Add(filter.Value2);
                        StrRule.Add(filter.Value2Unit.ToString());
                    }
                    else
                    {
                        StrRule.Add(filter.Value1);
                        StrRule.Add(filter.Value1Unit.ToString());
                    }
                    FillNullCellValue(StrRule, 2);
                }
                else if (filter.Condition == ArchiverFilterCondition.FromTo)
                {
                    FillNullCellValue(StrRule, 2);
                    StrRule.Add(filter.StartTimeInfo.StartTime);
                    StrRule.Add(filter.EndTimeInfo.StartTime);
                }
                else
                {
                    FillNullCellValue(StrRule, 2);
                    StrRule.Add(filter.StartTimeInfo.StartTime);
                    StrRule.Add("");              
                }
            }
            else
            {
                if ((filter.RuleType == ArchiverFilterRuleType.BooleanColumn || filter.RuleType == ArchiverFilterRuleType.BooleanCustomProperty
                    || filter.RuleType == ArchiverFilterRuleType.ParentLibraryBoolean || filter.RuleType == ArchiverFilterRuleType.ParentSiteCollectionBoolean
                    || filter.RuleType == ArchiverFilterRuleType.PropertyBagBoolean || filter.RuleType == ArchiverFilterRuleType.OrphanedFolderRule) &&
                    (string.Equals("Yes".ToLower(), filter.Value1.ToLower(), StringComparison.OrdinalIgnoreCase) ||
                    string.Equals("No".ToLower(), filter.Value1.ToLower(), StringComparison.OrdinalIgnoreCase)))
                {
                    bool yes = string.Equals("Yes".ToLower(), filter.Value1.ToLower(), StringComparison.OrdinalIgnoreCase);
                    string value1 = yes ? I18NEntity.GetString("RM_JS_Common_Yes") : I18NEntity.GetString("RM_JS_Common_No");
                    StrRule.Add(!string.IsNullOrEmpty(filter.Value1) ? value1 : "");
                }
                else if (filter.RuleType == ArchiverFilterRuleType.TextLabelProperty || filter.RuleType == ArchiverFilterRuleType.NumberLabelProperty)
                {
                    StrRule.Add(!string.IsNullOrEmpty(filter.Value2) ? filter.Value2 : "");
                }
                else
                {
                    StrRule.Add(!string.IsNullOrEmpty(filter.Value1) ? filter.Value1 : "");
                }
                StrRule.Add(filter.Value1Unit != PolicyValueUnit.None ? filter.Value1Unit.ToString() : "");
                FillNullCellValue(StrRule, 2);
            }
        }
        public void AppendManualInfo()
        {
            var r = this.CurrentRule;
            StrRule.Add(r.EnableManualApproval ? TrueString : FalseString); 
            StrRule.Add(r.IsSendEmailToOwner ? TrueString : FalseString);
            if (r.EnableManualApproval)
            {
                if (r.ManualReviewType == ReviewType.Workflow)
                {
                    StrRule.Add("Manual approval process");
                    StrRule.Add(r.WorkflowName);
                    StrRule.Add("");
                }
                else if (r.ManualReviewType == ReviewType.RecordOwner)
                {
                    StrRule.Add("Record owner");
                    StrRule.Add("");
                    if (r.Users != null && r.Users.Count > 0)
                    {
                        List<string> userNames = r.Users.Select(u => u.UserPrincipalName).ToList();
                        StrRule.Add(String.Join(";", userNames));
                    }
                    else
                    {
                        StrRule.Add("");
                    }
                }
            }
            else
            {
                StrRule.Add("");
                StrRule.Add("");
                StrRule.Add("");
            }
        }
        public void AppendExportInfo()
        {
            var r = this.CurrentRule;
            var archiverRuleAction = GetArchiverRuleAction(r);
            if (!archiverRuleAction.Equals(Action_ExportOnly))
            {
                StrRule.Add(r.EnableExport ? TrueString : FalseString);
                if (r.EnableExport)
                {
                    StrRule.Add(r.ExportInfo.exportType.ToString());
                }
                else
                {
                    StrRule.Add("");
                }
            }
            else
            {
                FillNullCellValue(StrRule, 2);
            }
        }

        public virtual void AppendArchiveStorage()
        {
            if (!string.IsNullOrWhiteSpace(this.CurrentRule.StoragePolicyId) && !this.CurrentRule.StoragePolicyId.Equals(Guid.Empty.ToString(), StringComparison.OrdinalIgnoreCase))
            {
                if (!StorageIdNameMapping.ContainsKey(this.CurrentRule.StoragePolicyId))
                {
                    var storageDevice = StorageDeviceService.GetStorageDeviceById(this.CurrentRule.StoragePolicyId);
                    if(storageDevice == null)
                    {
                        logger.Info($"Current rule has no storage ,rule name is {this.CurrentRule.RuleName}, storage policy id is {this.CurrentRule.StoragePolicyId}");
                        StrRule.Add("");
                        return;
                    }
                    StorageIdNameMapping.Add(this.CurrentRule.StoragePolicyId, storageDevice.Name);
                }

                StrRule.Add(StorageIdNameMapping[this.CurrentRule.StoragePolicyId]);
            }
            else
            {
                StrRule.Add("");
            }
        }
        public virtual void AppendExportToDestinationLibrary()
        {
            if (CurrentRule.EnableExport && CurrentRule.MoveDto != null &&
                CurrentRule.MoveDto.LocationPath.IsNotNullOrEmpty())
            {
                StrRule.Add(CurrentRule.MoveDto.LocationPath);
            }
            else
            {
                StrRule.Add("");
            }
        }
        private void AppendExportLocation()
        {
            var rule = this.CurrentRule;
            string storageId = rule.ExportInfo?.exportLocationId;
            if (storageId.IsNullOrEmpty())
            {
                StrRule.Add("");
            }
            else
            {
                if (!StorageIdNameMapping.ContainsKey(storageId))
                {
                    var storageDevice = StorageDeviceService.GetStorageDeviceById(storageId);

                    if (storageDevice == null)
                    {
                        StrRule.Add("");
                        return;
                    }

                    StorageIdNameMapping[storageId] = storageDevice.Name;
                }

                StrRule.Add(StorageIdNameMapping[storageId]);
            }
        }
        private void AppendDeleteToRecycleBin()
        {
            StrRule.Add(CurrentRule.DeleteToRecycleBin ? TrueString : FalseString);
            
            // for site collection level
            StrRule.Add(CurrentRule.IsDeleteSiteCollectionToRecycleBin() ? TrueString : FalseString); 
            StrRule.Add(CurrentRule.LockRecordBeforeDestroy ? TrueString : FalseString);
        }
        protected void FillNullCellValue(List<string> sourceArrary, int repeat)
        {
            if (sourceArrary != null)
            {
                for (int i = 0; i < repeat; i++)
                {
                    sourceArrary.Add("");
                }
            }
        }
        protected string GetConflictOptionStr(FileNameConflictOption conflictOption)
        {
            switch (conflictOption)
            {
                case FileNameConflictOption.Skip:
                    return I18NEntity.GetString("RM_TM_Excel_Skip");
                case FileNameConflictOption.Overwrite:
                    return I18NEntity.GetString("RM_TM_Excel_Overwrite");
                case FileNameConflictOption.Rename:
                    return I18NEntity.GetString("RM_TM_Excel_AddSuffix");
                default:
                    return "";
            }
        }
        protected string GetArchiverRuleAction(RMRuleInfos rule)
        {
            string strArchiverActions = "";
            int keepDataOption = rule.RuleKeepDataOption;
            if (rule.IsCalculationDisposalDate)
            {
                strArchiverActions = Action_CalculateDisposalDate;
                return strArchiverActions;
            }
            if (rule.EnableExport == true && rule.ExportInfo != null && rule.ExportInfo.exportSPDataOption == ExportSPDataOption.ExportWithoutArchive)
            {
                strArchiverActions = Action_ExportOnly;
                return strArchiverActions;
            }
            if (rule.RuleLevel == PolicyLevel.ExchangeOnlineItem)
            {
                if ((keepDataOption & (int)KeepDataStatus.Keep) == (int)KeepDataStatus.Keep)
                {
                    strArchiverActions = IsSupportRecordLabel ? Action_NewKeepDataActionString : Action_KeepDataActionString;
                }
                else if (keepDataOption == (int)KeepDataStatus.Delete && rule.MoveDto != null && !rule.EnableExport)
                {
                    strArchiverActions = Action_MoveDataActionString;
                }
                else
                {
                    strArchiverActions = Action_RemoveDataActionString;
                }
            }
            else
            {
                keepDataOption = ExcludeAffectCheckOption(keepDataOption);
                if (keepDataOption != (int)KeepDataStatus.Delete
                    && keepDataOption != (int)KeepDataStatus.Remove
                    && (keepDataOption & 128) != (int)KeepDataStatus.LinkToDocument
                    && (keepDataOption & 256) != (int)KeepDataStatus.NotBackup
                    && keepDataOption != (int)KeepDataStatus.Vault
                    && keepDataOption != (int)KeepDataStatus.Archive
                    && keepDataOption != (int)KeepDataStatus.ArchiveAndLeaveStub
                    && (keepDataOption & (int)KeepDataStatus.TriggerMicrosoft365Archiving) != (int)KeepDataStatus.TriggerMicrosoft365Archiving)
                {
                    strArchiverActions = IsSupportRecordLabel ? Action_NewKeepDataActionString : Action_KeepDataActionString;
                }
                else if (keepDataOption == (int)KeepDataStatus.Delete && rule.MoveDto != null && !rule.EnableExport)
                {
                    strArchiverActions = Action_MoveDataActionString;
                }
                else if (keepDataOption == (int)KeepDataStatus.Archive || keepDataOption == (int)KeepDataStatus.ArchiveAndLeaveStub)
                {
                    strArchiverActions = Action_ArchiveDataActionString;
                }
                else if ((keepDataOption & (int)KeepDataStatus.TriggerMicrosoft365Archiving) == (int)KeepDataStatus.TriggerMicrosoft365Archiving)
                {
                    strArchiverActions = I18NEntity.GetString("RM_JS_RDM_CreateRule_Options_StoreInM365Archive");
                }
                else
                {
                    strArchiverActions = Action_RemoveDataActionString;
                }
            }
            return strArchiverActions;
        }

        private int ExcludeAffectCheckOption(int keepDataOption)
        {
            if((keepDataOption & (int)KeepDataOption.IsEnableRemoveRetentionLabel) == (int)KeepDataOption.IsEnableRemoveRetentionLabel)
            {
                keepDataOption -= (int)KeepDataOption.IsEnableRemoveRetentionLabel;
            }
            return keepDataOption;
        }

        protected string GetTagType(TagContentInfoType tagContentType)
        {
            string tagStr = "";
            switch (tagContentType)
            {
                case TagContentInfoType.Text:
                    tagStr = "RM_JS_RDM_CreateRule_TagType_Text";
                    break;
                case TagContentInfoType.Number:
                    tagStr = "RM_JS_RDM_CreateRule_TagType_Nubmer";
                    break;
                case TagContentInfoType.DateTime:
                    tagStr = "RM_JS_RDM_CreateRule_TagType_DateTime";
                    break;
                case TagContentInfoType.Boolean:
                    tagStr = "RM_JS_RDM_CreateRule_TagType_YesNo";
                    break;
            }
            return I18NEntity.GetString(tagStr);
        }
        protected string GetRuleLevelStr(PolicyLevel level)
        {
            string ruleLevelKey = "";
            switch (level)
            {
                case PolicyLevel.Document:
                    ruleLevelKey = "RM_JS_Rule_ObjectLevel_Document";
                    break;
                case PolicyLevel.Item:
                    ruleLevelKey = "RM_JS_Rule_ObjectLevel_Item";
                    break;
                case PolicyLevel.Folder:
                    ruleLevelKey = "RM_JS_Rule_ObjectLevel_Folder";
                    break;
                case PolicyLevel.List:
                case PolicyLevel.Library:
                    ruleLevelKey = "RM_JS_Rule_ObjectLevel_List";
                    break;
                case PolicyLevel.Site:
                    ruleLevelKey = "RM_JS_Rule_ObjectLevel_Site";
                    break;
                case PolicyLevel.SiteCollection:
                    ruleLevelKey = "RM_JS_Rule_ObjectLevel_SiteCollection";
                    break;
                default:
                    ruleLevelKey = level.ToString();
                    break;
            }
            return I18NEntity.GetString(ruleLevelKey);
        }
        protected bool IsCustomPropertyCriteria(ArchiverFilterRuleType filterRuleType)
        {
            switch (filterRuleType)
            {
                case ArchiverFilterRuleType.TextColumn:
                case ArchiverFilterRuleType.BooleanColumn:
                case ArchiverFilterRuleType.NumberColumn:
                case ArchiverFilterRuleType.DateTimeColumn:
                case ArchiverFilterRuleType.TextCustomProperty:
                case ArchiverFilterRuleType.NumberCustomProperty:
                case ArchiverFilterRuleType.BooleanCustomProperty:
                case ArchiverFilterRuleType.DateTimeCustomProperty:
                case ArchiverFilterRuleType.MetadataTextColumn:
                case ArchiverFilterRuleType.MetadataNumberColumn:
                case ArchiverFilterRuleType.NumberLabelProperty:
                case ArchiverFilterRuleType.TextLabelProperty:
                case ArchiverFilterRuleType.DateTimeLabelProperty:
                case ArchiverFilterRuleType.ParentLibraryText:
                case ArchiverFilterRuleType.ParentLibraryNumber:
                case ArchiverFilterRuleType.ParentLibraryBoolean:
                case ArchiverFilterRuleType.ParentLibraryDateTime:
                case ArchiverFilterRuleType.ParentSiteCollectionText:
                case ArchiverFilterRuleType.ParentSiteCollectionNumber:
                case ArchiverFilterRuleType.ParentSiteCollectionBoolean:
                case ArchiverFilterRuleType.ParentSiteCollectionDateTime:
                case ArchiverFilterRuleType.PropertyBagText:
                case ArchiverFilterRuleType.PropertyBagNumber:
                case ArchiverFilterRuleType.PropertyBagBoolean:
                case ArchiverFilterRuleType.PropertyBagDateTime:
                    return true;
                default:
                    return false;
            }
        }
    }

    public class ExportSPRuleHelper : ExportRuleHelper
    {
        public ExportSPRuleHelper(RMRuleInfos outerRule, RMRuleInfos innerRule, string ruleSourceTitle) : base(outerRule, innerRule, ruleSourceTitle)
        { 
            
        }

        public override void AppendMoveInfo()
        {
            var r = this.CurrentRule;
            if (r.MoveDto != null && !r.EnableExport)
            {
                StrRule.Add(r.MoveDto.LocationPath);
                StrRule.Add(GetConflictOptionStr(r.MoveDto.FileNameConflictOption));
                StrRule.Add(!r.MoveDto.NotDeclareMovedData ? TrueString : FalseString);
                FillNullCellValue(StrRule, 1);
                StrRule.Add(r.MoveDto.isKeepClassification ? TrueString : FalseString);
            }
            else
            {
                FillNullCellValue(StrRule, 5);
            }
        }
    }

    public class ExportTeamsRuleHelper : ExportRuleHelper
    {
        public ExportTeamsRuleHelper(RMRuleInfos outerRule, RMRuleInfos innerRule, string ruleSourceTitle) : base(outerRule, innerRule, ruleSourceTitle)
        {

        }

        public override void AppendMoveInfo()
        {
            var r = this.CurrentRule;
            if (r.MoveDto != null && !r.EnableExport)
            {
                StrRule.Add(r.MoveDto.LocationPath);
                StrRule.Add(GetConflictOptionStr(r.MoveDto.FileNameConflictOption));
                StrRule.Add(!r.MoveDto.NotDeclareMovedData ? TrueString : FalseString);
                FillNullCellValue(StrRule, 1);
                StrRule.Add(r.MoveDto.isKeepClassification ? TrueString : FalseString);
            }
            else
            {
                FillNullCellValue(StrRule, 5);
            }
        }
    }

    public class ExportEXORuleHelper : ExportRuleHelper
    {
        public ExportEXORuleHelper(RMRuleInfos outerRule, RMRuleInfos innerRule, string ruleSourceTitle) : base(outerRule, innerRule, ruleSourceTitle)
        {

        }
        public override void AppendArchiverActions()
        {
            var r = this.CurrentRule;
            var archiverRuleAction = GetArchiverRuleAction(r);
            StrRule.Add(archiverRuleAction);
            if (archiverRuleAction.Equals(Action_ExportOnly))
            {
                StrRule.Add(r.ExportInfo.exportType.ToString());
            }
            else
            {
                StrRule.Add("");
            }
            FillNullCellValue(StrRule, IsSupportRecordLabel ? 9 : 8);
        }
        public override void AppendTagContent(bool withRetentionLable = true)
        {
            var r = this.CurrentRule;
            List<RMTagContentInfo> tagContentInfos = r.TagContentInfo;
            if (tagContentInfos != null && tagContentInfos.Count > 0)
            {
                StrRule.Add(TrueString);
                FillNullCellValue(StrRule, 8);
                var tagRetentionLabel = tagContentInfos.Where(t => t.Type == TagContentInfoType.RetentionLabel).FirstOrDefault();
                if (IsSupportRecordLabel)
                {
                    if (tagRetentionLabel != null)
                    {
                        StrRule.Add(TrueString);
                        StrRule.Add(tagRetentionLabel.Value);
                        FillNullCellValue(StrRule, 1);
                    }
                    else
                    {
                        StrRule.Add(FalseString);
                        FillNullCellValue(StrRule, 2);
                    }
                }
                else
                {
                    StrRule.Add(tagRetentionLabel != null ? tagRetentionLabel.Value : "");
                }
            }
            else
            {
                FillNullCellValue(StrRule, IsSupportRecordLabel ? 12 : 10);
            }
        }
        public override void AppendMoveInfo()
        {
            var r = this.CurrentRule;
            if (r.MoveDto != null && !r.EnableExport)
            {
                StrRule.Add(r.MoveDto.LocationPath);
                StrRule.Add(GetConflictOptionStr(r.MoveDto.FileNameConflictOption));
                FillNullCellValue(StrRule, 1);
                StrRule.Add(r.MoveDto.IsDeleteSourceItem ? TrueString : FalseString);
                StrRule.Add(r.MoveDto.isKeepClassification ? TrueString : FalseString);
            }
            else
            {
                FillNullCellValue(StrRule, 5);
            }
        }
    }

    public class ExportFSRuleHelper : ExportRuleHelper
    {
        public ExportFSRuleHelper(RMRuleInfos outerRule, RMRuleInfos innerRule, string ruleSourceTitle) : base(outerRule, innerRule, ruleSourceTitle)
        {

        }
        public override void AppendArchiverActions()
        {
            var r = this.CurrentRule;
            var archiverRuleAction = GetArchiverRuleAction(r);
            StrRule.Add(archiverRuleAction);
            FillNullCellValue(StrRule, IsSupportRecordLabel ? 4 : 3);
            StrRule.Add((r.RuleKeepDataOption & 128) == (int)KeepDataStatus.LinkToDocument ? TrueString : FalseString);
            FillNullCellValue(StrRule, 7);
        }
        public override void AppendTagContent(bool withRetentionLable = true)
        {
            FillNullCellValue(StrRule, IsSupportRecordLabel ? 10 : 8);
        }
        public override void AppendMoveInfo()
        {
            var r = this.CurrentRule;
            if (r.MoveDto != null)
            {
                StrRule.Add(r.MoveDto.LocationPath);
                StrRule.Add(GetConflictOptionStr(r.MoveDto.FileNameConflictOption));
                FillNullCellValue(StrRule, 1);
                StrRule.Add(r.MoveDto.IsDeleteSourceItem ? TrueString : FalseString);
                StrRule.Add(r.MoveDto.isKeepClassification ? TrueString : FalseString);
            }
            else
            {
                FillNullCellValue(StrRule, 5);
            }
        }
    }

    public class ExportBoxRuleHelper : ExportRuleHelper
    {
        public ExportBoxRuleHelper(RMRuleInfos outerRule, RMRuleInfos innerRule, string ruleSourceTitle) : base(outerRule, innerRule, ruleSourceTitle)
        {

        }
        public override void AppendArchiverActions()
        {
            var r = this.CurrentRule;
            var archiverRuleAction = GetArchiverRuleAction(r);
            StrRule.Add(archiverRuleAction);
            FillNullCellValue(StrRule, IsSupportRecordLabel ? 12 : 11);
        }
        public override void AppendTagContent(bool withRetentionLable = true)
        {
            FillNullCellValue(StrRule, IsSupportRecordLabel ? 10 : 8);
        }
        public override void AppendMoveInfo()
        {
            var r = this.CurrentRule;
            if (r.MoveDto != null)
            {
                StrRule.Add(r.MoveDto.LocationPath);
                StrRule.Add(GetConflictOptionStr(r.MoveDto.FileNameConflictOption));
                FillNullCellValue(StrRule, 1);
                StrRule.Add(r.MoveDto.IsDeleteSourceItem ? TrueString : FalseString);
                StrRule.Add(r.MoveDto.isKeepClassification ? TrueString : FalseString);
            }
            else
            {
                FillNullCellValue(StrRule, 5);
            }
        }
    }

    public class ExportPhysicalRuleHelper : ExportRuleHelper
    {
        public ExportPhysicalRuleHelper(RMRuleInfos outerRule, RMRuleInfos innerRule, string ruleSourceTitle) : base(outerRule, innerRule, ruleSourceTitle)
        {

        }
        public override void AppendArchiverActions()
        {
            var r = this.CurrentRule;
            var archiverRuleAction = GetArchiverRuleAction(r);
            StrRule.Add(archiverRuleAction);
            StrRule.Add("");
            StrRule.Add((int)r.RelatedRecordOption == (int)AvePoint.RA.Contract.RMRuleManageMent.RelatedRecordOption.Both ? TrueString : FalseString);
            FillNullCellValue(StrRule, IsSupportRecordLabel ? 5 : 4);
            StrRule.Add(r.DestroyEmptyBoxOnFolderRule ? TrueString : FalseString);
            FillNullCellValue(StrRule, 3);
        }
        public override void AppendTagContent(bool withRetentionLable = true)
        {
            FillNullCellValue(StrRule, IsSupportRecordLabel ? 11 : 9);
        }
        public override void AppendMoveInfo()
        {
            var r = this.CurrentRule;
            if (r.MoveDto != null)
            {
                if (r.MoveDto.PhysicalTreeNode != null)
                {
                    StrRule.Add(r.MoveDto.PhysicalTreeNode.LocationName);
                }
                else
                {
                    FillNullCellValue(StrRule, 1);
                }
                if (TenantService.IsNewOpusTenant())
                {
                    StrRule.Add("");
                }
                else
                {
                    StrRule.Add(GetConflictOptionStr(r.MoveDto.FileNameConflictOption));
                }
                
                FillNullCellValue(StrRule, 1);
                FillNullCellValue(StrRule, 2);
            }
            else
            {
                FillNullCellValue(StrRule, 5);
            }
        }
    }

    public class ExportSPLocalRuleHelper : ExportRuleHelper
    {
        public ExportSPLocalRuleHelper(RMRuleInfos outerRule, RMRuleInfos innerRule, string ruleSourceTitle) : base(outerRule, innerRule, ruleSourceTitle)
        {

        }
        public override void AppendTagContent(bool withRetentionLable = true)
        {
            base.AppendTagContent(false);
        }
        public override void AppendMoveInfo()
        {
            FillNullCellValue(StrRule, 5);
        }
    }

    public class ExportOneDriveRuleHelper : ExportRuleHelper
    {
        private static IStubSettingService StubSettingService => PlatformWindsorManager.GetService<IStubSettingService>();

        public ExportOneDriveRuleHelper(RMRuleInfos outerRule, RMRuleInfos innerRule, string ruleSourceTitle) : base(outerRule, innerRule, ruleSourceTitle)
        {

        }

        public override void AppendArchiverActions()
        {
            var r = this.CurrentRule;
            var archiverRuleAction = GetArchiverRuleAction(r);
            StrRule.Add(archiverRuleAction);
            if (archiverRuleAction.Equals(Action_ExportOnly))
            {
                StrRule.Add(r.ExportInfo.exportType.ToString());
            }
            else
            {
                StrRule.Add("");
            }
            FillNullCellValue(StrRule, 1);
            StrRule.Add(r.DeleteRecords ? TrueString : FalseString);
            if (IsSupportRecordLabel)
            {
                StrRule.Add(r.IncludeDeleteRecordLabel ? TrueString : FalseString);
            }
            StrRule.Add((r.RuleKeepDataOption & 128) == (int)KeepDataStatus.LinkToDocument || (r.RuleKeepDataOption & 2048) == (int)KeepDataStatus.ArchiveAndLeaveStub ? TrueString : FalseString);
            if ((r.RuleKeepDataOption & 128) == (int)KeepDataStatus.LinkToDocument || (r.RuleKeepDataOption & 2048) == (int)KeepDataStatus.ArchiveAndLeaveStub)
            {
                try
                {
                    if (!StubIdNameMapping.ContainsKey(r.StubTemplateId))
                    {
                        var stubTemplate = StubSettingService.GetStubTemplateByIdAsync(r.StubTemplateId).GetAwaiter().GetResult();
                        StubIdNameMapping.Add(r.StubTemplateId, stubTemplate.Name);
                    }

                    StrRule.Add(StubIdNameMapping[r.StubTemplateId]);
                }
                catch
                {
                    StrRule.Add("");
                }
            }
            else
            {
                StrRule.Add("");
            }

            var archiverContentBeforeDisposal = FalseString;
            if (archiverRuleAction == Action_RemoveDataActionString)
            {
                archiverContentBeforeDisposal = (r.RuleKeepDataOption & 256) != (int)KeepDataStatus.NotBackup ? TrueString : FalseString;
            }
            StrRule.Add(archiverContentBeforeDisposal);
            FillNullCellValue(StrRule, 1);

            StrRule.Add((r.RuleKeepDataOption & (int)KeepDataStatus.IsEnableRemoveRetentionLabel) == (int)KeepDataStatus.IsEnableRemoveRetentionLabel ? TrueString : FalseString);

            StrRule.Add((r.RuleKeepDataOption & (int)KeepDataStatus.DeclareRecord) == (int)KeepDataStatus.DeclareRecord ? TrueString : FalseString);
            StrRule.Add((r.RuleKeepDataOption & (int)KeepDataStatus.TagContent) == (int)KeepDataStatus.TagContent ? TrueString : FalseString);
        }

        public override void AppendMoveInfo()
        {
            var r = this.CurrentRule;
            if (r.MoveDto != null && !r.EnableExport)
            {
                StrRule.Add(r.MoveDto.LocationPath);
                StrRule.Add(GetConflictOptionStr(r.MoveDto.FileNameConflictOption));
                StrRule.Add(!r.MoveDto.NotDeclareMovedData ? TrueString : FalseString);
                FillNullCellValue(StrRule, 1);
                StrRule.Add(r.MoveDto.isKeepClassification ? TrueString : FalseString);
            }
            else
            {
                FillNullCellValue(StrRule, 5);
            }
        }
    }

    public class ExportGoogleRuleHelper : ExportRuleHelper
    {
        private static IStorageDeviceService StorageDeviceService => PlatformWindsorManager.GetService<IStorageDeviceService>();
        public ExportGoogleRuleHelper(RMRuleInfos outerRule, RMRuleInfos innerRule, string ruleSourceTitle)
            : base(outerRule, innerRule, ruleSourceTitle)
        {
        }

        public override void AppendArchiverActions()
        {
            var r = this.CurrentRule;

            var archiverRuleAction = GetArchiverRuleAction(r);
            StrRule.Add(archiverRuleAction);

            if (archiverRuleAction.Equals(Action_ExportOnly))
            {
                StrRule.Add(r.ExportInfo?.exportType.ToString() ?? "");
            }
            else
            {
                StrRule.Add("");
            }

            FillNullCellValue(StrRule, IsSupportRecordLabel ? 9 : 8);
        }

        public override void AppendTagContent(bool withRetentionLabel = true)
        {
            var r = this.CurrentRule;
            List<RMTagContentInfo> tagContentInfos = r.TagContentInfo;
            if (tagContentInfos != null && tagContentInfos.Count > 0)
            {
                StrRule.Add(TrueString);
                FillNullCellValue(StrRule, 8);
                var tagRetentionLabel = tagContentInfos
                    .Where(t => t.Type == TagContentInfoType.RetentionLabel)
                    .FirstOrDefault();
                if (IsSupportRecordLabel)
                {
                    if(tagRetentionLabel != null)
                    {
                        StrRule.Add(TrueString);
                        StrRule.Add(tagRetentionLabel.Value);
                        FillNullCellValue(StrRule, 1);
                    }
                    else
                    {
                        FillNullCellValue(StrRule, 3);
                    }
                }
                else
                {
                    StrRule.Add(tagRetentionLabel?.Value ?? "");
                }
            }
            else
            {
                FillNullCellValue(StrRule, IsSupportRecordLabel ? 12 : 10);
            }
        }

        public override void AppendMoveInfo()
        {
            var r = this.CurrentRule;

            if (r.MoveDto != null && !r.EnableExport)
            {
                var destination = GetDestinationPathByGoogleTree(r);
                StrRule.Add(destination);
                StrRule.Add(GetConflictOptionStr(r.MoveDto.FileNameConflictOption));
                FillNullCellValue(StrRule, 3);
            }
            else
            {
                FillNullCellValue(StrRule, 5);
            }
        }

        private string GetDestinationPathByGoogleTree(RMRuleInfos ruleInfo)
        {
            var pathParts = new List<string>();
            var current = ruleInfo.MoveDto.GoogleTree;

            while (current != null && !string.IsNullOrEmpty(current.ObjectId))
            {
                pathParts.Insert(0, current.Name);
                current = current.Parent;
            }

            return string.Join("/", pathParts);
        }
    }
}
