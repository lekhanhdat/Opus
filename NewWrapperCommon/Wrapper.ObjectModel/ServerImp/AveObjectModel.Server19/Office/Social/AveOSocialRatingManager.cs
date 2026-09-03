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
using AvePoint.Wrapper.Common;
using AvePoint.Wrapper.Common.Office;
using Microsoft.Office.Server.SocialData;


namespace AvePoint.ObjectModel.Server19.Office
{
    class AveOSocialRatingManager : AveOSocialDataManager, IAveOSocialRatingManager
    {
        private SocialRatingManager mSocialRatingmanager;

        public AveOSocialRatingManager(SocialRatingManager socialRatingManager)
            : base(socialRatingManager)
        {
            mSocialRatingmanager = socialRatingManager;
        }

        public AveOSocialRatingManager(IAveServiceContext serviceContentext)
            : this(new SocialRatingManager((serviceContentext as AveServiceContext).ServiceContext))
        { }

        public IAveOSocialRating[] GetRatings(IAveOUserProfile user)
        {
            SocialRating[] socialRatings = mSocialRatingmanager.GetRatings((user as AveOUserProfile).UserProfile);
            IAveOSocialRating[] aveSocialRatings = null;
            if (socialRatings != null)
            {
                aveSocialRatings = socialRatings.OrderBy(rating => rating.LastModifiedTime).Select(rating => new AveOSocialRating(mSocialRatingmanager, rating)).ToArray();
            }
            return aveSocialRatings;
        }

        public IAveOSocialRating[] GetRatings(DateTime startTime, DateTime endTime)
        {
            SocialRating[] socialRating = (SocialRating[])AveAssemblyUtility.InvokeMethod(mSocialRatingmanager, "GetRatings", new object[] { startTime, endTime });
            AveOSocialRating[] aveSocialRating = null;
            if (socialRating != null)
            {
                aveSocialRating = new AveOSocialRating[socialRating.Length];
                for (int i = 0; i < socialRating.Length; i++)
                {
                    aveSocialRating[i] = new AveOSocialRating(mSocialRatingmanager, socialRating[i]);
                }
            }
            return aveSocialRating;
        }

        public DateTime SetRating(Uri url, int rating)
        {
            return mSocialRatingmanager.SetRating(url, rating);
        }

        public DateTime SetRating(Uri url, int rating, string title)
        {
            return mSocialRatingmanager.SetRating(url, rating, title);
        }
    }
}
