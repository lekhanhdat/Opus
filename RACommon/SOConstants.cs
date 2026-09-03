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

namespace AvePoint.RA.Common
{
    public static class SOConstants
    {
        public const string ControlStorageOptimizationCastle = "ControlStorageOptimizationCastle.config";
        public const string ProfileName = "ProfileName";
        public const string TheRuleNameAlreadyExists = "The rule name already exists.";
        public const string EditArchiverRuleSuccessful = "Edit Archiver rule successful.";
        public const string CriteriaFilters = "Criteria Filters: ";
        public const string Compression = "Compression:";
        public const string None = "None";
        public const string Encryption = "Encryption:";
        public const string SecurityProfile = "Security Profile: ";
        public const string N = "\n";
        public const string Criteria = "Criteria:";
        public const string Order = "Order";
        public const string PLAN = "PLAN";
        public const string NodeLevel = "NodeLevel";
        public const string InheritArchiverRuleSuccessful = "Inherit Archiver rule successful.";
        public const string StopInheritArchiverRuleSuccessful = "Stop inherit Archiver rule successful.";

        public const string TestTableName = "TestTable";
        public const string TheJobStartTime = "The job start time is in the period when the DocAve Timer Service is down.";
        public const string ArchiverDBName = "ArchiverDatabase";
        public const string ConfigsNodeIsNull = "ConfigNodes is null or empty.";
        public const string NoEnabledRuleFound = "No enabled rule found.";
        public const string ThereIsAJobCurrently = "There is a job currently running for the specified node, and this job is skipped.";
        public const string NoScanJobFound = "No scan job found.";
        public const string StartJob_ = "StartJob_";
        public const string S = "S";
        public const string A = "A";
        public const string M = "M";
        public const string EA = "EA";
        public const string SubJobId = "_{0:D3}";
        public const string Null = "";
        public const string Name = "Name";
        public const string MarkDoubleBackslash = "\\";
        public const string MarkColon = ":";
        public const string MarkSharp = "#";
        public const string MarkLeftBracket = "(";
        public const string MarkRightBracket = ")";
        public const string MarkPoint = ".";
        public const string MarkSlash = "/";
        public const string MarkSemicolon = ";";
        public const string MarkSpaceAndPoint = " . ";
        public const string Mark10 = " ; \n";
        public const string PrimaryAdministrator = "Primary Administrator";
        public const string ColumnText = "Column(Text):";
        public const string ColumnNumber = "Column(Number):";
        public const string ColumnYesNo = "Column(Yes/No):";
        public const string ColumnDateAndTime = "Column(Date and Time):";
        public const string CustomPropertyText = "Custom Property(Text):";
        public const string CustomPropertyNumber = "Custom Property(Number):";
        public const string CustomPropertyYesNo = "Custom Property(Yes/No):";
        public const string CustomPropertyDateandTime = "Custom Property(Date and Time):";
        public const string From = "From";
        public const string To = "To";

        public const string SOUtilitySiteCollection = "Site Collection";
        public const string SOUtilitySite = "Site";
        public const string LIST1 = "List";
        public const string LIST2 = "Lists/";
        public const string SOUtilityFolder = "Folder";
        public const string SOUtilityList = "List/Library";
        public const string SOUtilityItem = "Item";
        public const string SOUtilityItemVersion = "Item Version";
        public const string SOUtilityDocument = "Document";
        public const string SOUtilityDocumentVersion = "Document Version";
        public const string SOUtilityAttachment = "Attachment";
        public const string Attachments = "/Attachments/";

