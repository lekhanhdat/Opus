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
using AvePoint.Wrapper.Common;
using Microsoft.SharePoint.Administration;
using Microsoft.SharePoint;

namespace AvePoint.ObjectModel.Server19
{
    class AvePolicyRole : AveAutoSerializingObject, IAvePolicyRole
    {
        private SPPolicyRole mPolicyRole;

        public AvePolicyRole()
            : this(new SPPolicyRole())
        { }

        public AvePolicyRole(SPPolicyRole policyRole)
            : base(policyRole)
        {
            mPolicyRole = policyRole;
        }

        #region IAvePolicyRole Members

        public string Name
        {
            get
            {
                return mPolicyRole.Name;
            }
            set
            {
                mPolicyRole.Name = value;
            }
        }

        public Guid ID
        {
            get { return mPolicyRole.Id; }
        }

        public AveBasePermissions DenyRightsMask
        {
            get
            {
                return (AveBasePermissions)mPolicyRole.DenyRightsMask;
            }
            set
            {
                mPolicyRole.DenyRightsMask = (SPBasePermissions)value;
            }
        }

        public string Description
        {
            get
            {
                return mPolicyRole.Description;
            }
            set
            {
                mPolicyRole.Description = value;
            }
        }

        public AveBasePermissions GrantRightsMask
        {
            get
            {
                return (AveBasePermissions)mPolicyRole.GrantRightsMask;
            }
            set
            {
                mPolicyRole.GrantRightsMask = (SPBasePermissions)value;
            }
        }

        public bool IsSiteAdmin
        {
            get
            {
                return mPolicyRole.IsSiteAdmin;
            }
            set
            {
                mPolicyRole.IsSiteAdmin = value;
            }
        }

        public bool IsSiteAuditor
        {
            get
            {
                return mPolicyRole.IsSiteAuditor;
            }
            set
            {
                mPolicyRole.IsSiteAuditor = value;
            }
        }

        public AvePolicyRoleType Type
        {
            get
            {
                return (AvePolicyRoleType)mPolicyRole.Type;
            }
        }

        #endregion
    }
}
