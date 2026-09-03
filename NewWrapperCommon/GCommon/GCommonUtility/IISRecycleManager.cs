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



namespace AvePoint.GCommon
{
    #region using directives
    using System;
    using System.Collections.Generic;
    using System.DirectoryServices;
    using System.Collections;
    using System.Reflection;
    #endregion

    /// <summary>
    /// 当我们要做iis reset的时候要首选下面这个类，这个类会recycle所有的application pool除了Control所使用的pool.
    /// </summary>
    public class IISRecycleManager
    {
        static AveLogger logger = new AveLogger(MethodBase.GetCurrentMethod().DeclaringType);

        const string ControlPhysicalPath = @"DocAve6\manager\control";

        /// <summary>
        /// recycle 除了control使用的其他所有application pool
        /// </summary>
        public static void RecycleAllAppPoolsExceptControl()
        {
            using (DirectoryEntry service = new DirectoryEntry("IIS://localhost/W3SVC"))
            {
                List<DirectoryEntry> servers = GetAllSitesExceptControl(service);
                servers.ForEach(server =>
                    {
                        String appId = GetAppPoolID(server);
                        RecycleAppPool(appId);
                    });
            }
        }

        static List<DirectoryEntry> GetAllSitesExceptControl(DirectoryEntry service)
        {
            List<DirectoryEntry> servers = new List<DirectoryEntry>();
            foreach (DirectoryEntry server in service.Children)
            {
                if ("iisWebServer".Equals(server.SchemaClassName, StringComparison.OrdinalIgnoreCase))
                {
                    foreach (DirectoryEntry child in server.Children)
                    {
                        if ("iisWebVirtualDir".Equals(child.Name, StringComparison.OrdinalIgnoreCase)
                            || "root".Equals(child.Name, StringComparison.OrdinalIgnoreCase))
                        {
                            if (!(child.Properties["path"][0].ToString().ToLowerInvariant().Trim().Contains(ControlPhysicalPath))
#if DEBUG
 && !(child.Properties["path"][0].ToString().ToLower().Trim().Contains(@"VCManager\control.web"))
#endif
)
                                servers.Add(child);
                        }
                    }
                }
            }
            return servers;
        }

        static void RecycleControl()
        {
            using (DirectoryEntry service = new DirectoryEntry("IIS://localhost/W3SVC"))
            {
                DirectoryEntry site = GetControlSite(service);
                String appId = GetAppPoolID(site);
                RecycleAppPool(appId);
            }
        }

        static DirectoryEntry GetControlSite(DirectoryEntry service)
        {
            foreach (DirectoryEntry server in service.Children)
            {
                if ("iisWebServer".Equals(server.SchemaClassName, StringComparison.OrdinalIgnoreCase))
                {
                    foreach (DirectoryEntry child in server.Children)
                    {
                        if ("iisWebVirtualDir".Equals(child.Name, StringComparison.OrdinalIgnoreCase)
                            || "root".Equals(child.Name, StringComparison.OrdinalIgnoreCase))
                        {
                            if (child.Properties["path"][0].ToString().ToLowerInvariant().Trim().Contains(ControlPhysicalPath)
#if DEBUG
 || child.Properties["path"][0].ToString().ToLower().Trim().Contains(@"VCManager\control.web")
#endif
)
                                return child;
                        }
                    }
                }
            }
            return null;
        }

        static String GetAppPoolID(DirectoryEntry site)
        {
            var appPool = site.Properties["AppPoolId"];
            return appPool.Value.ToString();
        }

        static void RecycleAppPool(String poolId)
        {
            try
            {
                const String method = "Recycle";
                using (DirectoryEntry appPools = new DirectoryEntry("IIS://localhost/W3SVC/AppPools"))
                {
                    DirectoryEntry findPool = appPools.Children.Find(poolId, "IisApplicationPool");
                    findPool.Invoke(method, null);
                    appPools.CommitChanges();
                }
            }
            catch (Exception ex)
            {
                logger.Warn("An error occurred while recycling application pool. {0} {1}", poolId, ex.ToString());
            }
        }
    }
}