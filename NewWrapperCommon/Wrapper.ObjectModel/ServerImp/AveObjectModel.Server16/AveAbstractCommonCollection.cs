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
using System.Collections;
using System.Collections.Generic;

namespace AvePoint.ObjectModel.Server16
{
    internal abstract class AveAbstractCommonCollection<T> : AveServerObject, IEnumerable<T>, ICollection
    {
        public AveAbstractCommonCollection(IEnumerable enumerable)
        {
            mEnumerable = enumerable;
        }

        protected IEnumerable mEnumerable;

        public virtual IEnumerator<T> GetEnumerator()
        {
            foreach (object t in mEnumerable)
            {
                if (t != null)
                {
                    yield return (T)CreatElementInstance(t);
                }
                else
                {
                    yield return default(T);
                }
            }
        }

        protected abstract object CreatElementInstance(object t);

        public virtual T this[int index]
        {
            get
            {
                throw new Exception("UnImplement");
            }
        }

        public abstract int Count
        {
            get;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        public void CopyTo(Array array, int index)
        {
            if (array == null)
            {
                throw new ArgumentNullException("array");
            }
            int startIndex = index;
            for (int i = 0; i < this.Count; i++)
            {
                array.SetValue(this[i], startIndex + i);
            }
        }

        public bool IsSynchronized
        {
            get
            {
                return false;
            }
        }

        public object SyncRoot
        {
            get
            {
                return this;
            }
        }
    }

    internal abstract class AveAbstractCommonCollection : IEnumerable, IEnumerator
    {
        private IEnumerator mEnumerator;

        public AveAbstractCommonCollection(IEnumerable enumerable)
        {
            mEnumerator = enumerable.GetEnumerator();
        }

        public object Current
        {
            get
            {
                object obj = mEnumerator.Current;
                if (obj != null)
                {
                    return CreatElementInstance(mEnumerator.Current);
                }
                return obj;
            }
        }

        internal abstract object CreatElementInstance(object obj);

        public bool MoveNext()
        {
            return mEnumerator.MoveNext();
        }

        public void Reset()
        {
            mEnumerator.Reset();
        }

        public IEnumerator GetEnumerator()
        {
            return this;
        }
    }
}
