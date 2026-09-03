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
using Microsoft.SharePoint.Taxonomy;
using AvePoint.Wrapper.Common;

namespace AvePoint.ObjectModel.Server16
{
    class AveTaxonomyFieldValueCollection : AveAbstractCommonCollection<IAveTaxonomyFieldValue>, IAveTaxonomyFieldValueCollection
    {
        private TaxonomyFieldValueCollection mTaxonomyFieldValueCollection;

        public AveTaxonomyFieldValueCollection(TaxonomyFieldValueCollection taxonomyFieldValues)
            : base(taxonomyFieldValues)
        {
            mTaxonomyFieldValueCollection = taxonomyFieldValues;
        }

        public AveTaxonomyFieldValueCollection(IAveField creatingField)
            : this(new TaxonomyFieldValueCollection((creatingField as AveField).Field))
        { }

        #region IAveTaxonomyFieldValueCollection Members

        public void Add(IAveTaxonomyFieldValue value)
        {
            mTaxonomyFieldValueCollection.Add((value as AveTaxonomyFieldValue).TaxonomyFieldValue);
        }

        public int IndexOf(IAveTaxonomyFieldValue item)
        {
            return mTaxonomyFieldValueCollection.IndexOf((item as AveTaxonomyFieldValue).TaxonomyFieldValue);
        }

        public void Insert(int index, IAveTaxonomyFieldValue item)
        {
            mTaxonomyFieldValueCollection.Insert(index, (item as AveTaxonomyFieldValue).TaxonomyFieldValue);
        }

        public void RemoveAt(int index)
        {
            mTaxonomyFieldValueCollection.RemoveAt(index);
        }

        public new IAveTaxonomyFieldValue this[int index]
        {
            get
            {
                TaxonomyFieldValue taxonomyFieldValue = mTaxonomyFieldValueCollection[index];
                if (taxonomyFieldValue == null)
                {
                    return null;
                }
                return new AveTaxonomyFieldValue(taxonomyFieldValue);
            }
            set
            {
                AveTaxonomyFieldValue taxonomyFieldValue = value as AveTaxonomyFieldValue;
                if (taxonomyFieldValue != null)
                {
                    mTaxonomyFieldValueCollection[index] = taxonomyFieldValue.TaxonomyFieldValue;
                }
                else
                {
                    mTaxonomyFieldValueCollection[index] = null;
                }
            }
        }

        public void Clear()
        {
            mTaxonomyFieldValueCollection.Clear();
        }

        public bool Contains(IAveTaxonomyFieldValue item)
        {
            return mTaxonomyFieldValueCollection.Contains((item as AveTaxonomyFieldValue).TaxonomyFieldValue);
        }

        public void CopyTo(IAveTaxonomyFieldValue[] array, int arrayIndex)
        {
            CopyTo(array as Array, arrayIndex);
        }

        public bool IsReadOnly
        {
            get { return (mTaxonomyFieldValueCollection as IList).IsReadOnly; }
        }

        public bool Remove(IAveTaxonomyFieldValue item)
        {
            return mTaxonomyFieldValueCollection.Remove((item as AveTaxonomyFieldValue).TaxonomyFieldValue);
        }

        protected override object CreatElementInstance(object t)
        {
            return new AveTaxonomyFieldValue(t as TaxonomyFieldValue);
        }

        public override int Count
        {
            get { return mTaxonomyFieldValueCollection.Count; }
        }

        internal TaxonomyFieldValueCollection TaxonomyFieldValueCollection
        {
            get
            {
                return mTaxonomyFieldValueCollection;
            }
        }

        public override string ToString()
        {
            if (mTaxonomyFieldValueCollection != null)
            {
                return mTaxonomyFieldValueCollection.ToString();
            }
            return base.ToString();
        }

        #endregion
    }
}
