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
using AvePoint.RA.Contract.Box;
using AvePoint.RA.Contract.Box.Model;
using AvePoint.RA.Contract.RMWeb.Audit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AvePoint.RA.Service.Services.Box.AuditHandler
{
    public class BoxConnectionBeforeAuditHandler : IBeforeAuditHandler
    {
        private static IRMBoxConnectionService BoxConnectionService => PlatformWindsorManager.GetService<IRMBoxConnectionService>();

        public async Task<RMAuditInfo> CollectAsync(int model, int category, int action, object[] args, object target)
        {
            var info = new RMAuditInfo
            {
                Module = (AuditModule)model,
                Category = (AuditCategory)category,
                Action = (AuditAction)action,
                ModifyContent = new List<AuditItem>()
            };
            switch ((AuditAction)action)
            {
                case AuditAction.BoxCreateConnection:
                    Create(info, args);
                    break;
                case AuditAction.BoxEditConnection:
                    await EditAsync(info, args);
                    break;
                case AuditAction.BoxDeleteConnection:
                    await RemoveAsync(info, args);
                    break;
            }
            return info;
        }

        public async Task RemoveAsync(RMAuditInfo info, object[] args)
        {
            var ids = args[0] as List<Guid>;
            var connections = await BoxConnectionService.GetAllByIdsAsync(ids);
            info.Object = string.Join("; ", connections.Select(item => item.Name));
        }

        public void Create(RMAuditInfo info, object[] args)
        {
            var connectionItem = args[0] as BoxConnectionItem;
            info.Object = connectionItem.Name;
            info.ModifyContent.Add(new AuditItem
            {
                TargetSetting = "RM_FS_Register_ConnectionName",
                NewValue = connectionItem.Name
            });
            info.ModifyContent.Add(new AuditItem
            {
                TargetSetting = "RM_FS_Register_Description",
                NewValue = connectionItem.Description
            });
            info.ModifyContent.Add(new AuditItem
            {
                TargetSetting = "RM_Box_Register_Connection_InformationTitle"
            });
        }

        public async Task EditAsync(RMAuditInfo info, object[] args)
        {
            var newConnectionItem = args[0] as BoxConnectionItem;
            var oldConnectionItem = await BoxConnectionService.GetByIdAsync(newConnectionItem.Id);
            info.Object = newConnectionItem.Name;
            if (newConnectionItem.Name != oldConnectionItem.Name)
            {
                info.ModifyContent.Add(new AuditItem
                {
                    TargetSetting = "RM_FS_Register_ConnectionName",
                    NewValue = newConnectionItem.Name,
                    OldValue = oldConnectionItem.Name
                });
            }

            if (newConnectionItem.Description != oldConnectionItem.Name)
            {
                info.ModifyContent.Add(new AuditItem
                {
                    TargetSetting = "RM_FS_Register_Description",
                    NewValue = newConnectionItem.Description,
                    OldValue = oldConnectionItem.Description
                });
            }

            if (newConnectionItem.AuthenticationType != oldConnectionItem.AuthenticationType ||
                newConnectionItem.EnterpriseId != oldConnectionItem.EnterpriseId ||
                newConnectionItem.ClientId != oldConnectionItem.ClientId ||
                newConnectionItem.ClientSecret != oldConnectionItem.ClientSecret ||
                newConnectionItem.AccessToken != oldConnectionItem.AccessToken ||
                newConnectionItem.EmailAddress != oldConnectionItem.AccessToken ||
                newConnectionItem.JsonFileName != oldConnectionItem.JsonFileName ||
                newConnectionItem.JsonFileContent != oldConnectionItem.JsonFileContent ||
                newConnectionItem.RedirectUrl != oldConnectionItem.RedirectUrl)
            {
                info.ModifyContent.Add(new AuditItem
                {
                    TargetSetting = "RM_Box_Register_Connection_InformationTitle",
                });
            }

            if (newConnectionItem.AuthenticationType != oldConnectionItem.AuthenticationType)
            {
                info.ModifyContent.Add(new AuditItem
                {
                    TargetSetting = "RM_Box_Register_Connection_TypeTitle",
                    NewValue = newConnectionItem.AuthenticationType == BoxAuthenticationType.UserAuth ? 
                    "RM_Box_Register_Connection_Type_User" 
                    : "RM_Box_Register_Connection_Type_Server",
                    OldValue = oldConnectionItem.AuthenticationType == BoxAuthenticationType.UserAuth ?
                    "RM_Box_Register_Connection_Type_User" 
                    : "RM_Box_Register_Connection_Type_Server",
                });
            }
        }
    }
}
