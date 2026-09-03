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

namespace AvePoint.ObjectModel.Server16
{
    class AvePolicyRoleCollection : AveAbstractCommonCollection<IAvePolicyRole>, IAvePolicyRoleCollection
    {
        private SPPolicyRoleCollection mPolicyRoleCollection;

        public AvePolicyRoleCollection(SPPolicyRoleCollection policyRoleCollection)
            : base(policyRoleCollection)
        {
            mPolicyRoleCollection = policyRoleCollection;
        }

        protected override object CreatElementInstance(object t)
        {
            return new AvePolicyRole(t as SPPolicyRole);
        }

        public override int Count
        {
            get
            {
                return mPolicyRoleCollection.Count;
            }
        }

        #region IAvePolicyRoleCollection Members

        public IAvePolicyRole Add(string name, string description, AveBasePermissions grantRightsMask, AveBasePermissions denyRightsMask)
        {
            SPPolicyRole policyRole = mPolicyRoleCollection.Add(name, description, (SPBasePermissions)grantRightsMask, (SPBasePermissions)denyRightsMask);
            return new AvePolicyRole(policyRole);
        }

        public void DeleteById(Guid id)
        {
            mPolicyRoleCollection.DeleteById(id);
        }

        public IAvePolicyRole GetById(Guid id)
        {
            SPPolicyRole policyRole = mPolicyRoleCollection.GetById(id);
            if (policyRole == null)
            {
                return null;
            }
            return new AvePolicyRole(policyRole);
        }

        #endregion
    }
}
