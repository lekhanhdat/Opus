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
using System.Net;
using System.Text;

namespace AvePoint.Media.Storage.Cloud.OpenStack
{
    class OpenStackV1IdentityService : OpenStackIdentityService
    {
        public override OpenStackIdentityInfo Authentication()
        {
            try
            {
                var request = (HttpWebRequest)WebRequest.Create(this.openStackIdentityInfo.AuthenticationURL);
                request.Headers.Add(OpenStackConstants.X_STORAGE_USER, openParameter.UserName);
                request.Headers.Add(OpenStackConstants.X_STORAGE_PASS, openParameter.Password);
                request.Method = OpenStackConstants.HttpMethod_POST;
                using (var response = request.GetResponse() as HttpWebResponse)
                {
                    if (response != null && response.StatusCode == HttpStatusCode.NoContent)
                    {
                        using (var reader = new StreamReader(response.GetResponseStream()))
                        {
                            this.openStackIdentityInfo.StorageURL = response.Headers[OpenStackConstants.X_STORAGE_URL];
                            this.openStackIdentityInfo.CdnURL = response.Headers[OpenStackConstants.X_CDN_MANAGEMENT_URL];
                            this.openStackIdentityInfo.AuthToken = response.Headers[OpenStackConstants.X_AUTH_TOKEN];
                            this.openStackIdentityInfo.HasAuthentication = true;
                            logger.Info("Authentication Succeed.");
                        }
                    }
                    else if (response != null && response.StatusCode == HttpStatusCode.Unauthorized)
                    {
                        using (var reader = new StreamReader(response.GetResponseStream()))
                        {
                            var errorMessage = reader.ReadToEnd();
                            openStackIdentityInfo.HasAuthentication = false;
                            logger.Info("Authentication Failed. " + errorMessage);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                logger.Error("Authentication failed : {0}" , e);
                throw;
            }
            return openStackIdentityInfo;
        }
    }
}
