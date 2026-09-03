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
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.RMWeb.Explorer;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;

namespace AvePoint.RA.DB.Explorer.Dao.CosmosImp.Builder
{
    public class ModifiedByQueryBuilder : BaseArrayFilterBuilder
    {
        //private static List<string> builtInOrDefaultColumnIds = new List<string> { DefaultColumnIDs.ModifiedBy, BuildInColumnIDs.ModifiedBy };

        protected override string GetFilterColumnName()
        {
            return CosmosConst.C_ModifiedByLower;
        }

        protected override bool CanFilter(ExplorerFilterOptionV2 filterOption)
        {
            return filterOption != null && filterOption.ModifiedBy != null && filterOption.ModifiedBy.Count > 0;

        }

        protected override object GetFilterValue(ExplorerFilterOptionV2 filterOption)
        {
            return filterOption.ModifiedBy.Select(o => o.DisplayName.ToLower()).ToList();
        }

        protected override string GetColumnId()
        {
            return Contract.TemplateManagement.QueryCloumnIds.ModifiedBy;
        }

        protected override ExplorerFilterOptionV2 Convert2SearchOptionV2(ExplorerQueryColumn column, string objJson, ExplorerSearchColumnOperationLogic columnOperationLogic, ExplorerSearchKeyOperationLogic keyOperationLogic)
        {
            return new ExplorerFilterOptionV2
            {
                ModifiedBy = JsonConvert.DeserializeObject<List<AOSUserDto>>(objJson)
            };
        }
    }
}
