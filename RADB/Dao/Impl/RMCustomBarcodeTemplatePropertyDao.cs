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

namespace AvePoint.RA.DB.Dao.Impl
{
    public class RMCustomBarcodeTemplatePropertyDao : BaseDao<RMCustomBarcodeTemplateProperty>, IRMCustomBarcodeTemplatePropertyDao
    {
        public async Task<RMCustomBarcodeTemplateProperty> GetByIdAsync(int id)
        {
            using (var context = GetNewContext())
            {
                return await context.RMCustomBarcodeTemplateProperties.FirstOrDefaultAsync(p => p.Id == id);
            }
        }

        public async Task<List<RMCustomBarcodeTemplateProperty>> GetByTemplateIdAsync(int templateId)
        {
            using (var context = GetNewContext())
            {
                return await context.RMCustomBarcodeTemplateProperties
                    .Where(p => p.TemplateId == templateId)
                    .OrderBy(p => p.Id)
                    .ToListAsync();
            }
        }

        public async Task<List<RMCustomBarcodeTemplateProperty>> GetByTemplateIdAndPositionAsync(int templateId, BarcodeTemplatePosition position)
        {
            using (var context = GetNewContext())
            {
                return await context.RMCustomBarcodeTemplateProperties
                    .Where(p => p.TemplateId == templateId && p.Position == position)
                    .OrderBy(p => p.Id)
                    .ToListAsync();
            }
        }

        public async Task<List<RMCustomBarcodeTemplateProperty>> GetByTemplateIdsAsync(List<int> templateIds)
        {
            using (var context = GetNewContext())
            {
                return await context.RMCustomBarcodeTemplateProperties
                    .Where(p => templateIds.Contains(p.TemplateId))
                    .OrderBy(p => p.TemplateId)
                    .ThenBy(p => p.Id)
                    .ToListAsync();
            }
        }

        public async Task<int> CreateAsync(RMCustomBarcodeTemplateProperty property)
        {
            using (var context = GetNewContext())
            {
                context.RMCustomBarcodeTemplateProperties.Add(property);
                await context.SaveChangesAsync();
                return property.Id;
            }
        }

        public new async Task<bool> UpdateAsync(RMCustomBarcodeTemplateProperty property)
        {
            using (var context = GetNewContext())
            {
                var existingProperty = await context.RMCustomBarcodeTemplateProperties.FirstOrDefaultAsync(p => p.Id == property.Id);
                if (existingProperty == null)
                    return false;

                existingProperty.Name = property.Name;
                existingProperty.FontSize = property.FontSize;
                existingProperty.Position = property.Position;
                existingProperty.SortOrder = property.SortOrder;
                existingProperty.ModifiedTime = property.ModifiedTime;

                await context.SaveChangesAsync();
                return true;
            }
        }

        public async Task<bool> DeleteAsync(int id)
        {
            using (var context = GetNewContext())
            {
                var property = await context.RMCustomBarcodeTemplateProperties.FirstOrDefaultAsync(p => p.Id == id);
                if (property == null)
                    return false;

                context.RMCustomBarcodeTemplateProperties.Remove(property);
                await context.SaveChangesAsync();
                return true;
            }
        }

        public async Task<int> DeleteByTemplateIdAsync(int templateId)
        {
            using (var context = GetNewContext())
            {
                var properties = await context.RMCustomBarcodeTemplateProperties
                    .Where(p => p.TemplateId == templateId)
                    .ToListAsync();

                context.RMCustomBarcodeTemplateProperties.RemoveRange(properties);
                await context.SaveChangesAsync();
                return properties.Count;
            }
        }

        public async Task<bool> IsNameExistsAsync(int templateId, string name, int? excludeId = null)
        {
            using (var context = GetNewContext())
            {
                var query = context.RMCustomBarcodeTemplateProperties
                    .Where(p => p.TemplateId == templateId && p.Name == name);

                if (excludeId.HasValue)
                {
                    query = query.Where(p => p.Id != excludeId.Value);
                }

                return await query.AnyAsync();
            }
        }

        public async Task<int> BatchUpdateAsync(List<RMCustomBarcodeTemplateProperty> properties)
        {
            using (var context = GetNewContext())
            {
                var propertyIds = properties.Select(p => p.Id).ToList();
                var existingProperties = await context.RMCustomBarcodeTemplateProperties
                    .Where(p => propertyIds.Contains(p.Id))
                    .ToListAsync();

                int updatedCount = 0;
                foreach (var property in properties)
                {
                    var existingProperty = existingProperties.FirstOrDefault(p => p.Id == property.Id);
                    if (existingProperty != null)
                    {
                        existingProperty.Name = property.Name;
                        existingProperty.FontSize = property.FontSize;
                        existingProperty.Position = property.Position;
                        existingProperty.SortOrder = property.SortOrder;
                        existingProperty.ModifiedTime = property.ModifiedTime;
                        updatedCount++;
                    }
                }

                await context.SaveChangesAsync();
                return updatedCount;
            }
        }

        public async Task<List<int>> BatchCreateAsync(List<RMCustomBarcodeTemplateProperty> properties)
        {
            using (var context = GetNewContext())
            {
                context.RMCustomBarcodeTemplateProperties.AddRange(properties);
                await context.SaveChangesAsync();
                return properties.Select(p => p.Id).ToList();
            }
        }

        public async Task<int> BatchDeleteAsync(List<int> ids)
        {
            using (var context = GetNewContext())
            {
                var properties = await context.RMCustomBarcodeTemplateProperties
                    .Where(p => ids.Contains(p.Id))
                    .ToListAsync();

                context.RMCustomBarcodeTemplateProperties.RemoveRange(properties);
                await context.SaveChangesAsync();
                return properties.Count;
            }
        }
    }
}
