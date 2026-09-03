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
    class AveListItemVersion : AveClientObject, IAveListItemVersion
    {
        private AveListItemVersionCollection mListItemVerCollection;
        private IAveRequest mRequest;        

        public AveListItemVersion(AveListItemVersionCollection listItemVerCollection, IAveRequest request, IDictionary<string, object> listItemVerProperites)
        {
            mRequest = request;
            mListItemVerCollection = listItemVerCollection;
            base.DataCache.AddPropertyies(listItemVerProperites);
        }

        #region IAveListItemVersion Members

        public DateTime Created
        {
            get 
            { 
                return base.DataCache.GetProperty<DateTime>("Created");
            }
        }

        public IAveFieldUserValue CreatedBy
        {
            get 
            {
                if (base.DataCache.IsPropertyNotLoaded("CreatedBy"))
                {
                    string loginName = base.DataCache.GetProperty<string>("CreatedBy" + AveObjectModelConstant.ObjectPropertySuffix);
                    AveUser createdBy = mListItemVerCollection.ListItem.Web.SiteUsers.GetByLoginName(loginName) as AveUser;
                    AveFieldUserValue fieldUserValue = new AveFieldUserValue(createdBy.ID);
                    base.DataCache.AddProperty("CreatedBy",fieldUserValue);
                    return fieldUserValue;
                }
                return base.DataCache.GetProperty<IAveFieldUserValue>("CreatedBy");
            }
        }

        public IAveFieldCollection Fields
        {
            get 
            { 
                return mListItemVerCollection.ListItem.Fields; 
            }
        }

        public bool IsCurrentVersion
        {
            get 
            { 
                return base.DataCache.GetProperty<bool>("IsCurrentVersion"); 
            }
        }

        public object this[string fieldName]
        {
            get 
            {
                Dictionary<string, object> fieldValues = base.DataCache.GetProperty<Dictionary<string, object>>("FieldValues");                
                object fieldValue = null;
                fieldValues.TryGetValue(fieldName, out fieldValue);
                return fieldValue;
            }
        }

        public object this[int index]
        {
            get 
            { 
                return this[this.Fields[index].InternalName];
            }
        }

        public AveFileLevel Level
        {
            get
            { 
                return base.DataCache.GetProperty<AveFileLevel>("Level"); 
            }
        }

        public IAveListItem ListItem
        {
            get 
            { 
                return mListItemVerCollection.ListItem; 
            }
        }

        public string Url
        {
            get 
            { 
                return base.DataCache.GetProperty<string>("Url"); 
            }
        }

        public int VersionId
        {
            get
            { 
                return base.DataCache.GetProperty<int>("VersionId"); 
            }
        }

        public string VersionLabel
        {
            get 
            { 
                return base.DataCache.GetProperty<string>("VersionLabel"); 
            }
        }

        public long Length
        {
            get
            {
                return long.Parse(base.DataCache.GetProperty<object>("Length").ToString());
            }
        }

        public void Delete()
        {
            IAveList parentList = this.ListItem.ParentList;
            mRequest.DeleteItemVersion(parentList.ParentWeb.ServerRelativeUrl, parentList.DefaultViewUrl, parentList.Title, parentList.ID, this.ListItem.ID, this.VersionId);
            mListItemVerCollection.ListData.Remove(this);
        }
        #endregion
    }
}
