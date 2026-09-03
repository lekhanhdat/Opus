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
using Microsoft.Office.Server.Microfeed;
using System.Collections.Generic;

namespace AvePoint.ObjectModel.Server16.Office
{
    class AveOSocialThread : IAveOSocialThread
    {
        private SPSocialThread mSocialThread;
        private AveOSocialPost mSocialPost;

        public AveOSocialThread()
        {
        }

        public AveOSocialThread(SPSocialThread socialThread)
        {
            mSocialThread = socialThread;
        }

        public AveOSocialThreadAttributes Attributes
        {
            get
            {
                return (AveOSocialThreadAttributes)mSocialThread.Attributes;
            }
        }

        public string Id
        {
            get
            {
                return mSocialThread.Id;
            }
        }

        public IAveOSocialPost[] Replies
        {
            get
            {
                List<IAveOSocialPost> post = new List<IAveOSocialPost> { };
                SPSocialPost[] tmpPost = mSocialThread.Replies;
                foreach (SPSocialPost sp in tmpPost)
                {
                    if (post != null)
                    {
                        post.Add(new AveOSocialPost(sp));
                    }
                    else
                    {
                        post.Add(null);
                    }
                }
                return post.ToArray();
            }
        }

        public IAveOSocialPost RootPost
        {
            get
            {
                if (mSocialPost == null)
                {
                    SPSocialPost post = mSocialThread.RootPost;
                    if (post != null)
                    {
                        mSocialPost = new AveOSocialPost(post);
                    }
                }
                return mSocialPost;
            }
        }

        public IAveOSocialActor[] Actors
        {
            get
            {
                List<IAveOSocialActor> actor = new List<IAveOSocialActor> { };
                SPSocialActor[] tmpActor = mSocialThread.Actors;
                foreach (SPSocialActor sa in tmpActor)
                {
                    if (actor != null)
                    {
                        actor.Add(new AveOSocialActor(sa));
                    }
                    else
                    {
                        actor.Add(null);
                    }
                }
                return actor.ToArray();
            }
        }

        public int OwnerIndex
        {
            get
            {
                return mSocialThread.OwnerIndex;
            }
        }

        public AveOSocialThreadType ThreadType
        {
            get
            {
                return (AveOSocialThreadType)mSocialThread.ThreadType;
            }
        }


        public Uri Permalink
        {
            get { return mSocialThread.Permalink; }
        }

        public IAveOSocialPostReference PostReference
        {
            get { return new AveOSocialPostReference(mSocialThread.PostReference); }
        }

        public AveOSocialStatusCode Status
        {
            get { return (AveOSocialStatusCode)mSocialThread.Status; }
        }

        public int TotalReplyCount
        {
            get { return mSocialThread.TotalReplyCount; }
        }
    }
}
