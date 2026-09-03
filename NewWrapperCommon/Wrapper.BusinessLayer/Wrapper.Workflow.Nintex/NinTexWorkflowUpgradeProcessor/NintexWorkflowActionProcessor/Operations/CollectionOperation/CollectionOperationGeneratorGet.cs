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
    internal class CollectionOperationGeneratorGet : CollectionOperationGenerator
    {
        private const string OutputItemDescription = "Variable to store the retrieved item.";

        public override Image CreateImage()
        {
            return new Image
            {
                Src = "ext/workflowDesigner/Img/icons/NW2013_Action_IconMap.png?noCache=1469150084695",
                ClassName = "#CollectionOperationItemGet",
                x49x49 = 0,
                y49x49 = 474,
                x30x30 = 0,
                y30x30 = 523,
                x16x16 = 33,
                y16x16 = 523
            };
        }


        public CollectionOperationGeneratorGet(CollectionAvailableOperation availableOperation) : base(availableOperation)
        {
            ClassName = "#CollectionOperationItemGet";
            InputCollectionDescription = "The collection variable to retrieve the item from.";
            InputIndexDescription = "Index of the item to retrieve from the collection.";
            ActionName = "Get Item from Collection";
        }

        public override List<Property> GenerateProperties(NintexWFActionProcessor nintexWorkflowActionProcessor, NWActionConfig config)
        {
            var activityParameters = config.Parameters.ToList();
            var targetActivityParameter = GetActivityParameterByName(activityParameters, "Target");
            var indexActivityParameter = GetActivityParameterByName(activityParameters, "Index");
            var outputCountActivityParameter = GetActivityParameterByName(activityParameters, "Output");
            List<Property> properties = new List<Property>
            {
                CreateInputOperationProperty(),
                CreateInputCollectionProperty(nintexWorkflowActionProcessor, targetActivityParameter),
                CreateInputIndexProperty(nintexWorkflowActionProcessor, indexActivityParameter),
                CreateOutputItemProperty(nintexWorkflowActionProcessor,outputCountActivityParameter)
            };
            return properties;
        }

        protected Property CreateOutputItemProperty(NintexWFActionProcessor nintexWorkflowActionProcessor, ActivityParameter activityParameter)
        {
            Property property = new Property
            {
                ID = "OutputItem",
                DesignerType = "Variable",
                DisplayName = "Output"
            };
            Parameters parameter = new Parameters
            {
                Name = "OutputItem",
                Description = OutputItemDescription,
                Required = true,
                DataType = "Any",
                DesignerType = "Variable",
                Direction = "Output",
                DependentOn = null,
                Properties = null,
                OriginalSelectedValue = null
            };
            var value = NWValueConverter.ConvertValue(nintexWorkflowActionProcessor, activityParameter);
            var parameterValue = new ParametersValue
            {
                PrimitiveValue = value.PrimitiveValue,
                Variable = value.Variable,
            };
            parameter.Value = parameterValue;
            property.Parameters = new[] { parameter };
            return property;
        }

    }
}