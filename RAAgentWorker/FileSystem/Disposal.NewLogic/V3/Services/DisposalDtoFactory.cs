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
using System.Linq;
using AvePoint.GCommon.Contract.StorageOptimization.Object;
using AvePoint.GCommon.Contract.Tree.Object;
using AvePoint.Media.Storage;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.Contract.FileSystem;
using AvePoint.RA.FileSystem.Collect;
using AvePoint.RA.FileSystem.Core;
using AvePoint.RA.FileSystem.Stubs;
using AvePoint.RA.FileSystem.Utils;
using RAFileSystem.Utils;

namespace RAFileSystem.FileSystem.Disposal.NewLogic.V3.Services
{
    /// <summary>
    /// Creates <see cref="FSAzureTableEntityDto"/> instances from file stubs and associated data.
    /// </summary>
    public class DisposalDtoFactory
    {
        private readonly FileMetadataExtractor _metadataExtractor;

        public DisposalDtoFactory(FileMetadataExtractor metadataExtractor)
        {
            _metadataExtractor = metadataExtractor;
        }

        public FSAzureTableEntityDto CreateDto(FSFileStub stub, Rule rule, FileSystemRecordDto dbRecord)
        {
            var dto = BuildBaseDto(stub, rule);
            dto.TermId = dbRecord.TermId;
            dto.TermName = dbRecord.TermName;
            ApplyHoldFromDbRecord(dto, dbRecord);
            dto.ManualApprovedBy = dbRecord.ManualApprovedBy;
            dto.ManualEscalateFrom = dbRecord.ManualEscalateFrom;
            dto.Depth = stub.Depth;
            return dto;
        }

        public FSAzureTableEntityDto CreateDto(
            FSFileStub stub,
            Rule rule,
            FSFolderStub folder,
            FileSystemRecordDto holdFolder,
            Guid termId,
            string termName)
        {
            var dto = BuildBaseDto(stub, rule);
            dto.TermId = termId;
            dto.TermName = termName;
            dto.Depth = stub.Depth;

            if (holdFolder != null && holdFolder.HoldStatus)
            {
                ApplyHoldFromDbRecord(dto, holdFolder);
            }

            return dto;
        }

        private FSAzureTableEntityDto BuildBaseDto(FSFileStub stub, Rule rule)
        {
            var xObj = new XFileInfoEx(stub.MediaObj);
            return new FSAzureTableEntityDto
            {
                ConnectionId = FSJobCache.Instance.RootPath.ToLowerInvariant().ToMd5(),
                ScopeID = FSJobCache.Instance.ScopeSettingCache[stub.ScopeSettingId].ScopeId,
                CreateTime = xObj.CreationTimeUtc,
                NodeLevel = (int)NodeLevel.FSFile,
                LastModifiedTme = xObj.LastWriteTimeUtc,
                RuleId = rule != null ? rule.Id : string.Empty,
                ParentID = stub.ParentId,
                LowName = xObj.LowName,
                HighName = xObj.HighName,
                FullPath = ExternalUtil.CombinePath(FSJobCache.Instance.RootPath, xObj.HighName, xObj.LowName),
                MovedToApprovalTable = false,
                ScanTime = DateTime.UtcNow,
                FilePathMd5 = stub.FullPath.ToLowerInvariant().ToMd5(),
                KeepDataOption = rule != null ? rule.KeepDataOption : 0,
                Status = (int)SOApproveDBStatus.None,
                SortTicks = Snowflake.Instance().GetTicks(),
                RuleAction = rule != null ? (int)DisposalRuleUtility.GetRuleAction(rule) : 0,
                Size = xObj.FileSize,
                Property = _metadataExtractor.GetMetaData(xObj),
                InternalConnectionId = FSJobCache.Instance.AveConnectionId,
                CurrentSettingId = ResolveCurrentSettingId(stub)
            };
        }

        private static void ApplyHoldFromDbRecord(FSAzureTableEntityDto dto, FileSystemRecordDto record)
        {
            dto.HoldStatus = record.HoldStatus;
            dto.HoldReleaseTime = record.HoldReleaseTime;
            dto.HoldBy = record.HoldBy;
            dto.HoldId = record.HoldId;
            dto.HoldType = record.HoldType;
            dto.HoldByUsers = record.HoldByUsers;
            dto.HoldUntilTimes = record.HoldUntilTimes;
            dto.AppendHolds_Array = record.AppendHolds_Array;
        }

        private static Guid ResolveCurrentSettingId(FSFileStub stub)
        {
            var settings = FSJobCache.Instance.GroupSettingCache
                .Where(c => stub.FullPath.StartsWith(c.Key));

            if (settings.Any())
            {
                var currentSetting = settings.OrderByDescending(c => c.Key).First();
                return currentSetting.Value.ScopeId;
            }

            return FSJobCache.Instance.ScopeSettingCache[stub.ScopeSettingId].ScopeId;
        }
    }
}

