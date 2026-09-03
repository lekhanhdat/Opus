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
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Web.Script.Serialization;

namespace AvePoint.Media.Storage.Cloud.OpenStack
{
    class KeystoneIdentityService : OpenStackIdentityService
    {
        protected override void Init(OpenStackOpenParameter openParameter)
        {
            base.Init(openParameter);
            InitIdentityInfo();
        }

        protected virtual void InitIdentityInfo()
        {
            var authInfoTable = new Hashtable
                {
                    {"auth.passwordCredentials.username", this.openParameter.UserName},
                    {"auth.passwordCredentials.password", this.openParameter.Password}
                };

            if (!String.IsNullOrEmpty(openParameter.TenantName))
            {
                authInfoTable.Add("auth.tenantName", openParameter.TenantName);
            }
            if (!String.IsNullOrEmpty(openParameter.TenantId))
            {
                authInfoTable.Add("auth.tenantId", openParameter.TenantId);
            }
            openStackIdentityInfo.TokenJosnPath = "access.token.id".Split(new char[] { '.' });
            var authRequestString = JsonConvertor.GenJsonString(authInfoTable);
            openStackIdentityInfo.AuthenticationURL = openParameter.AuthenticationURL;
            openStackIdentityInfo.AuthRequestString = authRequestString;
        }

        public override OpenStackIdentityInfo Authentication()
        {
            try
            {
                var request = WebRequest.Create(openStackIdentityInfo.AuthenticationURL) as HttpWebRequest;
                request.Method = OpenStackConstants.HttpMethod_POST;
                request.ContentType = "application/json";
                request.Accept = "application/json";
                var buffer = Encoding.UTF8.GetBytes(openStackIdentityInfo.AuthRequestString);
                request.ContentLength = buffer.Length;
                request.GetRequestStream().Write(buffer, 0, buffer.Length);
                using (var response = request.GetResponse() as HttpWebResponse)
                {
                    String httpResultString;
                    if (response != null && response.StatusCode == HttpStatusCode.OK)
                    {
                        using (var reader = new StreamReader(response.GetResponseStream()))
                        {
                            httpResultString = reader.ReadToEnd();
                            HandleResult(httpResultString);
                            openStackIdentityInfo.HasAuthentication = true;
                            logger.Info("Authentication Succeed.");
                        }
                    }
                    else if (response != null && response.StatusCode == HttpStatusCode.Unauthorized)
                    {
                        using (StreamReader reader = new StreamReader(response.GetResponseStream()))
                        {
                            httpResultString = reader.ReadToEnd();
                            HandleErrorResult(httpResultString);
                            openStackIdentityInfo.HasAuthentication = false;
                            logger.Info("Authentication Failed. " + httpResultString);
                        }
                    }
                }
            }
            catch (Exception t)
            {
                logger.Error("Authentication failed : " + t.Message, t);
                throw;
            }
            return openStackIdentityInfo;
        }

        protected virtual void HandleResult(String httpResultString)
        {
            HandleJsonResult(httpResultString);
        }

        protected virtual void HandleErrorResult(string httpResultString)
        {
            var js = new JavaScriptSerializer();
            var jsonData = (Dictionary<String, Object>)js.DeserializeObject(httpResultString);
            openStackIdentityInfo.ErrorMessage = (JsonConvertor.GetValuesFromJson(jsonData, "unauthorized", "message") as string[])[0];
        }

        protected virtual void HandleJsonResult(String httpResultString)
        {
            var jsonData = (Dictionary<String, Object>)JsonConvertor.GetValuesFromJson(httpResultString);
            string regionName = null;
            if (!String.IsNullOrEmpty(openStackIdentityInfo.Region))
            {
                regionName = openStackIdentityInfo.Region;
            }
            else if (openStackIdentityInfo.RegionJosnPath != null && openStackIdentityInfo.RegionJosnPath.Length > 0)
            {
                regionName = (JsonConvertor.GetValuesFromJson(jsonData, openStackIdentityInfo.RegionJosnPath) as string[])[0];
            }
            openStackIdentityInfo.StorageURL = GetEndpointURL(jsonData, regionName, openStackIdentityInfo.StorageEndpointType);
            openStackIdentityInfo.AuthToken = (JsonConvertor.GetValuesFromJson(jsonData, "access", "token", "id") as string[])[0];
            if (openStackIdentityInfo.EnableCDN)
            {
                openStackIdentityInfo.CdnURL = GetEndpointURL(jsonData, regionName, openStackIdentityInfo.CDNEndpointType);
            }
        }

        private string GetEndpointURL(Dictionary<string, object> jsonData, string regionName, string endpointType)
        {
            string endpointURL = null;
            object[] serviceCatalogs = JsonConvertor.GetValuesFromJson(jsonData, "access", "serviceCatalog") as object[];
            foreach (Dictionary<string, object> dict in serviceCatalogs)
            {
                if (endpointType.Equals(dict["type"] as string, StringComparison.OrdinalIgnoreCase))
                {
                    foreach (Dictionary<string, object> endpoints in dict["endpoints"] as object[])
                    {
                        if (regionName == null)
                        {
                            endpointURL = endpoints["publicURL"] as string;
                            break;
                        }
                        if (regionName.Equals(endpoints["region"] as string, StringComparison.OrdinalIgnoreCase))
                        {
                            endpointURL = endpoints["publicURL"] as string;
                            break;
                        }
                    }
                }
            }
            return endpointURL;
        }
    }


    enum EndpointsType
    {
        Object_Store,
        Object_CDN
    }
}
