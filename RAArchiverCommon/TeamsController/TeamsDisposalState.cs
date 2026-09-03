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
using AvePoint.GCommon.Contract.Server.ControlPanel.Office365;
using AvePoint.RA.CommonUtil;
using System.Web;

namespace RAArchiverCommon.TeamsController
{
    public static class TeamsDisposalState
    {
        private static RALogger _logger = RALogger.GetInstance(typeof(TeamsDisposalState));

        private static Dictionary<string, TeamsChannelType> _allowDisposalChannelFolderURLs = new();
        private static List<string> _disposalSuccessfulChannelSites = new();
        private static List<string> _archivedChannelSites = new(); // used to save the archived channel site URLs, some channel site may archived but not deleted successfully

        private static List<string> _disposalSuccessfulChannel = new(); // used to save the disposal successful channel site URLs, skipped delete channel also considered as disposal successful channel if the job support include archived Teams

        public static bool AllowGroupSiteDisposal { get; set; }
        public static bool IsGroupDeleted { get; set; }
        public static bool IsExchangeDisposalSuccessful { get; set; }
        public static bool IsGroupSiteDisposalSuccessful { get; set; }

        public static bool IsSiteHasMatchRule { get; set; }

        public static bool IsTeamsArchived { get; set; }
        public static bool HasChannelSiteReadOnly { get; set; }

        public static IEnumerable<string> GetAllArchivedChannelSites()
        {
            return _archivedChannelSites.ToArray();
        }

        public static IEnumerable<string> GetAllDisposalSuccessfulChannelSites()
        {
            return _disposalSuccessfulChannel.ToArray();
        }

        public static bool HasSubJobFailed()
        {
            bool hasFailedChannelSite = false;
            foreach (var channelFolderUrl in _allowDisposalChannelFolderURLs.Keys)
            {
                if (!IsChannelSiteDisposalSuccessful(channelFolderUrl))
                {
                    hasFailedChannelSite = true;
                    _logger.Warn($"The channel site is not disposal successful: {channelFolderUrl}");
                }
            }

            return !IsGroupSiteDisposalSuccessful || !IsExchangeDisposalSuccessful || hasFailedChannelSite;
        }

        public static void AddAllowDisposalChannelSite(string channelFolderUrl, TeamsChannelType channelType)
        {
            var tempUrl = NormalizeUrl(channelFolderUrl);
            if (!string.IsNullOrEmpty(tempUrl))
            {
                _logger.Info($"AddAllowDisposalChannelSite: {tempUrl}");
                _allowDisposalChannelFolderURLs.TryAdd(tempUrl, channelType);
            }
        }

        public static bool IsAllowDisposalChannelSite(string channelSiteUrl, out TeamsChannelType channelType)
        {
            var tempUrl = NormalizeUrl(channelSiteUrl);
            foreach (var channelFolderUrl in _allowDisposalChannelFolderURLs)
            {
                if (channelFolderUrl.Key.StartsWith(tempUrl, StringComparison.OrdinalIgnoreCase))
                {
                    channelType = channelFolderUrl.Value;
                    return true;
                }
            }
            channelType = TeamsChannelType.None;
            return false;
        }

        public static void AddArchivedChannelSite(string channelSiteUrl)
        {
            if (_archivedChannelSites.Contains(channelSiteUrl))
            {
                _logger.Info($"Already add ArchivedChannelSite: {channelSiteUrl}");
                return;
            }
            _logger.Info($"AddArchivedChannelSite: {channelSiteUrl}");
            _archivedChannelSites.Add(channelSiteUrl);
        }

        public static void AddDisposalSuccessfulChannelSite(string channelSiteUrl)
        {
            var tempUrl = NormalizeUrl(channelSiteUrl);
            if (_disposalSuccessfulChannelSites.Contains(tempUrl))
            {
                _logger.Info($"Already add DisposalSuccessfulChannelSite: {tempUrl}");
                return;
            }
            _logger.Info($"AddDisposalSuccessfulChannelSite: {tempUrl}");
            _disposalSuccessfulChannelSites.Add(tempUrl);
        }

        public static bool IsChannelSiteDisposalSuccessful(string channelFolderUrl)
        {
            var tempUrl = NormalizeUrl(channelFolderUrl);
            foreach (var channelSiteUrl in _disposalSuccessfulChannelSites)
            {
                if (tempUrl.StartsWith(channelSiteUrl, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }

        public static void AddDisposalSuccessfulChannel(string filesFolderUrl)
        {
            var tempUrl = NormalizeUrl(filesFolderUrl);
            // check if channel site is backup and mark as disposal successfully
            foreach (var channelSiteUrl in _disposalSuccessfulChannelSites)
            {
                if (tempUrl.StartsWith(channelSiteUrl, StringComparison.OrdinalIgnoreCase))
                {
                    if (_disposalSuccessfulChannel.Contains(channelSiteUrl))
                    {
                        _logger.Info($"Already add DisposalSuccessfulChannel: {channelSiteUrl}");
                        return;
                    }

                    // add the channel site url instead of the files folder url
                    _disposalSuccessfulChannel.Add(channelSiteUrl);
                    _logger.Info($"AddDisposalSuccessfulChannel: {channelSiteUrl}");
                    return;
                }
            }
            _logger.Warn($"AddDisposalSuccessfulChannel: {tempUrl} not found in disposal successful channel sites.");
        }

        // add '/' at the end to prevent mismatch channel site. Ex: pri1 vs pri10
        private static string NormalizeUrl(string url)
        {
            if (string.IsNullOrEmpty(url)) return string.Empty;
            string decoded = HttpUtility.UrlDecode(url);
            return decoded.TrimEnd('/') + "/";
        }
    }
}
