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
using AvePoint.Wrapper.Resource.ServerAPI2010;
using AvePoint.GCommon.Utility.Exceptions.SharePoint;

namespace AvePoint.ObjectModel.Server16
{
    abstract class AvePrincipal : AveMember, IAvePrincipal
    {
        private SPPrincipal mPrincipal;
        private AveRoleCollection mRoles;
        protected AveWeb mWeb;

        protected AvePrincipal(){}
        public AvePrincipal(AveWeb web, SPPrincipal principal)
            : base(principal)
        {
            mWeb = web;
            mPrincipal = principal;
        }

        internal SPPrincipal Principal
        {
            get
            {
                CheckPrincipal();
                return mPrincipal;
            }
        }

        internal static AvePrincipal InitPrincipal(AveWeb web, SPPrincipal principal)
        {
            return (AvePrincipal)AveServerAssemblyInit.CreateElement(typeof(IAvePrincipal), new object[] { web, principal });
        }

        #region IAvePrincipal Members

        public override int ID
        {
            get 
            {
                CheckPrincipal();
                return mPrincipal.ID;
            }
        }

        public abstract string LoginName
        {
            get;
        }

        public abstract string Name
        {
            get;
            set;
        }

        public abstract AvePrincipalType PrincipalType
        {
            get;
        }

        public IAveRoleCollection Roles
        {
            get
            {
                CheckPrincipal();
                if (mRoles == null)
                {
                    mRoles = new AveRoleCollection(mWeb, mPrincipal.Roles);
                }
                return mRoles;
            }
        }

        public IAveWeb ParentWeb
        {
            get 
            {
                CheckPrincipal();
                return mWeb; 
            }
        }

        private void CheckPrincipal()
        {
            if (mPrincipal == null)
            {
                throw new UserNotFoundException(-1);
            }
        }
        #endregion
    }
}
