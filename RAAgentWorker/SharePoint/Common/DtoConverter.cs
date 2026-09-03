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
using AvePoint.GCommon.Contract.Tree.Object;
using AvePoint.RA.Contract.Global.Object;
using System;
using System.Collections.Generic;
using System.Linq;
using GCObject = AvePoint.GCommon.Contract.CentralAdmin.Object;

namespace AvePoint.RA.SharePoint.Common
{
    public class DtoConverter
    {
        private static List<PolicyLevel> mSupportLevels = new List<PolicyLevel>()
        {
                PolicyLevel.FileSysFile,
                PolicyLevel.FileSysFolder,
                PolicyLevel.Document,
                PolicyLevel.Item,
                PolicyLevel.DocumentVersion,
                PolicyLevel.ItemVersion,
                PolicyLevel.Attachment,
        };
        public static SPTreeNodeDto ConvertRMTree2SPTree(RMSPTreeNode rmTree, SPTreeNodeDto sp = null)
        {
            if (sp == null)
            {
                sp = new SPTreeNodeDto();
            }
            sp.ID = rmTree.Id;
            sp.FarmID = rmTree.FarmId;
            sp.FarmName = rmTree.FarmName;
            sp.Name = rmTree.Name;
            sp.Title = rmTree.Title;
            sp.FullPath = rmTree.FullPath;
            sp.Url = rmTree.FullPath;
            sp.Level = (NodeLevel)rmTree.Level;
            sp.Type = (NodeType)rmTree.NodeType;
            sp.SPType = (SPType)rmTree.SPType;
            sp.SPObjectId = rmTree.SPObjectId;
            sp.SPVersion = rmTree.SPVersion;
            sp.Expanded = rmTree.Expanded;
            sp.ChildrenCount = rmTree.ChildrenCount;
            sp.CheckNumber = rmTree.CheckNumber;
            sp.Hidden = rmTree.Hidden;
            sp.Template = rmTree.TemplateId;
            if (sp.NodeExtension == null)
            {
                sp.NodeExtension = new NodeExtensionDto();
            }
            //sp.NodeExtension.BposInfo = rmTree.BposInfo;
            if (rmTree.Parent != null && sp.Parent == null)
            {
                SPTreeNodeDto tempParent = new SPTreeNodeDto();
                tempParent.Children = new List<SPTreeNodeDto> { sp };
                sp.Parent = ConvertRMTree2SPTree(rmTree.Parent, tempParent);
            }
            if (rmTree.Children != null && rmTree.Children.Count > 0 &&
                (sp.Children == null || sp.Children.Count == 0))
            {
                sp.Children = new List<SPTreeNodeDto>();
                foreach (RMSPTreeNode child in rmTree.Children)
                {
                    SPTreeNodeDto tempChild = new SPTreeNodeDto();
                    tempChild.Parent = sp;
                    sp.Children.Add(ConvertRMTree2SPTree(child, tempChild));
                }
            }
            return sp;
        }

        public static RMSPTreeNode ConvertSPTree2RMTree(SPTreeNodeDto spTree, RMSPTreeNode rm = null)
        {
            if (rm == null)
            {
                rm = new RMSPTreeNode();
            }
            rm.Id = spTree.ID;
            rm.FarmId = spTree.FarmID;
            rm.FarmName = spTree.FarmName;
            rm.Name = spTree.Name;
            rm.Title = spTree.Title;
            rm.FullPath = spTree.FullPath;
            rm.Level = (int)spTree.Level;
            rm.NodeType = (int)spTree.Type;
            rm.SPObjectId = spTree.SPObjectId;
            rm.SPVersion = spTree.SPVersion;
            rm.SPType = (int)spTree.SPType;
            rm.Expanded = spTree.Expanded;
            rm.Hidden = spTree.Hidden;
            if (rm.Name == "{System Folder}")
            {
                rm.Hidden = true;
            }
            rm.ChildrenCount = spTree.ChildrenCount;
            rm.CheckNumber = spTree.CheckNumber;
            rm.TemplateId = spTree.Template;
            //rm.TeamName = spTree.TeamName;
            if (spTree.NodeExtension != null && spTree.NodeExtension.BposInfo != null)
            {
                // rm.BposInfo = spTree.NodeExtension.BposInfo;
            }
            if (spTree.Parent != null && rm.Parent == null)
            {
                RMSPTreeNode tempParent = new RMSPTreeNode();
                tempParent.Children = new List<RMSPTreeNode>() { rm };
                rm.Parent = ConvertSPTree2RMTree(spTree.Parent, tempParent);
            }
            if (spTree.Children != null && spTree.Children.Count > 0 &&
                (rm.Children == null || rm.Children.Count == 0))
            {
                rm.Children = new List<RMSPTreeNode>();
                foreach (SPTreeNodeDto child in spTree.Children)
                {
                    RMSPTreeNode temp = new RMSPTreeNode();
                    temp.Parent = rm;
                    RMSPTreeNode rmChild = ConvertSPTree2RMTree(child, temp);
                    rm.Children.Add(rmChild);
                }
            }
            return rm;
        }

