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

namespace AvePoint.Media.Storage
{
    using System;
    public class SystemPropertyKeys
    {
        public static readonly String TotalReadBytes = "TotalReadBytes";
        public static readonly String TotalReadTicks = "TotalReadTicks";
        public static readonly String TotalWriteBytes = "TotalWriteBytes";
        public static readonly String TotalWriteTicks = "TotalWriteTicks";
        public static readonly String SystemDescriptionKey = "SystemDescription";
        public static readonly String DATA_TRANSFER_IN = "Data Transfer In";
        public static readonly String DATA_TRANSFER_OUT = "Data Transfer Out";
        public static readonly String REQUEST_PUT = "Request Put";
        public static readonly String REQUEST_COPY = "Request Copy";
        public static readonly String REQUEST_POST = "Request Post";
        public static readonly String REQUEST_LIST = "Request List";
        public static readonly String REQUEST_GET = "Request Get";
        public static readonly String REQUEST_DELETE = "Request Delete";
        public static readonly String REQUEST_HEAD = "Request Head";
    }
}
