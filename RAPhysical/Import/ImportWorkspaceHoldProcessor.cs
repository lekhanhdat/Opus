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
using AvePoint.GCommon.Contract.ExchangeOnline.ExchangeOnlineMailbox.Object;
using AvePoint.GCommon.Contract.Server.ControlPanel.Office365;
using AvePoint.GCommon.Contract.Tree.Object;
using AvePoint.GCommon.Utility;
using AvePoint.RA.Common;
using AvePoint.RA.Common.Report;
using AvePoint.RA.Common.SystemSetting;
using AvePoint.RA.Common.Util;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.Contract.JobMonitor;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using AvePoint.RA.Contract.RMWeb.ReportCenter;
using AvePoint.RA.Contract.RMWeb.Tree.Base;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Dao.Extension;
using AvePoint.RA.DB.Dao.Impl;
using AvePoint.RA.DB.Model;
using AvePoint.RA.I18N.Core;
using DnsClient.Protocol;
using DocumentFormat.OpenXml.Spreadsheet;
using Google.Apis.Admin.Directory.directory_v1.Data;
using Google.Apis.Vault.v1.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace AvePoint.RA.RAPhysical.Import
{
    public class ImportWorkspaceHoldProcessor
    {
        private static readonly AvePoint.RA.Contract.Services.IRALogger logger = RALogger.GetInstance(typeof(ImportWorkspaceHoldProcessor));

        private static readonly IJobInfoUpdater JobInfoUpdater = (IJobInfoUpdater)PlatformWindsorManager.GetService(typeof(IJobInfoUpdater));
        private static readonly IRMReportManager ReportManager = ReportMangerFactory.Instance.ReportManager;
        private static readonly IRMSubJobDao SubJobDao = (IRMSubJobDao)PlatformWindsorManager.GetService(typeof(IRMSubJobDao));
        private static readonly IExplorerService ExplorerService = (IExplorerService)PlatformWindsorManager.GetService(typeof(IExplorerService));
        private static readonly IHoldDao HoldDao = (IHoldDao)PlatformWindsorManager.GetService(typeof(IHoldDao));
        private static readonly RA.DB.Explorer.Dao.IExplorerDao ExplorerDao = new RA.DB.Explorer.Dao.CosmosImp.ExplorerDao();
        private static readonly IUserService UserService = (IUserService)PlatformWindsorManager.GetService(typeof(IUserService));
        private static readonly IRMRemoteNodeService RMRemoteNodeService = (IRMRemoteNodeService)PlatformWindsorManager.GetService(typeof(IRMRemoteNodeService));
        private static readonly IWorkplaceHoldDao WorkplaceHoldDao = (IWorkplaceHoldDao)PlatformWindsorManager.GetService(typeof(IWorkplaceHoldDao));
        private IRMKeyValueDao RMKeyValueDao => PlatformWindsorManager.GetService<IRMKeyValueDao>();
        private static readonly IRMMailboxService RMMailboxService = (IRMMailboxService)PlatformWindsorManager.GetService(typeof(IRMMailboxService));
        //private static RMKeyValueDao RMKeyValueDao = (RMKeyValueDao)PlatformWindsorManager.GetService(typeof(RMKeyValueDao));
        private readonly string mJobId;
        private readonly string mParentJobId;
        private bool HasSucceedDetail { get; set; }
        private bool HasFailedDetail { get; set; }
        private string JobComment { get; set; }
        private const int NodeLevel_SiteCollection = (int)NodeLevel.SiteCollection;
        private const int NodeLevel_WebApplication = (int)NodeLevel.WebApplication;
        private const int NodeLevel_SkyDrivePro = (int)NodeLevel.SkyDrivePro;
        private const int NodeLevel_SkyDriveProGroup = (int)NodeLevel.SkyDriveProGroup;
        private const int NodeLevel_O365GroupSites = (int)NodeLevel.O365GroupSites;
        private const int NodeLevel_O365GroupSitesGroup = (int)NodeLevel.O365GroupSitesGroup;
        private const int NodeLevel_PrivateChannel = (int)NodeLevel.PrivateChannel;
        private const int NodeLevel_SharedChannel = (int)NodeLevel.SharedChannel;
        private const int NodeLevel_PrivateChannelSitesGroup = (int)NodeLevel.PrivateChannelGroup;
        private static readonly string[] SharePointOnlineSources = { "SharePoint Online", "SPO" };
        private static readonly string[] TeamsSources = { "Teams", "TE" };
        private static readonly string[] ExchangeOnlineSources = { "Exchange Online", "EXO" };
        private static readonly string[] OneDriveSources = { "OneDrive", "OD" };

        private static readonly Dictionary<string, int> ContentSourceToFlag = BuildContentSourceToFlagMap();
        public ImportWorkspaceHoldProcessor(string jobId, string parentJobId)
        {
            mJobId = jobId;
            mParentJobId = parentJobId;
            ReportMangerFactory.Instance.Init(mJobId, JobType.ImportWorkspaceHold);
            JobInfoUpdater.UpdateJobState(mJobId, (int)JobStatus.InProgress);
            ReportManager.StartUpdateJobProgress();
        }

        private static Dictionary<string, int> BuildContentSourceToFlagMap()
        {
            var map = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

            AddAliases(map, SharePointOnlineSources, (int)RMBrowseTreeNodeSourceType.SharepointOnline);
            AddAliases(map, TeamsSources, (int)RMBrowseTreeNodeSourceType.Teams);
            AddAliases(map, ExchangeOnlineSources, (int)RMBrowseTreeNodeSourceType.Exchange);
            AddAliases(map, OneDriveSources, (int)RMBrowseTreeNodeSourceType.SkyDrivePro);

            return map;
        }

        private static void AddAliases(Dictionary<string, int> map, IEnumerable<string> aliases, int sourceFlag)
        {
            foreach (var alias in aliases)
            {
                if (!string.IsNullOrWhiteSpace(alias))
                {
                    map[alias.Trim()] = sourceFlag;
                }
            }
        }

        private static bool TryMapContentSource(string contentSource, out int sourceFlag)
        {
            sourceFlag = (int)SourceFlag.None;
            if (string.IsNullOrWhiteSpace(contentSource))
            {
                return false;
            }

            return ContentSourceToFlag.TryGetValue(contentSource.Trim(), out sourceFlag);
        }
        public async System.Threading.Tasks.Task RunAsync()
        {
            logger.Info($"ImportWorkspaceHoldProcessor start. JobId: {mJobId}, ParentJobId: {mParentJobId}.");
            RMSubJob subJob = SubJobDao.GetSubJob(mJobId, true);
            string blobName = subJob.JobContext?.Content;

            string localFilePath = Path.Combine(Path.GetTempPath(), $"HoldImport_{mJobId}.csv");
            try
            {
                RAStorageUtil.DownloadReportBlobToFile(blobName, localFilePath);

                await ProcessCsvAsync(localFilePath);

                if (!HasSucceedDetail && !HasFailedDetail)
                {
                    HasSucceedDetail = true;
                }

                logger.Info("Import hold records complete.");
            }
            catch (Exception ex)
            {
                HasFailedDetail = true;
                JobComment = ex.Message;
                logger.Error($"Import hold records failed, error: {ex}");
            }
            finally
            {
                TryDeleteLocalFile(localFilePath);
                var jobFinishStatus = HasSucceedDetail && HasFailedDetail
                    ? JobStatus.FinishWithException
                    : (HasFailedDetail ? JobStatus.Failed : JobStatus.Finished);
                ReportManager.SetJobFinished(jobFinishStatus, JobComment);
            }
        }

        private async Task ProcessCsvAsync(string filePath)
        {
            logger.Info($"Start parsing hold import CSV. FilePath: {filePath}");
            var loggedInUser = WebUtil.LogonUserDisplayName;

            var allHolds = HoldDao.GetAllHolds((int)HoldProfileType.All)?.ToDictionary(h => h.Name?.Trim(), h => h, StringComparer.OrdinalIgnoreCase)
                           ?? new Dictionary<string, RMHold>(StringComparer.OrdinalIgnoreCase);

            using var reader = new StreamReader(filePath, System.Text.Encoding.UTF8);
            string headerLine = await reader.ReadLineAsync();
            if (string.IsNullOrWhiteSpace(headerLine))
            {
                logger.Warn("CSV file is empty or missing header.");
                return;
            }

            ValidateHeaders(headerLine);
            var headerIndexes = BuildHeaderIndex(headerLine);
            var requiredKeys = new[]
            {
                "Hold title",
                "Type",
                "URL",
            };

            var missingColumns = requiredKeys
                .Where(col => !headerIndexes.ContainsKey(NormalizeHeader(col)))
                .ToList();

            if (missingColumns.Count > 0)
            {
                var missing = string.Join(", ", missingColumns);
                logger.Warn($"CSV header is missing required columns: {missing}.");
                throw new Exception("CSV header is missing required columns.");
            }

            int holdTitleIndex = headerIndexes[NormalizeHeader("Hold title")];
            int sourceTypeIndex = headerIndexes[NormalizeHeader("Type")];
            int locationUrlIndex = headerIndexes[NormalizeHeader("URL")];
            int holdByIndex = headerIndexes[NormalizeHeader("Placed on hold by")];

            logger.Info($"CSV header indexes resolved. Url={locationUrlIndex}, sourceTypeIndex={sourceTypeIndex}, HoldTitle={holdTitleIndex}, HoldBy={holdByIndex}.");

            int rowCount = 0;
            string line = string.Empty;
            bool special = false;
            string rowStr = string.Empty;
            var rows = new List<WorkspaceHoldCsvRowItem>();

            while ((line = await reader.ReadLineAsync()) != null)
            {
                if (string.IsNullOrWhiteSpace(line))
                {
                    continue;
                }

                rowStr += line;
                int remainder = (line.Split(new char[] { '"' }, StringSplitOptions.None).Length - 1) % 2;
                if (remainder != 0)
                {
                    if (special)
                    {
                        special = false;
                    }
                    else
                    {
                        rowStr += Environment.NewLine;
                        special = true;
                        continue;
                    }
                }
                else if (special)
                {
                    rowStr += Environment.NewLine;
                    continue;
                }

                rowCount++;
                var columns = CSVHelper.AnalyseCSVRow2Array(rowStr);
                rowStr = string.Empty;
                string sourceType = GetColumnValue(columns, sourceTypeIndex);
                string locationUrl = GetColumnValue(columns, locationUrlIndex);
                string holdTitle = GetColumnValue(columns, holdTitleIndex);
                string holdBy = GetColumnValue(columns, holdByIndex);
                if (!string.IsNullOrWhiteSpace(holdBy))
                {
                    try
                    {
                        var account = await UserService.GetUserByNameAsync(holdBy);
                        if (account == null)
                        {
                            logger.Info($"Row {rowCount}: HoldBy user '{holdBy}' not found in Opus. Use import user '{loggedInUser}'.");
                            holdBy = loggedInUser;
                        }
                        else
                        {
                            holdBy = !string.IsNullOrWhiteSpace(account.DisplayName) ? account.DisplayName : (!string.IsNullOrWhiteSpace(account.Email) ? account.Email : holdBy);
                        }
                    }
                    catch (Exception ex)
                    {
                        logger.Warn($"Row {rowCount}: Validate HoldBy user '{holdBy}' failed: {ex.Message}. Use import user '{loggedInUser}'.");
                        holdBy = loggedInUser;
                    }
                }
                else
                {
                    holdBy = loggedInUser;
                }

                if (string.IsNullOrWhiteSpace(holdTitle))
                {
                    logger.Warn($"Row {rowCount}: Hold title is empty.");
                    AddJobDetail(locationUrl, sourceType, holdTitle, JobDetailsStatus.Failed, "RM_PRM_PRE_Missing_HoldType");
                    continue;
                }

                if (!allHolds.TryGetValue(holdTitle.Trim(), out RMHold targetHold))
                {
                    logger.Warn($"Row {rowCount}: Hold '{holdTitle}' not found.");
                    AddJobDetail(locationUrl, sourceType, holdTitle, JobDetailsStatus.Failed, "RM_PRM_PRE_NotFound_HoldType");
                    continue;
                }
                rows.Add(new WorkspaceHoldCsvRowItem
                {
                    SourceType = sourceType,
                    LocationUrl = locationUrl,
                    HoldTitle = holdTitle,
                    HoldBy = holdBy,
                    TargetHold = targetHold
                });
            }

            await SaveWorkspaceHoldsAsync(rows);
            logger.Info($"Finished parsing hold import CSV. TotalRowsParsed={rowCount}.");
        }

        private async Task SaveWorkspaceHoldsAsync(List<WorkspaceHoldCsvRowItem> rows)
        {
            if (rows == null || rows.Count == 0)
            {
                return;
            }

            var workplacesByUrl = BuildWorkplacesByUrl(rows);
            var pendingInserts = new List<PendingWorkspaceHoldInsert>();
            var pendingKeys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (var row in rows)
            {
                if (!TryMapContentSource(row.SourceType, out int sourceType))
                {
                    AddJobDetail(row.LocationUrl, row.SourceType, row.HoldTitle, JobDetailsStatus.Failed, "RM_JS_RDM_SourceType_Invalid");
                    continue;
                }

                if (!workplacesByUrl.TryGetValue(row.LocationUrl, out var workplaceId) || string.IsNullOrWhiteSpace(workplaceId))
                {
                    AddJobDetail(row.LocationUrl, row.SourceType, row.HoldTitle, JobDetailsStatus.Failed, "RM_JS_RDM_Url_NotFound");
                    continue;
                }

                var workspaceRequest = new WorkspaceRequestDto
                {
                    WorkplaceId = workplaceId,
                    HoldId = row.TargetHold.Id,
                    SourceType = sourceType,
                };

                if (WorkplaceHoldDao.CheckWorkspaceHoldExist(workspaceRequest))
                {
                    AddJobDetail(row.LocationUrl, row.SourceType, row.HoldTitle, JobDetailsStatus.Skipped, "RM_JS_RDM_WorkplaceHold_Exist");
                    continue;
                }

                var rowKey = $"{workspaceRequest.WorkplaceId}_{workspaceRequest.HoldId}";
                if (!pendingKeys.Add(rowKey))
                {
                    AddJobDetail(row.LocationUrl, row.SourceType, row.HoldTitle, JobDetailsStatus.Skipped, "RM_JS_RDM_WorkplaceHold_Exist");
                    continue;
                }
                var holdsetting = new HoldSetting()
                {
                    Id = row.TargetHold.Id,
                    Type = (HoldDateType)row.TargetHold.HoldDateType,
                    Number = row.TargetHold.Number,
                    Unit = (HoldDateUnit)row.TargetHold.HoldUnit,
                    CalenderTime = new DateTime((row.TargetHold.CalendarTime)).ToString(JSDateTimeFormat.DEFAULT_TIME_FORMAT),
                    IsDayLightSaving = row.TargetHold.IsDaylightSaving,
                    TimeZoneId = row.TargetHold.TimeZoneId
                };
                var releaseTime = CalculateHoldReleaseTime(holdsetting);

                if (releaseTime.Ticks < DateTime.UtcNow.Ticks)
                {
                    AddJobDetail(row.LocationUrl, row.SourceType, row.HoldTitle, JobDetailsStatus.Failed, "RM_PRM_PRE_Msg_BeforeCurrentTime");
                    continue;
                }
                pendingInserts.Add(new PendingWorkspaceHoldInsert
                {
                    Row = row,
                    Entity = new RMWorkspaceHold
                    {
                        Id = Guid.NewGuid().ToString(),
                        WorkplaceId = workspaceRequest.WorkplaceId,
                        HoldId = workspaceRequest.HoldId,
                        HoldBy = row.HoldBy,
                        SourceType = workspaceRequest.SourceType,
                        ReleaseTime = releaseTime.Ticks
                    }
                });
            }

            if (pendingInserts.Count == 0)
            {
                return;
            }

            bool saved;
            try
            {
                saved = WorkplaceHoldDao.SaveWorkspaceHolds(pendingInserts.Select(x => x.Entity).ToList());
            }
            catch (Exception ex)
            {
                logger.Error($"Bulk save workspace holds failed. Count={pendingInserts.Count}, Error='{ex}'.");
                foreach (var item in pendingInserts)
                {
                    AddJobDetail(item.Row.LocationUrl, item.Row.SourceType, item.Row.HoldTitle, JobDetailsStatus.Failed, ex.Message);
                }
                return;
            }

            foreach (var item in pendingInserts)
            {
                AddJobDetail(item.Row.LocationUrl, item.Row.SourceType, item.Row.HoldTitle, saved ? JobDetailsStatus.Successful : JobDetailsStatus.Failed, saved ? string.Empty : "RM_JS_RDM_WorkplaceHold_Save_Failed");
            }
        }
        private DateTime CalculateHoldReleaseTime(HoldSetting hold)
        {
            if (hold.Type == HoldDateType.Custom)
            {
                DateTime tempNow = new DateTime();
                if (hold.Unit == HoldDateUnit.Day)
                {
                    tempNow = DateTime.UtcNow.AddDays(hold.Number);
                }
                else if (hold.Unit == HoldDateUnit.Week)
                {
                    tempNow = DateTime.UtcNow.AddDays(7 * hold.Number);
                }
                else if (hold.Unit == HoldDateUnit.Month)
                {
                    tempNow = DateTime.UtcNow.AddMonths(hold.Number);
                }
                else if (hold.Unit == HoldDateUnit.Years)
                {
                    tempNow = DateTime.UtcNow.AddYears(hold.Number);
                }
                return tempNow;
            }
            else
            {
                DateTime calenderTime = DateTime.Parse(hold.CalenderTime);
                calenderTime = DateTime.SpecifyKind(calenderTime, DateTimeKind.Unspecified);
                DateTime utcTime = DateTimeUtil.ConvertTimeToUtcDate(calenderTime, GeneralSettingConfig.FindSystemTimeZoneById(hold.TimeZoneId), !hold.IsDayLightSaving);
                return utcTime;
            }
        }
        private Dictionary<string, string> BuildWorkplacesByUrl(List<WorkspaceHoldCsvRowItem> rows)
        {
            var urls = rows
                .Select(r => r.LocationUrl)
                .Where(url => !string.IsNullOrWhiteSpace(url))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            if (urls.Count == 0)
            {
                return result;
            }

            var remoteSites = RMRemoteNodeService.GetRemoteSiteCollectionBySiteUrls(urls) ?? new List<RemoteSiteCollection>();
            foreach (var site in remoteSites.Where(s => s != null && !string.IsNullOrWhiteSpace(s.url) && !string.IsNullOrWhiteSpace(s.id)))
            {
                if (!result.ContainsKey(site.url))
                {
                    result[site.url] = site.id;
                }
            }

            var mailboxes = RMMailboxService.GetMailboxesByEmailAddressName(urls) ?? new List<EmailAccountDto>();
            foreach (var mailbox in mailboxes.Where(m => m != null && !string.IsNullOrWhiteSpace(m.Id)))
            {
                if (!string.IsNullOrWhiteSpace(mailbox.Email) && !result.ContainsKey(mailbox.Email))
                {
                    result[mailbox.Email] = mailbox.ObjectId;
                }

            }

            return result;
        }
        public RMBrowseTreeNodeSourceType GetSourceTypeByNodeLevel(int nodeLevel)
        {
            if (nodeLevel == NodeLevel_SkyDrivePro)
            {
                return RMBrowseTreeNodeSourceType.SkyDrivePro;
            }

            if (nodeLevel == NodeLevel_SiteCollection)
            {
                return RMBrowseTreeNodeSourceType.SharepointOnline;
            }

            bool isTeamsLevel = nodeLevel == NodeLevel_O365GroupSites ||
                                nodeLevel == NodeLevel_PrivateChannel ||
                                nodeLevel == NodeLevel_SharedChannel;

            if (isTeamsLevel)
            {
                if (RMKeyValueDao.HasUpgradeTeams())
                {
                    return RMBrowseTreeNodeSourceType.Teams;
                }

                return RMBrowseTreeNodeSourceType.SharepointOnline;
            }

            return RMBrowseTreeNodeSourceType.Exchange;
        }

        private class WorkspaceHoldCsvRowItem
        {
            public string SourceType { get; set; }
            public string LocationUrl { get; set; }
            public string HoldTitle { get; set; }
            public string HoldBy { get; set; }
            public RMHold TargetHold { get; set; }
        }

        private class PendingWorkspaceHoldInsert
        {
            public WorkspaceHoldCsvRowItem Row { get; set; }
            public RMWorkspaceHold Entity { get; set; }
        }


        private static void ValidateHeaders(string headerLine)
        {
            var headers = CSVHelper.AnalyseCSVRow2Array(headerLine);
            var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var duplicates = new List<string>();

            foreach (var header in headers)
            {
                var key = NormalizeHeader(header);
                if (string.IsNullOrWhiteSpace(key))
                {
                    continue;
                }

                if (!seen.Add(key))
                {
                    duplicates.Add(header.Trim());
                }
            }
            if (duplicates.Count > 0)
            {
                var dup = string.Join(", ", duplicates.Distinct());
                logger.Warn($"CSV header contains duplicate columns: {dup}.");
                throw new Exception($"CSV header contains duplicate columns: {dup}.");
            }
        }

        private static Dictionary<string, int> BuildHeaderIndex(string headerLine)
        {
            var headers = CSVHelper.AnalyseCSVRow2Array(headerLine);
            var indexMap = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            for (int i = 0; i < headers.Length; i++)
            {
                var key = NormalizeHeader(headers[i]);
                if (!indexMap.ContainsKey(key))
                {
                    indexMap.Add(key, i);
                }
            }
            return indexMap;
        }

        private static string GetColumnValue(string[] columns, int index)
        {
            if (index >= 0 && index < columns.Length)
            {
                return columns[index]?.Trim() ?? string.Empty;
            }

            return string.Empty;
        }

        private static string NormalizeHeader(string header)
        {
            if (string.IsNullOrWhiteSpace(header))
            {
                return string.Empty;
            }

            return header.Trim().Replace(" ", string.Empty).Replace("/", string.Empty).Replace("_", string.Empty).ToLowerInvariant();
        }

        private void AddJobDetail(string url, string type, string holdTitle, JobDetailsStatus status, string comment)
        {
            ReportManager.SendJobDetail(new JMWorkspaceHoldImportJobDetail
            {
                Url = url,
                Type = type,
                HoldTitle = holdTitle,
                Status = status,
                Comment = comment
            });

            if (status == JobDetailsStatus.Failed)
            {
                HasFailedDetail = true;
            }
            else
            {
                HasSucceedDetail = true;
            }
        }

      
        private static void TryDeleteLocalFile(string filePath)
        {
            try
            {
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                }
            }
            catch (Exception ex)
            {
                logger.Warn($"Failed to delete temp file '{filePath}': {ex.Message}");
            }
        }
    }
}
