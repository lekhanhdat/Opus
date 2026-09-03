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
    public class RecordsExplorerTelemetryGenerator : TelemetryGenerator
    {
        private readonly IAccountDao AccountDao = PlatformWindsorManager.GetService<IAccountDao>();

        private readonly IRMTemplateDao RMTemplateDao = PlatformWindsorManager.GetService<IRMTemplateDao>();

        public override TelemetryModule Module => TelemetryModule.RecordsExplorer;

        protected override Dictionary<TelemetryEventType, Func<IList<object>, CloudRecordsCommonRecord>> EventTypeGenerateMapping =>
            new Dictionary<TelemetryEventType, Func<IList<object>, CloudRecordsCommonRecord>>
            {
                { TelemetryEventType.Search, Search },
                {TelemetryEventType.Filter, Filter },
                {TelemetryEventType.ContentPageLoaded, PageLoaded }
            };

        public CloudRecordsCommonRecord PageLoaded(IList<object> args)
        {
            var record = new CloudRecordsRecordsExplorerRecord();
            record.NonAdministrator = !AccountDao.CheckAdminRole(TenantLocalValue.LogonUserId);
            return record;
        }

        public CloudRecordsCommonRecord Search(IList<object> args)
        {
            var record = new CloudRecordsRecordsExplorerSearchRecord();
            record.SearchResultMoreThanOnePage = Convert.ToBoolean(args[0]);
            record.SearchUsageTime = Convert.ToInt64(args[1]);
            return record;
        }

        public CloudRecordsCommonRecord Filter(IList<object> args)
        {
            var record = new CloudRecordsRecordsExplorerSearchRecord();
            var filters = JsonConvert.DeserializeObject<ExplorerFilterOptionV2>(Convert.ToString(args[0]));
            record.SelectedFilters = GetExplorerSelectedFilters(filters);
            record.SearchUsageTime = Convert.ToInt64(args[1]);
            return record;
        }

        private string GetExplorerSelectedFilters(ExplorerFilterOptionV2 filterOption)
        {
            string result = string.Empty;
            #region default filter
            if (filterOption.SourceFlags != null && filterOption.SourceFlags.Count > 0)
            {
                result += I18NEntity.GetString("RM_JS_BCM_Explorer_Datagrid_Source") + ", ";
            }
            if (filterOption.FileExtensions != null && filterOption.FileExtensions.Count > 0)
            {
                result += I18NEntity.GetString("RM_PRM_PRE_MRR_Column_Type").Replace(":", "") + ", ";
            }
            if (filterOption.ModifiedDateInfo != null && filterOption.ModifiedDateInfo.Condition != DateCondition.None)
            {
                result += I18NEntity.GetString("RM_PRM_PRE_Column_ModifiedTime").Replace(":", "") + ", ";
            }
            if ((filterOption.TermIds != null && filterOption.TermIds.Count > 0) || (filterOption.WithOutTerms.HasValue && filterOption.WithOutTerms.Value))
            {
                result += I18NEntity.GetString("RM_PRM_PRE_PanelTitle_DisposalClass").Replace(":", "") + ", ";
            }
            #endregion

            #region more filter
            if (filterOption.CreatedBy != null && filterOption.CreatedBy.Count > 0)
            {
                result += I18NEntity.GetString("RM_JS_BCM_Explorer_Datagrid_Author") + ", ";
            }
            if (filterOption.ModifiedBy != null && filterOption.ModifiedBy.Count > 0)
            {
                result += I18NEntity.GetString("RM_PRM_PRE_Filter_ModifiedBy").Replace(":", "") + ", ";
            }
            if (filterOption.DisposalDateInfo != null && filterOption.DisposalDateInfo.Condition != DateCondition.None)
            {
                result += I18NEntity.GetString("RM_JS_BCM_Explorer_Datagrid_DisposalDueDate") + ", ";
            }
            if (filterOption.Owners != null && filterOption.Owners.Count > 0)
            {
                result += I18NEntity.GetString("RM_JS_BCM_Explorer_Datagrid_RecordsOwner") + ", ";
            }
            if (filterOption.HoldStatus.HasValue)
            {
                result += I18NEntity.GetString("RM_JS_BCM_Explorer_Datagrid_OnHold") + ", ";
            }
            if (filterOption.HoldBy != null && filterOption.HoldBy.Count > 0)
            {
                result += I18NEntity.GetString("RM_JS_BCM_Explorer_Details_HoldBy").Replace(":", "") + ", ";
            }
            if (filterOption.CreatedDateInfo != null && filterOption.CreatedDateInfo.Condition != DateCondition.None)
            {
                result += I18NEntity.GetString("RM_JS_RDM_Explorer_CreateTime") + ", ";
            }
            if (filterOption.DeclaredRecord.HasValue)
            {
                result += I18NEntity.GetString("RM_JS_BCM_Explorer_Datagrid_Declared") + ", ";
            }
            #endregion

            #region Column in Template
            if (filterOption.CustomColumns != null && filterOption.CustomColumns.Count > 0)
            {
                var allColumns = GetAllColumns();
                foreach (var filterColumn in filterOption.CustomColumns)
                {
                    var columnName = allColumns.Where(c => c.UniqueId == new Guid(filterColumn.Column.Id)).Select(c => c.ColumnName).FirstOrDefault();
                    if (!string.IsNullOrEmpty(columnName))
                    {
                        result += I18NEntity.GetString(columnName) + ", ";
                    }
                }
            }
            #endregion

            if (filterOption.RuleIds != null && filterOption.RuleIds.Count > 0)
            {
                result += I18NEntity.GetString("RM_JS_BCM_Explorer_Datagrid_Rule") + ", ";
            }
            if (filterOption.SPNodes != null && filterOption.SPNodes.Count > 0)
            {
                result += I18NEntity.GetString("RM_JS_BCM_Explorer_Filter_SPLocation") + ", ";
            }

            if (result.LastIndexOf(", ") > 0)
            {
                result = result.Substring(0, result.LastIndexOf(", "));
            }
            return result;
        }

        private List<TemplateColumn4Display> GetAllColumns()
        {
            var results = new List<TemplateColumn4Display>();
            List<RMTemplate> templates = RMTemplateDao.GetTemplate();
            Dictionary<string, string> templateDic = new Dictionary<string, string>(); //key : template unique id, value: template name
            foreach (RMTemplate rm in templates)
            {
                templateDic[rm.UniqueId.ToString().ToLower()] = rm.Name; //template id and name dic

                foreach (var displyColumn in rm.GetColumnList4Display())
                {
                    var loadedColumn = results.FirstOrDefault(r => r.UniqueId == displyColumn.UniqueId);

                    if (loadedColumn == null)
                    {
                        results.Add(displyColumn);
                        continue;
                    }

                    //assign template id
                    var relatedTemplateIds = displyColumn.Templates.Select(o => o.Id);
                    var exceptionTemplateIds = relatedTemplateIds.Except(loadedColumn.Templates.Select(o => o.Id));
                    if (exceptionTemplateIds != null && exceptionTemplateIds.Count() > 0)
                    {
                        loadedColumn.Templates.AddRange(exceptionTemplateIds.Select(o =>
                        new NameAndIdDto { Id = o }));
                    }
                }
            }
            //assign template name
            foreach (var r in results)
            {
                foreach (var template in r.Templates)
                {
                    if (templateDic.ContainsKey(template.Id))
                    {
                        template.Name = templateDic[template.Id];
                    }
                }
            }
            return results;
        }
    }
}
