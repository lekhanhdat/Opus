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
using AvePoint.GCommon.Contract.Tree.Object;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.Contract.ManualApproval.Enums;
using Cloud.Sdk.Data.Amls.Ics.Contracts;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Drawing.Printing;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.Contract.ManualApproval.Model
{
    public class ManualApprovalFilterFolderPathResult
    {
        [JsonProperty("folderPathResults")]
        public HashSet<string> FolderPathResults { get; set; }

        [JsonProperty("folderPathResultsCount")]
        public int FolderPathResultsCount { get; set; }

        [JsonProperty("continuation")]
        public string Continuation { get; set; }

    }
    [DataContract]
    public class ManualApprovalFolderPathQueryDefinition
    {
        [DataMember]
        public string SearchValue { get; set; }
        [DataMember]
        public List<string> WorkSpaceSource { get; set; }
        [DataMember]
        public int ContentSource { get; set; }
        [DataMember]
        public int PageIndex { get; set; }
        [DataMember]
        public int PageSize { get; set; }
        [DataMember]
        public string Continuation { get; set; }
        [DataMember]
        public ManualApprovalTab ManualApprovalTab { get; set; }
        [DataMember]
        public string PartitionKeyId { get; set; }
    }
    public class PaginateQueryFolderPathResult
    {

        public string Continuation { get; set; }

        public HashSet<string> Items { get; set; }
    }


}