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
using System.Text;

namespace LS.SPWorkflowProcessor
{
    class NWActionMappingManagerAdapter
    {
        private Dictionary<String, ActionMappingManagerBase> adapter;
        private ActionMappingManagerBase defaultMappingManager;
        public NWActionMappingManagerAdapter(NWListLookupCacheManager listLookupCacheManager, INintexDataMappingManager mappingManager, ListLookupMappingManger listLookupMappingManager)
        {
            adapter = new Dictionary<string, ActionMappingManagerBase>(StringComparer.OrdinalIgnoreCase)
                     {
                         {"#SendNotificationActivity" , new SendEmailActionMappingManager(listLookupCacheManager, mappingManager, listLookupMappingManager)},
                         {"Microsoft.SharePoint.WorkflowServices.Activities.CreateListItem" , new CreateListItemActionMappingManager(listLookupCacheManager, mappingManager, listLookupMappingManager)},
                         {"Microsoft.SharePoint.WorkflowServices.Activities.UpdateListItem" , new UpdateListItemActionMappingManager(listLookupCacheManager, mappingManager, listLookupMappingManager)},
                         {"Microsoft.SharePoint.WorkflowServices.Activities.CopyItem" , new CopyItemActionMappingManager(listLookupCacheManager, mappingManager, listLookupMappingManager)},
                         {"Office365UpdateItemPermissions" , new SetItemPermissionsActionMappingManager(listLookupCacheManager, mappingManager, listLookupMappingManager)},
                         {"Office365QueryUserProfile" , new QueryUserProfileActionMappingmanager(listLookupCacheManager, mappingManager, listLookupMappingManager)},
                         {"#Filter" , new ConditionActionMappingManager(listLookupCacheManager, mappingManager, listLookupMappingManager)},
                         {"#ConditionalBranch" , new ConditionActionMappingManager(listLookupCacheManager, mappingManager, listLookupMappingManager)},
                         {"#LoopCondition" , new ConditionActionMappingManager(listLookupCacheManager, mappingManager, listLookupMappingManager)},
                         {"#RunIf" , new ConditionActionMappingManager(listLookupCacheManager, mappingManager, listLookupMappingManager)},
                         {"Microsoft.SharePoint.WorkflowServices.Activities.SetField" , new SetFieldActionMappingManager(listLookupCacheManager, mappingManager, listLookupMappingManager)},
                         {"Microsoft.SharePoint.WorkflowServices.Activities.WaitForFieldChange" , new WaitForItemUpdateActionMappingManager(listLookupCacheManager, mappingManager, listLookupMappingManager)},
                         {"#QuerySPList", new QueryListActionMappingManager(listLookupCacheManager, mappingManager, listLookupMappingManager) }
                      };

            defaultMappingManager = new ActionMappingManagerBase(listLookupCacheManager, mappingManager, listLookupMappingManager);
        }

        public ActionMappingManagerBase GetMappingManager(string className)
        {
            if (adapter.ContainsKey(className))
            {
                return adapter[className];
            }
            return defaultMappingManager;
        }

    }
}
