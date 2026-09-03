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
using Native13NinTexWorkflowEntity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LS.SPWorkflowProcessor
{
    static class NWValueStorageConverter
    {
        private static Dictionary<string, string> valueTypeMappings = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
                                {
                                    {"text","String"},
                                    {"Integer","Int32"},
                                    {"Counter","Int32"},
                                    {"choice","String"},
                                    {"Number","Double"},
                                    {"File","String"},
                                    {"Computed","String"},
                                };

        public static DictionaryValue[] ConvertToDictionaryValueArray(ValueStorageCollection valueStorages)
        {
            var dictionaryValues = new List<DictionaryValue>();
            foreach (var valueStorage in valueStorages)
            {
                dictionaryValues.Add(new DictionaryValue
                {
                    Key = valueStorage.ValueIdentifier,
                    Value = new Value(new Variable
                    {
                        Name = valueStorage.VariableName,
                        DataType = valueTypeMappings[valueStorage.VariableType]
                    })
                });
            }

            return dictionaryValues.ToArray();
        }

        public static ParametersValue ConvertToParametersValue(ValueStorage valueStorage)
        {
            return new ParametersValue
            {
                Variable = new Variable
                {
                    Name = valueStorage.VariableName,
                    DataType = valueTypeMappings[valueStorage.VariableType]
                }
            };
        }

        public static ParametersValue ConvertToParametersValueFromActivityParameter(ActivityParameter para)
        {
            var parametersValue = new ParametersValue();
            if (para.Variable != null)
            {
                parametersValue.Variable = new Variable
                {
                    Name = para.Variable.Name,
                    DataType = valueTypeMappings[para.Variable.Type]
                };
            }
            return parametersValue;
        }
    }
}
