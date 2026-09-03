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
using AvePoint.GCommon.Utility;
using AvePoint.RA.Common;
using AvePoint.RA.Common.Util;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RMWeb.ReportCenter;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.DB.Explorer;
using AvePoint.RA.DB.Model;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Validation;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;

namespace AvePoint.RA.DB.Dao.Impl
{
    public class HoldDao : BaseDao<RMHold>, IHoldDao
    {
        private IAccountDao AccountDao => PlatformWindsorManager.GetService<IAccountDao>();
        private ILnkUserGroupDao LnkUserGroupDao => PlatformWindsorManager.GetService<ILnkUserGroupDao>();
        public bool CheckHoldNameExist(RMHold holdDto)
        {
            using (var context = GetNewContext())
            {
                bool exist = false;
                exist = context.Hold.AsQueryable().Any(h => h.Name.Equals(holdDto.Name));
                return exist;
            }
        }
        public bool SaveHold(RMHold holdDto, List<ToUserInfo> holdUsers)
        {
            try
            {
                using (var context = GetNewContext())
                {
                    var exist = context.Hold.Any(d => d.Id == holdDto.Id);
                    if (!exist)
                    {
                        context.Hold.Add(holdDto);

                        var count = context.SaveChanges();
                        if (count > 0)
                        {
                            var holdMemberships = new List<RMHoldMemberships>();
                            foreach (var user in holdUsers)
                            {
                                var holdMembership = new RMHoldMemberships()
                                {
                                    UserId = user.UserId,
                                    HoldId = holdDto.Id
                                };
                                holdMemberships.Add(holdMembership);
                            }
                            context.RMHoldMemberships.AddRange(holdMemberships);
                            context.SaveChanges();
                        }
                        return count > 0;
                    }
                }
                return false;
            }
            catch (DbEntityValidationException dbex)
            {
                string message = string.Join("; ", dbex.EntityValidationErrors.SelectMany(x => x.ValidationErrors).Select(x => x.ErrorMessage));
                throw new DbEntityValidationException(message);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public bool EditHold(RMHold holdDto, List<ToUserInfo> holdUsers)
        {
            try
            {
                using (var context = GetNewContext())
                {
                    using (var transaction = context.Database.BeginTransaction())
                    {
                        try
                        {
                            var entities = context.Hold.Where(h => h.Id == holdDto.Id).ToList();
                            if (!entities.Any())
                            {
                                return false;
                            }

                            foreach (var entity in entities)
                            {
                                entity.HoldDateType = holdDto.HoldDateType;
                                entity.Number = holdDto.Number;
                                entity.HoldUnit = holdDto.HoldUnit;
                                entity.CalendarTime = holdDto.CalendarTime;
                                entity.TimeZoneId = holdDto.TimeZoneId;
                                entity.IsDaylightSaving = holdDto.IsDaylightSaving;
                                entity.Description = holdDto.Description;
                                entity.IsEmailNotificationEnabled = holdDto.IsEmailNotificationEnabled;
                                entity.ReminderDurationDays = holdDto.ReminderDurationDays;
                                entity.EmailRecipients = holdDto.EmailRecipients;
                                entity.LastSentEmailTime = holdDto.LastSentEmailTime;
                                entity.IsHoldManagerEmailNotificationEnabled = holdDto.IsHoldManagerEmailNotificationEnabled;
                            }

                            var countUpdate = this.BatchUpdate(entities);

                            if (countUpdate > 0)
                            {
                                var existingMemberships = context.RMHoldMemberships
                                    .Where(m => m.HoldId == holdDto.Id)
                                    .ToList();

                                if (existingMemberships.Any())
                                {
                                    context.RMHoldMemberships.RemoveRange(existingMemberships);
                                    context.SaveChanges();
                                }

                                if (holdUsers != null && holdUsers.Any())
                                {
                                    var uniqueUserIds = holdUsers
                                        .Where(u => !string.IsNullOrEmpty(u?.UserId))
                                        .Select(u => u.UserId)
                                        .Distinct()
                                        .ToList();

                                    if (uniqueUserIds.Any())
                                    {
                                        var holdMemberships = uniqueUserIds
                                            .Select(userId => new RMHoldMemberships
                                            {
                                                UserId = userId,
                                                HoldId = holdDto.Id
                                            })
                                            .ToList();

                                        context.RMHoldMemberships.AddRange(holdMemberships);
                                        context.SaveChanges();
                                    }
                                }
                            }

                            transaction.Commit();
                            return countUpdate > 0;
                        }
                        catch
                        {
                            transaction.Rollback();
                            throw;
                        }
                    }
                }
            }
            catch (DbEntityValidationException dbex)
            {
                string message = string.Join("; ", dbex.EntityValidationErrors
                    .SelectMany(x => x.ValidationErrors)
                    .Select(x => x.ErrorMessage));
                throw new DbEntityValidationException(message);
            }
        }

        public List<RMHold> GetAllHolds(int profileType = 0)
        {
            List<RMHold> holds = null;
            using (var context = GetNewContext())
            {
                //前台是insert  所以想降序显示的话 后台得升序
                if (profileType == (int)HoldProfileType.All)
                {
                    holds = context.Hold.SortBy("CreateTime", SortDirectionEnum.Ascending).ToList();
                }
                else
                {
                    holds = context.Hold.Where(a => a.Type == profileType).SortBy("CreateTime", SortDirectionEnum.Ascending).ToList();
                }
            }
            if (holds != null)
            {
                //append duplicate hold name
                var existingNames = holds.Select(n => n.Name.ToLower()).Distinct().ToList();
                List<string> processedNames = new List<string>();
                foreach (var hold in holds)
                {
                    if (processedNames.Contains(hold.Name.ToLower()))
                    {
                        int startIndex = 1;
                        if (!existingNames.Contains(hold.Name.ToLower()))
                        {
                            startIndex = Convert.ToInt32(hold.Name.Substring(hold.Name.LastIndexOf("_"))) + 1;
                        }
                        for (int i = startIndex; i < startIndex + 1000; i++)
                        {
                            var newName = hold.Name + "_" + i;
                            if (!processedNames.Contains(newName.ToLower()) && !existingNames.Contains(newName.ToLower()))
                            {
                                hold.Name = newName;
                                processedNames.Add(newName.ToLower());
                                break;
                            }
                        }
                    }
                    else
                    {
                        processedNames.Add(hold.Name.ToLower());
                    }
                }
            }

            return holds;
        }
        public List<RMHold> GetAllHoldsByUserAssignedManage()
        {
            List<RMHold> holds = null;
            var userHoldIds = new List<string>();
            var userGroupIds = LnkUserGroupDao.GetAllGroupIdsAsync(TenantLocalValue.LogonUserId).GetAwaiter().GetResult();
            using (var context = GetNewContext())
            {
                if (userGroupIds != null)
                {
                    var userIds = context.LnkUserGroup
                        .Where(x => userGroupIds.Contains(x.GroupId))
                        .Select(x => x.GroupId)
                        .Distinct()
                        .ToList(); 
                    userHoldIds = context.RMHoldMemberships
                        .Where(m => userIds.Contains(m.UserId))
                        .Select(m => m.HoldId)
                        .Distinct()
                        .ToList();
                }
                
                    var userId = context.RMHoldMemberships
                        .Where(m => m.UserId == TenantLocalValue.LogonUserId)
                        .Select(m => m.HoldId)
                        .Distinct()
                        .ToList();
                if (userId != null)
                {
                    userHoldIds.AddRange(userId);
                }

                holds = context.Hold
                       .Where(h => userHoldIds.Contains(h.Id))
                       .SortBy("CreateTime", SortDirectionEnum.Ascending)
                       .ToList(); 
            }

            if (holds != null)
            {
                var existingNames = holds.Select(n => n.Name.ToLower()).Distinct().ToList();
                List<string> processedNames = new List<string>();
                foreach (var hold in holds)
                {
                    if (processedNames.Contains(hold.Name.ToLower()))
                    {
                        int startIndex = 1;
                        if (!existingNames.Contains(hold.Name.ToLower()))
                        {
                            startIndex = Convert.ToInt32(hold.Name.Substring(hold.Name.LastIndexOf("_"))) + 1;
                        }
                        for (int i = startIndex; i < startIndex + 1000; i++)
                        {
                            var newName = hold.Name + "_" + i;
                            if (!processedNames.Contains(newName.ToLower()) && !existingNames.Contains(newName.ToLower()))
                            {
                                hold.Name = newName;
                                processedNames.Add(newName.ToLower());
                                break;
                            }
                        }
                    }
                    else
                    {
                        processedNames.Add(hold.Name.ToLower());
                    }
                }
            }
            return holds;
        }
        public async Task<Dictionary<string, List<ToUserInfo>>> GetUsersManageHold(List<string> holdIds)
        {
            Dictionary<string, List<ToUserInfo>> dict = new Dictionary<string, List<ToUserInfo>>();
            using (var context = GetNewContext())
            {
                var memberships = context.RMHoldMemberships.AsNoTracking()
                    .Where(o => holdIds.Contains(o.HoldId))
                    .ToList();

                var userIds = memberships.Select(o => o.UserId).Distinct().ToList();

                var accounts = (await AccountDao.FindListAsync(o => userIds.Contains(o.UserId) && o.IsRemoved == 0 && o.ObjectType != RMActiveDirectoryObjectType.UserInGroup)).ToList();

                foreach (var holdId in holdIds)
                {
                    var holdUserIds = memberships.Where(m => m.HoldId == holdId).Select(m => m.UserId).ToList();
                    var holdUsers = accounts.Where(a => holdUserIds.Contains(a.UserId))
                        .Select(a => new ToUserInfo { UserId = a.UserId, UserPrincipalName = a.UserPrincipalName, DisplayName = a.DisplayName, Id = a.AADId , RMUserId = a.Id})
                        .ToList();
                    dict[holdId] = holdUsers;
                }

                return dict;
            }
        }


        public List<RMHold> GetHoldByIds(List<string> ids)
        {
            using (var ctx = GetNewContext())
            {
                return ctx.Hold.Where(m => ids.Contains(m.Id)).ToList<RMHold>();
            }
        }
        public RMHold GetHoldById(string id)
        {
            using (var ctx = GetNewContext())
            {
                return ctx.Hold.FirstOrDefault(m => m.Id == id);
            }
        }
        public Task DeleteHoldAsync(List<string> holdIds)
        {
            //using (var ctx = GetNewContext())
            //{
            //    ctx.Hold.Where(h => holdIds.Any(id => id == h.Id)).Delete();
            //}
            return BatchDeleteAsync(h => holdIds.Contains(h.Id));
        }

        public List<RMHold> GetFileHoldByBoxId(Guid boxId)
        {
            using (var ctx = GetNewContext())
            {
                string sql = "select h.* from {0}.RMHolds as h join {0}.RMRecordAlliances as a on a.HoldId = h.Id where a.BoxId = @boxId";
                return ctx.Database.SqlQuery<RMHold>(string.Format(sql, SecurityUtils.SanitizeSQLSchemaName(ctx.SchemaName)), new SqlParameter("boxId", boxId)).ToList();
            }
        }
        public async Task<List<RMHold>> GetHoldsPendingReminderEmailAsync()
        {
            using (var ctx = GetNewContext())
            {
                return await ctx.Hold.Where(h => h.IsEmailNotificationEnabled && h.LastSentEmailTime == 0).AsNoTracking().ToListAsync();
            }
        }

        public async Task<bool> UpdateLastSentEmailTimeAsync(List<string> holdIds, long lastSentEmailTime)
        {
            using (var ctx = GetNewContext())
            {
                var parameters = holdIds.Select((id, index) => new SqlParameter($"@id{index}", id)).ToList();

                var sql = $@"UPDATE {SecurityUtils.SanitizeSQLSchemaName(ctx.SchemaName)}.RMHolds SET LastSentEmailTime = @lastSentEmailTime WHERE Id IN ({string.Join(",", parameters.Select(p => p.ParameterName))})";

                parameters.Add(new SqlParameter("lastSentEmailTime", lastSentEmailTime));

                return await ctx.Database.ExecuteSqlCommandAsync(sql, parameters.Cast<object>().ToArray()) > 0;
            }
        }
    }
}
