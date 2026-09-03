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
using AvePoint.RA.SharePoint.Common.CAMLHelper.CAML;
using AvePoint.RA.SharePoint.Common.CAMLHelper.General;
using AvePoint.Wrapper.Common;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.SharePoint.EnforceRetention
{
    public class CAMLManagerUtil
    {
        private static readonly AvePoint.RA.Contract.Services.IRALogger logger = RALogger.GetInstance(typeof(CAMLManagerUtil));
        //query pager 20
        protected const int mQueryConditionMaxCount = 20;
        private static Dictionary<Guid, Dictionary<Guid, int>> wssIdCacheDic = new Dictionary<Guid, Dictionary<Guid, int>>();
        private static int rowLimit = 0;
        public static void Init(IAveSite aveSite) 
        {
            rowLimit = GetMaxItemsPerThrottledOperation(aveSite);
            CacheAllWssids(aveSite);
        }

        public static List<CAMLManager> BuildCAMLMangager(Guid siteId, List<Guid> termIds, string bcsColumnInternalName)
        {
            
            using (var performance = new PerformanceScope("SP.RMEnforceRetentionProcesser.GetTaxonomyHiddenListTerms"))
            {
                List<CAMLManager> cms = new List<CAMLManager>();
                var wssids = GetWssidByTermIds(siteId, termIds);
                if (wssids.Count < mQueryConditionMaxCount)
                {
                    CAMLManager cm = InitCamlQuery(wssids, bcsColumnInternalName);
                    if (cm != null)
                    {
                        cms.Add(cm);
                    }
                }
                else
                {
                    int index = 0;
                    while (wssids.Skip(index).Take(mQueryConditionMaxCount) != null && wssids.Skip(index).Take(mQueryConditionMaxCount).Count() != 0)
                    {
                        var queryIds = wssids.Skip(index).Take(mQueryConditionMaxCount).ToList();
                        index += mQueryConditionMaxCount;
                        if (queryIds.Count != 0)
                        {
                            CAMLManager cm = InitCamlQuery(queryIds, bcsColumnInternalName);
                            if (cm != null)
                            {
                                cms.Add(cm);
                            }
                        }
                    }
                }
                
                return cms;
            }
            
            
           
        }
        public static List<CAMLManager> BuildCAMLMangagerForRetention(Guid siteId, List<Guid> changedTermIds, string needApplyLabel, string bcsColumnInternalName)
        {

            using (var performance = new PerformanceScope("SP.RMEnforceRetentionProcesser.GetTaxonomyHiddenListTerms"))
            {
                
                List<CAMLManager> cms = new List<CAMLManager>();
                var changedTermWssids = GetWssidByTermIds(siteId, changedTermIds);
                if (changedTermWssids.Count < mQueryConditionMaxCount)
                {
                    CAMLManager cm = InitCamlQueryForRetention(changedTermWssids, needApplyLabel, bcsColumnInternalName);
                    if (cm != null)
                    {
                        cms.Add(cm);
                    }
                }
                else
                {
                    int index = 0;
                    while (changedTermWssids.Skip(index).Take(mQueryConditionMaxCount) != null && changedTermWssids.Skip(index).Take(mQueryConditionMaxCount).Count() != 0)
                    {
                        var queryIds = changedTermWssids.Skip(index).Take(mQueryConditionMaxCount).ToList();
                        index += mQueryConditionMaxCount;
                        if (queryIds.Count != 0)
                        {
                            CAMLManager cm = InitCamlQueryForRetention(queryIds, needApplyLabel, bcsColumnInternalName);
                            if (cm != null)
                            {
                                cms.Add(cm);
                            }
                        }
                    }
                }

                return cms;
            }



        }
        private static void CacheAllWssids(IAveSite aveSite) 
        {
            Dictionary<Guid, int> allWssIds = new Dictionary<Guid, int>();
            if (!wssIdCacheDic.TryGetValue(aveSite.ID, out allWssIds))
            {
                allWssIds = GetTaxonomyHiddenListTerms(aveSite);
                wssIdCacheDic.Add(aveSite.ID, allWssIds);
                logger.Debug($"cache term wssids: {string.Join(",", allWssIds)}");
            }

        }
        private static List<int> GetWssidByTermIds(Guid siteId, List<Guid> termIds) 
        {
            Dictionary<Guid, int> allWssIds = new Dictionary<Guid, int>();
            if (wssIdCacheDic.TryGetValue(siteId, out allWssIds))
            {
                return allWssIds.Where(w => termIds.Contains(w.Key)).Select(w => w.Value).ToList();
            }
            return new List<int>();
        }

        protected static CAMLManager InitCamlQuery(List<int> termWssIds, string bcsColumnInternalName)
        {
            CAMLManager cm = new CAMLManager();

            if (termWssIds.Count > 0)
            {
                QueryCondition condition = QueryConditionFactory.GetTaxonomyQueryCondition(bcsColumnInternalName, termWssIds.ToArray(), Types.JoinTypes.Or);
                cm.QueryGroup.AddGroup(new QueryGroup(Types.JoinTypes.And, null, new List<QueryCondition> { condition }));
            }
            logger.Info("End Dealing TermIds , Count {0} , Time {1}", termWssIds.Count, DateTime.Now.ToString("yyyy-dd-M--HH-mm-ss"));

            if (cm.QueryGroup.Groups.Count > 0)
            {

                return cm;
            }
            else
            {
                return null;
            }
        }

        protected static CAMLManager InitCamlQueryForRetention(List<int> changedWssIds, string needApplyLabel, string bcsColumnInternalName)
        {
            CAMLManager cm = new CAMLManager();

            if (changedWssIds.Count > 0 && !string.IsNullOrEmpty(needApplyLabel))
            {
                QueryCondition condition1 = QueryConditionFactory.GetTaxonomyQueryCondition(bcsColumnInternalName, changedWssIds.ToArray(), Types.JoinTypes.And);
                QueryCondition condition2 = new QueryCondition(Types.JoinTypes.And, SPColumnConstants.SP_ComplianceTag, Types.FieldTypes.Lookup, Types.QueryTypes.Neq, needApplyLabel);
                cm.QueryGroup.AddGroup(new QueryGroup(Types.JoinTypes.And, null, new List<QueryCondition> { condition1, condition2 }));
            }
            logger.Info($"End Dealing TermIds , applyLabelWssIds: {changedWssIds.Count} , label:{needApplyLabel} Time {DateTime.Now.ToString("yyyy-dd-M--HH-mm-ss")}");

            if (cm.QueryGroup.Groups.Count > 0)
            {

                return cm;
            }
            else
            {
                return null;
            }
        }

        protected static int GetMaxItemsPerThrottledOperation(IAveSite discoverSite)
        {
            int maxItemsPer = 2000;
            try
            {
                var dataCacheType = discoverSite.GetType().GetProperty("DataCache");
                var dataCacheObj = dataCacheType.GetValue(discoverSite);
                var propertiesCacheProp = dataCacheObj.GetType().GetProperty("PropertiesCache");
                var propertiesCacheObj = propertiesCacheProp.GetValue(dataCacheObj);
                var propertiesDic = (propertiesCacheObj as Dictionary<string, object>);
                object maxItemsPerObj;
                if (propertiesDic.TryGetValue("MaxItemsPerThrottledOperation", out maxItemsPerObj))
                {
                    maxItemsPer = Convert.ToInt32(maxItemsPerObj);
                }
            }
            catch (Exception ex)
            {
                logger.Warn("GetMaxItemsPerThrottledOperation by siteCollection NodeItem faild, Error message:", ex.ToString());
            }
            return maxItemsPer;
        }

        public static Dictionary<Guid, int> GetTaxonomyHiddenListTerms(IAveSite aveSite)
        {
            Dictionary<Guid, int> mWssids = new Dictionary<Guid, int>();
            try
            {
                using (var performance = new PerformanceScope("SP.RMEnforceRetentionProcesser.GetTaxonomyHiddenListTerms"))
                {
                    IAveList taxonomyList = aveSite.RootWeb.Lists.GetByTitle("TaxonomyHiddenList");
                    AveCamlQuery query = new AveCamlQuery();
                    query.ListItemCollectionPosition = new AveItemCollectionPosition();
                    CAMLManager caml = new CAMLManager();
                    caml.ScopeType = Types.ScopeTypes.Recursive;
                    caml.RowLimit = 2000;
                    string queryXml = caml.GetFullCAML(true);
                    IAveListItemCollectionPosition pagerPosition = null;
                    query.ViewXml = queryXml;
                    logger.Info("TaxonomyHiddenList query xml {0}", queryXml);
                    do
                    {
                        IAveListItemCollection termItems = taxonomyList.GetItems(query);
                        pagerPosition = termItems.ListItemCollectionPosition;
                        foreach (var termItem in termItems)
                        {
                            if (termItem[SPColumnConstants.SP_Title] == null)
                            {
                                logger.Warn("Term Title in TaxonomyHiddenList is null.TermGuid:[{0}] TermSetId:[{1}]"
                                    , termItem[SPColumnConstants.ID_FOR_TERM].ToString(), termItem[SPColumnConstants.ID_FOR_TERM]);
                                continue;
                            }
                            if (!mWssids.ContainsKey(new Guid(termItem[SPColumnConstants.ID_FOR_TERM].ToString())))
                            {
                                mWssids.Add(new Guid(termItem[SPColumnConstants.ID_FOR_TERM].ToString()), int.Parse(termItem[SPColumnConstants.SP_ID].ToString()));
                            }
                        }

                    } while (pagerPosition != null);
                    
                }

            }
            catch (Exception e1)
            {
                logger.Error("get wwsid for term error: {0}", e1.ToString());
                throw e1;
            }
            return mWssids;
        }
    }
}
