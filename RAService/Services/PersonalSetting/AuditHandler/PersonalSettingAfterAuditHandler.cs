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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AvePoint.RA.Service.Services.PersonalSetting.AuditHandler
{
    public class PersonalSettingAfterAuditHandler : IAfterAuditHandler
    {
        private IRMSecurityGroupDao SecurityGroupDao => PlatformWindsorManager.GetService<IRMSecurityGroupDao>();
        private IPersonalSettingDao PersonalSettingDao => PlatformWindsorManager.GetService<IPersonalSettingDao>();


        public async Task<RMAuditInfo> CollectAsync(RMAuditInfo info, int model, int category, int action, object[] args, object target, object returnValue)
        {
            RMAuditInfo auditInfo = info != null ? info : new RMAuditInfo() { Action = (AuditAction)action };
            auditInfo.ModifyContent = auditInfo.ModifyContent ?? new List<AuditItem>();
            auditInfo.Module = (AuditModule)model;
            auditInfo.Category = (AuditCategory)category;
            //auditInfo.Action = (AuditAction)action;

            switch (auditInfo.Action)
            {
                case AuditAction.CreateSearchCriteria:
                    HandleCreateSearchCriteriaAction(info, args[0], returnValue);
                    break;
                case AuditAction.SetSearchCriteriaAsDefault:
                    HandleSetSearchCriteriaAsDefaultAction(info, args[0], returnValue);
                    break;
                case AuditAction.SaveSearchCriteria:
                    HandleUpdateSearchCriteriaAction(info, args[0], returnValue);
                    break;
                case AuditAction.DeleteSearchCriteria:
                    HandleDeleteSearchCriteriaAction(info, args[0], returnValue);
                    break;
                case AuditAction.ShareSearchCriteria:
                    HandleShareSearchCriteriaAction(info, args[0]);
                    break;
                case AuditAction.CancelShareSearchCriteria:
                    HandleCancelShareSearchCriteriaAction(info);
                    break;
                case AuditAction.RunOfflineSearch:
                    int id = (int)args[0];
                    RMPersonalSettingDto profile = PersonalSettingDao.GetById(id, false);
                    if(profile != null)
                    {
                        auditInfo.ModifyContent.Add(new AuditItem
                        {
                            TargetSetting = RMPersonalSettingConst.TargetSetting_Name,
                            NewValue = profile.Name,
                        });
                    }
                    string ret = returnValue as string;
                    if(ret != null)
                    {
                        auditInfo.Status = (int)AuditStatus.Successful;
                    }
                    else
                    {
                        auditInfo.Status = (int)AuditStatus.Failed;
                    }
                    break;
                default:
                    break;
            }

            return auditInfo;

        }

        private void HandleUpdateSearchCriteriaAction(RMAuditInfo auditInfo, object arg, object returnValue)
        {
            var dto = arg as RMPersonalSettingDto;
            //auditInfo.Object = dto.Name;

            var nameEditItem = auditInfo.ModifyContent.Where(a => a.TargetSetting != null && a.TargetSetting.Equals(RMPersonalSettingConst.TargetSetting_Name)).FirstOrDefault();
            if (nameEditItem != null) { nameEditItem.NewValue = dto.Name; }

            var isDefaultEditItem = auditInfo.ModifyContent.Where(a => a.TargetSetting != null && a.TargetSetting.Equals(RMPersonalSettingConst.TargetSetting_IsDefault)).FirstOrDefault();
            if (isDefaultEditItem != null) { isDefaultEditItem.NewValue = dto.IsDefault ? RMPersonalSettingConst.TargetSetting_IsDefaultYes : RMPersonalSettingConst.TargetSetting_IsDefaultNo; }

            var contentEditItem = auditInfo.ModifyContent.Where(a => a.TargetSetting != null && a.TargetSetting.Equals(RMPersonalSettingConst.TargetSetting_Content)).FirstOrDefault();
            if (contentEditItem != null) { contentEditItem.NewValue = dto.ContentStr; }
            
            auditInfo.Status = int.TryParse(returnValue.ToString(), out int intReturnValue) ? (int)AuditStatus.Successful : (int)AuditStatus.Failed;
        }

        private void HandleCreateSearchCriteriaAction(RMAuditInfo auditInfo, object arg, object returnValue)
        {
            var dto = arg as RMPersonalSettingDto;
            auditInfo.ModifyContent.Add(new AuditItem
            {
                TargetSetting = RMPersonalSettingConst.TargetSetting_Name,
                NewValue = dto.Name,
            });

            auditInfo.ModifyContent.Add(new AuditItem
            {
                TargetSetting = RMPersonalSettingConst.TargetSetting_IsDefault,
                NewValue = dto.IsDefault? RMPersonalSettingConst.TargetSetting_IsDefaultYes: RMPersonalSettingConst.TargetSetting_IsDefaultNo,
            });

            auditInfo.ModifyContent.Add(new AuditItem
            {
                TargetSetting = RMPersonalSettingConst.TargetSetting_Content,
                NewValue = dto.ContentStr,
            });
            auditInfo.Object = dto.Name;
            auditInfo.Status = int.TryParse(returnValue.ToString(), out int intReturnValue) ? (int)AuditStatus.Successful : (int)AuditStatus.Failed;
        }

        private void HandleSetSearchCriteriaAsDefaultAction(RMAuditInfo auditInfo, object arg, object returnValue)
        {
            var dto = arg as RMPersonalSettingDto;
            bool.TryParse(returnValue.ToString(), out bool boolReturnValue);
            var isDefaultEditItem = auditInfo.ModifyContent.Where(a => a.TargetSetting != null && a.TargetSetting.Equals(RMPersonalSettingConst.TargetSetting_IsDefault)).FirstOrDefault();
            if (isDefaultEditItem != null)
            {
                isDefaultEditItem.NewValue = boolReturnValue ? RMPersonalSettingConst.TargetSetting_IsDefaultYes : RMPersonalSettingConst.TargetSetting_IsDefaultNo;
            }
            auditInfo.Status = boolReturnValue ? (int)AuditStatus.Successful : (int)AuditStatus.Failed;
        }

        private void HandleDeleteSearchCriteriaAction(RMAuditInfo auditInfo, object arg, object returnValue)
        {
            var dto = arg as RMPersonalSettingDto;
            auditInfo.ModifyContent.Add(new AuditItem
            {
                TargetSetting = RMPersonalSettingConst.TargetSetting_Name,
                NewValue = dto.Name,
            });
            //auditInfo.ModifyContent.Add(new AuditItem
            //{
            //    TargetSetting = RMPersonalSettingConst.TargetSetting_Status,
            //    NewValue = ServiceStatus.Deleted.GetI18NKey(),
            //});
            auditInfo.Status = Boolean.Parse(returnValue.ToString()) ? (int)AuditStatus.Successful : (int)AuditStatus.Failed;
        }

        private void HandleShareSearchCriteriaAction(RMAuditInfo auditInfo, object arg)
        {
            var dto = arg as RMPersonalSettingSecurityGroupMappingDto;
            var groups = SecurityGroupDao.GetGroupNames(dto.SecurityGroups);
            HandleShareSearchCriteriaActionInfo(auditInfo, string.Join(", ", groups));
        }

        private void HandleCancelShareSearchCriteriaAction(RMAuditInfo auditInfo)
        {
            auditInfo.Action = AuditAction.ShareSearchCriteria;
            HandleShareSearchCriteriaActionInfo(auditInfo, RMPersonalSettingConst.Audit_None);
        }

        private void HandleShareSearchCriteriaActionInfo(RMAuditInfo auditInfo, string newValue)
        {
            var nameEditItem = auditInfo.ModifyContent.Where(a => a.TargetSetting != null && a.TargetSetting.Equals(RMPersonalSettingConst.ShareToGroups)).FirstOrDefault();
            if (nameEditItem != null)
            {
                nameEditItem.NewValue = newValue;
            }
            auditInfo.Status = (int)AuditStatus.Successful;
        }
    }
}
