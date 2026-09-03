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



using Microsoft.SharePoint;
using AvePoint.Wrapper.Common;
using System;

namespace AvePoint.ObjectModel.ServerSE
{
    class AveFieldRatingScale : AveFieldMultiChoice, IAveFieldRatingScale
    {
        private SPFieldRatingScale mFieldRatingScale;

        public AveFieldRatingScale(AveFieldCollection fieldColl, SPFieldRatingScale field)
            : base(fieldColl, field)
        {
            mFieldRatingScale = field;
        }

        public override Type FieldValueType
        {
            get
            {
                return typeof(IAveFieldRatingScaleValue);
            }
        }

        #region IAveFieldRatingScale Members

        public int GridEndNumber
        {
            get
            {
                return mFieldRatingScale.GridEndNumber;
            }
            set
            {
                mFieldRatingScale.GridEndNumber = value;
            }
        }

        public string GridNAOptionText
        {
            get
            {
                return mFieldRatingScale.GridNAOptionText;
            }
            set
            {
                mFieldRatingScale.GridNAOptionText = value;
            }
        }

        public int GridStartNumber
        {
            get
            {
                return mFieldRatingScale.GridStartNumber;
            }
            set
            {
                mFieldRatingScale.GridStartNumber = value;
            }
        }

        public string GridTextRangeAverage
        {
            get
            {
                return mFieldRatingScale.GridTextRangeAverage;
            }
            set
            {
                mFieldRatingScale.GridTextRangeAverage = value;
            }
        }

        public string GridTextRangeHigh
        {
            get
            {
                return mFieldRatingScale.GridTextRangeHigh;
            }
            set
            {
                mFieldRatingScale.GridTextRangeHigh = value;
            }
        }

        public string GridTextRangeLow
        {
            get
            {
                return mFieldRatingScale.GridTextRangeLow;
            }
            set
            {
                mFieldRatingScale.GridTextRangeLow = value;
            }
        }

        public int RangeCount
        {
            get { return mFieldRatingScale.RangeCount; }
        }

        public override object GetFieldValue(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return null;
            }
            return new AveFieldRatingScaleValue(new SPFieldRatingScaleValue(value));
        }

        public override string GetFieldValueAsText(object value)
        {
            if (value == null)
            {
                return string.Empty;
            }
            return value.ToString();
        }

        #endregion
    }
}
