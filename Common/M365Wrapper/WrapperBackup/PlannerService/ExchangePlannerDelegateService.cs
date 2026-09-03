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

using AvePoint.GCommon.GraphAPI;
using M365.Wrapper.Backup.Auth.Common;
using System;
using System.Collections.Generic;
using System.Linq;


public class ExchangePlannerDelegateService : ExchangePlannerService
{
    public ExchangePlannerDelegateService(IAppTokenAuthObject authObj) : base(authObj)
    {

    }

    public override bool CreateConversationThread(CreateConversationThreadInfo createInfo, out (string ConversationThreadId, string MessageId) result)
    {
        var taskProperties = createInfo.TaskProperties;
        var groupId = createInfo.GroupId;
        var firstComment = taskProperties.CommentProperties.Comments.First();
        var postBody = ExchangePlannerConverter.AddRestoreFlag(firstComment);
        var sourceTopic = taskProperties.CommentProperties.Topic;
        var topic = String.IsNullOrEmpty(sourceTopic) ? String.Format("Comments on task \"{0}\"", taskProperties.BasicProperties.Title) : sourceTopic;
        var ccThreadObj = new CreateConversationThreadObj(topic, firstComment.BodyType, postBody);
        result.ConversationThreadId = msGraphAPIService.CreateConversationThread(groupId, ccThreadObj);
        result.MessageId = string.Empty;
        taskProperties.CommentProperties.Comments.Remove(firstComment);
        return CheckConversationReady(groupId, result.ConversationThreadId);
    }

    public override IBatchRequestCollection BuildBatchRequestObj(AddPlannerTaskCommentsInfo addCommnetsInfo)
    {
        var taskComments = addCommnetsInfo.TaskComments;
        var requestId = String.Empty;
        var httpHeaders = new Dictionary<string, string> { { "Content-Type", "application/json" } };
        var groupId = addCommnetsInfo.GroupId;
        var conversationThreadId = addCommnetsInfo.ConversationThreadId;
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
                Url = $"/groups/{groupId}/threads/{conversationThreadId}/reply",
                Body = taskComment.ToAddObj()
            };
            batchRequestObj.Add(requestItem);
        }
        return batchRequestObj;
    }
}
