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
using System.Reflection;
using AvePoint.GCommon;

namespace AvePoint.Wrapper.Common
{
    public class AveDatabaseStatusChecker
    {
        private static AveLogger log = AveLogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);

        public static bool CheckWebAppDatabaseStatus(string webAppUrl, AveObjectModelFactory omFactory)
        {
            bool online = false;
            try
            {
                IAveWebApplication webApp = omFactory.CreateWebApplication().Lookup(new Uri(webAppUrl));
                if (webApp == null)
                {
                    log.Error("WebApplication can not be found.WebApp Url:{0}.", webAppUrl);
                }
                else
                {
                    online = CheckWebAppDatabaseStatus(webApp);
                }
            }
            catch (Exception ex)
            {
                log.Error("WebApplication can not be found.WebAppUrl:{0}.Reason:{1}.", webAppUrl, ex.ToString());
            }
            return online;
        }

        public static bool CheckWebAppDatabaseStatus(IAveWebApplication webApp)
        {
            bool online = false;
            try
            {
                if (webApp.ContentDatabases.Count > 0)
                {
                    foreach (IAveContentDatabase contentDB in webApp.ContentDatabases)
                    {
                        if (contentDB == null)
                        {
                            log.Warn("These is an empty content database when CheckWebAppDatabaseStatus.");
                        }
                        else
                        {
                            online = (contentDB.Status == AveObjectStatus.Online)/* && CheckSQLDatabaseAccessable(contentDB)*/;
                            if (online)
                            {
                                break;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error("Error happened when check WebApp All ContentDatabase Status.Reason:{0}.", ex.ToString());
            }
            return online;
        }

        //public static bool CheckSiteDatabaseStatus(string siteUrl, bool onlyCheckSQL, AveObjectModelFactory omFactory)
        //{
        //    bool online = false;
        //    try
        //    {
        //        using (IAveSite site = omFactory.CreateSite(siteUrl))
        //        {
        //            IAveContentDatabase contentDB = site.ContentDatabase;
        //            online = CheckSiteDatabaseStatus(contentDB, onlyCheckSQL);
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        log.Error("SiteCollection can not be found.SiteUrl:{0}.Reason:{1}.", siteUrl, ex.ToString());
        //    }
        //    return online;
        //}

        //public static bool CheckSiteDatabaseStatus(IAveContentDatabase contentDB, bool onlyCheckSQL)
        //{
        //    bool online = false;
        //    try
        //    {
        //        if (onlyCheckSQL)
        //        {
        //            online = CheckSQLDatabaseAccessable(contentDB);
        //        }
        //        else
        //        {
        //            online = contentDB.Status == AveObjectStatus.Online && CheckSQLDatabaseAccessable(contentDB);
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        log.Error("Error happened when check SiteCollection ContentDatabase Status.Reason:{0}.", ex.ToString());
        //    }
        //    return online;
        //}

        //private static bool CheckSQLDatabaseAccessable(IAveDatabase dataBase)
        //{
        //    bool accessalbe = true;
        //    try
        //    {
        //        object sqlSession = AveAssemblyUtility.GetPropertyValue(dataBase, "SqlSession");
        //        AveAssemblyUtility.InvokeMethod(sqlSession, sqlSession.GetType(), "TestConnection", new object[] { });
        //    }
        //    catch (Exception ex)
        //    {
        //        log.Warn("Error happened when check ContentDB is accessible or not.Reason:{0}", ex.ToString());
        //    }
        //    return accessalbe;
        //}
    }
}