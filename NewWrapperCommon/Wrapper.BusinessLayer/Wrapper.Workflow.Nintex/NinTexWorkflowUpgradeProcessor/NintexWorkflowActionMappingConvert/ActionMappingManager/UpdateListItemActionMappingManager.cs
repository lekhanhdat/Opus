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
    class UpdateListItemActionMappingManager : ActionMappingManagerBase
    {
        public UpdateListItemActionMappingManager(NWListLookupCacheManager listLookupCacheManager, INintexDataMappingManager dataMappingManager, ListLookupMappingManger listLookupMappingManager)
            : base(listLookupCacheManager, dataMappingManager, listLookupMappingManager)
        { }

        public override void ConvertActionData(WorkflowAction workflowAction)
        {
            base.ConvertActionData(workflowAction);
            var listItemPropertiesParameter = FindParameterByName(workflowAction.Configuration.Properties[0].Parameters, "ListItemProperties");
            AddFieldReference(GetListIdAfterMapping(workflowAction.Configuration.Properties[0].Parameters), listItemPropertiesParameter);
        }

        protected Guid GetListIdAfterMapping(Parameters[] parameters)
        {
            var updateListItemParameter = FindParameterByName(parameters, "ListId");
            if (string.Equals(updateListItemParameter.Value.ListLookup.SelectList, "[Current Item]", StringComparison.OrdinalIgnoreCase))
            {
                return dataMappingManager.GetParentList().ID;
            }
            return new Guid(updateListItemParameter.Value.ListLookup.SelectList);
        }

        private void AddFieldReference(Guid listId, Parameters listItemPropertiesParameter)
        {
            foreach (var fieldReference in listItemPropertiesParameter.Value.Dictionary)
            {
                listLookupCacheManager.AddField(listId, fieldReference.Key);
            }
        }


    }
}
