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
using System.Text;

namespace AvePoint.Wrapper.Common
{
    [Serializable]
    public class WorkflowSiteCollectionCache
    {
        private static object locker = new object();
        public Guid SiteId { get; set; }
        public Dictionary<Guid, Dictionary<Guid, WorkflowStartOptionCache>> Cache { get; set; }
        public WorkflowSiteCollectionCache()
        {
            Cache = new Dictionary<Guid, Dictionary<Guid, WorkflowStartOptionCache>>();
        }
        public void AddCache(Guid webId, Guid listId, WorkflowStartOptionCache cacheData)
        {
            lock (locker)
            {
                var webCache = EnsureWebCache(webId);
                if (!webCache.ContainsKey(listId))
                {
                    webCache.Add(listId, cacheData);
                }
                else
                {
                    MergeCache(cacheData, webCache[listId]);
                }
            }
        }

        public bool TryGetListCache(Guid webId, Guid listId, out WorkflowStartOptionCache cache)
        {
            cache = null;
            bool exist = false;
            lock (locker)
            {
                Dictionary<Guid, WorkflowStartOptionCache> webCache;
                if (Cache.TryGetValue(webId, out webCache))
                {
                    if (webCache.TryGetValue(listId, out cache))
                    {
                        webCache.Remove(listId);
                        exist = true;
                    }
                }
                return exist;
            }
        }

        private void MergeCache(WorkflowStartOptionCache source, WorkflowStartOptionCache dest)
        {
            if (source == null || dest == null)
            {
                return;
            }
            foreach (var item in source.SP2010ModeWorkflowAutoStartCache)
            {
                var dest10Mode = dest.SP2010ModeWorkflowAutoStartCache;
                if (!dest10Mode.ContainsKey(item.Key))
                {
                    dest10Mode.Add(item.Key, new List<WorkflowStartOption>());
                }
                if (item.Value.Count > 0)
                {
                    foreach (var singleCache in item.Value)
                    {
                        dest10Mode[item.Key].Add(singleCache);
                    }
                }
            }

            foreach (var item in source.SP2013ModeWorkflowAutoStartCache)
            {
                var dest13Mode = dest.SP2013ModeWorkflowAutoStartCache;
                if (!dest13Mode.ContainsKey(item.Key))
                {
                    dest13Mode.Add(item.Key, new List<WorkflowStartOption>());
                }
                if (item.Value.Count > 0)
                {
                    foreach (var singleCache in item.Value)
                    {
                        dest13Mode[item.Key].Add(singleCache);
                    }
                }
            }
        }

        private Dictionary<Guid, WorkflowStartOptionCache> EnsureWebCache(Guid webId)
        {
            if (!Cache.ContainsKey(webId))
            {
                Cache.Add(webId, new Dictionary<Guid, WorkflowStartOptionCache>());
            }
            return Cache[webId];
        }

    }

    [Serializable]
    public class WorkflowStartOptionCache : IDisposable
    {
        public const string ListWorkflow = "ListWorkflow";
        public WorkflowStartOptionCache()
        {
            SP2010ModeWorkflowAutoStartCache = new Dictionary<string, List<WorkflowStartOption>>();
            SP2013ModeWorkflowAutoStartCache = new Dictionary<string, List<WorkflowStartOption>>();
            //SP2013ModeWorkflowAutoStartCache = new List<WorkflowStartOption>();
        }
        public Dictionary<string, List<WorkflowStartOption>> SP2010ModeWorkflowAutoStartCache { get; set; }

        public Dictionary<string, List<WorkflowStartOption>> SP2013ModeWorkflowAutoStartCache { get; set; }
        //public List<WorkflowStartOption> SP2013ModeWorkflowAutoStartCache { get; set; }

        public bool HasData()
        {
            bool hasData = false;
            if (SP2010ModeWorkflowAutoStartCache != null && SP2010ModeWorkflowAutoStartCache.Count > 0)
            {
                foreach (var item in SP2010ModeWorkflowAutoStartCache.Values)
                {
                    if (item.Count > 0)
                    {
                        hasData = true;
                        break;
                    }
                }
            }
            if (!hasData)
            {
                if (SP2013ModeWorkflowAutoStartCache != null && SP2013ModeWorkflowAutoStartCache.Count > 0)
                {
                    foreach (var item in SP2013ModeWorkflowAutoStartCache.Values)
                    {
                        if (item.Count > 0)
                        {
                            hasData = true;
                            break;
                        }
                    }
                }
            }
            return hasData;
        }

        public void Dispose()
        {
            if (SP2010ModeWorkflowAutoStartCache != null)
            {
                SP2010ModeWorkflowAutoStartCache.Clear();
                SP2010ModeWorkflowAutoStartCache = null;
            }
            if (SP2013ModeWorkflowAutoStartCache != null)
            {
                SP2013ModeWorkflowAutoStartCache.Clear();
                SP2013ModeWorkflowAutoStartCache = null;
            }
        }
    }

    [Serializable]
    public struct WorkflowStartOption
    {
        public Guid DefinitionId;
        public bool ItemAdded;
        public bool ItemUpdated;
    }

}

