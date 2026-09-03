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
namespace AvePoint.Application.Storage
{
    using global::Storage;
    using System;
    using System.Text.RegularExpressions;

    public class AzureStorageContext
    {
        private const string FORMAT_CONN_STR = "DefaultEndpointsProtocol=https;AccountName={0};AccountKey={1};EndpointSuffix={2}";

        public string AccountName
        {
            get;
            set;
        }

        public string AccountKey
        {
            get;
            set;
        }

        public string EndpointSuffix
        {
            get;
            set;
        }

        public string ContainerName
        {
            get;
            set;
        }

        public string ConnectionString
        {
            get 
            {
                return string.Format(FORMAT_CONN_STR,
                    AccountName, AccountKey, EndpointSuffix);
            }
        }

        public static (String ConnectionString, String ContainerName) ParseAzureXriConnectionString(String xri)
        {
            var storageContext = AzureStorageContext.ConvertFrom(ConnectionBuilder.ValueOf(xri));
            var connectionString = String.Format(FORMAT_CONN_STR, storageContext.AccountName, storageContext.AccountKey, storageContext.EndpointSuffix);
            return (connectionString,storageContext.ContainerName);
        }

        public static AzureStorageContext ConvertFrom(ConnectionBuilder xri)
        {
            xri.Params.TryGetValue("accesspoint", out var accessPoint);
            xri.Params.TryGetValue("name", out var accountName);
            xri.Params.TryGetValue("secret", out var accountKey);
            xri.Params.TryGetValue("containername", out var containerName);
            accessPoint = Regex.Replace(accessPoint, "^\\w+://(\\S+\\.)*blob\\.", "").Trim('/');
            accessPoint = Regex.Replace(accessPoint, "^\\w+://(\\S+\\.)*table\\.", "").Trim('/');
            return new AzureStorageContext
            {
                AccountName = accountName,
                AccountKey = accountKey,
                EndpointSuffix = accessPoint,
                ContainerName = containerName,
            };
        }
    }

}