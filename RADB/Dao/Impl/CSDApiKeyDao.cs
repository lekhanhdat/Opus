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
using System.Data.Entity;
using System.Linq;

namespace AvePoint.RA.DB.Dao.Impl
{
    public class CSDApiKeyDao : BaseDao<CSDApiKey>, ICSDApiKeyDao
    {
        public CSDApiKey GetApiKey(int id)
        {
            using (var context = GetNewContext())
            {
                return context.CSDApiKey.FirstOrDefault(k => k.Id == id);
            }
        }

        public List<CSDApiKey> GetApiKeys(IEnumerable<int> ids)
        {
            using (var context = GetNewContext())
            {
                return context.CSDApiKey.Where(k => ids.Contains(k.Id)).ToList();
            }
        }

        public List<CSDApiKey> GetApiKeys(int pageIndex, int pageSize, out int totalCount)
        {
            if (pageIndex <= 0)
            {
                pageIndex = 1;
            }
            using (var context = GetNewContext())
            {
                var allKeys = context.CSDApiKey.AsQueryable()
                    .Where(k => !k.IsRemoved);
                totalCount = allKeys.Count();

                return allKeys
                    .OrderByDescending(k => k.Id)
                    .Skip(pageSize * (pageIndex - 1))
                    .Take(pageSize)
                    .ToList();
            }
        }

        public bool AddApiKey(string name, string encryptedValue, string valuePrefix, long expiredTime, string operatorLoginName)
        {
            using (var context = GetNewContext())
            {
                var newKey = context.CSDApiKey.Add(new CSDApiKey()
                {
                    Name = name.Trim(),
                    Value = encryptedValue,
                    ValuePrefix = valuePrefix,
                    OperatorLoginName = operatorLoginName.Trim(),
                    Expired = expiredTime,
                    Created = DateTime.UtcNow.Ticks,
                    Modified = DateTime.UtcNow.Ticks,
                    IsRemoved = false
                });

                return context.SaveChanges() > 0;
            }
        }

        public bool EditApiKey(int id, string name, long expiredTime, string operatorLoginName)
        {
            using (var context = GetNewContext())
            {
                var existsKey = context.CSDApiKey.FirstOrDefault(k => k.Id == id);
                if (existsKey != null)
                {
                    existsKey.Name = name.Trim();
                    existsKey.OperatorLoginName = operatorLoginName.Trim();
                    existsKey.Expired = expiredTime;
                    existsKey.Modified = DateTime.UtcNow.Ticks;
                }

                return context.SaveChanges() > 0;
            }
        }

        public bool RemoveApiKeys(IEnumerable<int> ids)
        {
            using (var context = GetNewContext())
            {
                var existsKeys = context.CSDApiKey.Where(k => ids.Contains(k.Id)).ToList();
                if (existsKeys.Count > 0)
                {
                    foreach (var keyItem in existsKeys)
                    {
                        keyItem.IsRemoved = true;
                        keyItem.Modified = DateTime.UtcNow.Ticks;
                    }
                    return context.SaveChanges() > 0;
                }

                return false;
            }
        }

        public bool ExistsKeyName(int id, string name)
        {
            return Exist(k => !k.IsRemoved && k.Id != id && k.Name == name);
        }
    }
}
