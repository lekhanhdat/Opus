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
using AvePoint.Wrapper.Resource.ServerAPI2010;
using AvePoint.Wrapper.Resource.Restore;

namespace AvePoint.Wrapper.Restore
{
    public class AveDocumentTagging : AvePoint.Wrapper.Restore.IAveDocumentTagging,IDisposable
    {
        private static AveLogger log = AveLogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);
        private string mUrl;
        private AveSPSite mAveSite;
        public AveDocumentTagging(string url, AveSPSite aveSite)
        {
            mUrl = url;
            mAveSite = aveSite;
        }

        private IReport report = new AveWrapperReport();
        public IReport GetReport()
        {
            return report;
        }

        public void Restore(List<AveDocumentTaggingInfo> DTCollection)
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveDocumentTagging.Restore"))
            {

            try
            {
                if (DTCollection.Count == 0)
                {
                    return;
                }
                if (!AveSPUtility.IfServiceAvailable(mAveSite.SPSite.WebApplication, AveServiceApplicationType.UserProfileService) ||
                     !AveSPUtility.IfServiceAvailable(mAveSite.SPSite.WebApplication, AveServiceApplicationType.ManagedMetadataService))
                {
                    return;
                }
                //!< add for 07item
                IAveServiceContext context = null;
                IAveOUserProfileManager userProfileManager = null;
                IAveSiteSubscriptionIdentifier siteSubscriptionIdentifier = mAveSite.ObjectModelFactory.CreateSiteSubscriptionIdentifier();
                IAveServiceContext tempContext = mAveSite.ObjectModelFactory.CreateServiceContext();
                context = tempContext.GetContext(mAveSite.SPSite.WebApplication.ServiceApplicationProxyGroup, siteSubscriptionIdentifier.Default);
                userProfileManager = mAveSite.ObjectModelFactory.CreateUserProfileManager(context);
                IAveOUserProfile userProfile = null;
                IAveOSocialTagManager socialTagManager = mAveSite.ObjectModelFactory.CreateSocialTagManager(context);
                IAveTaxonomySession session = mAveSite.ObjectModelFactory.CreateTaxonomySession(mAveSite.SPSite);
                //IAveTermSet termSet = session.DefaultKeywordsTermStore.KeywordsTermSet;
                IAveTermSet termSet = null;
                int lcid = 0;
                if (session.DefaultKeywordsTermStore != null)
                {
                    lcid = session.DefaultKeywordsTermStore.DefaultLanguage;
                    termSet = session.DefaultKeywordsTermStore.KeywordsTermSet;
                }
                else
                {
                    lcid = session.TermStores[0].DefaultLanguage;
                    termSet = session.TermStores[0].KeywordsTermSet;
                }

                IAveTermCollection termCollection = termSet.Terms;

                foreach (AveDocumentTaggingInfo dtInfo in DTCollection)
                {
                    try
                    {
                        string ownerLogin = dtInfo.Owner;

                        string termOwner = dtInfo.TermOwner;
                        if (string.IsNullOrEmpty(termOwner) || string.IsNullOrEmpty(ownerLogin))
                        {
                            continue;
                        }
                        ownerLogin = mAveSite.SPMembers.GetMappingUserLogin(ownerLogin, true);
                        termOwner = mAveSite.SPMembers.GetMappingUserLogin(termOwner, true);
                        if (userProfileManager.UserExists(ownerLogin))
                        {
                            userProfile = userProfileManager.GetUserProfile(ownerLogin);
                        }
                        else
                        {
                            userProfile = userProfileManager.CreateUserProfile(ownerLogin);
                        }
                        if (userProfile != null)
                        {
                            IAveTerm term = session.GetTerm(dtInfo.Term.Id);
                            if (term == null)
                            {
                                term = termCollection[dtInfo.Term.TermName];
                            }
                            if (term == null)
                            {
                                term = termSet.CreateTerm(dtInfo.Term.TermName, lcid, dtInfo.Term.Id);
                                session.DefaultKeywordsTermStore.CommitAll();
                            }
                            //AddTag(new Uri(mUrl), userProfile, term, dtInfo.Title, dtInfo.IsPrivate, context);
                            #region reset m_UserProfile
                            IAveOProfileLoader objProfileLoder = socialTagManager.ProfileLoader;// typeof(SocialDataManager).InvokeMember("ProfileLoader", BindingFlags.GetProperty | BindingFlags.NonPublic | BindingFlags.Instance, null, socialTagManager, new object[] { });
                            objProfileLoder.UserProfile = userProfile;
                            //object objUserProfile = typeof(ProfileLoader).InvokeMember("m_UserProfile", BindingFlags.Static | BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.IgnoreCase | BindingFlags.SetField, null, objProfileLoder, new object[] { userProfile });
                            #endregion
                            //目前通过API可以正常还原不确定AddTag方法是否需要重写，如需要重写需要对重写的方法进行进一步修改。
                            socialTagManager.AddTag(new Uri(mUrl), term, dtInfo.Title, Convert.ToBoolean(dtInfo.IsPrivate));
                        }
                    }
                    catch (AveSecurityTrimingException)
                    {
                        throw;
                    }
                }
            }
            catch (AveSecurityTrimingException ex)
            {
                log.Log(AveLogLevel.WARN, WrapperRestoreResource.AddTagFailed, mUrl, ex);
                //log.Log(EventSources.DocAveAgentService, EventCategorys.DocAveAgentService.Wrapper, new EventIds.SharePoint.RestoreDocumentTaggingFailedEventMessage(), ex);
                report.AddDetail(new AveWrapperReportDto("Tag", "Tag", AveReportObjectType.SocialTag, AveStatus.Skipped, AveReportResource.Wrapper_Report_DonnotHavePermissionRestoreTag , ex.Message));
            }
            catch (Exception ex)
            {
                log.Log(EventSources.DocAveAgentService, EventCategorys.DocAveAgentService.Common_Wrapper, new EventIds.SharePoint.RestoreDocumentTagFailedEventMessage(mUrl, ex));
            }


            }

        }

        //public void AddTag(Uri url, IAveOUserProfile userProfile, IAveTerm term, string title, bool isPrivate, IAveServiceContext context)
        //{

        //    using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveDocumentTagging.AddTag"))
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
        //}
        

        //}


        public void Dispose()
        {
            report.Dispose();
        }
    }
}
