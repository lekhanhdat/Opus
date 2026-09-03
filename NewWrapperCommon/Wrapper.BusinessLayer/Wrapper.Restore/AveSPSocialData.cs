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
using AvePoint.Wrapper.Common;
using AvePoint.Wrapper.Common.Office;
using AvePoint.Wrapper.Resource.ServerAPI2010;
using AvePoint.Wrapper.SPService;

namespace AvePoint.Wrapper.Restore
{
    public class AveSPSocialTag : AvePoint.Wrapper.Restore.IAveSPSocialTag, IDisposable
    {
        private static AveLogger mLog = AveLogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);
        private IReport report = new AveWrapperReport();
        public IReport GetReport()
        {
            return report;
        }
        private string mUrl;
        private AveSPSite mAveSite;
        private AveSocialTag mTag;
        public AveSPSocialTag(string url, AveSPSite aveSite)
        {
            mUrl = url;
            mAveSite = aveSite;
            mTag = new AveSocialTag(mAveSite.ServiceContext, url);
            mTag.SetReport(report);
        }

        public AveSPSocialTag(AveSPSite aveSite)
        {
            mAveSite = aveSite;
            mTag = new AveSocialTag(mAveSite.ServiceContext);
            mTag.SetReport(report);
        }

        public void Restore(List<AveSocialTagInfo> DTCollection)
        {
            if (mAveSite.SPContextKind != AveContextKind.ClientObjectModel && (!AveSPUtility.IfServiceAvailable(mAveSite.SPSite.WebApplication, AveServiceApplicationType.UserProfileService) ||
                 !AveSPUtility.IfServiceAvailable(mAveSite.SPSite.WebApplication, AveServiceApplicationType.ManagedMetadataService)))
            {
                return;
            }
            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveSPSocialTag.SocialTag"))
            {
                foreach (var ct in DTCollection)
                {
                    try
                    {
                        mTag.Restore(ct);
                    }
                    catch (AveSecurityTrimingException ex)
                    {
                        mLog.Warn("Error while restore DocumentTagging" + ex.ToString());
                        report.AddDetail(new AveWrapperReportDto("SocialTag", "SocialTag", AveReportObjectType.SocialTag, AveStatus.Skipped, AveReportResource.Wrapper_Report_NoPermissionToRestoreSocailTag , ex.Message));
                    }
                    catch (Exception e)
                    {
                        mLog.Warn("Error while restore DocumentTagging" + e.ToString());
                        report.AddDetail(new AveWrapperReportDto("SocialTag", "SocialTag", AveReportObjectType.SocialTag, AveStatus.Failed, e.Message));
                    }
                }
            }
        }

        //public void AddTag(Uri url, IAveOUserProfile userProfile, IAveTerm term, string title, bool isPrivate, IAveServiceContext context)
        //{

        //    using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveSPSocialTag.AddTag"))
        //    {

        //    if (null == url)
        //    {
        //        throw new ArgumentNullException("url");
        //    }
        //    if (term == null)
        //    {
        //        throw new ArgumentNullException("term");
        //    }
        //    if (!term.IsAvailableForTagging)
        //    {
        //        throw new InvalidOperationException();
        //    }
        //    if (userProfile == null)
        //    {
        //        throw new UnauthorizedAccessException();
        //    }
        //    IAveOUserProfileApplicationProxy appProxy = mAveSite.ObjectModelFactory.CreateOUserProfileApplicationProxy();
        //    IAveOUserProfileApplicationProxy o_UserProfileApplicationProxy = appProxy.GetProxy(context);
        //    Guid rawPartitionID = appProxy.GetRawPartitionID(context);
        //    using (SqlCommand command = new SqlCommand("dbo.proc_SocialTags_Add"))
        //    {
        //        command.CommandType = CommandType.StoredProcedure;
        //        command.Parameters.Add("@partitionID", SqlDbType.UniqueIdentifier).Value = rawPartitionID;
        //        //command.Parameters.Add("@correlationId", SqlDbType.UniqueIdentifier).Value = ULS.CorrelationGet();
        //        command.Parameters.Add("@user_recordID", SqlDbType.BigInt).Value = userProfile.RecordId;
        //        command.Parameters.Add("@userID", SqlDbType.UniqueIdentifier).Value = userProfile.ID;
        //        command.Parameters.Add("@url", SqlDbType.NVarChar, 0x824).Value = url.AbsoluteUri;
        //        command.Parameters.Add("@termID", SqlDbType.UniqueIdentifier).Value = term.ID;
        //        command.Parameters.Add("@inputTermLabel", SqlDbType.NVarChar, 0xff).Value = term.Name;
        //        command.Parameters.Add("@isPrivate", SqlDbType.Bit).Value = isPrivate;
        //        if (!string.IsNullOrEmpty(title))
        //        {
        //            command.Parameters.Add("@title", SqlDbType.NVarChar, 500).Value = title;
        //        }
        //        SqlParameter parameter = command.Parameters.Add("@lastModifiedTime", SqlDbType.DateTime);
        //        parameter.Direction = ParameterDirection.Output;

        //        IAveOSqlSession o_SocialDBSqlSession = o_UserProfileApplicationProxy.SocialDBSqlSession;
        //        o_SocialDBSqlSession.ExecuteNonQuery(command);
        //    }


