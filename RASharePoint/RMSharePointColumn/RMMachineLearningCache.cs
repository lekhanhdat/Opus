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
using AvePoint.RA.CommonUtil;
using AvePoint.RA.SharePoint.Object;
using AvePoint.Wrapper.Common;
using DocumentFormat.OpenXml.Spreadsheet;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.SharePoint.RMSharePointColumn
{
    public class RMMLAutoSmartItemsCache : IDisposable
    {
        private static readonly RALogger logger = RALogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);
        private RMMLAutoSmartItemsCache() { }

        private static RMMLAutoSmartItemsCache _instance = null;

        private static readonly object locker = new();

        private readonly static object objLocker = new();

        private static readonly int _bulkSize = 100;

        private ConcurrentBag<AutoSmartCacheItemInfo> _items = new();

        private static Action<List<AutoSmartCacheItemInfo>> _itemsExcuteAction = null;

        public bool HasError { get; set; }

        public bool NeedProcessCache { get; private set; }

        public static RMMLAutoSmartItemsCache Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (locker)
                    {
                        if (_instance == null)
                        {
                            _instance = new RMMLAutoSmartItemsCache();
                        }
                    }
                }
                return _instance;
            }
        }

        public void Init(Action<List<AutoSmartCacheItemInfo>> processItemsAction)
        {
            SetItemExcuteAction(processItemsAction);
        }

        private void SetItemExcuteAction(Action<List<AutoSmartCacheItemInfo>> action)
        {
            if (action != null)
            {
                _itemsExcuteAction = action;
            }
        }

        public void ProcessItem(AutoSmartCacheItemInfo item)
        {
            lock (objLocker)
            {
                _items.Add(item);
                if (!NeedProcessCache)
                {
                    NeedProcessCache = true;
                }

                if (_items.Count > _bulkSize && _itemsExcuteAction != null)
                {
                    var processedItems = TakeItems(_bulkSize);
                    _itemsExcuteAction(processedItems.ToList());
                }
            }
        }
        
        private IEnumerable<AutoSmartCacheItemInfo> TakeItems(int count)
        {
            lock (objLocker)
            {
                List<AutoSmartCacheItemInfo> result = new();
                int tempCount = 0;
                while (tempCount < count)
                {
                    if (_items.TryTake(out AutoSmartCacheItemInfo item))
                    {
                        result.Add(item);
                    }
                    else 
                    {
                        break;
                    }
                    tempCount++;
                }
                return result;
            }
        }

        public void SetFinished()
        {
            if (_items != null && !_items.IsEmpty && _itemsExcuteAction != null)
            {
                _itemsExcuteAction(_items.ToList());
                logger.Info("Last processed items is finished.");
            }
            logger.Info("cache items is finished");
        }

        public void Dispose()
        {
            if (_instance != null)
            {
                HasError = false;
                NeedProcessCache = false;
                _items?.Clear();
            }
        }
    }
}
