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
using AvePoint.RA.Contract.MachineLearning;
using AvePoint.RA.DB.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.DB.Dao.Impl
{
    public class MLManualEmailNotificationDao : BaseDao<RMMLManualEmailNotification>, IMLManualEmailNotificationDao
    {
        public void BatchAdd(MLManualEmailDto dto)
        {
            using var context = GetNewContext();
            var ids = dto.ReviewerIds;
            var existsIds = context.RMMLManualEmailNotifications.Where(o => o.JobId.Equals(dto.JobId)).Select(o => o.ReviewerId).ToList();
            ids = ids.Except(existsIds).ToList();
            if (ids.Any())
            {
                context.RMMLManualEmailNotifications.AddRange(ConvertToEntiry(dto));
                context.SaveChanges();
            }
        }

        public void Remove(string jobId)
        {
            using var context = GetNewContext();
            var items = context.RMMLManualEmailNotifications.Where(o => o.JobId.Equals(jobId)).ToList();
            if (items.Any())
            {
                context.RMMLManualEmailNotifications.RemoveRange(items);
                context.SaveChanges();
            }
        }

        private static List<RMMLManualEmailNotification> ConvertToEntiry(MLManualEmailDto dto)
        {
            var result = new List<RMMLManualEmailNotification>();
            foreach (var id in dto.ReviewerIds)
            {
                result.Add(new RMMLManualEmailNotification
                {
                    JobId = dto.JobId,
                    ReviewerId = id,
                });
            }
            return result;
        }

        public List<int> GetReviewerIds(string jobId)
        {
            using var context = GetNewContext();
            var reviewerIds = context.RMMLManualEmailNotifications.Where(o => o.JobId.Equals(jobId)).Select(o => o.ReviewerId).Distinct().ToList();
            return reviewerIds;
        }
    }
}
