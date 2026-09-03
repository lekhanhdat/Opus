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

                            DTs.Add(dtInfo);
                        }
                        return DTs;
                    }
                    catch (Exception e)
                    {
                        mAveParentSite.UserProfileApplicationAvailable = false;
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
                        //mLog.Log(AveLogLevel.WARN, EventSources.DocAveAgentService, EventCategorys.DocAveAgentService.Wrapper, EventIds.SharePoint.SharePointWarning, "The current user has insufficient permissions for user profile service.");
                        mAveParentSite.UserProfileApplicationAvailable = false;
                        mLog.Warn("Exception was thrown while export tag. URL:" + mUrl, e);
                    }
                    catch (Exception e)
                    {
                        if (e.InnerException != null && e.InnerException.GetType().Name.Equals("UserProfileApplicationNotAvailableException", StringComparison.CurrentCultureIgnoreCase))
                        {
                            //mLog.Log(AveLogLevel.WARN, EventSources.DocAveAgentService, EventCategorys.DocAveAgentService.Wrapper, EventIds.SharePoint.SharePointWarning, "Can not backup document tag because user profile application does not exist or is not running;");
                            mAveParentSite.UserProfileApplicationAvailable = false;
                        }
                        else
                        {
                            mLog.Warn("Exception was thrown while export tag. URL:" + mUrl, e);
                        }
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
}