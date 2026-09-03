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

namespace AvePoint.ObjectModel.Common
{
    class AveFieldUserResource : AveUserResource
    {
        private IAveRequest mRequest;
        private string mWebServerRelativeUrl;
        private string mFieldSource;
        private string mResourceName;
        private Guid mListId;
        private string mListTitle;
        private AveClientObjectData mDataCache;

        private Dictionary<string, string> mKeyValues;
        private IDictionary<string, object> mContentTypeProp;
        private Dictionary<string, object> mFieldProp = new Dictionary<string, object>();

        public AveFieldUserResource(IAveRequest request, string webServerRelativeUrl, AveList aveList, string fieldSource, string resourceName, IDictionary<string, object> contentProp, AveClientObjectData dataCache)
        {
            // TODO: Complete member initialization
            this.mRequest = request;
            this.mWebServerRelativeUrl = webServerRelativeUrl;
            this.mFieldSource = fieldSource;
            this.mResourceName = resourceName;
            this.mContentTypeProp = contentProp;
            this.mDataCache = dataCache;

            this.mListId = aveList == null ? Guid.Empty : aveList.ID;
            this.mListTitle = aveList == null ? string.Empty : aveList.Title;

            this.mFieldProp["ObjectPath"] = dataCache.GetProperty<object>("ObjectPath");
            this.mFieldProp["FieldType"] = dataCache.GetProperty<object>("FieldType");

            this.mKeyValues = dataCache.GetProperty<Dictionary<string, string>>(mResourceName);
            if (this.mKeyValues != null)
            {
                base.keyValues = this.mKeyValues;
            }
        }

        protected override string GetValueForUICultureWithRequest(string cultureName)
        {
            return mRequest.GetFieldUserResource(mWebServerRelativeUrl, mListId, mListTitle, mFieldSource, mResourceName, mContentTypeProp, mFieldProp, cultureName);
        }

        protected override void InternalUpdate(Dictionary<string, string> changedTitle)
        {
            this.mDataCache.AddChangedProperty(this.mResourceName, changedTitle);
        }
    }
}
