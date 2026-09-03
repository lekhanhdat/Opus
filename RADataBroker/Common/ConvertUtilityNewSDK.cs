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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AvePoint.GCommon.Contract.StorageOptimization.Object;
using AvePoint.GCommon.Contract.Server.ControlPanel.Office365;
using AvePoint.GCommon.Contract.CentralAdmin.Object;
using AvePoint.GCommon.Contract.Tree.Object;
using AvePoint.GCommon.Contract.ExchangeOnline.ExchangeOnlineMailbox.Object;
using AvePoint.GCommon.Utility;
using AvePoint.RA.Contract.Object;
using System.IO;
using System.Xml.Serialization;
using Newtonsoft.Json;
using SerializerHelper = AvePoint.RA.Common.Global.Utils.SerializerHelper;
using ArgumentCheck = AvePoint.GCommon.Utility.ArgumentCheck;
using Microsoft.SharePoint.Client;

namespace AvePoint.RA.RADataBroker.Common
{
    public static class ConvertUtilityNewSDK
    {
        #region Rule Convert
        public static Cloud.Sdk.Data.Dao.ArchiverRule ConvertSORuleToArchiverRule(Rule rule)
        {
            if (null == rule) return null;
            Cloud.Sdk.Data.Dao.ArchiverRule archiverRule = new Cloud.Sdk.Data.Dao.ArchiverRule();
            archiverRule.AndOrExpression = rule.AndOrExpression == null ? null : ConvertToArchiverRulePolicyLevel(rule.AndOrExpression);
            archiverRule.ArchiverCompressionType = (Cloud.Sdk.Data.Dao.CompressionType)rule.ArchiverCompressionType;
            archiverRule.ArchiverDataSecurity = (Cloud.Sdk.Data.Dao.DataSecurity)rule.ArchiverDataSecurity;
            archiverRule.ArchiverSetting = rule.ArchiverSetting == null ? null : ConvertToArchiverRuleArchiverSetting(rule.ArchiverSetting);
            archiverRule.ArchiverVEOSetting = rule.ArchiverVEOSetting == null ? null : ConvertToArchiverRuleArchiverVEOSetting(rule.ArchiverVEOSetting);
            archiverRule.CheckStatus = (Cloud.Sdk.Data.Dao.ActionStatus)rule.CheckStatus;
            archiverRule.Compression = rule.Compression;
            archiverRule.DataEncryptionProfileId = rule.DataEncryptionProfileId;
            archiverRule.DataEncryptionProfileName = rule.DataEncryptionProfileName;
            archiverRule.DataSecurity = rule.DataSecurity;
            archiverRule.Description = rule.Description;
            archiverRule.Detail = rule.Detail;
            archiverRule.Encryption = rule.Encryption;
            archiverRule.DisposalClass = rule.DisposalClass;
            archiverRule.NAAConfigFile = rule.NAAConfigFile;
            archiverRule.NARAConfigFile = rule.NARAConfigFile;
            archiverRule.DeleteRecords = rule.DeleteRecords;
            archiverRule.DeclareLinkFile = rule.DeclareLinkFile;
            archiverRule.EncryptionInfoId = rule.EncryptionInfoId;
            archiverRule.EncryptionInfoName = rule.EncryptionInfoName;
            archiverRule.EncryptionMethods = (Cloud.Sdk.Data.Dao.EncryptionMethods)rule.EncryptionMethods;
            archiverRule.ExportInfo = rule.ExportInfo == null ? null : ConvertToArchiverRuleExportInfo(rule.ExportInfo);
            archiverRule.ExportType = (Cloud.Sdk.Data.Dao.ExportTypeValue)rule.ExportType;
            archiverRule.FileVEO = rule.FileVEO;
            archiverRule.Filters = rule.Filters == null ? new List<Cloud.Sdk.Data.Dao.FilterPolicy>() : ConvertFilterPolicyToApiFilterPolicy(rule.Filters);
            archiverRule.Id = rule.Id;
            archiverRule.IncludeNew = rule.IncludeNew;
            archiverRule.IsCheckedCheckBox = rule.IsCheckedCheckBox;
            archiverRule.IsEnabledComboBox = rule.IsEnabledComboBox;
            archiverRule.IsManualApproval = rule.IsManualApproval;
            archiverRule.KeepDataOption = rule.KeepDataOption;
            archiverRule.KeepStructrue = rule.KeepStructrue;
            archiverRule.LogicalDeviceId = rule.LogicalDeviceId;
            archiverRule.LogicalDeviceName = rule.LogicalDeviceName;
            archiverRule.ManifestVEO = rule.ManifestVEO;
            archiverRule.ModifyTime = rule.ModifyTime;
            archiverRule.Module = rule.Module;
            archiverRule.MoveToRecordCenterAndDelareSetting = rule.MoveToRecordCenterAndDelareSetting == null ? null : ConvertToArchiverRuleMoveToRecordCenterAndDelareSetting(rule.MoveToRecordCenterAndDelareSetting);
            archiverRule.Name = rule.Name;
            archiverRule.NodeLevel = (Cloud.Sdk.Data.Dao.NodeLevel)rule.NodeLevel;
            archiverRule.NotToCheck = rule.NotToCheck;
            archiverRule.Order = rule.Order;
            archiverRule.OrderList = rule.OrderList == null ? new List<int>() : rule.OrderList;
            archiverRule.PolicyLevel = (Cloud.Sdk.Data.Dao.PolicyLevel)rule.PolicyLevel;
            archiverRule.ProfileInfo = rule.ProfileInfo;
            archiverRule.ProfileType = (Cloud.Sdk.Data.Dao.ProfileType)rule.ProfileType;
            archiverRule.RecordVEO = rule.RecordVEO;
            archiverRule.ExportDataEncryptionIV = rule.ExportDataEncryptionIV;
            archiverRule.ExportDataEncryptionKey = rule.ExportDataEncryptionKey;
            archiverRule.RuleScope = rule.RuleScope;
            archiverRule.RuleStatus = (Cloud.Sdk.Data.Dao.RuleStatus)rule.RuleStatus;
            archiverRule.SOFilters = ConvertToArchiverRuleSOFilterPolicy(rule.SOFilters);
            archiverRule.StoragePolicyId = rule.StoragePolicyId;
            archiverRule.StoragePolicyName = rule.StoragePolicyName;
            archiverRule.TagContentInfo = rule.TagContentInfo == null ? new List<Cloud.Sdk.Data.Dao.TagContentInfo>() : ConvertToArchiverRuleTagContentInfo(rule.TagContentInfo);
            archiverRule.Type = (Cloud.Sdk.Data.Dao.RuleType)rule.Type;
            archiverRule.UseSnapLock = rule.UseSnapLock;
            archiverRule.IsSendEmailToOwner = rule.IsSendEamilToOwner;
            archiverRule.UserInfos = ConvertToArchiverRuleUserInfo(rule.UserInfos);
            archiverRule.ReviewType = (Cloud.Sdk.Data.Dao.ReviewType)rule.ReviewType;
            archiverRule.WorkflowId = rule.WorkflowId;
            archiverRule.RelatedRecordOption = (Cloud.Sdk.Data.Dao.RelatedRecordOption)rule.RelatedRecordOption;
            archiverRule.LeaveStubType = (int)rule.LeaveStubType;
            archiverRule.LeaveStubMessage = rule.LeaveStubMessage;
            archiverRule.StubTemplateId = rule.StubTemplateId;
            archiverRule.IsFileName = rule.IsFileName;
            archiverRule.IsFilePath = rule.IsFilePath;
            archiverRule.IsArchivedDate = rule.IsArchivedDate;
            archiverRule.IsRuleName = rule.IsRuleName;
            archiverRule.IsRestoreLink = rule.IsRestoreLink;
            archiverRule.IsEnableRetention = rule.IsEnableRetention;
            archiverRule.RetentionInfo = rule.RetentionInfo == null ? null : convertToRetentionInfo(rule.RetentionInfo);
            archiverRule.spMoveOption = rule.spMoveOption == null ? null : ConvertToArchiverRuleMoveOption(rule.spMoveOption);
            archiverRule.EXORule = rule.EXORule == null ? null : ConvertEXORuleToArchiverRule(rule.EXORule);
            archiverRule.PhysicalRule = rule.PhysicalRule == null ? null : ConvertPhysicalRuleToArchiverRule(rule.PhysicalRule);
            archiverRule.FSRule = rule.FSRule == null ? null : ConvertFSRuleToArchiverRule(rule.FSRule);
            archiverRule.SPLocalRule = rule.SPLocalRule == null ? null : ConvertSPLocalRuleToArchiverRule(rule.SPLocalRule);
            archiverRule.OneDriveRule = rule.OneDriveRule == null ? null : ConvertOneDriveRuleToArchiverRule(rule.OneDriveRule);
            archiverRule.AzureFileRule = rule.AzureFileRule == null ? null : ConvertAzureFileRuleToArchiverRule(rule.AzureFileRule);
            archiverRule.ConnectorRule = rule.ConnectorRule == null ? null : ConvertConnectorRuleToArchiverRule(rule.ConnectorRule);
            archiverRule.BoxRule = rule.BoxRule == null ? null : ConvertBoxRuleToArchiverRule(rule.BoxRule);
            return archiverRule;
        }
        public static Cloud.Sdk.Data.Dao.ArchiverRule ConvertEXORuleToArchiverRule(Rule rule)
        {
            if (null == rule) return null;
            Cloud.Sdk.Data.Dao.ArchiverRule archiverRule = new Cloud.Sdk.Data.Dao.ArchiverRule();
            archiverRule.AndOrExpression = rule.AndOrExpression == null ? null : ConvertToArchiverRulePolicyLevel(rule.AndOrExpression);
            archiverRule.ArchiverCompressionType = (Cloud.Sdk.Data.Dao.CompressionType)rule.ArchiverCompressionType;
            archiverRule.ArchiverDataSecurity = (Cloud.Sdk.Data.Dao.DataSecurity)rule.ArchiverDataSecurity;
            archiverRule.ArchiverSetting = rule.ArchiverSetting == null ? null : ConvertToArchiverRuleArchiverSetting(rule.ArchiverSetting);
            archiverRule.ArchiverVEOSetting = rule.ArchiverVEOSetting == null ? null : ConvertToArchiverRuleArchiverVEOSetting(rule.ArchiverVEOSetting);
            archiverRule.CheckStatus = (Cloud.Sdk.Data.Dao.ActionStatus)rule.CheckStatus;
            archiverRule.Compression = rule.Compression;
            archiverRule.DataEncryptionProfileId = rule.DataEncryptionProfileId;
            archiverRule.DataEncryptionProfileName = rule.DataEncryptionProfileName;
            archiverRule.DataSecurity = rule.DataSecurity;
            archiverRule.Description = rule.Description;
            archiverRule.Detail = rule.Detail;
            archiverRule.Encryption = rule.Encryption;
            archiverRule.DisposalClass = rule.DisposalClass;
            archiverRule.NAAConfigFile = rule.NAAConfigFile;
            archiverRule.NARAConfigFile = rule.NARAConfigFile;
            archiverRule.DeleteRecords = rule.DeleteRecords;
            archiverRule.DeclareLinkFile = rule.DeclareLinkFile;
            archiverRule.EncryptionInfoId = rule.EncryptionInfoId;
            archiverRule.EncryptionInfoName = rule.EncryptionInfoName;
            archiverRule.EncryptionMethods = (Cloud.Sdk.Data.Dao.EncryptionMethods)rule.EncryptionMethods;
            archiverRule.ExportInfo = rule.ExportInfo == null ? null : ConvertToArchiverRuleExportInfo(rule.ExportInfo);
            archiverRule.ExportType = (Cloud.Sdk.Data.Dao.ExportTypeValue)rule.ExportType;
            archiverRule.FileVEO = rule.FileVEO;
            archiverRule.Filters = rule.Filters == null ? new List<Cloud.Sdk.Data.Dao.FilterPolicy>() : ConvertFilterPolicyToApiFilterPolicy(rule.Filters);
            archiverRule.Id = Guid.NewGuid().ToString();
            archiverRule.IncludeNew = rule.IncludeNew;
            archiverRule.IsCheckedCheckBox = rule.IsCheckedCheckBox;
            archiverRule.IsEnabledComboBox = rule.IsEnabledComboBox;
            archiverRule.IsManualApproval = rule.IsManualApproval;
            archiverRule.KeepDataOption = rule.KeepDataOption;
            archiverRule.KeepStructrue = rule.KeepStructrue;
            archiverRule.LogicalDeviceId = rule.LogicalDeviceId;
            archiverRule.LogicalDeviceName = rule.LogicalDeviceName;
            archiverRule.ManifestVEO = rule.ManifestVEO;
            archiverRule.ModifyTime = rule.ModifyTime;
            archiverRule.Module = rule.Module;
            archiverRule.MoveToRecordCenterAndDelareSetting = rule.MoveToRecordCenterAndDelareSetting == null ? null : ConvertToArchiverRuleMoveToRecordCenterAndDelareSetting(rule.MoveToRecordCenterAndDelareSetting);
            archiverRule.Name = rule.Name;
            archiverRule.NodeLevel = (Cloud.Sdk.Data.Dao.NodeLevel)rule.NodeLevel;
            archiverRule.NotToCheck = rule.NotToCheck;
            archiverRule.Order = rule.Order;
            archiverRule.OrderList = rule.OrderList == null ? new List<int>() : rule.OrderList;
            archiverRule.PolicyLevel = (Cloud.Sdk.Data.Dao.PolicyLevel)rule.PolicyLevel;
            archiverRule.ProfileInfo = rule.ProfileInfo;
            archiverRule.ProfileType = (Cloud.Sdk.Data.Dao.ProfileType)rule.ProfileType;
            archiverRule.RecordVEO = rule.RecordVEO;
            archiverRule.ExportDataEncryptionIV = rule.ExportDataEncryptionIV;
            archiverRule.ExportDataEncryptionKey = rule.ExportDataEncryptionKey;
            archiverRule.RuleScope = rule.RuleScope;
            archiverRule.RuleStatus = (Cloud.Sdk.Data.Dao.RuleStatus)rule.RuleStatus;
            archiverRule.SOFilters = ConvertToArchiverRuleSOFilterPolicy(rule.SOFilters);
            archiverRule.StoragePolicyId = rule.StoragePolicyId;
            archiverRule.StoragePolicyName = rule.StoragePolicyName;
            archiverRule.TagContentInfo = rule.TagContentInfo == null ? new List<Cloud.Sdk.Data.Dao.TagContentInfo>() : ConvertToArchiverRuleTagContentInfo(rule.TagContentInfo);
            archiverRule.Type = (Cloud.Sdk.Data.Dao.RuleType)rule.Type;
            archiverRule.UseSnapLock = rule.UseSnapLock;
            archiverRule.IsSendEmailToOwner = rule.IsSendEamilToOwner;
            archiverRule.UserInfos = ConvertToArchiverRuleUserInfo(rule.UserInfos);
            archiverRule.ReviewType = (Cloud.Sdk.Data.Dao.ReviewType)rule.ReviewType;
            archiverRule.WorkflowId = rule.WorkflowId;
            archiverRule.spMoveOption = rule.spMoveOption == null ? null : ConvertToArchiverRuleEXOMoveOption(rule.spMoveOption);
            archiverRule.RelatedRecordOption = (Cloud.Sdk.Data.Dao.RelatedRecordOption)rule.RelatedRecordOption;
            archiverRule.spMoveOption = rule.spMoveOption == null ? null : ConvertToArchiverRuleMoveOption(rule.spMoveOption);
            return archiverRule;
        }
        public static Cloud.Sdk.Data.Dao.ArchiverRule ConvertPhysicalRuleToArchiverRule(Rule rule)
        {
            if (null == rule) return null;
            Cloud.Sdk.Data.Dao.ArchiverRule archiverRule = new Cloud.Sdk.Data.Dao.ArchiverRule();
            archiverRule.AndOrExpression = rule.AndOrExpression == null ? null : ConvertToArchiverRulePolicyLevel(rule.AndOrExpression);
            archiverRule.ArchiverCompressionType = (Cloud.Sdk.Data.Dao.CompressionType)rule.ArchiverCompressionType;
            archiverRule.ArchiverDataSecurity = (Cloud.Sdk.Data.Dao.DataSecurity)rule.ArchiverDataSecurity;
            archiverRule.ArchiverSetting = rule.ArchiverSetting == null ? null : ConvertToArchiverRuleArchiverSetting(rule.ArchiverSetting);
            archiverRule.ArchiverVEOSetting = rule.ArchiverVEOSetting == null ? null : ConvertToArchiverRuleArchiverVEOSetting(rule.ArchiverVEOSetting);
            archiverRule.CheckStatus = (Cloud.Sdk.Data.Dao.ActionStatus)rule.CheckStatus;
            archiverRule.Compression = rule.Compression;
            archiverRule.DataEncryptionProfileId = rule.DataEncryptionProfileId;
            archiverRule.DataEncryptionProfileName = rule.DataEncryptionProfileName;
            archiverRule.DataSecurity = rule.DataSecurity;
            archiverRule.Description = rule.Description;
            archiverRule.Detail = rule.Detail;
            archiverRule.Encryption = rule.Encryption;
            archiverRule.DisposalClass = rule.DisposalClass;
            archiverRule.NAAConfigFile = rule.NAAConfigFile;
            archiverRule.NARAConfigFile = rule.NARAConfigFile;
            archiverRule.DeleteRecords = rule.DeleteRecords;
            archiverRule.DeclareLinkFile = rule.DeclareLinkFile;
            archiverRule.EncryptionInfoId = rule.EncryptionInfoId;
            archiverRule.EncryptionInfoName = rule.EncryptionInfoName;
            archiverRule.EncryptionMethods = (Cloud.Sdk.Data.Dao.EncryptionMethods)rule.EncryptionMethods;
            archiverRule.ExportInfo = rule.ExportInfo == null ? null : ConvertToArchiverRuleExportInfo(rule.ExportInfo);
            archiverRule.ExportType = (Cloud.Sdk.Data.Dao.ExportTypeValue)rule.ExportType;
            archiverRule.FileVEO = rule.FileVEO;
            archiverRule.Filters = rule.Filters == null ? new List<Cloud.Sdk.Data.Dao.FilterPolicy>() : ConvertFilterPolicyToApiFilterPolicy(rule.Filters);
            archiverRule.Id = Guid.NewGuid().ToString();
            archiverRule.IncludeNew = rule.IncludeNew;
            archiverRule.IsCheckedCheckBox = rule.IsCheckedCheckBox;
            archiverRule.IsEnabledComboBox = rule.IsEnabledComboBox;
            archiverRule.IsManualApproval = rule.IsManualApproval;
            archiverRule.KeepDataOption = rule.KeepDataOption;
            archiverRule.KeepStructrue = rule.KeepStructrue;
            archiverRule.LogicalDeviceId = rule.LogicalDeviceId;
            archiverRule.LogicalDeviceName = rule.LogicalDeviceName;
            archiverRule.ManifestVEO = rule.ManifestVEO;
            archiverRule.ModifyTime = rule.ModifyTime;
            archiverRule.Module = rule.Module;
            archiverRule.MoveToRecordCenterAndDelareSetting = rule.MoveToRecordCenterAndDelareSetting == null ? null : ConvertToArchiverRuleMoveToRecordCenterAndDelareSetting(rule.MoveToRecordCenterAndDelareSetting);
            archiverRule.Name = rule.Name;
            archiverRule.NodeLevel = (Cloud.Sdk.Data.Dao.NodeLevel)rule.NodeLevel;
            archiverRule.NotToCheck = rule.NotToCheck;
            archiverRule.Order = rule.Order;
            archiverRule.OrderList = rule.OrderList == null ? new List<int>() : rule.OrderList;
            archiverRule.PolicyLevel = (Cloud.Sdk.Data.Dao.PolicyLevel)rule.PolicyLevel;
            archiverRule.ProfileInfo = rule.ProfileInfo;
            archiverRule.ProfileType = (Cloud.Sdk.Data.Dao.ProfileType)rule.ProfileType;
            archiverRule.RecordVEO = rule.RecordVEO;
            archiverRule.ExportDataEncryptionIV = rule.ExportDataEncryptionIV;
            archiverRule.ExportDataEncryptionKey = rule.ExportDataEncryptionKey;
            archiverRule.RuleScope = rule.RuleScope;
            archiverRule.RuleStatus = (Cloud.Sdk.Data.Dao.RuleStatus)rule.RuleStatus;
            archiverRule.SOFilters = ConvertToArchiverRuleSOFilterPolicy(rule.SOFilters);
            archiverRule.StoragePolicyId = rule.StoragePolicyId;
            archiverRule.StoragePolicyName = rule.StoragePolicyName;
            archiverRule.TagContentInfo = rule.TagContentInfo == null ? new List<Cloud.Sdk.Data.Dao.TagContentInfo>() : ConvertToArchiverRuleTagContentInfo(rule.TagContentInfo);
            archiverRule.Type = (Cloud.Sdk.Data.Dao.RuleType)rule.Type;
            archiverRule.UseSnapLock = rule.UseSnapLock;
            archiverRule.IsSendEmailToOwner = rule.IsSendEamilToOwner;
            archiverRule.UserInfos = ConvertToArchiverRuleUserInfo(rule.UserInfos);
            archiverRule.ReviewType = (Cloud.Sdk.Data.Dao.ReviewType)rule.ReviewType;
            archiverRule.WorkflowId = rule.WorkflowId;
            archiverRule.RelatedRecordOption = (Cloud.Sdk.Data.Dao.RelatedRecordOption)rule.RelatedRecordOption;
            archiverRule.spMoveOption = ConvertToPhyArchiverRuleMoveOption(rule.spMoveOption);
            archiverRule.IsDeleteParentBox = rule.IsDeleteParentBox;
            return archiverRule;
        }
        public static Cloud.Sdk.Data.Dao.ArchiverRule ConvertFSRuleToArchiverRule(Rule rule)
        {
            if (null == rule) return null;
            Cloud.Sdk.Data.Dao.ArchiverRule archiverRule = new Cloud.Sdk.Data.Dao.ArchiverRule();
            archiverRule.AndOrExpression = rule.AndOrExpression == null ? null : ConvertToArchiverRulePolicyLevel(rule.AndOrExpression);
            archiverRule.ArchiverCompressionType = (Cloud.Sdk.Data.Dao.CompressionType)rule.ArchiverCompressionType;
            archiverRule.ArchiverDataSecurity = (Cloud.Sdk.Data.Dao.DataSecurity)rule.ArchiverDataSecurity;
            archiverRule.ArchiverSetting = rule.ArchiverSetting == null ? null : ConvertToArchiverRuleArchiverSetting(rule.ArchiverSetting);
            archiverRule.ArchiverVEOSetting = rule.ArchiverVEOSetting == null ? null : ConvertToArchiverRuleArchiverVEOSetting(rule.ArchiverVEOSetting);
            archiverRule.CheckStatus = (Cloud.Sdk.Data.Dao.ActionStatus)rule.CheckStatus;
            archiverRule.Compression = rule.Compression;
            archiverRule.DataEncryptionProfileId = rule.DataEncryptionProfileId;
            archiverRule.DataEncryptionProfileName = rule.DataEncryptionProfileName;
            archiverRule.DataSecurity = rule.DataSecurity;
            archiverRule.Description = rule.Description;
            archiverRule.Detail = rule.Detail;
            archiverRule.Encryption = rule.Encryption;
            archiverRule.DisposalClass = rule.DisposalClass;
            archiverRule.NAAConfigFile = rule.NAAConfigFile;
            archiverRule.NARAConfigFile = rule.NARAConfigFile;
            archiverRule.DeleteRecords = rule.DeleteRecords;
            archiverRule.DeclareLinkFile = rule.DeclareLinkFile;
            archiverRule.EncryptionInfoId = rule.EncryptionInfoId;
            archiverRule.EncryptionInfoName = rule.EncryptionInfoName;
            archiverRule.EncryptionMethods = (Cloud.Sdk.Data.Dao.EncryptionMethods)rule.EncryptionMethods;
            archiverRule.ExportInfo = rule.ExportInfo == null ? null : ConvertToArchiverRuleExportInfo(rule.ExportInfo);
            archiverRule.ExportType = (Cloud.Sdk.Data.Dao.ExportTypeValue)rule.ExportType;
            archiverRule.FileVEO = rule.FileVEO;
            archiverRule.Filters = rule.Filters == null ? new List<Cloud.Sdk.Data.Dao.FilterPolicy>() : ConvertFilterPolicyToApiFilterPolicy(rule.Filters);
            archiverRule.Id = Guid.NewGuid().ToString();
            archiverRule.IncludeNew = rule.IncludeNew;
            archiverRule.IsCheckedCheckBox = rule.IsCheckedCheckBox;
            archiverRule.IsEnabledComboBox = rule.IsEnabledComboBox;
            archiverRule.IsManualApproval = rule.IsManualApproval;
            archiverRule.KeepDataOption = rule.KeepDataOption;
            archiverRule.KeepStructrue = rule.KeepStructrue;
            archiverRule.LogicalDeviceId = rule.LogicalDeviceId;
            archiverRule.LogicalDeviceName = rule.LogicalDeviceName;
            archiverRule.ManifestVEO = rule.ManifestVEO;
            archiverRule.ModifyTime = rule.ModifyTime;
            archiverRule.Module = rule.Module;
            archiverRule.MoveToRecordCenterAndDelareSetting = rule.MoveToRecordCenterAndDelareSetting == null ? null : ConvertToArchiverRuleMoveToRecordCenterAndDelareSetting(rule.MoveToRecordCenterAndDelareSetting);
            archiverRule.Name = rule.Name;
            archiverRule.NodeLevel = (Cloud.Sdk.Data.Dao.NodeLevel)rule.NodeLevel;
            archiverRule.NotToCheck = rule.NotToCheck;
            archiverRule.Order = rule.Order;
            archiverRule.OrderList = rule.OrderList == null ? new List<int>() : rule.OrderList;
            archiverRule.PolicyLevel = (Cloud.Sdk.Data.Dao.PolicyLevel)rule.PolicyLevel;
            archiverRule.ProfileInfo = rule.ProfileInfo;
            archiverRule.ProfileType = (Cloud.Sdk.Data.Dao.ProfileType)rule.ProfileType;
            archiverRule.RecordVEO = rule.RecordVEO;
            archiverRule.ExportDataEncryptionIV = rule.ExportDataEncryptionIV;
            archiverRule.ExportDataEncryptionKey = rule.ExportDataEncryptionKey;
            archiverRule.RuleScope = rule.RuleScope;
            archiverRule.RuleStatus = (Cloud.Sdk.Data.Dao.RuleStatus)rule.RuleStatus;
            archiverRule.SOFilters = ConvertToArchiverRuleSOFilterPolicy(rule.SOFilters);
            archiverRule.StoragePolicyId = rule.StoragePolicyId;
            archiverRule.StoragePolicyName = rule.StoragePolicyName;
            archiverRule.TagContentInfo = rule.TagContentInfo == null ? new List<Cloud.Sdk.Data.Dao.TagContentInfo>() : ConvertToArchiverRuleTagContentInfo(rule.TagContentInfo);
            archiverRule.Type = (Cloud.Sdk.Data.Dao.RuleType)rule.Type;
            archiverRule.UseSnapLock = rule.UseSnapLock;
            archiverRule.IsSendEmailToOwner = rule.IsSendEamilToOwner;
            archiverRule.UserInfos = ConvertToArchiverRuleUserInfo(rule.UserInfos);
            archiverRule.ReviewType = (Cloud.Sdk.Data.Dao.ReviewType)rule.ReviewType;
            archiverRule.WorkflowId = rule.WorkflowId;
            archiverRule.RelatedRecordOption = (Cloud.Sdk.Data.Dao.RelatedRecordOption)rule.RelatedRecordOption;
            archiverRule.spMoveOption = rule.spMoveOption == null ? null : ConvertToArchiverRuleFSMoveOption(rule.spMoveOption);
            return archiverRule;
        }
        public static Cloud.Sdk.Data.Dao.ArchiverRule ConvertSPLocalRuleToArchiverRule(Rule rule)
        {
            if (null == rule) return null;
            Cloud.Sdk.Data.Dao.ArchiverRule archiverRule = new Cloud.Sdk.Data.Dao.ArchiverRule();
            archiverRule.AndOrExpression = rule.AndOrExpression == null ? null : ConvertToArchiverRulePolicyLevel(rule.AndOrExpression);
            archiverRule.ArchiverCompressionType = (Cloud.Sdk.Data.Dao.CompressionType)rule.ArchiverCompressionType;
            archiverRule.ArchiverDataSecurity = (Cloud.Sdk.Data.Dao.DataSecurity)rule.ArchiverDataSecurity;
            archiverRule.ArchiverSetting = rule.ArchiverSetting == null ? null : ConvertToArchiverRuleArchiverSetting(rule.ArchiverSetting);
            archiverRule.ArchiverVEOSetting = rule.ArchiverVEOSetting == null ? null : ConvertToArchiverRuleArchiverVEOSetting(rule.ArchiverVEOSetting);
            archiverRule.CheckStatus = (Cloud.Sdk.Data.Dao.ActionStatus)rule.CheckStatus;
            archiverRule.Compression = rule.Compression;
            archiverRule.DataEncryptionProfileId = rule.DataEncryptionProfileId;
            archiverRule.DataEncryptionProfileName = rule.DataEncryptionProfileName;
            archiverRule.DataSecurity = rule.DataSecurity;
            archiverRule.Description = rule.Description;
            archiverRule.Detail = rule.Detail;
            archiverRule.Encryption = rule.Encryption;
            archiverRule.DisposalClass = rule.DisposalClass;
            archiverRule.NAAConfigFile = rule.NAAConfigFile;
            archiverRule.NARAConfigFile = rule.NARAConfigFile;
            archiverRule.DeleteRecords = rule.DeleteRecords;
            archiverRule.DeclareLinkFile = rule.DeclareLinkFile;
            archiverRule.EncryptionInfoId = rule.EncryptionInfoId;
            archiverRule.EncryptionInfoName = rule.EncryptionInfoName;
            archiverRule.EncryptionMethods = (Cloud.Sdk.Data.Dao.EncryptionMethods)rule.EncryptionMethods;
            archiverRule.ExportInfo = rule.ExportInfo == null ? null : ConvertToArchiverRuleExportInfo(rule.ExportInfo);
            archiverRule.ExportType = (Cloud.Sdk.Data.Dao.ExportTypeValue)rule.ExportType;
            archiverRule.FileVEO = rule.FileVEO;
            archiverRule.Filters = rule.Filters == null ? new List<Cloud.Sdk.Data.Dao.FilterPolicy>() : ConvertFilterPolicyToApiFilterPolicy(rule.Filters);
            archiverRule.Id = Guid.NewGuid().ToString();
            archiverRule.IncludeNew = rule.IncludeNew;
            archiverRule.IsCheckedCheckBox = rule.IsCheckedCheckBox;
            archiverRule.IsEnabledComboBox = rule.IsEnabledComboBox;
            archiverRule.IsManualApproval = rule.IsManualApproval;
            archiverRule.KeepDataOption = rule.KeepDataOption;
            archiverRule.KeepStructrue = rule.KeepStructrue;
            archiverRule.LogicalDeviceId = rule.LogicalDeviceId;
            archiverRule.LogicalDeviceName = rule.LogicalDeviceName;
            archiverRule.ManifestVEO = rule.ManifestVEO;
            archiverRule.ModifyTime = rule.ModifyTime;
            archiverRule.Module = rule.Module;
            archiverRule.MoveToRecordCenterAndDelareSetting = rule.MoveToRecordCenterAndDelareSetting == null ? null : ConvertToArchiverRuleMoveToRecordCenterAndDelareSetting(rule.MoveToRecordCenterAndDelareSetting);
            archiverRule.Name = rule.Name;
            archiverRule.NodeLevel = (Cloud.Sdk.Data.Dao.NodeLevel)rule.NodeLevel;
            archiverRule.NotToCheck = rule.NotToCheck;
            archiverRule.Order = rule.Order;
            archiverRule.OrderList = rule.OrderList == null ? new List<int>() : rule.OrderList;
            archiverRule.PolicyLevel = (Cloud.Sdk.Data.Dao.PolicyLevel)rule.PolicyLevel;
            archiverRule.ProfileInfo = rule.ProfileInfo;
            archiverRule.ProfileType = (Cloud.Sdk.Data.Dao.ProfileType)rule.ProfileType;
            archiverRule.RecordVEO = rule.RecordVEO;
            archiverRule.ExportDataEncryptionIV = rule.ExportDataEncryptionIV;
            archiverRule.ExportDataEncryptionKey = rule.ExportDataEncryptionKey;
            archiverRule.RuleScope = rule.RuleScope;
            archiverRule.RuleStatus = (Cloud.Sdk.Data.Dao.RuleStatus)rule.RuleStatus;
            archiverRule.SOFilters = ConvertToArchiverRuleSOFilterPolicy(rule.SOFilters);
            archiverRule.StoragePolicyId = rule.StoragePolicyId;
            archiverRule.StoragePolicyName = rule.StoragePolicyName;
            archiverRule.TagContentInfo = rule.TagContentInfo == null ? new List<Cloud.Sdk.Data.Dao.TagContentInfo>() : ConvertToArchiverRuleTagContentInfo(rule.TagContentInfo);
            archiverRule.Type = (Cloud.Sdk.Data.Dao.RuleType)rule.Type;
            archiverRule.UseSnapLock = rule.UseSnapLock;
            archiverRule.IsSendEmailToOwner = rule.IsSendEamilToOwner;
            archiverRule.UserInfos = ConvertToArchiverRuleUserInfo(rule.UserInfos);
            archiverRule.ReviewType = (Cloud.Sdk.Data.Dao.ReviewType)rule.ReviewType;
            archiverRule.WorkflowId = rule.WorkflowId;
            archiverRule.RelatedRecordOption = (Cloud.Sdk.Data.Dao.RelatedRecordOption)rule.RelatedRecordOption;
            archiverRule.spMoveOption = rule.spMoveOption == null ? null : ConvertToArchiverRuleSPLocalMoveOption(rule.spMoveOption);
            return archiverRule;
        }
        public static Cloud.Sdk.Data.Dao.ArchiverRule ConvertOneDriveRuleToArchiverRule(Rule rule)
        {
            if (null == rule) return null;
            Cloud.Sdk.Data.Dao.ArchiverRule archiverRule = new Cloud.Sdk.Data.Dao.ArchiverRule();
            archiverRule.AndOrExpression = rule.AndOrExpression == null ? null : ConvertToArchiverRulePolicyLevel(rule.AndOrExpression);
            archiverRule.ArchiverCompressionType = (Cloud.Sdk.Data.Dao.CompressionType)rule.ArchiverCompressionType;
            archiverRule.ArchiverDataSecurity = (Cloud.Sdk.Data.Dao.DataSecurity)rule.ArchiverDataSecurity;
            archiverRule.ArchiverSetting = rule.ArchiverSetting == null ? null : ConvertToArchiverRuleArchiverSetting(rule.ArchiverSetting);
            archiverRule.ArchiverVEOSetting = rule.ArchiverVEOSetting == null ? null : ConvertToArchiverRuleArchiverVEOSetting(rule.ArchiverVEOSetting);
            archiverRule.CheckStatus = (Cloud.Sdk.Data.Dao.ActionStatus)rule.CheckStatus;
            archiverRule.Compression = rule.Compression;
            archiverRule.DataEncryptionProfileId = rule.DataEncryptionProfileId;
            archiverRule.DataEncryptionProfileName = rule.DataEncryptionProfileName;
            archiverRule.DataSecurity = rule.DataSecurity;
            archiverRule.Description = rule.Description;
            archiverRule.Detail = rule.Detail;
            archiverRule.Encryption = rule.Encryption;
            archiverRule.DisposalClass = rule.DisposalClass;
            archiverRule.NAAConfigFile = rule.NAAConfigFile;
            archiverRule.NARAConfigFile = rule.NARAConfigFile;
            archiverRule.DeleteRecords = rule.DeleteRecords;
            archiverRule.DeclareLinkFile = rule.DeclareLinkFile;
            archiverRule.EncryptionInfoId = rule.EncryptionInfoId;
            archiverRule.EncryptionInfoName = rule.EncryptionInfoName;
            archiverRule.EncryptionMethods = (Cloud.Sdk.Data.Dao.EncryptionMethods)rule.EncryptionMethods;
            archiverRule.ExportInfo = rule.ExportInfo == null ? null : ConvertToArchiverRuleExportInfo(rule.ExportInfo);
            archiverRule.ExportType = (Cloud.Sdk.Data.Dao.ExportTypeValue)rule.ExportType;
            archiverRule.FileVEO = rule.FileVEO;
            archiverRule.Filters = rule.Filters == null ? new List<Cloud.Sdk.Data.Dao.FilterPolicy>() : ConvertFilterPolicyToApiFilterPolicy(rule.Filters);
            archiverRule.Id = Guid.NewGuid().ToString();
            archiverRule.IncludeNew = rule.IncludeNew;
            archiverRule.IsCheckedCheckBox = rule.IsCheckedCheckBox;
            archiverRule.IsEnabledComboBox = rule.IsEnabledComboBox;
            archiverRule.IsManualApproval = rule.IsManualApproval;
            archiverRule.KeepDataOption = rule.KeepDataOption;
            archiverRule.KeepStructrue = rule.KeepStructrue;
            archiverRule.LogicalDeviceId = rule.LogicalDeviceId;
            archiverRule.LogicalDeviceName = rule.LogicalDeviceName;
            archiverRule.ManifestVEO = rule.ManifestVEO;
            archiverRule.ModifyTime = rule.ModifyTime;
            archiverRule.Module = rule.Module;
            archiverRule.MoveToRecordCenterAndDelareSetting = rule.MoveToRecordCenterAndDelareSetting == null ? null : ConvertToArchiverRuleMoveToRecordCenterAndDelareSetting(rule.MoveToRecordCenterAndDelareSetting);
            archiverRule.Name = rule.Name;
            archiverRule.NodeLevel = (Cloud.Sdk.Data.Dao.NodeLevel)rule.NodeLevel;
            archiverRule.NotToCheck = rule.NotToCheck;
            archiverRule.Order = rule.Order;
            archiverRule.OrderList = rule.OrderList == null ? new List<int>() : rule.OrderList;
            archiverRule.PolicyLevel = (Cloud.Sdk.Data.Dao.PolicyLevel)rule.PolicyLevel;
            archiverRule.ProfileInfo = rule.ProfileInfo;
            archiverRule.ProfileType = (Cloud.Sdk.Data.Dao.ProfileType)rule.ProfileType;
            archiverRule.RecordVEO = rule.RecordVEO;
            archiverRule.ExportDataEncryptionIV = rule.ExportDataEncryptionIV;
            archiverRule.ExportDataEncryptionKey = rule.ExportDataEncryptionKey;
            archiverRule.RuleScope = rule.RuleScope;
            archiverRule.RuleStatus = (Cloud.Sdk.Data.Dao.RuleStatus)rule.RuleStatus;
            archiverRule.SOFilters = ConvertToArchiverRuleSOFilterPolicy(rule.SOFilters);
            archiverRule.StoragePolicyId = rule.StoragePolicyId;
            archiverRule.StoragePolicyName = rule.StoragePolicyName;
            archiverRule.TagContentInfo = rule.TagContentInfo == null ? new List<Cloud.Sdk.Data.Dao.TagContentInfo>() : ConvertToArchiverRuleTagContentInfo(rule.TagContentInfo);
            archiverRule.Type = (Cloud.Sdk.Data.Dao.RuleType)rule.Type;
            archiverRule.UseSnapLock = rule.UseSnapLock;
            archiverRule.IsSendEmailToOwner = rule.IsSendEamilToOwner;
            archiverRule.UserInfos = ConvertToArchiverRuleUserInfo(rule.UserInfos);
            archiverRule.ReviewType = (Cloud.Sdk.Data.Dao.ReviewType)rule.ReviewType;
            archiverRule.WorkflowId = rule.WorkflowId;
            archiverRule.RelatedRecordOption = (Cloud.Sdk.Data.Dao.RelatedRecordOption)rule.RelatedRecordOption;
            archiverRule.spMoveOption = rule.spMoveOption == null ? null : ConvertToArchiverRuleMoveOption(rule.spMoveOption);
            archiverRule.LeaveStubType = (int)rule.LeaveStubType;
            archiverRule.LeaveStubMessage = rule.LeaveStubMessage;
            archiverRule.StubTemplateId = rule.StubTemplateId;
            archiverRule.IsFileName = rule.IsFileName;
            archiverRule.IsFilePath = rule.IsFilePath;
            archiverRule.IsArchivedDate = rule.IsArchivedDate;
            archiverRule.IsRuleName = rule.IsRuleName;
            archiverRule.IsRestoreLink = rule.IsRestoreLink;
            archiverRule.IsEnableRetention = rule.IsEnableRetention;
            archiverRule.RetentionInfo = rule.RetentionInfo == null ? null : convertToRetentionInfo(rule.RetentionInfo);
            return archiverRule;
        }

