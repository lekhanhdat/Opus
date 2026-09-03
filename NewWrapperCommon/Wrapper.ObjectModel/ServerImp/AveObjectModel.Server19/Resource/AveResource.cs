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



using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using AvePoint.Wrapper.Common;
using Microsoft.SharePoint;

namespace AvePoint.ObjectModel.Server19
{
    class AveResource : IAveResource
    {
        private SPResource mResource;

        public AveResource()
        { }

        public AveResource(SPResource resource)
        {
            mResource = resource;
        }

        [SuppressMessage("Microsoft.Globalization", "CA1304:SpecifyCultureInfo", Justification = "Copy Code From SharePoint Dll")]
        public string GetString(string name, params object[] values)
        {
            return SPResource.GetString(name, values);
        }

        public string GetString(CultureInfo culture, string name, params object[] values)
        {
            return SPResource.GetString(culture, name, values);
        }
    }
}
