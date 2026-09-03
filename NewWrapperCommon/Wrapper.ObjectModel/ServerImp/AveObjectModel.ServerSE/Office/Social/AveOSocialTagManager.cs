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
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics.CodeAnalysis;
using AvePoint.GCommon;
using AvePoint.GCommon.Utility.Exceptions.SharePoint;
using AvePoint.Wrapper.Common;
using AvePoint.Wrapper.Common.Office;
using AvePoint.Wrapper.Resource.ServerAPI2010;
using Microsoft.Office.Server.SocialData;
using Microsoft.Office.Server.UserProfiles;
using Microsoft.SharePoint.Taxonomy;


namespace AvePoint.ObjectModel.ServerSE.Office
{
    class AveOSocialTagManager : AveOSocialDataManager, IAveOSocialTagManager
    {
        private static AveLogger logger = AveLogger.GetInstance(typeof(AveOSocialTagManager));
        private SocialTagManager mSocialTagManager;

        #region Tag Command

        private const string AddTagCommand_Ready = @"
	    declare @retVal int
        exec @retVal = proc_Social_EnsureUserID @partitionID = @partitionID, @user_recordID = @user_recordID, @userID = @userID, @correlationId = @correlationId;
	    if (@retVal != 0)
		    return;
	    exec proc_Social_EnsureUrlID @partitionID = @partitionID, @url = @url, @urlID = @urlID output, @correlationId = @correlationId;
	    if (@urlID is null)
		    return;";

        private const string mAddTagCommand_Check = @"select TOP 1 LastModifiedTime from SocialTags (NOLOCK)
			where UrlID = @urlID and User_RecordID = @user_recordID and TermID = @termID and PartitionID = @partitionID";

        private const string mAddTagCommand_Update = @"
        update SocialTags set LastModifiedTime = @lastModifiedTime, InputTermLabel = @inputTermLabel, IsPrivate = @isPrivate, Title = isnull(@title, Title)
		where UrlID = @urlID and User_RecordID = @user_recordID and TermID = @termID and PartitionID = @partitionID;";

        private const string mAddTagCommand_Insert = @"
		insert into SocialTags (PartitionID, UrlID, User_RecordID, TermID, InputTermLabel, LastModifiedTime, Title, IsPrivate) values (@partitionID, @urlID, @user_recordID, @termID, @inputTermLabel, @lastModifiedTime, @title, @isPrivate);";

        private const string mAddTagCommand_Delete = @"
        DELETE FROM SocialTags_ChangeLog WHERE PartitionID=@partitionID AND UrlID=@urlID AND User_RecordID=@user_recordID AND TermID=@termID AND (ChangedTime=@lastModifiedTime OR ChangedTime=@oldTime)";

        #endregion

        public AveOSocialTagManager(SocialTagManager socialTagManager)
            : base(socialTagManager)
        {
            mSocialTagManager = socialTagManager;
        }

        public AveOSocialTagManager(IAveServiceContext serviceContentext)
            : this(new SocialTagManager((serviceContentext as AveServiceContext).ServiceContext))
        { }

        #region IAveSocialTagManager Members

        public IAveOSocialTag[] GetTags(IAveOUserProfile user)
        {
            SocialTag[] socialTag = mSocialTagManager.GetTags((user as AveOUserProfile).UserProfile);
            AveOSocialTag[] aveSocialTag = new AveOSocialTag[socialTag.Length];
            for (int i = 0; i < socialTag.Length; i++)
            {
                aveSocialTag[i] = new AveOSocialTag(mSocialTagManager, socialTag[i]);
            }
            return aveSocialTag;
        }

