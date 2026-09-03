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
using AvePoint.RA.DB.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.RMWeb.ReportCenter;
using System.Data.Entity.Migrations;
using System.Data.Entity;
using AvePoint.RA.CommonUtil;
using AvePoint.GCommon.Utility;

namespace AvePoint.RA.DB.Dao.Impl
{
    public class RecordOwnerDao : BaseDao<RMRecordOwner>, IRecordOwnerDao
    {
        private readonly RALogger Logger = RALogger.GetInstance(typeof(RecordOwnerDao));
        public IAccountDao AccountDao { get; set; }
        public async Task UpdateRecordOwnersAsync(int spSettingId, List<ToUserInfo> accounts, RecordOwnerSettingType type = RecordOwnerSettingType.SharePoint)
        {
            foreach (var user in accounts)
            {
                var item = AccountDao.Find(s => s.UserId == user.UserId);
                if (item == null)
                {
                    RMAccount owner = new RMAccount();
                    owner.UserPrincipalName = user.UserPrincipalName;
                    owner.UserId = user.UserId;
                    owner.DisplayName = user.DisplayName;
                    RMAccount dbOwner = await AccountDao.CreateAsync(owner);
                }
            }
            using var context = GetNewContext();
            var originalOwners = context.RecordOwner.Where(o => o.SPSettingId == spSettingId && o.SettingType == (int)type).ToList();
            var nowOwners = DistinctAccounts(accounts);
            var originalOwnerIds = originalOwners.Select(o => o.ObjectId).ToList();
            var nowOwnerIds = nowOwners.Select(s => s.UserId);

            var addingOwners = nowOwners.Where(o => !originalOwnerIds.Contains(o.UserId)).Select(r => ConvertToEntity(r, spSettingId, type)).ToList();
            if (addingOwners.Count > 0)
            {
                BatchCreate(addingOwners);
            }

            var removingOwners = originalOwners.Where(o => !nowOwnerIds.Contains(o.ObjectId)).ToList();
            if (removingOwners.Count > 0)
            {
                BatchDelete(removingOwners);
            }
        }

        public async Task AddRecordOwnersAsync(int spSettingId, List<ToUserInfo> accounts, RecordOwnerSettingType type = RecordOwnerSettingType.SharePoint)
        {
            if (accounts != null && accounts.Count > 0)
            {
                accounts = DistinctAccounts(accounts);
                using var context = GetNewContext();

                foreach (var user in accounts)
                {
                    var item = AccountDao.Find(s => s.UserId == user.UserId);
                    if (item == null)
                    {
                        RMAccount owner = new RMAccount();
                        owner.UserPrincipalName = user.UserPrincipalName;
                        owner.UserId = user.UserId;
                        owner.DisplayName = user.DisplayName;
                        await AccountDao.CreateAsync(owner);
                    }
                }
                var addingOwners = accounts.Select(r => ConvertToEntity(r, spSettingId, type)).ToList();
                BatchCreate(addingOwners);
            }
        }

        public List<RMRecordOwner> GetRecordOwner(int spSettingId, RecordOwnerSettingType type = RecordOwnerSettingType.SharePoint)
        {
            using var context = GetNewContext();
            return context.RecordOwner.AsQueryable().Where(o => o.SPSettingId == spSettingId && o.SettingType == (int)type).ToList();
        }

        public List<RMRecordOwner> GetRecordOwner(List<int> spSettingIds, params RecordOwnerSettingType[] types)
        {
            using var context = GetNewContext();
            return
            [
                .. context.RecordOwner.AsQueryable().Where(o => spSettingIds.Contains(o.SPSettingId) && Enumerable.Contains(types, (RecordOwnerSettingType)o.SettingType)),
            ];
        }

        public void UpdateRecordOwnerToTeams(Dictionary<int, int?> spAndTeamsSettingIdMapping, RecordOwnerSettingType type = RecordOwnerSettingType.SharePoint)
        {
            using var context = GetNewContext();
            var spOwners = context.RecordOwner.AsQueryable().Where(o => spAndTeamsSettingIdMapping.Keys.Contains(o.SPSettingId)).ToList();
            var spAIOwners = spOwners.Where(owner => owner.SettingType == (int)RecordOwnerSettingType.AISharePointOnline);
            var spMAOwners = spOwners.Where(owner => owner.SettingType == (int)RecordOwnerSettingType.SharePoint);

            spAIOwners.ForEach(item =>
            {
                item.SPSettingId = spAndTeamsSettingIdMapping[item.SPSettingId] ?? item.SPSettingId;
                item.SettingType = (int)RecordOwnerSettingType.AITeams;
            });

            spMAOwners.ForEach(item =>
            {
                item.SPSettingId = spAndTeamsSettingIdMapping[item.SPSettingId] ?? item.SPSettingId;
                item.SettingType = (int)RecordOwnerSettingType.Teams;
            });

            context.RecordOwner.AddOrUpdate(spAIOwners.Union(spMAOwners).ToArray());
            context.SaveChanges();
        }

