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
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RMWeb.Tree.Base;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.DB.Dao;
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
    public class PhysicalRecordsExplorerTelemetryGenerator : TelemetryGenerator
    {
        private readonly IAccountDao AccountDao = PlatformWindsorManager.GetService<IAccountDao>();

        public override TelemetryModule Module => TelemetryModule.PhysicalRecordsExplorer;

        protected override Dictionary<TelemetryEventType, Func<IList<object>, CloudRecordsCommonRecord>> EventTypeGenerateMapping =>
            new Dictionary<TelemetryEventType, Func<IList<object>, CloudRecordsCommonRecord>>
            {
                { TelemetryEventType.Filter, Filter },
                { TelemetryEventType.Search, Search },
                { TelemetryEventType.ContentPageLoaded, PageLoaded },
                { TelemetryEventType.BoxCreationRequest, RecordRequest },
                { TelemetryEventType.FolderCreationRequest, RecordRequest },
                { TelemetryEventType.RecordCreationRequest, RecordRequest },
                { TelemetryEventType.LoanRequest, RecordRequest },
            };

        public CloudRecordsCommonRecord RecordRequest(IList<object> args)
        {
            return new CloudRecordsPhysicalExplorerRecord();
        }

        public CloudRecordsCommonRecord PageLoaded(IList<object> args)
        {
            var record = new CloudRecordsPhysicalExplorerRecord();
            record.NonAdministrator = !AccountDao.CheckAdminRole(TenantLocalValue.LogonUserId);
            return record;
        }

        public CloudRecordsCommonRecord Search(IList<object> args)
        {
            var record = new CloudRecordsPhysicalExplorerSearchRecord();
            record.SearchResultMoreThanOnePage = Convert.ToBoolean(args[0]);
            record.SearchUsageTime = Convert.ToInt64(args[1]);
            return record;
        }

        public CloudRecordsCommonRecord Filter(IList<object> args)
        {
            var record = new CloudRecordsPhysicalExplorerSearchRecord();
            var phyFilters = JsonConvert.DeserializeObject<PhysicalExplorerFilterOption>(Convert.ToString(args[0]));
            record.SelectedFilters = GetPhysicalExplorerSelectedFilters(phyFilters);
            record.SearchUsageTime = Convert.ToInt64(args[1]);
            return record;
        }

        private string GetPhysicalExplorerSelectedFilters(PhysicalExplorerFilterOption filterOption)
        {
            string result = string.Empty;
            if (filterOption.NodeType != RMNodeLevel.Undefined && filterOption.NodeType != RMNodeLevel.RMSelectAll)
            {
                result += I18NEntity.GetString("RM_PRM_PRE_ItemType").Replace(":", "") + ", ";
            }
            if (filterOption.Status != (int)RMRecordStatus.None && filterOption.Status != (int)RMRecordStatus.All)
            {
                result += I18NEntity.GetString("RM_PRM_PRE_Column_Status").Replace(":", "") + ", ";
            }
            if (filterOption.RecordsOwner != null && filterOption.RecordsOwner.Count > 0)
            {
                result += I18NEntity.GetString("RM_PRM_PRE_Filter_RecordsOwner").Replace(":", "") + ", ";
            }
            if (filterOption.CreatedBy != null && filterOption.CreatedBy.Count > 0)
            {
                result += I18NEntity.GetString("RM_PRM_PRE_Filter_CreatedBy").Replace(":", "") + ", ";
            }
            if (filterOption.ModifiedBy != null && filterOption.ModifiedBy.Count > 0)
            {
                result += I18NEntity.GetString("RM_PRM_PRE_Filter_ModifiedBy").Replace(":", "") + ", ";
            }

            if (result.LastIndexOf(", ") > 0)
            {
                result = result.Substring(0, result.LastIndexOf(", "));
            }
            return result;
        }
    }
}
