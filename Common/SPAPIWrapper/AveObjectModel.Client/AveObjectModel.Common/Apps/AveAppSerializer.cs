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
using AvePoint.Wrapper.Common;
using AveClientRequest.Common;

namespace AvePoint.ObjectModel.Common
{
    class AveAppSerializer : IAveAppSerializer
    {
        private AveSite mSite;
        private AveWeb mWeb;
        private IAveRequest mRequest;
        private AveRestoreOption mRestoreOption;

        /// <summary>
        /// add this for records for deploy app.
        /// </summary>
        /// <param name="web"></param>
        /// <param name="restoreMode"></param>
        public AveAppSerializer(AveWeb web,int restoreMode)
        {
            mWeb = web;
            mSite = web.Site as AveSite;
            mRequest = (mWeb.Site as AveSite).Request as IAveRequest;
            mRestoreOption = (AveRestoreOption)restoreMode;
        }
        public AveAppSerializer(AveSite site, AveWeb web, int restoreMode)
        {
            mSite = site;
            mWeb = web;
            mRequest = site.Request as IAveRequest;
            mRestoreOption = (AveRestoreOption)restoreMode;
        }

        public AveAppPackageInfo GetObjectData()
        {
            throw new NotImplementedException();
        }

        public IAveAppInstance SetObjectData(AveAppPackageInfo appInfo)
        {
            Dictionary<string, object> restoreInfo = new Dictionary<string, object>();
            restoreInfo["RestoreOption"] = mRestoreOption;            
            IDictionary<string, object> appPropertiesList = mRequest.RestoreApp(mWeb.ServerRelativeUrl, appInfo, restoreInfo, mSite.AvaliableTenantApp, mSite.AvaliableSiteApp);
            var appListProperties = appPropertiesList.GetChildren();
            if (appPropertiesList.ContainsKey(AveObjectModelConstant.IsNewCreated) && (bool)appPropertiesList[AveObjectModelConstant.IsNewCreated])
            {
                mRestoreOption = AveRestoreOption.Restore;
            }
            if (appListProperties.Count > 0)
            {
                return new AveAppInstance(mSite, appListProperties[0]);
            }
            else
            {
                return null;
            }
        }

        public int GetRestoreOption()
        {
            return (int)mRestoreOption;
        }
    }
}