        public IAveOSocialTag[] GetTags(string url, Dictionary<long, string> profiles)
        {
            if (!IsSocialAdmin)
            {
                throw new UserProfileInaccessibleException();
            }
            List<Guid> termIDs = new List<Guid>();
            List<DateTime> lmts = new List<DateTime>();
            List<string> titles = new List<string>();
            List<long> rgUserRecordIds = new List<long>();
            List<Uri> urls = new List<Uri>();
            List<bool> isPrivates = new List<bool>();
            List<IAveOSocialTag> ss;
            Uri tmpuri = new AveOAlternateAccessMapping().GetSerializedUrl(new Uri(url));
            using (SqlCommand command = new SqlCommand("dbo.proc_SocialTags_GetForUrlByTime"))
            {
                command.Parameters.Add("@url", SqlDbType.NVarChar, 0x824).Value = tmpuri.AbsoluteUri;
                command.Parameters.Add("@partitionID", SqlDbType.UniqueIdentifier).Value = PartitionID;
                command.Parameters.Add("@correlationId", SqlDbType.UniqueIdentifier).Value = new AveOULS().CorrelationGet();
                command.Parameters.Add("@fromDate", SqlDbType.DateTime).Value = new DateTime(1754, 1, 1, 0, 0, 0, 0);
                command.Parameters.Add("@toDate", SqlDbType.DateTime).Value = new DateTime(9999, 12, 31, 0, 0, 0, 0);
                command.CommandType = CommandType.StoredProcedure;
                IAveOUserProfileApplicationProxy userprofileProxy = UserProfileApplicationProxy;
                IAveOSqlSession socialDB = userprofileProxy.SocialDBSqlSession;
                using (SqlDataReader reader = socialDB.ExecuteReader(command))
                {
                    if (reader.HasRows)
                    {
                        int ordinal = reader.GetOrdinal("Url");
                        int num2 = reader.GetOrdinal("TermID");
                        int num3 = reader.GetOrdinal("LastModifiedTime");
                        int num4 = reader.GetOrdinal("Title");
                        int num5 = reader.GetOrdinal("User_RecordID");
                        int num6 = reader.GetOrdinal("IsPrivate");
                        while (reader.Read())
                        {
                            termIDs.Add(reader.GetGuid(num2));
                            lmts.Add(reader.GetDateTime(num3));
                            urls.Add(new Uri(reader.GetString(ordinal)));
                            rgUserRecordIds.Add(reader.GetInt64(num5));
                            titles.Add(reader.IsDBNull(num4) ? null : reader.GetString(num4));
                            isPrivates.Add(reader.GetBoolean(num6));
                        }
                    }
                }
            }
            ss = CreateSocialTags(termIDs, lmts, titles, rgUserRecordIds, urls, isPrivates, null, profiles);
            return ss.ToArray();
        }

