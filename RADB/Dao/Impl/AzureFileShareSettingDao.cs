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
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.Schedule;
using AvePoint.RA.DB.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AvePoint.RA.DB.Dao.Impl
{
    public class AzureFileShareSettingDao : BaseDao<RMAzureFileShareSetting>, IAzureFileShareSettingDao
    {
        public IRecordOwnerDao RecordOwnerDao { get; set; }
        public IAccountDao AccountDao { get; set; }
        public IScheduleService ScheduleService { get; set; }

        public async Task SaveSettingAsync(AzureFileSettingDto dto, Guid connGId)
        {
            var node = dto.SelectedNode;
            EnsureTermName(dto);
            using (var context = GetNewContext())
            {
                var nodeId = new Guid(node.Id);
                RMAzureFileShareSetting setting = context.RMAzureFileShareSettings.AsQueryable().Where(s => s.ScopeId.Equals(nodeId) && s.ConnectionGroupId.Equals(connGId)).FirstOrDefault();
                if (setting != null)
                {
                    setting.DefaultTermId = dto.DefaultTermId;
                    setting.DefaultTermName = dto.DefaultTermName;
                    setting.FullPath = node.FullPath;
                    setting.ScopeId = nodeId;
                    setting.ConnectionGroupId = connGId;
                    setting.TermId = dto.TermId;
                    setting.TermName = dto.TermName;
                    setting.TermSetId = dto.TermSetId;
                    setting.TermSetName = dto.TermSetName;
                    setting.SettingTime = 0;
                    setting.NodeInfo = SerializerHelper.SerializeByDataContractSerializer(node);
                    setting.NeedCheckDefaultValue = dto.NeedCheckDefaultValue;
                    setting.ApplyExistType = dto.ApplyExistType;
                    setting.EMailToRecordOwner = dto.EMailToRecordOwner;
                    setting.IsNewEdited = true;
                    setting.IsActive = dto.IsActive;
                    setting.IdPath = ScheduleService.GetProfileId(node);
                    setting.DeployTermMethod = (int)dto.DeployTermMethod;
                    setting.AutoClassificationRules = dto.AutoClassificationRules == null ?
                        null : SerializerHelper.SerializeByDataContractSerializer(dto.AutoClassificationRules);
                    setting.RunAutoFullJob = dto.RunAutoFullJob;
                    setting.AutoJobOption = (int)dto.AutoJobOption;
                    setting.ApprovalType = (ApprovalType)dto.ApprovalType;
                    setting.WorkflowReferenceId = dto.WorkflowReferenceId;
                    await this.UpdateAsync(setting);
                    //RecordOwnerDao.UpdateRecordOwners(setting.Id, dto.RecordOwner, RecordOwnerSettingType.AzureFileShare);
                }
                else
                {
                    RMAzureFileShareSetting settings = new RMAzureFileShareSetting()
                    {
                        DefaultTermId = dto.DefaultTermId,
                        DefaultTermName = dto.DefaultTermName,
                        FullPath = node.FullPath,
                        ScopeId = nodeId,
                        ConnectionGroupId = connGId,
                        TermId = dto.TermId,
                        TermName = dto.TermName,
                        TermSetId = dto.TermSetId,
                        TermSetName = dto.TermSetName,
                        SettingTime = 0,
                        NeedCheckDefaultValue = dto.NeedCheckDefaultValue,
                        ApplyExistType = dto.ApplyExistType,
                        EMailToRecordOwner = dto.EMailToRecordOwner,
                        IsNewEdited = true,
                        IsActive = dto.IsActive,
                        IdPath = ScheduleService.GetProfileId(node),
                        NodeInfo = SerializerHelper.SerializeByDataContractSerializer(node),
                        DeployTermMethod = (int)dto.DeployTermMethod,
                        AutoClassificationRules = dto.AutoClassificationRules == null ?
                            null : SerializerHelper.SerializeByDataContractSerializer(dto.AutoClassificationRules),
                        RunAutoFullJob = dto.RunAutoFullJob,
                        AutoJobOption = (int)dto.AutoJobOption,
                        ApprovalType = (ApprovalType)dto.ApprovalType,
                        WorkflowReferenceId = dto.WorkflowReferenceId
                    };

                    context.RMAzureFileShareSettings.Add(settings);
                    context.SaveChanges();
                    //setting = context.RMAzureFileShareSettings.AsQueryable().Where(s => s.ScopeId == settings.ScopeId).FirstOrDefault();
                    //RecordOwnerDao.AddRecordOwners(setting.Id, node.RecordOwner, RecordOwnerSettingType.AzureFileShare);
                }
            }
        }

        public RMAzureFileShareSetting LoadSetting(Guid scpoeId, Guid connGId)
        {
            RMAzureFileShareSetting setting = null;
            using (var context = GetNewContext())
            {
                if (connGId != Guid.Empty)
                {
                    setting = context.RMAzureFileShareSettings.AsQueryable().Where(s => s.ScopeId.Equals(scpoeId) && s.ConnectionGroupId.Equals(connGId)).FirstOrDefault();
                }
            }
            return setting;
        }

        private void EnsureTermName(AzureFileSettingDto dto)
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

        public RMAzureFileShareSetting GetSetting(Guid scopeId)
        {
            using (var context = GetNewContext())
            {
                RMAzureFileShareSetting setting = context.RMAzureFileShareSettings.AsQueryable().Where(s => s.ScopeId.Equals(scopeId)).FirstOrDefault();
                return setting;
            }
        }

        public void DeleteAzureFileShareSetting(Guid id, Guid connGid)
        {
            using (var context = GetNewContext())
            {
                RMAzureFileShareSetting setting = context.RMAzureFileShareSettings.AsQueryable().Where(s => s.ScopeId.Equals(id) && s.ConnectionGroupId.Equals(connGid)).FirstOrDefault();
                if (setting != null)
                {
                    context.RMAzureFileShareSettings.Remove(setting);
                    //RecordOwnerDao.BatchDelete(o => o.SPSettingId == setting.Id && o.SettingType == (int)RecordOwnerSettingType.AzureFileShare);
                    context.SaveChanges();
                }
            }
        }

        public List<RMAzureFileShareSetting> LoadAllSetting()
        {
            using (var ctx = GetNewContext())
            {
                return ctx.RMAzureFileShareSettings.ToList();
            }
        }

        public List<RMAzureFileShareSetting> LoadAllSettingsUnderGroup(Guid groupId)
        {
            using (var ctx = GetNewContext())
            {
                return ctx.RMAzureFileShareSettings.Where(s => s.ConnectionGroupId == groupId).ToList();
            }
        }

        public bool TryGet(Guid scopeId, out RMAzureFileShareSetting settingInfo)
        {
            using(var context = GetNewContext())
            {
                settingInfo = context.RMAzureFileShareSettings.FirstOrDefault(item => item.ScopeId == scopeId);
                return settingInfo != null;
            }
        }

        public bool Has(Guid scopeId)
        {
            using(var context = GetNewContext())
            {
                return context.RMAzureFileShareSettings.Any(item => item.ScopeId == scopeId);
            }
        }
    }
}
