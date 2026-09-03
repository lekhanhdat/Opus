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
using AvePoint.Common;
using System;
using System.Globalization;
using System.ServiceModel;

namespace AvePoint.GCommon.Utility.SslStreamSecurity
{
    internal sealed class XSR
    {
        private static Type srType;
        private static Func<string, string> getString;
        private static Func<string, object> getObject;

        static XSR()
        {
            srType = typeof(ChannelFactory).Assembly.GetType("System.ServiceModel.SR");
        }

        public static string GetString(string name, params object[] args)
        {
            string @string = GetString(name);
            if (args != null && args.Length > 0)
            {
                for (int i = 0; i < args.Length; i++)
                {
                    string text = args[i] as string;
                    if (text != null && text.Length > 1024)
                    {
                        args[i] = text.Substring(0, 1021) + "...";
                    }
                }
                return string.Format(CultureInfo.CurrentCulture, @string, args);
            }
            return @string;
        }

        public static string GetString(string name)
        {
            if (getString == null)
            {
                var mi = Invoker.GetMethod(srType, "GetString", new Type[] { typeof(string) });
                getString = (Func<string, string>)Delegate.CreateDelegate(typeof(Func<string, string>), mi);
            }
            return getString(name);
        }

        public static object GetObject(string name)
        {
            if (getObject == null)
            {
                var mi = Invoker.GetMethod(srType, "GetObject", new Type[] { typeof(string) });
                getObject = (Func<string, object>)Delegate.CreateDelegate(typeof(Func<string, object>), mi);
            }
            return getObject(name);
        }
    }
}