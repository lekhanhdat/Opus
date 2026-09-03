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
    class AveRoleAssignmentCollection : AveAbstractCommonCollection<IAveRoleAssignment>, IAveRoleAssignmentCollection
    {
        private AveSite mSite;
        private AveWeb mWeb;
        private AveList mList;
        private int mItemId;
        private IAveRequest mRequest;
        private AveSecurableObject mSecuableObject;
        private string mSource;

        public AveRoleAssignmentCollection(AveSecurableObject securableObject, IAveRequest request, AveSite site, AveWeb web, AveList list, int itemId, string source, Dictionary<string, object> roleAssignmentColProperties)
        {
            mSecuableObject = securableObject;
            mSite = site;
            mWeb = web;
            mList = list;
            mItemId = itemId;
            mRequest = request;
            mSource = source;
            base.DataCache.AddPropertyies(roleAssignmentColProperties);
            InitRoleAssignmentCollection();
        }

        internal void InitRoleAssignmentCollection()
        {
            List<Dictionary<string, object>> roleAssignmentPropertiesList = base.DataCache.GetChildren();
            mListData = new List<IAveRoleAssignment>(roleAssignmentPropertiesList.Count);
            foreach (Dictionary<string, object> roleAssignmentProperties in roleAssignmentPropertiesList)
            {
                AveRoleAssignment roleAssignment = new AveRoleAssignment(this, mRequest, mSite, mWeb, mList, mItemId, mSource, roleAssignmentProperties);
                mSecuableObject.InitRoleAssignmentProperties(roleAssignment.DataCache.ChangedProperties);
                mListData.Add(roleAssignment);
            }
        }

        internal AveSecurableObject SecurableObject
        {
            get
            {
                return mSecuableObject;
            }
        }

        #region IAveRoleAssignmentCollection Members

        public Guid ID
        {
            get { return base.DataCache.GetProperty<Guid>("Id"); }
        }

        public void Add(IAveRoleAssignment roleAssignment)
        {
            AveRoleAssignment aveRoleAssignment = roleAssignment as AveRoleAssignment;
            aveRoleAssignment.RoleAssignmentCollection = this;
            aveRoleAssignment.InitRoleDefinitionBeforeAddOrUpdate();
            Dictionary<string, object> newRoleAssignmentProperties = mSecuableObject.AddRoleAssignment(aveRoleAssignment.DataCache.ChangedProperties);
            aveRoleAssignment.DataCache.UpdateProperties(newRoleAssignmentProperties);
            mListData.Add(roleAssignment);
        }

        public IAveRoleAssignment Add(IAvePrincipal principal, IAveRoleDefinitionBindingCollection bindingCol)
        {
            AveRoleAssignment roleAssigment = new AveRoleAssignment(principal as AvePrincipal, mSite, mWeb, mList, mItemId, mSource);
            roleAssigment.ImportRoleDefinitionBindings(bindingCol);
            this.Add(roleAssigment);
            return roleAssigment;
        }

        public IAveRoleAssignment GetAssignmentByPrincipal(IAvePrincipal principal)
        {
            return mListData.Find(r => r.Member != null ? r.Member.ID == principal.ID : false);
        }

        public IAveRoleAssignment GetByPrincipalId(int principalId)
        {
            return mListData.Find(r => r.Member != null ? r.Member.ID == principalId : false);
        }

        public List<AveRoleAssignmentInfo> GetRoleAssignments(Guid siteId)
        {
            List<AveRoleAssignmentInfo> roleAssignmentsInfo = null;
            if (this.Count > 0)
            {
                roleAssignmentsInfo = new List<AveRoleAssignmentInfo>(this.Count);
            }
            foreach (IAveRoleAssignment roleAssignment in mListData)
            {
                IAveRoleDefinitionBindingCollection roleDefinitonBindingCol = roleAssignment.RoleDefinitionBindings;
                foreach (IAveRoleDefinition roleDef in roleDefinitonBindingCol)
                {
                    AveRoleAssignmentInfo roleAssignmentInfo = new AveRoleAssignmentInfo();
                    IAvePrincipal member = roleAssignment.Member;
                    if (member != null)
                    {
                        roleAssignmentInfo.PrincipalId = member.ID;
                    }
                    roleAssignmentInfo.RoleId = roleDef.ID;
                    roleAssignmentsInfo.Add(roleAssignmentInfo);
                }
            }
            return roleAssignmentsInfo;
        }

        public int GetRoleAssignmentCount(Guid scopeId, int roleId, int principalId)
        {
            int count = 0;
            IAveRoleAssignment roleAssignment = this.GetByPrincipalId(principalId);

            if (roleAssignment != null && roleAssignment.Member.ID == principalId)
            {
                foreach (IAveRoleDefinition roleDefinition in roleAssignment.RoleDefinitionBindings)
                {
                    if (roleDefinition.ID == roleId)
                    {
                        count++;
                    }
                }
            }

            return count;
        }

        public void Remove(int index)
        {
            this.RemoveById(this[index].Member.ID);
        }

        public void RemoveById(int Id)
        {
            mSecuableObject.RemoveRoleAssignment(Id);
            mListData.Remove(GetByPrincipalId(Id));
        }

        public void Remove(IAvePrincipal member)
        {
            RemoveById(member.ID);
        }

        public void RemoveFromCurrentScopeOnly(IAvePrincipal member)
        {
            //office365不支持RemoveFromCurrentScopeOnly方法，暂时用RemoveAll代替,但是这种方法无法删除Limited Access
            IAveRoleAssignment roleAssignment = GetByPrincipalId(member.ID);
            if (roleAssignment != null)
            {
                roleAssignment.RoleDefinitionBindings.RemoveAll();
                roleAssignment.Update();
                mListData.Remove(roleAssignment);
            }
        }

        public IAveRoleAssignment CreateRoleAssignment(IAvePrincipal principal)
        {
            return new AveRoleAssignment(principal as AvePrincipal);//给外界提供方法可以创建RoleAssignment
        }

        #endregion

        public IAveGroupCollection Groups
        {
            get
            {
                throw new NotImplementedException();
            }
        }
    }
}
