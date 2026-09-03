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
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AvePoint.RA.DB.Model;

namespace AvePoint.RA.DB.Dao.Impl
{
    public class MobileHistoryDao : BaseDao<RMMobileHistory>, IMobileHistoryDao
    {
        public void AddHistory(List<RMMobileHistory> histories)
        {
            using (var ctx = GetNewContext())
            {
                ctx.RMMobileHistory.AddRange(histories);
                ctx.SaveChanges();
            }
        }

        public List<RMMobileHistory> GetHistoryByUserId(string userEmail, int pageSize, int pageIndex)
        {
            var result = new List<RMMobileHistory>();
            using (var ctx = GetNewContext())
            {
                result = ctx.RMMobileHistory.AsNoTracking().Where(h => h.UserEmail.Equals(userEmail, StringComparison.OrdinalIgnoreCase)).OrderByDescending(o => o.Id).Skip(pageIndex * pageSize).Take(pageSize).ToList();
            }
            return result;
        }

        public List<RMMobileHistory> GetHistoryByIds(List<int> ids)
        {
            var result = new List<RMMobileHistory>();
            using (var ctx = GetNewContext())
            {
                result = ctx.RMMobileHistory.AsNoTracking().Where(h => ids.Contains(h.Id)).ToList();
            }
            return result;
        }

        public async Task UpdateHistoryStatusAsync(int id, int status)
        {
            using (var ctx = GetNewContext())
            {
                var history = ctx.RMMobileHistory.Where(h => id == h.Id).FirstOrDefault();
                if(history != null)
                {
                    history.Status = status;
                    await UpdateAsync(history);
                }
            }
        }
    }
}
