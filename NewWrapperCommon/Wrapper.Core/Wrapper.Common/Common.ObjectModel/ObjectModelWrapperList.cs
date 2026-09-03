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

namespace AvePoint.Wrapper.Common
{
    /// <summary>
    /// 使用这个类会将对AveObjList的修改同步到SPObjList中
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="TSPObject"></typeparam>
    internal class ObjectModelWrapperList<T, TSPObject> : List<T>, IList<T>
    {
        private IList<TSPObject> spCollection;
        private Func<T, TSPObject> aveToSPSelector;

        /// <summary>
        /// 初始化集合类, 对AveObjList的修改同步到collection中
        /// </summary>
        /// <param name="collection">SharePoint object list</param>
        /// <param name="spToAveSelector">SP对象到Ave对象的Selector</param>
        /// <param name="aveToSPSelector">Ave对象到SP对象的Selector</param>
        public ObjectModelWrapperList(IList<TSPObject> collection, Func<TSPObject, T> spToAveSelector, Func<T, TSPObject> aveToSPSelector)
            : base(SelectEnumerableT(collection, spToAveSelector))
        {
            if (aveToSPSelector == null)
            {
                throw new ArgumentNullException("aveToSPSelector");
            }
            this.spCollection = collection ?? new List<TSPObject>();
            this.aveToSPSelector = aveToSPSelector;
        }

        private static IEnumerable<T> SelectEnumerableT(IList<TSPObject> collection, Func<TSPObject, T> spToAveSelector)
        {
            if (spToAveSelector == null)
            {
                throw new ArgumentNullException("spToAveSelector");
            }
            return collection == null ? null : collection.Select(spToAveSelector);
        }

        public void Insert(int index, T item)
        {
            this.spCollection.Insert(index, this.aveToSPSelector(item));
            base.Insert(index, item);
        }

        public void RemoveAt(int index)
        {
            this.spCollection.RemoveAt(index);
            base.RemoveAt(index);
        }

        public void Add(T item)
        {
            this.spCollection.Add(this.aveToSPSelector(item));
            base.Add(item);
        }

        public void Clear()
        {
            this.spCollection.Clear();
            base.Clear();
        }

        public bool Remove(T item)
        {
            this.spCollection.Remove(this.aveToSPSelector(item));
            return base.Remove(item);
        }
    }
}
