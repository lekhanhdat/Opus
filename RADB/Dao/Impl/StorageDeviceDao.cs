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
using AvePoint.GCommon.Contract.AveModuleContract;
using AvePoint.GCommon.Contract.Server.Common;
using AvePoint.GCommon.Contract.Server.Common.Profile.Object;
using AvePoint.GCommon.Contract.Storage.Entity;
using AvePoint.GCommon.Utility;
using AvePoint.RA.Common.Util;
using AvePoint.RA.Contract.Common;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.Contract.Task;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.DB.Dao.Utility;
using AvePoint.RA.DB.Model;
using AvePoint.RA.DB.SecurityTrimming;
using Microsoft.Extensions.DependencyModel.Resolution;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Core.Objects;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using AvePoint.RA.Common.Configurations;

namespace AvePoint.RA.DB.Dao.Impl
{
    public class StorageDeviceDao : BaseDao<RMStorageDeviceInfo>, IRMStorageDeviceInfoDao
    {
        public List<RMStorageDeviceInfo> GetAllStorageByIsOldRecord(int isOldRecord, CommonSettingResultForPage pageInfo)
        {
            List<RMStorageDeviceInfo> result = GetProfiles(pageInfo);
            return result;

        }
        public List<RMStorageDeviceInfo> GetProfiles(CommonSettingResultForPage pageInfo)
        {
            pageInfo.IsDesc = true;
            List<Expression> allExpressionList = new List<Expression>();
            ParameterExpression param = Expression.Parameter(typeof(RMStorageDeviceInfo), "c");
            allExpressionList.Add(Expression4DynamicQuery.GetEqualExpression(typeof(RMStorageDeviceInfo), param, "Status", StorageStatus.UsedStorage));
            if (!string.IsNullOrEmpty(pageInfo.SearchValue))
            {
                allExpressionList.Add(Expression4DynamicQuery.GetContainsExpressionIgnoreCase(typeof(RMStorageDeviceInfo), param, "Name", pageInfo.SearchValue));
            }

            List<Expression> typesExpressionList = new List<Expression>();
            int totalRecord = 0;
            Expression queryExpr = allExpressionList.Aggregate(Expression.AndAlso);
            var lambda = Expression.Lambda<Func<RMStorageDeviceInfo, bool>>(queryExpr, param);
            List<RMStorageDeviceInfo> profiles = GetProfiles(pageInfo.PageIndex, pageInfo.PageSize, out totalRecord, "ModifiedTime", pageInfo.IsDesc, lambda);
            pageInfo.TotalNumber = totalRecord;
            return profiles;
        }
        public List<RMStorageDeviceInfo> GetProfiles(int pageIndex, int pageSize, out int totalRecord, string orderKey, bool isAsc, Expression<Func<RMStorageDeviceInfo, bool>> whereLambda = null)
        {
            try
            {
                using (var context = GetNewContext())
                {
                    IOrderedQueryable<RMStorageDeviceInfo> query = null;
                    var sortDirection = isAsc ? SortDirectionEnum.Descending : SortDirectionEnum.Ascending;
                    if (whereLambda != null)
                    {
                        query = context.RMStorageInfos.AsQueryable().Where(whereLambda).SortBy(orderKey, sortDirection);
                    }
                    else
                    {
                        query = context.RMStorageInfos.AsQueryable().SortBy(orderKey, sortDirection);
                    }
                    totalRecord = query.Count();

                    if (pageIndex == -1)
                    {
                        return query.OrderBy(q => q.Name).ToList();
                    }
                    else
                    {
                        return query.Skip((pageIndex - 1) * pageSize).Take(pageSize).ToList();
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<string> UpdateAsync(StorageDeviceDto dto)
        {
            //RMStorageDeviceInfo oldDto = base.Find(o => o.Id.ToString() == dto.Id);
            await base.UpdateAsync(StorageDeviceConvert.ConvertStorageDeviceDto(dto));
            return dto.Id;
        }

        public string Create(StorageDeviceDto dto)
        {
            dto.Id = Guid.NewGuid().ToString();
            base.Create(StorageDeviceConvert.ConvertStorageDeviceDto(dto));
            //ObjectPermissionDao.CreatePermissionOnEntity(dto.Id);
            return dto.Id;
        }

        public async Task<List<RMStorageDeviceInfo>> GetStoragesDeviceByFilterAsync(bool IsFilter)
        {
            List<RMStorageDeviceInfo> rmList = (await base.FindListAsync(o => o.Status == (int)StorageStatus.UsedStorage)).OrderBy(o => o.Name).ToList();
            List<RMStorageDeviceInfo> filterResult = new List<RMStorageDeviceInfo>();
            foreach (var rm in rmList)
            {
                if (IsFilter)
                {
                    //filter out default storage
                    if (rm.Type == (int)StorageDeviceType.CloudAzure && (rm.Id == new Guid(Common.RecordsConstants.AVEPOINT_DEFAULT_STORAGEID) || rm.IsSystemStorage))
                    {
                        continue;
                    }
                    if (rm.Type == (int)StorageDeviceType.SFTP || rm.Type == (int)StorageDeviceType.CloudAzure)
                    {
                        filterResult.Add(rm);
                    }
                }
                else
                {
                    if (rm.Type != (int)StorageDeviceType.Box)
                    {
                        filterResult.Add(rm);
                    }
                }
            }
            return filterResult;
        }

        public RMStorageDeviceInfo GetStorageDevicesById(Guid opusStorageId)
        {
            using var context = GetNewContext();
            return context.RMStorageInfos.FirstOrDefault(s => s.Id == opusStorageId);
        }

        public List<RMStorageDeviceInfo> GetStorageDevicesByIds(params Guid[] opusStorageIds)
        {
            using var context = GetNewContext();
            return context.RMStorageInfos.Where(s => opusStorageIds.ToList().Contains(s.Id)).ToList();
        }

        public async Task<int> DeleteMigratedStorageDevicesAsync()
        {
            var sql = $"DELETE FROM [{SecurityUtils.SanitizeSQLSchemaName(GetTenantSchemaName())}].RMStorageDeviceInfoes WHERE DAOMigrated=1";

            using (var context = GetNewContext())
            {
                return await context.Database.ExecuteSqlCommandAsync(sql);
            }
        }

        public async Task<List<RMStorageDeviceInfo>> GetGoogleStoragesDeviceAsync()
        {
            List<RMStorageDeviceInfo> rmList = (await base.FindListAsync(o => o.Status == (int)StorageStatus.UsedStorage)).OrderBy(o => o.Name).ToList();
            List<RMStorageDeviceInfo> filterResult = new List<RMStorageDeviceInfo>();
            foreach (var rm in rmList)
            {
                if (rm.Type == (int)StorageDeviceType.CloudAzure && (rm.Id == new Guid(Common.RecordsConstants.AVEPOINT_DEFAULT_STORAGEID) || rm.IsSystemStorage))
                {
                    continue;
                }
                else if (RMGlobalConfiguration.EnvSetting.IsGCPEnvironment &&
                         rm.Type == (int)StorageDeviceType.Google &&
                         (rm.Id == new Guid(Common.RecordsConstants.AVEPOINT_DEFAULT_STORAGEID)))
                {
                    continue;
                }


                if (rm.Type is (int)StorageDeviceType.SFTP or (int)StorageDeviceType.CloudAzure or (int)StorageDeviceType.Google)
                {
                    filterResult.Add(rm);
                }
            }
            return filterResult;
        }
    }
}
