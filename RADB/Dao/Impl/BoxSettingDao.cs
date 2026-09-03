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
using AvePoint.RA.Common.Global.Utils;
using AvePoint.RA.Contract.Box;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.RMWeb.Tree.Base;
using AvePoint.RA.DB.Model;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AvePoint.RA.DB.Dao.Impl
{
    public class BoxSettingDao : BaseDao<RMBoxSetting>, IBoxSettingDao
    {
        public IRecordOwnerDao RecordOwnerDao { get; set; }

        public IAccountDao AccountDao { get; set; }

        /// <summary>
        /// Save or update setting 
        /// 1 query out whether have exist setting based on the node
        /// 2 init the setting object based the BoxSetting dto
        /// 3 Save or update to Database
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        public async Task UpdateOrCreateSettingAsync(BoxSettingDto dto)
        {
            var node = dto.SelectedNode;
            var connectionGroupId = node.Level == RMNodeLevel.BoxConnectionGroup ? node.Id : node.ContainerId;
            EnsureTermName(dto);
            using (var context = GetNewContext())
            {
                RMBoxSetting existSetting = GetSettingByScopeIdAndGroupId(node.Id, connectionGroupId);

                if (existSetting != null)
                {
                    existSetting.DefaultTermId = dto.DefaultTermId;
                    existSetting.DefaultTermName = dto.DefaultTermName;
                    existSetting.FullPath = node.FullPath;
                    existSetting.TermId = dto.TermId;
                    existSetting.TermName = dto.TermName;
                    existSetting.TermSetId = dto.TermSetId;
                    existSetting.TermSetName = dto.TermSetName;
                    existSetting.SettingTime += 1;
                    existSetting.NodeInfo = SerializerHelper.SerializeByDataContractSerializer(node);
                    existSetting.NeedCheckDefaultValue = dto.NeedCheckDefaultValue;
                    existSetting.ApplyExistType = dto.NeedCheckDefaultValue == true ? dto.ApplyExistType : 0;
                    existSetting.IsActive = dto.IsActive;
                    existSetting.DeployTermMethod = (int)dto.DeployTermMethod;
                    existSetting.AutoClassificationRules = dto.AutoClassificationRules == null ?
                        null : SerializerHelper.SerializeByDataContractSerializer(dto.AutoClassificationRules);
                    existSetting.RunAutoFullJob = dto.RunAutoFullJob;
                    existSetting.ScopeId = node.Id;
                    existSetting.ConnectionGroupId = new Guid(connectionGroupId);
                    existSetting.AutoJobOption = (int)dto.AutoJobOption;
                    existSetting.UserId = TryGetUserNodeId(node);
                    existSetting.ConnectionId = new Guid(node.ConnectionId);
                    existSetting.FolderId = node.Level == RMNodeLevel.BoxFolder ? node.Id : "";
                    existSetting.ApprovalType = (ApprovalType)dto.ApprovalType;
                    existSetting.WorkflowReferenceId = dto.WorkflowReferenceId;
                    existSetting.EMailToRecordOwner = dto.EMailToRecordOwner;
                    await this.UpdateAsync(existSetting);
                    await RecordOwnerDao.UpdateRecordOwnersAsync(existSetting.Id, dto.RecordOwner, RecordOwnerSettingType.Box);
                }
                else
                {
                    RMBoxSetting settings = new RMBoxSetting()
                    {
                        DefaultTermId = dto.DefaultTermId,
                        DefaultTermName = dto.DefaultTermName,
                        FullPath = node.FullPath,
                        TermId = dto.TermId,
                        TermName = dto.TermName,
                        TermSetId = dto.TermSetId,
                        TermSetName = dto.TermSetName,
                        SettingTime = 0,
                        NeedCheckDefaultValue = dto.NeedCheckDefaultValue,
                        ApplyExistType = dto.NeedCheckDefaultValue == true ? dto.ApplyExistType : 0,
                        IsActive = dto.IsActive,
                        NodeInfo = SerializerHelper.SerializeByDataContractSerializer(node),
                        DeployTermMethod = (int)dto.DeployTermMethod,
                        AutoClassificationRules = dto.AutoClassificationRules == null ?
                            null : SerializerHelper.SerializeByDataContractSerializer(dto.AutoClassificationRules),
                        RunAutoFullJob = dto.RunAutoFullJob,
                        AutoJobOption = (int)dto.AutoJobOption,
                        ScopeId = node.Id,
                        ConnectionGroupId = new Guid(connectionGroupId),
                        UserId = TryGetUserNodeId(node),
                        ConnectionId = new Guid(node.ConnectionId),
                        FolderId = node.Level == RMNodeLevel.BoxFolder ? node.Id : "",
                        EMailToRecordOwner = dto.EMailToRecordOwner,
                        ApprovalType = (ApprovalType)dto.ApprovalType,
                        WorkflowReferenceId = dto.WorkflowReferenceId,
                    };
                    context.RMBoxSettings.Add(settings);
                    context.SaveChanges();
                    existSetting = GetSettingByScopeIdAndGroupId(node.Id, connectionGroupId);
                    ArgumentNullException.ThrowIfNull(existSetting);
                    await RecordOwnerDao.AddRecordOwnersAsync(existSetting.Id, dto.RecordOwner, RecordOwnerSettingType.Box);
                }
            }
        }
        public RMBoxSetting GetSettingByScopeIdAndGroupId(string scopeId, string connGroupId)
        {
            RMBoxSetting setting = null;
            using (var context = GetNewContext())
            {
                setting = context.RMBoxSettings.AsQueryable().Where(s => s.ScopeId.Equals(scopeId) && s.ConnectionGroupId.Equals(new Guid(connGroupId))).FirstOrDefault();
            }
            return setting;
        }

        public RMBoxSetting GetSettingByConnGroupId(Guid connGroupId)
        {
            using (var context = GetNewContext())
            {
                RMBoxSetting setting = context.RMBoxSettings.AsNoTracking().Where(s => s.ConnectionGroupId.Equals(connGroupId)).FirstOrDefault();
                return setting;
            }
        }

        public RMBoxSetting GetSettingByScopeId(string scopeId)
        {
            using (var context = GetNewContext())
            {
                RMBoxSetting setting = context.RMBoxSettings.AsQueryable().Where(s => s.ScopeId.Equals(scopeId)).FirstOrDefault();
                return setting;
            }
        }

        public async Task DeleteSettingAsync(string id, Guid connGroupId)
        {
            using (var context = GetNewContext())
            {
                RMBoxSetting setting = context.RMBoxSettings.AsQueryable().Where(s => s.ScopeId.Equals(id) && s.ConnectionGroupId.Equals(connGroupId)).FirstOrDefault();
                if (setting != null)
                {
                    context.RMBoxSettings.Remove(setting);
                    await RecordOwnerDao.BatchDeleteAsync(o => o.SPSettingId == setting.Id && o.SettingType == (int)RecordOwnerSettingType.Box);
                    context.SaveChanges();
                }
            }
        }

        public List<RMBoxSetting> LoadAllSetting()
        {
            using (var ctx = GetNewContext())
            {
                return ctx.RMBoxSettings.ToList();
            }
        }

        public List<RMBoxSetting> LoadAllSettingsUnderGroup(Guid groupId)
        {
            using (var ctx = GetNewContext())
            {
                return ctx.RMBoxSettings.Where(s => s.ConnectionGroupId == groupId).ToList();
            }
        }

        public bool TryGet(string scopeId, string containerId, string connectionId, string userId, out RMBoxSetting settingInfo)
        {
            using (var context = GetNewContext())
            {
                settingInfo = context.RMBoxSettings.FirstOrDefault(item => item.ScopeId == scopeId &&
                                                                    item.ConnectionGroupId.ToString() == containerId &&
                                                                    (string.IsNullOrEmpty(connectionId) || item.ConnectionId.ToString() == connectionId) &&
                                                                    (string.IsNullOrEmpty(userId) || item.UserId == userId));
                return settingInfo != null;
            }
        }

        private void EnsureTermName(BoxSettingDto dto)
        {
            if (!string.IsNullOrEmpty(dto.TermName) && dto.TermName.Contains(":"))
            {
                dto.TermName = dto.TermName.Substring(dto.TermName.LastIndexOf(":") + 1);
            }
            if (!string.IsNullOrEmpty(dto.DefaultTermName) && dto.DefaultTermName.Contains(":"))
            {
                dto.DefaultTermName = dto.DefaultTermName.Substring(dto.DefaultTermName.LastIndexOf(":") + 1);
            }
        }

        private string TryGetUserNodeId(BoxTreeNode node)
        {
            if (node.Level != RMNodeLevel.BoxFolder && node.Level != RMNodeLevel.BoxUser)
                return string.Empty;
            if (node.Level == RMNodeLevel.BoxUser)
                return node.Id;
            var nodeParent = node.Parent;
            while (nodeParent.Level != RMNodeLevel.BoxUser)
            {
                nodeParent = nodeParent.Parent;
            }
            return nodeParent.Id;
        }

        public List<GCommon.Contract.StorageOptimization.Object.UserInfo> GetRecordOwnersBySettingId(int settingId)
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
    }
}

