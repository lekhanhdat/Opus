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
using AvePoint.Wrapper.Common.Office;
using Microsoft.Office.Server.Utilities;
using AvePoint.Wrapper.Common;
using Microsoft.SharePoint;

namespace AvePoint.ObjectModel.ServerSE.Office
{
    class AveOContentIterator : IAveOContentIterator
    {
        private ContentIterator mContentIterator;

        public AveOContentIterator()
        {
            mContentIterator = new ContentIterator();
        }

        public AveOContentIterator(ContentIterator contentIterator)
        {
            mContentIterator = contentIterator;
        }

        #region IAveOContentIterator Members

        public void ProcessLists(IAveListCollection lists, ListProcessor listProcessor, ListProcessorErrorCallout errorCallout)
        {
            string[] strArray;
            int num;
            int num2;
            if (lists == null)
            {
                throw new ArgumentNullException("lists");
            }
            if (listProcessor == null)
            {
                throw new ArgumentNullException("listProcessor");
            }
            string strWebId = lists.Web.ID.ToString("B");
            this.ResumeProcessLists(strWebId, lists, out strArray, out num, out num2);
            while (num2 < num)
            {
                string str2 = (strArray != null) ? strArray[num2] : null;
                IAveList list = !string.IsNullOrEmpty(str2) ? lists[new Guid(str2)] : lists[num2];
                try
                {
                    string title = list.Title;
                    listProcessor(list);
                    this.OnProcessedList(strWebId, list, title, str2);
                }
                catch (Exception exception)
                {
                    if ((errorCallout == null) || errorCallout(list, exception))
                    {
                        throw;
                    }
                }
                if (this.ShouldCancel(AveIterationGranularity.List))
                {
                    return;
                }
                num2++;
            }
        }

        public void ProcessListItems(IAveList list, string strQuery, bool fRecursive, ItemsProcessor itemsProcessor, ItemsProcessorErrorCallout errorCallout)
        {
            this.ProcessListItems(list, strQuery, 0, fRecursive, itemsProcessor, errorCallout);
        }

        public void ProcessListItems(IAveList list, string strQuery, uint rowLimit, bool fRecursive, ItemsProcessor itemsProcessor, ItemsProcessorErrorCallout errorCallout)
        {
            this.ProcessListItems(list, strQuery, rowLimit, fRecursive, null, itemsProcessor, errorCallout);
        }

        public void ProcessListItems(IAveList list, string strQuery, uint rowLimit, bool fRecursive, IAveFolder folder, ItemsProcessor itemsProcessor, ItemsProcessorErrorCallout errorCallout)
        {
            if (list == null)
            {
                throw new ArgumentNullException("list");
            }
            if (itemsProcessor == null)
            {
                throw new ArgumentNullException("itemsProcessor");
            }
            AveQuery query = new AveQuery();
            if (!string.IsNullOrEmpty(strQuery))
            {
                query.Query = strQuery;
            }
            query.RowLimit = rowLimit;
            if (folder != null)
            {
                query.Folder = folder;
            }
            if (fRecursive)
            {
                query.ViewAttributes = "Scope=\"RecursiveAll\"";
            }
            this.ProcessListItems(list, query, itemsProcessor, errorCallout);

        }

        public void ResumeProcessLists(string strWebId, IAveListCollection lists, out string[] listNames, out int cLists, out int iList)
        {
            if (lists == null)
            {
                throw new ArgumentNullException("lists");
            }
            listNames = null;
            cLists = lists.Count;
            iList = 0;

            object[] objs = new object[] { strWebId, (lists as AveListCollection).Lists, listNames, cLists, iList };
            AveAssemblyUtility.InvokeMethod(mContentIterator, "ResumeProcessLists", objs);
            if (objs[2] != null)
            {
                listNames = (string[])objs[2];
            }
            cLists = (int)objs[3];
            iList = (int)objs[4];
        }

        public void OnProcessedList(string strWebId, IAveList list, string strListTitle, string strListName)
        {
            AveAssemblyUtility.InvokeMethod(mContentIterator, "OnProcessedList", new Type[] { typeof(string), typeof(SPList), typeof(string), typeof(string) }, new object[] { strWebId, (list as AveList).List, strListTitle, strListName });
        }

