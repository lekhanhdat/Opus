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
using System.Threading.Tasks;

namespace AvePoint.RA.Contract.Global.Exceptions
{
    public class RMApiErrorCode
    {
        public static readonly Int32 Nonexistent = 10000;

        public static readonly Int32 NoToken = 10001;

        public static readonly Int32 InvalidToken = 10002;

        public static readonly Int32 ExpiredToken = 10003;

        public static readonly Int32 Linkexpired = 10004;

        public static readonly Int32 NoProductInstance = 10005;

        public static readonly Int32 NoStartedApplication = 10006;

        public static readonly Int32 AccountExisted = 10007;

        public static readonly Int32 PasswordExpired = 10008;

        public static readonly Int32 NotmatchedPassword = 10009;

        public static readonly Int32 NoDatabaseServer = 10010;
        public static readonly Int32 NoAvailableDatabaseServer = 10011;

        public static readonly Int32 PublicKeyRequestTypeNotAllowed = 10100;
        public static readonly Int32 PublicKeyCertificateError = 10101;

        public static readonly Int32 RefreshTokenError = 10102;

        public static readonly Int32 Forbidden = 403;
        public static readonly Int32 NotFound = 404;
        public static readonly Int32 Unauthorized = 401;
    }
}
