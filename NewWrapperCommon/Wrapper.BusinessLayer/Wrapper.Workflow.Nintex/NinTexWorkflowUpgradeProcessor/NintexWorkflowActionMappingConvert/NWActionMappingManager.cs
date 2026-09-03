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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LS.SPWorkflowProcessor
{
    public class NWActionMappingManager
    {
        private static AveLogger logger = AveLogger.GetInstance(typeof(NWActionMappingManager));
        private NWListLookupCacheManager listLookupCacheManager;
        private NWActionMappingManagerAdapter mappingManagerAdapter;
        private INintexDataMappingManager mappingManager;
        private List<string> notFoundLists = new List<string>();
        public NWActionMappingManager(INintexDataMappingManager mappingManager, string taskListId, string historyListId, bool isListLevel)
        {
            if (mappingManager == null)
            {
                throw new ArgumentNullException("mappingManager");
            }
            this.mappingManager = mappingManager;
            listLookupCacheManager = new NWListLookupCacheManager(isListLevel ? mappingManager.GetParentList().ID : Guid.Empty);
            ListLookupMappingManger listLookupMappingManager = new ListLookupMappingManger(listLookupCacheManager, mappingManager, taskListId, historyListId);
            mappingManagerAdapter = new NWActionMappingManagerAdapter(listLookupCacheManager, mappingManager, listLookupMappingManager);
        }


        private void MappingData(WorkflowAction workflowAction)
        {
            try
            {
                var mappingManager = mappingManagerAdapter.GetMappingManager(GetMappingKey(workflowAction));
                mappingManager.ConvertActionData(workflowAction);
            }
            catch (NWListNotFoundException e)
            {
                if (!notFoundLists.Contains(e.ListId))
                {
                    notFoundLists.Add(e.ListId);
                }
            }
        }
        private string GetMappingKey(WorkflowAction workflowAction)
        {
            if (!string.Equals("#NintexLive", workflowAction.ClassName, StringComparison.OrdinalIgnoreCase))
            {
                return workflowAction.ClassName;
            }
            return workflowAction.Configuration.Live.ProductId;
        }

        private void MappingDataInWorkflowAction(WorkflowAction rootWorkflowAction)
        {
            MappingData(rootWorkflowAction);
            foreach (var childWorkflowAction in rootWorkflowAction.Children)
            {
                MappingDataInWorkflowAction(childWorkflowAction);
            }
            if (rootWorkflowAction.Next != null)
            {
                MappingDataInWorkflowAction(rootWorkflowAction.Next);
            }

        }

        private void RecordNotFoundList()
        {
            StringBuilder stringBuilder = new StringBuilder("Not found lists:");
            foreach (var listId in notFoundLists)
            {
                stringBuilder.AppendLine(listId);
                var listName = mappingManager.GetListTitleFromListReferences(listId);
                stringBuilder.AppendLine(string.Format("list Name: {0}, List Id: {1}", listName, listId));
            }
            logger.Warn("An error occurred while mapping data, error messasge: {0}.", stringBuilder);
        }
        public NWListLookupCacheManager MappingWorkflowActionData(WorkflowAction rootWorkflowAction,bool isPostAction)
        {
            MappingDataInWorkflowAction(rootWorkflowAction);
            if (notFoundLists.Count != 0)
            {
                //在Post Action中打出没有找到的List.
                if (isPostAction)
                {
                    RecordNotFoundList();
                }
                throw new NWListNotFoundException("");
            }
            return listLookupCacheManager;
        }

    }
}
