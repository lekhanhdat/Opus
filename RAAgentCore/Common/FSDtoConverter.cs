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

namespace RAFileSystemCore.Common
{
    public class FSDtoConverter
    {
        public const long NextJob = -1;
        public const long Pending = -2;
        public static long None = DateTime.MinValue.Ticks;
        //public static RMTerm ConvertJobTermObjt2RMTerm(FSTermDto dto)
        //{
        //    RMTerm term = new RMTerm()
        //    {
        //        Id = dto.Id,
        //        TermSetId = dto.TermSetId,
        //        UniqueId = dto.UniqueId,
        //        Name = dto.Name,
        //        //BreakInheritFromParent = dto.BreakInheritFromParent,
        //        //TimeZoneId = dto.TimeZoneId,
        //        //RuleInfo = dto.RuleInfo,
        //        TermExpirationFrom = dto.TermExpirationFrom,
        //        TermExpirationTo = dto.TermExpirationTo,
        //        IsDeprecated = dto.IsDeprecated,
        //        IsRemoved = dto.IsRemoved,
        //        //IsRootTerm = dto.IsRootTerm,
        //        //IsDayLight = dto.IsDayLight,
        //        //AvailableSpace = dto.AvailableSpace,
        //        //IsDefaultTerm = dto.IsDefaultTerm,
        //        //IsPermanent = dto.IsPermanent
        //    };
        //    return term;
        //}

        //public static Rule ConvertRuleDto2Rule(RuleDto dto)
        //{
        //    Rule rule = new Rule()
        //    {
        //        Id = dto.RuleId,
        //        Name = dto.RuleName,
        //    };
        //    return rule;
        //}

        //public static RMFileSystemSetting ConvertJobRMSetting2RMFSSetting(FSSettingDto scopeSetting)
        //{
        //    RMFileSystemSetting setting = new RMFileSystemSetting()
        //    {
        //        ApplyExistType = scopeSetting.ApplyExistType,
        //        AutoClassificationRules = scopeSetting.AutoClassificationRules,
        //        AutoJobOption = scopeSetting.AutoJobOption,
        //        //ConnectionGroupId = scopeSetting.ConnectionGroupId,
        //        DefaultTermId = scopeSetting.DefaultTermId,
        //        //DefaultTermName = scopeSetting.DefaultTermName,
        //        DeployTermMethod = scopeSetting.DeployTermMethod,
        //        FullPath = scopeSetting.FullPath,
        //        IsActive = scopeSetting.IsActive,
        //        //Name = scopeSetting.Name,
        //        NeedCheckDefaultValue = scopeSetting.NeedCheckDefaultValue,
        //        RunAutoFullJob = scopeSetting.RunAutoFullJob,
        //        ScopeId = scopeSetting.ScopeId,
        //        TermId = scopeSetting.TermId,
        //        //TermName = scopeSetting.TermName,
        //        TermSetId = scopeSetting.TermSetId,
        //        //TermSetName = scopeSetting.TermSetName

        //    };
        //    return setting;
        //}

        //public static FileSystemRecordDto ConvertRMBaseRecordToFSDto(Record record)
        //{
        //    return new FileSystemRecordDto()
        //    {
        //        NodeId = record.NodeId,
        //        SourceFlag = (int)SourceFlag.FileSystem,
        //        RecordStatus = record.RecordStatus,
        //        LeafName = record.LeafName,
        //        NodeType = (int)record.NodeType,
        //        TermId = record.TermId,
        //        TermName = record.TermName,
        //        ItemId = record.ItemId,
        //        ListId = record.ListId,
        //        ItemRowId = record.ItemRowId,
        //        ParentId = record.ParentId,
        //        AveSiteId = record.AveSiteId,
        //        CollectionTime = record.CollectTime,
        //        DirPath = record.DirPath,
        //        FolderId = record.FolderId,
        //        RecordHistory = record.RecordHistory,
        //        RecordOwner = record.RecordOwner,
        //        RuleId = record.RuleId,
        //        ScopeId = record.ScopeId,
        //        WebId = record.WebId,
        //        RuleLevel = record.RuleLevel,
        //        MetaInfo = record.MetaInfo,
        //        Extsion1 = record.Extsion1,
        //        FullPath = record.FullPath,
        //        ExtensionForFile = record.ExtensionForFile,
        //        HoldStatus = record.HoldStatus,
        //        PreviosDisposalDueDate = ConvertLongDueDate2String(record.PreviosDisposalDueDate),
        //        DeclareAsRecord = record.DeclareAsRecord,
        //        RecordsId = record.RecordsId,
        //        LocationId = record.LocationId,
        //        BoxId = record.BoxId,
        //        FileId = record.FileId,
        //        TemplateId = record.TemplateId,
        //        DisposalDueDate = ConvertLongDueDate2String(record.DisposalDueDate),
        //        //System Fields
        //        CreatedBy = record.CreatedBy,
        //        ModifiedBy = record.ModifiedBy,
        //        TimeCreated1 = DateTime.UtcNow,
        //        TimeLastModified = DateTime.UtcNow.Ticks
        //    };
        //}


