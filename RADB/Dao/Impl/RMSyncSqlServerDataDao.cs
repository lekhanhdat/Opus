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
using AvePoint.RA.DB.Core;
using AvePoint.RA.DB.Model;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Migrations;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.DB.Dao.Impl
{
    public class RMSyncSqlServerDataDao : IRMSyncSqlServerDataDao
    {
        public async Task<int> DeleteSharePointOnlineSettings(bool isContainer, Guid scopeId)
        {
            using var context = RMDBContextManager.GetNewDBContext();
            Expression<Func<RMSharePointSetting, bool>> predicate = isContainer ?
                item => item.ScopeId == scopeId || item.SiteGroupId == scopeId :
                item => item.ScopeId == scopeId || item.SiteId == scopeId;
            var willDeleteIds = await context.RMSharePointSettings.Where(predicate).Select(item => item.Id).ToListAsync();

            willDeleteIds.ForEach(id =>
            {
                var item = new RMSharePointSetting
                {
                    Id = id
                };
                context.Entry(item).State = EntityState.Deleted;
            });

            return await context.SaveChangesAsync();
        }

        public async Task<int> DeleteOneDriveSettings(bool isContainer, Guid scopeId)
        {
            using var context = RMDBContextManager.GetNewDBContext();
            Expression<Func<RMOneDriveSetting, bool>> predicate = isContainer ?
                item => item.ScopeId == scopeId || item.SiteGroupId == scopeId :
                item => item.ScopeId == scopeId || item.SiteId == scopeId;
            var willDeleteIds = await context.RMOneDriveSettings.Where(predicate).Select(item => item.Id).ToListAsync();

            willDeleteIds.ForEach(id =>
            {
                var item = new RMOneDriveSetting
                {
                    Id = id
                };
                context.Entry(item).State = EntityState.Deleted;
            });

            return await context.SaveChangesAsync();
        }

        public async Task<int> DeleteExchangeOnlineSettings(bool isContainer, Guid scopeId)
        {
            using var context = RMDBContextManager.GetNewDBContext();
            Expression<Func<RMExchangeOnlineSetting, bool>> predicate = isContainer ?
                item => item.ScopeId == scopeId || item.GroupId == scopeId :
                item => item.ScopeId == scopeId;
            var willDeleteIds = await context.RMExchangeOnlineSettings.Where(predicate).Select(item => item.Id).ToListAsync();

            willDeleteIds.ForEach(id =>
            {
                var item = new RMExchangeOnlineSetting
                {
                    Id = id
                };
                context.Entry(item).State = EntityState.Deleted;
            });

            return await context.SaveChangesAsync();
        }

        public async Task<int> ChangeNameForSharePointOnlineSettingsByGroupAsync(Guid groupId, string changedName)
        {
            using var context = RMDBContextManager.GetNewDBContext();
            var willChangedItems = await context.RMSharePointSettings.Where(item => item.ScopeId == groupId).ToListAsync();
            willChangedItems.ForEach(item => item.FullPath = changedName);
            context.RMSharePointSettings.AddOrUpdate(willChangedItems.ToArray());
            return await context.SaveChangesAsync();
        }

        public async Task<int> ChangeNameForSharePointOnlineSettingsBySiteAsync(Guid siteId, string beforeName, string changedName)
        {
            using var context = RMDBContextManager.GetNewDBContext();
            var willChangedItems = await context.RMSharePointSettings.Where(item => item.ScopeId == siteId || item.SiteId == siteId).ToListAsync();
            willChangedItems.ForEach(item => { 
                if(item.FullPath.StartsWith(beforeName))
                {
                    item.FullPath = item.FullPath.Replace(beforeName, changedName);
                }
            });
            context.RMSharePointSettings.AddOrUpdate(willChangedItems.ToArray());
            return await context.SaveChangesAsync();
        }

        public async Task<int> ChangeNameForOneDriveSettingsByGroupAsync(Guid groupId, string changedName)
        {
            using var context = RMDBContextManager.GetNewDBContext();
            var willChangedItems = await context.RMOneDriveSettings.Where(item => item.ScopeId == groupId).ToListAsync();
            willChangedItems.ForEach(item => item.FullPath = changedName);
            context.RMOneDriveSettings.AddOrUpdate(willChangedItems.ToArray());
            return await context.SaveChangesAsync();
        }

        public async Task<int> ChangeNameForOneDriveSettingsBySiteAsync(Guid siteId, string beforeName, string changedName)
        {
            using var context = RMDBContextManager.GetNewDBContext();
            var willChangedItems = await context.RMOneDriveSettings.Where(item => item.ScopeId == siteId || item.SiteId == siteId).ToListAsync();
            willChangedItems.ForEach(item => {
                if (item.FullPath.StartsWith(beforeName))
                {
                    item.FullPath = item.FullPath.Replace(beforeName, changedName);
                }
            });
            context.RMOneDriveSettings.AddOrUpdate(willChangedItems.ToArray());
            return await context.SaveChangesAsync();
        }

        public async Task<int> ChangeNameForExchangeOnlineSettingsAsync(Guid scopeId, string changedName)
        {
            using var context = RMDBContextManager.GetNewDBContext();
            var willChangedItems = await context.RMExchangeOnlineSettings.Where(item => item.ScopeId == scopeId).ToListAsync();
            willChangedItems.ForEach(item => item.Name = changedName);
            context.RMExchangeOnlineSettings.AddOrUpdate(willChangedItems.ToArray());
            return await context.SaveChangesAsync();
        }
    }
}
