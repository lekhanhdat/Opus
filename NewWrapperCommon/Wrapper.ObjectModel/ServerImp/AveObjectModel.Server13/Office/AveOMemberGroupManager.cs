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
using Microsoft.Office.Server.UserProfiles;
using AvePoint.Wrapper.Common.Office;
using System.Collections;

namespace AvePoint.ObjectModel.Server13.Office
{
    class AveOMemberGroupManager : AveAbstractCommonCollection, IAveOMemberGroupManager
    {
        private AveOUserProfileManager mUserProfileManager;
        private MemberGroupManager mMemberGroupManager;
        private AveOMembershipManager mMembershipManager;

        public AveOMemberGroupManager(AveOMembershipManager membershipManager, AveOUserProfileManager userProfileManager, MemberGroupManager memberGroupManager)
            : base(memberGroupManager)
        {
            mMembershipManager = membershipManager;
            mUserProfileManager = userProfileManager;
            mMemberGroupManager = memberGroupManager;
        }

        internal override object CreatElementInstance(object obj)
        {
            return new AveOMemberGroup(mMembershipManager, (MemberGroup)obj);
        }

        #region IAveMemberGroupManager Members

        public IAveOMemberGroup CreateMemberGroup(Guid source, string displayName, string mailNickname, string description, string url, string sourceReference)
        {
            MemberGroup tempMemberGroup = mMemberGroupManager.CreateMemberGroup(source, displayName, mailNickname, description, url, sourceReference);
            return new AveOMemberGroup(mMembershipManager, tempMemberGroup);
        }

        public IAveOMemberGroup GetMemberGroupBySourceAndSourceReference(Guid source, string sourceReference)
        {
            return new AveOMemberGroup(mMembershipManager, mMemberGroupManager.GetMemberGroupBySourceAndSourceReference(source, sourceReference));
        }

        public long Count
        {
            get { return mMemberGroupManager.Count; }
        }

        public IAveOMemberGroup this[long id]
        {
            get
            {
                return new AveOMemberGroup(mMembershipManager, mMemberGroupManager[id]);
            }
        }

        #endregion
    }
}
