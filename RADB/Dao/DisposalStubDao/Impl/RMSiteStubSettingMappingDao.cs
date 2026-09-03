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
using AvePoint.GCommon.Contract.Storage.Entity;
using AvePoint.GCommon.Utility;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.DB.Dao.Impl;
using AvePoint.RA.DB.Model.DisposalStub;
using DocumentFormat.OpenXml.Office2010.Excel;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Migrations;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace AvePoint.RA.DB.Dao.DisposalStubDao.Impl
{
    public class RMSiteStubSettingMappingDao : BaseDao<RMSiteStubSettingMapping>, IRMSiteStubSettingMappingDao
    {
        private RALogger logger = RALogger.GetInstance(typeof(RMSiteStubSettingMappingDao));

        public async Task<RMSiteStubSettingMapping> GetMappingBySiteUrlAndTemplateIdAsync(string siteUrl, Guid templateId)
        {
            using (var context = GetNewContext())
            {
                var existResult = await context.RMSiteStubSettingMappings.FirstOrDefaultAsync(m => m.SiteCollectionUrl == siteUrl && m.StubTemplateId == templateId);

                if (existResult == null)
                {
                    logger.Info($"No existing RMSiteStubSettingMapping found for siteUrl: {siteUrl}, templateId: {templateId}");
                }

                return existResult;
            }
        }

        public async Task<List<RMSiteStubSettingMapping>> GetAllMappingsBySiteUrlAsync(string siteUrl)
        {
            using (var context = GetNewContext())
            {
                var existResults = await context.RMSiteStubSettingMappings.Where(m => m.SiteCollectionUrl == siteUrl).ToListAsync();

                if (existResults == null || existResults.Count == 0)
                {
                    logger.Info($"No existing RMSiteStubSettingMapping found for siteUrl: {siteUrl}");
                }
                return existResults;
            }
        }

        public async Task<List<RMSiteStubSettingMapping>> GetAllMappingsByStubTemplateAsync(Guid stubTemplate)
        {
            using (var context = GetNewContext())
            {
                var existResults = await context.RMSiteStubSettingMappings.Where(m => m.StubTemplateId == stubTemplate).ToListAsync();

                if (existResults == null || existResults.Count == 0)
                {
                    logger.Info($"No existing RMSiteStubSettingMapping found for stub template: {stubTemplate}");
                }
                return existResults;
            }
        }

        public async Task AddOrUpdateMappingAsync(RMSiteStubSettingMapping mapping)
        {
            ArgumentNullException.ThrowIfNull(mapping);

            if (mapping.Id == Guid.Empty)
            {
                logger.Info($"Create new mapping for siteUrl: {mapping.SiteCollectionUrl}, templateId: {mapping.StubTemplateId}");
                mapping.Id = Guid.NewGuid();
            }

            using (var context = GetNewContext())
            {

                context.RMSiteStubSettingMappings.AddOrUpdate(mapping);
                await context.SaveChangesAsync();
            }
        }

        public void UpdateFirstStubCreateTimeBySiteStlp(string templateId, string siteUrl, long firstStubCreateTime)
        {
            using (var context = GetNewContext())
            {
                string sql = $"update {SecurityUtils.SanitizeSQLSchemaName(context.SchemaName)}.RMSiteStubSettingMappings set {nameof(RMSiteStubSettingMapping.FirstStubCreatedTime)} = @FirstStubCreatedTime where {nameof(RMSiteStubSettingMapping.StubTemplateId)} = @StubTemplateId and {nameof(RMSiteStubSettingMapping.SiteCollectionUrl)} = @SiteCollectionUrl";

                int result = context.Database.ExecuteSqlCommand(string.Format(sql, context.SchemaName),
                    new SqlParameter("StubTemplateId", templateId),
                    new SqlParameter("SiteCollectionUrl", siteUrl),
                    new SqlParameter("FirstStubCreatedTime", firstStubCreateTime)
                    );
            }
        }

        public void UpdateRetentionInfoByStubTemplateId(Guid templateId, bool isEnableRetention, int retentionValue, int retentionUnit)
        {
            using (var context = GetNewContext())
            {
                string sql = $"update {SecurityUtils.SanitizeSQLSchemaName(context.SchemaName)}.RMSiteStubSettingMappings set " +
                    $"{nameof(RMSiteStubSettingMapping.IsEnabledRetention)} = @IsEnabledRetention, " +
                    $"{nameof(RMSiteStubSettingMapping.RetentionValue)} = @RetentionValue, " +
                    $"{nameof(RMSiteStubSettingMapping.RetentionUnit)} = @RetentionUnit where StubTemplateId = @StubTemplateId";

                int result = context.Database.ExecuteSqlCommand(string.Format(sql, context.SchemaName),
                    new SqlParameter("IsEnabledRetention", isEnableRetention),
                    new SqlParameter("RetentionValue", retentionValue),
                    new SqlParameter("RetentionUnit", retentionUnit),
                    new SqlParameter("StubTemplateId", templateId)
                    );
            }
        }

        public void DeleteMappingsByTemplateId(Guid templateId)
        {
            using (var context = GetNewContext())
            {
                string sql = $"delete {SecurityUtils.SanitizeSQLSchemaName(context.SchemaName)}.RMSiteStubSettingMappings " +
                    $"where StubTemplateId = @StubTemplateId";

                int result = context.Database.ExecuteSqlCommand(string.Format(sql, context.SchemaName),
                    new SqlParameter("StubTemplateId", templateId)
                    );
            }
        }

        public void DeleteMappingBySiteUrlAndTemplateId(string siteUrl, Guid templateId)
        {
            using (var context = GetNewContext())
            {
                string sql = $"delete {SecurityUtils.SanitizeSQLSchemaName(context.SchemaName)}.RMSiteStubSettingMappings " +
                    $"where {nameof(RMSiteStubSettingMapping.SiteCollectionUrl)} = @SiteCollectionUrl and " +
                    $"{nameof(RMSiteStubSettingMapping.StubTemplateId)} = @StubTemplateId";

                int result = context.Database.ExecuteSqlCommand(string.Format(sql, context.SchemaName),
                    new SqlParameter("SiteCollectionUrl", siteUrl),
                    new SqlParameter("StubTemplateId", templateId)
                    );
            }
        }
    }
}
