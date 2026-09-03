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





using System;
using System.Runtime.Serialization;
using AvePoint.GCommon.Contract.Common;

namespace AvePoint.GCommon.Contract.Replicator.Object.Message
{
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class ReplicatorFileHeader
    {
        [DataMember]
        public ReplicatorFileHeaderType Type { get; set; }

        [DataMember]
        public string SourcePath { get; set; }

        [DataMember]
        public string DestPath { get; set; }

        [DataMember]
        public long Time { get; set; }

        [DataMember]
        public long Size { get; set; }

        [DataMember]
        public DateTime RestoreTime { get; set; }

        [DataMember]
        public ReplicatorFileHeaderOption Option { get; set; }

        [DataMember]
        public ReplicatorFileHeaderStatus Status { get; set; }

    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum ReplicatorFileHeaderType
    {
        [EnumMember]
        WebApp,

        [EnumMember]
        SiteCollection,

        [EnumMember]
        Web,

        [EnumMember]
        List,

        [EnumMember]
        Document,

        [EnumMember]
        Folder
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum ReplicatorFileHeaderOption
    {
        [EnumMember]
        Overwrite,

        [EnumMember]
        Merged,
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum ReplicatorFileHeaderStatus
    {
        [EnumMember]
        Unknown,

        [EnumMember]
        Successful,

        [EnumMember]
        Failed,
    }
}
