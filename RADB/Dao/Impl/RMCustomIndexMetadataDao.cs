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
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.DB.Core;
using AvePoint.RA.DB.Model;
using AvePoint.RA.DB.Model.Discovery;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Migrations;
using System.Linq;
using System.Threading.Tasks;

namespace AvePoint.RA.DB.Dao.Impl
{
    public class RMCustomIndexMetadataDao : IRMCustomIndexMetadataDao
    {

        public async Task AddOrUpdateCustomIndexMetadatasAsync(params RMCustomIndexMetadata[] customIndexMetadatas)
        {
            using var context = RMDBContextManager.GetNewDBContext();
            context.RMCustomIndexMetadatas.AddOrUpdate(customIndexMetadatas);
            await context.SaveChangesAsync();
        }

        public async Task<IEnumerable<RMCustomIndexMetadata>> GetAllCustomIndexMetadatasAsync()
        {
            using var context = RMDBContextManager.GetNewDBContext();
            return await context.RMCustomIndexMetadatas.AsNoTracking().ToListAsync();
        }

        public async Task DeleteCustomIndexMetadataAsync(params RMCustomIndexMetadata[] customIndexMetadatas)
        {
            using var context = RMDBContextManager.GetNewDBContext();
            foreach (var data in customIndexMetadatas)
            {
                context.RMCustomIndexMetadatas.Attach(data);
                context.RMCustomIndexMetadatas.Remove(data);
            }
            await context.SaveChangesAsync();
        }

        public async Task DeleteCustomIndexMetadataAsync(SourceFlag sourceFlag)
        {
            using var context = RMDBContextManager.GetNewDBContext();
            var customIndexMetadatas = context.RMCustomIndexMetadatas.Where(item => item.ContentSource == sourceFlag);
            foreach (var data in customIndexMetadatas)
            {
                context.RMCustomIndexMetadatas.Attach(data);
                context.RMCustomIndexMetadatas.Remove(data);
            }
            await context.SaveChangesAsync();
        }

        public async Task DeleteAllCustomIndexMetadataAsync()
        {
            using var context = RMDBContextManager.GetNewDBContext();
            context.RMCustomIndexMetadatas.RemoveRange(context.RMCustomIndexMetadatas.ToArray());
            await context.SaveChangesAsync();
        }

        public async Task<IEnumerable<RMCustomIndexMetadata>> GetCustomIndexMetadatasBySourceFlagAsync(SourceFlag sourceFlag)
        {
            using var context = RMDBContextManager.GetNewDBContext();
            return await context.RMCustomIndexMetadatas.AsNoTracking().Where(item => item.ContentSource == sourceFlag).ToListAsync();
        }
    }
}
