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

namespace AutoInstallationCommon.ActiveDirectory
{
    public class Logs
    {
        private static Action<string, object[]> debugProxy;
        private static Action<string, object[]> infoProxy;
        private static Action<string, object[]> warningProxy;
        private static Action<string, object[]> errorProxy;

        public static Logs CreateUniformLog()
        {
            return new Logs();
        }

        public static void Install(Action<string, object[]> debug,
            Action<string, object[]> info,
            Action<string, object[]> warning,
            Action<string, object[]> error)
        {
            debugProxy = debug;
            infoProxy = info;
            warningProxy = warning;
            errorProxy = error;
        }

        public static void Uninstall()
        {
            debugProxy = null;
            infoProxy = null;
            warningProxy = null;
            errorProxy = null;
        }

        public void Debug(string format, params object[] args)
        {
            if (debugProxy != null) debugProxy(format, args);
        }

        public void Info(string format, params object[] args)
        {
            if (infoProxy != null) infoProxy(format, args);
        }

        public void Warn(string format, params object[] args)
        {
            if (warningProxy != null) warningProxy(format, args);
        }

        public void Error(string format, params object[] args)
        {
            if (errorProxy != null) errorProxy(format, args);
        }
    }
}