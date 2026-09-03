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
using System.ServiceProcess;

namespace AutoInstallationCommon.Utility
{
    public class CommonServiceWrapper
    {
        private static CommonServiceWrapper _thisInstance = new CommonServiceWrapper();

        public static CommonServiceWrapper GetInstance()
        {
            if (_thisInstance == null)
            {
                _thisInstance = new CommonServiceWrapper();
                return _thisInstance;
            }

            return _thisInstance;
        }

        /// <summary>
        ///     判断Service是否存在
        /// </summary>
        /// <param name="serviceName"></param>
        /// <returns></returns>
        public bool VerifyServiceExist(string serviceName)
        {
            if (string.IsNullOrEmpty(serviceName)) return false;
            var services = ServiceController.GetServices();
            foreach (var s in services)
                if (s.ServiceName.Equals(serviceName, StringComparison.OrdinalIgnoreCase)) // == serviceName)
                    return true;
            return false;
        }

        /// <summary>
        ///     判断Service是否处于运行状态
        /// </summary>
        /// <param name="serviceName"></param>
        /// <returns></returns>
        public bool VerifyServiceRun(string serviceName)
        {
            if (VerifyServiceExist(serviceName))
            {
                var result = VerifyServiceRunHandler(serviceName);
                if (result) return true;
            }

            return false;
        }

        private bool VerifyServiceRunHandler(string serviceName)
        {
            var service = new ServiceController(serviceName);
            if (service.Status == ServiceControllerStatus.Running)
                return true;
            return false;
        }
    }
}