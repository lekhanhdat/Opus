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
//using AvePoint.Adonis.Records.Object.ActionOnly;
//using AvePoint.GCommon.Contract.Tree.Object;
//using AvePoint.RA.DB.Dao;
//using AvePoint.RA.DB.Dao.Impl;
//using AvePoint.RA.DB.Model;
////using AvePoint.RA.SharePoint.SPObjects.Collection;
//using AvePoint.Wrapper.Common;
//using AvePoint.Wrapper.Discovery;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading;
//using System.Threading.Tasks;

//namespace AvePoint.RA.SharePoint.ActionOnly.SPActionOnly
//{
//    public class SPActionProcessorByExp : BaseSPActionProcessor
//    {
//        public IExplorerDao ExplorerDao = new ExplorerDao();
//        public SPActionProcessorByExp(RecordsActionOnlyJobMessage message) : base(message)
//        {

//        }
//        public SPActionProcessorByExp(RecordsActionOnlyJobMessage message, SPTreeNodeDto current) : base(message, current)
//        {

//        }
//        public override void ProcessSiteCollection(SPTreeNodeDto site)
//        {
//            //Get Items By Explore DB (query item by rule id && item term.)
//            IAveSite aveSite = null;
//            using (new RA.Common.PerformanceScope(string.Format("Process Site Collection")))
//            {
//                aveSite = ObjectModelFactory.CreateSite(site.FullPath);
//                List<RMManagedRecord> itemsFromExp = new List<RMManagedRecord>();
//                int startIndex = 0;
//                int count = 2000;
//                using (new RA.Common.PerformanceScope(string.Format("Explorer Do Action")))
//                {
//                    var RuleIds = JobMessage.AllRecordsRule.Select(t => new Guid(t.Id)).ToList();
//                    do
//                    {
//                        using (new RA.Common.PerformanceScope(string.Format("query{0}", startIndex)))
//                        {
//                            itemsFromExp = ExplorerDao.GetRecordsByRule(aveSite.ID, RuleIds, startIndex, count);
//                            if (itemsFromExp.Count > 0)
//                            {
//                                startIndex = itemsFromExp.Last().Id;
//                            }
//                            else
//                            {
//                                break;
//                            }
//                        }
//                        var webRecordDic = itemsFromExp.GroupBy(r => r.WebId).ToDictionary(r => r.Key);
//                        ProgressService.IncreaseBase(itemsFromExp.Count);
//                        foreach (var webId in webRecordDic.Keys)
//                        {
//                            var web = aveSite.OpenWeb(webId);
//                            if (IsInExcludeNodeList(web.Url))
//                            {
//                                continue;
//                            }
//                            var records = webRecordDic[webId].GroupBy(t => t.ListId).ToDictionary(r => r.Key);
//                            foreach (var listId in records.Keys)
//                            {
//                                var list = web.GetList(listId);
//                                var taxField = list.Fields.GetField(BCSColumnName);
//                                if (list.BaseType != AveBaseType.DocumentLibrary)
//                                {
//                                    logger.Info($"Skip all other list type {list.BaseType} :{list.Title}");
//                                    continue;
//                                }
//                                if (IsInExcludeNodeList(list.RootFolder.ServerRelativeUrl))
//                                {
//                                    continue;
//                                }
//                                #region handleItem logic
//                                if (ActionUseMultiThreads && records[listId].Count() > ThreadCount)
//                                {
//                                    logger.Info($"Items count {records[listId].Count()} {list.Title} use multithread");
//                                    var cts = new CancellationTokenSource();
//                                    List<IAveListItem> items = new List<IAveListItem>();
//                                    foreach (var record in records[listId])
//                                    {
//                                        try
//                                        {
//                                            var item = list.GetItemById(record.ItemRowId);
//                                            items.Add(item);
//                                        }
//                                        catch (Exception e)
//                                        {
//                                            logger.Info($"Init item failed {record.FullPath} : {e.ToString()}");
//                                        }
//                                    }
//                                    RunMultiThreadsProcessItem(items, ThreadCount, cts, taxField.InternalName);
//                                }
//                                else
//                                {
//                                    foreach (var record in records[listId])
//                                    {
//                                        try
//                                        {
//                                            var item = list.GetItemById(record.ItemRowId);
//                                            ProcessItem(item, taxField.InternalName);
//                                        }
//                                        catch (Exception e)
//                                        {
//                                            logger.Info($"Init item failed {record.FullPath} : {e.ToString()}");
//                                        }
//                                    }
//                                }
//                                #endregion
//                            }
//                        }
//                    }//consider memory issue.
//                    while (itemsFromExp.Count > 0);
//                    #region old method
//                    //Temp solution
//                    //foreach (var item in itemsFromExpTemp)
//                    //{
//                    //    long dueTicks = 0;
//                    //    if (Int64.TryParse(item.DisposalDueDate, out dueTicks))
//                    //    {
//                    //        if (DateTime.UtcNow.Ticks > dueTicks)
//                    //        {
//                    //            itemsFromExp.Add(item);
//                    //        }
//                    //        else
//                    //        {
//                    //            logger.Info($"Due date time {item.LeafName} : {dueTicks}");
//                    //        }
//                    //    }
//                    //    else
//                    //    {
//                    //        itemsFromExp.Add(item);
//                    //    }
//                    //}
//                    #endregion
//                }

