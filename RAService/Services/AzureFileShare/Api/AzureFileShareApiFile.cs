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
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Service.Services.AzureFileShare.Exceptions;
using AvePoint.Records.Core.Utilities.Extensions;
using Azure;
using Azure.Storage.Files.Shares;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.Service.Services.AzureFileShare.Api
{
    public class AzureFileShareApiFile
    {
        private static readonly RALogger Logger = RALogger.GetInstance(typeof(AzureFileShareApiFile));

        public Guid Id { get; internal set; }

        public string RealId { get; internal set; }

        public string Name { get; internal set; }

        public string FullPath { get; internal set; }

        public string RelativePath { get; internal set; }

        public long Created { get; internal set; }

        public long Modified { get; internal set; }

        public AzureFileShareApiContext ApiContext { get; private set; }

        public AzureFileShareApiDirectory Parent => GetParent();

        private ShareFileClient FileClient { get; set; }

        public AzureFileShareApiFile(AzureFileShareApiContext apiContext, string fullPath)
        {
            var url = apiContext.ConnectionInfo.AccessEndPoint + "/" + apiContext.ConnectionInfo.FileShareName;
            if (!fullPath.Contains(url))
            {
                throw new AzureFileShareApiException("Azure file share url not match.");
            }

            ApiContext = apiContext;
            FullPath = fullPath;
            RelativePath = fullPath.Replace(url, "");
            FileClient = ApiContext.GetFile(RelativePath);
        }

        public bool Exist()
        {
            try
            {
                return FileClient.Exists().Value;
            }
            catch (RequestFailedException e)
            {
                Logger.Error($"Failed to get azure storage file share file item, has request failed exception. Error: {e}");
            }
            catch (Exception e)
            {
                Logger.Error($"Failed to get azure storage file share file item. Error: {e}");
            }
            return false;
        }

        public void LoadProperties()
        {
            var properties = FileClient.GetProperties().Value;
            var subProperties = properties.SmbProperties;
            Id = subProperties.FileId.ToLower().ToMd5();
            RealId = subProperties.FileId;
            Created = subProperties.FileCreatedOn.Value.UtcDateTime.Ticks;
            Modified = properties.LastModified.UtcDateTime.Ticks;
            Name = RelativePath.Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries).Last();
        }

        private AzureFileShareApiDirectory GetParent()
        {
            string parentFullPath;
            if (FullPath.EndsWith("/"))
            {
                parentFullPath = FullPath.Substring(0, FullPath.Length - 1);
            }

            parentFullPath = FullPath.Replace(Name, "");
            if (parentFullPath == "/")
            {
                parentFullPath = "";
            }
            return new AzureFileShareApiDirectory(ApiContext, parentFullPath);
        }
    }
}
