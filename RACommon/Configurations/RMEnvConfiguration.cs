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
using AvePoint.RA.Common.Util;
using AvePoint.RA.Contract.Common;
using AvePoint.RA.Contract.Configurations;
using k8s;
using System;
using System.Linq;
using Util;

namespace AvePoint.RA.Common.Configurations
{
    public class RMEnvConfiguration : RMBaseConfiguration<RMEnvSettingKey>
    {
        public RMEnvConfiguration() : base()
        {
            
        }

        public bool IsDevEnvironment
        {
            get
            {
                return GetBooleanValue(RMEnvSettingKey.DEV_MODE, false);
            }
        }

        public bool IsGCPEnvironment
        {
            get
            {
                var value = RMGlobalConfiguration.EnvSetting[RMEnvSettingKey.ENVIRONMENT_NAME];
                if (string.IsNullOrWhiteSpace(value))
                {
                    return false;
                }
                return ContractConstants.ENVIRONMENT_NAME_GCP.Contains(value?.ToLower());
            }
        }

        private string _productVersion;
        public string ProductVersion
        {
            get
            {
                if (string.IsNullOrEmpty(_productVersion))
                {
                    _productVersion = WebUtil.GetProductVersion();
                }
                return _productVersion;
            }
        }

        private string _roleId;
        public string RoleId
        {
            get
            {
#if DEBUG
                    return "DEV_ENV_ROLE";
#else
                    if (string.IsNullOrEmpty(_roleId))
                    {
                        _roleId = System.Net.Dns.GetHostName();
                    }
                    return _roleId;
#endif
            }
        }

    }

}
