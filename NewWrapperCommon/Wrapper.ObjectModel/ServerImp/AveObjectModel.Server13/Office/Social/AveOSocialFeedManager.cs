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
using Microsoft.Office.Server.UserProfiles;
using AvePoint.Wrapper.Common.Office;
using Microsoft.Office.Server.Social;
using Microsoft.SharePoint;

namespace AvePoint.ObjectModel.Server13.Office
{
    class AveOSocialFeedManager : IAveOSocialFeedManager
    {
        private SPSocialFeedManager mSocialFeedManager;

        public AveOSocialFeedManager()
        {
            mSocialFeedManager = new SPSocialFeedManager();
        }

        public AveOSocialFeedManager(SPSocialFeedManager socialFeedManager)
        {
            mSocialFeedManager = socialFeedManager;
        }

        public AveOSocialFeedManager(IAveOUserProfile profile, IAveServiceContext context)
        {
            UserProfile tmpProfile = ((profile as AveOUserProfile) == null) ? null : (profile as AveOUserProfile).UserProfile;
            SPServiceContext tmpServiceContext = ((context as AveServiceContext) == null) ? null : (context as AveServiceContext).ServiceContext;
            mSocialFeedManager = new SPSocialFeedManager((profile as AveOUserProfile).UserProfile, (context as AveServiceContext).ServiceContext);
        }

        public IAveOSocialFeed GetFeedFor(string actorId, IAveOSocialFeedOptions options)
        {
            return new AveOSocialFeed(mSocialFeedManager.GetFeedFor(actorId, ((AveOSocialFeedOptions)options).SocialFeedOptions));
        }

        public IAveOSocialThread GetFullThread(string threadId)
        {
            return new AveOSocialThread(mSocialFeedManager.GetFullThread(threadId));
        }

        public IAveOSocialThread CreatePost(string targetId, IAveOSocialPostCreationData creationData)
        {
            SPSocialThread thread = null;
            try
            {
                thread = mSocialFeedManager.CreatePost(targetId, (creationData as AveOSocialPostCreationData).SocialPostCreationData);
            }
            catch (SPSocialException ex)
            {
                if(ex.InternalErrorCode == 97)
                    thread = mSocialFeedManager.CreatePost(null, (creationData as AveOSocialPostCreationData).SocialPostCreationData);
            }

            return new AveOSocialThread(thread);
        }

        public IAveOSocialThread LikePost(string postId)
        {
            return new AveOSocialThread(mSocialFeedManager.LikePost(postId));
        }

        public IAveOSocialAttachment CreateImageAttachment(string name, string description, System.IO.Stream content)
        {
            return new AveOSocialAttachment(mSocialFeedManager.CreateImageAttachment(name, description, content));
        }

        public IAveOSocialThread DeletePost(string postId)
        {
            return new AveOSocialThread(mSocialFeedManager.DeletePost(postId));
        }

        public IAveOSocialThread LockThread(string postId)
        {
            return new AveOSocialThread(mSocialFeedManager.LockThread(postId));
        }

        public IAveOSocialThread UnlockThread(string postId)
        {
            return new AveOSocialThread(mSocialFeedManager.UnlockThread(postId));
        }
    }
}
