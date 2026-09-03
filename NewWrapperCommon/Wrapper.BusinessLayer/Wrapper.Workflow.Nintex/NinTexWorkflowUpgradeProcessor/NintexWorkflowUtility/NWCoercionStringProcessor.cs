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
    static class NWCoercionStringProcessor
    {
        public static string GetCoercionString(string parametersDataType, string valueType)
        {
            if (string.IsNullOrEmpty(parametersDataType) || string.IsNullOrEmpty(valueType))
            {
                return null;
            }
            if (string.Equals(parametersDataType, valueType, StringComparison.OrdinalIgnoreCase))
            {
                return string.Format("AsDN{0}", parametersDataType); ;
            }
            if (string.Equals("Lookup", parametersDataType, StringComparison.OrdinalIgnoreCase))
            {
                return string.Format("As{0}From{1}", "DynamicValue", "String");
            }
            if (string.Equals("User", valueType, StringComparison.OrdinalIgnoreCase))
            {   //ADO-189817
                return "UserLoginNameAsText";
            }
            if (string.Equals("UserMulti", valueType, StringComparison.OrdinalIgnoreCase))
            {   //ADO-189817
                return "UserMultiLoginNameSemicolon";
            }
            return string.Format("AsDN{0}From{1}", parametersDataType, valueType);
        }

        public static string GenerateCoercionString(string leftDataType, string rightDataType, string coercionString)
        {
            if (leftDataType.Equals("User", StringComparison.OrdinalIgnoreCase)
                && rightDataType.Equals("User", StringComparison.OrdinalIgnoreCase)
                && !string.IsNullOrEmpty(coercionString))
            {
                if (coercionString.StartsWith("DisplayNameAs", StringComparison.OrdinalIgnoreCase)
                    || coercionString.StartsWith("LoginNameAs", StringComparison.OrdinalIgnoreCase)
                    || coercionString.StartsWith("EmailAddressAs", StringComparison.OrdinalIgnoreCase))
                {
                    return string.Format("User{0}", coercionString);
                }
                else
                {
                    return coercionString;
                }
            }
            else
            {
                return GetCoercionString(leftDataType, rightDataType);
            }
        }

        public static string GetCoercionString(Parameters parameters)
        {
            if (parameters == null || parameters.Value == null)
            {
                return null;
            }
            if (parameters.Value.PrimitiveValue != null)
            {
                return GetCoercionString(parameters.DataType, parameters.Value.PrimitiveValue.Type);
            }
            else if (parameters.Value.ListLookup != null)
            {
                return GetCoercionString(parameters.DataType, parameters.Value.ListLookup.SelectFieldType);
            }
            else if (parameters.Value.Variable != null)
            {
                return GetCoercionString(parameters.DataType, parameters.Value.Variable.DataType);
            }
            else if (parameters.Value.WorkflowContext != null)
            {
                return GetCoercionString(parameters.DataType, parameters.Value.WorkflowContext.Type);
            }
            return null;
        }

        public static string GetCoercionString(string dateType, Value value)
        {
            if (value.PrimitiveValue != null)
            {
                return GetCoercionString(dateType, value.PrimitiveValue.Type);
            }
            else if (value.ListLookup != null)
            {
                return GetCoercionString(dateType, value.ListLookup.SelectFieldType);
            }
            else if (value.Variable != null)
            {
                return GetCoercionString(dateType, value.Variable.DataType);
            }
            else if (value.WorkflowContext != null)
            {
                return GetCoercionString(dateType, value.WorkflowContext.Type);
            }
            return null;
        }

        public static string GetCoercionString(string dateType, ParametersValue value)
        {
            if (value.PrimitiveValue != null)
            {
                return GetCoercionString(dateType, value.PrimitiveValue.Type);
            }
            else if (value.ListLookup != null)
            {
                return GetCoercionString(dateType, value.ListLookup.SelectFieldType);
            }
            else if (value.Variable != null)
            {
                return GetCoercionString(dateType, value.Variable.DataType);
            }
            else if (value.WorkflowContext != null)
            {
                return GetCoercionString(dateType, value.WorkflowContext.Type);
            }
            return null;
        }
    }
}
