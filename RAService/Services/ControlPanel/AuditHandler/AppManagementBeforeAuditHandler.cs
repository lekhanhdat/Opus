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
using AvePoint.RA.Common.Security;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Common;
using AvePoint.RA.Contract.FileSystemRegister;
using AvePoint.RA.Contract.Multi_Geo;
using AvePoint.RA.Contract.Multi_Geo.Enum;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.RMWeb.Audit;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Dao.Extension;
using AvePoint.RA.DB.Dao.Impl;
using AvePoint.RA.I18N.Core;
using AvePoint.RA.Service.Services.Extensions;
using AvePoint.RA.Service.Services.Multi_Geo;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AvePoint.RA.Service.Services.ControlPanel.AuditHandler
{
    public class AppManagementBeforeAuditHandler : IBeforeAuditHandler
    {

        public IRMAgentDao RMAgentDao => PlatformWindsorManager.GetService<IRMAgentDao>();
        public IKeyValueService KeyValueService => PlatformWindsorManager.GetService<IKeyValueService>();
        private IRMCertificateDao  RMCertificateDao => PlatformWindsorManager.GetService<IRMCertificateDao>();
        private IRMKeyValueDao RMKeyValueDao => PlatformWindsorManager.GetService<IRMKeyValueDao>();
        private IMultiGeoDataCenterService MultiGeoDataCenterService => PlatformWindsorManager.GetService<IMultiGeoDataCenterService>();

        private readonly string optionSelectDC = "RM_FS_Register_Agent_DC";
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
                case AuditAction.AddClientId:
                    HandleClientIdAction(info, args[0]);
                    break;
                case AuditAction.DownloadAgentConfigFile:
                    HandleDownloadConfigAction(info, args[0]);
                    break;
                case AuditAction.SetAsDefaultCertificate:
                case AuditAction.DeleteCertificate:
                case AuditAction.UpdateCertificate2Agents:
                case AuditAction.DownloadCertficate:
                    HandleGetCertificateThumbprintAction(info, args[0]);
                    break;
                case AuditAction.RegisterAgent:
                    break;
                case AuditAction.EditAgent:
                    HandleUpdateAgentAction(info, args[0]);
                    break;
                case AuditAction.EnableAgent:
                case AuditAction.DisableAgent:
                case AuditAction.DeleteAgent:
                    HandleUpdateAgentStatusAction(info, args[0]);
                    break;
                case AuditAction.UpgradeAgent:
                    break;
            }

            return info;
        }

        private void HandleClientIdAction(RMAuditInfo auditInfo, object arg)
        {
            var old = KeyValueService.Get(KeyNameCollection.AppManagementClientId, RMNameValueType.AppManagementClientId)?.Value;
            if (!string.IsNullOrEmpty(old))
            {
                auditInfo.Action = AuditAction.EditClientId;
                auditInfo.ModifyContent.Add(new AuditItem
                {
                    TargetSetting = AppManagementAfterAuditHandler.TargetSetting_ClientId,
                    OldValue = old,
                });
                auditInfo.Object = old;
            }
        }

        private void HandleGetCertificateThumbprintAction(RMAuditInfo auditInfo, object arg)
        {
            var certId = (Guid)arg;
            var cert = RMCertificateDao.Find(o => o.Id == certId);
            auditInfo.Object = cert?.Thumbprint;
        }

        private void HandleDownloadConfigAction(RMAuditInfo auditInfo, object arg)
        {

        }

        private void HandleUpdateAgentAction(RMAuditInfo auditInfo, object arg)
        {
            var dto = arg as RMAgentDto;
            if (!AppManagementAuditUtil.IsAuditRequired(dto.DCInternalName))
            {
                auditInfo.NotNeedRecordAudit = true;
                return;
            }
            var old = RMAgentDao.Find(o => o.Id == dto.Id).Convert2Dto();
            if (old == null) return;
            var isEnableJPMCFeature = RMKeyValueDao.IsEnableJPMCFileSystemFeature();
            auditInfo.ModifyContent.Add(new AuditItem
            {
                TargetSetting = AppManagementAfterAuditHandler.TargetSetting_AgentName,
                OldValue = old.Name,
            });
            auditInfo.ModifyContent.Add(new AuditItem
            {
                TargetSetting = AppManagementAfterAuditHandler.TargetSetting_AgentDescription,
                OldValue = old.Description,
            });
            auditInfo.ModifyContent.Add(new AuditItem
            {
                TargetSetting = AppManagementAfterAuditHandler.TargetSetting_AgentSelectDataCenter,
                OldValue = AppManagementAuditUtil.GetDataCenter(old.DCInternalName).GetAwaiter().GetResult(),
            });
            if (isEnableJPMCFeature)
            {
                auditInfo.ModifyContent.Add(new AuditItem
                {
                    TargetSetting = "RM_CP_Agent_Column_CollectLog_Enable",
                    OldValue = YesOrNoString(old.CollectLog),
                });
            }

            auditInfo.Object = old.Name;
        }

        private void HandleUpdateAgentStatusAction(RMAuditInfo auditInfo, object arg)
        {
            var id = Guid.Parse(arg.ToString());
            var old = RMAgentDao.Find(o => o.Id == id).Convert2Dto();
            if (old == null) return;
            auditInfo.ModifyContent.Add(new AuditItem
            {
                TargetSetting = AppManagementAfterAuditHandler.TargetSetting_AgentStatus,
                OldValue = old.Status.GetI18NKey()
            });

            auditInfo.Object = old.Name;
        }
        private string YesOrNoString(bool boolValue)
        {
            return boolValue ? "RM_JS_Common_Yes" : "RM_JS_Common_No";
        }
    }
}
