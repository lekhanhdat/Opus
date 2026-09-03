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
    class SetFieldActionMappingManager : ActionMappingManagerBase
    {
        public SetFieldActionMappingManager(NWListLookupCacheManager listLookupCacheManager, INintexDataMappingManager dataMappingManager, ListLookupMappingManger listLookupMappingManager)
            : base(listLookupCacheManager, dataMappingManager, listLookupMappingManager)
        { }

        public override void ConvertActionData(WorkflowAction workflowAction)
        {
            base.ConvertActionData(workflowAction);
            RemoveFieldNameParameterNoUseData(FindParameterByName(workflowAction.Configuration.Properties[0].Parameters, "FieldName"));
        }

        private void RemoveFieldNameParameterNoUseData(Parameters fieldNameParameter)
        {
            if (fieldNameParameter.Value != null && fieldNameParameter.Value.ListLookup != null)
            {
                //该值需要设置为null,否则publish 会失败
                fieldNameParameter.Value.ListLookup.SelectFieldType = null;
                //该值不置为null 也不会失败，但是online的数据都是 null，因此该属性也被置为了null
                fieldNameParameter.Value.ListLookup.DisplayValue = null;
            }
        }
    }
}
