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
using Aspose.Pdf.Operators;
using AvePoint.GCommon.Contract.Tree.Object;
using AvePoint.GCommon.Utility;
using AvePoint.RA.Common;
using AvePoint.RA.Common.Audit.JPMC;
using AvePoint.RA.Common.SystemSetting;
using AvePoint.RA.Common.Util;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Audit.JPMC;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.Contract.FileSystem;
using AvePoint.RA.Contract.JPMC;
using AvePoint.RA.Contract.ManualApproval.Model;
using AvePoint.RA.Contract.Myhub.Items.Actions;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RMWeb.Audit;
using AvePoint.RA.Contract.RMWeb.CP;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Model;
using AvePoint.RA.I18N.Core;
using AvePoint.RA.Service.Services.Audit.JPMC;
using Castle.Components.DictionaryAdapter.Xml;
using Cloud.Sdk.Data.Aos;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using SOApproveDBStatus = AvePoint.RA.Contract.SOApproveDBStatus;

namespace AvePoint.RA.Service.Service.Audit.JPMC
{
    public class FSAuditSinkService : IFSAuditSinkService
    {
        private static readonly RALogger _logger = RALogger.GetInstance(typeof(FSAuditSinkService));

        private IRMFSAuditDao _auditDao => PlatformWindsorManager.GetService<IRMFSAuditDao>();
        private IGeneralSettingService _generalSettingService => PlatformWindsorManager.GetService<IGeneralSettingService>();

        private readonly int _maxRetries = 3;

        private readonly TimeSpan _initialRetryDelay = TimeSpan.FromMilliseconds(300);

        public async Task FlushAsync(FSAuditRecord record)
        {
            if (record == null) return;
            try
            {
                await _auditDao.InsertAsync(record);
            }
            catch (Exception ex)
            {
                _logger.Error("Failed to write single audit record: {0}", ex);
                throw;
            }
        }

        public async Task BulkInsertAsync(IReadOnlyList<FSAuditRecord> records)
        {
            if (records == null || records.Count == 0) return;

            var attempt = 0;
            var delay = _initialRetryDelay;

            while (true)
            {
                try
                {
                    attempt++;
                    await _auditDao.BulkInsertAsync(records);
                    return;
                }
                catch (Exception ex) when (attempt < _maxRetries)
                {
                    _logger.Warn("Bulk insert attempt {0}/{1} failed ({2} records). Retrying in {3}ms. Error: {4}",
                        attempt, _maxRetries, records.Count, delay.TotalMilliseconds, ex.Message);
                    await Task.Delay(delay);
                    delay = TimeSpan.FromTicks(delay.Ticks * 2);
                }
                catch (Exception ex)
                {
                    _logger.Error("Bulk insert failed after {0} attempts ({1} records): {2}", _maxRetries, records.Count, ex);
                    throw;
                }
            }
        }

        public async Task<(List<FSAuditRecord> Items, int TotalCount)> QueryAsync(List<FSAuditQueryFilter> filters, int? skip = null, int? take = null, FSAuditQueryOrder order = null)
        {
            var pageSize = take ?? 10;
            var pageIndex = skip.HasValue && pageSize > 0 ? (skip.Value / pageSize) + 1 : 1;

            var queryParam = BuildQueryParam(filters, pageIndex, pageSize, order);
            var filterExpression = BuildFilterExpressionAsync(filters);
            var (items, totalCount) = await _auditDao.QueryAsync(filterExpression, queryParam);

            var records = items.Select(MapEntityToRecord).ToList();
            return (records, totalCount);
        }

        public Dictionary<int, string> FetchAllAuditTypes()
        {
            Dictionary<int, string> actionItems = new Dictionary<int, string>();
            try
            {
                var auditTypes = _auditDao.FetchAllAuditTypes();
                foreach (var audit in auditTypes)
                {
                    actionItems.Add((int)audit, audit.ToDescription());
                }
                return actionItems.Where(x=> !string.IsNullOrEmpty(x.Value))
                    .ToDictionary(x=> x.Key, x=> x.Value);
            }
            catch (Exception ex)
            {
                _logger.Error("Failed to fetch audit types: {0}", ex);
            }
            return actionItems;
        }

