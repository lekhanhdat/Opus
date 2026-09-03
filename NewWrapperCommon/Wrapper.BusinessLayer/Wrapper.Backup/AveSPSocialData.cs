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
using System.Reflection;
using AvePoint.GCommon;
using AvePoint.GCommon.Utility.I18N;
using AvePoint.Wrapper.Common;
using AvePoint.Wrapper.Common.Office;
using System.Linq;

namespace AvePoint.Wrapper.Backup
{
    public class AveSPSocialTag
    {
        private static AveLogger mLog = AveLogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);
        private AveSPSite mAveParentSite;

        public AveSPSite ParentSite
        {
            get { return mAveParentSite; }
        }

        public string mUrl;

        public AveSPSocialTag(string url, AveSPSite aveSite)
        {
            mUrl = url;
            mAveParentSite = aveSite;
        }

        public List<AveSocialTagInfo> GetSocialTags()
        {
            if (mAveParentSite.UserProfileApplicationAvailable)
            {
                using (AvePerformanceScope pc = new AvePerformanceScope("Backup.AveSPSocialTag.GetSocialTags"))
                {
                    try
                    {
                        List<AveSocialTagInfo> DTs = new List<AveSocialTagInfo>();
                        IAveOSocialTag[] tags = mAveParentSite.TagManager.GetTags(mUrl, mAveParentSite.UserProfiles);
                        foreach (IAveOSocialTag tag in tags)
                        {
                            AveSocialTagInfo dtInfo = new AveSocialTagInfo();
                            dtInfo.Url = tag.Url.ToString();
                            dtInfo.Title = tag.Title;

                            dtInfo.Owner = tag.OwnerName;
                            dtInfo.IsPrivate = tag.IsPrivate;
                            dtInfo.LastModifiedTime = tag.LastModifiedTime;

                            dtInfo.Term = new AveTermInfo();
                            IAveTerm term = tag.Term;
                            dtInfo.Term.Owner = term.Owner;
                            dtInfo.Term.Id = term.ID;
                            dtInfo.Term.TermName = term.Name;
                            dtInfo.Term.IsRoot = term.IsRoot;
                            dtInfo.Term.IsKeyword = term.IsKeyword;
                            dtInfo.Term.SourceTermId = tag.Term.SourceTerm.ID;
                            dtInfo.Term.SourceTermName = tag.Term.SourceTerm.Name;
                            dtInfo.Term.IsAvailableForTagging = tag.Term.IsAvailableForTagging;
                            dtInfo.Term.ParentTermId = term.TermSet.ID;
                            dtInfo.Term.MergedTermIds = term.MergedTermIds;

                            dtInfo.tagExtention = new TagExtention();
                            dtInfo.tagExtention.ParentTermSetName = term.TermSet.Name;
                            dtInfo.tagExtention.TermGroupId = term.TermSet.Group.ID;
                            dtInfo.tagExtention.TermGroupName = term.TermSet.Group.Name;
                            dtInfo.tagExtention.TermStoreId = term.TermStore.ID;
                            dtInfo.tagExtention.TermStoreName = term.TermStore.Name;

                            DTs.Add(dtInfo);
                        }
                        return DTs;
                    }
                    catch (UnauthorizedAccessException e)
                    {
                        mAveParentSite.UserProfileApplicationAvailable = false;
                        mLog.Log(EventSources.DocAveAgentService, EventCategorys.DocAveAgentService.Common_Wrapper, new EventIds.SharePoint.BackupDocumentTagFailedEventMessage(mUrl, e));
                    }
                    catch (Exception e)
                    {
                        if (e.GetType().Name.Equals("UserProfileApplicationNotAvailableException", StringComparison.CurrentCultureIgnoreCase))
                        {
                            mAveParentSite.UserProfileApplicationAvailable = false;
                        }
                        mLog.Log(EventSources.DocAveAgentService, EventCategorys.DocAveAgentService.Common_Wrapper, new EventIds.SharePoint.BackupDocumentTagFailedEventMessage(mUrl, e));
                    }
                }
            }
            return null;
        }

