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

namespace AvePoint.GCommon.Contract.Replicator.Object.OperationResults
{
    public interface IReplicatorOperationResult
    {
        bool HasError { get; }

        ReplicatorOperationResultError Error { get; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public abstract class ReplicatorOperationResult : IReplicatorOperationResult
    {
        public ReplicatorOperationResult(bool hasError, ReplicatorOperationResultError exception)
        {
            HasError = hasError;
            Error = exception;
        }

        [DataMember]
        public bool HasError { get; set; }

        [DataMember]
        public ReplicatorOperationResultError Error { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class ReplicatorOperationResultError
    {
        public ReplicatorOperationResultError(ReplicatorOperationResultErrorType errorType)
        {
            ErrorType = errorType;
        }

        [DataMember]
        public ReplicatorOperationResultErrorType ErrorType { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum ReplicatorOperationResultErrorType
    {
        [EnumMember]
        Unknown,

        [EnumMember]
        Plan_Name_Already_Exist,

        [EnumMember]
        Plan_With_No_Mapping,

        [EnumMember]
        Plan_With_Duplicate_Mapping,

        [EnumMember]
        Plan_With_No_Name,

        [EnumMember]
        Plan_Does_Not_Exist,

        [EnumMember]
        Mapping_With_No_Selected_Item_Found,

        [EnumMember]
        Profile_Name_Already_Exist,

        [EnumMember]
        Profile_Does_Not_Have_A_Name,

        [EnumMember]
        Profile_Does_Not_Exist,

        [EnumMember]
        Replication_Sub_Profile_Does_Not_Exist,

        [EnumMember]
        Conflict_Sub_Profile_Does_Not_Exist,

        [EnumMember]
        Profile_Is_In_Use,

        [EnumMember]
        Profile_Default_Cannot_Be_Deleted,

        [EnumMember]
        Profile_Byte_Level_Test_Failed,

        [EnumMember]
        Netshare_Path_Not_Exist,

        [EnumMember]
        Create_DB_Failed,
        [EnumMember]
        Config_DB_Not_Exist,
        [EnumMember]
        Cannot_Create_Config_DB_When_Job_Running,

        [EnumMember]
        Cannot_Create_Config_DB_When_Other_Farm_Uses,

        [EnumMember]
        Load_Default_Config_DB_Failed,

        [EnumMember]
        Rollback_NoBackupBefore,

        [EnumMember]
        TestDBFailed,

        [EnumMember]
        Import_File_Not_Valid_Formatted,

        [EnumMember]
        Import_Plan_Job_Is_Running,

        [EnumMember]
        OnLine_Plan_Has_SiteCollection_To_Share,

        [EnumMember]
        OnLine_Plan_Has_SiteCollection_To_Share_To_Plan_Creater,

        [EnumMember]
        Current_User_Has_No_Permission,

        [EnumMember]
        Cannot_Connect_Service,

        [EnumMember]
        RealTime_Mapping_Source_Not_Exist,
    }
}
