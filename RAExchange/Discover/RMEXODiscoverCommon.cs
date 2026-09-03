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
using AvePoint.RA.Contract.Object;
using AvePoint.RA.RAExchange.Discover.DiscoverImpl;
using Microsoft.Exchange.WebServices.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AvePoint.RA.RAExchange.Discover.DiscoverImplV2;

namespace AvePoint.RA.RAExchange.Discover
{
    public class EXODiscoverFactory
    {
        public static IBatchDiscover CreateFactory(RMEXODiscoverHelper helper, EXODiscoverType discoverType, NodeFlagType jobType, Guid groupId, SearchFilter searchFilter = null)
        {
            IBatchDiscover discover = null;
            switch (discoverType)
            {
                case EXODiscoverType.Full:
                    discover = new FullDiscover(helper);
                    break;
                case EXODiscoverType.Incremental:
                    discover = new IncrementalDiscover(helper, jobType, groupId);
                    break;
                case EXODiscoverType.Search:
                    discover = new SearchDiscover(helper, searchFilter);
                    break;
                default:
                    throw new Exception("Unknow discover type.");
            }
            return discover;
        }
    }
    
    public class EXODiscoverFactoryV2
    {
        public static IBatchDiscoverV2 CreateFactory(EXODiscoverType discoverType, NodeFlagType jobType, Guid groupId, SearchFilter searchFilter = null)
        {
            IBatchDiscoverV2 discover = null;
            switch (discoverType)
            {
                case EXODiscoverType.Full:
                    discover = new FullDiscoverV2();
                    break;
                case EXODiscoverType.Incremental:
                    //discover = new IncrementalDiscover(helper, jobType, groupId);
                    break;
                case EXODiscoverType.Search:
                    discover = new SearchDiscoverV2(searchFilter);
                    break;
                default:
                    throw new Exception("Unknow discover type.");
            }
            return discover;
        }
    }

    public enum EXODiscoverType
    {
        Full = 0,
        Incremental = 1,
        Search = 2
    }
}
