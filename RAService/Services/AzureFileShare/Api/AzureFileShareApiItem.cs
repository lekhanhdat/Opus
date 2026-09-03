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
using AvePoint.RA.Service.Services.AzureFileShare.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.Service.Services.AzureFileShare.Api
{
    public class AzureFileShareApiItem
    {
        public Guid Id { get; internal set; }

        public Guid ParentId { get; internal set; }

        public string RealId { get; internal set; }

        public string Name { get; internal set; }

        public string FullPath { get; internal set; }

        public string RelativePath { get; internal set; }

        public bool IsDirectory { get; internal set; }

        public long Created { get; internal set; }

        public long Modified { get; internal set; }

        public long LastAccessTime { get; internal set; }

        public long? Size { get; internal set; }

        public bool IsRoot { get; internal set; }
        
        public AzureFileShareApiContext ApiContext { get; private set; }

        internal AzureFileShareApiItem(AzureFileShareApiContext apiContext)
        {
            ApiContext = apiContext;
        }

        public AzureFileShareApiDirectoryClient ToDirectoryClient()
        {
            if(!IsDirectory)
            {
                throw new AzureFileShareApiException("Can't convert file client to directory client.");
            }

            return new AzureFileShareApiDirectoryClient(ApiContext, FullPath)
            {
                Id = Id,
                ParentId = ParentId,
                RealId = RealId,
                Name = Name,
                FullPath = FullPath,
                RelativePath = RelativePath,
                Created = Created,
                Modified = Modified,
                IsLoadedProperties = true
            };
        }

        public AzureFileShareApiFileClient ToFileClient()
        {
            if(IsDirectory)
            {
                throw new AzureFileShareApiException($"Can't convert directory client to file client.");
            }

            return new AzureFileShareApiFileClient(ApiContext, FullPath)
            {
                Id = Id,
                ParentId = ParentId,
                RealId = RealId,
                Name = Name,
                FullPath = FullPath,
                RelativePath = RelativePath,
                Created = Created,
                Modified = Modified,
                IsLoadedProperties = true
            };
        }
    }
}
