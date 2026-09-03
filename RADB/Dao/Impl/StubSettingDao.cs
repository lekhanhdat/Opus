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
using AvePoint.Common.Portal;
using AvePoint.GCommon.Contract.Server.Common;
using AvePoint.GCommon.Contract.Server.Common.Profile.Object;
using AvePoint.GCommon.Contract.Server.StubSetting;
using AvePoint.GCommon.Utility;
using AvePoint.RA.Common.Util;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.DB.Dao.Utility;
using AvePoint.RA.DB.Model;
using DocumentFormat.OpenXml.Spreadsheet;
using DocumentFormat.OpenXml.Wordprocessing;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.DB.Dao.Impl
{
    public class StubSettingDao : BaseDao<RMMiscProfile>, IRMMiscProfileDao
    {
        private readonly RALogger Logger = RALogger.GetInstance(typeof(StubSettingDao));
        public int Create(RMMiscProfile profile)
        {
            try
            {
                base.Create(profile);
                return (int)CreateOrEditStatus.Success;
            }
            catch (Exception ex)
            {
                //return ((int)RAFailedType.).ToString();
                return 1;
            }
        }

        public async Task<int> CreateAsync(RMMiscProfile profile)
        {
            try
            {
                await base.CreateAsync(profile);
                return (int)CreateOrEditStatus.Success;
            }
            catch (Exception ex)
            {
                //return ((int)RAFailedType.).ToString();
                return 1;
            }
        }

        public async Task<int> UpdateAsync(RMMiscProfile profile)
        {
            try
            {
                await base.UpdateAsync(profile);
                return (int)CreateOrEditStatus.Success;
            }
            catch (Exception ex)
            {
                //return ((int)CreateOrEditStatus.Failed).ToString();
                return 1;
            }
        }
        public int Delete(string Id)
        {
            try
            {
                base.DeleteByKey(Id);
                return (int)CreateOrEditStatus.Success;
            }
            catch (Exception ex)
            {
                //return ((int)CreateOrEditStatus.Failed).ToString();
                return 1;
            }
        }

        // batch delete only process for record rules
        public Task<int> BatchDeleteAsync(List<string> ids)
        {
            return base.BatchDeleteAsync(o => ids.Contains(o.Id));
        }

        public async Task<int> SoftDeleteAsync(RMMiscProfile profile)
        {
            try
            {
                if (profile != null)
                {
                    profile.IsRemoved = true;
                    await base.UpdateAsync(profile);
                }
                return (int)CreateOrEditStatus.Success;
            }
            catch (Exception)
            {
                //return ((int)CreateOrEditStatus.Failed).ToString();
                return 1;
            }
        }

        public bool IsNameExist(int type, string name)
        {
            using (var context = GetNewContext())
            {
                return context.MiscProfile.Any(m => m.Type == type && m.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
            }
        }

        public RMMiscProfile Load(string id)
        {
            RMMiscProfile rmProfile = base.Find(o => o.Id == id);
            return rmProfile;
        }


        public List<RMMiscProfile> LoadByTypes(List<int> types)
        {
            using (var context = GetNewContext())
            {
                return context.MiscProfile.AsNoTracking().Where(m => types.Contains(m.Type)).ToList();
            }
        }

        public List<RMMiscProfile> LoadAll(CommonSettingResultForPage pageInfo)
        {
            pageInfo.IsDesc = true;
            List<Expression> allExpressionList = new List<Expression>();
            ParameterExpression param = Expression.Parameter(typeof(RMMiscProfile), "c");
            allExpressionList.Add(Expression4DynamicQuery.GetEqualExpression(typeof(RMMiscProfile), param, "Type", (int)ProfileType.StubSetting));
            if (!string.IsNullOrEmpty(pageInfo.SearchValue))
            {
                allExpressionList.Add(Expression4DynamicQuery.GetContainsExpressionIgnoreCase(typeof(RMMiscProfile), param, "Name", pageInfo.SearchValue));
            }

            if (pageInfo is StubSettingResult)
            {
                allExpressionList.Add(Expression4DynamicQuery.GetNotEqualityExpression(typeof(RMMiscProfile), param, "IsRemoved", "true"));
            }

            List<Expression> typesExpressionList = new List<Expression>();
            int totalRecord = 0;
            Expression queryExpr = allExpressionList.Aggregate(Expression.AndAlso);
            var lambda = Expression.Lambda<Func<RMMiscProfile, bool>>(queryExpr, param);
            List<RMMiscProfile> profiles = GetProfiles(pageInfo.PageIndex, pageInfo.PageSize, out totalRecord, "ModifiedTime", pageInfo.IsDesc, lambda);
            pageInfo.TotalNumber = totalRecord;
            return profiles;
        }

        public List<RMMiscProfile> GetProfiles(int pageIndex, int pageSize, out int totalRecord, string orderKey, bool isAsc, Expression<Func<RMMiscProfile, bool>> whereLambda = null)
        {
            try
            {
                using (var context = GetNewContext())
                {
                    IOrderedQueryable<RMMiscProfile> query = null;
                    var sortDirection = isAsc ? SortDirectionEnum.Descending : SortDirectionEnum.Ascending;
                    if (whereLambda != null)
                    {
                        query = context.MiscProfile.AsQueryable().Where(whereLambda).SortBy(orderKey, sortDirection);
                    }
                    else
                    {
                        query = context.MiscProfile.AsQueryable().SortBy(orderKey, sortDirection);
                    }
                    totalRecord = query.Count();
                    if (pageIndex == -1)
                        return query.ToList();
                    else
                        return query.Skip((pageIndex - 1) * pageSize).Take(pageSize).ToList();
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        public RMMiscProfile Load(RMMiscProfile profile)
        {
            RMMiscProfile oldDto = base.Find(o => o.Type == profile.Type && o.Name == profile.Name);
            return oldDto;
        }

        public async Task<RMMiscProfile> LoadAsync(RMMiscProfile profile)
        {
            using var context = GetNewContext();
            RMMiscProfile oldDto = await context.MiscProfile.FirstOrDefaultAsync(o => o.Type == profile.Type && o.Name == profile.Name);
            return oldDto;
        }

        public List<RMMiscProfile> LoadAllByTypeNotPage(ProfileType type)
        {
            try
            {
                using (var context = GetNewContext())
                {
                    IOrderedQueryable<RMMiscProfile> query = null;
                    var sortDirection = SortDirectionEnum.Descending;
                    if (type == ProfileType.StubSetting)
                    {
                        var stubSetting = (int)ProfileType.StubSetting;
                        query = context.MiscProfile.AsQueryable().Where(a => a.Type == stubSetting && !a.IsRemoved).SortBy("ModifiedTime", sortDirection);
                    }
                    else
                    {
                        query = context.MiscProfile.AsQueryable().Where(a => a.Type == (int)type).SortBy("ModifiedTime", sortDirection);
                    }
                    return query.ToList();
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        public List<RMMiscProfile> LoadAllRecordsRules()
        {
            try
            {
                using (var context = GetNewContext())
                {
                    var query = context.MiscProfile.AsQueryable()
                        .Where(a => a.Type == (int)ProfileType.ArchiverRuleForRevIM);
                    return query.ToList();
                }
            }
            catch (Exception)
            {
                throw;
            }

        }

        public async Task<int> DeleteMigratedMiscProfilesAsync()
        {
            var sql = $"DELETE FROM [{SecurityUtils.SanitizeSQLSchemaName(GetTenantSchemaName())}].RMMiscProfiles WHERE DAOMigrated=1";

            using (var context = GetNewContext())
            {
                return await context.Database.ExecuteSqlCommandAsync(sql);
            }
        }

        public async Task<IEnumerable<RMMiscProfile>> LoadByPager(int pageIndex, int pageSize)
        {
            using var context = GetNewContext();
            return await context.MiscProfile.AsNoTracking().OrderBy(m => m.Id).Skip((pageIndex - 1) * pageSize).Take(pageSize).ToListAsync();
        }

        public async Task<long> MultiGeoInsertMiscProfileTableAsync(IEnumerable<RMMiscProfile> miscProfiles)
        {
            using var context = GetNewContext();
            try
            {
                context.MiscProfile.AddRange(miscProfiles);
                return await context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Logger.Error($"Insert RMMiscProfiles data has error: {ex}");
                return 0;
            }
        }

        public async Task<long> MultiGeoDeleteAllMiscProfileAsync()
        {
            return await TruncateAllDataInTableAsync("RMMiscProfiles");
        }
    }
}
