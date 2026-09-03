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
namespace LS.SPWorkflowProcessor
{

    using System;
    using AvePoint.Wrapper.Common;
    using System.Collections.Generic;
    using AvePoint.GCommon;

    public class AveWorkflowRunningInstanceRecalculationService
    {

        private static AveLogger logger = AveLogger.GetInstance(typeof(AveWorkflowRunningInstanceRecalculationService));

        /// <summary>
        /// siteId,webId,AssociationId,List<AveWorkflowAssociationBasicInfo>
        /// </summary>
        private static Dictionary<Guid, Dictionary<Guid, Dictionary<Guid, AveWorkflowAssociationCacheInfo>>> needUpdateCache;

        private static object privateLock = new object();

        /// <summary>
        /// add association info to cache, and recalculate the running instance count for this association in web post action
        /// </summary>
        /// <param name="association"></param>
        public static void AddAssociationToCache(Guid siteId,Guid webId,Guid listId,Guid associationId,string associationName)
        {
            using (AvePerformanceScope pf = new AvePerformanceScope("WFAssociationProcAPI.RestoreAssociationUnit.AddAssociationToCache"))
            {
                lock (privateLock)
                {
                    Dictionary<Guid, Dictionary<Guid, AveWorkflowAssociationCacheInfo>> siteCache;
                    Dictionary<Guid, AveWorkflowAssociationCacheInfo> webCache;
                    AveWorkflowAssociationCacheInfo basicInfo;
                    if (needUpdateCache == null)
                    {
                        needUpdateCache = new Dictionary<Guid, Dictionary<Guid, Dictionary<Guid, AveWorkflowAssociationCacheInfo>>> { };
                    }
                    if (!needUpdateCache.TryGetValue(siteId, out siteCache))
                    {
                        needUpdateCache.Add(siteId, new Dictionary<Guid, Dictionary<Guid, AveWorkflowAssociationCacheInfo>> { });
                        siteCache = needUpdateCache[siteId];
                    }
                    if (!siteCache.TryGetValue(webId, out webCache))
                    {
                        siteCache.Add(webId, new Dictionary<Guid, AveWorkflowAssociationCacheInfo> { });
                        webCache = siteCache[webId];
                    }
                    if (!webCache.TryGetValue(associationId, out basicInfo))
                    {
                        basicInfo = new AveWorkflowAssociationCacheInfo(siteId, webId, listId, associationId, associationName);
                        webCache.Add(associationId, basicInfo);
                        logger.Debug("Add association to recalculation running instance cache.Info:{0}", basicInfo);
                    }
                }
            }
        }

        /// <summary>
        /// 加到缓存中，web post action再处理，这样可以减少更新次数(一个association一次而不是一个instance一次)
        /// </summary>
        /// <param name="web"></param>
        /// <param name="queryService"></param>
        public static void RecalculateRunningInstanceCount(IAveWeb web, IAveBackupRestoreQueryService queryService)
        {
            using (AvePerformanceScope pf = new AvePerformanceScope("WFAssociationProcAPI.RestoreAssociationUnit.RecalculateRunningInstanceCount"))
            {
                lock (privateLock)
                {
                    if (web != null && queryService != null && web.Site.APIType == AveAPIType.Server)
                    {
                        Guid siteId = web.Site.ID;
                        Guid webId = web.ID;
                        if (needUpdateCache != null)
                        {
                            Dictionary<Guid, Dictionary<Guid, AveWorkflowAssociationCacheInfo>> sitecache;
                            if (needUpdateCache.TryGetValue(siteId, out sitecache))
                            {
                                Dictionary<Guid, AveWorkflowAssociationCacheInfo> webCache;
                                if (sitecache.TryGetValue(webId, out webCache))
                                {
                                    foreach (var info in webCache.Values)
                                    {
                                        RecalculateOneAssociation(info, queryService);
                                    }
                                    webCache.Clear();
                                }
                                sitecache.Remove(webId);
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 我们还原workflow instance的时候会导致Association上的RunningInstances数量不正确，需要用native方式更新一下
        /// </summary>
        private static void RecalculateOneAssociation(AveWorkflowAssociationCacheInfo basicInfo, IAveBackupRestoreQueryService queryService)
        {
            using (AvePerformanceScope pf = new AvePerformanceScope("WFAssociationProcAPI.RestoreAssociationUnit.RecalculateOneAssociation"))
            {
                try
                {
                    queryService.RecalculateRunningInstanceCount(basicInfo.SiteId, basicInfo.WebId, basicInfo.ListId, basicInfo.AssociationId);
                    logger.Debug("Recalculate running instance count finish.Info:{0}", basicInfo);
                }
                catch (Exception e)
                {
                    logger.Warn("An error occurred while recalculate one association running instance count.Info:{0},Error:{1}", basicInfo, e);
                }
            }
        }

        /// <summary>
        /// internal class,only used for cache info
        /// </summary>
        private class AveWorkflowAssociationCacheInfo
        {
            internal string Name;
            /// <summary>
            /// workflow table中的SiteId列值
            /// </summary>
            internal Guid SiteId;
            /// <summary>
            /// workflow table中的WebId列值
            /// </summary>
            internal Guid WebId;
            /// <summary>
            /// workflow table中的ListId列值
            /// </summary>
            internal Guid ListId;
            internal Guid AssociationId;

            internal AveWorkflowAssociationCacheInfo(Guid siteId,Guid webId,Guid listId,Guid associationId,string name)
            {
                Name =name;
                SiteId = siteId;
                WebId = webId;
                ListId = listId;
                AssociationId = associationId;
            }

            public override string ToString()
            {
                return String.Format("[AveWorkflowAssociationBasicInfo][{0}][{1}][{2}][{3}][{4}]", Name, SiteId, WebId, ListId, AssociationId);
            }
        }
    }

    
}
