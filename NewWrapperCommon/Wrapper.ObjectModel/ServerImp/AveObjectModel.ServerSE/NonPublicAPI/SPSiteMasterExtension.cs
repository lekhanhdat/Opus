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
    using Wrapper.Core.Util;
    using System;
    using System.Reflection;
    using Microsoft.SharePoint;
    using Wrapper.Common;

    [NonPublicAPI("Microsoft.SharePoint.Administration.SPSiteMaster")]
    static class SPSiteMasterExtension
    {
        private static readonly Type TypeOfSPSiteMaster = typeof(SPSite).Assembly.GetType("Microsoft.SharePoint.Administration.SPSiteMaster");
        #region Delete
        private static Action<object> GetDelegate_Delete()
        {
            return TypeOfSPSiteMaster.GetMethod<Action<object>>(nameof(Delete), BindingFlags.Instance | BindingFlags.NonPublic);
        }
        public static void Delete(object siteMaster)
        {
            GetDelegate_Delete()(siteMaster);
        }
        #endregion

        #region SiteId
        private static Func<object, Guid> GetDelegate_get_SiteId()
        {
            return TypeOfSPSiteMaster.GetMethod<Func<object, Guid>>(nameof(get_SiteId), BindingFlags.Instance | BindingFlags.Public);
        }
        public static Guid get_SiteId(object siteMaster)
        {
            return GetDelegate_get_SiteId()(siteMaster);
        }
        #endregion

        #region TemplateName
        private static Func<object, string> GetDelegate_get_TemplateName()
        {
            return TypeOfSPSiteMaster.GetMethod<Func<object, string>>(nameof(get_TemplateName), BindingFlags.Instance | BindingFlags.Public);
        }
        public static string get_TemplateName(object siteMaster)
        {
            return GetDelegate_get_TemplateName()(siteMaster);
        }
        #endregion

    }
}
