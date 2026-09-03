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
using AvePoint.RA.Api.Contract;
using AvePoint.RA.Contract.CustomizeConnector.Enums;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.DB.Core;
using AvePoint.RA.DB.Model;
using DocumentFormat.OpenXml.Office2010.Excel;
using RazorEngine.Compilation.ImpromptuInterface.InvokeExt;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Migrations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.DB.Dao.Impl
{
    public class RMCustomizeConnectorColumnDao : IRMCustomizeConnectorColumnDao
    {
        public async Task<RMCustomizeConnectorColumn> Add(RMCustomizeConnectorColumn columnInfo)
        {
            using var context = RMDBContextManager.GetNewDBContext();
            return (await Add(context, new List<RMCustomizeConnectorColumn> { columnInfo })).FirstOrDefault();
        }

        public async Task<RMCustomizeConnectorColumn> Add(RMDbContext context, RMCustomizeConnectorColumn columnInfo)
        {
            return (await Add(context, new List<RMCustomizeConnectorColumn> { columnInfo })).FirstOrDefault();
        }

        public async Task<IEnumerable<RMCustomizeConnectorColumn>> Add(IEnumerable<RMCustomizeConnectorColumn> columnInfoes)
        {
            using var context = RMDBContextManager.GetNewDBContext();
            return await Add(context, columnInfoes);
        }

        public async Task<IEnumerable<RMCustomizeConnectorColumn>> Add(RMDbContext context, IEnumerable<RMCustomizeConnectorColumn> columnInfoes)
        {
            var now = DateTime.UtcNow.Ticks;

            foreach (var columnInfo in columnInfoes)
            {
                if (columnInfo.Origin == CustomizeConnectorOrigin.BuildIn)
                {
                    continue;
                }

                columnInfo.Id = Guid.NewGuid();

                columnInfo.Created = now;
                columnInfo.Modified = now;

                columnInfo.CreatedBy = TenantLocalValue.LogonUserId;
                columnInfo.ModifiedBy = TenantLocalValue.LogonUserId;
                columnInfo.Origin = CustomizeConnectorOrigin.ExternalCustomize;
                columnInfo.Scope = CustomizeConnectorColumnScope.Template;

                context.RMCustomizeConnectorColumns.Add(columnInfo);
            }

            await context.SaveChangesAsync();

            return columnInfoes;
        }

        public async Task Delete(Guid id)
        {
            using var context = RMDBContextManager.GetNewDBContext();
            await Delete(context, new List<Guid> { id });
        }

        public async Task Delete(RMDbContext context, Guid id)
        {
            await Delete(context, new List<Guid> { id });
        }

        public async Task Delete(IEnumerable<Guid> ids)
        {
            using var context = RMDBContextManager.GetNewDBContext();
            await Delete(context, ids);
        }

        public async Task Delete(RMDbContext context, IEnumerable<Guid> ids)
        {
            var existEntities = await context.RMCustomizeConnectorColumns.Where(item => ids.Contains(item.Id) && item.Origin != CustomizeConnectorOrigin.BuildIn).ToListAsync();
            context.RMCustomizeConnectorColumns.RemoveRange(existEntities);

            await context.SaveChangesAsync();
        }

        public async Task Update(RMCustomizeConnectorColumn columnInfo)
        {
            using var context = RMDBContextManager.GetNewDBContext();
            await Update(context, new List<RMCustomizeConnectorColumn> { columnInfo });
        }

        public async Task Update(RMDbContext context, RMCustomizeConnectorColumn columnInfo)
        {
            await Update(context, new List<RMCustomizeConnectorColumn> { columnInfo });
        }

        public async Task Update(IEnumerable<RMCustomizeConnectorColumn> columnInfoes)
        {
            using var context = RMDBContextManager.GetNewDBContext();
            await Update(context, columnInfoes);
        }

        public async Task Update(RMDbContext context, IEnumerable<RMCustomizeConnectorColumn> columnInfoes)
        {
            var now = DateTime.UtcNow.Ticks;
            var columnIds = columnInfoes.Select(item => item.Id).ToHashSet();
            var columnInfoesDic = columnInfoes.ToDictionary(item => item.Id, item => item);
            var existColumnsInfoes = await context.RMCustomizeConnectorColumns.Where(item => columnIds.Contains(item.Id) && item.Origin != CustomizeConnectorOrigin.BuildIn).ToListAsync();

            foreach(var columnInfo in existColumnsInfoes)
            {
                var needUpdateColumnInfo = columnInfoesDic[columnInfo.Id];
                columnInfo.Name = needUpdateColumnInfo.Name;
                columnInfo.Description = needUpdateColumnInfo.Description;
                columnInfo.Extention = needUpdateColumnInfo.Extention;
                columnInfo.Modified = now;
                columnInfo.ModifiedBy = TenantLocalValue.LogonUserId;
            }

            context.RMCustomizeConnectorColumns.AddOrUpdate(item => item.Id, existColumnsInfoes.ToArray());

            await context.SaveChangesAsync();
        }

        public async Task<IEnumerable<RMCustomizeConnectorColumn>> GetAll(params CustomizeConnectorOrigin[] origins)
        {
            using var context = RMDBContextManager.GetNewDBContext();
            return await context.RMCustomizeConnectorColumns.AsNoTracking().Where(item => Enumerable.Contains(origins, item.Origin)).ToListAsync();
        }
    }
}
