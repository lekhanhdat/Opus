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
using AvePoint.Wrapper.Resource;


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

        internal string GetMappingUser(string login)
        {
            return mServiceContext.GetMappingUser(login);
        }

        public void Dispose()
        {
            if(mReport != null)
            {
                mReport.Dispose();
            }
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
            ChangeUserProfile(ownerLogin);
            string url = mUrl;
            if (String.IsNullOrEmpty(mUrl))
            {
                url = noteInfo.Url;
                //sitecollection 级别userprofile service还原需要替换url；ADO-33630；
                if (url.StartsWith(noteInfo.ProfileManagerUrl))
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
            mServiceContext.GetUserProfileCache(ownerLogin, out recordid, out userId);

            Restore(url, noteInfo, recordid, userId);
        }

        public void Restore(string url, AveSocialCommentInfo noteInfo, long recordId, Guid userId)
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.SocialComment"))
            {
#endif
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
#if PerformanceLog
            }
#endif
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
            ChangeUserProfile(ownerLogin);
            string url = mUrl;
            if (string.IsNullOrEmpty(mUrl))
            {
                url = tagInfo.Url;
                //sitecollection 级别userprofile service还原需要替换url；ADO-33630；
                if (url.StartsWith(tagInfo.ProfileManagerUrl))
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
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.SocialTag"))
            {
#endif
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
                        mServiceContext.GetUserProfileCache(ownerLogin, out recordId, out userId);

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
                        mReport.AddDetail(new AveWrapperReportDto("SocialTag", "SocialTag", AveReportObjectType.SocialTag, AveStatus.Failed, "Restore term failed while add tag with url:" + url + "\n Title:" + tagInfo.Title + ". Error: " + e.Message));
                    }
                }
                catch (AveSecurityTrimingException)
                {
                    throw;
                }
                catch (Exception e)
                {
                    log.Warn("Error while restore DocumentTagging" + e.ToString());
                    mReport.AddDetail(new AveWrapperReportDto("SocialTag", "SocialTag", AveReportObjectType.SocialTag, AveStatus.Failed, "Error while restore DocumentTagging" + e.Message));
                }
#if PerformanceLog
            }
#endif
        }
    }
}