        public List<string> FetchAllAuditUsers()
        {
            var userNames = _auditDao.FetchAllAuditUsers();
            var systemUserNames = GetRecordsSystemUserNames();
            if (userNames.Any(o => systemUserNames.Contains(o)))
            {
                userNames = userNames.Except(systemUserNames).ToList();
                userNames.Add("RM_TS_RunSchedule");
            }
            return userNames;
        }

        #region Move file action
        public async Task FlushAsync(List<FsRecordProcessDto> records)
        {
            try
            {
                await FlushChunksAsync(records, BuildAuditRecords);
            }
            catch (Exception ex)
            {
                _logger.Error("Failed to write {0} audit records: {1}", records.Count, ex);
                throw;
            }
        }

        public async Task FlushAsync(List<RMFileSystemAudit> audtis)
        {
            try
            {
                await FlushChunksAsync(audtis, BuildAuditRecords);
            }
            catch (Exception ex)
            {
                _logger.Error("Failed to write {0} audit records: {1}", audtis.Count, ex);
                throw;
            }
        }

        private static List<FSAuditRecord> BuildAuditRecords(List<FsRecordProcessDto> records)
        {
            var movedRecords = records.Where(r => r.NodeType == (int)NodeLevel.FSFile).ToList();
            var result = new List<FSAuditRecord>(movedRecords.Count);
            for (int i = 0; i < movedRecords.Count; i++)
            {
                var record = movedRecords[i];
                var auditLevel = ResolveAuditLevel(record.AuditLevel);
                var context = FSAuditContext.GetNewContext(FSAuditType.MoveFile, auditLevel);
                context.ExecutedBy = FSAuditExecutedBy.User;
                context.ActionTimeUtc = DateTime.UtcNow.Ticks;
                context.CurrentPath = DetectDirectoryPathOrFile(record.NewPath);
                context.PreviousPath = DetectDirectoryPathOrFile(record.FullPath);
                context.ConnectionGroupId = record.ConnectionGroupId;
                context.ConnectionId = record.ConnectionId;
                context.ItemId = record.NewNodeId;
                context.ObjectName = DetectDirectoryPathOrFile(record.NewPath, true);
                context.AddModifiedContent("RM_FS_Monitoring_MoveFile", record.FullPath, record.NewPath);
                result.Add(FSAuditRecordBuilder.BuildWithValidation(context, null));
            }
            return result;
        }

        private static List<FSAuditRecord> BuildAuditRecords(List<RMFileSystemAudit> audits)
        {
            var result = new List<FSAuditRecord>(audits.Count);
            for (int i = 0; i < audits.Count; i++)
            {
                var record = audits[i];
                var auditLevel = ResolveAuditLevel((int)record.Level);
                var context = FSAuditContext.GetNewContext(FSAuditType.MoveFile, auditLevel);
                context.ExecutedBy = FSAuditExecutedBy.User;
                context.ActionTimeUtc = DateTime.UtcNow.Ticks;
                context.CurrentPath = DetectDirectoryPathOrFile(record.TargetPath);
                context.PreviousPath = DetectDirectoryPathOrFile(record.OriginPath);
                context.ConnectionGroupId = new Guid(record.ConnectionGroupId);
                context.ConnectionId = new Guid(record.ConnectionId);
                context.ItemId = record.ItemId;
                context.ObjectName = DetectDirectoryPathOrFile(record.TargetPath, true);
                context.AddModifiedContent("RM_FS_Monitoring_MoveFile", record.OriginPath, record.TargetPath);
                result.Add(FSAuditRecordBuilder.BuildWithValidation(context, null));
            }
            return result;
        }

        #endregion

        #region Download RCC report action

        public async Task RCCFlushAsync(RCCReportRequest request, string jobId)
        {
            try
            {
                await FlushChunksAsync(request.Nodes, nodes => BuildRCCAuditRecords(nodes, request, jobId));
            }
            catch (Exception ex)
            {
                _logger.Error("Failed to write {0} RCC audit records: {1}", request.Nodes.Count, ex);
                throw;
            }
        }

