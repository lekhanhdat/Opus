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





namespace AvePoint.GCommon.Contract.CentralAdmin.Object
{
    #region using directives
    using System.Collections.Generic;
    using System.Runtime.Serialization;
    using AvePoint.GCommon.Contract.Common;
    #endregion

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class CAListWorkflowSettingsOperation : CAOperation
    {
        [DataMember]
        public bool ContentTypesEnabled { get; set; }

        /// <summary>
        ///     All
        ///     ...
        /// </summary>
        [DataMember]
        public List<string> ContentTypes { get; set; }

        [DataMember]
        public List<CAListTasksAndHistoryListOperation> TasksAndHistoryLists { get; set; }

        [DataMember]
        public List<CAListWorkFlowTemplateOperation> WorkflowTemplates { get; set; }

        [DataMember]
        public List<CAListWorkflowOperation> Workflows { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class CAListTasksAndHistoryListOperation
    {
        [DataMember]
        public string Id { get; set; }

        [DataMember]
        public bool IsTasks { get; set; }

        [DataMember]
        public string Title { get; set; }

        [DataMember]
        public string Discription { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class CAListWorkFlowTemplateOperation
    {
        [DataMember]
        public string Description { get; set; }

        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public string Id { get; set; }

        [DataMember]
        public string BaseId { get; set; }

        [DataMember]
        public bool IsDeclarative { get; set; }

        /// <summary>
        ///     Require Manage Lists Permissions to start the workflow.
        ///     -1 : disable
        ///     1 : checked
        ///     0 : no checked
        /// </summary>
        [DataMember]
        public int ManualPermManageListRequired { get; set; }

        /// <summary>
        ///     Start this workflow to approve publishing a major version of an item. 
        /// </summary>
        [DataMember]
        public bool AllowDefaultContentApproval { get; set; }

        /// <summary>
        ///     Start this workflow when a new item is created.
        /// </summary>
        [DataMember]
        public bool AutoStartChange { get; set; }

        /// <summary>
        ///     Start this workflow when an item is changed.
        /// </summary>
        [DataMember]
        public bool AutoStartCreate { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class CAListWorkflowOperation
    {
        /// <summary>
        ///     All
        ///     Document
        ///     Folder
        /// </summary>
        [DataMember]
        public string ContentType { get; set; }

        [DataMember]
        public string Description { get; set; }

        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public string Id { get; set; }

        [DataMember]
        public string BaseId { get; set; }

        [DataMember]
        public string HistoryListId { get; set; }

        [DataMember]
        public string TaskListId { get; set; }

        [DataMember]
        public bool IsDeclarative { get; set; }

        /// <summary>
        ///     Require Manage Lists Permissions to start the workflow.
        ///     -1 : disable
        ///     1 : checked
        ///     0 : no checked
        /// </summary>
        [DataMember]
        public int ManualPermManageListRequired { get; set; }

        /// <summary>
        ///     Start this workflow to approve publishing a major version of an item. 
        /// </summary>
        [DataMember]
        public bool AllowDefaultContentApproval { get; set; }

        /// <summary>
        ///     Start this workflow when a new item is created.
        /// </summary>
        [DataMember]
        public bool AutoStartChange { get; set; }

        /// <summary>
        ///     Start this workflow when an item is changed.
        /// </summary>
        [DataMember]
        public bool AutoStartCreate { get; set; }

        /// <summary>
        ///     Specify workflows to remove from this document library. You can optionally let currently running workflows finish.   
        ///     1 : Allow
        ///     2 : No New Instances
        ///     3 : Remove 
        /// </summary>
        [DataMember]
        public int WorkFlowStatus { get; set; }

        [DataMember]
        public int Instances { get; set; }

    }
    
}
