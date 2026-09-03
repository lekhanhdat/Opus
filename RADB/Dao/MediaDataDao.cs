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
using AvePoint.GCommon.Contract.Media.Object;
using AvePoint.GCommon.Contract.Server;
using AvePoint.GCommon.Utility;
using AvePoint.RA.DB.Dao.Impl;
using AvePoint.RA.DB.Model;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.DB.Dao
{
    public class MediaDataDao:BaseDao<MediaData>, IMediaDataDao
    {
        public int AcceptLoadBalanceInfo(MediaDataDto dto)
        {
            int record = 0;
            if (dto != null)
            {
                if (string.IsNullOrEmpty(dto.Id))
                {
                    dto.Id = Guid.NewGuid().ToString();
                }
                base.Create(ConvertToMediaDate(dto));
            }
            record = 1;
            return record;
        }

        public async Task<List<MediaDataDto>> GetMediaDatasAsync(string key)
        {
            string queryText = "select m.Id,m.[Key],m.Value from {0}.MediaData as m where m.[Key] = @key";
            List<MediaData> mds= await base.FindListAsync(k=>k.Key==key);
            List<MediaDataDto> result = new List<MediaDataDto>();
            foreach (var temp in mds)
            {
                result.Add(ConvertToMediaDataDto(temp));
            }
            return result;
        }

       /* private MediaDataDto AssambleMediaDataDto(IDataRecord reader)
        {
            return new MediaDataDto()
            {
                Id = reader[0].ToString(),
                Key = reader[1].ToString(),
                Value = reader[2].ToString(),
            };
        }*/

        public void DeleteMediaDatas(string key)
        {
            var deleteInfo = base.Find(k=>k.Key==key);
            if(deleteInfo!=null)
            {
                base.Delete(deleteInfo);
            }
        }

        public async Task<int> ClearAllAsync()
        {
            string sql = $"DELETE FROM {SecurityUtils.SanitizeSQLSchemaName(GetTenantSchemaName())}.MediaDatas";
            using (var ctx = GetNewContext())
            {
                return await ctx.Database.ExecuteSqlCommandAsync(sql);
            }
        }

        public async Task UpdateOrInsertMediaDataAsync(string key, string value)
        {
            List<MediaDataDto> existedDatas = await GetMediaDatasAsync(key);
            if (null != existedDatas && existedDatas.Count > 0)
            {
                await existedDatas.ForEachAsync(item =>
                {
                    item.Value = value;
                    return UpdateMediaDataAsync(item);
                });
            }
            else
            {
                MediaDataDto dto = new MediaDataDto { Id = Guid.NewGuid().ToString(), Key = key, Value = value};
                AcceptLoadBalanceInfo(dto);
            }
        }

        private Task UpdateMediaDataAsync(MediaDataDto dto)
        {
            return base.UpdateAsync(ConvertToMediaDate(dto));
        }
        private MediaDataDto ConvertToMediaDataDto(MediaData dbDto)
        {
            MediaDataDto dto = new MediaDataDto();
            dto.Id = dbDto.Id;
            dto.Key = dbDto.Key;
            dto.Value = dbDto.Value.ToString();
            return dto;
        }
        private MediaData ConvertToMediaDate(MediaDataDto dto)
        {
            MediaData dbDto = new MediaData();
            dbDto.Id = dto.Id;
            dbDto.Key = dto.Key;
            dbDto.Value = Convert.ToInt64(dto.Value);
            return dbDto;
        }
    }
}
