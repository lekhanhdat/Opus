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
using AvePoint.RA.Contract.AzureFileShare;
using AvePoint.RA.Contract.AzureFileShare.Model;
using AvePoint.RA.Contract.RMWeb.Audit;
using AvePoint.RA.DB.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.Service.Services.AzureFileShare.AuditHandler
{
    public class AzureFileShareConnectionGroupBeforeAuditHandler : IBeforeAuditHandler
    {
        private static readonly IRMAzureFileShareConnectionGroupService AzureFileShareConnectionGroupService = PlatformWindsorManager.GetService<IRMAzureFileShareConnectionGroupService>();

        private static readonly IRMAzureFileShareConnectionService AzureFileShareConnectionService = PlatformWindsorManager.GetService<IRMAzureFileShareConnectionService>();

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
                case AuditAction.AzureFileShareCreateGroup:
                    await CreateAsync(info, args);
                    break;
                case AuditAction.AzureFileShareEditGroup:
                    await EditAsync(info, args);
                    break;
                case AuditAction.AzureFileShareDeleteGroup:
                    await RemoveAsync(info, args);
                    break;
            }
            return info;
        }

        public async System.Threading.Tasks.Task RemoveAsync(RMAuditInfo info, object[] args)
        {
            var ids = args[0] as List<Guid>;
            var groups = await AzureFileShareConnectionGroupService.GetAllAsync(ids);
            info.Object = string.Join("; ", groups.Select(item => item.Name));
        }

        public async System.Threading.Tasks.Task CreateAsync(RMAuditInfo info, object[] args)
        {
            var groupItem = args[0] as AzureFileShareConnectionGroupItem;
            var connections = await AzureFileShareConnectionService.GetAllAsync(groupItem.Connections.Select(item => item.Id).ToList());
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

        public async System.Threading.Tasks.Task EditAsync(RMAuditInfo info, object[] args)
        {
            var newGroupItem = args[0] as AzureFileShareConnectionGroupItem;
            var oldGroupItem = await AzureFileShareConnectionGroupService.GetAsync(newGroupItem.Id);
            info.Object = newGroupItem.Name;
            if(newGroupItem.Name != oldGroupItem.Name)
            {
                info.ModifyContent.Add(new AuditItem
                {
                    TargetSetting = "RM_FS_Register_GroupName",
                    NewValue = newGroupItem.Name,
                    OldValue = oldGroupItem.Name
                });
            }

            if(newGroupItem.Description != oldGroupItem.Description)
            {
                info.ModifyContent.Add(new AuditItem
                {
                    TargetSetting = "RM_FS_Register_Description",
                    NewValue = newGroupItem.Description,
                    OldValue = oldGroupItem.Description
                });
            }

            var newConnections = await AzureFileShareConnectionService.GetAllAsync(newGroupItem.Connections.Select(item => item.Id).ToList());
            var newConnectionNames = string.Join("; ", newConnections.Select(item => item.Name));

            var oldConnections = await AzureFileShareConnectionService.GetAllAsync(oldGroupItem.Connections.Select(item => item.Id).ToList());
            var oldConnectionNames = string.Join("; ", oldConnections.Select(item => item.Name));

            if(newConnectionNames != oldConnectionNames)
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
