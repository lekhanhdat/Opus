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
using AvePoint.Wrapper.Resource;

namespace AvePoint.Wrapper.Backup
{
    //public class AveDocumentTagging
    //{
    //    private static AveLogger mLog = AveLogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);
    //    private AveSPSite mAveParentSite;

    //    public AveSPSite ParentSite
    //    {
    //        get { return mAveParentSite; }
    //    }

    //    public string mUrl;

    //    public AveDocumentTagging(string url, AveSPSite aveSite)
    //    {
    //        mUrl = url;
    //        mAveParentSite = aveSite;
    //    }

    //    public List<AveDocumentTaggingInfo> GetDocumentTaggings()
    //    {
    //        if (mAveParentSite.UserProfileApplicationAvailable)
    //        {
    //            try
    //            {
    //                List<AveDocumentTaggingInfo> DTs = new List<AveDocumentTaggingInfo>();
    //                IAveServiceContext tempContext = mAveParentSite.ObjectModelFactory.CreateServiceContext();
    //                IAveServiceContext mContext = tempContext.GetContext(mAveParentSite.SPSite);
    //                IAveOSocialTagManager st = mAveParentSite.ObjectModelFactory.CreateSocialTagManager(mContext);
    //                IAveOSocialTag[] tags = st.GetTags(mUrl, mAveParentSite.UserProfiles);
    //                foreach (IAveOSocialTag tag in tags)
    //                {
    //                    AveDocumentTaggingInfo dtInfo = new AveDocumentTaggingInfo();
    //                    dtInfo.Url = tag.Url.ToString();
    //                    dtInfo.Title = tag.Title;
    //                    try
    //                    {
    //                        dtInfo.Owner = tag.Owner["AccountName"].Value.ToString();
    //                    }
    //                    catch (Exception e)
    //                    {
    //                        mLog.Log(AveLogLevel.DEBUG, WrapperBackupResource.AWBCannotFindDocTagOwner, e.ToString());
    //                        dtInfo.Owner = tag.Owner.DisplayName;
    //                    }
    //                    dtInfo.TermOwner = tag.Term.Owner;
    //                    dtInfo.IsPrivate = tag.IsPrivate;

    //                    dtInfo.Term.Id = tag.Term.ID;
    //                    dtInfo.Term.TermName = tag.Term.Name;
    //                    dtInfo.Term.IsRoot = tag.Term.IsRoot;
    //                    dtInfo.Term.IsKeyword = tag.Term.IsKeyword;
    //                    dtInfo.Term.SourceTermId = tag.Term.SourceTerm.ID;
    //                    dtInfo.Term.SourceTermName = tag.Term.SourceTerm.Name;
    //                    dtInfo.Term.IsAvailableForTagging = tag.Term.IsAvailableForTagging;
    //                    DTs.Add(dtInfo);
    //                }
    //                return DTs;
    //            }
    //            catch (Exception e)
    //            {
    //                mAveParentSite.UserProfileApplicationAvailable = false;
    //                mLog.Log(EventSources.DocAveAgentService, EventCategorys.DocAveAgentService.Common_Wrapper, new EventIds.SharePoint.BackupDocumentTagFailedEventMessage(mUrl, e));
    //            }
    //        }
    //        return null;
    //    }

