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
namespace AvePoint.ObjectModel.Common
{
    class AveFieldUrlValue : AveClientObject, IAveFieldUrlValue
    {
        private string mValue = string.Empty;

        public AveFieldUrlValue()
        { }

        public AveFieldUrlValue(string fieldValue)
        {
            if (!string.IsNullOrEmpty(fieldValue))
            {
                int length = 0;
                while (length < fieldValue.Length)
                {
                    if (fieldValue[length] == ',')
                    {
                        if (length == (fieldValue.Length - 1))
                        {
                            fieldValue = fieldValue.Substring(0, fieldValue.Length - 1);
                            break;
                        }
                        if (((length + 1) < fieldValue.Length) && (fieldValue[length + 1] == ' '))
                        {
                            break;
                        }
                        length++;
                    }
                    length++;
                }
                if (length < fieldValue.Length)
                {
                    var url= fieldValue.Substring(0, length).Replace(",,", ",");
                    DataCache.AddProperty("Url", url);
                    length += 2;
                    if (length < fieldValue.Length)
                    {
                        base.DataCache.AddProperty("Description",fieldValue.Substring(length, fieldValue.Length - length));
                    }
                }
                else
                {
                    DataCache.AddProperty("Url", fieldValue.Replace(",,", ","));
                    DataCache.AddProperty("Description", fieldValue.Replace(",,", ","));
                }
            }

            mValue = fieldValue;
        }

        public string Url
        {
            get
            {
                return base.DataCache.GetProperty<string>("Url");
            }
            set
            {
                base.DataCache.AddChangedProperty("Url", value);
            }
        }

        public string Description
        {
            get
            {

                return base.DataCache.GetProperty<string>("Description");
            }
            set
            {
                base.DataCache.AddChangedProperty("Description", value);
            }
        }

        public override string ToString()
        {
            if (string.IsNullOrEmpty(this.Url) && string.IsNullOrEmpty(this.Description))
            {
                return string.Empty;
            }
            if (this.Url.EndsWith(",", StringComparison.OrdinalIgnoreCase))
            {
                this.Url = this.Url.Substring(0, this.Url.Length - 1);
            }
            return (this.Url.Replace(",", ",,") + ", " + this.Description);
        }
    }
}