        public static GCObject.BposInfo ConvertRMBposInfoToBposInfo(BposInfo info)
        {
            GCObject.BposInfo gcBposinfo = new GCObject.BposInfo()
            {
                SiteUrl = info.SiteUrl,
                Mode = (AvePoint.GCommon.Contract.CentralAdmin.Object.BPOSMode)info.Mode,
                UserAccountInfo = new GCObject.BposUserAccountInfo()
                {
                    Domain = info.UserAccountInfo.Domain,
                    Password = info.UserAccountInfo.Password,
                    Username = info.UserAccountInfo.Username

                }
            };

            return gcBposinfo;
        }

        public static AvePoint.GCommon.Contract.StorageOptimization.Object.Rule ConvertGlobalRule2Rule(Rule globalRule)
        {
            AvePoint.GCommon.Contract.StorageOptimization.Object.Rule rule = new GCommon.Contract.StorageOptimization.Object.Rule();
            rule.AndOrExpression = GetAndOrExpression(globalRule.AndOrExpression);
            if (globalRule.ArchiverSetting != null)
            {
                rule.ArchiverSetting = new GCommon.Contract.StorageOptimization.Object.ArchiverSetting()
                {
                    EnableArchiverVEOMerge = globalRule.ArchiverSetting.EnableArchiverVEOMerge,
                    FileNumber = globalRule.ArchiverSetting.FileNumber,
                    FileSize = globalRule.ArchiverSetting.FileSize,
                    FolderName = globalRule.ArchiverSetting.FolderName,
                    IsDeleteOldFile = globalRule.ArchiverSetting.IsDeleteOldFile,
                    NumberOfThreadSendingEmail = globalRule.ArchiverSetting.NumberOfThreadSendingEmail,
                    StorageId = globalRule.ArchiverSetting.StoragePolicyId,
                };
            }
            if (globalRule.ArchiverVEOSetting != null)
            {
                rule.ArchiverVEOSetting = new GCommon.Contract.StorageOptimization.Object.ArchiverVEOSetting()
                {
                    AgencyId = globalRule.ArchiverVEOSetting.AgencyId,
                    ConsignmentNumber = globalRule.ArchiverVEOSetting.ConsignmentNumber,
                    SeriesIdentifier = globalRule.ArchiverVEOSetting.SeriesIdentifier,
                    SeriesNumber = globalRule.ArchiverVEOSetting.SeriesNumber
                };
            }
            rule.Compression = globalRule.Compression;
            rule.DataSecurity = globalRule.DataSecurity;
            rule.DeleteRecords = globalRule.DeleteRecords;
            rule.DeclareLinkFile = globalRule.DeclareLinkFile;
            rule.DisposalClass = globalRule.DisposalClass;
            rule.Encryption = globalRule.Encryption;
            rule.EncryptionInfoId = globalRule.EncryptionInfoId;
            rule.EncryptionInfoName = globalRule.EncryptionInfoName;
            rule.StoragePolicyId = globalRule.StoragePolicyId;
            rule.ModifyTime = globalRule.ModifyTime;
            if (globalRule.ExportInfo != null)
            {
                rule.ExportInfo = new GCommon.Contract.StorageOptimization.Object.SOExportInfo()
                {
                    exportLocationId = globalRule.ExportInfo.exportLocationId,
                    exportLocationName = globalRule.ExportInfo.exportLocationName,
                    exportSPDataOption = (AvePoint.GCommon.Contract.StorageOptimization.Object.ExportSPDataOption)globalRule.ExportInfo.exportSPDataOption,
                    exportType = (AvePoint.GCommon.Contract.StorageOptimization.Object.ExportTypeValue)globalRule.ExportInfo.exportType
                };
            }
            rule.ExportType = (AvePoint.GCommon.Contract.StorageOptimization.Object.ExportTypeValue)globalRule.ExportType;
            rule.FileVEO = globalRule.FileVEO;
            if (globalRule.Filters != null)
            {
                var supportFilters = globalRule.Filters.Where(f => mSupportLevels.Contains(GetPolicyLevel(f.Level))).ToList();
                if (supportFilters != null && supportFilters.Count > 0)
                {
                    rule.Filters = supportFilters.ConvertAll(f => ConvertGlobalDto2FilterPolicy(f));
                }
                else
                {
                    rule.Filters = new List<GCommon.Contract.CommonFilter.FilterPolicy>();
                }
            }
            if (globalRule.FSRule != null)
            {
                rule.FSRule = ConvertGlobalRule2Rule(globalRule.FSRule);
            };
            if (globalRule.SPLocalRule != null)
            {
                rule.SPLocalRule = ConvertGlobalRule2Rule(globalRule.SPLocalRule);
            };
            rule.Id = globalRule.Id;
            rule.IsManualApproval = globalRule.IsManualApproval;
            rule.IsSendEamilToOwner = globalRule.IsSendEamilToOwner;
            rule.KeepDataOption = globalRule.KeepDataOption;
            rule.ManifestVEO = globalRule.ManifestVEO;
            if (globalRule.MoveToRecordCenterAndDelareSetting != null && globalRule.MoveToRecordCenterAndDelareSetting.DestinationLocation != null)
            {
                rule.MoveToRecordCenterAndDelareSetting = new AvePoint.GCommon.Contract.StorageOptimization.Object.MoveToRecordCenterAndDelareSetting()
                {
                    ContentConflictResolution = (AvePoint.GCommon.Contract.StorageOptimization.Object.ContentConflictResolution)globalRule.MoveToRecordCenterAndDelareSetting.ContentConflictResolution,
                    DelaredRecord = globalRule.MoveToRecordCenterAndDelareSetting.DelaredRecord,
                    DestinationLocation = new AvePoint.GCommon.Contract.StorageOptimization.Object.DestinationLocationInfo()
                    {
                        Password = globalRule.MoveToRecordCenterAndDelareSetting.DestinationLocation.Password,
                        ServiceAccountName = globalRule.MoveToRecordCenterAndDelareSetting.DestinationLocation.ServiceAccountName,
                        Url = globalRule.MoveToRecordCenterAndDelareSetting.DestinationLocation.Url,
                        UserName = globalRule.MoveToRecordCenterAndDelareSetting.DestinationLocation.UserName
                    },
                    IsMoveVersions = globalRule.MoveToRecordCenterAndDelareSetting.IsMoveVersions,
                    KeepFolderStructure = globalRule.MoveToRecordCenterAndDelareSetting.KeepFolderStructure,
                    LeaveLinkInSource = globalRule.MoveToRecordCenterAndDelareSetting.LeaveLinkInSource,
                    OperateDataMode = (AvePoint.GCommon.Contract.StorageOptimization.Object.OperatingSharePointDataMode)globalRule.MoveToRecordCenterAndDelareSetting.OperateDataMode,
                    OriginalMetaDataAsXML = globalRule.MoveToRecordCenterAndDelareSetting.OriginalMetaDataAsXML
                };
            };
            rule.NAAConfigFile = globalRule.NAAConfigFile;
            rule.Name = globalRule.Name;
            rule.NARAConfigFile = globalRule.NARAConfigFile;
            rule.Order = globalRule.Order;
            rule.OrderList = globalRule.OrderList;
            rule.PolicyLevel = GetPolicyLevel(globalRule.PolicyLevel);
            rule.RecordVEO = globalRule.RecordVEO;
            rule.RelatedRecordOption = (AvePoint.GCommon.Contract.StorageOptimization.Object.RelatedRecordOption)globalRule.RelatedRecordOption;
            rule.ReviewType = (AvePoint.GCommon.Contract.StorageOptimization.Object.ReviewType)globalRule.ReviewType;
            if (globalRule.SOFilters != null)
            {
                //rule.SOFilters = globalRule.SOFilters.ConvertAll(f => ConvertGlobalDto2SOFilterPolicy(f));
                var supportFilters = globalRule.SOFilters.Where(f => mSupportLevels.Contains(GetPolicyLevel(f.Level))).ToList();
                if (supportFilters != null && supportFilters.Count > 0)
                {
                    rule.SOFilters = supportFilters.ConvertAll(f => ConvertGlobalDto2SOFilterPolicy(f));
                }
                else
                {
                    rule.SOFilters = new List<GCommon.Contract.StorageOptimization.Object.SOFilterPolicy>();
                }
            }
            if (globalRule.spMoveOption != null && globalRule.spMoveOption.MoveDestination != null)
            {
                rule.spMoveOption = new AvePoint.GCommon.Contract.StorageOptimization.Object.MoveOption()
                {
                    DestFlag = (AvePoint.GCommon.Contract.StorageOptimization.Object.RecordFlag)globalRule.spMoveOption.DestFlag,
                    MoveDestination = new AvePoint.GCommon.Contract.StorageOptimization.Object.MoveDestination()
                    {
                        ContainerId = globalRule.spMoveOption.MoveDestination.ContainerId,
                        DeleteSourceItem = globalRule.spMoveOption.MoveDestination.DeleteSourceItem,
                        DestMode = (AvePoint.GCommon.Contract.StorageOptimization.Object.DestMode)globalRule.spMoveOption.MoveDestination.DestMode,
                        FSAccountProfileId = globalRule.spMoveOption.MoveDestination.FSAccountProfileId,
                        FSConectionPath = globalRule.spMoveOption.MoveDestination.FSConectionPath,
                        FSPath = globalRule.spMoveOption.MoveDestination.FSPath,
                        FSTreeStr = globalRule.spMoveOption.MoveDestination.FSTreeStr,
                        FSTreeNode = globalRule.spMoveOption.MoveDestination.FSTreeNode == null ? null : RAFileSystem.FileSystem.Common.DtoConverter.ConvertGlobalDto2FSTreeNodeDto(globalRule.spMoveOption.MoveDestination.FSTreeNode),
                        KeepSourceClassification = globalRule.spMoveOption.MoveDestination.KeepSourceClassification,
                        NotDeclareMovedData = globalRule.spMoveOption.MoveDestination.NotDeclareMovedData,
                        SPAccountProfileId = globalRule.spMoveOption.MoveDestination.SPAccountProfileId,
                        SPTreeStr = globalRule.spMoveOption.MoveDestination.SPTreeStr,
                        SPUrl = globalRule.spMoveOption.MoveDestination.SPUrl
                    },
                    MoveSetting = globalRule.spMoveOption.MoveSetting != null ? new AvePoint.GCommon.Contract.StorageOptimization.Object.MoveRecordSetting()
                    {
                        ConflictType = (AvePoint.GCommon.Contract.StorageOptimization.Object.ConflictType)globalRule.spMoveOption.MoveSetting.ConflictType,
                        ContainerLevelConflictOption = (AvePoint.GCommon.Contract.StorageOptimization.Object.ConflictOption)globalRule.spMoveOption.MoveSetting.ContainerLevelConflictOption,
                        FileInherit = globalRule.spMoveOption.MoveSetting.FileInherit,
                        FolderInherit = globalRule.spMoveOption.MoveSetting.FolderInherit,
                        FolderUnderInherit = globalRule.spMoveOption.MoveSetting.FolderUnderInherit,
                        ItemLevelConflictOption = (AvePoint.GCommon.Contract.StorageOptimization.Object.ConflictOption)globalRule.spMoveOption.MoveSetting.ItemLevelConflictOption,
                        PhysicalHoldConflictOption = (AvePoint.GCommon.Contract.StorageOptimization.Object.PhysicalHoldConflictOption)globalRule.spMoveOption.MoveSetting.PhysicalHoldConflictOption,
                        FilePropertiesMapping = globalRule.spMoveOption.MoveSetting.FilePropertiesMapping != null && globalRule.spMoveOption.MoveSetting.FilePropertiesMapping.PropertiesMappingItems != null ? new AvePoint.GCommon.Contract.StorageOptimization.Object.FilePropertiesMapping()
                        {
                            PropertiesMappingItems = globalRule.spMoveOption.MoveSetting.FilePropertiesMapping.PropertiesMappingItems.ConvertAll(p => ConvertGlobalDto2PropertiesMappingItem(p))
                        } : null
                    } : null,
                    SourceFlag = (AvePoint.GCommon.Contract.StorageOptimization.Object.RecordFlag)globalRule.spMoveOption.SourceFlag
                };
            }
            if (globalRule.TagContentInfo != null && globalRule.TagContentInfo.Count > 0)
            {
                rule.TagContentInfo = globalRule.TagContentInfo.ConvertAll(t => ConvertGlobalDto2TagContentInfo(t));
            }
            rule.Type = (AvePoint.GCommon.Contract.StorageOptimization.Object.RuleType)globalRule.Type;
            if (globalRule.UserInfos != null && globalRule.UserInfos.Count > 0)
            {
                rule.UserInfos = globalRule.UserInfos.ConvertAll(u => ConvertGlobalDto2UserInfo(u));
            }
            rule.WorkflowId = globalRule.WorkflowId;
            return rule;
        }