        public void AddTag(Uri url, IAveTerm term, string tagTitle, bool isPrivate, long recordId, Guid id, DateTime lastTime)
        {
            AveTerm aveTerm = term as AveTerm;
            if (aveTerm == null)
            {
                return;
            }
            try
            {
                //在SocialTags内插入数据就会在SocialTags_ChangeLog生成一些记录，由于时间一直都是一样的，多次还原导致SocialTags_ChangeLog表的主键PK_SocialTags_ChangeLog冲突
                //User Profile添加Tag默认是当前时间，为了实现修改时间的还原，删除SocialTags_ChangeLog表中时间对应的记录
                //所以将proc_SocialTags_Add存储过程拆分成4部分执行。并添加删除ChangeLog的Command
                using (SqlCommand command = new SqlCommand(AddTagCommand_Ready))
                {
                    command.CommandType = CommandType.Text;
                    command.Parameters.Add("@partitionID", SqlDbType.UniqueIdentifier).Value = PartitionID;
                    command.Parameters.Add("@correlationId", SqlDbType.UniqueIdentifier).Value = new AveOULS().CorrelationGet();
                    command.Parameters.Add("@user_recordID", SqlDbType.BigInt).Value = recordId;
                    command.Parameters.Add("@userID", SqlDbType.UniqueIdentifier).Value = id;
                    command.Parameters.Add("@url", SqlDbType.NVarChar, 0x824).Value = url.AbsoluteUri;
                    command.Parameters.Add("@termID", SqlDbType.UniqueIdentifier).Value = term.ID;
                    command.Parameters.Add("@inputTermLabel", SqlDbType.NVarChar, 0xff).Value = term.Name;
                    command.Parameters.Add("@isPrivate", SqlDbType.Bit).Value = isPrivate;

                    if (!string.IsNullOrEmpty(tagTitle))
                    {
                        command.Parameters.Add("@title", SqlDbType.NVarChar, 500).Value = tagTitle;
                    }
                    //declare @retVal int
                    SqlParameter parameter = command.Parameters.Add("@urlID", SqlDbType.BigInt);
                    parameter.Direction = ParameterDirection.Output;
                    ExecuteNonQuery(command);

                    if (parameter.Value != null && parameter.Value != DBNull.Value)
                    {
                        command.CommandText = mAddTagCommand_Check;
                        parameter.Direction = ParameterDirection.Input;
                        bool read = false;
                        DateTime time = lastTime;
                        using (SqlDataReader reader = ExecuteReader(command))
                        {
                            if (reader.Read())
                            {
                                read = true;
                                time = reader.GetDateTime(0);
                            }
                        }

                        command.Parameters.Add("@lastModifiedTime", SqlDbType.DateTime).Value = lastTime;
                        command.Parameters.Add("@oldTime", SqlDbType.DateTime).Value = time;
                        command.CommandText = mAddTagCommand_Delete;
                        //Tag多次还原可能会出现异常: SocialTags_ChangeLog表的主键PK_SocialTags_ChangeLog冲突, 所以清除Change Log
                        ExecuteNonQuery(command);
                        if (read)
                        {
                            command.CommandText = mAddTagCommand_Update;
                        }
                        else
                        {
                            command.CommandText = mAddTagCommand_Insert;
                        }
                        ExecuteNonQuery(command);
                    }
                }
            }
            catch (Exception e)
            {
                logger.Log(AveLogLevel.DEBUG, ServerAPIResource.TagAddingError,
                    url.ToString(), tagTitle, term.Name, recordId, lastTime, e);
                try
                {
                    //Tag多次还原可能会出现异常: SocialTags_ChangeLog表的主键PK_SocialTags_ChangeLog冲突
                    mSocialTagManager.AddTag(url, aveTerm.Term, tagTitle, isPrivate);
                }
                catch (Exception ex)
                {
                    logger.Log(AveLogLevel.DEBUG, ServerAPIResource.AddSocialTagError, ex.ToString());
                }
            }
        }

        public void AddTag(Uri uri, IAveTerm term, string tagTitle, bool isPrivate)
        {
            mSocialTagManager.AddTag(uri, (term as AveTerm).Term, tagTitle, isPrivate);
        }

        public void DeleteTag(Uri uri, IAveTerm term)
        {
            mSocialTagManager.DeleteTag(uri, ((AveTerm)term).Term);
        }

        public void DeleteTags(Uri uri)
        {
            mSocialTagManager.DeleteTags(uri);
        }

