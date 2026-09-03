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



using Microsoft.SharePoint;
using AvePoint.Wrapper.Common;

namespace AvePoint.ObjectModel.Server16
{
    class AveRoleAssignment : AveServerObject, IAveRoleAssignment
    {
        private SPRoleAssignment mRoleAssignment;
        private AvePrincipal mPrincipal;
        private AveRoleDefinitionBindingCollection mRoleDefinitionBindings;
        private AveWeb mWeb;

        public AveRoleAssignment(AveWeb web, SPRoleAssignment roleAssignment)
        {
            mWeb = web;
            object principal = roleAssignment.Member;
            mPrincipal = (AvePrincipal)AveServerAssemblyInit.CreateElement(typeof(IAvePrincipal), new object[] { mWeb, principal });
            mRoleAssignment = roleAssignment;
        }

        public AveRoleAssignment(IAvePrincipal principal)
        {
            mWeb = principal.ParentWeb as AveWeb;
            mRoleAssignment = new SPRoleAssignment((principal as AvePrincipal).Principal);
            mPrincipal = principal as AvePrincipal;
        }

        public AveRoleAssignment(AvePrincipal principal, SPRoleAssignment roleAssignment)
        {
            mWeb = principal.ParentWeb as AveWeb;
            mPrincipal = principal;
            mRoleAssignment = roleAssignment;
        }

        internal SPRoleAssignment RoleAssignment
        {
            get
            {
                return mRoleAssignment;
            }
        }

        #region IAveRoleAssignment Members

        public IAvePrincipal Member
        {
            get
            {
                if (mPrincipal == null)
                {
                    SPPrincipal member = mRoleAssignment.Member;
                    if (member != null)
                    {
                        mPrincipal = AvePrincipal.InitPrincipal(mWeb, member);
                    }
                }
                return mPrincipal;
            }
        }

        public IAveRoleDefinitionBindingCollection RoleDefinitionBindings
        {
            get
            {
                if (mRoleDefinitionBindings == null)
                {
                    mRoleDefinitionBindings = new AveRoleDefinitionBindingCollection(mWeb, mRoleAssignment.RoleDefinitionBindings);
                }
                return mRoleDefinitionBindings;
            }
        }

        public void ImportRoleDefinitionBindings(IAveRoleDefinitionBindingCollection roleDefinitionBindings)
        {
            mRoleAssignment.ImportRoleDefinitionBindings((roleDefinitionBindings as AveRoleDefinitionBindingCollection).RoleDefinitionBindingCollection);
        }

        public void DeleteObject()
        {
            mRoleAssignment.Parent.RoleAssignments.Remove(this.mPrincipal.Principal);
        }

        public void Update()
        {
            mRoleAssignment.Update();
        }

        #endregion
    }
}
