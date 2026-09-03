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
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using AvePoint.GCommon;
using AvePoint.Wrapper.Common;
using System.Reflection;
using System.Data;
using System.Data.SqlClient;
using AvePoint.Wrapper.Common.Office;
using AvePoint.Wrapper.Resource.ServerAPI2010;
using AvePoint.Common;
using System.IO;
using System.Threading;
using AvePoint.Wrapper.Resource.SPService;

namespace AvePoint.Wrapper.SPService
{
    public enum SocialDataType
    {
        Tag,
        Comment
    }

    public class AveSocialData : IDisposable
    {
        private static AveLogger mLog = AveLogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);
        protected string mUrl;
        protected AveUserProfile mUserProfile;
        protected AveServiceContext mServiceContext;
        protected IReport mReport = new AveWrapperReport();
        //public static AveSocialData CreateInstance(SocialDataType type, AveSPUserProfile profile, string url)
        //{
        //    switch (type)
        //    {
        //        case SocialDataType.Comment:
        //            return new AveSocialComment(profile.ServiceContext, url);
        //        case SocialDataType .Tag:
        //            return new AveSocialTag(profile.ServiceContext, url);
        //    }
        //    return null;
        //}

        //public static AveSocialData CreateInstance(SocialDataType type, AveSPUserProfile profile)
        //{
        //    switch (type)
        //    {
        //        case SocialDataType.Comment:
        //            return new AveSocialComment(profile);
        //        case SocialDataType.Tag:
        //            return new AveSocialTag(profile);
        //    }
        //    return null;
        //}

        //public static AveSocialData CreateInstance(SocialDataType type, AveServiceContext context, string url)
        //{
        //    switch (type)
        //    {
        //        case SocialDataType.Comment:
        //            return new AveSocialComment(context, url);
        //        case SocialDataType.Tag:
        //            return new AveSocialTag(context, url);
        //    }
        //    return null;
        //}

        //public static AveSocialData CreateInstance(SocialDataType type, AveServiceContext profile)
        //{
        //    switch (type)
        //    {
        //        case SocialDataType.Comment:
        //            return new AveSocialComment(profile);
        //        case SocialDataType.Tag:
        //            return new AveSocialTag(profile);
        //    }
        //    return null;
        //}

        protected AveSocialData(AveServiceContext context, string url)
            : this(context)
        {
            mUrl = url;
        }

        protected AveSocialData(AveServiceContext context)
        {
            mServiceContext = context;
        }

        protected AveSocialData(AveUserProfile userProfile)
        {
            mUserProfile = userProfile;//添加构造函数，满足socialtag和socialComment还原url替换属性的需求；
            mServiceContext = userProfile.ServiceContext;
        }

        public virtual void Restore(IList dtCollection)
        {
            //if (dtCollection != null)
            //{
            //    foreach (AveDocumentTaggingInfo dtInfo in dtCollection)
            //    {
            //        Restore(dtInfo);
            //    }
            //}
        }

        public void SetReport(IReport report)
        {
            mReport = report;
        }

        internal void ChangeUserProfile(string loginName)
        {
            if (!loginName.Equals(mServiceContext.LoginName))
            {
                IAveOUserProfile userProfile = null;
                if (mServiceContext.UserProfileManager.UserExists(loginName))
                {
                    userProfile = mServiceContext.UserProfileManager.GetUserProfile(loginName);
                }
                else
                {
                    userProfile = mServiceContext.UserProfileManager.CreateUserProfile(loginName);
                }
                mServiceContext.SocialTagManager.ProfileLoader.UserProfile = userProfile;
                mServiceContext.LoginName = loginName;
                mServiceContext.UserProfile = userProfile;
            }
        }

