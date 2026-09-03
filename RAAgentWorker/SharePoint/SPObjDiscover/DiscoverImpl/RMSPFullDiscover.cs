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
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Services;
using AvePoint.RA.SharePoint.Common;
using AvePoint.Wrapper.Common;
using AvePoint.Wrapper.Discovery;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AvePoint.RA.SharePoint.SPObjDiscover.DiscoverImpl
{
    //TODO 多线程Discover
    public class RMSPFullDiscover : RMSPDiscoverBase, ISPDiscover
    {
        private static readonly AveLogger logger = AveLogger.GetInstance(typeof(RMSPFullDiscover));
        public RMSPDiscoverHelper mdiscoverHelper;

        public RMSPFullDiscover(RMSPDiscoverHelper discoverHelper)
        {
            this.mdiscoverHelper = discoverHelper;
        }

        public IEnumerable<IAveDiscoverFolder> GetSubFolders(AveDiscoverList list)
        {
            var rootFolder = list.GetRootFolder();
            return rootFolder.GetSubFolders();
        }

        public IEnumerable<IAveDiscoverItem> GetItems(IAveList list, AveDiscoverFolder folder, ref string pagerInfo)
        {
            var result = new List<AveDiscoverItem>();
            try
            {
                result = folder.GetItems();
            }
            catch (Exception ex)
            {
                logger.Error($"Error Get items:{folder?.FullUrl.LogBase64()}, ERROR:{ex.ToString()}");
            }
            finally
            {
                //result.CompleteAdding();
            }
            return result;
        }
        public IEnumerable<IAveDiscoverItem> GetItems(AveDiscoverList list, IAveList aveList, ref string pagerInfo)
        {
            using (var scope = new AgentPerformanceScope("RMSPDsicover.GetItems", $"RMSPDsicover.GetItems:{list?.RootFolderUrl}", true))
            {
                var rootFolder = list.GetRootFolder();
                return GetItems(aveList, rootFolder, ref pagerInfo);
            }

        }

        public IEnumerable<IAveListItem> GetAllItems(AveDiscoverList list, out long totalCount, List<AveCamlQuery> aveCamlQueries = null)
        {
            //var result = new BlockingCollection<IAveListItem>();
            BlockingCollection<IAveListItem> result = new BlockingCollection<IAveListItem>();
            totalCount = 1000;
            var listObj = list.GetListObject();
            try
            {
                if (aveCamlQueries == null || aveCamlQueries.Count == 0)
                {
                    var items = listObj.Items;
                    items.ToList().ForEach(i =>
                    {
                        result.Add(i);
                    });
                    result.CompleteAdding();
                }
                else
                {
                    ThreadPool.QueueUserWorkItem(obj =>
                    {
                        try
                        {
                            List<IAveFolder> discoverFolders = null;

                            using (var scope = new AgentPerformanceScope("RMSPFullDiscover.GetAllFolders", addToStatistics: true))
                            {
                                discoverFolders = SPCommonUtility.GetAllFolders(listObj);
                                logger.Info("The folder count:" + discoverFolders.Count);
                            }
                            foreach (var discoverFolder in discoverFolders)
                            {
                                foreach (var camlQuery in aveCamlQueries)
                                {
                                    camlQuery.FolderServerRelativeUrl = discoverFolder.ServerRelativeUrl;
                                    logger.Info("query xml {0}", camlQuery.ViewXml.LogBase64());
                                    var items = listObj.GetItems(camlQuery);
                                    items.ToList().ForEach(i =>
                                    {
                                        result.Add(i);
                                    });
                                    while (items.ListItemCollectionPosition != null)
                                    {
                                        camlQuery.ListItemCollectionPosition.PagingInfo = items.ListItemCollectionPosition.PagingInfo;
                                        items = listObj.GetItems(camlQuery);
                                        items.ToList().ForEach(i =>
                                        {
                                            result.Add(i);
                                        });
                                    }
                                }

                            }
                        }
                        catch (Exception ex)
                        {
                            logger.Error($"discover item error:{ex.ToString()}");
                        }
                        finally
                        {
                            result.CompleteAdding();
                        }

                    });
                }
            }
            catch (Exception ex)
            {
                result.CompleteAdding();
                logger.Error($"Error Get f items by query:{list?.RootFolderUrl.LogBase64()}, ERROR:{ex.ToString()}");
            }

            return result.GetConsumingEnumerable();
        }

        public IEnumerable<AveDiscoverList> GetLists(AveDiscoverWeb web, bool skipSystemList = true)
        {
            return web.GetLists().Values.SkipWhile(l => skipSystemList && this.IsSystemList(l));
        }

        public IEnumerable<AveDiscoverWeb> GetWebs(AveDiscoverSite site)
        {
            return site.GetWebs().Values;
        }

        public IEnumerable<AveDiscoverFolder> GetSubFolders(AveDiscoverFolder folder)
        {
            return folder.GetSubFolders();
        }

        public IEnumerable<IAveDiscoverItem> GetItems(IAveList list, IAveDiscoverFolder folder)
        {
            throw new NotImplementedException("Get items without pager method not implemented.");
        }

        public IEnumerable<IAveDiscoverItem> GetItems(AveDiscoverList list, IAveList aveList)
        {
            throw new NotImplementedException("Get items without pager method not implemented.");
        }

        public IAveDiscoverFolder GetRootFolder(AveDiscoverList list)
        {
            var rootFolder = list.GetRootFolder();
            return rootFolder;
        }
    }
}
