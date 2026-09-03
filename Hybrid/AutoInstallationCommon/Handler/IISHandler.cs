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

namespace AutoInstallationCommon.Utility.Handler
{
    public class IISHandler
    {
        private static readonly IiiSUtil iisUtil = IISVersionHandler.FindIISUtil();

        public static void DeleteWebSite(string name)
        {
            try
            {
                iisUtil.DeleteWebSite(name);
            }
            catch (Exception ex)
            {
                //log
            }
        }

        public static void DeleteAppPool(string name)
        {
            try
            {
                iisUtil.DeleteApplicationPool(name);
            }
            catch (Exception ex)
            {
                //log
            }
        }

        public static void StopAppPool(string pool)
        {
            try
            {
                iisUtil.StopApplicationPool(pool);
            }
            catch (Exception ex)
            {
                //log
            }
        }

        public static void StopWebSite(string websiteName)
        {
            try
            {
                iisUtil.StopWebSite(websiteName);
            }
            catch (Exception ex)
            {
            }
        }

        public static void StartWebSite(string websiteName)
        {
            try
            {
                iisUtil.StartWebSite(websiteName);
            }
            catch (Exception ex)
            {
            }
        }

        public static void StatrAppPool(string pool)
        {
            try
            {
                iisUtil.StartApplicationPool(pool);
            }
            catch (Exception ex)
            {
            }
        }
    }
}