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
    internal class CollectionOperationGeneratorJoin : CollectionOperationGenerator
    {
        private const string InputDelimiterDescription = "Text to insert between items being concatenated.";

        private const string OutputJoinDescription = "Text variable to store the string of concatenated items.";

        public CollectionOperationGeneratorJoin(CollectionAvailableOperation availableOperation) : base(availableOperation)
        {
            ClassName = "#CollectionOperationItemJoin";
            InputCollectionDescription = "The collection variable containing the items to be concatenated.";
            ActionName = "Join Items in Collection";
        }

        public override Image CreateImage()
        {
            return new Image
            {
                Src = "ext/workflowDesigner/Img/icons/NW2013_Action_IconMap.png?noCache=1469150084691",
                ClassName = "#CollectionOperationItemJoin",
                x49x49 = 49,
                y49x49 = 474,
                x30x30 = 49,
                y30x30 = 523,
                x16x16 = 82,
                y16x16 = 523
            };
        }

        public override List<Property> GenerateProperties(NintexWFActionProcessor nintexWorkflowActionProcessor, NWActionConfig config)
        {
            var activityParameters = config.Parameters.ToList();
            var targetActivityParameter = GetActivityParameterByName(activityParameters, "Target");
            var joinDelimiterActivityParameter = GetActivityParameterByName(activityParameters, "JoinDelimiter");
            var outputActivityParameter = GetActivityParameterByName(activityParameters, "Output");
            List<Property> properties = new List<Property>
            {
                CreateInputOperationProperty(),
                CreateInputCollectionProperty(nintexWorkflowActionProcessor, targetActivityParameter),
                CreateJoinDelimiterProperty(nintexWorkflowActionProcessor, joinDelimiterActivityParameter),
                CreateOutputJoinProperty(nintexWorkflowActionProcessor, outputActivityParameter)
            };
            return properties;
        }

        protected Property CreateJoinDelimiterProperty(NintexWFActionProcessor nintexWorkflowActionProcessor, ActivityParameter activityParameter)
        {
            Property property = new Property
            {
                ID = "InputDelimiter",
                DesignerType = "Text",
                DisplayName = "Delimiter"
            };
            Parameters parameter = new Parameters
            {
                Name = "InputDelimiter",
                Description = InputDelimiterDescription,
                Required = false,
                DataType = "String",
                DesignerType = "Text",
                Direction = "Input",
                DependentOn = null,
                Properties = null,
                OriginalSelectedValue = null,
                Value = NWValueConverter.ConvertValueToParametersValue(nintexWorkflowActionProcessor, activityParameter)
            };
            if (parameter.Value == null)
            {
                parameter.Value = new ParametersValue { PrimitiveValue = new PrimitiveValue("String","") };
            }
            property.Parameters = new[] { parameter };
            return property;
        }

        protected Property CreateOutputJoinProperty(NintexWFActionProcessor nintexWorkflowActionProcessor, ActivityParameter activityParameter)
        {
            Property property = new Property
            {
                ID = "OutputJoin",
                DesignerType = "Variable",
                DisplayName = "Output"
            };
            Parameters parameter = new Parameters
            {
                Name = "OutputJoin",
                Description = OutputJoinDescription,
                Required = true,
                DataType = "String",
                DesignerType = "Variable",
                Direction = "Output",
                DependentOn = null,
                Properties = null,
                OriginalSelectedValue = null,
                Value = NWValueConverter.ConvertValueToParametersValue(nintexWorkflowActionProcessor, activityParameter)
            };
            property.Parameters = new[] { parameter };
            return property;
        }
    }
}