        private List<FSAuditRecord> BuildRCCAuditRecords(List<RCCNode> nodes, RCCReportRequest request, string jobId)
        {
            var result = new List<FSAuditRecord>(nodes.Count);
            
            var auditLevel = ResolveAuditLevel(request.Level);
            var timeRangeStr = BuildRCCTimeRange(request);
            var actionTime = DateTime.UtcNow.Ticks;

            switch (request.Level)
            {
                case (int)NodeLevel.SiteCollection:
                    auditLevel = FSAuditLevel.Connection;
                    break;
                case (int)NodeLevel.FSFolder:
                    auditLevel = FSAuditLevel.Folder;
                    break;
                case (int)NodeLevel.FSFile:
                    auditLevel = FSAuditLevel.File;
                    break;
            }
            for (int i = 0; i < nodes.Count; i++)
            {
                var node = nodes[i];
                var context = FSAuditContext.GetNewContext(FSAuditType.DownloadRCCReport, auditLevel);
                context.AuditLevel = auditLevel;
                context.ExecutedBy = FSAuditExecutedBy.User;
                context.ActionTimeUtc = actionTime;
                context.CurrentPath = node.FullPath;
                context.ConnectionGroupId = request.ConnGroupId;
                context.ConnectionId = request.ConnectionId;
                context.ItemId = node.Id;
                context.ObjectName = request.IsMyHub ? request.DisplayName : jobId;

                //context.AddModifiedContent("RM_FS_DateRangeCustom_Title", "", timeRangeStr);

                result.Add(FSAuditRecordBuilder.BuildWithValidation(context, null));
            }

            return result;
        }

        private string BuildRCCTimeRange(RCCReportRequest request)
        {
            if (request.TimeRange == null) return string.Empty;

            var gls = _generalSettingService.GetGeneralSettingAsync().GetAwaiter().GetResult();
            return request.TimeRange.PresetType switch
            {
                1 => I18NEntity.GetString("RM_FS_DateRangeCustom_3M"),
                2 => I18NEntity.GetString("RM_FS_DateRangeCustom_6M"),
                3 => I18NEntity.GetString("RM_FS_DateRangeCustom_1Y"),
                _ => BuildCustomDateString(request, gls)
            };
        }

        private string BuildCustomDateString(RCCReportRequest request, GeneralSettingModel gls)
        {
            string fromLabel = I18NEntity.GetString("RM_JS_RDM_CreateRule_DateOption_From");
            string toLabel = I18NEntity.GetString("RM_JS_RDM_CreateRule_DateOption_To");
            var startStr = string.Empty;
            var endStr = string.Empty;
            string dateFormat = GeneralSettingConfig.DateFormats[(DateFormat)Enum.Parse(typeof(DateFormat), Enum.GetName(typeof(DateFormat), gls.DataFormatId), true)];

            var timeZoneIndex = ResolveTimeZoneIndex(request.TimeZoneId);
            if (timeZoneIndex >= 0)
            {
                startStr = _generalSettingService.ConvertTiksToDateTime(gls, request.TimeRange.StartDateTicks, true, timeZoneIndex, request.IsDaylight, dateFormat).SimplifyFormatTime;
                endStr = _generalSettingService.ConvertTiksToDateTime(gls, request.TimeRange.EndDateTicks, true, timeZoneIndex, request.IsDaylight, dateFormat).SimplifyFormatTime;
            }
            else
            {
                startStr = _generalSettingService.ConvertTiksToDateTime(gls, request.TimeRange.StartDateTicks, true).SimplifyFormatTime;
                endStr = _generalSettingService.ConvertTiksToDateTime(gls, request.TimeRange.EndDateTicks, true).SimplifyFormatTime;
            }

            return $"{fromLabel} {startStr} {toLabel} {endStr}";
        }

