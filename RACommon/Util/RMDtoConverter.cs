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
using AvePoint.GCommon.Contract.CommonFilter.Rules;
using AvePoint.GCommon.Contract.ExchangeOnline.ExchangeOnlineMailbox.Object;
using AvePoint.GCommon.Contract.Server.ControlPanel.Office365;
using AvePoint.GCommon.Contract.Tree;
using AvePoint.GCommon.Contract.Tree.Object;
using AvePoint.GCommon.Utility;
using AvePoint.GCommon.Utility.Cryptography;
using AvePoint.RA.Common.Aos;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Box;
using AvePoint.RA.Contract.BoxBrowser;
using AvePoint.RA.Contract.CodeView;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.RMReport;
using AvePoint.RA.Contract.RMRuleManageMent;
using AvePoint.RA.Contract.Tenant;
using PnP.Framework.Diagnostics.Tree;
using System;
using System.Collections.Generic;

namespace AvePoint.RA.Common.Util
{
    /// <summary>
    /// 此处转换的原因是为了尽量精简jason的内容
    /// </summary>
    [RACodeReview("Allen Yin")]
    public class RMDtoConverter
    {
        private static readonly AvePoint.RA.Contract.Services.IRALogger logger = RALogger.GetInstance(typeof(RMDtoConverter));
        public static Contract.Object.RMSPTreeNode ConvertSPTree2RMTree(SPTreeNodeDto spTree, Contract.Object.RMSPTreeNode rm = null)
        {
            if (rm == null)
            {
                rm = new Contract.Object.RMSPTreeNode();
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
            rm.IsOrphenOneDrive = spTree.IsOrphenOneDrive;
            if (rm.Name == "{System Folder}")
            {
                rm.Hidden = true;
            }
            rm.ChildrenCount = spTree.ChildrenCount;
            rm.CheckNumber = spTree.CheckNumber;
            rm.TemplateId = spTree.Template;
            rm.TeamName = spTree.TeamName;
            rm.O365TenantId = spTree.O365TenantId;
            rm.TeamsId = spTree.TeamsId;
            if (spTree.NodeExtension != null && spTree.NodeExtension.BposInfo != null)
            {
                rm.BposInfo = spTree.NodeExtension.BposInfo;
            }
            if (spTree.Parent != null && rm.Parent == null)
            {
                Contract.Object.RMSPTreeNode tempParent = new Contract.Object.RMSPTreeNode();
                tempParent.Children = new List<Contract.Object.RMSPTreeNode>() { rm };
                rm.Parent = ConvertSPTree2RMTree(spTree.Parent, tempParent);
            }
            if (spTree.Children != null && spTree.Children.Count > 0 &&
                (rm.Children == null || rm.Children.Count == 0))
            {
                rm.Children = new List<Contract.Object.RMSPTreeNode>();
                foreach (SPTreeNodeDto child in spTree.Children)
                {
                    Contract.Object.RMSPTreeNode temp = new Contract.Object.RMSPTreeNode();
                    temp.Parent = rm;
                    Contract.Object.RMSPTreeNode rmChild = ConvertSPTree2RMTree(child, temp);
                    rm.Children.Add(rmChild);
                }
            }
            return rm;
        }

        public static AvePoint.RA.Contract.Global.Object.RMSPTreeNode ConvertRMSPTreeNode2GlobalDto(Contract.Object.RMSPTreeNode spTree, AvePoint.RA.Contract.Global.Object.RMSPTreeNode rm = null)
        {
            if (rm == null)
            {
                rm = new AvePoint.RA.Contract.Global.Object.RMSPTreeNode();
            }
            rm.Id = spTree.Id;
            rm.FarmId = spTree.FarmId;
            rm.FarmName = spTree.FarmName;
            rm.Name = spTree.Name;
            rm.Title = spTree.Title;
            rm.FullPath = spTree.FullPath;
            rm.Level = (int)spTree.Level;
            rm.NodeType = (int)spTree.NodeType;
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
            rm.TemplateId = spTree.TemplateId;
            rm.TeamName = spTree.TeamName;
            if (spTree.BposInfo != null)
            {
                rm.BposInfo = ConvertBpos2GlobalDto(spTree.BposInfo);
            }
            if (spTree.Parent != null && rm.Parent == null)
            {
                AvePoint.RA.Contract.Global.Object.RMSPTreeNode tempParent = new AvePoint.RA.Contract.Global.Object.RMSPTreeNode();
                tempParent.Children = new List<AvePoint.RA.Contract.Global.Object.RMSPTreeNode>() { rm };
                rm.Parent = ConvertRMSPTreeNode2GlobalDto(spTree.Parent, tempParent);
            }
            if (spTree.Children != null && spTree.Children.Count > 0 &&
                (rm.Children == null || rm.Children.Count == 0))
            {
                rm.Children = new List<AvePoint.RA.Contract.Global.Object.RMSPTreeNode>();
                foreach (Contract.Object.RMSPTreeNode child in spTree.Children)
                {
                    AvePoint.RA.Contract.Global.Object.RMSPTreeNode temp = new AvePoint.RA.Contract.Global.Object.RMSPTreeNode();
                    temp.Parent = rm;
                    AvePoint.RA.Contract.Global.Object.RMSPTreeNode rmChild = ConvertRMSPTreeNode2GlobalDto(child, temp);
                    rm.Children.Add(rmChild);
                }
            }

            if (spTree.AutoClassificationRules != null && spTree.AutoClassificationRules.Count > 0)
            {
                rm.AutoClassificationRules = spTree.AutoClassificationRules.ConvertAll(a => ConvertClassificationRule2GlobalDto(a));
            }
            return rm;
        }

        public static AvePoint.RA.Contract.Global.Object.BposInfo ConvertBpos2GlobalDto(GCommon.Contract.CentralAdmin.Object.BposInfo bposInfo)
        {
            AvePoint.RA.Contract.Global.Object.BposInfo info = new Contract.Global.Object.BposInfo()
            {
                UserAccountInfo = bposInfo.UserAccountInfo != null ? new Contract.Global.Object.BposUserAccountInfo()
                {
                    Domain = bposInfo.UserAccountInfo.Domain,
                    Username = bposInfo.UserAccountInfo.Username,
                    Password = bposInfo.UserAccountInfo.Password
                } : null,
                SiteUrl = bposInfo.SiteUrl
            };
            return info;
        }

        public static AvePoint.RA.Contract.Global.Object.ClassificationRule ConvertClassificationRule2GlobalDto(Contract.RMRuleManageMent.ClassificationRule rule)
        {
            AvePoint.RA.Contract.Global.Object.ClassificationRule classificationRule = new Contract.Global.Object.ClassificationRule()
            {
                AndOrExpression = rule.AndOrExpression,
                Category = (int)rule.Category,
                IsDefaultRule = rule.IsDefaultRule,
                NoDefaultTerm = rule.NoDefaultTerm,
                RuleLevel = (int)rule.RuleLevel,
                TermId = rule.TermId,
                RuleOrder = rule.RuleOrder,
                TermIsDeprecated = rule.TermIsDeprecated,
                TermIsRemoved = rule.TermIsRemoved,
                TermName = rule.TermName,
                FilterGroups = rule.FilterGroups != null && rule.FilterGroups.Count > 0 ? rule.FilterGroups.ConvertAll(f => ConvertFilterGroup2GlobalDto(f)) : new List<Contract.Global.Object.FilterGroup>()
            };
            return classificationRule;
        }

        public static AvePoint.RA.Contract.Global.Object.FilterGroup ConvertFilterGroup2GlobalDto(Contract.RMRuleManageMent.FilterGroup group)
        {
            AvePoint.RA.Contract.Global.Object.FilterGroup filterGroup = new Contract.Global.Object.FilterGroup();
            filterGroup.CombineMode = (AvePoint.RA.Contract.RMRuleManageMent.ArchiverFilterCombineMode)group.CombineMode;
            filterGroup.TrueFalse = group.TrueFalse;
            if (group.FilterGroups != null && group.FilterGroups.Count > 0)
            {
                filterGroup.FilterGroups = group.FilterGroups.ConvertAll(f => ConvertFilterGroup2GlobalDto(f));
            }
            else
            {
                filterGroup.FilterGroups = new List<Contract.Global.Object.FilterGroup>();
            }
            if (group.Filters != null && group.Filters.Count > 0)
            {
                filterGroup.Filters = group.Filters.ConvertAll(f => ConvertRuleFilter2GlobalDto(f));
            }
            else
            {
                filterGroup.Filters = new List<Contract.Global.Object.RuleFilter>();
            }
            return filterGroup;
        }

        public static AvePoint.RA.Contract.Global.Object.RuleFilter ConvertRuleFilter2GlobalDto(Contract.RMRuleManageMent.RuleFilter filter)
        {
            AvePoint.RA.Contract.Global.Object.RuleFilter ruleFilter = new Contract.Global.Object.RuleFilter()
            {
                CombineMode = (int)filter.CombineMode,
                Condition = (int)filter.Condition,
                FilterCretia = filter.FilterCretia,
                filterName = filter.filterName,
                Level = (int)filter.Level,
                RuleBaseString = filter.RuleBase?.Value1,
                RuleType = (int)filter.RuleType,
                SequenceNo = filter.SequenceNo,
                Value1 = filter.Value1,
                Value1Unit = (int)filter.Value1Unit,
                Value2 = filter.Value2,
                Value2Unit = (int)filter.Value2Unit,
                EndTimeInfo = filter.EndTimeInfo != null ? new Contract.Global.Object.DisplayDateTime()
                {
                    IsDayLightSaving = filter.EndTimeInfo.IsDayLightSaving,
                    StartTime = filter.EndTimeInfo.StartTime,
                    TimeZoneId = filter.EndTimeInfo.TimeZoneId
                } : null,
                StartTimeInfo = filter.StartTimeInfo != null ? new Contract.Global.Object.DisplayDateTime()
                {
                    IsDayLightSaving = filter.StartTimeInfo.IsDayLightSaving,
                    StartTime = filter.StartTimeInfo.StartTime,
                    TimeZoneId = filter.StartTimeInfo.TimeZoneId
                } : null
            };
            return ruleFilter;
        }

        public static AvePoint.RA.Contract.Global.Object.FSTreeNodeDto ConvertFSTreeNode2GlobalDto(FSTreeNodeDto rmTree, AvePoint.RA.Contract.Global.Object.FSTreeNodeDto fs = null, bool needDecryptPath = false)
        {
            if (fs == null)
            {
                fs = new AvePoint.RA.Contract.Global.Object.FSTreeNodeDto();
            }
            fs.Id = new Guid(rmTree.ID);
            fs.FarmID = rmTree.FarmID;
            fs.Name = rmTree.Name;
            fs.FullPath = needDecryptPath ? EncodeUtil.DecryptByCommunicationKey(rmTree.FullPath) : rmTree.FullPath;
            fs.Level = (int)rmTree.Level;
            fs.NodeType = (int)rmTree.Type;
            fs.Expanded = rmTree.Expanded;
            fs.ChildrenCount = rmTree.ChildrenCount;
            fs.CheckNumber = rmTree.CheckNumber;

            fs.Domain = rmTree.Domain;
            fs.Username = rmTree.Username;
            fs.EncryptedPassword = rmTree.EncryptedPassword;
            fs.TimeStamp = rmTree.TimeStamp;
            //fs.IncludeNew = Convert.ToBoolean(rmTree.IncludeNew) ? IncludeNewState.Checked : IncludeNewState.Unchecked;
            //if (fs.NodeExtension == null)
            //{
            //    fs.NodeExtension = new NodeExtensionDto();
            //}
            //sp.NodeExtension.BposInfo = rmTree.BposInfo;
            if (rmTree.Parent != null && fs.Parent == null)
            {
                AvePoint.RA.Contract.Global.Object.FSTreeNodeDto tempParent = new AvePoint.RA.Contract.Global.Object.FSTreeNodeDto();
                tempParent.Children = new List<AvePoint.RA.Contract.Global.Object.FSTreeNodeDto> { fs };
                fs.Parent = ConvertFSTreeNode2GlobalDto(rmTree.Parent, tempParent, needDecryptPath);
                fs.ParentId = rmTree.Parent.ID.ToString();
            }
            if (rmTree.CheckNumber == 1)
            {
                return fs;
            }
            if (rmTree.Children != null && rmTree.Children.Count > 0 &&
                (fs.Children == null || fs.Children.Count == 0))
            {
                fs.Children = new List<AvePoint.RA.Contract.Global.Object.FSTreeNodeDto>();
                foreach (FSTreeNodeDto child in rmTree.Children)
                {
                    if (HasSelectNodeForFS(child))
                    {
                        AvePoint.RA.Contract.Global.Object.FSTreeNodeDto tempChild = new AvePoint.RA.Contract.Global.Object.FSTreeNodeDto();
                        tempChild.Parent = fs;
                        tempChild.ParentId = fs.Id.ToString();
                        fs.Children.Add(ConvertFSTreeNode2GlobalDto(child, tempChild, needDecryptPath));
                    }
                    else
                    {
                        logger.Debug("No select node in {0}", child.Name);
                    }
                }
            }
            return fs;
        }

        public static AvePoint.RA.Contract.Global.Object.Rule ConvertRule2GlobalDto(GCommon.Contract.StorageOptimization.Object.Rule rule)
        {
            AvePoint.RA.Contract.Global.Object.Rule ruleDto = new Contract.Global.Object.Rule();

            Dictionary<int, string> expressions = new Dictionary<int, string>();
            foreach (var exp in rule.AndOrExpression)
            {
                expressions.Add((int)exp.Key, exp.Value);
            }
            ruleDto.AndOrExpression = expressions;
            if (rule.ArchiverSetting != null)
            {
                ruleDto.ArchiverSetting = new Contract.Global.Object.ArchiverSetting()
                {
                    EnableArchiverVEOMerge = rule.ArchiverSetting.EnableArchiverVEOMerge,
                    FileNumber = rule.ArchiverSetting.FileNumber,
                    FileSize = rule.ArchiverSetting.FileSize,
                    FolderName = rule.ArchiverSetting.FolderName,
                    IsDeleteOldFile = rule.ArchiverSetting.IsDeleteOldFile,
                    NumberOfThreadSendingEmail = rule.ArchiverSetting.NumberOfThreadSendingEmail,
                    StoragePolicyId = rule.StoragePolicyId,
                    
                };
            }
            if (rule.ArchiverVEOSetting != null)
            {
                ruleDto.ArchiverVEOSetting = new Contract.Global.Object.ArchiverVEOSetting()
                {
                    AgencyId = rule.ArchiverVEOSetting.AgencyId,
                    ConsignmentNumber = rule.ArchiverVEOSetting.ConsignmentNumber,
                    SeriesIdentifier = rule.ArchiverVEOSetting.SeriesIdentifier,
                    SeriesNumber = rule.ArchiverVEOSetting.SeriesNumber
                };
            }
            ruleDto.Compression = rule.Compression;
            ruleDto.DataSecurity = rule.DataSecurity;
            ruleDto.DeleteRecords = rule.DeleteRecords;
            ruleDto.LockRecordBeforeDestroy = rule.LockRecordBeforeDestroy;
            ruleDto.DeclareLinkFile = rule.DeclareLinkFile;
            ruleDto.DisposalClass = rule.DisposalClass;
            ruleDto.Encryption = rule.Encryption;
            ruleDto.EncryptionInfoId = rule.EncryptionInfoId;
            ruleDto.EncryptionInfoName = rule.EncryptionInfoName;
            ruleDto.StoragePolicyId = rule.StoragePolicyId;
            ruleDto.ModifyTime = rule.ModifyTime;
            if (rule.ExportInfo != null)
            {
                ruleDto.ExportInfo = new Contract.Global.Object.SOExportInfo()
                {
                    exportLocationId = rule.ExportInfo.exportLocationId,
                    exportLocationName = rule.ExportInfo.exportLocationName,
                    exportSPDataOption = (AvePoint.RA.Contract.Global.Object.ExportSPDataOption)rule.ExportInfo.exportSPDataOption,
                    exportType = (AvePoint.RA.Contract.Global.Object.ExportTypeValue)rule.ExportInfo.exportType
                };
            }
            ruleDto.ExportType = (AvePoint.RA.Contract.Global.Object.ExportTypeValue)rule.ExportType;
            ruleDto.FileVEO = rule.FileVEO;
            if (rule.Filters != null)
            {
                ruleDto.Filters = rule.Filters.ConvertAll(f => ConvertFilterPolicy2GlobalDto(f));
            }
            if (rule.FSRule != null)
            {
                ruleDto.FSRule = ConvertRule2GlobalDto(rule.FSRule);
            };
            //if (rule.FSRuleString != null)
            //{
            //    ruleDto.FSRuleString = rule.FSRuleString;
            //};
            if (rule.SPLocalRule != null)
            {
                ruleDto.SPLocalRule = ConvertRule2GlobalDto(rule.SPLocalRule);
            };
            ruleDto.Id = rule.Id;
            ruleDto.IsManualApproval = rule.IsManualApproval;
            ruleDto.IsSendEamilToOwner = rule.IsSendEamilToOwner;
            ruleDto.KeepDataOption = rule.KeepDataOption;
            ruleDto.ManifestVEO = rule.ManifestVEO;
            if (rule.MoveToRecordCenterAndDelareSetting != null && rule.MoveToRecordCenterAndDelareSetting.DestinationLocation != null)
            {
                ruleDto.MoveToRecordCenterAndDelareSetting = new Contract.Global.Object.MoveToRecordCenterAndDelareSetting()
                {
                    ContentConflictResolution = (AvePoint.RA.Contract.Global.Object.ContentConflictResolution)rule.MoveToRecordCenterAndDelareSetting.ContentConflictResolution,
                    DelaredRecord = rule.MoveToRecordCenterAndDelareSetting.DelaredRecord,
                    DestinationLocation = new Contract.Global.Object.DestinationLocationInfo()
                    {
                        Password = rule.MoveToRecordCenterAndDelareSetting.DestinationLocation.Password,
                        ServiceAccountName = rule.MoveToRecordCenterAndDelareSetting.DestinationLocation.ServiceAccountName,
                        Url = rule.MoveToRecordCenterAndDelareSetting.DestinationLocation.Url,
                        UserName = rule.MoveToRecordCenterAndDelareSetting.DestinationLocation.UserName
                    },
                    IsMoveVersions = rule.MoveToRecordCenterAndDelareSetting.IsMoveVersions,
                    KeepFolderStructure = rule.MoveToRecordCenterAndDelareSetting.KeepFolderStructure,
                    LeaveLinkInSource = rule.MoveToRecordCenterAndDelareSetting.LeaveLinkInSource,
                    OperateDataMode = (AvePoint.RA.Contract.Global.Object.OperatingSharePointDataMode)rule.MoveToRecordCenterAndDelareSetting.OperateDataMode,
                    OriginalMetaDataAsXML = rule.MoveToRecordCenterAndDelareSetting.OriginalMetaDataAsXML
                };
            };
            ruleDto.NAAConfigFile = rule.NAAConfigFile;
            ruleDto.Name = rule.Name;
            ruleDto.NARAConfigFile = rule.NARAConfigFile;
            ruleDto.Order = rule.Order;
            ruleDto.OrderList = rule.OrderList;
            ruleDto.PolicyLevel = (int)rule.PolicyLevel;
            ruleDto.RecordVEO = rule.RecordVEO;
            ruleDto.RelatedRecordOption = (AvePoint.RA.Contract.Global.Object.RelatedRecordOption)rule.RelatedRecordOption;
            ruleDto.ReviewType = (AvePoint.RA.Contract.Global.Object.ReviewType)rule.ReviewType;
            if (rule.SOFilters != null)
            {
                ruleDto.SOFilters = rule.SOFilters.ConvertAll(f => ConvertSOFilterPolicy2GlobalDto(f));
            }
            if (rule.spMoveOption != null && rule.spMoveOption.MoveDestination != null)
            {
                ruleDto.spMoveOption = new Contract.Global.Object.MoveOption()
                {
                    DestFlag = (AvePoint.RA.Contract.Global.Object.RecordFlag)rule.spMoveOption.DestFlag,
                    MoveDestination = new Contract.Global.Object.MoveDestination()
                    {
                        ContainerId = rule.spMoveOption.MoveDestination.ContainerId,
                        DeleteSourceItem = rule.spMoveOption.MoveDestination.DeleteSourceItem,
                        DestMode = (AvePoint.RA.Contract.Global.Object.DestMode)rule.spMoveOption.MoveDestination.DestMode,
                        FSAccountProfileId = rule.spMoveOption.MoveDestination.FSAccountProfileId,
                        FSConectionPath = rule.spMoveOption.MoveDestination.FSConectionPath,
                        FSPath = rule.spMoveOption.MoveDestination.FSPath,
                        FSTreeStr = rule.spMoveOption.MoveDestination.FSTreeStr,
                        FSTreeNode = rule.spMoveOption.MoveDestination.FSTreeNode == null ? null : ConvertFSTreeNode2GlobalDto(rule.spMoveOption.MoveDestination.FSTreeNode, null, true),
                        KeepSourceClassification = rule.spMoveOption.MoveDestination.KeepSourceClassification,
                        NotDeclareMovedData = rule.spMoveOption.MoveDestination.NotDeclareMovedData,
                        SPAccountProfileId = rule.spMoveOption.MoveDestination.SPAccountProfileId,
                        SPTreeStr = rule.spMoveOption.MoveDestination.SPTreeStr,
                        SPUrl = rule.spMoveOption.MoveDestination.SPUrl
                    },
                    MoveSetting = rule.spMoveOption.MoveSetting != null ? new Contract.Global.Object.MoveRecordSetting()
                    {
                        ConflictType = (AvePoint.RA.Contract.Global.Object.ConflictType)rule.spMoveOption.MoveSetting.ConflictType,
                        ContainerLevelConflictOption = (AvePoint.RA.Contract.Global.Object.ConflictOption)rule.spMoveOption.MoveSetting.ContainerLevelConflictOption,
                        FileInherit = rule.spMoveOption.MoveSetting.FileInherit,
                        FolderInherit = rule.spMoveOption.MoveSetting.FolderInherit,
                        FolderUnderInherit = rule.spMoveOption.MoveSetting.FolderUnderInherit,
                        ItemLevelConflictOption = (AvePoint.RA.Contract.Global.Object.ConflictOption)rule.spMoveOption.MoveSetting.ItemLevelConflictOption,
                        PhysicalHoldConflictOption = (AvePoint.RA.Contract.Global.Object.PhysicalHoldConflictOption)rule.spMoveOption.MoveSetting.PhysicalHoldConflictOption,
                        FilePropertiesMapping = rule.spMoveOption.MoveSetting.FilePropertiesMapping != null && rule.spMoveOption.MoveSetting.FilePropertiesMapping.PropertiesMappingItems != null ? new Contract.Global.Object.FilePropertiesMapping()
                        {
                            PropertiesMappingItems = rule.spMoveOption.MoveSetting.FilePropertiesMapping.PropertiesMappingItems.ConvertAll(p => ConvertPropertiesMappingItem2GlobalDto(p))
                        } : null
                    } : null,
                    SourceFlag = (AvePoint.RA.Contract.Global.Object.RecordFlag)rule.spMoveOption.SourceFlag
                };
            }
            if (rule.TagContentInfo != null && rule.TagContentInfo.Count > 0)
            {
                ruleDto.TagContentInfo = rule.TagContentInfo.ConvertAll(t => ConvertTagContentInfo2GlobalDto(t));
            }
            ruleDto.Type = (AvePoint.RA.Contract.Global.Object.RuleType)rule.Type;
            if (rule.UserInfos != null && rule.UserInfos.Count > 0)
            {
                ruleDto.UserInfos = rule.UserInfos.ConvertAll(u => ConvertUserInfo2GlobalDto(u));
            }
            ruleDto.WorkflowId = rule.WorkflowId;
            return ruleDto;
        }

        public static GCommon.Contract.StorageOptimization.Object.Rule ConvertGCommonSPLocalRule2GCommonRule(GCommon.Contract.StorageOptimization.Object.Rule commonRule)
        {
            GCommon.Contract.StorageOptimization.Object.Rule ruleDto = new GCommon.Contract.StorageOptimization.Object.Rule();
            var rule = commonRule.SPLocalRule;
            ruleDto.AndOrExpression = rule.AndOrExpression;
            if (rule.ArchiverSetting != null)
            {
                ruleDto.ArchiverSetting = new GCommon.Contract.StorageOptimization.Object.ArchiverSetting()
                {
                    EnableArchiverVEOMerge = rule.ArchiverSetting.EnableArchiverVEOMerge,
                    FileNumber = rule.ArchiverSetting.FileNumber,
                    FileSize = rule.ArchiverSetting.FileSize,
                    FolderName = rule.ArchiverSetting.FolderName,
                    IsDeleteOldFile = rule.ArchiverSetting.IsDeleteOldFile,
                    NumberOfThreadSendingEmail = rule.ArchiverSetting.NumberOfThreadSendingEmail
                };
            }
            if (rule.ArchiverVEOSetting != null)
            {
                ruleDto.ArchiverVEOSetting = new GCommon.Contract.StorageOptimization.Object.ArchiverVEOSetting()
                {
                    AgencyId = rule.ArchiverVEOSetting.AgencyId,
                    ConsignmentNumber = rule.ArchiverVEOSetting.ConsignmentNumber,
                    SeriesIdentifier = rule.ArchiverVEOSetting.SeriesIdentifier,
                    SeriesNumber = rule.ArchiverVEOSetting.SeriesNumber
                };
            }
            ruleDto.Compression = rule.Compression;
            ruleDto.DataSecurity = rule.DataSecurity;
            ruleDto.DeleteRecords = rule.DeleteRecords;
            ruleDto.LockRecordBeforeDestroy = rule.LockRecordBeforeDestroy;
            ruleDto.DeclareLinkFile = rule.DeclareLinkFile;
            ruleDto.DisposalClass = rule.DisposalClass;
            ruleDto.Encryption = rule.Encryption;
            ruleDto.EncryptionInfoId = rule.EncryptionInfoId;
            ruleDto.EncryptionInfoName = rule.EncryptionInfoName;
            if (rule.ExportInfo != null)
            {
                ruleDto.ExportInfo = new GCommon.Contract.StorageOptimization.Object.SOExportInfo()
                {
                    exportLocationId = rule.ExportInfo.exportLocationId,
                    exportLocationName = rule.ExportInfo.exportLocationName,
                    exportSPDataOption = (GCommon.Contract.StorageOptimization.Object.ExportSPDataOption)rule.ExportInfo.exportSPDataOption,
                    exportType = (GCommon.Contract.StorageOptimization.Object.ExportTypeValue)rule.ExportInfo.exportType
                };
            }
            ruleDto.ExportType = (GCommon.Contract.StorageOptimization.Object.ExportTypeValue)rule.ExportType;
            ruleDto.FileVEO = rule.FileVEO;
            if (rule.Filters != null)
            {
                ruleDto.Filters = rule.Filters;
            }
            else
            {
                ruleDto.Filters = ConvertCommonSOFiletrPolicyToCommonFilterPolicy(rule.SOFilters);
            }
            ruleDto.Id = commonRule.Id;
            ruleDto.IsManualApproval = rule.IsManualApproval;
            ruleDto.IsSendEamilToOwner = rule.IsSendEamilToOwner;
            ruleDto.KeepDataOption = rule.KeepDataOption;
            ruleDto.ManifestVEO = rule.ManifestVEO;
            ruleDto.MoveToRecordCenterAndDelareSetting = rule.MoveToRecordCenterAndDelareSetting;
            ruleDto.NAAConfigFile = rule.NAAConfigFile;
            ruleDto.Name = commonRule.Name;
            ruleDto.NARAConfigFile = rule.NARAConfigFile;
            ruleDto.Order = rule.Order;
            ruleDto.OrderList = rule.OrderList;
            ruleDto.PolicyLevel = rule.PolicyLevel;
            ruleDto.RecordVEO = rule.RecordVEO;
            ruleDto.RelatedRecordOption = rule.RelatedRecordOption;
            ruleDto.ReviewType = rule.ReviewType;
            if (rule.SOFilters != null)
            {
                ruleDto.SOFilters = rule.SOFilters;
            }
            ruleDto.spMoveOption = rule.spMoveOption;
            if (rule.TagContentInfo != null && rule.TagContentInfo.Count > 0)
            {
                ruleDto.TagContentInfo = rule.TagContentInfo;
            }
            ruleDto.Type = rule.Type;
            if (rule.UserInfos != null && rule.UserInfos.Count > 0)
            {
                ruleDto.UserInfos = rule.UserInfos;
            }
            ruleDto.WorkflowId = rule.WorkflowId;
            return ruleDto;
        }

        public static AvePoint.RA.Contract.Global.Object.Rule ConvertGlbalSPLocalRule2GlobalRule(AvePoint.RA.Contract.Global.Object.Rule globalRule)
        {
            AvePoint.RA.Contract.Global.Object.Rule ruleDto = new AvePoint.RA.Contract.Global.Object.Rule();
            var rule = globalRule.SPLocalRule;
            Dictionary<int, string> expressions = new Dictionary<int, string>();
            foreach (var exp in rule.AndOrExpression)
            {
                expressions.Add((int)exp.Key, exp.Value);
            }
            ruleDto.AndOrExpression = expressions;
            if (rule.ArchiverSetting != null)
            {
                ruleDto.ArchiverSetting = new Contract.Global.Object.ArchiverSetting()
                {
                    EnableArchiverVEOMerge = rule.ArchiverSetting.EnableArchiverVEOMerge,
                    FileNumber = rule.ArchiverSetting.FileNumber,
                    FileSize = rule.ArchiverSetting.FileSize,
                    FolderName = rule.ArchiverSetting.FolderName,
                    IsDeleteOldFile = rule.ArchiverSetting.IsDeleteOldFile,
                    NumberOfThreadSendingEmail = rule.ArchiverSetting.NumberOfThreadSendingEmail
                };
            }
            if (rule.ArchiverVEOSetting != null)
            {
                ruleDto.ArchiverVEOSetting = new Contract.Global.Object.ArchiverVEOSetting()
                {
                    AgencyId = rule.ArchiverVEOSetting.AgencyId,
                    ConsignmentNumber = rule.ArchiverVEOSetting.ConsignmentNumber,
                    SeriesIdentifier = rule.ArchiverVEOSetting.SeriesIdentifier,
                    SeriesNumber = rule.ArchiverVEOSetting.SeriesNumber
                };
            }
            ruleDto.Compression = rule.Compression;
            ruleDto.DataSecurity = rule.DataSecurity;
            ruleDto.DeleteRecords = rule.DeleteRecords;
            ruleDto.LockRecordBeforeDestroy = rule.LockRecordBeforeDestroy;
            ruleDto.DisposalClass = rule.DisposalClass;
            ruleDto.Encryption = rule.Encryption;
            ruleDto.EncryptionInfoId = rule.EncryptionInfoId;
            ruleDto.EncryptionInfoName = rule.EncryptionInfoName;
            if (rule.ExportInfo != null)
            {
                ruleDto.ExportInfo = new Contract.Global.Object.SOExportInfo()
                {
                    exportLocationId = rule.ExportInfo.exportLocationId,
                    exportLocationName = rule.ExportInfo.exportLocationName,
                    exportSPDataOption = (AvePoint.RA.Contract.Global.Object.ExportSPDataOption)rule.ExportInfo.exportSPDataOption,
                    exportType = (AvePoint.RA.Contract.Global.Object.ExportTypeValue)rule.ExportInfo.exportType
                };
            }
            ruleDto.ExportType = (AvePoint.RA.Contract.Global.Object.ExportTypeValue)rule.ExportType;
            ruleDto.FileVEO = rule.FileVEO;
            if (rule.Filters != null)
            {
                ruleDto.Filters = rule.Filters;
            }
            else
            {
                ruleDto.Filters = ConvertGlobalSOFiletrPolicyToGlobalFilterPolicy(rule.SOFilters);
            }
            ruleDto.Id = globalRule.Id;
            ruleDto.IsManualApproval = rule.IsManualApproval;
            ruleDto.IsSendEamilToOwner = rule.IsSendEamilToOwner;
            ruleDto.KeepDataOption = rule.KeepDataOption;
            ruleDto.ManifestVEO = rule.ManifestVEO;
            if (rule.MoveToRecordCenterAndDelareSetting != null && rule.MoveToRecordCenterAndDelareSetting.DestinationLocation != null)
            {
                ruleDto.MoveToRecordCenterAndDelareSetting = new Contract.Global.Object.MoveToRecordCenterAndDelareSetting()
                {
                    ContentConflictResolution = (AvePoint.RA.Contract.Global.Object.ContentConflictResolution)rule.MoveToRecordCenterAndDelareSetting.ContentConflictResolution,
                    DelaredRecord = rule.MoveToRecordCenterAndDelareSetting.DelaredRecord,
                    DestinationLocation = new Contract.Global.Object.DestinationLocationInfo()
                    {
                        Password = rule.MoveToRecordCenterAndDelareSetting.DestinationLocation.Password,
                        ServiceAccountName = rule.MoveToRecordCenterAndDelareSetting.DestinationLocation.ServiceAccountName,
                        Url = rule.MoveToRecordCenterAndDelareSetting.DestinationLocation.Url,
                        UserName = rule.MoveToRecordCenterAndDelareSetting.DestinationLocation.UserName
                    },
                    IsMoveVersions = rule.MoveToRecordCenterAndDelareSetting.IsMoveVersions,
                    KeepFolderStructure = rule.MoveToRecordCenterAndDelareSetting.KeepFolderStructure,
                    LeaveLinkInSource = rule.MoveToRecordCenterAndDelareSetting.LeaveLinkInSource,
                    OperateDataMode = (AvePoint.RA.Contract.Global.Object.OperatingSharePointDataMode)rule.MoveToRecordCenterAndDelareSetting.OperateDataMode,
                    OriginalMetaDataAsXML = rule.MoveToRecordCenterAndDelareSetting.OriginalMetaDataAsXML
                };
            };
            ruleDto.NAAConfigFile = rule.NAAConfigFile;
            ruleDto.Name = globalRule.Name;
            ruleDto.NARAConfigFile = rule.NARAConfigFile;
            ruleDto.Order = rule.Order;
            ruleDto.OrderList = rule.OrderList;
            ruleDto.PolicyLevel = (int)rule.PolicyLevel;
            ruleDto.RecordVEO = rule.RecordVEO;
            ruleDto.RelatedRecordOption = (AvePoint.RA.Contract.Global.Object.RelatedRecordOption)rule.RelatedRecordOption;
            ruleDto.ReviewType = (AvePoint.RA.Contract.Global.Object.ReviewType)rule.ReviewType;
            if (rule.SOFilters != null)
            {
                ruleDto.SOFilters = rule.SOFilters;
            }
            if (rule.spMoveOption != null && rule.spMoveOption.MoveDestination != null)
            {
                ruleDto.spMoveOption = new Contract.Global.Object.MoveOption()
                {
                    DestFlag = (AvePoint.RA.Contract.Global.Object.RecordFlag)rule.spMoveOption.DestFlag,
                    MoveDestination = new Contract.Global.Object.MoveDestination()
                    {
                        ContainerId = rule.spMoveOption.MoveDestination.ContainerId,
                        DeleteSourceItem = rule.spMoveOption.MoveDestination.DeleteSourceItem,
                        DestMode = (AvePoint.RA.Contract.Global.Object.DestMode)rule.spMoveOption.MoveDestination.DestMode,
                        FSAccountProfileId = rule.spMoveOption.MoveDestination.FSAccountProfileId,
                        FSConectionPath = rule.spMoveOption.MoveDestination.FSConectionPath,
                        FSPath = rule.spMoveOption.MoveDestination.FSPath,
                        FSTreeStr = rule.spMoveOption.MoveDestination.FSTreeStr,
                        KeepSourceClassification = rule.spMoveOption.MoveDestination.KeepSourceClassification,
                        NotDeclareMovedData = rule.spMoveOption.MoveDestination.NotDeclareMovedData,
                        SPAccountProfileId = rule.spMoveOption.MoveDestination.SPAccountProfileId,
                        SPTreeStr = rule.spMoveOption.MoveDestination.SPTreeStr,
                        SPUrl = rule.spMoveOption.MoveDestination.SPUrl
                    },
                    MoveSetting = rule.spMoveOption.MoveSetting != null ? new Contract.Global.Object.MoveRecordSetting()
                    {
                        ConflictType = (AvePoint.RA.Contract.Global.Object.ConflictType)rule.spMoveOption.MoveSetting.ConflictType,
                        ContainerLevelConflictOption = (AvePoint.RA.Contract.Global.Object.ConflictOption)rule.spMoveOption.MoveSetting.ContainerLevelConflictOption,
                        FileInherit = rule.spMoveOption.MoveSetting.FileInherit,
                        FolderInherit = rule.spMoveOption.MoveSetting.FolderInherit,
                        FolderUnderInherit = rule.spMoveOption.MoveSetting.FolderUnderInherit,
                        ItemLevelConflictOption = (AvePoint.RA.Contract.Global.Object.ConflictOption)rule.spMoveOption.MoveSetting.ItemLevelConflictOption,
                        PhysicalHoldConflictOption = (AvePoint.RA.Contract.Global.Object.PhysicalHoldConflictOption)rule.spMoveOption.MoveSetting.PhysicalHoldConflictOption,
                        FilePropertiesMapping = rule.spMoveOption.MoveSetting.FilePropertiesMapping != null && rule.spMoveOption.MoveSetting.FilePropertiesMapping.PropertiesMappingItems != null ? new Contract.Global.Object.FilePropertiesMapping()
                        {
                            PropertiesMappingItems = rule.spMoveOption.MoveSetting.FilePropertiesMapping.PropertiesMappingItems
                        } : null
                    } : null,
                    SourceFlag = (AvePoint.RA.Contract.Global.Object.RecordFlag)rule.spMoveOption.SourceFlag
                };
            }
            if (rule.TagContentInfo != null && rule.TagContentInfo.Count > 0)
            {
                ruleDto.TagContentInfo = rule.TagContentInfo;
            }
            ruleDto.Type = (AvePoint.RA.Contract.Global.Object.RuleType)rule.Type;
            if (rule.UserInfos != null && rule.UserInfos.Count > 0)
            {
                ruleDto.UserInfos = rule.UserInfos;
            }
            ruleDto.WorkflowId = rule.WorkflowId;
            return ruleDto;
        }

        public static List<AvePoint.RA.Contract.Global.Object.FilterPolicy> ConvertGlobalSOFiletrPolicyToGlobalFilterPolicy(List<AvePoint.RA.Contract.Global.Object.SOFilterPolicy> soFilters)
        {
            List<AvePoint.RA.Contract.Global.Object.FilterPolicy> filerPolicies = new List<AvePoint.RA.Contract.Global.Object.FilterPolicy>();
            foreach (var filter in soFilters)
            {
                AvePoint.RA.Contract.Global.Object.FilterPolicy filterPolicy = new AvePoint.RA.Contract.Global.Object.FilterPolicy();
                if (filter.Condition == (int)PolicyCondition.Exactly || filter.Condition == (int)PolicyCondition.Equals)
                {
                    filterPolicy.Condition = (int)PolicyCondition.Equals;
                }
                else
                {
                    filterPolicy.Condition = filter.Condition;
                }
                filterPolicy.Level = filter.Level;
                filterPolicy.Rule = filter.Rule;
                filterPolicy.ColumnName = filter.ColumnName;
                filterPolicy.RuleType = filter.RuleType;
                filterPolicy.SequenceNo = filter.SequenceNo;
                filterPolicy.Value = filter.Value;

                filerPolicies.Add(filterPolicy);
            }
            return filerPolicies;
        }

        public static List<AvePoint.GCommon.Contract.CommonFilter.FilterPolicy> ConvertCommonSOFiletrPolicyToCommonFilterPolicy(List<AvePoint.GCommon.Contract.StorageOptimization.Object.SOFilterPolicy> soFilters)
        {
            List<AvePoint.GCommon.Contract.CommonFilter.FilterPolicy> filerPolicies = new List<AvePoint.GCommon.Contract.CommonFilter.FilterPolicy>();
            foreach (var filter in soFilters)
            {
                AvePoint.GCommon.Contract.CommonFilter.FilterPolicy filterPolicy = new AvePoint.GCommon.Contract.CommonFilter.FilterPolicy();
                if (filter.Condition == AvePoint.GCommon.Contract.CommonFilter.PolicyCondition.Exactly || filter.Condition == AvePoint.GCommon.Contract.CommonFilter.PolicyCondition.Equals)
                {
                    filterPolicy.Condition = AvePoint.GCommon.Contract.CommonFilter.PolicyCondition.Equals;
                }
                else
                {
                    filterPolicy.Condition = filter.Condition;
                }
                filterPolicy.Level = filter.Level;
                filterPolicy.Rule = filter.Rule;
                filterPolicy.RuleType = filter.RuleType;
                filterPolicy.SequenceNo = filter.SequenceNo;
                filterPolicy.Value = filter.Value;

                filerPolicies.Add(filterPolicy);
            }
            return filerPolicies;
        }

        public static AvePoint.RA.Contract.Global.Object.PropertiesMappingItem ConvertPropertiesMappingItem2GlobalDto(GCommon.Contract.StorageOptimization.Object.PropertiesMappingItem item)
        {
            AvePoint.RA.Contract.Global.Object.PropertiesMappingItem propertiesMappingItem = new Contract.Global.Object.PropertiesMappingItem()
            {
                ColumnType = (AvePoint.RA.Contract.Global.Object.ColumnType)item.ColumnType,
                FileSystemProperty = item.FileSystemProperty,
                SharePointProperty = item.SharePointProperty
            };
            return propertiesMappingItem;
        }

        public static AvePoint.RA.Contract.Global.Object.SOFilterPolicy ConvertSOFilterPolicy2GlobalDto(GCommon.Contract.StorageOptimization.Object.SOFilterPolicy policy)
        {
            AvePoint.RA.Contract.Global.Object.SOFilterPolicy sOFilterPolicy = new Contract.Global.Object.SOFilterPolicy();
            if (policy.BeginTime != null)
            {
                sOFilterPolicy.BeginTime = new Contract.Global.Object.DisplayDateTime()
                {
                    IsDayLightSaving = policy.BeginTime.IsDayLightSaving,
                    StartTime = policy.BeginTime.StartTime,
                    TimeZoneId = policy.BeginTime.TimeZoneId
                };
            }
            sOFilterPolicy.Condition = (int)policy.Condition;
            if (policy.EndTime != null)
            {
                sOFilterPolicy.EndTime = new Contract.Global.Object.DisplayDateTime()
                {
                    IsDayLightSaving = policy.EndTime.IsDayLightSaving,
                    StartTime = policy.EndTime.StartTime,
                    TimeZoneId = policy.EndTime.TimeZoneId
                };
            }
            sOFilterPolicy.IsAnd = policy.IsAnd;
            sOFilterPolicy.Level = (int)policy.Level;
            sOFilterPolicy.Result = policy.Result;
            sOFilterPolicy.ColumnName = policy.Rule.Value1;
            sOFilterPolicy.Rule = GetFilterRuleType(policy.Rule, policy.Level).ToString();
            sOFilterPolicy.RuleType = (int)policy.RuleType;
            sOFilterPolicy.SequenceNo = policy.SequenceNo;
            if (policy.Value != null)
            {
                sOFilterPolicy.Value = new Contract.Global.Object.PolicyValue()
                {
                    Extension = policy.Value.Extension != null ? new Contract.Global.Object.Extention()
                    {
                        isDST = policy.Value.Extension.isDST,
                        TimeZoneId = policy.Value.Extension.TimeZoneId
                    } : null,
                    Value1 = policy.Value.Value1,
                    Value1Unit = (AvePoint.RA.Contract.Global.Object.PolicyValueUnit)policy.Value.Value1Unit,
                    Value2 = policy.Value.Value2,
                    Value2Unit = (AvePoint.RA.Contract.Global.Object.PolicyValueUnit)policy.Value.Value2Unit
                };
            }
            return sOFilterPolicy;
        }

        private static ArchiverFilterRuleType GetFilterRuleType(AvePoint.GCommon.Contract.CommonFilter.PolicyRuleBase ruleBase, AvePoint.GCommon.Contract.CommonFilter.PolicyLevel level)
        {
            //#region remove 

            //switch (RuleBase.Value1)
            //{
            //    case "Name":
            //        return ArchiverFilterRuleType.Name;
            //    case "Size":
            //        return ArchiverFilterRuleType.Size;
            //    case "Document Size":
            //        return ArchiverFilterRuleType.DocumentSize;
            //    case "Site Collection Size Trigger":
            //        return ArchiverFilterRuleType.SiteCollectionSizeTrigger;
            //    case "Modified Time":
            //        return ArchiverFilterRuleType.ModifiedTime;
            //    case "Created Time":
            //        return ArchiverFilterRuleType.CreatedTime;
            //    case "Modified by":
            //        return ArchiverFilterRuleType.ModifiedBy;
            //    case "Created by":
            //        return ArchiverFilterRuleType.CreatedBy;
            //    case "Content Type":
            //        return ArchiverFilterRuleType.ContentType;
            //    case "Column(Text)":
            //        return ArchiverFilterRuleType.TextColumn;
            //    case "Column(Number)":
            //        return ArchiverFilterRuleType.NumberColumn;
            //    case "Column(Yes/No)":
            //        return ArchiverFilterRuleType.BooleanColumn;
            //    case "Column(Date and Time)":
            //        return ArchiverFilterRuleType.DateTimeColumn;
            //    case "Parent List Type ID":
            //        return ArchiverFilterRuleType.ParentListTypeID;
            //    case "Title":
            //        return ArchiverFilterRuleType.Title;
            //    case "Keep the Latest Version":
            //        return ArchiverFilterRuleType.KeepTheLatestVersion;
            //    case "URL":
            //        return ArchiverFilterRuleType.URL;

            //    case "Custom Property(Text)":
            //        return ArchiverFilterRuleType.TextCustomProperty;
            //    case "Custom Property(Number)":
            //        return ArchiverFilterRuleType.NumberCustomProperty;
            //    case "Custom Property(Yse/No)":
            //        return ArchiverFilterRuleType.BooleanCustomProperty;
            //    case "Custom Property(Date and Time)":
            //        return ArchiverFilterRuleType.DateTimeCustomProperty;
            //    default:
            //        throw new NotSupportedException();
            //}

            //#endregion


            if (ruleBase is NameRule)
            {
                return ArchiverFilterRuleType.Name;
            }
            else if (ruleBase is SizeRule)
            {
                if (ruleBase.Value1.Equals("Document Size", StringComparison.OrdinalIgnoreCase))
                {
                    return ArchiverFilterRuleType.DocumentSize;
                }
                else if (ruleBase.Value1.Equals("Size"))
                {
                    return ArchiverFilterRuleType.Size;
                }
                else // "Site Collection Size Trigger"
                {
                    return ArchiverFilterRuleType.SiteCollectionSizeTrigger;
                }
            }
            else if (ruleBase is TermRule)
            {
                return ArchiverFilterRuleType.Term;
            }
            else if (ruleBase is ModifiedRule)
            {
                return ArchiverFilterRuleType.ModifiedTime;
            }
            else if (ruleBase is CreatedRule)
            {
                return ArchiverFilterRuleType.CreatedTime;
            }
            else if (ruleBase is ModifiedByRule)
            {
                return ArchiverFilterRuleType.ModifiedBy;
            }
            else if (ruleBase is CreatedByRule)
            {
                if (level == PolicyLevel.SiteCollection)
                {
                    return ArchiverFilterRuleType.PrimaryAdministrator;
                }
                return ArchiverFilterRuleType.CreatedBy;
            }
            else if (ruleBase is ContentTypeRule)
            {
                return ArchiverFilterRuleType.ContentType;
            }
            else if (ruleBase is ColumnTextRule)
            {
                return ArchiverFilterRuleType.TextColumn;
            }
            else if (ruleBase is MetadataTextColumnRule)
            {
                return ArchiverFilterRuleType.MetadataTextColumn;
            }
            else if (ruleBase is ColumnNumberRule)
            {
                return ArchiverFilterRuleType.NumberColumn;
            }
            else if (ruleBase is MetadataNumberColumnRule)
            {
                return ArchiverFilterRuleType.MetadataNumberColumn;
            }
            else if (ruleBase is ColumnBooleanRule)
            {
                return ArchiverFilterRuleType.BooleanColumn;
            }
            else if (ruleBase is ColumnDateTimeRule)
            {
                return ArchiverFilterRuleType.DateTimeColumn;
            }
            else if (ruleBase is ListTypeRule)
            {
                return ArchiverFilterRuleType.ParentListTypeID;
            }
            else if (ruleBase is StubLastAccessTimeRule /*|| ruleBase is AccessTimeRule*/)
            {
                return ArchiverFilterRuleType.LastAccessedTime;
            }
            else if (ruleBase is TitleRule)
            {
                return ArchiverFilterRuleType.Title;
            }
            else if (ruleBase is KeepHistoryVersionRule)
            {
                return ArchiverFilterRuleType.KeepTheLatestVersion;
            }
            else if (ruleBase is UrlRule)
            {
                return ArchiverFilterRuleType.URL;
            }
            else if (ruleBase is CustomPropertyTextRule)
            {
                return ArchiverFilterRuleType.TextCustomProperty;
            }
            else if (ruleBase is CustomPropertyNumberRule)
            {
                return ArchiverFilterRuleType.NumberCustomProperty;
            }
            else if (ruleBase is CustomPropertyBooleanRule)
            {
                return ArchiverFilterRuleType.BooleanCustomProperty;
            }
            else if (ruleBase is CustomPropertyDateTimeRule)
            {
                return ArchiverFilterRuleType.DateTimeCustomProperty;
            }
            //else if (ruleBase is PostContentRule)
            //{
            //    return ArchiverFilterRuleType.ConversationContent;
            //}
            //else if (ruleBase is ParticipationRule)
            //{
            //    return ArchiverFilterRuleType.Participant;
            //}
            //else if (ruleBase is PostedByRule)
            //{
            //    return ArchiverFilterRuleType.PostedBy;
            //}
            //else if (ruleBase is RepliedByRule)
            //{
            //    return ArchiverFilterRuleType.RepliedBy;
            //}
            //else if (ruleBase is LikedByRule)
            //{
            //    return ArchiverFilterRuleType.LikedBy;
            //}
            //else if (ruleBase is MentionRule)
            //{
            //    return ArchiverFilterRuleType.MentionedName;
            //}
            //else if (ruleBase is TagRule)
            //{
            //    return ArchiverFilterRuleType.Hashtag;
            //}
            else if (ruleBase is SubjectRule)
            {
                return ArchiverFilterRuleType.Subject;
            }
            else if (ruleBase is AttachmentRule)
            {
                return ArchiverFilterRuleType.AttachmentCount;
            }
            else if (ruleBase is SendDateUTCRule)
            {
                return ArchiverFilterRuleType.SendDateUTC;
            }
            else if (ruleBase is SendFromRule)
            {
                return ArchiverFilterRuleType.SendFrom;
            }
            else if (ruleBase is SendToRule)
            {
                return ArchiverFilterRuleType.SendTo;
            }
            else if (ruleBase is ParentFolderNameRule)
            {
                return ArchiverFilterRuleType.ParentFolderName;
            }
            else if (ruleBase is ParentFolderNameHeirarchicallyRule)
            {
                return ArchiverFilterRuleType.ParentFolderNameHeirarchically;
            }
            else if (ruleBase is OwnerRule)
            {
                return ArchiverFilterRuleType.Owner;
            }
            else if (ruleBase is FileExtensionsRule)
            {
                return ArchiverFilterRuleType.Type;
            }
            else if (ruleBase is FilePathRule)
            {
                return ArchiverFilterRuleType.FilePath;
            }
            else if (ruleBase is RetentionLabelRule)
            {
                return ArchiverFilterRuleType.RetentionLabel;
            }
            else if (ruleBase is ParentListNameRule)
            {
                return ArchiverFilterRuleType.ParentLibraryName;
            }
            else if (ruleBase is StubLastActiveTimeRule)
            {
                return ArchiverFilterRuleType.LastActiveTime;
            }
            else if (ruleBase is SensitivityLabelRule)
            {
                return ArchiverFilterRuleType.SensitivityLabel;
            }
            else if (ruleBase is SensitivityLabelFullNameRule)
            {
                return ArchiverFilterRuleType.SensitivityLabelFullName;
            }
            else if (ruleBase is ParentLibraryTextRule)
            {
                return ArchiverFilterRuleType.ParentLibraryText;
            }
            else if (ruleBase is ParentLibraryNumberRule)
            {
                return ArchiverFilterRuleType.ParentLibraryNumber;
            }
            else if (ruleBase is ParentLibraryBooleanRule)
            {
                return ArchiverFilterRuleType.ParentLibraryBoolean;
            }
            else if (ruleBase is ParentLibraryDateTimeRule)
            {
                return ArchiverFilterRuleType.ParentLibraryDateTime;
            }
            else if (ruleBase is ParentSiteCollectionTextRule)
            {
                return ArchiverFilterRuleType.ParentSiteCollectionText;
            }
            else if (ruleBase is ParentSiteCollectionNumberRule)
            {
                return ArchiverFilterRuleType.ParentSiteCollectionNumber;
            }
            else if (ruleBase is ParentSiteCollectionBooleanRule)
            {
                return ArchiverFilterRuleType.ParentSiteCollectionBoolean;
            }
            else if (ruleBase is ParentSiteCollectionDateTimeRule)
            {
                return ArchiverFilterRuleType.ParentSiteCollectionDateTime;
            }
            else if(ruleBase is PropertyBagBooleanRule)
            {
                return ArchiverFilterRuleType.PropertyBagBoolean;
            }
            else if (ruleBase is PropertyBagDateTimeRule)
            {
                return ArchiverFilterRuleType.PropertyBagDateTime;
            }
            else if (ruleBase is PropertyBagNumberRule)
            {
                return ArchiverFilterRuleType.PropertyBagNumber;
            }
            else if (ruleBase is PropertyBagTextRule)
            {
                return ArchiverFilterRuleType.PropertyBagText;
            }
            else if(ruleBase is OrphanedFolderRule)
            {
                return ArchiverFilterRuleType.OrphanedFolderRule;
            }
            else
            {
                switch (ruleBase)
                {
                    case OrphanedFolderRule:
                        return ArchiverFilterRuleType.OrphanedFolderRule;
                    default:
                        throw new NotSupportedException();
                }
            }

        }

        public static AvePoint.RA.Contract.Global.Object.UserInfo ConvertUserInfo2GlobalDto(AvePoint.GCommon.Contract.StorageOptimization.Object.UserInfo user)
        {
            AvePoint.RA.Contract.Global.Object.UserInfo userInfo = new Contract.Global.Object.UserInfo()
            {
                DisplayName = user.DisplayName,
                Email = user.Email,
                InviteType = (int)user.InviteType,
                UserId = user.UserId,
                UserPrincipalName = user.UserPrincipalName
            };
            return userInfo;
        }

        public static AvePoint.RA.Contract.Global.Object.TagContentInfo ConvertTagContentInfo2GlobalDto(AvePoint.GCommon.Contract.StorageOptimization.Object.TagContentInfo tag)
        {
            AvePoint.RA.Contract.Global.Object.TagContentInfo tagContentInfo = new Contract.Global.Object.TagContentInfo()
            {
                ColumnName = tag.ColumnName,
                DateTime = tag.DateTime,
                Type = (Contract.Global.Object.TagContentInfoType)tag.Type,
                Value = tag.Value
            };
            return tagContentInfo;
        }

        public static AvePoint.RA.Contract.Global.Object.FilterPolicy ConvertFilterPolicy2GlobalDto(AvePoint.GCommon.Contract.CommonFilter.FilterPolicy policy)
        {
            AvePoint.RA.Contract.Global.Object.FilterPolicy filterPolicy = new Contract.Global.Object.FilterPolicy();
            filterPolicy.Condition = (int)policy.Condition;
            filterPolicy.Level = (int)policy.Level;
            filterPolicy.Result = policy.Result;
            filterPolicy.ColumnName = policy.Rule.Value1;
            filterPolicy.Rule = GetFilterRuleType(policy.Rule, policy.Level).ToString();
            filterPolicy.RuleType = (int)policy.RuleType;
            filterPolicy.SequenceNo = policy.SequenceNo;
            if (policy.Value != null)
            {
                filterPolicy.Value = new Contract.Global.Object.PolicyValue()
                {
                    Value1 = policy.Value.Value1,
                    Value1Unit = (AvePoint.RA.Contract.Global.Object.PolicyValueUnit)policy.Value.Value1Unit,
                    Value2 = policy.Value.Value2,
                    Value2Unit = (AvePoint.RA.Contract.Global.Object.PolicyValueUnit)policy.Value.Value2Unit,
                    Extension = policy.Value.Extension != null ? new Contract.Global.Object.Extention()
                    {
                        isDST = policy.Value.Extension.isDST,
                        TimeZoneId = policy.Value.Extension.TimeZoneId
                    } : null
                };
            }
            return filterPolicy;
        }


        public static RMFSTreeNode ConvertSPTree2RMTree(FSTreeNodeDto spTree, RMFSTreeNode rm = null)
        {
            if (rm == null)
            {
                rm = new RMFSTreeNode();
            }
            //rm.Id = spTree.ID;

            rm.Name = spTree.Name;
            rm.FullPath = spTree.FullPath;
            rm.Level = (int)spTree.Level;
            rm.NodeType = (int)spTree.Type;

            rm.Expanded = spTree.Expanded;

            rm.ChildrenCount = spTree.ChildrenCount;
            rm.CheckNumber = spTree.CheckNumber;

            if (spTree.Parent != null && rm.Parent == null)
            {
                RMFSTreeNode tempParent = new RMFSTreeNode();
                tempParent.Children = new List<RMFSTreeNode>() { rm };
                rm.Parent = ConvertFSTree2RMTree(spTree.Parent, tempParent);
            }
            if (spTree.Children != null && spTree.Children.Count > 0 &&
                (rm.Children == null || rm.Children.Count == 0))
            {
                rm.Children = new List<RMFSTreeNode>();
                foreach (FSTreeNodeDto child in spTree.Children)
                {
                    RMFSTreeNode temp = new RMFSTreeNode();
                    temp.Parent = rm;
                    RMFSTreeNode rmChild = ConvertFSTree2RMTree(child, temp);
                    rm.Children.Add(rmChild);
                }
            }
            return rm;
        }

        public static Contract.Object.RMSPTreeNode ConvertRemoteSite2RMTree(RemoteSiteCollection siteCollection, Contract.Object.RMSPTreeNode rm = null)
        {
            if (rm == null)
            {
                rm = new Contract.Object.RMSPTreeNode();
            }
            rm.Id = siteCollection.id;
            rm.Name = siteCollection.url;
            rm.FullPath = siteCollection.url;
            rm.FullUrl = siteCollection.url;
            rm.Level = (int)NodeLevel.SiteCollection;
            rm.NodeType = (int)(siteCollection.NodeType == RemoveNodeType.SiteCollection ? NodeType.SharePointSites : (siteCollection.NodeType == RemoveNodeType.O365GroupSites ? NodeType.O365GroupSites : NodeType.SkyDriveProSites));
            rm.SPObjectId = siteCollection.id;
            rm.SPType = (int)SPType.BPOS;
            rm.FarmId = siteCollection.parentId;
            if (!string.IsNullOrEmpty(siteCollection.SPVersion))
            {
                //logger.Debug(siteCollection.url + " sp version is " + siteCollection.SPVersion);
                if (siteCollection.SPVersion.StartsWith("15."))
                {
                    rm.SPVersion = GConstants.SPVersion.MOSS13;
                }
                else
                {
                    rm.SPVersion = GConstants.SPVersion.MOSS10;
                }
            }
            string domain = siteCollection.domain;
            string username = siteCollection.username;
            GCommon.Contract.CentralAdmin.Object.BPOSMode mode = GCommon.Contract.CentralAdmin.Object.BPOSMode.Undetermined;
            //if (siteCollection.LastModifyTime - DateTime.UtcNow.Ticks > 864000000000)
            DateTime siteCollectionModifyTime = new DateTime(siteCollection.CreateTime);
            if (siteCollectionModifyTime.AddDays(1).Ticks <= DateTime.UtcNow.Ticks)
            {
                mode = GCommon.Contract.CentralAdmin.Object.BPOSMode.Office365;
            }
            rm.BposInfo = new GCommon.Contract.CentralAdmin.Object.BposInfo()
            {
                SiteUrl = siteCollection.url,
                AppType = siteCollection.AppType,
                ConnectionType = siteCollection.AuthType,
                UserAccountInfo = GetBPOSInfo(siteCollection),
                Mode = mode,
            };
            rm.Hidden = false;
            return rm;
        }

        public static RMSPTreeNode ConvertRemoteSite2RMTeamsTree(RemoteSiteCollection siteCollection, RMSPTreeNode rm = null)
        {
            if (rm == null)
            {
                rm = new RMSPTreeNode();
            }
            rm.Id = siteCollection.id;
            rm.Name = siteCollection.url;
            rm.FullPath = siteCollection.url;
            rm.Level = (int)NodeLevel.SiteCollection;
            rm.NodeType = (int)(siteCollection.NodeType == RemoveNodeType.SiteCollection ? NodeType.SharePointSites : (siteCollection.NodeType == RemoveNodeType.O365GroupSites ? NodeType.O365GroupSites : NodeType.SkyDriveProSites));
            rm.SPObjectId = siteCollection.id;
            rm.SPType = (int)SPType.BPOS;
            rm.FarmId = siteCollection.parentId;
            if (!string.IsNullOrEmpty(siteCollection.SPVersion))
            {
                //logger.Debug(siteCollection.url + " sp version is " + siteCollection.SPVersion);
                if (siteCollection.SPVersion.StartsWith("15."))
                {
                    rm.SPVersion = GConstants.SPVersion.MOSS13;
                }
                else
                {
                    rm.SPVersion = GConstants.SPVersion.MOSS10;
                }
            }
            rm.Hidden = false;
            return rm;
        }

        public static Contract.Object.RMSPTreeNode ConvertRemoteWebApplication2RMTeamsTree(RemoteWebApplication webApp, Contract.Object.RMSPTreeNode rm = null)
        {
            if (rm == null)
            {
                rm = new Contract.Object.RMSPTreeNode();
            }
            rm.Id = webApp.id;
            rm.Name = webApp.url;
            rm.FullPath = webApp.url;
            rm.Level = (int)NodeLevel.WebApplication;
            rm.NodeType = (int)NodeType.O365GroupSitesGroup;
            rm.SPObjectId = webApp.id;
            rm.SPType = (int)SPType.Moss;
            rm.FarmId = webApp.id;
            rm.Hidden = false;
            rm.DisplayName = webApp.url;
            return rm;
        }

        public static Contract.Object.RMSPTreeNode ConvertRemoteWebApplication2RMTree(RemoteWebApplication webApp, Contract.Object.RMSPTreeNode rm = null)
        {
            if (rm == null)
            {
                rm = new Contract.Object.RMSPTreeNode();
            }
            rm.Id = webApp.id;
            rm.Name = webApp.url;
            rm.FullPath = webApp.url;
            rm.Level = (int)NodeLevel.WebApplication;
            rm.NodeType = (int)NodeType.SharePointSites;
            rm.SPObjectId = webApp.id;
            rm.SPType = (int)SPType.BPOS;
            rm.FarmId = webApp.id;
            rm.Hidden = false;
            return rm;
        }

        private static GCommon.Contract.CentralAdmin.Object.BposUserAccountInfo GetBPOSInfo(RemoteSiteCollection site, bool isCacheEnabled = false)
        {
            GCommon.Contract.CentralAdmin.Object.BposUserAccountInfo accountInfo = null;
            if (site == null)
            {
                ArgumentCheck.NotNull(site, nameof(site));
                throw new Exception(string.Format("Get AveBPOSAccountInfo Failed, Site fullPath: {0}.", site.url));
            }
            FipsModeUtil.InitControlCryptoMode();
            CspCommunicationWrapper.CommunicationEncryptionKey = CspCommunicationWrapper.staticCommunicationEncryptionKey;

            if (site.AuthType == GCommon.Contract.CentralAdmin.Object.BposConnectionType.ServiceAccount || !string.IsNullOrEmpty(site.password))
            {

                string domain = ".".Equals(site.domain) ? string.Empty : site.domain;
                string username = site.username;
                string password = RMAosApiClient.GetServiceAccountPassword(TenantLocalValue.LogonGroupId, username);
                accountInfo = new GCommon.Contract.CentralAdmin.Object.BposUserAccountInfo()
                {
                    Domain = domain,
                    Username = username,
                    Password = password,
                    AdminUrl = site.AdminUrl,
                    TenantId = TenantLocalValue.LogonGroupId
                };
            }
            else
            {

                var tenantId = site.TenantId;
                var profile = RMAosApiClient.GetSPOnlineProfile(TenantLocalValue.LogonGroupId, tenantId, isCacheEnabled);
                if (profile == null)
                {
                    throw new Exception("RM_APP_AppProfileNotAvailable");
                }

                var clientId = profile.AppClientId;
                //var cerContent = profile.AppCertContent;
                var cerSercert = profile.AppCertSecret;
                //var certificateBytes = Convert.FromBase64String(cerContent);
                //string secret = CryptoUtil.ConvertBytesToString(CspCrossPlatformExchangeWrapper.UnWrapKey(cerSercert));
                //X509Certificate2 apponlyCertificate = new X509Certificate2(
                //    certificateBytes,
                //    secret,
                //    X509KeyStorageFlags.Exportable |
                //    X509KeyStorageFlags.MachineKeySet |
                //    X509KeyStorageFlags.PersistKeySet);
                accountInfo = new GCommon.Contract.CentralAdmin.Object.BposUserAccountInfo()
                {
                    TenantId = tenantId,
                    AdminUrl = site.AdminUrl,
                    AppClientId = clientId,
                    //AppCertContent = cerContent,
                    AppCertSecret = cerSercert
                };
            }

            return accountInfo;
        }


        public static SPTreeNodeDto ConvertRMTree2SPTree(Contract.Object.RMSPTreeNode rmTree, SPTreeNodeDto sp = null)
        {
            if(rmTree == null)
            {
                return null;
            }
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
            sp.FullUrl = rmTree.FullUrl;
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
            sp.TeamName = rmTree.TeamName;
            sp.TeamsId = rmTree.TeamsId;
            sp.PredictionModeType = (int)rmTree.PredictionModeType;
            if (sp.NodeExtension == null)
            {
                sp.NodeExtension = new NodeExtensionDto();
            }
            sp.NodeExtension.BposInfo = rmTree.BposInfo;
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
                foreach (Contract.Object.RMSPTreeNode child in rmTree.Children)
                {
                    SPTreeNodeDto tempChild = new SPTreeNodeDto();
                    tempChild.Parent = sp;
                    sp.Children.Add(ConvertRMTree2SPTree(child, tempChild));
                }
            }
            return sp;
        }

