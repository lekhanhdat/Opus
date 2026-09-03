using AvePoint.GCommon.Contract.Tree.Object;
using AvePoint.GCommon.Utility.Cloud;
using AvePoint.RA.Common;
using AvePoint.RA.Common.Report;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.Contract.RMWeb.Explorer;
using AvePoint.RA.Contract.TemplateManagement;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Explorer.Dao.CosmosImp;
using AvePoint.RA.DB.Explorer.Model;
using AvePoint.RA.DB.SQLiteDB.Reporting.Runtime;
using AvePoint.RA.Service.Services.Explorer;
using AvePoint.RA.SharePoint.Common;
using AvePoint.RA.SharePoint.RMExplorer;
using AvePoint.Wrapper.Common;
using ExchangeUtility.Graph;
using Microsoft365.Authentication;
using RAGlobalSearch.Export;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace AvePoint.RA.Service.Services.SharePoint.Report
{
    public class RMSharePointSiteMetricsReportProcessor
    {
        private readonly RALogger _logger = RALogger.GetInstance(typeof(RMSharePointSiteMetricsReportProcessor));
        private IRMRemoteNodeDao RemoteNodeDao => PlatformWindsorManager.GetService<IRMRemoteNodeDao>();
        private ITenantService TenantService => PlatformWindsorManager.GetService<ITenantService>();

        private const int QueryPageSize = 2000;
        private readonly IRMReportManager _reportManager;
        private readonly string _dbPath;

        private readonly SiteMetricsExportPayload _exportPayload;
        private readonly RMExplorerUtility _explorerUtility;
        private readonly ExplorerQueryService _explorerQueryService;
        private readonly ExportSearchResult _projector;


        public RMSharePointSiteMetricsReportProcessor(IRMReportManager reportManager, string dbPath, SiteMetricsExportPayload payload)
        {
            _reportManager = reportManager;
            _dbPath = dbPath;
            _exportPayload = payload;
            _explorerUtility = new RMExplorerUtility();
            _explorerQueryService = new ExplorerQueryService();
            _projector = new ExportSearchResult(new GlobalSearchExportDto());
        }

        public async Task ExecuteAsync()
        {
            _logger.Info("Starting SharePoint report export processor. SiteCount={0}", _exportPayload.SiteExportTargets.Count);
            var selectedColumns = BuildAllColumnsSelection();

            var (rowCtx, headers) = await _projector.BuildReportRowContextAndHeadersAsync(selectedColumns);

            using (var explorerDao = new ExplorerDao())
            using (var writer = new RMSharePointSiteMetricsReportSQLiteWriter(_dbPath, batchSize: 2000))
            {
                await writer.InitializeAsync(headers);
                foreach (var target in _exportPayload.SiteExportTargets)
                {
                    var timming = Stopwatch.StartNew();
                    timming.Start();

                    _logger.Info("Start exporting site. SiteUrl={0}", target.SiteCollectionUrl);
                    var count = await IngestSingleSiteAsync(explorerDao, _projector, writer, rowCtx, target);

                    timming.Stop();
                    _logger.Info("Finished exporting site. SiteUrl={0}, TotalProcessed={1}, TimeElapsed={2}ms",
                        target.SiteCollectionUrl, count, timming.ElapsedMilliseconds);
                }
            }

            await UploadReportFileToDestinationLibraryAsync();
            _logger.Info("SharePoint report export processor completed.");
        }

        private async Task<long> IngestSingleSiteAsync(ExplorerDao explorerDao, ExportSearchResult projector, RMSharePointSiteMetricsReportSQLiteWriter writer, ExportRowBuilderContext rowCtx, SiteExportTarget target)
        {
            long siteProcessed = 0;
            var continuation = string.Empty;

            var drainedRecords = DrainRecordsByPage(explorerDao, target.SiteCollectionId, continuation, QueryPageSize);
            foreach (var records in drainedRecords)
            {
                if (records.Count == 0) break;

                _reportManager.IncreaseBase(records.Count);

                var dtoBatch = await _explorerQueryService.Convert2BaseDtoAsync(records, target.SiteCollectionUrl);

                var rows = projector.BuildRowsFromContext(dtoBatch, rowCtx);

                await writer.WriteAsync(rows, CancellationToken.None);

                siteProcessed += rows.Length;
                _reportManager.Increase(rows.Length);
                _logger.Info("Site export progress. SiteUrl={0}, Processed={1}", target.SiteCollectionUrl, rows.Length);
            }

            return siteProcessed;
        }

        private IEnumerable<List<Record>> DrainRecordsByPage(ExplorerDao explorerDao, string siteCollectionId, string continuationToken, int pageSize = 1000)
        {
            Expression<Func<Record, bool>> predicate = r => r.SourceFlag == (int)SourceFlag.SharePoint
                                 && r.AveSiteId == siteCollectionId
                                 && (r.NodeType == (int)NodeLevel.Item || r.NodeType == (int)NodeLevel.Document)
                                 && (r.RecordStatus == (int)RMRecordStatus.Active || r.RecordStatus == (int)RMRecordStatus.Archived);
            do
            {
                var result = explorerDao.QueryByPage(predicate, pageSize, continuationToken);
                continuationToken = result.Item2;
                yield return result.Item1.ToList();
            } while (!string.IsNullOrEmpty(continuationToken));
        }

        private async Task UploadReportFileToDestinationLibraryAsync()
        {
            try
            {
                _logger.Info("Start uploading exported report to SharePoint library. LibraryUrl={0}", _exportPayload.DestinationLibUrl);
                var parentSiteUrl = _explorerUtility.GetSiteCollectionUrlFromListUrl1(_exportPayload.DestinationLibUrl);
                var remoteNodeSite = RemoteNodeDao.GetRemoteSiteCollectionByUrl(parentSiteUrl);
                var bposInfo = await PoolUserUtil.GetBPOSInfoAsync(remoteNodeSite);
                var siteUrl = parentSiteUrl;
                using (var aveSite = new Wrapper.Restore.AveSPSite(parentSiteUrl, parentSiteUrl, AveContextKind.Auto, bposInfo))
                {
                    siteUrl = ResolveActualSiteUrl(_exportPayload.DestinationLibUrl, aveSite.SPSite);
                }
                var fileName = Path.GetFileName(_dbPath);
                var relativePath = _exportPayload.DestinationLibUrl.Substring(siteUrl.Length).TrimStart('/');
                var tokenProvider = ConvertToTokenProvider(bposInfo);
                var uploader = new TeamsFileUploader(siteUrl, relativePath, tokenProvider);
                using var fileStream = File.Open(_dbPath, FileMode.Open, FileAccess.Read);
                const long underLimit = 5 * 1024 * 1024; //Byte
                if (fileStream.Length > underLimit)
                {
                    _logger.Info("File size {0}B exceeds {1}B threshold. Initiating chunked upload pipeline for {2}.", fileStream.Length, underLimit, fileName);
                    uploader.UploadFileByChunkToDocumentLibrary(string.Empty, fileName, fileStream, true, (int)underLimit, 10, 2000);
                }
                else
                {
                    _logger.Info("File size {0}B bytes is within {1}B. Initiating standard direct upload for {2}.", fileStream.Length, underLimit, fileName);
                    uploader.UploadFileToDocumentLibrary(string.Empty, fileName, fileStream, true);
                }
                CheckInDestinationFile(siteUrl, fileName, bposInfo);
                _logger.Info("Finished uploading exported report to SharePoint library. LibraryUrl={0}", _exportPayload.DestinationLibUrl);
            }
            catch (Exception ex)
            {
                _logger.Error("Failed to get BPOS info for site. Url={0}, Error={1}", _exportPayload.DestinationLibUrl, ex);
                throw;
            }
        }

        private string ResolveActualSiteUrl(string destinationUrl, IAveSite site)
        {
            var uri = new Uri(destinationUrl);
            string[] siteTypes = { "sites", "teams", "personal" };
            var segments = uri.AbsolutePath.Trim('/').Split('/', StringSplitOptions.RemoveEmptyEntries);
            string lastFound = $"{uri.Scheme}://{uri.Host}";
            for (int i = 0; i < segments.Length; i++)
            {
                if (i == 0 && siteTypes.Contains(segments[i], StringComparer.OrdinalIgnoreCase)) continue;
                var serverRelativeUrl = "/" + string.Join("/", segments.Take(i + 1));
                var candidateUrl = $"{uri.Scheme}://{uri.Host}{serverRelativeUrl}";
                if (!WebExists(site, serverRelativeUrl)) break;
                lastFound = candidateUrl;
            }
            return lastFound;
        }

        private bool WebExists(IAveSite site, string serverRelativeUrl)
        {
            try
            {
                using IAveWeb web = site.OpenWeb(serverRelativeUrl);
                return web != null && web.Exists;
            }
            catch (Exception ex)
            {
                _logger.Info("Failed to probe sub-site existence. Url={0}, Error={1}", serverRelativeUrl, ex);
                return false;
            }
        }

        private void CheckInDestinationFile(string siteUrl, string fileName, AveBPOSAccountInfo accountInfo)
        {
            try
            {
                var aveSite = new Wrapper.Restore.AveSPSite(siteUrl, siteUrl, AveContextKind.Auto, accountInfo);
                var aveWeb = new Wrapper.Restore.AveSPWeb(aveSite);
                var fullUrl = $"{_exportPayload.DestinationLibUrl.TrimEnd('/')}/{fileName}";
                IAveFile file = aveWeb.SPWeb.GetFile(Uri.UnescapeDataString(fullUrl)) ?? throw new Exception($"Destination file not found. FileUrl={fullUrl}");
                _logger.Info("File info. UniqueId={0}, CheckOutType={1}, Url={2}", file.UniqueId, file.CheckOutType, fullUrl);
                if (file.CheckOutType != AveCheckOutType.None)
                {
                    if (file.UniqueId == Guid.Empty)
                    {
                        _logger.Info("File have no UniqueId. Url={0}", siteUrl);
                        return;
                    }
                    _logger.Info("Enforcing Check-In. Url={0}, File={1}", siteUrl, file.UniqueId);
                    file.CheckIn("", AveCheckinType.MajorCheckIn);
                    _logger.Info("Check in file successful. Url={0}, File={1}", siteUrl, file.UniqueId);
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Failed to enforce check-in.", ex);
            }
        }

        private static ITokenProvider ConvertToTokenProvider(AveBPOSAccountInfo info)
        {
            ITokenProvider provider = null;
            if (info != null)
            {
                if (info.TokenProvider != null)
                {
                    provider = info.TokenProvider;
                }
                else if (!string.IsNullOrEmpty(GCommonRoleConfiguration.AosTokenApiURL))
                {
                    provider = TokenProviderFactory.GetInstance().Get(info);
                }
                else
                {
                    throw new Exception("Token api url is null");
                }
            }
            return provider;
        }

        private List<SelectedColumn> BuildAllColumnsSelection()
        {
            var buildinCols = new[]
            {
                (RecordBuildInColumnIds.SPOLocation, "Location"),
                (RecordBuildInColumnIds.NameOrTitle, "Name"),
                (RecordBuildInColumnIds.UniqueId, nameof(RecordBuildInColumnIds.UniqueId)),
                (RecordBuildInColumnIds.Type, nameof(RecordBuildInColumnIds.Type)),
                (RecordBuildInColumnIds.Classification, nameof(RecordBuildInColumnIds.Classification)),
                (RecordBuildInColumnIds.RuleName, nameof(RecordBuildInColumnIds.RuleName)),
                (RecordBuildInColumnIds.RuleAction, nameof(RecordBuildInColumnIds.RuleAction)),
                (RecordBuildInColumnIds.HoldStatus, "OnHold"),
                (RecordBuildInColumnIds.HoldBy, "PlacedOnHoldBy"),
                (RecordBuildInColumnIds.HoldTitle.ToUpperInvariant(), nameof(RecordBuildInColumnIds.HoldTitle)),
                (RecordBuildInColumnIds.HoldUntil.ToUpperInvariant(), "HoldExpirationTime"),
                (RecordBuildInColumnIds.ActionDueDate, TenantService.IsCustomizationAppTenant() ? "RecordRetentionEndDate" : "ActionDueDate"),
                (RecordBuildInColumnIds.Owners, "RecordReviewer"),
                (RecordBuildInColumnIds.CreatedDateInfo, "CreatedTime"),
                (RecordBuildInColumnIds.DeclaredRecord, nameof(RecordBuildInColumnIds.DeclaredRecord)),
                (RecordBuildInColumnIds.LockedByRecordLabel, "RecordsLabel"),
                (RecordBuildInColumnIds.CreatedBy, nameof(RecordBuildInColumnIds.CreatedBy)),
                (RecordBuildInColumnIds.ModifiedBy, nameof(RecordBuildInColumnIds.ModifiedBy)),
                (RecordBuildInColumnIds.ModifiedTime, nameof(RecordBuildInColumnIds.ModifiedTime)),
                (RecordBuildInColumnIds.ArchivedTime, nameof(RecordBuildInColumnIds.ArchivedTime)),
                (RecordBuildInColumnIds.OnLoan, nameof(RecordBuildInColumnIds.OnLoan)),
                (RecordBuildInColumnIds.LoanBy, "CurrentHeldBy"),
            };

            var defaultCols = new[]
            {
                (DefaultColumnIDs.Coverage, nameof(DefaultColumnIDs.Coverage)),
                (DefaultColumnIDs.DateClosed, nameof(DefaultColumnIDs.DateClosed)),
                (DefaultColumnIDs.Description, nameof(DefaultColumnIDs.Description)),
                (DefaultColumnIDs.Format, nameof(DefaultColumnIDs.Format)),
                (DefaultColumnIDs.HomeLocation, nameof(DefaultColumnIDs.HomeLocation)),
                (DefaultColumnIDs.ProtectiveMarking, nameof(DefaultColumnIDs.ProtectiveMarking)),
                (DefaultColumnIDs.Rights, nameof(DefaultColumnIDs.Rights)),
                (DefaultColumnIDs.Capability, "Size"),
                (DefaultColumnIDs.Status, nameof(DefaultColumnIDs.Status)),
            };

            var allCols = buildinCols.Concat(defaultCols).ToList();
            return allCols.Select(c => new SelectedColumn
            {
                UniqueId = c.Item1,
                DisplayName = c.Item2
            }).ToList();
        }

        public class SiteExportTarget
        {
            public string SiteCollectionId { get; set; }

            public string SiteCollectionUrl { get; set; }
        }

        public class SiteMetricsExportPayload
        {
            public List<SiteExportTarget> SiteExportTargets { get; set; }
            public string DestinationLibUrl { get; set; }
        }
    }
}