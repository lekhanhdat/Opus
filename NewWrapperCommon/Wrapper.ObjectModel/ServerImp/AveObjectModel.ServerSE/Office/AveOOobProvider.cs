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
using Microsoft.Office.DocumentManagement.Internal;
using AvePoint.Wrapper.Common;
using Microsoft.SharePoint;
using System.Diagnostics.CodeAnalysis;

namespace AvePoint.ObjectModel.ServerSE.Office
{
    class AveOOobProvider : IAveOOobProvider
    {
        private OobProvider mOobProvider;
        //private readonly string MOobProvider_Type = "Microsoft.Office.DocumentManagement.Internal.OobProvider";

        public AveOOobProvider()
        {
            mOobProvider = new OobProvider();
        }

        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "SharePoint Property.")]
        public string GetSitePrefix(IAveSite site)
        {
            return site.RootWeb.Properties["docid_msft_hier_siteprefix"];
        }

        public void SetSitePrefix(IAveSite site, string prefix)
        {
            OobProvider.SetSitePrefix((site as AveSite).Site, prefix);
        }
    }
}
