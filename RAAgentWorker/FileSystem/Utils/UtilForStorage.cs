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
using Azure.Core;
using Azure.Identity;
using Azure.Storage.Blobs;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RAFileSystem.FileSystem.Utils
{
    public static class UtilForStorage
    {
        private static readonly LocalCache Cache = new LocalCache();
        public static BlobContainerClient GetContainerClient(string connectionString, string containerName, BlobClientOptions options = null)
        {
            string key = containerName + "@" + connectionString;
            return Cache.Get(key, () => IsConnectionString(connectionString) ? new BlobContainerClient(connectionString, containerName, options) : new BlobContainerClient(new Uri("https://" + connectionString + "/" + containerName), GetCredential(), options), TimeSpan.FromHours(2.0));
        }
        internal static bool IsConnectionString(string connectionString)
        {
            return connectionString.StartsWith("DefaultEndpointsProtocol=");
        }
        private static TokenCredential GetCredential()
        {
            //if (ConfigurationSetting.IsDebugMode)
            //{
            //    string value = ConfigurationSetting.GetValue("AZURE_IDENTITY_TENANT_ID");
            //    string value2 = ConfigurationSetting.GetValue("AZURE_IDENTITY_CLIENT_ID");
            //    string value3 = ConfigurationSetting.GetValue("AZURE_IDENTITY_CLIENT_SECRET");
            //    if (value == null && value2 == null && value3 == null)
            //    {
            //        throw new Exception("Invalid configuration value in debug mode to get azure credential.");
            //    }

            //    return new ClientSecretCredential(value, value2, value3);
            //}

            //string value4 = ConfigurationSetting.GetValue(SettingNameConstants.PodIdentity.ClientID);
            //if (value4 != null)
            //{
            //    if (value4.StartsWith("/subscriptions/"))
            //    {
            //        return new ManagedIdentityCredential(new ResourceIdentifier(value4));
            //    }

            //    return new ManagedIdentityCredential(value4);
            //}

            return new ManagedIdentityCredential();
        }
    }
}
