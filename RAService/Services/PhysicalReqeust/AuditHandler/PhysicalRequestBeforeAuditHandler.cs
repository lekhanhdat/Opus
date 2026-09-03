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
using AvePoint.RA.Common.Audit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AvePoint.RA.Contract.RMWeb.Audit;
using AvePoint.RA.Contract.RMWeb.Physical;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.Common;

namespace AvePoint.RA.Service.Services.PhysicalReqeust.AuditHandler
{
    public class PhysicalRequestBeforeAuditHandler : IBeforeAuditHandler
    {
        private IPhysicalRequestDao PhysicalRequestDao => PlatformWindsorManager.GetService<IPhysicalRequestDao>();

        public async Task<RMAuditInfo> CollectAsync(int model, int category, int action, object[] args, object target)
        {
            AuditAction act = (AuditAction)action;
            if (act == AuditAction.UpdatePhysicalRequest)
            {
                var info = new RMAuditInfo();
                info.Module = (AuditModule)model;
                info.Category = (AuditCategory)category;
                info.Action = (AuditAction)action;
                PhysicalRequestDto param = args[0] as PhysicalRequestDto;
                if (param != null)
                {
                    DB.Model.RMPhysicalRequest dbModel = PhysicalRequestDao.Find(a => a.Id == param.Id);
                    info.ModifyContent = new List<AuditItem>();
                    info.ModifyContent.Add(new AuditItem() { TargetSetting= AuditConstants.Audit_Physical_Request_File_Title, OldValue = dbModel.Title });
                    info.ModifyContent.Add(new AuditItem() { TargetSetting = AuditConstants.Audit_Physical_Request_Comment, OldValue = dbModel.Comment });
                    info.ModifyContent.Add(new AuditItem() { TargetSetting = AuditConstants.Audit_Physical_Request_Hold_User, OldValue = dbModel.HoldUserId });
                    info.ModifyContent.Add(new AuditItem() { TargetSetting = AuditConstants.Audit_Physical_Request_EndTime, OldValue = new DateTime(dbModel.EndTime).ToString() });
                }
                return info;
            }
            return null;
        }
    }
}
