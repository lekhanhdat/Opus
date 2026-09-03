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
using System.Reflection;
using Native13NinTexWorkflowEntity;

namespace LS.SPWorkflowProcessor
{
    internal enum CollectionAvailableOperation
    {
        Invalid = 0,
        Add = 1,
        Remove = 2,
        Count = 3,
        Get = 4,
        Exists = 5,
        Sort = 6,
        Pop = 7,
        Join = 8,
        Clear = 9,
        Deduplicate = 10,
        RemoveValue = 11,
    }

    internal abstract class CollectionOperationGenerator
    {
        protected CollectionAvailableOperation AvailableOperation { get; set; }

        protected string ClassName { get; set; }

        protected string InputCollectionDescription { get; set; }

        protected string InputIndexDescription { get; set; }

        protected string OutputCollectionDescription { get; set; }

        protected string ActionName { get; set; }

        protected CollectionOperationGenerator(CollectionAvailableOperation availableOperation)
        {
            AvailableOperation = availableOperation;
            ClassName = "";
            InputCollectionDescription = "";
            InputIndexDescription = "";
            OutputCollectionDescription = "";
            ActionName = "";
        }

        public abstract Image CreateImage();

        internal static CollectionOperationGenerator CreateInstance(CollectionAvailableOperation availableOperation)
        {
            string typeName = string.Format("{0}.{1}{2}", typeof(CollectionOperationGenerator).Namespace, "CollectionOperationGenerator", availableOperation);
            var generatorInstance = Activator.CreateInstance(Assembly.GetCallingAssembly().GetType(typeName), availableOperation) as CollectionOperationGenerator;
            return generatorInstance;
        }

        public virtual List<Property> GenerateProperties(NintexWFActionProcessor nintexWorkflowActionProcessor, NWActionConfig config)
        {
            return null;
        }

        public virtual void PostUpdateWorkflowAction(WorkflowAction action, NWActionConfig config)
        {
            action.Configuration.HelpKey = ClassName;
            action.Configuration.Image.ClassName = ClassName;
            action.Configuration.Name = ActionName;
        }

        protected Parameters CreateInputOperationParameter()
        {
            Parameters parameter = new Parameters
            {
                Name = "InputOperation",
                Value = new ParametersValue
                {
                    PrimitiveValue = new PrimitiveValue("Int32", Convert.ToString((int)AvailableOperation))
                },
                Description = "",
                Required = true,
                DataType = "Int32",
                DesignerType = "Hidden",
                Direction = "Input",
                DependentOn = null,
                Properties = null,
                OriginalSelectedValue = null,
                Type = "Any",
                DefaultType = "Any"
            };
            return parameter;
        }

        protected Property CreateInputOperationProperty()
        {
            return new Property
            {
                ID = "InputOperation",
                DesignerType = "Hidden",
                DisplayName = "",
                Parameters = new[] { CreateInputOperationParameter() }
            };
        }

        protected Parameters CreateInputCollectionParameter(NintexWFActionProcessor nintexWorkflowActionProcessor, ActivityParameter activityParameter)
        {
            Parameters parameter = new Parameters
            {
                Name = "InputCollection",
                Description = InputCollectionDescription,
                Required = true,
                DataType = "DynamicValue",
                Type = "Array",
                DesignerType = "Variable",
                Direction = "Input",
                DependentOn = null,
                Properties = null,
                OriginalSelectedValue = null,
                DefaultType = "Any",
                Value = NWValueConverter.ConvertValueToParametersValue(nintexWorkflowActionProcessor, activityParameter)
            };
            return parameter;
        }

        protected Property CreateInputCollectionProperty(NintexWFActionProcessor nintexWorkflowActionProcessor, ActivityParameter parameter)
        {
            return new Property
            {
                ID = "InputCollection",
                DesignerType = "Variable",
                DisplayName = "Target collection",
                Parameters = new[] { CreateInputCollectionParameter(nintexWorkflowActionProcessor, parameter) }
            };
        }

        protected virtual Parameters CreateOutputCollectionParameter(NintexWFActionProcessor nintexWorkflowActionProcessor, ActivityParameter activityParameter)
        {
            return new Parameters
            {
                Name = "OutputCollection",
                Description = OutputCollectionDescription,
                Required = true,
                DataType = "DynamicValue",
                Type = "Array",
                DefaultType = "Any",
                DesignerType = "Variable",
                Direction = "Output",
                DependentOn = null,
                Properties = null,
                OriginalSelectedValue = null,
                Value = NWValueConverter.ConvertValueToParametersValue(nintexWorkflowActionProcessor, activityParameter)
            };
        }

        protected virtual Property CreateOutputCollectionProperty(NintexWFActionProcessor nintexWorkflowActionProcessor, ActivityParameter parameter)
        {
            return new Property
            {
                ID = "OutputCollection",
                DesignerType = "Variable",
                DisplayName = "Output",
                Parameters = new[] { CreateOutputCollectionParameter(nintexWorkflowActionProcessor, parameter) }
            };
        }

