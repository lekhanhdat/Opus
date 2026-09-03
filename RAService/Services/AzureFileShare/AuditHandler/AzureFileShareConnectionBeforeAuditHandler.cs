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
    public class AzureFileShareConnectionBeforeAuditHandler : IBeforeAuditHandler
    {

        private static IRMAzureFileShareConnectionService AzureFileShareConnectionService => PlatformWindsorManager.GetService<IRMAzureFileShareConnectionService>();

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
                case AuditAction.AzureFileShareCreateConnection:
                    Create(info, args);
                    break;
                case AuditAction.AzureFileShareEditConnection:
                    await EditAsync(info, args);
                    break;
                case AuditAction.AzureFileShareDeleteConnection:
                    await RemoveAsync(info, args);
                    break;
            }

            return info;
        }

        public async System.Threading.Tasks.Task RemoveAsync(RMAuditInfo info, object[] args)
        {
            var ids = args[0] as List<Guid>;
            var connections = await AzureFileShareConnectionService.GetAllAsync(ids);
            info.Object = string.Join("; ", connections.Select(item => item.Name));
        }

        public void Create(RMAuditInfo info, object[] args)
        {
            var connectionItem = args[0] as AzureFileShareConnectionItem;
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
                TargetSetting = "RM_AZFS_Register_FileStorage"
            });
        }

        public async System.Threading.Tasks.Task EditAsync(RMAuditInfo info, object[] args)
        {
            var newConnectionItem = args[0] as AzureFileShareConnectionItem;
            var oldConnectionItem = await AzureFileShareConnectionService.GetAsync(newConnectionItem.Id);
            info.Object = newConnectionItem.Name;
            if(newConnectionItem.Name != oldConnectionItem.Name)
            {
                info.ModifyContent.Add(new AuditItem
                {
                    TargetSetting = "RM_FS_Register_ConnectionName",
                    NewValue = newConnectionItem.Name,
                    OldValue = oldConnectionItem.Name
                });
            }

            if(newConnectionItem.Description != oldConnectionItem.Name)
            {
                info.ModifyContent.Add(new AuditItem
                {
                    TargetSetting = "RM_FS_Register_Description",
                    NewValue = newConnectionItem.Description,
                    OldValue = oldConnectionItem.Description
                });
            }

            if(newConnectionItem.AccessEndPoint != oldConnectionItem.AccessEndPoint ||
                newConnectionItem.FileShareName != oldConnectionItem.FileShareName ||
                newConnectionItem.AccountKey != oldConnectionItem.AccountKey ||
                newConnectionItem.AccountName != oldConnectionItem.AccountName)
            {
                info.ModifyContent.Add(new AuditItem
                {
                    TargetSetting = "RM_AZFS_Register_FileStorage"
                });
            }
        }
    }
}
