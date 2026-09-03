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
using System.Text;
using AvePoint.Wrapper.Common.Office;
using AvePoint.Wrapper.Common;
namespace AvePoint.ObjectModel.Common.Office
{
    class AveOSocialFeedManager : IAveOSocialFeedManager
    {
        private IAveRequest request;

        public AveOSocialFeedManager()
        {

        }
        public AveOSocialFeedManager(IAveRequest request)
        {
            this.request = request;

        }
        public IAveOSocialFeed GetFeedFor(string actorId, IAveOSocialFeedOptions socialFeedOptions)
        {
            AveOSocialFeedOptions options = socialFeedOptions as AveOSocialFeedOptions;
            Dictionary<string, object> optionsData = new Dictionary<string, object>();
            optionsData.Add("MaxThreadCount", options.MaxThreadCount);
            optionsData.Add("OlderThan", options.OlderThan);
            optionsData.Add("NewerThan", options.NewerThan);
            optionsData.Add("SortOrder", options.SortOrder);
            List<Dictionary<string, object>> FeedForResults = request.GetFeedFor(actorId, optionsData)["Threads"] as List<Dictionary<string, object>>;
            AveOSocialFeed aveOSocialFeed = new AveOSocialFeed();
            List<IAveOSocialThread> thread = new List<IAveOSocialThread>();
            foreach (object Result in FeedForResults)
            {
                Dictionary<string, object> ResultDictionary = Result as Dictionary<string, object>;
                AveOSocialThread aveOSocialThread = new AveOSocialThread();
                AssembleSocialThreadProperties(ResultDictionary, aveOSocialThread);
                thread.Add(aveOSocialThread);
            }
            aveOSocialFeed.Threads = thread.ToArray();
            return aveOSocialFeed;
        }

