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
using AngleSharp.Dom;
using AvePoint.GCommon.Utility;
using AvePoint.RA.Common;
using AvePoint.RA.Common.Audit;
using AvePoint.RA.Contract.FileSystemRegister;
using AvePoint.RA.Contract.JPMC;
using AvePoint.RA.Contract.Myhub.Permission;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.RMWeb.Audit;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Dao.Extension;
using AvePoint.RA.DB.Dao.Impl;
using AvePoint.RA.Service.Services.RMSharePointSettings.AuditHandler;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.Service.Services.RMFileSystemSettings.AuditHandler
{
    public class FileSystemServiceAfterAuditHandler : IAfterAuditHandler
    {
        private IRMKeyValueDao RMKeyValueDao => PlatformWindsorManager.GetService<IRMKeyValueDao>();
        private ITermDao TermDao => PlatformWindsorManager.GetService<ITermDao>();
        private IFSConnectionDao FSConnectionDao => PlatformWindsorManager.GetService<IFSConnectionDao>();
        public async Task<RMAuditInfo> CollectAsync(RMAuditInfo info, int model, int category, int action, object[] args, object target, object returnValue)
        {
            var isEnableJPMCFeature = RMKeyValueDao.IsEnableJPMCFileSystemFeature();
            if (action == (int)AuditAction.EditFSConnection)
            {
                int resultCode = (int)returnValue;
                if (resultCode != 1)//TODO xwwang
                {
                    info.NotNeedRecordAudit = true;
                }
            }
            else if (action == (int)AuditAction.FSConnectionValidationTest)
            {
                bool success = (bool)returnValue;
                ConnectionDto dto = (ConnectionDto)args[0];
                info.Object = dto.UNCPath;
                info.Status = success ? (int)AuditStatus.Successful : (int)AuditStatus.Failed;
            }
            else if (action == (int)AuditAction.FSEditDocLevelSetting)
            {
                if (isEnableJPMCFeature)
                {
                    info.Action = AuditAction.FSEditDocLevelSettingForJPMC;
                }
                List<AuditItem> cretiaAudit = info.ModifyContent.Where(a => a.Id == ContentRepositoryAuditUtil.NeedReAuditorInAfter).ToList();
                if (cretiaAudit.Count > 0)
                {
                    RMFSTreeNode node = (RMFSTreeNode)args[0];
                    cretiaAudit[0].NewValue = ContentRepositoryAuditUtil.GetRulesCretiaString(node.AutoClassificationRules);
                }
            }
            else if (action == (int)AuditAction.ApplyClassCodeSettings4FS|| action == (int)AuditAction.MyhubClassify)
            {
                if (returnValue is string) {
                    if (info == null) {
                        info = new RMAuditInfo();
                    }
                    info.Object = (string)returnValue;
                    info.Status = (int)RAMessageType.Successful;
                    info.Module = (AuditModule)Enum.Parse(typeof(AuditModule), model.ToString());
                    info.Category = (AuditCategory)Enum.Parse(typeof(AuditCategory), category.ToString());
                    info.Action = (AuditAction)Enum.Parse(typeof(AuditAction), action.ToString());
                }
                else {
                    RAReturnMessage msg = (RAReturnMessage)returnValue;
                    if (msg != null)
                    {
                        info.Status = (int)msg.MessageType;
                    }
                }
            }

            else if (action == (int)AuditAction.RunFSCollectionJob)
            {
                #region AuditAction.RunFSCollectionJob
                var jobId = returnValue as string;
                //info.Category = AuditCategory.SharePointSettings;
                //info.Module = AuditModule.BusinessClassificationManagement;
                info.Module = AuditModule.BusinessClassificationManagement;
                info.Category = AuditCategory.SharePointSettings;
                //var node = ((RMSPTreeNode)args[0]);
                info.Object = jobId;
                #endregion
            }
            else if (action == (int)AuditAction.RunFSDisposalJob || action == (int)AuditAction.RunFSApplyClassCodeJob)
            {
                #region AuditAction.RunFSDisposalJob
                var jobId = returnValue as string;
                info.Category = AuditCategory.SharePointSettings;
                info.Module = AuditModule.BusinessClassificationManagement;
                info.Object = jobId;
                #endregion
            }
            else if (action == (int)AuditAction.RunFSClassCodeDisposalJob)
            {
                #region AuditAction.RunFSClassCodeDisposalJob
                var jobId = returnValue as string;
                info.Category = AuditCategory.SharePointSettings;
                info.Module = AuditModule.BusinessClassificationManagement;
                info.Object = jobId;

                var param = args[2] as string;
                if (!string.IsNullOrEmpty(param))
                {
                    var request = SerializerHelper.DeserializeByDataContractSerializer<FSDisposalByClassCodeRequest>(param);
                    if (request?.TermID != null && request.TermID.Count > 0)
                    {
                        var classCodeNames = request.TermID
                            .Select(termId => TermDao.GetRMTermByUniqueId(termId)?.Name)
                            .Where(name => !string.IsNullOrEmpty(name))
                            .ToList();

                        info.ModifyContent.Add(new AuditItem
                        {
                            NewValue = string.Join("; ", classCodeNames),
                        });
                    }
                }
                #endregion
            }
            else if (action == (int)AuditAction.RunFSRestoreJob)
            {
                #region AuditAction.RunFSRestoreJob
                var jobId = returnValue as string;
                info.Object = jobId;
                #endregion
            }
            else if (action == (int)AuditAction.ImportFSSetting)
            {
                info.Object = returnValue?.ToString();
            }
            else if (action == (int)AuditAction.ExportFSSetting)
            {
                info.Object = returnValue?.ToString();
            }
            else if (action == (int)AuditAction.GenerateRCCReport)
            {
                var isMyhub = JsonConvert.DeserializeObject<RCCReportRequest>(args[2].ToString()).IsMyHub;
                var displayName = JsonConvert.DeserializeObject<RCCReportRequest>(args[2].ToString()).DisplayName;
                if (isMyhub)
                {
                    info.Object = displayName;
                }
                else
                {
                    info.Object = returnValue?.ToString();
                }
            }
            else if (action == (int)AuditAction.PermissionChange)
            {
                info.Category = AuditCategory.FSMyhub;
                if (args[0] is RMConnectionRecordOwnerUpdateModel updateModel)
                {
                    info.Object = FSConnectionDao.GetConnectionById(updateModel.ConnectionId).Name;
                    List<AuditItem> cretiaAudit = info.ModifyContent;
                    if (cretiaAudit.Count > 0)
                    {
                        string newValue = string.Join(";", updateModel.RecordOwners.Select(n=>n.DisplayName).ToList());
                        cretiaAudit[0].NewValue = newValue;
                    }
                }
            }
            else
            {
                switch (action)
                {
                    case (int)AuditAction.CreateFSGroup:
                    case (int)AuditAction.EditFSGroup:
                        {
                            if (info != null && info.E != null)
                            {
                                info.Status = (int)AuditStatus.Failed;
                            }
                        }
                        break;
                    default:
                        break;
                }
            }
            return info;
        }
        private string YesOrNoString(bool boolValue)
        {
            return boolValue ? "RM_JS_Common_Yes" : "RM_JS_Common_No";
        }
    }
}