//            }
//            //Run Inc Explore Sync.To Update Exp Database.??
//            DisposeSPObj(aveSite);
//            base.ProcessSiteCollection(site);
//        }
//        public override void ProcessSite(IAveWeb site)
//        {
//            ProgressService.IncreaseBase(1);
//            using (new RA.Common.PerformanceScope(string.Format("Process Site")))
//            {
//                List<RMManagedRecord> itemsFromExp = new List<RMManagedRecord>();
//                int startIndex = 0;
//                int count = 2000;
//                using (new RA.Common.PerformanceScope(string.Format("Explorer Do Action by site")))
//                {
//                    var RuleIds = JobMessage.AllRecordsRule.Select(t => new Guid(t.Id)).ToList();
//                    do
//                    {
//                        itemsFromExp = ExplorerDao.GetRecordsByRuleIdWebId(site.Site.ID, site.ID, RuleIds, startIndex, count);//consider memory issue.
//                        if (itemsFromExp.Count > 0)
//                        {
//                            startIndex = itemsFromExp.Last().Id;
//                        }
//                        else
//                        {
//                            break;
//                        }
//                        ProgressService.IncreaseBase(itemsFromExp.Count);
//                        var listRecordsDic = itemsFromExp.GroupBy(t => t.ListId).ToDictionary(r => r.Key);
//                        foreach (var listId in listRecordsDic.Keys)
//                        {
//                            var list = site.GetList(listId);
//                            if (list.BaseType != AveBaseType.DocumentLibrary)
//                            {
//                                logger.Info($"Skip all other list type {list.BaseType} :{list.Title}");
//                                continue;
//                            }
//                            if (IsInExcludeNodeList(list.RootFolder.ServerRelativeUrl))
//                            {
//                                continue;
//                            }
//                            var taxField = list.Fields.GetField(BCSColumnName);
//                            if (ActionUseMultiThreads && listRecordsDic.Count() > ThreadCount)
//                            {
//                                logger.Info($"Items count {listRecordsDic[listId].Count()} {list.Title} use multithread");
//                                var cts = new CancellationTokenSource();
//                                List<IAveListItem> items = new List<IAveListItem>();
//                                foreach (var record in listRecordsDic[listId])
//                                {
//                                    try
//                                    {
//                                        var item = list.GetItemById(record.ItemRowId);
//                                        items.Add(item);
//                                    }
//                                    catch (Exception e)
//                                    {
//                                        logger.Info($"Init item failed {record.FullPath} : {e.ToString()}");
//                                    }
//                                }
//                                RunMultiThreadsProcessItem(items, ThreadCount, cts, taxField.InternalName);
//                            }
//                            else
//                            {
//                                foreach (var record in listRecordsDic[listId])
//                                {
//                                    try
//                                    {
//                                        var item = list.GetItemById(record.ItemRowId);
//                                        ProcessItem(item, taxField.InternalName);
//                                    }
//                                    catch (Exception e)
//                                    {
//                                        logger.Info($"Init item failed {record.FullPath} : {e.ToString()}");
//                                    }
//                                }
//                            }
//                        }
//                    }
//                    while (itemsFromExp.Count > 0);
//                }

//            }
//            DisposeSPObj(site);
//            base.ProcessSite(site);
//        }


