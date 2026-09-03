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
namespace ExchangeUtility.Graph;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

using ExchangeCommonWrapper;
using AvePoint.GCommon.GraphAPI;
using Microsoft365.Common.Utility;
using Util;
using M365.Wrapper.Backup.Auth.Common;

public class ExchangePlannerAppService : ExchangePlannerService
{
    private readonly string currentUser;
    private readonly Func<string> getToken;

    public bool IsCustomApp { get; }

    public ExchangePlannerAppService(IAppTokenAuthObject authObj) : base(authObj)
    {
        //studoIsCustomApp = (authObj as AOSTokenAuthObjectV2).IsCustomerApp;
        currentUser = authObj.UserName;
        getToken = authObj.GetAccessToken;
    }

    public override bool CreateConversationThread(CreateConversationThreadInfo createInfo, out (string ConversationThreadId, string MessageId) result)
    {
        var taskProperties = createInfo.TaskProperties;
        var groupId = createInfo.GroupId;
        var groupMail = createInfo.GroupMail;
        var topic = string.Empty;
        var internetMessageId = SendMailToCreateThread(taskProperties, groupMail, out topic);
        result.MessageId = "";
        result.ConversationThreadId = "";
        //stuto::result.MessageId = PollyRetry.HandleAsync<Exception, string>(async () => await GetMessageIdByInternetMessageId(currentUser, internetMessageId), ShouldRetry, 10, 18000).Result;
        //studo::result.ConversationThreadId = PollyRetry.HandleAsync<Exception, string>(async () => await GetLastConversationThreadIdByTopic(groupId, topic), ShouldRetry, 10, 18000).Result;
        taskProperties.CommentProperties.Comments.RemoveAt(0);
        return true;
    }

    private bool ShouldRetry(Exception ex)
    {
        if (ex.Message.Equals("MessageIsDraft"))
        {
            logger.Warn("Retry to get message id beacuse of mail is draft.");
            return true;
        }
        if (ex.Message.Equals("ThreadNotFound"))
        {
            logger.Warn("Retry to get thread id beacuse of thread is not ready.");
            return true;
        }
        return false;
    }

    private string SendMailToCreateThread(Office365PlannerTaskEntity taskProperties, string groupMail, out string topic)
    {
        var firstComment = taskProperties.CommentProperties.Comments.FirstOrDefault();
        topic = $"[Restore] Comments on task \"{Regex.Replace(taskProperties.BasicProperties.Title, @"[^a-zA-Z0-9]", string.Empty)}{DateTime.UtcNow}(UTC)\"";
        var sendMailObj = ExchangePlannerConverter.ConvertCommentToMail(firstComment, topic, groupMail);
        msGraphAPIService.SendMail(currentUser, sendMailObj);
        return sendMailObj.Message.InternetMessageId;
    }

    private async Task<string> GetMessageIdByInternetMessageId(string userIdOrUPN, string internetMessageId)
    {
        await Task.Delay(TimeSpan.FromSeconds(2));

        var messages = msGraphAPIService.ListMessageWithInternetMessageId(userIdOrUPN, internetMessageId);
        var message = messages.OrderBy(o => o.CreatedDateTime).FirstOrDefault();
        return message != null && !message.IsDraft ? message.Id : throw new Exception("MessageIsDraft");
    }

    private async Task<string> GetLastConversationThreadIdByTopic(string groupId, string topic)
    {
        await Task.Delay(TimeSpan.FromSeconds(2));

        var targetConversation = msGraphAPIService.ListConversationsBySearch(groupId, topic).FirstOrDefault();
        return targetConversation != null
            ? msGraphAPIService.ListThreadsOfConversation(groupId, targetConversation.Id).First().Id
            : msGraphAPIService.ListRecentCreatedConversationThread(groupId, 50).FirstOrDefault(t => t.Topic == topic)?.Id ?? throw new Exception("ThreadNotFound");
    }

    public override IBatchRequestCollection BuildBatchRequestObj(AddPlannerTaskCommentsInfo addCommnetsInfo)
    {
        var taskComments = addCommnetsInfo.TaskComments;
        var requestId = string.Empty;
        var httpHeaders = new Dictionary<string, string> { { "Content-Type", "application/json" } };
        var messageId = addCommnetsInfo.MessageId;
        var groupMail = addCommnetsInfo.GroupMail;
        var batchRequestObj = msGraphAPIService.CreateBatchRequestObj(20);
        foreach (var taskComment in taskComments)
        {
            var requestItem = new RequestItem()
            {
                //使用 DependsON ,以正确的顺序添加comment
                DependsOn = new string[] { requestId },
                Id = requestId = Guid.NewGuid().ToString("N"),
                Method = "POST",
                Headers = httpHeaders,
                Url = $"users/{currentUser}/messages/{messageId}/reply",
                Body = taskComment.ToReplyMessageObj(groupMail)
            };
            batchRequestObj.Add(requestItem);
        }
        return batchRequestObj;
    }

    public bool ContainsRoles(List<string> scopes)
    {
        var roles = JwtUtil.GetRolesFromToken(getToken());
        return scopes.All(s => roles.Contains(s, StringComparer.OrdinalIgnoreCase));
    }
}