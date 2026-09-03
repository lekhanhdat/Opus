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
using AvePoint.GCommon.Contract.Common;
using AvePoint.RA.Contract.Explorer;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace AvePoint.RA.Contract.RMWeb.Explorer
{
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class GlobalSearchActionDto
    {
        [DataMember]
        public bool IsRealTimeAction { get; set; }
        [DataMember]
        public ExplorerQueryV3Dto FilterInfo { get; set; }
        [DataMember]
        public List<Guid> RecordIds { get; set; }
        [DataMember]
        public GlobalSearchAction Action { get; set; }
        [DataMember]
        public int SourceFlag { get; set; }
        [DataMember]
        public object ActionExtension { get; set; }
        [DataMember]
        public bool ForceDiscoverAll { get; set; }
        [DataMember]
        public string UserId { get; set; }
        [DataMember]
        public ChangeTermOrigin ChangeTermOrigin { get; set; }
        [DataMember]
        public bool IsJpmc { get; set; }
        [DataMember]
        public string PartitionKeyId { get; set; }
    }


    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum GlobalSearchAction
    {
        [EnumMember]
        None = 0,
        [EnumMember]
        Reclassify = 1,
        [EnumMember]
        MoveTo = 2,
        [EnumMember]
        DeclareRecords = 3,
        [EnumMember]
        UnDeclareRecords = 4,
        [EnumMember]
        AccessControl = 5,
        [EnumMember]
        PhysicalBulkUpdate = 6,
        [EnumMember]
        AddRecordLabel = 7,
        [EnumMember]
        RemoveRecordLabel = 8,
    }
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class GlobalSearchExportDto
    {
        [DataMember]
        public List<SelectedColumn> SelectedColumns { get; set; }
        [DataMember]
        public ExplorerQueryV3Dto FilterInfo { get; set; }
        [DataMember]
        public string ExportLocationId { get; set; }
        [DataMember]
        public string ExportLocationName { get; set; }
        [DataMember]
        public string UserId { get; set; }
    }
    [DataContract]
    public class SelectedColumn
    {
        [DataMember]
        public string DisplayName { get; set; }
        [DataMember]
        public string UniqueId { get; set; }
    }
}