        public static GoogleDriveTreeNodeDto ConvertRMGoogleTreeNode2Dto(RMGoogleTreeNode rmNode, GoogleDriveTreeNodeDto nodeDto = null)
        {
            if (rmNode == null)
            {
                return null;
            }
            if (nodeDto == null)
            {
                nodeDto = new GoogleDriveTreeNodeDto();
            }
            nodeDto.ID = rmNode.Id;
            nodeDto.ContainerId = rmNode.ContainerId;
            if (rmNode.Level is (int)NodeLevel.GoogleMyDriveContainer or (int)NodeLevel.GoogleSharedDriveContainer)
            {
                nodeDto.NodeId = rmNode.ObjectId;
            }
            nodeDto.Name = rmNode.Name;
            nodeDto.Title = rmNode.Title;
            nodeDto.FullPath = rmNode.FullPath;
            nodeDto.Level = (NodeLevel)rmNode.Level;
            nodeDto.Expanded = rmNode.Expanded;
            nodeDto.ChildrenCount = rmNode.ChildrenCount;
            nodeDto.CheckNumber = rmNode.CheckNumber; 
            nodeDto.ObjectId = rmNode.ObjectId;
            nodeDto.TenantId = rmNode.GoogleTenantId;
            if (nodeDto.NodeExtension == null)
            {
                nodeDto.NodeExtension = new NodeExtensionDto();
            }
            nodeDto.Parent = ConvertRMGoogleTreeNode2Dto(rmNode.Parent);
            nodeDto.ParentId = rmNode.ParentId;
            nodeDto.Children = rmNode.Children?.ConvertAll(x => ConvertRMGoogleTreeNode2Dto(x));
            nodeDto.ChildrenCount = rmNode.ChildrenCount;
            return nodeDto;
        }
        
