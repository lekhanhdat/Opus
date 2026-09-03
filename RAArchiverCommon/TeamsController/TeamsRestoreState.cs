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
using AvePoint.RA.CommonUtil;
using System.Web;
using System.Collections.Concurrent;

namespace RAArchiverCommon.TeamsController
{
    public static class TeamsRestoreState
    {
        private static RALogger _logger = RALogger.GetInstance(typeof(TeamsRestoreState));

        private static Dictionary<string, bool> _allowRestoreChannelFolderURLs = new ();
        public static ConcurrentDictionary<string, string> mappingSiteURLs = new(StringComparer.OrdinalIgnoreCase);
        private static List<string> _restoreSuccessfulChannelSites = new ();

        public static bool IsAllowRestoreGroupSite { get; set; }
        public static bool IsGroupSiteRestoreSuccessful { get; set; }
        public static string RestoreGroupSite { get; set; } = string.Empty;
        public static bool IsGroupSiteNewlyCreated { get; set; }
        // consistent for Teams and all related sites 
        public static bool? IsEnableMigrationImportJob { get; set; }

        public static bool IsChannelSiteReadOnly { get; set; }

        public static bool HasSubJobFailed()
        {
            bool hasFailedChannelSite = false;
            foreach (var channelFolderUrl in _allowRestoreChannelFolderURLs.Keys)
            {
                if(!IsChannelSiteRestoreSuccessful(channelFolderUrl))
                {
                    hasFailedChannelSite = true;
                    _logger.Warn($"The channel site is not restore successful: {channelFolderUrl}");
                }
            }

            return !IsGroupSiteRestoreSuccessful || hasFailedChannelSite;
        }

        public static void AddAllowRestoreChannelSite(string channelFolderUrl, bool isNewCreate)
        {
            var normalizedUrl = NormalizeUrl(channelFolderUrl);
            if (!string.IsNullOrEmpty(channelFolderUrl))
            {
                _allowRestoreChannelFolderURLs.TryAdd(normalizedUrl, isNewCreate);
                _logger.Info($"Add allow restore channel site: {normalizedUrl}, isNewCreate: {isNewCreate}");
            }
        }

        public static bool IsAllowRestoreChannelSite(string channelSiteUrl)
        {
            var tempUrl = NormalizeUrl(channelSiteUrl) + "/";
            foreach (var channelFolderUrl in _allowRestoreChannelFolderURLs.Keys)
            {
                // add / for both to make sure matching correctly
                if ((channelFolderUrl + "/").StartsWith(tempUrl, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }

        public static void AddRestoreSuccessfulChannelSite(string channelSiteUrl, bool isNewCreate)
        {
            _allowRestoreChannelFolderURLs.TryAdd(channelSiteUrl, isNewCreate);
        }

        public static bool IsChannelSiteRestoreSuccessful(string channelFolderUrl)
        {
            var tempUrl = NormalizeUrl(channelFolderUrl) + "/";
            foreach (var channelSiteUrl in _allowRestoreChannelFolderURLs)
            {
                if ((channelSiteUrl.Key + "/").StartsWith(tempUrl, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }

        public static bool IsChannelSiteDefaultLibrary(string channelFolderUrl, out bool isNewlyCreated)
        {
            isNewlyCreated = false;
            if (_allowRestoreChannelFolderURLs.IsNullOrEmpty() || string.IsNullOrEmpty(channelFolderUrl)) return false;
            var tempUrl = NormalizeUrl(channelFolderUrl) + "/";
            foreach (var channelSiteUrl in _allowRestoreChannelFolderURLs)
            {
                if ((channelSiteUrl.Key + "/").StartsWith(tempUrl, StringComparison.OrdinalIgnoreCase))
                {
                    isNewlyCreated = channelSiteUrl.Value;
                    return true;
                }
            }
            return false;
        }

        public static bool IsNewCreateSite(string siteUrl)
        {
            if (RestoreGroupSite.Equals(siteUrl, StringComparison.OrdinalIgnoreCase))
            {
                return IsGroupSiteNewlyCreated;
            }
            var tempUrl = NormalizeUrl(siteUrl) + "/";
            foreach (var channelFolderUrl in _allowRestoreChannelFolderURLs)
            {
                if ((channelFolderUrl.Key + "/").StartsWith(tempUrl, StringComparison.OrdinalIgnoreCase))
                {
                    return channelFolderUrl.Value;
                }
            }
            return false;
        }

        private static string NormalizeUrl(string url)
        {
            if (string.IsNullOrEmpty(url)) return string.Empty;
            string decoded = HttpUtility.UrlDecode(url);
            return decoded.TrimEnd('/');
        }
    }
}
