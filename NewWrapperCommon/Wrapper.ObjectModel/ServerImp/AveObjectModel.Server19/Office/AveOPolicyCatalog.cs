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



using Microsoft.Office.RecordsManagement.InformationPolicy;
using AvePoint.Wrapper.Common;
using AvePoint.Wrapper.Common.Office;

namespace AvePoint.ObjectModel.Server19.Office
{
    class AveOPolicyCatalog : IAveOPolicyCatalog
    {
        private PolicyCatalog mPolicyCatalog;
        private AveOPolicyFeatureCollection mFeatureList;
        private AveOPolicyCollection mPolicyList;

        public AveOPolicyCatalog(PolicyCatalog policyCatalog)
        {
            mPolicyCatalog = policyCatalog;
        }

        /// <summary>
        /// For Calling Static Method
        /// </summary>
        public AveOPolicyCatalog()
        { }

        public AveOPolicyCatalog(IAveSite site)
        {
            mPolicyCatalog = new PolicyCatalog((site as AveSite).Site);
        }

        #region IAvePolicyCatalog Members

        public IAveOPolicyFeatureCollection FeatureList
        {
            get
            {
                if (mFeatureList == null)
                {
                    PolicyFeatureCollection policyFeatures = PolicyCatalog.FeatureList;
                    if (policyFeatures != null)
                    {
                        mFeatureList = new AveOPolicyFeatureCollection(policyFeatures);
                    }
                }
                return mFeatureList;
            }
        }

        public IAveOPolicyCollection PolicyList
        {
            get 
            {
                if (mPolicyList == null)
                {
                    mPolicyList = new AveOPolicyCollection(mPolicyCatalog.PolicyList);
                }
                return mPolicyList;
            }
        }

        #endregion
    }
}
