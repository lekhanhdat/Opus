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

namespace ExchangeUtility.Graph
{
    #region namespace
    using AvePoint.GCommon.GraphAPI;
    using ExchangeCommonWrapper;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Xml;
    #endregion

    public static class ExchangePlannerConverter
    {
        public static Office365PlanBasicProperties ToM(this GraphPlannerPlan lpValue)
        {
            return new Office365PlanBasicProperties()
            {
                Id = lpValue.Id,
                Title = lpValue.Title,
                Owner = lpValue.Owner,
                OdataEtag = lpValue.OdataEtag,
                CreatedDateTime = lpValue.CreatedDateTime,
                CreateByUserId = lpValue.CreatedBy?.User?.Id,
                CreateByUserName = lpValue.CreatedBy?.User?.DisplayName,
                CreateByApplicationId = lpValue.CreatedBy?.Application?.Id,
                CreateByApplicationName = lpValue.CreatedBy?.Application?.DisplayName,
            };
        }
        public static Office365PlanDetailsProperties ToM(this GraphPlannerPlanDetails gpDetailsObj)
        {
            return new Office365PlanDetailsProperties()
            {
                Id = gpDetailsObj.Id,
                OdataEtag = gpDetailsObj.OdataEtag,
                OdataContext = gpDetailsObj.OdataContext,
                CategoryDescriptionsDictionary = gpDetailsObj.CategoryDescriptions,
                SharedWith = new Dictionary<string, bool>(gpDetailsObj.SharedWith)
            };
        }
        public static Office365PlannerTaskBasicProperties ToM(this GraphPlannerTask lptValue)
        {
            return new Office365PlannerTaskBasicProperties()
            {
                Id = lptValue.Id,
                Title = lptValue.Title,
                PlanId = lptValue.PlanId,
                BucketId = lptValue.BucketId,
                OdataEtag = lptValue.OdataEtag,
                OrderHint = lptValue.OrderHint,
                PreviewType = lptValue.PreviewType,
                DueDateTime = lptValue.DueDateTime,
                StartDateTime = lptValue.StartDateTime,
                ReferenceCount = lptValue.ReferenceCount,
                HasDescription = lptValue.HasDescription,
                CreatedDateTime = lptValue.CreatedDateTime,
                PercentComplete = lptValue.PercentComplete,
                AssigneePriority = lptValue.AssigneePriority,
                CompletedDateTime = lptValue.CompletedDateTime,
                ChecklistItemCount = lptValue.ChecklistItemCount,
                ConversationThreadId = lptValue.ConversationThreadId,
                ActiveChecklistItemCount = lptValue.ActiveChecklistItemCount,
                CreatedByUserId = lptValue.CreatedBy?.User?.Id,
                CreatedByUserName = lptValue.CreatedBy?.User?.DisplayName,
                CompletedByUserId = lptValue.CompletedBy?.User?.Id,
                CompletedByUserName = lptValue.CompletedBy?.User?.DisplayName,
                Assignments = lptValue.Assignments.Select(taskAssignment => taskAssignment.ToM()).ToList(),
                LabelDictionary = lptValue.AppliedCategories,
                Priority = lptValue.Priority,
            };
        }
        public static TaskAssignment ToM(this KeyValuePair<string, GPTAssignmentValue> taskAssignment)
        {
            return new TaskAssignment()
            {
                AssignmentId = taskAssignment.Key,
                AssignedDateTime = taskAssignment.Value.AssignedDateTime,
                OdataType = taskAssignment.Value.OdataType,
                OrderHint = taskAssignment.Value.OrderHint,
                AssignedByUserId = taskAssignment.Value.AssignedBy?.User?.Id,
                AssignedByUserName = taskAssignment.Value.AssignedBy?.User?.DisplayName,
            };
        }
        public static Office365PlannerTaskDetailsProperties ToM(this GraphPlannerTaskDetails gptDetailsObj)
        {
            return new Office365PlannerTaskDetailsProperties()
            {
                Id = gptDetailsObj.Id,
                OdataEtag = gptDetailsObj.OdataEtag,
                Description = gptDetailsObj.Description,
                PreviewType = gptDetailsObj.PreviewType,
                OdataContext = gptDetailsObj.OdataContext,
                References = gptDetailsObj.References.Select(reference => reference.ToM()).ToList(),
                Checklist = gptDetailsObj.Checklist.Select(checkListItem => checkListItem.ToM()).ToList(),
            };
        }
        public static TaskReference ToM(this KeyValuePair<string, GPTDReferencesValue> taskReference)
        {
            return new TaskReference()
            {
                Type = taskReference.Value.Type,
                ReferencesId = taskReference.Key,
                Alias = taskReference.Value.Alias,
                OdataType = taskReference.Value.OdataType,
                PreviewPriority = taskReference.Value.PreviewPriority,
                ReferencesLastModifiedDateTime = taskReference.Value.LastModifiedDateTime,
                ReferencesLastModifiedByUserId = taskReference.Value.LastModifiedBy?.User?.Id,
                ReferencesLastModifiedByUserName = taskReference.Value.LastModifiedBy?.User?.DisplayName,
            };
        }
        public static TaskCheckList ToM(this KeyValuePair<string, GPTDCheckListValue> checkListItem)
        {
            return new TaskCheckList()
            {
                ChecklistId = checkListItem.Key,
                Title = checkListItem.Value.Title,
                OdataType = checkListItem.Value.OdataType,
                OrderHint = checkListItem.Value.OrderHint,
                IsChecked = checkListItem.Value.IsChecked,
                CheckListLastModifiedDateTime = checkListItem.Value.LastModifiedDateTime,
                CheckListLastModifiedByUserId = checkListItem.Value.LastModifiedBy?.User?.Id,
                CheckListLastModifiedByUserName = checkListItem.Value.LastModifiedBy?.User?.DisplayName,
            };
        }
        public static Office365PlannerTaskBucketProperties ToM2(this GraphPlannerBucket gptBucketObj)
        {
            return new Office365PlannerTaskBucketProperties()
            {
                Id = gptBucketObj.Id,
                PlanId = gptBucketObj.PlanId,
                Name = gptBucketObj.Name,
                OrderHint = gptBucketObj.OrderHint,
                OdataEtag = gptBucketObj.OdataEtag,
            };
        }
        public static Office365PlannerBucketProperties ToM(this GraphPlannerBucket lpbObj)
        {
            return new Office365PlannerBucketProperties()
            {
                Id = lpbObj.Id,
                Name = lpbObj.Name,
                PlanId = lpbObj.PlanId,
                OrderHint = lpbObj.OrderHint,
                OdataEtag = lpbObj.OdataEtag,
            };
        }
        public static TaskComment ToM(this GraphTaskComment lptcValue)
        {
            return new TaskComment()
            {
                Id = lptcValue.Id,
                OdataEtag = lptcValue.OdataEtag,
                ChangeKey = lptcValue.ChangeKey,
                Categories = lptcValue.Categories,
                BodyContent = AnalyzePosts(lptcValue.Body.Content),
                BodyType = lptcValue.Body.ContentType,
                HasAttachments = lptcValue.HasAttachments,
                CreatedDateTime = lptcValue.CreatedDateTime,
                ReceivedDateTime = lptcValue.ReceivedDateTime,
                FromEmailName = lptcValue.From?.EmailAddress?.Name,
                SenderEmailName = lptcValue.Sender?.EmailAddress?.Name,
                LastModifiedDateTime = lptcValue.LastModifiedDateTime,
                FromEmailAddress = lptcValue.From?.EmailAddress?.Address,
                SenderEmailAddress = lptcValue.Sender?.EmailAddress?.Address,
            };
        }
        public static CreatePlannerPlanObj ToCreateObj(this Office365PlanBasicProperties planBasicProperties, String groupId)
        {
            return new CreatePlannerPlanObj()
            {
                Owner = groupId,
                Title = planBasicProperties.Title,
            };
        }
        public static UpdatePlannerPlanDetailsObj ToUpdateObj(this Office365PlanDetailsProperties planDetailsProperties)
        {
            return new UpdatePlannerPlanDetailsObj()
            {
                CategoryDescriptions = planDetailsProperties.CategoryDescriptionsDictionary
            };
        }
        public static CreatePlannerBucketObj ToCreateObj(this Office365PlannerTaskBucketProperties taskBucketProperties, String planId, String orderHint = " !")
        {
            return new CreatePlannerBucketObj()
            {
                Name = taskBucketProperties.Name,
                PlanId = planId,
                OrderHint = orderHint //OrderHint 必须转化才能使用，否则会 BedRequest
            };
        }
        public static CreatePlannerTaskObj ToCreateObj(this Office365PlannerTaskBasicProperties taskBasicProperties, String planId, String bucketId)
        {
            return new CreatePlannerTaskObj()
            {
                PlanId = planId,
                BucketId = bucketId,
                Title = taskBasicProperties.Title,
                Assignments = new Dictionary<string, CPTAssignmentValue>(),
            };
        }
        public static UpdatePlannerTaskObj ToUpdateObj(this Office365PlannerTaskBasicProperties taskBasicProperties, string newBucketId, string conversationThreadId = "")
        {
            return new UpdatePlannerTaskObj()
            {
                BucketId = newBucketId,
                Title = taskBasicProperties.Title,
                OrderHint = " !",
                PercentComplete = taskBasicProperties.PercentComplete,
                StartDateTime = taskBasicProperties.StartDateTime,
                DueDateTime = taskBasicProperties.DueDateTime,
                ConversationThreadId = String.IsNullOrEmpty(conversationThreadId) ? taskBasicProperties.ConversationThreadId : conversationThreadId,
                AppliedCategories = taskBasicProperties.LabelDictionary,
                Assignments = (null == taskBasicProperties.Assignments)
                ? new Dictionary<string, UTAssignmentValue>() : taskBasicProperties.Assignments.ToDictionary
                (
                    key => key.AssignmentId,
                    value => new UTAssignmentValue()
                    {
                        OdataType = value.OdataType,
                        OrderHint = " !",    //OrderHint 必须转化才能使用，否则会 BedRequest
                    }
                ),
                Priority = taskBasicProperties.Priority,
            };
        }
        public static UpdatePlannerTaskDetailsObj ToUpdateObj(this Office365PlannerTaskDetailsProperties taskDetailsProperties, string siteWebUrl = "")
        {
            return new UpdatePlannerTaskDetailsObj()
            {
                Description = taskDetailsProperties.Description,
                PreviewType = taskDetailsProperties.PreviewType,
                Checklist = taskDetailsProperties.Checklist.ToUpdateObj(),
                References = taskDetailsProperties.References.ToUpdateObj(siteWebUrl),
            };
        }
        public static Dictionary<string, UTDCheckListValue> ToUpdateObj(this List<TaskCheckList> checklist)
        {
            if (null == checklist || !checklist.Any()) return new Dictionary<string, UTDCheckListValue>();
            checklist.Sort((TaskCheckList T1, TaskCheckList T2) => { return String.CompareOrdinal(T1.OrderHint, T2.OrderHint); });
            var tempLeft = 0;
            var tempRight = 1;
            return checklist.ToDictionary
                (
                   key => key.ChecklistId,
                   value => new UTDCheckListValue()
                   {
                       OdataType = value.OdataType,
                       Title = value.Title,
                       IsChecked = value.IsChecked,
                       OrderHint = OrderHintsSort.FixIntegerOrderHint(tempLeft++, tempRight++),
                   }
                );
        }
        public static Dictionary<string, UTDReferencesValue> ToUpdateObj(this List<TaskReference> references, String siteWebUrl)
        {
            if (null == references || !references.Any()) return new Dictionary<string, UTDReferencesValue>();
            references.Sort((TaskReference T1, TaskReference T2) => { return String.CompareOrdinal(T1.PreviewPriority, T2.PreviewPriority); });
            var tempLeft = 0;
            var tempRight = 1;
            return references.ToDictionary
                (
                    key =>
                    {
                        if (String.IsNullOrEmpty(siteWebUrl)) { return key.ReferencesId; }
                        var flagIndex = key.ReferencesId.IndexOf("/Shared%2520Documents/");
                        if (flagIndex < 0) flagIndex = key.ReferencesId.IndexOf("/Shared Documents/");
                        if (flagIndex < 0) { return key.ReferencesId; }
                        var sb = new StringBuilder(siteWebUrl);
                        //由于某些地方需要key匹配，只能手动转义
                        sb.Replace(".", "%2E");
                        sb.Replace(":", "%3A");
                        sb.Append(key.ReferencesId.Substring(flagIndex));
                        return sb.ToString();
                    },
                    value => new UTDReferencesValue()
                    {
                        OdataType = value.OdataType,
                        Alias = value.Alias,
                        Type = value.Type,
                        PreviewPriority = OrderHintsSort.FixIntegerOrderHint(tempLeft++, tempRight++),
                    }
                );
        }

