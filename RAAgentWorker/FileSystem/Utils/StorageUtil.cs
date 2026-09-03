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
using AvePoint.GCommon.Contract.Storage.Entity;
using AvePoint.GCommon.Utility.Cryptography;
using AvePoint.Hybrid.Utility.Util;
using AvePoint.RA.Api.Contract;
using AvePoint.RA.Contract.JobMonitor;
using AvePoint.RA.Contract.Services;
using AvePoint.RA.Contract.Tenant;
using Azure.Storage.Blobs;
using RAFileSystem.FileSystem.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;


namespace AvePoint.RA.CommonUtil
{
    public class RAStorageUtil
    {
        private static RALogger logger = RALogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);

        private static string GetConnString(string accountName, string accessKey, int accountType)
        {
            //Maybe useless code
            ThrowUtil.ThrowIfNull(accountName, "accountName");
            ThrowUtil.ThrowIfNull(accessKey, "accessKey");
            string connString = null;
            if (accountType == 0)
            {
                connString = string.Format("DefaultEndpointsProtocol=https;AccountName={0};AccountKey={1}", accountName, accessKey);
            }
            else if (accountType == 1)
            {
                connString = string.Format("DefaultEndpointsProtocol=https;AccountName={0};AccountKey={1};EndpointSuffix=core.usgovcloudapi.net", accountName, accessKey);
            }
            else
            {
                throw new NotSupportedException("Unsupported account type");
            }
            return connString;
        }

        
        public static BlobContainerClient GetBlobContainerClientByStorageXRI(string xri)
        {
            ConnectionBuilder xriObj = ConnectionBuilder.ValueOf(xri);
            string accessPoint = string.Empty;
            string containerName = string.Empty;
            string accountName = string.Empty;
            string accountKey = string.Empty;

            if (xriObj.Params.ContainsKey("accesspoint"))
            {
                accessPoint = xriObj.Params["accesspoint"];
            }
            if (xriObj.Params.ContainsKey("containername"))
            {
                containerName = xriObj.Params["containername"];
            }
            if (xriObj.Params.ContainsKey("name"))
            {
                accountName = xriObj.Params["name"];
            }
            if (xriObj.Params.ContainsKey("secret"))
            {
                accountKey = CryptoUtil.ConvertBytesToString(CspCrossPlatformExchangeWrapper.UnWrapKey(xriObj.Params["secret"]));
            }
            logger.Info("Blob url: {0}, container name: {1}, account name: {2}", accessPoint.LogBase64(), containerName.LogBase64(), accountName.LogBase64());

            string connString = null;
            var accessPointUri = new Uri(accessPoint);
            if (string.IsNullOrEmpty(accountKey))
            {
                connString = $"{accountName}.{accessPointUri.Authority}";
            }
            else
            {
                var blobPrefix = "blob.";
                var endpointSuffix = accessPoint.Substring(accessPoint.LastIndexOf(blobPrefix) + blobPrefix.Length);
                if (endpointSuffix.IndexOf('/') > 0)
                {
                    endpointSuffix = endpointSuffix.Split('/')[0];
                }
                connString = $"DefaultEndpointsProtocol={accessPointUri.Scheme};AccountName={accountName};AccountKey={accountKey};EndpointSuffix={endpointSuffix}";
            }
            return UtilForStorage.GetContainerClient(connString, containerName);
        }

       
        private static string ArchivedContentContainer = "archivedcontent";
        private static string EmailImageBase64Container = "imagecontent";

        
        private static string ChangeLogContainer => "changelogreport";

    }
}
