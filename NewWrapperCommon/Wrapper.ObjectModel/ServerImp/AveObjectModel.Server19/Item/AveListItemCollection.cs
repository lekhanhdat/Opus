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
using Microsoft.SharePoint;
using AvePoint.Wrapper.Common;
using AvePoint.GCommon;
using System.Reflection;

namespace AvePoint.ObjectModel.Server19
{
    class AveListItemCollection : AveAbstractCommonCollection<IAveListItem>, IAveListItemCollection
    {
        private static AveLogger logger = AveLogger.GetInstance(typeof(AveListItemCollection));
        private SPListItemCollection mListItems;
        private AveList mList;
        private AveListItemCollectionPosition mListItemCollectionPosition;
        private static MethodInfo m_internalMethod = null;
        private MethodInfo internalMethod
        {
            get
            {
                if (m_internalMethod == null)
                {
                    m_internalMethod = typeof(SPListItem).GetMethod("SetIDForMigration", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static | BindingFlags.IgnoreCase);
                }
                return m_internalMethod;
            }
        }

        public AveListItemCollection(AveList list, SPListItemCollection listItems)
            : base(listItems)
        {
            mList = list;
            mListItems = listItems;
        }

        #region IAveListItemCollection Members

        public IAveListItem GetById(int id)
        {
            SPListItem item;
            try
            {
                item = mListItems.GetItemById(id);
            }
            catch (Exception ex)
            {
                //List Item 获取不到的原因很多 1.只有一个Checkout Version 2.Dead User对应的Item 
                logger.Warn("Failed to get list item {0}, error message: {1} ", id, ex);
                item = null;
            }
            if (item == null)
            {
                return null;
            }
            return new AveListItem(this, item);
        }

        public IAveListItem GetById(string id)
        {
            int itemid = 0;
            if (int.TryParse(id, out itemid))
            {
                return GetById(itemid);
            }
            return null;
        }

        public IAveListItem this[Guid id]
        {
            get
            {
                return new AveListItem(this, mListItems[id]);
            }
        }

        public IAveListItem Add(string folderUrl, AveFileSystemObjectType underlyingObjectType, string leafName)
        {
            return new AveListItem(this, mListItems.Add(folderUrl, (SPFileSystemObjectType)underlyingObjectType, leafName));
        }

        public IAveListItem Add(string folderUrl, AveFileSystemObjectType underlyingObjectType, string leafName, int rowId)
        {
            var item = mListItems.Add(folderUrl, (SPFileSystemObjectType)underlyingObjectType, leafName);
            internalMethod.Invoke(item, new object[] { rowId });
            return new AveListItem(this, item);
        }

        public IAveListItem Add(string folderUrl, AveFileSystemObjectType underlyingObjectType)
        {
            return new AveListItem(this, mListItems.Add(folderUrl, (SPFileSystemObjectType)underlyingObjectType));
        }

        public IAveListItem Add()
        {
            return new AveListItem(this, mListItems.Add());
        }

        public void Delete(int index)
        {
            mListItems.Delete(index);
        }

        public void ReloadItems(IAveListItem items)
        {
            throw new NotImplementedException();
        }

        #endregion

        public override IAveListItem this[int index]
        {
            get
            {
                SPListItem listItem = mListItems[index];
                if (listItem == null)
                {
                    return null;
                }
                return new AveListItem(this, listItem);
            }
        }

        protected override object CreatElementInstance(object t)
        {
            return new AveListItem(this, t as SPListItem);
        }

        public override int Count
        {
            get { return mListItems.Count; }
        }

        public IAveList List
        {
            get { return mList; }
        }

        internal SPListItemCollection ListItemCollection
        {
            get { return mListItems; }
        }

        public IAveListItemCollectionPosition ListItemCollectionPosition
        {
            get
            {
                if (mListItemCollectionPosition == null)
                {
                    SPListItemCollectionPosition listItemsPosition = mListItems.ListItemCollectionPosition;
                    if (listItemsPosition != null)
                    {
                        mListItemCollectionPosition = new AveListItemCollectionPosition(mListItems.ListItemCollectionPosition);
                    }
                }
                return mListItemCollectionPosition;
            }
        }

    }
}
