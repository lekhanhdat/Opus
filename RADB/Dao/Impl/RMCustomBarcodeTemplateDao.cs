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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Data.Entity;
using AvePoint.RA.DB.Model;
using AvePoint.RA.Contract.TemplateManagement;
using AvePoint.RA.DB.Core;
using AvePoint.RA.DB.Dao;

namespace AvePoint.RA.DB.Dao.Impl
{
    public class RMCustomBarcodeTemplateDao : BaseDao<RMCustomBarcodeTemplate>, IRMCustomBarcodeTemplateDao
    {
        public IRMBarcodeTemplateColumnMembershipDao BarcodeTemplateColumnMembershipDao { get; set; }

        public async Task<RMCustomBarcodeTemplate> GetByIdAsync(int id)
        {
            using (var context = GetNewContext())
            {
                return await context.RMCustomBarcodeTemplates.FirstOrDefaultAsync(t => t.Id == id);
            }
        }

        public async Task<List<RMCustomBarcodeTemplate>> GetBySuiteIdAsync(Guid suiteId)
        {
            using (var context = GetNewContext())
            {
                return await context.RMCustomBarcodeTemplates
                    .Where(t => t.SuiteId == suiteId)
                    .OrderBy(t => t.Id)
                    .ToListAsync();
            }
        }

        public async Task<List<RMCustomBarcodeTemplate>> GetBySuiteIdAndTypeAsync(Guid suiteId, BarcodeTemplateType type)
        {
            using (var context = GetNewContext())
            {
                return await context.RMCustomBarcodeTemplates
                    .Where(t => t.SuiteId == suiteId && t.Type == type)
                    .OrderBy(t => t.Id)
                    .ToListAsync();
            }
        }

        public async Task<RMCustomBarcodeTemplate> GetDefaultTemplateAsync(Guid suiteId, BarcodeTemplateType type)
        {
            using (var context = GetNewContext())
            {
                return await context.RMCustomBarcodeTemplates
                    .FirstOrDefaultAsync(t => t.SuiteId == suiteId && t.Type == type && t.IsDefault);
            }
        }

        public async Task<RMBarcodeTemplate> GetDefaultTemplateAsync(BarcodeTemplateType type)
        {
            using (var context = GetNewContext())
            {
                var customTemplate = await context.RMCustomBarcodeTemplates
                    .FirstOrDefaultAsync(t => t.Type == type && t.IsDefault);

                if (customTemplate == null)
                    return null;

                // Get column memberships for the type
                var columnMemberships = await BarcodeTemplateColumnMembershipDao.GetByTypeAsync((int)type);

                // Convert custom template to default template
                return ConvertUtil.ConvertCustomBarcodeTemplateToDefault(customTemplate, columnMemberships);
            }
        }
        
        public async Task<bool> CheckDefaultBarcodeTemplateExistByTypeAsync(BarcodeTemplateType type)
        {
            using (var context = GetNewContext())
            {
                return await context.RMCustomBarcodeTemplates
                    .AnyAsync(t => t.Type == type && t.IsDefault);
            }
        }

        public async Task<int> CreateAsync(RMCustomBarcodeTemplate template)
        {
            using (var context = GetNewContext())
            {
                context.RMCustomBarcodeTemplates.Add(template);
                await context.SaveChangesAsync();
                return template.Id;
            }
        }

        public new async Task<bool> UpdateAsync(RMCustomBarcodeTemplate template)
        {
            using (var context = GetNewContext())
            {
                var existingTemplate = await context.RMCustomBarcodeTemplates.FirstOrDefaultAsync(t => t.Id == template.Id);
                if (existingTemplate == null)
                    return false;

                existingTemplate.Name = template.Name;
                existingTemplate.Type = template.Type;
                existingTemplate.IsDefault = template.IsDefault;
                existingTemplate.PropertiesJson = template.PropertiesJson;

                await context.SaveChangesAsync();
                return true;
            }
        }

        public async Task<bool> DeleteAsync(int id)
        {
            using (var context = GetNewContext())
            {
                var template = await context.RMCustomBarcodeTemplates.FirstOrDefaultAsync(t => t.Id == id);
                if (template == null)
                    return false;

                context.RMCustomBarcodeTemplates.Remove(template);
                await context.SaveChangesAsync();
                return true;
            }
        }

        public async Task<int> DeleteBySuiteIdAsync(Guid suiteId)
        {
            using (var context = GetNewContext())
            {
                var templates = await context.RMCustomBarcodeTemplates
                    .Where(t => t.SuiteId == suiteId)
                    .ToListAsync();

                context.RMCustomBarcodeTemplates.RemoveRange(templates);
                await context.SaveChangesAsync();
                return templates.Count;
            }
        }

        public async Task<bool> IsNameExistsAsync(Guid suiteId, string name, int? excludeId = null)
        {
            using (var context = GetNewContext())
            {
                var query = context.RMCustomBarcodeTemplates
                    .Where(t => t.SuiteId == suiteId && t.Name == name);

                if (excludeId.HasValue)
                {
                    query = query.Where(t => t.Id != excludeId.Value);
                }

                return await query.AnyAsync();
            }
        }

        public async Task<bool> SetAsDefaultAsync(int id)
        {
            using (var context = GetNewContext())
            {
                var template = await context.RMCustomBarcodeTemplates.FirstOrDefaultAsync(t => t.Id == id);
                if (template == null)
                    return false;

                var existingDefaultTemplates = await context.RMCustomBarcodeTemplates
                    .Where(t => t.SuiteId == template.SuiteId && t.Type == template.Type && t.IsDefault)
                    .ToListAsync();

                foreach (var existingTemplate in existingDefaultTemplates)
                {
                    existingTemplate.IsDefault = false;
                }

                template.IsDefault = true;
                await context.SaveChangesAsync();
                return true;
            }
        }

        public async Task<int> BatchUpdateAsync(List<RMCustomBarcodeTemplate> templates)
        {
            using (var context = GetNewContext())
            {
                var templateIds = templates.Select(t => t.Id).ToList();
                var existingTemplates = await context.RMCustomBarcodeTemplates
                    .Where(t => templateIds.Contains(t.Id))
                    .ToListAsync();

                int updatedCount = 0;
                foreach (var template in templates)
                {
                    var existingTemplate = existingTemplates.FirstOrDefault(t => t.Id == template.Id);
                    if (existingTemplate != null)
                    {
                        existingTemplate.Name = template.Name;
                        existingTemplate.Type = template.Type;
                        existingTemplate.IsDefault = template.IsDefault;
                        existingTemplate.PropertiesJson = template.PropertiesJson;
                        updatedCount++;
                    }
                }

                await context.SaveChangesAsync();
                return updatedCount;
            }
        }

        public async Task<List<int>> BatchCreateAsync(List<RMCustomBarcodeTemplate> templates)
        {
            using (var context = GetNewContext())
            {
                context.RMCustomBarcodeTemplates.AddRange(templates);
                await context.SaveChangesAsync();
                return templates.Select(t => t.Id).ToList();
            }
        }

        public async Task<int> BatchDeleteAsync(List<int> ids)
        {
            using (var context = GetNewContext())
            {
                var templates = await context.RMCustomBarcodeTemplates
                    .Where(t => ids.Contains(t.Id))
                    .ToListAsync();

                context.RMCustomBarcodeTemplates.RemoveRange(templates);
                await context.SaveChangesAsync();
                return templates.Count;
            }
        }
    }
}
