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
    class AveUserCustomActionCollection : AveAbstractCommonCollection<IAveUserCustomAction>, IAveUserCustomActionCollection
    {
        private IAveRequest mRequest;
        private AveSite mParentSite;
        private AveWeb mParentWeb;
        private AveList mParentList;
        private AveUserCustomActionScope mCollectionScope;
        public AveUserCustomActionCollection(AveSite site, IAveRequest request, Dictionary<string, object> prop)
        {
            mRequest = request as IAveRequest;
            mParentSite = site;
            mCollectionScope = AveUserCustomActionScope.Site;
            InitUserCustomActionCollection(prop);
        }

        public AveUserCustomActionCollection(AveSite site, AveWeb web, IAveRequest request, Dictionary<string, object> prop)
        {
            mRequest = request as IAveRequest;
            mParentSite = site;
            mParentWeb = web;
            mCollectionScope = AveUserCustomActionScope.Web;
            InitUserCustomActionCollection(prop);
        }
        public AveUserCustomActionCollection(AveSite site, AveWeb web, AveList list, IAveRequest request, Dictionary<string, object> prop)
        {
            mRequest = request as IAveRequest;
            mParentSite = site;
            mParentWeb = web;
            mParentList = list;
            mCollectionScope = AveUserCustomActionScope.List;
            InitUserCustomActionCollection(prop);
        }

        private void InitUserCustomActionCollection(IDictionary<string, object> prop)
        {
            base.DataCache.AddPropertyies(prop);
            this.mListData = new List<IAveUserCustomAction>();
            foreach (var dic in base.DataCache.GetChildren())
            {
                var userCustomAction = CreateUserCustomActionByScope(dic);
                mListData.Add(userCustomAction);
            }
        }

        private IAveUserCustomAction CreateUserCustomActionByScope(IDictionary<string,object> properties)
        {
            switch (mCollectionScope)
            {
                case AveUserCustomActionScope.Site:
                    return new AveSiteUserCustomAction(mParentSite, this, mRequest, properties);
                case AveUserCustomActionScope.Web:
                    return new AveWebUserCustomAction(mParentWeb, this, mRequest, properties);
                case AveUserCustomActionScope.List:
                    return new AveListUserCustomAction(mParentList, this, mRequest, properties);
                default:
                    return null;
            }
        }
  
        public IAveUserCustomAction Add(string location)
        {
            var webUrl = mParentWeb == null ? "" : mParentWeb.ServerRelativeUrl;
            var listId = mParentList == null ? Guid.Empty : mParentList.ID;
            var props=mRequest.UserCustomActionCollection_Add(mCollectionScope, webUrl, listId, location);
            var userCustomAction = CreateUserCustomActionByScope(props);
            mListData.Add(userCustomAction);
            return userCustomAction;  
        }

        public void Clear()
        {
            var webUrl = mParentWeb == null ? "" : mParentWeb.ServerRelativeUrl;
            var listId = mParentList == null ? Guid.Empty : mParentList.ID;
            mRequest.UserCustomActionCollection_Clear(mCollectionScope,webUrl,listId);
            mListData.Clear();
        }
        public IAveUserCustomAction GetById(Guid guid)
        {
            return mListData.Find(
                delegate(IAveUserCustomAction userCustomAction)
                {
                    return userCustomAction.Id.Equals(guid);
                });
        }

    }
}