        //private List<IAveOSocialTag> CreateSocialTags(List<Guid> termIDs, List<DateTime> lmts, List<string> titles, List<long> rgUserRecordIds,List<Uri> urls, List<bool> isPrivates, List<string> inputTermLabels)
        //{
        //    List<SocialTag> socialTags = AveAssemblyUtility.InvokeMethod(mSocialTagManager, mSocialTagManager.GetType(), "CreateSocialTags", new object[] { termIDs, lmts, titles, rgUserRecordIds, urls, isPrivates, null }) as List<SocialTag>;
        //    if (socialTags != null)
        //    {
        //        List<IAveOSocialTag> results = new List<IAveOSocialTag>();
        //        foreach (SocialTag tag in socialTags)
        //        {
        //            AveOSocialTag oSocialTag = new AveOSocialTag(mSocialTagManager, tag);
        //            results.Add(oSocialTag);
        //        }
        //        return results;
        //    }
        //    return null;
        //}
        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "SharePoint method name.")]
        private List<IAveOSocialTag> CreateSocialTags(List<Guid> termIDs, List<DateTime> lmts, List<string> titles, List<long> rgUserRecordIds, List<Uri> urls, List<bool> isPrivates, List<string> inputTermLabels, Dictionary<long, string> profiles)
        {
            List<IAveOSocialTag> list = new List<IAveOSocialTag>();
            bool flag = ((bool)AveAssemblyUtility.GetPropertyValue(mSocialTagManager, "UseLazyTermBinding")) && (inputTermLabels != null);
            if ((termIDs != null) && (termIDs.Count != 0))
            {
                if ((((termIDs.Count != lmts.Count) || (termIDs.Count != titles.Count)) || ((termIDs.Count != rgUserRecordIds.Count) || (termIDs.Count != urls.Count))) || ((termIDs.Count != isPrivates.Count) || (flag && (termIDs.Count != inputTermLabels.Count))))
                {
                    //ULS.SendTraceTag(0x64356667, ULSCat.msoulscat_MOSS_SocialData, ULSTraceLevel.Medium, "Count of data passed to do not match");
                    throw new ArgumentException("Count of data passed to do not match");
                }
                Term[] termsForTermIDs = null;
                if (!flag)
                {
                    termsForTermIDs = (Term[])AveAssemblyUtility.InvokeMethod(mSocialTagManager, "GetTermsForTermIDs", new object[] { termIDs.ToArray() });//mSocialTagManager.GetTermsForTermIDs(termIDs.ToArray());
                }
                GetBulkUserProfiles(rgUserRecordIds, profiles);
                Type[] types = new Type[] { typeof(SocialTagManager), typeof(UserProfile), typeof(Uri), typeof(DateTime), typeof(Term), typeof(string), typeof(bool) };
                Type[] anotherTypes = new Type[] { mSocialTagManager.GetType(), typeof(UserProfile), typeof(Uri), typeof(DateTime), typeof(Guid), typeof(string), typeof(DateTime), typeof(bool) };
                for (int i = 0; i < termIDs.Count; i++)
                {
                    SocialTag tag = null;
                    Term termForTermID = null;
                    if (!flag)
                    {
                        termForTermID = termsForTermIDs[i];
                    }
                    else if (inputTermLabels[i] == null)
                    {
                        termForTermID = (Term)AveAssemblyUtility.InvokeMethod(mSocialTagManager, "GetTermForTermID", new object[] { termIDs[i] });//mSocialTagManager.GetTermForTermID(termIDs[i]);
                    }
                    if ((termForTermID == null) && (!flag || (inputTermLabels[i] == null)))
                    {
                        //ULS.SendTraceTag(0x65703230, ULSCat.msoulscat_MOSS_SocialData, ULSTraceLevel.Medium, "Term not available for TermID: {0}", new object[] { termIDs[i] });
                    }
                    else if (!profiles.ContainsKey(rgUserRecordIds[i]))
                    {
                        //ULS.SendTraceTag(0x64356669, ULSCat.msoulscat_MOSS_SocialData, ULSTraceLevel.Medium, "UserProfile not found for Recordid: {0}", new object[] { rgUserRecordIds[i] });
                    }
                    else
                    {
                        Uri deserializedUrl = (Uri)AveAssemblyUtility.InvokeStaticMethod("Microsoft.Office.Server.SocialData.AlternateAccessMapping", "GetDeserializedUrl", new object[] { urls[i] });
                        if (termForTermID != null)
                        {
                            //this, bulkUserProfiles[i], deserializedUrl, lmts[i], termForTermID, titles[i], isPrivates[i]
                            tag = (SocialTag)AveAssemblyUtility.CreateInstance(typeof(SocialTag)
                                , types
                                , new object[] { mSocialTagManager, null, deserializedUrl, lmts[i], termForTermID, titles[i], false });
                        }
                        else
                        {
                            tag = (SocialTag)AveAssemblyUtility.CreateInstance(typeof(SocialTag)
                                , anotherTypes
                                , new object[] { mSocialTagManager, null, deserializedUrl, lmts[i], termIDs[i], inputTermLabels[i], titles[i], false });
                        }
                    }
                    if (tag != null)
                    {
                        AveOSocialTag aveTag = new AveOSocialTag(mSocialTagManager, tag);
                        aveTag.OwnerName = profiles[rgUserRecordIds[i]];
                        if (isPrivates[i])
                        {
                            AveAssemblyUtility.SetFieldValue(tag, "m_IsPrivate", true);
                        }
                        list.Add(aveTag);
                    }
                }
            }
            return list;
        }

        #endregion
    }
}
