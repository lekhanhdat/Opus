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
using AvePoint.RA.Contract.RMWeb.Physical.ColumnValues;
using AvePoint.RA.Contract.TemplateManagement;
using AvePoint.RA.DB.Explorer.Dao.CosmosImp.Builder.Extension;
using Newtonsoft.Json;
using SqlKata;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AvePoint.RA.DB.Explorer.Dao.CosmosImp.Builder
{
    public class CustomColumnSingleChoiceQueryBuilder : ICustomColumnFilterBuilder, IAdvancedQueryBuilder
    {
        public Query Filter(Query query, ExplorerQueryColumn column, string objJson)
        {
            if (CanFilter(column))
            {
                if (column.IdsWithDuplicateName != null && column.IdsWithDuplicateName.Count > 1)
                {
                    query.Where(q =>
                    {
                        foreach (Guid id in column.IdsWithDuplicateName)
                        {
                            q.OrWhere(a => a.WhereArrayContainV2(GetFilterObject(objJson), GetColumnName(column, id)));
                        }
                        return q;
                    });
                }
                else
                {
                    query.WhereArrayContainV2(GetFilterObject(objJson), GetColumnName(column));
                }
                
            }

            return query;
        }

        private string GetColumnName(ExplorerQueryColumn column)
        {
            ///if column is Status, should search 'Value' field instead of 'Name' field in Cosmos DB
            return column.Id == DefaultColumnIDs.Status ? column.GetCustomColumnName_Value() : column.GetCustomColumnName_Name();
        }
        private string GetColumnName(ExplorerQueryColumn column, Guid id)
        {
            ///if column is Status, should search 'Value' field instead of 'Name' field in Cosmos DB
            return column.Id == DefaultColumnIDs.Status ? column.GetCustomColumnName_Value(id) : column.GetCustomColumnName_Name(id);
        }
        private string[] GetFilterObject(string objJson)
        {
            List<ChoiceColumnValue> choices = JsonConvert.DeserializeObject<List<ChoiceColumnValue>>(objJson);
            return choices.Select(o => o.Value).ToArray();
        }

        private bool CanFilter(ExplorerQueryColumn column)
        {
            return !string.IsNullOrEmpty(column.Id) && column.Type == Contract.TemplateManagement.ColumnType.SingleChoice;
        }

        public Query Build(Query query, ExplorerQueryColumn column, string objJson, ExplorerSearchColumnOperationLogic columnOperationLogic, ExplorerSearchKeyOperationLogic keyOperationLogic = ExplorerSearchKeyOperationLogic.AND)
        {
            return Filter(query, column, objJson);
        }
    }
}
