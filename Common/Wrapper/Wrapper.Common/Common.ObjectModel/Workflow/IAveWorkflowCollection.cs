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
using System.Collections;
using System.Collections.ObjectModel;

namespace AvePoint.Wrapper.Common
{
    public interface IAveWorkflowCollection : ICollection, IEnumerable<IAveWorkflow>, IEnumerable
    {
       
        // Summary:
        //     Gets the total number of workflow instances in the collection.
        //
        // Returns:
        //     An Integer that represents the total number of workflow instances.
        int Count { get; }

        //
        // Summary:
        //     Returns a string that represents the workflow instance collection in XML
        //     format.
        //
        // Returns:
        //     A String that represents the workflow instance collection in XML format.
        string Xml { get; }

        // Summary:
        //     Gets the specified workflow instance.
        //
        // Parameters:
        //   instanceId:
        //     The ID of the workflow instance.
        //
        // Returns:
        //     An Microsoft.SharePoint.Workflow.SPWorkflow object that represents the workflow
        //     instance.
        IAveWorkflow this[Guid instanceId] { get; }
        //
        // Summary:
        //     Gets the specified workflow instance.
        //
        // Parameters:
        //   index:
        //     The index of the workflow instance in the collection.
        //
        // Returns:
        //     An Microsoft.SharePoint.Workflow.SPWorkflow object that represents the workflow
        //     instance.
        IAveWorkflow this[int index] { get; }

        //
        // Summary:
        //     Gets a collection of the GUIDs for the workflow instances in the collection.
        //
        // Returns:
        //     A collection of the GUIDs that represent the workflow IDs of the workflow
        //     instances in the collection.
        Collection<Guid> GetInstanceIds();
    }
}
