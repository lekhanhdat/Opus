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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ExchangeBackupUtility.Graph;
using ExchangeItem = ExchangeBackupUtility.ExchangeItem;

namespace AvePoint.RA.RAExchange.Discover
{
    //此类被用于封装ExchangeItem 集合，用于批处理。如果check rule 需要在分Group之前，则需要另外封装类包含Item和Rule 的关系
    public class IExchangeItemGroup
    {
        private readonly int maxItemsCount;
        private readonly int maxSizeLimit;
        private const int LargeSizeItemLimit = 850 * 1024;

        private List<IExchangeItem> items;

        public IEnumerable<IExchangeItem> Items
        {
            get { return items; }
        }

        public int ItemsCount
        {
            get { return items.Count; }
        }

        /// <summary>
        /// Item Size总和
        /// </summary>
        public long TotalSize { get; private set; }

        public IExchangeItemGroup(IEnumerable<IExchangeItem> items)
        {
            this.items = items.ToList();
            this.TotalSize = items.Sum(itemArg => itemArg.ItemSize);
        }


        /// <summary>
        /// 构造RMEXODiscoverItemCollection集合
        /// </summary>
        /// <param name="maxItemsCount">集合中Item数量上限</param>
        /// <param name="maxSizeLimit">集合中Item Size总和上限</param>
        /// <param name="item">初始化集合的第一个Item, 对于大于maxSizeLimit的Item, 请使用这个构造方法添加</param>
        public IExchangeItemGroup(int maxItemsCount, int maxSizeLimit, IExchangeItem item)
            : this(maxItemsCount, maxSizeLimit)
        {
            AddInternal(item);
        }

        /// <summary>
        /// 构造RMEXODiscoverItemCollection集合
        /// </summary>
        /// <param name="maxItemsCount">集合中Item数量上限</param>
        /// <param name="maxSizeLimit">集合中Item Size总和上限</param>
        public IExchangeItemGroup(int maxItemsCount, int maxSizeLimit)
        {
            this.items = new List<IExchangeItem>();
            this.TotalSize = 0L;
            this.maxItemsCount = maxItemsCount;
            this.maxSizeLimit = maxSizeLimit;
        }

        private bool IsFull(IExchangeItem item)
        {
            return (this.items.Count + 1 > this.maxItemsCount || this.TotalSize + item.ItemSize > maxSizeLimit);
        }

        /// <summary>
        /// 添加Item到集合中
        /// </summary>
        /// <param name="item">添加的Item对象</param>
        /// <returns>是否添加成功, 超过Item数量和Size总和上限会返回false, 否则返回true</returns>
        private bool Add(IExchangeItem item)
        {
            if (IsFull(item)) return false;
            AddInternal(item);
            return true;
        }

        private void AddInternal(IExchangeItem item)
        {
            this.items.Add(item);
            this.TotalSize += item.ItemSize;
        }

        //如果今后算法过于复杂, 考虑将分组算法提出到内部类中
        /// <summary>
        /// 
        /// </summary>
        /// <param name="items"></param>
        /// <param name="maxItemCount">每组最大的Item个数</param>
        /// <param name="maxSizeLimit">每组最大的Size, 以字节为单位</param>
        /// <returns></returns>
        public static List<IExchangeItemGroup> GroupCachedItems(IEnumerable<IExchangeItem> items, int maxItemCount, int maxSizeLimit)
        {
            if (maxItemCount <= 0) throw new ArgumentException("maxItemCount must be greater than 0.");
            if (maxSizeLimit <= 0) throw new ArgumentException("maxSizeLimit must be greater than 0.");

            var groupedItems = new List<IExchangeItemGroup>();

            //AddFilteredOutItems(items.Where(i => i.IsFilteredOut), groupedItems);
            AddSmallSizeItems(items.Where(i => i.ItemSize <= LargeSizeItemLimit), groupedItems, maxItemCount, maxSizeLimit);
            AddBigSizeItems(items.Where(i =>i.ItemSize > LargeSizeItemLimit), groupedItems);
            return groupedItems;
        }

        //private static void AddFilteredOutItems(IEnumerable<ExchangeItem> items, List<RMEXODiscoverItemCollection> groupedItems)
        //{
        //    var collection = new FilteredOutRMEXODiscoverItemCollection(items);
        //    if (collection.ItemsCount > 0)
        //    {
        //        groupedItems.Add(collection);
        //    }
        //}

        private static void AddBigSizeItems(IEnumerable<IExchangeItem> cachedItems, List<IExchangeItemGroup> groupedItems)
        {
            foreach (var item in cachedItems)
            {
                groupedItems.Add(new IExchangeItemGroup(1, -1, item));
            }
        }

        private static void AddSmallSizeItems(IEnumerable<IExchangeItem> cachedItems, List<IExchangeItemGroup> groupedItems, int maxItemCount, int maxSizeLimit)
        {
            IExchangeItemGroup currentCollection = null;
            foreach (var item in cachedItems)
            {
                if (currentCollection == null || currentCollection.IsFull(item))
                {
                    currentCollection = new IExchangeItemGroup(maxItemCount, maxSizeLimit);
                    groupedItems.Add(currentCollection);
                }
                currentCollection.Add(item);
            }
        }


    }
}