        public static Cloud.Sdk.Data.Dao.ArchiverRule ConvertAzureFileRuleToArchiverRule(Rule rule)
        {
            if (null == rule) return null;
            Cloud.Sdk.Data.Dao.ArchiverRule archiverRule = new Cloud.Sdk.Data.Dao.ArchiverRule();
            archiverRule.AndOrExpression = rule.AndOrExpression == null ? null : ConvertToArchiverRulePolicyLevel(rule.AndOrExpression);
            archiverRule.ArchiverCompressionType = (Cloud.Sdk.Data.Dao.CompressionType)rule.ArchiverCompressionType;
            archiverRule.ArchiverDataSecurity = (Cloud.Sdk.Data.Dao.DataSecurity)rule.ArchiverDataSecurity;
            archiverRule.ArchiverSetting = rule.ArchiverSetting == null ? null : ConvertToArchiverRuleArchiverSetting(rule.ArchiverSetting);
            archiverRule.ArchiverVEOSetting = rule.ArchiverVEOSetting == null ? null : ConvertToArchiverRuleArchiverVEOSetting(rule.ArchiverVEOSetting);
            archiverRule.CheckStatus = (Cloud.Sdk.Data.Dao.ActionStatus)rule.CheckStatus;
            archiverRule.Compression = rule.Compression;
            archiverRule.DataEncryptionProfileId = rule.DataEncryptionProfileId;
            archiverRule.DataEncryptionProfileName = rule.DataEncryptionProfileName;
            archiverRule.DataSecurity = rule.DataSecurity;
            archiverRule.Description = rule.Description;
            archiverRule.Detail = rule.Detail;
            archiverRule.Encryption = rule.Encryption;
            archiverRule.DisposalClass = rule.DisposalClass;
            archiverRule.NAAConfigFile = rule.NAAConfigFile;
            archiverRule.NARAConfigFile = rule.NARAConfigFile;
            archiverRule.DeleteRecords = rule.DeleteRecords;
            archiverRule.DeclareLinkFile = rule.DeclareLinkFile;
            archiverRule.EncryptionInfoId = rule.EncryptionInfoId;
            archiverRule.EncryptionInfoName = rule.EncryptionInfoName;
            archiverRule.EncryptionMethods = (Cloud.Sdk.Data.Dao.EncryptionMethods)rule.EncryptionMethods;
            archiverRule.ExportInfo = rule.ExportInfo == null ? null : ConvertToArchiverRuleExportInfo(rule.ExportInfo);
            archiverRule.ExportType = (Cloud.Sdk.Data.Dao.ExportTypeValue)rule.ExportType;
            archiverRule.FileVEO = rule.FileVEO;
            archiverRule.Filters = rule.Filters == null ? new List<Cloud.Sdk.Data.Dao.FilterPolicy>() : ConvertFilterPolicyToApiFilterPolicy(rule.Filters);
            archiverRule.Id = Guid.NewGuid().ToString();
            archiverRule.IncludeNew = rule.IncludeNew;
            archiverRule.IsCheckedCheckBox = rule.IsCheckedCheckBox;
            archiverRule.IsEnabledComboBox = rule.IsEnabledComboBox;
            archiverRule.IsManualApproval = rule.IsManualApproval;
            archiverRule.KeepDataOption = rule.KeepDataOption;
            archiverRule.KeepStructrue = rule.KeepStructrue;
            archiverRule.LogicalDeviceId = rule.LogicalDeviceId;
            archiverRule.LogicalDeviceName = rule.LogicalDeviceName;
            archiverRule.ManifestVEO = rule.ManifestVEO;
            archiverRule.ModifyTime = rule.ModifyTime;
            archiverRule.Module = rule.Module;
            archiverRule.MoveToRecordCenterAndDelareSetting = rule.MoveToRecordCenterAndDelareSetting == null ? null : ConvertToArchiverRuleMoveToRecordCenterAndDelareSetting(rule.MoveToRecordCenterAndDelareSetting);
            archiverRule.Name = rule.Name;
            archiverRule.NodeLevel = (Cloud.Sdk.Data.Dao.NodeLevel)rule.NodeLevel;
            archiverRule.NotToCheck = rule.NotToCheck;
            archiverRule.Order = rule.Order;
            archiverRule.OrderList = rule.OrderList == null ? new List<int>() : rule.OrderList;
            archiverRule.PolicyLevel = (Cloud.Sdk.Data.Dao.PolicyLevel)rule.PolicyLevel;
            archiverRule.ProfileInfo = rule.ProfileInfo;
            archiverRule.ProfileType = (Cloud.Sdk.Data.Dao.ProfileType)rule.ProfileType;
            archiverRule.RecordVEO = rule.RecordVEO;
            archiverRule.ExportDataEncryptionIV = rule.ExportDataEncryptionIV;
            archiverRule.ExportDataEncryptionKey = rule.ExportDataEncryptionKey;
            archiverRule.RuleScope = rule.RuleScope;
            archiverRule.RuleStatus = (Cloud.Sdk.Data.Dao.RuleStatus)rule.RuleStatus;
            archiverRule.SOFilters = ConvertToArchiverRuleSOFilterPolicy(rule.SOFilters);
            archiverRule.StoragePolicyId = rule.StoragePolicyId;
            archiverRule.StoragePolicyName = rule.StoragePolicyName;
            archiverRule.TagContentInfo = rule.TagContentInfo == null ? new List<Cloud.Sdk.Data.Dao.TagContentInfo>() : ConvertToArchiverRuleTagContentInfo(rule.TagContentInfo);
            archiverRule.Type = (Cloud.Sdk.Data.Dao.RuleType)rule.Type;
            archiverRule.UseSnapLock = rule.UseSnapLock;
            archiverRule.IsSendEmailToOwner = rule.IsSendEamilToOwner;
            archiverRule.UserInfos = ConvertToArchiverRuleUserInfo(rule.UserInfos);
            archiverRule.ReviewType = (Cloud.Sdk.Data.Dao.ReviewType)rule.ReviewType;
            archiverRule.WorkflowId = rule.WorkflowId;
            archiverRule.RelatedRecordOption = (Cloud.Sdk.Data.Dao.RelatedRecordOption)rule.RelatedRecordOption;
            archiverRule.spMoveOption = rule.spMoveOption == null ? null : ConvertToArchiverRuleMoveOption(rule.spMoveOption);
            archiverRule.LeaveStubMessage = rule.LeaveStubMessage;
            archiverRule.IsRestoreLink = rule.IsRestoreLink;
            archiverRule.IsEnableRetention = rule.IsEnableRetention;
            archiverRule.RetentionInfo = rule.RetentionInfo == null ? null : convertToRetentionInfo(rule.RetentionInfo);
            return archiverRule;
        }

        public static Cloud.Sdk.Data.Dao.ArchiverRule ConvertConnectorRuleToArchiverRule(Rule rule)
        {
            if (null == rule) return null;
            Cloud.Sdk.Data.Dao.ArchiverRule archiverRule = new Cloud.Sdk.Data.Dao.ArchiverRule();
            archiverRule.AndOrExpression = rule.AndOrExpression == null ? null : ConvertToArchiverRulePolicyLevel(rule.AndOrExpression);
            archiverRule.ArchiverCompressionType = (Cloud.Sdk.Data.Dao.CompressionType)rule.ArchiverCompressionType;
            archiverRule.ArchiverDataSecurity = (Cloud.Sdk.Data.Dao.DataSecurity)rule.ArchiverDataSecurity;
            archiverRule.ArchiverSetting = rule.ArchiverSetting == null ? null : ConvertToArchiverRuleArchiverSetting(rule.ArchiverSetting);
            archiverRule.ArchiverVEOSetting = rule.ArchiverVEOSetting == null ? null : ConvertToArchiverRuleArchiverVEOSetting(rule.ArchiverVEOSetting);
            archiverRule.CheckStatus = (Cloud.Sdk.Data.Dao.ActionStatus)rule.CheckStatus;
            archiverRule.Compression = rule.Compression;
            archiverRule.DataEncryptionProfileId = rule.DataEncryptionProfileId;
            archiverRule.DataEncryptionProfileName = rule.DataEncryptionProfileName;
            archiverRule.DataSecurity = rule.DataSecurity;
            archiverRule.Description = rule.Description;
            archiverRule.Detail = rule.Detail;
            archiverRule.Encryption = rule.Encryption;
            archiverRule.DisposalClass = rule.DisposalClass;
            archiverRule.NAAConfigFile = rule.NAAConfigFile;
            archiverRule.NARAConfigFile = rule.NARAConfigFile;
            archiverRule.DeleteRecords = rule.DeleteRecords;
            archiverRule.DeclareLinkFile = rule.DeclareLinkFile;
            archiverRule.EncryptionInfoId = rule.EncryptionInfoId;
            archiverRule.EncryptionInfoName = rule.EncryptionInfoName;
            archiverRule.EncryptionMethods = (Cloud.Sdk.Data.Dao.EncryptionMethods)rule.EncryptionMethods;
            archiverRule.ExportInfo = rule.ExportInfo == null ? null : ConvertToArchiverRuleExportInfo(rule.ExportInfo);
            archiverRule.ExportType = (Cloud.Sdk.Data.Dao.ExportTypeValue)rule.ExportType;
            archiverRule.FileVEO = rule.FileVEO;
            archiverRule.Filters = rule.Filters == null ? new List<Cloud.Sdk.Data.Dao.FilterPolicy>() : ConvertFilterPolicyToApiFilterPolicy(rule.Filters);
            archiverRule.Id = Guid.NewGuid().ToString();
            archiverRule.IncludeNew = rule.IncludeNew;
            archiverRule.IsCheckedCheckBox = rule.IsCheckedCheckBox;
            archiverRule.IsEnabledComboBox = rule.IsEnabledComboBox;
            archiverRule.IsManualApproval = rule.IsManualApproval;
            archiverRule.KeepDataOption = rule.KeepDataOption;
            archiverRule.KeepStructrue = rule.KeepStructrue;
            archiverRule.LogicalDeviceId = rule.LogicalDeviceId;
            archiverRule.LogicalDeviceName = rule.LogicalDeviceName;
            archiverRule.ManifestVEO = rule.ManifestVEO;
            archiverRule.ModifyTime = rule.ModifyTime;
            archiverRule.Module = rule.Module;
            archiverRule.MoveToRecordCenterAndDelareSetting = rule.MoveToRecordCenterAndDelareSetting == null ? null : ConvertToArchiverRuleMoveToRecordCenterAndDelareSetting(rule.MoveToRecordCenterAndDelareSetting);
            archiverRule.Name = rule.Name;
            archiverRule.NodeLevel = (Cloud.Sdk.Data.Dao.NodeLevel)rule.NodeLevel;
            archiverRule.NotToCheck = rule.NotToCheck;
            archiverRule.Order = rule.Order;
            archiverRule.OrderList = rule.OrderList == null ? new List<int>() : rule.OrderList;
            archiverRule.PolicyLevel = (Cloud.Sdk.Data.Dao.PolicyLevel)rule.PolicyLevel;
            archiverRule.ProfileInfo = rule.ProfileInfo;
            archiverRule.ProfileType = (Cloud.Sdk.Data.Dao.ProfileType)rule.ProfileType;
            archiverRule.RecordVEO = rule.RecordVEO;
            archiverRule.ExportDataEncryptionIV = rule.ExportDataEncryptionIV;
            archiverRule.ExportDataEncryptionKey = rule.ExportDataEncryptionKey;
            archiverRule.RuleScope = rule.RuleScope;
            archiverRule.RuleStatus = (Cloud.Sdk.Data.Dao.RuleStatus)rule.RuleStatus;
            archiverRule.SOFilters = ConvertToArchiverRuleSOFilterPolicy(rule.SOFilters);
            archiverRule.StoragePolicyId = rule.StoragePolicyId;
            archiverRule.StoragePolicyName = rule.StoragePolicyName;
            archiverRule.TagContentInfo = rule.TagContentInfo == null ? new List<Cloud.Sdk.Data.Dao.TagContentInfo>() : ConvertToArchiverRuleTagContentInfo(rule.TagContentInfo);
            archiverRule.Type = (Cloud.Sdk.Data.Dao.RuleType)rule.Type;
            archiverRule.UseSnapLock = rule.UseSnapLock;
            archiverRule.IsSendEmailToOwner = rule.IsSendEamilToOwner;
            archiverRule.UserInfos = ConvertToArchiverRuleUserInfo(rule.UserInfos);
            archiverRule.ReviewType = (Cloud.Sdk.Data.Dao.ReviewType)rule.ReviewType;
            archiverRule.WorkflowId = rule.WorkflowId;
            archiverRule.RelatedRecordOption = (Cloud.Sdk.Data.Dao.RelatedRecordOption)rule.RelatedRecordOption;
            archiverRule.spMoveOption = rule.spMoveOption == null ? null : ConvertToArchiverRuleMoveOption(rule.spMoveOption);
            archiverRule.LeaveStubMessage = rule.LeaveStubMessage;
            archiverRule.IsRestoreLink = rule.IsRestoreLink;
            archiverRule.IsEnableRetention = rule.IsEnableRetention;
            archiverRule.RetentionInfo = rule.RetentionInfo == null ? null : convertToRetentionInfo(rule.RetentionInfo);
            return archiverRule;
        }

        public static Cloud.Sdk.Data.Dao.ArchiverRule ConvertBoxRuleToArchiverRule(Rule rule)
        {
            if (null == rule) return null;
            Cloud.Sdk.Data.Dao.ArchiverRule archiverRule = new Cloud.Sdk.Data.Dao.ArchiverRule();
            archiverRule.AndOrExpression = rule.AndOrExpression == null ? null : ConvertToArchiverRulePolicyLevel(rule.AndOrExpression);
            archiverRule.ArchiverCompressionType = (Cloud.Sdk.Data.Dao.CompressionType)rule.ArchiverCompressionType;
            archiverRule.ArchiverDataSecurity = (Cloud.Sdk.Data.Dao.DataSecurity)rule.ArchiverDataSecurity;
            archiverRule.ArchiverSetting = rule.ArchiverSetting == null ? null : ConvertToArchiverRuleArchiverSetting(rule.ArchiverSetting);
            archiverRule.ArchiverVEOSetting = rule.ArchiverVEOSetting == null ? null : ConvertToArchiverRuleArchiverVEOSetting(rule.ArchiverVEOSetting);
            archiverRule.CheckStatus = (Cloud.Sdk.Data.Dao.ActionStatus)rule.CheckStatus;
            archiverRule.Compression = rule.Compression;
            archiverRule.DataEncryptionProfileId = rule.DataEncryptionProfileId;
            archiverRule.DataEncryptionProfileName = rule.DataEncryptionProfileName;
            archiverRule.DataSecurity = rule.DataSecurity;
            archiverRule.Description = rule.Description;
            archiverRule.Detail = rule.Detail;
            archiverRule.Encryption = rule.Encryption;
            archiverRule.DisposalClass = rule.DisposalClass;
            archiverRule.NAAConfigFile = rule.NAAConfigFile;
            archiverRule.NARAConfigFile = rule.NARAConfigFile;
            archiverRule.DeleteRecords = rule.DeleteRecords;
            archiverRule.DeclareLinkFile = rule.DeclareLinkFile;
            archiverRule.EncryptionInfoId = rule.EncryptionInfoId;
            archiverRule.EncryptionInfoName = rule.EncryptionInfoName;
            archiverRule.EncryptionMethods = (Cloud.Sdk.Data.Dao.EncryptionMethods)rule.EncryptionMethods;
            archiverRule.ExportInfo = rule.ExportInfo == null ? null : ConvertToArchiverRuleExportInfo(rule.ExportInfo);
            archiverRule.ExportType = (Cloud.Sdk.Data.Dao.ExportTypeValue)rule.ExportType;
            archiverRule.FileVEO = rule.FileVEO;
            archiverRule.Filters = rule.Filters == null ? new List<Cloud.Sdk.Data.Dao.FilterPolicy>() : ConvertFilterPolicyToApiFilterPolicy(rule.Filters);
            archiverRule.Id = Guid.NewGuid().ToString();
            archiverRule.IncludeNew = rule.IncludeNew;
            archiverRule.IsCheckedCheckBox = rule.IsCheckedCheckBox;
            archiverRule.IsEnabledComboBox = rule.IsEnabledComboBox;
            archiverRule.IsManualApproval = rule.IsManualApproval;
            archiverRule.KeepDataOption = rule.KeepDataOption;
            archiverRule.KeepStructrue = rule.KeepStructrue;
            archiverRule.LogicalDeviceId = rule.LogicalDeviceId;
            archiverRule.LogicalDeviceName = rule.LogicalDeviceName;
            archiverRule.ManifestVEO = rule.ManifestVEO;
            archiverRule.ModifyTime = rule.ModifyTime;
            archiverRule.Module = rule.Module;
            archiverRule.MoveToRecordCenterAndDelareSetting = rule.MoveToRecordCenterAndDelareSetting == null ? null : ConvertToArchiverRuleMoveToRecordCenterAndDelareSetting(rule.MoveToRecordCenterAndDelareSetting);
            archiverRule.Name = rule.Name;
            archiverRule.NodeLevel = (Cloud.Sdk.Data.Dao.NodeLevel)rule.NodeLevel;
            archiverRule.NotToCheck = rule.NotToCheck;
            archiverRule.Order = rule.Order;
            archiverRule.OrderList = rule.OrderList == null ? new List<int>() : rule.OrderList;
            archiverRule.PolicyLevel = (Cloud.Sdk.Data.Dao.PolicyLevel)rule.PolicyLevel;
            archiverRule.ProfileInfo = rule.ProfileInfo;
            archiverRule.ProfileType = (Cloud.Sdk.Data.Dao.ProfileType)rule.ProfileType;
            archiverRule.RecordVEO = rule.RecordVEO;
            archiverRule.ExportDataEncryptionIV = rule.ExportDataEncryptionIV;
            archiverRule.ExportDataEncryptionKey = rule.ExportDataEncryptionKey;
            archiverRule.RuleScope = rule.RuleScope;
            archiverRule.RuleStatus = (Cloud.Sdk.Data.Dao.RuleStatus)rule.RuleStatus;
            archiverRule.SOFilters = ConvertToArchiverRuleSOFilterPolicy(rule.SOFilters);
            archiverRule.StoragePolicyId = rule.StoragePolicyId;
            archiverRule.StoragePolicyName = rule.StoragePolicyName;
            archiverRule.TagContentInfo = rule.TagContentInfo == null ? new List<Cloud.Sdk.Data.Dao.TagContentInfo>() : ConvertToArchiverRuleTagContentInfo(rule.TagContentInfo);
            archiverRule.Type = (Cloud.Sdk.Data.Dao.RuleType)rule.Type;
            archiverRule.UseSnapLock = rule.UseSnapLock;
            archiverRule.IsSendEmailToOwner = rule.IsSendEamilToOwner;
            archiverRule.UserInfos = ConvertToArchiverRuleUserInfo(rule.UserInfos);
            archiverRule.ReviewType = (Cloud.Sdk.Data.Dao.ReviewType)rule.ReviewType;
            archiverRule.WorkflowId = rule.WorkflowId;
            archiverRule.RelatedRecordOption = (Cloud.Sdk.Data.Dao.RelatedRecordOption)rule.RelatedRecordOption;
            archiverRule.spMoveOption = rule.spMoveOption == null ? null : ConvertToArchiverRuleMoveOption(rule.spMoveOption);
            archiverRule.LeaveStubMessage = rule.LeaveStubMessage;
            archiverRule.IsRestoreLink = rule.IsRestoreLink;
            archiverRule.IsEnableRetention = rule.IsEnableRetention;
            archiverRule.RetentionInfo = rule.RetentionInfo == null ? null : convertToRetentionInfo(rule.RetentionInfo);
            return archiverRule;
        }

