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
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.GCommon.Contract.Server.Common.BackupDataSearch
{
    [DataContract]
    public class SiteCollectionNodesInfo
    {
        [DataMember]
        [JsonProperty]
        public string SiteUrl { get; set; }

        [DataMember]
        [JsonProperty]
        public string SiteGroupId { get; set; }

        [DataMember]
        [JsonProperty]
        public string SPObjectId { get; set; }
        [DataMember]
        [JsonProperty]
        public int PermissionLevel { get; set; }
    }

    [DataContract]
    public class ArchiverRestoreSerchResult
    {
        [DataMember]
        [JsonProperty]
        public string ObjectName { get; set; }
        [DataMember]
        [JsonProperty]
        public string Location { get; set; }
        [DataMember]
        [JsonProperty]
        public string LastModifiedTime { get; set; }
        [DataMember]
        [JsonProperty]
        public string ArchivedTime { get; set; }
        [DataMember]
        [JsonProperty]
        public string CreatedDate { get; set; }
        [DataMember]
        [JsonProperty]
        public string PathMd5 { get; set; }
        [DataMember]
        [JsonProperty]
        public string ParentPathMd5 { get; set; }
        [DataMember]
        [JsonProperty]
        public string TreeNode { get; set; }
        [DataMember]
        [JsonProperty]
        public string JobId { get; set; }
        [DataMember]
        [JsonProperty]
        public string Id { get; set; }
        [DataMember]
        [JsonProperty]
        public string ModifiedBy { get; set; }
        [DataMember]
        public string CreatedDateTicks { get; set; }
        [DataMember]
        public string FullPath { get; set; }
        [DataMember]
        public string SitePath { get; set; }
        [DataMember]
        public long ModifiedTime { get; set; }
        [DataMember]
        public long ArchiveTime { get; set; }
        [DataMember]
        public long ContentLenth { get; set; }
        [DataMember]
        public bool IsArchiveTier { get; set; }
        [DataMember]
        public bool IsSoftDeleted { get; set; }
    }

    [DataContract]
    public class ArchiverRestoreSimpleSearchQueryParameter
    {
        [DataMember]
        public string ContinuationToken { get; set; }

        [DataMember]
        public int PageSize { get; set; }

        [DataMember]
        public string Keyword { get; set; }

        [DataMember]
        public int CategoryId { get; set; }

        [DataMember]
        public string ArchivedStartTime { get; set; }

        [DataMember]
        public string ArchivedEndTime { get; set; }
    }

    [DataContract]
    public class ArchiverRestoreResult: CommonSettingResultForPage
    {
        [JsonProperty]
        [DataMember]
        public List<ArchiverRestoreSerchResult> RestoreSerchNodes { get; set; }
        [JsonProperty]
        [DataMember]
        public BackupDataSearchContract SerchContract { get; set; }

        [JsonProperty]
        [DataMember]
        public bool Failed { get; set; }
        [JsonProperty]
        [DataMember]
        public string OrderBy { get; set; }
        [JsonProperty]
        [DataMember]
        public string Message { get; set; }

        [IgnoreDataMember]
        [JsonIgnore]
        public int OpenIndexDbTimeoutInMs { get; set; }
        [JsonProperty]
        [DataMember]
        public int SearchMode { get; set; }
        [JsonProperty]
        [DataMember]
        public ArchiverRestoreSimpleSearchQueryParameter archiverRestoreSimpleSearchQueryParameter { get; set; }
    }
    [DataContract]
    public class RestoreSiteMappingInfo
    {

        [DataMember]
        public List<SiteMappingInfo> SiteMappings { get; set; }

        [DataMember]
        public long TotalCount { get; set; }
    }
    [DataContract]
    public class SiteMappingInfo
    {

        [DataMember]
        public string Id { get; set; }
        [DataMember]
        public string SourceSiteUrl { get; set; }
        [DataMember]
        public string TargetSiteUrl { get; set; }
    }

    [DataContract]
    public class RestoreSearchWhitelistInfo
    {

        [DataMember]
        public List<WhitelistInfo> SiteCollections { get; set; }

        [DataMember]
        public long TotalCount { get; set; }
    }

    [DataContract]
    public class WhitelistInfo
    {
        [DataMember]
        public string Id { get; set; }
        [DataMember]
        public string SiteCollectionUrl { get; set; }
    }
    public enum RestoreDataSource
    {
        None = 0,
        M365 = 1,
        FS = 2
    }
}