        public static RMGoogleTreeNode ConvertRMGoogleDto2TreeNode(GoogleDriveTreeNodeDto rmNode, RMGoogleTreeNode nodeDto = null)
        {
            if (rmNode == null)
            {
                return null;
            }
            if (nodeDto == null)
            {
                nodeDto = new RMGoogleTreeNode();
            }
            nodeDto.Id = rmNode.ID;
            nodeDto.ContainerId = rmNode.ContainerId;
            nodeDto.Name = rmNode.Name;
            nodeDto.Title = rmNode.Title;
            nodeDto.FullPath = rmNode.FullPath;
            nodeDto.Level = (int) rmNode.Level;
            nodeDto.Expanded = rmNode.Expanded;
            nodeDto.ChildrenCount = rmNode.ChildrenCount;
            nodeDto.CheckNumber = rmNode.CheckNumber; 
            nodeDto.ObjectId = rmNode.ObjectId;
            nodeDto.Parent = ConvertRMGoogleDto2TreeNode(rmNode.Parent);
            nodeDto.ParentId = rmNode.ParentId;
            nodeDto.Children = rmNode.Children?.ConvertAll(x => ConvertRMGoogleDto2TreeNode(x));
            nodeDto.ChildrenCount = rmNode.ChildrenCount;
            return nodeDto;
        }

