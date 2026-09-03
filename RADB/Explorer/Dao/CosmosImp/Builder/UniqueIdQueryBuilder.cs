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
using AvePoint.RA.Contract.RMWeb.Explorer;
using AvePoint.RA.Contract.TemplateManagement;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace AvePoint.RA.DB.Explorer.Dao.CosmosImp.Builder
{
    public class UniqueIdQueryBuilder : BaseArraySearchBuilder
    {
        private static List<string> builtInOrDefaultColumnIds = new List<string> { DefaultColumnIDs.UniqueId, BuildInColumnIDs.RecordsId };
        protected override List<string> GetSearchColumnIds()
        {
            return builtInOrDefaultColumnIds;
        }

        //protected override string[] SplitKey(string key)
        //{
        //    return key.ExplorerAnalyzeUniqueId();
        //}

        protected override string GetSearchArrayColumnName()
        {
            return CosmosConst.C_RecordIdArray;
        }

        protected override string GetSearchColumnName()
        {
            return CosmosConst.C_RecordId;
        }

        protected override ExplorerSearchOptionV2 Convert2SearchOptionV2(ExplorerQueryColumn column, string objJson, ExplorerSearchKeyOperationLogic keyOperationLogic)
        {
            return new ExplorerSearchOptionV2
            {
                Key = JsonConvert.DeserializeObject<string>(objJson).ToLower(),
                OperationLogic = keyOperationLogic,
                Columns = new List<ExplorerQueryColumn> {
                    new ExplorerQueryColumn {
                        Id = column.Id,
                        Name = column.Name
                    }
                }
            };
        }
    }
}
