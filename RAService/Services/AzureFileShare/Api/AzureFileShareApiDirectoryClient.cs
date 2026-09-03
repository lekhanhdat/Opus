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
using Azure.Storage.Files.Shares.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.Service.Services.AzureFileShare.Api
{
    public class AzureFileShareApiDirectoryClient
    {
        private static readonly RALogger Logger = RALogger.GetInstance(typeof(AzureFileShareApiDirectoryClient));

        public Guid Id { get; internal set; }

        public Guid ParentId { get; internal set; }

        public string RealId { get; internal set; }

        public string Name { get; internal set; }

        public string FullPath { get; internal set; }

        public string RelativePath { get; internal set; }

        public long Created { get; internal set; }

        public long Modified { get; internal set; }

        public bool IsRoot { get; internal set; }

        public List<AzureFileShareApiItem> SubDirectories => GetSubDirectories();

        public List<AzureFileShareApiItem> SubFiles => GetSubFiles();

        public int SubItemsCount => GetSubItemsCount();

        public bool IsLoadedProperties { get; internal set; }

        public AzureFileShareApiContext ApiContext { get; private set; }

        public AzureFileShareApiDirectoryClient Parent => GetParent();

        private ShareDirectoryClient DirectoryClient { get; set; }

        public AzureFileShareApiDirectoryClient(AzureFileShareApiContext apiContext, string fullPath)
        {
            var url = AzureFileShareApiUtil.UrlCombin(apiContext.ConnectionInfo.AccessEndPoint, apiContext.ConnectionInfo.FileShareName);
            if(!fullPath.Contains(url))
            {
                throw new AzureFileShareApiException("Azure file share url not match.");
            }
            ApiContext = apiContext;

            FullPath = AzureFileShareApiUtil.UrlCorrect(fullPath);
            RelativePath = AzureFileShareApiUtil.UrlCorrect(fullPath.Replace(url, ""));
            DirectoryClient = ApiContext.GetDirectory(RelativePath);
            if (string.IsNullOrEmpty(RelativePath))
            {
                IsRoot = true;
            }
        }

        public bool Exist()
        {
            try
            {
                return DirectoryClient.Exists().Value; 
            }
            catch(RequestFailedException e)
            {
                Logger.Error($"Failed to get azure storage file share directory item, has request failed exception. Error: {e}");
            }
            catch(Exception e)
            {
                Logger.Error($"Failed to get azure storage file share directory item. Error: {e}");
            }
            return false;
        }

        public void LoadProperties()
        {
            if(IsLoadedProperties)
            {
                return;
            }
            var properties = DirectoryClient.GetProperties().Value;
            var subProperties = properties.SmbProperties;
            if(IsRoot)
            {
                Id = AzureFileShareApiUtil.GenerateId(FullPath);
                Name = "";
                ParentId = Guid.Empty;
            }
            else
            {
                Id = AzureFileShareApiUtil.GenerateId(FullPath);
                Name = RelativePath.Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries).Last();
                ParentId = AzureFileShareApiUtil.GenerateParentId(FullPath, Name);
            }
            RealId = subProperties.FileId;
            Created = subProperties.FileCreatedOn.HasValue ? subProperties.FileCreatedOn.Value.UtcDateTime.Ticks : 0;
            Modified = properties.LastModified.UtcDateTime.Ticks;
            IsLoadedProperties = true;
        }

        public int GetSubItemsCount()
        {
            return DirectoryClient.GetFilesAndDirectories().Count();
        }

        public List<AzureFileShareApiItem> GetSubDirectoriesAndFiles(int skipCount, int takeCount)
        {
            var items = DirectoryClient.GetFilesAndDirectories(
                    new ShareDirectoryGetFilesAndDirectoriesOptions
                    {
                        Traits = ShareFileTraits.All
                    }
                )
                .OrderBy(item => item.IsDirectory)
                .ThenBy(item => item.Name)
                .Skip(skipCount)
                .Take(takeCount)
                .ToList();
            return items.ConvertAll(Convert);
        }

        private AzureFileShareApiDirectoryClient GetParent()
        {
            if(IsRoot)
            {
                return null;
            }

            var parentFullPath = AzureFileShareApiUtil.UrlCorrect(FullPath);
            var lastIndex = parentFullPath.LastIndexOf(Name);
            parentFullPath = parentFullPath.Substring(0, lastIndex);
            parentFullPath = AzureFileShareApiUtil.UrlCorrect(parentFullPath);

            return new AzureFileShareApiDirectoryClient(ApiContext, parentFullPath);
        }

        private List<AzureFileShareApiItem> GetSubDirectories()
        {
            var directories = DirectoryClient.GetFilesAndDirectories(new ShareDirectoryGetFilesAndDirectoriesOptions { Traits = ShareFileTraits.All }).Where(item => item.IsDirectory).ToList();
            return directories.ConvertAll(Convert);
        }

        private List<AzureFileShareApiItem> GetSubFiles()
        {
            var directories = DirectoryClient.GetFilesAndDirectories(new ShareDirectoryGetFilesAndDirectoriesOptions { Traits = ShareFileTraits.All }).Where(item => !item.IsDirectory).ToList();
            return directories.ConvertAll(Convert);
        }

        private AzureFileShareApiItem Convert(ShareFileItem item)
        {
            return new AzureFileShareApiItem(ApiContext)
            {
                Id = AzureFileShareApiUtil.GenerateId(AzureFileShareApiUtil.UrlCombin(FullPath, item.Name)),
                ParentId = Id,
                RealId = item.Id,
                Name = item.Name,
                FullPath = AzureFileShareApiUtil.UrlCombin(FullPath, item.Name),
                RelativePath = AzureFileShareApiUtil.UrlCombin(RelativePath, item.Name),
                Created = item.Properties.CreatedOn.HasValue ? item.Properties.CreatedOn.Value.UtcDateTime.Ticks : 0,
                Modified = item.Properties.LastModified.HasValue ? item.Properties.LastModified.Value.UtcDateTime.Ticks : 0,
                IsDirectory = item.IsDirectory,
                LastAccessTime = item.Properties.LastAccessedOn.HasValue ? item.Properties.LastAccessedOn.Value.UtcDateTime.Ticks : 0,
                Size = item.FileSize
            };
        }
    }
}
