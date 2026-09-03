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
using AvePoint.Common.FilterEngine.ObjectInfos;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.DB.Explorer.Model;
using AvePoint.RA.Service.Services.AzureFileShare.Api;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.Service.Services.AzureFileShare.Converters
{
    public class AzureFileShareRecordConverter
    {
        public static AzureFileInfo ConvertAzureFileItem2AzureFileInfo(AzureFileShareApiItem azureFileShareApiItem)
        {
            return new AzureFileInfo()
            {
                Title = azureFileShareApiItem.Name,
                Name = azureFileShareApiItem.Name,
                Created = new DateTime(azureFileShareApiItem.Created, DateTimeKind.Utc),
                Modified = new DateTime(azureFileShareApiItem.Modified, DateTimeKind.Utc),
                AccessTime = new DateTime(azureFileShareApiItem.LastAccessTime, DateTimeKind.Utc),
                Size = azureFileShareApiItem.Size == null ? 0 : (long)azureFileShareApiItem.Size,
                Path = azureFileShareApiItem.FullPath
            };
        }

        public static AzureFileInfo ConvertAzureFileItem2AzureFileInfo(Record item)
        {
            var metaInfo = JsonConvert.DeserializeObject<RecordMetaInfo>(item.MetaInfo);
            return new AzureFileInfo()
            {
                Title = item.LeafName,
                Name = item.LeafName,
                Created = new DateTime(item.TimeCreated, DateTimeKind.Utc),
                Modified = new DateTime(item.TimeModified, DateTimeKind.Utc),
                AccessTime = new DateTime(metaInfo.LastAccessTime, DateTimeKind.Utc),
                Size = metaInfo.FileSize,
                Path = AzureFileShareApiUtil.UrlCombin(item.DirPath, item.LeafName),
            };
        }
    }
}
