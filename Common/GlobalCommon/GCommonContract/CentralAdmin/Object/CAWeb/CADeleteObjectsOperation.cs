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




using System.Runtime.Serialization;
using AvePoint.GCommon.Contract.Common;
using AvePoint.GCommon.Contract.Server.Common.AdminSearch.Object;
using AvePoint.GCommon.Contract.CentralAdmin.Object;
using AvePoint.GCommon.Contract.Tree.Object;

namespace AvePoint.GCommon.Contract.CentralAdmin.Object
{
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class CADeleteObjectsOperation : CAOperation
    {
        [DataMember]
        public string Title { get; set; }
        [DataMember]
        public string Description { get; set; }
        [DataMember]
        public bool HasSubWeb { get; set; }
        [DataMember]
        public string FullPath { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class CADeleteObjectsInfo
    {
        [DataMember]
        public NodeLevel Level { get; set; }
        [DataMember]
        public string Url { get; set; }
        [DataMember]
        public string Title { get; set; }
        [DataMember]
        public DeleteStatus Status { get; set; }
        [DataMember]
        public string Comments { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class DeleteObjectsResult : ResultBase
    {
        [DataMember]
        public string Url { get; set; }

        [DataMember]
        public NodeLevel Level { get; set; }

        [DataMember]
        public string Title { get; set; }

        [DataMember]
        public DeleteStatus Status { get; set; }

        [DataMember]
        public string Comments { get; set; }

        [DataMember]
        public CADeleteObjectsInfo DeleteObjectsInfo { get; set; }

        //[DataMember]
        //public Dictionary<string, CADeleteObjectsInfo> DestUserInfos { get; set; }

    }

    [DataContract(Namespace = (ContractConstants.Namespace))]
    public enum DeleteStatus
    {
        [EnumMember]
        None,
        [EnumMember]
        Failed,
        [EnumMember]
        Succeed,
        [EnumMember]
        Skipped

    }
}