        public const string SOUtilityContains = "Contains";
        public const string SOUtilityDoesNotContains = "Does Not Contains";
        public const string SOUtilityStartWith = "Start With";
        public const string SOUtilityEndWith = "End With";
        public const string SOUtilityExactly = "Is (Exactly)";
        public const string SOUtilityGreaterOrEqualThan = ">=";
        public const string SOUtilityLessOrEqualThan = "<=";
        public const string SOUtilityBefore = "Before";
        public const string SOUtilityAfter = "After";
        public const string SOUtilityOlderThan = "Older Than";
        public const string SOUtilityOn = "On";
        public const string SOUtilityFromTo = "FromTo";
        public const string SOUtilityEquals = "=";
        public const string SOUtilityExceptLastNVersions = "Except Last NVersions";
        public const string SOUtilityWithIn = "With In";
        public const string SOUtilityIsExactlyNot = "Is (Exactly) Not ";
        public const string SOUtilityMatch = "Match";
        public const string SOUtilityDoesNotMatch = "Does Not Match";
        public const string SOUtilityMajorAndMintorVersions = "Major and Minor Versions";
        public const string SOUtilityOnlyMajorVersions = "Major Versions(with related minor versions)";
        public const string SOUtilityOnlyLastMajorNVersions = "Major Versions Only";
        public const string SOUtilityExceptLastNMajorVersions = "Major Versions";
        public const string SOUtilityNone = "";
        public const string NodeName = "NodeName";
        public const string ARCHIVER_JOB_ID_REGEX_STRING = @"AR\d{20}A\d.*";
        public const string ARCHIVER_SCAN_JOB_ID_REGEX_STRING = @"AR\d{20}S.*";
        public const string ARCHIVER_MERGEINDEX_JOB_ID_REGEX_STRING = @"AR\d{20}M.*";
        public const string StartMergeIndexJobEndWithSpace = "StartMergeIndexJob ";
        public const string StartMergeIndexJob = "StartMergeIndexJob";
        public const string StartBackupJobEndWithSpace = "StartBackupJob ";
        public const string StartBackupJob = "StartBackupJob";
        public const string ExecuteMergeIndex_ = "ExecuteMergeIndex_";
        public const string ExecuteMergeIndex = "ExecuteMergeIndex";

        public const string TreeFormat = "----[Tree's format :";
        public const string CheckNumber = "{Check Number}";
        public const string StubRetentionNodeName = "Node Name";
        public const string StubRetentionNodeLevel = "Node Level";
        public const string StubRetentionRN = " ]---\r\n";
        public const string RN = "\r\n";

        public const string Space = " ";
        public const string And = "and";
        public const string AndMark = "&";
        public const string AndMark2 = "&&";
        public const string Or = "or";
        public const string OrMark = "|";
        public const string OrMark2 = "||";
        public const string Star = "*";
        public const string QuestionMark = "?";
        public const string PointStart = ".*";
        public const string PointQuestionMark = ".?";
        public const string SpecificMark1 = "$";
        public const string SpecificMark2 = "^";

        public const string CompilerVersion = "CompilerVersion";
        public const string V4 = "v4.0";
        public const string SOUtilityExecuteCSharpCode = "ExecuteCSharpCode";
        public const string ExecuteCode = "ExecuteCode";
        public const string DetailInfoNA = "N/A";

        public const string Statistics = "Statistics";
        public const string BackupStatistics = "Backup Statistics";
        public const string StatisticsForExport = "StatisticsForExport";
        public const string StatisticsForBackup = "StatisticsForBackup";
        public const string StatisticsForDeletion = "StatisticsForDeletion";
        public const string StatisticsForRecordManager = "StatisticsForRecordManager";
        public const string StatisticsForTag = "StatisticsForTag";
        public const string DataSize = "DataSize";
        public const string Operation_Delete = "Delete";
        public const string Operation_DeleteOnly = "DeleteOnly";
        public const string Operation_DeleteOnlyAndKeepVersion = "DeleteOnlyAndKeepVersion";
        public const string Operation_DeleteOnlyAndDoesNotKeepVersion = "DeleteOnlyAndDoesNotKeepVersion";
        public const string Operation_Move = "Move";
        public const string Operation_PhysicalDelete = "PhysicalDelete";
        public const string Operation_Pending = "Pending";
        public const string Operation_Keep = "Keep";
        public const string LeaveLinkInSharePoint = "LeaveLinkInSharePoint";
        public const string Operation_Archive = "Archive";
        public const string Operation_ArchiveLeaveStub = "ArchiveLeaveStub";
        public const string MarkFirstSubJob = "_000";
        public const string A0 = "A0";
        public const string SPAuditId = "a5d4c9f5-55fa-4cce-b089-9990d2bb7921";
        public const string ODAuditId = "b6ed868a-b0e3-4ee9-a23e-395634b32715";
        public const string TEAMSAuditId = "3f7a9b2c-8e4d-4a1b-9c5f-6d2e8a0b1f3e";
        public const string GGAuditId = "37f41fae-237e-488b-895c-5be9b97d58da";
    }
}