        private static int ResolveTimeZoneIndex(string timeZoneId)
        {
            if (string.IsNullOrEmpty(timeZoneId)) return -1;

            if (int.TryParse(timeZoneId, out var numericIndex))
            {
                return numericIndex >= 0 && numericIndex < DateTimeUtil.AllTimeZones.Count
                    ? numericIndex
                    : -1;
            }

            var index = DateTimeUtil.AllTimeZones.FindIndex(
                tz => string.Equals(tz, timeZoneId, StringComparison.OrdinalIgnoreCase));

            return index;
        }

        #endregion

        #region Approve Or Reject file action
        public async Task ApproveOrRejectFlushAsync(List<ManualApprovalFSAuditRecordDto> records)
        {
            if (records == null || records.Count <= 0) return;
            const int chunkSize = 1000;
            var threshold = (int)(chunkSize * 0.5);

            try
            {
                if (records.Count < threshold)
                {
                    var auditRecords = ApproveOrRejectBuildAuditRecords(records);
                    if (auditRecords.Count == 0) return;
                    await _auditDao.InsertBatchAsync(auditRecords);
                    return;
                }

                var tasks = new List<Task>((records.Count / chunkSize) + 1);

                for (int i = 0; i < records.Count; i += chunkSize)
                {
                    var count = Math.Min(chunkSize, records.Count - i);
                    var chunk = records.GetRange(i, count);
                    tasks.Add(ApproveOrRejectProcessChunkAsync(chunk));
                }

                await Task.WhenAll(tasks);
            }
            catch (Exception ex)
            {
                _logger.Error("Failed to write {0} audit records: {1}", records.Count, ex);
                throw;
            }
        }

        private async Task ApproveOrRejectProcessChunkAsync(List<ManualApprovalFSAuditRecordDto> chunk)
        {
            var auditRecords = ApproveOrRejectBuildAuditRecords(chunk);
            if (auditRecords.Count == 0) return;
            await _auditDao.BulkInsertAsync(auditRecords);
        }

        private static List<FSAuditRecord> ApproveOrRejectBuildAuditRecords(List<ManualApprovalFSAuditRecordDto> records)
        {
            var result = new List<FSAuditRecord>(records.Count);
            for (int i = 0; i < records.Count; i++)
            {
                var record = records[i];
                var auditLevel = ResolveAuditLevel(record.AuditLevel);
                FSAuditContext context;
                if (record.ActionType == SOApproveDBStatus.Approved)
                {
                    context = FSAuditContext.GetNewContext(FSAuditType.JpmcAuditApprove, auditLevel);
                    context.AddModifiedContent("RM_FS_JpmcAuditApprove", "", record.Content);
                }
                else if (record.ActionType == SOApproveDBStatus.Rejected)
                {
                    context = FSAuditContext.GetNewContext(FSAuditType.JpmcAuditReject, auditLevel);
                    context.AddModifiedContent("RM_FS_JpmcAuditReject", "", record.Content);
                }
                else { 
                    continue;
                }
                context.ExecutedBy = FSAuditExecutedBy.User;
                context.ActionTimeUtc = DateTime.UtcNow.Ticks;
                context.CurrentPath = record.FullPath;
                context.ConnectionGroupId = Guid.Parse(record.ConnectionGroupId);
                context.ConnectionId = Guid.Parse(record.ConnectionId);
                context.ItemId = record.NodeId;
                context.ObjectName = record.NodeName;
                result.Add(FSAuditRecordBuilder.BuildWithValidation(context, null));
            }
            return result;
        }
        #endregion

        #region Pause Or Resume file action
        public async Task PauseOrResumeFlushAsync(List<ManualApprovalFSAuditRecordDto> records)
        {
            if (records == null || records.Count <= 0) return;
            const int chunkSize = 1000;
            var threshold = (int)(chunkSize * 0.5);

            try
            {
                if (records.Count < threshold)
                {
                    var auditRecords = PauseOrResumeBuildAuditRecords(records);
                    if (auditRecords.Count == 0) return;
                    await _auditDao.InsertBatchAsync(auditRecords);
                    return;
                }

                var tasks = new List<Task>((records.Count / chunkSize) + 1);

                for (int i = 0; i < records.Count; i += chunkSize)
                {
                    var count = Math.Min(chunkSize, records.Count - i);
                    var chunk = records.GetRange(i, count);
                    tasks.Add(PauseOrResumeProcessChunkAsync(chunk));
                }

                await Task.WhenAll(tasks);
            }
            catch (Exception ex)
            {
                _logger.Error("Failed to write {0} audit records: {1}", records.Count, ex);
                throw;
            }
        }

