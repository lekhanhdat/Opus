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
using AvePoint.GCommon;
using AvePoint.Wrapper.Common;
using Microsoft.SharePoint;
using Microsoft.SharePoint.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.ObjectModel.ServerSE
{
    public class AveSPCommentStorage : IAveSPCommentStorage
    {
        private Object storage;
        private string assemblyName;
        private static AveLogger log = AveLogger.GetInstance(typeof(AveSPCommentStorage));

        public AveSPCommentStorage(IAveSite stie)
        {
            var site = (stie as AveSite).Site;
            assemblyName = site.GetType().Assembly.FullName;
            storage = AveAssemblyUtility.CreateInstance(assemblyName, "Microsoft.SharePoint.Comments.Storage.SPCommentStorage");
        }

        public AveCommentInfo AddComment(IAveListItem item, AveCommentInfo comment)
        {
            var commentData = AveAssemblyUtility.CreateInstance(assemblyName, "Microsoft.SharePoint.Comments.CommentData");
            if (comment.Parent != null)
            {
                var parent = GetSPComment(item, int.Parse(comment.Parent.Id));
                AveAssemblyUtility.SetPropertyValue(commentData, "ParentComment", parent);
            }
            if (!string.IsNullOrEmpty(comment.OwnerInfo))
            {
                AveAssemblyUtility.SetPropertyValue(commentData, "User", (item.ParentList.ParentWeb.SiteUsers.GetByLoginName(comment.OwnerInfo) as AveUser).User);
            }
            AveAssemblyUtility.SetPropertyValue(commentData, "Text", comment.Text);
            var result = AveAssemblyUtility.InvokeMethod(storage, "AddComment", new object[] { (item as AveListItem).ListItem, commentData });
            var resultComment = AveAssemblyUtility.GetPropertyValue(result, "Comment");
            var newComment = ConvertSPCommentToAveComment(resultComment);
            log.Debug("Finish add comment:source id:{0} ,text:{1}, isReply:{2}, destination id:{3}", comment.Id, comment.Text, comment.Parent == null, newComment.Id);
            return newComment;
        }

        public void DeleteComment(AveCommentInfo comment)
        {
            throw new NotImplementedException();
        }

        public void DeleteComments(IAveListItem item)
        {
            AveAssemblyUtility.InvokeMethod(storage, "DeleteComments", new object[] { (item as AveListItem).ListItem });
        }

        public AveCommentInfo GetComment(IAveListItem commentedItem, int id)
        {
            return ConvertSPCommentToAveComment(GetSPComment(commentedItem, id));
        }

        private object GetSPComment(IAveListItem commentedItem, int id)
        {
            return AveAssemblyUtility.InvokeMethod(storage, "GetComment", new object[] { (commentedItem as AveListItem).ListItem, id });
        }

        public List<AveCommentInfo> GetComments(IAveListItem commentedItem)
        {
            var results = new List<AveCommentInfo>();
            var query = GetQuery();
            RecursGetComments(RealGetComments(commentedItem, results, query), commentedItem, query, results);
            log.Debug(OutputComments(results));
            return results;
        }

        private string OutputComments(List<AveCommentInfo> results)
        {
            var builder = new StringBuilder("SP Comments are:");
            results.ForEach(comment => {
                builder.AppendLine();
                builder.AppendFormat("comment:{0}", comment.Text);
                comment.Replies.ForEach(rp => {
                    builder.AppendLine();
                    builder.Append("      ");
                    builder.AppendFormat("replies:{0}", rp.Text);
                });
            });
            return builder.ToString();
        }

        private void RecursGetComments(object previousResult, IAveListItem commentedItem, object query, List<AveCommentInfo> results)
        {
            if (previousResult != null && (bool)AveAssemblyUtility.GetFieldValue(previousResult, "HasNextLink"))
            {
                AveAssemblyUtility.SetPropertyValue(query, "SkipToken", AveAssemblyUtility.GetFieldValue(previousResult, "PageInfo").ToString());
                RecursGetComments(RealGetComments(commentedItem, results, query), commentedItem, query, results);
            }
        }

        private object RealGetComments(IAveListItem commentedItem, List<AveCommentInfo> results, object query)
        {
            var resultComments = AveAssemblyUtility.InvokeMethod(storage, "GetComments", new object[] { (commentedItem as AveListItem).ListItem, query });
            if (resultComments != null)
            {
                var comments = AveAssemblyUtility.GetFieldValue(resultComments, "Comments") as ICollection;
                foreach (var comment in comments)
                {
                    var resultComment = ConvertSPCommentToAveComment(comment);
                    if (!(bool)AveAssemblyUtility.GetFieldValue(resultComment, "IsReply") && (int)AveAssemblyUtility.GetFieldValue(resultComment, "ReplyCount") > 0)
                    {
                        resultComment.Replies.AddRange(GetReplies(commentedItem, resultComment.Id));
                    }
                    results.Add(resultComment);
                }
            }
            return resultComments;
        }

        private List<AveCommentInfo> GetReplies(IAveListItem commentedItem, string parentId)
        {
            var results = new List<AveCommentInfo>();
            var query = GetQuery();
            AveAssemblyUtility.SetPropertyValue(query, "CommentId", parentId);
            AveAssemblyUtility.SetPropertyValue(query, "GetReplies", true);
            RecursGetComments(RealGetComments(commentedItem, results, query), commentedItem, query, results);
            return results;
        }

        private AveCommentInfo ConvertSPCommentToAveComment(object comment)
        {
            var result = new AveCommentInfo();
            result.Id = AveAssemblyUtility.GetPropertyValue(comment, "Id").ToString();
            result.IsReply = (bool)AveAssemblyUtility.GetPropertyValue(comment, "IsReply");
            result.OwnerInfo = ((SPPrincipalInfo)AveAssemblyUtility.GetPropertyValue(comment, "OwnerInfo")).LoginName;
            result.ParentId = AveAssemblyUtility.GetPropertyValue(comment, "ParentId").ToString();
            result.ReplyCount = (int)AveAssemblyUtility.GetPropertyValue(comment, "ReplyCount");
            result.Text = AveAssemblyUtility.GetPropertyValue(comment, "Text").ToString();
            return result;
        }

        private object GetQuery()
        {
            var query = AveAssemblyUtility.CreateInstance(assemblyName, "Microsoft.SharePoint.Comments.CommentQuery");
            AveAssemblyUtility.SetPropertyValue(query, "Ascending", true);
            return query;
        }

        public bool GetCommentsDisabled(IAveListItem commentedItem)
        {
            throw new NotImplementedException();
        }

        public void SaveComment(AveCommentInfo comment)
        {
            throw new NotImplementedException();
        }

        public void SetCommentsDisabled(IAveListItem commentedItem, bool value)
        {
            throw new NotImplementedException();
        }
    }
}
