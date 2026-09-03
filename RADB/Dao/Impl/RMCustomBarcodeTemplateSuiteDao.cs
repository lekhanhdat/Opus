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
    public class RMCustomBarcodeTemplateSuiteDao : BaseDao<RMCustomBarcodeTemplateSuite>, IRMCustomBarcodeTemplateSuiteDao
    {
        public async Task<RMCustomBarcodeTemplateSuite> GetByIdAsync(int id)
        {
            using (var context = GetNewContext())
            {
                return await context.RMCustomBarcodeTemplateSuites.FirstOrDefaultAsync(s => s.Id == id);
            }
        }

        public async Task<RMCustomBarcodeTemplateSuite> GetByUniqueIdAsync(Guid uniqueId)
        {
            using (var context = GetNewContext())
            {
                return await context.RMCustomBarcodeTemplateSuites.FirstOrDefaultAsync(s => s.UniqueId == uniqueId);
            }
        }

        public async Task<List<RMCustomBarcodeTemplateSuite>> GetByUniqueIdsAsync(List<Guid> uniqueIds)
        {
            if (uniqueIds == null || uniqueIds.Count == 0)
            {
                return new List<RMCustomBarcodeTemplateSuite>();
            }
            using (var context = GetNewContext())
            {
                return await context.RMCustomBarcodeTemplateSuites
                    .Where(s => uniqueIds.Contains(s.UniqueId))
                    .OrderBy(s => s.Name)
                    .ThenBy(s => s.Id)
                    .ToListAsync();
            }
        }

        public async Task<List<RMCustomBarcodeTemplateSuite>> GetAllAsync()
        {
            using (var context = GetNewContext())
            {
                return await context.RMCustomBarcodeTemplateSuites
                    .OrderBy(s => s.Name)
                    .ThenBy(s => s.Id)
                    .ToListAsync();
            }
        }

        public async Task<List<RMCustomBarcodeTemplateSuite>> GetByLabelTypeAsync(BarcodeTemplateLabelType labelType)
        {
            using (var context = GetNewContext())
            {
                return await context.RMCustomBarcodeTemplateSuites
                    .Where(s => s.LabelType == labelType)
                    .OrderBy(s => s.Name)
                    .ThenBy(s => s.Id)
                    .ToListAsync();
            }
        }

        public async Task<RMCustomBarcodeTemplateSuite> GetDefaultAsync()
        {
            using (var context = GetNewContext())
            {
                return await context.RMCustomBarcodeTemplateSuites
                    .FirstOrDefaultAsync(s => s.IsDefault);
            }
        }

        public async Task<RMCustomBarcodeTemplateSuite> GetDefaultByLabelTypeAsync(BarcodeTemplateLabelType labelType)
        {
            using (var context = GetNewContext())
            {
                return await context.RMCustomBarcodeTemplateSuites
                    .FirstOrDefaultAsync(s => s.IsDefault && s.LabelType == labelType);
            }
        }

        public async Task<RMCustomBarcodeTemplateSuite> GetByNameAsync(string name)
        {
            using (var context = GetNewContext())
            {
                return await context.RMCustomBarcodeTemplateSuites
                    .FirstOrDefaultAsync(s => s.Name == name);
            }
        }

        public async Task<int> CreateAsync(RMCustomBarcodeTemplateSuite suite)
        {
            using (var context = GetNewContext())
            {
                context.RMCustomBarcodeTemplateSuites.Add(suite);
                await context.SaveChangesAsync();
                return suite.Id;
            }
        }

        public new async Task<bool> UpdateAsync(RMCustomBarcodeTemplateSuite suite)
        {
            using (var context = GetNewContext())
            {
                var existingSuite = await context.RMCustomBarcodeTemplateSuites.FirstOrDefaultAsync(s => s.Id == suite.Id);
                if (existingSuite == null)
                    return false;

                existingSuite.Name = suite.Name;
                existingSuite.Description = suite.Description;
                existingSuite.LabelType = suite.LabelType;
                existingSuite.IsDefault = suite.IsDefault;
                existingSuite.ModifiedTime = suite.ModifiedTime;

                await context.SaveChangesAsync();
                return true;
            }
        }

        public async Task<bool> DeleteAsync(int id)
        {
            using (var context = GetNewContext())
            {
                var suite = await context.RMCustomBarcodeTemplateSuites.FirstOrDefaultAsync(s => s.Id == id);
                if (suite == null)
                    return false;

                context.RMCustomBarcodeTemplateSuites.Remove(suite);
                await context.SaveChangesAsync();
                return true;
            }
        }

        public async Task<bool> DeleteByUniqueIdAsync(Guid uniqueId)
        {
            using (var context = GetNewContext())
            {
                var suite = await context.RMCustomBarcodeTemplateSuites.FirstOrDefaultAsync(s => s.UniqueId == uniqueId);
                if (suite == null)
                    return false;

                context.RMCustomBarcodeTemplateSuites.Remove(suite);
                await context.SaveChangesAsync();
                return true;
            }
        }

        public async Task<bool> IsNameExistsAsync(string name, int? excludeId = null)
        {
            using (var context = GetNewContext())
            {
                var query = context.RMCustomBarcodeTemplateSuites
                    .Where(s => s.Name == name);

                if (excludeId.HasValue)
                {
                    query = query.Where(s => s.Id != excludeId.Value);
                }

                return await query.AnyAsync();
            }
        }

        public async Task<(List<RMCustomBarcodeTemplateSuite> Suites, int TotalCount)> GetPagedAsync(int pageIndex, int pageSize, string searchName = null, BarcodeTemplateLabelType? labelType = null)
        {
            using (var context = GetNewContext())
            {
                var query = context.RMCustomBarcodeTemplateSuites.AsQueryable();

                if (!string.IsNullOrWhiteSpace(searchName))
                {
                    query = query.Where(s => s.Name.Contains(searchName));
                }

                if (labelType.HasValue)
                {
                    query = query.Where(s => s.LabelType == labelType.Value);
                }

                var totalCount = await query.CountAsync();

                var suites = await query
                    .OrderBy(s => s.Name)
                    .ThenBy(s => s.Id)
                    .Skip(pageIndex * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                return (suites, totalCount);
            }
        }

        public async Task<List<RMCustomBarcodeTemplateSuite>> SearchByNameAsync(string searchName, BarcodeTemplateLabelType? labelType = null)
        {
            using (var context = GetNewContext())
            {
                var query = context.RMCustomBarcodeTemplateSuites.AsQueryable();

                if (!string.IsNullOrWhiteSpace(searchName))
                {
                    query = query.Where(s => s.Name.Contains(searchName));
                }

                if (labelType.HasValue)
                {
                    query = query.Where(s => s.LabelType == labelType.Value);
                }

                return await query
                    .OrderBy(s => s.Name)
                    .ThenBy(s => s.Id)
                    .ToListAsync();
            }
        }

        public async Task<int> GetCountAsync(string searchName = null, BarcodeTemplateLabelType? labelType = null)
        {
            using (var context = GetNewContext())
            {
                var query = context.RMCustomBarcodeTemplateSuites.AsQueryable();

                if (!string.IsNullOrWhiteSpace(searchName))
                {
                    query = query.Where(s => s.Name.Contains(searchName));
                }

                if (labelType.HasValue)
                {
                    query = query.Where(s => s.LabelType == labelType.Value);
                }

                return await query.CountAsync();
            }
        }
    }
}
