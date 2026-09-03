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
using AvePoint.RA.Contract.CustomizeConnector.Model.Api;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.Contract.TemplateManagement;
using AvePoint.RA.I18N.Core;
using AvePoint.RA.Service.Services.RMGeneralSetting;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Dynamic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.Service.Services.CustomizeConnector.ColumnManager
{
    public class ConnectorColumnManager
    {
        private static readonly RALogger Logger = RALogger.GetInstance(typeof(ConnectorColumnManager));

        private readonly List<CustomizeConnectorColumnInfo> DBColumns;

        private readonly Dictionary<Contract.TemplateManagement.ColumnType, IConnectorColumn> ColumnManagers;

        private static readonly HashSet<Contract.TemplateManagement.ColumnType> AvailableCustomizeColumn = new()
        {
            Contract.TemplateManagement.ColumnType.SingleText,
            Contract.TemplateManagement.ColumnType.MultipleText,
            Contract.TemplateManagement.ColumnType.SingleChoice,
            Contract.TemplateManagement.ColumnType.MultipleChoice,
            Contract.TemplateManagement.ColumnType.DateTime,
            Contract.TemplateManagement.ColumnType.Number,
            Contract.TemplateManagement.ColumnType.PeopleOrGroup
        };

        public ConnectorColumnManager(IEnumerable<CustomizeConnectorColumnInfo> dbColumns)
        {
            DBColumns = dbColumns.ToList();
            ColumnManagers = InitColumnManagers();
        }

        public bool ColumnListValidate(IEnumerable<CustomizeConnectorColumnInfo> needValidateColumnList)
        {
            var needValidateCustomizeColumnList = needValidateColumnList.Where(item => item.Origin == Contract.CustomizeConnector.Enums.CustomizeConnectorOrigin.ExternalCustomize);

            var hasDuplicateName = needValidateCustomizeColumnList.GroupBy(item => item.Name).ToDictionary(item => item.Key, item => item.Count()).Values.Any(item => item > 1);
            if(hasDuplicateName)
            {
                return false;
            }

            var hasDuplicateId = needValidateCustomizeColumnList.Where(item => item.Id != Guid.Empty).GroupBy(item => item.Id).ToDictionary(item => item.Key, item => item.Count()).Values.Any(item => item > 1);
            if(hasDuplicateId)
            {
                return false;
            }

            var hasIllegeField = needValidateCustomizeColumnList.Any(item => string.IsNullOrWhiteSpace(item.Name) || !AvailableCustomizeColumn.Contains(item.Type));
            if(hasIllegeField)
            {
                return false;
            }

            var buildInColumnNames = BuildInColumns.Columns.Select(item => I18NEntity.GetString(item.Name)).ToHashSet();
            var hasDuplicateNameWithBuildIn = needValidateCustomizeColumnList.Any(item => buildInColumnNames.Contains(item.Name));
            if(hasDuplicateNameWithBuildIn)
            {
                return false;
            }

            var buildInColumnIds = BuildInColumns.Columns.Select(item => item.Id).ToHashSet();
            var hasDuplicateIdWithBuild = needValidateCustomizeColumnList.Any(item => buildInColumnIds.Contains(item.Id));
            if(hasDuplicateIdWithBuild)
            {
                return false;
            }

            foreach (var needValidateColumn in needValidateCustomizeColumnList)
            {
                if(DBColumns.Any(item => item.Id == needValidateColumn.Id))
                {
                    var dbColumn = DBColumns.First(item => item.Id == needValidateColumn.Id);
                    if(dbColumn.Type != needValidateColumn.Type)
                    {
                        return false;
                    }
                }

                var columnManager = ColumnManagers[needValidateColumn.Type];
                if(!columnManager.DefinitionValidate(needValidateColumn))
                {
                    return false;
                }
            }

            return true;
        }

        public async Task<CustomizeConnectorDataValidateResult> ColumnDataListValidateAsync(List<object> dataList)
        {
            foreach(var data in dataList)
            {
                var validateRes = await ColumnDataValidateAsync(data);
                if(!validateRes.IsValidated)
                {
                    return validateRes;
                }
            }

            return CustomizeConnectorDataValidateResult.Validated();
        }

        public async Task<CustomizeConnectorDataValidateResult> ColumnDataValidateAsync(object data)
        {
            if(data == null)
            {
                return CustomizeConnectorDataValidateResult.UnValidated(I18NEntity.GetString("RM_Connector_DataListObjIllegal"));
            }

            Dictionary<string, object> columnDataDic = ConvertDataObjectToDictionary(data);

            foreach (var dbColumn in DBColumns)
            {
                var columnManager = ColumnManagers[dbColumn.Type];
                columnDataDic.TryGetValue(dbColumn.InternalName, out var columnData);
                var validateRes = await columnManager.ValueValidateAsync(dbColumn, columnData);
                if(!validateRes.IsValidated)
                {
                    return validateRes;
                }
            }

            return CustomizeConnectorDataValidateResult.Validated();
        }


        public async Task<CustomizeConnectorDataValidateResult> ColumnDataValidateV1Async(object data)
        {
            if (data == null)
            {
                return CustomizeConnectorDataValidateResult.UnValidated(I18NEntity.GetString("RM_Connector_DataListObjIllegal"));
            }

            Dictionary<string, object> columnDataDic = ConvertDataObjectToDictionary(data);

            foreach (var dbColumn in DBColumns)
            {
                var columnManager = ColumnManagers[dbColumn.Type];
                columnDataDic.TryGetValue(dbColumn.InternalName, out var columnData);
                if(columnManager.Type == Contract.TemplateManagement.ColumnType.DateTime)
                {
                    var dateValidateRes = DateTimeConnectorColumnValidate(dbColumn, columnData);
                    if (!dateValidateRes.IsValidated)
                    {
                        return dateValidateRes;
                    }
                    continue;
                }
                var validateRes = await columnManager.ValueValidateAsync(dbColumn, columnData);
                if (!validateRes.IsValidated)
                {
                    return validateRes;
                }
            }

            return CustomizeConnectorDataValidateResult.Validated();
        }

        public Task<CustomizeConnectorNameValue<string>> ConvertToNameValueAsync(CustomizeConnectorColumnInfo columnInfo, Dictionary<string, CustomColumn> customColumnDic, bool forDisplay = true)
        {
            var columnManager = ColumnManagers[columnInfo.Type];
            return columnManager.ConvertToNameValueAsync(columnInfo, customColumnDic, forDisplay);
        }

        public async Task<Dictionary<string, CustomColumn>> ConvertToExplorerColumnDicAsync(object data)
        {
            Dictionary<string, object> columnDataDic = ConvertDataObjectToDictionary(data);
            var res = new Dictionary<string, CustomColumn>();
            
            foreach (var dbColumn in DBColumns)
            {
                var columnManager = ColumnManagers[dbColumn.Type];
                columnDataDic.TryGetValue(dbColumn.InternalName, out var columnData);
                (var suc, var customColumn) = await columnManager.TryConvertToCustomColumnAsync(dbColumn, columnData);
                if(suc)
                {
                    res.Add(dbColumn.Id.ToString(), customColumn);
                }
            }

            return res;
        }

        public async Task<Dictionary<string, CustomColumn>> ConvertToExplorerColumnDicV1Async(object data)
        {
            Dictionary<string, object> columnDataDic = ConvertDataObjectToDictionary(data);
            var res = new Dictionary<string, CustomColumn>();

            foreach (var dbColumn in DBColumns)
            {
                var columnManager = ColumnManagers[dbColumn.Type];
                columnDataDic.TryGetValue(dbColumn.InternalName, out var columnData);
                if(columnManager.Type == Contract.TemplateManagement.ColumnType.DateTime)
                {
                    (var dateSuc, var dateCustomColumn) = await TryConvertDateTimeToCustomColumnAsync(columnData);
                    if (dateSuc)
                    {
                        res.Add(dbColumn.Id.ToString(), dateCustomColumn);
                    }
                    continue;
                }
                (var suc, var customColumn) = await columnManager.TryConvertToCustomColumnAsync(dbColumn, columnData);
                if (suc)
                {
                    res.Add(dbColumn.Id.ToString(), customColumn);
                }
            }

            return res;
        }

        public Dictionary<string, object> ConvertToRulePolicy(Dictionary<string, CustomColumn> customColumnDic)
        {
            var res = new Dictionary<string, object>();
            foreach(var columnInfo in DBColumns) 
            {
                var columnManager = ColumnManagers[columnInfo.Type];
                if(columnManager.TryConvertToRulePolicy(columnInfo, customColumnDic, out var value))
                {
                    res.Add(columnInfo.InternalName, value);
                }
            }
            return res;
        }

        private static Dictionary<Contract.TemplateManagement.ColumnType, IConnectorColumn> InitColumnManagers()
        {
            var res = new Dictionary<Contract.TemplateManagement.ColumnType, IConnectorColumn>();
            try
            {
                var columnManagerType = typeof(IConnectorColumn);
                var assembly = Assembly.GetAssembly(columnManagerType);

                foreach (var type in assembly.GetTypes())
                {
                    if (type.IsAbstract || type.IsInterface) continue;
                    if (type.GetInterface(columnManagerType.Name) != null)
                    {
                        var instance = Activator.CreateInstance(type) as IConnectorColumn;
                        res.Add(instance.Type, instance);
                    }
                }
                Logger.Info($"Successful initialize column managers.");
            }
            catch (Exception e)
            {
                Logger.Error($"An error occurred while initialize column managers. Error: {e}");
            }

            return res;
        }

        private static Dictionary<string, object> ConvertDataObjectToDictionary(object data)
        {
            var res = new Dictionary<string, object>();
            var dataJson = JsonConvert.SerializeObject(data);
            var dataExpandoObjs = JsonConvert.DeserializeObject<ExpandoObject>(dataJson);
            foreach(var dataExpandoObj in dataExpandoObjs)
            {
                res.Add(dataExpandoObj.Key, dataExpandoObj.Value);
            }

            return res;
        }

        private static CustomizeConnectorDataValidateResult DateTimeConnectorColumnValidate(CustomizeConnectorColumnInfo columnInfo, object valueJson)
        {
            if (columnInfo.IsRequired && string.IsNullOrEmpty(valueJson?.ToString()))
            {
                return CustomizeConnectorDataValidateResult.UnValidated(string.Format(I18NEntity.GetString("RM_Connector_Validate_IsRequired"), columnInfo.InternalName));
            }

            if (!columnInfo.IsRequired && string.IsNullOrEmpty(valueJson?.ToString()))
            {
                return CustomizeConnectorDataValidateResult.Validated();
            }

            if (DateTime.TryParse(valueJson.ToString(), out _))
            {
                return CustomizeConnectorDataValidateResult.Validated();
            }
            else
            {
                return CustomizeConnectorDataValidateResult.UnValidated(string.Format(I18NEntity.GetString("RM_Connector_Validate_IsIllegal"), columnInfo.InternalName));
            }
        }

        public async Task<(bool, CustomColumn)> TryConvertDateTimeToCustomColumnAsync(object valueJson)
        {
            CustomColumn customColumn = null;
            if (string.IsNullOrEmpty(valueJson?.ToString()))
            {
                return (false, customColumn);
            }

            if (!DateTime.TryParse(valueJson.ToString(), out var dateTime) || dateTime.Ticks < DateTime.MinValue.Ticks || dateTime.Ticks > DateTime.MaxValue.Ticks)
            {
                return (false, customColumn);
            }

            customColumn = new CustomColumn
            {
                Date = dateTime
            };

            return (true, customColumn);
        }
    }
}
