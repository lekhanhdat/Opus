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
using AvePoint.Wrapper.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LS.SPWorkflowProcessor
{
    class CopyItemActionMappingManager : ActionMappingManagerBase
    {
        AveLogger logger = AveLogger.GetInstance(typeof(CopyItemActionMappingManager));
        public CopyItemActionMappingManager(NWListLookupCacheManager listLookupCacheManager, INintexDataMappingManager dataMappingManager, ListLookupMappingManger listLookupMappingManager)
            : base(listLookupCacheManager, dataMappingManager, listLookupMappingManager)
        { }

        public override void ConvertActionData(WorkflowAction workflowAction)
        {
            base.ConvertActionData(workflowAction);
            var sourceListIdParameter = FindParameterByName(workflowAction.Configuration.Properties[0].Parameters, "ListId");
            var destinationListIdParameter = FindParameterByName(workflowAction.Configuration.Properties[1].Parameters, "ToListId");
            // 由于Online 对应的Action仅支持Document,如果关联的是一个list的话，publish同样可以成功，
            // 但是目的端不可用，因此增加该逻辑避免上述情况
            if (!IsSupportConfiguration(sourceListIdParameter, destinationListIdParameter))
            {
                throw new UnSupportedActionTypeException("source copy Item action is associated with list, destination action does not support this setting.");
            }

        }


        private bool IsDocumentLibrary(Guid listId)
        {
            try
            {
                return base.dataMappingManager.GetParentWeb().GetList(listId) is IAveDocumentLibrary;
            }
            catch (Exception e)
            {
                logger.Warn("An error occurred while get list by id. List id: {0}, error: {1}", listId, e);
            }
            return false;
        }

        private bool IsSupportConfiguration(Parameters sourceListIdParameter, Parameters destinationListIdParameter)
        {
            if (string.Equals(sourceListIdParameter.Value.ListLookup.SelectList, ListLookupMappingManger.CURRENTLIST, StringComparison.OrdinalIgnoreCase))
            {
                //Current Item && List
                if (base.dataMappingManager.GetParentList().BaseTemplate != AvePoint.Wrapper.Common.AveListTemplateType.DocumentLibrary)
                {
                    return false;
                }
            }
            else if(!IsDocumentLibrary(new Guid(sourceListIdParameter.Value.ListLookup.SelectList)))
            {
                return false;
            }
            //Destination is List
            if (!IsDocumentLibrary(new Guid(destinationListIdParameter.Value.ListLookup.SelectList)))
            {
                return false;
            }
            return true;
        }
    }
}
