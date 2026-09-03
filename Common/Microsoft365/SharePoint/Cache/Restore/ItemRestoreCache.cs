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
using Microsoft.SharePoint.ESign.Models.Requests;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft365.SharePoint.Cache.Restore
{
    public class ItemRestoreCache
    {
        
        private static volatile Dictionary<string, HashSet<string>> overwriteFailItemMap = new Dictionary<string, HashSet<string>>();

        private static volatile Dictionary<string, HashSet<string>> newCreateItemMap = new Dictionary<string, HashSet<string>>();

        public static void AddOverWriteFailItem(string listId, string itemId)
        {
            if(string.IsNullOrWhiteSpace(listId) || string.IsNullOrWhiteSpace(itemId))
            {
                return;
            }
            lock (typeof(ItemRestoreCache))
            {
                if (!overwriteFailItemMap.ContainsKey(listId))
                {
                    overwriteFailItemMap[listId] = new HashSet<string>();
                }
                overwriteFailItemMap[listId].Add(itemId);
            }
        }

        public static bool IsOverWriteFailItem(string listid, string itemId)
        {
            HashSet<string> itemSet;
            overwriteFailItemMap.TryGetValue(listid, out itemSet);
            return itemSet != null && itemSet.Contains(itemId);
        }

        public static void ClearOverWriteFailCache()
        {
            lock (typeof(ItemRestoreCache))
            {
                overwriteFailItemMap.Clear();
            }
        }

        public static void ClearOverWriteFailCache(string listId)
        {
            lock (typeof(ItemRestoreCache))
            {
                if(listId == null)
                {
                    return;
                }
                overwriteFailItemMap.Remove(listId);
            }
        }

        public static void AddNewCreateItem(string listId, string itemId)
        {
            if (string.IsNullOrWhiteSpace(listId) || string.IsNullOrWhiteSpace(itemId))
            {
                return;
            }
            lock (typeof(ItemRestoreCache))
            {
                if (!newCreateItemMap.ContainsKey(listId))
                {
                    newCreateItemMap[listId] = new HashSet<string>();
                }
                newCreateItemMap[listId].Add(itemId);
            }
        }

        public static bool IsNewCreateItem(string listid, string itemId)
        {
            HashSet<string> itemSet;
            newCreateItemMap.TryGetValue(listid, out itemSet);
            return itemSet != null && itemSet.Contains(itemId);
        }

        public static void ClearNewCreateItemCache()
        {
            lock (typeof(ItemRestoreCache))
            {
                newCreateItemMap.Clear();
            }
        }

        public static void ClearNewCreateItemCache(string listId)
        {
            lock (typeof(ItemRestoreCache))
            {
                if (listId == null)
                {
                    return;
                }
                newCreateItemMap.Remove(listId);
            }
        }
    }
}
