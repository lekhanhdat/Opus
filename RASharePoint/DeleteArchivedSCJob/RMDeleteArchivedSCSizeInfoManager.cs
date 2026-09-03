using AvePoint.GCommon;
using AvePoint.RA.Common;
using AvePoint.RA.Contract.Common;
using AvePoint.RA.DB.Dao;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace AvePoint.RA.SharePoint.DeleteArchivedSCJob
{
    public class RMDeleteArchivedSCSizeInfoManager
    {
        private static readonly AveLogger _logger = AveLogger.GetInstance(typeof(RMDeleteArchivedSCJobHandler));

        private readonly IRMArchiveSiteInfoDao _archiveSiteInfoDao = PlatformWindsorManager.GetService<IRMArchiveSiteInfoDao>();
        private readonly IRMSiteDeletedSizeInfoDao _siteDeletedSizeInfoDao = PlatformWindsorManager.GetService<IRMSiteDeletedSizeInfoDao>();
        private readonly IRMArchiveTeamsGroupInfoDao _archiveTeamsInfoDao = PlatformWindsorManager.GetService<IRMArchiveTeamsGroupInfoDao>();

        // site info
        private string _siteUrl;
        private bool needUpdateSiteInfo = false;
        private double siteArchivedSize = 0d;
        private string siteSizeInfoId; // to delete

        // teams info
        private string _groupMailboxAddress;
        private bool needUpdateTeamsInfo = false;
        private double teamsArchivedSize = 0d;

        private readonly RMDeleteArchivedSCJobReportManager _reportManager;
        public RMDeleteArchivedSCSizeInfoManager(RMDeleteArchivedSCJobReportManager reportManager)
        {
            _reportManager = reportManager;
        }

        public async Task InitAsync(string siteUrl, string groupMailboxAddress)
        {
            _siteUrl = siteUrl;
            _groupMailboxAddress = groupMailboxAddress;

            _logger.Info($"Start to init size info for site [{_siteUrl}] and group mailbox [{_groupMailboxAddress}].");

            var siteInfo = _archiveSiteInfoDao.GetSiteInfoesBySiteUrls([_siteUrl]).FirstOrDefault();

            if (siteInfo != null)
            {
                siteArchivedSize = siteInfo.ArchivedSize;
                siteSizeInfoId = siteInfo.Id;
                needUpdateSiteInfo = true;
            }
            _logger.Info($"needUpdateSiteInfo: {needUpdateSiteInfo}. ArchivedSize is {siteArchivedSize} GB");

            if (!string.IsNullOrEmpty(groupMailboxAddress))
            {
                var teamsInfo = await _archiveTeamsInfoDao.GetArchiverInfoByGroupMailboxAsync(groupMailboxAddress);

                if (teamsInfo != null)
                {
                    teamsArchivedSize = teamsInfo.ArchivedSize;
                    needUpdateTeamsInfo = true;
                }
            }
            _logger.Info($"needUpdateTeamsInfo: {needUpdateTeamsInfo}. teamsArchivedSize is {teamsArchivedSize} GB");

        }

        // Update the archived size in site info and teams info, and delete the site deleted size info record.
        public async Task UpdateSizeInfoAsync(string archivedJobId, long totalMediaDataSize)
        {
            var siteDeletedSizeInfo = _siteDeletedSizeInfoDao.GetSiteDeleteSizeInfoBySiteUrlAndJobId(_siteUrl, archivedJobId);
            if (!needUpdateSiteInfo && !needUpdateTeamsInfo && siteDeletedSizeInfo == null)
            {
                return;
            }

            _logger.Info($"Start to update size info for site [{_siteUrl}] and group mailbox [{_groupMailboxAddress}] with archivedJobId: {archivedJobId} and totalMediaDataSize: {totalMediaDataSize} bytes.");

            var reduceArchivedSize = (double)totalMediaDataSize / ContractConstants.GBSizeInterval;

            if (needUpdateSiteInfo)
            {
                var tempArchivedSize = siteArchivedSize;
                siteArchivedSize = tempArchivedSize - reduceArchivedSize;
                _logger.Info($"Reduce archived size by {reduceArchivedSize}, from {tempArchivedSize} to {siteArchivedSize} GB.");
            }

            if (needUpdateTeamsInfo)
            {
                var tempTeamsArchivedSize = teamsArchivedSize;
                teamsArchivedSize = tempTeamsArchivedSize - reduceArchivedSize;
                _logger.Info($"Reduce Teams archived size by {reduceArchivedSize}, from {tempTeamsArchivedSize} to {teamsArchivedSize} GB.");
            }

            if (siteDeletedSizeInfo != null)
            {
                _logger.Info($"Delete site deleted size info for site: {_siteUrl}, JobId: {archivedJobId}, DeletedSize: {siteDeletedSizeInfo.DeletedSize} bytes.");
                _siteDeletedSizeInfoDao.DeleteByKey(siteDeletedSizeInfo.Id);
            }
        }

        public async Task CommitSizeChangesAsync(bool hasFailed)
        {
            _logger.Info($"Finish processing all master indexes for site: {_siteUrl}, remain archivedSize: {siteArchivedSize}, remain archivedSize: {teamsArchivedSize}");

            try
            {
                if (needUpdateSiteInfo)
                {
                    // if any site master remain , need to update size instead of delete the archive site info
                    // update archivedSize or just remove the record after finish process ?
                    if (hasFailed)
                    {
                        var updateSize = Math.Max(siteArchivedSize, 0);
                        _archiveSiteInfoDao.UpdateArchiverSize(_siteUrl, updateSize);
                        _logger.Info($"Update archived size to {updateSize} GB for site: {_siteUrl} since there are some failed master index to process.");
                    }
                    else
                    {
                        _archiveSiteInfoDao.DeleteByKey(siteSizeInfoId);
                        _logger.Info($"Delete archive site info for site: {_siteUrl} since all master index is processed successfully.");
                        await ProcessDeletedSizeInfo();
                    }
                }

                if (needUpdateTeamsInfo)
                {
                    var updateSize = Math.Max(teamsArchivedSize, 0);
                    await _archiveTeamsInfoDao.UpdateArchivedSizeByGroupMailboxAsync(_groupMailboxAddress, updateSize);
                    _logger.Info($"Update archived size to {updateSize} GB for teams: {_groupMailboxAddress}");
                }
            }
            catch (Exception e)
            {
                _logger.Error($"Error occurred while committing size changes for site: {_siteUrl} and group mailbox: {_groupMailboxAddress}. Error: {e}");
            }
            finally
            {
                _reportManager.IncreaseProgress();
            }
        }

        public async Task ProcessDeletedSizeInfo()
        {
            var deletedSizeInfoes = await _siteDeletedSizeInfoDao.GetSiteDeleteSizeInfoBySiteUrlAsync(_siteUrl);

            if (deletedSizeInfoes.IsNullOrEmpty())
            {
                _logger.Info($"No deleted size info remain found for site: {_siteUrl}.");
                return;
            }

            _logger.Info($"Site {_siteUrl} still has deleted size infoes remain need to be deleted, count:{deletedSizeInfoes.Count}. {string.Join(";", deletedSizeInfoes.Select(info => $"\nJobId:{info.JobId}, DeletedSize:{info.DeletedSize} GB"))}");

            await _siteDeletedSizeInfoDao.DeleteInfoBySiteUrl(_siteUrl);

            _logger.Info($"Delete deleted site info for site: {_siteUrl} successfully.");
        }
    }
}
