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
using AvePoint.Wrapper.Common;
using Microsoft.Office.Server.SocialData;
using AvePoint.Wrapper.Common.Office;
using Microsoft.Office.Server.Social;

namespace AvePoint.ObjectModel.Server19.Office
{
    class AveOSocialFeedOptions : IAveOSocialFeedOptions
    {
        private SPSocialFeedOptions mSocialFeedOptions;

        public AveOSocialFeedOptions()
        {
            mSocialFeedOptions = new SPSocialFeedOptions();
        }

        public AveOSocialFeedOptions(SPSocialFeedOptions socialFeedOptions)
        {
            mSocialFeedOptions = socialFeedOptions;
        }

        internal SPSocialFeedOptions SocialFeedOptions
        {
            get
            {
                return mSocialFeedOptions;
            }
        }

        public int MaxThreadCount
        {
            get
            {
                return mSocialFeedOptions.MaxThreadCount;
            }
            set
            {
                mSocialFeedOptions.MaxThreadCount = value;
            }
        }

        public DateTime NewerThan
        {
            get
            {
                return mSocialFeedOptions.NewerThan;
            }
            set
            {
                mSocialFeedOptions.NewerThan = value;
            }
        }

        public DateTime OlderThan
        {
            get
            {
                return mSocialFeedOptions.OlderThan;
            }
            set
            {
                mSocialFeedOptions.OlderThan = value;
            }
        }

        public AveOSocialFeedSortOrder SortOrder
        {
            get
            {
                return (AveOSocialFeedSortOrder)mSocialFeedOptions.SortOrder;
            }
            set
            {
                mSocialFeedOptions.SortOrder = (SPSocialFeedSortOrder)value;
            }
        }
    }
}