        public void Export(IAveBackupStream stream)
        {
            List<AveSocialTagInfo> tags = GetSocialTags();
            if (tags != null && tags.Count > 0)
            {
                stream.WriteMetadata(AveMetadataType.SocialTag.ToString(), tags);
            }
        }
    }

    public class AveSPSocialComment
    {
        private static AveLogger mLog = AveLogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);
        private AveSPSite mAveParentSite;

        public AveSPSite ParentSite
        {
            get { return mAveParentSite; }
        }

        public string mUrl;

        public AveSPSocialComment(string url, AveSPSite aveSite)
        {
            mUrl = url;
            mAveParentSite = aveSite;
        }

        public List<AveSocialCommentInfo> GetSocialComments()
        {
            if (mAveParentSite.UserProfileApplicationAvailable)
            {
                using (AvePerformanceScope scope = new AvePerformanceScope("Backup.AveSPSocialComment.GetSocialComments"))
                {
                    try
                    {
                        List<AveSocialCommentInfo> commentInfos = new List<AveSocialCommentInfo>();
                        IAveOSocialComment[] comments = this.mAveParentSite.CommentManager.GetComments(mUrl, mAveParentSite.UserProfiles);
                        foreach (IAveOSocialComment comment in comments)
                        {
                            AveSocialCommentInfo commentInfo = new AveSocialCommentInfo();
                            commentInfo.Url = comment.Url.ToString();
                            commentInfo.Comment = comment.Comment;
                            commentInfo.Owner = comment.OwnerName;
                            commentInfo.IsHighPriority = comment.IsHighPriority;
                            commentInfo.Title = comment.Title;
                            commentInfo.LastModifiedTime = comment.LastModifiedTime;
                            commentInfos.Add(commentInfo);
                        }
                        return commentInfos;
                    }
                    catch (UnauthorizedAccessException e)
                    {
                        mLog.Log(EventSources.DocAveAgentService, EventCategorys.DocAveAgentService.Common_Wrapper, new EventIds.SharePoint.BackupDocumentTagFailedEventMessage(mUrl, e));
                        mAveParentSite.UserProfileApplicationAvailable = false;
                    }
                    catch (Exception e)
                    {
                        if (e.GetType().Name.Equals("UserProfileApplicationNotAvailableException", StringComparison.CurrentCultureIgnoreCase))
                        {
                            mAveParentSite.UserProfileApplicationAvailable = false;
                        }
                        mLog.Log(EventSources.DocAveAgentService, EventCategorys.DocAveAgentService.Common_Wrapper, new EventIds.SharePoint.BackupDocumentTagFailedEventMessage(mUrl, e));
                    }
                }
            }
            return null;
        }

        public void Export(IAveBackupStream stream)
        {
            List<AveSocialCommentInfo> comments = GetSocialComments();
            if (comments != null && comments.Count > 0)
            {
                stream.WriteMetadata(AveMetadataType.SocialComment.ToString(), comments);
            }
        }
    }

    internal class AveSPSocialFeed
    {
        private static AveLogger mLog = AveLogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);
        private AveSPSite mSite;
        private string mUrl;

        public AveSPSocialFeed(string webUrl, AveSPSite site)
        {
            mUrl = webUrl;
            mSite = site;
        }

        public List<AveSocialFeedInfo> GetSocialFeeds()
        {
            List<AveSocialFeedInfo> feeds = new List<AveSocialFeedInfo>();

            using (mSite.GetServiceContextScope())
            {
                IAveOSocialFeedOptions options = mSite.ObjectModelFactory.CreateSocialFeedOptions();
                options.SortOrder = AveOSocialFeedSortOrder.ByCreatedTime;
                options.MaxThreadCount = 100;
                options.NewerThan = DateTime.MinValue;
                options.OlderThan = DateTime.Now.ToUniversalTime();
                IAveOSocialFeed sf = null;
                do
                {
                    if ((sf = mSite.FeedManager.GetFeedFor(mUrl, options)) != null)
                    {

                        //We only backup the threads of which the thread type is normal.
                        foreach (IAveOSocialThread t in sf.Threads.Where<IAveOSocialThread>(t => t.ThreadType == AveOSocialThreadType.Normal))
                        {
                            AveSocialFeedInfo feedInfo = new AveSocialFeedInfo();
                            //Use RootPost.Id instead of thread.Id because the thread.Id is not a valid id sometimes. 
                            //When we use GetFeedFor(serverUrl, options), the thread.Id is 'InterimPlaceholderEntity' which is not a valid id actually.
                            IAveOSocialThread threadFull = mSite.FeedManager.GetFullThread(t.RootPost.Id);

                            if (HasFlag((long)threadFull.Attributes, (long)AveOSocialThreadAttributes.CanReply)
                                || HasFlag((long)threadFull.Attributes, (long)AveOSocialThreadAttributes.CanLock))
                            {
                                //backup thread
                                feedInfo.Id = threadFull.Id;
                                feedInfo.OwnerIndex = threadFull.OwnerIndex;
                                feedInfo.Attributes = threadFull.Attributes;
                                feedInfo.Permalink = threadFull.Permalink;
                                feedInfo.ThreadType = threadFull.ThreadType;
                                feedInfo.TotalReplyCount = threadFull.TotalReplyCount;
                                if (threadFull.PostReference != null)
                                {
                                    feedInfo.PostReference = new AveSocialPostReference
                                    {
                                        ThreadId = threadFull.PostReference.ThreadId,
                                        ThreadOwnerIndex = threadFull.PostReference.ThreadOwnerIndex
                                    };
                                }
                                feedInfo.Actors = new AveSocialActorInfo[threadFull.Actors.Count()];
                                for (int i = 0; i < threadFull.Actors.Count(); i++)
                                {
                                    feedInfo.Actors[i] = new AveSocialActorInfo()
                                    {
                                        AccountName = threadFull.Actors[i].AccountName,
                                        ActorType = threadFull.Actors[i].ActorType,
                                        CanFollow = threadFull.Actors[i].CanFollow,
                                        Id = threadFull.Actors[i].Id,
                                        Name = threadFull.Actors[i].Name,
                                        Status = threadFull.Actors[i].Status,
                                        StatusText = threadFull.Actors[i].StatusText,
                                        TagGuid = threadFull.Actors[i].TagGuid,
                                        Uri = threadFull.Actors[i].Uri,
                                        ContentUri = threadFull.Actors[i].ContentUri,
                                        EmailAddress = threadFull.Actors[i].EmailAddress,
                                        FollowedContentUri = threadFull.Actors[i].FollowedContentUri,
                                        ImageUri = threadFull.Actors[i].ImageUri,
                                        IsFollowed = threadFull.Actors[i].IsFollowed,
                                        LibraryUri = threadFull.Actors[i].LibraryUri,
                                        PersonalSiteUri = threadFull.Actors[i].PersonalSiteUri,
                                        Title = threadFull.Actors[i].Title
                                    };
                                }

                                feedInfo.RootPost = GeneratePostInfo(threadFull.RootPost, feedInfo);

                                //backup replies
                                foreach (IAveOSocialPost reply in threadFull.Replies.OrderBy(r => r.CreatedTime))
                                {
                                    feedInfo.Replies.Add(GeneratePostInfo(reply, feedInfo));
                                }

                                //backup the latest two replies' created time.
                                if (t.TotalReplyCount != 0)
                                {
                                    if (feedInfo.LatestTwoReplyTime == null && t.Replies.Count() == 1)
                                    {
                                        feedInfo.LatestTwoReplyTime = new DateTime[1] { t.Replies[0].CreatedTime };
                                    }
                                    else if (feedInfo.LatestTwoReplyTime == null && t.Replies.Count() == 2)
                                    {
                                        feedInfo.LatestTwoReplyTime = new DateTime[2] { t.Replies[0].CreatedTime, t.Replies[1].CreatedTime };
                                    }                                
                                }
                                feeds.Add(feedInfo);
                            }
                        }
                        if (sf.Threads.Count() > 0)
                            options.OlderThan = sf.Threads.Last().RootPost.CreatedTime;
                    }
                }
                while (sf.Threads.Count() > 0);
            }
            return feeds;
        }

        //Get both Post and Reply
        public List<AveSocialFeedInfo> GetSocialFeeds(ref List<AveSocialFeedReplyInfo> feedInfoCacheForArchive)
        {
            List<AveSocialFeedInfo> feeds = new List<AveSocialFeedInfo>();

            using (mSite.GetServiceContextScope())
            {
                IAveOSocialFeedOptions options = mSite.ObjectModelFactory.CreateSocialFeedOptions();
                options.SortOrder = AveOSocialFeedSortOrder.ByCreatedTime;
                options.MaxThreadCount = 100;
                options.NewerThan = DateTime.MinValue;
                options.OlderThan = DateTime.Now.ToUniversalTime();
                IAveOSocialFeed sf = null;
                do
                {
                    if ((sf = mSite.FeedManager.GetFeedFor(mUrl, options)) != null)
                    {

                        //We only backup the threads of which the thread type is normal.
                        foreach (IAveOSocialThread t in sf.Threads.Where<IAveOSocialThread>(t => t.ThreadType == AveOSocialThreadType.Normal))
                        {
                            AveSocialFeedInfo feedInfo = new AveSocialFeedInfo();
                            //Use RootPost.Id instead of thread.Id because the thread.Id is not a valid id sometimes. 
                            //When we use GetFeedFor(serverUrl, options), the thread.Id is 'InterimPlaceholderEntity' which is not a valid id actually.
                            IAveOSocialThread threadFull = mSite.FeedManager.GetFullThread(t.RootPost.Id);

                            if (HasFlag((long)threadFull.Attributes, (long)AveOSocialThreadAttributes.CanReply)
                                || HasFlag((long)threadFull.Attributes, (long)AveOSocialThreadAttributes.CanLock))
                            {
                                //backup thread
                                feedInfo.Id = threadFull.Id;
                                feedInfo.OwnerIndex = threadFull.OwnerIndex;
                                feedInfo.Attributes = threadFull.Attributes;
                                feedInfo.Permalink = threadFull.Permalink;
                                feedInfo.ThreadType = threadFull.ThreadType;
                                feedInfo.TotalReplyCount = threadFull.TotalReplyCount;
                                if (threadFull.PostReference != null)
                                {
                                    feedInfo.PostReference = new AveSocialPostReference
                                    {
                                        ThreadId = threadFull.PostReference.ThreadId,
                                        ThreadOwnerIndex = threadFull.PostReference.ThreadOwnerIndex
                                    };
                                }
                                feedInfo.Actors = new AveSocialActorInfo[threadFull.Actors.Count()];
                                for (int i = 0; i < threadFull.Actors.Count(); i++)
                                {
                                    feedInfo.Actors[i] = new AveSocialActorInfo()
                                    {
                                        AccountName = threadFull.Actors[i].AccountName,
                                        ActorType = threadFull.Actors[i].ActorType,
                                        CanFollow = threadFull.Actors[i].CanFollow,
                                        Id = threadFull.Actors[i].Id,
                                        Name = threadFull.Actors[i].Name,
                                        Status = threadFull.Actors[i].Status,
                                        StatusText = threadFull.Actors[i].StatusText,
                                        TagGuid = threadFull.Actors[i].TagGuid,
                                        Uri = threadFull.Actors[i].Uri,
                                        ContentUri = threadFull.Actors[i].ContentUri,
                                        EmailAddress = threadFull.Actors[i].EmailAddress,
                                        FollowedContentUri = threadFull.Actors[i].FollowedContentUri,
                                        ImageUri = threadFull.Actors[i].ImageUri,
                                        IsFollowed = threadFull.Actors[i].IsFollowed,
                                        LibraryUri = threadFull.Actors[i].LibraryUri,
                                        PersonalSiteUri = threadFull.Actors[i].PersonalSiteUri,
                                        Title = threadFull.Actors[i].Title
                                    };
                                }

                                feedInfo.RootPost = GeneratePostInfo(threadFull.RootPost, feedInfo, true);
                                feedInfo.PostName = feedInfo.Actors[threadFull.RootPost.AuthorIndex].Name;
                                //backup replies
                                foreach (IAveOSocialPost reply in threadFull.Replies.OrderBy(r => r.CreatedTime))
                                {
                                    feedInfo.Replies.Add(GeneratePostInfo(reply, feedInfo, true));
                                    feedInfo.ReplyNames += feedInfo.Actors[reply.AuthorIndex].Name + "#";
                                    string likes = string.Empty;
                                    string mentions = string.Empty;
                                    string tags = string.Empty;
                                    if (reply.Overlays != null)
                                    {
                                        foreach (IAveOSocialDataOverlay overlay in reply.Overlays)
                                        {
                                            IAveOSocialActor actor = threadFull.Actors[overlay.ActorIndexes[0]];
                                            if (actor.ActorType == AveOSocialActorType.User)
                                            {
                                                mentions += actor.Name + "#";
                                            }
                                            else if (actor.ActorType == AveOSocialActorType.Tag)
                                            {
                                                tags += actor.Name + "#";
                                            }
                                        }
                                    }
                                    if (reply.LikerInfo != null)
                                    {
                                        if (reply.LikerInfo.IncludesCurrentUser)
                                        {
                                            likes += feedInfo.Actors[reply.AuthorIndex].Name + "#";
                                        }
                                        foreach (int index in reply.LikerInfo.Indexes)
                                        {
                                            likes += threadFull.Actors[index].Name + "#";
                                        }
                                    }
                                    feedInfoCacheForArchive.Add(new AveSocialFeedReplyInfo() { Id = reply.Id, PostName = feedInfo.Actors[reply.AuthorIndex].Name, Likers = likes, Mentions = mentions, Tags = tags });
                                }
                                #region get post likes mentions and Tags
                                if (threadFull.RootPost.LikerInfo.IncludesCurrentUser)
                                {
                                    feedInfo.Likers += feedInfo.Actors[threadFull.RootPost.AuthorIndex].Name + "#";
                                }
                                foreach (int index in threadFull.RootPost.LikerInfo.Indexes)
                                {
                                    feedInfo.Likers += threadFull.Actors[index].Name + "#";
                                }
                                if (threadFull.RootPost.Overlays != null)
                                {
                                    foreach (IAveOSocialDataOverlay overlay in threadFull.RootPost.Overlays)
                                    {
                                        IAveOSocialActor actor = threadFull.Actors[overlay.ActorIndexes[0]];
                                        if (actor.ActorType == AveOSocialActorType.User)
                                        {
                                            feedInfo.Mentions += actor.Name + "#";
                                        }
                                        else if (actor.ActorType == AveOSocialActorType.Tag)
                                        {
                                            feedInfo.Tags += actor.Name + "#";
                                        }
                                    }
                                }
                                #endregion
                                feeds.Add(feedInfo);
                            }
                        }
                        if (sf.Threads.Count() > 0)
                            options.OlderThan = sf.Threads.Last().RootPost.CreatedTime;
                    }
                }
                while (sf.Threads.Count() > 0);
            }
            return feeds;
        }

        private AveSocialFeedPostInfo GeneratePostInfo(IAveOSocialPost post, AveSocialFeedInfo feedInfo, bool isArchiverJob = false)
        {
            AveSocialFeedPostInfo postInfo = new AveSocialFeedPostInfo
            {
                Attributes = post.Attributes,
                CreatedTime = post.CreatedTime,
                ModifiedTime = post.ModifiedTime,
                Text = post.Text,
                AuthorIndex = post.AuthorIndex,
                PreferredImageUri = post.PreferredImageUri,
                Id = post.Id,
                PostType = post.PostType
            };
            //Backup the liker info of root post.
            if (post.LikerInfo != null)
            {
                if (post.LikerInfo.IncludesCurrentUser)
                {
                    postInfo.Likers.Add(Environment.UserDomainName + "\\" + Environment.UserName);
                }
                foreach (int id in post.LikerInfo.Indexes)
                {
                    postInfo.Likers.Add(feedInfo.Actors[id].AccountName);
                }
            }
            //Backup attachment of the root post
            if (post.Attachment != null)
            {
                postInfo.Attachment = new AveSocialAttachmentInfo
                {
                    AttachmentKind = post.Attachment.AttachmentKind,
                    Description = post.Attachment.Description,
                    Name = post.Attachment.Name,
                    Uri = post.Attachment.Uri,
                };

                if (!isArchiverJob)
                {
                    try
                    {
                        using (var web = mSite.SPSite.OpenWeb())
                        {
                            var tmpFile = web.GetFile(postInfo.Attachment.Uri.ToString());
                            postInfo.Attachment.Content = tmpFile.OpenBinary(AveOpenBinaryOptions.SkipVirusScan);
                        }
                    }
                    catch (Exception ex)
                    {
                        mLog.Warn("Some exception occurred during backing up the attachment content: " + ex.ToString());
                    }
                }
            }
            //Backup Social link
            if (post.Source != null)
            {
                postInfo.Source = new AveSocialLink
                {
                    Uri = post.Source.Uri,
                    Text = post.Source.Text
                };
            }
            //Backup overlays
            if (post.Overlays != null && post.Overlays.Count() > 0)
            {
                for (int i = 0; i < post.Overlays.Count(); i++)
                {
                    postInfo.Overlays.Add(new AveSocialDataOverlay
                    {
                        ActorIndexes = post.Overlays[i].ActorIndexes,
                        Index = post.Overlays[i].Index,
                        Length = post.Overlays[i].Length,
                        LinkUri = post.Overlays[i].LinkUri,
                        OverlayType = post.Overlays[i].OverlayType
                    });
                }
            }

            return postInfo;
        }

        public void Export(IAveBackupStream stream)
        {
            try
            {
                List<AveSocialFeedInfo> feeds = GetSocialFeeds();
                if (feeds != null && feeds.Count > 0)
                {
                    stream.WriteMetadata(AveMetadataType.SocialFeed.ToString(), feeds);
                }
            }
            catch (Exception ex)
            {
                mLog.Warn("Some exception occurred during backing up the social feeds: " + ex.ToString());
            }
        }

        public void ExportSingleFeed(IAveBackupStream stream, object singlefeed)
        {
            try
            {
                AveSocialFeedInfo singleFeedInfo = singlefeed as AveSocialFeedInfo;
                try
                {
                    using (var web = mSite.SPSite.OpenWeb())
                    {
                        AveSocialFeedPostInfo post = singleFeedInfo.RootPost;
                        if (post.Attachment != null)
                        {
                            var tmpFile = web.GetFile(post.Attachment.Uri.ToString());
                            post.Attachment.Content = tmpFile.OpenBinary(AveOpenBinaryOptions.SkipVirusScan);
                        }
                        foreach (AveSocialFeedPostInfo reply in singleFeedInfo.Replies)
                        {
                            if (reply.Attachment != null)
                            {
                                var tmpFile = web.GetFile(reply.Attachment.Uri.ToString());
                                reply.Attachment.Content = tmpFile.OpenBinary(AveOpenBinaryOptions.SkipVirusScan);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    mLog.Warn("Some exception occurred during backing up the attachment content: " + ex.ToString());
                }
                if (singleFeedInfo != null)
                {
                    stream.WriteMetadata(AveMetadataType.SocialThread.ToString(), singleFeedInfo);
                }
            }
            catch (Exception ex)
            {
                mLog.Warn("Some exception occurred during backing up the single social feed: " + ex.ToString());
            }
        }

        private bool HasFlag(long val, long flag)
        {
            return (val & flag) == flag;
        }
    }

    public class AveSPFollowing
    {
        private static AveLogger mLog = AveLogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);
        private AveSPSite mSite;
        private IAveOUserProfile mUserProfile;

        public AveSPFollowing(IAveOUserProfile up, AveSPSite site)
        {
            mSite = site;
            mUserProfile = up;
        }

        public List<AveSocialActorInfo> GetFollowed()
        {
            List<AveSocialActorInfo> followed = new List<AveSocialActorInfo>();

            using (mSite.GetServiceContextScope())
            {
                IAveOSocialFollowingManager followingManager = mSite.ObjectModelFactory.CreateSPSocialFollowingManager(mUserProfile, mSite.ServiceContext);
                IAveOSocialActor[] actors = followingManager.GetFollowed(AveOSocialActorTypes.All);
                foreach (IAveOSocialActor actor in actors)
                {
                    followed.Add(new AveSocialActorInfo()
                        {
                            AccountName = actor.AccountName,
                            ActorType = actor.ActorType,
                            CanFollow = actor.CanFollow,
                            Id = actor.Id,
                            Name = actor.Name,
                            Status = actor.Status,
                            StatusText = actor.StatusText,
                            TagGuid = actor.TagGuid,
                            Uri = actor.Uri,
                            ContentUri = actor.ContentUri,
                            EmailAddress = actor.EmailAddress,
                            FollowedContentUri = actor.FollowedContentUri,
                            ImageUri = actor.ImageUri,
                            IsFollowed = actor.IsFollowed,
                            LibraryUri = actor.LibraryUri,
                            PersonalSiteUri = actor.PersonalSiteUri,
                            Title = actor.Title
                        });
                }
            }

            return followed;
        }
    }
}