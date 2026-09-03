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
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Migrations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.DB.Dao.Impl
{
    public class RMCustomizeConnectorTemplateDao : IRMCustomizeConnectorTemplateDao
    {

        public async Task<RMCustomizeConnectorTemplate> Add(RMCustomizeConnectorTemplate templateInfo)
        {
            using var context = RMDBContextManager.GetNewDBContext();
            return (await Add(context, new List<RMCustomizeConnectorTemplate> { templateInfo })).FirstOrDefault();
        }

        public async Task<RMCustomizeConnectorTemplate> Add(RMDbContext context, RMCustomizeConnectorTemplate templateInfo)
        {
            return (await Add(context, new List<RMCustomizeConnectorTemplate> { templateInfo })).FirstOrDefault();
        }

        public async Task<IEnumerable<RMCustomizeConnectorTemplate>> Add(IEnumerable<RMCustomizeConnectorTemplate> templateInfoes)
        {
            using var context = RMDBContextManager.GetNewDBContext();
            return await Add(context, templateInfoes);
        }

        public async Task<IEnumerable<RMCustomizeConnectorTemplate>> Add(RMDbContext context, IEnumerable<RMCustomizeConnectorTemplate> templateInfoes)
        {
            
            var now = DateTime.UtcNow.Ticks;

            foreach(var templateInfo in templateInfoes)
            {
                templateInfo.Id = Guid.NewGuid();
                templateInfo.Created = now;
                templateInfo.Modified = now;

                templateInfo.CreatedBy = TenantLocalValue.LogonUserId;
                templateInfo.ModifiedBy = TenantLocalValue.LogonUserId;

                templateInfo.Origin = CustomizeConnectorOrigin.ExternalCustomize;

                context.RMCustomizeConnectorTemplates.Add(templateInfo);
            }
            await context.SaveChangesAsync();

            return templateInfoes;
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
            var existEntities = await context.RMCustomizeConnectorTemplates.Where(item => ids.Contains(item.Id)).ToListAsync();
            context.RMCustomizeConnectorTemplates.RemoveRange(existEntities);

            await context.SaveChangesAsync();
        }

        public async Task Update(RMCustomizeConnectorTemplate templateInfo)
        {
            using var context = RMDBContextManager.GetNewDBContext();
            await Update(context, new List<RMCustomizeConnectorTemplate> { templateInfo });
        }

        public async Task Update(RMDbContext context, RMCustomizeConnectorTemplate templateInfo)
        {
            await Update(context, new List<RMCustomizeConnectorTemplate> { templateInfo });
        }

        public async Task Update(IEnumerable<RMCustomizeConnectorTemplate> templateInfoes)
        {
            using var context = RMDBContextManager.GetNewDBContext();
            await Update(context, templateInfoes);
        }

        public async Task Update(RMDbContext context, IEnumerable<RMCustomizeConnectorTemplate> templateInfoes)
        {
            var now = DateTime.UtcNow.Ticks;
            var templateIds = templateInfoes.Select(item => item.Id).ToHashSet();
            var templateInfoesDic = templateInfoes.ToDictionary(item => item.Id, item => item);
            var existTemplateInfoes = await context.RMCustomizeConnectorTemplates.Where(item => templateIds.Contains(item.Id) && item.Origin != CustomizeConnectorOrigin.BuildIn).ToListAsync();

            foreach (var templateInfo in existTemplateInfoes)
            {
                var needUpdateColumnInfo = templateInfoesDic[templateInfo.Id];
                templateInfo.Name = needUpdateColumnInfo.Name;
                templateInfo.Description = needUpdateColumnInfo.Description;
                templateInfo.Modified = now;
                templateInfo.ModifiedBy = TenantLocalValue.LogonUserId;
            }

            context.RMCustomizeConnectorTemplates.AddOrUpdate(item => item.Id, existTemplateInfoes.ToArray());

            await context.SaveChangesAsync();
        }

        public async Task<IEnumerable<RMCustomizeConnectorTemplate>> GetAll()
        {
            using var context = RMDBContextManager.GetNewDBContext();

            var query = from template in context.RMCustomizeConnectorTemplates
                        join templateAndColumnMerge in context.RMCustomizeConnectorTemplateAndColumnMerges
                        on template.Id equals templateAndColumnMerge.TemplateId
                        join column in context.RMCustomizeConnectorColumns
                        on templateAndColumnMerge.ColumnId equals column.Id
                        select new
                        {
                            Template = template,
                            Column = column,
                            ColumnOrder = templateAndColumnMerge.Order
                        };
            var result = new List<RMCustomizeConnectorTemplate>();

            var items = await query.ToListAsync();
            var templateGroups = items.GroupBy(item => item.Template.Id).ToDictionary(item => item.Key, item => item.ToList());
            foreach(var templateGroup in templateGroups)
            {
                var templateInfo = templateGroup.Value.First().Template;

                var columns = templateGroup.Value.GroupBy(item => item.Column.Id)
                    .ToDictionary(item => item.Key, item => item.FirstOrDefault()).Values.ToList()
                    .ConvertAll(item =>
                    {
                        var column = item.Column;
                        column.Order = item.ColumnOrder;
                        return column;
                    });
                templateInfo.Columns = columns;

                result.Add(templateInfo);
            }

            return result;
        }
    }
}
