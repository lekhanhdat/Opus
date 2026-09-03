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
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.Contract.JobMonitor
{
    public class JobMonitorConstants
    {
        public const int MAX_COUNT_ONE_SHEET = 65535;
        public const long MAX_ROWS_PER_FILE = MAX_COUNT_ONE_SHEET * 20;
        public const string ZIP = ".zip";

        public const int MAX_ROWS_PER_RPT_FILE = 1_000_000;
        public const string REPORT_EXTENSION = ".rpt";

        public const string JOB_REPORT_FOLDER = "Job Report";

        public const string REPORT_TEMPLE_FOLDER = "Temple";

        public const string RESTORE_REPORT_SC_FOLDER = "Restore Report";
        public const string RESTORE_REPORT_GD_FOLDER = "Google Restore Report";
        public const string GENERATE_RESTORE_REPORT = "Has Generated Restore Report";

        public const string JOBDETAIL = "JobDetail";

        public const string GET_TABLE_ALL_COL = "SELECT name FROM PRAGMA_table_info('{0}');";

        public const string GET_COUNT_OF_TABLE = "SELECT count(*) FROM {0};";

        public const string GET_TABLE_AND_INDEX_DEFINE = "SELECT * FROM sqlite_master WHERE type IN ('table','index') AND tbl_name LIKE '{0}';";

        public const string GET_TABLE_DEFINE = "SELECT * FROM sqlite_master WHERE type = 'table' AND tbl_name LIKE '{0}';";

        public const string PAGE_GET_DATA = "SELECT * FROM {0} limit {1} offset {2}";

        public const string Row_ID_COLUMN = "RowID";

        public const string SUMMARY_STATISTICS_COLUMN = "Statistics";

        public const string SUB_JOB_ID_COLUMN = "SubJobID";
        public const string STATUS_COLUMN = "Status";
        public const string SCOPE_COLUMN = "Scope";
        public const string SUCCESSFUL_COLUMN = "Successful";
        public const string FAILED_COLUMN = "Failed";
        public const string SKIPPED_COLUMN = "Skipped";
        public const string COMMENT_COLUMN = "Comment";

        public const string CREATE_TABLE_WITH_DEFINITION = "CREATE TABLE {0} ({1});";

        public const string PAGE_GET_DATA_BY_CURSOR = "SELECT RowID AS RowID, * FROM {0} WHERE RowID > {1} ORDER BY RowID LIMIT {2}";
        public const string PAGE_GET_DATA_BY_CURSOR_AND_CONDITION = "SELECT RowID AS RowID, * FROM {0} WHERE {1} AND RowID > {2} ORDER BY RowID LIMIT {3}";

        public const string CREATE_TABLE_TERM_USAGE_REPORT = @"Create table {0} (ID integer primary key autoincrement,Type nvarchar (500),TitleOrName nvarchar (500),
            Url nvarchar (500),Status nvarchar (500),Comment nvarchar (500))";
        public const string INSERT_DATA_TERM_USAGE_REPORT = "Insert into {0} (Type,TitleOrName,Url,Status,Comment) Values (@Type,@TitleOrName,@Url,@Status,@Comment)";

        public const string TERMSELECTION = "TermSelection";
        public const string CREATE_TABLE_TERM_SELECTION = @"Create table {0} (ID integer primary key autoincrement,Term nvarchar (500),TermFullPath nvarchar (500))";
        public const string INSERT_DATA_TERM_SELECTION = "Insert into {0} (Term,TermFullPath) Values (@Term,@TermFullPath)";

        public const string CREATE_TABLE_TERM_SYNCHRONIZATION = @"Create table {0} (ID integer primary key autoincrement,Term nvarchar (500),Action nvarchar (500),
            MMSApplication nvarchar (500),AgentName nvarchar (500),Status nvarchar (500),Comment nvarchar (500))";
        public const string INSERT_DATA_TERM_SYNCHRONIZATION = "Insert into {0} (Term,Action,MMSApplication,AgentName,Status,Comment) Values (@Term,@Action,@MMSApplication,@AgentName,@Status,@Comment)";

        public const string CREATE_TABLE_ITEMS_DUE_DISPOSAL = @"Create table {0} (ID integer primary key autoincrement,Type nvarchar (500),TitleOrName nvarchar (500),
            Url nvarchar (500),Status nvarchar (500),Comment nvarchar (500))";
        public const string INSERT_DATA_ITEMS_DUE_DISPOSAL = "Insert into {0} (Type,TitleOrName,Url,Status,Comment) Values (@Type,@TitleOrName,@Url,@Status,@Comment)";

        public const string CREATE_TABLE_GLOBAL_SETTING = @"Create table {0} (ID integer primary key autoincrement,ObjectName nvarchar (500),SourceURL nvarchar (500),AgentName nvarchar (500),
            ColumnName nvarchar (500),Action nvarchar (500),Status nvarchar (500),Comment nvarchar (500),Classification nvarchar (500))";
        public const string INSERT_DATA_GLOBAL_SETTING = "Insert into {0} (ObjectName,SourceURL,ColumnName,Action,AgentName,Status,Comment,Classification) Values (@ObjectName,@SourceURL,@ColumnName,@Action,@AgentName,@Status,@Comment,@Classification)";

        public const string CREATE_TABLE_ITEMS_PHYSICAL_SYNCHRONIZATION = @"Create table {0} (ID integer primary key autoincrement,TermName nvarchar (500),LocationPath nvarchar (500),SiteCollectionURL nvarchar (500),Action nvarchar (500),Status nvarchar (500),Comment nvarchar (500))";
        public const string INSERT_DATA_ITEMS_PHYSICAL_SYNCHRONIZATION = "Insert into {0} (TermName,LocationPath,SiteCollectionURL,Action,Status,Comment) Values (@TermName,@LocationPath,@SiteCollectionURL,@Action,@Status,@Comment)";

        public const string CREATE_TABLE_ITEMS_MANUALAPPROVAL = @"Create table {0} (ID integer primary key autoincrement,ObjectLevel nvarchar (500),TitleOrName nvarchar (500),Url nvarchar (500),ApprovalStatus nvarchar (500),Action nvarchar (500),RecordOwner nvarchar (500),RuleCriteria nvarchar (500),Status nvarchar (500),Comment nvarchar (500))";
        public const string INSERT_DATA_ITEMS_MANUALAPPROVAL = "Insert into {0} (ObjectLevel,TitleOrName,Url,ApprovalStatus,Action,RecordOwner,RuleCriteria,Status,Comment) Values (@ObjectLevel,@TitleOrName,@Url,@ApprovalStatus,@Action,@RecordOwner,@RuleCriteria,@Status,@Comment)";

        public const string CREATE_TABLE_ITEMS_PHYSICALTEMPLATEIMPORT = @"Create table {0} (ID integer primary key autoincrement,TemplateSuiteName nvarchar (500),TemplateSuiteStartFrom nvarchar (500),TemplateName nvarchar (500),TemplateType nvarchar (500),TemplatePrefix nvarchar (500),TemplateDigits nvarchar (500), Status nvarchar (500),Comment nvarchar (500))";
        public const string INSERT_DATA_ITEMS_PHYSICALTEMPLATEIMPORT = "Insert into {0} (TemplateSuiteName,TemplateSuiteStartFrom,TemplateName,TemplateType,TemplatePrefix,TemplateDigits,Status,Comment) Values (@TemplateSuiteName,@TemplateSuiteStartFrom,@TemplateName,@TemplateType,@TemplatePrefix,@TemplateDigits,@Status,@Comment)";

        public const string CREATE_TABLE_ITEMS_DOWNLOADJOBDETAIL = @"Create table {0} (ID integer primary key autoincrement,JobId nvarchar (500),Status nvarchar (500),Comment nvarchar (500))";
        public const string INSERT_DATA_ITEMS_DOWNLOADJOBDETAIL = "Insert into {0} (JobId,Status,Comment) Values (@JobId,@Status,@Comment)";

        public const string CREATE_TABLE_ITEMS_PHYSICAL_UPDATELOCATION = @"Create table {0} (ID integer primary key autoincrement,SiteCollectionURL nvarchar (500),ItemType nvarchar (500),SourceUrl nvarchar (500),DestinationUrl nvarchar (500),Status nvarchar (500),Comment nvarchar (500))";
        public const string INSERT_DATA_ITEMS_PHYSICAL_UPDATELOCATION = "Insert into {0} (SiteCollectionURL ,ItemType ,SourceUrl ,DestinationUrl ,Status,Comment) Values (@SiteCollectionURL,@ItemType,@SourceUrl,@DestinationUrl,@Status,@Comment)";

        public const string CREATE_TABLE_ITEMS_PHYSICAL_IMPORTPHYSICALRECORDS = @"Create table {0} (ID integer primary key autoincrement,SrcRecordType nvarchar (500),DestRecordType nvarchar (500),TemplateName nvarchar (500),UniqueId nvarchar (500),Barcode nvarchar (500),Title nvarchar (2000),Container nvarchar (500),SrcLocation nvarchar (500),LocationFullPath nvarchar (500),Status nvarchar (500),Comment nvarchar (500))";
        public const string INSERT_DATA_ITEMS_PHYSICAL_IMPORTPHYSICALRECORDS = "Insert into {0} (SrcRecordType ,DestRecordType ,TemplateName ,UniqueId ,Barcode ,Title ,Container,SrcLocation,LocationFullPath,Status,Comment) Values (@SrcRecordType,@DestRecordType,@TemplateName,@UniqueId,@Barcode,@Title,@Container,@SrcLocation,@LocationFullPath,@Status,@Comment)";

        public const string CREATE_TABLE_ITEMS_DELETION_IMPORTPHYSICALRECORDS = @"Create table {0} (ID integer primary key autoincrement,ObjectName nvarchar (2000),UniqueId nvarchar (500),Status nvarchar (500),Comment nvarchar (500))";
        public const string INSERT_DATA_ITEMS_DELETION_IMPORTPHYSICALRECORDS = "Insert into {0} (ObjectName ,UniqueId ,Status,Comment) Values (@ObjectName,@UniqueId,@Status,@Comment)";

        public const string CREATE_TABLE_ITEMS_PHYSICAL_IMPORTRECORDSRELATED = @"Create table {0} (ID integer primary key autoincrement, SrcId nvarchar (500),SrcType nvarchar (500), SrcName nvarchar (500),SrcLocation nvarchar (500),SrcSiteId nvarchar (500),SrcItemId nvarchar (500),SrcItemUrl nvarchar (500),DestName nvarchar (500),DestType nvarchar (500),DestItemId nvarchar (500),DestItemUrl nvarchar (500),DestSiteId nvarchar (500),DestSiteUrl nvarchar (500),Status nvarchar (500),Comment nvarchar (500))";
        public const string INSERT_DATA_ITEMS_PHYSICAL_IMPORTRECORDSRELATED = "Insert into {0} (SrcId,SrcType, SrcName,SrcLocation,SrcSiteId,SrcItemId,SrcItemUrl,DestName,DestType,DestItemId,DestItemUrl,DestSiteId,DestSiteUrl,Status,Comment) Values (@SrcId,@SrcType,@SrcName,@SrcLocation,@SrcSiteId,@SrcItemId,@SrcItemUrl,@DestName,@DestType,@DestItemId,@DestItemUrl,@DestSiteId,@DestSiteUrl,@Status,@Comment)";

        public const string CREATE_TABLE_ITEMS_AVAILABLE_SPACE_REPORT = @"Create table {0} (ID integer primary key autoincrement,Location nvarchar (500),Status nvarchar (500),Comment nvarchar (500),LocationSize varchar (20))";
        public const string INSERT_DATA_ITEMS_AVAILABLE_SPACE_REPORT = "Insert into {0} (Location,Status,Comment,LocationSize) Values (@Location,@Status,@Comment,@LocationSize)";

        public const string CREATE_TABLE_ITEMS_TIMEFRAME_REPORT = @"Create table {0} (ID integer primary key autoincrement,ObjectLevel nvarchar (500),Title nvarchar (500),TermName nvarchar (500),Url nvarchar (500),Status nvarchar (500),Comment nvarchar (500))";
        public const string INSERT_DATA_ITEMS_TIMEFRAME_REPORT = "Insert into {0} (ObjectLevel,Title,TermName,Url,Status,Comment) Values (@ObjectLevel,@Title,@TermName,@Url,@Status,@Comment)";

        public const string CREATE_TABLE_TERM_IMPORT = @"Create table {0} (ID integer primary key autoincrement,Term nvarchar (500),Action nvarchar (500),Status nvarchar (500),Comment nvarchar (500))";
        public const string INSERT_DATA_TERM_IMPORT = "Insert into {0} (Term,Action,Status,Comment) Values (@Term,@Action,@Status,@Comment)";

        public const string CREATE_TABLE_Unique_ID_SETTING = @"Create table {0} (ID integer primary key autoincrement,ObjectName nvarchar (500),SourceURL nvarchar (500),
            ColumnName nvarchar (500),Action nvarchar (500),AgentName nvarchar (500),Status nvarchar (500),Comment nvarchar (500),UniqueID nvarchar (500))";
        public const string INSERT_DATA_Unique_ID_SETTING = "Insert into {0} (ObjectName,SourceURL,ColumnName,Action,AgentName,Status,Comment,UniqueID) Values (@ObjectName,@SourceURL,@ColumnName,@Action,@AgentName,@Status,@Comment,@UniqueID)";

        public const string CREATE_TABLE_COLLECTION_DATA_SETTING = @"Create table {0} (ID integer primary key autoincrement,ObjectName nvarchar (500),FullPath nvarchar (500),AgentName nvarchar (500),
            Status nvarchar (500),Comment nvarchar (500))";
        public const string INSERT_DATA_COLLECTION_DATA_SETTING = "Insert into {0} (ObjectName,FullPath,AgentName,Status,Comment) Values (@ObjectName,@FullPath,@AgentName,@Status,@Comment)";

        public const string CREATE_TABLE_SYNC_SECURITY_CONTAINER_SETTING = @"Create table {0} (ID integer primary key autoincrement,Container nvarchar (500),ObjectName nvarchar (500),FullPath nvarchar (500),
            Status nvarchar (500),Comment nvarchar (500))";
        public const string INSERT_DATA__SYNC_SECURITY_CONTAINER_SETTING = "Insert into {0} (Container,ObjectName,FullPath,Status,Comment) Values (@Container,@ObjectName,@FullPath,@Status,@Comment)";

        public const string CREATE_TABLE_ENFORCE_RETENTION = @"Create table {0} (ID integer primary key autoincrement,ObjectName nvarchar (500),FullPath nvarchar (500),Action nvarchar (500),
            Status nvarchar (500),Comment nvarchar (500))";
        public const string INSERT_DATA_ENFORCE_RETENTION = "Insert into {0} (ObjectName,FullPath,Action,Status,Comment) Values (@ObjectName,@FullPath,@Action,@Status,@Comment)";

        public const string CREATE_TABLE_ITEMS_EXPLORER_MOVE = @"Create table {0} (ID integer primary key autoincrement,ObjectName nvarchar (500),ItemType nvarchar(64), FullPath nvarchar (500),DestinationFullPath nvarchar (500),
            Status nvarchar (500),Comment nvarchar (500))";
        public const string INSERT_DATA_ITEMS_EXPLORER_MOVE = "Insert into {0} (ObjectName,ItemType,FullPath,DestinationFullPath,Status,Comment) Values (@ObjectName,@ItemType,@FullPath,@DestinationFullPath,@Status,@Comment)";

        public const string CREATE_TABLE_ITEMS_EXO_ApplySetting = @"Create table {0} (ID integer primary key autoincrement,ObjectName nvarchar (500),ItemType nvarchar(64), FullPath nvarchar (500),Action nvarchar (500),
            Status nvarchar (500),Comment nvarchar (500), Classification nvarchar(500))";
        public const string INSERT_DATA_ITEMS_EXO_ApplySetting = "Insert into {0} (ObjectName,ItemType,FullPath,Action,Status,Comment, Classification) Values (@ObjectName,@ItemType,@FullPath,@Action,@Status,@Comment,@Classification)";

        public const string CREATE_TABLE_ITEMS_SP_ImportSetting = @"Create table {0} (ID integer primary key autoincrement, ObjectName nvarchar (500),Url nvarchar (2000),Status nvarchar (500),Comment nvarchar (2000))";
        public const string INSERT_DATA_ITEMS_SP_ImportSetting = "Insert into {0} (ObjectName,Url,Status,Comment) Values (@ObjectName, @Url,@Status,@Comment)";

        public const string CREATE_TABLE_ITEMS_EXO_DataSync = @"Create table {0} (ID integer primary key autoincrement,ObjectName nvarchar (500),ItemType nvarchar(64), FullPath nvarchar (500),
            Status nvarchar (500),Comment nvarchar (500))";
        public const string INSERT_DATA_ITEMS_EXO_DataSync = "Insert into {0} (ObjectName,ItemType,FullPath,Status,Comment) Values (@ObjectName,@ItemType,@FullPath,@Status,@Comment)";

        public const string CREATE_TABLE_ITEMS_EXO_Disposal = @"Create table {0} (ID integer primary key autoincrement,RuleName nvarchar (500),Action nvarchar (500), ObjectName nvarchar (500),ItemType nvarchar(64), FullPath nvarchar (500), DestinationUrl nvarchar (500),
            Status nvarchar (500),Comment nvarchar (500))";
        public const string INSERT_DATA_ITEMS_EXO_Disposal = "Insert into {0} (Action,ObjectName,ItemType,RuleName,FullPath,DestinationUrl,Status,Comment) Values (@Action,@ObjectName,@ItemType,@RuleName,@FullPath,@DestinationUrl,@Status,@Comment)";

        public const string CREATE_TABLE_PHYSICAL_Disposal = @"Create table {0} (ID integer primary key autoincrement,ObjectName nvarchar (500),ItemType nvarchar(64),RuleName nvarchar (500),ActionType nvarchar(64),FullPath nvarchar (500)
            ,DestinationPath nvarchar (500), Status nvarchar (500),Comment nvarchar (500))";
        public const string INSERT_DATA_PHYSICAL_Disposal = "Insert into {0} (ObjectName,ItemType,RuleName,ActionType,FullPath,DestinationPath,Status,Comment) Values (@ObjectName,@ItemType,@RuleName,@ActionType,@FullPath,@DestinationPath,@Status,@Comment)";
        
        public const string CREATE_TABLE_PHYSICAL_MoveJob = @"Create table {0} (ID integer primary key autoincrement,UniqueId nvarchar (500),ObjectName nvarchar (500),ItemType nvarchar(64), Status nvarchar (500),Comment nvarchar (500))";
        public const string INSERT_DATA_PHYSICAL_MoveJob = "Insert into {0} (UniqueId,ObjectName,ItemType,Status,Comment) Values (@UniqueId,@ObjectName,@ItemType,@Status,@Comment)";

        public const string CREATE_TABLE_PHYSICAL_ExplorerTimer = @"Create table {0} (ID integer primary key autoincrement,ObjectName nvarchar (500),ItemType nvarchar(64),FullPath nvarchar (500)
            ,RuleName nvarchar (500), Status nvarchar (500),Comment nvarchar (500))";
        public const string INSERT_DATA_PHYSICAL_ExplorerTimer = "Insert into {0} (ObjectName,ItemType,FullPath,RuleName,Status,Comment) Values (@ObjectName,@ItemType,@FullPath,@RuleName,@Status,@Comment)";

        public const string CREATE_TABLE_CONNECTOR_ExplorerTimer = @"Create table {0} (ID integer primary key autoincrement,ObjectName nvarchar (500),TermName nvarchar(500),FullPath nvarchar (500)
            ,RuleName nvarchar (500), Status nvarchar (500),Comment nvarchar (500))";
        public const string INSERT_DATA_CONNECTOR_ExplorerTimer = "Insert into {0} (ObjectName,TermName,FullPath,RuleName,Status,Comment) Values (@ObjectName,@TermName,@FullPath,@RuleName,@Status,@Comment)";

        public const string CREATE_TABLE_PHYSICAL_ExportBarcode = @"Create table {0} (ID integer primary key autoincrement,ObjectName nvarchar (500),ItemType nvarchar(64),FullPath nvarchar (500)
            , Status nvarchar (500),Comment nvarchar (500))";
        public const string INSERT_DATA_PHYSICAL_ExportBarcode = "Insert into {0} (ObjectName,ItemType,FullPath,Status,Comment) Values (@ObjectName,@ItemType,@FullPath,@Status,@Comment)";

        public const string CREATE_TABLE_HOLDS_RECORDS_EXPORT = @"Create table {0} (ID integer primary key autoincrement,ObjectName nvarchar (500),ItemType nvarchar(64),FullPath nvarchar (500)
            , Status nvarchar (500),Comment nvarchar (500))";
        public const string INSERT_DATA_HOLDS_RECORDS_EXPORT = "Insert into {0} (ObjectName,ItemType,FullPath,Status,Comment) Values (@ObjectName,@ItemType,@FullPath,@Status,@Comment)";

        public const string CREATE_TABLE_PHYSICAL_SetPermission = @"Create table {0} (ID integer primary key autoincrement,ObjectName nvarchar (500),ItemType nvarchar(64),FullPath nvarchar (500)
            , Status nvarchar (500),Comment nvarchar (500))";
        public const string INSERT_DATA_PHYSICAL_SetPermission = "Insert into {0} (ObjectName,ItemType,FullPath,Status,Comment) Values (@ObjectName,@ItemType,@FullPath,@Status,@Comment)";

        public const string SELECT_DATA_FROM_TABLE = @"select * from {0} limit {1} offset {2}";
        public const string SELECT_DATA_ON_CONDITION_FROM_TABLE = @"select * from {0} where {1} limit {2} offset {3}";
        public const string SELECT_DETAIL_COUNT_SQL = "select count(*) from {0}";
        public const string SELECT_DETAIL_COUNT_ON_CONDITION_SQL = "select count(*) from {0} where {1}";

        public const string SELECT_DATA_FROM_TABLE_ORDERBY_CONDITONSTR = @"select * from {0} order by {1} limit {2} offset {3}";
        public const string SELECT_DATA_ON_CONDITION_FROM_TABLE_ORDERBY_CONDITONSTR = @"select * from {0} where {1} order by {2} limit {3} offset {4}";

        public const string CREATE_TABLE_ACTIONONLY = @"Create table {0} (ID integer primary key autoincrement,ObjectName nvarchar (500),Url nvarchar (500),RuleName nvarchar (500),
            Status nvarchar (500),Comment nvarchar (500))";
        public const string INSERT_ACTIONONLY = "Insert into {0} (ObjectName,Url,RuleName,Status,Comment) Values (@ObjectName,@Url,@RuleName,@Status,@Comment)";

        public const string CREATE_TABLE_Dashboard = "Create table {0} (ID integer primary key autoincrement,Action nvarchar (500), SourceFlag nvarchar (500), Status nvarchar (500),Comment nvarchar (500))";
        public const string INSERT_DATA_Dashboard = "Insert into {0} (Action,SourceFlag,Status,Comment) Values (@Action,@SourceFlag,@Status,@Comment)";

        public const string CREATE_TABLE_AzureFileShareDataSync = "Create table {0} (ID integer primary key autoincrement,ObjectName nvarchar (500), FullPath nvarchar (500), NodeType nvarchar (500),Status nvarchar (500),Comment nvarchar (500))";
        public const string INSERT_DATA_AzureFileShareDataSync = "Insert into {0} (ObjectName,FullPath,NodeType,Status,Comment) Values (@ObjectName,@FullPath,@NodeType,@Status,@Comment)";

        public const string CREATE_TABLE_BoxDataSync = "Create table {0} (ID integer primary key autoincrement,ObjectName nvarchar (500), FullPath nvarchar (500), NodeType nvarchar (500),Status nvarchar (500),Comment nvarchar (500))";
        public const string INSERT_DATA_BoxDataSync = "Insert into {0} (ObjectName,FullPath,NodeType,Status,Comment) Values (@ObjectName,@FullPath,@NodeType,@Status,@Comment)";

        public const string CREATE_TABLE_Box_Disposal_Report = @"Create table {0} (ID integer primary key autoincrement,ActionTab nvarchar (50),Level nvarchar (50),SourceLocation nvarchar (500),DestinationLocation nvarchar (500),Action nvarchar (500),Size nvarchar(500),FinishTime nvarchar (100),Status nvarchar (500),Comment nvarchar (500),RuleName nvarchar (500))";
        public const string INSERT_DATA_Box_Disposal_Report = "Insert into {0} (ActionTab,Level,SourceLocation,DestinationLocation,Action,Size,FinishTime,Status,Comment,RuleName) Values (@ActionTab,@Level,@SourceLocation,@DestinationLocation,@Action,@Size,@FinishTime,@Status,@Comment,@RuleName)";

        public const string CREATE_TABLE_Box_Disposal_SUMMARYReport = @"Create table {0} (ID integer primary key autoincrement,Statistics nvarchar (5000))";
        public const string INSERT_DATA_Box_Disposal_SUMMARYReport = "Insert into {0} (Statistics) Values (@Statistics)";

        public const string CREATE_TABLE_GoogleApplySetting = "Create table {0} (ID integer primary key autoincrement,ObjectName nvarchar (500), FullPath nvarchar (500), Classification nvarchar (500), FileSize nvarchar (500), NodeType nvarchar (500),Status nvarchar (500),Comment nvarchar (500), Action nvarchar (500))";
        public const string INSERT_DATA_GoogleApplySetting = "Insert into {0} (ObjectName,FullPath,Classification,FileSize,NodeType,Status,Comment,Action) Values (@ObjectName,@FullPath,@Classification,@FileSize,@NodeType,@Status,@Comment,@Action)";

        public const string CREATE_TABLE_SalesforceDiscovery = "Create table {0} (ID integer primary key autoincrement,ObjectName nvarchar (500), ObjectType nvarchar (50), TotalItemCount integer, TotalSize integer, TenantId nvarchar (500),Status nvarchar (500),Comment nvarchar (500))";
        public const string INSERT_DATA_SalesforceDiscovery = "Insert into {0} (ObjectName,ObjectType,TotalItemCount,TotalSize,TenantId,Status,Comment) Values (@ObjectName,@ObjectType,@TotalItemCount,@TotalSize,@TenantId,@Status,@Comment)";

        public const string CREATE_TABLE_Google_Disposal_Report = @"Create table {0} (ID integer primary key autoincrement,ActionTab nvarchar (50),Level nvarchar (50),SourceLocation nvarchar (500),DestinationLocation nvarchar (500),Action nvarchar (500),Size nvarchar(500),FinishTime nvarchar (100),Status nvarchar (500),Comment nvarchar (500),RuleName nvarchar (500))";
        public const string INSERT_DATA_Google_Disposal_Report = "Insert into {0} (ActionTab,Level,SourceLocation,DestinationLocation,Action,Size,FinishTime,Status,Comment,RuleName) Values (@ActionTab,@Level,@SourceLocation,@DestinationLocation,@Action,@Size,@FinishTime,@Status,@Comment,@RuleName)";

        public const string CREATE_TABLE_Google_Disposal_SUMMARYReport = @"Create table {0} (ID integer primary key autoincrement,Statistics nvarchar (5000))";
        public const string INSERT_DATA_Google_Disposal_SUMMARYReport = "Insert into {0} (Statistics) Values (@Statistics)";

        public const string CREATE_TABLE_TenantUpgrade = "Create table {0} (ID integer primary key autoincrement,UpgradeModule nvarchar (500), Status nvarchar (500),Comment nvarchar (500))";
        public const string INSERT_DATA_TenantUpgrade = "Insert into {0} (UpgradeModule,Status,Comment) Values (@UpgradeModule,@Status,@Comment)";

        public const string CREATE_TABLE_ManualApprovalEmailSchedule = "Create table {0} (ID integer primary key autoincrement,TitleOrName nvarchar (500), Action nvarchar (500), Status nvarchar (500),Comment nvarchar (500))";
        public const string INSERT_DATA_ManualApprovalEmailSchedule = "Insert into {0} (TitleOrName,Action,Status,Comment) Values (@TitleOrName,@Action,@Status,@Comment)";

        public const string CREATE_TABLE_PHYSICAL_FSDashBoard = @"Create table {0} (ID integer primary key autoincrement,Action nvarchar (500), Status nvarchar (500),Comment nvarchar (500))";
        public const string INSERT_DATA_PHYSICAL_FSDashBoard = "Insert into {0} (Action,Status,Comment) Values (@Action,@Status,@Comment)";

        public const string CREATE_TABLE_PHYSICAL_SPOnPremDashBoard = @"Create table {0} (ID integer primary key autoincrement,Action nvarchar (500), Status nvarchar (500),Comment nvarchar (500))";
        public const string INSERT_DATA_PHYSICAL_SPOnPremDashBoard = "Insert into {0} (Action,Status,Comment) Values (@Action,@Status,@Comment)";

        public const string CREATE_TABLE_FileSystem_Disposal = @"Create table {0} (ID integer primary key autoincrement,Type nvarchar (50),ObjectName nvarchar (500),Size nvarchar(500),SourceLocation nvarchar (500),DestinationLocation  nvarchar (500),FinishTime  nvarchar (100),RuleName  nvarchar (500),Action  nvarchar (500),AgentName  nvarchar (500),
            Status nvarchar (500),Comment nvarchar (500))";
        public const string INSERT_DATA_FileSystem_Disposal = "Insert into {0} (Type,ObjectName,Size,SourceLocation,DestinationLocation,FinishTime,RuleName,AgentName,Action,Status,Comment) Values (@Type,@ObjectName,@Size,@SourceLocation,@DestinationLocation,@FinishTime,@RuleName,@AgentName,@Action,@Status,@Comment)";

        public const string CREATE_TABLE_HOLD_RECORDS_IMPORT = @"Create table {0} (ID integer primary key autoincrement,Name nvarchar (50),Url nvarchar (500),HoldTitle nvarchar (50),Status nvarchar (50),Comment nvarchar (500))";
        public const string INSERT_DATA_HOLD_RECORDS_IMPORT = "Insert into {0} (Name,Url,HoldTitle,Status,Comment) Values (@Name,@Url,@HoldTitle,@Status,@Comment)";
        public const string CREATE_TABLE_WORKSPACE_HOLD_IMPORT = @"Create table {0} (ID integer primary key autoincrement,Url nvarchar (50),Type nvarchar (500),HoldTitle nvarchar (50),Status nvarchar (50),Comment nvarchar (500))";
        public const string INSERT_DATA_WORKSPACE_HOLD_IMPORT = "Insert into {0} (Url,Type,HoldTitle,Status,Comment) Values (@Url,@Type,@HoldTitle,@Status,@Comment)";
        public const string CREATE_TABLE_FileSystem_DisposalV2 = @"Create table {0} (ID integer primary key autoincrement,Type nvarchar (50),ObjectName nvarchar (500),Size nvarchar(500),SourceLocation nvarchar (500),DestinationLocation  nvarchar (500),FinishTime  nvarchar (100),RuleName  nvarchar (500),Action  nvarchar (500),AgentName  nvarchar (500),
            Status nvarchar (500),Comment nvarchar (500),Depth integer, DirPath nvarchar (500), DetailAction integer)";
        public const string INSERT_DATA_FileSystem_DisposalV2 = "Insert into {0} (Type,ObjectName,Size,SourceLocation,DestinationLocation,FinishTime,RuleName,AgentName,Action,Status,Comment,Depth,DirPath,DetailAction) Values (@Type,@ObjectName,@Size,@SourceLocation,@DestinationLocation,@FinishTime,@RuleName,@AgentName,@Action,@Status,@Comment,@Depth,@DirPath,@DetailAction)";

        public const string CREATE_TABLE_FileSystem_DataSync = @"Create table {0} (ID integer primary key autoincrement,ObjectName nvarchar (500),FullPath nvarchar (500),AgentName nvarchar (500),
            Status nvarchar (500),Comment nvarchar (500))";
        public const string INSERT_DATA_FileSystem_DataSync = "Insert into {0} (ObjectName,FullPath,AgentName,Status,Comment) Values (@ObjectName,@FullPath,@AgentName,@Status,@Comment)";

        public const string CREATE_TABLE_FileSystem_DataSyncV2 = @"Create table {0} (ID integer primary key autoincrement,ObjectName nvarchar (500),FullPath nvarchar (500),AgentName nvarchar (500),
            Status nvarchar (500),Comment nvarchar (500),Depth integer, DirPath nvarchar (500))";
        public const string INSERT_DATA_FileSystem_DataSyncV2 = "Insert into {0} (ObjectName,FullPath,AgentName,Status,Comment,Depth,DirPath) Values (@ObjectName,@FullPath,@AgentName,@Status,@Comment,@Depth,@DirPath)";

        public const string CREATE_TABLE_FileSystem_FolderReclassify = @"Create table {0} (ID integer primary key autoincrement,ObjectName nvarchar (500),FullPath nvarchar (500),ItemType nvarchar (64),FinishTime nvarchar (100),
            Status nvarchar (500),Comment nvarchar (500))";
        public const string INSERT_DATA_FileSystem_FolderReclassify = "Insert into {0} (ObjectName,FullPath,ItemType,FinishTime,Status,Comment) Values (@ObjectName,@FullPath,@ItemType,@FinishTime,@Status,@Comment)";

        public const string CREATE_TABLE_FileSystem_FolderHold = @"Create table {0} (ID integer primary key autoincrement,ObjectName nvarchar (500),FullPath nvarchar (500),Action nvarchar (500),
            Status nvarchar (500),Comment nvarchar (500))";
        public const string INSERT_DATA_FileSystem_FolderHold = "Insert into {0} (ObjectName,FullPath,Action,Status,Comment) Values (@ObjectName,@FullPath,@Action,@Status,@Comment)";

        public const string CREATE_TABLE_Explorer_GlobalSearchAction = @"Create table {0} (ID integer primary key autoincrement,ObjectName nvarchar (500),Type nvarchar (500),FullPath nvarchar (500),Action nvarchar (500),DestinationLocation nvarchar (500),
            Status nvarchar (500),Comment nvarchar (500))";
        public const string INSERT_DATA_Explorer_GlobalSearchAction = "Insert into {0} (ObjectName,Type,FullPath,Action,DestinationLocation,Status,Comment) Values (@ObjectName,@Type,@FullPath,@Action,@DestinationLocation,@Status,@Comment)";
        public const string CREATE_TABLE_RemoteNode_DataSync = @"Create table {0} (ID integer primary key autoincrement,ObjectName nvarchar (500),Container nvarchar (500),
            Action nvarchar (500),ItemType nvarchar (500),Status nvarchar (500),Comment nvarchar (500))";
        public const string INSERT_DATA_RemoteNode_DataSync = "Insert into {0} (ObjectName,Container,Action,ItemType,Status,Comment) Values (@ObjectName,@Container,@Action,@ItemType,@Status,@Comment)";


        public const string CREATE_TABLE_LocalNode_DataScan = @"Create table {0} (ID integer primary key autoincrement,ObjectName nvarchar (500),FullPath nvarchar (500),
            ItemType nvarchar (500),Action nvarchar (500),Status nvarchar (500),Comment nvarchar (500),AgentName nvarchar (500))";
        public const string INSERT_DATA_LocalNode_DataScan = "Insert into {0} (ObjectName,FullPath,ItemType,Action,Status,Comment,AgentName) Values (@ObjectName,@FullPath,@ItemType,@Action,@Status,@Comment,@AgentName)";

        public const string CREATE_TABLE_OnpremiseSP_EnforceRuleAction = @"Create table {0} (ID integer primary key autoincrement,Type nvarchar (50),ObjectName nvarchar (500),Size nvarchar(500),SourceLocation nvarchar (500),FinishTime  nvarchar (100),RuleName  nvarchar (500),Action  nvarchar (500),AgentName  nvarchar (500),
            Status nvarchar (500),Comment nvarchar (500))";
        public const string INSERT_DATA_OnpremiseSP_EnforceRuleAction = "Insert into {0} (Type,ObjectName,Size,SourceLocation,FinishTime,RuleName,AgentName,Action,Status,Comment) Values (@Type,@ObjectName,@Size,@SourceLocation,@FinishTime,@RuleName,@AgentName,@Action,@Status,@Comment)";

        public const string CREATE_TABLE_OnPremiseSP_ScanLocalNode = @"Create table {0} (ID integer primary key autoincrement,ObjectName nvarchar (500),FullPath nvarchar (500),ItemType nvarchar (500),Action nvarchar (500),Status nvarchar (500),
            Comment nvarchar (500),AgentName nvarchar (500))";
        public const string INSERT_DATA_OnPremiseSP_ScanLocalNode = "Insert into {0} (ObjectName,FullPath,ItemType,Action,Status,Comment,AgentName) Values (@ObjectName,@FullPath,@ItemType,@Action,@Status,@Comment,@AgentName)";

        public const string CREATE_TABLE_Explorer_ExportSearchResult = @"Create table {0} (ID integer primary key autoincrement,ExportLocation nvarchar (500),ReportName nvarchar (500),
            Status nvarchar (500),Comment nvarchar (500))";
        public const string INSERT_DATA_Explorer_ExportSearchResult = "Insert into {0} (ExportLocation,ReportName,Status,Comment) Values (@ExportLocation,@ReportName,@Status,@Comment)";


        public const string CREATE_TABLE_Request_PhyLoanBox = @"Create table {0} (ID integer primary key autoincrement,Name nvarchar (500),Level nvarchar (500),
            Status nvarchar (500),Comment nvarchar (500))";
        public const string INSERT_DATA_Request_PhyLoanBox = "Insert into {0} (Name,Level,Status,Comment) Values (@Name,@Level,@Status,@Comment)";

        public const string CREATE_TABLE_Client_Audit_Report = @"Create table {0} (ID integer primary key autoincrement,Type nvarchar (50),ObjectPath nvarchar (500),Count nvarchar (500),Status nvarchar (500),Comment nvarchar (500))";
        public const string INSERT_DATA_Client_Audit_Report = "Insert into {0} (Type,ObjectPath,Count,Status,Comment) Values (@Type,@ObjectPath,@Count,@Status,@Comment)";

        public const string CREATE_TABLE_Request_PhyPickCompleteBox = @"Create table {0} (ID integer primary key autoincrement,Name nvarchar (500),FullPath nvarchar (500),
            Status nvarchar (500),Comment nvarchar (500))";
        public const string INSERT_DATA_Request_PhyPickCompleteBox = "Insert into {0} (Name,FullPath,Status,Comment) Values (@Name,@FullPath,@Status,@Comment)";

        public const string CREATE_TABLE_TrainingJob = @"Create table {0} (ID integer primary key autoincrement,TermName nvarchar (500),Name nvarchar (500),FullPath nvarchar (500),
            Status nvarchar (500),Comment nvarchar (500))";
        public const string INSERT_DATA_TrainingJob = "Insert into {0} (TermName,Name,FullPath,Status,Comment) Values (@TermName,@Name,@FullPath,@Status,@Comment)";

        public const string CREATE_TABLE_ArchiverFullTextIndexJob = @"Create table {0} (ID integer primary key autoincrement,Url nvarchar (500),
            Status nvarchar (500),Comment nvarchar (500))";
        public const string INSERT_DATA_ArchiverFullTextIndexJob = "Insert into {0} (Url,Status,Comment) Values (@Url,@Status,@Comment)";

        public const string CREATE_TABLE_ArchiverDeleteRestoredDataJob = @"Create table {0} (ID integer primary key autoincrement,Url nvarchar (500),
    RestoredUrl nvarchar(500), CleanOption nvarchar(500), CleanDelayDays integer, IsRelatedDelete nvarchar(500),
    Status nvarchar (500),Comment nvarchar (500))";
        public const string INSERT_DATA_ArchiverDeleteRestoredDataJob = "Insert into {0} (Url,RestoredUrl,CleanOption,CleanDelayDays,IsRelatedDelete,Status,Comment) Values (@Url,@RestoredUrl,@CleanOption,@CleanDelayDays,@IsRelatedDelete,@Status,@Comment)";

        public const string CREATE_TABLE_DiscoveryJobV2 = @"Create table {0} (ID integer primary key autoincrement,Url nvarchar (500),
            Status nvarchar (500),Comment nvarchar (500))";
        public const string INSERT_DATA_DiscoveryJobV2 = "Insert into {0} (Url,Status,Comment) Values (@Url,@Status,@Comment)";

        public const string CREATE_TABLE_DiscoveryProfileJob = @"Create table {0} (ID integer primary key autoincrement,ProfileName nvarchar (500),Url nvarchar (500),
            Status nvarchar (500),Comment nvarchar (500))";
        public const string INSERT_DATA_DiscoveryProfileJob = "Insert into {0} (ProfileName,Url,Status,Comment) Values (@ProfileName,@Url,@Status,@Comment)";
        public const string CREATE_TABLE_DiscoveryGoogleProfileJob = @"Create table {0} (ID integer primary key autoincrement,ProfileName nvarchar (500),DriveName nvarchar (500),
            Status nvarchar (500),Comment nvarchar (500))";
        public const string INSERT_DATA_DiscoveryGoogleProfileJob = "Insert into {0} (ProfileName,DriveName,Status,Comment) Values (@ProfileName,@DriveName,@Status,@Comment)";

        public const string CREATE_TABLE_DiscoveryGoogleJob = @"Create table {0} (ID integer primary key autoincrement,DriveName nvarchar (500),
            Status nvarchar (500),Comment nvarchar (500))";
        public const string INSERT_DATA_DiscoveryGoogleJob = "Insert into {0} (DriveName,Status,Comment) Values (@DriveName,@Status,@Comment)";

        public const string CREATE_TABLE_DiscoveryFileSystemJob = @"Create table {0} (ID integer primary key autoincrement,ConnectionName nvarchar (500),
            Status nvarchar (500),Comment nvarchar (500))";
        public const string INSERT_DATA_DiscoveryFileSystemJob = "Insert into {0} (ConnectionName,Status,Comment) Values (@ConnectionName,@Status,@Comment)";


        public const string CREATE_TABLE_SharePoint_Archiver_Report = @"
