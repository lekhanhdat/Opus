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
//using AvePoint.RA.Contract.RMWeb.Explorer;
//using AvePoint.RA.Contract.TemplateManagement;
//using SqlKata;
//using System;

//namespace AvePoint.RA.DB.Explorer.Dao.CosmosImp.Builder
//{
//    /// <summary>
//    /// Name or UniqueId query together
//    /// </summary>
//    public class NameOrUniqueIdQueryBuilder : IAdvancedQueryBuilder
//    {
//        public Query Build(Query query, ExplorerQueryColumn column, string objJson, ExplorerSearchColumnOperationLogic columnOperationLogic, ExplorerSearchKeyOperationLogic keyOperationLogic = ExplorerSearchKeyOperationLogic.AND)
//        {
//            if (!CanDo(column)) return query;

//            IAdvancedQueryBuilder nameQueryBuilder = new NameQueryBuilder();
//            IAdvancedQueryBuilder IdQueryBuilder = new UniqueIdQueryBuilder();
//            query.Where(q1 =>
//            {
//                q1.Where(q2 =>
//                {
//                    nameQueryBuilder.Build(q2, new ExplorerQueryColumn { Id = DefaultColumnIDs.NameOrTitle }, objJson, columnOperationLogic, keyOperationLogic);
//                    return q2;
//                }).OrWhere(q3 =>
//                {
//                    IdQueryBuilder.Build(q3, new ExplorerQueryColumn { Id = DefaultColumnIDs.UniqueId }, objJson, columnOperationLogic, keyOperationLogic);
//                    return q3;
//                });

//                return q1;
//            });

//            return query;
//        }

//        private bool CanDo(ExplorerQueryColumn column)
//        {
//            return string.Equals(QueryCloumnIds.NameOrUniqueId, column.Id, StringComparison.OrdinalIgnoreCase);
//        }
//    }
//}
