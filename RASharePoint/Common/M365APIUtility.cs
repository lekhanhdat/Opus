using Aspose.Pdf.Operators;
using AvePoint.GCommon.Contract.Server.ControlPanel.Office365;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Services;
using AvePoint.RA.DB.DBLocker;
using AvePoint.RA.RACommonUtility;
using AvePoint.RA.RACommonUtility.Common;
using AvePoint.Wrapper.Common;
using ExchangeUtility.Graph;
using M365.Wrapper.Backup.Auth.Common;
using System;
using System.Threading;

namespace AvePoint.RA.SharePoint.Common
{
    public class M365APIUtility : IDisposable
    {
        private readonly IRALogger _logger = RALogger.GetInstance(typeof(M365APIUtility));

        private readonly RemoteSiteCollection _teamsNode;
        private readonly IAppTokenAuthObject _graphApplicationAuthObj;
        private readonly IAppTokenAuthObject _graphDelegateAuthObj;

        private bool _isTeamsUnarchivedForLockedChannelSite = false;
        private bool _isInitialized = false;
        public bool IsTeamsUnarchivedForLockedChannelSite => _isTeamsUnarchivedForLockedChannelSite;
        private SampleSharedDBLocker _sharedDBLocker;


        public M365APIUtility() 
        {
            _isInitialized = false;
            // Default constructor.
            // If the detail constructor is not called again, it means no need to do any operation on Teams or M365 group.
        }


        // for Teams job, the services are already created and passed in, so no need to create again
        public M365APIUtility( string groupMailboxAddress, string groupSiteUrl, string groupO365TenantId, string teamsId, 
            MicrosoftTeamsAPIBase m365TeamsService, MicrosoftTeamsAPIBase m365TeamsServiceForDelegate, Microsoft365GroupServiceBase m365GroupService)
        {
            var remoteSiteCollection = new RemoteSiteCollection()
            {
                url = groupSiteUrl,
                TenantId = groupO365TenantId,
            };
            _teamsNode = remoteSiteCollection;
            _teamsNode.TeamId = teamsId;
            _teamsNode.Name = groupMailboxAddress;

            _m365TeamsService = m365TeamsService;
            _m365TeamsServiceForDelegate = m365TeamsServiceForDelegate;
            _m365GroupService = m365GroupService;
            _isInitialized = true;
            _logger.Info($"M365APIUtility fully initialized with all Service for group mailbox: {_teamsNode.Name}, TeamId: {_teamsNode.TeamId}, TenantId: {_teamsNode.TenantId}");
        }

        public M365APIUtility(string groupMailboxAddress, string groupSiteUrl, string groupO365TenantId, string teamsId = null)
        {
            var remoteSiteCollection = new RemoteSiteCollection()
            {
                url = groupSiteUrl,
                TenantId = groupO365TenantId,
            };
            var bposInfo = CommonPoolUserUtil.GetBPOSInfoForTeams(remoteSiteCollection, true);
            _graphApplicationAuthObj = AuthObjectFactory4TeamsJob.GetGraphAuthObjectForDelegateCustomApp(bposInfo, TokenPermissionType.Application);
            _graphDelegateAuthObj = AuthObjectFactory4TeamsJob.GetGraphAuthObjectForDelegateCustomApp(bposInfo, TokenPermissionType.Delegated);
            
            if (string.IsNullOrEmpty(teamsId))
            {
                if (GroupService.TryGetO365GroupId(groupMailboxAddress, out string groupId))
                {
                    _logger.Info($"Successfully retrieved group ID: {groupId}");
                    teamsId = groupId;
                }
                else
                {
                    teamsId = string.Empty;
                }
            }

            _teamsNode = remoteSiteCollection;
            _teamsNode.TeamId = teamsId;
            _teamsNode.Name = groupMailboxAddress;
            _isInitialized = true;
            _logger.Info($"M365APIUtility initialized for group mailbox: {_teamsNode.Name}, TeamId: {_teamsNode.TeamId}, TenantId: {_teamsNode.TenantId}");
        }

