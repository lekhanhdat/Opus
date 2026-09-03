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
using System.Reflection;
using System.Text;
using AvePoint.GCommon;
using AvePoint.Wrapper.Core.Common;
using AvePoint.Wrapper.Core.SPAPI;
using AvePoint.Wrapper.Core.SPBackup;
using AvePoint.Wrapper.Core.SPRestore;
using AvePoint.Wrapper.Core.Internal;
using AvePoint.Wrapper.Core.IOC;

namespace AvePoint.Wrapper.Core
{
    /// <summary>
    /// Wrapper Factory, 暂时不提供给外围使用，如果需要使用或者修改，请联系Long，谢谢
    /// </summary>
    public static class WrapperFactory
    {
        private static ISPBackupAPI _cacheSPBackupAPI;
        private static ISPRestoreAPI _cacheSPRestoreAPI;

        /// <summary>
        /// Get SP API Utility
        /// </summary>
        /// <param name="spMode"></param>
        /// <returns></returns>
        public static ISPAPIUtility GetSPAPIUtility(WrapperSPMode spMode)
        {
            switch (spMode)
            {
                case WrapperSPMode.Server:
                    {
                        switch (WrapperSPEnv.SPVersion)
                        {
                            case WrapperSPEnv.SPVersionInternal.SharePoint2013:
                                return WrapperCore.Manager.ResolveSPAPI(spMode, new Version(15, 0));
                            case WrapperSPEnv.SPVersionInternal.SharePoint2010:
                                return WrapperCore.Manager.ResolveSPAPI(spMode, new Version(14, 0));
                            case WrapperSPEnv.SPVersionInternal.SharePoint2007:
                                return WrapperCore.Manager.ResolveSPAPI(spMode, new Version(12, 0));
                        }
                    }
                    break;
                case WrapperSPMode.O365:
                    return WrapperCore.Manager.ResolveSPAPI(spMode, new Version(0, 0));
            }

            throw new WrapperException(WrapperResource.GetString(WrapperResourceKey.Wrapper_UnsupportedMode, spMode), WrapperErrorCode.UnsupportedMode);
        }

        /// <summary>
        /// Internal used for the deployment API
        /// </summary>
        /// <param name="spMode"></param>
        /// <returns></returns>
        internal static AvePoint.Wrapper.Core.Internal.IWrapperDeploymentAPI GetWrapperDeploymentAPI(WrapperSPMode spMode)
        {
            switch (spMode)
            {
                case WrapperSPMode.Server:
                    {
                        switch (WrapperSPEnv.SPVersion)
                        {
                            case WrapperSPEnv.SPVersionInternal.SharePoint2013:
                                return WrapperCore.Manager.ResolveWrapperDeploymentAPI(spMode, new Version(15, 0));
                            case WrapperSPEnv.SPVersionInternal.SharePoint2010:
                                return WrapperCore.Manager.ResolveWrapperDeploymentAPI(spMode, new Version(14, 0));
                            case WrapperSPEnv.SPVersionInternal.SharePoint2007:
                                return WrapperCore.Manager.ResolveWrapperDeploymentAPI(spMode, new Version(12, 0));
                        }
                    }
                    break;
                case WrapperSPMode.O365:
                    return WrapperCore.Manager.ResolveWrapperDeploymentAPI(spMode, new Version(0, 0));
            }

            throw new WrapperException(WrapperResource.GetString(WrapperResourceKey.Wrapper_UnsupportedMode, spMode), WrapperErrorCode.UnsupportedMode);
        }

        /// <summary>
        /// Get SPBackup API Utility
        /// </summary>
        /// <returns></returns>
        public static ISPBackupAPI GetSPBackupAPI()
        {
            if (_cacheSPBackupAPI == null)
            {
                _cacheSPBackupAPI = WrapperCore.Manager.Resolve<ISPBackupAPI>();
            }
            return _cacheSPBackupAPI;
        }

        /// <summary>
        /// Get SPBackup API Utility
        /// </summary>
        /// <returns></returns>
        public static ISPRestoreAPI GetSPRestoreAPI()
        {
            if (_cacheSPRestoreAPI == null)
            {
                var instances = WrapperCore.Manager.ResolveAll<ISPRestoreAPI>();

                if (instances.Length > 1)
                {
                    foreach (var item in instances.Where(item => item.CurrentVersion == WrapperConfig.Instance.Restore.DefaultVersion))
                    {
                        _cacheSPRestoreAPI = item;
                        break;
                    }
                    WrapperLogger.Instance.WriteToLogWithResourceKey(WrapperLogger.Level.Warning, WrapperResourceKey.Wrapper_SpecialVersionNotFound, WrapperConfig.Instance.Restore.DefaultVersion);
                }
                if (_cacheSPRestoreAPI == null)
                {
                    _cacheSPRestoreAPI = instances[0];
                }
            }

            return _cacheSPRestoreAPI;
        }
    }
}
