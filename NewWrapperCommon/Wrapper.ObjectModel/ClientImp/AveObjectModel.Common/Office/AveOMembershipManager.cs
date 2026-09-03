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
using AvePoint.Wrapper.Common;
using AvePoint.Wrapper.Common.Office;

namespace AvePoint.ObjectModel.Common.Office
{
    class AveOMembershipManager:AveAbstractCommonCollection<IAveOMembership>,IAveOMembershipManager
    {
        private IAveRequest mRequest;
        private AveOUserProfile mProfile;

        public AveOMembershipManager(IAveRequest request, AveOUserProfile profile, Dictionary<string, object> membershipsProp)
        {
            mRequest = request;
            mProfile = profile;
            base.DataCache.AddPropertyies(membershipsProp);
            InitMembershipManager();
        }
        internal void InitMembershipManager()
        {
            List<Dictionary<string, object>> membershipList = base.DataCache.GetProperty<List<Dictionary<string, object>>>(AveObjectModelConstant.ChildrenProperties);
            mListData = new List<IAveOMembership>(membershipList.Count);
            foreach(Dictionary<string,object>membershipProp in membershipList )
            {
                AveOMembership membership = new AveOMembership(this.mRequest,this.mProfile,membershipProp);
                mListData.Add(membership);
            }
        }
        public IAveOMembership[] GetItems()
        {
            IAveOMembership[] memberships = new IAveOMembership[mListData.Count];
            for (int i = 0; i < mListData.Count; i++)
            {
                memberships[i] = mListData[i];
            }
            return memberships;
        }
        public IAveOMembership this[IAveOMemberGroup memberGroup] 
        {
            get
            { throw new NotImplementedException(); }
        }
        public IAveOMembership Create(IAveOMemberGroup memberGroup, AveMembershipGroupType groupType, string groupName, AvePrivacy privacyLevel)
        {
            throw new NotImplementedException();
        }
    }
}
