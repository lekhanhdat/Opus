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
    internal class CollectionOperationGeneratorCount : CollectionOperationGenerator
    {
        private const string OutputCountDescription = "Integer variable to store the number of items in the collection.";
        public CollectionOperationGeneratorCount(CollectionAvailableOperation availableOperation) : base(availableOperation)
        {
            ClassName = "#CollectionOperationItemCount";
            InputCollectionDescription = "The collection variable containing the items to be counted.";
            ActionName = "Count Items in Collection";
        }


        public override Image CreateImage()
        {
            return new Image
            {
                Src = "ext/workflowDesigner/Img/icons/NW2013_Action_IconMap.png?noCache=1469150084703",
                ClassName = "#CollectionOperationItemCount",
                x49x49 = 392,
                y49x49 = 395,
                x30x30 = 392,
                y30x30 = 444,
                x16x16 = 425,
                y16x16 = 444
            };
        }

        public override List<Property> GenerateProperties(NintexWFActionProcessor nintexWorkflowActionProcessor, NWActionConfig config)
        {
            var activityParameters = config.Parameters.ToList();
            var targetActivityParameter = GetActivityParameterByName(activityParameters, "Target");
            var outputCountActivityParameter = GetActivityParameterByName(activityParameters, "Output");
            List<Property> properties = new List<Property>
            {
                CreateInputOperationProperty(),
                CreateInputCollectionProperty(nintexWorkflowActionProcessor, targetActivityParameter),
                CreateOutputCountProperty(nintexWorkflowActionProcessor,outputCountActivityParameter)
            };
            return properties;
        }

        protected Property CreateOutputCountProperty(NintexWFActionProcessor nintexWorkflowActionProcessor, ActivityParameter activityParameter)
        {
            Property property = new Property
            {
                ID = "OutputCount",
                DesignerType = "Variable",
                DisplayName = "Total items"
            };
            Parameters parameter = new Parameters
            {
                Name = "OutputCount",
                Description = OutputCountDescription,
                Required = true,
                DataType = "Int32",
                DesignerType = "Variable",
                Direction = "Output",
                DependentOn = null,
                Properties = null,
                OriginalSelectedValue = null
            };
            var value = NWValueConverter.ConvertValue(nintexWorkflowActionProcessor, activityParameter);
            if ((value.Variable != null) && !value.Variable.DataType.Equals("Int32", System.StringComparison.OrdinalIgnoreCase))
            {
                throw new UnSupportedSettingException("The setting \"Total items\" of destination action: \"Count Item in a Dictionary\" only supports Integer type, but source is {0}", value.Variable.DataType);
            }

            var parameterValue = new ParametersValue
            {
                PrimitiveValue = value.PrimitiveValue,
                Variable = value.Variable,
            };
            parameter.Value = parameterValue;
            property.Parameters = new[] {parameter};
            return property;
        }


    }
}