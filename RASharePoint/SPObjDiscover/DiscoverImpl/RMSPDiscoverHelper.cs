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
    public class RMSPDiscoverHelper/* : ISPDiscoverHelper*/
    {
        private static readonly AvePoint.RA.Contract.Services.IRALogger logger = RALogger.GetInstance(typeof(RMSPDiscoverHelper));
        public RMSPDiscoverHelper()
        {
        }

        public BlockingCollection<IAveListItem> GetChangedItems(AveDiscoverList list, List<AveCamlQuery> aveCamlQueries, bool scanALLIfNoQuery = false)
        {
            var result = new BlockingCollection<IAveListItem>();
            try
            {
                var listObj = list.GetListObject();
                if (scanALLIfNoQuery && aveCamlQueries.Count == 0)
                {
                    var rootFolder = list.GetChangeRootFolder();
                    rootFolder.GetChangeItems().Where(i => i.ChangeType != ChangeType.Delete && i.ID != null).ToList().ForEach(i =>
                    {
                        var item = listObj.GetItemById((int)i.ID);
                        result.Add(item);
                    });
                }
                else
                {
                    ThreadPool.QueueUserWorkItem(obj =>
                    {
                        foreach (var camlQuery in aveCamlQueries)
                        {
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
                    });
                }

            }
            catch (Exception ex)
            {
                logger.Error($"Error Get items by query:{list?.RootFolderUrl}, ERROR:{ex.ToString()}");
            }
            finally
            {
                result.CompleteAdding();
            }
            return result;
        }

        public BlockingCollection<IAveListItem> GetChangedItems(IAveList list, AveDiscoverFolder folder)
        {
            var result = new BlockingCollection<IAveListItem>();
            try
            {
                folder.GetChangeItems().Where(i => i.ChangeType != ChangeType.Delete && i.ID != null).ToList().ForEach(i =>
                {
                    var item = list.GetItemById((int)i.ID);
                    result.Add(item);
                });
            }
            catch (Exception ex)
            {
                logger.Error($"Error Get changed items by:{folder?.FullUrl}, ERROR:{ex.ToString()}");
            }
            finally
            {
                result.CompleteAdding();
            }
            return result;
        }

        public BlockingCollection<IAveListItem> GetItems(IAveList list, List<AveCamlQuery> aveCamlQueries, bool scanALLIfNoQuery = false) 
        {
            var result = new BlockingCollection<IAveListItem>();
            try
            {
                if (scanALLIfNoQuery && aveCamlQueries.Count == 0)
                {
                    var items = list.Items;
                    items.ToList().ForEach(i =>
                    {
                        result.Add(i);
                    });
                }
                else 
                {
                    ThreadPool.QueueUserWorkItem(obj =>
                    {
                        foreach (var camlQuery in aveCamlQueries)
                        {
                            var items = list.GetItems(camlQuery);
                            items.ToList().ForEach(i =>
                            {
                                result.Add(i);
                            });
                            while (items.ListItemCollectionPosition != null)
                            {
                                camlQuery.ListItemCollectionPosition.PagingInfo = items.ListItemCollectionPosition.PagingInfo;
                                items = list.GetItems(camlQuery);
                                items.ToList().ForEach(i =>
                                {
                                    result.Add(i);
                                });
                            }
                        }
                    });
                }
                
            }
            catch (Exception ex)
            {
                logger.Error($"Error Get items by query:{list?.RootFolder?.Url}, ERROR:{ex.ToString()}");
            }
            finally
            {
                result.CompleteAdding();
            }
            return result;
        }

        public BlockingCollection<IAveListItem> GetItems(IAveList list, AveDiscoverFolder folder)
        {
            var result = new BlockingCollection<IAveListItem>();
            try
            {
                folder.GetItems().Where(i => i.ChangeType != ChangeType.Delete && i.ID != null).ToList().ForEach(i =>
                {
                    var item = list.GetItemById((int)i.ID);
                    result.Add(item);
                });
            }
            catch (Exception ex)
            {
                logger.Error($"Error Get items:{folder?.FullUrl}, ERROR:{ex.ToString()}");
            }
            finally
            {
                result.CompleteAdding();
            }
            return result;
        }
    }
}
