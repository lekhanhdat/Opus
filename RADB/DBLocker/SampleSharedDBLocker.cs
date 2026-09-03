using AvePoint.GCommon.Utility;
using AvePoint.RA.Contract.JobMonitor;
using AvePoint.RA.DB.Model;
using Microsoft.Graph.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.DB.DBLocker
{
    public class SampleSharedDBLocker : SampleDBLocker
    {
        #region Control Shared Locker for archived teams with email address format

        // This method is used to acquire or join a shared locker for archived teams based on the email address.
        // Key: ArchivedTeamsLocker_{domainName}{name} (without jobType to share between different jobs)
        // Description: Serialized JSON string of a list containing email, groupId, and o365TenantId.
        public static async Task<SampleSharedDBLocker> AcquireOrJoin4ArchivedTeams(string email, string groupId, string o365TenantId, TimeSpan? waitLockerTimeout = null)
        {
            var key = GetLockerKey4ArchivedTeams(email);
            var description = SerializerHelper.SerializeByJsonConvert(new List<string>() { email, groupId, o365TenantId });

            int retryIntervalInMs = _MinRetryIntervalInMs;

            DateTime? endTime = waitLockerTimeout == null ? null : DateTime.UtcNow.Add(waitLockerTimeout.Value);

            if (waitLockerTimeout != null && waitLockerTimeout.Value.TotalMilliseconds < _MinRetryIntervalInMs && waitLockerTimeout.Value.TotalMilliseconds > 0)
            {
                retryIntervalInMs = (int)waitLockerTimeout.Value.TotalMilliseconds;
            }

            do
            {
                try
                {
                    var result = await _LockerDao.AcquireOrJoinAsync(
                        new SampleLocker
                        {
                            Key = key,
                            Extension = description,
                            Created = DateTime.UtcNow.Ticks,
                            Timestamp = DateTime.UtcNow.Ticks
                        });

                    switch (result)
                    {
                        case AcquireLockerResult.ProceedAction:
                        case AcquireLockerResult.AlreadyActive:
                            return new SampleSharedDBLocker(key, description, result);
                        case AcquireLockerResult.Retry:
                            _Logger.Info($"Please wait, shared DB locker [{key}] is in transition. Retry after {retryIntervalInMs} ms.");
                            break;
                        default:
                            throw new InvalidOperationException($"Unknown AcquireLockerResult [{result}] for locker [{key}].");
                    }
                }
                catch (Exception ex)
                {
                    _Logger.Error($"Error occurred while acquiring shared DB locker [{key}]. {ex}");
                }

                await Task.Delay(retryIntervalInMs);

                if (retryIntervalInMs < _MaxRetryIntervalInMs)
                {
                    retryIntervalInMs += 1000 * 30;
                    retryIntervalInMs = Math.Min(retryIntervalInMs, _MaxRetryIntervalInMs);
                }

                if (endTime != null && endTime < DateTime.UtcNow)
                {
                    throw new SampleDBLockerTimeoutException($"Get shared DB locker [{key}] timeout.");
                }

            } while (true);
        }

        // don't include jobType in the locker key, because we want to share the same locker for different job types for the same archived teams.
        private static string GetLockerKey4ArchivedTeams(string email)
        {
            ParseEmail(email, out var domainName, out var name);
            return $"ArchivedTeamsLocker_{domainName}{name}";
        }

        #endregion

        public AcquireLockerResult AcquireResult { get; }

        public bool ShouldProceedAction => AcquireResult == AcquireLockerResult.ProceedAction;

        public bool IsAlreadyActive => AcquireResult == AcquireLockerResult.AlreadyActive;
        private bool _isActivated;
        private bool _isReleased;


        private SampleSharedDBLocker(string key, string description, AcquireLockerResult acquireResult) 
            : base(key, description, false)
        {
            AcquireResult = acquireResult;
            _Logger.Info($"Shared DB locker acquired/joined. Key: {key}, des: {description}, AcquireResult: {acquireResult}");
        }

        public async Task<bool> ActivateAsync()
        {
            if (!ShouldProceedAction)
            {
                return false;
            }

            if (_isActivated)
            {
                return true;
            }

            var activated = await _LockerDao.ActivateAsync(this._LockerKey);

            if (activated)
            {
                _isActivated = true;

                _Logger.Info($"Shared DB locker [{this._LockerKey}] activated.");
            }

            return activated;
        }

        public async Task<bool> ReleaseAsync()
        {
            if (_isReleased)
            {
                return false;
            }

            if (IsAlreadyActive)
            {
                return false;
            }

            return await _LockerDao.ReleaseAsync(this._LockerKey);
        }

        public async Task FinalizeReleaseAsync()
        {
            if (_isReleased)
            {
                return;
            }

            await _LockerDao.FinalizeReleaseAsync(this._LockerKey);

            _isReleased = true;
        }

        public override async ValueTask DisposeAsync()
        {
            if (_isReleased)
            {
                return;
            }

            try
            {
                _Timer.Dispose();
                _Logger.Info($"Release shared locker [{this._LockerKey}]");
            }
            catch (Exception ex)
            {
                _Logger.Error($"Error occurred while releasing shared locker: {this._LockerKey}. {ex}");
            }
        }
    }

    // all the on-going state is for async action that is not completed yet.
    // if the action is not async, just use Active and Released to control the locker state.
    public enum SampleLockerStatus
    {
        Starting = -1,
        Active = 0, // default value for the locker status, because the locker is active when it is acquired/created.
        Releasing = 1,
        Released = 2,
        Reclaiming = 3,
        Reclaimed = 4,
    }

    public enum AcquireLockerResult
    {
        ProceedAction,
        AlreadyActive,
        Retry
    }

    // No row       → Active      → ProceedUnarchive
    // Active       →             → AlreadyActive
    // Releasing    → Reclaiming  → Retry
    // Reclaiming   →             → Retry
    // Released     → Active      → ProceedUnarchive
}
