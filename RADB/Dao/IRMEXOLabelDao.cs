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
using AvePoint.RA.DB.Model;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace AvePoint.RA.DB.Dao
{
    public interface IRMEXOLabelDao : IBaseDao<RMEXOLabel>
    {
        List<T> GetFilterList<T>(Expression<Func<RMEXOLabel, T>> selectLambda, Expression<Func<RMEXOLabel, bool>> whereLambda);
        RMEXOLabel GetLabel(int type, int status);
        List<RMEXOLabel> GetLabelByStatus(int status);
        List<RMEXOLabel> GetLabelByStatusAndType(int status, int type);

        int RemoveOldFaildLabel(int type);
        List<RMEXOLabel> GetLabelByType(int type);
        Task<IEnumerable<RMEXOLabel>> LoadByPager(int pageIndex, int pageSize);
        Task<long> MultiGeoInsertEXOLabelTableAsync(IEnumerable<RMEXOLabel> exoLabels);
        Task<long> MultiGeoDeleteAllEXOLabelAsync();
    }
}
