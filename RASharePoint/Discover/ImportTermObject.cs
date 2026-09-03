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
using AvePoint.RA.Contract.TaxonomyModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.SharePoint.Discover
{
    public class RMImportTermGroupObject
    {
        public string Name;
        public string Desciption;
        public string Path;
    }

    public class RMImportTermSetObject
    {
        public string Name;
        public string Desciption;
        public string Path;
    }

    public class RMImportTermObject
    {
        public string Name;
        public string Desciption;
        public string RuleName;
        public bool InheritParent = true;
        public int enforceRetention;
        public string spLabel;
        public string exoLabel;
        public string oneDriveLabel;
        public string teamsLabel;
        public DateType selDateType;
        public string beginTime { get; set; }
        public string endTime { get; set; }
        public string TimeZoneId { get; set; }

        public int TermSetId = 1;
        public int ParentTermId;
        public string Path;
        public int CurrentLevel;
        public int Id;
        public string AdvanceSetting;
    }

    public class RulePropertyIndex
    {
        public const int Name = 0;
        public const int Description = 1;
        public const int ContainerName = 2;
        public const int RuleLevel = 3;
        public const int DisposalClass = 4;
        public const int SourceType = 5;
        //public const int CriteriaCategory = 5;
        public const int CombineMode = 6;
        public const int CriteriaType = 7;
        public const int CriteriaName = 8;
        public const int CriteriaCondition = 9;
        public const int ConditionValue = 10;
        public const int ConditionValueUnit = 11;
        public const int ConditionBeginTime = 12;
        public const int ConditionEndTime = 13;
        //need jychu review start
        //public const int ConditionTimeZone = 14;
        public const int RuleAction = 14;
        public const int ExportOnlyFormat = 15;
        public const int IncludeRelatedRecord = 16;
        public const int IncludeDeclaredRecord = 17;
        public const int IncludeLockedFileByRecordLabel = 18;
        public const int LeaveStub = 18;
        public const int StubTemplate = 19;
        //public const int DeclareLinkFile = 20;
        public const int ArchiverBeforeDestory = 20;
        public const int RemoveBox = 21;
        public const int IsEnableRemoveRetentionLabel = 22;
        public const int DeclareRecord = 23;
        public const int DoTag = 24;
        public const int TagWithArchived = 25;
        public const int TagWithArchivedBy = 26;
        public const int TagWithArchivedTime = 27;
        public const int TagWithCustomColumn = 28;
        public const int CustomColumnType = 29;
        public const int CustomColumnName = 30;
        public const int CustomColumnValue = 31;
        public const int CustomColumnTimeZone = 32;
        //need jychu review end
        //public const int DoMove = 31;
        public const int RetentionLabel = 34;
        public const int Label = 33;
        public const int RecordLabel = 36;
        //public const int ArchiveEachdocument = 32;
        public const int MoveUrl = 34;
        public const int ConflictResolution = 35;
        public const int DeclareAfterMove = 36;
        public const int RemoveSourceAfterMove = 37;
        public const int KeepReclassifyAfterMove = 38;
        public const int EnableMannualApprove = 39;
        public const int SendEmail = 40;
        public const int ReviewType = 41;
        public const int WorkflowName = 42;
        public const int RecordOwner = 43;
        public const int EnableExport = 44;
        public const int ExportFormat = 45;
        public const int ArchiveDataStorage = 46;
        public const int ExportToDestinationLibrary = 47;
        public const int ExportLocation = 48;
        public const int DeleteToRecycleBin = 49;
        public const int DeleteSiteCollectionToRecycleBin = 50;
        public const int LockRecordBeforeDestroy = 51;
        //need jychu review
        
    }

    public class TermPropertyIndex
    {
        public const int TermGroupName = 0;
        public const int TermSetName = 1;
        public const int Level1 = 2;
        public const int Level2 = 3;
        public const int Level3 = 4;
        public const int Level4 = 5;
        public const int Level5 = 6;
        public const int Description = 7;
        public const int Inherit = 8;
        public const int RuleName = 9;
        public const int Retention = 10;
        public const int RetentionSourceType = 11;
        public const int SharePointOnlineLabelName = 12;
        public const int ExchangeOnlineLabelName = 13;
        public const int OneDriveLabelName = 14;
        public const int TermActivationSettings = 15;
        public const int StartTime = 16;
        public const int EndTime = 17;
        public const int TimeZone = 18;
        public const int AdvanceSetting = 19;
        public const int TeamsLabelName = 20;
    }

    public enum TermLevel
    {
        Group = 1,
        TermSet = 2,
        Term = 3
    }
}
