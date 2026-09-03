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
//using AvePoint.GCommon;
//using System;
//using System.Collections.Generic;
//using System.Reflection;

//namespace AvePoint.RA.FileSystem.Core
//{
//    public class UniqueIdService : IUniqueIdService
//    {
//        private static readonly AveLogger logger = AveLogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);
//       // private IRMLockDao RecordLock { get; set; }
//        private ICacheService<long> Cache { get; set; }
//        private int _allocatedCount;
//        public string UniqueIdPrefix { get; set; }       
//        public UniqueIdService()
//        {
//            Cache = new MemoryListCacheService<long>();
//            //RecordLock = new RMLockDao();
//            _allocatedCount = 0;
           
//        }

//        public void Allocate(int count)
//        {
//            try
//            {
//                _allocatedCount += count;
//                if (Cache.Count > _allocatedCount) return;
//                var result = JobContext.Current.ApiClient.AllocateNumbers(count);
//                    //RecordLock.AllocateNumbers(count);
//                if (result == null)
//                {
//                    throw new Exception("Failed to allocate the lock number.");
//                }

//                var temp = new List<long>();
//                for (var i = result.Item1; i < result.Item2; i++)
//                {
//                    temp.Add(i + 1);
//                }
//                Cache.AddBatch(temp);
//            }
//            catch (Exception ex)
//            {
//                logger.Error("Failed to allocate the lock number. Exception:{0}", ex.ToString());
//                throw;
//            }
//        }
//        public string Next()
//        {
//            var r = Cache.Take();
//            while (r == 0)
//            {
//                Allocate(3);
//                r = Cache.Take();
//            }
//            _allocatedCount--;
//            if (string.IsNullOrEmpty(UniqueIdPrefix))
//            {
//                return string.Format("{0:D10}", r);
//            }
//            else
//            {
//                return string.Format("{0}-{1:D10}", UniqueIdPrefix, r);
//            }

//        }
//    }
//}
