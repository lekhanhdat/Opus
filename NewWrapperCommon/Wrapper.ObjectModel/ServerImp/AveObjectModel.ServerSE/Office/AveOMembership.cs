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

namespace AvePoint.ObjectModel.ServerSE.Office
{
    class AveOMembership : IAveOMembership
    {
        private AveOMembershipManager mMembershipManager;
        private Membership mMembership;
        private AveOPrivacyPolicyItem mPorlicy;
        private AveOMemberGroup mMemberGroup;

        public AveOMembership(AveOMembershipManager aveMembershipManager, Membership membership)
        {
            mMembershipManager = aveMembershipManager;
            mMembership = membership;
        }

        public IAveOPrivacyPolicyItem Policy
        {
            get
            {
                if (mPorlicy == null)
                {
                    mPorlicy = new AveOPrivacyPolicyItem(mMembership.Policy);
                }
                return mPorlicy;
            }
        }

        public IAveOMemberGroup MembershipGroup
        {
            get
            {
                if (mMemberGroup == null)
                {
                    MemberGroup memberGroup = mMembership.MembershipGroup;
                    if (memberGroup != null)
                    {
                        mMemberGroup = new AveOMemberGroup(mMembershipManager, memberGroup);
                    }
                }
                return mMemberGroup;
            }
        }

        public string Title
        {
            get
            {
                return mMembership.Title;
            }
            set
            {
                mMembership.Title = value;
            }
        }

        public string Group
        {
            get
            {
                return mMembership.Group;
            }
            set
            {
                mMembership.Group = value;
            }
        }

        public AveMembershipGroupType GroupType
        {
            get
            {
                return (AveMembershipGroupType)mMembership.GroupType;
            }
            set
            {
                mMembership.GroupType = (MembershipGroupType)value;
            }
        }

        public bool IsEditable
        {
            get
            {
                return mMembership.IsEditable;
            }
        }

        public bool IsPrivacyLevelEditable
        {
            get
            {
                return mMembership.IsPrivacyLevelEditable;
            }
        }

        public bool IsTitleEditable
        {
            get
            {
                return mMembership.IsTitleEditable;
            }
        }

        public bool IsUrlEditable
        {
            get
            {
                return mMembership.IsUrlEditable;
            }
        }

        public AvePrivacy PrivacyLevel
        {
            get
            {
                return (AvePrivacy)mMembership.PrivacyLevel;
            }
            set
            {
                mMembership.PrivacyLevel = (Privacy)value;
            }
        }

        public string Url
        {
            get
            {
                return mMembership.Url;
            }
            set
            {
                mMembership.Url = value;
            }
        }

        public void Commit()
        {
            mMembership.Commit();
        }
    }
}
