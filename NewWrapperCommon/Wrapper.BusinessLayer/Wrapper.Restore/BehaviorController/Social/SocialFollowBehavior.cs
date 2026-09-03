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

namespace AvePoint.Wrapper.Restore
{
    using Common.Office;
    using System;
    using System.Collections.Generic;

    class SocialFollowPinStateBehavior : IWrapperBusinessBehavior
    {
        private IAveOUserProfile userProfile;
        private Uri uri;
        private Guid groupId;
        private int pinState;

        private bool containsPinInfo;

        public SocialFollowPinStateBehavior(IAveOUserProfile userProfile,Dictionary<string,object> userData)
        {
            this.userProfile = userProfile;
            containsPinInfo = TryGetPinnedStateInfo(userData, out uri, out groupId, out pinState);
        }


        public void Run()
        {
            if (!containsPinInfo) return;
            this.userProfile.FollowedContent.SetItemPinState(uri, groupId, pinState);
        }

        /// <summary>
        /// 是否存在PinnedState, Url和Pinned存在即可
        /// </summary>
        /// <param name="allUserData"></param>
        /// <param name="info"></param>
        /// <returns></returns>
        private bool TryGetPinnedStateInfo(Dictionary<string, object> allUserData, out Uri uri, out Guid groupId, out int pinState)
        {
            uri = default(Uri);
            groupId = default(Guid);
            pinState = default(int);
            string strUri;

            if (TryGetValue(allUserData, "Url", out strUri) &&
                TryGetValue(allUserData, "Pinned", out pinState))
            {
                uri = new Uri(strUri);
                TryGetValue(allUserData, "GroupId", out groupId);
                return true;
            }
            return false;
        }

        private bool TryGetValue<T>(Dictionary<string, object> allUserData, string key, out T value)
        {
            object obj;
            if (allUserData.TryGetValue(key, out obj))
            {
                value = (T)obj;
                return true;
            }
            value = default(T);
            return false;
        }
    }
}
