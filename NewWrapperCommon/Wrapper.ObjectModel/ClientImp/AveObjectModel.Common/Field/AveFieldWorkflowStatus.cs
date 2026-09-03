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
using System.Globalization;
using AvePoint.Wrapper.Common;

namespace AvePoint.ObjectModel.Common
{
    class AveFieldWorkflowStatus : AveFieldChoice, IAveFieldWorkflowStatus
    {
        private IAveRequest mRequest;
        private AveList mParentList;
        private AveWeb mWeb;
        private AveFieldCollection mFieldCollection;
        private string mFieldSource;
        private Dictionary<string, object> mContentTypeProp;

        public AveFieldWorkflowStatus(IAveRequest request, AveList list, AveWeb web, string fieldSource, AveFieldCollection fieldCollection, Dictionary<string, object> contentTypeProp, Dictionary<string, object> prop)
            : base(request, list, web, fieldSource, fieldCollection, contentTypeProp, prop)
        {
            mRequest = request;
            mParentList = list;
            mWeb = web;
            mFieldCollection = fieldCollection;
            mFieldSource = fieldSource;
            mContentTypeProp = contentTypeProp;
            base.DataCache.AddPropertyies(prop);
        }

        public override object GetFieldValue(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return value;
            }
            int length = CultureInfo.InvariantCulture.CompareInfo.IndexOf(value, ";#", CompareOptions.Ordinal);
            if (length > 0)
            {
                value = value.Substring(0, length);
            }
            return Convert.ToInt32(value, CultureInfo.InvariantCulture);
        }
        public override string GetFieldValueAsText(object value)
        {
            if (value != null)
            {
                int fieldValue;
                if (value is string)
                {
                    fieldValue = (int)this.GetFieldValue((string)value);
                }
                else
                {
                    fieldValue = Convert.ToInt32(value, CultureInfo.InvariantCulture);
                }
                if ((fieldValue >= 0) && (fieldValue < base.Choices.Count))
                {
                    return base.Choices[fieldValue];
                }
            }
            return string.Empty;
        }
    }
}
