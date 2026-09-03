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




namespace AvePoint.GCommon.Contract.Migration.Object.OperationResults
{
    #region usings
    using System.Runtime.Serialization;
    using AvePoint.GCommon.Contract.Common;
    #endregion

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class ErrorInformation
    {
        [DataMember]
        public ErrorInformationType Type { get; set; }

        [DataMember]
        public string Message { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum ErrorInformationType : int
    {
        [EnumMember]
        None = 0,

        [EnumMember]
        Unknown = 1,

        [EnumMember]
        NameAlreadyExisted,

        [EnumMember]//will be deleted
        ProfileNameAlreadyExisted,

        [EnumMember]//will be deleted
        PlanNameAlreadyExisted,

        [EnumMember]//not a daylight saving time
        ScheduleTimeError,

        [EnumMember]//start time is not a valid daylight saving time
        ScheduleStartTimeNotValidDST,

        [EnumMember]//end time is not a valid daylight saving time
        ScheduleEndTimeNotValidDST,

        [EnumMember]//start time is earlier than now
        ScheduleStartTimeEarlierThanNow,

        [EnumMember]//start time is later than end time
        ScheduleStartTimeLaterThanEndTime,

        [EnumMember]
        NoAvailableAgent,

        [EnumMember]
        NoAvailableConnection,

        [EnumMember]//when send message to client
        CommunicationError,

        [EnumMember]//will be deleted
        TestMigrationConfigDBUnknownError,

        [EnumMember]
        ValidateDatabaseExistsUnknownError,

        [EnumMember]
        CreateDatabaseUnknownError,

        [EnumMember]//will be deleted
        TestSourceFileManagementUnknownError,

        [EnumMember]//will be deleted
        TestFileConnectionUnknownError,

        [EnumMember]//will be deleted
        TestNotesConnectionUnknownError,

        [EnumMember]//will be deleted
        TestPublicFolderConnectionUnknownError,

        [EnumMember]//when delete a plan
        PlanBeInUsedError,

        [EnumMember]//when delete a profile
        ProfileBeInUsedError,

        [EnumMember]
        PlanIsRunning,

        [EnumMember]
        SubProfileNotExisted,

        #region eroom migration
        #endregion

        #region file migration
        [EnumMember]
        FileNetShareLoginFailure,

        [EnumMember]
        FileNetShareNetworkNameCannotBeFound,

        [EnumMember]
        FileNetShareNetworkAlreadyExists,

        [EnumMember]
        FileBrowseFolderDoesNotExist,

        [EnumMember]
        FileBrowsePermissionError,

        [EnumMember]
        FileBrowseFolderPathTooLong,

        #endregion

        #region livelink migration
        [EnumMember]
        TestLivelinkConnectionUnknownError,

        [EnumMember]
        TestLiveLinkServerUnknowError,

        [EnumMember]
        TestLiveLinkDatabaseConnectionUnknowError,
        #endregion

        #region notes migration
        [EnumMember]
        NotesIsRuning,

        //load inipath error
        [EnumMember]
        NotesLoadDefaultIniPathReadRegistryError,

        [EnumMember]
        NotesLoadDefaultIniPathCannotFindDefaultIniFile,

        //load usersid error
        [EnumMember]
        NotesLoadUserIDMissingAttributeInIniFile,

        [EnumMember]
        NotesLoadUserIDCannotFindDirectory,

        //test notes connection error
        [EnumMember]
        NotesTestConnectionUpdateNotesIniFileError,

        [EnumMember]
        NotesTestConnectionSessionError,

        //load domino server error
        [EnumMember]
        NotesLoadDominoServerCannotGetNsfDatabase,

        [EnumMember]
        NotesLoadDominoServerSerachAddressBookError,

        //load tree view error
        [EnumMember]
        NotesBrowseTreeLoadDatabaseError,

        [EnumMember]
        NotesBrowseLoadViewError,

        [EnumMember]
        NotesBrowseInvalidFilterOption,
        #endregion

        #region public folder migration
        #endregion

        [EnumMember]
        DynamicMappingRuleIsNull,
    }

}
