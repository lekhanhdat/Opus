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
using System.Collections;
using System.Collections.Concurrent;
using System.Linq;
using AvePoint.RA.CommonUtil;
using ProtoBuf.Meta;
using PB = ProtoBuf;

namespace AvePoint.RA.Common.Utils.ProtoBuf
{
    public static class ProtobufRuntimeHelper
    {
        private static readonly RALogger logger = RALogger.GetInstance(typeof(ProtobufRuntimeHelper));
        private static readonly ConcurrentDictionary<Type, bool> _registeredTypes = new ConcurrentDictionary<Type, bool>();
        private static readonly object _lockObj = new object();

        public static void EnsureTypeRegistered<T>()
        {
            lock (_lockObj)
            {
                logger.Info($"All registered types: {string.Join(", ", _registeredTypes.Keys.Select(t => t.FullName))}");
            }
            RegisterTypeRecursive(typeof(T));
        }

        private static void RegisterTypeRecursive(Type type)
        {
            if (typeof(IEnumerable).IsAssignableFrom(type) && type != typeof(string))
            {
                if (type.IsArray)
                {
                    RegisterTypeRecursive(type.GetElementType());
                    return;
                }
                else if (type.IsGenericType)
                {
                    foreach (var arg in type.GetGenericArguments())
                    {
                        RegisterTypeRecursive(arg);
                    }
                    return;
                }
            }

            if (IsSystemType(type) || _registeredTypes.ContainsKey(type) || Attribute.IsDefined(type, typeof(PB.ProtoContractAttribute)))
            {
                return;
            }

            lock (_lockObj)
            {
                try
                {
                    if (_registeredTypes.ContainsKey(type)) return;
                    _registeredTypes.TryAdd(type, true);
                    logger.Info($"Registering type: {type.FullName}");

                    var metaType = RuntimeTypeModel.Default.Add(type, applyDefaultBehaviour: false);

                    var props = type.GetProperties()
                                    .Where(p => p.CanRead && p.CanWrite)
                                    .OrderBy(p => p.Name) // need to have a consistent order
                                    .ToList();

                    int fieldId = 1;
                    foreach (var prop in props)
                    {
                        try
                        {
                            metaType.Add(fieldId++, prop.Name);
                        }
                        catch (Exception e)
                        {
                            logger.Error($"Failed to add property: {prop.Name} to type: {type.FullName}. Ex: {e}");
                            throw;
                        }

                        RegisterTypeRecursive(prop.PropertyType);
                    }

                    logger.Info($"Finish Registered type: {type.FullName} with {props.Count} properties.");
                }
                catch (Exception e)
                {
                    logger.Error($"Failed to register type: {type.FullName}. Ex: {e}");
                    throw;
                }
            }
        }

        private static bool IsSystemType(Type type)
        {
            var underlyingType = Nullable.GetUnderlyingType(type) ?? type;

            return underlyingType.IsPrimitive ||
                   underlyingType.IsEnum ||
                   //underlyingType == typeof(string) ||
                   //underlyingType == typeof(decimal) ||
                   //underlyingType == typeof(DateTime) ||
                   //underlyingType == typeof(DateTimeOffset) ||
                   //underlyingType == typeof(Guid) ||
                   //underlyingType == typeof(TimeSpan) ||
                   //underlyingType == typeof(byte[]) ||
                   underlyingType.Namespace.StartsWith("System");
        }
    }
}