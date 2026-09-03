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

namespace AvePoint.ObjectModel.ServerSE
{
    class AveFieldWorkflowStatus : AveFieldChoice, IAveFieldWorkflowStatus
    {
        private SPFieldWorkflowStatus mFieldWorkflowStatus;

        public AveFieldWorkflowStatus(SPFieldChoice fieldWorkflowStatus)
            : base(fieldWorkflowStatus)
        {
            mFieldWorkflowStatus = (SPFieldWorkflowStatus)fieldWorkflowStatus;
        }

        public AveFieldWorkflowStatus(AveFieldCollection fields, SPFieldWorkflowStatus fieldWorkflowStatus)
            : base(fields, fieldWorkflowStatus)
        {
            mFieldWorkflowStatus = fieldWorkflowStatus;
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
