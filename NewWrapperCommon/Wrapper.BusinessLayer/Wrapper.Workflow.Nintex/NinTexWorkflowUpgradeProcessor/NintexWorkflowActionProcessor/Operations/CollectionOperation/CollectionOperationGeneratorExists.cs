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
using System.Collections.Generic;
using System.Linq;
using Native13NinTexWorkflowEntity;

namespace LS.SPWorkflowProcessor
{
    internal class CollectionOperationGeneratorExists : CollectionOperationGenerator
    {
        private const string OutputExistsDescription = "Boolean variable to store evaluation results.";

        public override Image CreateImage()
        {
            return new Image
            {
                Src = "ext/workflowDesigner/Img/icons/NW2013_Action_IconMap.png?noCache=1469150084699",
                ClassName = "#CollectionOperationItemExists",
                x49x49 = 441,
                y49x49 = 395,
                x30x30 = 441,
                y30x30 = 444,
                x16x16 = 474,
                y16x16 = 444
            };
        }

        public CollectionOperationGeneratorExists(CollectionAvailableOperation availableOperation) : base(availableOperation)
        {
            ClassName = "#CollectionOperationItemExists";
            InputCollectionDescription = "The collection variable to evaluate for existence of the item.";
            ActionName = "Check if Item Exists in Collection";
        }

        public override List<Property> GenerateProperties(NintexWFActionProcessor nintexWorkflowActionProcessor, NWActionConfig config)
        {
            var property = new Property
            {
                ID = "CollectionOperationItemExists",
                DesignerType = "CollectionOperation",
                DisplayName = "",
            };
            List<ActivityParameter> sourceParameters = config.Parameters.ToList();
            var targetParameter = GetActivityParameterByName(sourceParameters, "Target");
            //parameters need to be added in specific order
            List<Parameters> parameters = new List<Parameters>
            {
                CreateInputOperationParameter(),
                CreateInputCollectionParameter(nintexWorkflowActionProcessor, targetParameter),
                CreateInputValueParameter(nintexWorkflowActionProcessor, GetActivityParameterByName(sourceParameters, "LookupFieldValue")),
                CreateOutPutTypeParameter(),
                CreateOutputExistsParameter(nintexWorkflowActionProcessor, GetActivityParameterByName(sourceParameters, "Output"))
            };
            property.Parameters = parameters.ToArray();
            return new List<Property>() { property };
        }

        protected Parameters CreateOutputExistsParameter(NintexWFActionProcessor nintexWorkflowActionProcessor, ActivityParameter activityParameter)
        {
            Parameters parameter = new Parameters
            {
                Name = "OutputExists",
                Description = OutputExistsDescription,
                Required = true,
                DataType = "Boolean",
                DesignerType = "Variable",
                Direction = "Output",
                DependentOn = null,
                Properties = null,
                OriginalSelectedValue = null,
                Value = NWValueConverter.ConvertValueToParametersValue(nintexWorkflowActionProcessor, activityParameter)
            };
            if (parameter.Value.Variable != null && !string.Equals(parameter.Value.Variable.DataType, "Boolean", System.StringComparison.OrdinalIgnoreCase))
            {
                throw new UnSupportedSettingException("Exist collection only can support boolean type variable.");
            }
            return parameter;
        }

    }
}