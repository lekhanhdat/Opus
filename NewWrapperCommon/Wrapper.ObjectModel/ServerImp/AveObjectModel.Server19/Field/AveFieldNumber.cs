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
using System.Globalization;

namespace AvePoint.ObjectModel.Server19
{
    class AveFieldNumber : AveField, IAveFieldNumber
    {
        private SPFieldNumber mFieldNumber;

        public AveFieldNumber(AveFieldCollection fieldColl, SPFieldNumber field)
            : base(fieldColl, field)
        {
            mFieldNumber = field;
        }

        public AveFieldNumber(SPFieldNumber fieldNumber)
            : base(fieldNumber)
        {
            mFieldNumber = fieldNumber;
        }

        internal SPFieldNumber FieldNumber
        {
            get
            {
                return mFieldNumber;
            }
        }

        public override Type FieldValueType
        {
            get
            {
                return typeof(double);
            }
        }

        public override string IMEMode
        {
            get
            {
                return "inactive";
            }
        }

        public override object GetFieldValue(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return value;
            }
            return Convert.ToDouble(value, CultureInfo.InvariantCulture);
        }

        public override string GetFieldValueAsText(object value)
        {
            return mFieldNumber.GetFieldValueAsText(value);
        }

        #region IAveFieldNumber Members

        public double MaximumValue
        {
            get
            {
                return mFieldNumber.MaximumValue;
            }
            set
            {
                mFieldNumber.MaximumValue = value;
            }
        }

        public double MinimumValue
        {
            get
            {
                return mFieldNumber.MinimumValue;
            }
            set
            {
                mFieldNumber.MinimumValue = value;
            }
        }

        public virtual bool ShowAsPercentage
        {
            get
            {
                return mFieldNumber.ShowAsPercentage;
            }
            set
            {
                mFieldNumber.ShowAsPercentage = value;
            }
        }

        public AveNumberFormatTypes DisplayFormat
        {
            get
            {
                return (AveNumberFormatTypes)mFieldNumber.DisplayFormat;
            }
            set
            {
                mFieldNumber.DisplayFormat = (SPNumberFormatTypes)value;
            }
        }

        #endregion
    }
}