        public static Rule ConvertArchiverRuleToSORule(Cloud.Sdk.Data.Dao.ArchiverRule archiverRule)
        {
            if (null == archiverRule) return null;
            Rule rule = new Rule();
            rule.AndOrExpression = archiverRule.AndOrExpression == null ? null : ConvertToSORulePolicyLevel(archiverRule.AndOrExpression);
            rule.ArchiverCompressionType = (AvePoint.GCommon.Contract.GranularBackup.Object.CompressionType)archiverRule.ArchiverCompressionType;
            rule.ArchiverDataSecurity = (AvePoint.GCommon.Contract.GranularBackup.Object.DataSecurity)archiverRule.ArchiverDataSecurity;
            rule.ArchiverSetting = (archiverRule.ArchiverSetting == null || string.IsNullOrEmpty(archiverRule.ArchiverSetting.FolderName)) ? null : ConvertToSORuleArchiverSetting(archiverRule.ArchiverSetting);
            rule.ArchiverVEOSetting = (archiverRule.ArchiverVEOSetting == null || string.IsNullOrEmpty(archiverRule.ArchiverVEOSetting.AgencyId)) ? null : ConvertToSORuleArchiverVEOSetting(archiverRule.ArchiverVEOSetting);
            rule.CheckStatus = (AvePoint.GCommon.Contract.StorageOptimization.Object.ActionStatus)archiverRule.CheckStatus;
            rule.Compression = archiverRule.Compression;
            rule.DataEncryptionProfileId = archiverRule.DataEncryptionProfileId;
            rule.DataEncryptionProfileName = archiverRule.DataEncryptionProfileName;
            rule.DataSecurity = archiverRule.DataSecurity;
            rule.Description = archiverRule.Description;
            rule.Detail = archiverRule.Detail;
            rule.Encryption = archiverRule.Encryption;
            rule.DisposalClass = archiverRule.DisposalClass;
            rule.NAAConfigFile = archiverRule.NAAConfigFile;
            rule.NARAConfigFile = archiverRule.NARAConfigFile;
            rule.DeleteRecords = archiverRule.DeleteRecords;
            rule.DeclareLinkFile = archiverRule.DeclareLinkFile;
            rule.EncryptionInfoId = archiverRule.EncryptionInfoId;
            rule.EncryptionInfoName = archiverRule.EncryptionInfoName;
            rule.EncryptionMethods = (AvePoint.GCommon.Contract.GranularBackup.Object.EncryptionMethods)archiverRule.EncryptionMethods;
            rule.ExportInfo = (archiverRule.ExportInfo == null || string.IsNullOrEmpty(archiverRule.ExportInfo.exportLocationId)) ? null : ConvertToSORuleExportInfo(archiverRule.ExportInfo);
            rule.ExportType = archiverRule.ExportType == 0 && archiverRule.ExportInfo != null ? (ExportTypeValue)archiverRule.ExportInfo.exportType : (ExportTypeValue)archiverRule.ExportType;
            rule.FileVEO = archiverRule.FileVEO;
            rule.Filters = archiverRule.Filters == null ? null : ConvertApiFilterPolicyToFilterPolicy(archiverRule.Filters);
            rule.Id = archiverRule.Id;
            rule.IncludeNew = archiverRule.IncludeNew;
            rule.IsCheckedCheckBox = archiverRule.IsCheckedCheckBox;
            rule.IsEnabledComboBox = archiverRule.IsEnabledComboBox;
            rule.IsManualApproval = archiverRule.IsManualApproval;
            rule.KeepDataOption = archiverRule.KeepDataOption;
            rule.KeepStructrue = archiverRule.KeepStructrue;
            rule.LogicalDeviceId = archiverRule.LogicalDeviceId;
            rule.LogicalDeviceName = archiverRule.LogicalDeviceName;
            rule.ManifestVEO = archiverRule.ManifestVEO;
            rule.ModifyTime = archiverRule.ModifyTime;
            rule.Module = archiverRule.Module;
            rule.MoveToRecordCenterAndDelareSetting = (archiverRule.MoveToRecordCenterAndDelareSetting == null || archiverRule.MoveToRecordCenterAndDelareSetting.DestinationLocation == null) ? null : ConvertToSORuleMoveToRecordCenterAndDelareSetting(archiverRule.MoveToRecordCenterAndDelareSetting);
            rule.Name = archiverRule.Name;
            rule.NodeLevel = (AvePoint.GCommon.Contract.Tree.Object.NodeLevel)archiverRule.NodeLevel;
            rule.NotToCheck = archiverRule.NotToCheck;
            rule.Order = archiverRule.Order;
            rule.OrderList = archiverRule.OrderList;
            rule.PolicyLevel = (AvePoint.GCommon.Contract.CommonFilter.PolicyLevel)archiverRule.PolicyLevel;
            rule.ProfileInfo = archiverRule.ProfileInfo;
            rule.ProfileType = (AvePoint.GCommon.Contract.Server.Common.Profile.Object.ProfileType)archiverRule.ProfileType;
            rule.RecordVEO = archiverRule.RecordVEO;
            rule.ExportDataEncryptionIV = archiverRule.ExportDataEncryptionIV;
            rule.ExportDataEncryptionKey = archiverRule.ExportDataEncryptionKey;
            rule.RuleScope = archiverRule.RuleScope;
            rule.RuleStatus = (AvePoint.GCommon.Contract.StorageOptimization.Object.RuleStatus)archiverRule.RuleStatus;
            rule.SOFilters = archiverRule.SOFilters == null ? null : ConvertToSORuleSOFilterPolicy(archiverRule.SOFilters);
            rule.StoragePolicyId = archiverRule.StoragePolicyId;
            rule.StoragePolicyName = archiverRule.StoragePolicyName;
            rule.TagContentInfo = (archiverRule.TagContentInfo == null || archiverRule.TagContentInfo.Count == 0) ? null : ConvertToSORuleTagContentInfo(archiverRule.TagContentInfo);
            rule.Type = (AvePoint.GCommon.Contract.StorageOptimization.Object.RuleType)archiverRule.Type;
            rule.UseSnapLock = archiverRule.UseSnapLock;
            rule.IsSendEamilToOwner = archiverRule.IsSendEmailToOwner;
            rule.UserInfos = ConvertToSORuleUserInfo(archiverRule.UserInfos);
            rule.ReviewType = (AvePoint.GCommon.Contract.StorageOptimization.Object.ReviewType)archiverRule.ReviewType;
            rule.WorkflowId = archiverRule.WorkflowId;
            rule.RelatedRecordOption = (AvePoint.GCommon.Contract.StorageOptimization.Object.RelatedRecordOption)archiverRule.RelatedRecordOption;
            rule.LeaveStubType = (LeaveStubType)archiverRule.LeaveStubType;
            rule.LeaveStubMessage = archiverRule.LeaveStubMessage;
            rule.StubTemplateId = archiverRule.StubTemplateId;
            rule.IsFileName = archiverRule.IsFileName;
            rule.IsFilePath = archiverRule.IsFilePath;
            rule.IsArchivedDate = archiverRule.IsArchivedDate;
            rule.IsRuleName = archiverRule.IsRuleName;
            rule.IsRestoreLink = archiverRule.IsRestoreLink;
            rule.IsEnableRetention = archiverRule.IsEnableRetention;
            rule.RetentionInfo = archiverRule.RetentionInfo == null ? null : ConvertToRetentionInfo(archiverRule.RetentionInfo);
            rule.spMoveOption = archiverRule.spMoveOption == null ? null : ConvertToSORuleMoveOption(archiverRule.spMoveOption);
            if (!string.IsNullOrEmpty(archiverRule.EXORuleString))
            {
                //var exoRule = SerializerHelper.DeserializeByDataContractSerializer<DocAveOnline.WebApi.Contracts.ArchiverRule>(archiverRule.EXORuleString);
                ////DeserializeFromXmlString<Cloud.Sdk.Data.Dao.ArchiverRule>(archiverRule.EXORuleString);
                ////SerializerHelper.DeserializeByDataContractSerializer<DocAveOnline.WebApi.Contracts.ArchiverRule>(archiverRule.EXORuleString);
                //rule.EXORule = ConvertUtility.ConvertArchiverRuleToEXORule(exoRule);

                var exoRule = DeserializeByJsonConvertWithReference<Cloud.Sdk.Data.Dao.ArchiverRule>(archiverRule.EXORuleString);
                rule.EXORule = ConvertArchiverRuleToEXORule(exoRule);

            }
            else
            {
                rule.EXORule = null;
            }
            if (!string.IsNullOrEmpty(archiverRule.PhysicalRuleString))
            {
                //archiverRule.PhysicalRule = SerializerHelper.DeserializeByDataContractSerializer<Cloud.Sdk.Data.Dao.ArchiverRule>(archiverRule.PhysicalRuleString);
                //rule.PhysicalRule = ConvertArchiverRuleToPhysicalRule(archiverRule.PhysicalRule);
                //var physicalRule = SerializerHelper.DeserializeByDataContractSerializer<DocAveOnline.WebApi.Contracts.ArchiverRule>(archiverRule.PhysicalRuleString);
                rule.PhysicalRule = SerializerHelper.DeserializeByDataContractSerializer<Rule>(archiverRule.PhysicalRuleString);
              
            }
            else
            {
                rule.PhysicalRule = null;
            }
            if (!string.IsNullOrEmpty(archiverRule.FSRuleString))
            {
                //var fsRule = SerializerHelper.DeserializeByDataContractSerializer<DocAveOnline.WebApi.Contracts.ArchiverRule>(archiverRule.FSRuleString);
                ////SerializerHelper.DeserializeByDataContractSerializer<DocAveOnline.WebApi.Contracts.ArchiverRule>(archiverRule.FSRuleString);
                //rule.FSRule = ConvertUtility.ConvertArchiverRuleToFSRule(fsRule);

                var fsRule = DeserializeByJsonConvertWithReference<Cloud.Sdk.Data.Dao.ArchiverRule>(archiverRule.FSRuleString);
                rule.FSRule = ConvertArchiverRuleToFSRule(fsRule);
            }
            else
            {
                rule.FSRule = null;
            }
            if (!string.IsNullOrEmpty(archiverRule.SPLocalRuleString))
            {
                //var spLocal = SerializerHelper.DeserializeByDataContractSerializer<DocAveOnline.WebApi.Contracts.ArchiverRule>(archiverRule.SPLocalRuleString);
                ////SerializerHelper.DeserializeByDataContractSerializer<DocAveOnline.WebApi.Contracts.ArchiverRule>(archiverRule.SPLocalRuleString);
                //rule.SPLocalRule = ConvertUtility.ConvertArchiverRuleToSPLocalRule(spLocal);

                var spLocal = DeserializeByJsonConvertWithReference<Cloud.Sdk.Data.Dao.ArchiverRule>(archiverRule.SPLocalRuleString);
                //SerializerHelper.DeserializeByDataContractSerializer<DocAveOnline.WebApi.Contracts.ArchiverRule>(archiverRule.SPLocalRuleString);
                rule.SPLocalRule = ConvertArchiverRuleToSPLocalRule(spLocal);
            }
            else
            {
                rule.SPLocalRule = null;
            }
            if (!string.IsNullOrEmpty(archiverRule.OneDriveRuleString))
            {
                //var oneDrive = SerializerHelper.DeserializeByDataContractSerializer<DocAveOnline.WebApi.Contracts.ArchiverRule>(archiverRule.OneDriveRuleString);
                ////SerializerHelper.DeserializeByDataContractSerializer<DocAveOnline.WebApi.Contracts.ArchiverRule>(archiverRule.OneDriveRuleString);
                //rule.OneDriveRule = ConvertUtility.ConvertArchiverRuleToOneDriveRule(oneDrive);

                var oneDrive = DeserializeByJsonConvertWithReference<Cloud.Sdk.Data.Dao.ArchiverRule>(archiverRule.OneDriveRuleString);
                //SerializerHelper.DeserializeByDataContractSerializer<DocAveOnline.WebApi.Contracts.ArchiverRule>(archiverRule.OneDriveRuleString);
                rule.OneDriveRule = ConvertArchiverRuleToOneDriveRule(oneDrive);
            }
            else
            {
                rule.OneDriveRule = null;
            }
            if (!string.IsNullOrEmpty(archiverRule.AzureFileRuleString))
            {
                //var azureFile = SerializerHelper.DeserializeByDataContractSerializer<DocAveOnline.WebApi.Contracts.ArchiverRule>(archiverRule.AzureFileRuleString);
                ////SerializerHelper.DeserializeByDataContractSerializer<DocAveOnline.WebApi.Contracts.ArchiverRule>(archiverRule.AzureFileRuleString);
                //rule.AzureFileRule = ConvertUtility.ConvertArchiverRuleToAzureFileRule(azureFile);

                var azureFile = DeserializeByJsonConvertWithReference<Cloud.Sdk.Data.Dao.ArchiverRule>(archiverRule.AzureFileRuleString);
                //SerializerHelper.DeserializeByDataContractSerializer<DocAveOnline.WebApi.Contracts.ArchiverRule>(archiverRule.AzureFileRuleString);
                rule.AzureFileRule = ConvertArchiverRuleToAzureFileRule(azureFile);
            }
            else
            {
                rule.AzureFileRule = null;
            }

            if (!string.IsNullOrEmpty(archiverRule.ConnectorRuleString))
            {
                //var azureFile = SerializerHelper.DeserializeByDataContractSerializer<DocAveOnline.WebApi.Contracts.ArchiverRule>(archiverRule.AzureFileRuleString);
                ////SerializerHelper.DeserializeByDataContractSerializer<DocAveOnline.WebApi.Contracts.ArchiverRule>(archiverRule.AzureFileRuleString);
                //rule.AzureFileRule = ConvertUtility.ConvertArchiverRuleToAzureFileRule(azureFile);

                var connectorFile = DeserializeByJsonConvertWithReference<Cloud.Sdk.Data.Dao.ArchiverRule>(archiverRule.ConnectorRuleString);
                //SerializerHelper.DeserializeByDataContractSerializer<DocAveOnline.WebApi.Contracts.ArchiverRule>(archiverRule.AzureFileRuleString);
                rule.ConnectorRule = ConvertArchiverRuleToConnectorRule(connectorFile);
            }
            else
            {
                rule.ConnectorRule = null;
            }

            if (!string.IsNullOrEmpty(archiverRule.BoxRuleString))
            {
                //var boxRule = SerializerHelper.DeserializeByDataContractSerializer<DocAveOnline.WebApi.Contracts.ArchiverRule>(archiverRule.BoxRuleString);
                ////SerializerHelper.DeserializeByDataContractSerializer<DocAveOnline.WebApi.Contracts.ArchiverRule>(archiverRule.BoxRuleString);
                //rule.BoxRule = ConvertUtility.ConvertArchiverRuleToBoxRule(boxRule);

                var boxRule = DeserializeByJsonConvertWithReference<Cloud.Sdk.Data.Dao.ArchiverRule>(archiverRule.BoxRuleString);
                //SerializerHelper.DeserializeByDataContractSerializer<DocAveOnline.WebApi.Contracts.ArchiverRule>(archiverRule.BoxRuleString);
                rule.BoxRule = ConvertArchiverRuleToBoxRule(boxRule);
            }
            else
            {
                rule.BoxRule = null;
            }
            return rule;
        }

        public static T DeserializeByJsonConvertWithReference<T>(string data)
        {
            var settings = new JsonSerializerSettings() { ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Serialize, DefaultValueHandling = DefaultValueHandling.Ignore };
            return JsonConvert.DeserializeObject<T>(data, settings);

        }

        public static Rule ConvertArchiverRuleToFSRule(Cloud.Sdk.Data.Dao.ArchiverRule archiverRule)
        {
            if (null == archiverRule) return null;
            Rule rule = new Rule();
            rule.AndOrExpression = archiverRule.AndOrExpression == null ? null : ConvertToSORulePolicyLevel(archiverRule.AndOrExpression);
            rule.ArchiverCompressionType = (AvePoint.GCommon.Contract.GranularBackup.Object.CompressionType)archiverRule.ArchiverCompressionType;
            rule.ArchiverDataSecurity = (AvePoint.GCommon.Contract.GranularBackup.Object.DataSecurity)archiverRule.ArchiverDataSecurity;
            rule.ArchiverSetting = (archiverRule.ArchiverSetting == null || string.IsNullOrEmpty(archiverRule.ArchiverSetting.FolderName)) ? null : ConvertToSORuleArchiverSetting(archiverRule.ArchiverSetting);
            rule.ArchiverVEOSetting = (archiverRule.ArchiverVEOSetting == null || string.IsNullOrEmpty(archiverRule.ArchiverVEOSetting.AgencyId)) ? null : ConvertToSORuleArchiverVEOSetting(archiverRule.ArchiverVEOSetting);
            rule.CheckStatus = (AvePoint.GCommon.Contract.StorageOptimization.Object.ActionStatus)archiverRule.CheckStatus;
            rule.Compression = archiverRule.Compression;
            rule.DataEncryptionProfileId = archiverRule.DataEncryptionProfileId;
            rule.DataEncryptionProfileName = archiverRule.DataEncryptionProfileName;
            rule.DataSecurity = archiverRule.DataSecurity;
            rule.Description = archiverRule.Description;
            rule.Detail = archiverRule.Detail;
            rule.Encryption = archiverRule.Encryption;
            rule.DisposalClass = archiverRule.DisposalClass;
            rule.NAAConfigFile = archiverRule.NAAConfigFile;
            rule.NARAConfigFile = archiverRule.NARAConfigFile;
            rule.DeleteRecords = archiverRule.DeleteRecords;
            rule.DeclareLinkFile = archiverRule.DeclareLinkFile;
            rule.EncryptionInfoId = archiverRule.EncryptionInfoId;
            rule.EncryptionInfoName = archiverRule.EncryptionInfoName;
            rule.EncryptionMethods = (AvePoint.GCommon.Contract.GranularBackup.Object.EncryptionMethods)archiverRule.EncryptionMethods;
            rule.ExportInfo = (archiverRule.ExportInfo == null || string.IsNullOrEmpty(archiverRule.ExportInfo.exportLocationId)) ? null : ConvertToSORuleExportInfo(archiverRule.ExportInfo);
            rule.ExportType = (AvePoint.GCommon.Contract.StorageOptimization.Object.ExportTypeValue)archiverRule.ExportType;
            rule.FileVEO = archiverRule.FileVEO;
            rule.Filters = archiverRule.Filters == null ? null : ConvertApiFilterPolicyToFilterPolicy(archiverRule.Filters);
            rule.Id = archiverRule.Id;
            rule.IncludeNew = archiverRule.IncludeNew;
            rule.IsCheckedCheckBox = archiverRule.IsCheckedCheckBox;
            rule.IsEnabledComboBox = archiverRule.IsEnabledComboBox;
            rule.IsManualApproval = archiverRule.IsManualApproval;
            rule.KeepDataOption = archiverRule.KeepDataOption;
            rule.KeepStructrue = archiverRule.KeepStructrue;
            rule.LogicalDeviceId = archiverRule.LogicalDeviceId;
            rule.LogicalDeviceName = archiverRule.LogicalDeviceName;
            rule.ManifestVEO = archiverRule.ManifestVEO;
            rule.ModifyTime = archiverRule.ModifyTime;
            rule.Module = archiverRule.Module;
            rule.MoveToRecordCenterAndDelareSetting = (archiverRule.MoveToRecordCenterAndDelareSetting == null || archiverRule.MoveToRecordCenterAndDelareSetting.DestinationLocation == null) ? null : ConvertToSORuleMoveToRecordCenterAndDelareSetting(archiverRule.MoveToRecordCenterAndDelareSetting);
            rule.Name = archiverRule.Name;
            rule.NodeLevel = (AvePoint.GCommon.Contract.Tree.Object.NodeLevel)archiverRule.NodeLevel;
            rule.NotToCheck = archiverRule.NotToCheck;
            rule.Order = archiverRule.Order;
            rule.OrderList = archiverRule.OrderList;
            rule.PolicyLevel = (AvePoint.GCommon.Contract.CommonFilter.PolicyLevel)archiverRule.PolicyLevel;
            rule.ProfileInfo = archiverRule.ProfileInfo;
            rule.ProfileType = (AvePoint.GCommon.Contract.Server.Common.Profile.Object.ProfileType)archiverRule.ProfileType;
            rule.RecordVEO = archiverRule.RecordVEO;
            rule.ExportDataEncryptionIV = archiverRule.ExportDataEncryptionIV;
            rule.ExportDataEncryptionKey = archiverRule.ExportDataEncryptionKey;
            rule.RuleScope = archiverRule.RuleScope;
            rule.RuleStatus = (AvePoint.GCommon.Contract.StorageOptimization.Object.RuleStatus)archiverRule.RuleStatus;
            rule.SOFilters = ConvertToSORuleSOFilterPolicy(archiverRule.SOFilters);
            rule.StoragePolicyId = archiverRule.StoragePolicyId;
            rule.StoragePolicyName = archiverRule.StoragePolicyName;
            rule.TagContentInfo = (archiverRule.TagContentInfo == null || archiverRule.TagContentInfo.Count == 0) ? null : ConvertToSORuleTagContentInfo(archiverRule.TagContentInfo);
            rule.Type = (AvePoint.GCommon.Contract.StorageOptimization.Object.RuleType)archiverRule.Type;
            rule.UseSnapLock = archiverRule.UseSnapLock;
            rule.IsSendEamilToOwner = archiverRule.IsSendEmailToOwner;
            rule.UserInfos = ConvertToSORuleUserInfo(archiverRule.UserInfos);
            rule.ReviewType = (AvePoint.GCommon.Contract.StorageOptimization.Object.ReviewType)archiverRule.ReviewType;
            rule.WorkflowId = archiverRule.WorkflowId;
            rule.RelatedRecordOption = (AvePoint.GCommon.Contract.StorageOptimization.Object.RelatedRecordOption)archiverRule.RelatedRecordOption;
            rule.spMoveOption = archiverRule.spMoveOption == null ? null : ConvertToSORuleFSMoveOption(archiverRule.spMoveOption);
            return rule;
        }
        public static Rule ConvertArchiverRuleToSPLocalRule(Cloud.Sdk.Data.Dao.ArchiverRule archiverRule)
        {
            if (null == archiverRule) return null;
            Rule rule = new Rule();
            rule.AndOrExpression = archiverRule.AndOrExpression == null ? null : ConvertToSORulePolicyLevel(archiverRule.AndOrExpression);
            rule.ArchiverCompressionType = (AvePoint.GCommon.Contract.GranularBackup.Object.CompressionType)archiverRule.ArchiverCompressionType;
            rule.ArchiverDataSecurity = (AvePoint.GCommon.Contract.GranularBackup.Object.DataSecurity)archiverRule.ArchiverDataSecurity;
            rule.ArchiverSetting = (archiverRule.ArchiverSetting == null || string.IsNullOrEmpty(archiverRule.ArchiverSetting.FolderName)) ? null : ConvertToSORuleArchiverSetting(archiverRule.ArchiverSetting);
            rule.ArchiverVEOSetting = (archiverRule.ArchiverVEOSetting == null || string.IsNullOrEmpty(archiverRule.ArchiverVEOSetting.AgencyId)) ? null : ConvertToSORuleArchiverVEOSetting(archiverRule.ArchiverVEOSetting);
            rule.CheckStatus = (AvePoint.GCommon.Contract.StorageOptimization.Object.ActionStatus)archiverRule.CheckStatus;
            rule.Compression = archiverRule.Compression;
            rule.DataEncryptionProfileId = archiverRule.DataEncryptionProfileId;
            rule.DataEncryptionProfileName = archiverRule.DataEncryptionProfileName;
            rule.DataSecurity = archiverRule.DataSecurity;
            rule.Description = archiverRule.Description;
            rule.Detail = archiverRule.Detail;
            rule.Encryption = archiverRule.Encryption;
            rule.DisposalClass = archiverRule.DisposalClass;
            rule.NAAConfigFile = archiverRule.NAAConfigFile;
            rule.NARAConfigFile = archiverRule.NARAConfigFile;
            rule.DeleteRecords = archiverRule.DeleteRecords;
            rule.DeclareLinkFile = archiverRule.DeclareLinkFile;
            rule.EncryptionInfoId = archiverRule.EncryptionInfoId;
            rule.EncryptionInfoName = archiverRule.EncryptionInfoName;
            rule.EncryptionMethods = (AvePoint.GCommon.Contract.GranularBackup.Object.EncryptionMethods)archiverRule.EncryptionMethods;
            rule.ExportInfo = (archiverRule.ExportInfo == null || string.IsNullOrEmpty(archiverRule.ExportInfo.exportLocationId)) ? null : ConvertToSORuleExportInfo(archiverRule.ExportInfo);
            rule.ExportType = (AvePoint.GCommon.Contract.StorageOptimization.Object.ExportTypeValue)archiverRule.ExportType;
            rule.FileVEO = archiverRule.FileVEO;
            rule.Filters = archiverRule.Filters == null ? null : ConvertApiFilterPolicyToFilterPolicy(archiverRule.Filters);
            rule.Id = archiverRule.Id;
            rule.IncludeNew = archiverRule.IncludeNew;
            rule.IsCheckedCheckBox = archiverRule.IsCheckedCheckBox;
            rule.IsEnabledComboBox = archiverRule.IsEnabledComboBox;
            rule.IsManualApproval = archiverRule.IsManualApproval;
            rule.KeepDataOption = archiverRule.KeepDataOption;
            rule.KeepStructrue = archiverRule.KeepStructrue;
            rule.LogicalDeviceId = archiverRule.LogicalDeviceId;
            rule.LogicalDeviceName = archiverRule.LogicalDeviceName;
            rule.ManifestVEO = archiverRule.ManifestVEO;
            rule.ModifyTime = archiverRule.ModifyTime;
            rule.Module = archiverRule.Module;
            rule.MoveToRecordCenterAndDelareSetting = (archiverRule.MoveToRecordCenterAndDelareSetting == null || archiverRule.MoveToRecordCenterAndDelareSetting.DestinationLocation == null) ? null : ConvertToSORuleMoveToRecordCenterAndDelareSetting(archiverRule.MoveToRecordCenterAndDelareSetting);
            rule.Name = archiverRule.Name;
            rule.NodeLevel = (AvePoint.GCommon.Contract.Tree.Object.NodeLevel)archiverRule.NodeLevel;
            rule.NotToCheck = archiverRule.NotToCheck;
            rule.Order = archiverRule.Order;
            rule.OrderList = archiverRule.OrderList;
            rule.PolicyLevel = (AvePoint.GCommon.Contract.CommonFilter.PolicyLevel)archiverRule.PolicyLevel;
            rule.ProfileInfo = archiverRule.ProfileInfo;
            rule.ProfileType = (AvePoint.GCommon.Contract.Server.Common.Profile.Object.ProfileType)archiverRule.ProfileType;
            rule.RecordVEO = archiverRule.RecordVEO;
            rule.ExportDataEncryptionIV = archiverRule.ExportDataEncryptionIV;
            rule.ExportDataEncryptionKey = archiverRule.ExportDataEncryptionKey;
            rule.RuleScope = archiverRule.RuleScope;
            rule.RuleStatus = (AvePoint.GCommon.Contract.StorageOptimization.Object.RuleStatus)archiverRule.RuleStatus;
            rule.SOFilters = archiverRule.SOFilters == null ? null : ConvertToSORuleSOFilterPolicy(archiverRule.SOFilters);
            rule.StoragePolicyId = archiverRule.StoragePolicyId;
            rule.StoragePolicyName = archiverRule.StoragePolicyName;
            rule.TagContentInfo = (archiverRule.TagContentInfo == null || archiverRule.TagContentInfo.Count == 0) ? null : ConvertToSORuleTagContentInfo(archiverRule.TagContentInfo);
            rule.Type = (AvePoint.GCommon.Contract.StorageOptimization.Object.RuleType)archiverRule.Type;
            rule.UseSnapLock = archiverRule.UseSnapLock;
            rule.IsSendEamilToOwner = archiverRule.IsSendEmailToOwner;
            rule.UserInfos = ConvertToSORuleUserInfo(archiverRule.UserInfos);
            rule.ReviewType = (AvePoint.GCommon.Contract.StorageOptimization.Object.ReviewType)archiverRule.ReviewType;
            rule.WorkflowId = archiverRule.WorkflowId;
            rule.RelatedRecordOption = (AvePoint.GCommon.Contract.StorageOptimization.Object.RelatedRecordOption)archiverRule.RelatedRecordOption;
            rule.spMoveOption = archiverRule.spMoveOption == null ? null : ConvertToSORuleMoveOption(archiverRule.spMoveOption);
            return rule;
        }
        public static Rule ConvertArchiverRuleToOneDriveRule(Cloud.Sdk.Data.Dao.ArchiverRule archiverRule)
        {
            if (null == archiverRule) return null;
            Rule rule = new Rule();
            rule.AndOrExpression = archiverRule.AndOrExpression == null ? null : ConvertToSORulePolicyLevel(archiverRule.AndOrExpression);
            rule.ArchiverCompressionType = (AvePoint.GCommon.Contract.GranularBackup.Object.CompressionType)archiverRule.ArchiverCompressionType;
            rule.ArchiverDataSecurity = (AvePoint.GCommon.Contract.GranularBackup.Object.DataSecurity)archiverRule.ArchiverDataSecurity;
            rule.ArchiverSetting = (archiverRule.ArchiverSetting == null || string.IsNullOrEmpty(archiverRule.ArchiverSetting.FolderName)) ? null : ConvertToSORuleArchiverSetting(archiverRule.ArchiverSetting);
            rule.ArchiverVEOSetting = (archiverRule.ArchiverVEOSetting == null || string.IsNullOrEmpty(archiverRule.ArchiverVEOSetting.AgencyId)) ? null : ConvertToSORuleArchiverVEOSetting(archiverRule.ArchiverVEOSetting);
            rule.CheckStatus = (AvePoint.GCommon.Contract.StorageOptimization.Object.ActionStatus)archiverRule.CheckStatus;
            rule.Compression = archiverRule.Compression;
            rule.DataEncryptionProfileId = archiverRule.DataEncryptionProfileId;
            rule.DataEncryptionProfileName = archiverRule.DataEncryptionProfileName;
            rule.DataSecurity = archiverRule.DataSecurity;
            rule.Description = archiverRule.Description;
            rule.Detail = archiverRule.Detail;
            rule.Encryption = archiverRule.Encryption;
            rule.DisposalClass = archiverRule.DisposalClass;
            rule.NAAConfigFile = archiverRule.NAAConfigFile;
            rule.NARAConfigFile = archiverRule.NARAConfigFile;
            rule.DeleteRecords = archiverRule.DeleteRecords;
            rule.DeclareLinkFile = archiverRule.DeclareLinkFile;
            rule.EncryptionInfoId = archiverRule.EncryptionInfoId;
            rule.EncryptionInfoName = archiverRule.EncryptionInfoName;
            rule.EncryptionMethods = (AvePoint.GCommon.Contract.GranularBackup.Object.EncryptionMethods)archiverRule.EncryptionMethods;
            rule.ExportInfo = (archiverRule.ExportInfo == null || string.IsNullOrEmpty(archiverRule.ExportInfo.exportLocationId)) ? null : ConvertToSORuleExportInfo(archiverRule.ExportInfo);
            rule.ExportType = (AvePoint.GCommon.Contract.StorageOptimization.Object.ExportTypeValue)archiverRule.ExportType;
            rule.FileVEO = archiverRule.FileVEO;
            rule.Filters = archiverRule.Filters == null ? null : ConvertApiFilterPolicyToFilterPolicy(archiverRule.Filters);
            rule.Id = archiverRule.Id;
            rule.IncludeNew = archiverRule.IncludeNew;
            rule.IsCheckedCheckBox = archiverRule.IsCheckedCheckBox;
            rule.IsEnabledComboBox = archiverRule.IsEnabledComboBox;
            rule.IsManualApproval = archiverRule.IsManualApproval;
            rule.KeepDataOption = archiverRule.KeepDataOption;
            rule.KeepStructrue = archiverRule.KeepStructrue;
            rule.LogicalDeviceId = archiverRule.LogicalDeviceId;
            rule.LogicalDeviceName = archiverRule.LogicalDeviceName;
            rule.ManifestVEO = archiverRule.ManifestVEO;
            rule.ModifyTime = archiverRule.ModifyTime;
            rule.Module = archiverRule.Module;
            rule.MoveToRecordCenterAndDelareSetting = (archiverRule.MoveToRecordCenterAndDelareSetting == null || archiverRule.MoveToRecordCenterAndDelareSetting.DestinationLocation == null) ? null : ConvertToSORuleMoveToRecordCenterAndDelareSetting(archiverRule.MoveToRecordCenterAndDelareSetting);
            rule.Name = archiverRule.Name;
            rule.NodeLevel = (AvePoint.GCommon.Contract.Tree.Object.NodeLevel)archiverRule.NodeLevel;
            rule.NotToCheck = archiverRule.NotToCheck;
            rule.Order = archiverRule.Order;
            rule.OrderList = archiverRule.OrderList;
            rule.PolicyLevel = (AvePoint.GCommon.Contract.CommonFilter.PolicyLevel)archiverRule.PolicyLevel;
            rule.ProfileInfo = archiverRule.ProfileInfo;
            rule.ProfileType = (AvePoint.GCommon.Contract.Server.Common.Profile.Object.ProfileType)archiverRule.ProfileType;
            rule.RecordVEO = archiverRule.RecordVEO;
            rule.ExportDataEncryptionIV = archiverRule.ExportDataEncryptionIV;
            rule.ExportDataEncryptionKey = archiverRule.ExportDataEncryptionKey;
            rule.RuleScope = archiverRule.RuleScope;
            rule.RuleStatus = (AvePoint.GCommon.Contract.StorageOptimization.Object.RuleStatus)archiverRule.RuleStatus;
            rule.SOFilters = archiverRule.SOFilters == null ? null : ConvertToSORuleSOFilterPolicy(archiverRule.SOFilters);
            rule.StoragePolicyId = archiverRule.StoragePolicyId;
            rule.StoragePolicyName = archiverRule.StoragePolicyName;
            rule.TagContentInfo = (archiverRule.TagContentInfo == null || archiverRule.TagContentInfo.Count == 0) ? null : ConvertToSORuleTagContentInfo(archiverRule.TagContentInfo);
            rule.Type = (AvePoint.GCommon.Contract.StorageOptimization.Object.RuleType)archiverRule.Type;
            rule.UseSnapLock = archiverRule.UseSnapLock;
            rule.IsSendEamilToOwner = archiverRule.IsSendEmailToOwner;
            rule.UserInfos = ConvertToSORuleUserInfo(archiverRule.UserInfos);
            rule.ReviewType = (AvePoint.GCommon.Contract.StorageOptimization.Object.ReviewType)archiverRule.ReviewType;
            rule.WorkflowId = archiverRule.WorkflowId;
            rule.RelatedRecordOption = (AvePoint.GCommon.Contract.StorageOptimization.Object.RelatedRecordOption)archiverRule.RelatedRecordOption;
            rule.spMoveOption = archiverRule.spMoveOption == null ? null : ConvertToSORuleMoveOption(archiverRule.spMoveOption);
            rule.LeaveStubType = (LeaveStubType)archiverRule.LeaveStubType;
            rule.LeaveStubMessage = archiverRule.LeaveStubMessage;
            rule.StubTemplateId = archiverRule.StubTemplateId;
            rule.IsFileName = archiverRule.IsFileName;
            rule.IsFilePath = archiverRule.IsFilePath;
            rule.IsArchivedDate = archiverRule.IsArchivedDate;
            rule.IsRuleName = archiverRule.IsRuleName;
            rule.IsRestoreLink = archiverRule.IsRestoreLink;
            rule.IsEnableRetention = archiverRule.IsEnableRetention;
            rule.RetentionInfo = archiverRule.RetentionInfo == null ? null : ConvertToRetentionInfo(archiverRule.RetentionInfo);
            return rule;
        }

