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
    public interface IAveWorkflowManager : IDisposable
    {
        void EnableDeclarativeWorkflows(IAveSite site, bool fEnable);
        IAveWorkflowTemplateCollection GetWorkflowTemplatesByCategory(IAveWeb web, string strReqCategs);
        IAveWorkflowTemplate WorkflowTemplateFromElement(IAveWorkflowElement wfDef);

        void RemoveWorkflowFromListItem(IAveWorkflow instance);
        List<IAveWorkflow> GetItemWorkflows(IAveListItem item, Guid id);

        List<IAveWorkflow> GetItemWorkflows(IAveListItem item);
        /// <summary>
        /// Used by SharePoint 2007
        /// </summary>
        /// <param name="parentItem"></param>
        /// <param name="association"></param>
        /// <param name="bAutoStart"></param>
        /// <param name="bCreateOnly"></param>
        /// <returns></returns>
        IAveWorkflow StartWorkflow(IAveListItem parentItem, IAveWorkflowAssociation association, bool bAutoStart, bool bCreateOnly);

        IAveWorkflow StartWorkflow(object parentItem, IAveWorkflowAssociation association, string eventData, AveWorkflowRunOptions options);

        int CountWorkflows(IAveWorkflowAssociation association);

        IAveWorkflowCollection GetItemActiveWorkflows(IAveListItem item);

        void CancelWorkflow(IAveWorkflow workflow);
    }
}
