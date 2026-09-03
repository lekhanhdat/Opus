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
using AvePoint.Wrapper.Common.Office;
using AvePoint.Wrapper.Common;
using Microsoft.SharePoint.Portal.WebControls;
using AvePoint.Wrapper.Common.Common.Constants;
using System.Globalization;

namespace AvePoint.ObjectModel.Server13.Office
{
    class AveOStringResourceManager : IAveOStringResourceManager
    {
        private static readonly string mAveOStringResourceManager_Type = "Microsoft.Office.Server.Internal.Resources.StringResourceManager";
        private object mAveOStringResourceManager;

        public AveOStringResourceManager(object obj)
        {
            mAveOStringResourceManager = obj;
        }

        public AveOStringResourceManager()
            : this(AveAssemblyUtility.CreateInstance(mAveOStringResourceManager_Type))
        { }

        public string GetString(AveOLocStringId lsid)
        {
            return (string)AveAssemblyUtility.InvokeStaticMethod(mAveOStringResourceManager_Type, "GetString", new object[] { lsid });
        }
        public string GetStringWithCultureInfo(string resourceName, CultureInfo cultureInfo)
        {
            return (string)AveAssemblyUtility.InvokeStaticMethod(mAveOStringResourceManager_Type, "GetStringWithCultureInfo", new object[] { resourceName, cultureInfo });

        }

    }
}
