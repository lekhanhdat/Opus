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
using AvePoint.Wrapper.Common;
using Native13NinTexWorkflowEntity;

namespace LS.SPWorkflowProcessor
{
    class NWVariablesCacheManager
    {
        private static Dictionary<string, Variable> variableCache = new Dictionary<string, Variable>(StringComparer.OrdinalIgnoreCase);

        private List<string> textBuilderModeDataType = new List<string>()
                                {
                                    "text",
                                    "choice",
                                    "File",
                                    "Computed",
                                    "String",
                                };

        public NWVariablesCacheManager(Variable[] variables)
        {
            if (variables != null)
            {
                AddVariableCache(variables);
            }
        }

        public List<string> TextBuilderModeDataType
        {
            get
            {
                return textBuilderModeDataType;
            }
        }

        private void AddVariableCache(Variable[] variables)
        {
            foreach (var variable in variables)
            {
                variableCache[variable.Name] = variable;
            }
        }

        public Variable GetVariable(string variableName, bool throwException)
        {
            Variable variable = null;
            if (!variableCache.TryGetValue(variableName, out variable) && throwException)
            {
                throw new AveWrapperBaseException(string.Format("Cannot find variable, variable name is {0}", variableName));
            }
            return variable;
        }

        public Variable GetSimpleVariable(NWWorkflowVariable nWWorkflowVariable)
        {
            return GetSimpleVariable(nWWorkflowVariable.Name);
        }

        /// <summary>
        /// Only get variable Name and DataType properties
        /// </summary>
        /// <param name="nWWorkflowVariable"></param>
        /// <returns></returns>
        public Variable GetSimpleVariable(string variableName)
        {
            Variable tempVariable = this.GetVariable(variableName, true);
            return new Variable { Name = tempVariable.Name, DataType = tempVariable.DataType };
        }
    }
}