        public bool ShouldCancel(AveIterationGranularity granularity)
        {
            return mContentIterator.ShouldCancel((IterationGranularity)granularity);
        }

        public void ProcessListItems(IAveList list, IAveQuery query, ItemsProcessor itemsProcessor, ItemsProcessorErrorCallout errorCallout)
        {
            string str2;
            IAveListItemCollection items;
            if (list == null)
            {
                throw new ArgumentNullException("list");
            }
            if (query == null)
            {
                throw new ArgumentNullException("query");
            }
            if (itemsProcessor == null)
            {
                throw new ArgumentNullException("itemsProcessor");
            }
            if (!list.HasExternalDataSource && (list.ItemCount == 0))
            {
                return;
            }
            if (list.HasExternalDataSource && (query.RowLimit == 0))
            {
                AveOULS ULS = new AveOULS();
                AveOULSCat ULSCat = new AveOULSCat();
                ULS.SendTraceTag(0x626e6c30, ULSCat.msoulscat_OSRV_General, AveULSTraceLevel.Medium, "RowLimit set for list with external data source...setting RowLimit for query: {0}", new object[] { query.Query });
                query.RowLimit = 0x7fffffff;
            }
            else if ((query.RowLimit == 0) || (query.RowLimit == 0x7fffffff))
            {
                AveOULS ULS = new AveOULS();
                AveOULSCat ULSCat = new AveOULSCat();
                ULS.SendTraceTag(0x6132746a, ULSCat.msoulscat_OSRV_General, AveULSTraceLevel.Medium, "RowLimit unset...using default RowLimit for query: {0}", new object[] { query.Query });
                //query.RowLimit = string.IsNullOrEmpty(query.ViewFields) ? 200 : 0x7d0;
                if (string.IsNullOrEmpty(query.ViewFields))
                {
                    query.RowLimit = 200;
                }
                else
                {
                    query.RowLimit = 0x7d0;
                }
            }
            if (!list.HasExternalDataSource && this.StrictQuerySemantics)
            {
                query.QueryThrottleMode = AveQueryThrottleOption.Strict;
            }
            string strListId = list.ID.ToString("B");
            this.ResumeProcessListItemsBatch(strListId, out str2);
            if (!string.IsNullOrEmpty(str2))
            {
                query.ListItemCollectionPosition = new AveListItemCollectionPosition(new SPListItemCollectionPosition(str2));
            }
            int batchNo = 0;

            bool jugde;
            do
            {
                jugde = false;
                items = list.GetItems(query);
                int count = items.Count;
                batchNo++;
                try
                {
                    itemsProcessor(items);
                    this.OnProcessedListItemsBatch(strListId, items, batchNo, count);
                }
                catch (Exception exception)
                {
                    if ((errorCallout == null) || errorCallout(items, exception))
                    {
                        throw;
                    }
                }
                if (!this.ShouldCancel(AveIterationGranularity.Item))
                {
                    query.ListItemCollectionPosition = items.ListItemCollectionPosition;
                    if (query.ListItemCollectionPosition != null)
                    {
                        jugde = true;
                    }
                }
            } while (jugde);
        }

        public bool StrictQuerySemantics
        {
            get
            {
                return mContentIterator.StrictQuerySemantics;
            }
            set
            {
                mContentIterator.StrictQuerySemantics = value;
            }
        }

        public void OnProcessedListItemsBatch(string strListId, IAveListItemCollection items, int batchNo, int batchItemCount)
        {
            AveAssemblyUtility.InvokeMethod(mContentIterator, "OnProcessedListItemsBatch",new Type[]{typeof(string),typeof(SPListItemCollection),typeof(int),typeof(int)},new object[]{strListId, (items as AveListItemCollection).ListItemCollection, batchNo, batchItemCount});
        }

        public void ResumeProcessListItemsBatch(string strListId, out string strPagingInfo)
        {
            strPagingInfo = null;
            object[] objs =new object[] { strListId, strPagingInfo };
            AveAssemblyUtility.InvokeMethod(mContentIterator, "ResumeProcessListItemsBatch", objs);
            if (objs[1] != null)
            {
                strPagingInfo = objs[1].ToString();
            }
        }

        #endregion
    }
}
