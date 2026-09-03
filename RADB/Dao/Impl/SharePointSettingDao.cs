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
using AvePoint.RA.Common.Util;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Common;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.Schedule;
using AvePoint.RA.DB.Core;
using AvePoint.RA.DB.Model;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.DB.Dao.Impl
{
    public class SharePointSettingDao : BaseDao<RMSharePointSetting>, ISharePointSettingDao
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

        public async Task AddOrUpdateCustomSettingAsync(RMSPTreeNode node, Guid siteId)
        {
            EnsureTermName(node);
            using (var context = GetNewContext())
            {
                var groupId = GetGroupIdBySiteId(siteId);
                RMSharePointSetting spSetting = context.RMSharePointSettings.AsQueryable().Where(s => s.SiteGroupId == groupId && s.ScopeId.Equals(new Guid(node.SPObjectId)) && s.SiteId.Equals(siteId) && !s.IsRemoved).FirstOrDefault();
                if (spSetting == null)
                {
                    //add this for RA 3.1 old data.
                    spSetting = context.RMSharePointSettings.AsQueryable().Where(s => s.SiteGroupId == groupId && s.ScopeId.Equals(new Guid(node.SPObjectId)) && s.SiteId.Equals(Guid.Empty) && !s.IsRemoved).FirstOrDefault();
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
                    spSetting.IsEnableHoldPhyical = node.IsEnableHoldPhyical;
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
                        await RecordOwnerDao.UpdateRecordOwnersAsync(spSetting.Id, node.RecordOwner);
                    }
                    if (node.AIReviewers != null)
                    {
                        await RecordOwnerDao.UpdateRecordOwnersAsync(spSetting.Id, node.AIReviewers, RecordOwnerSettingType.AISharePointOnline);
                    }
                }
                else
                {
                    node.ContainerTermFullPath = node.TermIdOfContainer != Guid.Empty ? TermDao.GetTermNamesPathByTermId(node.TermIdOfContainer) : "";
                    node.TermScopeFullPath = node.TermId != Guid.Empty ? TermDao.GetTermNamesPathByTermId(node.TermId) : TermDao.GetTermSetNamesPathByTermSetId(node.TermSetId);
                    node.DefaultTermFullPath = node.DefaultTermId != Guid.Empty ? TermDao.GetTermNamesPathByTermId(node.DefaultTermId) : "";

                    RMSharePointSetting settings = new RMSharePointSetting()
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
                        IsEnableHoldPhyical = node.IsEnableHoldPhyical,
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
                    //New Dispose Job Schedule & Collection Job Schedule
                    //if (node.Level == (int)NodeLevel.Folder)
                    //{
                    //    //settings.DisposalJobId1 = string.Empty;
                    //}
                    //else
                    //{
                    //    if (node.DisposeScheduleInfo != null)
                    //    {
                    //        node.DisposeScheduleInfo.Id = Guid.NewGuid().ToString();
                    //        node.DisposeScheduleInfo.ProfileId = node.ProfileId;
                    //        //REC-3945, Start Time和End Time中包含时区, 界面操作截取了字符串, 但其他Setting打破继承没有截取非法字符.
                    //        node.DisposeScheduleInfo.StartTime = node.DisposeScheduleInfo.StartTime.Substring(0, 19);
                    //        node.DisposeScheduleInfo.EndTime = node.DisposeScheduleInfo.EndTime.Substring(0, 19);
                    //        var dSchedule = ScheduleService.CopyCreateScheduleService(node.DisposeScheduleInfo, false, GetNodeFullPath(node));
                    //        if (string.IsNullOrEmpty(dSchedule))
                    //        {
                    //            node.DisposeScheduleInfo.Id = string.Empty;
                    //        }

                    //        settings.DisposalJobId1 = node.DisposeScheduleInfo.Id;
                    //    }
                    ////}
                    //if (node.Level == (int)NodeLevel.WebApplication || node.Level == (int)NodeLevel.SiteCollection)
                    //{
                    //    if (node.CollectionScheduleInfo != null)
                    //    {
                    //        node.CollectionScheduleInfo.Id = Guid.NewGuid().ToString();
                    //        node.CollectionScheduleInfo.ProfileId = node.ProfileId;
                    //        //REC-3945, Start Time和End Time中包含时区, 界面操作截取了字符串, 但其他Setting打破继承没有截取非法字符.
                    //        node.CollectionScheduleInfo.StartTime = node.CollectionScheduleInfo.StartTime.Substring(0, 19);
                    //        node.CollectionScheduleInfo.EndTime = node.CollectionScheduleInfo.EndTime.Substring(0, 19);
                    //        var cSchedule = ScheduleService.CopyCreateScheduleService(node.CollectionScheduleInfo, false, GetNodeFullPath(node));
                    //        if (string.IsNullOrEmpty(cSchedule))
                    //        {
                    //            node.CollectionScheduleInfo.Id = string.Empty;
                    //        }

                    //        settings.CollectionJobId1 = node.CollectionScheduleInfo.Id;
                    //    }
                    //}
                    //else
                    //{
                    //    settings.CollectionJobId1 = string.Empty;
                    //}
                    context.RMSharePointSettings.Add(settings);
                    context.SaveChanges();
                    spSetting = context.RMSharePointSettings.AsQueryable().Where(s => s.SiteGroupId == groupId && s.ScopeId == settings.ScopeId && !s.IsRemoved).First();
                    if (node.RecordOwner != null)
                    {
                        await RecordOwnerDao.AddRecordOwnersAsync(spSetting.Id, node.RecordOwner);
                    }
                    if (node.AIReviewers != null)
                    {
                        await RecordOwnerDao.AddRecordOwnersAsync(spSetting.Id, node.AIReviewers, RecordOwnerSettingType.AISharePointOnline);
                    }
                }
            }
        }
        /// <summary>
        /// method for upgrade
        /// </summary>
        /// <param name="spSetting">exist sp setting</param>
        public async Task AddOrUpdateCustomSettingAsync(RMSharePointSetting spSetting)
        {
            using var context = GetNewContext();
            using (var ctx = GetNewContext())
            {
                var setting = ctx.RMSharePointSettings.Where(s => s.ScopeId == spSetting.ScopeId && s.SiteId == spSetting.SiteId).FirstOrDefault();
                if (setting != null)
                {
                    setting.ColumnName = spSetting.ColumnName;
                    setting.DefaultTermId = spSetting.DefaultTermId;
                    setting.DefaultTermName = spSetting.DefaultTermName;
                    setting.FullPath = spSetting.FullPath;
                    setting.ScopeId = spSetting.ScopeId;
                    setting.TermId = spSetting.TermId;
                    setting.TermName = spSetting.TermName;
                    setting.TermSetId = spSetting.TermSetId;
                    setting.TermSetName = spSetting.TermSetName;
                    setting.Description = spSetting.Description;
                    setting.TermStoreId = spSetting.TermStoreId;
                    setting.DescriptionOfContainer = spSetting.DescriptionOfContainer;
                    setting.IsInheritParentTerm = spSetting.IsInheritParentTerm;
                    setting.TermIdOfContainer = spSetting.TermIdOfContainer;
                    setting.TermNameOfContainer = spSetting.TermNameOfContainer;
                    setting.isEnableClassification = spSetting.isEnableClassification;
                    setting.EnableRecordManagement = spSetting.EnableRecordManagement;
                    setting.isFailedConfigMetaDataColumn = spSetting.isFailedConfigMetaDataColumn;
                    setting.isFailedConfigClassification = spSetting.isFailedConfigClassification;
                    setting.SiteId = spSetting.SiteId;
                    setting.IsEnableHoldPhyical = spSetting.IsEnableHoldPhyical;
                    setting.WebId = spSetting.WebId;
                    setting.ListId = spSetting.ListId;
                    setting.SiteGroupId = spSetting.SiteGroupId;
                    setting.SettingTime = 0;
                    setting.IsNewEdited = true;
                    setting.NeedCheckDefaultValue = spSetting.NeedCheckDefaultValue;
                    setting.ApplyExistType = spSetting.ApplyExistType;
                    setting.IsDisplyaTermPath = spSetting.IsDisplyaTermPath;
                    //setting.CollectionJobId1 = spSetting.CollectionJobId1;
                    //setting.DisposalJobId1 = spSetting.DisposalJobId1;
                    //setting.IdPath = spSetting.IdPath;
                    setting.NodeInfo = spSetting.NodeInfo;
                    setting.IsSyncData = spSetting.IsSyncData;
                    await UpdateAsync(setting);
                }
                else
                {

                    ctx.RMSharePointSettings.Add(spSetting);
                    ctx.SaveChanges();
                }

            }
        }

        public void UpdateBCSColumnName(Guid groupId, string bcsColumnName, string columnDescription, bool columnRequired = true, bool columnHidden = false)
        {
            using (var context = GetNewContext())
            {
                RMSharePointSetting groupSetting = context.RMSharePointSettings.AsQueryable().Where(s => s.SiteGroupId.Equals(groupId) && s.SiteId.Equals(Guid.Empty)).FirstOrDefault();
                if (groupSetting != null)
                {
                    if ((groupSetting.ColumnName != null && !groupSetting.ColumnName.Equals(bcsColumnName)) ||
                        (groupSetting.Description != null && !groupSetting.Description.Equals(columnDescription)) ||
                        groupSetting.ColumnRequired != columnRequired || groupSetting.ColumnHidden != columnHidden)
                    {
                        context.RMSharePointSettings.AsQueryable().Where(s => s.SiteGroupId.Equals(groupId) && !s.SiteId.Equals(Guid.Empty)).ToList().ForEach(s =>
                        {
                            s.ColumnName = bcsColumnName;
                            s.Description = columnDescription;
                            s.SettingTime = 0;
                            s.ColumnRequired = columnRequired;
                            s.ColumnHidden = columnHidden;
                        });
                        context.SaveChanges();
                    }
                }
            }
        }
        //public Guid UpdateGlobalSetting(RMSPTreeNode node)
        //{
        //    var context = SharedDbContext;
        //    RMSharePointSetting spSetting = context.RMSharePointSettings.AsQueryable().Where(s => s.ScopeId.Equals(new Guid(node.SPObjectId)) && !s.IsRemoved).FirstOrDefault();
        //    if (spSetting != null)
        //    {
        //        spSetting.ColumnName = node.ColumnName;
        //        spSetting.DefaultTermId = node.DefaultTermId;
        //        spSetting.DefaultTermName = node.DefaultTermName;
        //        spSetting.Description = node.Description;
        //        spSetting.FullPath = node.FullPath;
        //        spSetting.ScopeId = new Guid(node.SPObjectId);
        //        spSetting.TermId = node.TermId;
        //        spSetting.TermName = node.TermName;
        //        spSetting.TermSetId = node.TermSetId;
        //        spSetting.TermSetName = node.TermSetName;
        //        spSetting.DescriptionOfContainer = node.DescriptionOfContainer;
        //        spSetting.TermIdOfContainer = node.TermIdOfContainer;
        //        spSetting.TermNameOfContainer = node.TermNameOfContainer;
        //        spSetting.DocLevelEnableClassification = node.DocLevelEnableClassification;
        //        spSetting.isFailedConfigMetaDataColumn = node.isFailedConfigMetaDataColumn;
        //        spSetting.isFailedConfigClassification = node.isFailedConfigClassification;
        //        spSetting.SiteGroupId = node.SiteGroupId;
        //        spSetting.EMailToRecordOwner = node.EMailToRecordOwner;
        //        this.Update(spSetting);
        //        RecordOwnerDao.UpdateRecordOwners(spSetting.Id, node.RecordOwner);
        //        return spSetting.ScopeId;
        //    }
        //    return Guid.Empty;
        //}
        public async Task AddOrUpdateGlobalSettingAsync(RMSPTreeNode node)
        {
            EnsureTermName(node);
            using (var context = GetNewContext())
            {
                var brokenSiteIds = context.RMSharePointSettings.AsQueryable()
                                    .Where(s => s.SiteGroupId == new Guid(node.SPObjectId) && s.SiteId != Guid.Empty && !s.IsRemoved)
                                    .Select(s => s.SiteId)
                                    .Distinct()
                                    .ToList();

                RMSharePointSetting spSetting = context.RMSharePointSettings.AsQueryable().Where(s => s.ScopeId.Equals(new Guid(node.SPObjectId))).FirstOrDefault();
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
                    spSetting.SiteGroupId = new Guid(node.Id);
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
                    spSetting.IsKeepSharePointDefaultValue = node.IsKeepSharePointDefaultValue;
                    spSetting.SetTermForEmptyDefaultValue = node.SetTermForEmptyDefaultValue;
                    spSetting.AITermUseType = node.AITermUseType;
                    spSetting.AIApprovalType = (ApprovalType)node.AIApprovalType;
                    spSetting.AISendEMail = node.AISendEMail;
                    spSetting.AIThenIsDefaultTermMethod = node.AIThenIsDefaultTermMethod;
                    spSetting.AIThenDefaultTermId = node.AIThenDefaultTermId;
                    spSetting.AIThenDefaultTermName = node.AIThenDefaultTermName;

                    await this.UpdateAsync(spSetting);
                    if (node.RecordOwner != null)
                    {
                        await RecordOwnerDao.UpdateRecordOwnersAsync(spSetting.Id, node.RecordOwner);
                    }
                    if (node.AIReviewers != null)
                    {
                        await RecordOwnerDao.UpdateRecordOwnersAsync(spSetting.Id, node.AIReviewers, RecordOwnerSettingType.AISharePointOnline);
                    }
                }
                else
                {
                    node.ContainerTermFullPath = node.TermIdOfContainer != Guid.Empty ? TermDao.GetTermNamesPathByTermId(node.TermIdOfContainer) : "";
                    node.TermScopeFullPath = node.TermId != Guid.Empty ? TermDao.GetTermNamesPathByTermId(node.TermId) : TermDao.GetTermSetNamesPathByTermSetId(node.TermSetId);
                    node.DefaultTermFullPath = node.DefaultTermId != Guid.Empty ? TermDao.GetTermNamesPathByTermId(node.DefaultTermId) : "";

                    RMSharePointSetting settings = new RMSharePointSetting()
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
                        SiteGroupId = new Guid(node.Id),
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
                        IsKeepSharePointDefaultValue = node.IsKeepSharePointDefaultValue,
                        SetTermForEmptyDefaultValue = node.SetTermForEmptyDefaultValue,
                        AITermUseType = node.AITermUseType,
                        AIApprovalType = (ApprovalType)node.AIApprovalType,
                        AISendEMail = node.AISendEMail,
                        AIThenIsDefaultTermMethod = node.AIThenIsDefaultTermMethod,
                        AIThenDefaultTermId = node.AIThenDefaultTermId,
                        AIThenDefaultTermName = node.AIThenDefaultTermName,
                    };
                    context.RMSharePointSettings.Add(settings);
                    context.SaveChanges();
                    spSetting = context.RMSharePointSettings.AsQueryable().Where(s => s.ScopeId == settings.ScopeId).First();
                    if (node.RecordOwner != null)
                    {
                        await RecordOwnerDao.AddRecordOwnersAsync(spSetting.Id, node.RecordOwner);
                    }
                    if (node.AIReviewers != null)
                    {
                        await RecordOwnerDao.AddRecordOwnersAsync(spSetting.Id, node.AIReviewers, RecordOwnerSettingType.AISharePointOnline);
                    }
                }
            }
        }
        public List<RMSharePointSetting> LoadSharePointSettings(Guid groupId, bool includeOnlySetPhysicalNode = false)
        {
            using (var context = GetNewContext())
            {
                List<RMSharePointSetting> results = null;
                var spSettings = context.RMSharePointSettings.AsQueryable().Where(s => s.SiteGroupId.Equals(groupId) && !s.IsRemoved).ToList();
                if (!includeOnlySetPhysicalNode)
                {
                    results = new List<RMSharePointSetting>();
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
        /// <summary>
        /// 获取Global或Custom Setting
        /// </summary>
        /// <param name="id"></param>
        /// <param name="siteId"></param>
        /// <param name="includeOnlySetPhysicalNode">是否获取只设置了“Mark the Physical Library”的节点。
        /// (界面回显需要传true，其他获取SharePoint Setting的情况不需要传值)</param>
        /// <returns></returns>
        public RMSharePointSetting LoadSharePointSetting(Guid id, Guid siteId, bool includeOnlySetPhysicalNode = false)
        {
            using (var context = GetNewContext())
            {
                RMSharePointSetting spSetting = null;
                if (siteId != Guid.Empty)
                {
                    var remoteSite = RMRemoteNodeDao.GetRemoteSiteCollectionById(siteId.ToString());
                    var groupId = remoteSite?.parentId;
                    if (!string.IsNullOrEmpty(groupId))
                    {
                        spSetting = context.RMSharePointSettings.AsQueryable().Where(s => s.ScopeId.Equals(id) && s.SiteId.Equals(siteId) && s.SiteGroupId.Equals(new Guid(groupId)) && !s.IsRemoved).FirstOrDefault();
                    }
                    else
                    {
                        spSetting = context.RMSharePointSettings.AsQueryable().Where(s => s.ScopeId.Equals(id) && s.SiteId.Equals(siteId) && !s.IsRemoved).FirstOrDefault();
                    }
                    //当TermId为空时，代表该节点只设置了“Mark the Physical Library”，并没有设置Custom Setting所以返回null.
                    if (!includeOnlySetPhysicalNode
                        && spSetting != null
                        && spSetting.TermId == Guid.Empty && spSetting.EnableRecordManagement == (int)EnableRecordManagementSetting.Enable)
                    {
                        spSetting = null;
                    }
                }
                if (spSetting == null)
                {
                    //add this for RA 3.1 old data.
                    spSetting = context.RMSharePointSettings.AsQueryable().Where(s => s.ScopeId.Equals(id) && s.SiteId.Equals(Guid.Empty) && !s.IsRemoved).FirstOrDefault();
                }
                return spSetting;
            }
        }

        public RMSharePointSetting LoadChannelSetting(Guid scopeId, int id)
        {
            var channelContainerId = "41cfe969-e07b-45cb-a7d0-b022f967e929";
            using var context = GetNewContext();
            try
            {
                var spSetting = context.RMSharePointSettings.AsQueryable().Where(s => s.ScopeId.Equals(scopeId) && s.Id == id && !s.IsRemoved).FirstOrDefault();
                if(spSetting == null)
                {
                    return context.RMSharePointSettings.AsQueryable().Where(s => s.ScopeId == new Guid(channelContainerId) && s.SiteGroupId == new Guid(channelContainerId) && !s.IsRemoved).FirstOrDefault();
                }

                var channelSetting = context.RMSharePointSettings.AsQueryable().Where(s => s.ScopeId == new Guid(channelContainerId) && s.SiteGroupId == new Guid(channelContainerId) && !s.IsRemoved).FirstOrDefault();
                if(channelSetting != null)
                {
                    spSetting.IsKeepSharePointDefaultValue = channelSetting.IsKeepSharePointDefaultValue;
                    spSetting.SetTermForEmptyDefaultValue = channelSetting.SetTermForEmptyDefaultValue;
                }
                return spSetting;
            }
            catch(Exception e)
            {
                throw;
            }
            
        }

        public async Task DeleteSharePointSettingAsync(Guid id, Guid siteId)
        {
            using (var context = GetNewContext())
            {
                var groupId = GetGroupIdBySiteId(siteId);
                RMSharePointSetting spSetting = context.RMSharePointSettings.AsQueryable().Where(s => s.SiteGroupId.Equals(groupId) && s.ScopeId.Equals(id) && s.SiteId.Equals(siteId) && !s.IsRemoved).FirstOrDefault();

                if (spSetting != null)
                {
                    context.RMSharePointSettings.Remove(spSetting);
                    await RecordOwnerDao.BatchDeleteAsync(o => o.SPSettingId == spSetting.Id);
                    context.SaveChanges();
                }
            }
        }

        public static string ForeachClassProperties<T>(T model)
        {
            var builder = new StringBuilder();
            builder.Append("{");
            Type t = model.GetType();
            PropertyInfo[] PropertyList = t.GetProperties();
            foreach (PropertyInfo item in PropertyList)
            {
                string name = item.Name;
                object value = item.GetValue(model, null);
                builder.AppendFormat(@"""{0}"":""{1}"", ", name, value?.ToString().Replace("\"", "\\\""));
            }
            builder.Remove(builder.Length - 2, 2);//remove , and space
            builder.Append("}");
            return builder.ToString();
        }

        public void MarkRemovedSharePointSetting(Guid scopeId)
        {
            using (var context = GetNewContext())
            {
                RMSharePointSetting spSetting = context.RMSharePointSettings.AsQueryable().Where(s => s.ScopeId.Equals(scopeId) && !s.IsRemoved).FirstOrDefault();
                if (spSetting != null)
                {
                    logger.Info("mark removed SharePoint setting dirty data:{0}", ForeachClassProperties(spSetting));
                    spSetting.IsRemoved = true;
                    //context.RMSharePointSettings.Remove(spSetting);
                    //var deletes = RecordOwnerDao.FindList(o => o.SPSettingId == spSetting.Id);
                    //foreach (var item in deletes)
                    //{
                    //    logger.Info("remove record owner dirty data:{0}", ForeachClassProperties(item));
                    //}
                    //RecordOwnerDao.BatchDelete(deletes);
                    context.SaveChanges();
                }
            }
        }

        public async Task MarkRemovedSharePointSettingUnderCurrentAsync(Expression<Func<RMSharePointSetting, bool>> lambda)
        {
            using (var context = GetNewContext())
            {
                var deletes = await FindListAsync(lambda);
                foreach (var item in deletes)
                {
                    logger.Info("mark removed SharePoint setting dirty data:{0}", ForeachClassProperties(item));
                    item.IsRemoved = true;
                }
                context.SaveChanges();
            }
            //BatchDelete(deletes);
        }
        public string GetMedataColumn()
        {
            using (var context = GetNewContext())
            {
                var setting = context.RMSharePointSettings.AsQueryable().Where(s => !s.IsRemoved).FirstOrDefault();
                if (setting != null)
                {
                    return setting.ColumnName;
                }
                return string.Empty;
            }
        }
        /// <summary>
        /// nodeId is Group Node Id for get column name.
        /// </summary>
        /// <param name="nodeId"></param>
        /// <returns></returns>
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
                var setting = context.RMSharePointSettings.AsQueryable().Where(t => t.SiteGroupId == groupId && t.ScopeId.Equals(nodeId) && !t.IsRemoved).FirstOrDefault();
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
        #region remove old method
        //public void AddOrUpdateColumnInfo(Guid siteId, Guid webId, Guid listId, Guid fieldId, RMSPTreeNode node)
        //{
        //    var context = SharedDbContext;
        //    var setting = context.RMSharePointSettings.AsQueryable().Where(t => t.SiteId.Equals(siteId)
        //    && t.WebId.Equals(webId) && t.ListId.Equals(listId)).FirstOrDefault();
        //    if (setting != null)
        //    {
        //        setting.FieldId = fieldId;
        //        setting.ColumnName = node.ColumnName;
        //        setting.DefaultTermId = node.DefaultTermId;
        //        setting.TermSetId = node.TermSetId;
        //        setting.TermId = node.TermId;
        //        setting.DefaultTermName = node.DefaultTermName;
        //        setting.TermName = node.TermName;
        //        setting.TermSetName = node.TermSetName;
        //        setting.TermStoreId = node.TermStoreId;
        //        setting.TermIdOfContainer = node.TermIdOfContainer;
        //        setting.TermNameOfContainer = node.TermNameOfContainer;
        //        setting.isFailedConfigMetaDataColumn = node.isFailedConfigMetaDataColumn;
        //        setting.isFailedConfigClassification = node.isFailedConfigClassification;
        //        this.Update(setting);
        //    }
        //    else
        //    {
        //        context.RMSharePointSettings.Add(new RMSharePointSetting()
        //        {
        //            ColumnName = node.ColumnName,
        //            FieldId = fieldId,
        //            ScopeId = Guid.NewGuid(),
        //            SiteId = siteId,
        //            WebId = webId,
        //            ListId = listId,
        //            FullPath = node.FullPath,
        //            DefaultTermId = node.DefaultTermId,
        //            TermSetId = node.TermSetId,
        //            TermId = node.TermId,
        //            DefaultTermName = node.DefaultTermName,
        //            TermName = node.TermName,
        //            TermSetName = node.TermSetName,
        //            TermStoreId = node.TermStoreId,
        //            Description = node.Description,
        //            DescriptionOfContainer = node.DescriptionOfContainer,
        //            TermIdOfContainer = node.TermIdOfContainer,
        //            TermNameOfContainer = node.TermNameOfContainer,
        //            isFailedConfigMetaDataColumn = node.isFailedConfigMetaDataColumn,
        //            isFailedConfigClassification = node.isFailedConfigClassification
        //        });
        //        context.SaveChanges();
        //    }
        //}
        //public Guid UpdateColumnInfo(Guid siteId, Guid webId, Guid listId, Guid fieldId, RMSPTreeNode node)
        //{
        //    var context = SharedDbContext;
        //    var setting = context.RMSharePointSettings.AsQueryable().Where(t => t.SiteId.Equals(siteId)
        //    && t.WebId.Equals(webId) && t.ListId.Equals(listId)).FirstOrDefault();
        //    if (setting != null)
        //    {
        //        setting.FieldId = fieldId;
        //        setting.ColumnName = node.ColumnName;
        //        setting.DefaultTermId = node.DefaultTermId;
        //        setting.TermSetId = node.TermSetId;
        //        setting.TermId = node.TermId;
        //        setting.DefaultTermName = node.DefaultTermName;
        //        setting.TermName = node.TermName;
        //        setting.TermSetName = node.TermSetName;
        //        setting.TermStoreId = node.TermStoreId;
        //        setting.TermIdOfContainer = node.TermIdOfContainer;
        //        setting.TermNameOfContainer = node.TermNameOfContainer;
        //        setting.DescriptionOfContainer = node.DescriptionOfContainer;
        //        setting.isFailedConfigMetaDataColumn = node.isFailedConfigMetaDataColumn;
        //        setting.isFailedConfigClassification = node.isFailedConfigClassification;
        //        this.Update(setting);
        //        return setting.ScopeId;
        //    }
        //    return Guid.Empty;
        //}

        //public Guid GetSiteColumnId(Guid siteId)
        //{
        //    var context = SharedDbContext;
        //    var setting = context.RMSharePointSettings.AsQueryable().Where(t => t.SiteId.Equals(siteId) && t.WebId.Equals(Guid.Empty) && t.ListId.Equals(Guid.Empty)).FirstOrDefault();
        //    if (setting != null)
        //    {
        //        return setting.FieldId;
        //    }
        //    return Guid.Empty;
        //}

        //public Guid GetListColumnId(Guid siteId, Guid webId, Guid listId)
        //{
        //    var context = SharedDbContext;
        //    var setting = context.RMSharePointSettings.AsQueryable().Where(t => t.SiteId.Equals(siteId) && t.WebId.Equals(webId) && t.ListId.Equals(listId)).FirstOrDefault();
        //    if (setting != null)
        //    {
        //        return setting.FieldId;
        //    }
        //    return Guid.Empty;
        //}
        //public RMSharePointSetting GetListClassificationSetting(Guid siteId, Guid webId, Guid listId)
        //{
        //    var context = SharedDbContext;
        //    var setting = context.RMSharePointSettings.AsQueryable().Where(t => t.SiteId.Equals(siteId) && t.WebId.Equals(webId) && t.ListId.Equals(listId)).FirstOrDefault();
        //    if (setting != null)
        //    {
        //        return setting;
        //    }
        //    return null;
        //}

        //public void DeleteCustomSetting(Guid siteId, Guid webId, Guid listId)
        //{
        //    var context = SharedDbContext;
        //    Guid scopeID = Guid.Empty;
        //    var setting = context.RMSharePointSettings.AsQueryable().Where(t => t.SiteId.Equals(siteId) && t.WebId.Equals(webId) && t.ListId.Equals(listId)).FirstOrDefault();
        //    if (setting != null)
        //    {
        //        scopeID = setting.ScopeId;
        //    }
        //    if (scopeID != Guid.Empty)
        //    {
        //        DeleteSharePointSetting(scopeID);
        //    }
        //}

        //public RMSharePointSetting GetSiteColumnInfo(Guid siteId)
        //{
        //    var context = SharedDbContext;
        //    RMSharePointSetting spSetting = context.RMSharePointSettings.AsQueryable().Where(s => s.SiteId.Equals(siteId)).FirstOrDefault();
        //    return spSetting;
        //}
        #endregion
        public List<RMSharePointSetting> GetColumnInfos(string[] ids)
        {
            using var context = GetNewContext();
            try
            {
                var groupId = GetGroupIdByScopeId(new Guid(ids.FirstOrDefault()), context);
                List<RMSharePointSetting> settings = null;
                if (groupId == Guid.Empty)
                {
                    settings = context.RMSharePointSettings.AsQueryable().Where(t => Enumerable.Contains(ids, t.ScopeId.ToString()) && !t.IsRemoved).ToList();
                }
                else
                {
                    settings = context.RMSharePointSettings.AsQueryable().Where(t => t.SiteGroupId == groupId && Enumerable.Contains(ids, t.ScopeId.ToString()) && !t.IsRemoved).ToList();
                }
                if (!settings.Any())
                {
                    return new List<RMSharePointSetting>();
                }
                return settings;
            }
            catch (Exception e)
            {
                throw e;
            }
            finally
            {
            }

        }
        public List<RMSharePointSetting> GetAllPhysicalSiteSettings()
        {
            using (var context = GetNewContext())
            {
                var settings = context.RMSharePointSettings.AsQueryable().Where(t => t.IsEnableHoldPhyical == true && !t.IsRemoved).ToList();
                return settings;
            }
        }

        public RMSharePointSetting GetGroupLevelGlobalSetting(string groupName, Guid scopeId)
        {
            using (var context = RMDBContextManager.GetNewDBContext())
            {
                return context.RMSharePointSettings.FirstOrDefault(a => a.FullPath.Equals(groupName, StringComparison.OrdinalIgnoreCase) && a.ScopeId.Equals(scopeId) && !a.IsRemoved);
            }
        }

        public RMSharePointSetting GetSiteLevelSetting(string fullPath, Guid scopeId)
        {
            using (var context = RMDBContextManager.GetNewDBContext())
            {
                return context.RMSharePointSettings.FirstOrDefault(a => a.FullPath.Equals(fullPath, StringComparison.OrdinalIgnoreCase) || a.ScopeId.Equals(scopeId) && !a.IsRemoved);
            }
        }


        public async Task DeleteSharePointSettingBySiteIdAsync(Guid id)
        {
            using (var context = GetNewContext())
            {
                IEnumerable<int> settingIds = null;
                //GET siteid
                Guid siteId = Guid.Empty;
                RMSharePointSetting scSetting = context.RMSharePointSettings.AsQueryable().Where(s => s.ScopeId.Equals(id)).FirstOrDefault();
                if (scSetting != null)
                {
                    siteId = context.RMSharePointSettings.AsQueryable().Where(s => s.FullPath.Equals(scSetting.FullPath) && !s.SiteId.Equals(Guid.Empty)).Select(s => s.SiteId).FirstOrDefault();
                }
                List<Guid> subWebIds = context.RMSharePointSettings.AsQueryable().Where(s => s.SiteId.Equals(siteId) && !s.WebId.Equals(Guid.Empty)).Select(s => s.WebId).ToList();
                //delete subsite
                if (subWebIds != null && subWebIds.Count > 0)
                {
                    List<RMSharePointSetting> subWebSettings = context.RMSharePointSettings.AsQueryable().Where(t => subWebIds.Contains(t.ScopeId)).ToList();
                    if (subWebSettings != null)
                    {
                        context.RMSharePointSettings.RemoveRange(subWebSettings);
                        settingIds = subWebSettings.Select(s => s.Id);
                        await RecordOwnerDao.BatchDeleteAsync(o => settingIds.Contains(o.SPSettingId));
                        context.SaveChanges();
                    }
                }
                //delete all list
                List<Guid> allListIds = context.RMSharePointSettings.AsQueryable().Where(s => s.SiteId.Equals(siteId) && !s.WebId.Equals(Guid.Empty) && !s.ListId.Equals(Guid.Empty)).Select(s => s.ListId).ToList();
                if (allListIds != null && allListIds.Count > 0)
                {
                    List<RMSharePointSetting> listSettingsOfScopeId = context.RMSharePointSettings.AsQueryable().Where(t => allListIds.Contains(t.ScopeId)).ToList();
                    if (listSettingsOfScopeId != null && listSettingsOfScopeId.Count > 0)
                    {
                        context.RMSharePointSettings.RemoveRange(listSettingsOfScopeId);
                        settingIds = listSettingsOfScopeId.Select(s => s.Id);
                        await RecordOwnerDao.BatchDeleteAsync(o => settingIds.Contains(o.SPSettingId));
                        context.SaveChanges();
                    }

                    List<RMSharePointSetting> listSettingsOfListId = context.RMSharePointSettings.AsQueryable().Where(s => s.SiteId.Equals(siteId) && allListIds.Contains(s.ListId)).ToList();
                    context.RMSharePointSettings.RemoveRange(listSettingsOfListId);
                    settingIds = listSettingsOfListId.Select(s => s.Id);
                    await RecordOwnerDao.BatchDeleteAsync(o => settingIds.Contains(o.SPSettingId));
                    context.SaveChanges();

                }
                //delete sitecollection
                RMSharePointSetting sitecSetting = context.RMSharePointSettings.AsQueryable().Where(s => s.ScopeId.Equals(id)).FirstOrDefault();
                if (sitecSetting != null)
                {
                    List<RMSharePointSetting> scSettings = context.RMSharePointSettings.AsQueryable().Where(s => s.FullPath == scSetting.FullPath).ToList();
                    if (scSettings != null && scSettings.Count > 0)
                    {
                        context.RMSharePointSettings.RemoveRange(scSettings);
                        settingIds = scSettings.Select(s => s.Id);
                        await RecordOwnerDao.BatchDeleteAsync(o => settingIds.Contains(o.SPSettingId));
                        context.SaveChanges();
                    }
                }
            }

        }
        public void DeleteCustomSettingUsingExistColumn(Guid siteGroupId)
        {
            using (var context = GetNewContext())
            {
                var entities = context.RMSharePointSettings.AsQueryable().Where(s => s.SiteGroupId == siteGroupId && s.SiteId != Guid.Empty && !s.IsRemoved);

                this.BatchDelete(entities.ToList());
            }
        }
        //public void AddOrUpdateGlobalSettingUsingExistColumn(RMSPTreeNode node)
        //{
        //    var context = SharedDbContext;
        //    RMSharePointSetting spSetting = context.RMSharePointSettings.AsQueryable().Where(s => s.ScopeId.Equals(new Guid(node.SPObjectId)) && !s.IsRemoved).FirstOrDefault();
        //    if (spSetting != null)
        //    {
        //        spSetting.IsUsingExistColumnName = node.IsUsingExistColumnName;
        //        spSetting.ExistColumnName = node.ExistColumnName;
        //        spSetting.SettingTime = 0;
        //        spSetting.TermIdOfContainer = node.TermIdOfContainer;
        //        spSetting.TermNameOfContainer = node.TermNameOfContainer;
        //        spSetting.DescriptionOfContainer = node.DescriptionOfContainer;
        //        spSetting.isFailedConfigClassification = false;
        //        spSetting.isFailedConfigMetaDataColumn = false;
        //        spSetting.isEnableClassification = node.isEnableClassification;
        //        spSetting.EMailToRecordOwner = node.EMailToRecordOwner;
        //        spSetting.EnableRelatedRecords = node.EnableRelatedRecords;
        //        spSetting.NodeInfo = SerializerHelper.SerializeByDataContractSerializer(node);
        //        this.Update(spSetting);
        //        RecordOwnerDao.UpdateRecordOwners(spSetting.Id, node.RecordOwner);
        //    }
        //    else
        //    {
        //        RMSharePointSetting settings = new RMSharePointSetting()
        //        {
        //            ExistColumnName = node.ExistColumnName,
        //            IsUsingExistColumnName = node.IsUsingExistColumnName,
        //            FullPath = node.FullPath,
        //            ScopeId = new Guid(node.SPObjectId),
        //            FieldId = Guid.Empty,
        //            SiteGroupId = node.SiteGroupId,
        //            SiteId = Guid.Empty,
        //            WebId = Guid.Empty,
        //            ListId = Guid.Empty,
        //            TermStoreId = Guid.Empty,
        //            TermSetId = Guid.Empty,
        //            TermId = Guid.Empty,
        //            DefaultTermId = Guid.Empty,
        //            TermIdOfContainer = node.TermIdOfContainer,
        //            TermNameOfContainer = node.TermNameOfContainer,
        //            DescriptionOfContainer = node.DescriptionOfContainer,
        //            isEnableClassification = node.isEnableClassification,
        //            EMailToRecordOwner = node.EMailToRecordOwner,
        //            EnableRelatedRecords = node.EnableRelatedRecords,
        //            SettingTime = 0,
        //            NodeInfo = SerializerHelper.SerializeByDataContractSerializer(node)
        //        };
        //        context.RMSharePointSettings.Add(settings);
        //        context.SaveChanges();
        //        spSetting = context.RMSharePointSettings.AsQueryable().Where(s => s.ScopeId == settings.ScopeId && !s.IsRemoved).FirstOrDefault();
        //        RecordOwnerDao.AddRecordOwners(spSetting.Id, node.RecordOwner);
        //    }
        //    //remove all custom setting node
        //    DeleteCustomSettingUsingExistColumn(new Guid(node.SPObjectId));
        //}

        public bool IsUsingExistingColumnByGroupIds(List<Guid> ids)
        {
            bool result = false;
            using (var context = GetNewContext())
            {
                RMSharePointSetting spSetting = context.RMSharePointSettings.AsQueryable().Where(s => ids.Contains(s.ScopeId) && s.IsUsingExistColumnName && !s.IsRemoved).FirstOrDefault();
                if (spSetting != null)
                {
                    result = true;
                }
                return result;
            }
        }

        public RMSharePointSetting GetSettingInfoByAgentGroupId(string id)
        {
            using (var context = GetNewContext())
            {
                RMSharePointSetting spSetting = context.RMSharePointSettings.AsQueryable().Where(s => s.ScopeId.Equals(new Guid(id)) && !s.IsRemoved).FirstOrDefault();
                return spSetting;
            }
        }

        public List<GCommon.Contract.StorageOptimization.Object.UserInfo> GetReocrdOwnersBySettingId(int settingId)
        {
            using (var context = GetNewContext())
            {
                var owners = context.RecordOwner.Where(item => item.SPSettingId == settingId && item.SettingType == 0).ToList();
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

        public List<RecordOwnerGroupDto> GetRecordOwners(List<Guid> groupIds, List<Guid> siteIds)
        {
            var results = new List<RecordOwnerGroupDto>();
            using (var context = GetNewContext())
            {
                var settings = context.RMSharePointSettings.AsQueryable()
                .Where(s => (siteIds.Contains(s.SiteId) || groupIds.Contains(s.ScopeId)) && !s.IsRemoved)
                .Select(s => new RecordOwnerGroupDto()
                {
                    SPSettingId = s.Id,
                    ScopeId = s.ScopeId,
                    SiteGroupId = s.SiteGroupId,
                    SiteId = s.SiteId,
                    WebId = s.WebId,
                    ListId = s.ListId,
                    FolderId = s.FolderId,
                    MailToOwner = s.EMailToRecordOwner
                }).ToDictionary(s => s.SPSettingId);

                if (settings.Count > 0)
                {
                    var settingIds = settings.Keys;
                    var ownerGroups = context.RecordOwner.AsQueryable()
                        .Where(o => settingIds.Contains(o.SPSettingId) && o.SettingType == 0)
                        .GroupBy(o => o.SPSettingId).ToList();
                    foreach (var setting in settings)
                    {
                        try
                        {
                            var groupDto = ownerGroups.Where(t => t.Key == setting.Key).FirstOrDefault();
                            if (groupDto != null)
                            {
                                setting.Value.AddOwnerRange(groupDto.Select(o =>
                                {
                                    var objectId = o.ObjectId;
                                    var owner = AccountDao.Find(s => s.UserId == objectId);
                                    if (owner == null)
                                    {
                                        return null;
                                    }
                                    return new RecordOwnerDto()
                                    {
                                        LnkId = owner.Id,
                                        ObjectId = o.ObjectId,
                                        DisplayName = owner.DisplayName,
                                        UserPrincipalName = owner.UserPrincipalName,
                                        Type = owner.ObjectType == Contract.RMWeb.RMActiveDirectoryObjectType.Group ? AccountType.Group : AccountType.User,
                                    };
                                }));
                            }
                            results.Add(setting.Value);
                        }
                        catch (Exception ex)
                        {
                            logger.Warn("get record owner {0} error:{1}", setting.Value.ScopeId, ex.ToString());
                        }
                    }

                }
            }
            return results;
        }

        public List<RecordOwnerGroupDto> GetRecordOwnersForEXO(List<Guid> parentIds, List<Guid> currentNodeIds)
        {
            var results = new List<RecordOwnerGroupDto>();
            using (var context = GetNewContext())
            {
                var settings = context.RMExchangeOnlineSettings.AsQueryable()
                .Where(s => (currentNodeIds.Contains(s.ScopeId) || parentIds.Contains(s.ScopeId)) && !s.IsRemoved)
                .Select(s => new RecordOwnerGroupDto()
                {
                    SPSettingId = s.Id,
                    ScopeId = s.ScopeId,
                    SiteGroupId = s.GroupId,
                    SiteId = Guid.Empty,
                    WebId = Guid.Empty,
                    ListId = Guid.Empty,
                    FolderId = Guid.Empty,
                    MailBoxId = s.MailBoxId,
                    MailToOwner = s.EMailToRecordOwner
                }).ToDictionary(s => s.SPSettingId);

                if (settings.Count > 0)
                {
                    var settingIds = settings.Keys;
                    var ownerGroups = context.RecordOwner.AsQueryable()
                        .Where(o => settingIds.Contains(o.SPSettingId) && o.SettingType == 1)
                        .GroupBy(o => o.SPSettingId).ToList();
                    foreach (var setting in settings)
                    {
                        try
                        {
                            var groupDto = ownerGroups.Where(t => t.Key == setting.Key).FirstOrDefault();
                            if (groupDto != null)
                            {
                                setting.Value.AddOwnerRange(groupDto.Select(o =>
                                {
                                    var objectId = o.ObjectId;
                                    var owner = AccountDao.Find(s => s.UserId == objectId);
                                    if (owner == null)
                                    {
                                        return null;
                                    }
                                    return new RecordOwnerDto()
                                    {
                                        LnkId = owner.Id,
                                        ObjectId = o.ObjectId,
                                        DisplayName = owner.DisplayName,
                                        UserPrincipalName = owner.UserPrincipalName,
                                        Type = owner.ObjectType == Contract.RMWeb.RMActiveDirectoryObjectType.Group ? AccountType.Group : AccountType.User,
                                    };
                                }));
                            }
                            results.Add(setting.Value);
                        }
                        catch (Exception ex)
                        {
                            logger.Warn("get record owner {0} error:{1}", setting.Value.ScopeId, ex.ToString());
                        }
                    }
                }
            }
            return results;
        }

        public List<RecordOwnerGroupDto> GetRecordOwnersForSPLocal(List<Guid> groupIds, List<Guid> siteIds)
        {
            var results = new List<RecordOwnerGroupDto>();
            using (var context = GetNewContext())
            {
                var settings = context.RMSharePointOnPremiseSettings.AsQueryable()
                .Where(s => (siteIds.Contains(s.SiteId) || groupIds.Contains(s.ScopeId)) && !s.IsRemoved)
                .Select(s => new RecordOwnerGroupDto()
                {
                    SPSettingId = s.Id,
                    ScopeId = s.ScopeId,
                    SiteGroupId = s.SiteGroupId,
                    SiteId = s.SiteId,
                    WebId = s.WebId,
                    ListId = s.ListId,
                    FolderId = s.FolderId,
                    MailToOwner = s.EMailToRecordOwner
                }).ToDictionary(s => s.SPSettingId);

                if (settings.Count > 0)
                {
                    var settingIds = settings.Keys;
                    var ownerGroups = context.RecordOwner.AsQueryable()
                        .Where(o => settingIds.Contains(o.SPSettingId) && o.SettingType == (int)RecordOwnerSettingType.SharePointOnPremise)
                        .GroupBy(o => o.SPSettingId).ToList();
                    foreach (var setting in settings)
                    {
                        try
                        {
                            var groupDto = ownerGroups.Where(t => t.Key == setting.Key).FirstOrDefault();
                            if (groupDto != null)
                            {
                                setting.Value.AddOwnerRange(groupDto.Select(o =>
                                {
                                    var objectId = o.ObjectId;
                                    var owner = AccountDao.Find(s => s.UserId == objectId);
                                    if (owner == null)
                                    {
                                        return null;
                                    }
                                    return new RecordOwnerDto()
                                    {
                                        LnkId = owner.Id,
                                        ObjectId = o.ObjectId,
                                        DisplayName = owner.DisplayName,
                                        UserPrincipalName = owner.UserPrincipalName,
                                        Type = owner.ObjectType == Contract.RMWeb.RMActiveDirectoryObjectType.Group ? AccountType.Group : AccountType.User,
                                    };
                                }));
                            }
                            results.Add(setting.Value);
                        }
                        catch (Exception ex)
                        {
                            logger.Warn("get record owner {0} error:{1}", setting.Value.ScopeId, ex.ToString());
                        }
                    }

                }
            }
            return results;
        }

        public List<RecordOwnerGroupDto> GetRecordOwnersForOneDrive(List<Guid> groupIds, List<Guid> siteIds)
        {
            var results = new List<RecordOwnerGroupDto>();
            using (var context = GetNewContext())
            {
                var settings = context.RMOneDriveSettings.AsQueryable()
                .Where(s => (siteIds.Contains(s.SiteId) || groupIds.Contains(s.ScopeId)) && !s.IsRemoved)
                .Select(s => new RecordOwnerGroupDto()
                {
                    SPSettingId = s.Id,
                    ScopeId = s.ScopeId,
                    SiteGroupId = s.SiteGroupId,
                    SiteId = s.SiteId,
                    WebId = s.WebId,
                    ListId = s.ListId,
                    FolderId = s.FolderId,
                    MailToOwner = s.EMailToRecordOwner
                }).ToDictionary(s => s.SPSettingId);

                if (settings.Count > 0)
                {
                    var settingIds = settings.Keys;
                    var ownerGroups = context.RecordOwner.AsQueryable()
                        .Where(o => settingIds.Contains(o.SPSettingId) && o.SettingType == (int)RecordOwnerSettingType.OneDrive)
                        .GroupBy(o => o.SPSettingId).ToList();
                    foreach (var setting in settings)
                    {
                        try
                        {
                            var groupDto = ownerGroups.Where(t => t.Key == setting.Key).FirstOrDefault();
                            if (groupDto != null)
                            {
                                setting.Value.AddOwnerRange(groupDto.Select(o =>
                                {
                                    var objectId = o.ObjectId;
                                    var owner = AccountDao.Find(s => s.UserId == objectId);
                                    if (owner == null)
                                    {
                                        return null;
                                    }
                                    return new RecordOwnerDto()
                                    {
                                        LnkId = owner.Id,
                                        ObjectId = o.ObjectId,
                                        DisplayName = owner.DisplayName,
                                        UserPrincipalName = owner.UserPrincipalName,
                                        Type = owner.ObjectType == Contract.RMWeb.RMActiveDirectoryObjectType.Group ? AccountType.Group : AccountType.User,
                                    };
                                }));
                            }
                            results.Add(setting.Value);
                        }
                        catch (Exception ex)
                        {
                            logger.Warn("get record owner {0} error:{1}", setting.Value.ScopeId, ex.ToString());
                        }
                    }

                }
            }
            return results;
        }
        public void UpdateRecordOwnerUserPrincipalName(RecordOwnerDto owner)
        {
            using (var context = GetNewContext())
            {
                var entities = context.RecordOwner.AsQueryable().Where(o => o.ObjectId == owner.ObjectId);
                foreach (var entity in entities)
                {
                    //entity.UserPrincipalName = owner.UserPrincipalName;
                }
                RecordOwnerDao.BatchUpdate(entities.ToList());
            }
        }

        public async Task<bool> CleanSettingJobTimeAsync(RMSPTreeNode node)
        {
            try
            {
                using (var context = GetNewContext())
                {
                    var groupId = Guid.Empty;
                    var scopeId = new Guid(node.SPObjectId);
                    if (node.Level == (int)NodeLevel.WebApplication)
                    {
                        groupId = scopeId;
                    }
                    else
                    {
                        groupId = GetGroupIdByScopeId(scopeId, context);
                    }
                    var setting = context.RMSharePointSettings.AsQueryable().Where(s => s.SiteGroupId == groupId && s.ScopeId.Equals(new Guid(node.SPObjectId)) && !s.IsRemoved).FirstOrDefault();
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
            catch
            {
                //to do log 
                return false;
            }
        }

        public async Task SetSettingJobTimeWithGroupIdAsync(Guid groupId, Guid scopeId, bool isFailedConfigColumn, bool isFailedConfigProperty)
        {
            try
            {
                using (var context = GetNewContext())
                {
                    var setting = context.RMSharePointSettings.AsQueryable().Where(s => s.SiteGroupId == groupId && s.ScopeId.Equals(scopeId) && !s.IsRemoved).FirstOrDefault();
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
        

        //scopeid可能重复，需要使用scope id+siteid去更新setting
        public async Task SetSettingJobTimeAsync(Guid scopeId, bool isFailedColumn, bool isFailedProperty)
        {
            try
            {
                using (var context = GetNewContext())
                {
                    var groupId = Guid.Empty;
                    var webApp = RMRemoteNodeDao.GetWebApplicationById(scopeId.ToString());
                    if (webApp != null)
                    {
                        groupId = scopeId;
                    }
                    else
                    {
                        groupId = GetGroupIdByScopeId(scopeId, context);
                    }
                    var setting = context.RMSharePointSettings.AsQueryable().Where(s => s.SiteGroupId == groupId && s.ScopeId.Equals(scopeId) && !s.IsRemoved).FirstOrDefault();
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
                logger.Error($"error occured when SetSettingJobTimeAsync,error:{e}");

            }
        }

        public async Task SetSettingJobTimeAsync(Guid scopeId, Guid siteId, bool isFailedColumn, bool isFailedProperty)
        {
            try
            {
                using (var context = GetNewContext())
                {
                    var groupId = Guid.Empty;
                    if (siteId == Guid.Empty)
                    {
                        groupId = scopeId;
                    }
                    else
                    {
                        groupId = GetGroupIdBySiteId(siteId);
                    }
                    var setting = context.RMSharePointSettings.AsQueryable().Where(s => s.SiteGroupId == groupId && s.ScopeId.Equals(scopeId) && s.SiteId.Equals(siteId) && !s.IsRemoved).FirstOrDefault();
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
                logger.Error($"An error occurred while set setting job time by node: [{scopeId}], site id: [{siteId}]. Error:[{e.ToString()}]");
            }
        }

        private Guid GetGroupIdBySiteId(Guid siteId)
        {
            var site = RMRemoteNodeDao.GetRemoteSiteCollectionById(siteId.ToString());
            return site != null ? new Guid(site.parentId) : Guid.Empty;
        }

        private Guid GetGroupIdByScopeId(Guid scopeId, RMDbContext context)
        {
            var setting = context.RMSharePointSettings.Where(s => s.ScopeId == scopeId).FirstOrDefault();
            if (setting != null)
            {
                var siteId = setting.SiteId;
                var site = RMRemoteNodeDao.GetRemoteSiteCollectionById(siteId.ToString());
                return site != null ? new Guid(site.parentId) : Guid.Empty;
            }
            return Guid.Empty;
        }

        public List<RMSharePointSetting> LoadRunJobSetting()
        {
            using (var context = GetNewContext())
            {
                return context.RMSharePointSettings.AsQueryable().Where(s => s.SettingTime.Equals(0) && s.NodeInfo != null && !s.IsRemoved).ToList();
            }
        }
        public List<RMSharePointSetting> LoadAllSetting()
        {
            using (var context = GetNewContext())
            {
                context.Database.CommandTimeout = 600;
                return context.RMSharePointSettings.AsQueryable().Where(s => s.NodeInfo != null && !s.IsRemoved).ToList();
            }
        }
        
        public IEnumerable<RMSharePointSetting> LoadSyncDataSettings(int batchSize = 100)
        {
            return LoadAllSettingStream(batchSize, GetNeededSyncNodesCondition());
        }

        public List<RMSharePointSetting> LoadExcludeSiteCollectionSetting()
        {
            using (var context = GetNewContext())
            {
                //return context.RMSharePointSettings.AsQueryable().Where(s => !s.SettingTime.Equals(0) && !s.NodeInfo.Equals(null) && s.ScopeId.Equals(s.SiteId)).ToList();
                return context.RMSharePointSettings.AsQueryable().Where(s => s.NodeInfo != null && s.ScopeId.Equals(s.SiteId)).ToList();
            }
        }

        public List<RMSharePointSetting> GetAllSettingsForLevel(RMSPTreeNode current, NodeLevel level)
        {
            Expression<Func<RMSharePointSetting, bool>> lambda = null;
            switch (level)
            {
                case NodeLevel.WebApplication:
                    lambda = s => s.ScopeId == s.SiteGroupId && !s.IsRemoved;
                    break;
                case NodeLevel.SiteCollection:
                    lambda = s => s.ScopeId == s.SiteId && s.SiteGroupId.ToString() == current.SPObjectId && !s.IsRemoved;
                    break;
                case NodeLevel.Site:
                    lambda = s => s.FullPath.Contains(current.Parent.FullPath) && s.ScopeId == s.WebId && !s.FullPath.Replace(current.Parent.FullPath + "/", "").Contains("/") && !s.IsRemoved;//防止查出所有层的sub site
                    break;
                case NodeLevel.List:
                case NodeLevel.Library:
                    lambda = s => s.FullPath.Contains(current.Parent.FullPath) && s.ScopeId == s.ListId && s.WebId == new Guid(current.Parent.SPObjectId) && !s.IsRemoved;//parent is site level
                    break;
                case NodeLevel.Folder:
                    lambda = s => s.FullPath.Contains(current.Parent.FullPath) && s.ScopeId == s.FolderId && !s.FullPath.Replace(current.Parent.FullPath + "/", "").Contains("/") && !s.IsRemoved;//parent is root folder or folder
                    break;
                default:
                    return null;
            }
            using (var context = GetNewContext())
            {
                return context.RMSharePointSettings.AsQueryable().Where(lambda).ToList();
            }
        }

        #region re sps
        public List<RMSharePointSetting> GetDescendantsDisableNodes(RMSPTreeNode node)
        {
            Expression<Func<RMSharePointSetting, bool>> lambda = null;
            var scopeId = new Guid(node.SPObjectId);
            var groupId = node.SiteGroupId;
            using (var context0 = RMDBContextManager.GetNewDBContext())
            {
                switch ((NodeLevel)node.Level)
                {
                    case NodeLevel.WebApplication:
                        lambda = s => s.SiteGroupId == scopeId;
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
                        return new List<RMSharePointSetting>();
                }
            }
            using (var context = RMDBContextManager.GetNewDBContext())
            {
                return context.RMSharePointSettings.Where(lambda).Where(s => s.SiteGroupId == groupId && s.ScopeId != node.SettingScopeId && s.EnableRecordManagement == (int)EnableRecordManagementSetting.Disable).ToList();
            }
        }

        public List<RMSharePointSetting> GetDescendantsFolderBreakNodes(RMSPTreeNode node)
        {
            Expression<Func<RMSharePointSetting, bool>> lambda = null;
            var scopeId = new Guid(node.SPObjectId);
            var groupId = node.SiteGroupId;
            using (var context0 = RMDBContextManager.GetNewDBContext())
            {
                switch ((NodeLevel)node.Level)
                {
                    case NodeLevel.WebApplication:
                        lambda = s => s.SiteGroupId == scopeId;
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
                        lambda = s => s.FolderId == scopeId;
                        break;
                }
            }
            using (var context = RMDBContextManager.GetNewDBContext())
            {
                return context.RMSharePointSettings.Where(lambda).Where(s => s.SiteGroupId == groupId && s.ScopeId == s.FolderId && !s.IsRemoved).ToList();
            }
        }
        public RMSharePointSetting GetParentLibraryCustomSetting(Guid listId)
        {
            using (var context = RMDBContextManager.GetNewDBContext())
            {
                var groupId = GetGroupIdByScopeId(listId, context);
                return context.RMSharePointSettings.Where(s => s.SiteGroupId == groupId && s.ScopeId == listId && s.ScopeId == s.ListId && !s.IsRemoved).FirstOrDefault();
            }
        }

        public List<RMSharePointSetting> GetFolderSettingUnderList(Guid listId, Guid siteId)
        {
            using (var context = RMDBContextManager.GetNewDBContext())
            {
                var groupId = GetGroupIdBySiteId(siteId);
                return context.RMSharePointSettings.Where(s => s.SiteGroupId == groupId && s.SiteId == siteId && s.ListId == listId && s.ScopeId == s.FolderId && !s.IsRemoved).ToList();
            }
        }

        public RMSharePointSetting GetSettingInfoByScope(Guid groupId, Guid siteId, Guid scopeId)
        {
            using (var ctx = GetNewContext())
            {
                return ctx.RMSharePointSettings.Where(s => s.SiteGroupId == groupId && s.SiteId == siteId && s.ScopeId == scopeId).FirstOrDefault();
            }
        }

        public List<RMSharePointSetting> LoadSPSettingsUnderSite(Guid siteId)
        {
            using (var context = GetNewContext())
            {
                var groupId = GetGroupIdBySiteId(siteId);
                return context.RMSharePointSettings.AsQueryable().Where(s => s.SiteGroupId == groupId && s.SiteId == siteId && !s.IsRemoved).ToList();
            }
        }

        public RMSharePointSetting LoadSPSiteSettingEnableManualApprovalFirst()
        {
            using var context = GetNewContext();
            return context.RMSharePointSettings.AsQueryable().Where(s => s.SiteId != Guid.Empty && s.EnableRecordManagement == (int)EnableRecordManagementSetting.Enable && !s.IsRemoved).FirstOrDefault();
        }

        public bool GetSettingEnableInfoByScope(Guid groupId, Guid siteId, Guid scopeId)
        {
            using (var ctx = GetNewContext())
            {
                var setting = ctx.RMSharePointSettings.Where(s => s.SiteGroupId == groupId && s.SiteId == siteId && s.ScopeId == scopeId && !s.IsRemoved).FirstOrDefault();
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

        public async Task AddOrUpdateGlobalSettingUsingExistColumnAsync(RMSPTreeNode node, bool isNewEditd = false)
        {
            EnsureTermName(node);
            using (var context = GetNewContext())
            {
                RMSharePointSetting spSetting = context.RMSharePointSettings.AsQueryable().Where(s => s.ScopeId.Equals(new Guid(node.SPObjectId))).FirstOrDefault();
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
                    spSetting.SiteGroupId = new Guid(node.Id);
                    //spSetting.IdPath = node.ProfileId;
                    spSetting.IsSyncData = node.IsSyncData;
                    spSetting.IsKeepSharePointDefaultValue = node.IsKeepSharePointDefaultValue;
                    spSetting.SetTermForEmptyDefaultValue = node.SetTermForEmptyDefaultValue;
                    if (isNewEditd)
                    {
                        spSetting.IsNewEdited = true;
                    }
                    await this.UpdateAsync(spSetting);
                    await RecordOwnerDao.UpdateRecordOwnersAsync(spSetting.Id, node.RecordOwner);
                }
                else
                {
                    RMSharePointSetting settings = new RMSharePointSetting()
                    {
                        ExistColumnName = node.ExistColumnName,
                        IsUsingExistColumnName = node.IsUsingExistColumnName,
                        SetDocLevelTermForExistColumn = node.SetDocLevelTermForExistColumn,
                        FullPath = node.FullPath,
                        ScopeId = new Guid(node.SPObjectId),
                        FieldId = Guid.Empty,
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
                        IsInheritParentTerm = node.IsInheritParentTerm,
                        isEnableClassification = node.isEnableClassification,
                        EnableRecordManagement = node.EnableRecordManagement,
                        EMailToRecordOwner = node.EMailToRecordOwner,
                        EnableRelatedRecords = node.EnableRelatedRecords,
                        IsShowUniqueId = node.IsShowUniqueId,
                        IsKeepSharePointDefaultValue = node.IsKeepSharePointDefaultValue,
                        SetTermForEmptyDefaultValue = node.SetTermForEmptyDefaultValue,
                        SettingTime = 0,
                        NodeInfo = SerializerHelper.SerializeByDataContractSerializer(node),
                        //IdPath = node.ProfileId,
                        IsSyncData = node.IsSyncData
                    };
                    if (isNewEditd)
                    {
                        settings.IsNewEdited = true;
                    }
                    context.RMSharePointSettings.Add(settings);
                    context.SaveChanges();
                    spSetting = context.RMSharePointSettings.AsQueryable().Where(s => s.ScopeId == settings.ScopeId).FirstOrDefault();
                    if (spSetting != null)
                    {
                        await RecordOwnerDao.AddRecordOwnersAsync(spSetting.Id, node.RecordOwner);
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

        public List<RMSharePointSetting> LoadShowUniqueIdSetting()
        {
            using (var ctx = GetNewContext())
            {
                return ctx.RMSharePointSettings.Where(s => s.EnableRecordManagement == (int)EnableRecordManagementSetting.Enable && s.IsShowUniqueId == true && s.ScopeId == s.SiteGroupId && !s.IsRemoved).ToList();
            }
        }
        public List<RMSharePointSetting> LoadGroupSetting(bool isRecheckRule = true)
        {
            using (var ctx = GetNewContext())
            {
                return ctx.RMSharePointSettings.Where(s => (s.EnableRecordManagement == (int)EnableRecordManagementSetting.Enable || !isRecheckRule) && s.ScopeId == s.SiteGroupId && !s.IsRemoved).ToList();
            }
        }
        public bool ExistShowUniqueIdSetting()
        {
            using (var ctx = GetNewContext())
            {
                return ctx.RMSharePointSettings.Any(s => s.EnableRecordManagement == (int)EnableRecordManagementSetting.Enable && s.IsShowUniqueId == true && s.ScopeId == s.SiteGroupId && !s.IsRemoved);
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
        public string GetNodeFullPath(RMSPTreeNode node)
        {
            if (node.Level == (int)NodeLevel.SiteCollection || node.Level == (int)NodeLevel.WebApplication)
            {
                return node.FullPath;
            }
            return WebUtil.MakeFullUrl(node.GetSiteCollectionNode().FullPath, node.FullPath);
        }

        public void SetCustomSettingUsingExistColumnByGroup(RMSPTreeNode gNode)
        {
            using (var context = GetNewContext())
            {
                var entities = context.RMSharePointSettings.AsQueryable().Where(s => s.SiteGroupId == new Guid(gNode.SPObjectId) && s.SiteId != Guid.Empty).ToList();

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
        #endregion

        #region check job skip
        public async Task UpdateRunningJobStartStatusAsync(List<int> spIds, string startJobId)
        {
            using (var context = GetNewContext())
            {
                var dbSettings = context.RMSharePointSettings.AsQueryable().Where(s => spIds.Contains(s.Id)).ToList();
                foreach (var setting in dbSettings)
                {
                    setting.IsNewEdited = false;
                    setting.IsRunning = true;
                    setting.SharePointSettingJobId = startJobId;
                    await UpdateAsync(setting);
                }
            }
        }

        public async Task UpdateRunningJobFinishStatusAsync(List<int> spIds)
        {
            using (var context = GetNewContext())
            {
                var dbSettings = context.RMSharePointSettings.AsQueryable().Where(s => spIds.Contains(s.Id)).ToList();
                foreach (var setting in dbSettings)
                {
                    setting.IsNewEdited = false;
                    setting.IsRunning = false;
                    setting.SharePointSettingJobId = "";
                    await UpdateAsync(setting);
                }
            }
        }
        public bool CheckJobIsSkip()
        {
            using var context = GetNewContext();
            return context.RMSharePointSettings.AsQueryable().Any(s => s.SettingTime.Equals(0) && s.IsRunning && s.IsNewEdited);
        }
        public void FlagCustomSettingNewColumn(Guid siteGroupId)
        {
            using var context = GetNewContext();
            var entities = context.RMSharePointSettings.AsQueryable().Where(s => s.SiteGroupId == siteGroupId && s.SiteId != Guid.Empty).ToList();
            foreach (var entity in entities)
            {
                entity.IsUsingExistColumnName = false;
                entity.IsNewEdited = true;
                entity.SettingTime = 0;
            }

            this.BatchUpdate(entities);
        }


        #endregion
        public Dictionary<Guid, int> GetDisableDocClassification()
        {
            using (var ctx = GetNewContext())
            {
                return ctx.RMSharePointSettings.Where(s => s.FolderId == Guid.Empty && s.EnableRecordManagement == 2).Select(s => new { ScopeId = s.ScopeId, DocSeting = s.EnableRecordManagement }).ToDictionary(s => s.ScopeId, o => o.DocSeting);
            }
        }
        public void RemoveDescendantsSetting(RMSPTreeNode node, string profileIdPath)
        {
            if (node.EnableRecordManagement != (int)EnableRecordManagementSetting.Enable)
            {
                ScheduleService.DeleteSchedules(ScheduleType.DisposalSchedule, profileIdPath);
                var deleteDescendantsSql = "Delete From {0}.[RMSharePointSettings] Where {1} = @scopeId And ScopeId <> @scopeId";
                //var deleteScheduleSql = "Delete From {0}.[RMSchedules] Where Id In (SELECT {1} From {0}.[RMSharePointSettings] Where {2} = @scopeId)";
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
        public RMSharePointSetting GetParentNode(Expression<Func<RMSharePointSetting, bool>> whereLambda)
        {
            RMSharePointSetting result = new RMSharePointSetting();
            using (var context = RMDBContextManager.GetNewDBContext())
            {
                result = context.RMSharePointSettings.AsQueryable().Where(whereLambda).FirstOrDefault();
            }
            return result;
        }
        public List<RMSharePointSetting> GetAllGroupSettings()
        {

            using (var context = RMDBContextManager.GetNewDBContext())
            {
                return context.RMSharePointSettings.AsQueryable().Where(g => g.ScopeId == g.SiteGroupId && !g.IsRemoved).ToList();
            }

        }
        public bool ChickGroupSettingExist(List<string> groupIds)
        {
            using (var context = RMDBContextManager.GetNewDBContext())
            {
                return context.RMSharePointSettings.AsQueryable()
                    .Any(s => s.NodeInfo != null && s.SiteId == Guid.Empty && !s.IsRemoved && groupIds.Contains(s.ScopeId.ToString()));
            }
        }

        public RMSharePointSetting LoadSharePointSettingForImportSetting(Guid siteId, Guid scopeId)
        {
            using (var context = RMDBContextManager.GetNewDBContext())
            {
                if (siteId == Guid.Empty)
                {
                    //查group setting
                    return context.RMSharePointSettings.AsQueryable().Where(s => s.ScopeId == scopeId).FirstOrDefault();
                }
                var groupId = GetGroupIdBySiteId(siteId);
                return context.RMSharePointSettings.AsQueryable().Where(s => s.SiteGroupId == groupId && s.ScopeId == scopeId && s.SiteId == siteId).FirstOrDefault();
            }
        }
        public RMSharePointSetting LoadSharePointSetting(string fullPath)
        {
            using (var context = RMDBContextManager.GetNewDBContext())
            {
                return context.RMSharePointSettings.AsQueryable().Where(s => s.FullPath == fullPath && !s.IsRemoved).FirstOrDefault();
            }
        }
        public RMSharePointSetting LoadContainerSharePointSettingByContainerName(string containerName)
        {
            using (var context = RMDBContextManager.GetNewDBContext())
            {
                return context.RMSharePointSettings.AsQueryable().Where(s => !string.IsNullOrEmpty(s.NodeInfo) && s.SiteId == Guid.Empty && !s.IsRemoved).AsEnumerable().Where(s => SerializerHelper.DeserializeByDataContractSerializer<RMSPTreeNode>(s.NodeInfo).Name.Equals(containerName)).FirstOrDefault();
            }
        }

        public List<RMSharePointSetting> GetAllSettingsBySiteGroupIds(List<Guid> siteGroupIds)
        {
            using var context = RMDBContextManager.GetNewDBContext();
            return [.. context.RMSharePointSettings.AsQueryable().Where(s => siteGroupIds.Contains(s.SiteGroupId) && !s.IsRemoved)];
        }

        public int GetSettingsCountBySiteGroupIds(List<Guid> siteGroupIds)
        {
            using var context = RMDBContextManager.GetNewDBContext();
            return context.RMSharePointSettings.AsQueryable().Count(s => siteGroupIds.Contains(s.SiteGroupId) && !s.IsRemoved);
        }

        public List<RMSharePointSetting> GetAllSettingsByScopeIds(List<Guid> scopeIds)
        {
            using var context = RMDBContextManager.GetNewDBContext();
            return context.RMSharePointSettings.AsQueryable().Where(s => scopeIds.Contains(s.ScopeId) && !s.IsRemoved).ToList();
        }

        // container level setting: container => site collection => site => list
        public RMSharePointSetting LoadClosestContainerSetting(RMSPTreeNode treeNode, Guid containerId, Guid siteId)
        {
            RMSharePointSetting spSetting = null;

            if (treeNode == null)
            {
                return spSetting;
            }

            if (treeNode.Level == (int)NodeLevel.WebApplication) siteId = Guid.Empty; // clear siteId for container node

            if (treeNode.Level == (int)NodeLevel.WebApplication 
                || treeNode.Level == (int)NodeLevel.SiteCollection 
                || treeNode.Level == (int)NodeLevel.Site 
                || treeNode.Level == (int)NodeLevel.List 
                || treeNode.Level == (int)NodeLevel.Library)
            {
                using var context = GetNewContext();
                spSetting = context.RMSharePointSettings.AsQueryable()
                    .FirstOrDefault(s => s.ScopeId == new Guid(treeNode.SPObjectId) && s.SiteId == siteId && !s.IsRemoved && s.SiteGroupId == containerId);
            }

            spSetting ??= LoadClosestContainerSetting(treeNode.Parent, containerId, siteId);
            return spSetting;
        }

        private IEnumerable<RMSharePointSetting> LoadAllSettingStream(int batchSize, Expression<Func<RMSharePointSetting, bool>> predicate = null)
        {
            using var context = GetNewContext();
            context.Database.CommandTimeout = 600;

            var baseQuery = context.RMSharePointSettings.AsQueryable().Where(s => s.NodeInfo != null && !s.IsRemoved);

            if (predicate != null)
            {
                baseQuery = baseQuery.Where(predicate);
            }

            var orderedQuery = baseQuery.OrderBy(s => s.Id);
            int lastId = 0;
            while (true)
            {
                var batch = orderedQuery
                    .Where(s => s.Id > lastId)
                    .Take(batchSize)
                    .ToList();

                if (batch.Count == 0) yield break;

                foreach (var item in batch)
                    yield return item;

                lastId = batch.Last().Id;
            }
        }

        private Expression<Func<RMSharePointSetting, bool>> GetNeededSyncNodesCondition()
        {
            return s => s.IsSyncData &&
            (
                (s.SiteGroupId.Equals(s.ScopeId) && s.SiteGroupId != Guid.Empty)
                || (s.SiteId.Equals(s.ScopeId) && s.SiteId != Guid.Empty)
            );
        }

        public bool CheckHasInheritChanged(Guid groupId)
        {
            using var context = GetNewContext();
            return context.RMSharePointSettings.Any(s => s.SiteGroupId == groupId && s.IsChangedInheritOption && !s.IsRemoved);
        }

        public bool CheckHasInheritChanged(Guid groupId, Guid siteId)
        {
            using var context = GetNewContext();
            return context.RMSharePointSettings.Any(s => s.SiteGroupId == groupId && s.SiteId == siteId && s.IsChangedInheritOption && !s.IsRemoved);
        }


        public bool UpdateChangedInheritOptionFlag(Guid groupId, Guid siteId)
        {
            using var context = GetNewContext();
            var settings = context.RMSharePointSettings.Where(s => s.SiteGroupId == groupId && s.SiteId == siteId && s.IsChangedInheritOption && !s.IsRemoved).ToList();
            foreach (var setting in settings)
            {
                setting.IsChangedInheritOption = false;
            }
            return context.SaveChanges() > 0;
        }

        public int UpdateChangedInheritOptionFlag(Guid groupId)
        {
            using var context = GetNewContext();
            var settings = context.RMSharePointSettings.Where(s => s.SiteGroupId == groupId && s.IsChangedInheritOption && !s.IsRemoved).ToList();
            foreach (var setting in settings)
            {
                setting.IsChangedInheritOption = false;
            }
            return context.SaveChanges();
        }
    }
}