        private static Dictionary<PolicyLevel, string> GetAndOrExpression(Dictionary<int, string> expressions)
        {
            Dictionary<PolicyLevel, string> resultExpressions = new Dictionary<PolicyLevel, string>();
            foreach (var exp in expressions)
            {
                var level = GetPolicyLevel(exp.Key);
                if (!resultExpressions.ContainsKey(level) && mSupportLevels.Contains(level))
                {
                    resultExpressions.Add(level, exp.Value);
                }
            }
            return resultExpressions;
        }

        public static AvePoint.GCommon.Contract.StorageOptimization.Object.TagContentInfo ConvertGlobalDto2TagContentInfo(TagContentInfo tag)
        {
            AvePoint.GCommon.Contract.StorageOptimization.Object.TagContentInfo tagContentInfo = new AvePoint.GCommon.Contract.StorageOptimization.Object.TagContentInfo()
            {
                ColumnName = tag.ColumnName,
                DateTime = tag.DateTime,
                Type = (AvePoint.GCommon.Contract.StorageOptimization.Object.TagContentInfoType)tag.Type,
                Value = tag.Value
            };
            return tagContentInfo;
        }
        public static AvePoint.GCommon.Contract.StorageOptimization.Object.UserInfo ConvertGlobalDto2UserInfo(UserInfo user)
        {
            AvePoint.GCommon.Contract.StorageOptimization.Object.UserInfo userInfo = new AvePoint.GCommon.Contract.StorageOptimization.Object.UserInfo()
            {
                DisplayName = user.DisplayName,
                Email = user.Email,
                InviteType = (AvePoint.GCommon.Contract.Server.Login.InviteType)user.InviteType,
                UserId = user.UserId,
                UserPrincipalName = user.UserPrincipalName
            };
            return userInfo;
        }

