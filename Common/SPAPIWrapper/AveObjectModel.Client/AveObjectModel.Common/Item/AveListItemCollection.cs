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
using AvePoint.Wrapper.Common;
namespace AvePoint.ObjectModel.Common
{
    class AveListItemCollection : AveAbstractCommonCollection<IAveListItem>, IAveListItemCollection
    {
        private IAveRequest mRequest;
        private IAveWeb mParentWeb;
        private IAveList mParentList;
        private AveListItemCollectionPosition mListItemCollectionPosition;

        public string PageInfo
        {
            get
            {
                if (base.DataCache.IsPropertyAvailable("PageInfo"))
                {
                    return base.DataCache.GetProperty<string>("PageInfo");
                }
                return null;
            }
        }

        public AveListItemCollection(IAveRequest request, IAveWeb parentWeb, IAveList parentList, bool isNewCreated, Dictionary<string, object> prop)
        {
            mRequest = request;
            mParentWeb = parentWeb;
            mParentList = parentList;
            base.DataCache.AddPropertyies(prop);
            this.mListData = new List<IAveListItem>(prop.Count);
            InitItemCollection(isNewCreated);
        }

        private void InitItemCollection(bool isNewCreated)
        {
            foreach (var dic in base.DataCache.GetChildren())
            {
                AveListItem item = new AveListItem(mRequest, mParentWeb, mParentList, dic, isNewCreated);
                this.Add(item);
            }
        }

        public void Add(IAveListItem listItem)
        {
            mListData.Add(listItem);
        }

        #region IAveListItemCollection Members

        public IAveListItem Add(string folderUrl, AveFileSystemObjectType underlyingObjectType, string leafName)
        {
            return mParentList.AddItem(folderUrl, underlyingObjectType, leafName);
        }

        public IAveListItem Add(string folderUrl, AveFileSystemObjectType underlyingObjectType)
        {
            return this.Add(folderUrl, underlyingObjectType, string.Empty);
        }

        public void Delete(int index)
        {
            IAveListItem item = this[index];
            if (item != null)
            {
                item.Delete();
                mListData.RemoveAt(index);
            }
        }

        public IAveListItem GetById(int id)
        {
            return mListData.Find(
                delegate(IAveListItem itm)
                {
                    return itm.ID == id;
                });
        }

        public IAveListItem GetById(string id)
        {
            return mListData.Find(
                delegate(IAveListItem item)
                {
                    return item.ID == int.Parse(id);
                });
        }


        public IAveListItem this[int index]
        {
            get
            {
                return mListData[index];
            }
        }

        public IAveListItem this[Guid id]
        {
            get
            {
                return mListData.Find(
                    delegate(IAveListItem item)
                    {
                        return item.UniqueId.Equals(id);
                    });
            }
        }
        public IAveListItem GetItemByGuid(Guid guid) 
        {
            return mListData.Find(
                delegate(IAveListItem item)
                {
                    return item.GetTPGuid().Equals(guid);
                });
        }
        public IAveListItem Add()
        {
            throw new NotImplementedException();
        }

        public IAveList List
        {
            get
            {
                return mParentList;
            }
        }

        public IAveListItemCollectionPosition ListItemCollectionPosition
        {
            get
            {
                if (!string.IsNullOrEmpty(PageInfo))
                {
                    if (mListItemCollectionPosition == null)
                    {
                        mListItemCollectionPosition = new AveListItemCollectionPosition();
                    }
                    mListItemCollectionPosition.PagingInfo = PageInfo;
                    return mListItemCollectionPosition;
                }
                return null;
            }
        }

        #endregion
    }
}
