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
    public class BoxConnectionGroupBeforeAuditHandler : IBeforeAuditHandler
    {
        private static readonly IRMBoxConnectionGroupService BoxConnectionGroupService = PlatformWindsorManager.GetService<IRMBoxConnectionGroupService>();

        private static readonly IRMBoxConnectionService BoxConnectionService = PlatformWindsorManager.GetService<IRMBoxConnectionService>();
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
                case AuditAction.BoxCreateGroup:
                    await CreateAsync(info, args);
                    break;
                case AuditAction.BoxEditGroup:
                    await EditAsync(info, args);
                    break;
                case AuditAction.BoxDeleteGroup:
                    await RemoveAsync(info, args);
                    break;
            }
            return info;
        }

        public async Task RemoveAsync(RMAuditInfo info, object[] args)
        {
            var ids = args[0] as List<Guid>;
            var groups = await BoxConnectionGroupService.GetAllByIdsAsync(ids);
            info.Object = string.Join("; ", groups.Select(item => item.Name));
        }

        public async Task CreateAsync(RMAuditInfo info, object[] args)
        {
            var groupItem = args[0] as BoxConnectionGroupItem;
            var connections = await BoxConnectionService.GetAllByIdsAsync(groupItem.Connections.Select(item => item.Id).ToList());
            info.Object = groupItem.Name;
            info.ModifyContent.Add(new AuditItem
            {
                TargetSetting = "RM_FS_Register_GroupName",
                NewValue = groupItem.Name
            });
            info.ModifyContent.Add(new AuditItem
            {
                TargetSetting = "RM_FS_Register_Description",
                NewValue = groupItem.Description
            });
            info.ModifyContent.Add(new AuditItem
            {
                TargetSetting = "RM_FS_Register_Connections",
                NewValue = string.Join("; ", connections.Select(item => item.Name))
            });
        }

        public async Task EditAsync(RMAuditInfo info, object[] args)
        {
            var newGroupItem = args[0] as BoxConnectionGroupItem;
            var oldGroupItem = await BoxConnectionGroupService.GetByIdAsync(newGroupItem.Id);
            info.Object = newGroupItem.Name;
            if (newGroupItem.Name != oldGroupItem.Name)
            {
                info.ModifyContent.Add(new AuditItem
                {
                    TargetSetting = "RM_FS_Register_GroupName",
                    NewValue = newGroupItem.Name,
                    OldValue = oldGroupItem.Name
                });
            }

            if (newGroupItem.Description != oldGroupItem.Description)
            {
                info.ModifyContent.Add(new AuditItem
                {
                    TargetSetting = "RM_FS_Register_Description",
                    NewValue = newGroupItem.Description,
                    OldValue = oldGroupItem.Description
                });
            }

            var newConnections = await BoxConnectionService.GetAllByIdsAsync(newGroupItem.Connections.Select(item => item.Id).ToList());
            var newConnectionNames = string.Join("; ", newConnections.Select(item => item.Name));

            var oldConnections = await BoxConnectionService.GetAllByIdsAsync(oldGroupItem.Connections.Select(item => item.Id).ToList());
            var oldConnectionNames = string.Join("; ", oldConnections.Select(item => item.Name));

            if (newConnectionNames != oldConnectionNames)
            {
                info.ModifyContent.Add(new AuditItem
                {
                    TargetSetting = "RM_FS_Register_Connections",
                    NewValue = string.IsNullOrEmpty(newConnectionNames) ? "RM_RC_Audit_None" : newConnectionNames,
                    OldValue = string.IsNullOrEmpty(oldConnectionNames) ? "RM_RC_Audit_None" : oldConnectionNames
                });
            }
        }
    }
}