        public static AvePoint.GCommon.Contract.StorageOptimization.Object.PropertiesMappingItem ConvertGlobalDto2PropertiesMappingItem(PropertiesMappingItem item)
        {
            AvePoint.GCommon.Contract.StorageOptimization.Object.PropertiesMappingItem propertiesMappingItem = new AvePoint.GCommon.Contract.StorageOptimization.Object.PropertiesMappingItem()
            {
                ColumnType = (AvePoint.GCommon.Contract.StorageOptimization.Object.ColumnType)item.ColumnType,
                FileSystemProperty = item.FileSystemProperty,
                SharePointProperty = item.SharePointProperty
            };
            return propertiesMappingItem;
        }

        public static AvePoint.GCommon.Contract.StorageOptimization.Object.SOFilterPolicy ConvertGlobalDto2SOFilterPolicy(SOFilterPolicy policy)
        {
            AvePoint.GCommon.Contract.StorageOptimization.Object.SOFilterPolicy sOFilterPolicy = new AvePoint.GCommon.Contract.StorageOptimization.Object.SOFilterPolicy();
            if (policy.BeginTime != null)
            {
                sOFilterPolicy.BeginTime = new AvePoint.GCommon.Contract.StorageOptimization.Object.DisplayDateTime()
                {
                    IsDayLightSaving = policy.BeginTime.IsDayLightSaving,
                    StartTime = policy.BeginTime.StartTime,
                    TimeZoneId = policy.BeginTime.TimeZoneId
                };
            }
            sOFilterPolicy.Condition = (PolicyCondition)policy.Condition;
            if (policy.EndTime != null)
            {
                sOFilterPolicy.EndTime = new AvePoint.GCommon.Contract.StorageOptimization.Object.DisplayDateTime()
                {
                    IsDayLightSaving = policy.EndTime.IsDayLightSaving,
                    StartTime = policy.EndTime.StartTime,
                    TimeZoneId = policy.EndTime.TimeZoneId
                };
            }
            sOFilterPolicy.IsAnd = policy.IsAnd;
            sOFilterPolicy.Level = GetPolicyLevel(policy.Level);
            sOFilterPolicy.Result = policy.Result;
            sOFilterPolicy.Rule = GetPolicyRuleBase(policy.Rule, policy.ColumnName);
            sOFilterPolicy.RuleType = (PolicyRuleType)policy.RuleType;
            sOFilterPolicy.SequenceNo = policy.SequenceNo;
            if (policy.Value != null)
            {
                sOFilterPolicy.Value = new GCommon.Contract.CommonFilter.PolicyValue()
                {
                    Extension = policy.Value.Extension != null ? new AvePoint.GCommon.Contract.CommonFilter.Extention()
                    {
                        //isDST = policy.Value.Extension.isDST,
                        TimeZoneId = policy.Value.Extension.TimeZoneId
                    } : null,
                    Value1 = policy.Value.Value1,
                    Value1Unit = (AvePoint.GCommon.Contract.CommonFilter.PolicyValueUnit)policy.Value.Value1Unit,
                    Value2 = policy.Value.Value2,
                    Value2Unit = (AvePoint.GCommon.Contract.CommonFilter.PolicyValueUnit)policy.Value.Value2Unit
                };
            }
            return sOFilterPolicy;
        }
        private static PolicyLevel GetPolicyLevel(int level)
        {
            switch (level)
            {
                case 1048576:
                    return PolicyLevel.FileSysFile;                       
                case 2097152:
                    return PolicyLevel.FileSysFolder;
                case 32:
                    return PolicyLevel.Item;
                case 64:
                    return PolicyLevel.Document;
                case 128:
                    return PolicyLevel.Attachment;
                case 256:
                    return PolicyLevel.DocumentVersion;
                case 512:
                    return PolicyLevel.ItemVersion;
                default:
                    return PolicyLevel.None;
            }
        }
        private static PolicyRuleBase GetPolicyRuleBase(string ruleTypeValue, string objectName)
        {
            var ruleType = (AvePoint.RA.Contract.RMRuleManageMent.ArchiverFilterRuleType)Enum.Parse(typeof(AvePoint.RA.Contract.RMRuleManageMent.ArchiverFilterRuleType), ruleTypeValue);
            switch (ruleType)
            {
                case AvePoint.RA.Contract.RMRuleManageMent.ArchiverFilterRuleType.Name:
                    return new NameRule() { Value1 = "Name" };
                case AvePoint.RA.Contract.RMRuleManageMent.ArchiverFilterRuleType.Size:
                    return new SizeRule() { Value1 = "Size" };
                case AvePoint.RA.Contract.RMRuleManageMent.ArchiverFilterRuleType.DocumentSize:
                    return new SizeRule() { Value1 = "Document Size" };
                case AvePoint.RA.Contract.RMRuleManageMent.ArchiverFilterRuleType.SiteCollectionSizeTrigger:
                    return new SizeRule() { Value1 = "Site Collection Size Trigger" };
                case AvePoint.RA.Contract.RMRuleManageMent.ArchiverFilterRuleType.ModifiedTime:
                    return new ModifiedRule() { Value1 = "Modified Time" };
                case AvePoint.RA.Contract.RMRuleManageMent.ArchiverFilterRuleType.CreatedTime:
                    return new CreatedRule() { Value1 = "Created Time" };
                case AvePoint.RA.Contract.RMRuleManageMent.ArchiverFilterRuleType.ModifiedBy:
                    return new ModifiedByRule() { Value1 = "Modified by" };
                case AvePoint.RA.Contract.RMRuleManageMent.ArchiverFilterRuleType.CreatedBy:
                case AvePoint.RA.Contract.RMRuleManageMent.ArchiverFilterRuleType.PrimaryAdministrator:
                    return new CreatedByRule() { Value1 = "Created by" };
                case AvePoint.RA.Contract.RMRuleManageMent.ArchiverFilterRuleType.ContentType:
                    return new ContentTypeRule() { Value1 = "Content Type" };
                case AvePoint.RA.Contract.RMRuleManageMent.ArchiverFilterRuleType.TextColumn:
                    return new ColumnTextRule() { Value1 = objectName };
                case AvePoint.RA.Contract.RMRuleManageMent.ArchiverFilterRuleType.MetadataTextColumn:
                    return new MetadataTextColumnRule() { Value1 = objectName };
                case AvePoint.RA.Contract.RMRuleManageMent.ArchiverFilterRuleType.NumberColumn:
                    return new ColumnNumberRule() { Value1 = objectName };
                case AvePoint.RA.Contract.RMRuleManageMent.ArchiverFilterRuleType.MetadataNumberColumn:
                    return new MetadataNumberColumnRule() { Value1 = objectName };
                case AvePoint.RA.Contract.RMRuleManageMent.ArchiverFilterRuleType.BooleanColumn:
                    return new ColumnBooleanRule() { Value1 = objectName };
                case AvePoint.RA.Contract.RMRuleManageMent.ArchiverFilterRuleType.DateTimeColumn:
                    return new ColumnDateTimeRule() { Value1 = objectName };
                case AvePoint.RA.Contract.RMRuleManageMent.ArchiverFilterRuleType.ParentListTypeID:
                    return new ListTypeRule() { Value1 = "Parent List Type ID" };
                case AvePoint.RA.Contract.RMRuleManageMent.ArchiverFilterRuleType.LastAccessedTime:
                    return new StubLastAccessTimeRule() { Value1 = "Last Accessed Time" };
                case AvePoint.RA.Contract.RMRuleManageMent.ArchiverFilterRuleType.Title:
                    return new TitleRule() { Value1 = "Title" };
                case AvePoint.RA.Contract.RMRuleManageMent.ArchiverFilterRuleType.KeepTheLatestVersion:
                    return new KeepHistoryVersionRule() { Value1 = "Keep the Latest Version" };
                case AvePoint.RA.Contract.RMRuleManageMent.ArchiverFilterRuleType.URL:
                    return new UrlRule() { Value1 = "URL" };
                case AvePoint.RA.Contract.RMRuleManageMent.ArchiverFilterRuleType.Term:
                    return new TermRule() { Value1 = objectName };
                case AvePoint.RA.Contract.RMRuleManageMent.ArchiverFilterRuleType.TextCustomProperty:
                    return new CustomPropertyTextRule() { Value1 = objectName };
                case AvePoint.RA.Contract.RMRuleManageMent.ArchiverFilterRuleType.NumberCustomProperty:
                    return new CustomPropertyNumberRule() { Value1 = objectName };
                case AvePoint.RA.Contract.RMRuleManageMent.ArchiverFilterRuleType.BooleanCustomProperty:
                    return new CustomPropertyBooleanRule() { Value1 = objectName };
                case AvePoint.RA.Contract.RMRuleManageMent.ArchiverFilterRuleType.DateTimeCustomProperty:
                    return new CustomPropertyDateTimeRule() { Value1 = objectName };
                //case ArchiverFilterRuleType.ConversationContent:
                //    return new PostContentRule() { Value1 = "Content" };
                //case ArchiverFilterRuleType.Participant:
                //    return new ParticipationRule() { Value1 = "Participation" };
                //case ArchiverFilterRuleType.PostedBy:
                //    return new PostedByRule() { Value1 = "Posted by" };
                //case ArchiverFilterRuleType.RepliedBy:
                //    return new RepliedByRule() { Value1 = "Replied by" };
                //case ArchiverFilterRuleType.LikedBy:
                //    return new LikedByRule() { Value1 = "Liked by" };
                //case ArchiverFilterRuleType.MentionedName:
                //    return new MentionRule() { Value1 = "Mention" };
                //case ArchiverFilterRuleType.Hashtag:
                //    return new TagRule() { Value1 = "Tags" };
                case AvePoint.RA.Contract.RMRuleManageMent.ArchiverFilterRuleType.Subject:
                    return new SubjectRule() { Value1 = "Subject" };
                case AvePoint.RA.Contract.RMRuleManageMent.ArchiverFilterRuleType.AttachmentCount:
                    return new AttachmentRule() { Value1 = "Attachment Count" };
                case AvePoint.RA.Contract.RMRuleManageMent.ArchiverFilterRuleType.SendDateUTC:
                    return new SendDateUTCRule() { Value1 = "Send Time" };
                case AvePoint.RA.Contract.RMRuleManageMent.ArchiverFilterRuleType.SendFrom:
                    return new SendFromRule() { Value1 = "Send From" };
                case AvePoint.RA.Contract.RMRuleManageMent.ArchiverFilterRuleType.SendTo:
                    return new SendToRule() { Value1 = "Send To" };
                case AvePoint.RA.Contract.RMRuleManageMent.ArchiverFilterRuleType.ParentFolderName:
                    return new ParentFolderNameRule() { Value1 = "Parent Folder Name" };
                case AvePoint.RA.Contract.RMRuleManageMent.ArchiverFilterRuleType.Type:
                    return new FileExtensionsRule() { Value1 = "FileExtensions" };
                case AvePoint.RA.Contract.RMRuleManageMent.ArchiverFilterRuleType.Owner:
                    return new OwnerRule() { Value1 = "Owner" };
                case AvePoint.RA.Contract.RMRuleManageMent.ArchiverFilterRuleType.FilePath:
                    return new FilePathRule() { Value1 = "Path" };
                default:
                    return new PolicyRuleBase() { Value1 = objectName };
            }
        }
        public static AvePoint.GCommon.Contract.CommonFilter.FilterPolicy ConvertGlobalDto2FilterPolicy(AvePoint.RA.Contract.Global.Object.FilterPolicy policy)
        {
            AvePoint.GCommon.Contract.CommonFilter.FilterPolicy filterPolicy = new AvePoint.GCommon.Contract.CommonFilter.FilterPolicy();
            filterPolicy.Condition = (PolicyCondition)policy.Condition;
            filterPolicy.Level = GetPolicyLevel(policy.Level);
            filterPolicy.Result = policy.Result;
            filterPolicy.Rule = GetPolicyRuleBase(policy.Rule, policy.ColumnName);
            filterPolicy.RuleType = (PolicyRuleType)policy.RuleType;
            filterPolicy.SequenceNo = policy.SequenceNo;
            if (policy.Value != null)
            {
                filterPolicy.Value = new AvePoint.GCommon.Contract.CommonFilter.PolicyValue()
                {
                    Value1 = policy.Value.Value1,
                    Value1Unit = (AvePoint.GCommon.Contract.CommonFilter.PolicyValueUnit)policy.Value.Value1Unit,
                    Value2 = policy.Value.Value2,
                    Value2Unit = (AvePoint.GCommon.Contract.CommonFilter.PolicyValueUnit)policy.Value.Value2Unit,
                    Extension = policy.Value.Extension != null ? new AvePoint.GCommon.Contract.CommonFilter.Extention()
                    {
                        //isDST = policy.Value.Extension.isDST,
                        TimeZoneId = policy.Value.Extension.TimeZoneId
                    } : null
                };
            }
            return filterPolicy;
        }

