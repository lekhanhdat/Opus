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
    public class AzureFileShareApiFileClient
    {
        private static readonly RALogger Logger = RALogger.GetInstance(typeof(AzureFileShareApiFileClient));

        public Guid Id { get; internal set; }

        public Guid ParentId { get; internal set; }

        public string RealId { get; internal set; }

        public string Name { get; internal set; }

        public string FullPath { get; internal set; }

        public string RelativePath { get; internal set; }

        public long Created { get; internal set; }

        public long Modified { get; internal set; }

        public bool IsLoadedProperties { get; internal set; }

        public AzureFileShareApiContext ApiContext { get; private set; }

        public AzureFileShareApiDirectoryClient Parent => GetParent();

        private ShareFileClient FileClient { get; set; }

        public AzureFileShareApiFileClient(AzureFileShareApiContext apiContext, string fullPath)
        {
            var url = AzureFileShareApiUtil.UrlCombin(apiContext.ConnectionInfo.AccessEndPoint, apiContext.ConnectionInfo.FileShareName);
            if (!fullPath.Contains(url))
            {
                throw new AzureFileShareApiException("Azure file share url not match.");
            }

            ApiContext = apiContext;
            FullPath = AzureFileShareApiUtil.UrlCorrect(fullPath);
            RelativePath = AzureFileShareApiUtil.UrlCorrect(fullPath.Replace(url, ""));
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
            if(IsLoadedProperties)
            {
                return;
            }
            var properties = FileClient.GetProperties().Value;
            var subProperties = properties.SmbProperties;
            Id = AzureFileShareApiUtil.GenerateId(FullPath);
            ParentId = AzureFileShareApiUtil.GenerateParentId(FullPath, Name);
            RealId = subProperties.FileId;
            Created = subProperties.FileCreatedOn.Value.UtcDateTime.Ticks;
            Modified = properties.LastModified.UtcDateTime.Ticks;
            Name = RelativePath.Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries).Last();
            IsLoadedProperties = true;
        }

        private AzureFileShareApiDirectoryClient GetParent()
        {
            string parentFullPath = AzureFileShareApiUtil.UrlCorrect(FullPath);
            parentFullPath = parentFullPath.Replace(Name, "");
            parentFullPath = AzureFileShareApiUtil.UrlCorrect(parentFullPath);
            return new AzureFileShareApiDirectoryClient(ApiContext, parentFullPath);
        }
    }
}
