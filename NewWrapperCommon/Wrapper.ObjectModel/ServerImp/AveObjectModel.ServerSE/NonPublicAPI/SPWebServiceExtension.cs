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
    using Wrapper.Common;
    using Microsoft.SharePoint.Administration;
    using System;
    using System.Collections.Generic;
    using System.Reflection;

    [NonPublicAPI("Microsoft.SharePoint.Administration.SPWebService")]
    internal static class SPWebServiceExtension
    {
        private static readonly Type TypeOfSPWebService = typeof(SPWebService);

        #region get_TemplatesEnabledForSiteMaster
        private static Func<SPWebService, ISet<string>> GetDelegate_get_TemplatesEnabledForSiteMaster()
        {
            return TypeOfSPWebService.GetMethod<Func<SPWebService, ISet<string>>>(nameof(get_TemplatesEnabledForSiteMaster), BindingFlags.Instance | BindingFlags.NonPublic, null, Type.EmptyTypes, null);
        }
        /// <summary>
        /// var service = SPWebService.ContentService;
        /// //Enable-SPWebTemplateForSiteMaster
        /// service.get_TemplatesEnabledForSiteMaster().Add(template);
        /// service.update();
        /// //Disable-SPWebTemplateForSiteMaster
        /// service.get_TemplatesEnabledForSiteMaster().Remove(template);
        /// service.update();
        /// </summary>
        /// <param name="service"></param>
        /// <returns></returns>
        public static ISet<string> get_TemplatesEnabledForSiteMaster(this SPWebService service)
        {
            return GetDelegate_get_TemplatesEnabledForSiteMaster()(service);
        } 
        #endregion

    }
}
