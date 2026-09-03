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
using AvePoint.Media.Storage.Util;
using System;

namespace AvePoint.Media.Storage.Cloud.OpenStack
{
    abstract class OpenStackIdentityService
    {
        protected static StorageLogger logger = StorageLogger.GetInstance(typeof(OpenStackIdentityService));
        protected OpenStackIdentityInfo openStackIdentityInfo;
        protected OpenStackOpenParameter openParameter;

        protected virtual void Init(OpenStackOpenParameter openParameter)
        {
            this.openParameter = openParameter;
            openStackIdentityInfo = new OpenStackIdentityInfo();
        }

        public static OpenStackIdentityService GetIdentityService(OpenStackOpenParameter openParameter)
        {
            OpenStackIdentityService openStackIdentityService;
            switch (openParameter.AuthenticationVersion)
            {
                case 1:
                    openStackIdentityService = new OpenStackV1IdentityService();
                    break;
                case 2:
                    //if (openParameter.AuthenticationType.Equals("keystone", StringComparison.OrdinalIgnoreCase))
                    //{
                    //    openStackIdentityService = new KeystoneIdentityService();
                    //}
                    //else if (openParameter.AuthenticationType.Equals("rackspace", StringComparison.OrdinalIgnoreCase))
                    //{
                    //    openStackIdentityService = new RackspaceKeystoneIdentityService();
                    //}
                    //else
                    //{
                    //    openStackIdentityService = new KeystoneIdentityService();
                    //} //todo 减少if判断次数
                    openStackIdentityService = openParameter.AuthenticationType.Equals("rackspace", StringComparison.OrdinalIgnoreCase) ? new RackspaceKeystoneIdentityService() : new KeystoneIdentityService();
                    break;
                default:
                    throw new UnsupportedXException("Unsupported authentication version : " + openParameter.AuthenticationVersion);
            }
            openStackIdentityService.Init(openParameter);
            return openStackIdentityService;
        }

        public abstract OpenStackIdentityInfo Authentication();
    }

}
