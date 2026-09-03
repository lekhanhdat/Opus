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

namespace Microsoft365.Initial
{
    using Microsoft365.Authentication.Configuration;
    using Microsoft365.Common.Logger;
    using Microsoft365.Configuration;
    using System;
    using System.Collections.Generic;

    public class DefaultConfiguration
    {
        private static TokenSetting TokenSetting = new TokenSetting()
        {
            CacheInstanceLifeCycleSecondTime = 5000,
            MaxCacheInstance = 1000
        };

        private static FormDigestSetting FormDigestSetting = new FormDigestSetting()
        {
            MaxCacheInstance = 1000
        };

        public static void Setup()
        {

            Microsoft365Configuration.AuthenticationConfiguration
                    .AddTokenSetting(TokenSetting);
            Microsoft365Configuration.CommonConfiguration.AddLoggerFactory(new LoggingFactory());
            Microsoft365Configuration.SharePointConfiguration.AddFormDigestCacheSetting(FormDigestSetting);
        }

        private class ConsoleLogger : IMicrosoft365Logger
        {
            protected Type ObjType { get; set; }
            public ConsoleLogger(Type t)
            {
                ObjType = t;
            }

            private void WriteMessage(string level, string message, params object[] param)
            {
                if (param != null && param.Length > 0)
                {
                    Console.WriteLine($"{level.PadLeft(10)} - {DateTime.UtcNow} - {string.Format(message, param)}");
                }
                else
                {
                    Console.WriteLine($"{level.PadLeft(10)} - {DateTime.UtcNow} - {message}");
                }
            }

            public void Debug(string message, params object[] param)
            {
                WriteMessage("Debug", message, param);
            }

            public void Error(string message, params object[] param)
            {
                WriteMessage("Error", message, param);
            }

            public void Info(string message, params object[] param)
            {
                WriteMessage("Info", message, param);
            }

            public void Trace(string message, params object[] param)
            {
                WriteMessage("Trace", message, param);
            }

            public void Warn(string message, params object[] param)
            {
                Console.WriteLine(string.Format(message, param));
            }
        }
        private class LoggingFactory : ILoggerFactory
        {
            public IMicrosoft365Logger GetLogger(Type t)
            {
                return new ConsoleLogger(t);
            }
        }
    }
}