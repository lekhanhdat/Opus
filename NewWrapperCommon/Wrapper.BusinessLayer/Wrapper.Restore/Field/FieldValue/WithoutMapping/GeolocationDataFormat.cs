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
using AvePoint.Wrapper.Common;

namespace AvePoint.Wrapper.Restore
{
    class GeolocationDataFormat : BaseDataFormat
    {
        public GeolocationDataFormat(AveXmlField xmlField, IAveField destField, AveSPItem mItem) :
            base(xmlField, destField, mItem)
        {
        }

        public override object CheckFieldValue(object value)
        {
            string hexString = string.Empty;
            if (value is byte[])
            {
                byte[] tempBytes = value as byte[];
                hexString = mItem.ParentSite.ObjectModelFactory.Utility.HexStringFromBytes(tempBytes);
            }
            else
            {
                hexString = mItem.ParentSite.ObjectModelFactory.Utility.HexStringFromBytes(Convert.FromBase64String(value.ToString()));
            }
            return (destField as IAveFieldGeolocation).ConvertHexToWellKnownText(hexString);
        }
    }
}
