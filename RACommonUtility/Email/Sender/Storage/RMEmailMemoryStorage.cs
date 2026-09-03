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
using AvePoint.RA.Common;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.AAD;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Dao.Impl;
using AvePoint.RA.RACommonUtility.Email.Model;
using AvePoint.RA.RACommonUtility.Email.Sender.Middleware;
using Microsoft.Azure.Cosmos.Linq;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.RACommonUtility.Email.Sender.Storage
{
    public class RMEmailMemoryStorage : IRMEmailStorage
    {
        //private RALogger logger = RALogger.GetInstance(typeof(RMEmailMemoryStorage));

        private readonly ConcurrentDictionary<Guid, HashSet<RMEmailTemplateParameters>> _templateMappings
            = new();
        
        private readonly ConcurrentDictionary<Guid, HashSet<RMEmailTemplateParameters>> _gControlTemplateMappings
            = new();

        private readonly IRMEmailStorageMiddleware _middleware;

        public RMEmailMemoryStorage(IRMEmailStorageMiddleware middleware)
        {
            _middleware = middleware;
        }

        public void Add(Guid templateId, RMEmailTemplateParameters parameters)
        {
            if (!_templateMappings.TryGetValue(templateId, out var value))
            {
                value = new();
                _ = _templateMappings.TryAdd(templateId, value);
            }

            value.Add(parameters);
        }

        public void AddGControlTemplate(Guid templateId, RMEmailTemplateParameters parameter)
        {
            if (!_gControlTemplateMappings.TryGetValue(templateId, out var value))
            {
                value = new();
                _ = _gControlTemplateMappings.TryAdd(templateId, value);
            }

            value.Add(parameter);
        }

        public void AddRange(Guid templateId, IEnumerable<RMEmailTemplateParameters> parameters)
        {
            if (!_templateMappings.TryGetValue(templateId, out var value))
            {
                value = new();
                _ = _templateMappings.TryAdd(templateId, value);
            }

            parameters.ForEach(item => value.Add(item));
        }

        public void AddGControlRange(Guid templateId, IEnumerable<RMEmailTemplateParameters> parameters)
        {
            if (!_gControlTemplateMappings.TryGetValue(templateId, out var value))
            {
                value = new();
                _ = _gControlTemplateMappings.TryAdd(templateId, value);
            }

            parameters.ForEach(item => value.Add(item));
        }

        public void Empty()
        {
            _templateMappings.Clear();
            _gControlTemplateMappings.Clear();
        }

        public IEnumerable<RMEmailTemplateParameters> GetParameters(Guid templateId)
        {
            if (_templateMappings.TryGetValue(templateId, out var value))
            {
                var RMEmailTemplateValue = value.ConvertAll(item => _middleware.ConvertMemory(templateId, item));
                return RMEmailTemplateValue;
            }
            
            if (_gControlTemplateMappings.TryGetValue(templateId, out var googleEmailValue))
            {
                var templateValue = googleEmailValue.ConvertAll(item => _middleware.ConvertMemory(templateId, item));
                return templateValue;
            }

            return new HashSet<RMEmailTemplateParameters>();
        }

        public IEnumerable<Guid> GetTemplates()
        {
            return _templateMappings.Keys;
        }
        
        public IEnumerable<Guid> GetGControlTemplates()
        {
            return _gControlTemplateMappings.Keys;
        }

        public void Remove(Guid templateId)
        {
            _templateMappings.TryRemove(templateId, out _);
        }
    }
}
