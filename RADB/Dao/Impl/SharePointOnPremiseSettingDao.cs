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
using AvePoint.GCommon.Contract.Tree.Object;
using AvePoint.RA.Common.Global.Utils;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.Schedule;
using AvePoint.RA.DB.Core;
using AvePoint.RA.DB.Model;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace AvePoint.RA.DB.Dao.Impl
{
    public class SharePointOnPremiseSettingDao : BaseDao<RMSharePointOnPremiseSetting>, ISharePointOnPremiseSettingDao
    {
        private RALogger logger = RALogger.GetInstance(typeof(SharePointOnPremiseSettingDao));
        public IScheduleService ScheduleService { get; set; }
        public IRecordOwnerDao RecordOwnerDao { get; set; }
        public IAccountDao AccountDao { get; set; }
        public async Task AddOrUpdateCustomSettingAsync(RMSPTreeNode node, Guid siteId)
        {
            EnsureTermName(node);
            using (var context = GetNewContext())
            {
                var spSetting = context.RMSharePointOnPremiseSettings.AsQueryable().Where(s => s.ScopeId.Equals(new Guid(node.SPObjectId)) && s.SiteId.Equals(siteId) && !s.IsRemoved).FirstOrDefault();
                if (spSetting == null)
                {
                    spSetting = context.RMSharePointOnPremiseSettings.AsQueryable().Where(s => s.ScopeId.Equals(new Guid(node.SPObjectId)) && s.SiteId.Equals(Guid.Empty) && !s.IsRemoved).FirstOrDefault();
                }
                if (spSetting != null)
                {
                    spSetting.ColumnName = node.ColumnName;
                    spSetting.ColumnRequired = node.ColumnRequired;
                    spSetting.DefaultTermId = node.DefaultTermId;
                    spSetting.DefaultTermName = node.DefaultTermName;
                    spSetting.FullPath = node.FullPath;
                    spSetting.ScopeId = new Guid(node.SPObjectId);
                    spSetting.TermId = node.TermId;
                    spSetting.TermName = node.TermName;
                    spSetting.TermSetId = node.TermSetId;
                    spSetting.TermSetName = node.TermSetName;
                    spSetting.TermStoreId = node.TermStoreId;
                    spSetting.Description = node.Description;
                    spSetting.DescriptionOfContainer = node.DescriptionOfContainer;
                    spSetting.TermIdOfContainer = node.TermIdOfContainer;
                    spSetting.TermNameOfContainer = node.TermNameOfContainer;
                    spSetting.IsEnableContainerLevelTerm = node.isEnableClassification;
                    spSetting.EnableRecordManagement = node.EnableRecordManagement;
                    spSetting.IsFailedConfigMetaDataColumn = node.isFailedConfigMetaDataColumn;
                    spSetting.IsFailedConfigClassification = node.isFailedConfigClassification;
                    spSetting.SiteId = siteId;
                    spSetting.WebId = node.WebId;
                    spSetting.ListId = node.ListId;
                    spSetting.FolderId = node.FolderId;
                    spSetting.SiteGroupId = node.SiteGroupId;
                    spSetting.EMailToRecordOwner = node.EMailToRecordOwner;
                    spSetting.SettingTime = 0;
                    spSetting.NodeInfo = SerializerHelper.SerializeByDataContractSerializer(node);
                    spSetting.NeedCheckDefaultValue = node.NeedCheckDefaultValue;
                    spSetting.IsDisplyaTermPath = node.IsDisplyaTermPath;
                    spSetting.ApplyExistType = node.ApplyExistType;
                    spSetting.EnableRelatedRecords = node.EnableRelatedRecords;
                    spSetting.IsShowUniqueId = node.IsShowUniqueId;
                    spSetting.DeployTermMethod = (int)node.DeployTermMethod;
                    spSetting.AutoClassificationRules = node.AutoClassificationRules == null ?
                        null : SerializerHelper.SerializeByDataContractSerializer(node.AutoClassificationRules);
                    spSetting.RunAutoFullJob = node.RunAutoFullJob;
                    spSetting.AutoJobOption = (int)node.AutoJobOption;
                    spSetting.IncludeDeclaredRecords = node.IncludeDeclaredRecords;
                    spSetting.IsUsingExistColumnName = node.IsUsingExistColumnName;
                    spSetting.ExistColumnName = node.ExistColumnName;
                    spSetting.SetDocLevelTermForExistColumn = node.SetDocLevelTermForExistColumn;
                    spSetting.IsSyncData = node.IsSyncData;
                    spSetting.ApprovalType = (ApprovalType)node.ApprovalType;
                    spSetting.WorkflowReferenceId = node.WorkflowReferenceId;
                    ApplyCurrentValues(context, spSetting);
                    await RecordOwnerDao.UpdateRecordOwnersAsync(spSetting.Id, node.RecordOwner, RecordOwnerSettingType.SharePointOnPremise);
                }
                else
                {
                    RMSharePointOnPremiseSetting settings = new RMSharePointOnPremiseSetting()
                    {
                        ColumnName = node.ColumnName,
                        ColumnRequired = node.ColumnRequired,
                        DefaultTermId = node.DefaultTermId,
                        DefaultTermName = node.DefaultTermName,
                        FullPath = node.FullPath,
                        ScopeId = new Guid(node.SPObjectId),
                        TermId = node.TermId,
                        TermName = node.TermName,
                        TermSetId = node.TermSetId,
                        TermSetName = node.TermSetName,
                        Description = node.Description,
                        TermStoreId = node.TermStoreId,
                        DescriptionOfContainer = node.DescriptionOfContainer,
                        TermIdOfContainer = node.TermIdOfContainer,
                        TermNameOfContainer = node.TermNameOfContainer,
                        IsEnableContainerLevelTerm = node.isEnableClassification,
                        EnableRecordManagement = node.EnableRecordManagement,
                        IsFailedConfigMetaDataColumn = node.isFailedConfigMetaDataColumn,
                        IsFailedConfigClassification = node.isFailedConfigClassification,
                        SiteId = siteId,
                        WebId = node.WebId,
                        FolderId = node.FolderId,
                        ListId = node.ListId,
                        SiteGroupId = node.SiteGroupId,
                        EMailToRecordOwner = node.EMailToRecordOwner,
                        SettingTime = 0,
                        NeedCheckDefaultValue = node.NeedCheckDefaultValue,
                        IsDisplyaTermPath = node.IsDisplyaTermPath,
                        ApplyExistType = node.ApplyExistType,
                        EnableRelatedRecords = node.EnableRelatedRecords,
                        IsShowUniqueId = node.IsShowUniqueId,
                        NodeInfo = SerializerHelper.SerializeByDataContractSerializer(node),
                        DeployTermMethod = (int)node.DeployTermMethod,
                        AutoClassificationRules = node.AutoClassificationRules == null ?
                            null : SerializerHelper.SerializeByDataContractSerializer(node.AutoClassificationRules),
                        RunAutoFullJob = node.RunAutoFullJob,
                        IncludeDeclaredRecords = node.IncludeDeclaredRecords,
                        AutoJobOption = (int)node.AutoJobOption,
                        IsUsingExistColumnName = node.IsUsingExistColumnName,
                        ExistColumnName = node.ExistColumnName,
                        SetDocLevelTermForExistColumn = node.SetDocLevelTermForExistColumn,
                        IsSyncData = node.IsSyncData,
                        ApprovalType = (ApprovalType)node.ApprovalType,
                        WorkflowReferenceId = node.WorkflowReferenceId
                    };
                    context.RMSharePointOnPremiseSettings.Add(settings);
                    context.SaveChanges();
                    spSetting = context.RMSharePointOnPremiseSettings.AsQueryable().Where(s => s.ScopeId == settings.ScopeId && !s.IsRemoved).First();
                    await RecordOwnerDao.AddRecordOwnersAsync(spSetting.Id, node.RecordOwner, RecordOwnerSettingType.SharePointOnPremise);
                }
            }
        }

        public async Task AddOrUpdateGlobalSettingAsync(RMSPTreeNode node)
        {
            EnsureTermName(node);
            using (var context = GetNewContext())
            {
                RMSharePointOnPremiseSetting spSetting = context.RMSharePointOnPremiseSettings.AsQueryable().Where(s => s.ScopeId.Equals(new Guid(node.SPObjectId))).FirstOrDefault();
                if (spSetting != null)
                {
                    spSetting.ColumnName = node.ColumnName;
                    spSetting.ColumnRequired = node.ColumnRequired;
                    spSetting.DefaultTermId = node.DefaultTermId;
                    spSetting.DefaultTermName = node.DefaultTermName;
                    spSetting.Description = node.Description;
                    spSetting.FullPath = node.FullPath;
                    spSetting.ScopeId = new Guid(node.SPObjectId);
                    spSetting.TermId = node.TermId;
                    spSetting.TermName = node.TermName;
                    spSetting.TermSetId = node.TermSetId;
                    spSetting.TermSetName = node.TermSetName;
                    spSetting.DescriptionOfContainer = node.DescriptionOfContainer;
                    spSetting.TermIdOfContainer = node.TermIdOfContainer;
                    spSetting.TermNameOfContainer = node.TermNameOfContainer;
                    spSetting.IsEnableContainerLevelTerm = node.isEnableClassification;
                    spSetting.EnableRecordManagement = node.EnableRecordManagement;
                    spSetting.IsFailedConfigMetaDataColumn = false;
                    spSetting.IsFailedConfigClassification = false;
                    spSetting.IsUsingExistColumnName = node.IsUsingExistColumnName;
                    spSetting.ExistColumnName = node.ExistColumnName;
                    spSetting.SiteGroupId = new Guid(node.Id);
                    spSetting.SettingTime = 0;
                    spSetting.NeedCheckDefaultValue = node.NeedCheckDefaultValue;
                    spSetting.IsDisplyaTermPath = node.IsDisplyaTermPath;
                    spSetting.IsShowUniqueId = node.IsShowUniqueId;
                    spSetting.EMailToRecordOwner = node.EMailToRecordOwner;
                    spSetting.NodeInfo = SerializerHelper.SerializeByDataContractSerializer(node);
                    spSetting.ApplyExistType = node.ApplyExistType;
                    spSetting.EnableRelatedRecords = node.EnableRelatedRecords;
                    spSetting.DeployTermMethod = (int)node.DeployTermMethod;
                    spSetting.AutoClassificationRules = node.AutoClassificationRules == null ?
                        null : SerializerHelper.SerializeByDataContractSerializer(node.AutoClassificationRules);
                    spSetting.RunAutoFullJob = node.RunAutoFullJob;
                    spSetting.AutoJobOption = (int)node.AutoJobOption;
                    spSetting.IncludeDeclaredRecords = node.IncludeDeclaredRecords;
                    spSetting.IsSyncData = node.IsSyncData;
                    spSetting.ApprovalType = (ApprovalType)node.ApprovalType;
                    spSetting.WorkflowReferenceId = node.WorkflowReferenceId;
                    ApplyCurrentValues(context, spSetting);
                    await RecordOwnerDao.UpdateRecordOwnersAsync(spSetting.Id, node.RecordOwner, RecordOwnerSettingType.SharePointOnPremise);
                }
                else
                {
                    RMSharePointOnPremiseSetting settings = new RMSharePointOnPremiseSetting()
                    {
                        ColumnName = node.ColumnName,
                        ColumnRequired = node.ColumnRequired,
                        DefaultTermId = node.DefaultTermId,
                        Description = node.Description,
                        DefaultTermName = node.DefaultTermName,
                        FullPath = node.FullPath,
                        ScopeId = new Guid(node.SPObjectId),
                        TermId = node.TermId,
                        TermName = node.TermName,
                        TermSetId = node.TermSetId,
                        TermSetName = node.TermSetName,
                        DescriptionOfContainer = node.DescriptionOfContainer,
                        TermIdOfContainer = node.TermIdOfContainer,
                        TermNameOfContainer = node.TermNameOfContainer,
                        IsEnableContainerLevelTerm = node.isEnableClassification,
                        EnableRecordManagement = node.EnableRecordManagement,
                        IsFailedConfigMetaDataColumn = false,
                        IsFailedConfigClassification = false,
                        IsUsingExistColumnName = node.IsUsingExistColumnName,
                        ExistColumnName = node.ExistColumnName,
                        SiteGroupId = new Guid(node.Id),
                        SettingTime = 0,
                        NeedCheckDefaultValue = node.NeedCheckDefaultValue,
                        ApplyExistType = node.ApplyExistType,
                        EnableRelatedRecords = node.EnableRelatedRecords,
                        IsDisplyaTermPath = node.IsDisplyaTermPath,
                        IsShowUniqueId = node.IsShowUniqueId,
                        EMailToRecordOwner = node.EMailToRecordOwner,
                        NodeInfo = SerializerHelper.SerializeByDataContractSerializer(node),
                        DeployTermMethod = (int)node.DeployTermMethod,
                        AutoClassificationRules = node.AutoClassificationRules == null ?
                            null : SerializerHelper.SerializeByDataContractSerializer(node.AutoClassificationRules),
                        RunAutoFullJob = node.RunAutoFullJob,
                        AutoJobOption = (int)node.AutoJobOption,
                        IncludeDeclaredRecords = node.IncludeDeclaredRecords,
                        IsSyncData = node.IsSyncData,
                        ApprovalType = (ApprovalType)node.ApprovalType,
                        WorkflowReferenceId = node.WorkflowReferenceId
                    };
                    context.RMSharePointOnPremiseSettings.Add(settings);
                    context.SaveChanges();
                    spSetting = context.RMSharePointOnPremiseSettings.AsQueryable().Where(s => s.ScopeId == settings.ScopeId).First();
                    await RecordOwnerDao.AddRecordOwnersAsync(spSetting.Id, node.RecordOwner, RecordOwnerSettingType.SharePointOnPremise);
                }
            }
        }

        public async Task AddOrUpdateGlobalSettingUsingExistColumnAsync(RMSPTreeNode node)
        {
            EnsureTermName(node);
            using (var context = GetNewContext())
            {
                RMSharePointOnPremiseSetting spSetting = context.RMSharePointOnPremiseSettings.AsQueryable().Where(s => s.ScopeId.Equals(new Guid(node.SPObjectId))).FirstOrDefault();
                if (spSetting != null)
                {
                    spSetting.IsUsingExistColumnName = node.IsUsingExistColumnName;
                    spSetting.ExistColumnName = node.ExistColumnName;
                    spSetting.SetDocLevelTermForExistColumn = node.SetDocLevelTermForExistColumn;
                    spSetting.SettingTime = 0;
                    spSetting.TermIdOfContainer = node.TermIdOfContainer;
                    spSetting.TermNameOfContainer = node.TermNameOfContainer;
                    spSetting.DescriptionOfContainer = node.DescriptionOfContainer;
                    spSetting.IsFailedConfigClassification = false;
                    spSetting.EnableRecordManagement = node.EnableRecordManagement;
                    spSetting.IsFailedConfigMetaDataColumn = false;
                    spSetting.IsEnableContainerLevelTerm = node.isEnableClassification;
                    spSetting.EMailToRecordOwner = node.EMailToRecordOwner;
                    spSetting.NodeInfo = SerializerHelper.SerializeByDataContractSerializer(node);
                    spSetting.EnableRelatedRecords = node.EnableRelatedRecords;
                    spSetting.IsShowUniqueId = node.IsShowUniqueId;
                    spSetting.SiteGroupId = new Guid(node.Id);
                    spSetting.IsSyncData = node.IsSyncData;
                    ApplyCurrentValues(context, spSetting);
                    await RecordOwnerDao.UpdateRecordOwnersAsync(spSetting.Id, node.RecordOwner, RecordOwnerSettingType.SharePointOnPremise);
                }
                else
                {
                    RMSharePointOnPremiseSetting settings = new RMSharePointOnPremiseSetting()
                    {
                        ExistColumnName = node.ExistColumnName,
                        IsUsingExistColumnName = node.IsUsingExistColumnName,
                        SetDocLevelTermForExistColumn = node.SetDocLevelTermForExistColumn,
                        FullPath = node.FullPath,
                        ScopeId = new Guid(node.SPObjectId),
                        SiteGroupId = new Guid(node.Id),
                        SiteId = Guid.Empty,
                        WebId = Guid.Empty,
                        ListId = Guid.Empty,
                        TermStoreId = Guid.Empty,
                        TermSetId = Guid.Empty,
                        TermId = Guid.Empty,
                        DefaultTermId = Guid.Empty,
                        TermIdOfContainer = node.TermIdOfContainer,
                        TermNameOfContainer = node.TermNameOfContainer,
                        DescriptionOfContainer = node.DescriptionOfContainer,
                        IsEnableContainerLevelTerm = node.isEnableClassification,
                        EnableRecordManagement = node.EnableRecordManagement,
                        EMailToRecordOwner = node.EMailToRecordOwner,
                        EnableRelatedRecords = node.EnableRelatedRecords,
                        IsShowUniqueId = node.IsShowUniqueId,
                        SettingTime = 0,
                        NodeInfo = SerializerHelper.SerializeByDataContractSerializer(node),
                        IsSyncData = node.IsSyncData
                    };
                    context.RMSharePointOnPremiseSettings.Add(settings);
                    context.SaveChanges();
                    spSetting = context.RMSharePointOnPremiseSettings.AsQueryable().Where(s => s.ScopeId == settings.ScopeId).First();
                    await RecordOwnerDao.AddRecordOwnersAsync(spSetting.Id, node.RecordOwner, RecordOwnerSettingType.SharePointOnPremise);
                }
                SetCustomSettingUsingExistColumnByGroup(node);
            }
        }

        public void SetCustomSettingUsingExistColumnByGroup(RMSPTreeNode gNode)
        {
            using (var context = GetNewContext())
            {
                var entities = context.RMSharePointOnPremiseSettings.AsQueryable().Where(s => s.SiteGroupId == new Guid(gNode.SPObjectId) && s.SiteId != Guid.Empty).ToList();
                foreach (var entity in entities)
                {
                    entity.IsUsingExistColumnName = true;
                    entity.SetDocLevelTermForExistColumn = gNode.SetDocLevelTermForExistColumn;
                    entity.SettingTime = 0;
                    entity.EnableRelatedRecords = gNode.EnableRelatedRecords;
                    entity.ExistColumnName = gNode.ExistColumnName;
                    entity.IsShowUniqueId = gNode.IsShowUniqueId;
                    entity.EnableRelatedRecords = gNode.EnableRelatedRecords;
                    entity.IsEnableContainerLevelTerm = gNode.isEnableClassification;
                    entity.TermIdOfContainer = gNode.TermIdOfContainer;
                    entity.TermNameOfContainer = gNode.TermNameOfContainer;
                    entity.DescriptionOfContainer = gNode.DescriptionOfContainer;
                    entity.IsFailedConfigClassification = false;
                    entity.IsFailedConfigMetaDataColumn = false;
                    entity.EMailToRecordOwner = gNode.EMailToRecordOwner;
                    entity.IsSyncData = gNode.IsSyncData;
                }
                BatchUpdate(context, entities);
            }
        }

        public RMSharePointOnPremiseSetting GetGroupLevelSetting(string groupName, Guid scopeId)
        {
            using (var context = RMDBContextManager.GetNewDBContext())
            {
                return context.RMSharePointOnPremiseSettings.FirstOrDefault(a => a.FullPath.Equals(groupName, StringComparison.OrdinalIgnoreCase) && a.ScopeId.Equals(scopeId) && !a.IsRemoved);
            }
        }

        public RMSharePointOnPremiseSetting GetSiteLevelSetting(string fullPath, Guid scopeId)
        {
            using (var context = RMDBContextManager.GetNewDBContext())
            {
                return context.RMSharePointOnPremiseSettings.FirstOrDefault(a => a.FullPath.Equals(fullPath, StringComparison.OrdinalIgnoreCase) || a.ScopeId.Equals(scopeId) && !a.IsRemoved);
            }
        }

        public bool IsUsingExistingColumnByGroupIds(List<Guid> ids)
        {
            bool result = false;
            using (var context = GetNewContext())
            {
                RMSharePointOnPremiseSetting spSetting = context.RMSharePointOnPremiseSettings.AsQueryable().Where(s => ids.Contains(s.ScopeId) && s.IsUsingExistColumnName && !s.IsRemoved).FirstOrDefault();
                if (spSetting != null)
                {
                    result = true;
                }
                return result;
            }
        }

        public RMSharePointOnPremiseSetting LoadSharePointSetting(Guid scopeId, Guid siteId)
        {
            using (var context = RMDBContextManager.GetNewDBContext())
            {
                return context.RMSharePointOnPremiseSettings.AsQueryable().Where(s => s.ScopeId == scopeId && s.SiteId == siteId && !s.IsRemoved).FirstOrDefault();
            }
        }

        public List<RMSharePointOnPremiseSetting> LoadSharePointSettings(Guid groupId)
        {
            using (var context = GetNewContext())
            {
                var spSettings = context.RMSharePointOnPremiseSettings.AsQueryable().Where(s => s.SiteGroupId.Equals(groupId) && !s.IsRemoved).ToList();
                return spSettings;
            }
        }

        public void UpdateBCSColumnName(Guid groupId, string bcsColumnName, string columnDescription, bool columnRequired = true)
        {
            using (var context = GetNewContext())
            {
                RMSharePointOnPremiseSetting groupSetting = context.RMSharePointOnPremiseSettings.AsQueryable().Where(s => s.SiteGroupId.Equals(groupId) && s.SiteId.Equals(Guid.Empty)).FirstOrDefault();
                if (groupSetting != null)
                {
                    if ((groupSetting.ColumnName != null && !groupSetting.ColumnName.Equals(bcsColumnName)) ||
                        (groupSetting.Description != null && !groupSetting.Description.Equals(columnDescription)) ||
                        groupSetting.ColumnRequired != columnRequired)
                    {
                        context.RMSharePointOnPremiseSettings.AsQueryable().Where(s => s.SiteGroupId.Equals(groupId) && !s.SiteId.Equals(Guid.Empty)).ToList().ForEach(s =>
                        {
                            s.ColumnName = bcsColumnName;
                            s.Description = columnDescription;
                            s.SettingTime = 0;
                            s.ColumnRequired = columnRequired;
                        });
                        context.SaveChanges();
                    }
                }
            }
        }

        public List<GCommon.Contract.StorageOptimization.Object.UserInfo> GetReocrdOwnersBySettingId(int settingId)
        {
            using (var context = GetNewContext())
            {
                var owners = context.RecordOwner.Where(item => item.SPSettingId == settingId && item.SettingType == (int)RecordOwnerSettingType.SharePointOnPremise).ToList();
                return owners.ConvertAll(item =>
                {
                    var owner = AccountDao.Find(s => s.UserId == item.ObjectId);
                    return new GCommon.Contract.StorageOptimization.Object.UserInfo
                    {
                        UserId = owner.UserId,
                        UserPrincipalName = owner.UserPrincipalName,
                        DisplayName = owner.DisplayName,
                        Email = owner.UserPrincipalName,
                        InviteType = owner.ObjectType == Contract.RMWeb.RMActiveDirectoryObjectType.Group ? GCommon.Contract.Server.Login.InviteType.Group : GCommon.Contract.Server.Login.InviteType.User
                    };
                });
            }
        }

        private void EnsureTermName(RMSPTreeNode node)
        {
            if (!string.IsNullOrEmpty(node.TermName) && node.TermName.Contains(":"))
            {
                node.TermName = node.TermName.Substring(node.TermName.LastIndexOf(":") + 1);
            }
            if (!string.IsNullOrEmpty(node.DefaultTermName) && node.DefaultTermName.Contains(":"))
            {
                node.DefaultTermName = node.DefaultTermName.Substring(node.DefaultTermName.LastIndexOf(":") + 1);
            }
        }

        public void CheckNeedRemoveDescendantsSetting(RMSPTreeNode node, string profileIdPath)
        {
            if (node.EnableRecordManagement != (int)EnableRecordManagementSetting.Enable)
            {
                ScheduleService.DeleteSchedules(ScheduleType.SPOnPremDisposalSchedule, profileIdPath);
                var deleteDescendantsSql = "Delete From {0}.[RMSharePointOnPremiseSettings] Where {1} = @scopeId And ScopeId <> @scopeId";
                var IdLevel = "";
                switch ((NodeLevel)node.Level)
                {
                    case NodeLevel.WebApplication:
                        IdLevel = "SiteGroupId";
                        break;
                    case NodeLevel.SiteCollection:
                        IdLevel = "SiteId";
                        break;
                    case NodeLevel.Site:
                        IdLevel = "WebId";
                        break;
                    case NodeLevel.List:
                        IdLevel = "ListId";
                        break;
                }
                int result = 0;
                using (var context = RMDBContextManager.GetNewDBContext())
                {
                    GCommon.Utility.SecurityUtils.SanitizeSQLSchemaName(context.SchemaName);
                    var sql = string.Format(deleteDescendantsSql, context.SchemaName, IdLevel);
                    using (var tran = context.Database.BeginTransaction())
                    {
                        result = context.Database.ExecuteSqlCommand(sql, new SqlParameter("@scopeId", node.SPObjectId));
                        tran.Commit();
                    }
                }
            }
        }

        public async Task DeleteSharePointSettingAsync(Guid id, Guid siteId)
        {
            using (var context = GetNewContext())
            {
                var spSetting = context.RMSharePointOnPremiseSettings.AsQueryable().Where(s => s.ScopeId.Equals(id) && s.SiteId.Equals(siteId) && !s.IsRemoved).FirstOrDefault();

                if (spSetting != null)
                {
                    context.RMSharePointOnPremiseSettings.Remove(spSetting);
                    await RecordOwnerDao.BatchDeleteAsync(o => o.SPSettingId == spSetting.Id);
                    context.SaveChanges();
                }
            }
        }

        public bool CleanSettingJobTime(RMSPTreeNode node)
        {
            try
            {
                using (var context = GetNewContext())
                {
                    var setting = context.RMSharePointOnPremiseSettings.AsQueryable().Where(s => s.ScopeId.Equals(new Guid(node.SPObjectId)) && !s.IsRemoved).FirstOrDefault();
                    if (setting != null)
                    {
                        setting.SettingTime = 0;
                        ApplyCurrentValues(context, setting);
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
            }
            catch (Exception e)
            {
                logger.Warn($"CleanSettingJobTime Error {e}");
                return false;
            }
        }

        public List<RMSharePointOnPremiseSetting> GetColumnInfos(string[] ids)
        {
            using (var context = GetNewContext())
            {
                return context.RMSharePointOnPremiseSettings.AsNoTracking().Where(t => Enumerable.Contains(ids, t.ScopeId.ToString()) && !t.IsRemoved).ToList();
            }
        }
        public RMSharePointOnPremiseSetting GetSettingInfoByScope(Guid groupId, Guid siteId, Guid scopeId)
        {
            using (var ctx = GetNewContext())
            {
                return ctx.RMSharePointOnPremiseSettings.Where(s => s.SiteGroupId == groupId && s.SiteId == siteId && s.ScopeId == scopeId).FirstOrDefault();
            }
        }
        public RMSharePointOnPremiseSetting GetParentNode(Expression<Func<RMSharePointOnPremiseSetting, bool>> whereLambda)
        {
            RMSharePointOnPremiseSetting result = new RMSharePointOnPremiseSetting();
            using (var context = RMDBContextManager.GetNewDBContext())
            {
                result = context.RMSharePointOnPremiseSettings.AsQueryable().Where(whereLambda).FirstOrDefault();
            }
            return result;
        }

        public RMSharePointOnPremiseSetting GetSettingInfoByAgentGroupId(string id)
        {
            using (var context = GetNewContext())
            {
                var spSetting = context.RMSharePointOnPremiseSettings.AsQueryable().Where(s => s.ScopeId.Equals(new Guid(id)) && !s.IsRemoved).FirstOrDefault();
                return spSetting;
            }
        }

        public List<RMSharePointOnPremiseSetting> LoadRunJobSetting()
        {
            using (var context = GetNewContext())
            {
                return context.RMSharePointOnPremiseSettings.AsQueryable().Where(s => s.SettingTime.Equals(0) && s.NodeInfo != null  && !s.IsRemoved).ToList();
            }
        }
        public List<RMSharePointOnPremiseSetting> LoadAllSetting()
        {
            using (var context = GetNewContext())
            {
                return context.RMSharePointOnPremiseSettings.AsQueryable().Where(s => s.NodeInfo != null && !s.IsRemoved).ToList();
            }
        }

        public List<RMSharePointOnPremiseSetting> LoadExcludeSiteCollectionSetting()
        {
            using (var context = GetNewContext())
            {
                //return context.RMSharePointSettings.AsQueryable().Where(s => !s.SettingTime.Equals(0) && !s.NodeInfo.Equals(null) && s.ScopeId.Equals(s.SiteId)).ToList();
                return context.RMSharePointOnPremiseSettings.AsQueryable().Where(s => s.NodeInfo != null && s.ScopeId.Equals(s.SiteId)).ToList();
            }
        }

        public async Task<bool> SetSettingJobTimeAsync(Guid scopeId, Guid siteId, bool isFailedColumn, bool isFailedProperty)
        {
            bool result = false;
            try
            {
                using (var context = GetNewContext())
                {
                    var setting = context.RMSharePointOnPremiseSettings.AsQueryable().Where(s => s.ScopeId.Equals(scopeId) && s.SiteId.Equals(siteId) && !s.IsRemoved).FirstOrDefault();
                    if (setting != null)
                    {
                        setting.SettingTime = DateTime.UtcNow.Ticks;
                        setting.IsFailedConfigMetaDataColumn = isFailedColumn;
                        setting.IsFailedConfigClassification = isFailedProperty;
                        setting.NeedCheckDefaultValue = false;
                        setting.RunAutoFullJob = false;
                        setting.IncludeDeclaredRecords = false;
                    }
                    result = await UpdateAsync(setting);
                }
            }
            catch (Exception e)
            {
                logger.Error($"An error occurred while set setting job time by node: [{scopeId}], site id: [{siteId}].");
            }
            return result;
        }
        public async Task<bool> SetSettingJobTimeAsync(Guid scopeId, bool isFailedColumn, bool isFailedProperty)
        {
            bool result = false;
            try
            {
                using (var context = GetNewContext())
                {
                    var setting = context.RMSharePointOnPremiseSettings.AsQueryable().Where(s => s.ScopeId.Equals(scopeId) && !s.IsRemoved).FirstOrDefault();
                    if (setting != null)
                    {
                        setting.SettingTime = DateTime.UtcNow.Ticks;
                        setting.IsFailedConfigMetaDataColumn = isFailedColumn;
                        setting.IsFailedConfigClassification = isFailedProperty;
                        setting.NeedCheckDefaultValue = false;
                        setting.RunAutoFullJob = false;
                        setting.IncludeDeclaredRecords = false;
                    }
                    result = await UpdateAsync(setting);
                }
            }
            catch
            {
                //to do log 
                logger.Error($"An error occurred while set setting job time by node: [{scopeId}].");
            }
            return result;
        }

        public List<RMSharePointOnPremiseSetting> LoadShowUniqueIdSetting()
        {
            using (var ctx = GetNewContext())
            {
                return ctx.RMSharePointOnPremiseSettings.Where(s => s.EnableRecordManagement == (int)EnableRecordManagementSetting.Enable && s.IsShowUniqueId == true && s.ScopeId == s.SiteGroupId && !s.IsRemoved).ToList();
            }
        }

        public bool ExistShowUniqueIdSetting()
        {
            using (var ctx = GetNewContext())
            {
                return ctx.RMSharePointOnPremiseSettings.Any(s => s.EnableRecordManagement == (int)EnableRecordManagementSetting.Enable && s.IsShowUniqueId == true && s.ScopeId == s.SiteGroupId && !s.IsRemoved);
            }
        }

        public Dictionary<Guid, bool> GetWebEnableManagementSettingInfo(Guid groupId, Guid siteId)
        {
            Dictionary<Guid, bool> mapping = new Dictionary<Guid, bool>();
            using (var ctx = GetNewContext())
            {
                var webSettings = ctx.RMSharePointOnPremiseSettings.Where(s => s.SiteGroupId == groupId && s.SiteId == siteId && s.ScopeId == s.WebId && s.ScopeId != Guid.Empty && !s.IsRemoved).ToList();
                foreach (var setting in webSettings)
                {
                    mapping.Add(setting.ScopeId, setting.EnableRecordManagement == (int)EnableRecordManagementSetting.Enable);
                }
                return mapping;
            }
        }
    }
}
