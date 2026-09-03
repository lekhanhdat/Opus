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
    class VariablesProcessorBase
    {
        private Dictionary<WorkflowInitiationControlType, string> VariablesTypeMapping = new Dictionary<WorkflowInitiationControlType, string>()
        {
            {WorkflowInitiationControlType.SingleLine,"String"},
            {WorkflowInitiationControlType.MultipleLine,"String"},
            {WorkflowInitiationControlType.ChoiceDropDown,"Choice"},
            {WorkflowInitiationControlType.ChoiceList,"Choice"},
            {WorkflowInitiationControlType.ChoiceRadioButtons,"Choice"},
            {WorkflowInitiationControlType.ArrayList,"Array"},
            {WorkflowInitiationControlType.User,"PersonGroup"},
            {WorkflowInitiationControlType.SPItemKey,"Integer"},
            {WorkflowInitiationControlType.Long,"Integer"},
            {WorkflowInitiationControlType.Number,"Number"},

        };

        private Dictionary<WorkflowInitiationControlType, string> VariablesDataTypeMapping = new Dictionary<WorkflowInitiationControlType, string>()
        {
            {WorkflowInitiationControlType.SingleLine,"String"},
            {WorkflowInitiationControlType.MultipleLine,"String"},
            {WorkflowInitiationControlType.Number,"Double"},
            {WorkflowInitiationControlType.Long,"Int32"},
            {WorkflowInitiationControlType.Integer,"Int32"},
            {WorkflowInitiationControlType.SPItemKey,"Int32"},
            {WorkflowInitiationControlType.ChoiceDropDown,"String"},
            {WorkflowInitiationControlType.ChoiceList,"String"},
            {WorkflowInitiationControlType.ChoiceRadioButtons,"String"},
            {WorkflowInitiationControlType.ArrayList,"DynamicValue"},
            {WorkflowInitiationControlType.User,"User"},
        };

        protected NWWorkflowVariable currentVar;

        public VariablesProcessorBase(NWWorkflowVariable currentVar)
        {
            this.currentVar = currentVar;
        }

        public  Variable GetUpgradedVariable()
        {
            var variable = new Variable()
            {
                Name = currentVar.Name,
                Type = VariablesTypeMapping.ContainsKey(currentVar.ControlType)
                        ? VariablesTypeMapping[currentVar.ControlType] : currentVar.Type,
                DataType = VariablesDataTypeMapping.ContainsKey(currentVar.ControlType)
                            ? VariablesDataTypeMapping[currentVar.ControlType] : currentVar.Type,

                Identifier = Guid.NewGuid().ToString(),
            };
            variable.VariableConfiguration = GetVariableConfiguration();
            variable.Initiate = GetInitiate();
            return variable;
        }

        protected virtual bool GetInitiate()
        {
            return currentVar.Initiate;
        }

        protected virtual VariableConfiguration GetVariableConfiguration()
        {
            var configuration = new VariableConfiguration
            {
                DefaultValue = currentVar.DefaultValue,
                Description = currentVar.Description,
            };
            return configuration;
        }
    }
}
