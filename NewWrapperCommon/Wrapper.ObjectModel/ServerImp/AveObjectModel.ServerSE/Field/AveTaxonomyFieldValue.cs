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



using Microsoft.SharePoint.Taxonomy;
using AvePoint.Wrapper.Common;

namespace AvePoint.ObjectModel.ServerSE
{
    class AveTaxonomyFieldValue : IAveTaxonomyFieldValue
    {
        private TaxonomyFieldValue mTaxonomyFieldValue;

        public AveTaxonomyFieldValue(TaxonomyFieldValue spTaxonomyFieldValue)
        {
            mTaxonomyFieldValue = spTaxonomyFieldValue;
        }

        public AveTaxonomyFieldValue(IAveField creatingField)
        {
            mTaxonomyFieldValue = new TaxonomyFieldValue((creatingField as AveField).Field);
        }

        public AveTaxonomyFieldValue(string value)
        {
            mTaxonomyFieldValue = new TaxonomyFieldValue(value);
        }

        internal TaxonomyFieldValue TaxonomyFieldValue
        {
            get
            {
                return mTaxonomyFieldValue;
            }
        }

        #region IAveTaxonomyFieldValue Members

        public string Label
        {
            get
            {
                return mTaxonomyFieldValue.Label;
            }
            set
            {
                mTaxonomyFieldValue.Label = value;
            }
        }

        public string TermGuid
        {
            get
            {
                return mTaxonomyFieldValue.TermGuid;
            }
            set
            {
                mTaxonomyFieldValue.TermGuid = value;
            }
        }

        public string ValidatedString
        {
            get { return mTaxonomyFieldValue.ValidatedString; }
        }

        public int WssId
        {
            get
            {
                return mTaxonomyFieldValue.WssId;
            }
            set
            {
                mTaxonomyFieldValue.WssId = value;
            }
        }

        public void PopulateFromLabelGuidPair(string text)
        {
            mTaxonomyFieldValue.PopulateFromLabelGuidPair(text);
        }

        public override string ToString()
        {
            if (mTaxonomyFieldValue != null)
            {
                return mTaxonomyFieldValue.ToString();
            }
            return base.ToString();
        }

        #endregion
    }
}
