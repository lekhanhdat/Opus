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
using AvePoint.GCommon.Utility;
using AvePoint.RA.Common.Util;
using AvePoint.RA.Contract.Security;
using AvePoint.RA.DB.Model;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;

namespace AvePoint.RA.DB.Dao.Impl
{
    public class EncryptionDataDao : BaseDao<RMEncryptionData>, IEncryptionDataDao
    {
        public const string TABLE_NAME = "RMEncryptionDatas";

        public RMEncryptionDataInfo Add(RMEncryptionDataInfo data)
        {
            return ExecuteWithRetry(context =>
            {
                var entity = Convert(data);
                entity.UpdateTime = DateTime.UtcNow.Ticks;
                context.EncryptionData.Add(entity);
                context.SaveChanges();
                return Convert(entity);
            });
        }

        public IEnumerable<RMEncryptionDataInfo> GetAll()
        {
            var allData = new List<RMEncryptionDataInfo>();
            List<RMEncryptionData> tempData = null;
            int pageSize = 2000;
            int pageIndex = 1;

            do
            {
                tempData = GetAll(pageSize, pageIndex);
                allData.AddRange(tempData.Select(Convert));
                pageIndex++;
            } while (tempData.Count >= pageSize);

            return allData;
        }

        private List<RMEncryptionData> GetAll(int pageSize, int pageIndex)
        {
            return ExecuteWithRetry(context =>
            {
                return context.EncryptionData
                    .OrderBy(d => d.Id)
                    .Paging(pageIndex, pageSize)
                    .ToList();
            });
        }

        int IEncryptionDataDao.Update(RMEncryptionDataInfo data)
        {
            return ExecuteWithRetry(context =>
            {
                var sql =
$@"UPDATE [{SecurityUtils.SanitizeSQLSchemaName(context.SchemaName)}].{SecurityUtils.SanitizeSQLSchemaName(TABLE_NAME)} SET 
  ProfileId=@ProfileId, Content=@Content, UpdateTime=@UpdateTime
WHERE Id=@Id";
                var parameters = new SqlParameter[] {
                    new SqlParameter("@Id", System.Data.SqlDbType.Int) { Value = data.Id },
                    new SqlParameter("@ProfileId", data.ProfileId),
                    new SqlParameter("@Content", data.Content),
                    new SqlParameter("@UpdateTime", System.Data.SqlDbType.BigInt) { Value =  DateTime.UtcNow.Ticks }
                };

                return context.Database.ExecuteSqlCommand(sql, parameters);
            });
        }




        private RMEncryptionData Convert(RMEncryptionDataInfo item)
        {
            return new RMEncryptionData()
            {
                Id = item.Id,
                Content = item.Content,
                DataType = item.DataType,
                ProfileId = item.ProfileId,
                UpdateTime = item.UpdateTime
            };
        }

        private RMEncryptionDataInfo Convert(RMEncryptionData item)
        {
            return new RMEncryptionDataInfo()
            {
                Id = item.Id,
                Content = item.Content,
                DataType = item.DataType,
                ProfileId = item.ProfileId,
                UpdateTime = item.UpdateTime
            };
        }
    }
}