        public static AddPlannerTaskCommentObj ToAddObj(this TaskComment taskComment)
        {
            return new AddPlannerTaskCommentObj(taskComment.BodyType, AddRestoreFlag(taskComment));
        }

        public static ReplyMessageObj ToReplyMessageObj(this TaskComment taskComment, string groupMail)
        {
            return new ReplyMessageObj()
            {
                Message = new RMMessage()
                {
                    ToRecipients = new List<MailRecipients>() { new MailRecipients() { EmailAdress = new MailEmailAdress() { Address = groupMail } } }
                },
                Comment = AddRestoreFlag(taskComment)
            };
        }

        public static SendMailObj ConvertCommentToMail(TaskComment taskComment, string topic, string groupMail)
        {
            var postBody = AddRestoreFlag(taskComment);
            return new SendMailObj()
            {
                Message = new SMMessage()
                {
                    InternetMessageId = Guid.NewGuid().ToString(),
                    Subject = topic,
                    Body = new MailBody()
                    {
                        ContentType = taskComment.BodyType,
                        Content = postBody
                    },
                    ToRecipients = new List<MailRecipients> { new MailRecipients() { EmailAdress = new MailEmailAdress() { Address = groupMail } }
                    }
                },
                SaveToSentItems = "true"
            };
        }

