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

namespace AvePoint.GCommon.GraphAPI
{
    using Newtonsoft.Json;

    public class PrivateChannelSite : EntityBase
    {
        [JsonProperty("createdDateTime")]
        public string CreatedDateTime { get; set; }

        [JsonProperty("lastModifiedDateTime")]
        public string LastModifiedDateTime { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("webUrl")]
        public string WebUrl { get; set; }

        [JsonProperty("size")]
        public long Size { get; set; }

        [JsonProperty("parentReference")]
        public ParentReference ParentReference { get; set; }

        [JsonProperty("fileSystemInfo")]
        public FileSystemInfo FileSystemInfo { get; set; }

        [JsonProperty("folder")]
        public TCFolder Folder { get; set; }
    }

    public class ParentReference
    {

        [JsonProperty("driveId")]
        public string DriveId { get; set; }

        [JsonProperty("driveType")]
        public string DriveType { get; set; }
    }

    public class FileSystemInfo
    {

        [JsonProperty("createdDateTime")]
        public string CreatedDateTime { get; set; }

        [JsonProperty("lastModifiedDateTime")]
        public string LastModifiedDateTime { get; set; }
    }

    public class TCFolder
    {

        [JsonProperty("childCount")]
        public int ChildCount { get; set; }
    }
}