        public static Rule ConvertArchiverRuleToAzureFileRule(Cloud.Sdk.Data.Dao.ArchiverRule archiverRule)
        {
            if (null == archiverRule) return null;
            Rule rule = new Rule();
            rule.AndOrExpression = archiverRule.AndOrExpression == null ? null : ConvertToSORulePolicyLevel(archiverRule.AndOrExpression);
            rule.ArchiverCompressionType = (AvePoint.GCommon.Contract.GranularBackup.Object.CompressionType)archiverRule.ArchiverCompressionType;
            rule.ArchiverDataSecurity = (AvePoint.GCommon.Contract.GranularBackup.Object.DataSecurity)archiverRule.ArchiverDataSecurity;
            rule.ArchiverSetting = (archiverRule.ArchiverSetting == null || string.IsNullOrEmpty(archiverRule.ArchiverSetting.FolderName)) ? null : ConvertToSORuleArchiverSetting(archiverRule.ArchiverSetting);
            rule.ArchiverVEOSetting = (archiverRule.ArchiverVEOSetting == null || string.IsNullOrEmpty(archiverRule.ArchiverVEOSetting.AgencyId)) ? null : ConvertToSORuleArchiverVEOSetting(archiverRule.ArchiverVEOSetting);
            rule.CheckStatus = (AvePoint.GCommon.Contract.StorageOptimization.Object.ActionStatus)archiverRule.CheckStatus;
            rule.Compression = archiverRule.Compression;
            rule.DataEncryptionProfileId = archiverRule.DataEncryptionProfileId;
            rule.DataEncryptionProfileName = archiverRule.DataEncryptionProfileName;
            rule.DataSecurity = archiverRule.DataSecurity;
            rule.Description = archiverRule.Description;
            rule.Detail = archiverRule.Detail;
            rule.Encryption = archiverRule.Encryption;
            rule.DisposalClass = archiverRule.DisposalClass;
            rule.NAAConfigFile = archiverRule.NAAConfigFile;
            rule.NARAConfigFile = archiverRule.NARAConfigFile;
            rule.DeleteRecords = archiverRule.DeleteRecords;
            rule.DeclareLinkFile = archiverRule.DeclareLinkFile;
            rule.EncryptionInfoId = archiverRule.EncryptionInfoId;
            rule.EncryptionInfoName = archiverRule.EncryptionInfoName;
            rule.EncryptionMethods = (AvePoint.GCommon.Contract.GranularBackup.Object.EncryptionMethods)archiverRule.EncryptionMethods;
            rule.ExportInfo = (archiverRule.ExportInfo == null || string.IsNullOrEmpty(archiverRule.ExportInfo.exportLocationId)) ? null : ConvertToSORuleExportInfo(archiverRule.ExportInfo);
            rule.ExportType = (AvePoint.GCommon.Contract.StorageOptimization.Object.ExportTypeValue)archiverRule.ExportType;
            rule.FileVEO = archiverRule.FileVEO;
            rule.Filters = archiverRule.Filters == null ? null : ConvertApiFilterPolicyToFilterPolicy(archiverRule.Filters);
            rule.Id = archiverRule.Id;
            rule.IncludeNew = archiverRule.IncludeNew;
            rule.IsCheckedCheckBox = archiverRule.IsCheckedCheckBox;
            rule.IsEnabledComboBox = archiverRule.IsEnabledComboBox;
            rule.IsManualApproval = archiverRule.IsManualApproval;
            rule.KeepDataOption = archiverRule.KeepDataOption;
            rule.KeepStructrue = archiverRule.KeepStructrue;
            rule.LogicalDeviceId = archiverRule.LogicalDeviceId;
            rule.LogicalDeviceName = archiverRule.LogicalDeviceName;
            rule.ManifestVEO = archiverRule.ManifestVEO;
            rule.ModifyTime = archiverRule.ModifyTime;
            rule.Module = archiverRule.Module;
            rule.MoveToRecordCenterAndDelareSetting = (archiverRule.MoveToRecordCenterAndDelareSetting == null || archiverRule.MoveToRecordCenterAndDelareSetting.DestinationLocation == null) ? null : ConvertToSORuleMoveToRecordCenterAndDelareSetting(archiverRule.MoveToRecordCenterAndDelareSetting);
            rule.Name = archiverRule.Name;
            rule.NodeLevel = (AvePoint.GCommon.Contract.Tree.Object.NodeLevel)archiverRule.NodeLevel;
            rule.NotToCheck = archiverRule.NotToCheck;
            rule.Order = archiverRule.Order;
            rule.OrderList = archiverRule.OrderList;
            rule.PolicyLevel = (AvePoint.GCommon.Contract.CommonFilter.PolicyLevel)archiverRule.PolicyLevel;
            rule.ProfileInfo = archiverRule.ProfileInfo;
            rule.ProfileType = (AvePoint.GCommon.Contract.Server.Common.Profile.Object.ProfileType)archiverRule.ProfileType;
            rule.RecordVEO = archiverRule.RecordVEO;
            rule.ExportDataEncryptionIV = archiverRule.ExportDataEncryptionIV;
            rule.ExportDataEncryptionKey = archiverRule.ExportDataEncryptionKey;
            rule.RuleScope = archiverRule.RuleScope;
            rule.RuleStatus = (AvePoint.GCommon.Contract.StorageOptimization.Object.RuleStatus)archiverRule.RuleStatus;
            rule.SOFilters = archiverRule.SOFilters == null ? null : ConvertToSORuleSOFilterPolicy(archiverRule.SOFilters);
            rule.StoragePolicyId = archiverRule.StoragePolicyId;
            rule.StoragePolicyName = archiverRule.StoragePolicyName;
            rule.TagContentInfo = (archiverRule.TagContentInfo == null || archiverRule.TagContentInfo.Count == 0) ? null : ConvertToSORuleTagContentInfo(archiverRule.TagContentInfo);
            rule.Type = (AvePoint.GCommon.Contract.StorageOptimization.Object.RuleType)archiverRule.Type;
            rule.UseSnapLock = archiverRule.UseSnapLock;
            rule.IsSendEamilToOwner = archiverRule.IsSendEmailToOwner;
            rule.UserInfos = ConvertToSORuleUserInfo(archiverRule.UserInfos);
            rule.ReviewType = (AvePoint.GCommon.Contract.StorageOptimization.Object.ReviewType)archiverRule.ReviewType;
            rule.WorkflowId = archiverRule.WorkflowId;
            rule.RelatedRecordOption = (AvePoint.GCommon.Contract.StorageOptimization.Object.RelatedRecordOption)archiverRule.RelatedRecordOption;
            rule.spMoveOption = archiverRule.spMoveOption == null ? null : ConvertToSORuleMoveOption(archiverRule.spMoveOption);
            rule.LeaveStubMessage = archiverRule.LeaveStubMessage;
            rule.IsRestoreLink = archiverRule.IsRestoreLink;
            rule.IsEnableRetention = archiverRule.IsEnableRetention;
            rule.RetentionInfo = archiverRule.RetentionInfo == null ? null : ConvertToRetentionInfo(archiverRule.RetentionInfo);
            return rule;
        }

        public static Rule ConvertArchiverRuleToConnectorRule(Cloud.Sdk.Data.Dao.ArchiverRule archiverRule)
        {
            if (null == archiverRule) return null;
            Rule rule = new Rule();
            rule.AndOrExpression = archiverRule.AndOrExpression == null ? null : ConvertToSORulePolicyLevel(archiverRule.AndOrExpression);
            rule.ArchiverCompressionType = (AvePoint.GCommon.Contract.GranularBackup.Object.CompressionType)archiverRule.ArchiverCompressionType;
            rule.ArchiverDataSecurity = (AvePoint.GCommon.Contract.GranularBackup.Object.DataSecurity)archiverRule.ArchiverDataSecurity;
            rule.ArchiverSetting = (archiverRule.ArchiverSetting == null || string.IsNullOrEmpty(archiverRule.ArchiverSetting.FolderName)) ? null : ConvertToSORuleArchiverSetting(archiverRule.ArchiverSetting);
            rule.ArchiverVEOSetting = (archiverRule.ArchiverVEOSetting == null || string.IsNullOrEmpty(archiverRule.ArchiverVEOSetting.AgencyId)) ? null : ConvertToSORuleArchiverVEOSetting(archiverRule.ArchiverVEOSetting);
            rule.CheckStatus = (AvePoint.GCommon.Contract.StorageOptimization.Object.ActionStatus)archiverRule.CheckStatus;
            rule.Compression = archiverRule.Compression;
            rule.DataEncryptionProfileId = archiverRule.DataEncryptionProfileId;
            rule.DataEncryptionProfileName = archiverRule.DataEncryptionProfileName;
            rule.DataSecurity = archiverRule.DataSecurity;
            rule.Description = archiverRule.Description;
            rule.Detail = archiverRule.Detail;
            rule.Encryption = archiverRule.Encryption;
            rule.DisposalClass = archiverRule.DisposalClass;
            rule.NAAConfigFile = archiverRule.NAAConfigFile;
            rule.NARAConfigFile = archiverRule.NARAConfigFile;
            rule.DeleteRecords = archiverRule.DeleteRecords;
            rule.DeclareLinkFile = archiverRule.DeclareLinkFile;
            rule.EncryptionInfoId = archiverRule.EncryptionInfoId;
            rule.EncryptionInfoName = archiverRule.EncryptionInfoName;
            rule.EncryptionMethods = (AvePoint.GCommon.Contract.GranularBackup.Object.EncryptionMethods)archiverRule.EncryptionMethods;
            rule.ExportInfo = (archiverRule.ExportInfo == null || string.IsNullOrEmpty(archiverRule.ExportInfo.exportLocationId)) ? null : ConvertToSORuleExportInfo(archiverRule.ExportInfo);
            rule.ExportType = (AvePoint.GCommon.Contract.StorageOptimization.Object.ExportTypeValue)archiverRule.ExportType;
            rule.FileVEO = archiverRule.FileVEO;
            rule.Filters = archiverRule.Filters == null ? null : ConvertApiFilterPolicyToFilterPolicy(archiverRule.Filters);
            rule.Id = archiverRule.Id;
            rule.IncludeNew = archiverRule.IncludeNew;
            rule.IsCheckedCheckBox = archiverRule.IsCheckedCheckBox;
            rule.IsEnabledComboBox = archiverRule.IsEnabledComboBox;
            rule.IsManualApproval = archiverRule.IsManualApproval;
            rule.KeepDataOption = archiverRule.KeepDataOption;
            rule.KeepStructrue = archiverRule.KeepStructrue;
            rule.LogicalDeviceId = archiverRule.LogicalDeviceId;
            rule.LogicalDeviceName = archiverRule.LogicalDeviceName;
            rule.ManifestVEO = archiverRule.ManifestVEO;
            rule.ModifyTime = archiverRule.ModifyTime;
            rule.Module = archiverRule.Module;
            rule.MoveToRecordCenterAndDelareSetting = (archiverRule.MoveToRecordCenterAndDelareSetting == null || archiverRule.MoveToRecordCenterAndDelareSetting.DestinationLocation == null) ? null : ConvertToSORuleMoveToRecordCenterAndDelareSetting(archiverRule.MoveToRecordCenterAndDelareSetting);
            rule.Name = archiverRule.Name;
            rule.NodeLevel = (AvePoint.GCommon.Contract.Tree.Object.NodeLevel)archiverRule.NodeLevel;
            rule.NotToCheck = archiverRule.NotToCheck;
            rule.Order = archiverRule.Order;
            rule.OrderList = archiverRule.OrderList;
            rule.PolicyLevel = (AvePoint.GCommon.Contract.CommonFilter.PolicyLevel)archiverRule.PolicyLevel;
            rule.ProfileInfo = archiverRule.ProfileInfo;
            rule.ProfileType = (AvePoint.GCommon.Contract.Server.Common.Profile.Object.ProfileType)archiverRule.ProfileType;
            rule.RecordVEO = archiverRule.RecordVEO;
            rule.ExportDataEncryptionIV = archiverRule.ExportDataEncryptionIV;
            rule.ExportDataEncryptionKey = archiverRule.ExportDataEncryptionKey;
            rule.RuleScope = archiverRule.RuleScope;
            rule.RuleStatus = (AvePoint.GCommon.Contract.StorageOptimization.Object.RuleStatus)archiverRule.RuleStatus;
            rule.SOFilters = archiverRule.SOFilters == null ? null : ConvertToSORuleSOFilterPolicy(archiverRule.SOFilters);
            rule.StoragePolicyId = archiverRule.StoragePolicyId;
            rule.StoragePolicyName = archiverRule.StoragePolicyName;
            rule.TagContentInfo = (archiverRule.TagContentInfo == null || archiverRule.TagContentInfo.Count == 0) ? null : ConvertToSORuleTagContentInfo(archiverRule.TagContentInfo);
            rule.Type = (AvePoint.GCommon.Contract.StorageOptimization.Object.RuleType)archiverRule.Type;
            rule.UseSnapLock = archiverRule.UseSnapLock;
            rule.IsSendEamilToOwner = archiverRule.IsSendEmailToOwner;
            rule.UserInfos = ConvertToSORuleUserInfo(archiverRule.UserInfos);
            rule.ReviewType = (AvePoint.GCommon.Contract.StorageOptimization.Object.ReviewType)archiverRule.ReviewType;
            rule.WorkflowId = archiverRule.WorkflowId;
            rule.RelatedRecordOption = (AvePoint.GCommon.Contract.StorageOptimization.Object.RelatedRecordOption)archiverRule.RelatedRecordOption;
            rule.spMoveOption = archiverRule.spMoveOption == null ? null : ConvertToSORuleMoveOption(archiverRule.spMoveOption);
            rule.LeaveStubMessage = archiverRule.LeaveStubMessage;
            rule.IsRestoreLink = archiverRule.IsRestoreLink;
            rule.IsEnableRetention = archiverRule.IsEnableRetention;
            rule.RetentionInfo = archiverRule.RetentionInfo == null ? null : ConvertToRetentionInfo(archiverRule.RetentionInfo);
            return rule;
        }

        public static Rule ConvertArchiverRuleToEXORule(Cloud.Sdk.Data.Dao.ArchiverRule archiverRule)
        {
            if (null == archiverRule) return null;
            Rule rule = new Rule();
            rule.AndOrExpression = archiverRule.AndOrExpression == null ? null : ConvertToSORulePolicyLevel(archiverRule.AndOrExpression);
            rule.ArchiverCompressionType = (AvePoint.GCommon.Contract.GranularBackup.Object.CompressionType)archiverRule.ArchiverCompressionType;
            rule.ArchiverDataSecurity = (AvePoint.GCommon.Contract.GranularBackup.Object.DataSecurity)archiverRule.ArchiverDataSecurity;
            rule.ArchiverSetting = (archiverRule.ArchiverSetting == null || string.IsNullOrEmpty(archiverRule.ArchiverSetting.FolderName)) ? null : ConvertToSORuleArchiverSetting(archiverRule.ArchiverSetting);
            rule.ArchiverVEOSetting = (archiverRule.ArchiverVEOSetting == null || string.IsNullOrEmpty(archiverRule.ArchiverVEOSetting.AgencyId)) ? null : ConvertToSORuleArchiverVEOSetting(archiverRule.ArchiverVEOSetting);
            rule.CheckStatus = (AvePoint.GCommon.Contract.StorageOptimization.Object.ActionStatus)archiverRule.CheckStatus;
            rule.Compression = archiverRule.Compression;
            rule.DataEncryptionProfileId = archiverRule.DataEncryptionProfileId;
            rule.DataEncryptionProfileName = archiverRule.DataEncryptionProfileName;
            rule.DataSecurity = archiverRule.DataSecurity;
            rule.Description = archiverRule.Description;
            rule.Detail = archiverRule.Detail;
            rule.Encryption = archiverRule.Encryption;
            rule.DisposalClass = archiverRule.DisposalClass;
            rule.NAAConfigFile = archiverRule.NAAConfigFile;
            rule.NARAConfigFile = archiverRule.NARAConfigFile;
            rule.DeleteRecords = archiverRule.DeleteRecords;
            rule.DeclareLinkFile = archiverRule.DeclareLinkFile;
            rule.EncryptionInfoId = archiverRule.EncryptionInfoId;
            rule.EncryptionInfoName = archiverRule.EncryptionInfoName;
            rule.EncryptionMethods = (AvePoint.GCommon.Contract.GranularBackup.Object.EncryptionMethods)archiverRule.EncryptionMethods;
            rule.ExportInfo = (archiverRule.ExportInfo == null || string.IsNullOrEmpty(archiverRule.ExportInfo.exportLocationId)) ? null : ConvertToSORuleExportInfo(archiverRule.ExportInfo);
            rule.ExportType = (AvePoint.GCommon.Contract.StorageOptimization.Object.ExportTypeValue)archiverRule.ExportType;
            rule.FileVEO = archiverRule.FileVEO;
            rule.Filters = archiverRule.Filters == null ? null : ConvertApiFilterPolicyToFilterPolicy(archiverRule.Filters);
            rule.Id = archiverRule.Id;
            rule.IncludeNew = archiverRule.IncludeNew;
            rule.IsCheckedCheckBox = archiverRule.IsCheckedCheckBox;
            rule.IsEnabledComboBox = archiverRule.IsEnabledComboBox;
            rule.IsManualApproval = archiverRule.IsManualApproval;
            rule.KeepDataOption = archiverRule.KeepDataOption;
            rule.KeepStructrue = archiverRule.KeepStructrue;
            rule.LogicalDeviceId = archiverRule.LogicalDeviceId;
            rule.LogicalDeviceName = archiverRule.LogicalDeviceName;
            rule.ManifestVEO = archiverRule.ManifestVEO;
            rule.ModifyTime = archiverRule.ModifyTime;
            rule.Module = archiverRule.Module;
            rule.MoveToRecordCenterAndDelareSetting = (archiverRule.MoveToRecordCenterAndDelareSetting == null || archiverRule.MoveToRecordCenterAndDelareSetting.DestinationLocation == null) ? null : ConvertToSORuleMoveToRecordCenterAndDelareSetting(archiverRule.MoveToRecordCenterAndDelareSetting);
            rule.Name = archiverRule.Name;
            rule.NodeLevel = (AvePoint.GCommon.Contract.Tree.Object.NodeLevel)archiverRule.NodeLevel;
            rule.NotToCheck = archiverRule.NotToCheck;
            rule.Order = archiverRule.Order;
            rule.OrderList = archiverRule.OrderList;
            rule.PolicyLevel = (AvePoint.GCommon.Contract.CommonFilter.PolicyLevel)archiverRule.PolicyLevel;
            rule.ProfileInfo = archiverRule.ProfileInfo;
            rule.ProfileType = (AvePoint.GCommon.Contract.Server.Common.Profile.Object.ProfileType)archiverRule.ProfileType;
            rule.RecordVEO = archiverRule.RecordVEO;
            rule.ExportDataEncryptionIV = archiverRule.ExportDataEncryptionIV;
            rule.ExportDataEncryptionKey = archiverRule.ExportDataEncryptionKey;
            rule.RuleScope = archiverRule.RuleScope;
            rule.RuleStatus = (AvePoint.GCommon.Contract.StorageOptimization.Object.RuleStatus)archiverRule.RuleStatus;
            rule.SOFilters = ConvertToSORuleSOFilterPolicy(archiverRule.SOFilters);
            rule.StoragePolicyId = archiverRule.StoragePolicyId;
            rule.StoragePolicyName = archiverRule.StoragePolicyName;
            rule.TagContentInfo = (archiverRule.TagContentInfo == null || archiverRule.TagContentInfo.Count == 0) ? null : ConvertToSORuleTagContentInfo(archiverRule.TagContentInfo);
            rule.Type = (AvePoint.GCommon.Contract.StorageOptimization.Object.RuleType)archiverRule.Type;
            rule.UseSnapLock = archiverRule.UseSnapLock;
            rule.IsSendEamilToOwner = archiverRule.IsSendEmailToOwner;
            rule.UserInfos = ConvertToSORuleUserInfo(archiverRule.UserInfos);
            rule.ReviewType = (AvePoint.GCommon.Contract.StorageOptimization.Object.ReviewType)archiverRule.ReviewType;
            rule.WorkflowId = archiverRule.WorkflowId;
            rule.RelatedRecordOption = (AvePoint.GCommon.Contract.StorageOptimization.Object.RelatedRecordOption)archiverRule.RelatedRecordOption;
            rule.spMoveOption = archiverRule.spMoveOption == null ? null : ConvertToSORuleMoveOption(archiverRule.spMoveOption);
            return rule;
        }
        public static Rule ConvertArchiverRuleToPhysicalRule(Cloud.Sdk.Data.Dao.ArchiverRule archiverRule)
        {
            if (null == archiverRule) return null;
            Rule rule = new Rule();
            rule.AndOrExpression = archiverRule.AndOrExpression == null ? null : ConvertToSORulePolicyLevel(archiverRule.AndOrExpression);
            rule.ArchiverCompressionType = (AvePoint.GCommon.Contract.GranularBackup.Object.CompressionType)archiverRule.ArchiverCompressionType;
            rule.ArchiverDataSecurity = (AvePoint.GCommon.Contract.GranularBackup.Object.DataSecurity)archiverRule.ArchiverDataSecurity;
            rule.ArchiverSetting = (archiverRule.ArchiverSetting == null || string.IsNullOrEmpty(archiverRule.ArchiverSetting.FolderName)) ? null : ConvertToSORuleArchiverSetting(archiverRule.ArchiverSetting);
            rule.ArchiverVEOSetting = (archiverRule.ArchiverVEOSetting == null || string.IsNullOrEmpty(archiverRule.ArchiverVEOSetting.AgencyId)) ? null : ConvertToSORuleArchiverVEOSetting(archiverRule.ArchiverVEOSetting);
            rule.CheckStatus = (AvePoint.GCommon.Contract.StorageOptimization.Object.ActionStatus)archiverRule.CheckStatus;
            rule.Compression = archiverRule.Compression;
            rule.DataEncryptionProfileId = archiverRule.DataEncryptionProfileId;
            rule.DataEncryptionProfileName = archiverRule.DataEncryptionProfileName;
            rule.DataSecurity = archiverRule.DataSecurity;
            rule.Description = archiverRule.Description;
            rule.Detail = archiverRule.Detail;
            rule.Encryption = archiverRule.Encryption;
            rule.DisposalClass = archiverRule.DisposalClass;
            rule.NAAConfigFile = archiverRule.NAAConfigFile;
            rule.NARAConfigFile = archiverRule.NARAConfigFile;
            rule.DeleteRecords = archiverRule.DeleteRecords;
            rule.DeclareLinkFile = archiverRule.DeclareLinkFile;
            rule.EncryptionInfoId = archiverRule.EncryptionInfoId;
            rule.EncryptionInfoName = archiverRule.EncryptionInfoName;
            rule.EncryptionMethods = (AvePoint.GCommon.Contract.GranularBackup.Object.EncryptionMethods)archiverRule.EncryptionMethods;
            rule.ExportInfo = (archiverRule.ExportInfo == null || string.IsNullOrEmpty(archiverRule.ExportInfo.exportLocationId)) ? null : ConvertToSORuleExportInfo(archiverRule.ExportInfo);
            rule.ExportType = (AvePoint.GCommon.Contract.StorageOptimization.Object.ExportTypeValue)archiverRule.ExportType;
            rule.FileVEO = archiverRule.FileVEO;
            rule.Filters = archiverRule.Filters == null ? null : ConvertApiFilterPolicyToFilterPolicy(archiverRule.Filters);
            rule.Id = archiverRule.Id;
            rule.IncludeNew = archiverRule.IncludeNew;
            rule.IsCheckedCheckBox = archiverRule.IsCheckedCheckBox;
            rule.IsEnabledComboBox = archiverRule.IsEnabledComboBox;
            rule.IsManualApproval = archiverRule.IsManualApproval;
            rule.KeepDataOption = archiverRule.KeepDataOption;
            rule.KeepStructrue = archiverRule.KeepStructrue;
            rule.LogicalDeviceId = archiverRule.LogicalDeviceId;
            rule.LogicalDeviceName = archiverRule.LogicalDeviceName;
            rule.ManifestVEO = archiverRule.ManifestVEO;
            rule.ModifyTime = archiverRule.ModifyTime;
            rule.Module = archiverRule.Module;
            rule.MoveToRecordCenterAndDelareSetting = (archiverRule.MoveToRecordCenterAndDelareSetting == null || archiverRule.MoveToRecordCenterAndDelareSetting.DestinationLocation == null) ? null : ConvertToSORuleMoveToRecordCenterAndDelareSetting(archiverRule.MoveToRecordCenterAndDelareSetting);
            rule.Name = archiverRule.Name;
            rule.NodeLevel = (AvePoint.GCommon.Contract.Tree.Object.NodeLevel)archiverRule.NodeLevel;
            rule.NotToCheck = archiverRule.NotToCheck;
            rule.Order = archiverRule.Order;
            rule.OrderList = archiverRule.OrderList;
            rule.PolicyLevel = (AvePoint.GCommon.Contract.CommonFilter.PolicyLevel)archiverRule.PolicyLevel;
            rule.ProfileInfo = archiverRule.ProfileInfo;
            rule.ProfileType = (AvePoint.GCommon.Contract.Server.Common.Profile.Object.ProfileType)archiverRule.ProfileType;
            rule.RecordVEO = archiverRule.RecordVEO;
            rule.ExportDataEncryptionIV = archiverRule.ExportDataEncryptionIV;
            rule.ExportDataEncryptionKey = archiverRule.ExportDataEncryptionKey;
            rule.RuleScope = archiverRule.RuleScope;
            rule.RuleStatus = (AvePoint.GCommon.Contract.StorageOptimization.Object.RuleStatus)archiverRule.RuleStatus;
            rule.SOFilters = ConvertToSORuleSOFilterPolicy(archiverRule.SOFilters);
            rule.StoragePolicyId = archiverRule.StoragePolicyId;
            rule.StoragePolicyName = archiverRule.StoragePolicyName;
            rule.TagContentInfo = (archiverRule.TagContentInfo == null || archiverRule.TagContentInfo.Count == 0) ? null : ConvertToSORuleTagContentInfo(archiverRule.TagContentInfo);
            rule.Type = (AvePoint.GCommon.Contract.StorageOptimization.Object.RuleType)archiverRule.Type;
            rule.UseSnapLock = archiverRule.UseSnapLock;
            rule.IsSendEamilToOwner = archiverRule.IsSendEmailToOwner;
            rule.UserInfos = ConvertToSORuleUserInfo(archiverRule.UserInfos);
            rule.ReviewType = (AvePoint.GCommon.Contract.StorageOptimization.Object.ReviewType)archiverRule.ReviewType;
            rule.WorkflowId = archiverRule.WorkflowId;
            rule.RelatedRecordOption = (AvePoint.GCommon.Contract.StorageOptimization.Object.RelatedRecordOption)archiverRule.RelatedRecordOption;
            rule.spMoveOption = archiverRule.spMoveOption == null ? null : ConvertToPhySORuleMoveOption(archiverRule.spMoveOption);
            return rule;
        }
        
