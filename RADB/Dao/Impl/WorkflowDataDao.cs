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
using AvePoint.GCommon.Contract.PlatformRecovery;
using AvePoint.RA.DB.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.DB.Dao.Impl
{
    public class WorkflowDataDao : BaseDao<RMWorkflowData>, IWorkflowDataDao
    {
        public void DeleteById(Guid id)
        {
            try
            {
                using (var context = GetNewContext())
                {
                    base.DeleteByKey(id);
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        public RMWorkflowData GetById(Guid id)
        {
            try
            {
                using (var context = GetNewContext())
                {
                    return context.WorkflowData.Where(o => o.Id == id).FirstOrDefault();
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<bool> SaveAsync(RMWorkflowData data)
        {
            try
            {
                using (var context = GetNewContext())
                {
                    var exist = context.WorkflowData.Where(o => o.Id == data.Id).Select(o => o.Id).FirstOrDefault();
                    if (exist == Guid.Empty)
                    {
                        context.WorkflowData.Add(data);
                        return context.SaveChanges() > 0;
                    }
                    else
                    {
                        return await UpdateAsync(data);
                    }
                }
            }
            catch (Exception)
            {

                throw;
            }
        }

    }
}
