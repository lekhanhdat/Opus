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
using AvePoint.GCommon.Contract.CommonFilter;
using AvePoint.GCommon.Contract.StorageOptimization.Object;
using AvePoint.GCommon.Utility;
using AvePoint.RA.Common;
using AvePoint.RA.Common.Aos;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.DocAve;
using AvePoint.RA.Contract.Label;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.RMRuleManageMent;
using AvePoint.RA.Contract.RMWeb.CP;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.DB.Core;
using AvePoint.RA.DB.Model;
using DocumentFormat.OpenXml.Office2010.Excel;
using Microsoft365.SharePoint.WebService.Lists;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Validation;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.DB.Dao.Impl
{
    public class LabelDao : BaseDao<RMLabel>, ILabelDao
    {
        public async Task<RMLabel> GetLabelByUniqueIdAsync(string uniqueId, bool includeDelete = false)
        {
            using (var ctx = GetNewContext())
            {
                if (includeDelete)
                {
                    return await ctx.RMLabels.AsQueryable().FirstOrDefaultAsync(x => x.UniqueId == new Guid(uniqueId));
                }
                else
                {
                    return await ctx.RMLabels.AsQueryable().FirstOrDefaultAsync(x => x.UniqueId == new Guid(uniqueId) && !x.IsDeleted);
                }
            }
        }

        public RMLabel GetLabelByUniqueId(Guid uniqueId)
        {
            using var context = GetNewContext();
            RMLabel label = context.RMLabels.AsQueryable().Where(tm => tm.UniqueId.Equals(uniqueId)).FirstOrDefault();

            return label;
        }

        public async Task<List<RMLabel>> GetAllLabelsAsync()
        {
            using (var ctx = GetNewContext())
            {
                var labels = await ctx.RMLabels.Where(l => !l.IsDeleted).ToListAsync();
                return labels;
            }
        }

        public async Task<List<RMTerm>> GetLabelsByIdsAsync(List<string> labelIds)
        {
            using var context = GetNewContext();
            return await context.Terms.AsNoTracking().Where(label => labelIds.Contains(label.UniqueId.ToString())).ToListAsync();
        }
    }
}
