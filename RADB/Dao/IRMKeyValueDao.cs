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
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.DB.Dao
{
    public interface IRMKeyValueDao: IBaseDao<RMKeyValue>
    {
        Task<bool> UpdateAsync(Dictionary<string, string> entities, IEnumerable<string> willDeleteEntityKeys);
        Task<List<RMKeyValue>> GetAllAsync();
        Task<bool> UpsertAsync(string key, string value);
        Task<bool> SaveOrUpdateAsync(RMKeyValue entity);
        bool Save(RMKeyValue entity);
        RMKeyValue GetValueByKey(string key);
        Task<string> GetValueByKeyAsync(string key);
        bool TryGetBoolValue(string key, out bool value);

        bool AtomicityUpdate(string key, string oldValue, string newValue);

        Task<T> GetValueByKeyAsync<T>(string key);

        Task<T> GetValueByKeyAsync<T>(string key, T defaultValue);
        Task<IEnumerable<RMKeyValue>> LoadByPager(int pageIndex, int pageSize);
        Task<long> MultiGeoInsertKeyValueTableAsync(IEnumerable<RMKeyValue> keyValues);
        Task<long> MultiGeoDeleteAllKeyValueAsync();
    }
}
