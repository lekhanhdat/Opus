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
using AvePoint.RA.DB.Core;
using AvePoint.RA.DB.Model;
using Castle.Core.Resource;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.DB.Dao.Impl
{
    public class RMGlobalKeyValueDao : BaseDao<RMGlobalKeyValue>, IRMGlobalKeyValueDao
    {
        public bool Save(RMGlobalKeyValue entity)
        {
            using (var ctx = RMDBContextManager.GetSystemDBContext())
            {
                if (ctx.RMGlobalKeyValue.Any(m => m.Key.Equals(entity.Key)))
                {
                    var module = ctx.RMGlobalKeyValue.Where(m => m.Key.Equals(entity.Key)).FirstOrDefault();
                    module.Value = entity.Value;
                    return ctx.SaveChanges() > 0;
                }
                else
                {
                    ctx.RMGlobalKeyValue.Add(entity);
                    return ctx.SaveChanges() > 0;
                }
            }
        }

        public RMGlobalKeyValue GetValueByKey(string key)
        {
            using (var ctx = RMDBContextManager.GetSystemDBContext())
            {
                var setting = ctx.RMGlobalKeyValue.Where(k => k.Key.Equals(key)).FirstOrDefault();
                return setting;
            }
        }
    }
}