        public static void ConvertSPTreeBeforeToJSON(SPTreeNodeDto currentNode)
        {
            RemoveParentChildrenNodes(currentNode);
            RemoveChildrenParentNodes(currentNode);
        }
        private static SPTreeNodeDto RemoveParentChildrenNodes(SPTreeNodeDto currentNode)
        {
            if (currentNode.Parent != null)
            {
                currentNode.Parent.ChildrenCount = 0;
                currentNode.Parent.Children = new List<SPTreeNodeDto>();
                return RemoveParentChildrenNodes(currentNode.Parent);
            }
            else
            {
                return null;
            }
        }

        private static SPTreeNodeDto RemoveChildrenParentNodes(SPTreeNodeDto currentNode)
        {
            if (currentNode.Children != null && currentNode.Children.Count > 0)
            {
                foreach (var c in currentNode.Children)
                {
                    c.Parent = null;
                    RemoveChildrenParentNodes(c);
                }
            }
            return null;
        }

        public static RMSPSampleTreeNode ConvertSPTree2RMSampleTree(SPTreeNodeDto spTree, RMSPSampleTreeNode rm = null)
        {
            if (rm == null)
            {
                rm = new RMSPSampleTreeNode();
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
            rm.Hidden = spTree.Hidden;
            rm.TeamName = spTree.TeamName;
            rm.TeamsId = spTree.TeamsId;
            if (rm.Name == "{System Folder}")
            {
                rm.Hidden = true;
            }
            rm.ChildrenCount = spTree.ChildrenCount;
            rm.CheckNumber = spTree.CheckNumber;
            rm.TemplateId = spTree.Template;
            if (spTree.NodeExtension != null && spTree.NodeExtension.BposInfo != null)
            {
                rm.BposInfo = spTree.NodeExtension.BposInfo;
            }
            rm.ParentId = spTree.ParentId;
            if (spTree.Parent != null && rm.Parent == null)
            {
                RMSPSampleTreeNode tempParent = new RMSPSampleTreeNode();
                tempParent.Children = new List<RMSPSampleTreeNode>() { rm };
                rm.Parent = ConvertSPTree2RMSampleTree(spTree.Parent, tempParent);
            }
            //if (spTree.Children != null && spTree.Children.Count > 0 && (rm.Children == null || rm.Children.Count == 0))
            //{
            //    rm.Children = new List<RMSPSampleTreeNode>();
            //    foreach (SPTreeNodeDto child in spTree.Children)
            //    {
            //        RMSPSampleTreeNode temp = new RMSPSampleTreeNode();
            //        temp.Parent = rm;
            //        RMSPSampleTreeNode rmChild = ConvertSPTree2RMSampleTree(child, temp);
            //        rm.Children.Add(rmChild);
            //    }
            //}
            return rm;
        }

        public static SPTreeNodeDto ConvertRMSampleTree2SPTree(RMSPSampleTreeNode rmTree, SPTreeNodeDto sp = null)
        {
            if(rmTree == null)
            {
                return null;
            }
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
            sp.IsSOMode = rmTree.IsArchiverTree;
            sp.TeamsId = string.IsNullOrEmpty(rmTree.TeamsId) ? rmTree.Parent?.TeamsId : rmTree.TeamsId;
            if (sp.NodeExtension == null)
            {
                sp.NodeExtension = new NodeExtensionDto();
            }
            sp.NodeExtension.BposInfo = rmTree.BposInfo;
            if (rmTree.Parent != null && sp.Parent == null)
            {
                SPTreeNodeDto tempParent = new SPTreeNodeDto();
                tempParent.Children = new List<SPTreeNodeDto> { sp };
                sp.Parent = ConvertRMSampleTree2SPTree(rmTree.Parent, tempParent);
            }
            if (rmTree.Children != null && rmTree.Children.Count > 0 &&
                (sp.Children == null || sp.Children.Count == 0))
            {
                sp.Children = new List<SPTreeNodeDto>();
                foreach (RMSPSampleTreeNode child in rmTree.Children)
                {
                    SPTreeNodeDto tempChild = new SPTreeNodeDto();
                    tempChild.Parent = sp;
                    sp.Children.Add(ConvertRMSampleTree2SPTree(child, tempChild));
                }
            }
            return sp;
        }


        public static ExchangeOnlineTreeNodeDto ConvertRMSampleExchangeTree2TreeNodeDto(RMSampleEXOTreeNode rmTree, ExchangeOnlineTreeNodeDto daTreeNode = null)
        {
            if (daTreeNode == null)
            {
                daTreeNode = new ExchangeOnlineTreeNodeDto();
            }
            daTreeNode.ID = rmTree.Id;
            //daTreeNode.FarmID = rmTree.FarmId;
            //daTreeNode.FarmName = rmTree.FarmName;
            daTreeNode.Name = rmTree.Name;
            daTreeNode.Title = rmTree.Title;
            daTreeNode.FullPath = rmTree.FullPath;
            daTreeNode.Level = (NodeLevel)rmTree.Level;
            daTreeNode.Type = (NodeType)rmTree.NodeType;
            daTreeNode.Category = rmTree.Category;
            daTreeNode.DisplayName = rmTree.DisplayName;
            daTreeNode.DisplayTo = rmTree.DisplayTo;
            daTreeNode.Expanded = rmTree.Expanded;
            daTreeNode.ChildrenCount = rmTree.ChildrenCount;
            daTreeNode.CheckNumber = rmTree.CheckNumber;
            daTreeNode.EmailAddress = rmTree.Email;
            daTreeNode.HasAttachment = rmTree.HasAttachment;
            daTreeNode.MailboxType = rmTree.MailboxType;
            if (daTreeNode.NodeExtension == null)
            {
                daTreeNode.NodeExtension = new NodeExtensionDto();
            }

            if (rmTree.Parent != null && daTreeNode.Parent == null)
            {
                ExchangeOnlineTreeNodeDto tempParent = new ExchangeOnlineTreeNodeDto();
                tempParent.Children = new List<ExchangeOnlineTreeNodeDto> { daTreeNode };
                daTreeNode.Parent = ConvertRMSampleExchangeTree2TreeNodeDto(rmTree.Parent, tempParent);
            }
            if (rmTree.Children != null && rmTree.Children.Count > 0 &&
                (daTreeNode.Children == null || daTreeNode.Children.Count == 0))
            {
                daTreeNode.Children = new List<ExchangeOnlineTreeNodeDto>();
                foreach (RMSampleEXOTreeNode child in rmTree.Children)
                {
                    ExchangeOnlineTreeNodeDto tempChild = new ExchangeOnlineTreeNodeDto();
                    tempChild.Parent = daTreeNode;
                    daTreeNode.Children.Add(ConvertRMSampleExchangeTree2TreeNodeDto(child, tempChild));
                }
            }
            return daTreeNode;
        }


        public static RMSampleEXOTreeNode ConvertTreeNodeDto2RMSampleExchangeTree(ExchangeOnlineTreeNodeDto daTree, RMSampleEXOTreeNode rmTree = null)
        {
            if (rmTree == null)
            {
                rmTree = new RMSampleEXOTreeNode();
            }
            rmTree.Id = daTree.ID;
            //daTreeNode.FarmID = rmTree.FarmId;
            //daTreeNode.FarmName = rmTree.FarmName;
            rmTree.Name = daTree.Name;
            rmTree.Title = daTree.Title;
            rmTree.FullPath = daTree.FullPath;
            rmTree.Level = (int)daTree.Level;
            rmTree.NodeType = (int)daTree.Type;
            rmTree.Category = daTree.Category;
            rmTree.DisplayName = daTree.DisplayName;
            rmTree.DisplayTo = daTree.DisplayTo;
            rmTree.Expanded = daTree.Expanded;
            rmTree.ChildrenCount = daTree.ChildrenCount;
            rmTree.CheckNumber = daTree.CheckNumber;
            rmTree.Email = daTree.EmailAddress;
            rmTree.HasAttachment = daTree.HasAttachment;
            rmTree.MailboxType = daTree.MailboxType;

            if (daTree.Parent != null)
            {
                RMSampleEXOTreeNode tempParent = new RMSampleEXOTreeNode();
                tempParent.Children = new List<RMSampleEXOTreeNode> { rmTree };
                rmTree.Parent = ConvertTreeNodeDto2RMSampleExchangeTree(daTree.Parent, tempParent);
            }

            return rmTree;
        }

        public static RMSampleGoogleTreeNode ConvertTreeNodeDto2RMGoogleDriveTree(GoogleDriveTreeNodeDto daTree, RMSampleGoogleTreeNode rmSampleTree = null)
        {
            if (rmSampleTree == null)
            {
                rmSampleTree = new ();
            }
            rmSampleTree.Id = daTree.ID;
            rmSampleTree.Name = daTree.Name;
            rmSampleTree.Title = daTree.Title;
            rmSampleTree.FullPath = daTree.FullPath;
            rmSampleTree.Level = (int)daTree.Level;
            rmSampleTree.NodeType = (int)daTree.Type;
            rmSampleTree.DisplayName = daTree.DisplayName;
            rmSampleTree.Expanded = daTree.Expanded;
            rmSampleTree.ChildrenCount = daTree.ChildrenCount;
            rmSampleTree.CheckNumber = daTree.CheckNumber;
            rmSampleTree.ContainerId = daTree.ContainerId;
            rmSampleTree.NodeId = daTree.NodeId;
            rmSampleTree.ObjectId = daTree.ObjectId;
            rmSampleTree.GoogleTenantId = daTree.TenantId;
            rmSampleTree.ParentId = daTree.ParentId;
            if (daTree.Parent != null && daTree.Parent == null)
            {
                RMSampleGoogleTreeNode tempParent = new();
                tempParent.Children = [rmSampleTree];
                rmSampleTree.Parent = ConvertTreeNodeDto2RMGoogleDriveTree(daTree.Parent, tempParent);
            }

            return rmSampleTree;
        }

        public static GoogleDriveTreeNodeDto ConvertSampleTree2Dto(RMSampleGoogleTreeNode node)
        {
            return new GoogleDriveTreeNodeDto()
            {
                ID = node.Id,
                Name = node.Name ?? "",
                Title = node.Title ?? "",
                FullPath = node.FullPath ?? node.Name ?? node.DisplayName ?? "",
                Level = (NodeLevel)node.Level,
                DisplayName = node.DisplayName,
                Expanded = node.Expanded,
                ChildrenCount = node.ChildrenCount,
                CheckNumber = node.CheckNumber,
                Parent = node.Parent != null ? ConvertSampleTree2Dto(node.Parent) : null,
                Children = node.Children?.ConvertAll(x => ConvertSampleTree2Dto(x)),
                ParentId = node.ParentId,
                NodeId = node.NodeId,
                ContainerId = node.ContainerId,
                ObjectId = node.ObjectId
            };     
        }

        public static GoogleDriveTreeNodeDto ConvertGoogleRM2Dto(RMGoogleTreeNode node)
        {
            return new()
            {
                ID = node.Id,
                Name = node.Name ?? "",
                Title = node.Title ?? "",
                FullPath = node.FullPath ?? "",
                Level = (NodeLevel)node.Level,
                DisplayName = node.DisplayName,
                Expanded = node.Expanded,
                ChildrenCount = node.ChildrenCount,
                CheckNumber = node.CheckNumber,
                Parent = node.Parent != null ? ConvertGoogleRM2Dto(node.Parent) : null,
                Children = node.Children?.ConvertAll(x => ConvertGoogleRM2Dto(x)),
                ParentId = node.ParentId,
                NodeId = node.DriveId,
                ContainerId = node.ContainerId,
                ObjectId = node.ObjectId,
                TenantId = node.GoogleTenantId
            };
        }

        public static ExchangeOnlineTreeNodeDto ConvertRMSPTree2EXOTreeNodeDto(RMSPTreeNode rmTree, ExchangeOnlineTreeNodeDto daTreeNode = null)
        {
            if (daTreeNode == null)
            {
                daTreeNode = new ExchangeOnlineTreeNodeDto();
            }
            daTreeNode.ID = rmTree.Id;
            daTreeNode.ObjectId = rmTree.SPObjectId;
            //daTreeNode.FarmID = rmTree.FarmId;
            //daTreeNode.FarmName = rmTree.FarmName;
            daTreeNode.Name = rmTree.Name;
            daTreeNode.Title = rmTree.Title;
            daTreeNode.FullPath = rmTree.FullPath;
            daTreeNode.Level = (NodeLevel)rmTree.Level;
            daTreeNode.Type = (NodeType)rmTree.NodeType;
            //daTreeNode.Category = rmTree.Category;
            daTreeNode.DisplayName = rmTree.DisplayName;
            //daTreeNode.DisplayTo = rmTree.DisplayTo;
            daTreeNode.Expanded = rmTree.Expanded;
            daTreeNode.ChildrenCount = rmTree.ChildrenCount;
            daTreeNode.CheckNumber = rmTree.CheckNumber;
            daTreeNode.EmailAddress = rmTree.Name;
            //daTreeNode.HasAttachment = rmTree.HasAttachment;
            daTreeNode.MailboxType = ConvertToMailboxType(daTreeNode.Type);
            daTreeNode.IsNullClassificationSetting = rmTree.IsNullClassificationSetting;
            daTreeNode.SkipRemoveContentAndDestroyAction = rmTree.SkipRemoveContentAndDestroyAction;
            daTreeNode.O365TenantId = rmTree.O365TenantId;
            if (daTreeNode.NodeExtension == null)
            {
                daTreeNode.NodeExtension = new NodeExtensionDto();
            }

            if (rmTree.Parent != null && daTreeNode.Parent == null)
            {
                ExchangeOnlineTreeNodeDto tempParent = new ExchangeOnlineTreeNodeDto();
                tempParent.Children = new List<ExchangeOnlineTreeNodeDto> { daTreeNode };
                daTreeNode.Parent = ConvertRMSPTree2EXOTreeNodeDto(rmTree.Parent, tempParent);
            }
            if (rmTree.Children != null && rmTree.Children.Count > 0 &&
                (daTreeNode.Children == null || daTreeNode.Children.Count == 0))
            {
                daTreeNode.Children = new List<ExchangeOnlineTreeNodeDto>();
                foreach (RMSPTreeNode child in rmTree.Children)
                {
                    ExchangeOnlineTreeNodeDto tempChild = new ExchangeOnlineTreeNodeDto();
                    tempChild.Parent = daTreeNode;
                    daTreeNode.Children.Add(ConvertRMSPTree2EXOTreeNodeDto(child, tempChild));
                }
            }

            return daTreeNode;

            MailboxType ConvertToMailboxType(NodeType nodeType)
            {
                return nodeType switch
                {
                    NodeType.O365GroupSites => MailboxType.Group,
                    NodeType.O365TeamSites or NodeType.EOMailBox => MailboxType.Teams,
                    _ => MailboxType.None,
                };
            }
        }

        public static ExchangeOnlineTreeNodeDto ConvertRMExchangeTree2TreeNodeDto(RMEXOTreeNode rmTree, ExchangeOnlineTreeNodeDto daTreeNode = null)
        {
            if (daTreeNode == null)
            {
                daTreeNode = new ExchangeOnlineTreeNodeDto();
            }
            daTreeNode.ID = rmTree.Id;
            //daTreeNode.FarmID = rmTree.FarmId;
            //daTreeNode.FarmName = rmTree.FarmName;
            daTreeNode.Name = rmTree.Name;
            daTreeNode.Title = rmTree.Title;
            daTreeNode.FullPath = rmTree.FullPath;
            daTreeNode.Level = (NodeLevel)rmTree.Level;
            daTreeNode.Type = (NodeType)rmTree.NodeType;
            daTreeNode.Category = rmTree.Category;
            daTreeNode.DisplayName = rmTree.DisplayName;
            daTreeNode.DisplayTo = rmTree.DisplayTo;
            daTreeNode.Expanded = rmTree.Expanded;
            daTreeNode.ChildrenCount = rmTree.ChildrenCount;
            daTreeNode.CheckNumber = rmTree.CheckNumber;
            daTreeNode.EmailAddress = rmTree.Email;
            daTreeNode.HasAttachment = rmTree.HasAttachment;
            daTreeNode.MailboxType = rmTree.MailboxType;
            daTreeNode.IsNullClassificationSetting = rmTree.IsNullClassificationSetting;
            daTreeNode.SkipRemoveContentAndDestroyAction = rmTree.SkipRemoveContentAndDestroyAction;
            if (daTreeNode.NodeExtension == null)
            {
                daTreeNode.NodeExtension = new NodeExtensionDto();
            }

            if (rmTree.Parent != null && daTreeNode.Parent == null)
            {
                ExchangeOnlineTreeNodeDto tempParent = new ExchangeOnlineTreeNodeDto();
                tempParent.Children = new List<ExchangeOnlineTreeNodeDto> { daTreeNode };
                daTreeNode.Parent = ConvertRMExchangeTree2TreeNodeDto(rmTree.Parent, tempParent);
            }
            if (rmTree.Children != null && rmTree.Children.Count > 0 &&
                (daTreeNode.Children == null || daTreeNode.Children.Count == 0))
            {
                daTreeNode.Children = new List<ExchangeOnlineTreeNodeDto>();
                foreach (RMEXOTreeNode child in rmTree.Children)
                {
                    ExchangeOnlineTreeNodeDto tempChild = new ExchangeOnlineTreeNodeDto();
                    tempChild.Parent = daTreeNode;
                    daTreeNode.Children.Add(ConvertRMExchangeTree2TreeNodeDto(child, tempChild));
                }
            }
            return daTreeNode;
        }


        public static RMEXOTreeNode ConvertTreeNodeDto2RMExchangeTree(ExchangeOnlineTreeNodeDto daTree, RMEXOTreeNode rmTree = null)
        {
            if (rmTree == null)
            {
                rmTree = new RMEXOTreeNode();
            }
            rmTree.Id = daTree.ID;

            rmTree.Name = daTree.Name;
            rmTree.Title = daTree.Title;
            rmTree.FullPath = daTree.FullPath;
            rmTree.Level = (int)daTree.Level;
            rmTree.NodeType = (int)daTree.Type;
            rmTree.Category = daTree.Category;
            rmTree.DisplayName = daTree.DisplayName;
            rmTree.DisplayTo = daTree.DisplayTo;
            rmTree.Expanded = daTree.Expanded;
            rmTree.ChildrenCount = daTree.ChildrenCount;
            rmTree.CheckNumber = daTree.CheckNumber;
            rmTree.Email = daTree.EmailAddress;
            rmTree.HasAttachment = daTree.HasAttachment;
            rmTree.MailboxType = daTree.MailboxType;
            rmTree.O365TenantId = daTree.O365TenantId;

            if (daTree.Parent != null && rmTree.Parent == null)
            {
                RMEXOTreeNode tempParent = new RMEXOTreeNode();
                tempParent.Children = new List<RMEXOTreeNode> { rmTree };
                rmTree.Parent = ConvertTreeNodeDto2RMExchangeTree(daTree.Parent, tempParent);
            }

            return rmTree;
        }

        public static void ConvertEXOTreeBeforeToJSON(ExchangeOnlineTreeNodeDto currentNode)
        {
            RemoveParentChildrenNodes(currentNode);
            RemoveChildrenParentNodes(currentNode);
        }
        private static ExchangeOnlineTreeNodeDto RemoveParentChildrenNodes(ExchangeOnlineTreeNodeDto currentNode)
        {
            if (currentNode.Parent != null)
            {
                currentNode.Parent.ChildrenCount = 0;
                currentNode.Parent.Children = new List<ExchangeOnlineTreeNodeDto>();
                return RemoveParentChildrenNodes(currentNode.Parent);
            }
            else
            {
                return null;
            }
        }

        private static ExchangeOnlineTreeNodeDto RemoveChildrenParentNodes(ExchangeOnlineTreeNodeDto currentNode)
        {
            if (currentNode.Children != null && currentNode.Children.Count > 0)
            {
                foreach (var c in currentNode.Children)
                {
                    c.Parent = null;
                    RemoveChildrenParentNodes(c);
                }
            }
            return null;
        }
        #region fs TreeNode convert.
        public static RMFSTreeNode ConvertFSTree2RMTree(FSTreeNodeDto fsTree, RMFSTreeNode rm = null)
        {
            if (rm == null)
            {
                rm = new RMFSTreeNode();
            }
            if (fsTree.Level == NodeLevel.FSFolder)
            {
                rm.Id = new Guid(HashCodeHelper.ToMD5HashCode(fsTree.FullPath.ToLowerInvariant()));
            }
            else
            {
                if (!string.IsNullOrEmpty(fsTree.ID))
                {
                    rm.Id = new Guid(fsTree.ID);
                }
            }
            rm.FarmID = fsTree.FarmID;
            rm.Name = fsTree.Name;
            rm.FullPath = fsTree.FullPath;
            rm.Level = (int)fsTree.Level;
            rm.NodeType = (int)fsTree.Type;
            rm.Expanded = fsTree.Expanded;
            rm.ChildrenCount = fsTree.ChildrenCount;
            rm.CheckNumber = fsTree.CheckNumber;

            rm.Domain = fsTree.Domain;
            rm.Username = fsTree.Username;
            rm.EncryptedPassword = fsTree.EncryptedPassword;

            if (fsTree.NodeExtension != null && fsTree.NodeExtension.BposInfo != null)
            {
            }
            if (fsTree.Parent != null && rm.Parent == null)
            {
                RMFSTreeNode tempParent = new RMFSTreeNode();
                tempParent.Children = new List<RMFSTreeNode>() { rm };
                rm.Parent = ConvertFSTree2RMTree(fsTree.Parent, tempParent);
            }
            if (fsTree.Children != null && fsTree.Children.Count > 0 &&
                (rm.Children == null || rm.Children.Count == 0))
            {
                rm.Children = new List<RMFSTreeNode>();
                foreach (FSTreeNodeDto child in fsTree.Children)
                {
                    RMFSTreeNode temp = new RMFSTreeNode();
                    temp.Parent = rm;
                    RMFSTreeNode rmChild = ConvertFSTree2RMTree(child, temp);
                    rm.Children.Add(rmChild);
                }
            }
            return rm;
        }
        public static AvePoint.RA.Contract.Global.Object.FSTreeNodeDto ConvertRMTree2FSTree4AGent(RMFSTreeNode rmTree, AvePoint.RA.Contract.Global.Object.FSTreeNodeDto fs = null, bool needDecryptPath = false)
        {
            if (fs == null)
            {
                fs = new AvePoint.RA.Contract.Global.Object.FSTreeNodeDto();
            }
            fs.Id = rmTree.Id;
            fs.FarmID = rmTree.FarmID;
            fs.Name = rmTree.Name;
            fs.FullPath = needDecryptPath ? EncodeUtil.DecryptByCommunicationKey(rmTree.FullPath) : rmTree.FullPath;
            fs.Level = rmTree.Level;
            fs.NodeType = rmTree.NodeType;
            fs.Expanded = rmTree.Expanded;
            fs.ChildrenCount = rmTree.ChildrenCount;
            fs.CheckNumber = rmTree.CheckNumber;

            fs.Domain = rmTree.Domain;
            fs.Username = rmTree.Username;
            fs.EncryptedPassword = rmTree.EncryptedPassword;

            //sp.NodeExtension.BposInfo = rmTree.BposInfo;
            if (rmTree.Parent != null && fs.Parent == null)
            {
                AvePoint.RA.Contract.Global.Object.FSTreeNodeDto tempParent = new AvePoint.RA.Contract.Global.Object.FSTreeNodeDto();
                tempParent.Children = new List<AvePoint.RA.Contract.Global.Object.FSTreeNodeDto> { fs };
                fs.Parent = ConvertRMTree2FSTree4AGent(rmTree.Parent, tempParent, needDecryptPath);
                fs.ParentId = rmTree.Parent.Id.ToString();
            }
            if (rmTree.CheckNumber == 1)
            {
                return fs;
            }
            if (rmTree.Children != null && rmTree.Children.Count > 0 &&
                (fs.Children == null || fs.Children.Count == 0))
            {
                fs.Children = new List<AvePoint.RA.Contract.Global.Object.FSTreeNodeDto>();
                foreach (RMFSTreeNode child in rmTree.Children)
                {
                    if (HasSelectNodeForFS(child))
                    {
                        AvePoint.RA.Contract.Global.Object.FSTreeNodeDto tempChild = new AvePoint.RA.Contract.Global.Object.FSTreeNodeDto();
                        tempChild.Parent = fs;
                        tempChild.ParentId = fs.Id.ToString();
                        fs.Children.Add(ConvertRMTree2FSTree4AGent(child, tempChild, needDecryptPath));
                    }
                    else
                    {
                        logger.Info("No select node in {0}", child.Name);
                    }
                }
            }
            return fs;
        }
        public static FSTreeNodeDto ConvertRMTree2FSTree(RMFSTreeNode rmTree, FSTreeNodeDto fs = null, bool needDecryptPath = false)
        {
            if (fs == null)
            {
                fs = new FSTreeNodeDto();
            }
            fs.ID = rmTree.Id.ToString();
            fs.FarmID = rmTree.FarmID;
            fs.Name = rmTree.Name;
            fs.FullPath = needDecryptPath ? EncodeUtil.DecryptByCommunicationKey(rmTree.FullPath) : rmTree.FullPath;
            fs.Level = (NodeLevel)rmTree.Level;
            fs.Type = (NodeType)rmTree.NodeType;
            fs.Expanded = rmTree.Expanded;
            fs.ChildrenCount = rmTree.ChildrenCount;
            fs.CheckNumber = rmTree.CheckNumber;

            fs.Domain = rmTree.Domain;
            fs.Username = rmTree.Username;
            fs.EncryptedPassword = rmTree.EncryptedPassword;

            fs.IncludeNew = Convert.ToBoolean(rmTree.IncludeNew) ? IncludeNewState.Checked : IncludeNewState.Unchecked;
            if (fs.NodeExtension == null)
            {
                fs.NodeExtension = new NodeExtensionDto();
            }
            //sp.NodeExtension.BposInfo = rmTree.BposInfo;
            if (rmTree.Parent != null && fs.Parent == null)
            {
                FSTreeNodeDto tempParent = new FSTreeNodeDto();
                tempParent.Children = new List<FSTreeNodeDto> { fs };
                fs.Parent = ConvertRMTree2FSTree(rmTree.Parent, tempParent, needDecryptPath);
                fs.ParentId = rmTree.Parent.Id.ToString();
            }
            if (rmTree.CheckNumber == 1)
            {
                return fs;
            }
            if (rmTree.Children != null && rmTree.Children.Count > 0 &&
                (fs.Children == null || fs.Children.Count == 0))
            {
                fs.Children = new List<FSTreeNodeDto>();
                foreach (RMFSTreeNode child in rmTree.Children)
                {
                    if (HasSelectNodeForFS(child))
                    {
                        FSTreeNodeDto tempChild = new FSTreeNodeDto();
                        tempChild.Parent = fs;
                        tempChild.ParentId = fs.ID.ToString();
                        fs.Children.Add(ConvertRMTree2FSTree(child, tempChild, needDecryptPath));
                    }
                    else
                    {
                        logger.Info("No select node in {0}", child.Name);
                    }
                }
            }
            return fs;
        }

        private static bool HasSelectNodeForFS(RMFSTreeNode current)
        {
            if (current.CheckNumber != 0)
            {
                return true;
            }
            if (current.Children == null || current.Children.Count == 0)
            {
                return false;
            }
            else
            {
                foreach (RMFSTreeNode child in current.Children)
                {
                    if (HasSelectNodeForFS(child))
                    {
                        return true;
                    }
                }
                return false;
            }
        }

        private static bool HasSelectNodeForFS(FSTreeNodeDto current)
        {
            if (current.CheckNumber != 0)
            {
                return true;
            }
            if (current.Children == null || current.Children.Count == 0)
            {
                return false;
            }
            else
            {
                foreach (FSTreeNodeDto child in current.Children)
                {
                    if (HasSelectNodeForFS(child))
                    {
                        return true;
                    }
                }
                return false;
            }
        }
        #endregion

        public static Dictionary<Guid, Contract.Global.Object.RMRuleItemCollection> ConvertTermAndRuleMappings2GlobalDto(Dictionary<Guid, AvePoint.RA.Contract.RMReport.RMRuleItemCollection> keyValuePairs)
        {
            Dictionary<Guid, Contract.Global.Object.RMRuleItemCollection> keyValues = new Dictionary<Guid, Contract.Global.Object.RMRuleItemCollection>();
            foreach (var ruleItems in keyValuePairs)
            {
                Contract.Global.Object.RMRuleItemCollection rMRuleItemCollection = new Contract.Global.Object.RMRuleItemCollection();
                rMRuleItemCollection.TermId = ruleItems.Value.TermId;
                rMRuleItemCollection.TermName = ruleItems.Value.TermName;
                rMRuleItemCollection.HasUnCamlQueryableCondition = ruleItems.Value.HasUnCamlQueryableCondition;
                rMRuleItemCollection.CommonRules = new Contract.Global.Object.RuleCollection();
                rMRuleItemCollection.CommonRules.Rules = new Dictionary<int, Contract.Global.Object.Rule>();
                foreach (var SORule in ruleItems.Value.CommonRules.Rules)
                {
                    rMRuleItemCollection.CommonRules.Rules.Add(SORule.Key, ConvertRule2GlobalDto(SORule.Value));
                }
                rMRuleItemCollection.Rules = new List<Contract.Global.Object.RMRuleItem>();
                foreach (var RMRuleItem in ruleItems.Value.Rules)
                {
                    AvePoint.RA.Contract.Global.Object.RMRuleItem rMRuleItem  = new Contract.Global.Object.RMRuleItem();
                    rMRuleItem.RuleId = RMRuleItem.RuleId;
                    rMRuleItem.RuleName = RMRuleItem.RuleName;
                    rMRuleItem.IsMoveRule = RMRuleItem.IsMoveRule;
                    rMRuleItem.ArchiverAction = RMRuleItem.ArchiverAction;
                    rMRuleItem.IsManualApproval = RMRuleItem.IsManualApproval;
                    rMRuleItem.ExportType = (AvePoint.RA.Contract.Global.Object.ExportTypeValue)Enum.Parse(typeof(AvePoint.RA.Contract.Global.Object.ExportTypeValue), RMRuleItem.ExportType.ToString());
                    rMRuleItem.HasUnCamlQueryableCondition = RMRuleItem.HasUnCamlQueryableCondition;
                    rMRuleItem.DeleteRecords = RMRuleItem.DeleteRecords;
                    rMRuleItem.RelatedRecordOption = (AvePoint.RA.Contract.Global.Object.RelatedRecordOption)Enum.Parse(typeof(AvePoint.RA.Contract.Global.Object.RelatedRecordOption), RMRuleItem.RelatedRecordOption.ToString());
                    rMRuleItem.DisposalClass = RMRuleItem.DisposalClass;
                    rMRuleItemCollection.Rules.Add(rMRuleItem);
                }
                keyValues.Add(ruleItems.Key, rMRuleItemCollection);
            }
            return keyValues;
        }

        #region Box

        public static BoxTreeNodeDto ConvertRMTree2BoxTree(BoxTreeNode rmTree, BoxTreeNodeDto box = null, bool needDecryptPath = false)
        {
            if (box == null)
            {
                box = new BoxTreeNodeDto();
            }
            box.ID = rmTree.Id.ToString();
            box.Name = rmTree.Name;
            box.FullPath = needDecryptPath ? EncodeUtil.DecryptByCommunicationKey(rmTree.FullPath) : rmTree.FullPath;
            box.Level = (NodeLevel)rmTree.Level;
            box.Expanded = rmTree.Expanded;
            box.ChildrenCount = rmTree.ChildrenCount;
            box.CheckNumber = rmTree.CheckNumber;

            box.RealId = rmTree.RealId;
            box.ContainerId = rmTree.ContainerId;
            box.LeafName = rmTree.LeafName;
            box.OwnerId = rmTree.OwnerId;
            box.ConnectionId = rmTree.ConnectionId;
            box.StartJobNodeLevel = (int)rmTree.StartJobNodeLevel;

            if (box.NodeExtension == null)
            {
                box.NodeExtension = new NodeExtensionDto();
            }
            //sp.NodeExtension.BposInfo = rmTree.BposInfo;
            if (rmTree.Parent != null && box.Parent == null)
            {
                BoxTreeNodeDto tempParent = new BoxTreeNodeDto();
                tempParent.Children = new List<BoxTreeNodeDto> { box };
                box.Parent = ConvertRMTree2BoxTree(rmTree.Parent, tempParent, needDecryptPath);
                box.ParentId = rmTree.Parent.Id.ToString();
            }
            if (rmTree.CheckNumber == 1)
            {
                return box;
            }
            if (rmTree.Children != null && rmTree.Children.Count > 0 &&
                (box.Children == null || box.Children.Count == 0))
            {
                box.Children = new List<BoxTreeNodeDto>();
                foreach (BoxTreeNode child in rmTree.Children)
                {
                    if (HasSelectNodeForBox(child))
                    {
                        BoxTreeNodeDto tempChild = new BoxTreeNodeDto();
                        tempChild.Parent = box;
                        tempChild.ParentId = box.ID.ToString();
                        box.Children.Add(ConvertRMTree2BoxTree(child, tempChild, needDecryptPath));
                    }
                    else
                    {
                        logger.Info("No select node in {0}", child.Name);
                    }
                }
            }
            return box;
        }

        public static AvePoint.RA.Contract.Global.Object.BoxTreeNodeDto ConvertRMTree2BoxTree4AGent(BoxTreeNode rmTree, AvePoint.RA.Contract.Global.Object.BoxTreeNodeDto box = null, bool needDecryptPath = false)
        {
            if (box == null)
            {
                box = new AvePoint.RA.Contract.Global.Object.BoxTreeNodeDto();
            }
            box.Id = rmTree.Id;
            box.Name = rmTree.Name;
            box.FullPath = needDecryptPath ? EncodeUtil.DecryptByCommunicationKey(rmTree.FullPath) : rmTree.FullPath;
            box.Level = (RMNodeLevel)rmTree.Level;
            box.Expanded = rmTree.Expanded;
            box.ChildrenCount = rmTree.ChildrenCount;
            box.CheckNumber = rmTree.CheckNumber;

            box.RealId = rmTree.RealId;
            box.ContainerId = rmTree.ContainerId;
            box.LeafName = rmTree.LeafName;
            box.OwnerId = rmTree.OwnerId;
            box.ConnectionId = rmTree.ConnectionId;
            box.StartJobNodeLevel = (RMNodeLevel)rmTree.StartJobNodeLevel;

            //sp.NodeExtension.BposInfo = rmTree.BposInfo;
            if (rmTree.Parent != null && box.Parent == null)
            {
                AvePoint.RA.Contract.Global.Object.BoxTreeNodeDto tempParent = new AvePoint.RA.Contract.Global.Object.BoxTreeNodeDto();
                tempParent.Children = new List<AvePoint.RA.Contract.Global.Object.BoxTreeNodeDto> { box };
                box.Parent = ConvertRMTree2BoxTree4AGent(rmTree.Parent, tempParent, needDecryptPath);
            }
            if (rmTree.CheckNumber == 1)
            {
                return box;
            }
            if (rmTree.Children != null && rmTree.Children.Count > 0 &&
                (box.Children == null || box.Children.Count == 0))
            {
                box.Children = new List<AvePoint.RA.Contract.Global.Object.BoxTreeNodeDto>();
                foreach (BoxTreeNode child in rmTree.Children)
                {
                    if (HasSelectNodeForBox(child))
                    {
                        AvePoint.RA.Contract.Global.Object.BoxTreeNodeDto tempChild = new AvePoint.RA.Contract.Global.Object.BoxTreeNodeDto();
                        tempChild.Parent = box;
                        box.Children.Add(ConvertRMTree2BoxTree4AGent(child, tempChild, needDecryptPath));
                    }
                    else
                    {
                        logger.Info("No select node in {0}", child.Name);
                    }
                }
            }
            return box;
        }

        private static bool HasSelectNodeForBox(BoxTreeNode current)
        {
            if (current.CheckNumber != 0)
            {
                return true;
            }
            if (current.Children == null || current.Children.Count == 0)
            {
                return false;
            }
            else
            {
                foreach (BoxTreeNode child in current.Children)
                {
                    if (HasSelectNodeForBox(child))
                    {
                        return true;
                    }
                }
                return false;
            }
        }

        #endregion
    }

}
