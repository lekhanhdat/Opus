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
using AvePoint.GCommon.Contract.CentralAdmin.Object;
using AvePoint.RA.Common;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RMWeb.Explorer;
using AvePoint.RA.Contract.RMWeb.TemplateManagement;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Model;
using AvePoint.RA.DB.Model.Extension;
using AvePoint.RA.I18N.Core;
using Cloud.Sdk.Telemetry.Data.CloudRecords;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.RACommonUtility.Telemetry.Generator
{
    public class GlobalSearchTelemetryGenerator : TelemetryGenerator
    {
        public override TelemetryModule Module => TelemetryModule.GlobalSearch;

        private readonly IAccountDao AccountDao = PlatformWindsorManager.GetService<IAccountDao>();

        protected override Dictionary<TelemetryEventType, Func<IList<object>, CloudRecordsCommonRecord>> EventTypeGenerateMapping =>
            new Dictionary<TelemetryEventType, Func<IList<object>, CloudRecordsCommonRecord>>
            {
                { TelemetryEventType.Search, Search },
                {TelemetryEventType.ContentPageLoaded, PageLoaded }
            };

        public CloudRecordsCommonRecord Search(IList<object> args)
        {
            var record = new CloudRecordsGlobalSearchRecord();
            record.SearchResultMoreThanOnePage = Convert.ToBoolean(args[0]);
            record.SelectedFilters = GetSelectedFilters(Convert.ToString(args[1]));
            record.SearchUsageTime = Convert.ToInt64(args[2]);
            return record;
        }

        public CloudRecordsCommonRecord PageLoaded(IList<object> args)
        {
            return new CloudRecordsGlobalSearchRecord
            {
                NonAdministrator = !AccountDao.CheckAdminRole(TenantLocalValue.LogonUserId)
            };
        }

        private string GetSelectedFilters(object filterOptions)
        {
            var filters = JsonConvert.DeserializeObject<List<ExplorerSearchOptionV3>>(Convert.ToString(filterOptions));
            var filterIds = filters.Select(item => item.Column.Id);
            var buildInFilterIds = filterIds.Where(filterId => FilterColumns.IsBuildInColumn(filterId));
            var customeFilterIds = filterIds.Where(filterId => !FilterColumns.IsBuildInColumn(filterId));

            var buildInFilterNames = buildInFilterIds.Select(filterId => FilterColumns.GetBuildInColumnName(filterId));
            var customeFilterNames = FilterColumns.GetCustomeColumnNames(customeFilterIds);
            var filterNames = customeFilterNames.Concat(buildInFilterNames);

            return string.Join(",", filterNames);
        }

        internal class FilterColumns
        {

            private static readonly IRMTemplateDao RMTemplateDao = PlatformWindsorManager.GetService<IRMTemplateDao>();

            private static readonly Dictionary<string, string> BuildInColumns = new Dictionary<string, string>
            {
                {"de5e99cb-4fb4-4e25-b732-a1dce71dd048", "RM_PRM_PRE_Column_Name" },
                {"c980eb95-ea92-4f07-9f97-1a8ab2a053fa", "RM_PRM_PRE_Column_ID" },
                {"edbac887-d4cc-ed92-ad0d-0e68ceb336a0", "RM_JS_BCM_Explorer_Datagrid_Source" },
                {"90c0f7ce-ad79-4a9d-a5eb-3b097006b03d", "RM_JS_BCM_Explorer_Datagrid_FileType" },
                {"ce693d2c-ab58-4d29-9db5-3191bfc5c81a", "RM_JS_JMD_Grid_Classification" },
                //{"da9dcebc-5628-45b7-9dff-37ca8a601e31", "" },
                //{"4de03a10-4b33-4091-8929-68be1f7d2325", "" },
                {"38e1e287-4077-44a5-ba57-3de64561c51f", "RM_JS_BCM_Explorer_Datagrid_RecordsOwner" },
                {"f9806a66-1be8-4f85-867e-f0de4fa4c073", "RM_JS_BCM_Explorer_Datagrid_OnHold" },
                {"8499e388-9c52-4366-a7b3-df77c70e648f", "RM_PRM_PRE_Column_HoldBy" },
                {"9117fd6b-4171-4405-b881-cbe139e6ced7", "RM_JS_BCM_Explorer_Datagrid_DisposalDueDate" },
                {"c55a2cc4-2825-42ff-b1d4-fb72b7be7dc5", "RM_JS_RDM_Explorer_CreateTime" },
                {"91a08d45-c5dd-43da-b6c4-670f11ac273e", "RM_JS_Common_CreatedBy" },
                {"3ec9a488-90fa-4d62-835f-0df0cd2e9f97", "RM_PRM_PRE_Column_ModifiedTime" },
                {"1f2e8c3f-e49a-473c-bd16-8647258cf15c", "RM_TemplateManage_ModifiedBy" },
                {"bf4e131c-1d9b-403b-8a9f-a1fa3b63cd15", "RM_JS_BCM_Explorer_Datagrid_Declared" },
                {"becf61cd-bd6b-440c-8e33-4b6300be58d5", "RM_JS_BCM_Explorer_Filter_FolderLabel" },
                {"df21d79c-bc37-fdfd-f59e-641f7d630488", "RM_PRM_PRE_Column_LoanBy" },
                {"ee86426d-488f-4bdb-a63b-2ef6a61c7bef", "RM_JS_BCM_Explorer_Filter_SharePointOnlineLabel" },
                //{"b3512f95-198e-c3c9-c2d6-ec21c81e0bae", "" },
            };

            public static bool IsBuildInColumn(string columnId)
            {
                return BuildInColumns.ContainsKey(columnId);
            }

            public static string GetBuildInColumnName(string columnId)
            {
                if(BuildInColumns.TryGetValue(columnId, out var i18nLabel))
                {
                    return I18NEntity.GetString(i18nLabel);
                }
                return "";
            }

            private static Dictionary<string, string> GetCustomColumns()
            {
                var result = new Dictionary<string, string>();

                var templates = RMTemplateDao.GetTemplate();
                foreach(var template in templates)
                {
                    foreach(var column in template.GetColumnList4Display())
                    {
                        var columnId = column.UniqueId.ToString();
                        if(result.ContainsKey(columnId))
                        {
                            continue;
                        }
                        result.Add(columnId, column.ColumnName);
                    }
                }

                return result;
            }

            public static List<string> GetCustomeColumnNames(IEnumerable<string> columnIds)
            {
                var result = new List<string>();
                var customColumns = GetCustomColumns();

                foreach(var columnId in columnIds)
                {
                    if (customColumns.TryGetValue(columnId, out var columnName))
                    {
                        var i18nName = I18NEntity.GetString(columnName);
                        if(!string.IsNullOrEmpty(i18nName))
                        {
                            columnName = i18nName;
                        }
                        result.Add(columnName);
                    }
                }

                return result;
            }
        }
    }
}
