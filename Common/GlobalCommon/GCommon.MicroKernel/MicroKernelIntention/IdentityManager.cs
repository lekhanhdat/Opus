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



namespace AvePoint.GCommon.MicroKernel
{
    #region using directives
    using System;
    using System.Runtime.Remoting.Messaging;
    #endregion
    public class IdentityManager
    {
        [ThreadStatic]
        static String identityTypeThreadMode;
        [ThreadStatic]
        static String identityContentThreadMode;

        static String identityTypeProcessMode;

        static String identityContentProcessMode;

        public static String IdentityType
        {
            get
            {
                var result = default(String);
                switch (IdentityMode)
                {
                    case IdentityMode.Thread:
                        result = identityTypeThreadMode;
                        break;
                    case IdentityMode.Process:
                        result = identityTypeProcessMode;
                        break;
                    case IdentityMode.LogicalCallContext:
                        result = CallContext.LogicalGetData("IdentityType").ToString();
                        break;
                }
                return result;
            }
            set
            {
                switch (IdentityMode)
                {
                    case IdentityMode.Thread:
                        identityTypeThreadMode = value;
                        break;
                    case IdentityMode.Process:
                        identityTypeProcessMode = value;
                        break;
                    case IdentityMode.LogicalCallContext:
                        CallContext.LogicalSetData("IdentityType", value);
                        break;
                }
            }
        }

        public static String IdentityContent
        {
            get
            {
                var result = default(String);
                switch (IdentityMode)
                {
                    case IdentityMode.Thread:
                        result = identityContentThreadMode;
                        break;
                    case IdentityMode.Process:
                        result = identityContentProcessMode;
                        break;
                    case IdentityMode.LogicalCallContext:
                        result = CallContext.LogicalGetData("IdentityContent") as string;
                        break;
                }
                return result;
            }
            set
            {
                switch (IdentityMode)
                {
                    case IdentityMode.Thread:
                        identityContentThreadMode = value;
                        break;
                    case IdentityMode.Process:
                        identityContentProcessMode = value;
                        break;
                    case IdentityMode.LogicalCallContext:
                        CallContext.LogicalSetData("IdentityContent", value);
                        break;
                }
            }
        }

        public static IdentityMode IdentityMode { get; set; }
    }
}
