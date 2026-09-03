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

namespace AvePoint.Wrapper.Common.Office
{
    public interface IAveOContentIterator
    {
        bool StrictQuerySemantics { get; set; }

        void ProcessLists(IAveListCollection lists, ListProcessor listProcessor, ListProcessorErrorCallout errorCallout);
        void ProcessListItems(IAveList list, IAveQuery query, ItemsProcessor itemsProcessor, ItemsProcessorErrorCallout errorCallout);
        void ProcessListItems(IAveList list, string strQuery, bool fRecursive, ItemsProcessor itemsProcessor, ItemsProcessorErrorCallout errorCallout);
        void ProcessListItems(IAveList list, string strQuery, uint rowLimit, bool fRecursive, ItemsProcessor itemsProcessor, ItemsProcessorErrorCallout errorCallout);
        void ProcessListItems(IAveList list, string strQuery, uint rowLimit, bool fRecursive, IAveFolder folder, ItemsProcessor itemsProcessor, ItemsProcessorErrorCallout errorCallout);
        void ResumeProcessLists(string strWebId, IAveListCollection lists, out string[] listNames, out int cLists, out int iList);
        void OnProcessedList(string strWebId, IAveList list, string strListTitle, string strListName);
        bool ShouldCancel(AveIterationGranularity granularity);
        void OnProcessedListItemsBatch(string strListId, IAveListItemCollection items, int batchNo, int batchItemCount);
        void ResumeProcessListItemsBatch(string strListId, out string strPagingInfo);
    }

    public delegate void ListProcessor(IAveList list);

    public delegate void ItemsProcessor(IAveListItemCollection items);

    public delegate bool ItemsProcessorErrorCallout(IAveListItemCollection items, Exception e);

    public delegate bool ListProcessorErrorCallout(IAveList list, Exception e);
}
