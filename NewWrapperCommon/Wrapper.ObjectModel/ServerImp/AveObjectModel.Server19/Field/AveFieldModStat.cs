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
    class AveFieldModStat : AveFieldChoice, IAveFieldModStat
    {
        private SPFieldModStat mFieldModStat;

        public AveFieldModStat(AveFieldCollection fieldColl, SPFieldModStat field)
            : base(fieldColl, field)
        {
            mFieldModStat = field;
        }

        public override Type FieldValueType
        {
            get
            {
                return typeof(int);
            }
        }

        public override object GetFieldValue(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return value;
            }
            return Convert.ToInt32(value, CultureInfo.InvariantCulture);
        }

        public override string GetFieldValueAsText(object value)
        {
            return mFieldModStat.GetFieldValueAsText(value);
        }
    }
}
