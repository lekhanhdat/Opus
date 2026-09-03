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

namespace AvePoint.ObjectModel.ServerSE.NonPublicAPI
{
    using Microsoft.SharePoint;
    using Microsoft.SharePoint.Administration;
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using AvePoint.Wrapper.Core.Util;
    using Wrapper.Common;

    [NonPublicAPI("Microsoft.SharePoint.Administration.SPSiteMasterManager")]
    static class SPSiteMasterManagerExtension
    {
        private static readonly Type TypeOfSPSiteMasterManager = typeof(SPSite).Assembly.GetType("Microsoft.SharePoint.Administration.SPSiteMasterManager");

        #region SPSiteMaster(SPContentDatabase)
        private static Func<SPContentDatabase, object> getSPSiteMasterManagerInstanceDelegate;
        private static Func<SPContentDatabase, object> GetDelegate_GetSPSiteMasterManagerInstance()
        {
            if (getSPSiteMasterManagerInstanceDelegate == null)
            {
                getSPSiteMasterManagerInstanceDelegate = WrapperInvoker.CreateInstance<SPContentDatabase, object>(
                    TypeOfSPSiteMasterManager.GetConstructor(BindingFlags.Instance | BindingFlags.Public, null, new Type[] { typeof(SPContentDatabase) }, null));
            }
            return getSPSiteMasterManagerInstanceDelegate;
        }   
        public static object GetSPSiteMasterManagerInstance(SPContentDatabase cdb)
        {
            return GetDelegate_GetSPSiteMasterManagerInstance()(cdb);
        }
        #endregion

        #region GetSiteMasters
        private static Func<object, IList<object>> GetDelegate_GetSiteMasters()
        {
            return TypeOfSPSiteMasterManager.GetMethod<Func<object, IList<object>>>(nameof(GetSiteMasters), BindingFlags.Instance | BindingFlags.NonPublic);
        }
        public static IList<object> GetSiteMasters(object mngner)
        {
            return GetDelegate_GetSiteMasters()(mngner);
        }

        #endregion

        #region EnsureSiteMaster
        private static Func<object, string, uint, int, object> GetDelegate_EnsureSiteMaster()
        {
            return TypeOfSPSiteMasterManager.GetMethod<Func<object, string, uint, int, object>>(nameof(EnsureSiteMaster), BindingFlags.Instance | BindingFlags.NonPublic);
        }
        public static object EnsureSiteMaster(object mngner,string webTemplate, uint nLCID, int compatibilityLevel)
        {
            return GetDelegate_EnsureSiteMaster()(mngner,webTemplate, nLCID, compatibilityLevel);
        }
        #endregion
    }
}
