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
using AvePoint.RA.Common;
using AvePoint.RA.Common.Global.Utils;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Common;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.Schedule;
using AvePoint.RA.DB.Core;
using AvePoint.RA.DB.Model;
using PnP.Core.QueryModel;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace AvePoint.RA.DB.Dao.Impl
{
    public class TeamsSettingDao : BaseDao<RMTeamsSetting>, ITeamsSettingDao
    {
        private RALogger logger = RALogger.GetInstance(typeof(SharePointSettingDao));
        public IScheduleService ScheduleService { get; set; }
        public IRecordOwnerDao RecordOwnerDao { get; set; }
        public IAccountDao AccountDao { get; set; }
        public ITermDao TermDao { get; set; }
        public IRMKeyValueDao KeyValueDao => (IRMKeyValueDao)PlatformWindsorManager.GetService(typeof(IRMKeyValueDao));

        private IRMRemoteNodeDao mRMRemoteNodeDao;
        protected IRMRemoteNodeDao RMRemoteNodeDao
        {
            get
            {
                if (mRMRemoteNodeDao == null)
                {
                    mRMRemoteNodeDao = (IRMRemoteNodeDao)PlatformWindsorManager.GetService(typeof(IRMRemoteNodeDao));
                }
                return mRMRemoteNodeDao;
            }
        }

        private IRMNodeFlagDao NodeFlagDao => PlatformWindsorManager.GetService<IRMNodeFlagDao>();

        public RMTeamsSetting LoadTeamsSetting(Guid id, Guid teamsId, Guid siteId, bool includeOnlySetPhysicalNode = false)
        {
            using (var context = GetNewContext())
            {
                RMTeamsSetting teamsSetting = null;
                string groupId = null;

                if (teamsId != Guid.Empty)
                {
                    var remoteSite = RMRemoteNodeDao.GetTeamsGroupAndChannelsCollectionByTeamsId(teamsId.ToString());
                    groupId = remoteSite.Item1?.parentId;
                }

                if (siteId != Guid.Empty)
                {
                    teamsSetting = context.RMTeamsSettings.AsQueryable()
                        .FirstOrDefault(s => s.ScopeId == id && s.SiteId == siteId && s.TeamsId == teamsId && !s.IsRemoved && (string.IsNullOrEmpty(groupId) || s.TeamsGroupId == new Guid(groupId)));
                }
                else if (teamsId != Guid.Empty)
                {
                    Expression<Func<RMTeamsSetting, bool>> isExistGroupId = groupId switch
                    {
                        null => setting => true,
                        "" => setting => true,
                        _ => setting => setting.TeamsGroupId == new Guid(groupId)
                    };
                    teamsSetting = context.RMTeamsSettings.AsQueryable().Where(isExistGroupId)
                        .FirstOrDefault(s => s.ScopeId == id && s.TeamsId == teamsId && !s.IsRemoved);
                }

                return teamsSetting ?? context.RMTeamsSettings.AsQueryable()
                    .FirstOrDefault(s => s.ScopeId == id && s.TeamsId == Guid.Empty && s.SiteId == Guid.Empty && !s.IsRemoved);
            }
        }

        // container level setting: container => teams/group => site collection => site => list
        public RMTeamsSetting LoadClosestContainerSetting(RMSPTreeNode treeNode, Guid containerId, Guid teamsId, Guid siteId)
        {
            RMTeamsSetting teamsSetting = null;

            if (treeNode == null)
            {
                return teamsSetting;
            }

            if (treeNode.Level == (int)NodeLevel.WebApplication) teamsId = Guid.Empty; // clear teamsId for container node
            if (treeNode.Level == (int)NodeLevel.WebApplication 
                || treeNode.Level == (int)NodeLevel.Office365GroupEntire) siteId = Guid.Empty; // clear siteId for teams and container node

            if (treeNode.Level == (int)NodeLevel.WebApplication 
                || treeNode.Level == (int)NodeLevel.Office365GroupEntire 
                || treeNode.Level == (int)NodeLevel.SiteCollection 
                || treeNode.Level == (int)NodeLevel.Site 
                || treeNode.Level == (int)NodeLevel.List
                || treeNode.Level == (int)NodeLevel.Library)
            {
                using var context = GetNewContext();
                teamsSetting = context.RMTeamsSettings.AsQueryable()
                    .FirstOrDefault(s => s.ScopeId == new Guid(treeNode.SPObjectId) && s.SiteId == siteId 
                        && s.TeamsId == teamsId && s.TeamsGroupId == containerId && !s.IsRemoved);
            }

            teamsSetting ??= LoadClosestContainerSetting(treeNode.Parent, containerId, teamsId, siteId);
            return teamsSetting;
        }

        public RMTeamsSetting LoadChannalSetting(Guid teamsId, Guid siteId)
        {
            using (var context = GetNewContext())
            {
                RMTeamsSetting teamsSetting = null;
                string groupId = null;

                if (teamsId != Guid.Empty)
                {
                    var remoteSite = RMRemoteNodeDao.GetTeamsGroupAndChannelsCollectionByTeamsId(teamsId.ToString());
                    groupId = remoteSite.Item1?.parentId;
                }

                if (siteId != Guid.Empty)
                {
                    teamsSetting = context.RMTeamsSettings.AsQueryable()
                        .FirstOrDefault(s => s.SiteId == siteId && s.TeamsId == teamsId && !s.IsRemoved && (string.IsNullOrEmpty(groupId) || s.TeamsGroupId == new Guid(groupId)));
                }
                else if (teamsId != Guid.Empty)
                {
                    Expression<Func<RMTeamsSetting, bool>> isExistGroupId = groupId switch
                    {
                        null => setting => true,
                        "" => setting => true,
                        _ => setting => setting.TeamsGroupId == new Guid(groupId)
                    };
                    teamsSetting = context.RMTeamsSettings.AsQueryable().Where(isExistGroupId)
                        .FirstOrDefault(s => s.TeamsGroupId == new Guid(groupId) && s.TeamsId == teamsId && !s.IsRemoved);
                }

                return teamsSetting ?? context.RMTeamsSettings.AsQueryable()
                    .FirstOrDefault(s => s.ScopeId == new Guid(groupId) && s.TeamsId == Guid.Empty && s.SiteId == Guid.Empty && !s.IsRemoved);
            }
        }
        public List<RMTeamsSetting> LoadTeamsSettings(List<Guid> ids, List<Guid> teamsIds)
        {
             using var context = GetNewContext();
             
             var remoteSite = RMRemoteNodeDao.GetTeamsGroupAndChannelsCollectionByListTeamsId(teamsIds.Select(item => item.ToString()));
             List<Guid> groupIds = remoteSite.Select(item => new Guid(item.parentId)).ToList();

             return context.RMTeamsSettings
                 .Where(s => 
                     groupIds.Contains(s.TeamsGroupId) 
                     && ids.Contains(s.ScopeId) 
                     && teamsIds.Contains(s.TeamsId) 
                     && s.SiteId == Guid.Empty
                     && !s.IsRemoved).ToList(); 
        }

        public List<RMTeamsSetting> LoadTeamsSettings(Guid groupId, bool includeOnlySetPhysicalNode = false)
        {
            using (var context = GetNewContext())
            {
                List<RMTeamsSetting> results = null;
                var spSettings = context.RMTeamsSettings.AsQueryable().Where(s => s.TeamsGroupId.Equals(groupId) && !s.IsRemoved).ToList();
                if (!includeOnlySetPhysicalNode)
                {
                    results = new List<RMTeamsSetting>();
                    foreach (var spSetting in spSettings)
                    {
                        if (spSetting.TermId != Guid.Empty || spSetting.EnableRecordManagement != (int)EnableRecordManagementSetting.Enable)
                        {
                            results.Add(spSetting);
                        }
                    }
                }
                else
                {
                    results = spSettings;
                }
                return spSettings;
            }
        }

        public RMTeamsSetting GetParentNode(Expression<Func<RMTeamsSetting, bool>> whereLambda)
        {
            RMTeamsSetting result = new RMTeamsSetting();
            using (var context = GetNewContext())
            {
                result = context.RMTeamsSettings.AsQueryable().Where(whereLambda).FirstOrDefault();
            }
            return result;
        }

        public async Task<bool> CleanSettingJobTimeAsync(RMSPTreeNode node)
        {
            try
            {
                if (node.Type == ContentSourceType.Teams && node.Level == (int)NodeLevel.SiteCollections) // virtual node no setting
                {
                    return false;
                }
                using (var context = GetNewContext())
                {
                    var groupId = Guid.Empty;
                    var teamsId = string.IsNullOrEmpty(node.TeamsId) ? Guid.Empty : new Guid(node.TeamsId);
                    var scopeId = new Guid(node.SPObjectId);
                    if (node.Level == (int)NodeLevel.WebApplication)
                    {
                        groupId = scopeId;
                    }
                    else
                    {
                        groupId = GetGroupIdByScopeId(scopeId, context);
                    }
                    var setting = context.RMTeamsSettings.AsQueryable()
                        .Where(s => s.TeamsGroupId == groupId && s.ScopeId.Equals(new Guid(node.SPObjectId)) && s.TeamsId == teamsId && !s.IsRemoved)
                        .FirstOrDefault();
                    if (setting != null)
                    {
                        setting.SettingTime = 0;
                        await UpdateAsync(setting);
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
                logger.Error($"CleanSettingJobTime error {e}");
                return false;
            }
        }

        private Guid GetGroupIdByScopeId(Guid scopeId, RMDbContext context)
        {
            var setting = context.RMTeamsSettings.Where(s => s.ScopeId == scopeId).FirstOrDefault();
            if (setting != null)
            {
                var teamsId = setting.TeamsId;
                var teams = RMRemoteNodeDao.GetTeamsGroupAndChannelsCollectionByTeamsId(teamsId.ToString()).Item1;
                return teams != null ? new Guid(teams.parentId) : Guid.Empty;
            }
            return Guid.Empty;
        }

        public void UpdateBCSColumnName(Guid groupId, string bcsColumnName, string bcsColumnDescription, bool columnRequired = true, bool columnHidden = false)
        {
            using (var context = GetNewContext())
            {
                RMTeamsSetting groupSetting = context.RMTeamsSettings.AsQueryable().Where(s => s.TeamsGroupId.Equals(groupId) && s.TeamsId.Equals(Guid.Empty)).FirstOrDefault();
                if (groupSetting != null)
                {
                    if ((groupSetting.ColumnName != null && !groupSetting.ColumnName.Equals(bcsColumnName)) ||
                        (groupSetting.Description != null && !groupSetting.Description.Equals(bcsColumnDescription)) ||
                        groupSetting.ColumnRequired != columnRequired || groupSetting.ColumnHidden != columnHidden)
                    {
                        context.RMTeamsSettings.AsQueryable().Where(s => s.TeamsGroupId.Equals(groupId) && !s.TeamsId.Equals(Guid.Empty)).ToList().ForEach(s =>
                        {
                            s.ColumnName = bcsColumnName;
                            s.Description = bcsColumnDescription;
                            s.SettingTime = 0;
                            s.ColumnRequired = columnRequired;
                            s.ColumnHidden = columnHidden;
                        });
                        context.SaveChanges();
                    }
                }
            }
        }

        public async Task AddOrUpdateGlobalSettingAsync(RMSPTreeNode node)
        {
            EnsureTermName(node);
            using (var context = GetNewContext())
            {
                var brokenSiteIds = context.RMTeamsSettings.AsQueryable()
                    .Where(s => s.TeamsGroupId == new Guid(node.SPObjectId) && s.SiteId != Guid.Empty && !s.IsRemoved)
                    .Select(s => s.SiteId)
                    .Distinct()
                    .ToList();

                RMTeamsSetting spSetting = context.RMTeamsSettings.AsQueryable().Where(s => s.ScopeId.Equals(new Guid(node.SPObjectId))).FirstOrDefault();
                if (spSetting != null)
                {
                    node.ContainerTermFullPath = node.TermIdOfContainer != Guid.Empty ? TermDao.GetTermNamesPathByTermId(node.TermIdOfContainer) : "";
                    node.TermScopeFullPath = node.TermId != Guid.Empty ? TermDao.GetTermNamesPathByTermId(node.TermId) : TermDao.GetTermSetNamesPathByTermSetId(node.TermSetId);
                    node.DefaultTermFullPath = node.DefaultTermId != Guid.Empty ? TermDao.GetTermNamesPathByTermId(node.DefaultTermId) : "";

                    spSetting.ColumnName = node.ColumnName;
                    spSetting.ColumnRequired = node.ColumnRequired;
                    spSetting.ColumnHidden = node.ColumnHidden;
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
                    if (node.IsInheritParentTerm != spSetting.IsInheritParentTerm)
                    {
                        spSetting.IsChangedInheritOption = !spSetting.IsChangedInheritOption;
                    }
                    spSetting.IsInheritParentTerm = node.IsInheritParentTerm;
                    spSetting.TermIdOfContainer = node.TermIdOfContainer;
                    spSetting.TermNameOfContainer = node.TermNameOfContainer;
                    //spSetting.IdPath = node.ProfileId;
                    spSetting.isEnableClassification = node.isEnableClassification;
                    spSetting.EnableRecordManagement = node.EnableRecordManagement;
                    spSetting.isFailedConfigMetaDataColumn = false;
                    spSetting.isFailedConfigClassification = false;
                    spSetting.IsUsingExistColumnName = node.IsUsingExistColumnName;
                    spSetting.ExistColumnName = node.ExistColumnName;
                    spSetting.TeamsGroupId = new Guid(node.Id);
                    spSetting.SettingTime = 0;
                    spSetting.NeedCheckDefaultValue = node.NeedCheckDefaultValue;
                    spSetting.IsDisplyaTermPath = node.IsDisplyaTermPath;
                    spSetting.IsShowUniqueId = node.IsShowUniqueId;
                    spSetting.EMailToRecordOwner = node.EMailToRecordOwner;
                    spSetting.NodeInfo = SerializerHelper.SerializeByDataContractSerializer(node);//<JobSettings>(jobInfo)
                    spSetting.ApplyExistType = node.ApplyExistType;
                    spSetting.EnableRelatedRecords = node.EnableRelatedRecords;
                    spSetting.IsNewEdited = true;
                    spSetting.DeployTermMethod = (int)node.DeployTermMethod;
                    spSetting.AutoClassificationRules = node.AutoClassificationRules == null ?
                        null : SerializerHelper.SerializeByDataContractSerializer(node.AutoClassificationRules);
                    spSetting.RunAutoFullJob = node.RunAutoFullJob;
                    spSetting.AlwaysScanAllExistDocuments = node.AlwaysScanAllExistDocuments;
                    spSetting.AutoJobOption = (int)node.AutoJobOption;
                    spSetting.IncludeDeclaredRecords = node.IncludeDeclaredRecords;
                    spSetting.IsSyncData = node.IsSyncData;
                    spSetting.ApprovalType = (ApprovalType)node.ApprovalType;
                    spSetting.WorkflowReferenceId = node.WorkflowReferenceId;
                    spSetting.ApplyTermIncludeFolder = node.ApplyTermIncludeFolder;
                    spSetting.IsKeepSharePointDefaultValue = spSetting.IsUsingExistColumnName 
                        ? node.SetDocLevelTermForExistColumn && node.IsKeepSharePointDefaultValue 
                        : node.IsKeepSharePointDefaultValue;
                    spSetting.SetTermForEmptyDefaultValue = spSetting.IsUsingExistColumnName 
                        ? node.SetDocLevelTermForExistColumn && node.SetTermForEmptyDefaultValue 
                        : node.SetTermForEmptyDefaultValue;
                    spSetting.AITermUseType = node.AITermUseType;
                    spSetting.AIApprovalType = (ApprovalType)node.AIApprovalType;
                    spSetting.AISendEMail = node.AISendEMail;
                    spSetting.AIThenIsDefaultTermMethod = node.AIThenIsDefaultTermMethod;
                    spSetting.AIThenDefaultTermId = node.AIThenDefaultTermId;
                    spSetting.AIThenDefaultTermName = node.AIThenDefaultTermName;

                    await this.UpdateAsync(spSetting);
                    if (node.RecordOwner != null)
                    {
                        await RecordOwnerDao.UpdateRecordOwnersAsync(spSetting.Id, node.RecordOwner, RecordOwnerSettingType.Teams);
                    }
                    if (node.AIReviewers != null)
                    {
                        await RecordOwnerDao.UpdateRecordOwnersAsync(spSetting.Id, node.AIReviewers, RecordOwnerSettingType.AITeams);
                    }
                }
                else
                {
                    node.ContainerTermFullPath = node.TermIdOfContainer != Guid.Empty ? TermDao.GetTermNamesPathByTermId(node.TermIdOfContainer) : "";
                    node.TermScopeFullPath = node.TermId != Guid.Empty ? TermDao.GetTermNamesPathByTermId(node.TermId) : TermDao.GetTermSetNamesPathByTermSetId(node.TermSetId);
                    node.DefaultTermFullPath = node.DefaultTermId != Guid.Empty ? TermDao.GetTermNamesPathByTermId(node.DefaultTermId) : "";

                    RMTeamsSetting settings = new RMTeamsSetting()
                    {
                        ColumnName = node.ColumnName,
                        ColumnRequired = node.ColumnRequired,
                        ColumnHidden = node.ColumnHidden,
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
                        IsInheritParentTerm = node.IsInheritParentTerm,
                        TermIdOfContainer = node.TermIdOfContainer,
                        TermNameOfContainer = node.TermNameOfContainer,
                        //IdPath = node.ProfileId,
                        isEnableClassification = node.isEnableClassification,
                        EnableRecordManagement = node.EnableRecordManagement,
                        isFailedConfigMetaDataColumn = false,
                        isFailedConfigClassification = false,
                        IsUsingExistColumnName = node.IsUsingExistColumnName,
                        ExistColumnName = node.ExistColumnName,
                        TeamsGroupId = new Guid(node.Id),
                        SettingTime = 0,
                        NeedCheckDefaultValue = node.NeedCheckDefaultValue,
                        ApplyExistType = node.ApplyExistType,
                        EnableRelatedRecords = node.EnableRelatedRecords,
                        IsDisplyaTermPath = node.IsDisplyaTermPath,
                        IsShowUniqueId = node.IsShowUniqueId,
                        EMailToRecordOwner = node.EMailToRecordOwner,
                        IsNewEdited = true,
                        NodeInfo = SerializerHelper.SerializeByDataContractSerializer(node),
                        DeployTermMethod = (int)node.DeployTermMethod,
                        AutoClassificationRules = node.AutoClassificationRules == null ?
                            null : SerializerHelper.SerializeByDataContractSerializer(node.AutoClassificationRules),
                        RunAutoFullJob = node.RunAutoFullJob,
                        AlwaysScanAllExistDocuments = node.AlwaysScanAllExistDocuments,
                        AutoJobOption = (int)node.AutoJobOption,
                        IncludeDeclaredRecords = node.IncludeDeclaredRecords,
                        IsSyncData = node.IsSyncData,
                        ApprovalType = (ApprovalType)node.ApprovalType,
                        WorkflowReferenceId = node.WorkflowReferenceId,
                        ApplyTermIncludeFolder = node.ApplyTermIncludeFolder,
                        IsKeepSharePointDefaultValue = node.IsUsingExistColumnName
                            ? node.SetDocLevelTermForExistColumn && node.IsKeepSharePointDefaultValue
                            : node.IsKeepSharePointDefaultValue,
                        SetTermForEmptyDefaultValue = node.IsUsingExistColumnName
                            ? node.SetDocLevelTermForExistColumn && node.SetTermForEmptyDefaultValue
                            : node.SetTermForEmptyDefaultValue,
                        AITermUseType = node.AITermUseType,
                        AIApprovalType = (ApprovalType)node.AIApprovalType,
                        AISendEMail = node.AISendEMail,
                        AIThenIsDefaultTermMethod = node.AIThenIsDefaultTermMethod,
                        AIThenDefaultTermId = node.AIThenDefaultTermId,
                        AIThenDefaultTermName = node.AIThenDefaultTermName,
                    };
                    context.RMTeamsSettings.Add(settings);
                    context.SaveChanges();
                    spSetting = context.RMTeamsSettings.AsQueryable().Where(s => s.ScopeId == settings.ScopeId).First();
                    if (node.RecordOwner != null)
                    {
                        await RecordOwnerDao.AddRecordOwnersAsync(spSetting.Id, node.RecordOwner, RecordOwnerSettingType.Teams);
                    }
                    if (node.AIReviewers != null)
                    {
                        await RecordOwnerDao.AddRecordOwnersAsync(spSetting.Id, node.AIReviewers, RecordOwnerSettingType.AITeams);
                    }
                }
            }
        }

        public async Task<List<RMTeamsSetting>> AddTeamsSettingAsync(List<RMSharePointSetting> spSettings, Guid teamsId)
        {
            using (var context = GetNewContext())
            {
                var settings = spSettings.ConvertAll(spSetting =>
                {
                    return new RMTeamsSetting()
                    {
                        ColumnName = spSetting.ColumnName,
                        ColumnRequired = spSetting.ColumnRequired,
                        ColumnHidden = spSetting.ColumnHidden,
                        DefaultTermId = spSetting.DefaultTermId,
                        Description = spSetting.Description,
                        DefaultTermName = spSetting.DefaultTermName,
                        FullPath = spSetting.FullPath,
                        ScopeId = spSetting.ScopeId,
                        TermId = spSetting.TermId,
                        TermName = spSetting.TermName,
                        TermStoreId = spSetting.TermStoreId,
                        TermSetId = spSetting.TermSetId,
                        TermSetName = spSetting.TermSetName,
                        DescriptionOfContainer = spSetting.DescriptionOfContainer,
                        IsInheritParentTerm = spSetting.IsInheritParentTerm,
                        TermIdOfContainer = spSetting.TermIdOfContainer,
                        TermNameOfContainer = spSetting.TermNameOfContainer,
                        isEnableClassification = spSetting.isEnableClassification,
                        EnableRecordManagement = spSetting.EnableRecordManagement,
                        isFailedConfigMetaDataColumn = spSetting.isFailedConfigClassification,
                        isFailedConfigClassification = spSetting.isFailedConfigMetaDataColumn,
                        IsUsingExistColumnName = spSetting.IsUsingExistColumnName,
                        ExistColumnName = spSetting.ExistColumnName,
                        SetDocLevelTermForExistColumn = spSetting.SetDocLevelTermForExistColumn,
                        TeamsGroupId = spSetting.SiteGroupId,
                        SettingTime = spSetting.SettingTime,
                        NeedCheckDefaultValue = spSetting.NeedCheckDefaultValue,
                        ApplyExistType = spSetting.ApplyExistType,
                        EnableRelatedRecords = spSetting.EnableRelatedRecords,
                        IsDisplyaTermPath = spSetting.IsDisplyaTermPath,
                        IsShowUniqueId = spSetting.IsShowUniqueId,
                        SiteId = spSetting.SiteId,
                        TeamsId = teamsId,
                        IsEnableHoldPhyical = spSetting.IsEnableHoldPhyical,
                        WebId = spSetting.WebId,
                        ListId = spSetting.ListId,
                        FolderId = spSetting.FolderId,
                        EMailToRecordOwner = spSetting.EMailToRecordOwner,
                        IsNewEdited = spSetting.IsNewEdited,
                        NodeInfo = spSetting.NodeInfo,
                        DeployTermMethod = spSetting.DeployTermMethod,
                        AutoClassificationRules = spSetting.AutoClassificationRules,
                        RunAutoFullJob = spSetting.RunAutoFullJob,
                        AlwaysScanAllExistDocuments = spSetting.AlwaysScanAllExistDocuments,
                        AutoJobOption = spSetting.AutoJobOption,
                        IncludeDeclaredRecords = spSetting.IncludeDeclaredRecords,
                        IsSyncData = spSetting.IsSyncData,
                        ApprovalType = spSetting.ApprovalType,
                        WorkflowReferenceId = spSetting.WorkflowReferenceId,
                        ApplyTermIncludeFolder = spSetting.ApplyTermIncludeFolder,
                        IsKeepSharePointDefaultValue = spSetting.IsKeepSharePointDefaultValue,
                        SetTermForEmptyDefaultValue = spSetting.SetTermForEmptyDefaultValue,
                        AITermUseType = spSetting.AITermUseType,
                        AIApprovalType = spSetting.AIApprovalType,
                        AISendEMail = spSetting.AISendEMail,
                        AIThenIsDefaultTermMethod = spSetting.AIThenIsDefaultTermMethod,
                        AIThenDefaultTermId = spSetting.AIThenDefaultTermId,
                        AIThenDefaultTermName = spSetting.AIThenDefaultTermName,
                    };
                });
                var result = context.RMTeamsSettings.AddRange(settings);
                context.SaveChanges();
                return result.ToList();
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

        public void FlagCustomSettingNewColumn(Guid teamsGroupId)
        {
            using var context = GetNewContext();
            var entities = context.RMTeamsSettings.AsQueryable().Where(s => s.TeamsGroupId == teamsGroupId && s.TeamsId != Guid.Empty).ToList();
            foreach (var entity in entities)
            {
                entity.IsUsingExistColumnName = false;
                entity.IsNewEdited = true;
                entity.SettingTime = 0;
            }

            this.BatchUpdate(entities);
        }

        public List<RMTeamsSetting> GetColumnInfos(string[] ids)
        {
            using var context = GetNewContext();
            try
            {
                var groupId = GetGroupIdByScopeId(new Guid(ids.FirstOrDefault()), context);
                List<RMTeamsSetting> settings = null;
                if (groupId == Guid.Empty)
                {
                    settings = context.RMTeamsSettings.AsQueryable().Where(t => Enumerable.Contains(ids, t.ScopeId.ToString()) && !t.IsRemoved).ToList();
                }
                else
                {
                    settings = context.RMTeamsSettings.AsQueryable().Where(t => t.TeamsGroupId == groupId && Enumerable.Contains(ids, t.ScopeId.ToString()) && !t.IsRemoved).ToList();
                }
                if (!settings.Any())
                {
                    return new List<RMTeamsSetting>();
                }
                return settings;
            }
            catch (Exception e)
            {
                throw;
            }
            finally
            {
            }
        }

        public async Task AddOrUpdateGlobalSettingUsingExistColumnAsync(RMSPTreeNode node, bool isNewEditd = false)
        {
            EnsureTermName(node);
            using (var context = GetNewContext())
            {
                RMTeamsSetting spSetting = context.RMTeamsSettings.AsQueryable().Where(s => s.ScopeId.Equals(new Guid(node.SPObjectId))).FirstOrDefault();
                if (spSetting != null)
                {
                    spSetting.IsUsingExistColumnName = node.IsUsingExistColumnName;
                    spSetting.ExistColumnName = node.ExistColumnName;
                    spSetting.SetDocLevelTermForExistColumn = node.SetDocLevelTermForExistColumn;
                    spSetting.SettingTime = 0;
                    spSetting.TermIdOfContainer = node.TermIdOfContainer;
                    spSetting.TermNameOfContainer = node.TermNameOfContainer;
                    spSetting.DescriptionOfContainer = node.DescriptionOfContainer;
                    spSetting.IsInheritParentTerm = node.IsInheritParentTerm;
                    spSetting.isFailedConfigClassification = false;
                    spSetting.EnableRecordManagement = node.EnableRecordManagement;
                    spSetting.isFailedConfigMetaDataColumn = false;
                    spSetting.isEnableClassification = node.isEnableClassification;
                    spSetting.EMailToRecordOwner = node.EMailToRecordOwner;
                    spSetting.NodeInfo = SerializerHelper.SerializeByDataContractSerializer(node);
                    spSetting.EnableRelatedRecords = node.EnableRelatedRecords;
                    spSetting.IsShowUniqueId = node.IsShowUniqueId;
                    spSetting.TeamsGroupId = new Guid(node.Id);
                    //spSetting.IdPath = node.ProfileId;
                    spSetting.IsSyncData = node.IsSyncData;
                    spSetting.IsKeepSharePointDefaultValue = node.SetDocLevelTermForExistColumn && node.IsKeepSharePointDefaultValue;
                    spSetting.SetTermForEmptyDefaultValue = node.SetDocLevelTermForExistColumn && node.SetTermForEmptyDefaultValue;
                    if (isNewEditd)
                    {
                        spSetting.IsNewEdited = true;
                    }
                    await this.UpdateAsync(spSetting);
                    await RecordOwnerDao.UpdateRecordOwnersAsync(spSetting.Id, node.RecordOwner);
                }
                else
                {
                    RMTeamsSetting settings = new RMTeamsSetting()
                    {
                        ExistColumnName = node.ExistColumnName,
                        IsUsingExistColumnName = node.IsUsingExistColumnName,
                        SetDocLevelTermForExistColumn = node.SetDocLevelTermForExistColumn,
                        FullPath = node.FullPath,
                        ScopeId = new Guid(node.SPObjectId),
                        FieldId = Guid.Empty,
                        TeamsGroupId = new Guid(node.Id),
                        SiteId = Guid.Empty,
                        TeamsId = Guid.Empty,
                        WebId = Guid.Empty,
                        ListId = Guid.Empty,
                        TermStoreId = Guid.Empty,
                        TermSetId = Guid.Empty,
                        TermId = Guid.Empty,
                        DefaultTermId = Guid.Empty,
                        TermIdOfContainer = node.TermIdOfContainer,
                        TermNameOfContainer = node.TermNameOfContainer,
                        DescriptionOfContainer = node.DescriptionOfContainer,
                        IsInheritParentTerm = node.IsInheritParentTerm,
                        isEnableClassification = node.isEnableClassification,
                        EnableRecordManagement = node.EnableRecordManagement,
                        EMailToRecordOwner = node.EMailToRecordOwner,
                        EnableRelatedRecords = node.EnableRelatedRecords,
                        IsShowUniqueId = node.IsShowUniqueId,
                        IsKeepSharePointDefaultValue = node.SetDocLevelTermForExistColumn && node.IsKeepSharePointDefaultValue,
                        SetTermForEmptyDefaultValue = node.SetDocLevelTermForExistColumn && node.SetTermForEmptyDefaultValue,
                        SettingTime = 0,
                        NodeInfo = SerializerHelper.SerializeByDataContractSerializer(node),
                        //IdPath = node.ProfileId,
                        IsSyncData = node.IsSyncData,
                    };
                    if (isNewEditd)
                    {
                        settings.IsNewEdited = true;
                    }
                    context.RMTeamsSettings.Add(settings);
                    context.SaveChanges();
                    spSetting = context.RMTeamsSettings.AsQueryable().Where(s => s.ScopeId == settings.ScopeId).FirstOrDefault();
                    if (spSetting != null)
                    {
                        await RecordOwnerDao.AddRecordOwnersAsync(spSetting.Id, node.RecordOwner, RecordOwnerSettingType.Teams);
                    }
                }
                /*
                 * 
                 * 
                 * REC-3771
                //remove all custom setting node
                DeleteCustomSettingUsingExistColumn(new Guid(node.SPObjectId));
                 * 现在由于即使应用了Exist Column，子节点在保存schedule的时候，也会有打破继承的情况，
                 * 故不可以直接将所有子节点删除，只能将其设置成IsNewEdit=false,IsUsingExistColumnName=true,
                 * 在跑job的时候进行判断
                 */
                SetCustomSettingUsingExistColumnByGroup(node);
            }
        }

        public void SetCustomSettingUsingExistColumnByGroup(RMSPTreeNode gNode)
        {
            using (var context = GetNewContext())
            {
                var entities = context.RMTeamsSettings.AsQueryable().Where(s => s.TeamsGroupId == new Guid(gNode.SPObjectId) && s.TeamsId != Guid.Empty).ToList();

                foreach (var entity in entities)
                {
                    entity.IsUsingExistColumnName = true;
                    entity.SetDocLevelTermForExistColumn = gNode.SetDocLevelTermForExistColumn;
                    entity.IsNewEdited = false;
                    entity.SettingTime = 0;
                    entity.EnableRelatedRecords = gNode.EnableRelatedRecords;


                    //entity.IsUsingExistColumnName = gNode.IsUsingExistColumnName;
                    entity.ExistColumnName = gNode.ExistColumnName;
                    entity.IsShowUniqueId = gNode.IsShowUniqueId;
                    entity.EnableRelatedRecords = gNode.EnableRelatedRecords;
                    entity.isEnableClassification = gNode.isEnableClassification;
                    //entity.EnableRecordManagement = gNode.EnableRecordManagement;
                    entity.TermIdOfContainer = gNode.TermIdOfContainer;
                    entity.TermNameOfContainer = gNode.TermNameOfContainer;
                    entity.DescriptionOfContainer = gNode.DescriptionOfContainer;
                    entity.IsInheritParentTerm = gNode.IsInheritParentTerm;
                    entity.isFailedConfigClassification = false;
                    entity.isFailedConfigMetaDataColumn = false;
                    entity.EMailToRecordOwner = gNode.EMailToRecordOwner;
                    entity.IsSyncData = gNode.IsSyncData;
                }

                this.BatchUpdate(entities);
            }
        }

        public async Task DeleteTeamsSettingAsync(Guid id, Guid teamsId, Guid siteId)
        {
            using (var context = GetNewContext())
            {
                var groupId = GetGroupIdByTeamsId(teamsId);
                RMTeamsSetting spSetting = context.RMTeamsSettings.AsQueryable()
                    .Where(s => s.TeamsGroupId.Equals(groupId) && s.ScopeId.Equals(id) && s.TeamsId.Equals(teamsId) && s.SiteId.Equals(siteId) && !s.IsRemoved).FirstOrDefault();

                if (spSetting != null)
                {
                    context.RMTeamsSettings.Remove(spSetting);
                    await RecordOwnerDao.BatchDeleteAsync(o => o.SPSettingId == spSetting.Id);
                    context.SaveChanges();
                }
            }
        }

        private Guid GetGroupIdByTeamsId(Guid teamsId)
        {
            var teams = RMRemoteNodeDao.GetTeamsGroupAndChannelsCollectionByTeamsId(teamsId.ToString()).Item1;
            return teams != null ? new Guid(teams.parentId) : Guid.Empty;
        }

        public async Task AddOrUpdateCustomSettingAsync(RMSPTreeNode node, Guid teamsId, Guid siteId)
        {
            EnsureTermName(node);
            using (var context = GetNewContext())
            {
                var groupId = GetGroupIdByTeamsId(teamsId);

                var needRemoveFlagSiteIds = GetNeedRemoveFlagSiteIds(groupId, teamsId, siteId);

                RMTeamsSetting spSetting = context.RMTeamsSettings.AsQueryable()
                    .Where(s => s.TeamsGroupId == groupId && s.ScopeId.Equals(new Guid(node.SPObjectId)) && s.TeamsId.Equals(teamsId) && s.SiteId.Equals(siteId) && !s.IsRemoved)
                    .FirstOrDefault();
                if (spSetting == null)
                {
                    //add this for RA 3.1 old data.
                    spSetting = context.RMTeamsSettings.AsQueryable()
                        .Where(s => s.TeamsGroupId == groupId && s.ScopeId.Equals(new Guid(node.SPObjectId)) && s.TeamsId.Equals(teamsId) && s.SiteId.Equals(Guid.Empty) && !s.IsRemoved)
                        .FirstOrDefault();
                }
                if (spSetting != null)
                {
                    node.ContainerTermFullPath = node.TermIdOfContainer != Guid.Empty ? TermDao.GetTermNamesPathByTermId(node.TermIdOfContainer) : "";
                    node.TermScopeFullPath = node.TermId != Guid.Empty ? TermDao.GetTermNamesPathByTermId(node.TermId) : TermDao.GetTermSetNamesPathByTermSetId(node.TermSetId);
                    node.DefaultTermFullPath = node.DefaultTermId != Guid.Empty ? TermDao.GetTermNamesPathByTermId(node.DefaultTermId) : "";

                    spSetting.ColumnName = node.ColumnName;
                    spSetting.ColumnRequired = node.ColumnRequired;
                    spSetting.ColumnHidden = node.ColumnHidden;
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
                    if (node.IsInheritParentTerm != spSetting.IsInheritParentTerm)
                    {
                        spSetting.IsChangedInheritOption = !spSetting.IsChangedInheritOption;
                    }
                    spSetting.IsInheritParentTerm = node.IsInheritParentTerm;
                    spSetting.TermIdOfContainer = node.TermIdOfContainer;
                    spSetting.TermNameOfContainer = node.TermNameOfContainer;
                    spSetting.isEnableClassification = node.isEnableClassification;
                    spSetting.EnableRecordManagement = node.EnableRecordManagement;
                    spSetting.isFailedConfigMetaDataColumn = node.isFailedConfigMetaDataColumn;
                    spSetting.isFailedConfigClassification = node.isFailedConfigClassification;
                    spSetting.SiteId = siteId;
                    spSetting.TeamsId = teamsId;
                    spSetting.IsEnableHoldPhyical = node.IsEnableHoldPhyical;
                    spSetting.WebId = node.WebId;
                    spSetting.ListId = node.ListId;
                    spSetting.FolderId = node.FolderId;
                    spSetting.TeamsGroupId = node.SiteGroupId;
                    spSetting.EMailToRecordOwner = node.EMailToRecordOwner;
                    spSetting.SettingTime = 0;
                    spSetting.NodeInfo = SerializerHelper.SerializeByDataContractSerializer(node);
                    spSetting.NeedCheckDefaultValue = node.NeedCheckDefaultValue;
                    spSetting.IsDisplyaTermPath = node.IsDisplyaTermPath;
                    spSetting.ApplyExistType = node.ApplyExistType;
                    spSetting.EnableRelatedRecords = node.EnableRelatedRecords;
                    spSetting.IsShowUniqueId = node.IsShowUniqueId;
                    spSetting.IsNewEdited = true;
                    spSetting.DeployTermMethod = (int)node.DeployTermMethod;
                    spSetting.AutoClassificationRules = node.AutoClassificationRules == null ?
                        null : SerializerHelper.SerializeByDataContractSerializer(node.AutoClassificationRules);
                    spSetting.RunAutoFullJob = node.RunAutoFullJob;
                    spSetting.AlwaysScanAllExistDocuments = node.AlwaysScanAllExistDocuments;
                    spSetting.AutoJobOption = (int)node.AutoJobOption;
                    spSetting.IncludeDeclaredRecords = node.IncludeDeclaredRecords;
                    //spSetting.IdPath = node.ProfileId;
                    spSetting.IsUsingExistColumnName = node.IsUsingExistColumnName;
                    spSetting.ExistColumnName = node.ExistColumnName;
                    spSetting.SetDocLevelTermForExistColumn = node.SetDocLevelTermForExistColumn;
                    spSetting.IsSyncData = node.IsSyncData;
                    spSetting.ApprovalType = (ApprovalType)node.ApprovalType;
                    spSetting.WorkflowReferenceId = node.WorkflowReferenceId;
                    spSetting.ApplyTermIncludeFolder = node.ApplyTermIncludeFolder;

                    spSetting.AITermUseType = node.AITermUseType;
                    spSetting.AIApprovalType = (ApprovalType)node.AIApprovalType;
                    spSetting.AISendEMail = node.AISendEMail;
                    spSetting.AIThenIsDefaultTermMethod = node.AIThenIsDefaultTermMethod;
                    spSetting.AIThenDefaultTermId = node.AIThenDefaultTermId;
                    spSetting.AIThenDefaultTermName = node.AIThenDefaultTermName;
                    
                    await this.UpdateAsync(spSetting);
                    if (node.RecordOwner != null)
                    {
                        await RecordOwnerDao.UpdateRecordOwnersAsync(spSetting.Id, node.RecordOwner, RecordOwnerSettingType.Teams);
                    }
                    if (node.AIReviewers != null)
                    {
                        await RecordOwnerDao.UpdateRecordOwnersAsync(spSetting.Id, node.AIReviewers, RecordOwnerSettingType.AITeams);
                    }
                }
                else
                {
                    node.ContainerTermFullPath = node.TermIdOfContainer != Guid.Empty ? TermDao.GetTermNamesPathByTermId(node.TermIdOfContainer) : "";
                    node.TermScopeFullPath = node.TermId != Guid.Empty ? TermDao.GetTermNamesPathByTermId(node.TermId) : TermDao.GetTermSetNamesPathByTermSetId(node.TermSetId);
                    node.DefaultTermFullPath = node.DefaultTermId != Guid.Empty ? TermDao.GetTermNamesPathByTermId(node.DefaultTermId) : "";

                    RMTeamsSetting settings = new RMTeamsSetting()
                    {
                        ColumnName = node.ColumnName,
                        ColumnRequired = node.ColumnRequired,
                        ColumnHidden = node.ColumnHidden,
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
                        IsInheritParentTerm = node.IsInheritParentTerm,
                        TermIdOfContainer = node.TermIdOfContainer,
                        TermNameOfContainer = node.TermNameOfContainer,
                        isEnableClassification = node.isEnableClassification,
                        EnableRecordManagement = node.EnableRecordManagement,
                        isFailedConfigMetaDataColumn = node.isFailedConfigMetaDataColumn,
                        isFailedConfigClassification = node.isFailedConfigClassification,
                        SiteId = siteId,
                        TeamsId = teamsId,
                        IsEnableHoldPhyical = node.IsEnableHoldPhyical,
                        WebId = node.WebId,
                        FolderId = node.FolderId,
                        ListId = node.ListId,
                        TeamsGroupId = node.SiteGroupId,
                        EMailToRecordOwner = node.EMailToRecordOwner,
                        SettingTime = 0,
                        NeedCheckDefaultValue = node.NeedCheckDefaultValue,
                        IsDisplyaTermPath = node.IsDisplyaTermPath,
                        ApplyExistType = node.ApplyExistType,
                        EnableRelatedRecords = node.EnableRelatedRecords,
                        IsShowUniqueId = node.IsShowUniqueId,
                        IsNewEdited = true,
                        //IdPath = node.ProfileId,
                        NodeInfo = SerializerHelper.SerializeByDataContractSerializer(node),
                        DeployTermMethod = (int)node.DeployTermMethod,
                        AutoClassificationRules = node.AutoClassificationRules == null ?
                            null : SerializerHelper.SerializeByDataContractSerializer(node.AutoClassificationRules),
                        RunAutoFullJob = node.RunAutoFullJob,
                        AlwaysScanAllExistDocuments = node.AlwaysScanAllExistDocuments,
                        IncludeDeclaredRecords = node.IncludeDeclaredRecords,
                        AutoJobOption = (int)node.AutoJobOption,
                        IsUsingExistColumnName = node.IsUsingExistColumnName,
                        ExistColumnName = node.ExistColumnName,
                        SetDocLevelTermForExistColumn = node.SetDocLevelTermForExistColumn,
                        IsSyncData = node.IsSyncData,
                        ApprovalType = (ApprovalType)node.ApprovalType,
                        WorkflowReferenceId = node.WorkflowReferenceId,
                        ApplyTermIncludeFolder = node.ApplyTermIncludeFolder,
                        AITermUseType = node.AITermUseType,
                        AIApprovalType = (ApprovalType)node.AIApprovalType,
                        AISendEMail = node.AISendEMail,
                        AIThenIsDefaultTermMethod = node.AIThenIsDefaultTermMethod,
                        AIThenDefaultTermId = node.AIThenDefaultTermId,
                        AIThenDefaultTermName = node.AIThenDefaultTermName,
                    };

                    context.RMTeamsSettings.Add(settings);
                    context.SaveChanges();
                    spSetting = context.RMTeamsSettings.AsQueryable().Where(s => s.TeamsGroupId == groupId && s.ScopeId == settings.ScopeId && !s.IsRemoved).First();
                    if (node.RecordOwner != null)
                    {
                        await RecordOwnerDao.AddRecordOwnersAsync(spSetting.Id, node.RecordOwner, RecordOwnerSettingType.Teams);
                    }
                    if (node.AIReviewers != null)
                    {
                        await RecordOwnerDao.AddRecordOwnersAsync(spSetting.Id, node.AIReviewers, RecordOwnerSettingType.AITeams);
                    }
                }
            }
        }

        private List<Guid> GetNeedRemoveFlagSiteIds(Guid teamsGroupId, Guid teamsId, Guid siteId)
        {
            using var context = GetNewContext();
            if (siteId != Guid.Empty)
            {
                return new List<Guid> { siteId };
            }

            var brokenSiteIds = context.RMTeamsSettings.AsQueryable()
                                    .Where(s => s.TeamsGroupId == teamsGroupId && s.TeamsId == teamsId && s.SiteId != Guid.Empty && !s.IsRemoved)
                                    .Select(s => s.SiteId)
                                    .Distinct()
                                    .ToList();

            var siteIdsUnderTeams = context.RMRemoteNodes.AsQueryable()
                .Where(n => n.TeamId == teamsId.ToString())
                .Select(n => n.Id)
                .ToList()
                .ConvertAll(id => new Guid(id));

            return siteIdsUnderTeams.Except(brokenSiteIds).ToList();
        }

        public void RemoveDescendantsSetting(RMSPTreeNode node, string profileIdPath)
        {
            if (node.EnableRecordManagement != (int)EnableRecordManagementSetting.Enable)
            {
                ScheduleService.DeleteSchedules(ScheduleType.TeamsDisposalSchedule, profileIdPath);
                var deleteDescendantsSql = "Delete From {0}.[RMTeamsSettings] Where {1} = @scopeId And ScopeId <> @scopeId";
                //var deleteScheduleSql = "Delete From {0}.[RMSchedules] Where Id In (SELECT {1} From {0}.[RMSharePointSettings] Where {2} = @scopeId)";
                var IdLevel = "";
                switch ((NodeLevel)node.Level)
                {
                    case NodeLevel.WebApplication:
                        IdLevel = "TeamsGroupId";
                        break;
                    case NodeLevel.Office365GroupEntire:
                        IdLevel = "TeamsId";
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
                    //var deleteSql1 = string.Format(deleteScheduleSql, context.SchemaName, "DisposalJobId1", IdLevel);
                    //var deleteSql2 = string.Format(deleteScheduleSql, context.SchemaName, "CollectionJobId1", IdLevel);
                    using (var tran = context.Database.BeginTransaction())
                    {
                        //result = context.Database.ExecuteSqlCommand(deleteSql1, new SqlParameter("@scopeId", node.SPObjectId));
                        //result = context.Database.ExecuteSqlCommand(deleteSql2, new SqlParameter("@scopeId", node.SPObjectId));
                        result = context.Database.ExecuteSqlCommand(sql, new SqlParameter("@scopeId", node.SPObjectId));
                        tran.Commit();
                    }
                }
            }
        }

        public RMTeamsSetting GetSettingInfoByAgentGroupId(string id)
        {
            using (var context = GetNewContext())
            {
                var spSetting = context.RMTeamsSettings.AsQueryable().Where(s => s.ScopeId.Equals(new Guid(id)) && !s.IsRemoved).FirstOrDefault();
                return spSetting;
            }
        }

        public List<RMTeamsSetting> LoadRunJobSetting()
        {
            using (var context = GetNewContext())
            {
                context.Database.CommandTimeout = 600;
                return context.RMTeamsSettings.AsQueryable().Where(s => s.SettingTime.Equals(0) && s.NodeInfo != null && !s.IsRemoved).ToList();
            }
        }

        public List<RMTeamsSetting> LoadAllSetting()
        {
            using (var context = GetNewContext())
            {
                return context.RMTeamsSettings.AsQueryable().Where(s => s.NodeInfo != null && !s.IsRemoved).ToList();
            }
        }

        public List<RMTeamsSetting> LoadExcludeTeamsSetting()
        {
            using (var context = GetNewContext())
            {
                return context.RMTeamsSettings.AsQueryable().Where(s => s.NodeInfo != null && s.ScopeId.Equals(s.TeamsId)).ToList();
            }
        }

        public async Task SetSettingJobTimeWithGroupIdAsync(Guid groupId, Guid scopeId, bool isFailedConfigColumn, bool isFailedConfigProperty)
        {
            try
            {
                using (var context = GetNewContext())
                {
                    var setting = context.RMTeamsSettings.AsQueryable().Where(s => s.TeamsGroupId == groupId && s.ScopeId.Equals(scopeId) && !s.IsRemoved).FirstOrDefault();
                    if (setting != null)
                    {
                        setting.SettingTime = DateTime.UtcNow.Ticks;
                        setting.isFailedConfigMetaDataColumn = isFailedConfigColumn;
                        setting.isFailedConfigClassification = isFailedConfigProperty;
                        if (bool.TryParse(KeyValueDao.GetValueByKey(RMKeyValuesConstants.EnableApplySettingAlwaysScanAll)?.Value, out bool enableScanAll) && enableScanAll
                            && (setting.DeployTermMethod == (int)DeployTermMethod.UseDefaultTerm || setting.DeployTermMethod == (int)DeployTermMethod.UseAutoClassification)
                            && (setting.NeedCheckDefaultValue || setting.RunAutoFullJob) && setting.AlwaysScanAllExistDocuments)
                        {
                            logger.Warn($"We will keep scan all, because the setting path:{setting.FullPath}");
                        }
                        else
                        {
                            setting.NeedCheckDefaultValue = false;
                            setting.RunAutoFullJob = false;
                            setting.IncludeDeclaredRecords = false;
                            setting.ApplyTermIncludeFolder = false;
                        }
                    }
                    await UpdateAsync(setting);
                }
            }
            catch (Exception ex)
            {
                logger.Warn($@"fail set setting job time with group id async,ex:{ex}");
            }
        }


        public RMTeamsSetting LoadTeamsSettingForImportSetting(Guid teamsId, Guid scopeId)
        {
            using (var context = RMDBContextManager.GetNewDBContext())
            {
                if (teamsId == Guid.Empty)
                {
                    //查group setting
                    return context.RMTeamsSettings.AsQueryable().Where(s => s.ScopeId == scopeId).FirstOrDefault();
                }
                var groupId = GetGroupIdByTeamsId(teamsId);
                return context.RMTeamsSettings.AsQueryable().Where(s => s.TeamsGroupId == groupId && s.ScopeId == scopeId && s.TeamsId == teamsId).FirstOrDefault();
            }
        }

        public List<RMTeamsSetting> LoadShowUniqueIdSetting()
        {
            using (var ctx = GetNewContext())
            {
                return ctx.RMTeamsSettings.Where(s => s.EnableRecordManagement == (int)EnableRecordManagementSetting.Enable && s.IsShowUniqueId == true && s.ScopeId == s.TeamsGroupId && !s.IsRemoved).ToList();
            }
        }

        public bool ExistShowUniqueIdSetting()
        {
            using (var ctx = GetNewContext())
            {
                return ctx.RMTeamsSettings.Any(s => s.EnableRecordManagement == (int)EnableRecordManagementSetting.Enable && s.IsShowUniqueId == true && s.ScopeId == s.TeamsGroupId && !s.IsRemoved);
            }
        }

        public async Task SetSettingJobTimeAsync(Guid scopeId,Guid teamsId ,Guid siteId, bool isFailedColumn, bool isFailedProperty)
        {
            try
            {
                using (var context = GetNewContext())
                {
                    var groupId = Guid.Empty;
                    if (teamsId == Guid.Empty)
                    {
                        groupId = scopeId;
                    }
                    else
                    {
                        groupId = GetGroupIdByTeamsId(teamsId);
                    }
                    var setting = context.RMTeamsSettings.AsQueryable().Where(s => s.TeamsGroupId == groupId && s.TeamsId == teamsId && s.ScopeId.Equals(scopeId) && s.SiteId.Equals(siteId) && !s.IsRemoved).FirstOrDefault();
                    if (setting != null)
                    {
                        setting.SettingTime = DateTime.UtcNow.Ticks;
                        setting.isFailedConfigMetaDataColumn = isFailedColumn;
                        setting.isFailedConfigClassification = isFailedProperty;
                        if (bool.TryParse(KeyValueDao.GetValueByKey(RMKeyValuesConstants.EnableApplySettingAlwaysScanAll)?.Value, out bool enableScanAll) && enableScanAll
                            && (setting.DeployTermMethod == (int)DeployTermMethod.UseDefaultTerm || setting.DeployTermMethod == (int)DeployTermMethod.UseAutoClassification)
                            && (setting.NeedCheckDefaultValue || setting.RunAutoFullJob) && setting.AlwaysScanAllExistDocuments)
                        {
                            logger.Warn($"We will keep scan all, because the setting path:{setting.FullPath}");
                        }
                        else
                        {
                            setting.NeedCheckDefaultValue = false;
                            setting.RunAutoFullJob = false;
                            setting.IncludeDeclaredRecords = false;
                            setting.ApplyTermIncludeFolder = false;
                        }
                    }
                    await UpdateAsync(setting);
                }
            }
            catch (Exception e)
            {
                logger.Error($"An error occurred while set setting job time by node: [{scopeId}], teams id : {teamsId}, site id: [{siteId}]. Error:[{e.ToString()}]");
            }
        }

        public List<RMTeamsSetting> GetFolderSettingUnderList(Guid listId, Guid siteId, Guid teamsId)
        {
            using (var context = RMDBContextManager.GetNewDBContext())
            {
                var groupId = GetGroupIdByTeamsId(teamsId);
                return context.RMTeamsSettings.Where(s => s.TeamsGroupId == groupId && s.TeamsId == teamsId && s.SiteId == siteId && s.ListId == listId && s.ScopeId == s.FolderId && !s.IsRemoved).ToList();
            }
        }

        public RMTeamsSetting GetSettingInfoByScope(Guid groupId, Guid teamId, Guid siteId, Guid scopeId)
        {
            using (var ctx = GetNewContext())
            {
                return ctx.RMTeamsSettings.Where(s => s.TeamsGroupId == groupId && s.TeamsId == teamId && s.SiteId == siteId && s.ScopeId == scopeId).FirstOrDefault();
            }
        }

        public List<RMTeamsSetting> LoadSettingsUnderTeams(Guid groupId, List<Guid> teamIds)
        {
            using var context = GetNewContext();
            return context.RMTeamsSettings.Where(s => s.TeamsGroupId == groupId && teamIds.Contains(s.TeamsId) && s.ScopeId != s.TeamsId && !s.IsRemoved).ToList();
        }
        
        public List<RMTeamsSetting> LoadSettingsUnderSite(Guid groupId, Guid teamId, Guid siteId)
        {
            using (var context = GetNewContext())
            {
                if(groupId == Guid.Empty)
                {
                    groupId = GetGroupIdBySiteId(siteId);
                }
                return context.RMTeamsSettings.AsQueryable().Where(s => s.TeamsGroupId == groupId && s.TeamsId == teamId && s.SiteId == siteId && !s.IsRemoved).ToList();
            }
        }
        private Guid GetGroupIdBySiteId(Guid siteId)
        {
            var site = RMRemoteNodeDao.GetRemoteSiteCollectionById(siteId.ToString());
            return site != null ? new Guid(site.parentId) : Guid.Empty;
        }

        public bool GetSettingEnableInfoByScope(Guid groupId, Guid teamId, Guid siteId, Guid scopeId)
        {
            using (var ctx = GetNewContext())
            {
                var setting = ctx.RMTeamsSettings.Where(s => s.TeamsGroupId == groupId && s.TeamsId == teamId && s.SiteId == siteId && s.ScopeId == scopeId && !s.IsRemoved).FirstOrDefault();
                if (setting != null)
                {
                    return setting.EnableRecordManagement == (int)EnableRecordManagementSetting.Enable;
                }
                else
                {
                    return true;
                }
            }
        }

        public string GetMedataColumn(Guid nodeId)
        {
            using (var context = GetNewContext())
            {
                Guid groupId = Guid.Empty;
                var webApp = RMRemoteNodeDao.GetWebApplicationById(nodeId.ToString());
                if (webApp != null)
                {
                    groupId = nodeId;
                }
                else
                {
                    groupId = GetGroupIdByScopeId(nodeId, context);
                }
                var setting = context.RMTeamsSettings.AsQueryable().Where(t => t.TeamsGroupId == groupId && t.ScopeId == nodeId && !t.IsRemoved).FirstOrDefault();
                if (setting != null)
                {
                    if (!setting.IsUsingExistColumnName)
                    {
                        return setting.ColumnName;
                    }
                    else
                    {
                        return setting.ExistColumnName;
                    }
                }
                return string.Empty;
            }
        }

        public List<RMTeamsSetting> GetAllGroupSettings()
        {
            using (var context = RMDBContextManager.GetNewDBContext())
            {
                return context.RMTeamsSettings.AsQueryable().Where(g => g.ScopeId == g.TeamsGroupId && !g.IsRemoved).ToList();
            }
        }

        public List<RMTeamsSetting> GetDescendantsDisableNodes(RMSPTreeNode node)
        {
            Expression<Func<RMTeamsSetting, bool>> lambda = null;
            var scopeId = new Guid(node.SPObjectId);
            var groupId = node.SiteGroupId;
            using (var context0 = RMDBContextManager.GetNewDBContext())
            {
                switch ((NodeLevel)node.Level)
                {
                    case NodeLevel.WebApplication:
                        lambda = s => s.TeamsGroupId == scopeId;
                        break;
                    case NodeLevel.Office365GroupEntire:
                        lambda = s => s.TeamsId == scopeId;
                        break;
                    case NodeLevel.SiteCollection:
                        lambda = s => s.SiteId == scopeId;
                        break;
                    case NodeLevel.Site:
                        lambda = s => s.WebId == scopeId;
                        break;
                    case NodeLevel.List:
                        lambda = s => s.ListId == scopeId;
                        break;
                    case NodeLevel.Folder:
                        return new List<RMTeamsSetting>();
                }
            }
            using (var context = RMDBContextManager.GetNewDBContext())
            {
                return context.RMTeamsSettings.Where(lambda).Where(s => s.TeamsGroupId == groupId && s.ScopeId != node.SettingScopeId && s.EnableRecordManagement == (int)EnableRecordManagementSetting.Disable).ToList();
            }
        }

        public List<GCommon.Contract.StorageOptimization.Object.UserInfo> GetRecordOwnersBySettingId(int settingId)
        {
            using (var context = GetNewContext())
            {
                var owners = context.RecordOwner.Where(item => item.SPSettingId == settingId && item.SettingType == (int)RecordOwnerSettingType.Teams).ToList();
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

        public List<RMTeamsSetting> LoadGroupSetting(bool isRecheckRule = true)
        {
            using (var ctx = GetNewContext())
            {
                return ctx.RMTeamsSettings.Where(s => (s.EnableRecordManagement == (int)EnableRecordManagementSetting.Enable || !isRecheckRule) && s.ScopeId == s.TeamsGroupId && !s.IsRemoved).ToList();
            }
        }

        public bool CheckGroupSettingExist(List<string> groupIds)
        {
            using (var context = GetNewContext())
            {
                return context.RMTeamsSettings.AsQueryable()
                    .Any(s => s.NodeInfo != null && s.SiteId == Guid.Empty && s.TeamsId == Guid.Empty && !s.IsRemoved && groupIds.Contains(s.ScopeId.ToString()));
            }
        }

        public Dictionary<Guid, string> GetSiteCollectionIdAndUrlAsync(IEnumerable<string> siteCollectionIds)
        {            
            using var context = GetNewContext();
            return context.RMRemoteNodes.Where(node => siteCollectionIds.Contains(node.Id)).ToDictionary(node => new Guid(node.Id), node => node.Url);
        }

        public List<RMTeamsSetting> GetAllSettings()
        {
            using (var context = GetNewContext())
            {
                return context.RMTeamsSettings.AsQueryable().Where(s => s.NodeInfo != null && !s.IsRemoved).ToList();
            }
        }

        public bool CheckHasInheritChanged(Guid groupId, Guid teamsId)
        {
            using var context = GetNewContext();
            if (teamsId == Guid.Empty)
            {
                return context.RMTeamsSettings.Any(s => s.TeamsGroupId == groupId && s.IsChangedInheritOption && !s.IsRemoved);
            }
            return context.RMTeamsSettings.Any(s => s.TeamsGroupId == groupId && s.TeamsId == teamsId && s.IsChangedInheritOption && !s.IsRemoved);
        }
        
        public bool CheckHasInheritChangedUnderGroup(Guid groupId)
        {
            using var context = GetNewContext();
            return context.RMTeamsSettings.Any(s => s.TeamsGroupId == groupId && s.ScopeId != groupId && s.IsChangedInheritOption && !s.IsRemoved);
        }

        public bool CheckGroupHasInheritChanged(Guid groupId)
        {
            using var context = GetNewContext();
            return context.RMTeamsSettings.Any(s => s.TeamsGroupId == groupId && s.ScopeId == s.TeamsGroupId && s.IsChangedInheritOption && !s.IsRemoved);
        }
        

        public int UpdateChangedInheritOptionFlag(Guid groupId, Guid teamsId)
        {
            using var context = GetNewContext();
            var settings = context.RMTeamsSettings.Where(s => s.TeamsGroupId == groupId && s.TeamsId == teamsId && s.IsChangedInheritOption && !s.IsRemoved).ToList();
            foreach (var setting in settings)
            {
                setting.IsChangedInheritOption = false;
            }
            return context.SaveChanges();
        }
    }
}
