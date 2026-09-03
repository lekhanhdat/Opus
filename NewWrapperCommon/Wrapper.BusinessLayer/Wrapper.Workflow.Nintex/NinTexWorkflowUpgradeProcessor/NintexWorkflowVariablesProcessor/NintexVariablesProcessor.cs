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
using AvePoint.Wrapper.Common;
using Native13NinTexWorkflowEntity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LS.SPWorkflowProcessor
{
    class NintexVariablesProcessor
    {
        private IAveWeb parentWeb;
        public NintexVariablesProcessor(IAveWeb parentWeb)
        {
            this.parentWeb = parentWeb;
        }

        private VariablesProcessorBase GetVariablesProcessor(NWWorkflowVariable variable)
        {
            switch (variable.ControlType)
            {
                case WorkflowInitiationControlType.SingleLine:
                case WorkflowInitiationControlType.MultipleLine:
                    return new StringVariablesProcessor(variable);
                case WorkflowInitiationControlType.YesNo:
                    return new BooleanVariablesProcessor(variable);
                case WorkflowInitiationControlType.ChoiceDropDown:
                case WorkflowInitiationControlType.ChoiceList:
                case WorkflowInitiationControlType.ChoiceRadioButtons:
                    return new ChoiceVariablesProcessor(variable);
                case WorkflowInitiationControlType.DateOnly:
                case WorkflowInitiationControlType.DateTime:
                    return new DatetimeVariablesProcessor(variable);
                case WorkflowInitiationControlType.Integer:
                case WorkflowInitiationControlType.SPItemKey:
                case WorkflowInitiationControlType.Number:
                case WorkflowInitiationControlType.Long://对应On-premise的ActionID,当前处理逻辑是将ActionID mapping到目的端的Int类型
                    return new IntegerVariablesProcessor(variable);
                case WorkflowInitiationControlType.ArrayList:
                    return new CollectionVariableProcessor(variable);
                case WorkflowInitiationControlType.User:
                    return new UserVariableProcessor(variable,parentWeb);
                default:
                    return new VariablesProcessorBase(variable);
            }
        }

        public ArrayOfVariable GetArrayOfVariable(NWWorkflowVariable[] variables)
        {
            ArrayOfVariable arrayOfVariable = new ArrayOfVariable();
            List<Variable> variableList = new List<Variable>();
            foreach (var variable in variables)
            {
                var variableProcessor = GetVariablesProcessor(variable);
                if (variableProcessor != null)
                {
                    var upgradedVariable = variableProcessor.GetUpgradedVariable();
                    variableList.Add(upgradedVariable);
                }
            }
            arrayOfVariable.Variable = variableList.ToArray();
            return arrayOfVariable;
        }
    }
}
