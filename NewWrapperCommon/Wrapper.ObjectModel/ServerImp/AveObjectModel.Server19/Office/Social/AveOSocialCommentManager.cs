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
using System.Reflection;
using AvePoint.GCommon;
using AvePoint.Wrapper.Common;
using AvePoint.Wrapper.Common.Office;
using AvePoint.Wrapper.Resource.ServerAPI2010;
using AvePoint.Wrapper.Resource.ServerAPI2010;
using Microsoft.Office.Server.Infrastructure;
using Microsoft.Office.Server.SocialData;
using Microsoft.Office.Server.UserProfiles;
using Microsoft.SharePoint.Utilities;

namespace AvePoint.ObjectModel.Server19.Office
{
    class AveOSocialCommentManager : AveOSocialDataManager, IAveOSocialCommentManager
    {
        private static AveLogger log = AveLogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);
        private SocialCommentManager mSocialCommentManager;
        private const string mAddCommentCommand = @"SET NOCOUNT ON;

	-- cache this user...
	declare @retVal int
	exec @retVal = proc_Social_EnsureUserID @partitionID = @partitionID, @user_recordID = @user_recordID, @userID = @userID, @correlationId = @correlationId;
	if (@retVal != 0)
		return;

	-- cache this url...
	declare @urlID bigint;
	exec proc_Social_EnsureUrlID @partitionID = @partitionID, @url = @url, @urlID = @urlID output, @correlationId = @correlationId;
	if (@urlID is null)
		return;
 if exists (select TOP 1 1 from SocialComments (NOLOCK)
			where PartitionID = @partitionID and User_RecordID = @user_recordID and LastModifiedTime=@lastModifiedTime and UrlID = @urlID and Comment = @comment)
    return;
else
	insert into SocialComments (PartitionID, UrlID, User_RecordID, Comment, LastModifiedTime, IsHighPriority, Title) values (@partitionID, @urlID, @user_recordID, @comment, @lastModifiedTime, @isHighPriority, @title);";

        private const string mDeleteCommentChangeLog = "DELETE FROM SocialComments_ChangeLog WHERE partitionID = @partitionID AND lastModifiedTime = @lastModifiedTime AND user_recordID = @user_recordID";

        public AveOSocialCommentManager(SocialCommentManager socialCommentManager)
            : base(socialCommentManager)
        {
            mSocialCommentManager = socialCommentManager;
        }

        public AveOSocialCommentManager(IAveServiceContext serviceContentext)
            : this(new SocialCommentManager((serviceContentext as AveServiceContext).ServiceContext))
        { }

        #region IAveSocialCommentManager Members

        public IAveOSocialComment[] GetComments(IAveOUserProfile user)
        {
            SocialComment[] socialComment = mSocialCommentManager.GetComments((user as AveOUserProfile).UserProfile);
            AveOSocialComment[] aveSocialComment = new AveOSocialComment[socialComment.Length];
            for (int i = 0; i < socialComment.Length; i++)
            {
                aveSocialComment[i] = new AveOSocialComment(mSocialCommentManager, socialComment[i]);
            }
            return aveSocialComment;
        }

        public void AddComment(Uri uri, string comment, bool isHighPriority)
        {
            mSocialCommentManager.AddComment(uri, comment, isHighPriority);
        }

        public void AddComment(Uri url, string comment, bool isHighPriority, string title, DateTime modifiedTime, long recordId, Guid id)
        {
            try
            {
                if (null == url)
                {
                    throw new ArgumentNullException("url");
                }
                if (comment == null)
                {
                    throw new ArgumentNullException("comment");
                }
                if (comment.Length == 0)
                {
                    throw new ArgumentException(null, "comment");
                }
                SPUtility.ValidateFormDigest();
                string str = SafeHtmlWrapper.MakeSafe(comment);
                url = new AveOAlternateAccessMapping().GetSerializedUrl(url);
                using (SqlCommand command = new SqlCommand(mDeleteCommentChangeLog))
                {//还原至同一Site时 会导致数据库中SocialComments_ChangeLog表中主键内容冲突，故先删除ChangeLog中对应行
                    command.CommandType = CommandType.Text;
                    command.Parameters.Add("@partitionID", SqlDbType.UniqueIdentifier).Value = base.PartitionID;
                    command.Parameters.Add("@user_recordID", SqlDbType.BigInt).Value = recordId;
                    command.Parameters.Add("@lastModifiedTime", SqlDbType.DateTime).Value = modifiedTime;
                    base.UserProfileApplicationProxy.SocialDBSqlSession.ExecuteNonQuery(command);
                }
                using (SqlCommand command = new SqlCommand(mAddCommentCommand))
                {
                    command.CommandType = CommandType.Text;
                    command.Parameters.Add("@partitionID", SqlDbType.UniqueIdentifier).Value = base.PartitionID;
                    command.Parameters.Add("@correlationId", SqlDbType.UniqueIdentifier).Value = new AveOULS().CorrelationGet();
                    command.Parameters.Add("@user_recordID", SqlDbType.BigInt).Value = recordId;
                    command.Parameters.Add("@userID", SqlDbType.UniqueIdentifier).Value = id;
                    command.Parameters.Add("@url", SqlDbType.NVarChar, 0x824).Value = url.AbsoluteUri;
                    command.Parameters.Add("@comment", SqlDbType.NVarChar, 0xfa0).Value = str;
                    command.Parameters.Add("@isHighPriority", SqlDbType.Bit).Value = isHighPriority;
                    if (!string.IsNullOrEmpty(title))
                    {
                        command.Parameters.Add("@title", SqlDbType.NVarChar, 500).Value = title;
                    }
                    command.Parameters.Add("@lastModifiedTime", SqlDbType.DateTime).Value = modifiedTime;
                    base.UserProfileApplicationProxy.SocialDBSqlSession.ExecuteNonQuery(command);
                }
            }
            catch (Exception e)
            {
                log.Log(AveLogLevel.DEBUG, ServerAPIResource.AddSQLComPraError, e.ToString());
                try
                {
                    //使用该方法会导致添加Comment的User为当前Agent的User
                    mSocialCommentManager.AddComment(url, comment, isHighPriority);
                }
                catch(Exception ex)
                {
                    log.Log(AveLogLevel.DEBUG, ServerAPIResource.AddSQLComPraError, ex.ToString());
                }
            }
        }

