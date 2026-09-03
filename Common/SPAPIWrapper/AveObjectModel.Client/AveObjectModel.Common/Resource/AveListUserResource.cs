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
using System.Collections.Generic;

namespace AvePoint.ObjectModel.Common
{
    class AveListUserResource : AveUserResource
    {
        private string mResourceName;
        private AveList mList;
        private IAveRequest mRequest;
        private Dictionary<string, string> mKeyValues;
        private AveClientObjectData mDataCache;

        public AveListUserResource(AveList list, string resourceName, AveClientObjectData dataCache)
        {
            // TODO: Complete member initialization
            this.mList = list;
            this.mRequest = list.Request;
            this.mResourceName = resourceName;
            this.mDataCache = dataCache;

            this.mKeyValues = dataCache.GetProperty<Dictionary<string, string>>(resourceName);
            if (mKeyValues != null)
            {
                base.keyValues = mKeyValues;
            }
        }

        protected override string GetValueForUICultureWithRequest(string cultureName)
        {
            return mRequest.GetListUserResource(mList.ParentWeb.ServerRelativeUrl, mList.ID, mResourceName, cultureName);
        }

        protected override void InternalUpdate(Dictionary<string, string> changedResource)
        {
            this.mDataCache.AddChangedProperty(mResourceName, changedResource);
            //mRequest.SetListUserResource(mList.ParentWeb.ServerRelativeUrl, mList.ID, mResourceName, changedTitle);
        }
    }
}
