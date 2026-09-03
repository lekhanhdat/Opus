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

namespace AvePoint.ObjectModel.Server16.Office
{
    class AveOSocialPost : IAveOSocialPost
    {
        private SPSocialPost mSocialPost;
        private AveOSocialAttachment mSocialAttachment;

        public AveOSocialPost(SPSocialPost socialPost)
        {
            if (socialPost == null)
            {
                throw new ArgumentNullException("SocialPost");
            }
            mSocialPost = socialPost;
        }

        public string Id
        {
            get
            {
                return mSocialPost.Id;
            }
        }

        public string Text
        {
            get 
            {
                return mSocialPost.Text;
            }
        }

        public AveOSocialPostAttributes Attributes
        {
            get
            {
                return (AveOSocialPostAttributes)mSocialPost.Attributes;
            }
        }

        public DateTime CreatedTime
        {
            get
            {
                return mSocialPost.CreatedTime;
            }
        }

        public DateTime ModifiedTime
        {
            get
            {
                return mSocialPost.ModifiedTime;
            }
        }

        public IAveOSocialAttachment Attachment
        {
            get
            {
                if (mSocialAttachment == null)
                {
                   SPSocialAttachment socialAttachment = mSocialPost.Attachment;
                   if (socialAttachment != null)
                    {
                        mSocialAttachment = new AveOSocialAttachment(socialAttachment);
                    }
                }
                return mSocialAttachment;
            }
        }

        public int AuthorIndex
        {
            get 
            {
                return mSocialPost.AuthorIndex;
            }
        }

        public AveOSocialPostType PostType
        {
            get
            {
                return (AveOSocialPostType)mSocialPost.PostType;
            }
        }

        public IAveOSocialPostActorInfo LikerInfo
        {
            get
            {
                return new AveOSocialPostActorInfo(mSocialPost.LikerInfo);
            }
        }

         private IAveOSocialDataOverlay[] mOverlays = null;
        public IAveOSocialDataOverlay[] Overlays
        {
            get 
            {
                if (mOverlays == null)
                {
                    if (mSocialPost.Overlays != null)
                    {
                        mOverlays = new IAveOSocialDataOverlay[mSocialPost.Overlays.Length];
                        for (int i = 0; i < mOverlays.Length; i++)
                        {
                            mOverlays[i] = new AveOSocialDataOverlay(mSocialPost.Overlays[i]);
                        }
                    }
                }

                return mOverlays;
            }
        }

        public Uri PreferredImageUri
        {
            get { return mSocialPost.PreferredImageUri; }
        }

        public IAveOSocialLink Source
        {
            get { return new AveOSocialLink(mSocialPost.Source); }
        }
    }
}
