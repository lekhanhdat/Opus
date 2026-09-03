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
using Microsoft.SharePoint;
using AvePoint.Wrapper.Common;

namespace AvePoint.ObjectModel.Server19
{
    class AveRoleAssignmentCollection : AveAbstractCommonCollection<IAveRoleAssignment>, IAveRoleAssignmentCollection
    {
        private SPRoleAssignmentCollection mRoleAssignments;
        private AveWeb mWeb;
        private AveSite mSite;

        public AveRoleAssignmentCollection(AveWeb web, SPRoleAssignmentCollection roleAssignments)
            : base(roleAssignments)
        {
            mWeb = web;
            mSite = web.Site as AveSite;
            mRoleAssignments = roleAssignments;
        }

        public List<AveRoleAssignmentInfo> GetRoleAssignments(Guid siteId)
        {
            return mSite.QueryService.GetObjectRoleAssignments(siteId, ID);
        }

        public int GetRoleAssignmentCount(Guid scopeId, int roleId, int principalId)
        {
            return mSite.QueryService.GetRoleAssignmentCount(mWeb.Site.ID, scopeId, roleId, principalId);
        }

        #region IAveRoleAssignmentCollection Members
        //allowed to add limited access
        public void Add(IAveRoleAssignment roleAssignment)
        {
            //NonPublicAPIChange:
            //SP2013: private void AddInternal(SPRoleAssignment roleAssignment, bool addToCurrentScopeOnly, bool allowAddToLimitedAccess);
            //SP2016: private void AddInternal(SPRoleAssignment roleAssignment, bool addToCurrentScopeOnly, bool allowAddToLimitedAccess, bool propagateAcl, List<SPBasePermissions> requiredPermsForPropagation);

            AveAssemblyUtility.InvokeMethod(mRoleAssignments, "AddInternal",
                new Type[] { typeof(SPRoleAssignment), typeof(bool), typeof(bool), typeof(bool), typeof(List<SPBasePermissions>) },
                new object[] { (roleAssignment as AveRoleAssignment).RoleAssignment, false, true, false, null });
        }

        public IAveRoleAssignment Add(IAvePrincipal principal, IAveRoleDefinitionBindingCollection bindingCol)
        {
            SPRoleAssignment roleAssignment = new SPRoleAssignment((principal as AvePrincipal).Principal);
            roleAssignment.ImportRoleDefinitionBindings((bindingCol as AveRoleDefinitionBindingCollection).RoleDefinitionBindingCollection);
            mRoleAssignments.Add(roleAssignment);
            SPRoleAssignment newRoleAssingment = mRoleAssignments.GetAssignmentByPrincipal((principal as AvePrincipal).Principal);
            if (newRoleAssingment == null)
            {
                return null;
            }
            return new AveRoleAssignment(mWeb, newRoleAssingment);
        }

        public IAveRoleAssignment GetByPrincipal(IAvePrincipal principalToFind)
        {
            SPRoleAssignment roleAssignment = mRoleAssignments.GetAssignmentByPrincipal((principalToFind as AvePrincipal).Principal);
            if (roleAssignment == null)
            {
                return null;
            }
            return new AveRoleAssignment(mWeb, roleAssignment);
        }

        public IAveRoleAssignment GetByPrincipalId(int principalId)
        {
            foreach (SPRoleAssignment roleAssignment in mRoleAssignments)
            {
                if (roleAssignment.Member.ID == principalId)
                {
                    return new AveRoleAssignment(mWeb, roleAssignment);
                }
            }
            return null;
        }

        public Guid ID
        {
            get { return mRoleAssignments.Id; }
        }

        public IAveRoleAssignment GetAssignmentByPrincipal(IAvePrincipal principal)
        {
            SPRoleAssignment roleAssignment = mRoleAssignments.GetAssignmentByPrincipal((principal as AvePrincipal).Principal);
            if (roleAssignment == null)
            {
                return null;
            }
            return new AveRoleAssignment((principal as AvePrincipal), roleAssignment);
        }

        public void Remove(int index)
        {
            mRoleAssignments.Remove(index);
        }

        public void RemoveById(int Id)
        {
            mRoleAssignments.RemoveById(Id);
        }

        public override IAveRoleAssignment this[int index]
        {
            get
            {
                return new AveRoleAssignment(mWeb, mRoleAssignments[index]);
            }
        }

        public void Remove(IAvePrincipal member)
        {
            mRoleAssignments.Remove((member as AvePrincipal).Principal);
        }

        public void RemoveFromCurrentScopeOnly(IAvePrincipal member)
        {
            mRoleAssignments.RemoveFromCurrentScopeOnly((member as AvePrincipal).Principal);
        }

        #endregion

        protected override object CreatElementInstance(object t)
        {
            return new AveRoleAssignment(mWeb, t as SPRoleAssignment);
        }

        public override int Count
        {
            get { return mRoleAssignments.Count; }
        }

        public IAveRoleAssignment CreateRoleAssignment(IAvePrincipal principal)
        {
            return new AveRoleAssignment(principal);//给外界提供方法可以创建RoleAssignment
        }

        public IAveGroupCollection Groups
        {
            get { return new AveGroupCollection(mWeb, mRoleAssignments.Groups); }
        }
    }
}