        public static string AnalyzePosts(string html)
        {
            if (String.IsNullOrEmpty(html)) { return String.Empty; }
            try
            {
                var xdoc = new XmlDocument();
                xdoc.LoadXml(html.Replace("<br>", "&lt;br&gt;"));
                var tableNode = xdoc.SelectSingleNode("//table");
                if (null == tableNode)
                {
                    return xdoc.InnerText.Trim();
                }
                var parentNode = tableNode.ParentNode;
                parentNode.RemoveChild(tableNode);
                return parentNode.InnerText.Trim();
            }
            catch
            {
                return html;
            }
        }

        public static string AddRestoreFlag(TaskComment taskComment)
        {
            if (taskComment.BodyContent.Contains("[Restore flag]"))
            {
                return taskComment.BodyContent.Replace("[Restore flag] By", "This comment was originally added by.");
            }
            if (taskComment.BodyContent.Contains("This comment was originally added by"))
            {
                return taskComment.BodyContent;
            }
            else
            {
                return $"{taskComment.BodyContent}<br>This comment was originally added by \"{taskComment.SenderEmailAddress}\" on {taskComment.ReceivedDateTime.ToPostedTime()}.";
            }
        }
    }

    #region Function class
    public static class OrderHintsSort
    {
        #region Definition
        /// <summary>
        /// 支持的最大精度位(50)的最大左值
        /// </summary>
        static readonly String maxLeft = "~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~|";
        /// <summary>
        /// 使用 maxOrderHint 后返回的值
        /// </summary>
        static readonly String maxValue = "~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~}";
        static readonly String minRight = "\"\"\"\"\"\"\"\"\"\"\"\"\"\"\"\"\"\"\"\"\"\"\"\"\"\"\"\"\"\"\"\"\"\"\"\"\"\"\"\"\"\"\"\"\"\"\"\"\"$";
        static readonly String minValue = "\"\"\"\"\"\"\"\"\"\"\"\"\"\"\"\"\"\"\"\"\"\"\"\"\"\"\"\"\"\"\"\"\"\"\"\"\"\"\"\"\"\"\"\"\"\"\"\"\"#";
        /// <summary>
        /// 支持的用于指定的最大精度值的位置
        /// </summary>
        static readonly String maxOrderHint = String.Format("{0} !", maxLeft);
        static readonly String minOrderHint = String.Format(" {0}!", minRight);
        static readonly Int32 maxPrecision = 50;
        #endregion