        public IAveOSocialComment[] GetComments(string url, Dictionary<long, string> profiles)
        {
            Uri uri = new AveOAlternateAccessMapping().GetSerializedUrl(new Uri(url));

            List<long> commentIDList = new List<long>();
            List<DateTime> timeList = new List<DateTime>();
            List<string> commentList = new List<string>();
            List<bool> priorityList = new List<bool>();
            List<string> titleList = new List<string>();
            List<long> userRecordIDs = new List<long>();
            List<Uri> urls = new List<Uri>();
            using (SqlCommand command = new SqlCommand("dbo.proc_SocialComments_GetForUrlByTime"))
            {
                command.CommandType = CommandType.StoredProcedure;
                command.Parameters.Add("@partitionID", SqlDbType.UniqueIdentifier).Value = base.PartitionID;
                command.Parameters.Add("@correlationId", SqlDbType.UniqueIdentifier).Value = new AveOULS().CorrelationGet();
                command.Parameters.Add("@url", SqlDbType.NVarChar, 0x824).Value = uri.OriginalString;

                command.Parameters.Add("@fromDate", SqlDbType.DateTime).Value = new DateTime(1754, 1, 1, 0, 0, 0, 0);
                command.Parameters.Add("@toDate", SqlDbType.DateTime).Value = new DateTime(9999, 12, 31, 0, 0, 0, 0);

                using (SqlDataReader reader = base.UserProfileApplicationProxy.SocialDBSqlSession.ExecuteReader(command))
                {
                    if (reader.HasRows)
                    {
                        int ordinal = reader.GetOrdinal("CommentID");
                        int num2 = reader.GetOrdinal("User_RecordID");
                        int num3 = reader.GetOrdinal("LastModifiedTime");
                        int num4 = reader.GetOrdinal("Comment");
                        int num5 = reader.GetOrdinal("IsHighPriority");
                        int num6 = reader.GetOrdinal("Title");
                        int num7 = reader.GetOrdinal("Url");

                        while (reader.Read())
                        {
                            userRecordIDs.Add(reader.GetInt64(num2));
                            commentIDList.Add(reader.GetInt64(ordinal));
                            timeList.Add(reader.GetDateTime(num3));
                            commentList.Add(reader.GetString(num4));
                            priorityList.Add(reader.GetBoolean(num5));
                            titleList.Add(reader.IsDBNull(num6) ? null : reader.GetString(num6));
                            urls.Add(new Uri(reader.GetString(num7)));
                        }
                    }
                }
            }
            List<IAveOSocialComment> data = CreateSocialComments(urls, commentIDList, timeList, commentList, priorityList, titleList, userRecordIDs, profiles);
            return data.ToArray();
        }

        private List<IAveOSocialComment> CreateSocialComments(List<Uri> uriList, List<long> commentIDList, List<DateTime> timeList, List<string> commentList, List<bool> priorityList, List<string> titleList, List<long> userRecordIDs, Dictionary<long, string> profiles)
        {
            GetBulkUserProfiles(userRecordIDs, profiles);
            List<IAveOSocialComment> comments = new List<IAveOSocialComment>();
            //SocialCommentManager manager, long commentID, UserProfile owner, Uri url, DateTime lastModifiedTime, string comment, bool isHighPriority, string title)
            Type[] types = new Type[] { typeof(SocialCommentManager), typeof(long), typeof(UserProfile), typeof(Uri), typeof(DateTime), typeof(string), typeof(bool), typeof(string) };
            for (int i = 0; i < commentIDList.Count; i++)
            {
                if (profiles.ContainsKey(userRecordIDs[i]))
                {
                    SocialComment comment = (SocialComment)AveAssemblyUtility.CreateInstance(typeof(SocialComment), types
                        , new object[] { mSocialCommentManager, commentIDList[i], null, uriList[i], timeList[i], commentList[i], priorityList[i], titleList[i] });

                    AveOSocialComment aveCom = new AveOSocialComment(mSocialCommentManager, comment);
                    aveCom.OwnerName = profiles[userRecordIDs[i]];

                    comments.Add(aveCom);
                }
            }
            return comments;
        }

        #endregion

        private Action<SocialCommentManager,Uri, DateTime> deleteCommentFuc;
        public void DeleteComment(Uri uri, DateTime dateTime)
        {
            if (deleteCommentFuc == null)
            {
                var method = typeof(SocialCommentManager).GetMethod("DeleteComment", BindingFlags.IgnoreCase | BindingFlags.Instance | BindingFlags.NonPublic,null,
                           new Type[] { typeof(Uri), typeof(DateTime) }, null);
                deleteCommentFuc = (Action<SocialCommentManager, Uri, DateTime>)Delegate.CreateDelegate(typeof(Action<SocialCommentManager, Uri, DateTime>), method);
            }
            deleteCommentFuc(mSocialCommentManager,uri, dateTime);
        }
    }
}