        public static Rule ConvertArchiverRuleToBoxRule(Cloud.Sdk.Data.Dao.ArchiverRule archiverRule)
        {
            if (null == archiverRule) return null;
            Rule rule = new Rule();
            rule.AndOrExpression = archiverRule.AndOrExpression == null ? null : ConvertToSORulePolicyLevel(archiverRule.AndOrExpression);
            rule.ArchiverCompressionType = (AvePoint.GCommon.Contract.GranularBackup.Object.CompressionType)archiverRule.ArchiverCompressionType;
            rule.ArchiverDataSecurity = (AvePoint.GCommon.Contract.GranularBackup.Object.DataSecurity)archiverRule.ArchiverDataSecurity;
            rule.ArchiverSetting = (archiverRule.ArchiverSetting == null || string.IsNullOrEmpty(archiverRule.ArchiverSetting.FolderName)) ? null : ConvertToSORuleArchiverSetting(archiverRule.ArchiverSetting);
            rule.ArchiverVEOSetting = (archiverRule.ArchiverVEOSetting == null || string.IsNullOrEmpty(archiverRule.ArchiverVEOSetting.AgencyId)) ? null : ConvertToSORuleArchiverVEOSetting(archiverRule.ArchiverVEOSetting);
            rule.CheckStatus = (AvePoint.GCommon.Contract.StorageOptimization.Object.ActionStatus)archiverRule.CheckStatus;
            rule.Compression = archiverRule.Compression;
            rule.DataEncryptionProfileId = archiverRule.DataEncryptionProfileId;
            rule.DataEncryptionProfileName = archiverRule.DataEncryptionProfileName;
            rule.DataSecurity = archiverRule.DataSecurity;
            rule.Description = archiverRule.Description;
            rule.Detail = archiverRule.Detail;
            rule.Encryption = archiverRule.Encryption;
            rule.DisposalClass = archiverRule.DisposalClass;
            rule.NAAConfigFile = archiverRule.NAAConfigFile;
            rule.NARAConfigFile = archiverRule.NARAConfigFile;
            rule.DeleteRecords = archiverRule.DeleteRecords;
            rule.DeclareLinkFile = archiverRule.DeclareLinkFile;
            rule.EncryptionInfoId = archiverRule.EncryptionInfoId;
            rule.EncryptionInfoName = archiverRule.EncryptionInfoName;
            rule.EncryptionMethods = (AvePoint.GCommon.Contract.GranularBackup.Object.EncryptionMethods)archiverRule.EncryptionMethods;
            rule.ExportInfo = (archiverRule.ExportInfo == null || string.IsNullOrEmpty(archiverRule.ExportInfo.exportLocationId)) ? null : ConvertToSORuleExportInfo(archiverRule.ExportInfo);
            rule.ExportType = (AvePoint.GCommon.Contract.StorageOptimization.Object.ExportTypeValue)archiverRule.ExportType;
            rule.FileVEO = archiverRule.FileVEO;
            rule.Filters = archiverRule.Filters == null ? null : ConvertApiFilterPolicyToFilterPolicy(archiverRule.Filters);
            rule.Id = archiverRule.Id;
            rule.IncludeNew = archiverRule.IncludeNew;
            rule.IsCheckedCheckBox = archiverRule.IsCheckedCheckBox;
            rule.IsEnabledComboBox = archiverRule.IsEnabledComboBox;
            rule.IsManualApproval = archiverRule.IsManualApproval;
            rule.KeepDataOption = archiverRule.KeepDataOption;
            rule.KeepStructrue = archiverRule.KeepStructrue;
            rule.LogicalDeviceId = archiverRule.LogicalDeviceId;
            rule.LogicalDeviceName = archiverRule.LogicalDeviceName;
            rule.ManifestVEO = archiverRule.ManifestVEO;
            rule.ModifyTime = archiverRule.ModifyTime;
            rule.Module = archiverRule.Module;
            rule.MoveToRecordCenterAndDelareSetting = (archiverRule.MoveToRecordCenterAndDelareSetting == null || archiverRule.MoveToRecordCenterAndDelareSetting.DestinationLocation == null) ? null : ConvertToSORuleMoveToRecordCenterAndDelareSetting(archiverRule.MoveToRecordCenterAndDelareSetting);
            rule.Name = archiverRule.Name;
            rule.NodeLevel = (AvePoint.GCommon.Contract.Tree.Object.NodeLevel)archiverRule.NodeLevel;
            rule.NotToCheck = archiverRule.NotToCheck;
            rule.Order = archiverRule.Order;
            rule.OrderList = archiverRule.OrderList;
            rule.PolicyLevel = (AvePoint.GCommon.Contract.CommonFilter.PolicyLevel)archiverRule.PolicyLevel;
            rule.ProfileInfo = archiverRule.ProfileInfo;
            rule.ProfileType = (AvePoint.GCommon.Contract.Server.Common.Profile.Object.ProfileType)archiverRule.ProfileType;
            rule.RecordVEO = archiverRule.RecordVEO;
            rule.ExportDataEncryptionIV = archiverRule.ExportDataEncryptionIV;
            rule.ExportDataEncryptionKey = archiverRule.ExportDataEncryptionKey;
            rule.RuleScope = archiverRule.RuleScope;
            rule.RuleStatus = (AvePoint.GCommon.Contract.StorageOptimization.Object.RuleStatus)archiverRule.RuleStatus;
            rule.SOFilters = archiverRule.SOFilters == null ? null : ConvertToSORuleSOFilterPolicy(archiverRule.SOFilters);
            rule.StoragePolicyId = archiverRule.StoragePolicyId;
            rule.StoragePolicyName = archiverRule.StoragePolicyName;
            rule.TagContentInfo = (archiverRule.TagContentInfo == null || archiverRule.TagContentInfo.Count == 0) ? null : ConvertToSORuleTagContentInfo(archiverRule.TagContentInfo);
            rule.Type = (AvePoint.GCommon.Contract.StorageOptimization.Object.RuleType)archiverRule.Type;
            rule.UseSnapLock = archiverRule.UseSnapLock;
            rule.IsSendEamilToOwner = archiverRule.IsSendEmailToOwner;
            rule.UserInfos = ConvertToSORuleUserInfo(archiverRule.UserInfos);
            rule.ReviewType = (AvePoint.GCommon.Contract.StorageOptimization.Object.ReviewType)archiverRule.ReviewType;
            rule.WorkflowId = archiverRule.WorkflowId;
            rule.RelatedRecordOption = (AvePoint.GCommon.Contract.StorageOptimization.Object.RelatedRecordOption)archiverRule.RelatedRecordOption;
            rule.spMoveOption = archiverRule.spMoveOption == null ? null : ConvertToSORuleMoveOption(archiverRule.spMoveOption);
            rule.LeaveStubMessage = archiverRule.LeaveStubMessage;
            rule.IsRestoreLink = archiverRule.IsRestoreLink;
            rule.IsEnableRetention = archiverRule.IsEnableRetention;
            rule.RetentionInfo = archiverRule.RetentionInfo == null ? null : ConvertToRetentionInfo(archiverRule.RetentionInfo);
            return rule;
        }

        private static List<Cloud.Sdk.Data.Dao.AndOrExpression> ConvertToArchiverRulePolicyLevel(Dictionary<AvePoint.GCommon.Contract.CommonFilter.PolicyLevel, string> andOrExpression)
        {
            List<Cloud.Sdk.Data.Dao.AndOrExpression> result = new List<Cloud.Sdk.Data.Dao.AndOrExpression>();
            foreach (var item in andOrExpression.Keys)
            {
                Cloud.Sdk.Data.Dao.AndOrExpression expression = new Cloud.Sdk.Data.Dao.AndOrExpression();
                expression.Key = (Cloud.Sdk.Data.Dao.PolicyLevel)item;
                expression.Value = andOrExpression[item];
                result.Add(expression);
            }
            return result;
        }

        private static Cloud.Sdk.Data.Dao.RetentionInfo convertToRetentionInfo(RetentionInfo info)
        {
            Cloud.Sdk.Data.Dao.RetentionInfo retentionInfo = new Cloud.Sdk.Data.Dao.RetentionInfo();
            retentionInfo.IsManualApproval = info.IsManualApproval;
            retentionInfo.Condition = (int)info.Condition;
            retentionInfo.ColumnName = info.ColumnName;
            retentionInfo.KeepDateNumber = info.KeepDateNumber;
            retentionInfo.KeepDateUnite = (int)info.KeepDateUnite;
            retentionInfo.Date = info.Date;
            retentionInfo.ReviewType = (Cloud.Sdk.Data.Dao.ReviewType)info.ReviewType;
            retentionInfo.WorkflowId = info.WorkflowId;
            retentionInfo.IsSendEamilToOwner = info.IsSendEamilToOwner;
            retentionInfo.UserInfos = ConvertToApiRuleUserInfo(info.UserInfos);
            return retentionInfo;
        }

        private static List<Cloud.Sdk.Data.Dao.UserInfo> ConvertToApiRuleUserInfo(List<UserInfo> userInfos)
        {
            var results = new List<Cloud.Sdk.Data.Dao.UserInfo>();
            if (userInfos != null && userInfos.Count > 0)
            {
                foreach (var item in userInfos)
                {
                    results.Add(new Cloud.Sdk.Data.Dao.UserInfo
                    {
                        UserId = item.UserId,
                        UserPrincipalName = item.UserPrincipalName,
                        InviteType = (Cloud.Sdk.Data.Dao.InviteType)item.InviteType,
                        DisplayName = item.DisplayName,
                        Email = item.Email
                    });
                }
            }
            return results;
        }

        public static Cloud.Sdk.Data.Dao.MoveOption ConvertToArchiverRuleMoveOption(MoveOption soRuleMoveOption)
        {
            #region SP
            Cloud.Sdk.Data.Dao.MoveOption archiverMoveOption = new Cloud.Sdk.Data.Dao.MoveOption();
            if (soRuleMoveOption != null && soRuleMoveOption.MoveDestination != null && (!string.IsNullOrEmpty(soRuleMoveOption.MoveDestination.SPUrl) || soRuleMoveOption.MoveDestination.SPTreeNode != null))
            {
                archiverMoveOption.MoveDestination = new Cloud.Sdk.Data.Dao.MoveDestination();
                archiverMoveOption.MoveDestination.NotDeclareMovedData = soRuleMoveOption.MoveDestination.NotDeclareMovedData;
                archiverMoveOption.MoveDestination.DeleteSourceItem = soRuleMoveOption.MoveDestination.DeleteSourceItem;
                archiverMoveOption.MoveDestination.KeepSourceClassification = soRuleMoveOption.MoveDestination.KeepSourceClassification;
                archiverMoveOption.SourceFlag = Cloud.Sdk.Data.Dao.RecordFlag.SP;
                archiverMoveOption.DestFlag = Cloud.Sdk.Data.Dao.RecordFlag.SP;
                if (soRuleMoveOption.MoveDestination.DestMode == DestMode.UrlMode)
                {
                    archiverMoveOption.MoveDestination.DestMode = Cloud.Sdk.Data.Dao.DestMode.UrlMode;
                    archiverMoveOption.MoveDestination.SPUrl = soRuleMoveOption.MoveDestination.SPUrl;
                    archiverMoveOption.MoveDestination.ContainerId = soRuleMoveOption.MoveDestination.ContainerId;
                    Cloud.Sdk.Data.Dao.Office365AccountInfo account = new Cloud.Sdk.Data.Dao.Office365AccountInfo();
                    account.UserName = soRuleMoveOption.MoveDestination.SPAccount.UserName;
                    account.Password = soRuleMoveOption.MoveDestination.SPAccount.Password;
                    archiverMoveOption.MoveDestination.SPAccount = account;
                }
                else
                {
                    archiverMoveOption.MoveDestination.DestMode = Cloud.Sdk.Data.Dao.DestMode.TreeMode;
                    archiverMoveOption.MoveDestination.SPUrl = soRuleMoveOption.MoveDestination.SPUrl;
                    archiverMoveOption.MoveDestination.ContainerId = soRuleMoveOption.MoveDestination.ContainerId;
                    archiverMoveOption.MoveDestination.SPTreeNode = null;// ConvertToSPTreeNodeDtoInfo(soRuleMoveOption.MoveDestination.SPTreeNode);
                    archiverMoveOption.MoveDestination.SPTreeStr = !string.IsNullOrEmpty(soRuleMoveOption.MoveDestination.SPTreeStr) ? soRuleMoveOption.MoveDestination.SPTreeStr : "";
                }
                #region Move Settings
                archiverMoveOption.MoveSetting = new Cloud.Sdk.Data.Dao.MoveRecordSetting();
                archiverMoveOption.MoveSetting.ConflictType = (Cloud.Sdk.Data.Dao.ConflictType)soRuleMoveOption.MoveSetting.ConflictType;
                archiverMoveOption.MoveSetting.ItemLevelConflictOption = (Cloud.Sdk.Data.Dao.ConflictOption)soRuleMoveOption.MoveSetting.ItemLevelConflictOption;
                archiverMoveOption.MoveSetting.ContainerLevelConflictOption = (Cloud.Sdk.Data.Dao.ConflictOption)soRuleMoveOption.MoveSetting.ContainerLevelConflictOption;
                #endregion
            }

            return archiverMoveOption;
            #endregion
        }
        public static Cloud.Sdk.Data.Dao.MoveOption ConvertToArchiverRuleFSMoveOption(MoveOption soRuleMoveOption)
        {
            #region FS
            Cloud.Sdk.Data.Dao.MoveOption archiverMoveOption = new Cloud.Sdk.Data.Dao.MoveOption();
            if (soRuleMoveOption != null && soRuleMoveOption.MoveDestination != null && (!string.IsNullOrEmpty(soRuleMoveOption.MoveDestination.FSPath) || soRuleMoveOption.MoveDestination.FSTreeNode != null))
            {
                archiverMoveOption.MoveDestination = new Cloud.Sdk.Data.Dao.MoveDestination();
                archiverMoveOption.MoveDestination.NotDeclareMovedData = soRuleMoveOption.MoveDestination.NotDeclareMovedData;
                archiverMoveOption.SourceFlag = Cloud.Sdk.Data.Dao.RecordFlag.FS;
                archiverMoveOption.DestFlag = Cloud.Sdk.Data.Dao.RecordFlag.FS;
                if (soRuleMoveOption.MoveDestination.DestMode == DestMode.UrlMode)
                {
                    archiverMoveOption.MoveDestination.DestMode = Cloud.Sdk.Data.Dao.DestMode.UrlMode;
                    archiverMoveOption.MoveDestination.FSPath = soRuleMoveOption.MoveDestination.FSPath;
                    Cloud.Sdk.Data.Dao.Office365AccountInfo account = new Cloud.Sdk.Data.Dao.Office365AccountInfo();
                }
                else
                {
                    archiverMoveOption.MoveDestination.DestMode = Cloud.Sdk.Data.Dao.DestMode.TreeMode;
                    archiverMoveOption.MoveDestination.FSTreeStr = soRuleMoveOption.MoveDestination.FSTreeStr;
                    archiverMoveOption.MoveDestination.FSTreeNode = null;
                    archiverMoveOption.MoveDestination.FSTreeStr = !string.IsNullOrEmpty(soRuleMoveOption.MoveDestination.FSTreeStr) ? soRuleMoveOption.MoveDestination.FSTreeStr : "";
                    archiverMoveOption.MoveDestination.FSPath = soRuleMoveOption.MoveDestination.FSPath;
                }
                #region Move Settings
                archiverMoveOption.MoveSetting = new Cloud.Sdk.Data.Dao.MoveRecordSetting();
                archiverMoveOption.MoveSetting.ConflictType = (Cloud.Sdk.Data.Dao.ConflictType)soRuleMoveOption.MoveSetting.ConflictType;
                archiverMoveOption.MoveSetting.ItemLevelConflictOption = (Cloud.Sdk.Data.Dao.ConflictOption)soRuleMoveOption.MoveSetting.ItemLevelConflictOption;
                archiverMoveOption.MoveSetting.ContainerLevelConflictOption = (Cloud.Sdk.Data.Dao.ConflictOption)soRuleMoveOption.MoveSetting.ContainerLevelConflictOption;
                #endregion
            }

            return archiverMoveOption;
            #endregion
        }
        public static Cloud.Sdk.Data.Dao.MoveOption ConvertToArchiverRuleEXOMoveOption(MoveOption soRuleMoveOption)
        {
            #region EXO
            Cloud.Sdk.Data.Dao.MoveOption archiverMoveOption = new Cloud.Sdk.Data.Dao.MoveOption();
            if (soRuleMoveOption != null && soRuleMoveOption.MoveDestination != null && (!string.IsNullOrEmpty(soRuleMoveOption.MoveDestination.SPUrl) || soRuleMoveOption.MoveDestination.SPTreeNode != null))
            {
                archiverMoveOption.MoveDestination = new Cloud.Sdk.Data.Dao.MoveDestination();
                archiverMoveOption.MoveDestination.NotDeclareMovedData = soRuleMoveOption.MoveDestination.NotDeclareMovedData;
                archiverMoveOption.MoveDestination.DeleteSourceItem = soRuleMoveOption.MoveDestination.DeleteSourceItem;
                archiverMoveOption.MoveDestination.KeepSourceClassification = soRuleMoveOption.MoveDestination.KeepSourceClassification;
                archiverMoveOption.SourceFlag = Cloud.Sdk.Data.Dao.RecordFlag.EXO;
                archiverMoveOption.DestFlag = Cloud.Sdk.Data.Dao.RecordFlag.SP;
                if (soRuleMoveOption.MoveDestination.DestMode == DestMode.UrlMode)
                {
                    archiverMoveOption.MoveDestination.DestMode = Cloud.Sdk.Data.Dao.DestMode.UrlMode;
                    archiverMoveOption.MoveDestination.SPUrl = soRuleMoveOption.MoveDestination.SPUrl;
                    archiverMoveOption.MoveDestination.ContainerId = soRuleMoveOption.MoveDestination.ContainerId;
                    Cloud.Sdk.Data.Dao.Office365AccountInfo account = new Cloud.Sdk.Data.Dao.Office365AccountInfo();
                    account.UserName = soRuleMoveOption.MoveDestination.SPAccount.UserName;
                    account.Password = soRuleMoveOption.MoveDestination.SPAccount.Password;
                    archiverMoveOption.MoveDestination.SPAccount = account;
                }
                else
                {
                    archiverMoveOption.MoveDestination.DestMode = Cloud.Sdk.Data.Dao.DestMode.TreeMode;
                    archiverMoveOption.MoveDestination.SPUrl = soRuleMoveOption.MoveDestination.SPUrl;
                    archiverMoveOption.MoveDestination.ContainerId = soRuleMoveOption.MoveDestination.ContainerId;
                    archiverMoveOption.MoveDestination.SPTreeNode = null;// ConvertToSPTreeNodeDtoInfo(soRuleMoveOption.MoveDestination.SPTreeNode);
                    archiverMoveOption.MoveDestination.SPTreeStr = !string.IsNullOrEmpty(soRuleMoveOption.MoveDestination.SPTreeStr) ? soRuleMoveOption.MoveDestination.SPTreeStr : "";
                }
                #region Move Settings
                archiverMoveOption.MoveSetting = new Cloud.Sdk.Data.Dao.MoveRecordSetting();
                archiverMoveOption.MoveSetting.ConflictType = (Cloud.Sdk.Data.Dao.ConflictType)soRuleMoveOption.MoveSetting.ConflictType;
                archiverMoveOption.MoveSetting.ItemLevelConflictOption = (Cloud.Sdk.Data.Dao.ConflictOption)soRuleMoveOption.MoveSetting.ItemLevelConflictOption;
                archiverMoveOption.MoveSetting.ContainerLevelConflictOption = (Cloud.Sdk.Data.Dao.ConflictOption)soRuleMoveOption.MoveSetting.ContainerLevelConflictOption;
                #endregion
            }

            return archiverMoveOption;
            #endregion
        }
        public static Cloud.Sdk.Data.Dao.MoveOption ConvertToArchiverRuleSPLocalMoveOption(MoveOption soRuleMoveOption)
        {
            #region SP Local
            Cloud.Sdk.Data.Dao.MoveOption archiverMoveOption = new Cloud.Sdk.Data.Dao.MoveOption();
            if (soRuleMoveOption != null && soRuleMoveOption.MoveDestination != null && (!string.IsNullOrEmpty(soRuleMoveOption.MoveDestination.SPUrl) || soRuleMoveOption.MoveDestination.SPTreeNode != null))
            {
                archiverMoveOption.MoveDestination = new Cloud.Sdk.Data.Dao.MoveDestination();
                archiverMoveOption.MoveDestination.NotDeclareMovedData = soRuleMoveOption.MoveDestination.NotDeclareMovedData;
                archiverMoveOption.MoveDestination.DeleteSourceItem = soRuleMoveOption.MoveDestination.DeleteSourceItem;
                archiverMoveOption.MoveDestination.KeepSourceClassification = soRuleMoveOption.MoveDestination.KeepSourceClassification;
                archiverMoveOption.SourceFlag = Cloud.Sdk.Data.Dao.RecordFlag.SPLocal;
                archiverMoveOption.DestFlag = Cloud.Sdk.Data.Dao.RecordFlag.SPLocal;
                if (soRuleMoveOption.MoveDestination.DestMode == DestMode.UrlMode)
                {
                    archiverMoveOption.MoveDestination.DestMode = Cloud.Sdk.Data.Dao.DestMode.UrlMode;
                    archiverMoveOption.MoveDestination.SPUrl = soRuleMoveOption.MoveDestination.SPUrl;
                    archiverMoveOption.MoveDestination.ContainerId = soRuleMoveOption.MoveDestination.ContainerId;
                    Cloud.Sdk.Data.Dao.Office365AccountInfo account = new Cloud.Sdk.Data.Dao.Office365AccountInfo();
                    account.UserName = soRuleMoveOption.MoveDestination.SPAccount.UserName;
                    account.Password = soRuleMoveOption.MoveDestination.SPAccount.Password;
                    archiverMoveOption.MoveDestination.SPAccount = account;
                }
                else
                {
                    archiverMoveOption.MoveDestination.DestMode = Cloud.Sdk.Data.Dao.DestMode.TreeMode;
                    archiverMoveOption.MoveDestination.SPUrl = soRuleMoveOption.MoveDestination.SPUrl;
                    archiverMoveOption.MoveDestination.ContainerId = soRuleMoveOption.MoveDestination.ContainerId;
                    archiverMoveOption.MoveDestination.SPTreeNode = null;// ConvertToSPTreeNodeDtoInfo(soRuleMoveOption.MoveDestination.SPTreeNode);
                    archiverMoveOption.MoveDestination.SPTreeStr = !string.IsNullOrEmpty(soRuleMoveOption.MoveDestination.SPTreeStr) ? soRuleMoveOption.MoveDestination.SPTreeStr : "";
                }
                #region Move Settings
                archiverMoveOption.MoveSetting = new Cloud.Sdk.Data.Dao.MoveRecordSetting();
                archiverMoveOption.MoveSetting.ConflictType = (Cloud.Sdk.Data.Dao.ConflictType)soRuleMoveOption.MoveSetting.ConflictType;
                archiverMoveOption.MoveSetting.ItemLevelConflictOption = (Cloud.Sdk.Data.Dao.ConflictOption)soRuleMoveOption.MoveSetting.ItemLevelConflictOption;
                archiverMoveOption.MoveSetting.ContainerLevelConflictOption = (Cloud.Sdk.Data.Dao.ConflictOption)soRuleMoveOption.MoveSetting.ContainerLevelConflictOption;
                #endregion
            }

            return archiverMoveOption;
            #endregion
        }
        public static MoveOption ConvertToSORuleFSMoveOption(Cloud.Sdk.Data.Dao.MoveOption archiverMoveOption)
        {
            #region SP
            MoveOption soMoveOption = new MoveOption();
            if (archiverMoveOption != null && archiverMoveOption.MoveDestination != null && (!string.IsNullOrEmpty(archiverMoveOption.MoveDestination.FSPath) || archiverMoveOption.MoveDestination.FSTreeStr != null))
            {
                soMoveOption.MoveDestination = new MoveDestination();
                soMoveOption.MoveDestination.NotDeclareMovedData = archiverMoveOption.MoveDestination.NotDeclareMovedData;

                if (archiverMoveOption.MoveDestination.DestMode == Cloud.Sdk.Data.Dao.DestMode.UrlMode)
                {
                    soMoveOption.MoveDestination.DestMode = DestMode.UrlMode;
                    soMoveOption.MoveDestination.FSPath = archiverMoveOption.MoveDestination.FSPath;


                }
                else
                {
                    soMoveOption.MoveDestination.DestMode = DestMode.TreeMode;
                    //soMoveOption.MoveDestination.FSTreeNode = ConvertToFSTreeNodeDtoInfo(archiverMoveOption.MoveDestination.FSTreeNode);
                    soMoveOption.MoveDestination.FSPath = archiverMoveOption.MoveDestination.FSPath;
                    soMoveOption.MoveDestination.FSTreeStr = !string.IsNullOrEmpty(archiverMoveOption.MoveDestination.FSTreeStr) ? archiverMoveOption.MoveDestination.FSTreeStr : "";
                }
                #region Move Settings
                soMoveOption.MoveSetting = new MoveRecordSetting();
                soMoveOption.MoveSetting.ConflictType = (ConflictType)archiverMoveOption.MoveSetting.ConflictType;
                soMoveOption.MoveSetting.ItemLevelConflictOption = (ConflictOption)archiverMoveOption.MoveSetting.ItemLevelConflictOption;
                soMoveOption.MoveSetting.ContainerLevelConflictOption = (ConflictOption)archiverMoveOption.MoveSetting.ContainerLevelConflictOption;
                #endregion
            }
            return soMoveOption;
            #endregion
        }

        private static RetentionInfo ConvertToRetentionInfo(Cloud.Sdk.Data.Dao.RetentionInfo info)
        {
            RetentionInfo retentionInfo = new RetentionInfo();
            retentionInfo.IsManualApproval = info.IsManualApproval;
            retentionInfo.Condition = (GCommon.Contract.Server.Common.Profile.Object.TimeFilterCondition)info.Condition;
            retentionInfo.ColumnName = info.ColumnName;
            retentionInfo.KeepDateNumber = info.KeepDateNumber;
            retentionInfo.KeepDateUnite = (GCommon.Contract.Server.Common.Profile.Object.TimeUnit)info.KeepDateUnite;
            retentionInfo.Date = info.Date;
            retentionInfo.ReviewType = (ReviewType)info.ReviewType;
            retentionInfo.IsSendEamilToOwner = info.IsSendEamilToOwner;
            retentionInfo.WorkflowId = info.WorkflowId;
            retentionInfo.UserInfos = ConvertToSoRuleUserInfo(info.UserInfos);
            return retentionInfo;
        }

        private static List<UserInfo> ConvertToSoRuleUserInfo(List<Cloud.Sdk.Data.Dao.UserInfo> userInfos)
        {
            var results = new List<UserInfo>();
            if (userInfos != null && userInfos.Count > 0)
            {
                foreach (var item in userInfos)
                {
                    results.Add(new UserInfo
                    {
                        UserId = item.UserId,
                        UserPrincipalName = item.UserPrincipalName,
                        InviteType = (GCommon.Contract.Server.Login.InviteType)item.InviteType,
                        DisplayName = item.DisplayName,
                        Email = item.Email
                    });
                }
            }
            return results;
        }
        public static MoveOption ConvertToSORuleMoveOption(Cloud.Sdk.Data.Dao.MoveOption archiverMoveOption)
        {
            #region SP
            MoveOption soMoveOption = new MoveOption();
            if (archiverMoveOption != null && archiverMoveOption.MoveDestination != null && (!string.IsNullOrEmpty(archiverMoveOption.MoveDestination.SPUrl) || archiverMoveOption.MoveDestination.SPTreeNode != null))
            {
                soMoveOption.MoveDestination = new MoveDestination();
                soMoveOption.MoveDestination.NotDeclareMovedData = archiverMoveOption.MoveDestination.NotDeclareMovedData;
                soMoveOption.MoveDestination.DeleteSourceItem = archiverMoveOption.MoveDestination.DeleteSourceItem;
                soMoveOption.MoveDestination.KeepSourceClassification = archiverMoveOption.MoveDestination.KeepSourceClassification;

                if (archiverMoveOption.MoveDestination.DestMode == Cloud.Sdk.Data.Dao.DestMode.UrlMode)
                {
                    soMoveOption.MoveDestination.DestMode = DestMode.UrlMode;
                    soMoveOption.MoveDestination.SPUrl = archiverMoveOption.MoveDestination.SPUrl;
                    soMoveOption.MoveDestination.ContainerId = archiverMoveOption.MoveDestination.ContainerId;
                    AvePoint.GCommon.Contract.Server.ControlPanel.Office365Account.Object.Office365AccountInfo account = new AvePoint.GCommon.Contract.Server.ControlPanel.Office365Account.Object.Office365AccountInfo();
                    account.UserName = archiverMoveOption.MoveDestination.SPAccount.UserName;
                    account.Password = archiverMoveOption.MoveDestination.SPAccount.Password;
                    soMoveOption.MoveDestination.SPAccount = account;
                }
                else
                {
                    soMoveOption.MoveDestination.DestMode = DestMode.TreeMode;
                    soMoveOption.MoveDestination.SPTreeNode = null;//ConvertToSPTreeNodeDtoInfo(archiverMoveOption.MoveDestination.SPTreeNode);
                    soMoveOption.MoveDestination.SPUrl = archiverMoveOption.MoveDestination.SPUrl;
                    soMoveOption.MoveDestination.ContainerId = archiverMoveOption.MoveDestination.ContainerId;
                    soMoveOption.MoveDestination.SPTreeStr = !string.IsNullOrEmpty(archiverMoveOption.MoveDestination.SPTreeStr) ? archiverMoveOption.MoveDestination.SPTreeStr : "";
                }
                #region Move Settings
                soMoveOption.MoveSetting = new MoveRecordSetting();
                soMoveOption.MoveSetting.ConflictType = (ConflictType)archiverMoveOption.MoveSetting.ConflictType;
                soMoveOption.MoveSetting.ItemLevelConflictOption = (ConflictOption)archiverMoveOption.MoveSetting.ItemLevelConflictOption;
                soMoveOption.MoveSetting.ContainerLevelConflictOption = (ConflictOption)archiverMoveOption.MoveSetting.ContainerLevelConflictOption;
                #endregion
            }
            return soMoveOption;
            #endregion
        }

