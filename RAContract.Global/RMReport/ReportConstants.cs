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
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.Contract.RMReport
{
    public class ReportConstants
    {
        public const string REPORT = "Rport";
        public const string ReportDETAIL = "ReportDetail";
        public const string CREATE_TABLE_BCS_TERM_USAGE_REPORT = @"
Create table {0} (ID integer primary key autoincrement,ObjectLevel int,TitleOrName nvarchar (500),Url nvarchar (500),
BCSTermId nvarchar (500),BCSTermName nvarchar (500),CreatedBy nvarchar (500),CreatedTime bigint,LastModifiedBy nvarchar (500),LastModifiedTime bigint,SPWebTimeZoneID nvarchar (500),TermStatus nvarchar (500),BCSTermFullPath nvarchar (500),LifecycleStatus nvarchar (500),CurrentHeldBy nvarchar (500),Box nvarchar (500),HomeLocation nvarchar (500),Availablity nvarchar (500))";
        public const string CREATE_TABLE_DUE_DISPOSAL_REPORT = @"
Create table {0} (ID integer primary key autoincrement,ObjectLevel int,TitleOrName nvarchar (500),SiteCollectionTitle nvarchar (500),Url nvarchar (500),
BCSTermId nvarchar (500),BCSTermName nvarchar (500),AppliedRuleId nvarchar (500),AppliedRuleName nvarchar (500),DisposalAction int
,CreatedBy nvarchar (500),CreatedTime bigint,LastModifiedBy nvarchar (500),LastModifiedTime bigint,SPWebTimeZoneID nvarchar (500),ManualApproval int,ExportType int,Status int,Comment nvarchar (500),LifecycleStatus nvarchar (500),CurrentHeldBy nvarchar (500),Box nvarchar (500),HomeLocation nvarchar (500),Availablity nvarchar (500),RelatedRecords nvarchar,RelatedRecordsAction int,DisposalClass nvarchar (500))";

        public const string CREATE_TABLE_DUE_TIMEFRAME_REPORT = @"
Create table {0} (ID integer primary key autoincrement,ObjectLevel int,TitleOrName nvarchar (500),OperationTime nvarchar (500),OperationBy nvarchar (500),
BCSTermName nvarchar (500),Url nvarchar (500),LifecycleStatus nvarchar (500),HomeLocation nvarchar (500),Box nvarchar (500),Availablity nvarchar (500),CurrentHeldBy nvarchar (500),Operation int,DisposalClass nvarchar (500),ApprovedBy nvarchar (500),ApprovedByUPN nvarchar (500), CreatedTime bigint, LastModifiedTime bigint, FileType nvarchar (500), RecordsId nvarchar (500), RuleName nvarchar (500), ApprovalStatus int, InternalApprovedStatus int)";

        public const string CREATE_TABLE_Client_Audit_REPORT = @"
CREATE TABLE {0}(ID integer primary key autoincrement, ObjectLevel int,TitleOrName nvarchar (500),UserName nvarchar(256),EventTypeName nvarchar(128),EventTypeI18NName nvarchar(500),Occurred bigint,Url nvarchar(1024),SiteUrl nvarchar(256),Event int,DisplayName nvarchar(256),Browser nvarchar(256));"
            + " CREATE INDEX IX_SiteUrl ON {0}(SiteUrl);"
            + " CREATE INDEX IX_Occurred ON {0}(Occurred);"
            + " CREATE INDEX IX_Event ON {0}(Event);";

        public const string INSERT_DATA_BCS_TERM_USAGE_REPORT = @"
Insert into {0} (ObjectLevel,TitleOrName,Url,BCSTermId,BCSTermName,CreatedBy,CreatedTime,LastModifiedBy,LastModifiedTime,SPWebTimeZoneID,TermStatus,BCSTermFullPath,LifecycleStatus,CurrentHeldBy,Box,HomeLocation,Availablity)
 Values (@ObjectLevel,@TitleOrName,@Url,@BCSTermId,@BCSTermName,@CreatedBy,@CreatedTime,@LastModifiedBy,@LastModifiedTime,@SPWebTimeZoneID,@TermStatus,@BCSTermFullPath,@LifecycleStatus,@CurrentHeldBy,@Box,@HomeLocation,@Availablity)";
        public const string INSERT_DATA_DUE_DISPOSAL_REPORT = @"
Insert into {0} (ObjectLevel,TitleOrName,SiteCollectionTitle,Url,BCSTermId,BCSTermName,AppliedRuleId,AppliedRuleName,DisposalAction,CreatedBy,CreatedTime,LastModifiedBy,LastModifiedTime,SPWebTimeZoneID,ManualApproval,ExportType,Status,Comment,LifecycleStatus,CurrentHeldBy,Box,HomeLocation,Availablity,RelatedRecords,RelatedRecordsAction,DisposalClass)
 Values (@ObjectLevel,@TitleOrName,@SiteCollectionTitle,@Url,@BCSTermId,@BCSTermName,@AppliedRuleId,@AppliedRuleName,@DisposalAction,@CreatedBy,@CreatedTime,@LastModifiedBy,@LastModifiedTime,@SPWebTimeZoneID,@ManualApproval,@ExportType,@Status,@Comment,@LifecycleStatus,@CurrentHeldBy,@Box,@HomeLocation,@Availablity,@RelatedRecords,@RelatedRecordsAction,@DisposalClass)";
        public const string INSERT_DATA_DUE_TIMEFRAME_REPORT = @"
Insert into {0} (ObjectLevel,TitleOrName,OperationTime,OperationBy,BCSTermName,Url,LifecycleStatus,HomeLocation,Box,Availablity,CurrentHeldBy,Operation,DisposalClass,ApprovedBy,ApprovedByUPN,CreatedTime,LastModifiedTime,FileType,RecordsId,RuleName,ApprovalStatus,InternalApprovedStatus)
 Values (@ObjectLevel,@TitleOrName,@OperationTime,@OperationBy,@BCSTermName,@Url,@LifecycleStatus,@HomeLocation,@Box,@Availablity,@CurrentHeldBy,@Operation,@DisposalClass,@ApprovedBy,@ApprovedByUPN,@CreatedTime,@LastModifiedTime,@FileType,@RecordsId,@RuleName,@ApprovalStatus,@InternalApprovedStatus)";
        public const string INSERT_DATA_Client_Audit_REPORT = @"
INSERT INTO {0}
(ObjectLevel,TitleOrName,Url,UserName,EventTypeName,EventTypeI18NName,Occurred,SiteUrl,Event,DisplayName,Browser)
VALUES 
(@ObjectLevel,@TitleOrName,@Url,@UserName,@EventTypeName,@EventTypeI18NName,@Occurred,@SiteUrl,@Event,@DisplayName,@Browser)";

        public const string CREATE_ARCHIVED_SITE_REPORT_TABLE = @"
CREATE TABLE {0}(ID integer primary key autoincrement, ObjectLevel int, TitleOrName nvarchar(500), Url nvarchar(1024), Type nvarchar(128), SourceUrl nvarchar(1024), ArchivedDataSize float, CreatedTime bigint, LastModifiedTime bigint, ArchivedTime bigint);";
        public const string INSERT_ARCHIVED_SITE_REPORT = @"
INSERT INTO {0} (ObjectLevel,TitleOrName,Url,Type,SourceUrl,ArchivedDataSize,CreatedTime,LastModifiedTime,ArchivedTime)
VALUES (@ObjectLevel,@TitleOrName,@Url,@Type,@SourceUrl,@ArchivedDataSize,@CreatedTime,@LastModifiedTime,@ArchivedTime)";

        public const string SELECT_DATA_FROM_TABLE = @"select * from {0} limit {1} offset {2}";
        public const string SELECT_DATA_ORDERBY_FROM_TABLE = @"select * from {0} order by {1} {2} limit {3} offset {4}";
        public const string SELECT_DATA_ORDERBY_LOWER_FROM_TABLE = @"select * from {0} order by LOWER({1}) {2} limit {3} offset {4}";
        public const string SELECT_DATA_ORDERBY_CAST_INT_FROM_TABLE = @"select * from {0} order by CAST({1} AS INTEGER) {2} limit {3} offset {4}";
        public const string SELECT_DATA_ON_CONDITION_FROM_TABLE = @"select * from {0} where {1} limit {2} offset {3}";
        public const string SELECT_DATA_ON_CONDITION_ORDERBY_FROM_TABLE = @"select * from {0} where {1} order by {2} {3} limit {4} offset {5}";
        public const string SELECT_REPORT_COUNT_SQL = "select count(*) from {0}";
        public const string SELECT_REPORT_COUNT_ON_CONDITION_SQL = "select count(*) from {0} where {1}";

        public const string CREATE_TABLE_PRM_AVAILABLE_SPACE_REPORT = @"
Create table {0} (ID integer primary key autoincrement,Location nvarchar (500),AvailableSpace float,
LocationSize float,InculdingContainerInfo nvarchar (4000))";
        public const string INSERT_DATA_PRM_AVAILABLE_SPACE_REPORT = @"
Insert into {0} (Location,AvailableSpace,LocationSize,InculdingContainerInfo)
 Values (@Location,@AvailableSpace,@LocationSize,@InculdingContainerInfo)";
        public const string INSERT_DATA_INTO_RESTORE_REPORT_TABLE_SQL = "Insert into {0} (ObjectLevel,TitleOrName,SourceURL,Size,JobId,StartTime,FinishTime,RestoreBy,RestoreTo,IsDaoMigration,IsEndUserOpt,Status,Comment) Values (@ObjectLevel,@TitleOrName,@SourceURL,@Size,@JobId,@StartTime,@FinishTime,@RestoreBy,@RestoreTo,@IsDaoMigration,@IsEndUserOpt,@Status,@Comment)";
        public const string CREATE_RESTORE_REPORT_TABLE_SQL = @"Create table {0} (ID integer primary key autoincrement,ObjectLevel nvarchar (500),TitleOrName nvarchar (500),SourceURL nvarchar (500),Size integer,JobId nvarchar (500),StartTime integer,FinishTime integer,RestoreBy nvarchar (500),RestoreTo nvarchar (500),IsDaoMigration nvarchar (500),ISEndUserOpt nvarchar (500),Status nvarchar (500),Comment nvarchar (500))";
    }
}
