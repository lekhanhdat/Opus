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
using Microsoft.Office.Server.UserProfiles;
using AvePoint.Wrapper.Common.Office;

namespace AvePoint.ObjectModel.Server16.Office
{
    class AveOMembershipManager : IAveOMembershipManager
    {
        private MembershipManager mMembershipManager;

        public AveOMembershipManager(MembershipManager membershipManager)
        {
            mMembershipManager = membershipManager;
        }

        #region IAveMembershipManager Members

        public IAveOMembership[] GetItems()
        {
            Membership[] memberships = mMembershipManager.GetItems();
            IAveOMembership[] aveMemberships = new IAveOMembership[memberships.Length];
            for (int i = 0; i < memberships.Length; i++)
            {
                if (memberships[i] != null)
                {
                    aveMemberships[i] = new AveOMembership(this, memberships[i]);
                }
                else
                {
                    aveMemberships[i] = null;
                }
            }
            return aveMemberships;
        }

        public IAveOMembership this[IAveOMemberGroup memberGroup]
        {
            get
            {
                return new AveOMembership(this, mMembershipManager[(memberGroup as AveOMemberGroup).MemberGroup]);
            }
        }

        public IAveOMembership Create(IAveOMemberGroup memberGroup, AveMembershipGroupType groupType, string groupName, AvePrivacy privacyLevel)
        {
            Membership tempMembership = mMembershipManager.Create((memberGroup as AveOMemberGroup).MemberGroup, (MembershipGroupType)groupType, groupName, (Privacy)privacyLevel);
            return new AveOMembership(this, tempMembership);
        }

        #endregion
    }
}
