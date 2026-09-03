/********************************************************************
 *
 *  PROPRIETARY and CONFIDENTIAL
 *
 *  This file is licensed from, and is a trade secret of:
 *
 *                    AvePoint, Inc.
 *                    525 Washington Blvd, Suite 1400
 *                    Jersey City, NJ 07310
 *                    United States of America
 *                    Telephone: +1-201-793-1111
 *                    WWW: www.avepoint.com
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
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.DB.Model;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Validation;
using System.Linq;
using System.Threading.Tasks;

namespace AvePoint.RA.DB.Dao.Impl
{
    public class WorkplaceHoldDao : BaseDao<RMWorkspaceHold>, IWorkplaceHoldDao
    {
        public bool CheckWorkspaceHoldExist(WorkspaceRequestDto workspaceHoldDto)
        {
            try
            {
                if (workspaceHoldDto == null || string.IsNullOrEmpty(workspaceHoldDto.WorkplaceId))
                {
                    return false;
                }

                using var context = GetNewContext();
                return context.WorkspaceHold.Any(h => h.WorkplaceId == workspaceHoldDto.WorkplaceId);
            }
            catch (DbEntityValidationException dbex)
            {
                string message = string.Join("; ", dbex.EntityValidationErrors.SelectMany(x => x.ValidationErrors).Select(x => x.ErrorMessage));
                throw new DbEntityValidationException(message);
            }
            catch (Exception)
            {
                throw new Exception("An error occurred while checking if the workspace hold exists.");
            }
        }

        public bool SaveWorkspaceHold(RMWorkspaceHold workspaceHoldDto)
        {
            try
            {
                if (workspaceHoldDto == null || string.IsNullOrEmpty(workspaceHoldDto.Id))
                {
                    return false;
                }

                using var context = GetNewContext();
                var exist = context.WorkspaceHold.Any(d => d.Id == workspaceHoldDto.Id);
                if (exist)
                {
                    return false;
                }

                context.WorkspaceHold.Add(workspaceHoldDto);
                var count = context.SaveChanges();
                return count > 0;
            }
            catch (DbEntityValidationException dbex)
            {
                string message = string.Join("; ", dbex.EntityValidationErrors.SelectMany(x => x.ValidationErrors).Select(x => x.ErrorMessage));
                throw new DbEntityValidationException(message);
            }
            catch (Exception)
            {
                throw new Exception("An error occurred while saving the workspace hold.");
            }
        }

        public bool SaveWorkspaceHolds(List<RMWorkspaceHold> workspaceHolds)
        {
            try
            {
                if (workspaceHolds == null || workspaceHolds.Count == 0)
                {
                    return false;
                }

                var validWorkspaceHolds = workspaceHolds
                    .Where(h => h != null && !string.IsNullOrEmpty(h.Id))
                    .ToList();

                if (validWorkspaceHolds.Count != workspaceHolds.Count)
                {
                    return false;
                }

                using var context = GetNewContext();
                var ids = validWorkspaceHolds.Select(h => h.Id).Distinct(StringComparer.OrdinalIgnoreCase).ToList();
                var existingIds = context.WorkspaceHold
                    .Where(h => ids.Contains(h.Id))
                    .Select(h => h.Id)
                    .ToList();

                if (existingIds.Any())
                {
                    return false;
                }

                context.WorkspaceHold.AddRange(validWorkspaceHolds);
                return context.SaveChanges() > 0;
            }
            catch (DbEntityValidationException dbex)
            {
                string message = string.Join("; ", dbex.EntityValidationErrors.SelectMany(x => x.ValidationErrors).Select(x => x.ErrorMessage));
                throw new DbEntityValidationException(message);
            }
            catch (Exception)
            {
                throw new Exception("An error occurred while saving workspace holds.");
            }
        }

        public async Task<bool> UpdateWorkspaceHoldAsync(RMWorkspaceHold workspaceHoldDto)
        {
            try
            {
                if (workspaceHoldDto == null || string.IsNullOrEmpty(workspaceHoldDto.Id))
                {
                    return false;
                }

                using var context = GetNewContext();
                var existing = await context.WorkspaceHold.FirstOrDefaultAsync(h => h.Id == workspaceHoldDto.Id);
                if (existing == null)
                {
                    return false;
                }

                existing.HoldId = workspaceHoldDto.HoldId;
                existing.HoldBy = workspaceHoldDto.HoldBy;
                existing.ReleaseTime = workspaceHoldDto.ReleaseTime;
                return await context.SaveChangesAsync() > 0;
            }
            catch (DbEntityValidationException dbex)
            {
                string message = string.Join("; ", dbex.EntityValidationErrors.SelectMany(x => x.ValidationErrors).Select(x => x.ErrorMessage));
                throw new DbEntityValidationException(message);
            }
            catch (Exception)
            {
                throw new Exception("An error occurred while updating the workspace hold.");
            }
        }

        public async Task DeleteWorkspaceHolds(List<string> workspaceHoldIds)
        {
            try
            {
                if (workspaceHoldIds == null || workspaceHoldIds.Count == 0)
                {
                    return;
                }

                await BatchDeleteAsync(h => workspaceHoldIds.Contains(h.Id));
            }
            catch (DbEntityValidationException dbex)
            {
                string message = string.Join("; ", dbex.EntityValidationErrors.SelectMany(x => x.ValidationErrors).Select(x => x.ErrorMessage));
                throw new DbEntityValidationException(message);
            }
            catch (Exception)
            {
                throw new Exception("An error occurred while deleting workspace holds.");
            }
        }

        public async Task<List<WorkspaceHoldItemDto>> GetWorkspaceHoldsByPageSizeAsync()
        {
            try
            {

                using var context = GetNewContext();

                var dataCount = await context.WorkspaceHold.AsNoTracking().CountAsync();

                var items = await (from workspaceHold in context.WorkspaceHold.AsNoTracking()
                                   join hold in context.Hold.AsNoTracking() on workspaceHold.HoldId equals hold.Id into holdJoin
                                   from hold in holdJoin.DefaultIfEmpty()
                                   join remoteNode in context.RMRemoteNodes.AsNoTracking() on workspaceHold.WorkplaceId equals remoteNode.Id into remoteNodeJoin
                                   from remoteNode in remoteNodeJoin.DefaultIfEmpty()
                                   join mailbox in context.RMMailboxes.AsNoTracking() on workspaceHold.WorkplaceId equals mailbox.ObjectId into mailboxJoin
                                   from mailbox in mailboxJoin.DefaultIfEmpty()
                                   orderby workspaceHold.Id descending
                                   select new WorkspaceHoldItemDto
                                   {
                                       Id = workspaceHold.Id,
                                       WorkplaceId = workspaceHold.WorkplaceId,
                                       HoldId = workspaceHold.HoldId,
                                       HoldBy = workspaceHold.HoldBy,
                                       SourceType = workspaceHold.SourceType,
                                       HoldTitle = hold != null ? hold.Name : string.Empty,
                                       WorkplaceUrl = workspaceHold.SourceType == 6
                                           ? (mailbox != null ? mailbox.Name : string.Empty)
                                           : (remoteNode != null ? remoteNode.Url : string.Empty),
                                       WorkplaceName = workspaceHold.SourceType == 6
                                           ? (mailbox != null ? mailbox.Name : string.Empty)
                                           : (remoteNode != null ? remoteNode.Name : string.Empty),
                                       ReleaseTime = workspaceHold.ReleaseTime.ToString()
                                   })
                                   .ToListAsync();

                return items;
            }
            catch (DbEntityValidationException dbex)
            {
                string message = string.Join("; ", dbex.EntityValidationErrors.SelectMany(x => x.ValidationErrors).Select(x => x.ErrorMessage));
                throw new DbEntityValidationException(message);
            }
            catch (Exception)
            {
                throw new Exception("An error occurred while retrieving workspace holds by page size.");
            }
        }
        public async Task<long> GetReleaseTimeByAveSiteIdAsync(string workspaceId)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(workspaceId))
                {
                    return 0L;
                }
                using var context = GetNewContext();
                return await context.WorkspaceHold
                            .AsNoTracking()
                            .Where(x => x.WorkplaceId == workspaceId && x.ReleaseTime > 0)
                            .Select(x => x.ReleaseTime)
                            .FirstOrDefaultAsync();
            }
            catch (DbEntityValidationException dbex)
            {
                string message = string.Join("; ", dbex.EntityValidationErrors.SelectMany(x => x.ValidationErrors).Select(x => x.ErrorMessage));
                throw new DbEntityValidationException(message);
            }
            catch (Exception)
            {
                throw new Exception("An error occurred while retrieving the maximum release time by AveSiteId.");
            }
        }
        public async Task<bool> ExistWorkspaceHold()
        {
            try
            {
                using var context = GetNewContext();

                return await context.WorkspaceHold.AnyAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to check workspace hold exist");
            }
        }
    }
}