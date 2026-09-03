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
    internal class CollectionOperationGeneratorPop : CollectionOperationGenerator
    {
        private const string OutputItemDescription = "Variable to store the value of the last item to be removed.";

        public CollectionOperationGeneratorPop(CollectionAvailableOperation availableOperation) : base(availableOperation)
        {
            ClassName = "#CollectionOperationItemPop";
            InputCollectionDescription = "The collection variable to remove an item from.";
            OutputCollectionDescription = "Collection variable to store the collection with the item removed.";
            ActionName = "Remove Last Item from Collection";
        }

        public override Image CreateImage()
        {
            return new Image
            {
                Src = "ext/workflowDesigner/Img/icons/NW2013_Action_IconMap.png?noCache=1469150084688",
                ClassName = "#CollectionOperationItemPop",
                x49x49 = 98,
                y49x49 = 474,
                x30x30 = 98,
                y30x30 = 523,
                x16x16 = 131,
                y16x16 = 523
            };
        }

        public override List<Property> GenerateProperties(NintexWFActionProcessor nintexWorkflowActionProcessor, NWActionConfig config)
        {
            var activityParameters = config.Parameters.ToList();
            var targetActivityParameter = GetActivityParameterByName(activityParameters, "Target");
            var outputActivityParameter = GetActivityParameterByName(activityParameters, "Output");
            List<Property> properties = new List<Property>
            {
                CreateInputOperationProperty(),
                CreateInputCollectionProperty(nintexWorkflowActionProcessor, targetActivityParameter),
                CreateOutputItemProperty(nintexWorkflowActionProcessor, outputActivityParameter),
                CreateOutputCollectionProperty(nintexWorkflowActionProcessor, targetActivityParameter)
            };
            return properties;
        }

        protected Property CreateOutputItemProperty(NintexWFActionProcessor nintexWorkflowActionProcessor, ActivityParameter activityParameter)
        {
            Property property = new Property
            {
                ID = "OutputItem",
                DesignerType = "Variable",
                DisplayName = "Value of last item"
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
                OriginalSelectedValue = null,
                Value=NWValueConverter.ConvertValueToParametersValue(nintexWorkflowActionProcessor,activityParameter)
            };
            property.Parameters = new[] { parameter };
            return property;
        }
    }
}