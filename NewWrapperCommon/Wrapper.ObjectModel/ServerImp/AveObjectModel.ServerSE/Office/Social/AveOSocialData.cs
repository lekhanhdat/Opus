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



using AvePoint.Wrapper.Common.Office;
using Microsoft.Office.Server.SocialData;
using Microsoft.Office.Server.UserProfiles;
using System;

namespace AvePoint.ObjectModel.ServerSE.Office
{
    abstract class AveOSocialData : IAveOSocialData
    {
        private SocialData mSocialData;
        private SocialDataManager mDataManager;
        private AveOUserProfile mOwner;
        private string mOwnerName;

        public AveOSocialData(SocialDataManager dataManager, SocialData socialData)
        {
            mSocialData = socialData;
            mDataManager = dataManager;
        }

        #region IAveOSocialData Members

        public IAveOUserProfile Owner
        {
            get
            {
                if (mOwner == null)
                {
                    UserProfile owner = mSocialData.Owner;
                    if (owner != null)
                    {
                        mOwner = new AveOUserProfile(owner);
                    }
                }
                return mOwner;
            }
        }

        public string OwnerName
        {
            get
            {
                if (mSocialData.Owner != null && string.IsNullOrEmpty(mOwnerName))
                {
                    return mSocialData.Owner.MultiloginAccounts[0];
                }
                return mOwnerName;
            }
            set { mOwnerName = value; }
        }

        public DateTime LastModifiedTime
        {
            get { return mSocialData.LastModifiedTime; }
        }

        public string Title
        {
            get { return mSocialData.Title; }
        }

        public Uri Url
        {
            get { return mSocialData.Url; }
        }

        #endregion
    }
}