        public static Dictionary<Guid, AvePoint.Hybrid.AgentContract.Rule.RMRuleItemCollection> ConvertGlobalRuleTermMappingToAgentRuleTermMapping(Dictionary<Guid, AvePoint.RA.Contract.Global.Object.RMRuleItemCollection> globalTermAndRulesMappings)
        {
            Dictionary<Guid, AvePoint.Hybrid.AgentContract.Rule.RMRuleItemCollection> agentTermRuleMappings = new Dictionary<Guid, AvePoint.Hybrid.AgentContract.Rule.RMRuleItemCollection>();
            foreach (var termRuleMapping in globalTermAndRulesMappings)
            {
                AvePoint.Hybrid.AgentContract.Rule.RMRuleItemCollection rMRuleItemCollection = new AvePoint.Hybrid.AgentContract.Rule.RMRuleItemCollection();
                rMRuleItemCollection.TermId = termRuleMapping.Value.TermId;
                rMRuleItemCollection.TermName = termRuleMapping.Value.TermName;
                rMRuleItemCollection.HasUnCamlQueryableCondition = termRuleMapping.Value.HasUnCamlQueryableCondition;
                rMRuleItemCollection.CommonRules = new AvePoint.GCommon.Contract.StorageOptimization.Object.RuleCollection();
                rMRuleItemCollection.CommonRules.Rules = new Dictionary<int, GCommon.Contract.StorageOptimization.Object.Rule>();
                foreach (var SORule in termRuleMapping.Value.CommonRules.Rules)
                {
                    rMRuleItemCollection.CommonRules.Rules.Add(SORule.Key, ConvertGlobalRule2Rule(SORule.Value));
                }
                rMRuleItemCollection.Rules = new List<AvePoint.Hybrid.AgentContract.Rule.RMRuleItem>();
                foreach (var RMRuleItem in termRuleMapping.Value.Rules)
                {
                    AvePoint.Hybrid.AgentContract.Rule.RMRuleItem rMRuleItem = new AvePoint.Hybrid.AgentContract.Rule.RMRuleItem();
                    rMRuleItem.RuleId = RMRuleItem.RuleId;
                    rMRuleItem.RuleName = RMRuleItem.RuleName;
                    rMRuleItem.IsMoveRule = RMRuleItem.IsMoveRule;
                    rMRuleItem.ArchiverAction = RMRuleItem.ArchiverAction;
                    rMRuleItem.IsManualApproval = RMRuleItem.IsManualApproval;
                    rMRuleItem.ExportType = (AvePoint.RA.Contract.RMRuleManageMent.ExportTypeValue)Enum.Parse(typeof(AvePoint.RA.Contract.RMRuleManageMent.ExportTypeValue), RMRuleItem.ExportType.ToString());
                    rMRuleItem.HasUnCamlQueryableCondition = RMRuleItem.HasUnCamlQueryableCondition;
                    rMRuleItem.DeleteRecords = RMRuleItem.DeleteRecords;
                    rMRuleItem.RelatedRecordOption = (AvePoint.RA.Contract.RMRuleManageMent.RelatedRecordOption)Enum.Parse(typeof(AvePoint.RA.Contract.RMRuleManageMent.RelatedRecordOption), RMRuleItem.RelatedRecordOption.ToString());
                    rMRuleItem.DisposalClass = RMRuleItem.DisposalClass;
                    rMRuleItem.RuleFilters = new List<Hybrid.AgentContract.Rule.ArchiverRuleFilter>();
                    var soFilterPolicys = rMRuleItemCollection.CommonRules.Rules.Values.ToList().Where(r => r.Id == rMRuleItem.RuleId).FirstOrDefault().SOFilters;
                    foreach (var filterPolicy in soFilterPolicys)
                    {
                        Hybrid.AgentContract.Rule.ArchiverRuleFilter archiverRuleFilter = new Hybrid.AgentContract.Rule.ArchiverRuleFilter(filterPolicy);
                        rMRuleItem.RuleFilters.Add(archiverRuleFilter);
                    }
                    rMRuleItemCollection.Rules.Add(rMRuleItem);
                }
                agentTermRuleMappings.Add(termRuleMapping.Key, rMRuleItemCollection);
            }
            return agentTermRuleMappings;
        }
    }
}