        public IAveOSocialThread GetFullThread(string threadId)
        {
            Dictionary<string, object> ResultDictionary = request.GetFullThread(threadId);
            AveOSocialThread aveOSocialThread = new AveOSocialThread();
            AssembleSocialThreadProperties(ResultDictionary, aveOSocialThread);
            return aveOSocialThread;
        }
        private void AssembleSocialThreadProperties(Dictionary<string, object> SocialThreadProperty, AveOSocialThread aveOSocialThread)
        {
            #region basicInfo
            aveOSocialThread.Id = SocialThreadProperty["Id"].ToString();
            aveOSocialThread.OwnerIndex = int.Parse(SocialThreadProperty["OwnerIndex"].ToString());
            aveOSocialThread.TotalReplyCount = int.Parse(SocialThreadProperty["TotalReplyCount"].ToString());
            aveOSocialThread.Attributes = (AveOSocialThreadAttributes)Enum.Parse(typeof(AveOSocialThreadAttributes), SocialThreadProperty["Attributes"].ToString(), true);
            if (!string.IsNullOrEmpty(SocialThreadProperty["Permalink"].ToString()))
            {
                aveOSocialThread.Permalink = new Uri(SocialThreadProperty["Permalink"].ToString());
            }
            aveOSocialThread.ThreadType = (AveOSocialThreadType)Enum.Parse(typeof(AveOSocialThreadType), SocialThreadProperty["ThreadType"].ToString(), true);
            if (SocialThreadProperty.ContainsKey("PostReference.ThreadId"))
            {
                AveOSocialPostReference postReference = new AveOSocialPostReference();
                postReference.ThreadId = SocialThreadProperty["PostReference.ThreadId"].ToString();
                postReference.ThreadOwnerIndex = int.Parse(SocialThreadProperty["PostReference.ThreadId"].ToString());
                aveOSocialThread.PostReference = postReference;
            }
            #endregion

            #region rootPost
            AveOSocialPost aveOSocialRootPost = new AveOSocialPost();
            aveOSocialRootPost.Id = SocialThreadProperty["RootPostId"].ToString();
            aveOSocialRootPost.Attributes = (AveOSocialPostAttributes)Enum.Parse(typeof(AveOSocialPostAttributes), SocialThreadProperty["RootPostAttributes"].ToString(), true);
            aveOSocialRootPost.CreatedTime = DateTime.Parse(SocialThreadProperty["RootPostCreatedTime"].ToString());
            aveOSocialRootPost.ModifiedTime = DateTime.Parse(SocialThreadProperty["RootPostModifiedTime"].ToString());
            aveOSocialRootPost.PostType = (AveOSocialPostType)Enum.Parse(typeof(AveOSocialPostType), SocialThreadProperty["RootPostPostType"].ToString());
            if (!string.IsNullOrEmpty(SocialThreadProperty["RootPostPreferredImageUri"].ToString()))
            {
                aveOSocialRootPost.PreferredImageUri = new Uri(SocialThreadProperty["RootPostPreferredImageUri"].ToString());
            }
            aveOSocialRootPost.Text = SocialThreadProperty["RootPostText"].ToString();
            aveOSocialRootPost.AuthorIndex = int.Parse(SocialThreadProperty["RootPostAuthorIndex"].ToString());
            #region
            AveOSocialPostActorInfo likeInfo = new AveOSocialPostActorInfo();
            likeInfo.TotalCount = int.Parse(SocialThreadProperty["RootPostLikerInfoTotalCount"].ToString());
            likeInfo.Indexes = SocialThreadProperty["RootPostLikerInfoIndexes"] as int[];
            likeInfo.IncludesCurrentUser = bool.Parse(SocialThreadProperty["RootPostLikerInfoIncludesCurrentUser"].ToString());
            aveOSocialRootPost.LikerInfo = likeInfo;
            #endregion

            aveOSocialThread.RootPost = aveOSocialRootPost;
            #endregion

            #region actors
            List<Dictionary<string, object>> ActorsResult = SocialThreadProperty["Actors"] as List<Dictionary<string, object>>;
            List<AveOSocialActor> ThreadActors = new List<AveOSocialActor>();
            for (int i = 0; i < ActorsResult.Count(); i++)
            {
                AveOSocialActor threadActor = new AveOSocialActor();
                threadActor.AccountName = ActorsResult[i]["AccountName"].ToString();
                threadActor.ActorType = (AveOSocialActorType)Enum.Parse(typeof(AveOSocialActorType), ActorsResult[i]["ActorType"].ToString(), true);
                threadActor.CanFollow = bool.Parse(ActorsResult[i]["CanFollow"].ToString());
                threadActor.Id = ActorsResult[i]["Id"].ToString();
                threadActor.Name = ActorsResult[i]["Name"].ToString();
                threadActor.Status = (AveOSocialStatusCode)Enum.Parse(typeof(AveOSocialStatusCode), ActorsResult[i]["Status"].ToString(), true);
                threadActor.StatusText = ActorsResult[i]["StatusText"].ToString();
                threadActor.TagGuid = new Guid(ActorsResult[i]["TagGuid"].ToString());
                if (!string.IsNullOrEmpty(ActorsResult[i]["Uri"].ToString()))
                {
                    threadActor.Uri = new Uri(ActorsResult[i]["Uri"].ToString());
                }
                if (!string.IsNullOrEmpty(ActorsResult[i]["ContentUri"].ToString()))
                {
                    threadActor.ContentUri = new Uri(ActorsResult[i]["ContentUri"].ToString());
                }
                if (!string.IsNullOrEmpty(ActorsResult[i]["FollowedContentUri"].ToString()))
                {
                    threadActor.FollowedContentUri = new Uri(ActorsResult[i]["FollowedContentUri"].ToString());
                }
                if (!string.IsNullOrEmpty(ActorsResult[i]["ImageUri"].ToString()))
                {
                    threadActor.ImageUri = new Uri(ActorsResult[i]["ImageUri"].ToString());
                }
                if (!string.IsNullOrEmpty(ActorsResult[i]["LibraryUri"].ToString()))
                {
                    threadActor.LibraryUri = new Uri(ActorsResult[i]["LibraryUri"].ToString());
                }
                if (!string.IsNullOrEmpty(ActorsResult[i]["PersonalSiteUri"].ToString()))
                {
                    threadActor.PersonalSiteUri = new Uri(ActorsResult[i]["PersonalSiteUri"].ToString());
                }
                threadActor.IsFollowed = bool.Parse(ActorsResult[i]["IsFollowed"].ToString());
                threadActor.EmailAddress = ActorsResult[i]["EmailAddress"].ToString();
                threadActor.Title = ActorsResult[i]["Title"].ToString();
                ThreadActors.Add(threadActor);
            }
            aveOSocialThread.Actors = ThreadActors.ToArray();
            #endregion

            #region replies
            List<IAveOSocialPost> replies = new List<IAveOSocialPost>();
            foreach (object sp in SocialThreadProperty["Replies"] as List<Dictionary<string, object>>)
            {
                Dictionary<string, object> spd = sp as Dictionary<string, object>;
                AveOSocialPost aveOSocialReply = new AveOSocialPost();
                aveOSocialReply.Id = spd["Id"].ToString();
                aveOSocialReply.Text = spd["Text"].ToString();
                aveOSocialReply.ModifiedTime = DateTime.Parse(spd["ModifiedTime"].ToString());
                aveOSocialReply.CreatedTime = DateTime.Parse(spd["CreatedTime"].ToString());
                aveOSocialReply.AuthorIndex = int.Parse(spd["AuthorIndex"].ToString());
                AveOSocialPostActorInfo replyLikeInfo = new AveOSocialPostActorInfo();
                replyLikeInfo.IncludesCurrentUser = bool.Parse(spd["LikerInfoIncludesCurrentUser"].ToString());
                replyLikeInfo.Indexes = spd["LikerInfoIndexes"] as int[];
                replyLikeInfo.TotalCount = int.Parse(spd["LikerInfoTotalCount"].ToString());
                aveOSocialReply.LikerInfo = replyLikeInfo;
                replies.Add(aveOSocialReply);
            }
            aveOSocialThread.Replies = replies.ToArray();
            #endregion
        }
        public IAveOSocialThread CreatePost(string targetId, IAveOSocialPostCreationData creationData)
        {
            Dictionary<string, object> CreationDataProperties = new Dictionary<string, object>();
            CreationDataProperties.Add("ContentText", creationData.ContentText);
            CreationDataProperties.Add("UpdateStatusText", creationData.UpdateStatusText);
            CreationDataProperties.Add("Attachment", creationData.Attachment);
            Dictionary<string, object> ResultDictionary = request.CreatePost(targetId, CreationDataProperties);
            AveOSocialThread aveOSocialThread = new AveOSocialThread();
            AssembleSocialThreadProperties(ResultDictionary, aveOSocialThread);
            return aveOSocialThread;
        }

        public IAveOSocialThread LikePost(string postId)
        {
            Dictionary<string, object> LikePostResult = request.LikePost(postId);
            AveOSocialThread aveOSocialThread = new AveOSocialThread();
            return aveOSocialThread;
        }

        public IAveOSocialAttachment CreateImageAttachment(string name, string description, System.IO.Stream content)
        {
            throw new NotImplementedException();
        }
        
        public IAveOSocialThread DeletePost(string postId)
        {
            throw new NotImplementedException();
        }

        public IAveOSocialThread LockThread(string postId)
        {
            throw new NotImplementedException();
        }


        public IAveOSocialThread UnlockThread(string postId)
        {
            throw new NotImplementedException();
        }
    }
}