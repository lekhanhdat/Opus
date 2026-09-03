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
using AvePoint.RA.Common.Cache;
using AvePoint.RA.Contract.CustomizeConnector.Enums;
using AvePoint.RA.Contract.Services;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.DB.Core;
using AvePoint.RA.DB.Explorer.Dao.CosmosImp;
using AvePoint.RA.DB.Model;
using Azure.Storage.Blobs.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Migrations;
using System.Linq;
using System.Threading.Tasks;

namespace AvePoint.RA.DB.Dao.Impl
{
    public class RMCustomizeConnectorContentSourceDao :BaseDao<RMCustomizeConnectorContentSource>, IRMCustomizeConnectorContentSourceDao
    {

        public IRMCustomizeConnectorTemplateDao CustomizeConnectorTemplateDao { get; set; }

        public IRMCustomizeConnectorColumnDao CustomizeConnectorColumnDao { get; set; }

        //public IRMCache Cache { get; set; }

        public async Task<RMCustomizeConnectorContentSource> Add(RMCustomizeConnectorContentSource contentSourceInfo)
        {

            using var context = RMDBContextManager.GetNewDBContext();
            using var transaction = context.Database.BeginTransaction();

            var now = DateTime.UtcNow.Ticks;
            var logonUser = TenantLocalValue.LogonUserId;

            contentSourceInfo.Id = Guid.NewGuid();
            contentSourceInfo.Created = now;
            contentSourceInfo.Modified = now;
            contentSourceInfo.CreatedBy = logonUser;
            contentSourceInfo.ModifiedBy = logonUser;
            contentSourceInfo.Origin = Contract.CustomizeConnector.Enums.CustomizeConnectorOrigin.ExternalCustomize;

            var explorerDao = new ExplorerDao(true);
            var existMaxFlagRecord = explorerDao.GetFirstOrDefaultByOrderDesc(item => item.SourceFlag >= 1000, item => item.SourceFlag);
            var explorerMaxFlag = existMaxFlagRecord?.SourceFlag ?? 0;
            var dbMaxFlag = (await context.RMCustomizeConnectorContentSources.OrderByDescending(item => item.Flag).FirstOrDefaultAsync()).Flag;
            var maxFlag = Math.Max(explorerMaxFlag, dbMaxFlag);
            contentSourceInfo.Flag = maxFlag < 1000 ? 1000 : maxFlag + 1;


            //await InvalidateCacheAsync();
            await RMCacheManager.SimpleInfoAdded();
            context.RMCustomizeConnectorContentSources.Add(contentSourceInfo);
            await context.SaveChangesAsync();


            

            var templates = await CustomizeConnectorTemplateDao.Add(context, contentSourceInfo.Templates);
            await RelatedContentSourceAndTemplates(context, contentSourceInfo.Id, templates.Select(item => item.Id).ToList());

            foreach (var template in templates)
            {
                var columns = await CustomizeConnectorColumnDao.Add(context, template.Columns);
                await RelatedTemplateAndColumns(context, template.Id, columns.ToList());
            }

            transaction.Commit();
            return contentSourceInfo;
        }

        public async Task Delete(Guid id)
        {
            using var context = RMDBContextManager.GetNewDBContext();
            using var transaction = context.Database.BeginTransaction();

            var existEntity = await Get(id);
            foreach(var template in existEntity.Templates)
            {
                await CustomizeConnectorColumnDao.Delete(context, template.Columns.Select(item => item.Id));
                await CustomizeConnectorTemplateDao.Delete(context, template.Id);

                await UnRelatedTemplateAndColumns(context, template.Id);
            }

            var wiilRemovedContentSource = await context.RMCustomizeConnectorContentSources.FirstOrDefaultAsync(item => item.Id == id);
            context.RMCustomizeConnectorContentSources.Remove(wiilRemovedContentSource);
            //await InvalidateCacheAsync();
            await RMCacheManager.SimpleInfoDeleted();
            await context.SaveChangesAsync();

            await UnRelatedContentSourceAndTemplates(context, existEntity.Id);

            transaction.Commit();
        }

        public async Task Delete(IEnumerable<Guid> ids)
        {
            foreach(var id in ids)
            {
                await Delete(id);
            }
        }

        public async Task<RMCustomizeConnectorContentSource> Get(Guid id)
        {
            using var context = RMDBContextManager.GetNewDBContext();
            var query = from contentSource in context.RMCustomizeConnectorContentSources
                        join contentSourceAndTemplateMerge in context.RMCustomizeConnectorSourceAndTemplateMerges
                        on contentSource.Id equals contentSourceAndTemplateMerge.SourceId
                        join template in context.RMCustomizeConnectorTemplates
                        on contentSourceAndTemplateMerge.TemplateId equals template.Id
                        join templateAndColumnMerge in context.RMCustomizeConnectorTemplateAndColumnMerges
                        on template.Id equals templateAndColumnMerge.TemplateId
                        join column in context.RMCustomizeConnectorColumns
                        on templateAndColumnMerge.ColumnId equals column.Id
                        where contentSource.Id == id
                        select new
                        {
                            ContentSource = contentSource,
                            Template = template,
                            ColumnOrder = templateAndColumnMerge.Order,
                            Column = column
                        };

            var res = new RMCustomizeConnectorContentSource();

            var items = await query.ToListAsync();
            var contentSourceInfo = items.First().ContentSource;
            res.Id = contentSourceInfo.Id;
            res.Name = contentSourceInfo.Name;
            res.Description = contentSourceInfo.Description;
            res.Created = contentSourceInfo.Created;
            res.Modified = contentSourceInfo.Modified;
            res.CreatedBy = contentSourceInfo.CreatedBy;
            res.ModifiedBy = contentSourceInfo.ModifiedBy;
            res.Origin = contentSourceInfo.Origin;
            res.Flag = contentSourceInfo.Flag;

            var templateGroups = items.GroupBy(item => item.Template.Id).ToDictionary(item => item.Key, item => item.ToList());
            foreach (var templateGroup in templateGroups)
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

                res.Templates.Add(templateInfo);
            }

