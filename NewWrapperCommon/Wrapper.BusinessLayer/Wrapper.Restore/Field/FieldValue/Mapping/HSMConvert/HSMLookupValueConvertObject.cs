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
using AvePoint.Wrapper.Common;

namespace AvePoint.Wrapper.Restore
{
    class HSMLookupValueConvertObject : LookupValueConvertObject
    {
        public HSMLookupValueConvertObject(IAveFieldLookup destField, AveSPItem mItem, int originalRowId, object sourceValue, int originalVersion, string sourceFieldName) 
            : base(destField, mItem, originalRowId, sourceValue, originalVersion, sourceFieldName)
        {
        }

        public override object ConvertSingleValue(string value)
        {
            var idInfo = new Dictionary<int, Guid>();
            if (!string.IsNullOrEmpty(destLookupField.LookupList))
            {
                idInfo = mItem.ParentSite.GetLookupItemIdAndUniqueIdByDisplayValue(destLookupField.LookupWebId, new Guid(destLookupField.LookupList), destLookupField.LookupField, value);
            }
            if (idInfo.Keys.Count == 0)
            {
                CacheLookupValueInfo();
                return null;
            }
            return new LookupItemValue
            {
                ItemRowId = idInfo.Keys.First(),
                ItemUniqueId = idInfo.Values.First(),
            };
        }

        public override object ConvertMultiValue(List<string> values)
        {
            bool needCache = false;
            var lookupValues = new List<LookupItemValue>();
            foreach (var displayValue in values)
            {
                var singleValue = ConvertSingleValue(displayValue);
                if (singleValue != null)
                {
                    lookupValues.Add(singleValue as LookupItemValue);
                }
                else
                {
                    needCache = true;
                }
            }
            if (needCache)
            {
                CacheLookupValueInfo();
                return null;
            }
            return lookupValues;
        }


    }
}
