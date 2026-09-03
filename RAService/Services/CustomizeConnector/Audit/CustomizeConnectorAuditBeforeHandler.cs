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
using AvePoint.RA.Contract.Audit.Async;
using AvePoint.RA.Contract.CustomizeConnector;
using AvePoint.RA.Contract.CustomizeConnector.Model;
using AvePoint.RA.Contract.RMWeb.Audit;
using AvePoint.RA.I18N.Core;
using AvePoint.RA.Service.Services.CustomizeConnector.ColumnManager.BuildIn;
using RazorEngine.Compilation.ImpromptuInterface.InvokeExt;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.Service.Services.CustomizeConnector.Audit
{
    public class CustomizeConnectorAuditBeforeHandler : RMServiceBase, IAsyncAuditBeforeHandler
    {

        private static IRMCustomizeConnectorService CustomizeConnectorService => PlatformWindsorManager.GetService<IRMCustomizeConnectorService>();

        public async Task<RMAuditInfo> CollectAsync(RMAuditInfo auditInfo, AuditModule module, AuditAction action, AuditCategory category, object[] args)
        {
            var arg = args[0];
            switch(action)
            {
                case AuditAction.CustomizeConnectorCreate:
                    Add(auditInfo, arg);
                    break;
                case AuditAction.CustomizeConnectorEdit:
                    await Edit(auditInfo, arg);
                    break;
                case AuditAction.CustomizeConnectorDelete:
                    await Delete(auditInfo, arg);
                    break;
            }

            return auditInfo;
        }

        private static void Add(RMAuditInfo auditInfo, object arg)
        {
            var connectorInfo = arg as CustomizeConnectorInfo;
            auditInfo.Object = connectorInfo.Name;
            auditInfo.ModifyContent.Add(new AuditItem
            {
                NewValue = connectorInfo.Name,
                TargetSetting = I18NEntity.GetString("RM_RC_Audit_Connector_Name")
            });
            auditInfo.ModifyContent.Add(new AuditItem
            {
                NewValue = connectorInfo.Description,
                TargetSetting = I18NEntity.GetString("RM_Connector_Description")
            });

            var columnNames = connectorInfo.ColumnInfoes
                .Where(item => item.Id != CustomizeConnectorBuildColumnIds.RowKey)
                .OrderBy(item => item.Order)
                .Select(item => item.Name);
            auditInfo.ModifyContent.Add(new AuditItem
            {
                NewValue = string.Join(", ", columnNames),
                TargetSetting = I18NEntity.GetString("RM_RC_Audit_Connector_Column")
            });
        }

        private static async System.Threading.Tasks.Task Edit(RMAuditInfo auditInfo, object arg)
        {
            var connectorInfo = arg as CustomizeConnectorInfo;
            var existConnectorInfo = await CustomizeConnectorService.GetAsync(connectorInfo.Id);
            auditInfo.Object = connectorInfo.Name;
            auditInfo.ModifyContent.Add(new AuditItem
            {
                NewValue = connectorInfo.Name,
                OldValue = existConnectorInfo.Name,
                TargetSetting = I18NEntity.GetString("RM_RC_Audit_Connector_Name")
            });
            auditInfo.ModifyContent.Add(new AuditItem
            {
                NewValue = connectorInfo.Description,
                OldValue = existConnectorInfo.Description,
                TargetSetting = I18NEntity.GetString("RM_Connector_Description")
            });

            var columnNames = connectorInfo.ColumnInfoes
                .Where(item => item.Id != CustomizeConnectorBuildColumnIds.RowKey)
                .OrderBy(item => item.Order)
                .Select(item => item.Name);
            var existColumnNames = existConnectorInfo.ColumnInfoes
                .Where(item => item.Id != CustomizeConnectorBuildColumnIds.RowKey)
                .OrderBy(item => item.Order)
                .Select(item => item.Name);
            auditInfo.ModifyContent.Add(new AuditItem
            {
                NewValue = string.Join(", ", columnNames),
                OldValue = string.Join(", ", existColumnNames),
                TargetSetting = I18NEntity.GetString("RM_RC_Audit_Connector_Column")
            });
        }

        private static async System.Threading.Tasks.Task Delete(RMAuditInfo auditInfo, object arg)
        {
            var ids = arg as List<Guid>;
            var connectorInfoes = await CustomizeConnectorService.GetAllAsync();
            var willDeleteNames = connectorInfoes.Where(item => ids.Contains(item.Id)).Select(item => item.Name).ToList();
            auditInfo.Object = string.Join("; ", willDeleteNames);
        }
    }
}
