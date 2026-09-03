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
namespace AvePoint.Service
{
    using System;
    using GCommon.Utility.Cloud;
    using Microsoft.WindowsAzure;
    using Microsoft.WindowsAzure.ServiceRuntime;
    using System.Collections.Generic;

    class AzureCloudServiceRoleConfiguration : IRoleConfiguration
    {
        private IRoleConfiguration innerDictConfig = new DictRoleConfiguration();

        public string this[string key]
        {
            get
            {
                string value;
                if (!innerDictConfig.TryGetVaule(key, out value))
                {
                    value = GetValue(key, true);
                }
                return value;
            }
            set
            {
                innerDictConfig[key] = value;
            }
        }

        public string GetValue(string key, bool throwExceptionIfNotFound)
        {
            try
            {
                if (!RoleEnvironment.IsAvailable)
                {
                    return null;
                }

                switch (key)
                {
                    case GCommonRoleConfiguration.ConfigKey.IsRoleEnvironment:
                        return RoleEnvironment.IsAvailable.ToString();
                    case GCommonRoleConfiguration.ConfigKey.RoleLocalResourceMaximumSize:
                        {
                            var localResource = RoleEnvironment.GetLocalResource(GCommonRoleConfiguration.ConfigKey.RoleLocalResourceName);
                            if (localResource != null)
                            {
                                return localResource.MaximumSizeInMegabytes.ToString();
                            }
                            return "0";
                        }
                    case GCommonRoleConfiguration.ConfigKey.RoleLocalResourceName:
                        {
                            var localResource = RoleEnvironment.GetLocalResource(GCommonRoleConfiguration.ConfigKey.RoleLocalResourceName);
                            if (localResource != null)
                            {
                                return localResource.RootPath;
                            }
                            return null;
                        }
                    case GCommonRoleConfiguration.ConfigKey.AgentCoreServicePort:
                        return RoleEnvironment.CurrentRoleInstance.InstanceEndpoints["AgentCoreService"].IPEndpoint.Port.ToString();
                    case GCommonRoleConfiguration.ConfigKey.RoleId:
                        return RoleEnvironment.CurrentRoleInstance.Id;
                    case GCommonRoleConfiguration.ConfigKey.DeploymentId:
                        return RoleEnvironment.DeploymentId;
                    default:
                        {
                            var value = CloudConfigurationManager.GetSetting(key);
                            if (value == null && throwExceptionIfNotFound)
                            {
                                throw new KeyNotFoundException(string.Format("The key:{0} is not found.", key));
                            }
                            return value;
                        }
                }
            }
            catch (Exception)
            {
                if (throwExceptionIfNotFound)
                {
                    throw;
                }
            }

            return null;
        }

        public bool TryGetVaule(string key, out string value)
        {
            value = GetValue(key, false);

            return value != null;
        }

        public void Update()
        {
            
        }
    }
}
