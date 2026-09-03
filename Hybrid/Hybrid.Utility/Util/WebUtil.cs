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
using System.Configuration;
using System.IO;
using System.Net;

namespace  AvePoint.Hybrid.Utility.Util
{
    public class WebUtil
    {
        /// <summary> Get web install path. </summary>
        /// <returns></returns>
        public static string GetInstallPath()
        {
            string path = AppDomain.CurrentDomain.BaseDirectory;
            if (path.EndsWith(@"bin" + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase))
            {
                path = path.Substring(0, path.Length - 4);
            }
            if (path.EndsWith(Path.DirectorySeparatorChar.ToString()))
            {
                path = path.TrimEnd(Path.DirectorySeparatorChar);
            }
            return path;
        }

        public static System.Configuration.Configuration GetWebConfiguration()
        {
            ExeConfigurationFileMap map = new ExeConfigurationFileMap();
            map.ExeConfigFilename = Path.Combine(GetInstallPath(), "Web.config");
            return ConfigurationManager.OpenMappedExeConfiguration(map, ConfigurationUserLevel.None);
        }

        public static void InitServicePoint()
        {
            // Set the maximum number of concurrent connections
            ServicePointManager.DefaultConnectionLimit = 12;
            ServicePointManager.SecurityProtocol |= SecurityProtocolType.Tls12;
        }
    }
}
