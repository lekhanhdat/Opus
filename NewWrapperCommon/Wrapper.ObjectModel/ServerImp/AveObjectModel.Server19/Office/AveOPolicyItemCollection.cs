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
using AvePoint.Wrapper.Common.Office;
using Microsoft.Office.RecordsManagement.InformationPolicy;
using AvePoint.Wrapper.Common;

namespace AvePoint.ObjectModel.Server19.Office
{
    class AveOPolicyItemCollection : AveAbstractCommonCollection<IAveOPolicyItem>, IAveOPolicyItemCollection
    {
        private PolicyItemCollection mPolicyItemCollection;

        public AveOPolicyItemCollection(PolicyItemCollection policyItemCollection)
            : base(policyItemCollection)
        {
            mPolicyItemCollection = policyItemCollection;
        }

        public AveOPolicyItemCollection(IAveOPolicy policy)
            : this((PolicyItemCollection)AveAssemblyUtility.CreateInstance(typeof(PolicyItemCollection), new Type[] { typeof(Policy) }, new object[] { (policy as AveOPolicy).Policy }))
        { }

        protected override object CreatElementInstance(object t)
        {
            return new AveOPolicyItem(t as PolicyItem);
        }

        public override int Count
        {
            get { return mPolicyItemCollection.Count; }
        }

        #region IAveOPolicyItemCollection Members

        public bool IsDirty
        {
            get
            {
                return mPolicyItemCollection.IsDirty;
            }
        }

        public IAveOPolicyItem this[string policyFeatureId]
        {
            get
            {
                PolicyItem policyItem = mPolicyItemCollection[policyFeatureId];
                if (policyItem != null)
                {
                    return new AveOPolicyItem(policyItem);
                }
                return null;
            }
        }

        public override IAveOPolicyItem this[int index]
        {
            get
            {
                PolicyItem policyItem = mPolicyItemCollection[index];
                if (policyItem != null)
                {
                    return new AveOPolicyItem(policyItem);
                }
                return null;
            }
        }

        public void Add(string policyFeatureId, string customData)
        {
            mPolicyItemCollection.Add(policyFeatureId, customData);
        }

        public void Delete(string policyFeatureId)
        {
            mPolicyItemCollection.Delete(policyFeatureId);
        }

        #endregion
    }
}
