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
using AvePoint.RA.Cache.Services;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.RACommonUtility.Email.Model;
using AvePoint.RA.RACommonUtility.Email.Sender.Middleware;
using AvePoint.RA.RedisCache;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.RACommonUtility.Email.Sender.Storage
{
    public class RMEmailRedisStorage : IRMEmailStorage
    {
        private readonly IRedisCacheProvider _redis = RedisCacheService.CacheProvider;

        private readonly string _prefix;

        private readonly IRMEmailStorageMiddleware _middleware;

        private readonly HashSet<Guid> _addedTemplateIds = new();

        public RMEmailRedisStorage(string prefix) :
            this(prefix, new RMEmailStorageDefaultMiddleware())
        { }

        public RMEmailRedisStorage(string prefix, IRMEmailStorageMiddleware middleware)
        {
            _prefix = prefix;
            _middleware = middleware;
        }

        public void Add(Guid templateId, RMEmailTemplateParameters parameters)
        {
            var key = GetRedisTemplateKey(templateId);

            if (!_addedTemplateIds.Contains(templateId))
            {
                var tenantKey = GetRedisTenantKey();
                _redis.ListRightPush(tenantKey, key);
                _addedTemplateIds.Add(templateId);
            }

            if (_middleware.NeedAdded(templateId, parameters))
            {
                var value = _middleware.Convert(templateId, parameters);
                _redis.ListRightPush(key, value);
            }
        }

        public void AddGControlTemplate(Guid templateId, RMEmailTemplateParameters parameter)
        {
            var key = GetRedisTemplateKey(templateId);

            if (!_addedTemplateIds.Contains(templateId))
            {
                var tenantKey = GetRedisTenantKeyForGControl();
                _redis.ListRightPush(tenantKey, key);
                _addedTemplateIds.Add(templateId);
            }

            if (_middleware.NeedAdded(templateId, parameter))
            {
                var value = _middleware.Convert(templateId, parameter);
                _redis.ListRightPush(key, value);
            }
        }

        public void AddRange(Guid templateId, IEnumerable<RMEmailTemplateParameters> parameters)
        {
            var key = GetRedisTemplateKey(templateId);

            if (!_addedTemplateIds.Contains(templateId))
            {
                var tenantKey = GetRedisTenantKey();
                _redis.ListRightPush(tenantKey, key);
                _addedTemplateIds.Add(templateId);
            }

            var needAddedParameters = parameters.Where(item => _middleware.NeedAdded(templateId, item)).ToList();
            if (needAddedParameters.Any())
            {
                var valueList = needAddedParameters.ConvertAll(item => _middleware.Convert(templateId, item)).ConvertAll(value => new RedisValue(value));
                _redis.ListRightPush(key, valueList);
            }
        }

        public void AddGControlRange(Guid templateId, IEnumerable<RMEmailTemplateParameters> parameters)
        {
            throw new NotImplementedException();
        }

        public void Empty()
        {
            var tenantKey = GetRedisTenantKey();
            var templateIds = _redis.ListRange(tenantKey);

            var keyList = templateIds.Where(item => item.HasValue).ConvertAll(templateId => templateId.ToString());
            keyList.ForEach(key => _redis.KeyDel(key));

            _redis.KeyDel(tenantKey);
        }

        public IEnumerable<RMEmailTemplateParameters> GetParameters(Guid templateId)
        {
            var key = GetRedisTemplateKey(templateId);

            var values = _redis.ListRange(key);

            var parameters = values.Where(item => item.HasValue).ConvertAll(item => _middleware.ConvertRedis(templateId, item.ToString()));

            return parameters.ToHashSet();

        }

        public IEnumerable<Guid> GetTemplates()
        {
            var tenantKey = GetRedisTenantKey();

            var values = _redis.ListRange(tenantKey);

            var templateIds = values.Where(item => item.HasValue).ConvertAll(item =>
            {
                var templateId = item.ToString().Split("=AVE=").Last();
                return new Guid(templateId);
            });

            return templateIds.ToHashSet();
        }
        
        public IEnumerable<Guid> GetGControlTemplates()
        {
            var tenantKey = GetRedisTenantKeyForGControl();

            var values = _redis.ListRange(tenantKey);

            var templateIds = values.Where(item => item.HasValue).ConvertAll(item =>
            {
                var templateId = item.ToString().Split("=AVE=").Last();
                return new Guid(templateId);
            });

            return templateIds.ToHashSet();
        }

        public void Remove(Guid templateId)
        {
            var key = GetRedisTemplateKey(templateId);
            _redis.KeyDel(key);
        }

        private string GetRedisTemplateKey(Guid templateId) => $"{TenantLocalValue.LogonGroupId}=AVE={_prefix}=AVE={templateId}";

        private string GetRedisTenantKey() => $"{TenantLocalValue.LogonGroupId}=AVE={_prefix}";
        
        private string GetRedisTenantKeyForGControl() => $"GCONTROL_{TenantLocalValue.LogonGroupId}=AVE={_prefix}";

    }
}
