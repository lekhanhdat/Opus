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
using AvePoint.GCommon;
using ExchangeUtility.Graph;

namespace RAArchiverCommon.TeamsController
{
    /// <summary>
    /// Build first before using Instance.
    /// Reset before switch to another channel under the same team
    /// </summary>
    public class TeamsArchiveStateTransitionScope : IDisposable
    {
        private static readonly AveLogger Logger = AveLogger.GetInstance(typeof(TeamsArchiveStateTransitionScope));
        private static readonly object _instanceLock = new object();
        private static TeamsArchiveStateTransitionScope? _instance;

        // teams data
        private readonly string _teamsId;
        private readonly MicrosoftTeamsWithGraph _appTeamsService;
        private readonly MicrosoftTeamsWithGraph _delegateTeamsService;
        private readonly bool _isTeamsArchived;

        // site state transition control
        private bool _hasAttemptedUnarchive;
        private bool _needRestoreArchive;
        private bool _originalSiteWasReadOnly;
        private bool _disposed;

        public bool IsInitialized { get; private set; }

        public static TeamsArchiveStateTransitionScope Instance
        {
            get
            {
                lock (_instanceLock)
                {
                    if (_instance == null)
                    {
                        _instance = new TeamsArchiveStateTransitionScope();
                    }
                    return _instance;
                }
            }
        }
        private TeamsArchiveStateTransitionScope()
        {
            IsInitialized = false;
            Logger.Debug("TeamsArchiveStateTransitionScope created in uninitialized state.");
        }
        private TeamsArchiveStateTransitionScope(string teamsId, MicrosoftTeamsWithGraph appService, MicrosoftTeamsWithGraph delegateService, bool isTeamsArchived)
        {
            _teamsId = teamsId;
            _appTeamsService = appService;
            _delegateTeamsService = delegateService;
            _isTeamsArchived = isTeamsArchived;
            IsInitialized = true;
        }

        public static void Build(string teamsId, MicrosoftTeamsWithGraph appService, MicrosoftTeamsWithGraph delegateService, bool isTeamsArchived)
        {
            lock (_instanceLock)
            {
                // Re-build if not initialized, or teamsId is different
                if (_instance == null || !_instance.IsInitialized || !string.Equals(_instance._teamsId, teamsId, StringComparison.OrdinalIgnoreCase))
                {
                    _instance = new TeamsArchiveStateTransitionScope(teamsId, appService, delegateService, isTeamsArchived);
                    Logger.Info($"TeamsArchiveStateTransitionScope built for Team: {teamsId}");
                }
            }
        }

        // Unarchive the team if it's currently archived and channel is read-only. This is needed for disposal to have the permission to delete data under the channel site
        public bool TryUnarchiveTeams(bool isArchiveTeamsChannelSiteReadOnly)
        {
            if (!IsInitialized) return false;

            if (_hasAttemptedUnarchive || _appTeamsService == null)
                return _needRestoreArchive;

            _hasAttemptedUnarchive = true;

            try
            {
                if (isArchiveTeamsChannelSiteReadOnly && _isTeamsArchived)
                {
                    _originalSiteWasReadOnly = true;
                    TeamsDisposalState.HasChannelSiteReadOnly |= true;
                    _needRestoreArchive = true;

                    Logger.Info($"Starting to unarchive team for disposal. TeamsId: {_teamsId}, OriginalSiteWasReadOnly: {_originalSiteWasReadOnly}");
                    _appTeamsService.UnarchiveTeam(_teamsId);
                    //Thread.Sleep(5 * 1000);
                    Logger.Info($"Successfully unarchived team for disposal. TeamsId: {_teamsId}");

                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                Logger.Error($"Failed to unarchive teams for disposal. TeamsId: {_teamsId}, Ex: {ex}");
                return false;
            }
        }

        // reset when go to the next channel, as the scope is shared for all channels under the same team
        public void Reset(string siteUrl = "")
        {
            Logger.Info($"Resetting TeamsArchiveStateTransitionScope for Team: {_teamsId}, site: {siteUrl}");
            _hasAttemptedUnarchive = false;
            _needRestoreArchive = false;
            _originalSiteWasReadOnly = false;
            _disposed = false;
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;

            if (!_needRestoreArchive || _delegateTeamsService == null) return;

            try
            {
                if (TeamsDisposalState.IsGroupDeleted && !_delegateTeamsService.IsTeamExist(_teamsId))
                {
                    Logger.Info($"Team {_teamsId} has been deleted, no need to restore archive state.");
                    return;
                }
                Logger.Info($"Restoring archive state for Teams:{_teamsId}. SetReadOnly: {_originalSiteWasReadOnly}");
                _delegateTeamsService.ArchiveTeam(_teamsId, _originalSiteWasReadOnly);
                //Thread.Sleep(5 * 1000);
                Logger.Info($"Successfully restored archive state for Teams:{_teamsId}.");
            }
            catch (Exception ex)
            {
                Logger.Error($"Failed to restore archive state for Teams:{_teamsId} in Dispose. Ex: {ex}");
            }
        }
    }
}
