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
using System.Collections;
using AvePoint.Wrapper.Common;

namespace AvePoint.ObjectModel.Common
{
    internal abstract class AveAbstractCommonCollection<T> : AveClientObject, IEnumerable<T>, ICollection
    {
        protected object lockObject = new object();
        protected bool IsCollectionInitialized = false;

        protected List<T> mListData;

        #region IEnumerable<T> Members

        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            return new AveCommonEnumerator<T>(this);
        }

        #endregion

        internal virtual List<T> ListData
        {
            get
            {
                return mListData;
            }
        }

        public virtual T this[int index]
        {
            get
            {
                ArgumentCheck.CheckBoundary(index, mListData);
                return mListData[index];
            }
        }

        #region IEnumerable Members

        IEnumerator IEnumerable.GetEnumerator()
        {
            return new AveCommonEnumerator<T>(this);
        }

        #endregion

        #region ICollection Members          

        public virtual int Count
        {
            get
            {
                return mListData.Count;
            }
        }

        public virtual void CopyTo(Array array, int index)
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

        public virtual bool IsSynchronized
        {
            get { return false; }
        }

        public virtual object SyncRoot
        {
            get { return this; }
        }

        #endregion

    }

    internal sealed class AveCommonEnumerator<T> : IEnumerator<T>
    {
        private AveAbstractCommonCollection<T> m_data;
        private int index = -1;

        public AveCommonEnumerator(AveAbstractCommonCollection<T> data)
        {
            m_data = data;
        }

        #region IEnumerator<T> Members

        public T Current
        {
            get { return m_data[index]; }
        }

        #endregion

        #region IDisposable Members

        public void Dispose()
        {
            index = -1;
            m_data = null;
        }

        #endregion

        #region IEnumerator Members

        object IEnumerator.Current
        {
            get { return m_data[index]; }
        }

        public bool MoveNext()
        {
            return ++index < m_data.Count;
        }

        public void Reset()
        {
            index = -1;
        }

        #endregion
    }
}