        //    }

        //}
        public void Dispose()
        {
            report.Dispose();
        }
    }

    public class AveSPSocialComment : AvePoint.Wrapper.Restore.IAveSPSocialComment, IDisposable
    {
        private static AveLogger mLog = AveLogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);
        private IReport report = new AveWrapperReport();
        public IReport GetReport()
        {
            return report;
        }
        private string mUrl;
        private AveSPSite mAveSite;
        private AveSocialComment mComment;
        public AveSPSocialComment(string url, AveSPSite aveSite)
        {
            mUrl = url;
            mAveSite = aveSite;
            mComment = new AveSocialComment(mAveSite.ServiceContext, url);
            mComment.SetReport(report);
        }

        public AveSPSocialComment(AveSPSite aveSite)
        {
            mAveSite = aveSite;
            mComment = new AveSocialComment(mAveSite.ServiceContext);
            mComment.SetReport(report);
        }

        public void Restore(List<AveSocialCommentInfo> DTCollection)
        {
            if (mAveSite.SPContextKind != AveContextKind.ClientObjectModel && (!AveSPUtility.IfServiceAvailable(mAveSite.SPSite.WebApplication, AveServiceApplicationType.UserProfileService)))
                 //|| !AveSPUtility.IfServiceAvailable(mAveSite.SPSite.WebApplication, AveServiceApplicationType.ManagedMetadataService)))
            {
                return;
            }
            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveSPSocialComments.SocialComment"))
            {
                foreach (var comment in DTCollection)
                {
                    try
                    {
                        mComment.Restore(comment);
                    }
                    catch (AveSecurityTrimingException ex)
                    {
                        mLog.Warn("Error while restore DocumentTagging" + ex.ToString());
                        report.AddDetail(new AveWrapperReportDto("SocialComment", "SocialComment", AveReportObjectType.SocailComment, AveStatus.Skipped, AveReportResource.Wrapper_Report_NoPermissionToRestoreSocialComment , ex.Message));
                    }
                    catch (Exception e)
                    {
                        report.AddDetail(new AveWrapperReportDto("SocialComment", "SocialComment", AveReportObjectType.SocailComment, AveStatus.Failed, AveReportResource.Wrapper_Report_RestoreDocumentTaggingError , e.Message));
                        mLog.Warn("Error while restore DocumentTagging" + e.ToString());
                    }
                }
            }
        }
        public void Dispose()
        {
            report.Dispose();
        }
    }

    public class AveSPSocialFeed : IDisposable, AvePoint.Wrapper.Restore.IAveSPSocialFeed
    {
        private Dictionary<string, DateTime> modifiedTimeCache = null;
        private Dictionary<string, DateTime> createdTimeCache = null;
        public Dictionary<string, DateTime> ModifiedTimeCache
        {
            get
            {
                if (modifiedTimeCache == null)
                {
                    modifiedTimeCache = new Dictionary<string, DateTime>();
                }
                return modifiedTimeCache;
            }           
        }
        public Dictionary<string, DateTime> CreatedTimeCache
        {
            get
            {
                if (createdTimeCache == null)
                {
                    createdTimeCache = new Dictionary<string, DateTime>();
                }
                return createdTimeCache;
            }
        }
        protected IAveBackupRestoreQueryService mQueryService;
        private static AveLogger mLog = AveLogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);
        private AveSocialFeed mFeed;
        private IReport report = new AveWrapperReport();
        public IReport GetReport()
        {
            return report;
        }

        public AveSPSocialFeed(AveSPWeb web)
        {
            if (web.ParentSite.SPContextKind.IsServerMode13Upper())
            {
                mFeed = new AveSocialFeed(web.ParentSite.ServiceContext, web.SPWeb.Url, web.ParentSite.ObjectModelFactory);                
            }
            else
            {
                mFeed = new AveSocialFeed(web.ParentSite.ServiceContext, web.SPWeb.Url, web.ParentSite.ObjectModelFactory.AccountInfo, web.ParentSite.ObjectModelFactory.ContextKind);
            }
            mQueryService = web.ParentSite.QueryService;
        }

        public void Restore(List<AveSocialFeedInfo> feeds)
        {
            try
            {
                mFeed.Restore(feeds);
            }
            catch (Exception e)
            {
                mLog.Log(AveLogLevel.WARN, "An error occurred while restoring the newsfeed", e.ToString());
                report.AddDetail(new AveWrapperReportDto("SocialFeed", "SocialFeed", AveReportObjectType.SocialFeed, AveStatus.Failed, e.Message));
            }
        }

        //add for Micro Feed Archiver
        public void RestoreForArchiver(List<AveSocialFeedInfo> feeds)
        {
            try
            {
                mFeed.RestoreForArchiver(feeds);
            }
            catch (Exception e)
            {
                mLog.Log(AveLogLevel.WARN, "An error occurred while restoring the newsfeed", e.ToString());
                report.AddDetail(new AveWrapperReportDto("SocialFeed", "SocialFeed", AveReportObjectType.SocialFeed, AveStatus.Failed, e.Message));
            }
        }

        public void Dispose()
        {
            report.Dispose();
        }
    }
}
