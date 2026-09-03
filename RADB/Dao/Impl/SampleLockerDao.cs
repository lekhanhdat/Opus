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
using AvePoint.RA.DB.Core;
using AvePoint.RA.DB.DBLocker;
using AvePoint.RA.DB.Model;
using System;
using System.Data;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace AvePoint.RA.DB.Dao.Impl
{
    public class SampleLockerDao : BaseDao<SampleLocker>, ISampleLockerDao
    {
        private readonly static RALogger _Logger = RALogger.GetInstance(typeof(SampleLockerDao));

        public async Task CreateAsync(SampleLocker entity)
        {
            using var context = GetNewContext();
            context.SampleLockers.Add(entity);
            await context.SaveChangesAsync();
        }

        public async Task<bool> DeleteAsync(string key)
        {
            using var context = GetNewContext();
            var results = await context.Database.ExecuteSqlCommandAsync(
                $"DELETE FROM [{GetTenantSchemaName()}].SampleLockers WHERE [Key]=@Key;",
                new SqlParameter("@Key", key)
            );
            return results > 0;
        }

        public async Task<SampleLocker> GetAsync(string key)
        {
            using var context = GetNewContext();
            return await context.SampleLockers.FirstOrDefaultAsync(i => i.Key == key);
        }

        public async Task UpdateTimestampAsync(string key)
        {
            using var context = GetNewContext();
            await context.Database.ExecuteSqlCommandAsync(
                $"UPDATE [{GetTenantSchemaName()}].SampleLockers SET Timestamp=@Timestamp WHERE [Key]=@Key;",
                new SqlParameter("@Key", key),
                new SqlParameter("@Timestamp", DateTime.UtcNow.Ticks)
            );
        }

        public async Task<AcquireLockerResult> AcquireOrJoinAsync(SampleLocker entity/*, bool isAsyncAction*/)
        {
            using var context = GetNewContext();
            using var transaction = context.Database.BeginTransaction(IsolationLevel.ReadCommitted);

            try
            {
                var schema = GetTenantSchemaName();
                var now = DateTime.UtcNow.Ticks;
                //var firstState = isAsyncAction ? SampleLockerStatus.Starting : SampleLockerStatus.Active;

                var lockerStates = await context.Database.SqlQuery<int>(
                        $@"SELECT TOP 1 RefState FROM [{schema}].SampleLockers WITH (UPDLOCK, HOLDLOCK) WHERE [Key] = @Key;",
                        new SqlParameter("@Key", entity.Key))
                    .ToListAsync();

                // 1. No locker -> create new 
                if (lockerStates.Count == 0)
                {
                    await context.Database.ExecuteSqlCommandAsync(
                        $@" INSERT INTO [{schema}].SampleLockers ([Key], Extension, Created, [Timestamp], RefState) VALUES (@Key, @Description, @Now, @Now, @StartingState);",
                        new SqlParameter("@Key", entity.Key), 
                        new SqlParameter("@Description", entity.Extension), 
                        new SqlParameter("@Now", now), 
                        new SqlParameter("@StartingState", SqlDbType.Int) { Value = (int)SampleLockerStatus.Starting }
                    );

                    transaction.Commit();

                    _Logger.Info(
                        $"AcquireOrJoin locker [{entity.Key}] -> created new locker with State [{SampleLockerStatus.Starting}].");

                    return AcquireLockerResult.ProceedAction;
                }

                var currentState = (SampleLockerStatus)lockerStates[0];

                switch (currentState)
                {
                    // Already active -> just reuse it, no need to change state
                    case SampleLockerStatus.Active:
                        {
                            transaction.Commit();
                            _Logger.Info($"AcquireOrJoin locker [{entity.Key}] -> Active. Already active");
                            return AcquireLockerResult.AlreadyActive;
                        }
                    case SampleLockerStatus.Starting:
                        {
                            transaction.Commit();
                            _Logger.Info(
                                $"AcquireOrJoin locker [{entity.Key}] -> Starting. Retry later.");
                            return AcquireLockerResult.Retry;
                        }
                    // Async action is still running
                    case SampleLockerStatus.Releasing:
                        {
                            var reclaimRows = await context.Database.ExecuteSqlCommandAsync(
                            $@"
                            UPDATE [{schema}].SampleLockers
                            SET RefState = @ReclaimingState, [Timestamp] = @Now
                            WHERE [Key] = @Key AND RefState = @ReleasingState;",
                            new SqlParameter("@Key", entity.Key),
                            new SqlParameter("@Now", now),
                            new SqlParameter("@ReclaimingState", SqlDbType.Int) { Value = (int)SampleLockerStatus.Reclaiming },
                            new SqlParameter("@ReleasingState", SqlDbType.Int) { Value = (int)SampleLockerStatus.Releasing });

                            transaction.Commit();
                            if (reclaimRows > 0)
                            {
                                _Logger.Info($"AcquireOrJoin locker [{entity.Key}] -> Releasing -> Reclaiming. Retry later.");
                            }
                            else
                            {
                                _Logger.Warn($"AcquireOrJoin locker [{entity.Key}] -> Releasing but failed to update to Reclaiming. Retry later.");
                            }

                            return AcquireLockerResult.Retry;
                        }
                    // 4. Another job has already requested reclaim
                    case SampleLockerStatus.Reclaiming:
                        {
                            transaction.Commit();
                            _Logger.Info($"AcquireOrJoin locker [{entity.Key}] -> Reclaiming. Retry later.");
                            return AcquireLockerResult.Retry;
                        }
                    // 5. Archive has completed
                    case SampleLockerStatus.Released:
                        {
                            var startRows = await context.Database.ExecuteSqlCommandAsync(
                            $@"
                            UPDATE [{schema}].SampleLockers
                            SET RefState = @StartingState, [Timestamp] = @Now
                            WHERE [Key] = @Key AND RefState = @ReleasedState;",
                            new SqlParameter("@Key", entity.Key),
                            new SqlParameter("@Now", now),
                            new SqlParameter("@StartingState", SqlDbType.Int) { Value = (int)SampleLockerStatus.Starting },
                            new SqlParameter("@ReleasedState", SqlDbType.Int) { Value = (int)SampleLockerStatus.Released });

                            transaction.Commit();
                            if (startRows > 0)
                            {
                                _Logger.Info($"AcquireOrJoin locker [{entity.Key}] -> Released -> Starting. Proceed action.");
                                return AcquireLockerResult.ProceedAction;
                            }

                            // Defensive fallback. Another transaction changed the state.
                            _Logger.Warn($"AcquireOrJoin locker [{entity.Key}] -> state changed during acquire. Retry later.");
                            return AcquireLockerResult.Retry;
                        }
                    default:
                        transaction.Rollback();
                        throw new InvalidOperationException($"Unsupported SampleLocker state [{currentState}] for key [{entity.Key}].");
                }
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                _Logger.Error($"Error occurred while preparing unarchive locker [{entity.Key}]. {ex}");
                throw;
            }
        }

        public async Task<bool> ActivateAsync(string key)
        {
            using var context = GetNewContext();
            try
            {
                var schema = GetTenantSchemaName();
                var now = DateTime.UtcNow.Ticks;

                var updatedRows = await context.Database.ExecuteSqlCommandAsync(
                    $@"
                    UPDATE [{schema}].SampleLockers
                    SET RefState = @ActiveState, [Timestamp] = @Now
                    WHERE [Key] = @Key AND RefState = @StartingState;",
                    new SqlParameter("@Key", key),
                    new SqlParameter("@Now", now),
                    new SqlParameter("@ActiveState", SqlDbType.Int) { Value = (int)SampleLockerStatus.Active }, 
                    new SqlParameter("@StartingState", SqlDbType.Int) { Value = (int)SampleLockerStatus.Starting }
                );

                if (updatedRows > 0)
                {
                    _Logger.Info($"Activate locker [{key}] -> Starting -> Active.");
                    return true;
                }

                _Logger.Warn($"Activate locker [{key}] failed because locker [{key}] is no longer Starting.");
                return false;
            }
            catch (Exception ex)
            {
                _Logger.Error($"Error occurred while activating locker [{key}]. {ex}");
                throw;
                //return false;
            }
        }

        public async Task<bool> ReleaseAsync(string key)
        {
            using var context = GetNewContext();
            try
            {
                var schema = GetTenantSchemaName();
                var now = DateTime.UtcNow.Ticks;
                var updatedRows = await context.Database.ExecuteSqlCommandAsync(
                    $@"UPDATE [{schema}].SampleLockers
                    SET RefState = @ReleasingState, [Timestamp] = @Now
                    WHERE [Key] = @Key AND RefState = @ActiveState;",
                    new SqlParameter("@Key", key),
                    new SqlParameter("@Now", now),
                    new SqlParameter("@ReleasingState", SqlDbType.Int) { Value = (int)SampleLockerStatus.Releasing },
                    new SqlParameter("@ActiveState", SqlDbType.Int) { Value = (int)SampleLockerStatus.Active }
                );

                if (updatedRows > 0)
                {
                    _Logger.Info($"Release locker [{key}] -> Active -> Releasing.");

                    return true;
                }

                _Logger.Info($"Release locker [{key}] -> cannot release because locker is not Active.");

                return false;
            }
            catch (Exception ex)
            {
                _Logger.Error($"Error occurred while releasing locker: {key}. {ex}");
                throw;
            }
        }

        public async Task FinalizeReleaseAsync(string key)
        {
            using var context = GetNewContext();

            try
            {
                var schema = GetTenantSchemaName();
                var now = DateTime.UtcNow.Ticks;

                // No one requested reclaim while releasing.
                // The temporary locker can be removed.
                var deletedRows = await context.Database.ExecuteSqlCommandAsync(
                    $@"DELETE FROM [{schema}].SampleLockers
                    WHERE [Key] = @Key AND RefState = @ReleasingState;",
                    new SqlParameter("@Key", key),
                    new SqlParameter("@ReleasingState", SqlDbType.Int) { Value = (int)SampleLockerStatus.Releasing });

                if (deletedRows > 0)
                {
                    _Logger.Info($"FinalizeRelease locker [{key}] -> Releasing -> deleted.");
                    return;
                }

                // Other job/process requested reclaim while the release action was running.
                // Keep the locker and notify the waiting job that the resource has been released.
                var releasedRows = await context.Database.ExecuteSqlCommandAsync(
                    $@"UPDATE [{schema}].SampleLockers
                    SET RefState = @ReleasedState, [Timestamp] = @Now
                    WHERE [Key] = @Key AND RefState = @ReclaimingState;",
                    new SqlParameter("@Key", key),
                    new SqlParameter("@Now", now),
                    new SqlParameter("@ReleasedState", SqlDbType.Int) { Value = (int)SampleLockerStatus.Released },
                    new SqlParameter("@ReclaimingState", SqlDbType.Int) { Value = (int)SampleLockerStatus.Reclaiming });

                if (releasedRows > 0)
                {
                    _Logger.Info($"FinalizeRelease locker [{key}] -> Reclaiming -> Released.");
                    return;
                }
                _Logger.Info($"FinalizeRelease locker [{key}] -> no action required.");
            }
            catch (Exception ex)
            {
                _Logger.Error($"Error occurred while finalizing release locker: {key}. {ex}");
                // Keep the original release/archive result unaffected.
                // A cleanup/recovery mechanism can handle the stale locker later.
            }
        }
    }
}
