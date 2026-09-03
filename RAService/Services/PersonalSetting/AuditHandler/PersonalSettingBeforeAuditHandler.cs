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
using AvePoint.RA.Common;
using AvePoint.RA.Common.Audit;
using AvePoint.RA.Contract.PersonalSetting;
using AvePoint.RA.Contract.RMWeb.Audit;
using AvePoint.RA.DB.Dao;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AvePoint.RA.Service.Services.PersonalSetting.AuditHandler
{
    public class PersonalSettingBeforeAuditHandler : IBeforeAuditHandler
    {
        private IPersonalSettingDao PersonalSettingDao => PlatformWindsorManager.GetService<IPersonalSettingDao>();
        private IRMSecurityGroupDao SecurityGroupDao => PlatformWindsorManager.GetService<IRMSecurityGroupDao>();

        public async Task<RMAuditInfo> CollectAsync(int model, int category, int action, object[] args, object target)
        {
            var info = new RMAuditInfo()
            {
                ModifyContent = new List<AuditItem>(),
                Module = (AuditModule)model,
                Category = (AuditCategory)category,
                Action = (AuditAction)action
            };

            switch (info.Action)
            {
                case AuditAction.SaveSearchCriteria:
                    HandleSaveSearchCriteriaAction(info, args[0]);
                    break;
                case AuditAction.SetSearchCriteriaAsDefault:
                    HandleCreateSearchCriteriaAction(info, args[0]);
                    break;
                case AuditAction.DeleteSearchCriteria:
                    HandleDeleteSearchCriteriaAction(info, args[0]);
                    break;
                case AuditAction.ShareSearchCriteria:
                    HandleShareSearchCriteriaAction(info, args[0]);
                    break;
                case AuditAction.CancelShareSearchCriteria:
                    HandleCancelShareSearchCriteriaAction(info, args[1]);
                    break;
                default:
                    break;
            }
            return info;
        }

        private void HandleSaveSearchCriteriaAction(RMAuditInfo auditInfo, object arg)
        {
            var dto = arg as RMPersonalSettingDto;
            if (dto.Id == 0) // if create new
            {
                auditInfo.Action = AuditAction.CreateSearchCriteria;
                return; 
            }

            var old = PersonalSettingDao.GetById(dto.Id);
            if (old == null) return;
            auditInfo.ModifyContent.Add(new AuditItem
            {
                TargetSetting = RMPersonalSettingConst.TargetSetting_Name,
                OldValue = old.Name,
            });

            auditInfo.ModifyContent.Add(new AuditItem
            {
                TargetSetting = RMPersonalSettingConst.TargetSetting_IsDefault,
                OldValue = old.IsDefault ? RMPersonalSettingConst.TargetSetting_IsDefaultYes : RMPersonalSettingConst.TargetSetting_IsDefaultNo,
            });

            auditInfo.ModifyContent.Add(new AuditItem
            {
                TargetSetting = RMPersonalSettingConst.TargetSetting_Content,
                OldValue = old.ContentStr,
            });

            auditInfo.Object = old.Name;
        }

        private void HandleCreateSearchCriteriaAction(RMAuditInfo auditInfo, object arg)
        {
            var dto = arg as RMPersonalSettingDto;
            auditInfo.ModifyContent.Add(new AuditItem
            {
                TargetSetting = RMPersonalSettingConst.TargetSetting_IsDefault,
                OldValue = RMPersonalSettingConst.TargetSetting_IsDefaultNo,
            });

            auditInfo.Object = dto.Name;
        }
        private void HandleDeleteSearchCriteriaAction(RMAuditInfo auditInfo, object arg)
        {
            var dto = arg as RMPersonalSettingDto;
            var old = PersonalSettingDao.GetById(dto.Id, false);
            if (old == null) return;
            auditInfo.Object = old.Name;
        }

        private void HandleShareSearchCriteriaAction(RMAuditInfo auditInfo, object arg)
        {
            var dto = arg as RMPersonalSettingSecurityGroupMappingDto;
            HandleShareInfo(auditInfo, dto.Id);
        }

        private void HandleCancelShareSearchCriteriaAction(RMAuditInfo auditInfo, object arg)
        {
            var id = (int)arg;
            HandleShareInfo(auditInfo, id);
        }

        private void HandleShareInfo(RMAuditInfo auditInfo, int id)
        {
            var old = PersonalSettingDao.GetById(id, false);
            if (old == null) return;
            auditInfo.Object = old.Name;
            var groupIds = PersonalSettingDao.GetSharedGroups(old.Id);
            var groups = SecurityGroupDao.GetGroupNames(groupIds);
            auditInfo.ModifyContent.Add(new AuditItem
            {
                TargetSetting = RMPersonalSettingConst.ShareToGroups,
                OldValue = groups?.Count > 0? string.Join(", ", groups): RMPersonalSettingConst.Audit_None
            });
        }
    }
}