        private async Task PauseOrResumeProcessChunkAsync(List<ManualApprovalFSAuditRecordDto> chunk)
        {
            var auditRecords = PauseOrResumeBuildAuditRecords(chunk);
            if (auditRecords.Count == 0) return;
            await _auditDao.BulkInsertAsync(auditRecords);
        }

        private static List<FSAuditRecord> PauseOrResumeBuildAuditRecords(List<ManualApprovalFSAuditRecordDto> records)
        {
            var result = new List<FSAuditRecord>(records.Count);
            for (int i = 0; i < records.Count; i++)
            {
                var record = records[i];
                var auditLevel = ResolveAuditLevel(record.AuditLevel);
                FSAuditContext context;
                if (record.IsPause == 1)
                {
                    context = FSAuditContext.GetNewContext(FSAuditType.JpmcAuditPause, auditLevel);
                }
                else 
                {
                    context = FSAuditContext.GetNewContext(FSAuditType.JpmcAuditResume, auditLevel);
                }
                context.ExecutedBy = FSAuditExecutedBy.User;
                context.ActionTimeUtc = DateTime.UtcNow.Ticks;
                context.CurrentPath = record.FullPath;
                context.ConnectionGroupId = Guid.Parse(record.ConnectionGroupId);
                context.ConnectionId = Guid.Parse(record.ConnectionId);
                context.ItemId = record.NodeId;
                context.ObjectName = record.NodeName;
                result.Add(FSAuditRecordBuilder.BuildWithValidation(context, null));
            }
            return result;
        }
        #endregion

        #region Delete myhub report action

        public async Task MyhubReportContentFlushAsync(List<RMMyhubReportAuditItem> records, int auditType, int reportType)
        {
            const int chunkSize = 1000;
            var threshold = (int)(chunkSize * 0.5);

            try
            {
                if (records.Count < threshold)
                {
                    var auditRecords = BuildMyhubReportAuditRecords(records, auditType, reportType);
                    if (auditRecords.Count == 0) return;
                    await _auditDao.InsertBatchAsync(auditRecords);
                    return;
                }

                var tasks = new List<Task>((records.Count / chunkSize) + 1);

                for (int i = 0; i < records.Count; i += chunkSize)
                {
                    var count = Math.Min(chunkSize, records.Count - i);
                    var chunk = records.GetRange(i, count);
                    tasks.Add(ProcessMyhubReportChunkAsync(chunk, auditType, reportType));
                }

                await Task.WhenAll(tasks);
            }
            catch (Exception ex)
            {
                _logger.Error("Failed to write {0} RCC audit records: {1}", records.Count, ex);
                throw;
            }
        }

        private async Task ProcessMyhubReportChunkAsync(List<RMMyhubReportAuditItem> chunk, int auditType, int reportType)
        {
            var auditRecords = BuildMyhubReportAuditRecords(chunk, auditType, reportType);
            if (auditRecords.Count == 0) return;
            await _auditDao.BulkInsertAsync(auditRecords);
        }

        private List<FSAuditRecord> BuildMyhubReportAuditRecords(List<RMMyhubReportAuditItem> records, int auditType, int reportType)
        {
            var result = new List<FSAuditRecord>(records.Count);
            var actionTime = DateTime.UtcNow.Ticks;
            foreach (var record in records)
            {
                var auditLevel = ResolveAuditLevel(record.Level);
                var context = FSAuditContext.GetNewContext((FSAuditType)auditType, auditLevel);
                context.ExecutedBy = FSAuditExecutedBy.User;
                context.ActionTimeUtc = actionTime;
                context.CurrentPath = record.FullPath;
                context.ConnectionGroupId = record.ConnGroupId;
                context.ConnectionId = record.ConnectionId;
                context.ItemId = record.ItemId;
                context.ObjectName = record.ReportName;
                //if (auditType == (int)FSAuditType.DeleteDisposalHistory)
                //{
                //    context.AddModifiedContent("", "", record.ReportName);
                //}
                //else if (auditType == (int)FSAuditType.DeleteRCCReport)
                //{
                //    context.AddModifiedContent("", "", record.ReportName);
                //}
                result.Add(FSAuditRecordBuilder.BuildWithValidation(context, null));
            }
            return result;
        }

