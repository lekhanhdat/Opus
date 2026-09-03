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
    class AveListItemVersionCollection : AveAbstractCommonCollection<IAveListItemVersion>, IAveListItemVersionCollection
    {
        private AveListItem mListItem;
        private IAveRequest mRequest;

        public AveListItemVersionCollection(AveListItem listItem, IAveRequest request, Dictionary<string, object> listItemVerColProperites)
        {
            mListItem = listItem;
            mRequest = request;
            base.DataCache.AddPropertyies(listItemVerColProperites);
            InitListItemVersionCollection();
        }

        internal void InitListItemVersionCollection()
        {
            List<Dictionary<string, object>> listItemVerPropertiesList = base.DataCache.GetProperty<List<Dictionary<string, object>>>(AveObjectModelConstant.ChildrenProperties);
            mListData = new List<IAveListItemVersion>(listItemVerPropertiesList.Count);
            foreach (Dictionary<string, object> listItemVerProperites in listItemVerPropertiesList)
            {
                AveListItemVersion listItemVer = new AveListItemVersion(this, mRequest, listItemVerProperites);
                mListData.Add(listItemVer);
            }
        }

        #region IAveListItemVersionCollection Members

        public IAveListItem ListItem
        {
            get 
            { 
                return mListItem; 
            }
        }

        public IAveListItemVersion GetVersionFromID(int versionId)
        {
            return mListData.Find(v => v.VersionId.Equals(versionId));
        }

        #endregion      
    

        public IAveWeb Web
        {
            get 
            { 
                return mListItem.ParentList.ParentWeb; 
            }
        }


        public void RecycleAll()
        {
            //client API没有该方法
        }

        public void DeleteAll()
        {
            //client API没有该方法
        }

    }
}
