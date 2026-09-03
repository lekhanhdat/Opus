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
using AvePoint.RA.Contract.FileSystemRegister;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.Schedule;
using AvePoint.RA.Contract.TaxonomyModel;
using AvePoint.RA.DB.Core;
using AvePoint.RA.DB.Dao.Discovery.FileSystem;
using AvePoint.RA.DB.Dao.Discovery.Impl.FileSystem;
using AvePoint.RA.DB.Explorer;
using AvePoint.RA.DB.Explorer.Dao;
using AvePoint.RA.DB.Model;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace AvePoint.RA.DB.Dao.Impl
{
    public class FileSystemSettingDao : BaseDao<RMFileSystemSetting>, IFileSystemSettingDao
    {
        private AvePoint.RA.Contract.Services.IRALogger logger = RALogger.GetInstance(typeof(FileSystemSettingDao));
        public IRecordOwnerDao RecordOwnerDao { get; set; }
        public IAccountDao AccountDao { get; set; }
        public IScheduleService ScheduleService { get; set; }
        private IFSConnectionDao FSConnectionDao => PlatformWindsorManager.GetService<IFSConnectionDao>();

        private IExplorerDao mExplorerDao;
        public IExplorerDao ExplorerDao
        {
            get
            {
                if (mExplorerDao == null)
                {
                    mExplorerDao = new RA.DB.Explorer.Dao.CosmosImp.ExplorerDao();
                }
                return mExplorerDao;
            }
        }

        public async Task AddOrUpdateFSSettingAsync(RMFileSystemSetting fsSetting)
        {
            using (var context = GetNewContext())
            {
                var existingSetting = await context.RMFileSystemSettings.FindAsync(fsSetting.Id);
                if (existingSetting != null)
                {
                    context.Entry(existingSetting).CurrentValues.SetValues(fsSetting);
                }
                else
                {
                    context.RMFileSystemSettings.Add(fsSetting);
                }
                await context.SaveChangesAsync();
            }
        }

        public async Task AddOrUpdateFSSettingAsync(RMFSTreeNode node, Guid connGId)
        {
            EnsureTermName(node);
            using (var context = GetNewContext())
            {
                RMFileSystemSetting fsSetting = context.RMFileSystemSettings.AsQueryable().Where(s => s.ScopeId.Equals(node.Id) && s.ConnectionGroupId.Equals(connGId)).FirstOrDefault();

                if (fsSetting != null)
                {
                    fsSetting.DefaultTermId = node.DefaultTermId;
                    fsSetting.DefaultTermName = node.DefaultTermName;
                    fsSetting.FullPath = node.FullPath;
                    fsSetting.ScopeId = node.Id;
                    fsSetting.ConnectionGroupId = connGId;
                    fsSetting.TermId = node.TermId;
                    fsSetting.TermName = node.TermName;
                    fsSetting.TermSetId = node.TermSetId;
                    fsSetting.TermSetName = node.TermSetName;
                    fsSetting.DescriptionOfContainer = node.DescriptionOfContainer;
                    fsSetting.TermIdOfContainer = node.TermIdOfContainer;
                    fsSetting.TermNameOfContainer = node.TermNameOfContainer;
                    fsSetting.IsEnableContainerLevelClassification = node.isEnableClassification;
                    fsSetting.SettingTime = 0;
                    fsSetting.NodeInfo = SerializerHelper.SerializeByDataContractSerializer(node);
                    fsSetting.NeedCheckDefaultValue = node.NeedCheckDefaultValue;
                    fsSetting.ApplyExistType = node.ApplyExistType;
                    fsSetting.ApplyExistDocument = node.ApplyExistDocument;
                    fsSetting.EnableRelatedRecords = node.EnableRelatedRecords;
                    fsSetting.EMailToRecordOwner = node.EMailToRecordOwner;
                    fsSetting.IsNewEdited = true;
                    //fsSetting.IdPath = node.ProfileId;
                    fsSetting.IdPath = ScheduleService.GetProfileId(node);
                    fsSetting.IsActive = node.IsActive;
                    fsSetting.DeployTermMethod = (int)node.DeployTermMethod;
                    fsSetting.AutoClassificationRules = node.AutoClassificationRules == null ?
                        null : SerializerHelper.SerializeByDataContractSerializer(node.AutoClassificationRules);
                    fsSetting.RunAutoFullJob = node.RunAutoFullJob;
                    fsSetting.AutoJobOption = (int)node.AutoJobOption;
                    fsSetting.ApprovalType = (ApprovalType)node.ApprovalType;
                    fsSetting.WorkflowReferenceId = node.WorkflowReferenceId;
                    fsSetting.EnableRecordManagement = node.EnableRecordManagement;
                    fsSetting.IsAllowUserDownloadRCCReport = node.IsAllowUserDownloadRCCReport;
                    fsSetting.ClassCode = node.ClassCode?.ClassCodeId;
                    fsSetting.CountryCode = node.ClassCode?.CountryCode;
                    fsSetting.RetentionScheduleType = node.ClassCode?.RetentionType ?? RetentionScheduleType.Flat;
                    fsSetting.StartDate = long.TryParse(node.ClassCode?.RetentionDate, out var ticksUpdate)
                        ? ticksUpdate
                        : node.ClassCode == null ? 0 : node.ClassCode.StartDate;
                    fsSetting.EffectScope = node.EffectScope;
                    await this.UpdateAsync(fsSetting);
                    await RecordOwnerDao.UpdateRecordOwnersAsync(fsSetting.Id, node.RecordOwner, RecordOwnerSettingType.FileSystem);
                }
                else
                {
                    RMFileSystemSetting settings = new RMFileSystemSetting()
                    {
                        DefaultTermId = node.DefaultTermId,
                        DefaultTermName = node.DefaultTermName,
                        FullPath = node.FullPath,
                        ScopeId = node.Id,
                        ConnectionGroupId = connGId,
                        TermId = node.TermId,
                        TermName = node.TermName,
                        TermSetId = node.TermSetId,
                        TermSetName = node.TermSetName,
                        DescriptionOfContainer = node.DescriptionOfContainer,
                        TermIdOfContainer = node.TermIdOfContainer,
                        TermNameOfContainer = node.TermNameOfContainer,
                        IsEnableContainerLevelClassification = node.isEnableClassification,
                        SettingTime = 0,
                        NeedCheckDefaultValue = node.NeedCheckDefaultValue,
                        ApplyExistType = node.ApplyExistType,
                        ApplyExistDocument = node.ApplyExistDocument,
                        EnableRelatedRecords = node.EnableRelatedRecords,
                        EMailToRecordOwner = node.EMailToRecordOwner,
                        IsNewEdited = true,
                        //IdPath = node.ProfileId,
                        IdPath = ScheduleService.GetProfileId(node),
                        IsActive = node.IsActive,
                        NodeInfo = SerializerHelper.SerializeByDataContractSerializer(node),
                        DeployTermMethod = (int)node.DeployTermMethod,
                        AutoClassificationRules = node.AutoClassificationRules == null ?
                            null : SerializerHelper.SerializeByDataContractSerializer(node.AutoClassificationRules),
                        RunAutoFullJob = node.RunAutoFullJob,
                        AutoJobOption = (int)node.AutoJobOption,
                        ApprovalType = (ApprovalType)node.ApprovalType,
                        WorkflowReferenceId = node.WorkflowReferenceId,
                        EnableRecordManagement = node.EnableRecordManagement,
                        IsAllowUserDownloadRCCReport = node.IsAllowUserDownloadRCCReport,
                        ClassCode = node.ClassCode?.ClassCodeId,
                        CountryCode = node.ClassCode?.CountryCode,
                        RetentionScheduleType = node.ClassCode?.RetentionType ?? RetentionScheduleType.Flat,
                        StartDate = long.TryParse(node.ClassCode?.RetentionDate, out var ticksInsert)
                            ? ticksInsert
                            : node.ClassCode == null ? 0 : node.ClassCode.StartDate,
                        EffectScope = node.EffectScope,
                    };

                    context.RMFileSystemSettings.Add(settings);
                    context.SaveChanges();
                    fsSetting = context.RMFileSystemSettings.AsQueryable().Where(s => s.ScopeId.Equals(node.Id) && s.ConnectionGroupId.Equals(connGId)).First();
                    await RecordOwnerDao.AddRecordOwnersAsync(fsSetting.Id, node.RecordOwner, RecordOwnerSettingType.FileSystem);
                }
            }
        }
        public async Task<bool> IsFSEnableRecordManagement(Guid scpoeId)
        {
            RMFileSystemSetting spSetting = null;
            using (var context = GetNewContext())
            {
                if (scpoeId != Guid.Empty)
                {
                    spSetting = context.RMFileSystemSettings.AsQueryable().Where(s => s.ScopeId.Equals(scpoeId)).FirstOrDefault();
                }
            }
            if (spSetting == null)
            {
                return true;
            }
            else
            {
                return spSetting.EnableRecordManagement != (int)RMFSTreeNode.EnableRecordManagementSetting.Disable;
            }
        }

        public async Task<bool> IsFullPathConnectionExist(RMFSTreeNode sNode)
        {
            if (sNode != null && sNode.Level == (int)NodeLevel.SiteCollection && sNode.ConnGroupId != Guid.Empty && !string.IsNullOrEmpty(sNode.FullPath))
                return FSConnectionDao.GetConnectionByUNCPath(sNode.FullPath) != null;

            return true;
        }
        
        public async Task<List<string>> AllDisabledRecordManagementPath()
        {
            List<RMFileSystemSetting> fsSettings = null;
            List<string> result = new List<string>();
            using (var context = GetNewContext())
            {
                fsSettings = context.RMFileSystemSettings.AsQueryable()?.ToList();
            }

            if (fsSettings != null && fsSettings.Count>0)
            {
                foreach(var setting in fsSettings)
                {
                    if (setting.EnableRecordManagement == (int)RMFSTreeNode.EnableRecordManagementSetting.Disable)
                    {
                        result.Add(setting.FullPath);
                    }
                }
            }
            return result;
        }
        public List<Guid> ValidateEnableRecordManagementNodes(List<Guid> nodeIds)
        {
            const int BatchSize = 500;
            var disableMode = (int)RMFSTreeNode.EnableRecordManagementSetting.Disable;

            if (nodeIds == null || nodeIds.Count == 0)
            {
                return new List<Guid>();
            }

            var disabledIds = new HashSet<Guid>(nodeIds.Count);

            using (var ctx = GetNewContext())
            {
                foreach (var batch in nodeIds.Chunk(BatchSize))
                {
                    var ids = ctx.RMFileSystemSettings
                        .Where(x => Enumerable.Contains(batch, x.ScopeId) && x.EnableRecordManagement == disableMode)
                        .Select(x => x.ScopeId)
                        .ToList();

                    disabledIds.UnionWith(ids);
                }

                return nodeIds.Where(id => !disabledIds.Contains(id)).ToList();
            }
        }

        public RMFileSystemSetting LoadFSSetting(Guid scpoeId, Guid connGId)
        {
            RMFileSystemSetting spSetting = null;
            using (var context = GetNewContext())
            {
                if (connGId != Guid.Empty)
                {
                    spSetting = context.RMFileSystemSettings.AsQueryable().Where(s => s.ScopeId.Equals(scpoeId) && s.ConnectionGroupId.Equals(connGId)).FirstOrDefault();
                }
            }
            return spSetting;
        }

        private void EnsureTermName(RMFSTreeNode node)
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

        public RMFileSystemSetting GetSettingByConnGroupId(Guid connGroupId)
        {
            using (var context = GetNewContext())
            {
                RMFileSystemSetting spSetting = context.RMFileSystemSettings.AsQueryable().Where(s => s.ScopeId.Equals(connGroupId)).FirstOrDefault();
                return spSetting;
            }
        }

        public async Task DeleteFileSystemSettingAsync(Guid id, Guid connGid)
        {
            using var context = GetNewContext();
            RMFileSystemSetting fsSetting = context.RMFileSystemSettings.AsQueryable().Where(s => s.ScopeId.Equals(id) && s.ConnectionGroupId.Equals(connGid)).FirstOrDefault();
            if (fsSetting != null)
            {
                context.RMFileSystemSettings.Remove(fsSetting);
                await RecordOwnerDao.BatchDeleteAsync(o => o.SPSettingId == fsSetting.Id && o.SettingType == (int)RecordOwnerSettingType.FileSystem);
                context.SaveChanges();
            }
        }

        public async Task DeleteFSWithSubFolderSettingAsync(List<Guid> ids)
        {
            try
            {
                using var context = GetNewContext();
                var fsSettings = await context.RMFileSystemSettings.AsQueryable().Where(s => ids.Contains(s.ScopeId)).ToListAsync();
                var fsConnections = await context.FSConnection.AsQueryable().Where(s => ids.Contains(s.Id)).ToListAsync();

                if (fsSettings.Count != 0)
                {
                    context.RMFileSystemSettings.RemoveRange(fsSettings);
                    var fsSettingIds = fsSettings.Select(f => f.Id).ToList();
                    var needDeleteOwner = await context.RecordOwner.AsQueryable().Where(o => fsSettingIds.Any(f => o.SPSettingId == f) && o.SettingType == (int)RecordOwnerSettingType.FileSystem).ToListAsync();
                    await RecordOwnerDao.BatchDeleteAsync(needDeleteOwner);
                }
                if (fsConnections.Count != 0)
                {
                    var connectionUNCPaths = fsConnections.Select(f => f.UNCPath + '\\').ToList();
                    var subFolderSetting = await context.RMFileSystemSettings.AsQueryable().Where(s => connectionUNCPaths.Any(f => s.FullPath.Contains(f))).ToListAsync();
                    context.RMFileSystemSettings.RemoveRange(subFolderSetting);
                }
                await context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public List<string> GetBreakNodeIds(string parentId)
        {
            using (var ctx = GetNewContext())
            {
                return ctx.RMFileSystemSettings.AsQueryable().Where(sc => sc.IdPath.Contains(parentId)).AsEnumerable().Select(s => s.IdPath).ToList();
            }

        }

        public async Task DeacitveDescendantsSettingAsync(RMFSTreeNode node, Guid connGId)
        {
            EnsureTermName(node);
            using var context = GetNewContext();
            var descendantsSettings = context.RMFileSystemSettings.AsQueryable().Where(s => s.IdPath.Contains(node.ProfileId) && s.ConnectionGroupId.Equals(connGId)).ToList();

            foreach (var setting in descendantsSettings)
            {
                setting.IsActive = false;
                await this.UpdateAsync(setting);
            }
        }
        public RMFileSystemSetting LoadInheritSetting(Guid nodeId, Guid connGId, ref Guid firstParentId)
        {
            RMFileSystemSetting fsSetting = null;
            if (nodeId.Equals(Guid.Empty))
            {
                //root node
                return fsSetting;
            }
            if (firstParentId.Equals(Guid.Empty))
            {
                //using (var ctx = GetExplorerContext())
                //{
                //    var fsNode = ctx.ManagedRecord.Where(d => d.NodeId == nodeId).FirstOrDefault();
                //    if (fsNode != null)
                //    {
                //        firstParentId = fsNode.NodeType == 2100 ? nodeId : fsNode.ParentId;
                //    }
                //}

                var fsNode = ExplorerDao.QueryAll(r => r.NodeId == nodeId).FirstOrDefault();
                if (fsNode != null)
                {
                    firstParentId = fsNode.NodeType == 2100 ? nodeId : fsNode.ParentId;
                }
            }
            using var context = GetNewContext();
            if (nodeId.Equals(connGId))
            {
                fsSetting = context.RMFileSystemSettings.AsQueryable().Where(s => s.ScopeId.Equals(nodeId) && s.ConnectionGroupId.Equals(connGId)).FirstOrDefault();
            }
            else
            {
                var settings = context.RMFileSystemSettings.AsQueryable().ToList();
                //foreach (var item in settings)
                //{
                //    //解密
                //    var decryptNodePath = EncodeUtil.DecryptByCommunicationKey(item.FullPath);
                //    //转md5
                //    Guid pathMd5 = new Guid(HashCodeHelper.ToMD5HashCode(decryptNodePath.ToLowerInvariant()));
                //    if (pathMd5.Equals(nodeId) && item.ConnectionGroupId.Equals(connGId))
                //    {
                //        fsSetting = item;
                //        break;
                //    }
                //}
            }
            if (fsSetting != null)
            {
                return fsSetting;
            }
            else
            {
                //using (var ctx = GetExplorerContext())
                //{
                //    var fsNode = ctx.ManagedRecord.Where(d => d.NodeId == nodeId).FirstOrDefault();
                //    if (fsNode != null)
                //    {
                //        return LoadInheritSetting(fsNode.ParentId, connGId, ref firstParentId);
                //    }
                //}
                var fsNode = ExplorerDao.QueryAll(r => r.NodeId == nodeId).FirstOrDefault();
                if (fsNode != null)
                {
                    return LoadInheritSetting(fsNode.ParentId, connGId, ref firstParentId);
                }
            }
            return fsSetting;
        }
        /*private ExplorerDbContext GetExplorerContext()
        {
            return new ExplorerDbContext();
        }*/

        public string GetTreeNodeInfoByScheduleId(ScheduleType type, string scheduleId)
        {
            string nodeInfo = string.Empty;
            //using (var ctx = GetNewContext())
            //{
            //    if (type == ScheduleType.FSColletionDataSchedule)
            //    {
            //        nodeInfo = ctx.RMFileSystemSettings.Where(s => s.CollectionJobId1 == scheduleId).Select(c => c.NodeInfo).FirstOrDefault();
            //    }
            //    else if (type == ScheduleType.FSDisposalSchedule)
            //    {
            //        nodeInfo = ctx.RMFileSystemSettings.Where(s => s.DisposalJobId1 == scheduleId).Select(c => c.NodeInfo).FirstOrDefault();
            //    }
            //}
            return nodeInfo;
        }

        public bool ResetApplyExistingOption(Guid scopeId)
        {
            using (var ctx = GetNewContext())
            {
                var settings = ctx.RMFileSystemSettings.Where(t => t.ScopeId == scopeId).ToList();
                //.Update(n => new RMFileSystemSetting
                //{
                //    NeedCheckDefaultValue = false,
                //    ApplyExistType = (int)ApplyExistingTermType.None,
                //    RunAutoFullJob = false
                //});
                foreach (var s in settings)
                {
                    s.NeedCheckDefaultValue = false;
                    s.ApplyExistType = (int)ApplyExistingTermType.None;
                    s.RunAutoFullJob = false;
                }
                this.BatchUpdate(ctx, settings);
                return settings.Count() > 0;
            }

        }

        public bool ResetApplyClassCodeExistingOption(Guid scopeId)
        {
            using (var ctx = GetNewContext())
            {
                var settings = ctx.RMFileSystemSettings.Where(t => t.ScopeId == scopeId || t.ConnectionGroupId == scopeId).ToList();
                foreach (var setting in settings)
                {
                    setting.ApplyExistDocument = false;
                }
                return BatchUpdate(ctx, settings) > 0;
            }
        }

        public List<RMFileSystemSetting> LoadAllSetting()
        {
            using (var ctx = GetNewContext())
            {
                return ctx.RMFileSystemSettings.OrderByDescending(setting => setting.FullPath.Length).ToList();
            }
        }

        public IEnumerable<RMFileSystemSetting> LoadAllSettingByGroupIds(IEnumerable<Guid> groupIds)
        {
            using (var ctx = GetNewContext())
            {
                if (groupIds == null) return Array.Empty<RMFileSystemSetting>();
                return ctx.RMFileSystemSettings.AsNoTracking().Where(setting => groupIds.Contains(setting.ConnectionGroupId)).OrderByDescending(setting => setting.FullPath.Length).ToList();
            }
        }

        public List<RMFileSystemSetting> GetAllDeactiveUnderGroup(Guid groupId)
        {
            using (var ctx = GetNewContext())
            {
                return ctx.RMFileSystemSettings.Where(s => s.ConnectionGroupId == groupId && s.IsActive == false).ToList();
            }
        }

        public List<RMFileSystemSetting> LoadAllSettingsUnderGroup(Guid groupId)
        {
            using (var ctx = GetNewContext())
            {
                return ctx.RMFileSystemSettings.Where(s => s.ConnectionGroupId == groupId).ToList();
            }
        }

        public List<RMFileSystemSetting> LoadAllSettingsUnderConnection(string connectionPath)
        {
            using var ctx = GetNewContext();
            return ctx.RMFileSystemSettings.Where(s => s.FullPath.StartsWith(connectionPath + "\\") || s.FullPath.Equals(connectionPath)).ToList();
        }

        public List<RMFileSystemSetting> LoadAllSettingsByConnectionGroupIdAndConnectionPath(Guid groupId, string connectionPath)
        {
            using var ctx = GetNewContext();
            return ctx.RMFileSystemSettings.Where(s => s.ConnectionGroupId == groupId && (s.FullPath.StartsWith(connectionPath + "\\") || s.FullPath.Equals(connectionPath))).ToList();
        }

        public List<GCommon.Contract.StorageOptimization.Object.UserInfo> GetReocrdOwnersBySettingId(int settingId)
        {
            using (var context = GetNewContext())
            {
                var owners = context.RecordOwner.Where(item => item.SPSettingId == settingId && item.SettingType == 3).ToList();
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

        public List<RecordOwnerGroupDto> GetRecordOwners(List<Guid> scopeIds)
        {
            var results = new List<RecordOwnerGroupDto>();

            using (var ctx = GetNewContext())
            {
                var settings = ctx.RMFileSystemSettings.Where(o => scopeIds.Contains(o.ScopeId))
                .Select(s => new RecordOwnerGroupDto()
                {
                    SPSettingId = s.Id,
                    ScopeId = s.ScopeId,
                    MailToOwner = s.EMailToRecordOwner
                }).ToDictionary(s => s.SPSettingId);

                if (settings.Count > 0)
                {
                    var settingIds = settings.Keys;
                    var ownerGroups = ctx.RecordOwner
                        .Where(o => settingIds.Contains(o.SPSettingId) && o.SettingType == 3)
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
                                        Type = owner.ObjectType == RMActiveDirectoryObjectType.Group ? Contract.Object.AccountType.Group : Contract.Object.AccountType.User,
                                    };
                                }));
                            }
                            results.Add(setting.Value);
                        }
                        catch (Exception ex)
                        {
                            logger.Warn("FS: get record owner {0} error:{1}", setting.Value.ScopeId, ex.ToString());
                        }
                    }
                }
            }
            return results;
        }

        public bool IsDeactivedNode(string profileId)
        {
            var isDeactived = false;
            try
            {
                var idPaths = GetNodeIdPathList(profileId);
                using (var ctx = GetNewContext())
                {
                    //当前节点或者父级是Deactived
                    var existDeactivedNode = ctx.RMFileSystemSettings.Where(o => idPaths.Contains(o.IdPath) && o.IsActive == false).OrderByDescending(o => o.IdPath).FirstOrDefault();
                    isDeactived = existDeactivedNode != null;
                    logger.Info($"Check the node is deactived {isDeactived}, id:[{profileId}]");
                }
            }
            catch (Exception ex)
            {
                logger.Warn($"An error when excute IsDeactivedNode, id:{profileId}, message:{ex}");
            }
            return isDeactived;
        }

        public List<string> GetAllDeactiveId()
        {
            var ScopeIdLists = new List<string>();
            using (var ctx = GetNewContext())
            {
                var DeactiveNodes = ctx.RMFileSystemSettings.Where(o => o.IsActive == false).ToList();
                ScopeIdLists.AddRange(DeactiveNodes.Select(d => d.FullPath).ToList());
            }
            return ScopeIdLists;
        }
        public List<string> GetAllDisableRecordManagementPath(Guid groupId)
        {
            var path = new List<string>();
            using (var ctx = GetNewContext())
            {
                var disableNodes = ctx.RMFileSystemSettings.Where(o => o.EnableRecordManagement == (int)RMFSTreeNode.EnableRecordManagementSetting.Disable && o.ConnectionGroupId == groupId).ToList();
                path.AddRange(disableNodes.Select(d => d.FullPath).ToList());
            }
            return path;
        }

        public List<KeyValuePair<string, bool>> GetAllNodeRCCSettings(Guid groupId)
        {
            using (var ctx = GetNewContext())
            {
                return ctx.RMFileSystemSettings
                    .Where(o => o.ConnectionGroupId == groupId && !string.IsNullOrEmpty(o.FullPath))
                    .OrderByDescending(o => o.FullPath) 
                    .Select(o => new
                    {
                        o.FullPath,
                        IsAllow = o.IsAllowUserDownloadRCCReport
                    })
                    .ToList()
                    .Select(x => new KeyValuePair<string, bool>(x.FullPath, x.IsAllow))
                    .ToList();
            }
        }

        public bool IsConnGroupEnableDownloadRCC(Guid groupId)
        {
            var isEnable = false;
            using (var ctx = GetNewContext())
            {
                isEnable = ctx.RMFileSystemSettings.Where(c => c.ScopeId == groupId).FirstOrDefault()?.IsAllowUserDownloadRCCReport ?? false;
            }
            return isEnable;
        }

        public List<KeyValuePair<string, bool>> GetAllDeactivePath(Guid groupId)
        {
            using (var ctx = GetNewContext())
            {
                return ctx.RMFileSystemSettings
                    .Where(o => o.ConnectionGroupId == groupId && !string.IsNullOrEmpty(o.FullPath) && !o.IsActive)
                    .OrderByDescending(o => o.FullPath)
                    .Select(o => new
                    {
                        o.FullPath,
                        IsAllow = o.IsActive
                    })
                    .ToList()
                    .Select(x => new KeyValuePair<string, bool>(x.FullPath, x.IsAllow))
                    .ToList();
            }
        }

        public bool IsConnGroupActive(Guid groupId)
        {
            var isEnable = false;
            using (var ctx = GetNewContext())
            {
                isEnable = ctx.RMFileSystemSettings.Where(c => c.ScopeId == groupId).FirstOrDefault()?.IsActive ?? false;
            }
            return isEnable;
        }

        /// <summary>
        /// 查出所有父级包括自己的IdPath集合
        /// </summary>
        /// <param name="profileId"></param>
        /// <returns></returns>
        private List<string> GetNodeIdPathList(string profileId)
        {
            var idPaths = new List<string>();
            MatchCollection mc = Regex.Matches(profileId, "\\|", RegexOptions.None, RecordsConstants.REGEX_DEFAULT_MATCH_TIMEOUT);
            foreach (Match item in mc)
            {
                idPaths.Add(profileId.Substring(0, item.Index + 1).TrimEnd('|'));
            }
            idPaths.Add(profileId);
            idPaths.Reverse();
            return idPaths;
        }

        public async Task BatchUpdateClassCodeAsync(List<RMFileSystemSetting> settings, Guid classCodeId, string classCode, string countryCode, RetentionScheduleType retentionScheduleType, long startDate, bool applyExistDocument)
        {
            if (settings == null || settings.Count == 0)
            {
                return;
            }

            using (var ctx = GetNewContext())
            {
                var settingIds = settings.Select(s => s.Id).ToList();
                var dbSettings = ctx.RMFileSystemSettings.Where(s => settingIds.Contains(s.Id)).ToList();

                foreach (var setting in dbSettings)
                {
                    var node = SerializerHelper.DeserializeByDataContractSerializer<RMFSTreeNode>(setting.NodeInfo);
                    if (node != null)
                    {
                        if (node.ClassCode == null)
                        {
                            node.ClassCode = new Contract.FileSystemRegister.JPMC.FSClassCodeDto();
                        }
                        node.ClassCode.ClassCodeId = classCode;
                        node.ClassCode.CountryCode = countryCode;
                        node.ClassCode.RetentionType = retentionScheduleType;
                        node.ClassCode.StartDate = startDate;
                        node.ClassCode.TermUniqueId = classCodeId.ToString();
                        node.ClassCode.ApplyExistDocuments = applyExistDocument;
                        setting.NodeInfo = SerializerHelper.SerializeByDataContractSerializer(node);
                    }
                    setting.ClassCode = classCode;
                    setting.CountryCode = countryCode;
                    setting.RetentionScheduleType = retentionScheduleType;
                    setting.StartDate = startDate;
                    setting.ApplyExistDocument = applyExistDocument;
                    setting.DefaultTermId = classCodeId;
                }

                this.BatchUpdate(ctx, dbSettings);
                await ctx.SaveChangesAsync();
            }
        }

        public async Task UpdateRecordManagementStatus(Guid scopeId, int enableRecordManagement)
        {
            using (var ctx = GetNewContext())
            {
                var setting = ctx.RMFileSystemSettings.Where(s => s.ScopeId == scopeId).FirstOrDefault();
                if (setting != null)
                {
                    setting.EnableRecordManagement = enableRecordManagement;
                    await this.UpdateAsync(setting);
                }
            }
        }
        public Guid GetTermSetIdFromScopeId(Guid scopeId)
        {
            using (var ctx = GetNewContext())
            {
                var termSetId = ctx.RMFileSystemSettings.Where(s => s.ScopeId == scopeId).Select(s => s.TermSetId).FirstOrDefault();
                return termSetId;
            }
        }

        public List<RMFileSystemSetting> LoadAllConnectionSettingsUnderGroup(Guid groupId, IEnumerable<string> connectionPaths)
        {
            if (connectionPaths == null || !connectionPaths.Any()) return new List<RMFileSystemSetting>();
            using (var ctx = GetNewContext())
            {
                return ctx.RMFileSystemSettings.Where(setting => setting.ConnectionGroupId == groupId && connectionPaths.Any(path => setting.FullPath.Equals(path))).ToList();
            }
        }
        public List<RMFileSystemSetting> LoadAllSettingsByScopeIds(List<Guid> scopeIds)
        {
            using (var ctx = GetNewContext())
            {
                return ctx.RMFileSystemSettings.Where(s => scopeIds.Contains(s.ScopeId)).ToList();
            }
        }

        public async Task RemoveDescendantsSettingAsync(RMFSTreeNode node, string profileIdPath)
        {
            if (node.EnableRecordManagement != (int)EnableRecordManagementSetting.Enable)
            {
                ScheduleService.DeleteSchedules(ScheduleType.FSDisposalSchedule, profileIdPath);

                string deleteDescendantsSql = string.Empty;
                List<SqlParameter> sqlParams = new List<SqlParameter>();

                if (node.Level == (int)NodeLevel.WebApplication)
                {
                    deleteDescendantsSql = "DELETE FROM {0}.[RMFileSystemSettings] WHERE ConnectionGroupId = @connGroupId AND ScopeId <> @scopeId";
                    sqlParams.Add(new SqlParameter("@connGroupId", node.ConnGroupId));
                    sqlParams.Add(new SqlParameter("@scopeId", node.Id));
                }
                else
                {
                    string fullPathPre = node.FullPath + @"\%";

                    deleteDescendantsSql = "DELETE FROM {0}.[RMFileSystemSettings] WHERE ConnectionGroupId = @connGroupId AND FullPath LIKE @fullPathPre";
                    sqlParams.Add(new SqlParameter("@connGroupId", node.ConnGroupId));
                    sqlParams.Add(new SqlParameter("@fullPathPre", fullPathPre));
                }

                int result = 0;
                using (var context = RMDBContextManager.GetNewDBContext())
                {
                    GCommon.Utility.SecurityUtils.SanitizeSQLSchemaName(context.SchemaName);

                    var sql = string.Format(deleteDescendantsSql, context.SchemaName);

                    using (var tran = context.Database.BeginTransaction())
                    {
                        result = await context.Database.ExecuteSqlCommandAsync(sql, sqlParams.ToArray());
                        tran.Commit();
                    }
                }
            }
        }

        //test
        public async Task<RMFileSystemSetting> LoadFSSettingAsync(Guid scpoeId, Guid connGId)
        {
            using (var context = GetNewContext())
            {
                if (connGId == Guid.Empty)
                {
                    return null;
                }

                return await context.RMFileSystemSettings
                    .AsQueryable()
                    .Where(s => s.ScopeId.Equals(scpoeId) && s.ConnectionGroupId.Equals(connGId))
                    .FirstOrDefaultAsync();
            }
        }
        public async Task<List<string>> GetAllDisableRecordManagementPathAsync(Guid groupId)
        {
            using (var ctx = GetNewContext())
            {
                var disableNodes = await ctx.RMFileSystemSettings
                    .Where(o => o.EnableRecordManagement == (int)RMFSTreeNode.EnableRecordManagementSetting.Disable && o.ConnectionGroupId == groupId)
                    .ToListAsync();
                return disableNodes.Select(d => d.FullPath).ToList();
            }
        }
        public async Task<List<KeyValuePair<string, bool>>> GetAllNodeRCCSettingsAsync(Guid groupId)
        {
            using (var ctx = GetNewContext())
            {
                var rows = await ctx.RMFileSystemSettings
                    .Where(o => o.ConnectionGroupId == groupId && !string.IsNullOrEmpty(o.FullPath))
                    .OrderByDescending(o => o.FullPath)
                    .Select(o => new
                    {
                        o.FullPath,
                        IsAllow = o.IsAllowUserDownloadRCCReport
                    })
                    .ToListAsync();

                return rows.Select(x => new KeyValuePair<string, bool>(x.FullPath, x.IsAllow)).ToList();
            }
        }
        public async Task<bool> IsConnGroupEnableDownloadRCCAsync(Guid groupId)
        {
            using (var ctx = GetNewContext())
            {
                var setting = await ctx.RMFileSystemSettings.Where(c => c.ScopeId == groupId).FirstOrDefaultAsync();
                return setting?.IsAllowUserDownloadRCCReport ?? false;
            }
        }
        public async Task<List<KeyValuePair<string, bool>>> GetAllDeactivePathAsync(Guid groupId)
        {
            using (var ctx = GetNewContext())
            {
                var rows = await ctx.RMFileSystemSettings
                    .Where(o => o.ConnectionGroupId == groupId && !string.IsNullOrEmpty(o.FullPath) && !o.IsActive)
                    .OrderByDescending(o => o.FullPath)
                    .Select(o => new
                    {
                        o.FullPath,
                        IsAllow = o.IsActive
                    })
                    .ToListAsync();

                return rows.Select(x => new KeyValuePair<string, bool>(x.FullPath, x.IsAllow)).ToList();
            }
        }
    }
}