        public static Cloud.Sdk.Data.Dao.MoveOption ConvertToPhyArchiverRuleMoveOption(MoveOption soRuleMoveOption)
        {
            #region Physical
            Cloud.Sdk.Data.Dao.MoveOption archiverMoveOption = null;
            if (soRuleMoveOption != null && soRuleMoveOption.MoveDestination != null && soRuleMoveOption.MoveDestination.PhysicalTree != null)
            {
                archiverMoveOption = new Cloud.Sdk.Data.Dao.MoveOption();
                archiverMoveOption.MoveDestination = new Cloud.Sdk.Data.Dao.MoveDestination();
                archiverMoveOption.MoveDestination.DestMode = Cloud.Sdk.Data.Dao.DestMode.TreeMode;
                archiverMoveOption.DestFlag = Cloud.Sdk.Data.Dao.RecordFlag.Physical;
                archiverMoveOption.SourceFlag = Cloud.Sdk.Data.Dao.RecordFlag.Physical;
                Cloud.Sdk.Data.Dao.PhysicalDestTree apiPhyTreeDto = new Cloud.Sdk.Data.Dao.PhysicalDestTree();
                apiPhyTreeDto.LocationId = soRuleMoveOption.MoveDestination.PhysicalTree.LocationId;
                apiPhyTreeDto.BoxId = soRuleMoveOption.MoveDestination.PhysicalTree.BoxId;
                apiPhyTreeDto.FullPath = soRuleMoveOption.MoveDestination.PhysicalTree.FullPath;
                apiPhyTreeDto.FileId = soRuleMoveOption.MoveDestination.PhysicalTree.FileId;

                archiverMoveOption.MoveDestination.PhysicalTree = apiPhyTreeDto;
                archiverMoveOption.MoveDestination.PhysicalTreeStr = soRuleMoveOption.MoveDestination.PhysicalTreeStr;
                archiverMoveOption.MoveSetting = new Cloud.Sdk.Data.Dao.MoveRecordSetting()
                {
                    ItemLevelConflictOption = GetWebApiConflictOption(soRuleMoveOption.MoveSetting.ItemLevelConflictOption),
                    PhysicalHoldConflictOption = GetWebApiPhysicalHoldConflictOption(soRuleMoveOption.MoveSetting.PhysicalHoldConflictOption),
                };
            }
            return archiverMoveOption;
            #endregion
        }
        public static MoveOption ConvertToPhySORuleMoveOption(Cloud.Sdk.Data.Dao.MoveOption archiverMoveOption)
        {
            #region Physical
            MoveOption soMoveOption = new MoveOption();
            if (archiverMoveOption != null && archiverMoveOption.MoveDestination != null && archiverMoveOption.MoveDestination.PhysicalTree != null)
            {
                soMoveOption.MoveDestination = new MoveDestination();
                soMoveOption.MoveDestination.DestMode = DestMode.TreeMode;
                PhysicalDestTree soPhyTreeDto = new PhysicalDestTree();
                soPhyTreeDto.FullPath = archiverMoveOption.MoveDestination.PhysicalTree.FullPath;
                soPhyTreeDto.LocationId = archiverMoveOption.MoveDestination.PhysicalTree.LocationId;
                soPhyTreeDto.BoxId = archiverMoveOption.MoveDestination.PhysicalTree.BoxId;
                soPhyTreeDto.FileId = archiverMoveOption.MoveDestination.PhysicalTree.FileId;
                soMoveOption.MoveDestination.PhysicalTree = soPhyTreeDto;//ConvertToSPTreeNodeDtoInfo(archiverMoveOption.MoveDestination.SPTreeNode);
                soMoveOption.MoveDestination.PhysicalTreeStr = archiverMoveOption.MoveDestination.PhysicalTreeStr;
            }
            ArgumentCheck.NotNull(archiverMoveOption, nameof(archiverMoveOption));
            soMoveOption.MoveSetting = new MoveRecordSetting()
            {
                ConflictType = (ConflictType)archiverMoveOption.MoveSetting.ConflictType,
                ItemLevelConflictOption = GetConflictOption(archiverMoveOption.MoveSetting.ItemLevelConflictOption),
                PhysicalHoldConflictOption = GetPhysicalHoldConflictOption(archiverMoveOption.MoveSetting.PhysicalHoldConflictOption),
            };
            return soMoveOption;
            #endregion
        }
        private static ConflictOption GetConflictOption(Cloud.Sdk.Data.Dao.ConflictOption conflictOption)
        {
            switch (conflictOption)
            {
                case Cloud.Sdk.Data.Dao.ConflictOption.Skip:
                    return ConflictOption.Skip;
                case Cloud.Sdk.Data.Dao.ConflictOption.NotOverwrite:
                    return ConflictOption.NotOverwrite;
                case Cloud.Sdk.Data.Dao.ConflictOption.AppendByName:
                    return ConflictOption.AppendByName;
                case Cloud.Sdk.Data.Dao.ConflictOption.AppendByVersion:
                    return ConflictOption.AppendByVersion;
                case Cloud.Sdk.Data.Dao.ConflictOption.Overwrite:
                    return ConflictOption.Overwrite;
                case Cloud.Sdk.Data.Dao.ConflictOption.Replace:
                    return ConflictOption.Replace;
                case Cloud.Sdk.Data.Dao.ConflictOption.Merge:
                    return ConflictOption.Merge;
                case Cloud.Sdk.Data.Dao.ConflictOption.OverwriteByLastModifiedTime:
                    return ConflictOption.OverwriteByLastModifiedTime;
                default:
                    return ConflictOption.Skip;
            }
        }
        private static PhysicalHoldConflictOption GetPhysicalHoldConflictOption(Cloud.Sdk.Data.Dao.PhysicalHoldConflictOption physicalHoldConflictOption)
        {
            switch (physicalHoldConflictOption)
            {
                case Cloud.Sdk.Data.Dao.PhysicalHoldConflictOption.UseDesDefinedHoldSetting:
                    return PhysicalHoldConflictOption.UseDesDefinedHoldSetting;
                case Cloud.Sdk.Data.Dao.PhysicalHoldConflictOption.CompareHoldSetting:
                    return PhysicalHoldConflictOption.CompareHoldSetting;
                default:
                    return PhysicalHoldConflictOption.UseDesDefinedHoldSetting;
            }
        }
        private static Cloud.Sdk.Data.Dao.ConflictOption GetWebApiConflictOption(ConflictOption conflictOption)
        {
            switch (conflictOption)
            {
                case ConflictOption.Skip:
                    return Cloud.Sdk.Data.Dao.ConflictOption.Skip;
                case ConflictOption.NotOverwrite:
                    return Cloud.Sdk.Data.Dao.ConflictOption.NotOverwrite;
                case ConflictOption.AppendByName:
                    return Cloud.Sdk.Data.Dao.ConflictOption.AppendByName;
                case ConflictOption.AppendByVersion:
                    return Cloud.Sdk.Data.Dao.ConflictOption.AppendByVersion;
                case ConflictOption.Overwrite:
                    return Cloud.Sdk.Data.Dao.ConflictOption.Overwrite;
                case ConflictOption.Replace:
                    return Cloud.Sdk.Data.Dao.ConflictOption.Replace;
                case ConflictOption.Merge:
                    return Cloud.Sdk.Data.Dao.ConflictOption.Merge;
                case ConflictOption.OverwriteByLastModifiedTime:
                    return Cloud.Sdk.Data.Dao.ConflictOption.OverwriteByLastModifiedTime;
                default:
                    return Cloud.Sdk.Data.Dao.ConflictOption.Skip;
            }
        }
        private static Cloud.Sdk.Data.Dao.PhysicalHoldConflictOption GetWebApiPhysicalHoldConflictOption(PhysicalHoldConflictOption physicalHoldConflictOption)
        {
            switch (physicalHoldConflictOption)
            {
                case PhysicalHoldConflictOption.UseDesDefinedHoldSetting:
                    return Cloud.Sdk.Data.Dao.PhysicalHoldConflictOption.UseDesDefinedHoldSetting;
                case PhysicalHoldConflictOption.CompareHoldSetting:
                    return Cloud.Sdk.Data.Dao.PhysicalHoldConflictOption.CompareHoldSetting;
                default:
                    return Cloud.Sdk.Data.Dao.PhysicalHoldConflictOption.UseDesDefinedHoldSetting;
            }
        }
        //private static Cloud.Sdk.Data.Dao.FSTreeNodeDto ConvertToFSTreeNodeDtoInfo(AvePoint.GCommon.Contract.Tree.Object.FSTreeNodeDto info)
        //{
        //    var result = new Cloud.Sdk.Data.Dao.FSTreeNodeDto();
        //    result.CanChildrenBeLoaded = info.CanChildrenBeLoaded;
        //    result.CheckNumber = info.CheckNumber;
        //    result.Children = ConvertToListFSTreeNodeDtoInfo(info.Children);
        //    result.ChildrenCount = info.ChildrenCount;
        //    result.ChildrenLoaded = info.ChildrenLoaded;
        //    //result.Description = info.Description;
        //    result.Expanded = info.Expanded;
        //    //result.FarmName = info.FarmName;
        //    result.FullPath = info.FullPath;
        //   // result.Hidden = info.Hidden;
        //    //result.FarmId = info.FarmID;
        //    result.Id = string.IsNullOrEmpty(info.ID) ? string.Empty : info.ID;
        //    result.Level = (Cloud.Sdk.Data.Dao.NodeLevel)info.Level;
        //   // result.LoginName = info.LoginName;
        //    result.Name = info.Name;
        //    //result.NodeExtension = ConvertToNodeExtensionDto(info.NodeExtension);
        //    result.OffSet = info.Offset;
        //    //result.SPObjectId = info.SPObjectId;
        //    //result.SPType = (Cloud.Sdk.Data.Dao.SPType)info.SPType;
        //    //result.SPVersion = info.SPVersion;
        //    //result.Template = info.Template;
        //    //result.Url = info.Url;
        //    if (info.Parent != null && result.Parent == null)
        //    {
        //        Cloud.Sdk.Data.Dao.FSTreeNodeDto tempParent = new Cloud.Sdk.Data.Dao.FSTreeNodeDto();
        //        tempParent.Children = new List<Cloud.Sdk.Data.Dao.FSTreeNodeDto>() { result };
        //        result.Parent = ConvertToFSTreeNodeDtoInfo(info.Parent);
        //    }
        //    result.Parent = ConvertToFSTreeNodeDtoInfo(info.Parent);
        //    return result;
        //}
        //private static Cloud.Sdk.Data.Dao.SPTreeNodeDto ConvertToSPTreeNodeDtoInfo(AvePoint.GCommon.Contract.Tree.Object.SPTreeNodeDto info)
        //{
        //    var result = new Cloud.Sdk.Data.Dao.SPTreeNodeDto();
        //    result.CanChildrenBeLoaded = info.CanChildrenBeLoaded;
        //    result.CheckNumber = info.CheckNumber;
        //    result.Children = ConvertToListSPTreeNodeDto(info.Children);
        //    result.ChildrenCount = info.ChildrenCount;
        //    result.ChildrenLoaded = info.ChildrenLoaded;
        //    result.Description = info.Description;
        //    result.Expanded = info.Expanded;
        //    result.FarmName = info.FarmName;
        //    result.FullPath = info.FullPath;
        //    result.Hidden = info.Hidden;
        //    result.FarmId = info.FarmID;
        //    result.Id = info.ID;
        //    result.Level = (Cloud.Sdk.Data.Dao.NodeLevel)info.Level;
        //    result.LoginName = info.LoginName;
        //    result.Name = info.Name;
        //    result.NodeExtension = ConvertToNodeExtensionDto(info.NodeExtension);
        //    result.OffSet = info.Offset;
        //    result.SPObjectId = info.SPObjectId;
        //    result.SPType = (Cloud.Sdk.Data.Dao.SPType)info.SPType;
        //    result.SPVersion = info.SPVersion;
        //    result.Template = info.Template;
        //    result.Url = info.Url;
        //    if (info.Parent != null && result.Parent == null)
        //    {
        //        Cloud.Sdk.Data.Dao.SPTreeNodeDto tempParent = new Cloud.Sdk.Data.Dao.SPTreeNodeDto();
        //        tempParent.Children = new List<Cloud.Sdk.Data.Dao.SPTreeNodeDto>() { result };
        //        result.Parent = ConvertToSPTreeNodeDtoInfo(info.Parent);
        //    }
        //    result.Parent = ConvertToSPTreeNodeDtoInfo(info.Parent);
        //    return result;
        //}
        //private static List<Cloud.Sdk.Data.Dao.SPTreeNodeDto> ConvertToListSPTreeNodeDto(IList<AvePoint.GCommon.Contract.Tree.Object.SPTreeNodeDto> info)
        //{
        //    var result = new List<Cloud.Sdk.Data.Dao.SPTreeNodeDto>();
        //    foreach (var item in info)
        //    {
        //        result.Add(ConvertToSPTreeNodeDtoInfo(item));
        //    }
        //    return result;
        //}

        //private static List<Cloud.Sdk.Data.Dao.FSTreeNodeDto> ConvertToListFSTreeNodeDtoInfo(IList<AvePoint.GCommon.Contract.Tree.Object.FSTreeNodeDto> info)
        //{
        //    var result = new List<Cloud.Sdk.Data.Dao.FSTreeNodeDto>();
        //    foreach (var item in info)
        //    {
        //        result.Add(ConvertToFSTreeNodeDtoInfo(item));
        //    }
        //    return result;
        //}

        //private static List<FSTreeNodeDto> ConvertToListFSTreeNodeDto(IList<Cloud.Sdk.Data.Dao.FSTreeNodeDto> info)
        //{
        //    var result = new List<FSTreeNodeDto>();
        //    foreach (var item in info)
        //    {
        //        result.Add(ConvertToFSTreeNodeDtoInfo(item));
        //    }
        //    return result;
        //}

        //private static AvePoint.GCommon.Contract.Tree.Object.FSTreeNodeDto ConvertToFSTreeNodeDtoInfo(Cloud.Sdk.Data.Dao.FSTreeNodeDto info)
        //{
        //    var result = new AvePoint.GCommon.Contract.Tree.Object.FSTreeNodeDto();
        //    result.CanChildrenBeLoaded = info.CanChildrenBeLoaded;
        //    result.CheckNumber = info.CheckNumber;
        //    result.Children = ConvertToListFSTreeNodeDto(info.Children);
        //    result.ChildrenCount = info.ChildrenCount;
        //    result.ChildrenLoaded = info.ChildrenLoaded;

        //    result.Expanded = info.Expanded;
        //    result.FullPath = info.FullPath;

        //    result.ID = info.Id;
        //    result.Level = (AvePoint.GCommon.Contract.Tree.Object.NodeLevel)info.Level;

        //    result.Name = info.Name;
        //    result.NodeExtension = ConvertToNodeExtensionDto(info.NodeExtension);
        //    result.Offset = info.OffSet;
        //    result.Parent = ConvertToFSTreeNodeDtoInfo(info.Parent);


        //    if (info.Parent != null && result.Parent == null)
        //    {
        //        AvePoint.GCommon.Contract.Tree.Object.FSTreeNodeDto tempParent = new AvePoint.GCommon.Contract.Tree.Object.FSTreeNodeDto();
        //        tempParent.Children = new List<AvePoint.GCommon.Contract.Tree.Object.FSTreeNodeDto>() { result };
        //        result.Parent = ConvertToFSTreeNodeDtoInfo(info.Parent);
        //    }
        //    return result;
        //}
        //private static AvePoint.GCommon.Contract.Tree.Object.SPTreeNodeDto ConvertToSPTreeNodeDtoInfo(Cloud.Sdk.Data.Dao.SPTreeNodeDto info)
        //{
        //    var result = new AvePoint.GCommon.Contract.Tree.Object.SPTreeNodeDto();
        //    result.CanChildrenBeLoaded = info.CanChildrenBeLoaded;
        //    result.CheckNumber = info.CheckNumber;
        //    result.Children = ConvertToListSPTreeNodeDto(info.Children);
        //    result.ChildrenCount = info.ChildrenCount;
        //    result.ChildrenLoaded = info.ChildrenLoaded;
        //    result.Description = info.Description;
        //    result.Expanded = info.Expanded;
        //    result.FarmID = info.FarmId;
        //    result.FarmName = info.FarmName;
        //    result.FullPath = info.FullPath;
        //    result.Hidden = info.Hidden;
        //    result.ID = info.Id;
        //    result.Level = (AvePoint.GCommon.Contract.Tree.Object.NodeLevel)info.Level;
        //    result.LoginName = info.LoginName;
        //    result.Name = info.Name;
        //    result.NodeExtension = ConvertToNodeExtensionDto(info.NodeExtension);
        //    result.Offset = info.OffSet;
        //    result.Parent = ConvertToSPTreeNodeDtoInfo(info.Parent);
        //    result.SPObjectId = info.SPObjectId;
        //    result.SPType = (AvePoint.GCommon.Contract.Tree.Object.SPType)info.SPType;
        //    result.SPVersion = info.SPVersion;
        //    result.Template = info.Template;
        //    result.Url = info.Url;

        //    if (info.Parent != null && result.Parent == null)
        //    {
        //        AvePoint.GCommon.Contract.Tree.Object.SPTreeNodeDto tempParent = new AvePoint.GCommon.Contract.Tree.Object.SPTreeNodeDto();
        //        tempParent.Children = new List<AvePoint.GCommon.Contract.Tree.Object.SPTreeNodeDto>() { result };
        //        result.Parent = ConvertToSPTreeNodeDtoInfo(info.Parent);
        //    }
        //    return result;
        //}
        //private static List<AvePoint.GCommon.Contract.Tree.Object.SPTreeNodeDto> ConvertToListSPTreeNodeDto(IList<Cloud.Sdk.Data.Dao.SPTreeNodeDto> info)
        //{
        //    var result = new List<AvePoint.GCommon.Contract.Tree.Object.SPTreeNodeDto>();
        //    foreach (var item in info)
        //    {
        //        result.Add(ConvertToSPTreeNodeDtoInfo(item));
        //    }
        //    return result;
        //}
        private static List<Cloud.Sdk.Data.Dao.UserInfo> ConvertToArchiverRuleUserInfo(List<UserInfo> users)
        {
            List<Cloud.Sdk.Data.Dao.UserInfo> userList = new List<Cloud.Sdk.Data.Dao.UserInfo>();
            if (users == null)
            {
                return userList;
            }
            foreach (var user in users)
            {
                userList.Add(new Cloud.Sdk.Data.Dao.UserInfo()
                {
                    UserId = user.UserId,
                    UserPrincipalName = user.UserPrincipalName,
                    DisplayName = user.DisplayName,
                    Email = user.Email,
                    InviteType = (Cloud.Sdk.Data.Dao.InviteType)user.InviteType
                });
            }
            return userList;
        }

        private static List<UserInfo> ConvertToSORuleUserInfo(List<Cloud.Sdk.Data.Dao.UserInfo> users)
        {

            List<UserInfo> userList = new List<UserInfo>();
            if (users == null)
            {
                return userList;
            }
            foreach (var user in users)
            {
                userList.Add(new UserInfo()
                {
                    UserId = user.UserId,
                    UserPrincipalName = user.UserPrincipalName,
                    DisplayName = user.DisplayName,
                    Email = user.Email,
                    InviteType = (GCommon.Contract.Server.Login.InviteType)user.InviteType
                });
            }
            return userList;
        }

        private static Dictionary<AvePoint.GCommon.Contract.CommonFilter.PolicyLevel, string> ConvertToSORulePolicyLevel(List<Cloud.Sdk.Data.Dao.AndOrExpression> andOrExpression)
        {
            Dictionary<AvePoint.GCommon.Contract.CommonFilter.PolicyLevel, string> result = new Dictionary<AvePoint.GCommon.Contract.CommonFilter.PolicyLevel, string>();
            foreach (var item in andOrExpression)
            {
                result.Add((AvePoint.GCommon.Contract.CommonFilter.PolicyLevel)item.Key, item.Value);
            }
            return result;
        }

        private static Cloud.Sdk.Data.Dao.ArchiverSetting ConvertToArchiverRuleArchiverSetting(AvePoint.GCommon.Contract.StorageOptimization.Object.ArchiverSetting setting)
        {
            if (null == setting) return null;
            Cloud.Sdk.Data.Dao.ArchiverSetting result = new Cloud.Sdk.Data.Dao.ArchiverSetting();
            result.EnableArchiverVEOMerge = setting.EnableArchiverVEOMerge;
            result.FileNumber = setting.FileNumber;
            result.FileSize = setting.FileSize;
            result.FolderName = setting.FolderName;
            result.IsDeleteOldFile = setting.IsDeleteOldFile;
            result.NumberOfThreadSendingEmail = setting.NumberOfThreadSendingEmail;
            return result;
        }
        private static AvePoint.GCommon.Contract.StorageOptimization.Object.ArchiverSetting ConvertToSORuleArchiverSetting(Cloud.Sdk.Data.Dao.ArchiverSetting setting)
        {
            if (null == setting) return null;
            AvePoint.GCommon.Contract.StorageOptimization.Object.ArchiverSetting result = new AvePoint.GCommon.Contract.StorageOptimization.Object.ArchiverSetting();
            result.EnableArchiverVEOMerge = setting.EnableArchiverVEOMerge;
            result.FileNumber = setting.FileNumber;
            result.FileSize = setting.FileSize;
            result.FolderName = setting.FolderName;
            result.IsDeleteOldFile = setting.IsDeleteOldFile;
            result.NumberOfThreadSendingEmail = setting.NumberOfThreadSendingEmail;
            return result;
        }
        private static Cloud.Sdk.Data.Dao.ArchiverVEOSetting ConvertToArchiverRuleArchiverVEOSetting(AvePoint.GCommon.Contract.StorageOptimization.Object.ArchiverVEOSetting setting)
        {
            if (null == setting) return null;
            Cloud.Sdk.Data.Dao.ArchiverVEOSetting result = new Cloud.Sdk.Data.Dao.ArchiverVEOSetting();
            result.AgencyId = setting.AgencyId;
            result.ConsignmentNumber = setting.ConsignmentNumber;
            result.SeriesIdentifier = setting.SeriesIdentifier;
            result.SeriesNumber = setting.SeriesNumber;
            return result;
        }
        private static AvePoint.GCommon.Contract.StorageOptimization.Object.ArchiverVEOSetting ConvertToSORuleArchiverVEOSetting(Cloud.Sdk.Data.Dao.ArchiverVEOSetting setting)
        {
            if (null == setting) return null;
            AvePoint.GCommon.Contract.StorageOptimization.Object.ArchiverVEOSetting result = new AvePoint.GCommon.Contract.StorageOptimization.Object.ArchiverVEOSetting();
            result.AgencyId = setting.AgencyId;
            result.ConsignmentNumber = setting.ConsignmentNumber;
            result.SeriesIdentifier = setting.SeriesIdentifier;
            result.SeriesNumber = setting.SeriesNumber;
            return result;
        }
        private static Cloud.Sdk.Data.Dao.SOExportInfo ConvertToArchiverRuleExportInfo(AvePoint.GCommon.Contract.StorageOptimization.Object.SOExportInfo info)
        {
            if (null == info) return null;
            Cloud.Sdk.Data.Dao.SOExportInfo result = new Cloud.Sdk.Data.Dao.SOExportInfo();
            result.exportLocationId = info.exportLocationId;
            result.exportLocationName = info.exportLocationName;
            result.exportSPDataOption = (Cloud.Sdk.Data.Dao.ExportSPDataOption)info.exportSPDataOption;
            result.exportType = (Cloud.Sdk.Data.Dao.ExportTypeValue)info.exportType;
            return result;
        }
        private static AvePoint.GCommon.Contract.StorageOptimization.Object.SOExportInfo ConvertToSORuleExportInfo(Cloud.Sdk.Data.Dao.SOExportInfo info)
        {
            if (null == info) return null;
            AvePoint.GCommon.Contract.StorageOptimization.Object.SOExportInfo result = new AvePoint.GCommon.Contract.StorageOptimization.Object.SOExportInfo();
            result.exportLocationId = info.exportLocationId;
            result.exportLocationName = info.exportLocationName;
            result.exportSPDataOption = (AvePoint.GCommon.Contract.StorageOptimization.Object.ExportSPDataOption)info.exportSPDataOption;
            result.exportType = (AvePoint.GCommon.Contract.StorageOptimization.Object.ExportTypeValue)info.exportType;
            return result;
        }
        private static Cloud.Sdk.Data.Dao.MoveToRecordCenterAndDelareSetting ConvertToArchiverRuleMoveToRecordCenterAndDelareSetting(AvePoint.GCommon.Contract.StorageOptimization.Object.MoveToRecordCenterAndDelareSetting info)
        {
            if (null == info) return null;
            Cloud.Sdk.Data.Dao.MoveToRecordCenterAndDelareSetting result = new Cloud.Sdk.Data.Dao.MoveToRecordCenterAndDelareSetting();
            result.ContentConflictResolution = (Cloud.Sdk.Data.Dao.ContentConflictResolution)info.ContentConflictResolution;
            result.DestinationLocation = ConvertToArchiverRuleDestinationLocationInfo(info.DestinationLocation);
            result.OperateDataMode = (Cloud.Sdk.Data.Dao.OperatingSharePointDataMode)info.OperateDataMode;
            result.OriginalMetaDataAsXML = info.OriginalMetaDataAsXML;
            result.UseTransferedFileMode = (Cloud.Sdk.Data.Dao.UseTransferedFileMode)info.UseTransferedFileMode;
            result.DelaredRecord = info.DelaredRecord;
            return result;
        }
        private static AvePoint.GCommon.Contract.StorageOptimization.Object.MoveToRecordCenterAndDelareSetting ConvertToSORuleMoveToRecordCenterAndDelareSetting(Cloud.Sdk.Data.Dao.MoveToRecordCenterAndDelareSetting info)
        {
            if (null == info) return null;
            AvePoint.GCommon.Contract.StorageOptimization.Object.MoveToRecordCenterAndDelareSetting result = new AvePoint.GCommon.Contract.StorageOptimization.Object.MoveToRecordCenterAndDelareSetting();
            result.ContentConflictResolution = (AvePoint.GCommon.Contract.StorageOptimization.Object.ContentConflictResolution)info.ContentConflictResolution;
            result.DestinationLocation = ConvertToSORuleDestinationLocationInfo(info.DestinationLocation);
            result.OperateDataMode = (AvePoint.GCommon.Contract.StorageOptimization.Object.OperatingSharePointDataMode)info.OperateDataMode;
            result.OriginalMetaDataAsXML = info.OriginalMetaDataAsXML;
            result.UseTransferedFileMode = (AvePoint.GCommon.Contract.StorageOptimization.Object.UseTransferedFileMode)info.UseTransferedFileMode;
            result.DelaredRecord = info.DelaredRecord;
            return result;
        }
        private static Cloud.Sdk.Data.Dao.DestinationLocationInfo ConvertToArchiverRuleDestinationLocationInfo(AvePoint.GCommon.Contract.StorageOptimization.Object.DestinationLocationInfo info)
        {
            if (null == info) return null;
            Cloud.Sdk.Data.Dao.DestinationLocationInfo result = new Cloud.Sdk.Data.Dao.DestinationLocationInfo();
            result.Password = info.Password;
            result.Url = info.Url;
            result.UserName = info.UserName;
            return result;
        }
        private static AvePoint.GCommon.Contract.StorageOptimization.Object.DestinationLocationInfo ConvertToSORuleDestinationLocationInfo(Cloud.Sdk.Data.Dao.DestinationLocationInfo info)
        {
            if (null == info) return null;
            AvePoint.GCommon.Contract.StorageOptimization.Object.DestinationLocationInfo result = new AvePoint.GCommon.Contract.StorageOptimization.Object.DestinationLocationInfo();
            result.Password = info.Password;
            result.Url = info.Url;
            result.UserName = info.UserName;
            return result;
        }
        private static List<Cloud.Sdk.Data.Dao.SOFilterPolicy> ConvertToArchiverRuleSOFilterPolicy(List<AvePoint.GCommon.Contract.StorageOptimization.Object.SOFilterPolicy> soFilters)
        {
            List<Cloud.Sdk.Data.Dao.SOFilterPolicy> result = new List<Cloud.Sdk.Data.Dao.SOFilterPolicy>();
            if (soFilters == null)
            {
                return result;
            }
            foreach (var item in soFilters)
            {
                Cloud.Sdk.Data.Dao.SOFilterPolicy soFilter = new Cloud.Sdk.Data.Dao.SOFilterPolicy();
                soFilter.BeginTime = item.BeginTime == null ? null : ConvertToArchiverRuleDisplayDateTime(item.BeginTime);
                soFilter.Condition = (Cloud.Sdk.Data.Dao.PolicyCondition)item.Condition;
                soFilter.EndTime = item.EndTime == null ? null : ConvertToArchiverRuleDisplayDateTime(item.EndTime);
                soFilter.IsAnd = item.IsAnd;
                soFilter.Level = (Cloud.Sdk.Data.Dao.PolicyLevel)item.Level;
                soFilter.Result = item.Result == null ? false : item.Result;
                soFilter.Rule = ConvertPolicyRuleBaseToApiPolicyRuleBase(item.Rule);
                soFilter.RuleGUIType = (Cloud.Sdk.Data.Dao.RuleGUIType)item.RuleGUIType;
                soFilter.RuleType = (Cloud.Sdk.Data.Dao.PolicyRuleType)item.RuleType;
                soFilter.SequenceNo = item.SequenceNo;
                soFilter.Value = ConvertPolicyValueToApiPolicyValue(item.Value);
                result.Add(soFilter);
            }
            return result;
        }
        private static List<AvePoint.GCommon.Contract.StorageOptimization.Object.SOFilterPolicy> ConvertToSORuleSOFilterPolicy(List<Cloud.Sdk.Data.Dao.SOFilterPolicy> soFilters)
        {
            List<AvePoint.GCommon.Contract.StorageOptimization.Object.SOFilterPolicy> result = new List<AvePoint.GCommon.Contract.StorageOptimization.Object.SOFilterPolicy>();
            foreach (var item in soFilters)
            {
                AvePoint.GCommon.Contract.StorageOptimization.Object.SOFilterPolicy soFilter = new AvePoint.GCommon.Contract.StorageOptimization.Object.SOFilterPolicy();
                soFilter.BeginTime = item.BeginTime == null ? null : ConvertToSORuleDisplayDateTime(item.BeginTime);
                soFilter.Condition = (GCommon.Contract.CommonFilter.PolicyCondition)item.Condition;
                soFilter.EndTime = item.EndTime == null ? null : ConvertToSORuleDisplayDateTime(item.EndTime);
                soFilter.IsAnd = item.IsAnd;
                soFilter.Level = (GCommon.Contract.CommonFilter.PolicyLevel)item.Level;
                soFilter.Result = item.Result == null ? false : item.Result;
                soFilter.Rule = ConvertApiPolicyRuleBaseToPolicyRuleBase(item.Rule);
                soFilter.RuleGUIType = (GCommon.Contract.CommonFilter.RuleGUIType)item.RuleGUIType;
                soFilter.RuleType = (GCommon.Contract.CommonFilter.PolicyRuleType)item.RuleType;
                soFilter.SequenceNo = item.SequenceNo;
                soFilter.Value = ConvertApiPolicyValueToPolicyValue(item.Value);
                result.Add(soFilter);
            }
            return result;
        }
        private static Cloud.Sdk.Data.Dao.DisplayDateTime ConvertToArchiverRuleDisplayDateTime(AvePoint.GCommon.Contract.StorageOptimization.Object.DisplayDateTime info)
        {
            Cloud.Sdk.Data.Dao.DisplayDateTime result = new Cloud.Sdk.Data.Dao.DisplayDateTime();
            result.IsDayLightSaving = info.IsDayLightSaving;
            result.StartTime = info.StartTime;
            result.TimeZoneId = info.TimeZoneId;
            return result;
        }
        private static AvePoint.GCommon.Contract.StorageOptimization.Object.DisplayDateTime ConvertToSORuleDisplayDateTime(Cloud.Sdk.Data.Dao.DisplayDateTime info)
        {
            AvePoint.GCommon.Contract.StorageOptimization.Object.DisplayDateTime result = new AvePoint.GCommon.Contract.StorageOptimization.Object.DisplayDateTime();
            result.IsDayLightSaving = info.IsDayLightSaving;
            result.StartTime = info.StartTime;
            result.TimeZoneId = info.TimeZoneId;
            return result;
        }
        private static List<Cloud.Sdk.Data.Dao.TagContentInfo> ConvertToArchiverRuleTagContentInfo(List<AvePoint.GCommon.Contract.StorageOptimization.Object.TagContentInfo> infos)
        {
            List<Cloud.Sdk.Data.Dao.TagContentInfo> result = new List<Cloud.Sdk.Data.Dao.TagContentInfo>();
            foreach (var item in infos)
            {
                Cloud.Sdk.Data.Dao.TagContentInfo info = new Cloud.Sdk.Data.Dao.TagContentInfo();
                info.ColumnName = item.ColumnName;
                info.DateTime = item.DateTime;
                info.Type = (Cloud.Sdk.Data.Dao.TagContentInfoType)item.Type;
                info.Value = item.Value;
                result.Add(info);
            }
            return result;
        }
        private static List<AvePoint.GCommon.Contract.StorageOptimization.Object.TagContentInfo> ConvertToSORuleTagContentInfo(List<Cloud.Sdk.Data.Dao.TagContentInfo> infos)
        {
            List<AvePoint.GCommon.Contract.StorageOptimization.Object.TagContentInfo> result = new List<AvePoint.GCommon.Contract.StorageOptimization.Object.TagContentInfo>();
            foreach (var item in infos)
            {
                AvePoint.GCommon.Contract.StorageOptimization.Object.TagContentInfo info = new AvePoint.GCommon.Contract.StorageOptimization.Object.TagContentInfo();
                info.ColumnName = item.ColumnName;
                info.DateTime = item.DateTime;
                info.Type = (AvePoint.GCommon.Contract.StorageOptimization.Object.TagContentInfoType)item.Type;
                info.Value = item.Value;
                result.Add(info);
            }
            return result;
        }

