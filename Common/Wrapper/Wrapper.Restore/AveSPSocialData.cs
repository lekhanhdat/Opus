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
using AvePoint.Wrapper.SPService;
using AvePoint.Wrapper.Common.Office;


namespace AvePoint.Wrapper.Restore
{
    public class AveSPSocialTag:IDisposable
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
            if (!AveSPUtility.IfServiceAvailable(mAveSite.SPSite.WebApplication, ServiceApplicationType.UserProfileService) ||
                 !AveSPUtility.IfServiceAvailable(mAveSite.SPSite.WebApplication, ServiceApplicationType.ManagedMetadataService))
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
                        report.AddDetail(new AveWrapperReportDto("SocialTag", "SocialTag", AveReportObjectType.SocialTag, AveStatus.Skipped, "You don't have permission to restore SocialTag. " + ex.Message));
                    }
                    catch (Exception e)
                    {
                        mLog.Warn("Error while restore DocumentTagging" + e.ToString());
                        report.AddDetail(new AveWrapperReportDto("SocialTag", "SocialTag", AveReportObjectType.SocialTag, AveStatus.Failed, e.Message));
                    }
                }
            }
        }

        public void AddTag(Uri url, IAveOUserProfile userProfile, IAveTerm term, string title, bool isPrivate, IAveServiceContext context)
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveSPSocialTag.AddTag"))
            {
#endif
                if (null == url)
                {
                    throw new ArgumentNullException("url");
                }
                if (term == null)
                {
                    throw new ArgumentNullException("term");
                }
                if (!term.IsAvailableForTagging)
                {
                    throw new InvalidOperationException();
                }
                if (userProfile == null)
                {
                    throw new UnauthorizedAccessException();
                }
                IAveOUserProfileApplicationProxy appProxy = mAveSite.ObjectModelFactory.CreateOUserProfileApplicationProxy();
                IAveOUserProfileApplicationProxy o_UserProfileApplicationProxy = appProxy.GetProxy(context);
                Guid rawPartitionID = appProxy.GetRawPartitionID(context);
                using (SqlCommand command = new SqlCommand("dbo.proc_SocialTags_Add"))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.Add("@partitionID", SqlDbType.UniqueIdentifier).Value = rawPartitionID;
                    //command.Parameters.Add("@correlationId", SqlDbType.UniqueIdentifier).Value = ULS.CorrelationGet();
                    command.Parameters.Add("@user_recordID", SqlDbType.BigInt).Value = userProfile.RecordId;
                    command.Parameters.Add("@userID", SqlDbType.UniqueIdentifier).Value = userProfile.ID;
                    command.Parameters.Add("@url", SqlDbType.NVarChar, 0x824).Value = url.AbsoluteUri;
                    command.Parameters.Add("@termID", SqlDbType.UniqueIdentifier).Value = term.ID;
                    command.Parameters.Add("@inputTermLabel", SqlDbType.NVarChar, 0xff).Value = term.Name;
                    command.Parameters.Add("@isPrivate", SqlDbType.Bit).Value = isPrivate;
                    if (!string.IsNullOrEmpty(title))
                    {
                        command.Parameters.Add("@title", SqlDbType.NVarChar, 500).Value = title;
                    }
                    SqlParameter parameter = command.Parameters.Add("@lastModifiedTime", SqlDbType.DateTime);
                    parameter.Direction = ParameterDirection.Output;

                    IAveOSqlSession o_SocialDBSqlSession = o_UserProfileApplicationProxy.SocialDBSqlSession;
                    o_SocialDBSqlSession.ExecuteNonQuery(command);
                }

#if PerformanceLog
            }
#endif
        }

        public void Dispose()
        {
            if(mTag != null)
            {
                mTag.Dispose();
            }

            if(report != null)
            {
                report.Dispose();
            }
        }
    }

    public class AveSPSocialComment: IDisposable
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
            if (!AveSPUtility.IfServiceAvailable(mAveSite.SPSite.WebApplication, ServiceApplicationType.UserProfileService) ||
                 !AveSPUtility.IfServiceAvailable(mAveSite.SPSite.WebApplication, ServiceApplicationType.ManagedMetadataService))
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
                        report.AddDetail(new AveWrapperReportDto("SocialComment", "SocialComment", AveReportObjectType.SocailComment, AveStatus.Skipped, "You don't have permission to restore SocialComment. " + ex.Message));
                    }
                    catch (Exception e)
                    {
                        report.AddDetail(new AveWrapperReportDto("SocialComment", "SocialComment", AveReportObjectType.SocailComment, AveStatus.Failed, "Error while restore DocumentTagging" + e.Message));
                        mLog.Warn("Error while restore DocumentTagging" + e.ToString());
                    }
                }
            }
        }

        public void Dispose()
        {
            if(mComment != null)
            {
                mComment.Dispose();
            }

            if(report != null)
            {
                report.Dispose();
            }
        }
    }
}
