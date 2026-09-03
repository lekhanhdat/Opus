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
namespace GCommon.SharePointUtility.Controller
{
    using System;
    using System.Collections.Generic;
    using Microsoft.SharePoint.Client;


    public class ContentIterator
    {
        private readonly ClientRuntimeContext _context;

        public ContentIterator(ClientRuntimeContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException("context");
            }
            _context = context;
        }

        public delegate void ItemsProcessor(ListItemCollection items);

        public delegate void ItemsRetriever(ListItemCollection items);

        public delegate bool ItemsProcessorErrorCallout(ListItemCollection items, System.Exception e);

        /// <summary>
        /// Process ListItems batch by batch
        /// </summary>
        /// <param name="listName">ListName</param>
        /// <param name="camlQuery">CamlQuery</param>
        /// <param name="itemsProcessor">itemprocessor delegate</param>
        /// <param name="errorCallout">error delegate</param>
        public void ProcessListItems(List list, CamlQuery camlQuery, ItemsRetriever itemsRetriever, ItemsProcessor itemsProcessor, ItemsProcessorErrorCallout errorCallout)
        {
            CamlQuery query = camlQuery;

            ListItemCollectionPosition position = null;
            query.ListItemCollectionPosition = position;

            //make a copy to reduce the memory
            var originalObjectPaths = new Dictionary<long, ObjectPath>(_context.ReadObjectPaths());

            while (true)
            {
                ListItemCollection listItems = list.GetItems(query);
                _context.Load(listItems, items => items.ListItemCollectionPosition, items => items.Include(item => item.Id));

                if (itemsRetriever != null)
                {
                    itemsRetriever(listItems);
                }

                _context.ExecuteQuery();

                try
                {
                    itemsProcessor(listItems);
                }
                catch (Exception ex)
                {
                    if (errorCallout == null || errorCallout(listItems, ex))
                    {
                        throw;
                    }
                }

                if (listItems.ListItemCollectionPosition == null)
                {
                    return;
                }
                else
                {
                    /*if query contains lookup column filter last batch returns null 
                     by removing the lookup column in paginginfo query will return next records
                     */
                    string pagingInfo = listItems.ListItemCollectionPosition.PagingInfo;
                    string[] parameters = pagingInfo.Split(new char[] { '&' }, StringSplitOptions.RemoveEmptyEntries);
                    List<string> requiredParameters = new List<string>();
                    foreach (string str in parameters)
                    {
                        if (str.Contains("Paged=") || str.Contains("p_ID="))
                            requiredParameters.Add(str);
                    }

                    pagingInfo = string.Join("&", requiredParameters.ToArray());
                    listItems.ListItemCollectionPosition.PagingInfo = pagingInfo;
                    query.ListItemCollectionPosition = listItems.ListItemCollectionPosition;
                }

                //always write a new copy to reduce the memory
                _context.WriteObjectPaths(new Dictionary<long, ObjectPath>(originalObjectPaths));
            }
        }
    }
}
