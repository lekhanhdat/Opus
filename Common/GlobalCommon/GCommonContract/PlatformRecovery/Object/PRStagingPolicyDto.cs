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
using System.Collections.Generic;
using System.Runtime.Serialization;
using AvePoint.GCommon.Contract.Common;

namespace AvePoint.GCommon.Contract.PlatformRecovery.Object
{
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class PRStagingPolicyDto
    {
        #region base
        [DataMember]
        public string SQLAgentName { get; set; }
        [DataMember]
        public long MinLeaveSpace { get; set; }
        [DataMember]
        public NumberType LeaveSpaceNumberType { get; set; }
        [DataMember]
        public string TempDBPath { get; set; }
        [DataMember]
        public string TempLogPath { get; set; }
        [DataMember]
        public string FreeSpace { get; set; }
        [DataMember]
        public FreeSpaceType FreeSpaceType { get; set; }
        [DataMember]
        public string TempDBFileLocation { get; set; }
        [DataMember]
        public string ServiceID { get; set; }
        [DataMember]
        public bool AvailableSpacePassed { get; set; }
        [DataMember]
        public bool DataLocationPassed { get; set; }
        [DataMember]
        public bool LogLocationPassed { get; set; }
        [DataMember]
        public bool SqlServerAcountPassed { get; set; }
        [DataMember]
        public OldErrorCode StagingNamePassedCode { get; set; }
        #endregion

        //#region live model加载tree时使用，校验Cluster
        //[DataMember]
        //public bool IsClustered { get; set; }

        //[DataMember]
        //public List<string> PhysicalNodeList { get; set; }

        //[DataMember]
        //public List<ServiceDto> PhysicalNodeServiceList { get; set; }
        //#endregion

        #region 与PRSN公用属性
        [DataMember]
        public string Id { get; set; }
        [DataMember]
        public string StagingPolicyName { get; set; }// ProfileName
        [DataMember]
        public string SQLInstanceName { get; set; }//ServerName
        [DataMember]
        public AuthenticationType Authentication { get; set; }
        [DataMember]
        public string Username { get; set; }
        [DataMember]
        public string Password { get; set; }
        [DataMember]
        public string Description { get; set; }
        #endregion

        #region PRSN for verification and index
        [DataMember]
        public bool IsAllFarmsUsedServer { get; set; }
        [DataMember]
        public List<string> FarmNames { get; set; }
        #endregion

        //[DataMember]
        //public string Id { get; set; }
        //[DataMember]
        //public string ProfileName { get; set; }
        //[DataMember]
        //public string ServerName { get; set; }
        //[DataMember]
        //public AuthenticationType Authentication { get; set; }
        //[DataMember]
        //public string Username { get; set; }
        //[DataMember]
        //public string Description { get; set; }
        //[DataMember]
        //public bool IsAllFarmsUsedServer { get; set; }
        //[DataMember]
        //public List<string> FarmNames { get; set; }
        //[DataMember]
        //public PRSNErrorCode StagingErrorCode { get; set; }
        /// <summary>标记平台属性</summary>
        [DataMember]
        public PRPlatformType PlatformType { get; set; }
    }

    [Flags, DataContract(Namespace = ContractConstants.Namespace)]
    public enum FreeSpaceType
    {
        [EnumMember]
        MB = 0,
        [EnumMember]
        GB = 1,
    }

    [Flags, DataContract(Namespace = ContractConstants.Namespace)]
    public enum AuthenticationType
    {
        [EnumMember]
        Undefined = 0,
        [EnumMember]
        WindowsAuthentication = 1,
        [EnumMember]
        SQLServerAuthentication = 2     
    }

    [Flags, DataContract(Namespace = ContractConstants.Namespace)]
    public enum OldErrorCode
    {
        [EnumMember]
        NoError = 0,
        [EnumMember]
        StagingPolicyNameError = 1,
        [EnumMember]
        AccountInfoError = 2,
        [EnumMember]
        TemporaryDBError = 3,
    }

    [Flags, DataContract(Namespace = ContractConstants.Namespace)]
    public enum NumberType
    {
        [EnumMember]
        Undefined = 0,
        [EnumMember]
        Byte = 1,
        [EnumMember]
        KB = 2,
        [EnumMember]
        MB = 3,
        [EnumMember]
        GB = 4,
        [EnumMember]
        TB = 5,
    }
}
