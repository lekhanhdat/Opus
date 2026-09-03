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
using System.Text;

namespace AvePoint.Wrapper.Common
{

    [Serializable]
    public class AveWorkflowInfo
    {
        public string ColumnName = string.Empty;
        public string TableName = string.Empty;
        public byte[] AssociationUnit;
        public string CTId = string.Empty;
        public string CTName = string.Empty;
        public Guid OrigAssoId;
        public Guid OrigBaseId;
        public string OrigAssoName;
    }
    public enum AveWorkflowState
    {
        // Summary:
        //     Include or exclude no workflows or workflow tasks from the collection.
        None = 0,
        //
        // Summary:
        //     Include or exclude all locked workflows or workflow tasks from the collection.
        Locked = 1,
        //
        // Summary:
        //     Include or exclude all workflows or workflow tasks that are currently running
        //     from the collection.
        Running = 2,
        //
        // Summary:
        //     Include or exclude all completed workflows or workflow tasks from the collection.
        Completed = 4,
        //
        // Summary:
        //     Include or exclude all cancelled workflows or workflow tasks from the collection.
        Cancelled = 8,
        //
        // Summary:
        //     Include or exclude all workflows or workflow tasks that are currently expiring
        //     from the collection.
        Expiring = 16,
        //
        // Summary:
        //     Include or exclude all expired workflows or workflow tasks from the collection.
        Expired = 32,
        //
        // Summary:
        //     Include or exclude all workflows or workflow tasks that are currently faulting
        //     from the collection.
        Faulting = 64,
        //
        // Summary:
        //     Include or exclude all workflows or workflow tasks that have been terminated
        //     from the collection.
        Terminated = 128,
        //
        // Summary:
        //     Include or exclude all workflows or workflow tasks that have been suspended
        //     from the collection.
        Suspended = 256,
        //
        // Summary:
        //     Include or exclude all workflows or workflow tasks that have been orphaned
        //     from the collection.
        Orphaned = 512,
        //
        // Summary:
        //     Include or exclude all workflows or workflow tasks that have new workflow
        //     events from the collection.
        HasNewEvents = 1024,
        //
        // Summary:
        //     Include or exclude all workflows or workflow tasks that have not yet started
        //     from the collection.
        NotStarted = 2048,
        //
        // Summary:
        //     Include or exclude all workflows or workflow tasks from the collection.
        All = 4095,
    }
    // Summary:
    //     Specifies the type of object that is hosting the event.
    public enum AveEventHostType
    {
        // Summary:
        //     Not used.
        Invalid = -1,
        //
        // Summary:
        //     Specifies site collection.
        Site = 0,
        //
        // Summary:
        //     Specifies site.
        Web = 1,
        //
        // Summary:
        //     Specifies list.
        List = 2,
        //
        // Summary:
        //     Specifies list item.
        ListItem = 3,
        //
        // Summary:
        //     Specifies content type.
        ContentType = 4,
        //
        // Summary:
        //     Specifies workflow.
        Workflow = 5,
        //
        // Summary:
        //     Specifies Feature.
        Feature = 6,
        WorkflowList = 7,
    }

    public enum AveWorkflowRunOptions
    {
        Asynchronous = 6,
        Synchronous = 1,
        SynchronousAllowPostpone = 3
    }

    public enum AveWorkflowStatus13Model
    {
        NotStarted,
        Started,
        Suspended,
        Canceling,
        Canceled,
        Terminated,
        Completed,
        NotSpecified,
        Invalid
    }

    public class AveWorkflowAssociationInfo
    {
        public AveWorkflowModel WorkFlowModle { get; set; }
        public Guid AssociationId { get; set; }
        public Guid AssociationBaseId { get; set; }
        public Guid DefinitionId { get; set; }
        public Guid SubScriptionId { get; set; }
        public string CTName { get; set; }
        public string Name { get; set; }
        public bool IsCTWorkflowAssociation { get; set; }
    }

    public class AveReusableWorkflowTemplateInfo
    {
        public bool AllowDefaultContentApproval { get; set; }
        public bool AutoStartChange { get; set; }
        public bool AutoStartCreate { get; set; }
        public Guid BaseId { get; set; }
        public string Description { get; set; }
        public Guid ID { get; set; }
        public bool IsRootPublic { get; set; }
        public string Name { get; set; }
    }

    public enum AveWorkflowModel
    {
        Model2010,
        Model2013,
        ModelNintex
    }

}