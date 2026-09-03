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
using System.Text;

namespace AvePoint.Wrapper.Common
{
    public class AveRatingsFeatureConstants
    {
        // Fields
        public const string EmptyIconUrl_Name = "Ratings_EmptyIconUrl";
        public const string EmptyIconUrl_Value = "/_layouts/Images/RatingsEmpty.png";
        public const string FeatureActivated_False_Value = "false";
        public const string FeatureActivated_Name = "Ratings_FeatureActivated";
        public const string FeatureActivated_True_Value = "true";
        public const string ImageStripRtlUrl_Name = "Ratings_ImageStripRtlUrl";
        public const string ImageStripRtlUrl_Value = "/_layouts/Images/Ratingsrtl.png";
        public const string ImageStripUrl_Name = "Ratings_ImageStripUrl";
        public const string ImageStripUrl_Value = "/_layouts/Images/Ratings.png";
        public const string LoadingImg = @"<img src=\'/_layouts/images/loading16.gif\'> ";
        public const string NewRatingIconUrl_Name = "Ratings_NewRatingIconUrl";
        public const string NewRatingIconUrl_Value = "/_layouts/Images/RatingsNew.png";
        public static readonly Guid RatingsFieldGuid_AverageRating;
        public static readonly Guid RatingsFieldGuid_FeatureId;
        public static readonly Guid RatingsFieldGuid_RatingCount;

        // Methods
        static AveRatingsFeatureConstants()
        {
            RatingsFieldGuid_FeatureId = new Guid("915c240e-a6cc-49b8-8b2c-0bff8b553ed3");
            RatingsFieldGuid_AverageRating = new Guid("5a14d1ab-1513-48c7-97b3-657a5ba6c742");
            RatingsFieldGuid_RatingCount = new Guid("b1996002-9167-45e5-a4df-b2c41c6723c7");
        }
        private AveRatingsFeatureConstants()
        { }
    }
}
