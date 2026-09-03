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

using Microsoft.Identity.Client;
using Microsoft365.Common.Logger;

namespace Microsoft365.Authentication.Extension
{
    internal static class MsalExtension
    {
        private static IMicrosoft365Logger logger = Microsoft365LoggerManager.CreateLogger(typeof(MsalExtension));
        internal static T WithDefaultLogging<T>(this AbstractApplicationBuilder<T> builder) where T : AbstractApplicationBuilder<T>
        {
            return builder.WithLogging((level, message, containsPii) =>
            {
                switch (level)
                {
                    case LogLevel.Error:
                        logger.Error(message);
                        break;
                    case LogLevel.Warning:
                        logger.Warn(message);
                        break;
                    case LogLevel.Info:
                        //logger.Info(message);
                        break;
                }
            }, LogLevel.Info, true, true);
        }
    }
}