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
using AvePoint.Wrapper.Restore;
using WrapperRestoreOption = AvePoint.Wrapper.Restore.AveRestoreOption;
using ClientRestoreOption = AveRestoreOption;

namespace AvePoint.ObjectModel.Common
{
    class AveAppSerializer : IAveAppSerializer
    {
        private AveWeb _ParentWeb;
        private IAveRequest mRequest;
        private ClientRestoreOption mRestoreOption;
        private Guid mProductId = Guid.Empty;

        public AveAppSerializer(AveWeb web)
        {
            _ParentWeb = web;
            mRequest = (_ParentWeb.Site as AveSite).Request;
        }

        #region IAveAppSerializer Members

        public Guid ProductId
        {
            get
            {
                return mProductId;
            }
            set
            {
                mProductId = value;
            }
        }

        public System.IO.Stream GetAppPackage()
        {
            return null;
        }

        public void SetStream(IAveRestoreStream stream)
        {
            //throw new NotImplementedException();
        }

        public void SetRestoreOption(object option)
        {
            WrapperRestoreOption tempOption = option as WrapperRestoreOption;
            mRestoreOption = (ClientRestoreOption)((int)tempOption.mAveRestoreMode);
        }

        #endregion

        #region IAveSerializationSurrogate<AveAppPackageInfo,IAveAppInstance,AveAppPackageInfo> Members

        public AveAppPackageInfo GetObjectData()
        {
            AveAppPackageInfo packageInfo = new AveAppPackageInfo();
            IAveAppInstance appInstance = _ParentWeb.GetAppInstancesByProductId(this.ProductId)[0];
            packageInfo.ProductId = appInstance.App.ProductId;
            packageInfo.Version = appInstance.App.VersionString;
            packageInfo.AppSource = appInstance.App.Source;
            packageInfo.InstanceId = appInstance.Id;

            return packageInfo;
        }

        public IAveAppInstance SetObjectData(AveAppPackageInfo appInfo)
        {
            Dictionary<string, object> restoreInfo = new Dictionary<string, object>();
            restoreInfo["RestoreOption"] = mRestoreOption;
            Dictionary<string, object> appPropertiesList = mRequest.RestoreApp(_ParentWeb.ServerRelativeUrl, appInfo, restoreInfo);
            IList<Dictionary<string, object>> appListProperties = appPropertiesList[AveObjectModelConstant.ChildrenProperties] as IList<Dictionary<string, object>>;
            if (appListProperties.Count > 0)
            {
                return new AveAppInstance(_ParentWeb.Site as AveSite, appListProperties[0]);
            }
            else
            {
                return null;
            }
        }

        #endregion


        public void UpgradeAppByProductId(Guid productId)
        {
            throw new NotImplementedException();
        }

        public int GetRestoreOption()
        {
            return (int)mRestoreOption;
        }
        
        public void TrustApp(Guid productId)
        {
            throw new NotImplementedException();
        }


        public System.IO.Stream GetAppPackageForPRItem13()
        {
            //PRItem do not use client API
            throw new NotImplementedException();
        }

        /// <summary>
        /// 避免build失败现在抛NotImplementedException，该属性需要实现
        /// </summary>
        public IList<IAveAppInstance> Apps
        {
            get { return new AveAppCatalog(this.mRequest).GetAppInstances(this._ParentWeb); }
        }

        public bool CheckAppInstanceUninstalled(IAveAppInstance appInstance)
        {
            //避免build失败现在抛NotImplementedException,该方法需要实现
            throw new NotImplementedException();

        }
    }
}
