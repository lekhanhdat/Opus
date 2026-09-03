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
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace ExchangeCommonWrapper
{
    [DataContract]
    public class Office365PlannerTaskEntity
    {
        [DataMember]
        public Office365PlannerTaskBasicProperties BasicProperties { get; set; }
        [DataMember]
        public Office365PlannerTaskDetailsProperties DetailProperties { get; set; }
        [DataMember]
        public Office365PlannerTaskBucketProperties BucketProperties { get; set; }
        [DataMember]
        public Office365PlannerTaskCommentProperties CommentProperties { get; set; }
    }
    [DataContract]
    public class Office365PlannerTaskBasicProperties
    {
        [DataMember]
        public string OdataEtag { get; set; }
        [DataMember]
        public string PlanId { get; set; }
        [DataMember]
        public string BucketId { get; set; }
        [DataMember]
        public string Title { get; set; }
        [DataMember]
        public string OrderHint { get; set; }
        [DataMember]
        public string AssigneePriority { get; set; }
        [DataMember]
        public int PercentComplete { get; set; }
        [DataMember]
        public string StartDateTime { get; set; }
        [DataMember]
        public string CreatedDateTime { get; set; }
        [DataMember]
        public string DueDateTime { get; set; }
        [DataMember]
        public bool HasDescription { get; set; }
        [DataMember]
        public string PreviewType { get; set; }
        [DataMember]
        public String CompletedDateTime { get; set; }
        [DataMember]
        public int ReferenceCount { get; set; }
        [DataMember]
        public int ChecklistItemCount { get; set; }
        [DataMember]
        public int ActiveChecklistItemCount { get; set; }
        [DataMember]
        public string ConversationThreadId { get; set; }
        [DataMember]
        public string Id { get; set; }
        [DataMember]
        public string CreatedByUserName { get; set; }
        [DataMember]
        public string CreatedByUserId { get; set; }
        [DataMember]
        public string CompletedByUserName { get; set; }
        [DataMember]
        public string CompletedByUserId { get; set; }
        [DataMember]
        public TaskLabels Labels { get; set; }
        [DataMember]
        public Dictionary<string,bool> LabelDictionary { get; set; }
        [DataMember]
        public List<TaskAssignment> Assignments { get; set; }
        [DataMember]
        public Dictionary<string, string> AssignmentNames { get; set; }
        [DataMember]
        public int? Priority { get; set; }
    }
    [DataContract]
    public class Office365PlannerTaskDetailsProperties
    {
        [DataMember]
        public string OdataContext { get; set; }
        [DataMember]
        public string OdataEtag { get; set; }
        [DataMember]
        public string Description { get; set; }
        [DataMember]
        public string PreviewType { get; set; }
        [DataMember]
        public string Id { get; set; }
        [DataMember]
        public List<TaskReference> References { get; set; }
        [DataMember]
        public List<TaskCheckList> Checklist { get; set; }
    }
    [DataContract]
    public class Office365PlannerTaskBucketProperties
    {
        [DataMember]
        public string OdataEtag { get; set; }
        [DataMember]
        public string Name { get; set; }
        [DataMember]
        public string PlanId { get; set; }
        [DataMember]
        public string OrderHint { get; set; }
        [DataMember]
        public string Id { get; set; }
    }
    [DataContract]
    public class Office365PlannerTaskCommentProperties
    {
        [DataMember]
        public string GroupId { get; set; }
        [DataMember]
        public string TaskId { get; set; }
        [DataMember]
        public string Topic { get; set; }
        [DataMember]
        public string ConversationLastDeliveredDateTime { get; set; }
        [DataMember]
        public List<TaskComment> Comments { get; set; }
        [IgnoreDataMember]
        public string CurrentState
        {
            get
            {
                if (string.IsNullOrEmpty(ConversationLastDeliveredDateTime)) return string.Empty;
                return string.Format("{0},{1}", ConversationLastDeliveredDateTime, Comments.Count);
            }
        }
    }

    [DataContract]
    public class TaskComment
    {
        [DataMember]
        public string OdataEtag { get; set; }
        [DataMember]
        public string Id { get; set; }
        [DataMember]
        public string CreatedDateTime { get; set; }
        [DataMember]
        public string LastModifiedDateTime { get; set; }
        [DataMember]
        public string ChangeKey { get; set; }
        [DataMember]
        public object[] Categories { get; set; }
        [DataMember]
        public string ReceivedDateTime { get; set; }
        [DataMember]
        public bool HasAttachments { get; set; }
        [DataMember]
        public string BodyType { get; set; }
        [DataMember]
        public string BodyContent { get; set; }
        [DataMember]
        public string FromEmailName { get; set; }
        [DataMember]
        public string FromEmailAddress { get; set; }
        [DataMember]
        public string SenderEmailName { get; set; }
        [DataMember]
        public string SenderEmailAddress { get; set; }
    }

    [DataContract]
    public class TaskAssignment
    {
        [DataMember]
        public string AssignmentId { get; set; }
        [DataMember]
        public string OdataType { get; set; }
        [DataMember]
        public string AssignedDateTime { get; set; }
        [DataMember]
        public string OrderHint { get; set; }
        [DataMember]
        public string AssignedByUserName { get; set; }
        [DataMember]
        public string AssignedByUserId { get; set; }
    }

    [DataContract]
    public class TaskLabels
    {
        [DataMember]
        public bool Label1 { get; set; }

        [DataMember]
        public bool Label2 { get; set; }

        [DataMember]
        public bool Label3 { get; set; }

        [DataMember]
        public bool Label4 { get; set; }

        [DataMember]
        public bool Label5 { get; set; }

        [DataMember]
        public bool Label6 { get; set; }

        [DataMember]
        public bool Label7 { get; set; }

        [DataMember]
        public bool Label8 { get; set; }

        [DataMember]
        public bool Label9 { get; set; }

        [DataMember]
        public bool Label10 { get; set; }

        [DataMember]
        public bool Label11 { get; set; }

        [DataMember]
        public bool Label12 { get; set; }

        [DataMember]
        public bool Label13 { get; set; }

        [DataMember]
        public bool Label14 { get; set; }

        [DataMember]
        public bool Label15 { get; set; }

        [DataMember]
        public bool Label16 { get; set; }

        [DataMember]
        public bool Label17 { get; set; }

        [DataMember]
        public bool Label18 { get; set; }

        [DataMember]
        public bool Label19 { get; set; }

        [DataMember]
        public bool Label20 { get; set; }

        [DataMember]
        public bool Label21 { get; set; }

        [DataMember]
        public bool Label22 { get; set; }

        [DataMember]
        public bool Label23 { get; set; }

        [DataMember]
        public bool Label24 { get; set; }

        [DataMember]
        public bool Label25 { get; set; }
    }

    [DataContract]
    public class TaskReference
    {
        [DataMember]
        public string ReferencesId { get; set; }
        [DataMember]
        public string OdataType { get; set; }
        [DataMember]
        public string Alias { get; set; }
        [DataMember]
        public string Type { get; set; }
        [DataMember]
        public string PreviewPriority { get; set; }
        [DataMember]
        public string ReferencesLastModifiedDateTime { get; set; }
        [DataMember]
        public string ReferencesLastModifiedByUserName { get; set; }
        [DataMember]
        public string ReferencesLastModifiedByUserId { get; set; }
    }
    [DataContract]
    public class TaskCheckList
    {
        [DataMember]
        public string ChecklistId { get; set; }
        [DataMember]
        public string OdataType { get; set; }
        [DataMember]
        public bool IsChecked { get; set; }
        [DataMember]
        public string Title { get; set; }
        [DataMember]
        public string OrderHint { get; set; }
        [DataMember]
        public string CheckListLastModifiedDateTime { get; set; }
        [DataMember]
        public string CheckListLastModifiedByUserName { get; set; }
        [DataMember]
        public string CheckListLastModifiedByUserId { get; set; }

    }
}