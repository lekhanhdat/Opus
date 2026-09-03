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
using AvePoint.RA.CommonUtil;
using AvePoint.RA.DB.Explorer;
using AvePoint.RA.DB.Model;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AvePoint.RA.DB.Dao.Impl
{
    public class RMScopeDao : BaseDao<RMScope>, IRMScopeDao
    {
       // private static readonly RALogger logger = RALogger.GetInstance(typeof(RMScopeDao));
        public void AddOrUpateSiteScope(RMScope scope)
        {
            using (var ctx = GetRMDbContext())
            {
                if (!ctx.Scope.Any(s => s.ScopeId == scope.ScopeId))
                {
                    ctx.Scope.Add(scope);
                    ctx.SaveChanges();
                }
                else
                {
                    var site = ctx.Scope.Where(s => s.ScopeId == scope.ScopeId).FirstOrDefault();

                    site.FullPath = scope.FullPath;
                    site.ScopeName = scope.ScopeName;
                    ApplyCurrentValues(ctx, site);
                }
            }
        }
    
        public List<RMScope> GetExistScopeInfo()
        {
            using (var ctx = GetRMDbContext())
            {
                return ctx.Scope.AsQueryable().Where(s => s.IsRemoved == false).ToList();
            }
        }

        private Core.RMDbContext GetRMDbContext()
        {
            return Core.RMDBContextManager.GetNewDBContext();
        }

        public Dictionary<Guid, RMScope> GetScopeInfoByIds(List<Guid> ids)
        {
            using (var ctx = GetRMDbContext())
            {
                return ctx.Scope.AsQueryable().Where(s => s.IsRemoved == false && ids.Contains(s.ScopeId)).ToDictionary(o => o.ScopeId);
            }
        }

        public List<Guid> GetScopeIds(List<string> scopePaths)
        {
            var scopeIds = new List<Guid>();    
            if (scopePaths != null && scopePaths.Any())
            {
                using var ctx = GetRMDbContext();
                scopeIds =  ctx.Scope.AsQueryable().Where(s => scopePaths.Contains(s.FullPath)).Select(s => s.ScopeId).ToList();
            }
            return scopeIds;
        }
    }
}
