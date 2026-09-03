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

namespace AvePoint.ObjectModel.Server13
{
    class AveFieldLookupValueCollection : AveAbstractCommonCollection<IAveFieldLookupValue>, IAveFieldLookupValueCollection
    {
        private SPFieldLookupValueCollection mFieldLookupValues;

        internal SPFieldLookupValueCollection FieldLookupValues
        {
            get { return mFieldLookupValues; }
        }

        public AveFieldLookupValueCollection(SPFieldLookupValueCollection fieldLookupValues)
            : base(fieldLookupValues)
        {
            mFieldLookupValues = fieldLookupValues;
        }

        public AveFieldLookupValueCollection()
            : this(new SPFieldLookupValueCollection())
        { }

        #region IList Members

        public void Add(IAveFieldLookupValue value)
        {
            mFieldLookupValues.Add((value as AveFieldLookupValue).FieldLookupValue);
        }

        public void Clear()
        {
            mFieldLookupValues.Clear();
        }

        public bool Contains(IAveFieldLookupValue value)
        {
            return mFieldLookupValues.Contains((value as AveFieldLookupValue).FieldLookupValue);
        }

        public void CopyTo(IAveFieldLookupValue[] values, int index)
        {
            CopyTo(values as Array, index);
        }

        public int IndexOf(IAveFieldLookupValue value)
        {
            return mFieldLookupValues.IndexOf((value as AveFieldLookupValue).FieldLookupValue);
        }

        public void Insert(int index, IAveFieldLookupValue value)
        {
            mFieldLookupValues.Insert(index, (value as AveFieldLookupValue).FieldLookupValue);
        }

        public bool IsFixedSize
        {
            get { return (mFieldLookupValues as IList).IsFixedSize; }
        }

        public bool IsReadOnly
        {
            get { return (mFieldLookupValues as IList).IsReadOnly; }
        }

        public bool Remove(IAveFieldLookupValue value)
        {
            return mFieldLookupValues.Remove((value as AveFieldLookupValue).FieldLookupValue);
        }

        public void RemoveAt(int index)
        {
            mFieldLookupValues.RemoveAt(index);
        }

        #endregion

        protected override object CreatElementInstance(object t)
        {
            return new AveFieldLookupValue(t as SPFieldLookupValue);
        }

        public override int Count
        {
            get { return mFieldLookupValues.Count; }
        }

        public new IAveFieldLookupValue this[int index]
        {
            get
            {
                SPFieldLookupValue fieldLookupValue = mFieldLookupValues[index];
                if (fieldLookupValue == null)
                {
                    return null;
                }
                return new AveFieldLookupValue(fieldLookupValue);
            }
            set
            {
                AveFieldLookupValue fieldLookupValue = value as AveFieldLookupValue;
                if (fieldLookupValue != null)
                {
                    mFieldLookupValues[index] = fieldLookupValue.FieldLookupValue;
                }
                else
                {
                    mFieldLookupValues[index] = null;
                }
            }
        }

        public override string ToString()
        {
            return mFieldLookupValues.ToString();
        }
    }
}