Create table {0} (ID integer primary key autoincrement,ActionTab nvarchar (50),Level nvarchar (50),SourceLocation nvarchar (500),DestinationLocation nvarchar (500),Action nvarchar (500),Size nvarchar(500),FinishTime bigint,Status nvarchar (500),Comment nvarchar (500),RuleName nvarchar (500), CreatedDate bigint, CreatedBy nvarchar(500), ModifiedDate bigint, ModifiedBy nvarchar(500), RuleMatchFile nvarchar(500)); 
CREATE INDEX multiColIndex ON {0}(ActionTab, RuleName, FinishTime);";
        public const string INSERT_DATA_SharePoint_Archiver_Report = "Insert into {0} (ActionTab,Level,SourceLocation,DestinationLocation,Action,Size,FinishTime,Status,Comment,RuleName,CreatedDate,CreatedBy,ModifiedDate,ModifiedBy,RuleMatchFile) Values (@ActionTab,@Level,@SourceLocation,@DestinationLocation,@Action,@Size,@FinishTime,@Status,@Comment,@RuleName,@CreatedDate,@CreatedBy,@ModifiedDate,@ModifiedBy,@RuleMatchFile)";

        public const string CREATE_TABLE_DISCOVERY_EXPORT = @"Create table {0} (ID integer primary key autoincrement, ProfileName nvarchar(500), ProfileCriteria nvarchar(500), Action nvarchar(500), FinishTime nvarchar(100), Status nvarchar(500), Comment nvarchar(500))";
        public const string INSERT_DATA_DISCOVERY_EXPORT = @"Insert into {0} (ProfileName, ProfileCriteria, Action, FinishTime, Status, Comment) Values (@ProfileName, @ProfileCriteria, @Action, @FinishTime, @Status, @Comment)";

        public const string REMOVE_DEDUP_DATA_SharePoint_Archiver_Report = "delete from {0} where id in (select max(ID)  from {0} Group by actionTab, level , SourceLocation having count(id) > 1)";
        public const string STATISTICS_SUMMAY_SharePoint_Archiver_Report = "select ActionTab, Status, Level, Action, count(level) as Count, sum(size) as Size  from {0}  Group by actionTab, status, level, action";
        public const string CLEAN_DATA = "delete from {0}";

        //给dedup job用的
        public const string CREATE_TABLE_Archiver_Dedup_Report = @"Create table {0} (ID integer primary key autoincrement,Date bigint,Size bigint,SrcURL nvarchar (500),SubJobId nvarchar (50),Remark9 nvarchar (500),Remark10 bigint,Remark11 nvarchar (500),Remark12 nvarchar (500),Remark13 nvarchar (500),Status nvarchar (500),Comment nvarchar (500))";
        public const string INSERT_DATA_Archiver_Dedup_Report = "Insert into {0} (Date,Size,SrcURL,SubJobId,Remark9,Remark10,Remark11,Remark12,Remark13,Status,Comment) Values (@Date,@Size,@SrcURL,@SubJobId,@Remark9,@Remark10,@Remark11,@Remark12,@Remark13,@Status,@Comment)";

        //给dedup report job用的
        public const string CREATE_TABLE_Archiver_Dedup_ReportJob_Report = @"Create table {0} (ID integer primary key autoincrement,Date bigint,Size bigint,SrcURL nvarchar (500),SubJobId nvarchar (50),Remark1 bigint,Status nvarchar (500),Comment nvarchar (500))";
        public const string INSERT_DATA_Archiver_Dedup_ReportJob_Report = "Insert into {0} (Date,Size,SrcURL,SubJobId,Remark1,Status,Comment) Values (@Date,@Size,@SrcURL,@SubJobId,@Remark1,@Status,@Comment)";

        public const string JOBSUMMAYDETAIL = "JobSummaryDetail";
        public const string CREATE_TABLE_SharePoint_Archiver_SUMMARYReport = @"Create table {0} (ID integer primary key autoincrement,Statistics nvarchar (5000))";
        public const string INSERT_DATA_SharePoint_Archiver_SUMMARYReport = "Insert into {0} (Statistics) Values (@Statistics)";

        public const string CREATE_TABLE_Restore_SUMMARYReport = @"Create table {0} (ID integer primary key autoincrement,Statistics nvarchar (5000))";
        public const string INSERT_DATA_Restore_SUMMARYReport = "Insert into {0} (Statistics) Values (@Statistics)";
        public const string DELETE_DATA_Restore_SUMMARYReport = "Delete from {0}";

        public const string CREATE_TABLE_SharePoint_Restore_Report = @"Create table {0} (ID integer primary key autoincrement,Level nvarchar (500),SourceLocation nvarchar (500),Size nvarchar(500),FinishTime nvarchar (100), ConflictResolution nvarchar (100) ,Status nvarchar (500),Comment nvarchar (500),Path nvarchar (500),PathMd5 nvarchar (50),PolicyLevel nvarchar (50),DestinationUrl nvarchar (500))";
        public const string INSERT_DATA_SharePoint_Restore_Report = "Insert into {0} (Level,SourceLocation,Size,FinishTime,ConflictResolution,Status,Comment,Path,PathMd5,PolicyLevel,DestinationUrl) Values (@Level,@SourceLocation,@Size,@FinishTime,@ConflictResolution,@Status,@Comment,@Path,@PathMd5,@PolicyLevel,@DestinationUrl)";

        public const string START_TIME_COLUMN = "StartTime"; // support sort job detail for job that have inconsistent finish time
        public const string CREATE_TABLE_SharePoint_Migration_Restore_Report = @"Create table {0} (ID integer primary key autoincrement,Level nvarchar (500),SourceLocation nvarchar (500),Size nvarchar(500), StartTime nvarchar (100),FinishTime nvarchar (100), ConflictResolution nvarchar (100) ,Status nvarchar (500),Comment nvarchar (500),Path nvarchar (500),PathMd5 nvarchar (50),PolicyLevel nvarchar (50),DestinationUrl nvarchar (500))";
        public const string INSERT_DATA_SharePoint_Migration_Restore_Report = "Insert into {0} (Level,SourceLocation,Size,StartTime,FinishTime,ConflictResolution,Status,Comment,Path,PathMd5,PolicyLevel,DestinationUrl) Values (@Level,@SourceLocation,@Size,@StartTime,@FinishTime,@ConflictResolution,@Status,@Comment,@Path,@PathMd5,@PolicyLevel,@DestinationUrl)";

        public const string CREATE_TABLE_ArchiverMergeIndex_Report = @"Create table {0} (ID integer primary key autoincrement,SiteCollectionURL nvarchar (500),SourceLocation nvarchar (500),DestinationLocation nvarchar (500),Size nvarchar(500),Status nvarchar (500),Comment nvarchar (500))";
        public const string INSERT_DATA_ArchiverMergeIndex_Report = "Insert into {0} (SiteCollectionURL,SourceLocation,DestinationLocation,Size,Status,Comment) Values (@SiteCollectionURL,@SourceLocation,@DestinationLocation,@Size,@Status,@Comment)";

        public const string CREATE_TABLE_VEOMerge_Report = @"Create table {0} (ID integer primary key autoincrement,FileName nvarchar (500),SourceLocation nvarchar (500),DestinationLocation nvarchar (500),Size nvarchar(500),FinishTime nvarchar (100),Status nvarchar (500),Comment nvarchar (500))";
        public const string INSERT_DATA_VEOMerge_Report = "Insert into {0} (FileName,SourceLocation,DestinationLocation,Size,FinishTime,Status,Comment) Values (@FileName,@SourceLocation,@DestinationLocation,@Size,@FinishTime,@Status,@Comment)";

        public const string CREATE_TABLE_ArchiverRetention_Report = @"Create table {0} (ID integer primary key autoincrement,SiteCollectionURL nvarchar (500),JobId nvarchar (500),SourceLocation nvarchar (500),DestinationLocation nvarchar (500),Size nvarchar(500),Status nvarchar (500),Comment nvarchar (500),Action nvarchar (500))";
        public const string INSERT_DATA_ArchiverRetention_Report = "Insert into {0} (SiteCollectionURL,JobId,SourceLocation,DestinationLocation,Size,Status,Comment,Action) Values (@SiteCollectionURL,@JobId,@SourceLocation,@DestinationLocation,@Size,@Status,@Comment,@Action)";

        public const string CREATE_TABLE_ArchiverRetention_Dashboard_Report = @"Create table {0} (ID integer primary key autoincrement,SiteCollectionURL nvarchar (500),JobId nvarchar (500),SourceLocation nvarchar (500),DestinationLocation nvarchar (500),Size nvarchar(500),Status nvarchar (500),Comment nvarchar (500),Action nvarchar (500),FileName nvarchar (500), SourceFlag int, RetentionSource nvarchar (500),RetentionKeepDate int,RetentionKeepDateUnit int)";
        public const string INSERT_DATA_ArchiverRetention_Dashboard_Report = "Insert into {0} (SiteCollectionURL,JobId,SourceLocation,DestinationLocation,Size,Status,Comment,Action,FileName,SourceFlag,RetentionSource,RetentionKeepDate,RetentionKeepDateUnit) Values (@SiteCollectionURL,@JobId,@SourceLocation,@DestinationLocation,@Size,@Status,@Comment,@Action,@FileName,@SourceFlag,@RetentionSource,@RetentionKeepDate,@RetentionKeepDateUnit)";

        public const string CREATE_TABLE_ArchiverMigration_Report = @"Create table {0} (ID integer primary key autoincrement,ObjectName nvarchar (500),ObjectType nvarchar (500),Status nvarchar (500),Comment nvarchar (500))";
        public const string INSERT_DATA_ArchiverMigration_Report = "Insert into {0} (ObjectName,ObjectType,Status,Comment) Values (@ObjectName,@ObjectType,@Status,@Comment)";

        public const string CREATE_TABLE_StatisticsSoSize_Report = @"Create table {0} (ID integer primary key autoincrement,SourceLocation nvarchar (500),Size nvarchar(500),FinishTime nvarchar(500),KeepDataOption integer,Action nvarchar(500),AuthorID int,AuthorEmail nvarchar(500),ModifiedID int,ModifiedEmail nvarchar(500),CreateTime nvarchar(50),ModifiedTime nvarchar(50),VersionCount int)";
        public const string INSERT_DATA_StatisticsSoSize_Report = "Insert into {0} (SourceLocation,Size,FinishTime,KeepDataOption,Action,AuthorID,AuthorEmail,ModifiedID,ModifiedEmail,CreateTime,ModifiedTime,VersionCount) Values (@SourceLocation,@Size,@FinishTime,@KeepDataOption,@Action,@AuthorID,@AuthorEmail,@ModifiedID,@ModifiedEmail,@CreateTime,@ModifiedTime,@VersionCount)";

        public const string CREATE_TABLE_ArchiverRebuildStub_Report = @"Create table {0} (ID integer primary key autoincrement,SiteCollectionURL nvarchar (500),JobId nvarchar (500),SourceLocation nvarchar (500),DestinationLocation nvarchar (500),Size nvarchar(500),Status nvarchar (500),Comment nvarchar (500))";
        public const string INSERT_DATA_ArchiverRebuildStub_Report = "Insert into {0} (SiteCollectionURL,JobId,SourceLocation,DestinationLocation,Size,Status,Comment) Values (@SiteCollectionURL,@JobId,@SourceLocation,@DestinationLocation,@Size,@Status,@Comment)";

        public const string CREATE_TABLE_ArchiverRebuildIndex_Report = @"Create table {0} (ID integer primary key autoincrement,SiteUrl nvarchar (500),ObjectUrl nvarchar (500),ObjectType nvarchar (50),JobId nvarchar (50),Status nvarchar (500),Comment nvarchar (500))";
        public const string INSERT_DATA_ArchiverRebuildIndex_Report = "Insert into {0} (SiteUrl,ObjectUrl,ObjectType,JobId,Status,Comment) Values (@SiteUrl,@ObjectUrl,@ObjectType,@JobId,@Status,@Comment)";

        public const string CREATE_TABLE_RestoreReport_Report = @"Create table {0} (ID integer primary key autoincrement,Level nvarchar (500),Title nvarchar (500),Url nvarchar (500),Status nvarchar (500),Comment nvarchar (500))";
        public const string INSERT_DATA_RestoreReport_Report = "Insert into {0} (Level,Title,Url,Status,Comment) Values (@Level,@Title,@Url,@Status,@Comment)";

        #region google
        // label
        public const string CREATE_TABLE_GoogleLabelSync = "Create table {0} (ID integer primary key autoincrement,LabelName nvarchar (500), LabelId nvarchar (500),Action nvarchar (500),TenantId nvarchar (128),Status nvarchar (500),Comment nvarchar (500))";
        public const string INSERT_DATA_GoogleLabelSync = "Insert into {0} (LabelName,LabelId,Action,TenantId,Status,Comment) Values (@LabelName,@LabelId,@Action,@TenantId,@Status,@Comment)";

        public const string CREATE_DATA_GoogleDataSync = "Create table {0} (ID integer primary key autoincrement,ObjectName nvarchar (500), FullPath nvarchar(500),Status nvarchar (100), Comment nvarchar (500), ItemType nvarchar(500))";
        public const string INSERT_DATA_GoogleDataSync = "Insert into {0} (ObjectName,FullPath, Status, Comment, ItemType) Values (@ObjectName,@FullPath,@Status,@Comment,@ItemType)";

        public const string CREATE_TABLE_Google_Restore_Report = @"Create table {0} (ID integer primary key autoincrement,DriveId nvarchar (500), Level nvarchar (500),SourceLocation nvarchar (500),Size nvarchar(500),FinishTime nvarchar (100),Status nvarchar (500),Comment nvarchar (500),Path nvarchar (500))";
        public const string INSERT_DATA_Google_Restore_Report = "Insert into {0} (DriveId,Level,SourceLocation,Size,FinishTime,Status,Comment,Path) Values (@DriveId,@Level,@SourceLocation,@Size,@FinishTime,@Status,@Comment,@Path)";
        #endregion

        public const string CREATE_TABLE_DeleteOrphanDatas_Report = @"Create table {0} (ID integer primary key autoincrement,SiteCollectionURL nvarchar (500),JobId nvarchar (500),Size nvarchar(500),Status nvarchar (500),Comment nvarchar (500))";
        public const string INSERT_DATA_DeleteOrphanDatas_Report = "Insert into {0} (SiteCollectionURL,JobId,Size,Status,Comment) Values (@SiteCollectionURL,@JobId,@Size,@Status,@Comment)";

        public const string CREATE_TABLE_ConvertStub = "Create table {0} (ID integer primary key autoincrement, FullPath nvarchar (500), FinishTime nvarchar (100), Action int,Status nvarchar (500),Comment nvarchar (500))";
        public const string INSERT_DATA_ConvertStub = "Insert into {0} (FullPath,FinishTime,Action,Status,Comment) Values (@FullPath,@FinishTime,@Action,@Status,@Comment)";

        public const string CREATE_TABLE_DeclaredRecordsMigration = "Create table {0} (ID integer primary key autoincrement, Url nvarchar (500), FinishTime nvarchar (100), Status nvarchar (500),Comment nvarchar (500))";
        public const string INSERT_DATA_DeclaredRecordsMigration = "Insert into {0} (Url,FinishTime,Status,Comment) Values (@Url,@FinishTime,@Status,@Comment)";

        public const string CREATE_TABLE_StubDisposal = "Create table {0} (ID integer primary key autoincrement, Url nvarchar (500), FinishTime nvarchar (100), Status nvarchar (500),Comment nvarchar (500))";
        public const string INSERT_DATA_StubDisposal = "Insert into {0} (Url,FinishTime,Status,Comment) Values (@Url,@FinishTime,@Status,@Comment)";

        public const string CREATE_TABLE_DeleteArchivedSC = "Create table {0} (ID integer primary key autoincrement, Url nvarchar (500), JobId nvarchar (50), Size bigint, SourceStorageName nvarchar (500), Status nvarchar (500),Comment nvarchar (500))";
        public const string INSERT_DATA_DeleteArchivedSC = "Insert into {0} (Url,JobId,Size,SourceStorageName,Status,Comment) Values (@Url,@JobId,@Size,@SourceStorageName,@Status,@Comment)";

        public const string CREATE_TABLE_MainSyncCommonData = "Create table {0} (ID integer primary key autoincrement, DataCenterName nvarchar (500), Action int, Status nvarchar (500), Comment nvarchar (500))";
        public const string INSERT_DATA_MainSyncCommonData = "Insert into {0} (DataCenterName,Action,Status,Comment) Values (@DataCenterName,@Action,@Status,@Comment)";

        public const string CREATE_TABLE_OtherSyncCommonData = "Create table {0} (ID integer primary key autoincrement, ActionName nvarchar (500), Type nvarchar (500), Status nvarchar (500), Comment nvarchar (500))";
        public const string INSERT_DATA_OtherSyncCommonData = "Insert into {0} (ActionName,Type,Status,Comment) Values (@ActionName,@Type,@Status,@Comment)";

        public const string CREATE_TABLE_FileSystem_Restore = @"Create table {0} (ID integer primary key autoincrement,SourceLocation nvarchar (500),Size nvarchar(500),FinishTime nvarchar (100),Status nvarchar (500),Comment nvarchar (500))";
        public const string INSERT_DATA_FileSystem_Restore = "Insert into {0} (SourceLocation,Size,FinishTime,Status,Comment) Values (@SourceLocation,@Size,@FinishTime,@Status,@Comment)";

        public const string CREATE_TABLE_MainJobDetails = "CREATE TABLE {0} (SubJobID TEXT PRIMARY KEY, Status INTEGER NOT NULL, ProgressStatus INTEGER NOT NULL, Scope TEXT NULL, Comment TEXT NULL," +
            " IsSavedJobDetails INTEGER NOT NULL DEFAULT 0," +
            " Successful INTEGER NOT NULL DEFAULT 0, Failed INTEGER NOT NULL DEFAULT 0, Skipped INTEGER NOT NULL DEFAULT 0," +
            " StartTime INTEGER NULL, FinishTime INTEGER NULL, LastUpdatedTime INTEGER NULL, TotalFiles INTEGER NOT NULL DEFAULT 0," +
            " TotalMatchedRuleFilesForExport INTEGER NOT NULL DEFAULT 0, TotalMatchedRuleFilesForArchive INTEGER NOT NULL DEFAULT 0, TotalMatchedRuleFilesForOtherActions INTEGER NOT NULL DEFAULT 0, ProcessedItemsInfos TEXT NULL," +
            " StartScanTime INTEGER NULL, EstimatedScanFinishedTime INTEGER NULL," +
            " StartExportTime INTEGER NULL, EstimatedExportFinishedTime INTEGER NULL," +
            " StartArchivedTime INTEGER NULL, EstimatedArchivedFinishedTime INTEGER NULL," +
            " StartOtherTime INTEGER NULL, EstimatedOtherFinishedTime INTEGER NULL);";
        public const string UPSERT_DATA_MainJobDetails = "INSERT INTO {0} (SubJobID, Status, ProgressStatus, Scope, Successful, Failed, Skipped, StartTime, FinishTime, LastUpdatedTime, TotalFiles, TotalMatchedRuleFilesForExport, TotalMatchedRuleFilesForArchive, TotalMatchedRuleFilesForOtherActions, ProcessedItemsInfos, StartScanTime, EstimatedScanFinishedTime, StartExportTime, EstimatedExportFinishedTime, StartArchivedTime, EstimatedArchivedFinishedTime, StartOtherTime, EstimatedOtherFinishedTime, Comment, IsSavedJobDetails) " +
            "VALUES (@SubJobID, @Status, @ProgressStatus, @Scope, @Successful, @Failed, @Skipped, @StartTime, @FinishTime, @LastUpdatedTime, @TotalFiles, @TotalMatchedRuleFilesForExport, @TotalMatchedRuleFilesForArchive, @TotalMatchedRuleFilesForOtherActions, @ProcessedItemsInfos, @StartScanTime, @EstimatedScanFinishedTime, @StartExportTime, @EstimatedExportFinishedTime, @StartArchivedTime, @EstimatedArchivedFinishedTime, @StartOtherTime, @EstimatedOtherFinishedTime, @Comment, @IsSavedJobDetails) " +
            "ON CONFLICT(SubJobID) DO UPDATE SET " +
            "Status=excluded.Status, ProgressStatus=excluded.ProgressStatus, Scope=excluded.Scope, Successful=excluded.Successful, Failed=excluded.Failed, Skipped=excluded.Skipped, " +
            "StartTime=excluded.StartTime, FinishTime=excluded.FinishTime, LastUpdatedTime=excluded.LastUpdatedTime, TotalFiles=excluded.TotalFiles, " +
            "TotalMatchedRuleFilesForExport=excluded.TotalMatchedRuleFilesForExport, TotalMatchedRuleFilesForArchive=excluded.TotalMatchedRuleFilesForArchive, TotalMatchedRuleFilesForOtherActions=excluded.TotalMatchedRuleFilesForOtherActions, ProcessedItemsInfos=excluded.ProcessedItemsInfos, " +
            "StartScanTime=excluded.StartScanTime, EstimatedScanFinishedTime=excluded.EstimatedScanFinishedTime, " +
            "StartExportTime=excluded.StartExportTime, EstimatedExportFinishedTime=excluded.EstimatedExportFinishedTime, " +
            "StartArchivedTime=excluded.StartArchivedTime, EstimatedArchivedFinishedTime=excluded.EstimatedArchivedFinishedTime, " +
            "StartOtherTime=excluded.StartOtherTime, EstimatedOtherFinishedTime=excluded.EstimatedOtherFinishedTime, Comment=excluded.Comment, IsSavedJobDetails=excluded.IsSavedJobDetails;";
    }
}
