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
using AvePoint.RA.DB.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.DB.Dao
{
    public interface IMediaDataDao: IBaseDao<MediaData>
    {
        /// <summary> Control端负责接收Media发送过来的数据，保存成功返回1，否则返回0 </summary>
        /// <param name="dto"></param>
        public int AcceptLoadBalanceInfo(MediaDataDto dto);

        /// <summary> 根据serviceId、key获取MediaData记录。</summary>
        /// <param name="serviceId"></param>
        /// <param name="key">不能为empty or null</param>
        public Task<List<MediaDataDto>> GetMediaDatasAsync(string key);

        /// <summary> 根据serviceId、key删除MediaData记录。 </summary>
        /// <param name="serviceId"></param>
        /// <param name="key">不能为empty or null</param>
        public void DeleteMediaDatas(string key);

        /// <summary>根据serviceId、key更新MediaData记录，如果不存在MediaData记录，则创建一条MediaData记录。</summary>
        /// <param name="serviceId"></param>
        /// <param name="key">不能为empty or null</param>
        /// <param name="value"></param>
        public Task UpdateOrInsertMediaDataAsync(string key, string value);

        public Task<int> ClearAllAsync();

    }
}
