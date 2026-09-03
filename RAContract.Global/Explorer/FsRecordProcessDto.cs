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
using AvePoint.RA.Contract.Common;
using System;
using System.Runtime.Serialization;

namespace AvePoint.RA.Contract.Explorer
{
    /// <summary>
    /// A lightweight DTO containing only the properties required to process records
    /// with ADS (Alternate Data Streams) during file system data sync.
    /// </summary>
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class FsRecordProcessDto
    {
        /// <summary>
        /// The unique records identifier (ADS-based unique ID).
        /// Used as the dictionary grouping key in ADS processing.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public string RecordsId { get; set; }

        /// <summary>
        /// The node identifier of the record.
        /// Used for logging and deletion operations.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public Guid NodeId { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public Guid NewNodeId { get; set; }
        /// <summary>
        /// The node type (e.g. FSFolder, FSFile).
        /// Used to determine whether to check directory or file existence.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public int NodeType { get; set; }

        /// <summary>
        /// The partition date used for deletion operations.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public int CreateDate { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public long LastAccessTime { get; set; }

        /// <summary>
        /// Serialized metadata containing the local full path of the record.
        /// Used to resolve the physical path and check existence on disk.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public string MetaInfo { get; set; }

        #region Audit props
        [DataMember(EmitDefaultValue = false)]
        public Guid ConnectionId { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public Guid ConnectionGroupId { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public string FullPath { get; set; }

        // Use to store the destination path for moved file.
        [DataMember(EmitDefaultValue = false)]
        public string NewPath { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public int AuditLevel { get; set; }
        #endregion

        // Hold-related properties used in MergeHoldInfo

        [DataMember(EmitDefaultValue = false)]
        public bool HoldStatus { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public int HoldType { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public long HoldReleaseTime { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public string HoldId { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public string HoldBy { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public string HoldByUsers { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public string HoldUntilTimes { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public string[] AppendHolds_Array { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public string DisposalDueDate { get; set; }
    }

    public enum FSJPMCAuditLevel
    {
        Unknown = 0,
        ConnectionGroup = 1,
        Connection = 2,
        Folder = 3,
        File = 4,
    }
}