        #endregion

        #region Private methods

        private System.Linq.Expressions.Expression<Func<RMFSAudit, bool>> BuildFilterExpressionAsync(List<FSAuditQueryFilter> filters)
        {
            if (filters == null || filters.Count == 0) return null;
            return (new FSAuditFilterBuilder(filters)).Build();
        }

        private async Task FlushChunksAsync<T>(List<T> records, Func<List<T> , List<FSAuditRecord>> assembleRecordFunc)
        {
            if (records == null || records.Count <= 0) return;

            const int chunkSize = 1000;
            const int threshold = 500;

            if (records.Count < threshold)
            {
                var auditRecords = assembleRecordFunc(records);
                if (auditRecords.Count > 0) await _auditDao.InsertBatchAsync(auditRecords);
                return;
            }

            var tasks = new List<Task>((records.Count + chunkSize - 1) / chunkSize);

            for (int i = 0; i < records.Count; i += chunkSize)
            {
                var count = Math.Min(chunkSize, records.Count - i);
                var chunk = records.GetRange(i, count);

                tasks.Add(Task.Run(async () =>
                {
                    var auditRecords = assembleRecordFunc(chunk);
                    if (auditRecords.Count > 0)
                        await _auditDao.BulkInsertAsync(auditRecords);
                }));
            }

            await Task.WhenAll(tasks);
        }

        private static FSAuditQueryParam BuildQueryParam(List<FSAuditQueryFilter> filters, int pageIndex = 1, int pageSize = 10, FSAuditQueryOrder order = null)
        {
            return new FSAuditQueryParam
            {
                Filters = filters,
                PageIndex = pageIndex,
                PageSize = pageSize,
                Order = order ?? new FSAuditQueryOrder
                {
                    ColumnName = nameof(RMFSAudit.ExecutedTime),
                    IsDesc = true
                }
            };
        }

        private static string GetEnumDescription(Enum value)
        {
            var field = value.GetType().GetField(value.ToString());
            if (field == null) return value.ToString();

            var attribute = field.GetCustomAttribute<DescriptionAttribute>();
            return attribute?.Description ?? value.ToString();
        }

        private FSAuditRecord MapEntityToRecord(RMFSAudit entity)
        {
            return new FSAuditRecord
            {
                AuditType = entity.AuditType,
                AuditTypeStr = ResolveAuditTypeStr(entity.AuditType),
                AuditLevel = entity.AuditLevel,
                Content = ConvertContentToJson(entity.Content),
                ClientIP = entity.ClientIP,
                UserName = entity.ExecutedBy,
                ActionTimeUtc = entity.ExecutedTime,
                Status = entity.Status,
                StatusStr = I18NEntity.GetString(((AuditStatus)entity.Status).ToDescription()),
                ObjectName = entity.ObjectName,
                ConnectionGroupId = entity.ConnectionGroupId,
                ConnectionId = entity.ConnectionId,
                ItemId = entity.ItemId,
                CurrentPath = entity.FullPath
            };
        }

        private string ConvertContentToJson(string xmlContent)
        {
            if (string.IsNullOrEmpty(xmlContent)) return "[]";
            try
            {
                var sanitized = ConvertXmlString(xmlContent);
                var items = SerializerHelper.DeserializeFromXmlString<List<FSAuditModifiedContent>>(sanitized);
                if (items == null || items.Count == 0) return "[]";

                items.ForEach(item =>
                {
                    item.TargetSetting = I18NEntity.GetString(item.TargetSetting);
                    item.NewValue = ReplaceI18NKeys(item.NewValue, "RM_", [" ", ",", ";", ":"]);
                    item.OldValue = ReplaceI18NKeys(item.OldValue, "RM_", [" ", ",", ";", ":"]);
                });

                return Newtonsoft.Json.JsonConvert.SerializeObject(items);
            }
            catch (Exception ex)
            {
                _logger.Warn("Failed to convert audit content to JSON: {0}", ex.Message);
                return "[]";
            }
        }

