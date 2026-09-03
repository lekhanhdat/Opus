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
using AvePoint.RA.SharePoint.SPObjDiscover.DiscoverImpl;
using AvePoint.Wrapper.Discovery;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.SharePoint.SPObjDiscover
{
    public class RMSPDiscoverFactory
    {
        public static ISPDiscover CreateFactory(RMSPDiscoverHelper discoverHelper, SPDiscoverType discoverType) 
        {
            ISPDiscover sPDiscover = null;
            switch (discoverType)
            {
                case SPDiscoverType.Full:
                case SPDiscoverType.CAMLSearch:
                    sPDiscover = new RMSPFullDiscover(discoverHelper);
                    break;
                case SPDiscoverType.Incremental:
                    sPDiscover = new RMSPIncrementalDiscover(discoverHelper);
                    break;
                default:
                    break;
            }
            return sPDiscover;
        }
    }

    public enum SPDiscoverType 
    {
        Full = 1,
        Incremental,
        CAMLSearch
    }
}
