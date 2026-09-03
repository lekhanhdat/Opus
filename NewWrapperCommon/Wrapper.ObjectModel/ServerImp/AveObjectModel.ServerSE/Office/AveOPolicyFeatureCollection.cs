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
using AvePoint.Wrapper.Common.Office;

namespace AvePoint.ObjectModel.ServerSE.Office
{
    class AveOPolicyFeatureCollection : AveAbstractCommonCollection<IAveOPolicyFeature>, IAveOPolicyFeatureCollection
    {
        private PolicyFeatureCollection mPolicyCollection;

        public AveOPolicyFeatureCollection(PolicyFeatureCollection policyCollection)
            : base(policyCollection)
        {
            mPolicyCollection = policyCollection;
        }

        public override IAveOPolicyFeature this[int index]
        {
            get
            {
                PolicyFeature policyFeature = mPolicyCollection[index];
                if (policyFeature == null)
                {
                    return null;
                }
                return new AveOPolicyFeature(policyFeature);
            }
        }

        protected override object CreatElementInstance(object t)
        {
            return new AveOPolicyFeature(t as PolicyFeature);
        }

        public override int Count
        {
            get { return mPolicyCollection.Count; }
        }

        public IAveOPolicyFeature this[string Id]
        {
            get
            {
                PolicyFeature policyFeature = mPolicyCollection[Id];
                if (policyFeature == null)
                {
                    return null;
                }
                return new AveOPolicyFeature(policyFeature);
            }
        }
    }
}
