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

namespace AvePoint.ObjectModel.Server19.Office
{
    class AveOSocialTag : AveOSocialData, IAveOSocialTag,IDisposable
    {
        private SocialTagManager mSocialTagManager;
        private SocialTag mSocialTag;
        private AveTerm mTerm;

        public AveOSocialTag(SocialTagManager socialTagManager, SocialTag socialTag)
            : base(socialTagManager, socialTag)
        {
            // TODO: Complete member initialization
            this.mSocialTagManager = socialTagManager;
            this.mSocialTag = socialTag;
        }

        #region IAveSocialTag Members

        public string Title
        {
            get
            {
                return mSocialTag.Title;
            }
        }

        public Uri Url
        {
            get
            {
                return mSocialTag.Url;
            }
        }

        public bool IsPrivate
        {
            get
            {
                return mSocialTag.IsPrivate;
            }
        }

        public IAveTerm Term
        {
            get
            {
                if (mTerm == null)
                {
                    mTerm = new AveTerm(mSocialTag.Term);
                }
                return mTerm;
            }
        }

        #endregion

        public void Dispose()
        {
            if (mTerm != null)
            {
                mTerm.Dispose();
            }
        }
    }
}
