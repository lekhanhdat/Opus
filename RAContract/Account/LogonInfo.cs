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
namespace AvePoint.RA.Contract.Logon
{

    public class RMLogonInfo
    {
        public string state { get; private set; }
        public string token { get; private set; }
        public string correlation_id { get; private set; }
        public string access_token { get; private set; }
        public string refresh_token { get; private set; }

        public RMLogonInfo(string state, string token, string correlation_id,
                   string access_token, string refresh_token) 
        {
            this.state = state;
            this.token = token;
            this.correlation_id = correlation_id;
            this.access_token = access_token; 
            this.refresh_token = refresh_token;
        }
    }
}
