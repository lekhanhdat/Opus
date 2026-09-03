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
using System.Diagnostics;
using System.Linq;
using System.Text;
using AvePoint.GCommon.Contract.AveLicense;
using AvePoint.GCommon.Utility;

namespace AvePoint.GCommon
{
    public class InstallationUtility
    {
        /// <summary>
        /// 获取DocAve Manager 安装路径
        /// </summary>
        /// <returns></returns>
        public static string GetControlInstallPath()
        {
            string path = null;
            try
            {
                string key = @"SOFTWARE\AvePoint\DocAve6";
                try
                {
                    if (LicenseWrapper.ProductType == ProductType.NetApp)
                    {
                        key = @"SOFTWARE\Network Appliance\SnapManager for SharePoint 8";
                    }
                    if (LicenseWrapper.ProductType == ProductType.NetApp_IBM)
                    {
                        key = @"SOFTWARE\IBM\SnapManager for SharePoint 8\Manager";
                    }
                }
                catch (Exception e)
                {
                    Trace.TraceWarning(e.ToString());
                }
                path = RegistryManager.ReadLocalMachine(key, "InstallPath");
                if (path == null || path.Length == 0)
                {
                    throw new Exception("Can not find installation path in register.");
                }
                //SAAS-2220, 有些路径结尾包含了\
                if (path.EndsWith(@"\", StringComparison.OrdinalIgnoreCase))
                {
                    path = path + @"Control\";
                }
                else
                {
                    path = path + @"\Control\";
                }
            }
            catch (Exception e)
            {
                Trace.TraceWarning(e.ToString());
            }

            if (path == null || path.Length == 0)
            {

                path = System.AppDomain.CurrentDomain.BaseDirectory;
                if (path.EndsWith(@"bin\", StringComparison.OrdinalIgnoreCase))
                {
                    //if path contains bin, remove it.
                    path = path.Substring(0, path.Length - 4);
                }
            }

            return path;
        }
    }
}