        //public static Record ConvertFSDtoToRMBaseRecord(FileSystemRecordDto dto)
        //{
        //    return new Record()
        //    {
        //        NodeId = dto.NodeId,
        //        SourceFlag = (int)SourceFlag.FileSystem,
        //        RecordStatus = dto.RecordStatus,
        //        LeafName = dto.LeafName,
        //        NodeType = (int)dto.NodeType,
        //        TermId = dto.TermId,
        //        TermName = dto.TermName,
        //        ItemId = dto.ItemId,
        //        ListId = dto.ListId,
        //        ItemRowId = dto.ItemRowId,
        //        ParentId = dto.ParentId,
        //        AveSiteId = dto.AveSiteId,
        //        CollectTime = dto.CollectionTime,
        //        DirPath = dto.DirPath,
        //        FolderId = dto.FolderId,
        //        RecordHistory = dto.RecordHistory,
        //        RecordOwner = dto.RecordOwner,
        //        RuleId = dto.RuleId,
        //        ScopeId = dto.ScopeId,
        //        WebId = dto.WebId,
        //        RuleLevel = dto.RuleLevel,
        //        MetaInfo = dto.MetaInfo,
        //        Extsion1 = dto.Extsion1,
        //        FullPath = dto.FullPath,
        //        ExtensionForFile = dto.ExtensionForFile,
        //        HoldStatus = dto.HoldStatus,
        //        PreviosDisposalDueDate = ConvertStringDueDate2Long(dto.PreviosDisposalDueDate),
        //        DeclareAsRecord = dto.DeclareAsRecord,
        //        RecordsId = dto.RecordsId,
        //        LocationId = dto.LocationId,
        //        BoxId = dto.BoxId,
        //        FileId = dto.FileId,
        //        TemplateId = dto.TemplateId,
        //        DisposalDueDate = ConvertStringDueDate2Long(dto.DisposalDueDate),
        //        //System Fields
        //        CreatedBy = dto.CreatedBy,
        //        ModifiedBy = dto.CreatedBy,
        //        TimeCreated = dto.TimeCreated1.Ticks,
        //        TimeModified = dto.TimeLastModified
        //    };
        //}
        public static long ConvertStringDueDate2Long(string dueDateStr)
        {
            switch (dueDateStr)
            {
                case null:
                case "":
                    return None;
                case "RM_JS_JM_EndTimePending":
                case "Pending":
                    return Pending;
                case "RDM_RecordsExporer_Status_NextJob":
                case "Next Job":
                    return NextJob;
                default:
                    long dueDateLong;
                    if (long.TryParse(dueDateStr, out dueDateLong))
                    {
                        DateTime dt = new DateTime(dueDateLong);
                        return dueDateLong;
                    }
                    else
                    {
                        throw new Exception("DueDate can not convert to long...");
                    }
            }

        }

        public static string ConvertLongDueDate2String(long dueDate)
        {
            switch (dueDate)
            {
                case 0:
                    return string.Empty;
                case Pending:
                    return "RM_JS_JM_EndTimePending";
                case NextJob:
                    return "RDM_RecordsExporer_Status_NextJob";
                default:
                    return dueDate.ToString();
            }
        }
    }
}