        /// <summary>
        /// 排序并得到新的位置的 Dictionary
        /// </summary>
        /// <param name="orderHintList"></param>
        /// <returns></returns>
        public static Dictionary<String, String> ToSortDictionary(List<String> orderHintList, Boolean desc = false)
        {
            if (!orderHintList.Any()) return new Dictionary<string, string>();
            orderHintList.Sort(String.CompareOrdinal);
            if (desc) orderHintList.Reverse();
            var tempLeft = String.Empty;
            var tempDic = new Dictionary<String, String>();
            tempDic.Add(minValue, minOrderHint);
            for (int i = 0; i < orderHintList.Count; i++)
            {
                var tempRight = orderHintList[i];
                try
                {
                    tempDic.Add(tempRight, String.Concat(tempLeft, " ", tempRight, "!"));
                    tempLeft = tempRight;
                }
                catch (Exception e)
                {
                    Logger.Error($"The sort failed when the sort reached the number of {i}. error message : {e.Message}");
                }
            }
            return tempDic;
        }
        /// <summary>
        /// 排序并得到新的位置的 Dictionary,新位置用相对顺序的数字如："0 1!"
        /// </summary>
        /// <param name="orderHintList"></param>
        /// <returns></returns>
        public static Dictionary<String, String> ToSimpleSortDictionary(List<String> orderHintList, Boolean desc = false)
        {
            if (!orderHintList.Any()) return new Dictionary<string, string>();
            orderHintList.Sort(String.CompareOrdinal);
            if (desc) orderHintList.Reverse();
            var tempDic = new Dictionary<String, String>();
            for (int i = 0; i < orderHintList.Count; i++)
            {
                try
                {
                    tempDic.Add(orderHintList[i], FixIntegerOrderHint(i, i + 1)); ;
                }
                catch (Exception e)
                {
                    Logger.Error($"The simple sort failed when the sort reached the number of {i}. error message : {e.Message}");
                }
            }
            return tempDic;
        }
        public static string FixIntegerOrderHint(Int32 left, Int32 right)
        {
            return $"{left.ToString().PadLeft(10, '0')} {right.ToString().PadLeft(10, '0')}!";
        }

