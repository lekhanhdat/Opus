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
using System.Web;

namespace AvePoint.RA.Api.Web.Public.Authorize
{
    public static class RequestHeadersParam
    {
        public static readonly string USE_INTERNAL_IDS = "Is-Internal-Identity-Server";
        public static readonly string PRODUCT = "Product";
        public static readonly string TOKEN_SOURCE = "Token-Source";
        public static readonly string CLOUD_SDK = "CloudSDK-RequestId";
        public static readonly string AGENT_JOB_ID = "Agent-Job-Id";
        public static readonly string HYBRID_AGENT_ID = "Hybrid-Agent-Id";
        public static readonly string AOS_VNEXT = "X-OPUS-AOS_VNEXT";
        public static readonly string CALLER = "Caller";
    }
    public static class ProductName
    {
        public static readonly string RECORDAGENT = "RecordsAgent";
        public static readonly string RECORDS = "AvePointRecords";
        public static readonly string COP = "CloudOperationsPortal";
        public static readonly string OC = "OfficeConnect";
        public static readonly string Myhub = "Myhub";
    }

    public enum ProductType
    {
        None = 0,
        Records = 1,
        RecordsAgent = 2,
        COP = 3,
        OC = 4,
        Myhub = 5,
        RecordsSpfx = 6,
        AOSVNext = 7
    }

    public enum TokenSource
    {
        None,
        SpfxOAuth,
    }
}
