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

namespace Microsoft365.Common.RequestMonitor
{
    using System;
    using System.Linq;
    using System.Collections.Generic;

    public class TimerRange : IDisposable
    {
        private static class DateTimeExtension
        {
            public static DateTime GetMin(params DateTime[] dateTimes)
            {
                var times = dateTimes.ToList();
                times.Sort();
                return times.FirstOrDefault();
            }
        }
        public string Name { get; set; }
        protected bool IsEnsured { get; set; } = false;

        private List<RangeItem> rangeList = new List<RangeItem>();

        public void AddRange(RangeItem newItem)
        {
            lock (rangeList)
            {
                bool processed = false;
                foreach (var item in rangeList)
                {
                    if (AddInternal(newItem.Start, newItem.End, item))
                    {
                        break;
                    }
                }
                if (!processed)
                {
                    rangeList.Add(newItem);
                }
                IsEnsured = false;
            }
        }

        public IList<RangeItem> GetDetails()
        {
            lock (rangeList)
            {
                EnsureRange();
                var list = new List<RangeItem>();
                list.AddRange(rangeList);
                return list;
            }
        }
        public TimeSpan GetTotalTime()
        {
            lock (rangeList)
            {
                EnsureRange();
                TimeSpan cost = new TimeSpan();
                foreach (var item in rangeList)
                {
                    cost += item.Cost;
                }
                return cost;
            }
        }

        protected bool AddInternal(DateTime start, DateTime end, RangeItem item)
        {
            bool processed = false;
            if (item.End >= start && start >= item.Start)
            {
                item.End = end > item.End ? end : item.End;
                processed = true;
            }
            else if (item.End >= end && end >= item.Start)
            {
                item.Start = DateTimeExtension.GetMin(start, item.Start);
                // item.Start = start > item.Start ? item.Start : start;
                processed = true;

            }
            else if (item.End <= end && start <= item.Start)
            {
                item.Start = start;
                item.End = end;
                // item.Start = start > item.Start ? item.Start : start;
                processed = true;
            }
            return processed;
        }

        private void EnsureRange()
        {
            if (IsEnsured)
            {
                return;
            }
            for (int k = rangeList.Count - 1; k > 0; k--)
            {
                var seedItem = rangeList[k];
                for (int x = k - 1; x >= 0; x--)
                {
                    var compareItem = rangeList[x];
                    if (AddInternal(seedItem.Start, seedItem.End, compareItem))
                    {
                        rangeList.RemoveAt(k);
                        break;
                    }
                }
            }
            IsEnsured = true;
        }

        public override string ToString()
        {
            return $"[{Name}]{GetTotalTime()}";
        }

        public void Dispose()
        {
            lock (rangeList)
            {
                rangeList.Clear();
            }
        }
    }
}