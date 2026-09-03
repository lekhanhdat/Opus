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
    class QueryListActionMappingManager : ActionMappingManagerBase
    {
        private ParametersValue oldTargetListIdValue;
        private ParametersValue newTargetListIdValue;
        private string convertedFieldsStringValue;

        public QueryListActionMappingManager(NWListLookupCacheManager listLookupCacheManager, INintexDataMappingManager dataMappingManager, ListLookupMappingManger listLookupMappingManager)
            : base(listLookupCacheManager, dataMappingManager, listLookupMappingManager)
        {
        }

        public override void ConvertActionData(WorkflowAction workflowAction)
        {
            oldTargetListIdValue = FindParameterByName(workflowAction.Configuration.Properties[0].Parameters, "TargetListId").Value;
            base.ConvertActionData(workflowAction);
            newTargetListIdValue = FindParameterByName(workflowAction.Configuration.Properties[0].Parameters, "TargetListId").Value;
            ConvertODataSelect(workflowAction);
            ConvertODataExpand(workflowAction);
            ConvertParameterVariableMap(workflowAction);
        }

        private string ConvertFieldName(string srcListIdOrTitle, string fieldName)
        {
            if (string.IsNullOrEmpty(fieldName))
            {
                return fieldName;
            }

            var fieldInListReferences = base.dataMappingManager.GetFieldFromListReferences(srcListIdOrTitle, fieldName);
            string fieldStr = fieldName;
            if (fieldInListReferences != null)
            {
                if (fieldInListReferences.FieldType == AvePoint.Wrapper.Common.AveFieldType.User)
                {
                    fieldStr = string.Format("{0}/{1}", fieldStr, "EMail");
                }
            }
            else
            {
                var list = base.dataMappingManager.GetParentWeb().GetList(new Guid(newTargetListIdValue.ListLookup.SelectList));
                if (list != null)
                {
                    var field = list.Fields.TryGetFieldByStaticName(fieldStr);
                    if (field != null 
                        && (field.TypeAsString.Equals("User", StringComparison.OrdinalIgnoreCase) || field.TypeAsString.Equals("UserMulti", StringComparison.OrdinalIgnoreCase)))
                    {
                        fieldStr = string.Format("{0}/{1}", fieldStr, "EMail");
                    }
                }
            }

            return fieldStr;
        }

        private void ConvertODataSelect(WorkflowAction workflowAction)
        {
            var oDataSelectParameter = FindParameterByName(workflowAction.Configuration.Properties[0].Parameters, "ODataSelect");
            var oldODataSelectStringValueList = oDataSelectParameter.Value.PrimitiveValue.Value.StringValue.Split(',').ToList();
            foreach (string f in oldODataSelectStringValueList)
            {
                var fieldName = ConvertFieldName(oldTargetListIdValue.ListLookup.SelectList, f);

                convertedFieldsStringValue = string.IsNullOrEmpty(convertedFieldsStringValue) ? fieldName : convertedFieldsStringValue + "," + fieldName;
            }
            oDataSelectParameter.Value.PrimitiveValue.Value.StringValue = convertedFieldsStringValue;
        }

        private void ConvertODataExpand(WorkflowAction workflowAction)
        {
            var oDataExpandParameter = FindParameterByName(workflowAction.Configuration.Properties[0].Parameters, "ODataExpand");
            if (!string.IsNullOrEmpty(oDataExpandParameter.Value.PrimitiveValue.Value.StringValue))
            {
                oDataExpandParameter.Value.PrimitiveValue.Value.StringValue = convertedFieldsStringValue;
            }
        }

        private void ConvertParameterVariableMap(WorkflowAction workflowAction)
        {
            var parameterVariableMapParameter = FindParameterByName(workflowAction.Configuration.Properties[0].Parameters, "ParameterVariableMap");
            var oldParameterVariableMapDictionary = parameterVariableMapParameter.Value.Dictionary.ToList();
            var fieldsList = convertedFieldsStringValue.Split(',').ToList();
            foreach (DictionaryValue dv in oldParameterVariableMapDictionary)
            {
                if (fieldsList.Contains(string.Format("{0}/EMail", dv.Key)))
                {
                    dv.Key = string.Format("{0}/EMail", dv.Key);
                }
            }
        }
    }
}
