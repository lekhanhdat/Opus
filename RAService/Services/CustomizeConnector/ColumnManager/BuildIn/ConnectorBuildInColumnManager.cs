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
using AvePoint.RA.Contract.CustomizeConnector;
using AvePoint.RA.Contract.CustomizeConnector.Model;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.Contract.TemplateManagement;
using AvePoint.RA.DB.Explorer.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.Service.Services.CustomizeConnector.ColumnManager.BuildIn
{
    public class ConnectorBuildInColumnManager
    {
        private static readonly RALogger Logger = RALogger.GetInstance(typeof(ConnectorBuildInColumnManager));

        private static readonly List<IConnectorBuildInColumn> BuildInColumnManagers = new();

        private static readonly HashSet<Guid> BuildColumnIds = new()
        {
            CustomizeConnectorBuildColumnIds.RowKey,
            CustomizeConnectorBuildColumnIds.Name,
            CustomizeConnectorBuildColumnIds.Term,
            CustomizeConnectorBuildColumnIds.Created,
            CustomizeConnectorBuildColumnIds.Modified,
            CustomizeConnectorBuildColumnIds.CreatedBy,
            CustomizeConnectorBuildColumnIds.ModifiedBy
        };

        static ConnectorBuildInColumnManager()
        {
            try
            {
                var columnManagerType = typeof(IConnectorBuildInColumn);
                var assembly = Assembly.GetAssembly(columnManagerType);

                foreach (var type in assembly.GetTypes())
                {
                    if (type.IsAbstract || type.IsInterface) continue;
                    if (type.GetInterface(columnManagerType.Name) != null)
                    {
                        var instance = Activator.CreateInstance(type) as IConnectorBuildInColumn;
                        BuildInColumnManagers.Add(instance);
                    }
                }
                Logger.Info($"Successful initialize build-in column managers.");
            }
            catch (Exception e)
            {
                Logger.Error($"An error occurred while initialize build-in column managers. Error: {e}");
            }
        }

        public static bool ColumnListValidate(IEnumerable<CustomizeConnectorColumnInfo> needValidateColumnList)
        {
            var needValidateBuildInColumnList = needValidateColumnList.Where(item => item.Origin == Contract.CustomizeConnector.Enums.CustomizeConnectorOrigin.BuildIn).ToList();
            if(needValidateBuildInColumnList.Count != BuildInColumns.Columns.Count)
            {
                return false;
            }

            if (needValidateBuildInColumnList.Any(item => !BuildColumnIds.Contains(item.Id)))
            {
                return false;
            }

            return true;
        }

        public static void ApplyRecordValues(Record record, Dictionary<string, CustomColumn> customColumnDic)
        {
            foreach(var columnManager in BuildInColumnManagers)
            {
                columnManager.ApplyRecordValue(record, customColumnDic);
            }
        }

        public static Task<CustomizeConnectorNameValue<string>> ConvertToNameValueAsync(CustomizeConnectorColumnInfo columnInfo, Record record, bool forDisplay = true)
        {
            var columnManager = BuildInColumnManagers.First(item => item.ColumnId == columnInfo.Id);
            return columnManager.ConvertToNameValueAsync(columnInfo, record, forDisplay);
        }
    }
}
