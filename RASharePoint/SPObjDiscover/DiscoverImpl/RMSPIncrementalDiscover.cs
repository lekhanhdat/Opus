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
    public class RMSPIncrementalDiscover : RMSPDiscoverBase, ISPDiscover
    {
        private static readonly AvePoint.RA.Contract.Services.IRALogger logger = RALogger.GetInstance(typeof(RMSPIncrementalDiscover));
        public RMSPDiscoverHelper mdiscoverHelper;

        public RMSPIncrementalDiscover(RMSPDiscoverHelper discoverHelper)
        {
            this.mdiscoverHelper = discoverHelper;
        }

        public IEnumerable<AveDiscoverFolder> GetSubFolders(AveDiscoverList list)
        {
            var discoverRootFolder = list.GetChangeRootFolder();
            return discoverRootFolder.GetChangeSubFoldersWithoutCache();
        }

        public IEnumerable<AveDiscoverItem> GetItems(IAveList list, AveDiscoverFolder folder)
        {
            var result = new BlockingCollection<AveDiscoverItem>();

            return folder.GetChangeItemsWithoutCache();
        }

        public IEnumerable<AveDiscoverItem> GetItems(AveDiscoverList list, IAveList aveList)
        {
            var rootFolder = list.GetChangeRootFolder();
            return GetItems(aveList, rootFolder);
        }

        public IEnumerable<IAveListItem> GetAllItems(AveDiscoverList list, out long totalCount, List<AveCamlQuery> aveCamlQueries = null)
        {
            var result = new BlockingCollection<IAveListItem>();
            long subItemsCount = 0;
            totalCount = 0;
            try
            {

                var listObj = list.GetListObject();
                if (aveCamlQueries == null || aveCamlQueries.Count == 0)
                {
                    var rootFolder = list.GetChangeRootFolder();
                    var items = rootFolder.GetChangeItems();
                    var subFolders = rootFolder.GetChangeSubFolders();
                    totalCount = items.Count() + subFolders.Count();
                    ThreadPool.QueueUserWorkItem(obj =>
                    {
                        try
                        {

                            items.Where(i => i.ChangeType != ChangeType.Delete && i.ID != null).ToList().ForEach(i =>
                            {
                                var item = listObj.GetItemById((int)i.ID);
                                result.Add(item);
                            });

                            DiscoverFolders(listObj, subFolders, result, ref subItemsCount);

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
                else
                {
                    throw new NotImplementedException();
                }

            }
            catch (Exception ex)
            {
                result.CompleteAdding();
                logger.Error($"Error Get i items by query:{list?.RootFolderUrl}, ERROR:{ex.ToString()}");
            }
            return result.GetConsumingEnumerable();
        }

        private void DiscoverFolders(IAveList list, List<AveDiscoverFolder> folders, BlockingCollection<IAveListItem> result, ref long total)
        {
            var tempFolders = folders.Where(f => f.ChangeType != ChangeType.Delete && f.ID != null).ToList();
            foreach (var folder in tempFolders)
            {
                var tempChangeItems = folder.GetChangeItems();
                total += tempChangeItems.Count;
                tempChangeItems.Where(i => i.ChangeType != ChangeType.Delete && i.ID != null).ToList().ForEach(i =>
                {
                    var item = list.GetItemById((int)i.ID);
                    result.Add(item);
                });
                var subFolders = folder.GetChangeSubFolders();
                total += subFolders.Count;
                DiscoverFolders(list, subFolders, result, ref total);
            }


        }

        public IEnumerable<AveDiscoverList> GetLists(AveDiscoverWeb web, bool skipSystemList = true)
        {
            return web.GetChangeLists().Values.SkipWhile(l => (skipSystemList && IsSystemList(l)));
        }

        public IEnumerable<AveDiscoverWeb> GetWebs(AveDiscoverSite site)
        {
            return site.GetChangeWebs().Values;
        }

        public IEnumerable<AveDiscoverFolder> GetSubFolders(AveDiscoverFolder folder)
        {
            return folder.GetChangeSubFoldersWithoutCache();
        }

        public IEnumerable<AveDiscoverItem> GetItems(IAveList list, AveDiscoverFolder folder, ref string pagerInfo)
        {
            pagerInfo = string.Empty;
            return this.GetItems(list, folder);
        }

        public IEnumerable<AveDiscoverItem> GetItems(AveDiscoverList list, IAveList aveList, ref string pagerInfo)
        {
            using (var performance = new PerformanceScope("RMSPDsicover.GetItems", $"RMSPDsicover.GetItems:{list?.RootFolderUrl}"))
            {
                pagerInfo = string.Empty;
                return this.GetItems(list, aveList);
            }
        }

        public AveDiscoverFolder GetRootFolder(AveDiscoverList list)
        {
            var discoverRootFolder = list.GetChangeRootFolder();
            return discoverRootFolder;
        }
    }
}
