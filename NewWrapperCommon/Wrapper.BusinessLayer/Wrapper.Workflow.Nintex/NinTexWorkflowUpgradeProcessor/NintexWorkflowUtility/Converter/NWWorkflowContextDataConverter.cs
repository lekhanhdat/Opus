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
using Native13NinTexWorkflowEntity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LS.SPWorkflowProcessor
{
    static class NWWorkflowContextDataConverter
    {
        private static Dictionary<string, string> workflowContextNameMapping = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
                                {
                                    { "ContextItemUrl","CurrentItemUrl"},
                                    { "WebUrl","CurrentWebUrl"},
                                    { "Initiator","InitiatorUserId"},
                                    //{ "InitiatorsDisplayName","NW_InitiatorDisplayName"},
                                    { "WorkflowInstanceID","WorkflowInstanceId"},
                                    { "ListId","ListId"},
                                    { "ListName","ListName"},
                                    { "Manager","NW_ManagerLoginName"},
                                    { "ManagerDisplayName","NW_ManagerDisplayName"},
                                    { "WorkflowOwner","AssociatorUserId"},
                                    { "WorkflowTitle","AssociationTitle"},
                                };

        private static Dictionary<string, string> workflowContextTypeMapping = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
                                {
                                    { "ContextItemUrl","String"},
                                    { "WebUrl","String"},
                                    { "Initiator","User"},
                                    //{ "InitiatorsDisplayName","User"},
                                    { "WorkflowInstanceID","String"},
                                    { "ListId","String"},
                                    { "ListName","String"},
                                    { "Manager","User"},
                                    { "ManagerDisplayName","User"},
                                    { "WorkflowOwner","User"},
                                    { "WorkflowTitle","String"},
                                };


        public static List<string> TextBuilderModeWorkflowContextType = new List<string>()
                                {
                                    "ContextItemUrl",
                                    "CurrentItemUrl",
                                    "WebUrl",
                                    "CurrentWebUrl",
                                    "WorkflowInstanceID",
                                    "WorkflowInstanceId",
                                    "ListId",
                                    "ListName",
                                    "WorkflowTitle",
                                    "AssociationTitle"
                                };

        private static void CheckSupportData(string workflowContextDataName)
        {
            if (!workflowContextNameMapping.ContainsKey(workflowContextDataName))
            {
                throw new NotSupportedException(string.Format("Not support workflow context type {0}", workflowContextDataName));
            }
        }
        public static WorkflowContext ConvertWorkflowContextData(WorkflowContextData workflowContextData)
        {
            CheckSupportData(workflowContextData.Name);
            return new WorkflowContext
            {
                Value = workflowContextNameMapping[workflowContextData.Name],
                Type = workflowContextTypeMapping[workflowContextData.Name],
            };
        }

    }
}
