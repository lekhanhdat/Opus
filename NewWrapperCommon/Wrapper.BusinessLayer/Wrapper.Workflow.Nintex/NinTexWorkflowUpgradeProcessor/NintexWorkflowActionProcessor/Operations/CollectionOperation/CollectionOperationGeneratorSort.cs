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
using Native13NinTexWorkflowEntity;

namespace LS.SPWorkflowProcessor
{
    internal class CollectionOperationGeneratorSort : CollectionOperationGenerator
    {
        private const string InputDirectionDescription = "Direction for the sort operation.";

        public CollectionOperationGeneratorSort(CollectionAvailableOperation availableOperation) : base(availableOperation)
        {
            ClassName = "#CollectionOperationItemSort";
            InputCollectionDescription = "The collection variable containing the items to be sorted.";
            OutputCollectionDescription = "Collection variable to store the sorted collection.";
            ActionName = "Sort Items in Collection";
        }

        public override Image CreateImage()
        {
            return new Image
            {
                Src = "ext/workflowDesigner/Img/icons/NW2013_Action_IconMap.png?noCache=1469150084669",
                ClassName = "#CollectionOperationItemSort",
                x49x49 = 294,
                y49x49 = 474,
                x30x30 = 294,
                y30x30 = 523,
                x16x16 = 327,
                y16x16 = 523
            };
        }

        public override List<Property> GenerateProperties(NintexWFActionProcessor nintexWorkflowActionProcessor, NWActionConfig config)
        {
            var property = new Property
            {
                ID = "CollectionOperationItemSort",
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
                CreateInputDirectionParameter(nintexWorkflowActionProcessor, GetActivityParameterByName(sourceParameters, "SortDirection")),
                CreateOutputCollectionParameter(nintexWorkflowActionProcessor, GetActivityParameterByName(sourceParameters, "Output"))
            };
            property.Parameters = parameters.ToArray();
            return new List<Property>() { property };
        }

        protected Parameters CreateInputDirectionParameter(NintexWFActionProcessor nintexWorkflowActionProcessor, ActivityParameter activityParameter)
        {
            string value = "1";
            if (string.Equals(activityParameter.PrimitiveValue.Value,"Descending",StringComparison.OrdinalIgnoreCase))
            {
                value ="2";
            }
            Parameters parameter = new Parameters
            {
                Name = "InputDirection",
                Description = InputDirectionDescription,
                Required = false,
                DataType = "Int32",
                DesignerType = "Integer",
                Direction = "Input",
                DependentOn = null,
                Properties = null,
                OriginalSelectedValue = null,
                Value = new ParametersValue
                {
                    PrimitiveValue = new PrimitiveValue("Int32", value)
                }
            };
            return parameter;
        }

    }
}