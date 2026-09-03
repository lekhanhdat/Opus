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
using AvePoint.RA.Contract.RMWeb.Tree.Base;
using Newtonsoft.Json;
using SqlKata;
using System;
using System.Collections.Generic;

namespace AvePoint.RA.DB.Explorer.Dao.CosmosImp.Builder
{
    /// <summary>
    /// loan date在Cosmos DB中没有对应的字段,不能直接查询,所以采取了先在外围找出符合loan date的box/folder的id，
    ///然后再用这些id去Cosmos DB查找box/folder(根据id)，以及对应的records(根据folderId)
    /// </summary>
    public class LoanDateQueryBuilder : IFilterBuilder, IAdvancedQueryBuilder
    {
        public Query Filter(Query query, ExplorerFilterOptionV2 filterOption)
        {
            if (!CanFilter(filterOption)) return query;
            IFilterBuilder idFilter = new IdQueryBuilder();
            IFilterBuilder folderFilter = new PhysicalFileQueryBuilder();
            IFilterBuilder nodeTypeFilter = new NodeTypeQueryBuilder();

            return query.Where(q =>
            {
                //return q.Where(q1 =>
                //{
                //    idFilter.Filter(q1, filterOption);
                //    nodeTypeFilter.Filter(q1, new ExplorerFilterOptionV2
                //    {
                //        NodeTypes = new List<RMNodeLevel> { RMNodeLevel.PhysicalBox, RMNodeLevel.PhysicalFile }
                //    });
                //    return q1;
                //})
                return idFilter.Filter(q, filterOption)
                .OrWhere(q3 =>
                {
                    folderFilter.Filter(q3, filterOption);
                    nodeTypeFilter.Filter(q3, new ExplorerFilterOptionV2
                    {
                        NodeTypes = new List<RMNodeLevel> { RMNodeLevel.PhysicalRecord }
                    });
                    return q3;
                });
            });

        }

        private bool CanFilter(ExplorerFilterOptionV2 filterOption)
        {
            return filterOption?.Ids != null;
        }

        #region Advanced search  
        public Query Build(Query query, ExplorerQueryColumn column, string objJson, ExplorerSearchColumnOperationLogic columnOperationLogic, ExplorerSearchKeyOperationLogic keyOperationLogic = ExplorerSearchKeyOperationLogic.AND)
        {
            if (!string.Equals(GetColumnId(), column.Id, System.StringComparison.OrdinalIgnoreCase)) return query;

            var filterOption = Convert2SearchOptionV2(column, objJson, columnOperationLogic, keyOperationLogic);
            return Filter(query, filterOption);
        }

        protected string GetColumnId()
        {
            return Contract.TemplateManagement.QueryCloumnIds.LoanDate;
        }
        protected ExplorerFilterOptionV2 Convert2SearchOptionV2(ExplorerQueryColumn column, string objJson, ExplorerSearchColumnOperationLogic columnOperationLogic, ExplorerSearchKeyOperationLogic keyOperationLogic)
        {
            var objIds = JsonConvert.DeserializeObject<List<Guid>>(objJson);
            return new ExplorerFilterOptionV2() {
                Ids = objIds, 
                PhysicalFileIds = objIds, 
            };
        }
        #endregion
    }
}
