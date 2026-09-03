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
using System.Globalization;
using AvePoint.Wrapper.Common;
using Microsoft.SharePoint;
using Microsoft.SharePoint.Utilities;

namespace AvePoint.ObjectModel.Server16
{
    class AveFieldBoolean : AveField, IAveFieldBoolean
    {
        private SPFieldBoolean mFieldBoolead;

        public AveFieldBoolean(SPFieldBoolean field)
            : base(field)
        {
            mFieldBoolead = field;
        }

        public AveFieldBoolean(AveFieldCollection fieldCollection, SPFieldBoolean field)
            : base(fieldCollection, field)
        {
            mFieldBoolead = field;
        }

        #region IAveFieldBoolean Members

        public string JumpToNoField
        {
            get
            {
                return mFieldBoolead.JumpToNoField;
            }
            set
            {
                mFieldBoolead.JumpToNoField = value;
            }
        }

        public string JumpToYesField
        {
            get
            {
                return mFieldBoolead.JumpToYesField;
            }
            set
            {
                mFieldBoolead.JumpToYesField = value;
            }
        }

        public override string GetFieldValueAsText(object value)
        {
            if (value == null)
            {
                return string.Empty;
            }
            bool data = false;
            if (value is bool)
            {
                data = (bool)value;
            }
            else
            {
                data = (bool)GetFieldValue(value as string);
            }
            return GetFieldValueAsText(data);
        }

        public override object GetFieldValue(string value)
        {
            int LCID = base.Fields.Web.UICulture.LCID;
            if (string.IsNullOrEmpty(value) || ((!string.Equals(value, "TRUE", StringComparison.OrdinalIgnoreCase) && !(SPUtility.GetLocalizedString(value, "core", (uint)LCID) == "TRUE")) && (!(value == "-1") && !(value == "1"))))
            {
                return false;
            }
            return true;
        }

        public static string GetFieldValueAsText(bool data)
        {
            if (!data)
            {
                return SPResource.GetString(CultureInfo.CurrentUICulture, "YesNoFieldNo", new object[0]);
            }
            return SPResource.GetString(CultureInfo.CurrentUICulture, "YesNoFieldYes", new object[0]);
        }

        public override Type FieldValueType
        {
            get
            {
                return typeof(bool);
            }
        }

        #endregion
    }
}