        protected bool CheckUserLoginName(string loginName)
        {
            if (loginName.Equals(mServiceContext.OMFactory.AccountInfo.UserName.Trim(), StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
            if (loginName.Equals(mServiceContext.Site.RootWeb.CurrentUser.LoginName, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
            return false;
        }

        internal string GetMappingUser(string login)
        {
            return mServiceContext.GetMappingUser(login);
        }
        public void Dispose()
        {
            mReport.Dispose();
        }
    }

    public class AveSocialComment : AveSocialData
    {
        private static AveLogger log = AveLogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);

        public AveSocialComment(AveServiceContext context, string url)
            : base(context, url)
        { }

        public AveSocialComment(AveServiceContext context)
            : base(context)
        { }

        public AveSocialComment(AveUserProfile userProfile)
            : base(userProfile)
        { }

        public override void Restore(IList dtCollection)
        {
            if (dtCollection != null)
            {
                foreach (AveSocialCommentInfo dtInfo in dtCollection)
                {
                    Restore(dtInfo);
                }
            }
        }

        public void Restore(AveSocialCommentInfo noteInfo)
        {
            string ownerLogin = GetMappingUser(noteInfo.Owner);
            if (string.IsNullOrEmpty(ownerLogin))
            {
                return;
            }
            if (mServiceContext.OMFactory.ContextKind != AveContextKind.ClientObjectModel)
            {
                ChangeUserProfile(ownerLogin);
            }
            //目的端是O365的情况 只支持还原当前User的 Tag&Note
            else if (!CheckUserLoginName(ownerLogin))
            {
                return;
            }
            string url = mUrl;
            if (String.IsNullOrEmpty(mUrl))
            {
                url = noteInfo.Url;
                //sitecollection 级别userprofile service还原需要替换url；ADO-33630；
                if (!string.IsNullOrEmpty(noteInfo.ProfileManagerUrl) && url.StartsWith(noteInfo.ProfileManagerUrl, StringComparison.OrdinalIgnoreCase))
                {
                    url = url.Replace(noteInfo.ProfileManagerUrl, mServiceContext.UserProfileManager.MySiteHostUrl);
                }
                else
                {
                    Dictionary<string, string> absoluteUrlMapping = new Dictionary<string, string>();
                    absoluteUrlMapping[mUserProfile.SourceSiteInfo.Url] = mUserProfile.DestSiteUrl;
                    url = AveReplaceProcessor.UrlReplace(url, absoluteUrlMapping, new ReplaceOption(true, true), mUserProfile.SourceSiteInfo, mUserProfile.DestSiteUrl);
                }
            }
            long recordid = 1;
            Guid userId = Guid.Empty;
            if (mServiceContext.OMFactory.ContextKind != AveContextKind.ClientObjectModel)
            {
                mServiceContext.GetUserProfileCache(ownerLogin, out recordid, out userId);
            }

            Restore(url, noteInfo, recordid, userId);
        }

        public void Restore(string url, AveSocialCommentInfo noteInfo, long recordId, Guid userId)
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.SocialComment"))
            {

            try
            {
                mServiceContext.CommentManager.AddComment(new Uri(url), noteInfo.Comment, noteInfo.IsHighPriority, noteInfo.Title, noteInfo.LastModifiedTime, recordId, userId);
            }
            catch (AveSecurityTrimingException)
            {
                throw;
            }
            catch (Exception e)
            {
                log.Log(AveLogLevel.WARN, WrapperSPServiceResource.AddNoteToTermFailed, url, noteInfo.Comment, e);
                mReport.AddDetail(new AveWrapperReportDto("SocialComment", "SocialComment", AveReportObjectType.SocailComment, AveStatus.Failed, string.Format(WrapperSPServiceResource.AddNoteToTermFailed, url, noteInfo.Comment, e.Message)));
            }

            }

        }
    }

    public class AveSocialTag : AveSocialData
    {
        private static AveLogger log = AveLogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);
        private AveMetadataService mMemadataService;

        public AveSocialTag(AveServiceContext context, string url)
            : base(context, url)
        {

            mMemadataService = new AveMetadataService(context);
        }

        public AveSocialTag(AveServiceContext context)
            : base(context)
        {
            mMemadataService = new AveMetadataService(context);
        }

        public AveSocialTag(AveUserProfile userProfile)
            : base(userProfile)
        {
            mMemadataService = new AveMetadataService(userProfile.ServiceContext);
        }

        public override void Restore(IList dtCollection)
        {
            if (dtCollection != null)
            {
                foreach (AveSocialTagInfo dtInfo in dtCollection)
                {
                    Restore(dtInfo);
                }
            }
        }

        public void Restore(AveSocialTagInfo tagInfo)
        {
            string ownerLogin = GetMappingUser(tagInfo.Owner);
            if (string.IsNullOrEmpty(ownerLogin))
            {
                return;
            }
            if (mServiceContext.OMFactory.ContextKind != AveContextKind.ClientObjectModel)
            {
                ChangeUserProfile(ownerLogin);
            }
            //目的端是O365的情况 只支持还原当前User的 Tag&Note
            else if (!CheckUserLoginName(ownerLogin))
            {
                return;
            }
            string url = mUrl;
            if (string.IsNullOrEmpty(mUrl))
            {
                url = tagInfo.Url;
                //sitecollection 级别userprofile service还原需要替换url；ADO-33630；
                if (!string.IsNullOrEmpty(tagInfo.ProfileManagerUrl) && url.StartsWith(tagInfo.ProfileManagerUrl, StringComparison.OrdinalIgnoreCase))
                {
                    url = url.Replace(tagInfo.ProfileManagerUrl, mServiceContext.UserProfileManager.MySiteHostUrl);
                }
                else
                {
                    Dictionary<string, string> absoluteUrlMapping = new Dictionary<string, string>();
                    absoluteUrlMapping[mUserProfile.SourceSiteInfo.Url] = mUserProfile.DestSiteUrl;
                    url = AveReplaceProcessor.UrlReplace(url, absoluteUrlMapping, new ReplaceOption(true, true), mUserProfile.SourceSiteInfo, mUserProfile.DestSiteUrl);
                }
            }

            Restore(url, ownerLogin, tagInfo);
        }

        public void Restore(string url, string ownerLogin, AveSocialTagInfo tagInfo)//AveTermInfo termInfo, string tagTitle, bool isPrivate, DateTime time)
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.SocialTag"))
            {

            try
            {
                IAveTerm term = mServiceContext.TaxonomySession.GetTerm(tagInfo.Term.Id);
                if (term == null)
                {
                    try
                    {
                        //term = mServiceContext.TermSet.Terms[tagInfo.Term.TermName];
                        term = mServiceContext.TaxonomySession.GetTerms(tagInfo.Term.TermName, false)[0];
                    }
                    catch (AveSecurityTrimingException)
                    {
                        throw;
                    }
                    catch (Exception e)
                    {
                        log.Log(AveLogLevel.DEBUG, WrapperSPServiceResource.GetTermByNameError, e.ToString());
                        term = null;
                    }
                }
                if (term == null)
                {
                    term = mMemadataService.CreateTerm(mServiceContext.TermSet, tagInfo.Term);
                }
                try
                {
                    long recordId = 1;
                    Guid userId = Guid.Empty;
                    if (mServiceContext.OMFactory.ContextKind != AveContextKind.ClientObjectModel)
                    {
                        mServiceContext.GetUserProfileCache(ownerLogin, out recordId, out userId);
                    }
                    mServiceContext.SocialTagManager.DeleteTag(new Uri(url), term);
                    mServiceContext.SocialTagManager.AddTag(new Uri(url), term, tagInfo.Title, tagInfo.IsPrivate, recordId, userId, tagInfo.LastModifiedTime);
                }
                catch (AveSecurityTrimingException)
                {
                    throw;
                }
                catch (Exception e)
                {
                    log.Warn("Restore term failed while add tag with url:" + url + "\n Title:" + tagInfo.Title + ". Error: " + e.ToString());
                    mReport.AddDetail(new AveWrapperReportDto("SocialTag", "SocialTag", AveReportObjectType.SocialTag, AveStatus.Failed, AveReportResource.Wrapper_Report_RestoreTermFailed, url, tagInfo.Title, e.Message));
                }
            }
            catch (AveSecurityTrimingException)
            {
                throw;
            }
            catch (Exception e)
            {
                log.Warn("Error while restore DocumentTagging" + e.ToString());
                mReport.AddDetail(new AveWrapperReportDto("SocialTag", "SocialTag", AveReportObjectType.SocialTag, AveStatus.Failed, AveReportResource.Wrapper_Report_RestoreDocumentTaggingError, e.Message));
            }

            }

        }
    }

    public class AveSocialFeed : AveSocialData
    {
        private static AveLogger log = AveLogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);
        private IAveOUserProfileManager mUserProfileManager;
        private AveObjectModelFactory mAOMFactory;
        private IAveOUserProfile mCurrentUserProfile;
        private string mCurrentAccountName;
        private const int mRetry = 3;

        public AveSocialFeed(AveUserProfile profile,AveObjectModelFactory mFactory)
            : base(profile)
        {
            mUrl = profile.DestSiteUrl;
            mAOMFactory = AveObjectModelFactory.CreateObjectModelFactory(profile.DestSiteUrl, mFactory.AccountInfo, mFactory.ContextKind);
            mUserProfileManager = mAOMFactory.CreateUserProfileManager(mServiceContext.ServiceContext);
        }
        public AveSocialFeed(AveServiceContext context, string url, AveObjectModelFactory mFactory)
            : base(context, url)
        {
            mAOMFactory = AveObjectModelFactory.CreateObjectModelFactory(url, mFactory.AccountInfo, mFactory.ContextKind);
            mUserProfileManager = mAOMFactory.CreateUserProfileManager(mServiceContext.ServiceContext);
        }

        public AveSocialFeed(AveServiceContext context, string url, AveBPOSAccountInfo accountInfo, AveContextKind contextKind)
            : base(context, url)
        {
            mAOMFactory = AveObjectModelFactory.CreateObjectModelFactory(url, accountInfo, contextKind);
            mAOMFactory.CreateSite();
            //mUserProfileManager = mAOMFactory.CreateUserProfileManager(mServiceContext.ServiceContext);
            //mCurrentUserProfile = mUserProfileManager.GetUserProfile(Environment.UserDomainName + "\\" + Environment.UserName);//clinet 现不支持userProfile故注掉
        }
        //This is not a public one
        protected AveSocialFeed(AveServiceContext context)
            : base(context)
        {
        }

        public void Restore(List<AveSocialFeedInfo> feeds)
        {
            if (feeds == null || feeds.Count == 0 || mServiceContext == null || (mUserProfile == null && string.IsNullOrEmpty(mUrl)))
            {
                log.Debug("There is no feed to restore.");
                return;
            }

            try
            {
                feeds.Reverse(); //先备份的后还原
                using (mAOMFactory.CreateServiceContextScope(mServiceContext.ServiceContext))
                {
                    #region Get the existing feeds in destination

                    IAveOSocialFeed existingFeed = null;

                    try
                    {
                        IAveOSocialFeedManager dtManager = mAOMFactory.CreateSocialFeedManager();
                        //If this is a personal site, we should get feed using the account name.
                        //If this is a normal site, we should use the web url to get the feeds which belongs to this web exactly.
                        existingFeed = dtManager.GetFeedFor(mUrl, mAOMFactory.CreateSocialFeedOptions());
                    }
                    catch (System.Exception ex)
                    {
                        log.Warn("Exception occurred when trying to get the existing feeds: " + ex.ToString());
                    }

                    #endregion

                    #region Real restore posts
                    IAveOSocialFeedManager fdManager = null;
                    mCurrentAccountName = string.Empty;
                    foreach (AveSocialFeedInfo feedInfo in feeds)
                    {
                        //Check if we need to change the feed manager according to the author 
                        fdManager = ChangeFeedManager(feedInfo.Actors[feedInfo.RootPost.AuthorIndex].AccountName, fdManager);

                        string existingThreadId = CheckConflictThread(existingFeed, feedInfo);
                        IAveOSocialThread st = null;
                        bool bExisted = false;
                        //If the thread is already existing in the destination, we won't create a duplicated one.
                        if (!string.IsNullOrEmpty(existingThreadId))
                        {
                            st = fdManager.GetFullThread(feedInfo.Id);
                            RestoreLikerInfo(feedInfo.RootPost.Likers, st, st.RootPost, ref fdManager);
                            bExisted = true;
                        }
                        else
                        {
                            //////////////////////////////////////////////////////////////////////////
                            // Sometimes it will throw the exception as following
                            // "System.NotSupportedException: Unsupported principal type 'GenericPrincipal'. Ensure that Claims authentication mode is enabled."
                            // But when we try again, the exception disappeared.
                            // So we will retry this operation for 3 times in 1s time span.
                            //////////////////////////////////////////////////////////////////////////
                            int retry = 0;
                            log.Debug(string.Format("if some exception occurred, we will retry {0} times.", mRetry));
                            while (retry < mRetry)
                            {
                                try
                                {
                                    st = RestoreSinglePost(feedInfo, feedInfo.RootPost, ref fdManager, null, bExisted);
                                    break;
                                }
                                catch (Exception ex)
                                {
                                    retry++;
                                    if (retry < mRetry)
                                    {
                                        log.Debug(string.Format("Some exception occurred because of {0}. \r\n Try again No.{1}", ex.ToString(), retry));
                                    }
                                    else
                                    {
                                        log.Warn(string.Format("Exception occurred when restore single post : {0}", ex.ToString()));
                                    }
                                    Thread.Sleep(1000);
                                }
                            }
                        }

                        //restore reply
                        if (st != null)
                        {
                            foreach (AveSocialFeedPostInfo reply in feedInfo.Replies.OrderBy(r => r.CreatedTime))
                            {
                                RestoreSinglePost(feedInfo, reply, ref fdManager, st, bExisted);
                            }
                        }
                    }
                    #endregion
                }
            }
            catch (Exception e)
            {
                log.Log(AveLogLevel.WARN, "An error occurred while restoring the news feed", e.ToString());
                mReport.AddDetail(new AveWrapperReportDto("SocialFeed", "SocialFeed", AveReportObjectType.SocialFeed, AveStatus.Failed, AveReportResource.Wrapper_Report_RestoreSocialFeedError, e.Message));
            }
        }

        public void RestoreForArchiver(List<AveSocialFeedInfo> feeds)
        {
            if (feeds == null || feeds.Count == 0 || mServiceContext == null || (mUserProfile == null && string.IsNullOrEmpty(mUrl)))
            {
                log.Debug("There is no feed to restore.");
                return;
            }

            try
            {
                using (mAOMFactory.CreateServiceContextScope(mServiceContext.ServiceContext))
                {
                    #region Get the existing feeds in destination

                    IAveOSocialThread[] existingThreads = null;
                    try
                    {
                        IAveOSocialFeedManager dtManager = mAOMFactory.CreateSocialFeedManager();
                        //If this is a personal site, we should get feed using the account name.
                        //If this is a normal site, we should use the web url to get the feeds which belongs to this web exactly.
                        IAveOSocialFeedOptions options = mAOMFactory.CreateSocialFeedOptions();
                        options.MaxThreadCount = int.MaxValue;
                        options.SortOrder = AveOSocialFeedSortOrder.ByCreatedTime;
                        options.NewerThan = DateTime.MinValue;
                        options.OlderThan = DateTime.Now.ToUniversalTime().AddMinutes(1);
                        IAveOSocialFeed tempFeed;
                        List<IAveOSocialThread> tempThreads = new List<IAveOSocialThread>();
                        do
                        {
                            tempFeed = dtManager.GetFeedFor(mUrl, options);
                            foreach (IAveOSocialThread thread in tempFeed.Threads)
                            {
                                tempThreads.Add(thread);
                            }
                            if (tempFeed.Threads.Count() != 0)
                            {
                                options.OlderThan = tempFeed.Threads.Last().RootPost.CreatedTime;
                            }
                        }
                        while (tempFeed != null && tempFeed.Threads.Count() != 0);
                        existingThreads = tempThreads.ToArray();
                    }
                    catch (System.Exception ex)
                    {
                        log.Warn("Exception occurred when trying to get the existing feeds: " + ex.ToString());
                    }

                    #endregion

                    #region Real restore posts
                    IAveOSocialFeedManager fdManager = null;
                    mCurrentAccountName = string.Empty;
                    foreach (AveSocialFeedInfo feedInfo in feeds)
                    {
                        //Check if we need to change the feed manager according to the author 
                        fdManager = ChangeFeedManagerForArchiver(feedInfo.Actors[feedInfo.RootPost.AuthorIndex].AccountName, fdManager);

                        string existingThreadId = CheckConflictThreadForArchiver(existingThreads, feedInfo);
                        IAveOSocialThread st = null;
                        bool bExisted = false;
                        //If the thread is already existing in the destination, we won't create a duplicated one.
                        if (!string.IsNullOrEmpty(existingThreadId))
                        {
                            if ((feedInfo.Attributes & AveOSocialThreadAttributes.IsLocked) != AveOSocialThreadAttributes.IsLocked)
                            {
                                fdManager.UnlockThread(existingThreadId);
                            }
                            st = fdManager.GetFullThread(existingThreadId);
                            RestoreLikerInfoForArchiver(feedInfo.RootPost.Likers, st, st.RootPost, ref fdManager);
                            bExisted = true;
                        }
                        else
                        {
                            //////////////////////////////////////////////////////////////////////////
                            // Sometimes it will throw the exception as following
                            // "System.NotSupportedException: Unsupported principal type 'GenericPrincipal'. Ensure that Claims authentication mode is enabled."
                            // But when we try again, the exception disappeared.
                            // So we will retry this operation for 3 times in 1s time span.
                            //////////////////////////////////////////////////////////////////////////
                            int retry = 0;
                            log.Debug(string.Format("if some exception occurred, we will retry {0} times.", mRetry));
                            while (retry < mRetry)
                            {
                                try
                                {
                                    st = RestoreSinglePostForArchiver(feedInfo, feedInfo.RootPost, ref fdManager, null, bExisted);
                                    break;
                                }
                                catch (Exception ex)
                                {
                                    retry++;
                                    if (retry < mRetry)
                                    {
                                        log.Debug(string.Format("Some exception occurred because of {0}. \r\n Try again No.{1}", ex.ToString(), retry));
                                    }
                                    else
                                    {
                                        log.Warn(string.Format("Exception occurred when restore single post : {0}", ex.ToString()));
                                    }
                                    Thread.Sleep(1000);
                                }
                            }
                        }

                        //restore reply
                        if (st != null)
                        {
                            foreach (AveSocialFeedPostInfo reply in feedInfo.Replies.OrderBy(r => r.CreatedTime))
                            {
                                RestoreSinglePostForArchiver(feedInfo, reply, ref fdManager, st, bExisted);
                            }
                        }

                        if ((feedInfo.Attributes & AveOSocialThreadAttributes.IsLocked) == AveOSocialThreadAttributes.IsLocked)
                        {
                            fdManager.LockThread(st.Id);
                        }
                    }
                    #endregion
                }
            }
            catch (Exception e)
            {
                log.Log(AveLogLevel.WARN, "An error occurred while restoring the news feed", e.ToString());
                mReport.AddDetail(new AveWrapperReportDto("SocialFeed", "SocialFeed", AveReportObjectType.SocialFeed, AveStatus.Failed, AveReportResource.Wrapper_Report_RestoreSocialFeedError, e.Message));
            }
        }

        /// <summary>
        /// Restore likers according to the account names which we backed up.
        /// </summary>
        /// <param name="likers"></param>
        /// <param name="thread"></param>
        /// <param name="post"></param>
        /// <param name="factory"></param>
        /// <param name="pManager"></param>
        /// <param name="context"></param>
        private void RestoreLikerInfo(List<string> likers, IAveOSocialThread thread, IAveOSocialPost post, ref IAveOSocialFeedManager fManager)
        {
            using (AvePerformanceScope scope = new AvePerformanceScope("RestoreLikerInfo"))
            {
                if (likers.Count <= 0 || post == null)
                {
                    return;
                }

                if (thread != null && post != null && post.LikerInfo != null &&
                    post.LikerInfo.Indexes != null && post.LikerInfo.Indexes.Length > 0)
                {
                    foreach (int i in post.LikerInfo.Indexes)
                    {
                        if (likers.Contains(thread.Actors[i].AccountName))
                        {
                            likers.Remove(thread.Actors[i].AccountName);
                        }
                    }
                }
                if (thread != null && post != null && post.LikerInfo != null && post.LikerInfo.IncludesCurrentUser && likers.Where(s => s.Equals(mCurrentAccountName, StringComparison.OrdinalIgnoreCase)) != null)
                {
                    likers.Remove(likers.Where(s => s.Equals(mCurrentAccountName, StringComparison.OrdinalIgnoreCase)).Single());
                }

                foreach (string liker in likers)
                {
                    fManager = ChangeFeedManager(liker, fManager);
                    fManager.LikePost(post.Id);
                }
            }
        }

        private void RestoreLikerInfoForArchiver(List<string> likers, IAveOSocialThread thread, IAveOSocialPost post, ref IAveOSocialFeedManager fManager)
        {
            using (AvePerformanceScope scope = new AvePerformanceScope("RestoreLikerInfo"))
            {
                if (likers.Count <= 0 || post == null)
                {
                    return;
                }

                if (thread != null && post != null && post.LikerInfo != null &&
                    post.LikerInfo.Indexes != null && post.LikerInfo.Indexes.Length > 0)
                {
                    foreach (int i in post.LikerInfo.Indexes)
                    {
                        if (likers.Contains(thread.Actors[i].AccountName))
                        {
                            likers.Remove(thread.Actors[i].AccountName);
                        }
                    }
                }
                if (thread != null && post != null && post.LikerInfo != null && post.LikerInfo.IncludesCurrentUser && likers.Contains(mCurrentAccountName))
                {
                    likers.Remove(mCurrentAccountName);
                }

                foreach (string liker in likers)
                {
                    fManager = ChangeFeedManagerForArchiver(liker, fManager);
                    fManager.LikePost(post.Id);
                }
            }
        }

        /// <summary>
        /// Change feed manager according to the account name.
        /// </summary>
        /// <param name="accountName"></param>
        /// <returns></returns>
        private IAveOSocialFeedManager ChangeFeedManager(string accountName, IAveOSocialFeedManager feedManager, bool bForce = false)
        {
            if (!mCurrentAccountName.Equals(accountName, StringComparison.OrdinalIgnoreCase) || feedManager == null || bForce)
            {
                log.Debug(string.Format("Change feed manager according to the account name. The original user is {0}; current user is {1}", mCurrentAccountName, accountName));
                mCurrentAccountName = accountName;
                IAveOUserProfile profile = mUserProfileManager == null ? null : mUserProfileManager.GetUserProfile(accountName);
                return mAOMFactory.CreateSocialFeedManager(profile, mServiceContext.ServiceContext);
            }
            else
            {
                log.Debug("Use the original feed manager object.");
                return feedManager;
            }
        }

        private IAveOSocialFeedManager ChangeFeedManagerForArchiver(string accountName, IAveOSocialFeedManager feedManager, bool bForce = false)
        {
            if (!mCurrentAccountName.Equals(accountName, StringComparison.OrdinalIgnoreCase) || feedManager == null || bForce)
            {
                log.Debug(string.Format("Change feed manager according to the account name. The original user is {0}; current user is {1}", mCurrentAccountName, accountName));
                mCurrentAccountName = accountName;
                IAveOUserProfile profile = mUserProfileManager == null ? null : mUserProfileManager.GetUserProfile(accountName);
                return mAOMFactory.CreateSocialFeedManager(profile, mServiceContext.ServiceContext);
            }
            else
            {
                log.Debug("Use the original feed manager object.");
                return feedManager;
            }
        }

        /// <summary>
        /// Restore a single post (both root and replies)
        /// </summary>
        /// <param name="feedInfo"></param>
        /// <param name="postInfo"></param>
        /// <param name="parentThread"></param>
        /// <param name="feedManager"></param>
        /// <param name="isExisting"></param>
        /// <returns></returns>
        private IAveOSocialThread RestoreSinglePost(AveSocialFeedInfo feedInfo, AveSocialFeedPostInfo postInfo, ref IAveOSocialFeedManager fdManager, IAveOSocialThread parentThread, bool isExisting)
        {
            using (AvePerformanceScope scope = new AvePerformanceScope("RestoreSinglePost"))
            {
                IAveOSocialThread thread = null;
                fdManager = ChangeFeedManager(feedInfo.Actors[postInfo.AuthorIndex].AccountName, fdManager);

                #region Restore the existing replies
                if (isExisting)
                {
                    var existingReplies = parentThread.Replies.Where(r => (parentThread.Actors[r.AuthorIndex].AccountName.Equals(mCurrentAccountName, StringComparison.OrdinalIgnoreCase) &&
                        r.Text.Equals(postInfo.Text, StringComparison.Ordinal)));

                    if (existingReplies.Count() > 0)
                    {
                        foreach (IAveOSocialPost reply in existingReplies)
                        {
                            RestoreLikerInfo(postInfo.Likers, parentThread, reply, ref fdManager);
                        }

                        return null;
                    }
                }
                #endregion

                #region Restore the new post (rootpost and replies)
                IAveOSocialPostCreationData postData = mAOMFactory.CreateSocialPostCreationData();

                if (postInfo.Overlays != null && postInfo.Overlays.Count > 0)
                {
                    try
                    {
                        string content = postInfo.Text;
                        int count = postInfo.Overlays.Count;
                        IAveOSocialDataItem[] contentItems = mAOMFactory.CreateSocialDataItemCollection(count);
                        for (int i = count - 1; i >= 0; i--)
                        {
                            IAveOSocialDataItem dataItem = mAOMFactory.CreateSocialDataItem();
                            AveSocialDataOverlay overlay = postInfo.Overlays[i];
                            int index = overlay.Index;
                            int length = overlay.Length;
                            if (overlay.OverlayType == AveOSocialDataOverlayType.Link)
                            {
                                dataItem.Text = content.Substring(index, length);
                                dataItem.Uri = overlay.LinkUri;
                                dataItem.ItemType = AveOSocialDataItemType.Link;
                                content = content.Substring(0, index) + "{" + i + "}" + content.Substring(index + length);
                            }
                            else //AveOSocialDataOverlayType.Actors
                            {
                                int actorIndex = overlay.ActorIndexes[0];

                                AveSocialActorInfo actorInfo = feedInfo.Actors[actorIndex];
                                switch (actorInfo.ActorType)
                                {
                                    case AveOSocialActorType.User:
                                        dataItem.AccountName = actorInfo.AccountName;
                                        dataItem.ItemType = AveOSocialDataItemType.User;
                                        content = content.Substring(0, index) + "{" + i + "}" + content.Substring(index + length);
                                        break;
                                    case AveOSocialActorType.Tag:
                                        dataItem.TagGuid = actorInfo.TagGuid; //to do change the id
                                        dataItem.Text = actorInfo.Name;
                                        dataItem.ItemType = AveOSocialDataItemType.Tag;
                                        content = content.Substring(0, index) + "{" + i + "}" + content.Substring(index + length);
                                        break;
                                    case AveOSocialActorType.Site:

                                        break;
                                    case AveOSocialActorType.Document:

                                        break;
                                    default:
                                        break;
                                }

                            }

                            contentItems[i] = dataItem;
                        }
                        postData.ContentItems = contentItems;
                        postData.ContentText = content;
                    }
                    catch (Exception e) //如果出错就按照原来的逻辑Post纯文本
                    {
                        log.Debug("An error occurred while restore post and use previous logic to restore post. Error message:{0}.", e.ToString());
                        postData.ContentItems = null;
                        postData.ContentText = postInfo.Text;
                    }
                }
                else
                {
                    postData.ContentText = postInfo.Text;
                }

                #region Restore Attachment in 2 methods. By default, we use the API to create the SPSocialAttachment object. We can also use the reflection to upload the attachment to my site host temporarily.
                if (postInfo.Attachment != null)
                {
                    if (postInfo.Attachment.AttachmentKind == AveOSocialAttachmentKind.Image && postInfo.Attachment.Content != null)
                    {
                        postData.Attachment = fdManager.CreateImageAttachment(postInfo.Attachment.Name, postInfo.Attachment.Description, new MemoryStream(postInfo.Attachment.Content));
                    }
                    else
                    {
                        IAveOSocialAttachment att = mAOMFactory.CreateSocialAttachment();
                        att.AttachmentKind = postInfo.Attachment.AttachmentKind;
                        att.Description = postInfo.Attachment.Description;

                        mCurrentUserProfile = mUserProfileManager.GetUserProfile(feedInfo.Actors[postInfo.AuthorIndex].AccountName);
                        string[] paths = mCurrentUserProfile.SaveTempFile(postInfo.Attachment.Content, postInfo.Attachment.Name);

                        if (paths != null && paths.Length >= 3)
                        {
                            att.Uri = new Uri(paths[0]);
                            att.Name = paths[2];
                        }
                        else
                        {
                            att.Uri = postInfo.Attachment.Uri;
                            att.Name = postInfo.Attachment.Name;
                        }

                        if (att.Uri != null)
                        {
                            postData.Attachment = att;
                        }
                    }
                }
                #endregion

                string targetId = parentThread == null ? (mUserProfile == null ? mUrl : null) : parentThread.Id;
                if (mAOMFactory.ContextKind.IsServerMode13Upper()
                    || mAOMFactory.AccountInfo.UserName.Equals(feedInfo.Actors[postInfo.AuthorIndex].AccountName, StringComparison.OrdinalIgnoreCase))
                {
                    thread = fdManager.CreatePost(targetId, postData);
                }
                ////This is a new post. So we don't need to check the existing likers. Just set the thread and post to null.
                //RestoreLikerInfo(postInfo.Likers, null, null, fdManager);
                //This is a new post. We need to check whether it's a rootpost or reply so that all the post is restored correctly.
                IAveOSocialPost newCreatedPost = null;
                if (parentThread == null)
                {
                    newCreatedPost = thread.RootPost;
                }
                else
                {
                    newCreatedPost = thread.Replies.First();
                }
                RestoreLikerInfo(postInfo.Likers, null, newCreatedPost, ref fdManager);
                #endregion

                return thread;
            }
        }

        private IAveOSocialThread RestoreSinglePost(AveSocialFeedInfo feedInfo, AveSocialFeedPostInfo postInfo, ref IAveOSocialFeedManager fdManager, IAveOSocialThread parentThread, bool isExisting, ref Dictionary<string, DateTime> modifiedTimeCache, ref Dictionary<string, DateTime> createdTimeCache)
        {
            using (AvePerformanceScope scope = new AvePerformanceScope("RestoreSinglePost"))
            {
                IAveOSocialThread thread = null;
                fdManager = ChangeFeedManager(feedInfo.Actors[postInfo.AuthorIndex].AccountName, fdManager);

                #region Restore the existing replies
                if (isExisting)
                {
                    var existingReplies = parentThread.Replies.Where(r => (parentThread.Actors[r.AuthorIndex].AccountName.Equals(mCurrentAccountName, StringComparison.OrdinalIgnoreCase) &&
                ((r.Attachment != null && postInfo.Attachment != null && r.Attachment.Name.Equals(postInfo.Attachment.Name, StringComparison.Ordinal) &&
                (r.Text != null && postInfo.Text != null && r.Text.Equals(postInfo.Text, StringComparison.Ordinal) || r.Text == null && postInfo.Text == null)) ||
                r.Attachment == null && postInfo.Attachment == null && r.Text != null && postInfo.Text != null && r.Text.Equals(postInfo.Text, StringComparison.Ordinal)) && r.CreatedTime.CompareTo(postInfo.CreatedTime) == 0));

                    if (existingReplies.Count() > 0)
                    {
                        foreach (IAveOSocialPost reply in existingReplies)
                        {
                            RestoreLikerInfo(postInfo.Likers, parentThread, reply, ref fdManager);
                        }

                        return null;
                    }
                }
                #endregion

                #region Restore the new post (rootpost and replies)
                IAveOSocialPostCreationData postData = mAOMFactory.CreateSocialPostCreationData();

                if (postInfo.Overlays != null && postInfo.Overlays.Count > 0)
                {
                    try
                    {
                        string content = postInfo.Text;
                        int count = postInfo.Overlays.Count;
                        IAveOSocialDataItem[] contentItems = mAOMFactory.CreateSocialDataItemCollection(count);
                        for (int i = count - 1; i >= 0; i--)
                        {
                            IAveOSocialDataItem dataItem = mAOMFactory.CreateSocialDataItem();
                            AveSocialDataOverlay overlay = postInfo.Overlays[i];
                            int index = overlay.Index;
                            int length = overlay.Length;
                            if (overlay.OverlayType == AveOSocialDataOverlayType.Link)
                            {
                                dataItem.Text = content.Substring(index, length);
                                dataItem.Uri = overlay.LinkUri;
                                dataItem.ItemType = AveOSocialDataItemType.Link;
                                content = content.Substring(0, index) + "{" + i + "}" + content.Substring(index + length);
                            }
                            else //AveOSocialDataOverlayType.Actors
                            {
                                int actorIndex = overlay.ActorIndexes[0];

                                AveSocialActorInfo actorInfo = feedInfo.Actors[actorIndex];
                                switch (actorInfo.ActorType)
                                {
                                    case AveOSocialActorType.User:
                                        dataItem.AccountName = actorInfo.AccountName;
                                        dataItem.ItemType = AveOSocialDataItemType.User;
                                        content = content.Substring(0, index) + "{" + i + "}" + content.Substring(index + length);
                                        break;
                                    case AveOSocialActorType.Tag:
                                        dataItem.TagGuid = actorInfo.TagGuid; //to do change the id
                                        dataItem.Text = actorInfo.Name;
                                        dataItem.ItemType = AveOSocialDataItemType.Tag;
                                        content = content.Substring(0, index) + "{" + i + "}" + content.Substring(index + length);
                                        break;
                                    case AveOSocialActorType.Site:

                                        break;
                                    case AveOSocialActorType.Document:

                                        break;
                                    default:
                                        break;
                                }

                            }

                            contentItems[i] = dataItem;
                        }
                        postData.ContentItems = contentItems;
                        postData.ContentText = content;
                    }
                    catch (Exception e) //如果出错就按照原来的逻辑Post纯文本
                    {
                        log.Debug("An error occurred while restore post and use previous logic to restore post. Error message:{0}.", e.ToString());
                        postData.ContentItems = null;
                        postData.ContentText = postInfo.Text;
                    }
                }
                else
                {
                    postData.ContentText = postInfo.Text;
                }

                #region Restore Attachment in 2 methods. By default, we use the API to create the SPSocialAttachment object. We can also use the reflection to upload the attachment to my site host temporarily.
                if (postInfo.Attachment != null)
                {
                    if (postInfo.Attachment.AttachmentKind == AveOSocialAttachmentKind.Image && postInfo.Attachment.Content != null)
                    {
                        postData.Attachment = fdManager.CreateImageAttachment(postInfo.Attachment.Name, postInfo.Attachment.Description, new MemoryStream(postInfo.Attachment.Content));
                    }
                    else
                    {
                        IAveOSocialAttachment att = mAOMFactory.CreateSocialAttachment();
                        att.AttachmentKind = postInfo.Attachment.AttachmentKind;
                        att.Description = postInfo.Attachment.Description;

                        mCurrentUserProfile = mUserProfileManager.GetUserProfile(feedInfo.Actors[postInfo.AuthorIndex].AccountName);
                        string[] paths = mCurrentUserProfile.SaveTempFile(postInfo.Attachment.Content, postInfo.Attachment.Name);

                        if (paths != null && paths.Length >= 3)
                        {
                            att.Uri = new Uri(paths[0]);
                            att.Name = paths[2];
                        }
                        else
                        {
                            att.Uri = postInfo.Attachment.Uri;
                            att.Name = postInfo.Attachment.Name;
                        }

                        if (att.Uri != null)
                        {
                            postData.Attachment = att;
                        }
                    }
                }
                #endregion

                string targetId = parentThread == null ? (mUserProfile == null ? mUrl : null) : parentThread.Id;
                if (mAOMFactory.ContextKind.IsServerMode13Upper()
                    || mAOMFactory.AccountInfo.UserName.Equals(feedInfo.Actors[postInfo.AuthorIndex].AccountName, StringComparison.OrdinalIgnoreCase))
                {
                    thread = fdManager.CreatePost(targetId, postData);
                }
                ////This is a new post. So we don't need to check the existing likers. Just set the thread and post to null.
                //RestoreLikerInfo(postInfo.Likers, null, null, fdManager);
                //This is a new post. We need to check whether it's a rootpost or reply so that all the post is restored correctly.
                IAveOSocialPost newCreatedPost = null;
                if (parentThread == null)
                {
                    newCreatedPost = thread.RootPost;
                }
                else
                {
                    newCreatedPost = thread.Replies.First();
                }
                RestoreLikerInfo(postInfo.Likers, null, newCreatedPost, ref fdManager);
                #endregion
                if (modifiedTimeCache == null || createdTimeCache == null)
                {
                    modifiedTimeCache = new Dictionary<string, DateTime>();
                    createdTimeCache = new Dictionary<string, DateTime>();
                }
                if (!modifiedTimeCache.ContainsKey(newCreatedPost.Id) && !createdTimeCache.ContainsKey(newCreatedPost.Id))
                {
                    modifiedTimeCache.Add(Convert.ToString(newCreatedPost.Id.Split('.')[7]), postInfo.ModifiedTime);
                    createdTimeCache.Add(Convert.ToString(newCreatedPost.Id.Split('.')[7]), postInfo.CreatedTime);
                }
                return thread;
            }
        }

        private IAveOSocialThread RestoreSinglePostForPR(AveSocialFeedInfo feedInfo, AveSocialFeedPostInfo postInfo, ref IAveOSocialFeedManager fdManager, IAveOSocialThread parentThread, bool isExisting, ref Dictionary<string, DateTime> modifiedTimeCache, ref Dictionary<string, DateTime> createdTimeCache, Dictionary<string, string> allTags)
        {
            using (AvePerformanceScope scope = new AvePerformanceScope("RestoreSinglePost"))
            {
                IAveOSocialThread thread = null;
                fdManager = ChangeFeedManager(feedInfo.Actors[postInfo.AuthorIndex].AccountName, fdManager);

                #region Restore the existing replies
                if (isExisting)
                {
                    var existingReplies = parentThread.Replies.Where(r => (parentThread.Actors[r.AuthorIndex].AccountName.Equals(mCurrentAccountName, StringComparison.OrdinalIgnoreCase) &&
                ((r.Attachment != null && postInfo.Attachment != null && r.Attachment.Name.Equals(postInfo.Attachment.Name, StringComparison.Ordinal) &&
                (r.Text != null && postInfo.Text != null && r.Text.Equals(postInfo.Text, StringComparison.Ordinal) || r.Text == null && postInfo.Text == null)) ||
                r.Attachment == null && postInfo.Attachment == null && r.Text != null && postInfo.Text != null && r.Text.Equals(postInfo.Text, StringComparison.Ordinal)) && r.CreatedTime.CompareTo(postInfo.CreatedTime) == 0));

                    if (existingReplies.Count() > 0)
                    {
                        foreach (IAveOSocialPost reply in existingReplies)
                        {
                            RestoreLikerInfo(postInfo.Likers, parentThread, reply, ref fdManager);
                        }

                        return null;
                    }
                }
                #endregion

                #region Restore the new post (rootpost and replies)
                IAveOSocialPostCreationData postData = mAOMFactory.CreateSocialPostCreationData();

                #region convert the post text to the correct format.
                if (postInfo.Text.Contains('#') || postInfo.Text.Contains('@'))
                {
                    string text = postInfo.Text;
                    const int currentPosition = 0;
                    int i = 0;
                    Dictionary<string, string> dic = new Dictionary<string, string>();
                    List<IAveOSocialDataItem> dataItemCollection = new List<IAveOSocialDataItem>();

                    StringBuilder result = new StringBuilder();
                    while (!string.IsNullOrEmpty(text))
                    {
                        int index = text.IndexOfAny(new char[] { '@', '#' }, currentPosition);
                        if (index != -1)
                        {
                            string subForAppend = text.Substring(currentPosition, index);
                            string subForIndex = text.Substring(index);

                            if (text[index] == '@')
                            {
                                result.Append(subForAppend + "@" + "{" + i.ToString() + "}");
                                dic.Add(string.Format("User|{0}", i), subForIndex.Substring(1, subForIndex.IndexOf('|')));
                            }
                            else if (text[index] == '#')
                            {
                                result.Append(subForAppend + "#" + '{' + i.ToString() + '}');
                                dic.Add(string.Format("Tag|{0}", i), subForIndex.Substring(0, subForIndex.IndexOf('|')));
                            }
                            text = subForIndex.Substring(subForIndex.IndexOf('|') + 1);
                            i++;
                        }
                        else
                        {
                            text = null;
                        }

                    }
                    foreach (KeyValuePair<string, string> item in dic)
                    {
                        if (item.Key.StartsWith("User", StringComparison.OrdinalIgnoreCase))
                        {
                            string loginName;
                            if (!WrapperRuntime.CurrentContext.MappingManager.SiteMappingManager.TryGetValueFromUserLoginNameMapping(item.Key, out loginName))
                            {
                                loginName = item.Key;
                            }
                            IAveOSocialDataItem dataItem = mAOMFactory.CreateSocialDataItem();
                            dataItem.ItemType = AveOSocialDataItemType.User;
                            dataItem.AccountName = loginName;
                            dataItemCollection.Add(dataItem);
                        }
                        else if (item.Key.StartsWith("Tag", StringComparison.OrdinalIgnoreCase))
                        {
                            IAveOSocialDataItem dataItem = mAOMFactory.CreateSocialDataItem();
                            dataItem.ItemType = AveOSocialDataItemType.Tag;
                            dataItem.Text = item.Value.Substring(1);
                            if (allTags.ContainsKey(item.Value))
                            {
                                dataItem.TagGuid = new Guid(allTags[item.Value]);
                            }
                            dataItemCollection.Add(dataItem);
                        }

                    }
                    postData.ContentItems = dataItemCollection.ToArray();
                    postData.ContentText = text;
                }
                else
                {
                    postData.ContentText = postInfo.Text;
                }
                #endregion

                #region Restore Attachment in 2 methods. By default, we use the API to create the SPSocialAttachment object. We can also use the reflection to upload the attachment to my site host temporarily.
                if (postInfo.Attachment != null)
                {
                    if (postInfo.Attachment.AttachmentKind == AveOSocialAttachmentKind.Image && postInfo.Attachment.Content != null)
                    {
                        postData.Attachment = fdManager.CreateImageAttachment(postInfo.Attachment.Name, postInfo.Attachment.Description, new MemoryStream(postInfo.Attachment.Content));
                    }
                    else
                    {
                        IAveOSocialAttachment att = mAOMFactory.CreateSocialAttachment();
                        att.AttachmentKind = postInfo.Attachment.AttachmentKind;
                        att.Description = postInfo.Attachment.Description;

                        mCurrentUserProfile = mUserProfileManager.GetUserProfile(feedInfo.Actors[postInfo.AuthorIndex].AccountName);
                        string[] paths = mCurrentUserProfile.SaveTempFile(postInfo.Attachment.Content, postInfo.Attachment.Name);

                        if (paths != null && paths.Length >= 3)
                        {
                            att.Uri = new Uri(paths[0]);
                            att.Name = paths[2];
                        }
                        else
                        {
                            att.Uri = postInfo.Attachment.Uri;
                            att.Name = postInfo.Attachment.Name;
                        }

                        if (att.Uri != null)
                        {
                            postData.Attachment = att;
                        }
                    }
                }
                #endregion

                string targetId = parentThread == null ? (mUserProfile == null ? mUrl : null) : parentThread.Id;
                if (mAOMFactory.ContextKind.IsServerMode13Upper()
                    || mAOMFactory.AccountInfo.UserName.Equals(feedInfo.Actors[postInfo.AuthorIndex].AccountName, StringComparison.OrdinalIgnoreCase))
                {
                    thread = fdManager.CreatePost(targetId, postData);
                }
                ////This is a new post. So we don't need to check the existing likers. Just set the thread and post to null.
                //RestoreLikerInfo(postInfo.Likers, null, null, fdManager);
                //This is a new post. We need to check whether it's a rootpost or reply so that all the post is restored correctly.
                IAveOSocialPost newCreatedPost = null;
                if (parentThread == null)
                {
                    newCreatedPost = thread.RootPost;
                }
                else
                {
                    newCreatedPost = thread.Replies.First();
                }
                RestoreLikerInfo(postInfo.Likers, null, newCreatedPost, ref fdManager);
                #endregion
                if (modifiedTimeCache == null || createdTimeCache == null)
                {
                    modifiedTimeCache = new Dictionary<string, DateTime>();
                    createdTimeCache = new Dictionary<string, DateTime>();
                }
                if (!modifiedTimeCache.ContainsKey(newCreatedPost.Id) && !createdTimeCache.ContainsKey(newCreatedPost.Id))
                {
                    modifiedTimeCache.Add(Convert.ToString(newCreatedPost.Id.Split('.')[7]), postInfo.ModifiedTime);
                    createdTimeCache.Add(Convert.ToString(newCreatedPost.Id.Split('.')[7]), postInfo.CreatedTime);
                }
                return thread;
            }
        }

        private IAveOSocialThread RestoreSinglePostForArchiver(AveSocialFeedInfo feedInfo, AveSocialFeedPostInfo postInfo, ref IAveOSocialFeedManager fdManager, IAveOSocialThread parentThread, bool isExisting)
        {
            using (AvePerformanceScope scope = new AvePerformanceScope("RestoreSinglePost"))
            {
                IAveOSocialThread thread = null;
                fdManager = ChangeFeedManagerForArchiver(feedInfo.Actors[postInfo.AuthorIndex].AccountName, fdManager);

                #region Restore the existing replies
                if (isExisting)
                {
                    var existingReplies = parentThread.Replies.Where(r => (parentThread.Actors[r.AuthorIndex].AccountName.Equals(mCurrentAccountName, StringComparison.OrdinalIgnoreCase) &&
                ((r.Attachment != null && postInfo.Attachment != null && r.Attachment.Name.Equals(postInfo.Attachment.Name, StringComparison.Ordinal) &&
                (r.Text != null && postInfo.Text != null && r.Text.Equals(postInfo.Text, StringComparison.Ordinal) || r.Text == null && postInfo.Text == null)) ||
                r.Attachment == null && postInfo.Attachment == null && r.Text != null && postInfo.Text != null && r.Text.Equals(postInfo.Text, StringComparison.Ordinal))));

                    if (existingReplies.Count() > 0)
                    {
                        foreach (IAveOSocialPost reply in existingReplies)
                        {
                            RestoreLikerInfoForArchiver(postInfo.Likers, parentThread, reply, ref fdManager);
                        }

                        return null;
                    }
                }
                #endregion

                #region Restore the new post (rootpost and replies)
                IAveOSocialPostCreationData postData = mAOMFactory.CreateSocialPostCreationData();

                if (postInfo.Overlays != null && postInfo.Overlays.Count > 0)
                {
                    try
                    {
                        string content = postInfo.Text;
                        int count = postInfo.Overlays.Count;
                        IAveOSocialDataItem[] contentItems = mAOMFactory.CreateSocialDataItemCollection(count);
                        for (int i = count - 1; i >= 0; i--)
                        {
                            IAveOSocialDataItem dataItem = mAOMFactory.CreateSocialDataItem();
                            AveSocialDataOverlay overlay = postInfo.Overlays[i];
                            int index = overlay.Index;
                            int length = overlay.Length;
                            if (overlay.OverlayType == AveOSocialDataOverlayType.Link)
                            {
                                dataItem.Text = content.Substring(index, length);
                                dataItem.Uri = overlay.LinkUri;
                                dataItem.ItemType = AveOSocialDataItemType.Link;
                                content = content.Substring(0, index) + "{" + i + "}" + content.Substring(index + length);
                            }
                            else //AveOSocialDataOverlayType.Actors
                            {
                                int actorIndex = overlay.ActorIndexes[0];

                                AveSocialActorInfo actorInfo = feedInfo.Actors[actorIndex];
                                switch (actorInfo.ActorType)
                                {
                                    case AveOSocialActorType.User:
                                        dataItem.AccountName = actorInfo.AccountName;
                                        dataItem.ItemType = AveOSocialDataItemType.User;
                                        content = content.Substring(0, index) + "{" + i + "}" + content.Substring(index + length);
                                        break;
                                    case AveOSocialActorType.Tag:
                                        dataItem.TagGuid = actorInfo.TagGuid; //to do change the id
                                        dataItem.Text = actorInfo.Name;
                                        dataItem.ItemType = AveOSocialDataItemType.Tag;
                                        content = content.Substring(0, index) + "{" + i + "}" + content.Substring(index + length);
                                        break;
                                    case AveOSocialActorType.Site:

                                        break;
                                    case AveOSocialActorType.Document:

                                        break;
                                    default:
                                        break;
                                }

                            }

                            contentItems[i] = dataItem;
                        }
                        postData.ContentItems = contentItems;
                        postData.ContentText = content;
                    }
                    catch (Exception e) //如果出错就按照原来的逻辑Post纯文本
                    {
                        log.Debug("An error occurred while restore post and use previous logic to restore post. Error message:{0}.", e.ToString());
                        postData.ContentItems = null;
                        postData.ContentText = postInfo.Text;
                    }
                }
                else
                {
                    postData.ContentText = postInfo.Text;
                }

                #region Restore Attachment in 2 methods. By default, we use the API to create the SPSocialAttachment object. We can also use the reflection to upload the attachment to my site host temporarily.
                if (postInfo.Attachment != null)
                {
                    if (postInfo.Attachment.AttachmentKind == AveOSocialAttachmentKind.Image && postInfo.Attachment.Content != null)
                    {
                        postData.Attachment = fdManager.CreateImageAttachment(postInfo.Attachment.Name, postInfo.Attachment.Description, new MemoryStream(postInfo.Attachment.Content));
                    }
                    else
                    {
                        IAveOSocialAttachment att = mAOMFactory.CreateSocialAttachment();
                        att.AttachmentKind = postInfo.Attachment.AttachmentKind;
                        att.Description = postInfo.Attachment.Description;

                        mCurrentUserProfile = mUserProfileManager.GetUserProfile(feedInfo.Actors[postInfo.AuthorIndex].AccountName);
                        string[] paths = mCurrentUserProfile.SaveTempFile(postInfo.Attachment.Content, postInfo.Attachment.Name);

                        if (paths != null && paths.Length >= 3)
                        {
                            att.Uri = new Uri(paths[0]);
                            att.Name = paths[2];
                        }
                        else
                        {
                            att.Uri = postInfo.Attachment.Uri;
                            att.Name = postInfo.Attachment.Name;
                        }

                        if (att.Uri != null)
                        {
                            postData.Attachment = att;
                        }
                    }
                }
                #endregion

                string targetId = parentThread == null ? (mUserProfile == null ? mUrl : null) : parentThread.Id;
                if (mAOMFactory.ContextKind.IsServerMode13Upper()
                    || mAOMFactory.AccountInfo.UserName.Equals(feedInfo.Actors[postInfo.AuthorIndex].AccountName, StringComparison.OrdinalIgnoreCase))
                {
                    thread = fdManager.CreatePost(targetId, postData);
                }
                //This is a new post. So we don't need to check the existing likers. Just set the thread and post to null.
                IAveOSocialPost newCreatedPost = null;
                if (parentThread == null)
                {
                    newCreatedPost = thread.RootPost;
                }
                else
                {
                    newCreatedPost = thread.Replies.First();
                }
                RestoreLikerInfoForArchiver(postInfo.Likers, null, newCreatedPost, ref fdManager);
                #endregion

                return thread;
            }
        }

        /// <summary>
        /// Get the conflict thread from the existing feed in the destination.
        /// if thread ids are same between source and destination, they are same.
        /// if the author account names and content are same between source and destination, they are same.
        /// Text (content) should be case sensitive.
        /// </summary>
        /// <param name="existingFeed"></param>
        /// <param name="feedInfo"></param>
        /// <returns></returns>
        private string CheckConflictThread(IAveOSocialFeed existingFeed, AveSocialFeedInfo feedInfo)
        {
            if (existingFeed == null || existingFeed.Threads == null || existingFeed.Threads.Length <= 0)
            {
                return string.Empty;
            }

            var tmpThreads = existingFeed.Threads.Where(t => (t.Id.Equals(feedInfo.Id, StringComparison.OrdinalIgnoreCase) ||
                t.Actors[t.RootPost.AuthorIndex].AccountName.Equals(feedInfo.Actors[feedInfo.RootPost.AuthorIndex].AccountName, StringComparison.OrdinalIgnoreCase) &&
                ((t.RootPost.Attachment != null && feedInfo.RootPost.Attachment != null && t.RootPost.Attachment.Name.Equals(feedInfo.RootPost.Attachment.Name, StringComparison.Ordinal) &&
                (t.RootPost.Text != null && feedInfo.RootPost.Text != null && t.RootPost.Text.Equals(feedInfo.RootPost.Text, StringComparison.Ordinal) || t.RootPost.Text == null && feedInfo.RootPost.Text == null)) ||
                t.RootPost.Attachment == null && feedInfo.RootPost.Attachment == null && t.RootPost.Text != null && feedInfo.RootPost.Text != null && t.RootPost.Text.Equals(feedInfo.RootPost.Text, StringComparison.Ordinal)) && t.RootPost.CreatedTime.CompareTo(feedInfo.RootPost.CreatedTime) == 0)); //For the Text, we should treat it as case sensitive.

            if (tmpThreads.Count() > 0)
            {
                return tmpThreads.First().Id;
            }
            else
            {
                return string.Empty;
            }
        }

        private string CheckConflictThreadForArchiver(IAveOSocialThread[] existingThreads, AveSocialFeedInfo feedInfo)
        {
            if (existingThreads == null || existingThreads.Length <= 0)
            {
                return string.Empty;
            }

            var tmpThreads = existingThreads.Where(t => (t.Id.Equals(feedInfo.Id, StringComparison.OrdinalIgnoreCase) ||
                t.Actors[t.RootPost.AuthorIndex].AccountName.Equals(feedInfo.Actors[feedInfo.RootPost.AuthorIndex].AccountName, StringComparison.OrdinalIgnoreCase) &&
                ((t.RootPost.Attachment != null && feedInfo.RootPost.Attachment != null && t.RootPost.Attachment.Name.Equals(feedInfo.RootPost.Attachment.Name, StringComparison.Ordinal) &&
                (t.RootPost.Text != null && feedInfo.RootPost.Text != null && t.RootPost.Text.Equals(feedInfo.RootPost.Text, StringComparison.Ordinal) || t.RootPost.Text == null && feedInfo.RootPost.Text == null)) ||
                t.RootPost.Attachment == null && feedInfo.RootPost.Attachment == null && t.RootPost.Text != null && feedInfo.RootPost.Text != null && t.RootPost.Text.Equals(feedInfo.RootPost.Text, StringComparison.Ordinal)))); //For the Text, we should treat it as case sensitive.

            if (tmpThreads.Count() > 0)
            {
                return tmpThreads.First().RootPost.Id;
            }
            else
            {
                return string.Empty;
            }
        }

        public string Restore(AveSocialFeedInfo socialThread, ref Dictionary<string, DateTime> modifiedTimeCache, ref Dictionary<string, DateTime> createdTimeCache, int conflictSolution = 6)
        {
            if (socialThread == null || mServiceContext == null || (mUserProfile == null && string.IsNullOrEmpty(mUrl)))
            {
                log.Debug("There is no feed to restore.");
                return string.Empty;
            }

            try
            {
                //feeds.Reverse(); //先备份的后还原
                using (mAOMFactory.CreateServiceContextScope(mServiceContext.ServiceContext))
                {
                    #region Get the existing feeds in destination

                    IAveOSocialFeed existingFeed = null;

                    try
                    {
                        IAveOSocialFeedManager dtManager = mAOMFactory.CreateSocialFeedManager();
                        //If this is a personal site, we should get feed using the account name.
                        //If this is a normal site, we should use the web url to get the feeds which belongs to this web exactly.
                        existingFeed = dtManager.GetFeedFor(mUrl, mAOMFactory.CreateSocialFeedOptions());
                    }
                    catch (System.Exception ex)
                    {
                        log.Warn("Exception occurred when trying to get the existing feeds: " + ex.ToString());
                    }

                    #endregion

                    #region Real restore posts
                    IAveOSocialFeedManager fdManager = null;
                    mCurrentAccountName = string.Empty;
                    //foreach (AveSocialFeedInfo feedInfo in feeds)
                    //{
                    //Check if we need to change the feed manager according to the author 
                    fdManager = ChangeFeedManager(socialThread.Actors[socialThread.RootPost.AuthorIndex].AccountName, fdManager);

                    string existingThreadId = CheckConflictThread(existingFeed, socialThread);
                    IAveOSocialThread st = null;
                    bool bExisted = false;

                    if (!string.IsNullOrEmpty(existingThreadId))
                    {
                        switch (conflictSolution)
                        {
                            case 0: //skip
                                return string.Empty;
                            case 4: //replace
                                fdManager.DeletePost(existingThreadId);
                                existingThreadId = string.Empty;
                                break;
                            default: //merge
                                break;
                        }
                    }
                    //If the thread is already existing in the destination, we won't create a duplicated one.
                    if (!string.IsNullOrEmpty(existingThreadId))
                    {
                        st = fdManager.GetFullThread(existingThreadId);
                        RestoreLikerInfo(socialThread.RootPost.Likers, st, st.RootPost, ref fdManager);
                        bExisted = true;
                    }
                    else
                    {
                        //////////////////////////////////////////////////////////////////////////
                        // Sometimes it will throw the exception as following
                        // "System.NotSupportedException: Unsupported principal type 'GenericPrincipal'. Ensure that Claims authentication mode is enabled."
                        // But when we try again, the exception disappeared.
                        // So we will retry this operation for 3 times in 1s time span.
                        //////////////////////////////////////////////////////////////////////////
                        int retry = 0;
                        log.Debug(string.Format("if some exception occurred, we will retry {0} times.", mRetry));
                        while (retry < mRetry)
                        {
                            try
                            {
                                st = RestoreSinglePost(socialThread, socialThread.RootPost, ref fdManager, null, bExisted, ref modifiedTimeCache, ref createdTimeCache);
                                break;
                            }
                            catch (Exception ex)
                            {
                                retry++;
                                if (retry < mRetry)
                                {
                                    log.Debug(string.Format("Some exception occurred because of {0}. \r\n Try again No.{1}", ex.ToString(), retry));
                                }
                                else
                                {
                                    log.Warn(string.Format("Exception occurred when restore single post : {0}", ex.ToString()));
                                }
                                Thread.Sleep(1000);
                            }
                        }
                    }

                    //restore reply
                    if (st != null)
                    {
                        foreach (AveSocialFeedPostInfo reply in socialThread.Replies.OrderBy(r => r.CreatedTime))
                        {
                            RestoreSinglePost(socialThread, reply, ref fdManager, st, bExisted, ref modifiedTimeCache, ref createdTimeCache);
                        }

                        //change the latest two created time
                        UpdateSocialThreadInfo(socialThread, st);

                        return st.Id;
                    }
                    //}
                    #endregion
                }

            }
            catch (Exception e)
            {
                log.Log(AveLogLevel.WARN, "An error occurred while restoring the social Thread", e.ToString());
                mReport.AddDetail(new AveWrapperReportDto("SocialFeed", "SocialFeed", AveReportObjectType.SocialFeed, AveStatus.Failed, AveReportResource.Wrapper_Report_RestoreSocialFeedError, e.Message));
            }
            return string.Empty;
        }

        private void UpdateSocialThreadInfo(AveSocialFeedInfo socialThread, IAveOSocialThread destSocialThread)
        {
            try
            {
                List<DateTime> temp = new List<DateTime>();
                if (destSocialThread.Replies != null)
                {
                    temp.AddRange(destSocialThread.Replies.Select(t => t.CreatedTime));
                }
                if (socialThread.LatestTwoReplyTime != null)
                {
                    temp.AddRange(socialThread.LatestTwoReplyTime);
                }

                if (temp.Count() == 1)
                {
                    socialThread.LatestTwoReplyTime = new DateTime[1] { temp[0] };
                }
                else if (temp.Count() >= 2)
                {
                    socialThread.LatestTwoReplyTime = new DateTime[2];
                    int i = 0;
                    var times = temp.OrderByDescending(t => t).ToList();
                    foreach (DateTime t in times)
                    {
                        if (i < 2)
                        {
                            socialThread.LatestTwoReplyTime[i] = t;
                            i++;
                        }
                        else
                        {
                            break;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                log.Log(AveLogLevel.WARN, "An error occurred while updating social thread information", e.ToString());
            }
        }
    }

    public class AveSocialFollowing : AveSocialData
    {
        private static AveLogger log = AveLogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);
        private IAveOSocialFollowingManager mFollowingManager;
        private AveObjectModelFactory mAOMFactory;

        public AveSocialFollowing(AveUserProfile profile,AveObjectModelFactory mFactory)
            : base(profile)
        {
            mAOMFactory = AveObjectModelFactory.CreateObjectModelFactory(profile.DestSiteUrl, mFactory.AccountInfo, mFactory.ContextKind);
            mFollowingManager = mAOMFactory.CreateSPSocialFollowingManager(mUserProfile.UserProfile, mServiceContext.ServiceContext);
        }

        protected AveSocialFollowing(AveServiceContext context, string url)
            : base(context, url)
        {
        }

        //This is not a public one
        protected AveSocialFollowing(AveServiceContext context)
            : base(context)
        {
        }

        public void Restore(List<AveSocialActorInfo> followed)
        {
            if (mUserProfile == null || followed == null || followed.Count <= 0)
            {
                log.Debug("There is no followed information to restore.");
                return;
            }

            using (mAOMFactory.CreateServiceContextScope(mServiceContext.ServiceContext))
            {
                foreach (AveSocialActorInfo actor in followed)
                {
                    try
                    {
                        IAveOSocialActorInfo actorInfo = mAOMFactory.CreateSPSocialActorInfo(actor);
                        AveOSocialFollowResult result = mFollowingManager.Follow(actorInfo);
                        log.Debug(string.Format("Followed {0} with the result of {1}.", actorInfo.AccountName + actorInfo.ContentUri + actorInfo.TagGuid, result.ToString()));
                    }
                    catch (Exception ex)
                    {
                        log.Warn(string.Format("Some exception occurred while {0} tried to follow {1} : {2}", mUserProfile.UserProfile.AccountName, actor.AccountName, ex.ToString()));
                    }
                }
            }
        }

        public void Restore(AveSocialActorInfo socialActorInfo)
        {
            using (mAOMFactory.CreateServiceContextScope(mServiceContext.ServiceContext))
            {
                try
                {
                    IAveOSocialActorInfo actorInfo = mAOMFactory.CreateSPSocialActorInfo(socialActorInfo);
                    AveOSocialFollowResult result = mFollowingManager.Follow(actorInfo);
                    log.Debug(string.Format("Follow {0} with the result of {1}. Type: {2}", socialActorInfo.ContentUri, result.ToString(),socialActorInfo.ActorType));
                }
                catch (Exception ex)
                {
                    log.Warn("An error occurred while {0} tried to follow {1}, Type {2} {3}.", mUserProfile.UserProfile.AccountName, socialActorInfo.ContentUri, socialActorInfo.ActorType, ex);
                    throw;
                }
            }
        }
    }

    public class AveSocialRating : AveSocialData
    {
        private static AveLogger log = AveLogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);
        private IAveOSocialRatingManager mRatingManager;
        public AveSocialRating(AveUserProfile profile)
            : base(profile)
        {
            mRatingManager = mServiceContext.OMFactory.CreateSocialRatingManager(mServiceContext.ServiceContext);
            mRatingManager.ProfileLoader.UserProfile = profile.UserProfile;
        }

        public override void Restore(IList dtCollection)
        {
            if (dtCollection == null || mRatingManager == null)
            {
                return;
            }
            Dictionary<string, string> absoluteUrlMapping = new Dictionary<string, string> { 
            { 
                mUserProfile.SourceSiteInfo.Url, mUserProfile.DestSiteUrl } 
            };
            ReplaceOption replcaceOption = new ReplaceOption(true, true, true);
            foreach (AveSOcialRatingInfo rating in dtCollection)
            {
                try
                {
                    rating.Url = AveReplaceProcessor.UrlReplace(rating.Url, absoluteUrlMapping, replcaceOption, mUserProfile.SourceSiteInfo, mUserProfile.DestSiteUrl);
                    if (!rating.Url.StartsWith(mUserProfile.DestSiteUrl, StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }
                    mRatingManager.SetRating(new Uri(rating.Url), rating.Rating, rating.Title);
                }
                catch (Exception e)
                {
                    log.Warn("An error occurred while {0} tried to rate {1}. Error: {2}.", mUserProfile.UserProfile.AccountName, rating.Url, e);
                }
            }
        }
    }
}
