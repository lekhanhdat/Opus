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
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.DB.Dao.Impl
{
    public class FileSystemTreeCacheDao : BaseDao<FileSystemTreeCache>, IFileSystemTreeCacheDao
    {
        public FileSystemTreeCache GetTreeNodeInfoByBatchId(Guid batchId)
        {
            using (var ctx = GetNewContext())
            {
                return ctx.FileSystemTreeCache.Where(c => c.BatchId == batchId).FirstOrDefault();
            }
        }

        public int SaveTreeNodeInfo(FileSystemTreeCache info)
        {
            using (var ctx = GetNewContext())
            {
                ctx.FileSystemTreeCache.Add(info);
                return ctx.SaveChanges();
            }
        }

        public async Task<bool> AddValidateConnectionIds(Guid batchId, List<Guid> connectionIds)
        {
            var cacheInfo = new FileSystemTreeCache
            {
                BatchId = batchId,
                TreeData = JsonConvert.SerializeObject(connectionIds)
            };

            using(var context = GetNewContext())
            {
                context.FileSystemTreeCache.Add(cacheInfo);
                return (await context.SaveChangesAsync()) > 0;
            }
        }
    }
}