        public async Task<IEnumerable<RMRecordOwner>> LoadByPager(int pageIndex, int pageSize)
        {
            using var context = GetNewContext();
            return await context.RecordOwner.AsNoTracking().OrderBy(r => r.Id).Skip((pageIndex - 1) * pageSize).Take(pageSize).ToListAsync();
        }

        public async Task<long> MultiGeoInsertRecordOwnerTableAsync(IEnumerable<RMRecordOwner> recordOwners)
        {
            using var context = GetNewContext();
            string tableName = "RMRecordOwners";
            try
            {
                await ExecuteSetInsertIdentityOn(context, tableName);
                string schemaName = SecurityUtils.SanitizeSQLSchemaName(context.SchemaName);
                var sqlBuilder = new System.Text.StringBuilder();
                var parameters = new List<System.Data.SqlClient.SqlParameter>();
                int paramIndex = 0;

                sqlBuilder.AppendLine($"INSERT INTO {schemaName}.{tableName} (Id, ObjectId, SPSettingId, SettingType, UserId) VALUES ");
                int i = 0;
                foreach (var item in recordOwners)
                {
                    if (i > 0) sqlBuilder.Append(", ");
                    sqlBuilder.AppendLine($"(@p{paramIndex}, @p{paramIndex + 1}, @p{paramIndex + 2}, @p{paramIndex + 3}, @p{paramIndex + 4})");

                    parameters.Add(new System.Data.SqlClient.SqlParameter($"@p{paramIndex}", item.Id));
                    parameters.Add(new System.Data.SqlClient.SqlParameter($"@p{paramIndex + 1}", (object)item.ObjectId ?? DBNull.Value));
                    parameters.Add(new System.Data.SqlClient.SqlParameter($"@p{paramIndex + 2}", item.SPSettingId));
                    parameters.Add(new System.Data.SqlClient.SqlParameter($"@p{paramIndex + 3}", item.SettingType));
                    parameters.Add(new System.Data.SqlClient.SqlParameter($"@p{paramIndex + 4}", item.UserId));
                    paramIndex += 5;
                    i++;
                }
                return await context.Database.ExecuteSqlCommandAsync(sqlBuilder.ToString(), parameters.ToArray());
            }
            catch (Exception ex)
            {
                Logger.Error($"Insert RMRecordOwners data has error: {ex}");
                return 0;
            }
            finally
            {
                await ExecuteSetInsertIdentityOff(context, tableName);
            }
        }

        public async Task<long> MultiGeoDeleteAllRecordOwnerAsync()
        {
            return await TruncateAllDataInTableAsync("RMRecordOwners");
        }
        public async Task<List<ToUserInfo>> GetRecordOwnerAccountsAsync(int spSettingId, RecordOwnerSettingType type = RecordOwnerSettingType.SharePoint)
        {
            List<ToUserInfo> userInfo = new List<ToUserInfo>();
            var ownerIds = GetRecordOwner(spSettingId, type).Select(p => p.ObjectId);
            //account表有可能出现多条userid一样的记录 RECO-7264
            List<ToUserInfo> accountInfoContainRepeated =  (await AccountDao.FindListAsync(o => ownerIds.Contains(o.UserId))).Select(o => ConvertToAccount(o)).ToList();
            if (accountInfoContainRepeated != null && accountInfoContainRepeated.Count > 0)
            {
                List<string> userIdList = new List<string>();
                foreach (ToUserInfo account in accountInfoContainRepeated)
                {
                    if (!userIdList.Contains(account.UserId))
                    {
                        userIdList.Add(account.UserId);
                        userInfo.Add(account);
                    }
                }
            }
            return userInfo;
        }

        private List<ToUserInfo> DistinctAccounts(List<ToUserInfo> accounts)
        {
            if(accounts != null && accounts.Count > 1)
            {
                Dictionary<string, ToUserInfo> tempDic = new Dictionary<string, ToUserInfo>();
                accounts.ForEach(a =>
                {
                    if (!tempDic.ContainsKey(a.UserId))
                    {
                        tempDic.Add(a.UserId, a);
                    }
                });
                return tempDic.Values.ToList();
            }
            else
            {
                return accounts ?? new List<ToUserInfo>();
            }
        }

        private RMRecordOwner ConvertToEntity(ToUserInfo owner, int spSettingId, RecordOwnerSettingType type = RecordOwnerSettingType.SharePoint)
        {
            return new RMRecordOwner()
            {
                
                ObjectId = owner.UserId,
                SPSettingId = spSettingId,
                SettingType = (int)type,
                //TenantId = owner.tenantId,
                //DisplayName = owner.DisplayName,
                //UserPrincipalName = owner.UserPrincipalName,
                //Type = (AccountType)Enum.Parse(typeof(AccountType), owner.InviteType)
            };
        }

        private ToUserInfo ConvertToAccount(RMAccount owner)
        {
            return new ToUserInfo()
            {
                UserId = owner.UserId,
                DisplayName = owner.DisplayName,
                UserPrincipalName = owner.UserPrincipalName,
            };
        }


    }
}
