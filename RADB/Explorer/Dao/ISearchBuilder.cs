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

namespace AvePoint.RA.DB.Explorer.Dao
{
    public interface ISearchBuilder
    {
        SqlKata.Query Search(SqlKata.Query query, ExplorerSearchOptionV2 searchOption);
    }

    public interface IFilterBuilder
    {
        SqlKata.Query Filter(SqlKata.Query query, ExplorerFilterOptionV2 filterOption);
    }

    /// <summary>
    /// Advanced search builder which unify the search and filter together.
    /// </summary>
    public interface IAdvancedQueryBuilder
    {
        /// <summary>
        /// build advanced search query
        /// </summary>
        /// <param name="query"></param>
        /// <param name="column"></param>
        /// <param name="objJson"></param>
        /// <param name="columnOperationLogic"></param>
        /// <param name="keyOperationLogic"></param>
        /// <returns></returns>
        SqlKata.Query Build(SqlKata.Query query, ExplorerQueryColumn column, string objJson, ExplorerSearchColumnOperationLogic columnOperationLogic, ExplorerSearchKeyOperationLogic keyOperationLogic = ExplorerSearchKeyOperationLogic.AND);
    }

    public interface IObjectArrayFilterBuilder<T>
    {
        SqlKata.Query Filter(SqlKata.Query query, T[] values);
    }

    public interface ICustomColumnSearchBuilder : IAdvancedQueryBuilder
    {
        SqlKata.Query Search(SqlKata.Query query, ExplorerQueryColumn column, string key, ExplorerSearchKeyOperationLogic OperationLogic = ExplorerSearchKeyOperationLogic.AND);
    }

    public interface ICustomColumnFilterBuilder : IAdvancedQueryBuilder
    {
        SqlKata.Query Filter(SqlKata.Query query, ExplorerQueryColumn column, string objJson);
    }
}
