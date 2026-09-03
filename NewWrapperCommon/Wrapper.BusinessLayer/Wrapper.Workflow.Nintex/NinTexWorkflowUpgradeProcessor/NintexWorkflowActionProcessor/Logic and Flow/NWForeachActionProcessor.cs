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
using Native13NinTexWorkflowEntity;

namespace LS.SPWorkflowProcessor
{
    class NWForeachActionProcessor : NWContainerActionBase
    {
        public NWForeachActionProcessor(NintexWFActionProcessor workflowActionProcessor)
            : base(workflowActionProcessor)
        {
            CLASSNAME = "#ForEach";
        }

        protected override List<Property> CreateProperties()
        {
            var property = new Property();
            property.DesignerType = "ForEach";
            property.ID = "ForEach";
            property.Parameters = BuildParameters(sourceConfig.Parameters);
            return new List<Property> { property };
        }

        protected override Image CreateImage()
        {
            return new Image
            {
                Src = "ext/workflowDesigner/Img/icons/NW2013_Action_IconMap.png?noCache=1469003374434",
                ClassName = CLASSNAME,
                x49x49 = 343,
                y49x49 = 0,
                x30x30 = 343,
                y30x30 = 49,
                x16x16 = 376,
                y16x16 = 49
            };
        }

        private Parameters[] BuildParameters(ActivityParameter[] activityPrameters)
        {
            List<Parameters> parameters = new List<Parameters>();
            parameters.Add(CreateTargetDictionaryParameters(activityPrameters.First(item => IsFoundParameter(item, "Target"))));
            parameters.Add(CreateOutputParameters(activityPrameters.First(item => IsFoundParameter(item, "Value"))));
            parameters.Add(CreateKeyParameters());
            parameters.Add(CreateIndexParameters(activityPrameters.First(item => IsFoundParameter(item, "Index"))));
            parameters.Add(CreateStopProcessingParameters(activityPrameters.First(item => IsFoundParameter(item, "Break"))));
            parameters.Add(CreateIsDictionaryParameters());
            return parameters.ToArray();
        }


        private bool IsFoundParameter(ActivityParameter activityPrameter, string name)
        {
            return string.Equals(activityPrameter.Name, name, StringComparison.CurrentCulture);
        }

        private Parameters CreateTargetDictionaryParameters(ActivityParameter activityPrameter)
        {
            Parameters targetDictionaryParameter = new Parameters();
            targetDictionaryParameter.Name = "targetDictionary";
            targetDictionaryParameter.Description = "The dictionary or collection variable to step through.";
            targetDictionaryParameter.Required = true;
            targetDictionaryParameter.DataType = "DynamicValue";
            targetDictionaryParameter.DesignerType = "Variable";
            targetDictionaryParameter.Direction = "Input";

            targetDictionaryParameter.Value = ConvertParameterValue(activityPrameter);
            return targetDictionaryParameter;
        }

        private Parameters CreateOutputParameters(ActivityParameter activityPrameter)
        {
            Parameters outputParameter = new Parameters();
            outputParameter.Name = "output";
            outputParameter.Description = "The workflow variable for storing the value of the dictionary or collection variable item.";
            outputParameter.Required = true;
            outputParameter.DataType = "Any";
            outputParameter.DesignerType = "Variable";
            outputParameter.Direction = "Input";

            outputParameter.Value = ConvertParameterValue(activityPrameter);
            return outputParameter;
        }

        private Parameters CreateKeyParameters()
        {
            Parameters keyParameter = new Parameters();
            keyParameter.Name = "key";
            keyParameter.Description = "The workflow variable for storing the key of the dictionary variable item.";
            keyParameter.Required = false;
            keyParameter.DataType = "String";
            keyParameter.DesignerType = "Variable";
            keyParameter.Direction = "Input";

            keyParameter.Value = new ParametersValue { Variable = new Variable { Name = string.Empty, DataType = string.Empty } };
            return keyParameter;
        }

        private Parameters CreateIndexParameters(ActivityParameter activityPrameter)
        {
            return new Parameters
            {
                Name = "index",
                Description = "The workflow variable for storing the index of the collection variable item.",
                Required = false,
                DataType = "Int32",
                DesignerType = "Variable",
                Direction = "Input",
                Value = ConvertParameterValue(activityPrameter),
            };
        }

        protected override ParametersValue ConvertParameterValue(ActivityParameter activityParameter)
        {
            var parametersValue =  base.ConvertParameterValue(activityParameter);
            parametersValue.PrimitiveValue = null;
            return parametersValue;
        }

        private Parameters CreateStopProcessingParameters(ActivityParameter activityPrameter)
        {
            Parameters stopProcessingParameter = new Parameters();
            stopProcessingParameter.Name = "stopProcessing";
            stopProcessingParameter.Description = "A Boolean variable that when true will stop processing of the dictionary or collection variable.";
            stopProcessingParameter.Required = false;
            stopProcessingParameter.DataType = "Boolean";
            stopProcessingParameter.DesignerType = "Variable";
            stopProcessingParameter.Direction = "Input";
            stopProcessingParameter.Value = ConvertParameterValue(activityPrameter);
            return stopProcessingParameter;
        }

        private Parameters CreateIsDictionaryParameters()
        {
            Parameters isDictionaryParameter = new Parameters();
            isDictionaryParameter.Name = "isDictionary";
            isDictionaryParameter.Required = false;
            isDictionaryParameter.DataType = "Boolean";
            isDictionaryParameter.DesignerType = "Hidden";
            isDictionaryParameter.Direction = "Input";

            isDictionaryParameter.Value = new ParametersValue { PrimitiveValue = new PrimitiveValue { Type = "Boolean", Value = new Value("False") } };
            return isDictionaryParameter;
        }
    }
}
