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
    class ConditionActionMappingManager : ActionMappingManagerBase
    {
        public ConditionActionMappingManager(NWListLookupCacheManager listLookupCacheManager, INintexDataMappingManager dataMappingManager, ListLookupMappingManger listLookupMappingManager)
            : base(listLookupCacheManager, dataMappingManager, listLookupMappingManager)
        {
        }

        public override void ConvertActionData(WorkflowAction workflowAction)
        {
            base.ConvertActionData(workflowAction);
            var conditionProperty = workflowAction.Configuration.Properties.FirstOrDefault(p => string.Equals(p.DesignerType, "ConditionBuilder", StringComparison.OrdinalIgnoreCase));
            if (conditionProperty != null && conditionProperty.Parameters.Length > 0)
            {
                var dictionaryValues = GetDictionaryValues(conditionProperty.Parameters[0]);
                if (dictionaryValues != null)
                {
                    var condition = GetDictionValueByKey(dictionaryValues, "condition");
                    if (condition.Value.PrimitiveValue != null && string.Equals(condition.Value.PrimitiveValue.Value.StringValue, "EqualUser", StringComparison.OrdinalIgnoreCase))
                    {
                        var right = GetDictionValueByKey(dictionaryValues, "right");
                        if (right.Value != null && right.Value.PrimitiveValue != null)
                        {
                            right.Value.PrimitiveValue.Value.StringValue = dataMappingManager.GetMappingLoginName(right.Value.PrimitiveValue.Value.StringValue);
                        }
                    }
                }
            }
        }

        private DictionaryValue[] GetDictionaryValues(Parameters parameter)
        {
            if (parameter == null)
            {
                return null;
            }

            if (parameter.Value == null)
            {
                return null;
            }

            return parameter.Value.Dictionary;
        }

        private DictionaryValue GetDictionValueByKey(DictionaryValue[] dictionaryValues, string key)
        {
            return dictionaryValues.FirstOrDefault(d => string.Equals(d.Key, key, StringComparison.OrdinalIgnoreCase));
        }
    }
}
