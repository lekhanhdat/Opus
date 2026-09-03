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
    class AveRoleDefinition : AveClientObject, IAveRoleDefinition
    {
        private AveWeb mWeb;
        private IAveRequest mRequest;
        private AveRoleDefinitionCollection mRoleDefinitionCollection;
        private AveRoleDefinitionBindingCollection mRoleDefinitionBindingCollection;        

        public AveRoleDefinition(IAveRequest request, AveRoleDefinitionCollection roleDefinitionCollection)
        {
            mRequest = request;
            mRoleDefinitionCollection = roleDefinitionCollection;
            base.DataCache.ChangedProperties["Id"] = -1;         
        }

        public AveRoleDefinition(IAveRequest request, AveRoleDefinitionCollection roleDefinitionCollection, Dictionary<string, object> roleDefinitionProperties)
        {
            mRequest = request;
            mRoleDefinitionCollection = roleDefinitionCollection;
            base.DataCache.AddPropertyies(roleDefinitionProperties);
        }

        public AveRoleDefinition(IAveRequest request, AveRoleDefinitionBindingCollection roleDefinitionBindingCollection, Dictionary<string, object> roleDefinitionProperties)
        {
            mRequest = request;
            mRoleDefinitionBindingCollection = roleDefinitionBindingCollection;
            base.DataCache.AddPropertyies(roleDefinitionProperties);
        }

        public AveRoleDefinition(IAveRequest request, AveRoleDefinitionCollection roleDefinitionCollection, IAveWeb parentWeb, Dictionary<string, object> roleDefinitionProperties)
        {
            mRequest = request;
            if (parentWeb != null)
            {
                mWeb = parentWeb as AveWeb;
            }
            mRoleDefinitionCollection = roleDefinitionCollection;
            base.DataCache.AddPropertyies(roleDefinitionProperties);
        }

        public AveRoleDefinition(IAveRequest request, AveRoleDefinitionBindingCollection roleDefinitionBindingCollection, IAveWeb parentWeb, Dictionary<string, object> roleDefinitionProperties)
        {
            mRequest = request;
            if (parentWeb != null)
            {
                mWeb = parentWeb as AveWeb;
            }
            mRoleDefinitionBindingCollection = roleDefinitionBindingCollection;
            base.DataCache.AddPropertyies(roleDefinitionProperties);
        }

        public AveRoleDefinition()
        {            
            base.DataCache.ChangedProperties["Id"] = -1;            
        }

        public AveRoleDefinition(AveRoleDefinition roleDefinition)
        {            
            base.DataCache.AddChangedProperties(roleDefinition.DataCache.PropertiesCache);
        }

        internal AveWeb Web
        {
            set
            {
                mWeb = value;
                mRequest = (mWeb.Site as AveSite).Request;
                base.DataCache.PropertiesCache[AveObjectModelConstant.WebServerRelativeUrl] = mWeb.ServerRelativeUrl;
            }
        }

        internal AveRoleDefinitionCollection RoleDefinitionCollection
        {
            get
            {
                return mRoleDefinitionCollection;
            }
        }

        #region IAveRoleDefinition Members

        public void DeleteObject()
        {
            mRequest.DeleteRoleDefinition(base.DataCache.GetProperty<string>(AveObjectModelConstant.WebServerRelativeUrl), this.Name);
            mRoleDefinitionCollection.ListData.Remove(this);
        }

        public void Update()
        {
            if (base.DataCache.ChangedProperties.Count > 0)
            {
                Dictionary<string, object> newRoleDefintionProperties = mRequest.UpdateRoleDefinition(base.DataCache.GetProperty<string>(AveObjectModelConstant.WebServerRelativeUrl), this.ID, base.DataCache.ChangedProperties);
                base.DataCache.UpdateProperties(newRoleDefintionProperties);
            }
        }

        public AveBasePermissions BasePermissions
        {
            get
            {
                return base.DataCache.GetProperty<AveBasePermissions>("BasePermissions");
            }
            set
            {
                base.DataCache.AddChangedProperty("BasePermissions", (ulong)value);
            }
        }

        public string Description
        {
            get
            {
                return base.DataCache.GetProperty<string>("Description");
            }
            set
            {
                base.DataCache.AddChangedProperty("Description", value);
            }
        }

        public bool Hidden
        {
            get { return base.DataCache.GetProperty<bool>("Hidden"); }
        }

        public int ID
        {
            get { return base.DataCache.GetProperty<int>("Id"); }
        }

        public string Name
        {
            get
            {
                return base.DataCache.GetProperty<string>("Name");
            }
            set
            {
                base.DataCache.AddChangedProperty("Name", value);
            }
        }

        public int Order
        {
            get
            {
                return base.DataCache.GetProperty<int>("Order");
            }
            set
            {
                base.DataCache.AddChangedProperty("Order", value);
            }
        }

        public AveRoleType Type
        {
            get { return base.DataCache.GetProperty<AveRoleType>("Type"); }
        }

        #endregion


        public IAveWeb ParentWeb
        {
            get { return mWeb; }
        }

        /// <summary>
        /// client API没有该属性，默认返回空
        /// </summary>
        public string NameInternal
        {
            get { return string.Empty; }
        }
    }
}
