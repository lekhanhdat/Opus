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
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using AvePoint.RA.Common.Threads;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.DB.Explorer.Dao;
using AvePoint.RA.DB.Explorer.Model;
using AvePoint.RA.RAPhysical.API;
using AvePoint.RA.RAPhysical.Common;
using AvePoint.RA.RAPhysical.Report;

namespace AvePoint.RA.RAPhysical.Discover
{
    public class PRDiscover : IPRDiscover
    {

        public IEnumerable<IPhysicalRecord> Discover(IList<Guid> ids)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<ItemsGroup<IPhysicalRecord>> GetItemsGroup(Expression<Func<IPhysicalRecord, bool>> predicate, int groupSize)
        {
            throw new NotImplementedException();
            //var result = new BlockingCollection<ItemsGroup<Record>>();
            //AveTenantTasks.Run(() =>
            //{
            //    try
            //    {
            //        //不断批量读取数据，添加到group中
                    
            //        var pageIndex = string.Empty;
            //        bool hasNext = true;
            //        while (hasNext)
            //        {
            //            var queryData = ExplorerDao.QueryDataWithoutTotal(pageIndex, groupSize, out hasNext, predicate);
            //            if (queryData.Item1.Count() == 0) break;

            //            if (queryData.Item2 != null)
            //            {
            //                pageIndex = queryData.Item2;
            //            }

            //            var itemGroup = new ItemsGroup<Record>();
            //            foreach (var record in queryData.Item1)
            //            {
            //                itemGroup.Add(record);
            //            }
            //            result.Add(itemGroup);
            //        }
            //    }
            //    catch (Exception e)
            //    {
            //        mLog.Error("An error occurred while browsing record.", e.ToString());
            //    }
            //    finally
            //    {
            //        result.CompleteAdding();
            //    }
            //}
            //);

            //return result.GetConsumingEnumerable();
        }

    }
}
