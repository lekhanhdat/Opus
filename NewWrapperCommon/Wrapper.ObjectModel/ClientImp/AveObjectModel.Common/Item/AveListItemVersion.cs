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
using AvePoint.Wrapper.Resource.Client;

namespace AvePoint.ObjectModel.Common
{
    class AveListItemVersion : AveClientObject, IAveListItemVersion
    {
        private AveListItemVersionCollection mListItemVerCollection;
        private IAveRequest mRequest;        

        public AveListItemVersion(AveListItemVersionCollection listItemVerCollection, IAveRequest request, Dictionary<string, object> listItemVerProperites)
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
                    AveUser createdBy = null;
                    if (base.DataCache.IsPropertyAvailable("CreatedBy" + AveObjectModelConstant.ObjectPropertySuffix))
                    {
                        string loginName = base.DataCache.GetProperty<string>("CreatedBy" + AveObjectModelConstant.ObjectPropertySuffix);
                        createdBy = mListItemVerCollection.ListItem.Web.SiteUsers.GetByLoginName(loginName) as AveUser;
                    }
                    else if (base.DataCache.IsPropertyAvailable("Author"))
                    {
                        string authorName = base.DataCache.GetProperty<string>("Author");

                        string[] array = authorName.Split(new string[] { ";#" }, StringSplitOptions.RemoveEmptyEntries);
                        if (array.Length > 0)
                        {
                            int userId = -1;
                            if (Int32.TryParse(array[0], out userId))
                            {
                                createdBy = mListItemVerCollection.ListItem.Web.SiteUsers.GetByID(userId) as AveUser;
                            }
                        }
                    }

                    AveFieldUserValue fieldUserValue = new AveFieldUserValue(mListItemVerCollection.ListItem.Web, createdBy.ID, createdBy.LoginName);
                    base.DataCache.PropertiesCache["CreatedBy"] = fieldUserValue;
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
        
        public void Delete()
        {
            if (this.IsCurrentVersion)
            {
                throw new AveWrapperBaseException(AveInternalResourceKey.Wrapper_Exception_Office365_Common_DeleteItemVersionFailed);
            }
            IAveList parentList = this.ListItem.ParentList;
            mRequest.DeleteItemVersion(parentList.ParentWeb.ServerRelativeUrl, parentList.RootFolder.ServerRelativeUrl, parentList.Title.ToString(), parentList.ID, this.ListItem.ID, this.VersionId);
            mListItemVerCollection.ListData.Remove(this);
        }

        public void Recycle()
        {
            //this.Delete();
        }
        #endregion
    }
}
