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
using System;
using System.Collections.Generic;
using System.Linq;
using AvePoint.RA.Contract.DataIngestion;

namespace AvePoint.RA.Service.Services.DataIngestion.MessageQueue
{
    public static class RMDataIngestionAzureQueueHandlerFactory
    {
        private static readonly Lazy<IDictionary<RMDataIngestionType, Type>> ClientMap = new(CreateClientMap);

        public static RMDataIngestionAzureQueueHandler Create(RMDataIngestionType type)
        {
            if (!ClientMap.Value.TryGetValue(type, out var clientType))
            {
                throw new NotSupportedException($"No queue client registered for ingestion type {type}.");
            }

            return (RMDataIngestionAzureQueueHandler)Activator.CreateInstance(clientType);
        }

        private static IDictionary<RMDataIngestionType, Type> CreateClientMap()
        {
            var baseType = typeof(RMDataIngestionAzureQueueHandler);

            return typeof(RMDataIngestionAzureQueueHandlerFactory).Assembly
                .GetTypes()
                .Where(t => !t.IsAbstract && !t.IsInterface && baseType.IsAssignableFrom(t))
                .Select(CreateDescriptor)
                .ToDictionary(d => d.Type, d => d.Implementation);
        }

        private static (RMDataIngestionType Type, Type Implementation) CreateDescriptor(Type implementation)
        {
            var instance = (RMDataIngestionAzureQueueHandler)Activator.CreateInstance(implementation);
            return (instance.IngestionType, implementation);
        }
    }
}
