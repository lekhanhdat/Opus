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
using AvePoint.Wrapper.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.Wrapper.Restore
{ 
    public enum PostActionType
    {
        None=0,
        SitePostAction=1,
        WebPostAction=2,
        ListPostAction=3
    }

    public class PostFieldCacheWorker:IDisposable
    {
        private readonly object mLock = new object();
        private bool isDisposed;
        AveSPSite SPSite;
        public PostFieldCacheWorker(AveSPSite site)
        {
            SPSite = site;
            isDisposed = false;
            PostCacheList = new List<SPWebLevelCache> { };
        }
        private List<SPWebLevelCache> PostCacheList;

        public SPListLevelCache GetListCache(Guid webId,Guid listId)
        {
            lock (mLock)
            {
                var currentWeb = PostCacheList.FirstOrDefault(t => t.WebId == webId);
                if (currentWeb == null)
                {
                    return null;
                }
                var currentList = currentWeb.Lists.FirstOrDefault(t => t.ListId == listId);
                return currentList;
            }
        }

        public void FieldCacheSitePostAction()
        {
            foreach (var webCache in PostCacheList)
            {
                var web = SPSite.SPSite.OpenWeb(webCache.WebId);
                foreach (var listCache in webCache.Lists)
                {
                    var list = web.Lists.GetById(listCache.ListId);
                    FieldCachePostAction(list,PostActionType.SitePostAction);
                }
            }
        }

        //should check itemCachePostType
        public void FieldCachePostAction(IAveList currentList,PostActionType postType)
        {
            if (currentList == null)
            {
                return;
            }
            var listLevelCache = GetListCache(currentList.ParentWeb.ID, currentList.ID);
            if (listLevelCache == null)
            {
                return;
            }
            for (int k= listLevelCache.Items.Count-1; k>=0;k--)
            {
                var itemCache= listLevelCache.Items[k];
                if (itemCache.PostType < postType)
                {
                    continue;
                }
                Dictionary<int, int> itemIdCache;
                if (SPSite.MappingManager.SiteMappingManager.ItemIdMapping.TryGetValue(currentList.ID,out itemIdCache))
                {
                    int destinationItemId;
                    if (itemIdCache.TryGetValue(itemCache.RowId, out destinationItemId))
                    {
                        var item = currentList.GetItemById(destinationItemId);
                       int currentUIVersion = (int)item["_UIVersion"];
                        if (currentUIVersion != itemCache.UIVersion)
                        {
                            //version not match,should not update
                            continue;
                        }
                        bool needUpdateItem = false;
                        foreach (var fieldValue in itemCache.FieldValues)
                        {
                            var field = currentList.Fields.GetFieldByInternalName(fieldValue.Key);
                            var sourceValue = fieldValue.Value;
                            var newValue = sourceValue;
                            switch (field.Type)
                            {
                                case AveFieldType.URL:
                                    newValue = new UrlFieldValueHandler(SPSite).Process(field, sourceValue, false);
                                    break;
                                case AveFieldType.Note:
                                    newValue = new NoteFieldValueHandler(SPSite).Process(field, sourceValue, false);
                                    break;
                            }
                            //todo:handle field value replace
                            item[fieldValue.Key] = newValue;
                            needUpdateItem = true;
                        }
                        if (needUpdateItem)
                        {
                            item.SystemUpdate();
                            listLevelCache.Items.RemoveAt(k);
                        } 
                    }
                }
            }
        }

        public void AddCache(Guid webId,Guid listId,int itemId,int version,string fieldInternalName,object value,PostActionType postType)
        {
            lock (mLock)
            {
                var currentWeb = PostCacheList.FirstOrDefault(t => t.WebId == webId);
                if (currentWeb == null)
                {
                    currentWeb = new SPWebLevelCache(webId);
                    PostCacheList.Add(currentWeb);
                }
                var currentList = currentWeb.Lists.FirstOrDefault(t => t.ListId == listId);
                if (currentList == null)
                {
                    currentList = new SPListLevelCache(listId);
                    currentWeb.Lists.Add(currentList);
                }
                var currentItem = currentList.Items.FirstOrDefault(t => t.RowId == itemId);
                if (currentItem == null)
                {
                    currentItem = new SPItemLevelCache(itemId);
                    currentList.Items.Add(currentItem);
                }
                if (currentItem.UIVersion < version)
                {
                    //current added is larger version,so clear the history version,add current one
                    currentItem.FieldValues.Clear();
                    currentItem.UIVersion = version;
                }
                else if (currentItem.UIVersion > version)
                {
                    //cached item version is larger,so current added is version's, skip it.
                    return;
                }
                else
                {
                    //equal, only add field value cache
                }
                if (currentItem.FieldValues.ContainsKey(fieldInternalName))
                {
                    //should not go inside
                    //todo:need log
                }
                if (currentItem.PostType > postType)
                {
                    currentItem.PostType = postType;
                }
                currentItem.FieldValues[fieldInternalName] = value;
            }
        }



        public void Dispose()
        {
            if (!isDisposed)
            {
                isDisposed = true;
                PostCacheList.Clear();
            }
        }
    }

    public class SPWebLevelCache
    {
        public SPWebLevelCache(Guid webId)
        {
            WebId = webId;
            Lists = new List<SPListLevelCache> { };
        }
        public Guid WebId;
        public List<SPListLevelCache> Lists;
    }
    public class SPListLevelCache
    {
        public SPListLevelCache(Guid listId)
        {
            ListId = listId;
            Items = new List<SPItemLevelCache> { };
        }
        public Guid ListId;
        public List<SPItemLevelCache> Items;
    }

    public class SPItemLevelCache
    {
        public SPItemLevelCache(int rowId)
        {
            RowId = rowId;
            PostType = PostActionType.ListPostAction;
            FieldValues = new Dictionary<string, object>();
        }
        public int UIVersion;
        public int RowId;
        public PostActionType PostType;
        public Dictionary<string,object> FieldValues;
    }

    public class SPFieldValueCache
    {
        public string InternalName;
        public object Value;
    }
}