        /// <summary>
        /// 计算新的相对位置
        /// </summary>
        public static String CalculateOrderHint(String orderHint)
        {
            //首先方案一： 用最小精度补位到最大精度位数 50位，
            if (maxPrecision - orderHint.Length > 0)
            {
                var sBuilder = new StringBuilder(orderHint);
                sBuilder.Append(" ");
                sBuilder.Append(orderHint);
                for (int i = 1; i < maxPrecision - orderHint.Length; i++)
                {
                    sBuilder.Append("\"");
                }
                sBuilder.Append("$");
                return sBuilder.ToString();
            }
            //备选方案一： 处理精度值溢出，
            if (String.CompareOrdinal(orderHint, maxLeft) >= 0) return maxOrderHint;
            //备选方案二： 最小精度值增长
            var asciiList = StringToAsciiList(orderHint);
            var sl = asciiList.Count;
            asciiList.Insert(0, 0);//首位插入临时位
            asciiList[sl] += 2;//末位+2
            for (int i = sl; i > 0; i--)
            {
                if (asciiList[i] > 176)
                {
                    asciiList[i] = 0;
                    asciiList[i - 1] += 2;
                }
                else
                {
                    break;
                }
            }
            if (asciiList[0] > 0) return maxOrderHint;
            asciiList.Remove(0);//去除首位临时位
            var resultStr = new String(asciiList.Select(ascii => (Char)ascii).ToArray());
            return String.Format("{0} {1}!", orderHint, resultStr);
        }
        private static List<Int32> StringToAsciiList(String str)
        {
            str = str.Substring(0, maxPrecision).Trim();//限制最大精度位 ：50位
            var asciiList = str.Select(c => (Int32)c).ToList();
            return asciiList;
        }

    }

    #endregion

}