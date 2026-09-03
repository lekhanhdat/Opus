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
using System.Collections.Generic;
using System.Runtime.Serialization;
using Newtonsoft.Json;

namespace AvePoint.RA.Contract.Discovery.Model.Query.FileSystem.Parameter
{
    [DataContract]
    public class RMDiscoveryFSNodeQueryParameter
    {
        [DataMember]
        [JsonProperty("viewMode")]
        public RMDiscoveryFSNodeViewMode ViewMode { get; set; }
        [DataMember]
        [JsonProperty("searchKey")]
        public string SearchKey { get; set; }
        [DataMember]
        [JsonProperty("joinedContainerId")]
        public int JoinedContainerId { get; set; }
        [DataMember]
        [JsonProperty("containerIds")]
        public List<int> ContainerIds { get; set; } = new();
        [DataMember]
        [JsonProperty("connectionIds")]
        public List<string> ConnectionIds { get; set; } = new();

        [DataMember]
        [JsonProperty("sortBy")]
        public string SortBy { get; set; }

        [DataMember]
        [JsonProperty("isDesc")]
        public bool IsDesc { get; set; }

        [DataMember]
        [JsonProperty("pageIndex")]
        public int PageIndex { get; set; }
        [DataMember]
        [JsonProperty("pageSize")]
        public int PageSize { get; set; }
    }

    [DataContract]
    public enum RMDiscoveryFSNodeViewMode
    {
        [EnumMember]
        None = 0,
        [EnumMember]
        Container = 1,
        [EnumMember]
        Connection = 2,
        [EnumMember]
        ConnectionInContainer = 3,
    }
}
