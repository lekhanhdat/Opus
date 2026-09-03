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



using AvePoint.Wrapper.Common;
using Microsoft.SharePoint.Administration;

namespace AvePoint.ObjectModel.Server16
{
    class AvePolicy : AveAutoSerializingObject, IAvePolicy
    {
        private SPPolicy mPolicy;
        private AvePolicyRoleBindingCollection mPolicyRoleBindings;

        public AvePolicy()
            : this(new SPPolicy())
        { }

        public AvePolicy(SPPolicy policy)
            : base(policy)
        {
            mPolicy = policy;
        }

        public string UserName
        {
            get { return mPolicy.UserName; }
        }

        public IAvePolicyRoleBindingCollection PolicyRoleBindings
        {
            get
            {
                if (mPolicyRoleBindings == null)
                {
                    SPPolicy.SPPolicyRoleBindingCollection policyRoleBindings = mPolicy.PolicyRoleBindings;
                    if (policyRoleBindings != null)
                    {
                        mPolicyRoleBindings = new AvePolicyRoleBindingCollection(policyRoleBindings);
                    }
                }
                return mPolicyRoleBindings;
            }
        }

        #region IAvePolicy Members

        public string DisplayName
        {
            get
            {
                return mPolicy.DisplayName;
            }
            set
            {
                mPolicy.DisplayName = value;
            }
        }

        public bool IsSystemUser
        {
            get
            {
                return mPolicy.IsSystemUser;
            }
            set
            {
                mPolicy.IsSystemUser = value;
            }
        }

        #endregion
    }
}
