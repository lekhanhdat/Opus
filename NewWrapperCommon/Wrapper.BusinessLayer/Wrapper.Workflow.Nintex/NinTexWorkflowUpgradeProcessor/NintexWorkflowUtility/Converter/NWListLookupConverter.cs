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
using AvePoint.GCommon.Utility;
using AvePoint.Wrapper.Common;
using Native13NinTexWorkflowEntity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LS.SPWorkflowProcessor
{
    static class NWListLookupConverter
    {
        public static List<string> TextBuilderModeFieldType = new List<string>()
                                {
                                    "String",
                                    "Text",
                                    "Note",
                                    "Choice",
                                    "Computed",
                                    "Guid",
                                    "Calculated",
                                    "File",
                                    "ContentTypeId"
                                };


        public static ListLookup ConvertListLookup(ValueLookup valueLookup, NintexWFActionProcessor workflowActionProcessor)
        {
            ListLookup listLookup = null;
            if (valueLookup.LookupType == SLLookupType.ThisItemLookup || valueLookup.LookupType == SLLookupType.ThisItemLookupTopLevel)
            {
                listLookup = new ListLookup
                {
                    SelectList = "[Current Item]",
                    DisplayName = "Current Item",
                    SelectField = valueLookup.Field == null ? string.Empty : valueLookup.Field.Name,
                    SelectFieldType = valueLookup.Field == null || valueLookup.Field.Type == null ? string.Empty : NWFieldTypeMapping.ConvertFieldType(valueLookup.Field.Type),
                    //以下两个属性需要赋值为string.Empty,否则publish 有可能失败
                    WhereField = string.Empty,
                    WhereFieldType = string.Empty,
                };
            }
            else
            {
                listLookup = new ListLookup
                {
                    SelectList = valueLookup.ListId,
                    SelectField = valueLookup.Field == null ? string.Empty : valueLookup.Field.Name,
                    SelectFieldType = valueLookup.Field == null ? string.Empty : NWFieldTypeMapping.ConvertFieldType(valueLookup.Field.Type),
                    WhereField = valueLookup.CompareField == null ? string.Empty : valueLookup.CompareField.Name,
                    WhereFieldType = valueLookup.CompareField == null ? string.Empty : NWFieldTypeMapping.ConvertFieldType(valueLookup.CompareField.Type),
                };
                if (valueLookup.Coercion != null)
                {
                    listLookup.Coercion = valueLookup.Coercion.Value;
                }
            }
            listLookup.WhereValue = ConvertToListWhereValue(workflowActionProcessor, valueLookup);
            return listLookup;
        }

        private static Value ConvertToListWhereValue(NintexWFActionProcessor workflowActionProcessor, ValueLookup listLookup)
        {
            if (listLookup.PrimitiveValue != null && !string.IsNullOrEmpty(listLookup.PrimitiveValue.Value))
            {
                return  NWValueConverter.ConvertPrimitiveValueToValue(workflowActionProcessor, listLookup.PrimitiveValue);
            }
            else
            {
                return NWValueConverter.ConvertValue(workflowActionProcessor, listLookup);
            }
        }
    }
}