    //    //通过API只能获取当前用户的UserProfile中的tag,重写API通过proc_SocialTags_GetForUrlByTime去获得所有用户的TAG
    //    //private IAveOSocialTag[] GetTagsForAllUser(IAveOSocialTagManager socialTagManager)
    //    //{
    //    //    if (!socialTagManager.IsSocialAdmin)
    //    //    {
    //    //        throw new UnauthorizedAccessException();
    //    //    }
    //    //    List<Guid> termIDs = new List<Guid>();
    //    //    List<DateTime> lmts = new List<DateTime>();
    //    //    List<string> titles = new List<string>();
    //    //    List<long> rgUserRecordIds = new List<long>();
    //    //    List<Uri> urls = new List<Uri>();
    //    //    List<bool> isPrivates = new List<bool>();
    //    //    try
    //    //    {
    //    //        IAveOAlternateAccessMapping accessMapping = mAveParentSite.ObjectModelFactory.CreateOAlternateAccessMapping();
    //    //        Uri tmpuri = accessMapping.GetSerializedUrl(new Uri(mUrl));
    //    //        //(Uri)AveAssemblyUtility.InvokeStaticMethod(socialTagManager.GetType().Assembly.GetType("Microsoft.Office.Server.SocialData.AlternateAccessMapping"), "GetSerializedUrl", new object[] { new Uri(mUrl) });
    //    //        using (SqlCommand command = new SqlCommand("dbo.proc_SocialTags_GetForUrlByTime"))
    //    //        {
    //    //            command.Parameters.Add("@url", SqlDbType.NVarChar, 0x824).Value = tmpuri.AbsoluteUri;
    //    //            command.Parameters.Add("@partitionID", SqlDbType.UniqueIdentifier).Value = socialTagManager.PartitionID;
    //    //            IAveOULS uls = mAveParentSite.ObjectModelFactory.CreateOULF();
    //    //            command.Parameters.Add("@correlationId", SqlDbType.UniqueIdentifier).Value = uls.CorrelationGet();
    //    //            command.Parameters.Add("@fromDate", SqlDbType.DateTime).Value = new DateTime(1754, 1, 1, 0, 0, 0, 0);
    //    //            command.Parameters.Add("@toDate", SqlDbType.DateTime).Value = new DateTime(9999, 12, 31, 0, 0, 0, 0);
    //    //            command.CommandType = CommandType.StoredProcedure;
    //    //            IAveOUserProfileApplicationProxy userProfileProxy =socialTagManager.UserProfileApplicationProxy;
    //    //            IAveOSqlSession socialDB = userProfileProxy.SocialDBSqlSession;
    //    //            using (SqlDataReader reader = socialDB.ExecuteReader(command))
    //    //            {
    //    //                if (reader.HasRows)
    //    //                {
    //    //                    int ordinal = reader.GetOrdinal("Url");
    //    //                    int num2 = reader.GetOrdinal("TermID");
    //    //                    int num3 = reader.GetOrdinal("LastModifiedTime");
    //    //                    int num4 = reader.GetOrdinal("Title");
    //    //                    int num5 = reader.GetOrdinal("User_RecordID");
    //    //                    int num6 = reader.GetOrdinal("IsPrivate");
    //    //                    while (reader.Read())
    //    //                    {
    //    //                        termIDs.Add(reader.GetGuid(num2));
    //    //                        lmts.Add(reader.GetDateTime(num3));
    //    //                        urls.Add(new Uri(reader.GetString(ordinal)));
    //    //                        rgUserRecordIds.Add(reader.GetInt64(num5));
    //    //                        titles.Add(reader.IsDBNull(num4) ? null : reader.GetString(num4));
    //    //                        isPrivates.Add(reader.GetBoolean(num6));
    //    //                    }
    //    //                }
    //    //            }
    //    //        }
    //    //        List<IAveOSocialTag> ss = socialTagManager.CreateSocialTags(termIDs, lmts, titles, rgUserRecordIds, urls, isPrivates, null);
    //    //        return ss.ToArray();
    //    //    }
    //    //    catch (Exception e)
    //    //    {
    //    //        mLog.Warn("Exception was thrown while get tags for alluser.URL " + mUrl, e);
    //    //    }
    //    //    return null;
    //    //}

    //    public void Export(IAveBackupStream stream)
    //    {
    //        using (AvePerformanceScope pc = new AvePerformanceScope("Backup.AveDocumentTagging.Export"))
    //        {
    //            List<AveDocumentTaggingInfo> DTs = GetDocumentTaggings();
    //            if (DTs != null && DTs.Count > 0)
    //            {
    //                stream.WriteMetadata(AveMetadataType.DocumentTagging.ToString(), DTs);
    //            }
    //        }
    //    }
    //}
}