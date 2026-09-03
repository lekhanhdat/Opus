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
namespace LS.SPWorkflowProcessor
{
    class WaitForItemUpdateActionMappingManager : ActionMappingManagerBase
    {
        public WaitForItemUpdateActionMappingManager(NWListLookupCacheManager listLookupCacheManager, INintexDataMappingManager dataMappingManager, ListLookupMappingManger listLookupMappingManager)
            : base(listLookupCacheManager, dataMappingManager, listLookupMappingManager)
        { }

        public override void ConvertActionData(WorkflowAction workflowAction)
        {
            base.ConvertActionData(workflowAction);
            if (NeedUserMapping(workflowAction.Configuration.Properties[0].Parameters[0], workflowAction.Configuration.Properties[1].Parameters[0]))
            {
                var userLoginName = workflowAction.Configuration.Properties[1].Parameters[0].Value.PrimitiveValue.Value.StringValue;
                workflowAction.Configuration.Properties[1].Parameters[0].Value.PrimitiveValue.Value.StringValue = dataMappingManager.GetMappingLoginName(userLoginName);
            }
        }

        private bool NeedUserMapping(Parameters fieldNames, Parameters fieldValue)
        {
            if (fieldValue.Value.PrimitiveValue == null)
            {
                return false;
            }
            var fieldName = fieldNames.Value.PrimitiveValue.Value.StringValue;
            var parentList = dataMappingManager.GetParentList();
            var field = parentList.Fields.GetFieldByInternalName(fieldName);
            return field.Type == AvePoint.Wrapper.Common.AveFieldType.User;
        }


    }
}
