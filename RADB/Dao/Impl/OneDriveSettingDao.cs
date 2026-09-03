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
    public class OneDriveSettingDao : BaseDao<RMOneDriveSetting>, IOneDriveSettingDao
    {
        private RALogger logger = RALogger.GetInstance(typeof(OneDriveSettingDao));
        public IScheduleService ScheduleService { get; set; }
        public IRecordOwnerDao RecordOwnerDao { get; set; }
        public IAccountDao AccountDao { get; set; }
        public IEXOSettingRuleDao RMEXOSettingRuleDao { get; set; }
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
        public async Task AddOrUpdateCustomSettingAsync(RMSPTreeNode node, Guid siteId)
        {
            EnsureTermName(node);
            using (var context = GetNewContext())
            {
                var groupId = Guid.Empty;
                if (node.Level == (int)NodeLevel.WebApplication)
                {
                    groupId = new Guid(node.SPObjectId);
                }
                else
                {
                    groupId = GetGroupIdBySiteId(siteId);
                }               
                var groupSetting = context.RMExchangeOnlineSettings.Where(o => o.GroupId == groupId && o.ScopeId == groupId).FirstOrDefault();
                var spSetting = context.RMOneDriveSettings.AsQueryable().Where(s => s.SiteGroupId == groupId && s.ScopeId.Equals(new Guid(node.SPObjectId)) && s.SiteId.Equals(siteId) && !s.IsRemoved).FirstOrDefault();
                if (spSetting == null)
                {
                    spSetting = context.RMOneDriveSettings.AsQueryable().Where(s => s.SiteGroupId == groupId && s.ScopeId.Equals(new Guid(node.SPObjectId)) && s.SiteId.Equals(Guid.Empty) && !s.IsRemoved).FirstOrDefault();
                }
                if (spSetting != null)
                {
                    spSetting.DefaultTermId = node.DefaultTermId;
                    spSetting.DefaultTermName = node.DefaultTermName;
                    spSetting.FullPath = node.FullPath;
                    spSetting.ScopeId = new Guid(node.SPObjectId);
                    spSetting.TermId = node.TermId;
                    spSetting.TermName = node.TermName;
                    spSetting.TermSetId = node.TermSetId;
                    spSetting.TermSetName = node.TermSetName;
                    spSetting.TermStoreId = node.TermStoreId;
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
                    spSetting.ApplyExistType = node.ApplyExistType;
                    spSetting.DeployTermMethod = (int)node.DeployTermMethod;
                    spSetting.AutoClassificationRules = node.AutoClassificationRules == null ?
                        null : SerializerHelper.SerializeByDataContractSerializer(node.AutoClassificationRules);
                    spSetting.RunAutoFullJob = node.RunAutoFullJob;
                    spSetting.AutoJobOption = (int)node.AutoJobOption;
                    spSetting.IncludeDeclaredRecords = node.IncludeDeclaredRecords;
                    spSetting.ApprovalType = (ApprovalType)node.ApprovalType;
                    spSetting.WorkflowReferenceId = node.WorkflowReferenceId;
                    spSetting.IsShowUniqueId = node.IsShowUniqueId;
                    spSetting.IsNullClassificationSetting = node.IsNullClassificationSetting;

                    spSetting.AITermUseType = node.AITermUseType;
                    spSetting.AIApprovalType = (ApprovalType)node.AIApprovalType;
                    spSetting.AISendEMail = node.AISendEMail;
                    spSetting.AIThenIsDefaultTermMethod = node.AIThenIsDefaultTermMethod;
                    spSetting.AIThenDefaultTermId = node.AIThenDefaultTermId;
                    spSetting.AIThenDefaultTermName = node.AIThenDefaultTermName;
                    ApplyCurrentValues(context, spSetting);
                    await RecordOwnerDao.UpdateRecordOwnersAsync(spSetting.Id, node.RecordOwner, RecordOwnerSettingType.OneDrive);
                    if (node.AIReviewers != null)
                    {
                        await RecordOwnerDao.UpdateRecordOwnersAsync(spSetting.Id, node.AIReviewers, RecordOwnerSettingType.AIOneDrive);
                    }
                }
                else
                {
                    if (groupSetting != null && groupSetting.IsNullClassificationSetting)
                    {
                        node.TermSetId = Guid.Empty;
                        node.TermId = Guid.Empty;
                        node.DefaultTermId = Guid.Empty;
                        node.TermSetName = string.Empty;
                    }
                    RMOneDriveSetting settings = new RMOneDriveSetting()
                    {
                        DefaultTermId = node.DefaultTermId,
                        DefaultTermName = node.DefaultTermName,
                        FullPath = node.FullPath,
                        ScopeId = new Guid(node.SPObjectId),
                        TermId = node.TermId,
                        TermName = node.TermName,
                        TermSetId = node.TermSetId,
                        TermSetName = node.TermSetName,
                        TermStoreId = node.TermStoreId,
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
                        ApplyExistType = node.ApplyExistType,
                        NodeInfo = SerializerHelper.SerializeByDataContractSerializer(node),
                        DeployTermMethod = (int)node.DeployTermMethod,
                        AutoClassificationRules = node.AutoClassificationRules == null ?
                            null : SerializerHelper.SerializeByDataContractSerializer(node.AutoClassificationRules),
                        RunAutoFullJob = node.RunAutoFullJob,
                        IncludeDeclaredRecords = node.IncludeDeclaredRecords,
                        AutoJobOption = (int)node.AutoJobOption,
                        ApprovalType = (ApprovalType)node.ApprovalType,
                        WorkflowReferenceId = node.WorkflowReferenceId,
                        IsShowUniqueId = node.IsShowUniqueId,
                        IsNullClassificationSetting = node.IsNullClassificationSetting,

                        AITermUseType = node.AITermUseType,
                        AIApprovalType = (ApprovalType)node.AIApprovalType,
                        AISendEMail = node.AISendEMail,
                        AIThenIsDefaultTermMethod = node.AIThenIsDefaultTermMethod,
                        AIThenDefaultTermId = node.AIThenDefaultTermId,
                        AIThenDefaultTermName = node.AIThenDefaultTermName,
                    };
                    context.RMOneDriveSettings.Add(settings);
                    context.SaveChanges();
                    await RemoveDeletedSettingAsync(context, settings);
                    spSetting = context.RMOneDriveSettings.AsQueryable().Where(s => s.SiteGroupId == groupId && s.ScopeId == settings.ScopeId && !s.IsRemoved).FirstOrDefault();
                    ArgumentNullException.ThrowIfNull(spSetting);
                    await RecordOwnerDao.AddRecordOwnersAsync(spSetting.Id, node.RecordOwner, RecordOwnerSettingType.OneDrive);
                    if (node.AIReviewers != null)
                    {
                        await RecordOwnerDao.AddRecordOwnersAsync(spSetting.Id, node.AIReviewers, RecordOwnerSettingType.AIOneDrive);
                    }
                }

                RMEXOSettingRuleDao.SaveOneDriveMappingRules(node);
                if (node.Level == (int)NodeLevel.WebApplication)
                {
                    if (node.IsNullClassificationSetting)
                    {
                        MarkAllSettingUnderGroupDeleted(groupId);
                    }
                    else
                    {
                        await DeleteNullClassificationSiteSettingAsync(groupId);
                    }
                }

                
            }
        }

        private async Task RemoveDeletedSettingAsync(RMDbContext context, RMOneDriveSetting setting)
        {
            var deletedSetting = context.RMOneDriveSettings.AsQueryable().Where(s => s.SiteGroupId == setting.SiteGroupId && s.ScopeId == setting.ScopeId && s.IsRemoved).FirstOrDefault();
            if (deletedSetting != null)
            {
                context.RMOneDriveSettings.Remove(deletedSetting);
                await RecordOwnerDao.BatchDeleteAsync(o => o.SPSettingId == deletedSetting.Id);
                context.SaveChanges();
            }
        }
        private void MarkAllSettingUnderGroupDeleted(Guid groupId)
        {
            using (var context = GetNewContext())
            {
                var settings = context.RMOneDriveSettings.AsQueryable().Where(s => s.SiteGroupId == groupId && s.ScopeId != groupId && !s.IsRemoved && s.EnableRecordManagement == (int)EnableRecordManagementSetting.Enable).ToList();
                var needProcessSettings = settings.Where(s => !(s.SiteId == s.ScopeId && s.IsNullClassificationSetting)).ToList();
                if (needProcessSettings != null && needProcessSettings.Count > 0)
                {
                    needProcessSettings.ForEach(s =>
                    {
                        s.IsRemoved = true;
                    });
                    BatchUpdate(context, needProcessSettings);
                }
            }
        }

        private async Task DeleteNullClassificationSiteSettingAsync(Guid groupId)
        {
            using (var context = GetNewContext())
            {
                var siteSettings = context.RMOneDriveSettings.AsQueryable().Where(s => s.SiteGroupId == groupId && s.ScopeId != groupId && s.IsNullClassificationSetting).ToList();
                if (siteSettings != null && siteSettings.Count > 0)
                {
                    var enableRMNodes = siteSettings.Where(s => s.EnableRecordManagement == (int)EnableRecordManagementSetting.Enable).ToList();
                    if (enableRMNodes != null && enableRMNodes.Count > 0)
                    {
                        context.RMOneDriveSettings.RemoveRange(enableRMNodes);
                    }
                    var disableRMNodes = siteSettings.Where(s => s.EnableRecordManagement != (int)EnableRecordManagementSetting.Enable).ToList();
                    if (disableRMNodes != null && disableRMNodes.Count > 0)
                    {
                        disableRMNodes.ForEach(s =>
                        {
                            s.IsNullClassificationSetting = false;
                        });
                        BatchUpdate(context, disableRMNodes);
                    }

                    var ids = siteSettings.Select(s => s.Id).ToList();
                    await RecordOwnerDao.BatchDeleteAsync(o => ids.Contains(o.SPSettingId));
                    context.SaveChanges();
                }
            }
        }

        public async Task AddOrUpdateGlobalSettingAsync(RMSPTreeNode node)
        {
            EnsureTermName(node);
            using (var context = GetNewContext())
            {
                RMOneDriveSetting spSetting = context.RMOneDriveSettings.AsQueryable().Where(s => s.ScopeId.Equals(new Guid(node.SPObjectId))).FirstOrDefault();
                if (spSetting != null)
                {
                    spSetting.DefaultTermId = node.DefaultTermId;
                    spSetting.DefaultTermName = node.DefaultTermName;
                    spSetting.FullPath = node.FullPath;
                    spSetting.ScopeId = new Guid(node.SPObjectId);
                    spSetting.TermId = node.TermId;
                    spSetting.TermName = node.TermName;
                    spSetting.TermSetId = node.TermSetId;
                    spSetting.TermSetName = node.TermSetName;
                    spSetting.EnableRecordManagement = node.EnableRecordManagement;
                    spSetting.IsFailedConfigMetaDataColumn = false;
                    spSetting.IsFailedConfigClassification = false;
                    spSetting.SiteGroupId = new Guid(node.Id);
                    spSetting.SettingTime = 0;
                    spSetting.NeedCheckDefaultValue = node.NeedCheckDefaultValue;
                    spSetting.EMailToRecordOwner = node.EMailToRecordOwner;
                    spSetting.NodeInfo = SerializerHelper.SerializeByDataContractSerializer(node);
                    spSetting.ApplyExistType = node.ApplyExistType;
                    spSetting.DeployTermMethod = (int)node.DeployTermMethod;
                    spSetting.AutoClassificationRules = node.AutoClassificationRules == null ?
                        null : SerializerHelper.SerializeByDataContractSerializer(node.AutoClassificationRules);
                    spSetting.RunAutoFullJob = node.RunAutoFullJob;
                    spSetting.AutoJobOption = (int)node.AutoJobOption;
                    spSetting.IncludeDeclaredRecords = node.IncludeDeclaredRecords;
                    spSetting.ApprovalType = (ApprovalType)node.ApprovalType;
                    spSetting.WorkflowReferenceId = node.WorkflowReferenceId;
                    spSetting.IsShowUniqueId = node.IsShowUniqueId;

                    spSetting.AITermUseType = node.AITermUseType;
                    spSetting.AIApprovalType = (ApprovalType)node.AIApprovalType;
                    spSetting.AISendEMail = node.AISendEMail;
                    spSetting.AIThenIsDefaultTermMethod = node.AIThenIsDefaultTermMethod;
                    spSetting.AIThenDefaultTermId = node.AIThenDefaultTermId;
                    spSetting.AIThenDefaultTermName = node.AIThenDefaultTermName;
                    ApplyCurrentValues(context, spSetting);
                    await RecordOwnerDao.UpdateRecordOwnersAsync(spSetting.Id, node.RecordOwner, RecordOwnerSettingType.OneDrive);
                    if (node.AIReviewers != null)
                    {
                        await RecordOwnerDao.AddRecordOwnersAsync(spSetting.Id, node.AIReviewers, RecordOwnerSettingType.AISharePointOnline);
                    }
                }
                else
                {
                    RMOneDriveSetting settings = new RMOneDriveSetting()
                    {
                        DefaultTermId = node.DefaultTermId,
                        DefaultTermName = node.DefaultTermName,
                        FullPath = node.FullPath,
                        ScopeId = new Guid(node.SPObjectId),
                        TermId = node.TermId,
                        TermName = node.TermName,
                        TermSetId = node.TermSetId,
                        TermSetName = node.TermSetName,
                        EnableRecordManagement = node.EnableRecordManagement,
                        IsFailedConfigMetaDataColumn = false,
                        IsFailedConfigClassification = false,
                        SiteGroupId = new Guid(node.Id),
                        SettingTime = 0,
                        NeedCheckDefaultValue = node.NeedCheckDefaultValue,
                        ApplyExistType = node.ApplyExistType,
                        EMailToRecordOwner = node.EMailToRecordOwner,
                        NodeInfo = SerializerHelper.SerializeByDataContractSerializer(node),
                        DeployTermMethod = (int)node.DeployTermMethod,
                        AutoClassificationRules = node.AutoClassificationRules == null ?
                            null : SerializerHelper.SerializeByDataContractSerializer(node.AutoClassificationRules),
                        RunAutoFullJob = node.RunAutoFullJob,
                        AutoJobOption = (int)node.AutoJobOption,
                        IncludeDeclaredRecords = node.IncludeDeclaredRecords,
                        ApprovalType = (ApprovalType)node.ApprovalType,
                        WorkflowReferenceId = node.WorkflowReferenceId,
                        IsShowUniqueId = node.IsShowUniqueId,

                        AITermUseType = node.AITermUseType,
                        AIApprovalType = (ApprovalType)node.AIApprovalType,
                        AISendEMail = node.AISendEMail,
                        AIThenIsDefaultTermMethod = node.AIThenIsDefaultTermMethod,
                        AIThenDefaultTermId = node.AIThenDefaultTermId,
                        AIThenDefaultTermName = node.AIThenDefaultTermName,
                    };
                    context.RMOneDriveSettings.Add(settings);
                    context.SaveChanges();
                    spSetting = context.RMOneDriveSettings.AsQueryable().Where(s => s.ScopeId == settings.ScopeId).First();
                    await RecordOwnerDao.AddRecordOwnersAsync(spSetting.Id, node.RecordOwner, RecordOwnerSettingType.OneDrive);
                    if (node.AIReviewers != null)
                    {
                        await RecordOwnerDao.AddRecordOwnersAsync(spSetting.Id, node.AIReviewers, RecordOwnerSettingType.AIOneDrive);
                    }
                }
            }
        }

        public RMOneDriveSetting LoadOneDriveSetting(Guid scopeId, Guid siteId)
        {
            using (var context = RMDBContextManager.GetNewDBContext())
            {
                RMOneDriveSetting spSetting = null;
                if (siteId != Guid.Empty)
                {
                    var remoteSite = RMRemoteNodeDao.GetRemoteSiteCollectionById(siteId.ToString());
                    var groupId = remoteSite?.parentId;
                    if (!string.IsNullOrEmpty(groupId))
                    {
                        spSetting = context.RMOneDriveSettings.AsQueryable().Where(s => s.ScopeId.Equals(scopeId) && s.SiteId.Equals(siteId) && s.SiteGroupId.Equals(new Guid(groupId)) && !s.IsRemoved).FirstOrDefault();
                    }
                    else
                    {
                        spSetting = context.RMOneDriveSettings.AsQueryable().Where(s => s.ScopeId.Equals(scopeId) && s.SiteId.Equals(siteId) && !s.IsRemoved).FirstOrDefault();
                    }
                }
                if (spSetting == null)
                {
                    spSetting = context.RMOneDriveSettings.AsQueryable().Where(s => s.ScopeId.Equals(scopeId) && s.SiteId.Equals(Guid.Empty) && !s.IsRemoved).FirstOrDefault();
                }
                return spSetting;
            }
        }

        public List<RMOneDriveSetting> LoadOneDriveSettings(Guid groupId)
        {
            using (var context = GetNewContext())
            {
                var spSettings = context.RMOneDriveSettings.AsQueryable().Where(s => s.SiteGroupId.Equals(groupId) && !s.IsRemoved).ToList();
                return spSettings;
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

        public async Task DeleteOneDriveSettingAsync(Guid id, Guid siteId)
        {
            using (var context = GetNewContext())
            {
                var groupId = GetGroupIdBySiteId(siteId);
                var spSetting = context.RMOneDriveSettings.AsQueryable().Where(s => s.SiteGroupId.Equals(groupId) && s.ScopeId.Equals(id) && s.SiteId.Equals(siteId) && !s.IsRemoved).FirstOrDefault();

                if (spSetting != null)
                {
                    context.RMOneDriveSettings.Remove(spSetting);
                    await RecordOwnerDao.BatchDeleteAsync(o => o.SPSettingId == spSetting.Id);
                    await RMEXOSettingRuleDao.BatchDeleteAsync(o => o.ScopeId == siteId);
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
                    var setting = context.RMOneDriveSettings.AsQueryable().Where(s => s.SiteGroupId == groupId && s.ScopeId.Equals(new Guid(node.SPObjectId)) && !s.IsRemoved).FirstOrDefault();
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

        public RMOneDriveSetting GetParentNode(Expression<Func<RMOneDriveSetting, bool>> whereLambda)
        {
            RMOneDriveSetting result = new RMOneDriveSetting();
            using (var context = RMDBContextManager.GetNewDBContext())
            {
                result = context.RMOneDriveSettings.AsQueryable().Where(whereLambda).Where(s => !s.IsRemoved).FirstOrDefault();
            }
            return result;
        }


        public List<RMOneDriveSetting> GetFolderSettingUnderList(Guid listId, Guid siteId)
        {
            using (var context = GetNewContext())
            {
                var groupId = GetGroupIdBySiteId(siteId);
                return context.RMOneDriveSettings.Where(s => s.SiteGroupId == groupId && s.SiteId == siteId && s.ListId == listId && s.ScopeId == s.FolderId && !s.IsRemoved).ToList();
            }
        }

        public List<RMOneDriveSetting> LoadAllSetting()
        {
            using (var context = GetNewContext())
            {
                return context.RMOneDriveSettings.AsQueryable().Where(s => s.NodeInfo != null && !s.IsRemoved).ToList();
            }
        }

        public List<GCommon.Contract.StorageOptimization.Object.UserInfo> GetReocrdOwnersBySettingId(int settingId)
        {
            using (var context = GetNewContext())
            {
                var owners = context.RecordOwner.Where(item => item.SPSettingId == settingId && item.SettingType == 5).ToList();
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

        public List<RMOneDriveSetting> GetAllGroupSettings()
        {
            using (var context = GetNewContext())
            {
                return context.RMOneDriveSettings.AsQueryable().Where(g => g.ScopeId == g.SiteGroupId && !g.IsRemoved).ToList();
            }
        }

        public RMOneDriveSetting GetSettingInfoByScope(Guid groupId, Guid siteId, Guid scopeId)
        {
            using (var ctx = GetNewContext())
            {
                return ctx.RMOneDriveSettings.Where(s => s.SiteGroupId == groupId && s.SiteId == siteId && s.ScopeId == scopeId && !s.IsRemoved).FirstOrDefault();
            }
        }
        public RMOneDriveSetting GetSettingInfoByAgentGroupId(string id)
        {
            using (var context = GetNewContext())
            {
                var spSetting = context.RMOneDriveSettings.AsQueryable().Where(s => s.ScopeId.Equals(new Guid(id)) && !s.IsRemoved).FirstOrDefault();
                return spSetting;
            }
        }

        public List<RMOneDriveSetting> GetSettingsByIds(string[] ids)
        {
            using (var context = GetNewContext())
            {
                List<RMOneDriveSetting> settings = null;
                var groupId = GetGroupIdByScopeId(new Guid(ids.FirstOrDefault()), context);
                if (groupId == Guid.Empty)
                {
                    settings = context.RMOneDriveSettings.AsQueryable().Where(t => Enumerable.Contains(ids, t.ScopeId.ToString()) && !t.IsRemoved).ToList();
                }
                else
                {
                    settings = context.RMOneDriveSettings.AsQueryable().Where(t => t.SiteGroupId == groupId && Enumerable.Contains(ids, t.ScopeId.ToString()) && !t.IsRemoved).ToList();
                }

                if (!settings.Any())
                {
                    return new List<RMOneDriveSetting>();
                }
                return settings;
            }
        }

        public List<RMOneDriveSetting> LoadOneDriveSettingsUnderSite(Guid siteId)
        {
            using (var context = GetNewContext())
            {
                var groupId = GetGroupIdBySiteId(siteId);
                return context.RMOneDriveSettings.AsQueryable().Where(s => s.SiteGroupId == groupId && s.SiteId == siteId && !s.IsRemoved).ToList();
            }
        }

        public void CheckNeedRemoveDescendantsSetting(RMSPTreeNode node, string profileIdPath)
        {
            if (node.EnableRecordManagement != (int)EnableRecordManagementSetting.Enable)
            {
                ScheduleService.DeleteSchedules(ScheduleType.OneDriveDisposalSchedule, profileIdPath);
                var deleteDescendantsSql = "Delete From {0}.[RMOneDriveSettings] Where {1} = @scopeId And ScopeId <> @scopeId";
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

        public async Task SetSettingJobTimeAsync(Guid scopeId, Guid siteId)
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
                    var setting = context.RMOneDriveSettings.AsQueryable().Where(s => s.SiteGroupId == groupId && s.ScopeId.Equals(scopeId) && s.SiteId.Equals(siteId) && !s.IsRemoved).FirstOrDefault();
                    if (setting != null)
                    {
                        setting.SettingTime = DateTime.UtcNow.Ticks;
                        setting.IsFailedConfigClassification = false;
                        setting.IsFailedConfigMetaDataColumn = false;
                        setting.NeedCheckDefaultValue = false;
                        setting.RunAutoFullJob = false;
                        setting.IncludeDeclaredRecords = false;
                        setting.ApplyExistType = 0;
                    }
                    await UpdateAsync(setting);
                }
            }
            catch (Exception e)
            {
                logger.Error($"An error occurred while set setting job time by node: [{scopeId}], site id: [{siteId}]. error:[{e.ToString()}]");
            }
        }

        public string GetMetadataColumn(Guid nodeId)
        {
            using (var context = GetNewContext())
            {
                var setting = context.RMOneDriveSettings.AsQueryable().Where(t => t.ScopeId.Equals(nodeId) && !t.IsRemoved).FirstOrDefault();
                if (setting != null)
                {
                    //if (!setting.IsUsingExistColumnName)
                    //{
                    //    return setting.ColumnName;
                    //}
                    //else
                    //{
                    //    return setting.ExistColumnName;
                    //}
                }
                return string.Empty;
            }
        }

        public bool GetSettingEnableInfoByScope(Guid groupId, Guid siteId, Guid scopeId)
        {
            using (var ctx = GetNewContext())
            {
                var setting = ctx.RMOneDriveSettings.Where(s => s.SiteGroupId == groupId && s.SiteId == siteId && s.ScopeId == scopeId && !s.IsRemoved).FirstOrDefault();
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

        public List<RMOneDriveSetting> LoadShowUniqueIdSetting()
        {
            using (var ctx = GetNewContext())
            {
                return ctx.RMOneDriveSettings.Where(s => s.EnableRecordManagement == (int)EnableRecordManagementSetting.Enable && s.IsShowUniqueId == true && s.ScopeId == s.SiteGroupId && !s.IsRemoved).ToList();
            }
        }
        public List<RMOneDriveSetting> LoadGroupSetting(bool isRecheckRule = true)
        {
            using (var ctx = GetNewContext())
            {
                return ctx.RMOneDriveSettings.Where(s => (s.EnableRecordManagement == (int)EnableRecordManagementSetting.Enable || !isRecheckRule) && s.ScopeId == s.SiteGroupId && !s.IsRemoved).ToList();
            }
        }        
        
        public RMOneDriveSetting LoadOneSiteSettingEnableManualApprovalFirst()
        {
            using var ctx = GetNewContext();
            return ctx.RMOneDriveSettings.Where(s => s.EnableRecordManagement == (int)EnableRecordManagementSetting.Enable && s.SiteId != Guid.Empty && !s.IsRemoved).FirstOrDefault();
        }

        public bool ExistShowUniqueIdSetting()
        {
            using (var ctx = GetNewContext())
            {
                return ctx.RMOneDriveSettings.Any(s => s.EnableRecordManagement == (int)EnableRecordManagementSetting.Enable && s.IsShowUniqueId == true && s.ScopeId == s.SiteGroupId && !s.IsRemoved);
            }
        }

        public List<RMOneDriveSetting> GetDescendantsDisableNodes(RMSPTreeNode node)
        {
            Expression<Func<RMOneDriveSetting, bool>> lambda = null;
            var scopeId = new Guid(node.SPObjectId);
            var groupId = node.SiteGroupId;
            using (var context0 = GetNewContext())
            {
                switch ((NodeLevel)node.Level)
                {
                    case NodeLevel.WebApplication:
                    case NodeLevel.SkyDriveProGroup:
                        lambda = s => s.SiteGroupId == scopeId;
                        break;
                    case NodeLevel.SiteCollection:
                        lambda = s => s.SiteId == scopeId;
                        break;
                    case NodeLevel.Site:
                        lambda = s => s.FullPath.StartsWith(node.FullPath);
                        break;
                    case NodeLevel.List:
                        lambda = s => s.ListId == scopeId;
                        break;
                }
            }
            using (var context = GetNewContext())
            {
                return context.RMOneDriveSettings.Where(lambda).Where(s => s.SiteGroupId == groupId && s.ScopeId != node.SettingScopeId && s.EnableRecordManagement == (int)EnableRecordManagementSetting.Disable).ToList();
            }
        }

        private Guid GetGroupIdBySiteId(Guid siteId)
        {
            var site = RMRemoteNodeDao.GetRemoteSiteCollectionById(siteId.ToString());
            return site != null ? new Guid(site.parentId) : Guid.Empty;
        }

        private Guid GetGroupIdByScopeId(Guid scopeId, RMDbContext context)
        {
            var setting = context.RMOneDriveSettings.Where(s => s.ScopeId == scopeId).FirstOrDefault();
            if (setting != null)
            {
                var siteId = setting.SiteId;
                var site = RMRemoteNodeDao.GetRemoteSiteCollectionById(siteId.ToString());
                return site != null ? new Guid(site.parentId) : Guid.Empty;
            }
            return Guid.Empty;
        }
    }
}
