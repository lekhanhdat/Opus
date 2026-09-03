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
using AvePoint.GCommon;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AvePoint.Hybrid.AgentService.Initiator
{
    public sealed class InitiatorManager
    {
        private static readonly AveLogger _logger = AveLogger.GetInstance(typeof(InitiatorManager));
        private static readonly object _lock = new object();
        private static Dictionary<string, BaseInitiator> _initiators;

        public static void StartInitiators(IEnumerable<string> specifyInitiators = null)
        {
            EnsureLoaded();

            IEnumerable<BaseInitiator> toStart;

            if (specifyInitiators == null)
            {
                toStart = _initiators.Values;
            }
            else
            {
                var list = new List<BaseInitiator>();
                foreach (var name in specifyInitiators)
                {
                    BaseInitiator initiator;
                    if (_initiators.TryGetValue(name, out initiator))
                    {
                        list.Add(initiator);
                    }
                }
                toStart = list;
            }

            foreach (var initiator in toStart)
            {
                try
                {
                    initiator.Start();
                    _logger.Info($"Finished to start the initiator: [{initiator.Name}].");
                }
                catch (Exception ex)
                {
                    _logger.Error("Initiator [{0}] failed to start. Error: {1}", initiator.Name, ex);
                }
            }
        }

        private static void EnsureLoaded()
        {
            if (_initiators != null) return;
            lock (_lock)
            {
                if (_initiators == null)
                {
                    _initiators = LoadAllInitiators();
                }
            }
        }

        private static Dictionary<string, BaseInitiator> LoadAllInitiators()
        {
            var result = new Dictionary<string, BaseInitiator>(StringComparer.OrdinalIgnoreCase);
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                foreach (var type in assembly.GetTypes())
                {
                    if (type == null || type.IsAbstract || type.IsInterface || !typeof(BaseInitiator).IsAssignableFrom(type))
                        continue;
                    var instance = (BaseInitiator)Activator.CreateInstance(type);
                    if (!result.ContainsKey(instance.Name))
                    {
                        result.Add(instance.Name, instance);
                    }
                }
            }
            _logger.Info($"Loaded initiators: {result.Values.Select(i => i.Name)}.");
            return result;
        }
    }
}
