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
using AvePoint.RA.DB.Model;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.DB.Dao.Impl
{
    public class FeatureUsageLimitDao : BaseDao<FeatureUsageLimit>, IFeatureUsageLimitDao
    {
        private static readonly int LimitUsageAIRecommendation = 100;
        private static readonly int LimitUsageEmbedding = 5000;

        public async Task<bool> CheckUsageLimit(FeatureType featureType)
        {
            using var context = GetNewContext();
            var feature = await context.FeatureUsageLimits.FirstOrDefaultAsync(e => e.FeatureType == featureType);
            if (feature == null)
                return true;
            return feature.Usaged < feature.LimitUsage;
        }

        public void AddOrUpdate(FeatureType featureType)
        {
            using var context = GetNewContext();

            var feature = context.FeatureUsageLimits.FirstOrDefault(e => e.FeatureType == featureType);
            if(feature == null)
            {
                var model = new FeatureUsageLimit
                {
                    FeatureType = featureType,
                    Usaged = 1,
                    LimitUsage = featureType == FeatureType.AIRecommendation ? LimitUsageAIRecommendation : LimitUsageEmbedding,
                    LastUpdatTime = DateTime.UtcNow,
                };
                context.FeatureUsageLimits.Add(model);
            }
            else
            {
                feature.Usaged += 1;
                feature.LastUpdatTime = DateTime.UtcNow;
            }
            context.SaveChanges();
        }

        public void ClearUsage()
        {
            try
            {
                using var context = GetNewContext();
                var features = context.FeatureUsageLimits.ToList(); 
                foreach (var feature in features)
                {
                    feature.Usaged = 0;     
                }
                context.SaveChanges();
            }
            catch(Exception ex)
            {
                throw;
            }
        }

        public async Task<FeatureUsageLimit> GetFeatureUsageLimit(FeatureType featureType)
        {
            using var context = GetNewContext();
            var feature = await context.FeatureUsageLimits.FirstOrDefaultAsync(e => e.FeatureType == featureType);
            return feature;
        }
    }
}
