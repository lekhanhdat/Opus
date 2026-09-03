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
    class NWWaitForItemUpdateActionProcessor : NWActionProcessorBase
    {
        public NWWaitForItemUpdateActionProcessor(NintexWFActionProcessor workflowActionProcessor)
            : base(workflowActionProcessor)
        {
            CLASSNAME = "Microsoft.SharePoint.WorkflowServices.Activities.WaitForFieldChange";
        }

        public override WorkflowAction UpgradeWorkflowAction(NWActionConfig nwActionConfig)
        {
            if (this.workflowActionProcessor.IsWebLevel)
            {
                throw new NotSupportedException("Wait for item update only suppport in list level.");
            }

            sourceConfig = nwActionConfig;
            return new WorkflowAction()
            {
                Id = actionId,
                ClassName = CLASSNAME,
                Configuration = new Configuration()
                {
                    Id = Guid.NewGuid().ToString(),
                    Name = sourceConfig.TLabel,
                    Image = CreateImage(),
                    ServerInfo = new ServerInfo { ClassName = CLASSNAME },
                    Properties = CreateProperties(),
                    HelpKey = CLASSNAME
                },

            };
        }

        protected override Image CreateImage()
        {
            return new Image
            {
                Src = "ext/workflowDesigner/Img/icons/NW2013_Action_IconMap.png?noCache=1469583696475",
                ClassName = CLASSNAME,
                x49x49 = 392,
                y49x49 = 79,
                x30x30 = 392,
                y30x30 = 128,
                x16x16 = 229,
                y16x16 = 286
            };
        }

        private void CheckFieldExist(ActivityParameter lookupFieldValue)
        {
            try
            {
                if (lookupFieldValue.PrimitiveValue != null && !string.IsNullOrEmpty(lookupFieldValue.PrimitiveValue.Value))
                {
                    var parentList = workflowActionProcessor.DataMappingManager.GetParentList();
                    if (parentList != null)
                    {
                        var field = parentList.Fields.GetFieldByInternalName(lookupFieldValue.PrimitiveValue.Value);
                    }
                }
            }
            catch (Exception e)
            {
                throw new NWNeedPostActionException(string.Format("Field not exist in destination, field name: {0}", lookupFieldValue.PrimitiveValue.Value), e);
            }
        }
        protected override List<Property> CreateProperties()
        {
            ActivityParameter lookupField = null;
            ActivityParameter lookupFieldType = null;
            ActivityParameter lookupFieldValue = null;
            ActivityParameter operatorPar = null;

            foreach (var para in sourceConfig.Parameters)
            {
                if (string.Equals(para.Name, "LookupField", StringComparison.OrdinalIgnoreCase))
                {
                    lookupField = para;
                }
                else if (string.Equals(para.Name, "LookupFieldType", StringComparison.OrdinalIgnoreCase))
                {
                    lookupFieldType = para;
                }
                else if (string.Equals(para.Name, "LookupFieldValue", StringComparison.OrdinalIgnoreCase))
                {
                    lookupFieldValue = para;
                }
                else if (string.Equals(para.Name, "Operator", StringComparison.OrdinalIgnoreCase))
                {
                    operatorPar = para;
                }
            }

            CheckUnsupportedActionType(lookupFieldValue);
            CheckUnsupportedOperator(CLASSNAME, operatorPar);
            CheckFieldExist(lookupField);
            var p0 = new Property
            {
                ID = "p0",
                DesignerType = "FieldNames",
                DisplayName = "Field",
                Parameters = new[]
                {
                    new Parameters() {
                        Name = "FieldName",
                        Value = new ParametersValue() {
                            PrimitiveValue = new PrimitiveValue() {
                                Type = "String",
                                Value = new Value(lookupField.PrimitiveValue.Value)
                            }
                        },
                        Description = "Field to use in comparison.",
                        Required = true,
                        DataType = "String",
                        DesignerType = "FieldNames",
                        Direction = "Input"
                    }
                }
            };

            var p1 = new Property
            {
                ID = "p1",
                DesignerType = "Dependent",
                DisplayName = "Value",
                Parameters = new[]
                {
                    new Parameters()
                    {
                        Name = "FieldValue",
                        Value = new ParametersValue(),
                        Description = "Value to use in comparison. When the field equals this value, the action will complete.",
                        Required = true,
                        DataType = (lookupFieldValue.ListLookup != null || lookupFieldValue.Variable != null) ? "String" : "Any",
                        DesignerType = (lookupFieldValue.ListLookup != null || lookupFieldValue.Variable != null) ? lookupFieldType.PrimitiveValue.ValueType : null,
                        Direction = "Input",
                        DependentOn = "FieldName"
                    }
                }
            };
            p1.Parameters[0].Value = ConvertParameterValue(lookupFieldValue);

            return new List<Property> { p0, p1 };
        }

        protected override ParametersValue ConvertParameterValue(ActivityParameter activityParameter)
        {
            var parameterValue = new ParametersValue { };
            if (activityParameter.ListLookup != null)
            {
                parameterValue.PrimitiveValue = new PrimitiveValue("String", "{0}");
                parameterValue.PrimitiveValue.FormatValues = new List<FormatValues>()
                {
                    new FormatValues
                    {
                        SelectedValue = new SelectedValue
                        {
                            ListLookup = NWListLookupConverter.ConvertListLookup(activityParameter.ListLookup, this.workflowActionProcessor)
                        }
                    }
                };
                if (activityParameter.Coercion != null)
                {
                    parameterValue.PrimitiveValue.FormatValues[0].SelectedValue.Coercion = activityParameter.Coercion.Value;
                }
            }

            if (activityParameter.PrimitiveValue != null)
            {
                parameterValue.PrimitiveValue = NWPrimitiveValueConverter.ConvertPrimitiveValue(activityParameter.PrimitiveValue, this.workflowActionProcessor, true);
            }

            if (activityParameter.WorkflowContextData != null)
            {
                parameterValue.PrimitiveValue = new PrimitiveValue("String", "{0}");
                parameterValue.PrimitiveValue.FormatValues = new List<FormatValues>()
                {
                    new FormatValues
                    {
                        SelectedValue = new SelectedValue
                        {
                            WorkflowContext = NWWorkflowContextDataConverter.ConvertWorkflowContextData(activityParameter.WorkflowContextData)
                        }
                    }
                };
                if (activityParameter.Coercion != null)
                {
                    parameterValue.PrimitiveValue.FormatValues[0].SelectedValue.Coercion = activityParameter.Coercion.Value;
                }
            }

            if (activityParameter.Variable != null && !string.IsNullOrEmpty(activityParameter.Variable.Name))
            {
                parameterValue.PrimitiveValue = new PrimitiveValue("String", "{0}");
                parameterValue.PrimitiveValue.FormatValues = new List<FormatValues>()
                {
                    new FormatValues
                    {
                        SelectedValue = new SelectedValue
                        {
                            Variable = this.workflowActionProcessor.VariablesCacheManager.GetSimpleVariable(activityParameter.Variable),
                        }
                    }
                };
                if (activityParameter.Coercion != null)
                {
                    parameterValue.PrimitiveValue.FormatValues[0].SelectedValue.Coercion = activityParameter.Coercion.Value;
                }
            }
            return parameterValue;
        }
    }
}
