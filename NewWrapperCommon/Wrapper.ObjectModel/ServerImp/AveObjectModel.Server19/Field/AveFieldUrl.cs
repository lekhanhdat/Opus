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

namespace AvePoint.ObjectModel.Server19
{
    class AveFieldUrl : AveField, IAveFieldUrl
    {
        private SPFieldUrl mFieldUrl;

        public AveFieldUrl(SPFieldUrl fieldUrl)
            : base(fieldUrl)
        {
            mFieldUrl = fieldUrl;
        }

        public AveFieldUrl(AveFieldCollection fieldColl, SPFieldUrl field)
            : base(fieldColl, field)
        {
            mFieldUrl = field;
        }

        public override Type FieldValueType
        {
            get
            {
                return typeof(IAveFieldUrlValue);
            }
        }

        public override object GetFieldValue(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return null;
            }
            return new AveFieldUrlValue(new SPFieldUrlValue(value));
        }

        public override string GetFieldValueAsText(object value)
        {
            if (value != null)
            {
                SPFieldUrlValue validatedUrlValue = this.GetValidatedUrlValue(value);
                if (validatedUrlValue != null)
                {
                    return (validatedUrlValue.Url + ", " + validatedUrlValue.Description);
                }
            }
            return string.Empty;
        }

        /// <summary>
        /// SharePoint API For GetFieldValueAsTest()
        /// </summary>
        /// <param name="value">this value should be string or AveFieldUrlValue</param>
        /// <returns></returns>
        private SPFieldUrlValue GetValidatedUrlValue(object value)
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveFieldUrl.GetValidatedUrlValue"))
            {

                SPFieldUrlValue value2;
                if (value is string)
                {
                    value2 = new SPFieldUrlValue((string)value);
                }
                else
                {
                    if (!(value is AveFieldUrlValue))
                    {
                        throw new ArgumentNullException();
                    }
                    value2 = (value as AveFieldUrlValue).FieldUrlValue;
                }
                string url = value2.Url;
                if (string.IsNullOrEmpty(url))
                {
                    return null;
                }
                if (url.StartsWith(@"\\", StringComparison.OrdinalIgnoreCase) || url.StartsWith("//", StringComparison.OrdinalIgnoreCase))
                {
                    url = "file:" + url;
                }
                if (url.StartsWith("file:", StringComparison.OrdinalIgnoreCase))
                {
                    url = url.Replace(@"\", "/");
                }
                value2.Url = url;
                if (!SPUrlUtility.IsProtocolAllowed(url, false) && !url.StartsWith("/", StringComparison.OrdinalIgnoreCase))
                {
                    throw new SPFieldValidationException(SPResource.GetString(CultureInfo.CurrentUICulture, "InvalidUrl", new object[] { url }));
                }
                return value2;


            }

        }

        #region IAveFieldUrl Members

        public AveUrlFieldFormatType DisplayFormat
        {
            get
            {
                return (AveUrlFieldFormatType)mFieldUrl.DisplayFormat;
            }
            set
            {
                mFieldUrl.DisplayFormat = (SPUrlFieldFormatType)value;
            }
        }

        #endregion
    }
}
