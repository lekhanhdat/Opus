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
using System.Xml.Linq;

namespace AvePoint.Item.Common
{
    public class AveSiteAttributeInfo
    {
        public AveSiteAttributeInfo() { }
        public AveSiteAttributeInfo(string info)
        {
            var siteAttribute = XElement.Parse(info, LoadOptions.PreserveWhitespace);
            if (siteAttribute != null)
            {
                WebAppUrl = siteAttribute.Attribute("VirtualServer").Value;
                WriteState = siteAttribute.Attribute("writeState").Value.ToBoolean();
                ReadState = siteAttribute.Attribute("readState").Value.ToBoolean();
                HostHead = siteAttribute.Attribute("hostHead").Value.ToBoolean();
                LockSuccess = siteAttribute.Attribute("needUnlock").Value.ToBoolean();
            }
        }
        public string OwnerLogin { get; set; }
        public string WebAppUrl { get; set; }
        public bool WriteState { get; set; }
        public bool HostHead { get; set; }
        public bool LockSuccess { get; set; }
        public bool ReadState { get; set; }
        public override string ToString()
        {
            var siteAttribute = new XElement("SiteAttribute");
            siteAttribute.SetAttributeValue("writeState", WriteState.ToString().ToLower());
            siteAttribute.SetAttributeValue("readState", ReadState.ToString().ToLower());
            siteAttribute.SetAttributeValue("needUnlock", LockSuccess);
            siteAttribute.SetAttributeValue("hostHead", HostHead);
            siteAttribute.SetAttributeValue("VirtualServer", WebAppUrl);
            return siteAttribute.ToString();
            //return "<SiteAttribute writeState=\"" + WriteState.ToString().ToLower() + "\" readState=\"" + ReadState.ToString().ToLower() + "\" needUnlock=\"" + this.LockSuccess + "\" hostHead=\"" + HostHead.ToString().ToLower() + "\" VirtualServer=\"" + WebAppUrl + "\"/>";
        }
    }

    static class stringExtension
    {
        public static bool ToBoolean(this string value, bool defaultValue)
        {
            bool result = defaultValue;
            bool.TryParse(value, out result);
            return result;
        }
        public static bool ToBoolean(this string value)
        {
            return ToBoolean(value, false);
        }
    }
}
