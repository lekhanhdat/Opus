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
using AvePoint.RA.Common;

namespace AvePoint.RA.Service.Services.LocationManagement.AuditHandler
{
    public class ContainerManagementBeforeAuditHandler : IBeforeAuditHandler
    {
        private RALogger logger = RALogger.GetInstance(typeof(ContainerManagementBeforeAuditHandler));

        private IContainerDao ContainerDao => PlatformWindsorManager.GetService<IContainerDao>();

        public async Task<RMAuditInfo> CollectAsync(int model, int category, int action, object[] args, object target)
        {
            var info = new RMAuditInfo();
            try
            {
                switch ((AuditAction)action)
                {
                    case AuditAction.CreateContainer:
                        info.Object = args[0] != null ? args[0].ToString() : string.Empty;
                        break;
                    case AuditAction.EditContainer:
                        info.Object = args[1] != null ? args[1].ToString() : string.Empty;
                        int Id = int.Parse(args[0].ToString());
                        if (info.ModifyContent == null) { info.ModifyContent = new List<AuditItem>(); }
                        RMContainer box = ContainerDao.GetContainerById(Id);
                        AuditItem nameItem = new AuditItem();
                        nameItem.TargetSetting = I18NEntity.GetString("RM_JS_PRM_CZ_ContentType");
                        nameItem.OldValue = box.TypeName;
                        info.ModifyContent.Add(nameItem);
                        AuditItem sizeItem = new AuditItem();
                        sizeItem.TargetSetting = I18NEntity.GetString("RM_JS_PRM_CZ_Size");
                        box.Size = Math.Round(box.Size, 2);
                        sizeItem.OldValue = box.Size.ToString();
                        info.ModifyContent.Add(sizeItem);
                        AuditItem descItem = new AuditItem();
                        descItem.TargetSetting = I18NEntity.GetString("RM_JS_PRM_CZ_Description");
                        descItem.OldValue = box.Description;
                        info.ModifyContent.Add(descItem);
                        break;
                    case AuditAction.DeleteContainer:
                        int containerId = int.Parse(args[0].ToString());
                        RMContainer container = ContainerDao.GetContainerById(containerId);
                        info.Object = container.TypeName;
                        break;
                    case AuditAction.EditContainerDefault:
                        List<RMContainer> defaultContainers = ContainerDao.GetDefaultContainers();
                        string oldDefaultBoxType = (defaultContainers != null && defaultContainers.Count > 0) ? defaultContainers[0].TypeName : "";
                        AuditItem oldDefaultItem = new AuditItem();
                        oldDefaultItem.TargetSetting = I18NEntity.GetString("RM_RC_Audit_Action_EditContainerDefault");
                        oldDefaultItem.OldValue = oldDefaultBoxType;
                        if (info.ModifyContent == null) { info.ModifyContent = new List<AuditItem>(); }
                        info.ModifyContent.Add(oldDefaultItem);
                        break;
                    default:
                        break;
                }

            }
            catch (Exception e)
            {
                logger.Error(e.Message);
            }

            return info;
        }
    }
}