        private string ReplaceI18NKeys(string content, string prefix, string[] separators)
        {
            if (string.IsNullOrEmpty(content) || separators?.Length == 0)
                return I18NEntity.ReplaceI18NKey(content, prefix, separators);

            var pattern = "(" + string.Join("|", Array.ConvertAll(separators, Regex.Escape)) + ")";
            var parts = Regex.Split(content, pattern);

            foreach (var i in Enumerable.Range(0, parts.Length))
            {
                if (!string.IsNullOrWhiteSpace(parts[i]) && Array.IndexOf(separators, parts[i]) < 0)
                {
                    parts[i] = I18NEntity.ReplaceI18NKey(parts[i], prefix, separators);
                }
            }

            return string.Concat(parts);
        }

        private string ConvertXmlString(string content)
        {
            if (string.IsNullOrEmpty(content)) return content;
            return IsValidXmlString(content) ? content : RemoveInvalidXmlChars(content);
        }

        private static string RemoveInvalidXmlChars(string text)
        {
            var validXmlChars = text.Where(ch => System.Xml.XmlConvert.IsXmlChar(ch)).ToArray();
            return new string(validXmlChars);
        }

        private bool IsValidXmlString(string text)
        {
            try
            {
                System.Xml.XmlConvert.VerifyXmlChars(text);
                return true;
            }
            catch (Exception e)
            {
                _logger.Info("Find InvalidXmlChars, Exception {0}", e);
                return false;
            }
        }

        private static string ResolveAuditTypeStr(int auditType)
        {
            if (!Enum.IsDefined(typeof(FSAuditType), auditType)) return auditType.ToString();
            var key = GetEnumDescription((FSAuditType)auditType);
            return I18NEntity.GetString(key);
        }

        private static FSAuditLevel ResolveAuditLevel(int auditLevel)
        {
            return auditLevel switch
            {
                1 => FSAuditLevel.ConnectionGroup,
                2 => FSAuditLevel.Connection,
                3 => FSAuditLevel.Folder,
                4 => FSAuditLevel.File,
                _ => FSAuditLevel.Unknown
            };
        }

        private static string DetectDirectoryPathOrFile(string path, bool isDetectFileName = false)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(path))
                {
                    _logger.Warn("Path is null or whitespace. Returning empty string as directory path.");
                    return path;
                }

                path = path.TrimEnd('\\', '/');

                var lastSeparatorIndex = path.LastIndexOfAny(new[] { '\\', '/' });

                if (lastSeparatorIndex <= 0) return path;

                if (isDetectFileName) return path.Substring(lastSeparatorIndex + 1);

                return path.Substring(0, lastSeparatorIndex);
            }
            catch (Exception ex)
            {
                _logger.Warn("Failed to detect directory path or file name from path", ex);
                return path;
            }
        }

        private List<string> GetRecordsSystemUserNames()
        {
            return new List<string> {
                I18NEntity.GetString("RM_TS_RunSchedule", CultureInfo.CreateSpecificCulture("en-US")),
                I18NEntity.GetString("RM_TS_RunSchedule", CultureInfo.CreateSpecificCulture("ja-JP")),
                I18NEntity.GetString("RM_TS_RunSchedule", CultureInfo.CreateSpecificCulture("ko-KR")),
                I18NEntity.GetString("RM_TS_RunSchedule", CultureInfo.CreateSpecificCulture("fr-FR")),
                I18NEntity.GetString("RM_TS_RunSchedule", CultureInfo.CreateSpecificCulture("fr-CA")),
                "RM_TS_RunSchedule"
            };
        }
        #endregion
    }
}
