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
using AvePoint.Hybrid.Contract.Object;
using AvePoint.Hybrid.Contract.SignalR;
using AvePoint.RA.Common;
using AvePoint.RA.Common.Audit;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.RMWeb.Audit;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Dao.Extension;
using AvePoint.RA.DB.Dao.Impl;
using AvePoint.RA.Service.Services.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AvePoint.RA.Service.Services.ControlPanel.AuditHandler
{
    public class AppManagementAfterAuditHandler : IAfterAuditHandler
    {
        private RALogger logger = RALogger.GetInstance(typeof(AppManagementAfterAuditHandler));
        public static string TargetSetting_AgentName = "RM_CP_Audit_AGM_Agent_Name";
        public static string TargetSetting_ClientId = "RM_CP_Audit_AGM_App_Client_Id";

        public static string TargetSetting_AgentDescription = "RM_CP_Audit_AGM_Agent_Description";
        public static string TargetSetting_AgentStatus = "RM_CP_Audit_AGM_Agent_Status";
        public static string TargetSetting_AgentConfigFile = "RM_CP_Audit_AGM_Agent_Configuration_File";
        public static string TargetSetting_Certificate = "RM_CP_Audit_AGM_App_Certificate";
    public static string TargetSetting_AgentUpgradeResult = "RM_CP_Audit_AGM_Agent_Upgrade_Result";
        public static string TargetSetting_AgentSelectDataCenter = "RM_FS_Register_Agent_DC";

        private IRMCertificateDao  RMCertificateDao => PlatformWindsorManager.GetService<IRMCertificateDao>();
        private IRMKeyValueDao RMKeyValueDao => PlatformWindsorManager.GetService<IRMKeyValueDao>();



        public async Task<RMAuditInfo> CollectAsync(RMAuditInfo info, int model, int category, int action, object[] args, object target, object returnValue)
        {
            RMAuditInfo auditInfo = info != null ? info : new RMAuditInfo() { Action = (AuditAction)action };
            auditInfo.ModifyContent = auditInfo.ModifyContent ?? new List<AuditItem>();
            auditInfo.Module = (AuditModule)model;
            auditInfo.Category = (AuditCategory)category;
            //auditInfo.Action = (AuditAction)action;
            switch (auditInfo.Action)
            {
                case AuditAction.AddClientId:
                    HandleAddClientIdAction(info, args[0], returnValue);
                    break;
                case AuditAction.EditClientId:
                    HandleEditClientIdAction(info, args[0], returnValue);
                    break;
                case AuditAction.DownloadAgentConfigFile:
                    HandleDownloadConfigAction(info, args[0], returnValue);
                    break;
                case AuditAction.CreateCertificate:
                    HandleCreateCertificateAction(info, args[0], returnValue);
                    break;
                case AuditAction.SetAsDefaultCertificate:
                    HandleSetAsDefaultCertificateAction(info, args[0], returnValue);
                    break;
                case AuditAction.DownloadCertficate:
                    HandleDownloadCertificateAction(info, args[0], returnValue);
                    break;
                case AuditAction.DeleteCertificate:
                    HandleDeleteCertificateAction(info, args[0], returnValue);
                    break;
                case AuditAction.UpdateCertificate2Agents:
                    HandleUpdateCertificate2AgentsAction(info, args[0], returnValue);
                    break;
                case AuditAction.RegisterAgent:
                    HandleAddAgentAction(auditInfo, args[0], returnValue);
                    break;
                case AuditAction.EditAgent:
                    HandleUpdateAgentAction(auditInfo, args[0], returnValue);
                    break;
                case AuditAction.EnableAgent:
                    HandleUpdateAgentStatusAction(auditInfo, args[0], ServiceStatus.Active, returnValue);
                    break;
                case AuditAction.DisableAgent:
                    HandleUpdateAgentStatusAction(auditInfo, args[0], ServiceStatus.Disabled, returnValue);
                    break;
                case AuditAction.DeleteAgent:
                    HandleUpdateAgentStatusAction(auditInfo, args[0], ServiceStatus.Deleted, returnValue);
                    break;
                case AuditAction.UpgradeAgent:
                    HandleUpgradeAgentAction(auditInfo, returnValue);
                    break;
            }

            return auditInfo;
        }

        private void HandleAddClientIdAction(RMAuditInfo auditInfo, object arg, object returnValue)
        {
            auditInfo.ModifyContent.Add(new AuditItem
            {
                TargetSetting = TargetSetting_ClientId,
                NewValue = arg.ToString(),
            });
            auditInfo.Object = arg.ToString();
            auditInfo.Status = Boolean.Parse(returnValue.ToString()) ? (int)AuditStatus.Successful : (int)AuditStatus.Failed;

        }

        private void HandleEditClientIdAction(RMAuditInfo auditInfo, object arg, object returnValue)
        {
            var item = auditInfo.ModifyContent.Where(a => a.TargetSetting != null && a.TargetSetting.Equals(TargetSetting_ClientId)).FirstOrDefault();
            if (item != null) { item.NewValue = arg.ToString(); }

            auditInfo.Status = Boolean.Parse(returnValue.ToString()) ? (int)AuditStatus.Successful : (int)AuditStatus.Failed;

        }

        private void HandleDownloadCertificateAction(RMAuditInfo auditInfo, object arg, object returnValue)
        {
            auditInfo.Status = returnValue != null ? (int)AuditStatus.Successful : (int)AuditStatus.Failed;
        }

        private void HandleCreateCertificateAction(RMAuditInfo auditInfo, object arg, object returnValue)
        {
            var certId = (Guid)returnValue;
            var cert = RMCertificateDao.Find(o => o.Id == certId);

            //var cert = arg as RMCertificateDto;

            auditInfo.Object = cert?.Thumbprint;
            auditInfo.Status = cert != null ? (int)AuditStatus.Successful : (int)AuditStatus.Failed;
        }

        private void HandleSetAsDefaultCertificateAction(RMAuditInfo auditInfo, object arg, object returnValue)
        {
            auditInfo.Status = ((bool)returnValue) == true ? (int)AuditStatus.Successful : (int)AuditStatus.Failed;
        }

        private void HandleDeleteCertificateAction(RMAuditInfo auditInfo, object arg, object returnValue)
        {
            auditInfo.Status = ((bool)returnValue)== true ? (int)AuditStatus.Successful : (int)AuditStatus.Failed;
        }

        private void HandleUpdateCertificate2AgentsAction(RMAuditInfo auditInfo, object arg, object returnValue)
        {
            logger.Info($"return value is {returnValue}");
            var result = returnValue as List<AgentCertificateUpdateResult>;
            foreach(var r in result)
            {
                logger.Info($"Update agent is {r}, {r.AgentId},{r.AgentName},{r.Message},{r.Result}");
            }
            var succeedAgents = result.Where(o => o.Result == AgentCertificateUpdateResultEnum.Succeed).Select(o => o.AgentName).ToList();

            auditInfo.ModifyContent.Add(new AuditItem
            {
                TargetSetting = TargetSetting_AgentName,
                NewValue = string.Join(", ", succeedAgents)
            });
            auditInfo.Status = succeedAgents.Count > 0? (int)AuditStatus.Successful : (int)AuditStatus.Failed;
        }


        private void HandleDownloadConfigAction(RMAuditInfo auditInfo, object arg, object returnValue)
        {
            var agent = arg as RMAgentDto;
            var config = returnValue as AgentConfigurtion;

            auditInfo.ModifyContent.Add(new AuditItem
            {
                TargetSetting = TargetSetting_AgentConfigFile,
                NewValue = agent?.Name,
            });
            auditInfo.Object = agent?.Name;
            auditInfo.Status = config != null ? (int)AuditStatus.Successful : (int)AuditStatus.Failed;
        }

        private void HandleAddAgentAction(RMAuditInfo auditInfo, object arg, object returnValue)
        {
            var dto = arg as RMAgentDto;
            if(!AppManagementAuditUtil.IsAuditRequired(dto.DCInternalName))
            {
                auditInfo.NotNeedRecordAudit = true;
                return;
            }
            var isEnableJPMCFeature = RMKeyValueDao.IsEnableJPMCFileSystemFeature();
            auditInfo.ModifyContent.Add(new AuditItem
            {
                TargetSetting = TargetSetting_AgentName,
                NewValue = dto.Name,
            });
            auditInfo.ModifyContent.Add(new AuditItem
            {
                TargetSetting = TargetSetting_AgentDescription,
                NewValue = dto.Description,
            });
            auditInfo.ModifyContent.Add(new AuditItem
            {
                TargetSetting = TargetSetting_AgentSelectDataCenter,
                NewValue = AppManagementAuditUtil.GetDataCenter(dto.DCInternalName).GetAwaiter().GetResult(),
            });
            if (isEnableJPMCFeature)
            {
                auditInfo.ModifyContent.Add(new AuditItem
                {
                    TargetSetting = "RM_CP_Agent_Column_CollectLog_Enable",
                    NewValue = YesOrNoString(dto.CollectLog),
                });
            }
            auditInfo.Object = dto.Name;
            bool success = false;
            if (returnValue is bool boolValue)
            {
                success = boolValue;
            }
            else if (returnValue is string stringValue &&
                     (stringValue == "0" || stringValue.Equals("true", StringComparison.OrdinalIgnoreCase)))
            {
                success = stringValue == "0";
            }
            else if (returnValue is Guid guidValue)
            {
                success = guidValue != Guid.Empty;
            }

            auditInfo.Status = success ? (int)AuditStatus.Successful : (int)AuditStatus.Failed;
        }

        private void HandleUpgradeAgentAction(RMAuditInfo auditInfo, object returnValue)
        {
            if (returnValue is (List<RMAgentDto> dtos, Contract.Object.RMAgentUpgradeResult upgradeResult))
            {
                string agentNames = (dtos != null && dtos.Any()) ? string.Join(", ", dtos.Select(d => d.Name)) : ""; 
                auditInfo.Object = agentNames;

                auditInfo.Status = (upgradeResult == Contract.Object.RMAgentUpgradeResult.Success)
                    ? (int)AuditStatus.Successful
                    : (int)AuditStatus.Failed;
            }
            else
            {
                auditInfo.Status = (int)AuditStatus.Failed;
            }
        }

        private void HandleUpdateAgentAction(RMAuditInfo auditInfo, object arg, object returnValue)
        {
            var dto = arg as RMAgentDto;
            if (!AppManagementAuditUtil.IsAuditRequired(dto.DCInternalName))
            {
                auditInfo.NotNeedRecordAudit = true;
                return;
            }
            var isEnableJPMCFeature = RMKeyValueDao.IsEnableJPMCFileSystemFeature();
            var nameEditItem = auditInfo.ModifyContent.Where(a => a.TargetSetting != null && a.TargetSetting.Equals(TargetSetting_AgentName)).FirstOrDefault();
            if (nameEditItem != null) { nameEditItem.NewValue = dto.Name; }

            var descEditItem = auditInfo.ModifyContent.Where(a => a.TargetSetting != null && a.TargetSetting.Equals(TargetSetting_AgentDescription)).FirstOrDefault();
            if (descEditItem != null) { descEditItem.NewValue = dto.Description; }

            var selectedDC = auditInfo.ModifyContent.Where(a => a.TargetSetting != null && a.TargetSetting.Equals(TargetSetting_AgentSelectDataCenter)).FirstOrDefault();
            if (selectedDC != null) { selectedDC.NewValue = AppManagementAuditUtil.GetDataCenter(dto.DCInternalName).GetAwaiter().GetResult(); }

            if (isEnableJPMCFeature)
            {
                var collectLogEditItem = auditInfo.ModifyContent.Where(a => a.TargetSetting != null && a.TargetSetting.Equals("RM_CP_Agent_Column_CollectLog_Enable")).FirstOrDefault();
                if (collectLogEditItem != null) { collectLogEditItem.NewValue = YesOrNoString(dto.CollectLog); }
            }

            //auditInfo.Object = dto.Name;
            bool success = false;
            if (returnValue is bool boolValue)
            {
                success = boolValue;
            }
            else if (returnValue is string stringValue &&
                     (stringValue == "0" || stringValue.Equals("true", StringComparison.OrdinalIgnoreCase)))
            {
                success = stringValue == "0";
            }

            auditInfo.Status = success ? (int)AuditStatus.Successful : (int)AuditStatus.Failed;
        }

        private void HandleUpdateAgentStatusAction(RMAuditInfo auditInfo, object arg, ServiceStatus status, object returnValue)
        {
            var item = auditInfo.ModifyContent.Where(a => a.TargetSetting != null && a.TargetSetting.Equals(TargetSetting_AgentStatus)).FirstOrDefault();
            if (item != null) {
                item.NewValue = status.GetI18NKey();
            }
            auditInfo.Status = Boolean.Parse(returnValue.ToString()) ? (int)AuditStatus.Successful : (int)AuditStatus.Failed;
        }
        private string YesOrNoString(bool boolValue)
        {
            return boolValue ? "RM_JS_Common_Yes" : "RM_JS_Common_No";
        }
    }
}
