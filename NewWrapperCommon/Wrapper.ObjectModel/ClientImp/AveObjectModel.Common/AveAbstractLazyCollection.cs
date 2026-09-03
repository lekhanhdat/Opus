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
namespace AvePoint.ObjectModel.Common
{

    using System.Collections.Generic;

    internal abstract class AveAbstractLazyCollection<T> : AveAbstractCommonCollection<T>,IEnumerable<T>
    {
        protected object lockObject = new object();
        protected bool IsCollectionInitialized = false;
        protected abstract void InitCollection();

        internal override List<T> ListData
        {

            get
            {
                InitCollection();
                return mListData;
            }
        }

        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            InitCollection();
            return new AveCommonEnumerator<T>(this);
        }

        public override int Count
        {
            get
            {
                InitCollection();
                return base.Count;
            }
        }

        public override T this[int index]
        {
            get
            {
                InitCollection();
                return base[index];
            }
        }

        /// <summary>
        /// 如果集合没有初始化,不需要初始化缓存,当下次调用ListData时会通过request取数据加载缓存,会包含新加的object
        /// </summary>
        /// <param name="obj"></param>
        internal virtual void AddToCache(T obj)
        {
            if (IsCollectionInitialized)
            {
                ListData.Add(obj);
            }
        }
    }
}
