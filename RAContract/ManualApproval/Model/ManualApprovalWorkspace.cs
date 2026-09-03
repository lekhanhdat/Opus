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
using AvePoint.RA.Contract.Explorer;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.Contract.ManualApproval.Model
{
    [DataContract]
    public class ManualApprovalWorkspaceQueryDefinition
    {
        [DataMember]
        public SourceFlag ContentSource { get; set; }
        [DataMember]
        public int PageIndex { get; set; }
        [DataMember]
        public int PageSize { get; set; }
        [DataMember]
        public string SearchValue { get; set; }
        [DataMember]
        public bool IsJpmc { get; set; }
    }

    public class ManualApprovalWorkspacePaginateResult
    {
        [JsonProperty("workspaceItems")]
        public List<ManualApprovalWorkspaceItem> WorkspaceItems { get; set; } = new ();

        [JsonProperty("workspaceCount")]
        public int WorkspaceCount { get; set; }

        [JsonProperty("searchResultCount")]
        public int SearchResultCount { get; set; }
    }

    public class ManualApprovalWorkspaceItem
    {
        [JsonProperty("workspacePath")]
        public string WorkspacePath { get; set; }

        [JsonProperty("workspaceId")]
        public Guid WorkspaceId { get; set; }

        [JsonProperty("extention")]
        public string Extention { get; set; } // Added to store ObjectId for Google workspaces (not a Guid like in SPO)
    }
}
