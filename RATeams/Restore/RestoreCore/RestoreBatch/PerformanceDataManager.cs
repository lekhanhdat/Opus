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

namespace Office365GroupRestore
{
    using AvePoint.RA.CommonUtil;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;

    

    public class PerformanceDataManager
    {
        private static readonly RALogger logger = RALogger.GetInstance(typeof(PerformanceDataManager));

        private Stopwatch stopwatch = new Stopwatch();
        private List<PerformanceData> performanceDataList = new List<PerformanceData>();

        public PerformanceDataManager()
        {
        }

        public void Start()
        {
            try
            {
                stopwatch.Start();
            }
            catch (Exception ex)
            {
                logger.Error("An error occured when starting stopwatch, ex:{0}", ex);
            }
        }

        public void CollectPerformanceData(ExchangeDataBlockType dataBlockType, Int64 dataSize, Int64 itemCounts)
        {
            try
            {
                performanceDataList.Add(new PerformanceData() { dataBlockType = dataBlockType, collectionDataSize = dataSize, collectionCount = itemCounts });
            }
            catch (Exception ex)
            {
                logger.Error("An error occured when collect performance data, ex:{0}", ex);
            }
        }

        public void Finish()
        {
            try
            {
                stopwatch.Stop();
                var totalSeconds = stopwatch.Elapsed.TotalSeconds;
                var pLookup = performanceDataList.ToLookup(k => k.dataBlockType, v => v);
                logger.Info(">>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>");
                foreach (var packageGroup in pLookup)
                {
                    var totalSize = packageGroup.Select(data => data.collectionDataSize).Sum();
                    var totalCount = packageGroup.Select(data => data.collectionCount).Sum();
                    var speed = totalSize / totalSeconds * 60 * 60 / 1024 / 1024;
                    logger.Info("<Batch>:Type:[{0}], Total Size:[{1}], Total Count:[{2}],TotalSeconds:[{3}], Speed:{4} GB/H", packageGroup.Key, totalSize, totalCount, totalSeconds, speed);
                }
                logger.Info(">>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>");
            }
            catch (Exception ex)
            {
                logger.Error("An error occured when create performance log, ex:{0}", ex);
            }
        }
    }

    internal class PerformanceData
    {
        public ExchangeDataBlockType dataBlockType;
        public Int64 collectionDataSize = 0L;
        public Int64 collectionCount = 0L;
    }
}