            return res;
        }

        public async Task<bool> Exist(Guid id)
        {
            using var context = RMDBContextManager.GetNewDBContext();

            return await context.RMCustomizeConnectorContentSources.AsNoTracking().AnyAsync(item => item.Id == id);
        }

        public async Task<IEnumerable<RMCustomizeConnectorContentSource>> GetAllSimpleInfoes(params CustomizeConnectorOrigin[] origins)
        {
            using var context = RMDBContextManager.GetNewDBContext();

            return await context.RMCustomizeConnectorContentSources
                .Where(item => Enumerable.Contains(origins, item.Origin))
                .OrderByDescending(item => item.Modified)
                .ToListAsync();
        }

        public async Task<RMCustomizeConnectorContentSource> GetSimpleInfoByName(string name)
        {
            using var context = RMDBContextManager.GetNewDBContext();

            return await context.RMCustomizeConnectorContentSources.FirstOrDefaultAsync(item => item.Name == name);
        }

        public async Task Update(RMCustomizeConnectorContentSource contentSourceInfo)
        {
            using var context = RMDBContextManager.GetNewDBContext();
            using var transaction = context.Database.BeginTransaction();

            var now = DateTime.UtcNow.Ticks;
            var existEntity = await Get(contentSourceInfo.Id);

            existEntity.Name = contentSourceInfo.Name;
            existEntity.Description = contentSourceInfo.Description;
            existEntity.Modified = now;
            existEntity.ModifiedBy = TenantLocalValue.LogonUserId;

            context.RMCustomizeConnectorContentSources.AddOrUpdate(item => item.Id, existEntity);
            await RMCacheManager.SimpleInfoUpdated();
            await context.SaveChangesAsync();

            var template = contentSourceInfo.Templates.First();
            template.Id = existEntity.Templates.First().Id;
            await CustomizeConnectorTemplateDao.Update(context, template);

            var needAddedColumns = template.Columns.Where(item => item.Id == Guid.Empty).ToList();
            var needUpdateColumns = template.Columns.IntersectBy(existEntity.Templates.First().Columns.Select(item => item.Id), item => item.Id).ToList();
            var needDeleteColumns = existEntity.Templates.First().Columns.ExceptBy(template.Columns.Select(item => item.Id), item => item.Id).ToList();
            needAddedColumns = (await CustomizeConnectorColumnDao.Add(context, needAddedColumns)).ToList();
            await CustomizeConnectorColumnDao.Update(context, needUpdateColumns);
            await CustomizeConnectorColumnDao.Delete(context, needDeleteColumns.Select(item => item.Id));

            await UnRelatedTemplateAndColumns(context, existEntity.Templates.First().Id);
            await RelatedTemplateAndColumns(context, existEntity.Templates.First().Id, needAddedColumns.Concat(needUpdateColumns).ToList());

            transaction.Commit();
        }

        private static async Task UnRelatedContentSourceAndTemplates(RMDbContext context, Guid contentSourceId)
        {
            var merges = await context.RMCustomizeConnectorSourceAndTemplateMerges.Where(item => item.SourceId == contentSourceId).ToListAsync();

            context.RMCustomizeConnectorSourceAndTemplateMerges.RemoveRange(merges);
            await context.SaveChangesAsync();
        }

        private static async Task UnRelatedTemplateAndColumns(RMDbContext context, Guid templateId)
        {
            var merges = await context.RMCustomizeConnectorTemplateAndColumnMerges.Where(item => item.TemplateId == templateId).ToListAsync();

            context.RMCustomizeConnectorTemplateAndColumnMerges.RemoveRange(merges);
            await context.SaveChangesAsync();
        }

        private static Task RelatedContentSourceAndTemplates(RMDbContext context, Guid contentSourceId, List<Guid> templateIds)
        {
            var merges = templateIds.ConvertAll(item => new RMCustomizeConnectorSourceAndTemplateMerge
            {
                SourceId = contentSourceId,
                TemplateId = item
            });

            context.RMCustomizeConnectorSourceAndTemplateMerges.AddRange(merges);

            return context.SaveChangesAsync();
        }

        private static Task RelatedTemplateAndColumns(RMDbContext context, Guid templateId, List<RMCustomizeConnectorColumn> columns)
        {
            var merges = columns.ConvertAll(item => new RMCustomizeConnectorTemplateAndColumnMerge
            {
                TemplateId = templateId,
                ColumnId = item.Id,
                Order = item.Order
            });

            context.RMCustomizeConnectorTemplateAndColumnMerges.AddRange(merges);
            return context.SaveChangesAsync();
        }
    }
}
