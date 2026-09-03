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




namespace AvePoint.GCommon.Contract.Common
{
    #region using directices
    using System;
    using System.Runtime.Serialization;
    using AvePoint.GCommon.Contract.AveModuleContract;
    #endregion

    public class ContractConstants
    {
        public static readonly String CORE_IOC_CONTAINER_IDENTIFIER = "CoreIOCContainerIdentifier";
        public static readonly String NETWORKSERVER_RUNNING_STATUS_IDENTIFIER = "NetworkServerRunningStatusIdentifier";
        public static readonly String NETWORKSERVER_RUNNING_STATUS_TRACESTART_IDENTIFIER = "NetworkServerRunningStatusTraceStartIdentifier";
        public static readonly String NETWORKSERVER_STARTING_ERRORMESSAGE_IDENTIFIER = "NetworkServerStartingErrorMessageIdentifier";

        public const string Namespace = "http://www.avepoint.com";
        public const string NamespaceSQLServerDataManager = "http://www.avepoint.com/SQLServerDataManager";
        public const string NamespacePlatformRecovery = "http://www.avepoint.com/PlatformRecovery";
        public const string SHAREPOINT = "SharePoint_TREE";
        public const int Node_Count_PerPage = 5;
        /**
         * 
         * for tree node type
         */
        public const int VirtualNode = -2;
        public const int FarmType = -1;
        public const int WebAppType = 2;
        public const string SHAREPOINT_TREE = "SharePointTree";
        public const string SHAREPOINT_TREE_ADMIN_SEARCH = "SharePointSearchTree";
        public const string SHAREPOINT_TREE_SECURITY_SEARCH = "SharePointSecuritySearchTree";
        public const string MIGRATION_SOURCE_TREE = "MigrationSouceTree";
        public const string MIGRATION_DEST_TREE = "MigrationDestTree";

        /*
         * For WCF Message header name
         * */
        public const string WCF_MESSAGE_HEADER_NAME = "AveMessageHeader";
        public const string WCF_MESSAGE_HEADER_CHALLENGE_INFO = "AveChallengeHeader";
        public const string WCF_MESSAGE_HEADER_INFO = "AveHeaderInfo";
        public const string WCF_MESSAGE_HEADER_ACTION = "AveActionHeader";

        //Database Columns
        public const string JOB_ID = "JobId";
        public const string TYPE = "Type";
        public const string START_TIME = "StartTime";
        public const string FINISH_TIME = "FinishTime";
        public const string PROGRESS = "Progress";
        public const string STATE = "State";
        public const string PARENT_ID = "ParentId";
        public const string UPDATE_Time = "UpdateTime";
        public const string PLAN_Name = "PlanName";

        public const string INT_1 = "Int1";
        public const string INT_2 = "Int2";
        public const string INT_3 = "Int3";
        public const string INT_4 = "Int4";
        public const string INT_5 = "Int5";
        public const string INT_6 = "Int6";
        public const string INT_7 = "Int7";
        public const string INT_8 = "Int8";
        public const string INT_9 = "Int9";
        public const string INT_10 = "Int10";
        public const string INT_11 = "Int11";
        public const string INT_12 = "Int12";
        public const string INT_13 = "Int13";
        public const string INT_14 = "Int14";
        public const string INT_15 = "Int15";
        public const string INT_16 = "Int16";

        public const string LONG_1 = "Long1";
        public const string LONG_2 = "Long2";
        public const string LONG_3 = "Long3";
        public const string LONG_4 = "Long4";

        public const string STRING_1 = "String1";
        public const string STRING_2 = "String2";
        public const string STRING_3 = "String3";
        public const string STRING_4 = "String4";
        public const string STRING_5 = "String5";
        public const string STRING_6 = "String6";
        public const string STRING_7 = "String7";
        public const string STRING_8 = "String8";
        public const string STRING_9 = "String9";
        public const string STRING_10 = "String10";
        public const string STRING_11 = "String11";
        public const string STRING_12 = "String12";
        public const string STRING_13 = "String13";
        public const string STRING_14 = "String14";
        public const string STRING_15 = "String15";
        public const string STRING_16 = "String16";
        public const string STRING_17 = "String17";
        public const string STRING_18 = "String18";

        public const string CLOB_1 = "Clob1";
        public const string CLOB_2 = "Clob2";


        public const int BACKUP_JOB_DTO_TYPE = (int)JobTypes.BackupJob;
        public const int CA_SEARCH_JOB_DTO_TYPE = (int)JobTypes.CASearchJob;
        public const int CA_JOB_DTO_TYPE = (int)JobTypes.CAJob;
        public const int CONTENTMANAGER_JOB_DTO_TYPE = (int)JobTypes.ContentManagerJob;
        public const int SO_CONVERT_STUB_TO_CONTENT = (int)JobTypes.SOConvertStubToContent;
        public const int RC_COLLECTOR_JOB = (int)JobTypes.RCCollectorJob;

        public const string CountryCode = "[CountryCode]";
        public const string RetentionType = "[RetentionType]";
        public const string StartDate = "[StartDate]";
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum UpdatePlanNameResult
    {
        [EnumMember]
        Error = 0,

        [EnumMember]
        Success = 1,

        [EnumMember]
        NameAlreadyExists = 2,

        [EnumMember]
        PlanNotExists = 3,

        [EnumMember]
        ArgumentsNull = 4
    }
}