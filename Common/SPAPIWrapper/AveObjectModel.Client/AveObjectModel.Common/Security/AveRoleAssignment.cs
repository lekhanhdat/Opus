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

        private AvePrincipal member;
        private int principalId;
        private AveRoleDefinitionBindingCollection bindingCollection;

        public AveRoleAssignment(AveRoleAssignmentCollection roleAssignmentCollection,IAveRequest request,  AveSite site, AveWeb web, AveList list, int itemId, string source, IDictionary<string, object> roleAssignmentProperties)
        {
            mSite = site;
            mWeb = web;
            mList = list;
            mItemId = itemId;
            mRoleAssignmentCollection = roleAssignmentCollection;
            mRequest = request;
            mSource = source;
            InitRoleAssignment(roleAssignmentProperties);
            //base.DataCache.AddPropertyies(roleAssignmentProperties);
        }

        private void InitRoleAssignment(IDictionary<string, object> roleAssignmentProperties)
        {
            principalId = (int)roleAssignmentProperties["PrincipalId"];
            var bindingLists = (List<int>)roleAssignmentProperties["RoleDefinitionBindings" + AveObjectModelConstant.ObjectPropertySuffix];
            RemoveUnusableDefinitionBindings(bindingLists);
            bindingCollection = new AveRoleDefinitionBindingCollection(mWeb, bindingLists);
        }

        private void RemoveUnusableDefinitionBindings(List<int> bindingLists)
        {
            for (int i = bindingLists.Count - 1; i >= 0; i--)
            {
                if (mWeb.RoleDefinitions.GetById(bindingLists[i]) == null)
                {
                    mLogger.Warn("Get web definition failed,roledefinition id {0}", bindingLists[i]);
                    bindingLists.Remove(bindingLists[i]);
                }
            }
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
            member = pricipal;
            principalId = pricipal.ID;
            //base.DataCache.PropertiesCache["Member"] = pricipal;
        }

        internal void Init(AveRoleAssignmentCollection roleAssignmentCollection, IAveRequest request, AveSite site, AveWeb web, AveList list, int itemId, string source, bool isNewCreated)
        {
            mSite = site;
            mWeb = web;
            mList = list;
            mItemId = itemId;
            mRoleAssignmentCollection = roleAssignmentCollection;
            mRequest = request;
            mSource = source;
            mIsNewCreated = isNewCreated;
        }

        public AveRoleAssignment(AvePrincipal pricipal)
        {            
            mIsNewCreated = true;
            member = pricipal;
            principalId = pricipal.ID;
            //base.DataCache.PropertiesCache["Member"] = pricipal;
        }

        internal Dictionary<string,object> InitChanges()
        {
            var changes = new Dictionary<string, object> { };
            List<int> roleDefinitionIdSet = new List<int>();
            if (bindingCollection != null)
            {
                bindingCollection.CopyTo(roleDefinitionIdSet);
            }
            changes.Add("MemberId", this.principalId);
            changes.Add("RoleDefinitionBindingCollection", roleDefinitionIdSet);
            changes.Add("MemberLoginName", this.Member.LoginName);
            changes.Add("MemberType", this.Member is IAveGroup ? "Group" : "User");
            changes.Add("IsNewCreated", mIsNewCreated);
            if (mWeb != null)
            {
                changes.Add(AveObjectModelConstant.WebServerRelativeUrl, mWeb.ServerRelativeUrl);
            }
            if (mList != null)
            {
                changes.Add(AveObjectModelConstant.ListTitle, mList.Title);
                changes.Add(AveObjectModelConstant.ListId, mList.ID);
            }
            if (mItemId > 0)
            {
                changes.Add(AveObjectModelConstant.ItemId, mItemId);
            }
            return changes;
        }

        #region IAveRoleAssignment Members

        public IAvePrincipal Member
        {
            get
            {
                if (member == null)
                {
                    if (principalId > 0)
                    {
                        var user = mSite.RootWeb.SiteUsers.GetByID(principalId);
                        if (user != null)
                        {
                            member = user as AveUser;
                        }
                        else
                        {
                            var group = mSite.RootWeb.SiteGroups.GetByID(principalId);
                            if (group != null)
                            {
                                member = group as AveGroup;
                            }
                            else
                            {
                                throw new Exception(string.Format("Cannot find the principal with id:{0}", principalId));
                            }
                        }
                    }
                    else
                    {
                        throw new Exception(string.Format("principal id:{0} is invalid.", principalId));
                    }
                }
                return member;
            }
        }

        public IAveRoleDefinitionBindingCollection RoleDefinitionBindings
        {
            get 
            {
                if (bindingCollection == null)
                {
                    bindingCollection = new AveRoleDefinitionBindingCollection(mWeb);
                }

                return bindingCollection;
                //if (base.DataCache.IsPropertyNotLoaded("RoleDefinitionBindings"))
                //{
                //    AveRoleDefinitionBindingCollection roleDefinitionBindingCol = null;
                //    if (mIsNewCreated)
                //    {
                //        roleDefinitionBindingCol = new AveRoleDefinitionBindingCollection(this, mWeb, mRequest);
                //    }
                //    else
                //    {
                //        var bindings = base.DataCache.GetProperty<List<int>>("RoleDefinitionBindings" + AveObjectModelConstant.ObjectPropertySuffix);
                //        roleDefinitionBindingCol = new AveRoleDefinitionBindingCollection(this, mRequest, mWeb, bindings);
                //    } 
                //    base.DataCache.PropertiesCache["RoleDefinitionBindings"] = roleDefinitionBindingCol;
                //    return roleDefinitionBindingCol;
                //}
                //return base.DataCache.GetProperty<IAveRoleDefinitionBindingCollection>("RoleDefinitionBindings");
            }
        }

        public int PrincipalId
        {
            get
            {
                return principalId;
            }
        }

        public void ImportRoleDefinitionBindings(IAveRoleDefinitionBindingCollection roleDefinitionBindings)
        {
            bindingCollection = roleDefinitionBindings as AveRoleDefinitionBindingCollection;
            //base.DataCache.PropertiesCache["RoleDefinitionBindings"] = roleDefinitionBindings;
        }

        public void DeleteObject()
        {
            //if (principalId > 0)
            //{
            //    this.mRequest.DeleteRoleAssignment(mWeb.ServerRelativeUrl, mList.DefaultViewUrl, mList.Title, mList.ID, mItemId, principalId, mSource);
            //}
            //else
            //{
            //    this.mRequest.DeleteRoleAssignment(mWeb.ServerRelativeUrl, mList.DefaultViewUrl, mList.Title, mList.ID, mItemId, this.Member.ID, mSource);
            //}
            if(mRoleAssignmentCollection != null)
            {
                mRoleAssignmentCollection.RemoveById(principalId);
            }
            else
            {
                throw new Exception("Cannot delete role assignment without parent.");
            }
        }

        public void Update()
        {
            var changes=InitChanges();
            if (mIsNewCreated)
            {
                var roleAssignmentProperties = this.mRoleAssignmentCollection.SecurableObject.AddRoleAssignment(changes);
                InitRoleAssignment(roleAssignmentProperties);
            }
            else
            {
                var roleAssignmentProperties = this.mRoleAssignmentCollection.SecurableObject.UpdateRoleAssignment(this.Member.ID, changes);
                //base.DataCache.UpdateProperties(roleAssignmentProperties);
                //SAAS-28957,先从缓存中清除，让其进入add操作.
                if (roleAssignmentProperties.ContainsKey("RoleDefinitionBindings" + AveObjectModelConstant.ObjectPropertySuffix)
                    && (roleAssignmentProperties["RoleDefinitionBindings" + AveObjectModelConstant.ObjectPropertySuffix] as List<int>).Count == 0)
                {
                    this.mRoleAssignmentCollection.ListData.Remove(this);
                }
                InitRoleAssignment(roleAssignmentProperties);
            }
        }
        #endregion
    }
}
