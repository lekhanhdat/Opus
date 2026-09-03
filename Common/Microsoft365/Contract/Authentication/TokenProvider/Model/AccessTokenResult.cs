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
namespace Microsoft365.Authentication.TokenProvider
{
    using System;
    public class AccessTokenResult
    {
        public AccessTokenResult(string token, string error, DateTimeOffset expiresOn,TokenType tokenType)
        {
            AccessToken = token;
            Error = error;
            ExpiresOn = expiresOn;
            TokenType= tokenType;
        }

        public AccessTokenResult(Exception exception)
        {
            Error = exception.Message;
            Exception = exception;
        }

        public TokenType TokenType { get; private set; }
        public string AccessToken { get;private set; }
        public string Error { get; private set; }
        public Exception Exception { get; set; }
        public DateTimeOffset ExpiresOn { get; private set; }
    }
}