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
using Microsoft.SharePoint;

namespace AvePoint.ObjectModel.Server16
{
    class AvePolicyCollection : AveAbstractCommonCollection<IAvePolicy>, IAvePolicyCollection
    {
        private SPPolicyCollection mPolicyCollection;

        public AvePolicyCollection(SPPolicyCollection policyCollection)
            : base(policyCollection)
        {
            mPolicyCollection = policyCollection;
        }

        public AvePolicyCollection()
            : this(new SPPolicyCollection())
        { }

        public override IAvePolicy this[int index]
        {
            get
            {
                SPPolicy policy = mPolicyCollection[index];
                if (policy == null)
                {
                    return null;
                }
                return new AvePolicy(policy);
            }
        }

        protected override object CreatElementInstance(object t)
        {
            return new AvePolicy(t as SPPolicy);
        }

        public override int Count
        {
            get { return mPolicyCollection.Count; }
        }

        #region IAvePolicyCollection Members

        public AveAnonymousPolicy AnonymousPolicy
        {
            get
            {
                return (AveAnonymousPolicy)mPolicyCollection.AnonymousPolicy;
            }
            set
            {
                mPolicyCollection.AnonymousPolicy = (SPAnonymousPolicy)value;
            }
        }

        public IAvePolicy Add(string userName, string displayName)
        {
            return new AvePolicy(mPolicyCollection.Add(userName, displayName));
        }

        public IAvePolicy this[string name]
        {
            get
            {
                SPPolicy policy = mPolicyCollection[name];
                if (policy == null)
                {
                    return null;
                }
                return new AvePolicy(policy);
            }
        }

        public void Remove(string name)
        {
            mPolicyCollection.Remove(name);
        }

        #endregion
    }
}
