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
using Microsoft.SharePoint;
using AvePoint.Wrapper.Common;

namespace AvePoint.ObjectModel.Server16
{
    class AveFieldUserValueCollection : AveAbstractCommonCollection<IAveFieldUserValue>, IAveFieldUserValueCollection
    {
        private SPFieldUserValueCollection mFieldUserValueCollection;

        internal SPFieldUserValueCollection FieldUserValueCollection
        {
            get { return mFieldUserValueCollection; }
        }

        public AveFieldUserValueCollection(SPFieldUserValueCollection fieldUserValues)
            : base(fieldUserValues)
        {
            mFieldUserValueCollection = fieldUserValues;
        }

        public AveFieldUserValueCollection()
            : this(new SPFieldUserValueCollection())
        { }

        #region IList Members

        public void Add(IAveFieldUserValue value)
        {
            mFieldUserValueCollection.Add((value as AveFieldUserValue).FieldUserValue);
        }

        public void Clear()
        {
            mFieldUserValueCollection.Clear();
        }

        public bool Contains(IAveFieldUserValue value)
        {
            return mFieldUserValueCollection.Contains((value as AveFieldUserValue).FieldUserValue);
        }

        public void CopyTo(IAveFieldUserValue[] values, int index)
        {
            CopyTo(values as Array, index);
        }

        public int IndexOf(IAveFieldUserValue value)
        {
            return mFieldUserValueCollection.IndexOf((value as AveFieldUserValue).FieldUserValue);
        }

        public void Insert(int index, IAveFieldUserValue value)
        {
            mFieldUserValueCollection.Insert(index, (value as AveFieldUserValue).FieldUserValue);
        }

        public bool IsFixedSize
        {
            get { return (mFieldUserValueCollection as IList).IsFixedSize; }
        }

        public bool IsReadOnly
        {
            get { return (mFieldUserValueCollection as IList).IsReadOnly; }
        }

        public bool Remove(IAveFieldUserValue value)
        {
            return mFieldUserValueCollection.Remove((value as AveFieldUserValue).FieldUserValue);
        }

        public void RemoveAt(int index)
        {
            mFieldUserValueCollection.RemoveAt(index);
        }

        #endregion

        protected override object CreatElementInstance(object t)
        {
            return new AveFieldUserValue(t as SPFieldUserValue);
        }

        public override int Count
        {
            get { return mFieldUserValueCollection.Count; }
        }

        public new IAveFieldUserValue this[int index]
        {
            get
            {
                SPFieldUserValue fieldUserValue = mFieldUserValueCollection[index];
                if (fieldUserValue == null)
                {
                    return null;
                }
                return new AveFieldUserValue(fieldUserValue);
            }
            set
            {
                AveFieldUserValue fieldUserValue = value as AveFieldUserValue;
                if (fieldUserValue != null)
                {
                    mFieldUserValueCollection[index] = fieldUserValue.FieldUserValue;
                }
                else
                {
                    mFieldUserValueCollection[index] = null;
                }
            }
        }

        public override string ToString()
        {
            return mFieldUserValueCollection.ToString();
        }
    }
}