        public static Cloud.Sdk.Data.Dao.SORuleNodeInfo ConvertToAPIRuleInfoContract(SORuleInfoContract info)
        {
            if (null == info) return null;
            Cloud.Sdk.Data.Dao.SORuleNodeInfo result = new Cloud.Sdk.Data.Dao.SORuleNodeInfo();
            result.Rules = new List<Cloud.Sdk.Data.Dao.ArchiverRule>();
            if (info.Rules != null && info.Rules.Count > 0)
            {
                foreach (var item in info.Rules)
                {
                    result.Rules.Add(ConvertSORuleToArchiverRule(item));
                }
            }
            result.TermRuleMapping = info.TermRuleMapping;
            result.RecordsStorageInfo = ConvertToAPIRecordsStorageInfo(info.RecordsStorageInfo);
            result.SourceFlag = info.SourceFlag;
            result.SOPlan = new Cloud.Sdk.Data.Dao.SOPlan();
            result.SOPlan.PlanType = info.Plan.PlanType;
            result.SOPlan.PlanCategory = (Cloud.Sdk.Data.Dao.PlanCategory)info.Plan.Category;
            result.SOPlan.RunNow = info.Plan.RunNow;
            result.SOPlan.RecordWebDBConnectionString = info.Plan.RecordWebDBConnectionString;
            result.SOPlan.RecordsHistoryDBConnectionString = info.Plan.RecordsHistoryDBConnectionString;
            result.SOPlan.RunJobUser = info.Plan.RunJobUser;
            result.SOPlan.GroupBCSColumnDictionary = info.Plan?.GroupBCSColumnDictionary != null ? SerializerHelper.SerializeByDataContractSerializer(info.Plan.GroupBCSColumnDictionary) : null;
            result.SOPlan.RecordsGlobalStorageSettings = ConvertToAPIRecordsGlobalStorageSettings(info.Plan.RecordsGlobalStorageSettings);
            result.SOPlan.SkipRemoveContentAndDestroyAction = info.Plan.SkipRemoveContentAndDestroyAction;
            result.SOPlan.IsRecordsOneDriveNode = info.Plan.IsRecordsOneDriveNode;
            result.SOPlan.IsNullClassificationSetting = info.Plan.IsNullClassificationSetting;            
            Cloud.Sdk.Data.Dao.CosmosConnectionInfo toApiCosmosConnectionInfo = null;
            if (info.Plan.RecordExplorerDB != null)
            {
                toApiCosmosConnectionInfo = new Cloud.Sdk.Data.Dao.CosmosConnectionInfo
                {
                    CollectionId = info.Plan.RecordExplorerDB.CollectionId,
                    DatabaseId = info.Plan.RecordExplorerDB.DatabaseId,
                    Endpoint = info.Plan.RecordExplorerDB.Endpoint,
                    Key = info.Plan.RecordExplorerDB.Key
                };
            }
            result.SOPlan.RecordExplorerDB = toApiCosmosConnectionInfo;
            result.SOPlan.SOPlanExtension = new Cloud.Sdk.Data.Dao.SOPlanExtension();
            if (info.Plan.SOPlanExtension != null)
            {
                result.SOPlan.SOPlanExtension.DisableIRMSetting = info.Plan.SOPlanExtension.DisableIRMSetting;
                result.SOPlan.SOPlanExtension.EnableSuperUserDecryptsFiles = info.Plan.SOPlanExtension.EnableSuperUserDecryptsFiles;
                result.SOPlan.SOPlanExtension.IncludeListView = info.Plan.SOPlanExtension.IncludeListView;
                result.SOPlan.SOPlanExtension.IncludeTerm = info.Plan.SOPlanExtension.IncludeTerm;
                result.SOPlan.SOPlanExtension.ProcessingPoolId = info.Plan.SOPlanExtension.ProcessingPoolId;
                if (info.Plan.SOPlanExtension.WorkflowState != null)
                {
                    result.SOPlan.SOPlanExtension.WorkflowState = new Cloud.Sdk.Data.Dao.BackupRestoreWorkflow();
                    result.SOPlan.SOPlanExtension.WorkflowState.DefinitionConflictResolution = (Cloud.Sdk.Data.Dao.WorkflowConflictResolutionType)info.Plan.SOPlanExtension.WorkflowState.DefinitionConflictResolution;
                    result.SOPlan.SOPlanExtension.WorkflowState.IncludeWorkflowDefinition = info.Plan.SOPlanExtension.WorkflowState.IncludeWorkflowDefinition;
                    result.SOPlan.SOPlanExtension.WorkflowState.IncludeWorkflowInstance = info.Plan.SOPlanExtension.WorkflowState.IncludeWorkflowInstance;
                    result.SOPlan.SOPlanExtension.WorkflowState.InstanceConflictResolution = (Cloud.Sdk.Data.Dao.WorkflowConflictResolutionType)info.Plan.SOPlanExtension.WorkflowState.InstanceConflictResolution;
                }
            }
            return result;
        }

        public static Cloud.Sdk.Data.Dao.RecordsGlobalStorageSettings ConvertToAPIRecordsGlobalStorageSettings(RecordsGlobalStorageSettings recordsGlobalStorageSettings)
        {
            Cloud.Sdk.Data.Dao.RecordsGlobalStorageSettings result = null;
            if (recordsGlobalStorageSettings != null)
            {
                result = new Cloud.Sdk.Data.Dao.RecordsGlobalStorageSettings();
                Cloud.Sdk.Data.Dao.DataSecurity encryptionMethod = recordsGlobalStorageSettings.UseEncryption ? ConvertToAPIDataSecurity(recordsGlobalStorageSettings.EncryptionMethod) : Cloud.Sdk.Data.Dao.DataSecurity.None;
                Cloud.Sdk.Data.Dao.DataSecurity compressionMethod = recordsGlobalStorageSettings.UseCompression ? ConvertToAPIDataSecurity(recordsGlobalStorageSettings.CompressionMethod) : Cloud.Sdk.Data.Dao.DataSecurity.None;
                result.UseCompression = recordsGlobalStorageSettings.UseCompression;
                result.UseEncryption = recordsGlobalStorageSettings.UseEncryption;
                result.CompressionMethod = compressionMethod;
                result.EncryptionMethod = encryptionMethod;
                result.CompressionSpeed = recordsGlobalStorageSettings.CompressionSpeed;
                result.SecurityProfileId = recordsGlobalStorageSettings.SecurityProfileId;
                result.SecurityProfileName = recordsGlobalStorageSettings.SecurityProfileName;
                result.StoragePolicyId = recordsGlobalStorageSettings.StoragePolicyId;
                result.StoragePolicyName = recordsGlobalStorageSettings.StoragePolicyName;
                result.ExportLocationId = recordsGlobalStorageSettings.ExportLocationId;
                result.ExportLocationName = recordsGlobalStorageSettings.ExportLocationName;
            }
            return result;
        }

        public static Cloud.Sdk.Data.Dao.RecordsStorageInfo ConvertToAPIRecordsStorageInfo(RecordsStorageInfo  recordsStorageInfo)
        {
            Cloud.Sdk.Data.Dao.RecordsStorageInfo result = null;
            if (recordsStorageInfo != null)
            {
                result = new Cloud.Sdk.Data.Dao.RecordsStorageInfo()
                {
                    ArchiverCompressionType =( Cloud.Sdk.Data.Dao.CompressionType)recordsStorageInfo.ArchiverCompressionType,
                    ArchiverDataSecurity = (Cloud.Sdk.Data.Dao.DataSecurity)recordsStorageInfo.ArchiverDataSecurity,
                    ArchiverSetting = ConvertToArchiverRuleArchiverSetting(recordsStorageInfo.ArchiverSetting),
                    ArchiverVEOSetting = ConvertToArchiverRuleArchiverVEOSetting(recordsStorageInfo.ArchiverVEOSetting),
                    DataEncryptionProfileId = recordsStorageInfo.DataEncryptionProfileId,
                    DataEncryptionProfileName = recordsStorageInfo.DataEncryptionProfileName,
                    ExportDataEncryptionIV = recordsStorageInfo.ExportDataEncryptionIV,
                    ExportDataEncryptionKey = recordsStorageInfo.ExportDataEncryptionKey,
                    ExportLocationId = recordsStorageInfo.ExportLocationId,
                    ExportLocationName = recordsStorageInfo.ExportLocationName,
                    FileVEO = recordsStorageInfo.FileVEO,
                    ManifestVEO = recordsStorageInfo.ManifestVEO,
                    NAAConfigFile = recordsStorageInfo.NAAConfigFile,
                    NARAConfigFile = recordsStorageInfo.NARAConfigFile,
                    RecordVEO = recordsStorageInfo.RecordVEO,
                    StoragePolicyId = recordsStorageInfo.StoragePolicyId,
                    StoragePolicyName = recordsStorageInfo.StoragePolicyName
                };

                result.StubTemplatesList = new();
                foreach (var stub in recordsStorageInfo.StubTemplatesList ?? new())
                {
                    result.StubTemplatesList.Add(new()
                    {
                        Id = stub.Id,
                        Name = stub.Name,
                        StubType = stub.StubType,
                        StubContent = stub.StubContent,
                        IsDeclareStubAsRecords = stub.IsDeclareStubAsRecords,
                    });
                }
            }
            return result;
        }

        public static Cloud.Sdk.Data.Dao.DataSecurity ConvertToAPIDataSecurity(AvePoint.GCommon.Contract.GranularBackup.Object.DataSecurity dataSecurity)
        {
            Cloud.Sdk.Data.Dao.DataSecurity result = Cloud.Sdk.Data.Dao.DataSecurity.None;
            switch (dataSecurity)
            {
                case GCommon.Contract.GranularBackup.Object.DataSecurity.CompressionMedia:
                    result = Cloud.Sdk.Data.Dao.DataSecurity.CompressionMedia;
                    break;
                case GCommon.Contract.GranularBackup.Object.DataSecurity.CompressionAgent:
                    result = Cloud.Sdk.Data.Dao.DataSecurity.CompressionAgent;
                    break;
                case GCommon.Contract.GranularBackup.Object.DataSecurity.EncryptionMedia:
                    result = Cloud.Sdk.Data.Dao.DataSecurity.EncryptionMedia;
                    break;
                case GCommon.Contract.GranularBackup.Object.DataSecurity.EncryptionAgent:
                    result = Cloud.Sdk.Data.Dao.DataSecurity.EncryptionAgent;
                    break;
                default:
                    break;
            }
            return result;
        }

        public static Cloud.Sdk.Data.Dao.BreakInheritingNode ConvertToAPIRuleNodeContactList(List<RuleNodeContract> breakInheritingNodes)
        {
            Cloud.Sdk.Data.Dao.BreakInheritingNode results = new Cloud.Sdk.Data.Dao.BreakInheritingNode();
            results.NodeInfoList = new List<Cloud.Sdk.Data.Dao.BreakInheritingNodeNodeInfo>();
            if (breakInheritingNodes == null || breakInheritingNodes.Count == 0)
            {
                return results;
            }
            foreach (var item in breakInheritingNodes)
            {
                Cloud.Sdk.Data.Dao.BreakInheritingNodeNodeInfo node = new Cloud.Sdk.Data.Dao.BreakInheritingNodeNodeInfo();
                node.AlliancedRuleNames = item.AlliancedRuleNames;
                if (item.BposInfo != null)
                {
                    node.BposInfo = new Cloud.Sdk.Data.Dao.BposInfo();
                    node.BposInfo.AppType = (Cloud.Sdk.Data.Dao.AppType)item.BposInfo.AppType;
                    node.BposInfo.ConnectionType = (Cloud.Sdk.Data.Dao.BposConnectionType)item.BposInfo.ConnectionType;
                    node.BposInfo.MailboxType = (Cloud.Sdk.Data.Dao.MailboxType)item.BposInfo.MailboxType;
                    node.BposInfo.Mode = (Cloud.Sdk.Data.Dao.BPOSMode)item.BposInfo.Mode;
                    node.BposInfo.SiteUrl = item.BposInfo.SiteUrl;
                    node.BposInfo.UserAccountInfo = new Cloud.Sdk.Data.Dao.BposUserAccountInfo();
                    if (item.BposInfo.UserAccountInfo != null)
                    {
                        node.BposInfo.UserAccountInfo.AdminUrl = item.BposInfo.UserAccountInfo.AdminUrl;
                        node.BposInfo.UserAccountInfo.Domain = item.BposInfo.UserAccountInfo.Domain;
                        node.BposInfo.UserAccountInfo.Password = item.BposInfo.UserAccountInfo.Password;
                        node.BposInfo.UserAccountInfo.SecondaryPassword = item.BposInfo.UserAccountInfo.SecondaryPassword;
                        node.BposInfo.UserAccountInfo.SecondaryUsername = item.BposInfo.UserAccountInfo.SecondaryUsername;
                        node.BposInfo.UserAccountInfo.TenantId = item.BposInfo.UserAccountInfo.TenantId;
                        node.BposInfo.UserAccountInfo.Username = item.BposInfo.UserAccountInfo.Username;
                    }
                }
                node.DisplayName = item.DisplayName;
                if (item.Extension != null)
                {
                    node.Extension = new Cloud.Sdk.Data.Dao.RuleNodeExtension();
                    node.Extension.ContentDatabaseName = item.Extension.ContentDatabaseName;
                    node.Extension.IsUpgradeData = item.Extension.IsUpgradeData;
                    node.Extension.UsedcrawlProfile = item.Extension.UsedcrawlProfile;
                }
                node.FarmId = item.FarmId;
                node.FarmName = item.FarmName;
                node.FullPath = item.FullPath;
                node.Id = item.Id;
                node.IndexDeviceId = item.IndexDeviceId;
                node.ListId = item.ListId;
                node.ListTitle = item.ListTitle;
                node.ManagerTreeId = item.ManagerTreeId;
                node.NodeId = item.NodeId;
                node.NodeLevel = (Cloud.Sdk.Data.Dao.NodeLevel)item.NodeLevel;
                node.NodeName = item.NodeName;
                node.ParentNodeId = item.ParentNodeId;
                node.ParentNodeName = item.ParentNodeName;
                node.PlanId = item.PlanId;
                node.ProfileName = item.ProfileName;
                node.ProviderType = (Cloud.Sdk.Data.Dao.BlobProviderType)item.ProviderType;
                node.SiteId = item.SiteId;
                node.SiteUrl = item.SiteUrl;
                node.SPVersion = item.SPVersion;
                node.Type = (Cloud.Sdk.Data.Dao.RuleNodeType)item.Type;
                node.WebAppId = item.WebAppId;
                node.WebAppUrl = item.WebAppUrl;
                node.WebId = item.WebId;
                results.NodeInfoList.Add(node);
            }
            return results;
        }

        private static List<Cloud.Sdk.Data.Dao.FilterPolicy> ConvertFilterPolicyToApiFilterPolicy(List<AvePoint.GCommon.Contract.CommonFilter.FilterPolicy> list)
        {
            var result = new List<Cloud.Sdk.Data.Dao.FilterPolicy>();
            foreach (var item in list)
            {
                var policy = new Cloud.Sdk.Data.Dao.FilterPolicy
                {
                    Condition = (Cloud.Sdk.Data.Dao.PolicyCondition)item.Condition,
                    Level = (Cloud.Sdk.Data.Dao.PolicyLevel)item.Level,
                    Result = item.Result,
                    Rule = ConvertPolicyRuleBaseToApiPolicyRuleBase(item.Rule),
                    RuleGUIType = (Cloud.Sdk.Data.Dao.RuleGUIType)item.RuleGUIType,
                    RuleType = (Cloud.Sdk.Data.Dao.PolicyRuleType)item.RuleType,
                    SequenceNo = item.SequenceNo,
                    Value = ConvertPolicyValueToApiPolicyValue(item.Value),

                };
                result.Add(policy);
            }

            return result;
        }
        private static List<AvePoint.GCommon.Contract.CommonFilter.FilterPolicy> ConvertApiFilterPolicyToFilterPolicy(List<Cloud.Sdk.Data.Dao.FilterPolicy> list)
        {
            var result = new List<AvePoint.GCommon.Contract.CommonFilter.FilterPolicy>();
            foreach (var item in list)
            {
                var policy = new AvePoint.GCommon.Contract.CommonFilter.FilterPolicy
                {
                    Condition = (AvePoint.GCommon.Contract.CommonFilter.PolicyCondition)item.Condition,
                    Level = (AvePoint.GCommon.Contract.CommonFilter.PolicyLevel)item.Level,
                    Result = item.Result,
                    Rule = ConvertApiPolicyRuleBaseToPolicyRuleBase(item.Rule),
                    RuleGUIType = (AvePoint.GCommon.Contract.CommonFilter.RuleGUIType)item.RuleGUIType,
                    RuleType = (AvePoint.GCommon.Contract.CommonFilter.PolicyRuleType)item.RuleType,
                    SequenceNo = item.SequenceNo,
                    Value = ConvertApiPolicyValueToPolicyValue(item.Value),

                };
                result.Add(policy);
            }

            return result;
        }
        private static Cloud.Sdk.Data.Dao.PolicyRuleBase ConvertPolicyRuleBaseToApiPolicyRuleBase(GCommon.Contract.CommonFilter.PolicyRuleBase rulebase)
        {
            var fullName = $"Cloud.Sdk.Data.Dao.{rulebase.GetType().Name}";
            var assembly = Array.Find(AppDomain.CurrentDomain.GetAssemblies(), (System.Reflection.Assembly a) => a.FullName.StartsWith("Cloud.Sdk.Data.Dao"));
            var result = (Cloud.Sdk.Data.Dao.PolicyRuleBase)assembly.CreateInstance(fullName);
            result.Value1 = rulebase.Value1;
            result.Type = rulebase.GetType().Name;
            return result;

        }
        private static GCommon.Contract.CommonFilter.PolicyRuleBase ConvertApiPolicyRuleBaseToPolicyRuleBase(Cloud.Sdk.Data.Dao.PolicyRuleBase rulebase)
        {
            var fullName = $"AvePoint.GCommon.Contract.CommonFilter.{rulebase.Type}";
            var assembly = Array.Find(AppDomain.CurrentDomain.GetAssemblies(), (System.Reflection.Assembly a) => a.FullName.StartsWith("CommonContract"));
            var result = (GCommon.Contract.CommonFilter.PolicyRuleBase)assembly.CreateInstance(fullName);
            result.Value1 = rulebase.Value1;
            return result;

        }
        private static Cloud.Sdk.Data.Dao.PolicyValue ConvertPolicyValueToApiPolicyValue(GCommon.Contract.CommonFilter.PolicyValue value)
        {
            var policyValue = new Cloud.Sdk.Data.Dao.PolicyValue()
            {
                Value1 = value.Value1,
                Value1Unit = (Cloud.Sdk.Data.Dao.PolicyValueUnit)value.Value1Unit,
                Value2 = value.Value2,
                Value2Unit = (Cloud.Sdk.Data.Dao.PolicyValueUnit)value.Value2Unit
            };
            if (value.Extension != null)
            {
                policyValue.Extension = new Cloud.Sdk.Data.Dao.Extention()
                {
                    isDST = value.Extension.isDST,
                    TimeZoneId = value.Extension.TimeZoneId
                };
            }
            return policyValue;
        }
        private static GCommon.Contract.CommonFilter.PolicyValue ConvertApiPolicyValueToPolicyValue(Cloud.Sdk.Data.Dao.PolicyValue value)
        {
            var policyValue = new GCommon.Contract.CommonFilter.PolicyValue()
            {
                Value1 = value.Value1,
                Value1Unit = (GCommon.Contract.CommonFilter.PolicyValueUnit)value.Value1Unit,
                Value2 = value.Value2,
                Value2Unit = (GCommon.Contract.CommonFilter.PolicyValueUnit)value.Value2Unit
            };
            if (value.Extension != null)
            {
                policyValue.Extension = new GCommon.Contract.CommonFilter.Extention()
                {
                    isDST = value.Extension.isDST,
                    TimeZoneId = value.Extension.TimeZoneId
                };
            }
            return policyValue;
        }


        //public static ConvertToAPIRuleInfoContract(GCommon.Contract.StorageOptimization.Object.SORuleInfoContract ruleInfo) { }

        #endregion

        #region AzureTableConvert

        public static AzureTableConnectContract ConvertToAzureTableContract(Cloud.Sdk.Data.Dao.ArchiverDataBaseConfigInfo databaseConfig)
        {
            if (null == databaseConfig) return null;
            AzureTableConnectContract acc = new AzureTableConnectContract();
            acc.AccountKey = databaseConfig.AccountKey;
            acc.AccountName = databaseConfig.AccountName;
            acc.Endpoint = databaseConfig.Endpoint;
            return acc;
        }

        #endregion

        #region TreeNodeConvert
        public static RemoteWebApplication ConverToRemoteWeApp(Cloud.Sdk.Data.Dao.WebApplication webapp)
        {
            if (null == webapp) return null;
            RemoteWebApplication rwebapp = new RemoteWebApplication();
            rwebapp.id = webapp.Id;
            rwebapp.description = webapp.Id;
            rwebapp.domainName = webapp.DomainName;
            rwebapp.modifiedDate = webapp.ModifiedDate;
            rwebapp.NodeType = (RemoveNodeType)webapp.NodeType;
            rwebapp.url = webapp.Url;
            rwebapp.useSSL = webapp.UseSSL;
            return rwebapp;
        }
        public static RemoteSiteCollection ConverToRemoteSiteCollection(Cloud.Sdk.Data.Dao.SiteCollection remoteSiteCollection)
        {
            if (null == remoteSiteCollection) return null;
            RemoteSiteCollection sitecollection = new RemoteSiteCollection();
            sitecollection.AdminUrl = remoteSiteCollection.AdminUrl;
            sitecollection.AppType = (AvePoint.GCommon.Contract.CentralAdmin.Object.AppType)remoteSiteCollection.AppType;
            sitecollection.AuthType = (AvePoint.GCommon.Contract.CentralAdmin.Object.BposConnectionType)remoteSiteCollection.AuthType;
            sitecollection.AvailableAgentIds = remoteSiteCollection.AvailableAgentIds;
            sitecollection.BPOSMould = remoteSiteCollection.BPOSMould;
            sitecollection.CreateTime = remoteSiteCollection.CreateTime;
            sitecollection.domain = remoteSiteCollection.Domain;
            sitecollection.id = remoteSiteCollection.Id;
            sitecollection.IsPublicWebSite = remoteSiteCollection.IsPublicWebSite;
            sitecollection.Name = remoteSiteCollection.Name;
            sitecollection.NodeType = (AvePoint.GCommon.Contract.Server.ControlPanel.Office365.RemoveNodeType)remoteSiteCollection.NodeType;
            sitecollection.parentId = remoteSiteCollection.ParentId;
            sitecollection.password = remoteSiteCollection.Password;
            sitecollection.ServiceAccountId = remoteSiteCollection.ServiceAccountId;
            sitecollection.SiteCollectionType = (AvePoint.GCommon.Contract.SharePointBrowser.SiteCollectionType)remoteSiteCollection.SiteCollectionType;
            sitecollection.SPVersion = remoteSiteCollection.SPVersion;
            sitecollection.state = (AvePoint.GCommon.Contract.Server.ControlPanel.Office365.SiteCollectionState)remoteSiteCollection.State;
            sitecollection.TemplateName = remoteSiteCollection.TemplateName;
            sitecollection.TemplateTitle = remoteSiteCollection.TemplateTitle;
            sitecollection.TenantGroupId = remoteSiteCollection.TenantGroupId;
            sitecollection.TenantId = remoteSiteCollection.TenantId;
            sitecollection.url = remoteSiteCollection.Url;
            sitecollection.username = remoteSiteCollection.Username;
            sitecollection.AADEnvironment = (AADEnvironment)remoteSiteCollection.AADEnvironment;

            return sitecollection;
        }

        public static Cloud.Sdk.Data.Dao.SiteCollection ConverToAPISiteCollection(RemoteSiteCollection remoteSiteCollection)
        {
            if (null == remoteSiteCollection) return null;
            Cloud.Sdk.Data.Dao.SiteCollection sitecollection = new Cloud.Sdk.Data.Dao.SiteCollection();
            sitecollection.AdminUrl = remoteSiteCollection.AdminUrl;
            sitecollection.AppType = (Cloud.Sdk.Data.Dao.AppType)remoteSiteCollection.AppType;
            sitecollection.AuthType = (Cloud.Sdk.Data.Dao.BposConnectionType)remoteSiteCollection.AuthType;
            sitecollection.AvailableAgentIds = remoteSiteCollection.AvailableAgentIds;
            sitecollection.BPOSMould = remoteSiteCollection.BPOSMould;
            sitecollection.CreateTime = remoteSiteCollection.CreateTime;
            sitecollection.Domain = remoteSiteCollection.domain;
            sitecollection.Id = remoteSiteCollection.id;
            sitecollection.IsPublicWebSite = remoteSiteCollection.IsPublicWebSite;
            sitecollection.Name = remoteSiteCollection.Name;
            sitecollection.NodeType = (Cloud.Sdk.Data.Dao.RemoveNodeType)remoteSiteCollection.NodeType;
            sitecollection.ParentId = remoteSiteCollection.parentId;
            sitecollection.Password = remoteSiteCollection.password;
            sitecollection.ServiceAccountId = remoteSiteCollection.ServiceAccountId;
            sitecollection.SiteCollectionType = (Cloud.Sdk.Data.Dao.SiteCollectionType)remoteSiteCollection.SiteCollectionType;
            sitecollection.SPVersion = remoteSiteCollection.SPVersion;
            sitecollection.State = (Cloud.Sdk.Data.Dao.SiteCollectionState)remoteSiteCollection.state;
            sitecollection.TemplateName = remoteSiteCollection.TemplateName;
            sitecollection.TemplateTitle = remoteSiteCollection.TemplateTitle;
            sitecollection.TenantGroupId = remoteSiteCollection.TenantGroupId;
            sitecollection.TenantId = remoteSiteCollection.TenantId;
            sitecollection.Url = remoteSiteCollection.url;
            sitecollection.Username = remoteSiteCollection.username;
            sitecollection.AADEnvironment = (Cloud.Sdk.Data.Dao.AADEnvironment)remoteSiteCollection.AADEnvironment;
            return sitecollection;
        }

