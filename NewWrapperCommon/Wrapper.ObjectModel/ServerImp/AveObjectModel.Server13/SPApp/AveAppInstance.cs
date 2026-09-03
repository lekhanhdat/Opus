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
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AvePoint.Wrapper.Common;
using Microsoft.SharePoint.Administration;

namespace AvePoint.ObjectModel.Server13
{
    public class AveAppInstance : IAveAppInstance
    {
        
        private SPAppInstance appInstance = null;

        #region Methods
        public Guid Install()
        {
            return appInstance.Install();
        }

        public Guid Uninstall()
        {
            return appInstance.Uninstall();
        }

        public void Upgrade(Stream appPackageStream)
        {
            appInstance.Upgrade(appPackageStream);
        }

        public void Upgrade(Stream appPackageStream, IAveWeb web, int appSource)
        {
            SPAppSource source = (SPAppSource)appSource;
            AveWeb aveWeb = (AveWeb)web;
            object[] param = new object[] { appPackageStream, aveWeb.Web, source, false, string.Empty, string.Empty };
            object obj = AveAssemblyUtility.InvokeStaticMethod(typeof(SPApp), "CreateAppUsingPackageMetadata", param);
            if(obj != null)
            {
                AveAssemblyUtility.InvokeGenericMethod(appInstance, "Upgrade", new object[] { obj }, typeof(SPApp));
            }
        }
        #endregion

        #region Properties
        public AveAppInstance(SPAppInstance instance)
        {
            appInstance = instance;
        }

        public Guid Id
        {
            get { return appInstance.Id; }
        }

        public string Title
        {
            get { return appInstance.Title; }
        }

        public IAveApp App
        {
            get { return new AveApp(appInstance.App); }
        }

        public string AppPrincipalId
        {
            get { return appInstance.AppPrincipalId; }
        }

        public Uri AppWebFullUrl
        {
            get { return appInstance.AppWebFullUrl; }
        }

        public Uri LaunchUrl
        {
            get { return appInstance.LaunchUrl; }
        }

        public Guid SiteId
        {
            get { return appInstance.SiteId; }
        }

        public AveAppInstanceStatus Status
        {
            get { return (AveAppInstanceStatus)appInstance.Status; }
        }

        public Guid WebId
        {
            get { return appInstance.WebId; }
        }

        #endregion


       
    }
}
