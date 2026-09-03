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
using AvePoint.GCommon.Contract.Common;

namespace AvePoint.GCommon.Contract.Vault.Object
{
    [KnownType(typeof(ProfileOperationResult))]
    [KnownType(typeof(VaultNodeInfoResponse))]
    [KnownType(typeof(ProPoolOperationResult))]
    [DataContract(Namespace = ContractConstants.Namespace)]
    public abstract class OperationResult
    {
        public bool HasError
        {
            get { return ErrorInformations.Count > 0; }
        }
        [DataMember]
        public List<ErrorInfo> ErrorInformations { get; set; }

        [DataMember]
        public OperationType Operation { get; set; }

        public OperationResult()
        {
            ErrorInformations = new List<ErrorInfo>();
        }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class ErrorInfo
    {
        [DataMember]
        public ErrorInfoType Type { get; set; }

        [DataMember]
        public string Message { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum ErrorInfoType : int
    {
        [EnumMember]
        None = 0,

        [EnumMember]
        Unknown = 1,

        [EnumMember]
        NameExisting = 2,

        [EnumMember]
        SameSize = 3,

        [EnumMember]
        EarlierStartTime = 4,

        [EnumMember]
        PoolNotExisting = 5,

        [EnumMember]
        LicenseExpired = 6
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum OperationType
    {
        #region Processing pool
        [EnumMember]
        AddProcessingPool,
        [EnumMember]
        DeleteProcessingPool,
        [EnumMember]
        EditProcessingPool,
        [EnumMember]
        UpdateProcessingPool,
        #endregion

        #region Index
        [EnumMember]
        ConfigIndex,
        [EnumMember]
        GetAllIndexProfile,
        #endregion

        #region Node Setting
        [EnumMember]
        GetNodeSetting,
        [EnumMember]
        Apply,
        [EnumMember]
        Retract,
        [EnumMember]
        Remove,
        [EnumMember]
        Inherit,
        [EnumMember]
        StopInherit,
        [EnumMember]
        RunNow,
        [EnumMember]
        ApplyAndRunNow,
        #endregion

        #region profile
        [EnumMember]
        EditProfile
        #endregion
    }
}