        private MicrosoftTeamsAPIBase _m365TeamsService;
        public MicrosoftTeamsAPIBase TeamsService
        {
            get
            {
                _m365TeamsService ??= ExchangeServiceFactory.CreateExchangeMicrosoftTeams(_graphApplicationAuthObj);
                return _m365TeamsService;
            }
        }

        private MicrosoftTeamsAPIBase _m365TeamsServiceForDelegate;
        public MicrosoftTeamsAPIBase TeamsServiceForDelegate
        {
            get
            {
                _m365TeamsServiceForDelegate ??= ExchangeServiceFactory.CreateExchangeMicrosoftTeams(_graphDelegateAuthObj);
                return _m365TeamsServiceForDelegate;
            }
        }

        private Microsoft365GroupServiceBase _m365GroupService;
        public Microsoft365GroupServiceBase GroupService
        {
            get
            {
                _m365GroupService ??= ExchangeServiceFactory.CreateMicrosoft365Group(_graphApplicationAuthObj);
                return _m365GroupService;
            }
        }

        public bool TryArchiveTeams(bool makeSiteReadOnly)
        {
            if (!_isInitialized) return false;
            _logger.Info($"Start to try archive teams for group mailbox: {_teamsNode.Name}");
            try
            {
                return TryDoActionWithTryUnlockTeamsSite("ArchiveTeams", SiteState.ReadOnly, () =>
                {
                    var result = TeamsServiceForDelegate.ArchiveTeam(_teamsNode.TeamId, makeSiteReadOnly);
                    if (result)
                    {
                        _logger.Info($"Successfully sent archive request for the team. Waiting for the team to be archived.");
                        if (ValidateTeamsState(false))
                        {
                            return true;
                        }
                        _logger.Warn($"Failed to archive the team.");
                        return false;
                    }
                    else
                    {
                        _logger.Warn($"Failed to archive the team.");
                        return false;
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.Error($"Failed to try archive teams: {ex.Message}", ex);
                throw;
            }
        }

        public bool TryUnarchiveTeamsForLockedChannelSite(bool forChannelSite = true)
        {
            if (!_isInitialized) return false;
            _logger.Info($"Start to try unarchive teams for group mailbox: {_teamsNode.Name}, for Channel site processing: {forChannelSite}");

            _sharedDBLocker = SampleSharedDBLocker.AcquireOrJoin4ArchivedTeams(_teamsNode.Name, _teamsNode.TeamId, _teamsNode.TenantId).GetAwaiter().GetResult();
            if (_sharedDBLocker.IsAlreadyActive)
            {
                // should usually be true here after the first holder finished to unarchive the team
                return ValidateTeamsState();
            }

            if (!_sharedDBLocker.ShouldProceedAction)
            {
                // Should normally not happen because AcquireOrJoin4ArchivedTeams waits for Retry internally.
                return false;
            }

            try
            {
                var teamsSettings = TeamsService.GetTeamSettings(_teamsNode.TeamId);
                if (teamsSettings is not null && teamsSettings.IsArchived.HasValue && teamsSettings.IsArchived.Value)
                {
                    var actionResult = TryDoActionWithTryUnlockTeamsSite("UnarchiveTeams", SiteState.ReadOnly, () => {
                        var result = TeamsServiceForDelegate.UnarchiveTeam(_teamsNode.TeamId);
                        if (result)
                        {
                            _logger.Info($"Successfully sent unarchive request for the team. Waiting for the team to be unarchived.");
                            if (ValidateTeamsState())
                            {
                                return true;
                            }
                            _logger.Warn($"Failed to unarchive the team.");
                            return false;
                        }
                        else
                        {
                            _logger.Warn($"Failed to unarchive the team.");
                            return false;
                        }
                    });

                    if (actionResult)
                    {
                        // The job that created the Starting locker is responsible for changing Starting -> Active.
                        if (!_sharedDBLocker.ActivateAsync().GetAwaiter().GetResult())
                        {
                            _logger.Warn($"Failed to activate shared DB locker for team " +$"{_teamsNode.TeamId}.");
                            return false;
                        }
                    }
                    return actionResult;
                }
                else
                {
                    _logger.Info($"The team is not archived, no need to try unarchive.");

                    if (!_sharedDBLocker.ActivateAsync().GetAwaiter().GetResult())
                    {
                        _logger.Warn($"Failed to activate shared DB locker for team {_teamsNode.TeamId}.");
                        return false;
                    }
                    return true; // If the team is not archived, consider it as success
                }
            }
            catch (Exception ex)
            {
                _logger.Error($"Failed to try unarchive teams: {ex.Message}", ex);
                throw;
            }
        }

        // needTeamsUnarchive = true if need check the team is unarchived, false if need check the team is archived
        private bool ValidateTeamsState(bool needTeamsUnarchive = true)
        {
            int maxRetryCount = 10;
            for (int i = 1; i <= maxRetryCount; i++)
            {
                var updatedSettings = TeamsService.GetTeamSettings(_teamsNode.TeamId);
                if (updatedSettings is not null && updatedSettings.IsArchived.HasValue )
                {
                    if (needTeamsUnarchive && !updatedSettings.IsArchived.Value)
                    {
                        _logger.Info($"The team is now unarchived.");
                        _isTeamsUnarchivedForLockedChannelSite = true;
                        return true;
                    }
                    else if (!needTeamsUnarchive && updatedSettings.IsArchived.Value)
                    {
                        _logger.Info($"The team is now archived.");
                        _isTeamsUnarchivedForLockedChannelSite = false;
                        return true;
                    }
                    else
                    {
                        _logger.Warn($"The team state is not as expected. Expected Teams State IsArchived: {!needTeamsUnarchive}, Actual IsArchived: {updatedSettings.IsArchived.Value}");
                    }
                    
                }
                _logger.Info($"Waiting for the team to be in the expected state. Attempt {i}/{maxRetryCount}.");
                Thread.Sleep(5000 * i);
            }

            _logger.Warn($"Timed out waiting for the team to match expect state. Expected Teams State IsArchived: {!needTeamsUnarchive}. Return false");
            // confirming: should throw here ? because the team is not in the expected state which can make the following operation fail
            return false;
        }

        public T TryDoActionWithTryUnlockTeamsSite<T>(string actionName, SiteState siteState, Func<T> action)
        {
            _logger.Info($"Start to try do action '{actionName}' with try unlock teams site for group mailbox: {_teamsNode.Name}");
            try
            {
                using var _ = new SiteStateTransitionScopeUtility(_teamsNode.url, siteState, true, true);
                T result = action();
                return result;
            }
            catch (AveSkipLockSiteException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.Error($"Failed to try do action '{actionName}' with try unlock teams site: {ex.Message}", ex);
                return default;
            }
        }

        public void Dispose()
        {
            if (!_isInitialized) return;
            if (_sharedDBLocker == null) return;

            try
            {
                _logger.Info($"Start disposing M365APIUtility for group mailbox: {_teamsNode.Name}");
                var shouldArchive = _sharedDBLocker.ReleaseAsync().GetAwaiter().GetResult();
                if (!shouldArchive)
                {
                    _logger.Info($"The shared locker [{_teamsNode.TeamId}] was not released by the current instance. No need to re-archive the team.");
                    return;
                }

                if (_isTeamsUnarchivedForLockedChannelSite)
                {
                    _logger.Info($"The team was unarchived during the operation, attempting to re-archive it.");
                    try
                    {
                        TryArchiveTeams(true);
                    }
                    finally
                    {
                        _sharedDBLocker.FinalizeReleaseAsync().GetAwaiter().GetResult();
                    }
                }
                else
                {
                    _sharedDBLocker.FinalizeReleaseAsync().GetAwaiter().GetResult();
                }
            }
            catch (Exception e)
            {
                _logger.Error($"Failed to release shared DB locker for group mailbox {_teamsNode.Name}: {e.Message}");
                throw;
            }
            finally
            {
                _sharedDBLocker.DisposeAsync().GetAwaiter().GetResult();

                _logger.Info($"Finished disposing M365APIUtility for group mailbox: " +$"{_teamsNode.Name}");
            }
        }
    }
}