        protected Parameters CreateIndexParameter(NintexWFActionProcessor nintexWorkflowActionProcessor, ActivityParameter activityParameter)
        {
            Parameters parameter = new Parameters
            {
                Name = "InputIndex",
                Description = InputIndexDescription,
                Required = true,
                DataType = "Int32",
                Type = "Any",
                DefaultType = "Any",
                DesignerType = "Integer",
                Direction = "Input",
                DependentOn = null,
                Properties = null,
                OriginalSelectedValue = null,
                Value = NWValueConverter.ConvertValueToParametersValue(nintexWorkflowActionProcessor, activityParameter)
            };
            if (parameter.Value != null)
            {
                parameter.Value.Coercion = NWCoercionStringProcessor.GetCoercionString(parameter);
            }
            return parameter;
        }

        protected Property CreateInputIndexProperty(NintexWFActionProcessor nintexWorkflowActionProcessor, ActivityParameter parameter)
        {
            return new Property
            {
                ID = "InputIndex",
                DesignerType = "Integer",
                DisplayName = "Index",
                Parameters = new[] { CreateIndexParameter(nintexWorkflowActionProcessor, parameter) }
            };
        }


        protected Parameters CreateInputValueParameter(NintexWFActionProcessor nintexWorkflowActionProcessor, ActivityParameter activityParameter)
        {
            CheckUnsupportedActionType(activityParameter);
            var parametersValue = GetInputValueParametersValue(nintexWorkflowActionProcessor, activityParameter);

            Parameters parameter = new Parameters
            {
                Name = "InputValue",
                Required = true,
                DataType = "Any",
                Type = "String",
                DesignerType = "Multiline",
                Direction = "Input",
                Description = "",
                DependentOn = null,
                Properties = null,
                OriginalSelectedValue = null,
                Value = parametersValue
            };
            return parameter;
        }

        private ParametersValue GetInputValueParametersValue(NintexWFActionProcessor nintexWorkflowActionProcessor, ActivityParameter activityParameter)
        {
            ParametersValue parametersValue = null;
            if (activityParameter.PrimitiveValue != null)
            {
                parametersValue = new ParametersValue
                {
                    PrimitiveValue = NWPrimitiveValueConverter.ConvertPrimitiveValue(activityParameter.PrimitiveValue, nintexWorkflowActionProcessor, true)
                };
            }
            else
            {
                parametersValue = new ParametersValue
                {
                    PrimitiveValue = new PrimitiveValue
                    {
                        Type = "String",
                        Value = new Value("{0}"),
                        FormatValues = CreateFormatValues(nintexWorkflowActionProcessor, activityParameter),
                    },
                };
            }
            return parametersValue;
        }

        private List<FormatValues> CreateFormatValues(NintexWFActionProcessor nintexWorkflowActionProcessor, ActivityParameter activityParameter)
        {
            var selectedValue = new SelectedValue
            {
                Coercion = "AsDNString",
            };

            if (activityParameter.ListLookup != null)
            {
                selectedValue.ListLookup = NWListLookupConverter.ConvertListLookup(activityParameter.ListLookup, nintexWorkflowActionProcessor);
            }
            else if (activityParameter.WorkflowContextData != null)
            {
                selectedValue.WorkflowContext = NWWorkflowContextDataConverter.ConvertWorkflowContextData(activityParameter.WorkflowContextData);
            }
            else if (activityParameter.Variable != null)
            {
                selectedValue.Variable = nintexWorkflowActionProcessor.VariablesCacheManager.GetVariable(activityParameter.Variable.Name, true);
            }
            return new List<FormatValues> { new FormatValues { SelectedValue = selectedValue } };
        }

        protected Parameters CreateOutPutTypeParameter()
        {
            Parameters parameter = new Parameters
            {
                Name = "OutputType",
                Value = new ParametersValue
                {
                    PrimitiveValue = new PrimitiveValue("String", "String")
                },
                Required = true,
                DataType = "String",
                DesignerType = "Hidden",
                Description = "",
                Direction = "Input",
                Type = "Any",
                DefaultType = "Any",
                DependentOn = null,
                Properties = null,
                OriginalSelectedValue = null
            };
            return parameter;
        }


        protected void CheckUnsupportedActionType(ActivityParameter para)
        {
            if (para.ProfileLookup != null)
            {
                throw new UnSupportedActionTypeException("Unsupported value type ProfileLookup");
            }
            if (para.WorkflowConstant != null)
            {
                throw new UnSupportedActionTypeException("Unsupported value type WorkflowConstant");
            }
        }

        protected ActivityParameter GetActivityParameterByName(List<ActivityParameter> array, string name)
        {
            var activityParameter = array.Find(para => string.Equals(para.Name, name, StringComparison.OrdinalIgnoreCase));
            if (activityParameter == null)
            {
                throw new UnSupportedActionTypeException("Invalid action.Cannot find the parameter {0}.", name);
            }
            return activityParameter;
        }

    }
}