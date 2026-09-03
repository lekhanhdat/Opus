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

namespace AvePoint.Wrapper.Common
{
    public enum AveSPServerVersionType
    {
        None,
        SP2007,
        SP2010,
        SP2013,
        SP2016,
        SP2019,
        SPSE,
        Office365
    }

    [Flags]
    public enum AveAuthenticationMode
    {
        None,
        Windows = 1,
        Claims = 2,
        Forms = 4,
        ADFS = 8,
        OnlineServiceAccount = 16,
        OnlineAppToken = 32,
        OnlineGraphToken = 64,
        Online = 112 // service account 和 app token, graph 集合，便于判断online 站点
    }

    public class AveServerVersion
    {

        public AveSPServerVersionType VersionType { get; set; }
        
        public string Version { get; set; }

        //NoUse remove this property
        ///// <summary>
        ///// 2 -> 12.x.x.x 4 ->14.x.x.x 8->15.x.x.x 16->16.x.x.x
        ///// </summary>
        //public string BitVersion { get; set; }

        public string SiteUrl { get; set; }

    }
}