//        public override void ProcessList(IAveList list)
//        {
//            ProgressService.IncreaseBase(1);
//            using (new RA.Common.PerformanceScope(string.Format("Process List")))
//            {
//                List<RMManagedRecord> itemsFromExp = new List<RMManagedRecord>();
//                int startIndex = 0;
//                int count = 2000;
//                using (new RA.Common.PerformanceScope(string.Format("Query From Explorer by List")))
//                {
//                    var RuleIds = JobMessage.AllRecordsRule.Select(t => new Guid(t.Id)).ToList();
//                    do
//                    {
//                        itemsFromExp = ExplorerDao.GetRecordsByRuleIdListId(list.ParentWeb.Site.ID, list.ID, RuleIds, startIndex, count);//consider memory issue.
//                        if (itemsFromExp.Count > 0)
//                        {
//                            startIndex = itemsFromExp.Last().Id;
//                        }
//                        else
//                        {
//                            break;
//                        }
//                        ProgressService.IncreaseBase(itemsFromExp.Count);

//                        if (list.BaseType != AveBaseType.DocumentLibrary)
//                        {
//                            logger.Info($"Skip all other list type {list.BaseType} :{list.Title}");
//                            return;
//                        }
//                        var taxField = list.Fields.GetField(BCSColumnName);
//                        if (taxField == null)
//                        {
//                            logger.Info($"Current list not config bcs column {list.RootFolder.Url}");
//                            return;
//                        }
//                        #region do action
//                        if (ActionUseMultiThreads && itemsFromExp.Count > ThreadCount)
//                        {
//                            logger.Info($"Items count {itemsFromExp.Count} {list.Title} use multithread");
//                            var cts = new CancellationTokenSource();
//                            List<IAveListItem> items = new List<IAveListItem>();
//                            foreach (var record in itemsFromExp)
//                            {
//                                try
//                                {
//                                    var item = list.GetItemById(record.ItemRowId);
//                                    items.Add(item);
//                                }
//                                catch (Exception e)
//                                {
//                                    logger.Info($"Init item failed {record.FullPath} : {e.ToString()}");
//                                }
//                            }
//                            RunMultiThreadsProcessItem(items, ThreadCount, cts, taxField.InternalName);
//                        }
//                        else
//                        {
//                            foreach (var record in itemsFromExp)
//                            {
//                                try
//                                {
//                                    var item = list.GetItemById(record.ItemRowId);

//                                    ProcessItem(item, taxField.InternalName);
//                                }
//                                catch (Exception e)
//                                {
//                                    logger.Info($"Init item failed {record.FullPath} : {e.ToString()}");
//                                }
//                            }
//                        }
//                        #endregion
//                    }
//                    while (itemsFromExp.Count > 0);
//                }

//            }
//            DisposeSPObj(list);
//            base.ProcessList(list);
//        }


//        public override void ProcessSite(IAveDiscoverWeb site)
//        {
//            base.ProcessSite(site);
//        }
//        public override void ProcessList(IAveDiscoverList list)
//        {
//            base.ProcessList(list);
//        }
//        public override void ProcessFolder(IAveDiscoverFolder folder)
//        {
//            base.ProcessFolder(folder);
//        }
//        public override void ProcessItem(IAveListItem item, string BCSColumnInternalName)
//        {
//            base.ProcessItem(item, BCSColumnInternalName);
//        }
//        public override bool Run()
//        {
//            bool result = false;
//            try
//            {
//                //Multithread is need in site collection level???
//                logger.Info($"Current Node Level {CurrentNode.Level.ToString()} : URL {CurrentNode.Url}");
//                switch (CurrentNode.Level)
//                {
//                    case NodeLevel.SiteCollection:
//                        ProcessSiteCollection(CurrentSiteColTreeNode);
//                        break;
//                    case NodeLevel.Site:
//                        var aveSite = ObjectModelFactory.CreateSite(CurrentSiteColTreeNode.FullPath);
//                        var aveWeb = aveSite.OpenWeb(new Guid(CurrentNode.SPObjectId));
//                        ProcessSite(aveWeb);
//                        break;
//                    case NodeLevel.List://No Need discover obj.
//                        var aveSite1 = ObjectModelFactory.CreateSite(CurrentSiteColTreeNode.FullPath);
//                        var web = aveSite1.OpenWeb(new Guid(CurrentNode.Parent.Parent.SPObjectId));
//                        var list = web.GetList(new Guid(CurrentNode.SPObjectId));
//                        ProcessList(list);
//                        break;
//                }
//            }
//            finally
//            {
//                result = base.Run();
//            }
//            return result;
//        }
//    }
//}
