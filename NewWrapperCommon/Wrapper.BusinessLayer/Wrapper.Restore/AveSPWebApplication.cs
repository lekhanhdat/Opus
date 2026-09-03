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
using System.Linq;
using System.Text;
using System.Reflection;
using AvePoint.GCommon;
using AvePoint.Wrapper.Common;
using System.Collections;
using System.Diagnostics.CodeAnalysis;
using AvePoint.Wrapper.Resource.ServerAPI2010;
using AvePoint.Wrapper.Resource.Restore;

namespace AvePoint.Wrapper.Restore
{
    public class AveSPWebApplication
    {
        private static AveLogger log = AveLogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);

        [Obsolete("this will be removed in one day since it is not thread safe")]
        public static string DestinationURL = string.Empty;
        private IAveWebApplication mWebApplication = null;

        public static IAveWebApplication FindWebApplication(string url, string webAppUrl, bool useWebAppUrl, AveObjectModelFactory aOMFactory)
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveSPWebApplication.FindWebApplication"))
            {

                if (!url.EndsWith("/", StringComparison.OrdinalIgnoreCase))
                    url += "/";

                IAveWebApplication app = null;
                try
                {
                    if (useWebAppUrl)
                    {
                        app = aOMFactory.CreateWebApplication(webAppUrl);
                        if (app == null)
                        {
                            throw new AveWrapperBaseException(AveInternalResourceKey.Wrapper_Exception_Restore_NotFindWebApplication,webAppUrl);
                        }
                    }
                    else
                    {
                        app = aOMFactory.CreateWebApplication(url);
                        if (app == null)
                        {
                            app = aOMFactory.CreateWebApplication(webAppUrl);
                            if (app == null)
                            {
                                throw new AveWrapperBaseException(AveInternalResourceKey.Wrapper_Exception_Restore_NotFindWebApplication, webAppUrl);
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    log.Log(AveLogLevel.DEBUG, WrapperRestoreResource.CreateWebAppByUrlError, e.ToString());

                    if (useWebAppUrl)
                    {
                        app = aOMFactory.CreateWebApplication(webAppUrl);
                        if (app == null)
                        {
                            throw new AveWrapperBaseException(AveInternalResourceKey.Wrapper_Exception_Restore_NotFindWebApplication, webAppUrl);
                        }
                    }
                }
                //URL = app.AlternateUrls.GetResponseUrl(SPUrlZone.Default).Uri.ToString();
                return app;

            }

        }

        public Dictionary<AveUrlZone, IAveIisSettings> IisSettings
        {
            get
            {
                return mWebApplication.IisSettings;
            }
        }

        /// <summary>
        /// To check if SPDatabase in webapplication is ok. 
        /// </summary>
        /// <param name="url">url of webapplication</param>
        /// <param name="dbId">id collection of SPDatabase, null/Guid.Empty means all SPDatabases in webapplication</param>
        /// <returns>true: SPDatabase is ok; false: no available SPDatabase</returns>
        [SuppressMessage("FxCopCustomRules", "C100001:DoNotNameSensitiveWords")]
        public static bool CheckDatabaseStatus(string url, Guid[] dbId, AveObjectModelFactory aOMFactory)
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveSPWebApplication.CheckDatabaseStatus"))
            {

                if (!string.IsNullOrEmpty(url))
                {
                    try
                    {
                        IAveWebApplication webApp = aOMFactory.CreateWebApplication().Lookup(new Uri(url));
                        ICollection spDBCol = null;
                        if (dbId == null || dbId.Length == 0 || (dbId.Length == 1 && dbId[0].Equals(Guid.Empty)))
                        {
                            spDBCol = webApp.ContentDatabases;
                        }
                        else
                        {
                            List<IAveContentDatabase> temp = new List<IAveContentDatabase>();
                            foreach (Guid id in dbId)
                            {
                                if (!Guid.Empty.Equals(id))
                                {
                                    temp.Add(webApp.ContentDatabases[id]);
                                }
                            }
                            spDBCol = temp;
                        }
                        bool online = false;
                        foreach (IAveContentDatabase cdb in spDBCol)
                        {
                            if (cdb == null)
                            {
                                log.Warn("These is an empty content database when CheckDatabaseStatus.");
                            }
                            else
                            {
                                online = online | cdb.Exists;
                                if (online)
                                {
                                    break;
                                }
                            }
                        }
                        return online;
                    }
                    catch (Exception ex)
                    {
                        log.Error("There is an error when check webApplication {0},Exception {1}", ex.ToString());
                    }
                }
                return false;


            }

        }
    }
}
