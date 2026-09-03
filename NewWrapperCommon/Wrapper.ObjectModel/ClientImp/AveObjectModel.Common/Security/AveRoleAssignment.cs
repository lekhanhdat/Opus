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
    class AveRoleAssignment : AveClientObject, IAveRoleAssignment
    {
        private AveSite mSite;
        private AveWeb mWeb;
        private AveList mList;
        private int mItemId;
        private IAveRequest mRequest;
        private AveRoleAssignmentCollection mRoleAssignmentCollection;
        private bool mIsNewCreated;
        private string mSource;

        public AveRoleAssignment(AveRoleAssignmentCollection roleAssignmentCollection, IAveRequest request, AveSite site, AveWeb web, AveList list, int itemId, string source, Dictionary<string, object> roleAssignmentProperties)
        {
            mSite = site;
            mWeb = web;
            mList = list;
            mItemId = itemId;
            mRoleAssignmentCollection = roleAssignmentCollection;
            mRequest = request;
            mSource = source;
            base.DataCache.AddPropertyies(roleAssignmentProperties);
        }

        public AveRoleAssignment(AvePrincipal pricipal, AveSite site, AveWeb web, AveList list, int itemId, string source)
        {
            mSite = site;
            mWeb = web;
            mList = list;
            mItemId = itemId;
            mRequest = site.Request;
            mSource = source;
            mIsNewCreated = true;
            base.DataCache.PropertiesCache["Member"] = pricipal;
        }

        public AveRoleAssignment(AvePrincipal pricipal)
        {
            mIsNewCreated = true;
            base.DataCache.PropertiesCache["Member"] = pricipal;
        }

        internal AveRoleAssignmentCollection RoleAssignmentCollection
        {
            set { mRoleAssignmentCollection = value; }
        }

        internal void InitRoleDefinitionBeforeAddOrUpdate()
        {
            List<string> roleDefinitionNameSet = new List<string>();
            foreach (AveRoleDefinition roleDefintion in this.RoleDefinitionBindings)
            {
                roleDefinitionNameSet.Add(roleDefintion.Name);
            }
            base.DataCache.AddChangedProperty("MemberId", this.Member.ID);
            base.DataCache.AddChangedProperty("RoleDefinitionBindingCollection", roleDefinitionNameSet);
            base.DataCache.AddChangedProperty("MemberLoginName", this.Member.LoginName);
            base.DataCache.AddChangedProperty("MemberType", this.Member is IAveGroup ? "Group" : "User");
            base.DataCache.AddChangedProperty("IsNewCreated", mIsNewCreated);
            if (mWeb != null)
            {
                base.DataCache.AddChangedProperty(AveObjectModelConstant.WebServerRelativeUrl, mWeb.ServerRelativeUrl);
            }
            if (mList != null)
            {
                base.DataCache.AddChangedProperty(AveObjectModelConstant.ListTitle, mList.Title);
            }
            if (mItemId > 0)
            {
                base.DataCache.AddChangedProperty(AveObjectModelConstant.ItemId, mItemId);
            }
        }

        #region IAveRoleAssignment Members

        public IAvePrincipal Member
        {
            get
            {
                if (base.DataCache.IsPropertyNotLoaded("Member"))
                {
                    string loginName = base.DataCache.GetProperty<string>("MemberLoginName");
                    string memberType = base.DataCache.GetProperty<string>("MemberType");
                    AvePrincipal member = null;
                    switch (memberType)
                    {
                        case "Group":
                            member = mSite.RootWeb.SiteGroups[loginName] as AveGroup;
                            break;
                        case "User":
                            member = mSite.RootWeb.SiteUsers.GetByLoginName(loginName) as AveUser;
                            break;
                        default:
                            member = mSite.RootWeb.SiteUsers.GetByLoginName(loginName) as AveUser;
                            break;
                    }
                    base.DataCache.PropertiesCache["Member"] = member;
                    return member;
                }
                return base.DataCache.GetProperty<IAvePrincipal>("Member");
            }
        }

        public IAveRoleDefinitionBindingCollection RoleDefinitionBindings
        {
            get
            {
                if (base.DataCache.IsPropertyNotLoaded("RoleDefinitionBindings"))
                {
                    AveRoleDefinitionBindingCollection roleDefinitionBindingCol = null;
                    if (mIsNewCreated)
                    {
                        roleDefinitionBindingCol = new AveRoleDefinitionBindingCollection(this, mWeb, mRequest);
                    }
                    else
                    {
                        Dictionary<string, object> roleDefinitionBindingColProperites = base.DataCache.GetProperty<Dictionary<string, object>>("RoleDefinitionBindings" + AveObjectModelConstant.ObjectPropertySuffix);
                        roleDefinitionBindingCol = new AveRoleDefinitionBindingCollection(this, mRequest, mWeb, roleDefinitionBindingColProperites);
                    }
                    base.DataCache.PropertiesCache["RoleDefinitionBindings"] = roleDefinitionBindingCol;
                    return roleDefinitionBindingCol;
                }
                return base.DataCache.GetProperty<IAveRoleDefinitionBindingCollection>("RoleDefinitionBindings");
            }
        }

        public void ImportRoleDefinitionBindings(IAveRoleDefinitionBindingCollection roleDefinitionBindings)
        {
            base.DataCache.PropertiesCache["RoleDefinitionBindings"] = roleDefinitionBindings;
        }

        public void DeleteObject()
        {
            this.mRequest.DeleteRoleAssignment(mWeb.ServerRelativeUrl, mList.DefaultViewUrl, mList.Title, mList.ID, mItemId, this.Member.ID, mSource);
        }

        public void Update()
        {
            if (mRoleAssignmentCollection == null)
            {
                throw new AveArgumentException(" Cannot update a permission level assignment that is not part of a permission level assignment collection.");
            }
            InitRoleDefinitionBeforeAddOrUpdate();
            Dictionary<string, object> roleAssignmentProperties = this.mRoleAssignmentCollection.SecurableObject.UpdateRoleAssignment(this.Member.ID, base.DataCache.ChangedProperties);
            base.DataCache.UpdateProperties(roleAssignmentProperties);
        }
        #endregion
    }
}