        public static Cloud.Sdk.Data.Dao.Tree ConvertToAPITreeMessage(GCommon.Contract.Tree.Object.SPTreeMessage message)
        {
            if (null == message) return null;
            Cloud.Sdk.Data.Dao.Tree treeMessage = new Cloud.Sdk.Data.Dao.Tree();
            treeMessage.ChildrenCount = message.ChildrenCount;
            treeMessage.HasError = message.HasError;
            treeMessage.HasNextPage = message.HasNextPage;
            treeMessage.Message = message.Message;
            treeMessage.Node = message.Node == null ? null : ConvertToAPISPTreeNodeDto(new List<AvePoint.GCommon.Contract.Tree.Object.SPTreeNodeDto>() { (message as AvePoint.GCommon.Contract.Tree.Object.SPTreeMessage).Node })[0];
            treeMessage.NodeList = message.NodeList == null ? new List<Cloud.Sdk.Data.Dao.SPTreeNodeDto>() : ConvertToAPISPTreeNodeDto((message as AvePoint.GCommon.Contract.Tree.Object.SPTreeMessage).NodeList);
            treeMessage.PageInfo = message.PageInfo;
            treeMessage.TreeType = (Cloud.Sdk.Data.Dao.TreeType)message.TreeType;
            return treeMessage;
        }

        public static List<Cloud.Sdk.Data.Dao.SPTreeNodeDto> ConvertToAPISPTreeNodeDto(List<AvePoint.GCommon.Contract.Tree.Object.SPTreeNodeDto> nodeDtos)
        {
            List<Cloud.Sdk.Data.Dao.SPTreeNodeDto> nodes = new List<Cloud.Sdk.Data.Dao.SPTreeNodeDto>();
            foreach (var nodeDto in nodeDtos)
            {
                Cloud.Sdk.Data.Dao.SPTreeNodeDto node = new Cloud.Sdk.Data.Dao.SPTreeNodeDto();
                node.CanChildrenBeLoaded = nodeDto.CanChildrenBeLoaded;
                node.CheckNumber = nodeDto.CheckNumber;
                node.Children = (nodeDto.Children != null && nodeDto.Children.Count > 0) ? ConvertToAPISPTreeNodeDto(nodeDto.Children) : null;
                node.ChildrenCount = nodeDto.ChildrenCount;
                node.Description = nodeDto.Description;
                node.DisplayName = nodeDto.DisplayName;
                node.Expanded = nodeDto.Expanded;
                node.FarmId = nodeDto.FarmID;
                node.FarmName = nodeDto.FarmName;
                node.FullPath = nodeDto.FullPath;
                node.Hidden = nodeDto.Hidden;
                node.Url = nodeDto.Url;
                node.Id = nodeDto.ID;
                node.Level = (Cloud.Sdk.Data.Dao.NodeLevel)nodeDto.Level;
                node.LoginName = nodeDto.LoginName;
                node.Name = nodeDto.Name;
                node.NodeExtension = new Cloud.Sdk.Data.Dao.NodeExtension();
                if (nodeDto.NodeExtension != null)
                {
                    node.NodeExtension.BposInfo = new Cloud.Sdk.Data.Dao.BposInfo();
                    if (nodeDto.NodeExtension.BposInfo != null)
                    {
                        node.NodeExtension.BposInfo = ConvertToAPIBposInfo(nodeDto.NodeExtension.BposInfo);
                    }
                    node.NodeExtension.TreeType = (Cloud.Sdk.Data.Dao.TreeType)nodeDto.NodeExtension.TreeType;
                }
                node.NodeType = (Cloud.Sdk.Data.Dao.NodeType)nodeDto.Type;
                node.OffSet = nodeDto.Offset;
                node.Parent = nodeDto.Parent == null ? null : ConvertToAPISPTreeNodeDto(new List<AvePoint.GCommon.Contract.Tree.Object.SPTreeNodeDto>() { nodeDto.Parent })[0];
                node.SPObjectId = nodeDto.SPObjectId;
                node.SPType = (Cloud.Sdk.Data.Dao.SPType)nodeDto.SPType;
                node.SPVersion = nodeDto.SPVersion;
                node.Template = nodeDto.Template;
                node.Title = nodeDto.Title;
                node.TeamName = nodeDto.TeamName;
                nodes.Add(node);
            }
            return nodes;
        }

        private static Cloud.Sdk.Data.Dao.BposInfo ConvertToAPIBposInfo(BposInfo bposInfo)
        {
            Cloud.Sdk.Data.Dao.BposInfo info = new Cloud.Sdk.Data.Dao.BposInfo();
            info.AppType = (Cloud.Sdk.Data.Dao.AppType)bposInfo.AppType;
            info.ConnectionType = (Cloud.Sdk.Data.Dao.BposConnectionType)bposInfo.ConnectionType;
            info.MailboxType = (Cloud.Sdk.Data.Dao.MailboxType)bposInfo.MailboxType;
            info.Mode = (Cloud.Sdk.Data.Dao.BPOSMode)bposInfo.Mode;
            info.SiteUrl = bposInfo.SiteUrl;
            info.UserAccountInfo = new Cloud.Sdk.Data.Dao.BposUserAccountInfo();
            //DAO API need add TokenType.
            info.TokenType = Cloud.Sdk.Data.Dao.TokenType.Basic;   //change default value in july ci RECO-9521
            if (bposInfo.UserAccountInfo != null)
            {
                info.UserAccountInfo.AdminUrl = bposInfo.UserAccountInfo.AdminUrl;
                info.UserAccountInfo.Domain = bposInfo.UserAccountInfo.Domain;
                info.UserAccountInfo.Password = bposInfo.UserAccountInfo.Password;
                info.UserAccountInfo.SecondaryPassword = bposInfo.UserAccountInfo.SecondaryPassword;
                info.UserAccountInfo.SecondaryUsername = bposInfo.UserAccountInfo.SecondaryUsername;
                info.UserAccountInfo.TenantId = bposInfo.UserAccountInfo.TenantId;
                info.UserAccountInfo.Username = bposInfo.UserAccountInfo.Username;
                info.UserAccountInfo.AADEnvironment = (Cloud.Sdk.Data.Dao.AADEnvironment)bposInfo.UserAccountInfo.AADEnvironment;
            }
            return info;
        }

        private static List<AvePoint.GCommon.Contract.Tree.Object.SPTreeNodeDto> ConvertToSPTreeNodeDto(IList<Cloud.Sdk.Data.Dao.SPTreeNodeDto> nodes)
        {
            List<AvePoint.GCommon.Contract.Tree.Object.SPTreeNodeDto> dtos = new List<AvePoint.GCommon.Contract.Tree.Object.SPTreeNodeDto>();
            foreach (var item in nodes)
            {
                AvePoint.GCommon.Contract.Tree.Object.SPTreeNodeDto dto = new AvePoint.GCommon.Contract.Tree.Object.SPTreeNodeDto();
                dto.NodeExtension = new AvePoint.GCommon.Contract.Tree.Object.NodeExtensionDto();
                dto.NodeExtension.BposInfo = new AvePoint.GCommon.Contract.CentralAdmin.Object.BposInfo();
                if (item.NodeExtension != null)
                {
                    if (item.NodeExtension.BposInfo != null)
                    {
                        dto.NodeExtension.BposInfo = ConvertToBposInfo(item.NodeExtension.BposInfo);
                    }
                    dto.NodeExtension.TreeType = (AvePoint.GCommon.Contract.Tree.Object.TreeType)item.NodeExtension.TreeType;
                }
                dto.CheckNumber = item.CheckNumber;
                dto.Children = (item.Children != null && item.Children.Count > 0) ? ConvertToSPTreeNodeDto(item.Children) : null;
                dto.ChildrenCount = item.ChildrenCount;
                dto.Description = item.Description;
                dto.DisplayName = item.DisplayName;
                dto.Expanded = item.Expanded;
                dto.FarmID = item.FarmId;
                dto.FarmName = item.FarmName;
                dto.Type = (AvePoint.GCommon.Contract.Tree.Object.NodeType)item.NodeType;
                dto.FullPath = item.FullPath;
                dto.Hidden = item.Hidden;
                dto.ID = item.Id;
                dto.Url = item.Url;
                dto.Level = (AvePoint.GCommon.Contract.Tree.Object.NodeLevel)item.Level;
                dto.LoginName = item.LoginName;
                dto.Name = item.Name;
                dto.Parent = item.Parent == null ? null : ConvertToSPTreeNodeDto(new List<Cloud.Sdk.Data.Dao.SPTreeNodeDto>() { item.Parent })[0];
                dto.SPObjectId = item.SPObjectId;
                dto.SPType = (AvePoint.GCommon.Contract.Tree.Object.SPType)item.SPType;
                dto.SPVersion = item.SPVersion;
                dto.Template = item.Template;
                dto.Title = item.Title;
                dto.TeamName = item.TeamName;
                dtos.Add(dto);
            }
            dtos = dtos.OrderBy(n => n.DisplayName).ToList();

            return dtos;
        }

        private static AvePoint.GCommon.Contract.CentralAdmin.Object.BposInfo ConvertToBposInfo(Cloud.Sdk.Data.Dao.BposInfo bposInfo)
        {
            AvePoint.GCommon.Contract.CentralAdmin.Object.BposInfo info = new AvePoint.GCommon.Contract.CentralAdmin.Object.BposInfo();
            info.AppType = (AvePoint.GCommon.Contract.CentralAdmin.Object.AppType)bposInfo.AppType;
            info.ConnectionType = (AvePoint.GCommon.Contract.CentralAdmin.Object.BposConnectionType)bposInfo.ConnectionType;
            info.MailboxType = (AvePoint.GCommon.Contract.ExchangeOnline.ExchangeOnlineMailbox.Object.MailboxType)bposInfo.MailboxType;
            info.Mode = (AvePoint.GCommon.Contract.CentralAdmin.Object.BPOSMode)bposInfo.Mode;
            info.SiteUrl = bposInfo.SiteUrl;
            info.UserAccountInfo = new AvePoint.GCommon.Contract.CentralAdmin.Object.BposUserAccountInfo();
            if (bposInfo.UserAccountInfo != null)
            {
                info.UserAccountInfo.AdminUrl = bposInfo.UserAccountInfo.AdminUrl;
                info.UserAccountInfo.Domain = bposInfo.UserAccountInfo.Domain;
                info.UserAccountInfo.Password = bposInfo.UserAccountInfo.Password;
                info.UserAccountInfo.SecondaryPassword = bposInfo.UserAccountInfo.SecondaryPassword;
                info.UserAccountInfo.SecondaryUsername = bposInfo.UserAccountInfo.SecondaryUsername;
                info.UserAccountInfo.TenantId = bposInfo.UserAccountInfo.TenantId;
                info.UserAccountInfo.Username = bposInfo.UserAccountInfo.Username;
                info.UserAccountInfo.AADEnvironment = (AADEnvironment)bposInfo.UserAccountInfo.AADEnvironment;
            }
            return info;
        }

        public static GCommon.Contract.Tree.Object.SPTreeMessage ConvertToSOTreeMessage(Cloud.Sdk.Data.Dao.Tree message)
        {
            if (null == message) return null;
            AvePoint.GCommon.Contract.Tree.Object.SPTreeMessage treeMessage = new AvePoint.GCommon.Contract.Tree.Object.SPTreeMessage();
            treeMessage.ChildrenCount = message.ChildrenCount;
            treeMessage.HasError = message.HasError;
            treeMessage.HasNextPage = message.HasNextPage;
            treeMessage.Message = message.Message;
            treeMessage.Node = message.Node == null ? null : ConvertToSPTreeNodeDto(new List<Cloud.Sdk.Data.Dao.SPTreeNodeDto>() { message.Node })[0];
            treeMessage.NodeList = message.NodeList == null ? new List<GCommon.Contract.Tree.Object.SPTreeNodeDto>() : ConvertToSPTreeNodeDto(message.NodeList);
            treeMessage.PageInfo = message.PageInfo;
            treeMessage.TreeType = (AvePoint.GCommon.Contract.Tree.Object.TreeType)message.TreeType;
            return treeMessage;
        }

        #endregion

        #region Storage & Security & ExportLocation Convert

        public static AvePoint.GCommon.Contract.Storage.Entity.StoragePolicyDto ConvertToStoragePolicyDto(Cloud.Sdk.Data.Dao.StoragePolicyInfo sp)
        {
            if (null == sp) return null;
            GCommon.Contract.Storage.Entity.StoragePolicyDto spd = new GCommon.Contract.Storage.Entity.StoragePolicyDto();
            spd.Id = sp.Id;
            spd.Name = sp.Name;
            spd.Type = (int)sp.StorageType;
            return spd;
        }

        public static AvePoint.GCommon.Contract.Server.ControlPanel.Cryptography.DataEncryptionProfile ConvertToDateEncryption(Cloud.Sdk.Data.Dao.SecurityProfile sp)
        {
            if (null == sp) return null;
            GCommon.Contract.Server.ControlPanel.Cryptography.DataEncryptionProfile dep = new GCommon.Contract.Server.ControlPanel.Cryptography.DataEncryptionProfile();
            dep.Guid = sp.Id;
            dep.Name = sp.Name;
            dep.IsDefault = sp.IsDefault;
            return dep;
        }

        public static AvePoint.GCommon.Contract.Server.Common.ExportReport.Object.ExportReportDto ConvertToExportLocation(Cloud.Sdk.Data.Dao.ExportLocationInfo el)
        {
            if (null == el) return null;
            GCommon.Contract.Server.Common.ExportReport.Object.ExportReportDto erd = new GCommon.Contract.Server.Common.ExportReport.Object.ExportReportDto();
            erd.Id = el.Id;
            erd.Name = el.Name;
            erd.ReportType = (AvePoint.GCommon.Contract.Server.Common.ExportReport.Object.ExportReportType)el.ReportType;
            return erd;
        }

        #endregion

        #region JobMonitor
        public static SOJob ConvertJobDtoToSOJob(Cloud.Sdk.Data.Dao.JobDto job)
        {
            SOJob soJob = new SOJob();
            soJob.Id = job.Id;
            soJob.PlanId = job.PlanId;
            soJob.Progress = job.Progress;
            soJob.State = (int)job.Status;
            return soJob;
        }

        public static SOJob ConvertToSOJob(Cloud.Sdk.Data.Dao.Job job)
        {
            SOJob soJob = new SOJob();
            soJob.Id = job.Id;
            soJob.Type = job.Type;
            soJob.Category = job.Category;
            soJob.Dependency = job.Dependency;
            soJob.DestAgentName = job.DestAgentName;
            soJob.Detail = job.Detail;
            soJob.FinishTime = job.FinishTime;
            soJob.PlanId = job.PlanId;
            soJob.PlanType = job.PlanType;
            soJob.Progress = job.Progress;
            soJob.SrcAgentName = job.SrcAgentName;
            soJob.StartTime = job.StartTime;
            soJob.State = job.State;
            soJob.UserName = job.UserName;
            soJob.ProfileId = job.RevIMKey;
            soJob.Scope = job.Scope;
            return soJob;
        }

        public static GCommon.Contract.Server.Common.Monitor.Object.Detail.JobSummaryInfos ConvertToJobSummaryInfos(Cloud.Sdk.Data.Dao.JobDetail result, SOJob job)
        {
            var infos = new GCommon.Contract.Server.Common.Monitor.Object.Detail.JobSummaryInfos();
            if (result != null)
            {
                infos.SummaryItem = new List<GCommon.Contract.Server.Common.Monitor.Object.Detail.JobSummaryItem>();
                foreach (var item in result.SummaryItems)
                {
                    var rmItem = new GCommon.Contract.Server.Common.Monitor.Object.Detail.JobSummaryItem();
                    rmItem.SummaryRow = new List<GCommon.Contract.Server.Common.Monitor.Object.Detail.JobSummaryRow>();
                    rmItem.Title = item.Title;
                    infos.SummaryItem.Add(rmItem);
                    foreach (var row in item.SummaryRow)
                    {
                        //Summary 中没有Job Information 相关信息
                        var rowKey = row.Key;
                        var rowValue = row.Value;
                        if (row.Key == "Start Time" || row.Key == "開始時刻")
                        {
                            rowValue = job.StartTime.ToString();
                        }
                        if (row.Key == "Finish Time" || row.Key == "終了時刻")
                        {
                            rowValue = job.FinishTime.ToString();
                        }
                        if (row.Key == "Job Run by" || row.Key == "Job Run By" || row.Key == @"ジョブ実行者" || row.Key == "Job Operated By")
                        {
                            rowKey = "Job Run by";
                            rowValue = job.UserName;
                        }
                        if (row.Key == "Scope" || row.Key == "範囲")
                        {
                            rowValue = job.Scope;
                        }
                        rmItem.SummaryRow.Add(new GCommon.Contract.Server.Common.Monitor.Object.Detail.JobSummaryRow() { Key = rowKey, Value = rowValue });
                    }
                }
            }
            return infos;
        }

        public static GCommon.Contract.Server.Common.Monitor.Object.Detail.JobDetailInfos ConvertToJobDetailsInfos(Cloud.Sdk.Data.Dao.JobDetail result)
        {
            var infos = new GCommon.Contract.Server.Common.Monitor.Object.Detail.JobDetailInfos();
            infos.TotalLength = result.TotalLength;
            infos.Values = new List<GCommon.Contract.Server.Common.Monitor.Object.Detail.JobDetailDto>();
            if (result == null || result.Values == null)
            {
                return new GCommon.Contract.Server.Common.Monitor.Object.Detail.JobDetailInfos() { Values = new List<GCommon.Contract.Server.Common.Monitor.Object.Detail.JobDetailDto>() };
            }
            else
            {
                foreach (var detail in result.Values)
                {
                    var soDetail = detail as Cloud.Sdk.Data.Dao.SOJobDetailDto;
                    infos.Values.Add(new GCommon.Contract.Server.Common.Monitor.Object.Detail.SOJobDetailDto()
                    {
                        Action = soDetail.Action,
                        Comment = soDetail.Comment,
                        DataOperation = soDetail.DataOperation,
                        EntityType = soDetail.EntityType,
                        Date = soDetail.Date,
                        DestAgentHost = soDetail.DestAgentHost,
                        DestURL = soDetail.DestURL,
                        Farm = soDetail.Farm,
                        FileName = soDetail.FileName,
                        //ID = soDetail.ID,
                        MediaHost = soDetail.MediaHost,
                        MoveDataTo = soDetail.MoveDataTo,
                        Option = soDetail.Option,
                        RuleName = soDetail.RuleName,
                        Size = soDetail.Size,
                        SrcAgentHost = soDetail.SrcAgentHost,
                        SrcURL = soDetail.SrcURL,
                        Status = soDetail.Status,
                        TimeZoneId = soDetail.TimeZoneId,
                        Type = soDetail.Type,
                        Title = soDetail.Title,

                    });
                }
            }
            return infos;
        }

        #endregion

        #region
        public class SPTreeNodeComparer : IEqualityComparer<RMSPTreeNode>
        {
            public bool Equals(RMSPTreeNode x, RMSPTreeNode y)
            {
                if (x == null && y == null)
                {
                    return true;
                }
                if ((x == null && y != null) || (x != null && y == null))
                {
                    return false;
                }
                return x.Level == y.Level && x.Name == y.Name;
            }

            public int GetHashCode(RMSPTreeNode obj)
            {
                if (obj == null)
                {
                    return 0;
                }
                return obj.Name.GetHashCode();
            }
        }

        public class EXOTreeNodeComparer : IEqualityComparer<RMEXOTreeNode>
        {
            public bool Equals(RMEXOTreeNode x, RMEXOTreeNode y)
            {
                if (x == null && y == null)
                {
                    return true;
                }
                if ((x == null && y != null) || (x != null && y == null))
                {
                    return false;
                }
                return x.Level == y.Level && x.Name == y.Name;
            }

            public int GetHashCode(RMEXOTreeNode obj)
            {
                if (obj == null)
                {
                    return 0;
                }
                return obj.Name.GetHashCode();
            }
        }
        #endregion

        #region Exchange Online
        //TODO fpwang
        public static Cloud.Sdk.Data.Dao.ExchangeOnlineTree ConvertToAPIExchangeTreeMessage(ExchangeOnlineTreeMessage message)
        {
            Cloud.Sdk.Data.Dao.ExchangeOnlineTree treeMessage = new Cloud.Sdk.Data.Dao.ExchangeOnlineTree();
            treeMessage.ChildrenCount = message.ChildrenCount;
            treeMessage.HasError = message.HasError;
            treeMessage.HasNextPage = message.HasNextPage;
            treeMessage.Message = message.Message;
            treeMessage.Node = ConvertToApiExOLTreeNodeDto((message as ExchangeOnlineTreeMessage).Node);
            treeMessage.NodeList = ConvertToApiExOLTreeNodeDtos((message as ExchangeOnlineTreeMessage).NodeList);
            treeMessage.PageInfo = message.PageInfo;
            treeMessage.TreeType = (Cloud.Sdk.Data.Dao.TreeType)message.TreeType;
            return treeMessage;
        }

        internal static List<Cloud.Sdk.Data.Dao.ExchangeOnlineTreeNodeDto> ConvertToApiExOLTreeNodeDtos(List<ExchangeOnlineTreeNodeDto> nodeDtos)
        {
            List<Cloud.Sdk.Data.Dao.ExchangeOnlineTreeNodeDto> result = new List<Cloud.Sdk.Data.Dao.ExchangeOnlineTreeNodeDto>();
            if (nodeDtos == null)
            {
                return result;
            }
            foreach (ExchangeOnlineTreeNodeDto dto in nodeDtos)
            {
                result.Add(ConvertToApiExOLTreeNodeDto(dto));
            }
            return result;
        }

        internal static Cloud.Sdk.Data.Dao.ExchangeOnlineTreeNodeDto ConvertToApiExOLTreeNodeDto(ExchangeOnlineTreeNodeDto nodeDto)
        {
            if (nodeDto == null)
            {
                return null;
            }
            Cloud.Sdk.Data.Dao.ExchangeOnlineTreeNodeDto node = new Cloud.Sdk.Data.Dao.ExchangeOnlineTreeNodeDto();
            node.CanChildrenBeLoaded = nodeDto.CanChildrenBeLoaded;
            node.CheckNumber = nodeDto.CheckNumber;
            node.Children = ConvertToApiExOLTreeNodeDtos(nodeDto.Children);
            node.ChildrenCount = nodeDto.ChildrenCount;
            node.ChildrenLoaded = nodeDto.ChildrenLoaded;
            node.DisplayName = nodeDto.DisplayName;
            node.Expanded = nodeDto.Expanded;
            node.FarmId = nodeDto.FarmID;
            node.FarmName = nodeDto.FarmName;
            node.ParentId = nodeDto.ParentId;
            node.FullPath = nodeDto.FullPath;
            node.Id = nodeDto.ID;
            node.Level = (Cloud.Sdk.Data.Dao.ExchangeNodeLevel)nodeDto.Level;
            node.Name = nodeDto.Name;
            node.NodeExtension = new Cloud.Sdk.Data.Dao.NodeExtension();
            if (nodeDto.NodeExtension != null)
            {
                node.NodeExtension.TreeType = (Cloud.Sdk.Data.Dao.TreeType)nodeDto.NodeExtension.TreeType;
            }
            node.NodeType = (Cloud.Sdk.Data.Dao.ExchangeNodeType)nodeDto.Type;
            node.OffSet = nodeDto.Offset;
            node.Title = nodeDto.Title;
            node.InternalFolderPath = nodeDto.InternalFolderPath;
            node.EmailAddress = nodeDto.EmailAddress;
            node.SubFolderCount = nodeDto.SubFolderCount;
            node.Sender = nodeDto.Sender;
            node.DisplayTo = nodeDto.DisplayTo;
            node.SendDate = nodeDto.SendDate;
            node.HasAttachment = nodeDto.HasAttachment;
            node.Category = nodeDto.Category;
            node.GroupName = nodeDto.GroupName;
            node.SiteCollectionUrl = nodeDto.SiteCollectionUrl;
            node.MailboxType = (Cloud.Sdk.Data.Dao.MailboxType)nodeDto.MailboxType;

            if (node.Children != null && node.Children.Count > 0)
            {
                node.Children.ToList().ForEach(c => c.Parent = node);
            }
            return node;
        }

        internal static ExchangeOnlineTreeMessage ConvertToDaoExchangeTreeMessage(Cloud.Sdk.Data.Dao.ExchangeOnlineTree message)
        {
            var treeMessage = new ExchangeOnlineTreeMessage();
            treeMessage.ChildrenCount = message.ChildrenCount;
            treeMessage.HasError = message.HasError;
            treeMessage.HasNextPage = message.HasNextPage;
            treeMessage.Message = message.Message;
            if (message.Node != null)
            {
                treeMessage.Node = ConvertToDaoExchangeNodeDto(new List<Cloud.Sdk.Data.Dao.ExchangeOnlineTreeNodeDto>() { message.Node })[0];
            }
            treeMessage.NodeList = ConvertToDaoExchangeNodeDto(message.NodeList);
            treeMessage.PageInfo = message.PageInfo;
            treeMessage.TreeType = (TreeType)message.TreeType;
            return treeMessage;
        }
        internal static List<ExchangeOnlineTreeNodeDto> ConvertToDaoExchangeNodeDto(IList<Cloud.Sdk.Data.Dao.ExchangeOnlineTreeNodeDto> nodes)
        {
            if (nodes == null)
            {
                return new List<ExchangeOnlineTreeNodeDto>();
            }
            var dtos = new List<ExchangeOnlineTreeNodeDto>();
            foreach (var item in nodes)
            {
                var dto = new ExchangeOnlineTreeNodeDto();
                dto.NodeExtension = new NodeExtensionDto();
                if (item.NodeExtension != null)
                {
                    dto.NodeExtension.TreeType = (TreeType)item.NodeExtension.TreeType;
                }
                dto.CheckNumber = item.CheckNumber;
                dto.Children = (item.Children != null && item.Children.Count > 0) ? ConvertToDaoExchangeNodeDto(item.Children) : new List<ExchangeOnlineTreeNodeDto>();
                dto.ChildrenCount = item.ChildrenCount;
                dto.DisplayName = item.DisplayName;
                dto.Expanded = item.Expanded;
                dto.FarmID = item.FarmId;
                dto.FarmName = item.FarmName;
                dto.FullPath = item.FullPath;
                dto.ParentId = item.ParentId;
                dto.ID = item.Id;
                dto.Level = (NodeLevel)item.Level;
                dto.Name = item.Name;
                dto.Parent = item.Parent == null || (item.Parent.Children != null && item.Parent.Children.Count > 0) ? null : ConvertToDaoExchangeNodeDto(new List<Cloud.Sdk.Data.Dao.ExchangeOnlineTreeNodeDto>() { item.Parent })[0];
                dto.Title = item.Title;
                dto.CanChildrenBeLoaded = item.CanChildrenBeLoaded;
                dto.ChildrenLoaded = item.ChildrenLoaded;
                dto.Type = (NodeType)item.NodeType;
                dto.InternalFolderPath = item.InternalFolderPath;
                dto.EmailAddress = item.EmailAddress;
                dto.SubFolderCount = item.SubFolderCount;
                dto.Sender = item.Sender;
                dto.DisplayTo = item.DisplayTo;
                dto.SendDate = item.SendDate;
                dto.HasAttachment = item.HasAttachment;
                dto.Category = item.Category;
                dto.GroupName = item.GroupName;
                dto.SiteCollectionUrl = item.SiteCollectionUrl;
                dto.MailboxType = (MailboxType)item.MailboxType;

                if (dto.Children != null && dto.Children.Count > 0)
                {
                    dto.Children.ToList().ForEach(c => c.Parent = dto);
                }
                dtos.Add(dto);
            }
            return dtos;
        }

        internal static BposInfo ConvertToDaoBposInfo(Cloud.Sdk.Data.Dao.BposInfo bposInfo)
        {
            var info = new BposInfo();
            info.AppType = (GCommon.Contract.CentralAdmin.Object.AppType)bposInfo.AppType;
            info.ConnectionType = (BposConnectionType)bposInfo.ConnectionType;
            info.MailboxType = (MailboxType)bposInfo.MailboxType;
            info.Mode = (BPOSMode)bposInfo.Mode;
            info.SiteUrl = bposInfo.SiteUrl;
            info.TenantGroupId = bposInfo.TenantGroupId;
            var userAccountInfo = new BposUserAccountInfo();
            if (bposInfo.UserAccountInfo != null)
            {
                var bposUserAccountInfo = bposInfo.UserAccountInfo;
                userAccountInfo.AdminUrl = bposUserAccountInfo.AdminUrl;
                userAccountInfo.Domain = bposUserAccountInfo.Domain;
                userAccountInfo.Password = bposUserAccountInfo.Password;
                userAccountInfo.SecondaryPassword = bposUserAccountInfo.SecondaryPassword;
                userAccountInfo.SecondaryUsername = bposUserAccountInfo.SecondaryUsername;
                userAccountInfo.TenantId = bposUserAccountInfo.TenantId;
                userAccountInfo.Username = bposUserAccountInfo.Username;
                //userAccountInfo.AppCertContent = bposUserAccountInfo.AppCertContent;
                userAccountInfo.AppCertSecret = bposUserAccountInfo.AppCertSecret;
                userAccountInfo.AppClientId = bposUserAccountInfo.AppClientId;
                userAccountInfo.AppCertSecretContent = bposUserAccountInfo.AppCertSecretContent;
                userAccountInfo.AADEnvironment = (AADEnvironment)bposUserAccountInfo.AADEnvironment;
            }
            info.UserAccountInfo = userAccountInfo;
            return info;
        }

        public static TData DeserializeFromXmlString<TData>(String data)
        {
            using (StringReader reader = new StringReader(data))
            {
                XmlSerializer serializer = new XmlSerializer(typeof(TData));
                return (TData)serializer.Deserialize(reader);
            }
        }
        #endregion
    }
}
