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
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Common.Audit;
using AvePoint.RA.Contract.JobMonitor;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.RMWeb.Audit;
using AvePoint.RA.Contract.RMWeb.ReportCenter;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Model;
using AvePoint.RA.I18N.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AvePoint.Common;
using AvePoint.GCommon.Utility;
using AvePoint.RA.Common;

namespace AvePoint.RA.Service.Services.LocationManagement.AuditHandler
{
    public class ContainerManagementAfterAuditHandler : IAfterAuditHandler
    {
        private RALogger logger = RALogger.GetInstance(typeof(ContainerManagementAfterAuditHandler));

        private IContainerDao ContainerDao => PlatformWindsorManager.GetService<IContainerDao>();

        public async Task<RMAuditInfo> CollectAsync(Contract.RMWeb.Audit.RMAuditInfo info, int model, int category, int action, object[] args, object target, object returnValue)
        {
            RMAuditInfo auditInfo = null;
            try
            {
                auditInfo = new RMAuditInfo();
                switch ((AuditAction)action)
                {
                    case AuditAction.CreateContainer:
                    case AuditAction.EditContainer:
                    case AuditAction.EditContainerDefault:
                        auditInfo.Module = (AuditModule)model;
                        auditInfo.Category = (AuditCategory)category;
                        auditInfo.Action = (AuditAction)action;
                        if ((AuditAction)action == AuditAction.CreateContainer)
                        {
                            auditInfo.Object = args[0] != null ? args[0].ToString() : string.Empty;
                        }
                        else if ((AuditAction)action == AuditAction.EditContainer)
                        {
                            auditInfo.Object = args[1] != null ? args[1].ToString() : string.Empty;
                        }
                        else if ((AuditAction)action == AuditAction.EditContainerDefault)
                        {
                            int id = int.Parse(args[0].ToString());
                            RMContainer temp = ContainerDao.GetContainerById(id);
                            auditInfo.Object = string.Empty;

                            if (info.ModifyContent != null && info.ModifyContent.Count != 0)
                            {
                                AuditItem defaultBoxTypeItem = info.ModifyContent.Where(a => a.TargetSetting != null && a.TargetSetting.Equals(I18NEntity.GetString("RM_RC_Audit_Action_EditContainerDefault"))).FirstOrDefault();
                                if (defaultBoxTypeItem != null) {
                                    List<RMContainer> defaultContainers = ContainerDao.GetDefaultContainers();
                                    string newDefaultBoxType = (defaultContainers != null && defaultContainers.Count > 0) ? defaultContainers[0].TypeName : "";
                                    defaultBoxTypeItem.NewValue = newDefaultBoxType;
                                }
                            }
                            auditInfo.ModifyContent = info != null && info.ModifyContent != null ? info.ModifyContent : auditInfo.ModifyContent;
                        }
                        if ((AuditAction)action == AuditAction.EditContainerDefault)
                        {
                            auditInfo.Status = Boolean.Parse(returnValue.ToString()) ? (int)AuditStatus.Successful : (int)AuditStatus.Failed;
                        }
                        else
                        {
                            var returnValueObj = Newtonsoft.Json.JsonConvert.DeserializeObject<RMContainer>(returnValue.ToString());
                            if (returnValueObj == null || returnValueObj.Id == 0)
                            {
                                //auditInfo.Status = (int)AuditStatus.Failed;
                                auditInfo.NotNeedRecordAudit = true;
                            }
                            else
                            {
                                auditInfo.Status = (int)AuditStatus.Successful;
                            }
                            if ((AuditAction)action == AuditAction.EditContainer)
                            {
                                if (info.ModifyContent != null && info.ModifyContent.Count != 0)
                                {
                                    int boxId = int.Parse(args[0].ToString());
                                    RMContainer box = ContainerDao.GetContainerById(boxId);
                                    AuditItem nameItem = info.ModifyContent.Where(a => a.TargetSetting != null && a.TargetSetting.Equals(I18NEntity.GetString("RM_JS_PRM_CZ_ContentType"))).FirstOrDefault();
                                    if (nameItem != null) { nameItem.NewValue = box.TypeName; }

                                    AuditItem sizeItem = info.ModifyContent.Where(a => a.TargetSetting != null && a.TargetSetting.Equals(I18NEntity.GetString("RM_JS_PRM_CZ_Size"))).FirstOrDefault();
                                    if (sizeItem != null) { box.Size = Math.Round(box.Size, 2); sizeItem.NewValue = box.Size.ToString(); }

                                    AuditItem descItem = info.ModifyContent.Where(a => a.TargetSetting != null && a.TargetSetting.Equals(I18NEntity.GetString("RM_JS_PRM_CZ_Description"))).FirstOrDefault();
                                    if (descItem != null) { descItem.NewValue = box.Description; }
                                }
                                auditInfo.ModifyContent = info != null && info.ModifyContent != null ? info.ModifyContent : auditInfo.ModifyContent;
                            }
                        }
                        break;
                    case AuditAction.DeleteContainer:
                        auditInfo.Module = (AuditModule)model;
                        auditInfo.Category = (AuditCategory)category;
                        auditInfo.Action = (AuditAction)action;
                        int containerId = int.Parse(args[0].ToString());
                        RMContainer container = ContainerDao.GetContainerById(containerId);
                        auditInfo.Object = container.TypeName;
                        auditInfo.Status = Boolean.Parse(returnValue.ToString()) ? (int)AuditStatus.Successful : (int)AuditStatus.Failed;
                        break;
                    default:
                        break;
                }
                auditInfo.Module = (AuditModule)model;

                return auditInfo;
            }
            catch (Exception e)
            {
                ArgumentCheck.NotNull(auditInfo, nameof(auditInfo));
                auditInfo.Status = (int)AuditStatus.Failed;
                logger.Error(e.Message);
                throw;
            }

        }

    }
}
