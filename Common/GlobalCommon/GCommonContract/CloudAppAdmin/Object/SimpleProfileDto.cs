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

namespace AvePoint.GCommon.Contract.CloudAppAdmin.Object
{
    using AvePoint.GCommon.Contract.Common;
    using AvePoint.GCommon.Contract.Server.Common.Profile.Object;
    using System.Runtime.Serialization;

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class SimpleProfileDto
    {
        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public string Id { get; set; }

        [DataMember]
        public string Description { get; set; }

        [DataMember]
        public ProfileType Type { get; set; }

        [DataMember]
        public bool IsBuiltin { get; set; }

        [DataMember]
        public long LastModifiedTime { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class OperationResult
    {
        [DataMember]
        public bool OperationStatus { get; set; }

        [DataMember]
        public OperationError OperationError { get; set; }

        [DataMember]
        public string Remark1 { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum OperationError
    {
        [EnumMember]
        Unknown = 0,

        [EnumMember]
        NameAlreadyExist = 1,

        [EnumMember]
        DeleteFailed = 2,

        [EnumMember]
        RunJobFailed = 3,

        [EnumMember]
        CheckEOCredentialFailed = 4,

        [EnumMember]
        CSVFormatIllegal=5,
    }
}